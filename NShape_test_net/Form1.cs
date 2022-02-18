using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.Layouters;

using JsonEditorForm;

using JsonPathParserLib;

using Newtonsoft.Json;

namespace StatesDiagram
{
    public partial class Form1 : Form
    {
        public struct WinPosition
        {
            public int WinX;
            public int WinY;
            public int WinW;
            public int WinH;

            [JsonIgnore] public bool Initialized => !(WinX <= 0 && WinY <= 0 && WinW <= 0 && WinH <= 0);
        }

        // diagram settings
        private const string _inputFileExtension = "*.json";
        private const string _diagramFileExtension = "nspj";
        private const string _tagsFileExtension = "_tags.json";
        private string _currentDiagramName = "Sample Diagram";
        private const string _shapeType = "RoundedBox"; //Square, Box, Diamond, RoundedBox, Circle, Ellipse
        private const string _arrowLineType = "Polyline";
        private const string _startingShapeColor = "Green";
        private const string _endingShapeColor = "Yellow";
        private const string _orphanShapeColor = "Red";
        private const string _defaultShapeColor = "Blue";
        private const string _returnOnlyShapeColor = "White";

        // custom arrow styles introduced in the fixed NShape.Core
        private const string _defaultNormalArrowType = "StraightArrow";
        private Guid _defaultNormalArrowId = Guid.Parse("74245395-2ae1-47da-bafd-a64f22e11b67");
        private const string _defaultReverseArrowType = "BackArrow";
        private Guid _defaultReverseArrowId = Guid.Parse("edef5bdf-56c8-457c-ae58-60bf74fce981");

        // custom arrow types only works normally with fixed NShape.Core (fixes attached to the project)
        // otherise please use standard "ArrowClosed"/"ArrowOpen" to allow diagram to be saved normally
        private string _normalArrowType = "StraightArrow";
        private string _reverseArrowType = "BackArrow";

        // list of shapes on the canvas to work with
        private Dictionary<string, RectangleBase> _shapeDict = new Dictionary<string, RectangleBase>();

        // JSON parser settings
        private JsonPathParser _parser = new JsonPathParser();
        private const string RootName = "";
        private const char _pathDivider = '.';

        // graph structure storege for JSON export
        private DiagramExport _diagramJson = new DiagramExport();

        // Json viewer window
        private JsonViewer _sideViewer;
        private const string PreViewCaption = "JSON File ";
        private bool _useVsCode = false;
        private bool _singleLineBrackets = false;
        private WinPosition _editorPosition;
        private bool _showPreview = true;

        // initial diagram and shapes settings
        private const int _initX = 50;
        private const int _initY = 50;
        private const int _initW = 200;
        private const int _initH = 100;
        private int diagramWidth = 1500;
        private int diagramHeight = 1500;

        #region GUI
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkBox_useV1.Checked = Properties.Settings.Default.useV1;

            diagramWidth = Properties.Settings.Default.SizeX;
            textBox_sizeX.Text = diagramWidth.ToString();

            diagramHeight = Properties.Settings.Default.SizeY;
            textBox_sizeY.Text = diagramHeight.ToString();

            CheckBox_useV1_CheckedChanged(this, EventArgs.Empty);

            checkBox_useVsCode.Checked = _useVsCode = Properties.Settings.Default.UseVsCode;
            CheckBox_useVsCode_CheckedChanged(this, EventArgs.Empty);

            checkBox_ShowPreview.Checked = _showPreview = Properties.Settings.Default.ShowPreview;
            CheckBox_ShowPreview_CheckedChanged(this, EventArgs.Empty);

            _parser = new JsonPathParser
            {
                TrimComplexValues = false,
                SaveComplexValues = true,
                RootName = RootName,
                JsonPathDivider = _pathDivider,
                SearchStartOnly = true
            };
        }

        private void Button_loadStates_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastFolder))
                folderBrowserDialog1.SelectedPath = Properties.Settings.Default.LastFolder;

            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK
                || string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
                return;

            Properties.Settings.Default.LastFolder = folderBrowserDialog1.SelectedPath;
            var filesList = Directory.GetFiles(folderBrowserDialog1.SelectedPath,
                _inputFileExtension,
                SearchOption.AllDirectories);

            var states = new List<ParsedState>();
            var links = new List<StateLink>();
            _shapeDict.Clear();
            _diagramJson = new DiagramExport();
            foreach (var file in filesList)
            {
                var newPathList = ParseJson(file, _parser);

                if (newPathList == null)
                    continue;

                ParsedState newState;
                List<StateLink> newLinks;

                if (Properties.Settings.Default.useV1)
                    GetStateV1(newPathList, file, out newState, out newLinks);
                else
                    GetStateV2(newPathList, file, out newState, out newLinks);

                if (!string.IsNullOrEmpty(newState.Name))
                    states.Add(newState);

                if (newLinks != null)
                    links.AddRange(newLinks);
            }

            var flowName = new DirectoryInfo(folderBrowserDialog1.SelectedPath).Name;
            project1.Name = flowName;
            _currentDiagramName = flowName;

            InitProject();

            Diagram diagram = new Diagram(_currentDiagramName)
            {
                Width = diagramWidth,
                Height = diagramHeight,
                HighQualityRendering = true,
                Title = _currentDiagramName
            };

            CreateShapes(diagram, states);
            CreateLinks(diagram, states, links);
            ColorizeStates(diagram, states, links);

            cachedRepository1.InsertAll(diagram);
            display1.Diagram = diagram;

            RefreshLayout();
        }

        private void Button_save_Click(object sender, EventArgs e)
        {
            xmlStore1.DirectoryName = @".";
            xmlStore1.FileExtension = _diagramFileExtension;

            try
            {
                project1.Repository.Update(project1.Design);
                project1.Repository.Update();
                project1.Repository.SaveChanges();

                // save tags to additional file
                var tags = new Dictionary<string, ShapeTag>();
                foreach (var shape in display1.Diagram.Shapes)
                {
                    tags.Add(((IEntity)shape).Id.ToString(), (ShapeTag)shape.Tag);
                }

                File.WriteAllText(_currentDiagramName + _tagsFileExtension, JsonConvert.SerializeObject(tags, Formatting.Indented));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show($"Diagram saved to \"{project1.Name}{xmlStore1.FileExtension}\" .");
        }

        private void Button_load_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Title = "Open diagram file";
            openFileDialog1.DefaultExt = _diagramFileExtension;
            openFileDialog1.Filter = $"Diagram files|*.{_diagramFileExtension}|All files|*.*";
            openFileDialog1.ShowDialog();
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            var file = new FileInfo(openFileDialog1.FileName);

            project1.Close();
            project1.RemoveAllLibraries();
            project1.LibrarySearchPaths.Clear();

            // Set path to the sample diagram and the diagram file extension
            xmlStore1.DirectoryName = file.Directory.FullName;
            xmlStore1.FileExtension = file.Extension;
            // Set the name of the project that should be loaded from the store
            project1.Name = file.Name.Replace(file.Extension, "");
            project1.LibrarySearchPaths.Add(@".");
            project1.AutoLoadLibraries = true;
            // Open the NShape project
            try
            {
                project1.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Load the diagram and display it
            var diagram = project1.Repository.GetDiagrams().FirstOrDefault();
            display1.LoadDiagram(diagram.Name);

            // restore tags from file
            try
            {
                if (File.Exists(project1.Name + _tagsFileExtension))
                {
                    var tagsText = File.ReadAllText(project1.Name + _tagsFileExtension);
                    var tags = JsonConvert.DeserializeObject<Dictionary<string, ShapeTag>>(tagsText);
                    if (tags != null)
                    {
                        foreach (var tag in tags)
                        {
                            display1.Diagram.Shapes.FirstOrDefault(n => ((IEntity)n).Id.ToString() == tag.Key).Tag = tag.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            display1.ZoomWithMouseWheel = true;
            display1.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            display1.ActiveTool = new SelectionTool();
        }

        private void Button_export_Click(object sender, EventArgs e)
        {
            try
            {
                display1.Diagram.CreateImage(ImageFileFormat.Jpeg).Save(_currentDiagramName + ".jpg");
                File.WriteAllText(_currentDiagramName + ".json", JsonConvert.SerializeObject(_diagramJson, Formatting.Indented));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show($"Diagram exported to \"{_currentDiagramName}.jpg/json\" .");
        }

        private void Button_RefreshLayout_Click(object sender, EventArgs e)
        {
            RefreshLayout();
        }

        private void Display1_ShapeClick(object sender, Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs e)
        {
            if (e?.Shape?.Tag != null && e?.Shape?.Tag is ShapeTag shapeTag)
            {
                textBox_tag.Text = shapeTag.ToString();
                if (_showPreview) ShowPreviewEditor(shapeTag.FileName, shapeTag.JsonPath, false);
            }
        }

        private void TextBox_sizeX_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox_sizeX.Text) && int.TryParse(textBox_sizeX.Text, out diagramWidth))
            {
                Properties.Settings.Default.SizeX = diagramWidth;

                if (display1.Diagram != null)
                    display1.Diagram.Size = new System.Drawing.Size(diagramWidth, display1.Diagram.Size.Height);
            }

            textBox_sizeX.Text = diagramWidth.ToString();
        }

        private void TextBox_sizeY_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox_sizeX.Text) && int.TryParse(textBox_sizeY.Text, out diagramHeight))
            {
                Properties.Settings.Default.SizeY = diagramHeight;

                if (display1.Diagram != null)
                    display1.Diagram.Size = new System.Drawing.Size(display1.Diagram.Size.Width, diagramHeight);
            }

            textBox_sizeY.Text = diagramHeight.ToString();
        }

        private void CheckBox_useV1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.useV1 = checkBox_useV1.Checked;
            checkBox_useV1.Text = checkBox_useV1.Checked ? "SHELF JSON" : "NLMK JSON";
        }

        private void OnClosingEditor(object sender, CancelEventArgs e)
        {
            if (sender is Form senderForm)
            {
                _editorPosition.WinX = senderForm.Location.X;
                _editorPosition.WinY = senderForm.Location.Y;
                _editorPosition.WinW = senderForm.Width;
                _editorPosition.WinH = senderForm.Height;
            }
        }

        private void OnResizeEditor(object sender, EventArgs e)
        {
            if (sender is Form senderForm)
            {
                _editorPosition.WinX = senderForm.Location.X;
                _editorPosition.WinY = senderForm.Location.Y;
                _editorPosition.WinW = senderForm.Width;
                _editorPosition.WinH = senderForm.Height;
            }
        }

        private void CheckBox_ShowPreview_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowPreview = _showPreview = checkBox_ShowPreview.Checked;

            if (!_showPreview)
                checkBox_useVsCode.Checked = false;

            checkBox_useVsCode.Enabled = _showPreview;
        }

        private void CheckBox_useVsCode_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseVsCode = _useVsCode = checkBox_useVsCode.Checked;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
        #endregion

        #region Utilities
        private void InitProject()
        {
            xmlStore1.DirectoryName = @".";
            xmlStore1.FileExtension = _diagramFileExtension;
            project1.Name = _currentDiagramName;
            project1.Close();
            project1.RemoveAllLibraries();

            project1.Create();
            project1.AddLibrary(typeof(Box).Assembly, false);

            if (!project1.Design.Styles.Any(n => n.Name == _defaultNormalArrowType))
                AddCustomNormalArrowCapStyles();
            if (!project1.Design.Styles.Any(n => n.Name == _defaultReverseArrowType))
                AddCustomBackArrowCapStyles();

            display1.ZoomWithMouseWheel = true;
            display1.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            display1.ActiveTool = new SelectionTool();

            textBox_sizeX.Text = diagramWidth.ToString();
            textBox_sizeY.Text = diagramHeight.ToString();
        }

        private void AddCustomNormalArrowCapStyles()
        {
            _normalArrowType = _defaultNormalArrowType;
            var straightArrow = new CapStyle(_normalArrowType)
            {
                CapShape = CapShape.ClosedArrow,
                CapSize = 30,
                ColorStyle = project1.Design.ColorStyles.LightGreen,
            };
            straightArrow.AssignId(_defaultNormalArrowId);
            //project1.Design.CapStyles.Add(straightArrow, straightArrow);
            project1.Design.AddStyle(straightArrow);
            //project1.Design.AssignStyle(straightArrow);

            // need to save CapStyle changes in the project to allow export *.nspj file
            project1.Repository.Update(project1.Design);
            //but it doesn't work for some reason
        }

        private void AddCustomBackArrowCapStyles()
        {
            _reverseArrowType = _defaultReverseArrowType;
            var backArrow = new CapStyle(_reverseArrowType)
            {
                CapShape = CapShape.ClosedArrow,
                CapSize = 30,
                ColorStyle = project1.Design.ColorStyles.Red,
            };
            backArrow.AssignId(_defaultReverseArrowId);
            //project1.Design.CapStyles.Add(backArrow, backArrow);
            project1.Design.AddStyle(backArrow);
            //project1.Design.AssignStyle(backArrow);

            // need to save CapStyle changes in the project to allow export *.nspj file
            project1.Repository.Update(project1.Design);
            //but it doesn't work for some reason
        }

        private void GetStateV1(IEnumerable<ParsedProperty> pathList, string filePath, out ParsedState state, out List<StateLink> links)
        {
            state = new ParsedState();
            links = new List<StateLink>();

            // find state defined
            var stateObject = pathList.FirstOrDefault(n => n.Path.Equals(RootName + _pathDivider + "state", StringComparison.OrdinalIgnoreCase));

            if (stateObject == null)
                return;

            // find all transitions defined
            var transitions = pathList.Where(n => n.Name.Equals("state", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Contains(_pathDivider + "transition")).ToList();

            // generate links from current state
            foreach (var transition in transitions)
            {
                // get the name of the method which is running the transition for description
                var tName = pathList.FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && n.ParentPath == TrimPathEnd(transition.ParentPath, 1, _pathDivider));
                var newLink = new StateLink()
                {
                    FromState = stateObject.Value,
                    ToState = transition?.Value,
                    Tag = new ShapeTag
                    {
                        Description = tName?.Value ?? "",
                        FileName = filePath,
                        JsonPath = transition?.Path,
                    },
                };

                links.Add(newLink);

            }

            // find all calculated transitions defined and create links for them
            var ctransitions = pathList.Where(n => n.Name.Equals("calculatedState", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Contains(_pathDivider + "transition"));
            var ct = ctransitions.FirstOrDefault(n => n.Value == "NextStateByInitialRiskValue");
            if (ct != null)
            {
                var cTrans = pathList.Where(n => n.ParentPath.Equals(ct.ParentPath + _pathDivider + "calculatedStateParams", StringComparison.OrdinalIgnoreCase));

                foreach (var c in cTrans)
                {
                    // get the name of the method which is running the transition for description
                    var tName = pathList.FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Equals(TrimPathEnd(c.ParentPath, 2, _pathDivider), StringComparison.OrdinalIgnoreCase));

                    var newLink = new StateLink()
                    {
                        FromState = stateObject.Value,
                        ToState = c.Name,
                        Tag = new ShapeTag
                        {
                            Description = tName?.Value ?? "",
                            FileName = filePath,
                            JsonPath = c.Path,
                        }
                    };

                    links.Add(newLink);
                }
            }

            // find all PreviousState transitions defined
            var returntransitions = pathList.Where(n => n.Name.Equals("calculatedState", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Contains(_pathDivider + "transition"));
            var returnLink = returntransitions.FirstOrDefault(n => n.Value.Equals("GetPreviousState", StringComparison.OrdinalIgnoreCase));

            var newReturnTag = new ShapeTag();
            if (returnLink != null)
            {
                newReturnTag.Description = "GetPreviousState";
                newReturnTag.FileName = filePath;
                newReturnTag.JsonPath = returnLink.Path;
            }

            state = new ParsedState()
            {
                Name = stateObject.Value,
                Tag = new ShapeTag
                {
                    Description = stateObject.Value,
                    FileName = filePath,
                    JsonPath = stateObject.Path,
                    Color = returnLink == null ? _defaultShapeColor : _returnOnlyShapeColor
                },
                ToPreviousState = returnLink != null,
                ToPreviousStateTag = newReturnTag
            };
        }

        private void GetStateV2(IEnumerable<ParsedProperty> pathList, string filePath, out ParsedState state, out List<StateLink> links)
        {
            state = new ParsedState();
            links = new List<StateLink>();

            // ignore metadata file
            if (filePath.EndsWith("configuration.json", StringComparison.OrdinalIgnoreCase))
                return;

            // find state defined
            var stateObject = pathList.FirstOrDefault(n => n.Path.Equals(RootName + _pathDivider + "name", StringComparison.OrdinalIgnoreCase));

            if (stateObject == null)
                return;

            // find all transitions defined
            var transitions = pathList.Where(n => n.Name.Equals("type", StringComparison.OrdinalIgnoreCase) && n.Value.Equals("transition", StringComparison.OrdinalIgnoreCase)).ToList();

            // Generate links from current state
            // state_name_to, filter_method_name
            foreach (var transition in transitions)
            {
                var tName = pathList.FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && n.ParentPath == transition.ParentPath);

                var newLink = new StateLink()
                {
                    FromState = stateObject.Value,
                    ToState = tName?.Value,
                    Tag = new ShapeTag
                    {
                        Description = tName?.Value ?? "",
                        FileName = filePath,
                        JsonPath = tName?.Path ?? ""
                    }
                };

                links.Add(newLink);
            }

            state = new ParsedState()
            {
                Name = stateObject.Value,
                Tag = new ShapeTag
                {
                    FileName = filePath,
                    JsonPath = stateObject.Path,
                    Description = stateObject.Value,
                    Color = _defaultShapeColor
                }
            };
        }

        private IEnumerable<ParsedProperty> ParseJson(string filePath, JsonPathParser parser)
        {
            string text;
            try
            {
                text = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

            // replace &nbsp char with space
            if (text.Contains((char)160))
            {
                text = text.Replace((char)160, (char)32);
            }

            var pos = -1;
            var errorFound = false;
            IEnumerable<ParsedProperty> newPathList;
            try
            {
                newPathList = parser.ParseJsonToPathList(text, out pos, out errorFound);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

            // parsing failed
            if (errorFound)
            {
                MessageBox.Show($"Error parsing \"{filePath}\" at position {pos}");
                return null;
            }

            return newPathList;
        }

        private void CreateShapes(Diagram diagram, List<ParsedState> states)
        {
            foreach (var state in states)
            {
                var name = state.Name;
                DrawShape(diagram, name, state.Tag);
                AddNodeToJson(name, name);
            }
        }

        private void DrawShape(Diagram diagram, string name, ShapeTag tag, bool incorrect = false)
        {
            if (_shapeDict.TryGetValue(name, out var _))
                return;

            RectangleBase shape;
            shape = (RectangleBase)project1.ShapeTypes[_shapeType].CreateInstance();
            shape.Width = _initW;
            shape.Height = _initH;
            shape.X = _initX;
            shape.Y = _initY;
            shape.Tag = tag;
            if (!string.IsNullOrEmpty(tag.Color))
            {
                shape.FillStyle = FindColorByName(tag.Color);
            }

            if (incorrect)
            {
                shape.FillStyle = FindColorByName(_orphanShapeColor);
            }

            shape.SetCaptionText(0, name);

            diagram.Shapes.Add(shape);
            _shapeDict.Add(name, shape);
        }

        void CreateLinks(Diagram diagram, List<ParsedState> states, List<StateLink> links)
        {
            for (var i = 0; i < links.Count; i++)
            {
                var link = links[i];

                var tag = new ShapeTag()
                {
                    FileName = link.Tag.FileName,
                    JsonPath = link.Tag.JsonPath,
                    Description = ""
                };

                // there should be exactly one state to point to
                var remoteState = states.FirstOrDefault(n => n.Name == link.ToState);

                // if there is a link to non-existing state then create this state in red color
                if (remoteState == null)
                {
                    var newState = new ParsedState()
                    {
                        Name = link.ToState,
                        Orphan = true,
                        Tag = new ShapeTag
                        {
                            FileName = link.Tag.FileName,
                            JsonPath = link.Tag.JsonPath,
                            Description = "Missing state",
                            Color = _orphanShapeColor
                        }
                    };
                    states.Add(newState);

                    link.ToOrphan = true;
                    tag.Description = "Incorrect state name!!!";
                    tag.Color = "Red";
                    DrawShape(diagram, link.ToState, tag, true);
                    AddNodeToJson(link.ToState, link.ToState);
                }

                DrawLink(diagram, link.FromState, link.ToState, tag, link.ToPreviousState);
                AddEdgeToJson(link.FromState, link.ToState, link.Tag.ToString());

                // create reverse links to all links defined in the remote state
                if (remoteState != null && remoteState.ToPreviousState)
                {
                    var newReturnLink = new StateLink
                    {
                        FromState = link.ToState,
                        ToState = link.FromState,
                        Tag = remoteState.ToPreviousStateTag,
                        ToPreviousState = true,
                    };
                    links.Add(newReturnLink);
                    /*tag.Description = "Return_to_previous_state";
                    DrawLink(diagram, link.ToState, link.FromState, tag, true);
                    AddEdgeToJson(link.ToState, link.FromState, link.Tag.ToString());*/
                }
            }
        }

        void DrawLink(Diagram diagram, string fromName, string toName, ShapeTag tag, bool backArrow = false)
        {
            Polyline arrow = (Polyline)project1.ShapeTypes[_arrowLineType].CreateInstance();
            diagram.Shapes.Add(arrow);

            if (backArrow)
                arrow.EndCapStyle = project1.Design.CapStyles.FirstOrDefault(n => n.Name == _reverseArrowType);
            else
                arrow.EndCapStyle = project1.Design.CapStyles.FirstOrDefault(n => n.Name == _normalArrowType);

            arrow.Tag = tag;
            if (!_shapeDict.TryGetValue(fromName, out var shStart))
            {
                MessageBox.Show($"State \"{fromName}\" does not exist");
            }
            else
            {
                // Connect one of the line shape's endings (first vertex) to the referring shape's reference point
                arrow.Connect(ControlPointId.FirstVertex, shStart, ControlPointId.Reference);
            }

            if (!_shapeDict.TryGetValue(toName, out var shEnd))
            {
                MessageBox.Show($"State \"{fromName}\" does not exist");
            }
            else
            {
                // Connect the other of the line shape's endings (last vertex) to the referred shape
                arrow.Connect(ControlPointId.LastVertex, shEnd, ControlPointId.Reference);
            }
        }

        void ColorizeStates(Diagram diagram, List<ParsedState> states, List<StateLink> links)
        {
            var shapes = diagram.Shapes;

            // подсветить зеленым стейты, из которых только выходы (начальные)
            var allStates = states.Where(n => !n.Orphan).Select(n => n.Name).Distinct();
            var statesWithInput = links.Select(n => n.ToState).Distinct();
            var statesWithOutput = links.Select(n => n.FromState).Distinct();

            var startingBlockNames = allStates.Except(statesWithInput);
            var endingBlockNames = allStates.Except(statesWithOutput);

            foreach (var blockName in startingBlockNames)
            {
                var block = states.FirstOrDefault(n => n.Name == blockName);
                block.Tag.Color = _startingShapeColor;
                var shape = shapes.FirstOrDefault(n => n is CaptionedShapeBase shapeObject && shapeObject.GetCaptionText(0) == blockName);
                ((CaptionedShapeBase)shape).FillStyle = FindColorByName(_startingShapeColor);
            }

            // подсветить желтым стейты, в которые идут только входы (конечные)
            foreach (var blockName in endingBlockNames)
            {
                var block = states.FirstOrDefault(n => n.Name == blockName);
                block.Tag.Color = _endingShapeColor;
                var shape = shapes.FirstOrDefault(n => n is CaptionedShapeBase shapeObject && shapeObject.GetCaptionText(0) == block.Name);
                ((RectangleBase)shape).FillStyle = FindColorByName(_endingShapeColor);
            }
        }

        private IFillStyle FindColorByName(string colorName)
        {
            var design = project1?.Repository?.GetDesigns();
            var fillStyles = design?.FirstOrDefault()?.FillStyles;
            return fillStyles?.FirstOrDefault(n => n.Name == colorName);
        }

        void RefreshLayout()
        {
            if (display1 == null || display1.Diagram == null || display1.Diagram.Shapes == null)
                return;

            // First, place all shapes to the same position
            foreach (Shape shape in display1.Diagram.Shapes)
            {
                shape.X = _initX;
                shape.Y = _initY;
            }

            // Create the layouter and set up layout parameters
            /*ExpansionLayouter layouter = new ExpansionLayouter(project1)
            {
                HorizontalCompression = 0,
                VerticalCompression = 0,
                AllShapes = display1.Diagram.Shapes,
                Shapes = display1.Diagram.Shapes
            };*/

            /*FlowLayouter layouter = new FlowLayouter(project1)
            {
                Direction = FlowLayouter.FlowDirection.TopDown,
                LayerDistance = _initH + 100,
                RowDistance = _initW + 100,
                AllShapes = display1.Diagram.Shapes,
                Shapes = display1.Diagram.Shapes
            };*/

            /*GridLayouter layouter = new GridLayouter(project1)
            {
                CoarsenessX = 1,
                CoarsenessY = 1,
                AllShapes = display1.Diagram.Shapes,
                Shapes = display1.Diagram.Shapes
            };*/

            RepulsionLayouter layouter = new RepulsionLayouter(project1)
            {
                // Set the repulsion force and its range
                SpringRate = 2,
                Repulsion = 10,
                RepulsionRange = 500,
                // Set the friction and the mass of the shapes
                Friction = 0,
                Mass = 50,
                // Set all shapes 
                AllShapes = display1.Diagram.Shapes,
                // Set shapes that should be layouted
                Shapes = display1.Diagram.Shapes,
            };

            // Now prepare and execute the layouter
            try
            {
                layouter.Prepare();
                layouter.Execute(30);
            }
            catch
            { }

            // Fit the result into the diagram bounds
            layouter.Fit(_initX, _initY, display1.Diagram.Width - _initW - _initX, display1.Diagram.Height - _initH - _initY);
        }

        private void ShowPreviewEditor(string longFileName,
            string jsonPath,
            bool standAloneEditor = false)
        {
            if (_useVsCode)
            {
                var lineNumber = GetLineNumberForPath(longFileName, jsonPath);
                var execParams = "-r -g " + longFileName + ":" + lineNumber;
                VsCodeOpenFile(execParams);

                return;
            }

            var textEditor = _sideViewer;
            if (standAloneEditor) textEditor = null;

            var fileLoaded = false;
            var newWindow = false;
            if (textEditor != null && !textEditor.IsDisposed)
            {
                if (textEditor.SingleLineBrackets != _singleLineBrackets ||
                    textEditor.Text != PreViewCaption + longFileName)
                {
                    textEditor.SingleLineBrackets = _singleLineBrackets;
                    fileLoaded = textEditor.LoadJsonFromFile(longFileName);
                }
                else
                {
                    fileLoaded = true;
                }
            }
            else
            {
                if (textEditor != null)
                {
                    textEditor.Close();
                    textEditor.Dispose();
                }

                textEditor = new JsonViewer("", "", standAloneEditor)
                {
                    SingleLineBrackets = _singleLineBrackets
                };

                newWindow = true;
                fileLoaded = textEditor.LoadJsonFromFile(longFileName);
            }

            if (!standAloneEditor)
                _sideViewer = textEditor;

            textEditor.AlwaysOnTop = false;
            textEditor.Show();


            if (!standAloneEditor && newWindow)
            {
                if (!(_editorPosition.WinX == 0
                      && _editorPosition.WinY == 0
                      && _editorPosition.WinW == 0
                      && _editorPosition.WinH == 0))
                {
                    textEditor.Location = new Point(_editorPosition.WinX, _editorPosition.WinY);
                    textEditor.Width = _editorPosition.WinW;
                    textEditor.Height = _editorPosition.WinH;
                }

                textEditor.Closing += OnClosingEditor;
                textEditor.ResizeEnd += OnResizeEditor;
            }

            if (!fileLoaded)
            {
                textEditor.Text = "Failed to load " + longFileName;
                return;
            }

            if (!standAloneEditor)
                textEditor.Text = PreViewCaption + longFileName;
            else
                textEditor.Text = longFileName;

            textEditor.HighlightPathJson(jsonPath);
        }

        private void VsCodeOpenFile(string command)
        {
            var processInfo = new ProcessStartInfo("code", command)
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int GetLineNumberForPath(string longFileName, string jsonPath)
        {
            string jsonStr;
            try
            {
                jsonStr = File.ReadAllText(longFileName);
            }
            catch
            {
                return 0;
            }

            if (string.IsNullOrEmpty(jsonStr))
                return 0;

            var startLine = 0;
            var property = _parser.SearchJsonPath(jsonStr, jsonPath);
            _parser.SearchStartOnly = true;

            if (property != null)
            {
                JsonPathParser.GetLinesNumber(jsonStr, property.StartPosition, property.EndPosition, out startLine,
                    out var _);
            }

            return startLine;
        }

        private void AddNodeToJson(string nodeName, string labelText)
        {
            var newNode = new DiagramNode()
            {
                stringId = nodeName,
                label = labelText
            };
            _diagramJson.AddNode(newNode);
        }

        private void AddEdgeToJson(string sourceName, string targetName, string labelText)
        {
            var retLink = new DiagramEdge()
            {
                source = sourceName,
                target = targetName,
                label = labelText
            };

            try
            {
                _diagramJson.AddEdge(retLink);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static string TrimPathEnd(string originalPath, int levels, char pathDivider)
        {
            for (; levels > 0; levels--)
            {
                var pos = originalPath.LastIndexOf(pathDivider);
                if (pos >= 0)
                {
                    originalPath = originalPath.Substring(0, pos);
                }
                else
                    break;
            }

            return originalPath;
        }
        #endregion
    }
}
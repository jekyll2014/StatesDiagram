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
        public class ParsedState
        {
            public string Name = "";
            public bool ToPreviousState = false;
            public string FileName = "";
            public string JsonPath = "";
            public List<StateLink> LinksTo = new List<StateLink>();
        }

        public class StateLink
        {
            public string ToState = "";
            public string Tag = "";
            public string FileName = "";
            public string JsonPath = "";
        }

        public class DiagramNode
        {
            public int id = 0;
            public string stringId = "";
            public string label = "";
        }

        public class DiagramEdge
        {
            public int from = 0;
            public int to = 0;
            public string source = "";
            public string target = "";
            public string label;
        }

        public class DiagramExport
        {
            private int idCounter = 0;
            private Dictionary<string, int> dic = new Dictionary<string, int>();
            public List<DiagramNode> nodes = new List<DiagramNode>();
            public List<DiagramEdge> edges = new List<DiagramEdge>();

            public void AddNode(DiagramNode newNode)
            {
                if (!dic.ContainsKey(newNode.stringId))
                {
                    newNode.id = idCounter;
                    dic.Add(newNode.stringId, idCounter);
                    nodes.Add(newNode);
                    idCounter++;
                }
            }

            public void AddNodes(IEnumerable<DiagramNode> newNodes)
            {
                foreach (var node in newNodes)
                {
                    AddNode(node);
                }
            }

            public void AddEdge(DiagramEdge newEdge)
            {
                var r = dic.TryGetValue(newEdge.source, out var fromId);
                r &= dic.TryGetValue(newEdge.target, out var toId);
                if (r)
                {
                    newEdge.from = fromId;
                    newEdge.to = toId;
                    edges.Add(newEdge);
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Id's \"{newEdge.source}\" or \"{newEdge.target}\" were not found");
                }
            }
        }

        public class ShapeTag
        {
            public string FileName = "";
            public string JsonPath = "";
            public string Description = "";

            public override string ToString()
            {
                return $"File: {FileName}{Environment.NewLine}JSON path: {JsonPath}{Environment.NewLine}Description: {Description}";
            }
        }

        public struct WinPosition
        {
            public int WinX;
            public int WinY;
            public int WinW;
            public int WinH;

            [JsonIgnore] public bool Initialized => !(WinX <= 0 && WinY <= 0 && WinW <= 0 && WinH <= 0);
        }

        // diagram settings
        private string _currentDiagramName = "Sample Diagram";
        private const string _shapeType = "RoundedBox"; //"RoundedBox", "Ellipse"
        private const string _defaultArrowType = "ArrowOpen";
        private const string _defaultNormalArrowType = "StraightArrow";
        private const string _defaultReverseArrowType = "BackArrow";
        private const string _arrowLineType = "Polyline";

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

            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
                return;

            Properties.Settings.Default.LastFolder = folderBrowserDialog1.SelectedPath;
            var filesList = Directory.GetFiles(folderBrowserDialog1.SelectedPath,
                "*.json",
                SearchOption.AllDirectories);

            var states = new List<ParsedState>();
            _shapeDict.Clear();
            _diagramJson = new DiagramExport();
            foreach (var file in filesList)
            {
                var newPathList = ParseJson(file, _parser);

                if (newPathList == null)
                    continue;

                ParsedState newState;

                if (Properties.Settings.Default.useV1)
                    newState = GetStateV1(newPathList, file);
                else
                    newState = GetStateV2(newPathList, file);

                if (newState != null)
                    states.Add(newState);
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

            GenerateStatesDiagram(diagram, states);
            ConnectStateShapes(diagram, states);

            cachedRepository1.InsertAll(diagram);
            display1.Diagram = diagram;

            RefreshLayout();
        }

        private void Button_save_Click(object sender, EventArgs e)
        {
            xmlStore1.DirectoryName = @".";
            xmlStore1.FileExtension = "nspj";

            try
            {
                project1.Repository.Update(project1.Design);
                project1.Repository.Update();
                project1.Repository.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_load_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Title = "Open NSPJ file";
            openFileDialog1.DefaultExt = "nspj";
            openFileDialog1.Filter = "NSPJ files|*.nspj|All files|*.*";
            openFileDialog1.ShowDialog();
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            var i = new FileInfo(openFileDialog1.FileName);

            project1.Close();
            project1.RemoveAllLibraries();
            project1.LibrarySearchPaths.Clear();

            // Set path to the sample diagram and the diagram file extension
            xmlStore1.DirectoryName = i.Directory.FullName;
            xmlStore1.FileExtension = i.Extension;
            // Set the name of the project that should be loaded from the store
            project1.Name = i.Name.Replace(i.Extension, "");
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
            var d = project1.Repository.GetDiagrams().FirstOrDefault();
            display1.LoadDiagram(d.Name);

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
            if (e?.Shape?.Tag != null && e?.Shape?.Tag is ShapeTag t)
            {
                textBox_tag.Text = t.ToString();
                if (_showPreview) ShowPreviewEditor(t.FileName, t.JsonPath, false);
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
            if (sender is Form s)
            {
                _editorPosition.WinX = s.Location.X;
                _editorPosition.WinY = s.Location.Y;
                _editorPosition.WinW = s.Width;
                _editorPosition.WinH = s.Height;
            }
        }

        private void OnResizeEditor(object sender, EventArgs e)
        {
            if (sender is Form s)
            {
                _editorPosition.WinX = s.Location.X;
                _editorPosition.WinY = s.Location.Y;
                _editorPosition.WinW = s.Width;
                _editorPosition.WinH = s.Height;
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
            xmlStore1.FileExtension = "nspj";
            project1.Name = _currentDiagramName;
            project1.Close();
            project1.RemoveAllLibraries();

            project1.Create();
            project1.AddLibrary(typeof(Box).Assembly, false);

            if (Properties.Settings.Default.useV1)
            {
                _normalArrowType = _defaultNormalArrowType;
                var straightArrow = new CapStyle(_normalArrowType)
                {
                    CapShape = CapShape.ClosedArrow,
                    CapSize = 30,
                    ColorStyle = project1.Design.ColorStyles.LightGreen,
                };
                project1.Design.CapStyles.Add(straightArrow, straightArrow);

                _reverseArrowType = _defaultReverseArrowType;
                var backArrow = new CapStyle(_reverseArrowType)
                {
                    CapShape = CapShape.ClosedArrow,
                    CapSize = 30,
                    ColorStyle = project1.Design.ColorStyles.Red,
                };
                project1.Design.CapStyles.Add(backArrow, backArrow);

                // need to save CapStyle changes in the project to allow export *.nspj file
                //project1.Design.AddStyle(backArrow);
                //project1.Design.AddStyle(straightArrow);
                project1.Repository.Update(project1.Design);
                project1.Repository.Update();
                //but it doesn't work for some reason
            }
            else
            {
                _normalArrowType = _defaultArrowType;
                _reverseArrowType = _defaultArrowType;
            }
            display1.ZoomWithMouseWheel = true;
            display1.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            display1.ActiveTool = new SelectionTool();

            textBox_sizeX.Text = diagramWidth.ToString();
            textBox_sizeY.Text = diagramHeight.ToString();
        }

        private ParsedState GetStateV1(IEnumerable<ParsedProperty> pathList, string filePath)
        {
            // find state defined
            var state = pathList.FirstOrDefault(n => n.Path.Equals(RootName + _pathDivider + "state", StringComparison.OrdinalIgnoreCase));

            // find all transitions defined
            var transitions = pathList.Where(n => n.Name.Equals("state", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Contains(_pathDivider + "transition")).ToList();

            // find all PreviousState transitions defined
            var returntransitions = pathList.Where(n => n.Name.Equals("calculatedState", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Contains(_pathDivider + "transition"));
            var returnLink = returntransitions.Any(n => n.Value.Equals("GetPreviousState", StringComparison.OrdinalIgnoreCase));

            // state_name_to, filter_method_name
            List<StateLink> tLinks = new List<StateLink>();
            foreach (var transition in transitions)
            {
                var tName = pathList.FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && n.ParentPath == TrimPathEnd(transition.ParentPath, 1, _pathDivider));
                if (!tLinks.Any(n => n.ToState == transition.Value))
                {
                    var newLink = new StateLink()
                    {
                        FileName = filePath,
                        JsonPath = transition?.Path,
                        Tag = tName?.Value ?? "",
                        ToState = transition?.Value
                    };

                    tLinks.Add(newLink);
                }
            }

            // find all calculated transitions defined
            var ctransitions = pathList.Where(n => n.Name.Equals("calculatedState", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Contains(_pathDivider + "transition"));
            var ct = ctransitions.FirstOrDefault(n => n.Value == "NextStateByInitialRiskValue");
            if (ct != null)
            {
                var cTrans = pathList.Where(n => n.ParentPath.Equals(ct.ParentPath + _pathDivider + "calculatedStateParams", StringComparison.OrdinalIgnoreCase));

                foreach (var c in cTrans)
                {
                    var tName = pathList.FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Equals(TrimPathEnd(c.ParentPath, 2, _pathDivider), StringComparison.OrdinalIgnoreCase));
                    var newLink = new StateLink()
                    {
                        FileName = filePath,
                        JsonPath = c?.Path,
                        Tag = tName?.Value ?? "",
                        ToState = c?.Name
                    };

                    tLinks.Add(newLink);
                }
            }

            var newState = new ParsedState()
            {
                Name = state?.Value,
                FileName = filePath,
                JsonPath = state?.Path,
                LinksTo = tLinks,
                ToPreviousState = returnLink
            };

            return newState;
        }

        private ParsedState GetStateV2(IEnumerable<ParsedProperty> pathList, string filePath)
        {
            if (filePath.EndsWith("configuration.json", StringComparison.OrdinalIgnoreCase))
                return null;

            // find state defined
            var state = pathList.FirstOrDefault(n => n.Path.Equals(RootName + _pathDivider + "name", StringComparison.OrdinalIgnoreCase));

            // find all transitions defined
            var transitions = pathList.Where(n => n.Name.Equals("type", StringComparison.OrdinalIgnoreCase) && n.Value.Equals("transition", StringComparison.OrdinalIgnoreCase)).ToList();

            // state_name_to, filter_method_name
            List<StateLink> tLinks = new List<StateLink>();
            foreach (var transition in transitions)
            {
                var tName = pathList.FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && n.ParentPath == transition.ParentPath);
                var newLink = new StateLink()
                {
                    FileName = filePath,
                    JsonPath = tName?.Path,
                    Tag = tName?.Value ?? "",
                    ToState = tName?.Value
                };

                tLinks.Add(newLink);
            }

            var newState = new ParsedState()
            {
                Name = state?.Value,
                FileName = filePath,
                JsonPath = state?.Path,
                LinksTo = tLinks,
            };

            return newState;
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
            IEnumerable<ParsedProperty> newPathList = null;
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

        private void GenerateStatesDiagram(Diagram diagram, List<ParsedState> states)
        {
            foreach (var state in states)
            {
                var name = state.Name;
                var tag = new ShapeTag()
                {
                    FileName = state?.FileName,
                    JsonPath = state?.JsonPath,
                    Description = ""
                };

                CreateShape(diagram, name, tag);

                var newNode = new DiagramNode()
                {
                    stringId = name,
                    label = name
                };
                _diagramJson.AddNode(newNode);
            }
        }

        private void CreateShape(Diagram diagram, string name, ShapeTag tag, bool incorrect = false)
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

            if (incorrect)
            {
                var d = project1.Repository.GetDesigns();
                var f = d.FirstOrDefault().FillStyles;
                shape.FillStyle = f.Red;
            }

            shape.SetCaptionText(0, name);

            diagram.Shapes.Add(shape);
            _shapeDict.Add(name, shape);
        }

        void ConnectStateShapes(Diagram diagram, List<ParsedState> states)
        {
            foreach (var state in states)
            {
                foreach (var link in state.LinksTo)
                {
                    var tag = new ShapeTag()
                    {
                        FileName = link?.FileName,
                        JsonPath = link?.JsonPath,
                        Description = ""
                    };

                    var s = states.FirstOrDefault(n => n.Name == link.ToState);
                    if (s == null)
                    {
                        tag.Description = "Incorrect state name!!!";
                        CreateShape(diagram, link.ToState, tag, true);
                    }

                    CreateLink(diagram, state.Name, link.ToState, tag);

                    var newLink = new DiagramEdge()
                    {
                        source = state.Name,
                        target = link.ToState,
                        label = link.Tag
                    };

                    try
                    {
                        _diagramJson.AddEdge(newLink);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    // create reverse links to all links defined
                    if (s != null && s.ToPreviousState)
                    {
                        tag.Description = "To_previous_state";
                        CreateLink(diagram, link.ToState, state.Name, tag, true);

                        var retLink = new DiagramEdge()
                        {
                            source = link.ToState,
                            target = state.Name,
                            label = link.Tag
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
                }
            }
        }

        void CreateLink(Diagram diagram, string fromName, string toName, ShapeTag tag, bool backArrow = false)
        {
            Polyline arrow1 = (Polyline)project1.ShapeTypes[_arrowLineType].CreateInstance();
            diagram.Shapes.Add(arrow1);

            if (backArrow)
                arrow1.EndCapStyle = project1.Design.CapStyles.FirstOrDefault(n => n.Name == _reverseArrowType);
            else
                arrow1.EndCapStyle = project1.Design.CapStyles.FirstOrDefault(n => n.Name == _normalArrowType);

            arrow1.Tag = tag;
            if (!_shapeDict.TryGetValue(fromName, out var shStart))
            {
                MessageBox.Show($"State \"{fromName}\" does not exist");
            }
            else
            {
                // Connect one of the line shape's endings (first vertex) to the referring shape's reference point
                arrow1.Connect(ControlPointId.FirstVertex, shStart, ControlPointId.Reference);
            }
            if (!_shapeDict.TryGetValue(toName, out var shEnd))
            {
                MessageBox.Show($"State \"{fromName}\" does not exist");
            }
            else
            {
                // Connect the other of the line shape's endings (last vertex) to the referred shape
                arrow1.Connect(ControlPointId.LastVertex, shEnd, ControlPointId.Reference);
            }
        }

        void RefreshLayout()
        {
            // First, place all shapes to the same position
            foreach (Shape s in display1.Diagram.Shapes)
            {
                s.X = _initX;
                s.Y = _initY;
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
            layouter.Prepare();
            layouter.Execute(10);
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

namespace StatesDiagram
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            Dataweb.NShape.RoleBasedSecurityManager roleBasedSecurityManager1 = new Dataweb.NShape.RoleBasedSecurityManager();
            this.display1 = new Dataweb.NShape.WinFormsUI.Display();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_useV1 = new System.Windows.Forms.CheckBox();
            this.textBox_tag = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_sizeY = new System.Windows.Forms.TextBox();
            this.textBox_sizeX = new System.Windows.Forms.TextBox();
            this.button_loadStates = new System.Windows.Forms.Button();
            this.button_RefreshLayout = new System.Windows.Forms.Button();
            this.button_load = new System.Windows.Forms.Button();
            this.button_save = new System.Windows.Forms.Button();
            this.button_export = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.diagramSetController1 = new Dataweb.NShape.Controllers.DiagramSetController();
            this.project1 = new Dataweb.NShape.Project(this.components);
            this.cachedRepository1 = new Dataweb.NShape.Advanced.CachedRepository();
            this.xmlStore1 = new Dataweb.NShape.XmlStore();
            this.toolSetController1 = new Dataweb.NShape.Controllers.ToolSetController();
            this.toolSetListViewPresenter1 = new Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter(this.components);
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.display1.SuspendLayout();
            this.SuspendLayout();
            // 
            // display1
            // 
            this.display1.AllowDrop = true;
            this.display1.BackColorGradient = System.Drawing.SystemColors.Control;
            this.display1.Controls.Add(this.label3);
            this.display1.Controls.Add(this.label2);
            this.display1.Controls.Add(this.checkBox_useV1);
            this.display1.Controls.Add(this.textBox_tag);
            this.display1.Controls.Add(this.label1);
            this.display1.Controls.Add(this.textBox_sizeY);
            this.display1.Controls.Add(this.textBox_sizeX);
            this.display1.Controls.Add(this.button_loadStates);
            this.display1.Controls.Add(this.button_RefreshLayout);
            this.display1.Controls.Add(this.button_load);
            this.display1.Controls.Add(this.button_save);
            this.display1.Controls.Add(this.button_export);
            this.display1.Controls.Add(this.listView1);
            this.display1.DiagramMargin = 40;
            this.display1.DiagramSetController = this.diagramSetController1;
            this.display1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.display1.GridColor = System.Drawing.Color.Gainsboro;
            this.display1.GridSize = 19;
            this.display1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.display1.Location = new System.Drawing.Point(0, 0);
            this.display1.Name = "display1";
            this.display1.PropertyController = null;
            this.display1.SelectionHilightColor = System.Drawing.Color.Firebrick;
            this.display1.SelectionInactiveColor = System.Drawing.Color.Gray;
            this.display1.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
            this.display1.SelectionNormalColor = System.Drawing.Color.DarkGreen;
            this.display1.ShowScrollBars = true;
            this.display1.Size = new System.Drawing.Size(800, 450);
            this.display1.TabIndex = 0;
            this.display1.ToolPreviewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
            this.display1.ToolPreviewColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.display1.ShapeClick += new System.EventHandler<Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs>(this.Display1_ShapeClick);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(664, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Element notes:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(664, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Diagram size";
            // 
            // checkBox_useV1
            // 
            this.checkBox_useV1.AutoSize = true;
            this.checkBox_useV1.Location = new System.Drawing.Point(12, 160);
            this.checkBox_useV1.Name = "checkBox_useV1";
            this.checkBox_useV1.Size = new System.Drawing.Size(91, 17);
            this.checkBox_useV1.TabIndex = 16;
            this.checkBox_useV1.Text = "SHELF JSON";
            this.checkBox_useV1.UseVisualStyleBackColor = true;
            this.checkBox_useV1.CheckedChanged += new System.EventHandler(this.CheckBox_useV1_CheckedChanged);
            // 
            // textBox_tag
            // 
            this.textBox_tag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tag.Location = new System.Drawing.Point(667, 64);
            this.textBox_tag.Multiline = true;
            this.textBox_tag.Name = "textBox_tag";
            this.textBox_tag.ReadOnly = true;
            this.textBox_tag.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBox_tag.Size = new System.Drawing.Size(121, 106);
            this.textBox_tag.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(724, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "x";
            // 
            // textBox_sizeY
            // 
            this.textBox_sizeY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sizeY.Location = new System.Drawing.Point(738, 25);
            this.textBox_sizeY.Name = "textBox_sizeY";
            this.textBox_sizeY.Size = new System.Drawing.Size(50, 20);
            this.textBox_sizeY.TabIndex = 13;
            this.textBox_sizeY.Leave += new System.EventHandler(this.TextBox_sizeY_Leave);
            // 
            // textBox_sizeX
            // 
            this.textBox_sizeX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sizeX.Location = new System.Drawing.Point(667, 25);
            this.textBox_sizeX.MaxLength = 5;
            this.textBox_sizeX.Name = "textBox_sizeX";
            this.textBox_sizeX.Size = new System.Drawing.Size(50, 20);
            this.textBox_sizeX.TabIndex = 13;
            this.textBox_sizeX.WordWrap = false;
            this.textBox_sizeX.Leave += new System.EventHandler(this.TextBox_sizeX_Leave);
            // 
            // button_loadStates
            // 
            this.button_loadStates.Location = new System.Drawing.Point(12, 12);
            this.button_loadStates.Name = "button_loadStates";
            this.button_loadStates.Size = new System.Drawing.Size(75, 23);
            this.button_loadStates.TabIndex = 12;
            this.button_loadStates.Text = "Load states";
            this.button_loadStates.UseVisualStyleBackColor = true;
            this.button_loadStates.Click += new System.EventHandler(this.Button_loadStates_Click);
            // 
            // button_RefreshLayout
            // 
            this.button_RefreshLayout.Location = new System.Drawing.Point(12, 44);
            this.button_RefreshLayout.Name = "button_RefreshLayout";
            this.button_RefreshLayout.Size = new System.Drawing.Size(75, 23);
            this.button_RefreshLayout.TabIndex = 11;
            this.button_RefreshLayout.Text = "Re-Layout";
            this.button_RefreshLayout.UseVisualStyleBackColor = true;
            this.button_RefreshLayout.Click += new System.EventHandler(this.Button_RefreshLayout_Click);
            // 
            // button_load
            // 
            this.button_load.Location = new System.Drawing.Point(12, 131);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(75, 23);
            this.button_load.TabIndex = 10;
            this.button_load.Text = "Load";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.Button_load_Click);
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(12, 102);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(75, 23);
            this.button_save.TabIndex = 10;
            this.button_save.Text = "Save";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.Button_save_Click);
            // 
            // button_export
            // 
            this.button_export.Location = new System.Drawing.Point(12, 73);
            this.button_export.Name = "button_export";
            this.button_export.Size = new System.Drawing.Size(75, 23);
            this.button_export.TabIndex = 10;
            this.button_export.Text = "Export";
            this.button_export.UseVisualStyleBackColor = true;
            this.button_export.Click += new System.EventHandler(this.Button_export_Click);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(667, 176);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(121, 262);
            this.listView1.TabIndex = 9;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // diagramSetController1
            // 
            this.diagramSetController1.ActiveTool = null;
            this.diagramSetController1.Project = this.project1;
            // 
            // project1
            // 
            this.project1.Description = null;
            this.project1.LibrarySearchPaths = ((System.Collections.Generic.IList<string>)(resources.GetObject("project1.LibrarySearchPaths")));
            this.project1.Name = null;
            this.project1.Repository = this.cachedRepository1;
            roleBasedSecurityManager1.CurrentRole = Dataweb.NShape.StandardRole.Administrator;
            roleBasedSecurityManager1.CurrentRoleName = "Administrator";
            this.project1.SecurityManager = roleBasedSecurityManager1;
            // 
            // cachedRepository1
            // 
            this.cachedRepository1.ProjectName = null;
            this.cachedRepository1.Store = this.xmlStore1;
            this.cachedRepository1.Version = 0;
            // 
            // xmlStore1
            // 
            this.xmlStore1.DesignFileName = "";
            this.xmlStore1.DirectoryName = "";
            this.xmlStore1.FileExtension = ".xml";
            this.xmlStore1.ImageLocation = Dataweb.NShape.XmlStore.ImageFileLocation.Directory;
            this.xmlStore1.ProjectFilePath = ".xml";
            this.xmlStore1.ProjectName = "";
            // 
            // toolSetController1
            // 
            this.toolSetController1.DiagramSetController = this.diagramSetController1;
            // 
            // toolSetListViewPresenter1
            // 
            this.toolSetListViewPresenter1.HideDeniedMenuItems = false;
            this.toolSetListViewPresenter1.ListView = this.listView1;
            this.toolSetListViewPresenter1.ShowDefaultContextMenu = true;
            this.toolSetListViewPresenter1.ToolSetController = this.toolSetController1;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog1_FileOk);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.display1);
            this.Name = "Form1";
            this.Text = "StatesDiagram";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.display1.ResumeLayout(false);
            this.display1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Dataweb.NShape.WinFormsUI.Display display1;
        private Dataweb.NShape.Controllers.DiagramSetController diagramSetController1;
        private Dataweb.NShape.Project project1;
        private Dataweb.NShape.Advanced.CachedRepository cachedRepository1;
        private Dataweb.NShape.XmlStore xmlStore1;
        private Dataweb.NShape.Controllers.ToolSetController toolSetController1;
        private System.Windows.Forms.ListView listView1;
        private Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter toolSetListViewPresenter1;
        private System.Windows.Forms.Button button_RefreshLayout;
        private System.Windows.Forms.Button button_export;
        private System.Windows.Forms.Button button_loadStates;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_sizeY;
        private System.Windows.Forms.TextBox textBox_sizeX;
        private System.Windows.Forms.TextBox textBox_tag;
        private System.Windows.Forms.CheckBox checkBox_useV1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_load;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}


namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmAutoMap
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAutoMap));
            this.wizardAutoMap = new DevExpress.XtraWizard.WizardControl();
            this.PageDataDownload = new DevExpress.XtraWizard.WizardPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmbThematicType = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbThematic = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btSave1 = new System.Windows.Forms.Button();
            this.tboutputGDB = new System.Windows.Forms.TextBox();
            this.txtUpgradeGDB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.cbAttach = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbBaseMapTemplate = new System.Windows.Forms.ComboBox();
            this.PageMapProcess = new DevExpress.XtraWizard.WizardPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.JustifyCulvertCmd = new System.Windows.Forms.CheckBox();
            this.button9 = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.button10 = new System.Windows.Forms.Button();
            this.HydlMaskProcessCmd = new System.Windows.Forms.CheckBox();
            this.btBOULSkip = new System.Windows.Forms.Button();
            this.BoulSkipDrawCmd = new System.Windows.Forms.CheckBox();
            this.button3 = new System.Windows.Forms.Button();
            this.RiverBoundayExtractCmd = new System.Windows.Forms.CheckBox();
            this.SymbolProcessCmd = new System.Windows.Forms.CheckBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.DirectionPointAdjustmentCmd = new System.Windows.Forms.CheckBox();
            this.btZoneColorCmd = new System.Windows.Forms.Button();
            this.ZoneColorCmd = new System.Windows.Forms.CheckBox();
            this.btAnno = new System.Windows.Forms.Button();
            this.MaplexAnnotateCmd = new System.Windows.Forms.CheckBox();
            this.RoadSymEndsCmd = new System.Windows.Forms.CheckBox();
            this.button12 = new System.Windows.Forms.Button();
            this.checkBox12 = new System.Windows.Forms.CheckBox();
            this.button7 = new System.Windows.Forms.Button();
            this.RiverAutoGradualCmd = new System.Windows.Forms.CheckBox();
            this.PageMapDes = new DevExpress.XtraWizard.WizardPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.NorthCmd = new System.Windows.Forms.CheckBox();
            this.btNorthCmd = new System.Windows.Forms.Button();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.btNameSet = new System.Windows.Forms.Button();
            this.tbMapName = new System.Windows.Forms.TextBox();
            this.tbProductFactory = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btAddFigureMapCmdXJ = new System.Windows.Forms.Button();
            this.AddFigureMapCmdXJ = new System.Windows.Forms.CheckBox();
            this.btScaleBarCmd = new System.Windows.Forms.Button();
            this.ScaleBarCmd = new System.Windows.Forms.CheckBox();
            this.btLengendDyCreateCmd = new System.Windows.Forms.Button();
            this.LengendDyCreateCmd = new System.Windows.Forms.CheckBox();
            this.btSDM = new System.Windows.Forms.Button();
            this.SDMColorCmd = new System.Windows.Forms.CheckBox();
            this.btFootBorderCmd = new System.Windows.Forms.Button();
            this.FootBorderCmd = new System.Windows.Forms.CheckBox();
            this.PageMapExport = new DevExpress.XtraWizard.WizardPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.GBColorMode = new System.Windows.Forms.GroupBox();
            this.rbCMYK = new System.Windows.Forms.RadioButton();
            this.rbRGB = new System.Windows.Forms.RadioButton();
            this.nudResolution = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnOutputFile = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.wizardAutoMap)).BeginInit();
            this.wizardAutoMap.SuspendLayout();
            this.PageDataDownload.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.PageMapProcess.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.PageMapDes.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.PageMapExport.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.GBColorMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudResolution)).BeginInit();
            this.SuspendLayout();
            // 
            // wizardAutoMap
            // 
            this.wizardAutoMap.CancelText = "取消";
            this.wizardAutoMap.Controls.Add(this.PageDataDownload);
            this.wizardAutoMap.Controls.Add(this.PageMapProcess);
            this.wizardAutoMap.Controls.Add(this.PageMapDes);
            this.wizardAutoMap.Controls.Add(this.PageMapExport);
            this.wizardAutoMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardAutoMap.FinishText = "完成";
            this.wizardAutoMap.HeaderImage = ((System.Drawing.Image)(resources.GetObject("wizardAutoMap.HeaderImage")));
            this.wizardAutoMap.HelpText = "帮助";
            this.wizardAutoMap.ImageLayout = System.Windows.Forms.ImageLayout.None;
            this.wizardAutoMap.Location = new System.Drawing.Point(0, 0);
            this.wizardAutoMap.Name = "wizardAutoMap";
            this.wizardAutoMap.NextText = "下一步";
            this.wizardAutoMap.Pages.AddRange(new DevExpress.XtraWizard.BaseWizardPage[] {
            this.PageDataDownload,
            this.PageMapProcess,
            this.PageMapDes,
            this.PageMapExport});
            this.wizardAutoMap.PreviousText = "上一步";
            this.wizardAutoMap.Size = new System.Drawing.Size(615, 469);
            this.wizardAutoMap.SelectedPageChanged += new DevExpress.XtraWizard.WizardPageChangedEventHandler(this.wizardAutoMap_SelectedPageChanged);
            this.wizardAutoMap.CancelClick += new System.ComponentModel.CancelEventHandler(this.wizardAutoMap_CancelClick);
            this.wizardAutoMap.FinishClick += new System.ComponentModel.CancelEventHandler(this.wizardAutoMap_FinishClick);
            this.wizardAutoMap.NextClick += new DevExpress.XtraWizard.WizardCommandButtonClickEventHandler(this.wizardAutoMap_NextClick);
            // 
            // PageDataDownload
            // 
            this.PageDataDownload.Controls.Add(this.groupBox1);
            this.PageDataDownload.DescriptionText = "根据出图范围从数据库下载数据，并完成地图符号化。";
            this.PageDataDownload.Name = "PageDataDownload";
            this.PageDataDownload.Size = new System.Drawing.Size(583, 324);
            this.PageDataDownload.Text = "数据获取";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(583, 324);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据获取";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cmbThematicType);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.cbThematic);
            this.groupBox3.Location = new System.Drawing.Point(7, 159);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(570, 71);
            this.groupBox3.TabIndex = 40;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "专题数据";
            // 
            // cmbThematicType
            // 
            this.cmbThematicType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbThematicType.Enabled = false;
            this.cmbThematicType.FormattingEnabled = true;
            this.cmbThematicType.Location = new System.Drawing.Point(358, 31);
            this.cmbThematicType.Name = "cmbThematicType";
            this.cmbThematicType.Size = new System.Drawing.Size(150, 20);
            this.cmbThematicType.TabIndex = 19;
            this.cmbThematicType.SelectedIndexChanged += new System.EventHandler(this.cmbThematicType_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(287, 36);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 18;
            this.label7.Text = "专题类型：";
            // 
            // cbThematic
            // 
            this.cbThematic.AutoSize = true;
            this.cbThematic.Location = new System.Drawing.Point(93, 35);
            this.cbThematic.Name = "cbThematic";
            this.cbThematic.Size = new System.Drawing.Size(96, 16);
            this.cbThematic.TabIndex = 16;
            this.cbThematic.Text = "下载专题数据";
            this.cbThematic.UseVisualStyleBackColor = true;
            this.cbThematic.CheckedChanged += new System.EventHandler(this.cbThematic_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btSave1);
            this.groupBox2.Controls.Add(this.tboutputGDB);
            this.groupBox2.Controls.Add(this.txtUpgradeGDB);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.btnSave);
            this.groupBox2.Location = new System.Drawing.Point(6, 28);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(571, 118);
            this.groupBox2.TabIndex = 39;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "基础底图";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Maroon;
            this.label1.Location = new System.Drawing.Point(6, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 16;
            this.label1.Text = "数据下载路径：";
            // 
            // btSave1
            // 
            this.btSave1.Location = new System.Drawing.Point(503, 66);
            this.btSave1.Name = "btSave1";
            this.btSave1.Size = new System.Drawing.Size(61, 28);
            this.btSave1.TabIndex = 38;
            this.btSave1.Text = "选择";
            this.btSave1.UseVisualStyleBackColor = true;
            this.btSave1.Click += new System.EventHandler(this.btSave1_Click);
            // 
            // tboutputGDB
            // 
            this.tboutputGDB.Location = new System.Drawing.Point(95, 31);
            this.tboutputGDB.Name = "tboutputGDB";
            this.tboutputGDB.ReadOnly = true;
            this.tboutputGDB.Size = new System.Drawing.Size(387, 21);
            this.tboutputGDB.TabIndex = 17;
            // 
            // txtUpgradeGDB
            // 
            this.txtUpgradeGDB.Location = new System.Drawing.Point(95, 68);
            this.txtUpgradeGDB.Name = "txtUpgradeGDB";
            this.txtUpgradeGDB.ReadOnly = true;
            this.txtUpgradeGDB.Size = new System.Drawing.Size(387, 21);
            this.txtUpgradeGDB.TabIndex = 37;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Maroon;
            this.label3.Location = new System.Drawing.Point(-2, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 36;
            this.label3.Text = "数据符号化路径：";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(504, 26);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(61, 28);
            this.btnSave.TabIndex = 18;
            this.btnSave.Text = "选择";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cbAttach);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.cbBaseMapTemplate);
            this.groupBox5.Location = new System.Drawing.Point(7, 239);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(570, 67);
            this.groupBox5.TabIndex = 35;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "地图符号化";
            // 
            // cbAttach
            // 
            this.cbAttach.AutoSize = true;
            this.cbAttach.Checked = true;
            this.cbAttach.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAttach.Location = new System.Drawing.Point(93, 30);
            this.cbAttach.Name = "cbAttach";
            this.cbAttach.Size = new System.Drawing.Size(48, 16);
            this.cbAttach.TabIndex = 84;
            this.cbAttach.Text = "附区";
            this.cbAttach.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(287, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 28;
            this.label2.Text = "符号模板：";
            // 
            // cbBaseMapTemplate
            // 
            this.cbBaseMapTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBaseMapTemplate.FormattingEnabled = true;
            this.cbBaseMapTemplate.Location = new System.Drawing.Point(358, 28);
            this.cbBaseMapTemplate.Name = "cbBaseMapTemplate";
            this.cbBaseMapTemplate.Size = new System.Drawing.Size(150, 20);
            this.cbBaseMapTemplate.TabIndex = 29;
            this.cbBaseMapTemplate.SelectedIndexChanged += new System.EventHandler(this.cbBaseMapTemplate_SelectedIndexChanged);
            // 
            // PageMapProcess
            // 
            this.PageMapProcess.Controls.Add(this.groupBox4);
            this.PageMapProcess.DescriptionText = "完成地图注记生成、河流渐变、境界跳绘、点符号冲突处理等地图要素自动处理。";
            this.PageMapProcess.Name = "PageMapProcess";
            this.PageMapProcess.Size = new System.Drawing.Size(583, 324);
            this.PageMapProcess.Text = "地图处理";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.JustifyCulvertCmd);
            this.groupBox4.Controls.Add(this.button9);
            this.groupBox4.Controls.Add(this.button10);
            this.groupBox4.Controls.Add(this.HydlMaskProcessCmd);
            this.groupBox4.Controls.Add(this.btBOULSkip);
            this.groupBox4.Controls.Add(this.BoulSkipDrawCmd);
            this.groupBox4.Controls.Add(this.button3);
            this.groupBox4.Controls.Add(this.RiverBoundayExtractCmd);
            this.groupBox4.Controls.Add(this.SymbolProcessCmd);
            this.groupBox4.Controls.Add(this.button4);
            this.groupBox4.Controls.Add(this.button5);
            this.groupBox4.Controls.Add(this.DirectionPointAdjustmentCmd);
            this.groupBox4.Controls.Add(this.btZoneColorCmd);
            this.groupBox4.Controls.Add(this.ZoneColorCmd);
            this.groupBox4.Controls.Add(this.btAnno);
            this.groupBox4.Controls.Add(this.MaplexAnnotateCmd);
            this.groupBox4.Controls.Add(this.RoadSymEndsCmd);
            this.groupBox4.Controls.Add(this.button12);
            this.groupBox4.Controls.Add(this.checkBox12);
            this.groupBox4.Controls.Add(this.button7);
            this.groupBox4.Controls.Add(this.RiverAutoGradualCmd);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(583, 324);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "地图处理";
            // 
            // JustifyCulvertCmd
            // 
            this.JustifyCulvertCmd.AutoSize = true;
            this.JustifyCulvertCmd.Location = new System.Drawing.Point(33, 262);
            this.JustifyCulvertCmd.Name = "JustifyCulvertCmd";
            this.JustifyCulvertCmd.Size = new System.Drawing.Size(15, 14);
            this.JustifyCulvertCmd.TabIndex = 34;
            this.JustifyCulvertCmd.UseVisualStyleBackColor = true;
            this.JustifyCulvertCmd.Visible = false;
            // 
            // button9
            // 
            this.button9.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button9.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button9.ImageIndex = 2;
            this.button9.ImageList = this.imageList1;
            this.button9.Location = new System.Drawing.Point(66, 247);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(119, 42);
            this.button9.TabIndex = 33;
            this.button9.Text = "涵洞符号调整";
            this.button9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Visible = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "btAnno.Image.png");
            this.imageList1.Images.SetKeyName(1, "btSDM.Image.png");
            this.imageList1.Images.SetKeyName(2, "GB一致性检查.png");
            // 
            // button10
            // 
            this.button10.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button10.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button10.ImageIndex = 2;
            this.button10.ImageList = this.imageList1;
            this.button10.Location = new System.Drawing.Point(251, 247);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(119, 42);
            this.button10.TabIndex = 32;
            this.button10.Text = "水系线消隐";
            this.button10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Visible = false;
            // 
            // HydlMaskProcessCmd
            // 
            this.HydlMaskProcessCmd.AutoSize = true;
            this.HydlMaskProcessCmd.Location = new System.Drawing.Point(218, 262);
            this.HydlMaskProcessCmd.Name = "HydlMaskProcessCmd";
            this.HydlMaskProcessCmd.Size = new System.Drawing.Size(15, 14);
            this.HydlMaskProcessCmd.TabIndex = 31;
            this.HydlMaskProcessCmd.UseVisualStyleBackColor = true;
            this.HydlMaskProcessCmd.Visible = false;
            // 
            // btBOULSkip
            // 
            this.btBOULSkip.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btBOULSkip.ForeColor = System.Drawing.Color.Black;
            this.btBOULSkip.Image = ((System.Drawing.Image)(resources.GetObject("btBOULSkip.Image")));
            this.btBOULSkip.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btBOULSkip.Location = new System.Drawing.Point(66, 183);
            this.btBOULSkip.Name = "btBOULSkip";
            this.btBOULSkip.Size = new System.Drawing.Size(119, 42);
            this.btBOULSkip.TabIndex = 30;
            this.btBOULSkip.Text = "境界跳绘";
            this.btBOULSkip.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btBOULSkip.UseVisualStyleBackColor = true;
            this.btBOULSkip.Click += new System.EventHandler(this.btBOULSkip_Click);
            // 
            // BoulSkipDrawCmd
            // 
            this.BoulSkipDrawCmd.AutoSize = true;
            this.BoulSkipDrawCmd.Checked = true;
            this.BoulSkipDrawCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.BoulSkipDrawCmd.Location = new System.Drawing.Point(33, 198);
            this.BoulSkipDrawCmd.Name = "BoulSkipDrawCmd";
            this.BoulSkipDrawCmd.Size = new System.Drawing.Size(15, 14);
            this.BoulSkipDrawCmd.TabIndex = 29;
            this.BoulSkipDrawCmd.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button3.ImageKey = "GB一致性检查.png";
            this.button3.ImageList = this.imageList1;
            this.button3.Location = new System.Drawing.Point(243, 119);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(127, 42);
            this.button3.TabIndex = 28;
            this.button3.Text = "水系面边线提取";
            this.button3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button3.UseVisualStyleBackColor = true;
            // 
            // RiverBoundayExtractCmd
            // 
            this.RiverBoundayExtractCmd.AutoSize = true;
            this.RiverBoundayExtractCmd.Checked = true;
            this.RiverBoundayExtractCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RiverBoundayExtractCmd.Location = new System.Drawing.Point(210, 134);
            this.RiverBoundayExtractCmd.Name = "RiverBoundayExtractCmd";
            this.RiverBoundayExtractCmd.Size = new System.Drawing.Size(15, 14);
            this.RiverBoundayExtractCmd.TabIndex = 27;
            this.RiverBoundayExtractCmd.UseVisualStyleBackColor = true;
            // 
            // SymbolProcessCmd
            // 
            this.SymbolProcessCmd.AutoSize = true;
            this.SymbolProcessCmd.Checked = true;
            this.SymbolProcessCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SymbolProcessCmd.Location = new System.Drawing.Point(400, 71);
            this.SymbolProcessCmd.Name = "SymbolProcessCmd";
            this.SymbolProcessCmd.Size = new System.Drawing.Size(15, 14);
            this.SymbolProcessCmd.TabIndex = 26;
            this.SymbolProcessCmd.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button4.ImageIndex = 2;
            this.button4.ImageList = this.imageList1;
            this.button4.Location = new System.Drawing.Point(433, 56);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(119, 42);
            this.button4.TabIndex = 25;
            this.button4.Text = "地名冲突处理";
            this.button4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button5.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button5.ImageIndex = 2;
            this.button5.ImageList = this.imageList1;
            this.button5.Location = new System.Drawing.Point(243, 56);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(127, 42);
            this.button5.TabIndex = 24;
            this.button5.Text = "调整有向点方向";
            this.button5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button5.UseVisualStyleBackColor = true;
            // 
            // DirectionPointAdjustmentCmd
            // 
            this.DirectionPointAdjustmentCmd.AutoSize = true;
            this.DirectionPointAdjustmentCmd.Checked = true;
            this.DirectionPointAdjustmentCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DirectionPointAdjustmentCmd.Location = new System.Drawing.Point(210, 71);
            this.DirectionPointAdjustmentCmd.Name = "DirectionPointAdjustmentCmd";
            this.DirectionPointAdjustmentCmd.Size = new System.Drawing.Size(15, 14);
            this.DirectionPointAdjustmentCmd.TabIndex = 23;
            this.DirectionPointAdjustmentCmd.UseVisualStyleBackColor = true;
            // 
            // btZoneColorCmd
            // 
            this.btZoneColorCmd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btZoneColorCmd.ForeColor = System.Drawing.Color.Black;
            this.btZoneColorCmd.Image = ((System.Drawing.Image)(resources.GetObject("btZoneColorCmd.Image")));
            this.btZoneColorCmd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btZoneColorCmd.Location = new System.Drawing.Point(66, 120);
            this.btZoneColorCmd.Name = "btZoneColorCmd";
            this.btZoneColorCmd.Size = new System.Drawing.Size(119, 42);
            this.btZoneColorCmd.TabIndex = 22;
            this.btZoneColorCmd.Text = "境界普色";
            this.btZoneColorCmd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btZoneColorCmd.UseVisualStyleBackColor = true;
            this.btZoneColorCmd.Click += new System.EventHandler(this.btZoneColorCmd_Click);
            // 
            // ZoneColorCmd
            // 
            this.ZoneColorCmd.AutoSize = true;
            this.ZoneColorCmd.Checked = true;
            this.ZoneColorCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ZoneColorCmd.Location = new System.Drawing.Point(33, 135);
            this.ZoneColorCmd.Name = "ZoneColorCmd";
            this.ZoneColorCmd.Size = new System.Drawing.Size(15, 14);
            this.ZoneColorCmd.TabIndex = 21;
            this.ZoneColorCmd.UseVisualStyleBackColor = true;
            // 
            // btAnno
            // 
            this.btAnno.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btAnno.ForeColor = System.Drawing.Color.Black;
            this.btAnno.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btAnno.ImageIndex = 1;
            this.btAnno.ImageList = this.imageList1;
            this.btAnno.Location = new System.Drawing.Point(66, 56);
            this.btAnno.Name = "btAnno";
            this.btAnno.Size = new System.Drawing.Size(119, 42);
            this.btAnno.TabIndex = 20;
            this.btAnno.Text = "注记生成";
            this.btAnno.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btAnno.UseVisualStyleBackColor = true;
            this.btAnno.Click += new System.EventHandler(this.btAnno_Click);
            // 
            // MaplexAnnotateCmd
            // 
            this.MaplexAnnotateCmd.AutoSize = true;
            this.MaplexAnnotateCmd.Checked = true;
            this.MaplexAnnotateCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MaplexAnnotateCmd.Location = new System.Drawing.Point(33, 71);
            this.MaplexAnnotateCmd.Name = "MaplexAnnotateCmd";
            this.MaplexAnnotateCmd.Size = new System.Drawing.Size(15, 14);
            this.MaplexAnnotateCmd.TabIndex = 19;
            this.MaplexAnnotateCmd.UseVisualStyleBackColor = true;
            // 
            // RoadSymEndsCmd
            // 
            this.RoadSymEndsCmd.AutoSize = true;
            this.RoadSymEndsCmd.Checked = true;
            this.RoadSymEndsCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RoadSymEndsCmd.Location = new System.Drawing.Point(400, 135);
            this.RoadSymEndsCmd.Name = "RoadSymEndsCmd";
            this.RoadSymEndsCmd.Size = new System.Drawing.Size(15, 14);
            this.RoadSymEndsCmd.TabIndex = 14;
            this.RoadSymEndsCmd.UseVisualStyleBackColor = true;
            // 
            // button12
            // 
            this.button12.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button12.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button12.ImageIndex = 2;
            this.button12.ImageList = this.imageList1;
            this.button12.Location = new System.Drawing.Point(433, 120);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(119, 42);
            this.button12.TabIndex = 13;
            this.button12.Text = "端头路处理";
            this.button12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button12.UseVisualStyleBackColor = true;
            // 
            // checkBox12
            // 
            this.checkBox12.AutoSize = true;
            this.checkBox12.Location = new System.Drawing.Point(7, 335);
            this.checkBox12.Name = "checkBox12";
            this.checkBox12.Size = new System.Drawing.Size(15, 14);
            this.checkBox12.TabIndex = 12;
            this.checkBox12.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            this.button7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button7.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button7.ImageIndex = 2;
            this.button7.ImageList = this.imageList1;
            this.button7.Location = new System.Drawing.Point(433, 247);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(119, 42);
            this.button7.TabIndex = 3;
            this.button7.Text = "河流渐变";
            this.button7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Visible = false;
            // 
            // RiverAutoGradualCmd
            // 
            this.RiverAutoGradualCmd.AutoSize = true;
            this.RiverAutoGradualCmd.Location = new System.Drawing.Point(400, 262);
            this.RiverAutoGradualCmd.Name = "RiverAutoGradualCmd";
            this.RiverAutoGradualCmd.Size = new System.Drawing.Size(15, 14);
            this.RiverAutoGradualCmd.TabIndex = 2;
            this.RiverAutoGradualCmd.UseVisualStyleBackColor = true;
            this.RiverAutoGradualCmd.Visible = false;
            // 
            // PageMapDes
            // 
            this.PageMapDes.Controls.Add(this.groupBox6);
            this.PageMapDes.DescriptionText = "完成地图图名、花边、图例、比例尺、境界普色、色带面等地图整饰要素的自动生成。";
            this.PageMapDes.Name = "PageMapDes";
            this.PageMapDes.Size = new System.Drawing.Size(583, 324);
            this.PageMapDes.Text = "地图整饰";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.NorthCmd);
            this.groupBox6.Controls.Add(this.btNorthCmd);
            this.groupBox6.Controls.Add(this.groupBox8);
            this.groupBox6.Controls.Add(this.btAddFigureMapCmdXJ);
            this.groupBox6.Controls.Add(this.AddFigureMapCmdXJ);
            this.groupBox6.Controls.Add(this.btScaleBarCmd);
            this.groupBox6.Controls.Add(this.ScaleBarCmd);
            this.groupBox6.Controls.Add(this.btLengendDyCreateCmd);
            this.groupBox6.Controls.Add(this.LengendDyCreateCmd);
            this.groupBox6.Controls.Add(this.btSDM);
            this.groupBox6.Controls.Add(this.SDMColorCmd);
            this.groupBox6.Controls.Add(this.btFootBorderCmd);
            this.groupBox6.Controls.Add(this.FootBorderCmd);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Location = new System.Drawing.Point(0, 0);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(583, 324);
            this.groupBox6.TabIndex = 7;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "地图整饰";
            // 
            // NorthCmd
            // 
            this.NorthCmd.AutoSize = true;
            this.NorthCmd.Checked = true;
            this.NorthCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.NorthCmd.Location = new System.Drawing.Point(89, 273);
            this.NorthCmd.Name = "NorthCmd";
            this.NorthCmd.Size = new System.Drawing.Size(15, 14);
            this.NorthCmd.TabIndex = 93;
            this.NorthCmd.UseVisualStyleBackColor = true;
            // 
            // btNorthCmd
            // 
            this.btNorthCmd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btNorthCmd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btNorthCmd.ImageIndex = 1;
            this.btNorthCmd.ImageList = this.imageList1;
            this.btNorthCmd.Location = new System.Drawing.Point(122, 258);
            this.btNorthCmd.Name = "btNorthCmd";
            this.btNorthCmd.Size = new System.Drawing.Size(107, 42);
            this.btNorthCmd.TabIndex = 92;
            this.btNorthCmd.Text = "指北针";
            this.btNorthCmd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btNorthCmd.UseVisualStyleBackColor = true;
            this.btNorthCmd.Click += new System.EventHandler(this.btNorthCmd_Click);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.btNameSet);
            this.groupBox8.Controls.Add(this.tbMapName);
            this.groupBox8.Controls.Add(this.tbProductFactory);
            this.groupBox8.Controls.Add(this.label9);
            this.groupBox8.Controls.Add(this.label10);
            this.groupBox8.ForeColor = System.Drawing.Color.Black;
            this.groupBox8.Location = new System.Drawing.Point(63, 20);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(404, 83);
            this.groupBox8.TabIndex = 91;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "图名设置";
            // 
            // btNameSet
            // 
            this.btNameSet.Location = new System.Drawing.Point(323, 20);
            this.btNameSet.Name = "btNameSet";
            this.btNameSet.Size = new System.Drawing.Size(75, 23);
            this.btNameSet.TabIndex = 91;
            this.btNameSet.Text = "高级设置";
            this.btNameSet.UseVisualStyleBackColor = true;
            this.btNameSet.Click += new System.EventHandler(this.button2_Click);
            // 
            // tbMapName
            // 
            this.tbMapName.Location = new System.Drawing.Point(72, 20);
            this.tbMapName.Name = "tbMapName";
            this.tbMapName.Size = new System.Drawing.Size(232, 21);
            this.tbMapName.TabIndex = 88;
            // 
            // tbProductFactory
            // 
            this.tbProductFactory.Location = new System.Drawing.Point(72, 56);
            this.tbProductFactory.Name = "tbProductFactory";
            this.tbProductFactory.Size = new System.Drawing.Size(232, 21);
            this.tbProductFactory.TabIndex = 90;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(31, 23);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(35, 12);
            this.label9.TabIndex = 87;
            this.label9.Text = "图名:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 59);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 12);
            this.label10.TabIndex = 89;
            this.label10.Text = "编制单位:";
            // 
            // btAddFigureMapCmdXJ
            // 
            this.btAddFigureMapCmdXJ.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btAddFigureMapCmdXJ.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btAddFigureMapCmdXJ.ImageIndex = 1;
            this.btAddFigureMapCmdXJ.ImageList = this.imageList1;
            this.btAddFigureMapCmdXJ.Location = new System.Drawing.Point(300, 258);
            this.btAddFigureMapCmdXJ.Name = "btAddFigureMapCmdXJ";
            this.btAddFigureMapCmdXJ.Size = new System.Drawing.Size(107, 42);
            this.btAddFigureMapCmdXJ.TabIndex = 14;
            this.btAddFigureMapCmdXJ.Text = "位置图";
            this.btAddFigureMapCmdXJ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btAddFigureMapCmdXJ.UseVisualStyleBackColor = true;
            this.btAddFigureMapCmdXJ.Click += new System.EventHandler(this.btAddFigureMapCmdXJ_Click);
            // 
            // AddFigureMapCmdXJ
            // 
            this.AddFigureMapCmdXJ.AutoSize = true;
            this.AddFigureMapCmdXJ.Checked = true;
            this.AddFigureMapCmdXJ.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AddFigureMapCmdXJ.Location = new System.Drawing.Point(267, 273);
            this.AddFigureMapCmdXJ.Name = "AddFigureMapCmdXJ";
            this.AddFigureMapCmdXJ.Size = new System.Drawing.Size(15, 14);
            this.AddFigureMapCmdXJ.TabIndex = 13;
            this.AddFigureMapCmdXJ.UseVisualStyleBackColor = true;
            // 
            // btScaleBarCmd
            // 
            this.btScaleBarCmd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btScaleBarCmd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btScaleBarCmd.ImageIndex = 1;
            this.btScaleBarCmd.ImageList = this.imageList1;
            this.btScaleBarCmd.Location = new System.Drawing.Point(300, 191);
            this.btScaleBarCmd.Name = "btScaleBarCmd";
            this.btScaleBarCmd.Size = new System.Drawing.Size(107, 42);
            this.btScaleBarCmd.TabIndex = 12;
            this.btScaleBarCmd.Text = "比例尺生成";
            this.btScaleBarCmd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btScaleBarCmd.UseVisualStyleBackColor = true;
            this.btScaleBarCmd.Click += new System.EventHandler(this.btScaleBarCmd_Click);
            // 
            // ScaleBarCmd
            // 
            this.ScaleBarCmd.AutoSize = true;
            this.ScaleBarCmd.Checked = true;
            this.ScaleBarCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScaleBarCmd.Location = new System.Drawing.Point(267, 206);
            this.ScaleBarCmd.Name = "ScaleBarCmd";
            this.ScaleBarCmd.Size = new System.Drawing.Size(15, 14);
            this.ScaleBarCmd.TabIndex = 11;
            this.ScaleBarCmd.UseVisualStyleBackColor = true;
            // 
            // btLengendDyCreateCmd
            // 
            this.btLengendDyCreateCmd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btLengendDyCreateCmd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btLengendDyCreateCmd.ImageIndex = 2;
            this.btLengendDyCreateCmd.ImageList = this.imageList1;
            this.btLengendDyCreateCmd.Location = new System.Drawing.Point(122, 191);
            this.btLengendDyCreateCmd.Name = "btLengendDyCreateCmd";
            this.btLengendDyCreateCmd.Size = new System.Drawing.Size(107, 42);
            this.btLengendDyCreateCmd.TabIndex = 10;
            this.btLengendDyCreateCmd.Text = "图例生成";
            this.btLengendDyCreateCmd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btLengendDyCreateCmd.UseVisualStyleBackColor = true;
            // 
            // LengendDyCreateCmd
            // 
            this.LengendDyCreateCmd.AutoSize = true;
            this.LengendDyCreateCmd.Checked = true;
            this.LengendDyCreateCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LengendDyCreateCmd.Location = new System.Drawing.Point(89, 206);
            this.LengendDyCreateCmd.Name = "LengendDyCreateCmd";
            this.LengendDyCreateCmd.Size = new System.Drawing.Size(15, 14);
            this.LengendDyCreateCmd.TabIndex = 9;
            this.LengendDyCreateCmd.UseVisualStyleBackColor = true;
            // 
            // btSDM
            // 
            this.btSDM.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btSDM.ForeColor = System.Drawing.Color.Black;
            this.btSDM.Image = ((System.Drawing.Image)(resources.GetObject("btSDM.Image")));
            this.btSDM.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btSDM.Location = new System.Drawing.Point(300, 122);
            this.btSDM.Name = "btSDM";
            this.btSDM.Size = new System.Drawing.Size(107, 42);
            this.btSDM.TabIndex = 5;
            this.btSDM.Text = "色带面";
            this.btSDM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btSDM.UseVisualStyleBackColor = true;
            this.btSDM.Click += new System.EventHandler(this.btSDM_Click);
            // 
            // SDMColorCmd
            // 
            this.SDMColorCmd.AutoSize = true;
            this.SDMColorCmd.Checked = true;
            this.SDMColorCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SDMColorCmd.Location = new System.Drawing.Point(267, 137);
            this.SDMColorCmd.Name = "SDMColorCmd";
            this.SDMColorCmd.Size = new System.Drawing.Size(15, 14);
            this.SDMColorCmd.TabIndex = 4;
            this.SDMColorCmd.UseVisualStyleBackColor = true;
            // 
            // btFootBorderCmd
            // 
            this.btFootBorderCmd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btFootBorderCmd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btFootBorderCmd.ImageIndex = 1;
            this.btFootBorderCmd.ImageList = this.imageList1;
            this.btFootBorderCmd.Location = new System.Drawing.Point(122, 122);
            this.btFootBorderCmd.Name = "btFootBorderCmd";
            this.btFootBorderCmd.Size = new System.Drawing.Size(107, 42);
            this.btFootBorderCmd.TabIndex = 3;
            this.btFootBorderCmd.Text = "花边生成";
            this.btFootBorderCmd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btFootBorderCmd.UseVisualStyleBackColor = true;
            this.btFootBorderCmd.Click += new System.EventHandler(this.btFootBorderCmd_Click);
            // 
            // FootBorderCmd
            // 
            this.FootBorderCmd.AutoSize = true;
            this.FootBorderCmd.Checked = true;
            this.FootBorderCmd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.FootBorderCmd.Location = new System.Drawing.Point(89, 137);
            this.FootBorderCmd.Name = "FootBorderCmd";
            this.FootBorderCmd.Size = new System.Drawing.Size(15, 14);
            this.FootBorderCmd.TabIndex = 2;
            this.FootBorderCmd.UseVisualStyleBackColor = true;
            // 
            // PageMapExport
            // 
            this.PageMapExport.Controls.Add(this.groupBox7);
            this.PageMapExport.DescriptionText = "完成将地图输出为PDF。";
            this.PageMapExport.Name = "PageMapExport";
            this.PageMapExport.Size = new System.Drawing.Size(583, 324);
            this.PageMapExport.Text = "地图输出";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.GBColorMode);
            this.groupBox7.Controls.Add(this.nudResolution);
            this.groupBox7.Controls.Add(this.label5);
            this.groupBox7.Controls.Add(this.label6);
            this.groupBox7.Controls.Add(this.btnOutputFile);
            this.groupBox7.Controls.Add(this.txtFileName);
            this.groupBox7.Controls.Add(this.label4);
            this.groupBox7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox7.Location = new System.Drawing.Point(0, 0);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(583, 324);
            this.groupBox7.TabIndex = 0;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "地图输出";
            // 
            // GBColorMode
            // 
            this.GBColorMode.Controls.Add(this.rbCMYK);
            this.GBColorMode.Controls.Add(this.rbRGB);
            this.GBColorMode.Location = new System.Drawing.Point(58, 180);
            this.GBColorMode.Name = "GBColorMode";
            this.GBColorMode.Size = new System.Drawing.Size(364, 52);
            this.GBColorMode.TabIndex = 98;
            this.GBColorMode.TabStop = false;
            this.GBColorMode.Text = "颜色模式";
            // 
            // rbCMYK
            // 
            this.rbCMYK.AutoSize = true;
            this.rbCMYK.Checked = true;
            this.rbCMYK.Location = new System.Drawing.Point(222, 23);
            this.rbCMYK.Name = "rbCMYK";
            this.rbCMYK.Size = new System.Drawing.Size(47, 16);
            this.rbCMYK.TabIndex = 1;
            this.rbCMYK.TabStop = true;
            this.rbCMYK.Text = "CMYK";
            this.rbCMYK.UseVisualStyleBackColor = true;
            // 
            // rbRGB
            // 
            this.rbRGB.AutoSize = true;
            this.rbRGB.Location = new System.Drawing.Point(69, 23);
            this.rbRGB.Name = "rbRGB";
            this.rbRGB.Size = new System.Drawing.Size(41, 16);
            this.rbRGB.TabIndex = 0;
            this.rbRGB.Text = "RGB";
            this.rbRGB.UseVisualStyleBackColor = true;
            // 
            // nudResolution
            // 
            this.nudResolution.Location = new System.Drawing.Point(125, 128);
            this.nudResolution.Maximum = new decimal(new int[] {
            1200,
            0,
            0,
            0});
            this.nudResolution.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudResolution.Name = "nudResolution";
            this.nudResolution.Size = new System.Drawing.Size(103, 21);
            this.nudResolution.TabIndex = 97;
            this.nudResolution.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(234, 132);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 12);
            this.label5.TabIndex = 96;
            this.label5.Text = "dpi";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(56, 132);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 12);
            this.label6.TabIndex = 95;
            this.label6.Text = "分辨率:";
            // 
            // btnOutputFile
            // 
            this.btnOutputFile.Location = new System.Drawing.Point(449, 68);
            this.btnOutputFile.Margin = new System.Windows.Forms.Padding(2);
            this.btnOutputFile.Name = "btnOutputFile";
            this.btnOutputFile.Size = new System.Drawing.Size(66, 28);
            this.btnOutputFile.TabIndex = 94;
            this.btnOutputFile.Text = "浏览";
            this.btnOutputFile.UseVisualStyleBackColor = true;
            this.btnOutputFile.Click += new System.EventHandler(this.btnOutputFile_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(125, 75);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.ReadOnly = true;
            this.txtFileName.Size = new System.Drawing.Size(297, 21);
            this.txtFileName.TabIndex = 93;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Maroon;
            this.label4.Location = new System.Drawing.Point(44, 78);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 92;
            this.label4.Text = "输出文件:";
            // 
            // timer
            // 
            this.timer.Interval = 300;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // FrmAutoMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 469);
            this.Controls.Add(this.wizardAutoMap);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmAutoMap";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "一键成图";
            this.Load += new System.EventHandler(this.FrmAutoMap_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wizardAutoMap)).EndInit();
            this.wizardAutoMap.ResumeLayout(false);
            this.PageDataDownload.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.PageMapProcess.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.PageMapDes.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.PageMapExport.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.GBColorMode.ResumeLayout(false);
            this.GBColorMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudResolution)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraWizard.WizardControl wizardAutoMap;
        private DevExpress.XtraWizard.WizardPage PageDataDownload;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox tboutputGDB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbBaseMapTemplate;
        private System.Windows.Forms.CheckBox cbAttach;
        private DevExpress.XtraWizard.WizardPage PageMapProcess;
        private DevExpress.XtraWizard.WizardPage PageMapDes;
        private DevExpress.XtraWizard.WizardPage PageMapExport;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.CheckBox checkBox12;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button btScaleBarCmd;
        private System.Windows.Forms.Button btLengendDyCreateCmd;
        private System.Windows.Forms.Button btSDM;
        private System.Windows.Forms.Button btFootBorderCmd;
        private System.Windows.Forms.Button btAddFigureMapCmdXJ;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.GroupBox GBColorMode;
        private System.Windows.Forms.RadioButton rbCMYK;
        private System.Windows.Forms.RadioButton rbRGB;
        private System.Windows.Forms.NumericUpDown nudResolution;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnOutputFile;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.TextBox tbMapName;
        private System.Windows.Forms.TextBox tbProductFactory;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Timer timer;
        public System.Windows.Forms.GroupBox groupBox4;
        public System.Windows.Forms.CheckBox RoadSymEndsCmd;
        public System.Windows.Forms.CheckBox RiverAutoGradualCmd;
        public System.Windows.Forms.GroupBox groupBox6;
        public System.Windows.Forms.CheckBox ScaleBarCmd;
        public System.Windows.Forms.CheckBox LengendDyCreateCmd;
        public System.Windows.Forms.CheckBox SDMColorCmd;
        public System.Windows.Forms.CheckBox FootBorderCmd;
        public System.Windows.Forms.CheckBox AddFigureMapCmdXJ;
        private System.Windows.Forms.Button btSave1;
        private System.Windows.Forms.TextBox txtUpgradeGDB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btNameSet;
        private System.Windows.Forms.Button btAnno;
        public System.Windows.Forms.CheckBox MaplexAnnotateCmd;
        public System.Windows.Forms.CheckBox ZoneColorCmd;
        public System.Windows.Forms.Button btZoneColorCmd;
        public System.Windows.Forms.CheckBox NorthCmd;
        private System.Windows.Forms.Button btNorthCmd;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox cbThematic;
        private System.Windows.Forms.ComboBox cmbThematicType;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.Button btBOULSkip;
        public System.Windows.Forms.CheckBox BoulSkipDrawCmd;
        private System.Windows.Forms.Button button3;
        public System.Windows.Forms.CheckBox RiverBoundayExtractCmd;
        public System.Windows.Forms.CheckBox SymbolProcessCmd;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        public System.Windows.Forms.CheckBox DirectionPointAdjustmentCmd;
        private System.Windows.Forms.ImageList imageList1;
        public System.Windows.Forms.CheckBox JustifyCulvertCmd;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        public System.Windows.Forms.CheckBox HydlMaskProcessCmd;

    }
}
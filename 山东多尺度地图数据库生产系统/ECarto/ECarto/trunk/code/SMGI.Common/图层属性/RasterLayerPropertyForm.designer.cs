namespace SMGI.Common
{
    partial class RasterLayerPropertyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RasterLayerPropertyForm));
            this.PropertytabControl = new System.Windows.Forms.TabControl();
            this.SymbolPage = new System.Windows.Forms.TabPage();
            this.Renderertab = new System.Windows.Forms.TabControl();
            this.StretchPage = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.symbologyControl = new ESRI.ArcGIS.Controls.AxSymbologyControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbStretchInvert = new System.Windows.Forms.CheckBox();
            this.cmbStretchStretchType = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cbStretchBackground = new System.Windows.Forms.CheckBox();
            this.btnStretchNoDataColor = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.btnStretchBGColor = new System.Windows.Forms.Button();
            this.tbStretchColorG = new System.Windows.Forms.TextBox();
            this.tbMaxValue = new System.Windows.Forms.TextBox();
            this.tbMinValue = new System.Windows.Forms.TextBox();
            this.Bandpanel = new System.Windows.Forms.Panel();
            this.cmbBand = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnLabel = new System.Windows.Forms.Button();
            this.tbMinValueLabel = new System.Windows.Forms.TextBox();
            this.tbMediumValueLabel = new System.Windows.Forms.TextBox();
            this.tbMaxValueLabel = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ColorRampPicBox = new System.Windows.Forms.PictureBox();
            this.RGBPage = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbRGBInvert = new System.Windows.Forms.CheckBox();
            this.cmbRGBStretchType = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.btnRGBNoDataColor = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btnRGBBGColor = new System.Windows.Forms.Button();
            this.tbRGBColorB = new System.Windows.Forms.TextBox();
            this.tbRGBColorG = new System.Windows.Forms.TextBox();
            this.tbRGBColorR = new System.Windows.Forms.TextBox();
            this.cbRGBBackground = new System.Windows.Forms.CheckBox();
            this.ColorDataGridView = new System.Windows.Forms.DataGridView();
            this.DisplayStateColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ChannelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BandColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.RendererTypeListBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.DisplayPage = new System.Windows.Forms.TabPage();
            this.tbTrans = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.tbBrightness = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.tbContrastRatio = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnApp = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.cmbColorRamp = new SMGI.Common.ColorRampComboBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.PropertytabControl.SuspendLayout();
            this.SymbolPage.SuspendLayout();
            this.Renderertab.SuspendLayout();
            this.StretchPage.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.symbologyControl)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.Bandpanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorRampPicBox)).BeginInit();
            this.RGBPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorDataGridView)).BeginInit();
            this.DisplayPage.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PropertytabControl
            // 
            this.PropertytabControl.Controls.Add(this.SymbolPage);
            this.PropertytabControl.Controls.Add(this.DisplayPage);
            this.PropertytabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertytabControl.Location = new System.Drawing.Point(0, 0);
            this.PropertytabControl.Name = "PropertytabControl";
            this.PropertytabControl.SelectedIndex = 0;
            this.PropertytabControl.Size = new System.Drawing.Size(547, 453);
            this.PropertytabControl.TabIndex = 0;
            // 
            // SymbolPage
            // 
            this.SymbolPage.Controls.Add(this.Renderertab);
            this.SymbolPage.Controls.Add(this.RendererTypeListBox);
            this.SymbolPage.Controls.Add(this.label2);
            this.SymbolPage.Location = new System.Drawing.Point(4, 22);
            this.SymbolPage.Name = "SymbolPage";
            this.SymbolPage.Padding = new System.Windows.Forms.Padding(3);
            this.SymbolPage.Size = new System.Drawing.Size(539, 427);
            this.SymbolPage.TabIndex = 0;
            this.SymbolPage.Text = "符号系统";
            this.SymbolPage.UseVisualStyleBackColor = true;
            // 
            // Renderertab
            // 
            this.Renderertab.Controls.Add(this.StretchPage);
            this.Renderertab.Controls.Add(this.RGBPage);
            this.Renderertab.Location = new System.Drawing.Point(152, 2);
            this.Renderertab.Name = "Renderertab";
            this.Renderertab.SelectedIndex = 0;
            this.Renderertab.Size = new System.Drawing.Size(383, 385);
            this.Renderertab.TabIndex = 123;
            this.Renderertab.SelectedIndexChanged += new System.EventHandler(this.Renderertab_SelectedIndexChanged);
            // 
            // StretchPage
            // 
            this.StretchPage.Controls.Add(this.panel3);
            this.StretchPage.Location = new System.Drawing.Point(4, 22);
            this.StretchPage.Name = "StretchPage";
            this.StretchPage.Padding = new System.Windows.Forms.Padding(3);
            this.StretchPage.Size = new System.Drawing.Size(375, 359);
            this.StretchPage.TabIndex = 0;
            this.StretchPage.Text = "拉伸";
            this.StretchPage.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.AutoScroll = true;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.symbologyControl);
            this.panel3.Controls.Add(this.groupBox1);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Controls.Add(this.cbStretchBackground);
            this.panel3.Controls.Add(this.btnStretchNoDataColor);
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.btnStretchBGColor);
            this.panel3.Controls.Add(this.tbStretchColorG);
            this.panel3.Controls.Add(this.cmbColorRamp);
            this.panel3.Controls.Add(this.tbMaxValue);
            this.panel3.Controls.Add(this.tbMinValue);
            this.panel3.Controls.Add(this.Bandpanel);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.btnLabel);
            this.panel3.Controls.Add(this.tbMinValueLabel);
            this.panel3.Controls.Add(this.tbMediumValueLabel);
            this.panel3.Controls.Add(this.tbMaxValueLabel);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.ColorRampPicBox);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(369, 353);
            this.panel3.TabIndex = 124;
            // 
            // symbologyControl
            // 
            this.symbologyControl.Location = new System.Drawing.Point(142, 156);
            this.symbologyControl.Name = "symbologyControl";
            this.symbologyControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("symbologyControl.OcxState")));
            this.symbologyControl.Size = new System.Drawing.Size(141, 23);
            this.symbologyControl.TabIndex = 147;
            this.symbologyControl.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbStretchInvert);
            this.groupBox1.Controls.Add(this.cmbStretchStretchType);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Location = new System.Drawing.Point(6, 257);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(350, 82);
            this.groupBox1.TabIndex = 146;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "拉伸";
            // 
            // cbStretchInvert
            // 
            this.cbStretchInvert.AutoSize = true;
            this.cbStretchInvert.Location = new System.Drawing.Point(16, 52);
            this.cbStretchInvert.Name = "cbStretchInvert";
            this.cbStretchInvert.Size = new System.Drawing.Size(48, 16);
            this.cbStretchInvert.TabIndex = 133;
            this.cbStretchInvert.Text = "反向";
            this.cbStretchInvert.UseVisualStyleBackColor = true;
            this.cbStretchInvert.CheckedChanged += new System.EventHandler(this.cbStretchInvert_CheckedChanged);
            // 
            // cmbStretchStretchType
            // 
            this.cmbStretchStretchType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStretchStretchType.FormattingEnabled = true;
            this.cmbStretchStretchType.Items.AddRange(new object[] {
            "标准差"});
            this.cmbStretchStretchType.Location = new System.Drawing.Point(76, 17);
            this.cmbStretchStretchType.Name = "cmbStretchStretchType";
            this.cmbStretchStretchType.Size = new System.Drawing.Size(220, 20);
            this.cmbStretchStretchType.TabIndex = 130;
            this.cmbStretchStretchType.SelectedIndexChanged += new System.EventHandler(this.cmbStretchStretchType_SelectedIndexChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Cursor = System.Windows.Forms.Cursors.Default;
            this.label14.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label14.Location = new System.Drawing.Point(13, 19);
            this.label14.MaximumSize = new System.Drawing.Size(415, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(40, 13);
            this.label14.TabIndex = 129;
            this.label14.Text = "类型:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(266, 198);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 143;
            this.label7.Text = "为";
            // 
            // cbStretchBackground
            // 
            this.cbStretchBackground.AutoSize = true;
            this.cbStretchBackground.Location = new System.Drawing.Point(22, 197);
            this.cbStretchBackground.Name = "cbStretchBackground";
            this.cbStretchBackground.Size = new System.Drawing.Size(90, 16);
            this.cbStretchBackground.TabIndex = 140;
            this.cbStretchBackground.Text = "显示背景值:";
            this.cbStretchBackground.UseVisualStyleBackColor = true;
            this.cbStretchBackground.CheckedChanged += new System.EventHandler(this.cbStretchBackground_CheckedChanged);
            // 
            // btnStretchNoDataColor
            // 
            this.btnStretchNoDataColor.BackColor = System.Drawing.SystemColors.Control;
            this.btnStretchNoDataColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStretchNoDataColor.Location = new System.Drawing.Point(291, 228);
            this.btnStretchNoDataColor.Name = "btnStretchNoDataColor";
            this.btnStretchNoDataColor.Size = new System.Drawing.Size(52, 23);
            this.btnStretchNoDataColor.TabIndex = 144;
            this.btnStretchNoDataColor.UseVisualStyleBackColor = false;
            this.btnStretchNoDataColor.Click += new System.EventHandler(this.btnBackColor_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(196, 233);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 12);
            this.label8.TabIndex = 145;
            this.label8.Text = "将NoData显示为";
            // 
            // btnStretchBGColor
            // 
            this.btnStretchBGColor.BackColor = System.Drawing.SystemColors.Control;
            this.btnStretchBGColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStretchBGColor.Location = new System.Drawing.Point(291, 195);
            this.btnStretchBGColor.Name = "btnStretchBGColor";
            this.btnStretchBGColor.Size = new System.Drawing.Size(52, 23);
            this.btnStretchBGColor.TabIndex = 142;
            this.btnStretchBGColor.UseVisualStyleBackColor = false;
            this.btnStretchBGColor.Click += new System.EventHandler(this.btnBackColor_Click);
            // 
            // tbStretchColorG
            // 
            this.tbStretchColorG.Enabled = false;
            this.tbStretchColorG.Location = new System.Drawing.Point(162, 195);
            this.tbStretchColorG.Name = "tbStretchColorG";
            this.tbStretchColorG.Size = new System.Drawing.Size(42, 21);
            this.tbStretchColorG.TabIndex = 141;
            this.tbStretchColorG.Text = "0";
            this.tbStretchColorG.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tbMaxValue
            // 
            this.tbMaxValue.BackColor = System.Drawing.SystemColors.Window;
            this.tbMaxValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbMaxValue.Location = new System.Drawing.Point(109, 78);
            this.tbMaxValue.Name = "tbMaxValue";
            this.tbMaxValue.ReadOnly = true;
            this.tbMaxValue.Size = new System.Drawing.Size(70, 14);
            this.tbMaxValue.TabIndex = 138;
            this.tbMaxValue.Tag = "255";
            this.tbMaxValue.Text = "255";
            this.tbMaxValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tbMinValue
            // 
            this.tbMinValue.BackColor = System.Drawing.SystemColors.Window;
            this.tbMinValue.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbMinValue.Location = new System.Drawing.Point(109, 129);
            this.tbMinValue.Name = "tbMinValue";
            this.tbMinValue.ReadOnly = true;
            this.tbMinValue.Size = new System.Drawing.Size(70, 14);
            this.tbMinValue.TabIndex = 138;
            this.tbMinValue.Tag = "1";
            this.tbMinValue.Text = "1";
            this.tbMinValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Bandpanel
            // 
            this.Bandpanel.Controls.Add(this.cmbBand);
            this.Bandpanel.Controls.Add(this.label1);
            this.Bandpanel.Location = new System.Drawing.Point(4, 4);
            this.Bandpanel.Name = "Bandpanel";
            this.Bandpanel.Size = new System.Drawing.Size(352, 38);
            this.Bandpanel.TabIndex = 137;
            // 
            // cmbBand
            // 
            this.cmbBand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBand.FormattingEnabled = true;
            this.cmbBand.Location = new System.Drawing.Point(78, 9);
            this.cmbBand.Name = "cmbBand";
            this.cmbBand.Size = new System.Drawing.Size(261, 20);
            this.cmbBand.TabIndex = 130;
            this.cmbBand.SelectedIndexChanged += new System.EventHandler(this.cmbBand_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.label1.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(15, 11);
            this.label1.MaximumSize = new System.Drawing.Size(415, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 129;
            this.label1.Text = "波段:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Cursor = System.Windows.Forms.Cursors.Default;
            this.label4.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(19, 161);
            this.label4.MaximumSize = new System.Drawing.Size(415, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 129;
            this.label4.Text = "色带:";
            // 
            // btnLabel
            // 
            this.btnLabel.Location = new System.Drawing.Point(262, 50);
            this.btnLabel.Name = "btnLabel";
            this.btnLabel.Size = new System.Drawing.Size(81, 20);
            this.btnLabel.TabIndex = 136;
            this.btnLabel.Text = "标注";
            this.btnLabel.UseVisualStyleBackColor = true;
            this.btnLabel.Click += new System.EventHandler(this.btnLabel_Click);
            // 
            // tbMinValueLabel
            // 
            this.tbMinValueLabel.Location = new System.Drawing.Point(189, 129);
            this.tbMinValueLabel.Name = "tbMinValueLabel";
            this.tbMinValueLabel.Size = new System.Drawing.Size(154, 21);
            this.tbMinValueLabel.TabIndex = 135;
            this.tbMinValueLabel.Text = "低：1";
            // 
            // tbMediumValueLabel
            // 
            this.tbMediumValueLabel.Location = new System.Drawing.Point(189, 102);
            this.tbMediumValueLabel.Name = "tbMediumValueLabel";
            this.tbMediumValueLabel.Size = new System.Drawing.Size(154, 21);
            this.tbMediumValueLabel.TabIndex = 134;
            // 
            // tbMaxValueLabel
            // 
            this.tbMaxValueLabel.Location = new System.Drawing.Point(189, 75);
            this.tbMaxValueLabel.Name = "tbMaxValueLabel";
            this.tbMaxValueLabel.Size = new System.Drawing.Size(154, 21);
            this.tbMaxValueLabel.TabIndex = 133;
            this.tbMaxValueLabel.Text = "高：255";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Cursor = System.Windows.Forms.Cursors.Default;
            this.label6.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(186, 53);
            this.label6.MaximumSize = new System.Drawing.Size(415, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(33, 13);
            this.label6.TabIndex = 125;
            this.label6.Text = "标注";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Cursor = System.Windows.Forms.Cursors.Default;
            this.label5.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(159, 53);
            this.label5.MaximumSize = new System.Drawing.Size(415, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 13);
            this.label5.TabIndex = 125;
            this.label5.Text = "值";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Cursor = System.Windows.Forms.Cursors.Default;
            this.label3.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(19, 50);
            this.label3.MaximumSize = new System.Drawing.Size(415, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 125;
            this.label3.Text = "颜色";
            // 
            // ColorRampPicBox
            // 
            this.ColorRampPicBox.Location = new System.Drawing.Point(22, 66);
            this.ColorRampPicBox.Name = "ColorRampPicBox";
            this.ColorRampPicBox.Size = new System.Drawing.Size(22, 80);
            this.ColorRampPicBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ColorRampPicBox.TabIndex = 121;
            this.ColorRampPicBox.TabStop = false;
            // 
            // RGBPage
            // 
            this.RGBPage.Controls.Add(this.groupBox2);
            this.RGBPage.Controls.Add(this.btnRGBNoDataColor);
            this.RGBPage.Controls.Add(this.label9);
            this.RGBPage.Controls.Add(this.label10);
            this.RGBPage.Controls.Add(this.btnRGBBGColor);
            this.RGBPage.Controls.Add(this.tbRGBColorB);
            this.RGBPage.Controls.Add(this.tbRGBColorG);
            this.RGBPage.Controls.Add(this.tbRGBColorR);
            this.RGBPage.Controls.Add(this.cbRGBBackground);
            this.RGBPage.Controls.Add(this.ColorDataGridView);
            this.RGBPage.Location = new System.Drawing.Point(4, 22);
            this.RGBPage.Name = "RGBPage";
            this.RGBPage.Padding = new System.Windows.Forms.Padding(3);
            this.RGBPage.Size = new System.Drawing.Size(375, 359);
            this.RGBPage.TabIndex = 1;
            this.RGBPage.Text = "RGB合成";
            this.RGBPage.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbRGBInvert);
            this.groupBox2.Controls.Add(this.cmbRGBStretchType);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Location = new System.Drawing.Point(7, 212);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(354, 82);
            this.groupBox2.TabIndex = 147;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "拉伸";
            // 
            // cbRGBInvert
            // 
            this.cbRGBInvert.AutoSize = true;
            this.cbRGBInvert.Location = new System.Drawing.Point(16, 52);
            this.cbRGBInvert.Name = "cbRGBInvert";
            this.cbRGBInvert.Size = new System.Drawing.Size(48, 16);
            this.cbRGBInvert.TabIndex = 133;
            this.cbRGBInvert.Text = "反向";
            this.cbRGBInvert.UseVisualStyleBackColor = true;
            // 
            // cmbRGBStretchType
            // 
            this.cmbRGBStretchType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRGBStretchType.FormattingEnabled = true;
            this.cmbRGBStretchType.Items.AddRange(new object[] {
            "标准差"});
            this.cmbRGBStretchType.Location = new System.Drawing.Point(76, 17);
            this.cmbRGBStretchType.Name = "cmbRGBStretchType";
            this.cmbRGBStretchType.Size = new System.Drawing.Size(227, 20);
            this.cmbRGBStretchType.TabIndex = 130;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Cursor = System.Windows.Forms.Cursors.Default;
            this.label11.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.Location = new System.Drawing.Point(13, 19);
            this.label11.MaximumSize = new System.Drawing.Size(415, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 13);
            this.label11.TabIndex = 129;
            this.label11.Text = "类型:";
            // 
            // btnRGBNoDataColor
            // 
            this.btnRGBNoDataColor.BackColor = System.Drawing.SystemColors.Control;
            this.btnRGBNoDataColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRGBNoDataColor.Location = new System.Drawing.Point(309, 183);
            this.btnRGBNoDataColor.Name = "btnRGBNoDataColor";
            this.btnRGBNoDataColor.Size = new System.Drawing.Size(52, 23);
            this.btnRGBNoDataColor.TabIndex = 19;
            this.btnRGBNoDataColor.UseVisualStyleBackColor = false;
            this.btnRGBNoDataColor.Click += new System.EventHandler(this.btnBackColor_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(214, 188);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(89, 12);
            this.label9.TabIndex = 20;
            this.label9.Text = "将NoData显示为";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(286, 137);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(17, 12);
            this.label10.TabIndex = 18;
            this.label10.Text = "为";
            // 
            // btnRGBBGColor
            // 
            this.btnRGBBGColor.BackColor = System.Drawing.SystemColors.Control;
            this.btnRGBBGColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRGBBGColor.Location = new System.Drawing.Point(309, 133);
            this.btnRGBBGColor.Name = "btnRGBBGColor";
            this.btnRGBBGColor.Size = new System.Drawing.Size(52, 23);
            this.btnRGBBGColor.TabIndex = 17;
            this.btnRGBBGColor.UseVisualStyleBackColor = false;
            this.btnRGBBGColor.Click += new System.EventHandler(this.btnBackColor_Click);
            // 
            // tbRGBColorB
            // 
            this.tbRGBColorB.Enabled = false;
            this.tbRGBColorB.Location = new System.Drawing.Point(235, 134);
            this.tbRGBColorB.Name = "tbRGBColorB";
            this.tbRGBColorB.Size = new System.Drawing.Size(42, 21);
            this.tbRGBColorB.TabIndex = 16;
            this.tbRGBColorB.Text = "0";
            this.tbRGBColorB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tbRGBColorG
            // 
            this.tbRGBColorG.Enabled = false;
            this.tbRGBColorG.Location = new System.Drawing.Point(187, 133);
            this.tbRGBColorG.Name = "tbRGBColorG";
            this.tbRGBColorG.Size = new System.Drawing.Size(42, 21);
            this.tbRGBColorG.TabIndex = 14;
            this.tbRGBColorG.Text = "0";
            this.tbRGBColorG.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tbRGBColorR
            // 
            this.tbRGBColorR.Enabled = false;
            this.tbRGBColorR.Location = new System.Drawing.Point(139, 134);
            this.tbRGBColorR.Name = "tbRGBColorR";
            this.tbRGBColorR.Size = new System.Drawing.Size(42, 21);
            this.tbRGBColorR.TabIndex = 15;
            this.tbRGBColorR.Text = "0";
            this.tbRGBColorR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cbRGBBackground
            // 
            this.cbRGBBackground.AutoSize = true;
            this.cbRGBBackground.Location = new System.Drawing.Point(23, 137);
            this.cbRGBBackground.Name = "cbRGBBackground";
            this.cbRGBBackground.Size = new System.Drawing.Size(90, 16);
            this.cbRGBBackground.TabIndex = 13;
            this.cbRGBBackground.Text = "显示背景值:";
            this.cbRGBBackground.UseVisualStyleBackColor = true;
            this.cbRGBBackground.CheckedChanged += new System.EventHandler(this.cbRGBBackground_CheckedChanged);
            // 
            // ColorDataGridView
            // 
            this.ColorDataGridView.AllowUserToAddRows = false;
            this.ColorDataGridView.AllowUserToDeleteRows = false;
            this.ColorDataGridView.AllowUserToOrderColumns = true;
            this.ColorDataGridView.AllowUserToResizeColumns = false;
            this.ColorDataGridView.AllowUserToResizeRows = false;
            this.ColorDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ColorDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.ColorDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ColorDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DisplayStateColumn,
            this.ChannelColumn,
            this.BandColumn});
            this.ColorDataGridView.Location = new System.Drawing.Point(7, 7);
            this.ColorDataGridView.Name = "ColorDataGridView";
            this.ColorDataGridView.RowHeadersVisible = false;
            this.ColorDataGridView.RowTemplate.Height = 23;
            this.ColorDataGridView.Size = new System.Drawing.Size(354, 110);
            this.ColorDataGridView.TabIndex = 0;
            // 
            // DisplayStateColumn
            // 
            this.DisplayStateColumn.FillWeight = 12F;
            this.DisplayStateColumn.HeaderText = "状态";
            this.DisplayStateColumn.Name = "DisplayStateColumn";
            // 
            // ChannelColumn
            // 
            this.ChannelColumn.FillWeight = 30F;
            this.ChannelColumn.HeaderText = "通道";
            this.ChannelColumn.Name = "ChannelColumn";
            this.ChannelColumn.ReadOnly = true;
            this.ChannelColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // BandColumn
            // 
            this.BandColumn.FillWeight = 70F;
            this.BandColumn.HeaderText = "波段";
            this.BandColumn.Name = "BandColumn";
            // 
            // RendererTypeListBox
            // 
            this.RendererTypeListBox.FormattingEnabled = true;
            this.RendererTypeListBox.ItemHeight = 12;
            this.RendererTypeListBox.Location = new System.Drawing.Point(9, 23);
            this.RendererTypeListBox.Name = "RendererTypeListBox";
            this.RendererTypeListBox.Size = new System.Drawing.Size(136, 364);
            this.RendererTypeListBox.TabIndex = 122;
            this.RendererTypeListBox.SelectedIndexChanged += new System.EventHandler(this.RendererTypeListBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Cursor = System.Windows.Forms.Cursors.Default;
            this.label2.Font = new System.Drawing.Font("宋体", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(6, 6);
            this.label2.MaximumSize = new System.Drawing.Size(415, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 121;
            this.label2.Text = "显示:";
            // 
            // DisplayPage
            // 
            this.DisplayPage.Controls.Add(this.tbTrans);
            this.DisplayPage.Controls.Add(this.label18);
            this.DisplayPage.Controls.Add(this.tbBrightness);
            this.DisplayPage.Controls.Add(this.label16);
            this.DisplayPage.Controls.Add(this.label17);
            this.DisplayPage.Controls.Add(this.tbContrastRatio);
            this.DisplayPage.Controls.Add(this.label15);
            this.DisplayPage.Controls.Add(this.label13);
            this.DisplayPage.Controls.Add(this.label12);
            this.DisplayPage.Location = new System.Drawing.Point(4, 22);
            this.DisplayPage.Name = "DisplayPage";
            this.DisplayPage.Padding = new System.Windows.Forms.Padding(3);
            this.DisplayPage.Size = new System.Drawing.Size(539, 427);
            this.DisplayPage.TabIndex = 1;
            this.DisplayPage.Text = "显示";
            this.DisplayPage.UseVisualStyleBackColor = true;
            // 
            // tbTrans
            // 
            this.tbTrans.Location = new System.Drawing.Point(69, 79);
            this.tbTrans.Name = "tbTrans";
            this.tbTrans.Size = new System.Drawing.Size(100, 21);
            this.tbTrans.TabIndex = 1;
            this.tbTrans.Text = "0";
            this.tbTrans.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(171, 84);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(11, 12);
            this.label18.TabIndex = 0;
            this.label18.Text = "%";
            // 
            // tbBrightness
            // 
            this.tbBrightness.Location = new System.Drawing.Point(69, 43);
            this.tbBrightness.Name = "tbBrightness";
            this.tbBrightness.Size = new System.Drawing.Size(100, 21);
            this.tbBrightness.TabIndex = 1;
            this.tbBrightness.Text = "0";
            this.tbBrightness.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(171, 48);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(11, 12);
            this.label16.TabIndex = 0;
            this.label16.Text = "%";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(9, 82);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(53, 12);
            this.label17.TabIndex = 0;
            this.label17.Text = "透明度：";
            // 
            // tbContrastRatio
            // 
            this.tbContrastRatio.Location = new System.Drawing.Point(69, 7);
            this.tbContrastRatio.Name = "tbContrastRatio";
            this.tbContrastRatio.Size = new System.Drawing.Size(100, 21);
            this.tbContrastRatio.TabIndex = 1;
            this.tbContrastRatio.Text = "10";
            this.tbContrastRatio.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(9, 46);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(41, 12);
            this.label15.TabIndex = 0;
            this.label15.Text = "亮度：";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(171, 12);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(11, 12);
            this.label13.TabIndex = 0;
            this.label13.Text = "%";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(9, 10);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(53, 12);
            this.label12.TabIndex = 0;
            this.label12.Text = "对比度：";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel5);
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.btnApp);
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 417);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(547, 36);
            this.panel1.TabIndex = 4;
            // 
            // panel4
            // 
            this.panel4.Location = new System.Drawing.Point(455, 6);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(10, 26);
            this.panel4.TabIndex = 9;
            // 
            // btnApp
            // 
            this.btnApp.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnApp.Location = new System.Drawing.Point(467, 5);
            this.btnApp.Name = "btnApp";
            this.btnApp.Size = new System.Drawing.Size(75, 26);
            this.btnApp.TabIndex = 8;
            this.btnApp.Text = "应用";
            this.btnApp.UseVisualStyleBackColor = true;
            this.btnApp.Click += new System.EventHandler(this.btnApp_Click);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(282, 5);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 26);
            this.btOK.TabIndex = 7;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(511, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(10, 26);
            this.panel2.TabIndex = 9;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(375, 5);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 26);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // cmbColorRamp
            // 
            this.cmbColorRamp.BackColor = System.Drawing.SystemColors.Window;
            this.cmbColorRamp.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbColorRamp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColorRamp.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cmbColorRamp.FormattingEnabled = true;
            this.cmbColorRamp.Location = new System.Drawing.Point(82, 159);
            this.cmbColorRamp.Name = "cmbColorRamp";
            this.cmbColorRamp.Size = new System.Drawing.Size(261, 22);
            this.cmbColorRamp.TabIndex = 139;
            this.cmbColorRamp.SelectedIndexChanged += new System.EventHandler(this.cmbColorRamp_SelectedIndexChanged);
            // 
            // panel5
            // 
            this.panel5.Location = new System.Drawing.Point(363, 6);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(10, 26);
            this.panel5.TabIndex = 9;
            // 
            // RasterLayerPropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 453);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.PropertytabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "RasterLayerPropertyForm";
            this.Text = "图层属性";
            this.Load += new System.EventHandler(this.RasterLayerPropertyForm_Load);
            this.PropertytabControl.ResumeLayout(false);
            this.SymbolPage.ResumeLayout(false);
            this.SymbolPage.PerformLayout();
            this.Renderertab.ResumeLayout(false);
            this.StretchPage.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.symbologyControl)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.Bandpanel.ResumeLayout(false);
            this.Bandpanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorRampPicBox)).EndInit();
            this.RGBPage.ResumeLayout(false);
            this.RGBPage.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorDataGridView)).EndInit();
            this.DisplayPage.ResumeLayout(false);
            this.DisplayPage.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl PropertytabControl;
        private System.Windows.Forms.TabPage SymbolPage;
        private System.Windows.Forms.TabPage DisplayPage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl Renderertab;
        private System.Windows.Forms.TabPage StretchPage;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel Bandpanel;
        private System.Windows.Forms.ComboBox cmbBand;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnLabel;
        private System.Windows.Forms.TextBox tbMinValueLabel;
        private System.Windows.Forms.TextBox tbMediumValueLabel;
        private System.Windows.Forms.TextBox tbMaxValueLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox ColorRampPicBox;
        private System.Windows.Forms.TabPage RGBPage;
        private System.Windows.Forms.ListBox RendererTypeListBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox tbMaxValue;
        private System.Windows.Forms.TextBox tbMinValue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private SMGI.Common.ColorRampComboBox cmbColorRamp;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox cbStretchBackground;
        private System.Windows.Forms.Button btnStretchNoDataColor;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnStretchBGColor;
        private System.Windows.Forms.TextBox tbStretchColorG;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbStretchInvert;
        private System.Windows.Forms.ComboBox cmbStretchStretchType;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.DataGridView ColorDataGridView;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox cbRGBInvert;
        private System.Windows.Forms.ComboBox cmbRGBStretchType;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnRGBBGColor;
        private System.Windows.Forms.TextBox tbRGBColorB;
        private System.Windows.Forms.Button btnRGBNoDataColor;
        private System.Windows.Forms.TextBox tbRGBColorG;
        private System.Windows.Forms.TextBox tbRGBColorR;
        private System.Windows.Forms.CheckBox cbRGBBackground;
        private System.Windows.Forms.TextBox tbTrans;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox tbBrightness;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tbContrastRatio;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private ESRI.ArcGIS.Controls.AxSymbologyControl symbologyControl;
        private System.Windows.Forms.DataGridViewCheckBoxColumn DisplayStateColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChannelColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn BandColumn;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnApp;
        private System.Windows.Forms.Panel panel5;
    }
}
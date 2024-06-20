namespace SMGI.Plugin.EmergencyMap
{
    partial class AnnotationClassifiedModifyForm
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
            this.cbAnnotationLayer = new System.Windows.Forms.ComboBox();
            this.lbMainRESALevel = new System.Windows.Forms.Label();
            this.cbAnnotationType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gbBasic = new System.Windows.Forms.GroupBox();
            this.label24 = new System.Windows.Forms.Label();
            this.lbAnnoColorCMYK = new System.Windows.Forms.Label();
            this.btnAnnoColor = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbFontFamily = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbAlign = new System.Windows.Forms.CheckBox();
            this.gb_TextAlign = new System.Windows.Forms.GroupBox();
            this.cb_Vertical = new System.Windows.Forms.ComboBox();
            this.cb_Horizontal = new System.Windows.Forms.ComboBox();
            this.label_Horizontal = new System.Windows.Forms.Label();
            this.label_Vertical = new System.Windows.Forms.Label();
            this.cbBackGround = new System.Windows.Forms.CheckBox();
            this.gb_Balloon = new System.Windows.Forms.GroupBox();
            this.lbBackgroundBdColorCMYK = new System.Windows.Forms.Label();
            this.lbBackgroundColorCMYK = new System.Windows.Forms.Label();
            this.btLineColor = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.cbEnableBackgroud = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btBackgroudCol = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.num_LineWidth = new System.Windows.Forms.NumericUpDown();
            this.cb_Balloon = new System.Windows.Forms.ComboBox();
            this.label_Balloon = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.num_up = new System.Windows.Forms.NumericUpDown();
            this.num_down = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.num_left = new System.Windows.Forms.NumericUpDown();
            this.num_right = new System.Windows.Forms.NumericUpDown();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.cbMask = new System.Windows.Forms.CheckBox();
            this.gb_Mask = new System.Windows.Forms.GroupBox();
            this.lbMaskColorCMYK = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.num_Mask = new System.Windows.Forms.NumericUpDown();
            this.label_MaskSize = new System.Windows.Forms.Label();
            this.label_MaskColor = new System.Windows.Forms.Label();
            this.btnMaskColor = new System.Windows.Forms.Button();
            this.cb_Mask = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbAnnoFeatureCount = new System.Windows.Forms.Label();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.cbBold = new System.Windows.Forms.CheckBox();
            this.cbStyle = new System.Windows.Forms.CheckBox();
            this.gbStyle = new System.Windows.Forms.GroupBox();
            this.num_fontSize = new System.Windows.Forms.NumericUpDown();
            this.gbBasic.SuspendLayout();
            this.gb_TextAlign.SuspendLayout();
            this.gb_Balloon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_LineWidth)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_up)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_down)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_left)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_right)).BeginInit();
            this.gb_Mask.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_Mask)).BeginInit();
            this.panel1.SuspendLayout();
            this.gbStyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_fontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // cbAnnotationLayer
            // 
            this.cbAnnotationLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAnnotationLayer.FormattingEnabled = true;
            this.cbAnnotationLayer.Location = new System.Drawing.Point(83, 6);
            this.cbAnnotationLayer.Name = "cbAnnotationLayer";
            this.cbAnnotationLayer.Size = new System.Drawing.Size(120, 20);
            this.cbAnnotationLayer.TabIndex = 11;
            this.cbAnnotationLayer.SelectedIndexChanged += new System.EventHandler(this.cbAnnotationLayer_SelectedIndexChanged);
            // 
            // lbMainRESALevel
            // 
            this.lbMainRESALevel.AutoSize = true;
            this.lbMainRESALevel.Location = new System.Drawing.Point(14, 9);
            this.lbMainRESALevel.Name = "lbMainRESALevel";
            this.lbMainRESALevel.Size = new System.Drawing.Size(65, 12);
            this.lbMainRESALevel.TabIndex = 10;
            this.lbMainRESALevel.Text = "注记图层：";
            // 
            // cbAnnotationType
            // 
            this.cbAnnotationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAnnotationType.FormattingEnabled = true;
            this.cbAnnotationType.Location = new System.Drawing.Point(338, 6);
            this.cbAnnotationType.Name = "cbAnnotationType";
            this.cbAnnotationType.Size = new System.Drawing.Size(120, 20);
            this.cbAnnotationType.TabIndex = 13;
            this.cbAnnotationType.SelectedIndexChanged += new System.EventHandler(this.cbAnnotationType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(267, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "注记类型：";
            // 
            // gbBasic
            // 
            this.gbBasic.Controls.Add(this.num_fontSize);
            this.gbBasic.Controls.Add(this.label24);
            this.gbBasic.Controls.Add(this.lbAnnoColorCMYK);
            this.gbBasic.Controls.Add(this.btnAnnoColor);
            this.gbBasic.Controls.Add(this.label6);
            this.gbBasic.Controls.Add(this.label5);
            this.gbBasic.Controls.Add(this.label4);
            this.gbBasic.Controls.Add(this.cbFontFamily);
            this.gbBasic.Controls.Add(this.label3);
            this.gbBasic.Location = new System.Drawing.Point(14, 31);
            this.gbBasic.Margin = new System.Windows.Forms.Padding(2);
            this.gbBasic.Name = "gbBasic";
            this.gbBasic.Padding = new System.Windows.Forms.Padding(2);
            this.gbBasic.Size = new System.Drawing.Size(447, 72);
            this.gbBasic.TabIndex = 39;
            this.gbBasic.TabStop = false;
            this.gbBasic.Text = "基本属性";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(5, 155);
            this.label24.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(0, 12);
            this.label24.TabIndex = 11;
            // 
            // lbAnnoColorCMYK
            // 
            this.lbAnnoColorCMYK.AutoSize = true;
            this.lbAnnoColorCMYK.Font = new System.Drawing.Font("SimSun", 7F);
            this.lbAnnoColorCMYK.Location = new System.Drawing.Point(96, 53);
            this.lbAnnoColorCMYK.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbAnnoColorCMYK.Name = "lbAnnoColorCMYK";
            this.lbAnnoColorCMYK.Size = new System.Drawing.Size(25, 10);
            this.lbAnnoColorCMYK.TabIndex = 10;
            this.lbAnnoColorCMYK.Text = "C100";
            // 
            // btnAnnoColor
            // 
            this.btnAnnoColor.BackColor = System.Drawing.Color.Transparent;
            this.btnAnnoColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAnnoColor.Location = new System.Drawing.Point(43, 49);
            this.btnAnnoColor.Margin = new System.Windows.Forms.Padding(2);
            this.btnAnnoColor.Name = "btnAnnoColor";
            this.btnAnnoColor.Size = new System.Drawing.Size(50, 18);
            this.btnAnnoColor.TabIndex = 9;
            this.btnAnnoColor.UseVisualStyleBackColor = false;
            this.btnAnnoColor.Click += new System.EventHandler(this.btnAnnoColor_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 52);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 7;
            this.label6.Text = "颜色：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(361, 23);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 6;
            this.label5.Text = "mm";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(257, 23);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "字大：";
            // 
            // cbFontFamily
            // 
            this.cbFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFontFamily.FormattingEnabled = true;
            this.cbFontFamily.Location = new System.Drawing.Point(43, 18);
            this.cbFontFamily.Margin = new System.Windows.Forms.Padding(2);
            this.cbFontFamily.Name = "cbFontFamily";
            this.cbFontFamily.Size = new System.Drawing.Size(146, 20);
            this.cbFontFamily.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 23);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "字体：";
            // 
            // cbAlign
            // 
            this.cbAlign.AutoSize = true;
            this.cbAlign.Location = new System.Drawing.Point(14, 181);
            this.cbAlign.Name = "cbAlign";
            this.cbAlign.Size = new System.Drawing.Size(96, 16);
            this.cbAlign.TabIndex = 40;
            this.cbAlign.Text = "修改对齐方式";
            this.cbAlign.UseVisualStyleBackColor = true;
            this.cbAlign.CheckedChanged += new System.EventHandler(this.cbAlign_CheckedChanged);
            // 
            // gb_TextAlign
            // 
            this.gb_TextAlign.Controls.Add(this.cb_Vertical);
            this.gb_TextAlign.Controls.Add(this.cb_Horizontal);
            this.gb_TextAlign.Controls.Add(this.label_Horizontal);
            this.gb_TextAlign.Controls.Add(this.label_Vertical);
            this.gb_TextAlign.Enabled = false;
            this.gb_TextAlign.Location = new System.Drawing.Point(14, 197);
            this.gb_TextAlign.Name = "gb_TextAlign";
            this.gb_TextAlign.Size = new System.Drawing.Size(447, 47);
            this.gb_TextAlign.TabIndex = 41;
            this.gb_TextAlign.TabStop = false;
            this.gb_TextAlign.Text = "对齐方式";
            // 
            // cb_Vertical
            // 
            this.cb_Vertical.FormattingEnabled = true;
            this.cb_Vertical.Location = new System.Drawing.Point(321, 19);
            this.cb_Vertical.Name = "cb_Vertical";
            this.cb_Vertical.Size = new System.Drawing.Size(120, 20);
            this.cb_Vertical.TabIndex = 4;
            // 
            // cb_Horizontal
            // 
            this.cb_Horizontal.FormattingEnabled = true;
            this.cb_Horizontal.Location = new System.Drawing.Point(70, 19);
            this.cb_Horizontal.Name = "cb_Horizontal";
            this.cb_Horizontal.Size = new System.Drawing.Size(120, 20);
            this.cb_Horizontal.TabIndex = 3;
            // 
            // label_Horizontal
            // 
            this.label_Horizontal.AutoSize = true;
            this.label_Horizontal.Location = new System.Drawing.Point(5, 22);
            this.label_Horizontal.Name = "label_Horizontal";
            this.label_Horizontal.Size = new System.Drawing.Size(59, 12);
            this.label_Horizontal.TabIndex = 0;
            this.label_Horizontal.Text = "水平对齐:";
            // 
            // label_Vertical
            // 
            this.label_Vertical.AutoSize = true;
            this.label_Vertical.Location = new System.Drawing.Point(256, 22);
            this.label_Vertical.Name = "label_Vertical";
            this.label_Vertical.Size = new System.Drawing.Size(59, 12);
            this.label_Vertical.TabIndex = 1;
            this.label_Vertical.Text = "垂直对齐:";
            // 
            // cbBackGround
            // 
            this.cbBackGround.AutoSize = true;
            this.cbBackGround.Location = new System.Drawing.Point(14, 256);
            this.cbBackGround.Name = "cbBackGround";
            this.cbBackGround.Size = new System.Drawing.Size(96, 16);
            this.cbBackGround.TabIndex = 40;
            this.cbBackGround.Text = "修改文本背景";
            this.cbBackGround.UseVisualStyleBackColor = true;
            this.cbBackGround.CheckedChanged += new System.EventHandler(this.cbBackGround_CheckedChanged);
            // 
            // gb_Balloon
            // 
            this.gb_Balloon.Controls.Add(this.lbBackgroundBdColorCMYK);
            this.gb_Balloon.Controls.Add(this.lbBackgroundColorCMYK);
            this.gb_Balloon.Controls.Add(this.btLineColor);
            this.gb_Balloon.Controls.Add(this.label13);
            this.gb_Balloon.Controls.Add(this.label10);
            this.gb_Balloon.Controls.Add(this.cbEnableBackgroud);
            this.gb_Balloon.Controls.Add(this.label8);
            this.gb_Balloon.Controls.Add(this.btBackgroudCol);
            this.gb_Balloon.Controls.Add(this.label11);
            this.gb_Balloon.Controls.Add(this.num_LineWidth);
            this.gb_Balloon.Controls.Add(this.cb_Balloon);
            this.gb_Balloon.Controls.Add(this.label_Balloon);
            this.gb_Balloon.Controls.Add(this.groupBox2);
            this.gb_Balloon.Enabled = false;
            this.gb_Balloon.Location = new System.Drawing.Point(14, 272);
            this.gb_Balloon.Name = "gb_Balloon";
            this.gb_Balloon.Size = new System.Drawing.Size(447, 192);
            this.gb_Balloon.TabIndex = 42;
            this.gb_Balloon.TabStop = false;
            this.gb_Balloon.Text = "文本背景";
            // 
            // lbBackgroundBdColorCMYK
            // 
            this.lbBackgroundBdColorCMYK.AutoSize = true;
            this.lbBackgroundBdColorCMYK.Font = new System.Drawing.Font("SimSun", 7F);
            this.lbBackgroundBdColorCMYK.Location = new System.Drawing.Point(375, 76);
            this.lbBackgroundBdColorCMYK.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbBackgroundBdColorCMYK.Name = "lbBackgroundBdColorCMYK";
            this.lbBackgroundBdColorCMYK.Size = new System.Drawing.Size(25, 10);
            this.lbBackgroundBdColorCMYK.TabIndex = 37;
            this.lbBackgroundBdColorCMYK.Text = "C100";
            // 
            // lbBackgroundColorCMYK
            // 
            this.lbBackgroundColorCMYK.AutoSize = true;
            this.lbBackgroundColorCMYK.Font = new System.Drawing.Font("SimSun", 7F);
            this.lbBackgroundColorCMYK.Location = new System.Drawing.Point(375, 43);
            this.lbBackgroundColorCMYK.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbBackgroundColorCMYK.Name = "lbBackgroundColorCMYK";
            this.lbBackgroundColorCMYK.Size = new System.Drawing.Size(25, 10);
            this.lbBackgroundColorCMYK.TabIndex = 36;
            this.lbBackgroundColorCMYK.Text = "C100";
            // 
            // btLineColor
            // 
            this.btLineColor.BackColor = System.Drawing.Color.Transparent;
            this.btLineColor.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btLineColor.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btLineColor.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btLineColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btLineColor.Location = new System.Drawing.Point(322, 71);
            this.btLineColor.Name = "btLineColor";
            this.btLineColor.Size = new System.Drawing.Size(50, 18);
            this.btLineColor.TabIndex = 17;
            this.btLineColor.UseVisualStyleBackColor = false;
            this.btLineColor.Click += new System.EventHandler(this.btLineColor_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(136, 71);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(17, 12);
            this.label13.TabIndex = 26;
            this.label13.Text = "mm";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(257, 71);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 12);
            this.label10.TabIndex = 18;
            this.label10.Text = "轮廓颜色:";
            // 
            // cbEnableBackgroud
            // 
            this.cbEnableBackgroud.AutoSize = true;
            this.cbEnableBackgroud.Location = new System.Drawing.Point(9, 19);
            this.cbEnableBackgroud.Name = "cbEnableBackgroud";
            this.cbEnableBackgroud.Size = new System.Drawing.Size(72, 16);
            this.cbEnableBackgroud.TabIndex = 40;
            this.cbEnableBackgroud.Text = "文本背景";
            this.cbEnableBackgroud.UseVisualStyleBackColor = true;
            this.cbEnableBackgroud.CheckedChanged += new System.EventHandler(this.cbBackGround_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 71);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 12);
            this.label8.TabIndex = 35;
            this.label8.Text = "轮廓宽度:";
            // 
            // btBackgroudCol
            // 
            this.btBackgroudCol.BackColor = System.Drawing.Color.Transparent;
            this.btBackgroudCol.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btBackgroudCol.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btBackgroudCol.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btBackgroudCol.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btBackgroudCol.Location = new System.Drawing.Point(322, 38);
            this.btBackgroudCol.Name = "btBackgroudCol";
            this.btBackgroudCol.Size = new System.Drawing.Size(50, 18);
            this.btBackgroudCol.TabIndex = 33;
            this.btBackgroudCol.UseVisualStyleBackColor = false;
            this.btBackgroudCol.Click += new System.EventHandler(this.btBackgroudCol_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(257, 41);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(59, 12);
            this.label11.TabIndex = 34;
            this.label11.Text = "填充颜色:";
            // 
            // num_LineWidth
            // 
            this.num_LineWidth.DecimalPlaces = 1;
            this.num_LineWidth.Location = new System.Drawing.Point(70, 69);
            this.num_LineWidth.Name = "num_LineWidth";
            this.num_LineWidth.Size = new System.Drawing.Size(61, 21);
            this.num_LineWidth.TabIndex = 22;
            // 
            // cb_Balloon
            // 
            this.cb_Balloon.FormattingEnabled = true;
            this.cb_Balloon.Location = new System.Drawing.Point(70, 38);
            this.cb_Balloon.Name = "cb_Balloon";
            this.cb_Balloon.Size = new System.Drawing.Size(63, 20);
            this.cb_Balloon.TabIndex = 32;
            // 
            // label_Balloon
            // 
            this.label_Balloon.AutoSize = true;
            this.label_Balloon.Location = new System.Drawing.Point(7, 41);
            this.label_Balloon.Name = "label_Balloon";
            this.label_Balloon.Size = new System.Drawing.Size(59, 12);
            this.label_Balloon.TabIndex = 31;
            this.label_Balloon.Text = "背景样式:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label21);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.num_up);
            this.groupBox2.Controls.Add(this.num_down);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.num_left);
            this.groupBox2.Controls.Add(this.num_right);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.label19);
            this.groupBox2.Location = new System.Drawing.Point(9, 95);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(432, 91);
            this.groupBox2.TabIndex = 29;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "边框距";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(408, 36);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(17, 12);
            this.label21.TabIndex = 28;
            this.label21.Text = "mm";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(255, 66);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(17, 12);
            this.label15.TabIndex = 27;
            this.label15.Text = "mm";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(104, 36);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(17, 12);
            this.label12.TabIndex = 26;
            this.label12.Text = "mm";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(160, 13);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(23, 12);
            this.label14.TabIndex = 12;
            this.label14.Text = "上:";
            // 
            // num_up
            // 
            this.num_up.DecimalPlaces = 1;
            this.num_up.Location = new System.Drawing.Point(195, 11);
            this.num_up.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.num_up.Name = "num_up";
            this.num_up.Size = new System.Drawing.Size(57, 21);
            this.num_up.TabIndex = 8;
            // 
            // num_down
            // 
            this.num_down.DecimalPlaces = 1;
            this.num_down.Location = new System.Drawing.Point(192, 64);
            this.num_down.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.num_down.Name = "num_down";
            this.num_down.Size = new System.Drawing.Size(57, 21);
            this.num_down.TabIndex = 9;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(258, 13);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(17, 12);
            this.label9.TabIndex = 25;
            this.label9.Text = "mm";
            // 
            // num_left
            // 
            this.num_left.DecimalPlaces = 1;
            this.num_left.Location = new System.Drawing.Point(41, 34);
            this.num_left.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.num_left.Name = "num_left";
            this.num_left.Size = new System.Drawing.Size(57, 21);
            this.num_left.TabIndex = 10;
            // 
            // num_right
            // 
            this.num_right.DecimalPlaces = 1;
            this.num_right.Location = new System.Drawing.Point(345, 34);
            this.num_right.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.num_right.Name = "num_right";
            this.num_right.Size = new System.Drawing.Size(57, 21);
            this.num_right.TabIndex = 11;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(160, 66);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(23, 12);
            this.label17.TabIndex = 13;
            this.label17.Text = "下:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(6, 36);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(23, 12);
            this.label18.TabIndex = 14;
            this.label18.Text = "左:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(314, 36);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(23, 12);
            this.label19.TabIndex = 15;
            this.label19.Text = "右:";
            // 
            // cbMask
            // 
            this.cbMask.AutoSize = true;
            this.cbMask.Location = new System.Drawing.Point(14, 476);
            this.cbMask.Name = "cbMask";
            this.cbMask.Size = new System.Drawing.Size(96, 16);
            this.cbMask.TabIndex = 40;
            this.cbMask.Text = "修改蒙版效果";
            this.cbMask.UseVisualStyleBackColor = true;
            this.cbMask.CheckedChanged += new System.EventHandler(this.cbMask_CheckedChanged);
            // 
            // gb_Mask
            // 
            this.gb_Mask.Controls.Add(this.lbMaskColorCMYK);
            this.gb_Mask.Controls.Add(this.label22);
            this.gb_Mask.Controls.Add(this.num_Mask);
            this.gb_Mask.Controls.Add(this.label_MaskSize);
            this.gb_Mask.Controls.Add(this.label_MaskColor);
            this.gb_Mask.Controls.Add(this.btnMaskColor);
            this.gb_Mask.Controls.Add(this.cb_Mask);
            this.gb_Mask.Controls.Add(this.label16);
            this.gb_Mask.Enabled = false;
            this.gb_Mask.Location = new System.Drawing.Point(14, 492);
            this.gb_Mask.Name = "gb_Mask";
            this.gb_Mask.Size = new System.Drawing.Size(449, 74);
            this.gb_Mask.TabIndex = 43;
            this.gb_Mask.TabStop = false;
            this.gb_Mask.Text = "蒙版";
            // 
            // lbMaskColorCMYK
            // 
            this.lbMaskColorCMYK.AutoSize = true;
            this.lbMaskColorCMYK.Font = new System.Drawing.Font("SimSun", 7F);
            this.lbMaskColorCMYK.Location = new System.Drawing.Point(97, 56);
            this.lbMaskColorCMYK.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbMaskColorCMYK.Name = "lbMaskColorCMYK";
            this.lbMaskColorCMYK.Size = new System.Drawing.Size(25, 10);
            this.lbMaskColorCMYK.TabIndex = 37;
            this.lbMaskColorCMYK.Text = "C100";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(363, 23);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(17, 12);
            this.label22.TabIndex = 26;
            this.label22.Text = "mm";
            // 
            // num_Mask
            // 
            this.num_Mask.DecimalPlaces = 2;
            this.num_Mask.Location = new System.Drawing.Point(302, 19);
            this.num_Mask.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.num_Mask.Name = "num_Mask";
            this.num_Mask.Size = new System.Drawing.Size(60, 21);
            this.num_Mask.TabIndex = 17;
            // 
            // label_MaskSize
            // 
            this.label_MaskSize.AutoSize = true;
            this.label_MaskSize.Location = new System.Drawing.Point(259, 23);
            this.label_MaskSize.Name = "label_MaskSize";
            this.label_MaskSize.Size = new System.Drawing.Size(35, 12);
            this.label_MaskSize.TabIndex = 16;
            this.label_MaskSize.Text = "大小:";
            // 
            // label_MaskColor
            // 
            this.label_MaskColor.AutoSize = true;
            this.label_MaskColor.Location = new System.Drawing.Point(7, 54);
            this.label_MaskColor.Name = "label_MaskColor";
            this.label_MaskColor.Size = new System.Drawing.Size(35, 12);
            this.label_MaskColor.TabIndex = 9;
            this.label_MaskColor.Text = "颜色:";
            // 
            // btnMaskColor
            // 
            this.btnMaskColor.BackColor = System.Drawing.Color.Transparent;
            this.btnMaskColor.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnMaskColor.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnMaskColor.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnMaskColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMaskColor.Location = new System.Drawing.Point(44, 51);
            this.btnMaskColor.Name = "btnMaskColor";
            this.btnMaskColor.Size = new System.Drawing.Size(50, 18);
            this.btnMaskColor.TabIndex = 8;
            this.btnMaskColor.UseVisualStyleBackColor = false;
            this.btnMaskColor.Click += new System.EventHandler(this.btnMaskColor_Click);
            // 
            // cb_Mask
            // 
            this.cb_Mask.FormattingEnabled = true;
            this.cb_Mask.Location = new System.Drawing.Point(45, 20);
            this.cb_Mask.Name = "cb_Mask";
            this.cb_Mask.Size = new System.Drawing.Size(97, 20);
            this.cb_Mask.TabIndex = 7;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(9, 23);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(35, 12);
            this.label16.TabIndex = 5;
            this.label16.Text = "样式:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbAnnoFeatureCount);
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 567);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(473, 31);
            this.panel1.TabIndex = 44;
            // 
            // lbAnnoFeatureCount
            // 
            this.lbAnnoFeatureCount.AutoSize = true;
            this.lbAnnoFeatureCount.Location = new System.Drawing.Point(12, 10);
            this.lbAnnoFeatureCount.Name = "lbAnnoFeatureCount";
            this.lbAnnoFeatureCount.Size = new System.Drawing.Size(0, 12);
            this.lbAnnoFeatureCount.TabIndex = 8;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(332, 4);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(64, 23);
            this.btOK.TabIndex = 7;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(396, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(405, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // cbBold
            // 
            this.cbBold.AutoSize = true;
            this.cbBold.Location = new System.Drawing.Point(5, 17);
            this.cbBold.Name = "cbBold";
            this.cbBold.Size = new System.Drawing.Size(72, 16);
            this.cbBold.TabIndex = 13;
            this.cbBold.Text = "字体加粗";
            this.cbBold.UseVisualStyleBackColor = true;
            // 
            // cbStyle
            // 
            this.cbStyle.AutoSize = true;
            this.cbStyle.Location = new System.Drawing.Point(14, 114);
            this.cbStyle.Name = "cbStyle";
            this.cbStyle.Size = new System.Drawing.Size(72, 16);
            this.cbStyle.TabIndex = 40;
            this.cbStyle.Text = "修改样式";
            this.cbStyle.UseVisualStyleBackColor = true;
            this.cbStyle.CheckedChanged += new System.EventHandler(this.cbStyle_CheckedChanged);
            // 
            // gbStyle
            // 
            this.gbStyle.Controls.Add(this.cbBold);
            this.gbStyle.Enabled = false;
            this.gbStyle.Location = new System.Drawing.Point(14, 131);
            this.gbStyle.Name = "gbStyle";
            this.gbStyle.Size = new System.Drawing.Size(447, 38);
            this.gbStyle.TabIndex = 42;
            this.gbStyle.TabStop = false;
            this.gbStyle.Text = "样式";
            // 
            // num_fontSize
            // 
            this.num_fontSize.DecimalPlaces = 2;
            this.num_fontSize.Location = new System.Drawing.Point(296, 19);
            this.num_fontSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.num_fontSize.Name = "num_fontSize";
            this.num_fontSize.Size = new System.Drawing.Size(60, 21);
            this.num_fontSize.TabIndex = 18;
            // 
            // AnnotationClassifiedModifyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 598);
            this.Controls.Add(this.gbStyle);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gb_Mask);
            this.Controls.Add(this.gb_Balloon);
            this.Controls.Add(this.gb_TextAlign);
            this.Controls.Add(this.cbMask);
            this.Controls.Add(this.cbBackGround);
            this.Controls.Add(this.cbStyle);
            this.Controls.Add(this.cbAlign);
            this.Controls.Add(this.gbBasic);
            this.Controls.Add(this.cbAnnotationType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbAnnotationLayer);
            this.Controls.Add(this.lbMainRESALevel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AnnotationClassifiedModifyForm";
            this.Text = "注记分类属性统改";
            this.Load += new System.EventHandler(this.AnnotationClassifiedModifyForm_Load);
            this.gbBasic.ResumeLayout(false);
            this.gbBasic.PerformLayout();
            this.gb_TextAlign.ResumeLayout(false);
            this.gb_TextAlign.PerformLayout();
            this.gb_Balloon.ResumeLayout(false);
            this.gb_Balloon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_LineWidth)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_up)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_down)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_left)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_right)).EndInit();
            this.gb_Mask.ResumeLayout(false);
            this.gb_Mask.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_Mask)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.gbStyle.ResumeLayout(false);
            this.gbStyle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_fontSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbAnnotationLayer;
        private System.Windows.Forms.Label lbMainRESALevel;
        private System.Windows.Forms.ComboBox cbAnnotationType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbBasic;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Button btnAnnoColor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbFontFamily;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbAlign;
        private System.Windows.Forms.GroupBox gb_TextAlign;
        private System.Windows.Forms.Label label_Horizontal;
        private System.Windows.Forms.Label label_Vertical;
        private System.Windows.Forms.CheckBox cbBackGround;
        private System.Windows.Forms.ComboBox cb_Vertical;
        private System.Windows.Forms.ComboBox cb_Horizontal;
        private System.Windows.Forms.GroupBox gb_Balloon;
        private System.Windows.Forms.Button btLineColor;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btBackgroudCol;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown num_LineWidth;
        private System.Windows.Forms.ComboBox cb_Balloon;
        private System.Windows.Forms.Label label_Balloon;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown num_up;
        private System.Windows.Forms.NumericUpDown num_down;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown num_left;
        private System.Windows.Forms.NumericUpDown num_right;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.CheckBox cbMask;
        private System.Windows.Forms.GroupBox gb_Mask;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.NumericUpDown num_Mask;
        private System.Windows.Forms.Label label_MaskSize;
        private System.Windows.Forms.Label label_MaskColor;
        private System.Windows.Forms.Button btnMaskColor;
        private System.Windows.Forms.ComboBox cb_Mask;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label lbBackgroundBdColorCMYK;
        private System.Windows.Forms.Label lbBackgroundColorCMYK;
        private System.Windows.Forms.Label lbMaskColorCMYK;
        private System.Windows.Forms.Label lbAnnoFeatureCount;
        private System.Windows.Forms.Label lbAnnoColorCMYK;
        private System.Windows.Forms.CheckBox cbBold;
        private System.Windows.Forms.CheckBox cbEnableBackgroud;
        private System.Windows.Forms.CheckBox cbStyle;
        private System.Windows.Forms.GroupBox gbStyle;
        private System.Windows.Forms.NumericUpDown num_fontSize;
    }
}
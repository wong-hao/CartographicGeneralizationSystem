namespace BuildingGen {
    partial class CreateWorkspaceDlg {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btPath = new System.Windows.Forms.Button();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbGenScale = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbOrgScale = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button15 = new System.Windows.Forms.Button();
            this.tbPOI = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.button14 = new System.Windows.Forms.Button();
            this.tbIsland = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.button13 = new System.Windows.Forms.Button();
            this.tbBRT = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.button10 = new System.Windows.Forms.Button();
            this.tbWater = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.button9 = new System.Windows.Forms.Button();
            this.tbForbid = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.button8 = new System.Windows.Forms.Button();
            this.tbPlant = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.button7 = new System.Windows.Forms.Button();
            this.tbFactory = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.button6 = new System.Windows.Forms.Button();
            this.tbBuilding = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.tbRoadHL = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.tbRoadHA = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.tbRoadL = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.tbRoadA = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.tbRailway = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btPath);
            this.groupBox1.Controls.Add(this.tbPath);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(415, 55);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "路径";
            // 
            // btPath
            // 
            this.btPath.Location = new System.Drawing.Point(331, 19);
            this.btPath.Name = "btPath";
            this.btPath.Size = new System.Drawing.Size(75, 23);
            this.btPath.TabIndex = 1;
            this.btPath.Text = "浏览";
            this.btPath.UseVisualStyleBackColor = true;
            this.btPath.Click += new System.EventHandler(this.btPath_Click);
            // 
            // tbPath
            // 
            this.tbPath.Location = new System.Drawing.Point(13, 21);
            this.tbPath.Name = "tbPath";
            this.tbPath.ReadOnly = true;
            this.tbPath.Size = new System.Drawing.Size(308, 21);
            this.tbPath.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbGenScale);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cbOrgScale);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(13, 74);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(414, 50);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "比例尺";
            // 
            // cbGenScale
            // 
            this.cbGenScale.FormattingEnabled = true;
            this.cbGenScale.Items.AddRange(new object[] {
            "1:500",
            "1:2,000",
            "1:5,000",
            "1:10,000",
            "1:25,000",
            "1:50,000",
            "1:10,000"});
            this.cbGenScale.Location = new System.Drawing.Point(294, 17);
            this.cbGenScale.Name = "cbGenScale";
            this.cbGenScale.Size = new System.Drawing.Size(111, 20);
            this.cbGenScale.TabIndex = 3;
            this.cbGenScale.Text = "1:10,000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(211, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "综合比例尺：";
            // 
            // cbOrgScale
            // 
            this.cbOrgScale.FormattingEnabled = true;
            this.cbOrgScale.Items.AddRange(new object[] {
            "1:500",
            "1:2,000",
            "1:5,000",
            "1:10,000",
            "1:25,000",
            "1:50,000",
            "1:10,000"});
            this.cbOrgScale.Location = new System.Drawing.Point(89, 17);
            this.cbOrgScale.Name = "cbOrgScale";
            this.cbOrgScale.Size = new System.Drawing.Size(116, 20);
            this.cbOrgScale.TabIndex = 1;
            this.cbOrgScale.Text = "1:2,000";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "原始比例尺:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button15);
            this.groupBox3.Controls.Add(this.tbPOI);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.button14);
            this.groupBox3.Controls.Add(this.tbIsland);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.button13);
            this.groupBox3.Controls.Add(this.tbBRT);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.button10);
            this.groupBox3.Controls.Add(this.tbWater);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.button9);
            this.groupBox3.Controls.Add(this.tbForbid);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.button8);
            this.groupBox3.Controls.Add(this.tbPlant);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.button7);
            this.groupBox3.Controls.Add(this.tbFactory);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.button6);
            this.groupBox3.Controls.Add(this.tbBuilding);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.button5);
            this.groupBox3.Controls.Add(this.tbRoadHL);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.button4);
            this.groupBox3.Controls.Add(this.tbRoadHA);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.tbRoadL);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.tbRoadA);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.tbRailway);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(13, 131);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(414, 394);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "数据";
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(333, 343);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(75, 23);
            this.button15.TabIndex = 38;
            this.button15.Tag = "POI";
            this.button15.Text = "浏览";
            this.button15.UseVisualStyleBackColor = true;
            this.button15.Click += new System.EventHandler(this.ViewData);
            // 
            // tbPOI
            // 
            this.tbPOI.Location = new System.Drawing.Point(73, 345);
            this.tbPOI.Name = "tbPOI";
            this.tbPOI.ReadOnly = true;
            this.tbPOI.Size = new System.Drawing.Size(247, 21);
            this.tbPOI.TabIndex = 37;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(14, 348);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(35, 12);
            this.label15.TabIndex = 36;
            this.label15.Text = "POI：";
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(333, 316);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(75, 23);
            this.button14.TabIndex = 35;
            this.button14.Tag = "绿化岛";
            this.button14.Text = "浏览";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.ViewData);
            // 
            // tbIsland
            // 
            this.tbIsland.Location = new System.Drawing.Point(73, 318);
            this.tbIsland.Name = "tbIsland";
            this.tbIsland.ReadOnly = true;
            this.tbIsland.Size = new System.Drawing.Size(247, 21);
            this.tbIsland.TabIndex = 34;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(14, 321);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(53, 12);
            this.label14.TabIndex = 33;
            this.label14.Text = "绿化岛：";
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(333, 289);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(75, 23);
            this.button13.TabIndex = 32;
            this.button13.Tag = "BRT交通面";
            this.button13.Text = "浏览";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.ViewData);
            // 
            // tbBRT
            // 
            this.tbBRT.Location = new System.Drawing.Point(73, 291);
            this.tbBRT.Name = "tbBRT";
            this.tbBRT.ReadOnly = true;
            this.tbBRT.Size = new System.Drawing.Size(247, 21);
            this.tbBRT.TabIndex = 31;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(14, 294);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(59, 12);
            this.label13.TabIndex = 30;
            this.label13.Text = "BRT交通：";
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(333, 262);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.TabIndex = 29;
            this.button10.Tag = "水系面";
            this.button10.Text = "浏览";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.ViewData);
            // 
            // tbWater
            // 
            this.tbWater.Location = new System.Drawing.Point(73, 264);
            this.tbWater.Name = "tbWater";
            this.tbWater.ReadOnly = true;
            this.tbWater.Size = new System.Drawing.Size(247, 21);
            this.tbWater.TabIndex = 28;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(14, 267);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(53, 12);
            this.label12.TabIndex = 27;
            this.label12.Text = "水系面：";
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(333, 235);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 23);
            this.button9.TabIndex = 26;
            this.button9.Tag = "禁测面";
            this.button9.Text = "浏览";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.ViewData);
            // 
            // tbForbid
            // 
            this.tbForbid.Location = new System.Drawing.Point(73, 237);
            this.tbForbid.Name = "tbForbid";
            this.tbForbid.ReadOnly = true;
            this.tbForbid.Size = new System.Drawing.Size(247, 21);
            this.tbForbid.TabIndex = 25;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(14, 240);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(53, 12);
            this.label11.TabIndex = 24;
            this.label11.Text = "禁测面：";
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(333, 208);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.TabIndex = 23;
            this.button8.Tag = "植被面";
            this.button8.Text = "浏览";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.ViewData);
            // 
            // tbPlant
            // 
            this.tbPlant.Location = new System.Drawing.Point(73, 210);
            this.tbPlant.Name = "tbPlant";
            this.tbPlant.ReadOnly = true;
            this.tbPlant.Size = new System.Drawing.Size(247, 21);
            this.tbPlant.TabIndex = 22;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(14, 21);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 12);
            this.label10.TabIndex = 21;
            this.label10.Text = "铁路线：";
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(333, 178);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 20;
            this.button7.Tag = "工矿面";
            this.button7.Text = "浏览";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.ViewData);
            // 
            // tbFactory
            // 
            this.tbFactory.Location = new System.Drawing.Point(73, 180);
            this.tbFactory.Name = "tbFactory";
            this.tbFactory.ReadOnly = true;
            this.tbFactory.Size = new System.Drawing.Size(247, 21);
            this.tbFactory.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(14, 213);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 18;
            this.label9.Text = "植被面：";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(333, 151);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 17;
            this.button6.Tag = "房屋面";
            this.button6.Text = "浏览";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.ViewData);
            // 
            // tbBuilding
            // 
            this.tbBuilding.Location = new System.Drawing.Point(73, 153);
            this.tbBuilding.Name = "tbBuilding";
            this.tbBuilding.ReadOnly = true;
            this.tbBuilding.Size = new System.Drawing.Size(247, 21);
            this.tbBuilding.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(14, 183);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 15;
            this.label8.Text = "工矿面：";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(333, 124);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 14;
            this.button5.Tag = "高架线";
            this.button5.Text = "浏览";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.ViewData);
            // 
            // tbRoadHL
            // 
            this.tbRoadHL.Location = new System.Drawing.Point(73, 126);
            this.tbRoadHL.Name = "tbRoadHL";
            this.tbRoadHL.ReadOnly = true;
            this.tbRoadHL.Size = new System.Drawing.Size(247, 21);
            this.tbRoadHL.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 156);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 12;
            this.label7.Text = "房屋面：";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(333, 97);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 11;
            this.button4.Tag = "高架面";
            this.button4.Text = "浏览";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.ViewData);
            // 
            // tbRoadHA
            // 
            this.tbRoadHA.Location = new System.Drawing.Point(73, 99);
            this.tbRoadHA.Name = "tbRoadHA";
            this.tbRoadHA.ReadOnly = true;
            this.tbRoadHA.Size = new System.Drawing.Size(247, 21);
            this.tbRoadHA.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 129);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 9;
            this.label6.Text = "高架线：";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(333, 70);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 8;
            this.button3.Tag = "道路线";
            this.button3.Text = "浏览";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.ViewData);
            // 
            // tbRoadL
            // 
            this.tbRoadL.Location = new System.Drawing.Point(73, 72);
            this.tbRoadL.Name = "tbRoadL";
            this.tbRoadL.ReadOnly = true;
            this.tbRoadL.Size = new System.Drawing.Size(247, 21);
            this.tbRoadL.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 6;
            this.label5.Text = "高架面：";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(333, 43);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Tag = "道路面";
            this.button2.Text = "浏览";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ViewData);
            // 
            // tbRoadA
            // 
            this.tbRoadA.Location = new System.Drawing.Point(73, 45);
            this.tbRoadA.Name = "tbRoadA";
            this.tbRoadA.ReadOnly = true;
            this.tbRoadA.Size = new System.Drawing.Size(247, 21);
            this.tbRoadA.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "道路线：";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(333, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Tag = "铁路线";
            this.button1.Text = "浏览";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ViewData);
            // 
            // tbRailway
            // 
            this.tbRailway.Location = new System.Drawing.Point(73, 18);
            this.tbRailway.Name = "tbRailway";
            this.tbRailway.ReadOnly = true;
            this.tbRailway.Size = new System.Drawing.Size(247, 21);
            this.tbRailway.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "道路面：";
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(261, 531);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(75, 23);
            this.button11.TabIndex = 3;
            this.button11.Text = "确定";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.create);
            // 
            // button12
            // 
            this.button12.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button12.Location = new System.Drawing.Point(346, 531);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(75, 23);
            this.button12.TabIndex = 4;
            this.button12.Text = "取消";
            this.button12.UseVisualStyleBackColor = true;
            // 
            // CreateWorkspaceDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 566);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CreateWorkspaceDlg";
            this.Text = "创建工作区";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btPath;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cbGenScale;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbOrgScale;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.TextBox tbFactory;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.TextBox tbBuilding;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox tbRoadHL;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox tbRoadHA;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox tbRoadL;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox tbRoadA;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox tbRailway;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.TextBox tbPlant;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.TextBox tbWater;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.TextBox tbForbid;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.TextBox tbBRT;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.TextBox tbIsland;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.TextBox tbPOI;
        private System.Windows.Forms.Label label15;
    }
}
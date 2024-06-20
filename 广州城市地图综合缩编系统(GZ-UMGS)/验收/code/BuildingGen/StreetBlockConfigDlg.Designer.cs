namespace BuildingGen {
    partial class StreetBlockConfigDlg {
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
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btExport = new System.Windows.Forms.Button();
            this.tbExport = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btBuilding = new System.Windows.Forms.Button();
            this.tbBuilding = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btArea = new System.Windows.Forms.Button();
            this.tbArea = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btRoad = new System.Windows.Forms.Button();
            this.tbRoad = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rbWidth = new System.Windows.Forms.RadioButton();
            this.rbStatic = new System.Windows.Forms.RadioButton();
            this.gbWidth = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbWidthSize = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbWidthField = new System.Windows.Forms.ComboBox();
            this.gbStatic = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbStaticSize = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tbBufferDis = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.gbWidth.SuspendLayout();
            this.gbStatic.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(225, 320);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 9;
            this.button4.Text = "确定";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button5.Location = new System.Drawing.Point(306, 320);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 10;
            this.button5.Text = "取消";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btExport);
            this.groupBox1.Controls.Add(this.tbExport);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(15, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(366, 54);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "导出路径";
            // 
            // btExport
            // 
            this.btExport.Location = new System.Drawing.Point(283, 18);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(75, 23);
            this.btExport.TabIndex = 16;
            this.btExport.Text = "浏览";
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // tbExport
            // 
            this.tbExport.Location = new System.Drawing.Point(71, 20);
            this.tbExport.Name = "tbExport";
            this.tbExport.ReadOnly = true;
            this.tbExport.Size = new System.Drawing.Size(206, 21);
            this.tbExport.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "导出路径:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btBuilding);
            this.groupBox2.Controls.Add(this.tbBuilding);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.btArea);
            this.groupBox2.Controls.Add(this.tbArea);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btRoad);
            this.groupBox2.Controls.Add(this.tbRoad);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(15, 72);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(366, 101);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "数据源";
            // 
            // btBuilding
            // 
            this.btBuilding.Location = new System.Drawing.Point(283, 70);
            this.btBuilding.Name = "btBuilding";
            this.btBuilding.Size = new System.Drawing.Size(75, 23);
            this.btBuilding.TabIndex = 17;
            this.btBuilding.Tag = "building";
            this.btBuilding.Text = "浏览";
            this.btBuilding.UseVisualStyleBackColor = true;
            this.btBuilding.Click += new System.EventHandler(this.inport_Click);
            // 
            // tbBuilding
            // 
            this.tbBuilding.Location = new System.Drawing.Point(112, 71);
            this.tbBuilding.Name = "tbBuilding";
            this.tbBuilding.ReadOnly = true;
            this.tbBuilding.Size = new System.Drawing.Size(165, 21);
            this.tbBuilding.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "房屋面(可选)";
            // 
            // btArea
            // 
            this.btArea.Location = new System.Drawing.Point(283, 42);
            this.btArea.Name = "btArea";
            this.btArea.Size = new System.Drawing.Size(75, 23);
            this.btArea.TabIndex = 14;
            this.btArea.Tag = "area";
            this.btArea.Text = "浏览";
            this.btArea.UseVisualStyleBackColor = true;
            this.btArea.Click += new System.EventHandler(this.inport_Click);
            // 
            // tbArea
            // 
            this.tbArea.Location = new System.Drawing.Point(112, 44);
            this.tbArea.Name = "tbArea";
            this.tbArea.ReadOnly = true;
            this.tbArea.Size = new System.Drawing.Size(165, 21);
            this.tbArea.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 12;
            this.label2.Text = "政区范围面(必选)";
            // 
            // btRoad
            // 
            this.btRoad.Location = new System.Drawing.Point(283, 16);
            this.btRoad.Name = "btRoad";
            this.btRoad.Size = new System.Drawing.Size(75, 23);
            this.btRoad.TabIndex = 11;
            this.btRoad.Tag = "road";
            this.btRoad.Text = "浏览";
            this.btRoad.UseVisualStyleBackColor = true;
            this.btRoad.Click += new System.EventHandler(this.inport_Click);
            // 
            // tbRoad
            // 
            this.tbRoad.Location = new System.Drawing.Point(112, 17);
            this.tbRoad.Name = "tbRoad";
            this.tbRoad.ReadOnly = true;
            this.tbRoad.Size = new System.Drawing.Size(165, 21);
            this.tbRoad.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "道路中心线(必选)";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.rbWidth);
            this.groupBox3.Controls.Add(this.rbStatic);
            this.groupBox3.Controls.Add(this.gbWidth);
            this.groupBox3.Controls.Add(this.gbStatic);
            this.groupBox3.Location = new System.Drawing.Point(15, 179);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(366, 135);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "参数";
            // 
            // rbWidth
            // 
            this.rbWidth.AutoSize = true;
            this.rbWidth.Location = new System.Drawing.Point(116, 22);
            this.rbWidth.Name = "rbWidth";
            this.rbWidth.Size = new System.Drawing.Size(95, 16);
            this.rbWidth.TabIndex = 4;
            this.rbWidth.Text = "使用道路宽度";
            this.rbWidth.UseVisualStyleBackColor = true;
            this.rbWidth.CheckedChanged += new System.EventHandler(this.rbWidth_CheckedChanged);
            // 
            // rbStatic
            // 
            this.rbStatic.AutoSize = true;
            this.rbStatic.Checked = true;
            this.rbStatic.Location = new System.Drawing.Point(11, 22);
            this.rbStatic.Name = "rbStatic";
            this.rbStatic.Size = new System.Drawing.Size(83, 16);
            this.rbStatic.TabIndex = 3;
            this.rbStatic.TabStop = true;
            this.rbStatic.Text = "使用固定值";
            this.rbStatic.UseVisualStyleBackColor = true;
            // 
            // gbWidth
            // 
            this.gbWidth.Controls.Add(this.label7);
            this.gbWidth.Controls.Add(this.tbWidthSize);
            this.gbWidth.Controls.Add(this.label6);
            this.gbWidth.Controls.Add(this.cbWidthField);
            this.gbWidth.Enabled = false;
            this.gbWidth.Location = new System.Drawing.Point(116, 44);
            this.gbWidth.Name = "gbWidth";
            this.gbWidth.Size = new System.Drawing.Size(161, 85);
            this.gbWidth.TabIndex = 2;
            this.gbWidth.TabStop = false;
            this.gbWidth.Text = "道路宽度参数";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 19;
            this.label7.Text = "缝隙大小";
            // 
            // tbWidthSize
            // 
            this.tbWidthSize.Location = new System.Drawing.Point(64, 50);
            this.tbWidthSize.Name = "tbWidthSize";
            this.tbWidthSize.Size = new System.Drawing.Size(87, 21);
            this.tbWidthSize.TabIndex = 18;
            this.tbWidthSize.Text = "20";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 17;
            this.label6.Text = "宽度字段";
            // 
            // cbWidthField
            // 
            this.cbWidthField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbWidthField.FormattingEnabled = true;
            this.cbWidthField.Location = new System.Drawing.Point(64, 20);
            this.cbWidthField.Name = "cbWidthField";
            this.cbWidthField.Size = new System.Drawing.Size(88, 20);
            this.cbWidthField.TabIndex = 0;
            // 
            // gbStatic
            // 
            this.gbStatic.Controls.Add(this.label5);
            this.gbStatic.Controls.Add(this.tbStaticSize);
            this.gbStatic.Location = new System.Drawing.Point(11, 44);
            this.gbStatic.Name = "gbStatic";
            this.gbStatic.Size = new System.Drawing.Size(99, 85);
            this.gbStatic.TabIndex = 1;
            this.gbStatic.TabStop = false;
            this.gbStatic.Text = "固定值参数";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 16;
            this.label5.Text = "固定值大小";
            // 
            // tbStaticSize
            // 
            this.tbStaticSize.Location = new System.Drawing.Point(9, 38);
            this.tbStaticSize.Name = "tbStaticSize";
            this.tbStaticSize.Size = new System.Drawing.Size(75, 21);
            this.tbStaticSize.TabIndex = 0;
            this.tbStaticSize.Text = "20";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tbBufferDis);
            this.groupBox4.Location = new System.Drawing.Point(283, 44);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(75, 85);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "圆角半径";
            // 
            // tbBufferDis
            // 
            this.tbBufferDis.Location = new System.Drawing.Point(8, 23);
            this.tbBufferDis.Name = "tbBufferDis";
            this.tbBufferDis.Size = new System.Drawing.Size(61, 21);
            this.tbBufferDis.TabIndex = 17;
            this.tbBufferDis.Text = "50";
            // 
            // StreetBlockConfigDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 348);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "StreetBlockConfigDlg";
            this.Text = "街区配置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.gbWidth.ResumeLayout(false);
            this.gbWidth.PerformLayout();
            this.gbStatic.ResumeLayout(false);
            this.gbStatic.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btExport;
        private System.Windows.Forms.TextBox tbExport;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btBuilding;
        private System.Windows.Forms.TextBox tbBuilding;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btArea;
        private System.Windows.Forms.TextBox tbArea;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btRoad;
        private System.Windows.Forms.TextBox tbRoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox gbWidth;
        private System.Windows.Forms.GroupBox gbStatic;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbStaticSize;
        private System.Windows.Forms.RadioButton rbWidth;
        private System.Windows.Forms.RadioButton rbStatic;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbWidthSize;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cbWidthField;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox tbBufferDis;
    }
}
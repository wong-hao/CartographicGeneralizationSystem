namespace SMGI.Plugin.EmergencyMap
{
    partial class NormalQJForm
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
            this.panel3 = new System.Windows.Forms.Panel();
            this.gbMain = new System.Windows.Forms.GroupBox();
            this.gbDisEx = new System.Windows.Forms.GroupBox();
            this.tbMainProvinceQJWidth = new System.Windows.Forms.TextBox();
            this.tbMainCountyQJWidth = new System.Windows.Forms.TextBox();
            this.tbMainStateQJWidth = new System.Windows.Forms.TextBox();
            this.tbMainTownQJWidth = new System.Windows.Forms.TextBox();
            this.lbMainTownQJWidth = new System.Windows.Forms.Label();
            this.lbMainCountyQJWidth = new System.Windows.Forms.Label();
            this.lbMainStateQJWidth = new System.Windows.Forms.Label();
            this.lbMainProvinceQJWidth = new System.Windows.Forms.Label();
            this.btMainQJColor = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cbMainBOULSQL = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbMainQJ = new System.Windows.Forms.CheckBox();
            this.panelAdj = new System.Windows.Forms.Panel();
            this.gbAdj = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tbAdjProvinceQJWidth = new System.Windows.Forms.TextBox();
            this.tbAdjCountyQJWidth = new System.Windows.Forms.TextBox();
            this.tbAdjStateQJWidth = new System.Windows.Forms.TextBox();
            this.tbAdjTownQJWidth = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btAdjQJColor = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.cbAdjBOULSQL = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.cbAdjQJ = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.panel3.SuspendLayout();
            this.gbMain.SuspendLayout();
            this.gbDisEx.SuspendLayout();
            this.panelAdj.SuspendLayout();
            this.gbAdj.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.gbMain);
            this.panel3.Controls.Add(this.cbMainQJ);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(5, 5);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(464, 167);
            this.panel3.TabIndex = 14;
            // 
            // gbMain
            // 
            this.gbMain.Controls.Add(this.gbDisEx);
            this.gbMain.Controls.Add(this.btMainQJColor);
            this.gbMain.Controls.Add(this.label3);
            this.gbMain.Controls.Add(this.cbMainBOULSQL);
            this.gbMain.Controls.Add(this.label19);
            this.gbMain.Controls.Add(this.label2);
            this.gbMain.Enabled = false;
            this.gbMain.Location = new System.Drawing.Point(7, 27);
            this.gbMain.Name = "gbMain";
            this.gbMain.Size = new System.Drawing.Size(451, 135);
            this.gbMain.TabIndex = 15;
            this.gbMain.TabStop = false;
            this.gbMain.Text = "主区骑界";
            // 
            // gbDisEx
            // 
            this.gbDisEx.Controls.Add(this.tbMainProvinceQJWidth);
            this.gbDisEx.Controls.Add(this.tbMainCountyQJWidth);
            this.gbDisEx.Controls.Add(this.tbMainStateQJWidth);
            this.gbDisEx.Controls.Add(this.tbMainTownQJWidth);
            this.gbDisEx.Controls.Add(this.lbMainTownQJWidth);
            this.gbDisEx.Controls.Add(this.lbMainCountyQJWidth);
            this.gbDisEx.Controls.Add(this.lbMainStateQJWidth);
            this.gbDisEx.Controls.Add(this.lbMainProvinceQJWidth);
            this.gbDisEx.Location = new System.Drawing.Point(6, 58);
            this.gbDisEx.Name = "gbDisEx";
            this.gbDisEx.Size = new System.Drawing.Size(433, 65);
            this.gbDisEx.TabIndex = 30;
            this.gbDisEx.TabStop = false;
            this.gbDisEx.Text = "骑界宽度(单位：mm)";
            // 
            // tbMainProvinceQJWidth
            // 
            this.tbMainProvinceQJWidth.Location = new System.Drawing.Point(49, 29);
            this.tbMainProvinceQJWidth.Name = "tbMainProvinceQJWidth";
            this.tbMainProvinceQJWidth.Size = new System.Drawing.Size(55, 21);
            this.tbMainProvinceQJWidth.TabIndex = 27;
            this.tbMainProvinceQJWidth.Text = "3.5";
            // 
            // tbMainCountyQJWidth
            // 
            this.tbMainCountyQJWidth.Location = new System.Drawing.Point(263, 29);
            this.tbMainCountyQJWidth.Name = "tbMainCountyQJWidth";
            this.tbMainCountyQJWidth.Size = new System.Drawing.Size(55, 21);
            this.tbMainCountyQJWidth.TabIndex = 31;
            this.tbMainCountyQJWidth.Text = "1.5";
            // 
            // tbMainStateQJWidth
            // 
            this.tbMainStateQJWidth.Location = new System.Drawing.Point(155, 29);
            this.tbMainStateQJWidth.Name = "tbMainStateQJWidth";
            this.tbMainStateQJWidth.Size = new System.Drawing.Size(55, 21);
            this.tbMainStateQJWidth.TabIndex = 29;
            this.tbMainStateQJWidth.Text = "2.5";
            // 
            // tbMainTownQJWidth
            // 
            this.tbMainTownQJWidth.Location = new System.Drawing.Point(368, 29);
            this.tbMainTownQJWidth.Name = "tbMainTownQJWidth";
            this.tbMainTownQJWidth.Size = new System.Drawing.Size(55, 21);
            this.tbMainTownQJWidth.TabIndex = 33;
            this.tbMainTownQJWidth.Text = "1";
            // 
            // lbMainTownQJWidth
            // 
            this.lbMainTownQJWidth.AutoSize = true;
            this.lbMainTownQJWidth.Location = new System.Drawing.Point(333, 33);
            this.lbMainTownQJWidth.Name = "lbMainTownQJWidth";
            this.lbMainTownQJWidth.Size = new System.Drawing.Size(41, 12);
            this.lbMainTownQJWidth.TabIndex = 32;
            this.lbMainTownQJWidth.Tag = " ";
            this.lbMainTownQJWidth.Text = "乡级：";
            // 
            // lbMainCountyQJWidth
            // 
            this.lbMainCountyQJWidth.AutoSize = true;
            this.lbMainCountyQJWidth.Location = new System.Drawing.Point(228, 33);
            this.lbMainCountyQJWidth.Name = "lbMainCountyQJWidth";
            this.lbMainCountyQJWidth.Size = new System.Drawing.Size(41, 12);
            this.lbMainCountyQJWidth.TabIndex = 30;
            this.lbMainCountyQJWidth.Tag = " ";
            this.lbMainCountyQJWidth.Text = "县级：";
            // 
            // lbMainStateQJWidth
            // 
            this.lbMainStateQJWidth.AutoSize = true;
            this.lbMainStateQJWidth.Location = new System.Drawing.Point(119, 33);
            this.lbMainStateQJWidth.Name = "lbMainStateQJWidth";
            this.lbMainStateQJWidth.Size = new System.Drawing.Size(41, 12);
            this.lbMainStateQJWidth.TabIndex = 28;
            this.lbMainStateQJWidth.Tag = " ";
            this.lbMainStateQJWidth.Text = "地级：";
            // 
            // lbMainProvinceQJWidth
            // 
            this.lbMainProvinceQJWidth.AutoSize = true;
            this.lbMainProvinceQJWidth.Location = new System.Drawing.Point(14, 33);
            this.lbMainProvinceQJWidth.Name = "lbMainProvinceQJWidth";
            this.lbMainProvinceQJWidth.Size = new System.Drawing.Size(41, 12);
            this.lbMainProvinceQJWidth.TabIndex = 26;
            this.lbMainProvinceQJWidth.Tag = " ";
            this.lbMainProvinceQJWidth.Text = "省级：";
            // 
            // btMainQJColor
            // 
            this.btMainQJColor.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btMainQJColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btMainQJColor.Location = new System.Drawing.Point(371, 25);
            this.btMainQJColor.Margin = new System.Windows.Forms.Padding(2);
            this.btMainQJColor.Name = "btMainQJColor";
            this.btMainQJColor.Size = new System.Drawing.Size(68, 18);
            this.btMainQJColor.TabIndex = 23;
            this.btMainQJColor.UseVisualStyleBackColor = false;
            this.btMainQJColor.Click += new System.EventHandler(this.btMainQJColor_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(301, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 22;
            this.label3.Tag = " ";
            this.label3.Text = "骑界颜色：";
            // 
            // cbMainBOULSQL
            // 
            this.cbMainBOULSQL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMainBOULSQL.FormattingEnabled = true;
            this.cbMainBOULSQL.Location = new System.Drawing.Point(89, 22);
            this.cbMainBOULSQL.Name = "cbMainBOULSQL";
            this.cbMainBOULSQL.Size = new System.Drawing.Size(98, 20);
            this.cbMainBOULSQL.TabIndex = 11;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(193, 28);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(41, 12);
            this.label19.TabIndex = 10;
            this.label19.Text = "及以上";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "生成范围：";
            // 
            // cbMainQJ
            // 
            this.cbMainQJ.AutoSize = true;
            this.cbMainQJ.Location = new System.Drawing.Point(7, 4);
            this.cbMainQJ.Name = "cbMainQJ";
            this.cbMainQJ.Size = new System.Drawing.Size(72, 16);
            this.cbMainQJ.TabIndex = 14;
            this.cbMainQJ.Text = "主区骑界";
            this.cbMainQJ.UseVisualStyleBackColor = true;
            this.cbMainQJ.CheckedChanged += new System.EventHandler(this.cbMainQJ_CheckedChanged);
            // 
            // panelAdj
            // 
            this.panelAdj.Controls.Add(this.gbAdj);
            this.panelAdj.Controls.Add(this.cbAdjQJ);
            this.panelAdj.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAdj.Location = new System.Drawing.Point(5, 172);
            this.panelAdj.Name = "panelAdj";
            this.panelAdj.Size = new System.Drawing.Size(464, 164);
            this.panelAdj.TabIndex = 15;
            // 
            // gbAdj
            // 
            this.gbAdj.Controls.Add(this.groupBox3);
            this.gbAdj.Controls.Add(this.btAdjQJColor);
            this.gbAdj.Controls.Add(this.label8);
            this.gbAdj.Controls.Add(this.cbAdjBOULSQL);
            this.gbAdj.Controls.Add(this.label9);
            this.gbAdj.Controls.Add(this.label10);
            this.gbAdj.Location = new System.Drawing.Point(7, 23);
            this.gbAdj.Name = "gbAdj";
            this.gbAdj.Size = new System.Drawing.Size(451, 135);
            this.gbAdj.TabIndex = 15;
            this.gbAdj.TabStop = false;
            this.gbAdj.Text = "邻区骑界";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tbAdjProvinceQJWidth);
            this.groupBox3.Controls.Add(this.tbAdjCountyQJWidth);
            this.groupBox3.Controls.Add(this.tbAdjStateQJWidth);
            this.groupBox3.Controls.Add(this.tbAdjTownQJWidth);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Location = new System.Drawing.Point(6, 58);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(433, 65);
            this.groupBox3.TabIndex = 30;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "骑界宽度(单位：mm)";
            // 
            // tbAdjProvinceQJWidth
            // 
            this.tbAdjProvinceQJWidth.Location = new System.Drawing.Point(49, 29);
            this.tbAdjProvinceQJWidth.Name = "tbAdjProvinceQJWidth";
            this.tbAdjProvinceQJWidth.Size = new System.Drawing.Size(55, 21);
            this.tbAdjProvinceQJWidth.TabIndex = 27;
            this.tbAdjProvinceQJWidth.Text = "3.5";
            // 
            // tbAdjCountyQJWidth
            // 
            this.tbAdjCountyQJWidth.Location = new System.Drawing.Point(263, 29);
            this.tbAdjCountyQJWidth.Name = "tbAdjCountyQJWidth";
            this.tbAdjCountyQJWidth.Size = new System.Drawing.Size(55, 21);
            this.tbAdjCountyQJWidth.TabIndex = 31;
            this.tbAdjCountyQJWidth.Text = "1.5";
            // 
            // tbAdjStateQJWidth
            // 
            this.tbAdjStateQJWidth.Location = new System.Drawing.Point(155, 29);
            this.tbAdjStateQJWidth.Name = "tbAdjStateQJWidth";
            this.tbAdjStateQJWidth.Size = new System.Drawing.Size(55, 21);
            this.tbAdjStateQJWidth.TabIndex = 29;
            this.tbAdjStateQJWidth.Text = "2.5";
            // 
            // tbAdjTownQJWidth
            // 
            this.tbAdjTownQJWidth.Location = new System.Drawing.Point(368, 29);
            this.tbAdjTownQJWidth.Name = "tbAdjTownQJWidth";
            this.tbAdjTownQJWidth.Size = new System.Drawing.Size(55, 21);
            this.tbAdjTownQJWidth.TabIndex = 33;
            this.tbAdjTownQJWidth.Text = "1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(333, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 32;
            this.label4.Tag = " ";
            this.label4.Text = "乡级：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(228, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 30;
            this.label5.Tag = " ";
            this.label5.Text = "县级：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(119, 33);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 28;
            this.label6.Tag = " ";
            this.label6.Text = "地级：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 26;
            this.label7.Tag = " ";
            this.label7.Text = "省级：";
            // 
            // btAdjQJColor
            // 
            this.btAdjQJColor.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btAdjQJColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btAdjQJColor.Location = new System.Drawing.Point(371, 25);
            this.btAdjQJColor.Margin = new System.Windows.Forms.Padding(2);
            this.btAdjQJColor.Name = "btAdjQJColor";
            this.btAdjQJColor.Size = new System.Drawing.Size(68, 18);
            this.btAdjQJColor.TabIndex = 23;
            this.btAdjQJColor.UseVisualStyleBackColor = false;
            this.btAdjQJColor.Click += new System.EventHandler(this.btAdjQJColor_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(301, 28);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 22;
            this.label8.Tag = " ";
            this.label8.Text = "骑界颜色：";
            // 
            // cbAdjBOULSQL
            // 
            this.cbAdjBOULSQL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAdjBOULSQL.FormattingEnabled = true;
            this.cbAdjBOULSQL.Location = new System.Drawing.Point(89, 22);
            this.cbAdjBOULSQL.Name = "cbAdjBOULSQL";
            this.cbAdjBOULSQL.Size = new System.Drawing.Size(98, 20);
            this.cbAdjBOULSQL.TabIndex = 11;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(193, 28);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 12);
            this.label9.TabIndex = 10;
            this.label9.Text = "及以上";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 28);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 12);
            this.label10.TabIndex = 9;
            this.label10.Text = "生成范围：";
            // 
            // cbAdjQJ
            // 
            this.cbAdjQJ.AutoSize = true;
            this.cbAdjQJ.Checked = true;
            this.cbAdjQJ.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAdjQJ.Location = new System.Drawing.Point(7, 4);
            this.cbAdjQJ.Name = "cbAdjQJ";
            this.cbAdjQJ.Size = new System.Drawing.Size(72, 16);
            this.cbAdjQJ.TabIndex = 14;
            this.cbAdjQJ.Text = "邻区骑界";
            this.cbAdjQJ.UseVisualStyleBackColor = true;
            this.cbAdjQJ.Click += new System.EventHandler(this.cbAdjQJ_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(5, 336);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(464, 31);
            this.panel1.TabIndex = 16;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(323, 4);
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
            this.panel2.Location = new System.Drawing.Point(387, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(396, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // NormalQJForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 371);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelAdj);
            this.Controls.Add(this.panel3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NormalQJForm";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "常规骑界";
            this.Load += new System.EventHandler(this.NormalQJForm_Load);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.gbMain.ResumeLayout(false);
            this.gbMain.PerformLayout();
            this.gbDisEx.ResumeLayout(false);
            this.gbDisEx.PerformLayout();
            this.panelAdj.ResumeLayout(false);
            this.panelAdj.PerformLayout();
            this.gbAdj.ResumeLayout(false);
            this.gbAdj.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox gbMain;
        private System.Windows.Forms.GroupBox gbDisEx;
        public System.Windows.Forms.TextBox tbMainProvinceQJWidth;
        public System.Windows.Forms.TextBox tbMainCountyQJWidth;
        public System.Windows.Forms.TextBox tbMainStateQJWidth;
        public System.Windows.Forms.TextBox tbMainTownQJWidth;
        private System.Windows.Forms.Label lbMainTownQJWidth;
        private System.Windows.Forms.Label lbMainCountyQJWidth;
        private System.Windows.Forms.Label lbMainStateQJWidth;
        private System.Windows.Forms.Label lbMainProvinceQJWidth;
        private System.Windows.Forms.Button btMainQJColor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbMainBOULSQL;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbMainQJ;
        private System.Windows.Forms.Panel panelAdj;
        private System.Windows.Forms.GroupBox gbAdj;
        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.TextBox tbAdjProvinceQJWidth;
        public System.Windows.Forms.TextBox tbAdjCountyQJWidth;
        public System.Windows.Forms.TextBox tbAdjStateQJWidth;
        public System.Windows.Forms.TextBox tbAdjTownQJWidth;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btAdjQJColor;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbAdjBOULSQL;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox cbAdjQJ;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;

    }
}
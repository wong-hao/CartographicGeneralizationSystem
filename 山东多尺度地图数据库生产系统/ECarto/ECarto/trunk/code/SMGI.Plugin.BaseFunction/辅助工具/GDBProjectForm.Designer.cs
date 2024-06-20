namespace SMGI.Plugin.BaseFunction
{
    partial class GDBProjectForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.listBox_project_GDB = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbProjectionFileList = new System.Windows.Forms.ComboBox();
            this.buttonOutput = new System.Windows.Forms.Button();
            this.ReprjPositionTextBox = new System.Windows.Forms.TextBox();
            this.buttonOpenPro = new System.Windows.Forms.Button();
            this.ProjectOK = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lbInfo = new System.Windows.Forms.Label();
            this.rbImg = new System.Windows.Forms.RadioButton();
            this.rbGDB = new System.Windows.Forms.RadioButton();
            this.rbMdb = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ks = new System.Windows.Forms.TextBox();
            this.zr = new System.Windows.Forms.TextBox();
            this.yr = new System.Windows.Forms.TextBox();
            this.xr = new System.Windows.Forms.TextBox();
            this.zm = new System.Windows.Forms.TextBox();
            this.ym = new System.Windows.Forms.TextBox();
            this.xm = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkpara = new System.Windows.Forms.CheckBox();
            this.btnDel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDel);
            this.groupBox1.Controls.Add(this.buttonOpen);
            this.groupBox1.Controls.Add(this.listBox_project_GDB);
            this.groupBox1.Location = new System.Drawing.Point(12, 88);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 200);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "需要投影的数据";
            // 
            // buttonOpen
            // 
            this.buttonOpen.Location = new System.Drawing.Point(398, 20);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(46, 23);
            this.buttonOpen.TabIndex = 1;
            this.buttonOpen.Text = "浏览";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // listBox_project_GDB
            // 
            this.listBox_project_GDB.FormattingEnabled = true;
            this.listBox_project_GDB.ItemHeight = 12;
            this.listBox_project_GDB.Location = new System.Drawing.Point(6, 20);
            this.listBox_project_GDB.Name = "listBox_project_GDB";
            this.listBox_project_GDB.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBox_project_GDB.Size = new System.Drawing.Size(386, 160);
            this.listBox_project_GDB.TabIndex = 0;
            this.listBox_project_GDB.SelectedValueChanged += new System.EventHandler(this.listBox_project_GDB_SelectedValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbProjectionFileList);
            this.groupBox2.Controls.Add(this.buttonOutput);
            this.groupBox2.Controls.Add(this.ReprjPositionTextBox);
            this.groupBox2.Controls.Add(this.buttonOpenPro);
            this.groupBox2.Location = new System.Drawing.Point(12, 290);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(458, 95);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "投影信息";
            // 
            // cbProjectionFileList
            // 
            this.cbProjectionFileList.FormattingEnabled = true;
            this.cbProjectionFileList.Location = new System.Drawing.Point(7, 21);
            this.cbProjectionFileList.Name = "cbProjectionFileList";
            this.cbProjectionFileList.Size = new System.Drawing.Size(356, 20);
            this.cbProjectionFileList.TabIndex = 4;
            // 
            // buttonOutput
            // 
            this.buttonOutput.Location = new System.Drawing.Point(369, 56);
            this.buttonOutput.Name = "buttonOutput";
            this.buttonOutput.Size = new System.Drawing.Size(75, 23);
            this.buttonOutput.TabIndex = 3;
            this.buttonOutput.Text = "输出位置";
            this.buttonOutput.UseVisualStyleBackColor = true;
            this.buttonOutput.Click += new System.EventHandler(this.buttonOutput_Click);
            // 
            // ReprjPositionTextBox
            // 
            this.ReprjPositionTextBox.Location = new System.Drawing.Point(6, 58);
            this.ReprjPositionTextBox.Name = "ReprjPositionTextBox";
            this.ReprjPositionTextBox.Size = new System.Drawing.Size(357, 21);
            this.ReprjPositionTextBox.TabIndex = 2;
            // 
            // buttonOpenPro
            // 
            this.buttonOpenPro.Location = new System.Drawing.Point(369, 19);
            this.buttonOpenPro.Name = "buttonOpenPro";
            this.buttonOpenPro.Size = new System.Drawing.Size(75, 23);
            this.buttonOpenPro.TabIndex = 1;
            this.buttonOpenPro.Text = "浏览投影";
            this.buttonOpenPro.UseVisualStyleBackColor = true;
            this.buttonOpenPro.Click += new System.EventHandler(this.buttonOpenPro_Click);
            // 
            // ProjectOK
            // 
            this.ProjectOK.Location = new System.Drawing.Point(311, 518);
            this.ProjectOK.Name = "ProjectOK";
            this.ProjectOK.Size = new System.Drawing.Size(75, 23);
            this.ProjectOK.TabIndex = 8;
            this.ProjectOK.Text = "确定";
            this.ProjectOK.UseVisualStyleBackColor = true;
            this.ProjectOK.Click += new System.EventHandler(this.ProjectOK_Click);
            // 
            // button5
            // 
            this.button5.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button5.Location = new System.Drawing.Point(392, 518);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 9;
            this.button5.Text = "取消";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lbInfo);
            this.groupBox4.Controls.Add(this.rbImg);
            this.groupBox4.Controls.Add(this.rbGDB);
            this.groupBox4.Controls.Add(this.rbMdb);
            this.groupBox4.Location = new System.Drawing.Point(12, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(455, 70);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "数据类型";
            // 
            // lbInfo
            // 
            this.lbInfo.AutoSize = true;
            this.lbInfo.Location = new System.Drawing.Point(104, 46);
            this.lbInfo.Name = "lbInfo";
            this.lbInfo.Size = new System.Drawing.Size(0, 12);
            this.lbInfo.TabIndex = 26;
            // 
            // rbImg
            // 
            this.rbImg.AutoSize = true;
            this.rbImg.BackColor = System.Drawing.Color.Transparent;
            this.rbImg.Location = new System.Drawing.Point(344, 25);
            this.rbImg.Name = "rbImg";
            this.rbImg.Size = new System.Drawing.Size(47, 16);
            this.rbImg.TabIndex = 25;
            this.rbImg.Text = "影像";
            this.rbImg.UseVisualStyleBackColor = false;
            this.rbImg.CheckedChanged += new System.EventHandler(this.rbImg_CheckedChanged);
            // 
            // rbGDB
            // 
            this.rbGDB.AutoSize = true;
            this.rbGDB.BackColor = System.Drawing.Color.Transparent;
            this.rbGDB.Checked = true;
            this.rbGDB.Location = new System.Drawing.Point(188, 25);
            this.rbGDB.Name = "rbGDB";
            this.rbGDB.Size = new System.Drawing.Size(41, 16);
            this.rbGDB.TabIndex = 24;
            this.rbGDB.TabStop = true;
            this.rbGDB.Text = "GDB";
            this.rbGDB.UseVisualStyleBackColor = false;
            this.rbGDB.CheckedChanged += new System.EventHandler(this.rbImg_CheckedChanged);
            // 
            // rbMdb
            // 
            this.rbMdb.AutoSize = true;
            this.rbMdb.BackColor = System.Drawing.Color.Transparent;
            this.rbMdb.Location = new System.Drawing.Point(33, 25);
            this.rbMdb.Name = "rbMdb";
            this.rbMdb.Size = new System.Drawing.Size(41, 16);
            this.rbMdb.TabIndex = 23;
            this.rbMdb.Text = "MDB";
            this.rbMdb.UseVisualStyleBackColor = false;
            this.rbMdb.CheckedChanged += new System.EventHandler(this.rbImg_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ks);
            this.groupBox3.Controls.Add(this.zr);
            this.groupBox3.Controls.Add(this.yr);
            this.groupBox3.Controls.Add(this.xr);
            this.groupBox3.Controls.Add(this.zm);
            this.groupBox3.Controls.Add(this.ym);
            this.groupBox3.Controls.Add(this.xm);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(12, 392);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(455, 109);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "投影参数";
            // 
            // ks
            // 
            this.ks.Location = new System.Drawing.Point(59, 77);
            this.ks.Name = "ks";
            this.ks.Size = new System.Drawing.Size(65, 21);
            this.ks.TabIndex = 13;
            this.ks.Text = "0.0";
            // 
            // zr
            // 
            this.zr.Location = new System.Drawing.Point(360, 45);
            this.zr.Name = "zr";
            this.zr.Size = new System.Drawing.Size(52, 21);
            this.zr.TabIndex = 12;
            this.zr.Text = "0.0";
            // 
            // yr
            // 
            this.yr.Location = new System.Drawing.Point(211, 45);
            this.yr.Name = "yr";
            this.yr.Size = new System.Drawing.Size(67, 21);
            this.yr.TabIndex = 11;
            this.yr.Text = "0.0";
            // 
            // xr
            // 
            this.xr.Location = new System.Drawing.Point(59, 46);
            this.xr.Name = "xr";
            this.xr.Size = new System.Drawing.Size(65, 21);
            this.xr.TabIndex = 10;
            this.xr.Text = "0.0";
            // 
            // zm
            // 
            this.zm.Location = new System.Drawing.Point(359, 17);
            this.zm.Name = "zm";
            this.zm.Size = new System.Drawing.Size(52, 21);
            this.zm.TabIndex = 9;
            this.zm.Text = "0.0";
            // 
            // ym
            // 
            this.ym.Location = new System.Drawing.Point(211, 19);
            this.ym.Name = "ym";
            this.ym.Size = new System.Drawing.Size(67, 21);
            this.ym.TabIndex = 8;
            this.ym.Text = "0.0";
            // 
            // xm
            // 
            this.xm.Location = new System.Drawing.Point(57, 19);
            this.xm.Name = "xm";
            this.xm.Size = new System.Drawing.Size(67, 21);
            this.xm.TabIndex = 7;
            this.xm.Text = "0.0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 12);
            this.label7.TabIndex = 6;
            this.label7.Text = "K缩放：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(313, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 12);
            this.label6.TabIndex = 5;
            this.label6.Text = "Z旋转：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(165, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "Y旋转：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "X旋转：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(311, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "Z平移：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(163, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Y平移：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "X平移：";
            // 
            // checkpara
            // 
            this.checkpara.AutoSize = true;
            this.checkpara.Location = new System.Drawing.Point(23, 518);
            this.checkpara.Name = "checkpara";
            this.checkpara.Size = new System.Drawing.Size(132, 16);
            this.checkpara.TabIndex = 16;
            this.checkpara.Text = "是否自定义投影参数";
            this.checkpara.UseVisualStyleBackColor = true;
            // 
            // btnDel
            // 
            this.btnDel.Enabled = false;
            this.btnDel.Location = new System.Drawing.Point(398, 49);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(46, 23);
            this.btnDel.TabIndex = 1;
            this.btnDel.Text = "移除";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // GDBProjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 560);
            this.Controls.Add(this.checkpara);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.ProjectOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "GDBProjectForm";
            this.Text = "投影";
            this.Load += new System.EventHandler(this.FrmProjectGDB_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.ListBox listBox_project_GDB;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonOutput;
        private System.Windows.Forms.TextBox ReprjPositionTextBox;
        private System.Windows.Forms.Button buttonOpenPro;
        private System.Windows.Forms.Button ProjectOK;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.ComboBox cbProjectionFileList;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lbInfo;
        private System.Windows.Forms.RadioButton rbImg;
        private System.Windows.Forms.RadioButton rbGDB;
        private System.Windows.Forms.RadioButton rbMdb;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox ks;
        private System.Windows.Forms.TextBox zr;
        private System.Windows.Forms.TextBox yr;
        private System.Windows.Forms.TextBox xr;
        private System.Windows.Forms.TextBox zm;
        private System.Windows.Forms.TextBox ym;
        private System.Windows.Forms.TextBox xm;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkpara;
        private System.Windows.Forms.Button btnDel;
    }
}
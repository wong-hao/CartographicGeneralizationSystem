namespace SMGI.Plugin.CollaborativeWork
{
    partial class ExportDataBetweenVersionForm
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
            this.wizardExportDataBetweenVersion = new DevExpress.XtraWizard.WizardControl();
            this.ServerDBConnPage = new DevExpress.XtraWizard.WizardPage();
            this.gbServer = new System.Windows.Forms.GroupBox();
            this.cbShowPassword = new System.Windows.Forms.CheckBox();
            this.cbRemeberPassword = new System.Windows.Forms.CheckBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.tbUserName = new System.Windows.Forms.TextBox();
            this.tbDataBase = new System.Windows.Forms.TextBox();
            this.tbIPAdress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ExportOptionPage = new DevExpress.XtraWizard.WizardPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.tboutputGDB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbFieldNameUpper = new System.Windows.Forms.CheckBox();
            this.gbRange = new System.Windows.Forms.GroupBox();
            this.cbRange = new System.Windows.Forms.CheckBox();
            this.btnShpFile = new System.Windows.Forms.Button();
            this.tbShpFileName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbBoxEndVersion = new System.Windows.Forms.ComboBox();
            this.cmbBoxStartVersion = new System.Windows.Forms.ComboBox();
            this.cbEndVersion = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.wizardExportDataBetweenVersion)).BeginInit();
            this.wizardExportDataBetweenVersion.SuspendLayout();
            this.ServerDBConnPage.SuspendLayout();
            this.gbServer.SuspendLayout();
            this.ExportOptionPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gbRange.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardExportDataBetweenVersion
            // 
            this.wizardExportDataBetweenVersion.AllowDrop = true;
            this.wizardExportDataBetweenVersion.CancelText = "取消";
            this.wizardExportDataBetweenVersion.Controls.Add(this.ServerDBConnPage);
            this.wizardExportDataBetweenVersion.Controls.Add(this.ExportOptionPage);
            this.wizardExportDataBetweenVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardExportDataBetweenVersion.FinishText = "&确定";
            this.wizardExportDataBetweenVersion.HeaderImage = global::SMGI.Plugin.CollaborativeWork.Properties.Resources.导出数据;
            this.wizardExportDataBetweenVersion.Location = new System.Drawing.Point(0, 0);
            this.wizardExportDataBetweenVersion.Name = "wizardExportDataBetweenVersion";
            this.wizardExportDataBetweenVersion.NextText = "&下一步 >";
            this.wizardExportDataBetweenVersion.Pages.AddRange(new DevExpress.XtraWizard.BaseWizardPage[] {
            this.ServerDBConnPage,
            this.ExportOptionPage});
            this.wizardExportDataBetweenVersion.PreviousText = "< &上一步";
            this.wizardExportDataBetweenVersion.Size = new System.Drawing.Size(501, 455);
            this.wizardExportDataBetweenVersion.SelectedPageChanged += new DevExpress.XtraWizard.WizardPageChangedEventHandler(this.wizardExportData_SelectedPageChanged);
            this.wizardExportDataBetweenVersion.CancelClick += new System.ComponentModel.CancelEventHandler(this.wizardExportData_CancelClick);
            this.wizardExportDataBetweenVersion.FinishClick += new System.ComponentModel.CancelEventHandler(this.wizardExportData_FinishClick);
            this.wizardExportDataBetweenVersion.NextClick += new DevExpress.XtraWizard.WizardCommandButtonClickEventHandler(this.wizardExportData_NextClick);
            // 
            // ServerDBConnPage
            // 
            this.ServerDBConnPage.AllowDrop = true;
            this.ServerDBConnPage.Controls.Add(this.gbServer);
            this.ServerDBConnPage.DescriptionText = "输入服务器数据库信息，连接服务器数据库";
            this.ServerDBConnPage.Name = "ServerDBConnPage";
            this.ServerDBConnPage.Size = new System.Drawing.Size(469, 310);
            this.ServerDBConnPage.Text = "服务器数据库";
            // 
            // gbServer
            // 
            this.gbServer.Controls.Add(this.cbShowPassword);
            this.gbServer.Controls.Add(this.cbRemeberPassword);
            this.gbServer.Controls.Add(this.tbPassword);
            this.gbServer.Controls.Add(this.tbUserName);
            this.gbServer.Controls.Add(this.tbDataBase);
            this.gbServer.Controls.Add(this.tbIPAdress);
            this.gbServer.Controls.Add(this.label5);
            this.gbServer.Controls.Add(this.label3);
            this.gbServer.Controls.Add(this.label2);
            this.gbServer.Controls.Add(this.label4);
            this.gbServer.Location = new System.Drawing.Point(3, 14);
            this.gbServer.Name = "gbServer";
            this.gbServer.Size = new System.Drawing.Size(463, 261);
            this.gbServer.TabIndex = 20;
            this.gbServer.TabStop = false;
            this.gbServer.Text = "数据服务器";
            // 
            // cbShowPassword
            // 
            this.cbShowPassword.AutoSize = true;
            this.cbShowPassword.Location = new System.Drawing.Point(118, 232);
            this.cbShowPassword.Name = "cbShowPassword";
            this.cbShowPassword.Size = new System.Drawing.Size(72, 16);
            this.cbShowPassword.TabIndex = 15;
            this.cbShowPassword.Text = "显示密码";
            this.cbShowPassword.UseVisualStyleBackColor = true;
            this.cbShowPassword.CheckedChanged += new System.EventHandler(this.cbShowPassword_CheckedChanged);
            // 
            // cbRemeberPassword
            // 
            this.cbRemeberPassword.AutoSize = true;
            this.cbRemeberPassword.Checked = true;
            this.cbRemeberPassword.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRemeberPassword.Location = new System.Drawing.Point(26, 232);
            this.cbRemeberPassword.Name = "cbRemeberPassword";
            this.cbRemeberPassword.Size = new System.Drawing.Size(72, 16);
            this.cbRemeberPassword.TabIndex = 14;
            this.cbRemeberPassword.Text = "记住密码";
            this.cbRemeberPassword.UseVisualStyleBackColor = true;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(26, 188);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '●';
            this.tbPassword.Size = new System.Drawing.Size(431, 21);
            this.tbPassword.TabIndex = 12;
            // 
            // tbUserName
            // 
            this.tbUserName.Location = new System.Drawing.Point(27, 140);
            this.tbUserName.Name = "tbUserName";
            this.tbUserName.Size = new System.Drawing.Size(430, 21);
            this.tbUserName.TabIndex = 11;
            // 
            // tbDataBase
            // 
            this.tbDataBase.Location = new System.Drawing.Point(27, 88);
            this.tbDataBase.Name = "tbDataBase";
            this.tbDataBase.Size = new System.Drawing.Size(430, 21);
            this.tbDataBase.TabIndex = 10;
            // 
            // tbIPAdress
            // 
            this.tbIPAdress.Location = new System.Drawing.Point(27, 38);
            this.tbIPAdress.Name = "tbIPAdress";
            this.tbIPAdress.Size = new System.Drawing.Size(430, 21);
            this.tbIPAdress.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 173);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "密码";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "用户名";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "数据库名";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "服务器名或IP地址";
            // 
            // ExportOptionPage
            // 
            this.ExportOptionPage.Controls.Add(this.groupBox2);
            this.ExportOptionPage.Controls.Add(this.gbRange);
            this.ExportOptionPage.Controls.Add(this.groupBox1);
            this.ExportOptionPage.DescriptionText = "根据用户指定起始版本和终止版本信息，导出版本间增量数据到指定位置";
            this.ExportOptionPage.Name = "ExportOptionPage";
            this.ExportOptionPage.Size = new System.Drawing.Size(469, 310);
            this.ExportOptionPage.Text = "导出版本间增量数据";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSave);
            this.groupBox2.Controls.Add(this.tboutputGDB);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.cbFieldNameUpper);
            this.groupBox2.Location = new System.Drawing.Point(4, 224);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(462, 79);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "输出信息";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(417, 44);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(39, 23);
            this.btnSave.TabIndex = 36;
            this.btnSave.Text = "选择";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // tboutputGDB
            // 
            this.tboutputGDB.Location = new System.Drawing.Point(74, 46);
            this.tboutputGDB.Name = "tboutputGDB";
            this.tboutputGDB.ReadOnly = true;
            this.tboutputGDB.Size = new System.Drawing.Size(337, 21);
            this.tboutputGDB.TabIndex = 35;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 34;
            this.label1.Text = "输出位置:";
            // 
            // cbFieldNameUpper
            // 
            this.cbFieldNameUpper.AutoSize = true;
            this.cbFieldNameUpper.Location = new System.Drawing.Point(6, 20);
            this.cbFieldNameUpper.Name = "cbFieldNameUpper";
            this.cbFieldNameUpper.Size = new System.Drawing.Size(120, 16);
            this.cbFieldNameUpper.TabIndex = 33;
            this.cbFieldNameUpper.Text = "字段名转换为大写";
            this.cbFieldNameUpper.UseVisualStyleBackColor = true;
            // 
            // gbRange
            // 
            this.gbRange.Controls.Add(this.cbRange);
            this.gbRange.Controls.Add(this.btnShpFile);
            this.gbRange.Controls.Add(this.tbShpFileName);
            this.gbRange.Controls.Add(this.label6);
            this.gbRange.Location = new System.Drawing.Point(4, 131);
            this.gbRange.Name = "gbRange";
            this.gbRange.Size = new System.Drawing.Size(462, 84);
            this.gbRange.TabIndex = 20;
            this.gbRange.TabStop = false;
            this.gbRange.Text = "提取范围";
            // 
            // cbRange
            // 
            this.cbRange.AutoSize = true;
            this.cbRange.Location = new System.Drawing.Point(11, 21);
            this.cbRange.Name = "cbRange";
            this.cbRange.Size = new System.Drawing.Size(96, 16);
            this.cbRange.TabIndex = 12;
            this.cbRange.Text = "指定提取范围";
            this.cbRange.UseVisualStyleBackColor = true;
            this.cbRange.CheckedChanged += new System.EventHandler(this.cbRange_CheckedChanged);
            // 
            // btnShpFile
            // 
            this.btnShpFile.Enabled = false;
            this.btnShpFile.Location = new System.Drawing.Point(417, 44);
            this.btnShpFile.Name = "btnShpFile";
            this.btnShpFile.Size = new System.Drawing.Size(39, 23);
            this.btnShpFile.TabIndex = 11;
            this.btnShpFile.Text = "选择";
            this.btnShpFile.UseVisualStyleBackColor = true;
            this.btnShpFile.Click += new System.EventHandler(this.btnShpFile_Click);
            // 
            // tbShpFileName
            // 
            this.tbShpFileName.Location = new System.Drawing.Point(74, 46);
            this.tbShpFileName.Name = "tbShpFileName";
            this.tbShpFileName.ReadOnly = true;
            this.tbShpFileName.Size = new System.Drawing.Size(337, 21);
            this.tbShpFileName.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 12);
            this.label6.TabIndex = 9;
            this.label6.Text = "范围文件:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbBoxEndVersion);
            this.groupBox1.Controls.Add(this.cmbBoxStartVersion);
            this.groupBox1.Controls.Add(this.cbEndVersion);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(462, 118);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据库版本信息";
            // 
            // cmbBoxEndVersion
            // 
            this.cmbBoxEndVersion.FormattingEnabled = true;
            this.cmbBoxEndVersion.Location = new System.Drawing.Point(11, 91);
            this.cmbBoxEndVersion.Name = "cmbBoxEndVersion";
            this.cmbBoxEndVersion.Size = new System.Drawing.Size(445, 20);
            this.cmbBoxEndVersion.TabIndex = 11;
            // 
            // cmbBoxStartVersion
            // 
            this.cmbBoxStartVersion.FormattingEnabled = true;
            this.cmbBoxStartVersion.Location = new System.Drawing.Point(11, 36);
            this.cmbBoxStartVersion.Name = "cmbBoxStartVersion";
            this.cmbBoxStartVersion.Size = new System.Drawing.Size(445, 20);
            this.cmbBoxStartVersion.TabIndex = 11;
            // 
            // cbEndVersion
            // 
            this.cbEndVersion.AutoSize = true;
            this.cbEndVersion.Checked = true;
            this.cbEndVersion.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEndVersion.Location = new System.Drawing.Point(11, 69);
            this.cbEndVersion.Name = "cbEndVersion";
            this.cbEndVersion.Size = new System.Drawing.Size(306, 16);
            this.cbEndVersion.TabIndex = 10;
            this.cbEndVersion.Text = "终止版本(不指定,则默认为服务器数据库的最大版本)";
            this.cbEndVersion.UseVisualStyleBackColor = true;
            this.cbEndVersion.CheckedChanged += new System.EventHandler(this.cbEndVersion_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 17);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 9;
            this.label7.Text = "起始版本";
            // 
            // ExportDataBetweenVersionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 455);
            this.Controls.Add(this.wizardExportDataBetweenVersion);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportDataBetweenVersionForm";
            this.Text = "导出版本间增量数据";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.DownLoadVersionDataForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wizardExportDataBetweenVersion)).EndInit();
            this.wizardExportDataBetweenVersion.ResumeLayout(false);
            this.ServerDBConnPage.ResumeLayout(false);
            this.gbServer.ResumeLayout(false);
            this.gbServer.PerformLayout();
            this.ExportOptionPage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.gbRange.ResumeLayout(false);
            this.gbRange.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraWizard.WizardControl wizardExportDataBetweenVersion;
        private DevExpress.XtraWizard.WizardPage ServerDBConnPage;
        private DevExpress.XtraWizard.WizardPage ExportOptionPage;
        private System.Windows.Forms.GroupBox gbServer;
        private System.Windows.Forms.CheckBox cbShowPassword;
        private System.Windows.Forms.CheckBox cbRemeberPassword;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.TextBox tbUserName;
        private System.Windows.Forms.TextBox tbDataBase;
        private System.Windows.Forms.TextBox tbIPAdress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gbRange;
        private System.Windows.Forms.CheckBox cbRange;
        private System.Windows.Forms.Button btnShpFile;
        private System.Windows.Forms.TextBox tbShpFileName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox cbFieldNameUpper;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox tboutputGDB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbBoxEndVersion;
        private System.Windows.Forms.ComboBox cmbBoxStartVersion;
        private System.Windows.Forms.CheckBox cbEndVersion;
        private System.Windows.Forms.Label label7;


    }
}
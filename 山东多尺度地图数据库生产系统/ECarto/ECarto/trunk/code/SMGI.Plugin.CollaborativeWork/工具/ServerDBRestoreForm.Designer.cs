namespace SMGI.Plugin.CollaborativeWork
{
    partial class ServerDBRestoreForm
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
            this.wizardDBRestroe = new DevExpress.XtraWizard.WizardControl();
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
            this.VesionInfoPage = new DevExpress.XtraWizard.WizardPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.serverDBVersionDGV = new System.Windows.Forms.DataGridView();
            this.ServerDBVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SubmitDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OpName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.wizardDBRestroe)).BeginInit();
            this.wizardDBRestroe.SuspendLayout();
            this.ServerDBConnPage.SuspendLayout();
            this.gbServer.SuspendLayout();
            this.VesionInfoPage.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.serverDBVersionDGV)).BeginInit();
            this.SuspendLayout();
            // 
            // wizardDBRestroe
            // 
            this.wizardDBRestroe.CancelText = "取消";
            this.wizardDBRestroe.Controls.Add(this.ServerDBConnPage);
            this.wizardDBRestroe.Controls.Add(this.VesionInfoPage);
            this.wizardDBRestroe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardDBRestroe.FinishText = "&确定";
            this.wizardDBRestroe.HeaderImage = global::SMGI.Plugin.CollaborativeWork.Properties.Resources.导出数据;
            this.wizardDBRestroe.Location = new System.Drawing.Point(0, 0);
            this.wizardDBRestroe.Name = "wizardDBRestroe";
            this.wizardDBRestroe.NextText = "&下一步 >";
            this.wizardDBRestroe.Pages.AddRange(new DevExpress.XtraWizard.BaseWizardPage[] {
            this.ServerDBConnPage,
            this.VesionInfoPage});
            this.wizardDBRestroe.PreviousText = "< &上一步";
            this.wizardDBRestroe.Size = new System.Drawing.Size(501, 455);
            this.wizardDBRestroe.SelectedPageChanged += new DevExpress.XtraWizard.WizardPageChangedEventHandler(this.wizardExportData_SelectedPageChanged);
            this.wizardDBRestroe.CancelClick += new System.ComponentModel.CancelEventHandler(this.wizardExportData_CancelClick);
            this.wizardDBRestroe.FinishClick += new System.ComponentModel.CancelEventHandler(this.wizardExportData_FinishClick);
            this.wizardDBRestroe.NextClick += new DevExpress.XtraWizard.WizardCommandButtonClickEventHandler(this.wizardExportData_NextClick);
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
            // VesionInfoPage
            // 
            this.VesionInfoPage.Controls.Add(this.groupBox1);
            this.VesionInfoPage.DescriptionText = "根据用户指定的数据库版本，将服务器数据库回滚至指定版本，即高于指定版本的所有要素将会被直接删除";
            this.VesionInfoPage.Name = "VesionInfoPage";
            this.VesionInfoPage.Size = new System.Drawing.Size(469, 310);
            this.VesionInfoPage.Text = "服务器数据库版本回滚";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.serverDBVersionDGV);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(462, 303);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据库版本信息";
            // 
            // serverDBVersionDGV
            // 
            this.serverDBVersionDGV.AllowUserToAddRows = false;
            this.serverDBVersionDGV.AllowUserToDeleteRows = false;
            this.serverDBVersionDGV.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.serverDBVersionDGV.BackgroundColor = System.Drawing.SystemColors.Window;
            this.serverDBVersionDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.serverDBVersionDGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ServerDBVersion,
            this.SubmitDesc,
            this.OpName});
            this.serverDBVersionDGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serverDBVersionDGV.GridColor = System.Drawing.SystemColors.AppWorkspace;
            this.serverDBVersionDGV.Location = new System.Drawing.Point(3, 17);
            this.serverDBVersionDGV.MultiSelect = false;
            this.serverDBVersionDGV.Name = "serverDBVersionDGV";
            this.serverDBVersionDGV.RowHeadersVisible = false;
            this.serverDBVersionDGV.RowTemplate.Height = 23;
            this.serverDBVersionDGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.serverDBVersionDGV.Size = new System.Drawing.Size(456, 283);
            this.serverDBVersionDGV.TabIndex = 8;
            this.serverDBVersionDGV.TabStop = false;
            this.serverDBVersionDGV.MouseClick += new System.Windows.Forms.MouseEventHandler(this.serverDBVersionDGV_MouseClick);
            // 
            // ServerDBVersion
            // 
            this.ServerDBVersion.FillWeight = 20F;
            this.ServerDBVersion.HeaderText = "版本号";
            this.ServerDBVersion.Name = "ServerDBVersion";
            this.ServerDBVersion.ReadOnly = true;
            this.ServerDBVersion.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // SubmitDesc
            // 
            this.SubmitDesc.FillWeight = 60F;
            this.SubmitDesc.HeaderText = "描述";
            this.SubmitDesc.Name = "SubmitDesc";
            this.SubmitDesc.ReadOnly = true;
            this.SubmitDesc.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.SubmitDesc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // OpName
            // 
            this.OpName.FillWeight = 20F;
            this.OpName.HeaderText = "操作者";
            this.OpName.Name = "OpName";
            this.OpName.ReadOnly = true;
            this.OpName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ServerDBRestoreForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 455);
            this.Controls.Add(this.wizardDBRestroe);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ServerDBRestoreForm";
            this.Text = "服务器数据库版本回滚";
            this.Load += new System.EventHandler(this.ServerDBRestoreForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wizardDBRestroe)).EndInit();
            this.wizardDBRestroe.ResumeLayout(false);
            this.ServerDBConnPage.ResumeLayout(false);
            this.gbServer.ResumeLayout(false);
            this.gbServer.PerformLayout();
            this.VesionInfoPage.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.serverDBVersionDGV)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraWizard.WizardControl wizardDBRestroe;
        private DevExpress.XtraWizard.WizardPage ServerDBConnPage;
        private DevExpress.XtraWizard.WizardPage VesionInfoPage;
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
        private System.Windows.Forms.DataGridView serverDBVersionDGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn ServerDBVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn SubmitDesc;
        private System.Windows.Forms.DataGridViewTextBoxColumn OpName;


    }
}
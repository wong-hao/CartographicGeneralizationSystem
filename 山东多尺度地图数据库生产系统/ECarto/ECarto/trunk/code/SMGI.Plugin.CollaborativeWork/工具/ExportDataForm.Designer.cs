namespace SMGI.Plugin.CollaborativeWork
{
    partial class ExportDataForm
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
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.tbUserName = new System.Windows.Forms.TextBox();
            this.tbDataBase = new System.Windows.Forms.TextBox();
            this.tbIPAdress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.gbServer = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.tboutputGDB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btCancel = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.gbRange = new System.Windows.Forms.GroupBox();
            this.cbRange = new System.Windows.Forms.CheckBox();
            this.btnShpFile = new System.Windows.Forms.Button();
            this.tbShpFileName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbFieldNameUpper = new System.Windows.Forms.CheckBox();
            this.cbDelCollaField = new System.Windows.Forms.CheckBox();
            this.gbServer.SuspendLayout();
            this.panel1.SuspendLayout();
            this.gbRange.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(135, 131);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '●';
            this.tbPassword.Size = new System.Drawing.Size(165, 21);
            this.tbPassword.TabIndex = 12;
            // 
            // tbUserName
            // 
            this.tbUserName.Location = new System.Drawing.Point(135, 97);
            this.tbUserName.Name = "tbUserName";
            this.tbUserName.Size = new System.Drawing.Size(165, 21);
            this.tbUserName.TabIndex = 11;
            // 
            // tbDataBase
            // 
            this.tbDataBase.Location = new System.Drawing.Point(135, 59);
            this.tbDataBase.Name = "tbDataBase";
            this.tbDataBase.Size = new System.Drawing.Size(165, 21);
            this.tbDataBase.TabIndex = 10;
            // 
            // tbIPAdress
            // 
            this.tbIPAdress.Location = new System.Drawing.Point(135, 23);
            this.tbIPAdress.Name = "tbIPAdress";
            this.tbIPAdress.Size = new System.Drawing.Size(165, 21);
            this.tbIPAdress.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(25, 131);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "密码：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "用户名：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "数据库：";
            // 
            // gbServer
            // 
            this.gbServer.Controls.Add(this.tbPassword);
            this.gbServer.Controls.Add(this.tbUserName);
            this.gbServer.Controls.Add(this.tbDataBase);
            this.gbServer.Controls.Add(this.tbIPAdress);
            this.gbServer.Controls.Add(this.label5);
            this.gbServer.Controls.Add(this.label3);
            this.gbServer.Controls.Add(this.label2);
            this.gbServer.Controls.Add(this.label4);
            this.gbServer.Location = new System.Drawing.Point(16, 13);
            this.gbServer.Name = "gbServer";
            this.gbServer.Size = new System.Drawing.Size(359, 165);
            this.gbServer.TabIndex = 18;
            this.gbServer.TabStop = false;
            this.gbServer.Text = "数据服务器";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "服务器名或IP地址：";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(325, 331);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(39, 23);
            this.btnSave.TabIndex = 17;
            this.btnSave.Text = "...";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // tboutputGDB
            // 
            this.tboutputGDB.Location = new System.Drawing.Point(90, 332);
            this.tboutputGDB.Name = "tboutputGDB";
            this.tboutputGDB.ReadOnly = true;
            this.tboutputGDB.Size = new System.Drawing.Size(226, 21);
            this.tboutputGDB.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 335);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "输出位置:";
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(314, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 368);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(382, 31);
            this.panel1.TabIndex = 14;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(241, 4);
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
            this.panel2.Location = new System.Drawing.Point(305, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // gbRange
            // 
            this.gbRange.Controls.Add(this.cbRange);
            this.gbRange.Controls.Add(this.btnShpFile);
            this.gbRange.Controls.Add(this.tbShpFileName);
            this.gbRange.Controls.Add(this.label6);
            this.gbRange.Location = new System.Drawing.Point(16, 185);
            this.gbRange.Name = "gbRange";
            this.gbRange.Size = new System.Drawing.Size(359, 84);
            this.gbRange.TabIndex = 19;
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
            this.btnShpFile.Location = new System.Drawing.Point(309, 45);
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
            this.tbShpFileName.Size = new System.Drawing.Size(226, 21);
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
            // cbFieldNameUpper
            // 
            this.cbFieldNameUpper.AutoSize = true;
            this.cbFieldNameUpper.Location = new System.Drawing.Point(27, 301);
            this.cbFieldNameUpper.Name = "cbFieldNameUpper";
            this.cbFieldNameUpper.Size = new System.Drawing.Size(120, 16);
            this.cbFieldNameUpper.TabIndex = 20;
            this.cbFieldNameUpper.Text = "字段名转换为大写";
            this.cbFieldNameUpper.UseVisualStyleBackColor = true;
            // 
            // cbDelCollaField
            // 
            this.cbDelCollaField.AutoSize = true;
            this.cbDelCollaField.Checked = true;
            this.cbDelCollaField.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDelCollaField.Location = new System.Drawing.Point(27, 275);
            this.cbDelCollaField.Name = "cbDelCollaField";
            this.cbDelCollaField.Size = new System.Drawing.Size(96, 16);
            this.cbDelCollaField.TabIndex = 20;
            this.cbDelCollaField.Text = "删除协同字段";
            this.cbDelCollaField.UseVisualStyleBackColor = true;
            // 
            // ExportDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 399);
            this.Controls.Add(this.cbDelCollaField);
            this.Controls.Add(this.cbFieldNameUpper);
            this.Controls.Add(this.gbRange);
            this.Controls.Add(this.gbServer);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tboutputGDB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Name = "ExportDataForm";
            this.Text = "数据提取";
            this.gbServer.ResumeLayout(false);
            this.gbServer.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.gbRange.ResumeLayout(false);
            this.gbRange.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.TextBox tbUserName;
        private System.Windows.Forms.TextBox tbDataBase;
        private System.Windows.Forms.TextBox tbIPAdress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox gbServer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox tboutputGDB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox gbRange;
        private System.Windows.Forms.CheckBox cbRange;
        private System.Windows.Forms.Button btnShpFile;
        private System.Windows.Forms.TextBox tbShpFileName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cbFieldNameUpper;
        private System.Windows.Forms.CheckBox cbDelCollaField;

    }
}
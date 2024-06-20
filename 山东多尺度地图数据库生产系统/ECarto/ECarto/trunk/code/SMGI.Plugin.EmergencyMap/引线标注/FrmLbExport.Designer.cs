namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmLbExport
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
            this.btClose = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btView = new System.Windows.Forms.Button();
            this.txtShp = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gbServer = new System.Windows.Forms.GroupBox();
            this.cmbField = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmblyr = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbDb = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rbLocal = new System.Windows.Forms.RadioButton();
            this.rbSDE = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.gbServer.SuspendLayout();
            this.SuspendLayout();
            // 
            // btClose
            // 
            this.btClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btClose.Location = new System.Drawing.Point(404, 194);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(63, 28);
            this.btClose.TabIndex = 5;
            this.btClose.Text = "取消";
            this.btClose.UseVisualStyleBackColor = true;
            this.btClose.Click += new System.EventHandler(this.btClose_Click);
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(334, 194);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(64, 28);
            this.btOk.TabIndex = 4;
            this.btOk.Text = "确定";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tabControl1);
            this.groupBox1.Location = new System.Drawing.Point(12, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(465, 146);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(1, -22);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(463, 160);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btView);
            this.tabPage1.Controls.Add(this.txtShp);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(455, 134);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btView
            // 
            this.btView.Location = new System.Drawing.Point(385, 37);
            this.btView.Name = "btView";
            this.btView.Size = new System.Drawing.Size(64, 23);
            this.btView.TabIndex = 7;
            this.btView.Text = "浏览";
            this.btView.UseVisualStyleBackColor = true;
            this.btView.Click += new System.EventHandler(this.btView_Click);
            // 
            // txtShp
            // 
            this.txtShp.Location = new System.Drawing.Point(77, 37);
            this.txtShp.Name = "txtShp";
            this.txtShp.Size = new System.Drawing.Size(290, 21);
            this.txtShp.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 40);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 12;
            this.label6.Text = "输出结果：";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.gbServer);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(455, 134);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // gbServer
            // 
            this.gbServer.Controls.Add(this.cmbField);
            this.gbServer.Controls.Add(this.label3);
            this.gbServer.Controls.Add(this.cmblyr);
            this.gbServer.Controls.Add(this.label1);
            this.gbServer.Controls.Add(this.cmbDb);
            this.gbServer.Controls.Add(this.label2);
            this.gbServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbServer.Location = new System.Drawing.Point(3, 3);
            this.gbServer.Name = "gbServer";
            this.gbServer.Size = new System.Drawing.Size(449, 128);
            this.gbServer.TabIndex = 21;
            this.gbServer.TabStop = false;
            this.gbServer.Text = "专题数据服务器";
            // 
            // cmbField
            // 
            this.cmbField.FormattingEnabled = true;
            this.cmbField.Location = new System.Drawing.Point(310, 66);
            this.cmbField.Name = "cmbField";
            this.cmbField.Size = new System.Drawing.Size(127, 20);
            this.cmbField.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(232, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "图层字段：";
            // 
            // cmblyr
            // 
            this.cmblyr.FormattingEnabled = true;
            this.cmblyr.Location = new System.Drawing.Point(62, 66);
            this.cmblyr.Name = "cmblyr";
            this.cmblyr.Size = new System.Drawing.Size(141, 20);
            this.cmblyr.TabIndex = 14;
            this.cmblyr.SelectedIndexChanged += new System.EventHandler(this.cmblyr_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 13;
            this.label1.Text = "专题图层：";
            // 
            // cmbDb
            // 
            this.cmbDb.FormattingEnabled = true;
            this.cmbDb.Location = new System.Drawing.Point(62, 30);
            this.cmbDb.Name = "cmbDb";
            this.cmbDb.Size = new System.Drawing.Size(141, 20);
            this.cmbDb.TabIndex = 12;
            this.cmbDb.SelectedIndexChanged += new System.EventHandler(this.cmbDb_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "数据库：";
            // 
            // rbLocal
            // 
            this.rbLocal.AutoSize = true;
            this.rbLocal.Checked = true;
            this.rbLocal.Location = new System.Drawing.Point(113, 12);
            this.rbLocal.Name = "rbLocal";
            this.rbLocal.Size = new System.Drawing.Size(47, 16);
            this.rbLocal.TabIndex = 7;
            this.rbLocal.TabStop = true;
            this.rbLocal.Text = "本地";
            this.rbLocal.UseVisualStyleBackColor = true;
            this.rbLocal.Visible = false;
            this.rbLocal.CheckedChanged += new System.EventHandler(this.rbLocal_CheckedChanged);
            // 
            // rbSDE
            // 
            this.rbSDE.AutoSize = true;
            this.rbSDE.Location = new System.Drawing.Point(217, 12);
            this.rbSDE.Name = "rbSDE";
            this.rbSDE.Size = new System.Drawing.Size(77, 16);
            this.rbSDE.TabIndex = 8;
            this.rbSDE.Text = "SDE服务器";
            this.rbSDE.UseVisualStyleBackColor = true;
            this.rbSDE.Visible = false;
            this.rbSDE.CheckedChanged += new System.EventHandler(this.rbSDE_CheckedChanged);
            // 
            // FrmLbExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(489, 229);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.rbSDE);
            this.Controls.Add(this.rbLocal);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.btOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmLbExport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "导出标注信息";
            this.Load += new System.EventHandler(this.FrmLbExport_Load);
            this.groupBox1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.gbServer.ResumeLayout(false);
            this.gbServer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox gbServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbDb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmblyr;
        private System.Windows.Forms.TextBox txtShp;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btView;
        private System.Windows.Forms.RadioButton rbLocal;
        private System.Windows.Forms.RadioButton rbSDE;
        private System.Windows.Forms.ComboBox cmbField;
        private System.Windows.Forms.Label label3;
    }
}
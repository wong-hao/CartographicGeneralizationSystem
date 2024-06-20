namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmAnnoLyr
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAnnoLyr));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.cb_reserve = new System.Windows.Forms.CheckBox();
            this.cbLayers = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dgSelectRule = new System.Windows.Forms.DataGridView();
            this.Selected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tbMDBFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExportConfig = new System.Windows.Forms.Button();
            this.btnImportConfig = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnVerify = new System.Windows.Forms.Button();
            this.btAll = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgSelectRule)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnExportConfig);
            this.splitContainer1.Panel2.Controls.Add(this.btnImportConfig);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(755, 599);
            this.splitContainer1.SplitterDistance = 559;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.cb_reserve);
            this.splitContainer2.Panel1.Controls.Add(this.cbLayers);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgSelectRule);
            this.splitContainer2.Panel2.Controls.Add(this.panel2);
            this.splitContainer2.Size = new System.Drawing.Size(755, 559);
            this.splitContainer2.SplitterDistance = 41;
            this.splitContainer2.SplitterWidth = 1;
            this.splitContainer2.TabIndex = 0;
            // 
            // cb_reserve
            // 
            this.cb_reserve.AutoSize = true;
            this.cb_reserve.Location = new System.Drawing.Point(360, 13);
            this.cb_reserve.Name = "cb_reserve";
            this.cb_reserve.Size = new System.Drawing.Size(132, 16);
            this.cb_reserve.TabIndex = 10;
            this.cb_reserve.Text = "保留原注记图层要素";
            this.cb_reserve.UseVisualStyleBackColor = true;
            // 
            // cbLayers
            // 
            this.cbLayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLayers.FormattingEnabled = true;
            this.cbLayers.Location = new System.Drawing.Point(132, 10);
            this.cbLayers.Name = "cbLayers";
            this.cbLayers.Size = new System.Drawing.Size(221, 20);
            this.cbLayers.TabIndex = 9;
            this.cbLayers.SelectedIndexChanged += new System.EventHandler(this.cbLayers_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "生成注记的图层：";
            // 
            // dgSelectRule
            // 
            this.dgSelectRule.AllowUserToAddRows = false;
            this.dgSelectRule.AllowUserToDeleteRows = false;
            this.dgSelectRule.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgSelectRule.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgSelectRule.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Selected});
            this.dgSelectRule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgSelectRule.GridColor = System.Drawing.SystemColors.AppWorkspace;
            this.dgSelectRule.Location = new System.Drawing.Point(0, 0);
            this.dgSelectRule.MultiSelect = false;
            this.dgSelectRule.Name = "dgSelectRule";
            this.dgSelectRule.RowHeadersVisible = false;
            this.dgSelectRule.RowTemplate.Height = 23;
            this.dgSelectRule.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgSelectRule.Size = new System.Drawing.Size(755, 477);
            this.dgSelectRule.TabIndex = 7;
            this.dgSelectRule.TabStop = false;
            // 
            // Selected
            // 
            this.Selected.FillWeight = 40F;
            this.Selected.HeaderText = "选择状态";
            this.Selected.Name = "Selected";
            this.Selected.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Selected.Width = 59;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tbMDBFilePath);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 477);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(755, 40);
            this.panel2.TabIndex = 6;
            // 
            // tbMDBFilePath
            // 
            this.tbMDBFilePath.Location = new System.Drawing.Point(123, 9);
            this.tbMDBFilePath.Name = "tbMDBFilePath";
            this.tbMDBFilePath.ReadOnly = true;
            this.tbMDBFilePath.Size = new System.Drawing.Size(624, 21);
            this.tbMDBFilePath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "底图注记规则库路径：";
            // 
            // btnExportConfig
            // 
            this.btnExportConfig.Location = new System.Drawing.Point(110, 1);
            this.btnExportConfig.Name = "btnExportConfig";
            this.btnExportConfig.Size = new System.Drawing.Size(92, 31);
            this.btnExportConfig.TabIndex = 11;
            this.btnExportConfig.Text = "导出选择配置";
            this.btnExportConfig.UseVisualStyleBackColor = true;
            this.btnExportConfig.Click += new System.EventHandler(this.btnExportConfig_Click);
            // 
            // btnImportConfig
            // 
            this.btnImportConfig.Location = new System.Drawing.Point(12, 1);
            this.btnImportConfig.Name = "btnImportConfig";
            this.btnImportConfig.Size = new System.Drawing.Size(92, 31);
            this.btnImportConfig.TabIndex = 11;
            this.btnImportConfig.Text = "导入选择配置";
            this.btnImportConfig.UseVisualStyleBackColor = true;
            this.btnImportConfig.Click += new System.EventHandler(this.btnImportConfig_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnVerify);
            this.panel1.Controls.Add(this.btAll);
            this.panel1.Controls.Add(this.btClear);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Controls.Add(this.btOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(369, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(386, 39);
            this.panel1.TabIndex = 0;
            // 
            // btnVerify
            // 
            this.btnVerify.Location = new System.Drawing.Point(22, 1);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(54, 31);
            this.btnVerify.TabIndex = 11;
            this.btnVerify.Text = "验证";
            this.btnVerify.UseVisualStyleBackColor = true;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);
            // 
            // btAll
            // 
            this.btAll.Location = new System.Drawing.Point(89, 1);
            this.btAll.Name = "btAll";
            this.btAll.Size = new System.Drawing.Size(54, 31);
            this.btAll.TabIndex = 11;
            this.btAll.Text = "全选";
            this.btAll.UseVisualStyleBackColor = true;
            this.btAll.Click += new System.EventHandler(this.btAll_Click);
            // 
            // btClear
            // 
            this.btClear.Location = new System.Drawing.Point(165, 1);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(53, 31);
            this.btClear.TabIndex = 10;
            this.btClear.Text = "清空";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(322, 1);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(56, 32);
            this.btCancel.TabIndex = 9;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(241, 2);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(58, 31);
            this.btOk.TabIndex = 8;
            this.btOk.Text = "确定";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // FrmAnnoLyr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(755, 599);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmAnnoLyr";
            this.Text = "注记图层设置";
            this.Load += new System.EventHandler(this.FrmAnnoLyr_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgSelectRule)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ComboBox cbLayers;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btAll;
        private System.Windows.Forms.Button btClear;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.CheckBox cb_reserve;
        private System.Windows.Forms.Button btnExportConfig;
        private System.Windows.Forms.Button btnImportConfig;
        private System.Windows.Forms.Button btnVerify;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox tbMDBFilePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgSelectRule;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Selected;
    }
}
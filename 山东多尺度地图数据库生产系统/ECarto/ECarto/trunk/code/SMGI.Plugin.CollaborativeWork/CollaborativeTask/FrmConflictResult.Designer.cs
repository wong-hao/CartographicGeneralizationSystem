namespace SMGI.Plugin.CollaborativeWork
{
    partial class FrmConflictResult
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnAutoProcess = new System.Windows.Forms.Button();
            this.cbFilter = new System.Windows.Forms.ComboBox();
            this.lbInfo = new System.Windows.Forms.Label();
            this.dgConflictResult = new System.Windows.Forms.DataGridView();
            this.GUID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LocalFeature = new System.Windows.Forms.DataGridViewButtonColumn();
            this.localFeatureLN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.localFetureVer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.localFetureDelState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serverFeature = new System.Windows.Forms.DataGridViewButtonColumn();
            this.ServerFeatureOPUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serverFeatureLN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serverFeatureVer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serverFeatureDelState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PropertyProcess = new System.Windows.Forms.DataGridViewLinkColumn();
            this.GeoProcess = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Processed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.remark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgConflictResult)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnAutoProcess);
            this.panel1.Controls.Add(this.cbFilter);
            this.panel1.Controls.Add(this.lbInfo);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(1472, 32);
            this.panel1.TabIndex = 0;
            // 
            // btnAutoProcess
            // 
            this.btnAutoProcess.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAutoProcess.ForeColor = System.Drawing.Color.Black;
            this.btnAutoProcess.Location = new System.Drawing.Point(1163, 5);
            this.btnAutoProcess.Name = "btnAutoProcess";
            this.btnAutoProcess.Size = new System.Drawing.Size(105, 22);
            this.btnAutoProcess.TabIndex = 3;
            this.btnAutoProcess.Text = "自动处理伪冲突";
            this.btnAutoProcess.UseVisualStyleBackColor = true;
            this.btnAutoProcess.Click += new System.EventHandler(this.btnAutoProcess_Click);
            // 
            // cbFilter
            // 
            this.cbFilter.BackColor = System.Drawing.SystemColors.Window;
            this.cbFilter.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbFilter.DropDownHeight = 100;
            this.cbFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFilter.FormattingEnabled = true;
            this.cbFilter.IntegralHeight = false;
            this.cbFilter.ItemHeight = 14;
            this.cbFilter.Location = new System.Drawing.Point(1268, 5);
            this.cbFilter.Name = "cbFilter";
            this.cbFilter.Size = new System.Drawing.Size(199, 22);
            this.cbFilter.TabIndex = 2;
            this.cbFilter.SelectedIndexChanged += new System.EventHandler(this.cbFilter_SelectedIndexChanged);
            // 
            // lbInfo
            // 
            this.lbInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbInfo.Location = new System.Drawing.Point(5, 5);
            this.lbInfo.Name = "lbInfo";
            this.lbInfo.Size = new System.Drawing.Size(1462, 22);
            this.lbInfo.TabIndex = 1;
            this.lbInfo.Text = "label1";
            this.lbInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgConflictResult
            // 
            this.dgConflictResult.AllowUserToAddRows = false;
            this.dgConflictResult.AllowUserToDeleteRows = false;
            this.dgConflictResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Tahoma", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgConflictResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgConflictResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GUID,
            this.LocalFeature,
            this.localFeatureLN,
            this.localFetureVer,
            this.localFetureDelState,
            this.serverFeature,
            this.ServerFeatureOPUser,
            this.serverFeatureLN,
            this.serverFeatureVer,
            this.serverFeatureDelState,
            this.PropertyProcess,
            this.GeoProcess,
            this.Processed,
            this.remark});
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Tahoma", 9F);
            dataGridViewCellStyle10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(31)))), ((int)(((byte)(53)))));
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgConflictResult.DefaultCellStyle = dataGridViewCellStyle10;
            this.dgConflictResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgConflictResult.EnableHeadersVisualStyles = false;
            this.dgConflictResult.Location = new System.Drawing.Point(0, 32);
            this.dgConflictResult.Name = "dgConflictResult";
            this.dgConflictResult.RowHeadersWidth = 70;
            this.dgConflictResult.RowTemplate.Height = 23;
            this.dgConflictResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgConflictResult.Size = new System.Drawing.Size(1472, 490);
            this.dgConflictResult.TabIndex = 8;
            this.dgConflictResult.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgConflictResult_CellContentClick);
            this.dgConflictResult.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgConflictResult_EditingControlShowing);
            // 
            // GUID
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.GUID.DefaultCellStyle = dataGridViewCellStyle2;
            this.GUID.FillWeight = 200F;
            this.GUID.HeaderText = "GUID";
            this.GUID.MinimumWidth = 180;
            this.GUID.Name = "GUID";
            this.GUID.ReadOnly = true;
            // 
            // LocalFeature
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.NullValue = "缩放至要素";
            this.LocalFeature.DefaultCellStyle = dataGridViewCellStyle3;
            this.LocalFeature.FillWeight = 60F;
            this.LocalFeature.HeaderText = "本地要素";
            this.LocalFeature.Name = "LocalFeature";
            this.LocalFeature.ReadOnly = true;
            this.LocalFeature.Text = "本地要素";
            // 
            // localFeatureLN
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.localFeatureLN.DefaultCellStyle = dataGridViewCellStyle4;
            this.localFeatureLN.FillWeight = 60F;
            this.localFeatureLN.HeaderText = "本地要素图层名";
            this.localFeatureLN.Name = "localFeatureLN";
            this.localFeatureLN.ReadOnly = true;
            // 
            // localFetureVer
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.localFetureVer.DefaultCellStyle = dataGridViewCellStyle5;
            this.localFetureVer.FillWeight = 60F;
            this.localFetureVer.HeaderText = "本地要素版本号";
            this.localFetureVer.Name = "localFetureVer";
            this.localFetureVer.ReadOnly = true;
            // 
            // localFetureDelState
            // 
            this.localFetureDelState.FillWeight = 50F;
            this.localFetureDelState.HeaderText = "本地要素删除状态";
            this.localFetureDelState.Name = "localFetureDelState";
            // 
            // serverFeature
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.NullValue = "缩放至要素";
            this.serverFeature.DefaultCellStyle = dataGridViewCellStyle6;
            this.serverFeature.FillWeight = 60F;
            this.serverFeature.HeaderText = "冲突要素";
            this.serverFeature.Name = "serverFeature";
            this.serverFeature.ReadOnly = true;
            // 
            // ServerFeatureOPUser
            // 
            this.ServerFeatureOPUser.FillWeight = 60F;
            this.ServerFeatureOPUser.HeaderText = "冲突要素操作者";
            this.ServerFeatureOPUser.Name = "ServerFeatureOPUser";
            // 
            // serverFeatureLN
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.serverFeatureLN.DefaultCellStyle = dataGridViewCellStyle7;
            this.serverFeatureLN.FillWeight = 60F;
            this.serverFeatureLN.HeaderText = "冲突要素图层名";
            this.serverFeatureLN.Name = "serverFeatureLN";
            this.serverFeatureLN.ReadOnly = true;
            this.serverFeatureLN.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.serverFeatureLN.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // serverFeatureVer
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.serverFeatureVer.DefaultCellStyle = dataGridViewCellStyle8;
            this.serverFeatureVer.FillWeight = 60F;
            this.serverFeatureVer.HeaderText = "冲突要素版本号";
            this.serverFeatureVer.Name = "serverFeatureVer";
            this.serverFeatureVer.ReadOnly = true;
            // 
            // serverFeatureDelState
            // 
            this.serverFeatureDelState.FillWeight = 50F;
            this.serverFeatureDelState.HeaderText = "冲突要素删除状态";
            this.serverFeatureDelState.Name = "serverFeatureDelState";
            // 
            // PropertyProcess
            // 
            dataGridViewCellStyle9.NullValue = "属性冲突处理";
            this.PropertyProcess.DefaultCellStyle = dataGridViewCellStyle9;
            this.PropertyProcess.FillWeight = 80F;
            this.PropertyProcess.HeaderText = "属性冲突处理";
            this.PropertyProcess.Name = "PropertyProcess";
            this.PropertyProcess.Text = "属性冲突处理";
            this.PropertyProcess.ToolTipText = "属性冲突处理";
            this.PropertyProcess.UseColumnTextForLinkValue = true;
            // 
            // GeoProcess
            // 
            this.GeoProcess.FillWeight = 80F;
            this.GeoProcess.HeaderText = "几何冲突处理";
            this.GeoProcess.Items.AddRange(new object[] {
            "合并几何"});
            this.GeoProcess.Name = "GeoProcess";
            // 
            // Processed
            // 
            this.Processed.FillWeight = 40F;
            this.Processed.HeaderText = "已处理";
            this.Processed.Name = "Processed";
            // 
            // remark
            // 
            this.remark.FillWeight = 40F;
            this.remark.HeaderText = "备注";
            this.remark.Name = "remark";
            this.remark.ReadOnly = true;
            // 
            // FrmConflictResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1472, 522);
            this.Controls.Add(this.dgConflictResult);
            this.Controls.Add(this.panel1);
            this.Name = "FrmConflictResult";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgConflictResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbInfo;
        public System.Windows.Forms.DataGridView dgConflictResult;
        private System.Windows.Forms.ComboBox cbFilter;
        private System.Windows.Forms.Button btnAutoProcess;
        private System.Windows.Forms.DataGridViewTextBoxColumn GUID;
        private System.Windows.Forms.DataGridViewButtonColumn LocalFeature;
        private System.Windows.Forms.DataGridViewTextBoxColumn localFeatureLN;
        private System.Windows.Forms.DataGridViewTextBoxColumn localFetureVer;
        private System.Windows.Forms.DataGridViewTextBoxColumn localFetureDelState;
        private System.Windows.Forms.DataGridViewButtonColumn serverFeature;
        private System.Windows.Forms.DataGridViewTextBoxColumn ServerFeatureOPUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn serverFeatureLN;
        private System.Windows.Forms.DataGridViewTextBoxColumn serverFeatureVer;
        private System.Windows.Forms.DataGridViewTextBoxColumn serverFeatureDelState;
        private System.Windows.Forms.DataGridViewLinkColumn PropertyProcess;
        private System.Windows.Forms.DataGridViewComboBoxColumn GeoProcess;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Processed;
        private System.Windows.Forms.DataGridViewTextBoxColumn remark;
    }
}
namespace SMGI.Plugin.EmergencyMap
{
    partial class DataBaseStructUpgradeForm
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
            this.cmbMapSize = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbBaseMapTemplate = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbMapScale = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnTarget = new System.Windows.Forms.Button();
            this.txtTarget = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.txtExport = new System.Windows.Forms.TextBox();
            this.cbAttach = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbLayerRuleFile = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbMxdFile = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.cmbMapSize);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbBaseMapTemplate);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbMapScale);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(567, 168);
            this.groupBox1.TabIndex = 83;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "底图";
            // 
            // cmbMapSize
            // 
            this.cmbMapSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMapSize.FormattingEnabled = true;
            this.cmbMapSize.Location = new System.Drawing.Point(453, 32);
            this.cmbMapSize.Name = "cmbMapSize";
            this.cmbMapSize.Size = new System.Drawing.Size(107, 20);
            this.cmbMapSize.TabIndex = 35;
            this.cmbMapSize.SelectedIndexChanged += new System.EventHandler(this.cmbMapSize_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(381, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 34;
            this.label2.Text = "开本[可选]：";
            // 
            // cbBaseMapTemplate
            // 
            this.cbBaseMapTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBaseMapTemplate.FormattingEnabled = true;
            this.cbBaseMapTemplate.Location = new System.Drawing.Point(273, 32);
            this.cbBaseMapTemplate.Name = "cbBaseMapTemplate";
            this.cbBaseMapTemplate.Size = new System.Drawing.Size(100, 20);
            this.cbBaseMapTemplate.TabIndex = 29;
            this.cbBaseMapTemplate.SelectedIndexChanged += new System.EventHandler(this.cbBaseMapTemplate_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(206, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 28;
            this.label1.Text = "底图模板：";
            // 
            // tbMapScale
            // 
            this.tbMapScale.Location = new System.Drawing.Point(84, 32);
            this.tbMapScale.Name = "tbMapScale";
            this.tbMapScale.Size = new System.Drawing.Size(101, 21);
            this.tbMapScale.TabIndex = 21;
            this.tbMapScale.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbMapScale_KeyUp);
            this.tbMapScale.Leave += new System.EventHandler(this.tbMapScale_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 34);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 22;
            this.label7.Text = "比例尺 1：";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnTarget);
            this.groupBox3.Controls.Add(this.txtTarget);
            this.groupBox3.Location = new System.Drawing.Point(12, 186);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(567, 70);
            this.groupBox3.TabIndex = 84;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "源数据库";
            // 
            // btnTarget
            // 
            this.btnTarget.Location = new System.Drawing.Point(506, 29);
            this.btnTarget.Margin = new System.Windows.Forms.Padding(2);
            this.btnTarget.Name = "btnTarget";
            this.btnTarget.Size = new System.Drawing.Size(56, 26);
            this.btnTarget.TabIndex = 79;
            this.btnTarget.Text = "浏览";
            this.btnTarget.UseVisualStyleBackColor = true;
            this.btnTarget.Click += new System.EventHandler(this.btnTarget_Click);
            // 
            // txtTarget
            // 
            this.txtTarget.Location = new System.Drawing.Point(15, 32);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.ReadOnly = true;
            this.txtTarget.Size = new System.Drawing.Size(486, 21);
            this.txtTarget.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnExport);
            this.groupBox2.Controls.Add(this.txtExport);
            this.groupBox2.Location = new System.Drawing.Point(12, 265);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(567, 70);
            this.groupBox2.TabIndex = 85;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "输出数据库";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(506, 27);
            this.btnExport.Margin = new System.Windows.Forms.Padding(2);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(56, 26);
            this.btnExport.TabIndex = 79;
            this.btnExport.Text = "浏览";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // txtExport
            // 
            this.txtExport.Location = new System.Drawing.Point(15, 32);
            this.txtExport.Name = "txtExport";
            this.txtExport.Size = new System.Drawing.Size(486, 21);
            this.txtExport.TabIndex = 0;
            // 
            // cbAttach
            // 
            this.cbAttach.AutoSize = true;
            this.cbAttach.Location = new System.Drawing.Point(12, 344);
            this.cbAttach.Name = "cbAttach";
            this.cbAttach.Size = new System.Drawing.Size(132, 16);
            this.cbAttach.TabIndex = 86;
            this.cbAttach.Text = "符号化后区分主邻区";
            this.cbAttach.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(512, 351);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(69, 35);
            this.btnCancel.TabIndex = 88;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(421, 351);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(69, 35);
            this.btnOK.TabIndex = 87;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 398);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(473, 12);
            this.label3.TabIndex = 89;
            this.label3.Text = "开本：默认为空，采用比例尺模板符号化。选择开本后根据选择的地图开本模板符号化。";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.tbLayerRuleFile);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.tbMxdFile);
            this.groupBox4.Location = new System.Drawing.Point(15, 58);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(545, 97);
            this.groupBox4.TabIndex = 36;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "模板规则信息";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 12);
            this.label5.TabIndex = 22;
            this.label5.Text = "图层对照规则:";
            // 
            // tbLayerRuleFile
            // 
            this.tbLayerRuleFile.Location = new System.Drawing.Point(89, 64);
            this.tbLayerRuleFile.Name = "tbLayerRuleFile";
            this.tbLayerRuleFile.ReadOnly = true;
            this.tbLayerRuleFile.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbLayerRuleFile.Size = new System.Drawing.Size(450, 21);
            this.tbLayerRuleFile.TabIndex = 21;
            this.tbLayerRuleFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 12);
            this.label4.TabIndex = 22;
            this.label4.Text = "地图模板文档:";
            // 
            // tbMxdFile
            // 
            this.tbMxdFile.Location = new System.Drawing.Point(89, 27);
            this.tbMxdFile.Name = "tbMxdFile";
            this.tbMxdFile.ReadOnly = true;
            this.tbMxdFile.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbMxdFile.Size = new System.Drawing.Size(450, 21);
            this.tbMxdFile.TabIndex = 21;
            this.tbMxdFile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DataBaseStructUpgradeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 439);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbAttach);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Name = "DataBaseStructUpgradeForm";
            this.Text = "地图符号化";
            this.Load += new System.EventHandler(this.DataBaseStructUpgradeForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbMapSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbBaseMapTemplate;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox tbMapScale;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnTarget;
        private System.Windows.Forms.TextBox txtTarget;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.TextBox txtExport;
        private System.Windows.Forms.CheckBox cbAttach;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox tbLayerRuleFile;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox tbMxdFile;

    }
}
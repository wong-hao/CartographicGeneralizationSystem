namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmAnnoCfAd
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
            this.rgEn = new System.Windows.Forms.RadioButton();
            this.rbGeo = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btClose
            // 
            this.btClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btClose.Location = new System.Drawing.Point(185, 106);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(63, 28);
            this.btClose.TabIndex = 5;
            this.btClose.Text = "取消";
            this.btClose.UseVisualStyleBackColor = true;
            this.btClose.Click += new System.EventHandler(this.btClose_Click);
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(115, 106);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(64, 28);
            this.btOk.TabIndex = 4;
            this.btOk.Text = "确定";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbGeo);
            this.groupBox1.Controls.Add(this.rgEn);
            this.groupBox1.Location = new System.Drawing.Point(4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(244, 84);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "冲突检测方式";
            // 
            // rgEn
            // 
            this.rgEn.AutoSize = true;
            this.rgEn.Checked = true;
            this.rgEn.Location = new System.Drawing.Point(23, 36);
            this.rgEn.Name = "rgEn";
            this.rgEn.Size = new System.Drawing.Size(95, 16);
            this.rgEn.TabIndex = 0;
            this.rgEn.TabStop = true;
            this.rgEn.Text = "注记外包矩形";
            this.rgEn.UseVisualStyleBackColor = true;
            this.rgEn.CheckedChanged += new System.EventHandler(this.rgEn_CheckedChanged);
            // 
            // rbGeo
            // 
            this.rbGeo.AutoSize = true;
            this.rbGeo.Location = new System.Drawing.Point(143, 36);
            this.rbGeo.Name = "rbGeo";
            this.rbGeo.Size = new System.Drawing.Size(71, 16);
            this.rbGeo.TabIndex = 1;
            this.rbGeo.Text = "注记几何";
            this.rbGeo.UseVisualStyleBackColor = true;
            this.rbGeo.CheckedChanged += new System.EventHandler(this.rbGeo_CheckedChanged);
            // 
            // FrmAnnoCfAd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 146);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.btOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmAnnoCfAd";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "冲突高级设置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbGeo;
        private System.Windows.Forms.RadioButton rgEn;
    }
}
namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmCJM
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCJM));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbManual = new System.Windows.Forms.Label();
            this.cmbLyrs = new System.Windows.Forms.ComboBox();
            this.CJMWidth = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ColorPanel = new System.Windows.Forms.Panel();
            this.colorDetail = new System.Windows.Forms.Button();
            this.cmbColor = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            this.cbSingle = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbSingle);
            this.groupBox1.Controls.Add(this.lbManual);
            this.groupBox1.Controls.Add(this.cmbLyrs);
            this.groupBox1.Controls.Add(this.CJMWidth);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.cmbColor);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(459, 213);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "色带设置";
            // 
            // lbManual
            // 
            this.lbManual.AutoSize = true;
            this.lbManual.Location = new System.Drawing.Point(10, 152);
            this.lbManual.Name = "lbManual";
            this.lbManual.Size = new System.Drawing.Size(65, 12);
            this.lbManual.TabIndex = 20;
            this.lbManual.Text = "侧界类型：";
            // 
            // cmbLyrs
            // 
            this.cmbLyrs.FormattingEnabled = true;
            this.cmbLyrs.Location = new System.Drawing.Point(82, 149);
            this.cmbLyrs.Name = "cmbLyrs";
            this.cmbLyrs.Size = new System.Drawing.Size(150, 20);
            this.cmbLyrs.TabIndex = 19;
            this.cmbLyrs.SelectedIndexChanged += new System.EventHandler(this.cmbLyrs_SelectedIndexChanged);
            // 
            // CJMWidth
            // 
            this.CJMWidth.Location = new System.Drawing.Point(340, 148);
            this.CJMWidth.Name = "CJMWidth";
            this.CJMWidth.Size = new System.Drawing.Size(107, 21);
            this.CJMWidth.TabIndex = 26;
            this.CJMWidth.Text = "6";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ColorPanel);
            this.groupBox2.Controls.Add(this.colorDetail);
            this.groupBox2.Location = new System.Drawing.Point(7, 43);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(446, 103);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            // 
            // ColorPanel
            // 
            this.ColorPanel.AutoScroll = true;
            this.ColorPanel.Location = new System.Drawing.Point(3, 10);
            this.ColorPanel.Name = "ColorPanel";
            this.ColorPanel.Size = new System.Drawing.Size(390, 87);
            this.ColorPanel.TabIndex = 17;
            // 
            // colorDetail
            // 
            this.colorDetail.Location = new System.Drawing.Point(399, 46);
            this.colorDetail.Name = "colorDetail";
            this.colorDetail.Size = new System.Drawing.Size(41, 23);
            this.colorDetail.TabIndex = 16;
            this.colorDetail.Text = "详细";
            this.colorDetail.UseVisualStyleBackColor = true;
            this.colorDetail.Click += new System.EventHandler(this.colorDetail_Click);
            // 
            // cmbColor
            // 
            this.cmbColor.FormattingEnabled = true;
            this.cmbColor.Location = new System.Drawing.Point(80, 17);
            this.cmbColor.Name = "cmbColor";
            this.cmbColor.Size = new System.Drawing.Size(123, 20);
            this.cmbColor.TabIndex = 14;
            this.cmbColor.SelectedIndexChanged += new System.EventHandler(this.cmbColor_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(247, 151);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "侧界宽度mm：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "颜色方案：";
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(418, 231);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(57, 32);
            this.btCancel.TabIndex = 14;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(312, 231);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(60, 32);
            this.btOk.TabIndex = 13;
            this.btOk.Text = "确定";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // cbSingle
            // 
            this.cbSingle.AutoSize = true;
            this.cbSingle.Location = new System.Drawing.Point(12, 183);
            this.cbSingle.Name = "cbSingle";
            this.cbSingle.Size = new System.Drawing.Size(48, 16);
            this.cbSingle.TabIndex = 28;
            this.cbSingle.Text = "单层";
            this.cbSingle.UseVisualStyleBackColor = true;
            // 
            // FrmCJM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 275);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmCJM";
            this.Text = "侧界生成";
            this.Load += new System.EventHandler(this.FrmSDM_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel ColorPanel;
        private System.Windows.Forms.Button colorDetail;
        public System.Windows.Forms.ComboBox cmbColor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.TextBox CJMWidth;
        private System.Windows.Forms.Label lbManual;
        public System.Windows.Forms.ComboBox cmbLyrs;
        private System.Windows.Forms.CheckBox cbSingle;
    }
}
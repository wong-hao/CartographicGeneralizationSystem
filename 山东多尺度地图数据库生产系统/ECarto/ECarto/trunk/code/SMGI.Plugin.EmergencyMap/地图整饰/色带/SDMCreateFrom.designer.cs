namespace SMGI.Plugin.EmergencyMap
{
    partial class SDMCreateFrom
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SDMCreateFrom));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numSDM = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.lbManual = new System.Windows.Forms.Label();
            this.cmbLyrs = new System.Windows.Forms.ComboBox();
            this.txtSDMTotalWidth = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ColorPanel = new System.Windows.Forms.Panel();
            this.colorDetail = new System.Windows.Forms.Button();
            this.cmbColor = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSDM)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numSDM);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lbManual);
            this.groupBox1.Controls.Add(this.cmbLyrs);
            this.groupBox1.Controls.Add(this.txtSDMTotalWidth);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.cmbColor);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(459, 194);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "色带设置";
            // 
            // numSDM
            // 
            this.numSDM.Location = new System.Drawing.Point(327, 158);
            this.numSDM.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numSDM.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numSDM.Name = "numSDM";
            this.numSDM.Size = new System.Drawing.Size(75, 21);
            this.numSDM.TabIndex = 30;
            this.numSDM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numSDM.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(256, 161);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 29;
            this.label3.Text = "色带面层数：";
            // 
            // lbManual
            // 
            this.lbManual.AutoSize = true;
            this.lbManual.Location = new System.Drawing.Point(256, 20);
            this.lbManual.Name = "lbManual";
            this.lbManual.Size = new System.Drawing.Size(65, 12);
            this.lbManual.TabIndex = 20;
            this.lbManual.Text = "处理图层：";
            this.lbManual.Visible = false;
            // 
            // cmbLyrs
            // 
            this.cmbLyrs.FormattingEnabled = true;
            this.cmbLyrs.Location = new System.Drawing.Point(327, 17);
            this.cmbLyrs.Name = "cmbLyrs";
            this.cmbLyrs.Size = new System.Drawing.Size(120, 20);
            this.cmbLyrs.TabIndex = 19;
            this.cmbLyrs.Visible = false;
            this.cmbLyrs.SelectedIndexChanged += new System.EventHandler(this.cmbLyrs_SelectedIndexChanged);
            // 
            // txtSDMTotalWidth
            // 
            this.txtSDMTotalWidth.Location = new System.Drawing.Point(91, 158);
            this.txtSDMTotalWidth.Name = "txtSDMTotalWidth";
            this.txtSDMTotalWidth.Size = new System.Drawing.Size(75, 21);
            this.txtSDMTotalWidth.TabIndex = 26;
            this.txtSDMTotalWidth.Text = "5.0";
            this.txtSDMTotalWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(172, 161);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "mm";
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
            this.cmbColor.Size = new System.Drawing.Size(120, 20);
            this.cmbColor.TabIndex = 14;
            this.cmbColor.SelectedIndexChanged += new System.EventHandler(this.cmbColor_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 161);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "色带面总宽度：";
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
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 215);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(478, 31);
            this.panel1.TabIndex = 12;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(337, 4);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(64, 23);
            this.btOK.TabIndex = 7;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOk_Click);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(401, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(410, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // SDMCreateFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 246);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SDMCreateFrom";
            this.Text = "色带生成";
            this.Load += new System.EventHandler(this.FrmSDM_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSDM)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
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
        private System.Windows.Forms.TextBox txtSDMTotalWidth;
        private System.Windows.Forms.Label lbManual;
        public System.Windows.Forms.ComboBox cmbLyrs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numSDM;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
    }
}
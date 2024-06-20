namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmMapLine
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMapLine));
            this.inWidth = new System.Windows.Forms.TextBox();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.outUpWidth = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.outDownWidth = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.outLeftWidth = new System.Windows.Forms.TextBox();
            this.outRightWidth = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_apply = new System.Windows.Forms.Button();
            this.cbClip = new System.Windows.Forms.CheckBox();
            this.btLast = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // inWidth
            // 
            this.inWidth.Location = new System.Drawing.Point(138, 131);
            this.inWidth.Name = "inWidth";
            this.inWidth.Size = new System.Drawing.Size(106, 21);
            this.inWidth.TabIndex = 3;
            this.inWidth.Text = "17";
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(177, 174);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(40, 30);
            this.btOK.TabIndex = 4;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(223, 174);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(43, 30);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // outUpWidth
            // 
            this.outUpWidth.Location = new System.Drawing.Point(49, 29);
            this.outUpWidth.Name = "outUpWidth";
            this.outUpWidth.Size = new System.Drawing.Size(60, 21);
            this.outUpWidth.TabIndex = 1;
            this.outUpWidth.Text = "25";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "上：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(137, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "下：";
            // 
            // outDownWidth
            // 
            this.outDownWidth.Location = new System.Drawing.Point(172, 27);
            this.outDownWidth.Name = "outDownWidth";
            this.outDownWidth.Size = new System.Drawing.Size(60, 21);
            this.outDownWidth.TabIndex = 4;
            this.outDownWidth.Text = "30";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "左：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(137, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 6;
            this.label5.Text = "右：";
            // 
            // outLeftWidth
            // 
            this.outLeftWidth.Location = new System.Drawing.Point(49, 56);
            this.outLeftWidth.Name = "outLeftWidth";
            this.outLeftWidth.Size = new System.Drawing.Size(60, 21);
            this.outLeftWidth.TabIndex = 7;
            this.outLeftWidth.Text = "25";
            this.outLeftWidth.TextChanged += new System.EventHandler(this.outLeftWidth_TextChanged);
            // 
            // outRightWidth
            // 
            this.outRightWidth.Location = new System.Drawing.Point(172, 58);
            this.outRightWidth.Name = "outRightWidth";
            this.outRightWidth.Size = new System.Drawing.Size(60, 21);
            this.outRightWidth.TabIndex = 8;
            this.outRightWidth.Text = "25";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.outRightWidth);
            this.groupBox1.Controls.Add(this.outLeftWidth);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.outDownWidth);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.outUpWidth);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(254, 85);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "外图廓与边缘间距(mm)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "内外图廓间距(mm)：";
            // 
            // btn_apply
            // 
            this.btn_apply.Location = new System.Drawing.Point(132, 174);
            this.btn_apply.Name = "btn_apply";
            this.btn_apply.Size = new System.Drawing.Size(39, 30);
            this.btn_apply.TabIndex = 7;
            this.btn_apply.Text = "应用";
            this.btn_apply.UseVisualStyleBackColor = true;
            this.btn_apply.Click += new System.EventHandler(this.btn_apply_Click);
            // 
            // cbClip
            // 
            this.cbClip.AutoSize = true;
            this.cbClip.Location = new System.Drawing.Point(13, 103);
            this.cbClip.Name = "cbClip";
            this.cbClip.Size = new System.Drawing.Size(120, 16);
            this.cbClip.TabIndex = 8;
            this.cbClip.Text = "内图廓相切裁切面";
            this.cbClip.UseVisualStyleBackColor = true;
            // 
            // btLast
            // 
            this.btLast.Image = ((System.Drawing.Image)(resources.GetObject("btLast.Image")));
            this.btLast.Location = new System.Drawing.Point(12, 174);
            this.btLast.Name = "btLast";
            this.btLast.Size = new System.Drawing.Size(31, 30);
            this.btLast.TabIndex = 14;
            this.btLast.UseVisualStyleBackColor = true;
            this.btLast.Click += new System.EventHandler(this.btLast_Click);
            // 
            // FrmMapLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 211);
            this.Controls.Add(this.btLast);
            this.Controls.Add(this.cbClip);
            this.Controls.Add(this.btn_apply);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.inWidth);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmMapLine";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "图廓线";
            this.Load += new System.EventHandler(this.FrmMapLine_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox inWidth;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox outUpWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox outDownWidth;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox outLeftWidth;
        private System.Windows.Forms.TextBox outRightWidth;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_apply;
        private System.Windows.Forms.CheckBox cbClip;
        private System.Windows.Forms.Button btLast;
    }
}
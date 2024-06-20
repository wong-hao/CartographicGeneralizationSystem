namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    partial class FrmIntervalLine
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
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtIntervalWidth = new System.Windows.Forms.TextBox();
            this.label_toWidth = new System.Windows.Forms.Label();
            this.btColor = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btcancel = new System.Windows.Forms.Button();
            this.btok = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtWidth);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtIntervalWidth);
            this.groupBox1.Controls.Add(this.label_toWidth);
            this.groupBox1.Controls.Add(this.btColor);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(467, 68);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "线要素参数设置";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(375, 29);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(83, 21);
            this.txtWidth.TabIndex = 45;
            this.txtWidth.Text = "7";
            this.txtWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtWidth_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(324, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 44;
            this.label1.Text = "线宽度:";
            // 
            // txtIntervalWidth
            // 
            this.txtIntervalWidth.Location = new System.Drawing.Point(218, 29);
            this.txtIntervalWidth.Name = "txtIntervalWidth";
            this.txtIntervalWidth.Size = new System.Drawing.Size(83, 21);
            this.txtIntervalWidth.TabIndex = 43;
            this.txtIntervalWidth.Text = "2";
            this.txtIntervalWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtIntervalWidth_KeyPress);
            // 
            // label_toWidth
            // 
            this.label_toWidth.AutoSize = true;
            this.label_toWidth.Location = new System.Drawing.Point(155, 34);
            this.label_toWidth.Name = "label_toWidth";
            this.label_toWidth.Size = new System.Drawing.Size(59, 12);
            this.label_toWidth.TabIndex = 42;
            this.label_toWidth.Text = "间隔宽度:";
            // 
            // btColor
            // 
            this.btColor.BackColor = System.Drawing.Color.PowderBlue;
            this.btColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btColor.Location = new System.Drawing.Point(53, 31);
            this.btColor.Margin = new System.Windows.Forms.Padding(2);
            this.btColor.Name = "btColor";
            this.btColor.Size = new System.Drawing.Size(79, 18);
            this.btColor.TabIndex = 35;
            this.btColor.UseVisualStyleBackColor = false;
            this.btColor.Click += new System.EventHandler(this.btColor_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 34;
            this.label2.Text = "颜色:";
            // 
            // btcancel
            // 
            this.btcancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btcancel.Location = new System.Drawing.Point(413, 86);
            this.btcancel.Name = "btcancel";
            this.btcancel.Size = new System.Drawing.Size(63, 27);
            this.btcancel.TabIndex = 13;
            this.btcancel.Text = "取消";
            this.btcancel.UseVisualStyleBackColor = true;
            this.btcancel.Click += new System.EventHandler(this.btcancel_Click);
            // 
            // btok
            // 
            this.btok.Location = new System.Drawing.Point(325, 86);
            this.btok.Name = "btok";
            this.btok.Size = new System.Drawing.Size(59, 29);
            this.btok.TabIndex = 12;
            this.btok.Text = "确定";
            this.btok.UseVisualStyleBackColor = true;
            this.btok.Click += new System.EventHandler(this.btok_Click);
            // 
            // FrmIntervalLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 122);
            this.Controls.Add(this.btcancel);
            this.Controls.Add(this.btok);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmIntervalLine";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "间隔虚线线";
            this.Load += new System.EventHandler(this.FrmIntervalLine_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btColor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtIntervalWidth;
        private System.Windows.Forms.Label label_toWidth;
        private System.Windows.Forms.Button btcancel;
        private System.Windows.Forms.Button btok;
    }
}
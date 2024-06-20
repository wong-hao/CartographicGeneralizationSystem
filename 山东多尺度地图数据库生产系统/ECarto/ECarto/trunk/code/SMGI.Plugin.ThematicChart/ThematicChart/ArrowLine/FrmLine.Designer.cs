namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    partial class FrmLine
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
            this.label_toWidth = new System.Windows.Forms.Label();
            this.btColorFrom = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btcancel = new System.Windows.Forms.Button();
            this.btok = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtWidth);
            this.groupBox1.Controls.Add(this.label_toWidth);
            this.groupBox1.Controls.Add(this.btColorFrom);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(336, 69);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数设置";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(229, 30);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(83, 21);
            this.txtWidth.TabIndex = 43;
            this.txtWidth.Text = "2";
            this.txtWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtToWidth_KeyPress);
            // 
            // label_toWidth
            // 
            this.label_toWidth.AutoSize = true;
            this.label_toWidth.Location = new System.Drawing.Point(190, 34);
            this.label_toWidth.Name = "label_toWidth";
            this.label_toWidth.Size = new System.Drawing.Size(35, 12);
            this.label_toWidth.TabIndex = 42;
            this.label_toWidth.Text = "宽度:";
            // 
            // btColorFrom
            // 
            this.btColorFrom.BackColor = System.Drawing.Color.PowderBlue;
            this.btColorFrom.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btColorFrom.Location = new System.Drawing.Point(58, 31);
            this.btColorFrom.Margin = new System.Windows.Forms.Padding(2);
            this.btColorFrom.Name = "btColorFrom";
            this.btColorFrom.Size = new System.Drawing.Size(79, 18);
            this.btColorFrom.TabIndex = 35;
            this.btColorFrom.UseVisualStyleBackColor = false;
            this.btColorFrom.Click += new System.EventHandler(this.btColorFrom_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 34;
            this.label2.Text = "颜色:";
            // 
            // btcancel
            // 
            this.btcancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btcancel.Location = new System.Drawing.Point(285, 87);
            this.btcancel.Name = "btcancel";
            this.btcancel.Size = new System.Drawing.Size(63, 27);
            this.btcancel.TabIndex = 13;
            this.btcancel.Text = "取消";
            this.btcancel.UseVisualStyleBackColor = true;
            this.btcancel.Click += new System.EventHandler(this.btcancel_Click);
            // 
            // btok
            // 
            this.btok.Location = new System.Drawing.Point(197, 87);
            this.btok.Name = "btok";
            this.btok.Size = new System.Drawing.Size(59, 29);
            this.btok.TabIndex = 12;
            this.btok.Text = "确定";
            this.btok.UseVisualStyleBackColor = true;
            this.btok.Click += new System.EventHandler(this.btok_Click);
            // 
            // FrmLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 126);
            this.Controls.Add(this.btcancel);
            this.Controls.Add(this.btok);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLine";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "常规线";
            this.Load += new System.EventHandler(this.FrmLine_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btColorFrom;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label label_toWidth;
        private System.Windows.Forms.Button btcancel;
        private System.Windows.Forms.Button btok;
    }
}
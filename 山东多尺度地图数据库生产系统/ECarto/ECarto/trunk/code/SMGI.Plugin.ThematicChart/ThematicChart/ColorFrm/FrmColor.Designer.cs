namespace SMGI.Plugin.ThematicChart
{
    partial class FrmColor
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
            this.gBcolor = new System.Windows.Forms.GroupBox();
            this.ColorPanel = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.BtnCurColor = new System.Windows.Forms.Button();
            this.btMore = new System.Windows.Forms.Button();
            this.gBcolor.SuspendLayout();
            this.SuspendLayout();
            // 
            // gBcolor
            // 
            this.gBcolor.Controls.Add(this.ColorPanel);
            this.gBcolor.Location = new System.Drawing.Point(5, 7);
            this.gBcolor.Name = "gBcolor";
            this.gBcolor.Size = new System.Drawing.Size(337, 262);
            this.gBcolor.TabIndex = 0;
            this.gBcolor.TabStop = false;
            this.gBcolor.Text = "专家库颜色";
            // 
            // ColorPanel
            // 
            this.ColorPanel.AutoScroll = true;
            this.ColorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ColorPanel.Location = new System.Drawing.Point(3, 17);
            this.ColorPanel.Name = "ColorPanel";
            this.ColorPanel.Size = new System.Drawing.Size(331, 242);
            this.ColorPanel.TabIndex = 0;
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(194, 306);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(57, 31);
            this.btOK.TabIndex = 1;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(280, 306);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(62, 31);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(3, 315);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 16;
            this.label8.Text = "当前颜色：";
            // 
            // BtnCurColor
            // 
            this.BtnCurColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BtnCurColor.Location = new System.Drawing.Point(78, 312);
            this.BtnCurColor.Margin = new System.Windows.Forms.Padding(2);
            this.BtnCurColor.Name = "BtnCurColor";
            this.BtnCurColor.Size = new System.Drawing.Size(42, 18);
            this.BtnCurColor.TabIndex = 15;
            this.BtnCurColor.UseVisualStyleBackColor = true;
            // 
            // btMore
            // 
            this.btMore.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btMore.Font = new System.Drawing.Font("宋体", 8F);
            this.btMore.Location = new System.Drawing.Point(5, 274);
            this.btMore.Margin = new System.Windows.Forms.Padding(2);
            this.btMore.Name = "btMore";
            this.btMore.Size = new System.Drawing.Size(337, 20);
            this.btMore.TabIndex = 17;
            this.btMore.Text = "更多颜色";
            this.btMore.UseVisualStyleBackColor = true;
            this.btMore.Click += new System.EventHandler(this.btMore_Click);
            // 
            // FrmColor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 341);
            this.Controls.Add(this.btMore);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.BtnCurColor);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.gBcolor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmColor";
            this.Text = "颜色设置";
            this.Load += new System.EventHandler(this.FrmColor_Load);
            this.gBcolor.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gBcolor;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button BtnCurColor;
        private System.Windows.Forms.Button btMore;
        private System.Windows.Forms.Panel ColorPanel;

    }
}
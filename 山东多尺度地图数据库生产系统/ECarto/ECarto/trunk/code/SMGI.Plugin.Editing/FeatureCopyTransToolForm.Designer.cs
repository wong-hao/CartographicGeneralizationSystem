namespace SMGI.Plugin.GeneralEdit
{
    partial class FeatureCopyTransToolForm
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
            this.btStartCopy = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tBX = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btStartCopy
            // 
            this.btStartCopy.Location = new System.Drawing.Point(45, 58);
            this.btStartCopy.Name = "btStartCopy";
            this.btStartCopy.Size = new System.Drawing.Size(61, 24);
            this.btStartCopy.TabIndex = 0;
            this.btStartCopy.Text = "确定";
            this.btStartCopy.UseVisualStyleBackColor = true;
            this.btStartCopy.Click += new System.EventHandler(this.btStartCopy_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(138, 58);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(55, 24);
            this.button2.TabIndex = 1;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tBX
            // 
            this.tBX.Location = new System.Drawing.Point(83, 15);
            this.tBX.Name = "tBX";
            this.tBX.Size = new System.Drawing.Size(110, 21);
            this.tBX.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "目标图层：";
            // 
            // FeatureCopyTransToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(236, 94);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tBX);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btStartCopy);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FeatureCopyTransToolForm";
            this.Text = "要素拷贝";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btStartCopy;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox tBX;
        private System.Windows.Forms.Label label1;
    }
}
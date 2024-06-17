namespace BuildingGen {
    partial class GridSize {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.tbX = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.tbY = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(2, 6);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 16);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "显示网格";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // tbX
            // 
            this.tbX.Location = new System.Drawing.Point(75, 4);
            this.tbX.MaxLength = 8;
            this.tbX.Name = "tbX";
            this.tbX.Size = new System.Drawing.Size(79, 21);
            this.tbX.TabIndex = 1;
            this.tbX.Text = "4000";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(245, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tbY
            // 
            this.tbY.Location = new System.Drawing.Point(160, 4);
            this.tbY.MaxLength = 8;
            this.tbY.Name = "tbY";
            this.tbY.Size = new System.Drawing.Size(79, 21);
            this.tbY.TabIndex = 3;
            this.tbY.Text = "4000";
            // 
            // GridSize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 29);
            this.Controls.Add(this.tbY);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tbX);
            this.Controls.Add(this.checkBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "GridSize";
            this.Text = "网格大小";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox tbX;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox tbY;
    }
}
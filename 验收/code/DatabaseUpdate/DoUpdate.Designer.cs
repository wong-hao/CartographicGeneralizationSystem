namespace DatabaseUpdate
{
    partial class DoUpdate
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
          this.comboBox1 = new System.Windows.Forms.ComboBox();
          this.label1 = new System.Windows.Forms.Label();
          this.label2 = new System.Windows.Forms.Label();
          this.button1 = new System.Windows.Forms.Button();
          this.button2 = new System.Windows.Forms.Button();
          this.SuspendLayout();
          // 
          // comboBox1
          // 
          this.comboBox1.FormattingEnabled = true;
          this.comboBox1.Location = new System.Drawing.Point(71, 26);
          this.comboBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
          this.comboBox1.Name = "comboBox1";
          this.comboBox1.Size = new System.Drawing.Size(271, 23);
          this.comboBox1.TabIndex = 0;
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.Location = new System.Drawing.Point(16, 30);
          this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(0, 15);
          this.label1.TabIndex = 1;
          // 
          // label2
          // 
          this.label2.AutoSize = true;
          this.label2.Location = new System.Drawing.Point(16, 30);
          this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(45, 15);
          this.label2.TabIndex = 2;
          this.label2.Text = "图层:";
          // 
          // button1
          // 
          this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          this.button1.Location = new System.Drawing.Point(135, 81);
          this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
          this.button1.Name = "button1";
          this.button1.Size = new System.Drawing.Size(100, 29);
          this.button1.TabIndex = 3;
          this.button1.Text = "确定";
          this.button1.UseVisualStyleBackColor = true;
          this.button1.Click += new System.EventHandler(this.button1_Click);
          // 
          // button2
          // 
          this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          this.button2.Location = new System.Drawing.Point(244, 81);
          this.button2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
          this.button2.Name = "button2";
          this.button2.Size = new System.Drawing.Size(100, 29);
          this.button2.TabIndex = 4;
          this.button2.Text = "取消";
          this.button2.UseVisualStyleBackColor = true;
          this.button2.Click += new System.EventHandler(this.button2_Click);
          // 
          // DoUpdate
          // 
          this.AcceptButton = this.button1;
          this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.CancelButton = this.button2;
          this.ClientSize = new System.Drawing.Size(360, 128);
          this.Controls.Add(this.button2);
          this.Controls.Add(this.button1);
          this.Controls.Add(this.label2);
          this.Controls.Add(this.label1);
          this.Controls.Add(this.comboBox1);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
          this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
          this.Name = "DoUpdate";
          this.Text = "更新图层";
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}
namespace DomapTool
{
    partial class DoWithQueryFilterDlg
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
          this.label1 = new System.Windows.Forms.Label();
          this.comboBox1 = new System.Windows.Forms.ComboBox();
          this.button1 = new System.Windows.Forms.Button();
          this.button2 = new System.Windows.Forms.Button();
          this.groupBox2 = new System.Windows.Forms.GroupBox();
          this.textBox3 = new System.Windows.Forms.TextBox();
          this.label6 = new System.Windows.Forms.Label();
          this.textBox2 = new System.Windows.Forms.TextBox();
          this.label5 = new System.Windows.Forms.Label();
          this.groupBox2.SuspendLayout();
          this.SuspendLayout();
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.Location = new System.Drawing.Point(12, 16);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(41, 12);
          this.label1.TabIndex = 0;
          this.label1.Text = "图层：";
          // 
          // comboBox1
          // 
          this.comboBox1.FormattingEnabled = true;
          this.comboBox1.Location = new System.Drawing.Point(71, 12);
          this.comboBox1.Name = "comboBox1";
          this.comboBox1.Size = new System.Drawing.Size(200, 20);
          this.comboBox1.TabIndex = 1;
          this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
          // 
          // button1
          // 
          this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          this.button1.Location = new System.Drawing.Point(111, 140);
          this.button1.Name = "button1";
          this.button1.Size = new System.Drawing.Size(75, 23);
          this.button1.TabIndex = 7;
          this.button1.Text = "确定";
          this.button1.UseVisualStyleBackColor = true;
          this.button1.Click += new System.EventHandler(this.button1_Click);
          // 
          // button2
          // 
          this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          this.button2.Location = new System.Drawing.Point(196, 140);
          this.button2.Name = "button2";
          this.button2.Size = new System.Drawing.Size(75, 23);
          this.button2.TabIndex = 8;
          this.button2.Text = "取消";
          this.button2.UseVisualStyleBackColor = true;
          this.button2.Click += new System.EventHandler(this.button2_Click);
          // 
          // groupBox2
          // 
          this.groupBox2.Controls.Add(this.textBox3);
          this.groupBox2.Controls.Add(this.label6);
          this.groupBox2.Controls.Add(this.textBox2);
          this.groupBox2.Controls.Add(this.label5);
          this.groupBox2.Location = new System.Drawing.Point(14, 38);
          this.groupBox2.Name = "groupBox2";
          this.groupBox2.Size = new System.Drawing.Size(255, 87);
          this.groupBox2.TabIndex = 9;
          this.groupBox2.TabStop = false;
          this.groupBox2.Text = "毗邻参数";
          // 
          // textBox3
          // 
          this.textBox3.Location = new System.Drawing.Point(81, 54);
          this.textBox3.Name = "textBox3";
          this.textBox3.Size = new System.Drawing.Size(168, 21);
          this.textBox3.TabIndex = 3;
          this.textBox3.Text = "800";
          // 
          // label6
          // 
          this.label6.AutoSize = true;
          this.label6.Location = new System.Drawing.Point(10, 57);
          this.label6.Name = "label6";
          this.label6.Size = new System.Drawing.Size(65, 12);
          this.label6.TabIndex = 2;
          this.label6.Text = "最小面积：";
          // 
          // textBox2
          // 
          this.textBox2.Location = new System.Drawing.Point(57, 21);
          this.textBox2.Name = "textBox2";
          this.textBox2.Size = new System.Drawing.Size(192, 21);
          this.textBox2.TabIndex = 1;
          this.textBox2.Text = "10";
          // 
          // label5
          // 
          this.label5.AutoSize = true;
          this.label5.Location = new System.Drawing.Point(10, 24);
          this.label5.Name = "label5";
          this.label5.Size = new System.Drawing.Size(41, 12);
          this.label5.TabIndex = 0;
          this.label5.Text = "宽度：";
          // 
          // DoWithQueryFilterDlg
          // 
          this.AcceptButton = this.button1;
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.CancelButton = this.button2;
          this.ClientSize = new System.Drawing.Size(292, 175);
          this.Controls.Add(this.groupBox2);
          this.Controls.Add(this.button2);
          this.Controls.Add(this.button1);
          this.Controls.Add(this.comboBox1);
          this.Controls.Add(this.label1);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
          this.Name = "DoWithQueryFilterDlg";
          this.Text = "湖泊毗邻";
          this.groupBox2.ResumeLayout(false);
          this.groupBox2.PerformLayout();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox2;
    }
}
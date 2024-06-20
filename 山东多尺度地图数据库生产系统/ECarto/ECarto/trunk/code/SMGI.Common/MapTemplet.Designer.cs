namespace SMGI.Common
{
    partial class MapTemplet
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.input = new System.Windows.Forms.Button();
            this.output = new System.Windows.Forms.Button();
            this.save = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(272, 297);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.save);
            this.groupBox1.Controls.Add(this.output);
            this.groupBox1.Controls.Add(this.input);
            this.groupBox1.Location = new System.Drawing.Point(278, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(153, 119);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // input
            // 
            this.input.Location = new System.Drawing.Point(16, 31);
            this.input.Name = "input";
            this.input.Size = new System.Drawing.Size(56, 23);
            this.input.TabIndex = 1;
            this.input.Text = "导入";
            this.input.UseVisualStyleBackColor = true;
            // 
            // output
            // 
            this.output.Location = new System.Drawing.Point(78, 31);
            this.output.Name = "output";
            this.output.Size = new System.Drawing.Size(56, 23);
            this.output.TabIndex = 2;
            this.output.Text = "导出";
            this.output.UseVisualStyleBackColor = true;
            // 
            // save
            // 
            this.save.Location = new System.Drawing.Point(6, 73);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(77, 23);
            this.save.TabIndex = 3;
            this.save.Text = "保存数据库";
            this.save.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(89, 73);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(56, 23);
            this.button4.TabIndex = 4;
            this.button4.Text = "移除";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(294, 167);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(56, 23);
            this.button5.TabIndex = 4;
            this.button5.Text = "确定";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(356, 167);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(56, 23);
            this.cancel.TabIndex = 5;
            this.cancel.Text = "取消";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // MapTemplet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 296);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView1);
            this.Name = "MapTemplet";
            this.Text = "模板管理";
            this.Load += new System.EventHandler(this.MapTemplet_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button input;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.Button output;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button cancel;
    }
}
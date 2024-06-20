namespace DomapTool
{
    partial class GenParaDlg
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
          this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
          this.Load = new System.Windows.Forms.Button();
          this.Save = new System.Windows.Forms.Button();
          this.btSave = new System.Windows.Forms.Button();
          this.SuspendLayout();
          // 
          // propertyGrid1
          // 
          this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
          this.propertyGrid1.Name = "propertyGrid1";
          this.propertyGrid1.Size = new System.Drawing.Size(481, 425);
          this.propertyGrid1.TabIndex = 0;
          // 
          // Load
          // 
          this.Load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.Load.Location = new System.Drawing.Point(12, 433);
          this.Load.Name = "Load";
          this.Load.Size = new System.Drawing.Size(75, 23);
          this.Load.TabIndex = 1;
          this.Load.Text = "从文件导入";
          this.Load.UseVisualStyleBackColor = true;
          this.Load.Click += new System.EventHandler(this.Load_Click);
          // 
          // Save
          // 
          this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
          this.Save.Location = new System.Drawing.Point(93, 433);
          this.Save.Name = "Save";
          this.Save.Size = new System.Drawing.Size(75, 23);
          this.Save.TabIndex = 2;
          this.Save.Text = "导出至文件";
          this.Save.UseVisualStyleBackColor = true;
          this.Save.Click += new System.EventHandler(this.Save_Click);
          // 
          // btSave
          // 
          this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
          this.btSave.Location = new System.Drawing.Point(394, 433);
          this.btSave.Name = "btSave";
          this.btSave.Size = new System.Drawing.Size(75, 23);
          this.btSave.TabIndex = 3;
          this.btSave.Text = "保存";
          this.btSave.UseVisualStyleBackColor = true;
          this.btSave.Click += new System.EventHandler(this.btSave_Click);
          // 
          // GenParaDlg
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(481, 468);
          this.Controls.Add(this.btSave);
          this.Controls.Add(this.Save);
          this.Controls.Add(this.Load);
          this.Controls.Add(this.propertyGrid1);
          this.Name = "GenParaDlg";
          this.Text = "综合参数设置";
          this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GenParaDlg_FormClosing);
          this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button Load;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button btSave;
    }
}
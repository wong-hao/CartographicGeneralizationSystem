namespace SMGI.Plugin.ThematicChart.Common
{
    partial class FrmExcelSet
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
            this.cmbShs = new System.Windows.Forms.ComboBox();
            this.btOk = new System.Windows.Forms.Button();
            this.cbRowOrColumn = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "选择excel表单：";
            // 
            // cmbShs
            // 
            this.cmbShs.FormattingEnabled = true;
            this.cmbShs.Location = new System.Drawing.Point(113, 18);
            this.cmbShs.Name = "cmbShs";
            this.cmbShs.Size = new System.Drawing.Size(206, 20);
            this.cmbShs.TabIndex = 1;
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(252, 58);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(67, 32);
            this.btOk.TabIndex = 2;
            this.btOk.Text = "确定";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // cbRowOrColumn
            // 
            this.cbRowOrColumn.AutoSize = true;
            this.cbRowOrColumn.Location = new System.Drawing.Point(14, 48);
            this.cbRowOrColumn.Name = "cbRowOrColumn";
            this.cbRowOrColumn.Size = new System.Drawing.Size(72, 16);
            this.cbRowOrColumn.TabIndex = 4;
            this.cbRowOrColumn.Text = "行列互换";
            this.cbRowOrColumn.UseVisualStyleBackColor = true;
            // 
            // FrmExcelSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 98);
            this.Controls.Add(this.cbRowOrColumn);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.cmbShs);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmExcelSet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "选取表";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbShs;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.CheckBox cbRowOrColumn;
    }
}
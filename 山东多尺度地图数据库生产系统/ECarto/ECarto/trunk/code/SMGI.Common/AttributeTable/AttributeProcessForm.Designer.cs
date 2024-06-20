namespace SMGI.Common.AttributeTable
{
    partial class AttributeProcessForm
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
            this.tbModifyFieldValue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.checkButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.cmbFieldNames = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // tbModifyFieldValue
            // 
            this.tbModifyFieldValue.Location = new System.Drawing.Point(245, 28);
            this.tbModifyFieldValue.Name = "tbModifyFieldValue";
            this.tbModifyFieldValue.Size = new System.Drawing.Size(110, 21);
            this.tbModifyFieldValue.TabIndex = 97;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(225, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(11, 12);
            this.label4.TabIndex = 96;
            this.label4.Text = "=";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(62, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 94;
            this.label5.Text = "字段名:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(3, 61);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(426, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 98;
            // 
            // checkButton
            // 
            this.checkButton.Location = new System.Drawing.Point(116, 105);
            this.checkButton.Name = "checkButton";
            this.checkButton.Size = new System.Drawing.Size(67, 38);
            this.checkButton.TabIndex = 99;
            this.checkButton.Text = "开始";
            this.checkButton.UseVisualStyleBackColor = true;
            this.checkButton.Click += new System.EventHandler(this.checkButton_Click);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(256, 105);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(67, 38);
            this.button1.TabIndex = 100;
            this.button1.Text = "取消";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // cmbFieldNames
            // 
            this.cmbFieldNames.FormattingEnabled = true;
            this.cmbFieldNames.Location = new System.Drawing.Point(116, 28);
            this.cmbFieldNames.Name = "cmbFieldNames";
            this.cmbFieldNames.Size = new System.Drawing.Size(103, 20);
            this.cmbFieldNames.TabIndex = 101;
            // 
            // AttributeProcessForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 155);
            this.Controls.Add(this.cmbFieldNames);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkButton);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.tbModifyFieldValue);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "AttributeProcessForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "属性统改";
            this.Load += new System.EventHandler(this.btnAttributeProcess_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbModifyFieldValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button checkButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cmbFieldNames;
    }
}
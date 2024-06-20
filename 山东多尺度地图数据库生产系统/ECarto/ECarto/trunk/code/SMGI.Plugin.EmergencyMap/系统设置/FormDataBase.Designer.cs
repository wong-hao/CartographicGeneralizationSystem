namespace SMGI.Plugin.EmergencyMap.DataSource
{
    partial class DataBaseForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataBaseForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbScale = new System.Windows.Forms.TextBox();
            this.tbMinScale = new System.Windows.Forms.TextBox();
            this.tbMaxScale = new System.Windows.Forms.TextBox();
            this.tbDataBase = new System.Windows.Forms.TextBox();
            this.tbTemplate = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "基本比例尺:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "最小比例尺:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "最大比例尺:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "数据库:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "模板:";
            // 
            // tbScale
            // 
            this.tbScale.Location = new System.Drawing.Point(89, 6);
            this.tbScale.Name = "tbScale";
            this.tbScale.Size = new System.Drawing.Size(172, 21);
            this.tbScale.TabIndex = 5;
            // 
            // tbMinScale
            // 
            this.tbMinScale.Location = new System.Drawing.Point(89, 59);
            this.tbMinScale.Name = "tbMinScale";
            this.tbMinScale.Size = new System.Drawing.Size(172, 21);
            this.tbMinScale.TabIndex = 7;
            // 
            // tbMaxScale
            // 
            this.tbMaxScale.Location = new System.Drawing.Point(89, 32);
            this.tbMaxScale.Name = "tbMaxScale";
            this.tbMaxScale.Size = new System.Drawing.Size(172, 21);
            this.tbMaxScale.TabIndex = 6;
            // 
            // tbDataBase
            // 
            this.tbDataBase.Location = new System.Drawing.Point(89, 86);
            this.tbDataBase.Name = "tbDataBase";
            this.tbDataBase.Size = new System.Drawing.Size(172, 21);
            this.tbDataBase.TabIndex = 8;
            // 
            // tbTemplate
            // 
            this.tbTemplate.Location = new System.Drawing.Point(89, 113);
            this.tbTemplate.Name = "tbTemplate";
            this.tbTemplate.Size = new System.Drawing.Size(172, 21);
            this.tbTemplate.TabIndex = 9;
            // 
            // btnOK
            // 
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(184, 141);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(77, 45);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // DataBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(263, 192);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tbTemplate);
            this.Controls.Add(this.tbDataBase);
            this.Controls.Add(this.tbMaxScale);
            this.Controls.Add(this.tbMinScale);
            this.Controls.Add(this.tbScale);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DataBaseForm";
            this.Text = "多尺度数据库";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbScale;
        private System.Windows.Forms.TextBox tbMinScale;
        private System.Windows.Forms.TextBox tbMaxScale;
        private System.Windows.Forms.TextBox tbDataBase;
        private System.Windows.Forms.TextBox tbTemplate;
        private System.Windows.Forms.Button btnOK;
    }
}
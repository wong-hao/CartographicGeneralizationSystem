namespace SMGI.Plugin.EmergencyMap
{
    partial class AddHillShadingForm
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
            this.cbInputRaster = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbAzimuth = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbAltitude = new System.Windows.Forms.TextBox();
            this.cbInModelShadows = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbZFactor = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "输入栅格";
            // 
            // cbInputRaster
            // 
            this.cbInputRaster.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbInputRaster.FormattingEnabled = true;
            this.cbInputRaster.Location = new System.Drawing.Point(15, 29);
            this.cbInputRaster.Name = "cbInputRaster";
            this.cbInputRaster.Size = new System.Drawing.Size(365, 20);
            this.cbInputRaster.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "方位角";
            // 
            // tbAzimuth
            // 
            this.tbAzimuth.Location = new System.Drawing.Point(15, 77);
            this.tbAzimuth.Name = "tbAzimuth";
            this.tbAzimuth.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbAzimuth.Size = new System.Drawing.Size(365, 21);
            this.tbAzimuth.TabIndex = 2;
            this.tbAzimuth.Text = "315";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "高度角";
            // 
            // tbAltitude
            // 
            this.tbAltitude.Location = new System.Drawing.Point(15, 127);
            this.tbAltitude.Name = "tbAltitude";
            this.tbAltitude.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbAltitude.Size = new System.Drawing.Size(365, 21);
            this.tbAltitude.TabIndex = 2;
            this.tbAltitude.Text = "45";
            // 
            // cbInModelShadows
            // 
            this.cbInModelShadows.AutoSize = true;
            this.cbInModelShadows.Location = new System.Drawing.Point(15, 155);
            this.cbInModelShadows.Name = "cbInModelShadows";
            this.cbInModelShadows.Size = new System.Drawing.Size(72, 16);
            this.cbInModelShadows.TabIndex = 3;
            this.cbInModelShadows.Text = "模拟阴影";
            this.cbInModelShadows.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 176);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "Z因子";
            // 
            // tbZFactor
            // 
            this.tbZFactor.Location = new System.Drawing.Point(14, 192);
            this.tbZFactor.Name = "tbZFactor";
            this.tbZFactor.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbZFactor.Size = new System.Drawing.Size(365, 21);
            this.tbZFactor.TabIndex = 2;
            this.tbZFactor.Text = "1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 236);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(392, 31);
            this.panel1.TabIndex = 14;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(251, 4);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(64, 23);
            this.btOK.TabIndex = 7;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(315, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(324, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // AddHillShadingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 267);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cbInModelShadows);
            this.Controls.Add(this.tbZFactor);
            this.Controls.Add(this.tbAltitude);
            this.Controls.Add(this.tbAzimuth);
            this.Controls.Add(this.cbInputRaster);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "AddHillShadingForm";
            this.Text = "山体阴影";
            this.Load += new System.EventHandler(this.AddHillShadingForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbInputRaster;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbAzimuth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbAltitude;
        private System.Windows.Forms.CheckBox cbInModelShadows;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbZFactor;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
    }
}
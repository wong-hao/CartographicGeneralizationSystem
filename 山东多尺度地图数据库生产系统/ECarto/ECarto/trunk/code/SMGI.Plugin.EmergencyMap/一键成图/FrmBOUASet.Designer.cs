namespace SMGI.Plugin.EmergencyMap.OneKey
{
    partial class FrmBOUASet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBOUASet));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lbAttach = new System.Windows.Forms.Label();
            this.AttachColor = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ColorPanelBOUA = new System.Windows.Forms.Panel();
            this.cmbColorBOUA = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.cbLevel = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lbAttach);
            this.groupBox2.Controls.Add(this.AttachColor);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.cmbColorBOUA);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(459, 160);
            this.groupBox2.TabIndex = 95;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "普色设置";
            // 
            // lbAttach
            // 
            this.lbAttach.AutoSize = true;
            this.lbAttach.Location = new System.Drawing.Point(255, 29);
            this.lbAttach.Name = "lbAttach";
            this.lbAttach.Size = new System.Drawing.Size(65, 12);
            this.lbAttach.TabIndex = 17;
            this.lbAttach.Text = "附区颜色：";
            // 
            // AttachColor
            // 
            this.AttachColor.BackColor = System.Drawing.Color.White;
            this.AttachColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AttachColor.Location = new System.Drawing.Point(335, 24);
            this.AttachColor.Name = "AttachColor";
            this.AttachColor.Size = new System.Drawing.Size(116, 20);
            this.AttachColor.TabIndex = 16;
            this.AttachColor.UseVisualStyleBackColor = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ColorPanelBOUA);
            this.groupBox3.Location = new System.Drawing.Point(11, 50);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(446, 103);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            // 
            // ColorPanelBOUA
            // 
            this.ColorPanelBOUA.AutoScroll = true;
            this.ColorPanelBOUA.Location = new System.Drawing.Point(3, 10);
            this.ColorPanelBOUA.Name = "ColorPanelBOUA";
            this.ColorPanelBOUA.Size = new System.Drawing.Size(434, 87);
            this.ColorPanelBOUA.TabIndex = 17;
            // 
            // cmbColorBOUA
            // 
            this.cmbColorBOUA.FormattingEnabled = true;
            this.cmbColorBOUA.Location = new System.Drawing.Point(84, 24);
            this.cmbColorBOUA.Name = "cmbColorBOUA";
            this.cmbColorBOUA.Size = new System.Drawing.Size(123, 20);
            this.cmbColorBOUA.TabIndex = 14;
            this.cmbColorBOUA.SelectedIndexChanged += new System.EventHandler(this.cmbColorBOUA_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 27);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 8;
            this.label8.Text = "颜色方案：";
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(331, 178);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(59, 31);
            this.btOK.TabIndex = 98;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(387, 182);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(25, 23);
            this.panel2.TabIndex = 97;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(411, 178);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(58, 31);
            this.btCancel.TabIndex = 96;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // cbLevel
            // 
            this.cbLevel.FormattingEnabled = true;
            this.cbLevel.Location = new System.Drawing.Point(96, 187);
            this.cbLevel.Name = "cbLevel";
            this.cbLevel.Size = new System.Drawing.Size(123, 20);
            this.cbLevel.TabIndex = 100;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 190);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 99;
            this.label1.Text = "普色级别：";
            // 
            // FrmBOUASet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 220);
            this.Controls.Add(this.cbLevel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmBOUASet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "境界普色设置";
            this.Load += new System.EventHandler(this.FrmBOUASet_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lbAttach;
        private System.Windows.Forms.Button AttachColor;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel ColorPanelBOUA;
        public System.Windows.Forms.ComboBox cmbColorBOUA;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        public System.Windows.Forms.ComboBox cbLevel;
        private System.Windows.Forms.Label label1;
    }
}
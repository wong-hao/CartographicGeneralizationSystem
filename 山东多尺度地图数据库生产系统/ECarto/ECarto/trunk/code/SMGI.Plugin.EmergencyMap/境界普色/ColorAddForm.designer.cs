namespace SMGI.Plugin.EmergencyMap
{
    partial class ColorAddForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorAddForm));
            this.label8 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.C = new System.Windows.Forms.TextBox();
            this.M = new System.Windows.Forms.TextBox();
            this.txtm = new System.Windows.Forms.Label();
            this.K = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Y = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.Plan = new System.Windows.Forms.NumericUpDown();
            this.ColorPanel = new System.Windows.Forms.GroupBox();
            this.BtAdd = new System.Windows.Forms.Button();
            this.btRemove = new System.Windows.Forms.Button();
            this.lbInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Plan)).BeginInit();
            this.SuspendLayout();
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(11, 19);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 19;
            this.label8.Text = "颜色方案：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(55, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(11, 12);
            this.label1.TabIndex = 21;
            this.label1.Text = "C";
            // 
            // C
            // 
            this.C.Location = new System.Drawing.Point(81, 44);
            this.C.Name = "C";
            this.C.Size = new System.Drawing.Size(71, 21);
            this.C.TabIndex = 23;
            this.C.Text = "0";
            // 
            // M
            // 
            this.M.Location = new System.Drawing.Point(246, 44);
            this.M.Name = "M";
            this.M.Size = new System.Drawing.Size(73, 21);
            this.M.TabIndex = 25;
            this.M.Text = "0";
            // 
            // txtm
            // 
            this.txtm.AutoSize = true;
            this.txtm.Location = new System.Drawing.Point(219, 47);
            this.txtm.Name = "txtm";
            this.txtm.Size = new System.Drawing.Size(11, 12);
            this.txtm.TabIndex = 24;
            this.txtm.Text = "M";
            // 
            // K
            // 
            this.K.Location = new System.Drawing.Point(246, 74);
            this.K.Name = "K";
            this.K.Size = new System.Drawing.Size(73, 21);
            this.K.TabIndex = 29;
            this.K.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(219, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(11, 12);
            this.label3.TabIndex = 28;
            this.label3.Text = "K";
            // 
            // Y
            // 
            this.Y.Location = new System.Drawing.Point(81, 71);
            this.Y.Name = "Y";
            this.Y.Size = new System.Drawing.Size(71, 21);
            this.Y.TabIndex = 27;
            this.Y.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(55, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(11, 12);
            this.label4.TabIndex = 26;
            this.label4.Text = "Y";
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(297, 207);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(62, 37);
            this.btCancel.TabIndex = 33;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(236, 207);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(57, 37);
            this.btOK.TabIndex = 32;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // Plan
            // 
            this.Plan.Location = new System.Drawing.Point(81, 17);
            this.Plan.Name = "Plan";
            this.Plan.Size = new System.Drawing.Size(71, 21);
            this.Plan.TabIndex = 35;
            // 
            // ColorPanel
            // 
            this.ColorPanel.Location = new System.Drawing.Point(13, 101);
            this.ColorPanel.Name = "ColorPanel";
            this.ColorPanel.Size = new System.Drawing.Size(346, 100);
            this.ColorPanel.TabIndex = 37;
            this.ColorPanel.TabStop = false;
            this.ColorPanel.Text = "颜色";
            // 
            // BtAdd
            // 
            this.BtAdd.Image = ((System.Drawing.Image)(resources.GetObject("BtAdd.Image")));
            this.BtAdd.Location = new System.Drawing.Point(365, 110);
            this.BtAdd.Name = "BtAdd";
            this.BtAdd.Size = new System.Drawing.Size(28, 28);
            this.BtAdd.TabIndex = 38;
            this.BtAdd.UseVisualStyleBackColor = true;
            this.BtAdd.Click += new System.EventHandler(this.BtAdd_Click);
            // 
            // btRemove
            // 
            this.btRemove.Image = ((System.Drawing.Image)(resources.GetObject("btRemove.Image")));
            this.btRemove.Location = new System.Drawing.Point(365, 144);
            this.btRemove.Name = "btRemove";
            this.btRemove.Size = new System.Drawing.Size(28, 28);
            this.btRemove.TabIndex = 39;
            this.btRemove.UseVisualStyleBackColor = true;
            this.btRemove.Click += new System.EventHandler(this.btRemove_Click);
            // 
            // lbInfo
            // 
            this.lbInfo.AutoSize = true;
            this.lbInfo.Location = new System.Drawing.Point(9, 204);
            this.lbInfo.Name = "lbInfo";
            this.lbInfo.Size = new System.Drawing.Size(221, 12);
            this.lbInfo.TabIndex = 40;
            this.lbInfo.Text = "邻区颜色按照省内、省外、国外依次添加";
            // 
            // FrmColorAdd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 245);
            this.Controls.Add(this.lbInfo);
            this.Controls.Add(this.btRemove);
            this.Controls.Add(this.BtAdd);
            this.Controls.Add(this.ColorPanel);
            this.Controls.Add(this.Plan);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.K);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Y);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.M);
            this.Controls.Add(this.txtm);
            this.Controls.Add(this.C);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label8);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmColorAdd";
            this.Text = "颜色添加";
            this.Load += new System.EventHandler(this.FrmColorAdd_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Plan)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox C;
        private System.Windows.Forms.TextBox M;
        private System.Windows.Forms.Label txtm;
        private System.Windows.Forms.TextBox K;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Y;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.NumericUpDown Plan;
        private System.Windows.Forms.GroupBox ColorPanel;
        private System.Windows.Forms.Button BtAdd;
        private System.Windows.Forms.Button btRemove;
        private System.Windows.Forms.Label lbInfo;
    }
}
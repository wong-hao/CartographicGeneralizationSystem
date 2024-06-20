namespace SMGI.Plugin.MapGeneralization
{
    partial class FrmLineConficlt
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
            this.btCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btOK = new DevExpress.XtraEditors.SimpleButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbRiver = new System.Windows.Forms.ComboBox();
            this.cmbRoad = new System.Windows.Forms.ComboBox();
            this.tbDistance = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(247, 119);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 33);
            this.btCancel.TabIndex = 17;
            this.btCancel.Text = "取消";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(139, 119);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 33);
            this.btOK.TabIndex = 16;
            this.btOK.Text = "确定";
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 14);
            this.label1.TabIndex = 18;
            this.label1.Text = "距离阀值设置";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 14);
            this.label2.TabIndex = 19;
            this.label2.Text = "水系图层";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 14);
            this.label3.TabIndex = 20;
            this.label3.Text = "道路图层";
            // 
            // cmbRiver
            // 
            this.cmbRiver.FormattingEnabled = true;
            this.cmbRiver.Location = new System.Drawing.Point(112, 39);
            this.cmbRiver.Name = "cmbRiver";
            this.cmbRiver.Size = new System.Drawing.Size(210, 22);
            this.cmbRiver.TabIndex = 21;
            this.cmbRiver.SelectedIndexChanged += new System.EventHandler(this.cmbRiver_SelectedIndexChanged);
            // 
            // cmbRoad
            // 
            this.cmbRoad.FormattingEnabled = true;
            this.cmbRoad.Location = new System.Drawing.Point(112, 70);
            this.cmbRoad.Name = "cmbRoad";
            this.cmbRoad.Size = new System.Drawing.Size(210, 22);
            this.cmbRoad.TabIndex = 22;
            this.cmbRoad.SelectedIndexChanged += new System.EventHandler(this.cmbRoad_SelectedIndexChanged);
            // 
            // tbDistance
            // 
            this.tbDistance.Location = new System.Drawing.Point(112, 12);
            this.tbDistance.Name = "tbDistance";
            this.tbDistance.Size = new System.Drawing.Size(210, 22);
            this.tbDistance.TabIndex = 23;
            this.tbDistance.Text = "50";
            // 
            // FrmLineConficlt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 164);
            this.Controls.Add(this.tbDistance);
            this.Controls.Add(this.cmbRoad);
            this.Controls.Add(this.cmbRiver);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FrmLineConficlt";
            this.Text = "水路距离设置";
            this.Load += new System.EventHandler(this.FrmLineConficlt_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btCancel;
        private DevExpress.XtraEditors.SimpleButton btOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbRiver;
        private System.Windows.Forms.ComboBox cmbRoad;
        private System.Windows.Forms.TextBox tbDistance;
    }
}
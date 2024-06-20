namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    partial class FrmThematicBcg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmThematicBcg));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ColorPan = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.btrandom = new System.Windows.Forms.Button();
            this.btcancel = new System.Windows.Forms.Button();
            this.btok = new System.Windows.Forms.Button();
            this.txtPieTitle = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbGeo = new System.Windows.Forms.ComboBox();
            this.btDsOpen = new System.Windows.Forms.Button();
            this.txtChartDs = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.btrandom);
            this.splitContainer1.Panel2.Controls.Add(this.btcancel);
            this.splitContainer1.Panel2.Controls.Add(this.btok);
            this.splitContainer1.Panel2.Controls.Add(this.txtPieTitle);
            this.splitContainer1.Panel2.Controls.Add(this.label6);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.cmbGeo);
            this.splitContainer1.Panel2.Controls.Add(this.btDsOpen);
            this.splitContainer1.Panel2.Controls.Add(this.txtChartDs);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Size = new System.Drawing.Size(632, 428);
            this.splitContainer1.SplitterDistance = 210;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ColorPan);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(632, 210);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "颜色设置";
            // 
            // ColorPan
            // 
            this.ColorPan.AutoScroll = true;
            this.ColorPan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ColorPan.Location = new System.Drawing.Point(3, 17);
            this.ColorPan.Name = "ColorPan";
            this.ColorPan.Size = new System.Drawing.Size(626, 190);
            this.ColorPan.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 38;
            this.label2.Text = "颜色选择:";
            // 
            // btrandom
            // 
            this.btrandom.Location = new System.Drawing.Point(375, 158);
            this.btrandom.Name = "btrandom";
            this.btrandom.Size = new System.Drawing.Size(68, 30);
            this.btrandom.TabIndex = 37;
            this.btrandom.Text = "颜色随机";
            this.btrandom.UseVisualStyleBackColor = true;
            this.btrandom.Visible = false;
            this.btrandom.Click += new System.EventHandler(this.btrandom_Click);
            // 
            // btcancel
            // 
            this.btcancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btcancel.Location = new System.Drawing.Point(554, 158);
            this.btcancel.Name = "btcancel";
            this.btcancel.Size = new System.Drawing.Size(63, 30);
            this.btcancel.TabIndex = 36;
            this.btcancel.Text = "取消";
            this.btcancel.UseVisualStyleBackColor = true;
            this.btcancel.Click += new System.EventHandler(this.btcancel_Click);
            // 
            // btok
            // 
            this.btok.Location = new System.Drawing.Point(463, 158);
            this.btok.Name = "btok";
            this.btok.Size = new System.Drawing.Size(62, 30);
            this.btok.TabIndex = 35;
            this.btok.Text = "确定";
            this.btok.UseVisualStyleBackColor = true;
            this.btok.Click += new System.EventHandler(this.btok_Click);
            // 
            // txtPieTitle
            // 
            this.txtPieTitle.Location = new System.Drawing.Point(110, 125);
            this.txtPieTitle.Name = "txtPieTitle";
            this.txtPieTitle.Size = new System.Drawing.Size(410, 21);
            this.txtPieTitle.TabIndex = 34;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(25, 128);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 33;
            this.label6.Text = "统计图标题：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 32;
            this.label1.Text = "底质法图层：";
            // 
            // cmbGeo
            // 
            this.cmbGeo.FormattingEnabled = true;
            this.cmbGeo.Location = new System.Drawing.Point(110, 88);
            this.cmbGeo.Name = "cmbGeo";
            this.cmbGeo.Size = new System.Drawing.Size(200, 20);
            this.cmbGeo.TabIndex = 31;
            this.cmbGeo.SelectedIndexChanged += new System.EventHandler(this.cmbGeo_SelectedIndexChanged);
            // 
            // btDsOpen
            // 
            this.btDsOpen.Location = new System.Drawing.Point(538, 19);
            this.btDsOpen.Name = "btDsOpen";
            this.btDsOpen.Size = new System.Drawing.Size(45, 20);
            this.btDsOpen.TabIndex = 20;
            this.btDsOpen.Text = "浏览";
            this.btDsOpen.UseVisualStyleBackColor = true;
            this.btDsOpen.Click += new System.EventHandler(this.btDsOpen_Click);
            // 
            // txtChartDs
            // 
            this.txtChartDs.Location = new System.Drawing.Point(110, 20);
            this.txtChartDs.Name = "txtChartDs";
            this.txtChartDs.ReadOnly = true;
            this.txtChartDs.Size = new System.Drawing.Size(408, 21);
            this.txtChartDs.TabIndex = 19;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 18;
            this.label3.Text = "数据源设置：";
            // 
            // FrmThematicBcg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 428);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmThematicBcg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "专题图质底法设置";
            this.Load += new System.EventHandler(this.FrmThematicBcg_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel ColorPan;
        private System.Windows.Forms.Button btDsOpen;
        private System.Windows.Forms.TextBox txtChartDs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbGeo;
        private System.Windows.Forms.TextBox txtPieTitle;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btrandom;
        private System.Windows.Forms.Button btcancel;
        private System.Windows.Forms.Button btok;
        private System.Windows.Forms.Label label2;
    }
}
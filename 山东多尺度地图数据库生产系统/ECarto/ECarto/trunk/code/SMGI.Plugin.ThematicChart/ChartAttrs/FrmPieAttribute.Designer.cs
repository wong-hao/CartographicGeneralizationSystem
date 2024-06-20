namespace SMGI.Plugin.ThematicChart.ChartAttrs
{
    partial class FrmPieAttribute
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPieAttribute));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ColorPan = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            this.TotalStatic = new System.Windows.Forms.CheckBox();
            this.RingRate = new System.Windows.Forms.TextBox();
            this.lbRing = new System.Windows.Forms.Label();
            this.EllipseRate = new System.Windows.Forms.TextBox();
            this.lbEllipseRate = new System.Windows.Forms.Label();
            this.TxtSize = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lbCmb = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.Title = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dataGV_reuslt = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGV_reuslt)).BeginInit();
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
            this.splitContainer1.Panel2.Controls.Add(this.btCancel);
            this.splitContainer1.Panel2.Controls.Add(this.btOk);
            this.splitContainer1.Panel2.Controls.Add(this.TotalStatic);
            this.splitContainer1.Panel2.Controls.Add(this.RingRate);
            this.splitContainer1.Panel2.Controls.Add(this.lbRing);
            this.splitContainer1.Panel2.Controls.Add(this.EllipseRate);
            this.splitContainer1.Panel2.Controls.Add(this.lbEllipseRate);
            this.splitContainer1.Panel2.Controls.Add(this.TxtSize);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.lbCmb);
            this.splitContainer1.Panel2.Controls.Add(this.label19);
            this.splitContainer1.Panel2.Controls.Add(this.Title);
            this.splitContainer1.Panel2.Controls.Add(this.label6);
            this.splitContainer1.Panel2.Controls.Add(this.dataGV_reuslt);
            this.splitContainer1.Size = new System.Drawing.Size(301, 485);
            this.splitContainer1.SplitterDistance = 146;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ColorPan);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(301, 146);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "颜色属性";
            // 
            // ColorPan
            // 
            this.ColorPan.AutoScroll = true;
            this.ColorPan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ColorPan.Location = new System.Drawing.Point(3, 17);
            this.ColorPan.Name = "ColorPan";
            this.ColorPan.Size = new System.Drawing.Size(295, 126);
            this.ColorPan.TabIndex = 1;
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(222, 298);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(67, 28);
            this.btCancel.TabIndex = 33;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(135, 298);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(71, 28);
            this.btOk.TabIndex = 32;
            this.btOk.Text = "确定";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // TotalStatic
            // 
            this.TotalStatic.AutoSize = true;
            this.TotalStatic.Location = new System.Drawing.Point(90, 276);
            this.TotalStatic.Name = "TotalStatic";
            this.TotalStatic.Size = new System.Drawing.Size(96, 16);
            this.TotalStatic.TabIndex = 31;
            this.TotalStatic.Text = "显示统计总值";
            this.TotalStatic.UseVisualStyleBackColor = true;
            // 
            // RingRate
            // 
            this.RingRate.Location = new System.Drawing.Point(90, 249);
            this.RingRate.Name = "RingRate";
            this.RingRate.Size = new System.Drawing.Size(100, 21);
            this.RingRate.TabIndex = 30;
            // 
            // lbRing
            // 
            this.lbRing.AutoSize = true;
            this.lbRing.Location = new System.Drawing.Point(7, 253);
            this.lbRing.Name = "lbRing";
            this.lbRing.Size = new System.Drawing.Size(77, 12);
            this.lbRing.TabIndex = 29;
            this.lbRing.Text = "椭圆环比率：";
            // 
            // EllipseRate
            // 
            this.EllipseRate.Location = new System.Drawing.Point(90, 222);
            this.EllipseRate.Name = "EllipseRate";
            this.EllipseRate.Size = new System.Drawing.Size(100, 21);
            this.EllipseRate.TabIndex = 28;
            // 
            // lbEllipseRate
            // 
            this.lbEllipseRate.AutoSize = true;
            this.lbEllipseRate.Location = new System.Drawing.Point(7, 226);
            this.lbEllipseRate.Name = "lbEllipseRate";
            this.lbEllipseRate.Size = new System.Drawing.Size(77, 12);
            this.lbEllipseRate.TabIndex = 27;
            this.lbEllipseRate.Text = "长短轴比率：";
            // 
            // TxtSize
            // 
            this.TxtSize.Location = new System.Drawing.Point(90, 197);
            this.TxtSize.Name = "TxtSize";
            this.TxtSize.Size = new System.Drawing.Size(199, 21);
            this.TxtSize.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 200);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 25;
            this.label2.Text = "统计图尺寸：";
            // 
            // lbCmb
            // 
            this.lbCmb.FormattingEnabled = true;
            this.lbCmb.Items.AddRange(new object[] {
            "图例式标注",
            "引线式标注",
            "压盖式标注",
            "无标注"});
            this.lbCmb.Location = new System.Drawing.Point(90, 174);
            this.lbCmb.Name = "lbCmb";
            this.lbCmb.Size = new System.Drawing.Size(199, 20);
            this.lbCmb.TabIndex = 24;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(19, 177);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(65, 12);
            this.label19.TabIndex = 23;
            this.label19.Text = "标注方式：";
            // 
            // Title
            // 
            this.Title.Location = new System.Drawing.Point(90, 148);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(199, 21);
            this.Title.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 154);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "统计图标题：";
            // 
            // dataGV_reuslt
            // 
            this.dataGV_reuslt.AllowUserToAddRows = false;
            this.dataGV_reuslt.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGV_reuslt.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGV_reuslt.Location = new System.Drawing.Point(3, 5);
            this.dataGV_reuslt.Name = "dataGV_reuslt";
            this.dataGV_reuslt.RowTemplate.Height = 23;
            this.dataGV_reuslt.Size = new System.Drawing.Size(295, 137);
            this.dataGV_reuslt.TabIndex = 0;
            // 
            // FrmPieAttribute
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(301, 485);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmPieAttribute";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "饼图属性";
            this.Load += new System.EventHandler(this.FrmPieAttribute_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGV_reuslt)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel ColorPan;
        private System.Windows.Forms.DataGridView dataGV_reuslt;
        private System.Windows.Forms.TextBox Title;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.ComboBox lbCmb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtSize;
        private System.Windows.Forms.TextBox EllipseRate;
        private System.Windows.Forms.Label lbEllipseRate;
        private System.Windows.Forms.TextBox RingRate;
        private System.Windows.Forms.Label lbRing;
        private System.Windows.Forms.CheckBox TotalStatic;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOk;


    }
}
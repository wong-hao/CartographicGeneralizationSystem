namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    partial class FrmBarChartsSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBarChartsSet));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ColorPan = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btrandom = new System.Windows.Forms.Button();
            this.btcancel = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtKD = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbXYAxis = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lbGeo = new System.Windows.Forms.Label();
            this.cmbGeo = new System.Windows.Forms.ComboBox();
            this.cbGeo = new System.Windows.Forms.CheckBox();
            this.cbLengend = new System.Windows.Forms.CheckBox();
            this.btPieOpen = new System.Windows.Forms.Button();
            this.txtDataSource = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPieTitle = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPieSize = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btok = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.button5 = new System.Windows.Forms.Button();
            this.textBox14 = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.textBox15 = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.textBox16 = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.splitContainer1);
            this.groupBox1.Location = new System.Drawing.Point(3, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(598, 461);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 17);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(592, 441);
            this.splitContainer1.SplitterDistance = 177;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ColorPan);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(592, 177);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "颜色设置";
            // 
            // ColorPan
            // 
            this.ColorPan.AutoScroll = true;
            this.ColorPan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ColorPan.Location = new System.Drawing.Point(3, 17);
            this.ColorPan.Name = "ColorPan";
            this.ColorPan.Size = new System.Drawing.Size(586, 157);
            this.ColorPan.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(-10, -25);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(653, 294);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.btrandom);
            this.tabPage1.Controls.Add(this.btcancel);
            this.tabPage1.Controls.Add(this.groupBox4);
            this.tabPage1.Controls.Add(this.btok);
            this.tabPage1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(645, 268);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "三维饼图";
            // 
            // btrandom
            // 
            this.btrandom.Location = new System.Drawing.Point(334, 233);
            this.btrandom.Name = "btrandom";
            this.btrandom.Size = new System.Drawing.Size(68, 23);
            this.btrandom.TabIndex = 12;
            this.btrandom.Text = "颜色随机";
            this.btrandom.UseVisualStyleBackColor = true;
            this.btrandom.Click += new System.EventHandler(this.btrandom_Click);
            // 
            // btcancel
            // 
            this.btcancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btcancel.Location = new System.Drawing.Point(515, 233);
            this.btcancel.Name = "btcancel";
            this.btcancel.Size = new System.Drawing.Size(63, 23);
            this.btcancel.TabIndex = 9;
            this.btcancel.Text = "取消";
            this.btcancel.UseVisualStyleBackColor = true;
            this.btcancel.Click += new System.EventHandler(this.btcancel_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtKD);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.cbXYAxis);
            this.groupBox4.Controls.Add(this.groupBox2);
            this.groupBox4.Controls.Add(this.cbLengend);
            this.groupBox4.Controls.Add(this.btPieOpen);
            this.groupBox4.Controls.Add(this.txtDataSource);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.txtPieTitle);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.txtPieSize);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Location = new System.Drawing.Point(6, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(592, 221);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "设置";
            // 
            // txtKD
            // 
            this.txtKD.Location = new System.Drawing.Point(295, 119);
            this.txtKD.Name = "txtKD";
            this.txtKD.Size = new System.Drawing.Size(101, 21);
            this.txtKD.TabIndex = 35;
            this.txtKD.Text = "20";
            this.txtKD.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(245, 122);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 34;
            this.label1.Text = "刻度：";
            this.label1.Visible = false;
            // 
            // cbXYAxis
            // 
            this.cbXYAxis.AutoSize = true;
            this.cbXYAxis.Checked = true;
            this.cbXYAxis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbXYAxis.Location = new System.Drawing.Point(249, 95);
            this.cbXYAxis.Name = "cbXYAxis";
            this.cbXYAxis.Size = new System.Drawing.Size(108, 16);
            this.cbXYAxis.TabIndex = 33;
            this.cbXYAxis.Text = "XY轴线是否显示";
            this.cbXYAxis.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lbGeo);
            this.groupBox2.Controls.Add(this.cmbGeo);
            this.groupBox2.Controls.Add(this.cbGeo);
            this.groupBox2.Location = new System.Drawing.Point(8, 143);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(504, 72);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "地理关联";
            // 
            // lbGeo
            // 
            this.lbGeo.AutoSize = true;
            this.lbGeo.Enabled = false;
            this.lbGeo.Location = new System.Drawing.Point(284, 32);
            this.lbGeo.Name = "lbGeo";
            this.lbGeo.Size = new System.Drawing.Size(65, 12);
            this.lbGeo.TabIndex = 2;
            this.lbGeo.Text = "关联图层：";
            // 
            // cmbGeo
            // 
            this.cmbGeo.Enabled = false;
            this.cmbGeo.FormattingEnabled = true;
            this.cmbGeo.Location = new System.Drawing.Point(353, 28);
            this.cmbGeo.Name = "cmbGeo";
            this.cmbGeo.Size = new System.Drawing.Size(142, 20);
            this.cmbGeo.TabIndex = 1;
            this.cmbGeo.SelectedIndexChanged += new System.EventHandler(this.cmbGeo_SelectedIndexChanged);
            // 
            // cbGeo
            // 
            this.cbGeo.AutoSize = true;
            this.cbGeo.Location = new System.Drawing.Point(81, 32);
            this.cbGeo.Name = "cbGeo";
            this.cbGeo.Size = new System.Drawing.Size(96, 16);
            this.cbGeo.TabIndex = 0;
            this.cbGeo.Text = "地理空间关联";
            this.cbGeo.UseVisualStyleBackColor = true;
            this.cbGeo.CheckedChanged += new System.EventHandler(this.cbGeo_CheckedChanged);
            // 
            // cbLengend
            // 
            this.cbLengend.AutoSize = true;
            this.cbLengend.Checked = true;
            this.cbLengend.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLengend.Location = new System.Drawing.Point(89, 95);
            this.cbLengend.Name = "cbLengend";
            this.cbLengend.Size = new System.Drawing.Size(96, 16);
            this.cbLengend.TabIndex = 18;
            this.cbLengend.Text = "图例是否显示";
            this.cbLengend.UseVisualStyleBackColor = true;
            // 
            // btPieOpen
            // 
            this.btPieOpen.Location = new System.Drawing.Point(527, 22);
            this.btPieOpen.Name = "btPieOpen";
            this.btPieOpen.Size = new System.Drawing.Size(45, 20);
            this.btPieOpen.TabIndex = 17;
            this.btPieOpen.Text = "浏览";
            this.btPieOpen.UseVisualStyleBackColor = true;
            this.btPieOpen.Click += new System.EventHandler(this.btPieOpen_Click);
            // 
            // txtDataSource
            // 
            this.txtDataSource.Location = new System.Drawing.Point(89, 23);
            this.txtDataSource.Name = "txtDataSource";
            this.txtDataSource.Size = new System.Drawing.Size(414, 21);
            this.txtDataSource.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "数据源设置：";
            // 
            // txtPieTitle
            // 
            this.txtPieTitle.Location = new System.Drawing.Point(89, 56);
            this.txtPieTitle.Name = "txtPieTitle";
            this.txtPieTitle.Size = new System.Drawing.Size(414, 21);
            this.txtPieTitle.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 59);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "统计图标题：";
            // 
            // txtPieSize
            // 
            this.txtPieSize.Location = new System.Drawing.Point(89, 119);
            this.txtPieSize.Name = "txtPieSize";
            this.txtPieSize.Size = new System.Drawing.Size(101, 21);
            this.txtPieSize.TabIndex = 3;
            this.txtPieSize.Text = "20";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "统计图尺寸：";
            // 
            // btok
            // 
            this.btok.Location = new System.Drawing.Point(427, 233);
            this.btok.Name = "btok";
            this.btok.Size = new System.Drawing.Size(53, 23);
            this.btok.TabIndex = 8;
            this.btok.Text = "确定";
            this.btok.UseVisualStyleBackColor = true;
            this.btok.Click += new System.EventHandler(this.btok_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(645, 268);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "三维圆饼图";
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(645, 268);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "三维环饼图";
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage4.Controls.Add(this.groupBox7);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(645, 268);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "折线图";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.button5);
            this.groupBox7.Controls.Add(this.textBox14);
            this.groupBox7.Controls.Add(this.label15);
            this.groupBox7.Controls.Add(this.textBox15);
            this.groupBox7.Controls.Add(this.label16);
            this.groupBox7.Controls.Add(this.textBox16);
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Location = new System.Drawing.Point(6, 6);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(589, 131);
            this.groupBox7.TabIndex = 14;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "设置";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(484, 27);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(45, 20);
            this.button5.TabIndex = 17;
            this.button5.Text = "浏览";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // textBox14
            // 
            this.textBox14.Location = new System.Drawing.Point(89, 28);
            this.textBox14.Name = "textBox14";
            this.textBox14.Size = new System.Drawing.Size(357, 21);
            this.textBox14.TabIndex = 9;
            this.textBox14.Text = "0.7";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 31);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(77, 12);
            this.label15.TabIndex = 8;
            this.label15.Text = "数据源设置：";
            // 
            // textBox15
            // 
            this.textBox15.Location = new System.Drawing.Point(89, 61);
            this.textBox15.Name = "textBox15";
            this.textBox15.Size = new System.Drawing.Size(357, 21);
            this.textBox15.TabIndex = 1;
            this.textBox15.Text = "0.7";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 64);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 12);
            this.label16.TabIndex = 0;
            this.label16.Text = "统计图标题：";
            // 
            // textBox16
            // 
            this.textBox16.Location = new System.Drawing.Point(89, 91);
            this.textBox16.Name = "textBox16";
            this.textBox16.Size = new System.Drawing.Size(101, 21);
            this.textBox16.TabIndex = 3;
            this.textBox16.Text = "20";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(6, 96);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(77, 12);
            this.label17.TabIndex = 2;
            this.label17.Text = "统计图尺寸：";
            // 
            // FrmBarChartsSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 468);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmBarChartsSet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "条形图设置";
            this.Load += new System.EventHandler(this.FrmChartsSet_Load);
            this.groupBox1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btcancel;
        private System.Windows.Forms.Button btok;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel ColorPan;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btPieOpen;
        private System.Windows.Forms.TextBox txtDataSource;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPieTitle;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPieSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox textBox14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox textBox15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox textBox16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button btrandom;
        private System.Windows.Forms.CheckBox cbLengend;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lbGeo;
        private System.Windows.Forms.ComboBox cmbGeo;
        private System.Windows.Forms.CheckBox cbGeo;
        private System.Windows.Forms.CheckBox cbXYAxis;
        private System.Windows.Forms.TextBox txtKD;
        private System.Windows.Forms.Label label1;

    }
}
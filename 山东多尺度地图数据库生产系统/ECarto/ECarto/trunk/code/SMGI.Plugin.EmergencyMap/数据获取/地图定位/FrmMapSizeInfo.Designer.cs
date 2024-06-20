namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmMapSizeInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMapSizeInfo));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_Preview = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbPaperWidth = new System.Windows.Forms.TextBox();
            this.tbPaperHeight = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tbInlineHeight = new System.Windows.Forms.TextBox();
            this.tbInlineWidth = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tbMapSizeWidth = new System.Windows.Forms.TextBox();
            this.tbMapSizeHeight = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.tbInOutInterval = new System.Windows.Forms.NumericUpDown();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tbOutlineHeight = new System.Windows.Forms.TextBox();
            this.tbOutlineWidth = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbMapTop = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbMapDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.tbMapHorial = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbInOutInterval)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbMapTop)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbMapDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbMapHorial)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btn_Preview);
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 371);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(342, 31);
            this.panel1.TabIndex = 5;
            // 
            // btn_Preview
            // 
            this.btn_Preview.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_Preview.Location = new System.Drawing.Point(137, 4);
            this.btn_Preview.Name = "btn_Preview";
            this.btn_Preview.Size = new System.Drawing.Size(64, 23);
            this.btn_Preview.TabIndex = 10;
            this.btn_Preview.Text = "应用";
            this.btn_Preview.UseVisualStyleBackColor = true;
            this.btn_Preview.Visible = false;
            this.btn_Preview.Click += new System.EventHandler(this.btn_Preview_Click);
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(201, 4);
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
            this.panel2.Location = new System.Drawing.Point(265, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(274, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbPaperWidth);
            this.groupBox2.Controls.Add(this.tbPaperHeight);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(317, 62);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "纸张尺寸";
            // 
            // tbPaperWidth
            // 
            this.tbPaperWidth.Enabled = false;
            this.tbPaperWidth.Location = new System.Drawing.Point(63, 20);
            this.tbPaperWidth.Name = "tbPaperWidth";
            this.tbPaperWidth.Size = new System.Drawing.Size(75, 21);
            this.tbPaperWidth.TabIndex = 2;
            this.tbPaperWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbWidth_KeyPress);
            // 
            // tbPaperHeight
            // 
            this.tbPaperHeight.Enabled = false;
            this.tbPaperHeight.Location = new System.Drawing.Point(210, 20);
            this.tbPaperHeight.Name = "tbPaperHeight";
            this.tbPaperHeight.Size = new System.Drawing.Size(85, 21);
            this.tbPaperHeight.TabIndex = 3;
            this.tbPaperHeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbHeight_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(149, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "高度(mm)：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "宽度(mm)：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(41, 350);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 12);
            this.label6.TabIndex = 12;
            this.label6.Text = "内外图廓间距(mm)：";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tbInlineHeight);
            this.groupBox3.Controls.Add(this.tbInlineWidth);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Location = new System.Drawing.Point(12, 281);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(317, 53);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "内图廓尺寸(mm)";
            // 
            // tbInlineHeight
            // 
            this.tbInlineHeight.Enabled = false;
            this.tbInlineHeight.Location = new System.Drawing.Point(210, 20);
            this.tbInlineHeight.Name = "tbInlineHeight";
            this.tbInlineHeight.Size = new System.Drawing.Size(85, 21);
            this.tbInlineHeight.TabIndex = 3;
            // 
            // tbInlineWidth
            // 
            this.tbInlineWidth.Enabled = false;
            this.tbInlineWidth.Location = new System.Drawing.Point(57, 20);
            this.tbInlineWidth.Name = "tbInlineWidth";
            this.tbInlineWidth.Size = new System.Drawing.Size(75, 21);
            this.tbInlineWidth.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(141, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 5;
            this.label7.Text = "高度(mm)：";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(2, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 4;
            this.label8.Text = "宽度(mm)：";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.tbMapSizeWidth);
            this.groupBox5.Controls.Add(this.tbMapSizeHeight);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Location = new System.Drawing.Point(12, 80);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(317, 55);
            this.groupBox5.TabIndex = 13;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "成图尺寸";
            // 
            // tbMapSizeWidth
            // 
            this.tbMapSizeWidth.Enabled = false;
            this.tbMapSizeWidth.Location = new System.Drawing.Point(63, 20);
            this.tbMapSizeWidth.Name = "tbMapSizeWidth";
            this.tbMapSizeWidth.Size = new System.Drawing.Size(75, 21);
            this.tbMapSizeWidth.TabIndex = 10;
            // 
            // tbMapSizeHeight
            // 
            this.tbMapSizeHeight.Enabled = false;
            this.tbMapSizeHeight.Location = new System.Drawing.Point(210, 20);
            this.tbMapSizeHeight.Name = "tbMapSizeHeight";
            this.tbMapSizeHeight.Size = new System.Drawing.Size(85, 21);
            this.tbMapSizeHeight.TabIndex = 11;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(148, 23);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 12);
            this.label10.TabIndex = 13;
            this.label10.Text = "高度(mm)：";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 23);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 12;
            this.label11.Text = "宽度(mm)：";
            // 
            // tbInOutInterval
            // 
            this.tbInOutInterval.DecimalPlaces = 1;
            this.tbInOutInterval.Location = new System.Drawing.Point(186, 345);
            this.tbInOutInterval.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.tbInOutInterval.Name = "tbInOutInterval";
            this.tbInOutInterval.Size = new System.Drawing.Size(120, 21);
            this.tbInOutInterval.TabIndex = 14;
            this.tbInOutInterval.ValueChanged += new System.EventHandler(this.tbInOutWidth_ValueChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tbOutlineHeight);
            this.groupBox4.Controls.Add(this.tbOutlineWidth);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Location = new System.Drawing.Point(12, 222);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(317, 53);
            this.groupBox4.TabIndex = 15;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "外图廓尺寸(mm)";
            // 
            // tbOutlineHeight
            // 
            this.tbOutlineHeight.Enabled = false;
            this.tbOutlineHeight.Location = new System.Drawing.Point(210, 20);
            this.tbOutlineHeight.Name = "tbOutlineHeight";
            this.tbOutlineHeight.Size = new System.Drawing.Size(85, 21);
            this.tbOutlineHeight.TabIndex = 3;
            // 
            // tbOutlineWidth
            // 
            this.tbOutlineWidth.Enabled = false;
            this.tbOutlineWidth.Location = new System.Drawing.Point(57, 20);
            this.tbOutlineWidth.Name = "tbOutlineWidth";
            this.tbOutlineWidth.Size = new System.Drawing.Size(75, 21);
            this.tbOutlineWidth.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(153, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "高度(mm)：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "宽度(mm)：";
            // 
            // tbMapTop
            // 
            this.tbMapTop.DecimalPlaces = 1;
            this.tbMapTop.Location = new System.Drawing.Point(62, 21);
            this.tbMapTop.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.tbMapTop.Name = "tbMapTop";
            this.tbMapTop.Size = new System.Drawing.Size(80, 21);
            this.tbMapTop.TabIndex = 17;
            this.tbMapTop.ValueChanged += new System.EventHandler(this.tbMapTop_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbMapDown);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.tbMapHorial);
            this.groupBox1.Controls.Add(this.tbMapTop);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Location = new System.Drawing.Point(12, 141);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(317, 75);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "成图与外图廓间距";
            // 
            // tbMapDown
            // 
            this.tbMapDown.DecimalPlaces = 1;
            this.tbMapDown.Location = new System.Drawing.Point(227, 21);
            this.tbMapDown.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.tbMapDown.Name = "tbMapDown";
            this.tbMapDown.Size = new System.Drawing.Size(80, 21);
            this.tbMapDown.TabIndex = 20;
            this.tbMapDown.ValueChanged += new System.EventHandler(this.tbMapDown_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(172, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 19;
            this.label5.Text = "下间距：";
            // 
            // tbMapHorial
            // 
            this.tbMapHorial.DecimalPlaces = 1;
            this.tbMapHorial.Location = new System.Drawing.Point(63, 48);
            this.tbMapHorial.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.tbMapHorial.Name = "tbMapHorial";
            this.tbMapHorial.Size = new System.Drawing.Size(80, 21);
            this.tbMapHorial.TabIndex = 18;
            this.tbMapHorial.ValueChanged += new System.EventHandler(this.tbMapHorial_ValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(-3, 51);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 12);
            this.label9.TabIndex = 13;
            this.label9.Text = "左右间距：";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 23);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(53, 12);
            this.label12.TabIndex = 12;
            this.label12.Text = "上间距：";
            // 
            // FrmMapSizeInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 402);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.tbInOutInterval);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMapSizeInfo";
            this.Text = "地图尺寸信息调整";
            this.Load += new System.EventHandler(this.MouseClickLocationForm_Load);
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbInOutInterval)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbMapTop)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbMapDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbMapHorial)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbPaperHeight;
        private System.Windows.Forms.TextBox tbPaperWidth;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox tbInlineHeight;
        private System.Windows.Forms.TextBox tbInlineWidth;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox tbMapSizeWidth;
        private System.Windows.Forms.TextBox tbMapSizeHeight;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btn_Preview;
        private System.Windows.Forms.NumericUpDown tbInOutInterval;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox tbOutlineHeight;
        private System.Windows.Forms.TextBox tbOutlineWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown tbMapTop;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown tbMapHorial;
        private System.Windows.Forms.NumericUpDown tbMapDown;
        private System.Windows.Forms.Label label5;
    }
}
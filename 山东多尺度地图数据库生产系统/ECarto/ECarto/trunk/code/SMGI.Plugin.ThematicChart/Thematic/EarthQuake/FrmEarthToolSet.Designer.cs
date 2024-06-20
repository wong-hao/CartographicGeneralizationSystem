namespace SMGI.Plugin.ThematicChart
{
    partial class FrmEarthToolSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEarthToolSet));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtRingCount = new System.Windows.Forms.TextBox();
            this.txtRingDis = new System.Windows.Forms.TextBox();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_waveColor = new System.Windows.Forms.Button();
            this.label26 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.Wave_FontSize = new System.Windows.Forms.NumericUpDown();
            this.label23 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtLineWidth = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtCenterSize = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_centerColor = new System.Windows.Forms.Button();
            this.label27 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.Center_FontSize = new System.Windows.Forms.NumericUpDown();
            this.label25 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.tb_depth = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.tb_level = new System.Windows.Forms.TextBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Wave_FontSize)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Center_FontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "地震波环数：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "地震波间隔：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 12);
            this.label3.TabIndex = 2;
            // 
            // txtRingCount
            // 
            this.txtRingCount.Location = new System.Drawing.Point(105, 17);
            this.txtRingCount.Name = "txtRingCount";
            this.txtRingCount.Size = new System.Drawing.Size(100, 21);
            this.txtRingCount.TabIndex = 3;
            this.txtRingCount.Text = "6";
            // 
            // txtRingDis
            // 
            this.txtRingDis.Location = new System.Drawing.Point(104, 46);
            this.txtRingDis.Name = "txtRingDis";
            this.txtRingDis.Size = new System.Drawing.Size(100, 21);
            this.txtRingDis.TabIndex = 4;
            this.txtRingDis.Text = "10";
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(244, 349);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(48, 34);
            this.btOK.TabIndex = 5;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(298, 349);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(50, 34);
            this.btCancel.TabIndex = 6;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_waveColor);
            this.groupBox1.Controls.Add(this.label26);
            this.groupBox1.Controls.Add(this.label22);
            this.groupBox1.Controls.Add(this.Wave_FontSize);
            this.groupBox1.Controls.Add(this.label23);
            this.groupBox1.Controls.Add(this.label20);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.txtLineWidth);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtCenterSize);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtRingCount);
            this.groupBox1.Controls.Add(this.txtRingDis);
            this.groupBox1.Location = new System.Drawing.Point(5, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(351, 194);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            // 
            // btn_waveColor
            // 
            this.btn_waveColor.BackColor = System.Drawing.Color.Red;
            this.btn_waveColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_waveColor.Location = new System.Drawing.Point(87, 166);
            this.btn_waveColor.Name = "btn_waveColor";
            this.btn_waveColor.Size = new System.Drawing.Size(45, 23);
            this.btn_waveColor.TabIndex = 16;
            this.btn_waveColor.UseVisualStyleBackColor = false;
            this.btn_waveColor.Click += new System.EventHandler(this.btn_waveColor_Click);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(21, 168);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(65, 12);
            this.label26.TabIndex = 15;
            this.label26.Text = "字体颜色：";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(137, 142);
            this.label22.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(17, 12);
            this.label22.TabIndex = 14;
            this.label22.Text = "mm";
            // 
            // Wave_FontSize
            // 
            this.Wave_FontSize.DecimalPlaces = 1;
            this.Wave_FontSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.Wave_FontSize.Location = new System.Drawing.Point(87, 140);
            this.Wave_FontSize.Margin = new System.Windows.Forms.Padding(2);
            this.Wave_FontSize.Name = "Wave_FontSize";
            this.Wave_FontSize.Size = new System.Drawing.Size(45, 21);
            this.Wave_FontSize.TabIndex = 13;
            this.Wave_FontSize.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(21, 144);
            this.label23.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(65, 12);
            this.label23.TabIndex = 12;
            this.label23.Text = "字体大小：";
            // 
            // label20
            // 
            this.label20.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label20.Location = new System.Drawing.Point(2, 130);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(347, 3);
            this.label20.TabIndex = 11;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(210, 53);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 10;
            this.label8.Text = "千米";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(210, 79);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "毫米（图上）";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(210, 110);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 9;
            this.label7.Text = "毫米（图上）";
            // 
            // txtLineWidth
            // 
            this.txtLineWidth.Location = new System.Drawing.Point(129, 107);
            this.txtLineWidth.Name = "txtLineWidth";
            this.txtLineWidth.Size = new System.Drawing.Size(75, 21);
            this.txtLineWidth.TabIndex = 8;
            this.txtLineWidth.Text = "1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 107);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "地震波线宽：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "地震中心点尺寸：";
            // 
            // txtCenterSize
            // 
            this.txtCenterSize.Location = new System.Drawing.Point(128, 76);
            this.txtCenterSize.Name = "txtCenterSize";
            this.txtCenterSize.Size = new System.Drawing.Size(76, 21);
            this.txtCenterSize.TabIndex = 6;
            this.txtCenterSize.Text = "5";
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.btn_centerColor);
            this.groupBox2.Controls.Add(this.label27);
            this.groupBox2.Controls.Add(this.label24);
            this.groupBox2.Controls.Add(this.label21);
            this.groupBox2.Controls.Add(this.Center_FontSize);
            this.groupBox2.Controls.Add(this.label25);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.tb_depth);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.tb_level);
            this.groupBox2.Location = new System.Drawing.Point(5, 202);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(356, 141);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            // 
            // btn_centerColor
            // 
            this.btn_centerColor.BackColor = System.Drawing.Color.Red;
            this.btn_centerColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_centerColor.Location = new System.Drawing.Point(85, 98);
            this.btn_centerColor.Name = "btn_centerColor";
            this.btn_centerColor.Size = new System.Drawing.Size(47, 23);
            this.btn_centerColor.TabIndex = 17;
            this.btn_centerColor.UseVisualStyleBackColor = false;
            this.btn_centerColor.Click += new System.EventHandler(this.btn_centerColor_Click);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(21, 103);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(65, 12);
            this.label27.TabIndex = 16;
            this.label27.Text = "字体颜色：";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(137, 74);
            this.label24.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(17, 12);
            this.label24.TabIndex = 17;
            this.label24.Text = "mm";
            // 
            // label21
            // 
            this.label21.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label21.Location = new System.Drawing.Point(3, 62);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(347, 3);
            this.label21.TabIndex = 12;
            // 
            // Center_FontSize
            // 
            this.Center_FontSize.DecimalPlaces = 1;
            this.Center_FontSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.Center_FontSize.Location = new System.Drawing.Point(87, 72);
            this.Center_FontSize.Margin = new System.Windows.Forms.Padding(2);
            this.Center_FontSize.Name = "Center_FontSize";
            this.Center_FontSize.Size = new System.Drawing.Size(45, 21);
            this.Center_FontSize.TabIndex = 16;
            this.Center_FontSize.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(21, 76);
            this.label25.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(65, 12);
            this.label25.TabIndex = 15;
            this.label25.Text = "字体大小：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(135, 41);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 12;
            this.label9.Text = "千米";
            // 
            // tb_depth
            // 
            this.tb_depth.Location = new System.Drawing.Point(86, 38);
            this.tb_depth.Name = "tb_depth";
            this.tb_depth.Size = new System.Drawing.Size(60, 21);
            this.tb_depth.TabIndex = 8;
            this.tb_depth.Text = "1";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(21, 43);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(65, 12);
            this.label12.TabIndex = 7;
            this.label12.Text = "地震深度：";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(21, 17);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(65, 12);
            this.label13.TabIndex = 5;
            this.label13.Text = "地震等级：";
            // 
            // tb_level
            // 
            this.tb_level.Location = new System.Drawing.Point(85, 14);
            this.tb_level.Name = "tb_level";
            this.tb_level.Size = new System.Drawing.Size(60, 21);
            this.tb_level.TabIndex = 6;
            this.tb_level.Text = "5";
            // 
            // FrmEarthToolSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 389);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.label3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmEarthToolSet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "地震专题设置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Wave_FontSize)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Center_FontSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRingCount;
        private System.Windows.Forms.TextBox txtRingDis;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtLineWidth;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCenterSize;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tb_depth;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tb_level;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.NumericUpDown Wave_FontSize;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.NumericUpDown Center_FontSize;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button btn_waveColor;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Button btn_centerColor;
        private System.Windows.Forms.Label label27;
    }
}
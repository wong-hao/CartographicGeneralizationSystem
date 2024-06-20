namespace SMGI.Plugin.EmergencyMap
{
    partial class FrmGridLine
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmGridLine));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radiaIndex = new System.Windows.Forms.RadioButton();
            this.radiagraducal = new System.Windows.Forms.RadioButton();
            this.radiameature = new System.Windows.Forms.RadioButton();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.txtMeatureY = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtMeatureX = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtColumn = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtRow = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtlatSe = new System.Windows.Forms.TextBox();
            this.txtlatMin = new System.Windows.Forms.TextBox();
            this.txtlatDe = new System.Windows.Forms.TextBox();
            this.txtlongSe = new System.Windows.Forms.TextBox();
            this.txtlongMin = new System.Windows.Forms.TextBox();
            this.txtlongDe = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtAnnoInterval = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtAnnoSize = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.btn_LastParas = new System.Windows.Forms.Button();
            this.NumInterval = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txtNumSize = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.cmbMapType = new System.Windows.Forms.ComboBox();
            this.tabCtrlSet = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabCtrlSet.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radiaIndex);
            this.groupBox1.Controls.Add(this.radiagraducal);
            this.groupBox1.Controls.Add(this.radiameature);
            this.groupBox1.Location = new System.Drawing.Point(5, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(603, 75);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "格网选择";
            // 
            // radiaIndex
            // 
            this.radiaIndex.AutoSize = true;
            this.radiaIndex.Checked = true;
            this.radiaIndex.Location = new System.Drawing.Point(423, 35);
            this.radiaIndex.Name = "radiaIndex";
            this.radiaIndex.Size = new System.Drawing.Size(59, 16);
            this.radiaIndex.TabIndex = 2;
            this.radiaIndex.TabStop = true;
            this.radiaIndex.Text = "索引网";
            this.radiaIndex.UseVisualStyleBackColor = true;
            this.radiaIndex.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // radiagraducal
            // 
            this.radiagraducal.AutoSize = true;
            this.radiagraducal.Location = new System.Drawing.Point(229, 35);
            this.radiagraducal.Name = "radiagraducal";
            this.radiagraducal.Size = new System.Drawing.Size(59, 16);
            this.radiagraducal.TabIndex = 1;
            this.radiagraducal.Text = "经纬网";
            this.radiagraducal.UseVisualStyleBackColor = true;
            this.radiagraducal.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // radiameature
            // 
            this.radiameature.AutoSize = true;
            this.radiameature.Location = new System.Drawing.Point(41, 35);
            this.radiameature.Name = "radiameature";
            this.radiameature.Size = new System.Drawing.Size(59, 16);
            this.radiameature.TabIndex = 0;
            this.radiameature.Text = "方里网";
            this.radiameature.UseVisualStyleBackColor = true;
            this.radiameature.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(522, 30);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(41, 12);
            this.label16.TabIndex = 26;
            this.label16.Text = "(<=10)";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(221, 36);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(41, 12);
            this.label15.TabIndex = 25;
            this.label15.Text = "(<=10)";
            // 
            // txtMeatureY
            // 
            this.txtMeatureY.Location = new System.Drawing.Point(464, 51);
            this.txtMeatureY.Name = "txtMeatureY";
            this.txtMeatureY.Size = new System.Drawing.Size(99, 21);
            this.txtMeatureY.TabIndex = 24;
            this.txtMeatureY.Text = "10";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(309, 54);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(155, 12);
            this.label11.TabIndex = 23;
            this.label11.Text = "方里网间距（Y轴：千米）：";
            // 
            // txtMeatureX
            // 
            this.txtMeatureX.Location = new System.Drawing.Point(166, 51);
            this.txtMeatureX.Name = "txtMeatureX";
            this.txtMeatureX.Size = new System.Drawing.Size(105, 21);
            this.txtMeatureX.TabIndex = 22;
            this.txtMeatureX.Text = "5";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(11, 54);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(155, 12);
            this.label12.TabIndex = 21;
            this.label12.Text = "方里网间距（X轴：千米）：";
            // 
            // txtColumn
            // 
            this.txtColumn.Location = new System.Drawing.Point(445, 29);
            this.txtColumn.Name = "txtColumn";
            this.txtColumn.Size = new System.Drawing.Size(73, 21);
            this.txtColumn.TabIndex = 20;
            this.txtColumn.Text = "10";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(312, 33);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(125, 12);
            this.label10.TabIndex = 19;
            this.label10.Text = "索引网间距（列数）：";
            // 
            // txtRow
            // 
            this.txtRow.Location = new System.Drawing.Point(145, 35);
            this.txtRow.Name = "txtRow";
            this.txtRow.Size = new System.Drawing.Size(72, 21);
            this.txtRow.TabIndex = 15;
            this.txtRow.Text = "7";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(25, 36);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(125, 12);
            this.label9.TabIndex = 14;
            this.label9.Text = "索引网间距（行数）：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(530, 39);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "秒″";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(483, 38);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 12;
            this.label7.Text = "分′";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(441, 38);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 11;
            this.label8.Text = "度°";
            // 
            // txtlatSe
            // 
            this.txtlatSe.Location = new System.Drawing.Point(527, 58);
            this.txtlatSe.Name = "txtlatSe";
            this.txtlatSe.Size = new System.Drawing.Size(32, 21);
            this.txtlatSe.TabIndex = 10;
            this.txtlatSe.Text = "0";
            // 
            // txtlatMin
            // 
            this.txtlatMin.Location = new System.Drawing.Point(484, 58);
            this.txtlatMin.Name = "txtlatMin";
            this.txtlatMin.Size = new System.Drawing.Size(32, 21);
            this.txtlatMin.TabIndex = 9;
            this.txtlatMin.Text = "5";
            // 
            // txtlatDe
            // 
            this.txtlatDe.Location = new System.Drawing.Point(442, 59);
            this.txtlatDe.Name = "txtlatDe";
            this.txtlatDe.Size = new System.Drawing.Size(32, 21);
            this.txtlatDe.TabIndex = 8;
            this.txtlatDe.Text = "0";
            // 
            // txtlongSe
            // 
            this.txtlongSe.Location = new System.Drawing.Point(238, 59);
            this.txtlongSe.Name = "txtlongSe";
            this.txtlongSe.Size = new System.Drawing.Size(32, 21);
            this.txtlongSe.TabIndex = 7;
            this.txtlongSe.Text = "0";
            // 
            // txtlongMin
            // 
            this.txtlongMin.Location = new System.Drawing.Point(195, 59);
            this.txtlongMin.Name = "txtlongMin";
            this.txtlongMin.Size = new System.Drawing.Size(32, 21);
            this.txtlongMin.TabIndex = 6;
            this.txtlongMin.Text = "4";
            // 
            // txtlongDe
            // 
            this.txtlongDe.Location = new System.Drawing.Point(153, 60);
            this.txtlongDe.Name = "txtlongDe";
            this.txtlongDe.Size = new System.Drawing.Size(32, 21);
            this.txtlongDe.TabIndex = 5;
            this.txtlongDe.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(245, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "秒″";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(198, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "分′";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(156, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "度°";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(305, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "经纬网纵向间距(纬度)：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "经纬网横向间距(经度)：";
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(490, 300);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(52, 34);
            this.btOK.TabIndex = 2;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(558, 299);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(50, 35);
            this.btCancel.TabIndex = 3;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtAnnoInterval);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.txtAnnoSize);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Location = new System.Drawing.Point(5, 93);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(603, 58);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "注记设置";
            // 
            // txtAnnoInterval
            // 
            this.txtAnnoInterval.Location = new System.Drawing.Point(492, 24);
            this.txtAnnoInterval.Name = "txtAnnoInterval";
            this.txtAnnoInterval.Size = new System.Drawing.Size(88, 21);
            this.txtAnnoInterval.TabIndex = 26;
            this.txtAnnoInterval.Text = "0.25";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(326, 27);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(155, 12);
            this.label14.TabIndex = 25;
            this.label14.Text = "注记与内图廓间距（毫米）:";
            // 
            // txtAnnoSize
            // 
            this.txtAnnoSize.Location = new System.Drawing.Point(174, 24);
            this.txtAnnoSize.Name = "txtAnnoSize";
            this.txtAnnoSize.Size = new System.Drawing.Size(114, 21);
            this.txtAnnoSize.TabIndex = 24;
            this.txtAnnoSize.Text = "3";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(52, 27);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(107, 12);
            this.label13.TabIndex = 23;
            this.label13.Text = "注记尺寸（毫米）:";
            // 
            // btn_LastParas
            // 
            this.btn_LastParas.Image = ((System.Drawing.Image)(resources.GetObject("btn_LastParas.Image")));
            this.btn_LastParas.Location = new System.Drawing.Point(5, 304);
            this.btn_LastParas.Name = "btn_LastParas";
            this.btn_LastParas.Size = new System.Drawing.Size(31, 30);
            this.btn_LastParas.TabIndex = 15;
            this.btn_LastParas.UseVisualStyleBackColor = true;
            this.btn_LastParas.Click += new System.EventHandler(this.btn_LastParas_Click);
            // 
            // NumInterval
            // 
            this.NumInterval.Location = new System.Drawing.Point(445, 70);
            this.NumInterval.Name = "NumInterval";
            this.NumInterval.Size = new System.Drawing.Size(73, 21);
            this.NumInterval.TabIndex = 26;
            this.NumInterval.Text = "0.25";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(240, 71);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(197, 12);
            this.label17.TabIndex = 25;
            this.label17.Text = "索引网序号与内图廓间距（毫米）：";
            // 
            // txtNumSize
            // 
            this.txtNumSize.Location = new System.Drawing.Point(145, 70);
            this.txtNumSize.Name = "txtNumSize";
            this.txtNumSize.Size = new System.Drawing.Size(72, 21);
            this.txtNumSize.TabIndex = 24;
            this.txtNumSize.Text = "3";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(1, 71);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(149, 12);
            this.label18.TabIndex = 23;
            this.label18.Text = "索引网序号尺寸（毫米）：";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(44, 313);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(65, 12);
            this.label21.TabIndex = 30;
            this.label21.Text = "地图类型：";
            // 
            // cmbMapType
            // 
            this.cmbMapType.FormattingEnabled = true;
            this.cmbMapType.Items.AddRange(new object[] {
            "通用",
            "省图",
            "市图",
            "县图"});
            this.cmbMapType.Location = new System.Drawing.Point(115, 310);
            this.cmbMapType.Name = "cmbMapType";
            this.cmbMapType.Size = new System.Drawing.Size(111, 20);
            this.cmbMapType.TabIndex = 29;
            this.cmbMapType.SelectedIndexChanged += new System.EventHandler(this.cmbMapType_SelectedIndexChanged);
            // 
            // tabCtrlSet
            // 
            this.tabCtrlSet.Controls.Add(this.tabPage1);
            this.tabCtrlSet.Controls.Add(this.tabPage2);
            this.tabCtrlSet.Controls.Add(this.tabPage3);
            this.tabCtrlSet.Location = new System.Drawing.Point(5, 129);
            this.tabCtrlSet.Margin = new System.Windows.Forms.Padding(1);
            this.tabCtrlSet.Name = "tabCtrlSet";
            this.tabCtrlSet.SelectedIndex = 0;
            this.tabCtrlSet.Size = new System.Drawing.Size(605, 154);
            this.tabCtrlSet.TabIndex = 31;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.txtMeatureY);
            this.tabPage1.Controls.Add(this.label11);
            this.tabPage1.Controls.Add(this.label12);
            this.tabPage1.Controls.Add(this.txtMeatureX);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(638, 128);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "方里网";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.txtlongDe);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.txtlongMin);
            this.tabPage2.Controls.Add(this.txtlatSe);
            this.tabPage2.Controls.Add(this.txtlongSe);
            this.tabPage2.Controls.Add(this.txtlatMin);
            this.tabPage2.Controls.Add(this.txtlatDe);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(638, 128);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "经纬网";
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.txtRow);
            this.tabPage3.Controls.Add(this.NumInterval);
            this.tabPage3.Controls.Add(this.label17);
            this.tabPage3.Controls.Add(this.label16);
            this.tabPage3.Controls.Add(this.txtNumSize);
            this.tabPage3.Controls.Add(this.label15);
            this.tabPage3.Controls.Add(this.label18);
            this.tabPage3.Controls.Add(this.label10);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Controls.Add(this.txtColumn);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(1);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(1);
            this.tabPage3.Size = new System.Drawing.Size(597, 128);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "索引网";
            // 
            // FrmGridLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 343);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.tabCtrlSet);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.cmbMapType);
            this.Controls.Add(this.btn_LastParas);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmGridLine";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "格网线生成";
            this.Load += new System.EventHandler(this.FrmGridLine_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabCtrlSet.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radiaIndex;
        private System.Windows.Forms.RadioButton radiagraducal;
        private System.Windows.Forms.RadioButton radiameature;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtlongDe;
        private System.Windows.Forms.TextBox txtlongSe;
        private System.Windows.Forms.TextBox txtlongMin;
        private System.Windows.Forms.TextBox txtlatSe;
        private System.Windows.Forms.TextBox txtlatMin;
        private System.Windows.Forms.TextBox txtlatDe;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtRow;
        private System.Windows.Forms.TextBox txtColumn;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox txtMeatureY;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtMeatureX;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtAnnoSize;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtAnnoInterval;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btn_LastParas;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox NumInterval;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtNumSize;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.ComboBox cmbMapType;
        private System.Windows.Forms.TabControl tabCtrlSet;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
    }
}
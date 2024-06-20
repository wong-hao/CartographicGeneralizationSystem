﻿namespace SMGI.Plugin.EmergencyMap
{
    partial class MouseClickLocationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MouseClickLocationForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_Preview = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbPaperWidth = new System.Windows.Forms.TextBox();
            this.tbPaperHeight = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbRefSclae = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbPaperSize = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbInOutWidth = new System.Windows.Forms.TextBox();
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
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btn_Preview);
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 326);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(327, 31);
            this.panel1.TabIndex = 5;
            // 
            // btn_Preview
            // 
            this.btn_Preview.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_Preview.Location = new System.Drawing.Point(122, 4);
            this.btn_Preview.Name = "btn_Preview";
            this.btn_Preview.Size = new System.Drawing.Size(64, 23);
            this.btn_Preview.TabIndex = 10;
            this.btn_Preview.Text = "预览";
            this.btn_Preview.UseVisualStyleBackColor = true;
            this.btn_Preview.Click += new System.EventHandler(this.btn_Preview_Click);
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(186, 4);
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
            this.panel2.Location = new System.Drawing.Point(250, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(259, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbPaperWidth);
            this.groupBox2.Controls.Add(this.tbPaperHeight);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cbRefSclae);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cbPaperSize);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(311, 127);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "页面设置";
            // 
            // tbPaperWidth
            // 
            this.tbPaperWidth.Location = new System.Drawing.Point(62, 96);
            this.tbPaperWidth.Name = "tbPaperWidth";
            this.tbPaperWidth.Size = new System.Drawing.Size(75, 21);
            this.tbPaperWidth.TabIndex = 2;
            this.tbPaperWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbWidth_KeyPress);
            // 
            // tbPaperHeight
            // 
            this.tbPaperHeight.Location = new System.Drawing.Point(206, 96);
            this.tbPaperHeight.Name = "tbPaperHeight";
            this.tbPaperHeight.Size = new System.Drawing.Size(85, 21);
            this.tbPaperHeight.TabIndex = 3;
            this.tbPaperHeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbHeight_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "比例尺：";
            // 
            // cbRefSclae
            // 
            this.cbRefSclae.FormattingEnabled = true;
            this.cbRefSclae.Location = new System.Drawing.Point(104, 60);
            this.cbRefSclae.Name = "cbRefSclae";
            this.cbRefSclae.Size = new System.Drawing.Size(186, 20);
            this.cbRefSclae.TabIndex = 8;
            this.cbRefSclae.SelectedIndexChanged += new System.EventHandler(this.cbRefSclae_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(148, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "高度(mm)：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "宽度(mm)：";
            // 
            // cbPaperSize
            // 
            this.cbPaperSize.FormattingEnabled = true;
            this.cbPaperSize.Location = new System.Drawing.Point(104, 24);
            this.cbPaperSize.Name = "cbPaperSize";
            this.cbPaperSize.Size = new System.Drawing.Size(186, 20);
            this.cbPaperSize.TabIndex = 1;
            this.cbPaperSize.SelectedIndexChanged += new System.EventHandler(this.cbPaperSize_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 27);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "纸张尺寸选择：";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbInOutWidth);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Location = new System.Drawing.Point(9, 197);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(311, 119);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "图廓线";
            // 
            // tbInOutWidth
            // 
            this.tbInOutWidth.Location = new System.Drawing.Point(148, 86);
            this.tbInOutWidth.Name = "tbInOutWidth";
            this.tbInOutWidth.Size = new System.Drawing.Size(112, 21);
            this.tbInOutWidth.TabIndex = 13;
            this.tbInOutWidth.Text = "17";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(29, 89);
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
            this.groupBox3.Location = new System.Drawing.Point(10, 25);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(295, 53);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "内图廓尺寸(mm)";
            // 
            // tbInlineHeight
            // 
            this.tbInlineHeight.Location = new System.Drawing.Point(198, 20);
            this.tbInlineHeight.Name = "tbInlineHeight";
            this.tbInlineHeight.Size = new System.Drawing.Size(85, 21);
            this.tbInlineHeight.TabIndex = 3;
            // 
            // tbInlineWidth
            // 
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
            this.groupBox5.Location = new System.Drawing.Point(9, 147);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(311, 43);
            this.groupBox5.TabIndex = 13;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "成图尺寸";
            // 
            // tbMapSizeWidth
            // 
            this.tbMapSizeWidth.Location = new System.Drawing.Point(63, 15);
            this.tbMapSizeWidth.Name = "tbMapSizeWidth";
            this.tbMapSizeWidth.Size = new System.Drawing.Size(75, 21);
            this.tbMapSizeWidth.TabIndex = 10;
            // 
            // tbMapSizeHeight
            // 
            this.tbMapSizeHeight.Location = new System.Drawing.Point(207, 15);
            this.tbMapSizeHeight.Name = "tbMapSizeHeight";
            this.tbMapSizeHeight.Size = new System.Drawing.Size(85, 21);
            this.tbMapSizeHeight.TabIndex = 11;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(148, 18);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 12);
            this.label10.TabIndex = 13;
            this.label10.Text = "高度(mm)：";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 18);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 12;
            this.label11.Text = "宽度(mm)：";
            // 
            // MouseClickLocationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 357);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MouseClickLocationForm";
            this.Text = "鼠标定位";
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbRefSclae;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbPaperHeight;
        private System.Windows.Forms.TextBox tbPaperWidth;
        private System.Windows.Forms.ComboBox cbPaperSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbInOutWidth;
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
    }
}
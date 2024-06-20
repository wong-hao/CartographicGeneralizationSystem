namespace SMGI.Plugin.EmergencyMap.LabelSym
{
    partial class FrmSymTool
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
            this.panelBackGround = new System.Windows.Forms.Panel();
            this.gbBackground = new System.Windows.Forms.GroupBox();
            this.cmbFillStlye = new System.Windows.Forms.ComboBox();
            this.label28 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.txtFillLineWidth = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.btFillLineColor = new System.Windows.Forms.Button();
            this.cmbFillLineStyle = new System.Windows.Forms.ComboBox();
            this.label20 = new System.Windows.Forms.Label();
            this.btColorFill = new System.Windows.Forms.Button();
            this.cmbFillType = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panelText = new System.Windows.Forms.Panel();
            this.gbText = new System.Windows.Forms.GroupBox();
            this.cmbLyr = new System.Windows.Forms.ComboBox();
            this.txtSql = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.cmbFields = new System.Windows.Forms.ComboBox();
            this.label30 = new System.Windows.Forms.Label();
            this.lbColor = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btfontColor = new System.Windows.Forms.Button();
            this.btFont = new System.Windows.Forms.Button();
            this.lbFont = new System.Windows.Forms.Label();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.btExpress = new System.Windows.Forms.Button();
            this.cbVbScript = new System.Windows.Forms.CheckBox();
            this.panelBackGround.SuspendLayout();
            this.gbBackground.SuspendLayout();
            this.panelText.SuspendLayout();
            this.gbText.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBackGround
            // 
            this.panelBackGround.Controls.Add(this.gbBackground);
            this.panelBackGround.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBackGround.Location = new System.Drawing.Point(0, 157);
            this.panelBackGround.Name = "panelBackGround";
            this.panelBackGround.Size = new System.Drawing.Size(425, 113);
            this.panelBackGround.TabIndex = 48;
            // 
            // gbBackground
            // 
            this.gbBackground.Controls.Add(this.cmbFillStlye);
            this.gbBackground.Controls.Add(this.label28);
            this.gbBackground.Controls.Add(this.label12);
            this.gbBackground.Controls.Add(this.label13);
            this.gbBackground.Controls.Add(this.txtFillLineWidth);
            this.gbBackground.Controls.Add(this.label14);
            this.gbBackground.Controls.Add(this.btFillLineColor);
            this.gbBackground.Controls.Add(this.cmbFillLineStyle);
            this.gbBackground.Controls.Add(this.label20);
            this.gbBackground.Controls.Add(this.btColorFill);
            this.gbBackground.Controls.Add(this.cmbFillType);
            this.gbBackground.Controls.Add(this.label6);
            this.gbBackground.Controls.Add(this.label5);
            this.gbBackground.Location = new System.Drawing.Point(15, 6);
            this.gbBackground.Name = "gbBackground";
            this.gbBackground.Size = new System.Drawing.Size(398, 102);
            this.gbBackground.TabIndex = 41;
            this.gbBackground.TabStop = false;
            this.gbBackground.Text = "背景框";
            // 
            // cmbFillStlye
            // 
            this.cmbFillStlye.FormattingEnabled = true;
            this.cmbFillStlye.Items.AddRange(new object[] {
            "实心",
            "空心"});
            this.cmbFillStlye.Location = new System.Drawing.Point(253, 40);
            this.cmbFillStlye.Name = "cmbFillStlye";
            this.cmbFillStlye.Size = new System.Drawing.Size(77, 20);
            this.cmbFillStlye.TabIndex = 54;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(192, 43);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(53, 12);
            this.label28.TabIndex = 53;
            this.label28.Text = "填充类型";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(336, 71);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(17, 12);
            this.label12.TabIndex = 52;
            this.label12.Text = "mm";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(193, 71);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(53, 12);
            this.label13.TabIndex = 51;
            this.label13.Text = "边线宽度";
            // 
            // txtFillLineWidth
            // 
            this.txtFillLineWidth.Location = new System.Drawing.Point(253, 68);
            this.txtFillLineWidth.Name = "txtFillLineWidth";
            this.txtFillLineWidth.Size = new System.Drawing.Size(77, 21);
            this.txtFillLineWidth.TabIndex = 50;
            this.txtFillLineWidth.Text = "0.1";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(18, 71);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(53, 12);
            this.label14.TabIndex = 48;
            this.label14.Text = "边线颜色";
            // 
            // btFillLineColor
            // 
            this.btFillLineColor.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.btFillLineColor.Location = new System.Drawing.Point(77, 66);
            this.btFillLineColor.Name = "btFillLineColor";
            this.btFillLineColor.Size = new System.Drawing.Size(76, 23);
            this.btFillLineColor.TabIndex = 49;
            this.btFillLineColor.Tag = "false";
            this.btFillLineColor.UseVisualStyleBackColor = false;
            this.btFillLineColor.Click += new System.EventHandler(this.btColor_Click);
            // 
            // cmbFillLineStyle
            // 
            this.cmbFillLineStyle.FormattingEnabled = true;
            this.cmbFillLineStyle.Location = new System.Drawing.Point(77, 40);
            this.cmbFillLineStyle.Name = "cmbFillLineStyle";
            this.cmbFillLineStyle.Size = new System.Drawing.Size(76, 20);
            this.cmbFillLineStyle.TabIndex = 47;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(18, 43);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(53, 12);
            this.label20.TabIndex = 46;
            this.label20.Text = "边线样式";
            // 
            // btColorFill
            // 
            this.btColorFill.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.btColorFill.Location = new System.Drawing.Point(253, 11);
            this.btColorFill.Name = "btColorFill";
            this.btColorFill.Size = new System.Drawing.Size(77, 23);
            this.btColorFill.TabIndex = 24;
            this.btColorFill.Tag = "false";
            this.btColorFill.UseVisualStyleBackColor = false;
            this.btColorFill.Click += new System.EventHandler(this.btColor_Click);
            // 
            // cmbFillType
            // 
            this.cmbFillType.FormattingEnabled = true;
            this.cmbFillType.Location = new System.Drawing.Point(77, 14);
            this.cmbFillType.Name = "cmbFillType";
            this.cmbFillType.Size = new System.Drawing.Size(76, 20);
            this.cmbFillType.TabIndex = 26;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 17);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 25;
            this.label6.Text = "内容框类型";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(193, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 23;
            this.label5.Text = "填充颜色";
            // 
            // panelText
            // 
            this.panelText.Controls.Add(this.gbText);
            this.panelText.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelText.Location = new System.Drawing.Point(0, 0);
            this.panelText.Name = "panelText";
            this.panelText.Size = new System.Drawing.Size(425, 157);
            this.panelText.TabIndex = 47;
            // 
            // gbText
            // 
            this.gbText.Controls.Add(this.cbVbScript);
            this.gbText.Controls.Add(this.btExpress);
            this.gbText.Controls.Add(this.cmbLyr);
            this.gbText.Controls.Add(this.txtSql);
            this.gbText.Controls.Add(this.label1);
            this.gbText.Controls.Add(this.button2);
            this.gbText.Controls.Add(this.cmbFields);
            this.gbText.Controls.Add(this.label30);
            this.gbText.Controls.Add(this.lbColor);
            this.gbText.Controls.Add(this.label9);
            this.gbText.Controls.Add(this.btfontColor);
            this.gbText.Controls.Add(this.btFont);
            this.gbText.Controls.Add(this.lbFont);
            this.gbText.Location = new System.Drawing.Point(15, 5);
            this.gbText.Name = "gbText";
            this.gbText.Size = new System.Drawing.Size(398, 146);
            this.gbText.TabIndex = 33;
            this.gbText.TabStop = false;
            this.gbText.Text = "文本设置";
            // 
            // cmbLyr
            // 
            this.cmbLyr.FormattingEnabled = true;
            this.cmbLyr.Location = new System.Drawing.Point(77, 24);
            this.cmbLyr.Name = "cmbLyr";
            this.cmbLyr.Size = new System.Drawing.Size(76, 20);
            this.cmbLyr.TabIndex = 58;
            this.cmbLyr.SelectedIndexChanged += new System.EventHandler(this.cmbLyr_SelectedIndexChanged);
            // 
            // txtSql
            // 
            this.txtSql.Location = new System.Drawing.Point(77, 88);
            this.txtSql.Name = "txtSql";
            this.txtSql.Size = new System.Drawing.Size(250, 21);
            this.txtSql.TabIndex = 57;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 56;
            this.label1.Text = "过滤条件";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(333, 85);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(65, 25);
            this.button2.TabIndex = 55;
            this.button2.Text = "SQL语句";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // cmbFields
            // 
            this.cmbFields.FormattingEnabled = true;
            this.cmbFields.Location = new System.Drawing.Point(250, 23);
            this.cmbFields.Name = "cmbFields";
            this.cmbFields.Size = new System.Drawing.Size(77, 20);
            this.cmbFields.TabIndex = 54;
            this.cmbFields.SelectedIndexChanged += new System.EventHandler(this.cmbFields_SelectedIndexChanged);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(192, 26);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(53, 12);
            this.label30.TabIndex = 53;
            this.label30.Text = "字段名称";
            // 
            // lbColor
            // 
            this.lbColor.AutoSize = true;
            this.lbColor.Location = new System.Drawing.Point(276, 57);
            this.lbColor.Name = "lbColor";
            this.lbColor.Size = new System.Drawing.Size(53, 12);
            this.lbColor.TabIndex = 35;
            this.lbColor.Text = "字体颜色";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(18, 27);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 51;
            this.label9.Text = "图层名称";
            // 
            // btfontColor
            // 
            this.btfontColor.Location = new System.Drawing.Point(213, 51);
            this.btfontColor.Name = "btfontColor";
            this.btfontColor.Size = new System.Drawing.Size(51, 25);
            this.btfontColor.TabIndex = 34;
            this.btfontColor.Text = "颜色";
            this.btfontColor.UseVisualStyleBackColor = true;
            this.btfontColor.Click += new System.EventHandler(this.btfontColor_Click);
            // 
            // btFont
            // 
            this.btFont.Location = new System.Drawing.Point(77, 51);
            this.btFont.Name = "btFont";
            this.btFont.Size = new System.Drawing.Size(76, 25);
            this.btFont.TabIndex = 33;
            this.btFont.Text = "选择字体";
            this.btFont.UseVisualStyleBackColor = true;
            this.btFont.Click += new System.EventHandler(this.btFont_Click);
            // 
            // lbFont
            // 
            this.lbFont.AutoSize = true;
            this.lbFont.Location = new System.Drawing.Point(161, 57);
            this.lbFont.Name = "lbFont";
            this.lbFont.Size = new System.Drawing.Size(29, 12);
            this.lbFont.TabIndex = 33;
            this.lbFont.Text = "字体";
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(294, 282);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(53, 33);
            this.btOK.TabIndex = 49;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(353, 282);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(53, 33);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btExpress
            // 
            this.btExpress.Location = new System.Drawing.Point(163, 120);
            this.btExpress.Name = "btExpress";
            this.btExpress.Size = new System.Drawing.Size(53, 25);
            this.btExpress.TabIndex = 59;
            this.btExpress.Text = "表达式";
            this.btExpress.UseVisualStyleBackColor = true;
            this.btExpress.Click += new System.EventHandler(this.btExpress_Click);
            // 
            // cbVbScript
            // 
            this.cbVbScript.AutoSize = true;
            this.cbVbScript.Location = new System.Drawing.Point(77, 125);
            this.cbVbScript.Name = "cbVbScript";
            this.cbVbScript.Size = new System.Drawing.Size(84, 16);
            this.cbVbScript.TabIndex = 60;
            this.cbVbScript.Text = "注记表达式";
            this.cbVbScript.UseVisualStyleBackColor = true;
            this.cbVbScript.CheckedChanged += new System.EventHandler(this.cbVbScript_CheckedChanged);
            // 
            // FrmSymTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(425, 320);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.panelBackGround);
            this.Controls.Add(this.panelText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmSymTool";
            this.Text = "符号标注";
            this.Load += new System.EventHandler(this.FrmSymTool_Load);
            this.panelBackGround.ResumeLayout(false);
            this.gbBackground.ResumeLayout(false);
            this.gbBackground.PerformLayout();
            this.panelText.ResumeLayout(false);
            this.gbText.ResumeLayout(false);
            this.gbText.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBackGround;
        private System.Windows.Forms.GroupBox gbBackground;
        private System.Windows.Forms.ComboBox cmbFillStlye;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtFillLineWidth;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btFillLineColor;
        private System.Windows.Forms.ComboBox cmbFillLineStyle;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button btColorFill;
        private System.Windows.Forms.ComboBox cmbFillType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelText;
        private System.Windows.Forms.GroupBox gbText;
        private System.Windows.Forms.ComboBox cmbFields;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label lbColor;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btfontColor;
        private System.Windows.Forms.Button btFont;
        private System.Windows.Forms.Label lbFont;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TextBox txtSql;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox cmbLyr;
        private System.Windows.Forms.Button btExpress;
        private System.Windows.Forms.CheckBox cbVbScript;
    }
}
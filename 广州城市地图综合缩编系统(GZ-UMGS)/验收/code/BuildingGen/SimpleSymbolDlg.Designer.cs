namespace BuildingGen
{
    partial class SimpleSymbolDlg
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
            this.LineGroup = new System.Windows.Forms.GroupBox();
            this.LineWidth = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.useLineColor = new System.Windows.Forms.CheckBox();
            this.LineColor = new System.Windows.Forms.Panel();
            this.FillGroup = new System.Windows.Forms.GroupBox();
            this.UseFillColor = new System.Windows.Forms.CheckBox();
            this.FillColor = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.MarkerStyleGroup = new System.Windows.Forms.GroupBox();
            this.MarkerStyle = new System.Windows.Forms.ComboBox();
            this.LineGroup.SuspendLayout();
            this.FillGroup.SuspendLayout();
            this.MarkerStyleGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // LineGroup
            // 
            this.LineGroup.Controls.Add(this.LineWidth);
            this.LineGroup.Controls.Add(this.label1);
            this.LineGroup.Controls.Add(this.useLineColor);
            this.LineGroup.Controls.Add(this.LineColor);
            this.LineGroup.Location = new System.Drawing.Point(12, 1);
            this.LineGroup.Name = "LineGroup";
            this.LineGroup.Size = new System.Drawing.Size(116, 100);
            this.LineGroup.TabIndex = 0;
            this.LineGroup.TabStop = false;
            this.LineGroup.Text = "边框";
            // 
            // LineWidth
            // 
            this.LineWidth.FormattingEnabled = true;
            this.LineWidth.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.LineWidth.Location = new System.Drawing.Point(43, 59);
            this.LineWidth.Name = "LineWidth";
            this.LineWidth.Size = new System.Drawing.Size(67, 20);
            this.LineWidth.TabIndex = 4;
            this.LineWidth.SelectedIndexChanged += new System.EventHandler(this.LineWidth_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "宽度";
            // 
            // useLineColor
            // 
            this.useLineColor.AutoSize = true;
            this.useLineColor.Location = new System.Drawing.Point(6, 20);
            this.useLineColor.Name = "useLineColor";
            this.useLineColor.Size = new System.Drawing.Size(48, 16);
            this.useLineColor.TabIndex = 1;
            this.useLineColor.Text = "透明";
            this.useLineColor.UseVisualStyleBackColor = true;
            this.useLineColor.CheckedChanged += new System.EventHandler(this.useLineColor_CheckedChanged);
            // 
            // LineColor
            // 
            this.LineColor.BackColor = System.Drawing.SystemColors.Control;
            this.LineColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LineColor.Location = new System.Drawing.Point(60, 13);
            this.LineColor.Name = "LineColor";
            this.LineColor.Size = new System.Drawing.Size(48, 31);
            this.LineColor.TabIndex = 0;
            this.LineColor.Paint += new System.Windows.Forms.PaintEventHandler(this.LineColor_Paint);
            this.LineColor.Click += new System.EventHandler(this.LineColor_Click);
            // 
            // FillGroup
            // 
            this.FillGroup.Controls.Add(this.UseFillColor);
            this.FillGroup.Controls.Add(this.FillColor);
            this.FillGroup.Location = new System.Drawing.Point(134, 1);
            this.FillGroup.Name = "FillGroup";
            this.FillGroup.Size = new System.Drawing.Size(121, 52);
            this.FillGroup.TabIndex = 1;
            this.FillGroup.TabStop = false;
            this.FillGroup.Text = "填充";
            // 
            // UseFillColor
            // 
            this.UseFillColor.AutoSize = true;
            this.UseFillColor.Location = new System.Drawing.Point(7, 20);
            this.UseFillColor.Name = "UseFillColor";
            this.UseFillColor.Size = new System.Drawing.Size(48, 16);
            this.UseFillColor.TabIndex = 3;
            this.UseFillColor.Text = "透明";
            this.UseFillColor.UseVisualStyleBackColor = true;
            this.UseFillColor.CheckedChanged += new System.EventHandler(this.UseFillColor_CheckedChanged);
            // 
            // FillColor
            // 
            this.FillColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.FillColor.Location = new System.Drawing.Point(61, 13);
            this.FillColor.Name = "FillColor";
            this.FillColor.Size = new System.Drawing.Size(48, 31);
            this.FillColor.TabIndex = 2;
            this.FillColor.Paint += new System.Windows.Forms.PaintEventHandler(this.FillColor_Paint);
            this.FillColor.Click += new System.EventHandler(this.FillColor_Click);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(33, 114);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(152, 114);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // MarkerStyleGroup
            // 
            this.MarkerStyleGroup.Controls.Add(this.MarkerStyle);
            this.MarkerStyleGroup.Location = new System.Drawing.Point(135, 59);
            this.MarkerStyleGroup.Name = "MarkerStyleGroup";
            this.MarkerStyleGroup.Size = new System.Drawing.Size(120, 42);
            this.MarkerStyleGroup.TabIndex = 4;
            this.MarkerStyleGroup.TabStop = false;
            this.MarkerStyleGroup.Text = "形状";
            // 
            // MarkerStyle
            // 
            this.MarkerStyle.FormattingEnabled = true;
            this.MarkerStyle.Items.AddRange(new object[] {
            "圆形",
            "正方形",
            "十字",
            "X形",
            "菱形"});
            this.MarkerStyle.Location = new System.Drawing.Point(6, 16);
            this.MarkerStyle.Name = "MarkerStyle";
            this.MarkerStyle.Size = new System.Drawing.Size(102, 20);
            this.MarkerStyle.TabIndex = 5;
            this.MarkerStyle.SelectedIndexChanged += new System.EventHandler(this.MarkerStyle_SelectedIndexChanged);
            // 
            // SimpleSymbolDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(267, 149);
            this.ControlBox = false;
            this.Controls.Add(this.MarkerStyleGroup);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.FillGroup);
            this.Controls.Add(this.LineGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SimpleSymbolDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "符号配置";
            this.Load += new System.EventHandler(this.SimpleSymbolDlg_Load);
            this.LineGroup.ResumeLayout(false);
            this.LineGroup.PerformLayout();
            this.FillGroup.ResumeLayout(false);
            this.FillGroup.PerformLayout();
            this.MarkerStyleGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox LineGroup;
        private System.Windows.Forms.GroupBox FillGroup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox useLineColor;
        private System.Windows.Forms.Panel LineColor;
        private System.Windows.Forms.ComboBox LineWidth;
        private System.Windows.Forms.CheckBox UseFillColor;
        private System.Windows.Forms.Panel FillColor;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox MarkerStyleGroup;
        private System.Windows.Forms.ComboBox MarkerStyle;
    }
}
namespace SMGI.Plugin.EmergencyMap
{
    partial class AnnotationFontSizeAdjustForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.lbMainRESALevel = new System.Windows.Forms.Label();
            this.cbAnnotationLayer = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbAnnotationType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nupFontSizeScale = new System.Windows.Forms.NumericUpDown();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nupFontSizeScale)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btOK);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 122);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(284, 31);
            this.panel1.TabIndex = 7;
            // 
            // btOK
            // 
            this.btOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.btOK.Location = new System.Drawing.Point(143, 4);
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
            this.panel2.Location = new System.Drawing.Point(207, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(9, 23);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btCancel.Location = new System.Drawing.Point(216, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // lbMainRESALevel
            // 
            this.lbMainRESALevel.AutoSize = true;
            this.lbMainRESALevel.Location = new System.Drawing.Point(12, 9);
            this.lbMainRESALevel.Name = "lbMainRESALevel";
            this.lbMainRESALevel.Size = new System.Drawing.Size(65, 12);
            this.lbMainRESALevel.TabIndex = 8;
            this.lbMainRESALevel.Text = "注记图层：";
            // 
            // cbAnnotationLayer
            // 
            this.cbAnnotationLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAnnotationLayer.FormattingEnabled = true;
            this.cbAnnotationLayer.Location = new System.Drawing.Point(83, 6);
            this.cbAnnotationLayer.Name = "cbAnnotationLayer";
            this.cbAnnotationLayer.Size = new System.Drawing.Size(189, 20);
            this.cbAnnotationLayer.TabIndex = 9;
            this.cbAnnotationLayer.SelectedIndexChanged += new System.EventHandler(this.cbAnnotationLayer_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "注记类型：";
            // 
            // cbAnnotationType
            // 
            this.cbAnnotationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAnnotationType.FormattingEnabled = true;
            this.cbAnnotationType.Location = new System.Drawing.Point(83, 43);
            this.cbAnnotationType.Name = "cbAnnotationType";
            this.cbAnnotationType.Size = new System.Drawing.Size(189, 20);
            this.cbAnnotationType.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "缩放比例：";
            // 
            // nupFontSizeScale
            // 
            this.nupFontSizeScale.DecimalPlaces = 2;
            this.nupFontSizeScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nupFontSizeScale.Location = new System.Drawing.Point(83, 81);
            this.nupFontSizeScale.Margin = new System.Windows.Forms.Padding(2);
            this.nupFontSizeScale.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nupFontSizeScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nupFontSizeScale.Name = "nupFontSizeScale";
            this.nupFontSizeScale.Size = new System.Drawing.Size(63, 21);
            this.nupFontSizeScale.TabIndex = 10;
            this.nupFontSizeScale.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            // 
            // AnnotationFontSizeAdjustForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 153);
            this.Controls.Add(this.nupFontSizeScale);
            this.Controls.Add(this.cbAnnotationType);
            this.Controls.Add(this.cbAnnotationLayer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbMainRESALevel);
            this.Controls.Add(this.panel1);
            this.Name = "AnnotationFontSizeAdjustForm";
            this.Text = "注记字大缩放调整";
            this.Load += new System.EventHandler(this.AnnotationFontSizeAdjustForm_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nupFontSizeScale)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label lbMainRESALevel;
        private System.Windows.Forms.ComboBox cbAnnotationLayer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbAnnotationType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nupFontSizeScale;
    }
}
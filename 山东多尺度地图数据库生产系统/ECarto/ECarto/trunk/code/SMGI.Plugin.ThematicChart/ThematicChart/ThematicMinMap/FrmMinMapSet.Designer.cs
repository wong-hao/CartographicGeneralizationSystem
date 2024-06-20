namespace SMGI.Plugin.ThematicChart.ThematicChart.ThematicMinMap
{
    partial class FrmMinMapSet
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMinFullName = new System.Windows.Forms.TextBox();
            this.cbcbAimLayer = new System.Windows.Forms.ComboBox();
            this.btAdd = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPageNum = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btOK = new System.Windows.Forms.Button();
            this.btCen = new System.Windows.Forms.Button();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.txtMinFullName);
            this.groupBox3.Controls.Add(this.cbcbAimLayer);
            this.groupBox3.Controls.Add(this.btAdd);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.txtPageNum);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.txtTitle);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(517, 137);
            this.groupBox3.TabIndex = 92;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "基本信息";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 103;
            this.label2.Text = "专题小图";
            // 
            // txtMinFullName
            // 
            this.txtMinFullName.Location = new System.Drawing.Point(91, 98);
            this.txtMinFullName.Name = "txtMinFullName";
            this.txtMinFullName.ReadOnly = true;
            this.txtMinFullName.Size = new System.Drawing.Size(356, 21);
            this.txtMinFullName.TabIndex = 98;
            // 
            // cbcbAimLayer
            // 
            this.cbcbAimLayer.FormattingEnabled = true;
            this.cbcbAimLayer.Location = new System.Drawing.Point(91, 64);
            this.cbcbAimLayer.Name = "cbcbAimLayer";
            this.cbcbAimLayer.Size = new System.Drawing.Size(133, 20);
            this.cbcbAimLayer.TabIndex = 102;
            this.cbcbAimLayer.SelectedIndexChanged += new System.EventHandler(this.cbcbAimLayer_SelectedIndexChanged);
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(453, 98);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(56, 25);
            this.btAdd.TabIndex = 93;
            this.btAdd.Text = "浏览";
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 101;
            this.label4.Text = "目标图层：";
            // 
            // txtPageNum
            // 
            this.txtPageNum.Enabled = false;
            this.txtPageNum.Location = new System.Drawing.Point(324, 24);
            this.txtPageNum.Name = "txtPageNum";
            this.txtPageNum.Size = new System.Drawing.Size(175, 21);
            this.txtPageNum.TabIndex = 5;
            this.txtPageNum.Tag = "当前标题";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(252, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 4;
            this.label1.Tag = " ";
            this.label1.Text = "当前页码：";
            // 
            // txtTitle
            // 
            this.txtTitle.Enabled = false;
            this.txtTitle.Location = new System.Drawing.Point(91, 24);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(133, 21);
            this.txtTitle.TabIndex = 3;
            this.txtTitle.Tag = "当前标题";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 2;
            this.label3.Tag = " ";
            this.label3.Text = "当前页码标题";
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(361, 155);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(81, 30);
            this.btOK.TabIndex = 95;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCen
            // 
            this.btCen.Location = new System.Drawing.Point(448, 155);
            this.btCen.Name = "btCen";
            this.btCen.Size = new System.Drawing.Size(81, 30);
            this.btCen.TabIndex = 96;
            this.btCen.Text = "取消";
            this.btCen.UseVisualStyleBackColor = true;
            this.btCen.Click += new System.EventHandler(this.btCen_Click);
            // 
            // FrmMinMapSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 193);
            this.Controls.Add(this.btCen);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.groupBox3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMinMapSet";
            this.Text = "附图设计";
            this.Load += new System.EventHandler(this.FrmMinMapSet_Load);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtPageNum;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCen;
        private System.Windows.Forms.ComboBox cbcbAimLayer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtMinFullName;
        private System.Windows.Forms.Label label2;
    }
}
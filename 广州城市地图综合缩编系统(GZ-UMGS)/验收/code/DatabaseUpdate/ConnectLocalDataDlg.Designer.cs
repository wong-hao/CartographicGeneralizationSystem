namespace DatabaseUpdate
{
    partial class ConnectLocalDataDlg
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
          this.groupBox1 = new System.Windows.Forms.GroupBox();
          this.btPath = new System.Windows.Forms.Button();
          this.tbPath = new System.Windows.Forms.TextBox();
          this.groupBox2 = new System.Windows.Forms.GroupBox();
          this.clScale = new System.Windows.Forms.CheckedListBox();
          this.groupBox3 = new System.Windows.Forms.GroupBox();
          this.clFeature = new System.Windows.Forms.CheckedListBox();
          this.label1 = new System.Windows.Forms.Label();
          this.label2 = new System.Windows.Forms.Label();
          this.btOK = new System.Windows.Forms.Button();
          this.btCancel = new System.Windows.Forms.Button();
          this.groupBox1.SuspendLayout();
          this.groupBox2.SuspendLayout();
          this.groupBox3.SuspendLayout();
          this.SuspendLayout();
          // 
          // groupBox1
          // 
          this.groupBox1.Controls.Add(this.btPath);
          this.groupBox1.Controls.Add(this.tbPath);
          this.groupBox1.Location = new System.Drawing.Point(9, 10);
          this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
          this.groupBox1.Name = "groupBox1";
          this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
          this.groupBox1.Size = new System.Drawing.Size(420, 54);
          this.groupBox1.TabIndex = 0;
          this.groupBox1.TabStop = false;
          this.groupBox1.Text = "被更新数据文件路径";
          // 
          // btPath
          // 
          this.btPath.Location = new System.Drawing.Point(338, 18);
          this.btPath.Name = "btPath";
          this.btPath.Size = new System.Drawing.Size(75, 23);
          this.btPath.TabIndex = 3;
          this.btPath.Text = "浏览";
          this.btPath.UseVisualStyleBackColor = true;
          this.btPath.Click += new System.EventHandler(this.btPath_Click);
          // 
          // tbPath
          // 
          this.tbPath.Location = new System.Drawing.Point(9, 20);
          this.tbPath.Name = "tbPath";
          this.tbPath.ReadOnly = true;
          this.tbPath.Size = new System.Drawing.Size(324, 21);
          this.tbPath.TabIndex = 2;
          // 
          // groupBox2
          // 
          this.groupBox2.Controls.Add(this.clScale);
          this.groupBox2.Location = new System.Drawing.Point(9, 68);
          this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
          this.groupBox2.Name = "groupBox2";
          this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
          this.groupBox2.Size = new System.Drawing.Size(204, 127);
          this.groupBox2.TabIndex = 1;
          this.groupBox2.TabStop = false;
          this.groupBox2.Text = "包含比例尺";
          // 
          // clScale
          // 
          this.clScale.CheckOnClick = true;
          this.clScale.FormattingEnabled = true;
          this.clScale.Items.AddRange(new object[] {
            "500",
            "2000",
            "10000",
            "25000",
            "50000",
            "100000"});
          this.clScale.Location = new System.Drawing.Point(9, 19);
          this.clScale.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
          this.clScale.Name = "clScale";
          this.clScale.Size = new System.Drawing.Size(192, 100);
          this.clScale.TabIndex = 0;
          // 
          // groupBox3
          // 
          this.groupBox3.Controls.Add(this.clFeature);
          this.groupBox3.Location = new System.Drawing.Point(218, 68);
          this.groupBox3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
          this.groupBox3.Name = "groupBox3";
          this.groupBox3.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
          this.groupBox3.Size = new System.Drawing.Size(212, 127);
          this.groupBox3.TabIndex = 2;
          this.groupBox3.TabStop = false;
          this.groupBox3.Text = "包含要素类";
          // 
          // clFeature
          // 
          this.clFeature.CheckOnClick = true;
          this.clFeature.FormattingEnabled = true;
          this.clFeature.Items.AddRange(new object[] {
            "植被面",
            "水系面",
            "房屋面",
            "道路面",
            "道路中心线"});
          this.clFeature.Location = new System.Drawing.Point(9, 19);
          this.clFeature.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
          this.clFeature.Name = "clFeature";
          this.clFeature.Size = new System.Drawing.Size(197, 100);
          this.clFeature.TabIndex = 0;
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.ForeColor = System.Drawing.Color.Red;
          this.label1.Location = new System.Drawing.Point(10, 201);
          this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(443, 12);
          this.label1.TabIndex = 3;
          this.label1.Text = "说明：选中比例尺和要素类后必须在数据库中存在以[要素类+比例尺]为名的图层。";
          // 
          // label2
          // 
          this.label2.AutoSize = true;
          this.label2.ForeColor = System.Drawing.Color.Red;
          this.label2.Location = new System.Drawing.Point(10, 213);
          this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(443, 12);
          this.label2.TabIndex = 4;
          this.label2.Text = "例如：选中2000以及房屋面,则必须存在名为[房屋面2000]的图层(不包含方括号)。";
          // 
          // btOK
          // 
          this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
          this.btOK.Location = new System.Drawing.Point(266, 238);
          this.btOK.Name = "btOK";
          this.btOK.Size = new System.Drawing.Size(75, 23);
          this.btOK.TabIndex = 5;
          this.btOK.Text = "确定";
          this.btOK.UseVisualStyleBackColor = true;
          // 
          // btCancel
          // 
          this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          this.btCancel.Location = new System.Drawing.Point(353, 238);
          this.btCancel.Name = "btCancel";
          this.btCancel.Size = new System.Drawing.Size(75, 23);
          this.btCancel.TabIndex = 6;
          this.btCancel.Text = "取消";
          this.btCancel.UseVisualStyleBackColor = true;
          // 
          // ConnectLocalDataDlg
          // 
          this.AcceptButton = this.btOK;
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.CancelButton = this.btCancel;
          this.ClientSize = new System.Drawing.Size(438, 272);
          this.Controls.Add(this.btCancel);
          this.Controls.Add(this.btOK);
          this.Controls.Add(this.label2);
          this.Controls.Add(this.label1);
          this.Controls.Add(this.groupBox3);
          this.Controls.Add(this.groupBox2);
          this.Controls.Add(this.groupBox1);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
          this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
          this.MaximizeBox = false;
          this.Name = "ConnectLocalDataDlg";
          this.Text = "连接数据";
          this.groupBox1.ResumeLayout(false);
          this.groupBox1.PerformLayout();
          this.groupBox2.ResumeLayout(false);
          this.groupBox3.ResumeLayout(false);
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btPath;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.CheckedListBox clScale;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckedListBox clFeature;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
    }
}
namespace BuildingGen {
  partial class GWorkSpaceInfoDlg {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.cbGenScale = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbOrgScale = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tbPath = new System.Windows.Forms.TextBox();
      this.btSave = new System.Windows.Forms.Button();
      this.btCancle = new System.Windows.Forms.Button();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbGenScale);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.cbOrgScale);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Location = new System.Drawing.Point(13, 65);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(414, 50);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "比例尺";
      // 
      // cbGenScale
      // 
      this.cbGenScale.FormattingEnabled = true;
      this.cbGenScale.Items.AddRange(new object[] {
            "1:500",
            "1:2,000",
            "1:5,000",
            "1:10,000",
            "1:25,000",
            "1:50,000",
            "1:10,000"});
      this.cbGenScale.Location = new System.Drawing.Point(294, 17);
      this.cbGenScale.Name = "cbGenScale";
      this.cbGenScale.Size = new System.Drawing.Size(111, 20);
      this.cbGenScale.TabIndex = 3;
      this.cbGenScale.Text = "1:10,000";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(211, 21);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(77, 12);
      this.label2.TabIndex = 2;
      this.label2.Text = "综合比例尺：";
      // 
      // cbOrgScale
      // 
      this.cbOrgScale.FormattingEnabled = true;
      this.cbOrgScale.Items.AddRange(new object[] {
            "1:500",
            "1:2,000",
            "1:5,000",
            "1:10,000",
            "1:25,000",
            "1:50,000",
            "1:10,000"});
      this.cbOrgScale.Location = new System.Drawing.Point(89, 17);
      this.cbOrgScale.Name = "cbOrgScale";
      this.cbOrgScale.Size = new System.Drawing.Size(116, 20);
      this.cbOrgScale.TabIndex = 1;
      this.cbOrgScale.Text = "1:2,000";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 21);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(71, 12);
      this.label1.TabIndex = 0;
      this.label1.Text = "原始比例尺:";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.tbPath);
      this.groupBox1.Location = new System.Drawing.Point(12, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(415, 55);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "路径";
      // 
      // tbPath
      // 
      this.tbPath.Location = new System.Drawing.Point(13, 21);
      this.tbPath.Name = "tbPath";
      this.tbPath.ReadOnly = true;
      this.tbPath.Size = new System.Drawing.Size(393, 21);
      this.tbPath.TabIndex = 0;
      // 
      // btSave
      // 
      this.btSave.Location = new System.Drawing.Point(271, 130);
      this.btSave.Name = "btSave";
      this.btSave.Size = new System.Drawing.Size(75, 23);
      this.btSave.TabIndex = 4;
      this.btSave.Text = "保存";
      this.btSave.UseVisualStyleBackColor = true;
      this.btSave.Click += new System.EventHandler(this.btSave_Click);
      // 
      // btCancle
      // 
      this.btCancle.Location = new System.Drawing.Point(352, 130);
      this.btCancle.Name = "btCancle";
      this.btCancle.Size = new System.Drawing.Size(75, 23);
      this.btCancle.TabIndex = 5;
      this.btCancle.Text = "取消";
      this.btCancle.UseVisualStyleBackColor = true;
      // 
      // GWorkSpaceInfoDlg
      // 
      this.AcceptButton = this.btSave;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btCancle;
      this.ClientSize = new System.Drawing.Size(442, 165);
      this.Controls.Add(this.btCancle);
      this.Controls.Add(this.btSave);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "GWorkSpaceInfoDlg";
      this.Text = "工作区属性";
      this.Load += new System.EventHandler(this.GWorkSpaceInfoDlg_Load);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ComboBox cbGenScale;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox cbOrgScale;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TextBox tbPath;
    private System.Windows.Forms.Button btSave;
    private System.Windows.Forms.Button btCancle;
  }
}
namespace DomapTool {
  partial class POIForShortConfigDlg {
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
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.btOk = new System.Windows.Forms.Button();
      this.btCancle = new System.Windows.Forms.Button();
      this.cbFullName = new System.Windows.Forms.ComboBox();
      this.cbShortName = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // comboBox1
      // 
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Location = new System.Drawing.Point(72, 10);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(121, 20);
      this.comboBox1.TabIndex = 0;
      this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(53, 12);
      this.label1.TabIndex = 1;
      this.label1.Text = "操作图层";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(14, 47);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(53, 12);
      this.label2.TabIndex = 3;
      this.label2.Text = "全称字段";
      // 
      // btOk
      // 
      this.btOk.Location = new System.Drawing.Point(213, 8);
      this.btOk.Name = "btOk";
      this.btOk.Size = new System.Drawing.Size(75, 23);
      this.btOk.TabIndex = 5;
      this.btOk.Text = "确定";
      this.btOk.UseVisualStyleBackColor = true;
      this.btOk.Click += new System.EventHandler(this.btOk_Click);
      // 
      // btCancle
      // 
      this.btCancle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btCancle.Location = new System.Drawing.Point(213, 42);
      this.btCancle.Name = "btCancle";
      this.btCancle.Size = new System.Drawing.Size(75, 23);
      this.btCancle.TabIndex = 6;
      this.btCancle.Text = "取消";
      this.btCancle.UseVisualStyleBackColor = true;
      this.btCancle.Click += new System.EventHandler(this.btCancle_Click);
      // 
      // cbFullName
      // 
      this.cbFullName.FormattingEnabled = true;
      this.cbFullName.Location = new System.Drawing.Point(72, 45);
      this.cbFullName.Name = "cbFullName";
      this.cbFullName.Size = new System.Drawing.Size(121, 20);
      this.cbFullName.TabIndex = 7;
      // 
      // cbShortName
      // 
      this.cbShortName.FormattingEnabled = true;
      this.cbShortName.Location = new System.Drawing.Point(72, 79);
      this.cbShortName.Name = "cbShortName";
      this.cbShortName.Size = new System.Drawing.Size(121, 20);
      this.cbShortName.TabIndex = 9;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(14, 81);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(53, 12);
      this.label3.TabIndex = 8;
      this.label3.Text = "简称字段";
      // 
      // POIForShortConfigDlg
      // 
      this.AcceptButton = this.btOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btCancle;
      this.ClientSize = new System.Drawing.Size(295, 111);
      this.Controls.Add(this.cbShortName);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.cbFullName);
      this.Controls.Add(this.btCancle);
      this.Controls.Add(this.btOk);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.comboBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "POIForShortConfigDlg";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "POI简称配置";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btOk;
    private System.Windows.Forms.Button btCancle;
    private System.Windows.Forms.ComboBox cbFullName;
    private System.Windows.Forms.ComboBox cbShortName;
    private System.Windows.Forms.Label label3;
  }
}
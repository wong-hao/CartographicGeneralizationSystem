namespace DatabaseUpdate {
  partial class LayersTypePropertyPage {
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.labelPlaceholder = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbLayerType = new System.Windows.Forms.ComboBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // labelPlaceholder
      // 
      this.labelPlaceholder.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelPlaceholder.Location = new System.Drawing.Point(0, 0);
      this.labelPlaceholder.Name = "labelPlaceholder";
      this.labelPlaceholder.Size = new System.Drawing.Size(305, 160);
      this.labelPlaceholder.TabIndex = 1;
      this.labelPlaceholder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbLayerType);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(305, 160);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Õº≤„¿‡–Õ";
      // 
      // cbLayerType
      // 
      this.cbLayerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbLayerType.FormattingEnabled = true;
      this.cbLayerType.Location = new System.Drawing.Point(6, 20);
      this.cbLayerType.Name = "cbLayerType";
      this.cbLayerType.Size = new System.Drawing.Size(121, 20);
      this.cbLayerType.TabIndex = 0;
      this.cbLayerType.SelectedIndexChanged += new System.EventHandler(this.cbLayerType_SelectedIndexChanged);
      // 
      // LayersTypePropertyPage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.labelPlaceholder);
      this.Name = "LayersTypePropertyPage";
      this.Size = new System.Drawing.Size(305, 160);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label labelPlaceholder;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox cbLayerType;



  }
}

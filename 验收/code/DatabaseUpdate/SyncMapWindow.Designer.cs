namespace DatabaseUpdate {
  partial class SyncMapWindow {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncMapWindow));
      this.labelPlaceholder = new System.Windows.Forms.Label();
      this.axMapControl = new ESRI.ArcGIS.Controls.AxMapControl();
      ((System.ComponentModel.ISupportInitialize)(this.axMapControl)).BeginInit();
      this.SuspendLayout();
      // 
      // labelPlaceholder
      // 
      this.labelPlaceholder.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelPlaceholder.Location = new System.Drawing.Point(0, 0);
      this.labelPlaceholder.Name = "labelPlaceholder";
      this.labelPlaceholder.Size = new System.Drawing.Size(564, 281);
      this.labelPlaceholder.TabIndex = 0;
      this.labelPlaceholder.Text = "Place controls on the canvas for your dockable window definition";
      this.labelPlaceholder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // axMapControl
      // 
      this.axMapControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.axMapControl.Location = new System.Drawing.Point(0, 0);
      this.axMapControl.Name = "axMapControl";
      this.axMapControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axMapControl.OcxState")));
      this.axMapControl.Size = new System.Drawing.Size(564, 281);
      this.axMapControl.TabIndex = 1;
      // 
      // SyncMap
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.axMapControl);
      this.Controls.Add(this.labelPlaceholder);
      this.Name = "SyncMap";
      this.Size = new System.Drawing.Size(564, 281);
      ((System.ComponentModel.ISupportInitialize)(this.axMapControl)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label labelPlaceholder;
    private ESRI.ArcGIS.Controls.AxMapControl axMapControl;
  }
}

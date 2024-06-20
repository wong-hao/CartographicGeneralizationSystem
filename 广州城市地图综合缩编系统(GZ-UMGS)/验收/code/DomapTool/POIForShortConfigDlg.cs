using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace DomapTool {
  public partial class POIForShortConfigDlg : Form {
    public POIForShortConfigDlg(IMap map) {
      InitializeComponent();
      for (int i = 0; i < map.LayerCount; i++) {
        IFeatureLayer l = map.get_Layer(i) as IFeatureLayer;
        if (l!= null) { 
          comboBox1.Items.Add(new FeatureLayerWarp(l));
        }
      }
      if(comboBox1.Items.Count > 0)
        comboBox1.SelectedIndex = 0;
    }

    private void btOk_Click(object sender, EventArgs e) {
      this.DialogResult = DialogResult.OK;
    }

    private void btCancle_Click(object sender, EventArgs e) {
      this.DialogResult = DialogResult.Cancel;
    }

    public IFeatureLayer SelectLayer {
      get {
        return (comboBox1.SelectedItem as FeatureLayerWarp).layer;
      }
    }
    public IField FullField {
      get {
        return (this.cbFullName.SelectedItem as FieldWarp).field;
      }
    }
    public IField ShortField {
      get {
        return (this.cbShortName.SelectedItem as FieldWarp).field;
      }
    }
    class FeatureLayerWarp {
      internal IFeatureLayer layer;
      internal FeatureLayerWarp(IFeatureLayer l) {
        this.layer = l;
      }
      public override string ToString() {
        return layer.Name;
      }
    }
    class FieldWarp {
      internal IField field;
      internal FieldWarp(IField f) {
        this.field = f;
      }
      public override string ToString() {
        return field.Name;
      }
    }
    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
      if (this.SelectLayer == null)
        return;

      IFeatureClass fc = this.SelectLayer.FeatureClass;
      for (int i = 0; i < fc.Fields.FieldCount; i++) {
        IField f = fc.Fields.get_Field(i);
        if (f.Type == esriFieldType.esriFieldTypeString && f.Editable) {
          FieldWarp fw = new FieldWarp(f);
          this.cbFullName.Items.Add(fw);
          this.cbShortName.Items.Add(fw);
        }
      }
      if (this.cbFullName.Items.Count > 0) {
        this.cbFullName.SelectedIndex = 0;
        this.cbShortName.SelectedIndex = 0;
      }
    }
  }
}

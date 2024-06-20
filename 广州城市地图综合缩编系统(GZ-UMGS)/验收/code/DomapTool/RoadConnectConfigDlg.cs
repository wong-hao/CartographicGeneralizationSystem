using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
namespace DomapTool {
  public partial class RoadConnectConfigDlg : Form {
    public RoadConnectConfigDlg(IMap map) {
      InitializeComponent();
      for (int i = 0; i < map.LayerCount; i++) {
        IFeatureLayer l = map.get_Layer(i) as IFeatureLayer;
        if (l!= null && l.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline) { 
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
    public double Distance {
      get {
        return Convert.ToInt32(textBox1.Text);
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
  }
}

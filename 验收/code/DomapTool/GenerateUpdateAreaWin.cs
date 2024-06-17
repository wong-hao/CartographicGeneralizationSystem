using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace DomapTool {
  public partial class GenerateUpdateAreaWin : Form {
    private void AddLayerInGroupLayer(ICompositeLayer grouplayer) {
      for (int i = 0; i < grouplayer.Count; i++) {
        ILayer layer = grouplayer.get_Layer(i);
        if (layer is IGroupLayer) {
          AddLayerInGroupLayer(layer as ICompositeLayer);
        }

        IFeatureLayer l = layer as IFeatureLayer;
        if (l != null && l.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon) {
          comboBox1.Items.Add(new FeatureLayerWarp(l));
        }
        if (l != null && l.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline) {
          comboBox2.Items.Add(new FeatureLayerWarp(l));
        }
      }
    }
    public GenerateUpdateAreaWin(IMap map) {
      InitializeComponent();
      IFeatureLayer defaultLayer = new FeatureLayerClass();
      defaultLayer.Name = "全图";
      FeatureLayerWarp defaultWap = new FeatureLayerWarp(defaultLayer);
      comboBox2.Items.Add(defaultWap);
      for (int i = 0; i < map.LayerCount; i++) {
        ILayer layer = map.get_Layer(i);
        if (layer is IGroupLayer) {
          AddLayerInGroupLayer(layer as ICompositeLayer);
        }

        IFeatureLayer l = layer as IFeatureLayer;
        if (l != null && l.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon) {
          FeatureLayerWarp fw = new FeatureLayerWarp(l);
          comboBox1.Items.Add(fw);
          comboBox2.Items.Add(fw);
        }
      }
      if (comboBox1.Items.Count > 0)
        comboBox1.SelectedIndex = 0;
      if (comboBox2.Items.Count > 0)
        comboBox2.SelectedIndex = 0;
    }

    private void button1_Click(object sender, EventArgs e) {
      this.DialogResult = DialogResult.OK;
    }

    private void button2_Click(object sender, EventArgs e) {
      this.DialogResult = DialogResult.Cancel;
    }

    public IFeatureLayer SelectUpdateLayer {
      get {
        return (comboBox1.SelectedItem as FeatureLayerWarp).layer;
      }
    }

    public IFeatureLayer SelectUpdateAreaLayer {
      get {
        return (comboBox2.SelectedItem as FeatureLayerWarp).layer;
      }
    }

   public class FeatureLayerWarp {
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

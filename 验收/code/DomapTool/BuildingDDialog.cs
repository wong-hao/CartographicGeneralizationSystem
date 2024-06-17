using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace DomapTool {
  public partial class BuildingDDialog : Form {
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
    public BuildingDDialog(IMap map) {
      InitializeComponent();
      IFeatureLayer m = new FeatureLayerClass();
      m.Name = "无";
      comboBox2.Items.Add(new FeatureLayerWarp(m));
      for (int i = 0; i < map.LayerCount; i++) {
        ILayer layer = map.get_Layer(i);
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
      if (comboBox1.Items.Count > 0)
        comboBox1.SelectedIndex = 0;
      if (comboBox2.Items.Count > 0)
        comboBox2.SelectedIndex = 0;
      textBox1.Text = GGenPara.Para["建筑物最小上图面积"].ToString();
      textBox2.Text = GGenPara.Para["建筑物合并距离"].ToString();
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

    private void button1_Click(object sender, EventArgs e) {
      this.DialogResult = DialogResult.OK;
    }

    private void button2_Click(object sender, EventArgs e) {
      this.DialogResult = DialogResult.Cancel;
    }
    public IFeatureLayer buildingLayer {
      get {
        return (comboBox1.SelectedItem as FeatureLayerWarp).layer;
      }
    }
    public IFeatureLayer roadLayer {
      get {
        return (comboBox2.SelectedItem as FeatureLayerWarp).layer;
      }
    }
    public double BArea {
      get {
        double area = Convert.ToDouble(textBox1.Text);
        GGenPara.Para["建筑物最小上图面积"] = area;
        return area;
      }
    }
    public double BDis {
      get {
        double dis = Convert.ToDouble(textBox2.Text);
        GGenPara.Para["建筑物合并距离"] = dis;
        return dis;
      }
    }
  }
}

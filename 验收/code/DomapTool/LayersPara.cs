using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;


namespace DomapTool
{
    public partial class LayersPara : Form
    {
        public LayersPara(IMap map)
        {
            InitializeComponent();
            for (int i = 0; i < map.LayerCount; i++)
            {
                IFeatureLayer l = map.get_Layer(i) as IFeatureLayer;
                //if (l != null && l.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                if (l != null)
                {
                    comboBox1.Items.Add(new FeatureLayerWarp(l));
                }
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            if (GGenPara.Para["非邻近图斑合并距离"] != null)
              textBox1.Text = GGenPara.Para["非邻近图斑合并距离"].ToString();
        }

        class FeatureLayerWarp
        {
            internal IFeatureLayer layer;
            internal FeatureLayerWarp(IFeatureLayer l)
            {
                this.layer = l;
            }
            public override string ToString()
            {
                return layer.Name;
            }
        }

        public IFeatureLayer SelectLayer
        {
            get
            {
                return (comboBox1.SelectedItem as FeatureLayerWarp).layer;
            }
        }
        public double dis
        {
            get
            {
              double v = Convert.ToDouble(textBox1.Text);
              GGenPara.Para["非邻近图斑合并距离"] = v;
              return v;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace DatabaseUpdate
{
    public partial class GenerateUpdateAreaWin2 : Form
    {
        public GenerateUpdateAreaWin2(IMap map)
        {
            InitializeComponent();
            for (int i = 0; i < map.LayerCount; i++)
            {
                IFeatureLayer l = map.get_Layer(i) as IFeatureLayer;
                if (l != null && l.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                {
                    comboBox1.Items.Add(new FeatureLayerWarp(l));
                }
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
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

        public string UpdateAreaLayerFileName
        {
            get
            {
                return textBox1.Text;
            }
        }

        public IFeatureLayer SelectUpdateLayer
        {
            get
            {
                return (comboBox1.SelectedItem as FeatureLayerWarp).layer;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        public SaveFileDialog sfd;
        private void button1_Click(object sender, EventArgs e)
        {
            if (sfd == null)
            {
                sfd = new SaveFileDialog();
                sfd.Title = "生成更新区域";
                sfd.Filter = "shp(*.shp)|*.shp";
            }
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = sfd.FileName;
            }
        }


    }
}

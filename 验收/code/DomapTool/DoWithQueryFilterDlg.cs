using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace DomapTool
{
    public partial class DoWithQueryFilterDlg : Form
    {
        bool isFirstSelected3 = false;
        public DoWithQueryFilterDlg(IMap map)
        {
            InitializeComponent();
            //dataSta = new DataStatisticsClass();
            for (int i = 0; i < map.LayerCount; i++)
            {
                IFeatureLayer l = map.get_Layer(i) as IFeatureLayer;
                if (l != null)
                {
                    comboBox1.Items.Add(new FeatureLayerWarp(l));
                }
            }
            if (comboBox1.Items.Count > 0)
            {
                isFirstSelected3 = true;
                comboBox1.SelectedIndex = 0;
            }

            textBox3.Text = GGenPara.Para["湖泊毗邻最小面积"].ToString();
            textBox2.Text = GGenPara.Para["湖泊毗邻宽度"].ToString();
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

        public double width
        {
            get
            {
              double v = Convert.ToDouble(textBox2.Text);
              GGenPara.Para["湖泊毗邻宽度"] = v;
                return v;
            }
        }

        public double minArea
        {
            get
            {
                double v = Convert.ToDouble(textBox3.Text);
                GGenPara.Para["湖泊毗邻最小面积"] = v;

                return v;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.SelectLayer == null || isFirstSelected3)
            {
                isFirstSelected3 = false;
                return;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}

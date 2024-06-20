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
    public partial class SelectUpdateArea : Form
    {
        public SelectUpdateArea(IMap map)
        {
            InitializeComponent();
            for (int i = 0; i < map.LayerCount; i++)
            {
                IFeatureLayer l = map.get_Layer(i) as IFeatureLayer;
                if (l != null && l.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                {
                    FeatureLayerWarp ss = new FeatureLayerWarp(l);
                    comboBox1.Items.Add(ss);
                    //comboBox2.Items.Add(ss);
                }
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            //if (comboBox2.Items.Count > 0)
            //    comboBox2.SelectedIndex = 0;
        }

        public IFeatureLayer UpdateAreaLayer
        {
            get
            {
                return (comboBox1.SelectedItem as FeatureLayerWarp).layer;
            }
        }

        //public IFeatureLayer UpdateLayer
        //{
        //    get
        //    {
        //        return (comboBox2.SelectedItem as FeatureLayerWarp).layer;
        //    }
        //}

        public string UpdateAreaLayerFileName
        {
            get
            {
                return textBox1.Text;
            }
        }

        public bool IsActUpdate
        {
            get
            {
                return checkBox1.Checked;
            }
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

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
                radioButton2.Enabled = false;
                comboBox1.Enabled = false;
            }
            else
            {
                radioButton2.Enabled = true;
                comboBox1.Enabled = true;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
                radioButton1.Enabled = false;
                textBox1.Enabled = false;
                button1.Enabled = false;
            }
            else
            {
                textBox1.Enabled = true;
                button1.Enabled = true;    
            }
        }

        public SaveFileDialog sfd;
        private void button1_Click(object sender, EventArgs e)
        {
            if (sfd == null)
            {
                sfd = new SaveFileDialog();
                sfd.Title = "生成更新区域图层";
                sfd.Filter = "shp(*.shp)|*.shp";
            }
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = sfd.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            return;
        }

    }
}

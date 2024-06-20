using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class JustifyCulvertForm : Form
    {
        public string relatedLayerName;
        public string justifyFeatureName;
        public string justifyLayerName;
        public double tolerance;
        ILayer layerJustify;
        ILayer layerRelated;
        public JustifyCulvertForm()
        {
            InitializeComponent();
            this.cb_feature.SelectedIndex = 0;
            this.cb_layer.SelectedIndex = 0;
            //初始化
            relatedLayerName = this.cb_layer.Text;
            justifyFeatureName = this.cb_feature.Text.Split(',').First();
            justifyLayerName = this.cb_feature.Text.Split(',').Last();
            tolerance = Convert.ToDouble(this.numericUpDown1.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            relatedLayerName = this.cb_layer.Text;
            justifyFeatureName = this.cb_feature.Text.Split(',').First();
            justifyLayerName = this.cb_feature.Text.Split(',').Last();
            tolerance = Convert.ToDouble(this.numericUpDown1.Value);
            DialogResult = DialogResult.OK;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void JustifyCulvertForm_Load(object sender, EventArgs e)
        {

        }
    }
}

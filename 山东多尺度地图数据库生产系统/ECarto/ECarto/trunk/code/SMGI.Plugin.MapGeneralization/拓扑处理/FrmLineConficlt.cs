using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
namespace SMGI.Plugin.MapGeneralization
{
    public partial class FrmLineConficlt : DevExpress.XtraEditors.XtraForm
    {
        public double m_disValue;
        private IMap pMap;
        public ILayer hydlLayer;
        public ILayer lrdlLayer;
        public FrmLineConficlt()
        {
            InitializeComponent();
            m_disValue = 50;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            m_disValue = Convert.ToDouble(tbDistance.Text);
            string hydlSelectLayerName = cmbRiver.SelectedItem.ToString();
            string lrdlSelectLayerName = cmbRoad.SelectedItem.ToString();

            var ls = pMap.get_Layers();
            var layer = ls.Next();
            for (; layer != null; layer = ls.Next())
            {
                if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;
                if (layer.Name == hydlSelectLayerName)
                {
                    hydlLayer = layer;
                }

                if (layer.Name == lrdlSelectLayerName)
                {
                    lrdlLayer = layer;
                }
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmLineConficlt_Load(object sender, EventArgs e)
        {
            pMap = GApplication.Application.ActiveView as IMap;
            tbDistance.Text = m_disValue.ToString();
            var ls = pMap.get_Layers();
            var layer = ls.Next();
            for (; layer != null; layer = ls.Next())
            {
                if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;
              
                cmbRiver.Items.Add(layer.Name.ToString());
                if ((layer as IFeatureLayer).FeatureClass.AliasName == "HYDL")
                {
                    cmbRiver.SelectedItem = layer.Name.ToString();
                }
            }

            ls = pMap.get_Layers();
            layer = ls.Next();
            for (; layer != null; layer = ls.Next())
            {
                if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;

                cmbRoad.Items.Add(layer.Name.ToString());
                if ((layer as IFeatureLayer).FeatureClass.AliasName == "LRDL")
                {
                    cmbRoad.SelectedItem = layer.Name.ToString();
                }
            }
        }

        private void cmbRiver_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectLayerName = cmbRiver.SelectedItem.ToString();
            var ls = pMap.get_Layers();
            var layer = ls.Next();
            for (; layer != null; layer = ls.Next())
            {
                if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;
                if (layer.Name == selectLayerName)
                {
                    hydlLayer = layer;
                }
            }
        }

        private void cmbRoad_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectLayerName = cmbRoad.SelectedItem.ToString();
            var ls = pMap.get_Layers();
            var layer = ls.Next();
            for (; layer != null; layer = ls.Next())
            {
                if (!(layer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;
                if (layer.Name == selectLayerName)
                {
                    lrdlLayer = layer;
                }
            }
        }
    }
}
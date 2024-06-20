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
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.EmergencyMap

{
    public partial class LayerSelectableManageForm : Form
    {
        GApplication app;
        Dictionary< int,ILayer> lyrDic = new Dictionary<int,ILayer>();
        public LayerSelectableManageForm(GApplication app)
        {
            InitializeComponent();
            this.app = app;
        }

        private void MaskRuleConfigForm_Load(object sender, EventArgs e)
        {
            var layers = app.Workspace.LayerManager.GetLayer(l => { return l is IFeatureLayer; });
            int i = 0;
            foreach(var pLayer in layers)
            {
                if (pLayer is IFeatureLayer)
                {
                    IFeatureLayer flyr = pLayer as IFeatureLayer;
                    //if (pLayer.Name.ToUpper() == "PAGE")
                    //{
                    //    flyr.Selectable = true;
                    //    continue;
                    //}
                    lyrDic.Add(i,pLayer);
                    
                    object[] objs = new object[] { i + 1, pLayer.Name, flyr.Selectable};
                    dataGridView1.Rows.Add(objs);
                    i++;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int i=0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                //string layerName = row.Cells["图层"].Value.ToString().Trim();

                //var lyr = app.Workspace.LayerManager.GetLayer(l => { return l.Name == layerName; }).FirstOrDefault();
                //if (lyr == null)
                //    continue;
                //if (lyr is IFeatureLayer)
                //{
                //    IFeatureLayer flyr = lyr as IFeatureLayer;
                //    flyr.Selectable = Convert.ToBoolean(row.Cells["可选"].Value);
                //}
                ILayer lyr = lyrDic[i];
                IFeatureLayer flyr = lyr as IFeatureLayer;
                flyr.Selectable = Convert.ToBoolean(row.Cells["可选"].Value);
                 i++;


            }
            this.Close();
        }

        private void btnSelectALL_Click(object sender, EventArgs e)
        {

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["可选"].Value = true;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["可选"].Value = false;
            }
        }
    }
}

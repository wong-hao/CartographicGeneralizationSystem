using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class UnselectFeatureToolForm : Form
    {
        #region 属性
        public List<IFeatureClass> FeatureClassList
        {
            get
            {
                return _featureClassList;
            }
        }
        private List<IFeatureClass> _featureClassList;

        #endregion

        private GApplication _app;
        string _selStateFN;
        public UnselectFeatureToolForm(GApplication app, string selStateFN = "selectstate")
        {
            InitializeComponent();
            _app = app;
            _selStateFN = selStateFN;
        }

        private void CheckNewCollabGUIDForm_Load(object sender, EventArgs e)
        {
            //检索当前工作空间的所有图层对应要素类名称
            List<string> fcNameList = new List<string>();
            chkFCList.ValueMember = "Key";
            chkFCList.DisplayMember = "Value";
            var layers = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer));
            foreach (var lyr in layers)
            {
                IFeatureLayer feLayer = lyr as IFeatureLayer;
                IFeatureClass fc = feLayer.FeatureClass;
                if (fc == null)
                    continue;//空图层

                if (fc.ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint)
                    continue;//非点图层

                if ((fc as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)
                    continue;//临时数据

                if (fc.FindField(_selStateFN) == -1)
                    continue;//不包含选取字段

                if (!fcNameList.Contains(fc.AliasName))
                {
                    chkFCList.Items.Add(new KeyValuePair<IFeatureClass, string>(fc, fc.AliasName), false);
                    fcNameList.Add(fc.AliasName);
                }
            }
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFCList.Items.Count; i++)
            {
                chkFCList.SetItemChecked(i, true);
            }
        }

        private void btnUnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFCList.Items.Count; i++)
            {
                chkFCList.SetItemChecked(i, false);
            }
            chkFCList.ClearSelected();
        }


        private void btOK_Click(object sender, EventArgs e)
        {
            if (chkFCList.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择至少一个要素类！");
                return;
            }

            _featureClassList = new List<IFeatureClass>();
            foreach (var item in chkFCList.CheckedItems)
            {
                var kv = (KeyValuePair<IFeatureClass, string>)item;
                _featureClassList.Add(kv.Key);
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        
    }
}

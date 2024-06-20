using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
namespace SMGI.Common.AttributeTable
{
    public partial class FrmLayerOpenAttribute : Form
    {
        private IMap pMap;
        private IActiveView pAc;
        private IFeatureClass pCurrentFcl;
        private ILayer pCurrentLayer;
        private int OIDIndex = 0;
        public GApplication G_App;
        public FrmLayerOpenAttribute(ILayer _pLayer,IMap _pMap)
        {
            InitializeComponent();
            pCurrentLayer = _pLayer;
            pCurrentFcl = (_pLayer as IFeatureLayer).FeatureClass;
            pMap = _pMap;
            pAc = pMap as IActiveView;
        }

        private void FrmLayerOpenAttribute_Load(object sender, EventArgs e)
        {
            this.Text += "-"+pCurrentFcl.AliasName;
            InitColums();
            InitFeatureAttribute();
        }
        //初始化列表
        private void InitColums()
        {
            DataGridViewTextBoxColumn Column = null;
            
            for (int i = 0; i < pCurrentFcl.Fields.FieldCount; i++)
            {
                IField pfield = pCurrentFcl.Fields.get_Field(i);
                if (pfield.Name.ToUpper() == "OIDOBJECT")
                {
                    OIDIndex = i;
                }
                Column = new DataGridViewTextBoxColumn();
                Column.HeaderText = pfield.Name;
                Column.Name = pfield.Name;
                this.dataGridView1.Columns.Add(Column);
            }
        }
        private void InitFeatureAttribute()
        {
            IFeatureCursor pcursor = pCurrentFcl.Search(null, false);
            IFeature fe = null;
            int countFlag = 1;
            using (var wo = G_App.SetBusy())
            {
                wo.SetText("正在加载数据...");
                while ((fe = pcursor.NextFeature()) != null)
                {
                    string[] values = new string[pCurrentFcl.Fields.FieldCount];
                    for (int i = 0; i < pCurrentFcl.Fields.FieldCount; i++)
                    {
                        values[i] = fe.get_Value(i).ToString();
                        if (pCurrentFcl.Fields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                        {
                            values[i] = pCurrentFcl.ShapeType.ToString();
                        }

                    }
                    dataGridView1.Rows.Add(values);
                    dataGridView1.Rows[countFlag-1].HeaderCell.Value = countFlag.ToString();
                    countFlag++;
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pcursor);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int columnIndex = e.ColumnIndex;
            int rowIndex = e.RowIndex;
            if (columnIndex == -1)
            {
                //获取OID
                DataGridViewRow dr=this.dataGridView1.Rows[rowIndex];
                int oid = Convert.ToInt32(dr.Cells[OIDIndex].Value.ToString().Trim());
                IFeature fe = pCurrentFcl.GetFeature(oid);
                ZoomToFeature(fe);
                //pMap.ClearSelection();
                //pMap.SelectFeature(pCurrentLayer, fe);
                //pAc.Extent = fe.Extent;
                //pAc.Refresh();
            }

        }
        private void ZoomToFeature(IFeature fe)
        {
            if (pCurrentFcl.ShapeType == esriGeometryType.esriGeometryPoint)
            {
                pMap.ClearSelection();
                pMap.SelectFeature(pCurrentLayer, fe);
                G_App.MapControl.CenterAt(fe.Shape as IPoint);
                pAc.Refresh();
            }
            else if(pCurrentFcl.ShapeType == esriGeometryType.esriGeometryPolyline)
            {
                pMap.ClearSelection();
                pMap.SelectFeature(pCurrentLayer, fe);
                pAc.Extent = fe.Extent;
                pAc.Refresh();
            }
            else if (pCurrentFcl.ShapeType == esriGeometryType.esriGeometryPolygon)
            {
                pMap.ClearSelection();
                pMap.SelectFeature(pCurrentLayer, fe);
                pAc.Extent = fe.Extent;
                pAc.Refresh();
            }
            else 
            {
            }
        }
    }
}

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
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class DataSearchForm : DevExpress.XtraEditors.XtraForm
    {
        IFeatureLayer _objLayer;
        public DataSearchForm()
        {
            InitializeComponent();

            _objLayer = null;
        }

        private void DataSearchForm_Load(object sender, EventArgs e)
        {
            cbObjectLayer.ValueMember = "Key";
            cbObjectLayer.DisplayMember = "Value";

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && (l as IFeatureLayer).FeatureClass != null;
            })).ToArray();
            foreach (var l in lyrs)
            {
                cbObjectLayer.Items.Add(new KeyValuePair<IFeatureLayer, string>(l as IFeatureLayer, l.Name));
            }
        }

        private void cbObjectLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkFieldList.Items.Clear();
            if (cbObjectLayer.SelectedIndex != -1)
            {
                var item = (KeyValuePair<IFeatureLayer, string>)cbObjectLayer.SelectedItem;
                _objLayer = item.Key;
                IFeatureClass fc = _objLayer.FeatureClass;
                if (fc != null)
                {
                    for (var i = 0; i < fc.Fields.FieldCount; i++)
                    {
                        var fd = fc.Fields.Field[i];

                        if (fd.Type != esriFieldType.esriFieldTypeString)
                            continue;

                        chkFieldList.Items.Add(fd.Name);
                    }
                }
            }
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFieldList.Items.Count; i++)
            {
                chkFieldList.SetItemChecked(i, true);
            }
        }

        private void btnUnSelAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkFieldList.Items.Count; i++)
            {
                chkFieldList.SetItemChecked(i, false);
            }
            chkFieldList.ClearSelected();
        }

        private void btnResearch_Click(object sender, EventArgs e)
        {
            search(tbSearchText.Text);
        }

        private void tbSearchText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                search(tbSearchText.Text);
            }

        }

        private void dgResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (_objLayer == null)
                return;

            DataGridViewLinkColumn placeNameColumn = dgResult.Columns[e.ColumnIndex] as DataGridViewLinkColumn;
            if (null == placeNameColumn || placeNameColumn.Name != "reslut")
                return;

            int fid = int.Parse(dgResult.Rows[e.RowIndex].HeaderCell.Value.ToString());

            IFeature f = _objLayer.FeatureClass.GetFeature(fid);
            if (f == null)
                return;
            ZoomToFeature(_objLayer, f);

            Marshal.ReleaseComObject(f);
        }

        private void search(string text)
        {
            if (_objLayer == null)
            {
                MessageBox.Show(this, "请指定目标图层！");
                return;
            }
            if (chkFieldList.CheckedItems.Count == 0)
            {
                MessageBox.Show(this, "请选择至少一个字段项！");
                return;
            }
            
            btnResearch.Enabled = false;
            try
            {
                string filter = "";
                foreach (var item in chkFieldList.CheckedItems)
                {
                    if (filter != "")
                    {
                        filter += " or ";
                    }

                    if (chbFuzzySearch.Checked)
                    {
                        filter += string.Format("({0} like '%{1}%')", item.ToString(), text);
                    }
                    else
                    {
                        filter += string.Format("({0} = '{1}')", item.ToString(), text);
                    }
                }


                Dictionary<int, string> resultList = new Dictionary<int, string>();
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = filter;

                IFeatureCursor feCursor = _objLayer.Search(qf, true);
                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    string caption = "";
                    foreach (var item in chkFieldList.CheckedItems)
                    {
                        if (caption != "")
                            caption += "；";

                        caption += fe.get_Value(fe.Fields.FindField(item.ToString())).ToString();
                    }

                    resultList.Add(fe.OID, caption);
                }
                Marshal.ReleaseComObject(feCursor);

                //更新
                updateResultTable(_objLayer, resultList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
            finally
            {
                btnResearch.Enabled = true;
            }
            
        }

        private void updateResultTable(IFeatureLayer objLayer, Dictionary<int, string> resultList)
        {
            dgResult.Rows.Clear();
            resultTip.Text = "检索到0条要素";
            if (resultList == null || resultList.Count == 0)
            {
                return;
            }

            foreach(var kv in resultList)
            {
                IFeature f = objLayer.FeatureClass.GetFeature(kv.Key);
                if (f == null || f.Shape == null || f.Shape.IsEmpty)
                    continue;

                int rowIndex = dgResult.Rows.Add();
                dgResult.Rows[rowIndex].Cells["reslut"].Value = kv.Value;
                dgResult.Rows[rowIndex].HeaderCell.Value = kv.Key;

                Marshal.ReleaseComObject(f);
            }

            resultTip.Text = string.Format("检索到{0}条要素", resultList.Count);
        }

        private void ZoomToFeature(IFeatureLayer objLayer, IFeature f)
        {
            var env = f.Shape.Envelope;
            if ((env as IArea).Area > 1.0)
            {
                env.Expand(2, 2, true);
            }
            else
            {
                var envelope = objLayer.AreaOfInterest;
                env.Width = envelope.Width * 0.05;
                env.Height = envelope.Height * 0.05;
                if (env.Width < 1e-6)
                {
                    env.Height = GApplication.Application.MapControl.ActiveView.Extent.Height * 0.05;
                    env.Width = GApplication.Application.MapControl.ActiveView.Extent.Width * 0.05;

                }
                env.CenterAt((f.Shape.Envelope as IArea).Centroid);
            }

            GApplication.Application.MapControl.Extent = env;
            DelayFlashGeometry(f.Shape);
        }

        void DelayFlashGeometry(IGeometry geo)
        {
            Timer t = new Timer { Interval = 100 };
            t.Tick += (o, e) =>
            {
                t.Stop();
                GApplication.Application.MapControl.FlashShape(geo);
                t.Dispose();
            };
            t.Start();
        }
    }
}

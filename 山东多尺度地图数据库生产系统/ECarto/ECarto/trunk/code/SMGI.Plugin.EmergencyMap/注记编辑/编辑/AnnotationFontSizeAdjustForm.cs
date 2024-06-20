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

namespace SMGI.Plugin.EmergencyMap
{
    public partial class AnnotationFontSizeAdjustForm : Form
    {
        private string _typeFN = "分类";

        public IFeatureLayer ObjAnnoLayer
        {
            get
            {
                if(cbAnnotationLayer.SelectedItem == null)
                    return null;

                return ((KeyValuePair<IFeatureLayer, string>)cbAnnotationLayer.SelectedItem).Key;
            }

        }

        public string AnnoSelectSQL
        {
            get
            {
                if (cbAnnotationType.Text == "全部")
                {
                    return "";
                }
                else
                {
                    return string.Format("{0} = '{1}'", _typeFN, cbAnnotationType.Text);
                }
            }
        }

        public double AdjustScale
        {
            get
            {
                return (double)nupFontSizeScale.Value;
            }
        }
        
        public AnnotationFontSizeAdjustForm()
        {
            InitializeComponent();
        }

        private void AnnotationFontSizeAdjustForm_Load(object sender, EventArgs e)
        {
            //获取当前工作空间的所有注记图层
            cbAnnotationLayer.ValueMember = "Key";
            cbAnnotationLayer.DisplayMember = "Value";

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IFDOGraphicsLayer; })).ToArray();
            foreach (var lyr in lyrs)
            {
                if (lyr is IFeatureLayer)
                {
                    cbAnnotationLayer.Items.Add(new KeyValuePair<IFeatureLayer, string>(lyr as IFeatureLayer, lyr.Name));
                }
            }
            if (cbAnnotationLayer.Items.Count > 0)
                cbAnnotationLayer.SelectedIndex = cbAnnotationLayer.Items.Count - 1;

        }

        private void cbAnnotationLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbAnnotationType.Items.Clear();

            if (ObjAnnoLayer == null)
                return;

            cbAnnotationType.Items.Add("全部");
            int typeIndex = ObjAnnoLayer.FeatureClass.FindField(_typeFN);
            if (typeIndex != -1)
            {
                //分类的唯一值
                IQueryFilter qf = new QueryFilterClass();
                qf.SubFields = _typeFN;
                IFeatureCursor feCursor = ObjAnnoLayer.Search(qf, true);
                IDataStatistics dataStatistics = new DataStatisticsClass();
                dataStatistics.Field = _typeFN;//获取统计字段
                dataStatistics.Cursor = feCursor as ICursor;
                var enumerator = dataStatistics.UniqueValues;
                while (enumerator.MoveNext())
                {
                    string valuet = enumerator.Current.ToString();
                    if (valuet == null) 
                    { 
                        continue; 
                    }

                    cbAnnotationType.Items.Add(valuet);
                }
                Marshal.ReleaseComObject(feCursor);
            }
            cbAnnotationType.SelectedIndex = 0;

        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cbAnnotationLayer.Text == "")
            {
                MessageBox.Show("请选择目标注记图层！");
                return;
            }

            if (cbAnnotationType.Text == "")
            {
                MessageBox.Show("请选择注记类型！");
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}

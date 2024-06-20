using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class ExportDataForm : Form
    {
        public string IPAdress
        {
            get
            {
                return tbIPAdress.Text.Trim();
            }
        }
        public string UserName
        {
            get
            {
                return tbUserName.Text.Trim();
            }
        }
        public string Password
        {
            get
            {
                return tbPassword.Text.Trim();
            }
        }
        public string DataBase
        {
            get
            {
                return tbDataBase.Text.Trim();
            }
        }
        public bool Range
        {
            get
            {
                return cbRange.Checked;
            }
        }
        private IGeometry _rangeGeometry;
        public IGeometry RangeGeometry
        {
            get
            {
                return _rangeGeometry;
            }
        }
        public string OutputGDB
        {
            get
            {
                return tboutputGDB.Text;
            }
        }

        public bool DelCollaField
        {
            get
            {
                return cbDelCollaField.Checked;
            }
        }

        public bool FieldNameUpper
        {
            get
            {
                return cbFieldNameUpper.Checked;
            }
        }

        GApplication _app;

        public ExportDataForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
            _rangeGeometry = null;
        }

        private void cbRange_CheckedChanged(object sender, EventArgs e)
        {
            btnShpFile.Enabled = cbRange.Checked;
        }

        private void btnShpFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择一个范围文件";
            dlg.AddExtension = true;
            dlg.DefaultExt = "shp";
            dlg.Filter = "选择文件|*.shp";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbShpFileName.Text = dlg.FileName;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog pDialog = new SaveFileDialog();
            pDialog.AddExtension = true;
            pDialog.DefaultExt = "gdb";
            pDialog.Filter = "文件地理数据库|*.gdb";
            pDialog.FilterIndex = 0;
            if (pDialog.ShowDialog() == DialogResult.OK)
            {
                tboutputGDB.Text = pDialog.FileName;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbIPAdress.Text) || string.IsNullOrEmpty(tbDataBase.Text) || string.IsNullOrEmpty(tbUserName.Text) || string.IsNullOrEmpty(tbPassword.Text))
            {
                MessageBox.Show("请输入数据库联接信息！");
                return;
            }

            //IWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(tbIPAdress.Text, tbUserName.Text, tbPassword.Text, tbDataBase.Text);
            //if (null == sdeWorkspace)
            //{
            //    MessageBox.Show("无法连接服务器！");
            //    return ;
            //}

            if (cbRange.Checked && string.IsNullOrEmpty(tbShpFileName.Text))
            {
                MessageBox.Show("请指定提取范围文件！");
                return;
            }

            if (cbRange.Checked)
            {
                string refName = getRangeGeometryReference(tbShpFileName.Text);
                if (string.IsNullOrEmpty(refName))
                {
                    MessageBox.Show("范围文件没有空间参考！");
                    return;
                }
            }
            

            DialogResult = DialogResult.OK;
        }


        //读取shp文件,获取范围几何体并返回空间参考名称
        private string getRangeGeometryReference(string fileName)
        {
            string refName = "";

            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(fileName), 0);
            IFeatureWorkspace pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass shapeFC = pFeatureWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(fileName));

            //是否为多边形几何体
            if (shapeFC.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("范围文件应为多边形几何体，请重新指定范围文件！");
                return refName;
            }

            //默认为第一个要素的几何体
            IFeatureCursor featureCursor = shapeFC.Search(null, false);
            IFeature pFeature = featureCursor.NextFeature();
            if (pFeature != null && pFeature.Shape is IPolygon)
            {
                _rangeGeometry = pFeature.Shape;
                refName = _rangeGeometry.SpatialReference.Name;
            }
            Marshal.ReleaseComObject(featureCursor);

            return refName;
        }
    }
}

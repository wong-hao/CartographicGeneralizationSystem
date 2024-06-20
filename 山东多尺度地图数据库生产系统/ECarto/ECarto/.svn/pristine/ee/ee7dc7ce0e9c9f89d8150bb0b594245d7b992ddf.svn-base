using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesFile;
using Microsoft.Win32;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class DCDUpdatedDataExportForm : Form
    {
        #region 属性
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
        public bool cbnextDatabase
        {
            get
            {
                return cbDatabase.Checked;
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

        public string ReferGDB
        {
            get
            {
                return tbReferGDB.Text;
            }
        }
        public string NextGDB
        {
            get
            {
                return lbNextGDB.Text;
            }
        }
        public string OutputGDB
        {
            get
            {
                return tboutputGDB.Text;
            }
        }

        private GApplication _app;
        #endregion

        public DCDUpdatedDataExportForm(GApplication app)
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

        private void btReferGDB_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            fd.Description = "选择GDB数据库";
            fd.ShowNewFolderButton = false;
            if (fd.ShowDialog() != DialogResult.OK || !fd.SelectedPath.ToLower().Trim().EndsWith(".gdb"))
                return;

            tbReferGDB.Text = fd.SelectedPath;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.DefaultExt = "gdb";
            saveDialog.Filter = "文件地理数据库|*.gdb";
            saveDialog.FilterIndex = 0;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                tboutputGDB.Text = saveDialog.FileName;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbIPAdress.Text) || string.IsNullOrEmpty(tbDataBase.Text) || string.IsNullOrEmpty(tbUserName.Text) || string.IsNullOrEmpty(tbPassword.Text))
            {
                MessageBox.Show("请输入服务器数据库连接信息！");
                return;
            }


            if (cbRange.Checked && string.IsNullOrEmpty(tbShpFileName.Text))
            {
                MessageBox.Show("请指定提取范围文件或取消勾选提取范围复选框！");
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

            if (string.IsNullOrEmpty(tbReferGDB.Text))
            {
                MessageBox.Show("请指定原始参照数据库！");
                return;
            }

            if (string.IsNullOrEmpty(tboutputGDB.Text))
            {
                MessageBox.Show("请指定输出位置！");
                return;
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
        private void btNextGDB_Click(object sender, EventArgs e)
        {
            var fd = new FolderBrowserDialog();
            fd.Description = "选择GDB数据库";
            fd.ShowNewFolderButton = false;
            if (fd.ShowDialog() != DialogResult.OK || !fd.SelectedPath.ToLower().Trim().EndsWith(".gdb"))
                return;

            lbNextGDB.Text = fd.SelectedPath;
        }

        private void cbDatabase_CheckedChanged(object sender, EventArgs e)
        {
            btNextGDB.Enabled = cbDatabase.Checked;
        }
        private void DCDUpdatedDataExportForm_Load(object sender, EventArgs e)
        {
            if (RegistryHelper.IsRegistryExist(Registry.LocalMachine, "SOFTWARE", "SMGI"))
            {
                try
                {
                    string info = RegistryHelper.GetRegistryData(Registry.LocalMachine, "SOFTWARE\\SMGI", "DownLoad");

                    string[] Params = info.Split(',');
                    if (Params.Count() < 4)
                        return;

                    tbIPAdress.Text = Params[0];
                    tbDataBase.Text = Params[1];
                    tbUserName.Text = Params[2];
                    //tbShpFileName.Text = Params[3];
                    //上一次记住了密码
                    try
                    {
                  
                            tbPassword.Text = Params[4];
                      
                    }
                    catch
                    {

                    }

                    //tbPassword.Select();
                }
                catch
                {
                }

            }
        }
    }
}

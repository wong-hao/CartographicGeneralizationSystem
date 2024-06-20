using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class DownLoadDataForm : Form
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

        public string RangeFileName
        {
            get
            {
                return tbShpFileName.Text;
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

        public bool FieldNameUpper
        {
            get
            {
                return cbFieldNameUpper.Checked;
            }
        }

        private List<string> _featureClassNameList;
        public List<string> FeatureClassNameList
        {
            get
            {
                if (!cbDownLoadSomeFC.Checked)
                    return null;


                return _featureClassNameList;
            }
        }
        public bool RemberPassword
        {
            get
            {
                return cbRemeberPassword.Checked;
            }
        }
        GApplication _app;

        public DownLoadDataForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
            _rangeGeometry = null;

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

                    //上一次记住了密码
                    try
                    {
                        if (Params[5].ToUpper() == "TRUE")
                        {
                            cbRemeberPassword.Checked = true;//上次记录了密码，默认仍然再次记住
                            tbPassword.Text = Params[4];
                        }
                        else
                        {
                            cbRemeberPassword.Checked = false;//上次没记密码，这次默认不记
                        }
                    }
                    catch
                    {

                    }

                    tbPassword.Select();
                }
                catch
                {
                }
                
            }
            
        }

        private void btnShpFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择一个范围文件";
            dlg.AddExtension = true;
            dlg.DefaultExt = "shp";
            dlg.Filter = "选择文件|*.shp";
            if (tbShpFileName.Text != "")
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(tbShpFileName.Text);
            }
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

        private void cbDownLoadSomeFC_CheckedChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbIPAdress.Text) || string.IsNullOrEmpty(tbDataBase.Text) || string.IsNullOrEmpty(tbUserName.Text) || string.IsNullOrEmpty(tbPassword.Text))
            {
                MessageBox.Show("请输入数据库联接信息！");
                return;
            }

            //清空原先的选项
            chkLayerlist.Items.Clear();

            if (cbDownLoadSomeFC.Checked)
            {
                //获取图层名列表，填充选项
                using (var wo = _app.SetBusy())
                {
                    wo.SetText("正在连接数据库...");
                    List<string> fcNames = getFeatureClassNames(_app, tbIPAdress.Text, tbUserName.Text, tbPassword.Text, tbDataBase.Text);
                    chkLayerlist.Items.AddRange(fcNames.ToArray());
                }
            }

        }

        private void btn_All_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkLayerlist.Items.Count; i++)
            {
                chkLayerlist.SetItemChecked(i, true);
            }
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < chkLayerlist.Items.Count; i++)
            {
                chkLayerlist.SetItemChecked(i, false);
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
            //    return;
            //}

            if (cbDownLoadSomeFC.Checked)
            {
                _featureClassNameList = new List<string>();
                foreach (var ln in chkLayerlist.CheckedItems)
                {
                    _featureClassNameList.Add(ln as string);
                }

                if (_featureClassNameList.Count == 0)
                {
                    MessageBox.Show("请至少指定一个需要下载的要素类！");
                    return;
                }
            }

            if (string.IsNullOrEmpty(tbShpFileName.Text))
            {
                MessageBox.Show("请指定提取范围文件！");
                return;
            }

            string refName = getRangeGeometryReference(tbShpFileName.Text);
            if (string.IsNullOrEmpty(refName))
            {
                MessageBox.Show("范围文件没有空间参考！");
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


        //获取图层名
        public static List<string> getFeatureClassNames(GApplication app, string ipAddress, string userName, string passWord, string databaseName)
        {
            List<string> fcNames = new List<string>();

            IWorkspace pWorkspace = app.GetWorkspacWithSDEConnection(ipAddress, userName, passWord, databaseName);
            if (null == pWorkspace)
            {
                MessageBox.Show("无法访问服务器！");
                return fcNames;
            }


            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            while (pDataset != null)
            {
                if (pDataset is IFeatureDataset)//要素数据集
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;
                    IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                    IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF = pEnumDatasetF.Next();
                    while (pDatasetF != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);
                            if (fc != null)
                                fcNames.Add(fc.AliasName.Split('.').Last().ToUpper());
                        }

                        pDatasetF = pEnumDatasetF.Next();
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pDataset is IFeatureClass)//要素类
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;

                    IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDataset.Name);
                    if (fc != null)
                        fcNames.Add(fc.AliasName.Split('.').Last().ToUpper());
                }
                else
                {

                }

                pDataset = pEnumDataset.Next();

            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);

            return fcNames;
        }

        private void cbShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbShowPassword.Checked)
            {
                tbPassword.PasswordChar = '●';
            }
            else
            {
                tbPassword.PasswordChar = '\0';
            }
        }

        private void cbRemeberPassword_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void DownLoadDataForm_Load(object sender, EventArgs e)
        {

        }        

    }
}

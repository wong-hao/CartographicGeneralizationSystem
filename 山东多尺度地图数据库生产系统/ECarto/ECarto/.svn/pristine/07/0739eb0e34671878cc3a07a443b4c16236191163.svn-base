using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using Microsoft.Win32;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class ExportDataBetweenVersionForm : Form
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
        public bool RemberPassword
        {
            get
            {
                return cbRemeberPassword.Checked;
            }
        }

        public int StartDBVersion
        {
            get
            {
                return int.Parse(cmbBoxStartVersion.Text);
            }
        }

        public int EndDBVersion
        {
            get
            {
                if (cbEndVersion.Checked)
                {
                    return int.Parse(cmbBoxEndVersion.Text);
                }
                else
                {
                    return -1;
                }
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

        public bool FieldNameUpper
        {
            get
            {
                return cbFieldNameUpper.Checked;
            }
        }

        private List<int> _serverDBVersionList;


        public ExportDataBetweenVersionForm()
        {
            InitializeComponent();

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

        private void DownLoadVersionDataForm_Load(object sender, EventArgs e)
        {
            
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

        private void cbEndVersion_CheckedChanged(object sender, EventArgs e)
        {
            cmbBoxEndVersion.Enabled = cbEndVersion.Checked;
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

        private void wizardExportData_NextClick(object sender, DevExpress.XtraWizard.WizardCommandButtonClickEventArgs e)
        {
            if (wizardExportDataBetweenVersion.SelectedPage.Name == "ServerDBConnPage")
            {
                using (var wo = GApplication.Application.SetBusy())
                {
                    _serverDBVersionList = new List<int>();

                    if (string.IsNullOrEmpty(tbIPAdress.Text) || string.IsNullOrEmpty(tbDataBase.Text) || string.IsNullOrEmpty(tbUserName.Text) || string.IsNullOrEmpty(tbPassword.Text))
                    {
                        MessageBox.Show("请输入数据服务器连接信息！");
                        e.Handled = true;
                        return;
                    }
                    var serverWS = GApplication.Application.GetWorkspacWithSDEConnection(IPAdress, UserName, Password, DataBase);
                    if (null == serverWS)
                    {
                        MessageBox.Show("无法访问服务器！");
                        e.Handled = true;
                        return;
                    }

                    var smgiServerStateTable = (serverWS as IFeatureWorkspace).OpenTable("SMGIServerState");
                    IQueryFilter qf = new QueryFilterClass() { WhereClause = string.Format("versionstate is null or versionstate = ''") };//排除进行了回滚操作的版本
                    if (smgiServerStateTable == null || smgiServerStateTable.RowCount(null) == 0)
                    {
                        e.Handled = true;
                        return;
                    }
                    ICursor cursor = smgiServerStateTable.Search(qf, true);
                    IRow row = null;
                    while ((row = cursor.NextRow()) != null)
                    {
                        int ver = int.Parse(row.get_Value(row.Fields.FindField("versid")).ToString());
                        //string desc = row.get_Value(row.Fields.FindField("instruction")).ToString();
                        //string opuser = row.get_Value(row.Fields.FindField("opuser")).ToString();

                        if (!_serverDBVersionList.Contains(ver))
                            _serverDBVersionList.Add(ver);

                    }
                    Marshal.ReleaseComObject(cursor);
                }
            }

            
        }

        private void wizardExportData_SelectedPageChanged(object sender, DevExpress.XtraWizard.WizardPageChangedEventArgs e)
        {
            if (wizardExportDataBetweenVersion.SelectedPage.Name == "ExportOptionPage")
            {
                cmbBoxStartVersion.Items.Clear();
                cbEndVersion.Checked = true;
                cmbBoxEndVersion.Items.Clear();

                if (_serverDBVersionList == null || _serverDBVersionList.Count == 0)
                {
                    return;
                }

                _serverDBVersionList = _serverDBVersionList.OrderByDescending(x => x).ToList();//降序

                foreach(var ver in _serverDBVersionList)
                {
                    cmbBoxStartVersion.Items.Add(ver);
                    cmbBoxEndVersion.Items.Add(ver);
                }
            }
        }

        private void wizardExportData_FinishClick(object sender, CancelEventArgs e)
        {
            int startVer = -1;
            if (!int.TryParse(cmbBoxStartVersion.Text, out startVer) || startVer < 0)
            {
                MessageBox.Show(string.Format("请指定一个合法的起始版本！"));
                e.Cancel = true;
                return;
            }
            if (cbEndVersion.Checked)
            {
                int endVer = -1;
                if (!int.TryParse(cmbBoxEndVersion.Text, out endVer))
                {
                    MessageBox.Show(string.Format("请指定一个合法的终止版本！"));
                    e.Cancel = true;
                    return;
                }
            }
            if (cbEndVersion.Checked && StartDBVersion >= EndDBVersion)
            {
                MessageBox.Show(string.Format("终止版本必须大于起始版本！"));
                e.Cancel = true;
                return;
            }

            _rangeGeometry = null;
            if (cbRange.Checked)
            {
                if(string.IsNullOrEmpty(tbShpFileName.Text))
                {
                    MessageBox.Show("请指定提取范围文件！");
                    e.Cancel = true;
                    return;
                }

                string refName = getRangeGeometryReference(tbShpFileName.Text);
                if (string.IsNullOrEmpty(refName))
                {
                    MessageBox.Show("范围文件没有空间参考！");
                    e.Cancel = true;
                    return;
                }
            }

            if (tboutputGDB.Text == "")
            {
                MessageBox.Show(string.Format("请指定导出的数据库输出位置！"));
                e.Cancel = true;
                return;
            }

            if (EndDBVersion == -1)
            {
                if (System.Windows.Forms.DialogResult.No == MessageBox.Show(string.Format("本次操作将从服务器数据库中提取版本【{0}】与最新版本之间的增量数据，是否确定?", StartDBVersion, EndDBVersion), "提示", MessageBoxButtons.YesNo))
                {
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                if (System.Windows.Forms.DialogResult.No == MessageBox.Show(string.Format("本次操作将从服务器数据库中提取版本【{0}】与版本【{1}】之间的增量数据，是否确定?", StartDBVersion, EndDBVersion), "提示", MessageBoxButtons.YesNo))
                {
                    e.Cancel = true;
                    return;
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void wizardExportData_CancelClick(object sender, CancelEventArgs e)
        {
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

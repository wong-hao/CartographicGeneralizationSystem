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
    public partial class DownLoadVersionDataForm : Form
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

        public int DBVersion
        {
            get;
            private set;
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


        private Dictionary<int, KeyValuePair<string, string>> _ver2DescandOp;//Dictionary<version, KeyValuePair<desc, opuser>>
        private int _serverDBMaxVersion;

        public DownLoadVersionDataForm()
        {
            InitializeComponent();

            _rangeGeometry = null;
            _serverDBMaxVersion = -1;

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

        private void serverDBVersionDGV_MouseClick(object sender, MouseEventArgs e)
        {
            serverDBVersionDGV.ClearSelection();

            if (serverDBVersionDGV.FirstDisplayedScrollingRowIndex < 0)
                return;

            if (serverDBVersionDGV.ColumnHeadersVisible == true && e.Y <= serverDBVersionDGV.ColumnHeadersHeight)
                return;

            int index = serverDBVersionDGV.FirstDisplayedScrollingRowIndex;
            int displayedCount = serverDBVersionDGV.DisplayedRowCount(true);
            for (int k = 1; k <= displayedCount; )
            {
                if (serverDBVersionDGV.Rows[index].Visible == true)
                {
                    Rectangle rect = serverDBVersionDGV.GetRowDisplayRectangle(index, true);
                    if (e.Y >= rect.Top && e.Y < rect.Bottom)
                    {
                        //当前鼠标选中的行
                        serverDBVersionDGV.Rows[index].Selected = true;
                    }
                    ++k;
                }

                ++index;
            }
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
            if (wizardExportData.SelectedPage.Name == "ServerDBConnPage")
            {
                using (var wo = GApplication.Application.SetBusy())
                {
                    _ver2DescandOp = new Dictionary<int, KeyValuePair<string, string>>();
                    _serverDBMaxVersion = -1;

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
                    _serverDBMaxVersion = ServerDataInitializeCommand.getServerDBMaxVersion(serverWS);
                    var smgiServerStateTable = (serverWS as IFeatureWorkspace).OpenTable("SMGIServerState");
                    IQueryFilter qf = new QueryFilterClass() { WhereClause = string.Format("instruction like  '%【内容：%】' and (versionstate is null or versionstate = '')") };//排除服务器初始化版本及进行了回滚操作的版本
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
                        string desc = row.get_Value(row.Fields.FindField("instruction")).ToString();
                        string opuser = row.get_Value(row.Fields.FindField("opuser")).ToString();

                        if (_ver2DescandOp.ContainsKey(ver))
                        {
                            MessageBox.Show(string.Format("服务器状态表中存在多个【{0}】版本！", ver));
                            e.Handled = true;
                            return;
                        }

                        _ver2DescandOp.Add(ver, new KeyValuePair<string, string>(desc, opuser));
                    }
                    Marshal.ReleaseComObject(cursor);
                }
            }

            
        }

        private void wizardExportData_SelectedPageChanged(object sender, DevExpress.XtraWizard.WizardPageChangedEventArgs e)
        {
            if (wizardExportData.SelectedPage.Name == "ExportOptionPage")
            {
                serverDBVersionDGV.Rows.Clear();

                if (_ver2DescandOp == null || _ver2DescandOp.Count == 0)
                {
                    return;
                }

                _ver2DescandOp = _ver2DescandOp.OrderByDescending(p => p.Key).ToDictionary(p => p.Key, o => o.Value);//按Key降序排列
                if (_serverDBMaxVersion > _ver2DescandOp.First().Key)
                {
                    int rowIndex = serverDBVersionDGV.Rows.Add();

                    serverDBVersionDGV.Rows[rowIndex].Cells["ServerDBVersion"].Value = _serverDBMaxVersion;
                    serverDBVersionDGV.Rows[rowIndex].Cells["SubmitDesc"].Value = "";
                    serverDBVersionDGV.Rows[rowIndex].Cells["OpName"].Value = "";
                }

                foreach (var kv in _ver2DescandOp)
                {
                    int rowIndex = serverDBVersionDGV.Rows.Add();

                    serverDBVersionDGV.Rows[rowIndex].Cells["ServerDBVersion"].Value = kv.Key;
                    serverDBVersionDGV.Rows[rowIndex].Cells["SubmitDesc"].Value = kv.Value.Key;
                    serverDBVersionDGV.Rows[rowIndex].Cells["OpName"].Value = kv.Value.Value;
                }
                
            }
        }

        private void wizardExportData_FinishClick(object sender, CancelEventArgs e)
        {
            if (serverDBVersionDGV.SelectedRows.Count == 0)
            {
                MessageBox.Show(string.Format("请指定需导出的数据库版本！"));
                e.Cancel = true;
                return;
            }
            DBVersion = int.Parse(serverDBVersionDGV.SelectedRows[0].Cells["ServerDBVersion"].Value.ToString());

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

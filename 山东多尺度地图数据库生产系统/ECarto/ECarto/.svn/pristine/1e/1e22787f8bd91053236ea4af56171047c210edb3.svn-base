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
    public partial class ServerDBRestoreForm : Form
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

        private Dictionary<int, KeyValuePair<string, string>> _ver2DescandOp;//Dictionary<version, KeyValuePair<desc, opuser>>

        public ServerDBRestoreForm()
        {
            InitializeComponent();

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

        private void ServerDBRestoreForm_Load(object sender, EventArgs e)
        {
            if (this.Text == "服务器数据库版本删除")
            {
                this.VesionInfoPage.Text = this.Text;
                this.VesionInfoPage.DescriptionText = "从服务器数据库中删除用户指定版本的所有要素";
            }
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

        private void wizardExportData_NextClick(object sender, DevExpress.XtraWizard.WizardCommandButtonClickEventArgs e)
        {
            if (wizardDBRestroe.SelectedPage.Name == "ServerDBConnPage")
            {
                using (var wo = GApplication.Application.SetBusy())
                {
                    _ver2DescandOp = new Dictionary<int, KeyValuePair<string, string>>();

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
            if (wizardDBRestroe.SelectedPage.Name == "VesionInfoPage")
            {
                serverDBVersionDGV.Rows.Clear();

                if (_ver2DescandOp == null || _ver2DescandOp.Count == 0)
                {
                    return;
                }

                _ver2DescandOp = _ver2DescandOp.OrderByDescending(p => p.Key).ToDictionary(p => p.Key, o => o.Value);//按Key降序排列

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
                MessageBox.Show(string.Format("请指定一个服务器数据库版本！"));
                e.Cancel = true;
                return;
            }
            DBVersion = int.Parse(serverDBVersionDGV.SelectedRows[0].Cells["ServerDBVersion"].Value.ToString());

            if (this.Text == "服务器数据库版本删除")
            {
                if (System.Windows.Forms.DialogResult.No == MessageBox.Show(string.Format("本次操作将直接删除服务器数据库中所有版本号为【{0}】的要素，是否确定?", DBVersion), "提示", MessageBoxButtons.YesNo))
                {
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                if (System.Windows.Forms.DialogResult.No == MessageBox.Show(string.Format("本次操作将使服务器数据库回滚至版本【{0}】，即将直接删除服务器数据库中版本号大于【{0}】的要素，是否确定?", DBVersion), "提示", MessageBoxButtons.YesNo))
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

        
    }
}

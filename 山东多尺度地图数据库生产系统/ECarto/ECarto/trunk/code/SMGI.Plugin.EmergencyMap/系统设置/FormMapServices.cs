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
namespace SMGI.Plugin.EmergencyMap.DataSource
{
    public partial class MapServicesForm : Form
    {
        ListView listView;
        FormType type;
        public MapServicesForm(ListView listView,FormType type)
        {
            InitializeComponent();
            this.listView = listView;
            this.type = type;

            if (type == FormType.Edit)
            {
                ListViewItem LVItem = listView.SelectedItems[0];
                tbURL.Text = LVItem.SubItems[1].Text.ToString();
                this.poiDb.Text= LVItem.SubItems[2].Text.ToString();
                this.poiIP.Text = LVItem.SubItems[3].Text.ToString();
                this.poiUser.Text = LVItem.SubItems[4].Text.ToString();
                this.poiPassword.Text = LVItem.SubItems[5].Text.ToString();
                tbServerName.Text = LVItem.SubItems[6].Text.ToString();
                tbDes.Text = LVItem.SubItems[7].Text.ToString();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string url = tbURL.Text.Trim();
            string serverName = tbServerName.Text.Trim();
            string des = tbDes.Text.Trim();
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(des))
            {
                MessageBox.Show("底图服务信息填写不全！");
                return;
            }
            if (!TestSDE())
            {
              return;
            }
            if (type == FormType.Add)
            {
                ListViewItem LVItem = new ListViewItem((listView.Items.Count+1).ToString());
                LVItem.SubItems.AddRange(new string[] { url, this.poiDb.Text, this.poiIP.Text, this.poiUser.Text, this.poiPassword.Text,serverName, des });
                listView.Items.Add(LVItem);
                LVItem.EnsureVisible();
            }
            else if (type == FormType.Edit)
            {
                ListViewItem LVItem = listView.SelectedItems[0];
                LVItem.SubItems[1].Text = url;
                LVItem.SubItems[2].Text=poiDb.Text;
                LVItem.SubItems[3].Text = this.poiIP.Text;
                LVItem.SubItems[4].Text = this.poiUser.Text;
                LVItem.SubItems[5].Text = this.poiPassword.Text;
                LVItem.SubItems[6].Text = tbServerName.Text;
                LVItem.SubItems[7].Text = tbDes.Text;
                LVItem.EnsureVisible();
            }
            DialogResult = DialogResult.OK;
        }

        private void MapServicesForm_Load(object sender, EventArgs e)
        {

        }

        private void btTest_Click(object sender, EventArgs e)
        {
            if (TestSDE())
            {
                MessageBox.Show("连接服务器成功！");
            }
        }
        private bool TestSDE()
        {
            string ip, userName, password, dbName;
            IWorkspace sdeWorkspace = null;
           
            //获取相应服务器、图层信息进行连接验证
            try
            {

                dbName = poiDb.Text;
                ip = poiIP.Text;
                userName =poiUser.Text;
                password = poiPassword.Text;
                if (dbName == string.Empty || ip == string.Empty || userName == string.Empty || password == string.Empty)
                {
                    MessageBox.Show(string.Format("不能为空！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]", ip, userName, password, dbName));
                    return false;
                }
                sdeWorkspace =GApplication.Application.GetWorkspacWithSDEConnection(ip, userName, password, dbName);
                if (sdeWorkspace == null)
                {
                    MessageBox.Show(string.Format("无法访问服务器！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]", ip, userName, password, dbName));
                    return false;
                }

            }
            catch
            {
                MessageBox.Show(string.Format("无法访问服务器"));
                return false;
            }
            return true;
            
        }
       
    }
}

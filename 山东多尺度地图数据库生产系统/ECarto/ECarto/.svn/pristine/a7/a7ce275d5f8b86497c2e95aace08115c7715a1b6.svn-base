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
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;
using System.IO;
using System.Xml.Linq;

namespace SMGI.Plugin.CollaborativeWork
{
    
    public partial class SDECommonConnectionForm : Form
    {
        public SDECommonConnectionForm()
        {
            InitializeComponent();
        }

        private void SdeConnection_Load(object sender, EventArgs e)
        {

        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbIPAdress.Text))
            {
                MessageBox.Show("请指定连接地址");
                return;
            }

            if (string.IsNullOrEmpty(tbPassword.Text))
            {
                MessageBox.Show("请输入sde密码");
                return;
            }

            if (string.IsNullOrEmpty(cmbDataBaseList.Text))
            {
                MessageBox.Show("请指定连接的数据库名");
                return;
            }

            var wsServer = GApplication.Application.GetWorkspacWithSDEConnection(this.tbIPAdress.Text.Trim(), this.tbUserName.Text.Trim(), this.tbPassword.Text.Trim(), this.cmbDataBaseList.Text.ToString().Trim());
            if (wsServer == null)
            {
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btServerDCD_Click(object sender, EventArgs e)
        {
            GApplication.Application.GetWorkspacWithSDEConnection(this.tbIPAdress.Text.Trim(), this.tbUserName.Text.Trim(), this.tbPassword.Text.Trim(), this.cmbDataBaseList.Text.ToString().Trim());
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using SMGI.Common;
using System.IO;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GISClient;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.EmergencyMap.DataSource
{
    public partial class DEMData : Form
    {
        GApplication app;
        public DEMData(GApplication _app)
        {
            app = _app;
            InitializeComponent();
        }

        private void DEMData_Load(object sender, EventArgs e)
        {
            this.btServerDEM.Tag = this.gbDEM;
            LoadDEM();
        }

        private void btnValidServer(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            GroupBox gb = currentBtn.Tag as GroupBox;
            string ip, userName, password, dbName, layer;
            IWorkspace sdeWorkspace = null;
            //获取相应服务器、图层信息进行连接验证
            if (gb.Name=="gbDEM")
            {
                ip = demIP.Text;
                userName = demUser.Text;
                password = demPassword.Text;
                dbName = demDb.Text;
                layer = demLayer.Text;
                sdeWorkspace = app.GetWorkspacWithSDEConnection(ip, userName, password, dbName);
                if (sdeWorkspace == null)
                {
                    MessageBox.Show(string.Format("无法访问DEM服务器！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]信息", ip, userName, password, dbName));
                    return;
                }
                if (!(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTRasterDataset, layer))
                {
                    MessageBox.Show(string.Format("无法访问DEM服务器中的栅格数据集！请确认栅格数据集[{0}]信息"), layer);
                    return;
                }
            }
            MessageBox.Show("验证通过！");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            #region 验证输入合法性
            foreach (Control gb in this.Controls)
            {
                if (gb is GroupBox)
                {
                    GroupBox currentGB = gb as GroupBox;
                    foreach (Control c in currentGB.Controls)
                    {
                        if (c is TextBox)
                        {
                            TextBox currentTB = c as TextBox;
                            if (string.IsNullOrEmpty(currentTB.Text))
                            {
                                MessageBox.Show(string.Format("{0}的信息不能有空值！", currentGB.Text));
                                return;
                            }
                        }
                        else if (c is ListView)
                        {
                            ListView lv = c as ListView;
                            if (lv.Items.Count == 0)
                            {
                                MessageBox.Show(string.Format("{0}的列表内容不能为空！", currentGB.Text));
                                return;
                            }
                        }
                    }
                }
            }
            #endregion
            #region 更改xml参数配置
            //DEM服务
            ChangeDEM();
            #endregion
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        /// <summary>
        /// 加载DEM信息
        /// </summary>
        private void LoadDEM()
        {
            string fileName = app.Template.Root + @"\专家库\DEM\DEMMapServer.xml";
            if (File.Exists(fileName))
            {

               
                XDocument doc = XDocument.Load(fileName);
                
                {
                    
                    XElement item = doc.Root.Element("Content").Element("Server");
                    if (item != null)
                    {
                        demIP.Text = item.Element("IPAddress").Value;
                        demUser.Text = item.Element("UserName").Value;
                        demPassword.Text = item.Element("Password").Value;
                        demDb.Text = item.Element("DataBase").Value;
                        demLayer.Text = item.Element("DEMName").Value;
                    }

                }

            }
        }
        /// <summary>
        /// 更改DEM信息
        /// </summary>
        private void ChangeDEM()
        {
            string fileName = app.Template.Root + @"\专家库\DEM\DEMMapServer.xml";
            if (File.Exists(fileName))
            {

                FileInfo f = new FileInfo(fileName);
                XDocument doc = XDocument.Load(fileName);
               
                {
                   
                    XElement item = doc.Root.Element("Content").Element("Server");
                    if (item != null)
                    {
                        item.Element("IPAddress").Value = demIP.Text.Trim();
                        item.Element("UserName").Value = demUser.Text.Trim();
                        item.Element("Password").Value = demPassword.Text.Trim();
                        item.Element("DataBase").Value = demDb.Text.Trim();
                        item.Element("DEMName").Value = demLayer.Text.Trim();
                    }
                }
                doc.Save(fileName);
            }
        }




    }
}

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
    public partial class LocationData : Form
    {
        GApplication app;
        public LocationData(GApplication _app)
        {
            app = _app;
            InitializeComponent();
        }

        private void LocationData_Load(object sender, EventArgs e)
        {
            this.btServerNamePos.Tag = this.gbNamePos;
            this.btServerRegionPos.Tag = this.gbRegionPos;
            LoadLocation();
        }

        private void btnValidServer(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            GroupBox gb = currentBtn.Tag as GroupBox;
            string ip, userName, password, dbName, layer;
            IWorkspace sdeWorkspace = null;
            //获取相应服务器、图层信息进行连接验证
            switch (gb.Name)
            {
                case "gbNamePos":
                    ip = poiIP.Text;
                    userName = poiUser.Text;
                    password = poiPassword.Text;
                    dbName = poiDb.Text;
                    layer = poiLayer.Text;
                    sdeWorkspace = app.GetWorkspacWithSDEConnection(ip, userName, password, dbName);
                    if (sdeWorkspace == null)
                    {
                        MessageBox.Show(string.Format("无法访问地名服务器！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]信息", ip, userName, password, dbName));
                        return;
                    }
                    if (!(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, layer))
                    {
                        MessageBox.Show(string.Format("无法访问地名服务器中的要素类！请确认要素类[{0}]信息", layer));
                        return;
                    }
                    break;
                case "gbRegionPos":
                    ip = regionIP.Text;
                    userName = regionUser.Text;
                    password = regionPassword.Text;
                    dbName = regionDb.Text;
                    sdeWorkspace = app.GetWorkspacWithSDEConnection(ip, userName, password, dbName);
                    if (sdeWorkspace == null)
                    {
                        MessageBox.Show(string.Format("无法访问服务器！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]信息", ip, userName, password, dbName));
                        return;
                    }
                    break;
            }
            MessageBox.Show("验证通过！");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
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
            //定位服务
            ChangeLocation();
            #endregion
            DialogResult = DialogResult.OK;
        }
        /// <summary>
        /// 加载定位信息
        /// </summary>
        private void LoadLocation()
        {
            string fileName = app.Template.Root + @"\专家库\数据定位\LocationSearch.xml";
            if (File.Exists(fileName))
            {


                XDocument doc = XDocument.Load(fileName); ;
              
                {
                    
                    var items = doc.Root.Element("Content").Elements("Server");

                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            string attr = item.Attribute("name").Value;
                            if (attr == "地名定位")
                            {
                                poiIP.Text = item.Element("IPAddress").Value;
                                poiUser.Text = item.Element("UserName").Value;
                                poiPassword.Text = item.Element("Password").Value;
                                poiDb.Text = item.Element("DataBase").Value;
                                poiLayer.Text = item.Element("Lyr").Value;
                            }
                            if (attr == "行政区定位")
                            {
                                regionIP.Text = item.Element("IPAddress").Value;
                                regionUser.Text = item.Element("UserName").Value;
                                regionPassword.Text = item.Element("Password").Value;
                                regionDb.Text = item.Element("DataBase").Value;
                            }
                        }
                    }

                }


            }
        }

        /// <summary>
        /// 更改定位信息
        /// </summary>
        private void ChangeLocation()
        {
            string fileName = app.Template.Root + @"\专家库\数据定位\LocationSearch.xml";
            if (File.Exists(fileName))
            {
                try
                {
                   
                    XDocument doc = XDocument.Load(fileName);
                  
                    {
                       
                        var servers = doc.Root.Element("Content").Elements("Server");
                        if (servers != null)
                        {
                            foreach (var server in servers)
                            {
                                string attr = server.Attribute("name").Value;
                                if (attr == "地名定位")
                                {
                                    server.Element("IPAddress").Value = poiIP.Text.Trim();
                                    server.Element("UserName").Value = poiUser.Text.Trim();
                                    server.Element("Password").Value = poiPassword.Text.Trim();
                                    server.Element("DataBase").Value = poiDb.Text.Trim();
                                    server.Element("Lyr").Value = poiLayer.Text.Trim();
                                }
                                else if (attr == "行政区定位")
                                {
                                    server.Element("IPAddress").Value = regionIP.Text.Trim();
                                    server.Element("UserName").Value = regionUser.Text.Trim();
                                    server.Element("Password").Value = regionPassword.Text.Trim();
                                    server.Element("DataBase").Value = regionDb.Text.Trim();
                                }
                            }
                        }
                    }
                    doc.Save(fileName);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


    }
}

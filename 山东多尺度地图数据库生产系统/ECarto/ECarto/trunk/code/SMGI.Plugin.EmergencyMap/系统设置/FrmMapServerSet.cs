using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using SMGI.Common;
using SMGI.Plugin.EmergencyMap.DataSource;
using ESRI.ArcGIS.GISClient;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Plugin.EmergencyMap.SysSet;
namespace SMGI.Plugin.EmergencyMap
{   
   
    public partial class FrmMapServerSet : Form
    {
        GApplication app;
        public FrmMapServerSet()
        {
            InitializeComponent();
            app = GApplication.Application;
        }
        void ChangeStyle(Control.ControlCollection controls)
        {
            if (controls.Count == 0)
                return;
            foreach (Control con in controls)
            {
                if (con is SimpleButton)
                {
                    (con as SimpleButton).LookAndFeel.SetSkinStyle("DevExpress Style");
                }
                else
                {
                    ChangeStyle(con.Controls);
                }
            }
        }
        private void FrmMapServerSet_Load(object sender, EventArgs e)
        {

            navBarControl1.LookAndFeel.SetSkinStyle("DevExpress Style");
            ChangeStyle(this.Controls);
            LoadDataBase();
            LoadMapService();
            btServerNamePos.Tag = gbNamePos;
            btServerRegionPos.Tag = gbRegionPos;
            LoadLocation();
            LoadDEM();
            LoadThematic();
        }
        #region
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
        private void btServerDEM_Click(object sender, EventArgs e)
        {

            string ip, userName, password, dbName, layer;
            IWorkspace sdeWorkspace = null;
            try
            //获取相应服务器、图层信息进行连接验证
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
                    MessageBox.Show(string.Format("无法访问DEM服务器中的栅格数据集！请确认栅格数据集[{0}]信息", layer));
                    return;
                }
                MessageBox.Show("验证通过！");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("验证失败:"+ex.Message);
            }
            
        }
        #endregion
        #region 多尺度数据信息
        /// <summary>
        /// 加载多尺度数据信息
        /// </summary>
        private void LoadDataBase()
        {
            string fileName = app.Template.Root + @"\EnvironmentSettings.xml";
            if (File.Exists(fileName))
            {
                #region
                try
                {

                    XDocument doc = XDocument.Load(fileName);

                    {

                        var server = doc.Root.Element("Content").Element("Server");
                        tbMainIP.Text = server.Element("IPAddress").Value;
                        tbMainUserUame.Text = server.Element("UserName").Value;
                        tbMainPassword.Text = server.Element("Password").Value;
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                #endregion
            }

            lvDataBase.Columns.Add(new ColumnHeader { Text = "序号", Name = "id" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "基本比例尺", Name = "Scale" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "最大比例尺", Name = "Max" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "最小比例尺", Name = "Min" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "数据库", Name = "DatabaseName" });
            lvDataBase.Columns.Add(new ColumnHeader { Text = "模板", Name = "MapTemplate" });
            lvDataBase.Columns[0].Width = 50;
            lvDataBase.Columns[1].Width = 100;
            lvDataBase.Columns[2].Width = 100;
            lvDataBase.Columns[3].Width = 100;
            lvDataBase.Columns[4].Width = 80;
            lvDataBase.Columns[5].Width = 70;

            fileName = app.Template.Root + @"\专家库\Expertise.xml";
            if (File.Exists(fileName))
            {
                try
                {

                    XDocument doc = XDocument.Load(fileName);

                    {

                        XElement mapService = doc.Root.Element("Content").Element("MapScaleRule");
                        int ct = 1;
                        foreach (var item in mapService.Elements("Item"))
                        {

                            string Scale = item.Element("Scale").Value;
                            string Max = item.Element("Max").Value;
                            string Min = item.Element("Min").Value;
                            string DatabaseName = item.Element("DatabaseName").Value;
                            string MapTemplate = item.Element("MapTemplate").Value;
                            ListViewItem lvi = new ListViewItem(ct.ToString());
                            ct++;
                            lvi.SubItems.AddRange(new string[] { Scale, Max, Min, DatabaseName, MapTemplate });
                            lvDataBase.Items.Add(lvi);
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        /// <summary>
        /// 更改多尺度数据信息
        /// </summary>
        private void ChangeDataBase()
        {
            string fileName = app.Template.Root + @"\EnvironmentSettings.xml";
            if (File.Exists(fileName))
            {
                try
                {

                    XDocument doc = XDocument.Load(fileName);

                    {

                        var server = doc.Root.Element("Content").Element("Server");
                        server.Element("IPAddress").Value = tbMainIP.Text.Trim();
                        server.Element("UserName").Value = tbMainUserUame.Text.Trim();
                        server.Element("Password").Value = tbMainPassword.Text.Trim();
                    }
                    doc.Save(fileName);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            fileName = app.Template.Root + @"\专家库\Expertise.xml";
            if (File.Exists(fileName))
            {
                try
                {

                    XDocument doc = XDocument.Load(fileName);

                    {

                        XElement mapScaleRule = doc.Root.Element("Content").Element("MapScaleRule");
                        mapScaleRule.RemoveNodes();
                        foreach (ListViewItem LVItem in lvDataBase.Items)
                        {
                            string scale = LVItem.SubItems[1].Text;
                            string maxScale = LVItem.SubItems[2].Text;
                            string minScale = LVItem.SubItems[3].Text;
                            string databaseName = LVItem.SubItems[4].Text;
                            string template = LVItem.SubItems[5].Text;
                            XElement item = new XElement("Item");
                            XElement scaleNode = new XElement("Scale", scale);
                            item.Add(scaleNode);
                            XElement maxScaleNode = new XElement("Max", maxScale);
                            item.Add(maxScaleNode);
                            XElement minScaleNode = new XElement("Min", minScale);
                            item.Add(minScaleNode);
                            XElement databaseNameNode = new XElement("DatabaseName", databaseName);
                            item.Add(databaseNameNode);
                            XElement templateNode = new XElement("MapTemplate", template);
                            item.Add(templateNode);
                            mapScaleRule.Add(item);
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

        private void btAddDCD_Click(object sender, EventArgs e)
        {  
          
            ListView currentListView =  lvDataBase;
            var form = new DataBaseForm(currentListView, FormType.Add);
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
           
           
        

        }

        private void btDeleteDCD_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvDataBase.SelectedItems)
            {
                lvDataBase.Items.Remove(item);
            }
        }

        private void btEditDCD_Click(object sender, EventArgs e)
        {
            ListView currentListView = lvDataBase;
            if (currentListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一行进行编辑！");
                return;
            }
            Form form = null;
            if (currentListView.Name == "lvDataBase")
            {
                form = new DataBaseForm(currentListView, FormType.Edit);
            }
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }
       

        private void btServerDCD_Click(object sender, EventArgs e)
        {
            string ip, userName, password, dbName, layer;
            ip = tbMainIP.Text;
            userName = tbMainUserUame.Text;
            password = tbMainPassword.Text;
            try
            {
                foreach (ListViewItem item in lvDataBase.Items)
                {
                    dbName = item.SubItems[4].Text;
                    var sdeWorkspace = app.GetWorkspacWithSDEConnection(ip, userName, password, dbName);
                    if (sdeWorkspace == null)
                    {
                        MessageBox.Show(string.Format("无法访问多尺度服务器！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]", ip, userName, password, dbName));
                        return;
                    }
                }
                MessageBox.Show("连接服务器成功！");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("连接服务器失败："+ex.Message);

            }
        }
        #endregion

        #region 地图服务
        /// <summary>
        /// 加载底图服务信息
        /// </summary>
        private void LoadMapService()
        {
            lvMapService.Columns.Add(new ColumnHeader { Text = "序号", Name = "id" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "服务类型", Name = "type" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "数据库名称", Name = "name" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "服务器IP", Name = "ip" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "用户名", Name = "user" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "密码", Name = "password" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "服务名称", Name = "urlname" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "描述", Name = "des" });
            lvMapService.Columns[0].Width = 60;
            lvMapService.Columns[1].Width = 80;
            lvMapService.Columns[2].Width = 80;
            lvMapService.Columns[3].Width = 100;
            lvMapService.Columns[5].Width = 80;
            lvMapService.Columns[6].Width = 80;
            lvMapService.Columns[7].Width = 100;
            string fileName = app.Template.Root + @"\专家库\地图服务\BaseMap.xml";
            if (File.Exists(fileName))
            {
                try
                {

                    XDocument doc = XDocument.Load(fileName);

                    {

                        XElement mapService = doc.Root.Element("Content").Element("MapService");
                        int ct = 1;
                        foreach (var item in mapService.Elements("Item"))
                        {
                            string[] list = new string[] { "", "", "", "", "", "", "", "" };
                            string mapServiceUrl = item.Element("MapServiceUrl").Attribute("type").Value;
                            list[0] = mapServiceUrl;
                            
                            if (item.Element("MapServiceUrl").Attribute("type").Value == "Server")
                            {
                                if (item.Element("MapServiceUrl").Element("DataBase") != null)
                                {
                                    list[1] = item.Element("MapServiceUrl").Element("DataBase").Value;
                                }
                                if (item.Element("MapServiceUrl").Element("IPAddress") != null)
                                {
                                    list[2] = item.Element("MapServiceUrl").Element("IPAddress").Value;
                                }
                                if (item.Element("MapServiceUrl").Element("UserName") != null)
                                {
                                    list[3] = item.Element("MapServiceUrl").Element("UserName").Value;
                                }
                                if (item.Element("MapServiceUrl").Element("Password") != null)
                                {
                                    list[4] = item.Element("MapServiceUrl").Element("Password").Value;
                                }
                            }
                            string mapServiceName = item.Element("MapServiceName").Value;
                            string mapServiceDes = item.Element("MapServiceDes").Value;
                            list[5] = mapServiceName;
                            list[6] = mapServiceDes;
                            ListViewItem lvi = new ListViewItem(ct.ToString());
                            ct++;
                            lvi.SubItems.AddRange(list);
                            lvMapService.Items.Add(lvi);
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        /// <summary>
        /// 连接底图服务
        /// </summary>
        /// <param name="pHostOrUrl"></param>
        /// <param name="pServiceName"></param>
        /// <param name="pIsLAN"></param>
        /// <returns></returns>
        private IAGSServerObjectName GetMapServer(string pHostOrUrl, string pServiceName, bool pIsLAN)
        {
            IPropertySet pPropertySet = new PropertySetClass();
            IAGSServerConnectionFactory pFactory = new AGSServerConnectionFactory();
            if (pIsLAN)
                pPropertySet.SetProperty("machine", pHostOrUrl);
            else
                pPropertySet.SetProperty("url", pHostOrUrl);

            IAGSServerConnection pConnection = pFactory.Open(pPropertySet, 0);

            //Get the image server.
            IAGSEnumServerObjectName pServerObjectNames = pConnection.ServerObjectNames;
            pServerObjectNames.Reset();
            IAGSServerObjectName ServerObjectName = pServerObjectNames.Next();
            while (ServerObjectName != null)
            {
                if ((ServerObjectName.Name.ToLower() == pServiceName.ToLower()) &&
                    (ServerObjectName.Type == "MapServer"))
                {
                    break;
                }
                ServerObjectName = pServerObjectNames.Next();
            }

            //返回对象
            return ServerObjectName;
        }

        /// <summary>
        /// 更改底图服务信息
        /// </summary>
        private void ChangeMapServices()
        {
            string fileName = app.Template.Root + @"\专家库\地图服务\BaseMap.xml";
            if (File.Exists(fileName))
            {
                try
                {

                    XDocument doc = XDocument.Load(fileName);

                    {

                        XElement mapService = doc.Root.Element("Content").Element("MapService");
                        mapService.RemoveNodes();
                        foreach (ListViewItem LVItem in lvMapService.Items)
                        {
                            string mapServiceUrl = LVItem.SubItems[1].Text;
                            string mapServiceName = LVItem.SubItems[6].Text;
                            string mapServiceDes = LVItem.SubItems[7].Text;
                            XElement item = new XElement("Item");
                            XElement urlNode = new XElement("MapServiceUrl");
                            urlNode.SetAttributeValue("type", mapServiceUrl);
                            urlNode.Add(new XElement("DataBase", LVItem.SubItems[2].Text));
                            urlNode.Add(new XElement("IPAddress", LVItem.SubItems[3].Text));
                            urlNode.Add(new XElement("UserName", LVItem.SubItems[4].Text));
                            urlNode.Add(new XElement("Password", LVItem.SubItems[5].Text));
                            item.Add(urlNode);
                            XElement nameNode = new XElement("MapServiceName", mapServiceName);
                            item.Add(nameNode);
                            XElement desNode = new XElement("MapServiceDes", mapServiceDes);
                            item.Add(desNode);
                            mapService.Add(item);
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
        private void btAddWMS_Click(object sender, EventArgs e)
        {
            ListView currentListView = lvMapService;
            Form form = null;
            if (currentListView.Name == "lvMapService")
            {
                form = new MapServicesForm(currentListView, FormType.Add);
            }
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }
        private void btDeleteWMS_Click(object sender, EventArgs e)
        {
            ListView currentListView = lvMapService as ListView;
            foreach (ListViewItem item in currentListView.SelectedItems)
            {
                currentListView.Items.Remove(item);
            }
          
        }
        private void btEditWMS_Click(object sender, EventArgs e)
        {
            ListView currentListView = lvMapService;
            if (currentListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一行进行编辑！");
                return;
            }
            Form form = null;
            if (currentListView.Name == "lvMapService")
            {
                form = new MapServicesForm(currentListView, FormType.Edit);
            }
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }
        private void btServerWMS_Click(object sender, EventArgs e)
        {
            try
            {
                string ip, userName, password, dbName;
                IWorkspace sdeWorkspace = null;
               
                foreach (ListViewItem item in lvMapService.Items)
                {
                
                    //获取相应服务器、图层信息进行连接验证
                    try
                    {

                        dbName = item.SubItems[2].Text;
                        ip = item.SubItems[3].Text;
                        userName = item.SubItems[4].Text;
                        password = item.SubItems[5].Text;
                        sdeWorkspace = app.GetWorkspacWithSDEConnection(ip, userName, password, dbName);
                        if (sdeWorkspace == null)
                        {
                            MessageBox.Show(string.Format("无法访问专题服务器！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]", ip, userName, password, dbName));
                            return;
                        }

                    }
                    catch
                    {
                        MessageBox.Show(string.Format("无法访问专题服务器"));
                        return;
                    }
                  
                }
                MessageBox.Show("服务连接成功！");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(string.Format("地图服务[{0}]连接失败", ex.Message));
                return;
            }
        }
        #endregion

        #region 地图定位
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
            MessageBox.Show("连接服务成功！");
        }
        #endregion



        #region 专题数据

        /// <summary>
        /// 加载专题数据信息
        /// </summary>
        private void LoadThematic()
        {


            lvThematic.Columns.Add(new ColumnHeader { Text = "序号", Name = "id" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "专题名称", Name = "thematicName" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "IP", Name = "ip" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "用户名", Name = "userName" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "密码", Name = "password" });
            lvThematic.Columns.Add(new ColumnHeader { Text = "数据库", Name = "database" });

            lvThematic.Columns[0].Width = 50;
            lvThematic.Columns[1].Width = 100;
            lvThematic.Columns[2].Width = 100;
            lvThematic.Columns[3].Width = 100;
            lvThematic.Columns[4].Width = 80;
            lvThematic.Columns[5].Width = 70;
            string fileName = app.Template.Root + @"\专家库\ThematicMapRule.xml";
            if (File.Exists(fileName))
            {
                try
                {

                    XDocument doc = XDocument.Load(fileName);

                    {

                        XElement mapService = doc.Root.Element("Content");
                        int ct = 1;
                        foreach (var item in mapService.Elements("ThematicData"))
                        {
                            string name = item.Attribute("name").Value;
                            string IPAddress = item.Element("IPAddress").Value;
                            string UserName = item.Element("UserName").Value;
                            string Password = item.Element("Password").Value;
                            string DataBase = item.Element("DataBase").Value;


                            ListViewItem lvi = new ListViewItem(ct.ToString());
                            ct++;
                            lvi.SubItems.AddRange(new string[] { name, IPAddress, UserName, Password, DataBase });
                            lvThematic.Items.Add(lvi);
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 更改专题数据信息
        /// </summary>
        private void ChangeThematic()
        {
            string fileName = app.Template.Root + @"\专家库\ThematicMapRule.xml";
            if (File.Exists(fileName))
            {
                try
                {

                    XDocument doc = XDocument.Load(fileName);

                    {

                        var content = doc.Root.Element("Content");
                        content.RemoveNodes();
                        foreach (ListViewItem LVItem in lvThematic.Items)
                        {
                            string name = LVItem.SubItems[1].Text;
                            string IPAddress = LVItem.SubItems[2].Text;
                            string UserName = LVItem.SubItems[3].Text;
                            string Password = LVItem.SubItems[4].Text;
                            string DataBase = LVItem.SubItems[5].Text;
                            XElement ThematicData = new XElement("ThematicData");
                            ThematicData.SetAttributeValue("name", name);
                            XElement IPAddressNode = new XElement("IPAddress", IPAddress);
                            ThematicData.Add(IPAddressNode);
                            XElement UserNameNode = new XElement("UserName", UserName);
                            ThematicData.Add(UserNameNode);
                            XElement PasswordNode = new XElement("Password", Password);
                            ThematicData.Add(PasswordNode);
                            XElement DataBaseNode = new XElement("DataBase", DataBase);
                            ThematicData.Add(DataBaseNode);
                            content.Add(ThematicData);
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
        private void btAddThematic_Click(object sender, EventArgs e)
        {
            ListView currentListView = lvThematic as ListView;
            Form form = null;
            if (currentListView.Name == "lvThematic")
            {
                form = new ThematicForm(currentListView, FormType.Add);
            }
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }

        private void btDeleteThematic_Click(object sender, EventArgs e)
        {
            ListView currentListView = lvThematic as ListView;
            foreach (ListViewItem item in currentListView.SelectedItems)
            {
                currentListView.Items.Remove(item);
            }
        }

        private void btEditThematic_Click(object sender, EventArgs e)
        {
            ListView currentListView = lvThematic as ListView;
            if (currentListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一行进行编辑！");
                return;
            }
            Form form = null;
            if (currentListView.Name == "lvThematic")
            {
                form = new ThematicForm(currentListView, FormType.Edit);
            }
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();

        }

        private void btServerThematic_Click(object sender, EventArgs e)
        {
           
            string ip, userName, password, dbName;
            IWorkspace sdeWorkspace = null;
            //获取相应服务器、图层信息进行连接验证
            {
                foreach (ListViewItem item in lvThematic.Items)
                {
                    string name = item.SubItems[1].Text;
                    ip = item.SubItems[2].Text;
                    userName = item.SubItems[3].Text;
                    password = item.SubItems[4].Text;
                    dbName = item.SubItems[5].Text;
                    sdeWorkspace = app.GetWorkspacWithSDEConnection(ip, userName, password, dbName);
                    if (sdeWorkspace == null)
                    {
                        MessageBox.Show(string.Format("无法访问专题服务器！请确认ip[{0}]、用户名[{1}]、密码[{2}]、数据库[{3}]", ip, userName, password, dbName));
                        return;
                    }
                }
            }
            MessageBox.Show("验证通过！");
        }
        #endregion
        private void btnOK_Click(object sender, EventArgs e)
        {
            string msg = "验证输入合法性";
            try
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
                msg = "保存多尺度服务参数";
                //多尺度服务
                ChangeDataBase();
                msg = "保存底图服务参数";
                //底图服务
                ChangeMapServices();
                msg = "保存定位服务参数";
                //定位服务
                ChangeLocation();
                msg = "保存DEM服务参数";
                //dem
                ChangeDEM();
                msg = "保存专题服务参数";
                //专题服务
                ChangeThematic();
                #endregion
                MessageBox.Show("保存成功！");
                this.Close();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(msg+"出错:"+ex.Message);
            }
        }

        private void btIPEdit_Click(object sender, EventArgs e)
        {
            FrmIPSet frm = new FrmIPSet(tbMainIP.Text, tbMainUserUame.Text, tbMainPassword.Text);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                //1
                tbMainIP.Text = frm.IP;
                tbMainPassword.Text = frm.Password;
                tbMainUserUame.Text = frm.User;
                //2.地图服务
                foreach (ListViewItem item in lvMapService.Items)
                {
                    string url = item.SubItems[3].Text;
                    
                    
                    item.SubItems[3].Text = frm.IP;
                    item.SubItems[4].Text = frm.User;
                    item.SubItems[5].Text = frm.Password;
                }
                //3.定位服务
                poiIP.Text = frm.IP;
                poiUser.Text = frm.User;
                poiPassword.Text = frm.Password;
                regionIP.Text = frm.IP;
                regionUser.Text = frm.User;
                regionPassword.Text = frm.Password;
                //4.dem
                demIP.Text = frm.IP;
                demUser.Text = frm.User;
                demPassword.Text = frm.Password;
                //5.专题数据库
                foreach (ListViewItem item in lvThematic.Items)
                {
                    item.SubItems[2].Text = frm.IP;
                    item.SubItems[3].Text = frm.User;
                    item.SubItems[4].Text = frm.Password;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            string fileName = app.Template.Root + @"\专家库\MapServerSet.xml";
            SaveFileDialog sf = new SaveFileDialog();
            sf.Title = "地图服务信息保存";
            sf.InitialDirectory = app.Template.Root + @"\专家库";
            sf.FileName = "MapServerSet.xml";
            sf.Filter = "XML|*.XML";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                ExportMapServerSetClass ec=new ExportMapServerSetClass(this);
                bool flag = ec.ExportMapServerXml(sf.FileName);
                if (flag)
                {
                    MessageBox.Show("保存成功！");
                }

            }
        }

        private void btImport_Click(object sender, EventArgs e)
        {
            string fileName = app.Template.Root + @"\专家库\MapServerSet.xml";
            OpenFileDialog sf = new OpenFileDialog();
            sf.Title = "地图服务信息保存";
            sf.InitialDirectory = app.Template.Root + @"\专家库";
            sf.FileName = "MapServerSet.xml";
            sf.Filter = "XML|*.XML";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                ExportMapServerSetClass ec = new ExportMapServerSetClass(this);
                bool flag = ec.LoadMapServerXml(sf.FileName);
                if (flag)
                {
                   // MessageBox.Show("保存成功！");
                }

            }

        }

       
    
    }
}

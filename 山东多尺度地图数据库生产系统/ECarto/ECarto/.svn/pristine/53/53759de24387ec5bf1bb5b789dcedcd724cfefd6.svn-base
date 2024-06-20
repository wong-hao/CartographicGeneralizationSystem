using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GISClient;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;

namespace SMGI.Plugin.EmergencyMap.DataSource
{
    public partial class FormMapData : Form
    {
        GApplication app;
        public FormMapData(GApplication _app)
        {
            app = _app;
            InitializeComponent();
        }

        private void FormMapData_Load(object sender, EventArgs e)
        {
            this.btAddWMS.Tag = this.lvMapService;
            this.btDeleteWMS.Tag = this.lvMapService;
            this.btEditWMS.Tag = this.lvMapService;
            this.btServerWMS.Tag = this.gbMapService;
            LoadMapService();
        }
        private void groupBoxLeave(object sender, EventArgs s)
        {
            GroupBox currentGB = sender as GroupBox;
            foreach (Control c in currentGB.Controls)
            {
                if (c is ListView)
                {
                    ListView lv = c as ListView;
                    lv.SelectedItems.Clear();
                }
            }

        }

        private void btnAdd(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            ListView currentListView = currentBtn.Tag as ListView;
            Form form = null;
            if (currentListView.Name=="lvMapService")
            {
                form = new MapServicesForm(currentListView, FormType.Add);
            }
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }

        private void btnEdit(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            ListView currentListView = currentBtn.Tag as ListView;
            if (currentListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一行进行编辑！");
                return;
            }
            Form form = null;
            if (currentListView.Name=="lvMapService")
            {
                form = new MapServicesForm(currentListView, FormType.Edit);
            }
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }

        private void btnDelete(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            ListView currentListView = currentBtn.Tag as ListView;
            foreach (ListViewItem item in currentListView.SelectedItems)
            {
                currentListView.Items.Remove(item);
            }
        }

        private void btnValidServer(object sender, EventArgs e)
        {
            SimpleButton currentBtn = sender as SimpleButton;
            GroupBox gb = currentBtn.Tag as GroupBox;
            //获取相应服务器、图层信息进行连接验证
            if (gb.Name=="gbMapService")
            {
                foreach (ListViewItem item in lvMapService.Items)
                {
                    string mapServiceUrl = item.SubItems[1].Text;
                    string mapServiceName = item.SubItems[2].Text;
                    string mapServiceDes = item.SubItems[3].Text;
                    try
                    {
                        IAGSServerObjectName pServerObjectName = GetMapServer(mapServiceUrl, mapServiceName, false);
                        IName pName = (IName)pServerObjectName;
                        //访问地图服务
                        IAGSServerObject pServerObject = (IAGSServerObject)pName.Open();
                        IMapServer pMapServer = (IMapServer)pServerObject;
                        ESRI.ArcGIS.Carto.IMapServerLayer pMapServerLayer = new ESRI.ArcGIS.Carto.MapServerLayerClass();
                        //连接地图服务
                        pMapServerLayer.ServerConnect(pServerObjectName, pMapServer.DefaultMapName);
                    }
                    catch
                    {
                        MessageBox.Show(string.Format("地图服务[{0}]连接失败", mapServiceName));
                        return;
                    }
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
            //底图服务
            ChangeMapServices();
            #endregion
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 加载底图服务信息
        /// </summary>
        private void LoadMapService()
        {
            lvMapService.Columns.Add(new ColumnHeader { Text = "序号", Name = "id" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "服务Url", Name = "url" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "服务名称", Name = "name" });
            lvMapService.Columns.Add(new ColumnHeader { Text = "描述", Name = "des" });
            lvMapService.Columns[0].Width = 80;
            lvMapService.Columns[1].Width = 200;
            lvMapService.Columns[2].Width = 120;
            lvMapService.Columns[3].Width = 100;
            string fileName = app.Template.Root + @"\专家库\地图服务\MapService.xml";
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

                            string mapServiceUrl = item.Element("MapServiceUrl").Value;
                            string mapServiceName = item.Element("MapServiceName").Value;
                            string mapServiceDes = item.Element("MapServiceDes").Value;

                            ListViewItem lvi = new ListViewItem(ct.ToString());
                            ct++;
                            lvi.SubItems.AddRange(new string[] { mapServiceUrl, mapServiceName, mapServiceDes });
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
            string fileName = app.Template.Root + @"\专家库\地图服务\MapService.xml";
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
                            string mapServiceName = LVItem.SubItems[2].Text;
                            string mapServiceDes = LVItem.SubItems[3].Text;
                            XElement item = new XElement("Item");
                            XElement urlNode = new XElement("MapServiceUrl", mapServiceUrl);
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
    }
}

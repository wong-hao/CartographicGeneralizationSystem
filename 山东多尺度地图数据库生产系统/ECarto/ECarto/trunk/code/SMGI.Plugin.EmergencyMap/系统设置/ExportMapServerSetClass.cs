using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using SMGI.Plugin.EmergencyMap;
using SMGI.Common;

namespace SMGI.Plugin.EmergencyMap.SysSet
{
    class ExportMapServerSetClass
    {
        GApplication app = GApplication.Application;
        public FrmMapServerSet frm;
        public ExportMapServerSetClass(FrmMapServerSet f)
        {
            this.frm = f;
            
        }

        public bool ExportMapServerXml(string fileName)
        {
            try
            {
                ListView currMapServiceSet = frm.lvMapService;
                XDocument doc = new XDocument();
                XDeclaration docDeclaration = new XDeclaration("1.0", "utf-8", "null");
                doc.Declaration = docDeclaration;
                XElement temp = new XElement("Template");
                doc.Add(temp);
                XElement content = new XElement("Content");
                temp.Add(content);

                //在content下加 多尺度数据库配置
                #region
                ListView currMapRules = frm.lvDataBase;
                XElement mapserRules = new XElement("MapScaleRule");
                content.Add(mapserRules);
                XElement server = new XElement("server");
                server.Add(new XElement("IPAddress", frm.tbMainIP.Text.Trim()));
                server.Add(new XElement("UserName", frm.tbMainUserUame.Text.Trim()));
                server.Add(new XElement("Password", frm.tbMainPassword.Text.Trim()));
                mapserRules.Add(server);
                foreach (ListViewItem LVitem in frm.lvDataBase.Items)
                {
                    XElement item = new XElement("Item");
                    item.Add(new XElement("Scale", LVitem.SubItems[1].Text));
                    item.Add(new XElement("Max", LVitem.SubItems[2].Text));
                    item.Add(new XElement("Min", LVitem.SubItems[3].Text));
                    item.Add(new XElement("DatabaseName", LVitem.SubItems[4].Text));
                    item.Add(new XElement("MapTemplate", LVitem.SubItems[5].Text));
                    mapserRules.Add(item);
                }
                #endregion
                //在content下加  地图服务配置
                #region
                XElement mapservice = new XElement("MapService");
                content.Add(mapservice);
                foreach (ListViewItem LVitem in currMapServiceSet.Items)
                {
                    XElement mapsever = new XElement("Item");
                    XElement urlmapsever = new XElement("MapServiceUrl");
                    urlmapsever.SetAttributeValue("type", LVitem.SubItems[1].Text);
                    urlmapsever.Add(new XElement("DataBase", LVitem.SubItems[2].Text));
                    urlmapsever.Add(new XElement("IPAddress", LVitem.SubItems[3].Text));
                    urlmapsever.Add(new XElement("UserName", LVitem.SubItems[4].Text));
                    urlmapsever.Add(new XElement("Password", LVitem.SubItems[5].Text));
                    mapsever.Add(urlmapsever);
                    mapsever.Add(new XElement("MapServiceName", LVitem.SubItems[6].Text));
                    mapsever.Add(new XElement("MapServiceDes", LVitem.SubItems[7].Text));
                    mapservice.Add(mapsever);
                }
                #endregion
                //在content下加 地图定位配置
                XElement loaction = new XElement("MapLocation");
                content.Add(loaction);
                XElement pointLocation = new XElement("地点定位");
                pointLocation.Add(new XElement("IPAddress", frm.poiIP.Text));
                pointLocation.Add(new XElement("UserName", frm.poiUser.Text));
                pointLocation.Add(new XElement("Password", frm.poiPassword.Text));
                loaction.Add(pointLocation);

                XElement regionLocation = new XElement("区域定位");
                regionLocation.Add(new XElement("IPAddress", frm.regionIP.Text));
                regionLocation.Add(new XElement("UserName", frm.regionUser.Text));
                regionLocation.Add(new XElement("Password", frm.regionPassword.Text));
                loaction.Add(regionLocation);

                //在content下加 DEM数据源配置
                XElement DEMsorce = new XElement("DEMSource");
                DEMsorce.Add(new XElement("IPAddress", frm.demIP.Text));
                DEMsorce.Add(new XElement("UserName", frm.demUser.Text));
                DEMsorce.Add(new XElement("Password", frm.demPassword.Text));
                DEMsorce.Add(new XElement("DataBase", frm.demDb.Text.Trim()));
                DEMsorce.Add(new XElement("DEMName", frm.demLayer.Text.Trim()));
                content.Add(DEMsorce);

                //在content下加 专题数据配置
                XElement ThematicDataContent = new XElement("ThematicDataContent");
                content.Add(ThematicDataContent);
                foreach (ListViewItem item in frm.lvThematic.Items)
                {
                    XElement items = new XElement("Item");
                    XElement ThematicData = new XElement("ThematicData");
                    ThematicData.SetAttributeValue("name", item.SubItems[1].Text);
                    ThematicData.Add(new XElement("IPAddress", item.SubItems[2].Text));
                    ThematicData.Add(new XElement("UserName", item.SubItems[3].Text));
                    ThematicData.Add(new XElement("Password", item.SubItems[4].Text));
                    ThematicData.Add(new XElement("DataBase", item.SubItems[5].Text));
                    items.Add(ThematicData);
                    ThematicDataContent.Add(items);
                }
                //string fileName = app.Template.Root + @"\专家库\MapServerSet.xml";
                doc.Save(fileName);
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("导出配置信息失败："+ex.Message);
                return false;
            }
        }


        public bool LoadMapServerXml(string fileName)
        {
            try
            {
                frm.lvDataBase.Items.Clear();
                frm.lvThematic.Items.Clear();
                frm.lvMapService.Items.Clear();
                //读取地图尺度

                int ct = 1;
                ListViewItem MapRule = new ListViewItem();
                List<ListViewItem> MPrule = new List<ListViewItem>();
               // string fileName = app.Template.Root + @"\专家库\MapServerSet.xml";
                XDocument doc = XDocument.Load(fileName);
                XElement mapscalerule = doc.Root.Element("Content").Element("MapScaleRule");
                XElement server = mapscalerule.Element("server");
                frm.tbMainIP.Text = server.Element("IPAddress").Value;
                frm.tbMainPassword.Text = server.Element("Password").Value;
                frm.tbMainUserUame.Text = server.Element("UserName").Value;

                foreach (XElement item in mapscalerule.Elements("Item"))
                {
                    string Scale = item.Element("Scale").Value;
                    string Max = item.Element("Max").Value;
                    string Min = item.Element("Min").Value;
                    string DatabaseName = item.Element("DatabaseName").Value;
                    string MapTemplate = item.Element("MapTemplate").Value;
                    MapRule = new ListViewItem(ct.ToString());
                    ct++;
                    MapRule.SubItems.AddRange(new string[] { Scale, Max, Min, DatabaseName, MapTemplate });
                    MPrule.Add(MapRule);
                }
                for (int i = 0; i < MPrule.Count; i++)
                {
                    frm.lvDataBase.Items.Add(MPrule[i]);
                }


                //读取地图服务
                int cy = 1;
                ListViewItem MapServer = new ListViewItem();
                List<ListViewItem> MPserver = new List<ListViewItem>();
                XElement mapservice = doc.Root.Element("Content").Element("MapService");
                foreach (XElement item in mapservice.Elements("Item"))
                {
                    string MapServiceUrltype = item.Element("MapServiceUrl").Attribute("type").Value;
                    string DataBase = item.Element("MapServiceUrl").Element("DataBase").Value;
                    string IPAddress = item.Element("MapServiceUrl").Element("IPAddress").Value;
                    string UserName = item.Element("MapServiceUrl").Element("UserName").Value;
                    string Password = item.Element("MapServiceUrl").Element("Password").Value;
                    string MapServiceName = item.Element("MapServiceName").Value;
                    string MapServiceDes = item.Element("MapServiceDes").Value;
                    MapServer = new ListViewItem(ct.ToString());
                    cy++;
                    MapServer.SubItems.AddRange(new string[] { MapServiceUrltype, DataBase, IPAddress, UserName, Password, MapServiceName, MapServiceDes });
                    MPserver.Add(MapServer);
                }
                for (int j = 0; j < MPserver.Count; j++)
                {
                    frm.lvMapService.Items.Add(MPserver[j]);
                }
                //读取专题数据
                int cu = 1;
                ListViewItem TheMatic = new ListViewItem();
                List<ListViewItem> Tmatic = new List<ListViewItem>();
                XElement themaptic = doc.Root.Element("Content").Element("ThematicDataContent");
                foreach (XElement item in themaptic.Elements("Item"))
                {
                    string ThematicDataname = item.Element("ThematicData").Attribute("name").Value;
                    string IPAddress = item.Element("ThematicData").Element("IPAddress").Value;
                    string UserName = item.Element("ThematicData").Element("UserName").Value;
                    string Password = item.Element("ThematicData").Element("Password").Value;
                    string DataBase = item.Element("ThematicData").Element("DataBase").Value;
                    TheMatic = new ListViewItem(ct.ToString());
                    cu++;
                    TheMatic.SubItems.AddRange(new string[] { ThematicDataname, IPAddress, UserName, UserName, Password, DataBase });
                    Tmatic.Add(TheMatic);
                }
                for (int m = 0; m < Tmatic.Count; m++)
                {
                    frm.lvThematic.Items.Add(Tmatic[m]);
                }

                //读取地图定位服务
                XElement maplocation = doc.Root.Element("Content").Element("MapLocation");
                XElement pointlocation = maplocation.Element("地点定位");
                frm.poiIP.Text = pointlocation.Element("IPAddress").Value;
                frm.poiUser.Text = pointlocation.Element("UserName").Value;
                frm.poiPassword.Text = pointlocation.Element("Password").Value;
                XElement regionlocation = maplocation.Element("区域定位");
                frm.regionIP.Text = regionlocation.Element("IPAddress").Value;
                frm.regionUser.Text = regionlocation.Element("UserName").Value;
                frm.regionPassword.Text = regionlocation.Element("Password").Value;
                //4.dem
                XElement dem = doc.Root.Element("Content").Element("DEMSource");
                frm.demIP.Text = dem.Element("IPAddress").Value;
                frm.demUser.Text = dem.Element("UserName").Value;
                frm.demPassword.Text = dem.Element("Password").Value;
                frm.demDb.Text = dem.Element("DataBase").Value;
                frm.demLayer.Text = dem.Element("DEMName").Value;
                return false;
            }
            catch(Exception ex)
            {
                MessageBox.Show("导入配置信息失败："+ex.Message);
            
                return true;
            }
         }
    }
}



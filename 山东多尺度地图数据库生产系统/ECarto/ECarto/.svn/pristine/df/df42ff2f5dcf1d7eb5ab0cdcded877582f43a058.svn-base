using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using System.Data;
using ESRI.ArcGIS.Maplex;
using System.IO;
using System.Xml.Linq;
using System.Xml;
namespace SMGI.Plugin.EmergencyMap
{
    //专题数据处理类
    public class ThematicDataClass
    {
        //获取专题数据信息
        public static List<ThematicDataInfo> GetThemticElement(GApplication app, WaitOperation wo = null)
        {
            List<ThematicDataInfo> infos = new List<ThematicDataInfo>();
            try
            {
                var envFileName = @"\专家库\ThematicMapRule.xml";
                string fileName = app.Template.Root + envFileName;
               

               
                {
                    XDocument doc = XDocument.Load(fileName);

                    var content = doc.Element("Template").Element("Content");
                    var eles = content.Elements("ThematicData");
                    foreach (var ele in eles)
                    {
                        string name = ele.Attribute("name").Value;
                        string ip = ele.Element("IPAddress").Value;
                        string username = ele.Element("UserName").Value;
                        string ps = ele.Element("Password").Value;
                        string db = ele.Element("DataBase").Value;
                        ThematicDataInfo dinfo = new ThematicDataInfo();
                        dinfo.Name=name;
                        dinfo.IP = ip;
                        dinfo.UserName = username;
                        dinfo.Password = ps;
                        dinfo.DataBase = db;

                        if (wo != null)
                            wo.SetText(string.Format("正在获取专题数据库【{0}】信息......", dinfo.DataBase));

                        var lyrs = DataServerClass.getFeatureClassNamesEx(GApplication.Application, dinfo.IP, dinfo.UserName, dinfo.Password, dinfo.DataBase, dinfo);
                        Dictionary<string, bool> dic = new Dictionary<string, bool>();
                        foreach (var l in lyrs)
                        {
                            dic.Add(l.Key, true);
                        }
                        dinfo.Lyrs = dic;
                        dinfo.LyrsType = lyrs;
                        infos.Add(dinfo);
                          
                    }

                }
                return infos;
            }
            catch
            {
                return infos;
            }
        }
        //获取河湖专题数据信息 重新组织
        public static List<ThematicDataInfo> GetHydroThemticInfos(GApplication app)
        {
            List<ThematicDataInfo> infos = new List<ThematicDataInfo>();
            try
            {
                string folders = app.Template.Root + @"\专题";
                foreach (var folder in new System.IO.DirectoryInfo(folders).GetDirectories())
                {
                    var envFileName = @"\ThematicMapRule.xml";
                    string fileName = folder.FullName + envFileName;
                    

                   
                    {
                        XDocument doc = XDocument.Load(fileName);

                        var ele = doc.Element("Template").Element("DataBaseInfo");


                        {
                            string name = ele.Attribute("name").Value;
                            string ip = ele.Element("IP").Value;
                            string username = ele.Element("UserName").Value;
                            string ps = ele.Element("Password").Value;
                            string db = ele.Element("DataBase").Value;
                            var dinfo = new ThematicDataInfo();
                            dinfo.Name = name;
                            dinfo.IP = ip;
                            dinfo.UserName = username;
                            dinfo.Password = ps;
                            dinfo.DataBase = db;
                            var content = doc.Element("Template").Element("Content");

                            var lyrs = new Dictionary<string, string>();
                            foreach (var lyrele in content.Elements("Layer"))
                            {
                                string lyrName = lyrele.Attribute("name").Value;
                                string aliaName = lyrele.Element("AliaName").Value;
                                lyrs.Add(lyrName, aliaName);
                            }
                            Dictionary<string, bool> dic = new Dictionary<string, bool>();
                            foreach (var l in lyrs)
                            {
                                dic.Add(l.Key, true);
                            }
                            dinfo.Lyrs = dic;
                            dinfo.LyrsType = lyrs;
                            infos.Add(dinfo);

                        }

                    }
                }
                return infos;
            }
            catch
            {
                return infos;
            }
        }
        //获取专题数据库图层
        public static Dictionary<string, Dictionary<string, bool>> GetThemticInfos(List<ThematicDataInfo> infos)
        {
            var dataDic = new Dictionary<string, Dictionary<string, bool>>();
            try
            {

                //dataDic = new Dictionary<string, Dictionary<string, bool>>();
                //foreach (var info in infos)
                //{
                //    var list = DataServer.getFeatureClassNames(GApplication.Application, info.IP, info.UserName, info.Password, info.DataBase);
                //    Dictionary<string, bool> dic = new Dictionary<string, bool>();
                //    foreach (var l in list)
                //    {
                //        dic.Add(l, true);
                //    }
                //   // info.Lyrs.AddRange(dic.Keys.ToArray());
                //    dataDic[info.Name] = dic;
                //}
                return dataDic;
            }
            catch
            {
                return dataDic;
            }
        }
    }

    public class ThematicDataInfo
    {
        public  ThematicDataInfo()
        {
            Lyrs = new Dictionary<string, bool>();
            LyrsType = new Dictionary<string, string>();
            LyrsFields = new Dictionary<string, List<string>>();
        }
        public string Name;
        public string IP;
        public string UserName;
        public string Password;
        public string DataBase;
        public Dictionary <string,bool> Lyrs;
        public Dictionary<string, string> LyrsType;
        public Dictionary<string, List<string>> LyrsFields;
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}

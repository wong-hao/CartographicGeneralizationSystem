using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using SMGI.Common;
using System.Xml;

namespace SMGI.Plugin.EmergencyMap
{
    //专家经验参数方案类 v1.0
    public class ExpertiseParamsClass
    {

        public static string planStr = @"专家库\经验方案\经验方案.xml";

        /// <summary>
        /// 是否加载外部参数
        /// </summary>
        public static bool LoadOutParams(out string planxml)
        {
            GApplication app = GApplication.Application;
            string fileName = app.Template.Root + @"\EnvironmentSettings.xml";
            
            XDocument doc = XDocument.Load(fileName);  
            planxml = "";
            
            {
                
                var content = doc.Element("Template").Element("Content");
                var plan = content.Element("ExpertPlan");
                if (plan != null)
                {
                    var plandefault = plan.Element("PlanDefault");
                    var planoutside = plan.Element("PlanOutSide");
                    string val = planoutside.Value;
                    if (File.Exists(val) && val.IndexOf(".xml") != -1)
                    {

                        planxml = val;
                    }
                    else 
                    {
                        return false;
                    }
                    string planchecked = plandefault.Attribute("Checked").Value;
                    string outplanchecked = planoutside.Attribute("Checked").Value;
                    if (bool.Parse(planchecked))
                    {
                        return false;
                    }
                    if (bool.Parse(outplanchecked))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 1.更新图廓参数
        /// </summary>
        public static void UpdateMapLine(GApplication app, XElement rootEle)
        {
           
            string fileName = app.Template.Root + @"\" + planStr;
          
            XDocument doc = XDocument.Load(fileName); 
            
            {
                 

                var content = doc.Element("Template").Element("Content");
                var mapline = content.Element("MapLine");
                foreach (var ele in rootEle.Elements())
                {
                    string name = ele.Name.ToString();
                    string val = ele.Value;
                    mapline.SetElementValue(name, val);
                }
               
               

            }
            doc.Save(fileName);
        }

        /// <summary>
        /// 2.更新境界跳绘参数
        public static void UpdateBoulDraw(GApplication app, XElement rootEle)
        {
            string fileName = app.Template.Root + @"\" + planStr;
          
            XDocument doc = XDocument.Load(fileName); 
            
            {
              
                var content = doc.Element("Template").Element("Content");
                XElement BoulDraw = content.Element("BoulDraw");

                BoulDraw.SetElementValue("BufferValue", rootEle.Element("BufferValue").Value);
           
                BoulDraw.Element("ObjectLayer").RemoveAll();
                BoulDraw.Element("ObjectLayer").Add(rootEle.Element("ObjectLayer").Elements("Item"));

                var items = rootEle.Element("Boul").Elements("Item");
                foreach (var ele in BoulDraw.Element("Boul").Elements("Item"))
                {
                    string name = ele.Element("Level").Value;
                    var item = items.Where(t => t.Element("Level").Value == name).FirstOrDefault();
                    foreach (var sube in item.Elements())
                    {
                        //Level
                        string eName = sube.Name.ToString();
                        ele.SetElementValue(eName, sube.Value);
                    }
                }
            
            
            
            }
            doc.Save(fileName);
            
        }

        /// <summary>
        /// 3.更新图名参数
        /// </summary>
        public static void UpdateMapName(GApplication app, XElement rootEle)
        {

            string fileName = app.Template.Root + @"\" + planStr;
          
            XDocument doc = XDocument.Load(fileName);
            
            {
                

                var content = doc.Element("Template").Element("Content");
                var mapName = content.Element("MapName");

                mapName.SetElementValue("ArtChecked", rootEle.Element("ArtChecked").Value);
                if (mapName.Element("TopDis") != null)
                {
                    if (rootEle.Element("TopDis") != null)
                    {
                        mapName.SetElementValue("TopDis", rootEle.Element("TopDis").Value);
                    }
                }
                else
                {
                    mapName.Add(new XElement("TopDis", "4"));
                    if (rootEle.Element("TopDis") != null)
                    {
                        mapName.SetElementValue("TopDis", rootEle.Element("TopDis").Value);
                    }
                }
                

                string[] eleNames = new string[] { "Title", "ProductUnit", "MapTime" };
                foreach (string name in eleNames)
                {
                    var tempTitle = rootEle.Element(name);
                    var title = mapName.Element(name);
                    title.SetElementValue("TopDis", tempTitle.Element("TopDis").Value);
                    title.SetElementValue("FontSize", tempTitle.Element("FontSize").Value);
                    title.SetElementValue("FontName", tempTitle.Element("FontName").Value);
                    var font = title.Element("FontColor");
                    font.RemoveAll();
                    font.Add(tempTitle.Element("FontColor").Elements());
                }
                //出版
                if (mapName.Element("MapPublish") == null)
                {
                    mapName.Add(new XElement("MapPublish"));
                    var mappublish = mapName.Element("MapPublish");
                    mappublish.Add(new XElement("TopDis"));
                    mappublish.Add(new XElement("Content"));
                    mappublish.Add(new XElement("Direction"));
                    mapName.Add(mappublish);
                   
                }
                var mappublish1 = mapName.Element("MapPublish");
                mappublish1.SetElementValue("TopDis", rootEle.Element("MapPublish").Element("TopDis").Value);
                mappublish1.SetElementValue("Content", rootEle.Element("MapPublish").Element("Content").Value);
                mappublish1.SetElementValue("Direction", rootEle.Element("MapPublish").Element("Direction").Value);

            }
            doc.Save(fileName);
        }

        /// <summary>
        /// 4.更新花边参数
        /// </summary>
        public static void UpdateLaceBorder(GApplication app, XElement rootEle)
        {

            string fileName = app.Template.Root + @"\" + planStr;
           
            XDocument doc = XDocument.Load(fileName);
            
            {
                

                var content = doc.Element("Template").Element("Content");
                var mapBorder = content.Element("LaceBorder");

                mapBorder.SetElementValue("Width", rootEle.Element("Width").Value);
                mapBorder.SetElementValue("Interval", rootEle.Element("Interval").Value);
                mapBorder.Element("Border").RemoveAll();
                foreach (var item in rootEle.Element("Border").Elements("Item"))
                {
                    mapBorder.Element("Border").Add(item);
                }
                mapBorder.Element("Corner").RemoveAll();
                mapBorder.Element("Corner").Add(rootEle.Element("Corner").Element("Item"));
               

            }
            doc.Save(fileName);
        }

        /// <summary>
        /// 5.更新格网线参数
        /// </summary>
        public static void UpdateGridLine(GApplication app, XElement rootEle)
        {

            string fileName = app.Template.Root + @"\" + planStr;
          
            XDocument doc = XDocument.Load(fileName);
          
            {
               

                var content = doc.Element("Template").Element("Content");
                var gridLine = content.Element("GridLine");

                var MeatureGrid = gridLine.Element("MeatureGrid");
                MeatureGrid.SetElementValue("X", rootEle.Element("MeatureGrid").Element("X").Value);
                MeatureGrid.SetElementValue("Y", rootEle.Element("MeatureGrid").Element("Y").Value);
                MeatureGrid.SetAttributeValue("checked", rootEle.Element("MeatureGrid").Attribute("checked").Value);

                var GraticuleGrid = gridLine.Element("GraticuleGrid");
                GraticuleGrid.SetElementValue("Longitude", rootEle.Element("GraticuleGrid").Element("Longitude").Value);
                GraticuleGrid.SetElementValue("Latitude", rootEle.Element("GraticuleGrid").Element("Latitude").Value);
                GraticuleGrid.SetAttributeValue("checked", rootEle.Element("GraticuleGrid").Attribute("checked").Value);
              
                var IndexGrid = gridLine.Element("IndexGrid");
                IndexGrid.SetElementValue("Row", rootEle.Element("IndexGrid").Element("Row").Value);
                IndexGrid.SetElementValue("Column", rootEle.Element("IndexGrid").Element("Column").Value);
                IndexGrid.SetAttributeValue("checked", rootEle.Element("IndexGrid").Attribute("checked").Value);

                gridLine.SetElementValue("AnnoSize", rootEle.Element("AnnoSize").Value);
                gridLine.SetElementValue("AnnoInterval", rootEle.Element("AnnoInterval").Value);

                gridLine.SetElementValue("NumSize", rootEle.Element("NumSize").Value);
                gridLine.SetElementValue("NumInterval", rootEle.Element("NumInterval").Value);

            }
            doc.Save(fileName);
        }
        /// <summary>
        /// 6.更新丁字线参数
        /// </summary>
        public static void UpdateCutLine(GApplication app, XElement rootEle)
        {

            string fileName = app.Template.Root + @"\" + planStr;

            XDocument doc = XDocument.Load(fileName);
            
            {
                

                var content = doc.Element("Template").Element("Content");
                var mapline = content.Element("CutLine");
                foreach (var ele in rootEle.Elements())
                {
                    string name = ele.Name.ToString();
                    string val = ele.Value;
                    mapline.SetElementValue(name, val);
                }

            }
            doc.Save(fileName);
        }
        /// <summary>
        /// 7.更新地图图例线参数
        /// </summary>
        public static void UpdateMapLengend(GApplication app, XElement rootEle)
        {

            string fileName = app.Template.Root + @"\" + planStr;

            XDocument doc = XDocument.Load(fileName); 
          
            {
               

                var content = doc.Element("Template").Element("Content");
                var mapline = content.Element("MapLengend");
                mapline.RemoveAll();
                mapline.Add(rootEle.Elements());

            }
            doc.Save(fileName);
        }

    }
}

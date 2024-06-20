using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
namespace SMGI.Plugin.ThematicChart
{
    public class XMLHelper
    {
        public static bool CreateXML(string filename,string bh,string bw)
        {
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                XmlNode node = xmldoc.CreateXmlDeclaration("1.0", "utf-8", "");
                xmldoc.AppendChild(node);
                //创建根节点
                XmlElement baseroot = xmldoc.CreateElement("BaseInfo");
                xmldoc.AppendChild(baseroot);
                //地图集基本信息
                XmlNode info=  CreateNode(xmldoc, baseroot, "AtlasInfos", "");
                CreateNode(xmldoc, info, "BookHeight", bh);//地图开本高
                CreateNode(xmldoc, info, "BookWidth", bw);//地图开本宽
                //每页记录信息
                CreateNode(xmldoc, baseroot, "PageInfos", "");
                xmldoc.Save(filename);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建地图集工程失败："+ex.Message);
                return false;
            }
        }
        
        private static XmlNode CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value="")
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            if(value!="")
               node.InnerText = value;
            parentNode.AppendChild(node);
            return node;
        }
        /// <summary>
        /// 获取所有页面信息
        /// </summary>
        /// <param name="xmlpath"></param>
        /// <returns></returns>
        public static List<PageInfo> ObtainPageInfos()
        {
            List<PageInfo> pagesList = new List<PageInfo>();
            string xmlpath = AtlasApplication.ProjectIndexFile;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlpath);
            XmlNode xmlpages = xmlDoc.SelectSingleNode("/BaseInfo/PageInfos");
            XmlNodeList list = xmlpages.ChildNodes;
            foreach (XmlNode node in list)
            {
                PageInfo page=new PageInfo();
                string  PageID =node.SelectSingleNode("PageID").InnerText;
                page.PageID=PageID;
                string Title = node.SelectSingleNode("Title").InnerText;
                page.Title=Title;
                pageType MapPageType = (pageType)Convert.ToInt32(node.SelectSingleNode("MapPageType").InnerText);
                page.MapPageType=MapPageType;
                int PageNum = Convert.ToInt32(node.SelectSingleNode("PageNum").InnerText);
                page.PageNum = PageNum;
                pagesList.Add(page);
                //判断
                if (node.SelectSingleNode("DataSource").InnerText != "")
                {
                    page.DataSource = node.SelectSingleNode("DataSource").InnerText;
                }
                if (node.SelectSingleNode("MapTemplateStyle").InnerText != "")
                {

                    page.MapTemplateStyle = node.SelectSingleNode("MapTemplateStyle").InnerText;
                }
                if (node.SelectSingleNode("MapTemplateName").InnerText != "")
                {

                    page.MapTemplateName = node.SelectSingleNode("MapTemplateName").InnerText;
                }
                double MapScale = 0;
                Double.TryParse(node.SelectSingleNode("MapScale").InnerText, out MapScale);
                page.MapScale = MapScale;
                if (node.SelectSingleNode("DatabaseName").InnerText != "")
                {
                    page.DatabaseName = node.SelectSingleNode("DatabaseName").InnerText;
                }
                if (node.SelectSingleNode("MapTemplate").InnerText != "")
                {
                    page.MapTemplate = node.SelectSingleNode("MapTemplate").InnerText;
                }
               //if (node.SelectSingleNode("MapCenter/PointX").InnerText != "")
               //{
                double x = 0;
                Double.TryParse(node.SelectSingleNode("MapCenterX").InnerText, out x);
                double y = 0;
                Double.TryParse(node.SelectSingleNode("MapCenterY").InnerText, out y);
                IPoint MapCenter = new PointClass() { X = x, Y = y };
                page.MapCenter = MapCenter;
               //}
                double Height = 0;
                Double.TryParse(node.SelectSingleNode("Height").InnerText, out Height);
                page.Height = Height;
                double Width = 0;
                Double.TryParse(node.SelectSingleNode("Width").InnerText, out Width);
                page.Width = Width;
               string GDBPath =node.SelectSingleNode("GDBPath").InnerText;
               page.GDBPath=GDBPath;
              
            }
            return pagesList;
        }
        public static void ObtainBookInfos(out double height,out double width)
        {
            string xmlpath = AtlasApplication.ProjectIndexFile;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlpath);
            XmlNode xmlbook = xmlDoc.SelectSingleNode("/BaseInfo/AtlasInfos");
            double rs = 0;
            string bookwidth = xmlbook.SelectSingleNode("BookWidth").InnerText;
            Double.TryParse(bookwidth, out rs);
            width = rs;
            rs = 0;
            string bookheight = xmlbook.SelectSingleNode("BookHeight").InnerText;
            Double.TryParse(bookheight, out rs);
            height = rs;
        }
        /// <summary>
        /// 根据uid获取页面信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static PageInfo ObtainPageInfoByID(string uid)
        {
            PageInfo page = new PageInfo();
            string xmlpath = AtlasApplication.ProjectIndexFile;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlpath);
            XmlNode xmlpages = xmlDoc.SelectSingleNode("/BaseInfo/PageInfos");
            XmlNodeList list = xmlpages.ChildNodes;
            foreach (XmlNode node in list)
            {
               
                string PageID = node.SelectSingleNode("PageID").InnerText;
                if (PageID == uid)
                {
                    #region
                        page.PageID = PageID;
                        string Title = node.SelectSingleNode("Title").InnerText;
                        page.Title = Title;
                        pageType MapPageType = (pageType)Convert.ToInt32(node.SelectSingleNode("MapPageType").InnerText);
                        page.MapPageType = MapPageType;
                        int PageNum = Convert.ToInt32(node.SelectSingleNode("PageNum").InnerText);
                        page.PageNum = PageNum;
                       
                        //判断
                        if (node.SelectSingleNode("DataSource").InnerText != "")
                        {
                            page.DataSource = node.SelectSingleNode("DataSource").InnerText;
                        }
                        if (node.SelectSingleNode("MapTemplateStyle").InnerText != "")
                        {
                            page.MapTemplateStyle = node.SelectSingleNode("MapTemplateStyle").InnerText;
                        }
                        if (node.SelectSingleNode("MapTemplateName").InnerText != "")
                        {

                            page.MapTemplateName = node.SelectSingleNode("MapTemplateName").InnerText;
                        }
                        double MapScale = 0;
                        Double.TryParse(node.SelectSingleNode("MapScale").InnerText, out MapScale);
                        page.MapScale = MapScale;
                        if (node.SelectSingleNode("DatabaseName").InnerText != "")
                        {
                            page.DatabaseName = node.SelectSingleNode("DatabaseName").InnerText;
                        }
                        if (node.SelectSingleNode("MapTemplate").InnerText != "")
                        {
                            page.MapTemplate = node.SelectSingleNode("MapTemplate").InnerText;
                        }
                    
                        double x = 0;
                        Double.TryParse(node.SelectSingleNode("MapCenterX").InnerText, out x);
                        double y = 0;
                        Double.TryParse(node.SelectSingleNode("MapCenterY").InnerText, out y);
                        IPoint MapCenter = new PointClass() { X = x, Y = y };
                        page.MapCenter = MapCenter;
                        //}
                        double Height = 0;
                        Double.TryParse(node.SelectSingleNode("Height").InnerText, out Height);
                        page.Height = Height;
                        double Width = 0;
                        Double.TryParse(node.SelectSingleNode("Width").InnerText, out Width);
                        page.Width = Width;
                        string GDBPath = node.SelectSingleNode("GDBPath").InnerText;
                        page.GDBPath = GDBPath;
                        break;
                    #endregion
                }

            }
            return page;
        }
        /// <summary>
        /// 根据ID更新节点
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="nodeVal"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static bool EditPageByID(string nodeName, string nodeVal,string uid)
        {
         
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(AtlasApplication.ProjectIndexFile);
            XmlNode xmlpages = xmlDoc.SelectSingleNode("/BaseInfo/PageInfos");
            XmlNodeList list = xmlpages.ChildNodes;//page

            foreach (XmlNode node in list)
            {
                string PageID = node.SelectSingleNode("PageID").InnerText;
                if (PageID == uid)
                {
                    node.SelectSingleNode(nodeName).InnerText = nodeVal;
                    xmlDoc.Save(AtlasApplication.ProjectIndexFile);
                }
            }
            return true;
        }
      
        public static void UpdatePageInfos()
        {
            List<PageInfo> pagesList= ObtainPageInfos();
            if (AtlasApplication.CurrentPage == null)
                return;
            string guid = AtlasApplication.CurrentPage.PageID;
            foreach (var pgeinfo in pagesList)
            {
                if (pgeinfo.PageID == guid)
                {
                    AtlasApplication.CurrentPage = pgeinfo;
                    break;
                }
            }
            
        }
        /// <summary>
        /// 创建页面
        /// </summary>
        /// <param name="title"></param>
        /// <param name="type"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool CreatePage(string title, pageType type,out string uid)
        {
            uid = "";
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(AtlasApplication.ProjectIndexFile);
                XmlNode xmlpages = xmlDoc.SelectSingleNode("/BaseInfo/PageInfos");
               
                int num = 0;
                XmlNodeList list = xmlpages.ChildNodes;
                foreach (XmlNode node in list)
                {
                    int val = Convert.ToInt32(node.SelectSingleNode("MapPageType").InnerText);
                    val = (val > 2) ? (val - 2) : val;
                    num += val;
                }
                XmlNode page = CreateNode(xmlDoc, xmlpages, "Page");
                string guid = Guid.NewGuid().ToString();
                uid = guid;
                CreateDirectory(guid);
                CreateNode(xmlDoc, page, "PageID", guid);

                CreateNode(xmlDoc, page, "Title", title);
               
                CreateNode(xmlDoc, page, "MapPageType", ((int)type).ToString());

               

                CreateNode(xmlDoc, page, "MapScale");
                CreateNode(xmlDoc, page, "DatabaseName");
                CreateNode(xmlDoc, page, "DataSource");
                CreateNode(xmlDoc, page, "MapTemplate");
                CreateNode(xmlDoc, page, "MapTemplateStyle");
                CreateNode(xmlDoc, page, "MapTemplateName");
              
                CreateNode(xmlDoc, page, "MapCenterX");
                CreateNode(xmlDoc, page, "MapCenterY");
                CreateNode(xmlDoc, page, "Height");
                CreateNode(xmlDoc, page, "Width");
                CreateNode(xmlDoc, page, "GDBPath");
               
                int pagenum = (int)type;
                pagenum = (pagenum > 2) ? (pagenum - 2) : pagenum;
                CreateNode(xmlDoc, page, "PageNum", (num + pagenum).ToString());
                xmlDoc.Save(AtlasApplication.ProjectIndexFile);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建页面失败："+ex.Message);
                return false;
            }
        }

        private static void CreateDirectory(string uid)
        {
            string path = AtlasApplication.ProjectPath;
            string folder = path + "\\" + uid;
            System.IO.Directory.CreateDirectory(folder);
        }
        /// <summary>
        /// 编辑当前页面
        /// </summary>
        /// <returns></returns>
        public static bool EditPage(string nodeName, string nodeVal, string uid="")
        {
            if (uid == "")
            {
                uid = AtlasApplication.CurrentPage.PageID;
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(AtlasApplication.ProjectIndexFile);
            XmlNode xmlpages = xmlDoc.SelectSingleNode("/BaseInfo/PageInfos");
            XmlNodeList list = xmlpages.ChildNodes;//page
            
            foreach (XmlNode node in list)
            {
                string PageID = node.SelectSingleNode("PageID").InnerText;
                if (PageID == uid)
                {
                    node.SelectSingleNode(nodeName).InnerText = nodeVal;
                    xmlDoc.Save(AtlasApplication.ProjectIndexFile);
                }
            }
            return true;
        }
       /// <summary>
        /// 删除页面
       /// </summary>
       /// <returns></returns>
        public static bool DeletePageByID(string uid)
        { 
            string xmlpath = AtlasApplication.ProjectIndexFile;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlpath);
            XmlNode xmlpages = xmlDoc.SelectSingleNode("/BaseInfo/PageInfos");
            XmlNodeList list = xmlpages.ChildNodes;
            foreach (XmlNode node in list)
            {

                string PageID = node.SelectSingleNode("PageID").InnerText;
                if (PageID == uid)
                {
                    node.ParentNode.RemoveChild(node);
                    xmlDoc.Save(AtlasApplication.ProjectIndexFile);
                    break;
                }
            }
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using System.Data;
using SMGI.Common;
using System.IO;
using System.Xml.Linq;
using System.Windows;

namespace SMGI.Plugin.EmergencyMap
{
   public class CommonLocationMethods
    {

        //动态计算当前的比例尺
        public static  double CaluateMapScale(double InlineWidth, double InlineHeight, IGeometry polygon)
        { 

             GApplication m_Application = GApplication.Application;
           string templateRoot=m_Application.Template.Root;
           templateRoot= templateRoot.Substring(templateRoot.LastIndexOf("\\"));
            double scalew = polygon.Envelope.Width * 1.05 / (InlineWidth * 1.0e-3);//宽高各乘以1.05
            double scaleh = polygon.Envelope.Height * 1.05 / (InlineHeight * 1.0e-3);//宽高各乘以1.05
            double scale = Math.Max(scaleh, scalew);
           
            if (scale >= 10000)
            {
                if (templateRoot.Contains("江苏"))//江苏比例尺精确到5000
                {
                    scale = Math.Round(scale * 1.0e-4, 1) * 10000;
                    if (scale % 5000 != 0)
                    {
                        scale = scale + (5 - (scale / 1000) % 5) * 1000;
                    }
                }
                else//其他精确到1000
                {
                    scale = Math.Round(scale * 1.0e-4, 1) * 10000;
                }
            }
            if (scale < 10000 && scale >= 1000)
            {
                scale = Math.Round(scale * 1.0e-4, 2) * 10000;
            }
            if (scale < 1000 && scale >= 100)
            {
                scale = Math.Round(scale * 1.0e-4, 3) * 10000;
            }
            if (scale < 100)
            {
                scale = Math.Round(scale * 1.0e-2, 2) * 100;
            }
            return scale;
        }

        //匹配成图尺寸，内图廓尺寸，内外图廓间距
        public static DataRow CaluateMapAndInlineSize(double paperWidth, double paperHeight)
        {
          
            DataRow itemSel = null;
            GApplication m_Application = GApplication.Application;
            double min = Math.Min(paperWidth, paperHeight);
            double max = Math.Max(paperWidth, paperHeight);
            string filemdb = m_Application.Template.Root + @"\专家库\纸张整饰参数\整饰参数经验值.mdb";
            DataTable ruleDatatable;
            ruleDatatable = CommonMethods.ReadToDataTable(filemdb, "纸张整饰参数经验值");
            for (int i = 0; i < ruleDatatable.Rows.Count; i++)
            {
                #region 考虑两边
                DataRow row = ruleDatatable.Rows[i];
                double pagewidth = double.Parse(row["纸张宽"].ToString());
                double pageheight = double.Parse(row["纸张高"].ToString());
                double pagemin = Math.Min(pagewidth, pageheight);
                double pagemax = Math.Max(pagewidth, pageheight);
                if ((Math.Abs(pagemin - min) / min) < 0.2 && Math.Abs(pagemax - max) / max < 0.2)
                {
                    itemSel = row;
                    break;
                }
                #endregion
            }
            if (itemSel == null)
            {
                #region 考虑单边
                for (int i = 0; i < ruleDatatable.Rows.Count; i++)
                {
                    DataRow row = ruleDatatable.Rows[i];
                    double pagewidth = double.Parse(row["纸张宽"].ToString());
                    double pageheight = double.Parse(row["纸张高"].ToString());
                    double pagemin = Math.Min(pagewidth, pageheight);
                    double pagemax = Math.Max(pagewidth, pageheight);
                    if ((Math.Abs(pagemin - min) / min) < 0.2 || Math.Abs(pagemin - max) / max < 0.2)
                    {
                        itemSel = row;
                        break;
                    }
                    if ((Math.Abs(pagemax - min) / min) < 0.2 || Math.Abs(pagemax - max) / max < 0.2)
                    {
                        itemSel = row;
                        break;
                    }
                }
                #endregion
            }
            return itemSel;
        }

        /// <summary>
        /// 导出定位参数
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="scale"></param>
        /// <param name="paperWidth"></param>
        /// <param name="paperHeight"></param>
        /// <param name="mapWidth"></param>
        /// <param name="mapHeight"></param>
        /// <param name="innerBorderWidth"></param>
        /// <param name="innerBorderHeight"></param>
        /// <param name="borderSpacing"></param>
        /// <returns></returns>
        public static bool ExportLocationParamsXML(string fileName, double scale, double paperWidth, double paperHeight,
            double mapWidth, double mapHeight, double innerBorderWidth, double innerBorderHeight, double borderSpacing)
        {
            try
            {
                FileInfo f = new FileInfo(fileName);
                XDocument doc = new XDocument();
                doc.Declaration = new XDeclaration("1.0", "utf-8", "");
                var root = new XElement("LocationParams");
                doc.Add(root);

                var content = new XElement("Content");
                root.Add(content);

                var mapScaleItem = new XElement("mapScale");
                mapScaleItem.SetAttributeValue("scale", scale);
                content.Add(mapScaleItem);

                var paperSizeItem = new XElement("paperSize");
                paperSizeItem.SetAttributeValue("paperWidth", paperWidth);
                paperSizeItem.SetAttributeValue("paperHeight", paperHeight);
                content.Add(paperSizeItem);

                var mapSizeItem = new XElement("mapSize");
                mapSizeItem.SetAttributeValue("mapWidth", mapWidth);
                mapSizeItem.SetAttributeValue("mapHeight", mapHeight);
                content.Add(mapSizeItem);

                var borderSizeItem = new XElement("borderSize");
                borderSizeItem.SetAttributeValue("innerBorderWidth", innerBorderWidth);
                borderSizeItem.SetAttributeValue("innerBorderHeight", innerBorderHeight);
                borderSizeItem.SetAttributeValue("borderSpacing", borderSpacing);
                content.Add(borderSizeItem);

                doc.Save(fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 导入定位参数
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="scale"></param>
        /// <param name="paperWidth"></param>
        /// <param name="paperHeight"></param>
        /// <param name="mapWidth"></param>
        /// <param name="mapHeight"></param>
        /// <param name="innerBorderWidth"></param>
        /// <param name="innerBorderHeight"></param>
        /// <param name="borderSpacing"></param>
        /// <returns></returns>
        public static bool ImportLocationParamsXML(string fileName, ref double scale, ref double paperWidth, ref double paperHeight,
             ref double mapWidth, ref double mapHeight, ref double innerBorderWidth, ref double innerBorderHeight, ref double borderSpacing)
        {
            try
            {
                XDocument doc = XDocument.Load(fileName);
                var content = doc.Element("LocationParams").Element("Content");

                var mapScaleItem = content.Element("mapScale");
                scale = double.Parse(mapScaleItem.Attribute("scale").Value);

                var paperSizeItem = content.Element("paperSize");
                paperWidth = double.Parse(paperSizeItem.Attribute("paperWidth").Value);
                paperHeight = double.Parse(paperSizeItem.Attribute("paperHeight").Value);

                var mapSizeItem = content.Element("mapSize");
                mapWidth = double.Parse(mapSizeItem.Attribute("mapWidth").Value);
                mapHeight = double.Parse(mapSizeItem.Attribute("mapHeight").Value);

                var borderSizeItem = content.Element("borderSize");
                innerBorderWidth = double.Parse(borderSizeItem.Attribute("innerBorderWidth").Value);
                innerBorderHeight = double.Parse(borderSizeItem.Attribute("innerBorderHeight").Value);
                borderSpacing = double.Parse(borderSizeItem.Attribute("borderSpacing").Value);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }

    }
}

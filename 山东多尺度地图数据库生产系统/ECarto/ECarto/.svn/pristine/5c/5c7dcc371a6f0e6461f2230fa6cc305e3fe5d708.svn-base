using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SMGI.Common;
using System.IO;

namespace SMGI.Plugin.EmergencyMap
{
    public class ExpertiseDatabase
    {
        /// <summary>
        /// 从专家数据库中获取Content元素
        /// </summary>
        /// <returns></returns>
        public static XElement getContentElement(GApplication app)
        {
            string envFileName = app.Template.Content.Element("Expertise").Value;
            string fileName = app.Template.Root + @"\专家库\" + envFileName;
            XDocument doc = XDocument.Load(fileName);
            
            {
                return doc.Element("Expertise").Element("Content");
            }
        }

        public static double getTemplateScale(double scale)
        {
            var expertiseContent = ExpertiseDatabase.getContentElement(GApplication.Application);
            var mapScaleRule = expertiseContent.Element("MapScaleRule");
            var scaleItems = mapScaleRule.Elements("Item");
            foreach (XElement ele in scaleItems)
            {
                double min = double.Parse(ele.Element("Min").Value);
                double max = double.Parse(ele.Element("Max").Value);
                double templateScale = double.Parse(ele.Element("Scale").Value);
                if (scale >= min && scale <= max)
                {
                    return templateScale;
                }
            }

            return 0;
        }
    }
}

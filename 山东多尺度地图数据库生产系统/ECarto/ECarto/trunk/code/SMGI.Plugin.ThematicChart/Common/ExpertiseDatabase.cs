using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SMGI.Common;
using System.IO;

namespace SMGI.Plugin.ThematicChart
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
            string fileName = app.Template.Root + @"\" + envFileName;
            FileInfo f = new FileInfo(fileName);

            using (var fst = f.Open(FileMode.Open))
            {
                XDocument doc = XDocument.Load(fst);
                return doc.Element("Expertise").Element("Content");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Data;

namespace SMGI.Plugin.EmergencyMap
{
    public class PaperInfo
    {
        public string Name { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double Pram { get;  set; }//缩放系数
        public PaperInfo(string name, double width, double height)
        {
            this.Name = name;
            this.Height = height;
            this.Width = width;
        }

        public PaperInfo(string info)
        {
            var ss = info.Split(new string[] { ",", " ", "X" }, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length > 2)
            {
                this.Name = ss[0];
                this.Width = double.Parse(ss[1]);
                this.Height = double.Parse(ss[2]);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}mm X {2}mm", Name, Width, Height);
        }
    }

    public class PageSizeRules
    {
        public static string getPageSizeRulePath(GApplication app)
        {
            var PageSizeRulesFileName = app.Template.Content.Element("PageSizeRules").Value;
            string fileName = app.Template.Root + @"\专家库\" + PageSizeRulesFileName;

            return fileName;
        }

        public static DataTable getPageSizeRuleTable(GApplication app)
        {
            string fileName = PageSizeRules.getPageSizeRulePath(app);
            DataTable table = CommonMethods.ReadToDataTable(fileName, "PageSize");
            return table;
        }

        public static List<PaperInfo> getPageSizeInfos(GApplication app)
        {
            List<PaperInfo> pis = new List<PaperInfo>();
            DataTable dt = PageSizeRules.getPageSizeRuleTable(app);

            foreach (DataRow dr in dt.Rows)
            {
                string pagename = dr["名称"].ToString();
                string orientation = dr["朝向"].ToString();
                double width = Convert.ToDouble(dr["宽"].ToString());
                double height = Convert.ToDouble(dr["高"].ToString());
                    double param = Convert.ToDouble(dr["边框距离系数"].ToString());
                PaperInfo pi = new PaperInfo(pagename + orientation, width, height);
                pi.Pram = param;
                pis.Add(pi);
            }

            return pis;
        }
    }
}

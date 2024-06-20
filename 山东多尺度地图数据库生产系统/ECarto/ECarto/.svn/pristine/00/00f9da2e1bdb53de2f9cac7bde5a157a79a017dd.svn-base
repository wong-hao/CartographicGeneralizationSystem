using System;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
namespace ShellTBDivided
{
    /// <summary>
    /// 制图综合规则类
    /// </summary>
    public sealed class GenRulesClass
    {
        public GenRulesClass(string fileName)
        {
            TBGenRules = new Dictionary<string, TBGenRule>();
          
            if (File.Exists(fileName))
            {
                XDocument doc = XDocument.Load(fileName);
                var items = doc.Element("Rules").Elements("Rule");

                foreach (var item in items)
                {
                    string dlbm = item.Attribute("DLBM").Value;
                    string name = item.Attribute("Name").Value;
                    string area = item.Attribute("Area").Value;
                    TBGenRule tbRule = new TBGenRule { Name = name, Area = double.Parse(area) };
                    Dictionary<int, TBRule> rules = new Dictionary<int, TBRule>();
                    var mergeRules = item.Elements("MergeRule");
                    #region
                    foreach (var r in mergeRules)
                    {
                        TBRule rule = new TBRule { Name = r.Value };
                        string attrs = r.Attribute("DLBM").Value;
                        string level = r.Attribute("Level").Value;
                        string[] gbs = attrs.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                        Dictionary<string, int> dic = new Dictionary<string, int>();
                        foreach (var gb in gbs)
                        {
                            dic[gb] = int.Parse(level);
                        }
                        rule.MergeDic = dic;
                        rules[int.Parse(level)] = rule;
                    }
                    #endregion
                    tbRule.rules = rules;
                    TBGenRules[dlbm] = tbRule;
                }
            }
        }
        public Dictionary<string, TBGenRule> TBGenRules=new Dictionary<string,TBGenRule>();
    }
    public sealed class TBGenRule
    {
        public string Name;
        public double Area;
        public Dictionary<int, TBRule> rules;//level->rule

    }
    public sealed class TBRule
    {
        public string Name;//人工
        public Dictionary<string, int> MergeDic;
    }
}

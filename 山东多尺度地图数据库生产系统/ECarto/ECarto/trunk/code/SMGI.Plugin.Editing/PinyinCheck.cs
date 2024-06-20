using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using System.IO;

namespace SMGI.Plugin.GeneralEdit
{
    public class PinyinCheck : SMGICommand
    {
        public PinyinCheck()
        {
            m_caption = "拼音检查";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            pinyincheck();
        }


        private void pinyincheck()
        {
            List<string> list = new List<string>();
            List<string> wrong = new List<string>();
            wrong.Clear();
            list.Clear();
            var _dtLayerRule = Helper.ReadToDataTable(m_Application.Template.Root + "\\" + m_Application.Template.Content.Element("RuleMatch").Value, "汉字拼音");
            foreach (DataRow prow in _dtLayerRule.Rows)
            {
                list.Add(prow[0].ToString().Trim() + "\t" + prow[1].ToString().Trim());
            }
            var pFearureWorkspace = (IFeatureWorkspace)m_Application.Workspace.EsriWorkspace;
            var pwtable = pFearureWorkspace.OpenFeatureClass("AANP");
            IFeatureCursor cursor = pwtable.Search(null, false);
            IFeature fe;
            while ((fe = cursor.NextFeature()) != null)
            {
                string name = fe.get_Value(pwtable.FindField("NAME")).ToString();
                string pyname = string.Empty;
                for (int z = 0; z < name.Length; z++)
                {
                    var danzi = name.Substring(z, 1);
                    if (danzi == "-" || danzi == "(" || danzi == ")")
                    {
                        pyname += danzi;
                    }
                    else
                    {
                        var hang = list.FirstOrDefault(i => i.Contains(danzi));
                        if (hang != null)
                        {
                            pyname += hang.Split('\t')[1];
                        }
                    }
                }
                string bjname=fe.get_Value(pwtable.FindField("PINYIN")).ToString();
                if (pyname.ToUpper() != bjname.Replace(" ", "").ToUpper()) {
                    wrong.Add("aanp\t"+fe.OID);
                }
            }
            var pwtable1 = pFearureWorkspace.OpenFeatureClass("AGNP");
            IFeatureCursor cursor1 = pwtable1.Search(null, false);
            IFeature fe1;
            while ((fe1 = cursor1.NextFeature()) != null)
            {
                string name = fe1.get_Value(pwtable.FindField("NAME")).ToString();
                string pyname = string.Empty;
                for (int z = 0; z < name.Length; z++)
                {
                    var danzi = name.Substring(z, 1);
                    if (danzi == "-" || danzi == "(" || danzi == ")")
                    {
                        pyname += danzi;
                    }
                    else
                    {
                        var hang = list.FirstOrDefault(i => i.Contains(danzi));
                        if (hang != null)
                        {
                            pyname += hang.Split('\t')[1];
                        }
                    }
                }
                string bjname = fe1.get_Value(pwtable.FindField("PINYIN")).ToString();
                if (pyname.ToUpper() != bjname.Replace(" ", "").ToUpper())
                {
                    wrong.Add("agnp\t" + fe1.OID);
                }
            }
            if (wrong.Count > 0)
            {
                MessageBox.Show("检查完成有" + wrong.Count + "个错误");
                File.WriteAllLines(m_Application.Workspace.FullName.Substring(0, m_Application.Workspace.FullName.LastIndexOf('\\')) + "\\没有的字.txt", wrong.ToArray());
            }
            else {
                MessageBox.Show("检查完成没有错误");
            }
        }

    }
}

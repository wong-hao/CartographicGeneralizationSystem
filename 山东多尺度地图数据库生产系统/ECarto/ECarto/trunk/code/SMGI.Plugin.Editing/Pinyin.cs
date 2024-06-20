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
    public class Pinyin: SMGICommand
    {
        public Pinyin()
        {
            m_caption = "赋拼音";
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
            pinyin();
        }


        private void pinyin()
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
            IFeatureCursor cursor = pwtable.Update(null, false);
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
                            if (z == 0)
                            {
                                pyname = hang.Split('\t')[1].Substring(0, 1).ToUpper() + hang.Split('\t')[1].Substring(1);
                            }
                            else
                            {
                                pyname += hang.Split('\t')[1];
                            }
                        }
                        else
                        {
                            wrong.Add(danzi);
                        }
                    }
                }
                fe.set_Value(pwtable.FindField("PINYIN"), pyname);
                cursor.UpdateFeature(fe);
            }
            var pwtable1 = pFearureWorkspace.OpenFeatureClass("AGNP");
            IFeatureCursor cursor1 = pwtable1.Update(null, false);
            IFeature fe1;
            while ((fe1 = cursor1.NextFeature()) != null)
            {
                string name = fe1.get_Value(pwtable1.FindField("NAME")).ToString();
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
                            if (z == 0)
                            {
                                pyname = hang.Split('\t')[1].Substring(0, 1).ToUpper() + hang.Split('\t')[1].Substring(1);
                            }
                            else
                            {
                                pyname += hang.Split('\t')[1];
                            }
                        }
                        else
                        {
                            wrong.Add(danzi);
                        }
                    }
                }
                fe1.set_Value(pwtable1.FindField("PINYIN"), pyname);
                cursor1.UpdateFeature(fe1);
            }
            if (wrong.Count > 0)
            {
                MessageBox.Show("赋值完成有" + wrong.Count + "个没有的字");
                File.WriteAllLines(m_Application.Workspace.FullName.Substring(0, m_Application.Workspace.FullName.LastIndexOf('\\')) + "\\没有的字.txt", wrong.ToArray());
            }
            else {
                MessageBox.Show("赋值完成");
            }
        }

    }
}

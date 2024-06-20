using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.esriSystem;
using System.Data;
using System.Xml.Linq;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class BoulSkipDrawCmd : SMGI.Common.SMGICommand
    {
        public BoulSkipDrawCmd()
        {
            m_category = "境界跳绘";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            IWorkspace ws = m_Application.Workspace.EsriWorkspace;
            var view = m_Application.MapControl.ActiveView;
            var map = view.FocusMap;
            FrmBOULTHSet frm = new FrmBOULTHSet(m_Application,"XJ");
            frm.ShowDialog();
         
        }

        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                #region 解析参数并执行
                double tolerance = 0;
                if (args.Element("BufferValue") != null)
                    tolerance = double.Parse(args.Element("BufferValue").Value);
                string ruleMatchFileName = EnvironmentSettings.getLayerRuleDBFileName(GApplication.Application);
                DataTable dtLayerRule = Helper.ReadToDataTable(ruleMatchFileName, "图层对照规则");
                messageRaisedAction("境界跳绘开始...");
                Dictionary<string, string> layerList = new Dictionary<string, string>();
                Dictionary<string, BoulDrawInfo> BoulDrawDic = new Dictionary<string, BoulDrawInfo>();
                foreach (var lyr in args.Element("Layers").Elements("Layer"))
                {
                    layerList[lyr.Element("Name").Value] = lyr.Element("SQL").Value;
                }
                foreach (var lyr in args.Element("BoulInfos").Elements("Item"))
                {
                    string key = lyr.Attribute("Name").Value;
                    BoulDrawInfo info = new BoulDrawInfo();
                    info.BlankValue = double.Parse(lyr.Element("BlankValue").Value);
                    info.Level = lyr.Element("Level").Value;
                    info.Checked = true;
                    string gblist = lyr.Element("GBList").Value;
                    List<int> list = new List<int>();
                    foreach (string gb in gblist.Split(new char[] { ',' }))
                    {
                        list.Add(int.Parse(gb));
                    }
                    info.GBList = list;
                    info.PointStep = double.Parse(lyr.Element("PointStep").Value);
                    info.SolidValue = double.Parse(lyr.Element("SolidValue").Value);
                    info.SymbolGroup = double.Parse(lyr.Element("SymbolGroup").Value);
                    BoulDrawDic[key] = info;
                }
                var bp = new BoulSkipDrawHelper(m_Application, BoulDrawDic, tolerance, layerList);
                bp._dtLayerRule = dtLayerRule;
                string err = bp.boulProduce();
                if (err != "")
                {
                    messageRaisedAction("境界跳绘失败!");
                    return false;
                }
                m_Application.Workspace.Save();
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
        }
    }
}


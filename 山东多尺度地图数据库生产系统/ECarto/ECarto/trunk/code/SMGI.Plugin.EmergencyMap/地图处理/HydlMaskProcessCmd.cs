using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.CartoUI;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Display;
using System.IO;
using System.Xml.Linq;

namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class HydlMaskProcessCmd : SMGI.Common.SMGICommand
    {
        public HydlMaskProcessCmd()
        {
            m_caption = "水系结构线消隐";
            m_toolTip = "水系结构线消隐";
            m_category = "水系结构线消隐";
        }
      
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
                   
            }
        }
    
        public override void OnClick()
        {

            var frmMask =new Mask.FrmMaskingSet();
            if (frmMask.ShowDialog() != DialogResult.OK)
                return;

            bool usingMask = frmMask.UsingMask;
            string maskingLyr = frmMask.MaskingLyr;
            string maskedLyr = frmMask.MaskedLyr;
            CommonMethods.UsingMask = usingMask;
            CommonMethods.MaskLayer = frmMask.MaskedLyr;
            CommonMethods.MaskedLayer = frmMask.MaskingLyr;
            IFeatureClass HYDAfcl = null;
            IFeatureClass HYDLfcl = null;
            IGroupLayer groupLyr = null;
            #region
            var HYDLlyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
            {
                return x is IFeatureLayer && (x as IFeatureLayer).Name == maskingLyr && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;

            })).FirstOrDefault() as IFeatureLayer;
           
            //判断不是临时数据
            var HYDAlyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
            {
                return x is IFeatureLayer && (x as IFeatureLayer).Name == maskedLyr && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;

            })).FirstOrDefault() as IFeatureLayer;
            var groups = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
            {
                return (x is IGroupLayer);

            }));

            if (HYDLlyr != null)
            {
                HYDLfcl = HYDLlyr.FeatureClass;
            }
            else
            {
                MessageBox.Show("未找到水系线图层!", "提示");
                return;
            }
            if (HYDAlyr != null)
            {
                HYDAfcl = HYDAlyr.FeatureClass;

            }
            else
            {
                MessageBox.Show("未找到水系面图层!", "提示");
                return;
            }
            #endregion
            #region
            IGroupLayer groupLyr1 = null;
            IGroupLayer groupLyr2 = null;
            foreach (var group in groups)
            {
                ICompositeLayer g = group as ICompositeLayer;
                for (int i = 0; i < g.Count; i++)
                {
                    var l = g.get_Layer(i);
                    if (l is IFeatureLayer)
                    {
                        if ((l as IFeatureLayer).Name == maskingLyr)
                        {
                            groupLyr1 = g as IGroupLayer;
                        }
                        if ((l as IFeatureLayer).Name == maskedLyr)
                        {
                            groupLyr2 = g as IGroupLayer;
                        }
                    }
                }
            }
            if (groupLyr1.Equals(groupLyr2))
            {
                groupLyr = groupLyr1;

            }
            else
            {
                MessageBox.Show("不在同一个图层组!", "提示");
                return;
            }
            #endregion

            //增加定义查询：不显示要素
            var fd = HYDAlyr as ESRI.ArcGIS.Carto.IFeatureLayerDefinition;
            string finitionExpression = fd.DefinitionExpression;
            if (!finitionExpression.ToLower().Contains(string.Format("ruleid <> {0}", 1)))
            {
                if (finitionExpression != "")
                {
                    fd.DefinitionExpression = string.Format("({0}) and (ruleid <> {1})", finitionExpression, 1);
                }
                else
                {
                    fd.DefinitionExpression = string.Format("ruleid <> {0}", 1);
                }
            }

            ILayerMasking lyrMask = m_Application.ActiveView.FocusMap as ILayerMasking;
            if(groupLyr !=null)
                lyrMask = groupLyr as ILayerMasking;
            lyrMask.ClearMasking(HYDLlyr);
            if (usingMask)
            {
                lyrMask.UseMasking = true;
                ESRI.ArcGIS.esriSystem.ISet pSet = new ESRI.ArcGIS.esriSystem.SetClass();
                pSet.Add(HYDAlyr);
                lyrMask.set_MaskingLayers(HYDLlyr, pSet);
            }
            else
            {
                lyrMask.UseMasking = false;
            }
            m_Application.ActiveView.Refresh();
            return;
            #region

            WaitOperation wo = wo = GApplication.Application.SetBusy();
            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;
            gp.AddOutputsToMap = false;
            gp.SetEnvironmentValue("workspace", GApplication.Application.Workspace.EsriWorkspace.PathName);
            IWorkspace ws = GApplication.Application.Workspace.EsriWorkspace;

            string tempfullpath = AnnoHelper.GetAppDataPath() + "\\MyWorkspace886.gdb";
            IWorkspace pTempWorkspace = AnnoHelper.createTempWorkspace(tempfullpath);
            string tempwspath = pTempWorkspace.PathName;

            IFeatureClass HYDAIntersHYDLfcls = GetFclViaWs(pTempWorkspace, "HYDAIntersHYDL");
            if (HYDAIntersHYDLfcls != null)
            {
                (HYDAIntersHYDLfcls as IDataset).Delete();
            }
           
            IFeatureClass HYDLdifffcls = GetFclViaWs(pTempWorkspace, "HYDLdiff");
            if (HYDAIntersHYDLfcls != null)
            {
                (HYDLdifffcls as IDataset).Delete();
            }
            IFeatureClass HYDATemp = GetFclViaWs(pTempWorkspace, "HYDATemp");
            if (HYDAIntersHYDLfcls != null)
            {
                (HYDATemp as IDataset).Delete();
            }


            CommonMethods.CreateFeatureClass(pTempWorkspace, "HYDATemp", HYDAfcl.Fields);


            Intersect intersTool = new Intersect();
            intersTool.in_features = ws.PathName + "\\" + HYDAfcl.AliasName + ";" + ws.PathName + "\\" + HYDLfcl.AliasName;
            intersTool.out_feature_class = pTempWorkspace.PathName + "\\HYDAIntersHYDL";
            intersTool.output_type = "LINE";
            if (wo != null)
                wo.SetText("正在进行线面求交……");
            SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
            HYDAIntersHYDLfcls = GetFclViaWs(pTempWorkspace, "HYDAIntersHYDL");
            //求异
            SymDiff diff = new SymDiff();
            diff.in_features = ws.PathName + "\\" + HYDLfcl.AliasName;
            diff.update_features = pTempWorkspace.PathName + "\\" + HYDAIntersHYDLfcls.AliasName;
            diff.out_feature_class = pTempWorkspace.PathName + "\\HYDLdiff";
            if (wo != null)
                wo.SetText("正在进行求异……");
            SMGI.Common.Helper.ExecuteGPTool(gp, diff, null);

            var fclInsect = (pTempWorkspace as IFeatureWorkspace).OpenFeatureClass("HYDAIntersHYDL");
            IFeature fe;
            IFeatureCursor cursor = fclInsect.Search(null, false);
            Dictionary<int, bool> oidDic = new Dictionary<int, bool>();
            while ((fe = cursor.NextFeature()) != null)
            {
                int oid = int.Parse(fe.get_Value(fclInsect.FindField("FID_HYDL")).ToString());
                oidDic[oid] = false;
            }

            var fclDiff = (pTempWorkspace as IFeatureWorkspace).OpenFeatureClass("HYDLdiff");
            cursor = fclDiff.Search(null, false);
            IMapContext mapContext = new MapContextClass();
            var rv = (HYDLlyr as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            var rpcv = rv.RepresentationClass;
            var rules = rpcv.RepresentationRules;

            wo.SetText("正在进行水系中心线消隐……");
            try
            {
                while ((fe = cursor.NextFeature()) != null)
                {
                    int oid = int.Parse(fe.get_Value(fclDiff.FindField("FID_HYDL")).ToString());
                    if (oidDic.ContainsKey(oid))
                    {
                        IRepresentationGraphics rg = new RepresentationGraphicsClass();
                        IRepresentationRule rule = null;
                        IFeature hydlfe = HYDLfcl.GetFeature(oid);
                        int ruleID = int.Parse(hydlfe.get_Value(HYDLfcl.FindField("RuleID")).ToString());
                        //oidDic[fe.OID] = true;
                        oidDic[oid] = true;
                        if (ruleID < 0) continue;
                        rule = rules.get_Rule(ruleID);
                        mapContext.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
                        IRepresentation rep = rpcv.GetRepresentation(hydlfe, mapContext);
                        rg.Add(fe.ShapeCopy, rule);
                        rep.Graphics = rg;
                        rep.UpdateFeature();
                        rep.Feature.Store();

                    }

                }
                foreach (var kv in oidDic)
                {

                    if (!kv.Value)
                    {
                        IRepresentationGraphics rg = new RepresentationGraphicsClass();
                        IRepresentationRule rule = null;
                        int oid = kv.Key;
                        IFeature hydlfe = HYDLfcl.GetFeature(oid);
                        int ruleID = int.Parse(hydlfe.get_Value(HYDLfcl.FindField("RuleID")).ToString());
                        if (ruleID < 0) continue;
                        rule = rules.get_Rule(ruleID);
                        mapContext.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
                        IRepresentation rep = rpcv.GetRepresentation(hydlfe, mapContext);
                        //rg.Add(fe.ShapeCopy, rule);
                        rep.Graphics = rg;
                        rep.UpdateFeature();
                        rep.Feature.Store();

                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
            HYDAIntersHYDLfcls = GetFclViaWs(pTempWorkspace, "HYDAIntersHYDL");
            if (HYDAIntersHYDLfcls != null)
            {
                (HYDAIntersHYDLfcls as IDataset).Delete();
            }

            HYDLdifffcls = GetFclViaWs(pTempWorkspace, "HYDLdiff");
            if (HYDAIntersHYDLfcls != null)
            {
                (HYDLdifffcls as IDataset).Delete();
            }
 
            wo.Dispose();
            GC.Collect();
            m_Application.ActiveView.Refresh();
            #endregion
        }
        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                bool usingMask = true;
                string hydl = "HYDL";
                string hyda = "HYDA";
                string maskingLyr = "HYDL";
                string maskedLyr = "HYDA";
                var fileName = m_Application.Template.Root + @"\专家库\消隐\水系消隐.xml";

                if (File.Exists(fileName))
                {
                    XDocument doc = XDocument.Load(fileName);
                    var content = doc.Element("Template").Element("Content");
                    var mask = content.Element("MakedLayer");
                    if (mask != null)
                        maskedLyr = mask.Value;
                    var masked = content.Element("MakingLayer");
                    if (masked != null)
                        maskingLyr = masked.Value;
                }
                IFeatureClass HYDAfcl = null;
                IFeatureClass HYDLfcl = null;
                IGroupLayer groupLyr = null;
                #region
                var HYDLlyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                {
                    return x is IFeatureLayer && (x as IFeatureLayer).FeatureClass.AliasName.ToUpper() == hydl && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;

                })).FirstOrDefault() as IFeatureLayer;

                //判断不是临时数据
                var HYDAlyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                {
                    return x is IFeatureLayer && (x as IFeatureLayer).FeatureClass.AliasName.ToUpper() == hyda && ((x as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;

                })).FirstOrDefault() as IFeatureLayer;
                var groups = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                {
                    return (x is IGroupLayer);

                }));

                if (HYDLlyr != null)
                {
                    HYDLfcl = HYDLlyr.FeatureClass;
                    maskingLyr = HYDLlyr.Name;

                }
                else
                {
                    return false;
                }
                if (HYDAlyr != null)
                {
                    HYDAfcl = HYDAlyr.FeatureClass;
                    maskedLyr = HYDAlyr.Name;
                }
                else
                {
                    return false;
                }
                #endregion
                #region
                IGroupLayer groupLyr1 = null;
                IGroupLayer groupLyr2 = null;
                foreach (var group in groups)
                {
                    ICompositeLayer g = group as ICompositeLayer;
                    for (int i = 0; i < g.Count; i++)
                    {
                        var l = g.get_Layer(i);
                        if (l is IFeatureLayer)
                        {
                            if ((l as IFeatureLayer).Name == maskingLyr)
                            {
                                groupLyr1 = g as IGroupLayer;
                            }
                            if ((l as IFeatureLayer).Name == maskedLyr)
                            {
                                groupLyr2 = g as IGroupLayer;
                            }
                        }
                    }
                }
                if (groupLyr1.Equals(groupLyr2))
                {
                    groupLyr = groupLyr1;

                }
                else
                {

                    return false;
                }
                #endregion

                //增加定义查询：不显示要素
                var fd = HYDAlyr as ESRI.ArcGIS.Carto.IFeatureLayerDefinition;
                string finitionExpression = fd.DefinitionExpression;
                if (!finitionExpression.ToLower().Contains(string.Format("ruleid <> {0}", 1)))
                {
                    if (finitionExpression != "")
                    {
                        fd.DefinitionExpression = string.Format("({0}) and (ruleid <> {1})", finitionExpression, 1);
                    }
                    else
                    {
                        fd.DefinitionExpression = string.Format("ruleid <> {0}", 1);
                    }
                }

                ILayerMasking lyrMask = m_Application.ActiveView.FocusMap as ILayerMasking;
                if (groupLyr != null)
                    lyrMask = groupLyr as ILayerMasking;
                lyrMask.ClearMasking(HYDLlyr);
                if (usingMask)
                {
                    lyrMask.UseMasking = true;
                    ESRI.ArcGIS.esriSystem.ISet pSet = new ESRI.ArcGIS.esriSystem.SetClass();
                    pSet.Add(HYDAlyr);
                    lyrMask.set_MaskingLayers(HYDLlyr, pSet);
                }
                else
                {
                    lyrMask.UseMasking = false;
                }
                CommonMethods.UsingMask = true;
                CommonMethods.MaskedLayer = maskingLyr;
                CommonMethods.MaskLayer = maskedLyr;
                m_Application.Workspace.Save();
                m_Application.ActiveView.Refresh();
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
        /// <summary>
        /// 获取要素类
        /// </summary>
        /// <param name="pws"></param>
        /// <param name="fclName"></param>
        /// <returns></returns>
        public static IFeatureClass GetFclViaWs(IWorkspace pws, string fclName)
        {
            try
            {
                IFeatureClass fcl = null;
                fcl = (pws as IFeatureWorkspace).OpenFeatureClass(fclName);
                return fcl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null;
            }
        }
    }
}

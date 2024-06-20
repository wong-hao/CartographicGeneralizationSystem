using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
using ESRI.ArcGIS.Geometry;
using System.Data;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 根据水库（GB=240101）的选取状态，更新与其相关的一般堤（GB=270102）/拦水坝（GB=270600）的显示状态
    /// 水库旁边的一般堤/拦水坝定义为与水库共线（存在叠置关系）的一般堤（270102）/拦水坝（270600），当水库的选取状态发生变化时，其旁边的一般堤/拦水坝的处理原则如下：
    /// （1）	当水库选取状态发生变化时，其旁边的一般堤的选取状态与之保持一致；
    /// （2）	当水库选取状态发生变化时，其旁边的拦水坝则分以下情况：
    /// ①当水库为选取状态时，则其旁边拦水坝也应选取，且表示为拦水坝符号；
    /// ②当水库状态为非选取状态时，则其旁边拦水坝的处理情况又分如下情况：若拦水坝两端都接连着道路，则将该拦水坝表示为相应道路符号（若两端同时存在同一类型的道路，则该拦水坝符号与之保持一致，否则表示为任一连接道路符号），否则该栏水坝也设置为非选取状态。
    /// 20191101：详见文档《20191018以往未解决的问题.docx》
    /// 20200908：堤坝（270102/270600）与水库的关系从叠置关系改为相交关系，详见文档《快速制图软件需求讨论后0904》
    /// </summary>
    public class DykeDamSelectCmdJS : SMGICommand
    {
        public DykeDamSelectCmdJS()
        {
            m_caption = "堤坝符号处理";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                      m_Application.Workspace != null &&
                      m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            int dykeGB = 270102, damGB = 270600;
            string selStateFN = "selectstate";
            string gbFN = "GB";

            IFeatureLayer hydaLayer = null, hfclLayer = null, lrdlLayer = null;
            int hydaSelStateIndex = -1, hfclSelStateIndex = -1, lrdlSelStateIndex = -1;
            int hydaGBIndex = -1, hfclGBIndex = -1;
            #region 获取相关图层
            string fcName = "HYDA";
            hydaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (hydaLayer == null)
            {
                MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            hydaSelStateIndex = hydaLayer.FeatureClass.FindField(selStateFN);
            if (hydaSelStateIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, selStateFN));
                return;
            }
            hydaGBIndex = hydaLayer.FeatureClass.FindField(gbFN);
            if (hydaGBIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, gbFN));
                return;
            }

            fcName = "HFCL";
            hfclLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (hfclLayer == null)
            {
                MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            hfclSelStateIndex = hfclLayer.FeatureClass.FindField(selStateFN);
            if (hfclSelStateIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, selStateFN));
                return;
            }
            hfclGBIndex = hfclLayer.FeatureClass.FindField(gbFN);
            if (hfclGBIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, gbFN));
                return;
            }


            fcName = "LRDL";
            lrdlLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lrdlLayer == null)
            {
                MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            lrdlSelStateIndex = lrdlLayer.FeatureClass.FindField(selStateFN);
            if (lrdlSelStateIndex == -1)
            {
                MessageBox.Show(string.Format("要素类【{0}】中找不到字段【{1}】", fcName, selStateFN));
                return;
            }
            #endregion

            int hfclRuleIDFieldIndex = -1;//HFCL图层中制图表达的规则ID字段索引
            int damRuleID = -1;//拦水坝在水系辅助图层中的规则ID
            #region 获取HFCL图层中制图表达相关信息
            IRepresentationRenderer rp = (hfclLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            if (rp == null || rp.RepresentationClass == null)
            {
                MessageBox.Show(string.Format("要素类【{0}】不存在有效的制图表达规则！", fcName));
                return;
            }
            hfclRuleIDFieldIndex = rp.RepresentationClass.RuleIDFieldIndex;

            damRuleID = GetRuleIDByRuleName(hfclLayer, damGB.ToString());
            if (damRuleID == -1)
            {
                MessageBox.Show(string.Format("要素类{0}中未找到拦水坝的规则ID！", hfclLayer.FeatureClass.AliasName));
                return;
            }
            #endregion

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = string.Format("GB = 240101");
                    IFeatureCursor hydaCursor = hydaLayer.FeatureClass.Search(qf, true);
                    IFeature hydaFe = null;
                    while ((hydaFe = hydaCursor.NextFeature()) != null)//遍历水库，处理相关的一般堤/拦水坝
                    {
                        if (hydaFe.Shape == null || hydaFe.Shape.IsEmpty)
                            continue;
                        IPolyline boundary = (hydaFe.Shape as ITopologicalOperator).Boundary as IPolyline;
                        if (boundary == null || boundary.IsEmpty)
                            continue;

                        bool isSelected = false;//水库的选取状态
                        object selStateVal = hydaFe.get_Value(hydaSelStateIndex);
                        if (Convert.IsDBNull(selStateVal))//已选取
                        {
                            isSelected = true;
                        }

                        #region 处理该水库相关联的堤坝
                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.WhereClause = string.Format("GB={0} or GB={1}", dykeGB, damGB);
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;//叠置-》改为相交2020.0908
                        sf.Geometry = boundary;

                        IFeatureCursor hfclCursor = hfclLayer.FeatureClass.Update(sf, true);
                        IFeature hfclFe = null;
                        while ((hfclFe = hfclCursor.NextFeature()) != null)
                        {
                            string hfclGB = hfclFe.get_Value(hfclGBIndex).ToString();
                            if (hfclGB == dykeGB.ToString())//处理符合条件的一般提：选取状态与其关联水库保持一致
                            {
                                if (isSelected)
                                {
                                    hfclFe.set_Value(hfclSelStateIndex, DBNull.Value);//设置为选取状态
                                }
                                else
                                {
                                    hfclFe.set_Value(hfclSelStateIndex, "未选取");//设置为未选取状态
                                }

                                hfclCursor.UpdateFeature(hfclFe);
                            }
                            else if (hfclGB == damGB.ToString())//处理符合条件的拦水坝
                            {
                                UpdateDamSelStateAndSymbol(hfclFe, hfclSelStateIndex, hfclRuleIDFieldIndex, damRuleID, isSelected, hfclLayer, lrdlLayer);

                                hfclCursor.UpdateFeature(hfclFe);

                            }
                        }
                        hfclCursor.Flush();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(hfclCursor);
                        #endregion
                    }
                    Marshal.ReleaseComObject(hydaCursor);
                }

                MessageBox.Show("已完成堤坝符号处理！");

                m_Application.EngineEditor.StopOperation("堤坝符号处理");

                m_Application.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                m_Application.EngineEditor.AbortOperation();
            }
        }

        //根据规则名称获取规则ID
        public static int GetRuleIDByRuleName(IFeatureLayer layer, string ruleName)
        {
            int id = -1;

            if(layer == null)
                return id;

            IRepresentationRenderer rp = (layer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            if(rp == null || rp.RepresentationClass == null)
                return id;

            IRepresentationClass repClass = rp.RepresentationClass;
            IRepresentationRules repRules = repClass.RepresentationRules;
            repRules.Reset();
            IRepresentationRule rule = null;
            int ruleID;
            while (true)//相等
            {
                repRules.Next(out ruleID, out rule);
                if (rule == null) break;

                if (repRules.get_Name(ruleID).Trim().Replace(" ", "") == ruleName.Trim().Replace(" ", ""))//不考虑空白
                {
                    id = ruleID;
                    break;
                }
            }

            if (id == -1)
            {
                repRules.Reset();
                while (true)//包含
                {
                    repRules.Next(out ruleID, out rule);
                    if (rule == null) break;

                    if (repRules.get_Name(ruleID).Contains(ruleName.Trim()))
                    {
                        id = ruleID;
                        break;
                    }
                }

            }

            return id;
        }
        
        //处理符合条件的拦水坝
        public static void UpdateDamSelStateAndSymbol(IFeature damFe, int hfclSelStateIndex, int hfclRuleIDFieldIndex, int damRuleID, 
            bool reservoirSelState, IFeatureLayer hfclLayer, IFeatureLayer lrdlLayer)
        {
            //1.道路图层无制图表达符号：则水库未选取时，拦水坝也不选取
            IRepresentationClass lrdlRepClass = null;
            IRepresentationRules lrdlRepRules = null;
            #region 获取LRDL图层中制图表达相关信息
            IRepresentationRenderer lrdlRepRenderer = (lrdlLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            if (lrdlRepRenderer != null && lrdlRepRenderer.RepresentationClass != null)
            {
                lrdlRepClass = lrdlRepRenderer.RepresentationClass;
                lrdlRepRules = lrdlRepClass.RepresentationRules;
            }
            #endregion
            if (lrdlRepRules == null)
            {
                damFe.set_Value(hfclSelStateIndex, "未选取");//设置为未选取状态

                return;
            }

            //2.当水库为选取状态时:其旁边拦水坝也应选取，且表示为拦水坝符号
            if (reservoirSelState)
            {
                damFe.set_Value(hfclSelStateIndex, DBNull.Value);//设置为选取状态
                damFe.set_Value(hfclRuleIDFieldIndex, damRuleID);

                return;
            }

            //2.当水库状态为非选取状态时,则其旁边拦水坝的处理情况又分如下情况：若拦水坝两端都接连着道路，则将该拦水坝表示为相应道路符号（若两端同时存在同一类型的道路，则该拦水坝符号与之保持一致，否则表示为任一连接道路符号），否则该栏水坝也设置为非选取状态
            List<string> upRoadRuleNameList = new List<string>();//拦水坝上游道路符号情况
            List<string> downRoadRuleNameList = new List<string>();//拦水坝下游道路符号情况
            #region 查询拦水坝上游道路的规则名
            var mc = new MapContextClass();
            mc.InitFromDisplay(GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation);

            IPolyline pl = damFe.Shape as IPolyline;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches;//邻接
            sf.WhereClause = string.Format("selectstate is null");

            sf.Geometry = pl.FromPoint;
            IFeatureCursor lrdlCursor = lrdlLayer.FeatureClass.Search(sf, true);
            IFeature lrdlFe = null;
            while ((lrdlFe = lrdlCursor.NextFeature()) != null)
            {
                IRepresentation lrdlRep = lrdlRepClass.GetRepresentation(lrdlFe, mc);
                if (!lrdlRepRules.Exists(lrdlRep.RuleID))
                    continue;

                string ruleName = lrdlRepRules.get_Name(lrdlRep.RuleID).Trim();

                upRoadRuleNameList.Add(ruleName);
            }
            Marshal.ReleaseComObject(lrdlCursor);
            #endregion
            #region 查询拦水坝下游道路的规则名
            sf.Geometry = pl.ToPoint;
            lrdlCursor = lrdlLayer.FeatureClass.Search(sf, true);
            while ((lrdlFe = lrdlCursor.NextFeature()) != null)
            {
                IRepresentation lrdlRep = lrdlRepClass.GetRepresentation(lrdlFe, mc);
                if (!lrdlRepRules.Exists(lrdlRep.RuleID))
                    continue;

                string ruleName = lrdlRepRules.get_Name(lrdlRep.RuleID).Trim();

                downRoadRuleNameList.Add(ruleName);
            }
            Marshal.ReleaseComObject(lrdlCursor);
            #endregion
            if (upRoadRuleNameList.Count > 0 && downRoadRuleNameList.Count > 0)
            {
                string matchRuleName = upRoadRuleNameList.First();
                foreach (var ruleName in upRoadRuleNameList)
                {
                    if (downRoadRuleNameList.Contains(ruleName))
                    {
                        matchRuleName = ruleName;
                        break;
                    }
                }

                int newRuleID = GetRuleIDByRuleName(hfclLayer, matchRuleName);
                
                damFe.set_Value(hfclSelStateIndex, DBNull.Value);//设置为选取状态
                damFe.set_Value(hfclRuleIDFieldIndex, newRuleID);
            }
            else//上下游没有都找到道路符号
            {
                damFe.set_Value(hfclSelStateIndex, "未选取");//设置为未选取状态
            }

            
        }
    }
}

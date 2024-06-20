using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 常规骑界生成
    /// </summary>
    public class NormalQJCmd : SMGICommand
    {
        public NormalQJCmd()
        {
            m_caption = "常规骑界生成";
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
            var frm = new NormalQJForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            IFeatureLayer qjlLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("QJL"))).FirstOrDefault() as IFeatureLayer;
            if (qjlLayer == null)
            {
                MessageBox.Show(string.Format("没有找到要素类【QJL】！"));
                return;
            }

            IFeatureLayer boulLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("BOUL"))).FirstOrDefault() as IFeatureLayer;
            if(boulLayer == null)
            {
                MessageBox.Show(string.Format("没有找到要素类【BOUL】！"));
                return;
            }
            IFeatureClass boulFC = boulLayer.FeatureClass;
            int gbIndex = boulFC.FindField("GB");

            IPolygon clipGeo = GetClipGeo();
            if (clipGeo == null)
            {
                MessageBox.Show(string.Format("没有找到裁切面！"));
                return;
            }
            IRelationalOperator areaRO = clipGeo as IRelationalOperator;//裁切面
            IRelationalOperator lineRO = (clipGeo as ITopologicalOperator).Boundary as IRelationalOperator;//裁切面边界线

            double bufferDist = 0.5;//mm

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    IFeatureClass qjlFC = qjlLayer.FeatureClass;
                    int ruleIDIndex = -1;
                    int ruleID = -1;
                    #region 制图表达ID
                    if ((qjlLayer as IGeoFeatureLayer).Renderer is IRepresentationRenderer)
                    {
                        IRepresentationRenderer reprenderer = (qjlLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                        if (reprenderer != null)
                        {
                            IRepresentationClass repClass = reprenderer.RepresentationClass;
                            if (repClass != null)
                            {
                                ruleIDIndex = repClass.RuleIDFieldIndex;

                                IRepresentationRules rules = repClass.RepresentationRules;
                                rules.Reset();
                                IRepresentationRule rule = null;
                                while (true)
                                {
                                    int id;
                                    rules.Next(out id, out rule);
                                    if (rule == null) break;

                                    if (rules.get_Name(id).Contains("骑界"))
                                    {
                                        ruleID = id;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    //清空原要素
                    (qjlFC as ITable).DeleteSearchedRows(null);

                    #region 主区处理
                    if (frm.BMainRegionQJ)
                    {
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = frm.MainRegionQJSql;
                        var feCursor = boulFC.Search(qf, true);
                        IFeature fe = null;
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            IPolyline boulGeo = fe.ShapeCopy as IPolyline;
                            if (boulGeo == null || boulGeo.IsEmpty)
                                continue;

                            IPoint center = new PointClass();
                            boulGeo.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);
                            if (!areaRO.Contains(center))
                                continue;//中心点不在裁切面范围内,判做邻区要素，不做处理

                            //该界线是否在裁切面边线上
                            bool bClipBoundary = true;
                            for (int i = 0; i < 5; i++)
                            {
                                boulGeo.QueryPoint(esriSegmentExtension.esriNoExtension, 0.2 * (i + 1), true, center);
                                IEnvelope env = new EnvelopeClass();
                                env.PutCoords(center.X - bufferDist * m_Application.ActiveView.FocusMap.ReferenceScale * 0.001, 
                                    center.Y - bufferDist * m_Application.ActiveView.FocusMap.ReferenceScale * 0.001,
                                    center.X + bufferDist * m_Application.ActiveView.FocusMap.ReferenceScale * 0.001,
                                    center.Y + bufferDist * m_Application.ActiveView.FocusMap.ReferenceScale * 0.001);
                                if (lineRO.Disjoint(env))//某段不在裁切面边线上
                                {
                                    bClipBoundary = false;
                                    break;
                                }
                            }
                            if (bClipBoundary)
                                continue;//在裁切面边线上，不做处理

                            //骑界宽度
                            double width = frm.MainRegionGB2QJWidth.Last().Value;
                            if (gbIndex != -1)
                            {
                                string gb = fe.get_Value(gbIndex).ToString();
                                switch (gb)
                                {
                                    case "630201":
                                        width = frm.MainRegionGB2QJWidth[630201];
                                        break;
                                    case "640201":
                                        width = frm.MainRegionGB2QJWidth[640201];
                                        break;
                                    case "650201":
                                        width = frm.MainRegionGB2QJWidth[650201];
                                        break;
                                    case "660201":
                                        width = frm.MainRegionGB2QJWidth[660201];
                                        break;
                                    default:
                                        break;
                                }
                            }

                            //新建骑界要素
                            var newFe = qjlFC.CreateFeature();
                            newFe.Shape = boulGeo;
                            if (ruleIDIndex != -1)
                            {
                                newFe.set_Value(ruleIDIndex, ruleID);
                            }
                            newFe.Store();

                            QJRepresentationOverSet(qjlLayer as IGeoFeatureLayer, newFe, frm.MainRegionQJColor, width);
                        }
                        Marshal.ReleaseComObject(feCursor);
                    }

                    #endregion

                    #region 邻区处理
                    if (frm.BAdjRegionQJ)
                    {
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = frm.AdjRegionQJSql;
                        var feCursor = boulFC.Search(qf, true);
                        IFeature fe = null;
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            IPolyline boulGeo = fe.ShapeCopy as IPolyline;
                            if (boulGeo == null || boulGeo.IsEmpty)
                                continue;

                            IPoint center = new PointClass();
                            boulGeo.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);
                            if (areaRO.Contains(center))
                                continue;//中心点在裁切面范围内,判做主区要素，不做处理

                            //该界线是否在裁切面边线上
                            bool bClipBoundary = true;
                            for (int i = 0; i < 5; i++)
                            {
                                boulGeo.QueryPoint(esriSegmentExtension.esriNoExtension, 0.2 * (i + 1), true, center);
                                IEnvelope env = new EnvelopeClass();
                                env.PutCoords(center.X - bufferDist * m_Application.ActiveView.FocusMap.ReferenceScale * 0.001,
                                    center.Y - bufferDist * m_Application.ActiveView.FocusMap.ReferenceScale * 0.001,
                                    center.X + bufferDist * m_Application.ActiveView.FocusMap.ReferenceScale * 0.001,
                                    center.Y + bufferDist * m_Application.ActiveView.FocusMap.ReferenceScale * 0.001);
                                if (lineRO.Disjoint(env))//某段不在裁切面边线上
                                {
                                    bClipBoundary = false;
                                    break;
                                }
                            }
                            if (bClipBoundary)
                                continue;//在裁切面边线上，不做处理

                            //骑界宽度
                            double width = frm.AdjRegionGB2QJWidth.Last().Value;
                            if (gbIndex != -1)
                            {
                                string gb = fe.get_Value(gbIndex).ToString();
                                switch (gb)
                                {
                                    case "630201":
                                        width = frm.AdjRegionGB2QJWidth[630201];
                                        break;
                                    case "640201":
                                        width = frm.AdjRegionGB2QJWidth[640201];
                                        break;
                                    case "650201":
                                        width = frm.AdjRegionGB2QJWidth[650201];
                                        break;
                                    case "660201":
                                        width = frm.AdjRegionGB2QJWidth[660201];
                                        break;
                                    default:
                                        break;
                                }
                            }

                            //新建骑界要素
                            var newFe = qjlFC.CreateFeature();
                            newFe.Shape = boulGeo;
                            if (ruleIDIndex != -1)
                            {
                                newFe.set_Value(ruleIDIndex, ruleID);
                            }
                            newFe.Store();

                            QJRepresentationOverSet(qjlLayer as IGeoFeatureLayer, newFe, frm.AdjRegionQJColor, width);
                        }
                        Marshal.ReleaseComObject(feCursor);
                    }
                    #endregion

                }

                m_Application.EngineEditor.StopOperation("常规骑界生成");

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

        private void QJRepresentationOverSet(IGeoFeatureLayer qjLayer, IFeature fe, ICmykColor cmyk, double dist)
        {
            int widthIndex = fe.Fields.FindField("Width");
            if (widthIndex != -1)//重写覆盖字段
            {
                fe.set_Value(widthIndex, dist * 2.83);
                fe.Store();
            }

            IMapContext mctx = new MapContextClass();
            mctx.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, qjLayer.AreaOfInterest);

            IRepresentationRenderer repRender = qjLayer.Renderer as IRepresentationRenderer;
            IRepresentationClass rpc = repRender.RepresentationClass;

            var rep = rpc.GetRepresentation(fe, mctx);
            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);

            for (int k = 0; k < ruleOrg.LayerCount; k++)
            {
                IBasicLineSymbol lineSym = ruleOrg.get_Layer(k) as IBasicLineSymbol;
                if (lineSym != null)//影线模式
                {
                    ILineStroke fillPattern = lineSym.Stroke;
                    IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                    if (fillAttrs != null)
                    {
                        rep.set_Value(fillAttrs, 3, cmyk);
                    }
                    
                }
            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
            rep.UpdateFeature();
            rep.Feature.Store();
        }

        public static IPolygon GetClipGeo()
        {
            var layer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "CLIPBOUNDARY");
            })).ToArray().First();
            if (layer == null)
            {
                return null;
            }
            IFeatureClass fc = (layer as IFeatureLayer).FeatureClass;

            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE ='裁切面'";
            IFeatureCursor feCursor = fc.Search(qf, false);
            IFeature fe = feCursor.NextFeature();
            if (fe == null)
            {
                return null;
            }
            Marshal.ReleaseComObject(feCursor);

            return fe.ShapeCopy as IPolygon;

        }
    }
}

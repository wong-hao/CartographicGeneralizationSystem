using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class VertexDeleteTool : SMGITool
    {
        double searchRadius = 50;
        public VertexDeleteTool()
        {
            m_caption = "删除节点";
            m_toolTip = "鼠标左键拉框选节点删除";

            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "VertexDel.cur"));

            NeedSnap = true;
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing && m_Application.EngineEditor.CurrentTask.UniqueName == "ControlToolsEditing_ModifyFeatureTask";
            }
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (1 == button)
            {
                IEnvelope pEnvelope = m_Application.MapControl.TrackRectangle();
                if (!pEnvelope.IsEmpty)
                {
                    List<IPoint> selectedVertexs = new List<IPoint>();
                    var trackPolyline = new PolygonClass();
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.UpperLeft);
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.UpperRight);
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.LowerRight);
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.LowerLeft);
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.UpperLeft);

                    //将选择框内的节点标记为选择状态
                    IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
                    IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
                    if (ptcoll != null)
                    {
                        for (int i = 0; i < ptcoll.PointCount; ++i)
                        {
                            IPoint pt = ptcoll.get_Point(i);
                            if (trackPolyline.Contains(pt))
                            {
                                selectedVertexs.Add(pt);
                            }
                        }

                        if (selectedVertexs.Count == 0)
                            return;

                        foreach (var p in selectedVertexs)
                        {
                            deleteVertex(p);
                        }

                        //刷新地图
                        GApplication.Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, pEnvelope);
                    }
                    
                }
                else
                {
                    IPoint clickedPt = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                    bool res = deleteVertex(clickedPt);

                    //没有删除节点，且不在选中要素附近
                    if (!res)
                    {
                        IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
                        IProximityOperator ProxiOP = (editsketch.Geometry) as IProximityOperator;
                        if (ProxiOP.ReturnDistance(clickedPt) > m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(5))
                        {
                            //切换回编辑状态
                            if (m_Application.PluginManager.Commands.ContainsKey("SMGI.Plugin.GeneralEdit.EditSelector"))
                            {
                                PluginCommand cmd = m_Application.PluginManager.Commands["SMGI.Plugin.GeneralEdit.EditSelector"];
                                if (cmd != null && cmd.Enabled)
                                {
                                    m_Application.MapControl.CurrentTool = cmd.Command as ITool;

                                    m_Application.ActiveView.FocusMap.ClearSelection();
                                }

                            }
                        }

                        
                    }

                }
            }
        }

        public bool deleteVertex(IPoint clickedPt)
        {
            IEngineEditSketch m_pSketch = m_Application.EngineEditor as IEngineEditSketch;
            //1 获取点击参数
            IHitTest hitShape = (IHitTest)m_pSketch.Geometry;
            IPoint hitPoint = new PointClass();
            double hitDistance = 0;
            int hitPartIndex = 0;
            int hitSegmentIndex = 0;
            bool bRightSide = false;
            esriGeometryHitPartType hitPartType = esriGeometryHitPartType.esriGeometryPartNone;
            //2 节点判断
            hitPartType = esriGeometryHitPartType.esriGeometryPartVertex;
            bool isTrue = hitShape.HitTest(clickedPt, searchRadius, hitPartType, hitPoint, ref hitDistance,
                                           ref hitPartIndex, ref hitSegmentIndex, ref bRightSide);
            //3 删除节点 
            if (isTrue)
            {
                //3.1 草图操作开始
                IEngineSketchOperation pSketchOp = new EngineSketchOperationClass();
                pSketchOp.Start(m_Application.EngineEditor);
                pSketchOp.SetMenuString("Delete Vertex (Custom)");
                //3.2 获取点串
                IGeometryCollection pGeoCol = (IGeometryCollection)m_pSketch.Geometry;
                IPointCollection pPathOrRingPtCol = (IPointCollection)pGeoCol.get_Geometry(hitPartIndex);
                //3.3 删除节点
                object missing = Type.Missing;
                object partIndexObject = hitPartIndex;
                pPathOrRingPtCol.RemovePoints(hitSegmentIndex, 1);
                //4.4 移除旧的，添加新的
                pGeoCol.RemoveGeometries(hitPartIndex, 1);
                pGeoCol.AddGeometry((IGeometry)pPathOrRingPtCol, ref partIndexObject, ref missing);
                //4.5 草图操作完成
                esriEngineSketchOperationType opType =
                    esriEngineSketchOperationType.esriEngineSketchOperationVertexDeleted;
                pSketchOp.Finish(null, opType, hitPoint);
            }

            return isTrue;
        }
    }
}

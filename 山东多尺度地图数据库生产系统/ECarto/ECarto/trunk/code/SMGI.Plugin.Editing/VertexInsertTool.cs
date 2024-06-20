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
    public class VertexInsertTool : SMGITool
    {
        double searchRadius = 50;
        public VertexInsertTool()
        {
            m_caption = "插入节点";
            m_toolTip = "支持单击插入";

            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "VertexAdd.cur"));

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
            if (button == 1)
            {
                //转成地图坐标
                IPoint clickedPt = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                IEngineEditSketch m_pSketch = m_Application.EngineEditor as IEngineEditSketch;
                if (button == 1)
                {
                    #region 1 获取点击测试参数
                    IHitTest hitShape = (IHitTest)m_pSketch.Geometry;
                    IPoint hitPoint = new PointClass();
                    double hitDistance = 0;
                    int hitPartIndex = 0;
                    int hitSegmentIndex = 0;
                    bool bRightSide = false;
                    esriGeometryHitPartType hitPartType = esriGeometryHitPartType.esriGeometryPartNone;
                    double searchRadius = 50;
                    #endregion

                    //2 节点判断
                    hitPartType = esriGeometryHitPartType.esriGeometryPartVertex;
                    bool isTrue = hitShape.HitTest(clickedPt, searchRadius, hitPartType, hitPoint, ref hitDistance,
                                                   ref hitPartIndex, ref hitSegmentIndex, ref bRightSide);
                    if (isTrue) return; //已存在节点，不需要添加
                    //3 点击测试
                    hitPartType = esriGeometryHitPartType.esriGeometryPartBoundary;
                    isTrue = hitShape.HitTest(clickedPt, searchRadius, hitPartType, hitPoint, ref hitDistance,
                                              ref hitPartIndex, ref hitSegmentIndex, ref bRightSide);
                    //4 添加节点 
                    if (isTrue)
                    {
                        //4.1 草图操作开始
                        IEngineSketchOperation pSketchOp = new EngineSketchOperationClass();
                        pSketchOp.Start(m_Application.EngineEditor);
                        pSketchOp.SetMenuString("Insert Vertex (Custom)");
                        //4.2 获取点串
                        IGeometryCollection pGeoCol = (IGeometryCollection)m_pSketch.Geometry;
                        IPointCollection pPathOrRingPtCol = (IPointCollection)pGeoCol.get_Geometry(hitPartIndex);
                        //4.3 插入节点
                        object missing = Type.Missing;
                        object hitSegmentIndexObject = hitSegmentIndex;
                        object partIndexObject = hitPartIndex;
                        pPathOrRingPtCol.AddPoint(hitPoint, ref missing, ref hitSegmentIndexObject);
                        //4.4 移除旧的，添加新的
                        pGeoCol.RemoveGeometries(hitPartIndex, 1);
                        pGeoCol.AddGeometry((IGeometry)pPathOrRingPtCol, ref partIndexObject, ref missing);
                        //4.5 草图操作完成
                        esriEngineSketchOperationType opType =
                            esriEngineSketchOperationType.esriEngineSketchOperationVertexAdded;
                        pSketchOp.Finish(null, opType, hitPoint);
                    }
                }
            }
        }

    

    }
}

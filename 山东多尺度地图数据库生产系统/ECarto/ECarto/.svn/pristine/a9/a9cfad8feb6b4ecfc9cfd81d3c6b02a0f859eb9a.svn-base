using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.esriSystem;

using ESRI.ArcGIS.Geodatabase;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace SMGI.Plugin.EmergencyMap
{
  public class AnnoRotation: SMGI.Common.SMGITool
    {
        /// <summary>
        /// 本工具中的选择环境
        /// </summary>
        private ISelectionEnvironment m_SelectionEnvironment;
        /// <summary>
        /// 默认的选择环境的选择框颜色，本工具完成之后需要重置本颜色
        /// </summary>
        private IRgbColor defaultSelectEnviColor;
        /// <summary>
        /// 选中的注记要素
        /// </summary>
        private IElement m_Selectelement;
        /// <summary>
        /// 是否可以移动
        /// </summary>
        private bool m_moving;
        /// <summary>
        /// 鼠标点击下去（角度初始值），鼠标点与注记中心点的线角度
        /// </summary>
        private double m_OldAngle;
        /// <summary>
        /// 注记元素的中心点
        /// </summary>
        private IPoint m_CenterPoint;
        /// <summary>
        /// 鼠标移动时的前一点
        /// </summary>
        private IPoint m_OldPoint;
        /// <summary>
        /// 旋转显示的可视元素
        /// </summary>
        private IElement m_RotateElement;
        /// <summary>
        /// 注记要素图层
        /// </summary>
        public IFeatureLayer m_AnnotationLayer;
        public AnnoRotation()
        {
            m_caption = "注记旋转";
            m_toolTip = "选中注记，拖动鼠标旋转";
            m_category = "注记编辑";
            NeedSnap = false;
            //m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "size.cur"));
            // pMapControl = m_Application.MapControl;
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            m_SelectionEnvironment = new SelectionEnvironmentClass();
            defaultSelectEnviColor = m_SelectionEnvironment.DefaultColor as IRgbColor;//保存系统默认的颜色
            //将环境选择元素置为红色
            IRgbColor pColor = new RgbColor();
            pColor.Red = 255;
            m_SelectionEnvironment.DefaultColor = pColor;
            //清楚已经选中的状态
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
            m_Application.MapControl.Map.ClearSelection();
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
        }
        public override bool Deactivate()
        {
           // m_SelectionEnvironment.DefaultColor = defaultSelectEnviColor;
            m_Application.MapControl.Map.ClearSelection();
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            return true;
        }
        private IElement GetQueryAnnoEle(IPoint geometry)
        {
            
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IFDOGraphicsLayer;
            })).ToArray();
            ISpatialFilter sf=new  SpatialFilterClass();
            sf.Geometry=geometry;
            sf.SpatialRel=esriSpatialRelEnum.esriSpatialRelIntersects;
            foreach (var lyr in lyrs)
            {
                if ((lyr as IFeatureLayer).FeatureClass.FeatureCount(sf) > 0)
                {
                    var cursor = (lyr as IFeatureLayer).FeatureClass.Search(sf as IQueryFilter, false);
                    IFeature fe = cursor.NextFeature();
                    Marshal.ReleaseComObject(cursor);
                    return (fe as IAnnotationFeature).Annotation ;
                }
              
            }
            return null;
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1&&this.Enabled)
            {
                IGraphicsContainer pGraphicsContainer = m_Application.ActiveView as IGraphicsContainer;
                m_OldPoint = ToSnapedMapPoint(x, y);
             
                
                //pGraphicsContainer = m_Application.MapControl.ActiveView as IGraphicsContainer;
                //IEnumElement pEnumElement = pGraphicsContainer.LocateElements(m_OldPoint, 2.0e-3 * m_Application.MapControl.Map.MapScale);
                //if (pEnumElement == null) return;
                //m_Selectelement = pEnumElement.Next();


               // System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumElement);
                m_Selectelement= GetQueryAnnoEle(m_OldPoint);
                if (m_Selectelement == null) return;
                if (m_Selectelement is AnnotationElement)
                { 
                    //计算元素的中心点
                    if (!m_Selectelement.Geometry.IsEmpty)
                    {
                        m_CenterPoint = new ESRI.ArcGIS.Geometry.Point()
                        {
                            X = (m_Selectelement.Geometry.Envelope.XMax + m_Selectelement.Geometry.Envelope.XMin) / 2.0,
                            Y = (m_Selectelement.Geometry.Envelope.YMax + m_Selectelement.Geometry.Envelope.YMin) / 2.0
                        };
                    }
                    else
                    {
                        IGroupElement pGroupElement = m_Selectelement as IGroupElement;
                        m_RotateElement = pGroupElement as IElement;
                        IDisplay currentDisplay = m_Application.ActiveView.ScreenDisplay as IDisplay;
                        IPolygon polygon = new PolygonClass();
                        m_RotateElement.QueryOutline(currentDisplay, polygon);
                        m_CenterPoint = new ESRI.ArcGIS.Geometry.Point()
                        {
                            X = (polygon.Envelope.XMax + polygon.Envelope.XMin) / 2.0,
                            Y = (polygon.Envelope.YMax + polygon.Envelope.YMin) / 2.0
                        };
                    }

                    // 获取旧点角度
                    IPointCollection pPointCollection = new PolylineClass();
                    object missing = Type.Missing;
                    pPointCollection.AddPoint(m_CenterPoint, ref missing, ref missing);
                    pPointCollection.AddPoint(m_OldPoint, ref missing, ref missing);
                    m_OldAngle = GetAngle(pPointCollection as IPolyline);


                    if (m_Selectelement is IGroupElement)
                    {
                        IGroupElement pGroupElement = ((m_Selectelement as IGroupElement) as IClone).Clone() as IGroupElement;
                        m_RotateElement = pGroupElement as IElement;
                    }
                    else
                    {
                        ITextElement pTextElement = ((m_Selectelement as ITextElement) as IClone).Clone() as ITextElement;
                        m_RotateElement = pTextElement as IElement;
                        m_RotateElement.Geometry = m_Selectelement.Geometry;
                    }
                    m_Application.ActiveView.GraphicsContainer.AddElement(m_RotateElement, 99);
                    m_Application.MapControl.Map.SelectByShape(m_CenterPoint, null, true); //选择图形！
                    //m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphicSelection, null, null);
                    //m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                    // 移动状态信息
                    m_moving = true;
                }
            }
        }
       public override void OnMouseMove(int button, int shift, int x, int y)
        {
           if (button == 1&&this.Enabled)
            {
                if (!m_moving) return;
                if (m_Selectelement is AnnotationElement)
                {
                    // 新点为当前的鼠标点坐标
                    IPoint newPoint = ToSnapedMapPoint(x, y);
                    //临时获取旧角度
                    IPointCollection pPointCollection = new PolylineClass();
                    object missing = Type.Missing;
                    pPointCollection.AddPoint(m_CenterPoint, ref missing, ref missing);
                    pPointCollection.AddPoint(m_OldPoint, ref missing, ref missing);
                    double tOldAngle = GetAngle(pPointCollection as IPolyline);
                    //临时获取新点角度
                    pPointCollection = new PolylineClass();
                    pPointCollection.AddPoint(m_CenterPoint, ref missing, ref missing);
                    pPointCollection.AddPoint(newPoint, ref missing, ref missing);
                    double tNewAngle = GetAngle(pPointCollection as IPolyline);
                    // 旋转Element,角度为新旧点之差
                    ITransform2D pTransform2D = m_RotateElement as ITransform2D;
                   //pTransform2D.
                    pTransform2D.Rotate(m_CenterPoint, tNewAngle - tOldAngle);
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                    // 更新旧点变量
                    m_OldPoint = newPoint;
                }
            }
        }
       /// <summary>
       /// 获取polyline切线方向的角度（弧度）
       /// </summary>
       /// <param name="pPolyline">线</param>
       /// <returns></returns>
       private double GetAngle(IPolyline pPolyline)
       {
           //IPolycurve pPolycurve;
           ILine pTangentLine = new Line();
           pPolyline.QueryTangent(esriSegmentExtension.esriNoExtension, 0.5, true, pPolyline.Length, pTangentLine);
           Double radian = pTangentLine.Angle;
           Double angle = radian * 180 / Math.PI;
           // 如果要设置正角度执行以下方法
           while (angle < 0)
           {
               angle = angle + 360;
           }
           radian = angle * Math.PI / 180;
           // 返回弧度
           return radian;
       }
       public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (button == 1 && this.Enabled)
            {
                if (m_Selectelement is AnnotationElement)
                {
                    //IPoint centerPoint = m_Selectelement.Geometry as IPoint;
                    if (m_AnnotationLayer == null)
                    {
                        IFeatureClass eleFC = (m_Selectelement as IAnnotationElement).Feature.Class as IFeatureClass;
                        var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return l is IFDOGraphicsLayer && ((l as IFeatureLayer).FeatureClass as IDataset).Name == (eleFC as IDataset).Name;

                        })).ToArray();

                        if (lyrs == null || lyrs.Length == 0)
                        {
                            return;
                        }
                        ILayer pLayer = lyrs[0];
                        m_AnnotationLayer = pLayer as IFeatureLayer;
                    }
                    //IFeatureClass pFeatureClass = m_AnnotationLayer.FeatureClass;
                    //IDataset pDataset = (IDataset)pFeatureClass;
                    //IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDataset.Workspace;
                    m_Application.EngineEditor.EnableUndoRedo(true);
                    m_Application.EngineEditor.StartOperation();
                    //pWorkspaceEdit.StartEditOperation();
                    //pWorkspaceEdit.EnableUndoRedo();
                    IPoint newPoint = ToSnapedMapPoint(x, y);
                    IPointCollection pPointCollection = new PolylineClass();
                    object missing = Type.Missing;
                    pPointCollection.AddPoint(m_CenterPoint, ref missing, ref missing);
                    pPointCollection.AddPoint(m_OldPoint, ref missing, ref missing);
                    double newAngle = GetAngle(pPointCollection as IPolyline);
                    /*这种判断方式有问题，当初始角度都为0的时候，是没有问题，但如果初始角度不为0，则就会乱掉
                    double angle = ((ITextElement)m_RotateElement).Symbol.Angle;
                    double oldAngle = ((ITextElement)m_Selectelement).Symbol.Angle;
                    */
                    ITransform2D pTransform2D = m_Selectelement as ITransform2D;
                    pTransform2D.Rotate(m_CenterPoint,newAngle-m_OldAngle);
                    IGraphicsContainer pGraphicsContainer = m_Application.ActiveView as IGraphicsContainer;
                    pGraphicsContainer.UpdateElement(m_Selectelement);
                    //pWorkspaceEdit.DisableUndoRedo();
                   // pWorkspaceEdit.StopEditOperation();
                    
                    m_Application.EngineEditor.StopOperation("注记旋转");
                    pGraphicsContainer.DeleteElement(m_RotateElement);
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphicSelection, null, null);
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
                m_moving = false;
                m_Selectelement = null;
                
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.AnnotationEngine
{
    /// <summary>
    /// 注记旋转（LZ）
    /// </summary>
    public class AnnotationRotateTool : SMGI.Common.SMGITool
    {
        private IAnnotationFeature _selectFeature; //当前选择的注记要素
        private IElement _selectAnnoElement; //当前选择的注记部件
        private IPoint _centerPoint;// 注记元素的中心点
        private IPoint _lastPoint;// 上一点
        private double _orginAngle;//原始角度
        private IElement _tempRotateElement;// 旋转显示的临时可视元素

        private IMovePointFeedback _movePointFeedback; //移点反馈
        private bool _isPointMoving; //是否处于点移动状态

        private int _selPartIndex;//被选多部件索引

        public AnnotationRotateTool()
        {
            m_caption = "注记旋转";
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
            _selectFeature = null;
        }

        public override bool Deactivate()
        {
            _selectFeature = null;
            m_Application.MapControl.Map.ClearSelection();
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.MapControl.ActiveView.Extent);
            return true;
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;

            var pt = ToSnapedMapPoint(x, y);

            if (_selectFeature == null)
            {
                var fe = GetAnnoFeByPoint(pt, true);
                if (fe == null)
                    return;

                //初始化目标要素
                InitObjAnnoFeature(fe, pt);
            }
            else
            {
                var fe = GetAnnoFeByPoint(pt, true);
                int index = getSelTextPartIndex(fe, pt);
                if (fe != null && (_selPartIndex != index || 
                    fe.Class.ObjectClassID != (_selectFeature as IFeature).Class.ObjectClassID || fe.OID != (_selectFeature as IFeature).OID))//当前选中的要素非空，且(多部件不同或目标要素不同）
                {
                    //切换目标要素
                    InitObjAnnoFeature(fe, pt);
                }

            }

            double trackDis = m_Application.ConvertPixelsToMapUnits(4);
            if(_selPartIndex != -1)
                trackDis = m_Application.ConvertPixelsToMapUnits(2); 
            if (Math.Abs(pt.X - _centerPoint.X) <= trackDis && Math.Abs(pt.Y - _centerPoint.Y) <= trackDis)//位于中心点附近
            {
                _movePointFeedback.Refresh(0);
                _movePointFeedback.Start(_centerPoint, pt);
                _isPointMoving = true;
            }
            else
            {
                _lastPoint = ToSnapedMapPoint(x, y);

                IPointCollection pc = new PolylineClass();
                pc.AddPoint(_centerPoint);
                pc.AddPoint(_lastPoint);
                _orginAngle = GetTangentAngle(pc as IPolyline);
                _tempRotateElement = (IElement)((IClone)_selectAnnoElement).Clone();
                if (!(_selectAnnoElement is IGroupElement))
                {
                    //_tempRotateElement.Geometry = _selectAnnoElement.Geometry;
                }
                m_Application.ActiveView.GraphicsContainer.AddElement(_tempRotateElement, 99);
            }

        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (button != 1 || _selectFeature == null)
                return;

            var pt = ToSnapedMapPoint(x, y);


            if (_isPointMoving)
            {
                _movePointFeedback.MoveTo(pt);
            }
            else
            {
                if (_tempRotateElement != null && _centerPoint != null && _lastPoint != null)
                {
                    IPointCollection pca = new PolylineClass();
                    pca.AddPoint(_centerPoint);
                    pca.AddPoint(_lastPoint);
                    var oldAngle = GetTangentAngle(pca as IPolyline);

                    IPointCollection pcb = new PolylineClass();
                    pcb.AddPoint(_centerPoint);
                    pcb.AddPoint(pt);
                    var newAngle = GetTangentAngle(pcb as IPolyline);

                    (_tempRotateElement as ITransform2D).Rotate(_centerPoint, newAngle - oldAngle);
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                    _lastPoint = pt;
                }
            }
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;

            if (_isPointMoving)
            {
                _centerPoint = _movePointFeedback.Stop();
                m_Application.ActiveView.Refresh();
                _isPointMoving = false;
            }
            else
            {
                if (_selectAnnoElement != null && _centerPoint != null && _lastPoint != null)
                {
                    

                    IPointCollection pc = new PolylineClass();
                    pc.AddPoint(_centerPoint);
                    pc.AddPoint(_lastPoint);
                    var angle = GetTangentAngle(pc as IPolyline);

                    double delta = angle - _orginAngle;
                    if(Math.Abs(delta) > 0.00001)
                    {
                        m_Application.EngineEditor.StartOperation();

                        (_selectAnnoElement as ITransform2D).Rotate(_centerPoint, delta);
                        if (_selPartIndex != -1)
                        {
                            var multiPartTextElement = _selectFeature.Annotation as IMultiPartTextElement;

                            //更新文本几何
                            multiPartTextElement.ReplacePart(_selPartIndex, (_selectAnnoElement as ITextElement).Text, _selectAnnoElement.Geometry);
                            _selectFeature.Annotation = multiPartTextElement as IElement;
                            (_selectFeature as IFeature).Store();
                        }
                        else
                        {
                            _selectFeature.Annotation = _selectAnnoElement;
                            (_selectFeature as IFeature).Store();

                            (m_Application.ActiveView as IGraphicsContainer).UpdateElement(_selectFeature.Annotation); 
                        }

                        m_Application.EngineEditor.StopOperation("注记旋转");
                    }

                    if (_tempRotateElement != null)
                    {
                        (m_Application.ActiveView as IGraphicsContainer).DeleteElement(_tempRotateElement);
                        _tempRotateElement = null;
                    }
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
            }

            
        }

        public override void Refresh(int hdc)
        {
            ISymbol markerSymbol = new SimpleMarkerSymbolClass { Size = 8, Color = new RgbColorClass { Red = 223, Green = 115, Blue = 255 } };

            IDisplay simDis = new SimpleDisplayClass { DisplayTransformation = m_Application.ActiveView.ScreenDisplay.DisplayTransformation };
            simDis.DisplayTransformation.ReferenceScale = 0;
            simDis.StartDrawing(hdc, -1);
            simDis.SetSymbol(markerSymbol);
            simDis.DrawPoint(_centerPoint);
            simDis.FinishDrawing();
        }

        //初始化目标要素的相关变量
        private void InitObjAnnoFeature(IFeature fe, IPoint pt)
        {
            _selectFeature = fe as IAnnotationFeature;
            if (_selectFeature.Annotation is IMultiPartTextElement && (_selectFeature.Annotation as IMultiPartTextElement).PartCount > 1)
            {
                _selPartIndex = -1;
                _selectAnnoElement = null;
                _centerPoint = null;

                var multiPartTextElement = _selectFeature.Annotation as IMultiPartTextElement;
                IRelationalOperator ro = pt as IRelationalOperator;

                var gc = fe.ShapeCopy as IGeometryCollection;
                for (int i = 0; i < gc.GeometryCount; ++i)
                {
                    IPolygon partGeo = new PolygonClass();
                    (partGeo as IPointCollection).AddPointCollection(gc.get_Geometry(i) as IPointCollection);
                    (partGeo as ITopologicalOperator).Simplify();

                    if (ro.Within(partGeo))
                    {
                        #region 如何有效的根据要素部件几何确定text部件索引？？？
                        for (int j = 0; j < multiPartTextElement.PartCount; ++j)
                        {
                            var partEle = multiPartTextElement.QueryPart(j) as IElement;
                            IRelationalOperator textRO = null;
                            if (partEle.Geometry is IPoint)
                            {
                                textRO = partEle.Geometry as IRelationalOperator;
                            }
                            else if (partEle.Geometry is IPolyline)
                            {
                                var centerPT = new PointClass();
                                (partEle.Geometry as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerPT);

                                textRO = centerPT as IRelationalOperator;
                            }
                            else
                            {
                                textRO = partEle.Geometry as IRelationalOperator;
                            }


                            if (textRO.Within(partGeo))//有缺陷
                            {
                                _selPartIndex = j;
                                _selectAnnoElement = partEle;
                                _centerPoint = (partGeo as IArea).Centroid;
                                break;
                            }
                        }

                        #endregion


                        if (_selectAnnoElement != null)
                            break;
                    }
                }
            }
            else
            {
                _selPartIndex = -1;
                _selectAnnoElement = _selectFeature.Annotation;
                _centerPoint = (fe.Shape as IArea).Centroid;
            }
            _lastPoint = null;

            _movePointFeedback = new MovePointFeedbackClass { Display = m_Application.ActiveView.ScreenDisplay };
        }

        //选中的目标要素部件索引
        private int getSelTextPartIndex(IFeature fe, IPoint pt)
        {
            int index = -1;

            var annoFe = fe as IAnnotationFeature;
            if (annoFe != null && annoFe.Annotation is IMultiPartTextElement && (annoFe.Annotation as IMultiPartTextElement).PartCount > 1)
            {
                var multiPartTextElement = annoFe.Annotation as IMultiPartTextElement;
                IRelationalOperator ro = pt as IRelationalOperator;

                var gc = fe.ShapeCopy as IGeometryCollection;
                for (int i = 0; i < gc.GeometryCount; ++i)
                {
                    IPolygon partGeo = new PolygonClass();
                    (partGeo as IPointCollection).AddPointCollection(gc.get_Geometry(i) as IPointCollection);
                    (partGeo as ITopologicalOperator).Simplify();

                    if (ro.Within(partGeo))
                    {
                        #region 如何有效的根据要素部件几何确定text部件索引？？？
                        for (int j = 0; j < multiPartTextElement.PartCount; ++j)
                        {
                            var partEle = multiPartTextElement.QueryPart(j) as IElement;
                            IRelationalOperator textRO = null;
                            if (partEle.Geometry is IPoint)
                            {
                                textRO = partEle.Geometry as IRelationalOperator;
                            }
                            else if (partEle.Geometry is IPolyline)
                            {
                                var centerPT = new PointClass();
                                (partEle.Geometry as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerPT);

                                textRO = centerPT as IRelationalOperator;
                            }
                            else
                            {
                                textRO = partEle.Geometry as IRelationalOperator;
                            }


                            if (textRO.Within(partGeo))//有缺陷
                            {
                                index = j;
                                break;
                            }
                        }

                        #endregion


                        if (index != -1)
                            break;
                    }
                }
            }

            return index;
        }

        //点选获取注记元素
        private IFeature GetAnnoFeByPoint(IGeometry geo, bool needSelected)
        {
            IFeature selectAnnoFe = null;

            var layers = GApplication.Application.Workspace.LayerManager.GetLayer(l => l is IFDOGraphicsLayer).ToArray();

            ISpatialFilter sf = new SpatialFilterClass { Geometry = geo, SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects };
            foreach (var layer in layers)
            {
                var fc = (layer as IFeatureLayer).FeatureClass;
                if (fc.FeatureCount(sf) == 0) 
                    continue;

                var feCursor = (layer as IFeatureLayer).Search(sf, false);
                IFeature fe = null;
                while((fe=feCursor.NextFeature()) != null)
                {
                    if (fe is IAnnotationFeature && (fe as IAnnotationFeature).Annotation is AnnotationElement &&
                            (fe as IAnnotationFeature2).Status != esriAnnotationStatus.esriAnnoStatusUnplaced)
                    {
                        selectAnnoFe = fe;
                        break;
                    }
                }
                Marshal.ReleaseComObject(feCursor);

                if (selectAnnoFe != null)
                {
                    if (needSelected)
                    {
                        m_Application.MapControl.Map.ClearSelection();

                        GApplication.Application.MapControl.Map.SelectFeature(layer, fe);

                        m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                    }

                    break;
                }
            }

            return selectAnnoFe;
        }

        //获取线切线方向的角度（弧度）
        private double GetTangentAngle(IPolyline line)
        {
            ILine outline = new Line();
            line.QueryTangent(esriSegmentExtension.esriNoExtension, 0.5, true, line.Length, outline);
            var radian = outline.Angle;
            var angle = radian * 180 / Math.PI;

            while (angle < 0)
            {
                angle = angle + 360;
            }
            radian = angle * Math.PI / 180;
            return radian;
        }
    }
}

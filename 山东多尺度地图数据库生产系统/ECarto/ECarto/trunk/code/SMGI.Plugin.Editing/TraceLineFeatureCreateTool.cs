using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
     
   
    public class TraceLineFeatureCreateTool: SMGI.Common.SMGITool
    {
        /// <summary>
        /// 追踪线创建
        /// </summary>
        public TraceLineFeatureCreateTool()
        {
            m_caption = "追踪线创建";
            m_toolTip = "鼠标左键点击地图直接绘制点；当需要追踪要素时，通过Ctrl+鼠标左键选中(切换)需要追踪的线要素，点击线要素，系统自动追踪点;鼠标右键取消追踪;双击鼠标完成;Esc键清除";
            m_category = "要素编辑";
            NeedSnap = true;
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        private INewLineFeedback m_lineFeedback = null;
        IEngineEditor editor=null;
        private TraceLine traceLine = null;
        private IPolyline selPolyline = null;
        private TracePoint tracePts = null;
        private IPoint startPt = null;  //追踪起点
        List<IFeature> polygonFes = null;
        private IActiveView mActiveView;
        bool TraceLineFlag = false;//是否符合追踪线的条件
        bool isTrace = false;  //判断是否处于追踪状态
        public override void OnClick()
        {
            TraceLineFlag = false;
            editor = m_Application.EngineEditor;
            polygonFes = new List<IFeature>();
            TraceLineFlag = true;
            m_Application.MapControl.OnAfterScreenDraw += new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            
        }
        private void CheckValide()
        {
            
            polygonFes = new List<IFeature>();
            var map = m_Application.ActiveView.FocusMap;
            TraceLineFlag = true;
            if (m_lineFeedback == null)
            {
                m_lineFeedback = new NewLineFeedbackClass();
                m_lineFeedback.Display = m_Application.ActiveView.ScreenDisplay;
            }
           
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
           
            mActiveView = app.MapControl.ActiveView;
        }
        private IPolyline PolygonToLine(IPolygon polygon)
        {
            IGeometryCollection gc = polygon as IGeometryCollection;
            PolylineClass lineclass = new PolylineClass();
            if (gc.GeometryCount > 1)
            {
                for (int i = 0; i < gc.GeometryCount; i++)
                {
                    if (gc.get_Geometry(i).IsEmpty)
                        continue;

                    if ((gc.get_Geometry(i) as IRing).IsExterior)
                    {
                        lineclass.AddPointCollection(gc.get_Geometry(i) as IPointCollection);

                    }
                }
            }
            else
            {
                lineclass.AddPointCollection(polygon as IPointCollection);
            }
            lineclass.Simplify();
            return lineclass as IPolyline;
          
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1)
            {
                CheckValide();
            }
            if (!TraceLineFlag)
            {
                return;
            }
            #region 设置当前开编图层
            var Lyr = m_Application.TOCSelectItem.Layer;
            if (!(Lyr is FeatureLayer))
            {
                MessageBox.Show("请选择要素层！");
                return;
            }
            IFeatureLayer pFeatureLayer = Lyr as IFeatureLayer;
            if (pFeatureLayer == null || pFeatureLayer.FeatureClass == null)
                return;
            IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;
            if (!editLayer.IsEditable(pFeatureLayer))
            {
                MessageBox.Show("图层不可编辑！");
                return;
            }
            editLayer.SetTargetLayer(pFeatureLayer, 0);
            #endregion
            IPoint pPoint = ToSnapedMapPoint(x, y);
            if (button == 1&&shift==2)//按住ctrl+左键，选择面或者线要素，为了后续进行跟踪
            {
                isTrace = false;
                #region
                IFeature pFeature = SelectByPoint(pPoint);
              
                if (pFeature == null)
                {
                    MessageBox.Show("请选择要追踪的要素");
                    return;
                }
                //如果是面要素则需要，提取边界
                if (pFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    selPolyline = PolygonToLine(pFeature.ShapeCopy as IPolygon);
                }
                else
                {
                    selPolyline = pFeature.ShapeCopy as IPolyline;
                }

                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);  
                if (traceLine == null)
                {
                    traceLine = new TraceLine();
                }
             //   tracePts = new TracePoint();
                traceLine.Pt.Add(tracePts);
                tracePts.polyline = selPolyline;
                DrawSelePolyline(selPolyline);
                m_Application.ActiveView.Refresh();
                #endregion
            }
            else if (button == 1 && selPolyline != null)
            {
                IPointCollection pl = selPolyline as IPointCollection;
                IPoint lastPoint = ToSnapedMapPoint(x, y);
                IPoint outPoint = new PointClass();
                double alCurve = 0, frCurve = 0; bool rSide = false;
                (pl as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, lastPoint, false, outPoint, ref alCurve, ref frCurve, ref rSide);
                double ms = m_Application.ActiveView.FocusMap.ReferenceScale;
                double dis = 2.5 * ms / 1000 == 0 ? 2.5 : 2.5 * ms / 1000;

                if (frCurve < dis)
                {
                    if (!isTrace)
                    {
                        tracePts.points.Add(outPoint);

                        if (tracePts.points.Count == 1)
                        {
                            m_lineFeedback.Start(outPoint);
                        }
                        else
                        {
                            m_lineFeedback.AddPoint(outPoint);          
                        }
                        startPt = outPoint;
                        isTrace = true;
                    }
                    else
                    {
                        IPointCollection tracePoints = getColFrom2Point(startPt, outPoint, selPolyline);
                        for (int k = 0; k < tracePoints.PointCount; k++)
                        {
                            tracePts.points.Add(tracePoints.get_Point(k));
                        }
                     
                        startPt = outPoint;
                        for (int j = 0; j < tracePoints.PointCount; j++)
                        {
                            m_lineFeedback.AddPoint(tracePoints.get_Point(j));
                        }
                        
                        //isTrace = false;
                    }
                }
            }
            else if (button == 2)
            {
                isTrace = false;
                selPolyline = null;

                #region 清除追踪线
                IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
                gc.Reset();
                IElement ele = null;
                while ((ele = gc.Next()) != null)
                {
                    IElementProperties ep1 = ele as IElementProperties;
                    if (ep1.Name == "TraceSelLine")
                        m_Application.ActiveView.GraphicsContainer.DeleteElement(ele);
                }
                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_Application.ActiveView.Extent);
                #endregion

            }
            else if (button == 1 && selPolyline == null)//直接画点
            {
                isTrace = false;
                if (traceLine == null)
                {
                    traceLine = new TraceLine();
                }
                if (tracePts == null)
                {
                    tracePts = new TracePoint();
                    traceLine.Pt.Add(tracePts);
                }

                tracePts.points.Add(pPoint);

                if (tracePts.points.Count == 1)
                {
                    m_lineFeedback.Start(pPoint);
                }
                else
                {
                    m_lineFeedback.AddPoint(pPoint);
                }

            }
          
        }
        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            IPoint pPoint = ToSnapedMapPoint(x, y);
            if (m_lineFeedback != null)
            {
                m_lineFeedback.MoveTo(pPoint);
            }
        }

        public override void OnDblClick()
        {

            if (!TraceLineFlag)
            {
                return;
            }
            IPolyline resultLine= m_lineFeedback.Stop();
            m_lineFeedback = null;
           
            reset();
            var Lyr = m_Application.TOCSelectItem.Layer;
            if (!(Lyr is FeatureLayer))
            {
                MessageBox.Show("请选择要素层！");
                return;
            }
            IFeatureLayer pFeatureLayer = Lyr as IFeatureLayer;
            if (pFeatureLayer == null || pFeatureLayer.FeatureClass == null)
                return;
            var m_engineEditor = m_Application.EngineEditor;
            IEngineEditLayers editLayer = m_engineEditor as IEngineEditLayers;
            if (!editLayer.IsEditable(pFeatureLayer))
            {
                MessageBox.Show("图层不可编辑！");
                return;
            }
            if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
            {
                return;
            }
            editor.StartOperation();
            IFeature fe = pFeatureLayer.FeatureClass.CreateFeature();
            fe.Shape = resultLine;
            IFeature pFeature = fe as IFeature;

            //属性赋值
            ILegendGroup group = m_Application.TOCSelectItem.Group;
            ILegendClass cls = m_Application.TOCSelectItem.Class;
            if (group != null && cls != null)
            {
                if (cls is IRepresentationLegendClass)//制图表达(赋规则ID)
                {
                    var lc = cls as IRepresentationLegendClass;

                    int ruleID = lc.RuleID;
                    int ruleIDFieldIndex = lc.RepresentationClass.RuleIDFieldIndex;


                    if (ruleID != -1 && ruleIDFieldIndex != -1)//赋值规则ID
                        pFeature.set_Value(ruleIDFieldIndex, ruleID);

                }
                else
                {
                    if (group.Heading.ToLower() == "gb")//赋值GB码
                    {
                        int gb = int.Parse(cls.Label);

                        int index = pFeature.Fields.FindField("gb");
                        if (index != -1)
                        {
                            pFeature.set_Value(index, gb);
                        }
                    }
                }

            }

            pFeature.Store();
            fe.Store();
            editor.StopOperation("追踪线创建要素");
            //var lineElement = new LineElementClass();
            //var lineSymbol = new SimpleLineSymbolClass();
            //lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            //lineSymbol.Color = new RgbColorClass { Red = 255 };
            //lineSymbol.Width = 2;
            //lineElement.Symbol = lineSymbol;
            ////
            //var element = lineElement as IElement;
            //element.Geometry = resultLine;
            //IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;

            //gc.AddElement(element, 0);
        }


        public IPointCollection getColFrom2Point(IPoint p1, IPoint p2, IPolyline pLine)
        {
            IPointCollection pPointCol = new PolylineClass();
            IPointCollection pl = pLine as IPointCollection;
            bool isSpilt;
            int spiltPart, seg1Index, seg2Index;
            pLine.SplitAtPoint(p1, true, false, out isSpilt, out spiltPart, out seg1Index);  //start
            pLine.SplitAtPoint(p2, true, false, out isSpilt, out spiltPart, out seg2Index); //end

            object o = Type.Missing;
            if (seg1Index < seg2Index)
            {
                for (int i = seg1Index; i < seg2Index + 1; i++)
                {
                    pPointCol.AddPoint(pl.get_Point(i), ref o, ref o);
                }
            }
            else
            {
                for (int i = seg1Index; i > seg2Index - 1; i--)
                {
                    pPointCol.AddPoint(pl.get_Point(i), ref o, ref o);
                }
            }

          

            return pPointCol;
        }

        //根据点选取需要追踪的要素
        private IFeature SelectByPoint(IPoint pt)
        {
            IFeature tracefeature = null;
            var topoOper = (pt as ITopologicalOperator);
            IGeometry geometry = null;
            if (m_Application.Workspace.Map.SpatialReference is IGeographicCoordinateSystem)
            {
                geometry = topoOper.Buffer(5 * 0.000009).Envelope;
            }
            else
            {
                double dis = 2.5e-3 * m_Application.ActiveView.FocusMap.MapScale;//地图上2.5毫米
                dis = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(3);
                geometry = topoOper.Buffer(dis).Envelope;
            }


            ISpatialFilter sf = new SpatialFilter();
            sf.Geometry = geometry;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l.Visible) && ((l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline||(l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon);

            })).ToArray();
            try
            {
                IEngineEditLayers editLayer = editor as IEngineEditLayers;
                foreach (var item in lyrs)
                {
                    if (!editLayer.IsEditable(item as IFeatureLayer))
                    {
                        continue;
                    }
                    IGeoFeatureLayer geoFealyr = item as IGeoFeatureLayer;
                    IFeatureClass fc = geoFealyr.FeatureClass;
                    if (fc.FeatureCount(sf) > 0)
                    {
                        IFeatureCursor pCursor = fc.Search(sf, false);
                    

                        tracefeature = pCursor.NextFeature();
                        Marshal.ReleaseComObject(pCursor);
                        break;
                    }

                }
                return tracefeature;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
       

        public void reset()
        {
            ////变量重置
            m_lineFeedback = null;
            TraceLineFlag = false;
           
            traceLine = null;
            selPolyline = null;
            tracePts = null;
            IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            gc.Reset();
            gc.DeleteAllElements();
            m_Application.MapControl.ActiveView.Refresh();
        }

        public override bool Deactivate()
        {
            reset();
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);

            return true;
        }
        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (m_lineFeedback != null)
            {
                m_lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
            }
           
        }
        private void drawPoint(IPoint pt)
        {   
            //IPolyline NPL = GetTraceLine();
            //if (!NPL.IsEmpty)
            //{
            //    DrawTraceLine(NPL);
            //}
            //m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_Application.ActiveView.Extent);
            //m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null,m_Application.ActiveView.Extent);  
        }

       
        public override void OnKeyUp(int keyCode, int shift)
        {
            if (keyCode == (int)Keys.Escape)
            {
                reset();
                m_Application.MapControl.Refresh();
            }
        }
        private void ModifyPolygon(IPolyline polyline)
        {
            //(polyline as ITopologicalOperator).Simplify();
            //IPointCollection reshapePath = new PathClass();
            //reshapePath.AddPointCollection(polyline as IPointCollection);

            //ITopologicalOperator trackTopo = polyline as ITopologicalOperator;
           

            IPointCollection geoReshapePath = new PathClass();
            IGeometryCollection gc = polyline as IGeometryCollection;
            for (int g = 0; g < gc.GeometryCount; g++)
            {
                var geometry=gc.get_Geometry(g);
                geoReshapePath.AddPointCollection(geometry as IPointCollection);
            }
            try
            {
              

                editor.StartOperation();
                {
                    
                    foreach(var pFeature in polygonFes)
                    {
                     
                        IPolygon pg = pFeature.ShapeCopy as IPolygon;
                        IGeometryCollection pgCol = pg as IGeometryCollection;
                        for (int i = 0; i < pgCol.GeometryCount; i++)
                        {
                            IRing r = pgCol.get_Geometry(i) as IRing;
                            try
                            {
                                r.Reshape(geoReshapePath as IPath);
                            }
                            catch (Exception)
                            {

                                continue;
                            }
                        }
                        (pg as ITopologicalOperator).Simplify();
                        pFeature.Shape = pg;
                        pFeature.Store();

                        
                    }
                   // Marshal.ReleaseComObject(pCursor);
                }

                editor.StopOperation("追踪线修面");
            }
            catch (Exception ex)
            {
                editor.AbortOperation();
                System.Diagnostics.Trace.WriteLine(ex.Message, "Edit Geometry Failed");
            }

            m_Application.ActiveView.Refresh();
        }

        private void DrawSelePolyline(IPolyline pl)
        {
            IElement element = null;
            IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            gc.Reset();
            IElement ele = null;
            while ((ele = gc.Next()) != null)
            {
                IElementProperties ep1 = ele as IElementProperties;
                if (ep1.Name == "TraceSelLine")
                    m_Application.ActiveView.GraphicsContainer.DeleteElement(ele);
            }
            ILineElement lineElement = null;
            ISimpleLineSymbol lineSymbol = null;
            IElementProperties ep = null;
            //绘制选中的线
            if (selPolyline != null)
            {
                lineElement = new LineElementClass();
                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                lineSymbol.Color = new RgbColorClass { Red = 255 };
                lineSymbol.Width = 2;
                lineElement.Symbol = lineSymbol;
                //
                element = lineElement as IElement;
                element.Geometry = pl;
                ep = element as IElementProperties;
                ep.Name = "TraceSelLine";
                gc.AddElement(element, 0);
            }
          //  m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_Application.ActiveView.Extent);
        }
        private void DrawTraceLine(IPolyline pl)
        {
            return;
            IElement element = null;
            IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            gc.Reset();
            IElement ele = null;
            while ((ele = gc.Next()) != null)
            {
                IElementProperties ep1 = ele as IElementProperties;
                if (ep1.Name == "TraceLine")
                    m_Application.ActiveView.GraphicsContainer.DeleteElement(ele);
            }
            ILineElement lineElement = null;
            ISimpleLineSymbol lineSymbol = null;
            IElementProperties ep = null;
           
            //绘制追踪线
            {
                lineElement = new LineElementClass();
                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                lineSymbol.Color = new RgbColorClass { Blue = 255 };
                lineSymbol.Width = 2;
                lineElement.Symbol = lineSymbol;
                //
                element = lineElement as IElement;
                element.Geometry = pl;
                ep = element as IElementProperties;
                ep.Name = "TraceLine";
                gc.AddElement(element, 0);
            }
            //绘制追踪点
            {
                RgbColorClass c = new RgbColorClass();
                for (int i = 0; i < (pl as IPointCollection).PointCount; i++)
                {
                    var pt = (pl as IPointCollection).get_Point(i);
                    SimpleMarkerSymbolClass sms = new SimpleMarkerSymbolClass();
                    c.Red = 0;
                    c.Blue = 0;
                    c.Green = 255;

                    sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
                    sms.Color = c;
                    sms.Size = 2;
                    IMarkerElement markerEle = new MarkerElementClass();
                    markerEle.Symbol = sms;
                    element = markerEle as IElement;
                    element.Geometry = pt as IGeometry;
                    ep = element as IElementProperties;
                    ep.Name = "TraceLinePoint";
                    gc.AddElement(element, 0);
                }
            }

        }
        private void DrawTracePolyline(IPolyline pl)
        {
            IElement element = null;
            IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            gc.Reset();
            IElement ele = null;
            while ((ele = gc.Next()) != null)
            {
                IElementProperties ep1 = ele as IElementProperties;
                if (ep1.Name == "TraceLine")
                    m_Application.ActiveView.GraphicsContainer.DeleteElement(ele);
            }
            ILineElement lineElement = null;
            ISimpleLineSymbol lineSymbol = null;
            IElementProperties ep = null;
            //绘制选中的线

            lineElement = new LineElementClass();
            lineSymbol = new SimpleLineSymbolClass();
            lineSymbol.Style = esriSimpleLineStyle.esriSLSDash;
            lineSymbol.Color = new RgbColorClass { Red = 255 };
            lineSymbol.Width = 4;
            lineElement.Symbol = lineSymbol;
            //
            element = lineElement as IElement;
            element.Geometry = pl;
            ep = element as IElementProperties;
            ep.Name = "TraceLine";
            gc.AddElement(element, 0);

            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
        }
        public override void Refresh(int hdc)
        {
         
        }
    }
}

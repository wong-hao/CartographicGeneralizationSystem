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
     
     class TraceLine
    {
        public List<TracePoint> Pt;
        public TraceLine()
        {
            Pt = new List<TracePoint>();
        }
    }
    class TracePoint
    {
        public IPolyline polyline;
        public List<IPoint> points;
        public TracePoint()
        {
            points = new List<IPoint>();
        }
    }
    public class TraceLineReShapePolygons : SMGI.Common.SMGITool
    {
        /// <summary>
        /// 追踪提取边线
        /// </summary>
        public TraceLineReShapePolygons()
        {
            m_caption = "追踪线修面";
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

        IEngineEditor editor=null;
        private TraceLine traceLine = null;
        private IPolyline selPolyline = null;
        private TracePoint tracePts = null;
        List<IFeature> polygonFes = null;
        private IActiveView mActiveView;
        bool TraceLineFlag = false;//是否符合追踪线的条件
        public override void OnClick()
        {
            TraceLineFlag = false;
            editor = m_Application.EngineEditor;
            polygonFes = new List<IFeature>();
            var map = m_Application.ActiveView.FocusMap;
            var selection = map.FeatureSelection;
            if (map.SelectionCount > 0)
            {
                polygonFes = new List<IFeature>();
                IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
                selectEnumFeature.Reset();
                IFeature fe = null;
                while ((fe = selectEnumFeature.Next()) != null)
                {
                    if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                    {
                        polygonFes.Add(fe);
                    }
                }
                if (polygonFes.Count == 0)
                {
                    MessageBox.Show("请先选择至少一个面要素");
                    return;
                }
                TraceLineFlag = true;
            }
            else
            {
                MessageBox.Show("请先选择至少一个面要素");
            }
        }
        private void CheckValide()
        {
            TraceLineFlag = false;
            polygonFes = new List<IFeature>();
            var map = m_Application.ActiveView.FocusMap;
            var selection = map.FeatureSelection;
            if (map.SelectionCount > 0)
            {
                polygonFes = new List<IFeature>();
                IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
                selectEnumFeature.Reset();
                IFeature fe = null;
                while ((fe = selectEnumFeature.Next()) != null)
                {
                    if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                    {
                        polygonFes.Add(fe);
                    }
                }
                if (polygonFes.Count == 0)
                {
                    MessageBox.Show("请先选择至少一个面要素");
                    return;
                }
                TraceLineFlag = true;
            }
            else
            {
                MessageBox.Show("请先选择至少一个面要素");
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
           
            mActiveView = app.MapControl.ActiveView;
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
            IPoint pPoint = ToSnapedMapPoint(x, y);
            if (button == 1&&shift==2)//按住ctrl+左键，选择线要素，并进行跟踪
            {
               
                IFeature pFeature = SelectByPoint(pPoint);
              
                if (pFeature == null || pFeature.Shape.GeometryType!= esriGeometryType.esriGeometryPolyline)
                {
                    MessageBox.Show("请选择要提取的线要素");
                    return;
                }
                selPolyline = pFeature.ShapeCopy as IPolyline;
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);  
                if (traceLine == null)
                {
                    traceLine = new TraceLine();
                }
                tracePts = new TracePoint();
                traceLine.Pt.Add(tracePts);
                tracePts.polyline = selPolyline;
                DrawSelePolyline(selPolyline);
                m_Application.ActiveView.Refresh();
            }
            else if (button == 1&&selPolyline!=null)
            {
                //获取点集
                IPointCollection pl = selPolyline as IPointCollection;
                {
                   
                    IPoint lastPoint = ToSnapedMapPoint(x, y); 
                    IPoint outPoint=new PointClass();
                    double alCurve=0,frCurve=0;bool rSide=false;
                    (pl as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, lastPoint, false, outPoint, ref alCurve,ref frCurve, ref rSide);
                    if (frCurve < 0.5)
                    {
                        tracePts.points.Add(outPoint);
                    }
                    drawPoint(outPoint);
                }  
            }
            else if (button == 2)
            {
                
                selPolyline = null;
                tracePts = null;
                m_Application.ActiveView.Refresh();
               // tracePts = new TracePoint();
            }
            else if (button == 1 && selPolyline == null)//直接画点
            {
               if (traceLine == null)
               {
                   traceLine = new TraceLine();
               }
               if(tracePts==null)
               {
                   tracePts = new TracePoint();
                   traceLine.Pt.Add(tracePts);
               }
              
               tracePts.points.Add(pPoint);
               drawPoint(pPoint);
              
                
            }
            //else if (button == 2)
            //{ //右键重置
            //    reset();
            //}
        }
      
        public override void OnDblClick()
        {

            if (!TraceLineFlag)
            {
                return;
            }
            IPolyline NPL = GetTraceLine();
            if (!NPL.IsEmpty)
            {
                ModifyPolygon(NPL);
            }
            reset();

        }



        private IPointCollection NPC;
        //获取当前的追踪线
        private IPolyline GetTraceLine()
        {
            IPointCollection pc = new PolylineClass();
            IGeometryCollection   polylineGc = new PolylineClass();
            for (int i = 0; i < traceLine.Pt.Count; i++)
            {

                var pts = traceLine.Pt[i];
                if (pts.polyline == null)//自由追踪
                {
                    IPointCollection path = new PathClass();
                    #region
                    if (pc.PointCount > 0)
                    {
                        path.AddPoint(pc.get_Point(pc.PointCount - 1));
                    }
                    foreach (var pt in pts.points)
                    {
                        path.AddPoint(pt);
                        pc.AddPoint(pt);

                    }
                    #endregion
                    polylineGc.AddGeometry(path as IGeometry);
                }
                else//存在关联要素的情况下：关联追踪
                {
                    #region
                    if (pts.points.Count > 0)//连接线
                    {
                        IPointCollection path = new PathClass();
                        if (pc.PointCount > 0)
                        {
                            path.AddPoint(pc.get_Point(pc.PointCount-1));
                            path.AddPoint(pts.points[0]);
                            polylineGc.AddGeometry(path as IGeometry);
                        }
                        pc.AddPoint(pts.points[0]);
                    }
                    if (pts.points.Count > 1)//窃取关联线的一部分
                    {
                        IPointCollection path = new PathClass();
                        var line = pts.polyline;
                        for (int t = 0; t <= pts.points.Count - 2; t++)
                        {
                            #region
                            //当前的点
                            IPoint curPoint = pc.get_Point(pc.PointCount - 1);
                            var reOp = curPoint as IRelationalOperator;
                            var inputpt = pts.points[t];
                            var inputpt1 = pts.points[t + 1];
                            IPoint outpt = new PointClass();

                            double fdis = 0;
                            double dis = 0;
                            double dis1 = 0;
                            bool flag = true;
                            line.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, inputpt, false, outpt, ref dis, ref fdis, flag);
                            line.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, inputpt1, false, outpt, ref dis1, ref fdis, flag);
                            ICurve outCurve = new PolylineClass();
                            line.GetSubcurve(dis, dis1, false, out outCurve);
                            if (outCurve.IsEmpty)
                                continue;
                            if (reOp.Equals(outCurve.ToPoint))
                            {
                                outCurve.ReverseOrientation();
                            }
                            var pcurver = outCurve as IPointCollection;
                            path.AddPointCollection(pcurver);
                            pc.AddPointCollection(pcurver);
                            #endregion
                        }
                        polylineGc.AddGeometry(path as IGeometry);

                    }
                    #endregion

                }
            }
            IPolyline NPL = polylineGc as IPolyline;
           // (NPL as ITopologicalOperator).Simplify();
            //   drawLine(NPL);
            return NPL;
        }
        //根据点选取线要素
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
                geometry = topoOper.Buffer(dis).Envelope;
            }


            ISpatialFilter sf = new SpatialFilter();
            sf.Geometry = geometry;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l.Visible) && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline;

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

            TraceLineFlag = false;
            NPC = null;
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
            return true;
        }
       
        private void drawPoint(IPoint pt)
        {   
            IPolyline NPL = GetTraceLine();
            if (!NPL.IsEmpty)
            {
                DrawTraceLine(NPL);
            }
            
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null,m_Application.ActiveView.Extent);  
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
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);  
        }
        private void DrawTraceLine(IPolyline pl)
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
                    c.Blue = 255;
                    c.Green = 0;

                    sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
                    sms.Color = c;
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
        public override void Refresh(int hdc)
        {
            return;
            RgbColorClass c = new RgbColorClass();
            IDisplay dis = new SimpleDisplayClass();
            if (selPolyline != null)
            {
                dis.DisplayTransformation = m_Application.ActiveView.ScreenDisplay.DisplayTransformation;
                dis.DisplayTransformation.ReferenceScale = 0;
                dis.StartDrawing(hdc, -1);
                SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
                c.Red = 250;
                c.Blue = 0;
                c.Green = 0;
                sls.Color = c;
                sls.Width = 2;

                dis.SetSymbol(sls as ISymbol);
                dis.DrawPolyline(selPolyline);
                dis.FinishDrawing();
            }
            if (traceLine != null)
            {
               
                if (traceLine.Pt.Count > 0)
                {
                    if (traceLine.Pt[0].points.Count == 0)
                        return;
                    dis.DisplayTransformation = m_Application.ActiveView.ScreenDisplay.DisplayTransformation;
                    dis.DisplayTransformation.ReferenceScale = 0;
                    dis.StartDrawing(hdc, -1);
                    var fpt = traceLine.Pt[0].points[0];
                    var pt = fpt;
                    SimpleMarkerSymbolClass sms = new SimpleMarkerSymbolClass();
                    c.Red = 0;
                    c.Blue = 255;
                    c.Green = 0;

                    sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
                    sms.Color = c;
                    dis.SetSymbol(sms);
                    dis.DrawPoint(pt);
                    dis.FinishDrawing();
                }

                IPolyline NPL = GetTraceLine();
                if (NPL.IsEmpty)
                    return;
               

                dis.DisplayTransformation = m_Application.ActiveView.ScreenDisplay.DisplayTransformation;
                dis.DisplayTransformation.ReferenceScale = 0;
                dis.StartDrawing(hdc, -1);

                SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
               
                c.Red = 0;
                c.Blue = 255;
                c.Green = 0;

                sls.Color = c;
                sls.Width = 2;

                dis.SetSymbol(sls as ISymbol);
                // dis.DrawPolyline(NPL);
                // drawLine(NPL);
                //绘制点
                for (int i = 0; i < (NPL as IPointCollection).PointCount; i++)
                {
                    var pt = (NPL as IPointCollection).get_Point(i);
                    SimpleMarkerSymbolClass sms = new SimpleMarkerSymbolClass();
                    c.Red = 0;
                    c.Blue = 255;
                    c.Green = 0;

                    sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
                    sms.Color = c;
                    dis.SetSymbol(sms);
                    dis.DrawPoint(pt);
                }

                dis.FinishDrawing();
            }
        }
    }
}

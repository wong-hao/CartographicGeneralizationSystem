using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.ThematicChart
{
    public class RepFreeMoveTool:SMGITool
    {
        private IMap pMap;
        private IActiveView pAc;
        //选中需要移动的Marker
        NewEnvelopeFeedbackClass fb;
        private IRepresentation fetureRep;
        IRepresentationRule rule = null;
        IRepresentationGraphics repGraphic = null;
        ILayer repLayer = null;
        IFeatureClass repFcl = null;
        double Scale=1;
        public RepFreeMoveTool()
        {
            m_caption = "自由移动";
            m_toolTip = "自由移动";
            m_category = "自由";
                  
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
            pAc = m_Application.ActiveView;
            pMap = m_Application.ActiveView.FocusMap;
            repLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOINT"))).FirstOrDefault();
            repFcl = (repLayer as IFeatureLayer).FeatureClass;
        }

        IFeature currentFe = null;
        int GeoID;
        IGeometry RepGeo = null;
        IElement pGbEle = null;
        IGeometry pEleGeo = null;

        //List<IFeature> FeSelects = new List<IFeature>();
        IPoint startPt;
        public override void OnMouseDown(int button, int shift, int x, int y)
        {

            if (button != 1)
            {
                return;
            }
            IPoint pt = ToMapPoint(x, y);
            
            if (currentFe != null)
            {
                startPt = pt;
                fetureRep = GetSelectFeatureRepresentation(currentFe, ref repGraphic);
                getCurrentRepGeo(pt);
               // DrawBgMoveEle(pt);
            }
            else
            {
               
                pMap.ClearSelection();
                (pAc as IGraphicsContainer).DeleteAllElements();
                //第一步拉框选中要素
                IRubberBand rb = new RubberEnvelopeClass();
                IEnvelope pEnvelope = rb.TrackNew(pAc.ScreenDisplay, null) as IEnvelope;
                if (pEnvelope.IsEmpty)
                {
                    pAc.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, pAc.Extent);
                    return;
                }
                //选中要素
                ISpatialFilter pSpatialFilter = null;
                pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = pEnvelope;
                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor pcursor = repFcl.Search(pSpatialFilter, false);
                IFeature fe = pcursor.NextFeature();
                if (fe != null)
                {
                    pMap.SelectFeature(repLayer, fe);
                    currentFe = fe;
                }
                pAc.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, pAc.Extent);
            }      
           

        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {

        
          if (button != 1)
              return;
          IPoint pt = ToMapPoint(x, y);
          if (currentFe != null && RepGeo!=null)
          {
           //  DrawMoveEle(pt);
          }
         
        }
        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (currentFe == null)
                return;
            //if (button != 1)
            //{
            //    markerGeo = null;
            //    symGeo = null;
            //    rule = null;
            //    return;
            //}
            //var editor = m_Application.EngineEditor;
            //if (fetureRep == null)
            //    return;
            //var pt = ToSnapedMapPoint(x, y);
            //if (markerGeo != null)
            //{
            //    editor.StartOperation();
            //    GetMarkerGraphicsID();
            //    fetureRep.Graphics.ChangeGeometry(geoID, pt);
            //   // fetureRep.Graphics.ChangeGeometry(geoID, new PointClass());
            //    fetureRep.UpdateFeature();
            //    fetureRep.Feature.Store();
            //    pAc.Refresh();
            //    editor.StopOperation("制图移动");
            //}
            ////清除相关元素
            //markerGeo = null;
            //symGeo = null;
            //rule = null;
            //IGraphicsContainer pGraContainer = pAc as IGraphicsContainer;
            //pGraContainer.DeleteAllElements();
             
        }

        
        //获取选中要素的制图表达：默认是获取选中的第一个要素
        private IRepresentation GetSelectFeatureRepresentation(IFeature fe,ref IRepresentationGraphics repGraphics)
        {
            try
            {
                 
                if (fe == null)
                    return null;
                IFeatureClass pfeatureClass = fe.Class as IFeatureClass;
                var mc = new MapContextClass();
                mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
                IGeoFeatureLayer gl = null;
                for (int i = 0; i < pMap.LayerCount; i++)
                {
                    if (pMap.get_Layer(i).Name == pfeatureClass.AliasName)
                    {
                        gl = pMap.get_Layer(i) as IGeoFeatureLayer;
                        break;
                    }
                }
                   
                var rr = gl.Renderer as IRepresentationRenderer;
                var cls = rr.RepresentationClass;
                IRepresentation pReptation = cls.GetRepresentation(fe, mc);
                var g = pReptation.Graphics;
                g.Reset();
                IGeometry geo;
                g.Next(out geo, out rule);
                IBasicSymbol basicSym = rule.get_Layer(0);
              
              
                IBasicMarkerSymbol pMarkerSym = basicSym as IBasicMarkerSymbol;

                IGraphicAttributes at = basicSym as IGraphicAttributes;
                IRepresentationMarker mm = at.get_Value(1) as IRepresentationMarker;
                repGraphics = mm as IRepresentationGraphics;
                //获取外接矩形大小
                double minx = double.MaxValue;
                double miny = double.MaxValue;
                double maxy = double.MinValue;
                double maxx = double.MinValue;
                repGraphics.ResetGeometry();
                geo = null;
                int id;
                while (true)
                {
                    repGraphics.NextGeometry(out id, out geo);
                    if (geo == null)
                        break;
                    if (geo is IPoint)
                    {

                        
                        if ((geo as IPoint).X> maxx)
                        {
                            maxx = (geo as IPoint).X;
                        }
                        if ((geo as IPoint).Y > maxy)
                        {
                            maxy = (geo as IPoint).Y;
                        }
                        if ((geo as IPoint).X < minx)
                        {
                            minx = (geo as IPoint).X;
                        }
                        if ((geo as IPoint).Y < miny)
                        {
                            miny = (geo as IPoint).Y;
                        }
                    }
                    if(geo is IPolyline||geo is IPolygon)
                    {
                        IEnvelope pen = geo.Envelope;
                        if (pen.XMax > maxx)
                        {
                            maxx = pen.XMax;
                        }
                        if (pen.YMax > maxy)
                        {
                            maxy = pen.YMax;
                        }
                        if (pen.XMin<minx)
                        {
                            minx = pen.XMin;
                        }
                        if (pen.YMin <miny)
                        {
                            miny = pen.YMin;
                        }
                    }
                }
                double width = maxx - minx;
                double height = maxy - miny;
                double size = Convert.ToDouble(at.get_Value(2));
                Scale = size / 2.83465 * pMap.ReferenceScale * 1e-3 / Math.Max(width, height);
                return pReptation;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //鼠标点击选中相应的marker
        
      
        IGeometry symGeo = null;
       
        //获取图元Rule
        private void GetMarkerOnMouseDown(int x,int y)
        {      
            try
            {
                var pt = ToSnapedMapPoint(x, y);

                double dis = pAc.ScreenDisplay.DisplayTransformation.FromPoints(2);
                IRelationalOperator rp = (pt as ITopologicalOperator).Buffer(dis) as IRelationalOperator;
                var g= fetureRep.Graphics;
                g.ResetGeometry();
                g.Reset();
                int id;
                IGeometry geo1=null; 
                //IGeometry geo = null;
                //选中markerGraphic
                while (true)
                {
                   // g.NextGeometry(out id, out geo);
                    g.Next(out geo1, out rule);
                    if (geo1 == null)
                        break;
                    if (geo1.GeometryType != esriGeometryType.esriGeometryPoint)
                        continue;
                    if (!rp.Disjoint(geo1))//相交
                    {
                       // markerGeo = geo1;
                        //geoID = id;
                       
                        break;

                    }
                }
                
               
            }
            catch(Exception ex)
            {
                return ;
            }
       
        }
       
        /// <summary>
        ///  //绘制移动的要素
        /// </summary>
        /// <param name="curPoint">移动点</param>
        private void DrawSelectGraphic(IPoint curPoint)
        {
            IGraphicsContainer pGraContainer = pAc as IGraphicsContainer;
            pGraContainer.DeleteAllElements();
            IBasicSymbol basicSym = rule.get_Layer(0);
            IGeometry sysGeoClone = null;
            if (basicSym is IBasicMarkerSymbol)//点符号
            {
                IBasicMarkerSymbol pMarkerSym = basicSym as IBasicMarkerSymbol;

                IGraphicAttributes at = basicSym as IGraphicAttributes;
                IRepresentationMarker mm = at.get_Value(1) as IRepresentationMarker;
                double size = Convert.ToDouble(at.get_Value(2));
                double angle = Convert.ToDouble(at.get_Value(3));
                angle = angle / 180 * Math.PI;
                IRepresentationGraphics repGraphic = mm as IRepresentationGraphics;
                repGraphic.ResetGeometry();
                IGeometry pgeo11 = null;
                int id1;
                repGraphic.NextGeometry(out id1, out symGeo);
                sysGeoClone = (symGeo as IClone).Clone() as IGeometry;

                //var mp = pMarkerSym.MarkerPlacement;
                //var mapGeometry = (markerGeo as IClone).Clone() as IGeometry;
                //mp.Reset(mapGeometry);
                //IAffineTransformation2D atrans = null;
                //while ((atrans = mp.NextTransformation())!= null)
                //{
                    
                //}

                IPoint center = (sysGeoClone as IArea).Centroid;
                ITransform2D ptrans = sysGeoClone as ITransform2D;
                ptrans.Move((curPoint).X - center.X, (curPoint).Y - center.Y);
                double scale=size / 2.83 * 10 /Math.Max( symGeo.Envelope.Width,symGeo.Envelope.Height);
                ptrans.Scale((curPoint), scale, scale);
                 ptrans.Rotate(curPoint,angle);
            
                //绘制
                ISimpleFillSymbol sym = new SimpleFillSymbolClass();
                sym.Outline = new SimpleLineSymbolClass() { Width = 0.03, Style = esriSimpleLineStyle.esriSLSSolid, Color = new RgbColorClass() { Red = 200 } };
                sym.Style = esriSimpleFillStyle.esriSFSNull;
                IElement pele;
                IFillShapeElement pfillEle = new PolygonElementClass();

                pfillEle.Symbol = sym;

                pele = pfillEle as IElement;
                pele.Geometry = sysGeoClone;
                pGraContainer.AddElement(pele, 0);
                // pAc.Extent = markerGeo.Envelope;
                pAc.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGraphics, null, (sysGeoClone as ITopologicalOperator).Buffer(5).Envelope);

            }
            else if (basicSym is IBasicLineSymbol)//线符号
            {
                #region
               // IMapContext mapContext = new MapContextClass();
               // mapContext.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
               // sysGeoClone = (markerGeo as IClone).Clone() as IGeometry;
               // IPoint center = (sysGeoClone as IArea).Centroid;
               // ITransform2D ptrans = sysGeoClone as ITransform2D;
               // ptrans.Move((curPoint).X - center.X, (curPoint).Y - center.Y);
               ////绘制
               // ESRI.ArcGIS.Display.ISimpleLineSymbol simpleLineSymbol = new ESRI.ArcGIS.Display.SimpleLineSymbolClass();
               // simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
               // simpleLineSymbol.Color = new RgbColorClass() {Red=200 };
               // simpleLineSymbol.Width = 0.2;

               // ESRI.ArcGIS.Carto.ILineElement fillShapeElement = new ESRI.ArcGIS.Carto.LineElementClass();
               // fillShapeElement.Symbol = simpleLineSymbol;
               // IElement element = fillShapeElement as IElement;
               // element.Geometry = sysGeoClone as IGeometry;
               // pGraContainer.AddElement(element, 0);
               // double len = (sysGeoClone as IPolyline).Length;
               // pAc.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGraphics, null, (sysGeoClone as ITopologicalOperator).Buffer(len).Envelope);
                #endregion
            }
             
            
           
                  
        }
        private void DrawPoint(IGeometry pgeo)
        {
            IGraphicsContainer pGraContainer = pAc as IGraphicsContainer;
            //pGraContainer.DeleteAllElements();
           
            //ISymbol psym = (pLayer as IGeoFeatureLayer).Renderer.get_SymbolByFeature(m_Feature);
            ISimpleMarkerSymbol sym = new SimpleMarkerSymbolClass();
            sym.Style = esriSimpleMarkerStyle.esriSMSX;
            sym.Size = 4;
            sym.Color = new RgbColorClass() { Red=200};
            IElement pele;
            IMarkerElement pmarkerEle = new MarkerElementClass();
            pmarkerEle.Symbol = sym as IMarkerSymbol;
            pele = pmarkerEle as IElement;
            pele.Geometry = pgeo;
            pGraContainer.AddElement(pele, 0);
            pAc.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGraphics, null, (pgeo as ITopologicalOperator).Buffer(100).Envelope);
            //m_activeView.Refresh();
        }
        private IPoint ToMapPoint(int x, int y)
        {
            return ToSnapedMapPoint(x, y);
        }
        //获取选择的要素的自由表达几何
        private void getCurrentRepGeo(IPoint pt)
        {
             
            try
            {

                RepGeo = null;
                double dis = pAc.FocusMap.MapScale * 2.5e-3;
                IRelationalOperator rp = (pt as ITopologicalOperator).Buffer(dis) as IRelationalOperator;
                var g = repGraphic;
                g.ResetGeometry();
                // g.Reset();
                int id;
               
                IGeometry geo = null;
                //选中markerGraphic
                while (true)
                {
                    g.NextGeometry(out id, out geo);
                   
                    if (geo == null)
                        break;
                    IGeometry sysGeoClone = (geo as IClone).Clone() as IGeometry;
                    IPoint center = currentFe.ShapeCopy as IPoint;
                    sysGeoClone.Project(pt.SpatialReference);
                    
                    ITransform2D ptrans = sysGeoClone as ITransform2D;
                    ptrans.Scale(new PointClass() { X = 0, Y = 0 }, Scale, Scale);     
                    ptrans.Move(center.X, center.Y);
                   // ptrans.Scale(center, Scale, Scale);                                  
                    sysGeoClone.Project(pt.SpatialReference);
                    if (sysGeoClone is IPolygon)
                    {
                        GraphicsHelper gh = new GraphicsHelper(pAc);
                        gh.DrawPolygon(sysGeoClone as IPolygon);
                    }
                    if (sysGeoClone is IPoint)
                    {
                        GraphicsHelper gh = new GraphicsHelper(pAc);
                        DrawPoint(sysGeoClone as IPoint);
                        //gh.DrawTxt(sysGeoClone as IPoint, "G", 1);
                    }
                    if (!rp.Disjoint(sysGeoClone))//相交
                    {
                        //markerGeo = geo;
                        GeoID = id;
                        RepGeo = sysGeoClone;
                        break;

                    }
                }


            }
            catch (Exception ex)
            {
                return;
            }

           
            
        }
        private void DrawBgMoveEle(IPoint pt)
        {
            if (RepGeo.GeometryType == esriGeometryType.esriGeometryPoint)
            {

                double disbuffer = pMap.MapScale * 2.0e-3;
                IGeometry geobuffer = (pt as ITopologicalOperator).Buffer(disbuffer);

                IEnvelope envelop = geobuffer.Envelope;
                envelop.Expand(1.5, 1.5, true);
                ISimpleFillSymbol sf = new SimpleFillSymbolClass();
                sf.Style = esriSimpleFillStyle.esriSFSHollow;
                sf.Outline = new SimpleLineSymbolClass() { Width = 0.01 };
                IGraphicsContainer gc = pAc as IGraphicsContainer;
                gc.DeleteAllElements();
                IFillShapeElement fillele = new PolygonElementClass();
                fillele.Symbol = sf;
                IElement ele = fillele as IElement;
                pGbEle = ele;
                ele.Geometry = geobuffer;
                gc.AddElement(ele, 0);
                pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, envelop);
            }
            else if (RepGeo.GeometryType == esriGeometryType.esriGeometryPolygon)
            {
                //double dx = pt.X - startPt.X;
                //double dy = pt.Y - startPt.Y;
                IGeometry geobuffer = (RepGeo as IClone).Clone() as IGeometry;
                ITransform2D ptrans = geobuffer as ITransform2D;
                ptrans.Move(0, 0);

                IEnvelope envelop = geobuffer.Envelope;
                envelop.Expand(1.5, 1.5, true);
                ISimpleFillSymbol sf = new SimpleFillSymbolClass();
                sf.Style = esriSimpleFillStyle.esriSFSHollow;
                sf.Outline = new SimpleLineSymbolClass() { Width = 0.01 };
                IGraphicsContainer gc = pAc as IGraphicsContainer;
                gc.DeleteAllElements();
                IFillShapeElement fillele = new PolygonElementClass();
                fillele.Symbol = sf;
                IElement ele = fillele as IElement; pGbEle = ele;
                ele.Geometry = geobuffer;
                gc.AddElement(ele, 0);
                pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, envelop);
            }
            else if (RepGeo.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                //double dx = pt.X - startPt.X;
                //double dy = pt.Y - startPt.Y;
                IGeometry geobuffer = (RepGeo as IClone).Clone() as IGeometry;
                ITransform2D ptrans = geobuffer as ITransform2D;
                ptrans.Move(0, 0);

                IEnvelope envelop = geobuffer.Envelope;
                envelop.Expand(1.5, 1.5, true);
                ISimpleLineSymbol sl = new SimpleLineSymbolClass();
                sl.Width = 0.1;
                IGraphicsContainer gc = pAc as IGraphicsContainer;
                gc.DeleteAllElements();
                ILineElement plineEle = new LineElementClass();
                plineEle.Symbol = sl;
                IElement ele = plineEle as IElement; pGbEle = ele;
                ele.Geometry = geobuffer;
                gc.AddElement(ele, 0);
                pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, envelop);
            }
            pEleGeo = (pGbEle.Geometry as IClone).Clone() as IGeometry;
            
        }

        private void DrawMoveEle(IPoint pt)
        {
            double dx = pt.X - startPt.X;
            double dy = pt.Y - startPt.Y;
            IGeometry pgeo = (pEleGeo as IClone).Clone() as IGeometry;

            //IPoint ptStart = ((pgeo as IArea).LabelPoint as IClone).Clone() as IPoint;
            ITransform2D ptrans = pgeo as ITransform2D;
            //double dx = pt.X - ptStart.X;
            //double dy = pt.Y - ptStart.Y;
            ptrans.Move(dx, dy);
            pGbEle.Geometry = pgeo;
            IEnvelope envelop = pgeo.Envelope;
            envelop.Expand(1.5, 1.5, true);

            pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, envelop);
        }
    }
}

        

            


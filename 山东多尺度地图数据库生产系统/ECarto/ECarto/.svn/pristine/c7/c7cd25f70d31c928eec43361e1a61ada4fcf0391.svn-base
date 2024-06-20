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
using ESRI.ArcGIS.Editor;

namespace SMGI.Plugin.EmergencyMap
{
    public class RepRuleMove : SMGITool
    {
        public RepRuleMove()
        {
            m_caption = "规则移动";

            m_toolTip = "规则移动";
            m_category = "常规";
        }
        private IGeoFeatureLayer geoFlyr = null;
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
        }
        IActiveView view;
        IMap map;
        IScreenDisplay display;
        IDisplayTransformation dis;
        Dictionary<string, List<IRepresentation>> RepFeaturesDic = new Dictionary<string,List<IRepresentation>>();

        ILayer repLayer = null;
        IFeatureClass repFcl = null;
       
        public override void OnClick()
        {
            display = m_Application.MapControl.ActiveView.ScreenDisplay;
            map = m_Application.MapControl.Map;
            view = m_Application.MapControl.ActiveView;
            dis = display.DisplayTransformation;
            RepFeaturesDic.Clear();
            startPt = null;
            moveFlag = false;
            bFindRep = false;
            gelements = new List<IElement>();
            //鼠标状态
            
         
            if (map.SelectionCount == 0)
            {
                return;
            }

            RepFeaturesDic = new Dictionary<string, List<IRepresentation>>();
            IEnumFeature pEnumFeature = (IEnumFeature)map.FeatureSelection;
            ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
            pEnumFeature.Reset();
            IFeature pFeature = null;
            IMapContext mctx = new MapContextClass();
            while ((pFeature = pEnumFeature.Next()) != null)
            {
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.StartsWith(pFeature.Class.AliasName);

                })).ToArray();
                if (lyrs.Length == 0)
                    continue;
                geoFlyr = lyrs[0] as IGeoFeatureLayer;
                mctx.Init((geoFlyr.FeatureClass as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFlyr.AreaOfInterest);
                IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
                IRepresentationClass rpc = repRender.RepresentationClass;
                var rep = rpc.GetRepresentation(pFeature, mctx);
                if (rep == null)
                    continue;
                List<IRepresentation> replist = new List<IRepresentation>();
                if (RepFeaturesDic.ContainsKey(geoFlyr.Name))
                {
                    replist = RepFeaturesDic[geoFlyr.Name];
                }
                replist.Add(rep);
                RepFeaturesDic[geoFlyr.Name] = replist;
            }
            //监听【要素选择集】 
            m_Application.MapControl.OnSelectionChanged += new EventHandler(MapControl_OnSelectionChanged);
           
            
        }
      
        private void MapControl_OnSelectionChanged(object sender, EventArgs e)
        {

            //当点击清除选择集的时候执行
            if(m_Application.MapControl.Map.SelectionCount == 0)
            {
                IGraphicsContainer gc = view as IGraphicsContainer;
                gc.DeleteAllElements();
                startPt = null;
                moveFlag = false;
                bFindRep = false;
                gelements = new List<IElement>();
                RepFeaturesDic = new Dictionary<string, List<IRepresentation>>();
                m_Application.MapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
                view.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, view.Extent);
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                return;
            }
            if (m_Application.MapControl.CurrentTool is RepRuleMove)//
            {
                //RepFeaturesDic = new Dictionary<string, List<IRepresentation>>();
                //IEnumFeature pEnumFeature = (IEnumFeature)map.FeatureSelection;
                //((IEnumFeatureSetup)pEnumFeature).AllFields = true;
                //pEnumFeature.Reset();
                //IFeature pFeature = null;
                //IMapContext mctx = new MapContextClass();
                //while ((pFeature = pEnumFeature.Next()) != null)
                //{
                //    var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                //    {
                //        return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.StartsWith(pFeature.Class.AliasName);

                //    })).ToArray();
                //    geoFlyr = lyrs[0] as IGeoFeatureLayer;
                //    mctx.Init((geoFlyr.FeatureClass as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFlyr.AreaOfInterest);
                //    IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
                //    IRepresentationClass rpc = repRender.RepresentationClass;
                //    var rep = rpc.GetRepresentation(pFeature, mctx);
                //    if (rep == null)
                //        continue;
                //    List<IRepresentation> replist = new List<IRepresentation>();
                //    if (RepFeaturesDic.ContainsKey(geoFlyr.Name))
                //    {
                //        replist.Add(rep);
                //    }
                //    RepFeaturesDic[geoFlyr.Name] = replist;
                //}
            }
        } 

        public override bool Deactivate()
        {
            m_Application.MapControl.OnSelectionChanged -= new EventHandler(MapControl_OnSelectionChanged);
            startPt = null;
            moveFlag = false;
            bFindRep = false;
            gelements = new List<IElement>();
            RepFeaturesDic = new Dictionary<string, List<IRepresentation>>();
            base.m_cursor =Cursors.Arrow;;
            IGraphicsContainer gc = view as IGraphicsContainer;
            gc.DeleteAllElements();
            return true;
        }

        public override bool OnContextMenu(int x, int y)
        {
            return true;
        }

        public override void OnDblClick()
        {

        }

        public override void OnKeyDown(int keyCode, int shift)
        {

        }

        public override void OnKeyUp(int keyCode, int shift)
        {

        }
        IPoint startPt = null;
        bool moveFlag = false;
        bool bFindRep = false;
        List<IElement> gelements = new List<IElement>();
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 2)//右键取消
            {
                startPt = null;
                moveFlag = false;
                bFindRep = false;
                gelements = new List<IElement>();
                RepFeaturesDic = new Dictionary<string, List<IRepresentation>>();
                base.m_cursor = Cursors.Arrow; ;
                IGraphicsContainer gc = view as IGraphicsContainer;
                gc.DeleteAllElements();
                m_Application.MapControl.Map.ClearSelection();
                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, view.Extent);
            }
            if (button != 1)
            {
                return;
            }
            IPoint pt = ToMapPoint(x, y);
            if (bFindRep)//已选择
            {
                moveFlag = true;
                startPt = pt;
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), "move.cur");
                DrawBgMoveEle();

            }
            else// 重新拉开
            {
                (view as IGraphicsContainer).DeleteAllElements();
                RepFeaturesDic.Clear();
                m_Application.MapControl.Map.ClearSelection();
                //第一步拉框选中要素
                #region
                IRubberBand rb = new RubberEnvelopeClass();
                IEnvelope pEnvelope = rb.TrackNew(view.ScreenDisplay, null) as IEnvelope;
                if (pEnvelope.IsEmpty)
                {
                   
                   IPoint queryPoint = ToMapPoint(x, y);
                   IGeometry pgeoBuffer = (queryPoint as ITopologicalOperator).Buffer(5e-3 * m_Application.MapControl.MapScale);
                   pEnvelope = pgeoBuffer.Envelope;
                   
                }
                //选中要素
                IEnvelope mapExtent = m_Application.MapControl.Extent;
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).Visible == true && (l as IGeoFeatureLayer).Selectable == true;

                })).ToArray();

                ISpatialFilter sp = new SpatialFilterClass();
                sp.Geometry = mapExtent;
                sp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IRelationalOperator rel = pEnvelope as IRelationalOperator;
                IGeoFeatureLayer geoFlyr = null;
                IRepresentationClass rpc = null;

                foreach (var l in lyrs)
                {
                    geoFlyr = l as IGeoFeatureLayer;
                    if (geoFlyr.Renderer is IRepresentationRenderer)
                    {
                        IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
                        rpc = repRender.RepresentationClass;
                        IMapContext mctx = new MapContextClass();
                        mctx.Init((geoFlyr.FeatureClass as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, l.AreaOfInterest);
                        rpc.PrepareFilter(sp);
                        IFeatureCursor pCursor = geoFlyr.Search(sp, false);
                        IFeature f = null;
                        while ((f = pCursor.NextFeature()) != null)
                        {
                            var rep = rpc.GetRepresentation(f, mctx);
                            if (rep == null || rel.Disjoint(rep.Shape))
                            {
                                continue;
                            }
                            map.SelectFeature(geoFlyr, f);
                            List<IRepresentation> replist = new List<IRepresentation>();
                            if (RepFeaturesDic.ContainsKey(geoFlyr.Name))
                            {
                                replist = RepFeaturesDic[geoFlyr.Name];
                            }
                            replist.Add(rep);
                            RepFeaturesDic[geoFlyr.Name] = replist;
                        }
                        Marshal.ReleaseComObject(pCursor);
                    }

                }
                #endregion
                view.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, view.Extent);
                
            }
        }


        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            IPoint pt = ToMapPoint(x, y);
            base.m_cursor =Cursors.Arrow;;
            bFindRep = getMoveReps(pt);
            if (bFindRep)
            {
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), "move.cur");
            }
            if (button != 1)
                return;
          
            
           
            if (moveFlag)
            {
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), "move.cur");
                DrawMoveRepEle(pt);
            }

        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (!moveFlag)
                return;
            m_Application.EngineEditor.StartOperation();
            foreach (var kv in RepFeaturesDic)
            {
                var replist = kv.Value;
                foreach (var rep in replist)
                {
                    
                  
                    IFeature fe = rep.Feature;
                    IGeometry geoOld = fe.ShapeCopy;
                    IPoint pt = ToMapPoint(x, y);
                    double dx = pt.X - startPt.X;
                    double dy = pt.Y - startPt.Y;
                    //更改库 几何
                    ITransform2D ptrans = geoOld as ITransform2D;
                    ptrans.Move(dx, dy);
                    fe.Shape = geoOld;
                    fe.Store();
                    //更改制图图元:自由式
                    if (rep.RuleID == -1)
                    {
                        var stopPt = ToSnapedMapPoint(x, y);
                        if (rep.Graphics == null)
                        {
                            IGeometry geo = rep.ShapeCopy;
                            (geo as ITransform2D).Move(dx, dy);
                            rep.Shape = geo;
                        }
                        else
                        {
                            var g = rep.Graphics;
                            g.ResetGeometry();
                            while (true)
                            {
                                IGeometry geo;
                                int id;
                                g.NextGeometry(out id, out geo);
                                if (geo == null)
                                    break;

                                var trans = (geo as IClone).Clone() as ITransform2D;
                                trans.Move(dx, dy);
                                g.ChangeGeometry(id, trans as IGeometry);
                            }
                            rep.Graphics = g;
                        }
                        rep.UpdateFeature();
                        rep.Feature.Store();
                        //关联注记平移
                        RelatedAnnoFeatuesMove(fe, dx, dy);
                        (view as IGraphicsContainer).DeleteAllElements();
                       
                    }
                        //#endregion
                     
                }
            }
            //初始化参数
            startPt = null;
            moveFlag = false;
            bFindRep = false;
            //gelements = new List<IElement>();
            //RepFeaturesDic = new Dictionary<string, List<IRepresentation>>();
            base.m_cursor =Cursors.Arrow;;
            IGraphicsContainer gc = view as IGraphicsContainer;
            gc.DeleteAllElements();
           
            view.PartialRefresh(esriViewDrawPhase.esriViewAll, null, view.Extent);
            m_Application.EngineEditor.StopOperation("制图移动");
        }
        //关联注记移动
        private void RelatedAnnoFeatuesMove(IFeature sourcefe, double dx, double dy)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "AnnotationClassID = " + sourcefe.Class.ObjectClassID + "  and FeatureID =" + sourcefe.OID;
            IFeature fe;
            string anno = "ANNO";
            if (sourcefe.Class.AliasName == "LPOINT")
            {
                anno = "LANNO";
            }
            ILayer annoLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == (anno))).FirstOrDefault();
            IFeatureClass annoFcl = (annoLayer as IFeatureLayer).FeatureClass;
            if (annoFcl.FeatureCount(qf) == 0)
                return;
            IFeatureCursor cursor = annoFcl.Search(qf, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                IAnnotationFeature pAnnoFeature = fe as IAnnotationFeature;

                ITextElement pTextElement = new TextElementClass();
                ITextSymbol pTextSymbol = ((pAnnoFeature.Annotation as ITextElement).Symbol as IClone).Clone() as ITextSymbol;
                pTextElement.Symbol = pTextSymbol;
                pTextElement.Text = (pAnnoFeature.Annotation as ITextElement).Text;
                pTextElement.ScaleText = (pAnnoFeature.Annotation as ITextElement).ScaleText;

                ITransform2D ptrans = pAnnoFeature.Annotation.Geometry as ITransform2D;
                ptrans.Move(dx, dy);
                IElement pElement = pTextElement as IElement;
                pElement.Geometry = ptrans as IGeometry;
                pAnnoFeature.Annotation = pElement;
                fe.Store();
            }

            Marshal.ReleaseComObject(cursor);
        }
      
        public override void Refresh(int hdc)
        {

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
        private IPoint ToMapPoint(int x, int y)
        {
            return ToSnapedMapPoint(x, y);
        }
        private bool getMoveReps(IPoint pt)
        {
            bool flag = false;
          
            double disen= map.MapScale * 2.5e-3;//图上5 mm
            IEnvelope en = new EnvelopeClass();
            en.PutCoords(pt.X - disen, pt.Y - disen, pt.X + disen, pt.Y + disen);

            IRelationalOperator rp = en as IRelationalOperator;
           
            foreach (var kv in RepFeaturesDic)
            {
                var replist = kv.Value;
                foreach (var rep in replist)
                {
                    if (!rp.Disjoint(rep.Feature.ShapeCopy))//相交
                    {
                        flag = true;
                        break;
                    }
                }

            }
            return flag;
        }

        private void DrawBgMoveEle()
        {
            gelements = new List<IElement>();
            IGraphicsContainer gc = view as IGraphicsContainer;
            gc.DeleteAllElements();

            foreach (var kv in RepFeaturesDic)
            {
                var replist = kv.Value;
                foreach (var rep in replist)
                {
                    IGeometry geo = rep.Feature.ShapeCopy;
                    IElement ele = null;
                    switch (geo.GeometryType)
                    {
                        case esriGeometryType.esriGeometryPoint:
                            ISimpleMarkerSymbol sm = new SimpleMarkerSymbolClass();
                            sm.Color = new RgbColorClass { NullColor=true };
                            sm.Size = 10;
                            sm.Style = esriSimpleMarkerStyle.esriSMSCircle;
                            sm.Outline = true;
                            sm.OutlineColor = new RgbColorClass { Red = 200 };
                            sm.OutlineSize = 0.1;
                            MarkerElementClass markerEle = new MarkerElementClass();
                            markerEle.Symbol = sm;
                            ele = markerEle as IElement;
                            ele.Geometry = geo;

                            break;
                        case  esriGeometryType.esriGeometryPolyline:
                            ISimpleLineSymbol sl = new SimpleLineSymbolClass();
                            sl.Style = esriSimpleLineStyle.esriSLSSolid;
                            sl.Color = new RgbColorClass { Red = 200 };
                            LineElementClass lineEle = new LineElementClass();
                            lineEle.Symbol = sl;
                            ele = lineEle as IElement;
                            ele.Geometry = geo;
                            break;
                        case esriGeometryType.esriGeometryPolygon:
                            ISimpleFillSymbol sf = new SimpleFillSymbolClass();
                            sf.Style = esriSimpleFillStyle.esriSFSHollow;
                            sf.Outline = new SimpleLineSymbolClass() { Width = 0.01, Color = new RgbColorClass { Red = 200 } };
                            IFillShapeElement fillele = new PolygonElementClass();
                            fillele.Symbol = sf;
                            ele = fillele as IElement;
                            ele.Geometry = geo;
                            break;
                        default:
                            break;
                    }
                    gelements.Add(ele);
                    gc.AddElement(ele, 0);
                    FreeRuleGeoEnvelop(rep);
                }
            }
            view.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, view.Extent);
            
        }

        //自由表达要素的范围矩形
        private void FreeRuleGeoEnvelop(IRepresentation rep)
        {
            IFeature fe = rep.Feature;
            if (fe.Class.AliasName.ToUpper() != "LPOINT")
                return;
            if (rep.RuleID != -1)
                return;
            //处理[整饰、专题图表] 要素

            IPoint center = fe.ShapeCopy as IPoint;
            IEnvelope envelop = new EnvelopeClass();
            try
            {
                 
              
                IRepresentationGraphics repGraphic = rep.Graphics as IRepresentationGraphics;
                repGraphic.Reset();
                IGeometry symGeo=null;
                IRepresentationRule rule = null;
                repGraphic.ResetGeometry();
                IPoint orgin = new PointClass { X = 0, Y = 0 };
                double minx = 0, maxx = 0, miny = 0, maxy = 0;//外接范围
                while (true)
                {
                    repGraphic.Next(out symGeo, out rule);
                    if (symGeo == null)
                        break;
                    //处理自由表达 图元
                    IBasicSymbol basicSym = rule.get_Layer(0);
                    IGraphicAttributes at = basicSym as IGraphicAttributes;
                    if (at == null)
                        continue;
                    IRepresentationMarker mm = at.get_Value(1) as IRepresentationMarker;
                    double size = Convert.ToDouble(at.get_Value(2));
                    double width = Math.Max(mm.Width, mm.Height);
                    double scale = size / 2.83465 * m_Application.ActiveView.FocusMap.ReferenceScale * 1.0e-3;
                    scale = scale / width;
                    IRepresentationGraphics repFreeGraphic = mm as IRepresentationGraphics;
                    repFreeGraphic.ResetGeometry();
                    int id;
                    IGeometry freeGeo;
                    repFreeGraphic.ResetGeometry();
                    while (true)
                    {
                        #region
                        repFreeGraphic.NextGeometry(out id, out freeGeo);
                        if (freeGeo == null)
                            break;
                        IGeometry geoclone = (freeGeo as IClone).Clone() as IGeometry;
                        (geoclone as ITransform2D).Scale(orgin, scale, scale);
                        if (geoclone is IPolyline)
                        {

                            IEnvelope pextent = (geoclone as IPolyline).Envelope;
                            if (pextent.XMax > maxx)
                            {
                                maxx = pextent.XMax;
                            }
                            if (pextent.YMax > maxy)
                            {
                                maxy = pextent.YMax;
                            }
                            if (pextent.YMin < miny)
                            {
                                miny = pextent.YMin;
                            }
                            if (pextent.XMin < minx)
                            {
                                minx = pextent.XMin;
                            }

                            //geometrys.Add(geoclone);
                        }
                        if (geoclone is IPolygon)
                        {
                            IEnvelope pextent = (geoclone as IPolygon).Envelope;
                            if (pextent.XMax > maxx)
                            {
                                maxx = pextent.XMax;
                            }
                            if (pextent.YMax > maxy)
                            {
                                maxy = pextent.YMax;
                            }
                            if (pextent.YMin < miny)
                            {
                                miny = pextent.YMin;
                            }
                            if (pextent.XMin < minx)
                            {
                                minx = pextent.XMin;
                            }

                            //geometrys.Add(geoclone);
                        }
                        #endregion
                    }
                }
               
                envelop.PutCoords(minx, miny, maxx, maxy);
                ITransform2D transform2D = envelop as ITransform2D;
                transform2D.Move(center.X, center.Y);
                RectangleElementClass rectele = new RectangleElementClass();
                rectele.Geometry = envelop;
                ISimpleFillSymbol sf = new SimpleFillSymbolClass();
                sf.Style = esriSimpleFillStyle.esriSFSHollow;
                sf.Outline = new SimpleLineSymbolClass() { Width = 0.01, Color = new RgbColorClass { Red = 200 } };
                rectele.Symbol = sf;

                gelements.Add(rectele);
                IGraphicsContainer gc = view as IGraphicsContainer;
                gc.AddElement(rectele, 0);
                 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("制图规则几何失败：" + ex.Message);
            }
        }
        private void DrawMoveRepEle(IPoint pt)
        {
            IGraphicsContainer gc = view as IGraphicsContainer;
            gc.DeleteAllElements();
            foreach (var ele in gelements)
            {

                var elecurent = (ele as IClone).Clone() as IElement;
                IGeometry geo = elecurent.Geometry;
                ITransform2D ptrans = geo as ITransform2D;
                double dx = pt.X - startPt.X;
                double dy = pt.Y - startPt.Y;
                ptrans.Move(dx, dy);
                elecurent.Geometry = geo;
                gc.AddElement(elecurent, 0);
                 
            }
            view.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, view.Extent);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using System.Data;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.EmergencyMap
{
    
    public class MapElementMoveCmd : SMGITool
    {
        bool isInnerSnaped = false;
        private IActiveView pAc;
        private IMap pMap;
        ILayer repLayer = null;
        IFeatureClass repFcl = null;

        public MapElementMoveCmd()
        {
            m_caption = "整饰要素移动";
            m_toolTip = "整饰要素移动工具";
            m_category = "常规";
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
        }

        public override void OnClick()
        {
            pAc = m_Application.ActiveView;
            pMap = m_Application.ActiveView.FocusMap;
            repLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOINT"))).FirstOrDefault();
            repFcl = (repLayer as IFeatureLayer).FeatureClass;
            FeSelects = new List<IFeature>();
        }
        IPoint startPt;
        List<IFeature> FeSelects = null;
        IFeature currentFe = null;//当前要素

        public override void OnMouseDown(int button, int shift, int x, int y)
        {

            if (button != 1)
            {
                return;
            }
            IPoint pt = ToMapPoint(x, y);
            currentFe = getCurrentFe(pt);
            if (currentFe != null)
            {
                startPt = pt;
                DrawBgMoveEle(pt);
            }
            else
            {
                FeSelects.Clear();
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
                pSpatialFilter.WhereClause = "TYPE='图例' or TYPE='指北针' or TYPE='比例尺' or  TYPE='附图' ";
                IFeatureCursor pcursor = repFcl.Search(pSpatialFilter, false);
                IFeature fe = null;
                while ((fe = pcursor.NextFeature()) != null)
                {
                    FeSelects.Add(fe);
                    pMap.SelectFeature(repLayer, fe);
                }
                pAc.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, pAc.Extent);
            }
        }
        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;
            IPoint pt = ToMapPoint(x, y);
            if (currentFe != null)
            {
                DrawMoveEle(pt);
            }



        }
        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (currentFe == null)
                return;
            m_Application.EngineEditor.StartOperation();
            IPoint ptold = currentFe.ShapeCopy as IPoint;
            IPoint pt = ToMapPoint(x, y);
            double dx = pt.X - ptold.X;
            double dy = pt.Y - ptold.Y;
            foreach (var fe in FeSelects)
            {
                currentFe = fe;
                #region
                //更改库
                ptold = currentFe.ShapeCopy as IPoint;
                IPoint ptnew = new PointClass();
                ptnew.X = ptold.X + dx;
                ptnew.Y = ptold.Y + dy;
                currentFe.Shape = ptnew;
                currentFe.Store();
                //更改图 
                var stopPt = ToSnapedMapPoint(x, y);
                var rep = ObtainRep(currentFe);
                dx = stopPt.X - startPt.X;
                dy = stopPt.Y - startPt.Y;
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
                //关联要素平移
                string fetype = currentFe.get_Value(repFcl.FindField("TYPE")).ToString();
                RelatedFeatuesMove(fetype, dx, dy);
                RelatedAnnoFeatuesMove(currentFe, dx, dy);
                (pAc as IGraphicsContainer).DeleteAllElements();
                pAc.Refresh();
                #endregion
            }
            currentFe = null;
            m_Application.EngineEditor.StopOperation("整式要素移动");

        }
        //关联注记移动
        private void RelatedAnnoFeatuesMove(IFeature sourcefe, double dx, double dy)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "AnnotationClassID = " + sourcefe.Class.ObjectClassID + "  and FeatureID =" + sourcefe.OID;
            IFeature fe;

            ILayer annoLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LANNO"))).FirstOrDefault();
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
      

        //关联要素平移
        private void RelatedFeatuesMove(string filter, double dx, double dy)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE = '" + filter + "'";
            IFeature fe;

            //ILayer annoLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LANNO"))).FirstOrDefault();
            //IFeatureClass annoFcl = (annoLayer as IFeatureLayer).FeatureClass;
            //IFeatureCursor cursor = annoFcl.Search(qf, false);
            //while ((fe = cursor.NextFeature()) != null)
            //{
            //    IAnnotationFeature pAnnoFeature = fe as IAnnotationFeature;

            //    ITextElement pTextElement = new TextElementClass();
            //    ITextSymbol pTextSymbol = ((pAnnoFeature.Annotation as ITextElement).Symbol as IClone).Clone() as ITextSymbol;
            //    pTextElement.Symbol = pTextSymbol;
            //    pTextElement.Text = (pAnnoFeature.Annotation as ITextElement).Text;
            //    pTextElement.ScaleText = (pAnnoFeature.Annotation as ITextElement).ScaleText;

            //    ITransform2D ptrans = pAnnoFeature.Annotation.Geometry as ITransform2D;
            //    ptrans.Move(dx, dy);
            //    IElement pElement = pTextElement as IElement;
            //    pElement.Geometry = ptrans as IGeometry;
            //    pAnnoFeature.Annotation = pElement;
            //    fe.Store();
            //}

            ILayer polygonLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOLY"))).FirstOrDefault();
            IFeatureClass polygonFcl = (polygonLayer as IFeatureLayer).FeatureClass;
            qf.WhereClause = "TYPE like '" + filter + "%'";
            IFeatureCursor cursor = polygonFcl.Search(qf, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                ITransform2D ptrans = fe.ShapeCopy as ITransform2D;
                ptrans.Move(dx, dy);
                fe.Shape = ptrans as IGeometry;
                fe.Store();
            }
            Marshal.ReleaseComObject(cursor);
        }
        IElement pGbEle = null;
        private IFeature getCurrentFe(IPoint pt)
        {
            IFeature currentFe = null;
            foreach (var fe in FeSelects)
            {
                double disbuffer = pMap.MapScale * 3.5e-3;
                double dis = pAc.ScreenDisplay.DisplayTransformation.FromPoints(5);
                dis = disbuffer;
                IRelationalOperator rp = (pt as ITopologicalOperator).Buffer(dis) as IRelationalOperator;
                if (!rp.Disjoint(fe.ShapeCopy))
                {
                    currentFe = fe;
                    break;
                }

            }
            return currentFe;
        }
        private void DrawBgMoveEle(IPoint pt)
        {
            double disbuffer = pMap.MapScale * 2.5e-3;
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
            ele.Geometry = geobuffer;
            gc.AddElement(ele, 0);
            pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, envelop);
            pGbEle = ele;
        }
        private void DrawMoveEle(IPoint pt)
        {
            IGeometry pgeo = pGbEle.Geometry;

            IPoint ptStart = ((pgeo as IArea).LabelPoint as IClone).Clone() as IPoint;
            ITransform2D ptrans = pgeo as ITransform2D;
            double dx = pt.X - ptStart.X;
            double dy = pt.Y - ptStart.Y;
            ptrans.Move(dx, dy);
            pGbEle.Geometry = pgeo;
            IEnvelope envelop = pgeo.Envelope;
            envelop.Expand(1.5, 1.5, true);

            pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, envelop);
        }
        //绘制 
        private void DrawCross(IPoint pt)
        {
            IDisplay display = pAc.ScreenDisplay;
            var dis = new SimpleDisplayClass
            {
                DisplayTransformation = m_Application.ActiveView.ScreenDisplay.DisplayTransformation,
            };
            display.StartDrawing((display as IDisplay).hDC, (short)esriScreenCache.esriNoScreenCache);
            double disbuffer = pAc.ScreenDisplay.DisplayTransformation.FromPoints(1);
            IGeometry geobuffer = (pt as ITopologicalOperator).Buffer(disbuffer);

            IEnvelope envelop = geobuffer.Envelope;
            envelop.Expand(2, 2, true);
            ISimpleFillSymbol sf = new SimpleFillSymbolClass();
            sf.Style = esriSimpleFillStyle.esriSFSHollow;
            sf.Outline = new SimpleLineSymbolClass() { Width = 0.01 };
            ISymbol sketchSymbol = sf as ISymbol;
            display.SetSymbol(sketchSymbol);
            display.DrawPolygon(geobuffer as IPolygon);
            display.FinishDrawing();
            pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, envelop);

            Marshal.ReleaseComObject(sketchSymbol);

        }

        private IRepresentation ObtainRep(IFeature fe)
        {
            var rp = (repLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            var mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            return rep;
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
    }
}

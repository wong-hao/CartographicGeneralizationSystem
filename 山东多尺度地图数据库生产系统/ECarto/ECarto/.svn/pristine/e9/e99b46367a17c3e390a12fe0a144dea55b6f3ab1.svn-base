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
    public  class MapElementRepRuleEditor:SMGITool
    {
        private IActiveView pAc = null;
        private IGeoFeatureLayer geoFlyr = null;
        private IFeatureClass pfclPoint = null;
        private IMap pMap = null;
        private List<IFeature> FeSelects = new List<IFeature>();
        public MapElementRepRuleEditor()
        {
            m_caption = "规则编辑";
            
            m_toolTip = "规则编辑";
            m_category = "常规";
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
        }
        public override void OnClick()
        {
            pAc=m_Application.ActiveView;
            pMap = pAc as IMap;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && ((l as IFeatureLayer).FeatureClass.AliasName=="LPOINT");

            })).ToArray();
            geoFlyr = lyrs[0] as IGeoFeatureLayer;
            pfclPoint = geoFlyr.FeatureClass;
           
        }

     

       
     

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
           
            FeSelects.Clear();
            if (button != 1)
            {
                return;
            }
            
            var display = m_Application.MapControl.ActiveView.ScreenDisplay;
            var map = m_Application.MapControl.Map;
            var view = m_Application.MapControl.ActiveView;
            var dis = display.DisplayTransformation;
            
           
            IRubberBand pRubber=new RubberEnvelopeClass();
            IRubberBand rb = new RubberEnvelopeClass();
            IEnvelope pEnvelope = rb.TrackNew(pAc.ScreenDisplay, null) as IEnvelope;
            if (pEnvelope.IsEmpty)
             {
                 IPoint queryPoint = ToMapPoint(x, y);
                 IGeometry pgeoBuffer=   (queryPoint as ITopologicalOperator).Buffer(5e-3*m_Application.MapControl.MapScale);
                 pEnvelope=pgeoBuffer.Envelope;
             }
          
            
            //选中要素
             ISpatialFilter pSpatialFilter = null;
             pSpatialFilter = new SpatialFilterClass();
             pSpatialFilter.Geometry = pEnvelope;
             pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
             IFeatureCursor pcursor = pfclPoint.Search(pSpatialFilter, false);
             IFeature fe = null;
             while ((fe = pcursor.NextFeature()) != null)
             {
                 string type = fe.get_Value(fe.Fields.FindField("TYPE")).ToString();
                 if (type == "图例" || type == "比例尺" || type == "指北针" || type == "附图")
                 {
                     FeSelects.Add(fe);
                     pMap.SelectFeature(geoFlyr, fe);
                 }
              
             }
             Marshal.ReleaseComObject(pcursor);
             Marshal.ReleaseComObject(pSpatialFilter);

             
            
           
            if (FeSelects.Count>0)
            {
                RepresentationRuleEditorClass ruleEditor = new RepresentationRuleEditorClass();
                //根据RulueID取到原始的rule，对原始的rule的属性进行覆盖
                IPoint featureGeo = FeSelects[0].ShapeCopy as IPoint;
                IRepresentation rep = null;
                var r = RuleValueSet(FeSelects[0],ref rep);
                object size = (r.get_Layer(0) as IGraphicAttributes).get_Value(2);
		        //弹出属性编辑对话框，单机确定后更新rep中的属性
                if (ruleEditor.DoModal(m_Application.MapControl.hWnd, esriGeometryType.esriGeometryPoint, r))
                {
                    var newRule = ruleEditor.Rule;
                    m_Application.EngineEditor.StartOperation();
                    IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
                    repGraphics.Add(featureGeo, newRule);
                    rep.Graphics = repGraphics;
                    rep.UpdateFeature();
                    rep.Feature.Store();
                    object sizenew = (newRule.get_Layer(0) as IGraphicAttributes).get_Value(2);
                    double ratio = Convert.ToDouble(sizenew) / Convert.ToDouble(size);
                    //关联的注记
                    MoveAnno(FeSelects[0].OID, featureGeo, ratio);
                    ResizeAnno(FeSelects[0].OID, ratio);
                    //关联的要素
                    string fetype = FeSelects[0].get_Value( FeSelects[0].Class.FindField("TYPE")).ToString();
                    RelatedFeatuesScale(fetype,ratio, featureGeo);
                    m_Application.EngineEditor.StopOperation("规则编辑");
                   
                }

              
               
            }
            view.PartialRefresh( esriViewDrawPhase.esriViewAll,null,view.Extent);
        }
        //关联要素缩放
        private void RelatedFeatuesScale(string filter,double scale,IPoint anchor)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE = '" + filter + "'";
            IFeature fe;
            ILayer polygonLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOLY"))).FirstOrDefault();
            IFeatureClass polygonFcl = (polygonLayer as IFeatureLayer).FeatureClass;
            qf.WhereClause = "TYPE like '" + filter + "%'";
            IFeatureCursor cursor = polygonFcl.Search(qf, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                ITransform2D ptrans = fe.ShapeCopy as ITransform2D;
                ptrans.Scale(anchor, scale, scale);
                fe.Shape = ptrans as IGeometry;
                fe.Store();
            }
            Marshal.ReleaseComObject(cursor);
        }
        private void ResizeAnno(int id, double rat)
        {
            var  annoly = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFDOGraphicsLayer) && (((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LANNO")))).FirstOrDefault();
            var  annofcl = (annoly as IFeatureLayer).FeatureClass;

            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "FeatureID=" + id;
            int indexFontSize = annofcl.FindField("FontSize");
            IFeatureCursor fcur = annofcl.Search(qf, false);
            IFeature pFeature = null;
            while ((pFeature = fcur.NextFeature()) != null)
            {
                IAnnotationFeature2 annoFeature = pFeature as IAnnotationFeature2;
                ITextElement annoTxtEle = annoFeature.Annotation as ITextElement;
                double sizeTxt = annoTxtEle.Symbol.Size;
                double reSize = sizeTxt * rat;
                pFeature.set_Value(indexFontSize, reSize);
                pFeature.Store();
            }
            Marshal.ReleaseComObject(fcur);
        }
        private void MoveAnno(int id, IPoint feCenterPoint, double rat)
        {
            var annoly = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFDOGraphicsLayer) && (((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LANNO")))).FirstOrDefault();
            var annofcl = (annoly as IFeatureLayer).FeatureClass;
            IPoint frompoint = new PointClass() { X = feCenterPoint.X, Y = feCenterPoint.Y };
            IPoint topoint = null;
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "FeatureID=" + id;

            int indexFontSize = annofcl.FindField("FontSize");
            IFeatureCursor fcur = annofcl.Search(qf, false);
            IFeature pFeature = null;
            while ((pFeature = fcur.NextFeature()) != null)
            {
                IAnnotationFeature2 annoFeature = pFeature as IAnnotationFeature2;
                IElement annoEle = annoFeature.Annotation;
                IGeometry geo = annoEle.Geometry;
                topoint = geo as IPoint;
                double x = topoint.X - frompoint.X;
                double y = topoint.Y - frompoint.Y;
                double dx = 0;
                double dy = 0;
                double length = Math.Sqrt(x * x + y * y);
                if (length != 0)
                {
                    double length2 = length * rat - length;
                    double cos = (topoint.X - frompoint.X) / length;
                    double sin = (topoint.Y - frompoint.Y) / length;
                    dx = length2 * cos;
                    dy = length2 * sin;
                }
                IPoint pt = new PointClass() { X = topoint.X + dx, Y = topoint.Y + dy };

                //IGeometry geo = pt as IGeometry;
                ITransform2D trans2d = geo as ITransform2D;
                trans2d.Move(dx, dy);
                IGeometry geo2 = trans2d as IGeometry;
                annoEle.Geometry = geo2;
                annoFeature.Annotation = annoEle;
                pFeature.Store();
            }
            Marshal.ReleaseComObject(fcur);
        }



        private IRepresentationRule RuleValueSet(IFeature fe, ref IRepresentation rep)
        {
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            //初始化
            ILayer repLayer = geoFlyr;
            var rp = (repLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics graphics =   rep.Graphics  as IRepresentationGraphics;
            graphics.Reset();        
            IRepresentationRule r;
            IGeometry geo;
            graphics.Next(out geo, out r);
            return r;
         }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
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
    }
}

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

namespace SMGI.Plugin.EmergencyMap.CartoEdit
{
    public  class RepRuleEditor:SMGITool
    {
        public RepRuleEditor()
        {
            m_caption = "规则编辑";
            
            m_toolTip = "规则编辑";
            m_category = "常规";
        }
        private IGeoFeatureLayer geoFlyr = null;
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
        }
        public override void OnClick()
        {
            var display = m_Application.MapControl.ActiveView.ScreenDisplay;
            var map = m_Application.MapControl.Map;
            var view = m_Application.MapControl.ActiveView;
            var dis = display.DisplayTransformation;
            if (map.SelectionCount == 0)
            {
                return;
            }
            IEnumFeature pEnumFeature = (IEnumFeature)map.FeatureSelection;
            ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
            IFeature pFeature = pEnumFeature.Next();

            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.StartsWith(pFeature.Class.AliasName);

            })).ToArray();
            if (lyrs.Length == 0)
                return;
            geoFlyr = lyrs[0] as IGeoFeatureLayer;
            IMapContext mctx = new MapContextClass();
            mctx.Init((geoFlyr.FeatureClass as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFlyr.AreaOfInterest);
            IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
            IRepresentationClass rpc = repRender.RepresentationClass;
            var rep = rpc.GetRepresentation(pFeature, mctx);

            //当前选择要素不为空，那么判断鼠标指针落脚点是否在某个定点附近
            if (rep != null )
            {
                RepresentationRuleEditorClass ruleEditor = new RepresentationRuleEditorClass();
                //根据RulueID取到原始的rule，对原始的rule的属性进行覆盖
                IRepresentationRule r = null;
                if (rep.RuleID == -1)
                {
                    r = FreeRuleValueSet(rep);
                }
                else
                {
                    r = RuleValueSet(rep);
                }

		        //弹出属性编辑对话框，单机确定后更新rep中的属性
                if (ruleEditor.DoModal(m_Application.MapControl.hWnd, rep.Shape.GeometryType, r))
                {
                    var newRule = ruleEditor.Rule;
                    if (rep.RuleID == -1)
                    {
                        ThematicAnnoSet(pFeature, r, newRule);
                        OverrideFreeValueSet(newRule, rep, pFeature.ShapeCopy);
                    }
                    else
                    {
                        OverrideValueSet(newRule, rep);
                    }
               }

                m_Application.EngineEditor.StartOperation();
                rep.UpdateFeature();
                rep.Feature.Store();
                m_Application.EngineEditor.StopOperation("规则编辑");
               
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rep);
            }
            view.PartialRefresh( esriViewDrawPhase.esriViewAll,null,view.Extent);
        }
        #region 自由式制图表达重写
        public void OverrideFreeValueSet(IRepresentationRule newRule,IRepresentation rep, IGeometry featureGeo)
        {
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(featureGeo, newRule);
            rep.Graphics = repGraphics;
        }
        #endregion
        #region OverrideValueSet Functions
        public void OverrideValueSet(IRepresentationRule newRule, IRepresentation rep)
        {
            var ruleOrg = (rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID) as IClone).Clone() as IRepresentationRule;
            var rule = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            OverrideValueSet(rule as IGeometricEffects, newRule as IGeometricEffects, rep);
            for (int i = 0; i < rule.LayerCount; i++)
            {
                var layer = rule.Layer[i];
                var newLayer = newRule.Layer[i];
                //全局效果（几何效果集合）
                OverrideValueSet(layer as IGeometricEffects, newLayer as IGeometricEffects, rep);
                OverrideValueSet(layer as IGraphicAttributes, newLayer as IGraphicAttributes, rep);

                if (layer is IBasicMarkerSymbol)
                {
                    OverrideValueSet((layer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes,
                        (newLayer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes, rep);
                }
                else if (layer is IBasicLineSymbol)
                {
                    OverrideValueSet((layer as IBasicLineSymbol).Stroke as IGraphicAttributes,
                        (newLayer as IBasicLineSymbol).Stroke as IGraphicAttributes, rep);
                }
                else if (layer is IBasicFillSymbol)
                {
                    OverrideValueSet((layer as IBasicFillSymbol).FillPattern as IGraphicAttributes,
                        (newLayer as IBasicFillSymbol).FillPattern as IGraphicAttributes, rep);
                }
            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);

        }
        public void OverrideValueSet(IGeometricEffects ges, IGeometricEffects newges, IRepresentation rep)
        {
            if (ges == null || newges == null || rep == null)
                return;
            for (int j = 0; j < ges.Count; j++)
            {
                var ge = ges.Element[j];
                var newga = newges.Element[j] as IGraphicAttributes;
                IGraphicAttributes ga = ge as IGraphicAttributes;
                OverrideValueSet(ga, newga, rep);
            }
        }
        public void OverrideValueSet(IGraphicAttributes ga, IGraphicAttributes newga, IRepresentation rep)
        {
            if (ga == null || newga == null || rep == null)
                return;
            for (int k = 0; k < ga.GraphicAttributeCount; k++)
            {
                var id = ga.get_ID(k);
                object obj = newga.get_Value(id);
                if (obj is GraphicAttributeSizeTypeClass || obj is double)
                {
                    double newValue = (double)obj;
                    newValue = Math.Round(newValue * 2.83, 2);
                    rep.set_Value(ga, id, newValue);
                }
                else
                {
                    rep.set_Value(ga, id, obj);
                }

            }
        }
        #endregion

        #region RuleValueSet Functions
        public void RuleValueSet(IGeometricEffects ges, IGeometricEffects newges, IRepresentation rep)
        {
            if (ges == null || newges == null || rep == null)
            {
                return;
            }
            for (int j = 0; j < ges.Count; j++)
            {
                var ge = ges.Element[j];
                var newge = newges.Element[j];
                var ga = ge as IGraphicAttributes;
                var newga = newge as IGraphicAttributes;
                RuleValueSet(ga, newga, rep);
            }
        }

        public void RuleValueSet(IGraphicAttributes ga, IGraphicAttributes newga, IRepresentation rep)
        {
            if (ga == null || newga == null || rep == null)
            {
                return;
            }
            for (int k = 0; k < ga.GraphicAttributeCount; k++)
            {
                var id = ga.get_ID(k);
                object obj = rep.get_Value(ga, id);
                if (obj is GraphicAttributeSizeTypeClass || obj is double)
                {
                    double newValue = (double)obj;
                    newValue = Math.Round(newValue * 0.3528, 2);
                    newga.set_Value(id, newValue);
                }
                else
                {
                    newga.set_Value(id, obj);
                }
            }
        }
        private IRepresentationRule FreeRuleValueSet(IRepresentation rep)
        {
            //IMapContext mc = new MapContextClass();
            //mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            ////初始化
            //ILayer repLayer = geoFlyr;
            //var rp = (repLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            //IRepresentationClass m_RepClass = rp.RepresentationClass;
            //rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics graphics = rep.Graphics as IRepresentationGraphics;
            graphics.Reset();
            IRepresentationRule r;
            IGeometry geo;
            graphics.Next(out geo, out r);
            return r;
        }
        public IRepresentationRule RuleValueSet(IRepresentation rep)
        {
            var r = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            var newrule = (r as IClone).Clone() as IRepresentationRule;
            RuleValueSet(r as IGeometricEffects, newrule as IGeometricEffects, rep);
            for (int i = 0; i < newrule.LayerCount; i++)
            {
                var layer = r.Layer[i];
                var newlayer = newrule.Layer[i];
                RuleValueSet(layer as IGeometricEffects, newlayer as IGeometricEffects, rep);
                RuleValueSet(layer as IGraphicAttributes, newlayer as IGraphicAttributes, rep);
                if (layer is IBasicMarkerSymbol)
                {
                    RuleValueSet((layer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes,
                        (newlayer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes, rep);
                }
                else if (layer is IBasicLineSymbol)
                {
                    RuleValueSet((layer as IBasicLineSymbol).Stroke as IGraphicAttributes,
                        (newlayer as IBasicLineSymbol).Stroke as IGraphicAttributes, rep);
                }
                else if (layer is IBasicFillSymbol)
                {
                    RuleValueSet((layer as IBasicFillSymbol).FillPattern as IGraphicAttributes,
                       (newlayer as IBasicFillSymbol).FillPattern as IGraphicAttributes, rep);
                }
            }
            return newrule;
        }
        #endregion

        #region 专题图关联处理
        private void ThematicAnnoSet(IFeature fe, IRepresentationRule orginRule,IRepresentationRule newRule)
        {
            if (fe.Class.AliasName.ToUpper() != "LPOINT")
                return;
            int indexType = fe.Fields.FindField("TYPE");
            if (indexType == -1)
                return;
            if (fe.get_Value(indexType).ToString() != "专题图")
                return;
            double size =double.Parse((orginRule.get_Layer(0) as IGraphicAttributes).get_Value(2).ToString());
            double size2 = double.Parse((newRule.get_Layer(0) as IGraphicAttributes).get_Value(2).ToString());
            var marker = (orginRule.get_Layer(0) as IGraphicAttributes).get_Value(1) as IRepresentationMarker;
            if (size2 == size)
            {
                var marker2 = (newRule.get_Layer(0) as IGraphicAttributes).get_Value(1) as IRepresentationMarker;
                double sx = marker2.Width / marker.Width;
                double sy = marker2.Height / marker.Height;
            }
            else
            {
                m_Application.EngineEditor.StartOperation();
                double ratio = Convert.ToDouble(size2) / Convert.ToDouble(size);
                moveAnno(fe.OID, fe.ShapeCopy as IPoint, ratio);
                ResizeAnno(fe.OID, ratio);
                m_Application.EngineEditor.StopOperation("关联编辑");
            }
        }
        private void ResizeAnno(int id, double rat)
        {
            var annoly = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFDOGraphicsLayer) && ((l.Name.ToUpper() == ("LANNO")))).FirstOrDefault();
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
        private void moveAnno(int id, IPoint feCenterPoint, double rat)
        {
            var annoly = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LANNO"))).FirstOrDefault();
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
        #endregion
        public override bool Deactivate()
        {
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

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            IRepresentation rep = null;
            #region 选择
            if (button != 1)
            {
                return;
            }
            bool bFindRule=false; 
            var display = m_Application.MapControl.ActiveView.ScreenDisplay;
            var map = m_Application.MapControl.Map;
            var view = m_Application.MapControl.ActiveView;
            var dis = display.DisplayTransformation;

            IRubberBand pRubber = new RubberEnvelopeClass();
            IRubberBand rb = new RubberEnvelopeClass();
            IEnvelope pEnvelope = rb.TrackNew(view.ScreenDisplay, null) as IEnvelope;
            if (pEnvelope.IsEmpty)
            {
                IPoint queryPoint = ToMapPoint(x, y);
                IGeometry pgeoBuffer = (queryPoint as ITopologicalOperator).Buffer(5e-3 * m_Application.MapControl.MapScale);
                pEnvelope = pgeoBuffer.Envelope;
            }

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
                        rep = rpc.GetRepresentation(f, mctx);
                        if (rep == null || rel.Disjoint(rep.Shape))
                        {
                            continue;
                        }
                        map.SelectFeature(geoFlyr, f);
                        bFindRule=true;
                        break;
                    }
                    Marshal.ReleaseComObject(pCursor);
                }
                if (rep != null && bFindRule)
                {
                    break;
                }
            }
            view.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, view.Extent);
            #endregion
            //当前选择要素不为空，那么判断鼠标指针落脚点是否在某个定点附近
            if (rep != null && bFindRule)
            {
                RepresentationRuleEditorClass ruleEditor = new RepresentationRuleEditorClass();
                //根据RulueID取到原始的rule，对原始的rule的属性进行覆盖
                IRepresentationRule r = null;
                if (rep.RuleID == -1)
                {
                    r = FreeRuleValueSet(rep);
                }
                else
                {
                    r = RuleValueSet(rep);
                }

		        //弹出属性编辑对话框，单机确定后更新rep中的属性
                if (ruleEditor.DoModal(m_Application.MapControl.hWnd, rep.Shape.GeometryType, r))
                {
                    var newRule = ruleEditor.Rule;
                    if (rep.RuleID == -1)
                    {
                        ThematicAnnoSet(rep.Feature, r, newRule);
                        OverrideFreeValueSet(newRule, rep, rep.Feature.ShapeCopy);
                    }
                    else
                    {
                        OverrideValueSet(newRule, rep);
                    }
                   // OverrideValueSet(newRule, rep);
               }

                m_Application.EngineEditor.StartOperation();
                rep.UpdateFeature();
                rep.Feature.Store();
                m_Application.EngineEditor.StopOperation("规则编辑");
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rep);
            }
            view.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, view.Extent);
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

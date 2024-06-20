using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    //间隔线
   public  class DrawIntervalLineCmd:SMGITool
    {
        private IActiveView pAc;
        private double mapScale = 0;
        /// <summary>
        /// 编辑器
        /// </summary>
        IEngineEditor editor;
        /// <summary>
        /// 线型符号
        /// </summary>
        ISimpleLineSymbol lineSymbol;
        /// <summary>
        /// 线型反馈
        /// </summary>
        //INewLineFeedback lineFeedback;
        NewBezierCurveFeedback linebezeir;
        IRgbColor RGB = new RgbColorClass();
        private IFeatureLayer flayer = null;

        private double lineWidth = 15;
        private double lineIntervalWidth = 5;
        private IColor Color;

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override bool Deactivate()
        {
            m_Application.MapControl.OnAfterScreenDraw -= MapControl_OnAfterScreenDraw;
            return base.Deactivate();
        }

        public override void OnClick()
        {
            pAc = m_Application.ActiveView;
            mapScale = (m_Application.ActiveView as IMap).ReferenceScale;
            editor = m_Application.EngineEditor;
            lineSymbol = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass() { Red = 0, Green = 255, Blue = 0 };//Green
            lineSymbol.Color = color;
            lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            lineSymbol.Width = 1.5;
            (lineSymbol as ISymbol).ROP2 = esriRasterOpCode.esriROPNotXOrPen;//这个属性很重要，但不知道为啥重要
            /////用于解决在绘制feedback过程中进行地图平移出现线条混乱的问题
            m_Application.MapControl.OnAfterScreenDraw +=
                new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "moveline");
            })).ToArray();

            flayer = lyrs.First() as IFeatureLayer;
            ILayerEffects playereffects = flayer as ILayerEffects;
            playereffects.Transparency = 45;



            FrmIntervalLine frm = null;
            if (Color != null)
            {
                frm = new FrmIntervalLine(Color);
            }
            else
            {
                frm = new FrmIntervalLine();
            }
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            lineWidth = frm.lineWidth;
            lineIntervalWidth = frm.lineIntervalWidth;
            Color = frm.Color;
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            if (keyCode == 32)
            {
                FrmIntervalLine frm = null;
                if (Color != null)
                {
                    frm = new FrmIntervalLine(Color);
                }
                else
                {
                    frm = new FrmIntervalLine();
                }
                DialogResult dr = frm.ShowDialog();
                if (dr != DialogResult.OK)
                    return;
                lineWidth = frm.lineWidth;
                lineIntervalWidth = frm.lineIntervalWidth;
                Color = frm.Color;
            }
        }

        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (linebezeir != null)
            {
                linebezeir.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
            }
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            try
            {
                if (button != 1)
                {
                    return;
                }
                if (linebezeir == null)
                {
                    var dis = m_Application.ActiveView.ScreenDisplay;
                    linebezeir = new NewBezierCurveFeedbackClass { Display = dis, Symbol = lineSymbol as ISymbol };
                    linebezeir.Start(ToSnapedMapPoint(x, y));
                }
                else
                {
                    linebezeir.AddPoint(ToSnapedMapPoint(x, y));
                }
            }
            catch
            { }
        }
        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (linebezeir != null)
            {
                linebezeir.MoveTo(ToSnapedMapPoint(x, y));
            }
        }
        public override void OnDblClick()
        {
            IPolyline bezier = linebezeir.Stop();
            linebezeir = null;
            if (null == bezier || bezier.IsEmpty)
                return;
            DrawIntervalLine(bezier);
        }

        private void DrawIntervalLine(IPolyline orline)
        {
            try
            {
                double len = orline.Length;
                double val = lineIntervalWidth * mapScale * 1.0e-3;
                int count = Convert.ToInt32(Math.Ceiling(len / val));
                double mormallen = lineWidth * 0.5 * mapScale * 1.0e-3;
                for (int i = 1; i < count + 1; i = i + 2)
                {
                    ILine Normal = new LineClass();
                    orline.QueryNormal(esriSegmentExtension.esriExtendAtFrom, val * i, false, mormallen, Normal);
                    var pright1 = Normal.ToPoint;
                    var pcentre1 = Normal.FromPoint;
                    var pleft1 = new PointClass() { X = pcentre1.X * 2 - pright1.X, Y = pcentre1.Y * 2 - pright1.Y };

                    orline.QueryNormal(esriSegmentExtension.esriExtendAtFrom, val * (i + 1), false, mormallen, Normal);
                    var pright2 = Normal.ToPoint;
                    var pcentre2 = Normal.FromPoint;
                    var pleft2 = new PointClass() { X = pcentre2.X * 2 - pright2.X, Y = pcentre2.Y * 2 - pright2.Y };
                    CreateRectangle(pleft1, pleft2, pright1, pright2);
                }
                pAc.Refresh();
            }
            catch
            { }
        }
        public void CreateRectangle(IPoint left1, IPoint left2, IPoint right1, IPoint right2)
        {
            try
            {
                IGeometryCollection pClipRec = new PolygonClass();
                IPointCollection pCl = new RingClass();
                pCl.AddPoint(left1);
                pCl.AddPoint(left2);
                pCl.AddPoint(right2);
                pCl.AddPoint(right1);
                (pCl as IRing).Close();
                pClipRec.AddGeometry(pCl as IGeometry);
                (pClipRec as ITopologicalOperator).Simplify();
                //return pClipRec as IPolygon;
                IPolygon pgeo = pClipRec as IPolygon;
                //DrawPolygon(pClipRec as IPolygon,ColorFrom);
                insertPolygon(pgeo);
            }
            catch (Exception ex)
            {
                return;
            }

        }
        private void insertPolygon(IGeometry arrow)
        {
            IFeatureClass fcl = flayer.FeatureClass;
            IFeature fea = fcl.CreateFeature();
            fea.Shape = arrow;
            fea.set_Value(fcl.FindField("RuleID"), 1);
            fea.Store();
            //规则覆盖
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            IRepresentationRenderer repRender = (flayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass rpc = repRender.RepresentationClass;
            IRepresentation rep = rpc.GetRepresentation(fea, mc);

            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            try
            {
                for (int i = 0; i < ruleOrg.LayerCount; i++)
                {
                    IBasicSymbol ge = ruleOrg.get_Layer(i);
                    if (ge is IBasicFillSymbol)
                    {
                        IGraphicAttributes fillAttrs = (ge as IBasicFillSymbol).FillPattern as IGraphicAttributes;
                        for (int g = 0; g < fillAttrs.GraphicAttributeCount; g++)
                        {
                            int index = fillAttrs.get_ID(g);
                            string name = fillAttrs.get_Name(index);
                        }
                        int id = fillAttrs.get_IDByName("Color1");
                        rep.set_Value(fillAttrs, id, Color);
                        id = fillAttrs.get_IDByName("Color2");
                        rep.set_Value(fillAttrs, id, Color);
                    }
                    //else if (ge is IBasicLineSymbol)
                    //{
                    //    IRgbColor colorNull = new RgbColorClass();
                    //    colorNull.NullColor = true;
                    //    ILineStroke pLineStroke = new LineStrokeClass();
                    //    IGraphicAttributes lineAttrs = pLineStroke as IGraphicAttributes;
                    //    int id = lineAttrs.get_IDByName("Color");
                    //    rep.set_Value(lineAttrs, id, colorNull);
                    //}
                }
                rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
                rep.UpdateFeature();
                rep.Feature.Store();
            }
            catch
            { }
        }
   }
}

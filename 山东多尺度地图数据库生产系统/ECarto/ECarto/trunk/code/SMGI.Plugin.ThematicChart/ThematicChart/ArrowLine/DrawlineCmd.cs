using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SMGI.Common;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    //常规线
   public  class DrawlineCmd:SMGITool
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



            FrmLine frm = null;
            if (Color != null)
            {
                frm = new FrmLine(Color);
            }
            else
            {
                frm = new FrmLine();
            }
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            lineWidth = frm.lineWidth;
            Color = frm.ColorFrom;
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            if (keyCode == 32)
            {
                FrmLine frm = null;
                if (Color != null)
                {
                    frm = new FrmLine(Color);
                }
                else
                {
                    frm = new FrmLine();
                }
                DialogResult dr = frm.ShowDialog();
                if (dr != DialogResult.OK)
                    return;
                lineWidth = frm.lineWidth;
                Color = frm.ColorFrom;
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
            DrawLine(bezier);
        }
        private void DrawLine(IPolyline orline)
        {
            try
            {
                double len = orline.Length;
                IPointCollection pc = new RingClass();
                Dictionary<int, IPoint> ptright = new Dictionary<int, IPoint>();
                double mormallen = lineWidth * mapScale * 1.0e-3;
                ILine Normal = new LineClass();
                for (int i = 0; i < 51; i++)
                {
                    double raio = i * 0.02;
                    orline.QueryNormal(esriSegmentExtension.esriExtendAtFrom, raio, true, mormallen, Normal);
                    var pright = Normal.ToPoint;
                    var pcentre = Normal.FromPoint;
                    var pleft = new PointClass() { X = pcentre.X * 2 - pright.X, Y = pcentre.Y * 2 - pright.Y };
                    pc.AddPoint(pleft);
                    ptright.Add(50 - i, pright);
                }
                for (int i = 0; i < 51; i++)
                {
                    pc.AddPoint(ptright[i]);
                }
                (pc as IRing).Close();
                IGeometryCollection pclgeo = new PolygonClass();
                pclgeo.AddGeometry(pc as IGeometry);
                (pclgeo as ITopologicalOperator).Simplify();
                IPolygon pPolygon = pclgeo as IPolygon;
                pPolygon.Smooth(0);
                insertPolygon(pPolygon as IGeometry);
                pAc.Refresh();
            }
            catch
            { }
        }

        /// <summary>
        /// 将线塞进moveline图层中,并完成规则覆盖
        /// </summary>
        /// <param name="arrow"></param>
        private void insertPolygon(IGeometry geo)
        {
            IFeatureClass fcl = flayer.FeatureClass;
            IFeature fea = fcl.CreateFeature();
            fea.Shape = geo;
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

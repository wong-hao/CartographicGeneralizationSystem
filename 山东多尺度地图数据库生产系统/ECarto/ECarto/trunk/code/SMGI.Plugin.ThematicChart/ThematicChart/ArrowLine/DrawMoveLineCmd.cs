using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using stdole;

using ESRI.ArcGIS.Carto;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using System.Data;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    /// <summary>
    /// 箭头线
    /// </summary>
    public  class DrawMoveLineCmd:SMGITool
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

        private  double lineFromWidth = 2;
        private double lineToWidth = 6;
        private IColor ColorFrom;
        private IColor ColorBorder;
        private IColor ColorTo;
        private double angle = 0;

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
        Dictionary<string, int> ruleIDlpoly = new Dictionary<string, int>();
        private void InitRule()
        {
            if (ruleIDlpoly.Count > 0)
                return;
            string mdbpath = m_Application.Template.Root + @"\整饰\整饰规则库.mdb";
            string mdbname = "LLINE";//图廓压盖
            var ruleDt = Helper.ReadToDataTable(mdbpath, mdbname);
            DataRow[] drs = ruleDt.Select("图层='COMPASS'");
            drs = ruleDt.Select("图层='LPOLY'");
            foreach (DataRow dr in drs)
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDlpoly[keyname] = val;
            }
        }
        public override void OnClick()
        {
            InitRule();
            pAc = m_Application.ActiveView;
            mapScale = (m_Application.ActiveView as IMap).ReferenceScale;
            editor = m_Application.EngineEditor;
            lineSymbol = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass() { Red = 0, Green = 255, Blue = 0 };//Green
            lineSymbol.Color = color;
            lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            lineSymbol.Width = 1.5;
            (lineSymbol as ISymbol).ROP2 = esriRasterOpCode.esriROPNotXOrPen;//这个属性很重道为啥要，但不知重要
            /////用于解决在绘制feedback过程中进行地图平移出现线条混乱的问题
            m_Application.MapControl.OnAfterScreenDraw +=
                new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOLY");
            })).ToArray();
            if (lyrs.Length == 0)
            {
                MessageBox.Show("不存在图层");
                return;
            }
            flayer = lyrs.First() as IFeatureLayer;
            ILayerEffects playereffects = flayer as ILayerEffects;
            // playereffects.Transparency = 45;
            
            FrmMoveline frm = null;
            if (ColorFrom != null && ColorTo != null)
            {
                frm = new FrmMoveline(ColorFrom, ColorTo);
            }
            else
            {
                frm = new FrmMoveline();
            }
            frm.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            lineFromWidth = frm.lineFromWidth;
            lineToWidth = frm.lineToWidth;
            ColorFrom = frm.ColorFrom;
            ColorTo = frm.ColorTo;
            ColorBorder = frm.BorderColor;
            angle = frm.angle;
        }
        public override void OnKeyUp(int keyCode, int shift)
        {
            if (keyCode == 32)
            {
                FrmMoveline frm = null;
                if (ColorFrom != null && ColorTo != null)
                {
                    frm = new FrmMoveline(ColorFrom, ColorTo);
                }
                else
                {
                    frm = new FrmMoveline();
                }
                DialogResult dr = frm.ShowDialog();
                if (dr != DialogResult.OK)
                    return;
                lineFromWidth = frm.lineFromWidth;
                lineToWidth = frm.lineToWidth;
                angle = frm.angle;
                ColorBorder = frm.BorderColor;
                ColorFrom = frm.ColorFrom;
                ColorTo = frm.ColorTo;
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
            //try
            //{
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
            //}
            //catch
            //{ }
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
            if (linebezeir == null)
                return;
            IPolyline bezier = linebezeir.Stop();
            linebezeir = null;
            if (null == bezier || bezier.IsEmpty)
                return;
            DrawSwallowArrow(bezier);
        }

        #region  运动线绘制
        /// <summary>
        /// 将线塞进LPolygon图层中,并完成规则覆盖
        /// </summary>
        /// <param name="arrow"></param>
        private void insertPolygon(IGeometry arrow)
        {
            IFeatureClass fcl = flayer.FeatureClass;
            IFeature fea = fcl.CreateFeature();
            fea.Shape = arrow;
            fea.set_Value(fcl.FindField("RuleID"), ruleIDlpoly["图廓"]);
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
                        int id = fillAttrs.get_IDByName("Color");
                        rep.set_Value(fillAttrs, id, ColorFrom);
                    }
                    if (ge is IBasicLineSymbol)
                    {

                        IGraphicAttributes fillAttrs = (ge as IBasicLineSymbol).Stroke as IGraphicAttributes;
                        for (int g = 0; g < fillAttrs.GraphicAttributeCount; g++)
                        {
                            int index = fillAttrs.get_ID(g);
                            string name = fillAttrs.get_Name(index);
                        }
                        int id = fillAttrs.get_IDByName("Color");
                        rep.set_Value(fillAttrs, id, ColorBorder);
                    }

                }
                rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
                rep.UpdateFeature();
                rep.Feature.Store();
            }
            catch
            { 
            }
        }
        /// <summary>
        /// 燕尾型箭头绘制
        /// </summary>
        /// <param name="orline"></param>
        private void DrawSwallowArrow(IPolyline orline)
        {
            Dictionary<int, IPoint> ptLeft = new Dictionary<int, IPoint>();//箭头左边点集合
            Dictionary<int, IPoint> ptRight = new Dictionary<int, IPoint>();//箭头右边点集合
            IPolyline line = breakline(orline);//将线等分

            ISegmentCollection sgcol = line as ISegmentCollection;
            ILine Normalline = new LineClass();//法线
            double max = lineToWidth * mapScale * 1.0e-3;
            double min = lineFromWidth * mapScale * 1.0e-3;
            line.QueryNormal(esriSegmentExtension.esriExtendAtFrom, 0.03, true, 10, Normalline);//获取箭尾点
            ptLeft.Add(-1, Normalline.FromPoint);
            
            double len = 0;
            int coun = 0;
            try
            {
                for (int i = 0; i < sgcol.SegmentCount - 2; i++)
                {
                    coun++;
                    IPoint pright = new PointClass();
                    IPoint pcenter = new PointClass();
                    IPoint pleft = new PointClass();
                    ISegment sg = sgcol.get_Segment(i);
                    if (i == 0)
                    {
                        line.QueryNormal(esriSegmentExtension.esriExtendAtFrom, 0, true, max, Normalline);
                        pright = Normalline.ToPoint;
                        pcenter = Normalline.FromPoint;
                        pleft = new PointClass() { X = 2 * pcenter.X - pright.X, Y = 2 * pcenter.Y - pright.Y };
                        ptLeft.Add(i, pleft);
                        ptRight.Add(i, pright);
                    }
                    len += sg.Length;
                    double per = max - (max - min) * len / line.Length;
                    if (i == sgcol.SegmentCount - 3)
                    {
                        line.QueryNormal(esriSegmentExtension.esriExtendAtFrom, 0.96, true, per, Normalline);
                        pright = Normalline.ToPoint;
                        pcenter = Normalline.FromPoint;
                        pleft = new PointClass() { X = 2 * pcenter.X - pright.X, Y = 2 * pcenter.Y - pright.Y };
                        ptLeft.Add(i + 1, pleft);
                        ptRight.Add(i + 1, pright);
                        //箭头部分两边的点
                        line.QueryNormal(esriSegmentExtension.esriExtendAtFrom, 0.95, true, per * 2, Normalline);
                        pright = Normalline.ToPoint;
                        pcenter = Normalline.FromPoint;
                        pleft = new PointClass() { X = 2 * pcenter.X - pright.X, Y = 2 * pcenter.Y - pright.Y };
                        ptLeft.Add(-3, pleft);
                        ptLeft.Add(-4, pright);
                        //箭头顶点
                        line.QueryNormal(esriSegmentExtension.esriExtendAtFrom, 1, true, 10 * mapScale * 1.0e-3, Normalline);
                        ptLeft.Add(-2, Normalline.FromPoint);
                        break;
                    }
                    else
                    {

                        line.QueryNormal(esriSegmentExtension.esriExtendAtFrom, len, false, per, Normalline);
                        pright = Normalline.ToPoint;
                        pcenter = Normalline.FromPoint;
                        pleft = new PointClass() { X = 2 * pcenter.X - pright.X, Y = 2 * pcenter.Y - pright.Y };
                        ptLeft.Add(i + 1, pleft);
                        ptRight.Add(i + 1, pright);
                    }
                    line.QueryTangent(esriSegmentExtension.esriExtendAtFrom, len, true, max, Normalline);
                    angle += (180 * Normalline.Angle) / Math.PI;
                }
                angle = -(angle / coun);
                IPointCollection pc = new RingClass();
                pc.AddPoint(ptLeft[-1]);
                for (int i = 0; i < ptLeft.Count - 4; i++)
                {
                    pc.AddPoint(ptLeft[i]);
                }
                pc.AddPoint(ptLeft[-3]);
                pc.AddPoint(ptLeft[-2]);
                pc.AddPoint(ptLeft[-4]);
                for (int i = ptRight.Count - 1; i >= 0; i--)
                {
                    pc.AddPoint(ptRight[i]);
                }
                IGeometryCollection pcgeo = new PolygonClass();
                (pc as IRing).Close();
                pcgeo.AddGeometry(pc as IGeometry);
                (pcgeo as ITopologicalOperator).Simplify();

                IRgbColor rgb = new RgbColorClass() { Red = 0, Green = 255, Blue = 0 };
                IPolygon pPolygon = pcgeo as IPolygon;
                insertPolygon(pPolygon as IGeometry);
                pAc.Refresh();
            }
            catch
            { }
        }
        /// <summary>
        /// 按比例打断中轴线
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private IPolyline breakline(IPolyline line)
        {
            bool val1=false;
            int val2=0;
            int val3=0;
            for (int i = 2; i < 101; i=i+2)
            {
                double t = i * 0.01;
             line.SplitAtDistance(t, true, true, out val1,out val2,out val3);
            }
            return line;
        }
        /// <summary>
        /// 测试用函数
        /// </summary>
        /// <param name="pgeo"></param>
        /// <param name="pcolor"></param>
        /// <returns></returns>
        private IElement DrawPolygon(IGeometry pgeo, IColor pcolor)
        {
            try
            {
                IElement pEl = null;
                IFillShapeElement polygonElement = new PolygonElementClass();
                ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
                smsymbol.Style = esriSimpleFillStyle.esriSFSSolid;
                smsymbol.Color = pcolor;
                ISimpleLineSymbol smline = new SimpleLineSymbolClass();
                smline.Style = esriSimpleLineStyle.esriSLSNull;
                smsymbol.Outline = smline;

                polygonElement.Symbol = smsymbol;

                pEl = polygonElement as IElement;
                pEl.Geometry = pgeo as IGeometry;

                (pAc as IGraphicsContainer).AddElement(pEl, 0); 

                return pEl;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}

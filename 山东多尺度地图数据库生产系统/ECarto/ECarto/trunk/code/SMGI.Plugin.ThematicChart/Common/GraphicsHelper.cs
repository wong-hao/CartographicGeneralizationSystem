using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using stdole;
using SMGI.Common;
namespace SMGI.Plugin.ThematicChart
{
    /// <summary>
    /// 绘制图形元素帮助类
    /// </summary>
    public  class GraphicsHelper
    {
       IActiveView pAc;
       IColor ColorNULL = null;
       public  GraphicsHelper(IActiveView _ac)
       {
           pAc = _ac;
       }
       public  GraphicsHelper()
       {
          
       }
       //获取空颜色测试
       private IColor ObtainNULLColor()
       {
           ICmykColor pnullcolor = new CmykColorClass();
           pnullcolor.CMYK = 6;
           pnullcolor.Black = 6;
           pnullcolor.Cyan = 0;
           pnullcolor.Magenta = 0;
           pnullcolor.NullColor = true;
           pnullcolor.RGB = 15790320;
           pnullcolor.Transparency = 0;
           pnullcolor.UseWindowsDithering = true;
           pnullcolor.Yellow = 0;
           return pnullcolor;
 

       }
       /// <summary>
       /// 根据起始点创建多义线
       /// </summary>
       /// <param name="f"></param>
       /// <param name="t"></param>
       /// <returns></returns>
       public IPolyline ContructPolyLine(IPoint f, IPoint t)
       {
           try
           {
               IGeometryCollection pPolyline = new PolylineClass();
               IPointCollection pCl = new PathClass();
               pCl.AddPoint(f);
               pCl.AddPoint(t);
               pPolyline.AddGeometry(pCl as IGeometry);
               (pPolyline as ITopologicalOperator).Simplify();
               return pPolyline as IPolyline;
           }
           catch (Exception ex)
           {
               return null;
           }

       }
        /// <summary>
       /// 构造立方体顶面菱形
        /// </summary>
        /// <returns></returns>
        public IPolygon ConstructTopDiamond(IPoint leftdown, double width, double height, double angel = Math.PI/4)
        {
            try
            {
                IGeometryCollection pClipRec = new PolygonClass();
                IPointCollection pCl = new RingClass();
                double cx = leftdown.X;
                double cy = leftdown.Y;
                pCl.AddPoint(leftdown);
                pCl.AddPoint(new PointClass() { X = cx + width, Y = cy });
                pCl.AddPoint(new PointClass() { X = cx + width + height / Math.Tan(angel), Y = cy + height });
                pCl.AddPoint(new PointClass() { X = cx + height / Math.Tan(angel), Y = cy + height });
                (pCl as IRing).Close();
                pClipRec.AddGeometry(pCl as IGeometry);
                (pClipRec as ITopologicalOperator).Simplify();
                return pClipRec as IPolygon;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 构造立方体右侧面菱形
        /// </summary>
        /// <param name="leftdown"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angel"></param>
        /// <returns></returns>
        public IPolygon ConstructSideDiamond(IPoint leftdown, double width, double height, double angel = Math.PI/4)
        {
            try
            {
                IGeometryCollection pClipRec = new PolygonClass();
                IPointCollection pCl = new RingClass();
                double cx = leftdown.X;
                double cy = leftdown.Y;
                pCl.AddPoint(leftdown);
                pCl.AddPoint(new PointClass() { X = cx + width / Math.Tan(angel), Y = cy + width });
                pCl.AddPoint(new PointClass() { X = cx + width / Math.Tan(angel), Y = cy + height + width });
                pCl.AddPoint(new PointClass() { X = cx, Y = cy + height });
                (pCl as IRing).Close();
                pClipRec.AddGeometry(pCl as IGeometry);
                (pClipRec as ITopologicalOperator).Simplify();
                return pClipRec as IPolygon;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 构造椭圆
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="a">长半轴</param>
        /// <param name="b">短半轴</param>
        /// <returns></returns>
        public IPolygon ConsturctEllipse(IPoint center, double a, double b)
        {
            try
            {
                IPolygon pEllipse = null;
                ITopologicalOperator pTo = null;

                IEnvelope pEnvelope;
                IConstructEllipticArc pCoElliArc = new EllipticArcClass();

                IEllipticArc pArc;
                ISegmentCollection pSegCol = new PolygonClass();

                pEnvelope = new EnvelopeClass();
                double xmax = center.X + a;
                double xmin = center.X - a;
                double ymax = center.Y + b;
                double ymin = center.Y - b;
                pEnvelope.PutCoords(xmin, ymin, xmax, ymax);


                pCoElliArc.ConstructEnvelope(pEnvelope);
                pArc = pCoElliArc as IEllipticArc;
                ISegment psegment = pArc as ISegment;
                pSegCol.AddSegment(psegment);
                pEllipse = pSegCol as IPolygon;
                pTo = pEllipse as ITopologicalOperator;
                pTo.Simplify();
                return pEllipse;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 构造矩形
        /// </summary>
        /// <param name="upleftpoint">左上点</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <returns></returns>
        public IPolygon CreateRectangle(IPoint upleftpoint, double width, double height)
        {
            try
            {
                IGeometryCollection pClipRec = new PolygonClass();
                IPointCollection pCl = new RingClass();
                double cx = upleftpoint.X;
                double cy = upleftpoint.Y;
                pCl.AddPoint(upleftpoint);
                pCl.AddPoint(new PointClass() { X = cx + width, Y = cy });
                pCl.AddPoint(new PointClass() { X = cx + width, Y = cy - height });
                pCl.AddPoint(new PointClass() { X = cx, Y = cy - height });
                (pCl as IRing).Close();
                pClipRec.AddGeometry(pCl as IGeometry);
                (pClipRec as ITopologicalOperator).Simplify();
                return pClipRec as IPolygon;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //绘图相关
        #region
        public IElement DrawTxt(IPoint point, string txt, double fontsize, IFontDisp pFont)
        {

            ITextSymbol pTextSymbol = new TextSymbolClass()
            {

                Font = pFont,
                Size = fontsize
            };

            try
            {
                IElement pEl = null;
                ITextElement ptxt = new TextElementClass();
                ptxt.Text = txt.Trim();
                ptxt.ScaleText = true;
                ptxt.Symbol = pTextSymbol;
                pEl = ptxt as IElement;
                pEl.Geometry = point;

                return pEl;
            }
            catch
            {
                return null;
            }
        }
        public IElement DrawTxt(IPoint point, string txt, double fontsize)
        {

            IFontDisp pFont = new StdFont()
            {
                Name = "黑体",
                Size = 16
            } as IFontDisp;
            ITextSymbol pTextSymbol = new TextSymbolClass()
            {

                Font = pFont,
                Size = fontsize
            };

            try
            {
                IElement pEl = null;
                ITextElement ptxt = new TextElementClass();
                ptxt.Text = txt.Trim();
                ptxt.ScaleText = true;
                ptxt.Symbol = pTextSymbol;
                pEl = ptxt as IElement;
                pEl.Geometry = point;
            
                return pEl;
            }
            catch
            {
                return null;
            }
        }
        public IElement DrawColorPolygon(IGeometry pgeo, int ct)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {
                IFillShapeElement polygonElement = new PolygonElementClass();
                polygonElement.Symbol = GetSymbol(ct);

                pEl = polygonElement as IElement;
                pEl.Geometry = pgeo as IGeometry;
            
                return pEl;
            }
            catch
            {
                return pEl;
            }
        }
        private ISimpleFillSymbol GetFillSymbol()
        {
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Style = esriSimpleFillStyle.esriSFSNull;
            ILineSymbol linesym = new SimpleLineSymbolClass();
            linesym.Width = 0.01;
            smsymbol.Outline = linesym;
            return smsymbol;
        }
        private ISimpleFillSymbol GetSymbol(int type)
        {
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            ICmykColor pcolor = new CmykColorClass();
            pcolor.Cyan = 15;
            pcolor.Magenta = 10;
            pcolor.Yellow = 10;
            pcolor.Black = 10;
            if (type == 0)
            {
                pcolor.Cyan = 5;
                pcolor.Magenta = 10;
                pcolor.Yellow = 0;
                pcolor.Black = 0;
            }
            else if (type == 1)
            {
                pcolor.Cyan = 15;
                pcolor.Magenta = 0;
                pcolor.Yellow = 0;
                pcolor.Black = 0;
            }
            else if (type == 2)
            {
                pcolor.Cyan = 10;
                pcolor.Magenta = 0;
                pcolor.Yellow = 20;
                pcolor.Black = 0;
            }
            else if (type == 3)
            {
                pcolor.Cyan = 15;
                pcolor.Magenta = 20;
                pcolor.Yellow = 0;
                pcolor.Black = 0;
            }
            ILineSymbol linesym = new SimpleLineSymbolClass();
            linesym.Width = 0.05;
            smsymbol.Outline = linesym;
            smsymbol.Color = pcolor;
            return smsymbol;
        }
        public  IElement  DrawPolygon(IGeometry pgeo)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            polygonElement.Symbol = GetFillSymbol();
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
           
            
            return pEl;
        }
        public IElement DrawPolygon(IGeometry pgeo, IColor prgb, double linesize)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            ILineSymbol linesym = new SimpleLineSymbolClass();
            linesym.Width = linesize;
            smsymbol.Outline = linesym;
            smsymbol.Color = prgb;
            polygonElement.Symbol = smsymbol;
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;

            
            return pEl;
        }
        public IElement DrawPolygonBg(IGeometry pgeo, IColor prgb, double linesize)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            ISimpleLineSymbol linesym = new SimpleLineSymbolClass();
            linesym.Width = linesize;
            smsymbol.Outline = linesym;
            smsymbol.Color = prgb;
            linesym.Style = esriSimpleLineStyle.esriSLSNull;
            smsymbol.Style = esriSimpleFillStyle.esriSFSNull;
            polygonElement.Symbol = smsymbol;
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;


            return pEl;
        }
        public IElement DrawLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 2;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = 122;
                rgb.Blue = 122;
                rgb.Green = 122;
                linesym.Color = rgb;
                polygonElement.Symbol = linesym;
                pEl = polygonElement as IElement;
                pEl.Geometry = pline as IGeometry;
               
            }
            catch
            {

            }
            return pEl;
        }
        #endregion
        /// <summary>
        /// 创建渐变色 
        /// </summary>
        /// <returns></returns>
        public IGradientFillSymbol CreateGradientSym(IColor from, IColor to, int interval=100)
        {
            IAlgorithmicColorRamp pColorRamp = new AlgorithmicColorRampClass();
            pColorRamp.FromColor = from;
            pColorRamp.ToColor = to;
            pColorRamp.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;
            IGradientFillSymbol psym = new GradientFillSymbolClass();
            psym.ColorRamp = pColorRamp;
            psym.GradientAngle = 0;
            psym.GradientPercentage = 0.9;
            psym.IntervalCount = interval;
            psym.Style = esriGradientFillStyle.esriGFSLinear;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();
            smline.Style = esriSimpleLineStyle.esriSLSNull;
            psym.Outline = smline;
            return psym;
        }
        public IGradientFillSymbol CreateGradientSym(IColor from, IColor to,esriGradientFillStyle style, int interval = 100)
        {
            IAlgorithmicColorRamp pColorRamp = new AlgorithmicColorRampClass();
            pColorRamp.FromColor = from;
            pColorRamp.ToColor = to;
            pColorRamp.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;
            IGradientFillSymbol psym = new GradientFillSymbolClass();
            psym.ColorRamp = pColorRamp;
            psym.GradientAngle = 0;
            psym.GradientPercentage = 0.9;
            psym.IntervalCount = interval;
            psym.Style = style;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();
            smline.Style = esriSimpleLineStyle.esriSLSNull;
            psym.Outline = smline;
            return psym;
        }
        
        #region 将element写入制图表达中
        //将element写自由表达
        public void CreateFeatures( List<IElement> eles, ILayer player, IPoint pBasePoint,double size)
        {
            size = size * 2.83465;
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = pBasePoint;
            fe.set_Value(pointfcl.FindField("TYPE"),"专题图");
            fe.Store();
            //空颜色
            ColorNULL =  ObtainNULLColor();
            //获取当前要素的制图表达
            IRepresentationGraphics g = new RepresentationMarkerClass();
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            foreach (var e in eles)
            {
                FromElement1(e, g, pBasePoint);

            }
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, g); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false); 
            r.InsertLayer(0, bs);
            //自由
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            if (m_RepClass == null)
                return;
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint, r);
            rep.Graphics = repGraphics;
            
            rep.UpdateFeature();
            rep.Feature.Store();
        }

        /// <summary>
        /// 柱状图专用
        /// </summary>
        public IRepresentationMarker CreateFeaturesEx(List<IElement> eles, ILayer player, IPoint pBasePoint,out int fid, double size=20)
        {
            size = size * 2.83465;//将毫米转化为pt，1mm=2.83465pt
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = pBasePoint;
            fe.set_Value(pointfcl.FindField("TYPE"), "专题图");
            fe.Store();
            fid = fe.OID;
            //空颜色
            ColorNULL = ObtainNULLColor();
            //获取当前要素的制图表达
            IRepresentationGraphics g = new RepresentationMarkerClass();
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            foreach (var e in eles)
            {
                FromElement(e, g, pBasePoint);

            }
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, g); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint, r);
            rep.Graphics = repGraphics;
            
            rep.UpdateFeature();
            rep.Feature.Store();
            return g as IRepresentationMarker;
        }
        public IRepresentationMarker CreateFeaturesBgEx(List<IElement> eles, ILayer player, IPoint pBasePoint, out int fid, double size = 20)
        {
            size = size * 2.83465;//将毫米转化为pt，1mm=2.83465pt
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = pBasePoint;
            fe.set_Value(pointfcl.FindField("TYPE"), "专题图");
            fe.Store();
            fid = fe.OID;
            //空颜色
            ColorNULL = ObtainNULLColor();
            //获取当前要素的制图表达
            IRepresentationGraphics g = new RepresentationMarkerClass();
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            foreach (var el in eles)
            {
       
                IGeometry geo1 = el.Geometry;
                mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
                var geomap = mc.FromGeographyToMap(geo1);
                var geocenter = mc.FromGeographyToMap(new PointClass { X = 0, Y = 0 }) as ESRI.ArcGIS.Geometry.IPoint;
                IGeometry geo = (geomap as IClone).Clone() as IGeometry;
                ITransform2D ptrans2d = geo as ITransform2D;
                ptrans2d.Move(-geocenter.X, -geocenter.Y);
              
                IFillSymbol symbol = (el as IFillShapeElement).Symbol;
                var rule = new RepresentationRuleClass();
                rule.InitWithSymbol(symbol as ISymbol);
                CheckSymbolNUll(symbol as IFillSymbol, rule as IRepresentationRule);

                g.Add(geo, rule);
            }
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, g); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint, r);
            rep.Graphics = repGraphics;

            rep.UpdateFeature();
            rep.Feature.Store();
            return g as IRepresentationMarker;
        }
      
        public IRepresentationMarker CreateFeaturesEx(List<IElement> eles, ILayer player, IPoint pBasePoint, out int fid,string json, double size = 20)
        {
            size = size * 2.83465;//将毫米转化为pt，1mm=2.83465pt
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = pBasePoint;
            fe.set_Value(pointfcl.FindField("TYPE"), "专题图");
            fe.set_Value(pointfcl.FindField("JsonTxt"), json);
            fe.Store();
            fid = fe.OID;
            //空颜色
            ColorNULL = ObtainNULLColor();
            //获取当前要素的制图表达
            IRepresentationGraphics g = new RepresentationMarkerClass();
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            foreach (var e in eles)
            {
                FromElement(e, g, pBasePoint);

            }
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, g); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint, r);
            rep.Graphics = repGraphics;

            rep.UpdateFeature();
            rep.Feature.Store();
            return g as IRepresentationMarker;
        }
      
        void FromElement(IElement el, IRepresentationGraphics g, IPoint pBasePoint)
        {
            
            if (el == null || g == null)
                return;

            IGeometry geo1 = el.Geometry;
            IMapContext mc = new MapContextClass();

            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            var geomap = mc.FromGeographyToMap(geo1);
            //var geocenter = mc.FromGeographyToMap(pBasePoint) as ESRI.ArcGIS.Geometry.IPoint;
            IGeometry geo = (geomap as IClone).Clone() as IGeometry;
            //ITransform2D ptrans2d = geo as ITransform2D;
            //ptrans2d.Move(-geocenter.X, -geocenter.Y);
            if (el is IGroupElement)
            {
                var gl = el as IGroupElement;
                for (int i = 0; i < gl.ElementCount; i++)
                {
                    IElement ell = gl.Element[i];
                    FromElement(ell, g, pBasePoint);
                }
            }
            else if (el is IFillShapeElement)
            {
                IFillSymbol symbol = (el as IFillShapeElement).Symbol;
                var rule = new RepresentationRuleClass();
                rule.InitWithSymbol(symbol as ISymbol);
                CheckSymbolNUll(symbol as IFillSymbol, rule as IRepresentationRule);
               
                g.Add(geo, rule);
            }
            else if (el is ILineElement)
            {
                var symbol = (el as ILineElement).Symbol;

                var rule = new RepresentationRuleClass();
                rule.InitWithSymbol(symbol as ISymbol);
          
                g.Add(geo, rule);
            }
            else if (el is IMarkerElement)
            {
                var symbol = (el as IMarkerElement).Symbol;

                var rule = new RepresentationRuleClass();
                rule.InitWithSymbol(symbol as ISymbol);
                g.Add(geo, rule);
            }
            else if (el is ITextElement)
            {
                var te = el as ITextElement;
                var text = te.Text;
                var symbol = te.Symbol;
                var xx = symbol as ISimpleTextSymbol;

                if (!(geo1 is ESRI.ArcGIS.Geometry.IPoint) || text == null || text.Length < 0)
                {
                    return;
                }
                var texts = text.Split('\n', '\r');
                AddTxtElement(pBasePoint, geo1 as ESRI.ArcGIS.Geometry.IPoint, text, te.Symbol, g);
            }
        }
        void FromElement1(IElement el, IRepresentationGraphics g, IPoint pBasePoint)
        {

            if (el == null || g == null)
                return;

            IGeometry geo1 = el.Geometry;
            IMapContext mc = new MapContextClass();

            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            var geomap = mc.FromGeographyToMap(geo1);
            var geocenter = mc.FromGeographyToMap(pBasePoint) as ESRI.ArcGIS.Geometry.IPoint;
            IGeometry geo = (geomap as IClone).Clone() as IGeometry;
            ITransform2D ptrans2d = geo as ITransform2D;
            ptrans2d.Move(-geocenter.X, -geocenter.Y);
            if (el is IGroupElement)
            {
                var gl = el as IGroupElement;
                for (int i = 0; i < gl.ElementCount; i++)
                {
                    IElement ell = gl.Element[i];
                    FromElement(ell, g, pBasePoint);
                }
            }
            else if (el is IFillShapeElement)
            {
                IFillSymbol symbol = (el as IFillShapeElement).Symbol;
                var rule = new RepresentationRuleClass();
                rule.InitWithSymbol(symbol as ISymbol);
                CheckSymbolNUll(symbol as IFillSymbol, rule as IRepresentationRule);

                g.Add(geo, rule);
            }
            else if (el is ILineElement)
            {
                var symbol = (el as ILineElement).Symbol;

                var rule = new RepresentationRuleClass();
                rule.InitWithSymbol(symbol as ISymbol);

                g.Add(geo, rule);
            }
            else if (el is IMarkerElement)
            {
                var symbol = (el as IMarkerElement).Symbol;

                var rule = new RepresentationRuleClass();
                rule.InitWithSymbol(symbol as ISymbol);
                g.Add(geo, rule);
            }
            else if (el is ITextElement)
            {
                var te = el as ITextElement;
                var text = te.Text;
                var symbol = te.Symbol;
                var xx = symbol as ISimpleTextSymbol;

                if (!(geo1 is ESRI.ArcGIS.Geometry.IPoint) || text == null || text.Length < 0)
                {
                    return;
                }
                var texts = text.Split('\n', '\r');
                AddTxtElement(pBasePoint, geo1 as ESRI.ArcGIS.Geometry.IPoint, text, te.Symbol, g);
            }
        }
      
        private bool CheckSymbolNUll(IFillSymbol symbol, IRepresentationRule rule)
        {
            bool flag = false;
            IFillSymbol pfillsym = symbol as IFillSymbol;
            ISimpleLineSymbol psls = pfillsym.Outline as ISimpleLineSymbol;
            if (psls != null)
            {
                if (psls.Style == esriSimpleLineStyle.esriSLSNull)
                {
                   
                    int index = 0;
                    for (int i = 0; i < rule.LayerCount; i++)
                    {
                        IBasicSymbol pbasic = rule.get_Layer(i);
                        if (pbasic is IBasicLineSymbol)
                        {                           
                            break;
                        }
                        index++;
                    }
                    rule.RemoveLayer(index);
                    flag = true;
                }
            }
            ISimpleFillSymbol psfs = symbol as ISimpleFillSymbol;
            if (psfs != null)
            {
                if (psfs.Style == esriSimpleFillStyle.esriSFSNull)
                {
                     
                    int index = 0;
                    for (int i = 0; i < rule.LayerCount; i++)
                    {
                        IBasicSymbol pbasic = rule.get_Layer(i);
                        if (pbasic is IBasicFillSymbol)
                        {

                            IBasicFillSymbol pfsym = pbasic as IBasicFillSymbol;
                            IFillPattern fillpattern = pfsym.FillPattern;
                            IGraphicAttributes fillAttrs = fillpattern as IGraphicAttributes;
                            fillAttrs.set_Value(0, ColorNULL);
                           // IColor color = fillAttrs.get_Value(0) as IColor;
                           // break;
                        }
                        index++;
                    }
                   // rule.RemoveLayer(index);
                }
            }

            return flag;
        }
        private void TransMapPoint(IPoint pBasePoint, char c, IPoint txtpoint, ITextSymbol symbol, IRepresentationGraphics g)
        {
            IMapContext mc = new MapContextClass();

            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            var geomap = mc.FromGeographyToMap(txtpoint);
            var geocenter = mc.FromGeographyToMap(pBasePoint) as ESRI.ArcGIS.Geometry.IPoint;
            IGeometry geo = (geomap as IClone).Clone() as IGeometry;
            ITransform2D ptrans2d = geo as ITransform2D;
            ptrans2d.Move(-geocenter.X, -geocenter.Y);
            var xx = symbol as ISimpleTextSymbol;
            var cms = new CharacterMarkerSymbolClass();

            cms.CharacterIndex = (int)c;
            cms.Color = xx.Color;
            cms.Size = xx.Size;
            cms.Font = xx.Font;
            cms.Angle = xx.Angle;


            var rule = new RepresentationRuleClass();
            rule.InitWithSymbol(cms as ISymbol);
            var pt = ptrans2d as ESRI.ArcGIS.Geometry.IPoint;
            //获取每个文字的坐标
            g.Add(pt, rule);

        }
        private void AddTxtElement(IPoint pBasePoint, IPoint txtpoint, string txt, ITextSymbol symbol, IRepresentationGraphics g_)
        {
            double fsize = (symbol as ISimpleTextSymbol).Size;
            double dx = 0;
            var splits = txt.Split('\n', '\r');
            List<string> lists = new List<string>();
            foreach (string str in splits)
            {
                if (str.Trim() != "")
                {
                    lists.Add(str);
                }
            }
            double mapScale = GApplication.Application.MapControl.ReferenceScale;
            double txtunit = 3.52778;//1号字体和宽度
            txtunit = mapScale * txtunit * 1e-4;
            double heightunit = 3.97427;//1号字体1万的高度
            heightunit = mapScale * heightunit * 1e-4;
            string[] texts = lists.ToArray();
            int row = texts.Length - 1;//行
            foreach (var t in texts)
            {
                dx = txtpoint.X;
                int col = 0;
                double dis = 0;
                double tedis = 0;
                string tt = (string)t;
                double ttnum = GetStrLen(tt);
                dis = ttnum / 2 * txtunit * fsize / 2;//当前文字一半长度
                dx = dx - dis;
                for (int i = 0; i < t.Length; i++, col++)
                {
                    string temp = t[i].ToString();
                    double tempnum = GetStrLen(temp);
                    //当前的位置
                    double x = dx + tedis + tempnum * txtunit * fsize / 2 / 2;
                    double y = txtpoint.Y + row * heightunit * fsize + 0.65*0.5 * heightunit * fsize;
                    ESRI.ArcGIS.Geometry.IPoint p = new ESRI.ArcGIS.Geometry.PointClass() { X = x, Y = y };
                    //将当前点添加到制图表达中
                    TransMapPoint(pBasePoint,t[i], p, symbol, g_);
                    tedis += tempnum * txtunit * fsize / 2;
                }
                row--;
            }
        }
        //获取文字总长度
        public double GetStrLen(string str)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if ((int)str[i] > 127)
                    count++;
            }
            return str.Length - count + count * 2;
        }
        /// <summary>
        /// 字符的实际距离
        /// </summary>
        /// <param name="str"></param>
        /// <param name="mapScale"></param>
        /// <param name="fontsize"></param>
        /// <returns></returns>
        public double GetStrWidth(string str, double mapScale, double fontsize)
        {

            double txtunit = 3.97427;//1号字体和宽度
            double strwidth = GetStrLen(str)/2 * txtunit * (mapScale / 10000) * fontsize;
            return strwidth;
        }
        public double GetStrWidthmax(string[] str, double mapScale, double fontsize)
        {

            double txtunit = 1.52778;//1号字体和宽度
            double strwidthmax = 0;
            for (int i = 0; i < str.Length; i++)
            {
                double strwidth = GetStrLen(str[i]) / 2 * txtunit * (mapScale / 10000) * fontsize;
                if (strwidth >= strwidthmax)
                {
                    strwidthmax = strwidth;
                }
            }
            return strwidthmax;
        }

        #endregion
    }
}

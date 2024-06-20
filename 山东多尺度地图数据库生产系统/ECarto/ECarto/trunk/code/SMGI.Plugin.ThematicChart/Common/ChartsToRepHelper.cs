using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
    /// 将专题图写入自由式制图表达
    /// </summary>
    public  class ChartsToRepHelper
    {
       
        /// <summary>
        /// 更新
        /// </summary> 
        public static void CreateFeature(IActiveView pAc, List<IElement> eles, ILayer player, IPoint pBasePoint,string json, double size = 20)
        {
            IColor ColorNULL = ObtainNULLColor();
            size = size * 2.83465;
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = pBasePoint;
            fe.set_Value(pointfcl.FindField("TYPE"), "专题图");
            fe.set_Value(pointfcl.FindField("JsonTxt"), json);
            fe.Store();

            //添加规则
            #region
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();

            AddRepGraphics(eles, pGraphics, ColorNULL);

            #endregion
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint, r);
            rep.Graphics = repGraphics;

            rep.UpdateFeature();
            rep.Feature.Store();

        }
        
        /// <summary>
        /// 创建
        /// </summary> 
        public static IRepresentationMarker CreateFeature(IActiveView pAc, List<IElement> eles, ILayer player, IPoint pBasePoint, string json, out int objID, double size = 20)
        {
            IColor ColorNULL = ObtainNULLColor();
            size = size * 2.83465;
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = pBasePoint;
            fe.set_Value(pointfcl.FindField("TYPE"), "专题图");
            if(pointfcl.FindField("JsonTxt")!=-1)
            fe.set_Value(pointfcl.FindField("JsonTxt"), json);
            fe.Store();
            objID = fe.OID;
            //添加规则
            #region
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();

            AddRepGraphics(eles, pGraphics, ColorNULL);
            #endregion
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint, r);
            rep.Graphics = repGraphics;

            rep.UpdateFeature();
            rep.Feature.Store();
            return pGraphics as IRepresentationMarker;
        }

        /// <summary>
        /// 创建有角度的制图表达
        /// </summary>
        public static IRepresentationMarker CreateFeature(double angle, IActiveView pAc, List<IElement> eles, ILayer player, IPoint pBasePoint, string json, out int objID, double size = 20)
        {
            IColor ColorNULL = ObtainNULLColor();
            size = size * 2.83465;
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = pBasePoint;
            fe.set_Value(pointfcl.FindField("TYPE"), "专题图");
            if (pointfcl.FindField("JsonTxt") != -1)
            {
                fe.set_Value(pointfcl.FindField("JsonTxt"), json);
            }
            fe.Store();
            objID = fe.OID;
            //添加规则
            #region
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();

            AddRepGraphics(eles, pGraphics, ColorNULL);

            #endregion
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, angle);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint, r);
            rep.Graphics = repGraphics;

            rep.UpdateFeature();
            rep.Feature.Store();
            return pGraphics as IRepresentationMarker;
        }
       
        /// <summary>
        /// 老版本
        /// </summary> 
        public static void CreateFeature(IActiveView pAc,List<IElement> eles, ILayer player, IPoint pBasePoint, double size = 20)
        {
            IColor ColorNULL = ObtainNULLColor();
            size = size * 2.83465;
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = pBasePoint;
            fe.set_Value(pointfcl.FindField("TYPE"), "专题图");
            fe.Store();

            //添加规则
            #region
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();
          
            AddRepGraphics(eles, pGraphics, ColorNULL);
           
            #endregion
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint, r);
            rep.Graphics = repGraphics;

            rep.UpdateFeature();
            rep.Feature.Store();

        }

        public  IRepresentationMarker CreateFeatureEX(IActiveView pAc, List<IElement> eles, ILayer player, IPoint pBasePoint, out int fid, double size = 20)
        {
            IColor ColorNULL = ObtainNULLColor();
            size = size * 2.83465;
            IFeatureClass pointfcl = (player as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = pBasePoint;
            fe.set_Value(pointfcl.FindField("TYPE"), "专题图");
            fe.Store();
            fid = fe.OID;
            //添加规则
            #region
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();

            AddRepGraphics(eles, pGraphics, ColorNULL);

            #endregion
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pBasePoint, r);
            rep.Graphics = repGraphics;

            rep.UpdateFeature();
            rep.Feature.Store();
            return pGraphics as IRepresentationMarker;
        }

        public static void AddRepGraphics(List<IElement> eles, IRepresentationGraphics pGraphics, IColor ColorNULL)
        {
            try
            {
                foreach (IElement el in eles)
                {
                    IGeometry pRepGeo = el.Geometry;
                    if (el is IFillShapeElement)
                    {
                        #region
                        IFillSymbol pSymBol = (el as IFillShapeElement).Symbol;
                        IColor pcolor = (pSymBol as IFillSymbol).Color;
                        //定义rule
                        IRepresentationRule pRule = new RepresentationRule();

                        //定义线
                        IColor plineColor = (pSymBol as IFillSymbol).Outline.Color;
                        ISimpleLineSymbol psls = (pSymBol as IFillSymbol).Outline as ISimpleLineSymbol;
                        if (psls != null)
                        {
                            if (psls.Style == esriSimpleLineStyle.esriSLSNull)
                            {

                                plineColor = ColorNULL;
                            }
                        }
                        IBasicLineSymbol pBasicLine = new BasicLineSymbolClass();
                        ILineStroke pLineStroke = new LineStrokeClass();
                        IGraphicAttributes lineAttrs = pLineStroke as IGraphicAttributes;
                        lineAttrs.set_Value(0, psls.Width); //Define width.    
                        lineAttrs.set_Value(3, plineColor); //Define color.           
                        pBasicLine.Stroke = pLineStroke;
                        pRule.InsertLayer(0, pBasicLine as IBasicSymbol);
                        //定义面
                        IBasicFillSymbol pBasicFill = new BasicFillSymbolClass();
                        IFillPattern pFillPattern = null;
                        IGraphicAttributes fillAttrs = null;
                        IGradientFillSymbol gradiensym = pSymBol as IGradientFillSymbol;
                        if (gradiensym != null)
                        {
                            //渐变模式
                            pFillPattern = new GradientPattern();
                            fillAttrs = pFillPattern as IGraphicAttributes;

                            //   Color1 Color2 Algorithm Style Intervals Percentage
                            fillAttrs.set_Value(fillAttrs.get_IDByName("Color1"), (gradiensym.ColorRamp as IAlgorithmicColorRamp).FromColor); //Define Color 1 . 
                            fillAttrs.set_Value(fillAttrs.get_IDByName("Color2"), (gradiensym.ColorRamp as IAlgorithmicColorRamp).ToColor); //Define color 2. 
                            fillAttrs.set_Value(fillAttrs.get_IDByName("Algorithm"), (gradiensym.ColorRamp as IAlgorithmicColorRamp).Algorithm); //Define Algorithm. 
                            fillAttrs.set_Value(fillAttrs.get_IDByName("Style"), gradiensym.Style); //Define Style. 
                            fillAttrs.set_Value(fillAttrs.get_IDByName("Intervals"), 100); //Define Intervals. 
                            fillAttrs.set_Value(fillAttrs.get_IDByName("Percentage"), 90); //Define Percentage. 
                        
                        }
                        else
                        {
                            //单色模式
                            pFillPattern = new SolidColorPattern();
                            fillAttrs = pFillPattern as IGraphicAttributes;
                            ISimpleFillSymbol psfs = pSymBol as ISimpleFillSymbol;
                            if(psfs !=null)
                            {
                                if (psfs.Style == esriSimpleFillStyle.esriSFSNull)
                                {
                                    pcolor = ColorNULL;
                                }
                            }
                            fillAttrs.set_Value(0, pcolor); //Define color. 
                        }
                        pBasicFill.FillPattern = pFillPattern;
                        pRule.InsertLayer(1, pBasicFill as IBasicSymbol);

                        pGraphics.Add(pRepGeo, pRule);
                        #endregion
                    }
                    else if (el is ILineElement)
                    {
                        #region
                        IRepresentationRule pRule = new RepresentationRule();
                        ILineSymbol plineSym = (el as ILineElement).Symbol;
                        //定义线
                        IColor plineColor = plineSym.Color;
                        ISimpleLineSymbol psls = plineSym as ISimpleLineSymbol;
                        if (psls != null)
                        {
                            if (psls.Style == esriSimpleLineStyle.esriSLSNull)
                            {

                                plineColor = ColorNULL;
                            }
                        }
                        IBasicLineSymbol pBasicLine = new BasicLineSymbolClass();
                        ILineStroke pLineStroke = new LineStrokeClass();
                        IGraphicAttributes lineAttrs = pLineStroke as IGraphicAttributes;
                        lineAttrs.set_Value(0, psls.Width); //Define width.    
                        lineAttrs.set_Value(3, plineColor); //Define color.           
                        pBasicLine.Stroke = pLineStroke;
                        pRule.InsertLayer(0, pBasicLine as IBasicSymbol);
                        pGraphics.Add(pRepGeo, pRule);
                        #endregion
                    }
                    else if (el is IMarkerElement)
                    {
                        var symbol = (el as IMarkerElement).Symbol;

                        var rule = new RepresentationRuleClass();
                        rule.InitWithSymbol(symbol as ISymbol);
                        pGraphics.Add(pRepGeo, rule);
                    }
                    else if (el is ITextElement)
                    {
                        var te = el as ITextElement;
                        var text = te.Text;
                        var symbol = te.Symbol;
                        var xx = symbol as ISimpleTextSymbol;

                        if (text == null || text.Length < 0)
                        {
                            return;
                        }

                        AddTxtElement(el.Geometry  as IPoint,text, te.Symbol, pGraphics);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("专题图写入自由式制图表达错误："+ex.Message);
            }
        }
        private static void AddTxtElement(IPoint txtpoint, string txt, ITextSymbol symbol, IRepresentationGraphics g_)
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
            double txtunit = 1.0;//1号字体宽度
            
            double heightunit = 13.0 / 15.0;//1号字体的高度
          
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
                dis = ttnum / 2 * txtunit * fsize;//当前文字一半长度
                //dx = dx - dis;
                for (int i = 0; i < t.Length; i++, col++)
                {
                    string temp = t[i].ToString();
                    double tempnum = GetStrLen(temp);
                    //当前的位置
                    //double x = txtpoint.X;
                    //double y = txtpoint.Y;
                    double x = dx + tedis + tempnum * txtunit * fsize / 2 / 2;
                    double y = txtpoint.Y + row * heightunit * fsize + 0.65 * 0.5 * heightunit * fsize;
                    ESRI.ArcGIS.Geometry.IPoint p = new ESRI.ArcGIS.Geometry.PointClass() { X = x, Y = y };
                    //将当前点添加到制图表达中
                    TransMapPoint(t[i], p, symbol, g_);
                    tedis += tempnum * txtunit * fsize / 2;
                }
                row--;
            }
        }
        private static void TransMapPoint(char c, IPoint txtpoint, ITextSymbol symbol, IRepresentationGraphics g)
        {
            IMapContext mc = new MapContextClass();

             
            var xx = symbol as ISimpleTextSymbol;
            var cms = new CharacterMarkerSymbolClass();

            cms.CharacterIndex = (int)c;
            cms.Color = xx.Color;
            cms.Size = xx.Size;
            cms.Font = xx.Font;
            cms.Angle = xx.Angle;


            var rule = new RepresentationRuleClass();
            rule.InitWithSymbol(cms as ISymbol);
            var pt = txtpoint as ESRI.ArcGIS.Geometry.IPoint;
            //获取每个文字的坐标
            g.Add(pt, rule);

        }
        //获取文字总长度
        private static double GetStrLen(string str)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if ((int)str[i] > 127)
                    count++;
            }
            return str.Length - count + count * 2;
        }
        //获取空颜色测试
        private static IColor ObtainNULLColor()
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
    }
}

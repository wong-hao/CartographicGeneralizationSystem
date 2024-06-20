using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using stdole;
using SMGI.Common;
using System.Linq;
using SMGI.Plugin.ThematicChart.ThematicChart;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.ThematicChart
{
    public class ThematicPOICmd : SMGICommand
    {
        //定位符号法
        public ThematicPOICmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        IColor markerColor = null;
        Dictionary<string, string> GradeDic = null;//name->级别
        Dictionary<string, double> MarkerDic = null;//1~2->1
        double lengendSize = 0;
        double mapScale = 0;
        IActiveView pAc = null;
        private List<IElement> eles = new List<IElement>();
        ILayer pPointLayer = null;
        Dictionary<IPoint, string> annoLg = new Dictionary<IPoint, string>();
        public override void OnClick()
        {
            pAc = m_Application.ActiveView;
            mapScale = (pAc as IMap).ReferenceScale;
         
            FrmPOIBcg frm = new FrmPOIBcg();
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            string title = frm.GeoTitle;
            markerColor = frm.CMYKColors;
            GradeDic = frm.gradeData;
            MarkerDic = frm.MarkersDic;

            IntialRule(markerColor);
            string lyrname = frm.GeoLayer;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).Name == lyrname);
            })).ToArray();
            if (lyrs.Length == 0)
            {
                MessageBox.Show("不存在图层："+lyrname);

                return;
            }
            ILayer pRepLayer = lyrs.First();//boua
            var lyrsp = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pPointLayer = lyrsp.First();//point
            var extent = ((pRepLayer as IFeatureLayer).FeatureClass as IGeoDataset).Extent;
            IPoint anhorpoint = extent.LowerLeft;
            anhorpoint.Y -= 10 * 1e-3 * mapScale;

            m_Application.EngineEditor.StartOperation();
            WaitOperation wo = m_Application.SetBusy();
            try
            {
                //生成图例
                var clone = (anhorpoint as IClone).Clone() as IPoint;
                int lgID = CreateLengRep(clone);
                DrawLengend(title, clone, lgID);
              
           
                mapScale = 1000;
                CreateThPOIMaker(pRepLayer as IFeatureLayer);
                pAc.Refresh();
                m_Application.EngineEditor.StopOperation("定位符号法渲染");
                wo.Dispose();
                MessageBox.Show("生成完成！");
            }
            catch(Exception ex)
            {
                wo.Dispose();
            }
        }
        /// <summary>
        /// 绘制背景
        /// </summary>
        private void IntialRule(IColor color)
        {
            //SMGI.Plugin.ThematicChart.MapsDesign.MapFormat.MapFormatHelper mh = new MapsDesign.MapFormat.MapFormatHelper();
            // ruleCircle= mh.ObtainGradientCircle("渐变圆", color);
            CreateRuleCircle(color);
        }
        private void CreateRuleCircle(IColor color)
        {
            IPoint pt = new PointClass() { X = 0, Y = 0 };
            IPointCollection pc = new RingClass();
            double radio = 5;
            
            //上半圆
            for (int i = 0; i <= 50; i++)
            {
                double x = -radio + 2 * radio * i / 50;
                double y = radio * Math.Pow(1 - x * x / (radio * radio), 0.5);
                IPoint p = new PointClass() { X = x, Y = y };
                pc.AddPoint(p);
            }
            //下半圆
            for (int i = 0; i <= 50; i++)
            {
                double x = radio - 2 * radio * i / 50;
                double y = -radio * Math.Pow(1 - x * x / (radio * radio), 0.5);
                IPoint p = new PointClass() { X = x, Y = y };
                pc.AddPoint(p);
            }

            (pc as IRing).Close();
           

             IGeometryCollection polygon = new PolygonClass();
             polygon.AddGeometry(pc as IGeometry);
             (polygon as IPolygon).Smooth(0);
             (polygon as ITopologicalOperator4).Simplify();
            GraphicsHelper gh = new GraphicsHelper(pAc);
            #region
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 255;
            rgb.Blue = 255;
            rgb.Green = 255;
            IGradientFillSymbol gradiensym = gh.CreateGradientSym(markerColor, rgb, esriGradientFillStyle.esriGFSBuffered);
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();
            IRepresentationRule pRule = new RepresentationRule();
            IBasicFillSymbol pBasicFill = new BasicFillSymbolClass();

            IFillPattern pFillPattern = null;
            {
                IGraphicAttributes fillAttrs = null;
            
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
            pBasicFill.FillPattern = pFillPattern;
            pRule.InsertLayer(1, pBasicFill as IBasicSymbol);
            #endregion
            pGraphics.Add(polygon as IGeometry, pRule);

            ruleCircle = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            bs.set_Value(2, 20); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            ruleCircle.InsertLayer(0, bs);

        }
        private void CreateThPOIMaker(IFeatureLayer boualyr)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IFeature fe;
            IFeatureCursor cursor = boualyr.FeatureClass.Search(null, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                string name = fe.get_Value(boualyr.FeatureClass.FindField("NAME")).ToString();
                name = name.Trim();
                IArea pArea = fe.ShapeCopy as IArea;
                IPoint p = pArea.LabelPoint;
                if (GradeDic.ContainsKey(name))
                {
                    eles.Clear();
                    if (MarkerDic.ContainsKey(GradeDic[name]))
                    {
                        double size = MarkerDic[GradeDic[name]];
                        CreatePOIMaker(size, p);
                    }
                }
            }
        }
        IRepresentationRule ruleCircle = null;
        private void CreatePOIMaker(double size, IPoint anchorPoint)
        {
            try
            {
                IFeatureClass pointfcl = (pPointLayer as IFeatureLayer).FeatureClass;

                IFeature fe = pointfcl.CreateFeature();
                fe.Shape = anchorPoint;
                fe.set_Value(pointfcl.FindField("TYPE"), "定位符号专题图");
                fe.Store();

                //添加规则
                IGraphicAttributes bs = ruleCircle.get_Layer(0) as IGraphicAttributes;
                bs.set_Value(2, size * 2.83); //size

                //自由
                var rp = (pPointLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                IRepresentationClass m_RepClass = rp.RepresentationClass;
                if (m_RepClass == null)
                    return;
                IMapContext mc = new MapContextClass();
                mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
                IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
                IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
                repGraphics.Add(anchorPoint, ruleCircle);
                rep.Graphics = repGraphics;

                rep.UpdateFeature();
                rep.Feature.Store();
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建定位符号错误：" + ex.Message);
            }
        }
        
       
        private IEnvelope DrawLengend(string title, IPoint pBasePoint,int id)
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer)&& (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LANNO";
            })).ToArray();
            ILayer pAnoLayer = lyrs.First();
            IFeatureClass fclAnno = (pAnoLayer as IFeatureLayer).FeatureClass;
            IEnvelope penve = new EnvelopeClass();
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            GraphicsHelper gh = new GraphicsHelper(pAc);

            double maxSize = MarkerDic.Values.ToArray().Max();
            double cubedis = maxSize * 1e-3 * mapScale;

            double ystep = cubedis + 2.5e-3 * mapScale;
         
            double fontsize = 8*2.83;
            double heightunit = 3.97427;//1号字体1万的高度

            double txtHeight = heightunit * fontsize * mapScale * 1e-4;
            double cubstep = 0;

            double ylen = 0;
            
            var maxsize=  MarkerDic.Max(t=>t.Value);
         
            foreach (var kv in MarkerDic)
            {
              
                double circle = kv.Value * 1e-3 * mapScale;
                ylen += circle/2;
                IPoint point = new PointClass() { X = pBasePoint.X + cubstep, Y = pBasePoint.Y - ylen };
               
               
                //注记
                double stw = gh.GetStrWidth(kv.Key.ToString(), mapScale, fontsize);
                double sth = heightunit * fontsize * mapScale * 1e-4;
              
                double dy = (txtHeight) * 0.365;
                IPoint lbpoint = new PointClass() { X = point.X + stw / 2 + maxsize * 1e-3 * mapScale, Y = point.Y };
                InsertAnnoFea(fclAnno, lbpoint, kv.Key, fontsize, id);

                ylen += circle / 2 + 5e-3 * mapScale;
               
            }
            if (title != "")
            {
                IPoint titlepoint = new PointClass() { X = pBasePoint.X + 2e-2 * mapScale, Y = pBasePoint.Y + 5e-3 * mapScale };
                InsertAnnoFea(fclAnno, titlepoint, title, fontsize, id);  

            }
            return penve;


        }
        private int CreateLengRep(IPoint anchorPoint)
        {
            //渐变规则
           // IPoint anchorPoint = new PointClass { X = anchorPoint_.X, Y = anchorPoint_.Y };
            IRepresentationRule pRule = new RepresentationRule();
            #region
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 255;
            rgb.Blue = 255;
            rgb.Green = 255;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IGradientFillSymbol gradiensym = gh.CreateGradientSym(markerColor, rgb, esriGradientFillStyle.esriGFSBuffered);
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();
            IBasicFillSymbol pBasicFill = new BasicFillSymbolClass();
            IFillPattern pFillPattern = null;
            {
                IGraphicAttributes fillAttrs = null;

                //渐变模式
                pFillPattern = new GradientPattern();
                fillAttrs = pFillPattern as IGraphicAttributes;
                fillAttrs.set_Value(fillAttrs.get_IDByName("Color1"), (gradiensym.ColorRamp as IAlgorithmicColorRamp).FromColor); //Define Color 1 . 
                fillAttrs.set_Value(fillAttrs.get_IDByName("Color2"), (gradiensym.ColorRamp as IAlgorithmicColorRamp).ToColor); //Define color 2. 
                fillAttrs.set_Value(fillAttrs.get_IDByName("Algorithm"), (gradiensym.ColorRamp as IAlgorithmicColorRamp).Algorithm); //Define Algorithm. 
                fillAttrs.set_Value(fillAttrs.get_IDByName("Style"), gradiensym.Style); //Define Style. 
                fillAttrs.set_Value(fillAttrs.get_IDByName("Intervals"), 100); //Define Intervals. 
                fillAttrs.set_Value(fillAttrs.get_IDByName("Percentage"), 90); //Define Percentage. 

            }
            pBasicFill.FillPattern = pFillPattern;
            pRule.InsertLayer(1, pBasicFill as IBasicSymbol);
            #endregion
            #region 图例几何
            double step = 0;
            double markersize = (MarkerDic.Count-1)*5;
            foreach (var kv in MarkerDic)
            {
                double size = kv.Value;
                markersize += size;
                step += size / 2;
               
                IPointCollection pc = new RingClass();
                double radio = size/2;
                //上半圆
                for (int i = 0; i <= 50; i++)
                {
                    double x = -radio + 2 * radio * i / 50;
                    double y = radio * Math.Pow(1 - x * x / (radio * radio), 0.5);
                    IPoint p = new PointClass() { X = x, Y = y-step };
                    pc.AddPoint(p);
                }
                //下半圆
                for (int i = 0; i <= 50; i++)
                {
                    double x = radio - 2 * radio * i / 50;
                    double y = -radio * Math.Pow(1 - x * x / (radio * radio), 0.5);
                    IPoint p = new PointClass() { X = x, Y = y - step };
                    pc.AddPoint(p);
                }
                (pc as IRing).Close();
                IGeometryCollection polygon = new PolygonClass();
                polygon.AddGeometry(pc as IGeometry);
                (polygon as IPolygon).Smooth(0);
                (polygon as ITopologicalOperator4).Simplify();
                pGraphics.Add(polygon as IGeometry, pRule);
                step += size / 2 + 5;
            }
            var ruleLengend = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            bs.set_Value(2, markersize*2.83); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            ruleLengend.InsertLayer(0, bs);
            #endregion

            double dy = MarkerDic.Min(t => t.Value)*1e-3 * mapScale;
          
            anchorPoint.Y -= dy;
            IFeatureClass pointfcl = (pPointLayer as IFeatureLayer).FeatureClass;

            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = anchorPoint;
            fe.set_Value(pointfcl.FindField("TYPE"), "定位符号专题图");
            fe.Store();

            //自由
            var rp = (pPointLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            if (m_RepClass == null)
                return fe.OID;
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(pAc.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(anchorPoint, ruleLengend);
            rep.Graphics = repGraphics;

            rep.UpdateFeature();
            rep.Feature.Store();
            return fe.OID;
        }
        private bool InsertAnnoFea(IFeatureClass pFeatCls, IGeometry pGeometry, string annoName, double fontSize,int id)
        {  
            IFontDisp pFont = new StdFont()
            {
                Name="黑体",
                Size = 2
            } as IFontDisp;
           
            IFeatureClass annocls = pFeatCls;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, pFont, fontSize);
            ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElement;
            pSymbolCollEle.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annocls.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.AnnotationClassID = (pPointLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pAnnoFeature.Annotation = pElement;
            pFeature.Store();
            return true;
        }
        private ITextElement CreateTextElement(IGeometry pGeoTxt, string txt, IFontDisp pFont, double size)
        {
            IRgbColor pColor = new RgbColorClass()
            {
                Red = 0,
                Blue = 0,
                Green = 0
            };

            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = pColor,
                Font = pFont,
                Size = size
            };
            ITextElement pTextElment = null;
            IElement pEle = null;
            pTextElment = new TextElementClass()
            {
                Symbol = pTextSymbol,
                ScaleText = true,
                Text = txt
            };
            pEle = pTextElment as IElement;

            pEle.Geometry = pGeoTxt;
            return pTextElment;
        }
        private void CreateAnno(IPoint pt,string txt,int id)
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer)&&((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LANNO");
            })).ToArray();
            ILayer pAnoLayer = lyrs.First();
            IFeatureClass fclAnno = (pAnoLayer as IFeatureLayer).FeatureClass;

            InsertAnnoFea(fclAnno, pt, txt, 15, id);
            
        }

    }
}
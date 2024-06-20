using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using stdole;

namespace SMGI.Plugin.ThematicChart
{
    public class CreateEarthQuakeWaveCmd : SMGI.Common.SMGICommand
    {
        public int RingCount;
        public double RingDistance;
        public double CenterSize;
        public double RingLineWidth;
        public LongitudeOrLatitude Longitude;
        public LongitudeOrLatitude Latitude;
        public double Level;
        public double Depth;
        public double WaveFontSize;
        public double CenterFontSize;
        public IColor WaveAnnoColor;
        public IColor CenterAnnoColor;
        private IEngineEditor m_engineEditor;
     
        private double MapScale;

        private string EarthquakeLayerName = "LPOINT";
        private IRepresentationClass m_RepClass = null;
        private IFeatureLayer m_FeatureLayer = null;
        public CreateEarthQuakeWaveCmd()
        {
            m_caption = "创建地震波";
            m_toolTip = "根据经纬坐标创建地震波";
            m_category = "要素创建";
          
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
        public override void OnClick()
        {
            m_Application.MapControl.CurrentTool = null;
            MapScale = m_Application.MapControl.Map.ReferenceScale;
            m_engineEditor = m_Application.EngineEditor;
            //获取特定图层的制图表达规则
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            m_FeatureLayer = lyrs.First() as IFeatureLayer;
            if (m_FeatureLayer == null)
            {
                MessageBox.Show("缺失" + EarthquakeLayerName+"图层，无法进行创建");
                return;
            }
            if ((m_FeatureLayer as IGeoFeatureLayer).Renderer is RepresentationRenderer)
            {
                m_RepClass = ((IRepresentationRenderer)(m_FeatureLayer as IGeoFeatureLayer).Renderer).RepresentationClass;
            }
            IEnvelope FullExtent = m_Application.ActiveView.FullExtent;
            IPoint ProjectPoint = new PointClass() { X = 0.5 * (FullExtent.XMin + FullExtent.XMax), Y = 0.5 * (FullExtent.YMin + FullExtent.YMax) };
            IPoint GeoPoint = GetGeo2000(m_Application.ActiveView, ProjectPoint.X, ProjectPoint.Y);
            var frm = new SMGI.Plugin.ThematicChart.FrmEarthCmdSet(GeoPoint);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                RingCount = frm.RingCount;
                RingDistance = frm.RingDis;
                RingLineWidth = frm.RingLineWidth;
                CenterSize = frm.CenterSize;
                Longitude = frm.Longitude;
                Latitude = frm.Latitude;
                Level = frm.Level;
                Depth = frm.Depth;
                WaveFontSize = frm.WaveFontSize;
                CenterFontSize = frm.CenterFontSize;
                WaveAnnoColor = frm.WaveAnnoColor;
                CenterAnnoColor = frm.CenterAnnoColor;
                //锚点
                double dx = Longitude.Degree + Longitude.Minute / 60 + Longitude.Second / 3600;
                double dy = Latitude.Degree + Latitude.Minute / 60 + Latitude.Second / 3600;
                IPoint anchorPoint = GetProject2000(m_Application.ActiveView, dx, dy);
                m_Application.EngineEditor.StartOperation();
                IFeatureClass pointfcl = (m_FeatureLayer as IFeatureLayer).FeatureClass;
                IFeature fe = pointfcl.CreateFeature();
                fe.Shape = anchorPoint;
                fe.set_Value(pointfcl.FindField("TYPE"), "地震波");
                fe.Store();
                CreateEarthFeature(fe);
                CreateAnno(fe);
                m_Application.EngineEditor.StopOperation("地震专题图生成");
            }

        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
          
        }         
        
        /// <summary>
        ///  
        /// </summary>
        /// <param name="_object"></param>
        private void CreateEarthFeature(IFeature pFeature)
        {
         
            IPoint pPoint = pFeature.ShapeCopy as IPoint;
            //获取当前要素的制图表达
            var mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(pFeature, mc);
            //获取点和线的规则
            IRepresentationRule PointRule=null;
            IRepresentationRule PolylineRule=null;
            IRepresentationRule lineRule = null;
            #region
            PolylineRule = new RepresentationRule();
            //定义线
            IColor plineColor = new RgbColorClass { Red = 200 };
            IBasicLineSymbol pBasicLine = new BasicLineSymbolClass();
            ILineStroke pLineStroke = new LineStrokeClass();
            IGraphicAttributes lineAttrs = pLineStroke as IGraphicAttributes;
            lineAttrs.set_Value(0, RingLineWidth*2.83); //Define width.    
            lineAttrs.set_Value(3, plineColor); //Define color.           
            pBasicLine.Stroke = pLineStroke;
            PolylineRule.InsertLayer(0, pBasicLine as IBasicSymbol);

            lineRule = (PolylineRule as IClone).Clone() as IRepresentationRule;
            var ga= (lineRule.get_Layer(0) as IBasicLineSymbol).Stroke as IGraphicAttributes;
            ga.set_Value(0, RingLineWidth * 2.83*0.5);
            #endregion
            #region
            var el = new SimpleMarkerSymbolClass();
            el.Size = CenterSize*2.83;
            el.Color = new RgbColorClass { Red = 200 };
            el.Style = esriSimpleMarkerStyle.esriSMSCircle;
            var symbol = el;
            PointRule = new RepresentationRuleClass();
            (PointRule as IRepresentationRuleInit).InitWithSymbol(symbol as ISymbol);
            #endregion
            
            if(PointRule==null||PolylineRule==null)
            {
                MessageBox.Show("制图表达规则缺失，生成失败");
                return;
            }
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            //添加十字丝
            double len=CenterSize*MapScale*1.0e-3;
            IPointCollection line = new PolylineClass();
            line.AddPoint(new PointClass { X = pPoint.X - len, Y = pPoint.Y });
            line.AddPoint(new PointClass { X = pPoint.X + len, Y = pPoint.Y });
            repGraphics.Add(line as IPolyline, lineRule);

            line = new PolylineClass();
            line.AddPoint(new PointClass { X = pPoint.X, Y = pPoint.Y - len });
            line.AddPoint(new PointClass { X = pPoint.X, Y = pPoint.Y + len });
            repGraphics.Add(line as IPolyline, lineRule);
            //添加点状要素及规则
           
            repGraphics.Add(pPoint, PointRule);
            
            
            //------------生成地震波RingCount个环---实际距离按10，20，30等整数来-- 0.001 * BlendPara * scale;------
            double RingIntervalDistance = RingDistance;//
            for (int i = 1; i < RingCount+1; i++)
            {
                ITopologicalOperator topo = pPoint as ITopologicalOperator;
                IGeometry geometry = topo.Buffer(RingIntervalDistance*1000 * i);
                repGraphics.Add(geometry, PolylineRule);
            }
            rep.Graphics = repGraphics;
            rep.UpdateFeature();
            rep.Feature.Store();
            m_Application.MapControl.Refresh();
            m_engineEditor.StopOperation("创建地震波要素");
        }


        private void CreateAnno(IFeature pFeature)
        {
            var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LANNO");
            })).ToArray();
            var annoly = lyr.First();
            IFeatureClass annoFcl = (annoly as IFeatureLayer).FeatureClass;
            IPoint centerPoint = pFeature.Shape as IPoint;
            //该点距离中心点水平夹角30的位置
            IPoint directionPoint = new PointClass()
            {
                X = centerPoint.X + 1.732,
                Y = centerPoint.Y + 1
            };

            IPointCollection pPointCollection = new PolylineClass();
            object missing = Type.Missing;
            pPointCollection.AddPoint(centerPoint, ref missing, ref missing);
            pPointCollection.AddPoint(directionPoint, ref missing, ref missing);
            IPolyline pPolyline = pPointCollection as IPolyline;
            //求取该直线法线角度作为注记点的旋转角度
            double pNormalAngle = GetNormalAngle(pPolyline);
            for (int i = 1; i < RingCount + 1; i++)
            {

                //根据该点计算与环心点的距离
                double Distance = RingDistance *1000* i;
                //获取注记位置点
                IPoint outPoint = new PointClass();
                pPolyline.QueryPoint(esriSegmentExtension.esriExtendAtTo, Distance, false, outPoint);
                //创建注记
                string message = string.Empty;
                ITextElement pTextElement = CreateTextElement(outPoint, (Distance/1000).ToString() + "千米", "宋体", WaveFontSize, WaveAnnoColor, out message, 100, 0.5, pNormalAngle, true);

                IElement pElement = pTextElement as IElement;
                IFeature feature = annoFcl.CreateFeature();
                feature.set_Value(annoFcl.FindField("TYPE"), "地震波");
                IAnnotationFeature2 pAnnoFeature = feature as IAnnotationFeature2;
                pAnnoFeature.AnnotationClassID = pFeature.Class.ObjectClassID;
                pAnnoFeature.LinkedFeatureID = pFeature.OID;
                pAnnoFeature.Annotation = pElement;
                feature.Store();
            }
            //地震中心注记创建
            {
                string message = string.Empty;
                IPoint annoPoint = new PointClass()
                {
                    X = centerPoint.X + 0.5 * RingDistance * 1000,
                    Y = centerPoint.Y - 0.5 * RingDistance * 1000
                };
                string EarthQuakeCenter = string.Format("震中：北纬{0}°{1}\'{2}\" 东经{3}°{4}\'{5}\"\r\n震级：{6}级\r\n震源深度：{7}千米", Latitude.Degree, Latitude.Minute, Latitude.Second, Longitude.Degree, Longitude.Minute, Longitude.Second, Level, Depth);
                ITextElement pTextElement = CreateTextElement(annoPoint, EarthQuakeCenter, "宋体", CenterFontSize, CenterAnnoColor, out message, 100, 2, 0, true);
                ITextSymbol textSymbol = pTextElement.Symbol;
                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;
                formattedTextSymbol.Background = null;
                pTextElement.Symbol = textSymbol;
                IElement pElement = pTextElement as IElement;
                IFeature feature = annoFcl.CreateFeature();
                feature.set_Value(annoFcl.FindField("TYPE"), "地震波");
                IAnnotationFeature2 pAnnoFeature = feature as IAnnotationFeature2;
                pAnnoFeature.AnnotationClassID = pFeature.Class.ObjectClassID;
                pAnnoFeature.LinkedFeatureID = pFeature.OID;
                pAnnoFeature.Annotation = pElement;
                feature.Store();
            }
        }
        /// <summary>
        /// 创建字体符号    
        /// </summary>
        /// <param name="position">符号的位置</param>
        /// <param name="annoText">注记的文本</param>
        /// <param name="fontFamily">字体</param>
        /// <param name="fontSize">字大</param>
        /// <param name="cmykColor">颜色</param>
        /// <param name="characterWidth">字宽</param>
        /// <param name="IsTaobai">是否套白</param>
        /// <param name="TaobaiWidth">套白宽度</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <param name="Bold">是否粗体</param>
        /// <returns>文本元素</returns>
        public static ITextElement CreateTextElement(IGeometry position, string annoText, string fontFamily, double fontSize, IColor cmykColor, out string message, double characterWidth = 100, double TaobaiWidth = 0, double rotateAngle = 0, bool Bold = false)
        {
            message = string.Empty;
            bool fontExist = false;
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily ff in fonts.Families)
            {
                if (ff.Name == fontFamily)
                {
                    fontExist = true;
                }
            }
            if (fontExist)
            {
                //创建文本元素
                ITextElement pTextElment = new TextElementClass();
                pTextElment.Text = annoText;//文本内容
                pTextElment.ScaleText = true;
                IFontDisp pFont = new StdFont() as IFontDisp;
                pFont.Name = fontFamily;
                pFont.Bold = Bold;
                ITextSymbol pTextSymbol = new TextSymbolClass();
                pTextSymbol.Color = cmykColor;//字色
                pTextSymbol.Font = pFont;//字体
                pTextSymbol.Size = fontSize * 2.8345;//字大
                pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;//左对齐
                pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;//竖直居中
                (pTextSymbol as IFormattedTextSymbol).CharacterWidth = characterWidth;//字宽
                if (TaobaiWidth != 0)
                {   //套白
                    (pTextSymbol as IMask).MaskStyle = esriMaskStyle.esriMSHalo;
                    (pTextSymbol as IMask).MaskSize = TaobaiWidth * 2.83;
                    IFillSymbol pFillSymbol = new SimpleFillSymbol();
                    pFillSymbol.Color = new CmykColor()
                    {
                        CMYK = 0
                    } as IColor;
                    ILineSymbol pLineSymbol = new SimpleLineSymbol();
                    pLineSymbol.Width = 0;
                    pFillSymbol.Outline = pLineSymbol;
                    (pTextSymbol as IMask).MaskSymbol = pFillSymbol;
                }
                IFormattedTextSymbol formattedTextSymbol = pTextSymbol as IFormattedTextSymbol;
                IBalloonCallout balloonCallout = new BalloonCalloutClass();
                balloonCallout.Style = esriBalloonCalloutStyle.esriBCSRectangle;
                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                ILineSymbol lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 0;
                fillSymbol.Outline = lineSymbol;
                fillSymbol.Color = new RgbColorClass(){Red =255,Green=255,Blue=255};
                balloonCallout.Symbol = fillSymbol;
                formattedTextSymbol.Background = balloonCallout as ITextBackground;
                pTextElment.Symbol = pTextSymbol;       
                IElement pEle = pTextElment as IElement;
                pEle.Geometry = position;
                if (rotateAngle != 0)
                {
                    ITransform2D pTransform2D = pEle as ITransform2D;
                    pTransform2D.Rotate(position as IPoint, rotateAngle);
                }
                return pTextElment;
            }
            else
            {
                message = "系统缺失[" + fontFamily + "]字体库";
                return null;
            }
        }
        private double GetNormalAngle(IPolyline pPolyline)
        {
            //IPolycurve pPolycurve;
            ILine pTangentLine = new Line();
            pPolyline.QueryNormal(esriSegmentExtension.esriNoExtension, 10, true, pPolyline.Length, pTangentLine);
            Double radian = pTangentLine.Angle;
            Double angle = radian * 180 / Math.PI;
            double Angle_Mod = angle % 360;
            //必须字体朝北，角度必须控制在-90～90度之间
            if (!(Angle_Mod <= 90 && Angle_Mod >= -90))
            {
                Angle_Mod += 180;
            }
            while (Angle_Mod < 0)
            {
                Angle_Mod = Angle_Mod + 360;
            }
            radian = Angle_Mod * Math.PI / 180;
            // 返回弧度
            return radian;
        }

        /// <summary>
        /// 2000地理转投影
        /// </summary>
        /// <param name="activeView"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private IPoint GetProject2000(IActiveView activeView, double x, double y)
        {
            try
            {
                IMap map = activeView.FocusMap;
                IPoint pt = new PointClass();
                ISpatialReference geoRef = CreateCGCSys2000();
                ISpatialReference mapRef = map.SpatialReference;
                pt.PutCoords(x, y);
                IGeometry geo = pt as IGeometry;
                geo.SpatialReference = geoRef;
                geo.Project(mapRef);
                return pt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 2000投影转地理
        /// </summary>
        /// <param name="activeView"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private IPoint GetGeo2000(IActiveView activeView, double x, double y)
        {
            try
            {
                IMap map = activeView.FocusMap;
                IPoint pt = new PointClass();
                ISpatialReference geoRef = CreateCGCSys2000();
                ISpatialReference mapRef = map.SpatialReference;
                pt.PutCoords(x, y);
                IGeometry geo = pt as IGeometry;
                geo.SpatialReference = mapRef;
                geo.Project(geoRef);
                return pt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        private ISpatialReference CreateCGCSys2000()
        {
            ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
            ISpatialReference spatialReference = spatialReferenceFactory.CreateGeographicCoordinateSystem(4490);//CGCS2000

            //ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_World_Mercator);//esriSRProjCS_NAD1983UTM_20N);
            ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)spatialReference;
            spatialReferenceResolution.ConstructFromHorizon();
            ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)spatialReference;
            spatialReferenceTolerance.SetDefaultXYTolerance();
            return spatialReference;
        }

        public void ClearFeatures(IFeatureClass fc, IQueryFilter qf_)
        {
            IFeature fe;
            IFeatureCursor cursor = fc.Update(qf_, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                cursor.DeleteFeature();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }
    }
}

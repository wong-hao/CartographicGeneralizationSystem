using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class CreateEarthQuakeWaveForm : Form
    {
        public string QuakeCenterCoordText
        {
            get
            {
                return string.Format("震中：北纬{0}°{1}′{2}\" 东经{3}°{4}′{5}\"", tbLonDu.Text, tbLonMin.Text, tbLonSec.Text, tbLatDu.Text,tbLatMin.Text, tbLatSec.Text);
            }
        }

        public string QuakeLevelText
        {
            get
            {
                return string.Format("震级：{0}级", tb_level.Text);
            }
        }

        public string QuakeDepthText
        {
            get
            {
                return string.Format("震源深度：{0}千米", tb_depth.Text);
            }
        }

        public double QuakeCenterFontSize
        {
            get
            {
                return (double)Center_FontSize.Value;
            }
        }

        public IColor QuakeCenterFontColor
        {
            get
            {
                return new RgbColorClass { Red = btn_centerColor.BackColor.R, Green = btn_centerColor.BackColor.G, Blue = btn_centerColor.BackColor.B };
            }
        }


        public double QuakeCenterSymbolSize
        {
            get
            {
                return double.Parse(txtCenterSymbolSize.Text);
            }
        }


        public int QuakeRingCount
        {
            get
            {
                return int.Parse(txtRingCount.Text);
            }
        }

        public double QuakeRingLineWidth
        {
            get
            {
                return double.Parse(txtLineWidth.Text);
            }
        }

        public double QuakeRingDistance
        {
            get
            {
                return double.Parse(txtRingDis.Text);
            }
        }

        public double QuakeRingFontSize
        {
            get
            {
                return (double)Wave_FontSize.Value;
            }
        }

        public IColor QuakeRingFontColor
        {
            get
            {
                return new RgbColorClass { Red = btn_waveColor.BackColor.R, Green = btn_waveColor.BackColor.G, Blue = btn_waveColor.BackColor.B };
            }
        }

        private GApplication _app;
        private IPoint _quakeCenterPoint;

        public CreateEarthQuakeWaveForm(GApplication app, IPoint centerPoint = null)
        {
            InitializeComponent();

            _app = app;
            _quakeCenterPoint = centerPoint;
            if (_quakeCenterPoint != null)
            {
                ISpatialReference geoRF = null;
                #region 创建地理坐标系
                ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                geoRF = spatialReferenceFactory.CreateGeographicCoordinateSystem(4490);//CGCS2000

                ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)geoRF;
                spatialReferenceResolution.ConstructFromHorizon();
                ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)geoRF;
                spatialReferenceTolerance.SetDefaultXYTolerance();

                #endregion

                IPoint geoPT = (centerPoint as IClone).Clone() as IPoint;
                geoPT.SpatialReference = app.ActiveView.FocusMap.SpatialReference;
                geoPT.Project(geoRF);

                int lonDu = (int)Math.Floor(geoPT.X);
                int lonMin = (int)Math.Floor((geoPT.X - lonDu) * 60);
                int lonSec = (int)Math.Floor(((geoPT.X - lonDu) * 60 - lonMin) * 60);
                tbLonDu.Text = lonDu.ToString();
                tbLonMin.Text = lonMin.ToString();
                tbLonSec.Text = lonSec.ToString();
                
                int latDu = (int)Math.Floor(geoPT.Y);
                int latMin = (int)Math.Floor((geoPT.Y - latDu) * 60);
                int latSec = (int)Math.Floor(((geoPT.Y - latDu) * 60 - latMin) * 60);
                tbLatDu.Text = latDu.ToString();
                tbLatMin.Text = latMin.ToString();
                tbLatSec.Text = latSec.ToString();
            }
        }

        private void btn_Preview_Click(object sender, EventArgs e)
        {
            if (!ParamsIsValid())
                return;

            DeleteEarthQuakeGroupElement(_app.MapControl.ActiveView.GraphicsContainer, QuakeCenterCoordText);

            IGroupElement groupEle = CreateEarthQuakeGroupElement(_app, _quakeCenterPoint, QuakeCenterCoordText,
                QuakeLevelText, QuakeDepthText, QuakeCenterFontSize, QuakeCenterFontColor,
                QuakeCenterSymbolSize, QuakeRingCount, QuakeRingLineWidth, QuakeRingDistance,
                QuakeRingFontSize, QuakeRingFontColor);

            if (groupEle == null)
            {
                return;
            }

            _app.MapControl.ActiveView.GraphicsContainer.AddElement(groupEle as IElement, 0);
            _app.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, groupEle, _app.ActiveView.Extent);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (!ParamsIsValid())
                return;

            IGroupElement ge = getEarthQuakeGroupElement(_app.MapControl.ActiveView.GraphicsContainer,  QuakeCenterCoordText);
            if (ge == null)
            {
                IGroupElement groupEle = CreateEarthQuakeGroupElement(_app, _quakeCenterPoint, QuakeCenterCoordText,
                    QuakeLevelText, QuakeDepthText, QuakeCenterFontSize, QuakeCenterFontColor,
                    QuakeCenterSymbolSize, QuakeRingCount, QuakeRingLineWidth, QuakeRingDistance,
                    QuakeRingFontSize, QuakeRingFontColor);

                if (groupEle == null)
                {
                    return;
                }

                _app.MapControl.ActiveView.GraphicsContainer.AddElement(groupEle as IElement, 0);
                _app.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, groupEle, _app.ActiveView.Extent);
            }

            
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private bool ParamsIsValid()
        {
            int Lon = 0;
            if (!int.TryParse(tbLonDu.Text, out Lon) || Lon < 0)
            {
                MessageBox.Show("经度输入不正确！");
                return false;
            }

            int Lat = 0;
            if (!int.TryParse(tbLatDu.Text, out Lat) || Lat < 0)
            {
                MessageBox.Show("纬度输入不正确！");
                return false;
            }

            double level = 0;
            double.TryParse(tb_level.Text, out level);
            if (level <= 0)
            {
                MessageBox.Show("地震等级输入不正确！");
                return false;
            }
            
            double depth = 0;
            double.TryParse(tb_depth.Text, out depth);
            if (depth <= 0)
            {
                MessageBox.Show("地震深度输入不正确！");
                return false;
            }

            double symbolSize = 0;
            double.TryParse(txtCenterSymbolSize.Text, out symbolSize);
            if (symbolSize <= 0)
            {
                MessageBox.Show("震中符号尺寸输入不正确！");
                return false;
            }

            int ringCount = 0;
            int.TryParse(txtRingCount.Text, out ringCount);
            if (ringCount <= 0)
            {
                MessageBox.Show("地震波环数输入不正确！");
                return false;
            }

            double ringWidth = 0;
            double.TryParse(txtLineWidth.Text, out ringWidth);
            if (ringWidth <= 0)
            {
                MessageBox.Show("地震波线宽输入不正确！");
                return false;
            }

            double ringDistance = 0;
            double.TryParse(txtRingDis.Text, out ringDistance);
            if (ringDistance <= 0)
            {
                MessageBox.Show("地震波间隔输入不正确！");
                return false;
            }

            return true;
        }

        public static IGroupElement CreateEarthQuakeGroupElement(GApplication app, IPoint quakeCenterPoint,
            string quakeCenterCoordText, string quakeLevelText, string quakeDepthText, double quakeCenterFontSize,
            IColor quakeCenterFontColor, double quakeCenterSymbolSize, int quakeRingCount, double quakeRingLineWidth,
            double quakeRingDistance, double quakeRingFontSize, IColor quakeRingFontColor)
        {
            try
            {
                IGroupElement groupElement = new GroupElementClass() { Name = quakeCenterCoordText };

                double mapScale = app.ActiveView.FocusMap.ReferenceScale;

                #region 中心点符号
                ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbolClass();
                simpleMarkerSymbol.Color = new RgbColorClass { Red = 255 };//颜色
                simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCross;//符号类型
                simpleMarkerSymbol.Size = quakeCenterSymbolSize;//大小
                simpleMarkerSymbol.Outline = true;//显示外框线
                simpleMarkerSymbol.OutlineColor = new RgbColorClass { Red = 255 };//外框线颜色
                simpleMarkerSymbol.OutlineSize = 1;//外框线的宽度

                IElement markerElment = new MarkerElementClass();

                quakeCenterPoint.Project(app.MapControl.Map.SpatialReference);//投影变换
                (markerElment as IElementProperties3).Name = "震源中心";//名称
                markerElment.Geometry = quakeCenterPoint;
                (markerElment as IMarkerElement).Symbol = simpleMarkerSymbol;//符号

                groupElement.AddElement(markerElment);
                #endregion

                #region 震中文本
                TextElementClass centerTextElement = new TextElementClass() { FontName = "宋体", Size = quakeCenterFontSize, Color = quakeCenterFontColor, 
                    HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft, VerticalAlignment = esriTextVerticalAlignment.esriTVATop};
                centerTextElement.Text = string.Format("{0}\r\n{1}\r\n{2}", quakeCenterCoordText, quakeLevelText, quakeDepthText);
                IPoint annoPoint = new PointClass()
                {
                    X = quakeCenterPoint.X + (quakeCenterSymbolSize / 2.83) * mapScale * 0.001,
                    Y = quakeCenterPoint.Y - (quakeCenterSymbolSize / 2.83) * mapScale * 0.001
                };
                centerTextElement.Geometry = annoPoint;

                groupElement.AddElement(centerTextElement);
                #endregion

                #region 地震环形及文本
                IPolyline directLine = new PolylineClass();//
                (directLine as IPointCollection).AddPoint(quakeCenterPoint);
                (directLine as IPointCollection).AddPoint(new PointClass() { X = quakeCenterPoint.X + 1, Y = quakeCenterPoint.Y + 1 });//该点距离中心点水平夹角45度的位置
                double rotateAngle = -Math.PI * 0.25;//-45度

                for (int i = 1; i < quakeRingCount + 1; i++)
                {
                    double distance = quakeRingDistance * 1000 * i;//千米转米

                    //环
                    ITopologicalOperator topo = quakeCenterPoint as ITopologicalOperator;
                    IGeometry geometry = topo.Buffer(distance);

                    ISimpleLineSymbol ringSymbol = new SimpleLineSymbolClass();
                    ringSymbol.Color = new RgbColorClass { Red = 255 };//颜色
                    ringSymbol.Width = quakeRingLineWidth;

                    LineElementClass ringElement = new LineElementClass();
                    ringElement.Geometry = (geometry as ITopologicalOperator).Boundary;
                    ringElement.Symbol = ringSymbol;
                    groupElement.AddElement(ringElement);

                    //环注记
                    IPoint outPoint = new PointClass();//获取注记位置点
                    directLine.QueryPoint(esriSegmentExtension.esriExtendAtTo, distance, false, outPoint);
                    TextElementClass ringTextElement = new TextElementClass() { FontName = "宋体", Size = quakeRingFontSize, Color = quakeRingFontColor, 
                        HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter, VerticalAlignment = esriTextVerticalAlignment.esriTVACenter };
                    ringTextElement.Text = string.Format("{0}千米", quakeRingDistance * i);
                    ringTextElement.Geometry = outPoint;

                    IBalloonCallout balloonCallout = new BalloonCalloutClass();
                    balloonCallout.Style = esriBalloonCalloutStyle.esriBCSRectangle;
                    IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                    ILineSymbol lineSymbol = new SimpleLineSymbolClass();
                    lineSymbol.Width = 0;
                    fillSymbol.Outline = lineSymbol;
                    fillSymbol.Color = new RgbColorClass() { Red = 255, Green = 255, Blue = 255 };
                    balloonCallout.Symbol = fillSymbol;
                    ringTextElement.Background = balloonCallout as ITextBackground;

                    if (rotateAngle != 0)
                    {
                        ITransform2D pTransform2D = ringTextElement as ITransform2D;
                        pTransform2D.Rotate(outPoint as IPoint, rotateAngle);
                    }
                    groupElement.AddElement(ringTextElement);
                }
                #endregion

                return groupElement;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                return null;
            }
        }

        public static IGroupElement getEarthQuakeGroupElement(IGraphicsContainer graphicsContainer, string elementName)
        {
            if (graphicsContainer == null)
                return null;

            graphicsContainer.Reset();
            IElement element = graphicsContainer.Next();
            while (element != null)
            {
                if (element is IGroupElement)
                {
                    IGroupElement ge = element as IGroupElement;
                    IElementProperties3 eleProp = ge as IElementProperties3;
                    if (elementName == eleProp.Name)
                    {
                        return ge;
                    }
                }

                element = graphicsContainer.Next();
            }

            return null;

        }

        public static void DeleteEarthQuakeGroupElement(IGraphicsContainer graphicsContainer, string elementName)
        {
            IGroupElement ge = getEarthQuakeGroupElement(graphicsContainer, elementName);
            if (ge != null)
            {
                //清理过时的元素
                graphicsContainer.DeleteElement(ge as IElement);
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using stdole;
using System.Runtime.Serialization.Json;
using System.Data;
using System.Windows.Forms;
using SMGI.Common;
using System.Xml.Linq;
using SMGI.Plugin.EmergencyMap.LabelSym;
namespace SMGI.Plugin.EmergencyMap
{
    [DataContract]
    public class LabelJson
    {
        [DataMember(Order = 0, IsRequired = true)]
        public string LabelType;
        [DataMember(Order = 1)]
        public string AnchorType;
        [DataMember(Order = 2)]
        public double AnchorSize;
        [DataMember(Order = 3)]
        public string TextType;
        [DataMember(Order = 4)]
        public string FillColor;
        [DataMember(Order = 5)]
        public string LineColor;
        [DataMember(Order = 6)]
        public string LineType;
        [DataMember(Order = 7)]
        public double ConnectLens;       
    }

    public enum LabelType
    {
        /// <summary>
        /// 图形文本标注
        /// </summary>
        ConnectLine=0,
        /// <summary>
        /// 常规线
        /// </summary>
        NormalLine=1,
        /// <summary>
        /// 汽包标注
        /// </summary>
        BallCallout=2,
        /// <summary>
        /// 符号标注
        /// </summary>
        SymbolLine = 3,
        /// <summary>
        /// 属性标注
        /// </summary>
        AttrLabel = 4
    }
    public enum LabelState
    {
        Create = 0,
        Update = 1
    }
    public class AttrLabelInfos
    {
        public string TextDown;
        public string TextUp;
        public IPoint AnchorPoint;
    }

    public class LabelClass
    {
        
        private static LabelClass instance = null;
        public Dictionary<string, string> AnchorName = new Dictionary<string, string>();
        public Dictionary<string, string> ExtentName = new Dictionary<string, string>();
        public Dictionary<string, string> LineStyle = new Dictionary<string, string>();
        public Dictionary<string, string> FillStyle = new Dictionary<string, string>();
        IWorkspaceFactory GDBFactory = new FileGDBWorkspaceFactoryClass();
        Dictionary<string, IPolygon> geoDic = new Dictionary<string, IPolygon>();
        Dictionary<string, IPolygon> geoTextDic = new Dictionary<string, IPolygon>();

        public void ActiveDefaultLayer()
        {
            try
            {
                IGraphicsLayer basicLayer = GApplication.Application.ActiveView.FocusMap.BasicGraphicsLayer;

                ICompositeGraphicsLayer cgLayer = basicLayer as ICompositeGraphicsLayer;
                ICompositeLayer comLayer = cgLayer as ICompositeLayer;
                IGraphicsLayer sublayer = null;
                if (comLayer.Count > 0)
                {
                    sublayer = cgLayer.FindLayer("Default");//查找CompositeGraphicsLayer中有没有名为SubLayerName的GraphicsSubLayer  
                }
                if (sublayer == null)
                {
                    sublayer = cgLayer.AddLayer("Default", null);//ICompositeGraphicsLayer.AddLayer方法其实返回的是一个GraphicsSubLayer的实例对象  
                }
                GApplication.Application.ActiveView.FocusMap.ActiveGraphicsLayer = sublayer as ILayer;
            }
            catch
            {
            }
        }

        public IGraphicsContainer GraphicsLayer
        {
            get
            {
                IGraphicsLayer basicLayer = GApplication.Application.ActiveView.FocusMap.BasicGraphicsLayer;

                ICompositeGraphicsLayer cgLayer = basicLayer as ICompositeGraphicsLayer;
                ICompositeLayer comLayer = cgLayer as ICompositeLayer;
                IGraphicsLayer sublayer = null;
                if (comLayer.Count > 0)
                {
                    try
                    {
                        sublayer = cgLayer.FindLayer("Thematic");//查找CompositeGraphicsLayer中有没有名为SubLayerName的GraphicsSubLayer  
                    }
                    catch
                    {
                    }
                }
                if (sublayer == null)
                {
                    sublayer = cgLayer.AddLayer("Thematic", null);//ICompositeGraphicsLayer.AddLayer方法其实返回的是一个GraphicsSubLayer的实例对象  
                }
                GApplication.Application.ActiveView.FocusMap.ActiveGraphicsLayer = sublayer as ILayer; 
                return sublayer as IGraphicsContainer;
                 
            }
        }
      


        public LabelClass()
        {
            string mdbPath = GApplication.Application.Template.Root + @"\专家库\标注引线\引线规则.mdb";
            FillStyle["esriSFSSolid"] = "实心";
            FillStyle["esriSFSNull"] = "空心";
            DataTable dt = ReadToDataTable(mdbPath, "锚点类型");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["LabelName"].ToString();
                string chsname = dt.Rows[i]["AliaName"].ToString();
                AnchorName[name] = chsname;
                
            }
            dt = ReadToDataTable(mdbPath, "文本框类型");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["LabelName"].ToString();
                string chsname = dt.Rows[i]["AliaName"].ToString();
                ExtentName[name] = chsname;
                 
            }
            dt = ReadToDataTable(mdbPath, "边框线型");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["LabelName"].ToString();
                string chsname = dt.Rows[i]["AliaName"].ToString();
                LineStyle[name] = chsname;
                
            }
           
           


            IWorkspace ws = GDBFactory.OpenFromFile(GApplication.Application.Template.Root + @"\专家库\标注引线\LableRule.gdb", 0);

            IFeature fe;
            IFeatureClass fc = (ws as IFeatureWorkspace).OpenFeatureClass("LableExtent");
            IFeatureCursor cursor = fc.Search(null, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                string type = fe.get_Value(fc.FindField("TYPE")).ToString();
                var dic=  ExtentName.Where(t => t.Value == type).ToDictionary(p => p.Key, p => p.Value);
                geoDic[dic.First().Key] = fe.ShapeCopy as IPolygon;
            }
            fc = (ws as IFeatureWorkspace).OpenFeatureClass("TextExtent");
            cursor = fc.Search(null, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                string type = fe.get_Value(fc.FindField("TYPE")).ToString();
                var dic = ExtentName.Where(t => t.Value == type).ToDictionary(p => p.Key, p => p.Value);
                geoTextDic[dic.First().Key] = fe.ShapeCopy as IPolygon;
                
            }

            Marshal.ReleaseComObject(cursor);
            act = GApplication.Application.ActiveView.FocusMap as IActiveView;
            gc = GraphicsLayer as IGraphicsContainer;

        }
        public static LabelClass Instance
        {
          get
          {
              if (instance == null)
                  instance = new LabelClass();
              return instance;
          }
        

        }
        LabelState ls = LabelState.Create;
        LabelType lbType = LabelType.ConnectLine;
        public bool POIState = false;
        public bool AttrState = false;//属性关联
        public DataTable LabAttrDt;
        public DataRow LabelRule = null;
        #region 绘制标注参数

        //几何。元素
        IElement anchorEle = null;
        IGeometry anchorGeo = null;
        ILineElement line1 = null;
        IGeometry geoLine1 = null;
        ILineElement line2 = null;
        IGeometry geoLine2 = null;
        IPolygonElement polygon = null;
        IGeometry geoPolygon = null;
        ITextElement txtElement = null;
        IPoint txtGeometry = null;

        ITextElement txtElementDown = null;//下标文字
        IPoint txtGeometryDown = null;//下标注文字几何

        ILineElement sepline = null;
        IGeometry geosepLine = null;

        IActiveView act = null;
        IGraphicsContainer gc = null;
        IPoint center = null;
        double extentWidth = 200;
        double extentHeight = 35;

        //文本符号信息
        ICmykColor textColor = null;
        string fontName;
        double fontsize;
        bool fontBold = false;
        bool fontItalic = false;
        bool fontunderLine = false;
        string fontStyle = "居中";
        //上下文本间距
        double txtInterval = 0;
        //文本符号信息下标
        ICmykColor textColorDown = null;
        string fontNameDown;
        double fontsizeDown;
        bool fontBoldDown = false;
        bool fontItalicDown = false;
        bool fontunderLineDown = false;
        string fontStyleDown = "居中";


        //背景符号信息
        ICmykColor fillColor = null;
        ICmykColor fillLineColor = null;
        esriSimpleLineStyle fillLineStyle = esriSimpleLineStyle.esriSLSSolid;
        string lableGeometry = "Cloud";//文本框类型
        double fillLinewidth = 1;
        esriSimpleFillStyle fillStyle = esriSimpleFillStyle.esriSFSSolid;
        //锚点符号信息
        ICmykColor anchorLineColor = null;
        ICmykColor anchorFillColor = null;
        esriSimpleFillStyle anchorFillStyle = esriSimpleFillStyle.esriSFSSolid;
        esriSimpleLineStyle anchorLineStyle = esriSimpleLineStyle.esriSLSSolid;
        double anchorSize;
        double anchorsizeMM = 0;
        string anchorStyle = "Circle";//锚点类型
        double anchorLinewidth = 1;
        //连接线符号信息
        ICmykColor lineColor = null;
        esriSimpleLineStyle lineStyle = esriSimpleLineStyle.esriSLSSolid;
        double labelLineLens;
        double labelLineLensMM;
        double linewidth = 1;
        //分割线符号信息
        ICmykColor sepLineColor = null;
        esriSimpleLineStyle sepLineStyle = esriSimpleLineStyle.esriSLSSolid;
        double sepLinewidth = 1;
        double sepLineInterval = 0.5;
        double sepLineIntervalMM = 0.5;
        bool checkSep = false;
      
     
       
        #endregion
        public System.Drawing.FontStyle GetFontStlye(ITextElement ele)
        {
            System.Drawing.FontStyle fs = System.Drawing.FontStyle.Regular;
            if ((ele as ISymbolCollectionElement).Bold)
            {
                fs = System.Drawing.FontStyle.Bold;
            }
            if ((ele as ISymbolCollectionElement).Italic)
            {
                if (fs == System.Drawing.FontStyle.Bold)
                {
                    fs = System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic;
                }
                else
                {
                    fs = System.Drawing.FontStyle.Italic;
                }
            }
            return fs;
        }

        public void UpdateLabelElement(string text, IPoint ct)
        {

            gc = GraphicsLayer as IGraphicsContainer;
            try
            {

                this.ls = LabelState.Update;
                #region 获取参数
                string fileName = GApplication.Application.Template.Root + @"\专家库\标注引线\默认配置.xml";
                if (!File.Exists(fileName))
                {
                    return;
                }
                XDocument doc = XDocument.Load(fileName);
                //只有主区，没附区
                var root = doc.Element("Template").Element("LabelText");
                #region

                //文本
                XElement ele = root.Element("Text");
                fontName = ele.Element("Font").Attribute("FontName").Value;
                fontsize = double.Parse(ele.Element("Font").Attribute("FontSize").Value);
                string colorName = ele.Element("Font").Attribute("FontColor").Value;
                System.Drawing.Color cc = ColorHelper.GetColorByCmykStr(ele.Element("Font").Attribute("FontColor").Value);
                textColor = ColorHelper.ConvertColorToCMYK(cc);
                fontBold = bool.Parse(ele.Element("Font").Attribute("FontBold").Value);
                fontItalic = bool.Parse(ele.Element("Font").Attribute("FontItalic").Value);
                //锚点
                ele = root.Element("Anchor");
                anchorStyle = (ele.Element("AnchorType").Value);
                var dic = AnchorName.Where(t => t.Value == anchorStyle).ToDictionary(p => p.Key, p => p.Value);
                anchorStyle = dic.First().Key;
                anchorSize = double.Parse(ele.Element("AnchorSize").Value);
                anchorsizeMM = anchorSize;
                anchorSize = act.FocusMap.ReferenceScale * anchorSize * 0.001;
                anchorLineColor = ColorHelper.GetColorByString(ele.Element("AnchorLineColor").Value); ;
                anchorFillColor = ColorHelper.GetColorByString(ele.Element("AnchorFillColor").Value); ;
                dic = LineStyle.Where(t => t.Value == ele.Element("AnchorLineStyle").Value).ToDictionary(p => p.Key, p => p.Value);
                anchorLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
               // dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
                anchorFillStyle = esriSimpleFillStyle.esriSFSSolid;
                anchorLinewidth = double.Parse(ele.Element("AnchorLineWidth").Value);
                //背景
                ele = root.Element("Content");
                lableGeometry = (ele.Element("TextType").Value);
                dic = ExtentName.Where(t => t.Value == lableGeometry).ToDictionary(p => p.Key, p => p.Value);
                lableGeometry = dic.First().Key;
                fillColor = ColorHelper.GetColorByString(ele.Element("FillColor").Value);
                fillLineColor = ColorHelper.GetColorByString(ele.Element("TextLineColor").Value);
                dic = LineStyle.Where(t => t.Value == ele.Element("TextLineType").Value).ToDictionary(p => p.Key, p => p.Value);
                fillLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                fillLinewidth = double.Parse(ele.Element("TextLineWidth").Value);
                dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
                fillStyle = (esriSimpleFillStyle)System.Enum.Parse(typeof(esriSimpleFillStyle), dic.First().Key);

                //连接线
                ele = root.Element("ConnectLine");
                string ls = ele.Element("LineType").Value;
                dic = LineStyle.Where(t => t.Value == ls).ToDictionary(p => p.Key, p => p.Value);
                lineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                linewidth = double.Parse(ele.Element("LineWidth").Value);
                lineColor = ColorHelper.GetColorByString(ele.Element("LineColor").Value);
                labelLineLens = double.Parse(ele.Element("ConnectLens").Value);
                labelLineLensMM = labelLineLens;
                labelLineLens = act.FocusMap.ReferenceScale * labelLineLens * 0.001;
                //分割线
                ele = root.Element("SepLine");
                sepLineColor = ColorHelper.GetColorByString(ele.Element("SepLineColor").Value); ;
                dic = LineStyle.Where(t => t.Value == ele.Element("SepLineType").Value).ToDictionary(p => p.Key, p => p.Value);
                sepLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                sepLinewidth = double.Parse(ele.Element("SepLineWidth").Value); ;
                sepLineInterval = double.Parse(ele.Element("SepInterval").Value); ; ;
                checkSep = bool.Parse(ele.Attribute("check").Value);

                #endregion
                #endregion
                double anchorDx = 0;
                center = ct;
                switch (anchorStyle)
                {
                    case "Circle":
                        CreateCircle(ct, anchorSize * 0.5);
                        anchorDx = anchorSize * 0.5;
                        break;
                    case "Trangle":
                        CreateTrangle(ct, anchorSize);
                        anchorDx = anchorSize * Math.Pow(3, 0.5) * 0.5;
                        break;
                    case "TrangleLine":
                        CreateTrangleLine(ct, anchorSize);
                        break;
                    case "HoraLine":
                        CreateHoralLine(ct, anchorSize);
                        break;
                    case "Rectangle":
                        CreateRectangle(ct, anchorSize);
                        anchorDx = anchorSize * 0.5;
                        break;
                    case "None":
                        CreateCircle(ct, anchorSize * 0.5);
                        anchorDx = anchorSize * 0.5;
                        break;
                    default:
                        break;
                }
                anchorDx = 0;
                SimpleLineSymbolClass slsLine = new SimpleLineSymbolClass();
                slsLine.Width = linewidth * 2.83;
                slsLine.Style = lineStyle;
                slsLine.Color = lineColor;

                SimpleLineSymbolClass slsSepLine = new SimpleLineSymbolClass();
                slsSepLine.Width = sepLinewidth * 2.83;
                slsSepLine.Style = sepLineStyle;
                slsSepLine.Color = sepLineColor;

                SimpleFillSymbolClass sfsFill = new SimpleFillSymbolClass();
                sfsFill.Color = fillColor;
                sfsFill.Style = fillStyle;
                sfsFill.Outline = new SimpleLineSymbolClass()
                {
                    Width = fillLinewidth * 2.83,
                    Style = fillLineStyle,
                    Color = fillLineColor
                };

                SimpleFillSymbolClass sfsAnchor = new SimpleFillSymbolClass();
                sfsAnchor.Color = anchorFillColor;
                sfsAnchor.Style = anchorFillStyle;
                sfsAnchor.Outline = new SimpleLineSymbolClass()
                {
                    Width = anchorLinewidth * 2.83,
                    Style = anchorLineStyle,
                    Color = anchorLineColor
                };
                if (anchorStyle == "None")//特殊处理
                {
                    sfsAnchor.Style = esriSimpleFillStyle.esriSFSNull;
                    sfsAnchor.Outline = new SimpleLineSymbolClass()
                    {
                        Width = anchorLinewidth * 2.83,
                        Color = anchorLineColor,
                        Style = esriSimpleLineStyle.esriSLSNull
                    };
                }
                line1 = new LineElementClass();
                line1.Symbol = slsLine;
                PolylineClass polyline = new PolylineClass();
                polyline.AddPoint(new PointClass { X = center.X + anchorDx, Y = center.Y });
                polyline.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                (line1 as IElement).Geometry = polyline;
                geoLine1 = polyline;
                //
                line2 = new LineElementClass();
                line2.Symbol = slsLine;
                PolylineClass polyline1 = new PolylineClass();
                polyline1.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                polyline1.AddPoint(new PointClass { X = center.X + 35 + labelLineLens, Y = center.Y });
                (line2 as IElement).Geometry = polyline1;
                geoLine2 = polyline1;



                //创建文字
                ITextSymbol pTextSymbol = new TextSymbolClass()
                {
                    Color = textColor,
                };
                txtElement = new TextElementClass();
                txtGeometry = ct;
                txtElement.Symbol = pTextSymbol;
                (txtElement as ISymbolCollectionElement).FontName = fontName;
                (txtElement as ISymbolCollectionElement).Size = fontsize;
                (txtElement as ISymbolCollectionElement).Bold = fontBold;
                (txtElement as ISymbolCollectionElement).Italic = fontItalic;
                (txtElement as IElement).Geometry = txtGeometry;
                txtElement.Text = text;
                IPolygon outline = new PolygonClass();
                (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);

                IEnvelope enText = outline.Envelope;
                (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                //if (text.Contains("\r") || text.Contains("\n"))
                //    (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVABottom;

                //创建内容形状
                double orginWidth, orginHeight = 0;
                double orginTxtWidth, orginTxtHeight = 0;
                orginWidth = geoDic[lableGeometry].Envelope.Width;
                orginHeight = geoDic[lableGeometry].Envelope.Height;
                orginTxtWidth = geoTextDic[lableGeometry].Envelope.Width;
                orginTxtHeight = geoTextDic[lableGeometry].Envelope.Height;
                double txtHeight = enText.Height;
                double txtWidth = enText.Width;
                if (txtWidth / txtHeight > orginTxtWidth / orginTxtHeight)
                {
                    extentWidth = txtWidth / orginTxtWidth * orginWidth;
                    extentHeight = txtHeight / orginTxtHeight * orginHeight;
                }
                else
                {
                    if (lableGeometry.ToLower().Contains("cloud"))
                    {
                        extentHeight = txtHeight * orginHeight / orginTxtHeight;
                        // extentWidth = extentHeight / orginHeight * orginWidth;
                        extentWidth = extentHeight / orginHeight * orginWidth;
                    }
                    else
                    {
                        extentHeight = txtHeight / orginTxtHeight * orginHeight;
                        extentWidth = txtWidth / orginTxtWidth * orginWidth;
                    }
                }
             
                CreateLabelExtent();
                //
                sepline = new LineElementClass();
                sepline.Symbol = slsSepLine;
                PolylineClass polyline2 = new PolylineClass();
                sepLineIntervalMM = sepLineInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001;
                polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM, Y = center.Y });
                polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM + geoPolygon.Envelope.Width - 2 * sepLineIntervalMM, Y = center.Y });
                (sepline as IElement).Geometry = polyline2;
                geosepLine = polyline2;

                (polygon as IFillShapeElement).Symbol = sfsFill;
                if (anchorEle != null)
                {
                    if (anchorEle is IFillShapeElement)
                    {
                        (anchorEle as IFillShapeElement).Symbol = sfsAnchor;
                    }
                    if (anchorEle is ILineElement)
                    {
                        (anchorEle as ILineElement).Symbol = sfsAnchor.Outline;
                    }
                }
                #region //更新
                IGraphicsContainer pGraphicsContainer = gc as IGraphicsContainer;
                IGraphicsContainerSelect pGraphicsContainerSelect = gc as IGraphicsContainerSelect;
                //遍历Element
                IEnumElement pEnumElemen = pGraphicsContainerSelect.SelectedElements;
                pEnumElemen.Reset();
                IElement pElement = pEnumElemen.Next();
                var ge = pElement as IGroupElement;
                IElement anchorEle0 = null;
                ILineElement anchorline0 = null;
                ILineElement connectline0 = null;
                IPolygonElement polygon0 = null;
                ITextElement txtElement0 = null;
                ILineElement sepline0 = null;

                //引线标注
                for (int i = 0; i < ge.ElementCount; i++)
                {
                    #region
                    IElement ee = ge.get_Element(i);
                    switch ((ee as IElementProperties).Name)
                    {
                        case "锚点":
                            anchorEle0 = (ee as IClone).Clone() as IElement; ;
                            break;
                        case "锚线":
                            anchorline0 = (ee as IClone).Clone() as ILineElement;
                            break;
                        case "连接线":
                            connectline0 = (ee as IClone).Clone() as ILineElement;
                            break;
                        case "分割线":
                            sepline0 = (ee as IClone).Clone() as ILineElement;
                            break;
                        case "内容框":
                            polygon0 = (ee as IClone).Clone() as IPolygonElement;
                            break;
                        case "文本":
                            txtElement0 = (ee as IClone).Clone() as ITextElement;
                            break;
                        default:
                            break;

                    }
                    #endregion
                }
                //锚点
                ILine line = new LineClass();
                line.FromPoint = ((anchorline0 as IElement).Geometry as IPolyline).FromPoint;
                line.ToPoint = ((anchorline0 as IElement).Geometry as IPolyline).ToPoint;
                ITransform2D rans2dAnchor = anchorGeo as ITransform2D;
                rans2dAnchor.Rotate(center, line.Angle);
                if (anchorEle0.Geometry.GeometryType == anchorGeo.GeometryType)
                {
                    anchorEle0.Geometry = rans2dAnchor as IGeometry;
                }
                else//圆形变成三角线
                {
                    anchorEle0 = anchorEle;
                    (anchorEle0 as IElementProperties).Name = "锚点";
                    (anchorEle0 as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                    anchorEle0.Geometry = rans2dAnchor as IGeometry;
                }
                if (anchorEle0 is IFillShapeElement)
                {
                    (anchorEle0 as IFillShapeElement).Symbol = (anchorEle as IFillShapeElement).Symbol;
                }
                else if (anchorEle0 is ILineElement)
                {
                    (anchorEle0 as ILineElement).Symbol = (anchorEle as ILineElement).Symbol;
                }

                //连接线
                (anchorline0 as ILineElement).Symbol = line1.Symbol;//锚点线
                (connectline0 as ILineElement).Symbol = line2.Symbol;//连接线
                double scaleX = ((line2 as IElement).Geometry as IPolyline).Length / ((connectline0 as IElement).Geometry as IPolyline).Length;
                (connectline0 as ITransform2D).Scale(((connectline0 as IElement).Geometry as IPolyline).FromPoint, scaleX, 1);
                //内容
                double dx = ((polygon0 as IElement).Geometry.Envelope as IArea).Centroid.X - ((polygon as IElement).Geometry.Envelope as IArea).Centroid.X;
                double dy = ((polygon0 as IElement).Geometry.Envelope as IArea).Centroid.Y - ((polygon as IElement).Geometry.Envelope as IArea).Centroid.Y;

                bool isCircle = false;
                string extentShp = LabelClass.GetLabelInfo((ge as IElementProperties).Name).TextType;
                if (extentShp == "Circle")
                    isCircle = true;

                double scaleDx = (polygon as IElement).Geometry.Envelope.Width / (polygon0 as IElement).Geometry.Envelope.Width;
                double scaleDy = (polygon as IElement).Geometry.Envelope.Height / (polygon0 as IElement).Geometry.Envelope.Height;
                if (isCircle)
                {
                    scaleDy = 1;
                }

                ITransform2D rans2d = (polygon as IElement).Geometry as ITransform2D;
                rans2d.Move(dx, dy);
                IPoint enCenter = ((polygon0 as IElement).Geometry as IArea).Centroid;
                // rans2d.Scale(enCenter, scaleDx, scaleDy);
                //判断连接线是否边长
                {
                    //旋转内容窗体
                    #region
                    //
                    if (Math.Abs(line.Angle) < Math.PI / 2)
                    {
                        if (Math.Abs(line.Angle) > Math.PI / 2 * 0.8)//72度
                        {
                            double reg = line.Angle / Math.Abs(line.Angle);
                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMax + (rans2d as IGeometry).Envelope.XMin) * 0.5;
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - (reg < 0 ? ((rans2d as IGeometry).Envelope.YMax) : ((rans2d as IGeometry).Envelope.YMin));
                            rans2d.Move(dx, dy);

                        }
                        else
                        {


                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMin);
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - ((rans2d as IGeometry).Envelope.YMin + (rans2d as IGeometry).Envelope.YMax) * 0.5;
                            rans2d.Move(dx, dy);

                        }
                    }
                    else
                    {
                        if (Math.Abs(line.Angle) < Math.PI / 2 + Math.PI / 2 * 0.2)//72度
                        {


                            double reg = line.Angle / Math.Abs(line.Angle);
                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMax + (rans2d as IGeometry).Envelope.XMin) * 0.5;
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - (reg < 0 ? ((rans2d as IGeometry).Envelope.YMax) : ((rans2d as IGeometry).Envelope.YMin));
                            rans2d.Move(dx, dy);

                        }
                        else
                        {
                            //2 
                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMin) - (rans2d as IGeometry).Envelope.Width;
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - ((rans2d as IGeometry).Envelope.YMin + (rans2d as IGeometry).Envelope.YMax) * 0.5;
                            rans2d.Move(dx, dy);
                        }
                    }
                    #endregion
                }
                (polygon0 as IFillShapeElement).Symbol = (polygon as IFillShapeElement).Symbol;
                (polygon0 as IElement).Geometry = rans2d as IGeometry;
                //文本
                (txtElement0 as ITextElement).Symbol = (txtElement as ITextElement).Symbol;
                (txtElement0 as ISymbolCollectionElement).FontName = (txtElement as ISymbolCollectionElement).FontName;
                (txtElement0 as ISymbolCollectionElement).Size = (txtElement as ISymbolCollectionElement).Size;
                (txtElement0 as ITextElement).Text = (txtElement as ITextElement).Text;
                IPoint lineCt = new PointClass();
                if (checkSep)
                {
                    ((sepline as IElement).Geometry as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);
                    dx = ((polygon0 as IElement).Geometry as IArea).Centroid.X - lineCt.X;
                    dy = ((polygon0 as IElement).Geometry as IArea).Centroid.Y - lineCt.Y;
                    (sepline as ITransform2D).Move(dx, dy);
                    sepline0 = (sepline as IClone).Clone() as ILineElement;
                    (sepline0 as IElementProperties).Name = "分割线";

                }
                dx = ((polygon0 as IElement).Geometry as IArea).Centroid.X - ((txtElement0 as IElement).Geometry as IPoint).X;
                dy = ((polygon0 as IElement).Geometry as IArea).Centroid.Y - ((txtElement0 as IElement).Geometry as IPoint).Y;
                (txtElement0 as ITransform2D).Move(dx, dy);
                if ((txtElement0 as ISymbolCollectionElement).VerticalAlignment == esriTextVerticalAlignment.esriTVACenter)
                {
                    //纠正中心点位置
                    IPoint tCenter=((polygon0 as IElement).Geometry as IArea).Centroid;
                     outline = new PolygonClass();
                    (txtElement0 as IElement).QueryOutline(act.ScreenDisplay, outline);
                    IPoint txtCenter = (outline as IArea).Centroid;
                    (txtElement0 as ITransform2D).Move(tCenter.X - txtCenter.X, tCenter.Y - txtCenter.Y);
                }
                #endregion

                #region 标注信息
                LabelJson json = new LabelJson
                {
                    AnchorSize = anchorsizeMM,
                    AnchorType = anchorStyle,
                    ConnectLens = labelLineLensMM,
                    TextType = lableGeometry

                };

                #endregion
               
                string jsonText = LabelClass.GetJsonText(json);
                string type = (ge as IElementProperties).Type;
                ge.ClearElements();
                gc.DeleteElement(ge as IElement);
                ge = new GroupElementClass();
                (ge as IElementProperties).Name = jsonText;
                (ge as IElementProperties).Type = type;
                gc.AddElement(anchorline0 as IElement, 0);
                gc.AddElement(connectline0 as IElement, 0);
                gc.AddElement(anchorEle0 as IElement, 0); ;
                gc.AddElement(polygon0 as IElement, 0);
                gc.AddElement(txtElement0 as IElement, 0);


                gc.MoveElementToGroup(anchorline0 as IElement, ge);
                gc.MoveElementToGroup(connectline0 as IElement, ge);
                gc.MoveElementToGroup(anchorEle0 as IElement, ge);
                gc.MoveElementToGroup(polygon0 as IElement, ge);
                gc.MoveElementToGroup(txtElement0 as IElement, ge);
                //ge.AddElement(anchorline0 as IElement);
                //ge.AddElement(connectline0 as IElement);
                //ge.AddElement(anchorEle0);
                //ge.AddElement(polygon0 as IElement);
                //ge.AddElement(txtElement0 as IElement);

                if (checkSep)
                {
                    gc.AddElement(sepline0 as IElement, 0);
                    gc.MoveElementToGroup(sepline0 as IElement, ge);
                    // ge.AddElement(sepline0 as IElement);
                }


                gc.AddElement(ge as IElement, 0);
                pGraphicsContainerSelect.SelectElement(ge as IElement);
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }



        }
        public void UpdatePolylineElement(string text,string textdown, IPoint ct)
        {

            gc = GraphicsLayer as IGraphicsContainer;
            try
            {

                this.ls = LabelState.Update;
                #region 获取参数
                string fileName = GApplication.Application.Template.Root + @"\专家库\标注引线\折线配置.xml";
                if (!File.Exists(fileName))
                {
                    return;
                }
                XDocument doc = XDocument.Load(fileName);
                //只有主区，没附区
                var root = doc.Element("Template").Element("LabelText");
                #region

                //文本
                XElement ele = root.Element("Text");
                fontName = ele.Element("Font").Attribute("FontName").Value;
                fontsize = double.Parse(ele.Element("Font").Attribute("FontSize").Value);
                string colorName = ele.Element("Font").Attribute("FontColor").Value;
                System.Drawing.Color cc = ColorHelper.GetColorByCmykStr(ele.Element("Font").Attribute("FontColor").Value);
                textColor = ColorHelper.ConvertColorToCMYK(cc);
                txtInterval = double.Parse(ele.Value);
                fontBold = bool.Parse(ele.Element("Font").Attribute("FontBold").Value);
                fontItalic = bool.Parse(ele.Element("Font").Attribute("FontItalic").Value);
                //文本下标
                ele = root.Element("TextDown");
                fontNameDown = ele.Element("Font").Attribute("FontName").Value;
                fontsizeDown = double.Parse(ele.Element("Font").Attribute("FontSize").Value);
                colorName = ele.Element("Font").Attribute("FontColor").Value;
                cc = ColorHelper.GetColorByCmykStr(ele.Element("Font").Attribute("FontColor").Value);
                textColorDown = ColorHelper.ConvertColorToCMYK(cc);
                fontBoldDown = bool.Parse(ele.Element("Font").Attribute("FontBold").Value);
                fontItalicDown = bool.Parse(ele.Element("Font").Attribute("FontItalic").Value);
                //锚点
                ele = root.Element("Anchor");
                anchorStyle = (ele.Element("AnchorType").Value);
                var dic = AnchorName.Where(t => t.Value == anchorStyle).ToDictionary(p => p.Key, p => p.Value);
                anchorStyle = dic.First().Key;
                anchorSize = double.Parse(ele.Element("AnchorSize").Value);
                anchorsizeMM = anchorSize;
                anchorSize = act.FocusMap.ReferenceScale * anchorSize * 0.001;
                anchorLineColor = ColorHelper.GetColorByString(ele.Element("AnchorLineColor").Value); ;
                anchorFillColor = ColorHelper.GetColorByString(ele.Element("AnchorFillColor").Value); ;
                dic = LineStyle.Where(t => t.Value == ele.Element("AnchorLineStyle").Value).ToDictionary(p => p.Key, p => p.Value);
                anchorLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                //dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
                anchorFillStyle = (esriSimpleFillStyle.esriSFSSolid);
                anchorLinewidth = double.Parse(ele.Element("AnchorLineWidth").Value);
                //背景
                ele = root.Element("Content");
                lableGeometry = (ele.Element("TextType").Value);
                dic = ExtentName.Where(t => t.Value == lableGeometry).ToDictionary(p => p.Key, p => p.Value);
                lableGeometry = dic.First().Key;
                fillColor = ColorHelper.GetColorByString(ele.Element("FillColor").Value);
                fillLineColor = ColorHelper.GetColorByString(ele.Element("TextLineColor").Value);
                dic = LineStyle.Where(t => t.Value == ele.Element("TextLineType").Value).ToDictionary(p => p.Key, p => p.Value);
                fillLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                fillLinewidth = double.Parse(ele.Element("TextLineWidth").Value);
                dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
                fillStyle = (esriSimpleFillStyle)System.Enum.Parse(typeof(esriSimpleFillStyle), dic.First().Key);

                //连接线
                ele = root.Element("ConnectLine");
                string ls = ele.Element("LineType").Value;
                dic = LineStyle.Where(t => t.Value == ls).ToDictionary(p => p.Key, p => p.Value);
                lineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                linewidth = double.Parse(ele.Element("LineWidth").Value);
                lineColor = ColorHelper.GetColorByString(ele.Element("LineColor").Value);
                labelLineLens = double.Parse(ele.Element("ConnectLens").Value);
                labelLineLensMM = labelLineLens;
                labelLineLens = act.FocusMap.ReferenceScale * labelLineLens * 0.001;
                //分割线
                ele = root.Element("SepLine");
                sepLineColor = ColorHelper.GetColorByString(ele.Element("SepLineColor").Value); ;
                dic = LineStyle.Where(t => t.Value == ele.Element("SepLineType").Value).ToDictionary(p => p.Key, p => p.Value);
                sepLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                sepLinewidth = double.Parse(ele.Element("SepLineWidth").Value); ;
                sepLineInterval = double.Parse(ele.Element("SepInterval").Value); ; ;
                checkSep = bool.Parse(ele.Attribute("check").Value);
                checkSep = true;
                #endregion
                #endregion
                double anchorDx = 0;
                center = ct;
                switch (anchorStyle)
                {
                    case "Circle":
                        CreateCircle(ct, anchorSize * 0.5);
                        anchorDx = anchorSize * 0.5;
                        break;
                    case "Trangle":
                        CreateTrangle(ct, anchorSize);
                        anchorDx = anchorSize * Math.Pow(3, 0.5) * 0.5;
                        break;
                    case "TrangleLine":
                        CreateTrangleLine(ct, anchorSize);
                        break;
                    case "HoraLine":
                        CreateHoralLine(ct, anchorSize);
                        break;
                    case "Rectangle":
                        CreateRectangle(ct, anchorSize);
                        anchorDx = anchorSize * 0.5;
                        break;
                    case "None":
                        CreateCircle(ct, anchorSize * 0.5);
                        anchorDx = anchorSize * 0.5;
                        break;
                    default:
                        break;
                }
                anchorDx = 0;
                SimpleLineSymbolClass slsLine = new SimpleLineSymbolClass();
                slsLine.Width = linewidth * 2.83;
                slsLine.Style = lineStyle;
                slsLine.Color = lineColor;

                SimpleLineSymbolClass slsSepLine = new SimpleLineSymbolClass();
                slsSepLine.Width = sepLinewidth * 2.83;
                slsSepLine.Style = sepLineStyle;
                slsSepLine.Color = sepLineColor;

                SimpleFillSymbolClass sfsFill = new SimpleFillSymbolClass();
                sfsFill.Color = fillColor;
                sfsFill.Style = esriSimpleFillStyle.esriSFSNull;
                sfsFill.Outline = new SimpleLineSymbolClass()
                {
                    Width = fillLinewidth * 2.83,
                    Style = esriSimpleLineStyle.esriSLSNull,
                    Color = fillLineColor
                };

                SimpleFillSymbolClass sfsAnchor = new SimpleFillSymbolClass();
                sfsAnchor.Color = anchorFillColor;
                sfsAnchor.Style = anchorFillStyle;
                sfsAnchor.Outline = new SimpleLineSymbolClass()
                {
                    Width = anchorLinewidth * 2.83,
                    Style = anchorLineStyle,
                    Color = anchorLineColor
                };
                if (anchorStyle == "None")//特殊处理
                {
                    sfsAnchor.Style = esriSimpleFillStyle.esriSFSNull;
                    sfsAnchor.Outline = new SimpleLineSymbolClass()
                    {
                        Width = anchorLinewidth * 2.83,
                        Color = anchorLineColor,
                        Style = esriSimpleLineStyle.esriSLSNull
                    };
                }
                line1 = new LineElementClass();
                line1.Symbol = slsLine;
                PolylineClass polyline = new PolylineClass();
                polyline.AddPoint(new PointClass { X = center.X + anchorDx, Y = center.Y });
                polyline.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                (line1 as IElement).Geometry = polyline;
                geoLine1 = polyline;
                //
                line2 = new LineElementClass();
                line2.Symbol = slsLine;
                PolylineClass polyline1 = new PolylineClass();
                polyline1.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                polyline1.AddPoint(new PointClass { X = center.X + 35 + labelLineLens, Y = center.Y });
                (line2 as IElement).Geometry = polyline1;
                geoLine2 = polyline1;


                //创建文字
                double txtStep = txtInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001;
                ITextSymbol pTextSymbol = new TextSymbolClass()
                {
                    Color = textColor,
                };
                txtElement = new TextElementClass();
                txtElement.Symbol = pTextSymbol;
                (txtElement as ISymbolCollectionElement).FontName = fontName;
                (txtElement as ISymbolCollectionElement).Size = fontsize;
                (txtElement as ISymbolCollectionElement).Bold = fontBold;
                (txtElement as ISymbolCollectionElement).Italic = fontItalic;
                (txtElement as IElement).Geometry = new PointClass { X = ct.X, Y = ct.Y + txtStep };
                txtElement.Text = text;
                IPolygon outline = new PolygonClass();
                (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);

                IEnvelope enText = outline.Envelope;
                (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVABottom;
                (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                double txtHeight = enText.Height;
                double txtWidth = enText.Width;
                //下标文字
                if (textdown != string.Empty)
                {
                    txtElementDown = new TextElementClass();

                    txtElementDown.Symbol = new TextSymbolClass()
                    {
                        Color = textColorDown,
                    }; ;
                    (txtElementDown as ISymbolCollectionElement).FontName = fontNameDown;
                    (txtElementDown as ISymbolCollectionElement).Size = fontsizeDown;
                    (txtElementDown as IElement).Geometry = new PointClass { X = ct.X, Y = ct.Y - txtStep };
                    (txtElementDown as ISymbolCollectionElement).Bold = fontBoldDown;
                    (txtElementDown as ISymbolCollectionElement).Italic = fontItalicDown;

                    txtElementDown.Text = textdown;
                    IPolygon outline1 = new PolygonClass();
                    (txtElementDown as IElement).QueryOutline(act.ScreenDisplay, outline1);
                    (txtElementDown as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVATop;
                    (txtElementDown as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                    IEnvelope enText1 = outline1.Envelope;
                    if (txtWidth < enText1.Width)
                    {
                        txtWidth = enText1.Width;
                    }
                }
                //创建内容形状
                double orginWidth, orginHeight = 0;
                double orginTxtWidth, orginTxtHeight = 0;
                orginWidth = geoDic[lableGeometry].Envelope.Width;
                orginHeight = geoDic[lableGeometry].Envelope.Height;
                orginTxtWidth = geoTextDic[lableGeometry].Envelope.Width;
                orginTxtHeight = geoTextDic[lableGeometry].Envelope.Height;

                if (txtWidth / txtHeight > orginTxtWidth / orginTxtHeight)
                {
                    extentWidth = txtWidth / orginTxtWidth * orginWidth;
                    extentHeight = txtHeight / orginTxtHeight * orginHeight;
                }
                else
                {

                    extentHeight = txtHeight / orginTxtHeight * orginHeight;
                    extentWidth = txtWidth / orginTxtWidth * orginWidth;

                }

                CreateLabelExtent();
                //
                sepline = new LineElementClass();
                sepline.Symbol = slsLine;
                PolylineClass polyline2 = new PolylineClass();
                sepLineIntervalMM = 0;
                polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM, Y = center.Y });
                polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM + geoPolygon.Envelope.Width - 2 * sepLineIntervalMM, Y = center.Y });
                (sepline as IElement).Geometry = polyline2;
                geosepLine = polyline2;

                (polygon as IFillShapeElement).Symbol = sfsFill;
                if (anchorEle != null)
                {
                    if (anchorEle is IFillShapeElement)
                    {
                        (anchorEle as IFillShapeElement).Symbol = sfsAnchor;
                    }
                    if (anchorEle is ILineElement)
                    {
                        (anchorEle as ILineElement).Symbol = sfsAnchor.Outline;
                    }
                }
                #region //更新
                IGraphicsContainer pGraphicsContainer = gc as IGraphicsContainer;
                IGraphicsContainerSelect pGraphicsContainerSelect = gc as IGraphicsContainerSelect;
                //遍历Element
                IEnumElement pEnumElemen = pGraphicsContainerSelect.SelectedElements;
                pEnumElemen.Reset();
                IElement pElement = pEnumElemen.Next();
                var ge = pElement as IGroupElement;
                IElement anchorEle0 = null;
                ILineElement anchorline0 = null;
                ILineElement connectline0 = null;
                IPolygonElement polygon0 = null;
                ITextElement txtElement0 = null;
                ITextElement txtElementDown0 = null;
                ILineElement sepline0 = null;

                //引线标注
                for (int i = 0; i < ge.ElementCount; i++)
                {
                    #region
                    IElement ee = ge.get_Element(i);
                    switch ((ee as IElementProperties).Name)
                    {
                        case "锚点":
                            anchorEle0 = (ee as IClone).Clone() as IElement; ;
                            break;
                        case "锚线":
                            anchorline0 = (ee as IClone).Clone() as ILineElement;
                            break;
                        case "连接线":
                            connectline0 = (ee as IClone).Clone() as ILineElement;
                            break;
                        case "分割线":
                            sepline0 = (ee as IClone).Clone() as ILineElement;
                            break;
                        case "内容框":
                            polygon0 = (ee as IClone).Clone() as IPolygonElement;
                            break;
                        case "文本":
                            txtElement0 = (ee as IClone).Clone() as ITextElement;
                            break;
                        case "下标文本":
                            txtElementDown0 = (ee as IClone).Clone() as ITextElement;
                            break;
                        default:
                            break;

                    }
                    #endregion
                }
                //锚点
                ILine line = new LineClass();
                line.FromPoint = ((anchorline0 as IElement).Geometry as IPolyline).FromPoint;
                line.ToPoint = ((anchorline0 as IElement).Geometry as IPolyline).ToPoint;
                ITransform2D rans2dAnchor = anchorGeo as ITransform2D;
                rans2dAnchor.Rotate(center, line.Angle);
                if (anchorEle0.Geometry.GeometryType == anchorGeo.GeometryType)
                {
                    anchorEle0.Geometry = rans2dAnchor as IGeometry;
                }
                else//圆形变成三角线
                {
                    anchorEle0 = anchorEle;
                    (anchorEle0 as IElementProperties).Name = "锚点";
                    anchorEle0.Geometry = rans2dAnchor as IGeometry;
                }
                if (anchorEle0 is IFillShapeElement)
                {
                    (anchorEle0 as IFillShapeElement).Symbol = (anchorEle as IFillShapeElement).Symbol;
                }
                else if (anchorEle0 is ILineElement)
                {
                    (anchorEle0 as ILineElement).Symbol = (anchorEle as ILineElement).Symbol;
                }

                //连接线
                (anchorline0 as ILineElement).Symbol = line1.Symbol;//锚点线
                (connectline0 as ILineElement).Symbol = line2.Symbol;//连接线
                double scaleX = ((line2 as IElement).Geometry as IPolyline).Length / ((connectline0 as IElement).Geometry as IPolyline).Length;
                (connectline0 as ITransform2D).Scale(((connectline0 as IElement).Geometry as IPolyline).FromPoint, scaleX, 1);
                //内容
                double dx = ((polygon0 as IElement).Geometry.Envelope as IArea).Centroid.X - ((polygon as IElement).Geometry.Envelope as IArea).Centroid.X;
                double dy = ((polygon0 as IElement).Geometry.Envelope as IArea).Centroid.Y - ((polygon as IElement).Geometry.Envelope as IArea).Centroid.Y;

                bool isCircle = false;
                string extentShp = LabelClass.GetLabelInfo((ge as IElementProperties).Name).TextType;
                if (extentShp == "Circle")
                    isCircle = true;

                double scaleDx = (polygon as IElement).Geometry.Envelope.Width / (polygon0 as IElement).Geometry.Envelope.Width;
                double scaleDy = (polygon as IElement).Geometry.Envelope.Height / (polygon0 as IElement).Geometry.Envelope.Height;
                if (isCircle)
                {
                    scaleDy = 1;
                }

                ITransform2D rans2d = (polygon as IElement).Geometry as ITransform2D;
                rans2d.Move(dx, dy);
                IPoint enCenter = ((polygon0 as IElement).Geometry as IArea).Centroid;
                // rans2d.Scale(enCenter, scaleDx, scaleDy);
                //判断连接线是否边长
                {
                    //旋转内容窗体
                    #region
                    //
                    if (Math.Abs(line.Angle) < Math.PI / 2)
                    {
                        if (Math.Abs(line.Angle) > Math.PI / 2 * 0.8)//72度
                        {
                            double reg = line.Angle / Math.Abs(line.Angle);
                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMax + (rans2d as IGeometry).Envelope.XMin) * 0.5;
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - (reg < 0 ? ((rans2d as IGeometry).Envelope.YMax) : ((rans2d as IGeometry).Envelope.YMin));
                            rans2d.Move(dx, dy);

                        }
                        else
                        {


                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMin);
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - ((rans2d as IGeometry).Envelope.YMin + (rans2d as IGeometry).Envelope.YMax) * 0.5;
                            rans2d.Move(dx, dy);

                        }
                    }
                    else
                    {
                        if (Math.Abs(line.Angle) < Math.PI / 2 + Math.PI / 2 * 0.2)//72度
                        {


                            double reg = line.Angle / Math.Abs(line.Angle);
                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMax + (rans2d as IGeometry).Envelope.XMin) * 0.5;
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - (reg < 0 ? ((rans2d as IGeometry).Envelope.YMax) : ((rans2d as IGeometry).Envelope.YMin));
                            rans2d.Move(dx, dy);

                        }
                        else
                        {
                            //2 
                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMin) - (rans2d as IGeometry).Envelope.Width;
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - ((rans2d as IGeometry).Envelope.YMin + (rans2d as IGeometry).Envelope.YMax) * 0.5;
                            rans2d.Move(dx, dy);
                        }
                    }
                    #endregion
                }
                (polygon0 as IFillShapeElement).Symbol = (polygon as IFillShapeElement).Symbol;
                (polygon0 as IElement).Geometry = rans2d as IGeometry;
                //文本
                (txtElement0 as ITextElement).Symbol = (txtElement as ITextElement).Symbol;
                (txtElement0 as ISymbolCollectionElement).FontName = (txtElement as ISymbolCollectionElement).FontName;
                (txtElement0 as ISymbolCollectionElement).Size = (txtElement as ISymbolCollectionElement).Size;
                (txtElement0 as ISymbolCollectionElement).Bold = (txtElement as ISymbolCollectionElement).Bold;
                (txtElement0 as ISymbolCollectionElement).Italic = (txtElement as ISymbolCollectionElement).Italic;
                (txtElement0 as ITextElement).Text = (txtElement as ITextElement).Text;
                if (txtElementDown == null)
                {
                    txtElementDown0 = null;
                }
                else
                {
                    if (txtElementDown0 != null)
                    {
                        (txtElementDown0 as ITextElement).Symbol = (txtElementDown as ITextElement).Symbol;
                        (txtElementDown0 as ISymbolCollectionElement).FontName = (txtElementDown as ISymbolCollectionElement).FontName;
                        (txtElementDown0 as ISymbolCollectionElement).Size = (txtElementDown as ISymbolCollectionElement).Size;
                        (txtElementDown0 as ISymbolCollectionElement).Bold = (txtElementDown as ISymbolCollectionElement).Bold;
                        (txtElementDown0 as ISymbolCollectionElement).Italic = (txtElementDown as ISymbolCollectionElement).Italic;
                        (txtElementDown0 as ITextElement).Text = (txtElementDown as ITextElement).Text;
                    }
                    else
                    {
                        txtElementDown0 = txtElementDown;
                    }
                }
                IPoint lineCt = new PointClass();
                if (checkSep)
                {
                    ((sepline as IElement).Geometry as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);
                    dx = ((polygon0 as IElement).Geometry as IArea).Centroid.X - lineCt.X;
                    dy = ((polygon0 as IElement).Geometry as IArea).Centroid.Y - lineCt.Y;
                    (sepline as ITransform2D).Move(dx, dy);
                    sepline0 = sepline;

                }
                //  dx = ((polygon0 as IElement).Geometry as IArea).Centroid.X - ((txtElement0 as IElement).Geometry as IPoint).X;
                // dy = ((polygon0 as IElement).Geometry as IArea).Centroid.Y - ((txtElement0 as IElement).Geometry as IPoint).Y+txtStep;
                ((sepline as IElement).Geometry as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);
                (txtElement0 as IElement).Geometry = new PointClass { X = lineCt.X, Y = lineCt.Y + txtStep };

                if (txtElementDown0 != null)
                {
                    (txtElementDown0 as IElement).Geometry = new PointClass { X = lineCt.X, Y = lineCt.Y - txtStep };
                }
                #endregion

                #region 标注信息
                LabelJson json = new LabelJson
                {
                    AnchorSize = anchorsizeMM,
                    AnchorType = anchorStyle,
                    ConnectLens = labelLineLensMM,
                    TextType = lableGeometry

                };

                #endregion
              
                string jsonText = LabelClass.GetJsonText(json);
                string type = (ge as IElementProperties).Type;
                ge.ClearElements();
                gc.DeleteElement(ge as IElement);
                ge = new GroupElementClass();
                (ge as IElementProperties).Name = jsonText;
                (ge as IElementProperties).Type = type;
                gc.AddElement(anchorline0 as IElement, 0);
                gc.AddElement(connectline0 as IElement, 0);
                gc.AddElement(anchorEle0 as IElement, 0); ;
                gc.AddElement(polygon0 as IElement, 0);
                gc.AddElement(txtElement0 as IElement, 0);
                if (txtElementDown0 != null)
                {
                    gc.AddElement(txtElementDown0 as IElement, 0);
                }

                gc.MoveElementToGroup(anchorline0 as IElement, ge);
                gc.MoveElementToGroup(connectline0 as IElement, ge);
                gc.MoveElementToGroup(anchorEle0 as IElement, ge);
                gc.MoveElementToGroup(polygon0 as IElement, ge);
                gc.MoveElementToGroup(txtElement0 as IElement, ge);
            
                if (txtElementDown0 != null)
                {
                    ge.AddElement(txtElementDown0 as IElement);
                    gc.MoveElementToGroup(txtElementDown0 as IElement, ge);
                }
                if (checkSep)
                {
                    gc.AddElement(sepline0 as IElement, 0);
                    ge.AddElement(sepline0 as IElement);
                    gc.MoveElementToGroup(sepline0 as IElement, ge);
                }


                gc.AddElement(ge as IElement, 0);
                pGraphicsContainerSelect.SelectElement(ge as IElement);
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }


        }
        public void UpdateAttributeElement(string text, string textdown, IPoint ct)
        {

            gc = GraphicsLayer as IGraphicsContainer;
            try
            {

                this.ls = LabelState.Update;
                #region 获取参数
                string fileName = GApplication.Application.Template.Root + @"\专家库\标注引线\属性标注配置.xml";
                if (!File.Exists(fileName))
                {
                    return;
                }
                XDocument doc = XDocument.Load(fileName);
                //只有主区，没附区
                var root = doc.Element("Template").Element("LabelText");
                #region

                //文本
                XElement ele = root.Element("Text");
                fontName = ele.Element("Font").Attribute("FontName").Value;
                fontsize = double.Parse(ele.Element("Font").Attribute("FontSize").Value);
                string colorName = ele.Element("Font").Attribute("FontColor").Value;
                System.Drawing.Color cc = ColorHelper.GetColorByCmykStr(ele.Element("Font").Attribute("FontColor").Value);
                textColor = ColorHelper.ConvertColorToCMYK(cc);
                txtInterval = double.Parse(ele.Value);
                fontBold = bool.Parse(ele.Element("Font").Attribute("FontBold").Value);
                fontItalic = bool.Parse(ele.Element("Font").Attribute("FontItalic").Value);
                //文本下标
                ele = root.Element("TextDown");
                fontNameDown = ele.Element("Font").Attribute("FontName").Value;
                fontsizeDown = double.Parse(ele.Element("Font").Attribute("FontSize").Value);
                colorName = ele.Element("Font").Attribute("FontColor").Value;
                cc = ColorHelper.GetColorByCmykStr(ele.Element("Font").Attribute("FontColor").Value);
                textColorDown = ColorHelper.ConvertColorToCMYK(cc);
                fontBoldDown = bool.Parse(ele.Element("Font").Attribute("FontBold").Value);
                fontItalicDown = bool.Parse(ele.Element("Font").Attribute("FontItalic").Value);
                //锚点
                ele = root.Element("Anchor");
                anchorStyle = (ele.Element("AnchorType").Value);
                var dic = AnchorName.Where(t => t.Value == anchorStyle).ToDictionary(p => p.Key, p => p.Value);
                anchorStyle = dic.First().Key;
                anchorSize = double.Parse(ele.Element("AnchorSize").Value);
                anchorsizeMM = anchorSize;
                anchorSize = act.FocusMap.ReferenceScale * anchorSize * 0.001;
                anchorLineColor = ColorHelper.GetColorByString(ele.Element("AnchorLineColor").Value); ;
                anchorFillColor = ColorHelper.GetColorByString(ele.Element("AnchorFillColor").Value); ;
                dic = LineStyle.Where(t => t.Value == ele.Element("AnchorLineStyle").Value).ToDictionary(p => p.Key, p => p.Value);
                anchorLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                //dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
                anchorFillStyle = (esriSimpleFillStyle.esriSFSSolid);
                anchorLinewidth = double.Parse(ele.Element("AnchorLineWidth").Value);
                //背景
                ele = root.Element("Content");
                lableGeometry = (ele.Element("TextType").Value);
                dic = ExtentName.Where(t => t.Value == lableGeometry).ToDictionary(p => p.Key, p => p.Value);
                lableGeometry = dic.First().Key;
                fillColor = ColorHelper.GetColorByString(ele.Element("FillColor").Value);
                fillLineColor = ColorHelper.GetColorByString(ele.Element("TextLineColor").Value);
                dic = LineStyle.Where(t => t.Value == ele.Element("TextLineType").Value).ToDictionary(p => p.Key, p => p.Value);
                fillLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                fillLinewidth = double.Parse(ele.Element("TextLineWidth").Value);
                dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
                fillStyle = (esriSimpleFillStyle)System.Enum.Parse(typeof(esriSimpleFillStyle), dic.First().Key);

                //连接线
                ele = root.Element("ConnectLine");
                string ls = ele.Element("LineType").Value;
                dic = LineStyle.Where(t => t.Value == ls).ToDictionary(p => p.Key, p => p.Value);
                lineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                linewidth = double.Parse(ele.Element("LineWidth").Value);
                lineColor = ColorHelper.GetColorByString(ele.Element("LineColor").Value);
                labelLineLens = double.Parse(ele.Element("ConnectLens").Value);
                labelLineLensMM = labelLineLens;
                labelLineLens = act.FocusMap.ReferenceScale * labelLineLens * 0.001;
                //分割线
                ele = root.Element("SepLine");
                sepLineColor = ColorHelper.GetColorByString(ele.Element("SepLineColor").Value); ;
                dic = LineStyle.Where(t => t.Value == ele.Element("SepLineType").Value).ToDictionary(p => p.Key, p => p.Value);
                sepLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                sepLinewidth = double.Parse(ele.Element("SepLineWidth").Value); ;
                sepLineInterval = double.Parse(ele.Element("SepInterval").Value); ; ;
                checkSep =false;
                if (textdown != string.Empty)
                {
                    checkSep = true;
                }
                #endregion
                #endregion
                double anchorDx = 0;
                center = ct;
                switch (anchorStyle)
                {
                    case "Circle":
                        CreateCircle(ct, anchorSize * 0.5);
                        anchorDx = anchorSize * 0.5;
                        break;
                    case "Trangle":
                        CreateTrangle(ct, anchorSize);
                        anchorDx = anchorSize * Math.Pow(3, 0.5) * 0.5;
                        break;
                    case "TrangleLine":
                        CreateTrangleLine(ct, anchorSize);
                        break;
                    case "HoraLine":
                        CreateHoralLine(ct, anchorSize);
                        break;
                    case "Rectangle":
                        CreateRectangle(ct, anchorSize);
                        anchorDx = anchorSize * 0.5;
                        break;
                    case "None":
                        CreateCircle(ct, anchorSize * 0.5);
                        anchorDx = anchorSize * 0.5;
                        break;
                    default:
                        break;
                }
                anchorDx = 0;
                SimpleLineSymbolClass slsLine = new SimpleLineSymbolClass();
                slsLine.Width = linewidth * 2.83;
                slsLine.Style = lineStyle;
                slsLine.Color = lineColor;

                SimpleLineSymbolClass slsSepLine = new SimpleLineSymbolClass();
                slsSepLine.Width = sepLinewidth * 2.83;
                slsSepLine.Style = sepLineStyle;
                slsSepLine.Color = sepLineColor;

                SimpleFillSymbolClass sfsFill = new SimpleFillSymbolClass();
                sfsFill.Color = fillColor;
                sfsFill.Style = fillStyle;
                sfsFill.Outline = new SimpleLineSymbolClass()
                {
                    Width = fillLinewidth * 2.83,
                    Style = fillLineStyle,
                    Color = fillLineColor
                };

                SimpleFillSymbolClass sfsAnchor = new SimpleFillSymbolClass();
                sfsAnchor.Color = anchorFillColor;
                sfsAnchor.Style = anchorFillStyle;
                sfsAnchor.Outline = new SimpleLineSymbolClass()
                {
                    Width = anchorLinewidth * 2.83,
                    Style = anchorLineStyle,
                    Color = anchorLineColor
                };
                if (anchorStyle == "None")//特殊处理
                {
                    sfsAnchor.Style = esriSimpleFillStyle.esriSFSNull;
                    sfsAnchor.Outline = new SimpleLineSymbolClass()
                    {
                        Width = anchorLinewidth * 2.83,
                        Color = anchorLineColor,
                        Style = esriSimpleLineStyle.esriSLSNull
                    };
                }
                line1 = new LineElementClass();
                line1.Symbol = slsLine;
                PolylineClass polyline = new PolylineClass();
                polyline.AddPoint(new PointClass { X = center.X + anchorDx, Y = center.Y });
                polyline.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                (line1 as IElement).Geometry = polyline;
                geoLine1 = polyline;
                //
                line2 = new LineElementClass();
                line2.Symbol = slsLine;
                PolylineClass polyline1 = new PolylineClass();
                polyline1.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                polyline1.AddPoint(new PointClass { X = center.X + 35 + labelLineLens, Y = center.Y });
                (line2 as IElement).Geometry = polyline1;
                geoLine2 = polyline1;


                //创建文字
                ITextSymbol pTextSymbol = new TextSymbolClass()
                {
                    Color = textColor,
                };

                double txtStep = 0;
                txtElement = new TextElementClass();
                txtGeometry = ct;
                if (checkSep)
                {
                    //txtInterval = 0.75;
                    txtStep = txtInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001;
                    txtGeometry = new PointClass { X = ct.X, Y = ct.Y + txtStep };
                }
               
                txtElement.Symbol = pTextSymbol;
                (txtElement as ISymbolCollectionElement).FontName = fontName;
                (txtElement as ISymbolCollectionElement).Size = fontsize;
                (txtElement as ISymbolCollectionElement).Bold = fontBold;
                (txtElement as ISymbolCollectionElement).Italic = fontItalic;
                (txtElement as IElement).Geometry = txtGeometry;
                txtElement.Text = text;
                IPolygon outline = new PolygonClass();
                (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);
                IEnvelope enText = outline.Envelope;
                (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                (txtElement as ISymbolCollectionElement).Bold = fontBold;
                (txtElement as ISymbolCollectionElement).Underline = fontunderLine;
                (txtElement as ISymbolCollectionElement).Italic = fontItalic;
                double txtHeight = enText.Height;
                double txtWidth = enText.Width;
                //下标文字
                if (textdown!=string.Empty)
                {
                    (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVABottom;
                    txtElementDown = new TextElementClass();

                    txtElementDown.Symbol = new TextSymbolClass()
                    {
                        Color = textColorDown,
                    }; ;
                    (txtElementDown as ISymbolCollectionElement).FontName = fontNameDown;
                    (txtElementDown as ISymbolCollectionElement).Size = fontsizeDown;
                    (txtElementDown as ISymbolCollectionElement).Bold = fontBoldDown;
                    (txtElementDown as ISymbolCollectionElement).Italic = fontItalicDown;
                    (txtElementDown as IElement).Geometry = new PointClass { X = ct.X, Y = ct.Y - txtStep };
                    txtElementDown.Text = textdown;
                    IPolygon outline1 = new PolygonClass();
                    (txtElementDown as IElement).QueryOutline(act.ScreenDisplay, outline1);
                    (txtElementDown as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVATop;
                    (txtElementDown as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                    IEnvelope enText1 = outline1.Envelope;
                    if (txtWidth < enText1.Width)
                    {
                        txtWidth = enText1.Width;
                    }
                    txtHeight =2*Math.Max(txtHeight, enText1.Height) + txtStep;
                }
                //创建内容形状
                double orginWidth, orginHeight = 0;
                double orginTxtWidth, orginTxtHeight = 0;
                orginWidth = geoDic[lableGeometry].Envelope.Width;
                orginHeight = geoDic[lableGeometry].Envelope.Height;
                orginTxtWidth = geoTextDic[lableGeometry].Envelope.Width;
                orginTxtHeight = geoTextDic[lableGeometry].Envelope.Height;

                if (txtWidth / txtHeight > orginTxtWidth / orginTxtHeight)
                {
                    extentWidth = txtWidth / orginTxtWidth * orginWidth;
                    extentHeight = txtHeight / orginTxtHeight * orginHeight;
                }
                else
                {
                    if (lableGeometry.ToLower().Contains("cloud"))
                    {
                        extentHeight = txtHeight * orginHeight / orginTxtHeight;
                        // extentWidth = extentHeight / orginHeight * orginWidth;
                        extentWidth = extentHeight / orginHeight * orginWidth;
                    }
                    else
                    {
                        extentHeight = txtHeight / orginTxtHeight * orginHeight;
                        extentWidth = txtWidth / orginTxtWidth * orginWidth;
                    }
                }

                CreateLabelExtent();
                //
                sepline = new LineElementClass();
                sepline.Symbol = slsSepLine;
                PolylineClass polyline2 = new PolylineClass();
                sepLineIntervalMM = 0;
                polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM, Y = center.Y });
                polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM + geoPolygon.Envelope.Width - 2 * sepLineIntervalMM, Y = center.Y });
                (sepline as IElement).Geometry = polyline2;
                geosepLine = polyline2;

                (polygon as IFillShapeElement).Symbol = sfsFill;
                if (anchorEle != null)
                {
                    if (anchorEle is IFillShapeElement)
                    {
                        (anchorEle as IFillShapeElement).Symbol = sfsAnchor;
                    }
                    if (anchorEle is ILineElement)
                    {
                        (anchorEle as ILineElement).Symbol = sfsAnchor.Outline;
                    }
                }
                #region //更新
                IGraphicsContainer pGraphicsContainer = gc as IGraphicsContainer;
                IGraphicsContainerSelect pGraphicsContainerSelect = gc as IGraphicsContainerSelect;
                //遍历Element
                IEnumElement pEnumElemen = pGraphicsContainerSelect.SelectedElements;
                pEnumElemen.Reset();
                IElement pElement = pEnumElemen.Next();
                var ge = pElement as IGroupElement;
                IElement anchorEle0 = null;
                ILineElement anchorline0 = null;
                ILineElement connectline0 = null;
                IPolygonElement polygon0 = null;
                ITextElement txtElement0 = null;
                ITextElement txtElementDown0 = null;
                ILineElement sepline0 = null;

                //引线标注
                for (int i = 0; i < ge.ElementCount; i++)
                {
                    #region
                    IElement ee = ge.get_Element(i);
                    switch ((ee as IElementProperties).Name)
                    {
                        case "锚点":
                            anchorEle0 = (ee as IClone).Clone() as IElement; ;
                            break;
                        case "锚线":
                            anchorline0 = (ee as IClone).Clone() as ILineElement;
                            break;
                        case "连接线":
                            connectline0 = (ee as IClone).Clone() as ILineElement;
                            break;
                        case "分割线":
                            sepline0 = (ee as IClone).Clone() as ILineElement;
                            break;
                        case "内容框":
                            polygon0 = (ee as IClone).Clone() as IPolygonElement;
                            break;
                        case "文本":
                            txtElement0 = (ee as IClone).Clone() as ITextElement;
                            break;
                        case "下标文本":
                            txtElementDown0 = (ee as IClone).Clone() as ITextElement;
                            break;
                        default:
                            break;

                    }
                    #endregion
                }
                //锚点
                ILine line = new LineClass();
                line.FromPoint = ((anchorline0 as IElement).Geometry as IPolyline).FromPoint;
                line.ToPoint = ((anchorline0 as IElement).Geometry as IPolyline).ToPoint;
                ITransform2D rans2dAnchor = anchorGeo as ITransform2D;
                rans2dAnchor.Rotate(center, line.Angle);
                if (anchorEle0.Geometry.GeometryType == anchorGeo.GeometryType)
                {
                    anchorEle0.Geometry = rans2dAnchor as IGeometry;
                }
                else//圆形变成三角线
                {
                    anchorEle0 = anchorEle;
                    (anchorEle0 as IElementProperties).Name = "锚点";
                    anchorEle0.Geometry = rans2dAnchor as IGeometry;
                }
                if (anchorEle0 is IFillShapeElement)
                {
                    (anchorEle0 as IFillShapeElement).Symbol = (anchorEle as IFillShapeElement).Symbol;
                }
                else if (anchorEle0 is ILineElement)
                {
                    (anchorEle0 as ILineElement).Symbol = (anchorEle as ILineElement).Symbol;
                }

                //连接线
                (anchorline0 as ILineElement).Symbol = line1.Symbol;//锚点线
                (connectline0 as ILineElement).Symbol = line2.Symbol;//连接线
                double scaleX = ((line2 as IElement).Geometry as IPolyline).Length / ((connectline0 as IElement).Geometry as IPolyline).Length;
                (connectline0 as ITransform2D).Scale(((connectline0 as IElement).Geometry as IPolyline).FromPoint, scaleX, 1);
                //内容
                double dx = ((polygon0 as IElement).Geometry.Envelope as IArea).Centroid.X - ((polygon as IElement).Geometry.Envelope as IArea).Centroid.X;
                double dy = ((polygon0 as IElement).Geometry.Envelope as IArea).Centroid.Y - ((polygon as IElement).Geometry.Envelope as IArea).Centroid.Y;

                bool isCircle = false;
                string extentShp = LabelClass.GetLabelInfo((ge as IElementProperties).Name).TextType;
                if (extentShp == "Circle")
                    isCircle = true;

                double scaleDx = (polygon as IElement).Geometry.Envelope.Width / (polygon0 as IElement).Geometry.Envelope.Width;
                double scaleDy = (polygon as IElement).Geometry.Envelope.Height / (polygon0 as IElement).Geometry.Envelope.Height;
                if (isCircle)
                {
                    scaleDy = 1;
                }

                ITransform2D rans2d = (polygon as IElement).Geometry as ITransform2D;
                rans2d.Move(dx, dy);
                IPoint enCenter = ((polygon0 as IElement).Geometry as IArea).Centroid;
                // rans2d.Scale(enCenter, scaleDx, scaleDy);
                //判断连接线是否边长
                {
                    //旋转内容窗体
                    #region
                    //
                    if (Math.Abs(line.Angle) < Math.PI / 2)
                    {
                        if (Math.Abs(line.Angle) > Math.PI / 2 * 0.8)//72度
                        {
                            double reg = line.Angle / Math.Abs(line.Angle);
                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMax + (rans2d as IGeometry).Envelope.XMin) * 0.5;
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - (reg < 0 ? ((rans2d as IGeometry).Envelope.YMax) : ((rans2d as IGeometry).Envelope.YMin));
                            rans2d.Move(dx, dy);

                        }
                        else
                        {


                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMin);
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - ((rans2d as IGeometry).Envelope.YMin + (rans2d as IGeometry).Envelope.YMax) * 0.5;
                            rans2d.Move(dx, dy);

                        }
                    }
                    else
                    {
                        if (Math.Abs(line.Angle) < Math.PI / 2 + Math.PI / 2 * 0.2)//72度
                        {


                            double reg = line.Angle / Math.Abs(line.Angle);
                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMax + (rans2d as IGeometry).Envelope.XMin) * 0.5;
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - (reg < 0 ? ((rans2d as IGeometry).Envelope.YMax) : ((rans2d as IGeometry).Envelope.YMin));
                            rans2d.Move(dx, dy);

                        }
                        else
                        {
                            //2 
                            dx = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.X - ((rans2d as IGeometry).Envelope.XMin) - (rans2d as IGeometry).Envelope.Width;
                            dy = ((connectline0 as IElement).Geometry as IPolyline).ToPoint.Y - ((rans2d as IGeometry).Envelope.YMin + (rans2d as IGeometry).Envelope.YMax) * 0.5;
                            rans2d.Move(dx, dy);
                        }
                    }
                    #endregion
                }
                (polygon0 as IFillShapeElement).Symbol = (polygon as IFillShapeElement).Symbol;
                (polygon0 as IElement).Geometry = rans2d as IGeometry;
                //文本
                (txtElement0 as ITextElement).Symbol = (txtElement as ITextElement).Symbol;
                (txtElement0 as ISymbolCollectionElement).FontName = (txtElement as ISymbolCollectionElement).FontName;
                (txtElement0 as ISymbolCollectionElement).Size = (txtElement as ISymbolCollectionElement).Size;
                (txtElement0 as ISymbolCollectionElement).Bold = (txtElement as ISymbolCollectionElement).Bold;
                (txtElement0 as ISymbolCollectionElement).Italic = (txtElement as ISymbolCollectionElement).Italic;
                (txtElement0 as ITextElement).Text = (txtElement as ITextElement).Text;
                IPoint txtUpGeo = ((polygon0 as IElement).Geometry as IArea).Centroid;
                (txtElement0 as IElement).Geometry = new PointClass { X = txtUpGeo.X, Y = txtUpGeo.Y + txtStep };
                if (txtElementDown == null)
                {
                    txtElementDown0 = null;
                }
                else
                {
                    if (txtElementDown0 != null)
                    {
                        (txtElementDown0 as ITextElement).Symbol = (txtElementDown as ITextElement).Symbol;
                        (txtElementDown0 as ISymbolCollectionElement).FontName = (txtElementDown as ISymbolCollectionElement).FontName;
                        (txtElementDown0 as ISymbolCollectionElement).Size = (txtElementDown as ISymbolCollectionElement).Size;
                        (txtElementDown0 as ITextElement).Text = (txtElementDown as ITextElement).Text;
                        (txtElementDown0 as ISymbolCollectionElement).Bold = (txtElementDown as ISymbolCollectionElement).Bold;
                        (txtElementDown0 as ISymbolCollectionElement).Italic = (txtElementDown as ISymbolCollectionElement).Italic;
                    }
                    else
                    {
                        txtElementDown0 = txtElementDown;
                    }
                   
                    (txtElementDown0 as IElement).Geometry = new PointClass { X = txtUpGeo.X, Y = txtUpGeo.Y - txtStep };
                }
                checkSep = false;
                if (txtElementDown0 != null)
                {
                    checkSep = true;
                    
                }

                IPoint lineCt = new PointClass();
                if (checkSep)
                {
                    ((sepline as IElement).Geometry as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);
                    dx = ((polygon0 as IElement).Geometry as IArea).Centroid.X - lineCt.X;
                    dy = ((polygon0 as IElement).Geometry as IArea).Centroid.Y - lineCt.Y;
                    (sepline as ITransform2D).Move(dx, dy);
                    sepline0 = sepline;

                }
                (txtElement0 as IElement).Geometry = new PointClass { X = ((polygon0 as IElement).Geometry as IArea).Centroid.X, Y = ((polygon0 as IElement).Geometry as IArea).Centroid.Y + txtStep };
                if ((txtElement0 as ISymbolCollectionElement).VerticalAlignment == esriTextVerticalAlignment.esriTVACenter)
                {
                    //纠正中心点位置:只有上标时候
                    IPoint tCenter = ((polygon0 as IElement).Geometry as IArea).Centroid;
                    outline = new PolygonClass();
                    (txtElement0 as IElement).QueryOutline(act.ScreenDisplay, outline);
                    IPoint txtCenter = (outline as IArea).Centroid;
                    (txtElement0 as ITransform2D).Move(tCenter.X - txtCenter.X, tCenter.Y - txtCenter.Y);
                }
               
                #endregion

                #region 标注信息
                LabelJson json = new LabelJson
                {
                    AnchorSize = anchorsizeMM,
                    AnchorType = anchorStyle,
                    ConnectLens = labelLineLensMM,
                    TextType = lableGeometry

                };

                #endregion

                string jsonText = LabelClass.GetJsonText(json);
                string type = (ge as IElementProperties).Type;
                ge.ClearElements();
                gc.DeleteElement(ge as IElement);
                ge = new GroupElementClass();
                (ge as IElementProperties).Name = jsonText;
                (ge as IElementProperties).Type = type;
                gc.AddElement(anchorline0 as IElement, 0);
                gc.AddElement(connectline0 as IElement, 0);
                gc.AddElement(anchorEle0 as IElement, 0); ;
                gc.AddElement(polygon0 as IElement, 0);
                gc.AddElement(txtElement0 as IElement, 0);
               
                gc.MoveElementToGroup(anchorline0 as IElement, ge);
                gc.MoveElementToGroup(connectline0 as IElement, ge);
                gc.MoveElementToGroup(anchorEle0 as IElement, ge);
                gc.MoveElementToGroup(polygon0 as IElement, ge);
                gc.MoveElementToGroup(txtElement0 as IElement, ge);

                if (txtElementDown0 != null)
                {   
                    gc.AddElement(txtElementDown0 as IElement, 0);
                    (txtElementDown0 as IElementProperties).Name = "下标文本";
                    gc.MoveElementToGroup(txtElementDown0 as IElement, ge);
                }
                if (checkSep)
                {
                    (sepline0 as IElementProperties).Name = "分割线";
                    gc.AddElement(sepline0 as IElement, 0);
                    gc.MoveElementToGroup(sepline0 as IElement, ge);
                }


                gc.AddElement(ge as IElement, 0);
                pGraphicsContainerSelect.SelectElement(ge as IElement);
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }


        }



        private string POIText(IPoint pt)
        {
            string txt = "";
            try
            {
                double disbuffer = act.ScreenDisplay.DisplayTransformation.FromPoints(3);
                IGeometry geobuffer = (pt as ITopologicalOperator).Buffer(disbuffer);
                IFeatureLayer feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => l.Visible && (l is IGeoFeatureLayer) &&
                ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == "POI")).FirstOrDefault() as IFeatureLayer;
                if (feLayer == null)
                {
                    feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => l.Visible && (l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == "AGNP")).FirstOrDefault() as IFeatureLayer;
                }
                if (feLayer != null)
                {
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = geobuffer;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeatureCursor cursor = feLayer.Search(sf, false);
                    IFeature fe = cursor.NextFeature();
                    if (fe != null)
                    {
                        string val = fe.get_Value(feLayer.FeatureClass.FindField("Name")).ToString();
                        if (val.Trim() != string.Empty)
                            txt = val;

                    }
                    Marshal.ReleaseComObject(cursor);
                }
                return txt;
            }
            catch
            {
                return "";
            }
        }
        //属性关联
        private string AttrLableText(IGeometry geometry ,ref IPoint ct,ref string txtDown)
        {
            string txt = "";
            try
            {

                DataRow drLable = null;
                foreach (DataRow row in LabAttrDt.Rows)
                {
                    #region
                    double dis =double.Parse(row["捕捉容差"].ToString().Trim());
                    string lyrName = row["图层"].ToString();
                    string fieldName = row["标注字段"].ToString();
                    string fieldName1 = row["标注字段1"].ToString();
                    bool isExpress = row["表达式应用"].ToString() == "是";
                    bool isExpress1 = row["表达式应用下标"].ToString() == "是";
                    string expressStr = row["注记表达式"].ToString();
                    string expressStr1 = row["注记表达式下标"].ToString();

                    double disbuffer = act.ScreenDisplay.DisplayTransformation.FromPoints(dis);
                    IGeometry geobuffer = geometry;
                    if (geometry is IPoint)
                    {
                        geobuffer = (geometry as ITopologicalOperator).Buffer(disbuffer);
                    }
                    IFeatureLayer feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => l.Visible && (l is IGeoFeatureLayer) &&
                    ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == lyrName)).FirstOrDefault() as IFeatureLayer;
                    if (feLayer == null)
                    {
                        continue;
                    }
                 
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = geobuffer;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    if(row["标注方法"].ToString()=="分类标注")
                    {
                        sf.WhereClause = row["过滤条件"].ToString();
                    }
                    IFeatureCursor cursor = feLayer.Search(sf, false);
                    IFeature fe = cursor.NextFeature();
                    if (fe != null)
                    {
                        #region
                        string val = string.Empty;
                        if (isExpress && expressStr != string.Empty)
                        {
                            val = LabelExpressHelper.MapLableFromLyr(feLayer, fe, expressStr);
                        }
                        else
                        {
                            val = fe.get_Value(feLayer.FeatureClass.FindField(fieldName)).ToString();
                        }
                        if (val.Trim() != string.Empty)
                        {
                            drLable = row;
                            txt = val;
                            if (isExpress1 && expressStr1 != string.Empty)
                            {
                                txtDown = LabelExpressHelper.MapLableFromLyr(feLayer, fe, expressStr1);
                            }
                            else if(fieldName1!="")
                            {
                                txtDown = fe.get_Value(feLayer.FeatureClass.FindField(fieldName1)).ToString();
                            }
                            //修正最近的点
                            IProximityOperator po = fe.ShapeCopy as IProximityOperator;
                            ct = po.ReturnNearestPoint(ct, esriSegmentExtension.esriNoExtension);
                           // break;
                        }
                        #endregion
                    }
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        #region
                        string valDown = string.Empty;
                        string val = string.Empty;
                        if (isExpress && expressStr != string.Empty)
                        {
                            val = LabelExpressHelper.MapLableFromLyr(feLayer, fe, expressStr);
                        }
                        else
                        {
                            val = fe.get_Value(feLayer.FeatureClass.FindField(fieldName)).ToString();
                        }
                        if (val.Trim() != string.Empty)
                        {
                            
                            if (isExpress1 && expressStr1 != string.Empty)
                            {
                                valDown = LabelExpressHelper.MapLableFromLyr(feLayer, fe, expressStr1);
                            }
                            else if (fieldName1 != "")
                            {
                                valDown = fe.get_Value(feLayer.FeatureClass.FindField(fieldName1)).ToString();
                            }
                            //修正最近的点
                            IProximityOperator po = fe.ShapeCopy as IProximityOperator;
                            IPoint  anchor = po.ReturnNearestPoint(ct, esriSegmentExtension.esriNoExtension);
                            attributeLbInfos.Add(new AttrLabelInfos { TextUp = val, TextDown = valDown, AnchorPoint = anchor });
                        }
                        #endregion

                    }
                    Marshal.ReleaseComObject(cursor);
                    #endregion
                }
                if (drLable == null)
                    return txt;
                LabelRule = drLable;
                //设置符号信息
                #region 获取参数
                {
                    #region
                   
                    {
                        #region

                        //文本
                        fontName = drLable["字体名称"].ToString();
                        fontsize = double.Parse(drLable["字体大小"].ToString());
                        string colorCmyk = drLable["字体颜色"].ToString();
                        System.Drawing.Color cc = ColorHelper.GetColorByCmykStr(colorCmyk);
                        textColor = ColorHelper.ConvertColorToCMYK(cc);
                        fontBold = bool.Parse(drLable["字体加粗"].ToString());
                        fontItalic = bool.Parse(drLable["字体斜体"].ToString());
                        fontunderLine = bool.Parse(drLable["字体下划线"].ToString());
                        fontStyle = drLable["字体对齐方式"].ToString();
                        txtInterval = double.Parse(drLable["文本间距"].ToString());
                        //下标文本
                        fontNameDown = drLable["字体名称1"].ToString();
                        fontsizeDown = double.Parse(drLable["字体大小1"].ToString());
                        colorCmyk = drLable["字体颜色1"].ToString();
                        cc = ColorHelper.GetColorByCmykStr(colorCmyk);
                        textColorDown = ColorHelper.ConvertColorToCMYK(cc);
                        fontBoldDown = bool.Parse(drLable["字体加粗1"].ToString());
                        fontItalicDown = bool.Parse(drLable["字体斜体1"].ToString());
                        fontunderLineDown = bool.Parse(drLable["字体下划线1"].ToString());
                        fontStyleDown = drLable["字体对齐方式1"].ToString();
                        //锚点

                        anchorStyle = drLable["锚点类型"].ToString();
                        var dic = AnchorName.Where(t => t.Value == anchorStyle).ToDictionary(p => p.Key, p => p.Value);
                        anchorStyle = dic.First().Key;

                        anchorSize = double.Parse(drLable["锚点尺寸"].ToString());
                        anchorsizeMM = anchorSize;
                        anchorSize = act.FocusMap.ReferenceScale * anchorSize * 0.001;
                        anchorLineColor = ColorHelper.GetColorByString(drLable["锚点边线颜色"].ToString());
                        anchorFillColor = ColorHelper.GetColorByString(drLable["锚点填充颜色"].ToString()); ;
                        dic = LineStyle.Where(t => t.Value == drLable["锚点边线类型"].ToString()).ToDictionary(p => p.Key, p => p.Value);
                        anchorLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                        dic = FillStyle.Where(t => t.Value == drLable["锚点填充类型"].ToString()).ToDictionary(p => p.Key, p => p.Value);
                        anchorFillStyle = (esriSimpleFillStyle)System.Enum.Parse(typeof(esriSimpleFillStyle), dic.First().Key);
                        anchorLinewidth = double.Parse(drLable["锚点边线宽度"].ToString());
                        //背景
                        lableGeometry = drLable["内容框类型"].ToString();
                        dic = ExtentName.Where(t => t.Value == lableGeometry).ToDictionary(p => p.Key, p => p.Value);
                        lableGeometry = dic.First().Key;
                        fillColor = ColorHelper.GetColorByString(drLable["内容框填充颜色"].ToString());
                        fillLineColor = ColorHelper.GetColorByString(drLable["内容框边线颜色"].ToString());
                        dic = LineStyle.Where(t => t.Value == drLable["内容框边线类型"].ToString()).ToDictionary(p => p.Key, p => p.Value);
                        fillLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                        fillLinewidth = double.Parse(drLable["内容框边线宽度"].ToString());
                        dic = FillStyle.Where(t => t.Value == drLable["内容框填充类型"].ToString()).ToDictionary(p => p.Key, p => p.Value);
                        fillStyle = (esriSimpleFillStyle)System.Enum.Parse(typeof(esriSimpleFillStyle), dic.First().Key);

                        //连接线

                        linewidth = double.Parse(drLable["连接线宽度"].ToString());
                        labelLineLens = double.Parse(drLable["连接线长度"].ToString());
                        labelLineLensMM = labelLineLens;
                        labelLineLens = act.FocusMap.ReferenceScale * labelLineLens * 0.001;
                        dic = LineStyle.Where(t => t.Value == drLable["连接线类型"].ToString()).ToDictionary(p => p.Key, p => p.Value);
                        lineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                        lineColor = ColorHelper.GetColorByString(drLable["连接线颜色"].ToString());
                       
                        //分割线

                        sepLineColor = ColorHelper.GetColorByString(drLable["分割线颜色"].ToString());
                        dic = LineStyle.Where(t => t.Value == drLable["分割线类型"].ToString()).ToDictionary(p => p.Key, p => p.Value);
                        sepLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
                        sepLinewidth = double.Parse(drLable["分割线宽度"].ToString());
                        sepLineInterval = double.Parse(drLable["分割线间距"].ToString());
                        checkSep = drLable["分割线显示"].ToString() == "是";

                        #endregion

                    }
                    #endregion
                }
                #endregion
                return txt;
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// 1.常规折线标注
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ct"></param>
        public void CreateLabelPolyLine(string text, string textdown, IPoint ct)
        {
            lbType = LabelType.NormalLine;
            gc = GraphicsLayer as IGraphicsContainer;
            if (POIState)
            {
                string val = POIText(ct);
                if (val != "")
                    text = val;
            }

            anchorEle = null;
            anchorGeo = null;
            line1 = null;
            geoLine1 = null;
            line2 = null;
            geoLine2 = null;
            polygon = null;
            geoPolygon = null;

            txtElement = null;
            txtGeometry = null;

            sepline = null;
            geosepLine = null;

            #region 获取参数
            string fileName = GApplication.Application.Template.Root + @"\专家库\标注引线\折线配置.xml";
            if (!File.Exists(fileName))
            {
                return;
            }
            XDocument doc = XDocument.Load(fileName);
            //只有主区，没附区
            var root = doc.Element("Template").Element("LabelText");
            #region

            //文本
            XElement ele = root.Element("Text");
            fontName = ele.Element("Font").Attribute("FontName").Value;
            fontsize = double.Parse(ele.Element("Font").Attribute("FontSize").Value);
            string colorName = ele.Element("Font").Attribute("FontColor").Value;
            System.Drawing.Color cc = ColorHelper.GetColorByCmykStr(ele.Element("Font").Attribute("FontColor").Value);
            textColor = ColorHelper.ConvertColorToCMYK(cc);
            txtInterval = double.Parse(ele.Value);
            fontBold = bool.Parse(ele.Element("Font").Attribute("FontBold").Value);
            fontItalic = bool.Parse(ele.Element("Font").Attribute("FontItalic").Value);
            //文本下标
            ele = root.Element("TextDown");
            fontNameDown = ele.Element("Font").Attribute("FontName").Value;
            fontsizeDown = double.Parse(ele.Element("Font").Attribute("FontSize").Value);
            colorName = ele.Element("Font").Attribute("FontColor").Value;
            cc = ColorHelper.GetColorByCmykStr(ele.Element("Font").Attribute("FontColor").Value);
            textColorDown = ColorHelper.ConvertColorToCMYK(cc);
            fontBoldDown = bool.Parse(ele.Element("Font").Attribute("FontBold").Value);
            fontItalicDown = bool.Parse(ele.Element("Font").Attribute("FontItalic").Value);
            //锚点
            ele = root.Element("Anchor");
            anchorStyle = (ele.Element("AnchorType").Value);
            var dic = AnchorName.Where(t => t.Value == anchorStyle).ToDictionary(p => p.Key, p => p.Value);
            anchorStyle = dic.First().Key;
            anchorSize = double.Parse(ele.Element("AnchorSize").Value);
            anchorsizeMM = anchorSize;
            anchorSize = act.FocusMap.ReferenceScale * anchorSize * 0.001;
            anchorLineColor = ColorHelper.GetColorByString(ele.Element("AnchorLineColor").Value); ;
            anchorFillColor = ColorHelper.GetColorByString(ele.Element("AnchorFillColor").Value); ;
            dic = LineStyle.Where(t => t.Value == ele.Element("AnchorLineStyle").Value).ToDictionary(p => p.Key, p => p.Value);
            anchorLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
            dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
            anchorFillStyle = (esriSimpleFillStyle)System.Enum.Parse(typeof(esriSimpleFillStyle), dic.First().Key);
            anchorLinewidth = double.Parse(ele.Element("AnchorLineWidth").Value);
            //背景
            ele = root.Element("Content");
            lableGeometry = (ele.Element("TextType").Value);
            dic = ExtentName.Where(t => t.Value == lableGeometry).ToDictionary(p => p.Key, p => p.Value);
            lableGeometry = dic.First().Key;
            fillColor = ColorHelper.GetColorByString(ele.Element("FillColor").Value);
            fillLineColor = ColorHelper.GetColorByString(ele.Element("TextLineColor").Value);
            dic = LineStyle.Where(t => t.Value == ele.Element("TextLineType").Value).ToDictionary(p => p.Key, p => p.Value);
            fillLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
            fillLinewidth = double.Parse(ele.Element("TextLineWidth").Value);
            dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
            fillStyle = (esriSimpleFillStyle)System.Enum.Parse(typeof(esriSimpleFillStyle), dic.First().Key);

            //连接线
            ele = root.Element("ConnectLine");
            string ls = ele.Element("LineType").Value;
            dic = LineStyle.Where(t => t.Value == ls).ToDictionary(p => p.Key, p => p.Value);
            lineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
            linewidth = double.Parse(ele.Element("LineWidth").Value);
            lineColor = ColorHelper.GetColorByString(ele.Element("LineColor").Value);
            labelLineLens = double.Parse(ele.Element("ConnectLens").Value);
            labelLineLensMM = labelLineLens;
            labelLineLens = act.FocusMap.ReferenceScale * labelLineLens * 0.001;
            //分割线
            ele = root.Element("SepLine");
            sepLineColor = ColorHelper.GetColorByString(ele.Element("SepLineColor").Value); ;
            dic = LineStyle.Where(t => t.Value == ele.Element("SepLineType").Value).ToDictionary(p => p.Key, p => p.Value);
            sepLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
            sepLinewidth = double.Parse(ele.Element("SepLineWidth").Value); ;
            sepLineInterval = double.Parse(ele.Element("SepInterval").Value); ; ;
            checkSep = bool.Parse(ele.Attribute("check").Value);

            #endregion
            #endregion
            double anchorDx = 0;
            center = ct;
            switch (anchorStyle)
            {
                case "Circle":
                    CreateCircle(ct, anchorSize * 0.5);
                    anchorDx = anchorSize * 0.5;
                    break;
                case "Trangle":
                    CreateTrangle(ct, anchorSize);
                    anchorDx = anchorSize * Math.Pow(3, 0.5) * 0.5;
                    break;
                case "TrangleLine":
                    CreateTrangleLine(ct, anchorSize);
                    break;
                case "HoraLine":
                    CreateHoralLine(ct, anchorSize);
                    break;
                case "Rectangle":
                    CreateRectangle(ct, anchorSize);
                    anchorDx = anchorSize * 0.5;
                    break;
                case "None":
                    CreateCircle(ct, anchorSize * 0.5);
                    anchorDx = anchorSize * 0.5;
                    break;
                default:
                    break;
            }
            anchorDx = 0;
            SimpleLineSymbolClass slsLine = new SimpleLineSymbolClass();
            slsLine.Width = linewidth * 2.83;
            slsLine.Style = lineStyle;
            slsLine.Color = lineColor;

            SimpleLineSymbolClass slsSepLine = new SimpleLineSymbolClass();
            slsSepLine.Width = sepLinewidth * 2.83;
            slsSepLine.Style = sepLineStyle;
            slsSepLine.Color = sepLineColor;

            SimpleFillSymbolClass sfsFill = new SimpleFillSymbolClass();
            sfsFill.Color = fillColor;
            sfsFill.Style = esriSimpleFillStyle.esriSFSNull;
            sfsFill.Outline = new SimpleLineSymbolClass()
            {
                Width = fillLinewidth * 2.83,
                Style = esriSimpleLineStyle.esriSLSNull,
                Color = fillLineColor
            };

            SimpleFillSymbolClass sfsAnchor = new SimpleFillSymbolClass();
            sfsAnchor.Color = anchorFillColor;
            sfsAnchor.Style = anchorFillStyle;
            sfsAnchor.Outline = new SimpleLineSymbolClass()
            {
                Width = anchorLinewidth * 2.83,
                Style = anchorLineStyle,
                Color = anchorLineColor
            };
            if (anchorStyle == "None")//特殊处理
            {
                sfsAnchor.Style = esriSimpleFillStyle.esriSFSNull;
                sfsAnchor.Outline = new SimpleLineSymbolClass()
                {
                    Width = anchorLinewidth * 2.83,
                    Color = anchorLineColor,
                    Style = esriSimpleLineStyle.esriSLSNull
                };
            }
            checkSep = true;
            line1 = new LineElementClass();
            line1.Symbol = slsLine;
            PolylineClass polyline = new PolylineClass();
            polyline.AddPoint(new PointClass { X = center.X + anchorDx, Y = center.Y });
            polyline.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
            (line1 as IElement).Geometry = polyline;
            geoLine1 = polyline;
            //
            line2 = new LineElementClass();
            line2.Symbol = slsLine;
            PolylineClass polyline1 = new PolylineClass();
            polyline1.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
            polyline1.AddPoint(new PointClass { X = center.X + 35 + labelLineLens, Y = center.Y });
            (line2 as IElement).Geometry = polyline1;
            geoLine2 = polyline1;


           
            //创建文字
            double txtStep = txtInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001;
            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = textColor,
            };
            txtElement = new TextElementClass();
            txtElement.Symbol = pTextSymbol;
            (txtElement as ISymbolCollectionElement).FontName = fontName;
            (txtElement as ISymbolCollectionElement).Size = fontsize;
            (txtElement as ISymbolCollectionElement).Bold = fontBold;
            (txtElement as ISymbolCollectionElement).Italic = fontItalic;
            (txtElement as IElement).Geometry = new PointClass { X = ct.X, Y = ct.Y + txtStep };
            txtElement.Text = text;
            IPolygon outline = new PolygonClass();
            (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);

            IEnvelope enText = outline.Envelope;
            (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVABottom;
            (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            double txtHeight = enText.Height;
            double txtWidth = enText.Width;
            //下标文字
            if (textdown != string.Empty)
            {
                txtElementDown = new TextElementClass();

                txtElementDown.Symbol = new TextSymbolClass()
                {
                    Color = textColorDown,
                }; ;
                (txtElementDown as ISymbolCollectionElement).FontName = fontNameDown;
                (txtElementDown as ISymbolCollectionElement).Size = fontsizeDown;
                (txtElementDown as IElement).Geometry = new PointClass { X = ct.X, Y = ct.Y - txtStep }; 
                txtElementDown.Text = textdown;
                IPolygon outline1 = new PolygonClass();
                (txtElementDown as IElement).QueryOutline(act.ScreenDisplay, outline1);
                (txtElementDown as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVATop;
                (txtElementDown as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                (txtElementDown as ISymbolCollectionElement).Bold = fontBoldDown;
                (txtElementDown as ISymbolCollectionElement).Italic = fontItalicDown;
                IEnvelope enText1 = outline1.Envelope;
                if (txtWidth < enText1.Width)
                {
                    txtWidth = enText1.Width;
                }
                txtHeight += enText1.Height + 2 * txtStep;
            }
          

            //创建内容形状
            double orginWidth, orginHeight = 0;
            double orginTxtWidth, orginTxtHeight = 0;
            orginWidth = geoDic[lableGeometry].Envelope.Width;
            orginHeight = geoDic[lableGeometry].Envelope.Height;
            orginTxtWidth = geoTextDic[lableGeometry].Envelope.Width;
            orginTxtHeight = geoTextDic[lableGeometry].Envelope.Height;

            if (txtWidth / txtHeight > orginTxtWidth / orginTxtHeight)
            {
                extentWidth = txtWidth / orginTxtWidth * orginWidth;
                extentHeight = txtHeight / orginTxtHeight * orginHeight;
            }
            else
            {
                 
                extentHeight = txtHeight / orginTxtHeight * orginHeight;
                extentWidth = txtWidth / orginTxtWidth * orginWidth;
                
            }
           
            CreateLabelExtent();
            //
            sepline = new LineElementClass();
            sepline.Symbol = slsLine;
            PolylineClass polyline2 = new PolylineClass();
            sepLineIntervalMM =0;
            polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM, Y = center.Y });
            polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM + geoPolygon.Envelope.Width - 2 * sepLineIntervalMM, Y = center.Y });
            (sepline as IElement).Geometry = polyline2;
            geosepLine = polyline2;

            (polygon as IFillShapeElement).Symbol = sfsFill;
            if (anchorEle != null)
            {
                if (anchorEle is IFillShapeElement)
                {
                    (anchorEle as IFillShapeElement).Symbol = sfsAnchor;
                }
                if (anchorEle is ILineElement)
                {
                    (anchorEle as ILineElement).Symbol = sfsAnchor.Outline;
                }
                gc.UpdateElement(anchorEle);
            }
            gc.AddElement(line1 as IElement, 0);
            gc.AddElement(line2 as IElement, 0);
            gc.AddElement(polygon as IElement, 0);
            gc.AddElement(txtElement as IElement, 0);
            gc.AddElement(sepline as IElement, 0);
            if (txtElementDown != null)
            {
                gc.AddElement(txtElementDown as IElement, 0);
            }
            center = ct;
            lbType = LabelType.NormalLine;

        }

        /// <summary>
        /// 2.常规形状标注
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ct"></param>
        public void CreateLabelLine(string text, IPoint ct)
        {
            lbType = LabelType.ConnectLine;
            gc = GraphicsLayer as IGraphicsContainer;
            if (POIState)
            {
                string val = POIText(ct);
                if (val != "")
                    text = val;
            }

            anchorEle = null;
            anchorGeo = null;
            line1 = null;
            geoLine1 = null;
            line2 = null;
            geoLine2 = null;
            polygon = null;
            geoPolygon = null;

            txtElement = null;
            txtGeometry = null;

            sepline = null;
            geosepLine = null;

            #region 获取参数
            string fileName = GApplication.Application.Template.Root + @"\专家库\标注引线\默认配置.xml";
            if(!File.Exists(fileName))
            {
                return;
            }
            XDocument doc = XDocument.Load(fileName);
            //只有主区，没附区
            var root = doc.Element("Template").Element("LabelText");
            #region
               
            //文本
            XElement ele = root.Element("Text");  
            fontName= ele.Element("Font").Attribute("FontName").Value;
            fontsize= double.Parse(ele.Element("Font").Attribute("FontSize").Value);
            string colorName = ele.Element("Font").Attribute("FontColor").Value;
            System.Drawing.Color cc = ColorHelper.GetColorByCmykStr(ele.Element("Font").Attribute("FontColor").Value);
            textColor = ColorHelper.ConvertColorToCMYK(cc);
            fontBold = bool.Parse(ele.Element("Font").Attribute("FontBold").Value);
            fontItalic = bool.Parse(ele.Element("Font").Attribute("FontItalic").Value);
            //锚点
            ele = root.Element("Anchor");
            anchorStyle = (ele.Element("AnchorType").Value);
            var dic = AnchorName.Where(t => t.Value == anchorStyle).ToDictionary(p => p.Key, p => p.Value);
            anchorStyle = dic.First().Key;
            anchorSize=double.Parse( ele.Element("AnchorSize").Value);
            anchorsizeMM = anchorSize;
            anchorSize = act.FocusMap.ReferenceScale * anchorSize * 0.001;
            anchorLineColor = ColorHelper.GetColorByString(ele.Element("AnchorLineColor").Value);
            anchorFillColor = ColorHelper.GetColorByString(ele.Element("AnchorFillColor").Value);
            dic = LineStyle.Where(t => t.Value == ele.Element("AnchorLineStyle").Value).ToDictionary(p => p.Key, p => p.Value);
            anchorLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
            dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
            anchorFillStyle = (esriSimpleFillStyle)System.Enum.Parse(typeof(esriSimpleFillStyle), dic.First().Key);
            anchorLinewidth = double.Parse(ele.Element("AnchorLineWidth").Value);
            //背景
            ele = root.Element("Content");
            lableGeometry =(ele.Element("TextType").Value);
            dic = ExtentName.Where(t => t.Value == lableGeometry).ToDictionary(p => p.Key, p => p.Value);
            lableGeometry = dic.First().Key;
            fillColor = ColorHelper.GetColorByString(ele.Element("FillColor").Value);
            fillLineColor = ColorHelper.GetColorByString(ele.Element("TextLineColor").Value);
            dic = LineStyle.Where(t => t.Value == ele.Element("TextLineType").Value).ToDictionary(p => p.Key, p => p.Value);
            fillLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
            fillLinewidth = double.Parse(ele.Element("TextLineWidth").Value);
            dic = FillStyle.Where(t => t.Value == ele.Element("FillStlye").Value).ToDictionary(p => p.Key, p => p.Value);
            fillStyle = (esriSimpleFillStyle)System.Enum.Parse(typeof(esriSimpleFillStyle), dic.First().Key);
                
            //连接线
            ele = root.Element("ConnectLine");
            string ls = ele.Element("LineType").Value;
            dic = LineStyle.Where(t => t.Value == ls).ToDictionary(p => p.Key, p => p.Value); 
            lineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
            linewidth =double.Parse(ele.Element("LineWidth").Value);
            lineColor = ColorHelper.GetColorByString(ele.Element("LineColor").Value);
            labelLineLens =double.Parse(ele.Element("ConnectLens").Value);
            labelLineLensMM = labelLineLens;
            labelLineLens = act.FocusMap.ReferenceScale * labelLineLens * 0.001;
            //分割线
            ele = root.Element("SepLine");
            sepLineColor = ColorHelper.GetColorByString(ele.Element("SepLineColor").Value); ;
            dic = LineStyle.Where(t => t.Value == ele.Element("SepLineType").Value).ToDictionary(p => p.Key, p => p.Value);
            sepLineStyle = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key);
            sepLinewidth = double.Parse(ele.Element("SepLineWidth").Value); ;
            sepLineInterval = double.Parse(ele.Element("SepInterval").Value); ; ;
            checkSep = bool.Parse(ele.Attribute("check").Value) ;
                
            #endregion
            #endregion
            double anchorDx = 0;
            center = ct;
            switch (anchorStyle)
            {
                case "Circle":
                    CreateCircle(ct, anchorSize * 0.5);
                    anchorDx = anchorSize * 0.5;
                    break;
                case "Trangle":
                    CreateTrangle(ct, anchorSize);
                    anchorDx = anchorSize * Math.Pow(3, 0.5) * 0.5;
                    break;
                case "TrangleLine":
                    CreateTrangleLine(ct, anchorSize);
                    break;
                case "HoraLine":
                    CreateHoralLine(ct, anchorSize);
                    break;
                case "Rectangle":
                    CreateRectangle(ct, anchorSize);
                    anchorDx = anchorSize * 0.5;
                    break;
                case "None":
                    CreateCircle(ct, anchorSize * 0.5);
                    anchorDx = anchorSize * 0.5;
                    break;
                default:
                    break;
            } 
            anchorDx = 0;
            SimpleLineSymbolClass slsLine = new SimpleLineSymbolClass();
            slsLine.Width = linewidth * 2.83;
            slsLine.Style = lineStyle;
            slsLine.Color = lineColor;

            SimpleLineSymbolClass slsSepLine = new SimpleLineSymbolClass();
            slsSepLine.Width = sepLinewidth * 2.83;
            slsSepLine.Style = sepLineStyle;
            slsSepLine.Color = sepLineColor;

            SimpleFillSymbolClass sfsFill = new SimpleFillSymbolClass();
            sfsFill.Color = fillColor;
            sfsFill.Style = fillStyle;
            sfsFill.Outline = new SimpleLineSymbolClass() {
                Width = fillLinewidth * 2.83,
                Style=fillLineStyle,
                Color=fillLineColor
            };

            SimpleFillSymbolClass sfsAnchor = new SimpleFillSymbolClass();
            sfsAnchor.Color =anchorFillColor;
            sfsAnchor.Style = anchorFillStyle;
            sfsAnchor.Outline = new SimpleLineSymbolClass()
            {
                Width = anchorLinewidth * 2.83,
                Style = anchorLineStyle,
                Color = anchorLineColor
            };
            if (anchorStyle == "None")//特殊处理
            {
                sfsAnchor.Style = esriSimpleFillStyle.esriSFSNull;
                sfsAnchor.Outline = new SimpleLineSymbolClass()
                {
                    Width = anchorLinewidth * 2.83,
                     Color = anchorLineColor,
                    Style = esriSimpleLineStyle.esriSLSNull
                };
            }

            line1 = new LineElementClass();
            line1.Symbol = slsLine;
            PolylineClass polyline = new PolylineClass();
            polyline.AddPoint(new PointClass { X = center.X + anchorDx, Y = center.Y });
            polyline.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
            (line1 as IElement).Geometry = polyline;
            geoLine1 = polyline;
            //
            line2 = new LineElementClass();
            line2.Symbol = slsLine;
            PolylineClass polyline1 = new PolylineClass();
            polyline1.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
            polyline1.AddPoint(new PointClass { X = center.X + 35 + labelLineLens, Y = center.Y });
            (line2 as IElement).Geometry = polyline1;
            geoLine2 = polyline1;
          


            //创建文字
            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = textColor, 
            };
            txtElement = new TextElementClass();
            txtGeometry = ct;
            txtElement.Symbol = pTextSymbol;
            (txtElement as ISymbolCollectionElement).FontName = fontName;
            (txtElement as ISymbolCollectionElement).Size = fontsize;
            (txtElement as ISymbolCollectionElement).Bold = fontBold;
            (txtElement as ISymbolCollectionElement).Italic = fontItalic;

            (txtElement as IElement).Geometry = txtGeometry;
            txtElement.Text = text;
            IPolygon outline = new PolygonClass();
            (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);

            IEnvelope enText = outline.Envelope;
            (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
            (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            // if (text.Contains("\r") || text.Contains("\n"))
            // (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVABottom;
           
            //创建内容形状

            //创建内容形状
            double orginWidth, orginHeight = 0;
            double orginTxtWidth, orginTxtHeight = 0;
            orginWidth = geoDic[lableGeometry].Envelope.Width;
            orginHeight = geoDic[lableGeometry].Envelope.Height;
            orginTxtWidth = geoTextDic[lableGeometry].Envelope.Width;
            orginTxtHeight = geoTextDic[lableGeometry].Envelope.Height;
            double txtHeight = enText.Height;
            double txtWidth = enText.Width;
            if (txtWidth / txtHeight > orginTxtWidth / orginTxtHeight)
            {
                extentWidth = txtWidth / orginTxtWidth * orginWidth;
                extentHeight = txtHeight / orginTxtHeight * orginHeight;
            }
            else
            {
                if (lableGeometry.ToLower().Contains("cloud"))
                {
                    extentHeight = txtHeight * orginHeight / orginTxtHeight;
                    // extentWidth = extentHeight / orginHeight * orginWidth;
                    extentWidth = extentHeight / orginHeight * orginWidth;
                }
                else
                {
                    extentHeight = txtHeight / orginTxtHeight * orginHeight;
                    extentWidth = txtWidth / orginTxtWidth * orginWidth;
                }
            }
         
            CreateLabelExtent();
              //
            sepline = new LineElementClass();
            sepline.Symbol = slsSepLine;
            PolylineClass polyline2 = new PolylineClass();
            sepLineIntervalMM=sepLineInterval*GApplication.Application.ActiveView.FocusMap.ReferenceScale*0.001;
            polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM, Y = center.Y });
            polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineIntervalMM + geoPolygon.Envelope.Width - 2*sepLineIntervalMM, Y = center.Y });
            (sepline as IElement).Geometry = polyline2;
            geosepLine=polyline2;

            (polygon as IFillShapeElement).Symbol = sfsFill;
            if (anchorEle != null)
            {
                if (anchorEle is IFillShapeElement)
                {
                    (anchorEle as IFillShapeElement).Symbol = sfsAnchor;
                }
                if (anchorEle is ILineElement)
                {
                    (anchorEle as ILineElement).Symbol = sfsAnchor.Outline;
                }
                gc.UpdateElement(anchorEle);
            }
            gc.AddElement(line1 as IElement, 0);
            gc.AddElement(line2 as IElement, 0);
            gc.AddElement(polygon as IElement, 0);
            gc.AddElement(txtElement as IElement, 0);
            if(checkSep)
            { 
                gc.AddElement(sepline as IElement, 0);
            }
            center = ct;
                 

        }
        List<AttrLabelInfos> attributeLbInfos = new List<AttrLabelInfos>();
        /// <summary>
        /// 3.属性标注
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="queryGeo"></param>
        /// <returns></returns>
        public bool CreateAttributeLable(IPoint ct, IGeometry queryGeo)
        {
            attributeLbInfos.Clear();
            lbType = LabelType.AttrLabel;
            gc = GraphicsLayer as IGraphicsContainer;
            try
            {
                string text = string.Empty;
                string textDown = string.Empty;
                string val = AttrLableText(queryGeo, ref ct, ref textDown);
                if (val != "")
                    text = val;

                if (text == "")
                    return false;

                anchorEle = null;
                anchorGeo = null;
                line1 = null;
                geoLine1 = null;
                line2 = null;
                geoLine2 = null;
                polygon = null;
                geoPolygon = null;

                txtElement = null;
                txtGeometry = null;

                sepline = null;
                geosepLine = null;


                double anchorDx = 0;
                center = ct;
                switch (anchorStyle)
                {
                    case "Circle":
                        CreateCircle(ct, anchorSize * 0.5);
                        anchorDx = anchorSize * 0.5;
                        break;
                    case "Trangle":
                        CreateTrangle(ct, anchorSize);
                        anchorDx = anchorSize * Math.Pow(3, 0.5) * 0.5;
                        break;
                    case "TrangleLine":
                        CreateTrangleLine(ct, anchorSize);
                        break;
                    case "HoraLine":
                        CreateHoralLine(ct, anchorSize);
                        break;
                    case "Rectangle":
                        CreateRectangle(ct, anchorSize);
                        anchorDx = anchorSize * 0.5;
                        break;
                    case "None":
                        CreateCircle(ct, anchorSize * 0.5);
                        anchorDx = anchorSize * 0.5;
                        break;
                    default:

                        break;
                }
                anchorDx = 0;
                SimpleLineSymbolClass slsLine = new SimpleLineSymbolClass();
                slsLine.Width = linewidth * 2.83;
                slsLine.Style = lineStyle;
                slsLine.Color = lineColor;

                SimpleLineSymbolClass slsSepLine = new SimpleLineSymbolClass();
                slsSepLine.Width = sepLinewidth * 2.83;
                slsSepLine.Style = sepLineStyle;
                slsSepLine.Color = sepLineColor;

                SimpleFillSymbolClass sfsFill = new SimpleFillSymbolClass();
                sfsFill.Color = fillColor;
                sfsFill.Style = fillStyle;
                sfsFill.Outline = new SimpleLineSymbolClass()
                {
                    Width = fillLinewidth * 2.83,
                    Style = fillLineStyle,
                    Color = fillLineColor
                };

                SimpleFillSymbolClass sfsAnchor = new SimpleFillSymbolClass();
                sfsAnchor.Color = anchorFillColor;
                sfsAnchor.Outline = new SimpleLineSymbolClass()
                {
                    Width = anchorLinewidth * 2.83,
                    Style = anchorLineStyle,
                    Color = anchorLineColor
                };
                if (anchorStyle == "None")//特殊处理
                {
                    sfsAnchor.Style = esriSimpleFillStyle.esriSFSNull;
                    sfsAnchor.Outline = new SimpleLineSymbolClass()
                    {
                        Width = anchorLinewidth * 2.83,
                        Color = anchorLineColor,
                        Style = esriSimpleLineStyle.esriSLSNull
                    };
                }
                line1 = new LineElementClass();
                line1.Symbol = slsLine;
                PolylineClass polyline = new PolylineClass();
                polyline.AddPoint(new PointClass { X = center.X + anchorDx, Y = center.Y });
                polyline.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                (line1 as IElement).Geometry = polyline;
                geoLine1 = polyline;
                //
                line2 = new LineElementClass();
                line2.Symbol = slsLine;
                PolylineClass polyline1 = new PolylineClass();
                polyline1.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                polyline1.AddPoint(new PointClass { X = center.X + 35 + labelLineLens, Y = center.Y });
                (line2 as IElement).Geometry = polyline1;
                geoLine2 = polyline1;
                //创建文字
                ITextSymbol pTextSymbol = new TextSymbolClass()
                {
                    Color = textColor,
                };

                double txtStep = 0;
                txtElement = new TextElementClass();
                txtGeometry = ct;
                if (checkSep)
                {
                   // txtInterval = 0.75;
                    txtStep = txtInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001;
                    txtGeometry = new PointClass { X = ct.X, Y = ct.Y + txtStep };
                }
                txtElement.Symbol = pTextSymbol;
                (txtElement as ISymbolCollectionElement).FontName = fontName;
                (txtElement as ISymbolCollectionElement).Size = fontsize;
                (txtElement as ISymbolCollectionElement).Bold = fontBold;
                (txtElement as ISymbolCollectionElement).Italic = fontItalic;
                (txtElement as IElement).Geometry = txtGeometry;
                txtElement.Text = text;
                IPolygon outline = new PolygonClass();
                (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);
                IEnvelope enText = outline.Envelope;
                (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                switch (fontStyle)
                {
                    case "居中":
                        (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                        break;
                    case "靠右":
                        (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHARight;
                        break;
                    case "靠左":
                        (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
                        break;
                    default:
                        break;

                }
                //(txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                (txtElement as ISymbolCollectionElement).Bold = fontBold;
                (txtElement as ISymbolCollectionElement).Underline = fontunderLine;
                (txtElement as ISymbolCollectionElement).Italic = fontItalic;
                double txtHeight = enText.Height;
                double txtWidth = enText.Width;
                //下标文字
                if (checkSep)
                {
                    (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVABottom;
                    txtElementDown = new TextElementClass();

                    txtElementDown.Symbol = new TextSymbolClass()
                    {
                        Color = textColorDown,
                    }; ;
                    (txtElementDown as ISymbolCollectionElement).FontName = fontNameDown;
                    (txtElementDown as ISymbolCollectionElement).Size = fontsizeDown;
                    (txtElementDown as ISymbolCollectionElement).Bold = fontBoldDown;
                    (txtElementDown as ISymbolCollectionElement).Italic = fontItalicDown;
                    (txtElementDown as IElement).Geometry = new PointClass { X = ct.X, Y = ct.Y - txtStep };
                    txtElementDown.Text = textDown;
                    IPolygon outline1 = new PolygonClass();
                    (txtElementDown as IElement).QueryOutline(act.ScreenDisplay, outline1);
                    (txtElementDown as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVATop;
                    (txtElementDown as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                    switch (fontStyleDown)
                    {
                        case "居中":
                            (txtElementDown as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                            break;
                        case "靠右":
                            (txtElementDown as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHARight;
                            break;
                        case "靠左":
                            (txtElementDown as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
                            break;
                        default:
                            break;

                    }
                    IEnvelope enText1 = outline1.Envelope;
                    if (txtWidth < enText1.Width)
                    {
                        txtWidth = enText1.Width;
                    }
                    txtHeight = 2 * Math.Max(txtHeight, enText1.Height) + txtStep;
                    //txtHeight += enText1.Height + txtStep;
                }
                //创建内容形状
                double orginWidth, orginHeight = 0;
                double orginTxtWidth, orginTxtHeight = 0;
                orginWidth = geoDic[lableGeometry].Envelope.Width;
                orginHeight = geoDic[lableGeometry].Envelope.Height;
                orginTxtWidth = geoTextDic[lableGeometry].Envelope.Width;
                orginTxtHeight = geoTextDic[lableGeometry].Envelope.Height;

                if (txtWidth / txtHeight > orginTxtWidth / orginTxtHeight)
                {
                    extentWidth = txtWidth / orginTxtWidth * orginWidth;
                    extentHeight = txtHeight / orginTxtHeight * orginHeight;
                }
                else
                {
                    if (lableGeometry.ToLower().Contains("cloud"))
                    {
                        extentHeight = txtHeight * orginHeight / orginTxtHeight;
                        // extentWidth = extentHeight / orginHeight * orginWidth;
                        extentWidth = extentHeight / orginHeight * orginWidth;
                    }
                    else
                    {
                        extentHeight = txtHeight / orginTxtHeight * orginHeight;
                        extentWidth = txtWidth / orginTxtWidth * orginWidth;
                    }
                }


                CreateLabelExtent();
                //
                sepline = new LineElementClass();
                sepline.Symbol = slsSepLine;
                PolylineClass polyline2 = new PolylineClass();
                polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001, Y = center.Y });
                polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + geoPolygon.Envelope.Width - sepLineInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001, Y = center.Y });
                (sepline as IElement).Geometry = polyline2;
                geosepLine = polyline2;

                (polygon as IFillShapeElement).Symbol = sfsFill;
                if (anchorEle != null)
                {
                    if (anchorEle is IFillShapeElement)
                    {
                        (anchorEle as IFillShapeElement).Symbol = sfsAnchor;
                    }
                    if (anchorEle is ILineElement)
                    {
                        (anchorEle as ILineElement).Symbol = sfsAnchor.Outline;
                    }
                    gc.UpdateElement(anchorEle);
                }
                gc.AddElement(line1 as IElement, 0);
                gc.AddElement(line2 as IElement, 0);
                gc.AddElement(polygon as IElement, 0);
                gc.AddElement(txtElement as IElement, 0);
                if (checkSep)
                {
                    gc.AddElement(sepline as IElement, 0);
                    gc.AddElement(txtElementDown as IElement, 0);
                }
                center = ct;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;

        }

        private void CreateOthersAttriLable()
        {

            bool batch =false;
            bool.TryParse(LabelRule["批量"].ToString(), out batch);
            if(!batch)
               return;
            try
            {
              
                foreach (var l in attributeLbInfos)
                {
                    string text = string.Empty;
                    string textDown = l.TextDown;
                    IPoint ct = l.AnchorPoint;
                    string val = l.TextUp;
                    if (val != "")
                        text = val;

                    if (text == "")
                        continue;

                    anchorEle = null;
                    anchorGeo = null;
                    line1 = null;
                    geoLine1 = null;
                    line2 = null;
                    geoLine2 = null;
                    polygon = null;
                    geoPolygon = null;

                    txtElement = null;
                    txtGeometry = null;

                    sepline = null;
                    geosepLine = null;


                    double anchorDx = 0;
                    center = ct;
                    switch (anchorStyle)
                    {
                        case "Circle":
                            CreateCircle(ct, anchorSize * 0.5);
                            anchorDx = anchorSize * 0.5;
                            break;
                        case "Trangle":
                            CreateTrangle(ct, anchorSize);
                            anchorDx = anchorSize * Math.Pow(3, 0.5) * 0.5;
                            break;
                        case "TrangleLine":
                            CreateTrangleLine(ct, anchorSize);
                            break;
                        case "HoraLine":
                            CreateHoralLine(ct, anchorSize);
                            break;
                        case "Rectangle":
                            CreateRectangle(ct, anchorSize);
                            anchorDx = anchorSize * 0.5;
                            break;
                        case "None":
                            CreateCircle(ct, anchorSize * 0.5);
                            anchorDx = anchorSize * 0.5;
                            break;
                        default:

                            break;
                    }
                    anchorDx = 0;
                    SimpleLineSymbolClass slsLine = new SimpleLineSymbolClass();
                    slsLine.Width = linewidth * 2.83;
                    slsLine.Style = lineStyle;
                    slsLine.Color = lineColor;

                    SimpleLineSymbolClass slsSepLine = new SimpleLineSymbolClass();
                    slsSepLine.Width = sepLinewidth * 2.83;
                    slsSepLine.Style = sepLineStyle;
                    slsSepLine.Color = sepLineColor;

                    SimpleFillSymbolClass sfsFill = new SimpleFillSymbolClass();
                    sfsFill.Color = fillColor;
                    sfsFill.Style = fillStyle;
                    sfsFill.Outline = new SimpleLineSymbolClass()
                    {
                        Width = fillLinewidth * 2.83,
                        Style = fillLineStyle,
                        Color = fillLineColor
                    };

                    SimpleFillSymbolClass sfsAnchor = new SimpleFillSymbolClass();
                    sfsAnchor.Color = anchorFillColor;
                    sfsAnchor.Outline = new SimpleLineSymbolClass()
                    {
                        Width = anchorLinewidth * 2.83,
                        Style = anchorLineStyle,
                        Color = anchorLineColor
                    };
                    if (anchorStyle == "None")//特殊处理
                    {
                        sfsAnchor.Style = esriSimpleFillStyle.esriSFSNull;
                        sfsAnchor.Outline = new SimpleLineSymbolClass()
                        {
                            Width = anchorLinewidth * 2.83,
                            Color = anchorLineColor,
                            Style = esriSimpleLineStyle.esriSLSNull
                        };
                    }
                    line1 = new LineElementClass();
                    line1.Symbol = slsLine;
                    PolylineClass polyline = new PolylineClass();
                    polyline.AddPoint(new PointClass { X = center.X + anchorDx, Y = center.Y });
                    polyline.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                    (line1 as IElement).Geometry = polyline;
                    geoLine1 = polyline;
                    //
                    line2 = new LineElementClass();
                    line2.Symbol = slsLine;
                    PolylineClass polyline1 = new PolylineClass();
                    polyline1.AddPoint(new PointClass { X = center.X + 35, Y = center.Y });
                    polyline1.AddPoint(new PointClass { X = center.X + 35 + labelLineLens, Y = center.Y });
                    (line2 as IElement).Geometry = polyline1;
                    geoLine2 = polyline1;



                    //创建文字
                    ITextSymbol pTextSymbol = new TextSymbolClass()
                    {
                        Color = textColor,
                    };

                    double txtStep = 0;
                    txtElement = new TextElementClass();
                    txtGeometry = ct;
                    if (checkSep)
                    {
                        txtInterval = 0.75;
                        txtStep = txtInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001;
                        txtGeometry = new PointClass { X = ct.X, Y = ct.Y + txtStep };
                    }
                    txtElement.Symbol = pTextSymbol;
                    (txtElement as ISymbolCollectionElement).FontName = fontName;
                    (txtElement as ISymbolCollectionElement).Size = fontsize;
                    (txtElement as IElement).Geometry = txtGeometry;
                    txtElement.Text = text;
                    IPolygon outline = new PolygonClass();
                    (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);
                    IEnvelope enText = outline.Envelope;
                    (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                    (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                    (txtElement as ISymbolCollectionElement).Bold = fontBold;
                    (txtElement as ISymbolCollectionElement).Underline = fontunderLine;
                    (txtElement as ISymbolCollectionElement).Italic = fontItalic;
                    double txtHeight = enText.Height;
                    double txtWidth = enText.Width;
                    //下标文字
                    if (checkSep)
                    {
                        (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVABottom;
                        txtElementDown = new TextElementClass();

                        txtElementDown.Symbol = new TextSymbolClass()
                        {
                            Color = textColorDown,
                        }; ;
                        (txtElementDown as ISymbolCollectionElement).FontName = fontNameDown;
                        (txtElementDown as ISymbolCollectionElement).Size = fontsizeDown;

                        (txtElementDown as IElement).Geometry = new PointClass { X = ct.X, Y = ct.Y - txtStep };
                        txtElementDown.Text = textDown;
                        IPolygon outline1 = new PolygonClass();
                        (txtElementDown as IElement).QueryOutline(act.ScreenDisplay, outline1);
                        (txtElementDown as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVATop;
                        (txtElementDown as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                        IEnvelope enText1 = outline1.Envelope;
                        if (txtWidth < enText1.Width)
                        {
                            txtWidth = enText1.Width;
                        }
                        txtHeight += enText1.Height + txtStep;
                    }
                    //创建内容形状
                    double orginWidth, orginHeight = 0;
                    double orginTxtWidth, orginTxtHeight = 0;
                    orginWidth = geoDic[lableGeometry].Envelope.Width;
                    orginHeight = geoDic[lableGeometry].Envelope.Height;
                    orginTxtWidth = geoTextDic[lableGeometry].Envelope.Width;
                    orginTxtHeight = geoTextDic[lableGeometry].Envelope.Height;

                    if (txtWidth / txtHeight > orginTxtWidth / orginTxtHeight)
                    {
                        extentWidth = txtWidth / orginTxtWidth * orginWidth;
                        extentHeight = txtHeight / orginTxtHeight * orginHeight;
                    }
                    else
                    {
                        if (lableGeometry.ToLower().Contains("cloud"))
                        {
                            extentHeight = txtHeight * orginHeight / orginTxtHeight;
                            // extentWidth = extentHeight / orginHeight * orginWidth;
                            extentWidth = extentHeight / orginHeight * orginWidth;
                        }
                        else
                        {
                            extentHeight = txtHeight / orginTxtHeight * orginHeight;
                            extentWidth = txtWidth / orginTxtWidth * orginWidth;
                        }
                    }


                    CreateLabelExtent();
                    //
                    sepline = new LineElementClass();
                    sepline.Symbol = slsSepLine;
                    PolylineClass polyline2 = new PolylineClass();
                    polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + sepLineInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001, Y = center.Y });
                    polyline2.AddPoint(new PointClass { X = center.X + 35 + labelLineLens + geoPolygon.Envelope.Width - sepLineInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001, Y = center.Y });
                    (sepline as IElement).Geometry = polyline2;
                    geosepLine = polyline2;

                    (polygon as IFillShapeElement).Symbol = sfsFill;
                    if (anchorEle != null)
                    {
                        if (anchorEle is IFillShapeElement)
                        {
                            (anchorEle as IFillShapeElement).Symbol = sfsAnchor;
                        }
                        if (anchorEle is ILineElement)
                        {
                            (anchorEle as ILineElement).Symbol = sfsAnchor.Outline;
                        }
                        gc.UpdateElement(anchorEle);
                    }
                    gc.AddElement(line1 as IElement, 0);
                    gc.AddElement(line2 as IElement, 0);
                    gc.AddElement(polygon as IElement, 0);
                    gc.AddElement(txtElement as IElement, 0);
                    if (checkSep)
                    {
                        gc.AddElement(sepline as IElement, 0);
                        gc.AddElement(txtElementDown as IElement, 0);
                    }
                    center = ct;
                    //平移
                    MoveLabelLine(new PointClass { X = ct.X+7.5*GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001, Y = ct.Y });
                    //添加到地图
                    try
                    {
                        IGroupElement group = new GroupElementClass();
                        (anchorEle as IElementProperties).Name = "锚点";
                        (anchorEle as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                        (line1 as IElementProperties).Name = "锚线";
                        (line1 as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                        (line2 as IElementProperties).Name = "连接线";
                        (line2 as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                        (polygon as IElementProperties).Name = "内容框";
                        (polygon as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                        (txtElement as IElementProperties).Name = "文本";
                        (txtElement as IElementProperties).CustomProperty = Guid.NewGuid().ToString();

                        gc.MoveElementToGroup(line1 as IElement, group);
                        gc.MoveElementToGroup(line2 as IElement, group);
                        gc.MoveElementToGroup(anchorEle as IElement, group);
                        gc.MoveElementToGroup(polygon as IElement, group);

                        if (checkSep)
                        {
                            (sepline as IElementProperties).Name = "分割线";
                            (sepline as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                            gc.MoveElementToGroup(sepline as IElement, group);
                        }
                        if (txtElementDown != null)
                        {
                            (txtElementDown as IElementProperties).Name = "下标文本";
                            (txtElementDown as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                            gc.MoveElementToGroup(txtElementDown as IElement, group);
                        }
                        gc.MoveElementToGroup(txtElement as IElement, group);
                        anchorEle = null;
                        #region 标注信息
                        LabelJson json = new LabelJson
                        {
                            AnchorSize = anchorsizeMM,
                            AnchorType = anchorStyle,
                            ConnectLens = labelLineLensMM,
                            TextType = lableGeometry

                        };

                        #endregion
                        string jsonText = LabelClass.GetJsonText(json);
                        (group as IElementProperties).Name = jsonText;
                        (group as IElementProperties).Type = lbType.ToString();
                        LabelType r = (LabelType)1;//女
                        gc.AddElement(group as IElement, 0);
                        anchorEle = null;
                        anchorGeo = null;
                        line1 = null;
                        geoLine1 = null;
                        line2 = null;
                        geoLine2 = null;
                        polygon = null;
                        geoPolygon = null;
                        center = null;
                        txtElement = null;
                        txtGeometry = null;
                        txtElementDown = null;
                        txtInterval = 0;

                        act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                        MessageBox.Show(ex.Message);
                    }
                }
                attributeLbInfos.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message);
                
            }
        }

        public void MoveLabelLine(IPoint ct)
        {

            try
            {

                if (center == null)
                    return;

                //1
                ILine line = new LineClass();
                line.FromPoint = center;
                line.ToPoint = ct;
                //旋转锚点

                var trans = (anchorGeo as IClone).Clone() as ITransform2D;
                trans.Rotate(center, line.Angle);
                anchorEle.Geometry = trans as IGeometry;
                gc.UpdateElement(anchorEle as IElement);
                //旋转锚线
                ITransform2D trans1 = (geoLine1 as IClone).Clone() as ITransform2D;
                trans1.Rotate(center, line.Angle);
                PolylineClass polyline = new PolylineClass();
                IPoint pptfrom = (trans1 as IPolyline).FromPoint;
                polyline.AddPoint(new PointClass { X = pptfrom.X, Y = pptfrom.Y });
                polyline.AddPoint(new PointClass { X = ct.X, Y = ct.Y });

                (line1 as IElement).Geometry = polyline as IGeometry;
                gc.UpdateElement(line1 as IElement);

                //旋转内容窗体
                #region
                //
                if (Math.Abs(line.Angle) < Math.PI / 2)
                {
                    if (Math.Abs(line.Angle) > Math.PI / 2 * 0.8)//72度
                    {
                        double reg = line.Angle / Math.Abs(line.Angle);
                        PolylineClass polyline1 = new PolylineClass();
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y + labelLineLens * reg });
                        (line2 as IElement).Geometry = polyline1 as IGeometry;
                        gc.UpdateElement(line2 as IElement);

                        double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMax + geoPolygon.Envelope.XMin) * 0.5;
                        double dy = polyline1.ToPoint.Y - (reg < 0 ? (geoPolygon.Envelope.YMax) : (geoPolygon.Envelope.YMin));
                        ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                        polygonTran.Move(dx, dy);
                        (polygon as IElement).Geometry = polygonTran as IGeometry;
                        gc.UpdateElement(polygon as IElement);
                        if (checkSep)
                        {
                            ITransform2D sepLineTran = (geosepLine as IClone).Clone() as ITransform2D;
                            IPoint lineCt = new PointClass();
                            (geosepLine as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                            dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                            dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;

                            sepLineTran.Move(dx, dy);
                            (sepline as IElement).Geometry = sepLineTran as IGeometry;
                            gc.UpdateElement(sepline as IElement);
                        }


                    }
                    else
                    {
                        //2
                        PolylineClass polyline1 = new PolylineClass();
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                        polyline1.AddPoint(new PointClass { X = ct.X + labelLineLens, Y = ct.Y });
                        (line2 as IElement).Geometry = polyline1 as IGeometry;
                        gc.UpdateElement(line2 as IElement);

                        double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMin);
                        double dy = polyline1.ToPoint.Y - (geoPolygon.Envelope.YMin + geoPolygon.Envelope.YMax) * 0.5;
                        ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                        polygonTran.Move(dx, dy);
                        (polygon as IElement).Geometry = polygonTran as IGeometry;
                        gc.UpdateElement(polygon as IElement);
                        if (checkSep)
                        {
                            IPoint lineCt = new PointClass();
                            (geosepLine as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                            dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                            dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                            //dx = polyline1.ToPoint.X+sepLineIntervalMM - geosepLine.Envelope.XMin;
                            //dy = polyline1.ToPoint.Y - (geosepLine.Envelope.YMin + geosepLine.Envelope.YMax) * 0.5;

                            ITransform2D sepLineTran = (geosepLine as IClone).Clone() as ITransform2D;
                            sepLineTran.Move(dx, dy);
                            (sepline as IElement).Geometry = sepLineTran as IGeometry;
                            gc.UpdateElement(sepline as IElement);
                        }

                    }
                }
                else
                {
                    if (Math.Abs(line.Angle) < Math.PI / 2 + Math.PI / 2 * 0.2)//72度
                    {
                        double reg = line.Angle / Math.Abs(line.Angle);
                        PolylineClass polyline1 = new PolylineClass();
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y + labelLineLens * reg });
                        (line2 as IElement).Geometry = polyline1 as IGeometry;
                        gc.UpdateElement(line2 as IElement);


                        double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMax + geoPolygon.Envelope.XMin) * 0.5;
                        double dy = polyline1.ToPoint.Y - (reg < 0 ? (geoPolygon.Envelope.YMax) : (geoPolygon.Envelope.YMin));
                        ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                        polygonTran.Move(dx, dy);
                        (polygon as IElement).Geometry = polygonTran as IGeometry;
                        gc.UpdateElement(polygon as IElement);
                        if (checkSep)
                        {
                            IPoint lineCt = new PointClass();
                            (geosepLine as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                            dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                            dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;

                            ITransform2D sepLineTran = (geosepLine as IClone).Clone() as ITransform2D;
                            sepLineTran.Move(dx, dy);
                            (sepline as IElement).Geometry = sepLineTran as IGeometry;
                            gc.UpdateElement(sepline as IElement);
                        }

                    }
                    else
                    {
                        //2
                        PolylineClass polyline1 = new PolylineClass();
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                        polyline1.AddPoint(new PointClass { X = ct.X - labelLineLens, Y = ct.Y });
                        (line2 as IElement).Geometry = polyline1 as IGeometry;
                        gc.UpdateElement(line2 as IElement);


                        double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMin) - geoPolygon.Envelope.Width;
                        double dy = polyline1.ToPoint.Y - (geoPolygon.Envelope.YMin + geoPolygon.Envelope.YMax) * 0.5;
                        ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                        polygonTran.Move(dx, dy);
                        (polygon as IElement).Geometry = polygonTran as IGeometry;
                        gc.UpdateElement(polygon as IElement);
                        if (checkSep)
                        {
                            IPoint lineCt = new PointClass();
                            (geosepLine as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                            dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                            dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                            //dx = polyline1.ToPoint.X - geosepLine.Envelope.Width+sepLineIntervalMM - (geosepLine.Envelope.XMin);
                            //dy = polyline1.ToPoint.Y - (geosepLine.Envelope.YMin + geosepLine.Envelope.YMax) * 0.5;

                            ITransform2D sepLineTran = (geosepLine as IClone).Clone() as ITransform2D;
                            sepLineTran.Move(dx, dy);
                            (sepline as IElement).Geometry = sepLineTran as IGeometry;
                            gc.UpdateElement(sepline as IElement);
                        }

                    }
                }
                #endregion

                IPoint txtTopoint = ((polygon as IElement).Geometry as IArea).Centroid;

                (txtElement as IElement).Geometry = new PointClass { X = txtTopoint.X, Y = txtTopoint.Y + txtInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001 };
                if ((txtElement as ISymbolCollectionElement).VerticalAlignment == esriTextVerticalAlignment.esriTVACenter)
                {
                     //纠正中心点位置
                    IPolygon outline = new PolygonClass();
                    (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);
                    IPoint txtCenter = (outline as IArea).Centroid;
                    (txtElement as ITransform2D).Move(txtTopoint.X - txtCenter.X, txtTopoint.Y - txtCenter.Y);
                }
                
                
                gc.UpdateElement(txtElement as IElement);
                if (txtElementDown != null)
                {
                    (txtElementDown as IElement).Geometry = new PointClass { X = txtTopoint.X, Y = txtTopoint.Y - txtInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001 };
                    gc.UpdateElement(txtElementDown as IElement);
                }
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                
            }

        }
        public void MoveLabelLine8Dir(IPoint ct)
        {
            try
            {

                if (center == null)
                    return;

                //1
                ILine line = new LineClass();
                line.FromPoint = center;
                line.ToPoint = ct;
                double correctAngle = 0;
                //旋转锚点
                if (Math.PI / 8 >= line.Angle && -Math.PI / 8 <= line.Angle)
                {
                    correctAngle = 0;
                }
                else if (Math.PI / 8 <= line.Angle && Math.PI / 8 + Math.PI / 4 >= line.Angle)
                {
                    correctAngle = Math.PI / 4;
                }
                else if (Math.PI / 8 + Math.PI / 2 >= line.Angle && Math.PI / 8 + Math.PI / 4 <= line.Angle)
                {
                    correctAngle = Math.PI / 2;
                }
                else if (Math.PI / 8 + Math.PI / 2 <= line.Angle && Math.PI / 8 + 3 * Math.PI / 4 >= line.Angle)
                {
                    correctAngle = Math.PI / 2 + Math.PI / 4;
                }
                else if ((Math.PI >= line.Angle && Math.PI / 8 + 3 * Math.PI / 4 <= line.Angle) || (-Math.PI <= line.Angle && -Math.PI / 8 - 3 * Math.PI / 4 >= line.Angle))
                {
                    correctAngle = Math.PI;
                }
                else if (-Math.PI / 8 - Math.PI / 2 >= line.Angle && -Math.PI / 8 - 3 * Math.PI / 4 <= line.Angle)
                {
                    correctAngle = -Math.PI / 2 - Math.PI / 4;
                }
                else if (-Math.PI / 8 - Math.PI / 2 <= line.Angle && -Math.PI / 8 - Math.PI / 4 >= line.Angle)
                {
                    correctAngle = -Math.PI / 2;
                }
                else if (-Math.PI / 8 >= line.Angle && -Math.PI / 8 - Math.PI / 4 <= line.Angle)
                {
                    correctAngle = -Math.PI / 4;
                }
                (line as ITransform2D).Rotate(center, -line.Angle);
                (line as ITransform2D).Rotate(center, correctAngle);
                ct = line.ToPoint;

                var trans = (anchorGeo as IClone).Clone() as ITransform2D;
                trans.Rotate(center, line.Angle);
                anchorEle.Geometry = trans as IGeometry;
                gc.UpdateElement(anchorEle as IElement);
                //旋转锚线


                ITransform2D trans1 = (geoLine1 as IClone).Clone() as ITransform2D;
                trans1.Rotate(center, line.Angle);
                PolylineClass polyline = new PolylineClass();
                IPoint pptfrom = (trans1 as IPolyline).FromPoint;
                polyline.AddPoint(new PointClass { X = pptfrom.X, Y = pptfrom.Y });
                polyline.AddPoint(new PointClass { X = ct.X, Y = ct.Y });

                (line1 as IElement).Geometry = polyline as IGeometry;
                gc.UpdateElement(line1 as IElement);

                //旋转内容窗体
                #region
                //
                if (Math.Abs(line.Angle) < Math.PI / 2)
                {
                    if (Math.Abs(line.Angle) > Math.PI / 2 * 0.8)//72度
                    {
                        double reg = line.Angle / Math.Abs(line.Angle);
                        PolylineClass polyline1 = new PolylineClass();
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y + labelLineLens * reg });
                        (line2 as IElement).Geometry = polyline1 as IGeometry;
                        gc.UpdateElement(line2 as IElement);

                        double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMax + geoPolygon.Envelope.XMin) * 0.5;
                        double dy = polyline1.ToPoint.Y - (reg < 0 ? (geoPolygon.Envelope.YMax) : (geoPolygon.Envelope.YMin));
                        ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                        polygonTran.Move(dx, dy);
                        (polygon as IElement).Geometry = polygonTran as IGeometry;
                        gc.UpdateElement(polygon as IElement);
                        if (checkSep)
                        {
                            IPoint lineCt = new PointClass();
                            (geosepLine as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                            dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                            dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                            ITransform2D sepLineTran = (geosepLine as IClone).Clone() as ITransform2D;
                            sepLineTran.Move(dx, dy);
                            (sepline as IElement).Geometry = sepLineTran as IGeometry;
                            gc.UpdateElement(sepline as IElement);

                        }
                    }
                    else
                    {
                        //2
                        PolylineClass polyline1 = new PolylineClass();
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                        polyline1.AddPoint(new PointClass { X = ct.X + labelLineLens, Y = ct.Y });
                        (line2 as IElement).Geometry = polyline1 as IGeometry;
                        gc.UpdateElement(line2 as IElement);

                        double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMin);
                        double dy = polyline1.ToPoint.Y - (geoPolygon.Envelope.YMin + geoPolygon.Envelope.YMax) * 0.5;
                        ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                        polygonTran.Move(dx, dy);
                        (polygon as IElement).Geometry = polygonTran as IGeometry;
                        gc.UpdateElement(polygon as IElement);
                        if (checkSep)
                        {
                            IPoint lineCt = new PointClass();
                            (geosepLine as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                            dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                            dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                            ITransform2D sepLineTran = (geosepLine as IClone).Clone() as ITransform2D;
                            sepLineTran.Move(dx, dy);
                            (sepline as IElement).Geometry = sepLineTran as IGeometry;
                            gc.UpdateElement(sepline as IElement);

                        }
                    }
                }
                else
                {
                    if (Math.Abs(line.Angle) < Math.PI / 2 + Math.PI / 2 * 0.2)//72度
                    {
                        double reg = line.Angle / Math.Abs(line.Angle);
                        PolylineClass polyline1 = new PolylineClass();
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y + labelLineLens * reg });
                        (line2 as IElement).Geometry = polyline1 as IGeometry;
                        gc.UpdateElement(line2 as IElement);


                        double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMax + geoPolygon.Envelope.XMin) * 0.5;
                        double dy = polyline1.ToPoint.Y - (reg < 0 ? (geoPolygon.Envelope.YMax) : (geoPolygon.Envelope.YMin));
                        ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                        polygonTran.Move(dx, dy);
                        (polygon as IElement).Geometry = polygonTran as IGeometry;
                        gc.UpdateElement(polygon as IElement);
                        if (checkSep)
                        {
                            IPoint lineCt = new PointClass();
                            (geosepLine as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                            dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                            dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                            ITransform2D sepLineTran = (geosepLine as IClone).Clone() as ITransform2D;
                            sepLineTran.Move(dx, dy);
                            (sepline as IElement).Geometry = sepLineTran as IGeometry;
                            gc.UpdateElement(sepline as IElement);

                        }
                    }
                    else
                    {
                        //2
                        PolylineClass polyline1 = new PolylineClass();
                        polyline1.AddPoint(new PointClass { X = ct.X, Y = ct.Y });
                        polyline1.AddPoint(new PointClass { X = ct.X - labelLineLens, Y = ct.Y });
                        (line2 as IElement).Geometry = polyline1 as IGeometry;
                        gc.UpdateElement(line2 as IElement);


                        double dx = polyline1.ToPoint.X - (geoPolygon.Envelope.XMin) - geoPolygon.Envelope.Width;
                        double dy = polyline1.ToPoint.Y - (geoPolygon.Envelope.YMin + geoPolygon.Envelope.YMax) * 0.5;
                        ITransform2D polygonTran = (geoPolygon as IClone).Clone() as ITransform2D;
                        polygonTran.Move(dx, dy);
                        (polygon as IElement).Geometry = polygonTran as IGeometry;
                        gc.UpdateElement(polygon as IElement);
                        if (checkSep)
                        {
                            IPoint lineCt = new PointClass();
                            (geosepLine as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, lineCt);

                            dx = (polygonTran as IArea).Centroid.X - lineCt.X;
                            dy = (polygonTran as IArea).Centroid.Y - lineCt.Y;
                            ITransform2D sepLineTran = (geosepLine as IClone).Clone() as ITransform2D;
                            sepLineTran.Move(dx, dy);
                            (sepline as IElement).Geometry = sepLineTran as IGeometry;
                            gc.UpdateElement(sepline as IElement);

                        }
                    }
                }
                #endregion

                IPoint txtTopoint = ((polygon as IElement).Geometry as IArea).Centroid;

                (txtElement as IElement).Geometry = txtTopoint;
                if ((txtElement as ISymbolCollectionElement).VerticalAlignment == esriTextVerticalAlignment.esriTVACenter)
                {
                    //纠正中心点位置
                    IPolygon outline = new PolygonClass();
                    (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);
                    IPoint txtCenter = (outline as IArea).Centroid;
                    (txtElement as ITransform2D).Move(txtTopoint.X - txtCenter.X, txtTopoint.Y - txtCenter.Y);
                }
                gc.UpdateElement(txtElement as IElement);
                if (txtElementDown != null)
                {
                    (txtElementDown as IElement).Geometry = new PointClass { X = txtTopoint.X, Y = txtTopoint.Y - txtInterval * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 0.001 };
                    gc.UpdateElement(txtElementDown as IElement);
                }
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }

        }
        public void LabelLineCancel()
        {
            try
            {
                if (anchorEle != null)
                {
                    (act as IGraphicsContainer).DeleteElement(anchorEle);
                }
                if (line1 != null)
                {
                    (act as IGraphicsContainer).DeleteElement(line1 as IElement);
                }
                if (line2 != null)
                {
                    (act as IGraphicsContainer).DeleteElement(line2 as IElement);
                }
                if (polygon != null)
                {
                    (act as IGraphicsContainer).DeleteElement(polygon as IElement);
                }
                if (txtElement != null)
                {
                    (act as IGraphicsContainer).DeleteElement(txtElement as IElement);
                }
                if (txtElementDown != null)
                {
                    (act as IGraphicsContainer).DeleteElement(txtElementDown as IElement);
                }
                anchorEle = null;
                anchorGeo = null;
                line1 = null;
                geoLine1 = null;
                line2 = null;
                geoLine2 = null;
                polygon = null;
                geoPolygon = null;
                center = null;
                txtElement = null;
                txtGeometry = null;
                txtElementDown = null;
                lbType = LabelType.ConnectLine;
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }
        public void LabelLineToMap()
        {

            try
            {
                IGroupElement group = new GroupElementClass();
                (anchorEle as IElementProperties).Name = "锚点";
                (anchorEle as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                (line1 as IElementProperties).Name = "锚线";
                (line1 as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                (line2 as IElementProperties).Name = "连接线";
                (line2 as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                (polygon as IElementProperties).Name = "内容框";
                (polygon as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                (txtElement as IElementProperties).Name = "文本";
                (txtElement as IElementProperties).CustomProperty = Guid.NewGuid().ToString();

                gc.MoveElementToGroup(line1 as IElement, group);
                gc.MoveElementToGroup(line2 as IElement, group);
                gc.MoveElementToGroup(anchorEle as IElement, group);
                gc.MoveElementToGroup(polygon as IElement, group);

                if (checkSep)
                {
                    (sepline as IElementProperties).Name = "分割线";
                    (sepline as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                    gc.MoveElementToGroup(sepline as IElement, group);
                }
                if (txtElementDown != null)
                {
                    (txtElementDown as IElementProperties).Name = "下标文本";
                    (txtElementDown as IElementProperties).CustomProperty = Guid.NewGuid().ToString();
                    gc.MoveElementToGroup(txtElementDown as IElement, group);
                }
                gc.MoveElementToGroup(txtElement as IElement, group);
                anchorEle = null;
                #region 标注信息
                LabelJson json = new LabelJson
                {
                    AnchorSize = anchorsizeMM,
                    AnchorType = anchorStyle,
                    ConnectLens = labelLineLensMM,
                    TextType = lableGeometry

                };

                #endregion
                string jsonText = LabelClass.GetJsonText(json);
                (group as IElementProperties).Name = jsonText;
                (group as IElementProperties).Type = lbType.ToString();
                LabelType r = (LabelType)1;//女
                gc.AddElement(group as IElement, 0);
                anchorEle = null;
                anchorGeo = null;
                line1 = null;
                geoLine1 = null;
                line2 = null;
                geoLine2 = null;
                polygon = null;
                geoPolygon = null;
                center = null;
                txtElement = null;
                txtGeometry = null;
                txtElementDown = null;
                txtInterval = 0;

                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, act.Extent);
                if (lbType == LabelType.AttrLabel)
                {
                    CreateOthersAttriLable();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                attributeLbInfos.Clear();
                fontStyle = "居中";
                fontStyleDown = "居中";
            }
        }

        #region 锚点
        private void CreateCircle(IPoint pnt, double r)
        {
            IConstructCircularArc pConstructCircularArc = new CircularArcClass();
            pConstructCircularArc.ConstructCircle(pnt, r, false);
            ICircularArc pArc = pConstructCircularArc as ICircularArc;
            ISegment pSegment1 = pArc as ISegment; //通过ISegmentCollection构建Ring对象
            ISegmentCollection pSegCollection = new RingClass();
            object o = Type.Missing; //添加Segement对象即圆
            pSegCollection.AddSegment(pSegment1, ref o, ref o); //QI到IRing接口封闭Ring对象，使其有效
            IRing pRing1 = pSegCollection as IRing;
            pRing1.Close(); //通过Ring对象使用IGeometryCollection构建Polygon对象

            IGeometryCollection pGeometryColl = new PolygonClass();
            pGeometryColl.AddGeometry(pRing1, ref o, ref o); //构建一个CircleElement对象

            anchorEle = new PolygonElementClass();
            anchorEle.Geometry = pGeometryColl as IGeometry;
            anchorGeo = pGeometryColl as IGeometry;
            if(ls == LabelState.Create)
               gc.AddElement(anchorEle, 0);
            

        }
        private void CreateRectangle(IPoint pnt, double size)
        {
            IPoint p1 = new PointClass { X = -size * 0.5 + pnt.X, Y = -size * 0.5 + pnt.Y };
            IPoint p2 = new PointClass { X = size * 0.5 + pnt.X, Y = -size * 0.5 + pnt.Y };
            IPoint p3 = new PointClass { X = size * 0.5 + pnt.X, Y = size * 0.5 + pnt.Y };
            IPoint p4 = new PointClass { X = -size * 0.5 + pnt.X, Y = size * 0.5 + pnt.Y };

            PolygonClass polygon = new PolygonClass();
            polygon.AddPoint(p1);
            polygon.AddPoint(p2);
            polygon.AddPoint(p3);
            polygon.AddPoint(p4);
            polygon.AddPoint(p1);
            anchorGeo = polygon;
            anchorEle = new PolygonElementClass();
            anchorEle.Geometry = polygon;
            if (ls == LabelState.Create)
            gc.AddElement(anchorEle, 0);

        }
        private void CreateTrangle(IPoint pnt, double size)
        {
            IPoint p2 = new PointClass { X = size * Math.Pow(3, 0.5) * 0.5 + pnt.X, Y = size * 0.5 + pnt.Y };
            IPoint p3 = new PointClass { X = size * Math.Pow(3, 0.5) * 0.5 + pnt.X, Y = -size * 0.5 + pnt.Y };

            PolygonClass polygon = new PolygonClass();
            polygon.AddPoint(pnt);
            polygon.AddPoint(p2);
            polygon.AddPoint(p3);
            polygon.AddPoint(pnt);
            anchorGeo = polygon;
            (anchorGeo as ITransform2D).Move(-size * Math.Pow(3, 0.5) * 0.5 * 0.5, 0);
            anchorEle = new PolygonElementClass();
            anchorEle.Geometry = polygon;
            if (ls == LabelState.Create)
            gc.AddElement(anchorEle, 0);

        }
        private void CreateTrangleLine(IPoint pnt, double size)
        {

            IPoint p2 = new PointClass { X = size * Math.Pow(3, 0.5) * 0.5 + pnt.X, Y = size * 0.5 + pnt.Y };
            IPoint p3 = new PointClass { X = size * Math.Pow(3, 0.5) * 0.5 + pnt.X, Y = -size * 0.5 + pnt.Y };

            ILine line1 = new LineClass();
            line1.FromPoint = pnt; line1.ToPoint = p2;
            ILine line2 = new LineClass();
            line2.FromPoint = pnt; line2.ToPoint = p3;

            PolylineClass polyline = new PolylineClass();
            PathClass pPath = new PathClass();
            pPath.AddSegment(line1 as ISegment);
            pPath.AddSegment(line2 as ISegment);
            polyline.AddGeometry(pPath);
            polyline.Simplify();
            anchorGeo = polyline;
            anchorEle = new LineElementClass();
            anchorEle.Geometry = polyline;
            if (ls == LabelState.Create)
            gc.AddElement(anchorEle, 0);

        }
        private void CreateHoralLine(IPoint pnt, double size)
        {

            IPoint p2 = new PointClass { X = pnt.X, Y = -size * 0.5 + pnt.Y };
            IPoint p3 = new PointClass { X = pnt.X, Y = size * 0.5 + pnt.Y };

            ILine line1 = new LineClass();
            line1.FromPoint = p2; line1.ToPoint = p3;


            PolylineClass polyline = new PolylineClass();
            PathClass pPath = new PathClass();
            pPath.AddSegment(line1 as ISegment);

            polyline.AddGeometry(pPath);
            polyline.Simplify();
            anchorGeo = polyline;
            anchorEle = new LineElementClass();
            anchorEle.Geometry = polyline;
            if (ls == LabelState.Create)
            gc.AddElement(anchorEle, 0);

        }
        #endregion

        private void CreateLabelExtent()
        {
            IPolygon geoPoly = null;
            polygon = new PolygonElementClass();
            geoPoly = (geoDic[lableGeometry] as IClone).Clone() as IPolygon;
            if (lableGeometry.ToLower().Contains("circle"))
            {
                double r=Math.Max(extentWidth ,extentHeight);
                (geoPoly as ITransform2D).Scale(center,r / geoPoly.Envelope.Width, r / geoPoly.Envelope.Height);
            }
            else
            {
                (geoPoly as ITransform2D).Scale(center, extentWidth / geoPoly.Envelope.Width, extentHeight / geoPoly.Envelope.Height);
            }
            (polygon as IElement).Geometry = geoPoly;
            geoPolygon = geoPoly;
           
        }

        public static LabelJson GetLabelInfo(string jsonTxt)
        {
            try
            {
                LabelJson pieinfo = new LabelJson();
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonTxt)))
                {
                    DataContractJsonSerializer ds = new DataContractJsonSerializer(pieinfo.GetType());
                    pieinfo = (LabelJson)ds.ReadObject(stream);
                }
                return pieinfo;
            }
            catch
            {
                return null;
            }
        }
        public static string GetJsonText(LabelJson labelInfo)
        {
            string jsonText = "";
            DataContractJsonSerializer js = new DataContractJsonSerializer(labelInfo.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                js.WriteObject(ms, labelInfo);
                jsonText = Encoding.UTF8.GetString(ms.ToArray());
            }
            return jsonText;
        }
        private DataTable ReadToDataTable(string mdbFilePath, string tableName)
        {
            DataTable pDataTable = new DataTable();
            IWorkspaceFactory pWorkspaceFactory = new AccessWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(mdbFilePath, 0);
            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTTable);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            ITable pTable = null;
            while (pDataset != null)
            {
                if (pDataset.Name == tableName)
                {
                    pTable = pDataset as ITable;
                    break;
                }
                pDataset = pEnumDataset.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactory);

            if (pTable != null)
            {
                ICursor pCursor = pTable.Search(null, false);
                IRow pRow = pCursor.NextRow();
                //添加表的字段信息
                for (int i = 0; i < pRow.Fields.FieldCount; i++)
                {
                    pDataTable.Columns.Add(pRow.Fields.Field[i].Name);
                }
                //添加数据
                while (pRow != null)
                {
                    DataRow dr = pDataTable.NewRow();
                    for (int i = 0; i < pRow.Fields.FieldCount; i++)
                    {
                        object obValue = pRow.get_Value(i);
                        if (obValue != null && !Convert.IsDBNull(obValue))
                        {
                            dr[i] = pRow.get_Value(i);
                        }
                        else
                        {
                            dr[i] = "";
                        }
                    }
                    pDataTable.Rows.Add(dr);
                    pRow = pCursor.NextRow();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
            }

            return pDataTable;
        }
    }
}

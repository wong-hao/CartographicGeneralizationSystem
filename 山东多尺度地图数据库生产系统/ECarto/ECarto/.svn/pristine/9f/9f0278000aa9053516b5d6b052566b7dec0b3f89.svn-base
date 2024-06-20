using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 注记生成规则
    /// </summary>
    public class AnnoRule
    {
        /*----------------字体样式--------------------*/
        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName
        {
            get;
            set;
        }
        /// <summary>
        /// 字大（单位：毫米）
        /// </summary>
        public double FontSize
        {
            set;
            get;
        }
        /// <summary>
        /// 字体颜色
        /// </summary>
        public IColor FontColor
        {
            get;
            set;
        }
        /// <summary>
        /// 是否加粗
        /// </summary>
        public bool EnableBold
        {
            set;
            get;
        }

        /// <summary>
        /// 是否开启下划线
        /// </summary>
        public bool EnableUnderline
        {
            set;
            get;
        }

        /// <summary>
        /// 是否开启立体效果
        /// </summary>
        public bool EnableThreeDim
        {
            set;
            get;
        }

        /// <summary>
        /// 字宽
        /// </summary>
        public int CharacterWidth
        {
            set;
            get;
        }
        /*-------------------注记背景----------------------*/
        /// <summary>
        /// 是否有文字背景
        /// </summary>
        public bool HasTextBackground
        {
            set;
            get;
        }

        private IColor backgroundColor;
        /// <summary>
        /// 设置注记的背景色（注记压盖）
        /// </summary>
        public IColor BackgroundColor
        {
            set
            {
                this.HasTextBackground = true;
                this.backgroundColor = value;
            }
            get
            {
                return this.backgroundColor;
            }
        }
        /// <summary>
        /// 背景框样式（气泡：矩形\圆角\椭圆，标记符号：高速）
        /// </summary>
        public int BackgroundStyle
        {
            set;
            get;
        }
        /// <summary>
        /// 背景框边距
        /// </summary>
        public double BackgroundMargin
        {
            set;
            get;
        }
        /// <summary>
        /// 是否有背景边框
        /// </summary>
        public bool HasBackgroundBorder
        {
            set;
            get;
        }

        /// <summary>
        /// 气泡边界颜色
        /// </summary>
        public IColor BackgroundBorderColor
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡边界宽度
        /// </summary>
        public double BackgroundBorderWidth
        {
            set;
            get;
        }
        /*-----------晕圈设置-----------------*/
        /// <summary>
        /// 是否有晕圈
        /// </summary>
        public bool HasHalo
        {
            set;
            get;
        }
        /// <summary>
        /// 晕圈大小
        /// </summary>
        public double HaloSize
        {
            set;
            get;
        }
        /// <summary>
        /// 晕圈颜色
        /// </summary>
        public IColor HaloColor
        {
            set;
            get;
        }
        /*-----------文本-----------------*/
        /// <summary>
        /// 注记字段
        /// </summary>
        public string AnnoFieldName
        {
            set;
            get;
        }
        /// <summary>
        /// 注记条件，在Maplex引擎中，gdb和mdb的条件语句不分[]和""
        /// </summary>
        public string Condition
        {
            set;
            get;
        }
        /// <summary>
        /// 注记表达式
        /// </summary>
        public string Expression
        {
            set;
            get;
        }
        /*-----------注记相对要素的位置-----------------*/
        /// <summary>
        /// 点状注记的放置
        /// </summary>
        public esriMaplexPointPlacementMethod PointPlacement
        {
            set;
            get;
        }
        /// <summary>
        /// 线状注记的放置
        /// </summary>
        public esriMaplexLinePlacementMethod LinePlacement
        {
            set;
            get;
        }
        /// <summary>
        /// 面状注记的放置位置
        /// </summary>
        public esriMaplexPolygonPlacementMethod PolygonPlacement
        {
            set;
            get;
        }

        /*-----------注记的位置要素-----------------*/
        /// <summary>
        /// 注记文字要素
        /// </summary>
        public IGeometry TextPath
        {
            set;
            get;
        }
        /// <summary>
        /// 注记与要素的偏移距离（毫米）
        /// </summary>
        public double PrimaryOffset
        {
            set;
            get;
        }
        /// <summary>
        /// 是否重复标注，线、面要素标注中
        /// </summary>
        public bool CanRepeatLabel
        {
            set;
            get;
        }
        /// <summary>
        /// 线、面重复标注的最小距离间隔？？如何获取一个Feature的多个Label？？
        /// </summary>
        public double LineMinimumRepeatInterval
        {
            set;
            get;
        }
        /// <summary>
        /// 是否启用字符间隔 线、面
        /// </summary>
        public bool CanSpreadCharacters
        {
            set;
            get;
        }
        /// <summary>
        /// 设置最大的间隔（字体宽度百分比）如果为0，则没有限制，标注将延伸适合要素 线、面
        /// </summary>
        public double MaximumCharacterSpacing
        {
            set;
            get;
        }
        /// <summary>
        /// 设置字间距
        /// </summary>
        public double CharacterSpacing
        {
            set;
            get;
        }

        /// <summary>
        /// 要素权重
        /// </summary>
        public int FeatureWeight
        {
            set;
            get;
        }
        /// <summary>
        /// (线)链接要素标注，IMaplexOverposterLayerProperties4中，描述One label for feature等样式
        /// </summary>
        public esriMaplexMultiPartOption MultiPartOption
        {
            set;
            get;
        }
        /// <summary>
        /// (线)最小标注的尺寸，IMaplexOverposterLayerProperties2进行尺寸单位的设置，采用毫米
        /// </summary>
        public double MinimumSizeForLabeling
        {
            set;
            get;
        }
        /// <summary>
        /// （面）是否标注最大的多边形 IMaplexOverposterLayerProperties4
        /// </summary>
        public bool LabelLargestPolygon
        {
            set;
            get;
        }

        /// <summary>
        /// （面）面内权重,在API中没有找到对应的，推测其采用的FeatureWeight进行了替代
        /// </summary>
        public int InteriorFeatureWeight
        {
            set;
            get;
        }
        /// <summary>
        /// (面)边缘权重 IMaplexOverposterLayerProperties
        /// </summary>
        public int BoundaryFeatureWeight
        {
            set;
            get;
        }
        /// <summary>
        /// 是否可以重复移除重复注记
        /// </summary>
        public bool CanRemoveLabelDuplicate
        {
            set;
            get;
        }
        /// <summary>
        /// 移除重复注记间隔
        /// </summary>
        public double RemoveDuplicates
        {
            set;
            get;
        }
        /// <summary>
        /// 是否关闭要素与线的连接 IMaplexOverposterLayerProperties4
        /// </summary>
        public bool EnableConnection
        {
            set;
            get;
        }
        /// <summary>
        /// 是否移除
        /// </summary>
        public bool CanLabelRemove
        {
            set;
            get;
        }
        /// <summary>
        /// 点符号中心点与锚点的偏移量(X:mm)
        /// </summary>
        public double SymbolOffsetDX
        {
            set;
            get;
        }
        /// <summary>
        /// 点符号中心点与锚点的偏移量(Y:mm)
        /// </summary>
        public double SymbolOffsetDY
        {
            set;
            get;
        }

        /// <summary>
        /// 注记图层名
        /// </summary>
        public string AnnoLayerName
        {
            set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public AnnoRule(string annoFieldName, string fontName, string fontSize, string fontColor, string fontStyle, string condition, string expression,
            string haloSize, string haloColor, string backgroundColor, string backgroundStyle, string backgroundBoderColor, string backgroundBoderWdith,
            string placement, string offset, string lineMinimumRepeatInterval, string maximumCharacterSpacing, string CharacterSpacing, string CharacterWidth, string removeDuplicates, string multiPartOption,   string minimumSizeForLabeling, string labelLargestPolygon, string interiorFeatureWeight, string boundaryFeatureWeight, string featureWeight, 
            string canLabelRemove, string symbolOffsetDX, string symbolOffsetDY, string annoLayerName)
        {
            this.AnnoFieldName = annoFieldName;
            this.FontName = fontName;
            this.FontSize = double.Parse(fontSize);
            if (fontColor != string.Empty)
            {
                this.FontColor = GetmykColor(fontColor);
            }
            else
                this.FontColor = null;
            if (fontStyle != string.Empty)
            {
                if (fontStyle.Contains("加粗"))
                    this.EnableBold = true;

                if (fontStyle.Contains("下划线"))
                    this.EnableUnderline = true;

                if (fontStyle.Contains("立体字"))
                    this.EnableThreeDim = true;
            }
            this.Condition = condition;
            if (expression.Contains("[ANNOFN]") && annoFieldName != "")//特殊字符串，该字符串将用注记字段替换之
            {
                expression = expression.Replace("[ANNOFN]", string.Format("[{0}]", AnnoFieldName));
            }
            this.Expression = expression;

            double size = 0;
            double.TryParse(haloSize, out size);
            if (size != 0)
            {
                this.HasHalo = true;
                this.HaloSize = size;
                if (haloColor != string.Empty)
                {
                    this.HaloColor = GetmykColor(haloColor);
                }
                else
                    this.HaloColor = null;
            }
            else
            {
                this.HasHalo = false;
            }


            #region 背景边框设置
            if (backgroundColor != string.Empty)
            {
                this.BackgroundColor = GetmykColor(backgroundColor);
                this.HasTextBackground = true;
                this.BackgroundMargin = 0.2;
                switch (backgroundStyle)
                {
                    case "矩形":
                        this.BackgroundStyle = (int)esriBalloonCalloutStyle.esriBCSRectangle;
                        break;
                    case "圆角":
                        this.BackgroundStyle = (int)esriBalloonCalloutStyle.esriBCSRoundedRectangle;
                        break;
                    case "椭圆":
                        this.BackgroundStyle = (int)esriBalloonCalloutStyle.esriBCSOval;
                        break;
                    case "高速":
                        this.BackgroundStyle = 100;
                        break;
                    default:
                        this.BackgroundStyle = (int)esriBalloonCalloutStyle.esriBCSRectangle;//矩形
                        break;
                    
                }
                if (backgroundBoderColor != string.Empty)
                {
                    this.HasBackgroundBorder = true;
                    this.BackgroundBorderColor = GetmykColor(backgroundBoderColor);
                    this.BackgroundBorderWidth = Convert.ToDouble(backgroundBoderWdith);
                }
                else
                    this.HasBackgroundBorder = false;
            }
            else
            {
                this.HasTextBackground = false;
            }
            #endregion

            #region 注记位置设置
            if (placement != string.Empty)
            {
                switch (placement)
                {
                    #region 点注记位置
                    case "中间":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexCenteredOnPoint;
                        break;
                    case "北边":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexNorthOfPoint;
                        break;
                    case "南边":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexSouthOfPoint;
                        break;
                    case "东边":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexEastOfPoint;
                        break;
                    case "西边":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexWestOfPoint;
                        break;
                    case "四周":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexAroundPoint;
                        break;
                    #endregion

                    #region 线注记位置
                    case "沿线曲线":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexOffsetCurvedFromLine;
                        break;
                    case "沿线直线":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexOffsetStraightFromLine;
                        break;
                    case "线上曲线":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexCenteredCurvedOnLine;
                        break;
                    case "线上直线":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexCenteredStraightOnLine;
                        break;
                    case "沿线水平":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexOffsetHorizontalFromLine;
                        break;
                    case "线上水平":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexCenteredHorizontalOnLine; ;
                        break;
                    #endregion

                    #region 面注记位置
                    case "面外":
                        this.PolygonPlacement = esriMaplexPolygonPlacementMethod.esriMaplexHorizontalAroundPolygon;
                        break;
                    case "面内":
                        this.PolygonPlacement = esriMaplexPolygonPlacementMethod.esriMaplexHorizontalInPolygon;
                        break;
                    #endregion
                }
                if (offset != string.Empty)
                {
                    this.PrimaryOffset = Convert.ToDouble(offset);
                }
            }
            #endregion

            //#region 字宽
            //this.CharacterWidth = -1;
            //#endregion
            #region 字宽
            this.CharacterWidth = 100;
            if (CharacterWidth != string.Empty)
            {
                this.CharacterWidth = int.Parse(CharacterWidth);
            }
            #endregion

            #region 字间距
            this.CharacterSpacing = 0;
            if (CharacterSpacing != string.Empty)
            {
                this.CharacterSpacing = int.Parse(CharacterSpacing);
            }
            #endregion


            #region 字距、注记重复等等
            if (lineMinimumRepeatInterval != string.Empty && lineMinimumRepeatInterval != "-1")
            {
                this.CanRepeatLabel = true;
                this.LineMinimumRepeatInterval = double.Parse(lineMinimumRepeatInterval);
            }
            else
            {
                this.CanRepeatLabel = false;
            }
            //字间距
            if (maximumCharacterSpacing != string.Empty && maximumCharacterSpacing != "-1")
            {
                this.CanSpreadCharacters = true;
                this.MaximumCharacterSpacing = double.Parse(maximumCharacterSpacing);
            }
            else
            {
                this.CanSpreadCharacters = false;
            }
            //移除重复标注
            if (removeDuplicates != string.Empty)
            {
                this.CanRemoveLabelDuplicate = true;
                this.RemoveDuplicates = Convert.ToDouble(removeDuplicates);
            }
            else
            {
                this.CanRemoveLabelDuplicate = false;
            }
            //连接要素标注
            if (multiPartOption != string.Empty)
            {
                this.EnableConnection = true;
                switch (multiPartOption)
                {
                    case "one label per feature":
                        this.MultiPartOption = esriMaplexMultiPartOption.esriMaplexOneLabelPerFeature;
                        break;
                    case "one label per feature part":
                        this.MultiPartOption = esriMaplexMultiPartOption.esriMaplexOneLabelPerPart;
                        break;
                    case "one label per feature segment":
                        this.MultiPartOption = esriMaplexMultiPartOption.esriMaplexOneLabelPerSegment;
                        break;
                }
            }
            else
            {
                this.EnableConnection = false;
            }
            //标注最小要素大小
            if (minimumSizeForLabeling != string.Empty)
                this.MinimumSizeForLabeling = Convert.ToDouble(minimumSizeForLabeling);
            //标注要素最大的部分
            if (labelLargestPolygon == "是")
                this.LabelLargestPolygon = true;
            //要素权重
            if (featureWeight != string.Empty)
                this.FeatureWeight = int.Parse(featureWeight);
            //边缘权重
            if (boundaryFeatureWeight != string.Empty)
                this.BoundaryFeatureWeight = Convert.ToInt32(boundaryFeatureWeight);
            //面内权重
            if (interiorFeatureWeight != string.Empty)
                this.InteriorFeatureWeight = Convert.ToInt32(interiorFeatureWeight);
            //是否移除
            if (canLabelRemove != string.Empty)
            {
                switch (canLabelRemove)
                {
                    case "是":
                        this.CanLabelRemove = true;
                        break;
                    case "否":
                        this.CanLabelRemove = false;
                        break;
                }
            }

            //点符号锚点与中心点的偏移量
            double dx = 0;
            double dy = 0;
            if (symbolOffsetDX != string.Empty)
            {
                double.TryParse(symbolOffsetDX, out dx);
            }
            if (symbolOffsetDY != string.Empty)
            {
                double.TryParse(symbolOffsetDY, out dy);
            }
            this.SymbolOffsetDX = dx;
            this.SymbolOffsetDY = dy;

            #endregion

            this.AnnoLayerName = annoLayerName;
        }

        /// <summary>
        /// 根据注记规则里的CMYK字符串得到CMYK颜色值
        /// </summary>
        /// <param name="cmyk">cmyk字符串（形如：C100M200Y100K50）</param>
        /// <returns>CMYK颜色值</returns>
        private ICmykColor GetmykColor(string cmyk)
        {
            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;
            //
            for (int i = 0; i <= cmyk.Length; i++)
            {
                if (i == cmyk.Length)
                {
                    string sbs = sb.ToString();
                    if (sbs.Contains('C'))
                    {
                        CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                    }
                    if (sbs.Contains('M'))
                    {
                        CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                    }
                    if (sbs.Contains('Y'))
                    {
                        CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                    }
                    if (sbs.Contains('K'))
                    {
                        CMYK_Color.Black = int.Parse(sbs.Substring(1));
                    }
                    break;
                }
                else
                {
                    char C = cmyk[i];
                    if (D.Contains(C))
                    {
                        sb.Append(C);
                    }
                    else
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('C'))
                        {
                            CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('M'))
                        {
                            CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('Y'))
                        {
                            CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('K'))
                        {
                            CMYK_Color.Black = int.Parse(sbs.Substring(1));
                        }
                        sb.Clear();
                        sb.Append(C);
                    }
                }
            }
            return CMYK_Color;
        }
    }
}

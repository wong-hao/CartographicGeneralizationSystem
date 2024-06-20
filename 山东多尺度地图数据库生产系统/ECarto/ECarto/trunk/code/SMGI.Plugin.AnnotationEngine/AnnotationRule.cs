using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.AnnotationEngine
{
    /// <summary>
    /// LiZh(2018.10)
    /// 注记规则类
    /// </summary>
    public class AnnotationRule
    {
        #region 一般
        /// <summary>
        /// 要素类名(需生成注记的要素所在要素类名称)
        /// </summary>
        public string FCName
        {
            set;
            get;
        }

        /// <summary>
        /// SQL查询条件
        /// </summary>
        public string WhereClause
        {
            set;
            get;
        }

        /// <summary>
        /// 规则分类名(将要素分类，以不同的注记规则为每个类生成注记)
        /// </summary>
        public string AnnoClass
        {
            set;
            get;
        }

        /// <summary>
        /// 标注字段
        /// </summary>
        public string AnnoFieldName
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

        /// <summary>
        /// 目标注记要素类名(创建后的注记所属注记要素类)
        /// </summary>
        public string AnnoFCName
        {
            set;
            get;
        }
        #endregion

        #region 字体样式
        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName
        {
            get;
            set;
        }

        /// <summary>
        /// 字体大小（默认2.0，单位：毫米）
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
        /// 是否加粗（默认为false）
        /// </summary>
        public bool EnableBold
        {
            set;
            get;
        }

        /// <summary>
        /// 是否斜体（默认为false）
        /// </summary>
        public bool EnableItalic
        {
            set;
            get;
        }

        /// <summary>
        /// 是否下划线（默认为false）
        /// </summary>
        public bool EnableUnderline
        {
            set;
            get;
        }

        /// <summary>
        /// 是否开启晕圈（默认为false）
        /// </summary>
        public bool EnableHalo
        {
            set;
            get;
        }

        /// <summary>
        /// 晕圈大小（单位：毫米）
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

        /// <summary>
        /// 是否开启文本背景（气泡，默认为false）
        /// </summary>
        public bool EnableTextBackground
        {
            set;
            get;
        }

        /// <summary>
        /// 气泡样式
        /// </summary>
        public esriBalloonCalloutStyle BackgroundStyle
        {
            set;
            get;
        }

        /// <summary>
        /// 气泡背景色
        /// </summary>
        public IColor BackgroundColor
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
        /// 气泡边界宽度（单位：毫米）
        /// </summary>
        public double BackgroundBorderWidth
        {
            set;
            get;
        }

        /// <summary>
        /// 气泡边框距（单位：毫米）
        /// </summary>
        public double BackgroundMargin
        {
            set;
            get;
        }

        /// <summary>
        /// 字符宽度（默认为100.0）
        /// </summary>
        public double CharacterWidth
        {
            set;
            get;
        }
        
        /// <summary>
        /// 是否开启CJK字符方向（默认为false）
        /// </summary>
        public bool EnableCJKCharactersRotation
        {
            set;
            get;
        }
        #endregion

        #region 标注位置
        /// <summary>
        /// 线要素注记放置类型
        /// </summary>
        public esriMaplexLineFeatureType LineFeatureType
        {
            set;
            get;
        }

        /// <summary>
        /// 面要素注记放置类型
        /// </summary>
        public esriMaplexPolygonFeatureType PolygonFeatureType
        {
            set;
            get;
        }

        /// <summary>
        /// 点要素注记的放置位置
        /// </summary>
        public esriMaplexPointPlacementMethod PointPlacement
        {
            set;
            get;
        }

        /// <summary>
        /// 线要素注记的放置位置
        /// </summary>
        public esriMaplexLinePlacementMethod LinePlacement
        {
            set;
            get;
        }

        /// <summary>
        /// 面要素注记的放置位置
        /// </summary>
        public esriMaplexPolygonPlacementMethod PolygonPlacement
        {
            set;
            get;
        }

        /// <summary>
        /// 注记偏移（单位：毫米）
        /// </summary>
        public double PrimaryOffset
        {
            set;
            get;
        }

        /// <summary>
        /// 是否启用展开文字效果（默认为false）
        /// </summary>
        public bool EnableSpreadWords
        {
            set;
            get;
        }

        /// <summary>
        /// 最大文字间距（字宽百分比）
        /// </summary>
        public double MaximumWordSpacing
        {
            set;
            get;
        }

        /// <summary>
        /// 是否启用展开字符效果（默认为false）
        /// </summary>
        public bool EnableSpreadCharacters
        {
            set;
            get;
        }

        /// <summary>
        /// 最大字符间距（字宽百分比）
        /// </summary>
        public double MaximumCharacterSpacing
        {
            set;
            get;
        }
        #endregion

        #region 标注密度
        /// <summary>
        /// 是否开启移除同名注记（默认为false）
        /// </summary>
        public bool EnableThinDuplicateLabels
        {
            set;
            get;
        }

        /// <summary>
        /// 同名注记的检索距离（单位：毫米）
        /// </summary>
        public double ThinningDistance
        {
            set;
            get;
        }

        /// <summary>
        /// 是否开启重复标注（默认为false）
        /// </summary>
        public bool EnableRepeatLabel
        {
            set;
            get;
        }

        /// <summary>
        /// 最小重复间隔（单位：毫米）
        /// </summary>
        public double MinimumRepetitionInterval
        {
            set;
            get;
        }

        /// <summary>
        /// 标注缓冲大小（字体高度百分比,默认为15）
        /// </summary>
        public int LabelBuffer
        {
            set;
            get;
        }

        /// <summary>
        /// 标注的最小要素大小（单位：毫米，默认为0）
        /// </summary>
        public double MinimumSizeForLabeling
        {
            set;
            get;
        }

        /// <summary>
        /// 是否开启连接要素（默认为false）
        /// </summary>
        public bool EnableConnection
        {
            set;
            get;
        }

        /// <summary>
        /// 是否开启标注要素最大部分（默认为false）
        /// </summary>
        public bool EnableLabelLargestPolygon
        {
            set;
            get;
        }
        #endregion

        #region 冲突解决
        /// <summary>
        /// 要素权重(0-1000)
        /// </summary>
        public int FeatureWeight
        {
            set;
            get;
        }

        /// <summary>
        /// 是否开启从不移除功能（默认为false）
        /// </summary>
        public bool NeverRemoveLabel
        {
            set;
            get;
        }
        #endregion

        #region 其它
        /// <summary>
        /// 是否需要对要素进行消隐（"单要素几何覆盖"表示该类注记需要对其关联要素进行消隐处理（几何覆盖）；"多要素几何覆盖"表示该类注记需对其关联要素类中的所有相交要素进行消隐处理（几何覆盖）；"单要素局部消隐"表示该类注记需要对其关联要素进行消隐处理（局部消隐线)；其它字符则不进行消隐）
        /// </summary>
        public string BlankingType
        {
            set;
            get;
        }
        #endregion

        public AnnotationRule(string fcName, string whereClause, string annoClass, string annoFieldName, string expression, string annoFCName,
            string fontName, string fontSize, string fontColor, string fontStyle, string haloSize, string haloColor, string backgroundStyle, string backgroundColor,
            string backgroundBorderColor, string backgroundBorderWdith, string backgroundMargin, string characterWidth, string cjkCharactersRotation, string featureType, 
            string placePosition, string labelOffset, string maxWordSpacing, string maxCharacterSpacing, string thinningDistance, string minRepetitionInterval,
            string labelBuffer, string minSizeForLabeling, string connection, string labelLargestPolygon, string featureWeight, string neverRemoveLabel, string blankingType = "")
        {
            #region 一般
            //要素类名
            this.FCName = fcName;

            //SQL查询条件
            this.WhereClause = whereClause;

            //规则分类名
            this.AnnoClass = annoClass;

            //标注字段
            this.AnnoFieldName = annoFieldName;

            //注记表达式
            this.Expression = expression;

            //目标注记要素类名
            this.AnnoFCName = annoFCName;
            #endregion

            #region 字体样式
            //字体名称
            this.FontName = fontName;

            //字体大小（单位：毫米）
            this.FontSize = 2.0;
            if(!string.IsNullOrEmpty(fontSize))
                this.FontSize = double.Parse(fontSize);

            if (false)//临时，没有找到方法使得字高与字体大小保持一致？？？？？
            {
                this.FontSize = this.FontSize * 1.4;//考虑到arcgis实际的字高小于字体大小的情况，这里乘一个平均放大系数
            }

            //字体颜色
            this.FontColor = AnnotationHelper.GetColor(fontColor);

            //字体样式（加粗、倾斜、下划线）
            this.EnableBold = false;
            this.EnableItalic = false;
            this.EnableUnderline = false;
            if(!string.IsNullOrEmpty(fontStyle))
            {
                if(fontStyle.Contains("加粗"))
                    this.EnableBold = true;
                if (fontStyle.Contains("斜体"))
                    this.EnableItalic = true;
                if (fontStyle.Contains("下划线"))
                    this.EnableUnderline = true;
            }


            //晕圈
            this.EnableHalo = false;
            if (!string.IsNullOrEmpty(haloSize))
            {
                double size = 0;
                double.TryParse(haloSize, out size);
                if (size > 0)
                {
                    this.EnableHalo = true;
                    this.HaloSize = size;
                    this.HaloColor = AnnotationHelper.GetColor(haloColor);
                }
            }

            //文本背景（气泡）
            this.EnableTextBackground = false;
            if (backgroundStyle == "圆角" || backgroundStyle == "方角")
            {
                EnableTextBackground = true;
                switch (backgroundStyle)
                {
                    case "圆角":
                        this.BackgroundStyle = esriBalloonCalloutStyle.esriBCSRoundedRectangle;
                        break;
                    case "方角":
                        this.BackgroundStyle = esriBalloonCalloutStyle.esriBCSRectangle;
                        break;
                }
                this.BackgroundColor = AnnotationHelper.GetColor(backgroundColor);
                this.BackgroundBorderColor = AnnotationHelper.GetColor(backgroundBorderColor);
                this.BackgroundBorderWidth = 0.2;
                if (!string.IsNullOrEmpty(backgroundBorderWdith))
                {
                    this.BackgroundBorderWidth = double.Parse(backgroundBorderWdith);
                }
                this.BackgroundMargin = 0.2;
                if (!string.IsNullOrEmpty(backgroundMargin))
                {
                    this.BackgroundMargin = double.Parse(backgroundMargin);
                }
            }

            //字符宽度
            this.CharacterWidth = 100.0;
            if (!string.IsNullOrEmpty(characterWidth))
            {
                double cw = 0;
                double.TryParse(characterWidth, out cw);
                if (cw > 0)
                    this.CharacterWidth = cw;
            }

            //CJK字符方向
            this.EnableCJKCharactersRotation = false;
            if (cjkCharactersRotation == "是")
                this.EnableCJKCharactersRotation = true;
            #endregion

            #region 标注位置
            //放置类型
            if (!string.IsNullOrEmpty(featureType))
            {
                switch (featureType)
                {
                    case "街道放置":
                        this.LineFeatureType = esriMaplexLineFeatureType.esriMaplexStreetFeature;
                        break;
                    case "街道地址放置":
                        this.LineFeatureType = esriMaplexLineFeatureType.esriMaplexStreetAddressRange;
                        break;
                    case "等值线放置":
                        this.LineFeatureType = esriMaplexLineFeatureType.esriMaplexContourFeature;
                        break;
                    case "河流放置":
                        this.LineFeatureType = esriMaplexLineFeatureType.esriMaplexRiverFeature;
                        this.PolygonFeatureType = esriMaplexPolygonFeatureType.esriMaplexRiverPolygonFeature;
                        break;
                    case "地块放置":
                        this.PolygonFeatureType = esriMaplexPolygonFeatureType.esriMaplexLandParcelFeature;
                        break;
                    case "边界放置":
                        this.PolygonFeatureType = esriMaplexPolygonFeatureType.esriMaplexPolygonBoundaryFeature;
                        break;
                    default:
                        this.LineFeatureType = esriMaplexLineFeatureType.esriMaplexLineFeature;
                        this.PolygonFeatureType = esriMaplexPolygonFeatureType.esriMaplexPolygonFeature;
                        break;
                }
            }

            //放置位置
            if (!string.IsNullOrEmpty(placePosition))
            {
                switch (placePosition)
                {
                    #region 点注记位置
                    case "居中":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexCenteredOnPoint;
                        break;
                    case "北":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexNorthOfPoint;
                        break;
                    case "南":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexSouthOfPoint;
                        break;
                    case "东":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexEastOfPoint;
                        break;
                    case "西":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexWestOfPoint;
                        break;
                    case "四周":
                        this.PointPlacement = esriMaplexPointPlacementMethod.esriMaplexAroundPoint;
                        break;
                    #endregion

                    #region 线注记位置
                    case "水平居中":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexCenteredHorizontalOnLine;
                        break;
                    case "平直居中":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexCenteredStraightOnLine;
                        break;
                    case "弯曲居中":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexCenteredCurvedOnLine;
                        break;
                    case "垂直居中":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexCenteredPerpendicularOnLine;
                        break;
                    case "水平偏移":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexOffsetHorizontalFromLine; 
                        break;
                    case "弯曲偏移":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexOffsetCurvedFromLine; 
                        break;
                    case "垂直偏移":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexOffsetPerpendicularFromLine; 
                        break;
                    case "平直偏移":
                        this.LinePlacement = esriMaplexLinePlacementMethod.esriMaplexOffsetStraightFromLine;
                        break;
                    #endregion

                    #region 面注记位置
                    case "面外弯曲":
                        this.PolygonPlacement = esriMaplexPolygonPlacementMethod.esriMaplexCurvedAroundPolygon;
                        break;
                    case "面外水平":
                        this.PolygonPlacement = esriMaplexPolygonPlacementMethod.esriMaplexHorizontalAroundPolygon;
                        break;
                    case "面内弯曲":
                        this.PolygonPlacement = esriMaplexPolygonPlacementMethod.esriMaplexCurvedInPolygon;
                        break;
                    case "面内水平":
                        this.PolygonPlacement = esriMaplexPolygonPlacementMethod.esriMaplexHorizontalInPolygon;
                        break;
                    #endregion
                }
            }

            //注记偏移
            if (!string.IsNullOrEmpty(labelOffset))
            {
                double offset = 0;
                double.TryParse(labelOffset, out offset);
                if (offset > 0)
                    this.PrimaryOffset = offset;
            }

            //展开文字
            this.EnableSpreadWords = false;
            if(!string.IsNullOrEmpty(maxWordSpacing))
            {
                double wordSpacing = 0;
                double.TryParse(maxWordSpacing, out wordSpacing);
                if(wordSpacing >=0)
                {
                    this.EnableSpreadWords = true;
                    this.MaximumWordSpacing = wordSpacing;
                }
            }

            //展开字符
            this.EnableSpreadCharacters = false;
            if (!string.IsNullOrEmpty(maxCharacterSpacing))
            {
                double characterSpacing = 0;
                double.TryParse(maxCharacterSpacing, out characterSpacing);
                if (characterSpacing >= 0)
                {
                    this.EnableSpreadCharacters = true;
                    this.MaximumCharacterSpacing = characterSpacing;
                }
            }
            #endregion

            #region 标注密度
            //移除同名注记
            this.EnableThinDuplicateLabels = false;
            if (!string.IsNullOrEmpty(thinningDistance))
            {
                double distance = 0;
                double.TryParse(thinningDistance, out distance);
                if (distance >= 0)
                {
                    this.EnableThinDuplicateLabels = true;
                    this.ThinningDistance = distance;
                }
            }

            //重复标注
            this.EnableRepeatLabel = false;
            if (!string.IsNullOrEmpty(minRepetitionInterval))
            {
                double interval = 0;
                double.TryParse(minRepetitionInterval, out interval);
                if (interval > 0)
                {
                    this.EnableRepeatLabel = true;
                    this.MinimumRepetitionInterval = interval;
                }
            }

            //标注缓冲大小
            this.LabelBuffer = 15;
            if (!string.IsNullOrEmpty(labelBuffer))
            {
                int buffer = 0;
                int.TryParse(labelBuffer, out buffer);
                if (buffer > 0)
                    this.LabelBuffer = buffer;
            }

            //标注的最小要素大小
            this.MinimumSizeForLabeling = 0;
            if (!string.IsNullOrEmpty(minSizeForLabeling))
            {
                int size = 0;
                int.TryParse(minSizeForLabeling, out size);
                if (size > 0)
                    this.MinimumSizeForLabeling = size;
            }

            //连接要素
            this.EnableConnection = false;
            if (connection == "是")
                this.EnableConnection = true;

            //标注要素最大部分
            this.EnableLabelLargestPolygon = false;
            if (labelLargestPolygon == "是")
                this.EnableLabelLargestPolygon = true;
            #endregion

            #region 冲突解决
            //要素权重
            this.FeatureWeight = 0;
            if (!string.IsNullOrEmpty(featureWeight))
            {
                int weight = 0;
                int.TryParse(featureWeight, out weight);
                if (weight > 0)
                    this.FeatureWeight = weight;
            }

            //从不移除
            this.NeverRemoveLabel = false;
            if (neverRemoveLabel == "是")
                this.NeverRemoveLabel = true;
            #endregion

            #region 其它
            if (!string.IsNullOrEmpty(blankingType))
            {
                BlankingType = blankingType;
            }
            #endregion
        }
    }
}

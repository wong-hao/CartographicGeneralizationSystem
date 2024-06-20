using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Data;
using ESRI.ArcGIS.Geometry;
using System.Xml.Linq;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Maplex;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using stdole;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;
namespace SMGI.Plugin.EmergencyMap
{
    public class AnnoHelper
    {
        /// <summary>
        /// 根据注记样式创建一个标注引擎Class
        /// </summary>
        /// <param name="annoStyle">注记样式</param>
        /// <param name="geometryType">几何类型</param>
        /// <param name="refernceScale">参考比例尺</param>
        /// <returns>标注引擎</returns>
        public static ILabelEngineLayerProperties2 CreateLabelEngineLayerPropertiesOfMaplex(AnnoRule annoRule, esriGeometryType geometryType, double refernceScale)
        {
            //创建一个文本符号
            string subMessage = string.Empty;
            ITextSymbol textSymbol = CreateTextSymbol("", annoRule.FontName, annoRule.FontSize, annoRule.FontColor, annoRule.EnableBold, annoRule.EnableUnderline, out subMessage);
            if (textSymbol == null)
            {
                return null;
            }

            //创建标注引擎属性 作为一个Class
            ILabelEngineLayerProperties2 labelEngineLayerProperties = new MaplexLabelEngineLayerPropertiesClass();
            labelEngineLayerProperties.Symbol = textSymbol;
            if (annoRule.Expression.ToUpper().Contains("FUNCTION"))
            {
                labelEngineLayerProperties.Expression = annoRule.Expression;//设置注记的表达式
                labelEngineLayerProperties.IsExpressionSimple = false;
                IAnnotationExpressionEngine pAee = new AnnotationVBScriptEngineClass();
                labelEngineLayerProperties.ExpressionParser = pAee;
            }
            

            IMaplexOverposterLayerProperties maplexOverposterLayerProperties = new MaplexOverposterLayerProperties();
            maplexOverposterLayerProperties.PrimaryOffsetUnit = esriMaplexUnit.esriMaplexUnitMM;
            maplexOverposterLayerProperties.LabelBuffer = 15;//设置注记间的避让间距，很重要
            // 根据要素的不同线性设置注记定位的相关规则
            switch (geometryType)
            {
                case esriGeometryType.esriGeometryPoint:
                    maplexOverposterLayerProperties.FeatureType = esriBasicOverposterFeatureType.esriOverposterPoint;
                    maplexOverposterLayerProperties.PointPlacementMethod = annoRule.PointPlacement;
                    maplexOverposterLayerProperties.EnablePointPlacementPriorities = true;
                    //设置自定义的优先级
                    IPointPlacementPriorities pointPlacementPriorities = new PointPlacementPrioritiesClass();
                    pointPlacementPriorities.CenterRight = 1;
                    pointPlacementPriorities.BelowCenter = 2;
                    pointPlacementPriorities.CenterLeft = 3;
                    pointPlacementPriorities.AboveCenter = 4;
                    pointPlacementPriorities.AboveLeft = 0;
                    pointPlacementPriorities.AboveRight = 0;
                    pointPlacementPriorities.BelowLeft = 0;
                    pointPlacementPriorities.BelowRight = 0;
                    maplexOverposterLayerProperties.PointPlacementPriorities = pointPlacementPriorities;
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    maplexOverposterLayerProperties.FeatureType = esriBasicOverposterFeatureType.esriOverposterPolyline;
                    maplexOverposterLayerProperties.LinePlacementMethod = annoRule.LinePlacement;
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    maplexOverposterLayerProperties.FeatureType = esriBasicOverposterFeatureType.esriOverposterPolygon;
                    maplexOverposterLayerProperties.PolygonPlacementMethod = annoRule.PolygonPlacement;
                    break;
            }

            //注记偏移
            maplexOverposterLayerProperties.PrimaryOffset = annoRule.PrimaryOffset;
            maplexOverposterLayerProperties.PrimaryOffsetUnit = esriMaplexUnit.esriMaplexUnitMM;

            //字间距
            if (annoRule.CanSpreadCharacters)
            {
                maplexOverposterLayerProperties.SpreadCharacters = true;
                maplexOverposterLayerProperties.MaximumCharacterSpacing = annoRule.MaximumCharacterSpacing;
            }
            else
                maplexOverposterLayerProperties.SpreadCharacters = false;

            //注记重复标注
            if (annoRule.CanRepeatLabel)
            {
                maplexOverposterLayerProperties.RepeatLabel = true;
                maplexOverposterLayerProperties.MinimumRepetitionInterval = annoRule.LineMinimumRepeatInterval;
                (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties2).RepetitionIntervalUnit = esriMaplexUnit.esriMaplexUnitMM;
            }
            else
                maplexOverposterLayerProperties.RepeatLabel = false;

            //连接要素标注
            if (annoRule.EnableConnection)
            {
                IMaplexOverposterLayerProperties4 maplexOverposterLayerProperties4 = maplexOverposterLayerProperties as IMaplexOverposterLayerProperties4;
                maplexOverposterLayerProperties4.EnableConnection = true;
                maplexOverposterLayerProperties4.ConnectionType = esriMaplexConnectionType.esriMaplexMinimizeLabels;
            }
            else
            {
                IMaplexOverposterLayerProperties4 maplexOverposterLayerProperties4 = maplexOverposterLayerProperties as IMaplexOverposterLayerProperties4;
                maplexOverposterLayerProperties4.EnableConnection = false;
                maplexOverposterLayerProperties4.MultiPartOption = annoRule.MultiPartOption;

            }

            //移除重复标注
            if (annoRule.CanRemoveLabelDuplicate)
            {
                maplexOverposterLayerProperties.ThinDuplicateLabels = true;
                maplexOverposterLayerProperties.ThinningDistance = annoRule.RemoveDuplicates;
                (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties2).ThinningDistanceUnit = esriMaplexUnit.esriMaplexUnitMM;
            }
            else
                maplexOverposterLayerProperties.ThinDuplicateLabels = false;

            //标注最小要素大小
            maplexOverposterLayerProperties.MinimumSizeForLabeling = annoRule.MinimumSizeForLabeling;

            //标注要素最大的部分
            if (annoRule.LabelLargestPolygon)
            {
                IMaplexOverposterLayerProperties4 maplexOverposterLayerProperties4 = maplexOverposterLayerProperties as IMaplexOverposterLayerProperties4;
                maplexOverposterLayerProperties4.LabelLargestPolygon = true;
            }
            else
            {
                IMaplexOverposterLayerProperties4 maplexOverposterLayerProperties4 = maplexOverposterLayerProperties as IMaplexOverposterLayerProperties4;
                maplexOverposterLayerProperties4.LabelLargestPolygon = false;
            }

            //注记权重
            maplexOverposterLayerProperties.FeatureWeight = annoRule.FeatureWeight;
            //maplexOverposterLayerProperties.FeatureWeight = 0;

            //面内权重,边缘权重
            if (geometryType == esriGeometryType.esriGeometryPolygon)
            {
                maplexOverposterLayerProperties.FeatureWeight = annoRule.InteriorFeatureWeight;
                maplexOverposterLayerProperties.PolygonBoundaryWeight = annoRule.BoundaryFeatureWeight;
            }

            //是否删除
            if (annoRule.CanLabelRemove)
                maplexOverposterLayerProperties.NeverRemoveLabel = false;
            else
                maplexOverposterLayerProperties.NeverRemoveLabel = true;

            //设置标注的参考比例尺
            IAnnotateLayerTransformationProperties pAnnoLyrPros = labelEngineLayerProperties as IAnnotateLayerTransformationProperties;
            //pAnnoLyrPros.ReferenceScale = refernceScale;//设置标注的参考比例，在修改注记的对其方式后，注记会缩小？？？

            labelEngineLayerProperties.OverposterLayerProperties = maplexOverposterLayerProperties as IOverposterLayerProperties;
            labelEngineLayerProperties.OverposterLayerProperties.IsBarrier = true;
            labelEngineLayerProperties.OverposterLayerProperties.PlaceLabels = true;
            labelEngineLayerProperties.OverposterLayerProperties.PlaceSymbols = false;

            return labelEngineLayerProperties;
        }

        /// <summary>
        /// Maplex中用到的创建字体符号
        /// </summary>
        /// <param name="annoText">注记的文本</param>
        /// <param name="fontFamily">字体</param>
        /// <param name="fontSize">字大</param>
        /// <param name="cmykColor">颜色</param>
        /// <param name="message"></param>
        /// <returns>文本符号</returns>
        public static ITextSymbol CreateTextSymbol(string annoText, string fontFamily, double fontSize, IColor cmykColor, bool bBold, bool bUnderline, out string message)
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
            if (!fontExist)
            {
                message = "系统缺失[" + fontFamily + "]字体库";

                fontFamily = "宋体";//默认的话给宋体
            }

            ITextElement pTextElment = new TextElementClass();
            pTextElment.ScaleText = true;
            ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElment;
            pSymbolCollEle.FontName = fontFamily;
            pSymbolCollEle.Color = cmykColor;
            //pSymbolCollEle.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
            //pSymbolCollEle.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            pSymbolCollEle.Size = fontSize * 2.8345;
            pSymbolCollEle.Bold = bBold;
            pSymbolCollEle.Underline = bUnderline;

            
            return pTextElment.Symbol;
        }


        /// <summary>
        /// Maplex注记的创建方式，创建文本元素
        /// </summary>
        /// <param name="label"></param>
        /// <param name="annoRule"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ITextElement CreateTextElement(IMaplexPlacedLabel label, AnnoRule annoRule, out string message)
        {
            message = string.Empty;
            string fontFamily = annoRule.FontName;

            bool fontExist = false;
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily ff in fonts.Families)
            {
                if (ff.Name == fontFamily)
                {
                    fontExist = true;
                }
            }
            if (!fontExist)
            {
                message = "系统缺失[" + fontFamily + "]字体库,已被替换为宋体!";

                fontFamily = "宋体";//默认的话给宋体
            }

            IPoint anchorPoint = getTextLablePoint(label);
            IGeometry textPath = getTextPath(label);

            //创建文本元素
            ITextElement pTextElment = new TextElementClass();
            pTextElment.ScaleText = true;
            ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElment;

            pSymbolCollEle.Color = annoRule.FontColor;
            pSymbolCollEle.Bold = annoRule.EnableBold;
            pSymbolCollEle.Underline = annoRule.EnableUnderline;
            pSymbolCollEle.FontName = fontFamily;
            pSymbolCollEle.AnchorPoint = anchorPoint;
            //pSymbolCollEle.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
            //pSymbolCollEle.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            pSymbolCollEle.Size = label.Size;
            pSymbolCollEle.Text = label.Label;
            pSymbolCollEle.WordSpacing = label.WordSpacing;
            pSymbolCollEle.CharacterSpacing = annoRule.CharacterSpacing;
            pSymbolCollEle.CharacterWidth = annoRule.CharacterWidth;
            ITextSymbol textSymbol = pTextElment.Symbol;
            if (annoRule.HasTextBackground)
            {
                //设置文本背景框样式
                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;

                if (annoRule.BackgroundStyle == (int)esriBalloonCalloutStyle.esriBCSRoundedRectangle ||
                    annoRule.BackgroundStyle == (int)esriBalloonCalloutStyle.esriBCSRectangle ||
                    annoRule.BackgroundStyle == (int)esriBalloonCalloutStyle.esriBCSOval)//气泡
                {
                    IBalloonCallout balloonCallout = new BalloonCalloutClass();
                    balloonCallout.Style = (esriBalloonCalloutStyle)annoRule.BackgroundStyle;
                    // balloonCallout.Style = esriBalloonCalloutStyle.esriBCSOval;
                    //设置文本背景框颜色
                    IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                    fillSymbol.Color = annoRule.BackgroundColor;
                    ILineSymbol lineSymbol = new SimpleLineSymbol();
                    if (annoRule.BackgroundBorderColor != null)
                    {
                        lineSymbol.Width = annoRule.BackgroundBorderWidth;
                        lineSymbol.Color = annoRule.BackgroundBorderColor;
                    }
                    else
                        lineSymbol.Width = 0;
                    fillSymbol.Outline = lineSymbol;
                    balloonCallout.Symbol = fillSymbol;
                    //设置背景框边距
                    ITextMargins textMarigns = balloonCallout as ITextMargins;
                    double margin = annoRule.BackgroundMargin * 2.8345;
                    textMarigns.PutMargins(margin, margin, margin, margin);
                    formattedTextSymbol.Background = balloonCallout as ITextBackground;
                }
                else if(annoRule.BackgroundStyle == 100)//高速公路编号标记文本
                {
                    //设置文本背景框样式
                    IMarkerTextBackground markerText = new MarkerTextBackgroundClass();
                    markerText.ScaleToFit = true;

                    
                    //设置标记符号
                    string stylePath = GApplication.Application.Template.Root + @"\专家库\smgi.style";
                    string symbolName = "高速";
                    IMarkerSymbol markerSymbol = CommonMethods.GetMarkerSymbolFromStyleFile(stylePath, symbolName);
                    if (markerSymbol != null)
                    {
                        markerSymbol.Color = annoRule.BackgroundColor;
                        markerText.Symbol = markerSymbol;
                    }

                    formattedTextSymbol.Background = markerText as ITextBackground;
                }
                
            }
            if (annoRule.HasHalo)
            {
                IMask mask = textSymbol as IMask;
                mask.MaskStyle = esriMaskStyle.esriMSHalo;
                mask.MaskSize = annoRule.HaloSize * 2.8345;
                IFillSymbol fillSymbol = new SimpleFillSymbol();
                fillSymbol.Color = annoRule.HaloColor;
                ILineSymbol lineSymbol = new SimpleLineSymbol();
                lineSymbol.Width = 0;
                fillSymbol.Outline = lineSymbol;
                mask.MaskSymbol = fillSymbol;
            }
            (textSymbol as ICharacterOrientation).CJKCharactersRotation = label.CJKCharactersRotation;

            pTextElment.Symbol = textSymbol;//一定要写回去，否则没有效果
            IElement pEle = pTextElment as IElement;
            pEle.Geometry = textPath;

            return pTextElment;
        }

        /// <summary>
        /// 获得注记标识点
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static IPoint getTextLablePoint(IMaplexPlacedLabel label)
        {
            IPolygon bounds = label.Bounds;

            return (bounds as IArea).Centroid;
        }

        /// <summary>
        /// 获取注记的放置路径
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static IGeometry getTextPath(IMaplexPlacedLabel label)
        {
            IGeometry textPath = label.TextPath;
            //IPoint centerPoint = getTextLablePoint(label);

            //if (textPath is IPoint)
            //{
            //    textPath = centerPoint;
            //}
            //else if (textPath is IPolyline)
            //{
            //    //计算中心点到textPath的距离
            //    IPoint outPoint = new ESRI.ArcGIS.Geometry.Point();
            //    double distanceAlongCurve = 0;
            //    double distanceFromCurve = 0;
            //    bool bRightSide = false;
            //    (textPath as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, centerPoint, false, outPoint,
            //        ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
            //    if (!bRightSide)
            //        distanceFromCurve = -distanceFromCurve;
            //    IConstructCurve pConstructCurve = new PolylineClass();
            //    //获取注记的中心线
            //    pConstructCurve.ConstructOffset(textPath as IPolycurve, distanceFromCurve);//右为正值，左为负值

            //    //反转
            //    ReversePolylineDirection(pConstructCurve as IPolyline);

            //    textPath = pConstructCurve as IGeometry;
            //}

            return textPath;
        }

        /// <summary>
        /// 折线注记的防线处理，用于注记中，方向在45度~135度的进行反向,保证注记由上往下
        /// </summary>
        /// <param name="textPath"></param>
        public static void ReversePolylineDirection(IPolyline textPath)
        {
            IPoint fromPoint = textPath.FromPoint;
            IPoint toPoint = textPath.ToPoint;

            //计算弧度
            double dx = toPoint.X - fromPoint.X;
            double dy = toPoint.Y - fromPoint.Y;
            double angle = Math.Atan2(dy, dx) * 180 / Math.PI;//弧度转度[-180,+180]

            //45度到135度进行折线的反转
            if (angle > 45 && angle < 135)
            {
                textPath.ReverseOrientation();
            }
        }

        /// <summary>
        /// 创建文本元素，包含特殊字符的创建
        /// </summary>
        /// <param name="textPath">文本路径信息</param>
        /// <param name="annoRule">文本样式</param>
        /// <returns></returns>
        public static ITextElement CreateTextElement(IGeometry textPath, string annoText, AnnoRule annoRule, out string message, bool RoadLevel = false)
        {
            message = string.Empty;
            string fontFamily = annoRule.FontName;

            bool fontExist = false;
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily ff in fonts.Families)
            {
                if (ff.Name == fontFamily)
                {
                    fontExist = true;
                }
            }
            if (!fontExist)
            {
                message = "系统缺失[" + fontFamily + "]字体库,已被替换为宋体!";

                fontFamily = "宋体";//默认的话给宋体
            }


            //创建文本元素
            ITextElement pTextElment = new TextElementClass();
            pTextElment.ScaleText = true;
            ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElment;
            #region
            pSymbolCollEle.Color = annoRule.FontColor;
            pSymbolCollEle.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
            pSymbolCollEle.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            pSymbolCollEle.XOffset = 0;
            pSymbolCollEle.YOffset = 0;
            pSymbolCollEle.Size = annoRule.FontSize * 2.8345;
            pSymbolCollEle.Text = annoText;
            pSymbolCollEle.FontName = fontFamily;
            #endregion
            pSymbolCollEle.Bold = annoRule.EnableBold;
            pSymbolCollEle.Underline = annoRule.EnableUnderline;

            ////字宽设置
            if (annoRule.CharacterWidth != -1.0)
            {
                pSymbolCollEle.CharacterWidth = annoRule.CharacterWidth;
            }
            ITextSymbol textSymbol = pTextElment.Symbol;
            if (annoRule.HasTextBackground)
            {
                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;
                if (!RoadLevel)//非道路等级的背景样式
                {
                    //设置文本背景框样式
                    IBalloonCallout balloonCallout = new BalloonCalloutClass();
                    balloonCallout.Style = (esriBalloonCalloutStyle)annoRule.BackgroundStyle;
                    // balloonCallout.Style = esriBalloonCalloutStyle.esriBCSOval;
                    //设置文本背景框颜色
                    IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                    fillSymbol.Color = annoRule.BackgroundColor;
                    ILineSymbol lineSymbol = new SimpleLineSymbol();
                    if (annoRule.BackgroundBorderColor != null)
                    {
                        lineSymbol.Width = annoRule.BackgroundBorderWidth;
                        lineSymbol.Color = annoRule.BackgroundBorderColor;
                    }
                    else
                        lineSymbol.Width = 0;
                    fillSymbol.Outline = lineSymbol;
                    balloonCallout.Symbol = fillSymbol;
                    //设置背景框边距
                    ITextMargins textMarigns = balloonCallout as ITextMargins;
                    double margin = annoRule.BackgroundMargin * 2.8345;
                    textMarigns.PutMargins(margin, margin, margin, margin);
                    formattedTextSymbol.Background = balloonCallout as ITextBackground;
                }
                else
                {
                    #region 特殊字符处理，例如带圈的字符
                    //IMarkerTextBackground markerTextBackgound = new MarkerTextBackground();
                    //markerTextBackgound.ScaleToFit = false;
                    //IMultiLayerMarkerSymbol multiLayerMarkerSymbol = new MultiLayerMarkerSymbolClass();
                    ////字符Symbol
                    //IMarkerSymbol CharacterSymbol = new CharacterMarkerSymbolClass();
                    //int CharacterIndex = 0;//字符索引
                    //for (int i = 0; i < SpecialCharacterTable.Rows.Count; i++)
                    //{
                    //    DataRow dr = SpecialCharacterTable.Rows[i];
                    //    if (textSymbol.Text == dr["字符"].ToString())
                    //    {
                    //        CharacterIndex = Convert.ToInt32(dr["字符索引"]);
                    //        break;
                    //    }
                    //}
                    //ICharacterMarkerSymbol CharacterMarkerSymbol = new CharacterMarkerSymbolClass();
                    //CharacterMarkerSymbol.Angle = textSymbol.Angle;
                    //CharacterMarkerSymbol.Color = new RgbColorClass { Blue = 0, Red = 0, Green = 0 };
                    //CharacterMarkerSymbol.Size = textSymbol.Size;
                    //CharacterMarkerSymbol.Font = textSymbol.Font;
                    //CharacterMarkerSymbol.CharacterIndex = CharacterIndex;
                    ////圆形的套底
                    //IMarkerSymbol markSymbol = new SimpleMarkerSymbol();
                    //ISimpleMarkerSymbol simpleMarkerSymbol = markSymbol as ISimpleMarkerSymbol;
                    //simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;//圆形的底色
                    //simpleMarkerSymbol.Outline = false;
                    //simpleMarkerSymbol.Size = textSymbol.Size / 1.25;//字体大小和点符号大小的一个比例系数2.8345
                    //simpleMarkerSymbol.Color = annoStyle.BackgroundColor;
                    ////添加到图层符号中
                    //multiLayerMarkerSymbol.AddLayer(simpleMarkerSymbol as IMarkerSymbol);
                    //multiLayerMarkerSymbol.AddLayer(CharacterMarkerSymbol as IMarkerSymbol);
                    ////设置为背景符号
                    //markerTextBackgound.Symbol = multiLayerMarkerSymbol;
                    //formattedTextSymbol.Background = markerTextBackgound as ITextBackground;
                    ////将原字符颜色设置为透明，仅保留背景符号
                    //textSymbol.Color = new RgbColorClass { Transparency = 0 };

                    #endregion
                }
            }

            if (annoRule.HasHalo)
            {
                IMask mask = textSymbol as IMask;
                mask.MaskStyle = esriMaskStyle.esriMSHalo;
                mask.MaskSize = annoRule.HaloSize * 2.8345;
                IFillSymbol fillSymbol = new SimpleFillSymbol();
                fillSymbol.Color = annoRule.HaloColor;
                ILineSymbol lineSymbol = new SimpleLineSymbol();
                lineSymbol.Width = 0;
                fillSymbol.Outline = lineSymbol;
                mask.MaskSymbol = fillSymbol;
            }
            pTextElment.Symbol = textSymbol;//一定要写回去，否则没有效果
            IElement pEle = pTextElment as IElement;
            pEle.Geometry = textPath as IGeometry;

            return pTextElment;
        }

        /// <summary>
        /// 获取polyline切线方向的角度（弧度）,并保证角度范围在-90~90度之间
        /// </summary>
        /// <param name="pPolyline">线</param>
        /// <returns></returns>
        public static double GetAngle(IPolyline pPolyline, double MapScale)
        {
            //IPolycurve pPolycurve;
            try
            {
                ILine pTangentLine = new Line();
                pPolyline.QueryTangent(esriSegmentExtension.esriNoExtension, 1e-3 * MapScale, true, pPolyline.Length, pTangentLine);
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
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return 0;
            }
        }

        /// <summary>
        /// 返回指定要素的标注文本
        /// </summary>
        /// <param name="lyr"></param>
        /// <param name="annoFieldName"></param>
        /// <param name="expression"></param>
        /// <param name="aee"></param>
        /// <param name="oidList"></param>
        /// <returns></returns>
        public static Dictionary<int, string> getLabelText(IMap map, IFeatureLayer lyr, string annoFieldName, string expression, IAnnotationExpressionEngine aee, List<int> oidList)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

            IGeoFeatureLayer geoLyr = lyr as IGeoFeatureLayer;

            IAnnotateMap annoMap = new AnnotateMapClass();
            map.AnnotationEngine = annoMap;

            IAnnotateLayerPropertiesCollection annoPropColl = geoLyr.AnnotationProperties;
            annoPropColl.Clear();

            IBasicOverposterLayerProperties lyrProp = new BasicOverposterLayerPropertiesClass();
            ILabelEngineLayerProperties leLyrProp = new LabelEngineLayerPropertiesClass();

            ITextSymbol txtSymbol = new TextSymbolClass();

            if (expression == "")
            {
                expression = "[" + annoFieldName + "]";
            }
            else
            {
                leLyrProp.IsExpressionSimple = false;
                leLyrProp.ExpressionParser = new AnnotationVBScriptEngineClass();
            }
            leLyrProp.Expression = expression;

            lyrProp.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
            leLyrProp.BasicOverposterLayerProperties = lyrProp;
            leLyrProp.Symbol = txtSymbol;
            (leLyrProp as IAnnotateLayerProperties).CreateUnplacedElements = true;
            (lyrProp as IOverposterLayerProperties).PlaceLabels = true;

            geoLyr.AnnotationProperties.Add(leLyrProp as IAnnotateLayerProperties);
            geoLyr.DisplayAnnotation = true;

            string fullPath = GetAppDataPath() + "\\MyWorkspace886.gdb";
            IWorkspace ws = createTempWorkspace(fullPath);

            //删除原注记要素类
            deleteDataSet(ws, lyr.FeatureClass.AliasName + "_ANNO");

            //标注转注记，同时获取标注文本
            ITrackCancel tc = new CancelTrackerClass();
            IConvertLabelsToAnnotation convertLTA = new ConvertLabelsToAnnotationClass();

            try
            {
                convertLTA.Initialize(map, esriAnnotationStorageType.esriDatabaseAnnotation, esriLabelWhichFeatures.esriAllFeatures, true, tc, null);

                //转换
                convertLTA.AddFeatureLayer(geoLyr, lyr.FeatureClass.AliasName + "_ANNO", ws as IFeatureWorkspace, null, false, false, false, true, true, "");
                convertLTA.ConvertLabels();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                string err = convertLTA.ErrorInfo;
            }
            geoLyr.DisplayAnnotation = false;

            //遍历
            IEnumLayer pAnnoEnumLayer = convertLTA.AnnoLayers;
            pAnnoEnumLayer.Reset();
            ILayer pLayer = null;
            while ((pLayer = pAnnoEnumLayer.Next()) != null)
            {
                IAnnotationLayer iann = pLayer as IAnnotationLayer;
                if (iann != null)
                {
                    IFeature fe = null;
                    IFeatureCursor cursor = (iann as IFeatureLayer).FeatureClass.Search(null, false);
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        IAnnotationFeature IAnnotationFeature = fe as IAnnotationFeature;
                        if(oidList.Contains(IAnnotationFeature.LinkedFeatureID))
                        {
                            ITextElement pTextElement = ((fe as IAnnotationFeature).Annotation) as ITextElement;
                            if (pTextElement != null)
                            {
                                if(!result.ContainsKey(IAnnotationFeature.LinkedFeatureID))
                                    result.Add(IAnnotationFeature.LinkedFeatureID, pTextElement.Text);
                            }
                        }

                        Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(cursor);
                }
            }

            Marshal.ReleaseComObject(pAnnoEnumLayer);


            return result;
        }
        public static Dictionary<int, string> getSelectLabelText(IMap map, IFeatureLayer lyr, string annoFieldName, string expression, IAnnotationExpressionEngine aee, List<int> oidList)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

            IGeoFeatureLayer geoLyr = lyr as IGeoFeatureLayer;

            IAnnotateMap annoMap = new AnnotateMapClass();
            map.AnnotationEngine = annoMap;

            IAnnotateLayerPropertiesCollection annoPropColl = geoLyr.AnnotationProperties;
            annoPropColl.Clear();

            IBasicOverposterLayerProperties lyrProp = new BasicOverposterLayerPropertiesClass();
            ILabelEngineLayerProperties leLyrProp = new LabelEngineLayerPropertiesClass();

            ITextSymbol txtSymbol = new TextSymbolClass();

            if (expression == "")
            {
                expression = "[" + annoFieldName + "]";
            }
            else
            {
                leLyrProp.IsExpressionSimple = false;
                leLyrProp.ExpressionParser = new AnnotationVBScriptEngineClass();
            }
            leLyrProp.Expression = expression;

            lyrProp.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
            leLyrProp.BasicOverposterLayerProperties = lyrProp;
            leLyrProp.Symbol = txtSymbol;
            (leLyrProp as IAnnotateLayerProperties).CreateUnplacedElements = true;
            (lyrProp as IOverposterLayerProperties).PlaceLabels = true;

            geoLyr.AnnotationProperties.Add(leLyrProp as IAnnotateLayerProperties);
            geoLyr.DisplayAnnotation = true;

            string fullPath = GetAppDataPath() + "\\MyWorkspace886.gdb";
            IWorkspace ws = createTempWorkspace(fullPath);

            //删除原注记要素类
            deleteDataSet(ws, lyr.Name + "_ANNO");

            //标注转注记，同时获取标注文本
            ITrackCancel tc = new CancelTrackerClass();
            IConvertLabelsToAnnotation convertLTA = new ConvertLabelsToAnnotationClass();

            try
            {
                convertLTA.Initialize(map, esriAnnotationStorageType.esriDatabaseAnnotation, esriLabelWhichFeatures.esriSelectedFeatures, true, tc, null);

                //转换
                convertLTA.AddFeatureLayer(geoLyr, lyr.Name + "_ANNO", ws as IFeatureWorkspace, null, false, false, false, true, true, "");
                convertLTA.ConvertLabels();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                string err = convertLTA.ErrorInfo;
            }
            geoLyr.DisplayAnnotation = false;

            //遍历
            IEnumLayer pAnnoEnumLayer = convertLTA.AnnoLayers;
            pAnnoEnumLayer.Reset();
            ILayer pLayer = null;
            while ((pLayer = pAnnoEnumLayer.Next()) != null)
            {
                IAnnotationLayer iann = pLayer as IAnnotationLayer;
                if (iann != null)
                {
                    IFeature fe = null;
                    IFeatureCursor cursor = (iann as IFeatureLayer).FeatureClass.Search(null, false);
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        IAnnotationFeature IAnnotationFeature = fe as IAnnotationFeature;
                        if (oidList.Contains(IAnnotationFeature.LinkedFeatureID))
                        {
                            ITextElement pTextElement = ((fe as IAnnotationFeature).Annotation) as ITextElement;
                            if (pTextElement != null)
                            {
                                if (!result.ContainsKey(IAnnotationFeature.LinkedFeatureID))
                                    result.Add(IAnnotationFeature.LinkedFeatureID, pTextElement.Text);
                            }
                        }

                        Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(cursor);
                }
            }

            Marshal.ReleaseComObject(pAnnoEnumLayer);


            return result;
        }

        /// <summary>
        /// 获取应用程序默认路径
        /// </summary>
        public static string GetAppDataPath()
        {
            if (System.Environment.OSVersion.Version.Major <= 5)
            {
                return System.IO.Path.GetFullPath(
                    System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\..");
            }

            var dp = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var di = new System.IO.DirectoryInfo(dp);
            var ds = di.GetDirectories("SMGI");
            if (ds == null || ds.Length == 0)
            {
                var sdi = di.CreateSubdirectory("SMGI");
                return sdi.FullName;
            }
            else
            {
                return ds[0].FullName;
            }
        }

        /// <summary>
        /// 创建临时工作空间
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static IWorkspace createTempWorkspace(string fullPath)
        {
            IWorkspace pWorkspace = null;
            IWorkspaceFactory2 wsFactory = new FileGDBWorkspaceFactoryClass();

            if (!Directory.Exists(fullPath))
            {

                IWorkspaceName pWorkspaceName = wsFactory.Create(System.IO.Path.GetDirectoryName(fullPath),
                    System.IO.Path.GetFileName(fullPath), null, 0);
                IName pName = (IName)pWorkspaceName;
                pWorkspace = (IWorkspace)pName.Open();
            }
            else
            {
                pWorkspace = wsFactory.OpenFromFile(fullPath, 0);
            }



            return pWorkspace;
        }
        
     
        private static IFields createFeatureClassFields(IFeatureClass pSourceFeatureClass, ISpatialReference outputSpatialReference = null)
        {
            //获取源要素类的字段结构信息
            IFields targetFields = null;
            IObjectClassDescription featureDescription = new FeatureClassDescriptionClass();
            targetFields = featureDescription.RequiredFields; //要素类自带字段
            for (int i = 0; i < pSourceFeatureClass.Fields.FieldCount; ++i)
            {
                IField field = pSourceFeatureClass.Fields.get_Field(i);

                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    (targetFields as IFieldsEdit).set_Field(targetFields.FindFieldByAliasName((featureDescription as IFeatureClassDescription).ShapeFieldName),
                        (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField);

                    continue;
                }
                if (field.Type == esriFieldType.esriFieldTypeOID)
                    continue;
                if (targetFields.FindField(field.Name) != -1)//已包含该字段（要素类自带字段）
                {
                    continue;
                }

                //剔除sde数据中的"st_area_shape_"、"st_length_shape_"
                if ("st_area_shape_" == field.Name.ToLower() || "st_length_shape_" == field.Name.ToLower())
                {
                    continue;
                }

                IField newField = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (targetFields as IFieldsEdit).AddField(newField);
            }

            IGeometryDef pGeometryDef = new GeometryDefClass();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            if (null == outputSpatialReference)
                outputSpatialReference = (pSourceFeatureClass as IGeoDataset).SpatialReference;
            pGeometryDefEdit.SpatialReference_2 = outputSpatialReference;
            for (int i = 0; i < targetFields.FieldCount; i++)
            {
                IField pfield = targetFields.get_Field(i);
                if (pfield.Type == esriFieldType.esriFieldTypeOID)
                {
                    IFieldEdit pFieldEdit = (IFieldEdit)pfield;
                    pFieldEdit.Name_2 = pfield.AliasName;
                }

                if (pfield.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    pGeometryDefEdit.GeometryType_2 = pfield.GeometryDef.GeometryType;
                    IFieldEdit pFieldEdit = (IFieldEdit)pfield;
                    pFieldEdit.Name_2 = pfield.AliasName;
                    pFieldEdit.GeometryDef_2 = pGeometryDef;
                    break;
                }

            }


            return targetFields;
        }

        public static void clearTempWorkspace()
        {
            string fullpath = GetAppDataPath() + "\\MyWorkspace886.gdb";
            if (Directory.Exists(fullpath))
            {
                IWorkspace pWorkspace = null;
                IWorkspaceFactory2 wsFactory = new FileGDBWorkspaceFactoryClass();
                pWorkspace = wsFactory.OpenFromFile(fullpath, 0);
                IEnumDataset dsEnum = pWorkspace.get_Datasets(esriDatasetType.esriDTAny);
                dsEnum.Reset();
                IDataset ds = null;
                while ((ds = dsEnum.Next()) != null)
                {
                    if (ds.CanDelete())
                    {
                        ds.Delete();
                    }
                }
                Marshal.ReleaseComObject(dsEnum);
                Marshal.ReleaseComObject(pWorkspace);
            }
        }
        //从数据库中删除数据
        public static void deleteDataSet(IWorkspace ws, string datasetName)
        {
            if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, datasetName))
            {
                IFeatureClass fc = (ws as IFeatureWorkspace).OpenFeatureClass(datasetName);
                IDataset dt = fc as IDataset;
                if (dt.CanDelete())
                    dt.Delete();
            }
            if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, datasetName))
            {
                var fc = (ws as IFeatureWorkspace).OpenTable(datasetName);
                IDataset dt = fc as IDataset;
                if (dt.CanDelete())
                    dt.Delete();
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
                pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;//水平居中
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

    }

    /// <summary>
    /// 注记相关信息的数据结构
    /// </summary>
    public class labelEngineAndAnnoRule
    {
        public ILabelEngineLayerProperties2 LabelEngine;
        public AnnoRule AnnotationRule;
        public int GroupSymbolID;
        public int ClassIndex;
    }

}

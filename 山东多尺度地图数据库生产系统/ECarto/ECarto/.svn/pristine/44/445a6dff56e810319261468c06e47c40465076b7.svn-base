using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Data;
using System.Runtime.InteropServices;
using SMGI.Common;

namespace SMGI.Plugin.AnnotationEngine
{
    public class AnnotationHelper
    {
        /// <summary>
        /// 根据颜色字符串实例化颜色对象
        /// </summary>
        /// <param name="clrStr">C100M100 或 R255G125</param>
        /// <returns>若包含非法字符，则返回无色</returns>
        public static IColor GetColor(string clrStr)
        {
            IColor clr = new CmykColorClass();

            char[] cmyk = { 'C', 'M', 'Y', 'K' };
            char[] rgb = { 'R', 'G', 'B' };

            clrStr = clrStr.ToUpper();//转换为大写
            if (clrStr.IndexOfAny(cmyk) != -1)
            {
                clr = GetmykColor(clrStr);
            }
            else if (clrStr.IndexOfAny(rgb) != -1)
            {
                clr = GetRgbColor(clrStr);
            }
            else
            {
                clr.NullColor = true;
            }

            return clr;
        }

        /// <summary>
        /// 根据RGB字符串实例化RGB颜色对象
        /// </summary>
        /// <param name="rgb">RGB字符串（形如：R255G100B50）</param>
        /// <returns>RGB颜色对象</returns>
        public static IRgbColor GetRgbColor(string rgb)
        {
            //新建一个RGB颜色，然后各项值赋为0（黑色）
            IRgbColor rgb_Color = new RgbColorClass();
            rgb_Color.Red = 0;
            rgb_Color.Green = 0;
            rgb_Color.Blue = 0;

            try
            {
                char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                StringBuilder sb = new StringBuilder();

                rgb = rgb.ToUpper();//转换为大写
                for (int i = 0; i <= rgb.Length; i++)
                {
                    if (i == rgb.Length)
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('R'))
                        {
                            rgb_Color.Red = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('G'))
                        {
                            rgb_Color.Green = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('B'))
                        {
                            rgb_Color.Blue = int.Parse(sbs.Substring(1));
                        }
                        break;
                    }
                    else
                    {
                        char C = rgb[i];
                        if (D.Contains(C))
                        {
                            sb.Append(C);
                        }
                        else
                        {
                            string sbs = sb.ToString();
                            if (sbs.Contains('R'))
                            {
                                rgb_Color.Red = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('G'))
                            {
                                rgb_Color.Green = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('B'))
                            {
                                rgb_Color.Blue = int.Parse(sbs.Substring(1));
                            }
                            sb.Clear();
                            sb.Append(C);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                //包含非法字符,返回无色
                MessageBox.Show(string.Format("颜色字符串【{0}】中包含非法字符,该颜色将会用无色替代！", rgb));

                rgb_Color = new RgbColorClass() { NullColor = true };
            }

            return rgb_Color;
        }



        /// <summary>
        /// 根据CMYK字符串实例化CMYK颜色对象
        /// </summary>
        /// <param name="cmyk">cmyk字符串（形如：C100M200Y100K50）</param>
        /// <returns>CMYK颜色对象</returns>
        public static ICmykColor GetmykColor(string cmyk)
        {
            //新建一个CMYK颜色，然后各项值赋为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;

            try
            {
                char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                StringBuilder sb = new StringBuilder();

                cmyk = cmyk.ToUpper();//转换为大写
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
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                //包含非法字符,返回无色
                MessageBox.Show(string.Format("颜色字符串【{0}】中包含非法字符,该颜色将会用无色替代！", cmyk));

                CMYK_Color = new CmykColorClass() { NullColor = true };
            }

            return CMYK_Color;
        }

        /// <summary>
        /// 创建文本符号
        /// </summary>
        /// <param name="annoRule"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ITextSymbol CreateTextSymbol(AnnotationRule annoRule, out string message)
        {
            message = string.Empty;

            ITextSymbol textSymbol = new TextSymbolClass();

            stdole.IFontDisp fontDisp = new stdole.StdFontClass() as stdole.IFontDisp;
            #region 字体
            string fontName = annoRule.FontName;
            bool fontExist = false;
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily ff in fonts.Families)
            {
                if (ff.Name == fontName)
                {
                    fontExist = true;
                }
            }
            if (!fontExist)
            {
                message = "系统缺失[" + annoRule.FontName + "]字体库";

                fontName = "宋体";//默认的话给宋体
            }

            
            fontDisp.Name = fontName;
            fontDisp.Bold = annoRule.EnableBold;
            fontDisp.Italic = annoRule.EnableItalic;
            fontDisp.Underline = annoRule.EnableUnderline;
            #endregion

            textSymbol.Font = fontDisp;
            if (annoRule.FontSize > 0)
                textSymbol.Size = annoRule.FontSize * 2.8345;//毫米转磅
            if (annoRule.FontColor != null)
                textSymbol.Color = annoRule.FontColor;
            (textSymbol as ICharacterOrientation).CJKCharactersRotation = annoRule.EnableCJKCharactersRotation;

            IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;
            formattedTextSymbol.CharacterWidth = annoRule.CharacterWidth;

            //气泡
            if (annoRule.EnableTextBackground)
            {
                //设置文本背景框样式
                IBalloonCallout balloonCallout = new BalloonCalloutClass();
                balloonCallout.Style = annoRule.BackgroundStyle;

                //设置文本背景框颜色
                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = annoRule.BackgroundColor;
                fillSymbol.Outline = new SimpleLineSymbol() { Width = annoRule.BackgroundBorderWidth, Color = annoRule.BackgroundBorderColor};
                balloonCallout.Symbol = fillSymbol;

                //设置背景框边距
                ITextMargins textMarigns = balloonCallout as ITextMargins;
                double margin = annoRule.BackgroundMargin;
                textMarigns.PutMargins(margin, margin, margin, margin);

                formattedTextSymbol.Background = balloonCallout as ITextBackground;
            }

            //晕渲
            if (annoRule.EnableHalo)
            {
                IMask mask = textSymbol as IMask;
                mask.MaskStyle = esriMaskStyle.esriMSHalo;
                mask.MaskSize = annoRule.HaloSize * 2.8345;//毫米转磅
                IFillSymbol fillSymbol = new SimpleFillSymbol();
                fillSymbol.Color = annoRule.HaloColor;
                fillSymbol.Outline = new SimpleLineSymbol(){ Width = 0};
                mask.MaskSymbol = fillSymbol;
            }

            return textSymbol;
        }

        /// <summary>
        /// 复制注记文本
        /// </summary>
        /// <param name="textElment"></param>
        /// <param name="annoRule"></param>
        /// <returns></returns>
        public static ITextElement CopyTextElement(ITextElement textElment)
        {
            //创建文本元素
            ITextElement newTextElment = new TextElementClass();
            newTextElment.ScaleText = textElment.ScaleText;
            newTextElment.Text = textElment.Text;

            ISymbolCollectionElement symbolCollEle = textElment as ISymbolCollectionElement;

            if (symbolCollEle.AnchorPoint != null)
                (newTextElment as ISymbolCollectionElement).AnchorPoint = (symbolCollEle.AnchorPoint as IClone).Clone() as IPoint;
            if (symbolCollEle.Background != null)
                (newTextElment as ISymbolCollectionElement).Background = (symbolCollEle.Background as IClone).Clone() as ITextBackground;
            (newTextElment as ISymbolCollectionElement).Bold = symbolCollEle.Bold;
            (newTextElment as ISymbolCollectionElement).CharacterSpacing = symbolCollEle.CharacterSpacing;
            (newTextElment as ISymbolCollectionElement).CharacterWidth = symbolCollEle.CharacterWidth;
            if (symbolCollEle.Color != null)
                (newTextElment as ISymbolCollectionElement).Color = (symbolCollEle.Color as IClone).Clone() as IColor;
            (newTextElment as ISymbolCollectionElement).FlipAngle = symbolCollEle.FlipAngle;
            (newTextElment as ISymbolCollectionElement).FontName = symbolCollEle.FontName;
            if (symbolCollEle.Geometry != null)
                (newTextElment as ISymbolCollectionElement).Geometry = (symbolCollEle.Geometry as IClone).Clone() as IGeometry;
            (newTextElment as ISymbolCollectionElement).HorizontalAlignment = symbolCollEle.HorizontalAlignment;
            (newTextElment as ISymbolCollectionElement).Italic = symbolCollEle.Italic;
            (newTextElment as ISymbolCollectionElement).Leading = symbolCollEle.Leading;
            (newTextElment as ISymbolCollectionElement).Size = symbolCollEle.Size;
            (newTextElment as ISymbolCollectionElement).TextPath = symbolCollEle.TextPath;
            (newTextElment as ISymbolCollectionElement).Underline = symbolCollEle.Underline;
            (newTextElment as ISymbolCollectionElement).VerticalAlignment = symbolCollEle.VerticalAlignment;
            (newTextElment as ISymbolCollectionElement).WordSpacing = symbolCollEle.WordSpacing;
            (newTextElment as ISymbolCollectionElement).XOffset = symbolCollEle.XOffset;
            (newTextElment as ISymbolCollectionElement).YOffset = symbolCollEle.YOffset;


            ITextSymbol newTextSymbol = (textElment.Symbol as IClone).Clone() as ITextSymbol;

            newTextElment.Symbol = newTextSymbol;

            return newTextElment;
        }


        /// <summary>
        /// 创建文本元素，包含特殊字符的创建
        /// </summary>
        /// <param name="textPath"></param>
        /// <param name="annoText"></param>
        /// <param name="annoRule"></param>
        /// <param name="specialCharacterTable"></param>
        /// <param name="hAlignment"></param>
        /// <returns></returns>
        public static ITextElement CreateTextElement(IGeometry textPath, string annoText, AnnotationRule annoRule, DataTable specialCharacterTable = null, esriTextVerticalAlignment vAlignment = esriTextVerticalAlignment.esriTVACenter, esriTextHorizontalAlignment hAlignment = esriTextHorizontalAlignment.esriTHACenter)
        {
            ITextElement newTextElment = null;

            try
            {
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
                    fontFamily = "宋体";//默认的话给宋体
                }


                //创建文本元素
                newTextElment = new TextElementClass();
                newTextElment.ScaleText = true;
                newTextElment.Text = annoText;

                (newTextElment as ISymbolCollectionElement).FontName = annoRule.FontName;
                (newTextElment as ISymbolCollectionElement).Color = annoRule.FontColor;
                (newTextElment as ISymbolCollectionElement).Size = annoRule.FontSize * 2.8345;//毫米转磅
                (newTextElment as ISymbolCollectionElement).Bold = annoRule.EnableBold;
                (newTextElment as ISymbolCollectionElement).Italic = annoRule.EnableItalic;
                (newTextElment as ISymbolCollectionElement).Underline = annoRule.EnableUnderline;
                (newTextElment as ISymbolCollectionElement).VerticalAlignment = vAlignment;
                (newTextElment as ISymbolCollectionElement).HorizontalAlignment = hAlignment;
                (newTextElment as ISymbolCollectionElement).CharacterWidth = annoRule.CharacterWidth;
                (newTextElment as ISymbolCollectionElement).CharacterSpacing = annoRule.MaximumCharacterSpacing;
                (newTextElment as ISymbolCollectionElement).WordSpacing = annoRule.MaximumWordSpacing;


                ITextSymbol textSymbol = newTextElment.Symbol;
                #region 文本背景
                if (annoRule.EnableTextBackground)
                {
                    IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;
                    if (specialCharacterTable == null)//普通的文本背景
                    {
                        //设置文本背景框样式
                        IBalloonCallout balloonCallout = new BalloonCalloutClass();
                        balloonCallout.Style = annoRule.BackgroundStyle;

                        //设置文本背景框颜色
                        IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                        fillSymbol.Color = annoRule.BackgroundColor;
                        fillSymbol.Outline = new SimpleLineSymbol() { Width = annoRule.BackgroundBorderWidth, Color = annoRule.BackgroundBorderColor };
                        balloonCallout.Symbol = fillSymbol;

                        //设置背景框边距
                        ITextMargins textMarigns = balloonCallout as ITextMargins;
                        double margin = annoRule.BackgroundMargin;
                        textMarigns.PutMargins(margin, margin, margin, margin);

                        formattedTextSymbol.Background = balloonCallout as ITextBackground;
                    }
                    else//通过特殊字符实现的文本背景，如带圈的字符
                    {
                        IMarkerTextBackground markerTextBackgound = new MarkerTextBackground();
                        markerTextBackgound.ScaleToFit = false;
                        IMultiLayerMarkerSymbol multiLayerMarkerSymbol = new MultiLayerMarkerSymbolClass();

                        int CharacterIndex = 0;//字符索引
                        for (int i = 0; i < specialCharacterTable.Rows.Count; i++)
                        {
                            DataRow dr = specialCharacterTable.Rows[i];
                            if (textSymbol.Text == dr["字符"].ToString())
                            {
                                CharacterIndex = Convert.ToInt32(dr["字符索引"]);
                                break;
                            }
                        }

                        ICharacterMarkerSymbol CharacterMarkerSymbol = new CharacterMarkerSymbolClass();
                        CharacterMarkerSymbol.Angle = textSymbol.Angle;
                        CharacterMarkerSymbol.Color = new RgbColorClass { Blue = 0, Red = 0, Green = 0 };
                        CharacterMarkerSymbol.Size = textSymbol.Size;
                        CharacterMarkerSymbol.Font = textSymbol.Font;
                        CharacterMarkerSymbol.CharacterIndex = CharacterIndex;
                        //圆形的套底
                        IMarkerSymbol markSymbol = new SimpleMarkerSymbol();
                        ISimpleMarkerSymbol simpleMarkerSymbol = markSymbol as ISimpleMarkerSymbol;
                        simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;//圆形的底色
                        simpleMarkerSymbol.Outline = false;
                        simpleMarkerSymbol.Size = textSymbol.Size / 1.1;
                        simpleMarkerSymbol.Color = annoRule.BackgroundColor;
                        //添加到图层符号中
                        multiLayerMarkerSymbol.AddLayer(simpleMarkerSymbol as IMarkerSymbol);
                        multiLayerMarkerSymbol.AddLayer(CharacterMarkerSymbol as IMarkerSymbol);
                        //设置为背景符号
                        markerTextBackgound.Symbol = multiLayerMarkerSymbol;
                        formattedTextSymbol.Background = markerTextBackgound as ITextBackground;
                        //设置原字符颜色(透明)
                        textSymbol.Color = new RgbColorClass { Transparency = 0 };
                    }
                }
                #endregion
                #region 晕渲
                if (annoRule.EnableHalo)
                {
                    IMask mask = textSymbol as IMask;
                    mask.MaskStyle = esriMaskStyle.esriMSHalo;
                    mask.MaskSize = annoRule.HaloSize * 2.8345;//毫米转磅
                    IFillSymbol fillSymbol = new SimpleFillSymbol();
                    fillSymbol.Color = annoRule.HaloColor;
                    fillSymbol.Outline = new SimpleLineSymbol() { Width = 0 };
                    mask.MaskSymbol = fillSymbol;
                }
                #endregion
                (textSymbol as ICharacterOrientation).CJKCharactersRotation = annoRule.EnableCJKCharactersRotation;

                newTextElment.Symbol = textSymbol;//一定要写回去，否则没有效果
                (newTextElment as IElement).Geometry = textPath as IGeometry;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return newTextElment;
        }

        /// <summary>
        /// 弧度值规范化处理
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static double RadianNormalization(double rad)
        {
            double angle = rad * 180 / Math.PI;
            while (angle < 0)
            {
                angle += 360;
            }

            double angle_Mod = angle % 360;

            #region 限制角度范围在[0,90]或(270,360)度之间
            if (angle_Mod > 90 && angle_Mod < 180)
                angle_Mod += 180;
            if (angle_Mod >= 180 && angle_Mod <= 270)
                angle_Mod -= 180;
            #endregion

            #region 限制角度范围在[0,45)或[225,360)度之间
            //if (angle_Mod >= 45 && angle_Mod < 180)
            //    angle_Mod += 180;
            //if (angle_Mod >= 180 && angle_Mod < 225)
            //    angle_Mod -= 180;
            #endregion

            // 返回弧度
            return angle_Mod * Math.PI / 180;
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
        public static Dictionary<int, string> getFeatureLabelText(IMap map, IFeatureLayer lyr, List<int> oidList, string annoFieldName, string expression)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

            IGeoFeatureLayer geoLyr = lyr as IGeoFeatureLayer;


            ILabelEngineLayerProperties leLyrProp = new LabelEngineLayerPropertiesClass();
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

            IBasicOverposterLayerProperties lyrProp = new BasicOverposterLayerPropertiesClass();
            lyrProp.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
            leLyrProp.BasicOverposterLayerProperties = lyrProp;
            (leLyrProp as IAnnotateLayerProperties).CreateUnplacedElements = true;
            (lyrProp as IOverposterLayerProperties).PlaceLabels = true;

            IAnnotateLayerPropertiesCollection annoPropColl = (geoLyr.AnnotationProperties as IClone).Clone() as IAnnotateLayerPropertiesCollection;
            geoLyr.AnnotationProperties.Clear();
            bool bDispAnno = geoLyr.DisplayAnnotation;
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
                convertLTA.Initialize(map, esriAnnotationStorageType.esriDatabaseAnnotation, esriLabelWhichFeatures.esriAllFeatures, true, tc, null);

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
                throw new Exception(err);
            }
            finally
            {
                geoLyr.AnnotationProperties = annoPropColl;
                geoLyr.DisplayAnnotation = bDispAnno;
            }
            

            //遍历
            IEnumLayer annoEnumLayer = convertLTA.AnnoLayers;
            annoEnumLayer.Reset();
            ILayer layer = null;
            while ((layer = annoEnumLayer.Next()) != null)
            {
                IAnnotationLayer iann = layer as IAnnotationLayer;
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

            Marshal.ReleaseComObject(annoEnumLayer);


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
        }


        public static string InsertSpaceInChinese(string text)
        {
            string newText = "";
            for (int i = 0; i < text.Length; ++i)
            {
                newText += text[i];
                if (i == text.Length - 1)
                    break;

                if ((int)text[i] > 127)//中文字符
                {
                    if (i + 2 < text.Length)
                    {
                        if ((int)text[i + 1] != 13 && (int)text[i + 1] != 10)//后面不是换行符(\r\n)
                        {
                            newText += " ";
                        }
                    }
                    else
                    {
                        newText += " ";
                    }

                }
            }

            return newText;
        }
        public static string DelSpaceAfterChinese(string text)
        {
            string newText = "";

            bool lastCharIsChinese = false;
            for (int i = 0; i < text.Length; ++i)
            {
                if (lastCharIsChinese && text[i] == ' ')//上一个字符为中文，当前字符为空格，则删除当前空格
                {
                    //直接跳过该空格字符
                }
                else
                {
                    newText += text[i];
                }

                if ((int)text[i] > 127)//判断当前字符为中文字符
                {
                    lastCharIsChinese = true;
                }
                else
                {
                    lastCharIsChinese = false;
                }
            }

            return newText;
        }

        public static string ReplaceChineseChar(string text, ref List<char> charList, char replaceChar)
        {
            string newText = "";
            for (int i = 0; i < text.Length; ++i)
            {
                if ((int)text[i] <= 127 || i == text.Length - 1)//非中文字符 || 最后一个字符
                {
                    newText += text[i];
                }
                else
                {
                    if ((int)text[i] > 127)//中文字符
                    {
                        if (i + 2 < text.Length)
                        {
                            if ((int)text[i + 1] != 13 && (int)text[i + 1] != 10)//后面不是换行符(\r\n)
                            {
                                newText += string.Format("{0} ", replaceChar);
                                charList.Add(text[i]);
                            }
                            else//中文字符后紧跟\r\n
                            {
                                newText += text[i];
                            }
                        }
                        else
                        {
                            newText += string.Format("{0} ", replaceChar);
                            charList.Add(text[i]);
                        }

                    }
                }
            }

            return newText;
        }

        /// <summary>
        /// 要素类注记消隐(局部消隐线)
        /// </summary>
        /// <param name="fcName"></param>
        /// <param name="annoFCName"></param>
        /// <param name="annoFIDList"></param>
        /// <param name="onlySelf"></param>
        public static void AnnotationBlankingByDash(string fcName, string annoFCName, List<int> annoFIDList, bool onlySelf = true)
        {
            if (annoFIDList.Count == 0)
                return;

            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName.ToUpper();
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null)
                return;
            //获取选取要素制图表达规则
            var feRenderer = (lyr as IGeoFeatureLayer).Renderer;
            if (!(feRenderer is IRepresentationRenderer)) return;
            var repClass = ((IRepresentationRenderer)feRenderer).RepresentationClass;
            IMapContext mapContext = new MapContextClass();
            mapContext.InitFromDisplay(GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation);
            if (lyr.FeatureClass == null)
                return;
            IFeatureClass fc = lyr.FeatureClass;


            lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IFDOGraphicsLayer && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == annoFCName.ToUpper();
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null)
            {
                return;
            }
            if (lyr.FeatureClass == null)
                return;
            IFeatureClass annoFC = lyr.FeatureClass;

            string oidSet = "";
            foreach (var oid in annoFIDList)
            {
                if (oidSet != "")
                    oidSet += string.Format(",{0}", oid);
                else
                    oidSet = string.Format("{0}", oid);
            }
            string filter = string.Format("OBJECTID in ({0})", oidSet);

            IQueryFilter qf = new QueryFilterClass() { WhereClause = filter };
            IFeatureCursor feCursor = annoFC.Search(qf, false);
            IFeature annoFe = null;
            while ((annoFe = feCursor.NextFeature()) != null)
            {
                int fid = (annoFe as IAnnotationFeature2).LinkedFeatureID;
                if ((annoFe as IAnnotationFeature2).Status == esriAnnotationStatus.esriAnnoStatusUnplaced)
                    continue;//不显示注记

                IPolygon annoShape = annoFe.Shape as IPolygon;
                var reps = new List<IRepresentation>();
                if (onlySelf)//仅消隐关联要素本身
                {
                    IFeature objFe = fc.GetFeature(fid);
                    var rep = repClass.GetRepresentation(objFe, mapContext);
                    reps.Add(rep);

                    Marshal.ReleaseComObject(objFe);
                }
                else//消隐压盖的同类所有要素
                {
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = annoShape;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeatureCursor objFeCursor = fc.Search(sf, false);
                    IFeature objFe = null;
                    while ((objFe = objFeCursor.NextFeature()) != null)
                    {
                        var rep = repClass.GetRepresentation(objFe, mapContext);
                        reps.Add(rep);

                        Marshal.ReleaseComObject(objFe);
                    }
                    Marshal.ReleaseComObject(objFeCursor);
                }

                //消隐
                DashByGeometry(reps, annoShape);
            }
            Marshal.ReleaseComObject(feCursor);

        }
        //消隐（局部消隐线）
        public static void DashByGeometry(IEnumerable<IRepresentation> reps, IGeometry geo)
        {
            ITopologicalOperator topo = geo as ITopologicalOperator;
            topo.Simplify();

            foreach (var rep in reps)
            {
                if (rep.Graphics != null || (rep.Shape.GeometryType != esriGeometryType.esriGeometryPolyline && rep.Shape.GeometryType != esriGeometryType.esriGeometryPolygon)) 
                    continue;

                bool isSplit;
                int splitIndex, segIndex;
                var repGeo = rep.ShapeCopy;
                var pts = (IGeometryCollection)topo.Intersect(repGeo, esriGeometryDimension.esriGeometry0Dimension);
                for (var i = 0; i < pts.GeometryCount; i++)
                    (repGeo as IPolycurve).SplitAtPoint(pts.Geometry[i] as IPoint, false, false, out isSplit, out splitIndex, out segIndex);

                var intersLine = topo.Intersect(repGeo.GeometryType == esriGeometryType.esriGeometryPolygon ? ((ITopologicalOperator)repGeo).Boundary : repGeo, esriGeometryDimension.esriGeometry1Dimension);
                var segs = repGeo as ISegmentCollection;
                (repGeo as IPointIDAware).PointIDAware = true;
                for (var j = 0; j < segs.SegmentCount; j++)
                {
                    var seg = segs.Segment[j];
                    if ((intersLine as IRelationalOperator).Disjoint(seg.FromPoint) || (intersLine as IRelationalOperator).Disjoint(seg.ToPoint)) 
                        continue;
                    HideByPoint(repGeo as IPointCollection, seg.FromPoint);
                    rep.Shape = repGeo;

                    rep.UpdateFeature();
                    rep.Feature.Store();
                }
            }
        }
        //消隐恢复（局部消隐线）
        public static void DashRecoveryByGeometry(IEnumerable<IRepresentation> reps, IGeometry geo)
        {
            ITopologicalOperator topo = geo as ITopologicalOperator;
            topo.Simplify();
            foreach (var rep in reps)
            {
                if (rep.Graphics != null || (rep.Shape.GeometryType != esriGeometryType.esriGeometryPolyline && rep.Shape.GeometryType != esriGeometryType.esriGeometryPolygon))
                    continue;

                var repGeo = rep.ShapeCopy;
                var intersLine = topo.Intersect(repGeo.GeometryType == esriGeometryType.esriGeometryPolygon ? ((ITopologicalOperator)repGeo).Boundary : repGeo, esriGeometryDimension.esriGeometry1Dimension);
                var segs = (ISegmentCollection)repGeo;
                ((IPointIDAware)repGeo).PointIDAware = true;
                for (var j = 0; j < segs.SegmentCount; j++)
                {
                    var seg = segs.Segment[j];
                    if ((intersLine as IRelationalOperator).Disjoint(seg.FromPoint) || (intersLine as IRelationalOperator).Disjoint(seg.ToPoint)) 
                        continue;
                    HideByPoint(repGeo as IPointCollection, seg.FromPoint, true);
                    rep.Shape = repGeo;

                    rep.UpdateFeature();
                    rep.Feature.Store();
                }
            }
        }
        //消隐处理
        public static void HideByPoint(IPointCollection pc, IPoint qpt, bool isRecovery = false)
        {
            var rela = (IRelationalOperator)qpt;
            for (var i = 0; i < pc.PointCount; i++)
            {
                var pt = pc.Point[i];
                if (rela.Disjoint(pt)) continue;

                ((IPointIDAware)pt).PointIDAware = true;
                pt.ID = isRecovery ? 0 : -999;
                pc.UpdatePoint(i, pt);
            }
        }

        /// <summary>
        /// 要素类注记消隐（几何覆盖）
        /// </summary>
        /// <param name="fcName"></param>
        /// <param name="annoFCName"></param>
        /// <param name="annoFIDList"></param>
        /// <param name="onlySelf"></param>
        public static void AnnotationBlankingByOverride(string fcName, string annoFCName, List<int> annoFIDList, bool onlySelf = true)
        {
            if (annoFIDList.Count == 0)
                return;

            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName.ToUpper();
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null)
                return;
            //获取选取要素制图表达规则
            var feRenderer = (lyr as IGeoFeatureLayer).Renderer;
            if (!(feRenderer is IRepresentationRenderer)) return;
            var repClass = ((IRepresentationRenderer)feRenderer).RepresentationClass;
            IMapContext mapContext = new MapContextClass();
            mapContext.InitFromDisplay(GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation);
            if (lyr.FeatureClass == null)
                return;
            IFeatureClass fc = lyr.FeatureClass;


            lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IFDOGraphicsLayer && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == annoFCName.ToUpper();
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null)
            {
                return;
            }
            if (lyr.FeatureClass == null)
                return;
            IFeatureClass annoFC = lyr.FeatureClass;

            string oidSet = "";
            foreach (var oid in annoFIDList)
            {
                if (oidSet != "")
                    oidSet += string.Format(",{0}", oid);
                else
                    oidSet = string.Format("{0}", oid);
            }
            string filter = string.Format("OBJECTID in ({0})", oidSet);

            IQueryFilter qf = new QueryFilterClass() { WhereClause = filter };
            IFeatureCursor feCursor = annoFC.Search(qf, false);
            IFeature annoFe = null;
            while ((annoFe = feCursor.NextFeature()) != null)
            {
                int fid = (annoFe as IAnnotationFeature2).LinkedFeatureID;
                if ((annoFe as IAnnotationFeature2).Status == esriAnnotationStatus.esriAnnoStatusUnplaced)
                    continue;//不显示注记

                IPolygon annoShape = annoFe.Shape as IPolygon;
                var reps = new List<IRepresentation>();
                if (onlySelf)//仅消隐关联要素本身
                {
                    IFeature objFe = fc.GetFeature(fid);
                    var rep = repClass.GetRepresentation(objFe, mapContext);
                    reps.Add(rep);

                    Marshal.ReleaseComObject(objFe);
                }
                else//消隐压盖的同类所有要素
                {
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = annoShape;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeatureCursor objFeCursor = fc.Search(sf, false);
                    IFeature objFe = null;
                    while ((objFe = objFeCursor.NextFeature()) != null)
                    {
                        var rep = repClass.GetRepresentation(objFe, mapContext);
                        reps.Add(rep);

                        Marshal.ReleaseComObject(objFe);
                    }
                    Marshal.ReleaseComObject(objFeCursor);
                }

                //消隐
                OverrideByGeometry(reps, annoShape);
            }
            Marshal.ReleaseComObject(feCursor);

        }
        //消隐（几何覆盖）
        public static void OverrideByGeometry(IEnumerable<IRepresentation> reps, IGeometry geo)
        {
            foreach (var rep in reps)
            {
                if (rep.Graphics == null)
                {
                    var ogeo = rep.ShapeCopy;
                    var topo = (ITopologicalOperator)ogeo;
                    var ngeo = topo.Difference(geo);
                    if (ngeo.IsEmpty)
                    {
                        rep.Shape = ogeo;
                        rep.Shape.SetEmpty();
                    }
                    else
                        rep.Shape = ngeo;
                }
                else
                {
                    rep.Graphics.ResetGeometry();
                    while (true)
                    {
                        int id;
                        IGeometry ogeo;
                        rep.Graphics.NextGeometry(out id, out ogeo);
                        if (ogeo == null) break;
                        var topo = (ITopologicalOperator)ogeo;
                        var ngeo = topo.Difference(geo);
                        rep.Graphics.ChangeGeometry(id, ngeo);
                    }
                }
                rep.UpdateFeature();
                rep.Feature.Store();
            }
        }
        //消隐恢复（几何覆盖）
        public static void OverrideRecoveryByGeometry(IEnumerable<IRepresentation> reps, IGeometry geo)
        {
            ITopologicalOperator topo = geo as ITopologicalOperator;
            topo.Simplify();
            foreach (var rep in reps)
            {
                if (rep.Graphics != null || (rep.Shape.GeometryType != esriGeometryType.esriGeometryPolyline && rep.Shape.GeometryType != esriGeometryType.esriGeometryPolygon)) 
                    continue;
                var interGeo = topo.Intersect(rep.Feature.Shape, rep.Shape.Dimension);
                if (interGeo.IsEmpty) 
                    continue;
                (interGeo as ITopologicalOperator).Simplify();
                var ngeo = (rep.ShapeCopy as ITopologicalOperator).Union(interGeo);
                rep.Shape = ngeo;

                rep.UpdateFeature();
                rep.Feature.Store();
            }
        }

        /// <summary>
        ///  注记消隐(局部消隐线)
        /// </summary>
        /// <param name="objFeature">需要消隐的目标线要素</param>
        /// <param name="annoShape">注记面范围</param>
        public static void AnnotationBlanking(IFeature objFeature, IPolygon annoShape)
        {
            if (annoShape == null)
                return;

            var fc = objFeature.Class;
            IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(GApplication.Application.Workspace.EsriWorkspace);
            IRepresentationClass rpc = repWS.OpenRepresentationClass(fc.AliasName);
            IMapContext mctx = new MapContextClass();
            mctx.Init((fc as IGeoDataset).SpatialReference, GApplication.Application.ActiveView.FocusMap.ReferenceScale, (fc as IGeoDataset).Extent);

            var rep = rpc.GetRepresentation(objFeature, mctx);
            IGeometry geo = rep.ShapeCopy;

            IGeometry intersPoints = (annoShape as ITopologicalOperator).Intersect(geo, esriGeometryDimension.esriGeometry0Dimension);
            IPointCollection intersCol = intersPoints as IPointCollection;

            bool isSplit;
            int splitIndex, segIndex;
            for (int i = 0; i < intersCol.PointCount; i++)
            {
                IPoint intersPt = intersCol.get_Point(i) as IPoint;
                (geo as IPolycurve).SplitAtPoint(intersPt, false, false, out isSplit, out splitIndex, out segIndex);
            }

            IGeometry intersLine = (annoShape as ITopologicalOperator).Intersect(geo, esriGeometryDimension.esriGeometry1Dimension);
            IPolyline trackPolyline = new PolylineClass();
            (trackPolyline as IGeometryCollection).AddGeometryCollection(intersLine as IGeometryCollection);
            IRelationalOperator trackRel = trackPolyline as IRelationalOperator;
            ISegmentCollection geoCol = geo as ISegmentCollection;
            var fe = rep.Feature;
            (geo as IPointIDAware).PointIDAware = true;
            for (int j = 0; j < geoCol.SegmentCount; j++)
            {
                ISegment geoItem = geoCol.get_Segment(j);
                if (!trackRel.Disjoint(geoItem.FromPoint) && !trackRel.Disjoint(geoItem.ToPoint))
                {
                    if (geo is IPointCollection)
                    {
                        try
                        {
                            ChangeIDInEnvelope(geo as IPointCollection, geoItem.FromPoint, -999);
                            rep.Shape = geo;
                            rep.UpdateFeature();
                            fe.Store();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(string.Format("对要素类{0}中要素{1}进行消隐时出错：{2}", fe.Class.AliasName, fe.OID, ex.Message));
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 等高线注记字头方向调整
        /// </summary>
        /// <param name="terlFCName"></param>
        /// <param name="elevFN"></param>
        public static void AdjustTerlAnnotaionDirect(string terlFCName = "TERL", string elevFN = "ELEV")
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == terlFCName.ToUpper();
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null)
            {
                return;
            }
            if (lyr.FeatureClass == null)
                return;
            IFeatureClass terlFC = lyr.FeatureClass;

            int elevIndex = terlFC.FindField(elevFN);
            if (elevIndex == -1)
                return;

            IQueryFilter qf = new QueryFilterClass() { WhereClause = string.Format("{0} is not null", elevFN) };
            if (terlFC.FeatureCount(qf) < 2)
                return;

            string tinFileName = AnnotationHelper.GetAppDataPath() + "\\tin";
            try
            {
                #region 构建TIN
                ITinEdit tinEdit = new TinClass();
                tinEdit.InitNew((terlFC as IGeoDataset).Extent);
                tinEdit.AddFromFeatureClass(terlFC, qf, terlFC.Fields.get_Field(elevIndex), null, esriTinSurfaceType.esriTinHardLine, false);
                if (Directory.Exists(tinFileName))
                {
                    DirectoryInfo dir = new DirectoryInfo(tinFileName);
                    dir.Delete(true);
                }
                tinEdit.SaveAs(tinFileName, true);
                var tinSurface = tinEdit as ITinSurface;
                #endregion

                var annoLayers = GApplication.Application.Workspace.LayerManager.GetLayer(l => l is IFDOGraphicsLayer).ToArray();
                for (int i = 0; i < annoLayers.Length; i++)
                {
                    IFeatureLayer annoFeatureLayer = annoLayers[i] as IFeatureLayer;

                    qf.WhereClause = string.Format("AnnotationClassID = {0} ", terlFC.ObjectClassID);
                    IFeatureCursor feCursor = annoFeatureLayer.Search(qf, false);
                    IFeature annoFe = null;
                    while ((annoFe = feCursor.NextFeature()) != null)
                    {
                        if (annoFe.Shape == null || annoFe.Shape.IsEmpty)
                            continue;

                        ITextElement textElement = (annoFe as IAnnotationFeature).Annotation as ITextElement;
                        double elev = 0;
                        if (!double.TryParse(textElement.Text, out elev))
                            continue;

                        double fontSize = textElement.Symbol.Size / 2.8345;
                        double angle = textElement.Symbol.Angle * Math.PI / 180;//度转弧度

                        IPoint centerPt = null;
                        #region 求取注记中心点
                        centerPt = (annoFe.Shape as IArea).Centroid;
                        #endregion

                        //注记方向线
                        double deltaLen = fontSize * 0.5 * 0.001 * GApplication.Application.MapControl.Map.ReferenceScale + 0.1;
                        ILine directLine = new Line();
                        directLine.FromPoint = new PointClass() { X = centerPt.X - deltaLen, Y = centerPt.Y };//底部点
                        directLine.ToPoint = new PointClass() { X = centerPt.X + deltaLen, Y = centerPt.Y };//顶部点
                        (directLine as ITransform2D).Rotate(centerPt, Math.PI * 0.5 + angle);//垂线方向

                        double baseElev = -999, topELev = -999;
                        #region 求底部点、顶部点高程
                        baseElev = tinSurface.GetElevation(directLine.FromPoint);
                        topELev = tinSurface.GetElevation(directLine.ToPoint);
                        #endregion

                        if (baseElev > topELev)
                        {
                            (textElement as IElement).Geometry = centerPt;

                            var symbol = textElement.Symbol;
                            symbol.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                            symbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                            symbol.Angle += 180;

                            textElement.Symbol = symbol;
                            (annoFe as IAnnotationFeature).Annotation = textElement as IElement;
                            annoFe.Store();
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(string.Format("{0}"), ex.Message);
            }
            finally
            {
                if (Directory.Exists(tinFileName))
                {
                    DirectoryInfo dir = new DirectoryInfo(tinFileName);
                    dir.Delete(true);
                }
            }

        }

        public static void ChangeIDInEnvelope(IPointCollection pc, IPoint pos, int ID)
        {
            IRelationalOperator rel = pos as IRelationalOperator;
            for (int i = 0; i < pc.PointCount; i++)
            {
                IPoint pt = pc.get_Point(i);
                if (!rel.Disjoint(pt))
                {
                    (pt as IPointIDAware).PointIDAware = true;
                    pt.ID = ID;
                    pc.UpdatePoint(i, pt);
                }
            }
        }

        public static IRepresentationWorkspaceExtension GetRepersentationWorkspace(IWorkspace workspace)
        {
            IWorkspaceExtensionManager wem = workspace as IWorkspaceExtensionManager;
            UID uid = new UIDClass();
            uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            IRepresentationWorkspaceExtension rwe = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
            return rwe;
        }

    }
}

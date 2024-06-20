using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Data;
using System.IO;
using ESRI.ArcGIS.Carto;
using SMGI.Plugin.EmergencyMap;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using DevExpress.XtraCharts.Native;
using stdole;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 利用普通标注引擎创建专题数据的注记
    /// </summary>
    public class ThematicAnnotationCreateCmd : SMGI.Common.SMGICommand
    {
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
            DialogResult dialogResult = MessageBox.Show("该过程可能会删除目标注记图层的所有注记，确定要全部生成注记吗？", "提示", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes)
                return;

            //获取专题数据规则
            DataTable annoDT = GetThematicAnnoRule();
            if (annoDT.Rows.Count == 0)
            {
                MessageBox.Show("没有找到专题数据注记规则表！");
                return;
            }
            var frm = new FrmAnnoLyr(annoDT);
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            annoDT = frm.targetDt;

            var annoEngine = m_Application.MapControl.Map.AnnotationEngine;
            if (annoEngine.Name.ToUpper().Contains("MAPLEX"))
            {
                IAnnotateMap am = new AnnotateMap();
                m_Application.MapControl.Map.AnnotationEngine = am;
            }

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    Dictionary<string, IFeatureClass> annoName2FeatureClass = new Dictionary<string, IFeatureClass>();
                    #region 获取注记要素类
                    System.Data.DataTable dtLayers = annoDT.AsDataView().ToTable(true, new string[] { "注记图层" });//distinct
                    for (int i = 0; i < dtLayers.Rows.Count; ++i)
                    {
                        //图层名
                        string annoLayerName = dtLayers.Rows[i]["注记图层"].ToString().Trim();
                        if (!(m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, annoLayerName))
                        {
                            MessageBox.Show(string.Format("当前数据库缺少注记要素[{0}]!", annoLayerName), "警告", MessageBoxButtons.OK);
                            return;
                        }

                        IFeatureClass annoFC = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(annoLayerName);

                        annoName2FeatureClass.Add(annoLayerName, annoFC);
                    }

                    if (annoName2FeatureClass.Count == 0)
                    {
                        MessageBox.Show("规则库中没有指定注记目标图层!", "警告", MessageBoxButtons.OK);
                        return;
                    }
                    #endregion

                    if (!frm.Reserve)
                    {
                        #region 清空原注记图层要素
                        foreach (var kv in annoName2FeatureClass)
                        {
                            IFeatureCursor featureCurosr = kv.Value.Search(null, false);
                            IFeature feature = null;
                            while ((feature = featureCurosr.NextFeature()) != null)
                                feature.Delete();
                            if (feature != null)
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCurosr);
                        }
                        #endregion
                    }

                    List<string> fcNameList = new List<string>();
                    dtLayers = annoDT.AsDataView().ToTable(true, new string[] { "图层" });//distinct
                    for (int i = 0; i < dtLayers.Rows.Count; ++i)
                    {
                        string fcName = dtLayers.Rows[i]["图层"].ToString().Trim();
                        if(!fcNameList.Contains(fcName))
                            fcNameList.Add(fcName);
                    }

                    foreach (var fcName in fcNameList)
                    {
                        IFeatureLayer lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                        {
                            return x is IFeatureLayer && (x as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName.ToUpper();

                        })).FirstOrDefault() as IFeatureLayer;
                        if (lyr == null || lyr.FeatureClass == null)
                            continue;

                        var drs = annoDT.Select().Where(i => i["图层"].ToString().ToUpper() == fcName).ToList();

                        //生成注记
                        GenAnno(lyr, drs.ToList(), annoName2FeatureClass, wo);
                    }

                }

                m_Application.EngineEditor.StopOperation("专题注记创建");

                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                m_Application.EngineEditor.AbortOperation();
            }
            finally
            {
                m_Application.MapControl.Map.AnnotationEngine = annoEngine;
            }

            
        }

        private DataTable GetThematicAnnoRule()
        {
            DataTable dt = new DataTable();

            Dictionary<string, string> envString = m_Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            bool themFlag = false;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            if (envString.ContainsKey("ThemExist"))
                themFlag = bool.Parse(envString["ThemExist"]);
            if (!themFlag)
                return dt;

            string dirName = envString["ThemDataBase"];
            string dirpath = m_Application.Template.Root + "\\专题\\";
            {
                string mdbpath = dirpath + dirName + "\\普通注记规则.mdb";
                if (File.Exists(mdbpath))
                {
                    dt = CommonMethods.ReadToDataTable(mdbpath, "注记规则");
                }
            }

            return dt;
        }

        private List<int> GetFeaturesInFeatureClass(string featureClassName, string condition)
        {
            List<int> featureIDList = new List<int>();

            try
            {
                IFeatureClass featureClass = null;
                if ((m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, featureClassName))
                {
                    featureClass = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
                }
                if (featureClass == null)
                {
                    return featureIDList;
                }

                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = condition.Replace("[", "").Replace("]", "");//替换掉中括号，兼容mdb和gdb
                if (queryFilter.WhereClause == "")
                {
                    queryFilter = null;
                }
                if (featureClass.FeatureCount(queryFilter) > 0)
                {
                    IFeatureCursor featureCursor = featureClass.Search(queryFilter, false);
                    IFeature feature = null;
                    while ((feature = featureCursor.NextFeature()) != null)
                    {
                        featureIDList.Add(feature.OID);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return featureIDList;
        }

        private void GenAnno(IFeatureLayer lyr, List<DataRow> drArray, Dictionary<string, IFeatureClass> annoName2FeatureClass, WaitOperation wo = null)
        {
            IGeoFeatureLayer geoLyr = lyr as IGeoFeatureLayer;
            IAnnotateLayerPropertiesCollection annoPropColl = (geoLyr.AnnotationProperties as IClone).Clone() as IAnnotateLayerPropertiesCollection;
            geoLyr.AnnotationProperties.Clear();
            bool bDispAnno = geoLyr.DisplayAnnotation;
            geoLyr.DisplayAnnotation = true;

            Dictionary<int, string> classIndex2AnnoFCName = new Dictionary<int, string>();
            for (int i = 0; i < drArray.Count(); i++)
            {
                #region 设置分类标注属性
                DataRow dr = drArray[i];
                for (int j = 0; j < dr.Table.Columns.Count; j++)
                {
                    object val = dr[j];
                    if (val == null || Convert.IsDBNull(val))
                        dr[j] = "";
                }

                string fcName = dr["图层"].ToString().Trim();
                string classRuleName = dr["规则分类名"].ToString();
                string condition = dr["查询条件"].ToString();
                string annoFieldName = dr["注记字段"].ToString();
                string expression = dr["注记字段表达式"].ToString();
                string fontName = dr["注记字体"].ToString();
                double fontSize = double.Parse(dr["注记大小"].ToString());
                string fontColorStr = dr["注记颜色"].ToString();
                string annoFCName = dr["注记图层"].ToString();

                List<int> featureIDs = GetFeaturesInFeatureClass(fcName, condition);
                if (featureIDs.Count == 0)
                    continue;

                ILabelEngineLayerProperties leLyrProp = new LabelEngineLayerPropertiesClass();
                #region 放置属性
                (leLyrProp as IAnnotateLayerProperties).CreateUnplacedElements = true;
                (leLyrProp as IAnnotateLayerProperties).WhereClause = condition;
                (leLyrProp as IAnnotateLayerProperties).Class = classRuleName;
                (leLyrProp as IAnnotateLayerProperties).LabelWhichFeatures = esriLabelWhichFeatures.esriAllFeatures;
                (leLyrProp as ILabelEngineLayerProperties2).AnnotationClassID = geoLyr.AnnotationProperties.Count;

                classIndex2AnnoFCName.Add(geoLyr.AnnotationProperties.Count, annoFCName);


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

                #region symbol
                ITextSymbol textSymbol = new TextSymbolClass();

                IFontDisp fontDisp = new StdFontClass() as stdole.IFontDisp;
                fontDisp.Name = fontName;
                IColor fontColor = GetColor(fontColorStr);

                textSymbol.Font = fontDisp;
                if (fontSize > 0)
                    textSymbol.Size = fontSize * 2.8345;//毫米转磅
                if (fontColor != null)
                    textSymbol.Color = fontColor;
                (textSymbol as IFormattedTextSymbol).Leading = -4.0;

                leLyrProp.Symbol = textSymbol;
                #endregion

                #region BasicOverposterLayerProperties
                IBasicOverposterLayerProperties lyrProp = new BasicOverposterLayerPropertiesClass();
                lyrProp.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
                lyrProp.FeatureWeight = esriBasicOverposterWeight.esriHighWeight;
                lyrProp.GenerateUnplacedLabels = true;
                (lyrProp as IOverposterLayerProperties).PlaceLabels = true;


                if (lyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                {
                }
                else if (lyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    ILineLabelPosition linePosition = new LineLabelPositionClass();
                    linePosition.Above = true;
                    linePosition.InLine = false;
                    linePosition.Below = false;

                    lyrProp.LineLabelPosition = linePosition;
                }
                else if (lyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                {
                }
                
                leLyrProp.BasicOverposterLayerProperties = lyrProp;
                #endregion

                #endregion

                geoLyr.AnnotationProperties.Add(leLyrProp as IAnnotateLayerProperties);

                #endregion
            }

            string fullPath = AnnoHelper.GetAppDataPath() + "\\MyWorkspace886.gdb";
            IWorkspace ws = AnnoHelper.createTempWorkspace(fullPath);

            //删除原注记要素类
            AnnoHelper.deleteDataSet(ws, lyr.Name + "_label2anno");

            //标注转注记，同时获取标注文本
            ITrackCancel tc = new CancelTrackerClass();
            IConvertLabelsToAnnotation convertLTA = new ConvertLabelsToAnnotationClass();

            try
            {
                convertLTA.Initialize(GApplication.Application.MapControl.Map, esriAnnotationStorageType.esriDatabaseAnnotation, esriLabelWhichFeatures.esriAllFeatures, true, new CancelTrackerClass(), null);

                //转换
                convertLTA.AddFeatureLayer(lyr, lyr.Name + "_label2anno", ws as IFeatureWorkspace, null, false, false, false, false, false, "");

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
                //geoLyr.AnnotationProperties = annoPropColl;
                geoLyr.DisplayAnnotation = bDispAnno;
            }

            //遍历:复制注记到目标注记图层
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
                        IAnnotationFeature2 annoFeature = fe as IAnnotationFeature2;
                        if (annoFeature.Annotation == null || (annoFeature.Annotation as ITextElement).Text == "")
                            continue;

                        int annoClassID = annoFeature.AnnotationClassID;//分类ID
                        string annoFCName = classIndex2AnnoFCName[annoClassID];

                        //复制文本
                        ITextElement textElement = CopyTextElement(annoFeature.Annotation as ITextElement);

                        //添加注记要素
                        IFeature newFe = annoName2FeatureClass[annoFCName].CreateFeature();
                        IAnnotationFeature2 newAnnoFe = newFe as IAnnotationFeature2;
                        newAnnoFe.Annotation = textElement as IElement; ;
                        newAnnoFe.AnnotationClassID = lyr.FeatureClass.ObjectClassID;//这里用该属性记录注记所对应的实体要素类
                        newAnnoFe.LinkedFeatureID = annoFeature.LinkedFeatureID;
                        newAnnoFe.Status = annoFeature.Status;

                        newFe.Store();


                        Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(cursor);
                }
            }

            Marshal.ReleaseComObject(annoEnumLayer);
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
            catch (Exception ex)
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
    }
}

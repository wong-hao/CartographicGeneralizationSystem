using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Maplex;
using ESRI.ArcGIS.Display;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.AnnotationEngine
{
    /// <summary>
    /// LiZh(2018.10)
    /// 基于maplex和注记转换规则表生成注记
    /// </summary>
    public class AnnotationCreator
    {
        private GApplication _app;

        public AnnotationCreator(GApplication app)
        {
            _app = app;
        }

        public string createAnnotateAuto(DataTable annoRuleTable, DataTable fontMappingTable, Dictionary<string, IFeatureClass> annoName2FeatureClass, IPolyline extentGeo, WaitOperation wo = null)
        {
            string err = "";
            try
            {

                //修改注记要素类的扩展属性
                double refscale = _app.MapControl.Map.ReferenceScale;
                if (refscale != 0)
                {
                    CommonMethods.UpdateAnnoRefScale(_app.MapControl.Map, refscale);
                }

                #region 清空原注记图层要素
                foreach (var kv in annoName2FeatureClass)
                {
                    IFeatureCursor feCurosr = kv.Value.Search(null, false);
                    IFeature fe = null;
                    while ((fe = feCurosr.NextFeature()) != null)
                    {
                        fe.Delete();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feCurosr);
                }
                #endregion

                //启用maplex标注引擎
                string engineName = _app.MapControl.Map.AnnotationEngine.Name.ToUpper();
                if (!engineName.Contains("MAPLEX"))
                {
                    IAnnotateMap am = new MaplexAnnotateMap();
                    _app.MapControl.Map.AnnotationEngine = am;
                }

                if (wo != null)
                    wo.SetText("正在读取并解析注记规则表……");

                List<string> fcNameList = new List<string>();//注记规则表中的要素类名集合
                #region 获取生成注记相关的要素类信息
                DataTable dtLayerNames = annoRuleTable.AsDataView().ToTable(true, new string[] { "要素类名" });//distinct
                for (int i = 0; i < dtLayerNames.Rows.Count; ++i)
                {
                    //图层名
                    string fcName = dtLayerNames.Rows[i]["要素类名"].ToString().Trim().ToUpper();
                    if (fcNameList.Contains(fcName))//转换为大写后可能出现重复
                        continue;

                    fcNameList.Add(fcName);
                }
                #endregion

                //不显示图层集合
                List<IGeoFeatureLayer> invisibleLyrList = new List<IGeoFeatureLayer>();

                //规则列表
                List<AnnoClassRule> acrList = new List<AnnoClassRule>();

                #region 遍历规则表
                foreach (var fcName in fcNameList)
                {
                    IGeoFeatureLayer lyr = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer)
                            && ((l as IFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == fcName
                            && _app.Workspace.EsriWorkspace.PathName == ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName)).FirstOrDefault() as IGeoFeatureLayer;
                    if (lyr == null)
                        continue;

                    IFeatureClass fc = lyr.FeatureClass;

                    bool bVisible = lyr.Visible;
                    if (!bVisible)
                    {
                        invisibleLyrList.Add(lyr);//收集不显示图层
                        lyr.Visible = true;//图层显示
                    }

                    IAnnotateLayerPropertiesCollection annoProps = lyr.AnnotationProperties;
                    annoProps.Clear();


                    DataRow[] drArray = annoRuleTable.Select().Where(i => i["要素类名"].ToString().Trim().ToUpper() == fcName).ToArray();
                    for (int i = 0; i < drArray.Count(); i++)
                    {
                        DataRow dr = drArray[i];
                        for (int j = 0; j < dr.Table.Columns.Count; j++)
                        {
                            object val = dr[j];
                            if (val == null || Convert.IsDBNull(val))
                                dr[j] = "";
                        }

                        AnnotationRule annoRule = new AnnotationRule(dr["要素类名"].ToString(), dr["查询条件"].ToString(), dr["规则分类名"].ToString(), 
                            dr["标注字段"].ToString(), dr["注记表达式"].ToString(), dr["注记要素类名"].ToString(), dr["注记字体"].ToString(), dr["注记大小"].ToString(), 
                            dr["注记颜色"].ToString(), dr["注记样式"].ToString(), dr["晕圈大小"].ToString(), dr["晕圈颜色"].ToString(), dr["气泡形状"].ToString(), 
                            dr["气泡颜色"].ToString(), dr["气泡边线颜色"].ToString(), dr["气泡边线宽度"].ToString(), dr["气泡边框距"].ToString(), dr["字符宽度"].ToString(), 
                            dr["CJK字符方向"].ToString(), dr["放置类型"].ToString(), dr["注记位置"].ToString(), dr["注记偏移"].ToString(), dr["最大文字间距"].ToString(), 
                            dr["最大字符间距"].ToString(), dr["移除同名标注"].ToString(), dr["重复标注"].ToString(), dr["标注缓冲大小"].ToString(), dr["标注最小要素大小"].ToString(),
                            dr["连接要素"].ToString(), dr["标注要素最大部分"].ToString(), dr["要素权重"].ToString(), dr["从不移除"].ToString(), dr["消隐类型"].ToString());

                        //例外
                        if (fcName == "LRDL" && (annoRule.AnnoClass != "主干道" && annoRule.AnnoClass != "次干道" && annoRule.AnnoClass != "支线"))
                            continue;//非城市道路直接跳过

                        #region 字体映射
                        for (int j = 0; j < fontMappingTable.Rows.Count; j++)
                        {
                            DataRow row = fontMappingTable.Rows[j];
                            if (row["字体名"].ToString() == annoRule.FontName)
                            {
                                annoRule.FontName = row["目标字体名"].ToString();
                                break;
                            }
                        }
                        #endregion

                        ILabelEngineLayerProperties2 labelEngine = CreateLabelEngineLayerPropertiesOfMaplex(annoRule, fc.ShapeType, _app.MapControl.Map.ReferenceScale);
                        if (labelEngine == null)
                            continue;

                        if (extentGeo != null)
                        {
                            (labelEngine as IAnnotateLayerProperties).Extent = extentGeo.Envelope;
                        }
                        (labelEngine as ILabelEngineLayerProperties2).AnnotationClassID = annoProps.Count;
                        annoProps.Add(labelEngine as IAnnotateLayerProperties);

                        //添加规则Class,并保存规则的索引值
                        AnnoClassRule acr = new AnnoClassRule
                        {
                            AnnoRule = annoRule,
                            GroupSymbolID = fc.ObjectClassID,
                            ClassIndex = (labelEngine as ILabelEngineLayerProperties2).AnnotationClassID
                        };
                        acrList.Add(acr);

                    }

                    lyr.DisplayAnnotation = true;
                }
                #endregion


                if (wo != null)
                    wo.SetText("根据标注定位创建注记……");

                Dictionary<string, Dictionary<string, List<int>>> oneOverrideFCName2annoFID = new Dictionary<string, Dictionary<string, List<int>>>();//Dictionary<要素类名, Dictionary<注记要素类名, List<注记OID>>>
                Dictionary<string, Dictionary<string, List<int>>> multiOverrideFCName2annoFID = new Dictionary<string, Dictionary<string, List<int>>>();//Dictionary<要素类名, Dictionary<注记要素类名, List<注记OID>>>
                Dictionary<string, Dictionary<string, List<int>>> oneDashOverrideFCName2annoFID = new Dictionary<string, Dictionary<string, List<int>>>();//Dictionary<要素类名, Dictionary<注记要素类名, List<注记OID>>>

                #region 创建注记
                string fullPath = AnnotationHelper.GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = AnnotationHelper.createTempWorkspace(fullPath);

                foreach (var fcName in fcNameList)
                {
                    IGeoFeatureLayer lyr = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer)
                            && ((l as IFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == fcName
                            && _app.Workspace.EsriWorkspace.PathName == ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName)).FirstOrDefault() as IGeoFeatureLayer;
                    if (lyr == null)
                        continue;

                    IFeatureClass fc = lyr.FeatureClass;

                    //删除原注记要素类
                    AnnotationHelper.deleteDataSet(ws, lyr.Name + "_label2anno");

                    #region 标注转注记
                    IConvertLabelsToAnnotation convertLTA = new ConvertLabelsToAnnotationClass();

                    try
                    {
                        //确保启用了maplex标注引擎
                        if (!_app.MapControl.Map.AnnotationEngine.Name.ToUpper().Contains("MAPLEX"))
                        {
                            IAnnotateMap am = new MaplexAnnotateMap();
                            _app.MapControl.Map.AnnotationEngine = am;
                        }

                        convertLTA.Initialize(_app.MapControl.Map, esriAnnotationStorageType.esriDatabaseAnnotation, esriLabelWhichFeatures.esriAllFeatures, true, new CancelTrackerClass(), null);

                        //转换
                        convertLTA.AddFeatureLayer(lyr, lyr.Name + "_label2anno", ws as IFeatureWorkspace, null, false, false, false, false, false, "");
                        convertLTA.ConvertLabels();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        string convertErr = convertLTA.ErrorInfo;
                    }
                    lyr.DisplayAnnotation = false;
                    #endregion

                    #region 复制要素到目标图层
                    IEnumLayer annoEnumLayer = convertLTA.AnnoLayers;
                    annoEnumLayer.Reset();
                    ILayer layer = null;
                    while ((layer = annoEnumLayer.Next()) != null)
                    {
                        IAnnotationLayer annoLayer = layer as IAnnotationLayer;
                        if (annoLayer == null)
                            continue;

                        IFeature fe = null;
                        IFeatureCursor feCursor = (layer as IFeatureLayer).FeatureClass.Search(null, false);
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            IAnnotationFeature2 annoFe = fe as IAnnotationFeature2;
                            if (annoFe.Annotation == null || (annoFe.Annotation as ITextElement).Text == "")
                                continue;

                            //该注记对应的注记规则
                            AnnoClassRule classRule = acrList.Where(l => l.GroupSymbolID == fc.ObjectClassID
                                && l.ClassIndex == annoFe.AnnotationClassID).FirstOrDefault();
                            if(classRule == null)
                                continue;
                            AnnotationRule annoRule = classRule.AnnoRule;

                            //复制文本
                            ITextElement textElement = AnnotationHelper.CopyTextElement(annoFe.Annotation as ITextElement);

                            //添加注记要素
                            IFeature newFe = annoName2FeatureClass[annoRule.AnnoFCName].CreateFeature();
                            IAnnotationFeature2 newAnnoFe = newFe as IAnnotationFeature2;
                            newAnnoFe.Annotation = textElement as IElement; ;
                            newAnnoFe.AnnotationClassID = fc.ObjectClassID;//这里用该属性记录注记所对应的实体要素类
                            newAnnoFe.LinkedFeatureID = annoFe.LinkedFeatureID;
                            newAnnoFe.Status = annoFe.Status;

                            int blanktypeIndex = newFe.Fields.FindField("blankingtype");
                            if (blanktypeIndex != -1)
                            {
                                if (annoRule.BlankingType == "多要素几何覆盖" || annoRule.BlankingType == "单要素几何覆盖" || annoRule.BlankingType == "单要素局部消隐")
                                {
                                    newFe.set_Value(blanktypeIndex, annoRule.BlankingType);
                                }
                            }
                            int classNameIndex = newFe.Fields.FindField("classname");
                            if (classNameIndex != -1)
                            {
                                newFe.set_Value(classNameIndex, annoRule.AnnoClass);
                            }

                            newFe.Store();

                            #region 消隐注记收集
                            if (newAnnoFe.Status == esriAnnotationStatus.esriAnnoStatusPlaced)
                            {
                                if (annoRule.BlankingType == "多要素几何覆盖")
                                {
                                    if (multiOverrideFCName2annoFID.ContainsKey(annoRule.FCName))
                                    {
                                        if (multiOverrideFCName2annoFID[annoRule.FCName].ContainsKey(annoRule.AnnoFCName))
                                        {
                                            multiOverrideFCName2annoFID[annoRule.FCName][annoRule.AnnoFCName].Add(newFe.OID);
                                        }
                                        else
                                        {
                                            List<int> oidList = new List<int>();
                                            oidList.Add(newFe.OID);

                                            multiOverrideFCName2annoFID[annoRule.FCName].Add(annoRule.AnnoFCName, oidList);
                                        }
                                    }
                                    else
                                    {
                                        List<int> oidList = new List<int>();
                                        oidList.Add(newFe.OID);

                                        Dictionary<string, List<int>> kv = new Dictionary<string, List<int>>();
                                        kv.Add(annoRule.AnnoFCName, oidList);

                                        multiOverrideFCName2annoFID.Add(annoRule.FCName, kv);
                                    }
                                }
                                else if (annoRule.BlankingType == "单要素几何覆盖")
                                {
                                    if (oneOverrideFCName2annoFID.ContainsKey(annoRule.FCName))
                                    {
                                        if (oneOverrideFCName2annoFID[annoRule.FCName].ContainsKey(annoRule.AnnoFCName))
                                        {
                                            oneOverrideFCName2annoFID[annoRule.FCName][annoRule.AnnoFCName].Add(newFe.OID);
                                        }
                                        else
                                        {
                                            List<int> oidList = new List<int>();
                                            oidList.Add(newFe.OID);

                                            oneOverrideFCName2annoFID[annoRule.FCName].Add(annoRule.AnnoFCName, oidList);
                                        }
                                    }
                                    else
                                    {
                                        List<int> oidList = new List<int>();
                                        oidList.Add(newFe.OID);

                                        Dictionary<string, List<int>> kv = new Dictionary<string, List<int>>();
                                        kv.Add(annoRule.AnnoFCName, oidList);

                                        oneOverrideFCName2annoFID.Add(annoRule.FCName, kv);
                                    }
                                }
                                else if (annoRule.BlankingType == "单要素局部消隐")
                                {
                                    if (oneDashOverrideFCName2annoFID.ContainsKey(annoRule.FCName))
                                    {
                                        if (oneDashOverrideFCName2annoFID[annoRule.FCName].ContainsKey(annoRule.AnnoFCName))
                                        {
                                            oneDashOverrideFCName2annoFID[annoRule.FCName][annoRule.AnnoFCName].Add(newFe.OID);
                                        }
                                        else
                                        {
                                            List<int> oidList = new List<int>();
                                            oidList.Add(newFe.OID);

                                            oneDashOverrideFCName2annoFID[annoRule.FCName].Add(annoRule.AnnoFCName, oidList);
                                        }
                                    }
                                    else
                                    {
                                        List<int> oidList = new List<int>();
                                        oidList.Add(newFe.OID);

                                        Dictionary<string, List<int>> kv = new Dictionary<string, List<int>>();
                                        kv.Add(annoRule.AnnoFCName, oidList);

                                        oneDashOverrideFCName2annoFID.Add(annoRule.FCName, kv);
                                    }
                                }
                            }
                            #endregion

                            Marshal.ReleaseComObject(fe);
                        }
                        Marshal.ReleaseComObject(feCursor);
                    }
                    Marshal.ReleaseComObject(annoEnumLayer);

                    #endregion

                    //删除临时注记要素类
                    AnnotationHelper.deleteDataSet(ws, lyr.Name + "_label2anno");
                }
                #endregion

                //还原
                foreach (var lyr in invisibleLyrList)
                    lyr.Visible = false;


                if (wo != null)
                    wo.SetText("字头朝北处理……");
                foreach (var kv in annoName2FeatureClass)
                {
                    AnnoBreakFaceToNorth anno2North = new AnnoBreakFaceToNorth(_app, kv.Key);
                    anno2North.toNorth(-_app.ActiveView.ScreenDisplay.DisplayTransformation.Rotation);
                }

                if (wo != null)
                    wo.SetText("正在进行消隐等处理……");
                //字头方向
                AnnotationHelper.AdjustTerlAnnotaionDirect();
                //注记消隐
                if (multiOverrideFCName2annoFID.Count > 0)
                {
                    foreach (var kv in multiOverrideFCName2annoFID)
                    {
                        foreach(var kv2 in kv.Value)
                        {
                            AnnotationHelper.AnnotationBlankingByOverride(kv.Key, kv2.Key, kv2.Value, false);
                        }
                    }
                }
                if (oneOverrideFCName2annoFID.Count > 0)
                {
                    foreach (var kv in oneOverrideFCName2annoFID)
                    {
                        foreach (var kv2 in kv.Value)
                        {
                            AnnotationHelper.AnnotationBlankingByOverride(kv.Key, kv2.Key, kv2.Value, true);
                        }
                    }
                }
                if (oneDashOverrideFCName2annoFID.Count > 0)
                {
                    foreach (var kv in oneDashOverrideFCName2annoFID)
                    {
                        foreach (var kv2 in kv.Value)
                        {
                            AnnotationHelper.AnnotationBlankingByDash(kv.Key, kv2.Key, kv2.Value, true);
                        }
                    }
                }

                _app.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }

            return err;
        }



        private ILabelEngineLayerProperties2 CreateLabelEngineLayerPropertiesOfMaplex(AnnotationRule annoRule, esriGeometryType geometryType, double refernceScale)
        {
            //创建一个文本符号
            string subMessage = string.Empty;
            ITextSymbol textSymbol = AnnotationHelper.CreateTextSymbol(annoRule, out subMessage);
            if (textSymbol == null)
            {
                return null;
            }

            //创建标注引擎属性 作为一个Class
            ILabelEngineLayerProperties2 labelEngineLayerProperties = new MaplexLabelEngineLayerPropertiesClass();

            #region 分类、字体、注记表达式
            (labelEngineLayerProperties as IAnnotateLayerProperties).LabelWhichFeatures = esriLabelWhichFeatures.esriAllFeatures;//标注要素类型
            (labelEngineLayerProperties as IAnnotateLayerProperties).CreateUnplacedElements = true;//创建未放置注记
            (labelEngineLayerProperties as IAnnotateLayerProperties).FeatureLinked = true;//要素关联
            (labelEngineLayerProperties as IAnnotateLayerProperties).Class = annoRule.AnnoClass;//规则分类名
            (labelEngineLayerProperties as IAnnotateLayerProperties).WhereClause = annoRule.WhereClause;//分类条件
            labelEngineLayerProperties.Symbol = textSymbol;
            labelEngineLayerProperties.Expression = "[" + annoRule.AnnoFieldName + "]";
            if (annoRule.Expression.ToUpper().Contains("FUNCTION"))
            {
                labelEngineLayerProperties.Expression = annoRule.Expression;//设置注记的表达式
                labelEngineLayerProperties.IsExpressionSimple = false;
                IAnnotationExpressionEngine pAee = new AnnotationVBScriptEngineClass();
                labelEngineLayerProperties.ExpressionParser = pAee;
            }
            #endregion

            IMaplexOverposterLayerProperties maplexOverposterLayerProperties = new MaplexOverposterLayerProperties();
            
            #region 标注位置
            //放置样式
            switch (geometryType)
            {
                case esriGeometryType.esriGeometryPolyline:
                    {
                        (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties2).LineFeatureType = annoRule.LineFeatureType;
                        if (annoRule.LineFeatureType == esriMaplexLineFeatureType.esriMaplexContourFeature)
                        {
                            (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties2).ContourAlignmentType = esriMaplexContourAlignmentType.esriMaplexUphillAlignment;
                            //(maplexOverposterLayerProperties as IMaplexOverposterLayerProperties2).ContourLadderType = esriMaplexContourLadderType.esriMaplexNoLadder;
                        }
                        break;
                    }
                case esriGeometryType.esriGeometryPolygon:
                    {
                        (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties2).PolygonFeatureType = annoRule.PolygonFeatureType;
                        break;
                    }
            }
             
            //放置位置
            switch (geometryType)
            {
                case esriGeometryType.esriGeometryPoint:
                    maplexOverposterLayerProperties.PointPlacementMethod = annoRule.PointPlacement;
                    if (maplexOverposterLayerProperties.PointPlacementMethod == esriMaplexPointPlacementMethod.esriMaplexAroundPoint)
                    {
                        //设置自定义的优先级
                        maplexOverposterLayerProperties.EnablePointPlacementPriorities = true;
                        IPointPlacementPriorities pointPlacementPriorities = new PointPlacementPrioritiesClass();
                        pointPlacementPriorities.CenterRight = 1;
                        pointPlacementPriorities.AboveRight = 2;
                        pointPlacementPriorities.AboveCenter = 3;
                        pointPlacementPriorities.AboveLeft = 4;
                        pointPlacementPriorities.BelowRight = 5;
                        pointPlacementPriorities.CenterLeft = 6;
                        pointPlacementPriorities.BelowCenter = 7;
                        pointPlacementPriorities.BelowLeft = 8;
                        maplexOverposterLayerProperties.PointPlacementPriorities = pointPlacementPriorities;
                    }
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    maplexOverposterLayerProperties.LinePlacementMethod = annoRule.LinePlacement;
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    maplexOverposterLayerProperties.PolygonPlacementMethod = annoRule.PolygonPlacement;
                    break;
            }

            //注记偏移
            maplexOverposterLayerProperties.PrimaryOffset = annoRule.PrimaryOffset;
            maplexOverposterLayerProperties.PrimaryOffsetUnit = esriMaplexUnit.esriMaplexUnitMM;

            //展开文字效果
            maplexOverposterLayerProperties.SpreadWords = annoRule.EnableSpreadWords;
            if (annoRule.EnableSpreadWords)
            {
                maplexOverposterLayerProperties.MaximumWordSpacing = annoRule.MaximumWordSpacing;
            }

            //展开字符效果
            maplexOverposterLayerProperties.SpreadCharacters = annoRule.EnableSpreadCharacters;
            if (annoRule.EnableSpreadCharacters)
            {
                maplexOverposterLayerProperties.MaximumCharacterSpacing = annoRule.MaximumCharacterSpacing;
            }

            #endregion

            #region 标注密度
            //移除同名注记
            maplexOverposterLayerProperties.ThinDuplicateLabels = annoRule.EnableThinDuplicateLabels;
            if (annoRule.EnableThinDuplicateLabels)
            {
                maplexOverposterLayerProperties.ThinningDistance = annoRule.ThinningDistance;
                (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties2).ThinningDistanceUnit = esriMaplexUnit.esriMaplexUnitMM;
            }

            //重复标注
            maplexOverposterLayerProperties.RepeatLabel = annoRule.EnableRepeatLabel;
            if (annoRule.EnableRepeatLabel)
            {
                maplexOverposterLayerProperties.MinimumRepetitionInterval = annoRule.MinimumRepetitionInterval;
                (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties2).RepetitionIntervalUnit = esriMaplexUnit.esriMaplexUnitMM;
            }

            //标注缓冲大小
            maplexOverposterLayerProperties.LabelBuffer = annoRule.LabelBuffer;//设置注记间的避让间距(字体高度百分比)

            //标注的最小要素大小
            maplexOverposterLayerProperties.MinimumSizeForLabeling = annoRule.MinimumSizeForLabeling;
            (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties2).MinimumFeatureSizeUnit = esriMaplexUnit.esriMaplexUnitMM;

            //连接要素
            (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties4).EnableConnection = annoRule.EnableConnection;
            if (annoRule.EnableConnection)
            {
                (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties4).ConnectionType = esriMaplexConnectionType.esriMaplexMinimizeLabels;
            }

            //标注要素最大部分
            (maplexOverposterLayerProperties as IMaplexOverposterLayerProperties4).LabelLargestPolygon = annoRule.EnableLabelLargestPolygon;
            #endregion  
            
            #region 冲突解决
            //要素权重
            maplexOverposterLayerProperties.FeatureWeight = annoRule.FeatureWeight;
            if (geometryType == esriGeometryType.esriGeometryPolygon)
            {
                maplexOverposterLayerProperties.PolygonBoundaryWeight = annoRule.FeatureWeight;
            }

            //从不移除
            maplexOverposterLayerProperties.NeverRemoveLabel = annoRule.NeverRemoveLabel;
            #endregion

            (maplexOverposterLayerProperties as IOverposterLayerProperties).PlaceLabels = true;
            if (geometryType == esriGeometryType.esriGeometryPoint)
            {
                (maplexOverposterLayerProperties as IOverposterLayerProperties).IsBarrier = true;
                (maplexOverposterLayerProperties as IOverposterLayerProperties).PlaceSymbols = true;
            }
            else
            {
                (maplexOverposterLayerProperties as IOverposterLayerProperties).IsBarrier = false;
                (maplexOverposterLayerProperties as IOverposterLayerProperties).PlaceSymbols = false;
            }

            //设置标注的放置属性
            labelEngineLayerProperties.OverposterLayerProperties = maplexOverposterLayerProperties as IOverposterLayerProperties;
            //设置标注的参考比例尺
            (labelEngineLayerProperties as IAnnotateLayerTransformationProperties).ReferenceScale = refernceScale;//设置标注的参考比例，在修改注记的对其方式后，注记会缩小？？？

            return labelEngineLayerProperties;
        }

    }

    /// <summary>
    /// 注记规则类信息结构
    /// </summary>
    public class AnnoClassRule
    {
        /// <summary>
        /// 该规则分类所属的要素类编码
        /// </summary>
        public int GroupSymbolID
        {
            set;
            get;
        }

        /// <summary>
        /// 该规则分类的编码
        /// </summary>
        public int ClassIndex
        {
            set;
            get;
        }

        /// <summary>
        /// 该规则分类的注记规则
        /// </summary>
        public AnnotationRule AnnoRule
        {
            set;
            get;
        }
    }
}

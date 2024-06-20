using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Data;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Xml.Linq;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Maplex;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 基于maplex和注记转换规则表生成注记
    /// </summary>
    public class MaplexAnnotateCreate
    {
        private GApplication _app;

        public MaplexAnnotateCreate(GApplication app)
        {
            _app = app;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="annoRuleTable"></param>
        /// <param name="barrierRuleTable"></param>
        /// <param name="fontMappingTable">字体映射表</param>
        /// <param name="annoName2FeatureClass">注记图层名对应的要素类</param>
        /// <param name="extentGeo">注记生成范围线几何体</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public string createAnnotate(DataTable annoRuleTable, DataTable barrierRuleTable, DataTable fontMappingTable, Dictionary<string, IFeatureClass> annoName2FeatureClass, IPolyline extentGeo, WaitOperation wo = null,bool reserve=false)
        {
            string WarnErr = "";

            try
            {
                //清空注记临时库
                AnnoHelper.clearTempWorkspace();
                //修改注记要素类的扩展属性
                double refscale = _app.MapControl.Map.ReferenceScale;
                if (refscale != 0)
                {
                    CommonMethods.UpdateAnnoRefScale(_app.MapControl.Map, refscale);
                }

                //启用maplex标注引擎
                string engineName = _app.MapControl.Map.AnnotationEngine.Name.ToUpper();
                if (!engineName.Contains("MAPLEX"))
                {
                    IAnnotateMap am = new MaplexAnnotateMap();
                    _app.MapControl.Map.AnnotationEngine = am;
                }

                #region 清空原注记图层要素
                if (!reserve)
                { 
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
                }
                #endregion

                List<MaplexAttributeOfFeature> MaoList = new List<MaplexAttributeOfFeature>();
                List<labelEngineAndAnnoRule> leasList = new List<labelEngineAndAnnoRule>();

                if (wo != null)
                    wo.SetText("正在读取并解析规则表……");
                Dictionary<int, List<int>> localAnnoID2FID = new Dictionary<int, List<int>>();//要素类ClassID——>要素FID,一对多
                #region 读取规则表
                for (int i = 0; i < annoRuleTable.Rows.Count; i++)
                {
                    try
                    {
                        DataRow dr = annoRuleTable.Rows[i];
                        for (int j = 0; j < dr.Table.Columns.Count; j++)
                        {
                            object val = dr[j];
                            if (val == null || Convert.IsDBNull(val))
                                dr[j] = "";
                        }

                        string featureClassName = dr["图层"].ToString().Trim();
                        //if (featureClassName != "LFCL")
                        //{
                        //    continue;
                        //}
                        string condition = dr["查询条件"].ToString();
                        List<int> featureIDs = GetFeaturesInFeatureClass(featureClassName, condition);
                        if (featureIDs.Count == 0)
                            continue;

                        if (wo != null)
                            wo.SetText(string.Format("正在读取并解析规则表:\r\n规则表ID={0}、图层={1}、查询条件={2}", dr["ID"].ToString(), featureClassName, condition));

                        IFeatureClass featureClass = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
                        AnnoRule annoRule = new AnnoRule(dr["注记字段"].ToString(), dr["注记字体"].ToString(), dr["注记大小"].ToString(), dr["注记颜色"].ToString(),
                            dr["字体样式"].ToString(), condition, dr["注记字段表达式"].ToString(), dr["晕圈大小"].ToString(), dr["晕圈颜色"].ToString(),
                            dr["注记气泡颜色"].ToString(), dr["气泡形状"].ToString(), dr["气泡边线颜色"].ToString(), dr["气泡边线宽度"].ToString(),
                            dr["注记位置"].ToString(), dr["注记偏移"].ToString(), dr["注记重复标注"].ToString(), dr["最大字间距"].ToString(), dr["字间距"].ToString(), dr["字宽"].ToString(), dr["移除重复标注"].ToString(),
                            dr["连接要素标注"].ToString(), dr["标注最小要素大小"].ToString(), dr["标注要素最大部分"].ToString(), dr["面内权重"].ToString(), dr["边缘权重"].ToString(),
                            dr["注记权重"].ToString(), dr["是否移除"].ToString(), dr["锚点偏移X"].ToString(), dr["锚点偏移Y"].ToString(), dr["注记图层"].ToString());

                        #region 字体映射
                        for (int j = 0; j < fontMappingTable.Rows.Count; j++)
                        {
                            DataRow row = fontMappingTable.Rows[j];
                            if (row["国标字体"].ToString() == annoRule.FontName)
                            {
                                annoRule.FontName = row["替换字体"].ToString();
                                break;
                            }
                        }
                        #endregion

                        ILabelEngineLayerProperties2 labelEngine = AnnoHelper.CreateLabelEngineLayerPropertiesOfMaplex(annoRule, featureClass.ShapeType, _app.MapControl.Map.ReferenceScale);
                        if (labelEngine == null)
                            continue;
                        if (featureClassName.ToUpper() == "LRDL" )
                        {
                            var textSymbol = labelEngine.Symbol;
                            (textSymbol as ICharacterOrientation).CJKCharactersRotation = true;
                            labelEngine.Symbol = textSymbol;
                        }
                        labelEngineAndAnnoRule leas = new labelEngineAndAnnoRule { AnnotationRule = annoRule, LabelEngine = labelEngine, GroupSymbolID = i, ClassIndex = -1 };
                        leasList.Add(leas);

                        #region 注记文本
                        Dictionary<int, string> oid2Text = new Dictionary<int, string>();
                        var lyrs = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == featureClassName.ToUpper()));
                        foreach (var lyr in lyrs)//多个图层对应同一个要素类（如吉林的居民地点与街道办事处都对应的要素类AGNP）
                        {
                            bool bVisible = lyr.Visible;
                            if (!bVisible)
                                lyr.Visible = true;//图层显示

                            var lbTexts = AnnoHelper.getLabelText(_app.MapControl.Map, lyr as IFeatureLayer, annoRule.AnnoFieldName, annoRule.Expression, new AnnotationVBScriptEngineClass(), featureIDs.ToList());
                            var keys = lbTexts.Keys.ToArray();
                            //添加河流间距
                            if (featureClassName.ToUpper() == "HYDL" || featureClassName.ToUpper() == "AANL" || featureClassName.ToUpper() == "BRGL" || featureClassName.ToUpper() == "水系线")
                            {

                                int width = 0;
                                int.TryParse(dr["最大字间距"].ToString(), out width);
                                width = width / 100;
                                if (width != 0)
                                {
                                    string spaceword = "";
                                    for (int s = 0; s < width; s++)
                                        spaceword += "$";
                                    foreach (var key in keys)
                                    {
                                        string name = lbTexts[key];
                                        string newname = spaceword;
                                        var chars = name.ToCharArray();
                                        for (int c = 0; c < chars.Length; c++)
                                        {
                                            newname += chars[c].ToString() + spaceword;
                                        }
                                        oid2Text[key] = newname;
                                    }
                                }
                            }

                            foreach(var kv in lbTexts)
                            {
                                if (!oid2Text.ContainsKey(kv.Key))
                                {
                                    oid2Text.Add(kv.Key, kv.Value);
                                }
                                else//同一个要素出现在两个图层，已记录
                                {
                                }
                            }

                            if (!bVisible)
                                lyr.Visible = false;

                        }
                        #endregion


                        for (int k = 0; k < featureIDs.Count; k++)
                        {
                            IFeature feature = featureClass.GetFeature(featureIDs[k]);

                            string annoText = "";
                            if (oid2Text.ContainsKey(featureIDs[k]))
                            {
                                annoText = oid2Text[featureIDs[k]];
                            }
                            if (annoText == "")
                                continue; //如果注记内容为空的话，不进行注记要素的创建


                            MaplexAttributeOfFeature Mao = new MaplexAttributeOfFeature(annoText, feature, i, annoRule.FeatureWeight);
                            if (localAnnoID2FID.ContainsKey(Mao.AnnotationClassID))
                            {
                                localAnnoID2FID[Mao.AnnotationClassID].Add(Mao.FeatureID);
                            }
                            else
                            { 
                                localAnnoID2FID[Mao.AnnotationClassID] = new List<int>(){Mao.FeatureID};
                            }
                            MaoList.Add(Mao);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        MessageBox.Show("解析规则失败："+ex.Message);
                    }
                }
                int ppp = -1;
                #endregion
                #region 删除注记要素
                if (reserve)
                {
                    foreach (var kv in annoName2FeatureClass)
                    {
                        IFeatureCursor featureCurosr = kv.Value.Search(null, false);
                        IFeature feature = null;
                        while ((feature = featureCurosr.NextFeature()) != null)
                        {
                            IAnnotationFeature2 annoFea = feature as IAnnotationFeature2;
                            int annoClassID = annoFea.AnnotationClassID;
                            int linkedFeaID = annoFea.LinkedFeatureID;
                            if (localAnnoID2FID.ContainsKey(annoClassID) && localAnnoID2FID[annoClassID].Contains(linkedFeaID))
                            {
                                feature.Delete();
                            }   
                        }    
                        if (feature != null)
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCurosr);
                    }
                }
                #endregion
                if (wo != null)
                    wo.SetText("正在添加需生成注记的要素……");
                
                //要素类名称->要素类
                Dictionary<string, IFeatureClass> fclDics = new Dictionary<string, IFeatureClass>();
                //点要素类名称->ruleID->size
                Dictionary<string, Dictionary<int, double>> markerDic = new Dictionary<string, Dictionary<int, double>>();
              
                    #region 初始化MaplexOverposter
                    IMaplexOverposter maplexOverposter = new MaplexOverposter();
                    IDisplay display = _app.ActiveView.ScreenDisplay;
                    ISpatialReference spatialReference = _app.MapControl.SpatialReference;
                    IMaplexOverposterProperties maplexOverposterProp = new MaplexOverposterPropertiesClass
                    {
                        AllowBorderOverlap = false,//map border 是什么？？？
                        PlacementQuality = esriMaplexPlacementQuality.esriMaplexPlacementQualityHigh
                    };
                    maplexOverposter.Initialize(extentGeo.Envelope, display, spatialReference, maplexOverposterProp);
                    #endregion
                
       
              
                    #region 根据规则表,为MaplexOverposter添加要素
                    var mc = new MapContextClass();
                    try
                    {
                    mc.InitFromDisplay(_app.ActiveView.ScreenDisplay.DisplayTransformation);
                    //添加规则Class,并保存规则的索引值
                    for (int i = 0; i < leasList.Count; i++)
                    {
                        leasList[i].ClassIndex = maplexOverposter.AddClass(leasList[i].LabelEngine as ILabelEngineLayerProperties);
                    }
                    //添加要素
                    int cc = 0;
                    for (int i = 0; i < MaoList.Count; i++)
                    {
                        ppp = i;
                        MaplexAttributeOfFeature Mao = MaoList[i];
                        IFeatureClass featureClass = null;
                        string featureClassName = Mao.FeatureClassName;
                        if (fclDics.ContainsKey(featureClassName))
                        {
                            featureClass = fclDics[featureClassName];
                        }
                        else
                        {
                            featureClass = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
                        }
                        // IFeature feature = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(Mao.FeatureClassName).GetFeature(Mao.FeatureID);
                        IFeature feature = featureClass.GetFeature(Mao.FeatureID);

                        if (wo != null)
                            wo.SetText(string.Format("正在添加需生成注记的要素:\r\n图层={0}、要素ID={1}", featureClassName, feature.OID));

                        //查找其对应的ClassIndex
                        var leas = leasList.Where(l => l.GroupSymbolID == Mao.GroupSymbolID).ToArray()[0];
                        Mao.ClassIndex = leas.ClassIndex;
                        AnnoRule annoRule = leas.AnnotationRule;

                        //获取该要素及其符号
                        IGeometry geo = feature.ShapeCopy as IGeometry;
                        ISymbol featureSymbol = null;
                        if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                        {
                            var layers = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                            { return l is IGeoFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == feature.Class.AliasName; })).ToArray();
                            if (layers.Length == 0)
                                continue;
                            string fclName = feature.Class.AliasName;
                            //获取size大小
                            if ((layers[0] as IGeoFeatureLayer).Renderer is RepresentationRenderer)
                            {
                                bool res=false;
                                int ruleID;
                                res = int.TryParse(feature.get_Value(featureClass.FindField("RuleID")).ToString(), out ruleID);
                                if (!res) continue;
                                double markerSize = -1;
                                if (markerDic.ContainsKey(fclName))
                                {
                                    if (markerDic[fclName].ContainsKey(ruleID))
                                    {
                                        markerSize = markerDic[fclName][ruleID];
                                    }
                                }
                                if (markerSize == -1)
                                {
                                    IRepresentationClass repClass = ((IRepresentationRenderer)(layers[0] as IGeoFeatureLayer).Renderer).RepresentationClass;
                                    //IRepresentation rep = repClass.GetRepresentation(feature, mc);
                                    if (repClass == null || !repClass.RepresentationRules.Exists(ruleID))
                                        continue;//非法制图表达规则，不生成注记

                                    IRepresentationRule repRule = repClass.RepresentationRules.get_Rule(ruleID);
                                    IBasicSymbol pBasicSymbol = repRule.Layer[0];//取一个图层
                                    if (pBasicSymbol is IBasicMarkerSymbol)
                                    {
                                        IGraphicAttributes graphicAttributes = pBasicSymbol as IGraphicAttributes;
                                        double symbolSize = Convert.ToDouble(graphicAttributes.Value[2]);
                                        markerSize = symbolSize;

                                    }

                                    Dictionary<int, double> ruleSize = new Dictionary<int, double>();
                                    if (markerDic.ContainsKey(fclName))
                                    {
                                        ruleSize = markerDic[fclName];
                                    }
                                    markerSize = Math.Abs(markerSize);
                                    ruleSize[ruleID] = markerSize;
                                    markerDic[fclName] = ruleSize;
                                }
                                ISimpleMarkerSymbol sms = new SimpleMarkerSymbolClass();
                                sms.Size = markerSize;
                                featureSymbol = sms as ISymbol;
                            }

                            //纠正锚点不在点符号中心点的情况
                            if (annoRule.SymbolOffsetDX != 0.0 || annoRule.SymbolOffsetDY != 0.0)
                            {
                                IPoint p = geo as IPoint;
                                p.X += annoRule.SymbolOffsetDX * 1e-3 * _app.MapControl.ReferenceScale;
                                p.Y += annoRule.SymbolOffsetDY * 1e-3 * _app.MapControl.ReferenceScale;
                            }

                        }
                        else if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                        {
                            ISimpleLineSymbol sms = new SimpleLineSymbolClass();
                            featureSymbol = sms as ISymbol;
                        }
                        else if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                        {
                            ISimpleFillSymbol sms = new SimpleFillSymbolClass();
                            featureSymbol = sms as ISymbol;
                        }

                        //添加注记要素
                        maplexOverposter.AddFeature(leas.ClassIndex, geo, featureSymbol, feature.OID, Mao.AnnoText, 0, 0);
                        //释放组件
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(geo);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(featureSymbol);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                        cc++;
                    }
                    #endregion
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(ex.Message);
                }
            
                #region 基于避让规则表，收集所有需参与避让的Geo，为MaplexOverposter添加栅栏
                if (barrierRuleTable != null)
                {
                    //需避让的要素类名集合(如:AGNP：行政地名（点）、HYDP：水系（点）、RESP：居民地（点）、LRDL：机动车道路（线）、LRRL：铁路（线）、HYDL：水系（线）、VEGL：植被（线）等
                    IGeometryCollection geometryCollection1 = new GeometryBagClass();
                    IGeometryCollection geometryCollection2 = new GeometryBagClass();
                    IGeometryCollection geometryCollection3 = new GeometryBagClass();

                    for (int i = 0; i < barrierRuleTable.Rows.Count; i++)
                    {
                        try
                        {
                            ppp = i;
                            DataRow dr = barrierRuleTable.Rows[i];
                            for (int j = 0; j < dr.Table.Columns.Count; j++)
                            {
                                object val = dr[j];
                                if (val == null || Convert.IsDBNull(val))
                                    dr[j] = "";
                            }

                            string featureClassName = dr["图层"].ToString().Trim();
                            string condition = dr["定义查询"].ToString();
                            string level = dr["避让等级"].ToString();

                            var lyrs1 = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                            {
                                return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName.ToLower() == featureClassName.ToLower();
                            })).ToArray();
                            if (lyrs1.Length == 0)
                                continue;
                            ILayer lyr1 = lyrs1[0];
                            List<int> featureIDs = GetFeaturesInFeatureClass(featureClassName, condition);
                            if (featureIDs.Count == 0)
                                continue;

                            IGeometryCollection geometryCollection = null;
                            if (level == "高")
                            {
                                geometryCollection = geometryCollection1;
                            }
                            else if (level == "中")
                            {
                                geometryCollection = geometryCollection2;
                            }
                            else
                            {
                                geometryCollection = geometryCollection3;
                            }

                            var lyrs = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                            {
                                return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName.ToLower() == featureClassName.ToLower();
                            })).ToArray();
                            ILayer lyr = lyrs[0];

                            IFeatureClass featureClass = null;
                            if (fclDics.ContainsKey(featureClassName))
                            {
                                featureClass = fclDics[featureClassName];
                            }
                            else
                            {
                                featureClass = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
                            }
                            for (int j = 0; j < featureIDs.Count; j++)
                            {
                                IFeature feature = featureClass.GetFeature(featureIDs[j]);

                                if (wo != null)
                                    wo.SetText(string.Format("正在基于避让规则表添加避让要素:\r\n图层={0}、要素ID={1}", featureClassName, feature.OID));

                                IGeometry geometry = feature.ShapeCopy;
                                if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                                {
                                    var mList = MaoList.Where(M => M.AnnotationClassID == feature.Class.ObjectClassID && M.FeatureID == feature.OID).ToList();
                                    if (mList.Count > 0)
                                    {
                                        var Mao = mList[0];
                                        var thisleasList = leasList.Where(l => l.GroupSymbolID == Mao.GroupSymbolID && l.ClassIndex == Mao.ClassIndex).ToArray();
                                        if (thisleasList != null && thisleasList.Count() > 0)
                                        {
                                            AnnoRule annoRule = thisleasList[0].AnnotationRule;
                                            //纠正锚点不在点符号中心点的情况
                                            if (annoRule.SymbolOffsetDX != 0.0 || annoRule.SymbolOffsetDY != 0.0)
                                            {
                                                IPoint p = geometry as IPoint;
                                                p.X += annoRule.SymbolOffsetDX * 1e-3 * _app.MapControl.ReferenceScale;
                                                p.Y += annoRule.SymbolOffsetDY * 1e-3 * _app.MapControl.ReferenceScale;
                                            }
                                        }
                                            
                                    }

                                    double size = 1.2;//mm
                                    // if ((lyr as IGeoFeatureLayer).Renderer is RepresentationRenderer)
                                    {

                                        string fclName = feature.Class.AliasName;
                                        //获取size大小
                                        if ((lyr as IGeoFeatureLayer).Renderer is RepresentationRenderer)
                                        {
                                            int ruleID = -1;
                                            int.TryParse(feature.get_Value(featureClass.FindField("RuleID")).ToString(), out ruleID);
                                            double markerSize = -1;
                                            if (markerDic.ContainsKey(fclName))
                                            {
                                                if (markerDic[fclName].ContainsKey(ruleID))
                                                {
                                                    markerSize = markerDic[fclName][ruleID];
                                                }
                                            }
                                            if (markerSize == -1)
                                            {
                                                IRepresentationClass repClass = ((IRepresentationRenderer)(lyr as IGeoFeatureLayer).Renderer).RepresentationClass;
                                                //IRepresentation rep = repClass.GetRepresentation(feature, mc);
                                                if (repClass.RepresentationRules.Exists(ruleID))
                                                {
                                                    IRepresentationRule repRule = repClass.RepresentationRules.get_Rule(ruleID);
                                                    IBasicSymbol pBasicSymbol = repRule.Layer[0];//取一个图层
                                                    if (pBasicSymbol is IBasicMarkerSymbol)
                                                    {
                                                        IGraphicAttributes graphicAttributes = pBasicSymbol as IGraphicAttributes;
                                                        double symbolSize = Convert.ToDouble(graphicAttributes.Value[2]);
                                                        markerSize = symbolSize;

                                                    }
                                                }
                                                Dictionary<int, double> ruleSize = new Dictionary<int, double>();
                                                if (markerDic.ContainsKey(fclName))
                                                {
                                                    ruleSize = markerDic[fclName];
                                                }
                                                ruleSize[ruleID] = Math.Abs(markerSize);
                                                markerDic[fclName] = ruleSize;
                                            }

                                            size = markerSize * 0.5;
                                            size = size / 2.83;
                                            //pt转毫米
                                        }
                                    }

                                    geometry = (geometry as ITopologicalOperator).Buffer(size * 1e-3 * _app.MapControl.ReferenceScale);
                                }
                                else if (feature.Shape.GeometryType == esriGeometryType.esriGeometryLine)
                                {
                                    if ((lyr as IGeoFeatureLayer).Renderer is RepresentationRenderer)
                                    {
                                        IRepresentationClass repClass = ((IRepresentationRenderer)(lyr as IGeoFeatureLayer).Renderer).RepresentationClass;
                                        IRepresentation rep = repClass.GetRepresentation(feature, mc);

                                        if (repClass.RepresentationRules.Exists(rep.RuleID))
                                        {
                                            //rep.r
                                            IRepresentationRule repRule = repClass.RepresentationRules.get_Rule(rep.RuleID);
                                            IBasicSymbol pBasicSymbol = repRule.Layer[0];//取一个图层
                                            if (pBasicSymbol is IBasicLineSymbol)
                                            {
                                                //当前属性
                                                IGraphicAttributes ga = (pBasicSymbol as IBasicLineSymbol).Stroke as IGraphicAttributes;
                                                //线宽
                                                double lineWidth = Convert.ToDouble(ga.Value[0]) / 2.8345;// 磅-〉毫米

                                                geometry = (geometry as ITopologicalOperator).Buffer(lineWidth * 0.5 * 1e-3 * _app.MapControl.ReferenceScale);
                                            }
                                        }
                                    }
                                }

                                geometryCollection.AddGeometry(geometry);
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);

                                System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                            }

                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            System.Diagnostics.Trace.WriteLine(ex.Source);
                            System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                            MessageBox.Show(ex.ToString() + "...." + ppp.ToString());
                        }
                    }

                    //注记不能压盖内图廓
                    if (extentGeo != null)
                    {
                        geometryCollection1.AddGeometry(extentGeo);
                    }

                    //设置栅栏
                    maplexOverposter.AddBarriers(esriBasicOverposterWeight.esriHighWeight, geometryCollection1);
                    maplexOverposter.AddBarriers(esriBasicOverposterWeight.esriMediumWeight, geometryCollection2);
                    maplexOverposter.AddBarriers(esriBasicOverposterWeight.esriLowWeight, geometryCollection3);
                }
                #endregion
              
                if (wo != null)
                    wo.SetText("根据标注定位创建注记……");

                #region 放置标注，获取注记位置
                GC.Collect();
                //IFeatureClass annofcl = annoName2FeatureClass.First().Value;
                //IFeatureCursor editCursor = annofcl.Insert(true);
                //IFeatureBuffer newFeBuffer = annofcl.CreateFeatureBuffer();
                //渲染注记的位置
                maplexOverposter.PlaceLabels();

                //已放置标注
                IEnumMaplexPlacedLabel enumMaplexPlacedLabel = maplexOverposter.PlacedLabels;
                enumMaplexPlacedLabel.Reset();
                IMaplexPlacedLabel maplexPlacedLabel = null;
                while ((maplexPlacedLabel = enumMaplexPlacedLabel.Next()) != null)
                {
                    var mList = MaoList.Where(M => M.ClassIndex == maplexPlacedLabel.ClassIndex && M.FeatureID == maplexPlacedLabel.FeatureID).ToList();
                    if (mList.Count > 0)
                    {
                        mList[0].TextPathList.Add(maplexPlacedLabel.TextPath);
                        mList[0].TextBoundList.Add(maplexPlacedLabel.Bounds);


                        var Mao = mList[0];
                        var thisleasList = leasList.Where(l => l.GroupSymbolID == Mao.GroupSymbolID && l.ClassIndex == Mao.ClassIndex).ToArray();
                        if (thisleasList == null || thisleasList.Count() == 0)
                            continue;
                        AnnoRule annoRule = thisleasList[0].AnnotationRule;
                        IFeatureClass featureClass = null;
                        string featureClassName = Mao.FeatureClassName;
                        if (fclDics.ContainsKey(featureClassName))
                        {
                            featureClass = fclDics[featureClassName];
                        }
                        else
                        {
                            featureClass = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
                        }

                    //    IFeature feature = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(Mao.FeatureClassName).GetFeature(Mao.FeatureID);
                        IFeature feature = featureClass.GetFeature(Mao.FeatureID);

                        if (wo != null)
                            wo.SetText(string.Format("正在添加已放置的注记:\r\n关联要素所属图层={0}、关联要素ID={1}", featureClassName, feature.OID));

                        if (annoRule.EnableThreeDim)//立体效果
                        {
                            int numLayer = 11;//层数

                            ICmykColor fontColor = annoRule.FontColor as ICmykColor;
                            int cyanDelta = (int)(fontColor.Cyan * 0.5 / (numLayer-1));//最后一层50%
                            int magentaDelta = (int)(fontColor.Magenta * 0.5 / (numLayer-1));
                            int yellowDelta = (int)(fontColor.Yellow * 0.5 / (numLayer-1));
                            int blackDelta = (int)(fontColor.Black * 0.5 / (numLayer-1));

                            double fontSize = annoRule.FontSize * 2.8345;
                            double xOffsetDelta = fontSize * 0.1 / (numLayer - 1);//最后一层10%
                            double yOffsetDelta = -fontSize * 0.1 / (numLayer - 1);
                            for (int i = numLayer - 1; i >= 0; --i)
                            {
                                ICmykColor color = fontColor;
                                if (i != 0)
                                {
                                    color = new CmykColorClass()
                                    {
                                        Cyan = i * cyanDelta,
                                        Magenta = i * magentaDelta,
                                        Yellow = i * yellowDelta,
                                        Black = i * blackDelta
                                    };
                                }
                                double xOffset = i * xOffsetDelta;
                                double yOffset = i * yOffsetDelta;

                                string subMessage = string.Empty;
                                ITextElement textElement = AnnoHelper.CreateTextElement(maplexPlacedLabel, annoRule, out subMessage);
                                if (subMessage != "" && !WarnErr.Contains(subMessage))
                                {
                                    WarnErr += subMessage + "\n";
                                }
                                ISymbolCollectionElement symbolCollEle = (ISymbolCollectionElement)textElement;
                                symbolCollEle.Color = color;
                                symbolCollEle.XOffset = xOffset;
                                symbolCollEle.YOffset = yOffset;

                                IFeature pNomalFeature = annoName2FeatureClass[annoRule.AnnoLayerName].CreateFeature();
                                IAnnotationFeature2 pNomalAnnoFeature = pNomalFeature as IAnnotationFeature2;

                                //修正点注记的水平对齐方式
                                if (feature.Shape is IPoint)
                                {
                                    ISymbolCollectionElement pSymbolCollEle = textElement as ISymbolCollectionElement;
                                    if (pSymbolCollEle.AnchorPoint.X < (feature.Shape as IPoint).X)
                                    {
                                        pSymbolCollEle.HorizontalAlignment = esriTextHorizontalAlignment.esriTHARight;
                                    }
                                    else if (pSymbolCollEle.AnchorPoint.X > (feature.Shape as IPoint).X)
                                    {
                                        pSymbolCollEle.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
                                    }
                                }
                                pNomalAnnoFeature.Annotation = textElement as IElement;


                                pNomalAnnoFeature.AnnotationClassID = Mao.AnnotationClassID;
                                pNomalAnnoFeature.LinkedFeatureID = Mao.FeatureID;
                                pNomalAnnoFeature.Status = esriAnnotationStatus.esriAnnoStatusPlaced;
                                pNomalFeature.Store();
                            }
                        }
                        else
                        {
                            string subMessage = string.Empty;
                            ITextElement textElement = AnnoHelper.CreateTextElement(maplexPlacedLabel, annoRule, out subMessage);
                            if (subMessage != "" && !WarnErr.Contains(subMessage))
                            {
                                WarnErr += subMessage + "\n";
                            }

                            IFeature pNomalFeature = annoName2FeatureClass[annoRule.AnnoLayerName].CreateFeature();
                            IAnnotationFeature2 pNomalAnnoFeature = pNomalFeature as IAnnotationFeature2;

                            //修正点注记的水平对齐方式
                            if (feature.Shape is IPoint)
                            {
                                ISymbolCollectionElement pSymbolCollEle = textElement as ISymbolCollectionElement;
                                if (pSymbolCollEle.AnchorPoint.X < (feature.Shape as IPoint).X)
                                {
                                    pSymbolCollEle.HorizontalAlignment = esriTextHorizontalAlignment.esriTHARight;
                                }
                                else if (pSymbolCollEle.AnchorPoint.X > (feature.Shape as IPoint).X)
                                {
                                    pSymbolCollEle.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
                                }
                            }

                            if (GApplication.Application.Template.Root.Contains("江苏") || GApplication.Application.Template.Root.Contains("吉林"))
                            {
                                if (featureClassName.ToUpper().Contains("BOUA") && feature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                                {
                                    //将表面注记移至boua中心，不考虑压盖
                                    IArea area = feature.Shape as IArea;
                                    if (area != null && area.LabelPoint != null)
                                    {
                                        IPoint labelPoint = area.LabelPoint;

                                        ISymbolCollectionElement symbolCollEle = textElement as ISymbolCollectionElement;
                                        symbolCollEle.AnchorPoint = labelPoint;
                                        (textElement as IElement).Geometry = labelPoint;
                                    }
                                }
                            }

                            pNomalAnnoFeature.Annotation = textElement as IElement;


                            pNomalAnnoFeature.AnnotationClassID = Mao.AnnotationClassID;
                            pNomalAnnoFeature.LinkedFeatureID = Mao.FeatureID;
                            pNomalAnnoFeature.Status = esriAnnotationStatus.esriAnnoStatusPlaced;
                            pNomalFeature.Store();
                        }
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(maplexPlacedLabel);
                }
                GC.Collect();
                //未放置标注
                int unPlacedLabelOfRESP = 0;//resp中未放置注记的点数目
                IEnumMaplexPlacedLabel enumMaplexUnplacedLabel = maplexOverposter.UnplacedLabels;
                enumMaplexUnplacedLabel.Reset();
                IMaplexPlacedLabel maplexUnPlacedLabel = null;
                while ((maplexUnPlacedLabel = enumMaplexUnplacedLabel.Next()) != null)
                {
                    var mList = MaoList.Where(M => M.ClassIndex == maplexUnPlacedLabel.ClassIndex && M.FeatureID == maplexUnPlacedLabel.FeatureID).ToList();
                    if (mList.Count > 0)
                    {
                        mList[0].TextPathList.Add(maplexUnPlacedLabel.TextPath);
                        mList[0].TextBoundList.Add(maplexUnPlacedLabel.Bounds);

                        var Mao = mList[0];
                        var thisleasList = leasList.Where(l => l.GroupSymbolID == Mao.GroupSymbolID && l.ClassIndex == Mao.ClassIndex).ToArray();
                        if (thisleasList == null || thisleasList.Count() == 0)
                            continue;
                        AnnoRule annoRule = thisleasList[0].AnnotationRule;

                        IFeatureClass featureClass = null;
                        string featureClassName = Mao.FeatureClassName;
                        if (fclDics.ContainsKey(featureClassName))
                        {
                            featureClass = fclDics[featureClassName];
                        }
                        else
                        {
                            featureClass = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
                        }

                       // IFeature feature = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(Mao.FeatureClassName).GetFeature(Mao.FeatureID);
                        IFeature feature = featureClass.GetFeature(Mao.FeatureID);

                        if (wo != null)
                            wo.SetText(string.Format("正在添加已放置的注记:\r\n关联要素所属图层={0}、关联要素ID={1}", featureClassName, feature.OID));


                        if (annoRule.EnableThreeDim)//立体效果
                        {
                            int numLayer = 11;//层数

                            ICmykColor fontColor = annoRule.FontColor as ICmykColor;
                            int cyanDelta = (int)(fontColor.Cyan * 0.5 / (numLayer - 1));//最后一层50%
                            int magentaDelta = (int)(fontColor.Magenta * 0.5 / (numLayer - 1));
                            int yellowDelta = (int)(fontColor.Yellow * 0.5 / (numLayer - 1));
                            int blackDelta = (int)(fontColor.Black * 0.5 / (numLayer - 1));

                            double fontSize = annoRule.FontSize * 2.8345;
                            double xOffsetDelta = fontSize * 0.1 / (numLayer - 1);//最后一层10%
                            double yOffsetDelta = -fontSize * 0.1 / (numLayer - 1);
                            for (int i = numLayer-1; i >=0; --i)
                            {
                                ICmykColor color = fontColor;
                                if (i != 0)
                                {
                                    color = new CmykColorClass()
                                    {
                                        Cyan = i * cyanDelta,
                                        Magenta = i * magentaDelta,
                                        Yellow = i * yellowDelta,
                                        Black = i * blackDelta
                                    };
                                }
                                double xOffset = i * xOffsetDelta;
                                double yOffset = i * yOffsetDelta;

                                string subMessage = string.Empty;
                                ITextElement textElement = AnnoHelper.CreateTextElement(maplexUnPlacedLabel, annoRule, out subMessage);
                                if (subMessage != "" && !WarnErr.Contains(subMessage))
                                {
                                    WarnErr += subMessage + "\n";
                                }
                                ISymbolCollectionElement symbolCollEle = (ISymbolCollectionElement)textElement;
                                symbolCollEle.Color = color;
                                symbolCollEle.XOffset = xOffset;
                                symbolCollEle.YOffset = yOffset;

                                IFeature pNomalFeature = annoName2FeatureClass[annoRule.AnnoLayerName].CreateFeature();
                                IAnnotationFeature2 pNomalAnnoFeature = pNomalFeature as IAnnotationFeature2;
                                pNomalAnnoFeature.Annotation = textElement as IElement;
                                pNomalAnnoFeature.AnnotationClassID = Mao.AnnotationClassID;
                                pNomalAnnoFeature.LinkedFeatureID = Mao.FeatureID;
                                pNomalAnnoFeature.Status = esriAnnotationStatus.esriAnnoStatusUnplaced;

                                //为放置的注记，则将注记对应的点要素(RESP）隐藏掉
                                if (feature.Class.AliasName.ToUpper() == "RESP" || feature.Class.AliasName.ToUpper() == "AGNP")
                                {
                                    int selStateIndex = feature.Fields.FindField("selectstate");
                                    if (selStateIndex != -1)
                                    {
                                        feature.set_Value(selStateIndex, "关联注记未选取");
                                        feature.Store();

                                        unPlacedLabelOfRESP++;
                                    }
                                }
                                pNomalFeature.Store();
                            }
                        }
                        else
                        {
                            string subMessage = string.Empty;
                            ITextElement textElement = AnnoHelper.CreateTextElement(maplexUnPlacedLabel, annoRule, out subMessage);
                            if (subMessage != "" && !WarnErr.Contains(subMessage))
                            {
                                WarnErr += subMessage + "\n";
                            }

                            IFeature pNomalFeature = annoName2FeatureClass[annoRule.AnnoLayerName].CreateFeature();
                            IAnnotationFeature2 pNomalAnnoFeature = pNomalFeature as IAnnotationFeature2;

                            if (GApplication.Application.Template.Root.Contains("江苏") || GApplication.Application.Template.Root.Contains("吉林"))
                            {
                                if (featureClassName.ToUpper().Contains("BOUA") && feature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                                {
                                    //将表面注记移至boua中心，不考虑压盖
                                    IArea area = feature.Shape as IArea;
                                    if (area != null && area.LabelPoint != null)
                                    {
                                        IPoint labelPoint = area.LabelPoint;

                                        ISymbolCollectionElement symbolCollEle = textElement as ISymbolCollectionElement;
                                        symbolCollEle.AnchorPoint = labelPoint;
                                        (textElement as IElement).Geometry = labelPoint;
                                    }
                                }
                            }

                            pNomalAnnoFeature.Annotation = textElement as IElement;
                            pNomalAnnoFeature.AnnotationClassID = Mao.AnnotationClassID;
                            pNomalAnnoFeature.LinkedFeatureID = Mao.FeatureID;
                            pNomalAnnoFeature.Status = esriAnnotationStatus.esriAnnoStatusUnplaced;

                            //为放置的注记，则将注记对应的点要素(RESP）隐藏掉
                            if (feature.Class.AliasName.ToUpper() == "RESP" || feature.Class.AliasName.ToUpper() == "AGNP")
                            {
                                int selStateIndex = feature.Fields.FindField("selectstate");
                                if (selStateIndex != -1)
                                {
                                    feature.set_Value(selStateIndex, "关联注记未选取");
                                    feature.Store();

                                    unPlacedLabelOfRESP++;
                                }
                            }

                            pNomalFeature.Store();
                        }
                        
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(maplexUnPlacedLabel);
                }

                GC.Collect();
                
                if (wo != null)
                    wo.SetText("字头朝北处理……");
                //1.水系打散，字头朝北
                AnnoBreakFaceToNorth anno2North = new AnnoBreakFaceToNorth(_app);
                anno2North.BreakfclName = "HYDL";
                anno2North.toNorth(annoName2FeatureClass);
                //2.山岭打散，字头朝北
                var annolyr = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "AANL");
                })).FirstOrDefault() as IFeatureLayer;
                if (annolyr != null)
                {
                    if (annolyr.FeatureClass.FeatureCount(null) > 0)
                    {
                        anno2North.BreakfclName = "AANL";
                        anno2North.toNorth(annoName2FeatureClass);
                    }
                }
                annolyr = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "BRGL");
                })).FirstOrDefault() as IFeatureLayer;
                if (annolyr != null)
                {
                    if (annolyr.FeatureClass.FeatureCount(null) > 0)
                    {
                        anno2North.BreakfclName = "BRGL";
                        anno2North.toNorth(annoName2FeatureClass);
                    }
                }
                //天地图水系注记（吉林）
                annolyr = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "HCTLN");
                })).FirstOrDefault() as IFeatureLayer;
                if (annolyr != null)
                {
                    if (annolyr.FeatureClass.FeatureCount(null) > 0)
                    {
                        anno2North.BreakfclName = "HCTLN";
                        anno2North.toNorth(annoName2FeatureClass);
                    }
                }
                GC.Collect();
                //河流空格
                if (wo != null)
                    wo.SetText("河流间距处理……");
                foreach (var kv in annoName2FeatureClass)
                {
                    DeleteSpaceChars(kv.Value);
                }
                
                #endregion


                _app.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }

            return WarnErr;
        }

        //删除河流空格
        private void DeleteSpaceChars(IFeatureClass featureClass)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TextString ='$'";

            IFeatureCursor cursor = featureClass.Update(qf, false);
            IFeature fe = null;

            while ((fe = cursor.NextFeature()) != null)
            {
                fe.Delete();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);


        }
       
        /// <summary>
        /// 多条件查询相关的要素ID
        /// </summary>
        /// <param name="featureClassName"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        private List<int> GetFeaturesInFeatureClass(string featureClassName, string condition)
        {
            List<int> featureIDList = new List<int>();

            try
            {
                IFeatureClass featureClass = null;
                if ((_app.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, featureClassName))
                {
                    featureClass = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
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
    }

    

}

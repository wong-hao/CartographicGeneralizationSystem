using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Maplex;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.IO;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataManagementTools;


namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 根据选择的比例尺、模板风格及纸张开本情况更新数据的符号及注记大小等
    /// 更新时，若指定了开本，则按底图模板类型、开本类型选择符号模板（mxd）及注记规则，根据底图模板类型、比例尺选择图层对照规则。
    /// </summary>
    public class DataSymbolAndAnnotationUpdateCmd : SMGICommand
    {
        public DataSymbolAndAnnotationUpdateCmd()
        {
            m_caption = "模板规则更新";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null;
            }
        }
       
        public override void OnClick()
        {
            DataSymbolAndAnnotationUpdateForm dataUpdateFrom = new DataSymbolAndAnnotationUpdateForm(m_Application);
            dataUpdateFrom.Text = "模板规则更新";
            if (DialogResult.OK != dataUpdateFrom.ShowDialog())
                return;

            try
            {
                string warnningStr = "";
                using (var wo = m_Application.SetBusy())
                {
                    wo.SetText("正在更新要素符号...");

                    #region 符号模板更新（基于数据结构升级模块改造）
                    string sourceFileGDB = dataUpdateFrom.SourceGDBFile;
                    string outputFileGDB = dataUpdateFrom.OutputGDBFile;
                    int mapScale = int.Parse(dataUpdateFrom.Mapscale);

                    //获取底图模板配置信息
                    string mxdFullFileName = EnvironmentSettings.getMxdFullFileName(m_Application);
                    string ruleMatchFileName = EnvironmentSettings.getLayerRuleDBFileName(m_Application);
                    DataTable layerRuleDT = CommonMethods.ReadToDataTable(ruleMatchFileName, "图层对照规则");
                    if (layerRuleDT.Rows.Count == 0)
                    {
                        MessageBox.Show("没有找到图层对照规则或该规则表中没有内容！");
                        return;
                    }

                    //获取专题模板配置信息
                    Dictionary<string, string> themTemplateFCName2themName = new Dictionary<string, string>();//图层名-专题名称
                    Dictionary<string, IFeatureClass> themTemplateFCName2FC = new Dictionary<string, IFeatureClass>();//图层名-模板要素类（空）
                    Dictionary<string, DataTable> themName2DT = new Dictionary<string, DataTable>();//专题名称->图层规则表
                    if (CommonMethods.ThemData)
                    {
                        #region 专题规则
                        string dirpath = GApplication.Application.Template.Root + "\\专题\\";
                        DirectoryInfo dirs = new DirectoryInfo(dirpath);
                        Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                        IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                        string gdb = dirpath + CommonMethods.ThemDataBase + "\\Template.gdb";
                        if (Directory.Exists(gdb))
                        {
                            IWorkspace themWS = workspaceFactory.OpenFromFile(gdb, 0);
                            IEnumDataset enumDataset = themWS.get_Datasets(esriDatasetType.esriDTAny);
                            enumDataset.Reset();
                            IDataset dataset = null;
                            while ((dataset = enumDataset.Next()) != null)
                            {
                                if (dataset is IFeatureDataset)//数据集
                                {
                                    var enumds = (dataset as IFeatureDataset).Subsets;
                                    enumds.Reset();
                                    IDataset ds = null;
                                    while ((ds = enumds.Next()) != null)
                                    {
                                        if (ds is IFeatureClass)
                                        {
                                            themTemplateFCName2themName.Add(ds.Name, CommonMethods.ThemDataBase);
                                            themTemplateFCName2FC.Add(ds.Name, ds as IFeatureClass);
                                        }
                                    }
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(enumds);
                                }
                                if (dataset is IFeatureClass)
                                {
                                    themTemplateFCName2themName.Add(dataset.Name, CommonMethods.ThemDataBase);
                                    themTemplateFCName2FC.Add(dataset.Name, dataset as IFeatureClass);
                                }

                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);
                        }

                        foreach (var kv in themTemplateFCName2themName)
                        {
                            if (!themName2DT.ContainsKey(kv.Value))
                            {
                                string rulepath = m_Application.Template.Root + "\\专题\\" + kv.Value + "\\规则对照.mdb";
                                DataTable dtLayerRule = CommonMethods.ReadToDataTable(rulepath, "图层对照规则");
                                themName2DT[kv.Value] = dtLayerRule;
                            }

                        }
                        #endregion
                    }

                    //导出数据到目标数据库（升级后）
                    Dictionary<int, int> fcObjectClassIDMapping = new Dictionary<int, int>();
                    Dictionary<int, Dictionary<int, int>> sourceFCID2FIDMapping = new Dictionary<int, Dictionary<int, int>>();
                    if (!UpgradeBaseData(m_Application, sourceFileGDB, themTemplateFCName2FC, outputFileGDB, ref fcObjectClassIDMapping, ref sourceFCID2FIDMapping))
                    {
                        return;
                    }

                    //打开地图模板
                    IMapDocument pMapDoc = new MapDocumentClass();
                    pMapDoc.Open(mxdFullFileName, "");
                    if (pMapDoc.MapCount == 0)//如果地图模板为空
                    {
                        MessageBox.Show("地图模板不能为空！");
                        return;
                    }
                    IMap templateMap = pMapDoc.get_Map(0);
                    string engineName = templateMap.AnnotationEngine.Name;

                    //加载目标数据库（升级后）到视图
                    CommonMethods.OpenGDBFile(m_Application, outputFileGDB);
                    IActiveView view = m_Application.ActiveView;
                    IMap map = view.FocusMap;
                    if (engineName.Contains("Maplex"))
                    {
                        IAnnotateMap sm = new MaplexAnnotateMapClass();
                        map.AnnotationEngine = sm;
                    }
                    else
                    {
                        map.AnnotationEngine = templateMap.AnnotationEngine;
                    }
                    map.ReferenceScale = mapScale;
                    templateMap.SpatialReference = map.SpatialReference;
                    m_Application.Workspace.LayerManager.Map.ClearLayers();

                    #region 重新匹配制图表达
                    List<ILayer> layers = new List<ILayer>();
                    for (int i = templateMap.LayerCount - 1; i >= 0; i--)//加载底图模板图层
                    {
                        var l = templateMap.get_Layer(i);
                        layers.Add(l);
                    }
                    IGroupLayer comlyr = new GroupLayerClass();
                    comlyr.Name = "底图";
                    layers.Reverse();
                    foreach (var item in layers)
                    {
                        comlyr.Add(item);
                    }
                    MatchLayer(m_Application, comlyr, null, layerRuleDT, themTemplateFCName2themName, themName2DT);
                    #endregion

                    #region 重新匹配专题图层数据的制图表达
                    layers.Clear();
                    foreach (var kv in themTemplateFCName2themName)
                    {
                        var l = new FeatureLayerClass();
                        l.Name = kv.Key;
                        IFeatureClass fc = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(l.Name);
                        (l as IFeatureLayer).FeatureClass = fc;
                        layers.Add(l);
                    }
                    if (layers.Count > 0)
                    {
                        IGroupLayer thlyr = new GroupLayerClass();
                        thlyr.Name = "专题";
                        layers.Reverse();
                        foreach (var item in layers)
                        {
                            thlyr.Add(item);
                        }
                        MatchLayer(m_Application, thlyr, null, layerRuleDT, themTemplateFCName2themName, themName2DT);
                    }
                    #endregion


                    #region 修改注记地图比例尺
                    var lyrsAnno = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IFeatureLayer);
                    })).ToArray();

                    foreach (var l in lyrsAnno)
                    {
                        IFeatureClass pfcl = (l as IFeatureLayer).FeatureClass;
                        if (pfcl.Extension is IAnnoClass)
                        {
                            IAnnoClass pAnno = pfcl.Extension as IAnnoClass;
                            IAnnoClassAdmin3 pAnnoAdmin = pAnno as IAnnoClassAdmin3;
                            if (pAnno.ReferenceScale != map.ReferenceScale)
                            {
                                pAnnoAdmin.AllowSymbolOverrides = true;
                                pAnnoAdmin.ReferenceScale = map.ReferenceScale;
                                pAnnoAdmin.UpdateProperties();
                            }
                        }
                    }
                    #endregion

                    #region 更新视图
                    var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IGeoFeatureLayer;
                    })).ToArray();

                    IEnvelope env = null;
                    for (int i = 0; i < lyrs.Length; i++)
                    {
                        IFeatureClass fc = (lyrs[i] as IFeatureLayer).FeatureClass;
                        IFeatureClassManage fcMgr = fc as IFeatureClassManage;
                        fcMgr.UpdateExtent();

                        IEnvelope e = (fc as IGeoDataset).Extent;
                        if (!e.IsEmpty)
                        {
                            if (null == env)
                            {
                                env = e;
                            }
                            else
                            {
                                env.Union(e);
                            }
                        }
                    }

                    if (env != null && !env.IsEmpty)
                    {
                        env.Expand(1.2, 1.2, true);
                        m_Application.MapControl.Extent = env;
                        m_Application.Workspace.Map.AreaOfInterest = env;

                        for (int i = 0; i < lyrs.Length; i++)
                        {
                            (lyrs[i] as ILayer2).AreaOfInterest = env;
                        }
                    }
                    #endregion

                    //保存工程
                    m_Application.Workspace.Save();
                    GC.Collect();

                    //将环境配置信息写入econfig
                    EnvironmentSettings.UpdateEnvironmentToConfig(dataUpdateFrom.AttachMap);

                    #endregion

                    #region 注记大小更新
                    wo.SetText("正在更新注记...");
                    string annoRuleDBFileName = EnvironmentSettings.getAnnoRuleDBFileName(m_Application);
                    DataTable annoRuleDT = CommonMethods.ReadToDataTable(annoRuleDBFileName, "注记规则");
                    DataTable fontmappingTable = CommonMethods.ReadToDataTable(annoRuleDBFileName, "字体映射");
                    AddThematicAnnoRule(m_Application, annoRuleDT);//添加专题注记规则

                    //获取注记要素类
                    Dictionary<string, IFeatureClass> annoName2FeatureClass = new Dictionary<string, IFeatureClass>();
                    System.Data.DataTable dtAnnoLayers = annoRuleDT.AsDataView().ToTable(true, new string[] { "注记图层" });//distinct
                    for (int i = 0; i < dtAnnoLayers.Rows.Count; ++i)
                    {
                        //图层名
                        string annoLayerName = dtAnnoLayers.Rows[i]["注记图层"].ToString().Trim();
                        if (!(m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, annoLayerName))
                        {
                            MessageBox.Show(string.Format("当前数据库缺少注记要素[{0}]!", annoLayerName), "警告", MessageBoxButtons.OK);
                            return;
                        }

                        IFeatureClass annoFeatureClass = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(annoLayerName);

                        annoName2FeatureClass.Add(annoLayerName, annoFeatureClass);
                    }

                    if (annoName2FeatureClass.Count == 0)
                    {
                        MessageBox.Show("规则库中没有指定注记目标图层!", "警告", MessageBoxButtons.OK);
                        return;
                    }

                    //更新注记字体等信息
                    updateAnnoFont(m_Application, annoName2FeatureClass, annoRuleDT, fontmappingTable, fcObjectClassIDMapping, sourceFCID2FIDMapping);

                    #endregion
                }

                if (warnningStr != "")
                {
                    MessageBox.Show(warnningStr, "警告", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
        }

        public static bool UpgradeBaseData(GApplication app, string sourceFileGDB, Dictionary<string, IFeatureClass> themTemplateFCName2FC, string upgradeFileGDB,
            ref Dictionary<int, int> fcObjectClassIDMapping, ref Dictionary<int, Dictionary<int, int>> sourceFCID2FIDMapping)
        {
            bool res = true;
            try
            {
                if (!Directory.Exists(sourceFileGDB))
                {
                    MessageBox.Show("升级数据文件不存在！");
                    return false;
                }
                if (Directory.Exists(upgradeFileGDB))
                {
                    MessageBox.Show("导出数据文件已经存在！");
                    return false;
                }

                //模板数据库
                string templateFullFileName = EnvironmentSettings.getTemplateFullFileName(app);
                if (!Directory.Exists(templateFullFileName))
                {
                    MessageBox.Show("模板数据文件不存在！");
                    return false;
                }
                IWorkspaceFactory tempWSFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace tempWorkspace = tempWSFactory.OpenFromFile(templateFullFileName, 0);
                
                //源数据库
                IWorkspaceFactory sourceWSFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace sourceWorkspace = sourceWSFactory.OpenFromFile(sourceFileGDB, 0);

                //创建输出数据库
                Dictionary<IFeatureClass, IFeatureClass> fcList = CreateDBStructByTemplate(app, sourceWorkspace, tempWorkspace, themTemplateFCName2FC, upgradeFileGDB);

                
                //拷贝数据
                fcObjectClassIDMapping = new Dictionary<int, int>();
                sourceFCID2FIDMapping = new Dictionary<int, Dictionary<int, int>>();
                foreach (var item in fcList)
                {
                    //收集信息
                    fcObjectClassIDMapping.Add(item.Key.ObjectClassID, item.Value.ObjectClassID);

                    //拷贝数据
                    Dictionary<int, int> sourceOID2TargetOID = CopyFeatures(item.Key, null, item.Value);

                    //收集信息
                    sourceFCID2FIDMapping.Add(item.Key.ObjectClassID, sourceOID2TargetOID);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                res = false ;
            }

            return res;
        }

        public static void ReDefineWorkspaceSpatialReference(IWorkspace ws, ISpatialReference targetSpatialRef)
        {
            IClone targetSpatialRefClone = targetSpatialRef as IClone;

            IEnumDataset enumDataset = ws.get_Datasets(esriDatasetType.esriDTAny);
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset is IFeatureDataset)
                {
                    if (dataset.Name == "位置图")
                    {
                        continue;
                    }

                    ISpatialReference spatialRef = (dataset as IGeoDataset).SpatialReference;
                    IClone spatialRefClone = spatialRef as IClone;
                    if (!targetSpatialRefClone.IsEqual(spatialRefClone))
                    {
                        IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = dataset as IGeoDatasetSchemaEdit;
                        if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                        {
                            pGeoDatasetSchemaEdit.AlterSpatialReference(targetSpatialRef);
                        }
                    }

                    IEnumDataset enumSubset = dataset.Subsets;
                    enumSubset.Reset();
                    IDataset subsetDataset = enumSubset.Next();
                    while (subsetDataset != null)
                    {
                        if (subsetDataset is IFeatureClass)
                        {
                            if (!targetSpatialRefClone.IsEqual(spatialRefClone))
                            {
                                IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = subsetDataset as IGeoDatasetSchemaEdit;
                                if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                                {
                                    pGeoDatasetSchemaEdit.AlterSpatialReference(targetSpatialRef);
                                }
                            }
                        }
                        subsetDataset = enumSubset.Next();
                    }
                }
                else if (dataset is IFeatureClass)
                {
                    ISpatialReference spatialRef = (dataset as IGeoDataset).SpatialReference;
                    IClone spatialRefClone = spatialRef as IClone;
                    if (!targetSpatialRefClone.IsEqual(spatialRefClone))
                    {
                        IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = dataset as IGeoDatasetSchemaEdit;
                        if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                        {
                            pGeoDatasetSchemaEdit.AlterSpatialReference(targetSpatialRef);
                        }
                    }
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);
        }

        //复制模板数据库
        public static IWorkspace CopyTemplateDatabase(GApplication app, IWorkspace templateWorkspace, string outputGDB)
        {
            string msg = "";

            Copy pCopy = new Copy();
            pCopy.in_data = templateWorkspace.PathName;
            pCopy.out_data = outputGDB;

            IWorkspace ws = null;
            try
            {
                SMGI.Common.Helper.ExecuteGPTool(app.GPTool, pCopy, null);

                IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                ws = wsFactory.OpenFromFile(outputGDB, 0);

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLine(err.Message);
                System.Diagnostics.Trace.WriteLine(err.Source);
                System.Diagnostics.Trace.WriteLine(err.StackTrace);

                msg += err.Message;
                MessageBox.Show(msg);
            }

            return ws;
        }

        /// <summary>
        /// 复制要素类到数据库
        /// </summary>
        /// <param name="app"></param>
        /// <param name="fc"></param>
        /// <param name="targetWorkspace"></param>
        /// <param name="featureDatasetName"></param>
        /// <param name="targetSpatialRef"></param>
        /// <returns></returns>
        public static IFeatureClass CopyFC2Database(GApplication app, IFeatureClass fc, IWorkspace targetWorkspace, string featureDatasetName, ISpatialReference targetSpatialRef)
        {
            string msg = "";

            Copy pCopy = new Copy();
            pCopy.in_data = (fc as IDataset).Workspace.PathName + "\\" + fc.AliasName;
            string out_data = targetWorkspace.PathName + "\\";
            if (featureDatasetName != "")
            {
                out_data += featureDatasetName + "\\";
            }
            pCopy.out_data = out_data + fc.AliasName;

            IFeatureClass result = null;
            try
            {
                SMGI.Common.Helper.ExecuteGPTool(app.GPTool, pCopy, null);

                result = (targetWorkspace as IFeatureWorkspace).OpenFeatureClass(fc.AliasName);

                #region 重定义空间参考
                ISpatialReference spatialRef = (result as IGeoDataset).SpatialReference;
                IClone spatialRefClone = spatialRef as IClone;
                var targetSpatialRefClone = targetSpatialRef as IClone;
                if (!spatialRefClone.IsEqual(targetSpatialRefClone))
                {
                    IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = result as IGeoDatasetSchemaEdit;
                    if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                    {
                        pGeoDatasetSchemaEdit.AlterSpatialReference(targetSpatialRef);
                    }
                }
                #endregion
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLine(err.Message);
                System.Diagnostics.Trace.WriteLine(err.Source);
                System.Diagnostics.Trace.WriteLine(err.StackTrace);

                msg += err.Message;
                MessageBox.Show(msg);
            }

            return result;

        }

        /// <summary>
        /// 获取要素类的字段结构信息
        /// </summary>
        /// <param name="sourceFeatureClass"></param>
        /// <param name="srf"></param>
        /// <param name="bFieldNameUpper"></param>
        /// <returns></returns>
        public static IFields getFeatureClassFields(IFeatureClass sourceFeatureClass, ISpatialReference srf, bool bFieldNameUpper = false)
        {
            //获取源要素类的字段结构信息
            IFields targetFields = null;
            IObjectClassDescription featureDescription = new FeatureClassDescriptionClass();
            targetFields = featureDescription.RequiredFields; //要素类自带字段

            for (int i = 0; i < sourceFeatureClass.Fields.FieldCount; ++i)
            {
                IField field = sourceFeatureClass.Fields.get_Field(i);

                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    (targetFields as IFieldsEdit).set_Field(targetFields.FindFieldByAliasName((featureDescription as IFeatureClassDescription).ShapeFieldName),
                        (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField);

                    continue;
                }

                //剔除面积、长度字段
                if (field == sourceFeatureClass.LengthField || field == sourceFeatureClass.AreaField)
                {
                    continue;
                }

                //已包含该字段（要素类自带字段）
                if (targetFields.FindField(field.Name) != -1)
                {
                    continue;
                }

                IField newField = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (targetFields as IFieldsEdit).AddField(newField);
            }

            IGeometryDef pGeometryDef = new GeometryDefClass();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.SpatialReference_2 = srf;
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

            if (bFieldNameUpper)//转换为大写
            {
                for (int i = 0; i < targetFields.FieldCount; i++)
                {
                    IField pfield = targetFields.get_Field(i);

                    IFieldEdit2 pFieldEdit = pfield as IFieldEdit2;
                    pFieldEdit.Name_2 = pfield.Name.ToUpper();
                    pFieldEdit.AliasName_2 = pfield.AliasName.ToUpper();
                }

            }


            return targetFields;
        }

        /// <summary>
        /// 根据源数据库、相关模板信息(底图、专题)等，创建目标数据库结构（若源数据库包含栅格数据，则直接拷贝栅格数据到输出数据库）
        /// </summary>
        /// <param name="app"></param>
        /// <param name="sourceWorkspace">源数据库</param>
        /// <param name="templateWorkspace">底图模板库</param>
        /// <param name="themTemplateFCName2FC">专题模板中的矢量要素类集合</param>
        /// <param name="outputGDB">输出GDB路径</param>
        /// <returns></returns>
        public static Dictionary<IFeatureClass, IFeatureClass> CreateDBStructByTemplate(GApplication app, IWorkspace sourceWorkspace, IWorkspace templateWorkspace, Dictionary<string, IFeatureClass> themTemplateFCName2FC, string outputGDB)
        {
            Dictionary<IFeatureClass, IFeatureClass> result = new Dictionary<IFeatureClass, IFeatureClass>();

            IFeatureWorkspace templateFWS = templateWorkspace as IFeatureWorkspace;
            IFeatureWorkspace sourceFWS = sourceWorkspace as IFeatureWorkspace;

            //以底图模板数据库为基础，创建输出数据库
            if (System.IO.Directory.Exists(outputGDB))
            {
                System.IO.Directory.Delete(outputGDB, true);
            }
            IWorkspace outputWorkspace = CopyTemplateDatabase(app, templateWorkspace, outputGDB);
            IFeatureWorkspace outputFWS = outputWorkspace as IFeatureWorkspace;
            bool bRedefineSRF = false;

            //遍历源数据库（将底图模板数据库中不存在的要素类追加至输出数据库中）
            IEnumDataset sourceEnumDataset = sourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            sourceEnumDataset.Reset();
            IDataset sourceDataset = null;
            while ((sourceDataset = sourceEnumDataset.Next()) != null)
            {
                if (sourceDataset is IFeatureDataset)//要素数据集
                {
                    if (!bRedefineSRF && (templateWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureDataset, sourceDataset.Name))
                    {
                        //重定义基于底图模板数据库复制而来的输出数据库的空间参考
                        ReDefineWorkspaceSpatialReference(outputWorkspace, (sourceDataset as IGeoDataset).SpatialReference);
                        bRedefineSRF = true;
                    }

                    IFeatureDataset outputFeatureDataset = null;

                    //输出数据库（模板数据库）中是否存在该数据集，则存在则获取该数据集，否则在输出数据库中也不创建该数据集
                    if ((outputWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureDataset, sourceDataset.Name))
                    {
                        outputFWS.OpenFeatureDataset(sourceDataset.Name);
                    }
                    

                    //遍历子要素类
                    IFeatureDataset sourceFeatureDataset = sourceDataset as IFeatureDataset;
                    IEnumDataset sourceEnumDatasetF = sourceFeatureDataset.Subsets;
                    sourceEnumDatasetF.Reset();
                    IDataset sourceDatasetF = null;
                    while ((sourceDatasetF = sourceEnumDatasetF.Next()) != null)
                    {
                        if (sourceDatasetF is IFeatureClass)//要素类
                        {
                            IFeatureClass outputFC = null;

                            //要素类是否为底图模板数据库中的要素类
                            if ((outputWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, sourceDatasetF.Name))
                            {
                                outputFC = outputFWS.OpenFeatureClass(sourceDatasetF.Name);
                            }
                            else if(themTemplateFCName2FC.ContainsKey(sourceDatasetF.Name))//判断是否为专题数据库中的要素类
                            {
                                //复制专题要素类
                                string featureDatasetName="";
                                if (outputFeatureDataset != null)
                                {
                                    featureDatasetName = outputFeatureDataset.Name;
                                }

                                outputFC = CopyFC2Database(app, themTemplateFCName2FC[sourceDatasetF.Name], outputWorkspace, featureDatasetName, (sourceDataset as IGeoDataset).SpatialReference);
                            }
                            else//非模板数据库中的要素类，追加
                            {
                                IFeatureClass fc = sourceDatasetF as IFeatureClass;

                                IFields fields = getFeatureClassFields(fc, (sourceDataset as IGeoDataset).SpatialReference);
                                esriFeatureType featureType = fc.FeatureType;
                                string shapeFieldName = fc.ShapeFieldName;

                                //创建新的要素类
                                if (outputFeatureDataset != null)
                                {
                                    outputFC = outputFeatureDataset.CreateFeatureClass(sourceDatasetF.Name, fields, null, null, featureType, shapeFieldName, "");
                                }
                                else
                                {
                                    outputFC = outputFWS.CreateFeatureClass(sourceDatasetF.Name, fields, null, null, featureType, shapeFieldName, "");
                                }
                            }

                            //增加源数据库要素类与输出数据库对应要素类的映射关系
                            result.Add(sourceDatasetF as IFeatureClass, outputFC);
                        }
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(sourceEnumDatasetF);
                }
                else if (sourceDataset is IFeatureClass)//要素类
                {
                    if (!bRedefineSRF && (templateWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, sourceDataset.Name))
                    {
                        //重定义基于底图模板数据库复制而来的输出数据库的空间参考
                        ReDefineWorkspaceSpatialReference(outputWorkspace, (sourceDataset as IGeoDataset).SpatialReference);
                        bRedefineSRF = true;
                    }

                    IFeatureClass outputFC = null;

                    //要素类是否为底图模板数据库中的要素类
                    if ((outputWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, sourceDataset.Name))
                    {
                        outputFC = outputFWS.OpenFeatureClass(sourceDataset.Name);
                    }
                    else if (themTemplateFCName2FC.ContainsKey(sourceDataset.Name))//判断是否为专题数据库中的要素类
                    {
                        //复制专题要素类
                        outputFC = CopyFC2Database(app, themTemplateFCName2FC[sourceDataset.Name], outputWorkspace, "", (sourceDataset as IGeoDataset).SpatialReference);
                    }
                    else//非模板数据库中的要素类，追加
                    {
                        IFeatureClass fc = sourceDataset as IFeatureClass;

                        IFields fields = getFeatureClassFields(fc, (sourceDataset as IGeoDataset).SpatialReference);
                        esriFeatureType featureType = fc.FeatureType;
                        string shapeFieldName = fc.ShapeFieldName;

                        //创建新的要素类
                        outputFC = outputFWS.CreateFeatureClass(sourceDataset.Name, fields, null, null, featureType, shapeFieldName, "");
                    }
                    
                    //增加源数据库要素类与输出数据库对应要素类的映射关系
                    result.Add(sourceDataset as IFeatureClass, outputFC);
                }
                else if (sourceDataset is IRasterDataset)//栅格数据集
                {
                    IRasterDataset rasterDataset = (sourceWorkspace as IRasterWorkspaceEx).OpenRasterDataset(sourceDataset.Name);

                    //复制栅格数据集到输出数据库
                    IRasterValue rasterValue = new RasterValueClass();
                    rasterValue.RasterDataset = rasterDataset;
                    (outputWorkspace as IRasterWorkspaceEx).SaveAsRasterDataset(sourceDataset.Name, rasterDataset.CreateDefaultRaster(), rasterValue.RasterStorageDef, "", null, null);
                }
                else if (sourceDataset is IRasterCatalog)
                {
                    //暂不支持
                }
                else if (sourceDataset is IMosaicDataset)
                {
                    //暂不支持
                }
                else
                {
                    //暂不支持
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(sourceEnumDataset);

            return result;
        }

        /// <summary>
        /// 拷贝要素
        /// </summary>
        /// <param name="originFeatureClass"></param>
        /// <param name="qf"></param>
        /// <param name="targetFeatureClass"></param>
        /// <param name="newFieldName1"></param>
        /// <returns>返回同一要素在两个要素类中的OID映射关系</returns>
        public static Dictionary<int,int> CopyFeatures(IFeatureClass originFeatureClass, IQueryFilter qf, IFeatureClass targetFeatureClass, string newFieldName1 = "ATTACH")
        {
            Dictionary<int, int> sourceOID2TargetOID = new Dictionary<int, int>();

            IFeatureClassLoad pFCLoad = targetFeatureClass as IFeatureClassLoad;
            pFCLoad.LoadOnlyMode = true;
            try
            {
                //增加附区标识字段
                AddStringField(targetFeatureClass, newFieldName1);

                //在拷贝数据前，重新定义要素类的投影
                ISpatialReference sourceRef = (originFeatureClass as IGeoDataset).SpatialReference;
                ISpatialReference targetRef = (targetFeatureClass as IGeoDataset).SpatialReference;
                IClone sourceRefClone = sourceRef as IClone;
                IClone targetRefClone = targetRef as IClone;
                if (!targetRefClone.IsEqual(sourceRefClone))
                {
                    IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = targetFeatureClass as IGeoDatasetSchemaEdit;
                    if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                    {
                        pGeoDatasetSchemaEdit.AlterSpatialReference(sourceRef);
                    }
                }

                //遍历赋值
                if (originFeatureClass.FeatureCount(qf) > 0)
                {
                    IFeatureCursor targetCursor = targetFeatureClass.Insert(true);

                    IFeatureCursor originFeCursor = originFeatureClass.Search(qf, false);
                    IFeature originFe = null;
                    while ((originFe = originFeCursor.NextFeature()) != null)
                    {
                        IFeatureBuffer newFeatureBuf = targetFeatureClass.CreateFeatureBuffer();

                        //几何赋值
                        newFeatureBuf.Shape = originFe.Shape;

                        //属性赋值
                        for (int i = 0; i < newFeatureBuf.Fields.FieldCount; i++)
                        {
                            IField field = newFeatureBuf.Fields.get_Field(i);

                            //过滤系统属性字段
                            if (field.Name == originFeatureClass.OIDFieldName || field.Name == originFeatureClass.ShapeFieldName ||
                                field == originFeatureClass.LengthField || field == originFeatureClass.AreaField)
                                continue;

                            if (field.Name.StartsWith("OVERRIDE"))
                                continue;

                            int index = originFe.Fields.FindField(field.Name);
                            if (index != -1 && field.Editable)
                            {
                                newFeatureBuf.set_Value(i, originFe.get_Value(index));
                            }

                        }

                        object fid = targetCursor.InsertFeature(newFeatureBuf);

                        //收集映射关系
                        sourceOID2TargetOID.Add(originFe.OID, (int)fid);


                        System.Runtime.InteropServices.Marshal.ReleaseComObject(originFe);
                    }
                    targetCursor.Flush();

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(originFeCursor);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(targetCursor);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                pFCLoad.LoadOnlyMode = false;
            }

            return sourceOID2TargetOID;
        }

        /// <summary>
        /// 添加字段
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="newFieldName"></param>
        public static void AddStringField(IFeatureClass fc, string newFieldName)
        {
            if (fc.FindField(newFieldName) != -1)
                return ;

            IField newField = new FieldClass();
            IFieldEdit newFieldEdit = (IFieldEdit)newField;
            newFieldEdit.Name_2 = newFieldName;
            newFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;

            IClass pTable = fc as IClass;
            pTable.AddField(newField);
        }

        /// <summary>
        /// 规则匹配
        /// </summary>
        /// <param name="app"></param>
        /// <param name="layer"></param>
        /// <param name="parent"></param>
        /// <param name="dtLayerRule"></param>
        /// <param name="lyrName2themName"></param>
        /// <param name="themName2DT"></param>
        public static void MatchLayer(GApplication app, ILayer layer, IGroupLayer parent, DataTable dtLayerRule,
            Dictionary<string, string> lyrName2themName, Dictionary<string, DataTable> themName2DT)
        {
            if (parent == null)
            {
                app.Workspace.Map.AddLayer(layer);
            }
            else
            {
                (parent as IGroupLayer).Add(layer);
            }

            if (layer is IGroupLayer)
            {
                var l = (layer as ICompositeLayer);

                List<ILayer> layers = new List<ILayer>();
                for (int i = 0; i < l.Count; i++)
                {
                    layers.Add(l.get_Layer(i));
                }
                (layer as IGroupLayer).Clear();
                foreach (var item in layers)
                {
                    MatchLayer(app, item, layer as IGroupLayer, dtLayerRule, lyrName2themName, themName2DT);
                }
            }
            else
            {
                #region
                try
                {
                    string name = ((layer as IDataLayer2).DataSourceName as IDatasetName).Name;
                    if (layer is IFeatureLayer)
                    {
                        if ((app.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name))
                        {
                            //指定图层数据源
                            IFeatureClass fc = (app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(name);
                            (layer as IFeatureLayer).FeatureClass = fc;

                            DataRow[] drArray = dtLayerRule.Select().Where(i => i["映射图层"].ToString().Trim() == name).ToArray();
                            if (drArray.Length != 0)
                            {
                                int ruleIDIndex = -1;
                                int invisibleRuleID = 0;

                                #region 获取图层的制图表达字段索引值
                                for (int i = 0; i < drArray.Length; i++)
                                {
                                    string whereClause = drArray[i]["定义查询"].ToString().Trim();
                                    string lyrMappingName = drArray[i]["映射图层"].ToString().Trim();

                                    #region 数据结构升级时，已进行了一次数据的映射（原图层和目标图层中都存在该数据，其中原图层中数据进行了不显示设置）；这里进行符号规则更新时，不再进行数据的映射
                                    //string lyrOrginName = drArray[i]["图层"].ToString().Trim();

                                    //IQueryFilter qf = new QueryFilterClass();
                                    //qf.WhereClause = whereClause.ToString();
                                    //if (lyrOrginName != lyrMappingName)//图层映射
                                    //{
                                    //    if ((app.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, lyrMappingName))
                                    //    {
                                    //        var sourceFC = (app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(lyrOrginName);
                                    //        CopyFeature(sourceFC, qf, fc);//仅从库数据到制图数据时，需拷贝
                                    //    }
                                    //}
                                    #endregion


                                    //获取该图层的制图表达规则字段索引
                                    if (ruleIDIndex == -1)
                                    {
                                        ruleIDIndex = fc.FindField(drArray[i]["RuleIDFeildName"].ToString().Trim());
                                    }

                                    //获取该图层的不显示规则ID值
                                    if (invisibleRuleID == 0 && drArray[i]["RuleName"].ToString() == "不显示要素" )
                                    {
                                        invisibleRuleID = CommonMethods.GetRuleIDByRuleName(lyrMappingName, "不显示要素");
                                    }

                                }

                                if (ruleIDIndex == -1 && (layer as IGeoFeatureLayer).Renderer is IRepresentationRenderer)
                                {
                                    IRepresentationRenderer reprenderer = (layer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                                    if(reprenderer != null)
                                    {
                                        IRepresentationClass repClass = reprenderer.RepresentationClass;
                                        if (repClass != null)
                                            ruleIDIndex = repClass.RuleIDFieldIndex;
                                    }
                                }
                                #endregion

                                #region 更新制图表达规则

                                #region 1.初始化该图层数据的制图表达规则为空
                                if (ruleIDIndex != -1)
                                {
                                    IFeatureCursor feCursor = fc.Update(null, true);
                                    IFeature fe = null;
                                    while ((fe = feCursor.NextFeature()) != null)
                                    {
                                        fe.set_Value(ruleIDIndex, DBNull.Value);
                                        feCursor.UpdateFeature(fe);

                                        Marshal.ReleaseComObject(fe);//内存释放
                                    }
                                    Marshal.ReleaseComObject(feCursor);
                                }
                                else
                                {
                                    MessageBox.Show(string.Format("图层【{0}】中没有找到相关的制图表达规则字段，该图层的制图表达规则更新失败！", layer.Name));
                                    return;
                                }
                                #endregion

                                for (int i = 0; i < drArray.Length; i++)
                                {
                                    string whereClause = drArray[i]["定义查询"].ToString().Trim();
                                    string lyrMappingName = drArray[i]["映射图层"].ToString().Trim();
                                    string ruleName = drArray[i]["RuleName"].ToString().Trim();

                                    int ruleID = CommonMethods.GetRuleIDByRuleName(lyrMappingName, ruleName);
                                    if (ruleID == -1)
                                    {
                                        int.TryParse(drArray[i]["RuleID"].ToString().Trim(), out ruleID);
                                    }
                                    if (ruleID == -1 || ruleID == 0)
                                        continue;//无效的规则ID属性值
                                    if (ruleID == invisibleRuleID)
                                        continue;//不显示要素

                                    #region 2.更新制图表达规则属性值
                                    IQueryFilter qf = new QueryFilterClass();
                                    qf.WhereClause = whereClause.ToString();
                                    IFeatureCursor fCursor = fc.Update(qf, true);
                                    IFeature f = null;
                                    while ((f = fCursor.NextFeature()) != null)
                                    {
                                        f.set_Value(ruleIDIndex, ruleID);
                                        fCursor.UpdateFeature(f);

                                        Marshal.ReleaseComObject(f);
                                    }
                                    Marshal.ReleaseComObject(fCursor);
                                    #endregion
                                }

                                #region 3.将剩余要素设置为不显示要素
                                if (invisibleRuleID != 0)
                                {
                                    IQueryFilter qf = new QueryFilterClass();
                                    qf.WhereClause = string.Format("{0} is null", fc.Fields.get_Field(ruleIDIndex).Name);
                                    IFeatureCursor feCursor = fc.Update(qf, true);
                                    IFeature fe = null;
                                    while ((fe = feCursor.NextFeature()) != null)
                                    {
                                        fe.set_Value(ruleIDIndex, invisibleRuleID);
                                        feCursor.UpdateFeature(fe);

                                        Marshal.ReleaseComObject(fe);
                                    }
                                    Marshal.ReleaseComObject(feCursor);
                                }
                                #endregion
                                #endregion

                            }
                            else
                            {
                                if (lyrName2themName.ContainsKey(name)) //判断是否是专题图层
                                {
                                    var thematicName = lyrName2themName[name];
                                    var dt = themName2DT[thematicName];
                                    var drs = dt.Select().Where(i => i["图层"].ToString().Trim() == name).ToArray();
                                    if (drs.Length != 0)
                                    {
                                        int ruleIDIndex = -1;

                                        #region 获取图层的制图表达字段索引值
                                        for (int i = 0; i < drs.Length; i++)
                                        {
                                            object ruleID = drs[i]["RuleID"];
                                            object whereClause = drs[i]["定义查询"];


                                            //获取该图层的制图表达规则字段索引
                                            if (ruleIDIndex == -1)
                                            {
                                                ruleIDIndex = fc.FindField(drs[i]["RuleIDFeildName"].ToString().Trim());
                                                break;
                                            }

                                        }
                                        if (ruleIDIndex == -1 && (layer as IGeoFeatureLayer).Renderer is IRepresentationRenderer)
                                        {
                                            IRepresentationRenderer reprenderer = (layer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                                            if (reprenderer != null)
                                            {
                                                IRepresentationClass repClass = reprenderer.RepresentationClass;
                                                if (repClass != null)
                                                    ruleIDIndex = repClass.RuleIDFieldIndex;
                                            }
                                            
                                        }
                                        #endregion

                                        #region 更新制图表达规则属性值
                                        #region 1.初始化该图层所有数据的制图表达规则为空
                                        if (ruleIDIndex != -1)
                                        {
                                            IFeatureCursor feCursor = fc.Update(null, true);
                                            IFeature fe = null;
                                            while ((fe = feCursor.NextFeature()) != null)
                                            {
                                                fe.set_Value(ruleIDIndex, DBNull.Value);
                                                feCursor.UpdateFeature(fe);

                                                Marshal.ReleaseComObject(fe);//内存释放
                                            }
                                            Marshal.ReleaseComObject(feCursor);
                                        }
                                        else
                                        {
                                            MessageBox.Show(string.Format("图层【{0}】中没有找到相关的制图表达规则字段，该图层的制图表达规则更新失败！", layer.Name));
                                            return;
                                        }
                                        #endregion

                                        for (int i = 0; i < drs.Length; i++)
                                        {
                                            string whereClause = drs[i]["定义查询"].ToString().Trim();
                                            string ruleName = drs[i]["RuleName"].ToString().Trim();

                                            int ruleID = CommonMethods.GetRuleIDByRuleName(name, ruleName);
                                            if (ruleID == -1)
                                            {
                                                int.TryParse(drs[i]["RuleID"].ToString().Trim(), out ruleID);
                                            }
                                            if (ruleID == -1 || ruleID == 0)
                                                continue;//无效的规则ID属性值

                                            #region 2.更新制图表达规则属性值
                                            IQueryFilter qf = new QueryFilterClass();
                                            qf.WhereClause = whereClause.ToString();
                                            IFeatureCursor fCursor = fc.Update(qf, true);
                                            IFeature f = null;
                                            while ((f = fCursor.NextFeature()) != null)
                                            {
                                                f.set_Value(ruleIDIndex, ruleID);
                                                fCursor.UpdateFeature(f);

                                                Marshal.ReleaseComObject(f);
                                            }
                                            Marshal.ReleaseComObject(fCursor);
                                            #endregion
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                    else if (layer is IRasterLayer)
                    {
                        if ((app.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTRasterDataset, name))
                        {
                            (layer as IRasterLayer).CreateFromRaster((app.Workspace.EsriWorkspace as IRasterWorkspaceEx).OpenRasterDataset(name).CreateDefaultRaster());
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                #endregion
            }
        }


        /// <summary>
        /// 添加专题注记规则
        /// </summary>
        /// <param name="app"></param>
        /// <param name="baseRuleTable"></param>
        public static void AddThematicAnnoRule(GApplication app, DataTable baseRuleTable)
        {
            Dictionary<string, string> envString = app.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            bool themFlag = false;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            if (envString.ContainsKey("ThemExist"))
                themFlag = bool.Parse(envString["ThemExist"]);
            if (!themFlag)
                return;
            string dirName = envString["ThemDataBase"];
            string dirpath = GApplication.Application.Template.Root + "\\专题\\";
            {
                string mdbpath = dirpath + dirName + "\\规则对照.mdb";
                if (File.Exists(mdbpath))
                {
                    DataTable ruleTable = CommonMethods.ReadToDataTable(mdbpath, "注记规则");
                    //图层名
                    for (int i = 0; i < ruleTable.Rows.Count; i++)
                    {
                        string annoLayerName = ruleTable.Rows[i]["图层"].ToString().Trim();
                        var lyr = app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == annoLayerName);
                        })).FirstOrDefault() as IFeatureLayer;
                        if (lyr == null)
                            continue;
                        //存在
                        if ((app.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, annoLayerName))
                        {
                            baseRuleTable.Rows.Add(ruleTable.Rows[i].ItemArray);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 返回要素类中满足指定条件的要素OID集合
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="condition"></param>
        /// <param name="classID"></param>
        /// <returns></returns>
        public static List<int> GetFIDListInFC(IFeatureClass fc, string condition, out int classID)
        {
            classID = -1;
            List<int> featureIDList = new List<int>();
            if (fc == null)
            {
                return featureIDList;
            }

            try
            {
                classID = fc.ObjectClassID;

                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = condition.Replace("[", "").Replace("]", "");//替换掉中括号，兼容mdb和gdb
                if (queryFilter.WhereClause == "")
                {
                    queryFilter = null;
                }

                if (fc.FeatureCount(queryFilter) > 0)
                {
                    IFeatureCursor feCursor = fc.Search(queryFilter, false);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        featureIDList.Add(fe.OID);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return featureIDList;
        }

        /// <summary>
        /// 根据注记规则里的CMYK字符串得到CMYK颜色值
        /// </summary>
        /// <param name="cmyk">cmyk字符串（形如：C100M200Y100K50）</param>
        /// <returns>CMYK颜色值</returns>
        public static ICmykColor GetmykColor(string cmyk)
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

        /// <summary>
        /// 更新已存在注记的字体信息
        /// </summary>
        /// <param name="app"></param>
        /// <param name="annoName2FeatureClass"></param>
        /// <param name="annoRuleTable"></param>
        /// <param name="fontmappingTable"></param>
        /// <param name="fcObjectClassIDMapping"></param>
        /// <param name="sourceFCID2FIDMapping"></param>
        public static void updateAnnoFont(GApplication app, Dictionary<string, IFeatureClass> annoName2FeatureClass, DataTable annoRuleTable, DataTable fontmappingTable,
            Dictionary<int, int> fcObjectClassIDMapping, Dictionary<int, Dictionary<int, int>> sourceFCID2FIDMapping)
        {

            try
            {
                //要素类的ObjectClassID->要素类
                Dictionary<int, IFeatureClass> fcID2FC=new Dictionary<int,IFeatureClass>();
                var lyrs = app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer;
                })).ToArray();
                for (int i = 0; i < lyrs.Length; i++)
                {
                    IFeatureClass fc = (lyrs[i] as IFeatureLayer).FeatureClass;
                    if (fc == null)
                        continue;

                    if ((fc as IDataset).Workspace.PathName != app.Workspace.EsriWorkspace.PathName)
                        continue;

                    if(!fcID2FC.ContainsKey(fc.ObjectClassID))
                        fcID2FC.Add(fc.ObjectClassID, fc);

                }

                foreach (var kv in annoName2FeatureClass)
                {
                    IFeatureClass annoFC = kv.Value;
                    int typeIndex = annoFC.FindField("分类");

                    #region 收集该注记内的所有注记信息，并更新注记的关联信息
                    List<AnnoInfo> annoInfoList = new List<AnnoInfo>();
                    IFeatureCursor annoFeCursor = annoFC.Search(null, false);
                    IFeature f;
                    while ((f = annoFeCursor.NextFeature()) != null)
                    {
                        IAnnotationFeature2 annoFe = f as IAnnotationFeature2;

                        int linkedFID = annoFe.LinkedFeatureID;
                        int annoClassID = annoFe.AnnotationClassID;   
                        if (sourceFCID2FIDMapping.ContainsKey(annoClassID))
                        {
                            if (sourceFCID2FIDMapping[annoClassID].ContainsKey(linkedFID))
                                linkedFID = sourceFCID2FIDMapping[annoClassID][linkedFID];
                            else
                                linkedFID = -1;//原数据库中该注记对应的要素已经不存在
                        }
                        if (fcObjectClassIDMapping.ContainsKey(annoClassID))
                        {
                            annoClassID = fcObjectClassIDMapping[annoClassID];
                        }

                        //更新注记关联信息
                        annoFe.AnnotationClassID = annoClassID;
                        annoFe.LinkedFeatureID = linkedFID;
                        if (fcID2FC.ContainsKey(annoClassID) && linkedFID != -1)
                        {
                            #region 注记与关联要素的显示状态保持一致
                            IFeatureClass linkedFC = fcID2FC[annoClassID];
                            IFeature linkedFe = linkedFC.GetFeature(linkedFID);
                            int selStateIndex = linkedFC.FindField("selectstate");
                            if (annoFe.Status == esriAnnotationStatus.esriAnnoStatusPlaced)
                            {
                                //判断关联要素是否可见,从而决定该注记是否放置
                                if (selStateIndex != -1 && linkedFe.get_Value(selStateIndex) != DBNull.Value)//未选取
                                {
                                    annoFe.Status = esriAnnotationStatus.esriAnnoStatusUnplaced;
                                }
                            }
                            else//注记不显示
                            {
                                if (linkedFC.AliasName.ToUpper() == "RESP" || linkedFC.AliasName.ToUpper() == "AGNP")//与注记生成代码保持一致
                                {
                                    if (selStateIndex != -1)
                                    {
                                        linkedFe.set_Value(selStateIndex, "关联注记未选取");
                                        linkedFe.Store();
                                    }
                                }
                            }
                            #endregion
                        }
                        f.Store();


                        //收集注记信息
                        AnnoInfo ai = new AnnoInfo();
                        ai.AnnoID = f.OID;
                        ai.ReFeID = linkedFID;
                        ai.ClassID = annoClassID;
                        if (typeIndex != -1)
                            ai.AnnoType = f.get_Value(typeIndex).ToString();
                        ai.Updated = false;

                        annoInfoList.Add(ai);
                    }
                    Marshal.ReleaseComObject(annoFeCursor);

                    #endregion

                    #region 更新注记
                    DataRow[] drArray = annoRuleTable.Select().Where(l => l["注记图层"].ToString().Trim() == kv.Key).ToArray();
                    if (drArray.Length == 0)
                        continue;

                    string unUpdateAnnoStr= "";
                    for (int i = 0; i < drArray.Length; i++)
                    {
                        DataRow dr = drArray[i];
                        for (int j = 0; j < dr.Table.Columns.Count; j++)
                        {
                            object val = dr[j];
                            if (val == null || Convert.IsDBNull(val))
                                dr[j] = "";
                        }

                        string fcName = dr["图层"].ToString().Trim();
                        string condition = dr["查询条件"].ToString();
                        string fontName = dr["注记字体"].ToString();
                        string fontSize = dr["注记大小"].ToString();
                        string fontColor = dr["注记颜色"].ToString();
                        string fontType = dr["注记说明"].ToString();

                        #region 字体映射
                        for (int j = 0; j < fontmappingTable.Rows.Count; j++)
                        {
                            DataRow row = fontmappingTable.Rows[j];
                            if (row["国标字体"].ToString() == fontName)
                            {
                                fontName = row["替换字体"].ToString();
                                break;
                            }
                        }
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
                            fontName = "宋体";//默认的话给宋体
                        }
                        #endregion

                        IFeatureClass fc = null;
                        if ((app.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
                        {
                            fc = (app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(fcName);
                        }
                        if (fc == null)
                            continue;

                        int classID = -1;
                        List<int> featureIDs = GetFIDListInFC(fc, condition, out classID);
                        if (featureIDs.Count == 0)
                            continue;

                        foreach (var fid in featureIDs)
                        {
                            List<AnnoInfo> infos;
                            if (typeIndex != -1)
                            {
                                infos = annoInfoList.Where(t => t.ClassID == classID && t.ReFeID == fid && t.AnnoType == fontType).ToList();
                            }
                            else
                            {
                                infos = annoInfoList.Where(t => t.ClassID == classID && t.ReFeID == fid).ToList();
                            }

                            //更新注记
                            foreach (var info in infos)
                            {
                                info.Updated = true;

                                IFeature fe = annoFC.GetFeature(info.AnnoID);
                                IAnnotationFeature2 annoFe = fe as IAnnotationFeature2;

                                ITextElement textElement = (annoFe.Annotation as IClone).Clone() as ITextElement;
                                ITextSymbol newTextSymbol = (textElement.Symbol as IClone).Clone() as ITextSymbol;

                                //字体
                                newTextSymbol.Font.Name = fontName;
                                //字大
                                double size = 0;
                                double.TryParse(fontSize, out size);
                                if (size > 0)
                                    newTextSymbol.Size = size * 2.8345;
                                //颜色
                                IColor clr = GetmykColor(fontColor);
                                newTextSymbol.Color = clr;

                                //更新注记
                                textElement.Symbol = newTextSymbol;//一定要写回去，否则没有效果
                                annoFe.Annotation = textElement as IElement;
                                fe.Store();

                            }
                        }
                    }
                    #endregion

                    #region 收集该注记图层中未更新的注记(测试用)
                    List<int> unUpdatedAnnoFIDList = new List<int>();
                    foreach (var ai in annoInfoList)
                    {
                        if (!ai.Updated)
                            unUpdatedAnnoFIDList.Add(ai.AnnoID);
                    }
                    if (unUpdatedAnnoFIDList.Count > 0)
                    {
                        unUpdateAnnoStr += string.Format("注记图层【{0}】中的如下要素没有被更新：", kv.Key);
                        for (int i = 0; i < unUpdatedAnnoFIDList.Count; ++i)
                        {
                            if (i == 0)
                            {
                                unUpdateAnnoStr += unUpdatedAnnoFIDList[i].ToString();
                            }
                            else
                            {
                                unUpdateAnnoStr += string.Format(",{0}", unUpdatedAnnoFIDList[i]);
                            }
                        }
                        unUpdateAnnoStr += "\n";
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }

    class AnnoInfo
    {
        public int AnnoID
        {
            set;
            get;
        }
        public int ReFeID
        {
            set;
            get;
        }
        public int ClassID
        {
            set;
            get;
        }
        public string AnnoType
        {
            set;
            get;
        }
        public bool Updated
        {
            set;
            get;
        }
    }
}

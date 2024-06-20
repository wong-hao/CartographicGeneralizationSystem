using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Maplex;
using System.Runtime.InteropServices;
using System.IO;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesRaster;
using System.Linq;
using ESRI.ArcGIS.ConversionTools;
namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 数据库结构升级
    /// 2020@LZ
    /// </summary>
    [SMGIAutomaticCommand]
    public class DataBaseStructUpgradeCmd : SMGICommand
    {
        public DataBaseStructUpgradeCmd()
        {
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
            DataBaseStructUpgradeForm frm = new DataBaseStructUpgradeForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            bool res = false;
            using (var wo = m_Application.SetBusy())
            {
                //获取配置信息
                string mxdFullFileName = EnvironmentSettings.getMxdFullFileName(m_Application);
                string ruleMatchFileName = EnvironmentSettings.getLayerRuleDBFileName(m_Application);
                

                res = DataBaseStructUpgrade(frm.SourceGDBFile, frm.OutputGDBFile, frm.Mapscale,
                    frm.NeedAttachMap, mxdFullFileName, ruleMatchFileName, wo);
            }

            if (res)
            {
                MessageBox.Show("数据结构升级完成！");
            }
        }

        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            bool res = false;

            try
            {
                messageRaisedAction("正在解析地图符号化相关参数......");
                string sourceGDB = args.Element("SourceGDB").Value.Trim();
                string outputGDB = args.Element("OutPutGDB").Value.Trim();
                int scale = int.Parse(args.Element("MapScale").Value.Trim());
                bool needAttach = bool.Parse(args.Element("AttachMap").Value.Trim());
                string mxdFullFileName = EnvironmentSettings.getMxdFullFileName(m_Application);
                string ruleMatchFileName = EnvironmentSettings.getLayerRuleDBFileName(m_Application);
                //string mxdFullFileName = args.Element("mxdFullFileName").Value.Trim();
                //string ruleMatchFileName = args.Element("ruleMatchFileName").Value.Trim();

                messageRaisedAction("正在进行地图符号化......");
                res = DataBaseStructUpgrade(sourceGDB, outputGDB, scale, needAttach, mxdFullFileName, ruleMatchFileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }

            return res;
        }

        public static bool DataBaseStructUpgrade(string sourceGDB, string outputGDB, int scale, bool needAttach,
            string mxdFullFileName, string ruleMatchFileName, WaitOperation wo = null)
        {
            try
            {
                if (wo != null)
                    wo.SetText("正在读取规则配置信息......");

                #region 获取底图、专题规则配置信息
                //底图对照规则
                DataTable ruleMDB = CommonMethods.ReadToDataTable(ruleMatchFileName, "图层对照规则");
                if (ruleMDB.Rows.Count == 0)
                {
                    MessageBox.Show(string.Format("图层对照规则：【{0}】\\图层对照规则,没有找到或为空！", ruleMatchFileName));
                    return false;
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
                            string rulepath = GApplication.Application.Template.Root + "\\专题\\" + kv.Value + "\\规则对照.mdb";
                            DataTable dtLayerRule = CommonMethods.ReadToDataTable(rulepath, "图层对照规则");
                            themName2DT[kv.Value] = dtLayerRule;
                        }

                    }
                    #endregion
                }
                #endregion


                if (wo != null)
                    wo.SetText("正在进行数据结构升级...");
                #region 数据结构升级
                List<string> newFNList = new List<string>();
                newFNList.Add("ATTACH");//邻区标识字段
                newFNList.Add("SELECTSTATE");//选取状态标识字段
                if (!Upgrade(GApplication.Application, outputGDB, sourceGDB, ruleMDB, 
                    themTemplateFCName2themName, themTemplateFCName2FC, themName2DT, newFNList, false))
                {
                    return false;
                }

                //如果符号化后不区分主邻区，则将所有图层的Attach字段值为非空的要素赋值为空，且裁切面的几何赋值为纸张页面
                if (!needAttach)
                {
                    IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                    IWorkspace ws = wsFactory.OpenFromFile(outputGDB, 0);

                    List<string> fcNames = new List<string>();
                    List<string> fdtNames = new List<string>();

                    GApplication.Application.GetDatasetNames(ws, ref fcNames, ref fdtNames);
                    CommonMethods.NoMainRegionAndAdjRegion(ws as IFeatureWorkspace, fcNames);
                }
                #endregion

                if (wo != null)
                    wo.SetText("正在加载升级后的文件数据库......");

                #region 打开地理数据库，获取地图模板
                CommonMethods.OpenGDBFile(GApplication.Application, outputGDB);

                IMapDocument pMapDoc = new MapDocumentClass();
                pMapDoc.Open(mxdFullFileName, "");
                if (pMapDoc.MapCount == 0)//如果地图模板为空
                {
                    MessageBox.Show(string.Format("地图模板【{0}】不能为空！", mxdFullFileName));
                    return false;
                }
                IActiveView view = GApplication.Application.ActiveView;
                IMap map = view.FocusMap;
                IMap templateMap = pMapDoc.get_Map(0);
                string engineName = templateMap.AnnotationEngine.Name;
                if (engineName.Contains("Maplex"))
                {
                    IAnnotateMap sm = new MaplexAnnotateMapClass();
                    map.AnnotationEngine = sm;
                }
                else
                {
                    map.AnnotationEngine = templateMap.AnnotationEngine;
                }
                map.ReferenceScale = Convert.ToInt32(scale);
                templateMap.SpatialReference = map.SpatialReference;
                #endregion
                
                if (wo != null)
                    wo.SetText("正在匹配制图表达......");

                #region 重新匹配制图表达
                GApplication.Application.Workspace.LayerManager.Map.ClearLayers();
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
                MatchLayer(GApplication.Application, comlyr, null, ruleMDB, themTemplateFCName2themName, themName2DT);
                #endregion

                #region 重新匹配专题图层数据的制图表达
                layers.Clear();
                foreach (var kv in themTemplateFCName2themName)
                {
                    ILayer layer = null;
                    IFeatureClass fc = (GApplication.Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(kv.Key);
                    if (fc.FeatureType == esriFeatureType.esriFTAnnotation)
                    {
                        layer = new FDOGraphicsLayerClass();
                    }
                    else
                    {
                        layer = new FeatureLayerClass();
                    }

                    layer.Name = kv.Key;
                    (layer as IFeatureLayer).FeatureClass = fc;

                    if (fc.FindField("SELECTSTATE") != -1)
                    {
                        var fd = layer as ESRI.ArcGIS.Carto.IFeatureLayerDefinition;
                        string finitionExpression = fd.DefinitionExpression;
                        if (!finitionExpression.ToLower().Contains("SELECTSTATE"))
                        {
                            if (finitionExpression != "")
                            {
                                fd.DefinitionExpression = string.Format("({0}) and (SELECTSTATE IS NULL)", finitionExpression);
                            }
                            else
                            {
                                fd.DefinitionExpression = "SELECTSTATE IS NULL";
                            }
                        }
                    }

                    ILegendInfo legendInfo = layer as ILegendInfo;
                    if (legendInfo != null)
                    {
                        ILegendGroup lGroup;
                        for (int i = 0; i < legendInfo.LegendGroupCount; ++i)
                        {
                            lGroup = legendInfo.get_LegendGroup(i);
                            lGroup.Visible = false;//折叠
                        }
                    }

                    layers.Add(layer);
                }
                GApplication.Application.TOCControl.Update();

                if (layers.Count > 0)
                {
                    IGroupLayer thlyr = new GroupLayerClass();
                    thlyr.Name = "专题";
                    layers.Reverse();
                    //thlyr.Expanded = false;

                    foreach (var item in layers)
                    {
                        thlyr.Add(item);
                    }
                    MatchLayer(GApplication.Application, thlyr, null, ruleMDB, themTemplateFCName2themName, themName2DT);
                }
                #endregion

                #region 修改注记地图比例尺
                var lyrsAnno = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
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
                var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
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

                    if (fc.AliasName.ToUpper() == "CLIPBOUNDARY")
                    {
                        //获取纸张页面几何
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = "TYPE = '页面'";
                        var feCursor = fc.Search(qf, true);
                        var f = feCursor.NextFeature();
                        if (f != null)
                        {
                            IGeometry pageGeo = f.ShapeCopy;
                            GApplication.Application.Workspace.Map.ClipGeometry = pageGeo;
                        }
                        Marshal.ReleaseComObject(feCursor);
                    }
                }

                if (env != null && !env.IsEmpty)
                {
                    env.Expand(1.2, 1.2, true);
                    GApplication.Application.MapControl.Extent = env;
                    GApplication.Application.Workspace.Map.AreaOfInterest = env;

                    for (int i = 0; i < lyrs.Length; i++)
                    {
                        (lyrs[i] as ILayer2).AreaOfInterest = env;
                    }
                }
                #endregion

                #region 注记FID修改
                AnnoFIDSet(GApplication.Application.Workspace.EsriWorkspace, ruleMatchFileName, "库注记对照", wo);
                #endregion
                if (wo != null)
                    wo.SetText("正在保存工程......");
                
                //保存工程
                GApplication.Application.Workspace.Save();
                GC.Collect();

                //将环境配置信息写入econfig
                EnvironmentSettings.UpdateEnvironmentToConfig(needAttach);
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

        #region 数据库结构升级
        //复制模板数据库
        private static IWorkspace CopyTemplateDatabase(GApplication app, IWorkspace templateWorkspace, string outputGDB)
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
                msg += err.Message;
                MessageBox.Show(msg);
            }

            return ws;
        }

        private static void ReDefineWorkspaceSpatialReference(IWorkspace ws, ISpatialReference targetSpatialRef)
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

        /// <summary>
        /// 添加字段
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="newFieldName"></param>
        public static void AddStringField(IFeatureClass fc, string newFieldName)
        {
            if (fc.FindField(newFieldName) != -1)
                return;

            IField newField = new FieldClass();
            IFieldEdit newFieldEdit = (IFieldEdit)newField;
            newFieldEdit.Name_2 = newFieldName;
            newFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;

            IClass pTable = fc as IClass;
            pTable.AddField(newField);
        }

        /// <summary>
        /// 复制要素类（结构）到数据库
        /// </summary>
        /// <param name="app"></param>
        /// <param name="fc"></param>
        /// <param name="targetWorkspace"></param>
        /// <param name="featureDatasetName"></param>
        /// <param name="targetSpatialRef"></param>
        /// <returns></returns>
        private static IFeatureClass CopyFCStruct2Database(GApplication app, IFeatureClass fc, IWorkspace targetWorkspace, string featureDatasetName, ISpatialReference targetSpatialRef)
        {
            string msg = "";

            FeatureClassToFeatureClass gpTool = new FeatureClassToFeatureClass();
            gpTool.in_features = (fc as IDataset).Workspace.PathName + "\\" + fc.AliasName;
            gpTool.where_clause = "1<>1";
            gpTool.out_path = targetWorkspace.PathName;
            if (featureDatasetName != "")
            {
                gpTool.out_path += "\\" + featureDatasetName;
            }
            gpTool.out_name = fc.AliasName;

            IFeatureClass result = null;
            try
            {
                SMGI.Common.Helper.ExecuteGPTool(app.GPTool, gpTool, null);

                result = (targetWorkspace as IFeatureWorkspace).OpenFeatureClass(fc.AliasName);

                if (targetSpatialRef != null)
                {
                    #region 重定义空间参考
                    ISpatialReference spatialRef = (result as IGeoDataset).SpatialReference;
                    var targetSpatialRefClone = targetSpatialRef as IClone;
                    if (spatialRef == null || !targetSpatialRefClone.IsEqual(spatialRef as IClone))
                    {
                        IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = result as IGeoDatasetSchemaEdit;
                        if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                        {
                            pGeoDatasetSchemaEdit.AlterSpatialReference(targetSpatialRef);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception err)
            {
                msg += err.Message;
                MessageBox.Show(msg);
            }

            return result;
        }

        /// <summary>
        /// 复制要素类（结构+数据）到数据库
        /// </summary>
        /// <param name="app"></param>
        /// <param name="fc"></param>
        /// <param name="targetWorkspace"></param>
        /// <param name="featureDatasetName"></param>
        /// <param name="targetSpatialRef"></param>
        /// <returns></returns>
        private static IFeatureClass CopyFC2Database(GApplication app, IFeatureClass fc, IWorkspace targetWorkspace, string featureDatasetName, ISpatialReference targetSpatialRef)
        {
            string msg = "";

            Copy gpTool = new Copy();
            gpTool.in_data = (fc as IDataset).Workspace.PathName + "\\" + fc.AliasName;
            string out_data = targetWorkspace.PathName + "\\";
            if (featureDatasetName != "")
            {
                out_data += featureDatasetName + "\\";
            }
            gpTool.out_data = out_data + fc.AliasName;

            IFeatureClass result = null;
            try
            {
                SMGI.Common.Helper.ExecuteGPTool(app.GPTool, gpTool, null);

                result = (targetWorkspace as IFeatureWorkspace).OpenFeatureClass(fc.AliasName);

                if (targetSpatialRef != null)
                {
                    #region 重定义空间参考
                    ISpatialReference spatialRef = (result as IGeoDataset).SpatialReference;
                    var targetSpatialRefClone = targetSpatialRef as IClone;
                    if (spatialRef == null || !targetSpatialRefClone.IsEqual(spatialRef as IClone))
                    {
                        IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = result as IGeoDatasetSchemaEdit;
                        if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                        {
                            pGeoDatasetSchemaEdit.AlterSpatialReference(targetSpatialRef);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

            return result;

        }

        /// <summary>
        /// 拷贝要素
        /// </summary>
        /// <param name="originFeatureClass"></param>
        /// <param name="qf"></param>
        /// <param name="targetFeatureClass"></param>
        /// <param name="newFieldName1"></param>
        /// <returns></returns>
        private static void CopyFeatures(IFeatureClass originFeatureClass, IQueryFilter qf, IFeatureClass targetFeatureClass)
        {

            if (originFeatureClass.Extension as IAnnoClass != null)
            {
                CopyAnnoFeatures(originFeatureClass, qf, targetFeatureClass);
                return;
            }
            IFeatureClassLoad pFCLoad = targetFeatureClass as IFeatureClassLoad;
            pFCLoad.LoadOnlyMode = true;
            try
            {
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

                        #region 点线面要素
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
                                //if (originFe.get_Value(index) != DBNull.Value)
                                {
                                    string val = originFe.get_Value(index).ToString();
                                    newFeatureBuf.set_Value(i, originFe.get_Value(index));
                                }
                            }

                        }

                        object fid = targetCursor.InsertFeature(newFeatureBuf);


                        System.Runtime.InteropServices.Marshal.ReleaseComObject(originFe);
                        #endregion
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
        }
        private static void CopyAnnoFeatures(IFeatureClass originFeatureClass, IQueryFilter qf, IFeatureClass targetFeatureClass)
        {
            string[] annoFields = new string[] { "分类", "GUID" };
            IFeatureClassLoad pFCLoad = targetFeatureClass as IFeatureClassLoad;
            pFCLoad.LoadOnlyMode = true;
            try
            {
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

                        IAnnotationFeature2 annoFeature2 = newFeatureBuf as IAnnotationFeature2;
                        IGeometry geometryAnno = ((originFe as IAnnotationFeature2).Annotation as IElement).Geometry;
                        try
                        {
                            IElement annoEle = (((originFe as IAnnotationFeature2).Annotation as IElement) as IClone).Clone() as IElement;
                            annoEle.Geometry = geometryAnno;
                            annoFeature2.Annotation = annoEle;
                            annoFeature2.AnnotationClassID = (originFe as IAnnotationFeature2).AnnotationClassID;
                            annoFeature2.LinkedFeatureID = (originFe as IAnnotationFeature2).LinkedFeatureID;
                            annoFeature2.Status = (originFe as IAnnotationFeature2).Status;
                        }
                        catch
                        {
                        }
                        foreach (var field in annoFields)
                        {
                            int index = originFe.Class.FindField(field);
                            newFeatureBuf.set_Value(targetFeatureClass.FindField(field), originFe.get_Value(index));
                        }
                        object fid = targetCursor.InsertFeature(newFeatureBuf);
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
        }

        /// <summary>
        /// 获取要素类的字段结构信息
        /// </summary>
        /// <param name="sourceFeatureClass"></param>
        /// <param name="srf"></param>
        /// <param name="bFieldNameUpper"></param>
        /// <returns></returns>
        private static IFields getFeatureClassFields(IFeatureClass sourceFeatureClass, ISpatialReference srf, bool bFieldNameUpper = false)
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
        /// 返回工作空间中的所有要素类集合
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        private static Dictionary<string, IFeatureClass> getFeatureClassList(IWorkspace ws)
        {
            Dictionary<string, IFeatureClass> fcName2FC = new Dictionary<string, IFeatureClass>();

            if (null == ws)
                return fcName2FC;

            IEnumDataset enumDataset = ws.get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset is IFeatureDataset)//要素数据集
                {
                    IFeatureDataset featureDataset = dataset as IFeatureDataset;
                    IEnumDataset subEnumDataset = featureDataset.Subsets;
                    subEnumDataset.Reset();
                    IDataset subDataset = null;
                    while ((subDataset = subEnumDataset.Next()) != null)
                    {
                        if (subDataset is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = subDataset as IFeatureClass;
                            if (fc != null)
                                fcName2FC.Add(subDataset.Name.ToUpper(), fc);
                        }
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(subEnumDataset);
                }
                else if (dataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc = dataset as IFeatureClass;
                    if (fc != null)
                        fcName2FC.Add(dataset.Name.ToUpper(), fc);
                }
                else
                {

                }

            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);

            return fcName2FC;
        }

        /// <summary>
        /// 根据源数据库、相关模板信息(底图、专题)等，创建目标数据库结构
        /// 注意1.若源数据库包含栅格数据，则直接拷贝栅格数据到输出数据库；
        /// 注意2.若源数据库中的要素类在模板(底图、专题)中不存在，同时needAddNewFC=true，则直接拷贝该要素类的数据到输出数据库
        /// </summary>DataTable dtLayerRule
        /// <param name="app"></param>
        /// <param name="outputGDB">输出GDB路径</param>
        /// <param name="sourceWorkspace">源数据库</param>
        /// <param name="templateWorkspace">底图模板库</param>
        /// <param name="themTemplateFCName2FC">专题模板中的矢量要素类集合(空要素类)</param>
        /// <param name="needAddNewFC">是否需要将模板（底图+专题）中不存在的要素类添加至输出数据库中</param>
        /// <param name="inFCName2FC">输入数据库中所有的要素类集合</param>
        /// <param name="outFCName2FC">输出数据库中所有的要素类集合</param>
        /// <returns></returns>
        private static void CreateDBStructByTemplate(GApplication app, 
            string outputGDB,IWorkspace sourceWorkspace, IWorkspace templateWorkspace, 
            Dictionary<string, IFeatureClass> themTemplateFCName2FC, bool needAddNewFC,
            ref Dictionary<string, IFeatureClass> inFCName2FC, 
            ref Dictionary<string, IFeatureClass> outFCName2FC)
        {
            try
            {
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
                ISpatialReference sr = null;

                //获取当前输出数据库中的要素类信息
                outFCName2FC = getFeatureClassList(outputWorkspace);

                //遍历源数据库（将底图模板数据库中不存在的要素类视情况添加至输出数据库中）
                IEnumDataset sourceEnumDataset = sourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
                sourceEnumDataset.Reset();
                IDataset sourceDataset = null;
                while ((sourceDataset = sourceEnumDataset.Next()) != null)
                {
                    if (sourceDataset is IFeatureDataset)//要素数据集
                    {
                        if (sr == null)
                        {
                            sr = (sourceDataset as IGeoDataset).SpatialReference;
                        }

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
                            outputFeatureDataset = outputFWS.OpenFeatureDataset(sourceDataset.Name);
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

                                if (!(outputWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, sourceDatasetF.Name))//底图模板数据库中不存在该要素类
                                {
                                    if(themTemplateFCName2FC.ContainsKey(sourceDatasetF.Name))//判断是否为专题数据库中的要素类
                                    {
                                        #region 追加专题要素类（仅结构）
                                        IFeatureClass fc = themTemplateFCName2FC[sourceDatasetF.Name];//专题模板数据库中的要素类（空要素类）

                                        #region 方法1：CreateFeatureClass方法（制图表达丢失？？）
                                        //IFields fields = getFeatureClassFields(fc, (sourceDatasetF as IGeoDataset).SpatialReference);
                                        //esriFeatureType featureType = fc.FeatureType;
                                        //string shapeFieldName = fc.ShapeFieldName;

                                        ////创建新的要素类
                                        //if (outputFeatureDataset != null)
                                        //{
                                        //    outputFC = outputFeatureDataset.CreateFeatureClass(sourceDatasetF.Name, fields, null, null, featureType, shapeFieldName, "");
                                        //}
                                        //else
                                        //{
                                        //    outputFC = outputFWS.CreateFeatureClass(sourceDatasetF.Name, fields, null, null, featureType, shapeFieldName, "");
                                        //}
                                        #endregion

                                        #region 方法2：GP工具
                                        string featureDatasetName = "";
                                        if (outputFeatureDataset != null)
                                        {
                                            featureDatasetName = outputFeatureDataset.Name;
                                        }

                                        outputFC = CopyFCStruct2Database(app, fc, outputWorkspace, featureDatasetName, (sourceDatasetF as IGeoDataset).SpatialReference);
                                        #endregion

                                        #endregion


                                    }
                                    else//非模板数据库中的要素类
                                    {
                                        if(needAddNewFC)
                                        {
                                            #region 直接全部复制（包括数据）
                                            IFeatureClass fc = sourceDatasetF as IFeatureClass;

                                            string featureDatasetName = "";
                                            if (outputFeatureDataset != null)
                                            {
                                                featureDatasetName = outputFeatureDataset.Name;
                                            }

                                            outputFC = CopyFC2Database(app, fc, outputWorkspace, featureDatasetName, (sourceDatasetF as IGeoDataset).SpatialReference);
                                            #endregion
                                        }
                                    }
                                }

                                //添加输出数据库中的要素类
                                if(outputFC != null)
                                    outFCName2FC.Add(sourceDatasetF.Name.ToUpper(), outputFC);
                                //添加输入数据库中的要素类
                                inFCName2FC.Add(sourceDatasetF.Name.ToUpper(), sourceDatasetF as IFeatureClass);
                            }
                        }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(sourceEnumDatasetF);
                    }
                    else if (sourceDataset is IFeatureClass)//要素类
                    {
                        if (sr == null)
                        {
                            sr = (sourceDataset as IGeoDataset).SpatialReference;
                        }

                        if (!bRedefineSRF && (templateWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, sourceDataset.Name))
                        {
                            //重定义基于底图模板数据库复制而来的输出数据库的空间参考
                            ReDefineWorkspaceSpatialReference(outputWorkspace, (sourceDataset as IGeoDataset).SpatialReference);
                            bRedefineSRF = true;
                        }

                        IFeatureClass outputFC = null;

                        if (!(outputWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, sourceDataset.Name))//底图模板数据库中不存在该要素类
                        {
                            if(themTemplateFCName2FC.ContainsKey(sourceDataset.Name))//判断是否为专题数据库中的要素类
                            {
                                #region 追加专题要素类（仅结构）
                                IFeatureClass fc = themTemplateFCName2FC[sourceDataset.Name];//专题模板数据库中的要素类

                                #region 方法1：CreateFeatureClass方法（制图表达丢失？？）
                                //IFields fields = getFeatureClassFields(fc, (sourceDataset as IGeoDataset).SpatialReference);
                                //esriFeatureType featureType = fc.FeatureType;
                                //string shapeFieldName = fc.ShapeFieldName;

                                ////创建新的要素类
                                //outputFC = outputFWS.CreateFeatureClass(sourceDataset.Name, fields, null, null, featureType, shapeFieldName, "");
                                #endregion

                                #region 方法2：GP工具
                                outputFC = CopyFCStruct2Database(app, fc, outputWorkspace, "", (sourceDataset as IGeoDataset).SpatialReference);
                                #endregion

                                #endregion
                            }
                            else//非模板数据库中的要素类
                            {
                                if(needAddNewFC)
                                {
                                    #region 直接全部复制（包括数据）
                                    IFeatureClass fc = sourceDataset as IFeatureClass;

                                    outputFC = CopyFC2Database(app, fc, outputWorkspace, "", (sourceDataset as IGeoDataset).SpatialReference);
                                    #endregion
                                }
                            }

                        }

                        //添加输出数据库中的要素类
                        if (outputFC != null)
                            outFCName2FC.Add(sourceDataset.Name.ToUpper(), outputFC);
                        //添加输入数据库中的要素类
                        inFCName2FC.Add(sourceDataset.Name.ToUpper(), sourceDataset as IFeatureClass);
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

                //追加专题数据库模板中存在但输入数据库中不存在的要素类至输出数据库(输入数据库中没有、输出数据库模板中有)
                foreach (var kv in themTemplateFCName2FC)
                {
                    if (!(outputWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, kv.Key))
                    {
                        var outputFC = CopyFCStruct2Database(app, kv.Value, outputWorkspace, "", sr);//空间参考
                        outFCName2FC.Add(kv.Key.ToUpper(), outputFC);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        /// <summary>
        /// 数据结构升级(结构+数据)
        /// </summary>
        /// <param name="app"></param>
        /// <param name="outputFileGDB">输出数据库</param>
        /// <param name="sourceFileGDB">待升级的源数据库</param>
        /// <param name="themTemplateFCName2FC">模板库中的要素类集合</param>
        /// <param name="dtLayerRule">图层对照规则</param>
        /// <param name="lyrName2themName">图层名-专题名称</param>
        /// <param name="themName2DT">专题名称->图层规则表</param>
        /// <param name="needAddNewFC">是否需要将模板（底图+专题）中不存在的要素类添加至输出数据库中</param>
        /// <param name="newFieldName1">新增字段1</param>
        /// <param name="newFieldName2">新增字段2</param>
        /// <returns></returns>
        private static bool Upgrade(GApplication app, string outputFileGDB, string sourceFileGDB, DataTable dtLayerRule,
            Dictionary<string, string> themTemplateFCName2themName, Dictionary<string, IFeatureClass> themTemplateFCName2FC, 
            Dictionary<string, DataTable> themName2DT, List<string> newStringFNList, bool needAddNewFC = false)
        {
            bool res = true;
            try
            {
                if (!Directory.Exists(sourceFileGDB))
                {
                    MessageBox.Show("升级数据文件不存在！");
                    return false;
                }
                if (Directory.Exists(outputFileGDB))
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

                //创建输出数据库结构（栅格数据及新增的要素类数据全部直接复制）
                Dictionary<string, IFeatureClass> inFCName2FC = new Dictionary<string,IFeatureClass>();
                Dictionary<string, IFeatureClass> outFCName2FC = new Dictionary<string, IFeatureClass>();
                CreateDBStructByTemplate(app, outputFileGDB, sourceWorkspace, tempWorkspace, 
                    themTemplateFCName2FC, needAddNewFC, ref inFCName2FC, ref outFCName2FC);

                //为输出数据库要素类添加额外字段
                foreach (var item in outFCName2FC.Values)
                {
                    //增加文本字段
                    foreach (var fn in newStringFNList)
                    {
                        AddStringField(item, fn);
                    }
                }

                #region 拷贝底图数据
                for (int i = 0; i < dtLayerRule.Rows.Count; ++i)
                {
                    string inFCName = dtLayerRule.Rows[i]["图层"].ToString().Trim().ToUpper();
                    string outFCName = dtLayerRule.Rows[i]["映射图层"].ToString().Trim().ToUpper();
                    string whereClause = dtLayerRule.Rows[i]["定义查询"].ToString().Trim();
                    Console.WriteLine(inFCName +":"+ whereClause);
                    if (!inFCName2FC.ContainsKey(inFCName))
                    {
                        continue;
                    }
                    if (!outFCName2FC.ContainsKey(outFCName))
                    {
                        MessageBox.Show(string.Format("输出数据库中不包含要素类【{0}】！", outFCName));
                        return false;
                    }
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = whereClause;

                    //拷贝数据
                    int count = 0;
                    try
                    {
                        count = inFCName2FC[inFCName].FeatureCount(qf);//验证SQL语句会是否正确
                    }
                    catch (Exception ex1)
                    {
                        throw new Exception(string.Format("规则表中图层【{0}】的定义查询语句【{1}】不合法：{2}", inFCName, whereClause, ex1.Message));
                    }

                    if (count > 0)
                    {
                        CopyFeatures(inFCName2FC[inFCName], qf, outFCName2FC[outFCName]);
                    }
                }
                #endregion

                #region 拷贝专题数据
                foreach (var kv in inFCName2FC)
                {
                    string fcName = kv.Key;
                    IFeatureClass inFC = kv.Value;
                    if (themTemplateFCName2themName.ContainsKey(fcName))
                    {
                        var themName = themTemplateFCName2themName[fcName];
                        var dt = themName2DT[themName];
                        var drs = dt.Select().Where(i => i["图层"].ToString().Trim() == fcName).ToArray();
                        for (int i = 0; i < drs.Length; i++)
                        {
                            string outFCName = drs[i]["映射图层"].ToString().Trim();
                            string whereClause = drs[i]["定义查询"].ToString().Trim();

                            if (!outFCName2FC.ContainsKey(outFCName))
                            {
                                MessageBox.Show(string.Format("输出数据库中不包含要素类【{0}】！", outFCName));
                                return false;
                            }
                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = whereClause;

                            //拷贝数据
                            int count = 0;
                            try
                            {
                                count = inFC.FeatureCount(qf);//验证SQL语句会是否正确
                            }
                            catch (Exception ex1)
                            {
                                throw new Exception(string.Format("规则表中图层【{0}】的定义查询语句【{1}】不合法：{2}", fcName, whereClause, ex1.Message));
                            }

                            if (count > 0)
                            {
                                CopyFeatures(inFC, qf, outFCName2FC[outFCName]);
                            }
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                res = false;
            }

            return res;
        }
        #endregion

        #region 规则匹配
        /// <summary>
        /// 规则匹配
        /// </summary>
        /// <param name="app"></param>
        /// <param name="layer"></param>
        /// <param name="parent"></param>
        /// <param name="dtLayerRule"></param>
        /// <param name="lyrName2themName"></param>
        /// <param name="themName2DT"></param>
        private static void MatchLayer(GApplication app, ILayer layer, IGroupLayer parent, DataTable dtLayerRule,
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
                            try
                            {
                                //指定图层数据源
                                IFeatureClass fc = (app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(name);
                                (layer as IFeatureLayer).FeatureClass = fc;
                                if (fc.Extension is IAnnoClass)
                                    return;
                                DataRow[] drArray = dtLayerRule.Select().Where(i => i["映射图层"].ToString().Trim() == name).ToArray();
                                if (drArray.Length != 0)
                                {
                                    int selectStateIndex = fc.FindField("SELECTSTATE");

                                    int ruleIDIndex = -1;
                                    int invisibleRuleID = 0;
                                    #region 获取图层的制图表达字段索引值
                                    for (int i = 0; i < drArray.Length; i++)
                                    {
                                        //获取该图层的制图表达规则字段索引
                                        if (ruleIDIndex == -1)
                                        {
                                            ruleIDIndex = fc.FindField(drArray[i]["RuleIDFeildName"].ToString().Trim());
                                        }

                                        //获取该图层的不显示规则ID值
                                        if (invisibleRuleID == 0 && drArray[i]["RuleName"].ToString() == "不显示要素")
                                        {
                                            invisibleRuleID = CommonMethods.GetRuleIDByRuleName(name, "不显示要素");
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

                                    #region 更新制图表达规则
                                    for (int i = 0; i < drArray.Length; i++)
                                    {
                                        string whereClause = drArray[i]["定义查询"].ToString().Trim();
                                        string ruleName = drArray[i]["RuleName"].ToString().Trim();
                                        bool isSelect = (drArray[i]["选取状态"].ToString().Trim() != "否");

                                        int ruleID = CommonMethods.GetRuleIDByRuleName(name, ruleName);
                                        if (ruleID == -1)
                                        {
                                            int.TryParse(drArray[i]["RuleID"].ToString().Trim(), out ruleID);
                                        }
                                        if (ruleID == -1 || ruleID == 0)
                                            continue;//无效的规则ID属性值

                                        IQueryFilter qf = new QueryFilterClass();
                                        qf.WhereClause = whereClause.ToString();
                                        IFeatureCursor fCursor = fc.Update(qf, true);
                                        IFeature f = null;
                                        while ((f = fCursor.NextFeature()) != null)
                                        {
                                            f.set_Value(ruleIDIndex, ruleID);
                                            if (selectStateIndex != -1)
                                            {
                                                if (!isSelect)
                                                    f.set_Value(selectStateIndex, "不选取");
                                            }
                                            fCursor.UpdateFeature(f);

                                            Marshal.ReleaseComObject(f);
                                        }
                                        Marshal.ReleaseComObject(fCursor);
                                    }

                                    #region 将图层剩余要素设置为不显示要素
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
                                        drArray = dt.Select().Where(i => i["映射图层"].ToString().Trim() == name).ToArray();
                                        if (drArray.Length != 0)
                                        {
                                            int selectStateIndex = fc.FindField("SELECTSTATE");

                                            int ruleIDIndex = -1;
                                            int invisibleRuleID = 0;

                                            #region 获取图层的制图表达字段索引值
                                            for (int i = 0; i < drArray.Length; i++)
                                            {
                                                //获取该图层的制图表达规则字段索引
                                                if (ruleIDIndex == -1)
                                                {
                                                    ruleIDIndex = fc.FindField(drArray[i]["RuleIDFeildName"].ToString().Trim());
                                                    break;
                                                }

                                                //获取该图层的不显示规则ID值
                                                if (invisibleRuleID == 0 && drArray[i]["RuleName"].ToString() == "不显示要素")
                                                {
                                                    invisibleRuleID = CommonMethods.GetRuleIDByRuleName(name, "不显示要素");
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
                                            for (int i = 0; i < drArray.Length; i++)
                                            {
                                                string whereClause = drArray[i]["定义查询"].ToString().Trim();
                                                string ruleName = drArray[i]["RuleName"].ToString().Trim();
                                                bool isSelect = (drArray[i]["选取状态"].ToString().Trim() != "否");

                                                int ruleID = CommonMethods.GetRuleIDByRuleName(name, ruleName);
                                                if (ruleID == -1)
                                                {
                                                    int.TryParse(drArray[i]["RuleID"].ToString().Trim(), out ruleID);
                                                }
                                                if (ruleID == -1 || ruleID == 0)
                                                    continue;//无效的规则ID属性值

                                                IQueryFilter qf = new QueryFilterClass();
                                                qf.WhereClause = whereClause.ToString();
                                                IFeatureCursor fCursor = fc.Update(qf, true);
                                                IFeature f = null;
                                                while ((f = fCursor.NextFeature()) != null)
                                                {
                                                    f.set_Value(ruleIDIndex, ruleID);
                                                    if (selectStateIndex != -1)
                                                    {
                                                        if (!isSelect)
                                                            f.set_Value(selectStateIndex, "不选取");
                                                    }
                                                    fCursor.UpdateFeature(f);

                                                    Marshal.ReleaseComObject(f);
                                                }
                                                Marshal.ReleaseComObject(fCursor);
                                            }

                                            #region 将图层剩余要素设置为不显示要素
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
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                throw ex;
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
        #endregion
        #region 注记FID修改
        
        public static bool AnnoFIDSet(IWorkspace ws, string ruleMatchFileName, string AnnoTableName = "库注记对照", WaitOperation wo = null)
        {
            try
            {    
                DataTable ruleTable = Helper.ReadToDataTable(ruleMatchFileName, AnnoTableName);
                if (ruleTable.Rows.Count == 0)
                    return true;

                DataTable dtAnno= ruleTable.DefaultView.ToTable(true, "注记图层");
                for (int i = 0; i < dtAnno.Rows.Count; i++)
                {
                    DataRow dr = dtAnno.Rows[i];
                    string lyr = dr["注记图层"].ToString();
                    if (!(ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, lyr))
                        continue;
                    if (wo != null)
                    {
                        wo.SetText("正在更新："+lyr);
                    }

                    var drs = ruleTable.Select().Where(t => t["注记图层"].ToString().Trim() == lyr).ToArray();
                    Dictionary<string, string> guidsDic = new Dictionary<string, string>();
                    foreach (DataRow item in drs)
                    {
                        string fclname = item["要素图层"].ToString();
                        if (!(ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fclname))
                            continue;
                        IFeatureClass fcl = (ws as IFeatureWorkspace).OpenFeatureClass(fclname);
                        IFeature fe;
                        IFeatureCursor cursor = fcl.Search(null, false);
                        int index = fcl.FindField("GUID");
                        int classID = fcl.ObjectClassID;

                        while ((fe = cursor.NextFeature()) != null)
                        {
                            guidsDic[fe.get_Value(index).ToString()] = fe.OID + "_" + classID;
                        }
                        Marshal.ReleaseComObject(cursor);
                    }
                    //update anno
                    {
                        IFeatureClass fcl = (ws as IFeatureWorkspace).OpenFeatureClass(lyr);
                        IFeature fe;
                        IFeatureCursor cursor = fcl.Update(null, false);
                        int index = fcl.FindField("GUID");
                       
                        while ((fe = cursor.NextFeature()) != null)
                        {
                            string guid = fe.get_Value(index).ToString();
                            if (!guidsDic.ContainsKey(guid))
                                continue;
                            string[] vals = guidsDic[guid].Split(new char[] {'_'},  StringSplitOptions.RemoveEmptyEntries);
                            IAnnotationFeature2 annoFeature = fe as IAnnotationFeature2;
                            annoFeature.AnnotationClassID =int.Parse( vals[1]);
                            annoFeature.LinkedFeatureID = int.Parse(vals[0]);
                            cursor.UpdateFeature(fe);
                               
                        }
                        Marshal.ReleaseComObject(cursor);
                    }


                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("注记FID修改失败:"+ex.Message);
                return false;
            }
            return true;
        }
        #endregion
    }
}

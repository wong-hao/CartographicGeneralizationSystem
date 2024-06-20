using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 数据服务器(矢量):2017-11-22：保持边界线要素完整性
    /// </summary>
    public class DataServerClass
    {
        private GApplication _app;//应用程序
        private string _ipAddress;//服务器IP
        private string _userName;//用户名
        private string _passWord;//密码
        private string _databaseName;//数据库名

        public DataServerClass(GApplication app, string ipAddress, string userName, string passWord, string databaseName)
        {
            _app = app;

            _ipAddress = ipAddress;
            _userName = userName;
            _passWord = passWord;
            _databaseName = databaseName;
        }

        /// <summary>
        /// 单独裁切岛状图:[释放内存]
        /// </summary>
        /// <param name="clipGeo">裁切范围</param>
        /// <param name="outputGDB">输出的gdb</param>
        /// <param name="outputSpatialReference">输出gdb的空间参考</param>
        /// <param name="fcNameList">获取的要素类名称，若为空则获取原数据库中全部要素类</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public bool DownLoadData(IGeometry clipGeo, string outputGDB, ISpatialReference outputSpatialReference, List<string> fcNameList = null, WaitOperation wo = null)
        {
            try
            {
                //获取源数据库中的所有要素类名称集合
                IWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                if (null == sdeWorkspace)
                {
                    MessageBox.Show("无法访问服务器！");
                    return false;
                }

                if (wo != null)
                    wo.SetText("正在创建数据库...");

                //创建一个相同结构的空数据库
                Dictionary<IFeatureClass, IFeatureClass> fcList = CreateGDBStruct(sdeWorkspace, outputGDB, fcNameList, outputSpatialReference);

                //插入要素
                foreach (var item in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在裁切要素类:{0}...", item.Value.AliasName));

                    clipGeo.Project((item.Key as IGeoDataset).SpatialReference);//投影变换(与输入要素类的空间参考保持一致)

                    copyIntersectFeatures(item.Key, clipGeo, item.Value);//将输入要素类中位于裁切几何体中的要素插入到输出要素类
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }


        /// <summary>
        ///2.存在附区裁切时处理【领区主区】：从裁切几何外包矩形裁切，2.判断要素位于主区，还是附区,3.保证裁切面边缘的要素不被裁切断
        /// 增加内存释放控制
        /// </summary>
        public bool DownExtensionLoadData(IGeometry clipGeo, IGeometry pageGeo, string outputGDB, ISpatialReference outputSpatialReference, List<string> fcNameList = null, WaitOperation wo = null)
        {
            try
            {
                //获取源数据库中的所有要素类名称集合
                IWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                if (null == sdeWorkspace)
                {
                    MessageBox.Show("无法访问服务器！");
                    return false;
                }

                if (wo != null)
                    wo.SetText("正在创建数据库...");

                //创建一个相同结构的空数据库
                Dictionary<IFeatureClass, IFeatureClass> fcList = CreateGDBStruct(sdeWorkspace, outputGDB, fcNameList, outputSpatialReference);

                //插入要素
                foreach (var item in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在裁切要素类:{0}...", item.Value.AliasName));

                    clipGeo.Project((item.Key as IGeoDataset).SpatialReference);//投影变换(与输入要素类的空间参考保持一致)
                    //单独处理BOUA境界面,其余保持裁切面边界的要素完整性
                    if (item.Key.AliasName.ToUpper().IndexOf("BOUA") != -1)
                    {
                        AttachIntersectFeatures0(item.Key, clipGeo, pageGeo, item.Value, true);

                    }
                    else
                    {
                        AttachIntersectFeatures(item.Key, clipGeo, pageGeo, item.Value, true);
                    }
                    //AttachIntersectFeatures(item.Key, clipGeo,pageGeo, item.Value, true);
                    //copyIntersectFeatures(item.Key, clipGeo, item.Value);//将输入要素类中位于裁切几何体中的要素插入到输出要素类
                }
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

        /// <summary>
        /// 批量下载，通过shell
        /// </summary>
        public bool DownExtensionBatchData(IGeometry clipGeo, IGeometry pageGeo, string outputGDB, ISpatialReference outputSpatialReference,  List<string> fcNameList = null, WaitOperation wo = null)
        {
            try
            {
                
                //获取源数据库中的所有要素类名称集合
                IWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                if (null == sdeWorkspace)
                {
                    MessageBox.Show("无法访问服务器！");
                    return false;
                }

                if (wo != null)
                    wo.SetText("正在创建数据库...");

                //创建一个相同结构的空数据库
                Dictionary<IFeatureClass, IFeatureClass> fcList = CreateGDBStruct(sdeWorkspace, outputGDB, fcNameList, outputSpatialReference);

                //插入要素
                foreach (var item in fcList)
                {
                    try
                    {
                        
                        if (wo != null)
                            wo.SetText(string.Format("正在裁切要素类:{0}...", item.Value.AliasName));

                        clipGeo.Project((item.Key as IGeoDataset).SpatialReference);//投影变换(与输入要素类的空间参考保持一致)
                        //单独处理BOUA境界面,其余保持裁切面边界的要素完整性
                        if (item.Key.AliasName.ToUpper().IndexOf("BOUA") != -1)
                        {
                            AttachIntersectFeatures0(item.Key, clipGeo, pageGeo, item.Value, true);

                        }
                        else
                        {
                            AttachIntersectFeatures(item.Key, clipGeo, pageGeo, item.Value, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                    }
                }
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

        /// <summary>
        /// 向目标数据库追加附区数据（在输出要素类中增加附加数据标识字段）
        /// </summary>
        /// <param name="clipGeo"></param>
        /// <param name="pTargetWorkspace"></param>
        /// <param name="outputSpatialReference"></param>
        /// <param name="bAddFlagField"></param>
        /// <param name="fcNameList"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public double AttachMapScale = 0;
        public bool DownLoadAttachData(IGeometry clipGeo, IWorkspace pTargetWorkspace, ISpatialReference outputSpatialReference, bool bAddFlagField = true, List<string> fcNameList = null, WaitOperation wo = null)
        {
            try
            {
                //获取源数据库中的所有要素类名称集合
                IWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                if (null == sdeWorkspace)
                {
                    MessageBox.Show("无法访问服务器！");
                    return false;
                }
                
                //获取要素类映射关系
                Dictionary<IFeatureClass, IFeatureClass> fcList = FeatureClassMapping(sdeWorkspace, pTargetWorkspace, fcNameList);


                //插入要素
                foreach (var item in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在裁切要素类:{0}...", item.Value.AliasName));

                    clipGeo.Project((item.Key as IGeoDataset).SpatialReference);//投影变换(与输入要素类的空间参考保持一致)

                   // AttachIntersectFeatures(item.Key, clipGeo, item.Value, true);//将输入要素类中位于裁切几何体中的要素追加到输出要素类
                }
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

        /// <summary>
        /// 下载专题数据[内存释放]
        /// </summary>
        /// <param name="clipGeo"></param>
        /// <param name="pTargetWorkspace"></param>
        /// <param name="outputSpatialReference"></param>
        /// <param name="dataInfo"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public bool DownLoadThematicData(IGeometry clipGeo, IWorkspace pTargetWorkspace, ISpatialReference outputSpatialReference, ThematicDataInfo dataInfo, WaitOperation wo = null)
        {
            try
            {
                //获取专题源数据库中的所有要素类名称集合
                IWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(dataInfo.IP, dataInfo.UserName, dataInfo.Password, dataInfo.DataBase);
                if (null == sdeWorkspace)
                {
                    MessageBox.Show("无法访问服务器！");
                    return false;
                }

                if (wo != null)
                    wo.SetText("正在创建图层...");

                //向pTargetWorkspace增加与专题数据库相同的空数据结构
                Dictionary<IFeatureClass, IFeatureClass> fcList = AddThematicStructToGDB(sdeWorkspace, pTargetWorkspace, dataInfo, outputSpatialReference);

                //插入要素
                foreach (var item in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在裁切要素类:{0}...", item.Value.AliasName));

                    clipGeo.Project((item.Key as IGeoDataset).SpatialReference);//投影变换(与输入要素类的空间参考保持一致)

                    copyIntersectFeatures(item.Key, clipGeo, item.Value);//将输入要素类中位于裁切几何体中的要素插入到输出要素类
                }
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
        
        /// <summary>
        /// 创建要素类至GDB
        /// </summary>
        /// <param name="DBFile"></param>
        /// <param name="geoType"></param>
        /// <param name="name2Type">字段名称2字段类型</param>
        /// <param name="sr"></param>
        /// <param name="fcName"></param>
        /// <returns></returns>
        public IFeatureClass CreateFeatureClass(string DBFile, esriGeometryType geoType, Dictionary<string, esriFieldType> name2Type, ISpatialReference sr, string fcName)
        {
            //读取工作空间
            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(DBFile, 0);
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;

            //创建要素类的字段(RequiredFields)
            IObjectClassDescription objectClassDescription = new FeatureClassDescriptionClass();
            IFeatureClassDescription featureClassDescription = objectClassDescription as IFeatureClassDescription;
            IFields fields = objectClassDescription.RequiredFields;
            IGeometryDef geometryDef = new GeometryDefClass();
            var geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = geoType;
            geometryDefEdit.SpatialReference_2 = sr;
            int shapeFieldIndex = fields.FindField(featureClassDescription.ShapeFieldName);
            IField field = fields.get_Field(shapeFieldIndex);
            var fieldEdit = (IFieldEdit)field;
            fieldEdit.GeometryDef_2 = geometryDef;

            //创建要素类的字段(附加)
            IFieldsEdit pFieldsEdit = fields as IFieldsEdit;
            foreach (var kv in name2Type)
            {
                IField pfield = new FieldClass();
                IFieldEdit pfieldEdit = pfield as IFieldEdit;
                pfieldEdit.Name_2 = kv.Key;
                pfieldEdit.Type_2 = kv.Value;
                pFieldsEdit.AddField(pfield);
            }
            //创建新的要素类
            return pFeatureWorkspace.CreateFeatureClass(fcName, fields, null, null, esriFeatureType.esriFTSimple, featureClassDescription.ShapeFieldName, "");
        }

        /// <summary>
        /// 添加要素
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="clipGeo"></param>
        /// <param name="name2val"></param>
        public void addFeature2FC(IFeatureClass fc, IGeometry clipGeo, Dictionary<string, object> name2val)
        {
            IFeatureCursor pFeatureCursor = fc.Insert(true);
            IFeatureBuffer pFeatureBuffer = fc.CreateFeatureBuffer();
            pFeatureBuffer.Shape = clipGeo;
            foreach (var kv in name2val)
            {
                int index = pFeatureBuffer.Fields.FindField(kv.Key);
                if (index != -1)
                {
                    pFeatureBuffer.set_Value(index, kv.Value);
                }
            }

            pFeatureCursor.InsertFeature(pFeatureBuffer);
            pFeatureCursor.Flush();

            Marshal.ReleaseComObject(pFeatureCursor);

        }

        /// <summary>
        /// 创建一个与sourceWorkspace相同结构的空数据库，并返回源数据库与新数据库的要素类列表
        /// </summary>
        /// <param name="sourceWorkspace">sde</param>
        /// <param name="outputGDB">local</param>
        /// <param name="fcNameList">获取的要素类名称，若为空则获取原数据库中全部要素类</param>
        /// <param name="outputSpatialReference"></param>
        /// <returns></returns>
        private Dictionary<IFeatureClass, IFeatureClass>  CreateGDBStruct(IWorkspace pSourceWorkspace, string outputGDB, List<string> fcNameList, ISpatialReference outputSpatialReference = null)
        {
            Dictionary<IFeatureClass, IFeatureClass> result = new Dictionary<IFeatureClass,IFeatureClass>();

            //创建输出工作空间
            if (System.IO.Directory.Exists(outputGDB))
            {
                System.IO.Directory.Delete(outputGDB, true);
            }
            IWorkspace pOutputWorkspace = createGDB(outputGDB);
            IFeatureWorkspace pOutputFeatureWorkspace = pOutputWorkspace as IFeatureWorkspace;

            //创建数据库结构
            IFeatureWorkspace pSourceFeatureWorkspace = (IFeatureWorkspace)pSourceWorkspace;
            IEnumDataset pSourceEnumDataset = pSourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            pSourceEnumDataset.Reset();
            IDataset pSourceDataset ;
            while ( (pSourceDataset = pSourceEnumDataset.Next()) != null)
            {
                if (pSourceDataset is IFeatureDataset)//要素数据集
                {
                    //新数据集
                    IFeatureDataset pOutputFeatureDataset = null;

                    //遍历子要素类
                    IFeatureDataset pSourceFeatureDataset = pSourceFeatureWorkspace.OpenFeatureDataset(pSourceDataset.Name);
                    IEnumDataset pEnumDatasetF = pSourceFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF ;
                    while ( (pDatasetF = pEnumDatasetF.Next()) != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            if (fcNameList != null && !fcNameList.Contains(pDatasetF.Name.Split('.').Last()))
                            {
                                continue;
                            }

                            IFeatureClass fc = pSourceFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);

                            IFields fields = createFeatureClassFields(fc, outputSpatialReference);
                            esriFeatureType featureType = esriFeatureType.esriFTSimple;
                            string shapeFieldName = fc.ShapeFieldName;

                            if (null == pOutputFeatureDataset)
                            {
                                //创建新数据集
                                if (null == outputSpatialReference)
                                    outputSpatialReference = (pSourceDataset as IGeoDataset).SpatialReference;

                                pOutputFeatureDataset = pOutputFeatureWorkspace.CreateFeatureDataset(pSourceDataset.Name.Split('.').Last(), outputSpatialReference);
                            }
                            if (fc.Extension as IAnnoClass != null)
                            {
                                IFeatureClass newFC = CreateAnnoFeatureClass(null,pOutputFeatureDataset, pDatasetF.Name.Split('.').Last(), fc);
                                result.Add(fc, newFC);
                            }
                            else
                            {
                                //创建新的要素类
                                IFeatureClass newFC = pOutputFeatureDataset.CreateFeatureClass(pDatasetF.Name.Split('.').Last(), fields, null, null, featureType, shapeFieldName, "");

                                result.Add(fc, newFC);
                            }
                        }
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pSourceDataset is IFeatureClass)//要素类
                {
                    if (fcNameList != null && !fcNameList.Contains(pSourceDataset.Name.Split('.').Last()))
                    {
                        continue;
                    }

                    IFeatureClass fc = pSourceFeatureWorkspace.OpenFeatureClass(pSourceDataset.Name);

                    IFields fields = createFeatureClassFields(fc, outputSpatialReference);
                    esriFeatureType featureType = esriFeatureType.esriFTSimple;
                    string shapeFieldName = fc.ShapeFieldName;
                    if (fc.Extension as IAnnoClass != null)
                    {
                        IFeatureClass newFC = CreateAnnoFeatureClass(pOutputFeatureWorkspace,null, pSourceDataset.Name.Split('.').Last(), fc);
                        result.Add(fc, newFC);
                    }
                    else
                    {
                        //创建新的要素类
                        IFeatureClass newFC = pOutputFeatureWorkspace.CreateFeatureClass(pSourceDataset.Name.Split('.').Last(), fields, null, null, featureType, shapeFieldName, "");

                        result.Add(fc, newFC);
                    }
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pSourceEnumDataset);

            return result;
        }
        private IFeatureClass CreateAnnoFeatureClass( IFeatureWorkspace fws,IFeatureDataset pFeatureDataset, string Name,IFeatureClass sourefc)
        {
            var content = EnvironmentSettings.getContentElement(GApplication.Application);
            var server = content.Element("Server");
            double scale = double.Parse(content.Element("MapScale").Value);
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();

            IFeatureWorkspaceAnno pWorkspaceAnno = null;
            if (fws != null)
            {
                pWorkspaceAnno = fws as IFeatureWorkspaceAnno;
            }
            if (pFeatureDataset != null)
            {
                pWorkspaceAnno = pFeatureDataset.Workspace as IFeatureWorkspaceAnno;
            }/*------------------------------------------------*/
           
            IObjectClassDescription pObjectClassDes = new AnnotationFeatureClassDescriptionClass();
            IFeatureClassDescription pFCDescription = pObjectClassDes as IFeatureClassDescription;
            IFields pFields = pObjectClassDes.RequiredFields;

            IAnnotateLayerProperties pALProperties = new LabelEngineLayerPropertiesClass();
            pALProperties.FeatureLinked = false;
            pALProperties.AddUnplacedToGraphicsContainer = false;
            pALProperties.CreateUnplacedElements = true;
            pALProperties.DisplayAnnotation = true;
            pALProperties.UseOutput = true;
            ILabelEngineLayerProperties pLayerEngineLayerProps = pALProperties as ILabelEngineLayerProperties;
            IAnnotateLayerProperties pAlp = pLayerEngineLayerProps as IAnnotateLayerProperties;
            pAlp.Class = "Annotation Class 1";
            IAnnotateLayerPropertiesCollection pAnnoPropscollection = new AnnotateLayerPropertiesCollection();
            pAnnoPropscollection.Add(pAlp);

            IGraphicsLayerScale pGraphicsLayerScale = new GraphicsLayerScaleClass();
            pGraphicsLayerScale.Units = esriUnits.esriMeters;
            pGraphicsLayerScale.ReferenceScale = scale;

            /****************/
            IFormattedTextSymbol myTextSymbol = new TextSymbolClass();
            //peizhi 类里边是三个基础函数分辨是IRgbColor IFontDisp ITextSymbol 对应的，方法基础，此处无代码
            
            stdole.IFontDisp pFont =  new stdole.StdFont()
            {
                Name = "宋体",
                Size = 2
            } as stdole.IFontDisp;
            IColor pColor = new RgbColorClass{Red=200,Blue=200,Green=200};

            myTextSymbol.Color = pColor;
            myTextSymbol.Font = pFont;
            myTextSymbol.Angle = 0;
            myTextSymbol.RightToLeft = false;
            myTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVABaseline;
            myTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHAFull;
            myTextSymbol.CharacterSpacing = 200;
            myTextSymbol.Case = esriTextCase.esriTCNormal;

            ISymbolCollection pSymbolCollection = new SymbolCollectionClass();
            pSymbolCollection.set_Symbol(0, myTextSymbol as ISymbol);

            IFeatureClass pAnnoFeatureClass = pWorkspaceAnno.CreateAnnotationClass(
                Name,
                pFields,
                pObjectClassDes.InstanceCLSID,
                pObjectClassDes.ClassExtensionCLSID,
                pFCDescription.ShapeFieldName,
                "",
                pFeatureDataset,
                null,
                pAnnoPropscollection,
                pGraphicsLayerScale,
                pSymbolCollection,
                true);


            for(int i=0;i<sourefc.Fields.FieldCount;i++)
            {
                IField field = sourefc.Fields.get_Field(i);
                if (field.Type == esriFieldType.esriFieldTypeBlob || field.Type == esriFieldType.esriFieldTypeOID)
                    continue;
                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                    continue;
                if(field.Name.ToUpper().Contains("SHAPE"))
                    continue;
                if (pAnnoFeatureClass.FindField(field.Name) == -1)
                {
                    
                    IField pField = new FieldClass();
                    IFieldEdit pFieldEdit = pField as IFieldEdit;
                    pFieldEdit.Name_2 = field.Name;
                    pFieldEdit.AliasName_2 = field.Name;
                    pFieldEdit.Length_2 = field.Length;
                    pFieldEdit.Type_2 =  field.Type;
                    IClass pTable = pAnnoFeatureClass as IClass;
                    pTable.AddField(pField);
                }
            }
            ISpatialReference spatialRef = (pAnnoFeatureClass as IGeoDataset).SpatialReference;

            if (targetSpatialReference.Name != spatialRef.Name)
            {
                IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = pAnnoFeatureClass as IGeoDatasetSchemaEdit;
                if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                {
                    pGeoDatasetSchemaEdit.AlterSpatialReference(targetSpatialReference);
                }
            }
            return pAnnoFeatureClass;
        
        }
        /// <summary>
        /// 两个数据库中的要素类映射关系
        /// </summary>
        /// <param name="pSourceWorkspace">sde</param>
        /// <param name="pTargetWorkspace">local</param>
        /// <param name="fcNameList"></param>
        /// <returns></returns>
        private Dictionary<IFeatureClass, IFeatureClass> FeatureClassMapping(IWorkspace pSourceWorkspace, IWorkspace pTargetWorkspace, List<string> fcNameList)
        {
            Dictionary<IFeatureClass, IFeatureClass> result = new Dictionary<IFeatureClass, IFeatureClass>();


            IFeatureWorkspace pTargetFeatureWorkspace = pTargetWorkspace as IFeatureWorkspace;

            //创建数据库结构
            IFeatureWorkspace pSourceFeatureWorkspace = (IFeatureWorkspace)pSourceWorkspace;
            IEnumDataset pSourceEnumDataset = pSourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            pSourceEnumDataset.Reset();
            IDataset pSourceDataset;
            while ((pSourceDataset = pSourceEnumDataset.Next()) != null)
            {
                if (pSourceDataset is IFeatureDataset)//要素数据集
                {
                    //新数据集
                    IFeatureDataset pOutputFeatureDataset = null;

                    //遍历子要素类
                    IFeatureDataset pSourceFeatureDataset = pSourceFeatureWorkspace.OpenFeatureDataset(pSourceDataset.Name);
                    IEnumDataset pEnumDatasetF = pSourceFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF;
                    while ((pDatasetF = pEnumDatasetF.Next()) != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            string fcName = pDatasetF.Name.Split('.').Last();
                            if (fcNameList != null && !fcNameList.Contains(fcName))
                            {
                                continue;
                            }

                            //目标数据库中是否存在相对应的要素类
                            if ((pTargetWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
                            {
                                IFeatureClass fc = pSourceFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);

                                IFeatureClass targetFC = pTargetFeatureWorkspace.OpenFeatureClass(fcName);

                                result.Add(fc, targetFC);
                            }
                            
                        }
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pSourceDataset is IFeatureClass)//要素类
                {
                    string fcName = pSourceDataset.Name.Split('.').Last();
                    if (fcNameList != null && !fcNameList.Contains(fcName))
                    {
                        continue;
                    }

                    //目标数据库中是否存在相对应的要素类
                    if ((pTargetWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
                    {
                        IFeatureClass fc = pSourceFeatureWorkspace.OpenFeatureClass(pSourceDataset.Name);

                        IFeatureClass targetFC = pTargetFeatureWorkspace.OpenFeatureClass(fcName);

                        result.Add(fc, targetFC);
                    }
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pSourceEnumDataset);

            return result;
        }

        /// <summary>
        /// 增加专题数据图层(默认专题数据图层名与原底图数据图层名都不同)
        /// </summary>
        /// <param name="pSourceWorkspace"></param>
        /// <param name="pTargetWorkspace"></param>
        /// <param name="dataInfo"></param>
        /// <param name="outputSpatialReference"></param>
        /// <returns></returns>

        private Dictionary<IFeatureClass, IFeatureClass> AddThematicStructToGDB(IWorkspace pSourceWorkspace, IWorkspace pTargetWorkspace, ThematicDataInfo dataInfo, ISpatialReference outputSpatialReference = null)
        {
            Dictionary<IFeatureClass, IFeatureClass> result = new Dictionary<IFeatureClass, IFeatureClass>();

            //需下载的专题数据图层名
            var fcNameList = new List<string>();
            foreach (var kv in dataInfo.Lyrs)
            {
                if (kv.Value)
                {
                    fcNameList.Add(kv.Key);
                }
            }

            //目标工作空间
            IFeatureWorkspace pTargetFeatureWorkspace = pTargetWorkspace as IFeatureWorkspace;


            //追加数据库结构
            IFeatureWorkspace pSourceFeatureWorkspace = (IFeatureWorkspace)pSourceWorkspace;
            IEnumDataset pSourceEnumDataset = pSourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            pSourceEnumDataset.Reset();
            IDataset pSourceDataset;
            while ((pSourceDataset = pSourceEnumDataset.Next()) != null)
            {
                if (pSourceDataset is IFeatureDataset)//要素数据集
                {
                    //新数据集
                    IFeatureDataset pTargetFeatureDataset = null;

                    //遍历子要素类
                    IFeatureDataset pSourceFeatureDataset = pSourceFeatureWorkspace.OpenFeatureDataset(pSourceDataset.Name);
                    IEnumDataset pEnumDatasetF = pSourceFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF;
                    while ((pDatasetF = pEnumDatasetF.Next()) != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            if (!fcNameList.Contains(pDatasetF.Name.Split('.').Last()))
                            {
                                continue;
                            }

                            IFeatureClass fc = pSourceFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);

                            IFields fields = createFeatureClassFields(fc, outputSpatialReference);
                            esriFeatureType featureType = esriFeatureType.esriFTSimple;
                            string shapeFieldName = fc.ShapeFieldName;

                            //专题不创建数据集
                            //if (null == pTargetFeatureDataset)
                            //{
                            //    //创建新数据集
                            //    if (null == outputSpatialReference)
                            //        outputSpatialReference = (pSourceDataset as IGeoDataset).SpatialReference;

                            //    pTargetFeatureDataset = pTargetFeatureWorkspace.CreateFeatureDataset(pSourceDataset.Name.Split('.').Last(), outputSpatialReference);
                            //}

                            //创建新的要素类
                            IFeatureClass newFC = pTargetFeatureWorkspace.CreateFeatureClass(pDatasetF.Name.Split('.').Last(), fields, null, null, featureType, shapeFieldName, "");

                            result.Add(fc, newFC);
                        }
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pSourceDataset is IFeatureClass)//要素类
                {
                    if (fcNameList != null && !fcNameList.Contains(pSourceDataset.Name.Split('.').Last()))
                    {
                        continue;
                    }

                    IFeatureClass fc = pSourceFeatureWorkspace.OpenFeatureClass(pSourceDataset.Name);

                    IFields fields = createFeatureClassFields(fc, outputSpatialReference);
                    esriFeatureType featureType = esriFeatureType.esriFTSimple;
                    string shapeFieldName = fc.ShapeFieldName;

                    //创建新的要素类
                    IFeatureClass newFC = pTargetFeatureWorkspace.CreateFeatureClass(pSourceDataset.Name.Split('.').Last(), fields, null, null, featureType, shapeFieldName, "");

                    result.Add(fc, newFC);
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pSourceEnumDataset);

            return result;
        }
      

        /// <summary>
        /// 创建GDB数据库
        /// </summary>
        /// <param name="gdbFullFileName"></param>
        /// <returns></returns>
        private IWorkspace createGDB(string gdbFullFileName)
        {
            int lastSlashIndex = gdbFullFileName.LastIndexOf("\\");
            int lastDotIndex = gdbFullFileName.LastIndexOf(".");

            string path = gdbFullFileName.Substring(0, lastSlashIndex);
            string databasename = gdbFullFileName.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);

            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspaceName workspaceName = workspaceFactory.Create(path, databasename, null, 0);
            IName name = workspaceName as IName;
            IWorkspace workspace = name.Open() as IWorkspace;

            SetWorkspaceIcon(workspace);

            return workspace;
        }

        /// <summary>
        /// 获取要素类的字段结构信息
        /// </summary>
        /// <param name="pSourceFeatureClass"></param>
        /// <param name="outputSpatialReference"></param>
        /// <returns></returns>
        private IFields createFeatureClassFields(IFeatureClass pSourceFeatureClass, ISpatialReference outputSpatialReference = null)
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

        /// <summary>
        /// 将输入要素类中位于裁切几何体中的要素插入到输出要素类
        /// </summary>
        /// <param name="inFeatureClss">输入要素类</param>
        /// <param name="clipGeometry">裁切几何体</param>
        /// <param name="outFeatureClss">输出要素类</param>
        private void copyIntersectFeatures(IFeatureClass inFeatureClss, IGeometry clipGeometry, IFeatureClass outFeatureClss)
        {
            GC.Collect();
            IGeometry clipGeometry_out = (clipGeometry as IClone).Clone() as IGeometry;

            bool bProjection = false;
            ISpatialReference in_sr = (inFeatureClss as IGeoDataset).SpatialReference;
            ISpatialReference out_sr = (outFeatureClss as IGeoDataset).SpatialReference;
            if (in_sr.Name != out_sr.Name)
            {
                bProjection = true;
                clipGeometry_out.Project(out_sr);
            }
            string attachFieldName = "ATTACH";
            //邻区数据标识字段
            int attachIndex = outFeatureClss.FindField(attachFieldName);
            if (attachIndex == -1)
            {
                CommonMethods.AddField(outFeatureClss, attachFieldName);
                attachIndex = outFeatureClss.FindField(attachFieldName);
            }
            
            
            IFeatureCursor pOutFeatureCursor = outFeatureClss.Insert(true);
            
            esriGeometryDimension dim = esriGeometryDimension.esriGeometryNoDimension;
            switch (inFeatureClss.ShapeType)
            {
                case esriGeometryType.esriGeometryEnvelope:
                    dim = esriGeometryDimension.esriGeometry25Dimension;
                    break;
                case esriGeometryType.esriGeometryMultipoint:
                case esriGeometryType.esriGeometryPoint:
                    dim = esriGeometryDimension.esriGeometry0Dimension;
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    dim = esriGeometryDimension.esriGeometry2Dimension;
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    dim = esriGeometryDimension.esriGeometry1Dimension;
                    break;
                default:
                    break;
            }

            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = clipGeometry;
            pSpatialFilter.GeometryField = "SHAPE";
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            IFeatureCursor pInFeatureCursor = inFeatureClss.Search(pSpatialFilter, true);
            IFeature pInFeature = null;

            try
            {
                
                while ((pInFeature = pInFeatureCursor.NextFeature()) != null)
                {
                    IFeatureBuffer pFeatureBuffer = outFeatureClss.CreateFeatureBuffer();

                    IGeometry shape_copy = pInFeature.ShapeCopy;
                    if (bProjection)
                        shape_copy.Project(out_sr);//投影变换

                    ITopologicalOperator pTopologicalOperator = clipGeometry_out as ITopologicalOperator;
                    IGeometry pGeometryResult = null;
                    IRelationalOperator pRe = clipGeometry_out as IRelationalOperator;
                    if (pRe.Contains(shape_copy))//Contains预处理可优化Intersect的速度（Disjoint预处理对Intersect没什么效果）
                    {
                        pGeometryResult = shape_copy;
                    }
                    else
                    {
                        pGeometryResult = pTopologicalOperator.Intersect(shape_copy, dim);
                        ITopologicalOperator topo = pGeometryResult as ITopologicalOperator;
                        topo.Simplify();
                        if (pGeometryResult == null || pGeometryResult.IsEmpty)
                            continue;//空几何直接跳过

                        switch (inFeatureClss.ShapeType)
                        {
                            case esriGeometryType.esriGeometryPolygon:
                                {
                                    if ((pGeometryResult as IArea).Area == 0)
                                    {
                                        continue;//面积为0的要素，直接跳过
                                    }

                                    #region 多部件
                                    //var po = (IPolygon4)pGeometryResult;
                                    //var gc = (IGeometryCollection)po.ConnectedComponentBag;
                                    //if (gc.GeometryCount > 1)//多部件
                                    //{

                                    //}
                                    #endregion

                                    break;
                                }
                            case esriGeometryType.esriGeometryPolyline:
                                {
                                    if ((pGeometryResult as IPolyline).Length == 0)
                                    {
                                        continue;//长度为0的要素，直接跳过
                                    }

                                    #region 多部件
                                    var gc = (IGeometryCollection)pGeometryResult;
                                    if (gc.GeometryCount > 1)//多部件
                                    {
                                        //if (inFeatureClss.AliasName.ToUpper().EndsWith("HYDL"))
                                        //{
                                        //    pGeometryResult = shape_copy;
                                        //}
                                    }
                                    #endregion

                                    break;
                                }
                        }
                    }

                    


                    pFeatureBuffer.Shape = pGeometryResult;
                    for (int i = 0; i < pFeatureBuffer.Fields.FieldCount; i++)
                    {
                        IField pfield = pFeatureBuffer.Fields.get_Field(i);
                        if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                        {
                            continue;
                        }

                        if (pfield.Name == "SHAPE_Length" || pfield.Name == "SHAPE_Area")
                        {
                            continue;
                        }

                        int index = pInFeature.Fields.FindField(pfield.Name);
                        if (index != -1 && pfield.Editable)
                        {
                            pFeatureBuffer.set_Value(i, pInFeature.get_Value(index));
                        }

                    }
                    pOutFeatureCursor.InsertFeature(pFeatureBuffer);
                    Marshal.ReleaseComObject(shape_copy);
                    Marshal.ReleaseComObject(pInFeature);
                }
   
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
            pOutFeatureCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(clipGeometry_out);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pOutFeatureCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pInFeatureCursor);
            
        }


        /// <summary>
        /// 将输入要素类中位于裁切几何体中的要素插入到输出要素类:内存控制释放
        /// </summary>
        /// <param name="inFeatureClss">输入要素类</param>
        /// <param name="clipGeometry">裁切几何体</param>
        /// <param name="outFeatureClss">输出要素类</param>
        /// <param name="bAddAttachField">是否在输出要素类中增加附加数据标识字段</param>
        /// <param name="attachFieldName">标识字段名称</param>
        /// <param name="attachFieldValue">字段值</param>
        private void AttachIntersectFeatures(IFeatureClass inFeatureClss, IGeometry clipGeometry,IGeometry pageGeometry, IFeatureClass outFeatureClss,
            bool bAddAttachField, string attachFieldName = "ATTACH", string attachFieldValue = "1")
        {
            //对注记进行单独处理
            if (inFeatureClss.Extension as IAnnoClass != null)
            {
                AttachANNOFeatures(inFeatureClss, clipGeometry, pageGeometry, outFeatureClss, bAddAttachField);
                return;
            }


            IGeometry clipGeometry_out = (clipGeometry as IClone).Clone() as IGeometry;

            bool bProjection = false;
            ISpatialReference in_sr = (inFeatureClss as IGeoDataset).SpatialReference;
            ISpatialReference out_sr = (outFeatureClss as IGeoDataset).SpatialReference;
            if (in_sr.Name != out_sr.Name)
            {
                bProjection = true;
                clipGeometry_out.Project(out_sr);
            }

            //邻区数据标识字段
            int attachIndex = outFeatureClss.FindField(attachFieldName);
            if (attachIndex == -1 && bAddAttachField)
            {
                CommonMethods.AddField(outFeatureClss, attachFieldName);
                attachIndex = outFeatureClss.FindField(attachFieldName);
            }
           

            IFeatureCursor pOutFeatureCursor = outFeatureClss.Insert(true);

            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = pageGeometry.Envelope;//采用直接采用纸张页面获取所有要素
            pSpatialFilter.GeometryField = "SHAPE";
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            

            IFeatureCursor pInFeatureCursor = inFeatureClss.Search(pSpatialFilter, true);
            IFeature pInFeature = null;
            IRelationalOperator pRe0 = pageGeometry.Envelope as IRelationalOperator;//纸张外接矩形
            IRelationalOperator pRe = clipGeometry_out as IRelationalOperator;//裁切面
            ITopologicalOperator pTopologicalOperator = clipGeometry_out as ITopologicalOperator;
            try
            {

                while ((pInFeature = pInFeatureCursor.NextFeature()) != null)
                {
                    IFeatureBuffer pFeatureBuffer = outFeatureClss.CreateFeatureBuffer();

                    #region
                    bool bAttach = true;//是否为邻区要素
                    IGeometry shape_copy = pInFeature.ShapeCopy;
                    if (bProjection)
                        shape_copy.Project(out_sr);//裁切下来的数据进行投影变换

                 
                    IGeometry pGeometryResult = null;
                     //1.判断与外接矩形关系
                    if (pRe0.Contains(shape_copy))
                    {
                        pGeometryResult = shape_copy;
                        if (pRe.Disjoint(pGeometryResult))//相离，邻区要素
                        {
                            bAttach = true;//邻区要素
                        }
                        else if (pRe.Contains(pGeometryResult))//包含在裁切面内，主区要素
                        {
                            bAttach = false;//主区要素
                        }
                        else//相交：根据要素与裁切面的求交结果来判断
                        {
                            IGeometry intersetResult = pTopologicalOperator.Intersect(pGeometryResult, pGeometryResult.Dimension);
                            if (intersetResult == null || intersetResult.IsEmpty)
                            {
                                bAttach = true;//邻区要素
                            }
                            else
                            {
                                bAttach = false;//主区要素
                            }
                        }

                        
                    }
                    else //相交 
                    {
                        (shape_copy as ITopologicalOperator).Clip(pageGeometry.Envelope);
                        pGeometryResult = shape_copy;
                        ITopologicalOperator topo = pGeometryResult as ITopologicalOperator;
                        topo.Simplify();

                        if (pGeometryResult == null || pGeometryResult.IsEmpty)
                            continue;//空几何直接跳过
                        //2.判断与裁切面关系
                        if (pRe.Disjoint(pGeometryResult))//相离，邻区要素
                        {
                            bAttach = true;//邻区要素
                        }
                        else if (pRe.Contains(pGeometryResult))//包含在裁切面内，主区要素
                        {
                            bAttach = false;//主区要素
                        }
                        else//相交：根据要素与裁切面的求交结果来判断
                        {
                            IGeometry intersetResult = pTopologicalOperator.Intersect(pGeometryResult, pGeometryResult.Dimension);
                            if (intersetResult == null || intersetResult.IsEmpty)
                            {
                                bAttach = true;//邻区要素
                            }
                            else
                            {
                                bAttach = false;//主区要素
                            }
                        }
                        
                    }
                    if (pGeometryResult == null || pGeometryResult.IsEmpty)
                        continue;//空几何直接跳过
                    
                    pFeatureBuffer.Shape = pGeometryResult;
                    for (int i = 0; i < pFeatureBuffer.Fields.FieldCount; i++)
                    {
                        IField pfield = pFeatureBuffer.Fields.get_Field(i);
                        if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                        {
                            continue;
                        }

                        if (pfield.Name == "SHAPE_Length" || pfield.Name == "SHAPE_Area")
                        {
                            continue;
                        }

                        int index = pInFeature.Fields.FindField(pfield.Name);
                        if (index != -1 && pfield.Editable)
                        {
                            pFeatureBuffer.set_Value(i, pInFeature.get_Value(index));
                        }

                    }

                    if (bAddAttachField && attachIndex != -1)//初始化附加字段值
                    {
                        if (bAttach)//邻区
                          pFeatureBuffer.set_Value(attachIndex, attachFieldValue);
                        else//主区
                            pFeatureBuffer.set_Value(attachIndex, DBNull.Value);
                    }

                    pOutFeatureCursor.InsertFeature(pFeatureBuffer);
                    //释放内存
                    Marshal.ReleaseComObject(shape_copy);
                    Marshal.ReleaseComObject(pInFeature);
                    #endregion
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
            pOutFeatureCursor.Flush();
            //释放内存
            Marshal.ReleaseComObject(clipGeometry_out);
            Marshal.ReleaseComObject(in_sr);
            Marshal.ReleaseComObject(out_sr);

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pOutFeatureCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pInFeatureCursor);
            
        }
        string[] annoFields = new string[] { "分类","GUID"};
        /// <summary>
        /// 添加注记要素类
        /// </summary>
        private void AttachANNOFeatures(IFeatureClass inFeatureClss, IGeometry clipGeometry, IGeometry pageGeometry, IFeatureClass outFeatureClss,
            bool bAddAttachField, string attachFieldName = "ATTACH", string attachFieldValue = "1")
        {
            IGeometry clipGeometry_out = (clipGeometry as IClone).Clone() as IGeometry;

            bool bProjection = false;
            ISpatialReference in_sr = (inFeatureClss as IGeoDataset).SpatialReference;
            ISpatialReference out_sr = (outFeatureClss as IGeoDataset).SpatialReference;
            if (in_sr.Name != out_sr.Name)
            {
                bProjection = true;
                clipGeometry_out.Project(out_sr);
            }

            //邻区数据标识字段
            int attachIndex = outFeatureClss.FindField(attachFieldName);
            if (attachIndex == -1 && bAddAttachField)
            {
                CommonMethods.AddField(outFeatureClss, attachFieldName);
                attachIndex = outFeatureClss.FindField(attachFieldName);
            }


            //IFeatureCursor pOutFeatureCursor = outFeatureClss.Insert(true);
            //IFeatureBuffer pFeatureBuffer = outFeatureClss.CreateFeatureBuffer();


            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = pageGeometry.Envelope;//采用直接采用纸张页面获取所有要素
            pSpatialFilter.GeometryField = "SHAPE";
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;



            IFeatureCursor pInFeatureCursor = inFeatureClss.Search(pSpatialFilter, true);
            IFeature pInFeature = null;
            IRelationalOperator pRe0 = pageGeometry.Envelope as IRelationalOperator;//纸张外接矩形
            IRelationalOperator pRe = clipGeometry_out as IRelationalOperator;//裁切面
            ITopologicalOperator pTopologicalOperator = clipGeometry_out as ITopologicalOperator;
            try
            {

                while ((pInFeature = pInFeatureCursor.NextFeature()) != null)
                {
                    #region
                    bool bAttach = true;//是否为邻区要素
                    IGeometry shape_copy = pInFeature.ShapeCopy;
                    if (bProjection)
                        shape_copy.Project(out_sr);//裁切下来的数据进行投影变换

                    if (pRe.Disjoint(shape_copy))//相离，邻区要素
                    {
                        bAttach = true;//邻区要素
                    }
                    else //主区要素
                    {
                        bAttach = false;//主区要素
                    }
                    IFeature pFeatureBuffer = outFeatureClss.CreateFeature();
                    IAnnotationFeature2 annoFeature2 = pFeatureBuffer as IAnnotationFeature2;
                    IGeometry geometryAnno = ((pInFeature as IAnnotationFeature2).Annotation as IElement).Geometry;
                    if (bProjection)
                        geometryAnno.Project(out_sr);//裁切下来的数据进行投影变换
                    try
                    {
                        IElement annoEle = (((pInFeature as IAnnotationFeature2).Annotation as IElement) as IClone).Clone() as IElement;
                        annoEle.Geometry = geometryAnno;
                        annoFeature2.Annotation = annoEle;
                        annoFeature2.AnnotationClassID = (pInFeature as IAnnotationFeature2).AnnotationClassID;
                        annoFeature2.LinkedFeatureID = (pInFeature as IAnnotationFeature2).LinkedFeatureID;
                        annoFeature2.Status = (pInFeature as IAnnotationFeature2).Status;
                    }
                    catch
                    {
                    }
                    foreach (var field in annoFields)
                    {
                        int index = pInFeature.Class.FindField(field);
                        pFeatureBuffer.set_Value(outFeatureClss.FindField(field), pInFeature.get_Value(index));
                    }
                   // pFeatureBuffer.Shape = shape_copy;                    
                    if (bAddAttachField && attachIndex != -1)//初始化附加字段值
                    {
                        if (bAttach)//邻区
                            pFeatureBuffer.set_Value(attachIndex, attachFieldValue);
                        else//主区
                            pFeatureBuffer.set_Value(attachIndex, DBNull.Value);
                    }
                    pFeatureBuffer.Store();
                    //释放内存
                    Marshal.ReleaseComObject(shape_copy);
                    Marshal.ReleaseComObject(pInFeature);
                    #endregion
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
           // pOutFeatureCursor.Flush();
            //释放内存
            Marshal.ReleaseComObject(clipGeometry_out);
            Marshal.ReleaseComObject(in_sr);
            Marshal.ReleaseComObject(out_sr);

           // System.Runtime.InteropServices.Marshal.ReleaseComObject(pOutFeatureCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pInFeatureCursor);
            //Marshal.ReleaseComObject(pFeatureBuffer);
        }


        //设置工作空间的图标
        private void SetWorkspaceIcon(IWorkspace ws)
        {
            string dir = ws.PathName;

            string IniText = "[.ShellClassInfo]\nIconResource={0},0";
            string iniPath = dir + "\\desktop.ini";
            string icoPath = dir + "\\full.ico";
            File.Copy(GApplication.ExePath + "\\full.ico", icoPath);

            File.SetAttributes(icoPath, File.GetAttributes(icoPath) | FileAttributes.Hidden);

            using (var writer = System.IO.File.CreateText(iniPath))
            {
                writer.Write(string.Format(IniText, "full.ico"));
            }

            File.SetAttributes(iniPath, File.GetAttributes(iniPath) | FileAttributes.Hidden | FileAttributes.System);
            File.SetAttributes(dir, File.GetAttributes(dir) | FileAttributes.System);
        }


        /// <summary>
        /// 从工作空间中获取所有的要素类名称列表
        /// </summary>
        /// <param name="app"></param>
        /// <param name="ipAddress"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static List<string> getFeatureClassNames(GApplication app, string ipAddress, string userName, string passWord, string databaseName)
        {
            List<string> fcNames = new List<string>();

            IWorkspace pWorkspace = app.GetWorkspacWithSDEConnection(ipAddress, userName, passWord, databaseName);
            if (null == pWorkspace)
            {
                MessageBox.Show("无法访问服务器！");
                return fcNames;
            }


            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            while (pDataset != null)
            {
                if (pDataset is IFeatureDataset)//要素数据集
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;
                    IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                    IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF = pEnumDatasetF.Next();
                    while (pDatasetF != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);
                            if (fc != null)
                                fcNames.Add(fc.AliasName.Split('.').Last());
                        }

                        pDatasetF = pEnumDatasetF.Next();
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pDataset is IFeatureClass)//要素类
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;

                    IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDataset.Name);
                    if (fc != null)
                        fcNames.Add(fc.AliasName.Split('.').Last());
                }
                else
                {

                }

                pDataset = pEnumDataset.Next();

            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);

            return fcNames;
        }
       //获取要素类及几何类型
        public static Dictionary <string,string> getFeatureClassNamesEx(GApplication app, string ipAddress, string userName, string passWord, string databaseName,ThematicDataInfo di=null)
        {
            var fcNames = new Dictionary <string,string>();
            var fcFields= new Dictionary<string, List<string>>();
            IWorkspace pWorkspace = app.GetWorkspacWithSDEConnection(ipAddress, userName, passWord, databaseName);
            if (null == pWorkspace)
            {
                MessageBox.Show("无法访问专题数据库服务器！");
                return fcNames;
            }


            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            while (pDataset != null)
            {
                if (pDataset is IFeatureDataset)//要素数据集
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;
                    IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                    IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF = pEnumDatasetF.Next();
                    while (pDatasetF != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);
                            if (fc != null)
                            {
                                string fctype = "";
                                switch (fc.ShapeType)
                                {
                                    case esriGeometryType.esriGeometryPoint:
                                        fctype = "点";
                                        break;
                                    case esriGeometryType.esriGeometryPolyline:
                                        fctype = "线";
                                        break;
                                    case esriGeometryType.esriGeometryPolygon:
                                        fctype = "面";
                                        break;
                                    default:
                                        break;
                                }
                                fcNames.Add(fc.AliasName.Split('.').Last(), fctype);
                            }
                            var list = GetFields(fc);
                            fcFields[fc.AliasName.Split('.').Last()] = list;
                        }

                        pDatasetF = pEnumDatasetF.Next();
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pDataset is IFeatureClass)//要素类
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;

                    IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDataset.Name);
                    if (fc != null)
                    {
                        string fctype = "";
                        switch (fc.ShapeType)
                        {
                            case esriGeometryType.esriGeometryPoint:
                                fctype = "点";
                                break;
                            case esriGeometryType.esriGeometryPolyline:
                                fctype = "线";
                                break;
                            case esriGeometryType.esriGeometryPolygon:
                                fctype = "面";
                                break;
                            default:
                                break;
                        }
                        fcNames.Add(fc.AliasName.Split('.').Last(), fctype);
                        var list=GetFields(fc);
                        fcFields[fc.AliasName.Split('.').Last()] = list;
                    }
                }
                else
                {

                }

                pDataset = pEnumDataset.Next();

            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
            if (di != null)
            {
                di.LyrsFields = fcFields;
            }
            return fcNames;
        }
        private static List<string> GetFields(IFeatureClass fc)
        {
            List<string> ruleNames = new List<string>();
            for (int i = 0; i < fc.Fields.FieldCount; i++)
            {
                var field = fc.Fields.get_Field(i);
                if(field.Name.ToUpper().Contains("shape"))
                    continue;
                if (field.Type != esriFieldType.esriFieldTypeGeometry && field.Type != esriFieldType.esriFieldTypeBlob && field.Type != esriFieldType.esriFieldTypeOID)
                {
                    ruleNames.Add(field.Name);
                }
            }
            return ruleNames;
        }
        //原来处理方式

        private void AttachIntersectFeatures0(IFeatureClass inFeatureClss, IGeometry clipGeometry, IGeometry pageGeometry, IFeatureClass outFeatureClss,
          bool bAddAttachField, string attachFieldName = "ATTACH", string attachFieldValue = "1")
        {
            IGeometry clipGeometry_out = (clipGeometry as IClone).Clone() as IGeometry;

            bool bProjection = false;
            ISpatialReference in_sr = (inFeatureClss as IGeoDataset).SpatialReference;
            ISpatialReference out_sr = (outFeatureClss as IGeoDataset).SpatialReference;
            if (in_sr.Name != out_sr.Name)
            {
                bProjection = true;
                clipGeometry_out.Project(out_sr);
            }

            //邻区数据标识字段
            int attachIndex = outFeatureClss.FindField(attachFieldName);
            if (attachIndex == -1 && bAddAttachField)
            {
                CommonMethods.AddField(outFeatureClss, attachFieldName);
                attachIndex = outFeatureClss.FindField(attachFieldName);
            }
            esriGeometryDimension dim = esriGeometryDimension.esriGeometryNoDimension;
            switch (inFeatureClss.ShapeType)
            {
                case esriGeometryType.esriGeometryEnvelope:
                    dim = esriGeometryDimension.esriGeometry25Dimension;
                    break;
                case esriGeometryType.esriGeometryMultipoint:
                case esriGeometryType.esriGeometryPoint:
                    dim = esriGeometryDimension.esriGeometry0Dimension;
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    dim = esriGeometryDimension.esriGeometry2Dimension;
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    dim = esriGeometryDimension.esriGeometry1Dimension;
                    break;
                default:
                    break;
            }


            IFeatureCursor pOutFeatureCursor = outFeatureClss.Insert(true);
            
            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = pageGeometry.Envelope;//采用直接采用纸张页面获取所有要素
            pSpatialFilter.GeometryField = inFeatureClss.ShapeFieldName;
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;



            IFeatureCursor pInFeatureCursor = inFeatureClss.Search(pSpatialFilter, true);
            IFeature pInFeature = null;
            IRelationalOperator pRe0 = pageGeometry.Envelope as IRelationalOperator;//纸张外接矩形
            IRelationalOperator pRe = clipGeometry_out as IRelationalOperator;//裁切面
            ITopologicalOperator pTopologicalOperator = clipGeometry_out as ITopologicalOperator;
            try
            {

                while ((pInFeature = pInFeatureCursor.NextFeature()) != null)
                {
                    IFeatureBuffer pFeatureBuffer = outFeatureClss.CreateFeatureBuffer();

                    attachFieldValue = "1";
                    IGeometry shape_copy = pInFeature.ShapeCopy;
                    if (bProjection)
                        shape_copy.Project(out_sr);//裁切下来的数据进行投影变换


                    IGeometry pGeometryResult = null;
                    IGeometry pGeometryResult1 = null;
                    //1.判断与外接矩形关系
                    if (pRe0.Contains(shape_copy))
                    {
                        #region
                        //页面内
                        pGeometryResult = shape_copy;
                        if (!pRe.Disjoint(shape_copy)) //判断与主区内相交
                        {

                            if (pRe.Contains(pGeometryResult))//在主区内
                            {

                            }
                            else//在主区边缘
                            {
                                IGeometry intersetResult = pTopologicalOperator.Intersect(shape_copy, dim);
                                ITopologicalOperator topo = intersetResult as ITopologicalOperator;
                                topo.Simplify();
                                if (!pGeometryResult.IsEmpty)
                                {
                                    pGeometryResult1 = (pGeometryResult as ITopologicalOperator).Difference(intersetResult);
                                    pGeometryResult = intersetResult;
                                    (pGeometryResult1 as ITopologicalOperator).Simplify();
                                }
                            }
                        }
                        else//位于邻区
                        {
                            pGeometryResult1 = (pGeometryResult as IClone).Clone() as IGeometry;
                            pGeometryResult = null;
                        }
                        #endregion
                    }
                    else //相交 
                    {
                        #region
                        (shape_copy as ITopologicalOperator).Clip(pageGeometry.Envelope);
                        pGeometryResult = shape_copy;

                        //页面内
                        if (!pRe.Disjoint(shape_copy)) //判断与主区内相交
                        {

                            if (pRe.Contains(pGeometryResult))//在主区内
                            {

                            }
                            else//在主区边缘
                            {
                                IGeometry intersetResult = pTopologicalOperator.Intersect(shape_copy, dim);
                                ITopologicalOperator topo = intersetResult as ITopologicalOperator;
                                topo.Simplify();
                                if (!pGeometryResult.IsEmpty)
                                {
                                    pGeometryResult1 = (pGeometryResult as ITopologicalOperator).Difference(intersetResult);
                                    pGeometryResult = intersetResult;
                                    (pGeometryResult1 as ITopologicalOperator).Simplify();
                                }
                            }
                        }
                        else//位于邻区
                        {
                            pGeometryResult1 = (pGeometryResult as IClone).Clone() as IGeometry;
                            pGeometryResult = null;
                        }
                        #endregion

                    }
                    if (pGeometryResult != null && !pGeometryResult.IsEmpty)
                    {  //处理主区
                        #region
                        pFeatureBuffer.Shape = pGeometryResult;
                        for (int i = 0; i < pFeatureBuffer.Fields.FieldCount; i++)
                        {
                            IField pfield = pFeatureBuffer.Fields.get_Field(i);
                            if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                            {
                                continue;
                            }

                            if (pfield.Name == "SHAPE_Length" || pfield.Name == "SHAPE_Area")
                            {
                                continue;
                            }

                            int index = pInFeature.Fields.FindField(pfield.Name);
                            if (index != -1 && pfield.Editable)
                            {
                                pFeatureBuffer.set_Value(i, pInFeature.get_Value(index));
                            }

                        }


                        pFeatureBuffer.set_Value(attachIndex, DBNull.Value);
                        pOutFeatureCursor.InsertFeature(pFeatureBuffer);
                        //内存释放
                        Marshal.ReleaseComObject(pGeometryResult);
                        #endregion
                    }
                    if (pGeometryResult1 != null && !pGeometryResult1.IsEmpty)
                    {
                        //处理邻区
                        #region
                        pFeatureBuffer.Shape = pGeometryResult1;
                        for (int i = 0; i < pFeatureBuffer.Fields.FieldCount; i++)
                        {
                            IField pfield = pFeatureBuffer.Fields.get_Field(i);
                            if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                            {
                                continue;
                            }

                            if (pfield.Name == "SHAPE_Length" || pfield.Name == "SHAPE_Area")
                            {
                                continue;
                            }

                            int index = pInFeature.Fields.FindField(pfield.Name);
                            if (index != -1 && pfield.Editable)
                            {
                                pFeatureBuffer.set_Value(i, pInFeature.get_Value(index));
                            }

                        }
                        pFeatureBuffer.set_Value(attachIndex, "1");
                        pOutFeatureCursor.InsertFeature(pFeatureBuffer);
                        #endregion
                        //内存释放
                        Marshal.ReleaseComObject(pGeometryResult1);
                    }
                    //内存释放
                    Marshal.ReleaseComObject(pInFeature);

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
            pOutFeatureCursor.Flush();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pOutFeatureCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pInFeatureCursor);
            
        }

    }
}

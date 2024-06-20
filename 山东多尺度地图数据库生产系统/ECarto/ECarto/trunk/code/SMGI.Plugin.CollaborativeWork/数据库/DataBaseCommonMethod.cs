using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.CollaborativeWork
{
    public class DataBaseCommonMethod
    {
        /// <summary>
        /// 创建GDB数据库
        /// </summary>
        /// <param name="gdbFullFileName"></param>
        /// <returns></returns>
        public static IWorkspace createGDB(string gdbFullFileName)
        {
            int lastSlashIndex = gdbFullFileName.LastIndexOf("\\");
            int lastDotIndex = gdbFullFileName.LastIndexOf(".");

            string path = gdbFullFileName.Substring(0, lastSlashIndex);
            string databasename = gdbFullFileName.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);

            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspaceName workspaceName = workspaceFactory.Create(path, databasename, null, 0);
            IName name = workspaceName as IName;
            IWorkspace workspace = name.Open() as IWorkspace;

            Marshal.ReleaseComObject(workspaceFactory);

            return workspace;
        }

        /// <summary>
        /// 获取要素类的字段结构信息
        /// </summary>
        /// <param name="pSourceFeatureClass"></param>
        /// <param name="bDelCollaField"></param>
        /// <param name="bFieldNameUpper"></param>
        /// <returns></returns>
        public static IFields getFeatureClassFields(IFeatureClass pSourceFeatureClass, bool bDelCollaField, bool bFieldNameUpper)
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

                if (targetFields.FindField(field.Name) != -1)//已包含该字段（要素类自带字段）
                {
                    continue;
                }

                //剔除sde数据中面积、长度字段
                if (field.Name.ToLower().StartsWith("st_area") || field.Name.ToLower().StartsWith("st_length"))
                {
                    continue;
                }

                if (bDelCollaField)
                {
                    //剔除协同字段
                    if (ServerDataInitializeCommand.CollabGUID == field.Name.ToUpper() || 
                        ServerDataInitializeCommand.CollabVERSION == field.Name.ToUpper() ||
                        ServerDataInitializeCommand.CollabDELSTATE == field.Name.ToUpper() || 
                        ServerDataInitializeCommand.CollabOPUSER == field.Name.ToUpper())
                    {
                        continue;
                    }
                }

                IField newField = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (targetFields as IFieldsEdit).AddField(newField);
            }

            IGeometryDef pGeometryDef = new GeometryDefClass();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.SpatialReference_2 = (pSourceFeatureClass as IGeoDataset).SpatialReference;
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
        /// 创建一个与sourceWorkspace相同结构的空数据库，并返回源数据库与新数据库的要素类列表
        /// </summary>
        /// <param name="pSourceWorkspace"></param>
        /// <param name="outputGDB"></param>
        /// <param name="bDelCollaField"></param>
        /// <param name="bFieldNameUpper"></param>
        /// <param name="fcNameList">要素类集合</param>
        /// <returns></returns>
        public static Dictionary<IFeatureClass, IFeatureClass> CreateGDBStruct(IWorkspace pSourceWorkspace, string outputGDB, bool bDelCollaField, bool bFieldNameUpper, List<string> fcNameList)
        {
            Dictionary<IFeatureClass, IFeatureClass> result = new Dictionary<IFeatureClass, IFeatureClass>();

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
            IDataset pSourceDataset = null;
            while ((pSourceDataset = pSourceEnumDataset.Next()) != null)
            {
                if (pSourceDataset is IFeatureDataset)//要素数据集
                {
                    //创建新数据集
                    IFeatureDataset pOutputFeatureDataset = pOutputFeatureWorkspace.CreateFeatureDataset(pSourceDataset.Name.Split('.').Last(), (pSourceDataset as IGeoDataset).SpatialReference);

                    //遍历子要素类
                    IFeatureDataset pSourceFeatureDataset = pSourceFeatureWorkspace.OpenFeatureDataset(pSourceDataset.Name);
                    IEnumDataset pEnumDatasetF = pSourceFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF = null;
                    while ((pDatasetF = pEnumDatasetF.Next()) != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            if (fcNameList != null && !fcNameList.Contains(pDatasetF.Name.Split('.').Last().ToUpper()))
                            {
                                continue;
                            }

                            IFeatureClass fc = pSourceFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);

                            IFields fields = getFeatureClassFields(fc, bDelCollaField, bFieldNameUpper);
                            esriFeatureType featureType = esriFeatureType.esriFTSimple;
                            string shapeFieldName = fc.ShapeFieldName;

                            //创建新的要素类
                            IFeatureClass newFC = pOutputFeatureDataset.CreateFeatureClass(pDatasetF.Name.Split('.').Last(), fields, null, null, featureType, shapeFieldName, "");

                            result.Add(fc, newFC);
                        }
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pSourceDataset is IFeatureClass)//要素类
                {
                    if (fcNameList != null && !fcNameList.Contains(pSourceDataset.Name.Split('.').Last().ToUpper()))
                    {
                        continue;
                    }

                    IFeatureClass fc = pSourceFeatureWorkspace.OpenFeatureClass(pSourceDataset.Name);

                    IFields fields = getFeatureClassFields(fc, bDelCollaField, bFieldNameUpper);
                    esriFeatureType featureType = esriFeatureType.esriFTSimple;
                    string shapeFieldName = fc.ShapeFieldName;

                    //创建新的要素类
                    IFeatureClass newFC = pOutputFeatureWorkspace.CreateFeatureClass(pSourceDataset.Name.Split('.').Last(), fields, null, null, featureType, shapeFieldName, "");

                    result.Add(fc, newFC);
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pSourceEnumDataset);

            return result;
        }

        /// <summary>
        /// 创建一个与sourceWorkspace相同结构的空数据库(同时增加一个修改状态字段)，并返回源数据库与新数据库的要素类列表
        /// </summary>
        /// <param name="sourceWorkspace"></param>
        /// <param name="outputGDB"></param>
        /// <param name="fcNameList">要素类名称列表</param>
        /// <param name="addStacodField">是否增加STACOD字段</param>
        /// <param name="newFieldName">STACOD字段名</param>
        /// <returns></returns>
        public static Dictionary<IFeatureClass, IFeatureClass> CreateGDBStruct(IWorkspace sourceWorkspace, string outputGDB, List<string> fcNameList, bool addStacodField, string newFieldName, bool bFieldNameUpper = false)
        {
            Dictionary<IFeatureClass, IFeatureClass> result = new Dictionary<IFeatureClass, IFeatureClass>();

            //创建输出工作空间
            if (System.IO.Directory.Exists(outputGDB))
            {
                System.IO.Directory.Delete(outputGDB, true);
            }
            IWorkspace outputWorkspace = createGDB(outputGDB);
            IFeatureWorkspace outputFeatureWorkspace = outputWorkspace as IFeatureWorkspace;

            //创建数据库结构
            IFeatureWorkspace sourceFeatureWorkspace = (IFeatureWorkspace)sourceWorkspace;
            IEnumDataset sourceEnumDataset = sourceWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            sourceEnumDataset.Reset();
            IDataset sourceDataset = null;
            while ((sourceDataset = sourceEnumDataset.Next()) != null)
            {
                if (sourceDataset is IFeatureDataset)//要素数据集
                {
                    //创建新数据集
                    IFeatureDataset outputFeatureDataset = outputFeatureWorkspace.CreateFeatureDataset(sourceDataset.Name.Split('.').Last(), (sourceDataset as IGeoDataset).SpatialReference);

                    //遍历子要素类
                    IFeatureDataset sourceFeatureDataset = sourceDataset as IFeatureDataset;
                    IEnumDataset subEnumDataset = sourceFeatureDataset.Subsets;
                    subEnumDataset.Reset();
                    IDataset subDataset = null;
                    while ((subDataset = subEnumDataset.Next()) != null)
                    {
                        if (subDataset is IFeatureClass)//要素类
                        {
                            if (fcNameList != null && !fcNameList.Contains(subDataset.Name.Split('.').Last().ToUpper()))
                            {
                                continue;
                            }

                            IFeatureClass fc = subDataset as IFeatureClass;

                            IFields fields = getFeatureClassFields(fc, false, bFieldNameUpper);
                            esriFeatureType featureType = esriFeatureType.esriFTSimple;
                            string shapeFieldName = fc.ShapeFieldName;

                            if (addStacodField && fields.FindField(newFieldName) == -1)
                            {
                                IField newField = new FieldClass();
                                IFieldEdit pFieldEdit = (IFieldEdit)newField;
                                pFieldEdit.Name_2 = newFieldName;
                                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                                pFieldEdit.Length_2 = 2;
                                (fields as IFieldsEdit).AddField(newField);
                            }

                            //创建新的要素类
                            IFeatureClass newFC = outputFeatureDataset.CreateFeatureClass(subDataset.Name.Split('.').Last(), fields, null, null, featureType, shapeFieldName, "");

                            result.Add(fc, newFC);
                        }
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(subEnumDataset);
                }
                else if (sourceDataset is IFeatureClass)//要素类
                {
                    if (fcNameList != null && !fcNameList.Contains(sourceDataset.Name.Split('.').Last().ToUpper()))
                    {
                        continue;
                    }

                    IFeatureClass fc = sourceDataset as IFeatureClass;

                    IFields fields = getFeatureClassFields(fc, false, bFieldNameUpper);
                    esriFeatureType featureType = esriFeatureType.esriFTSimple;
                    string shapeFieldName = fc.ShapeFieldName;

                    if (addStacodField && fields.FindField(newFieldName) == -1)
                    {
                        IField newField = new FieldClass();
                        IFieldEdit pFieldEdit = (IFieldEdit)newField;
                        pFieldEdit.Name_2 = newFieldName;
                        pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                        pFieldEdit.Length_2 = 2;
                        (fields as IFieldsEdit).AddField(newField);
                    }

                    //创建新的要素类
                    IFeatureClass newFC = outputFeatureWorkspace.CreateFeatureClass(sourceDataset.Name.Split('.').Last(), fields, null, null, featureType, shapeFieldName, "");

                    result.Add(fc, newFC);
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(sourceEnumDataset);

            return result;
        }

        /// <summary>
        /// 将sourceWS备份
        /// </summary>
        /// <param name="sourceWS">源数据库</param>
        /// <param name="outputGDB">备份数据库路径（GDB）</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public static IWorkspace CopyWorkSpace(IWorkspace sourceWS, string outputGDBPath, WaitOperation wo = null)
        {
            IWorkspace ws = null;

            
            try
            {
                #region 创建输出工作空间
                if (System.IO.Directory.Exists(outputGDBPath))
                {
                    System.IO.Directory.Delete(outputGDBPath, true);
                }
                ws = createGDB(outputGDBPath);
                IFeatureWorkspace pOutputFeatureWorkspace = ws as IFeatureWorkspace;
                #endregion

                var gp = new Geoprocessor { OverwriteOutput = true }; 

                #region 创建数据库结构,并复制内容
                IFeatureWorkspace pSourceFeatureWorkspace = (IFeatureWorkspace)sourceWS;
                IEnumDataset pSourceEnumDataset = sourceWS.get_Datasets(esriDatasetType.esriDTAny);
                pSourceEnumDataset.Reset();
                IDataset pSourceDataset = null;
                while ((pSourceDataset = pSourceEnumDataset.Next()) != null)
                {
                    if (pSourceDataset is IFeatureDataset)//要素数据集
                    {
                        //创建新数据集
                        IFeatureDataset pOutputFeatureDataset = pOutputFeatureWorkspace.CreateFeatureDataset(pSourceDataset.Name.Split('.').Last(), (pSourceDataset as IGeoDataset).SpatialReference);

                        //遍历子要素类
                        IFeatureDataset pSourceFeatureDataset = pSourceFeatureWorkspace.OpenFeatureDataset(pSourceDataset.Name);
                        IEnumDataset pEnumDatasetF = pSourceFeatureDataset.Subsets;
                        pEnumDatasetF.Reset();
                        IDataset pDatasetF = null;
                        while ((pDatasetF = pEnumDatasetF.Next()) != null)
                        {
                            if (pDatasetF is IFeatureClass)//要素类
                            {
                                //复制要素
                                SMGI.Common.Helper.ExecuteGPTool(gp, new CopyFeatures { in_features = sourceWS.PathName + "\\" + pSourceDataset.Name + "\\" + pDatasetF.Name, out_feature_class = ws.PathName + "\\" + pSourceDataset.Name + "\\" + pDatasetF.Name }, null);
                            }
                        }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                    }
                    else if (pSourceDataset is IFeatureClass)//要素类
                    {
                        IFeatureClass fc = pSourceFeatureWorkspace.OpenFeatureClass(pSourceDataset.Name);

                        IFields fields = getFeatureClassFields(fc, false, false);
                        esriFeatureType featureType = esriFeatureType.esriFTSimple;
                        string shapeFieldName = fc.ShapeFieldName;

                        //创建新的要素类
                        IFeatureClass newFC = pOutputFeatureWorkspace.CreateFeatureClass(pSourceDataset.Name.Split('.').Last(), fields, null, null, featureType, shapeFieldName, "");

                        //复制要素
                        SMGI.Common.Helper.ExecuteGPTool(gp, new CopyFeatures { in_features = sourceWS.PathName + "\\" + pSourceDataset.Name, out_feature_class = ws.PathName + "\\" + pSourceDataset.Name }, null);
                    }
                    else if (pSourceDataset is ITable)//表
                    {

                        //复制表
                        SMGI.Common.Helper.ExecuteGPTool(gp, new Copy() { in_data = sourceWS.PathName + "\\" + pSourceDataset.Name, out_data = ws.PathName + "\\" + pSourceDataset.Name.Split('.').Last() }, null);
                    }
                    else if(pSourceDataset is IRasterDataset)
                    {
                    }
                    else
                    {
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pSourceEnumDataset);
                #endregion 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                return null;
            }

            return ws;
        }
    }
}

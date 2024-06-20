using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.DataManagementTools;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Data;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;

namespace SMGI.Plugin.CollaborativeWork
{
    public class commonMethod
    {
        public static long MaxMem
        {
            get
            {
                return 1024 * 1024 * 700;//字节
            }
        }

        public static long MaxMem2
        {
            get
            {
                return 1024 * 1024 * 500;//字节
            }
        }

        /// <summary>
        /// 打开shapefile文件,返回IFeatureClass
        /// </summary>
        /// <param name="shapeFileName"></param>
        /// <returns></returns>
        public static IFeatureClass getFeatureClassFromShapeFile(string shapeFileName)
        {
            try
            {
                IWorkspaceFactory pShapeFileWorkspaceFactory = new ShapefileWorkspaceFactory();
                IFeatureWorkspace pWorkspace = pShapeFileWorkspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(shapeFileName), 0) as IFeatureWorkspace;
                if (null == pWorkspace)
                    return null;

                IFeatureClass pFeatureClass = pWorkspace.OpenFeatureClass(System.IO.Path.GetFileName(shapeFileName));

                return pFeatureClass;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null;
            }

        }

        public static IMemoryBlobStream GeometryToBlob(IGeometry geo)
        {

            IMemoryBlobStream blob = new MemoryBlobStreamClass();
            (geo as IPersistStream).Save(blob as IStream, 0);

            return blob;

        }


        public static IFeatureClass getFeatureClassFromWorkspace(IFeatureWorkspace ws, string fcName)
        {
            if (null == ws || !(ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
                return null;

            return ws.OpenFeatureClass(fcName);
        }

        /// <summary>
        /// 复制模板数据库中的表格（本地状态表、本地记录表）
        /// </summary>
        /// <param name="targertWorkspace"></param>
        /// <returns></returns>
        public static bool CopyLocalTemplateCollaTable(IWorkspace targertWorkspace)
        {
            if (null == targertWorkspace)
                return false;

            var content = GApplication.Application.Template.Content;
            var dataTemplate = content.Element("DataTemplate");

            IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspace templateWS = pWorkspaceFactory.OpenFromFile(GApplication.Application.Template.Root + "\\" + dataTemplate.Value, 0);

            if (!(targertWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, "SMGILocalState"))
            {
                var templateLocalStateTable = (templateWS as IFeatureWorkspace).OpenTable("SMGILocalState");
                if (templateLocalStateTable != null)
                {
                    SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, new Copy() { in_data = templateWS.PathName + "\\SMGILocalState", out_data = targertWorkspace.PathName + "\\SMGILocalState" }, null);
                }
                else
                {
                    MessageBox.Show("复制模板数据库中的SMGILocalState表失败！");
                    return false;
                }
            }

            if (!(targertWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, "RecordTable"))
            {
                var templateRecordTable = (templateWS as IFeatureWorkspace).OpenTable("RecordTable");
                if (templateRecordTable != null)
                {
                    SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, new Copy() { in_data = templateWS.PathName + "\\RecordTable", out_data = targertWorkspace.PathName + "\\RecordTable" }, null);
                }
                else
                {
                    MessageBox.Show("复制模板数据库中的RecordTable表失败！");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 更新本地状态表
        /// </summary>
        /// <param name="targertWorkspace"></param>
        /// <param name="serverVersion"></param>
        /// <param name="ipAddress"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="databaseName"></param>
        /// <param name="extentName"></param>
        /// <param name="extentGeo"></param>
        /// <returns></returns>
        public static bool UpdateLocalStateTable(IWorkspace targertWorkspace, int serverVersion, string ipAddress,
            string userName, string passWord, string databaseName, string extentName, IGeometry extentGeo)
        {
            IMemoryBlobStream geoBlob = commonMethod.GeometryToBlob(extentGeo);//范围面几何

            IEnumDataset tableEnumDataset = targertWorkspace.get_Datasets(esriDatasetType.esriDTTable);
            tableEnumDataset.Reset();
            IDataset tableDataset = null;
            while ((tableDataset = tableEnumDataset.Next()) != null)
            {
                if (tableDataset.Name == "SMGILocalState")
                {
                    ITable recordTable = tableDataset as ITable;
                    recordTable.DeleteSearchedRows(null);

                    //插入记录
                    ICursor pCursor = recordTable.Insert(true);

                    IRowBuffer pRowBuffer = recordTable.CreateRowBuffer();
                    for (int i = 1; i < recordTable.Fields.FieldCount; i++)
                    {
                        string fieldName = pRowBuffer.Fields.get_Field(i).Name;
                        switch (fieldName)
                        {
                            case "BASEVERSION":
                                pRowBuffer.set_Value(i, serverVersion);
                                break;
                            case "IPADDRESS":
                                pRowBuffer.set_Value(i, ipAddress);
                                break;
                            case "USERNAME":
                                pRowBuffer.set_Value(i, userName);
                                break;
                            case "PASSWORD":
                                pRowBuffer.set_Value(i, passWord);
                                break;
                            case "DATABASE":
                                pRowBuffer.set_Value(i, databaseName);
                                break;
                            case "EXTENTNAME":
                                pRowBuffer.set_Value(i, extentName);
                                break;
                            case "EXTENTPOLYGON":
                                pRowBuffer.set_Value(i, (System.Object)geoBlob);
                                break;
                        }
                    }
                    pCursor.InsertRow(pRowBuffer);

                    pCursor.Flush();


                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// 加载工程文件
        /// </summary>
        /// <param name="app"></param>
        /// <param name="fullFileName"></param>
        public static void OpenGDBFile(GApplication app, string fullFileName)
        {
            if (app.Workspace != null)
            {
                MessageBox.Show("已经打开工作区，请先关闭工作区!");
                return;
            }
            if (!GApplication.GDBFactory.IsWorkspace(fullFileName))
            {
                MessageBox.Show("不是有效地GDB文件");
            }
            IWorkspace ws = GApplication.GDBFactory.OpenFromFile(fullFileName, 0);
            if (GWorkspace.IsWorkspace(ws))
            {
                app.OpenESRIWorkspace(ws);
            }
            else
            {
                app.InitESRIWorkspace(ws);
            }
        }

        /// <summary>
        /// 从工作空间中获取指定表的内容
        /// </summary>
        /// <param name="pWorkspace">工作空间</param>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public static DataTable ReadToDataTable(IWorkspace ws, string tableName)
        {
            DataTable dataTable = new DataTable();

            if (null == ws)
                return dataTable;

            if (!(ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, tableName))
                return dataTable;

            var table = (ws as IFeatureWorkspace).OpenTable(tableName);
            if (table == null)
                return dataTable;

            
            //添加表的字段信息
            for (int i = 0; i < table.Fields.FieldCount; i++)
            {
                dataTable.Columns.Add(table.Fields.Field[i].Name);
            }

            //添加数据
            ICursor cursor = table.Search(null, true);
            IRow row = null;
            while ((row = cursor.NextRow()) != null)
            {
                DataRow dr = dataTable.NewRow();

                for (int i = 0; i < row.Fields.FieldCount; i++)
                {
                    object obValue = row.get_Value(i);
                    if (obValue != null && !Convert.IsDBNull(obValue))
                    {
                        dr[i] = row.get_Value(i);
                    }
                    else
                    {
                        dr[i] = "";
                    }
                }

                dataTable.Rows.Add(dr);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);

            return dataTable;
        }

        /// <summary>
        /// 删除数据库中的空要素类
        /// </summary>
        /// <param name="fws"></param>
        /// <returns>若所有要素类都被删除，则返回true</returns>
        public static bool DeleteNullFeatureClass(IFeatureWorkspace fws)
        {
            bool result = true;

            IEnumDataset enumDataset = (fws as IWorkspace).get_Datasets(esriDatasetType.esriDTAny);
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
                            if (fc.FeatureCount(null) == 0)
                            {
                                subDataset.Delete();
                            }
                            else
                            {
                                result = false;
                            }

                        }
                    }
                    Marshal.ReleaseComObject(subEnumDataset);
                }
                else if (dataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc = dataset as IFeatureClass;
                    if (fc.FeatureCount(null) == 0)
                    {
                        dataset.Delete();
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            Marshal.ReleaseComObject(enumDataset);


            return result;
        }
    }
}

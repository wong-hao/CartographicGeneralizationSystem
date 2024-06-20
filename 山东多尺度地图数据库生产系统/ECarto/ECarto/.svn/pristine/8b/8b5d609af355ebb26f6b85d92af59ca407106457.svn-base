using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.CollaborativeWork
{
    /// <summary>
    /// 协同作业的本地数据库(GDB)
    /// </summary>
    public class LocalDataBase
    {
        private GApplication _app;//应用程序
        private IWorkspace _workspace;//本地工作空间

        public LocalDataBase(GApplication app, IWorkspace workspace)
        {
            _app = app;
            _workspace = workspace;
        }

        /// <summary>
        /// 提交本地更新，更新本地数据库（更新矢量数据集、更新记录表、本地状态表）
        /// </summary>
        /// <param name="serverLatestVersion"></param>
        /// <param name="LocalBaseVersion"></param>
        /// <param name="serverConflictFeatures"></param>
        /// <param name="localConflictFeatures"></param>
        /// <returns></returns>
        public bool updateDataBase(int serverLatestVersion, int LocalBaseVersion, List<string> serverConflictFeatures, List<string> localConflictFeatures)
        {
            //删除本地数据库中有冲突的要素，保持与服务器最新数据一致
            if (!deleteConflictFeatures(LocalBaseVersion, serverConflictFeatures, localConflictFeatures))
                return false;

            //更新本地数据库中更新要素的版本号信息
            if (!updateEditFeatureVersion(serverLatestVersion))
                return false;

            //清空本地数据库的记录表
            clearRecordTable();

            //更新本地状态表
            updateLocalStateTable(serverLatestVersion);

            return true;
        }

        /// <summary>
        /// 删除协同数据
        /// </summary>
        /// <param name="LocalBaseVersion"></param>
        /// <returns></returns>
        public bool deleteCollaborateFeatures(int LocalBaseVersion)
        {
            IFeatureWorkspace localFeatureWorkspace = _workspace as IFeatureWorkspace;
            if (null == localFeatureWorkspace)
            {
                MessageBox.Show("无效的本地数据库！");
                return false;
            }


            try
            {
                IEnumDataset localEnumDataset = _workspace.get_Datasets(esriDatasetType.esriDTAny);
                localEnumDataset.Reset();
                IDataset localDataset = null;
                while ((localDataset = localEnumDataset.Next()) != null)
                {
                    if (localDataset is IFeatureDataset)//要素数据集
                    {
                        IFeatureDataset pFeatureDataset = localDataset as IFeatureDataset;
                        IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                        pEnumDatasetF.Reset();
                        IDataset pDatasetF = null;
                        while ((pDatasetF = pEnumDatasetF.Next()) != null)
                        {
                            if (pDatasetF is IFeatureClass)//要素类
                            {
                                IFeatureClass fc = pDatasetF as IFeatureClass;
                                if (null == fc || fc.FindField(ServerDataInitializeCommand.CollabVERSION) == -1)
                                    continue;

                                IQueryFilter qf = new QueryFilterClass();
                                qf.WhereClause = string.Format("{0} > {1}", ServerDataInitializeCommand.CollabVERSION, LocalBaseVersion);//协同数据
                                IFeatureCursor pCursor = fc.Update(qf, true);//Recycling参数要慎重
                                IFeature fe = null;
                                while ((fe = pCursor.NextFeature()) != null)
                                {
                                    //删除大于基本号的数据(协同)
                                    pCursor.DeleteFeature();

                                }

                                Marshal.ReleaseComObject(pCursor);
                            }
                        }

                        Marshal.ReleaseComObject(pEnumDatasetF);
                    }
                    else if (localDataset is IFeatureClass)//要素类
                    {
                        IFeatureClass fc = localDataset as IFeatureClass;
                        if (null == fc || fc.FindField(ServerDataInitializeCommand.CollabVERSION) == -1)
                            continue;

                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("{0} > {1}", ServerDataInitializeCommand.CollabVERSION, LocalBaseVersion);//协同数据
                        IFeatureCursor pCursor = fc.Update(qf, true);//Recycling参数要慎重
                        IFeature fe = null;
                        while ((fe = pCursor.NextFeature()) != null)
                        {
                            //删除大于基本号的数据(协同)
                            pCursor.DeleteFeature();

                        }

                        Marshal.ReleaseComObject(pCursor);
                    }
                    else
                    {

                    }

                }

                Marshal.ReleaseComObject(localEnumDataset);
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
        /// 插入协调数据
        /// </summary>
        /// <param name="sdeWorkspace">服务器工作空间</param>
        /// <param name="collabData"> Dictionary<FeatureClassName, Dictionary<GUID, FeatureInfo>> </param>
        /// <returns></returns>
        public bool insertCollabData(IWorkspace sdeWorkspace, Dictionary<string, Dictionary<string, FeatureInfo>> collabData)
        {
            IFeatureWorkspace localFeatureWorkspace = _workspace as IFeatureWorkspace;
            if (null == localFeatureWorkspace)
            {
                MessageBox.Show("无效的本地数据库！");
                return false;
            }

            IFeatureWorkspace sdeFeatureWorkspace = sdeWorkspace as IFeatureWorkspace;
            if (null == sdeFeatureWorkspace)
            {
                MessageBox.Show("无效的服务器数据库！");
                return false;
            }

            try
            {
                foreach (var key in collabData.Keys)
                {
                    if (!(localFeatureWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, key.Split('.').Last()))
                    {
                        continue;//本地数据库中不存在该要素类
                    }
                    var localfc = localFeatureWorkspace.OpenFeatureClass(key.Split('.').Last());
                    var sdefc = sdeFeatureWorkspace.OpenFeatureClass(key);

                    bool bLoadMode = (localfc as IFeatureClassLoad).LoadOnlyMode;
                    try
                    {
                        string oidSet = "";
                        foreach (var item in collabData[key])
                        {
                            if (oidSet != "")
                                oidSet += string.Format(",{0}", item.Value.oid);
                            else
                                oidSet = string.Format("{0}", item.Value.oid);
                        }

                        if (oidSet != "")
                        {
                            IFeatureCursor localFeatureCursor = localfc.Insert(true);

                            
                            IFeature fe = null;

                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = string.Format("OBJECTID in ({0})", oidSet);
                            IFeatureCursor sdeFeatureCursor = sdefc.Search(qf, true);
                            while ((fe = sdeFeatureCursor.NextFeature()) != null)
                            {
                                int delIndex = sdefc.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE);
                                string delState = fe.get_Value(delIndex).ToString();
                                if (delState != ServerDataInitializeCommand.DelStateText)
                                {
                                    //IFeatureBuffer pFeatureBuffer = fe as IFeatureBuffer;//有时会报错

                                    IFeatureBuffer pFeatureBuffer = localfc.CreateFeatureBuffer();
                                    //几何赋值
                                    pFeatureBuffer.Shape = fe.Shape;
                                    //属性赋值
                                    for (int i = 0; i < pFeatureBuffer.Fields.FieldCount; i++)
                                    {
                                        IField pfield = pFeatureBuffer.Fields.get_Field(i);
                                        if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                                        {
                                            continue;
                                        }

                                        if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                                        {
                                            continue;
                                        }

                                        int index = fe.Fields.FindField(pfield.Name);
                                        if (index != -1 && pfield.Editable)
                                        {
                                            pFeatureBuffer.set_Value(i, fe.get_Value(index));
                                        }

                                    }

                                    if (!(localfc as IFeatureClassLoad).LoadOnlyMode)
                                    {
                                        (localfc as IFeatureClassLoad).LoadOnlyMode = true;
                                    }

                                    localFeatureCursor.InsertFeature(pFeatureBuffer);
                                }
                            }
                            localFeatureCursor.Flush();

                            System.Runtime.InteropServices.Marshal.ReleaseComObject(localFeatureCursor);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(sdeFeatureCursor);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (bLoadMode != (localfc as IFeatureClassLoad).LoadOnlyMode)
                        {
                            (localfc as IFeatureClassLoad).LoadOnlyMode = false;
                        }
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
        /// 向本地数据库插入要素
        /// </summary>
        /// <param name="features"></param>
        /// <returns></returns>
        public bool insertFeatures(List<IFeature> features)
        {
            IFeatureWorkspace localFeatureWorkspace = _workspace as IFeatureWorkspace;
            if (null == localFeatureWorkspace)
            {
                MessageBox.Show("无效的本地数据库！");
                return false;
            }

            try
            {
                Dictionary<string, List<IFeature>> fcName2Features = new Dictionary<string, List<IFeature>>();
                foreach (var f in features)
                {
                    if (null == f)
                        continue;

                    string fcName = f.Class.AliasName.Split('.').Last();
                    if (!fcName2Features.ContainsKey(fcName))
                    {
                        List<IFeature> feas = new List<IFeature>();
                        feas.Add(f);
                        fcName2Features[fcName] = feas;
                    }
                    else
                    {
                        fcName2Features[fcName].Add(f);
                    }
                }

                foreach (var item in fcName2Features)
                {
                    var fc = localFeatureWorkspace.OpenFeatureClass(item.Key);
                    if (null == fc)
                        continue;

                    var pload = fc as IFeatureClassLoad;
                    pload.LoadOnlyMode = true;

                    IFeatureCursor pCursor = fc.Insert(true);
                    foreach (var f in item.Value)
                    {
                        if (f != null)
                        {
                            int delIndex = f.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE);
                            string delState = f.get_Value(delIndex).ToString();
                            if (delState != ServerDataInitializeCommand.DelStateText)
                            {
                                //IFeatureBuffer pFeatureBuffer = f as IFeatureBuffer;

                                IFeatureBuffer pFeatureBuffer = fc.CreateFeatureBuffer();
                                //几何赋值
                                pFeatureBuffer.Shape = f.Shape;
                                //属性赋值
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

                                    int index = f.Fields.FindField(pfield.Name);
                                    if (index != -1 && pfield.Editable)
                                    {
                                        pFeatureBuffer.set_Value(i, f.get_Value(index));
                                    }

                                }

                                pCursor.InsertFeature(pFeatureBuffer);
                            }
                        }
                    }
                    pCursor.Flush();

                    Marshal.ReleaseComObject(pCursor);
                    pload.LoadOnlyMode = false;
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
        /// 删除指定本地要素(localVer)，并将协调下来的要素(serverVer)的版本号改为本地要素的版本号
        /// </summary>
        /// <param name="fcName"></param>
        /// <param name="guid"></param>
        /// <param name="localVer"></param>
        /// <param name="serverVer">协调下来的要素版本号</param>
        /// <returns></returns>
        public bool replaceFeature(string fcName, string guid, int localVer, int serverVer)
        {
            IFeatureWorkspace localFeatureWorkspace = _workspace as IFeatureWorkspace;
            if (null == localFeatureWorkspace)
            {
                MessageBox.Show("无效的本地数据库！");
                return false;
            }


            try
            {
                IFeatureClass fc = localFeatureWorkspace.OpenFeatureClass(fcName);
                if (null == fc)
                {
                    MessageBox.Show(string.Format("无效的要素类：{0}！", fcName));
                    return false;
                }

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("{0} = '{1}'", ServerDataInitializeCommand.CollabGUID, guid);
                IFeatureCursor pCursor = fc.Update(qf, true);
                IFeature f;
                while ((f = pCursor.NextFeature()) != null)
                {
                    int ver = 0;
                    int.TryParse(f.get_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out ver);
                    if (ver == localVer)
                    {
                        pCursor.DeleteFeature();//删除
                    }
                    else if (ver == serverVer)
                    {
                        //修改属性
                        f.set_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabVERSION), localVer);
                        f.set_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabOPUSER), System.Environment.MachineName);

                        pCursor.UpdateFeature(f);
                    }
                }

                Marshal.ReleaseComObject(pCursor);
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
        /// 获取本地数据库的范围多边形
        /// </summary>
        /// <returns></returns>
        public IPolygon getExtentPolygon()
        {
            IPolygon extPlg = null;

            if (!(_workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, "SMGILocalState"))
            {
                MessageBox.Show("没有找到SMGILocalState表！");
                return extPlg;
            }

            ITable localDT = (_workspace as IFeatureWorkspace).OpenTable("SMGILocalState");
            ICursor rowCursor = localDT.Search(null, false);
            IRow row = rowCursor.NextRow();
            Marshal.ReleaseComObject(rowCursor);

            if (row == null)
                return extPlg;

            //通过shp路径查找范围面
            int index = localDT.Fields.FindField("EXTENTNAME");
            if (index != -1)
            {
                string extentFileName = row.get_Value(index).ToString();
                IFeatureClass fc = commonMethod.getFeatureClassFromShapeFile(extentFileName);
                if (fc != null)
                {
                    IFeatureCursor feCursor = fc.Search(null, false);
                    IFeature fe = feCursor.NextFeature();
                    Marshal.ReleaseComObject(feCursor);

                    extPlg = fe.Shape as IPolygon;
                }
            }

            if (extPlg != null)
                return extPlg;//已经获取了范围面,返回

            index = localDT.Fields.FindField("EXTENTPOLYGON");
            if (row != null && index != -1)
            {
                IMemoryBlobStream blob = row.get_Value(index) as IMemoryBlobStream;
                IPersistStream ps = new PolygonClass();
                ps.Load(blob as IStream);

                extPlg = ps as IPolygon;
            }

            return extPlg;
        }

        /// <summary>
        /// 更新本地数据库，使之与服务器中的最新数据保持一致，主要删除两种数据：
        /// 1，删除与本地数据库中有冲突的协同数据，保留对应guid的本地数据
        /// 2，删除本地非编辑数据中已不是最新版本的数据，保留对应guid的协调数据
        /// </summary>
        /// <param name="LocalBaseVersion"></param>
        /// <param name="serverConflictFeatures">服务器中已更新的要素(guid)/param>
        /// <param name="localConflictFeatures">本地被编辑过，且与服务器有冲突的要素(guid)</param>
        /// <returns></returns>
        private bool deleteConflictFeatures(int LocalBaseVersion, List<string> serverConflictFeatures, List<string> localConflictFeatures)
        {
            IFeatureWorkspace localFeatureWorkspace = _workspace as IFeatureWorkspace;
            if (null == localFeatureWorkspace)
            {
                MessageBox.Show("无效的本地数据库！");
                return false;
            }


            try
            {
                IEnumDataset localEnumDataset = _workspace.get_Datasets(esriDatasetType.esriDTAny);
                localEnumDataset.Reset();
                IDataset localDataset = null;
                while ((localDataset = localEnumDataset.Next()) != null)
                {
                    if (localDataset is IFeatureDataset)//要素数据集
                    {
                        IFeatureDataset pFeatureDataset = localDataset as IFeatureDataset;
                        IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                        pEnumDatasetF.Reset();
                        IDataset pDatasetF = null;
                        while ((pDatasetF = pEnumDatasetF.Next()) != null)
                        {
                            if (pDatasetF is IFeatureClass)//要素类
                            {
                                IFeatureClass fc = pDatasetF as IFeatureClass;
                                if (null == fc || fc.FindField(ServerDataInitializeCommand.CollabVERSION) == -1
                                    || fc.FindField(ServerDataInitializeCommand.CollabGUID) == -1)
                                    continue;

                                IFeatureCursor pCursor = fc.Update(null, true);//Recycling参数要慎重
                                IFeature fe = null;
                                while ((fe = pCursor.NextFeature()) != null)
                                {
                                    int ver = 0;
                                    int.TryParse(fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out ver);
                                    string guid = fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString();

                                    if (ver > LocalBaseVersion)//协调数据
                                    {
                                        if (localConflictFeatures.Contains(guid))//有冲突的协同数据
                                        {
                                            pCursor.DeleteFeature();//删除
                                        }
                                    }
                                    else//本地数据
                                    {
                                        if (!localConflictFeatures.Contains(guid) && serverConflictFeatures.Contains(guid))//本地非编辑数据，且服务器中已有更高版本的数据
                                        {
                                            pCursor.DeleteFeature();//删除
                                        }
                                    }
                                }

                                Marshal.ReleaseComObject(pCursor);
                            }
                        }

                        Marshal.ReleaseComObject(pEnumDatasetF);
                    }
                    else if (localDataset is IFeatureClass)//要素类
                    {
                        IFeatureClass fc = localDataset as IFeatureClass;
                        if (null == fc || fc.FindField(ServerDataInitializeCommand.CollabVERSION) == -1 
                            || fc.FindField(ServerDataInitializeCommand.CollabGUID) == -1)
                            continue;

                        IFeatureCursor pCursor = fc.Update(null, true);//Recycling参数要慎重
                        IFeature fe = null;
                        while ((fe = pCursor.NextFeature()) != null)
                        {
                            int ver = 0;
                            int.TryParse(fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out ver);
                            string guid = fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString();

                            if (ver > LocalBaseVersion)//协调数据
                            {
                                if (localConflictFeatures.Contains(guid))//有冲突的协同数据
                                {
                                    pCursor.DeleteFeature();//删除
                                }
                            }
                            else//本地数据
                            {
                                if (!localConflictFeatures.Contains(guid) && serverConflictFeatures.Contains(guid))//本地非编辑数据，且服务器中已有更高版本的数据
                                {
                                    pCursor.DeleteFeature();//删除
                                }
                            }
                        }

                        Marshal.ReleaseComObject(pCursor);
                    }
                    else
                    {

                    }
                }
                Marshal.ReleaseComObject(localEnumDataset);
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

        //提交更新后，更新本地编辑要素的版本号
        private bool updateEditFeatureVersion(int serverLatestVersion)
        {
            IFeatureWorkspace localFeatureWorkspace = _workspace as IFeatureWorkspace;
            if (null == localFeatureWorkspace)
            {
                MessageBox.Show("无效的本地数据库！");
                return false;
            }


            try
            {
                IEnumDataset localEnumDataset = _workspace.get_Datasets(esriDatasetType.esriDTAny);
                localEnumDataset.Reset();
                IDataset localDataset = null;
                while ((localDataset = localEnumDataset.Next()) != null)
                {
                    if (localDataset is IFeatureDataset)//要素数据集
                    {
                        IFeatureDataset pFeatureDataset = localDataset as IFeatureDataset;
                        IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                        pEnumDatasetF.Reset();
                        IDataset pDatasetF = null;
                        while ((pDatasetF = pEnumDatasetF.Next()) != null)
                        {
                            if (pDatasetF is IFeatureClass)//要素类
                            {
                                IFeatureClass fc = pDatasetF as IFeatureClass;
                                if (null == fc || fc.FindField(ServerDataInitializeCommand.CollabVERSION) == -1)
                                    continue;

                                IQueryFilter qf = new QueryFilterClass();
                                qf.WhereClause = string.Format("{0} < 0", ServerDataInitializeCommand.CollabVERSION);
                                IFeatureCursor pCursor = fc.Update(qf, true);//Recycling参数要慎重
                                IFeature fe = null;
                                while ((fe = pCursor.NextFeature()) != null)
                                {
                                    int delIndex = fe.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE);
                                    string delState = fe.get_Value(delIndex).ToString();
                                    if (delState == ServerDataInitializeCommand.DelStateText)
                                    {
                                        pCursor.DeleteFeature();
                                        continue;
                                    }

                                    fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION), Convert.ToInt64(serverLatestVersion));
                                    pCursor.UpdateFeature(fe);

                                }

                                Marshal.ReleaseComObject(pCursor);
                            }
                        }

                        Marshal.ReleaseComObject(pEnumDatasetF);
                    }
                    else if (localDataset is IFeatureClass)//要素类
                    {
                        IFeatureClass fc = localDataset as IFeatureClass;
                        if (null == fc || fc.FindField(ServerDataInitializeCommand.CollabVERSION) == -1)
                            continue;

                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("{0} < 0", ServerDataInitializeCommand.CollabVERSION);
                        IFeatureCursor pCursor = fc.Update(qf, true);//Recycling参数要慎重
                        IFeature fe = null;
                        while ((fe = pCursor.NextFeature()) != null)
                        {
                            int delIndex = fe.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE);
                            string delState = fe.get_Value(delIndex).ToString();
                            if (delState == ServerDataInitializeCommand.DelStateText)
                            {
                                pCursor.DeleteFeature();
                                continue;
                            }

                            fe.set_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION), Convert.ToInt64(serverLatestVersion));
                            pCursor.UpdateFeature(fe);

                        }

                        Marshal.ReleaseComObject(pCursor);
                    }
                    else
                    {

                    }


                }
                Marshal.ReleaseComObject(localEnumDataset);
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
        /// 清空记录表
        /// </summary>
        private void clearRecordTable()
        {
            if (null == _workspace)
                return;

            IEnumDataset dtableEnumDataset = _workspace.get_Datasets(esriDatasetType.esriDTTable);
            dtableEnumDataset.Reset();
            IDataset dtableDataset = null;
            while ((dtableDataset = dtableEnumDataset.Next()) != null)
            {
                if (dtableDataset.Name == "RecordTable")
                {
                    ITable drtable = dtableDataset as ITable;
                    ICursor drCursor = drtable.Search(null, false);
                    IRow dRow = null;
                    while ((dRow = drCursor.NextRow()) != null)
                    {
                        dRow.Delete();
                    }

                    Marshal.ReleaseComObject(drCursor);
                }
            }
        }

        /// <summary>
        /// 更新本地状态表
        /// </summary>
        /// <param name="serverLatestVersion"></param>
        private void updateLocalStateTable(int serverLatestVersion)
        {
            if (null == _workspace)
                return;


            IEnumDataset tableEnumDataset = _workspace.get_Datasets(esriDatasetType.esriDTTable);
            tableEnumDataset.Reset();
            IDataset tableDataset = null;
            while ((tableDataset = tableEnumDataset.Next()) != null)
            {
                if (tableDataset.Name == "SMGILocalState")
                {
                    ITable table = tableDataset as ITable;

                    ICursor pCursor = table.Update(null, false);
                    IRow pRow = pCursor.NextRow();
                    if (pRow != null)
                    {
                        int index = pRow.Fields.FindField("BASEVERSION");
                        pRow.set_Value(index, serverLatestVersion);

                        index = pRow.Fields.FindField("COLLABSTATE");
                        pRow.set_Value(index, "");

                        pCursor.UpdateRow(pRow);
                    }
                    Marshal.ReleaseComObject(pCursor);
                }
            }
        }

    }
}

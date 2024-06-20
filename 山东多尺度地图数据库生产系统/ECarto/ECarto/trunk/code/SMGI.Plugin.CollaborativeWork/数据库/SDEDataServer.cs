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
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Npgsql;

namespace SMGI.Plugin.CollaborativeWork
{
    /// <summary>
    /// 协同作业的数据服务器
    /// </summary>
    public class SDEDataServer
    {
        public static string StacodFieldName
        {
            get
            {
                return "STACOD";
            }
        }

        private static NpgsqlConnection _conn = null;//Npgsql连接器

        private GApplication _app;//应用程序
        private string _ipAddress;//服务器IP
        private string _userName;//用户名
        private string _passWord;//密码
        private string _databaseName;//数据库名
        private string _port;//端口

        public IWorkspace SDEWorkspace
        {
            get
            {
                if (null == _sdeWorkspace)
                {
                    _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                }

                return _sdeWorkspace;
            }
        }
        private IWorkspace _sdeWorkspace;//工作空间


        public SDEDataServer(GApplication app, string ipAddress, string userName, string passWord, string databaseName, string port = "5432")
        {
            _app = app;

            _ipAddress = ipAddress;
            try
            {
                var arrIPAddresses = Dns.GetHostAddresses(ipAddress);//通过计算机名或地址获取IP地址
                foreach (var ip in arrIPAddresses)
                {
                    if (ip.AddressFamily.Equals(AddressFamily.InterNetwork))
                        _ipAddress = ip.ToString();
                }

            }
            catch
            {
            }
            _userName = userName;
            _passWord = passWord;
            _databaseName = databaseName;
            _port = port;
        }

        /// <summary>
        /// 下载协同数据
        /// </summary>
        /// <param name="outputGDB"></param>
        /// <param name="rangeGeometry"></param>
        /// <param name="bFieldNameUpper">下载数据时，是否把要素类的字段名转换为大写</param>
        /// <param name="fcNameList">指定下载的要素类列表，默认为空，则下载所有要素类</param>
        /// <param name="version">指定下载数据库的版本，默认为-1，则下载服务器数据库的现势数据</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public bool DownLoadCollaborativeData(string outputGDB, IGeometry rangeGeometry, bool bFieldNameUpper, List<string> fcNameList = null, int version = -1, WaitOperation wo = null)
        {
            try
            {
                //获取源数据库工作空间
                if (null == _sdeWorkspace)
                {
                    if (wo != null)
                        wo.SetText("正在连接服务器...");
                    _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                    if (null == _sdeWorkspace)
                    {
                        return false;
                    }
                }

                //创建一个相同结构的空数据库
                if (wo != null)
                    wo.SetText("正在创建数据库...");
                Dictionary<IFeatureClass, IFeatureClass> fcList = DataBaseCommonMethod.CreateGDBStruct(_sdeWorkspace, outputGDB, false, bFieldNameUpper, fcNameList);

                //插入要素
                foreach (var item in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在提取{0}...", item.Value.AliasName));

                    copyIntersectVesionFeatures(item.Key, item.Value, rangeGeometry, false, false, version);
                    
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
        /// 从服务数据库中导出指定版本间的增量数据
        /// </summary>
        /// <param name="outputGDB"></param>
        /// <param name="rangeGeometry"></param>
        /// <param name="bFieldNameUpper"></param>
        /// <param name="startVerion">起始版本</param>
        /// <param name="endVersion">终止版本，若为默认值-1，则为服务器数据库中各要素的最大版本</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public bool ExportDataBetweenVersion(string outputGDB, IGeometry rangeGeometry, bool bFieldNameUpper, int startVerion, int endVersion = -1, WaitOperation wo = null)
        {
            try
            {
                Dictionary<IFeatureClass, IFeatureClass> fcList = null;//<服务器数据库中的要素类，增量数据库中的要素类>

                #region 获取源数据库工作空间
                if (null == _sdeWorkspace)
                {
                    if (wo != null)
                        wo.SetText("正在连接服务器...");
                    _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                    if (null == _sdeWorkspace)
                    {
                        return false;
                    }
                }
                #endregion

                
                #region 依据服务器数据库结构，创建导出的增量包数据库结构
                //创建一个相同结构的空数据库
                if (wo != null)
                    wo.SetText("正在创建数据库...");
                fcList = DataBaseCommonMethod.CreateGDBStruct(_sdeWorkspace, outputGDB, null, true, SDEDataServer.StacodFieldName, bFieldNameUpper);
                foreach (var item in fcList)
                {
                    if (item.Key.FindField(ServerDataInitializeCommand.CollabVERSION) == -1 ||
                        item.Key.FindField(ServerDataInitializeCommand.CollabGUID) == -1)
                    {
                        MessageBox.Show(string.Format("服务器数据库中的要素类【{0}】找不到协同字段！", item.Key.AliasName), "提示", MessageBoxButtons.OK);
                        return false;
                    }

                    if (item.Value.FindField(SDEDataServer.StacodFieldName) == -1)
                    {
                        MessageBox.Show(string.Format("增量数据库中的要素类【{0}】找不到修改状态字段【{1}】！", item.Key.AliasName, SDEDataServer.StacodFieldName), "提示", MessageBoxButtons.OK);
                        return false;
                    }
                }
                #endregion

                #region 遍历要素类提取增量数据
                foreach (var item in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在提取{0}...", item.Value.AliasName));

                    extractUpdatedData(item.Key, rangeGeometry, startVerion, endVersion, item.Value);

                }
                #endregion
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
        /// 提交本地更新，更新服务器，（插入要素，更新RecordTable、SMGIServerState表）
        /// </summary>
        /// <param name="fcName2editedFeatures">本地编辑要素集合</param>
        /// <param name="localRecordTable"></param>
        /// <param name="instrduction"></param>
        /// <param name="opName"></param>
        /// <returns>返回本次提交的版本号，若为-1，则表示提交失败</returns>
        public int UpdateDatabase(Dictionary<string, List<int>> fcName2editedFeatures, DataTable localRecordTable, string instrduction, string opName, WaitOperation wo = null)
        {
            //获取源数据库工作空间
            if (null == _sdeWorkspace)
            {
                _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                if (null == _sdeWorkspace)
                {
                    return -1;
                }
            }

            IFeatureWorkspace sdeFeatureWorkspace = _sdeWorkspace as IFeatureWorkspace;
            if (null == sdeFeatureWorkspace)
            {
                MessageBox.Show("不是有效的矢量工作空间！");
                return -1;
            }

            //获取服务器最大版本号
            int serverMaxVersion = getServerMaxVersion();
            if (-1 == serverMaxVersion)
            {
                MessageBox.Show("获取服务器的最大版本号失败！");
                return -1;
            }
            int newVersion = serverMaxVersion + 1;

            //开启编辑
            var pWorkspaceEdit = sdeFeatureWorkspace as IWorkspaceEdit;
            pWorkspaceEdit.StartEditing(true);
            pWorkspaceEdit.StartEditOperation();

            //插入要素
            if (!insertCollaborateFeatureToDataBase(sdeFeatureWorkspace, fcName2editedFeatures, newVersion, wo))
            {
                pWorkspaceEdit.AbortEditOperation();
                pWorkspaceEdit.StopEditing(false);

                return -1;
            }

            if(wo != null)
                wo.SetText("正在更新服务器数据库的状态表...");

            //更新SMGIServerState表
            if (!updateSMGIServerStateTable(_sdeWorkspace, _databaseName, _userName, opName, newVersion, instrduction))
            {
                pWorkspaceEdit.AbortEditOperation();
                pWorkspaceEdit.StopEditing(false);

                return -1;
            }


            //更新RecordTable表
            if (!insertLocalTableToServer(localRecordTable, _sdeWorkspace, _databaseName, _userName))
            {
                pWorkspaceEdit.AbortEditOperation();
                pWorkspaceEdit.StopEditing(false);

                return -1;
            }

            

            //结束编辑
            pWorkspaceEdit.StopEditOperation();
            pWorkspaceEdit.StopEditing(true);

            return newVersion;
        }

        /// <summary>
        /// 获取服务器最大版本号
        /// </summary>
        /// <returns></returns>
        public int getServerMaxVersion()
        {
            if (null == _sdeWorkspace)
            {
                _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                if (null == _sdeWorkspace)
                {
                    return -1;
                }
            }

            int maxVer = ServerDataInitializeCommand.getServerDBMaxVersion(_sdeWorkspace);

            return maxVer;
        }


        /// <summary>
        /// 提取数据
        /// </summary>
        /// <param name="outputGDB"></param>
        /// <param name="rangeGeometry"></param>
        /// <param name="bFieldNameUpper"></param>
        /// <param name="bDelCollaField"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public bool ExtractAllData(string outputGDB, IGeometry rangeGeometry, bool bFieldNameUpper, bool bDelCollaField, WaitOperation wo = null)
        {
            try
            {
                //获取源数据库工作空间
                if (null == _sdeWorkspace)
                {
                    if (wo != null)
                        wo.SetText("正在连接服务器...");
                    _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                    if (null == _sdeWorkspace)
                    {
                        return false;
                    }
                }

                //创建一个相同结构的空数据库
                if (wo != null)
                    wo.SetText("正在创建数据库...");
                Dictionary<IFeatureClass, IFeatureClass> fcList = DataBaseCommonMethod.CreateGDBStruct(_sdeWorkspace, outputGDB, bDelCollaField, bFieldNameUpper, null);

                //插入要素
                foreach (var item in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在提取{0}...", item.Value.AliasName));

                    copyIntersectVesionFeatures(item.Key, item.Value, rangeGeometry);
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
        /// 提取数据
        /// </summary>
        /// <param name="outputGDB"></param>
        /// <param name="rangeGeometry"></param>
        /// <param name="bDelCollaField"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public bool ExtractUpdatedData(string outputGDB, IGeometry rangeGeometry, bool bFieldNameUpper, bool bDelCollaField, WaitOperation wo = null)
        {
            try
            {
                //获取源数据库工作空间
                if (null == _sdeWorkspace)
                {
                    if (wo != null)
                        wo.SetText("正在连接服务器...");
                    _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                    if (null == _sdeWorkspace)
                    {
                        return false;
                    }
                }

                //创建一个相同结构的空数据库
                if (wo != null)
                    wo.SetText("正在创建数据库...");
                Dictionary<IFeatureClass, IFeatureClass> fcList = DataBaseCommonMethod.CreateGDBStruct(_sdeWorkspace, outputGDB, bDelCollaField, bFieldNameUpper, null);

                //插入要素
                foreach (var item in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在提取{0}...", item.Value.AliasName));

                    copyIntersectVesionFeatures(item.Key, item.Value, rangeGeometry, true);
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
        /// 级联更新增量包数据导出
        /// </summary>
        /// <param name="referGDB">原始参照数据库路径</param>
        /// <param name="outputGDB">增量包数据库导出路径</param>
        /// <param name="rangeGeometry">提取范围</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public bool DCDUpdatedDataExport(string referGDB, string outputGDB, IGeometry rangeGeometry, WaitOperation wo = null, bool cbDatabase = false, string nextGDB = null)
        {
            bool res = true;

            try
            {
                #region 获取服务器数据库
                if (null == _sdeWorkspace)
                {
                    if (wo != null)
                        wo.SetText("正在连接服务器...");
                    _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                    if (null == _sdeWorkspace)
                    {
                        return false;
                    }
                }
                #endregion

                IWorkspace referWorkspace = null;//参考数据库
                #region 读取参考数据库
                if (wo != null)
                    wo.SetText("正在读取原始参考数据库...");
                var wsf = new FileGDBWorkspaceFactoryClass();
                if (!(Directory.Exists(referGDB) && wsf.IsWorkspace(referGDB)))
                {
                    MessageBox.Show("原始参考数据库读取失败!");
                    return false;
                }
                referWorkspace = wsf.OpenFromFile(referGDB, 0);
                #endregion
                #region 读取下一级别参考数据库
                IWorkspace nextreferWorkspace = null;//下一级别参考数据库
                if (cbDatabase)
                {
                    if (wo != null)
                        wo.SetText("正在读取下一级别参考数据库...");
                    var nextwsf = new FileGDBWorkspaceFactoryClass();
                    if (!(Directory.Exists(nextGDB) && wsf.IsWorkspace(nextGDB)))
                    {
                        MessageBox.Show("下一级别参考数据库读取失败!");
                        return false;
                    }
                    nextreferWorkspace = nextwsf.OpenFromFile(nextGDB, 0);
                }
                #endregion
                int dbMaxVersion = 0;
                #region 获取参考数据库的基版本号
                if (wo != null)
                    wo.SetText("正在获取参考数据库的基版本号...");
                IEnumDataset enumDataset = referWorkspace.get_Datasets(esriDatasetType.esriDTAny);
                enumDataset.Reset();
                IDataset dataset = null;
                while ((dataset = enumDataset.Next()) != null)
                {
                    if (dataset is IFeatureDataset)//要素数据集
                    {
                        IFeatureDataset feaDataset = dataset as IFeatureDataset;
                        IEnumDataset subEnumDataset = feaDataset.Subsets;
                        subEnumDataset.Reset();
                        IDataset subDataset = null;
                        while ((subDataset = subEnumDataset.Next()) != null)
                        {
                            if (subDataset is IFeatureClass)//要素类
                            {
                                IFeatureClass fc = subDataset as IFeatureClass;
                                if (fc.FindField(ServerDataInitializeCommand.CollabVERSION) == -1 ||
                                    fc.FindField(ServerDataInitializeCommand.CollabGUID) == -1)
                                {
                                    var dr = MessageBox.Show(string.Format("原始参考数据库中的要素类【{0}】不存在协同字段，是否跳过该要素类？", fc.AliasName), "提示", MessageBoxButtons.YesNo);
                                    if (dr == DialogResult.No)
                                    {
                                        System.Runtime.InteropServices.Marshal.ReleaseComObject(subEnumDataset);
                                        System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);

                                        return false;
                                    }
                                    else
                                        continue;
                                }

                                int ver = ServerDataInitializeCommand.GetFeatureClassMaxVersion(fc);
                                if (ver > dbMaxVersion)
                                    dbMaxVersion = ver;
                            }
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(subEnumDataset);
                    }
                    else if (dataset is IFeatureClass)//要素类
                    {
                        IFeatureClass fc = dataset as IFeatureClass;
                        if (fc.FindField(ServerDataInitializeCommand.CollabVERSION) == -1 ||
                                    fc.FindField(ServerDataInitializeCommand.CollabGUID) == -1)
                        {
                            var dr = MessageBox.Show(string.Format("原始参考数据库中的要素类【{0}】不存在协同字段，是否跳过该要素类？", fc.AliasName), "提示", MessageBoxButtons.YesNo);
                            if (dr == DialogResult.No)
                            {
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);

                                return false;
                            }
                            else
                                continue;
                        }

                        int ver = ServerDataInitializeCommand.GetFeatureClassMaxVersion(fc);
                        if (ver > dbMaxVersion)
                            dbMaxVersion = ver;

                    }

                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);
                #endregion

                Dictionary<IFeatureClass, IFeatureClass> fcList = null;//<服务器数据库中的要素类，增量数据库中的要素类>
                #region 依据服务器数据库结构，创建导出的增量包数据库结构
                //创建一个相同结构的空数据库
                if (wo != null)
                    wo.SetText("正在创建数据库...");
                fcList = DataBaseCommonMethod.CreateGDBStruct(_sdeWorkspace, outputGDB, null, true, SDEDataServer.StacodFieldName);
                foreach (var item in fcList)
                {
                    if (item.Key.FindField(ServerDataInitializeCommand.CollabVERSION) == -1 ||
                        item.Key.FindField(ServerDataInitializeCommand.CollabGUID) == -1)
                    {
                        MessageBox.Show(string.Format("服务器数据库中的要素类【{0}】找不到协同字段！", item.Key.AliasName), "提示", MessageBoxButtons.OK);
                        return false;
                    }

                    if (item.Value.FindField(SDEDataServer.StacodFieldName) == -1)
                    {
                        MessageBox.Show(string.Format("增量数据库中的要素类【{0}】找不到修改状态字段【{1}】！", item.Key.AliasName, SDEDataServer.StacodFieldName), "提示", MessageBoxButtons.OK);
                        return false;
                    }
                }
                #endregion

                #region 按图层依次导出增量数据的输出数据库
                foreach (var item in fcList)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在提取要素类【{0}】中的增量数据...", item.Value.AliasName));

                    Dictionary<string, KeyValuePair<int, string>> referGUID2Version = null;
                    #region 获取参考数据库该要素类中所有要素与最大版本号的映射字典
                    string fcName = item.Value.AliasName;
                    IFeatureClass referFC = null;
                    if (!(referWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
                    {
                        MessageBox.Show(string.Format("原始参考数据库中没有找到要素类【{0}】!", fcName));
                        return false;
                    }

                    referFC = (referWorkspace as IFeatureWorkspace).OpenFeatureClass(fcName);
                    //获取该要素类中各要素的最大版本号
                    referGUID2Version = getGUIDAndVerOfLatestFeatures(referFC);
                    #endregion
                    IFeatureClass nextfc = null;
                    if (nextreferWorkspace != null) { nextfc = (nextreferWorkspace as IFeatureWorkspace).OpenFeatureClass(item.Value.AliasName); }
                    extractUpdatedData(item.Key, rangeGeometry, referGUID2Version, dbMaxVersion, item.Value, nextfc);
                }
                #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                res = false;
            }
            finally
            {
            }

            return res;
        }


        /// <summary>
        /// 获取服务器中指定范围，且要素版本号大于localVersion的冲突数据（每个guid要素保证仅一条）
        /// </summary>
        /// <param name="rangeGeometry">范围</param>
        /// <param name="localVersion">本地版本号</param>
        /// <returns> Dictionary<FeatureClassName, Dictionary<GUID, FeatureInfo>> </returns>
        public Dictionary<string, Dictionary<string, FeatureInfo>> getConflictFeatures(IGeometry rangeGeometry, int localVersion)
        {
            Dictionary<string, Dictionary<string, FeatureInfo>> dicResult = new Dictionary<string, Dictionary<string, FeatureInfo>>();

            try
            {
                if (null == _sdeWorkspace)
                {
                    _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                    if (null == _sdeWorkspace)
                    {
                        return null;
                    }
                }

                IFeatureWorkspace sdeFeatureWorkspace = _sdeWorkspace as IFeatureWorkspace;
                IEnumDataset sdeEnumDataset = _sdeWorkspace.get_Datasets(esriDatasetType.esriDTAny);
                sdeEnumDataset.Reset();
                IDataset pDataset = null;
                while ((pDataset = sdeEnumDataset.Next()) != null)
                {
                    if (pDataset is IFeatureDataset)//要素数据集
                    {
                        IFeatureDataset pFeatureDataset = sdeFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                        IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                        pEnumDatasetF.Reset();
                        IDataset pDatasetF = pEnumDatasetF.Next();
                        while (pDatasetF != null)
                        {
                            if (pDatasetF is IFeatureClass)//要素类
                            {
                                IFeatureClass fc = pDatasetF as IFeatureClass;

                                Dictionary<string, FeatureInfo> guid2featureinfo = getConflictFeatures(fc, rangeGeometry, localVersion);

                                dicResult.Add((fc as IDataset).Name, guid2featureinfo);
                            }

                            pDatasetF = pEnumDatasetF.Next();
                        }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                    }
                    else if (pDataset is IFeatureClass)//要素类
                    {
                        IFeatureClass fc = pDataset as IFeatureClass;

                        Dictionary<string, FeatureInfo> guid2featureinfo = getConflictFeatures(fc, rangeGeometry, localVersion);

                        dicResult.Add((fc as IDataset).Name, guid2featureinfo);
                    }
                    else
                    {

                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(sdeEnumDataset);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                return null;
            }

            return dicResult;
        }

        /// <summary>
        /// 从服务器制定的要素类中获取指定guid、版本号大于baseVersion的最新要素
        /// </summary>
        /// <param name="fcName"></param>
        /// <param name="guid"></param>
        /// <param name="baseVersion"></param>
        /// <returns></returns>
        public IFeature getLatestFeature(string fcName, string guid, int baseVersion)
        {
            IFeature reulst = null;

            try
            {
                if (null == _sdeWorkspace)
                {
                    _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                    if (null == _sdeWorkspace)
                    {
                        return null;
                    }
                }

                IFeatureWorkspace pSDEFeatureWorkspace = _sdeWorkspace as IFeatureWorkspace;
                IFeatureClass fc = pSDEFeatureWorkspace.OpenFeatureClass(string.Format("{0}.{1}.{2}", _databaseName.Trim(), _userName.Trim(), fcName.Trim()));
                if (null == fc)
                    return null;

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("{0} = '{1}' and {2} > {3}",
                    ServerDataInitializeCommand.CollabGUID, guid, ServerDataInitializeCommand.CollabVERSION, baseVersion);
                IFeatureCursor pFeatureCursor = fc.Search(qf, false);
                IFeature feature = null;
                int maxVer = -1;
                while ((feature = pFeatureCursor.NextFeature()) != null)
                {
                    var ver = int.Parse(feature.get_Value(feature.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString());
                    if (ver > maxVer)
                    {
                        maxVer = ver;
                        reulst = feature;
                    }
                }
                Marshal.ReleaseComObject(pFeatureCursor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                return null;
            }

            return reulst;
        }


        /// <summary>
        /// 返回要素类中的冲突要素
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="rangeGeometry"></param>
        /// <param name="localVersion"></param>
        /// <returns>Dictionary<GUID, FeatureInfo></returns>
        private Dictionary<string, FeatureInfo> getConflictFeatures(IFeatureClass fc, IGeometry rangeGeometry, int localVersion)
        {
            Dictionary<string, FeatureInfo> result = new Dictionary<string, FeatureInfo>();

            //获取该要素类中所有要素的最新版本信息
            Dictionary<string, KeyValuePair<int, string>> guid2Ver = getGUIDAndVerOfLatestFeatures(fc);
            if (0 == guid2Ver.Count)
                return result;

            int guidIndex = fc.Fields.FindField(ServerDataInitializeCommand.CollabGUID);
            int verIndex = fc.Fields.FindField(ServerDataInitializeCommand.CollabVERSION);
            int delIndex = fc.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE);
            int opuserIndex = fc.Fields.FindField(ServerDataInitializeCommand.CollabOPUSER);

            //检索冲突要素
            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = rangeGeometry;
            pSpatialFilter.GeometryField = "SHAPE";
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            pSpatialFilter.WhereClause = string.Format("{0} > {1}", ServerDataInitializeCommand.CollabVERSION, localVersion);

            IFeatureCursor pCursor = fc.Search(pSpatialFilter, false);
            IFeature fe = null;
            while ((fe = pCursor.NextFeature()) != null)
            {
                string guid = fe.get_Value(guidIndex).ToString();
                int ver = 0;
                int.TryParse(fe.get_Value(verIndex).ToString(), out ver);

                if (!guid2Ver.ContainsKey(guid) || guid2Ver[guid].Key != ver)
                {
                    continue;//不是最新版本要素
                }

                string delState = fe.get_Value(delIndex).ToString();
                string user = fe.get_Value(opuserIndex).ToString();
                FeatureInfo info = new FeatureInfo { oid = fe.OID, collabVer = ver,  collabDel = delState, opuser = user };
                result[guid] = info;//冲突要素
                //try
                //{
                //    result.Add(guid, info);//冲突要素
                //}
                //catch (Exception ex)
                //{
                //    string err = ex.Message;
                //}

            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);

            return result;
        }

        /// <summary>
        /// 将协同客户端提交的要素插入到服务器数据库的数据集中
        /// </summary>
        /// <param name="sdeFeatureWorkspace"></param>
        /// <param name="fcName2editedFeatures">本地编辑要素集合</param>
        /// <param name="mewVersion">本次提交数据被赋予的新版本号</param>
        /// <returns></returns>
        private bool insertCollaborateFeatureToDataBase(IFeatureWorkspace sdeFeatureWorkspace, Dictionary<string, List<int>> fcName2editedFeatures, int mewVersion, WaitOperation wo = null)
        {
            bool res = true;
            IFeature f = null;
            IFeatureClassLoad pload = null;
            try
            {
                foreach (var item in fcName2editedFeatures)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在更新服务器数据库的要素类【{0}】...", item.Key));

                    IFeatureClass localFC = commonMethod.getFeatureClassFromWorkspace(CollaborativeTask.Instance.LocalWorkspace as IFeatureWorkspace, item.Key);
                    var fc = sdeFeatureWorkspace.OpenFeatureClass(string.Format("{0}.{1}.{2}", _databaseName.Trim(), _userName.Trim(), item.Key.Trim()));

                    pload = fc as IFeatureClassLoad;
                    pload.LoadOnlyMode = true;

                    #region 插入要素

                    IFeatureCursor pFeatureCursor = fc.Insert(true);
                    foreach (var subitem in item.Value)
                    {
                        f = localFC.GetFeature(subitem);
                        IGeometry shape = f.Shape;

                        int ver = 0;
                        int.TryParse(f.get_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out ver);
                        f.set_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabVERSION), mewVersion);

                        string delState = f.get_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE)).ToString();
                        if (delState == ServerDataInitializeCommand.DelStateText)//删除要素
                        {
                            if (shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline
                                && (shape.IsEmpty || (shape as IPolyline).Length == 0))
                            {
                                //删除的线要素长度为0，需要进行处理，否则提交不上
                                IPointCollection pc = shape as IPointCollection;
                                IPoint pt1 = null;
                                if (pc.PointCount > 1)
                                {
                                    pt1 = pc.get_Point(0);
                                }
                                else
                                {
                                    pt1 = (fc as IGeoDataset).Extent.LowerLeft;
                                }
                                IPoint pt2 = (pt1 as IClone).Clone() as IPoint;
                                pt2.X = pt2.X + 0.01;

                                IPointCollection newPC = new PolylineClass();
                                newPC.AddPoint(pt1);
                                newPC.AddPoint(pt2);
                                shape = newPC as IPolyline;
                            }
                            else if (shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon
                                && (shape.IsEmpty || (shape as IArea).Area == 0))
                            {
                                //删除的面要素面积为0，需要进行处理，否则提交不上
                                IPointCollection pc = shape as IPointCollection;
                                IPoint pt1 = null;
                                if (pc.PointCount > 1)
                                {
                                    pt1 = pc.get_Point(0);
                                }
                                else
                                {
                                    pt1 = (fc as IGeoDataset).Extent.LowerLeft;
                                }
                                IPoint pt2 = (pt1 as IClone).Clone() as IPoint;
                                pt2.X = pt2.X + 0.01;
                                IPoint pt3 = (pt1 as IClone).Clone() as IPoint;
                                pt3.Y = pt3.Y + 0.01;

                                IPointCollection newPC = new PolylineClass();
                                newPC.AddPoint(pt1);
                                newPC.AddPoint(pt2);
                                newPC.AddPoint(pt3);
                                shape = newPC as IPolygon;
                            }


                            if (shape.GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
                            {
                                #region 为防止出现面未正常闭合等情况的出现
                                var polygon = shape as IPolygon;
                                if (!polygon.IsClosed)
                                    polygon.Close();//面未闭合
                                #endregion
                            }

                        }

                        //IFeatureBuffer pFeatureBuffer = f as IFeatureBuffer;

                        IFeatureBuffer pFeatureBuffer = fc.CreateFeatureBuffer();
                        //几何赋值
                        pFeatureBuffer.Shape = shape;
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

                            int index = f.Fields.FindField(pfield.Name);
                            if (index != -1 && pfield.Editable)
                            {
                                pFeatureBuffer.set_Value(i, f.get_Value(index));
                            }

                        }

                        pFeatureCursor.InsertFeature(pFeatureBuffer);

                        Marshal.ReleaseComObject(f);
                    }
                    pFeatureCursor.Flush();

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);

                    #endregion

                    pload.LoadOnlyMode = false;
                    pload = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                if (f != null)
                {
                    MessageBox.Show(string.Format("更新要素类‘{0}’中的要素‘{1}’时失败：{2}", f.Class.AliasName, f.OID, ex.Message));
                }
                else
                {
                    MessageBox.Show(string.Format("更新服务器数据库失败：{0}", ex.Message));
                }

                if (pload != null)
                {
                    pload.LoadOnlyMode = false;
                }
                
                res = false;
            }

            return res;
        }

        /// <summary>
        /// 更新服务器的RecordTable表
        /// </summary>
        /// <param name="localRecordTable"></param>
        /// <param name="sdeworkspace"></param>
        /// <param name="databaseName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        private bool insertLocalTableToServer(DataTable localRecordTable, IWorkspace sdeworkspace, string databaseName, string userName)
        {
            if (localRecordTable.Rows.Count < 0)
                return true;

            bool res = true;
            try
            {
                //获取RecordTable表
                IEnumDataset tableEnumDataset = sdeworkspace.get_Datasets(esriDatasetType.esriDTTable);
                tableEnumDataset.Reset();
                IDataset tableDataset = null;
                ITable recordTable = null;
                while ((tableDataset = tableEnumDataset.Next()) != null)
                {
                    if (tableDataset.Name == string.Format("{0}.{1}.RecordTable", databaseName.Trim(), userName.Trim()))
                    {
                        recordTable = tableDataset as ITable;
                        break;
                    }
                }


                //插入记录
                ICursor pCursor = recordTable.Insert(true);
                for (int k = 0; k < localRecordTable.Rows.Count; k++)
                {
                    try
                    {
                        IRowBuffer pRowBuffer = recordTable.CreateRowBuffer();
                        for (int i = 1; i < recordTable.Fields.FieldCount; i++)
                        {
                            string fieldName = pRowBuffer.Fields.get_Field(i).Name.ToUpper();
                            if (!localRecordTable.Columns.Contains(fieldName))
                                continue;

                            pRowBuffer.set_Value(i, localRecordTable.Rows[k][fieldName].ToString());

                        }
                        pCursor.InsertRow(pRowBuffer);
                    }
                    catch
                    {
                        continue;
                    }
                }
                pCursor.Flush();

                //释放
                Marshal.ReleaseComObject(pCursor);
                Marshal.ReleaseComObject(tableEnumDataset);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                res = false;
            }


            return res;

        }

        /// <summary>
        /// 更新服务器的StateTable表
        /// </summary>
        /// <param name="sdeworkspace"></param>
        /// <param name="databaseName"></param>
        /// <param name="userName">数据库用户名</param>
        /// <param name="opUser">操作者机器名</param>
        /// <param name="newVersion"></param>
        /// <param name="instrduction"></param>
        /// <param name="versionstate"></param>
        /// <returns></returns>
        private bool updateSMGIServerStateTable(IWorkspace sdeworkspace, string databaseName, string userName, string opUser, int newVersion, string instrduction, string versionstate= "")
        {
            bool res = true;
            try
            {
                //获取RecordTable表
                IEnumDataset tableEnumDataset = sdeworkspace.get_Datasets(esriDatasetType.esriDTTable);
                tableEnumDataset.Reset();
                IDataset tableDataset = null;
                ITable stateTable = null;
                while ((tableDataset = tableEnumDataset.Next()) != null)
                {
                    if (tableDataset.Name == string.Format("{0}.{1}.SMGIServerState", databaseName.Trim(), userName.Trim()))
                    {
                        stateTable = tableDataset as ITable;
                        break;
                    }
                }

                //插入一行
                ICursor pCursor = stateTable.Insert(true);
                IRowBuffer pRowBuffer = stateTable.CreateRowBuffer();
                for (int i = 1; i < stateTable.Fields.FieldCount; i++)
                {
                    var fieldName = stateTable.Fields.get_Field(i).Name;
                    switch (fieldName.ToUpper())
                    {
                        case "VERSID":
                            pRowBuffer.set_Value(i, newVersion);
                            break;
                        case "OPUSER":
                            pRowBuffer.set_Value(i, opUser);
                            break;
                        case "OPTIME":
                            pRowBuffer.set_Value(i, DateTime.Now);
                            break;
                        case "INSTRUCTION":
                            pRowBuffer.set_Value(i, instrduction);
                            break;
                        case "VERSIONSTATE":
                            pRowBuffer.set_Value(i, versionstate);
                            break;
                    }

                }
                pCursor.InsertRow(pRowBuffer);
                pCursor.Flush();

                //释放
                Marshal.ReleaseComObject(pCursor);
                Marshal.ReleaseComObject(tableEnumDataset);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                res = false;
            }

            return res;
        }


        /// <summary>
        /// 将输入要素类中的最新版本（或指定版本）要素插入到输出要素类
        /// </summary>
        /// <param name="inFeatureClss"></param>
        /// <param name="outFeatureClss"></param>
        /// <param name="rangeGeometry">提取范围</param>
        /// <param name="bOnlyUpdated">是否仅复制更新要素的最新版本，否则复制全部要素的最新版本</param>
        /// <param name="bContainDeleted">是否需要包含删除状态的要素</param>
        /// <param name="version">需下载的版本数据库的版本号，默认为-1，则表示下载当前最新版本要素</param>
        private void copyIntersectVesionFeatures(IFeatureClass inFeatureClss, IFeatureClass outFeatureClss, IGeometry rangeGeometry, bool bOnlyUpdated = false, bool bContainDeleted = true, int version = -1)
        {
            IFeatureCursor pOutFeatureCursor = null;

            int guidIndex = inFeatureClss.Fields.FindField(ServerDataInitializeCommand.CollabGUID);
            int verIndex = inFeatureClss.Fields.FindField(ServerDataInitializeCommand.CollabVERSION);
            int delIndex = inFeatureClss.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE);
            if (-1 == guidIndex || -1 == verIndex || -1 == delIndex)
            {
                return;//不存在协同字段，无法提取数据
            }
            //制图中心特殊要求
            int feaidIndex = inFeatureClss.Fields.FindField("FEAID");
            int stacodIndex = inFeatureClss.Fields.FindField("STACOD");

            Dictionary<string, KeyValuePair<int, string>> guid2Ver = new Dictionary<string, KeyValuePair<int, string>>();
            if (version < 0)//下载现势数据
            {
                //获取该要素类中所有要素的最新版本信息
                guid2Ver = getGUIDAndVerOfLatestFeatures(inFeatureClss);
            }
            else//下载版本数据
            {
                //获取该要素类中所有要素的指定版本信息（不大于指定版本的最新要素）
                guid2Ver = getGUIDAndVerOfVersionFeatures(inFeatureClss, version);
            }
            if (0 == guid2Ver.Count)
                return;

            //裁切几何体
            ISpatialReference fcSpatialReference = (outFeatureClss as IGeoDataset).SpatialReference;
            if (rangeGeometry != null && fcSpatialReference.Name != rangeGeometry.SpatialReference.Name)
                rangeGeometry.Project(fcSpatialReference);//投影变换

            //输出要素类
            pOutFeatureCursor = outFeatureClss.Insert(true);
            

            //检索制定范围的最新要素
            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            if (rangeGeometry != null)
            {
                pSpatialFilter.Geometry = rangeGeometry;
                pSpatialFilter.GeometryField = "SHAPE";
                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            }
            pSpatialFilter.WhereClause = "";


            IFeatureCursor pInFeatureCursor = null;
            IFeature pInFeature = null;
            try
            {
                pInFeatureCursor = inFeatureClss.Search(pSpatialFilter, true);
                while ((pInFeature = pInFeatureCursor.NextFeature()) != null)
                {
                    IFeatureBuffer pFeatureBuffer = outFeatureClss.CreateFeatureBuffer();

                    string guid = pInFeature.get_Value(guidIndex).ToString();
                    int ver = 0;
                    int.TryParse(pInFeature.get_Value(verIndex).ToString(), out ver);
                    string delState = pInFeature.get_Value(delIndex).ToString();

                    if (!guid2Ver.ContainsKey(guid) || guid2Ver[guid].Key != ver)
                    {
                        continue;//不是最新版本要素(或大于指定版本)
                    }

                    //根据协同作业原理：ver=0的要素不是增量要素。
                    if (bOnlyUpdated && 0 == ver)
                    {
                        if (stacodIndex != -1)
                        {
                            //制图中心要求：制图中心的数据为半成品数据（在其他软件中部分数据已被编辑），对于ver=0的数据，若stacod为修改、增加、删除也被认为是增量要素
                            string stacod = pInFeature.get_Value(stacodIndex).ToString();
                            if (stacod != "删除" && stacod != "修改" && stacod != "增加")
                            {
                                continue;//不是增量要素
                            }
                        }
                        else
                        {
                            continue;//不是增量要素
                        }
                    }

                    if (!bContainDeleted && delState == ServerDataInitializeCommand.DelStateText)
                    {
                        continue;//已删除要素
                    }

                    if (delState == ServerDataInitializeCommand.DelStateText && feaidIndex != -1)//制图中心要求：在提取删除要素时，剔除feaid为空值的要素
                    {
                        object feaid = pInFeature.get_Value(feaidIndex);
                        if (null == feaid || Convert.IsDBNull(feaid) || string.Empty == feaid.ToString())
                        {
                            continue;//feaid为空值的删除要素
                        }
                    }

                    //几何赋值
                    pFeatureBuffer.Shape = pInFeature.Shape;

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

                        int index = pInFeature.Fields.FindField(pfield.Name);
                        if (index != -1 && pfield.Editable)
                        {
                            pFeatureBuffer.set_Value(i, pInFeature.get_Value(index));
                        }

                    }
                    pOutFeatureCursor.InsertFeature(pFeatureBuffer);

                    Marshal.ReleaseComObject(pInFeature);
                    
                    //内存监控
                    if (Environment.WorkingSet > commonMethod.MaxMem)
                    {
                        GC.Collect();
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                string err = ex.Message;
                if (pInFeature != null)
                {
                    err = string.Format("下载要素类【{0}】中的要素【{1}】时出现错误:", inFeatureClss.AliasName, pInFeature.OID) + err;
                }
                MessageBox.Show(err);
            }
            pOutFeatureCursor.Flush();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pOutFeatureCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pInFeatureCursor);
        }

        /// <summary>
        /// 将要素类inFC中指定版本间的增量数据导出到目标要素类outFC
        /// </summary>
        /// <param name="inFC">存在多版本的服务器要素类</param>
        /// <param name="rangeGeometry"></param>
        /// <param name="startVersion"></param>
        /// <param name="endVersion">若为-1，则表示为数据库的现势版本</param>
        /// <param name="outFC"></param>
        private void extractUpdatedData(IFeatureClass inFC, IGeometry rangeGeometry, int startVersion, int endVersion, IFeatureClass outFC)
        {
            int guidIndex = inFC.Fields.FindField(ServerDataInitializeCommand.CollabGUID);
            int verIndex = inFC.Fields.FindField(ServerDataInitializeCommand.CollabVERSION);
            int delIndex = inFC.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE);
            if (-1 == guidIndex || -1 == verIndex || -1 == delIndex)
                return;//不存在协同字段或修改状态字段，无法提取数据

            if (outFC.Fields.FindField(SDEDataServer.StacodFieldName) == -1)
                return;//输出要素类中不存在修改状态字段，无法导出增量状态


            Dictionary<string, KeyValuePair<int, string>> guid2EndVer = new Dictionary<string, KeyValuePair<int, string>>();
            List<string> newGUIDList = new List<string>();
            List<string> modifyGUIDList = new List<string>();
            List<string> delGUIDList = new List<string>();
            #region 获取要素类中两版本间的所有增量要素信息
            //获取要素类起始版本的所有要素信息
            Dictionary<string, KeyValuePair<int, string>> guid2StartVer = getGUIDAndVerOfVersionFeatures(inFC, startVersion);

            //获取要素类终止版本的所有要素信息
            if (endVersion < 0)//现势版本
            {
                guid2EndVer = getGUIDAndVerOfLatestFeatures(inFC);
            }
            else//指定版本
            {
                guid2EndVer = getGUIDAndVerOfVersionFeatures(inFC, endVersion);
            }

            //遍历终止版本的所有要素,获取增量要素信息
            foreach(var kv in guid2EndVer)
            {
                if(kv.Value.Value == ServerDataInitializeCommand.DelStateText)//该要素在终止版本中为删除状态
                {
                    if(guid2StartVer.ContainsKey(kv.Key) && guid2StartVer[kv.Key].Value != ServerDataInitializeCommand.DelStateText)//该要素在起始版本中为非删除状态
                    {
                        delGUIDList.Add(kv.Key);//标记为删除
                    }
                }
                else//该要素在终止版本中为非删除状态
                {
                    if (!guid2StartVer.ContainsKey(kv.Key) || guid2StartVer[kv.Key].Value == ServerDataInitializeCommand.DelStateText)//该要素在起始版本中不存在或为删除状态
                    {
                        newGUIDList.Add(kv.Key);//标记为新增
                    }
                    else //该要素在起始版本中也为非删除状态
                    {
                        if (guid2EndVer[kv.Key].Key > guid2StartVer[kv.Key].Key)//终止版本库大于起始版本库的版本号
                        {
                            modifyGUIDList.Add(kv.Key);//标记为修改
                        }
                    }
                }
            }
            ////遍历起始版本的所有要素【同一个库，不会出现终止版本中不存在而起始版本中存在的情况】
            //foreach (var kv in guid2StartVer)
            //{
            //    if (kv.Value.Value != ServerDataInitializeCommand.DelStateText && !guid2EndVer.ContainsKey(kv.Key))//该要素在起始版本中为非删除状态，而在终止版本中不存在
            //    {
            //        //标记为删除
            //    }
            //}
            #endregion
            if (newGUIDList.Count == 0 && modifyGUIDList.Count == 0 && delGUIDList.Count == 0)
                return;//没有更新要素

            //裁切几何体
            ISpatialReference fcSpatialReference = (outFC as IGeoDataset).SpatialReference;
            if (rangeGeometry != null && fcSpatialReference.Name != rangeGeometry.SpatialReference.Name)
                rangeGeometry.Project(fcSpatialReference);//投影变换

            //检索条件
            ISpatialFilter sf = new SpatialFilterClass();
            string guidSet = "";
            foreach (var guid in newGUIDList)
            {
                if (guidSet != "")
                    guidSet += string.Format(",'{0}'", guid);
                else
                    guidSet = string.Format("'{0}'", guid);
            }
            foreach (var guid in modifyGUIDList)
            {
                if (guidSet != "")
                    guidSet += string.Format(",'{0}'", guid);
                else
                    guidSet = string.Format("'{0}'", guid);
            }
            foreach (var guid in delGUIDList)
            {
                if (guidSet != "")
                    guidSet += string.Format(",'{0}'", guid);
                else
                    guidSet = string.Format("'{0}'", guid);
            }
            if (guidSet != "")
                sf.WhereClause = string.Format("{0} in ({1})", ServerDataInitializeCommand.CollabGUID, guidSet);
            if (rangeGeometry != null)
            {
                sf.Geometry = rangeGeometry;
                sf.GeometryField = "SHAPE";
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            }

            //提取要素
            IFeatureClassLoad pload = outFC as IFeatureClassLoad;
            if (pload != null)
                pload.LoadOnlyMode = true;

            IFeatureCursor outFeCursor = outFC.Insert(true);


            IFeatureCursor inFeCursor = null;
            IFeature inFe = null;
            try
            {
                inFeCursor = inFC.Search(sf, true);
                while ((inFe = inFeCursor.NextFeature()) != null)
                {
                    string stacodValue = "";

                    string guid = inFe.get_Value(guidIndex).ToString();
                    int ver = 0;
                    int.TryParse(inFe.get_Value(verIndex).ToString(), out ver);
                    string delstate = inFe.get_Value(delIndex).ToString();

                    if (!newGUIDList.Contains(guid) && !modifyGUIDList.Contains(guid) && !delGUIDList.Contains(guid))
                        continue;//非增量要素

                    if (!guid2EndVer.ContainsKey(guid) || guid2EndVer[guid].Key != ver)
                        continue;//不是指定的终止版本


                    if (newGUIDList.Contains(guid))
                    {
                        stacodValue = "增加";
                    }
                    else if (modifyGUIDList.Contains(guid))
                    {
                        stacodValue = "修改";
                    }
                    else if (delGUIDList.Contains(guid))
                    {
                        stacodValue = "删除";
                    }
                    else
                    {
                        continue;
                    }

                   
                    IFeatureBuffer outFeBuffer = outFC.CreateFeatureBuffer();

                    //几何赋值
                    outFeBuffer.Shape = inFe.Shape;
 
                    //属性赋值
                    for (int i = 0; i < outFeBuffer.Fields.FieldCount; i++)
                    {
                        IField field = outFeBuffer.Fields.get_Field(i);
                        if (field.Type == esriFieldType.esriFieldTypeGeometry || field.Type == esriFieldType.esriFieldTypeOID)
                            continue;

                        if (field.Name.ToUpper() == "SHAPE_LENGTH" || field.Name.ToUpper() == "SHAPE_AREA")
                            continue;

                        int index = inFe.Fields.FindField(field.Name);
                        if (index != -1 && field.Editable)
                        {
                            outFeBuffer.set_Value(i, inFe.get_Value(index));
                        }

                        //修改状态字段
                        if (field.Name.ToUpper() == SDEDataServer.StacodFieldName)
                        {
                            outFeBuffer.set_Value(i, stacodValue);
                        }

                    }
                    outFeCursor.InsertFeature(outFeBuffer);
                }

                outFeCursor.Flush();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                string err = ex.Message;
                if (inFe != null)
                {
                    err = string.Format("提取服务器要素类【{0}】中的要素【{1}】时出现错误:", inFC.AliasName, inFe.OID) + err;
                }

                throw new Exception(err);
            }
            finally
            {
                if (pload != null)
                    pload.LoadOnlyMode = false;

                System.Runtime.InteropServices.Marshal.ReleaseComObject(outFeCursor);
                if (inFeCursor != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(inFeCursor);
            }

        }

        /// <summary>
        /// 将输入要素类inFC中指定范围内的增量数据导出到目标要素类outFC中
        /// </summary>
        /// <param name="inFC">服务器中的输入要素类</param>
        /// <param name="rangeGeometry">范围面</param>
        /// <param name="referGUID2Version">原始参考数据库中要素的版本号</param>
        /// <param name="dbMaxVersion">原始参考数据库的基版本号</param>
        /// <param name="outFeatureClss"></param>
        private void extractUpdatedData(IFeatureClass inFC, IGeometry rangeGeometry, Dictionary<string, KeyValuePair<int, string>> referGUID2Version, int dbMaxVersion, IFeatureClass outFC, IFeatureClass nextFC)
        {
            int guidIndex = inFC.Fields.FindField(ServerDataInitializeCommand.CollabGUID);
            int verIndex = inFC.Fields.FindField(ServerDataInitializeCommand.CollabVERSION);
            int delIndex = inFC.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE);
            if (-1 == guidIndex || -1 == verIndex || -1 == delIndex)
                return;//不存在协同字段或修改状态字段，无法提取数据

            if (outFC.Fields.FindField(SDEDataServer.StacodFieldName) == -1)
                return;//输出要素类中不存在修改状态字段，无法导出增量状态

            //获取该要素类中所有要素的最新版本信息
            Dictionary<string, KeyValuePair<int, string>> guid2Ver = getGUIDAndVerOfLatestFeatures(inFC);
            if (0 == guid2Ver.Count)
                return;

            //裁切几何体
            ISpatialReference fcSpatialReference = (outFC as IGeoDataset).SpatialReference;
            if (rangeGeometry != null && fcSpatialReference.Name != rangeGeometry.SpatialReference.Name)
                rangeGeometry.Project(fcSpatialReference);//投影变换

            //检索条件
            ISpatialFilter sf = new SpatialFilterClass();
            if (rangeGeometry != null)
            {
                sf.Geometry = rangeGeometry;
                sf.GeometryField = "SHAPE";
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            }

            //提取要素
            IFeatureClassLoad pload = outFC as IFeatureClassLoad;
            if (pload != null)
                pload.LoadOnlyMode = true;

            IFeatureCursor outFeCursor = outFC.Insert(true);
            

            IFeatureCursor inFeCursor = null;
            IFeature inFe = null;
            try
            {
                inFeCursor = inFC.Search(sf, true);
                while ((inFe = inFeCursor.NextFeature()) != null)
                {
                    IFeatureBuffer outFeBuffer = outFC.CreateFeatureBuffer();

                    string stacodValue = "";

                    string guid = inFe.get_Value(guidIndex).ToString();
                    int ver = 0;
                    int.TryParse(inFe.get_Value(verIndex).ToString(), out ver);
                    if (!guid2Ver.ContainsKey(guid) || guid2Ver[guid].Key != ver)
                        continue;//不是最新版本要素

                    string delstate = inFe.get_Value(delIndex).ToString();

                    if (delstate != ServerDataInitializeCommand.DelStateText)//非删除状态
                    {
                        if (!referGUID2Version.ContainsKey(guid))//不存在：原始参考数据库中不存在对应要素
                        {
                            //原始参考数据库中不存在该要素，则可视该服务器要素为新增要素
                            stacodValue = "增加";
                        }
                        else//存在：原始参考数据库中存在对应要素
                        {
                            //if (ver > referGUID2Version[guid])
                            if (ver > dbMaxVersion)//修改要素提交到服务器后版本号应大于原始参考数据库的基版本号
                            {
                                stacodValue = "修改";//视为修改要素
                            }
                            else
                            {
                                continue;//没有更新，不视为增量数据，直接跳过
                            }
                        }
                    }
                    else//删除状态
                    {
                        if (!referGUID2Version.ContainsKey(guid))//不存在：原始参考数据库中不存在对应要素
                        {
                            continue;//不视为增量数据，直接跳过
                        }
                        else//存在：原始参考数据库中存在对应要素
                        {
                            stacodValue = "删除";//视为删除要素
                        }
                    }

                    //几何赋值
                    outFeBuffer.Shape = inFe.Shape;
                    #region 与下一级别要素对比，如果为修改的要素在下一级别中无相交要素则不视为增量
                    if (nextFC != null)
                    {
                        if (outFC.AliasName.ToUpper() == "LRDL")
                        {
                            if (stacodValue == "修改" || stacodValue == "删除")
                            {
                                ISpatialFilter qf = new SpatialFilterClass();
                                qf.Geometry = (inFe.ShapeCopy as ITopologicalOperator).Buffer(10);
                                qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                if (nextFC.FeatureCount(qf) < 1) { continue; };
                            }
                        }

                    }
                    #endregion
                    //属性赋值
                    for (int i = 0; i < outFeBuffer.Fields.FieldCount; i++)
                    {
                        IField field = outFeBuffer.Fields.get_Field(i);
                        if (field.Type == esriFieldType.esriFieldTypeGeometry || field.Type == esriFieldType.esriFieldTypeOID)
                            continue;

                        if (field.Name.ToUpper() == "SHAPE_LENGTH" || field.Name.ToUpper() == "SHAPE_AREA")
                            continue;

                        int index = inFe.Fields.FindField(field.Name);
                        if (index != -1 && field.Editable)
                        {
                            outFeBuffer.set_Value(i, inFe.get_Value(index));
                        }

                        //修改状态字段
                        if (field.Name.ToUpper() == SDEDataServer.StacodFieldName)
                        {
                            outFeBuffer.set_Value(i, stacodValue);
                        }

                    }
                    outFeCursor.InsertFeature(outFeBuffer);
                }

                outFeCursor.Flush();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                string err = ex.Message;
                if (inFe != null)
                {
                    err = string.Format("提取服务器要素类【{0}】中的要素【{1}】时出现错误:", inFC.AliasName, inFe.OID) + err;
                }

                throw new Exception(err);
            }
            finally
            {
                if (pload != null)
                    pload.LoadOnlyMode = false;

                System.Runtime.InteropServices.Marshal.ReleaseComObject(outFeCursor);
                if (inFeCursor != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(inFeCursor);
            }


        }


        /// <summary>
        /// 获取全要素的最新版本
        /// </summary>
        /// <param name="inFeatureClss"></param>
        /// <returns>Dictionary<guid, KeyValuePair<version,delstate>></returns>
        private Dictionary<string, KeyValuePair<int,string>> getGUIDAndVerOfLatestFeatures(IFeatureClass inFC)
        {
            Dictionary<string, KeyValuePair<int, string>> result = new Dictionary<string, KeyValuePair<int, string>>();

            IFeatureCursor inFeCursor = null;
            try
            {
                esriWorkspaceType wsType = (inFC as IDataset).Workspace.Type;
                if (wsType == esriWorkspaceType.esriRemoteDatabaseWorkspace)//sde数据库
                {
                    //将要素按GUID进行分组，取每组中最大的版本号
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = "";
                    //qf.SubFields = string.Format("{0}, MAX({1}) AS {2}, {3}",
                    //    ServerDataInitializeCommand.CollabGUID, ServerDataInitializeCommand.CollabVERSION,
                    //    ServerDataInitializeCommand.CollabVERSION, ServerDataInitializeCommand.CollabDELSTATE);//语法不支持：ServerDataInitializeCommand.CollabDELSTATE必须出现在 GROUP BY 子句中或者在聚合函数中使用
                    qf.SubFields = string.Format("{0}, MAX({1}) AS {2}",
                        ServerDataInitializeCommand.CollabGUID, ServerDataInitializeCommand.CollabVERSION,
                        ServerDataInitializeCommand.CollabVERSION);
                    (qf as IQueryFilterDefinition2).PostfixClause = string.Format("GROUP BY {0}", ServerDataInitializeCommand.CollabGUID);

                    //动态分配内存大小
                    int count = inFC.FeatureCount(qf);
                    result = new Dictionary<string, KeyValuePair<int, string>>(count);

                    //获取最新版本要素的信息
                    inFeCursor = inFC.Search(qf, true);
                    IFeature inFe = null;

                    int guidIndex = inFC.FindField(ServerDataInitializeCommand.CollabGUID);
                    int verIndex = inFC.FindField(ServerDataInitializeCommand.CollabVERSION);
                    int delStateIndex = inFC.FindField(ServerDataInitializeCommand.CollabDELSTATE);
                    while ((inFe = inFeCursor.NextFeature()) != null)
                    {
                        string guid = inFe.get_Value(guidIndex).ToString();
                        int ver = 0;
                        int.TryParse(inFe.get_Value(verIndex).ToString(), out ver);

                        result.Add(guid, new KeyValuePair<int, string>(ver, ""));//初始默认为非删除状态
                    }

                    //当前集合中的要素是否存在删除要素,并更新结果集
                    qf = new QueryFilterClass() { WhereClause = string.Format("{0} = '{1}'", ServerDataInitializeCommand.CollabDELSTATE, ServerDataInitializeCommand.DelStateText) };
                    inFeCursor = inFC.Search(qf, true);
                    while ((inFe = inFeCursor.NextFeature()) != null)
                    {
                        string guid = inFe.get_Value(guidIndex).ToString();
                        int ver = 0;
                        int.TryParse(inFe.get_Value(verIndex).ToString(), out ver);

                        if (result.ContainsKey(guid) && result[guid].Key == ver)
                        {
                            result[guid] = new KeyValuePair<int, string>(ver, ServerDataInitializeCommand.DelStateText);
                        }
                    }
                }
                else//本地数据库,貌似不支持MAX语法？
                {
                    //动态分配内存大小
                    int count = inFC.FeatureCount(null);
                    result = new Dictionary<string, KeyValuePair<int, string>>(count);

                    //获取最新版本要素的信息
                    inFeCursor = inFC.Search(null, true);
                    IFeature inFe = null;

                    int guidIndex = inFC.FindField(ServerDataInitializeCommand.CollabGUID);
                    int verIndex = inFC.FindField(ServerDataInitializeCommand.CollabVERSION);
                    int delStateIndex = inFC.FindField(ServerDataInitializeCommand.CollabDELSTATE);

                    while ((inFe = inFeCursor.NextFeature()) != null)
                    {
                        string guid = inFe.get_Value(guidIndex).ToString();
                        int ver = 0;
                        int.TryParse(inFe.get_Value(verIndex).ToString(), out ver);
                        string delState = inFe.get_Value(delStateIndex).ToString();

                        if (!result.ContainsKey(guid))
                        {
                            result.Add(guid, new KeyValuePair<int, string>(ver, delState));
                        }
                        else
                        {
                            if (ver > result[guid].Key)
                            {
                                result[guid] = new KeyValuePair<int, string>(ver, delState);
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                if (inFeCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(inFeCursor);
                }
            }

            return result;
        }


        /// <summary>
        /// 获取全要素的指定版本
        /// </summary>
        /// <param name="inFeatureClss"></param>
        /// <returns></returns>
        private Dictionary<string, KeyValuePair<int, string>> getGUIDAndVerOfVersionFeatures(IFeatureClass inFC, int version)
        {
            Dictionary<string, KeyValuePair<int, string>> result = new Dictionary<string, KeyValuePair<int, string>>();

            IFeatureCursor inFeCursor = null;
            try
            {
                esriWorkspaceType wsType = (inFC as IDataset).Workspace.Type;
                if (wsType == esriWorkspaceType.esriRemoteDatabaseWorkspace)//sde数据库
                {
                    //将要素按GUID进行分组，取每组中最大的版本号
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = string.Format("{0} <= {1}", ServerDataInitializeCommand.CollabVERSION, version);
                    qf.SubFields = string.Format("{0}, MAX({1}) AS {2}",
                        ServerDataInitializeCommand.CollabGUID, ServerDataInitializeCommand.CollabVERSION,
                        ServerDataInitializeCommand.CollabVERSION);
                    (qf as IQueryFilterDefinition2).PostfixClause = string.Format("GROUP BY {0}", ServerDataInitializeCommand.CollabGUID);

                    //动态分配内存大小
                    int count = inFC.FeatureCount(qf);
                    result = new Dictionary<string, KeyValuePair<int, string>>(count);

                    //获取最新版本要素的信息
                    inFeCursor = inFC.Search(qf, true);
                    IFeature inFe = null;

                    int guidIndex = inFC.FindField(ServerDataInitializeCommand.CollabGUID);
                    int verIndex = inFC.FindField(ServerDataInitializeCommand.CollabVERSION);
                    int delStateIndex = inFC.FindField(ServerDataInitializeCommand.CollabDELSTATE);
                    while ((inFe = inFeCursor.NextFeature()) != null)
                    {
                        string guid = inFe.get_Value(guidIndex).ToString();
                        int ver = 0;
                        int.TryParse(inFe.get_Value(verIndex).ToString(), out ver);

                        result.Add(guid, new KeyValuePair<int, string>(ver, ""));//初始默认为非删除状态
                    }

                    //当前集合中的要素是否存在删除要素,并更新结果集
                    qf = new QueryFilterClass() { WhereClause = string.Format("{0} <= {1} and {2} = '{3}'", ServerDataInitializeCommand.CollabVERSION, version, ServerDataInitializeCommand.CollabDELSTATE, ServerDataInitializeCommand.DelStateText) };
                    inFeCursor = inFC.Search(qf, true);
                    while ((inFe = inFeCursor.NextFeature()) != null)
                    {
                        string guid = inFe.get_Value(guidIndex).ToString();
                        int ver = 0;
                        int.TryParse(inFe.get_Value(verIndex).ToString(), out ver);

                        if (result.ContainsKey(guid) && result[guid].Key == ver)
                        {
                            result[guid] = new KeyValuePair<int, string>(ver, ServerDataInitializeCommand.DelStateText);
                        }
                    }
                }
                else//本地数据库,貌似不支持MAX语法？
                {
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = string.Format("{0} <= {1}", ServerDataInitializeCommand.CollabVERSION, version);

                    //动态分配内存大小
                    int count = inFC.FeatureCount(qf);
                    result = new Dictionary<string, KeyValuePair<int, string>>(count);

                    //获取最新版本要素的信息
                    inFeCursor = inFC.Search(qf, true);
                    IFeature inFe = null;

                    int guidIndex = inFeCursor.FindField(ServerDataInitializeCommand.CollabGUID);
                    int verIndex = inFeCursor.FindField(ServerDataInitializeCommand.CollabVERSION);
                    int delStateIndex = inFeCursor.FindField(ServerDataInitializeCommand.CollabDELSTATE);

                    while ((inFe = inFeCursor.NextFeature()) != null)
                    {
                        string guid = inFe.get_Value(guidIndex).ToString();
                        int ver = 0;
                        int.TryParse(inFe.get_Value(verIndex).ToString(), out ver);
                        string delState = inFe.get_Value(delStateIndex).ToString();

                        if (!result.ContainsKey(guid))
                        {
                            result.Add(guid, new KeyValuePair<int, string>(ver, delState));
                        }
                        else
                        {
                            if (ver > result[guid].Key)
                            {
                                result[guid] = new KeyValuePair<int, string>(ver, delState);
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                if (inFeCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(inFeCursor);
                }
            }

            return result;
        }

        /// <summary>
        /// 遍历服务器数据库的所有要素类，删除满足指定条件的要素;
        /// 更新SMGIServerState表
        /// </summary>
        /// <param name="fcQF"></param>
        /// <param name="tableQF"></param>
        /// <param name="opName"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public int DeleteFeatureInDatabase(IQueryFilter fcQF, IQueryFilter tableQF, string opName, string desc)
        {
            int delCount = 0;

            //获取源数据库工作空间
            if (null == _sdeWorkspace)
            {
                _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                if (null == _sdeWorkspace)
                {
                    return -1;
                }
            }

            if (null == _sdeWorkspace)
            {
                MessageBox.Show("不是有效的矢量工作空间！");
                return -1;
            }

            Dictionary<string, IFeatureClass> fcName2FC = ServerDataInitializeCommand.GetAllFeatureClassFromWorkspace(_sdeWorkspace as IFeatureWorkspace);
            if (fcName2FC.Count == 0)
                return -1;

            //开启编辑
            var pWorkspaceEdit = _sdeWorkspace as IWorkspaceEdit;
            pWorkspaceEdit.StartEditing(true);
            try
            {
                pWorkspaceEdit.StartEditOperation();

                #region 1.删除要素
                
                foreach (var kv in fcName2FC)
                {
                    IFeatureCursor feCursor = kv.Value.Update(fcQF, false);
                    IFeature fe = null;
                    while((fe = feCursor.NextFeature()) != null)
                    {
                        delCount++;

                        fe.Delete();

                        Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(feCursor);
                }
                
                #endregion

                #region 2.更新SMGIServerState表中被删除的版本记录
                var smgiServerStateTable = (_sdeWorkspace as IFeatureWorkspace).OpenTable("SMGIServerState");
                ICursor cursor = smgiServerStateTable.Update(tableQF, true);
                IRow row = null;
                while ((row = cursor.NextRow()) != null)
                {
                    row.set_Value(smgiServerStateTable.FindField("versionstate"), "删除");
                    cursor.UpdateRow(row);

                }
                Marshal.ReleaseComObject(cursor);
                #endregion


                #region 3.向SMGIServerState表增加一条记录
                if (!updateSMGIServerStateTable(_sdeWorkspace, _databaseName, _userName, opName, 0, desc))
                {
                    pWorkspaceEdit.AbortEditOperation();
                    pWorkspaceEdit.StopEditing(false);

                    return -1;
                }
                #endregion

                //结束编辑
                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                pWorkspaceEdit.AbortEditOperation();
                pWorkspaceEdit.StopEditing(false);

                MessageBox.Show(string.Format("操作失败：{0}！",ex.Message));
                
                return -1;
            }

            return delCount;
        }


        /// <summary>
        /// 插入本地更新要素，同时在数据库中删除各对象的所有历史版本
        /// </summary>
        /// <param name="fcName2editedFeatures"></param>
        /// <param name="localRecordTable"></param>
        /// <param name="instrduction"></param>
        /// <param name="opName"></param>
        /// <returns></returns>
        public int UpdateDatabaseAndDelHistoryVersion(Dictionary<string, List<int>> fcName2editedFeatures, DataTable localRecordTable, string instrduction, string opName, WaitOperation wo = null)
        {
            //获取源数据库工作空间
            if (null == _sdeWorkspace)
            {
                _sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                if (null == _sdeWorkspace)
                {
                    return -1;
                }
            }

            IFeatureWorkspace sdeFeatureWorkspace = _sdeWorkspace as IFeatureWorkspace;
            if (null == sdeFeatureWorkspace)
            {
                MessageBox.Show("不是有效的矢量工作空间！");
                return -1;
            }

            //获取服务器最大版本号
            int serverMaxVersion = getServerMaxVersion();
            if (-1 == serverMaxVersion)
            {
                MessageBox.Show("获取服务器的最大版本号失败！");
                return -1;
            }
            int newVersion = serverMaxVersion + 1;

            //开启编辑
            var pWorkspaceEdit = sdeFeatureWorkspace as IWorkspaceEdit;
            pWorkspaceEdit.StartEditing(true);
            pWorkspaceEdit.StartEditOperation();

            //插入要素
            if (!insertCollaborateFeatureToDataBaseAndDelHistoryVersion(sdeFeatureWorkspace, fcName2editedFeatures, newVersion, wo))
            {
                pWorkspaceEdit.AbortEditOperation();
                pWorkspaceEdit.StopEditing(false);

                return -1;
            }

            if (wo != null)
                wo.SetText("正在更新服务器数据库的状态表...");

            //更新SMGIServerState表
            if (!updateSMGIServerStateTable(_sdeWorkspace, _databaseName, _userName, opName, newVersion, instrduction))
            {
                pWorkspaceEdit.AbortEditOperation();
                pWorkspaceEdit.StopEditing(false);

                return -1;
            }

            //更新RecordTable表
            if (!insertLocalTableToServer(localRecordTable, _sdeWorkspace, _databaseName, _userName))
            {
                pWorkspaceEdit.AbortEditOperation();
                pWorkspaceEdit.StopEditing(false);

                return -1;
            }

            //结束编辑
            pWorkspaceEdit.StopEditOperation();
            pWorkspaceEdit.StopEditing(true);

            return newVersion;
        }


        /// <summary>
        /// 将协同客户端提交的要素插入到服务器数据库的数据集中，同时删除各对象的历史版本
        /// </summary>
        /// <param name="sdeFeatureWorkspace"></param>
        /// <param name="fcName2editedFeatures"></param>
        /// <param name="mewVersion"></param>
        /// <returns></returns>
        private bool insertCollaborateFeatureToDataBaseAndDelHistoryVersion(IFeatureWorkspace sdeFeatureWorkspace, Dictionary<string, List<int>> fcName2editedFeatures, int mewVersion, WaitOperation wo = null)
        {
            bool res = true;
            IFeature f = null;
            IFeatureClassLoad pload = null;
            try
            {
                foreach (var item in fcName2editedFeatures)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在更新服务器数据库的要素类【{0}】...", item.Key));

                    IFeatureClass localFC = commonMethod.getFeatureClassFromWorkspace(CollaborativeTask.Instance.LocalWorkspace as IFeatureWorkspace, item.Key);
                    var fc = sdeFeatureWorkspace.OpenFeatureClass(string.Format("{0}.{1}.{2}", _databaseName.Trim(), _userName.Trim(), item.Key.Trim()));

                    #region 删除历史版本
                    string guidSet = "";
                    int count = 0;
                    foreach (var subitem in item.Value)
                    {
                        count++;

                        f = localFC.GetFeature(subitem);
                        string guid = f.get_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString();

                        if (guidSet != "")
                            guidSet += string.Format(",'{0}'", guid);
                        else
                            guidSet = string.Format("'{0}'", guid);

                        Marshal.ReleaseComObject(f);

                        if (count > 1000)
                        {

                            #region 删除
                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = string.Format("{0} in ({1})", ServerDataInitializeCommand.CollabGUID, guidSet);
                            IFeatureCursor feCursor = fc.Update(qf, false);
                            IFeature fe = null;
                            while ((fe = feCursor.NextFeature()) != null)
                            {
                                fe.Delete();

                                Marshal.ReleaseComObject(fe);
                            }
                            Marshal.ReleaseComObject(feCursor);
                            #endregion

                            guidSet = "";
                            count = 0;
                        }

                        Marshal.ReleaseComObject(f);
                    }

                    if (count > 0)
                    {
                        #region 删除
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("{0} in ({1})", ServerDataInitializeCommand.CollabGUID, guidSet);
                        IFeatureCursor feCursor = fc.Update(qf, false);
                        IFeature fe = null;
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            fe.Delete();

                            Marshal.ReleaseComObject(fe);
                        }
                        Marshal.ReleaseComObject(feCursor);
                        #endregion
                    }
                    #endregion

                    pload = fc as IFeatureClassLoad;
                    pload.LoadOnlyMode = true;

                    #region 插入要素(非删除状态)
                    IFeatureCursor pFeatureCursor = fc.Insert(true);
                    foreach (var subitem in item.Value)
                    {
                        f = localFC.GetFeature(subitem);
                        IGeometry shape = f.Shape;

                        int ver = 0;
                        int.TryParse(f.get_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out ver);
                        f.set_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabVERSION), mewVersion);

                        string delState = f.get_Value(f.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE)).ToString();
                        if (delState == ServerDataInitializeCommand.DelStateText)//删除要素
                        {
                            continue;//删除要素不进行插入

                        }

                        //IFeatureBuffer pFeatureBuffer = f as IFeatureBuffer;

                        IFeatureBuffer pFeatureBuffer = fc.CreateFeatureBuffer();
                        //几何赋值
                        pFeatureBuffer.Shape = shape;
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

                            int index = f.Fields.FindField(pfield.Name);
                            if (index != -1 && pfield.Editable)
                            {
                                pFeatureBuffer.set_Value(i, f.get_Value(index));
                            }

                        }

                        pFeatureCursor.InsertFeature(pFeatureBuffer);

                        Marshal.ReleaseComObject(f);
                    }
                    pFeatureCursor.Flush();

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
                    #endregion

                    pload.LoadOnlyMode = false;
                    pload = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                if (f != null)
                {
                    MessageBox.Show(string.Format("更新要素类‘{0}’中的要素‘{1}’时失败：{2}", f.Class.AliasName, f.OID, ex.Message));
                }
                else
                {
                    MessageBox.Show(string.Format("更新服务器数据库失败：{0}", ex.Message));
                }

                if (pload != null)
                {
                    pload.LoadOnlyMode = false;
                }

                res = false;
            }

            return res;
        }


        #region 咨询锁
        /// <summary>
        /// 尝试获取会话级别的排他锁，若成功则返回true，否则返回false
        /// boolean pg_try_advisory_lock(key bigint)
        /// </summary>
        /// <returns></returns>
        public bool AdvisoryLock()
        {
            bool res;

            if (_conn == null)
            {
                string connStr = "server=" + _ipAddress + ";Port=" + _port + ";uid=" + _userName + ";pwd=" + _passWord + ";Database=" + _databaseName;
                _conn = new NpgsqlConnection(connStr);
                _conn.Open();
            }

            string sql = "select pg_try_advisory_lock(1)";
            NpgsqlCommand objCommand = new NpgsqlCommand(sql, _conn);
            res = (bool)objCommand.ExecuteScalar();

            return res;
        }

        /// <summary>
        /// 释放会话级别排他锁
        /// boolean pg_advisory_unlock(key bigint)
        /// </summary>
        /// <returns></returns>
        public bool AdvisoryUnlock()
        {
            bool res;

            string sql = "select pg_advisory_unlock(1)";
            NpgsqlCommand objCommand = new NpgsqlCommand(sql, _conn);
            res = (bool)objCommand.ExecuteScalar();

            _conn.Close();
            _conn.Dispose();
            _conn = null;

            return res;
        }
        #endregion

    }
}

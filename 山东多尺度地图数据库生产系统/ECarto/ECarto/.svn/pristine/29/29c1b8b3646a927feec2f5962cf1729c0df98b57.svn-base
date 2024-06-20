using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Net;
using System.Net.Sockets;

namespace SMGI.Plugin.CollaborativeWork
{
    public struct FeatureInfo
    {
        public int oid;
        public int collabVer;
        public string collabDel;
        public string opuser;
    }

    public class CollaborativeTask
    {
        public enum DetectionState
        {
            UNDETECTED, //未冲突检测
            DETECTING,  //正在冲突检测
            DETECTED    //冲突检测完成
        }

        private CollaborativeTask()
        {
            _detectState = DetectionState.UNDETECTED;
            
            _localWorkspace = null;
            _serverWorkspace = null;
            _extentPolygon = null;

            _sdeDataBaseName = "";
            _serverIPAddress = "";
            _sdeUsername = "";
            _sdePassword = "";

            _localBaseVersion = -1;
            _serverLatestVersion = -1;

            _serverConflictFeatures = null;
            _localConflictFeatures = null;
            _localConflictFeaturesState = null;
        }

        #region 成员、属性
        private GApplication _app;
        //唯一实例
        private static CollaborativeTask m_instance = null;
        public static CollaborativeTask Instance
        {
            get
            {
                if (null == CollaborativeTask.m_instance)
                {
                    CollaborativeTask.m_instance = new CollaborativeTask();
                }

                return CollaborativeTask.m_instance;
            }
        }

        //冲突检测状态
        private DetectionState _detectState;
        public DetectionState DetectState
        {
            get
            {
                return _detectState;
            }
            set
            {
                _detectState = value;
            }
        }

        //本地工作空间
        private IWorkspace _localWorkspace;
        public IWorkspace LocalWorkspace
        {
            get
            {
                return _localWorkspace;
            }
        }

        //本地数据库版本号
        private int _localBaseVersion;
        public int LocalBaseVersion
        {
            get
            {
                return _localBaseVersion;
            }
        }

        //服务器端IP
        private string _serverIPAddress;
        public string ServerIPAddress
        {
            get
            {
                return _serverIPAddress;
            }
        }


        private string _sdeUsername;
        public string SDEUsername
        {
            get
            {
                return _sdeUsername.Trim();
            }
        }

        private string _sdePassword;
        public string SDEPassword
        {
            get
            {
                return _sdePassword;
            }
        }


        //服务器端最新版本号
        private int _serverLatestVersion;
        public int ServerLatestVersion
        {
            get
            {
                return _serverLatestVersion;
            }
        }

        //服务器端数据库名
        private string _sdeDataBaseName;
        public string SDEDataBaseName
        {
            get
            {
                return _sdeDataBaseName.Trim();
            }
        }

        //服务器端工作空间
        private IWorkspace _serverWorkspace;
        public IWorkspace ServerWorkspace
        {
            get
            {
                return _serverWorkspace;
            }
        }


        //检索范围多边形
        private IPolygon _extentPolygon;
        public IPolygon ExtentPolygon
        {
            get
            {
                return _extentPolygon;
            }
        }

        //是否需要协调
        public bool NeedCollaborate
        {
            get
            {
                return _localBaseVersion < _serverLatestVersion;
            }
        }

        //服务器与本地数据库中有冲突的要素Dictionary<FeatureClassName, Dictionary<GUID, FeatureInfo>>
        private Dictionary<string, Dictionary<string, FeatureInfo>> _serverConflictFeatures;
        public Dictionary<string, Dictionary<string, FeatureInfo>> ServerConflictFeatures
        {
            get
            {
                return _serverConflictFeatures;
            }
        }

        //本地被编辑过且与服务器最新版数据有冲突的要素Dictionary<FeatureClassName, Dictionary<GUID, FeatureInfo>>
        private Dictionary<string, Dictionary<string, FeatureInfo>> _localConflictFeatures;
        public Dictionary<string, Dictionary<string, FeatureInfo>> LocalConflictFeatures
        {
            get
            {
                return _localConflictFeatures;
            }
        }

        //本地被编辑过且与服务器最新版数据有冲突的要素处理状态<guid,bool>
        private Dictionary<string, bool> _localConflictFeaturesState;
        public Dictionary<string, bool> LocalConflictFeaturesState
        {
            get
            {
                return _localConflictFeaturesState;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 准备协调，初始化工作
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public bool prepareCollaborate(GApplication app)
        {
            _serverConflictFeatures = null;
            _localConflictFeatures = null;
            _localConflictFeaturesState = null;

            _app = app;

            _localWorkspace = app.Workspace.EsriWorkspace;

            DataTable localDT = commonMethod.ReadToDataTable(_localWorkspace, "SMGILocalState");
            if (null == localDT || 0 == localDT.Rows.Count)
            {
                MessageBox.Show("本地数据库状态表为空,无法协调数据！");
                return false;
            }

            try
            {
                _localBaseVersion = Convert.ToInt32(localDT.AsEnumerable().Select(t => t.Field<string>("BASEVERSION")).FirstOrDefault());//本地数据库的基版本
            }
            catch
            {
                MessageBox.Show("本地数据库状态表中的基版本号为无效值，无法协调数据！");
                return false;
            }

            _serverIPAddress = localDT.AsEnumerable().Select(t => t.Field<string>("IPADDRESS")).FirstOrDefault();
            _sdeUsername = localDT.AsEnumerable().Select(t => t.Field<string>("USERNAME")).FirstOrDefault();
            _sdePassword = localDT.AsEnumerable().Select(t => t.Field<string>("PASSWORD")).FirstOrDefault();
            _sdeDataBaseName = localDT.AsEnumerable().Select(t => t.Field<string>("DATABASE")).FirstOrDefault();

            //纠正服务器IP
            try
            {
                var arrIPAddresses = Dns.GetHostAddresses(_serverIPAddress);//通过计算机名或地址获取IP地址
                foreach (var ip in arrIPAddresses)
                {
                    if (ip.AddressFamily.Equals(AddressFamily.InterNetwork))
                        _serverIPAddress = ip.ToString();
                }

            }
            catch
            {
            }

            LocalDataBase db = new LocalDataBase(app, _localWorkspace);

            //查找范围多边形
            _extentPolygon = db.getExtentPolygon();
            if (null == _extentPolygon)
            {
                MessageBox.Show(string.Format("找不到范围多边形"));
                return false;
            }

            //删除本地数据库中上次协调的协调数据
            db.deleteCollaborateFeatures(_localBaseVersion);

            _serverWorkspace = GApplication.Application.GetWorkspacWithSDEConnection(_serverIPAddress, _sdeUsername, _sdePassword, _sdeDataBaseName);
            _serverLatestVersion = ServerDataInitializeCommand.getServerDBMaxVersion(_serverWorkspace);//获取服务器数据库当前最大版本号
            if (-1 == _serverLatestVersion)
            {
                MessageBox.Show("获取服务器的最大版本号失败！");
                return false;
            }


            return true;
        }

        /// <summary>
        /// 开始协调
        /// </summary>
        public string startCollaborate(WaitOperation wo)
        {
            string err = "";

            _detectState = DetectionState.DETECTING;

            try
            {
                wo.SetText("正在从服务器获取最新数据...");
                SDEDataServer ds = new SDEDataServer(_app, _serverIPAddress, _sdeUsername, _sdePassword, _sdeDataBaseName);
                _serverConflictFeatures = ds.getConflictFeatures(_extentPolygon, _localBaseVersion);

                wo.SetText("正在进行冲突检测...");
                Dictionary<string, Dictionary<string, FeatureInfo>> tmpLocalEditedFeatures = getEditedFeatureList();
                _localConflictFeatures = getLocalConflictFeatures(tmpLocalEditedFeatures, _serverConflictFeatures);
                _localConflictFeaturesState = new Dictionary<string, bool>();
                foreach (var kv in _localConflictFeatures)
                {
                    foreach (var guid in kv.Value.Keys)
                    {
                        if (_localConflictFeaturesState.ContainsKey(guid))
                        {
                            throw new Exception(string.Format("编辑要素中的唯一编码【{0}】存在重复值！", guid));
                        }

                        _localConflictFeaturesState.Add(guid, false);
                    }
                }

                //向本地数据库插入服务器端新版本数据
                wo.SetText("正在更新本地数据库...");
                LocalDataBase db = new LocalDataBase(_app, _localWorkspace);
                db.insertCollabData(_serverWorkspace, _serverConflictFeatures);

                _detectState = DetectionState.DETECTED;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;

                _detectState = DetectionState.UNDETECTED;
            }

            return err;
        }

        /// <summary>
        /// 从本地工作空间中提取编辑过的数据的要素
        /// </summary>
        /// <param name="pWorkspace"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, FeatureInfo>> getEditedFeatureList()
        {
            Dictionary<string, Dictionary<string, FeatureInfo>> result = new Dictionary<string, Dictionary<string, FeatureInfo>>();

            List<IFeatureClass> fcList = getFeatureClassList(_localWorkspace);
            foreach (IFeatureClass fc in fcList)
            {
                int guidIndex = fc.FindField(ServerDataInitializeCommand.CollabGUID);
                int verIndex = fc.FindField(ServerDataInitializeCommand.CollabVERSION);
                int delIndex = fc.FindField(ServerDataInitializeCommand.CollabDELSTATE);
                if (guidIndex == -1 || verIndex == -1 || delIndex == -1)
                    continue;

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("{0} < 0", ServerDataInitializeCommand.CollabVERSION);
                IFeatureCursor fCursor = fc.Search(qf, true);

                IFeature f = null;
                while ((f =fCursor.NextFeature()) != null)
                {
                    string guid = f.get_Value(guidIndex).ToString();
                    int ver = 0;
                    int.TryParse(f.get_Value(verIndex).ToString(), out ver);
                    string delState = f.get_Value(delIndex).ToString();
                    if (!result.ContainsKey(fc.AliasName))
                    {
                        Dictionary<string, FeatureInfo> guid2FeInfo = new Dictionary<string, FeatureInfo>();
                        FeatureInfo feInfo = new FeatureInfo { oid = f.OID, collabVer = ver, collabDel = delState };
                        guid2FeInfo.Add(guid, feInfo);

                        result.Add(fc.AliasName, guid2FeInfo);
                    }
                    else
                    {
                        if (!result[fc.AliasName].ContainsKey(guid))
                        {
                            FeatureInfo feInfo = new FeatureInfo { oid = f.OID, collabVer = ver, collabDel = delState };

                            result[fc.AliasName].Add(guid, feInfo);
                        }
                    }
                    
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
            }

            return result;
        }

        /// <summary>
        /// 协调是否完成
        /// </summary>
        /// <returns></returns>
        public bool IsCompleted()
        {
            if (_detectState != DetectionState.DETECTED)
                return false;//未进行冲突检测

            if (_localConflictFeaturesState.ContainsValue(false))
                return false;//尚有未确认的冲突数据

            return true;
        }


        /// <summary>
        /// 协调完成
        /// </summary>
        public void completeCollaborate()
        {
            _detectState = DetectionState.UNDETECTED;

            System.Runtime.InteropServices.Marshal.ReleaseComObject(_serverWorkspace);

            _localWorkspace = null;
            _serverWorkspace = null;
            _extentPolygon = null;

            _sdeDataBaseName = "";
            _serverIPAddress = "";
            _sdeUsername = "";
            _sdePassword = "";

            _localBaseVersion = -1;
            _serverLatestVersion = -1;

            _serverConflictFeatures = null;
            _localConflictFeatures = null;
            _localConflictFeaturesState = null;

            ConflictResultTable.Instance.upateTable();

        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 打开一个文件数据库
        /// </summary>
        /// <param name="fullPath">文件数据库</param>
        /// <returns>工作空间</returns>
        private IWorkspace getWorkspacOfFileGDB(string fullPath)
        {
            try
            {
                IWorkspaceFactory pFileGDBWorkspaceFactory = new FileGDBWorkspaceFactoryClass();

                IWorkspace workspace = pFileGDBWorkspaceFactory.OpenFromFile(fullPath, 0);

                return workspace;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("数据库连接失败!" + ex.Message);
            }

            return null;
        }


        

        /// <summary>
        /// 从工作空间中获取所有的要素类
        /// </summary>
        /// <param name="pWorkspace"></param>
        /// <returns></returns>
        private List<IFeatureClass> getFeatureClassList(IWorkspace pWorkspace)
        {
            List<IFeatureClass> fcList = new List<IFeatureClass>();

            if (null == pWorkspace)
                return fcList;

            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataset = null;
            while ((pDataset = pEnumDataset.Next()) != null)
            {
                if (pDataset is IFeatureDataset)//要素数据集
                {
                    IFeatureDataset pFeatureDataset = pDataset as IFeatureDataset;
                    IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF = pEnumDatasetF.Next();
                    while (pDatasetF != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = pDatasetF as IFeatureClass;
                            if (fc != null)
                                fcList.Add(fc);
                        }

                        pDatasetF = pEnumDatasetF.Next();
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pDataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc = pDataset as IFeatureClass;
                    if (fc != null)
                        fcList.Add(fc);
                }
                else
                {

                }

            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);

            return fcList;
        }

        /// <summary>
        /// 返回本地数据库中经过编辑且与服务器最新版本数据有冲突的要素集合
        /// </summary>
        /// <param name="localEditedFeatures"></param>
        /// <param name="serverConflictFeatures"></param>
        /// <returns></returns>
        private Dictionary<string, Dictionary<string, FeatureInfo>> getLocalConflictFeatures(Dictionary<string, Dictionary<string, FeatureInfo>> localEditedFeatures, Dictionary<string, Dictionary<string, FeatureInfo>> serverConflictFeatures)
        {
            Dictionary<string, Dictionary<string, FeatureInfo>> result = new Dictionary<string, Dictionary<string, FeatureInfo>>();
            if (null == serverConflictFeatures || 0 == serverConflictFeatures.Count)
                return result;

            foreach (var kv in localEditedFeatures)
            {
                foreach(var kv2 in kv.Value)
                {
                    foreach(var item in serverConflictFeatures)
                    {
                        if (item.Value.ContainsKey(kv2.Key))
                        {
                            if(!result.ContainsKey(kv.Key))
                            {
                                Dictionary<string, FeatureInfo> guid2feInfo = new Dictionary<string, FeatureInfo>();
                                guid2feInfo.Add(kv2.Key, kv2.Value);

                                result.Add(kv.Key, guid2feInfo);
                            }
                            else
                            {
                                result[kv.Key].Add(kv2.Key, kv2.Value);
                            }
                            
                            break;
                        }
                    }
                }
            }

            return result;
        }


        #endregion
    }
}

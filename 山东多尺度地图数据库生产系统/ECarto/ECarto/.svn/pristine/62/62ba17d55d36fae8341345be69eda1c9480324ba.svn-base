using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataManagementTools;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.GeoDatabaseUI;
using System.Data;

namespace SMGI.Plugin.CollaborativeWork
{
    public class ServerDataInitializeCommand : SMGICommand
    {
        public const string DelStateText = "是";//要素的删除状态文本

        public static string CollabGUID//唯一标识
        {
            get;
            private set;
        }

        public static string CollabVERSION//版本号
        {
            get;
            private set;
        }

        public static string CollabDELSTATE//删除状态['是']
        {
            get;
            private set;
        }

        public static string CollabOPUSER//操作人员名（机器名）
        {
            get;
            private set;
        }

        


        public ServerDataInitializeCommand()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace==null && m_Application!=null;
            }
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            CollabGUID = m_Application.TemplateManager.getFieldAliasName("SMGIGUID");
            CollabVERSION = m_Application.TemplateManager.getFieldAliasName("SMGIVERSION");
            CollabDELSTATE = m_Application.TemplateManager.getFieldAliasName("SMGIDEL");
            CollabOPUSER = m_Application.TemplateManager.getFieldAliasName("SMGIOPUSER");
        }
       
        public override void OnClick()
        {
            SDECommonConnectionForm sdeConn=new SDECommonConnectionForm();
            if (sdeConn.ShowDialog()== DialogResult.OK)
            {
                IWorkspace wsServer=null;
                try
                {
                    wsServer = m_Application.GetWorkspacWithSDEConnection(sdeConn.tbIPAdress.Text.Trim(), sdeConn.tbUserName.Text.Trim(), sdeConn.tbPassword.Text.Trim(), sdeConn.cmbDataBaseList.Text.ToString().Trim());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(ex.Message);
                    return;
                }

                var propertySet = m_Application.GetPropertysetWithSDEConnection(sdeConn.tbIPAdress.Text.Trim(), sdeConn.tbUserName.Text.Trim(), sdeConn.tbPassword.Text.Trim(), sdeConn.cmbDataBaseList.Text.ToString().Trim());
                if (wsServer == null || propertySet==null)
                {
                    return;
                }

                string err = "";

                using (var wo = m_Application.SetBusy())
                {
                    wo.SetText("正在将协同状态表上传至服务器数据库......");

                    #region 为服务器数据库添加协同状态表
                    if (!(wsServer as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, "SMGIServerState"))
                    {
                        var content = m_Application.Template.Content;
                        var dataTemplate = content.Element("DataTemplate");

                        IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                        IWorkspace wsLocal = wsFactory.OpenFromFile(m_Application.Template.Root + "\\" + dataTemplate.Value, 0);

                        string sdePath = m_Application.GetSdeConnectionPath(m_Application.Template.Root, propertySet);
                        var serverTable = (wsLocal as IFeatureWorkspace).OpenTable("SMGIServerState");
                        if (serverTable != null)
                        {
                            SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, new Copy() { in_data = wsLocal.PathName + "\\SMGIServerState", out_data = sdePath + "\\SMGIServerState" }, null);
                        }
                        else
                        {
                            MessageBox.Show("服务器状态表上传失败！初始化失败！");
                            return;
                        }

                        var recordTable = (wsLocal as IFeatureWorkspace).OpenTable("RecordTable");
                        if (recordTable != null)
                        {
                            SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, new Copy() { in_data = wsLocal.PathName + "\\RecordTable", out_data = sdePath + "\\RecordTable" }, null);
                        }
                        else
                        {
                            MessageBox.Show("记录表上传失败！初始化失败！");
                            return;
                        }
                    }
                    #endregion

                    wo.SetText("正在获取服务器数据库的要素类信息......");
                    Dictionary<string, IFeatureClass> fcName2FC = GetAllFeatureClassFromWorkspace(wsServer as IFeatureWorkspace);
                    if (fcName2FC.Count == 0)
                        return;

                    foreach (var kv in fcName2FC)
                    {
                        var fcName = kv.Key;
                        var fc = kv.Value;

                        wo.SetText(string.Format("正在为服务器数据库中的要素类【{0}】添加协同字段......", fcName));
                        #region 添加协同字段
                        int guidIndex = fc.FindField(ServerDataInitializeCommand.CollabGUID);
                        if (guidIndex == -1)
                        {
                            IFieldEdit field = new FieldClass();
                            field.Name_2 = ServerDataInitializeCommand.CollabGUID;
                            field.Type_2 = esriFieldType.esriFieldTypeString;
                            field.Editable_2 = true;
                            field.DefaultValue_2 = "NULL";
                            fc.AddField(field);
                            AddIndexToFeatureClass(fc, fcName.Substring(fcName.LastIndexOf(".")) + ServerDataInitializeCommand.CollabGUID, ServerDataInitializeCommand.CollabGUID);
                        }

                        int versIndex = fc.FindField(ServerDataInitializeCommand.CollabVERSION);
                        if (versIndex == -1)
                        {
                            IFieldEdit field = new FieldClass();
                            field.Name_2 = ServerDataInitializeCommand.CollabVERSION;
                            field.Type_2 = esriFieldType.esriFieldTypeInteger;
                            field.DefaultValue_2 = 0;
                            field.Editable_2 = true;
                            fc.AddField(field);
                            AddIndexToFeatureClass(fc, fcName.Substring(fcName.LastIndexOf(".")) + ServerDataInitializeCommand.CollabVERSION, ServerDataInitializeCommand.CollabVERSION);
                        }

                        int delIndex = fc.FindField(ServerDataInitializeCommand.CollabDELSTATE);
                        if (delIndex == -1)
                        {
                            IFieldEdit field = new FieldClass();
                            field.Name_2 = ServerDataInitializeCommand.CollabDELSTATE;
                            field.Type_2 = esriFieldType.esriFieldTypeString;
                            field.Editable_2 = true;
                            field.Length_2 = 1;
                            fc.AddField(field);
                            AddIndexToFeatureClass(fc, fcName.Substring(fcName.LastIndexOf(".")) + ServerDataInitializeCommand.CollabDELSTATE, ServerDataInitializeCommand.CollabDELSTATE);
                        }

                        int opUserIndex = fc.FindField(ServerDataInitializeCommand.CollabOPUSER);
                        if (opUserIndex == -1)
                        {
                            IFieldEdit field = new FieldClass();
                            field.Name_2 = ServerDataInitializeCommand.CollabOPUSER;
                            field.Type_2 = esriFieldType.esriFieldTypeString;
                            field.Editable_2 = true;
                            field.DefaultValue_2 = "NULL";
                            fc.AddField(field);
                            AddIndexToFeatureClass(fc, fcName.Substring(fcName.LastIndexOf(".")) + ServerDataInitializeCommand.CollabOPUSER, ServerDataInitializeCommand.CollabOPUSER);
                        }
                        #endregion
                    }

                    IWorkspaceEdit wsServerEdit = wsServer as IWorkspaceEdit;
                    wsServerEdit.StartEditing(true);
                    wsServerEdit.StartEditOperation();
                    try
                    {
                        IQueryFilter queryFilter = new QueryFilterClass();
                        queryFilter.WhereClause = string.Format("{0} IS NULL or trim({1}) = '' or {2} = 'NULL'",
                            ServerDataInitializeCommand.CollabGUID,
                            ServerDataInitializeCommand.CollabGUID,
                            ServerDataInitializeCommand.CollabGUID);//sde数据库中可以使用trim来去除字段前后空格

                        wo.SetText(string.Format("正在统计所有未赋值的要素信息......"));

                        int totalCount = 0;
                        Dictionary<string, int> fcName2Count = new Dictionary<string,int>();
                        foreach (var kv in fcName2FC)
                        {
                            int count = kv.Value.FeatureCount(queryFilter);
                            fcName2Count.Add(kv.Key, count);
                            totalCount += count;
                        }
                        int countPerPercent =(int)Math.Ceiling(totalCount / 100.0);

                        int num = 0;
                        int step =0;
                        foreach (var kv in fcName2FC)
                        {
                            var fcName = kv.Key;
                            var fc = kv.Value;

                            #region 为未赋值的协同字段添加初始值
                            int guidIndex = fc.FindField(ServerDataInitializeCommand.CollabGUID);
                            int versIndex = fc.FindField(ServerDataInitializeCommand.CollabVERSION);
                            int delIndex = fc.FindField(ServerDataInitializeCommand.CollabDELSTATE);
                            int opUserIndex = fc.FindField(ServerDataInitializeCommand.CollabOPUSER);
                            if (guidIndex != -1 && versIndex != -1 && delIndex != -1 && opUserIndex != -1)
                            {
                                var fCursor = fc.Update(queryFilter, true);//不要设为false，容易导致内容溢出

                                #region 方法1 UpdateFeature方法
                                IFeature f = null;
                                while ((f = fCursor.NextFeature()) != null)
                                {
                                    wo.SetText(string.Format("正在为要素类{0}（{1}个）中的要素【{2}】赋初值......\r\n 本次需赋值的要素总计数量:{3}个,当前已完成赋值的总进度:{4}%", fcName, fcName2Count[fcName], f.OID, totalCount, step));

                                    f.set_Value(versIndex, 0);
                                    f.set_Value(guidIndex, Guid.NewGuid().ToString());
                                    f.set_Value(delIndex, DBNull.Value);
                                    f.set_Value(opUserIndex, "");

                                    fCursor.UpdateFeature(f);

                                    
                                    num++;
                                    if (num > countPerPercent)
                                    {
                                        ++step;

                                        num = 0;//重置
                                    }

                                }
                                #endregion

                                #region 方法2 ICalculator
                                //ICalculator calculator = new CalculatorClass();
                                //calculator.Cursor = fCursor as ICursor;
                                //calculator.Field = fc.Fields.get_Field(guidIndex).Name;
                                //calculator.Expression = "";//Guid怎么创建？？？？
                                //calculator.Calculate();
                                #endregion

                                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                            }
                            #endregion
                        }

                        wo.SetText("正在为服务器数据库中的协同状态表添加新记录......");
                        #region 向服务器端状态表添加新记录
                        var smgiServerStateTable = (wsServer as IFeatureWorkspace).OpenTable("SMGIServerState");
                        //if (smgiServerStateTable.RowCount(null) == 0)//任何一次服务器初始化信息都写入到状态表，20210608
                        {
                            IRow newRow = smgiServerStateTable.CreateRow();
                            newRow.set_Value(smgiServerStateTable.FindField("VERSID"), 0);
                            newRow.set_Value(smgiServerStateTable.FindField("OPUSER"), System.Environment.MachineName);
                            newRow.set_Value(smgiServerStateTable.FindField("OPTIME"), DateTime.Now);
                            newRow.set_Value(smgiServerStateTable.FindField("INSTRUCTION"), "服务器初始化");
                            newRow.set_Value(smgiServerStateTable.FindField("VERSIONSTATE"), "");
                            newRow.Store();
                        }
                        #endregion

                        //RegistryHelper.RegisterVersion(wsServer);//提交过程中保证只有一个用户，不再需要对数据库进行版本化操作（2021.06.08）

                        wsServerEdit.StopEditOperation();
                        wsServerEdit.StopEditing(true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.Message);
                        System.Diagnostics.Trace.WriteLine(ex.Source);
                        System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                        err = "数据库初始化失败：" + ex.Message;

                        wsServerEdit.AbortEditOperation();
                        wsServerEdit.StopEditing(false);
                    }
                }

                if (err != "")
                    MessageBox.Show(err);
                else
                    MessageBox.Show("数据库初始化完成！");
            }
        }

        /// <summary>
        /// 为要素类添加索引
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="indexName"></param>
        /// <param name="nameOfField"></param>
        public void AddIndexToFeatureClass(IFeatureClass featureClass, String indexName,String nameOfField)
        {
            // Ensure the feature class contains the specified field.
            int fieldIndex = featureClass.FindField(nameOfField);
            if (fieldIndex == -1)
            {
                throw new ArgumentException(
                    "The specified field does not exist in the feature class.");
            }

            // Get the specified field from the feature class.
            IFields featureClassFields = featureClass.Fields;
            IField field = featureClassFields.get_Field(fieldIndex);

            // Create a fields collection and add the specified field to it.
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;
            fieldsEdit.FieldCount_2 = 1;
            fieldsEdit.set_Field(0, field);

            // Create an index and cast to the IIndexEdit interface.
            IIndex index = new IndexClass();
            IIndexEdit indexEdit = (IIndexEdit)index;

            // Set the index's properties, including the associated fields.
            indexEdit.Fields_2 = fields;
            indexEdit.IsAscending_2 = false;
            indexEdit.IsUnique_2 = false;
            indexEdit.Name_2 = indexName;

            // Add the index to the feature class.
            featureClass.AddIndex(index);
        }

        /// <summary>
        /// 获取服务器数据库中的最大版本号
        /// </summary>
        /// <returns></returns>
        public static int getServerDBMaxVersion(IWorkspace sdeWorkspace)
        {
            int maxVersion = -1;

            //获取要素类中的最大版本号
            IEnumDataset enumDataset = sdeWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset is IFeatureDataset)//要素数据集
                {
                    //遍历子要素类
                    IFeatureDataset featureDataset = dataset as IFeatureDataset;
                    IEnumDataset subEnumDataset = featureDataset.Subsets;
                    subEnumDataset.Reset();
                    IDataset subDataset = null;
                    while ((subDataset = subEnumDataset.Next()) != null)
                    {
                        if (subDataset is IFeatureClass)//要素类
                        {
                            int ver = GetFeatureClassMaxVersion(subDataset as IFeatureClass);
                            if (ver > maxVersion)
                                maxVersion = ver;
                        }
                    }
                    Marshal.ReleaseComObject(subEnumDataset);
                }
                else if (dataset is IFeatureClass)//要素类
                {
                    int ver = GetFeatureClassMaxVersion(dataset as IFeatureClass);
                    if (ver > maxVersion)
                        maxVersion = ver;
                }
            }
            Marshal.ReleaseComObject(enumDataset);

            //获取服务器状态表中的最大版本号
            DataTable smgiServerStateTable = commonMethod.ReadToDataTable(sdeWorkspace, "SMGIServerState");
            if (smgiServerStateTable != null && smgiServerStateTable.Rows.Count > 0)
            {
                int tableMaxVersion = -1;
                try
                {
                    tableMaxVersion = Convert.ToInt32(smgiServerStateTable.AsEnumerable().Select(t => t.Field<string>("VERSID")).Max());//数据库的最新版本号
                }
                catch (Exception ex)
                {
                    tableMaxVersion = -1;
                }

                if (tableMaxVersion > maxVersion)
                    maxVersion = tableMaxVersion;
            }

            return maxVersion;
        }

        /// <summary>
        /// 获取要素类的最大版本号
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        public static int GetFeatureClassMaxVersion(IFeatureClass fc)
        {
            int maxVer = 0;

            if (fc.FindField(ServerDataInitializeCommand.CollabVERSION) != -1)
            {
                esriWorkspaceType wsType = (fc as IDataset).Workspace.Type;
                if (wsType == esriWorkspaceType.esriRemoteDatabaseWorkspace)//sde数据库
                {
                    IQueryFilter queryFilter = new QueryFilterClass();
                    //queryFilter.WhereClause = string.Format("{0} <> {1}", ServerDataInitializeCommand.CollabVERSION, ServerDataInitializeCommand.ServerDeleteState);
                    queryFilter.WhereClause = "";
                    queryFilter.SubFields = string.Format("MAX({0}) AS {0}", ServerDataInitializeCommand.CollabVERSION);

                    var feCursor = fc.Search(queryFilter, true);
                    IFeature fe = feCursor.NextFeature();
                    if (fe != null)
                    {
                        int.TryParse(fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out maxVer);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);
                }
                else//本地数据库,貌似不支持MAX语法？
                {
                    IQueryFilter queryFilter = new QueryFilterClass();
                   // queryFilter.WhereClause = string.Format("{0} <> {1}", ServerDataInitializeCommand.CollabVERSION, ServerDataInitializeCommand.ServerDeleteState);
                    queryFilter.WhereClause = "";

                    int verIndex = fc.FindField(ServerDataInitializeCommand.CollabVERSION);

                    var feCursor = fc.Search(queryFilter, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        int ver = 0;
                        int.TryParse(fe.get_Value(verIndex).ToString(), out ver);

                        if (ver > maxVer)
                            maxVer = ver;
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);
                }

            }

            return maxVer;
        }

        /// <summary>
        /// 获取工作空间中的所有要素类集合
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public static Dictionary<string, IFeatureClass> GetAllFeatureClassFromWorkspace(IFeatureWorkspace ws)
        {
            Dictionary<string, IFeatureClass> result = new Dictionary<string, IFeatureClass>();

            if (null == ws)
                return result;

            IEnumDataset enumDataset = (ws as IWorkspace).get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = null;
            while ((dataset = enumDataset.Next()) != null)
            {
                if (dataset is IFeatureDataset)//要素数据集
                {
                    IFeatureDataset feDataset = dataset as IFeatureDataset;
                    IEnumDataset subEnumDataset = feDataset.Subsets;
                    subEnumDataset.Reset();
                    IDataset subDataset = null;
                    while ((subDataset = subEnumDataset.Next()) != null)
                    {
                        if (subDataset is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = subDataset as IFeatureClass;
                            if (fc != null)
                                result.Add(subDataset.Name, fc);
                        }
                    }
                    Marshal.ReleaseComObject(subEnumDataset);
                }
                else if (dataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc = dataset as IFeatureClass;
                    if (fc != null)
                        result.Add(dataset.Name, fc);
                }
                else
                {

                }

            }
            Marshal.ReleaseComObject(enumDataset);


            return result;
        }
    }
}

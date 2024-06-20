using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.DataManagementTools;
using System.Threading;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.Display;
using System.Xml.Linq;
using System.Diagnostics;
namespace ShellTBDivided
{
    /// <summary>
    ///图斑符号化
    /// </summary>
    public class TBSymbolizeClass
    {

        Geoprocessor mGP = new Geoprocessor();
        WaitOperation mWo = new WaitOperation();
        bool mGPFished = false;
        public bool DisOverWrite = true;
        private Queue<IGPProcess> mGPToolsToExecute = new Queue<IGPProcess>();
        public TBSymbolizeClass()
        {
           mGP.OverwriteOutput = true;
        }

        public  bool SymbolizeDLTB(string rootPath , string _sourceFileGDB, string sourcedltbfclsName)
        {
            //获取模板文件名
            try
            {
                string templateGDBFileName = rootPath + "\\综合数据库\\综合数据库.gdb";
                if (!Directory.Exists(templateGDBFileName))
                {
                    MessageBox.Show("模板数据文件不存在！");
                    return false;
                }

                IWorkspaceFactory sourceWSFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace sourceWorkspace = sourceWSFactory.OpenFromFile(_sourceFileGDB, 0);
                IFeatureWorkspace sourceFeatureWorkspace = (IFeatureWorkspace)sourceWorkspace;

                //读取数据模板
                IWorkspaceFactory tempWSFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace tempWorkspace = tempWSFactory.OpenFromFile(templateGDBFileName, 0);

                Geoprocessor mGP = new Geoprocessor();
                mGP.OverwriteOutput = true;
                //将模板中DLTB拷贝到综合后gdb中
                ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass pFCTFC = new ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass();
                pFCTFC.in_features = templateGDBFileName + "\\DLTB";
                pFCTFC.out_path = _sourceFileGDB;
                pFCTFC.out_name = sourcedltbfclsName + "Update";
                mGP.Execute(pFCTFC, null);

                mGP.SetEnvironmentValue("workspace", _sourceFileGDB);
                //定义投影
                IFeatureClass sourceFC = sourceFeatureWorkspace.OpenFeatureClass(sourcedltbfclsName);
                IFeatureClass targetFC = sourceFeatureWorkspace.OpenFeatureClass(sourcedltbfclsName + "Update");

                var sourceSpatialReference = (sourceFC as IGeoDataset).SpatialReference;
                var tempSpatialReference = (targetFC as IGeoDataset).SpatialReference;

                if (sourceSpatialReference.Name != tempSpatialReference.Name)
                {
                    Project gpproject = new Project();
                    gpproject.in_dataset = sourcedltbfclsName + "Update";
                    gpproject.out_dataset = sourcedltbfclsName + "Update" + "project";
                    gpproject.out_coor_system = sourceSpatialReference;
                    mGP.Execute(gpproject, null);
                    //重命名
                    if ((targetFC as IDataset).CanDelete())
                    {
                        (targetFC as IDataset).Delete();
                    }
                    IDataset ds = sourceFeatureWorkspace.OpenFeatureClass(sourcedltbfclsName + "Update" + "project") as IDataset;
                    if (ds != null && ds.CanRename())
                    {
                        ds.Rename(sourcedltbfclsName + "Update");
                        targetFC = sourceFeatureWorkspace.OpenFeatureClass(sourcedltbfclsName + "Update");
                    }
                }

                //拷贝要素数据到导出的文件数据库
                //bool bSuccess = CopyFeatureClassFromOriginToTarget(rootPath, sourceFC, targetFC);
                Console.WriteLine("正在复制要素....");
                bool bSuccess = CopyFeatureClassFromOriginToTargetGP(_sourceFileGDB + "\\" + sourceFC.AliasName, _sourceFileGDB + "\\" + targetFC.AliasName);
                //改名
                if ((sourceFC as IDataset).CanDelete())
                {
                    (sourceFC as IDataset).Delete();
                    if ((targetFC as IDataset).CanDelete())
                    {
                        (targetFC as IDataset).Rename(sourcedltbfclsName);
                    }
                }

                if (!bSuccess)
                {
                    MessageBox.Show("拷贝要素数据到导出的文件数据库失败！");
                    return false;
                }
                string ruleMatchFileName = rootPath + "\\综合数据库\\规则对照.mdb";
                System.Data.DataTable ruleMDB =ReadToDataTable(ruleMatchFileName, "图层对照规则");
                System.Data.DataTable dtLayerRule = ruleMDB;
                string layerMathName = "图层";
                DataRow[] drArray = dtLayerRule.Select().Where(i => i[layerMathName].ToString().Trim() == "DLTB").ToArray();
                {
                    mWo.SetText("正在进行符号化DLTB...");
                    for (int i = 0; i < drArray.Length; i++)
                    {
                        string whereClause = drArray[i]["定义查询"].ToString().Trim();
                        string lyrOrginName = drArray[i]["图层"].ToString().Trim();
                        string lyrMappingName = drArray[i]["映射图层"].ToString().Trim();
                        //string lyrMappingName = lyrOrginName;
                        int ruleID = -1;
                        string ruleName = drArray[i]["RuleName"].ToString().Trim();
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = whereClause.ToString();
                        if (drArray[i]["RuleName"].ToString() == "不显示要素" && whereClause == "")
                        {
                            continue;
                        }
                        ruleID = GetRuleIDByRuleName(targetFC, ruleName);

                        string ruleFieldName = drArray[i]["RuleIDFeildName"].ToString();
                        if (ruleFieldName != "RuleID")
                        {
                            ruleFieldName = "RuleID";
                        }
                        if (ruleID == -1)
                        {
                            int.TryParse(drArray[i]["RuleID"].ToString().Trim(), out ruleID);
                        }
                        if (ruleID == -1 || ruleID == 0)
                        {
                            continue;
                        }
                        try
                        {
                            IFeatureCursor fCursor = targetFC.Update(qf, true);
                            IFeature f = null;
                            while ((f = fCursor.NextFeature()) != null)
                            {
                                f.set_Value(targetFC.FindField(ruleFieldName), ruleID);
                                fCursor.UpdateFeature(f);
                                //内存释放
                                Marshal.ReleaseComObject(f);
                            }
                            Marshal.ReleaseComObject(fCursor);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            System.Diagnostics.Trace.WriteLine(ex.Source);
                            System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                            //MessageBox.Show(ex.Message);
                        }
                    }

                }
            }
            catch
            {
                return false;
            }


            return true;

        }
        public  void BackupDLTB(string gdbPath, string infclsName, string outfclsName)
        {

            
                mWo.SetText("正在进行原始DLTB符号化升级");
                Geoprocessor mGP = new Geoprocessor();
                mGP.OverwriteOutput = true;
                ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass pFCTFC = new ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass();
                pFCTFC.in_features = gdbPath + "\\" + infclsName;
                pFCTFC.out_path = gdbPath;
                pFCTFC.out_name = outfclsName;
                mGP.Execute(pFCTFC, null);
             
        }


        private void gp_ToolExecuted(object sender, ToolExecutedEventArgs e)
        {

            IGeoProcessorResult2 gpResult = (IGeoProcessorResult2)e.GPResult;
            Console.WriteLine(gpResult.Process.Tool.Name + " 结束" + gpResult.Status.ToString());
            if (gpResult.Status == esriJobStatus.esriJobSucceeded)
            {
                if (mGPToolsToExecute.Count > 0)
                {
                    IGPProcess gpTool = mGPToolsToExecute.Dequeue();
                    mGP.ExecuteAsync(gpTool);
                }
                else
                {
                    mGP.ToolExecuting -= new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                    mGP.ToolExecuted -= new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                    mGPFished = true;
                    System.Timers.Timer tr = new System.Timers.Timer();
                    tr.Interval = 5000;
                    tr.Elapsed += (o, ee) => {

                        mGPWorkerEvent.Set();
                        tr.Stop(); 
                        tr.Dispose();
                    };
                    tr.Start();
                   
                }

            }

            else if (gpResult.Status == esriJobStatus.esriJobFailed)
            {

                Console.WriteLine(gpResult.Process.Tool.Name + "执行失败");
                string ms = "";
                if (gpResult.MessageCount > 0)
                {
                    for (int Count = 0; Count < gpResult.MessageCount; Count++)
                    {
                        ms += gpResult.GetMessage(Count);
                    }
                }
                Console.WriteLine(ms);
                MessageBox.Show(ms);
                mGPFished = true;
                mGPWorkerEvent.Set();
            }
            
        }
        private void gp_ToolExecuting(object sender, ToolExecutingEventArgs e)
        {
            IGeoProcessorResult2 gpResult = (IGeoProcessorResult2)e.GPResult;
            Console.WriteLine(gpResult.Process.Tool.Name + " " + gpResult.Status.ToString());

        }
       
       
        private AutoResetEvent mGPWorkerEvent = new AutoResetEvent(false);
        private bool CopyFeatureClassFromOriginToTargetGP0(string input, string outPut)
        {
            Append gpAppend = new Append();
            try
            {
                gpAppend.inputs = input;
                gpAppend.target = outPut;
                gpAppend.schema_type = "NO_TEST";
                mGPToolsToExecute.Clear();
                mGPToolsToExecute.Enqueue(gpAppend);
                mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
            }
            catch
            {
            }
            mGPWorkerEvent.WaitOne();

            return true;
        }
        private  bool CopyFeatureClassFromOriginToTargetGP(string input, string outPut)
        {
            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("TBAppend");
                cmd.Add(new XElement("InFeature", input));//
                cmd.Add(new XElement("OutFeature", outPut));//
                root.Add(cmd);
                doc.Add(root);
                string tempxml = Application.StartupPath + "\\autoTBSymAppend.xml";
                if (File.Exists(tempxml))
                {
                    File.Delete(tempxml);
                }
                doc.Save(tempxml);

                string exePath = Application.StartupPath + @"\TBToolShell.exe";
                Process p = null;
                ProcessStartInfo si = null;
                int pid = -1;
                using (p = new Process())
                {
                    si = new ProcessStartInfo();
                    si.FileName = exePath;
                    si.Arguments = string.Format("\"{0}\"", tempxml);
                    si.UseShellExecute = true;
                    si.CreateNoWindow = false;
                    p.StartInfo = si;
                    p.Start();
                    pid = p.Id;

                }
                int tempTime = 0;
                try
                {
                    while (true)
                    {
                        using (var pp = Process.GetProcessById(pid))
                        {
                            System.Threading.Thread.Sleep(500);
                            tempTime += 500;
                        }
                    }
                }
                catch
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                return false;
            }


            return true;
        }
        


        private void RunTool(Geoprocessor geoprocessor, IGPProcess process, ITrackCancel TC = null)
        {
            geoprocessor.OverwriteOutput = true;
            try
            {
                geoprocessor.Execute(process, null);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                string ms = "";
                if (geoprocessor.MessageCount > 0)
                {
                    for (int Count = 0; Count < geoprocessor.MessageCount; Count++)
                    {
                        ms += geoprocessor.GetMessage(Count);
                    }
                }
                MessageBox.Show(ms);
            }
        }
        private DataTable ReadToDataTable(string mdbFilePath, string tableName)
        {

            if (!File.Exists(mdbFilePath))
                return null;
            DataTable pDataTable = new DataTable();
            try
            {
                #region GIS 方式读取MDB
                IWorkspaceFactory pWorkspaceFactory = new AccessWorkspaceFactory();
                IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(mdbFilePath, 0);
                IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTTable);
                pEnumDataset.Reset();
                IDataset pDataset = pEnumDataset.Next();
                ITable pTable = null;
                while (pDataset != null)
                {
                    if (pDataset.Name == tableName)
                    {
                        pTable = pDataset as ITable;
                        break;
                    }
                    pDataset = pEnumDataset.Next();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
                if (pTable != null)
                {
                    ICursor pCursor = pTable.Search(null, false);
                    IRow pRow = pCursor.NextRow();
                    //添加表的字段信息
                    for (int i = 0; i < pRow.Fields.FieldCount; i++)
                    {
                        pDataTable.Columns.Add(pRow.Fields.Field[i].Name);
                    }
                    //添加数据
                    while (pRow != null)
                    {
                        DataRow dr = pDataTable.NewRow();
                        for (int i = 0; i < pRow.Fields.FieldCount; i++)
                        {
                            object obValue = pRow.get_Value(i);
                            if (obValue != null && !Convert.IsDBNull(obValue))
                            {
                                dr[i] = pRow.get_Value(i);
                            }
                            else
                            {
                                dr[i] = "";
                            }
                        }
                        pDataTable.Rows.Add(dr);
                        pRow = pCursor.NextRow();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
                }
                #endregion
            }
            catch
            {
                GC.Collect();
                return null;
            }
            return pDataTable;
        }
  
        
        /// <summary>
        /// 根据RuleName获取RuleID值
        /// </summary>
        /// <param name="fclName"></param>
        /// <param name="ruleName"></param>
        /// <returns></returns>
        public  int GetRuleIDByRuleName(IFeatureClass fcls, string ruleName)
        {
            IRepresentationRules rules = GetRepresentationRules(fcls);
            rules.Reset();
            IRepresentationRule rule = null;
            int ruleID;
            while (true)
            {
                rules.Next(out ruleID, out rule);
                if (rule == null) break;
                if (rules.get_Name(ruleID).Trim().Replace(" ", "") == ruleName.Trim().Replace(" ", ""))//不考虑空白
                {
                    return ruleID;
                }
            }
            return -1;
        }
        private  IRepresentationRules GetRepresentationRules(IFeatureClass fcls)
        {
            try
            {
                var path = (fcls as IDataset).Workspace.PathName;
                IWorkspaceExtensionManager wem = (fcls as IDataset).Workspace as IWorkspaceExtensionManager;
                UID uid = new UIDClass();
                uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
                IRepresentationWorkspaceExtension repWS = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
                IEnumDatasetName enumDatasetName = repWS.get_FeatureClassRepresentationNames(fcls);
                enumDatasetName.Reset();
                IDatasetName pDatasetName = enumDatasetName.Next();
                IRepresentationClass m_RepClass = repWS.OpenRepresentationClass(pDatasetName.Name);
                var rules = m_RepClass.RepresentationRules;
                rules.Reset();
                return rules;
            }
            catch
            {
                return null;
            }
        }


        private  bool CopyFeatureClassFromOriginToTargetTh(GApplication m_Application, IFeatureClass originFeatureclass, IFeatureClass targetFeatureclass)
        {

            try
            {
                IFeatureClassLoad pFCLoad = targetFeatureclass as IFeatureClassLoad;
                pFCLoad.LoadOnlyMode = true;

                //获取不一样属性字段的索引
                int indexDate_orgin = -1;
                int indexDate_target = -1;

                if (originFeatureclass.FindField("DATE_") > 0)
                {
                    indexDate_orgin = originFeatureclass.FindField("DATE_");
                    indexDate_target = targetFeatureclass.FindField("DATE");
                }

                int indexNames_origin = -1;
                int indexNames_target = -1;
                if (originFeatureclass.FindField("NAMES_") > 0)
                {
                    indexNames_origin = originFeatureclass.FindField("NAMES_");
                    indexNames_target = targetFeatureclass.FindField("NAMES");
                }

                int indexTime_origin = -1;
                int indexTime_target = -1;
                if (originFeatureclass.FindField("TIME_") > 0)
                {
                    indexTime_origin = originFeatureclass.FindField("TIME_");
                    indexTime_target = targetFeatureclass.FindField("TIME");
                }

                //遍历赋值
                var wo = mWo;
                {
                    if (originFeatureclass.FeatureCount(null) > 0)
                    {
                        IList<string> fieldNameList = new List<string>();
                        fieldNameList = GetAttributeList(originFeatureclass, true);

                        IFeatureCursor targetCursor = targetFeatureclass.Insert(true);
                        IFeatureCursor pFeatureCursor = originFeatureclass.Search(null, false);
                        IFeature pFeature = null;
                        int featureCount = originFeatureclass.FeatureCount(null);
                        int step = 0;
                        while ((pFeature = pFeatureCursor.NextFeature()) != null)
                        {
                            IFeatureBuffer newFeatureBuf = targetFeatureclass.CreateFeatureBuffer();
                            IGeometry geo = pFeature.Shape as IGeometry;
                            newFeatureBuf.Shape = geo;
                            step++;
                            if (step % 100 == 0)
                            {
                                wo.SetText("正在复制DLTB，当前：" + step + "/" + featureCount);
                            }
                            //属性赋值
                            for (int j = 0; j < fieldNameList.Count; j++)
                            {
                                if (fieldNameList[j] == "DATE" && indexDate_orgin > 0)
                                {
                                    IField feild = targetFeatureclass.Fields.get_Field(indexDate_orgin);
                                    if (feild.Editable)
                                    {
                                        object value = pFeature.get_Value(indexDate_orgin);
                                        if (value.ToString() != "" && value != null)
                                        {
                                            newFeatureBuf.set_Value(indexDate_target, value);
                                        }
                                    }
                                }
                                else if (fieldNameList[j] == "NAMES" && indexNames_origin > 0)
                                {
                                    IField feild = targetFeatureclass.Fields.get_Field(indexNames_origin);
                                    if (feild.Editable)
                                    {
                                        object value = pFeature.get_Value(indexNames_origin);
                                        if (value.ToString() != "" && value != null)
                                        {
                                            newFeatureBuf.set_Value(indexNames_target, value);
                                        }
                                    }
                                }
                                else if (fieldNameList[j] == "TIME" && indexTime_origin > 0)
                                {
                                    IField feild = targetFeatureclass.Fields.get_Field(indexTime_origin);
                                    if (feild.Editable)
                                    {
                                        object value = pFeature.get_Value(indexTime_origin);
                                        if (value.ToString() != "" && value != null)
                                        {
                                            newFeatureBuf.set_Value(indexTime_target, value);
                                        }
                                    }
                                }
                                else
                                {
                                    int indexOrigin = originFeatureclass.FindField(fieldNameList[j]);
                                    int indexTarget = targetFeatureclass.FindField(fieldNameList[j]);
                                    if (indexTarget != -1 && indexOrigin != -1)
                                    {
                                        IField feild = targetFeatureclass.Fields.get_Field(indexTarget);
                                        if (feild.Editable)
                                        {
                                            object value = pFeature.get_Value(indexOrigin);
                                            if (value.ToString() != "" && value != null)
                                            {
                                                newFeatureBuf.set_Value(indexTarget, value);
                                            }
                                        }
                                    }
                                }
                            }
                            targetCursor.InsertFeature(newFeatureBuf);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeature);
                        }
                        targetCursor.Flush();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(targetCursor);
                    }
                    pFCLoad.LoadOnlyMode = false;


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }
        //得到一个要素类的属性信息列表(可以判断是否加上系统属性字段)
        private  IList<string> GetAttributeList(IFeatureClass fc, bool bIgnoreSystemField = true)
        {

            IList<string> fieldList = new List<string>();

            for (int i = 0; i < fc.Fields.FieldCount; i++)
            {
                IField pField = fc.Fields.get_Field(i);
                string fieldName = pField.Name;

                //判断是否过滤系统属性字段
                if (bIgnoreSystemField)
                {
                    if (fieldName != fc.OIDFieldName && fieldName != fc.ShapeFieldName && pField != fc.LengthField && pField != fc.AreaField)
                    {
                        if (fieldName.StartsWith("RULEID") || fieldName.StartsWith("OVERRIDE"))
                        {
                            continue;
                        }
                        fieldList.Add(fieldName);
                    }
                }
                else
                {
                    fieldList.Add(fieldName);
                }
            }
            return fieldList;
        }

         
        
      

    }
}

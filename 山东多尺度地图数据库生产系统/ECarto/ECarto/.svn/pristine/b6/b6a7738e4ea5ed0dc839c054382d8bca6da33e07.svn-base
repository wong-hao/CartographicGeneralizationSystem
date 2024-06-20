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
using System.Xml.Linq;
using System.Diagnostics;
namespace ShellTBDivided
{
    /// <summary>
    /// GP融合
    /// </summary>
    public class TBGPClass
    {
        Geoprocessor mGP = new Geoprocessor();
        WaitOperation mWo = new WaitOperation();
        public TBGPClass()
        {
            mGP.OverwriteOutput = true;
        }
        bool mGPFished = false;
        public bool DisOverWrite = true;
        private AutoResetEvent mGPWorkerEvent = new AutoResetEvent(false);
        /// <summary>
        /// Dis融合；默认将删除以前的图层
        /// </summary>
        /// <param name="infeature"></param>
        /// <param name="outfeature"></param>
        /// <param name="field"></param>
        public void Dissolve(string gdb, string infeature,string outfeature, string field,string field2)
        {
            try
            {
                string disfield = field;
                if (field2 != null)
                {
                    if(!disfield.Contains(field2))
                    {
                      disfield = field + ";" + field2;
                    }
                }
                Dissolve gpDis = new Dissolve { in_features = gdb + "\\" + infeature, out_feature_class = gdb + "\\" + outfeature, dissolve_field = disfield, multi_part = "SINGLE_PART" };

                mGPToolsToExecute.Clear();
                mGPToolsToExecute.Enqueue(gpDis);
                mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);

                mGPWorkerEvent.WaitOne();
                if (!DisOverWrite)
                    return;

                try
                {
                    IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                    IWorkspace workspace = wsFactory.OpenFromFile(gdb, 0);
                    IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)wsFactory;//注意在java api中不能强转切记需要IWorkspaceFactoryLockControl ipWsFactoryLock = new IWorkspaceFactoryLockControlProxy(pwf);
                    if (ipWsFactoryLock.SchemaLockingEnabled)
                    {
                        ipWsFactoryLock.DisableSchemaLocking();
                    }
                    if ((workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, outfeature))
                    {

                        IDataset oldDs = (workspace as IFeatureWorkspace).OpenFeatureClass(infeature) as IDataset;
                        if (oldDs != null)
                            oldDs.Delete();
                        //重命名
                        IDataset ds = (workspace as IFeatureWorkspace).OpenFeatureClass(outfeature) as IDataset;
                        if (ds != null && ds.CanRename())
                            ds.Rename(infeature);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("融合重命名错误:" + ex.Message);
                }
            }
            catch
            {

            }
        }

        public void AppendFeature(string input, string outPut)
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
        }

        public void ExcuteTBErase(string  infeature,string eraseFeature,string gdbPath)
        {
            string fclName = infeature;
            string fclErase = eraseFeature;
            string fclOut = gdbPath + "//DLTB1";
            string fclOut1 = gdbPath + "//DLTB1S";


            ESRI.ArcGIS.AnalysisTools.Erase gpErase = new ESRI.ArcGIS.AnalysisTools.Erase { in_features = fclName, erase_features = fclErase, out_feature_class = fclOut };
            MultipartToSinglepart gpMs = new MultipartToSinglepart(fclOut, fclOut1);
            Delete gpDelete = new Delete();
            gpDelete.in_data = fclOut;
            Delete gpDelete1 = new Delete();
            gpDelete1.in_data = infeature;

            mGPToolsToExecute.Enqueue(gpErase);
            mGPToolsToExecute.Enqueue(gpMs);
            mGPToolsToExecute.Enqueue(gpDelete);
            mGPToolsToExecute.Enqueue(gpDelete1);

            mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
            mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
            mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
            mGPWorkerEvent.WaitOne();
            try
            {
                IWorkspaceFactory wf = new FileGDBWorkspaceFactoryClass();
                var fs = wf.OpenFromFile(gdbPath, 0) as IFeatureWorkspace;
                if ((fs as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "DLTB1S"))
                {
                    IDataset ds = fs.OpenFeatureClass("DLTB1S") as IDataset;
                    if (ds != null && ds.CanRename())
                        ds.Rename("DLTB");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("擦除重命名失败:"+ex.Message);
            }
        }

        /// <summary>
        /// 处理综合的缝隙//wjz修改：将boul改为xzq面。
        /// </summary>
        public void ExcuteTBGap(string gdbPath,string infeature,string bouafeature,double scale)
        {

            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = mGP;
            gp.SetEnvironmentValue("workspace", gdbPath);
            string fclName = infeature;
            string bouaName = bouafeature;
            //mGPToolsToExecute.Clear();
            try
            {
                
             
                mWo.SetText("境界融合");
                Dissolve gpDis = new Dissolve();
                gpDis.in_features = bouaName;
                gpDis.out_feature_class = bouaName + "0";
                mGPToolsToExecute.Enqueue(gpDis);
                //mGP.Execute(gpDis, null);
                mWo.SetText("境界裁切");
                //1.裁切
                ESRI.ArcGIS.AnalysisTools.Clip gpClip = new ESRI.ArcGIS.AnalysisTools.Clip();
                gpClip.in_features = fclName;
                gpClip.clip_features = bouaName + "0";
                gpClip.out_feature_class = fclName + "Clip";
                //mGP.Execute(gpClip, null);
                mGPToolsToExecute.Enqueue(gpClip);
                mWo.SetText("境界裁切取反");
                //2.取反得到缝隙
                SymDiff gpDiff = new SymDiff();
                gpDiff.in_features = fclName;
                gpDiff.update_features = bouaName + "0";
                gpDiff.out_feature_class = fclName + "Diff0";
                //mGP.Execute(gpDiff, null);
                mGPToolsToExecute.Enqueue(gpDiff);

                ESRI.ArcGIS.AnalysisTools.Clip gpClip1 = new ESRI.ArcGIS.AnalysisTools.Clip();
                gpClip1.in_features = fclName + "Diff0";
                gpClip1.clip_features = bouaName + "0";
                gpClip1.out_feature_class = fclName + "Diff";
                //mGP.Execute(gpClip1, null);
                mGPToolsToExecute.Enqueue(gpClip1);

                mWo.SetText("补齐缝隙");
                //3.补齐缝隙
                Update gpUpdate = new Update();
                gpUpdate.in_features = fclName + "Clip";
                gpUpdate.update_features = fclName + "Diff";
                gpUpdate.out_feature_class = fclName + "Update";
                //mGP.Execute(gpUpdate, null);
                mGPToolsToExecute.Enqueue(gpUpdate);
               
                //4.打散
                MultipartToSinglepart gpMs = new MultipartToSinglepart(fclName + "Update", fclName + "Ms");
                //mGP.Execute(gpMs, null);
                mGPToolsToExecute.Enqueue(gpMs);


                mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                mGPWorkerEvent.WaitOne();
                mWo.SetText("融合缝隙图斑");
                //5.融合缝隙图斑
                ElimateTBShell(fclName + "Ms", "DLBM =''", gdbPath);
                //消除1.5平方毫米以下要素
                ElimateTBShell(fclName + "Ms", 0.5 * scale * scale / 1000 / 1000, gdbPath);
                ElimateTBShell(fclName + "Ms", 1 * scale * scale / 1000 / 1000, gdbPath);
                ElimateTBShell(fclName + "Ms", 1.5 * scale * scale / 1000 / 1000, gdbPath);
                       
                mWo.SetText("删除临时图层");
               // gp.Execute(new Delete { in_data = bouaName + "Polygon" }, null);

                gp.Execute(new Delete { in_data = bouaName + "0" }, null);
                gp.Execute(new Delete { in_data = fclName + "Clip" }, null);
                gp.Execute(new Delete { in_data = fclName + "Diff" }, null);
                gp.Execute(new Delete { in_data = fclName + "Diff0" }, null);
                gp.Execute(new Delete { in_data = fclName + "Update" }, null);
                gp.Execute(new Delete { in_data = fclName}, null);
                //重命名
                IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace  mWorkSpace = wsFactory.OpenFromFile(gdbPath, 0);
                try
                {
                    IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB"+ "Ms") as IDataset;
                    if (ds != null && ds.CanRename())
                    {
                        ds.Rename("DLTB");                        
                    }
                    IFeatureClass fclOrgin = null;
                    if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "DLTB原始"))
                    {
                        fclOrgin = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB原始");
                    }
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = "DLBM =''";
                    ISpatialFilter sf = new SpatialFilterClass();
                    if ((ds as IFeatureClass).FeatureCount(qf) > 0)
                    {
                        IFeatureCursor fCursor = (ds as IFeatureClass).Update(qf, false);
                        IFeature f = null;
                        int index = (ds as IFeatureClass).FindField("DLBM");
                        while ((f = fCursor.NextFeature()) != null)
                        {
                            f.set_Value(index, "1105");
                            if (fclOrgin != null)
                            {

                                string gbCode = string.Empty;
                                sf.Geometry = f.Shape;
                                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                                var cursor = fclOrgin.Search(sf, false);
                                IFeature fe = cursor.NextFeature();
                                if (fe != null)
                                {
                                    gbCode = fe.get_Value(fclOrgin.FindField("DLBM")).ToString();
                                    f.set_Value(index, gbCode);
                                }
                                Marshal.ReleaseComObject(cursor);
                            }
                            fCursor.UpdateFeature(f);
                        }
                        Marshal.ReleaseComObject(fCursor);
                        Marshal.ReleaseComObject(qf);
                        Marshal.ReleaseComObject(sf);
                    }


                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                
              
            }
            catch (Exception ex)
            {
                var info = ex.ToString();
                for (var i = 0; i < gp.MessageCount; i++) info += gp.GetMessage(i);
                MessageBox.Show(info);
            }
        }
        private void ElimateTBShell(string fclName, string sql, string gdbPath)
        {
            string name = fclName;

            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("ElimateTB");
                cmd.Add(new XElement("InFeature", name));//
                cmd.Add(new XElement("GDB", gdbPath));//
                cmd.Add(new XElement("Scale", "1"));//
                cmd.Add(new XElement("SQL", sql));//
                root.Add(cmd);
                doc.Add(root);
                string tempxml = Application.StartupPath + "\\ElimateTB.xml";
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
                    return;
                }

            }
            catch (Exception ex)
            {

            }

        }
        private void ElimateTBShell(string fclName, double area, string gdbPath)
        {
            string name = fclName;

            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("ElimateTB");
                cmd.Add(new XElement("InFeature", name));//
                cmd.Add(new XElement("GDB", gdbPath));//
                cmd.Add(new XElement("Scale", "1"));//
                cmd.Add(new XElement("Area", area));//
                root.Add(cmd);
                doc.Add(root);
                string tempxml = Application.StartupPath + "\\ElimateTB.xml";
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
                    return;
                }

            }
            catch (Exception ex)
            {

            }

        }
        public void ElimateTB(string gdbPath, double mapScale)
        {
            double cellSize = mapScale / 10000;
            double area = cellSize * 5;
         
           


            mWo.SetText("正在消除小图斑,面积指标：" + area);


            //创建Layer
            Geoprocessor gp = mGP;
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", gdbPath);
            ESRI.ArcGIS.DataManagementTools.Eliminate pElim = new ESRI.ArcGIS.DataManagementTools.Eliminate();
            ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer pMakeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
            pMakeFeatureLayer.in_features = "DLTB";
            pMakeFeatureLayer.out_layer = "DLTBElimiateLayer";
            gp.Execute(pMakeFeatureLayer, null);
            //通过Layer选择
            ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute pSelectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
            pSelectLayerByAttribute.in_layer_or_view = "DLTBElimiateLayer";
            pSelectLayerByAttribute.where_clause = '"' + "Shape_Area" + '"' + "<=" + area;
            gp.Execute(pSelectLayerByAttribute, null);
         
            pElim.in_features = "DLTBElimiateLayer";
            pElim.out_feature_class = "DLTBElimate";
            //gp.Execute(pElim, null);
            mGPToolsToExecute.Clear();
            mGPToolsToExecute.Enqueue(pElim);
            mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
            mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
            mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
            mGPWorkerEvent.WaitOne();


            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            var mWorkSpace = workspaceFactory.OpenFromFile(gdbPath, 0);
            try
            {
                if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "DLTB"))
                {
                    IFeatureClass dltbFcls = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB");
                    IDataset dltbds = dltbFcls as IDataset;
                    if (dltbds.CanDelete())
                    {
                        dltbds.Delete();
                    }
                }
                if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, "DLTB"))
                {
                    ITable dltbFcls = (mWorkSpace as IFeatureWorkspace).OpenTable("DLTB");
                    IDataset dltbds = dltbFcls as IDataset;
                    if (dltbds.CanDelete())
                    {
                        dltbds.Delete();
                    }
                }
                IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTBElimate") as IDataset;
                if (ds != null && ds.CanRename())
                    ds.Rename("DLTB");
            }
            catch(Exception ex)
            {
                MessageBox.Show("重命名错误："+ex.Message);
            }

        }
        public void ElimateDltb(string gdbPath,double area = 10)
        {
            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            var mWorkSpace = workspaceFactory.OpenFromFile(gdbPath, 0);
            IFeatureClass dltbFcls = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB");
            //IQueryFilter filter = new QueryFilterClass();
            //filter.WhereClause = '"' + "Shape_Area" + '"' + "<=" + area;
            //if (dltbFcls.FeatureCount(filter) < 10000)//数量少于10000则不进行消除
            //{
            //    return;
            //}
         
           
               mWo.SetText("正在消除小图斑,面积指标：" + area);
            

            //创建Layer
               Geoprocessor gp = mGP;
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", gdbPath);
            ESRI.ArcGIS.DataManagementTools.Eliminate pElim = new ESRI.ArcGIS.DataManagementTools.Eliminate();
            ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer pMakeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
            pMakeFeatureLayer.in_features = "DLTB";
            pMakeFeatureLayer.out_layer = "DLTBElimiateLayer";
            //pMakeFeatureLayer.workspace = mFolderPath;
            gp.Execute(pMakeFeatureLayer, null);
            //通过Layer选择
            ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute pSelectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
            pSelectLayerByAttribute.in_layer_or_view = "DLTBElimiateLayer";
            pSelectLayerByAttribute.where_clause = '"' + "Shape_Area" + '"' + "<=" + area;
            gp.Execute(pSelectLayerByAttribute, null);
            pElim.in_features = "DLTBElimiateLayer";

            pElim.out_feature_class = "DLTBElimate";
            gp.Execute(pElim, null);

            IDataset dltbds = dltbFcls as IDataset;
            if (dltbds.CanDelete())
            {
                dltbds.Delete();
            }
            IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTBElimate") as IDataset;
            if (ds != null && ds.CanRename())
                ds.Rename("DLTB");

        }
        /// <summary>
        /// 面转线耗内存
        /// </summary>
        /// <param name="infeature"></param>
        /// <param name="outfeature"></param>
        public void PolygonToLine(string infeature, string outfeature)
        {

            ESRI.ArcGIS.DataManagementTools.PolygonToLine polygontoLine = new ESRI.ArcGIS.DataManagementTools.PolygonToLine(infeature, outfeature);
            polygontoLine.neighbor_option = "IDENTIFY_NEIGHBORS";
            mGPToolsToExecute.Clear();
            mGPToolsToExecute.Enqueue(polygontoLine);

            mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
            mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
            mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
            mGPWorkerEvent.WaitOne();
            

        }

        #region 房屋综合
        private double mAggregateDis;

        public double MAggregateDis
        {
            get { return mAggregateDis; }
            set { mAggregateDis = value; }
        }
        private double mMinArea;

        public double MMinArea
        {
            get { return mMinArea; }
            set { mMinArea = value; }
        }
        private double mHoleArea;

        public double MHoleArea
        {
            get { return mHoleArea; }
            set { mHoleArea = value; }
        }
        private bool mOrthogonality;

        public bool MOrthogonality
        {
            get { return mOrthogonality; }
            set { mOrthogonality = value; }
        }
        private double mSimplifyDis;

        public double MSimplifyDis
        {
            get { return mSimplifyDis; }
            set { mSimplifyDis = value; }
        }
        private double mMinAreaRESA;

        public double MMinAreaRESA
        {
            get { return mMinAreaRESA; }
            set { mMinAreaRESA = value; }
        }
        private bool mConflict;

        public bool MConflict
        {
            get { return mConflict; }
            set { mConflict = value; }
        }
        private string mField="DLTB";

        public string MField
        {
            get { return mField; }
            set { mField = value; }
        }
        #endregion
        private void AddCustomField(IFeatureClass fCls, string fieldName)
        {
            int index = fCls.FindField(fieldName);
            if (index != -1)
            {
                return;
            }
            IFields pFields = fCls.Fields;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.AliasName_2 = fieldName;
            pFieldEdit.Length_2 = 50;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            IClass pTable = fCls as IClass;
            pTable.AddField(pField);
            pFieldsEdit = null;
            pField = null;
        }
        public void BuildingGeneralize(double scale,string fclName,string gdpPath,string msql="")
        {
            mGPToolsToExecute.Clear();
            Console.WriteLine("房屋综合...");
            mGP.SetEnvironmentValue("workspace", gdpPath);
            mGP.OverwriteOutput = true;
            //// Console.WriteLine("1 设置图层...");
            //MakeFeatureLayer gpMFL = new MakeFeatureLayer { in_features = fclName, out_layer = fclName + "_Layer" };
            ////   mGPToolsToExecute.Enqueue(gpMFL);
            //mGP.Execute(gpMFL, null);
            ////  Console.WriteLine("2 筛选房屋要素...");
            string sql = "DLBM like '0701%' or DLBM like '0702%'";
            if (msql != "")
                sql = msql;
            //SelectLayerByAttribute gpSL = new SelectLayerByAttribute { in_layer_or_view = fclName + "_Layer", where_clause = sql };
            //// mGPToolsToExecute.Enqueue(gpSL);
            //mGP.Execute(gpSL, null);
            //CopyFeatures gpCopy = new CopyFeatures { in_features = fclName + "_Layer", out_feature_class = fclName + "_Building" };
            ////mGPToolsToExecute.Enqueue(gpCopy);
            //mGP.Execute(gpCopy, null);

            ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass pFC2FC = new ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass();
            pFC2FC.in_features = fclName;
            pFC2FC.out_path = gdpPath;
            pFC2FC.out_name = fclName + "_Building";
            if (sql.Trim() != string.Empty && fclName.ToLower()!="building")
            {
                pFC2FC.where_clause = sql;
            }
            mGPToolsToExecute.Enqueue(pFC2FC);
            // Console.WriteLine("3 聚合面处理...");
            ESRI.ArcGIS.CartographyTools.AggregatePolygons gpAp = new ESRI.ArcGIS.CartographyTools.AggregatePolygons
            {
                in_features = fclName + "_Building",
                out_feature_class = fclName + "_Building1",
                aggregation_distance = mAggregateDis * scale * 0.001,
                minimum_area = mMinArea * 0.000001 * scale * scale,
                minimum_hole_size = mHoleArea * 0.000001 * scale * scale,
                orthogonality_option = mOrthogonality ? "ORTHOGONAL" : "NON_ORTHOGONAL"
            };
            mGPToolsToExecute.Enqueue(gpAp);
            //Tips("4 简化建筑物处理...");
            ESRI.ArcGIS.CartographyTools.SimplifyBuilding gpBuilding = new ESRI.ArcGIS.CartographyTools.SimplifyBuilding
            {
                in_features = fclName + "_Building1",
                out_feature_class = fclName + "_Building2",
                simplification_tolerance = mSimplifyDis * 0.001 * scale,
                minimum_area = mMinAreaRESA * 0.000001 * scale * scale,
                conflict_option = mConflict ? "CHECK_CONFLICTS" : "NO_CHECK"
            };
            mGPToolsToExecute.Enqueue(gpBuilding);

            // Tips("5 重做简化建筑物处理...");
            ESRI.ArcGIS.CartographyTools.SimplifyBuilding gpBuilding1 = new ESRI.ArcGIS.CartographyTools.SimplifyBuilding
            {
                in_features = fclName + "_Building2",
                out_feature_class = "Building",
                simplification_tolerance = mSimplifyDis * 0.001 * scale,
                minimum_area = mMinAreaRESA * 0.000001 * scale * scale,
                conflict_option = mConflict ? "CHECK_CONFLICTS" : "NO_CHECK"
            };
            mGPToolsToExecute.Enqueue(gpBuilding1);
            //Tips("6 修复几何...");
            RepairGeometry gpRepair = new RepairGeometry { in_features = "Building", delete_null = "DELETE_NULL" };
            mGPToolsToExecute.Enqueue(gpRepair);
            //添加字段
            AddField gpAdd = new AddField { in_table = "Building", field_name = mField, field_type = "TEXT", field_length = 50 };
            mGPToolsToExecute.Enqueue(gpAdd);
            // Tips("7 删除临时图层···");
            Delete gpDelete1 = new Delete { in_data = fclName + "_Building1" };
            Delete gpDelete2 = new Delete { in_data = fclName + "_Building2" };
            mGPToolsToExecute.Enqueue(gpDelete1);
            mGPToolsToExecute.Enqueue(gpDelete2);
            mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
            mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
            mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);

            mGPWorkerEvent.WaitOne();
          
           

           
            


        }
        /// <summary>
        /// 房屋综合后的处理
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="fclName"></param>
        /// <param name="gdpPath"></param>
        public void BuildingGenAfter(string fclName, string gdpPath)
        {

            try
            {
                #region
                IWorkspaceFactory wsf = new FileGDBWorkspaceFactoryClass();
                IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)wsf;//注意在java api中不能强转切记需要IWorkspaceFactoryLockControl ipWsFactoryLock = new IWorkspaceFactoryLockControlProxy(pwf);
                if (ipWsFactoryLock.SchemaLockingEnabled)
                {
                    ipWsFactoryLock.DisableSchemaLocking();
                }
                var fws = (IFeatureWorkspace)wsf.OpenFromFile(gdpPath, 0);
                //寻找最大面CC并赋值
                var fclOrigin = fws.OpenFeatureClass(fclName + "_Building");
                var fclExport = fws.OpenFeatureClass("Building");
                AddCustomField(fclExport, mField);
                // Tips("7 聚合面DLBM码赋值...");
                var aearIndex = fclOrigin.FindField(fclOrigin.AreaField.Name);
                var gbIndex0 = fclOrigin.FindField(mField);
                var gbIndex1 = fclExport.FindField(mField);
                var featureCursor = fclExport.Update(null, false);
                IFeature feature;
                while ((feature = featureCursor.NextFeature()) != null)
                {
                  
                    var dics = new Dictionary<string, double>();
                    var cursor = fclOrigin.Search(new SpatialFilterClass { Geometry = feature.Shape, SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects }, false);
                    IFeature fe;
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        try
                        {
                            var area = Convert.ToDouble(fe.Value[aearIndex]);
                            var gb = fe.Value[gbIndex0].ToString();
                            if (dics.ContainsKey(gb))
                                dics[gb] += area;
                            else
                                dics.Add(gb, area);
                        }
                        catch
                        {
                        }
                    }
                    var cc = dics.OrderByDescending(i => i.Value).First().Key;
                    try
                    {
                        //逆序排列
                        if(cc.Length>=4)
                           cc = cc.Substring(0, 4);
                        feature.Value[gbIndex1] = cc;
                        featureCursor.UpdateFeature(feature);
                        Marshal.ReleaseComObject(cursor);
                    }
                    catch
                    {
                    }
                }
                Marshal.ReleaseComObject(featureCursor);

                IDataset ds = fclOrigin as IDataset;
                if (ds.CanDelete())
                {
                    ds.Delete();
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("房屋综合错误:" + ex.Message);
            }
        }

        private void WaitGpEndCheck()
        {
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(200);
                    if( mGPFished)
                    {
                       break;    
                    }
                }
            }
            catch
            {
                return;
            }
        }
        private Queue<IGPProcess> mGPToolsToExecute = new Queue<IGPProcess>();
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

     


      


        
      

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using System.IO;
using ESRI.ArcGIS.Geoprocessor;
using System.Collections;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.SpatialAnalystTools;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Xml.Linq;
using System.Diagnostics;

namespace ShellTBDivided
{
    /// <summary>
    /// 图斑弯曲化简
    /// </summary>
    public class TBSimplifyClass
    {
        private IWorkspace mWorkSpace = null;      
        private string mFclName = "DLTB";
        private string mGDBPath = string.Empty;
        WaitOperation wo = null;
        private double mScale = 1;
        private  double mBendWidth=1;
        private double mSmoothDis=1;

        private int mLineLimit = 200;
        private IWorkspace mTempWorkSpace = null;
        private string mTempGDB = "";
        private string mTempFclName = "TempPolygon";
        private string mFclTBName = "TempTB";

        Geoprocessor mGP = null;
        
        public TBSimplifyClass(string gdbPath, double scale,double bendWidth,double smoothDis)
        {
            mGDBPath = gdbPath;
            IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
            mWorkSpace = wsFactory.OpenFromFile(gdbPath, 0);
            wo = new WaitOperation();
            mScale = scale;
            mBendWidth = bendWidth;
            mSmoothDis = smoothDis;
            mTempGDB = Application.StartupPath + "\\Temp.gdb";
            mTempWorkSpace = wsFactory.OpenFromFile(mTempGDB, 0);
            mGP = new Geoprocessor();
            mGP.OverwriteOutput = true;
        }
        public TBSimplifyClass(string gdbPath)
        {
            mGDBPath = gdbPath;
            IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
            mWorkSpace = wsFactory.OpenFromFile(gdbPath, 0);
            wo = new WaitOperation();
            mTempGDB = Application.StartupPath + "\\Temp.gdb";
            mTempWorkSpace = wsFactory.OpenFromFile(mTempGDB, 0);
            mGP = new Geoprocessor();
            mGP.OverwriteOutput = true;
        }
        public void ExcuteTBSimplifyOne()
        {
            try
            {
                IWorkspace2 ws2 = mWorkSpace as IWorkspace2;
                if (!ws2.get_NameExists(esriDatasetType.esriDTFeatureClass, mFclName))
                {
                    MessageBox.Show("请检查数据库中是否存在DLTB要素集");
                    return;
                }
                //生成只有房屋和水系的图层“HB”
                wo.SetText("正在提取房屋和水系……");
               // CreateHB();
                CreateHBShell();


                //面转线
                wo.SetText("正在进行面转线……");
               // PolygonToLine();//DLTBToLine
                PolygonToLineShell();
                wo.SetText("正在进行选取图斑线……");
                //选取不包含房屋和水系的DLTB线
                SelectDLTBLine();//DLTB_Selected
                //选取包含房屋和水系的DLTB线
                SelectDLTBLineHB();//DLTBHB_Selected
                wo.SetText("正在进行简化图斑线……");
                //简化不包含房屋和水系的DLTB线,光滑抽稀DLTB线
                double BlendPara = mBendWidth;//弯曲化简参数
                BlendPara = 0.001 * BlendPara * mScale;
                // double SmoothPara = mSmoothDis;//平滑参数
                double SmoothPara = 0.001 * mSmoothDis * mScale;//平滑参数
               
                SimplifyLineShell(BlendPara, SmoothPara);//DLTB_Selected_sim2
                //将HB线合并到DLTB_Selected_sim2中
                AppendHBDLTB();
                //
                string divFileName = Application.StartupPath + "\\SimplifyPoly.txt";


                wo.SetText("预处理创建临时图层");
                PolyPreTxt(divFileName);
                //new fcl
                IFeatureClass currentFcl = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclName);
                if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclName + "_New"))
                {
                    IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclName + "_New") as IDataset;
                    ds.Delete();
                }
                (mWorkSpace as IFeatureWorkspace).CreateFeatureClass(mFclName + "_New", currentFcl.Fields, null, null, currentFcl.FeatureType, currentFcl.ShapeFieldName, "");
                //temp big fcl
                if ((mTempWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclTBName))
                {
                    IDataset ds = (mTempWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclTBName) as IDataset;
                    ds.Delete();
                }
                (mTempWorkSpace as IFeatureWorkspace).CreateFeatureClass(mFclTBName, currentFcl.Fields, null, null, currentFcl.FeatureType, currentFcl.ShapeFieldName, "");
            
                for (int i = 1; i <= 6; i++)
                {
                    
                    if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclName + "_New"+i))
                    {
                        IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclName + "_New" + i) as IDataset;
                        ds.Delete();
                    }
                    (mWorkSpace as IFeatureWorkspace).CreateFeatureClass(mFclName + "_New" + i, currentFcl.Fields, null, null, currentFcl.FeatureType, currentFcl.ShapeFieldName, "");
                    //temp fcl
                    if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "TempPoly" + i))
                    {
                        IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("TempPoly" + i) as IDataset;
                        ds.Delete();
                    }
                    (mWorkSpace as IFeatureWorkspace).CreateFeatureClass("TempPoly" + i, currentFcl.Fields, null, null, currentFcl.FeatureType, currentFcl.ShapeFieldName, "");
                    //temp big
                    if ((mTempWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "TempPolyBig" + i))
                    {
                        IDataset ds = (mTempWorkSpace as IFeatureWorkspace).OpenFeatureClass("TempPolyBig" + i) as IDataset;
                        ds.Delete();
                    }
                    (mTempWorkSpace as IFeatureWorkspace).CreateFeatureClass("TempPolyBig" + i, currentFcl.Fields, null, null, currentFcl.FeatureType, currentFcl.ShapeFieldName, "");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void PolyPreTxt(string fileName)
        {
            IFeatureClass DLTB_Polyline = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB_Selected_sim2");//抽稀线
            //遍历所有的线
            IFeatureCursor featureCursor = DLTB_Polyline.Search(null, false);
            int count = DLTB_Polyline.FeatureCount(null);
            IFeature feature = null;
            // 添加数据结构
            Dictionary<int, List<int>> fesStruct = new Dictionary<int, List<int>>();//新的数据结构:DLTB的feid->多边形转线的所有ID
            #region
          
            //List<PolygonToLineStruct> OID_L_RFID = new List<PolygonToLineStruct>();
            int iNum = 0;
            while ((feature = featureCursor.NextFeature()) != null)
            {
                iNum++;
                if (iNum % 10000 == 0)
                {
                    Console.WriteLine("获取化简边线:" + iNum);
                }
                int lineID = feature.OID;
                int LeftOID = int.Parse(feature.get_Value(feature.Fields.FindField("LEFT_FID")).ToString());
                int RightOID = int.Parse(feature.get_Value(feature.Fields.FindField("RIGHT_FID")).ToString());
                if (LeftOID != -1)
                {
                    if (fesStruct.ContainsKey(LeftOID))
                    {
                        List<int> ids = fesStruct[LeftOID];
                        if (!ids.Contains(feature.OID))
                        {
                            ids.Add(feature.OID);
                            fesStruct[LeftOID] = ids;
                        }
                    }
                    else
                    {
                        List<int> ids = new List<int>();
                        ids.Add(feature.OID);
                        fesStruct.Add(LeftOID, ids);
                    }

                }
                if (RightOID != -1)
                {
                    if (fesStruct.ContainsKey(RightOID))
                    {
                        List<int> ids = fesStruct[RightOID];
                        if (!ids.Contains(feature.OID))
                        {
                            ids.Add(feature.OID);
                            fesStruct[RightOID] = ids;
                        }
                    }
                    else
                    {
                        List<int> ids = new List<int>();
                        ids.Add(feature.OID);
                        fesStruct.Add(RightOID, ids);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
            }
            GC.Collect();
            #endregion
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);


            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }

            using (System.IO.FileStream fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate))
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fileStream))
                {

                    foreach (var fid in fesStruct)
                    {
                        string msg = string.Empty;
                        msg += fid.Key + ":";
                        foreach (var edge in fid.Value)
                        {
                            msg += edge + ",";
                        }
                        if (fid.Value.Count > 0)
                        {
                            msg = msg.Substring(0, msg.Length - 1);
                        }
                        msg += ";";
                        sw.WriteLine(msg);
                    }

                }
            }

        }
        //分步计算
        public void ExcuteTBSimPolygon(string fileName,string fclNew,string temp,string tempBig,int start,int poi)
        {
            try
            {
                wo.SetText("正在构面赋值DLBM值……");
                ConstructPolygon(fileName, fclNew, temp, tempBig, start,poi);
               

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
       
        public void ExcuteTBSimplifyTwo()
        {
            try
            {
                wo.SetText("正在构面赋值DLBM值……");
                //然后根据DLTB与DLTB_Selected_sim2邻接拓扑关系确定
                PolylineToDLTB();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public void ExcuteTBSimplifyThree()
        {
            try
            {
                // 删除重复面
                wo.SetText("正在处理追加图斑面……");
                string fclSmall=string.Empty;
                string fclBig=string.Empty;
                for (int i = 1; i <= 6; i++)
                {
                    IFeatureClass fclnew = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclName + "_New" + i);
                    if (fclnew.FeatureCount(null) > 0)
                    {
                        fclSmall += mGDBPath + "\\" + mFclName + "_New" + i + ";";
                    }
                    Marshal.ReleaseComObject(fclnew);
                    IFeatureClass fclbig = (mTempWorkSpace as IFeatureWorkspace).OpenFeatureClass("TempPolyBig" + i);
                    if (fclbig.FeatureCount(null) > 0)
                    {
                        fclBig += mTempGDB + "\\TempPolyBig" + i + ";";
                    }
                    Marshal.ReleaseComObject(fclbig);
                }
                fclSmall=fclSmall.Substring(0,fclSmall.Length-1);            
                AppendShell(fclSmall, mGDBPath + "\\" + mFclName + "_New");
                if (fclBig != string.Empty)
                {
                    fclBig = fclBig.Substring(0, fclBig.Length - 1);
                    AppendShell(fclBig, mGDBPath + "\\" + mFclName + "_New");
                     
                }
                //临时图层
                DeleteTempLyr();
                // 删除重复面
                wo.SetText("正在删除重复面……");
                DeleteOverlapShell();
                //整合
                wo.SetText("正在整合……");
                //IntegrateDLTB();
                IntegrateDLTBShell();
                wo.SetText("正在碎面处理……");
                ElimateTBShell();
                
                //删除相关数据:DLTB_Selected_sim2,DLTBHB_Selected,DLTBToLine
                wo.SetText("正在删除过程数据……");
                DeleteSim_Smo();
                wo.SetText("正在重命名……"); 
                //重新命名
                if((mWorkSpace  as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass,"DLTB"))
                {
                    var ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB");
                    (ds as IDataset).Delete();
                }
                //说明碎面处理失败
                if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "DLTBElimate"))
                {
                    if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "DLTB_New"))
                    {
                        var ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB_New");
                        (ds as IDataset).Delete();
                    }
                    var ds0 = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTBElimate");
                    (ds0 as IDataset).Rename("DLTB");
                }
                //说明碎面处理成功
                if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "DLTB_New"))
                {
                    var ds0 = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB_New");
                    (ds0 as IDataset).Rename("DLTB");
                }
                List<string> fclsList = new List<string>();
                fclsList.Add("DLTB_Selected_sim2");
                fclsList.Add("DLTBHB_Selected");
                fclsList.Add("DLTBToLine");
                fclsList.Add("HB");
                foreach (var kv in fclsList)
                {
                    if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, kv))
                    {
                        IDataset dataset = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(kv) as IDataset;
                        if (dataset != null)
                        {
                            dataset.Delete();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void DeleteTempLyr()
        {
            for (int i = 1; i <= 6; i++)
            {

                if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclName + "_New" + i))
                {
                    IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclName + "_New" + i) as IDataset;
                    ds.Delete();
                }
                //temp fcl
                if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "TempPoly" + i))
                {
                    IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("TempPoly" + i) as IDataset;
                    ds.Delete();
                }
                //temp big
                if ((mTempWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "TempPolyBig" + i))
                {
                    IDataset ds = (mTempWorkSpace as IFeatureWorkspace).OpenFeatureClass("TempPolyBig" + i) as IDataset;
                    ds.Delete();
                }
            }
        }

        /// <summary>
        /// 外部调用
        /// </summary>
        public void ExcuteTBSimplify()
        {
            try
            {
                IWorkspace2 ws2 = mWorkSpace as IWorkspace2;
                if (!ws2.get_NameExists(esriDatasetType.esriDTFeatureClass, mFclName))
                {
                    MessageBox.Show("请检查数据库中是否存在DLTB要素集");
                    return;
                }
                //生成只有房屋和水系的图层“HB”
                wo.SetText("正在提取房屋和水系……");
                CreateHB();
                //面转线
                wo.SetText("正在进行面转线……");
                PolygonToLine();//DLTBToLine
                wo.SetText("正在进行选取图斑线……");
                //选取不包含房屋和水系的DLTB线
                SelectDLTBLine();//DLTB_Selected
                //选取包含房屋和水系的DLTB线
                SelectDLTBLineHB();//DLTBHB_Selected
                wo.SetText("正在进行简化图斑线……");
                //简化不包含房屋和水系的DLTB线,光滑抽稀DLTB线
                double BlendPara = mBendWidth;//弯曲化简参数
                BlendPara = 0.001 * BlendPara * mScale;
               // double SmoothPara = mSmoothDis;//平滑参数
                double SmoothPara = 0.001 * mSmoothDis * mScale;//平滑参数
                SimplifyLine(BlendPara, SmoothPara);//DLTB_Selected_sim2
                //将HB线合并到DLTB_Selected_sim2中
                AppendHBDLTB();
                wo.SetText("正在构面赋值DLBM值……");
                //然后根据DLTB与DLTB_Selected_sim2邻接拓扑关系确定
                //PolylineToDLTBwjz();
                PolylineToDLTB();
               // 删除重复面
                wo.SetText("正在删除重复面……");
                DeleteOverlap();
                //整合
                IntegrateDLTB();




                //删除相关数据:DLTB_Selected_sim2,DLTBHB_Selected,DLTBToLine
                wo.SetText("正在删除过程数据……");
                DeleteSim_Smo();
                //重新命名
                var ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB");
                (ds as IDataset).Delete();
                var ds0 = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB_New");
                (ds0 as IDataset).Rename("DLTB");
                List<string> fclsList = new List<string>();
                fclsList.Add("DLTB_Selected_sim2");
                fclsList.Add("DLTBHB_Selected");
                fclsList.Add("DLTBToLine");
                fclsList.Add("HB");
                foreach (var kv in fclsList)
                {
                    IDataset dataset = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(kv) as IDataset;
                    if (dataset != null)
                    {
                        dataset.Delete();
                    }
                     
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


     

        private void SaveHBToGDB(IFeatureLayer featureLayer, IWorkspace ws, string name)
        {
            try
            {
                IFeatureClass inputFeatureClass = featureLayer.FeatureClass;
                IDataset inputDataset = (IDataset)inputFeatureClass;
                IDatasetName inputDatasetName = (IDatasetName)inputDataset.FullName;
                // Get the layer's selection set. 
                IFeatureSelection featureSelection = (IFeatureSelection)featureLayer;
                ISelectionSet selectionSet = featureSelection.SelectionSet;
                //根据ws获取其wsName
                IDataset ds = (IDataset)ws;
                IWorkspaceName wsName = (IWorkspaceName)ds.FullName;
                //设置一个要素集的数据库路径（WorkspaceName）和要素名称
                IFeatureClassName featClsName = new FeatureClassNameClass();
                IDatasetName dsName = (IDatasetName)featClsName;
                dsName.WorkspaceName = wsName;
                dsName.Name = name;
                //// Use the IFieldChecker interface to make sure all of the field names are valid for a shapefile. 
                IFieldChecker fieldChecker = new FieldCheckerClass();
                IFields shapefileFields = null;
                IEnumFieldError enumFieldError = null;
                fieldChecker.InputWorkspace = inputDataset.Workspace;
                fieldChecker.ValidateWorkspace = ws;
                fieldChecker.Validate(inputFeatureClass.Fields, out enumFieldError, out shapefileFields);
                // At this point, reporting/inspecting invalid fields would be useful, but for this example it's omitted.
                // We also need to retrieve the GeometryDef from the input feature class. 
                int shapeFieldPosition = inputFeatureClass.FindField(inputFeatureClass.ShapeFieldName);
                IFields inputFields = inputFeatureClass.Fields;
                IField shapeField = inputFields.get_Field(shapeFieldPosition);
                IGeometryDef geometryDef = shapeField.GeometryDef;
                IGeometryDef pGeometryDef = new GeometryDef();
                IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
                pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
                pGeometryDefEdit.SpatialReference_2 = (featureLayer as IGeoDataset).SpatialReference;
                // Now we can create a feature data converter. 
                IFeatureDataConverter2 featureDataConverter2 = new FeatureDataConverterClass();
                IEnumInvalidObject enumInvalidObject = featureDataConverter2.ConvertFeatureClass(inputDatasetName, null, selectionSet, null, featClsName, pGeometryDef, shapefileFields, "", 1000, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 创建只包含房屋和水的HB图层
        /// </summary>
        private void CreateHB(string sql = "DLBM LIKE'05%'OR DLBM LIKE'06%'OR DLBM LIKE'07%'OR DLBM LIKE'08%' OR DLBM LIKE'11%'")
        {

            Geoprocessor gp = mGP;
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", mWorkSpace.PathName);
            try
            {
                //新建图层前先删去已有HB层
                DeleteFeatureClassInWorkspace(mWorkSpace, "HB");
                MakeFeatureLayer makefeaturelayer = new MakeFeatureLayer();
                makefeaturelayer.in_features = (mFclName);
                makefeaturelayer.out_layer = "in_Lyr1";
                RunTool(gp, makefeaturelayer, null);
                ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute pSelectHB = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
                pSelectHB.in_layer_or_view = "in_Lyr1";
                pSelectHB.where_clause = sql;
                pSelectHB.selection_type = "NEW_SELECTION";
                IGeoProcessorResult pResult1 = (IGeoProcessorResult)gp.Execute(pSelectHB, null);
                IGPUtilities gpUtils = new GPUtilitiesClass();
                if (pResult1.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                    IFeatureLayer myFeatureLayer = gpUtils.DecodeLayer(pResult1.GetOutput(0)) as IFeatureLayer;
                    SaveHBToGDB(myFeatureLayer, mWorkSpace, "HB");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }

                }
                MessageBox.Show(ms);
            }
        }

        private void CreateHBShell(string sql = "DLBM LIKE'05%'OR DLBM LIKE'06%'OR DLBM LIKE'07%'OR DLBM LIKE'08%' OR DLBM LIKE'11%'")
        {


            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("ExportFeature");
                cmd.Add(new XElement("InFeature", mWorkSpace.PathName + "\\" + mFclName));//
                cmd.Add(new XElement("SQL", sql));//
                cmd.Add(new XElement("TargetGDB", mWorkSpace.PathName));
                cmd.Add(new XElement("FclName", "HB"));
              
                root.Add(cmd);
                doc.Add(root);
                string tempxml = Application.StartupPath + "\\autoExportHB.xml";
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
        private void PolygonToLineShell()
        {

            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("PolygonToLine");
                cmd.Add(new XElement("InFeature", mWorkSpace.PathName + "\\" + mFclName));//
                cmd.Add(new XElement("OutFeature", mWorkSpace.PathName + "\\DLTBToLine"));//
                root.Add(cmd);
                doc.Add(root);
                string tempxml = Application.StartupPath + "\\autoTBToLine.xml";
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


        private void AppendShell(string infeature, string tareget)
        {
            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("TBAppend");
                cmd.Add(new XElement("InFeature", infeature));//
                cmd.Add(new XElement("OutFeature", tareget));//
                root.Add(cmd);
                doc.Add(root);
                string tempxml = Application.StartupPath + "\\autoTBAppend.xml";
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

        private void DeleteOverlapShell()
        {
            string name = "DLTB_New";

            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("TBOverlapDel");
                cmd.Add(new XElement("InFeature", mWorkSpace.PathName + "\\" + name));//
                root.Add(cmd);
                doc.Add(root);
                string tempxml = Application.StartupPath + "\\autoTBOverlapDel.xml";
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

        private void IntegrateDLTBShell()
        {
            
            
                try
                {
                    XDocument doc = new XDocument();
                    XElement root = new XElement("Arg");
                    XElement cmd = new XElement("Command");

                    cmd = new XElement("TBInteg");
                    cmd.Add(new XElement("InFeature", mWorkSpace.PathName + "\\DLTB_New"));//
                    root.Add(cmd);
                    doc.Add(root);
                    string tempxml = Application.StartupPath + "\\autoTBInteg.xml";
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

        private void ElimateTBShell()
        {
            string name = "DLTB_New";

            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("ElimateTB");
                cmd.Add(new XElement("InFeature",  name));//
                cmd.Add(new XElement("GDB", mWorkSpace.PathName));//
                cmd.Add(new XElement("Scale", mScale));//
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

        private void SimplifyLineShell(double BendPara, double SmoothPara)
        {

            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("TBLineSimplifyLine");
                cmd.Add(new XElement("GDB", mWorkSpace.PathName ));//
                cmd.Add(new XElement("BendWidth", BendPara));//
                cmd.Add(new XElement("SmoothWidth", SmoothPara));//
                cmd.Add(new XElement("JopName", "图斑边线弯曲化简"));//
                root.Add(cmd);
                doc.Add(root);
                string tempxml = Application.StartupPath + "\\autoTBSMLine.xml";
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
        /// <summary>
        /// 综合好的DLTB转线
        /// </summary>
        private void PolygonToLine()
        {
            //新建DLTBToLine层之前先删去已有图层
            DeleteFeatureClassInWorkspace(mWorkSpace, "DLTBToLine");
            Geoprocessor gp = mGP;
            try
            {
                string workspacePath = mWorkSpace.PathName.ToString();
                gp.OverwriteOutput = true;
                string infeatures = workspacePath + @"\"+mFclName;
                string outfeatures = workspacePath + @"\DLTBToLine";
                ESRI.ArcGIS.DataManagementTools.PolygonToLine polygontoLine = new ESRI.ArcGIS.DataManagementTools.PolygonToLine(infeatures, outfeatures);
                polygontoLine.neighbor_option = "IDENTIFY_NEIGHBORS";
                gp.Execute(polygontoLine, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }

                }
                MessageBox.Show(ms);
            }
        }

        /// <summary>
        /// 选择没有房屋和水边界的DLTB线
        /// </summary>
        private void SelectDLTBLine()
        {
            //新建图层前先删去已有DLTB_Selected层
            DeleteFeatureClassInWorkspace(mWorkSpace, "DLTB_Selected");
            Geoprocessor gp = mGP;
            try
            {
                IWorkspace pws = mWorkSpace;
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", pws.PathName);
                IFeatureClass selectfcl = (pws as IFeatureWorkspace).OpenFeatureClass("HB");
                //wo.SetText("正在创建图层");
                MakeFeatureLayer makefeaturelayer = new MakeFeatureLayer();
                makefeaturelayer.in_features = System.IO.Path.Combine("DLTBToLine");
                makefeaturelayer.out_layer = "in_Lyr1";
                RunTool(gp, makefeaturelayer, null);
                //wo.SetText("正在筛选不共线的要素");
                ESRI.ArcGIS.DataManagementTools.SelectLayerByLocation gpSL = new ESRI.ArcGIS.DataManagementTools.SelectLayerByLocation();
                gpSL.in_layer = "in_Lyr1";
                gpSL.select_features = selectfcl;
                // gpSL.selection_type = "NEW_SELECTION";
                gpSL.selection_type = "NEW_SELECTION";
                gpSL.overlap_type = "SHARE_A_LINE_SEGMENT_WITH";
                IGeoProcessorResult pResult1 = (IGeoProcessorResult)gp.Execute(gpSL, null);
                IGPUtilities gpUtils = new GPUtilitiesClass();
                if (pResult1.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                    //wo.SetText("正在筛选不共线的要素");
                    IFeatureLayer myFeatureLayer = gpUtils.DecodeLayer(pResult1.GetOutput(0)) as IFeatureLayer;
                    gpSL = new ESRI.ArcGIS.DataManagementTools.SelectLayerByLocation();
                    gpSL.in_layer = myFeatureLayer;
                    //gpSL.select_features = selectfcl;
                    gpSL.selection_type = "SWITCH_SELECTION";
                    // gpSL.overlap_type = "INTERSECT";
                    pResult1 = (IGeoProcessorResult)gp.Execute(gpSL, null);
                    if (pResult1.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                    {
                        IFeatureLayer myFeatureLayer1 = gpUtils.DecodeLayer(pResult1.GetOutput(0)) as IFeatureLayer;
                        SaveSelectionFeatureToGDB(myFeatureLayer1, pws, "DLTB_Selected");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }

                }
                return;
            }
        }
        /// <summary>
        /// 选择包含房屋和水边界的DLTB线
        /// </summary>
        private void SelectDLTBLineHB()
        {
            //新建图层前先删去已有DLTB_Selected层
            DeleteFeatureClassInWorkspace(mWorkSpace, "DLTBHB_Selected");
            Geoprocessor gp = mGP;
            try
            {
                IWorkspace pws = mWorkSpace;
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", pws.PathName);
                IFeatureClass selectfcl = (pws as IFeatureWorkspace).OpenFeatureClass("HB");
                //wo.SetText("正在创建图层");
                MakeFeatureLayer makefeaturelayer = new MakeFeatureLayer(); 
                makefeaturelayer.in_features = System.IO.Path.Combine("DLTBToLine");
                makefeaturelayer.out_layer = "in_Lyr1";
                RunTool(gp, makefeaturelayer, null);
                //wo.SetText("正在筛选共线的要素");
                ESRI.ArcGIS.DataManagementTools.SelectLayerByLocation gpSL = new ESRI.ArcGIS.DataManagementTools.SelectLayerByLocation();
                gpSL.in_layer = "in_Lyr1";
                gpSL.select_features = selectfcl;
                gpSL.selection_type = "NEW_SELECTION";
                gpSL.overlap_type = "SHARE_A_LINE_SEGMENT_WITH";
                IGeoProcessorResult pResult1 = (IGeoProcessorResult)gp.Execute(gpSL, null);
                IGPUtilities gpUtils = new GPUtilitiesClass();
                if (pResult1.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                    //wo.SetText("正在筛选不共线的要素");
                    IFeatureLayer myFeatureLayer = gpUtils.DecodeLayer(pResult1.GetOutput(0)) as IFeatureLayer;
                    SaveSelectionFeatureToGDB(myFeatureLayer, pws, "DLTBHB_Selected");
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }

                }
                
                return;
            }
        }

        private void AppendHBDLTB()
        {
            Geoprocessor gp = mGP; 
            try
            {
                //将DLTBHB_Selected字段，与DLTB_Selected_sim2一样才能匹配，添加
                IFeatureClass fclDLTBLine = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB_Selected_sim2");
                IFeatureClass fclHBLine = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTBHB_Selected");
                #region
                Dictionary<string, IField> fieldsTarget = new Dictionary<string, IField>();
                for (int i = 0; i < fclHBLine.Fields.FieldCount; i++)
                {
                    IField pfield = fclHBLine.Fields.get_Field(i);
                    if (pfield.Type != esriFieldType.esriFieldTypeOID && pfield.Type != esriFieldType.esriFieldTypeGeometry)
                    {
                        fieldsTarget.Add(pfield.Name, pfield);
                    }
                }

                //
                for (int i = 0; i < fclDLTBLine.Fields.FieldCount; i++)
                {
                    IField pfield = fclDLTBLine.Fields.get_Field(i);

                    if (pfield.Type != esriFieldType.esriFieldTypeOID && pfield.Type != esriFieldType.esriFieldTypeGeometry)
                    {
                        string name = pfield.Name;
                        if (!fieldsTarget.ContainsKey(name))
                        {
                            IFields pFields = fclHBLine.Fields;
                            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
                            IField pField = new FieldClass();
                            IFieldEdit pFieldEdit = pField as IFieldEdit;
                            pFieldEdit.Name_2 = name;
                            pFieldEdit.AliasName_2 = name;
                            pFieldEdit.Length_2 = pfield.Length;
                            pFieldEdit.Type_2 = pfield.Type;
                            ITable pTable = fclHBLine as ITable;
                            pTable.AddField(pField);
                        }

                    }
                }
                #endregion

                IWorkspace pws = mWorkSpace;
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", pws.PathName);
                ESRI.ArcGIS.DataManagementTools.Append ap = new ESRI.ArcGIS.DataManagementTools.Append("DLTBHB_Selected", "DLTB_Selected_sim2");

                IGeoProcessorResult pResult1 = (IGeoProcessorResult)gp.Execute(ap, null);
                IGPUtilities gpUtils = new GPUtilitiesClass();
                if (pResult1.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                   
                }
            }
            catch
            {
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }

                }
               
                return;
            }

        }

        private void IntegrateDLTB()
        {
            Geoprocessor gp = mGP;
            try
            {
                
                IWorkspace pws = mWorkSpace;
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", pws.PathName);
                ESRI.ArcGIS.DataManagementTools.Integrate integrate = new ESRI.ArcGIS.DataManagementTools.Integrate();
                integrate.in_features = pws.PathName + "\\DLTB_New";
                integrate.cluster_tolerance = 0.1;

                IGeoProcessorResult pResult1 = (IGeoProcessorResult)gp.Execute(integrate, null);
                IGPUtilities gpUtils = new GPUtilitiesClass();
                if (pResult1.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {

                }
            }
            catch
            {
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }

                }

                return;
            }

        }
        /// <summary>
        /// 对不包含房屋和水边界的DLTB线进行光滑抽稀
        /// <param name="BendPara">弯曲化简参数</param>
        /// <param name="SmoothPara">平滑参数</param>
        /// </summary>
        private void SimplifyLine(double BendPara, double SmoothPara)
        {
            Geoprocessor gp = mGP;
            try
            {
                //创建之前先删图层 DLTB_Selected_sim DLTB_Selected_smo DLTB_Selected_sim2
                DeleteFeatureClassInWorkspace(mWorkSpace, "DLTB_Selected_sim");
                DeleteFeatureClassInWorkspace(mWorkSpace, "DLTB_Selected_smo");
                DeleteFeatureClassInWorkspace(mWorkSpace, "DLTB_Selected_sim2");
                //
                IFeatureClass simplfy_inputFcl = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB_Selected");
                gp = GApplication.Application.GPTool;
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", mWorkSpace.PathName);
                string workspacePath = mWorkSpace.PathName;
                string infeatures = workspacePath + @"\DLTB_Selected";
                string outfeatures = workspacePath + @"\DLTB_Selected_sim";
                string infeatures2 = workspacePath + @"\DLTB_Selected_sim";
                string outfeatures2 = workspacePath + @"\DLTB_Selected_smo";
                string infeatures3 = workspacePath + @"\DLTB_Selected_smo";
                string outfeatures3 = workspacePath + @"\DLTB_Selected_sim2";
                //wo.SetText("正在简化线");
                ESRI.ArcGIS.CartographyTools.SimplifyLine pSimplifyline = new ESRI.ArcGIS.CartographyTools.SimplifyLine(infeatures, outfeatures, "BEND_SIMPLIFY", BendPara);
                pSimplifyline.error_checking_option = "NO_CHECK";
                IGeoProcessorResult pGeoResult = null;
                pGeoResult = (IGeoProcessorResult)gp.Execute(pSimplifyline, null);
                if (pGeoResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                    //wo.SetText("正在平滑线");
                    ESRI.ArcGIS.CartographyTools.SmoothLine psmoothline = new ESRI.ArcGIS.CartographyTools.SmoothLine(infeatures2, outfeatures2, "PAEK", SmoothPara);
                    pGeoResult = (IGeoProcessorResult)gp.Execute(psmoothline, null);
                    if (pGeoResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                    {
                        //wo.SetText("正在进一步简化线");
                        ESRI.ArcGIS.CartographyTools.SimplifyLine pSimplifyline2 = new ESRI.ArcGIS.CartographyTools.SimplifyLine(infeatures3, outfeatures3, "POINT_REMOVE", 1);
                        pSimplifyline2.error_checking_option = "NO_CHECK";
                        pGeoResult = (IGeoProcessorResult)gp.Execute(pSimplifyline2, null);
                        DeleteSim_Smo();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }
                }
                MessageBox.Show(ms);
            }
        }
        /// <summary>
        /// 删除光滑抽稀过程中产生的中间图层
        /// </summary>
        private void DeleteSim_Smo()
        {
            try
            {
                IEnumDataset pEnumDataset = mWorkSpace.get_Datasets(esriDatasetType.esriDTFeatureClass) as IEnumDataset;
                pEnumDataset.Reset();
                IDataset pDataset = pEnumDataset.Next();
                while (pDataset != null)
                {
                    if (pDataset.Name == "DLTB_Selected" || pDataset.Name == "DLTB_Selected_sim" || pDataset.Name == "DLTB_Selected_sim_Pnt" || pDataset.Name == "DLTB_Selected_sim2_Pnt" || pDataset.Name == "DLTB_Selected_smo")
                    {
                        if (pDataset.CanDelete())
                            pDataset.Delete();
                    }
                   
                    pDataset = pEnumDataset.Next();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        /// <summary>
        /// 将DLTB线和HB层合起来转新的DLTB面
        /// </summary>
        private void FeatureToPolygon()
        {
            //创建要素前先删已有图层 DLTBtoNEW
            IWorkspace2 ws2 = mWorkSpace as IWorkspace2;
            DeleteFeatureClassInWorkspace(mWorkSpace, "DLTB_NEW");
            Geoprocessor gp = mGP;
            try
            {
                string infeatures = mWorkSpace.PathName.ToString() + @"\DLTB_Selected_sim2" + ";" + mWorkSpace.PathName.ToString() + @"\HB";
                //string labelfeature = mWorkSpace.PathName.ToString() + @"\DLTBtoPoint";
                string outfeatures = mWorkSpace.PathName.ToString() + @"\DLTB_NEW";
                ESRI.ArcGIS.DataManagementTools.FeatureToPolygon Featuretopolygon = new ESRI.ArcGIS.DataManagementTools.FeatureToPolygon();
                Featuretopolygon.in_features = infeatures;
                Featuretopolygon.out_feature_class = outfeatures;
                //Featuretopolygon.attributes = "ATTRIBUTES";
                //Featuretopolygon.label_features = labelfeature;
                gp.Execute(Featuretopolygon, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }
                }
                MessageBox.Show(ms);
            }
        }


        //相关函数
        #region
        //创建内存要素类
        private IFeatureClass CreateFclInmemeory(string DataSetName, string AliaseName, ISpatialReference SpatialRef, esriGeometryType GeometryType, IFields PropertyFields)
        {
            IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactoryClass();
            ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create("", "MyWorkspace", null, 0);
            ESRI.ArcGIS.esriSystem.IName name = (ESRI.ArcGIS.esriSystem.IName)workspaceName;
            ESRI.ArcGIS.Geodatabase.IWorkspace inmemWor = (IWorkspace)name.Open();
            IField oField = new FieldClass();
            IFields oFields = new FieldsClass();
            IFieldsEdit oFieldsEdit = null;
            IFieldEdit oFieldEdit = null;
            IFeatureClass oFeatureClass = null;

            try
            {
                oFieldsEdit = oFields as IFieldsEdit;
                oFieldEdit = oField as IFieldEdit;
                for (int i = 0; i < PropertyFields.FieldCount; i++)
                {
                    if (PropertyFields.get_Field(i).AliasName == "RuleID" || PropertyFields.get_Field(i).AliasName == "Override")
                    {
                        continue;
                    }
                    oFieldsEdit.AddField(PropertyFields.get_Field(i));
                }
                IGeometryDef geometryDef = new GeometryDefClass();
                IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                geometryDefEdit.AvgNumPoints_2 = 5;
                geometryDefEdit.GeometryType_2 = GeometryType;
                geometryDefEdit.GridCount_2 = 1;
                geometryDefEdit.HasM_2 = false;
                geometryDefEdit.HasZ_2 = false;
                geometryDefEdit.SpatialReference_2 = SpatialRef;
                oFieldEdit.Name_2 = "SHAPE";
                oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                oFieldEdit.GeometryDef_2 = geometryDef;
                oFieldEdit.IsNullable_2 = true;
                oFieldEdit.Required_2 = true;
                oFieldsEdit.AddField(oField);
                oFeatureClass = (inmemWor as IFeatureWorkspace).CreateFeatureClass(DataSetName, oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
                (oFeatureClass as IDataset).BrowseName = DataSetName;

            }
            catch
            {
            }
            finally
            {
                try
                {

                }
                catch { }

                GC.Collect();
            }
            return oFeatureClass;
        }
        private List<int> CreatePolygonFromGeosRing(List<int> ids, IInvalidArea pinvalidArea, IFeatureConstruction pfeBuild, IFeatureClass pPolygonFcl, IFeatureClass fclline)
        {
            ITable pTable = pPolygonFcl as ITable;
            pTable.DeleteSearchedRows(null);

            try
            {
                object _missing = Type.Missing;
                IGeometryCollection geometryBag = new GeometryBagClass();
                for (int i = 0; i < ids.Count; i++)
                {
                    IGeometry geo = fclline.GetFeature(ids[i]).Shape;
                    geometryBag.AddGeometry(geo, ref _missing, ref _missing);
                }

                IGeoDataset pGeodataset = pPolygonFcl as IGeoDataset;
                IEnvelope pEnvelope = pGeodataset.Extent;
                IWorkspace pws = (pPolygonFcl as IDataset).Workspace;

                ISelectionSet pSet;
                pfeBuild.AutoCompleteFromGeometries(pPolygonFcl, pEnvelope, geometryBag as IEnumGeometry, pinvalidArea, 0.000001, pws, out pSet);
                //IEnumIDs pEnumIDs = pSet.IDs;
                //pEnumIDs.Reset();
                //int id = pEnumIDs.Next();
                Dictionary<int, IGeometry> dics = new Dictionary<int, IGeometry>();
                Dictionary<int, string> fesCC = new Dictionary<int, string>();//所有要素
                string cc = "";
               
                #region
                IFeature fe = null;
                IFeatureCursor feaCursor = pPolygonFcl.Search(null, false);
                while ((fe = feaCursor.NextFeature()) != null)
                {

                    
                    int id = fe.OID;
                    
                    try
                    {
                        IGeometryCollection pGeoCol = fe.Shape as IGeometryCollection;
                        double area = (pGeoCol as IArea).Area;

                        fesCC.Add(id, cc);
                        //外环
                        if (pGeoCol.GeometryCount > 1)
                        {
                            for (int i = 0; i < pGeoCol.GeometryCount; i++)
                            {
                                IRing pRing = pGeoCol.get_Geometry(i) as IRing;
                                if (pRing.IsExterior)
                                {

                                    IGeometryCollection pgeoC = new PolygonClass();
                                    pgeoC.AddGeometry(pRing);

                                    (pgeoC as ITopologicalOperator).Simplify();

                                    dics.Add(id, pgeoC as IGeometry);

                                    break;
                                }

                            }
                        }
                    }
                    catch
                    {
                    }
                    Marshal.ReleaseComObject(fe);
                }


                #endregion
               
                
                List<int> keepIDs = new List<int>();
                
                ISpatialFilter sf = new SpatialFilterClass();
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                foreach (KeyValuePair<int, IGeometry> ringdic in dics)
                {
                    sf.Geometry = ringdic.Value;

                    IFeatureCursor cursor = pPolygonFcl.Search(sf, false);
                    
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        fesCC[fe.OID] = "hole";
                    }
                    keepIDs.Add(ringdic.Key);
                    Marshal.ReleaseComObject(cursor);
                }
                var sd = fesCC.Where(t => t.Value == "");//保留的
                foreach (var kv in sd)
                {
                    if (!keepIDs.Contains(kv.Key))
                    {
                        keepIDs.Add(kv.Key);
                    }
                }
                if (false)
                {
                    //筛选要素：去掉内环要素，相内切要素
                    foreach (KeyValuePair<int, string> kv in fesCC)
                    {
                        int feid = kv.Key;
                        if (dics.ContainsKey(feid))//包含
                        {
                            keepIDs.Add(feid);
                        }
                        else
                        {
                            #region//判断是否切,内环
                            //bool flag = true;
                            //IFeature fe = pPolygonFcl.GetFeature(feid);
                            //IRelationalOperator rp = fe.ShapeCopy as IRelationalOperator;
                            //foreach (KeyValuePair<int, IGeometry> ringdic in dics)
                            //{
                            //    if (rp.Within(ringdic.Value))
                            //    {

                            //        flag = false;
                            //        break;
                            //    }

                            //}
                            //if (flag)
                            //{
                            //    //内切
                            //    IGeometry pgeo = rp as IGeometry;
                            //    bool qieFlag = true;
                            //    foreach (KeyValuePair<int, IGeometry> ringdic in dics)
                            //    {
                            //        IGeometry pPly = (pgeo as ITopologicalOperator).Intersect(ringdic.Value, esriGeometryDimension.esriGeometry1Dimension);
                            //        if (pPly != null)
                            //        {
                            //            if (!pPly.IsEmpty)
                            //            {
                            //                qieFlag = false;
                            //                break;
                            //            }
                            //        }
                            //        Marshal.ReleaseComObject(pPly);
                            //    }
                            //    if (qieFlag)//不是内切
                            //    {
                            //        keepIDs.Add(feid);
                            //    }

                            //}
                            //Marshal.ReleaseComObject(fe);
                            //Marshal.ReleaseComObject(rp);
                            #endregion
                        }
                    }
                }
                Marshal.ReleaseComObject(geometryBag);
                Marshal.ReleaseComObject(pSet);
               // Marshal.ReleaseComObject(pEnumIDs);
                return keepIDs;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        private void CreatePolygonFromGeosRingShell(int OID,string gb)
        {
            

            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("ConstructPolygon");
                cmd.Add(new XElement("SourceGDB", mGDBPath));//
                cmd.Add(new XElement("Line", "DLTB_Selected_sim2"));//
                cmd.Add(new XElement("TempGDB", mTempGDB));
                cmd.Add(new XElement("TempFcl", mTempFclName));
                cmd.Add(new XElement("OID", OID));
                cmd.Add(new XElement("DLBM", gb));
                root.Add(cmd);
                doc.Add(root);
                string tempxml =Application.StartupPath + "\\autoConstructPoly.xml";
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

        private void CreatePolygonFromGeosRingShell(int OID, string gb,string fclBig,int poi)
        {


            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("ConstructPolygon");
                cmd.Add(new XElement("SourceGDB", mGDBPath));//
                cmd.Add(new XElement("Line", "DLTB_Selected_sim2"));//
                cmd.Add(new XElement("TempGDB", mTempGDB));
                cmd.Add(new XElement("TempFcl", fclBig));
                cmd.Add(new XElement("OID", OID));
                cmd.Add(new XElement("DLBM", gb));
                root.Add(cmd);
                doc.Add(root);
                string tempxml = Application.StartupPath + "\\autoConstructPoly" + poi + ".xml";
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

        private void ConstructPolygon(string fileName, string fclNewName,string fclTempPly, string fclTempNameBig,int start,int poi)
        {
            IFeatureClass fclTB = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclName);
            IFeatureClass pEditFc = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(fclNewName);
            IFeatureClass fclTopLine = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB_Selected_sim2");

            IFeatureClass pInmemberFcl = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(fclTempPly);

            mTempFclName = fclTempNameBig;

            IFeatureCursor peditCursor = pEditFc.Insert(true);
            int indexCCold = fclTB.FindField("DLBM");
            int indexCC = pEditFc.FindField("DLBM");
            Dictionary<int, List<int>> fesStruct = new Dictionary<int, List<int>>();//新的数据结构:DLTB的feid->多边形转线的所有ID
            #region
            string divItems = string.Empty;
            using (System.IO.FileStream fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fileStream))
                {
                    string line;

                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        divItems += line;
                    }
                }
            }
            string[] divFeatures = divItems.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var fe in divFeatures)
            {
                string[] kv = fe.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if(kv.Length==2)
                {
                   int feID =int.Parse( kv[0]);
                   string[] edges = kv[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                   List<int> list = new List<int>();
                   foreach (var edge in edges)
                   {
                       list.Add(int.Parse(edge));
                   }
                   fesStruct[feID] = list;
                }

            }
            #endregion

            int featureCount = fesStruct.Count;
            IInvalidArea pinvalidArea = new ESRI.ArcGIS.Carto.InvalidAreaClass();
            IFeatureConstruction pfeBuild = new FeatureConstructionClass();
            int numK = 0;
            foreach (KeyValuePair<int, List<int>> kv in fesStruct)
            {
                int lineCount = kv.Value.Count;
                numK++;
                string cc = fclTB.GetFeature(kv.Key).get_Value(indexCCold).ToString();
                start++;
                wo.SetText("正在构面当前【"+start+"】,OID:" + kv.Key + "当前第" + numK + "拓扑边线：" + lineCount + ",共" + fesStruct.Count);
                
                if (lineCount > mLineLimit)
                {
                    CreatePolygonFromGeosRingShell(kv.Key, cc, fclTempNameBig,poi);
                }
                else
                {
                    List<int> feGeoIDs = CreatePolygonFromGeosRing(kv.Value, pinvalidArea, pfeBuild, pInmemberFcl, fclTopLine);
                    foreach (int id in feGeoIDs)
                    {
                        IGeometry pgeo = pInmemberFcl.GetFeature(id).Shape;
                        IFeatureBuffer featurebuffer = pEditFc.CreateFeatureBuffer();
                        featurebuffer.Shape = pgeo;

                        featurebuffer.set_Value(indexCC, cc);
                        peditCursor.InsertFeature(featurebuffer);


                    }
                }
                GC.Collect();
            }



            peditCursor.Flush();
            Marshal.ReleaseComObject(pfeBuild);
            Marshal.ReleaseComObject(pinvalidArea);
            Marshal.ReleaseComObject(pInmemberFcl);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(peditCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fclTopLine);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fclTB);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEditFc);

        }

        //线转面并，赋值
        private void PolylineToDLTB()
        {
            string fclName = mFclName;
            IFeatureClass DLTB = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(fclName);//原始的
           
            IFeatureClass DLTB_Polyline = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB_Selected_sim2");//抽稀线

            //遍历所有的线
            IFeatureCursor featureCursor = DLTB_Polyline.Search(null, false);
            int count = DLTB_Polyline.FeatureCount(null);
            IFeature feature = null;
            // 添加数据结构
            #region
            Dictionary<int, List<int>> fesStruct = new Dictionary<int, List<int>>();//新的数据结构:DLTB的feid->多边形转线的所有ID
            //List<PolygonToLineStruct> OID_L_RFID = new List<PolygonToLineStruct>();
            int iNum = 0;
            while ((feature = featureCursor.NextFeature()) != null)
            {
                iNum++;
                if(iNum%10000==0)
                {
                    Console.WriteLine("获取化简边线:"+iNum);
                }
                int lineID = feature.OID;
                int LeftOID = int.Parse(feature.get_Value(feature.Fields.FindField("LEFT_FID")).ToString());
                int RightOID = int.Parse(feature.get_Value(feature.Fields.FindField("RIGHT_FID")).ToString());
                if (LeftOID != -1)
                {
                    if (fesStruct.ContainsKey(LeftOID))
                    {
                        List<int> ids = fesStruct[LeftOID];
                        if (!ids.Contains(feature.OID))
                        {
                            ids.Add(feature.OID);
                            fesStruct[LeftOID] = ids;
                        }
                    }
                    else
                    {
                        List<int> ids = new List<int>();
                        ids.Add(feature.OID);
                        fesStruct.Add(LeftOID, ids);
                    }

                }
                if (RightOID != -1)
                {
                    if (fesStruct.ContainsKey(RightOID))
                    {
                        List<int> ids = fesStruct[RightOID];
                        if (!ids.Contains(feature.OID))
                        {
                            ids.Add(feature.OID);
                            fesStruct[RightOID] = ids;
                        }
                    }
                    else
                    {
                        List<int> ids = new List<int>();
                        ids.Add(feature.OID);
                        fesStruct.Add(RightOID, ids);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
            }
            GC.Collect();
            #endregion
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
            //遍历:数据结构
            IFeatureClass currentFcl = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(fclName);
            if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fclName + "_New"))
            {
                IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(fclName + "_New") as IDataset;
                ds.Delete();
            }
            IFeatureClass pEditFc = (mWorkSpace as IFeatureWorkspace).CreateFeatureClass(fclName + "_New", currentFcl.Fields, null, null, currentFcl.FeatureType, currentFcl.ShapeFieldName, "");
            //IFeatureClass pInmemberFcl = CreateFclInmemeory("TestPly", "TestPly", (pEditFc as IGeoDataset).SpatialReference, esriGeometryType.esriGeometryPolygon, pEditFc.Fields);
            if ((mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "TestPly"))
            {
                IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("TestPly") as IDataset;
                ds.Delete();
            }
            IFeatureClass pInmemberFcl = (mWorkSpace as IFeatureWorkspace).CreateFeatureClass("TestPly", currentFcl.Fields, null, null, currentFcl.FeatureType, currentFcl.ShapeFieldName, "");


            IFeatureCursor peditCursor = pEditFc.Insert(true);
            int indexCCold = DLTB.FindField("DLBM");
            int indexCC = pEditFc.FindField("DLBM");

            int featureCount = fesStruct.Count;
            IInvalidArea pinvalidArea = new ESRI.ArcGIS.Carto.InvalidAreaClass();
            IFeatureConstruction pfeBuild = new FeatureConstructionClass();
            int numK = 0;
            foreach (KeyValuePair<int, List<int>> kv in fesStruct)
            {  
                int lineCount = kv.Value.Count;
                numK++;
                string cc = DLTB.GetFeature(kv.Key).get_Value(indexCCold).ToString();
                wo.SetText("正在构面,OID:" + kv.Key + "当前第" + numK + "拓扑边线：" + lineCount + ",共" + fesStruct.Count);
                if (lineCount > mLineLimit)
                {
                    CreatePolygonFromGeosRingShell(kv.Key, cc);
                    #region
                    //if (mTempFcl==null&&(mTempWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mTempFclName))
                    //{
                    //    mTempFcl = (mTempWorkSpace as IFeatureWorkspace).OpenFeatureClass(mTempFclName);
                        
                    //}
                    //if(mTempFcl!=null)
                    //{

                    //    IFeature fe = null;
                    //    IQueryFilter qf = new QueryFilterClass { WhereClause = "IsHole='None'" };
                    //    IFeatureCursor cursor = mTempFcl.Search(qf, false);
                    //    while ((fe = cursor.NextFeature()) != null)
                    //    {
                    //        IGeometry pgeo = fe.Shape;
                    //        IFeatureBuffer featurebuffer = pEditFc.CreateFeatureBuffer();
                    //        featurebuffer.Shape = pgeo;
                    //        string cc = DLTB.GetFeature(kv.Key).get_Value(indexCCold).ToString();
                    //        featurebuffer.set_Value(indexCC, cc);
                    //        peditCursor.InsertFeature(featurebuffer);
                    //    }
                    //    Marshal.ReleaseComObject(cursor);

                    //}
                    #endregion
                }
                else
                {
                    List<int> feGeoIDs = CreatePolygonFromGeosRing(kv.Value, pinvalidArea, pfeBuild, pInmemberFcl, DLTB_Polyline);
                    foreach (int id in feGeoIDs)
                    {
                        IGeometry pgeo = pInmemberFcl.GetFeature(id).Shape;
                        IFeatureBuffer featurebuffer = pEditFc.CreateFeatureBuffer();
                        featurebuffer.Shape = pgeo;
                       
                        featurebuffer.set_Value(indexCC, cc);
                        peditCursor.InsertFeature(featurebuffer);


                    }
                }
                GC.Collect();
            }



            peditCursor.Flush();
            Marshal.ReleaseComObject(pfeBuild);
            Marshal.ReleaseComObject(pinvalidArea);
            Marshal.ReleaseComObject(pInmemberFcl);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(peditCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(DLTB_Polyline);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(DLTB);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEditFc);

            //将临时数据添加到追加
        }

       
        private void DeleteOverlap()
        {
            string name = "DLTB_New";

            Geoprocessor gp = mGP;
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", mWorkSpace.PathName);
            IGeoProcessorResult pGeoResult = null;
            DeleteIdentical diGP = new DeleteIdentical();
            diGP.in_dataset = name;

            diGP.fields = "SHAPE_Length";
            pGeoResult = (IGeoProcessorResult)gp.Execute(diGP, null);
        }
        private void DeleteSim_Smo(IWorkspace mWorkSpace)
        {
            try
            {
                IEnumDataset pEnumDataset = mWorkSpace.get_Datasets(esriDatasetType.esriDTFeatureClass) as IEnumDataset;
                pEnumDataset.Reset();
                IDataset pDataset = pEnumDataset.Next();
                while (pDataset != null)
                {
                    if (pDataset.Name == "DLTB_Selected_sim2" || pDataset.Name == "DLTBHB_Selected" || pDataset.Name == "DLTBToLine")
                    // if (pDataset.Name == "DLTB_Selected_sim" || pDataset.Name == "DLTB_Selected_sim_Pnt" || pDataset.Name == "DLTB_Selected_sim2_Pnt" || pDataset.Name == "DLTB_Selected_smo")
                    {
                        if (pDataset.CanDelete())
                            pDataset.Delete();
                    }
                    pDataset = pEnumDataset.Next();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void DeleteFeatureClassInWorkspace(IWorkspace _Workspace,string _FeatureClassName)
        {
            IWorkspace2 ws2 = _Workspace as IWorkspace2;
            if (ws2.get_NameExists(esriDatasetType.esriDTFeatureClass, _FeatureClassName))
            {
                ((mWorkSpace as IFeatureWorkspace).OpenFeatureClass(_FeatureClassName) as IDataset).Delete();
            }
            if (ws2.get_NameExists(esriDatasetType.esriDTTable, _FeatureClassName))
            {
                ((mWorkSpace as IFeatureWorkspace).OpenTable(_FeatureClassName) as IDataset).Delete();
            }
        }
        private void RunTool(Geoprocessor geoprocessor, IGPProcess process, ITrackCancel TC=null)
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
        private void SaveSelectionFeatureToGDB(IFeatureLayer featureLayer, IWorkspace ws, string name)
        {
            try
            {
                IFeatureClass inputFeatureClass = featureLayer.FeatureClass;
                IDataset inputDataset = (IDataset)inputFeatureClass;
                IDatasetName inputDatasetName = (IDatasetName)inputDataset.FullName;

                // Get the layer's selection set. 
                IFeatureSelection featureSelection = (IFeatureSelection)featureLayer;
                ISelectionSet selectionSet = featureSelection.SelectionSet;
                IDataset ds = (IDataset)ws;
                IWorkspaceName wsName = (IWorkspaceName)ds.FullName;
                IFeatureClassName featClsName = new FeatureClassNameClass();
                IDatasetName dsName = (IDatasetName)featClsName;
                dsName.WorkspaceName = wsName;
                dsName.Name = name;

                //// Use the IFieldChecker interface to make sure all of the field names are valid for a shapefile. 
                IFieldChecker fieldChecker = new FieldCheckerClass();
                IFields shapefileFields = null;
                IEnumFieldError enumFieldError = null;
                fieldChecker.InputWorkspace = inputDataset.Workspace;
                fieldChecker.ValidateWorkspace = ws;
                fieldChecker.Validate(inputFeatureClass.Fields, out enumFieldError, out shapefileFields);

                // At this point, reporting/inspecting invalid fields would be useful, but for this example it's omitted.

                // We also need to retrieve the GeometryDef from the input feature class. 
                int shapeFieldPosition = inputFeatureClass.FindField(inputFeatureClass.ShapeFieldName);
                IFields inputFields = inputFeatureClass.Fields;

                IField shapeField = inputFields.get_Field(shapeFieldPosition);
                IGeometryDef geometryDef = shapeField.GeometryDef;

                IGeometryDef pGeometryDef = new GeometryDef();
                IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
                pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
                pGeometryDefEdit.SpatialReference_2 = (featureLayer as IGeoDataset).SpatialReference;

                // Now we can create a feature data converter. 
                IFeatureDataConverter2 featureDataConverter2 = new FeatureDataConverterClass();
                IEnumInvalidObject enumInvalidObject = featureDataConverter2.ConvertFeatureClass(inputDatasetName, null, selectionSet, null, featClsName, pGeometryDef, shapefileFields, "", 1000, 0);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
    }
}

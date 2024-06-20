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
using ESRI.ArcGIS.ConversionTools;
using System.Threading;
using ESRI.ArcGIS.AnalysisTools;
using System.Xml.Linq;
using System.Diagnostics;
namespace ShellTBDivided
{
    /// <summary>
    /// GP融合
    /// </summary>
    public class TBOtherClass
    {
       
        WaitOperation mWo = new WaitOperation();
        private Geoprocessor mGp= new Geoprocessor();
        public TBOtherClass()
        {
            mGp.OverwriteOutput = true;
        }

        public void UnionBuildingHYDA(string path, bool unionHYDA, bool unionBuilding)
        {
            try
            {
                IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace fgdbWorkspace = workspaceFactory.OpenFromFile(path, 0);
                if ((fgdbWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "DLTB") == false)
                {
                    MessageBox.Show("缺少DLTB层！ ");
                    return;
                }
               
                IFeatureClass editfcl = (fgdbWorkspace as IFeatureWorkspace).OpenFeatureClass("DLTB");
                IFeatureCursor editCursor = editfcl.Insert(true);
                int index = editfcl.FindField("DLBM");
                List<string> fclInfos = new List<string>();
                fclInfos.AddRange(new string[] { "HYDA", "Building" });
                for (int i = 0; i < fclInfos.Count; i++)
                {
                    if (fclInfos[i] == "HYDA")
                    {
                        if (!unionHYDA)
                            continue;
                    }
                    if (fclInfos[i] == "Building")
                    {
                        if (!unionBuilding)
                            continue;
                    }
                    if (!(fgdbWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fclInfos[i]))
                        continue;
                    string msg = string.Format("正在处理:{0}/{1}", (i + 1) + ":" + fclInfos[i], fclInfos.Count);
                    mWo.SetText(msg);
                    IFeatureClass fcl = (fgdbWorkspace as IFeatureWorkspace).OpenFeatureClass(fclInfos[i]);
                    int index_ = fcl.FindField("DLBM");
                    IFeature fe;
                    IFeatureCursor cursor = fcl.Search(null, false);
                    IFeatureBuffer buffer = editfcl.CreateFeatureBuffer();
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        buffer.Shape = fe.Shape;
                        buffer.set_Value(index, fe.get_Value(index_));
                        editCursor.InsertFeature(buffer);
                    }
                    Marshal.ReleaseComObject(cursor);
                    editCursor.Flush();
                }

                Marshal.ReleaseComObject(editCursor);
            }
            catch
            {
            }
        }
        private IWorkspace mWorkspace = null;
        private string mFclName = "DLTB";
        private double mArea = 1;

        public double Area
        {
            get { return mArea; }
            set { mArea = value; }
        }
        private double mWidth = 1;

        public double Width
        {
            get { return mWidth; }
            set { mWidth = value; }
        }
        private double mScale = 1;

        public double Scale
        {
            get { return mScale; }
            set { mScale = value; }
        }
        private double mBendwidth = 1;

        public double Bendwidth
        {
            get { return mBendwidth; }
            set { mBendwidth = value; }
        }
        private double mSmoothwidth = 1;

        public double Smoothwidth
        {
            get { return mSmoothwidth; }
            set { mSmoothwidth = value; }
        }
        public void ExcuteHYDAGen(string path,string fclName,bool deleteFlag)
        {
            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspace fgdbWorkspace = workspaceFactory.OpenFromFile(path, 0); 
            mWorkspace = fgdbWorkspace;
            if (fclName == "DLTB")//如果是DLTB则，导出为HYDA并融合
            {
                Console.WriteLine("导出河流面....");
                ExportHYDA(fclName);
                DissovleHYDA();
            }
            IFeatureClass fclHYDA = (fgdbWorkspace as IFeatureWorkspace).OpenFeatureClass("HYDA");
            Console.WriteLine("双线河选取....");
            HYDASelect(fclHYDA, deleteFlag);
            Console.WriteLine("双线河弯曲化简....");
            HYDASimplifyAndSmoothTop(fclHYDA);
            Console.WriteLine("河流房屋冲突处理....");
            HYDAEraseBuilding();

        }
        private void DeleteOverlapShell(string fclName)
        {
            string name = fclName;

            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("Arg");
                XElement cmd = new XElement("Command");

                cmd = new XElement("TBOverlapDel");
                cmd.Add(new XElement("InFeature", mWorkspace.PathName + "\\" + name));//
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
         
        private void ExportHYDA(string fclName)
        {

            string sql = "DLBM Like '1101%'or DLBM Like '1102%'or DLBM Like '1103%'or DLBM Like '1106%'or DLBM Like '1104%'or DLBM Like '1107%' or DLBM Like '1109%'";
            
            Geoprocessor gp = mGp;
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", mWorkspace.PathName);
           


            try
            {
              
                {

                    //MakeFeatureLayer makefeaturelayer = new MakeFeatureLayer();
                    //makefeaturelayer.in_features = fclName;
                    //makefeaturelayer.out_layer = "in_Lyr1";
                    //makefeaturelayer.where_clause = "OBJECTID= 4598";
                    //RunTool(gp, makefeaturelayer, null);

                    //导出为HYDAExport
                    FeatureClassToFeatureClass gpToGDB = new FeatureClassToFeatureClass();
                    gpToGDB.in_features = fclName;
                    gpToGDB.out_path = mWorkspace.PathName;
                    gpToGDB.out_name = "HYDAExport";
                    gpToGDB.where_clause = sql;
                    mGp.Execute(gpToGDB, null);
                    //删除原来的HYDA
                    if ((mWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "HYDA"))
                    {
                        Geoprocessor gp2 = mGp;
                        gp2.OverwriteOutput = true;
                        Delete gpDelete = new Delete();
                        gpDelete.in_data = mWorkspace.PathName + "\\HYDA";
                        try
                        {
                            gp2.Execute(gpDelete, null);
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show(err.ToString());
                        }
                    }
                    //将HYDAExport改名为HYDA
                    var _FeatureClass = (mWorkspace as IFeatureWorkspace).OpenFeatureClass("HYDAExport");
                    var _Dataset = _FeatureClass as IDataset;
                    if (_Dataset.CanRename())
                        _Dataset.Rename("HYDA");



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
        
    
        private void DissovleHYDA()
        {
            try
            {
                string fclName = "HYDA";
                string workspacePath = mWorkspace.PathName;
                Geoprocessor GPTool = mGp;
                string infeatures = workspacePath + @"\" + fclName;
                string outfeatures = workspacePath + @"\" + fclName + "ToDissolve";
                ESRI.ArcGIS.DataManagementTools.Dissolve disGp = new ESRI.ArcGIS.DataManagementTools.Dissolve(infeatures, outfeatures);
                disGp.dissolve_field = "DLBM";
                disGp.multi_part = "SINGLE_PART";
                IGeoProcessorResult tGeoResult = null;
                tGeoResult = (IGeoProcessorResult)GPTool.Execute(disGp, null);
                if (tGeoResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                    
                    //先删除原来的HYDA
                    if ((mWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fclName))
                    {
                        Geoprocessor gp2 = mGp;
                        gp2.OverwriteOutput = true;
                        Delete gpDelete = new Delete();
                        gpDelete.in_data = mWorkspace.PathName + "\\" + fclName;
                        try
                        {
                            gp2.Execute(gpDelete, null);
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show(err.ToString());
                        }
                    }
                    //将fclNameToDissolve重命名为fclName
                    var _FeatureClass = (mWorkspace as IFeatureWorkspace).OpenFeatureClass(fclName + "ToDissolve");
                    var _Dataset = _FeatureClass as IDataset;
                    if (_Dataset.CanRename())
                        _Dataset.Rename(fclName);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public double HYDAWidth = 1.4;
        //处理面积不够的湖泊和宽度不足的
        private void HYDASelect(IFeatureClass hydaFC,bool deleteFlag)
        {
            try
            {                
                List<string> riverdlbmList = new List<string> { "1101", "1107", "1107A", "1109" };
                List<string> lakedlbmList = new List<string> { "1102", "1103", "1104", "1104A", "1104K", "1106", "1109" };
                HYDAWidth = Width;
                double limit = mArea;
                int count = 0;
                count = hydaFC.FeatureCount(null);
               
                int dlbmFieldIndex = hydaFC.FindField("dlbm");
                int DeleteFlagFieldIndex = hydaFC.FindField("DeleteFlag");
                if (DeleteFlagFieldIndex < 0)
                {
                    AddField(hydaFC, "DeleteFlag");
                    DeleteFlagFieldIndex = hydaFC.FindField("DeleteFlag");
                }
                int step = 0;
                string dlbm;
                IFeatureCursor pCursor = hydaFC.Update(null, false);
               // IFeatureCursor pCursor = hydaFC.Search(null, false);
                IFeature pFeature;
                List<int> ids = new List<int>();
                while ((pFeature = pCursor.NextFeature()) != null)
                {
                    dlbm = pFeature.get_Value(dlbmFieldIndex).ToString();

                    double curArea = (pFeature.Shape as IArea).Area;
                    double length = (pFeature.Shape as IPolygon).Length * 0.5;
                    double width = curArea / length;
                    Console.WriteLine("正在判别宽度指标和面积指标：" + (++step));
                    bool isDelete = false;
                    if (riverdlbmList.Contains(dlbm))
                    {
                        if (width <= HYDAWidth * 0.001 * mScale)//不够宽度指标:
                        {
                            if (!deleteFlag)
                            {
                                pFeature.set_Value(DeleteFlagFieldIndex, "-1");
                                pCursor.UpdateFeature(pFeature);
                            }
                            else
                            {
                                pCursor.DeleteFeature();
                            }
                        }
                    }
                    if (lakedlbmList.Contains(dlbm) && !isDelete)
                    {
                        if (curArea <= limit * 0.000001 * mScale * mScale)//不够指标:
                        {
                            ids.Add(pFeature.OID);
                            if (!deleteFlag)
                            {
                                pFeature.set_Value(DeleteFlagFieldIndex, "-1");
                                pCursor.UpdateFeature(pFeature);
                            }
                            else
                            {
                                //pFeature.set_Value(DeleteFlagFieldIndex, "-1");
                                //pCursor.UpdateFeature(pFeature);
                                pCursor.DeleteFeature();
                            }
                        }
                    }
                }
               
                Marshal.ReleaseComObject(pCursor);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //曲面面化简
        public void HYDASimplifyAndSmooth(IFeatureClass inputFC)
        {
            Geoprocessor gp = mGp;
            double bendValue=mBendwidth;
            double smoothValue = mSmoothwidth;
            //0.要素类
            IWorkspace ws = (inputFC as IDataset).Workspace;
            IFeatureWorkspace feaWS = ws as IFeatureWorkspace;
            string workspacePath = ws.PathName;

            gp.OverwriteOutput = true;

            string simplifyInFeature = workspacePath + @"\" + inputFC.AliasName;
            string simplifyOutFeature = workspacePath + @"\" + inputFC.AliasName + "ToSimplify";
            try
            {

                double bendLens = bendValue * mScale / 1000;
                smoothValue = smoothValue * mScale / 1000;
                ESRI.ArcGIS.CartographyTools.SimplifyPolygon simplifyLineTool = new ESRI.ArcGIS.CartographyTools.SimplifyPolygon(simplifyInFeature, simplifyOutFeature, "BEND_SIMPLIFY", bendLens);

                IGeoProcessorResult geoResult = (IGeoProcessorResult)gp.Execute(simplifyLineTool, null);
                if (geoResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                    string smoothLineOutFeature = workspacePath + @"\" + inputFC.AliasName + "ToSimplifySmooth";
                    ESRI.ArcGIS.CartographyTools.SmoothPolygon smoothLineTool = new ESRI.ArcGIS.CartographyTools.SmoothPolygon(simplifyOutFeature, smoothLineOutFeature, "PAEK", smoothValue);
                    geoResult = (IGeoProcessorResult)gp.Execute(smoothLineTool, null);
                }
            }
            catch (Exception ex)
            {
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }

                }
                MessageBox.Show(ms);
                throw ex;

            }
            finally
            {

                //删除HYDA
                //HYDAToSimplifySmooth
                //先删除原来的HYDA
                if ((mWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "HYDA"))
                {
                    Geoprocessor gp2 = mGp;
                    gp2.OverwriteOutput = true;
                    Delete gpDelete = new Delete();
                    gpDelete.in_data = mWorkspace.PathName + "\\" + "HYDA";
                    try
                    {
                        gp2.Execute(gpDelete, null);
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.ToString());
                    }
                }
                //将fclNameToDissolve重命名为fclName
                var _FeatureClass = (mWorkspace as IFeatureWorkspace).OpenFeatureClass("HYDA" + "ToSimplifySmooth");
                var _Dataset = _FeatureClass as IDataset;
                if (_Dataset.CanRename())
                    _Dataset.Rename("HYDA");


                IFeatureClass simplifyFC = feaWS.OpenFeatureClass(inputFC.AliasName + "ToSimplify");
                if (simplifyFC != null)
                {
                    IDataset dt = simplifyFC as IDataset;
                    dt.Delete();
                }

                IFeatureClass simplifyPntFC = feaWS.OpenFeatureClass(inputFC.AliasName + "ToSimplify_Pnt");
                if (simplifyPntFC != null)
                {
                    IDataset dt = simplifyPntFC as IDataset;
                    dt.Delete();
                }

               
            }
        }


        //曲面面化简wjz:保持边界处理
        public void HYDASimplifyAndSmoothTop(IFeatureClass inputFC)
        {
            Geoprocessor gp = mGp;
            double bendValue = mBendwidth;
            double smoothValue = mSmoothwidth;
            //0.要素类
            IWorkspace ws = (inputFC as IDataset).Workspace;
            IFeatureWorkspace feaWS = ws as IFeatureWorkspace;
            string workspacePath = ws.PathName;

            gp.OverwriteOutput = true;



            //新建DLTBToLine层之前先删去已有图层
           
          
            try
            {           
                gp.OverwriteOutput = true;
                string infeatures = workspacePath + @"\" + "HYDA";
                string outfeatures = workspacePath + @"\HYDAToLine";
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
            //可能水系面内没有要素，则退出
            if(!((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "HYDAToLine")))
            {
                return;
            }
            string simplifyInFeature = workspacePath + @"\" + @"\HYDAToLine"; 
            string simplifyOutFeature = workspacePath + @"\" + @"\HYDAToLine" +"ToSimplify";
            try
            {

                double bendLens = bendValue * mScale / 1000;
                smoothValue = smoothValue * mScale / 1000;
                ESRI.ArcGIS.CartographyTools.SimplifyLine simplifyLineTool = new ESRI.ArcGIS.CartographyTools.SimplifyLine(simplifyInFeature, simplifyOutFeature, "BEND_SIMPLIFY", bendLens);

                IGeoProcessorResult geoResult = (IGeoProcessorResult)gp.Execute(simplifyLineTool, null);
                if (geoResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                    string smoothLineOutFeature = workspacePath  + @"\HYDAToLine" + "ToSimplifySmooth";
                    ESRI.ArcGIS.CartographyTools.SmoothLine smoothLineTool = new ESRI.ArcGIS.CartographyTools.SmoothLine(simplifyOutFeature, smoothLineOutFeature, "PAEK", smoothValue);
                    geoResult = (IGeoProcessorResult)gp.Execute(smoothLineTool, null);
                    //线转面
                    PolylineToDLTB("HYDA", "HYDAToLineToSimplifySmooth", ws);
                    Console.WriteLine("删除重复面..");
                    DeleteOverlapShell("HYDA_New");
                }
            }
            catch (Exception ex)
            {
                string ms = "";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        ms += gp.GetMessage(Count);
                    }

                }
                MessageBox.Show(ms);
                throw ex;

            }
            finally
            {
                IFeatureClass deldteFC = feaWS.OpenFeatureClass("HYDAToLine");
                if (deldteFC != null)
                {
                    IDataset dt = deldteFC as IDataset;
                    dt.Delete();
                }

               deldteFC = feaWS.OpenFeatureClass("HYDAToLineToSimplify");
                if (deldteFC != null)
                {
                    IDataset dt = deldteFC as IDataset;
                    dt.Delete();
                }
                deldteFC = feaWS.OpenFeatureClass("HYDAToLineToSimplifySmooth");
                if (deldteFC != null)
                {
                    IDataset dt = deldteFC as IDataset;
                    dt.Delete();
                }
                deldteFC = feaWS.OpenFeatureClass("HYDAToLineToSimplify_Pnt");
                if (deldteFC != null)
                {
                    IDataset dt = deldteFC as IDataset;
                    dt.Delete();
                }
                deldteFC = feaWS.OpenFeatureClass("HYDA");
                if (deldteFC != null)
                {
                    IDataset dt = deldteFC as IDataset;
                    dt.Delete();
                }
                deldteFC = feaWS.OpenFeatureClass("HYDA_NEW");
                if (deldteFC != null)
                {
                    var _Dataset = deldteFC as IDataset;
                    if (_Dataset.CanRename())
                        _Dataset.Rename("HYDA");
                }


            }
        }
        /// <summary>
        /// 综合后房屋跟水系之间有重叠，利用房屋擦除水系
        /// </summary>
        private void HYDAEraseBuilding()
        {
            try
            {
                if (!(mWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "Building"))
                {
                    return;
                    if (!(mWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "HYDA"))
                    {
                        return;
                    }
                }
                string workspacePath = mWorkspace.PathName;
                Geoprocessor GPTool =mGp;
                string infeatures = workspacePath + @"\" + "Building";
                string erasefeatures = workspacePath + @"\" + "HYDA";   
                string outfeatures = workspacePath + @"\" + "BuildingErase";     
                Erase disGp = new Erase();
                disGp.in_features = infeatures;
                disGp.erase_features = erasefeatures;
                disGp.out_feature_class = outfeatures;
                IGeoProcessorResult tGeoResult = null;
                tGeoResult = (IGeoProcessorResult)GPTool.Execute(disGp, null);
                if (tGeoResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                    //先删除原来的HYDA
                    if ((mWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "Building"))
                    {
                        Geoprocessor gp2 = mGp;
                        gp2.OverwriteOutput = true;
                        Delete gpDelete = new Delete();
                        gpDelete.in_data = mWorkspace.PathName + "\\" + "Building";
                        try
                        {
                            gp2.Execute(gpDelete, null);
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show(err.ToString());
                        }
                    }
                    //将fclNameToDissolve重命名为fclName
                    var _FeatureClass = (mWorkspace as IFeatureWorkspace).OpenFeatureClass("BuildingErase");
                    var _Dataset = _FeatureClass as IDataset;
                    if (_Dataset.CanRename())
                        _Dataset.Rename("Building");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        //线转面并，赋值
        private void PolylineToDLTB(string soucePolygonName,string lineName,IWorkspace mWorkSpace)
        {
            string fclName = soucePolygonName;
            IFeatureClass DLTB = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(fclName);//原始的
            IFeatureClass DLTB_Polyline = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(lineName);//抽稀线

            //遍历所有的线
            IFeatureCursor featureCursor = DLTB_Polyline.Search(null, false);
            int count = DLTB_Polyline.FeatureCount(null);
            IFeature feature = null;
            // 添加数据结构
            #region
            Dictionary<int, List<int>> fesStruct = new Dictionary<int, List<int>>();//新的数据结构:DLTB的feid->多边形转线的所有ID
            //List<PolygonToLineStruct> OID_L_RFID = new List<PolygonToLineStruct>();

            while ((feature = featureCursor.NextFeature()) != null)
            {
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

            int deleteFlagOld=DLTB.FindField("DeleteFlag");
            int deleteFlag = pEditFc.FindField("DeleteFlag");
            int featureCount = fesStruct.Count;
            IInvalidArea pinvalidArea = new ESRI.ArcGIS.Carto.InvalidAreaClass();
            IFeatureConstruction pfeBuild = new FeatureConstructionClass();
            int numK = 0;
            foreach (KeyValuePair<int, List<int>> kv in fesStruct)
            {
                numK++;              
                List<int> feGeoIDs = CreatePolygonFromGeosRing(kv.Value, pinvalidArea, pfeBuild, pInmemberFcl, DLTB_Polyline);
                foreach (int id in feGeoIDs)
                {
                    try
                    {
                        IGeometry pgeo = pInmemberFcl.GetFeature(id).Shape;
                        IFeatureBuffer featurebuffer = pEditFc.CreateFeatureBuffer();
                        featurebuffer.Shape = pgeo;
                        string cc = DLTB.GetFeature(kv.Key).get_Value(indexCCold).ToString();
                        featurebuffer.set_Value(indexCC, cc);
                        if (deleteFlagOld != -1 && deleteFlag != -1)
                        {
                            object deleteVal = DLTB.GetFeature(kv.Key).get_Value(deleteFlagOld);
                            featurebuffer.set_Value(deleteFlag, deleteVal);
                        }
                        peditCursor.InsertFeature(featurebuffer);
                    }
                    catch
                    {
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
        }
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
     
        private List<int> CreatePolygonFromGeosRing0(List<int> ids, IInvalidArea pinvalidArea, IFeatureConstruction pfeBuild, IFeatureClass pPolygonFcl, IFeatureClass fclline)
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
                pfeBuild.AutoCompleteFromGeometries(pPolygonFcl, pEnvelope, geometryBag as IEnumGeometry, pinvalidArea, 0.01, pws, out pSet);
                IEnumIDs pEnumIDs = pSet.IDs;
                pEnumIDs.Reset();
                int id = pEnumIDs.Next();
                Dictionary<int, IGeometry> dics = new Dictionary<int, IGeometry>();//外环构成的大环
                Dictionary<int, string> fesCC = new Dictionary<int, string>();//所有要素
                string cc = "";
                #region//找最大外环
                while (id > 0)
                {
                    IGeometryCollection pGeoCol = pPolygonFcl.GetFeature(id).Shape as IGeometryCollection;


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

                    id = pEnumIDs.Next();
                }
                #endregion
                List<int> keepIDs = new List<int>();
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
                        bool flag = true;
                        IRelationalOperator rp = pPolygonFcl.GetFeature(feid).Shape as IRelationalOperator;
                        foreach (KeyValuePair<int, IGeometry> ringdic in dics)
                        {
                            if (rp.Within(ringdic.Value))
                            {

                                flag = false;
                                break;
                            }

                        }
                        if (flag)
                        {
                            //内切
                            IGeometry pgeo = pPolygonFcl.GetFeature(feid).Shape;

                            bool qieFlag = true;
                            foreach (KeyValuePair<int, IGeometry> ringdic in dics)
                            {
                                IGeometry pPly = (pgeo as ITopologicalOperator).Intersect(ringdic.Value, esriGeometryDimension.esriGeometry1Dimension);
                                if (pPly != null)
                                {
                                    if (!pPly.IsEmpty)
                                    {
                                        qieFlag = false;
                                        break;
                                    }
                                }
                            }
                            if (qieFlag)//不是内切
                            {
                                keepIDs.Add(feid);
                            }

                        }
                        #endregion
                    }
                }
                Marshal.ReleaseComObject(geometryBag);
                Marshal.ReleaseComObject(pSet);
                Marshal.ReleaseComObject(pEnumIDs);
                return keepIDs;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        /// <summary>
        /// 添加字符串字段，有该字段则不添加
        /// </summary>
        /// <param name="fc">待添加字段要素类</param>
        /// <param name="newFieldName">字段名</param>
        /// <param name="fieldLen">字段长度</param>
        private  void AddField(IFeatureClass fc, string newFieldName, int fieldLen = 50)
        {
            if (fc.FindField(newFieldName) >= 0) return;//有该字段则返回，不添加
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = newFieldName;
            pFieldEdit.AliasName_2 = newFieldName;
            pFieldEdit.Length_2 = fieldLen;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;

            IClass pTable = fc as IClass;
            pTable.AddField(pField);
        }
        
      

    }
}

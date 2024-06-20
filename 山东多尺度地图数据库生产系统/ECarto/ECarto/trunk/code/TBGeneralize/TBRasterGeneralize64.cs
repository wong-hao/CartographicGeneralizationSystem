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
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.esriSystem;
using System.Threading;
using ESRI.ArcGIS.Geometry;

namespace ShellTBDivided
{
       //支持64位 GP
        public  class TBRasterGeneralize64
        {

            private string mExportGDBPath = "";
            private List<string> mLBGDBPathList = new List<string>();
            private string mFolderPath = string.Empty;//临时存放处理的
            private double mMapScale = 1;

            private string mZNTable = "";
            private string mNZTable = "";
            private int mStep = 0;
            private WaitOperation mWo = new WaitOperation();
            private string mFclName = "DLTB";
            private string mField = "DLBM";
            public int mmajorityStep;

            private bool mOverWrite = true;
            public bool DefautGDB = false;
         

            public string ZNTable
            {
                get { return mZNTable; }
                set { mZNTable = value; }
            }
            public string NZTable
            {
                get { return mNZTable; }
                set { mNZTable = value; }
            }
           

         
            public TBRasterGeneralize64(string export, double mapscale, string folder, List<string> gdbs, bool mOverWrite_ = true)
            {
                mExportGDBPath = export;
                mFolderPath = folder;
                mMapScale = mapscale;
                mLBGDBPathList = gdbs;
                mOverWrite = mOverWrite_;
                mGP.OverwriteOutput = true;
                mExportRasterFolder = GetGDBFolder(mExportGDBPath);
            }
            public TBRasterGeneralize64(string export, double mapscale, string folder, List<string> gdbs, int majorityStep,bool mOverWrite_ = true)
            {
                mExportGDBPath = export;
                mFolderPath = folder;
                mMapScale = mapscale;
                mLBGDBPathList = gdbs;
                mOverWrite = mOverWrite_;
                mGP.OverwriteOutput = true;
                mExportRasterFolder = GetGDBFolder(mExportGDBPath);
                this.mmajorityStep = majorityStep;
            }
            private string GetGDBFolder(string  GDBPath)
            {
              
                string folder = new FileInfo(GDBPath).Directory.FullName;
                string f = folder + "\\RasterGen";
                if (!System.IO.Directory.Exists(f))
                    System.IO.Directory.CreateDirectory(f);
                return folder+"\\RasterGen";
            }
            //预处理 step 1 赋值
            public void ExcuteRasterPre()
            {
                 bool defaultGDB=DefautGDB;
                 WaitOperation wo = mWo;
                    
                //检查并创建输出文件夹
                if (Directory.Exists(mExportGDBPath) == true)
                {
                    if (!defaultGDB)
                    {
                        
                        wo.SetText("正在创建输出文件");
                        Application.DoEvents();
                        CreateGDB(mExportGDBPath);
                        
                    }
                }
                else
                {
                    wo.SetText("正在创建输出文件");
                    Application.DoEvents();
                    CreateGDB(mExportGDBPath);
                }
                for (int i = 0; i < mLBGDBPathList.Count; i++)
                {
                    mStep++;
                    string GDBPath = mLBGDBPathList[i].ToString();
                    string FileName = System.IO.Path.GetFileNameWithoutExtension(GDBPath);
                    #region//添加TAG整型字段，DLBM复制到TAG，并将字母去掉
                    wo.SetText("正在转换等级，进度:" + (i + 1) + "/" + mLBGDBPathList.Count + ",当前：" + System.IO.Path.GetFileNameWithoutExtension(GDBPath));
                    Application.DoEvents();
                    IFeatureClass dltbFcls = GApplication.Application.QueryFeatureClass((GDBPath), mFclName);
                    GApplication.Application.AddField(dltbFcls, "TAG");
                    int TAGFieldIndex = dltbFcls.FindField("TAG");
                    int DLBMFieldIndex = dltbFcls.FindField(mField);
                    Geoprocessor gp = mGP;
                    gp.OverwriteOutput = true;
                    IFeatureCursor cursor = dltbFcls.Update(null, true);
                    string dlbm;
                    IFeature fea;
                    int numK = 0;
                    while ((fea = cursor.NextFeature()) != null)
                    {
                        dlbm = fea.get_Value(DLBMFieldIndex).ToString();
                        dlbm = dlbm.Replace("H", "7");
                        dlbm = dlbm.Replace("A", "");
                        dlbm = dlbm.Replace("K", "");
                        fea.set_Value(TAGFieldIndex, dlbm);
                        cursor.UpdateFeature(fea);
                        numK++;
                        if (numK % 2000 == 0)
                        {
                            wo.SetText("正在转换等级：" + numK.ToString());
                        }
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                    #endregion
                }
            }
           
            private string mExportRasterFolder = "";
            //预处理 step 2  转栅格
            public void ExcuteRasterToRaster(string  GDBPath)
            {
                WaitOperation wo = mWo;
               
                //for (int i = 0; i < mLBGDBPathList.Count; i++)
                {
                    mStep++;
                  
                    string FileName = System.IO.Path.GetFileNameWithoutExtension(GDBPath);
                    
                    Geoprocessor gp = mGP;
                    try
                    {

                        //gp.Execute(pCalculateField, null);
                        wo.SetText("DLTB进行栅格化");
                        ///图斑等级转换完成后，对DLTB进行栅格化
                        ESRI.ArcGIS.ConversionTools.PolygonToRaster pPolygonToRaster = new ESRI.ArcGIS.ConversionTools.PolygonToRaster();
                        pPolygonToRaster.in_features = GDBPath + "\\" + mFclName;
                        pPolygonToRaster.out_rasterdataset = mExportRasterFolder + "\\" + "DLTBRaster" + FileName+".tif";
                        pPolygonToRaster.value_field = "TAG";
                        //通过比例尺判断初始栅格网格大小，如果比例尺小于15，则初始为2，如果比例尺大于15，则初始为5，如过大于100，初始为10；
                        if (mMapScale < 150000)
                        {
                            pPolygonToRaster.cellsize = "2";
                         
                        }
                        else if (mMapScale >= 150000 && mMapScale < 1000000)
                        {
                            pPolygonToRaster.cellsize = "5";
                        }
                        else if (mMapScale >= 1000000)
                        {
                            pPolygonToRaster.cellsize = "20";
                        }
                        // gp.Execute(pPolygonToRaster, null);

                       // 64位GP处理
                        mGPToolsToExecute.Clear();
                        mGPToolsToExecute.Enqueue(pPolygonToRaster);
                        mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                        mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                        mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                        mGPWorkerEvent.WaitOne();
                        //mGP.Execute(pPolygonToRaster, null);
                    }

                    catch (Exception ex)
                    {
                        string ms = "";
                        if (mGP.MessageCount > 0)
                        {
                            for (int Count = 0; Count < mGP.MessageCount; Count++)
                            {
                                ms += mGP.GetMessage(Count);
                            }
                        }
                        MessageBox.Show(ms);
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            //预处理 step 3  栅格缩编
            public void ExcuteRasterToGen(string GDBPath, double NowScal)
            {  
                WaitOperation wo = mWo;
                //for (int i = 0; i < mLBGDBPathList.Count; i++)
                {
                    mStep++;
                    //string GDBPath = mLBGDBPathList[i].ToString();
                    string FileName = System.IO.Path.GetFileNameWithoutExtension(GDBPath);
                    string orginRaster = "DLTBRaster" + FileName;
                    Geoprocessor gp = mGP;
                    ///栅格完成后，开始进行逐级缩编
                    //先通过比例尺计算逐级缩编的变化表
                    List<double> ScalList = CalculateScal();
                    Dictionary<double, string> dicNameIn = new Dictionary<double, string>();
                    Dictionary<double, string> dicNameOut = new Dictionary<double, string>();
                    for(int i=0;i<ScalList.Count;i++)
                    {
                        dicNameOut[ScalList[i]] = "DLTBRaster" + ScalList[i];
                        if (i == 0)
                        {
                            dicNameIn[ScalList[0]] = orginRaster;
                        }
                        else
                        {
                          
                            dicNameIn[ScalList[i]] = "DLTBRaster" + ScalList[i - 1];
                        }
                        if (i == ScalList.Count - 1)
                        {
                            dicNameOut[ScalList[i]] = orginRaster;
                        }
                      
                    }
                 
                  
                    //读取正逆DLBM表；
                    //ReadCCCode(GDBPath, wo);
                    Application.DoEvents();
                  
                    
                    {
                        #region
                        mmajorityStep++;
                        wo.SetText("正在索引缩编，cellsize:"+NowScal.ToString() + "，第" + mmajorityStep + "/" + ScalList.Count + "：" + System.IO.Path.GetFileNameWithoutExtension(GDBPath) + "/r/n" + "总进度：" + mStep + "/" + mLBGDBPathList.Count + "，当前输入：" + mExportRasterFolder + "\\" + dicNameIn[NowScal] + ".tif");
                        //重采样
                        ESRI.ArcGIS.DataManagementTools.Resample pResample = new Resample();
                        pResample.in_raster = mExportRasterFolder + "\\" + dicNameIn[NowScal]+".tif";
                        pResample.out_raster = mExportRasterFolder + "\\" + dicNameOut[NowScal] + ".tif";
                        pResample.cell_size = NowScal.ToString();
                        pResample.resampling_type = "MAJORITY";
                        //gp.Execute(pResample, null);
                        //正向重分类 正向-----逆向
                        //ESRI.ArcGIS.SpatialAnalystTools.Reclassify pReClassify = new Reclassify();
                        //pReClassify.in_raster = mExportGDBPath + "\\" + "DLTBSYSB" + FileName; ;
                        //pReClassify.reclass_field = "Value";
                        //pReClassify.out_raster = mExportGDBPath + "\\" + "DLTBNX" + FileName; ;
                        //pReClassify.remap = mZNTable;
                       // gp.Execute(pReClassify, null);
                        //逆向重分类 逆向-----正向
                        //ESRI.ArcGIS.SpatialAnalystTools.Reclassify pReClassify2 = new Reclassify();
                        //pReClassify2.in_raster = mExportGDBPath + "\\" + "DLTBNX" + FileName; ;
                        //pReClassify2.reclass_field = "Value";
                        //pReClassify2.out_raster = mExportGDBPath + "\\" + "DLTBRaster" + FileName; ;
                        //pReClassify2.remap = mNZTable;
                      //  gp.Execute(pReClassify2, null);

                        mGPToolsToExecute.Clear();
                        mGPToolsToExecute.Enqueue(pResample);
                        //mGPToolsToExecute.Enqueue(pReClassify);
                        //mGPToolsToExecute.Enqueue(pReClassify2);
                        mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                        mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                        mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                        mGPWorkerEvent.WaitOne();

                        ESRI.ArcGIS.DataManagementTools.Delete pDelete = new Delete();
                        pDelete.in_data = mExportRasterFolder + "\\" + dicNameIn[NowScal] + ".tif";
                        gp.Execute(pDelete, null);
                        #endregion
                    }
                }
            }
          
            //处理 step 4 栅格合并镶嵌数据集
            public void ExcuteRasterMerge()
            {
                WaitOperation wo = mWo;
                //栅格合并
                try
                {
                  
                    Application.DoEvents();
                    Geoprocessor gp = mGP;
                    gp.OverwriteOutput = true;
                    string RasterFileName = "";
                    for (int i = 0; i < mLBGDBPathList.Count; i++)
                    {
                        RasterFileName = RasterFileName + mExportRasterFolder + "\\" + "DLTBRaster" + System.IO.Path.GetFileNameWithoutExtension(mLBGDBPathList[i].ToString()) + ".tif;";
                    }
                   // RasterFileName = RasterFileName.Substring(0, RasterFileName.Length - 1);
                    ESRI.ArcGIS.DataManagementTools.MosaicToNewRaster pMosaicToNewRaster = new MosaicToNewRaster();
                    pMosaicToNewRaster.input_rasters = RasterFileName;
                    pMosaicToNewRaster.output_location = mExportRasterFolder;
                    pMosaicToNewRaster.raster_dataset_name_with_extension = "DLTB0.tif";
                    pMosaicToNewRaster.pixel_type = "16_BIT_UNSIGNED";
                    wo.SetText("正在镶嵌至新栅格..."+"，输入："+RasterFileName);
                    pMosaicToNewRaster.number_of_bands = 1;
                    try
                    {

                        //gp.Execute(pMosaicToNewRaster, null);
                        mGPToolsToExecute.Clear();
                        mGPToolsToExecute.Enqueue(pMosaicToNewRaster);
                        mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                        mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                        mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                        mGPWorkerEvent.WaitOne();

                        //合并完成后删除中间各县过程数据
                        for (int i = 0; i < mLBGDBPathList.Count; i++)
                        {
                            ESRI.ArcGIS.DataManagementTools.Delete pDelete = new Delete();
                            pDelete.in_data = mExportRasterFolder + "\\" + "DLTBRaster" + System.IO.Path.GetFileNameWithoutExtension(mLBGDBPathList[i].ToString())+".tif";
                            gp.Execute(pDelete, null);
                        }
                    }
                    catch (System.Exception ex)
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
                catch
                {

                }

               
            }
            //处理 step 5  栅格滤波
            public void ExcuteRasterFliter()
            {
                WaitOperation wo = mWo;
                wo.SetText("正在处理碎片......" + "输入：" + mExportRasterFolder + "\\" + "DLTB0，1，2，3，4，5.tif");
                Application.DoEvents();
                Geoprocessor gp = mGP;
                gp.OverwriteOutput = true;
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter2 = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter3 = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter4 = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter5 = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter6 = new MajorityFilter();
                //循环做4次
                #region
                // for (int i = 0; i < 5; i++)
                {//1
                    pMajorityFilter.number_neighbors = "FOUR";
                    pMajorityFilter.majority_definition = "MAJORITY";
                    pMajorityFilter.in_raster = mExportRasterFolder + "\\" + "DLTB0.tif" ;
                    pMajorityFilter.out_raster = mExportRasterFolder + "\\" + "DLTB1";
                    //gp.Execute(pMajorityFilter, null);
                }
                //2
                {
                    pMajorityFilter2.number_neighbors = "FOUR";
                    pMajorityFilter2.majority_definition = "MAJORITY";
                    pMajorityFilter2.in_raster = mExportRasterFolder + "\\" + "DLTB1";
                    pMajorityFilter2.out_raster = mExportRasterFolder + "\\" + "DLTB2";
                   // gp.Execute(pMajorityFilter, null);
                }
                //3
                {
                    pMajorityFilter3.number_neighbors = "FOUR";
                    pMajorityFilter3.majority_definition = "MAJORITY";
                    pMajorityFilter3.in_raster = mExportRasterFolder + "\\" + "DLTB2";
                    pMajorityFilter3.out_raster = mExportRasterFolder + "\\" + "DLTB3";
                   // gp.Execute(pMajorityFilter, null);
                }
                //4
                {
                    pMajorityFilter4.number_neighbors = "FOUR";
                    pMajorityFilter4.majority_definition = "MAJORITY";
                    pMajorityFilter4.in_raster = mExportRasterFolder + "\\" + "DLTB3";
                    pMajorityFilter4.out_raster = mExportRasterFolder + "\\" + "DLTB4";
                    //gp.Execute(pMajorityFilter, null);
                }
                //5
                {
                    pMajorityFilter5.number_neighbors = "FOUR";
                    pMajorityFilter5.majority_definition = "MAJORITY";
                    pMajorityFilter5.in_raster = mExportRasterFolder + "\\" + "DLTB4";
                    pMajorityFilter5.out_raster = mExportRasterFolder + "\\" + "DLTB5";
                    //gp.Execute(pMajorityFilter, null);
                }
                //做第六次，把最后的文件名命名为DLTBDispose
                pMajorityFilter6.number_neighbors = "FOUR";
                pMajorityFilter6.majority_definition = "MAJORITY";
                pMajorityFilter6.in_raster = mExportRasterFolder + "\\" + "DLTB5";
                pMajorityFilter6.out_raster = mExportRasterFolder + "\\" + "DLTBDispose";
                //gp.Execute(pMajorityFilter, null);
                #endregion
                mGPToolsToExecute.Clear();
                mGPToolsToExecute.Enqueue(pMajorityFilter);
                mGPToolsToExecute.Enqueue(pMajorityFilter2);
                mGPToolsToExecute.Enqueue(pMajorityFilter3);
                mGPToolsToExecute.Enqueue(pMajorityFilter4);
                mGPToolsToExecute.Enqueue(pMajorityFilter5);
                mGPToolsToExecute.Enqueue(pMajorityFilter6);
                mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                mGPWorkerEvent.WaitOne();
                //删除过程数据
                ESRI.ArcGIS.DataManagementTools.Delete pDelete = new Delete();
                for (int i = 0; i <= 5; i++)
                {
                    if (i == 0)
                    {
                        pDelete.in_data = mExportRasterFolder + "\\" + "DLTB" + i.ToString() + ".tif";
                    }
                    else
                    {
                        pDelete.in_data = mExportRasterFolder + "\\" + "DLTB" + i.ToString() ;
                    }
                    gp.Execute(pDelete, null);
                }
            }
            public void ExcuteRasterToVector()
            { 
                WaitOperation wo = mWo;
                wo.SetText("正在转为DLTB......" + "，输入：" + mExportRasterFolder + "\\" + "DLTBDispose");
                Application.DoEvents();
                Geoprocessor gp = mGP;
                try
                {
                    //先删除mExportGDBPath的DLTB
                    ESRI.ArcGIS.DataManagementTools.Delete pDelete = new Delete();
                 
                    pDelete.in_data = mExportGDBPath + "\\" + "DLTB";
                    try
                    {
                        gp.Execute(pDelete, null);
                    }
                    catch
                    {

                    }
                    gp.OverwriteOutput = true;
                    ESRI.ArcGIS.ConversionTools.RasterToPolygon pRasterToPolygon = new RasterToPolygon();
                    pRasterToPolygon.in_raster = mExportRasterFolder + "\\" + "DLTBDispose";
                    pRasterToPolygon.out_polygon_features = mExportGDBPath + "\\" + "DLTB";
                    pRasterToPolygon.raster_field = "Value";
                   // gp.Execute(pRasterToPolygon, null);
                    mGPToolsToExecute.Clear();
                    mGPToolsToExecute.Enqueue(pRasterToPolygon);
                    mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                    mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                    mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                    mGPWorkerEvent.WaitOne();
                    //处理完成后删除过程数据
                    pDelete.in_data = mExportRasterFolder + "\\" + "DLTBDispose";
                    gp.Execute(pDelete, null);

                    //添加DLBM地段并删除其余字段
                    AddCCField(mExportGDBPath, "DLTB",wo);

                    
                    IFeatureClass pFeatureClass = null;
                    pFeatureClass = GApplication.Application.QueryFeatureClass(mExportGDBPath, mFclName);
                    ITable pTable = pFeatureClass as ITable;
                    int FieldIndex = pTable.FindField("grid_code");
                    IField pField = pTable.Fields.Field[FieldIndex];
                    if (FieldIndex != -1)
                    {
                        pTable.DeleteField(pField);
                    }
                    FieldIndex = pTable.FindField("Id");
                    pField = pTable.Fields.Field[FieldIndex];
                    if (FieldIndex != -1)
                    {
                        pTable.DeleteField(pField);
                    }

                  
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pTable);
                    GC.Collect();
                }
                catch (System.Exception ex)
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
            //处理 step 6  小图斑处理
            public void ExcuteRasterEliminate(int step)
            {
                WaitOperation wo = mWo;
               
                //矢量碎片简化
                DLTBFeatureClassSimplify(wo,step);   
            }
            //处理 step 7  覆盖
            public void ExcuteRasterCopy()
            {
                WaitOperation wo = mWo;
                //覆盖原始数据
                if (mOverWrite)
                {
                    wo.SetText("覆盖原始数据");
                    for (int i = 0; i < mLBGDBPathList.Count; i++)
                    {
                        string GDBPath = mLBGDBPathList[i].ToString();
                        Copy pCopy = new Copy();
                        pCopy.in_data = mExportGDBPath + "\\" + mFclName;
                        pCopy.out_data = GDBPath + "\\" + mFclName;
                        Geoprocessor gp = mGP;
                        gp.OverwriteOutput = true;
                        gp.Execute(pCopy, null);

                    }

                }
            }
            /// <summary>
            /// 外部调用程序：栅格缩编 原始
            /// </summary>
            public void LandToRasterAndSimplyfy(bool defaultGDB)
            {
                try
                {
                    WaitOperation wo = mWo;
                    //if (false)
                    {
                        //检查并创建输出文件夹
                        if (Directory.Exists(mExportGDBPath) == true)
                        {
                            if (!defaultGDB)
                            {
                                if (MessageBox.Show("文件已存在，是否覆盖", "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    wo.SetText("正在创建输出文件");
                                    Application.DoEvents();
                                    CreateGDB(mExportGDBPath);
                                }
                            }
                        }
                        else
                        {
                            wo.SetText("正在创建输出文件");
                            Application.DoEvents();
                            CreateGDB(mExportGDBPath);
                        }
                        for (int i = 0; i < mLBGDBPathList.Count; i++)
                        {
                            mStep++; 
                            string GDBPath = mLBGDBPathList[i].ToString();
                            string FileName = System.IO.Path.GetFileNameWithoutExtension(GDBPath);
                            #region//添加TAG整型字段，DLBM复制到TAG，并将字母去掉
                            wo.SetText("正在转换等级，进度:" + (i + 1) + "/" + mLBGDBPathList.Count + ",当前：" + System.IO.Path.GetFileNameWithoutExtension(GDBPath));
                            Application.DoEvents();
                            IFeatureClass dltbFcls = GApplication.Application.QueryFeatureClass((GDBPath), mFclName);
                            GApplication.Application.AddField(dltbFcls, "TAG");
                            int TAGFieldIndex = dltbFcls.FindField("TAG");
                            int DLBMFieldIndex = dltbFcls.FindField(mField);
                            Geoprocessor gp = mGP;
                            gp.OverwriteOutput = true;
                            IFeatureCursor cursor = dltbFcls.Update(null, true);
                            string dlbm;
                            IFeature fea;
                            int numK = 0;
                            while ((fea = cursor.NextFeature()) != null)
                            {
                                dlbm = fea.get_Value(DLBMFieldIndex).ToString();
                                dlbm = dlbm.Replace("H", "7");
                                dlbm = dlbm.Replace("A", "");
                                dlbm = dlbm.Replace("K", "");
                                fea.set_Value(TAGFieldIndex, dlbm);
                                cursor.UpdateFeature(fea);
                                numK++;
                                if (numK % 2000 == 0)
                                {
                                    wo.SetText("正在转换等级：" + numK.ToString());
                                }
                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                            #endregion
                            DLTBToRasterAndSimplify(mLBGDBPathList[i].ToString(), wo);
                        }
                        //栅格合并
                        MosicRaster(wo);

                        //栅格碎片处理
                        DLTBToRasterDispose(wo);
                        //转为矢量，并对字段属性进行处理
                        RasterToFeature(wo);
                    }
                    //矢量碎片简化
                    DLTBFeatureClassSimplify(wo,1);                    
                    //覆盖原始数据
                    if (mOverWrite)
                    {
                        wo.SetText("覆盖原始数据");
                        for (int i = 0; i < mLBGDBPathList.Count; i++)
                        {
                            string GDBPath = mLBGDBPathList[i].ToString();
                            Copy pCopy = new Copy();
                            pCopy.in_data = mExportGDBPath + "\\" + mFclName;
                            pCopy.out_data = GDBPath + "\\" + mFclName;
                            Geoprocessor gp = mGP;
                            gp.OverwriteOutput = true;
                            gp.Execute(pCopy, null);
                             
                        }

                    }
                    wo.SetText("处理完成！");
                     
                }
                catch
                {

                }
            }
            private bool mGPFished = false;
            private AutoResetEvent mGPWorkerEvent = new AutoResetEvent(false);
            private Geoprocessor mGP = new Geoprocessor();
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
                        tr.Elapsed += (o, ee) =>
                        {

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
            //矢量转栅格，并进行索引缩编
            private void DLTBToRasterAndSimplify(string GDBPath, WaitOperation wo)
            {
               
                Application.DoEvents();
                string FileName = System.IO.Path.GetFileNameWithoutExtension(GDBPath);
                Application.DoEvents();
                IFeatureClass dltbFcls = GApplication.Application.QueryFeatureClass((GDBPath), mFclName);
                Geoprocessor gp = mGP;
                try
                {
                    //gp.Execute(pCalculateField, null);
                    wo.SetText("DLTB进行栅格化");
                    ///图斑等级转换完成后，对DLTB进行栅格化
                    ESRI.ArcGIS.ConversionTools.PolygonToRaster pPolygonToRaster = new ESRI.ArcGIS.ConversionTools.PolygonToRaster();
                    pPolygonToRaster.in_features = GDBPath + "\\" + mFclName;
                    pPolygonToRaster.out_rasterdataset = mExportGDBPath + "\\" + "DLTBRaster" + FileName;
                    pPolygonToRaster.value_field = "TAG";
                    //通过比例尺判断初始栅格网格大小，如果比例尺小于15，则初始为2，如果比例尺大于15，则初始为5，如过大于100，初始为10；
                    if (mMapScale < 150000)
                    {
                        pPolygonToRaster.cellsize = "2";
                    }
                    else if (mMapScale >= 150000 && mMapScale < 1000000)
                    {
                        pPolygonToRaster.cellsize = "5";
                    }
                    else if (mMapScale >= 1000000)
                    {
                        pPolygonToRaster.cellsize = "20";
                    }
                   // gp.Execute(pPolygonToRaster, null);

                    //64位GP处理
                    mGPToolsToExecute.Clear();
                    mGPToolsToExecute.Enqueue(pPolygonToRaster);
                    mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                    mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                    mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                    mGPWorkerEvent.WaitOne();

                    ///栅格完成后，开始进行逐级缩编
                    //先通过比例尺计算逐级缩编的变化表
                    var ScalList = CalculateScal();
                    //读取正逆DLBM表；
                    ReadCCCode(GDBPath, wo);
                    Application.DoEvents();           
                    int count= 0;
                    
                    foreach (double NowScal in ScalList)
                    {
                        #region
                        count++;
                        wo.SetText("正在索引缩编，" + "第" + count + "/" + ScalList.Count + "：" + System.IO.Path.GetFileNameWithoutExtension(GDBPath) + "/r/n" + "总进度：" + mStep + "/" + mLBGDBPathList.Count);
                        //重采样
                        ESRI.ArcGIS.DataManagementTools.Resample pResample = new Resample();
                        pResample.in_raster = mExportGDBPath + "\\" + "DLTBRaster" + FileName; ;
                        pResample.out_raster = mExportGDBPath + "\\" + "DLTBSYSB" + FileName; ;
                        pResample.cell_size = NowScal.ToString();
                        pResample.resampling_type = "MAJORITY";
                       // gp.Execute(pResample, null);
                        //正向重分类 正向-----逆向
                        ESRI.ArcGIS.SpatialAnalystTools.Reclassify pReClassify = new Reclassify();
                        pReClassify.in_raster = mExportGDBPath + "\\" + "DLTBSYSB" + FileName; ;
                        pReClassify.reclass_field = "Value";
                        pReClassify.out_raster = mExportGDBPath + "\\" + "DLTBNX" + FileName; ;
                        pReClassify.remap = mZNTable;
                       // gp.Execute(pReClassify, null);
                        //逆向重分类 逆向-----正向
                        ESRI.ArcGIS.SpatialAnalystTools.Reclassify pReClassify2 = new Reclassify();
                        pReClassify2.in_raster = mExportGDBPath + "\\" + "DLTBNX" + FileName; ;
                        pReClassify2.reclass_field = "Value";
                        pReClassify2.out_raster = mExportGDBPath + "\\" + "DLTBRaster" + FileName; ;
                        pReClassify2.remap = mNZTable;
                       // gp.Execute(pReClassify2, null);

                        mGPToolsToExecute.Clear();
                        mGPToolsToExecute.Enqueue(pResample);
                        mGPToolsToExecute.Enqueue(pReClassify);
                        mGPToolsToExecute.Enqueue(pReClassify2);
                        mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                        mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                        mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                        mGPWorkerEvent.WaitOne();
                        #endregion
                    }

                    //处理完成后删除中间数据
                    ESRI.ArcGIS.DataManagementTools.Delete pDelete = new Delete();
                    pDelete.in_data = mExportGDBPath + "\\" + "DLTBNX" + FileName; ;
                    gp.Execute(pDelete, null);
                    ESRI.ArcGIS.DataManagementTools.Delete pDelete2 = new Delete();
                    pDelete2.in_data = mExportGDBPath + "\\" + "DLTBSYSB" + FileName; ;
                    gp.Execute(pDelete2, null);

                }
                catch (System.Exception ex)
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
                    return;
                }
            }
            //栅格拼接
            private void MosicRaster(WaitOperation wo)
            {
                try
                {                   
                    wo.SetText("正在合并数据...");
                    Application.DoEvents();
                    Geoprocessor gp = mGP;
                    gp.OverwriteOutput = true;
                    string RasterFileName = "";
                    for (int i = 0; i < mLBGDBPathList.Count; i++)
                    {
                        RasterFileName = RasterFileName + mExportGDBPath + "\\" + "DLTBRaster" + System.IO.Path.GetFileNameWithoutExtension(mLBGDBPathList[i].ToString()) + ";";
                    }
                    ESRI.ArcGIS.DataManagementTools.MosaicToNewRaster pMosaicToNewRaster = new MosaicToNewRaster();
                    pMosaicToNewRaster.input_rasters = RasterFileName;
                    pMosaicToNewRaster.output_location = mExportGDBPath;
                    pMosaicToNewRaster.raster_dataset_name_with_extension = "DLTB0";
                    pMosaicToNewRaster.pixel_type = "16_BIT_UNSIGNED";
                    pMosaicToNewRaster.number_of_bands = 1;
                    try
                    {

                        //gp.Execute(pMosaicToNewRaster, null);
                        mGPToolsToExecute.Clear();
                        mGPToolsToExecute.Enqueue(pMosaicToNewRaster);
                        mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                        mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                        mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                        mGPWorkerEvent.WaitOne();

                        //合并完成后删除中间各县过程数据
                        for (int i = 0; i < mLBGDBPathList.Count; i++)
                        {
                            ESRI.ArcGIS.DataManagementTools.Delete pDelete = new Delete();
                            pDelete.in_data = mExportGDBPath + "\\" + "DLTBRaster" + System.IO.Path.GetFileNameWithoutExtension(mLBGDBPathList[i].ToString());
                            gp.Execute(pDelete, null);
                        }
                    }
                    catch (System.Exception ex)
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
                catch
                {

                }
            }
            //栅格碎片处理
            private void DLTBToRasterDispose(WaitOperation wo)
            {
                wo.SetText("正在处理碎片......");
                Application.DoEvents();

                Geoprocessor gp = mGP;
                gp.OverwriteOutput = true;
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter2 = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter3 = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter4 = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter5 = new MajorityFilter();
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter6 = new MajorityFilter();


                //循环做4次
                #region
                // for (int i = 0; i < 5; i++)
                {//1
                    pMajorityFilter.number_neighbors = "FOUR";
                    pMajorityFilter.majority_definition = "MAJORITY";
                    pMajorityFilter.in_raster = mExportGDBPath + "\\" + "DLTB0" ;
                    pMajorityFilter.out_raster = mExportGDBPath + "\\" + "DLTB1";
                    //gp.Execute(pMajorityFilter, null);
                }
                //2
                {
                    pMajorityFilter2.number_neighbors = "FOUR";
                    pMajorityFilter2.majority_definition = "MAJORITY";
                    pMajorityFilter2.in_raster = mExportGDBPath + "\\" + "DLTB1" ;
                    pMajorityFilter2.out_raster = mExportGDBPath + "\\" + "DLTB2";
                   // gp.Execute(pMajorityFilter, null);
                }
                //3
                {
                    pMajorityFilter3.number_neighbors = "FOUR";
                    pMajorityFilter3.majority_definition = "MAJORITY";
                    pMajorityFilter3.in_raster = mExportGDBPath + "\\" + "DLTB2" ;
                    pMajorityFilter3.out_raster = mExportGDBPath + "\\" + "DLTB3" ;
                   // gp.Execute(pMajorityFilter, null);
                }
                //4
                {
                    pMajorityFilter4.number_neighbors = "FOUR";
                    pMajorityFilter4.majority_definition = "MAJORITY";
                    pMajorityFilter4.in_raster = mExportGDBPath + "\\" + "DLTB3" ;
                    pMajorityFilter4.out_raster = mExportGDBPath + "\\" + "DLTB4" ;
                    //gp.Execute(pMajorityFilter, null);
                }
                //5
                {
                    pMajorityFilter5.number_neighbors = "FOUR";
                    pMajorityFilter5.majority_definition = "MAJORITY";
                    pMajorityFilter5.in_raster = mExportGDBPath + "\\" + "DLTB4" ;
                    pMajorityFilter5.out_raster = mExportGDBPath + "\\" + "DLTB5" ;
                    //gp.Execute(pMajorityFilter, null);
                }
                //做第六次，把最后的文件名命名为DLTBDispose
                pMajorityFilter6.number_neighbors = "FOUR";
                pMajorityFilter6.majority_definition = "MAJORITY";
                pMajorityFilter6.in_raster = mExportGDBPath + "\\" + "DLTB5";
                pMajorityFilter6.out_raster = mExportGDBPath + "\\" + "DLTBDispose";
                //gp.Execute(pMajorityFilter, null);
                #endregion
                mGPToolsToExecute.Clear();
                mGPToolsToExecute.Enqueue(pMajorityFilter);
                mGPToolsToExecute.Enqueue(pMajorityFilter2);
                mGPToolsToExecute.Enqueue(pMajorityFilter3);
                mGPToolsToExecute.Enqueue(pMajorityFilter4);
                mGPToolsToExecute.Enqueue(pMajorityFilter5);
                mGPToolsToExecute.Enqueue(pMajorityFilter6);
                mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                mGPWorkerEvent.WaitOne();



                //删除过程数据
                ESRI.ArcGIS.DataManagementTools.Delete pDelete = new Delete();
                for (int i = 0; i <= 5; i++)
                {
                    pDelete.in_data = mExportGDBPath + "\\" + "DLTB" + i.ToString();
                    gp.Execute(pDelete, null);
                }
            }
            //创建错误数据库
            private void CreateGDB(string ErrorGDBPath)
            {
                Geoprocessor gp = mGP;
                gp.OverwriteOutput = true;
                ESRI.ArcGIS.DataManagementTools.CreateFileGDB pCreateGDB = new CreateFileGDB();
                pCreateGDB.out_folder_path = System.IO.Path.GetDirectoryName(ErrorGDBPath);
                pCreateGDB.out_name = System.IO.Path.GetFileNameWithoutExtension(ErrorGDBPath);
                try
                {
                    gp.Execute(pCreateGDB, null);
                }
                catch (System.Exception ex)
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
                    return;
                }

            }
            
            //矢量碎片处理
            private void DLTBFeatureClassSimplify(WaitOperation wo,int step,int EliCount=10)
            {
                
                //根据比例尺计算单次简化面积以及简化次数
                //图上要求是2mm，是单个格子的200倍，所以每次碎片处理10倍格子大小，处理20次
                //比例尺
                double Scale = mMapScale;
                //单个格子大小
                double PixArea = Scale * Scale / 10000.0 / 10000.0;
                //单次简化面积,单个格子的10倍
                //double SimplifyArea = PixArea * 10;
                double SimplifyArea = PixArea * 15;

                //简化20次
                Geoprocessor gp = mGP;
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", mExportGDBPath);
                mGP.SetEnvironmentValue("workspace", mExportGDBPath);
                ESRI.ArcGIS.DataManagementTools.Eliminate pElim = new ESRI.ArcGIS.DataManagementTools.Eliminate();

                try
                {                   
                        int i=step;
                        
                        Application.DoEvents();
                        //创建Layer
                        ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer pMakeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
                        if (i == 1)
                        {
                            pMakeFeatureLayer.in_features = "DLTB";
                        }
                        else
                        {
                            pMakeFeatureLayer.in_features = "DLTB_Ele" + (i - 1).ToString();
                        }
                        pMakeFeatureLayer.workspace = mFolderPath;
                        wo.SetText("正在处理DLTB碎面：" + i.ToString() + "/" + EliCount + "，输入:" + pMakeFeatureLayer.in_features);
                        pMakeFeatureLayer.out_layer = "DLTBLayer" + i.ToString();
                        gp.Execute(pMakeFeatureLayer, null);
                        //通过Layer选择
                        ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute pSelectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
                        pSelectLayerByAttribute.in_layer_or_view = "DLTBLayer" + i.ToString();
                        pSelectLayerByAttribute.where_clause = '"' + "Shape_Area" + '"' + "<=" + SimplifyArea * i;
                        wo.SetText("正在处理DLTB碎面：" + i.ToString() + "/" + EliCount + "，输入:" + pMakeFeatureLayer.in_features + ",面积参数：" + SimplifyArea * i);
                        gp.Execute(pSelectLayerByAttribute, null);

                        pElim.in_features = "DLTBLayer" + i.ToString();
                        pElim.out_feature_class = "DLTB_Ele" + i.ToString(); 
                        
                        ESRI.ArcGIS.DataManagementTools.Delete pDelete2 = new Delete();
                        if (i > 1)
                        {
                            pDelete2.in_data = "DLTB_Ele" + (i - 1).ToString(); 
                        }

                        mGPToolsToExecute.Clear();
                        mGPToolsToExecute.Enqueue(pElim);
                        if (i > 1)
                        {
                            mGPToolsToExecute.Enqueue(pDelete2);
                        }
                        mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                        mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                        mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                        mGPWorkerEvent.WaitOne();
                      //最后才进行改名删字段
                        if (step == EliCount)
                        {
                            #region
                            IFeatureClass pDLTBFeatureClass = null;
                            IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                            IWorkspace workspace = wsFactory.OpenFromFile(mExportGDBPath, 0);
                         

                            try
                            {
                                if ((workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclName))
                                {
                                   
                                    pDLTBFeatureClass = (workspace as IFeatureWorkspace).OpenFeatureClass(mFclName);
                                    (pDLTBFeatureClass as IDataset).Delete();
                                   
                                }
                            }
                            catch (Exception ex)
                            {

                            }

                            try
                            {

                                pDLTBFeatureClass = (workspace as IFeatureWorkspace).OpenFeatureClass("DLTB_Ele" + EliCount);

                                if ((pDLTBFeatureClass as IGeoDataset).SpatialReference.Name.ToUpper()=="UNKNOWN")
                                {
                                    IWorkspace inputworkspace = wsFactory.OpenFromFile(mLBGDBPathList[0], 0);
                                    IFeatureClass xzqfcls = (inputworkspace as IFeatureWorkspace).OpenFeatureClass("XZQ");
                                    ISpatialReference spr = (xzqfcls as IGeoDataset).SpatialReference;
                                    DefineProjection gpDp = new DefineProjection();
                                    gpDp.in_dataset = "DLTB_Ele" + EliCount;
                                    gpDp.coor_system = spr;
                                    gp.Execute(gpDp, null);
                                }

                                (pDLTBFeatureClass as IDataset).Rename(mFclName);

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            //处理完成后将DLTB中多余的字段删除掉


                            pDLTBFeatureClass = GApplication.Application.QueryFeatureClass(mExportGDBPath, mFclName);
                            ArrayList pDeleteFieldNameList = new ArrayList();
                            for (int j = 0; j < pDLTBFeatureClass.Fields.FieldCount; j++)
                            {
                                if (pDLTBFeatureClass.Fields.Field[i].Name.ToUpper().Contains("OBJECTID") || pDLTBFeatureClass.Fields.Field[i].Name.ToUpper().Contains("SHAPE") || pDLTBFeatureClass.Fields.Field[i].Name.ToUpper() == "DLBM"
                                    || pDLTBFeatureClass.Fields.Field[i].Name == "Shape_Length" || pDLTBFeatureClass.Fields.Field[i].Name == "Shape_Area")
                                {

                                }
                                else
                                {
                                    pDeleteFieldNameList.Add(pDLTBFeatureClass.Fields.Field[i].Name);
                                }

                            }

                            foreach (string DeleteFieldName in pDeleteFieldNameList)
                            {
                                try
                                {
                                    IField pField = pDLTBFeatureClass.Fields.Field[pDLTBFeatureClass.FindField(DeleteFieldName)];
                                    if (pField.Required == false)
                                    {
                                        pDLTBFeatureClass.DeleteField(pField);
                                    }
                                }
                                catch(Exception)
                                {
                                    wo.SetText(DeleteFieldName + "，该字段删除出错");
                                }
                            }
                            #endregion
                        }
                           
                }
                catch (System.Exception ex)
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
            //栅格重新转为矢量
            private void RasterToFeature(WaitOperation wo)
            {
                wo.SetText( "正在转为DLTB......");
                Application.DoEvents();
                Geoprocessor gp = mGP;
                try
                {

                    IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                    IWorkspace workspace = wsFactory.OpenFromFile(mExportGDBPath, 0);
                    //先删除mExportGDBPath的DLTB


                    if ((workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclName))
                    {
                        var  pDLTBFeatureClass = (workspace as IFeatureWorkspace).OpenFeatureClass(mFclName);
                        (pDLTBFeatureClass as IDataset).Delete();
                    }
                   
                    gp.OverwriteOutput = true;
                    ESRI.ArcGIS.ConversionTools.RasterToPolygon pRasterToPolygon = new RasterToPolygon();
                    pRasterToPolygon.in_raster = mExportGDBPath + "\\" + "DLTBDispose";
                    pRasterToPolygon.out_polygon_features = mExportGDBPath + "\\" + "DLTB";
                    pRasterToPolygon.raster_field = "Value";
                    // gp.Execute(pRasterToPolygon, null);
                    mGPToolsToExecute.Clear();
                    mGPToolsToExecute.Enqueue(pRasterToPolygon);
                    mGP.ToolExecuting += new EventHandler<ToolExecutingEventArgs>(gp_ToolExecuting);
                    mGP.ToolExecuted += new EventHandler<ToolExecutedEventArgs>(gp_ToolExecuted);
                    mGP.ExecuteAsync(mGPToolsToExecute.Dequeue() as IGPProcess);
                    mGPWorkerEvent.WaitOne();

                
                    //处理完成后删除过程数据
                    ESRI.ArcGIS.DataManagementTools.Delete pDelete = new Delete();
                    pDelete.in_data = mExportGDBPath + "\\" + "DLTBDispose";
                    gp.Execute(pDelete, null);

                    //添加DLBM地段并删除其余字段
                    AddCCField(mExportGDBPath, "DLTB",wo);

                    
                    IFeatureClass pFeatureClass = null;
                    pFeatureClass = (workspace as IFeatureWorkspace).OpenFeatureClass(mFclName);
                    ITable pTable = pFeatureClass as ITable;
                    int FieldIndex = pTable.FindField("grid_code");
                    IField pField = pTable.Fields.Field[FieldIndex];
                    if (FieldIndex != -1)
                    {
                        pTable.DeleteField(pField);
                    }
                    FieldIndex = pTable.FindField("Id");
                    pField = pTable.Fields.Field[FieldIndex];
                    if (FieldIndex != -1)
                    {
                        pTable.DeleteField(pField);
                    }

                  
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pTable);
                    GC.Collect();
                }
                catch (System.Exception ex)
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
          
            private void AddCCField(string FilePath, string FileName,WaitOperation wo)
            {
                wo.SetText("正在创建DLBM字段");
              
                Application.DoEvents();
              
                IFeatureClass pFeatureClass = GApplication.Application.QueryFeatureClass(FilePath, FileName);
              
                IField pField = new Field();
                IFieldEdit pFieldEdit = pField as IFieldEdit;
                pFieldEdit.Name_2 = "DLBM";
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFieldEdit.Length_2 = 8;
                pFeatureClass.AddField(pField);
                ITable pTable = pFeatureClass as ITable;
                wo.SetText( "正在赋值DLBM码");
                Application.DoEvents();
                Geoprocessor gp = mGP;
                    gp.OverwriteOutput = true;
                    ESRI.ArcGIS.DataManagementTools.CalculateField pCalculateField = new CalculateField();
                    pCalculateField.in_table = FilePath + "\\" + FileName;
                    pCalculateField.field = "DLBM";
                    pCalculateField.expression = @"[grid_code]";

                    try
                    {
                        gp.Execute(pCalculateField, null);
                        pCalculateField.in_table = FilePath + "\\" + FileName;
                        pCalculateField.field = "DLBM";
                        pCalculateField.expression = @"+[DLBM]";
                        gp.Execute(pCalculateField, null);
                        var cursor = pFeatureClass.Update(null, true);
                        int DLBMIndex = pFeatureClass.Fields.FindField("DLBM");
                        IFeature pFeature;
                        string temp = "";
                        while ((pFeature=cursor.NextFeature()) != null)
                        {
                            temp = pFeature.get_Value(DLBMIndex).ToString().Trim();
                            if (temp == "871" || temp == "872" || temp == "571")
                            {
                                temp = temp.Replace("7", "H");
                                pFeature.set_Value(DLBMIndex, temp);
                                cursor.UpdateFeature(pFeature);
                            }
                            if (temp.Length == 1 || temp.Length == 3)
                            {
                                temp = "0"+temp;
                                pFeature.set_Value(DLBMIndex, temp);
                                cursor.UpdateFeature(pFeature);
                            }
                            if (temp == "000")
                            {
                                cursor.DeleteFeature();
                            }
                            if (temp == "00")
                            {
                                cursor.DeleteFeature();
                            }
                            if (temp == "0")
                            {
                                cursor.DeleteFeature();
                            }
                            if (temp == "")
                            {
                                cursor.DeleteFeature();
                            }
                        }
                        Marshal.ReleaseComObject(cursor);
                    }
                    catch (System.Exception ex)
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
                  
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureClass);
                
            }
         
            private List<double> CalculateScal()
            {
                List<double> ScalList = new List<double>();
                double TergetScal = mMapScale / 10000;
                if (TergetScal <= 15)
                {
                    double Scal = 2;
                    while (Scal < TergetScal)
                    {
                        ScalList.Add(Scal);
                        Scal = Scal + 2;
                    }
                    ScalList.Add(TergetScal);
                }
                else if (TergetScal > 15 && TergetScal <= 30)
                {
                    double Scal = 5;
                    while (Scal < TergetScal)
                    {
                        ScalList.Add(Scal);
                        Scal = Scal + 5;
                    }
                    ScalList.Add(TergetScal);
                }
                else if (TergetScal > 30 && TergetScal < 100)
                {
                    double Scal = 10;
                    ScalList.Add(5.0);
                    while (Scal < TergetScal)
                    {
                        ScalList.Add(Scal);
                        Scal = Scal + 10;
                    }
                    ScalList.Add(TergetScal);
                }
                else if (TergetScal >= 100)
                {
                    double Scal = 20;
                    ScalList.Add(20.0);
                    while (Scal < TergetScal)
                    {
                        ScalList.Add(Scal);
                        Scal = Scal + 20;
                    }
                    ScalList.Add(TergetScal);
                }
                return ScalList;
            }

            //读取TAG字段中的DLBM码唯一值
            private void ReadCCCode(string GDBPath, WaitOperation wo)
            {
                try
                {
                   

                    
                    IFeatureClass pDLTB = null;
                    pDLTB = GApplication.Application.QueryFeatureClass(GDBPath, mFclName);
                    ITable pTable = pDLTB as ITable;
                    IQueryFilter pQueryFilter = new QueryFilter();
                    pQueryFilter.WhereClause = "";
                    int featureCount = pTable.RowCount(pQueryFilter);
                    wo.SetText("正在遍历DLBM码唯一值：" + System.IO.Path.GetFileNameWithoutExtension(GDBPath));
                    var uniqueDLBMList = GApplication.Application.UniqueValue(pDLTB, "TAG");
                    int[] CCCodeList = new int[uniqueDLBMList.Count];
                    int  m = 0;

                    foreach (string CCCode in uniqueDLBMList)
                    {
                        CCCodeList[m] = Convert.ToInt16(CCCode);
                        m++;
                    }

                    int temp = 0;
                    for (int i = 0; i < CCCodeList.Count() - 1; i++)
                    {
                        for (int j = 0; j < CCCodeList.Count() - 1 - i; j++)
                        {
                            if (CCCodeList[j] > CCCodeList[j + 1])
                            {
                                temp = CCCodeList[j + 1];
                                CCCodeList[j + 1] = CCCodeList[j];
                                CCCodeList[j] = temp;
                            }
                        }

                    }
                    //重置正逆表
                    mZNTable = "";
                    mNZTable = "";
                    for (int i = 0; i < uniqueDLBMList.Count; i++)
                    {
                        mZNTable = mZNTable + CCCodeList[i] + " " + CCCodeList[uniqueDLBMList.Count - 1 - i] + ";";
                        mNZTable = mNZTable + CCCodeList[uniqueDLBMList.Count - 1 - i] + " " + CCCodeList[i] + ";";
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pTable);
                  

                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        
        }


    
}

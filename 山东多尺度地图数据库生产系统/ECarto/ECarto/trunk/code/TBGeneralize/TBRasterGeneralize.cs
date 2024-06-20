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

namespace ShellTBDivided
{
        public   class TBRasterGeneralize
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


            private bool mOverWrite = true;
            public TBRasterGeneralize(string export, double mapscale, string folder, List<string> gdbs, bool mOverWrite_ = true)
            {
                mExportGDBPath = export;
                mFolderPath = folder;
                mMapScale = mapscale;
                mLBGDBPathList = gdbs;
                mOverWrite = mOverWrite_;
            }
            /// <summary>
            /// 外部调用程序：栅格缩编
            /// </summary>
            public void LandToRasterAndSimplyfy(bool defaultGDB)
            {
                try
                {
                    WaitOperation wo = mWo;
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
                        wo.SetText("正在转换等级，进度:"+(i+1)+"/"+mLBGDBPathList.Count + ",当前："+System.IO.Path.GetFileNameWithoutExtension(GDBPath));
                        Application.DoEvents();
                        IFeatureClass dltbFcls = GApplication.Application.QueryFeatureClass((GDBPath), mFclName);
                        GApplication.Application.AddField(dltbFcls, "TAG");
                        int TAGFieldIndex = dltbFcls.FindField("TAG");
                        int DLBMFieldIndex = dltbFcls.FindField(mField);
                        Geoprocessor gp = GApplication.Application.GPTool;
                        gp.OverwriteOutput = true;
                        IFeatureCursor cursor = dltbFcls.Update(null, true);
                        string dlbm;
                        IFeature fea;
                        int numK = 0;
                        while((fea=cursor.NextFeature())!=null)
                        {
                            dlbm=fea.get_Value(DLBMFieldIndex).ToString();
                            dlbm=dlbm.Replace("H","7");
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
                    //矢量碎片简化
                    DLTBFeatureClassSimplify(wo);
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
                            Geoprocessor gp = GApplication.Application.GPTool;
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
            //矢量转栅格，并进行索引缩编
            private void DLTBToRasterAndSimplify(string GDBPath, WaitOperation wo)
            {
               
                Application.DoEvents();
                string FileName = System.IO.Path.GetFileNameWithoutExtension(GDBPath);
                Application.DoEvents();
                IFeatureClass dltbFcls = GApplication.Application.QueryFeatureClass((GDBPath), mFclName);
                Geoprocessor gp = GApplication.Application.GPTool;
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
                    gp.Execute(pPolygonToRaster, null);
                    ///栅格完成后，开始进行逐级缩编
                    //先通过比例尺计算逐级缩编的变化表
                    ArrayList ScalList = CalculateScal();
                    //读取正逆DLBM表；
                    ReadCCCode(GDBPath, wo);
                    Application.DoEvents();           
                    int count= 0;

                    foreach (double NowScal in ScalList)
                    {
                        count++;
                        wo.SetText("正在索引缩编，" + "第" + count + "/" + ScalList.Count + "：" + System.IO.Path.GetFileNameWithoutExtension(GDBPath) + "/r/n" + "总进度：" + mStep + "/" + mLBGDBPathList.Count);
                        //重采样
                        ESRI.ArcGIS.DataManagementTools.Resample pResample = new Resample();
                        pResample.in_raster = mExportGDBPath + "\\" + "DLTBRaster" + FileName; ;
                        pResample.out_raster = mExportGDBPath + "\\" + "DLTBSYSB" + FileName; ;
                        pResample.cell_size = NowScal.ToString();
                        pResample.resampling_type = "MAJORITY";
                        gp.Execute(pResample, null);
                        //正向重分类 正向-----逆向
                        ESRI.ArcGIS.SpatialAnalystTools.Reclassify pReClassify = new Reclassify();
                        pReClassify.in_raster = mExportGDBPath + "\\" + "DLTBSYSB" + FileName; ;
                        pReClassify.reclass_field = "Value";
                        pReClassify.out_raster = mExportGDBPath + "\\" + "DLTBNX" + FileName; ;
                        pReClassify.remap = mZNTable;
                        gp.Execute(pReClassify, null);
                        //逆向重分类 逆向-----正向
                        pReClassify = new Reclassify();
                        pReClassify.in_raster = mExportGDBPath + "\\" + "DLTBNX" + FileName; ;
                        pReClassify.reclass_field = "Value";
                        pReClassify.out_raster = mExportGDBPath + "\\" + "DLTBRaster" + FileName; ;
                        pReClassify.remap = mNZTable;
                        gp.Execute(pReClassify, null);
                    }

                    //处理完成后删除中间数据
                    ESRI.ArcGIS.DataManagementTools.Delete pDelete = new Delete();
                    pDelete.in_data = mExportGDBPath + "\\" + "DLTBNX" + FileName; ;
                    gp.Execute(pDelete, null);
                    pDelete.in_data = mExportGDBPath + "\\" + "DLTBSYSB" + FileName; ;
                    gp.Execute(pDelete, null);

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
                    Geoprocessor gp = GApplication.Application.GPTool;
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
                        gp.Execute(pMosaicToNewRaster, null);
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

                Geoprocessor gp = GApplication.Application.GPTool;
                gp.OverwriteOutput = true;
                ESRI.ArcGIS.SpatialAnalystTools.MajorityFilter pMajorityFilter = new MajorityFilter();
                pMajorityFilter.number_neighbors = "FOUR";
                pMajorityFilter.majority_definition = "MAJORITY";

                //循环做4次
                for (int i = 0; i < 5; i++)
                {
                    pMajorityFilter.in_raster = mExportGDBPath + "\\" + "DLTB" + i.ToString();
                    pMajorityFilter.out_raster = mExportGDBPath + "\\" + "DLTB" + (i + 1).ToString();
                    gp.Execute(pMajorityFilter, null);
                }

                //做第六次，把最后的文件名命名为DLTBDispose
                pMajorityFilter.in_raster = mExportGDBPath + "\\" + "DLTB5";
                pMajorityFilter.out_raster = mExportGDBPath + "\\" + "DLTBDispose";
                gp.Execute(pMajorityFilter, null);

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
                Geoprocessor gp = GApplication.Application.GPTool;
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
            private void DLTBFeatureClassSimplify(WaitOperation wo)
            {
                //根据比例尺计算单次简化面积以及简化次数
                //图上要求是2mm，是单个格子的200倍，所以每次碎片处理10倍格子大小，处理20次
                //比例尺
                double Scale = mMapScale;
                //单个格子大小
                double PixArea = Scale * Scale / 10000.0 / 10000.0;
                //单次简化面积,单个格子的10倍
                //double SimplifyArea = PixArea * 10;
                double SimplifyArea = PixArea * 10;

                //简化20次
                Geoprocessor gp = GApplication.Application.GPTool;
                gp.OverwriteOutput = true;
                gp.SetEnvironmentValue("workspace", mExportGDBPath);
                ESRI.ArcGIS.DataManagementTools.Eliminate pElim = new ESRI.ArcGIS.DataManagementTools.Eliminate();

                try
                {
                    //for (int i = 1; i <= 20; i++)
                    for (int i = 1; i <= 20; i++)
                    {
                       wo.SetText( "正在处理DLTB碎面：" + i.ToString() + "/" + 20);
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
                        gp.Execute(pMakeFeatureLayer, null);
                        //通过Layer选择
                        ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute pSelectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
                        pSelectLayerByAttribute.in_layer_or_view = "DLTBLayer" + i.ToString();
                        pSelectLayerByAttribute.where_clause = '"' + "Shape_Area" + '"' + "<=" + SimplifyArea * i;
                        gp.Execute(pSelectLayerByAttribute, null);
                        pElim.in_features = "DLTBLayer" + i.ToString();
                        //输出
                        pElim.out_feature_class = "DLTB_Ele"+i.ToString();
                        gp.Execute(pElim, null);

                        Delete pDelete = new Delete();
                        pDelete.in_data = "DLTBLayer"+i.ToString();
                        try
                        {
                            gp.Execute(pDelete, null);
                        }
                        catch
                        {

                        }
                        if (i > 1)
                        {
                            pDelete.in_data = "DLTB_Ele" + (i - 1).ToString();
                            gp.Execute(pDelete, null);
                        }
                    }
                    IFeatureClass pDLTBFeatureClass = null;
                    Delete pDelete2 = new Delete();
                    pDelete2.in_data = "DLTB";
                    gp.Execute(pDelete2, null);

                    pDLTBFeatureClass = GApplication.Application.QueryFeatureClass(mExportGDBPath, "DLTB_Ele20");

                    (pDLTBFeatureClass as IDataset).Rename("DLTB");


                    //处理完成后将DLTB中多余的字段删除掉
                    

                   
                    pDLTBFeatureClass = GApplication.Application.QueryFeatureClass(mExportGDBPath, mFclName);
                    ArrayList pDeleteFieldNameList = new ArrayList();
                    for (int i = 0; i < pDLTBFeatureClass.Fields.FieldCount; i++)
                    {
                        if (pDLTBFeatureClass.Fields.Field[i].Name == "OBJECTID" || pDLTBFeatureClass.Fields.Field[i].Name == "Shape" || pDLTBFeatureClass.Fields.Field[i].Name == "DLBM"
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
                        IField pField = pDLTBFeatureClass.Fields.Field[pDLTBFeatureClass.FindField(DeleteFieldName)];
                        if (pField.Required == false)
                        {
                            pDLTBFeatureClass.DeleteField(pField);
                        }
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
                Geoprocessor gp = GApplication.Application.GPTool;
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
                pRasterToPolygon.in_raster = mExportGDBPath + "\\" + "DLTBDispose";
                pRasterToPolygon.out_polygon_features = mExportGDBPath + "\\" + "DLTB";
                pRasterToPolygon.raster_field = "Value";
                gp.Execute(pRasterToPolygon, null);

                    //处理完成后删除过程数据
                   
                    pDelete.in_data = mExportGDBPath + "\\" + "DLTBDispose";
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
                Geoprocessor gp = GApplication.Application.GPTool;
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
                            temp = pFeature.get_Value(DLBMIndex).ToString();
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
         
            private ArrayList CalculateScal()
            {
                ArrayList ScalList = new ArrayList();
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

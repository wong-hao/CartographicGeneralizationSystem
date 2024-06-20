using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using System.Xml.Linq;
using System.Data;
using System.Windows.Forms; 
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 境界跳绘: 2018-3-19[修改内容：不足一个跳绘实体段 合并到临近（伪节点）。
    /// </summary>
    public class BoulSkipDrawHelper
    {
        private GApplication _app;//应用程序
        double _lineWidth;//实线长(毫米)
        double _blankWidth;//跳绘线间距长（毫米）
        double _groups;//组数
        double _pointwidth;//点间距
        double _bufferWidth;//容差
        double _inDisbf = -0.5;//河流面内缓冲mm
        Dictionary<string,string> lyrNames = new Dictionary<string,string>();//跳绘图层
        Dictionary<string, BoulDrawInfo> BoulDrawDic = new Dictionary<string, BoulDrawInfo>();
        Geoprocessor gp = null;
        public bool ProvinceSkip = true;
        public bool StateSkip = true;
        public bool CountySkip = true;
        public bool TownSkip = true;
        public BoulSkipDrawHelper(GApplication app, Dictionary<string, BoulDrawInfo> BoulDrawDic_, double dis, Dictionary<string, string> lyrs)
        {
            _app = app;
            BoulDrawDic = BoulDrawDic_;
            _bufferWidth = dis;
            lyrNames = lyrs;
        }

        public   DataTable _dtLayerRule = null;
        public string boulProduce(WaitOperation wo = null)
        {
            string msg = "";
            //  临时提出的境界
            IFeatureClass tempProvinceBOULFC=null;
            IFeatureClass tempStateBOULFC = null;
            IFeatureClass tempCountyBOULFC = null;
            IFeatureClass tempTownBOULFC = null;
            //合并好的完整境界及跳绘图层
            IFeatureClass provinceBoulMergeFC = null;
            IFeatureClass stateBoulMergeFC = null; 
            IFeatureClass countyBoulMergeFC = null; 
            IFeatureClass townBoulMergeFC = null; 
            IFeatureClass hydlMergeFC = null; 

            //境界缓冲图层
            IFeatureClass provinceBoulBuffer = null;
            IFeatureClass stateBoulBuffer = null; 
            IFeatureClass countyBoulBuffer = null;
            IFeatureClass townBoulBuffer = null;
            //河流面
            IFeatureClass hydaDis = null;
            IFeatureClass hydaBuffer = null;
            IFeatureClass hydaFcl = null;
            //求交的过程数据
            IFeatureClass provinceIntersHYDL = null; 
            IFeatureClass stateIntersHYDL = null; 
            IFeatureClass countyIntersHYDL = null; 
            IFeatureClass townIntersHYDL = null;

            IFeatureClass provinceHYDLBF = null; ;
            IFeatureClass stateHYDLBF = null; ;
            IFeatureClass countyHYDLBF = null; ;
            IFeatureClass townHYDLBF = null; ;

            IFeatureClass provinceIntersFC = null; ;
            IFeatureClass stateIntersFC = null; ;
            IFeatureClass countyIntersFC = null; ;
            IFeatureClass townIntersFC = null; ;

            IFeatureClass provinceIntersFC1 = null;
            IFeatureClass stateIntersFC1 = null; 
            IFeatureClass countyIntersFC1 = null;
            IFeatureClass townIntersFC1 = null; 

            //求异的过程数据
            IFeatureClass provinceDiffFC = null; ;
            IFeatureClass stateDiffFC = null; ;
            IFeatureClass countyDiffFC = null; ;
            IFeatureClass townDiffFC = null; ;

            //多部件转换过程数据
            IFeatureClass provinceDiffMTSFC = null; ;
            IFeatureClass stateDiffMTSFC = null; ;
            IFeatureClass countyDiffMTSFC = null; ;
            IFeatureClass townDiffMTSFC = null; ;
            IWorkspace ws = _app.Workspace.EsriWorkspace;
            Intersect intersTool = new Intersect();
            Dissolve diss = new Dissolve();
            SymDiff diff = new SymDiff();
            Erase erasegp = new Erase();
            try
            {
              
           
                var view = _app.MapControl.ActiveView;
                var map = view.FocusMap;

                gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                gp.AddOutputsToMap = false;
                gp.SetEnvironmentValue("workspace", _app.Workspace.EsriWorkspace.PathName);

            

                double lineWidth = _lineWidth * _app.Workspace.Map.ReferenceScale / 1000;
                double blankWith = _blankWidth * _app.Workspace.Map.ReferenceScale / 1000;

                IFeatureClass thFCJJ = (ws as IFeatureWorkspace).OpenFeatureClass("JJTH");
                IFeatureClass boulFC = (ws as IFeatureWorkspace).OpenFeatureClass("BOUL");
                IFeatureClass hydlFC = null;
                if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "HYDA"))
                {
                    hydaFcl = (ws as IFeatureWorkspace).OpenFeatureClass("HYDA");
                   
                }
                if (thFCJJ == null || boulFC == null)
                {
                    return "图层 BOUL或JJTH不存在！";
                }
                if (wo != null)
                    wo.SetText("正在清空要素……");
                
                #region
                IFeatureCursor fCursor = thFCJJ.Update(null, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    fCursor.DeleteFeature();
                }
                #endregion
                Marshal.ReleaseComObject(fCursor);
                if (wo != null)
                    wo.SetText("正在分析……");
                //每个行政级别的GB码
                var provinceGB = BoulDrawDic["Province"].GBList;
                var StateGB = BoulDrawDic["State"].GBList;
                var CountyGB = BoulDrawDic["County"].GBList;
                var TownGB = BoulDrawDic["Town"].GBList;
                #region 导出省、市、县、镇境界数据
                IFeatureCursor tempProvinceCursor = null;
                if (ProvinceSkip)
                {
                    ExportLayer(ws, boulFC, "TEMP_Province");
                    tempProvinceBOULFC = (ws as IFeatureWorkspace).OpenFeatureClass("TEMP_Province");
                    tempProvinceCursor = tempProvinceBOULFC.Insert(true);
                }
                IFeatureCursor tempStateCursor = null;
                if (StateSkip)
                {
                    ExportLayer(ws, boulFC, "TEMP_State");
                    tempStateBOULFC = (ws as IFeatureWorkspace).OpenFeatureClass("TEMP_State");
                    tempStateCursor = tempStateBOULFC.Insert(true);
                }
                IFeatureCursor tempCountyCursor = null;
                if (CountySkip)
                {
                    ExportLayer(ws, boulFC, "TEMP_County");
                    tempCountyBOULFC = (ws as IFeatureWorkspace).OpenFeatureClass("TEMP_County");
                    tempCountyCursor = tempCountyBOULFC.Insert(true);
                }
                IFeatureCursor tempTownCursor = null;
                if (TownSkip)
                {
                    ExportLayer(ws, boulFC, "TEMP_Town");
                    tempTownBOULFC = (ws as IFeatureWorkspace).OpenFeatureClass("TEMP_Town");
                    tempTownCursor = tempTownBOULFC.Insert(true);
                }
                int gbIndex = boulFC.FindField("gb");
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("RuleID <> 1");//境界不显示要素，不参与跳绘。
                IFeatureCursor boulCursor = boulFC.Search(qf, false);
                IFeature boulFeature = null;
                IFeatureCursor tempJJTHCursor = thFCJJ.Insert(true);



                while ((boulFeature = boulCursor.NextFeature()) != null)
                {
                    int gbValue = Convert.ToInt32(boulFeature.get_Value(gbIndex));
                    if (provinceGB.Contains(gbValue) && ProvinceSkip)
                    {
                        IFeatureBuffer feaBuf = tempProvinceBOULFC.CreateFeatureBuffer();
                        feaBuf.Shape = boulFeature.Shape;
                        if (feaBuf.Fields.FindField("gb") != -1)
                            feaBuf.set_Value(feaBuf.Fields.FindField("gb"), gbValue);
                        tempProvinceCursor.InsertFeature(feaBuf);
                    }
                    else if (StateGB.Contains(gbValue) && StateSkip)
                    {
                        IFeatureBuffer feaBuf = tempStateBOULFC.CreateFeatureBuffer();
                        feaBuf.Shape = boulFeature.Shape;
                        if (feaBuf.Fields.FindField("gb") != -1)
                            feaBuf.set_Value(feaBuf.Fields.FindField("gb"), gbValue);
                        tempStateCursor.InsertFeature(feaBuf);
                    }
                    else if (CountyGB.Contains(gbValue) && CountySkip)
                    {
                        IFeatureBuffer feaBuf = tempCountyBOULFC.CreateFeatureBuffer();
                        feaBuf.Shape = boulFeature.Shape;
                        if (feaBuf.Fields.FindField("gb") != -1)
                            feaBuf.set_Value(feaBuf.Fields.FindField("gb"), gbValue);
                        tempCountyCursor.InsertFeature(feaBuf);
                    }
                    else if (TownGB.Contains(gbValue) && TownSkip)
                    {
                        IFeatureBuffer feaBuf = tempTownBOULFC.CreateFeatureBuffer();
                        feaBuf.Shape = boulFeature.Shape;
                        if (feaBuf.Fields.FindField("gb") != -1)
                            feaBuf.set_Value(feaBuf.Fields.FindField("gb"), gbValue);
                        tempTownCursor.InsertFeature(feaBuf);
                    }
                    else
                    {    //添加其余未参与跳绘的要素
                        IFeatureBuffer feaBuf = thFCJJ.CreateFeatureBuffer();
                        feaBuf.Shape = boulFeature.Shape;
                        if (feaBuf.Fields.FindField("gb") != -1)
                            feaBuf.set_Value(feaBuf.Fields.FindField("gb"), gbValue);
                        tempJJTHCursor.InsertFeature(feaBuf);
                    }
                     
                    Marshal.ReleaseComObject(boulFeature);
                }
                tempJJTHCursor.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(tempJJTHCursor);
                if (TownSkip)
                {
                    tempTownCursor.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(tempTownCursor);
                }
                if (CountySkip)
                {
                    tempCountyCursor.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(tempCountyCursor);
                }
                if (StateSkip)
                {
                    tempStateCursor.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(tempStateCursor);
                }
                if (ProvinceSkip)
                {
                    tempProvinceCursor.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(tempProvinceCursor);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(boulCursor);
                #endregion
                #region GP合并【1、跳绘制图层合并; 2、将省、市等要素分别全部合并为一个整体省界、市界....】
                //省界合并
                if (ProvinceSkip)
                {
                    if (wo != null)
                        wo.SetText("正在合并省界......");
                    diss.in_features = tempProvinceBOULFC;
                    diss.out_feature_class = ws.PathName + "\\ProvinceBOULMerge";
                    diss.dissolve_field = "GB";
                    SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                    provinceBoulMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("ProvinceBOULMerge");
                }
                //市界合并
                if (StateSkip)
                {
                    if (wo != null)
                        wo.SetText("正在合并市界......");
                    diss.in_features = tempStateBOULFC;
                    diss.out_feature_class = ws.PathName + "\\StateBOULMerge";
                    diss.dissolve_field = "GB";
                    SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                    stateBoulMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("StateBOULMerge");
                }
                //县界合并
                if (CountySkip)
                {
                    if (wo != null)
                        wo.SetText("正在合并县界......");
                    diss.in_features = tempCountyBOULFC;
                    diss.out_feature_class = ws.PathName + "\\CountyBOULMerge";
                    diss.dissolve_field = "GB";
                    SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                    countyBoulMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("CountyBOULMerge");
                }
                //乡界合并
                if (TownSkip)
                {
                    if (wo != null)
                        wo.SetText("正在合并乡界......");
                    diss.in_features = tempTownBOULFC;
                    diss.out_feature_class = ws.PathName + "\\TownBOULMerge";
                    diss.dissolve_field = "GB";
                    SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                    townBoulMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("TownBOULMerge");
                }

                #endregion
               
               
                #region 缓冲跳绘双线河和境界图层 默认 0.1mm，设置为0，则不缓冲
                if (wo != null)
                    wo.SetText("正在处理双线河......");
                if (hydaFcl != null)//河流面内缓冲
                {
                    //过滤要素
                    hydaBuffer = HydaInterset(boulFC, ws);
                 
                }
                if (_bufferWidth > 0)
                {
                    //省
                    if (ProvinceSkip)
                    {
                        var gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                        {
                            in_features = ws.PathName + "\\" + provinceBoulMergeFC.AliasName,
                            out_feature_class = ws.PathName + "\\" + provinceBoulMergeFC.AliasName + "Buffer",
                            buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                        };
                        SMGI.Common.Helper.ExecuteGPTool(gp, gpBuffer, null);
                        provinceBoulBuffer = (ws as IFeatureWorkspace).OpenFeatureClass("" + provinceBoulMergeFC.AliasName + "Buffer");
                    }
                    //市州
                    if (StateSkip)
                    {
                        var gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                        {
                            in_features = ws.PathName + "\\" + stateBoulMergeFC.AliasName,
                            out_feature_class = ws.PathName + "\\" + stateBoulMergeFC.AliasName + "Buffer",
                            buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                        };
                        SMGI.Common.Helper.ExecuteGPTool(gp, gpBuffer, null);
                        stateBoulBuffer = (ws as IFeatureWorkspace).OpenFeatureClass("" + stateBoulMergeFC.AliasName + "Buffer");
                    }
                    if (CountySkip)
                    {    //区县
                        var  gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                        {
                            in_features = ws.PathName + "\\" + countyBoulMergeFC.AliasName,
                            out_feature_class = ws.PathName + "\\" + countyBoulMergeFC.AliasName + "Buffer",
                            buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                        };
                        SMGI.Common.Helper.ExecuteGPTool(gp, gpBuffer, null);
                        countyBoulBuffer = (ws as IFeatureWorkspace).OpenFeatureClass("" + countyBoulMergeFC.AliasName + "Buffer");
                    }
                    if (TownSkip)
                    {  
                        //乡镇
                       var  gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                        {
                            in_features = ws.PathName + "\\" + townBoulMergeFC.AliasName,
                            out_feature_class = ws.PathName + "\\" + townBoulMergeFC.AliasName + "Buffer",
                            buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                        };
                       SMGI.Common.Helper.ExecuteGPTool(gp, gpBuffer, null);
                        townBoulBuffer = (ws as IFeatureWorkspace).OpenFeatureClass("" + townBoulMergeFC.AliasName + "Buffer");
                    }
                }
                else//不缓冲
                {
                    if (TownSkip)
                        townBoulBuffer = townBoulMergeFC;
                    if (CountySkip)
                        countyBoulBuffer = countyBoulMergeFC;
                    if (StateSkip)
                        stateBoulBuffer = stateBoulMergeFC;
                    if (ProvinceSkip)
                        provinceBoulBuffer = provinceBoulMergeFC;
                }
             
                #endregion
               
                #region GP求交
                //【省界】求交
                // 步骤：1.境界缓冲与水系等相交处理，得到相交的水系
                // 步骤：2.相交水系缓冲再与合并的境界求交
                // 步骤：3.排除在双线河内的境界（通过将双线河做内缓冲与结果求异）
                // 解决 小比例水系 缓冲 报错！
                IFeature fe = null;
                if (ProvinceSkip)
                {
                    if (wo != null)
                        wo.SetText("正在合并与【省界】相交单线河......");

                    IFeatureClass    hydlDisFC=   HydlInterset(provinceBoulBuffer, ws);
                    string hydlInterName = hydlDisFC.AliasName;
                    if (wo != null)
                        wo.SetText("正在求交【省界】......");



                    intersTool.in_features = IntersectParms(provinceBoulBuffer, hydlDisFC);
                        //ws.PathName + "\\" + provinceBoulBuffer.AliasName + ";" + ws.PathName + "\\" + hydlInterName;
                    intersTool.out_feature_class = ws.PathName + "\\ProvinceIntersHYDL";
                    intersTool.output_type = "LINE";
                    SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                    provinceIntersHYDL = (ws as IFeatureWorkspace).OpenFeatureClass("ProvinceIntersHYDL");
                    provinceHYDLBF = provinceIntersHYDL;
                    if (_bufferWidth > 0)
                    {
                        var gpbuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                        {
                            in_features = ws.PathName + "\\" + provinceIntersHYDL.AliasName,
                            out_feature_class = ws.PathName + "\\" + "ProvinceHYDLBF",
                            buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                        };
                        SMGI.Common.Helper.ExecuteGPTool(gp, gpbuffer, null);
                        provinceHYDLBF = (ws as IFeatureWorkspace).OpenFeatureClass("ProvinceHYDLBF");
                    }

                    //交
                    intersTool.in_features = IntersectParms(provinceHYDLBF, provinceBoulMergeFC);
                        //ws.PathName + "\\" + provinceHYDLBF.AliasName + ";" + ws.PathName + "\\" + provinceBoulMergeFC.AliasName;
                    intersTool.out_feature_class = ws.PathName + "\\ProvinceIntersResult1";
                    intersTool.output_type = "LINE";
                    SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                    //排除双线河内的河流
                    provinceIntersFC1 = (ws as IFeatureWorkspace).OpenFeatureClass("ProvinceIntersResult1");
                    provinceIntersFC = provinceIntersFC1;
                    fe = provinceIntersFC1.Search(null, false).NextFeature();
                    if (fe != null && hydaBuffer != null)
                    {
                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.Geometry = fe.ShapeCopy;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        if (hydaBuffer.FeatureCount(sf) > 0)
                        {
                            erasegp.in_features = ws.PathName + "\\ProvinceIntersResult1";
                            erasegp.erase_features = ws.PathName + "\\" + hydaBuffer.AliasName;
                            erasegp.out_feature_class = ws.PathName + "\\ProvinceIntersResult";
                            SMGI.Common.Helper.ExecuteGPTool(gp, erasegp, null);
                            provinceIntersFC = (ws as IFeatureWorkspace).OpenFeatureClass("ProvinceIntersResult");
                        }
                    }
                    (hydlDisFC as IDataset).Delete();
                }
                if (StateSkip)
                {
                    if (wo != null)
                        wo.SetText("正在合并与【市界】相交单线河......");

                    IFeatureClass hydlDisFC = HydlInterset(stateBoulBuffer, ws);
                    string hydlInterName = hydlDisFC.AliasName;
                    //【市界】求交
                    if (wo != null)
                        wo.SetText("正在求交【市界】......");
                    intersTool.in_features = IntersectParms(stateBoulBuffer, hydlDisFC);
                        //ws.PathName + "\\" + stateBoulBuffer.AliasName + ";" + ws.PathName + "\\" + hydlInterName;
                    intersTool.out_feature_class = ws.PathName + "\\StateIntersHYDL";
                    intersTool.output_type = "LINE";
                    SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                    stateIntersHYDL = (ws as IFeatureWorkspace).OpenFeatureClass("StateIntersHYDL");
                    stateHYDLBF = stateIntersHYDL;
                    if (_bufferWidth > 0)
                    {
                        var gpbuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                          {
                              in_features = ws.PathName + "\\" + stateIntersHYDL.AliasName,
                              out_feature_class = ws.PathName + "\\" + "StateHYDLBF",
                              buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                          };
                        SMGI.Common.Helper.ExecuteGPTool(gp, gpbuffer, null);
                        stateHYDLBF = (ws as IFeatureWorkspace).OpenFeatureClass("StateHYDLBF");
                    }
                    intersTool.in_features = IntersectParms(stateHYDLBF, stateBoulMergeFC);
                        //ws.PathName + "\\" + stateHYDLBF.AliasName + ";" + ws.PathName + "\\" + stateBoulMergeFC.AliasName;
                    intersTool.out_feature_class = ws.PathName + "\\StateIntersResult1";
                    intersTool.output_type = "LINE";
                    SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                    //排除双线河内的河流
                    stateIntersFC1 = (ws as IFeatureWorkspace).OpenFeatureClass("StateIntersResult1");
                    stateIntersFC = stateIntersFC1;
                    fe = stateIntersFC1.Search(null, false).NextFeature();
                    if (fe != null && hydaBuffer != null)
                    {
                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.Geometry = fe.ShapeCopy;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        if (hydaBuffer.FeatureCount(sf) > 0)
                        {
                            erasegp.in_features = ws.PathName + "\\StateIntersResult1";
                            erasegp.erase_features = ws.PathName + "\\" + hydaBuffer.AliasName;
                            erasegp.out_feature_class = ws.PathName + "\\StateIntersResult";
                            SMGI.Common.Helper.ExecuteGPTool(gp, erasegp, null);
                            stateIntersFC = (ws as IFeatureWorkspace).OpenFeatureClass("StateIntersResult");
                        }
                    }
                    (hydlDisFC as IDataset).Delete();

                }
                if (CountySkip)
                {

                    if (wo != null)
                        wo.SetText("正在合并与【县界】相交单线河......");

                    IFeatureClass hydlDisFC = HydlInterset(countyBoulBuffer, ws);
                    string hydlInterName = hydlDisFC.AliasName;
                    //【县界】求交
                    if (wo != null)
                        wo.SetText("正在求交【县界】......");
                    intersTool.in_features = IntersectParms(countyBoulBuffer, hydlDisFC);
                        //ws.PathName + "\\" + countyBoulBuffer.AliasName + ";" + ws.PathName + "\\" + hydlInterName;
                    intersTool.out_feature_class = ws.PathName + "\\CountyIntersHYDL";
                    intersTool.output_type = "LINE";
                    SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                    countyIntersHYDL = (ws as IFeatureWorkspace).OpenFeatureClass("CountyIntersHYDL");
                    countyHYDLBF = countyIntersHYDL;
                    if (_bufferWidth > 0)
                    {
                        var gpbuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                         {
                             in_features = ws.PathName + "\\" + countyIntersHYDL.AliasName,
                             out_feature_class = ws.PathName + "\\" + "CountyHYDLBF",
                             buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                         };
                        SMGI.Common.Helper.ExecuteGPTool(gp, gpbuffer, null);
                        countyHYDLBF = (ws as IFeatureWorkspace).OpenFeatureClass("CountyHYDLBF");
                    }
                    intersTool.in_features = IntersectParms(countyHYDLBF, countyBoulMergeFC);
                        //ws.PathName + "\\" + countyHYDLBF.AliasName + ";" + ws.PathName + "\\" + countyBoulMergeFC.AliasName;
                    intersTool.out_feature_class = ws.PathName + "\\CountyIntersResult1";
                    intersTool.output_type = "LINE";
                    SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                    //排除双线河内的河流
                    countyIntersFC1 = (ws as IFeatureWorkspace).OpenFeatureClass("CountyIntersResult1");
                    countyIntersFC = countyIntersFC1;
                    fe = countyIntersFC1.Search(null, false).NextFeature();
                    if (fe != null && hydaBuffer != null)
                    {
                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.Geometry = fe.ShapeCopy;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        if (hydaBuffer.FeatureCount(sf) > 0)
                        {
                            erasegp.in_features = ws.PathName + "\\CountyIntersResult1";
                            erasegp.erase_features = ws.PathName + "\\" + hydaBuffer.AliasName;
                            erasegp.out_feature_class = ws.PathName + "\\CountyIntersResult";
                            SMGI.Common.Helper.ExecuteGPTool(gp, erasegp, null);
                            countyIntersFC = (ws as IFeatureWorkspace).OpenFeatureClass("CountyIntersResult");
                        }
                    }
                    (hydlDisFC as IDataset).Delete();
                }
                if (TownSkip)
                {
                    if (wo != null)
                        wo.SetText("正在合并与【乡界】相交单线河......");

                    IFeatureClass hydlDisFC = HydlInterset(townBoulBuffer, ws);
                    string hydlInterName = hydlDisFC.AliasName;
                    //【乡界】求交
                    if (wo != null)
                        wo.SetText("正在求交【乡界】......");
                    intersTool.in_features = IntersectParms(townBoulBuffer, hydlDisFC);
                        //ws.PathName + "\\" + townBoulBuffer.AliasName + ";" + ws.PathName + "\\" + hydlInterName;
                    intersTool.out_feature_class = ws.PathName + "\\TownIntersHYDL";
                    intersTool.output_type = "LINE";
                    SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                    townIntersHYDL = (ws as IFeatureWorkspace).OpenFeatureClass("TownIntersHYDL");
                    townHYDLBF = townIntersHYDL;
                    if (_bufferWidth > 0)
                    {
                        var gpbuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                        {
                            in_features = ws.PathName + "\\" + townIntersHYDL.AliasName,
                            out_feature_class = ws.PathName + "\\" + "TownHYDLBF",
                            buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                        };
                        SMGI.Common.Helper.ExecuteGPTool(gp, gpbuffer, null);
                        townHYDLBF = (ws as IFeatureWorkspace).OpenFeatureClass("TownHYDLBF");
                    }
                    intersTool.in_features = IntersectParms(townHYDLBF, townBoulMergeFC);
                        //ws.PathName + "\\" + townHYDLBF.AliasName + ";" + ws.PathName + "\\" + townBoulMergeFC.AliasName;
                    intersTool.out_feature_class = ws.PathName + "\\TownIntersResult1";
                    intersTool.output_type = "LINE";
                    SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                    //排除双线河内的河流
                    townIntersFC1 = (ws as IFeatureWorkspace).OpenFeatureClass("TownIntersResult1");
                    townIntersFC = townIntersFC1;
                    fe = townIntersFC1.Search(null, false).NextFeature();
                    if (fe != null && hydaBuffer != null)
                    {
                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.Geometry = fe.ShapeCopy;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        if (hydaBuffer.FeatureCount(sf) > 0)
                        {
                            erasegp.in_features = ws.PathName + "\\TownIntersResult1";
                            erasegp.erase_features = ws.PathName + "\\" + hydaBuffer.AliasName;
                            erasegp.out_feature_class = ws.PathName + "\\TownIntersResult";
                            SMGI.Common.Helper.ExecuteGPTool(gp, erasegp, null);
                            townIntersFC = (ws as IFeatureWorkspace).OpenFeatureClass("TownIntersResult");
                        }
                    }
                    (hydlDisFC as IDataset).Delete();
                }
                #endregion


                Dictionary<int, List<IPolyline>> provinceGeos = new Dictionary<int, List<IPolyline>>();
                Dictionary<int, List<IPolyline>> stateGeos = new Dictionary<int, List<IPolyline>>();
                Dictionary<int, List<IPolyline>> countyGeos = new Dictionary<int, List<IPolyline>>();
                Dictionary<int, List<IPolyline>> townGeos = new Dictionary<int, List<IPolyline>>();
                #region GP求异，并将求异结果直接导出JJTH图层
                //【省界】求异
                if (wo != null)
                    wo.SetText("正在求异【省界】......");
                if (ProvinceSkip)
                {
                    diff.in_features = ws.PathName + "\\ProvinceBOULMerge";
                    diff.update_features = ws.PathName + "\\" + provinceIntersFC.AliasName;
                    diff.out_feature_class = ws.PathName + "\\省界";
                    SMGI.Common.Helper.ExecuteGPTool(gp, diff, null);
                    SMGI.Common.Helper.ExecuteGPTool(gp, new MultipartToSinglepart("省界", "省界MTS"), null);
                    provinceDiffMTSFC = (ws as IFeatureWorkspace).OpenFeatureClass("省界MTS");
                    provinceGeos = RecordResult(thFCJJ, provinceDiffMTSFC);//63020100
                    provinceDiffFC = (ws as IFeatureWorkspace).OpenFeatureClass("省界");
                }
                //【市界】求异
                if (wo != null)
                    wo.SetText("正在求异【市界】......");
                if (StateSkip)
                {
                    diff.in_features = ws.PathName + "\\StateBOULMerge";
                    diff.update_features = ws.PathName + "\\" + stateIntersFC.AliasName;
                    diff.out_feature_class = ws.PathName + "\\市界";
                    SMGI.Common.Helper.ExecuteGPTool(gp, diff, null);
                    SMGI.Common.Helper.ExecuteGPTool(gp, new MultipartToSinglepart("市界", "市界MTS"), null);
                    stateDiffMTSFC = (ws as IFeatureWorkspace).OpenFeatureClass("市界MTS");
                    stateGeos = RecordResult(thFCJJ, stateDiffMTSFC);// 64020100
                    stateDiffFC = (ws as IFeatureWorkspace).OpenFeatureClass("市界");
                }
                //【县界】求异
                if (wo != null)
                    wo.SetText("正在求异【县界】......");
                diff.in_features = ws.PathName + "\\CountyBOULMerge";
                if (CountySkip)
                {
                    diff.update_features = ws.PathName + "\\" + countyIntersFC.AliasName;
                    diff.out_feature_class = ws.PathName + "\\县界";
                    SMGI.Common.Helper.ExecuteGPTool(gp, diff, null);
                    SMGI.Common.Helper.ExecuteGPTool(gp, new MultipartToSinglepart("县界", "县界MTS"), null);
                    countyDiffMTSFC = (ws as IFeatureWorkspace).OpenFeatureClass("县界MTS");
                    countyGeos = RecordResult(thFCJJ, countyDiffMTSFC);// 65020100
                    countyDiffFC = (ws as IFeatureWorkspace).OpenFeatureClass("县界");
                }
                //【乡界】求异
                if (wo != null)
                    wo.SetText("正在求异【乡界】......");
                if (TownSkip)
                {
                    diff.in_features = ws.PathName + "\\TownBOULMerge";
                    diff.update_features = ws.PathName + "\\" + townIntersFC.AliasName; ;
                    diff.out_feature_class = ws.PathName + "\\乡界";
                    SMGI.Common.Helper.ExecuteGPTool(gp, diff, null);
                    SMGI.Common.Helper.ExecuteGPTool(gp, new MultipartToSinglepart("乡界", "乡界MTS"), null);
                    townDiffMTSFC = (ws as IFeatureWorkspace).OpenFeatureClass("乡界MTS");
                    townGeos = RecordResult(thFCJJ, townDiffMTSFC);//66020100
                    townDiffFC = (ws as IFeatureWorkspace).OpenFeatureClass("乡界");
                }
                #endregion   

                List<int> thFeOIDList = new List<int>();
                //记录【省界跳绘结果】
                if (wo != null)
                    wo.SetText("正在记录【省界跳绘结果】......");
                if (ProvinceSkip)
                {
                    List<int> oidList = RecordResultEx(thFCJJ, BoulDrawDic["Province"], provinceIntersFC, provinceGeos);
                    thFeOIDList.AddRange(oidList);
                  //合并非跳会几何,【伪节点处理】
                   RecordBOUL(ws,provinceGeos, thFCJJ);
                }
                //记录【市界跳绘结果】
                if (wo != null)
                    wo.SetText("正在记录【市界跳绘结果】......");
                if (StateSkip)
                {
                    List<int> oidList = RecordResultEx(thFCJJ, BoulDrawDic["State"], stateIntersFC, stateGeos);
                    thFeOIDList.AddRange(oidList);
                    RecordBOUL(ws, stateGeos, thFCJJ);
                }
                //记录【县界跳绘结果】
                if (wo != null)
                    wo.SetText("正在记录【县界跳绘结果】......");
                if (CountySkip)
                {
                    List<int> oidList = RecordResultEx(thFCJJ, BoulDrawDic["County"], countyIntersFC, countyGeos);
                    thFeOIDList.AddRange(oidList);
                    RecordBOUL(ws, countyGeos, thFCJJ);
                }
                //记录【乡界跳绘结果】
                if (wo != null)
                    wo.SetText("正在记录【乡界跳绘结果】......");
                if (TownSkip)
                {
                    List<int> oidList = RecordResultEx(thFCJJ, BoulDrawDic["Town"], townIntersFC, townGeos);
                    thFeOIDList.AddRange(oidList);
                    RecordBOUL(ws, townGeos, thFCJJ);
                    if (wo != null)
                        wo.SetText("正在【清理过程数据】......");
                }
               
                
              
                //匹配规则
                if (wo != null)
                    wo.SetText("正在匹配规则......");
                MatchRules(thFCJJ);

                #region 更新几何效果(解决跳绘后的几何效果中部分存在微短实线的效果)
                IRepresentationClass rpc = GetFCRepClass(thFCJJ.AliasName);
                //修正制图表达几何效果
                if (rpc != null)
                {
                    foreach (var fid in thFeOIDList)
                    {
                        IFeature thFe = thFCJJ.GetFeature(fid);
                        IRepresentation rep = GetRepByFeature(rpc, thFe);
                        if (rep != null)
                        {
                            var rule = RuleValueSet(rep);
                            for (int i = 0; i < rule.LayerCount; i++)
                            {
                                IBasicSymbol bs = rule.get_Layer(i);
                                if (bs is IBasicLineSymbol)
                                {
                                    //获取当前几何图形
                                    var allgeos = new List<IGeometry> { rep.Shape };
                                    //全局几何效果
                                    allgeos = GetGeometrysByGeometricEffects((IGeometricEffects)rule, allgeos);
                                    //当前几何效果
                                    var geos = GetGeometrysByGeometricEffects((IGeometricEffects)bs, allgeos);
                                    if (geos.Count > 1)
                                    {
                                        double extractLen = -1;
                                        if ((geos[geos.Count - 1] as IPolyline).Length < (geos[0] as IPolyline).Length * 0.5)
                                        {
                                            extractLen = (geos[geos.Count - 1] as IPolyline).Length;
                                        }
                                        while(extractLen >0)
                                        {
                                            IPolyline shpGeo = thFe.Shape as IPolyline;
                                            ICurve subPolyline = new PolylineClass();
                                            //缩短
                                            shpGeo.GetSubcurve(extractLen * 0.5, shpGeo.Length - extractLen, false, out subPolyline);
                                            thFe.Shape = subPolyline;
                                            thFe.Store();

                                            extractLen = -1;
                                            //获取当前几何图形
                                            allgeos = new List<IGeometry> { rep.Shape };
                                            //全局几何效果
                                            allgeos = GetGeometrysByGeometricEffects((IGeometricEffects)rule, allgeos);
                                            //当前几何效果
                                            geos = GetGeometrysByGeometricEffects((IGeometricEffects)bs, allgeos);
                                            if ((geos[geos.Count - 1] as IPolyline).Length < (geos[0] as IPolyline).Length * 0.5)
                                            {
                                                extractLen = (geos[geos.Count - 1] as IPolyline).Length;
                                            }
                                        }
                                    }

                                }
                            }

                        }
                    }
                }
                #endregion

                //刷新
                view.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                msg = ex.Message;
            }
            finally
            {
                #region 清理过程数据
                //临时 提取的境界
              if (tempProvinceBOULFC != null)
                (tempProvinceBOULFC as IDataset).Delete();
              if (tempStateBOULFC != null)
                (tempStateBOULFC as IDataset).Delete();
              if (tempCountyBOULFC != null)
                (tempCountyBOULFC as IDataset).Delete();
              if (tempTownBOULFC != null)
                (tempTownBOULFC as IDataset).Delete();
                //合并好的完整境界
              if (provinceBoulMergeFC != null)
                (provinceBoulMergeFC as IDataset).Delete();
              if (stateBoulMergeFC != null)
                (stateBoulMergeFC as IDataset).Delete();
              if (countyBoulMergeFC != null)
                  (countyBoulMergeFC as IDataset).Delete();
              if (townBoulMergeFC != null)
                  (townBoulMergeFC as IDataset).Delete();
              if (hydlMergeFC != null)
                  (hydlMergeFC as IDataset).Delete();
                 //缓冲图层
              if (_bufferWidth > 0)
              {
                    if (provinceBoulBuffer != null)
                        (provinceBoulBuffer as IDataset).Delete();
                    if (stateBoulBuffer != null)
                        (stateBoulBuffer as IDataset).Delete();
                    if (countyBoulBuffer != null)
                        (countyBoulBuffer as IDataset).Delete();
                    if (townBoulBuffer != null)
                        (townBoulBuffer as IDataset).Delete();
                    if (provinceHYDLBF != null)
                        (provinceHYDLBF as IDataset).Delete();
                    if (stateHYDLBF != null)
                        (stateHYDLBF as IDataset).Delete();
                    if (countyHYDLBF != null)
                        (countyHYDLBF as IDataset).Delete();
                    if (townHYDLBF != null)
                        (townHYDLBF as IDataset).Delete();
                }
              if (hydaBuffer != null)
              {
                  (hydaBuffer as IDataset).Delete();
              }
              //if (hydaFcl.AliasName == "HydaMerge")
              //{
              //    (hydaFcl as IDataset).Delete();
              //}
                //求交的过程数据
              if (provinceIntersFC != null)
                  (provinceIntersFC as IDataset).Delete();
              if (stateIntersFC != null)
                  (stateIntersFC as IDataset).Delete();
              if (countyIntersFC != null)
                  (countyIntersFC as IDataset).Delete();
              if (townIntersFC != null)
                  (townIntersFC as IDataset).Delete();
              //求交的过程数据
              if (provinceIntersFC1 != null)
              {
                  if(provinceIntersFC1.AliasName!=provinceIntersFC.AliasName)
                     (provinceIntersFC1 as IDataset).Delete();
              }
              if (stateIntersFC1 != null)
              {
                  if (stateIntersFC1.AliasName != stateIntersFC.AliasName)
                      (stateIntersFC1 as IDataset).Delete();
              }
              if (countyIntersFC1 != null)
              {
                  if (countyIntersFC1.AliasName != countyIntersFC.AliasName)
                  (countyIntersFC1 as IDataset).Delete();
              }
              if (townIntersFC1 != null)
              {
                  if (townIntersFC.AliasName != townIntersFC1.AliasName)
                  (townIntersFC1 as IDataset).Delete();
              }
            

              if (provinceIntersHYDL != null)
                  (provinceIntersHYDL as IDataset).Delete();
              if (stateIntersHYDL != null)
                  (stateIntersHYDL as IDataset).Delete();
              if (countyIntersHYDL != null)
                  (countyIntersHYDL as IDataset).Delete();
              if (townIntersHYDL != null)
                  (townIntersHYDL as IDataset).Delete();


                //求异的过程数据
              if (provinceDiffFC != null)
                (provinceDiffFC as IDataset).Delete();
              if (stateDiffFC != null)
                (stateDiffFC as IDataset).Delete();
              if (countyDiffFC != null)
                (countyDiffFC as IDataset).Delete();
              if (townDiffFC != null)
                (townDiffFC as IDataset).Delete();
                //多部件转换过程数据
              if (provinceDiffMTSFC != null)
                (provinceDiffMTSFC as IDataset).Delete();
              if (stateDiffMTSFC != null)
                (stateDiffMTSFC as IDataset).Delete();
              if (countyDiffMTSFC != null)
                (countyDiffMTSFC as IDataset).Delete();
              if (townDiffMTSFC != null)
                (townDiffMTSFC as IDataset).Delete();
                #endregion
              if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "HydaMTS"))
              {
                  var ds = (ws as IFeatureWorkspace).OpenFeatureClass("HydaMTS") as IDataset;
                  ds.Delete();
              }
              //关闭境界图层
              var boulLyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
              {
                  return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("BOUL");
              }));
              foreach (var l in boulLyrs)
              {
                  l.Visible = false;
              }
              //开启跳会图层
              var jjthLyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
              {
                  return (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("JJTH");
              }));
              foreach (var l in jjthLyrs)
              {
                  l.Visible = true;
              }
              GApplication.Application.TOCControl.Update();
              GApplication.Application.TOCControl.Refresh();
              GApplication.Application.Workspace.Save();
            }
         
          
           
            GC.Collect();
            return msg;
        }


        private IGpValueTableObject IntersectParms(IFeatureClass fcl1, IFeatureClass fcl2)
        {
           
            IGpValueTableObject valTbl = new GpValueTableObjectClass();
            valTbl.SetColumns(2);
            object o1 = fcl1;//输入 IFeatureClass 1 
            object o2 = fcl2;//输入 IFeatureClass 2 
            valTbl.AddRow(ref o1);
            valTbl.AddRow(ref o2);
            //object row = "";
            //object rank = 1;
            //row = inputFeatClass;
            // valTbl.SetRow(0, ref row);
            //valTbl.SetValue(0, 1, ref rank);

            //row = clipFeatClass;
            //valTbl.SetRow(1, ref row);
            //rank = 2;
            //valTbl.SetValue(1, 1, ref rank);
            return valTbl;
        }
        //处理与境界相关的河流
        /// <summary>
        /// 
        /// </summary>
        /// <param name="boul">筛选的境界图层</param>
        /// <param name="wo"></param>
        /// <param name="ws"></param>
        private IFeatureClass HydlInterset(IFeatureClass boul,IWorkspace ws)
        {
            IFeatureClass hydlMergeFC = null;//融合的水系
            string lyrName = lyrNames.First().Key;
            try
            {
               
                Dissolve diss = new Dissolve();
                if (lyrNames.Count > 1)//多个
                {
                    #region
                    string appendfcl = ws.PathName + "\\" + lyrNames.First().Key + "_Dis";
                    string inputs = "";
                    int lyrFlag = 0;
                    for (int i = 0; i < lyrNames.Count; i++)
                    {
                        string infeatures = lyrNames.Keys.ToArray()[i];

                        #region
                        MakeFeatureLayer gpLayer = new MakeFeatureLayer { in_features = infeatures, out_layer = infeatures + "_Layer" };
                        string sql = "RuleID <>1";
                        if (lyrNames[infeatures] != "")
                        {
                            sql += " and " + lyrNames[infeatures];
                        }
                        gpLayer.where_clause = sql;
                        SMGI.Common.Helper.ExecuteGPTool(gp, gpLayer, null);
                        //与境界相交
                        SelectLayerByLocation pSeLoc = new SelectLayerByLocation();
                        pSeLoc.in_layer = infeatures + "_Layer";
                        pSeLoc.select_features = boul.AliasName;
                        pSeLoc.overlap_type = "INTERSECT";
                        var geoRe = SMGI.Common.Helper.ExecuteGPTool(gp, pSeLoc, null);
                        var gpUtils = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
                        if (geoRe.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                        {
                            ESRI.ArcGIS.Carto.IFeatureLayer seLayer = gpUtils.DecodeLayer(geoRe.GetOutput(0)) as ESRI.ArcGIS.Carto.IFeatureLayer;
                            diss.in_features = seLayer;
                            diss.out_feature_class = ws.PathName + "\\" + infeatures + "_Dis";
                            SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);

                            if (lyrFlag > 0)
                                inputs += ws.PathName + "\\" + infeatures + "_Dis" + ";";
                            lyrFlag++;
                        }
                        #endregion



                    }
                    inputs = inputs.Substring(0, inputs.Length - 1);
                    Append gpAppend = new Append();
                    gpAppend.inputs = inputs;
                    gpAppend.target = appendfcl;
                    SMGI.Common.Helper.ExecuteGPTool(gp, gpAppend, null);

                    hydlMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass(lyrNames.First().Key + "_Dis");
                    //第一个图层保留,删除其余图层。
                    for (int i = 1; i < lyrNames.Count; i++)
                    {
                        IFeatureClass tempfcl = (ws as IFeatureWorkspace).OpenFeatureClass(lyrNames.Keys.ToArray()[i] + "_Dis");
                        (tempfcl as IDataset).Delete();
                    }
                    #endregion
                }
                else//一个
                {

                    #region
                    MakeFeatureLayer gpLayer = new MakeFeatureLayer { in_features = lyrName, out_layer = lyrName + "_Layer" };
                    string sql = "RuleID <>1";
                    if (lyrNames[lyrName] != "")
                    {
                        sql += " and " + lyrNames[lyrName];
                    }
                    gpLayer.where_clause = sql;
                    SMGI.Common.Helper.ExecuteGPTool(gp, gpLayer, null);

                    //与境界相交
                    SelectLayerByLocation pSeLoc = new SelectLayerByLocation();
                    pSeLoc.in_layer = lyrName + "_Layer";
                    pSeLoc.select_features = boul.AliasName;
                    pSeLoc.overlap_type = "INTERSECT";
                    var geoRe = SMGI.Common.Helper.ExecuteGPTool(gp, pSeLoc, null);
                    var gpUtils = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
                    if (geoRe.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                    {
                        ESRI.ArcGIS.Carto.IFeatureLayer hydlLayer = gpUtils.DecodeLayer(geoRe.GetOutput(0)) as ESRI.ArcGIS.Carto.IFeatureLayer;
                        diss.in_features = hydlLayer;
                        diss.out_feature_class = ws.PathName + "\\" + lyrName + "Merge";
                        SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                        hydlMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("" + lyrName + "Merge");
                    }
                    #endregion
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("合并于境界相交水系出错："+ex.Message);
                return null;
            }
            return hydlMergeFC;
        }

        private IFeatureClass HydaInterset(IFeatureClass boul, IWorkspace ws)
        {
            IFeatureClass hydaMergeFC = null;//融合的水系
          
            try
            {

                Dissolve diss = new Dissolve();
                string lyrName = "HYDA";
                {

                    #region
                    MakeFeatureLayer gpLayer = new MakeFeatureLayer { in_features = lyrName, out_layer = lyrName + "_Layer" };
                    string sql = "RuleID <>1";
                    if (System.IO.File.Exists(_app.Template.Root + @"\专家库\境界跳绘\双线河参数.xml"))
                    {
                        string fileName = _app.Template.Root + @"\专家库\境界跳绘\双线河参数.xml";
                        XDocument doc = XDocument.Load(fileName);
                        var content = doc.Element("Expertise").Element("Content");
                        foreach (var item in content.Elements("Item"))
                        {
                            string ruleID = item.Attribute("RuleID").Value;

                            sql += " and RuleID <> " + ruleID;
                        }

                    }
                    gpLayer.where_clause = sql;
                    SMGI.Common.Helper.ExecuteGPTool(gp, gpLayer, null);

                    //与境界相交
                    SelectLayerByLocation pSeLoc = new SelectLayerByLocation();
                    pSeLoc.in_layer = lyrName + "_Layer";
                    pSeLoc.select_features = boul.AliasName;
                    pSeLoc.overlap_type = "INTERSECT";
                    var geoRe = SMGI.Common.Helper.ExecuteGPTool(gp, pSeLoc, null);
                    var gpUtils = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
                    if (geoRe.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                    {
                        ESRI.ArcGIS.Carto.IFeatureLayer hydlLayer = gpUtils.DecodeLayer(geoRe.GetOutput(0)) as ESRI.ArcGIS.Carto.IFeatureLayer;
                        diss.in_features = hydlLayer;
                        diss.out_feature_class = ws.PathName + "\\" + lyrName + "Merge";
                        SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                        hydaMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("" + lyrName + "Merge");
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("合并于境界相交水系出错：" + ex.Message);
                return null;
            }
            return hydaMergeFC;
        }
        //导出图层
        static void ExportLayer(IWorkspace ws, IFeatureClass fc, string FeatureClassName, IQueryFilter qf = null)
        {
            IFeatureClassLoad pload = null;
            IFeatureClass featureClass = null;
            IFeatureCursor writeCursor = null;
            string name = FeatureClassName;
            var idx = name.LastIndexOf('.');
            if (idx != -1)
            {
                name = name.Substring(idx + 1);
            }
            else
            {
                name = name.Replace('.', '_');
            }
            try
            {
                IFields expFields;
                expFields = (fc.Fields as IClone).Clone() as IFields;
                for (int i = 0; i < expFields.FieldCount; i++)
                {
                    IField field = expFields.get_Field(i);
                    IFieldEdit fieldEdit = field as IFieldEdit;
                    fieldEdit.Name_2 = field.Name.ToUpper();
                }
                featureClass = CreateFeatureClass(ws, name, expFields);

                pload = featureClass as IFeatureClassLoad;
                if (pload != null)
                    pload.LoadOnlyMode = true;

                if (featureClass != null)
                {
                    (featureClass as IFeatureClassManage).UpdateExtent();
                }
            }
            catch
            {

            }
            finally
            {
                if (writeCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);
                }
                if (pload != null)
                {
                    pload.LoadOnlyMode = false;
                }
            }
        }

        //创建要素类
        static public IFeatureClass CreateFeatureClass(IWorkspace ws, string name, IFields org_fields)
        {
            IObjectClassDescription featureDescription = new FeatureClassDescriptionClass();

            IFieldsEdit fields = featureDescription.RequiredFields as IFieldsEdit;


            for (int i = 0; i < org_fields.FieldCount; i++)
            {
                IField field = org_fields.get_Field(i);
                if (!(field as IFieldEdit).Editable)
                {
                    continue;
                }
                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    (fields as IFieldsEdit).set_Field(fields.FindFieldByAliasName((featureDescription as IFeatureClassDescription).ShapeFieldName),
                        (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField);
                    continue;
                }
                if (fields.FindField(field.Name) >= 0)
                {
                    continue;
                }
                IField field_new = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (fields as IFieldsEdit).AddField(field_new);
            }

            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            System.String strShapeField = string.Empty;

            return fws.CreateFeatureClass(name, fields,
                  featureDescription.InstanceCLSID, featureDescription.ClassExtensionCLSID,
                  esriFeatureType.esriFTSimple,
                  (featureDescription as IFeatureClassDescription).ShapeFieldName,
                  string.Empty);

        }


        private Dictionary<int, List<IPolyline>> RecordResult(IFeatureClass expIntersFC, IFeatureClass intersFC)
        {
            Dictionary<int, List<IPolyline>> resultGeos = new Dictionary<int, List<IPolyline>>();
          //  int nCount = 0;
           // int gbIndex = expIntersFC.FindField("GB");
           // IFeatureCursor thCursor = expIntersFC.Insert(true);

            int gbIndex = intersFC.FindField("GB");

            IFeatureCursor intersCursor = intersFC.Search(null, true);
            IFeature intersFeature = null;
            //IFeatureBuffer newFea = expIntersFC.CreateFeatureBuffer();
            while ((intersFeature = intersCursor.NextFeature()) != null)
            {
                if (intersFeature.Shape.IsEmpty)
                {
                    continue;
                }
                if (intersFeature.Shape == null)
                {
                    continue;
                }

                int gb = 0;
                if (gbIndex != -1)
                {
                    int.TryParse(intersFeature.get_Value(gbIndex).ToString(), out gb); 
                }
                if (!resultGeos.ContainsKey(gb))
                {
                    List<IPolyline> plList = new List<IPolyline>();
                    plList.Add(intersFeature.ShapeCopy as IPolyline);

                    resultGeos.Add(gb, plList);
                }
                else
                {
                    resultGeos[gb].Add(intersFeature.ShapeCopy as IPolyline);
                }
                //double len = (intersFeature.Shape as IPolyline).Length;
                ////长度大于图上0.5毫米，才保留
                //if (len > 0.5e-3 * _app.ActiveView.FocusMap.ReferenceScale)
                //{
                //    newFea.Shape = intersFeature.Shape;
                //    newFea.set_Value(gbIndex, gbValue);
                //    thCursor.InsertFeature(newFea);
                //}
                //nCount++;
                //Marshal.ReleaseComObject(intersFeature);
            }
           
           // thCursor.Flush();
           //Marshal.ReleaseComObject(newFea);
           //  Marshal.ReleaseComObject(thCursor);
            Marshal.ReleaseComObject(intersCursor);

            return resultGeos;
        }

      
        //跳会算法修改2018-1.25:跳绘平滑
        private List<int> RecordResultEx(IFeatureClass expJJTHfcl, BoulDrawInfo info, IFeatureClass intersFC, Dictionary<int, List<IPolyline>> resultGeos)
        {
            List<int> feOIDList = new List<int>();

            int gbIndex = expJJTHfcl.FindField("GB");
            int nCount = 0;
            IFeatureCursor thCursor = expJJTHfcl.Insert(true);

            IFeatureCursor intersCursor = intersFC.Search(null, true);
            IFeature intersFeature = null;
            IFeatureBuffer newFea = expJJTHfcl.CreateFeatureBuffer();
            while ((intersFeature = intersCursor.NextFeature()) != null)
            {
                if (intersFeature.Shape.IsEmpty)
                {
                    continue;
                }

                int inGB = 0;//原始要素GB
                if (intersFC.FindField("GB") != -1)
                {
                    int.TryParse(intersFeature.get_Value(intersFC.FindField("GB")).ToString(), out inGB);
                }
                int gbValue = inGB *100;//境界跳绘后的GB(默认境界跳绘的GB是原boul要素中的GB值的100的基础上加10或11，如boul要素的gb为640201，则境界跳绘中该要素左跳为64020110，右跳为64020111)
                if (gbValue != 0)
                    gbValue += 10;

                IGeometryCollection geoCol = intersFeature.Shape as IGeometryCollection;
                //if (geoCol.GeometryCount > 1)//相交结果为一个多部件
                {
                    #region
                    for (int geoItem = 0; geoItem < geoCol.GeometryCount; geoItem++)
                    {
                        PolylineClass pl = new PolylineClass();
                        IPath pa = geoCol.get_Geometry(geoItem) as IPath;
                        (pl as IGeometryCollection).AddGeometry(pa);

                        double geoLength = pl.Length;

                        int offset = 0;
                        double lineWidth = info.SolidValue * _app.ActiveView.FocusMap.ReferenceScale * 1.0e-3;//实线长
                        double blankWidth = info.BlankValue * _app.ActiveView.FocusMap.ReferenceScale * 1.0e-3;//跳绘线间距长
                        double groups = info.SymbolGroup;//组数
                        double pointwidth = info.PointStep * _app.ActiveView.FocusMap.ReferenceScale * 1.0e-3;//点间距

                        //处理参数
                        double blankEndsValue = 0;//左右留白间距
                        
                        //一组线段长度:包括多个
                        double solidvalue = lineWidth * groups - pointwidth / 2;
                        
                        #region
                        if (geoLength > 0)
                        {
                            int solidnum = 0;
                            //1.小于一个实步
                            if (geoLength < solidvalue)
                            {
                                pl.Simplify();
                                if (!resultGeos.ContainsKey(inGB))
                                {
                                    List<IPolyline> plList = new List<IPolyline>();
                                    plList.Add(pl);

                                    resultGeos.Add(inGB, plList);
                                }
                                else
                                {
                                    resultGeos[inGB].Add(pl);
                                }
                                continue;
                                //不处理
                            }
                            //2.介于1个实步和两个实步
                            else if (geoLength < 2 * solidvalue && geoLength > solidvalue)
                            {
                                blankEndsValue = (geoLength - solidvalue) / 2;
                                blankWidth = 0;
                                solidnum = 1;
                            }
                            //3.介于两个实步和两个实步加虚步
                            else if (geoLength < 2 * solidvalue + blankWidth && geoLength > 2 * solidvalue)
                            {
                                blankEndsValue = (geoLength - 2 * solidvalue) / 3;
                                blankWidth = blankEndsValue;
                                solidnum = 2;
                            }
                            //4.大于两个实步加虚步
                            else if (geoLength > 2 * solidvalue + blankWidth)
                            {

                                double leftlen = geoLength - 2 * solidvalue - blankWidth;
                                //4.0 减去部分/实+虚
                                int times = (int)(leftlen / (solidvalue + blankWidth));
                                solidnum = times + 2;
                                //4.1 减去部分%/实+虚
                                double mod = leftlen % (solidvalue + blankWidth);
                                //4.1.1剩余部分小于一个实步
                                if (mod < solidvalue)
                                {
                                    blankEndsValue = mod / 2;
                                }
                                //4.2.2剩余部分介于一个实步和实步加虚步
                                else if (mod > solidvalue && mod < solidvalue + blankWidth)
                                {
                                    solidnum = 2 + times + 1;
                                    blankEndsValue = (mod - solidvalue) / 2;
                                    blankWidth = (geoLength - blankEndsValue * 2 - solidvalue) / (solidnum - 1);//增加一个实步，收缩虚步距离

                                }

                            }
                            ICurve subPolyline = new PolylineClass();
                            //切去线两头留白部分
                            pl.GetSubcurve(blankEndsValue, pl.Length - blankEndsValue, false, out subPolyline);
                            for (int i = 0; i < solidnum; i++)
                            {
                                //获取实部线
                                ICurve solidPlyline = new PolylineClass();
                                (subPolyline as IPolyline).GetSubcurve(i * (solidvalue + blankWidth), i * (solidvalue + blankWidth) + solidvalue, false, out solidPlyline);
                                newFea.Shape = solidPlyline as IGeometry;
                                newFea.Shape = solidPlyline as IGeometry;
                                newFea.set_Value(gbIndex, gbValue + (offset++));
                                if (offset == 2)
                                {
                                    offset = 0;
                                }
                               
                                object fid = thCursor.InsertFeature(newFea);
                                feOIDList.Add((int)fid);
                            }


                        }
                        #endregion
                        
                    }
                    #endregion

                }
                Marshal.ReleaseComObject(intersFeature);
            }
            thCursor.Flush();
            Marshal.ReleaseComObject(newFea);
            Marshal.ReleaseComObject(thCursor);
            Marshal.ReleaseComObject(intersCursor);

            return feOIDList;
        }
        //插入要素:非跳绘段
        private void RecordBOUL(IWorkspace ws, Dictionary<int, List<IPolyline>> geoLines, IFeatureClass fcl)
        {

            string fclName = "TempBoulDrawing";
            ExportLayer(ws, fcl, "TempBoulDrawing");

            IFeatureClass tempBOULFCl = (ws as IFeatureWorkspace).OpenFeatureClass(fclName);
            IFeatureCursor tempTownCursor = tempBOULFCl.Insert(true);
            IFeatureBuffer tempnewFea = tempBOULFCl.CreateFeatureBuffer();
            foreach (var item in geoLines)
            {
                foreach (var line in item.Value)
                {
                    tempnewFea.Shape = line as IGeometry;

                    if (tempBOULFCl.FindField("GB") != -1)
                    {
                        tempnewFea.set_Value(tempBOULFCl.FindField("GB"), item.Key);
                    }

                    tempTownCursor.InsertFeature(tempnewFea);
                }
            }
            tempTownCursor.Flush();
            Marshal.ReleaseComObject(tempTownCursor);
            Dissolve gpDis = new Dissolve();
            gpDis.in_features =  fclName;
            gpDis.out_feature_class = fclName+"Dis";
            gpDis.multi_part = "SINGLE_PART";
            gpDis.unsplit_lines = "UNSPLIT_LINES";
            gpDis.dissolve_field = "GB";
            SMGI.Common.Helper.ExecuteGPTool(gp, gpDis, null);

            IFeatureClass boulfclDis = (ws as IFeatureWorkspace).OpenFeatureClass(fclName + "Dis");
            IFeatureCursor fcursor = boulfclDis.Search(null, false);
            IFeature fe=null;
            IFeatureCursor thCursor = fcl.Insert(true);
            IFeatureBuffer newFea = fcl.CreateFeatureBuffer();
            int gbIndex = fcl.FindField("GB");
            while((fe=fcursor.NextFeature())!=null)
            {
                int gb = 0;
                if (boulfclDis.FindField("GB") != -1)
                {
                    int.TryParse(fe.get_Value(boulfclDis.FindField("GB")).ToString(), out gb);
                }
                int gbValue = gb * 100;//在境界跳绘图层的GB值

                IGeometry geo = fe.ShapeCopy;
                double len = (geo as IPolyline).Length;
                //长度大于图上0.5毫米，才保留
                if (len > 0.5e-3 * _app.ActiveView.FocusMap.ReferenceScale)
                {
                    newFea.Shape = geo;
                    newFea.set_Value(gbIndex, gbValue);
                    thCursor.InsertFeature(newFea);
                    Marshal.ReleaseComObject(geo);
                }
            }
            thCursor.Flush();
            Marshal.ReleaseComObject(fcursor);
            Marshal.ReleaseComObject(newFea);
            Marshal.ReleaseComObject(thCursor);

            (tempBOULFCl as IDataset).Delete();
            (boulfclDis as IDataset).Delete();
        }

        
        //匹配规则
        private void MatchRules(IFeatureClass thFC)
        {
            if (_dtLayerRule == null)
            {
                string ruleMatchFileName = EnvironmentSettings.getLayerRuleDBFileName(GApplication.Application);
                _dtLayerRule = ReadToDataTable(ruleMatchFileName, "图层对照规则");
            }
            if (_dtLayerRule.Rows.Count == 0)
            {
                return;
            }

            DataRow[] drArray = _dtLayerRule.Select().Where(i => i["图层"].ToString() == (thFC as IDataset).Name).ToArray();
            if (drArray.Length != 0)
            {
                int idIndex = thFC.FindField("RuleID");
                if (idIndex != -1)
                {
                    for (int i = 0; i < drArray.Length; i++)
                    {
                        object ruleID = drArray[i]["RuleID"];
                        object whereClause = drArray[i]["定义查询"];
                        
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = whereClause.ToString();

                        IFeatureCursor fCursor = thFC.Update(qf, true);
                        IFeature f = null;
                        while ((f = fCursor.NextFeature()) != null)
                        {
                            f.set_Value(idIndex, ruleID);
                            fCursor.UpdateFeature(f);
                            Marshal.ReleaseComObject(f);
                        }
                        Marshal.ReleaseComObject(fCursor);
                    }
                }
            }
        }
        private  DataTable ReadToDataTable(string mdbFilePath, string tableName)
        {
            DataTable pDataTable = new DataTable();
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
                Marshal.ReleaseComObject(pDataset);
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
                    Marshal.ReleaseComObject(pRow);
                    pRow = pCursor.NextRow();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
            }
            return pDataTable;
        }

        private IRepresentationClass GetFCRepClass(string fcName)
        {
            IMap map = _app.ActiveView as IMap;
            var mc = new MapContextClass();
            mc.InitFromDisplay((map as IActiveView).ScreenDisplay.DisplayTransformation);

            ILayer repLayer = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName.ToUpper()) &&
                ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == _app.Workspace.EsriWorkspace.PathName).FirstOrDefault();
            if (repLayer == null)
                return null;

            var rr = (repLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            if (rr == null)
                return null;

            var rpc = rr.RepresentationClass;

            return rpc;
        }

        private IRepresentation GetRepByFeature(IRepresentationClass rpc, IFeature feature)
        {
            if (rpc == null)
                return null;

            IMap Map = _app.ActiveView as IMap;
            var mc = new MapContextClass();
            mc.InitFromDisplay((Map as IActiveView).ScreenDisplay.DisplayTransformation);

            var r = rpc.GetRepresentation(feature, mc);
            return r;
        }

        public static void RuleValueSet(IGraphicAttributes ga, IGraphicAttributes newga, IRepresentation rep)
        {
            if (ga == null || newga == null || rep == null)
            {
                return;
            }
            for (int k = 0; k < ga.GraphicAttributeCount; k++)
            {
                try
                {
                    var id = ga.get_ID(k);
                    object obj = rep.get_Value(ga, id);
                    newga.set_Value(id, obj);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    //说明某些属性覆盖出了问题,此时删除属性覆盖
                    var or = rep as IOverride;
                    if (or.HasAttributeOverride)
                    {
                        or.RemoveOverrides();
                    }
                }
            }
        }

        public void RuleValueSet(IGeometricEffects ges, IGeometricEffects newges, IRepresentation rep)
        {
            if (ges == null || newges == null || rep == null)
            {
                return;
            }
            for (int j = 0; j < ges.Count; j++)
            {
                var ge = ges.Element[j];
                var newge = newges.Element[j];
                var ga = ge as IGraphicAttributes;
                var newga = newge as IGraphicAttributes;
                RuleValueSet(ga, newga, rep);
            }
        }

        public IRepresentationRule RuleValueSet(IRepresentation rep)
        {
            var r = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            var newrule = (r as IClone).Clone() as IRepresentationRule;
            RuleValueSet(r as IGeometricEffects, newrule as IGeometricEffects, rep);
            for (int i = 0; i < newrule.LayerCount; i++)
            {
                var layer = r.Layer[i];
                var newlayer = newrule.Layer[i];
                RuleValueSet(layer as IGeometricEffects, newlayer as IGeometricEffects, rep);
                RuleValueSet(layer as IGraphicAttributes, newlayer as IGraphicAttributes, rep);
                if (layer is IBasicMarkerSymbol)
                {
                    RuleValueSet((layer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes,
                        (newlayer as IBasicMarkerSymbol).MarkerPlacement as IGraphicAttributes, rep);
                }
                else if (layer is IBasicLineSymbol)
                {
                    RuleValueSet((layer as IBasicLineSymbol).Stroke as IGraphicAttributes,
                        (newlayer as IBasicLineSymbol).Stroke as IGraphicAttributes, rep);
                }
                else if (layer is IBasicFillSymbol)
                {
                    RuleValueSet((layer as IBasicFillSymbol).FillPattern as IGraphicAttributes,
                       (newlayer as IBasicFillSymbol).FillPattern as IGraphicAttributes, rep);
                }
            }
            return newrule;
        }

        //根据几何效果获取几何
        private List<IGeometry> GetGeometrysByGeometricEffects(IGeometricEffects geoEffects, List<IGeometry> geos)
        {
            try
            {
                IMap map = _app.ActiveView as IMap;
                IMapContext mc = new MapContextClass();
                //mc.InitFromDisplay((m_curMap as IActiveView).ScreenDisplay.DisplayTransformation);//初始化有问题，参考比例不对
                mc.Init(map.SpatialReference, map.ReferenceScale, (map as IActiveView).FullExtent);

                for (var i = 0; i < geoEffects.Count; i++)
                {
                    var geoEff = geoEffects.Element[i];

                    var geoc = new List<IGeometry>();
                    foreach (var geo in geos)
                    {
                        if (true == geo.IsEmpty)
                            continue;

                        var geo1 = mc.FromGeographyToMap(geo);

                        geoEff.Reset(geo1);
                        while (true)
                        {
                            var ngeo = geoEff.NextGeometry();
                            if (ngeo == null) break;
                            var ogeo = (IGeometry)((IClone)ngeo).Clone();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(ngeo);
                            geoc.Add(mc.FromMapToGeography(ogeo));
                        }
                    }
                    geos = geoc;

                }
                return geos;
            }
            catch (Exception ex)
            {
                return new List<IGeometry>();
            }

        }
  
    }
     
}


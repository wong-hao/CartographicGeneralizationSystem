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
namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 境界跳绘:修改版本2017-7-21
    /// </summary>
    public class BoulAutoProduceHelper
    {
        private GApplication _app;//应用程序
        double _lineWidth;//实线长(毫米)
        double _blankWidth;//跳绘线间距长（毫米）
        double _groups;//组数
        double _pointwidth;//点间距
        double _bufferWidth;//容差
        double _inDisbf = -0.5;//河流面内缓冲mm
        List<string> lyrNames = new List<string>();//跳绘图层
        Dictionary<string, BoulDrawInfo> BoulDrawDic = new Dictionary<string, BoulDrawInfo>();
        public BoulAutoProduceHelper(GApplication app, Dictionary<string, BoulDrawInfo> BoulDrawDic_,  double dis, List<string> lyrs)
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
            try
            {
              
           
                var view = _app.MapControl.ActiveView;
                var map = view.FocusMap;

                Geoprocessor gp = new Geoprocessor();
                gp.OverwriteOutput = true;
                gp.AddOutputsToMap = false;
                gp.SetEnvironmentValue("workspace", _app.Workspace.EsriWorkspace.PathName);

                Intersect intersTool = new Intersect();
                Dissolve diss = new Dissolve();
                SymDiff diff = new SymDiff();
                Erase erasegp = new Erase();

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
                ExportLayer(ws, boulFC, "TEMP_Province");
                tempProvinceBOULFC = (ws as IFeatureWorkspace).OpenFeatureClass("TEMP_Province");
                IFeatureCursor tempProvinceCursor = tempProvinceBOULFC.Insert(true);

                ExportLayer(ws, boulFC, "TEMP_State");
                tempStateBOULFC = (ws as IFeatureWorkspace).OpenFeatureClass("TEMP_State");
                IFeatureCursor tempStateCursor = tempStateBOULFC.Insert(true);

                ExportLayer(ws, boulFC, "TEMP_County");
                tempCountyBOULFC = (ws as IFeatureWorkspace).OpenFeatureClass("TEMP_County");
                IFeatureCursor tempCountyCursor = tempCountyBOULFC.Insert(true);

                ExportLayer(ws, boulFC, "TEMP_Town");
                tempTownBOULFC = (ws as IFeatureWorkspace).OpenFeatureClass("TEMP_Town");
                IFeatureCursor tempTownCursor = tempTownBOULFC.Insert(true);

                int gbIndex = boulFC.FindField("gb");
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("RuleID <> 1");//境界不显示要素，不参与跳绘。
                IFeatureCursor boulCursor = boulFC.Search(qf, false);
                
                IFeature boulFeature = null;




                while ((boulFeature = boulCursor.NextFeature()) != null)
                {
                    int gbValue = Convert.ToInt32(boulFeature.get_Value(gbIndex));
                    if (provinceGB.Contains(gbValue))
                    {
                        IFeatureBuffer feaBuf = tempProvinceBOULFC.CreateFeatureBuffer();
                        feaBuf.Shape = boulFeature.Shape;
                        tempProvinceCursor.InsertFeature(feaBuf);
                    }
                    else if (StateGB.Contains(gbValue))
                    {
                        IFeatureBuffer feaBuf = tempStateBOULFC.CreateFeatureBuffer();
                        feaBuf.Shape = boulFeature.Shape;
                        tempStateCursor.InsertFeature(feaBuf);
                    }
                    else if (CountyGB.Contains(gbValue))
                    {
                        IFeatureBuffer feaBuf = tempCountyBOULFC.CreateFeatureBuffer();
                        feaBuf.Shape = boulFeature.Shape;
                        tempCountyCursor.InsertFeature(feaBuf);
                    }
                    else if (TownGB.Contains(gbValue))
                    {
                        IFeatureBuffer feaBuf = tempTownBOULFC.CreateFeatureBuffer();
                        feaBuf.Shape = boulFeature.Shape;
                        tempTownCursor.InsertFeature(feaBuf);
                    }
                    Marshal.ReleaseComObject(boulFeature);
                }
                tempCountyCursor.Flush();
                tempTownCursor.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(tempCountyCursor);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(tempTownCursor);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(tempStateCursor);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(tempProvinceCursor);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(boulCursor);
                #endregion
                #region GP合并【1、跳绘制图层合并; 2、将省、市等要素分别全部合并为一个整体省界、市界....】
                //省界合并
                if (wo != null)
                    wo.SetText("正在合并省界......");
                diss.in_features = tempProvinceBOULFC;
                diss.out_feature_class = ws.PathName + "\\ProvinceBOULMerge";
                SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                provinceBoulMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("ProvinceBOULMerge");
                //市界合并
                if (wo != null)
                    wo.SetText("正在合并市界......");
                diss.in_features = tempStateBOULFC;
                diss.out_feature_class = ws.PathName + "\\StateBOULMerge";
                SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                stateBoulMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("StateBOULMerge");
                //县界合并
                if (wo != null)
                    wo.SetText("正在合并县界......");
                diss.in_features = tempCountyBOULFC;
                diss.out_feature_class = ws.PathName + "\\CountyBOULMerge";
                SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                countyBoulMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("CountyBOULMerge");
                //乡界合并
                if (wo != null)
                    wo.SetText("正在合并乡界......");
                diss.in_features = tempTownBOULFC;
                diss.out_feature_class = ws.PathName + "\\TownBOULMerge";
                SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                townBoulMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("TownBOULMerge");
                //河流面合并[解决 裁切边界将河流面裁断 影响双线河跳绘]
                if (hydaFcl != null)
                {
                    #region 新的裁切功能 保持 边界线的河流面等要素完整性 不需要合并
                    //if (wo != null)
                    //    wo.SetText("正在合并河流面......");

                    //diss.in_features = hydaFcl;
                    //diss.out_feature_class = ws.PathName + "\\HydaMerge";
                    //gp.Execute(diss, null);
                    //gp.Execute(new MultipartToSinglepart("HydaMerge", "HydaMTS"), null);
                    //var mergefcl=  (ws as IFeatureWorkspace).OpenFeatureClass("HydaMerge");
                    //(mergefcl as IDataset).Delete();
                    //hydaFcl = (ws as IFeatureWorkspace).OpenFeatureClass("HydaMTS");
                    #endregion
                }
                //合并所有HYDL,lRDL。
                if (wo != null)
                    wo.SetText("正在合并" + "......");
                string lyrName = lyrNames[0];
                string hydlInterName = "";
                hydlMergeFC = null;

                if (lyrNames.Count > 1)//多个
                {
                    #region
                    string appendfcl = ws.PathName + "\\" + lyrNames[0] + "_Dis";
                    string inputs = "";
                    int lyrFlag = 0;
                    for (int i = 0; i < lyrNames.Count; i++)
                    {
                        string infeatures = lyrNames[i];
                        if (lyrNames[i] == "LRDL")
                        {
                            MakeFeatureLayer gpLayer = new MakeFeatureLayer { in_features = lyrNames[i], out_layer = "LRDL_Layer" };
                            SMGI.Common.Helper.ExecuteGPTool(gp, gpLayer, null);

                            SelectLayerByAttribute pSeAtrr = new SelectLayerByAttribute();
                            pSeAtrr.in_layer_or_view = "LRDL_Layer";
                            pSeAtrr.where_clause = "RuleID >=20";//选择单线路
                            pSeAtrr.selection_type = "NEW_SELECTION";
                            ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult geoRe = SMGI.Common.Helper.ExecuteGPTool(gp, pSeAtrr, null);
                            ESRI.ArcGIS.Geoprocessing.IGPUtilities gpUtils = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
                            if (geoRe.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                            {
                                ESRI.ArcGIS.Carto.IFeatureLayer roadLayer = gpUtils.DecodeLayer(geoRe.GetOutput(0)) as ESRI.ArcGIS.Carto.IFeatureLayer;
                                diss.in_features = roadLayer;
                                diss.out_feature_class = ws.PathName + "\\" + lyrNames[i] + "_Dis";
                                SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);

                                if (lyrFlag > 0)
                                    inputs += ws.PathName + "\\" + lyrNames[i] + "_Dis" + ";";
                                lyrFlag++;
                            }
                        }
                        else//其他【水系】 排除 不显示要素
                        {
                            MakeFeatureLayer gpLayer = new MakeFeatureLayer { in_features = lyrNames[i], out_layer = "HYDL_Layer" };
                            SMGI.Common.Helper.ExecuteGPTool(gp, gpLayer, null);

                            SelectLayerByAttribute pSeAtrr = new SelectLayerByAttribute();
                            pSeAtrr.in_layer_or_view = "HYDL_Layer";
                            pSeAtrr.where_clause = "RuleID <>1";//过滤不显示要素
                            pSeAtrr.selection_type = "NEW_SELECTION";
                            ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult geoRe = SMGI.Common.Helper.ExecuteGPTool(gp, pSeAtrr, null);
                            ESRI.ArcGIS.Geoprocessing.IGPUtilities gpUtils = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
                            if (geoRe.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                            {
                                ESRI.ArcGIS.Carto.IFeatureLayer hydlLayer = gpUtils.DecodeLayer(geoRe.GetOutput(0)) as ESRI.ArcGIS.Carto.IFeatureLayer;
                                diss.in_features = hydlLayer;
                                diss.out_feature_class = ws.PathName + "\\" + lyrNames[i] + "_Dis";
                                SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);

                                if (lyrFlag > 0)
                                    inputs += ws.PathName + "\\" + lyrNames[i] + "_Dis" + ";";
                                lyrFlag++;
                            }
                            //diss.in_features = lyrNames[i];
                            //diss.out_feature_class = ws.PathName + "\\" + lyrNames[i] + "_Dis";
                            //gp.Execute(diss, null);
                            //if (lyrFlag > 0)
                            //    inputs += ws.PathName + "\\" + lyrNames[i] + "_Dis" + ";";
                            //lyrFlag++;
                        }


                    }
                    inputs = inputs.Substring(0, inputs.Length - 1);
                    Append gpAppend = new Append();
                    gpAppend.inputs = inputs;
                    gpAppend.target = appendfcl;
                    SMGI.Common.Helper.ExecuteGPTool(gp, gpAppend, null);

                    hydlMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass(lyrNames[0] + "_Dis");
                    //删除其余图层。
                    for (int i = 1; i < lyrNames.Count; i++)
                    {
                        IFeatureClass tempfcl = (ws as IFeatureWorkspace).OpenFeatureClass(lyrNames[i] + "_Dis");
                        (tempfcl as IDataset).Delete();
                    }
                    #endregion
                }
                else//一个
                {
                    hydlFC = (ws as IFeatureWorkspace).OpenFeatureClass(lyrName);
                    diss.in_features = hydlFC;
                    diss.out_feature_class = ws.PathName + "\\" + lyrName + "Merge";
                    SMGI.Common.Helper.ExecuteGPTool(gp, diss, null);
                    hydlMergeFC = (ws as IFeatureWorkspace).OpenFeatureClass("" + lyrName + "Merge");
                }

                #endregion
                #region 缓冲跳绘图层 默认 0.1mm，设置为0，则不缓冲
                if (hydaFcl != null)//河流面内缓冲
                {
                    var gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                    {
                        in_features = ws.PathName + "\\" + hydaFcl.AliasName,
                        out_feature_class = ws.PathName + "\\" + hydaFcl.AliasName+"Buffer",
                        buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _inDisbf * 1.0e-3
                    };
                    SMGI.Common.Helper.ExecuteGPTool(gp, gpBuffer, null);
                    hydaBuffer = (ws as IFeatureWorkspace).OpenFeatureClass(hydaFcl.AliasName + "Buffer");
                }
                if (_bufferWidth > 0)
                {
                    //省
                    var gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                    {
                        in_features = ws.PathName + "\\" + provinceBoulMergeFC.AliasName,
                        out_feature_class = ws.PathName + "\\" + provinceBoulMergeFC.AliasName + "Buffer",
                        buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                    };
                    SMGI.Common.Helper.ExecuteGPTool(gp, gpBuffer, null);
                    provinceBoulBuffer = (ws as IFeatureWorkspace).OpenFeatureClass("" + provinceBoulMergeFC.AliasName + "Buffer");
                    //hydlInterName = "" + lyrName + "MergeBuffer";
                    //市州
                    gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                    {
                        in_features = ws.PathName + "\\" + stateBoulMergeFC.AliasName,
                        out_feature_class = ws.PathName + "\\" + stateBoulMergeFC.AliasName + "Buffer",
                        buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                    };
                    SMGI.Common.Helper.ExecuteGPTool(gp, gpBuffer, null);
                    stateBoulBuffer = (ws as IFeatureWorkspace).OpenFeatureClass("" + stateBoulMergeFC.AliasName + "Buffer");
                    //区县
                    gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                    {
                        in_features = ws.PathName + "\\" + countyBoulMergeFC.AliasName,
                        out_feature_class = ws.PathName + "\\" + countyBoulMergeFC.AliasName + "Buffer",
                        buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                    };
                    SMGI.Common.Helper.ExecuteGPTool(gp, gpBuffer, null);
                    countyBoulBuffer = (ws as IFeatureWorkspace).OpenFeatureClass("" + countyBoulMergeFC.AliasName + "Buffer");
                    //乡镇
                    gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer
                    {
                        in_features = ws.PathName + "\\" + townBoulMergeFC.AliasName,
                        out_feature_class = ws.PathName + "\\" + townBoulMergeFC.AliasName + "Buffer",
                        buffer_distance_or_field = GApplication.Application.ActiveView.FocusMap.ReferenceScale * _bufferWidth * 1.0e-3
                    };
                    SMGI.Common.Helper.ExecuteGPTool(gp, gpBuffer, null);
                    townBoulBuffer = (ws as IFeatureWorkspace).OpenFeatureClass("" + townBoulMergeFC.AliasName + "Buffer");

                }
                else//不缓冲
                {
                    provinceBoulBuffer = provinceBoulMergeFC;
                    stateBoulBuffer = stateBoulMergeFC;
                    countyBoulBuffer = countyBoulMergeFC;
                    townBoulBuffer = townBoulMergeFC;


                    // hydlInterName = hydlMergeFC.AliasName;
                }
                hydlInterName = hydlMergeFC.AliasName;
                #endregion
                #region GP求交
                //【省界】求交
                // 步骤：1.境界缓冲与水系等相交处理，得到相交的水系
                // 步骤：2.相交水系缓冲再与合并的境界求交
                // 步骤：3.排除在双线河内的境界（通过将双线河做内缓冲与结果求异）
                // 解决 小比例水系 缓冲 报错！
                if (wo != null)
                    wo.SetText("正在求交【省界】......");



                intersTool.in_features = ws.PathName + "\\" + provinceBoulBuffer.AliasName + ";" + ws.PathName + "\\" + hydlInterName;
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
                intersTool.in_features = ws.PathName + "\\" + provinceHYDLBF.AliasName + ";" + ws.PathName + "\\" + provinceBoulMergeFC.AliasName;
                intersTool.out_feature_class = ws.PathName + "\\ProvinceIntersResult1";
                intersTool.output_type = "LINE";
                SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                //排除双线河内的河流
                provinceIntersFC1 = (ws as IFeatureWorkspace).OpenFeatureClass("ProvinceIntersResult1");
                provinceIntersFC = provinceIntersFC1;
                IFeature fe = provinceIntersFC1.Search(null, false).NextFeature();
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


                //【市界】求交
                if (wo != null)
                    wo.SetText("正在求交【市界】......");
                intersTool.in_features = ws.PathName + "\\" + stateBoulBuffer.AliasName + ";" + ws.PathName + "\\" + hydlInterName;
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
                intersTool.in_features = ws.PathName + "\\" + stateHYDLBF.AliasName + ";" + ws.PathName + "\\" + stateBoulMergeFC.AliasName;
                intersTool.out_feature_class = ws.PathName + "\\StateIntersResult1";
                intersTool.output_type = "LINE";
                SMGI.Common.Helper.ExecuteGPTool(gp, intersTool, null);
                //排除双线河内的河流
                stateIntersFC1 = (ws as IFeatureWorkspace).OpenFeatureClass("StateIntersResult1");
                stateIntersFC = stateIntersFC1;
                fe = stateIntersFC1.Search(null, false).NextFeature();
                if (fe != null && hydaBuffer!=null)
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
            
              
              
                //【县界】求交
                if (wo != null)
                    wo.SetText("正在求交【县界】......");
                intersTool.in_features = ws.PathName + "\\" + countyBoulBuffer.AliasName + ";" + ws.PathName + "\\" + hydlInterName;
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
                intersTool.in_features = ws.PathName + "\\" + countyHYDLBF.AliasName + ";" + ws.PathName + "\\" + countyBoulMergeFC.AliasName;
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
              
                //【乡界】求交
                if (wo != null)
                    wo.SetText("正在求交【乡界】......");
                intersTool.in_features = ws.PathName + "\\" + townBoulBuffer.AliasName + ";" + ws.PathName + "\\" + hydlInterName;
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
                intersTool.in_features = ws.PathName + "\\" + townHYDLBF.AliasName + ";" + ws.PathName + "\\" + townBoulMergeFC.AliasName;
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
                #endregion
                #region GP求异，并将求异结果直接导出JJTH图层
                //【省界】求异
                if (wo != null)
                    wo.SetText("正在求异【省界】......");
                diff.in_features = ws.PathName + "\\ProvinceBOULMerge";
                diff.update_features = ws.PathName + "\\"+provinceIntersFC.AliasName;
                diff.out_feature_class = ws.PathName + "\\省界";
                SMGI.Common.Helper.ExecuteGPTool(gp, diff, null);
                SMGI.Common.Helper.ExecuteGPTool(gp, new MultipartToSinglepart("省界", "省界MTS"), null);
                provinceDiffMTSFC = (ws as IFeatureWorkspace).OpenFeatureClass("省界MTS");
                RecordResult(thFCJJ, provinceDiffMTSFC, 63020100);
                provinceDiffFC = (ws as IFeatureWorkspace).OpenFeatureClass("省界");
                //【市界】求异
                if (wo != null)
                    wo.SetText("正在求异【市界】......");
                diff.in_features = ws.PathName + "\\StateBOULMerge";
                diff.update_features = ws.PathName + "\\" + stateIntersFC.AliasName;
                diff.out_feature_class = ws.PathName + "\\市界";
                SMGI.Common.Helper.ExecuteGPTool(gp, diff, null);
                SMGI.Common.Helper.ExecuteGPTool(gp, new MultipartToSinglepart("市界", "市界MTS"), null);
                stateDiffMTSFC = (ws as IFeatureWorkspace).OpenFeatureClass("市界MTS");
                RecordResult(thFCJJ, stateDiffMTSFC, 64020100);
                stateDiffFC = (ws as IFeatureWorkspace).OpenFeatureClass("市界");
                //【县界】求异
                if (wo != null)
                    wo.SetText("正在求异【县界】......");
                diff.in_features = ws.PathName + "\\CountyBOULMerge";
                diff.update_features = ws.PathName + "\\" + countyIntersFC.AliasName;
                diff.out_feature_class = ws.PathName + "\\县界";
                SMGI.Common.Helper.ExecuteGPTool(gp, diff, null);
                SMGI.Common.Helper.ExecuteGPTool(gp, new MultipartToSinglepart("县界", "县界MTS"), null);
                countyDiffMTSFC = (ws as IFeatureWorkspace).OpenFeatureClass("县界MTS");
                RecordResult(thFCJJ, countyDiffMTSFC, 65020100);
                countyDiffFC = (ws as IFeatureWorkspace).OpenFeatureClass("县界");
                //【乡界】求异
                if (wo != null)
                    wo.SetText("正在求异【乡界】......");
                diff.in_features = ws.PathName + "\\TownBOULMerge";
                diff.update_features = ws.PathName + "\\" + townIntersFC.AliasName; ;
                diff.out_feature_class = ws.PathName + "\\乡界";
                SMGI.Common.Helper.ExecuteGPTool(gp, diff, null);
                SMGI.Common.Helper.ExecuteGPTool(gp, new MultipartToSinglepart("乡界", "乡界MTS"), null);
                townDiffMTSFC = (ws as IFeatureWorkspace).OpenFeatureClass("乡界MTS");
                RecordResult(thFCJJ, townDiffMTSFC, 66020100);
                townDiffFC = (ws as IFeatureWorkspace).OpenFeatureClass("乡界");
                #endregion   
                //记录【省界跳绘结果】
                if (wo != null)
                    wo.SetText("正在记录【省界跳绘结果】......");
                RecordResultEx(thFCJJ, BoulDrawDic["Province"], provinceIntersFC, 63020110);           
                //记录【市界跳绘结果】
                if (wo != null)
                    wo.SetText("正在记录【市界跳绘结果】......");
                RecordResultEx(thFCJJ, BoulDrawDic["State"], stateIntersFC, 64020110);
                //记录【县界跳绘结果】
                if (wo != null)
                    wo.SetText("正在记录【县界跳绘结果】......");
                RecordResultEx(thFCJJ, BoulDrawDic["County"], countyIntersFC, 65020110);
                //记录【乡界跳绘结果】
                if (wo != null)
                    wo.SetText("正在记录【乡界跳绘结果】......");
                RecordResultEx(thFCJJ, BoulDrawDic["Town"], townIntersFC, 66020110);
                if (wo != null)
                    wo.SetText("正在【清理过程数据】......");


                //匹配规则
                if (wo != null)
                    wo.SetText("正在匹配规则......");
                MatchRules(thFCJJ);
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
              if (hydaFcl.AliasName == "HydaMerge")
              {
                  (hydaFcl as IDataset).Delete();
              }
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
            }
            GC.Collect();
            return msg;
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

        //插入要素
        private int RecordResult(IFeatureClass expIntersFC, IFeatureClass intersFC, int gbValue)
        {
            int nCount = 0;
            int gbIndex = expIntersFC.FindField("GB");
            IFeatureCursor thCursor = expIntersFC.Insert(true);

            IFeatureCursor intersCursor = intersFC.Search(null, true);
            IFeature intersFeature = null;
            IFeatureBuffer newFea = expIntersFC.CreateFeatureBuffer();
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
                double len = (intersFeature.Shape as IPolyline).Length;
                //长度大于图上0.5毫米，才保留
                if (len > 0.5e-3 * _app.ActiveView.FocusMap.ReferenceScale)
                {
                    newFea.Shape = intersFeature.Shape;
                    newFea.set_Value(gbIndex, gbValue);
                    thCursor.InsertFeature(newFea);
                }
                nCount++;
                Marshal.ReleaseComObject(intersFeature);
            }
           
            thCursor.Flush();
            Marshal.ReleaseComObject(newFea);
            Marshal.ReleaseComObject(thCursor);
            Marshal.ReleaseComObject(intersCursor);

            return nCount;
        }

        //插入要素:跳绘算法核心函数
        private int RecordResultEx0(IFeatureClass expJJTHfcl, BoulDrawInfo info, IFeatureClass intersFC, int gbValue)
        {  
            
            
           
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
                IGeometryCollection geoCol = intersFeature.Shape as IGeometryCollection;
                if (geoCol.GeometryCount > 1)//相交结果为一个多部件
                {
                    #region
                    for (int geoItem = 0; geoItem < geoCol.GeometryCount; geoItem++)
                    {
                        IPolyline pl = new PolylineClass();
                        IPath pa = geoCol.get_Geometry(geoItem) as IPath;
                        (pl as IGeometryCollection).AddGeometry(pa);

                        double geoLength = pl.Length;

                        int offset = 0;
                        double lineWidth = info.SolidValue*_app.ActiveView.FocusMap.ReferenceScale*1.0e-3;//实线长
                        double blankWidth = info.BlankValue * _app.ActiveView.FocusMap.ReferenceScale * 1.0e-3;//跳绘线间距长
                        double groups = info.SymbolGroup;//组数
                        double pointwidth = info.PointStep * _app.ActiveView.FocusMap.ReferenceScale * 1.0e-3;//点间距

                        //处理参数
                        double blankEndsValue = 0;//左右留白间距
                        int timesValue = 0;//跳绘组数
                        //一组线段长度:包括多个
                        double solidvalue = lineWidth * groups - pointwidth / 2;
                        double groupLength = 0;//跳绘一组长度
                        #region
                        if (geoLength > 0)
                        {
                            if (geoLength > (solidvalue + blankWidth))//大于一个实步+虚步
                            {
                                double modValue = (geoLength % (solidvalue + blankWidth));
                                blankEndsValue = modValue / 2;
                                timesValue = (int)(geoLength / (solidvalue + blankWidth));
                                groupLength = solidvalue + blankWidth;
                            }
                            else  //小于一个实步+虚步
                            {
                                if (geoLength > solidvalue)    //如果大于一个实步
                                {
                                    double modValue = (geoLength % (solidvalue));
                                   
                                    timesValue = (int)(geoLength / (solidvalue));
                                    blankEndsValue = modValue / (2+timesValue);
                                    groupLength = solidvalue + blankEndsValue;
                                }
                                else//如果小于一个实步：不处理
                                {
                                    continue;
                                }
                            }
                        }
                        #endregion
                        ICurve subPolyline = new PolylineClass();
                        //切去线两头留白部分
                        pl.GetSubcurve(blankEndsValue, pl.Length - blankEndsValue, false, out subPolyline);
                        for (int i = 0; i < timesValue; i++)
                        {
                            ICurve groupPlyline = new PolylineClass();
                            double part = i + 1;
                            (subPolyline as IPolyline).GetSubcurve(i*1.0 / timesValue, part / timesValue, true, out groupPlyline);
                            //获取实部线
                            ICurve solidPlyline = new PolylineClass();
                            (groupPlyline as IPolyline).GetSubcurve(0, solidvalue, false, out solidPlyline);

                          
                            newFea.Shape = solidPlyline as IGeometry;
                            newFea.set_Value(gbIndex, gbValue + (offset++));
                            if (offset == 2)
                            {
                                offset = 0;
                            }
                            thCursor.InsertFeature(newFea);
                        }
                    }
                    #endregion

                }
                Marshal.ReleaseComObject(intersFeature);
            }
            thCursor.Flush();
            Marshal.ReleaseComObject(newFea);
            Marshal.ReleaseComObject(thCursor);
            Marshal.ReleaseComObject(intersCursor);
            return nCount;
        }
        //跳会算法修改2018-1.25:跳绘平滑
        private int RecordResultEx(IFeatureClass expJJTHfcl, BoulDrawInfo info, IFeatureClass intersFC, int gbValue)
        {



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
                IGeometryCollection geoCol = intersFeature.Shape as IGeometryCollection;
                if (geoCol.GeometryCount > 1)//相交结果为一个多部件
                {
                    #region
                    for (int geoItem = 0; geoItem < geoCol.GeometryCount; geoItem++)
                    {
                        IPolyline pl = new PolylineClass();
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
                               
                                thCursor.InsertFeature(newFea);
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
            return nCount;
        }

        private int RecordResultEx(IFeatureClass expJJTHfcl, IFeatureClass intersFC, double lineWidth, double blankWith, int gbValue)
        {
            int gbIndex = expJJTHfcl.FindField("GB");
            int nCount = 0;
            IFeatureCursor thCursor = expJJTHfcl.Insert(true);

            IFeatureCursor intersCursor = intersFC.Search(null, true);
            IFeature intersFeature = null;
            while ((intersFeature = intersCursor.NextFeature()) != null)
            {
                if (intersFeature.Shape == null)
                {
                    continue;
                }
                IGeometryCollection geoCol = intersFeature.Shape as IGeometryCollection;
                if (geoCol.GeometryCount > 1)
                {
                    for (int geoItem = 0; geoItem < geoCol.GeometryCount; geoItem++)
                    {
                        IPolyline pl = new PolylineClass();
                        IPath pa = geoCol.get_Geometry(geoItem) as IPath;
                        (pl as IGeometryCollection).AddGeometry(pa);

                        double geoLength = pl.Length;
                        int timesValue = 0;
                        int offset = 0;
                        double lineValue = lineWidth;
                        double blankValue = blankWith;
                        if (geoLength > 0)
                        {
                            if (geoLength > (lineWidth + blankWith))
                            {
                                double modValue = (geoLength % (lineWidth + blankWith));
                                timesValue = (int)(geoLength / (lineWidth + blankWith));

                                if (modValue > lineWidth)    //如果余数大于一个实步
                                {
                                    double tempValue = modValue - lineWidth;
                                    blankValue = (tempValue + blankWith * timesValue) / (timesValue + 2);
                                }
                                else if (modValue < lineWidth) //如果余数小于一个实步
                                {
                                    double tempValue = lineWidth - modValue;
                                    blankValue = (tempValue + blankWith * timesValue) / (timesValue + 1);
                                }
                            }
                            else if (geoLength < (lineWidth + blankWith)) //如果相交线段不满足一个时虚步
                            {
                                if (geoLength > lineWidth)    //如果大于一个实步
                                {
                                    double tempValue = geoLength - lineWidth;
                                    blankValue = tempValue / 2;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                        for (int i = 0; ; i++)
                        {
                            bool isSplit;
                            int splitIndex, segIndex;
                            object o = Type.Missing;
                            bool bLine = false;
                            if (i % 2 == 0)
                            {
                                if (pl.Length < blankValue)
                                {
                                    break;
                                }
                                pl.SplitAtDistance(blankValue, false, false, out isSplit, out splitIndex, out segIndex);
                                bLine = false;
                            }
                            else
                            {
                                if (pl.Length < lineValue)
                                {
                                    break;
                                }
                                pl.SplitAtDistance(lineValue, false, false, out isSplit, out splitIndex, out segIndex);
                                bLine = true;
                            }
                            if (isSplit)
                            {
                                IPolyline frontLine = null;
                                IPolyline backLine = new PolylineClass();
                                ISegmentCollection lineSegCol = (ISegmentCollection)pl;
                                ISegmentCollection backSegCol = (ISegmentCollection)backLine;

                                for (int j = segIndex; j < lineSegCol.SegmentCount; j++)
                                {
                                    backSegCol.AddSegment(lineSegCol.get_Segment(j), ref o, ref o);
                                }

                                backSegCol.SegmentsChanged();
                                lineSegCol.RemoveSegments(segIndex, lineSegCol.SegmentCount - segIndex, true);
                                lineSegCol.SegmentsChanged();
                                frontLine = lineSegCol as IPolyline;
                                backLine = backSegCol as IPolyline;
                                if (bLine)
                                {
                                    for (int j = 0; j < (frontLine as IGeometryCollection).GeometryCount; j++)
                                    {
                                        nCount++;
                                        IPolyline pline = new PolylineClass();
                                        (pline as IGeometryCollection).AddGeometry((frontLine as IGeometryCollection).get_Geometry(j));
                                        IFeatureBuffer newFea = expJJTHfcl.CreateFeatureBuffer();
                                        newFea.Shape = pline as IGeometry;
                                        newFea.set_Value(gbIndex, gbValue + (offset++));
                                        if (offset == 2)
                                        {
                                            offset = 0;
                                        }
                                        thCursor.InsertFeature(newFea);
                                    }
                                }
                                pl = backLine;
                            }
                        }
                    }
                }
            }
            thCursor.Flush();

            Marshal.ReleaseComObject(thCursor);
            Marshal.ReleaseComObject(intersCursor);
            return nCount;
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
  
    }
}


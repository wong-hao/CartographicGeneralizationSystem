using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.IO;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;

namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class RiverBoundayExtractCmd : SMGI.Common.SMGICommand
    {
        public RiverBoundayExtractCmd()
        {
            m_caption = "水系面边线提取";
        }
      
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            string hydaFCName = "HYDA";
            var hydaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == hydaFCName);
            })).FirstOrDefault() as IFeatureLayer;
            if (hydaLayer == null || hydaLayer.FeatureClass == null)
            {
                MessageBox.Show(string.Format("未找到要素类【{0}】", hydaFCName));
                return;
            }

            string hydaBoundaryFCName = "HYDATOLINE";
            IFeatureLayer hydaBoundaryLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == hydaBoundaryFCName);
            })).FirstOrDefault() as IFeatureLayer;
            if (hydaBoundaryLayer == null || hydaBoundaryLayer.FeatureClass == null)
            {
                MessageBox.Show(string.Format("未找到要素类【{0}】", hydaBoundaryFCName));
                return;
            }


            string fileName = GApplication.Application.Template.Root + @"\专家库\RiverProcessRule.xml";
            if (!File.Exists(fileName))
            {
                MessageBox.Show(string.Format("未找到配置文件【{0}】", fileName));
                return;
            }

            m_Application.EngineEditor.StartOperation();
            try
            {
                

                using (WaitOperation wo = m_Application.SetBusy())
                {
                    wo.SetText("正在处理双线河");

                    HydaBoundayExtract(hydaLayer, hydaBoundaryLayer, fileName, wo);
                }

                m_Application.EngineEditor.StopOperation("水系面边线提取");

                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, hydaBoundaryLayer, m_Application.ActiveView.Extent);
            }
            catch
            {
                m_Application.EngineEditor.AbortOperation();
            }
            
        }

        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                string hydaFCName = "HYDA";
                var hydaLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == hydaFCName);
                })).FirstOrDefault() as IFeatureLayer;
                if (hydaLayer == null || hydaLayer.FeatureClass == null)
                {
                    messageRaisedAction(string.Format("未找到要素类【{0}】", hydaFCName));
                    return false;
                }

                string hydaBoundaryFCName = "HYDATOLINE";
                IFeatureLayer hydaBoundaryLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == hydaBoundaryFCName);
                })).FirstOrDefault() as IFeatureLayer;
                if (hydaBoundaryLayer == null || hydaBoundaryLayer.FeatureClass == null)
                {
                    messageRaisedAction(string.Format("未找到要素类【{0}】", hydaBoundaryFCName));
                    return false;
                }


                string fileName = GApplication.Application.Template.Root + @"\专家库\RiverProcessRule.xml";
                if (!File.Exists(fileName))
                {
                    messageRaisedAction(string.Format("未找到配置文件【{0}】", fileName));
                    return false;
                }

                messageRaisedAction("正在进行水系面边线提取...");

                HydaBoundayExtract(hydaLayer, hydaBoundaryLayer, fileName);

                m_Application.Workspace.Save();

                return true;
            }
            catch
            {
                messageRaisedAction("水系面边线提取失败！");
                return false;
            }
        }

        //双线河边界提取:处理色带与水系关系
        public static void HydaBoundayExtract(IFeatureLayer hydaLayer, IFeatureLayer hydaBoundayLayer, string configFilename, WaitOperation wo = null)
        {
            IFeatureClass hydaFC = hydaLayer.FeatureClass;
            IFeatureClass hydaBoundayFC = hydaBoundayLayer.FeatureClass;

            //清空水系面边线要素
            if (wo != null)
                wo.SetText("正在清空原水系面边线要素......");
            (hydaBoundayFC as ITable).DeleteSearchedRows(null);

            //读取配置表,进行边线提取
            XDocument doc = XDocument.Load(configFilename);
            var hyda2LineItem = doc.Element("Content").Element("HYDATOLINE");
            var lineTypeitem = hyda2LineItem.Elements("LineType");
            if (lineTypeitem == null || lineTypeitem.Count() == 0)
                return;

            #region 创建临时数据库、GP工具等
            string fullPath = CommonMethods.GetAppDataPath() + "\\MyWorkspace.gdb";
            IWorkspace ws = CommonMethods.createTempWorkspace(fullPath);
            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;
            #endregion

            IFeatureClass tempHYDAFC = null;
            IFeatureClass tempBoundaryFC = null;
            try
            {
                string filter = "RuleID <> 1";//不显示要素排除
                if ((hydaLayer as IFeatureLayerDefinition).DefinitionExpression != "")
                    filter += string.Format(" and ({0})", (hydaLayer as IFeatureLayerDefinition).DefinitionExpression);

                ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer makeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
                makeFeatureLayer.in_features = hydaFC;
                makeFeatureLayer.out_layer = hydaFC.AliasName + "_Layer";
                SMGI.Common.Helper.ExecuteGPTool(gp, makeFeatureLayer, null);

                ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute selectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
                selectLayerByAttribute.in_layer_or_view = hydaFC.AliasName + "_Layer";
                selectLayerByAttribute.out_layer_or_view = hydaFC.AliasName + "_Layer_Selected";
                

                foreach (var lineType in lineTypeitem)
                {
                    if (wo != null)
                        wo.SetText("正在提取水系面边线......");

                    int ruleID = int.Parse(lineType.Attribute("RuleID").Value);
   
                    IQueryFilter qf = new QueryFilterClass() { WhereClause = filter };

                    string dissSQL = "";//参与融合的要素
                    string notDissSQL = "";//不参与融合的要素
                    #region 读取配置
                    foreach (var item in lineType.Elements("Item"))
                    {
                        string sqlText = string.Format("({0})", item.Attribute("sql").Value);
                        bool needDiss = bool.Parse(item.Attribute("diss").Value);

                        if (needDiss)
                        {
                            if (dissSQL != "")
                            {
                                dissSQL += " or ";
                            }
                            dissSQL += string.Format("({0})", item.Attribute("sql").Value);
                        }
                        else
                        {
                            if (notDissSQL != "")
                            {
                                notDissSQL += " or ";
                            }
                            notDissSQL += string.Format("({0})", item.Attribute("sql").Value);
                        }
                    }
                    #endregion
                    if (dissSQL != "" && notDissSQL != "")
                        qf.WhereClause += string.Format(" and ({0} or {1})", dissSQL, notDissSQL);
                    if (hydaFC.FeatureCount(qf) == 0)
                        continue;

                    if (tempHYDAFC != null)
                    {
                        (tempHYDAFC as IDataset).Delete();
                        tempHYDAFC = null;
                    }
                    if (tempBoundaryFC != null)
                    {
                        (tempBoundaryFC as IDataset).Delete();
                        tempBoundaryFC = null;
                    }

                    IFeatureCursor insertFeCursor = null;
                    IFeatureBuffer feBuffer = null;
                    IFeatureCursor feCursor = null;
                    IFeature fe = null;
                    if (dissSQL != "")//存在需要融合的要素
                    {
                        
                        selectLayerByAttribute.where_clause = string.Format("{0} and ({1})", filter, dissSQL);
                        selectLayerByAttribute.selection_type = "NEW_SELECTION";
                        ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult geoRe = SMGI.Common.Helper.ExecuteGPTool(gp, selectLayerByAttribute, null);
                        if (geoRe.Status != ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                        {
                            continue;
                        }

                        ESRI.ArcGIS.Geoprocessing.IGPUtilities gpUtils = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
                        ESRI.ArcGIS.Carto.IFeatureLayer layer = gpUtils.DecodeLayer(geoRe.GetOutput(0)) as ESRI.ArcGIS.Carto.IFeatureLayer;

                        #region 处理需进行融合的面要素:融合
                        Dissolve ds = new Dissolve();
                        ds.in_features = layer;
                        ds.out_feature_class = ws.PathName + "\\" + string.Format("{0}_Diss", hydaFC.AliasName);
                        SMGI.Common.Helper.ExecuteGPTool(gp, ds, null);

                        tempHYDAFC = fws.OpenFeatureClass(string.Format("{0}_Diss", hydaFC.AliasName));
                        #endregion

                        #region 处理不需融合的面要素：直接插入
                        if (notDissSQL != "")
                        {
                            insertFeCursor = tempHYDAFC.Insert(true);
                            feBuffer = tempHYDAFC.CreateFeatureBuffer();

                            feCursor = hydaFC.Search(new QueryFilterClass() { WhereClause = string.Format("{0} and ({1})", filter, notDissSQL) }, false);
                            while ((fe = feCursor.NextFeature()) != null)
                            {
                                feBuffer.Shape = fe.ShapeCopy;
                                insertFeCursor.InsertFeature(feBuffer);

                                Marshal.ReleaseComObject(fe);
                            }
                            insertFeCursor.Flush();

                            Marshal.ReleaseComObject(insertFeCursor);
                            Marshal.ReleaseComObject(feCursor);
                        }
                        #endregion

                        #region 面转线
                        FeatureToLine fe2Line = new FeatureToLine();
                        fe2Line.in_features = ws.PathName + "\\" + string.Format("{0}_Diss", hydaFC.AliasName);
                        fe2Line.out_feature_class = ws.PathName + "\\" + string.Format("{0}_HYDA2Line", hydaFC.AliasName);
                        SMGI.Common.Helper.ExecuteGPTool(gp, fe2Line, null);
                        tempBoundaryFC = fws.OpenFeatureClass(string.Format("{0}_HYDA2Line", hydaFC.AliasName));
                        #endregion
                    }
                    else//不存在需要融合的要素
                    {
                        if (notDissSQL != "")
                        {
                            selectLayerByAttribute.where_clause = string.Format("{0} and ({1})", filter, notDissSQL);
                            selectLayerByAttribute.selection_type = "NEW_SELECTION";
                            ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult geoRe = SMGI.Common.Helper.ExecuteGPTool(gp, selectLayerByAttribute, null);
                            if (geoRe.Status != ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                            {
                                continue;
                            }

                            ESRI.ArcGIS.Geoprocessing.IGPUtilities gpUtils = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
                            ESRI.ArcGIS.Carto.IFeatureLayer layer = gpUtils.DecodeLayer(geoRe.GetOutput(0)) as ESRI.ArcGIS.Carto.IFeatureLayer;

                            #region 面转线
                            FeatureToLine fe2Line = new FeatureToLine();
                            fe2Line.in_features = layer;
                            fe2Line.out_feature_class = ws.PathName + "\\" + string.Format("{0}_HYDA2Line", hydaFC.AliasName);
                            SMGI.Common.Helper.ExecuteGPTool(gp, fe2Line, null);
                            tempBoundaryFC = fws.OpenFeatureClass(string.Format("{0}_HYDA2Line", hydaFC.AliasName));
                            #endregion
                        }
                    }

                    if (tempBoundaryFC == null)
                        continue;

                    #region 结果数据拷贝
                    insertFeCursor = hydaBoundayFC.Insert(true);
                    feBuffer = hydaBoundayFC.CreateFeatureBuffer();

                    feCursor = tempBoundaryFC.Search(null, false);
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        feBuffer.Shape = fe.ShapeCopy;
                        feBuffer.set_Value(hydaBoundayFC.FindField("RuleID"), ruleID);
                        insertFeCursor.InsertFeature(feBuffer);

                        Marshal.ReleaseComObject(fe);
                    }
                    insertFeCursor.Flush();

                    Marshal.ReleaseComObject(insertFeCursor);
                    Marshal.ReleaseComObject(feCursor);
                    #endregion
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (tempHYDAFC != null)
                {
                    (tempHYDAFC as IDataset).Delete();
                    tempHYDAFC = null;
                }
                if (tempBoundaryFC != null)
                {
                    (tempBoundaryFC as IDataset).Delete();
                    tempBoundaryFC = null;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Xml;
using System.Xml.Linq;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.CartographyTools;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Geoprocessing;
using System.IO;

namespace SMGI.Plugin.MapGeneralization
{
    //化简
    public class TopolgyEdgeSimpifyAutoCmd2 : SMGI.Common.SMGICommand
    {

        List<string> _ruleNames = new List<string>();

        public TopolgyEdgeSimpifyAutoCmd2()
        {
            m_caption = "拓扑边化简 自动";
            m_toolTip = "拓扑边化简 自动";
            m_category = "拓扑";

        }
        public override bool Enabled
        {
            get
            {

                return TopologyApplication.Topology != null && m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing; ;
            }
        }
       
        ITopology topgy = null;
        public override void OnClick()
        {
            SimplifyToleranceFrm frm = new SimplifyToleranceFrm();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var gp = new Geoprocessor() { OverwriteOutput = true };
            string workspacePath = m_Application.Workspace.EsriWorkspace.PathName;
            IFeatureWorkspace feaWS = m_Application.Workspace.EsriWorkspace as IFeatureWorkspace;

           
            double bendLens = frm.Tolerance;
            double smoothValue = 20;

            var wo = m_Application.SetBusy();
            List<IGeometry> errorsDic = new List<IGeometry>();
            
            IFeatureClassContainer fcContainer = TopologyApplication.Topology as IFeatureClassContainer;
            //获取需要化简的拓扑图层
            List<IFeatureClass> listfc = new List<IFeatureClass>();
            fcContainer.Classes.Reset();
            IFeatureClass tempfc = null;
            var classes = fcContainer.Classes;
            while ((tempfc = classes.Next()) != null)
            {
                if (!listfc.Contains(tempfc))
                {
                    listfc.Add(tempfc);
                }
            }

            foreach (var inputFC in listfc)
            {
                IFeatureClass simplifyFC = null;
                IFeatureClass smoothFcl = null;
                try
                {
                    //1初始化
                    wo.SetText("[1] 正在初始化 " + inputFC.AliasName + "...");
                    #region
                    Dictionary<string, int> objDic = new Dictionary<string, int>();
                    IFeature feIn = null;
                    IFeatureCursor cursorIn = inputFC.Search(null, false);
                    int smgiIndex=inputFC.FindField("SMGIGUID");
                    if(smgiIndex==-1)
                    {
                        MessageBox.Show("缺少SMGIGUID 字段");
                        continue;
                    }
                    while ((feIn = cursorIn.NextFeature()) != null)
                    {
                        objDic[feIn.get_Value(smgiIndex).ToString()] = feIn.OID;
                    }
                    #endregion
                    string simplifyInFeature = workspacePath + @"\" + inputFC.AliasName;
                    string simplifyOutFeature = workspacePath + @"\" + inputFC.AliasName + "ToSimplify";

                    SimplifyPolygon simplifyLineTool = new SimplifyPolygon(simplifyInFeature, simplifyOutFeature, "BEND_SIMPLIFY", bendLens);
                    simplifyLineTool.error_option = "RESOLVE_ERRORS";
                    simplifyLineTool.minimum_area = 0;
                    wo.SetText("[2] 正在化简 " + inputFC.AliasName + "...");
                    //2.化简
                    bool  geoResult=  RunTool(gp, simplifyLineTool);
                    if(geoResult)
                    {
                        string smoothLineOutFeature = workspacePath + @"\" + inputFC.AliasName + "ToSimplifySmooth";
                        SmoothPolygon smoothLineTool = new SmoothPolygon(simplifyOutFeature, smoothLineOutFeature, "PAEK", smoothValue);
                        smoothLineTool.error_option = "FLAG_ERRORS";
                        wo.SetText("[3] 正在平滑 " + inputFC.AliasName + "...");
                        //3.平滑
                        geoResult = RunTool(gp, smoothLineTool);
                        simplifyFC = feaWS.OpenFeatureClass(inputFC.AliasName + "ToSimplify");
                        smoothFcl = feaWS.OpenFeatureClass(inputFC.AliasName + "ToSimplifySmooth");

                        //4.化简化拓扑赋值

                        bool isEdit = m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
                        if (!isEdit)
                        {
                            m_Application.EngineEditor.StartEditing(m_Application.Workspace.EsriWorkspace, m_Application.ActiveView.FocusMap);
                            m_Application.EngineEditor.EnableUndoRedo(true);
                        }
                        bool b = true;
                        try
                        {
                            m_Application.EngineEditor.StartOperation();

                            IFeatureCursor featureCursor = smoothFcl.Search(null, false);
                            int count = smoothFcl.FeatureCount(null);
                            IFeature feature = null;
                            int flagindex = 0;
                            int filedindex = smoothFcl.FindField("SMGIGUID");
                            while ((feature = featureCursor.NextFeature()) != null)
                            {
                                wo.SetText(inputFC.AliasName + " [4] 赋值已完成 " + (flagindex++) + "/" + count);
                                string smgiuid = feature.get_Value(filedindex).ToString();
                                if (objDic.ContainsKey(smgiuid))
                                {
                                    int oid = objDic[smgiuid];
                                    IFeature featureInput = inputFC.GetFeature(oid);
                                    featureInput.Shape = feature.ShapeCopy;
                                    featureInput.Store();
                                }

                            }

                            m_Application.EngineEditor.StopOperation("拓扑化简（自动）") ;
                        }
                        catch (Exception ex)
                        {
                            b = false;
                            m_Application.EngineEditor.AbortOperation();

                            MessageBox.Show("化简结果赋值错误:" + ex.Message);
                            throw ex;
                        }
                        finally
                        {
                            if (!isEdit)
                            {
                                m_Application.EngineEditor.StopEditing(b);
                            }
                        }

                        


                        //5.拓扑检查，并记录
                        wo.SetText(inputFC.AliasName + " [5]面重叠检查");
                        TopologyHelper helper1 = new TopologyHelper(m_Application.ActiveView);
                        var topgy1 = TopologyApplication.Topology;

                        helper1.AddRuleToTopology(topgy1, esriTopologyRuleType.esriTRTAreaNoOverlap, "must not overlap", inputFC);
                        IGeoDataset geoDataset = (IGeoDataset)topgy1;
                        IEnvelope envelope = geoDataset.Extent;
                        helper1.ValidateTopology(topgy1, envelope);

                        //IFeatureDataset featureDataset = featureWorkspace.OpenFeatureDataset(featureDatasetName);
                        ITopologyContainer topologyContainer = (ITopologyContainer)(inputFC.FeatureDataset);
                        ITopology topologyChecke = topologyContainer.get_TopologyByName(TopologyApplication.TopName);

                        //ITopology topologyChecke = CheckPolygonOverlap.OpenTopology((inputFC.FeatureDataset.Workspace as IFeatureWorkspace), inputFC.FeatureDataset.Name, TopologyApplication.TopName);
                        IErrorFeatureContainer errorFeatureContainer = (IErrorFeatureContainer)topologyChecke;
                        IGeoDataset geoDatasetTopo = (IGeoDataset)topologyChecke;
                        ISpatialReference SpatialReference = geoDatasetTopo.SpatialReference;

                        IEnumTopologyErrorFeature enumTopologyErrorFeature = errorFeatureContainer.get_ErrorFeaturesByRuleType(SpatialReference, esriTopologyRuleType.esriTRTLineNoOverlap, geoDatasetTopo.Extent, true, false);
                        ITopologyErrorFeature topologyErrorFeature = null;
                        while ((topologyErrorFeature = enumTopologyErrorFeature.Next()) != null)
                        {
                            IFeature SelectFeature = inputFC.GetFeature(int.Parse(topologyErrorFeature.OriginOID + ""));
                            IFeature DestinationFeature = inputFC.GetFeature(int.Parse(topologyErrorFeature.DestinationOID + ""));
                            IFeature temp_Feature = topologyErrorFeature as IFeature;
                            errorsDic.Add(temp_Feature.Shape);
                        }

                        
                    }

                }
                catch (Exception ex)
                {                   
                    MessageBox.Show(ex.Message);
                    throw ex;
                }
                finally //删除临时数据
                {
                    if (simplifyFC != null)
                    {
                        (simplifyFC as IDataset).Delete();
                    }
                    if (smoothFcl != null)
                    {
                        (smoothFcl as IDataset).Delete();
                    }     
                   
                }

              
            }

            wo.Dispose();
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_Application.ActiveView.Extent);

            MessageBox.Show("化简完成!");

            if (errorsDic.Count > 0)
            {
                var errofrm = new Top.FrmTopErrors(errorsDic);
                errofrm.StartPosition = FormStartPosition.CenterParent;
                errofrm.ShowDialog();
            }
        }
        private bool RunTool(Geoprocessor geoprocessor, IGPProcess process, ITrackCancel TC = null)
        {
            bool geoResult = false;
            geoprocessor.OverwriteOutput = true;
            try
            {
                IGeoProcessorResult geoRe = (IGeoProcessorResult)geoprocessor.Execute(process, null); ;
                if (geoRe.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                     geoResult = true; 
                }
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
            return geoResult;
        }

        private void DrawSelePolyline(IPolyline pl)
        {
            IElement element = null;
            IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            gc.Reset();
            IElement ele = null;
            while ((ele = gc.Next()) != null)
            {
                IElementProperties ep1 = ele as IElementProperties;
                if (ep1.Name == "TraceSelLine")
                    m_Application.ActiveView.GraphicsContainer.DeleteElement(ele);
            }
            ILineElement lineElement = null;
            ISimpleLineSymbol lineSymbol = null;
            IElementProperties ep = null;
            //绘制选中的线
            // if (selPolyline != null)
            {
                lineElement = new LineElementClass();
                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                lineSymbol.Color = new RgbColorClass { Red = 255 };
                lineSymbol.Width = 2;
                lineElement.Symbol = lineSymbol;
                //
                element = lineElement as IElement;
                element.Geometry = pl;
                ep = element as IElementProperties;
                ep.Name = "TraceSelLine";
                gc.AddElement(element, 0);
            }
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
        }
        public void SetZValue(IGeometry geoSource, IGeometry pGeo)
        {
            IZAware pZAware0 = (IZAware)geoSource;
            if (pZAware0.ZAware)
            {
                IZAware pZAware = (IZAware)pGeo;
                pZAware.ZAware = true;
                IPointCollection pc = pGeo as IPointCollection;
                for (int i = 0; i < pc.PointCount; i++)
                {
                    ESRI.ArcGIS.Geometry.IPoint point = pc.get_Point(i);
                    point.Z = 0;
                }

            }
            else
            {
                IZAware pZAware = (IZAware)pGeo;
                pZAware.ZAware = false;
            }

            ////M值μ
            //if (pGeometryDef.HasM)
            //{
            //    IMAware pMAware = (IMAware)pGeo;
            //    pMAware.MAware = true;
            //}
            //else
            //{
            //    IMAware pMAware = (IMAware)pGeo;
            //    pMAware.MAware = false;

            //}
        }
    }
}

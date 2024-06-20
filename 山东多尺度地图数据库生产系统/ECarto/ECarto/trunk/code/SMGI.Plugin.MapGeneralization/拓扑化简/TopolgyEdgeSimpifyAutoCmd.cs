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
using SMGI.Common.Algrithm;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.CartographyTools;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF;

namespace SMGI.Plugin.MapGeneralization
{
    //化简
    public class TopolgyEdgeSimpifyAutoCmd: SMGI.Common.SMGICommand
    {
      
        List<string> _ruleNames = new List<string>();

        public TopolgyEdgeSimpifyAutoCmd()
        {
            m_caption = "拓扑边化简 自动";
            m_toolTip = "拓扑边化简 自动";
            m_category = "拓扑";

        }
        public override bool Enabled
        {
            get
            {
                
                return TopologyApplication.Topology != null && m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        double SMHeigth = 0;
        double SMWidth = 0;
        ITopology topgy = null;
        public override void OnClick()
        {
            var dlg = new FrmSimplify(SMHeigth,SMWidth);
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            SMHeigth = dlg.heigth;
            SMWidth = dlg.width;
            var pEditor = m_Application.EngineEditor;
            var wo = m_Application.SetBusy();
            List< IGeometry> errorsDic =new List< IGeometry>();

            #region
            //TopologyHelper helper1 = new TopologyHelper(m_Application.ActiveView);
            //var topgy1 = TopologyApplication.Topology;
            // IFeatureClassContainer fcContainer = TopologyApplication.Topology as IFeatureClassContainer;
            //    var targetfcl = fcContainer.get_Class(0);

            //    helper1.AddRuleToTopology(topgy1, esriTopologyRuleType.esriTRTAreaNoOverlap, "must not overlap", targetfcl);
            //    IGeoDataset geoDataset = (IGeoDataset)topgy1;
            //    IEnvelope envelope = geoDataset.Extent;
            //    helper1.ValidateTopology(topgy1, envelope);

            //    ITopology topologyChecke = CheckPolygonOverlap.OpenTopology((targetfcl.FeatureDataset.Workspace as IFeatureWorkspace), targetfcl.FeatureDataset.Name, TopologyApplication.TopName);
            //    IErrorFeatureContainer errorFeatureContainer = (IErrorFeatureContainer)topologyChecke;
            //    IGeoDataset geoDatasetTopo = (IGeoDataset)topologyChecke;
            //    ISpatialReference SpatialReference = geoDatasetTopo.SpatialReference;

            //    IEnumTopologyErrorFeature enumTopologyErrorFeature = errorFeatureContainer.get_ErrorFeaturesByRuleType(SpatialReference, esriTopologyRuleType.esriTRTLineNoOverlap, geoDatasetTopo.Extent, true, false);
            //    ITopologyErrorFeature topologyErrorFeature = null;
            //    while ((topologyErrorFeature = enumTopologyErrorFeature.Next()) != null)
            //    {
            //        IFeature SelectFeature = targetfcl.GetFeature(int.Parse(topologyErrorFeature.OriginOID + ""));
            //        IFeature DestinationFeature = targetfcl.GetFeature(int.Parse(topologyErrorFeature.DestinationOID + ""));

            //        IFeature temp_Feature = topologyErrorFeature as IFeature;

            //        errorsDic.Add(temp_Feature.Shape);


            //    }
            #endregion
            try
            {
               
                {
                    //wo.SetText("创建分区");
                    //IFeatureClassContainer fcContainer = TopologyApplication.Topology as IFeatureClassContainer;
                    //var targetfcl = fcContainer.get_Class(0);
                    //string inlyrs = (targetfcl as IDataset).Workspace.PathName + "\\" + targetfcl.FeatureDataset.Name + "\\" + targetfcl.AliasName;
                    //var gp = new Geoprocessor() { OverwriteOutput = true };
                    //MakeFeatureLayer gpLayer = new MakeFeatureLayer { in_features = inlyrs, out_layer = "Top_Layer" };
                    //ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult geoRe = (ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult)gp.Execute(gpLayer, null);
                    //ESRI.ArcGIS.Geoprocessing.IGPUtilities gpUtils = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
                    //if (geoRe.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                    //{

                    //    CreateCartographicPartitions gPpartition = new CreateCartographicPartitions();
                    //    gPpartition.in_features = "Top_Layer";
                    //    gPpartition.out_features = "Partitions";
                    //    gPpartition.feature_count = 500;
                    //    RunTool(gp, gPpartition);
                    //}
                    if (!(m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "Partitions"))
                    {
                        MessageBox.Show("不存在分区要素！");
                        return;
                    }
                    IFeatureClass parfcl = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass("Partitions");
                    IFeatureCursor fcursor=parfcl.Search(null,false);
                    IFeature fe = null;
                    int ct = parfcl.FeatureCount(null);
                    int indexflag = 0;
                    bool testflag = true;
                   
                    while((fe=fcursor.NextFeature())!=null)
                    {

                        string baseinfo = (indexflag++).ToString() + "/" + ct;
                        IEnvelope penvelope=null;

                        pEditor.StartOperation();

                        string msg = "正在获取拓扑边...";
                        int flag = 1;


                        TopologyHelper helper = new TopologyHelper(m_Application.ActiveView);
                        var topgy = TopologyApplication.Topology;

                        //var extent = (topgy.FeatureDataset as IGeoDataset).Extent;
                        var extent = fe.ShapeCopy.Envelope;

                        //// Get the dirty area within the provided envelope.
                        //IPolygon locationPolygon = new PolygonClass();
                        //ISegmentCollection segmentCollection = (ISegmentCollection)locationPolygon;
                        //segmentCollection.SetRectangle(extent);
                        //IPolygon polygon = topgy.get_DirtyArea(locationPolygon);

                        //// If a dirty area exists, validate the topology.
                        //IEnvelope areaValidated = null;
                        //if (!polygon.IsEmpty)
                        //{
                        //    // Define the area to validate and validate the topology.
                        //    IEnvelope areaToValidate = polygon.Envelope;
                        //     areaValidated = topgy.ValidateTopology(areaToValidate);
                        //}

                        IRelationalOperator rp = extent as IRelationalOperator;

                        ITopologyGraph graph = helper.CreateTopGraph(topgy, extent);
                        var edges = graph.Edges;
                        
                        
                        edges.Reset();
                        ITopologyEdge edge = null;
                        while ((edge = edges.Next()) != null)
                        {
                            #region
                            try
                            {
                                string info = (flag++).ToString() + "/" + edges.Count;
                                msg = "正在化简..." + info + "," + baseinfo;
                                wo.SetText(msg);

                                IPolyline line = (edge.Geometry as IClone).Clone() as IPolyline;
                                if (line.Length < SMWidth)
                                {
                                    continue;
                                }
                                if (rp.Disjoint(line))
                                    continue;


                                //第一根线
                                ICurve curve1 = null;
                                line.GetSubcurve(0, 0.5, true, out curve1);
                                IPointCollection path0 = new PathClass();
                                path0.AddPointCollection(curve1 as IPointCollection);
                                IGeometryCollection polyline1 = new PolylineClass();
                                polyline1.AddGeometry(path0 as IGeometry);
                                var pl1 = SimplifyByDTAlgorithm.SimplifyByDT(polyline1 as IPolycurve, SMHeigth, SMWidth);

                                //第二根线
                                ICurve curve2 = null;
                                line.GetSubcurve(0.5, 1, true, out curve2);
                                IPointCollection path1 = new PathClass();
                                path1.AddPointCollection(curve2 as IPointCollection);
                                IGeometryCollection polyline2 = new PolylineClass();
                                polyline2.AddGeometry(path1 as IGeometry);
                                var pl2 = SimplifyByDTAlgorithm.SimplifyByDT(polyline2 as IPolycurve, SMHeigth, SMWidth);

                                //合并两根线
                                IPointCollection pl = new PolylineClass();
                                pl.AddPointCollection(pl1 as IPointCollection);
                                pl.AddPointCollection(pl2 as IPointCollection);

                                

                                //对合并的线进行自相交检查
                                if (helper.IsSelfCross(pl as IGeometry))
                                {
                                    continue;
                                }
                                //若不存在自相交，则进行拓扑简化
                                (pl as ITopologicalOperator).Simplify();

                                //若化简后直线只存在两点，则直接跳过该种情况
                                if (pl.PointCount == 2)
                                {
                                    continue;
                                }
                                //draw line
                                //DrawSelePolyline(pl as IPolyline, new RgbColorClass() { Red = 0, Blue = 0, Green = 255 });
                                IPointCollection reshapePath = new PathClass();
                                reshapePath.AddPointCollection(pl as IPointCollection);
                                var path = reshapePath as IPath;
                                path.Smooth(0.1);
                                SetZValue(line, path);
                                //graph.SetEdgeGeometry(edge, path);
                                graph.ReshapeEdgeGeometry(edge, path);

                               

                                var parents = edge.Parents;

                                if (parents.Count == 2)
                                {
                                    int[] uid = new int[2];
                                    int i=0;
                                    esriTopologyParent p1=parents.Next();
                                    while ((p1.m_FID) != 0)
                                    {
                                        uid[i++] = p1.m_FID;
                                        p1 = parents.Next();                                      
                                    }

                                    if (uid.Contains(293) && uid.Contains(288))
                                    {
                                        DrawSelePolyline(line as IPolyline);
                                        testflag = false;
                                        break; 
                                    }
                                    
                                }

                                GC.Collect();
                            }
                            catch (Exception ex)
                            {
                                var geo = edge.Geometry;

                                //var parents = edge.Parents;
                                //parents.Reset();
                                //esriTopologyParent feature = parents.Next();
                                //{
                                //    var list =new List<IGeometry>();
                                //    if (errorsDic.ContainsKey(feature.m_pFC.AliasName))
                                //    {
                                //        list = errorsDic[feature.m_pFC.AliasName];
                                //    }

                                //    list.Add(geo);
                                //    errorsDic[feature.m_pFC.AliasName] = list;
                                //    System.Diagnostics.Trace.WriteLine("拓扑边化简错误ID：" + feature.m_FID);
                                //}

                                if (!errorsDic.Contains(geo))
                                {

                                    errorsDic.Add(geo);
                                }
                                pEditor.AbortOperation();
                                pEditor.StartOperation();
                                continue;
                            }

                            #endregion
                        }
                        wo.SetText("提交拓扑化简结果...");
                       
                        if (indexflag == 4)
                        {
                            testflag = false;
                        }
                        if (!testflag)
                        { break; }

                        graph.Post(out penvelope);
                        m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, penvelope);
                         
                        
                        pEditor.StopOperation("拓扑化简");
                      
                    }
                    wo.Dispose();
                    m_Application.ActiveView.Refresh();

                    ////拓扑检查重叠要素
                    //IFeatureClassContainer fcContainer = TopologyApplication.Topology as IFeatureClassContainer;
                    //var targetfcl = fcContainer.get_Class(0);
                    //errorsDic.AddRange(FinishedSelfIntersectChk(m_Application.Workspace.EsriWorkspace, targetfcl));

                }
            }
            catch(Exception  ex)
            {
                wo.Dispose();
                pEditor.AbortOperation();
                MessageBox.Show(ex.Message);
                return;
            }
            MessageBox.Show("化简完成!");
            if (errorsDic.Count > 0)
            {
                var frm = new Top.FrmTopErrors(errorsDic);
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog();
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
        public  void SetZValue(IGeometry geoSource, IGeometry pGeo)
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

        //public List<IGeometry> FinishedSelfIntersectChk( IFeatureClass fcls,ITopology topology)
        //{
            
        //    List<IGeometry> pListGeo = new List<IGeometry>();

        //    try
        //    {
        //        using (ComReleaser comReleaser = new ComReleaser())
        //        {                   
        //           // topology.AddClass(fcls, 5, 1, 1, false);
        //            CheckPolygonOverlap.AddRuleToTopology(topology, esriTopologyRuleType.esriTRTAreaNoOverlap, "Must Not Overlap", fcls);
        //            IGeoDataset geoDataset = (IGeoDataset)topology;
        //            IEnvelope envelope = geoDataset.Extent;
        //            CheckPolygonOverlap.ValidateTopology(topology, envelope);

        //            ITopology topologyChecke = CheckPolygonOverlap.OpenTopology((fcls.FeatureDataset.Workspace as IFeatureWorkspace), fcls.FeatureDataset.Name, "面重叠检查");
        //            IErrorFeatureContainer errorFeatureContainer = (IErrorFeatureContainer)topologyChecke;
        //            IGeoDataset geoDatasetTopo = (IGeoDataset)topologyChecke;
        //            ISpatialReference SpatialReference = geoDatasetTopo.SpatialReference;

        //            IEnumTopologyErrorFeature enumTopologyErrorFeature = errorFeatureContainer.get_ErrorFeaturesByRuleType(SpatialReference, esriTopologyRuleType.esriTRTLineNoOverlap, geoDatasetTopo.Extent, true, false);
        //            ITopologyErrorFeature topologyErrorFeature = null;
        //            while ((topologyErrorFeature = enumTopologyErrorFeature.Next()) != null)
        //            {
        //                IFeature SelectFeature = fcls.GetFeature(int.Parse(topologyErrorFeature.OriginOID + ""));
        //                IFeature DestinationFeature = fcls.GetFeature(int.Parse(topologyErrorFeature.DestinationOID + ""));

        //                IFeature temp_Feature = topologyErrorFeature as IFeature;

        //                pListGeo.Add(temp_Feature.Shape);


        //            }
        //            //删除临时拓扑
        //            topology.RemoveClass(fcls);
        //            (topology as IDataset).Delete();

        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    return pListGeo;
        //}


        private IPointCollection simplifyLine(IPolyline line)
        {
            //第一根线
            ICurve curve1 = null;
            line.GetSubcurve(0, 0.5, true, out curve1);
            IPointCollection path0 = new PathClass();
            path0.AddPointCollection(curve1 as IPointCollection);
            IGeometryCollection polyline1 = new PolylineClass();
            polyline1.AddGeometry(path0 as IGeometry);
            var pl1 = SimplifyByDTAlgorithm.SimplifyByDT(polyline1 as IPolycurve, SMHeigth, SMWidth);

            //第二根线
            ICurve curve2 = null;
            line.GetSubcurve(0.5, 1, true, out curve2);
            IPointCollection path1 = new PathClass();
            path1.AddPointCollection(curve2 as IPointCollection);
            IGeometryCollection polyline2 = new PolylineClass();
            polyline2.AddGeometry(path1 as IGeometry);
            var pl2 = SimplifyByDTAlgorithm.SimplifyByDT(polyline2 as IPolycurve, SMHeigth, SMWidth);

            //合并两根线
            IPointCollection pl = new PolylineClass();
            pl.AddPointCollection(pl1 as IPointCollection);
            pl.AddPointCollection(pl2 as IPointCollection);

            return pl;
        }
        
    }
}

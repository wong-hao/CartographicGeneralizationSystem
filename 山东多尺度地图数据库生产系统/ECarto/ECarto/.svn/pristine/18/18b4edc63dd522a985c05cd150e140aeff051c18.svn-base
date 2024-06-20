using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using System.IO;
using ESRI.ArcGIS.DataSourcesRaster;
using  SMGI.Common;
using SMGI.Common.Algrithm;
using System.Windows.Forms;
namespace SMGI.Plugin.MapGeneralization
{
    //拓扑全局变量
    public static class TopologyApplication
    {
        public static ITopology Topology = null;//全局变量
        public static string TopName = "";
        public static IPolyline SelPolyline = null;//选中的拓扑边
        public static ITopologyEdge SelEdge = null;//选中的拓扑边
        public static List<ITopologyEdge> SelEdges = null;//选中的拓扑边
        public static List<IGeometry> SelPolylines = null;//选中的拓扑边
    }
    //创建拓扑
    public  class TopologyHelper
    {
        private IActiveView pAc;
        private IEngineEditor pEditor=null;

        public TopologyHelper(IActiveView pAc_)
        {
            pAc = pAc_;
            pEditor =new EngineEditorClass();
        }
        /// <summary>
        /// 删除所有构件的拓扑
        /// </summary>
        public void TopologyDelete()
        {
            Dictionary<string, IFeatureDataset> featureds = new Dictionary<string, IFeatureDataset>();
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IFeatureLayer;
            })).ToArray();
            for (int i = 0; i < lyrs.Length; i++)
            {
                ILayer player = lyrs[i];
                if (player is IFeatureLayer && (player as IFeatureLayer).FeatureClass != null)
                {
                    if ((player as IFeatureLayer).FeatureClass != null)
                    {
                        IFeatureDataset pfds = (player as IFeatureLayer).FeatureClass.FeatureDataset;
                        if (pfds != null)
                        {

                            featureds[pfds.Name] = pfds;
                        }
                    }
                }
            }
            foreach (var kv in featureds)
            {
                var featureDataset = kv.Value;
                ITopology topology = null;
                // Attempt to acquire an exclusive schema lock on the feature dataset.
                ISchemaLock schemaLock = (ISchemaLock)kv.Value ;
                try
                {
                    schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

                    // delete the topology.
                    ITopologyContainer2 topologyContainer = (ITopologyContainer2)featureDataset;
                    for (int i=0; i < topologyContainer.TopologyCount; i++)
                    {
                        topology = topologyContainer.get_Topology(i);
                        if (topology != null)
                        {
                            (topology as IDataset).Delete();
                        }
                    }
                }
                catch (COMException comExc)
                {

                    throw new Exception(String.Format(
                        "Error creating topology: {0} Message: {1}", comExc.ErrorCode,
                        comExc.Message), comExc);

                }
                finally
                {
                    schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);

                }
            }
        }
        //创建拓扑
        public ITopology CreateTopology(string topolgyName, IFeatureDataset featureDataset)
        {
            ITopology topology = null;
            // Attempt to acquire an exclusive schema lock on the feature dataset.
            ISchemaLock schemaLock = (ISchemaLock)featureDataset;
            try
            {
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

                // Create the topology.
                ITopologyContainer2 topologyContainer = (ITopologyContainer2)featureDataset;
               
                topology=  GetTopByName(topologyContainer, topolgyName);
                if (topology != null)
                {
                    (topology as IDataset).Delete();
                }
                //重新创建
                topology = topologyContainer.CreateTopology(topolgyName, topologyContainer.DefaultClusterTolerance,  - 1, "");
                return topology;
            }
            catch (COMException comExc)
            {
               
                throw new Exception(String.Format(
                    "Error creating topology: {0} Message: {1}", comExc.ErrorCode,
                    comExc.Message), comExc);
                
            }
            finally
            {
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
               
            }
           
        }
        private ITopology GetTopByName(ITopologyContainer2 topologyContainer, string topolgyName)
        {
            ITopology top = null;
            try
            {

                top = topologyContainer.get_TopologyByName(topolgyName);
                return top;
            }
            catch (Exception ex)
            {
                return top;
            }
            
        }
        //创建拓扑图
        public ITopologyGraph CreateTopGraph(ITopology topology)
        {
            try
            {
               
                ITopologyGraph graph = topology.Cache;
              
                IEnvelope existEnvelope = graph.BuildExtent;
                IEnvelope penvelope = pAc.Extent;
                if (!existEnvelope.IsEmpty)
                {
                    IClone pgraphEn = existEnvelope as IClone;
                    IClone actview = penvelope as IClone;
                    if ((pgraphEn as IRelationalOperator).Contains(penvelope))
                    {
                        return graph;
                    }
                    if (!actview.IsEqual(pgraphEn))
                    {
                        graph.Build(pAc.Extent, false);
                    }
                   
                }
                else
                {
                    graph.Build(pAc.Extent, false);
                }
                GC.Collect();
                return graph;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ITopologyGraph CreateTopGraph(ITopology topology,IEnvelope en)
        {
            try
            {

                ITopologyGraph graph = topology.Cache;

                IEnvelope existEnvelope = graph.BuildExtent;
                IEnvelope penvelope = en;
                graph.Build(penvelope, false);
              
                GC.Collect();
                return graph;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
       
        /// <summary>
        /// 查询公共拓扑边
        /// </summary>
        public IGeometry QueryTopEdge(ITopologyGraph topology, IGeometry querygeo)
        {
            IGeometry pgeo = null;
            topology.SelectByGeometry((int)esriTopologyElementType.esriTopologyEdge, esriTopologySelectionResultEnum.esriTopologySelectionResultNew, querygeo);
            IEnumTopologyEdge enumEdges = topology.EdgeSelection;
            enumEdges.Reset();
            ITopologyEdge edge = null;
            edge = enumEdges.Next();
            if (edge != null)
            {
                pgeo = edge.Geometry;
            }
            TopologyApplication.SelEdge = edge;
            Marshal.ReleaseComObject(enumEdges);
            return pgeo;

        }

        public List<IGeometry> QueryTopEdges(ITopologyGraph topology, IGeometry querygeo)
        {
            var geoEdges = new List<ITopologyEdge>();
            var geos = new List<IGeometry>();

           
            topology.SelectByGeometry((int)esriTopologyElementType.esriTopologyEdge, esriTopologySelectionResultEnum.esriTopologySelectionResultNew, querygeo);
            IEnumTopologyEdge enumEdges = topology.EdgeSelection;
            enumEdges.Reset();
            ITopologyEdge edge = null;
            while(( edge = enumEdges.Next())!=null)
            {
                geoEdges.Add(edge);
                geos.Add(edge.Geometry);
            }
            TopologyApplication.SelEdges = geoEdges;
            Marshal.ReleaseComObject(enumEdges);
          
            return geos;

        }
     
        public void QueryTopEle(ITopologyGraph topology,IGeometry querygeo,double height,double width)
        {
            //esriTopologyNode 1  
            //esriTopologyEdge 2  
            //esriTopologyFace 4 

            topology.SelectByGeometry((int)esriTopologyElementType.esriTopologyEdge, esriTopologySelectionResultEnum.esriTopologySelectionResultNew, querygeo);
            IEnumTopologyEdge enumEdges = topology.EdgeSelection;
            enumEdges.Reset();
            ITopologyEdge edge=null;
            IEnvelope penvelope ;
            try
            {
                pEditor.StartOperation();

                while ((edge = enumEdges.Next()) != null)
                {

                    IGeometry pgeo = edge.Geometry;
                    #region


                    //IEnumTopologyParent leftParents = edge.get_LeftParents(true);
                    //leftParents.Reset();
                    //esriTopologyParent leftParent = leftParents.Next();
                    //int leftID = leftParent.m_FID;

                    //IEnumTopologyParent rightParents = edge.get_RightParents(true);
                    //rightParents.Reset();
                    //esriTopologyParent rightParent = rightParents.Next();
                    //int rightID = rightParent.m_FID;
                    //IFeatureClass fc = rightParent.m_pFC;
                    IPolyline line = (pgeo as IClone).Clone() as IPolyline;
                    var pl = SimplifyByDTAlgorithm.SimplifyByDT(line as IPolycurve, height, width);

                    //化简
                    //IPointCollection path = new PathClass();
                    //path.AddPoint(line.FromPoint);
                    //double x = (line.FromPoint.X + line.ToPoint.X) / 2;
                    //double y = (line.FromPoint.Y + line.ToPoint.Y) / 2;
                    //path.AddPoint(new PointClass() {X=x,Y=y });
                    //path.AddPoint(line.ToPoint);


                    IPointCollection reshapePath = new PathClass();
                    reshapePath.AddPointCollection(pl as IPointCollection);

                    // DrawLine(pgeo as IPolyline);
                    topology.SetEdgeGeometry(edge, reshapePath as IPath);
                    // topology.ReshapeEdgeGeometry(edge, path as IPath);
                    topology.Post(out penvelope);
                    #endregion
                }
                Marshal.ReleaseComObject(enumEdges);
                GC.Collect();
                pEditor.StopOperation("拓扑化简");
            }
            catch (Exception ex)
            {
                MessageBox.Show("拓扑化简报错："+ex.Message);
            }

        }

        private  void DrawLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 10.02;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = 255;
                //rgb.Blue = 122;
                //rgb.Green = 122;
                linesym.Color = rgb;
                polygonElement.Symbol = linesym;
                pEl = polygonElement as IElement;
                pEl.Geometry = pline as IGeometry;
                pContainer.AddElement(pEl, 0);
                pAc.Refresh();

            }
            catch
            {

            }
        }
        public void ValidateTopology(ITopology topology, IEnvelope envelope)
        {
            // Get the dirty area within the provided envelope.
            IPolygon locationPolygon = new PolygonClass();
            ISegmentCollection segmentCollection = (ISegmentCollection)locationPolygon;
            segmentCollection.SetRectangle(envelope);
            IPolygon polygon = topology.get_DirtyArea(locationPolygon);

            // If a dirty area exists, validate the topology.
            if (!polygon.IsEmpty)
            {
                // Define the area to validate and validate the topology.
                IEnvelope areaToValidate = polygon.Envelope;
                IEnvelope areaValidated = topology.ValidateTopology(areaToValidate);
            }
        }


        public void AddRuleToTopology(ITopology topology, esriTopologyRuleType ruleType, String ruleName, IFeatureClass featureClass)
        {
            // Create a topology rule.
            ITopologyRule topologyRule = new TopologyRuleClass();
            topologyRule.TopologyRuleType = ruleType;
            topologyRule.Name = ruleName;
            topologyRule.OriginClassID = featureClass.FeatureClassID;
            topologyRule.AllOriginSubtypes = true;

            // Cast the topology to the ITopologyRuleContainer interface and add the rule.
            ITopologyRuleContainer topologyRuleContainer = (ITopologyRuleContainer)topology;
            if (topologyRuleContainer.get_CanAddRule(topologyRule))
            {
                topologyRuleContainer.AddRule(topologyRule);
            }
            else
            {
                throw new ArgumentException("Could not add specified rule to the topology.");
            }
        }

        public void RemoveAllClass(ITopology pTopology)
        {
            IFeatureClassContainer fcContainer = pTopology as IFeatureClassContainer;
            List<IFeatureClass> fcls=new List<IFeatureClass>();
            try
            {
                for (int i = 0; i < fcContainer.ClassCount; i++)
                {
                    fcls.Add(fcContainer.get_Class(i));
                }
                foreach (var fcl in fcls)
                {
                    pTopology.RemoveClass(fcl);
                }
                 
            }
            catch(Exception ex)
            {
                MessageBox.Show("删除拓扑要素类失败！"+ex.Message);
            }
        }

        public void AddClass(ITopology pTopology, IFeatureClass fc)
        {
            
                IFeatureClassContainer fcContainer = pTopology as IFeatureClassContainer;
        
                IFeatureClass fcExist = null;
                try
                {

                    fcExist = fcContainer.get_ClassByName(fc.AliasName);
                }
                catch
                {
                    fcExist = null;
                }
                finally
                {
                    if (fcExist == null)
                    {
                        pTopology.AddClass(fc, 5, 1, 1, false);
                    }
                }
                //IEnumFeatureClass enumfc = fcContainer.Classes;
                //enumfc.Reset();
                //IFeatureClass fc = null;
                //while ((fc = enumfc.Next()) != null)
                //{
                //    list.Add(fc.AliasName);
                //}
                //Marshal.ReleaseComObject(enumfc);

            
          
        }

        public bool IsSelfCross(IGeometry pG)
        {
            ITopologicalOperator3 pTopologicalOperator3 = pG as ITopologicalOperator3;
            pTopologicalOperator3.IsKnownSimple_2 = false;
            esriNonSimpleReasonEnum reason = esriNonSimpleReasonEnum.esriNonSimpleOK;

            if (!pTopologicalOperator3.get_IsSimpleEx(out reason))
            {
                if (reason == esriNonSimpleReasonEnum.esriNonSimpleSelfIntersections)
                {
                    return true;
                }
            }

            return false;
        }

    }
}

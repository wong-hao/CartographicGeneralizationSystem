using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace BuildingGen {
    public class RoadTopoConnect : BaseGenTool {
        public RoadTopoConnect() {
            base.m_category = "GRoad";
            base.m_caption = "掐头去尾";
            base.m_message = "对道路进行掐头去尾";
            base.m_toolTip = "对道路进行掐头去尾";
            base.m_name = "RoadTopoConnect";
            base.m_usedParas = new GenDefaultPara[]
            {
                new GenDefaultPara("最小道路出头长度",(double)50)
                //,new GenDefaultPara("建筑物最小洞面积",(double)100)
                //,new GenDefaultPara("建筑物融合_最小保留洞面积",(double)0)
            };
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }

        public override void OnClick() {
            GLayerInfo layer = GetLayer();
            if (layer == null)
                return;
            if (!IsInTopo(layer)) {
                CreateTopo(layer);
            }
            ITopology topo = GetTopo(layer);

            IWorkspaceEdit edit = m_application.Workspace.Workspace as IWorkspaceEdit;
            edit.StartEditing(false);
            edit.StartEditOperation();

            try {
                topo.ValidateTopology(m_application.MapControl.Extent);
            }
            catch { 
            }
            ITopologyGraph graph = topo.Cache;
            graph.Build(m_application.MapControl.Extent, false);
            IEnumTopologyEdge edges = graph.Edges;
            List<ITopologyEdge> edgeToBeDelete = new List<ITopologyEdge>();
            double minLength = (double)m_application.GenPara["最小道路出头长度"];
            for (int i = 0; i < edges.Count; i++) {
                ITopologyEdge edge = edges.Next();
                if (edge.FromNode.Parents.Count > 1 && edge.ToNode.Parents.Count > 1) {
                    continue;
                }
                //ITopologyNode node = edge.FromNode;
                //if (node.Parents.Count <= 1) {
                //    IPoint point = node.Geometry as IPoint;
                //    IPolygon bufferArea = (point as ITopologicalOperator).Buffer(minLength) as IPolygon;

                //}
                if ((edge.Geometry as IPolycurve).Length < 50) {
                    edgeToBeDelete.Add(edge);
                    continue;
                }
            }
            List<esriTopologyParent> fidToBeDelete = new List<esriTopologyParent>();

            foreach (var item in edgeToBeDelete) {
                if (item.Parents.Count > 1) {
                    graph.DeleteEdge(item);
                    continue;
                }
                esriTopologyParent par = item.Parents.Next();
                if (graph.GetParentEdges(par.m_pFC, par.m_FID).Count > 1) {
                    graph.DeleteEdge(item);
                }
                fidToBeDelete.Add(par);
            }
            IEnvelope env = null;
            try {
                graph.Post(out env);
            }
            catch { 
            }
            foreach (var item in fidToBeDelete) {
                try {
                    IFeature feature = item.m_pFC.GetFeature(item.m_FID);
                    feature.Delete();
                }
                catch {
                    int i = 0;
                }
            }
            edit.StopEditOperation();
            edit.StopEditing(true);
            m_application.MapControl.Refresh();
        }

        private bool IsInTopo(GLayerInfo layer) {
            try {
                IFeatureLayer flayer = layer.Layer as IFeatureLayer;
                ITopologyClass topoClass = flayer.FeatureClass as ITopologyClass;
                return topoClass.IsInTopology;
            }
            catch {
                return false;
            }
        }

        private ITopology GetTopo(GLayerInfo layer) {
            return ((layer.Layer as IFeatureLayer).FeatureClass as ITopologyClass).Topology;
        }

        private ITopology CreateTopo(GLayerInfo layer) {
            IFeatureLayer flayer = layer.Layer as IFeatureLayer;
            IFeatureWorkspace workspace = (flayer as IDataset).Workspace as IFeatureWorkspace;
            //IFeatureWorkspace workspace = m_application.Workspace.Workspace as IFeatureWorkspace;
            IFeatureDataset dataset = null;
            if ((workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureDataset, "dsRoad")) {
                dataset = workspace.OpenFeatureDataset("dsRoad");
            }
            else {
                dataset = workspace.CreateFeatureDataset("dsRoad", (flayer.FeatureClass as IGeoDataset).SpatialReference);
                (dataset as ISchemaLock).ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
                (dataset as IDatasetContainer).AddDataset(flayer.FeatureClass as IDataset);
            }
            
            ITopology topo = null;
            if ((workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTopology, "RoadTopo")) {
                topo =  (dataset as ITopologyContainer).get_TopologyByName("RoadTopo");
            }
            else {
                topo = (dataset as ITopologyContainer).CreateTopology("RoadTopo", (dataset as ITopologyContainer).DefaultClusterTolerance, -1, "");
            }
            try {
                IFeatureClass fc = workspace.OpenFeatureClass((flayer.FeatureClass as IDataset).Name);
                topo.AddClass(fc, 1, 1, 1, false);
                (dataset as ISchemaLock).ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
                flayer.FeatureClass = fc;
            }
            catch (Exception ex){ 
                
            }
            //topo.ValidateTopology(flayer.AreaOfInterest);
            return topo;

        }
        private GLayerInfo GetLayer() {
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.道路
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    ) {
                    return info;
                }
            }
            return null;
        }
    }
}

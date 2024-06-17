using System;
using System.Collections.Generic;
using System.Text;
using BuildingGen.Road;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

namespace BuildingGen {
  class ConnectRoadEx : BaseGenCommand {
    public ConnectRoadEx() {
      base.m_category = "GRoad";
      base.m_caption = "去除伪节点";
      base.m_message = "去除伪节点";
      base.m_toolTip = "去除道路中的伪节点。";
      base.m_name = "ConnectRoadEx";
      m_usedParas = new GenDefaultPara[] 
            {
                new GenDefaultPara("伪节点判断等级",(int)1)
            };
    }
    public override bool Enabled {
      get {
        return m_application.Workspace != null;
      }
    }
    public override void OnClick() {
      GLayerInfo info = null;
      foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers) {
        if (tempInfo.LayerType == GCityLayerType.道路
            && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
            && tempInfo.OrgLayer != null
            ) {
          info = tempInfo;
          break;
        }
      }
      if (info == null) {
        System.Windows.Forms.MessageBox.Show("没有找到道路中心线图层");
        return;
      }
      IFeatureLayer layer = info.Layer as IFeatureLayer;
      IFeatureClass fc = layer.FeatureClass;
      IQueryFilter qf = new QueryFilterClass();
      qf.WhereClause = "道路等级 <= " + m_application.GenPara["伪节点判断等级"].ToString();
      Graph graph = new Graph(fc,qf);
      List<GraphNode> nodes = graph.GetPseudoNode();

      //int nameField = fc.FindField("道路名");
      int rankID = fc.FindField("道路等级");
      //int widthID = fc.FindField("宽度");

      Dictionary<int, int> connectDic = new Dictionary<int, int>();
      List<List<GraphNode>> groups = new List<List<GraphNode>>();
      
      foreach (var item in nodes) {
        if (connectDic.ContainsKey(item.index))
          continue;
        List<GraphNode> group = new List<GraphNode>();
        int index = groups.Count;
        groups.Add(group);
        connectDic.Add(item.index, index);
        group.Add(item);
        Action<int> findother = null;
        findother = (roadid) => {
          int fid = GraphNode.GetOID(roadid);
          bool isFrom = GraphNode.GetIsFromPoint(roadid);
          int otherID = GraphNode.ConvertOID(fid, !isFrom);
          int connectNode = graph.linesNode[otherID];
          GraphNode otherNode = graph.nodes[connectNode];
          if (connectDic.ContainsKey(otherNode.index))
            return;
          if (otherNode.conectFeatures.Count != 2)
            return;
          connectDic.Add(otherNode.index, index);
          group.Add(otherNode);
          foreach (var rid in otherNode.conectFeatures) {
            if (rid == roadid) {
              continue;
            }
            findother(rid);
          }
        };
        
        foreach (var roadid in item.conectFeatures) {
          findother(roadid);
        }
      }
      Comparison<IFeature> comp = (f1, f2) => {
        int rank1 = (int)f1.get_Value(rankID);
        int rank2 = (int)f2.get_Value(rankID);
        if (rank1 == rank2) {
          return (f1.Shape as IPolyline).Length.CompareTo((f2.Shape as IPolyline).Length);
        }
        return rank2.CompareTo(rank1);
      };
      foreach (var group in groups) {
        Dictionary<int, IFeature> features = new Dictionary<int, IFeature>();
        foreach (var node in group) {
          foreach (var roadid in node.conectFeatures) {
            int fid = GraphNode.GetOID(roadid);
            if (features.ContainsKey(fid))
              continue;
            IFeature feature = fc.GetFeature(fid);
            features.Add(fid, feature);
          }
        }
        IFeature maxFeature = null;
        ITopologicalOperator op = null;
        foreach (var feature in features) {
          if (maxFeature == null) {
            maxFeature = feature.Value;
            op = maxFeature.ShapeCopy as ITopologicalOperator;
            continue;
          }
          op = op.Union(feature.Value.Shape) as ITopologicalOperator;
          if (comp(feature.Value, maxFeature) > 0) {
            maxFeature.Delete();
            maxFeature = feature.Value;
          }
          else {
            feature.Value.Delete();
          }
        }
        maxFeature.Shape = op as IGeometry;
        maxFeature.Store();
      }
      m_application.MapControl.Refresh();
    }
  }
}

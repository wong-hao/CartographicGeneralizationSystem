using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace BuildingGen.Road {
  internal class Graph {
    internal SortedDictionary<PointWarp, GraphNode> nodesDic;
    internal List<GraphNode> nodes;
    /// <summary>
    /// 索引为FID，正数代表从起始点(from)，负数代表从终止点(to),0对应int.MinValue
    /// 值为对应的Node的索引
    /// </summary>
    internal Dictionary<int, int> linesNode;

    internal Dictionary<int, GraphEdge> edges;
    internal double TotalLenght { get; private set; }

    IFeatureClass featureClass;
    internal Graph(IFeatureClass fc, IQueryFilter qf) {
      nodesDic = new SortedDictionary<PointWarp, GraphNode>();
      nodes = new List<GraphNode>();
      linesNode = new Dictionary<int, int>();
      edges = new Dictionary<int, GraphEdge>();

      TotalLenght = 0;
      featureClass = fc;
      IFeatureCursor fCursor = featureClass.Search(qf, true);
      IFeature feature = null;
      //IComparer<IPoint> comp = (p1, p2) => { return p1.Compare(p2); };
      while ((feature = fCursor.NextFeature()) != null) {
        IPolyline line = feature.Shape as IPolyline;
        TotalLenght += line.Length;
        for (int i = 0; i < 2; i++) {
          IPoint p = (i == 0) ? line.FromPoint : line.ToPoint;
          GraphNode node = null;
          PointWarp pw = new PointWarp(p);
          if (!nodesDic.ContainsKey(pw)) {
            node = new GraphNode(this);
            node.index = nodes.Count;
            node.point = p;
            node.AddFeature(feature.OID, (i == 0));
            nodes.Add(node);
            nodesDic.Add(pw, node);
          }
          else {
            node = nodesDic[pw];
            node.AddFeature(feature.OID, (i == 0));
          }
          linesNode.Add(GraphNode.ConvertOID(feature.OID, (i == 0)), node.index);
        }
      }
      System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
    }
    internal Graph(IFeatureClass fc) {
      nodesDic = new SortedDictionary<PointWarp, GraphNode>();
      nodes = new List<GraphNode>();
      linesNode = new Dictionary<int, int>();
      edges = new Dictionary<int, GraphEdge>();

      TotalLenght = 0;
      featureClass = fc;
      IFeatureCursor fCursor = featureClass.Search(null, true);
      IFeature feature = null;
      //IComparer<IPoint> comp = (p1, p2) => { return p1.Compare(p2); };
      while ((feature = fCursor.NextFeature()) != null) {
        IPolyline line = feature.Shape as IPolyline;
        TotalLenght += line.Length;
        for (int i = 0; i < 2; i++) {
          IPoint p = (i == 0) ? line.FromPoint : line.ToPoint;
          GraphNode node = null;
          PointWarp pw = new PointWarp(p);
          if (!nodesDic.ContainsKey(pw)) {
            node = new GraphNode(this);
            node.index = nodes.Count;
            node.point = p;
            node.AddFeature(feature.OID, (i == 0));
            nodes.Add(node);
            nodesDic.Add(pw, node);
          }
          else {
            node = nodesDic[pw];
            node.AddFeature(feature.OID, (i == 0));
          }
          linesNode.Add(GraphNode.ConvertOID(feature.OID, (i == 0)), node.index);
        }
      }
      System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
    }
    /// <summary>
    /// 移除一条线
    /// </summary>
    /// <param name="lineFID">要被移除的线的id</param>
    internal void RemoveLine(int lineFID) {
      try {
        int fromID = GraphNode.ConvertOID(lineFID, true);
        int toID = GraphNode.ConvertOID(lineFID, false);
        int fromNodeIndex = this.linesNode[fromID];
        int toNodeIndex = this.linesNode[toID];
        this.nodes[fromNodeIndex].conectFeatures.Remove(fromID);
        this.nodes[toNodeIndex].conectFeatures.Remove(toID);
        this.linesNode.Remove(fromID);
        this.linesNode.Remove(toID);
      }
      catch { 
      }
    }
    internal List<GraphNode> GetHangNode() {
      List<GraphNode> re = new List<GraphNode>();
      foreach (var item in nodes) {
        if (item.conectFeatures.Count <= 1) {
          re.Add(item);
        }
      }
      return re;
    }
    internal List<GraphNode> GetPseudoNode() {
      List<GraphNode> re = new List<GraphNode>();
      foreach (var item in nodes) {
        if (item.conectFeatures.Count == 2) {
          re.Add(item);
        }
      }
      return re;
    }
  }
  internal class GraphNode : IComparable {
    internal List<int> conectFeatures;  //记录FID，正数代表从起始点(from)，负数代表从终止点(to),0对应int.MinValue
    internal IPoint point;              //记录点位
    internal int index;                 //记录索引
    internal Graph graph;
    internal GraphNode(Graph graph) {
      conectFeatures = new List<int>();
      point = null;
      this.graph = graph;
      this.index = graph.nodes.Count;
    }

    internal void AddFeature(int oid, bool asFrom) {
      conectFeatures.Add(ConvertOID(oid, asFrom));
    }
    internal static int ConvertOID(int oid, bool asFrom) {
      if (oid == 0) {
        return (asFrom ? 0 : int.MinValue);
      }
      else {
        return (asFrom ? oid : -oid);
      }
    }
    internal static int GetOID(int oid) {
      if (oid == int.MinValue)
        return 0;
      else
        return Math.Abs(oid);
    }
    internal static bool GetIsFromPoint(int oid) {
      if (oid == int.MinValue)
        return false;
      else
        return oid >= 0;
    }
    internal static int GetReversalIndex(int oid) {
      if (oid == int.MinValue)
        return 0;
      else if (oid == 0)
        return int.MinValue;
      else
        return -oid;
    }
    #region IComparable 成员

    public int CompareTo(object obj) {
      if (obj is GraphNode)
        return this.point.Compare((obj as GraphNode).point);
      if (obj is IPoint)
        return this.point.Compare(obj as IPoint);
      throw new NotImplementedException("没有实现相关类型的比较");
    }

    #endregion
  }

  internal class PointWarp : IComparable {
    IPoint point;
    internal PointWarp(IPoint p) {
      this.point = p;
    }


    #region IComparable 成员

    public int CompareTo(object obj) {
      if (obj is PointWarp)
        return this.point.Compare((obj as PointWarp).point);
      if (obj is IPoint)
        return this.point.Compare(obj as IPoint);
      throw new NotImplementedException("没有实现相关类型的比较");

    }

    #endregion
  }

  internal class GraphEdge {
    internal int FID;
    internal IPoint fromPoint;
    internal IPoint toPoint;
    internal GraphEdge(int fid, IPoint fromPoint, IPoint toPoint) { 

    }
  }
}

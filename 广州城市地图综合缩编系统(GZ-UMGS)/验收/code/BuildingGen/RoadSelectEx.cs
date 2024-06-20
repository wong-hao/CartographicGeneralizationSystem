using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using BuildingGen.Road;

namespace BuildingGen {
  class RoadSelectEx:BaseGenCommand {
    public RoadSelectEx() {
      base.m_category = "GRoad";
      base.m_caption = "道路选取";
      base.m_message = "对道路进行选取";
      base.m_toolTip = "对道路进行选取，按照选取比例，并保持密度";
      base.m_name = "RoadSelectEx";
      base.m_usedParas = new GenDefaultPara[]{
        new GenDefaultPara("道路选取比例",(double)0.3)
        ,new GenDefaultPara("道路锁定长度",(double)200)
      };
    }
    public override bool Enabled {
      get {
        return m_application.Workspace != null;
      }
    }

    public override void OnClick() {
#if DEBUG
      IDisplay dis = m_application.MapControl.ActiveView.ScreenDisplay;
      SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
      RgbColorClass deleteCorlor = new RgbColorClass();
      deleteCorlor.Red = 0;deleteCorlor.Blue = 0;deleteCorlor.Green = 255;
      RgbColorClass saveColor = new RgbColorClass();
      saveColor.Green = 0; saveColor.Red = 255; saveColor.Blue = 0;
      sls.Width = 2;
#endif

      #region 0 检查工作
      GLayerInfo roadLayer = GetLayer();
      if (roadLayer == null) {
        System.Windows.Forms.MessageBox.Show("缺少道路中心线图层！");
        return;
      }

      IFeatureClass fc = (roadLayer.Layer as IFeatureLayer).FeatureClass;
      double densifyDistance = 1;
      int rankID = fc.Fields.FindField("道路等级");
      int strokeID = fc.Fields.FindField("道路分组");
      if (rankID == -1 || strokeID == -1) {
        System.Windows.Forms.MessageBox.Show("没有计算道路等级！");
        return;
      }

      int usedID = fc.FindField("_GenUsed");
      if (usedID == -1) {
        IFieldEdit2 field = new FieldClass();
        field.Name_2 = "_GenUsed";
        field.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
        fc.AddField(field as IField);
        usedID = fc.Fields.FindField("_GenUsed");
      }

      int allRank = (int)m_application.GenPara["道路完全选取等级"];
      double selectRate = (double )m_application.GenPara["道路选取比例"];

      IFeatureLayer flayer = (roadLayer.Layer as IFeatureLayer);
      IFeatureLayerDefinition fdefinition = flayer as IFeatureLayerDefinition;
      fdefinition.DefinitionExpression = "";
      #endregion

      #region 1 建立图关系
      Graph graph = new Graph(fc);
      #endregion

      #region 2 建立三角网(节点的tag值是RoadInfo的索引),顺便算出总长度
      double allLength = 0;
      double lockLenght = (double)m_application.GenPara["道路锁定长度"];
      TinClass tin = new TinClass();
      
      tin.InitNew(flayer.AreaOfInterest);
      tin.StartInMemoryEditing();

      List<RoadInfo> roadInfos = new List<RoadInfo>();
      ///key是strokeid，value是strokeInfo
      Dictionary<int, StrokeInfo> strokeDic= new Dictionary<int, StrokeInfo>();
      List<StrokeInfo> sortStroke = new List<StrokeInfo>();
      IFeatureCursor buildTinCursor = fc.Search(null, false);
      IFeature buildTinFeature = null;
      while ((buildTinFeature = buildTinCursor.NextFeature()) != null) {
        int sID = (int)buildTinFeature.get_Value(strokeID);
        int rank = (int)buildTinFeature.get_Value(rankID);
        IPolyline line = buildTinFeature.ShapeCopy as IPolyline;
        

        RoadInfo roadInfo = new RoadInfo(buildTinFeature.OID, sID, rank, roadInfos.Count,line.Length);

        roadInfos.Add(roadInfo);
        StrokeInfo strokeInfo = null;
        int strokeKey = StrokeInfo.GetStrokeID(roadInfo);
        if (strokeDic.ContainsKey(strokeKey)) {
          strokeInfo = strokeDic[strokeKey];
          strokeInfo.RoadInfos.Add(roadInfo);
        }
        else {
          strokeInfo = new StrokeInfo(roadInfo);
          sortStroke.Add(strokeInfo);
          strokeDic.Add(strokeKey, strokeInfo);
        }
        if (roadInfo.Rank <= allRank) {
          strokeInfo.State = RoadState.HighRank;
        }
        else {
          allLength += line.Length;
        }

        line.Densify(densifyDistance, 0);
        for (int i = 0; i < (line as IPointCollection).PointCount; i++) {
          IPoint p = (line as IPointCollection).get_Point(i);
          p.Z = 0;
          ITinNode node = new TinNodeClass();
          tin.AddPointZ(p, strokeInfo.StrokeID,node);
          roadInfo.Nodes.Add(node);
        }
      }
      foreach (var item in sortStroke) {
        if (item.Length > lockLenght) {
          item.State = RoadState.Locked;
        }
      }
      #endregion

      #region 3 开始删除
      List<int> removedRoads = new List<int>();
      sortStroke.Sort((s1, s2) => s1.Length.CompareTo(s2.Length));
      LinkedList<StrokeInfo> strokeLinkedList = new LinkedList<StrokeInfo>(sortStroke);
      double deleteLength = 0;
      LinkedListNode<StrokeInfo> strokeNode = strokeLinkedList.First;
      while (deleteLength < allLength * (1 - selectRate)) {
        
        if (strokeNode == null) {
          break;
        }
        if (strokeNode.Value.State != RoadState.Normal) {
          goto NextStroke;
        }
        strokeNode.Value.State = RoadState.Delete;
        
        //锁定周围道路        
        Dictionary<int, int> nearStroke = GetNearStroke(strokeNode.Value);
        LockStrokes(nearStroke, strokeDic);
        //检查途径的节点有没有被删成一个点的
        Dictionary<int, List<int>> throughNodes = GetThroughNodes(graph, strokeNode.Value);
        Dictionary<int, bool> deleteRoads = GetDeleteRoad(throughNodes, graph, strokeNode.Value);
#if DEBUG
        dis.StartDrawing(dis.hDC, 0);
#endif
        foreach (var item in deleteRoads.Keys) {
          IPolyline deleteShape = null;
          if(deleteRoads[item])
          {
            graph.RemoveLine(item);
            removedRoads.Add(item);
            deleteShape = (fc.GetFeature(item).Shape as IPolyline);
            deleteLength += deleteShape.Length;
#if DEBUG
            sls.Color = deleteCorlor;
            dis.SetSymbol(sls);
            dis.DrawPolyline(deleteShape);
            continue;
#endif
          }
#if DEBUG
          deleteShape = (fc.GetFeature(item).Shape as IPolyline);
          sls.Color = saveColor;
          dis.SetSymbol(sls);
          dis.DrawPolyline(deleteShape);
#endif
        }
#if DEBUG
        dis.FinishDrawing();
        //System.Windows.Forms.MessageBox.Show("next");
#endif

      NextStroke:
        if (strokeNode.Next != null) {
          strokeNode = strokeNode.Next;
        }
        else {
          sortStroke.ForEach((info) => { if (info.State == RoadState.Locked) info.State = RoadState.Normal; });
          LinkedListNode<StrokeInfo> fristNormalNode = strokeLinkedList.First;
          while (fristNormalNode != null) {
            if (fristNormalNode.Value.State == RoadState.Normal) {
              break;
            }
            else
              fristNormalNode = fristNormalNode.Next;
          }
          strokeNode = fristNormalNode;
        }
      }//while
      #endregion

      foreach (var item in removedRoads) {
        IFeature feature = fc.GetFeature(item);
        feature.set_Value(usedID, 0);
        feature.Store();
      }
      fdefinition.DefinitionExpression = "_GenUsed = 1";
      m_application.MapControl.Refresh();
    }
    
    /// <summary>
    /// 获取需要道路实际删除情况
    /// </summary>
    /// <param name="roadOnNodeDic">预定义删除的道路，key是节点索引，value是在stroke上且挂在该节点上的道路ID（有向id，见GraphNode.ConvertOID（））</param>
    /// <param name="hangNodeAfterDelete">删除后可能出的悬挂点</param>
    /// <returns>索引是道路的fid，值表示道路的删除情况,true表示删除了，false表示没删除</returns>
    Dictionary<int, bool> GetDeleteRoad(Dictionary<int, List<int>> roadOnNodeDic, Graph graph, StrokeInfo stroke) {
      /*
       * 思想：
       * 0.排除本身就是悬挂点的点
       * 1.把所有都删除，计算删除后的状态
       * 2.看看还有没有悬挂点,如果没有则结束，如果有，则到3
       * 3.把悬挂点上被删除的边选一条最短的加上去，转到2
       */
      //删除一部分后的信息
      Dictionary<int, List<int>> currentNodesInfo = new Dictionary<int, List<int>>();
      //删除的信息
      Dictionary<int, List<int>> deleteNodesInfo = new Dictionary<int, List<int>>();
      //本身是不是悬挂点
      Dictionary<int, bool> orgNodeInfo = new Dictionary<int, bool>();

      //1.删除所有的
      foreach (var nodeIndex in roadOnNodeDic.Keys) {
        orgNodeInfo.Add(nodeIndex, graph.nodes[nodeIndex].conectFeatures.Count == 1);
        GraphNode node = graph.nodes[nodeIndex];
        currentNodesInfo.Add(nodeIndex, new List<int>(node.conectFeatures));
        deleteNodesInfo.Add(nodeIndex,new List<int>(roadOnNodeDic[nodeIndex]));
        roadOnNodeDic[nodeIndex].ForEach((roadID) => { 
          currentNodesInfo[nodeIndex].Remove(roadID);           
        });
      }

      //索引是道路的fid，值表示道路的删除情况,true表示删除了，false表示没删除
      Dictionary<int, bool> result = new Dictionary<int, bool>();
      stroke.RoadInfos.ForEach((r) => { result.Add(r.FID, true); });

      List<int> hangNodes = new List<int>();
      do {
        //2.1计算悬挂点
        hangNodes.Clear();
        foreach (var nodeIndex in currentNodesInfo.Keys) {
          if (!orgNodeInfo[nodeIndex] && currentNodesInfo[nodeIndex].Count == 1) {
            hangNodes.Add(nodeIndex);
          }
        }
        //2.2判断是否存在悬挂点
        if (hangNodes.Count == 0) {
          break;
        }
        //3 补上最短边
        foreach (var hangNodeIndex in hangNodes) {
          //3.1先检查现在还是不是悬挂点，有可能前面加了一条边变成不是悬挂点了
          if (currentNodesInfo[hangNodeIndex].Count != 1)
            continue;
          List<int> deleteRoadOnHangNode = deleteNodesInfo[hangNodeIndex];
          RoadInfo shortestRoad = null;
          foreach (var deleteRoadIndex in deleteRoadOnHangNode) {
            int fid = GraphNode.GetOID(deleteRoadIndex);
            RoadInfo info = stroke.RoadInfos.Find((r) => { return r.FID == fid; });
            if (shortestRoad == null 
              || (shortestRoad.Length > info.Length ) )             
              shortestRoad = info;
            
          }
          //如果这条边已经补回来了
          if (shortestRoad == null||result[shortestRoad.FID] == false) {
            continue;
          }
          //补回去
          for (int i = 0; i < 2; i++) {
            int addlineID = GraphNode.ConvertOID(shortestRoad.FID, i==0);
            int addnodeIndex = graph.linesNode[addlineID];
            currentNodesInfo[addnodeIndex].Add(addlineID);
            deleteNodesInfo[addnodeIndex].Remove(addlineID);
          }          
          result[shortestRoad.FID] = false;
        }
      } while (true);
      return result;
    }

    /// <summary>
    /// 获取stroke途经节点信息，可以获取到途经哪个节点几次
    /// key是节点索引，value是挂在该节点上的道路ID（有向id，见GraphNode.ConvertOID()）
    /// </summary>
    /// <param name="graph">图</param>
    /// <param name="info">stroke</param>
    /// <returns>key是节点索引，value是在stroke上且挂在该节点上的道路ID（有向id，见GraphNode.ConvertOID（））</returns>
    Dictionary<int, List<int>> GetThroughNodes(Graph graph, StrokeInfo info) {
      Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
      foreach (var item in info.RoadInfos) {
        for (int i = 0; i < 2; i++) {
          int lineID = GraphNode.ConvertOID(item.FID, (i == 0));
          int nodeIndex = graph.linesNode[lineID];
          if (!result.ContainsKey(nodeIndex)) {
            result.Add(nodeIndex, new List<int>());
          }
          result[nodeIndex].Add(lineID);
        }
      }
      return result;
    }
    /// <summary>
    /// 返回附近的stroke
    /// </summary>
    /// <param name="info">要找的strokeInfo</param>
    /// <returns>返回找的的Stroke列表，key是邻接的道路的strokeID，value是点数</returns>
    Dictionary<int, int> GetNearStroke(StrokeInfo info) {
      Dictionary<int, int> nearStroke = new Dictionary<int, int>(); 
      foreach (var item in info.RoadInfos) {
        //通过三角网的节点找到邻近边
        foreach (var tinNode in item.Nodes) {
          ITinNodeArray array = tinNode.GetAdjacentNodes();
          for (int i = 0; i < array.Count; i++) {
            ITinNode adNode = array.get_Element(i);
            if (!adNode.IsInsideDataArea)
              continue;
            int otherStrokeID = adNode.TagValue;
            if (!nearStroke.ContainsKey(otherStrokeID)) {
              nearStroke.Add(otherStrokeID, 0);
            }
            nearStroke[otherStrokeID] += 1;
          }
        }//foreach (var tinNode in item.Nodes)
      }//foreach (var item in strokeNode.Value.RoadInfos)
      return nearStroke;
    }
    /// <summary>
    /// 锁定周围的道路
    /// </summary>
    /// <param name="nearStroke">周围的道路，key是道路的strokeID，Value是点数</param>
    /// <param name="strokeDic">全部stroke的字典，key是strokeID，Value是StrokeInfo</param>
    void LockStrokes(Dictionary<int, int> nearStroke,Dictionary<int,StrokeInfo> strokeDic) {
      foreach (var item in nearStroke.Keys) {
        if (nearStroke[item] < 2)
          continue;
        StrokeInfo info = strokeDic[item];
        if (info.State == RoadState.Normal)
          info.State = RoadState.Locked;
      }
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

    internal enum RoadState {
      Normal,
      HighRank,
      Delete,
      Locked
    }

    internal class StrokeInfo {
      /// <summary>
      /// Stroke的StrokeID；如道路在stroke上，取本身值，否则取-FID
      /// </summary>
      internal int StrokeID { get; private set; }
      /// <summary>
      /// 所包含的道路
      /// </summary>
      internal List<RoadInfo> RoadInfos { get; private set; }
      /// <summary>
      /// stroke的状态
      /// </summary>
      internal RoadState State { get; set; }
      /// <summary>
      /// 总长度
      /// </summary>
      internal double Length {
        get {
          double l = 0;
          RoadInfos.ForEach(i => { l += i.Length; });
          return l;
        }
      }
      internal StrokeInfo(RoadInfo roadInfo) {
        RoadInfos = new List<RoadInfo>();
        StrokeID = GetStrokeID(roadInfo);
        RoadInfos.Add(roadInfo);
        State = RoadState.Normal;
      }
      internal static int GetStrokeID(RoadInfo roadInfo) {
        return (roadInfo.StrokeID > 0) ? roadInfo.StrokeID : (-roadInfo.FID);
      }
    }

    internal class RoadInfo {
      /// <summary>
      /// Road对应Feature的FID 
      /// </summary>
      internal int FID { get; private set; }
      /// <summary>
      /// Road所在Stroke的StrokeID；如道路在stroke上，取本身值，否则取-FID
      /// </summary>
      internal int StrokeID { get; private set; }
      /// <summary>
      /// 道路等级
      /// </summary>
      internal int Rank { get; private set; }
      /// <summary>
      /// 道路在图中的索引
      /// </summary>
      internal int Index { get; private set; }
      /// <summary>
      /// 道路上的三角网节点
      /// </summary>
      internal List<ITinNode> Nodes { get; private set; }
      /// <summary>
      /// 道路长度
      /// </summary>
      internal double Length { get; private set; }
      internal RoadInfo(int fid, int strokeID, int rank,int index,double length) {
        this.FID = fid;
        this.StrokeID = strokeID;
        this.Rank = rank;
        this.Index = index;
        this.Length = length;
        this.Nodes = new List<ITinNode>();
      }
    }
  }
}

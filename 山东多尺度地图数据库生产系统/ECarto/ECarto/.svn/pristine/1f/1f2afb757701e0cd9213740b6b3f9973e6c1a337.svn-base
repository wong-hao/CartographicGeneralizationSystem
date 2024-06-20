using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Windows.Forms;

//这个文件里面就是线匹配的主要方法
namespace DatabaseUpdate
{

    class NodePair
    {
        public NodePair(LineMatch.NodeInfo item1, LineMatch.NodeInfo item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
        public LineMatch.NodeInfo Item1 { get; set; }
        public LineMatch.NodeInfo Item2 { get; set; }
    }
    internal static class LineMatch
    {
        internal class FeaturePartInfo
        {
            public int FeatureCursorIndex { get; set; }
            public int FeatureIndex { get { return Feature.OID; } }
            public IFeature Feature { get; set; }
            public int PartIndex { get; set; }
            public IGeometry DensifyShape { get; set; }

            public ISegment SegmentAt(int sid)
            {
                return ((DensifyShape as IGeometryCollection).get_Geometry(PartIndex) as ISegmentCollection).get_Segment(sid);
            }

            public IPath PathAt(int fromSegmentIndex, int toSegmentIndex)
            {
                var sc = (DensifyShape as IGeometryCollection).get_Geometry(PartIndex) as ISegmentCollection;
                var path = new PathClass();
                for (int i = fromSegmentIndex; i <= toSegmentIndex; i++)
                {
                    object bf, af;
                    bf = af = Type.Missing;
                    path.AddSegment(sc.get_Segment(i), ref bf, ref af);
                }
                return path;
            }

            public FeaturePartInfo(int fcid, IFeature feature, int pid, IGeometry shp)
            {
                FeatureCursorIndex = fcid;
                Feature = feature;
                PartIndex = pid;
                DensifyShape = shp;
            }

            public override string ToString()
            {
                return FeatureCursorIndex.ToString()
                    + "," + FeatureIndex.ToString()
                    + "," + PartIndex.ToString(); ;
            }
            public override int GetHashCode()
            {
                return this.ToString().GetHashCode();
            }
        }

        internal class LineConflictInfo
        {
            public FeaturePartInfo FeatureA { get; set; }
            public int fromPointIndexA { get; set; }
            public int toPointIndexA { get; set; }
            public IPath ConflictPartA { get; set; }

            public FeaturePartInfo FeatureB { get; set; }
            public int fromPointIndexB { get; set; }
            public int toPointIndexB { get; set; }
            public IPath ConflictPartB { get; set; }
        }

        internal class NodeInfo
        {
            public FeaturePartInfo Feature { get; set; }
            public int SegmentIndex { get; set; }
            public NodeInfo(FeaturePartInfo featureInfo, int pointIndex)
            {
                Feature = featureInfo;
                SegmentIndex = pointIndex;
            }
        }

        internal class ConflictNodes
        {
            internal FeaturePartInfo FeatureA { get; set; }
            internal FeaturePartInfo FeatureB { get; set; }
            //internal DatabaseUpdate.Index.Envelope Envelope { get; set; }
            internal DatabaseUpdate.RTree.Rectangle Envelope { get; set; }

            internal ConflictNodes(NodeInfo a, NodeInfo b)
            {
                FeatureA = a.Feature;
                FeatureB = b.Feature;
                Envelope = MakeEnvelope(a, b);
            }

            static internal DatabaseUpdate.RTree.Rectangle MakeEnvelope(NodeInfo a, NodeInfo b)
            {
                //return new DatabaseUpdate.Index.Envelope(a.SegmentIndex - 1, b.SegmentIndex - 1, a.SegmentIndex + 1, b.SegmentIndex + 1);
                return new DatabaseUpdate.RTree.Rectangle(a.SegmentIndex - 1, b.SegmentIndex - 1, a.SegmentIndex + 1, b.SegmentIndex + 1);

            }

            internal void Expand(ConflictNodes other)
            {
                Envelope.add(other.Envelope);
                //Envelope.Extend(other.Envelope);
            }
        }

        internal static int NodePairCmpByItem1(NodePair l, NodePair r)
        {
            return l.Item1.SegmentIndex.CompareTo(r.Item1.SegmentIndex);
        }

        internal static int NodePairCmpByItem2(NodePair l, NodePair r)
        {
            return l.Item2.SegmentIndex.CompareTo(r.Item2.SegmentIndex);
        }

        /// <summary>
        /// 探测线冲突
        /// </summary>
        /// <param name="cursors">需要探测的所有目标的cursor(获得的Feature必须可存储)</param>
        /// <param name="distance">距离阈值</param>
        /// <returns>冲突信息</returns>
        internal static List<LineConflictInfo> DetectConflict(List<IFeatureClass> fcList, IEnvelope env, double distance)
        {
            List<LineConflictInfo> conflicts = new List<LineConflictInfo>();

            List<NodeInfo> nodeInfos = new List<NodeInfo>();
            TinClass tin = new TinClass();

            try
            {
                //这一步是用三角网探测冲突关系，换成用空间索引判断距离关系可能速度更快，也能避免重复点的问题
                //（如果改了测试数据就不需要做偏移了）
                #region 1.建网
                tin.InitNew(env);
                tin.StartInMemoryEditing();
                
                for (int fcid = 0; fcid < fcList.Count; fcid++)
                {
                    IFeatureCursor pCursor = fcList[fcid].Search(null, false);
                    for (var fe = pCursor.NextFeature(); fe != null; fe = pCursor.NextFeature())
                    {
                        //内存监控
                        if (Environment.WorkingSet > 1024 * 1024 * 700)//字节
                        {
                            GC.Collect();
                        }

                        IPolycurve curve = fe.ShapeCopy as IPolycurve;
                        curve.Densify(distance * 1.1, 0);

                        IGeometryCollection gc = curve as IGeometryCollection;
                        for (int pid = 0; pid < gc.GeometryCount; pid++)
                        {
                            ISegmentCollection sc = gc.get_Geometry(pid) as ISegmentCollection;
                            for (int segId = 0; segId < sc.SegmentCount; segId++)
                            {
                                ISegment seg = sc.get_Segment(segId);
                                IPoint pt = new PointClass();

                                pt.PutCoords((seg.FromPoint.X + seg.ToPoint.X) / 2, (seg.FromPoint.Y + seg.ToPoint.Y) / 2);
                                pt.Z = 0;


                                tin.AddPointZ(pt, nodeInfos.Count);
                                nodeInfos.Add(new NodeInfo(new FeaturePartInfo(fcid, fe, pid, curve), segId));
                            }
                        }

                        //Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(pCursor);

                }
                #endregion

                List<ITinEdge> conflictEdges = new List<ITinEdge>();

                #region 2.找出短边，只找index小于相邻边的多边形
                for (int i = 1; i <= tin.EdgeCount; i++)
                {
                    ITinEdge edge = tin.GetEdge(i);
                    if (!edge.FromNode.IsInsideDataArea)
                        continue;
                    if (!edge.ToNode.IsInsideDataArea)
                        continue;
                    if (edge.Index > edge.GetNeighbor().Index)
                        continue;

                    if (edge.Length > distance)
                        continue;

                    conflictEdges.Add(edge);
                }
                #endregion

                #region 3.整理结果

                //这个字典存储冲突信息，键：要素名称（假设为A，注意A是集合，其中某个是a），值：冲突信息  
                Dictionary<string,
                    //这个作为值的列表存与键中要素名称冲突的要素的信息，键：与a冲突的要素名称（假设为B,同A），值：导致a~b冲突的冲突边的集合
                             Dictionary<string, List<NodePair>>
                          > conflictEdgeDict
                    = new Dictionary<string, Dictionary<string, List<NodePair>>>();
                #region 3.1 按要素归类
                //这一部分主要是填上面的冲突字典
                foreach (var edge in conflictEdges)
                {
                    var fromNodeInfo = nodeInfos[edge.FromNode.TagValue];
                    var toNodeInfo = nodeInfos[edge.ToNode.TagValue];
                    if (fromNodeInfo.Feature.FeatureCursorIndex == toNodeInfo.Feature.FeatureCursorIndex)
                        continue;
                    //目前先把自己排除，自己与自己不构成冲突
                    var fromNodeFeatureName = fromNodeInfo.Feature.ToString();
                    var toNodeFeatureName = toNodeInfo.Feature.ToString();
                    //等于0说明两个名字是一样的，跳过
                    if (fromNodeFeatureName.CompareTo(toNodeFeatureName) == 0)
                        continue;

                    //如果a与b冲突，那b与a也冲突，所以记下一份就可以了，这里优先记在名称靠前的要素里面
                    //小于0说明from排在前面，比如【1，23，1】小于【2，500，1】，那么以from的name为键值
                    if (fromNodeFeatureName.CompareTo(toNodeFeatureName) <= 0)
                    {
                        //如果字典里面还没有from要素，就加一个
                        if (!conflictEdgeDict.ContainsKey(fromNodeFeatureName))
                        {
                            conflictEdgeDict.Add(fromNodeFeatureName, new Dictionary<string, List<NodePair>>());
                        }
                        //如果from要素的冲突信息字典里面没有to要素，就加一个
                        if (!conflictEdgeDict[fromNodeFeatureName].ContainsKey(toNodeFeatureName))
                        {
                            conflictEdgeDict[fromNodeFeatureName].Add(toNodeFeatureName, new List<NodePair>());
                        }
                        //在from要素与to要素的冲突边中加上当前一条
                        conflictEdgeDict[fromNodeFeatureName][toNodeFeatureName].Add(new NodePair(fromNodeInfo, toNodeInfo));
                    }
                    //这里是to排在前面，以to的name为键值，其他同上
                    else
                    {
                        if (!conflictEdgeDict.ContainsKey(toNodeFeatureName))
                        {
                            conflictEdgeDict.Add(toNodeFeatureName, new Dictionary<string, List<NodePair>>());
                        }
                        if (!conflictEdgeDict[toNodeFeatureName].ContainsKey(fromNodeFeatureName))
                        {
                            conflictEdgeDict[toNodeFeatureName].Add(fromNodeFeatureName, new List<NodePair>());
                        }
                        conflictEdgeDict[toNodeFeatureName][fromNodeFeatureName].Add(new NodePair(toNodeInfo, fromNodeInfo));
                    }
                }
                #endregion

                
                #region 3.2
                //这里是把每个要素的冲突信息汇聚成一条或多条完整的折线
                //因为检测冲突的时候把每条线分成一小段一小段的了，所以最后要拼起来
                //例如要素a的第2、3、4段与要素b的5、6、7、8段冲突，需要把这两段都拼接成一个完整的曲线

                //对每对冲突要素进行处理，用两个循环，这是第一个
                foreach (var cs in conflictEdgeDict)
                {
                    //这是要素A的名称
                    var featureA = cs.Key;
                    //这是第二个
                    foreach (var c in cs.Value)
                    {
                        //这是要素B的名称
                        var featureB = c.Key;
                        System.Diagnostics.Debug.WriteLine(featureA + " to " + featureB);

                        //这是A与B的冲突关系
                        List<NodePair> nps = c.Value;

#if DEBUG //调试用的代码，可以删除，zq，2016年5月4日
                        if (featureA == "0,28,0" && featureB == "1,27,0")
                        {
                            System.Diagnostics.Debug.WriteLine("seg{");
                            foreach (var nodePair in nps)
                            {
                                System.Diagnostics.Debug.WriteLine(nodePair.Item1.SegmentIndex + " - " + nodePair.Item2.SegmentIndex);
                            }
                            System.Diagnostics.Debug.WriteLine("}");
                        }
#endif
                        //没有冲突
                        if (nps.Count <= 0)
                            continue;

                        #region 原来R树有bug，可以删除，zq，2016年5月4日
#if zhouqi
          //构建R树来快速拼接
          DatabaseUpdate.Index.RTree<ConflictNodes> conflictNodesTree = new Index.RTree<ConflictNodes>();

          //针对A与B的每一条冲突边构建冲突节点插入R树中
          foreach (var nodePair in nps)
          {
            //利用冲突构建一个！冲突节点！
            //每个冲突节点就是一个矩形，x坐标表示A的冲突范围（索引范围），y坐标表示B的冲突范围
            //例如A的第2个点与B的第3个点冲突，那么冲突范围就是：([1,3],[2,4]）
            //详细可以参见ConflictNodes类
            ConflictNodes currentConflictNode = new ConflictNodes(nodePair.Item1, nodePair.Item2);

            //利用冲突节点的矩形在R树中查询所有相交矩形情况
            //矩形相交表示A和B有 ！连续冲突区域！
            //比如查询的冲突节点为([1,3],[2,4]），R树内有([2,4],[3,5]），则这两个矩形相交，说明A的第2、3个点与B的第3、4个点！连续冲突！
            var enumTreeNode = conflictNodesTree.Search(currentConflictNode.Envelope);

            //所有R树内的连续冲突区域都移除掉，方便下一步合并成一个大冲突节点再插入
            List<ConflictNodes> conflictNodes = new List<ConflictNodes>();
            foreach (var treeNode in enumTreeNode)
            {
              conflictNodes.Add(treeNode.Data);
              conflictNodesTree.Remove(treeNode.Data, treeNode.Envelope);
            }

            //将所有连续冲突区域合并成大冲突节点并插入R树
            foreach (var conflictNode in conflictNodes)
            {
              currentConflictNode.Expand(conflictNode);
            }
            conflictNodesTree.Insert(currentConflictNode, currentConflictNode.Envelope);
          }
          //↑至此R树中已经包含了A与B所有的连续冲突区间信息

          //拿出所有R树中的冲突区间信息，构建A和B各自的冲突曲线，形成冲突结果
          var enumTreeNodes = conflictNodesTree.AllLeafNode();
          foreach (var conflictNode in enumTreeNodes)
          {
            var conflictFeatureA = conflictNode.Data.FeatureA;
            var conflictFeatureB = conflictNode.Data.FeatureB;

            int minPartIndexA = conflictNode.Envelope.X1 + 1;
            int maxPartIndexA = conflictNode.Envelope.X2 - 1;
            int minPartIndexB = conflictNode.Envelope.Y1 + 1;
            int maxPartIndexB = conflictNode.Envelope.Y2 - 1;

            LineConflictInfo info = new LineConflictInfo();
            info.FeatureA = conflictFeatureA;
            info.FeatureB = conflictFeatureB;

            info.fromPointIndexA = minPartIndexA;
            info.toPointIndexA = minPartIndexA;

            info.fromPointIndexB = minPartIndexB;
            info.toPointIndexB = minPartIndexB;

            info.ConflictPartA = conflictFeatureA.PathAt(minPartIndexA, maxPartIndexA);
            info.ConflictPartB = conflictFeatureB.PathAt(minPartIndexB, maxPartIndexB);

            IPolyline plA = new PolylineClass();
            IPolyline plB = new PolylineClass();
            object bf, af;
            bf = af = Type.Missing;
            (plA as IGeometryCollection).AddGeometry(info.ConflictPartA, ref bf, ref af);
            bf = af = Type.Missing;
            (plB as IGeometryCollection).AddGeometry(info.ConflictPartB, ref bf, ref af);
            IRelationalOperator relOper = plA as IRelationalOperator;

            bool geo = relOper.Crosses(plB);

            if (!geo)
            {
              conflicts.Add(info);
            }

          }
#endif
                        #endregion

                        #region 新R树算法
                        //构建R树来快速拼接
                        DatabaseUpdate.RTree.RTree<ConflictNodes> conflictNodesTree = new RTree.RTree<ConflictNodes>();

                        try
                        {
                            //针对A与B的每一条冲突边构建冲突节点插入R树中
                            foreach (var nodePair in nps)
                            {
                                //利用冲突构建一个！冲突节点！
                                //每个冲突节点就是一个矩形，x坐标表示A的冲突范围（索引范围），y坐标表示B的冲突范围
                                //例如A的第2个点与B的第3个点冲突，那么冲突范围就是：([1,3],[2,4]）
                                //详细可以参见ConflictNodes类
                                ConflictNodes currentConflictNode = new ConflictNodes(nodePair.Item1, nodePair.Item2);

#if DEBUG //调试用的代码，可以删除，zq，2016年5月4日
                                if (featureA == "0,28,0" && featureB == "1,27,0")
                                {
                                    System.Diagnostics.Debug.WriteLine(nodePair.Item1.SegmentIndex + " !!! " + nodePair.Item2.SegmentIndex);
                                }
#endif

                                //利用冲突节点的矩形在R树中查询所有相交矩形情况
                                //矩形相交表示A和B有 ！连续冲突区域！
                                //比如查询的冲突节点为([1,3],[2,4]），R树内有([2,4],[3,5]），则这两个矩形相交，说明A的第2、3个点与B的第3、4个点！连续冲突！
                                var enumTreeNode = conflictNodesTree.Intersects(currentConflictNode.Envelope);

                                //所有R树内的连续冲突区域都移除掉，方便下一步合并成一个大冲突节点再插入
                                List<ConflictNodes> conflictNodes = new List<ConflictNodes>();
                                foreach (var treeNode in enumTreeNode)
                                {
                                    conflictNodes.Add(treeNode);
                                    conflictNodesTree.Delete(treeNode.Envelope, treeNode);
                                }

                                //将所有连续冲突区域合并成大冲突节点并插入R树
                                foreach (var conflictNode in conflictNodes)
                                {
                                    currentConflictNode.Expand(conflictNode);
                                }
                                conflictNodesTree.Add(currentConflictNode.Envelope, currentConflictNode);
                            }
                            //↑至此R树中已经包含了A与B所有的连续冲突区间信息
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        //拿出所有R树中的冲突区间信息，构建A和B各自的冲突曲线，形成冲突结果
                        var enumTreeNodes = conflictNodesTree.Intersects(conflictNodesTree.getBounds());
                        foreach (var conflictNode in enumTreeNodes)
                        {
                            var conflictFeatureA = conflictNode.FeatureA;
                            var conflictFeatureB = conflictNode.FeatureB;

                            int minPartIndexA = conflictNode.Envelope.min[0] + 1;
                            int maxPartIndexA = conflictNode.Envelope.max[0] - 1;
                            int minPartIndexB = conflictNode.Envelope.min[1] + 1;
                            int maxPartIndexB = conflictNode.Envelope.max[1] - 1;

                            LineConflictInfo info = new LineConflictInfo();
                            info.FeatureA = conflictFeatureA;
                            info.FeatureB = conflictFeatureB;

                            info.fromPointIndexA = minPartIndexA;
                            info.toPointIndexA = maxPartIndexA;//！！！这里原来写错了，原来写的minPartIndexA，zq，2016年5月4日

                            info.fromPointIndexB = minPartIndexB;
                            info.toPointIndexB = maxPartIndexB;//！！！这里原来写错了，原来写的minPartIndexB，zq，2016年5月4日

                            info.ConflictPartA = conflictFeatureA.PathAt(minPartIndexA, maxPartIndexA);
                            info.ConflictPartB = conflictFeatureB.PathAt(minPartIndexB, maxPartIndexB);

#if zhouqi  //这一部分有问题，忘记当时为啥要做这个判断了，但是可以删除，zq，2016年5月4日
            //IPolyline plA = new PolylineClass();
            //IPolyline plB = new PolylineClass();
            //object bf, af;
            //bf = af = Type.Missing;
            //(plA as IGeometryCollection).AddGeometry(info.ConflictPartA, ref bf, ref af);
            //bf = af = Type.Missing;
            //(plB as IGeometryCollection).AddGeometry(info.ConflictPartB, ref bf, ref af);
            //IRelationalOperator relOper = plA as IRelationalOperator;

            //bool geo = relOper.Crosses(plB);

            //if (!geo)
            //{
            //  conflicts.Add(info);
            //}
#endif

                            conflicts.Add(info);
                        }

                        #endregion


                    }
                }
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //返回冲突结果
            return conflicts;
        }
    }
}



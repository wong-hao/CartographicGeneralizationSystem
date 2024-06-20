using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Common.Algrithm
{
    public class SimplifyByDTAlgorithm
    {
        enum NodeType
        {
            Super = int.MaxValue
        }
        
        enum EdgeType 
        {
             Arc = 0x1,//弧
             Gut = 0x0,//弦

            SuperEdge = int.MaxValue,
        }

        enum SiteType 
        {
            RightSite = 0x00,
            LeftSite = 0x10,
        }
        enum TriangleType
        {
            T1 = 0x1,  //1条弦
            T2 = 0x2, //2条弦
            T3 = 0x3, //3条弦
            SuperTriangle = int.MaxValue,
        }

        /// <summary>
        /// 二叉树
        /// </summary>
        class BendTreeNode
        {
            internal BendTreeNode Parent { get; set; }
            internal List<BendTreeNode> Children { get; set; }
            internal ITinEdge BaseGut { get; set; }
            internal IPath Path { get; set; }
            internal BendTreeNode SourceNode { get; set; }
            internal double PathDepth { get; set; }
            internal int PathLevel { get; set; }

            internal ITin Tin
            {
                get { return BaseGut.TheTin; }
            }
            internal bool IsRightSite
            {
                get { return (BaseGut.RightTriangle.TagValue & (int)SiteType.LeftSite) == 0; }
            }
            internal double Width //开口宽度
            {
                get {
                    return BaseGut.Length;
                }
            }
            internal double Depth //纵深
            {
                get {
                    return Path.Length;
                }
            }

            internal BendTreeNode(BendTreeNode parent, ITinEdge baseGut)
            {
                this.BaseGut = baseGut;
                this.Parent = parent;
                this.PathDepth = 0;
                if (parent != null)
                {
                    parent.Children.Add(this);
                }
                this.Children = new List<BendTreeNode>();
                Path = new PathClass();

                InitAllTreePath();
                CalSourceNode();
            }

            void InitAllTreePath()
            {
                ITinEdge currentEdge = BaseGut;
                while (currentEdge != null)
                {
                    var n = AddPathPoint(currentEdge);
                    if (n == currentEdge)//三类
                    {
                        var c1 = new BendTreeNode(this, n.GetNextInTriangle().GetNeighbor());
                        var c2 = new BendTreeNode(this, n.GetPreviousInTriangle().GetNeighbor());
                        break;
                    }
                    currentEdge = n;
                }
            }

            /// <summary>
            /// 为中轴添加当前边对应的点并返回下一条边
            /// </summary>
            /// <param name="fromEdge">当前边</param>
            /// <returns>空值代表1类，与fromEdge相同代表3类，其他为2类</returns>
            ITinEdge AddPathPoint(ITinEdge fromEdge)
            {
                IPoint pt = new PointClass
                {
                    X = (fromEdge.FromNode.X + fromEdge.ToNode.X)/2,
                    Y = (fromEdge.FromNode.Y + fromEdge.ToNode.Y)/2
                };

                (Path as IPointCollection).AddPoint(pt);

                TriangleType triType = (TriangleType)(fromEdge.RightTriangle.TagValue & 0xF);
                switch (triType)
                {
                    case TriangleType.T3:
                        (Path as IPointCollection).AddPoint(new PointClass
                        {
                            X = fromEdge.GetNextInTriangle().ToNode.X,
                            Y = fromEdge.GetNextInTriangle().ToNode.Y
                        });
                        return fromEdge;
                    case TriangleType.T2:
                        break;
                    case TriangleType.T1:
                        (Path as IPointCollection).AddPoint(new PointClass {
                            X = fromEdge.GetNextInTriangle().ToNode.X,
                            Y = fromEdge.GetNextInTriangle().ToNode.Y
                        });
                        return null;
                    case TriangleType.SuperTriangle:
                    default:
                        return null;
                }

                //T2
                var nEdge = fromEdge.GetNextInTriangle();
                if (nEdge.TagValue == (int)EdgeType.Gut)
                    return nEdge.GetNeighbor();
                else
                    return fromEdge.GetPreviousInTriangle().GetNeighbor();                
            }

            /// <summary>
            /// 计算每段属于哪个河源
            /// </summary>
            void CalSourceNode()
            {
                if (Children.Count == 0)
                {
                    SourceNode = this;
                    PathDepth = this.Depth;
                    return;
                }
                BendTreeNode maxNode = Children[0];
                foreach (var c in Children)
                {
                    c.CalSourceNode();
                    if (c.PathDepth > maxNode.PathDepth)
                        maxNode = c;
                }
                this.PathDepth = maxNode.PathDepth + this.Depth;
                this.SourceNode = maxNode.SourceNode;
            }
            
        }

        /// <summary>
        /// 多叉树
        /// </summary>
        class BendPathTreeNode
        {
            internal List<BendTreeNode> Nodes { get; set; }
            internal List<BendPathTreeNode> Children { get; set; }
            internal BendPathTreeNode Parent { get; set; }

            internal BendTreeNode Root { get { return Nodes[0]; } }
            internal BendTreeNode Source { get { return Nodes[Nodes.Count - 1]; } }
            internal ITinEdge BaseGut { get { return Root.BaseGut; } }
            internal double Width { get { return Root.Width; } }
            internal double Depth { get { return Root.Depth; } }

            internal BendPathTreeNode(BendPathTreeNode parent,BendTreeNode root)
            {
                this.Parent = parent;
                this.Nodes = new List<BendTreeNode>();
                this.Children = new List<BendPathTreeNode>();
                
                var cur = root;
                while (true)
                {
                    this.Nodes.Add(cur);
                    if (cur.Children.Count <= 0)
                    {
                        break;
                    }
                    if (cur.Children[0].SourceNode == root.SourceNode)
                    {
                        Children.Add(new BendPathTreeNode(this, cur.Children[1]));                        
                        cur = cur.Children[0];
                    }
                    else
                    {
                        Children.Add(new BendPathTreeNode(this, cur.Children[0]));
                        cur = cur.Children[1];
                    }
                }
            }
        }

        /// <summary>
        /// 计算曲线是偏向哪一侧
        /// </summary>
        /// <param name="path">曲线</param>
        /// <returns>哪一侧</returns>
        static SiteType CalBendSite(IPath path)
        {
            RingClass ring = new RingClass();
            var pc = path as IPointCollection;
            ring.InsertPointCollection(0, pc);
            ring.Close();
            double a = ring.Area;
            PolygonClass polygon = new PolygonClass();
            polygon.AddGeometry(ring);
            polygon.Simplify();
            var ch = polygon.ConvexHull() as IArea;
            double ach = ch.Area;

            if (a > 0)
            {
                return a * 2 > ach ? SiteType.LeftSite : SiteType.RightSite;
            }
            else
            {
                return -a * 2 > ach ? SiteType.RightSite : SiteType.LeftSite;
            }
        }

        /// <summary>
        /// 将一个环分割为两条折线（按最远点分割）
        /// </summary>
        /// <param name="path">环，需要外部保证path.IsClosed</param>
        /// <returns>分割后的两条折线</returns>
        static IPath[] SpiltRing(IPath path)
        {
            
            var pc = path as IPointCollection;
            int farthestPointIndex = 0;
            double farchestDistance = 0;
            var pt0 = pc.get_Point(0);
            for (int i = 1; i < pc.PointCount; i++)
            {
                var pt = pc.get_Point(i);
                var dis = (pt.X - pt0.X) * (pt.X - pt0.X) + (pt.Y - pt0.Y) * (pt.Y - pt0.Y);
                if (farchestDistance < dis)
                {
                    farchestDistance = dis;
                    farthestPointIndex = i;
                }
            }

            var p1 = new PathClass();
            for (int i = 0; i <= farthestPointIndex; i++)
            {
                p1.AddPoint(pc.get_Point(i));
            }
            var p2 = new PathClass();
            for (int i = farthestPointIndex; i < pc.PointCount; i++)
			{
                p2.AddPoint(pc.get_Point(i));
			}
            return new IPath[] { p1, p2 };
        }

        /// <summary>
        /// 将两条折线合并为一个环
        /// </summary>
        /// <param name="paths">折线，需要保证收尾相接</param>
        /// <param name="asRing">作为PathClass 还是 RingClass返回</param>
        /// <returns>环</returns>
        static IPath MergePath(IPath path1,IPath path2,bool asRing = false)
        {
            IPointCollection pc = null;
            if (asRing)
            {
                pc = new RingClass();
            }
            else
            {
                pc = new PathClass();
            }


            var pc1 = path1 as IPointCollection;
            for (int i = 0; i < pc1.PointCount; i++)
            {
                pc.AddPoint(pc1.get_Point(i));
            }

            var pc2 = path2 as IPointCollection;
            for (int i = 1; i < pc2.PointCount; i++)
            {
                pc.AddPoint(pc2.get_Point(i));
            }
            
            return pc as IPath;
        }

        /// <summary>
        /// 化简曲线(不能处理闭环)
        /// </summary>
        /// <param name="path">曲线</param>
        /// <param name="blendDepth">最小弯曲深度</param>
        /// <param name="blendWidth">最小开口宽度</param>
        /// <returns>化简后的曲线（副本）</returns>
        public static IPath SimplifyByDT(IPath path, double blendDepth, double blendWidth)
        {
            if (path == null)
                return path;
            if (path.IsEmpty)
                return path;
            if (path.Length < blendDepth)
                return path;

            IPath p = (path as IClone).Clone() as IPath;
            PolylineClass pl = new PolylineClass();
            pl.AddGeometry(p);
            pl.Generalize(Math.Min(blendDepth, blendWidth) / 1000.0); //去除重复点
            if (pl.PointCount < 3)//判断有效点是否大于2
            {
                return pl.get_Geometry(0) as IPath;
            }

            var site = CalBendSite(p);

            if (site == SiteType.LeftSite)
                p.ReverseOrientation();

            p = SimplifyRightSiteByDT(p, blendDepth, blendWidth);
            p.ReverseOrientation();
            p = SimplifyRightSiteByDT(p, blendDepth, blendWidth);

            if (site == SiteType.RightSite)
                p.ReverseOrientation();


            return p;
        }

        /// <summary>
        /// 化简曲线(不能处理闭环)
        /// </summary>
        /// <param name="curve">曲线(Polyline,Polygon)</param>
        /// <param name="blendDepth">最小弯曲深度</param>
        /// <param name="blendWidth">最小开口宽度</param>
        /// <returns>化简后的曲线（副本）</returns>
        public static IPolycurve SimplifyByDT(IPolycurve curve, double blendDepth, double blendWidth)
        {
            IGeometryCollection rgc = null;
            if (curve is IPolygon)
            {
                rgc = new PolygonClass();
            }
            else if (curve is IPolyline)
            {
                rgc = new PolylineClass();
            }
            else
            {
                return curve;
            }

            var gc = curve as IGeometryCollection;
            for (int i = 0; i < gc.GeometryCount; i++)
            {
                var path = gc.get_Geometry(i) as IPath ;
                if (path.IsClosed)
                {
                    var ps = SpiltRing(path);
                    var p1 = SimplifyByDT(ps[0], blendDepth, blendWidth);
                    var p2 = SimplifyByDT(ps[1], blendDepth, blendWidth);
                    var geo = MergePath(p1, p2, path is IRing);
                    rgc.AddGeometry(geo);
                }
                else
                {
                    rgc.AddGeometry(SimplifyByDT(path, blendDepth, blendWidth));
                }
            }
            (rgc as ITopologicalOperator).Simplify();
            return rgc as IPolycurve;
        }

        static IPath SimplifyRightSiteByDT(IPath path, double blendDepth, double blendWidth)
        {
            IPath p = path;
            PolylineClass pl = new PolylineClass();
            pl.AddGeometry(path);
            pl.Generalize(Math.Min(blendDepth, blendWidth) / 1000.0); //去除重复点
            //pl.Densify(Math.Min(blendDepth, blendWidth) / 3.0, 0.0);//加密
            

            //List<double> segmentLength = GetEachSegmentLength(p);
            var tin = BuildTin(pl); //注意，这里会改变pl

            p = pl.get_Geometry(0) as IPath;

            MarkSuper(tin);
            var startEdges = MarkTriangleAndEdge(tin);

            Stack<BendPathTreeNode> bends = new Stack<BendPathTreeNode>();
            foreach (var e in startEdges)
            {
                var bn = new BendTreeNode(null, e);
                var bpn = new BendPathTreeNode(null, bn);
                bends.Push(bpn);
            }
            List<ITinEdge> removedBends = new List<ITinEdge>();

            while (bends.Count > 0)
            {
                var bpn = bends.Pop();
                if (bpn.Width < blendWidth && bpn.Depth < blendDepth)
                {
                    if(bpn.BaseGut.FromNode.TagValue> bpn.BaseGut.ToNode.TagValue)
                        removedBends.Add(bpn.BaseGut);
                }
                else
                {
                    foreach (var c in bpn.Children)
                    {
                        bends.Push(c);
                    }
                }
            }

            return RemovePoints(p,removedBends);
        }

        static IPath RemovePoints(IPath p, List<ITinEdge> removedBends)
        {
            if (removedBends.Count == 0)
            {
                return (p as IClone).Clone() as IPath;
            }

            PathClass r = new PathClass();
            removedBends.Sort(new Comparison<ITinEdge>( (_l, _r) => {
                return _l.ToNode.TagValue.CompareTo(_r.ToNode.TagValue);
            }));

            int curEdgeIndex = 0;
            var pc = p as IPointCollection;
            for (int i = 0; i < pc.PointCount; i++)
            {
                if (curEdgeIndex == removedBends.Count)
                {
                    r.AddPoint(pc.get_Point(i));
                }
                else if (i <= removedBends[curEdgeIndex].ToNode.TagValue)
                {
                    r.AddPoint(pc.get_Point(i));
                }
                else if (i == removedBends[curEdgeIndex].FromNode.TagValue)
                {
                    r.AddPoint(pc.get_Point(i));
                    curEdgeIndex++;
                }
            }
            return r;
        }

        static List<double> GetEachSegmentLength(IPath p)
        {
            List<double> segmentLength = new List<double>();
            var sc = p as ISegmentCollection;
            for (int i = 0; i < sc.SegmentCount; i++)
            {
                var seg = sc.get_Segment(i);
                segmentLength.Add(seg.Length);
            }
            return segmentLength;
        }

        static TinClass BuildTin(IPath p)
        {
            TinClass tin = new TinClass();
            tin.InitNew(p.Envelope);
            tin.StartInMemoryEditing();

            //将超级节点的tag设置为maxInt
            for (int i = 1; i <= tin.NodeCount; i++)
            {
                tin.SetNodeTagValue(i, (int)NodeType.Super);
            }

            var pc = p as IPointCollection;
            //封闭的话不加入最后一点
            int ptCount = p.IsClosed ? pc.PointCount - 1 : pc.PointCount;
            for (int i = 0; i < ptCount; i++)
            {
                var pt = pc.get_Point(i);
                pt.Z = 0;

                tin.AddPointZ(pt, i);
            }
            return tin;
        }

        static TinClass BuildTin(IPolyline p)
        {
            TinClass tin = new TinClass();
            tin.InitNew(p.Envelope);
            tin.StartInMemoryEditing();

            //将超级节点的tag设置为maxInt
            for (int i = 1; i <= tin.NodeCount; i++)
            {
                tin.SetNodeTagValue(i, (int)NodeType.Super);
            }
            var pc = p as IPointCollection;
            for (int i = 0; i < pc.PointCount; i++)
            {
                var pt = pc.get_Point(i);
                pt.Z = 0;

                tin.AddPointZ(pt, i);
            }

            tin.AddPolyline(p, esriTinEdgeType.esriTinSoftEdge, 1, 0, null);

            if (tin.NodeCount != pc.PointCount + 4)
            {
                for (int i = pc.PointCount + 5; i <= tin.NodeCount; i++)
                {
                    bool hp;
                    int pi, si;
                    IPoint pt = new PointClass();
                    tin.QueryNodeAsPoint(i, pt);
                    p.SplitAtPoint(pt, false, false, out hp, out pi, out si);
                }
                tin = BuildTin(p);
            }

            for (int i = 1; i <= tin.NodeCount; i++)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("index : {0}, tag : {1}",i,tin.GetNode(i).TagValue));
            }


            return tin;
        }


        static void MarkSuper(TinClass tin)
        {
            //前4个点是超级节点
            for (int i = 1; i <= 4; i++)
            {
                var n = tin.GetNode(i);
                var tris = n.GetIncidentTriangles();
                for (int j = 0; j < tris.Count; j++)
                {
                    var tri = tris.get_Element(j);
                    tin.SetTriangleTagValue(tri.Index, (int)TriangleType.SuperTriangle);

                    for (int k = 0; k < 3; k++)
                    {
                        var e = tri.get_Edge(k);
                        tin.SetEdgeTagValue(e.Index, (int)EdgeType.SuperEdge);
                    }
                }
            }
        }

        /// <summary>
        /// 标记三角形在线的左边还是在右边
        /// </summary>
        /// <param name="tri">需要判断的三角形</param>
        /// <returns>TriangleType.RightSite 或 TriangleType.LeftSite</returns>
        static SiteType TriangleSite(ITinTriangle tri)
        {
            ITinEdge minEdge = tri.get_Edge(0);
            for (int i = 0; i < 3; i++)
            {
                var e = tri.get_Edge(i);
                if (e.FromNode.TagValue < minEdge.FromNode.TagValue)
                {
                    minEdge = e;
                }
            }
            var nEdge = minEdge.GetNextInTriangle();

            return nEdge.ToNode.TagValue > nEdge.FromNode.TagValue ?
                SiteType.RightSite : SiteType.LeftSite;
        }

        /// <summary>
        /// 标记三角形和边，需要在标记超级边后使用
        /// </summary>
        /// <param name="tin">三角网</param>
        /// <returns>右侧的边界弦</returns>
        static List<ITinEdge> MarkTriangleAndEdge(TinClass tin)
        {
            List<ITinEdge> startEdges = new List<ITinEdge>();
            for (int i = 1; i <= tin.TriangleCount; i++)
            {
                var tri = tin.GetTriangle(i);
                if (tri.TagValue == (int)TriangleType.SuperTriangle)
                {
                    continue;
                }
                var triangleType = 0;
                ITinEdge minEdge = tri.get_Edge(0); //起始点tag值最小的边
                ITinEdge startEdge = null;
                for (int j = 0; j < 3; j++)
                {
                    var e = tri.get_Edge(j);
                    if (e.FromNode.TagValue < minEdge.FromNode.TagValue)
                    {
                        minEdge = e;
                    }
                    int eType = e.ToNode.TagValue - e.FromNode.TagValue;
                    //EdgeType boundType = (e.GetNeighbor().TagValue == (int)EdgeType.SuperEdge)?EdgeType.Bound:EdgeType.Inner;
                    bool onBound = (e.GetNeighbor().TagValue == (int)EdgeType.SuperEdge);
                    switch (eType)
                    { 
                        case 1:
                        case -1:
                            tin.SetEdgeTagValue(e.Index, (int)(EdgeType.Arc ));
                            break;
                        default:
                            tin.SetEdgeTagValue(e.Index, (int)(EdgeType.Gut ));
                            triangleType += 1;
                            if (onBound)
                                startEdge = e;
                            break;
                    }                    
                }

                var nEdge = minEdge.GetNextInTriangle(); 

                SiteType triangleSite = nEdge.ToNode.TagValue > nEdge.FromNode.TagValue ?
                    SiteType.RightSite : SiteType.LeftSite;

                tin.SetTriangleTagValue(tri.Index, triangleType | (int)triangleSite);
                if (startEdge != null && triangleSite == SiteType.RightSite)
                {
                    startEdges.Add(startEdge);
                }
            }
            return startEdges;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.IO;

namespace SMGI.Plugin.MapGeneralization
{
    public class PointWarp : IComparable<PointWarp>
    {
        public IPoint Point { get; private set; }

        public PointWarp(IPoint pt)
        {
            Point = pt;
        }
        public int CompareTo(PointWarp other)
        {
            return Point.Compare(other.Point);
        }
    }
    public class PointComparer : IComparer<PointWarp>
    {
        public double E { get; set; }
        public int Compare(PointWarp x, PointWarp y)
        {
            double dx = x.Point.X - y.Point.X;
            if (dx < E && dx > -E)
            {
                double dy = x.Point.Y - y.Point.Y;
                if (dy < E && dy > -E)
                {
                    return 0;
                }
                return dy > 0 ? 1 : -1;
            }
            return dx > 0 ? 1 : -1;
        }
    }
    public class Node
    {
        /// <summary>
        /// 限定这是角度最小的一条边
        /// </summary>
        public Edge Edge { get; set; }
        public IPoint Point { get; set; }
        public int ChildGraphIndex { get; set; }
        public Node(IPoint pt)
        {
            Point = pt;
        }
    }
    public class Edge
    {
        public Node From { get; set; }
        public Node To
        {
            get
            {
                return Twin.From;
            }
        }
        public Edge Twin { get; set; }
        public Edge Next { get; set; }
        public Edge Prev { get; set; }

        //标示Stroke
        public int StrokeID { get; set; }
        public double Area { get; set; }
        public double Length { get; set; }

        public Face RightFace { get; set; }
        public Face LeftFace
        {
            get { return Twin.RightFace; }
        }
        public int Index { get; set; }
        public int CommonIndex
        {
            get
            {
                return Index / 2;
            }
        }
        public bool IsFrist { get { return (Index % 2) == 0; } }

        public int ChildGraphIndex { get { return From.ChildGraphIndex; } }

        public double Angle { get; set; }

        /// <summary>
        /// 起点相同的顺时针方向下一条边
        /// </summary>
        public Edge CWEdge { get { return Prev.Twin; } }
        /// <summary>
        /// 起点相同的逆时针方向下一条边
        /// </summary>
        public Edge CCWEdge { get { return Twin.Next; } }

        public override string ToString()
        {
            return string.Format("{0}->{1}", Index, Twin.Index); ;
        }
    }
    public class Face
    {
        public Edge Edge { get; set; }
        public double Area { get; set; }
        public int ChildGraphIndex { get { return Edge.ChildGraphIndex; } }
        public IPolygon CalPolygon(Func<int, IPointCollection> queryPointsByCommonIndexFunc)
        {
            var p = new PolygonClass();
            var cEdge = this.Edge;
            do
            {
                var shape = queryPointsByCommonIndexFunc(cEdge.CommonIndex);
                if (cEdge.IsFrist)
                {
                    p.AddPointCollection(shape);
                }
                else
                {
                    (shape as IPolycurve).ReverseOrientation();
                    p.AddPointCollection(shape);
                }
                cEdge = cEdge.Next;
            }
            while (cEdge != this.Edge);
            //p.Simplify();
            return p;
        }
    }

    public class Stroke
    {
        public int ID { get; set; }
        public double Length { get; set; }
        public double Area { get; set; }
        public List<Edge> Edges { get; set; }
        public List<Face> Faces { get; set; }

        public Stroke()
        {
            ID = -1;
            Length = 0;
            Area = 0;
            Edges = new List<Edge>();
            Faces = new List<Face>();
        }
        public void CalcLenth()
        {
            foreach (var edge in Edges)
            {
                Length += edge.Length;
            }
        }

        public void CalcArea()
        {
            foreach (var edge in Edges)
            {
                Length += edge.Area;
            }
        }
    }
    public class PlanarGraph
    {
        public int ConnectGraphCount { get; set; }
        public List<Edge> Edges { get; set; }
        public SortedDictionary<PointWarp, Node> Nodes { get; set; }
        public Dictionary<int, Node> LeftNodeOfConnectGraph { get; set; }
        public List<Face> Faces { get; set; }
        public Dictionary<int, Stroke> StrokeDic { get; set; }
        public Dictionary<int,Stroke> LockStrokeDic { get; set; }

        /// <summary>
        /// 设置参数
        /// </summary>
        public static double DANGLELENGTH = 1000;//端头沟渠长度
        public static double STROKEANGLE = 0.7;//线段夹角
        public static double SELECTAREA = 780000;//网眼面积
        public static double DANGLENMAX = 5000;//沟渠长度最长

        public Edge LeftEdgeOfConnectGraph(int childGraphIndex)
        {
            return LeftNodeOfConnectGraph[childGraphIndex].Edge;
        }

        public PlanarGraph(IComparer<PointWarp> cmp = null)
        {
            Edges = new List<Edge>();
            if (cmp != null)
                Nodes = new SortedDictionary<PointWarp, Node>(cmp);
            else
                Nodes = new SortedDictionary<PointWarp, Node>();

            Faces = new List<Face>();
            LeftNodeOfConnectGraph = new Dictionary<int, Node>();
            ConnectGraphCount = 0;

            StrokeDic = new Dictionary<int, Stroke>();
            LockStrokeDic = new Dictionary<int,Stroke>();
        }


        /// <summary>
        /// 必须是简单线要素（不能是多部件），无重复点
        /// 必须在所有交点处打断了
        /// </summary>
        /// <param name="id">唯一id值，对应edge的CommonIndex</param>
        /// <param name="shape"></param>
        /// <returns>关联的边(From点出来的边)</returns>
        public Edge AddLine(IPolyline shape, int id)
        {
            var pc = shape as IPointCollection;
            if (pc.PointCount < 2)
            {
                return null;
            }

            var fromPoint = new PointWarp(shape.FromPoint);
            var toPoint = new PointWarp(shape.ToPoint);

            var fromNode = QueryOrCreateNode(fromPoint);
            var toNode = QueryOrCreateNode(toPoint);

            var e1 = MakePair(fromNode, toNode);
            var e2 = e1.Twin;
            e1.Index = id * 2;
            e2.Index = id * 2 + 1;

            e1.Angle = CalAngle(fromPoint.Point, pc.get_Point(1));
            e2.Angle = CalAngle(toPoint.Point, pc.get_Point(pc.PointCount - 2));

            ConnectToNode(e1);
            ConnectToNode(e2);
            return e1;
        }


        public void CalConnectGraph()
        {
            Stack<Node> nStack = new Stack<Node>();
            foreach (var n in Nodes.Values)
            {
                if (n.ChildGraphIndex != 0)
                    continue;

                ConnectGraphCount++;
                LeftNodeOfConnectGraph.Add(ConnectGraphCount, n);
                nStack.Push(n);
                while (nStack.Count > 0)
                {
                    var curNode = nStack.Pop();
                    curNode.ChildGraphIndex = ConnectGraphCount;
                    var curEdge = curNode.Edge;
                    do
                    {
                        var nextNode = curEdge.To;
                        if (nextNode.ChildGraphIndex == 0)
                            nStack.Push(nextNode);
                        curEdge = curEdge.CCWEdge;
                    } while (curEdge != curNode.Edge);
                }
            }
        }

        /// <summary>
        /// 清除
        /// </summary>
        public void ClearFace()
        {
            Faces.Clear();
            foreach (var e in Edges)
            {
                e.RightFace = null;
            }
        }
        /// <summary>
        /// 计算面信息，调用前需要自己看情况先调用ClearFace()，通常第一次不需要调用，后面添加了边的时候需要
        /// </summary>
        public void CalFace()
        {
            foreach (var edge in Edges)
            {
                if (edge.RightFace != null)
                {
                    continue;
                }

                var currentEdge = edge;
                var face = new Face();
                face.Edge = currentEdge;
                face.Area = -1;
                Faces.Add(face);
                while (currentEdge.RightFace == null)
                {
                    currentEdge.RightFace = face;
                    currentEdge = currentEdge.Next;
                }
            }
        }

        public Face MergeFace(Edge ee)
        {
            var ccwe = ee.CCWEdge;
            var tccwe = ee.Twin.CCWEdge;

            var nextEE = ee.Next;
            var preTwin = ee.Twin.Prev;
            nextEE.Prev = preTwin;
            preTwin.Next = nextEE;

            var preEE = ee.Prev;
            var nextTwin = ee.Twin.Next;
            preEE.Next = nextTwin;
            nextTwin.Prev = preEE;

            //把FACE的EDGE改为余下的一条边,同时将面积合并
            Edge faceEdge = null;
            if (nextEE!=ee.Twin && nextEE.RightFace != LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace)
            {
                nextEE.RightFace.Edge = nextEE;
                nextEE.RightFace.Area += preTwin.RightFace.Area;
                preTwin.RightFace.Area = -1;

                faceEdge = nextEE;
            }
            else if (preTwin != ee.Twin && preTwin.RightFace != LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace)
            {
                preTwin.RightFace.Edge = preTwin;
                preTwin.RightFace.Area += nextEE.RightFace.Area;
                nextEE.RightFace.Area = -1;

                faceEdge = preTwin;
            }
            else if (preEE != ee.Twin && preEE.RightFace != LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace)
            {
                preEE.RightFace.Edge = preEE;
                preEE.RightFace.Area += nextTwin.RightFace.Area;
                nextTwin.RightFace.Area = -1;

                faceEdge = preEE;
            }
            else if (nextTwin != ee.Twin && nextTwin.RightFace != LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace)
            {
                nextTwin.RightFace.Edge = nextTwin;
                nextTwin.RightFace.Area += preEE.RightFace.Area;
                preEE.RightFace.Area = -1;

                faceEdge = nextTwin;
            }

            //遍历所有的Edge，确保指向被合并后的Face
            if (faceEdge!=null && faceEdge.RightFace!=faceEdge.LeftFace)
            {
                var curEdge = faceEdge;
                do
                {
                    curEdge.RightFace = faceEdge.RightFace;
                    curEdge = curEdge.Next;
                } while (curEdge != faceEdge);
            }

            //维护点
            var from = QueryOrCreateNode(new PointWarp(ee.From.Point));
            var to = QueryOrCreateNode(new PointWarp( ee.To.Point));

            if (from.Edge==ccwe)
            {
                from.Edge = ccwe;
            }

            if (to.Edge==ee)
            {
                to.Edge = tccwe;
            }

            //维护边
            Edges.Remove(ee);
            Edges.Remove(ee.Twin);

            if (faceEdge!=null)
            {
                return faceEdge.RightFace;
            }
            else
            {
                return LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace;
            }
            
        }

        public void LockFace(Face face)
        {
            var curEdge = face.Edge;
            //int count = 0;
            //do
            //{
            //    count++;
            //    if (count>100000)
            //    {
            //        break;
            //    }
            //    curEdge = curEdge.Next;
            //} while (curEdge!=face.Edge);

            //if (count < 100000)
            //{
                //curEdge = face.Edge;
            do
            {
                int sid = curEdge.StrokeID;
                LockStrokeDic[sid] = StrokeDic[sid];
                curEdge = curEdge.Next;
            } while (curEdge != face.Edge);
            //}
        }
        public bool IsStrokeLocked(int sid)
        {
            if (LockStrokeDic.Keys.Contains(sid))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool HasDangle(Stroke sk)
        {
            foreach (var e in sk.Edges)
            {
                int nCount = 0;
                var curEdge = e;
                do
                {
                    if (curEdge.StrokeID!=e.StrokeID)
                    {
                        nCount++;
                    }
                    curEdge = curEdge.CCWEdge;
                } while (curEdge!=e);

                if (nCount==1)
                {
                    return true;
                }
            }
            return false;
        }
        public List<Edge> FindDangleEdges()
        {
            List<Edge> dangleEdges = new List<Edge>();
            foreach (var edge in Edges)
            {
                if (edge.IsFrist)
                {
                    var twin = edge.Twin;
                    if ((edge.Prev == twin && twin.Next == edge) || (twin.Prev == edge && edge.Next == twin))
                    {
                        dangleEdges.Add(edge);
                    }
                }
            }
            return dangleEdges;
        }
        /// <summary>
        /// 将每一条Edge的StrokeID写入指定字段
        /// </summary>
        /// <param name="fc">指定要素类</param>
        /// <param name="sidFieldName">指定StrokeID字段</param>
        public void WriteStroke(IFeatureClass fc, string sidFieldName)
        {
            int sidIndex = fc.Fields.FindField(sidFieldName);
            if (sidIndex != -1)
            {
                foreach (var edge in Edges)
                {
                    if (edge.IsFrist)
                    {
                        var f = fc.GetFeature(edge.CommonIndex);
                        f.set_Value(sidIndex, edge.StrokeID);
                        f.Store();
                    }
                }
            }
        }


        //将每一条Edge放入Stoke集合
        public void CalcStroke()
        {
            foreach (var edge in Edges)
            {
                if (StrokeDic.Keys.Contains(edge.StrokeID))
                {
                    StrokeDic[edge.StrokeID].Edges.Add(edge);
                    if (edge.IsFrist)
                    {
                        StrokeDic[edge.StrokeID].Length += edge.Length;
                    }
                    StrokeDic[edge.StrokeID].Area += edge.Area;

                    if (!StrokeDic[edge.StrokeID].Faces.Contains(edge.RightFace))
                    {
                        StrokeDic[edge.StrokeID].Faces.Add(edge.RightFace);
                    }
                }
                else
                {
                    Stroke sk = new Stroke();
                    sk.ID = edge.StrokeID;
                    sk.Edges.Add(edge);
                    if (edge.IsFrist)
                    {
                        sk.Length += edge.Length;
                    }
                    sk.Area += edge.Area;
                    sk.Faces.Add(edge.RightFace);
                    StrokeDic[edge.StrokeID] = sk;
                }
            }
        }

        //构建Stroke连接，依次遍历每一个弧段，根据相邻弧段夹角划入唯一的Stroke
        public void ConstructStroke()
        {
            //1.顺序读取弧段集合中的弧段，判断是否归属于某一Stroke，若属于再继续判断下一个弧段，若不属于则作为起始弧段构建新的Stroke
            int sid = 0;
            foreach (var edge in Edges)
            {
                //若属于再继续判断下一个弧段
                if (edge.StrokeID != Int32.MaxValue)
                {
                    continue;
                }

                //构建新的Stroke
                var curEdge = edge;
                sid++;
                curEdge.StrokeID = sid;
                curEdge.Twin.StrokeID = sid;
                //2.首尾节点双向空间搜索，找出相邻弧段，判断该相邻弧段是否属于某一Stroke
                //2.1 前向遍历
                curEdge = edge;
                do
                {
                    var tccwEdge = curEdge.CCWEdge;
                    double minValue = Math.PI;
                    Edge minEdge = null;
                    while (curEdge != tccwEdge)
                    {
                        if (tccwEdge.StrokeID == Int32.MaxValue)
                        {
                            double calValue = 0.0;

                            calValue = Math.Abs(curEdge.Angle - tccwEdge.Angle);
                            var angle = Math.Abs(calValue - Math.PI);

                            if (minValue > angle)
                            {
                                minValue = angle;
                                minEdge = tccwEdge;
                            }
                        }
                        tccwEdge = tccwEdge.CCWEdge;
                    }
                    if (minValue < STROKEANGLE && minEdge != null)
                    {
                        minEdge.StrokeID = sid;
                        minEdge.Twin.StrokeID = sid;
                        curEdge = minEdge.Twin;
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                //2.2 后向遍历
                curEdge = edge;

                do
                {
                    var tccwEdge = curEdge.Twin.CCWEdge;
                    double minValue = Math.PI;
                    Edge minEdge = null;
                    while (curEdge.Twin != tccwEdge)
                    {
                        if (tccwEdge.StrokeID == Int32.MaxValue)
                        {
                            double calValue = 0.0;
                            calValue = Math.Abs(curEdge.Twin.Angle - tccwEdge.Angle);
                            var angle = Math.Abs(calValue - Math.PI);

                            if (minValue > angle)
                            {
                                minValue = angle;
                                minEdge = tccwEdge;
                            }
                        }
                        tccwEdge = tccwEdge.CCWEdge;
                    }
                    if (minValue < STROKEANGLE && minEdge != null)
                    {
                        minEdge.StrokeID = sid;
                        minEdge.Twin.StrokeID = sid;
                        curEdge = minEdge;
                    }
                    else
                    {
                        break;
                    }
                } while (true);

            }
        }
        #region 私有函数
        double CalAngle(IPoint from, IPoint to)
        {
            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            return Math.Atan2(dy, dx);
        }

        double Angle(IPoint cen, IPoint pt1, IPoint pt2)
        {
            double ma_x = pt1.X - cen.X, ma_y = pt1.Y - cen.Y;
            double mb_x = pt2.X - cen.X, mb_y = pt2.Y - cen.Y;
            double v1 = ma_x * mb_x + ma_y * mb_y;
            double ma_val = Math.Sqrt(ma_x * ma_x + ma_y * ma_y);
            double mb_val = Math.Sqrt(mb_x * mb_x + mb_y * mb_y);
            double cosM = v1 / (ma_val * mb_val);
            double angAmb = Math.Acos(cosM) * 180 / Math.PI;
            return angAmb;
        }
        void ConnectToNode(Edge edge)
        {
            var n = edge.To;
            var e = edge.Twin;
            var c = n.Edge;
            if (c == null)
            {
                n.Edge = e;
            }
            else if (e.Angle < c.Angle)
            {
                Connect(edge, c);
                n.Edge = e;
            }
            else
            {
                while (true)
                {
                    c = c.Twin.Next;
                    if (e.Angle < c.Angle)
                    {
                        break;
                    }
                    if (c == n.Edge)
                    {
                        break;
                    }
                }
                Connect(edge, c);
            }
        }

        Node QueryOrCreateNode(PointWarp pt)
        {
            if (!Nodes.ContainsKey(pt))
            {
                Nodes.Add(pt, new Node(pt.Point));
            }
            return Nodes[pt];
        }

        Edge MakePair(Node from, Node to)
        {
            Edge e1 = new Edge();
            Edge e2 = new Edge();
            e1.Twin = e2;
            e2.Twin = e1;
            e1.Next = e2;
            e1.Prev = e2;
            e2.Next = e1;
            e2.Prev = e1;
            e1.From = from;
            e2.From = to;

            e1.StrokeID = Int32.MaxValue;
            e2.StrokeID = Int32.MaxValue;

            Edges.Add(e1);
            Edges.Add(e2);
            return e1;
        }

        /// <summary>
        /// e1 to e2;
        /// e1's to-node equals e2's from-node
        /// e2 must be at ccw of e1
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        public void Connect(Edge e1, Edge e2)
        {
            e1.Next.Prev = e2.Prev;
            e2.Prev.Next = e1.Next;
            e1.Next = e2;
            e2.Prev = e1;
        }
        #endregion
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
namespace SMGI.River.Algrithm
{
    //河流处理 回滚版本
    public static class HydroAlgorithm
    {
        public class HydroEdge
        {
            public HydroNode From { get; set; }
            public HydroNode To { get; set; }
            public double Length { get; set; }

            public double PathLength { get; set; }
            public int PathIndex { get; set; }


            public int TreeID { get; set; }
            public int FeatureOID { get; set; }
            public IFeatureClass FeatureClass { get; set; }
            public IFeature Feature { get { return FeatureClass.GetFeature(FeatureOID); } }

            public double StartWidth
            {
                get;
                set;
            }
            public double EndWidth
            {
                get;
                set;
            }
            public HydroEdge(IFeature fe)
            {
                FeatureOID = fe.OID;
                FeatureClass = fe.Class as IFeatureClass;
                From = null;
                To = null;
                Length = (fe.Shape as IPolyline).Length;
                PathIndex = -1;
            }
           
        }

        public class HydroPath : IComparable<HydroPath>
        {
            public int Index { get; set; }
            public List<HydroEdge> Edges { get; private set; }
            public HydroNode RootNode { get; set; }
            public HydroPath(int idx)
            {
                Index = idx;
                Edges = new List<HydroEdge>();
            }
            public double Lenght { get; set; }
            public void CalLength()
            {
                Lenght = 0;
                foreach (var e in Edges)
                {
                    Lenght += e.Length;
                }
            }

            public void CalculateWidth(double start, double end)
            {
                double maxLenght = Edges[0].PathLength;
                double l = 0;
                double lastWidth = start;
                foreach (var sh in Edges)
                {
                    sh.StartWidth = lastWidth;
                    l += sh.Length;
                    lastWidth = (l / maxLenght) * (end - start) + start;
                    sh.EndWidth = lastWidth;
                }
            }

            public int CompareTo(HydroPath other)
            {
                return Lenght.CompareTo(other.Lenght);
            }
        }


        public class PointWarp : IComparable<PointWarp>
        {
            public IPoint Point { get; private set; }

            public PointWarp(IPoint pt)
            {
                Point = pt;
            }
            public int CompareTo(PointWarp other)
            {
                //if (Point.X - other.Point.X > 0.1)
                //{
                //    return 1;
                //}
                //else if (Point.X - other.Point.X < -0.1)
                //{
                //    return -1;
                //}
                //else
                //{
                //    if (Point.Y - other.Point.Y > 0.1)
                //    {
                //        return 1;
                //    }
                //    else if (Point.Y - other.Point.Y < -0.1)
                //    {
                //        return -1;
                //    }
                //    else
                //        return 0;
                //}
                return Point.Compare(other.Point);
            }
        }
        public class HydroNode
        {
            public IPoint Point { get; private set; }

            public List<HydroEdge> UpStream { get; private set; }
            public List<HydroEdge> DownStream { get; private set; }

            public HydroNode RootNode { get; set; }
            public HydroEdge DownStreamEdge { get; set; }

            public double Length { get; set; }

            public HydroNode(IPoint pt)
            {
                Point = pt;
                UpStream = new List<HydroEdge>();
                DownStream = new List<HydroEdge>();
                Length = 0;
                RootNode = null;
                DownStreamEdge = null;
            }
        }

        public class HydroGraph
        {
            public SortedList<PointWarp, HydroNode> Nodes { get; set; }
            public List<HydroEdge> Edges { get; set; }
            public HydroGraph()
            {
                Nodes = new SortedList<PointWarp, HydroNode>();
                Edges = new List<HydroEdge>();
            }

            public void Add(IFeature feature)
            {
                HydroEdge edge = new HydroEdge(feature);
                Edges.Add(edge);
                IPolyline line = feature.Shape as IPolyline;


                {//上游
                    var fromPoint = new PointWarp(line.FromPoint);
                    HydroNode node;
                    if (!Nodes.ContainsKey(fromPoint))
                    {
                        node = new HydroNode(fromPoint.Point);
                        Nodes.Add(fromPoint, node);
                    }
                    else
                    {
                        node = Nodes[fromPoint];
                    }

                    node.DownStream.Add(edge);
                    edge.From = node;
                }

                {//下游
                    var toPoint = new PointWarp(line.ToPoint);
                    HydroNode node;
                    if (!Nodes.ContainsKey(toPoint))
                    {
                        node = new HydroNode(toPoint.Point);
                        Nodes.Add(toPoint, node);
                    }
                    else
                    {
                        node = Nodes[toPoint];
                    }

                    node.UpStream.Add(edge);
                    edge.To = node;
                }
            }

            public IEnumerable<HydroNode> GetAllHydroEstuary()
            {
                return from x in Nodes.Values where x.DownStream.Count == 0 select x;
            }
            public IEnumerable<HydroNode> GetAllHydroSource()
            {
                return from x in Nodes.Values where x.UpStream.Count == 0 select x;
            }
            public IEnumerable<HydroNode> GetAllHydroBranch()
            {
                return from x in Nodes.Values where x.DownStream.Count > 1 select x;
            }
            public IEnumerable<HydroNode> GetAllPseudoNode()
            {
                return from x in Nodes.Values
                       where x.DownStream.Count == 1 && x.UpStream.Count == 1
                       select x;
            }

            class HydroNodeComparer : IComparer<HydroNode>
            {
                public int Compare(HydroNode x, HydroNode y)
                {
                    if (x == y)
                        return 0;
                    else
                        return (x.Length > y.Length) ? 1 : -1;
                }
            }
        
            public List<HydroGraph> BuildTrees()
            {
                List<HydroGraph> trees = new List<HydroGraph>();
              
                foreach (var edge in Edges)
                {
                    edge.TreeID = -1;
                }
                //广度 搜索
                int flag=1;
                foreach (var edge in Edges)
                {
                    if (edge.TreeID == -1)
                    {
                        Queue<HydroEdge> queues = new Queue<HydroEdge>();
                        queues.Enqueue(edge);
                        DFS(queues, flag);
                        flag++;
                    }
                }
                // 绘制
                Dictionary<int, IColor> colorDic = new Dictionary<int, IColor>();
                Random rom = new Random();

                for (int i = 1; i <= flag; i++)
                {
                    RgbColorClass rgb = new RgbColorClass();
                    rgb.Red = rom.Next(1, 250);
                    rgb.Green = rom.Next(1, 250);
                    rgb.Blue = rom.Next(1, 250);
                    colorDic[i] = rgb;
                    HydroGraph treegraph = new HydroGraph();
                    trees.Add(treegraph);
                }
                foreach (var edge in Edges)
                {
                    TextElementClass txtEle = new TextElementClass();
                    txtEle.Text = edge.TreeID.ToString();
                    txtEle.Geometry = edge.From.Point;
                    var gh = GApplication.Application.ActiveView as IGraphicsContainer;

                    gh.AddElement(txtEle, 0);

                    //构建树
                    trees[edge.TreeID-1].Edges.Add(edge);
                    //下游点
                    var toPoint = new PointWarp(edge.To.Point);
                    if (!trees[edge.TreeID-1].Nodes.ContainsKey(toPoint))
                    {
                        trees[edge.TreeID-1].Nodes.Add(toPoint, edge.To);
                    }
                    //上游点
                    var fromPoint = new PointWarp(edge.From.Point);
                    if (!trees[edge.TreeID-1].Nodes.ContainsKey(fromPoint))
                    {
                        trees[edge.TreeID - 1].Nodes.Add(fromPoint, edge.From);
                    }
                   
                }
              
                
                return trees;

            }
            //深度优先
            private void DFS(Queue<HydroEdge> queues,int id)
            {
                while (queues.Count > 0)
                {
                    var edge = queues.Dequeue();
                    if (edge.TreeID == -1)
                    {
                        //设置访问标志
                        edge.TreeID = id;
                        //邻接
                        var upedges = edge.From.UpStream;
                        foreach (var e in upedges)
                        {
                            if (e.TreeID == -1)
                            {
                                queues.Enqueue(e);
                            }
                        }
                        var downedges = edge.To.DownStream;
                        foreach (var e in downedges)
                        {
                            if (e.TreeID == -1)
                            {
                                queues.Enqueue(e);
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// 获取水系路径
            /// </summary>
            /// <returns>排好序的水系路径</returns>
            public List<HydroPath> BuildHydroPaths(WaitOperation wo = null)
            {
                List<HydroPath> paths = new List<HydroPath>();
                Dijkstra(wo);
                var edges = GetSourceEdge();

                foreach (var e in edges)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在遍历要素【{0}】...", e.FeatureOID));

                    HydroPath path = new HydroPath(paths.Count);
                    var currentEdge = e;
                    while (true)
                    {
                        currentEdge.PathIndex = path.Index;
                        path.Edges.Add(currentEdge);

                        var n = currentEdge.To;
                        if (n.DownStreamEdge == null)
                            break;
                        if (n.DownStreamEdge.PathIndex != -1)
                            break;

                        currentEdge = n.DownStreamEdge;
                    }
                    path.RootNode = currentEdge.To;
                    paths.Add(path);
                }
                return paths;
            }
            private IEnumerable<HydroEdge> GetSourceEdge()
            {
                return from x in Edges
                       where x.From.UpStream.Count == 0 || x.From.DownStreamEdge != x
                       orderby x.Length + x.To.Length descending
                       select x;
            }
            private void Dijkstra(WaitOperation wo = null)
            {
                var roots = GetAllHydroEstuary();

                SortedSet<HydroNode> queue = new SortedSet<HydroNode>(new HydroNodeComparer());

                foreach (var root in roots)
                {
                    root.RootNode = root;
                    root.Length = 0;
                    queue.Add(root);
                }
                while (queue.Count > 0)
                {
                    var curent = queue.Min;
                    queue.Remove(curent);
                    //System.Diagnostics.Debug.WriteLine(curent.Length);
                    foreach (var edge in curent.UpStream)
                    {
                        if (wo != null)
                            wo.SetText(string.Format("正在构建河网最短路径，还剩【{0}】/【{1}】条 \n当前正在构建河口的要素ID为【{2}】的最短路径:【{3}】......", queue.Count(), roots.Count(), curent.UpStream.First().FeatureOID, edge.FeatureOID));

                        var upNode = edge.From;
                        edge.PathLength = curent.Length + edge.Length;
                        if (upNode.RootNode == null)//没有被遍历到
                        {
                            upNode.RootNode = curent.RootNode;
                            upNode.Length = edge.PathLength;
                            upNode.DownStreamEdge = edge;
                            queue.Add(upNode);
                        }
                        else if (upNode.RootNode == curent.RootNode)//已经被同一河源遍历到
                        {
                            if (upNode.Length > edge.PathLength)
                            {
                                queue.Remove(upNode);
                                upNode.Length = edge.PathLength;
                                upNode.DownStreamEdge = edge;
                                queue.Add(upNode);
                            }
                        }
                        else//已经被其他河源遍历到
                        {
                            if (upNode.Length < edge.PathLength)
                            {
                                queue.Remove(upNode);
                                upNode.RootNode = curent.RootNode;
                                upNode.Length = edge.PathLength;
                                upNode.DownStreamEdge = edge;
                                queue.Add(upNode);
                            }
                        }
                    }
                }
            }
        }
    }
    //新版 河流处理（zq已改 有问题 )
    public static class HydroAlgorithmEx
    {
        public class HydroEdge
        {
            public HydroNode From { get; set; }
            public HydroNode To { get; set; }
            public double Length { get; set; }

            public double PathLength { get; set; }
            public int PathIndex { get; set; }

            public int FeatureOID { get; set; }
            public IFeatureClass FeatureClass { get; set; }
            public IFeature Feature { get { return FeatureClass.GetFeature(FeatureOID); } }

            public double StartWidth
            {
                get;
                set;
            }
            public double EndWidth
            {
                get;
                set;
            }
            public HydroEdge(IFeature fe)
            {
                FeatureOID = fe.OID;
                FeatureClass = fe.Class as IFeatureClass;
                From = null;
                To = null;
                Length = (fe.Shape as IPolyline).Length;
                PathIndex = -1;
            }

        }

        public class HydroPath : IComparable<HydroPath>
        {
            public int Index { get; set; }
            public List<HydroEdge> Edges { get; private set; }
            public HydroNode RootNode { get; set; }
            public HydroNode FromNode { get { return Edges[0].From; } }
            public HydroPath(int idx)
            {
                Index = idx;
                Edges = new List<HydroEdge>();
            }
            public double Lenght { get; set; }
            public bool IsForkRiver
            {
                get
                {
                    var e = this.Edges[0];
                    return e != e.From.DownStreamEdge;
                }
            }

            public void CalLength()
            {
                Lenght = 0;
                foreach (var e in Edges)
                {
                    Lenght += e.Length;
                }
            }

            public void CalculateWidth(double start, double end)
            {
                double maxLenght = Edges[0].PathLength - this.RootNode.LengthToRoot;
                double l = 0;
                double lastWidth = start;
                foreach (var sh in Edges)
                {
                    if (sh.FeatureOID == 7757)
                    {
                    }
                    sh.StartWidth = lastWidth;
                    l += sh.Length;
                    lastWidth = HydroPath.CalculateWidth(maxLenght, l, start, end);
                    sh.EndWidth = lastWidth;
                }
            }

            public static double CalculateWidth(double maxLenght, double currentLength, double startWidth, double EndWidth)
            {
                return (Math.Sqrt(currentLength) / Math.Sqrt(maxLenght)) * (EndWidth - startWidth) + startWidth;
                // return (currentLength * currentLength / (maxLenght * maxLenght)) * (EndWidth - startWidth) + startWidth;
            }

            public int CompareTo(HydroPath other)
            {
                return Lenght.CompareTo(other.Lenght);
            }
        }


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
        public class HydroNode
        {
            public IPoint Point { get; private set; }

            public List<HydroEdge> UpStream { get; private set; }
            public List<HydroEdge> DownStream { get; private set; }

            public HydroNode RootNode { get; set; }
            public HydroEdge DownStreamEdge { get; set; }

            public double LengthToRoot { get; set; }

            public HydroNode(IPoint pt)
            {
                Point = pt;
                UpStream = new List<HydroEdge>();
                DownStream = new List<HydroEdge>();
                LengthToRoot = 0;
                RootNode = null;
                DownStreamEdge = null;
            }

        }

        public class HydroGraph
        {
            public SortedList<PointWarp, HydroNode> Nodes { get; set; }
            public List<HydroEdge> Edges { get; set; }
            public HydroGraph()
            {
                Nodes = new SortedList<PointWarp, HydroNode>();
                Edges = new List<HydroEdge>();
            }

            public void Add(IFeature feature)
            {
                HydroEdge edge = new HydroEdge(feature);
                Edges.Add(edge);
                IPolyline line = feature.Shape as IPolyline;


                {//上游
                    var fromPoint = new PointWarp(line.FromPoint);
                    HydroNode node;
                    if (!Nodes.ContainsKey(fromPoint))
                    {
                        node = new HydroNode(fromPoint.Point);
                        Nodes.Add(fromPoint, node);
                    }
                    else
                    {
                        node = Nodes[fromPoint];
                    }

                    node.DownStream.Add(edge);
                    edge.From = node;
                }

                {//下游
                    var toPoint = new PointWarp(line.ToPoint);
                    HydroNode node;
                    if (!Nodes.ContainsKey(toPoint))
                    {
                        node = new HydroNode(toPoint.Point);
                        Nodes.Add(toPoint, node);
                    }
                    else
                    {
                        node = Nodes[toPoint];
                    }

                    node.UpStream.Add(edge);
                    edge.To = node;
                }
            }

            public IEnumerable<HydroNode> GetAllHydroEstuary()
            {
                return from x in Nodes.Values where x.DownStream.Count == 0 select x;
            }
            public IEnumerable<HydroNode> GetAllHydroSource()
            {
                return from x in Nodes.Values where x.UpStream.Count == 0 select x;
            }
            public IEnumerable<HydroNode> GetAllHydroBranch()
            {
                return from x in Nodes.Values where x.DownStream.Count > 1 select x;
            }
            public IEnumerable<HydroNode> GetAllPseudoNode()
            {
                return from x in Nodes.Values
                       where x.DownStream.Count == 1 && x.UpStream.Count == 1
                       select x;
            }

            class HydroNodeComparer : IComparer<HydroNode>
            {
                public int Compare(HydroNode x, HydroNode y)
                {
                    if (x == y)
                        return 0;
                    else
                        return (x.LengthToRoot > y.LengthToRoot) ? 1 : -1;
                }
            }
            /// <summary>
            /// 获取水系路径
            /// </summary>
            /// <returns>排好序的水系路径</returns>
            public List<HydroPath> BuildHydroPaths(WaitOperation wo = null)
            {
                List<HydroPath> paths = new List<HydroPath>();
                Dijkstra(wo);
                var edges = GetSourceEdge();

                foreach (var e in edges)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在遍历要素【{0}】...", e.FeatureOID));

                    HydroPath path = new HydroPath(paths.Count);
                    var currentEdge = e;
                    while (true)
                    {
                        currentEdge.PathIndex = path.Index;
                        path.Edges.Add(currentEdge);

                        var n = currentEdge.To;
                        if (n.DownStreamEdge == null)
                            break;
                        if (n.DownStreamEdge.PathIndex != -1)
                            break;

                        currentEdge = n.DownStreamEdge;
                    }
                    path.RootNode = currentEdge.To;
                    paths.Add(path);
                }
                return paths;
            }
            private IEnumerable<HydroEdge> GetSourceEdge()
            {
                return from x in Edges
                       where x.From.UpStream.Count == 0 || x.From.DownStreamEdge != x
                       orderby x.Length + x.To.LengthToRoot descending
                       select x;
            }
            private void Dijkstra(WaitOperation wo = null)
            {
                var roots = GetAllHydroEstuary();

                SortedSet<HydroNode> queue = new SortedSet<HydroNode>(new HydroNodeComparer());

                foreach (var root in roots)
                {
                    root.RootNode = root;
                    root.LengthToRoot = 0;
                    queue.Add(root);
                }
                while (queue.Count > 0)
                {
                    var curent = queue.Min;
                    queue.Remove(curent);
                    //System.Diagnostics.Debug.WriteLine(curent.Length);
                    foreach (var edge in curent.UpStream)
                    {
                        if (wo != null)
                            wo.SetText(string.Format("正在构建河网最短路径，还剩【{0}】/【{1}】条 \n当前正在构建河口的要素ID为【{2}】的最短路径:【{3}】......", queue.Count(), roots.Count(), curent.UpStream.First().FeatureOID, edge.FeatureOID));

                        var upNode = edge.From;
                        edge.PathLength = curent.LengthToRoot + edge.Length;
                        if (upNode.RootNode == null)//没有被遍历到
                        {
                            upNode.RootNode = curent.RootNode;
                            upNode.LengthToRoot = edge.PathLength;
                            upNode.DownStreamEdge = edge;
                            queue.Add(upNode);
                        }
                        else if (upNode.RootNode == curent.RootNode)//已经被同一河源遍历到
                        {
                            if (upNode.LengthToRoot > edge.PathLength)
                            {
                                queue.Remove(upNode);
                                upNode.LengthToRoot = edge.PathLength;
                                upNode.DownStreamEdge = edge;
                                queue.Add(upNode);
                            }
                        }
                        else//已经被其他河源遍历到
                        {
                            if (upNode.LengthToRoot < edge.PathLength)
                            {
                                queue.Remove(upNode);
                                upNode.RootNode = curent.RootNode;
                                upNode.LengthToRoot = edge.PathLength;
                                upNode.DownStreamEdge = edge;
                                queue.Add(upNode);
                            }
                        }
                    }
                }
            }
        }
    }
}
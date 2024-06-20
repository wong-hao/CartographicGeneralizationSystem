using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geometry;
namespace BuildingGenCore
{
    public class SimplifyBuildingGeneralizer
    {
        /// <summary>
        /// 聚类之间最小相差角度
        /// </summary>
        public double paraAngle;
        /// <summary>
        /// 是不是长边
        /// </summary>
        public double paraLength;
        public SimplifyBuildingGeneralizer()
        {
            paraAngle = Math.PI / 60;
            paraLength = 5;
        }

        public IPolygon Generalize(IPolygon polygon)
        {
            //1.获取聚类信息
            SegClassCluster cluster = new SegClassCluster(new List<IPolygon>() { polygon }, paraAngle);

            PolygonClass poly = new PolygonClass();
            for (int i = 0; i < (polygon as IGeometryCollection).GeometryCount; i++)
            {
                IGeometry ring = SimplifyRing(cluster, 0, i);
                if (ring != null)
                {
                    poly.AddGeometries(1, ref ring);
                }
            }
            poly.SimplifyEx(true, true, true);

            return poly;
        }
        private IRing SimplifyRing(SegClassCluster cluseter, int polyIndex, int ringIndex)
        {
            List<SegInfo> segInfos = cluseter.SegInfos[polyIndex][ringIndex];

            List<PathInfo> paths = new List<PathInfo>();
            #region 1.获取各方向的移动信息,保存在paths中，这一步做完后，每两个相邻的移动区间的移动方向都不相同
            if (true)
            {
                PathInfo currentMoveInfo = null;
                for (int i = 0; i < segInfos.Count; i++)
                {
                    SegInfo info = segInfos[i];
                    if (currentMoveInfo == null)
                    {
                        currentMoveInfo = new PathInfo(info, i);
                        paths.Add(currentMoveInfo);
                        continue;
                    }
                    if (!currentMoveInfo.Add(info))
                    {
                        currentMoveInfo = new PathInfo(info, i);
                        paths.Add(currentMoveInfo);
                    }
                }
                if (paths.Count > 1)
                {
                    if (paths[paths.Count - 1].Merge(paths[0]))
                    {
                        paths.RemoveAt(0);
                    }
                }
            }
            #endregion

            #region 2.从最长的边开始遍历
            if (true)
            {

                //找到最长的一条边，与参数比较，如果小于最小边长度，则改参数为1/2；
                int beginIndex = 0;
                double maxLength = 0;
                for (int i = 0; i < paths.Count; i++)
                {
                    if (Math.Abs(paths[i].MainLength) > maxLength)
                    {
                        maxLength = Math.Abs(paths[i].MainLength);
                        beginIndex = i;
                        break;
                    }
                }

                List<GLine> lines = new List<GLine>();
                int currentIndex = beginIndex;
                List<PathAccumulation> unusedPath = new List<PathAccumulation>();
                PathAccumulation LastAccumulation = null;
                bool lastOrg = true;
                bool begin = false;
                while (true)
                {
                    currentIndex = SafeIndex(currentIndex, paths.Count);
                    #region   找回到一圈了,函数出口处
                    if (SafeIndex(currentIndex, paths.Count) == beginIndex && begin)
                    {
                        if (LastAccumulation != null)
                        {
                            lines.Add(LastAccumulation.GetLine(lastOrg, beginIndex));
                            lines.Add(LastAccumulation.GetLine(!lastOrg, beginIndex));
                        }
                        lines = MergeParellelLine(lines);
                        //什么都没找到
                        if (lines.Count == 0)
                        {
                            return null;
                        }
                        //会不会只有一条？
                        else if (lines.Count == 1)
                        {
                            return null;
                        }
                        //会不会只有两条？
                        else if (lines.Count == 2)
                        {
                            return null;
                        }
                        else
                        {
                            RingClass ring = new RingClass();
                            GLine lastLine = lines[lines.Count - 1];
                            object miss = Type.Missing;
                            foreach (GLine line in lines)
                            {
                                System.Diagnostics.Debug.WriteLine(String.Format("角度:{0},X:{1},Y:{2}", (line.a * 180 / Math.PI).ToString("###"), ((int)line.p.X) % 1000, ((int)line.p.Y) % 1000));
                                IPoint p = line.InsertPoint(lastLine);
                                if (p != null)
                                {
                                    ring.AddPoint(p, ref miss, ref miss);
                                }
                                lastLine = line;
                            }
                            ring.Close();

                            return ring;
                        }
                    }
                    #endregion

                    PathAccumulation existItem = null;

                    if (LastAccumulation != null && LastAccumulation.Add(currentIndex))
                    {
                        existItem = LastAccumulation;
                    }
                    else
                    {
                        foreach (PathAccumulation item in unusedPath)
                        {
                            if (item.Add(currentIndex))
                            {
                                existItem = item;
                                break;
                            }
                        }
                    }
                    if (existItem == null)
                    {
                        existItem = new PathAccumulation(paths, currentIndex);
                        unusedPath.Add(existItem);
                    }

                    if (existItem == LastAccumulation)
                    {
                        //if(existItem.LargeThanPara(lastOrg,paraLength))
                        //{
                        //    lines.Add(existItem.GetLine(lastOrg, currentIndex + 1));
                        //    lastOrg = !lastOrg;
                        //}
                        if (existItem.LargeThanPara(!lastOrg, paraLength))
                        {
                            lines.Add(existItem.GetLine(!lastOrg, currentIndex + 1));
                            lastOrg = !lastOrg;
                        }
                    }
                    else
                    {
                        if (existItem.LargeThanPara(true, paraLength))
                        {
                            if (LastAccumulation != null)
                            {
                                lines.Add(LastAccumulation.GetLine(lastOrg, currentIndex + 1));
                                unusedPath.Add(LastAccumulation);
                            }
                            LastAccumulation = existItem;
                            //lastChanged = true;
                            lastOrg = true;
                            unusedPath.Remove(existItem);
                        }
                        if (existItem.LargeThanPara(false, paraLength))
                        {
                            if (LastAccumulation != null)
                            {
                                lines.Add(LastAccumulation.GetLine(lastOrg, currentIndex + 1));
                                unusedPath.Add(LastAccumulation);
                            }
                            LastAccumulation = existItem;
                            //lastChanged = true;
                            lastOrg = false;
                            unusedPath.Remove(existItem);
                        }
                    }
                    currentIndex++;
                    begin = true;
                }
            }
            #endregion

            //return null;
        }

        private List<GLine> MergeParellelLine(List<GLine> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                return lines;
            }
            
            int[] relation = new int[lines.Count];

            List<GLine> Kinds = new List<GLine>();
            for (int i = 0; i < lines.Count; i++)
            {
                GLine line = lines[i];
                int k = -1;
                for (int j = 0; j < Kinds.Count; j++)
                {
                    GLine kind = Kinds[j];
                    if (line.ParallelWith(kind))
                    {
                        k = j;
                        break;
                    }
                }
                if (k == -1)
                {
                    Kinds.Add(line);
                    k = Kinds.Count;
                }
                relation[i] = k;
            }
            int beginIndex = 0;
            int lastKind = 1;
            for (int i = 0; i < relation.Length -1; i++)
            {
                if (relation[i] != relation[i + 1])
                {
                    beginIndex = i +1;
                    lastKind = relation[i +1];
                    break;
                }
            }
            int currentIndex =beginIndex;
            List<GLine> group = new List<GLine>();
            List<GLine> newLines = new List<GLine>();
            do
            {
                if (relation[currentIndex] == lastKind)
                {
                    group.Add(lines[currentIndex]);
                }
                else
                {
                    if (group.Count > 0)
                    {
                        IPoint p = new PointClass();
                        p.X = 0;
                        p.Y = 0;
                        foreach (GLine item in group)
                        {
                            p.X += item.p.X / group.Count;
                            p.Y += item.p.Y / group.Count;
                        }
                        newLines.Add(new GLine(group[0].a,p));
                    }
                    group.Clear();
                    group.Add(lines[currentIndex]);
                    lastKind = relation[currentIndex];
                }
                currentIndex = SafeIndex(currentIndex + 1, lines.Count);
            }
            while (currentIndex != beginIndex);
            return newLines;
        }

        private int SafeIndex(int index, int Count)
        {
            if (index < 0)
            {
                return SafeIndex(index + Count, Count);
            }
            else if (index >= Count)
            {
                return SafeIndex(index - Count, Count);
            }
            else
            {
                return index;
            }
        }

    }


    internal class PathAccumulation
    {
        PathAccumulation pair;
        private List<PathInfo> AllPaths;
        private SegClassInfo ClassInfo { get; set; }
        //方向与第一个的方向相同
        private bool Orthogonality { get; set; }
        internal double MainLength { get; private set; }
        internal double LessLength
        {
            get
            {
                return pair.MainLength + this.lessLength;
            }
            private set
            {
                this.lessLength = value - pair.MainLength;
            }
        }
        private double lessLength;
        private List<int> Indexs { get; set; }
        private int UsedIndx { get; set; }
        private double Angle
        {
            get;
            set;
        }
        private double OrtAngle
        {
            get
            {
                return pair.Angle;
            }
        }
        private PathAccumulation()
        {
        }
        internal void init(List<PathInfo> path, SegClassInfo cls, bool otrhogonality, double angle, PathAccumulation pair)
        {
            AllPaths = path;
            ClassInfo = cls;
            Orthogonality = otrhogonality;
            Angle = angle;
            Indexs = new List<int>();
            MainLength = 0;
            lessLength = 0;
            UsedIndx = 0;
            if (pair == null)
            {
                double pAngle = (angle < Math.PI / 2) ? (angle + Math.PI / 2) : (angle - Math.PI / 2);
                this.pair = new PathAccumulation();
                this.pair.init(path, cls, !otrhogonality, pAngle, this);
            }
            else
            {
                this.pair = pair;
            }

        }
        internal PathAccumulation(List<PathInfo> path, int beginIndex)
        {
            init(path, path[beginIndex].ClassInfo, path[beginIndex].Orthogonality, path[beginIndex].Angle, null);
            Add(beginIndex);
        }
        internal bool Add(int index)
        {
            PathInfo info = AllPaths[index];
            if (info.ClassInfo != this.ClassInfo)
            {
                return false;
            }
            if (this.Orthogonality == info.Orthogonality)
            {
                this.MainLength += info.MainLength;
                this.LessLength += info.LessLength;
                Indexs.Add(index);
            }
            else
            {
                pair.Add(index);
            }
            return true;
        }
        internal GLine GetLine(bool SameOrg, int toIndex)
        {
            if (!SameOrg)
            {
                return pair.GetLine(true, toIndex);
            }
            if (Indexs.Count == 0)
            {
                this.MainLength = 0;
                this.lessLength = this.lessLength / 2;
                IPoint fromP = AllPaths[pair.Indexs[pair.Indexs.Count -1]].ToPoint;
                return new GLine(Angle, fromP); 
            }
            UsedIndx = SafeIndex(UsedIndx, Indexs.Count);
            IPoint from = (AllPaths[Indexs[UsedIndx]].FromPoint as ESRI.ArcGIS.esriSystem.IClone).Clone() as IPoint;
            IPoint to = from;
            int fromIndex = Indexs[UsedIndx];
            List<PathInfo> other = new List<PathInfo>();
            toIndex = SafeIndex(toIndex, AllPaths.Count);
            for (int i = fromIndex; i != toIndex; i = SafeIndex(i + 1, AllPaths.Count))
            {
                i = SafeIndex(i, AllPaths.Count);
                PathInfo info = AllPaths[i];
                if (Indexs.Contains(i))
                {
                    info.MoveFromPoint(to);
                    to = info.ToPoint;
                    if (SameOrg)
                    {
                        info.MainUsed = true;
                    }
                    else
                    {
                        info.LessUsed = true;
                    }
                    UsedIndx++;
                }
                else
                {
                    if (!info.MainUsed)
                        other.Add(info);
                }
            }
            GLine line = null;

            this.MainLength = 0;
            this.lessLength = this.lessLength / 2;
            IPoint p = new PointClass();
            p.X = (from.X + to.X) / 2;
            p.Y = (from.Y +to.Y)/2;
            line = new GLine(Angle, p);

            foreach (PathInfo info in other)
            {
                info.MoveFromPoint(to);
                to = info.ToPoint;
            }
            return line;

        }
        internal bool LargeThanPara(bool org, double paraLength)
        {
            double orgLength;
            if (org)
            {
                orgLength = Math.Abs(this.MainLength);
            }
            else
            {
                orgLength = Math.Abs(this.LessLength);
            }
            return (orgLength > paraLength);
        }
        private int SafeIndex(int index, int Count)
        {
            if (Count == 0)
            {
                return -1;
            }
            if (index < 0)
            {
                return SafeIndex(index + Count, Count);
            }
            else if (index >= Count)
            {
                return SafeIndex(index - Count, Count);
            }
            else
            {
                return index;
            }
        }

    }

    internal class PathInfo
    {
        internal bool MainUsed = false;
        internal bool LessUsed = false;
        internal SegClassInfo ClassInfo;
        internal IPoint FromPoint
        {
            get { return SegInfos[0].Segment.FromPoint; }
        }
        internal IPoint ToPoint
        {
            get { return SegInfos[SegInfos.Count - 1].Segment.ToPoint; }
        }
        internal int BeginIndex { get; private set; }
        internal double X { get; private set; }
        internal double Y { get; private set; }

        internal double Angle
        {
            get
            {
                return Orthogonality ? (ClassInfo.Angle + Math.PI / 2) : (ClassInfo.Angle);
            }
        }
        internal bool Orthogonality
        {
            get;
            private set;
        }
        internal double MainLength
        {
            get
            {
                return Orthogonality ? Y : X;
            }
        }
        internal double LessLength
        {
            get
            {
                return Orthogonality ? X : Y;
            }
        }

        internal List<SegInfo> SegInfos { get; private set; }
        internal PathInfo(SegInfo info, int beginIndex)
        {
            this.BeginIndex = beginIndex;
            SegInfos = new List<SegInfo>();
            SegInfos.Add(info);
            this.Orthogonality = info.Orthogonality;
            ClassInfo = info.Class;
            X = info.XLength;
            Y = info.YLength;
        }

        /// <summary>
        /// 向移动信息中添加一条信息
        /// </summary>
        /// <param name="info">要添加的段信息</param>
        /// <returns>如果段不属于这一类、
        /// 或者段的XY方向与当前移动XY方向不同，
        /// 则不添加并返回<value>false</value></returns>
        internal bool Add(SegInfo info)
        {
            if (info.Class != ClassInfo
                || info.Orthogonality != this.Orthogonality
                //|| this.Positive != info.
                )
            {
                return false;
            }
            this.SegInfos.Add(info);

            this.X += info.XLength;
            this.Y += info.YLength;
            return true;
        }
        internal bool Merge(PathInfo other)
        {
            if (!SameWith(other))
            {
                return false;
            }
            this.SegInfos.AddRange(other.SegInfos);
            this.X += other.X;
            this.Y += other.Y;
            return true;
        }
        internal bool SameWith(PathInfo other)
        {
            return (
                other.ClassInfo == ClassInfo
                && other.Orthogonality == this.Orthogonality
                );
        }
        internal bool OrthogonalityWith(PathInfo other)
        {
            return (
                other.ClassInfo == ClassInfo
                && other.Orthogonality != this.Orthogonality
                );
        }
        internal void Move(double dx, double dy)
        {
            foreach (SegInfo info in SegInfos)
            {
                info.Move(dx, dy);
            }
        }
        internal void MoveFromPoint(IPoint p)
        {
            Move(p.X - FromPoint.X, p.Y - FromPoint.Y);
        }
    }

    internal class SegClassCluster
    {
        public List<IPolygon> Polygons;
        public List<SegClassInfo> Classes;
        public List<List<List<SegInfo>>> SegInfos;
        public SegClassCluster(List<IPolygon> Polygons, double angle)
        {
            this.Polygons = Polygons;
            #region 1.构建两个列表
            List<SegInfo> infos = new List<SegInfo>();
            SegInfos = new List<List<List<SegInfo>>>();
            int segIndex = 0;
            for (int k = 0; k < Polygons.Count; k++)
            {
                List<List<SegInfo>> pSegInfos = new List<List<SegInfo>>();
                IPolygon poly = Polygons[k];
                for (int i = 0; i < (poly as IGeometryCollection).GeometryCount; i++)
                {
                    List<SegInfo> rSegInfos = new List<SegInfo>();
                    IRing ring = (poly as IGeometryCollection).get_Geometry(i) as IRing;
                    for (int j = 0; j < (ring as ISegmentCollection).SegmentCount; j++)
                    {
                        ISegment seg = (ring as ISegmentCollection).get_Segment(j);
                        SegInfo info = new SegInfo(seg, k, i, j, segIndex++);
                        infos.Add(info);
                        rSegInfos.Add(info);
                    }
                    pSegInfos.Add(rSegInfos);
                }
                SegInfos.Add(pSegInfos);
            }
            #endregion

            #region 2.分组
            //2.1 排序
            infos.Sort(new Comparison<SegInfo>(SegInfoCmp));
            //2.2 分组
            Classes = new List<SegClassInfo>();
            SegClassInfo currentClass = null;
            SegInfo lastInfo = null;
            foreach (SegInfo info in infos)
            {
                if (currentClass == null)
                {
                    currentClass = new SegClassInfo(info);
                    Classes.Add(currentClass);
                }
                else
                {
                    if (currentClass.Angle - info.Angle > angle)
                    {
                        currentClass = new SegClassInfo(info);
                        Classes.Add(currentClass);
                    }
                    else
                    {
                        currentClass.Add(info);
                    }
                }
                info.Class = currentClass;
                lastInfo = info;
            }
            //2.3处理前后两组,如果第一组的第一个和最后一组的最后一个角度相差不大，则合并
            if (Classes.Count > 1)
            {
                SegClassInfo c2 = Classes[0];
                SegClassInfo c1 = Classes[Classes.Count - 1];
                if ((c1.Angle + Math.PI / 2) - c2.Angle < angle / 2)
                {
                    double l = c1.Length + c2.Length;
                    double a = ((c1.Angle + Math.PI / 2) * c1.Length + c2.Angle + c2.Length) / l;

                    foreach (SegInfo info in c2.Segment)
                    {
                        info.Class = c1;
                    }
                    c1.Segment.AddRange(c2.Segment);

                    c1.Angle = (a > Math.PI / 2) ? (a - Math.PI / 2) : a;
                    c1.Length = l;

                    Classes.RemoveAt(0);
                }
            }
            #endregion
        }

        int SegInfoCmp(SegInfo info1, SegInfo info2)
        {
            return (info1.Angle - info2.Angle) > 0 ? -1 : 1;
        }

    }
    internal class SegClassInfo
    {
        internal double Angle;
        internal double Length;
        internal List<SegInfo> Segment;
        internal SegClassInfo(SegInfo info)
        {
            Segment = new List<SegInfo>();
            Segment.Add(info);
            Angle = info.Angle;
            Length = info.Length;
            info.Class = this;
        }
        internal void Add(SegInfo info)
        {
            Segment.Add(info);
            Angle = (Angle * Length + info.Angle * info.Length) / (Length + info.Length);
            Length += info.Length;
        }
    }
    internal class SegInfo
    {
        public ISegment Segment { get; set; }
        public int PolygonIndex { get; set; }
        public int RingIndex { get; set; }
        public int SegIndex { get; set; }
        public int Index { get; set; }
        public SegClassInfo Class { get; set; }
        /// <summary>
        /// 线段的实际角度
        /// </summary>
        public double Angle
        {
            get
            {
                return AngleLessThanHalfPi((Segment as ILine).Angle);
            }
        }
        public double Length
        {
            get { return Segment.Length; }
        }
        public SegInfo(ISegment seg, int polygonIndex, int RingIndex, int SegIndex, int index)
        {
            this.Segment = (seg as ESRI.ArcGIS.esriSystem.IClone).Clone() as ISegment;
            this.PolygonIndex = polygonIndex;
            this.RingIndex = RingIndex;
            this.SegIndex = SegIndex;
            this.Index = index;
        }
        private double AngleLessThanHalfPi(double a)
        {
            while (a < 0 || a > Math.PI / 2)
            {
                if (a < 0)
                {
                    a += Math.PI / 2;
                }
                else
                {
                    a -= Math.PI / 2;
                }
            }
            return a;
        }

        internal void Move(double dx, double dy)
        {
            (Segment as ITransform2D).Move(dx, dy);
        }
        /// <summary>
        /// 根据分类获得最接近分类的角度
        /// </summary>
        public double NearestAngle
        {
            get
            {
                return Orthogonality ? (Class.Angle + Math.PI / 2) : Class.Angle;
            }
        }

        public double XLength
        {
            get
            {
                return Length * Math.Cos((Segment as ILine).Angle - Angle);
            }
        }
        public double YLength
        {
            get
            {
                return Length * Math.Sin((Segment as ILine).Angle - Angle);
            }
        }

        public double MainLength
        {
            get
            {
                return Orthogonality ? YLength : XLength;
            }
        }
        public double LessLength
        {
            get
            {
                return Orthogonality ? XLength : YLength;
            }
        }


        /// <summary>
        /// 是否与分类方向正交
        /// </summary>
        public bool Orthogonality
        {
            get
            {
                return Math.Abs(XLength) < Math.Abs(YLength);
            }
        }
    }

    /// <summary>
    /// 由直线上一点和指定角度的参数方程
    /// x = t*cos(a)+x;
    /// y = t*sin(a)+y;
    /// </summary>
    internal class GLine
    {
        internal double a;
        private double angle
        {
            get 
            {
                while (a < 0)
                {
                    a += Math.PI;
                }
                while (a >= Math.PI)
                {
                    a -= Math.PI;
                }
                return a;
            }
        }
        internal IPoint p;
        internal GLine(double a, IPoint p)
        {
            this.a = a;
            this.p = p;
        }
        internal bool ParallelWith(GLine other)
        {
            return Math.Abs(angle - other.angle) < 0.1 * Math.PI / 180;
            //return (Math.Abs(s1 * c2 - c1 * s2) < 0.0001);           
        }

        internal IPoint InsertPoint(GLine other)
        {
            IPoint point = new PointClass();
            if(ParallelWith(other))
            {
                return null;
            }
            try
            {
                double s1 = Math.Sin(a);
                double c1 = Math.Cos(a);
                double s2 = Math.Sin(other.a);
                double c2 = Math.Cos(other.a);
                double dx = this.p.X - other.p.X;
                double dy = this.p.Y - other.p.Y;
                double t = (dx * s2 - dy * c2) / (s1 * c2 - c1 * s2);
                point.X = Math.Cos(a) * t + p.X;
                point.Y = Math.Sin(a) * t + p.Y;
                return point;
            }
            catch
            {
                return null;
            }
        }
    }
}

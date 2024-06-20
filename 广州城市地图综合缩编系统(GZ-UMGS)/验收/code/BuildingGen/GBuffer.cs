using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
namespace BuildingGen
{
    public static class GBuffer
    {        
        public static IPolygon BufferEx(IPolygon poly, double distance)
        {
            BufferConstructionClass bfc = new BufferConstructionClass();
            bfc.EndOption = esriBufferConstructionEndEnum.esriBufferFlat;
            //bfc.DensifyDeviation = distance;
            bfc.GenerateCurves = true;
            GeometryBagClass gb = new GeometryBagClass();
            GeometryBagClass gbout = new GeometryBagClass();
            object miss = Type.Missing;
            gb.AddGeometry(poly, ref miss, ref miss);
            bfc.ConstructBuffers(gb, distance, gbout);
            IPolygon buffer = gbout.get_Geometry(0) as IPolygon;
            ISegmentCollection sg = buffer as ISegmentCollection;
            PolygonClass result = new PolygonClass();
            for (int i = 0; i < sg.SegmentCount; i++)
            {
                ISegment seg = sg.get_Segment(i);
                if (!(seg is ILine))
                {
                    if (seg is ICircularArc)
                    {
                        double a = (seg as ICircularArc).CentralAngle;
                    }
                    LineClass line = new LineClass();
                    seg.QueryTangent(esriSegmentExtension.esriExtendAtFrom, 0, true, distance, line);
                    double a1 = line.Angle;
                    seg.QueryTangent(esriSegmentExtension.esriExtendAtFrom, seg.Length, true, distance, line);
                    double a2 = line.Angle;

                    PointClass p = new PointClass();
                    p.ConstructAngleIntersection(seg.FromPoint, a1, seg.ToPoint, a2);
                    line = new LineClass();
                    line.FromPoint = seg.FromPoint;
                    line.ToPoint = p;
                    ISegment s = line;
                    result.AddSegments(1, ref s);
                    line = new LineClass();
                    line.FromPoint = p;
                    line.ToPoint = seg.ToPoint;
                    s = line;
                    result.AddSegments(1, ref s);
                }
                else
                {
                    result.AddSegments(1, ref seg);
                }
            }

            return result;
        }

        /// <summary>
        /// 直角buffer
        /// </summary>
        /// <param name="poly">传入的多边形</param>
        /// <returns>之后的多边形</returns>
        public static IPolygon Buffer(IPolygon poly, double distance)
        {
            IGeometryCollection gc = poly as IGeometryCollection;

            if (gc == null || gc.GeometryCount <= 0)
            {
                return null;
            }
            IPolygon ep = null;
            IPolygon ip = null;

            object miss = Type.Missing;
            for (int i = 0; i < gc.GeometryCount; i++)
            {
                IRing r = gc.get_Geometry(i) as IRing;
                List<IRing> rings = buffer(r, distance);
                foreach (IRing ring in rings)
                {
                    PolygonClass p = new PolygonClass();
                    p.AddGeometry(ring, ref miss, ref miss);
                    p.Simplify();
                    p.SimplifyEx(true, true, true);
                    p.SpatialReference = poly.SpatialReference;
                    //p.SnapToSpatialReference();
                    if (ring.IsExterior)
                    {
                        if (ep == null)
                        {
                            ep = p;
                        }
                        else
                        {
                            ep = p.Union(ep) as IPolygon;
                        }
                    }
                    else
                    {
                        if (ip == null)
                        {
                            ip = p;
                        }
                        else
                        {
                            ip = p.Union(ip) as IPolygon;
                        }
                    }
                }
            }
            if (ep != null)
            {
                if (ip != null)
                {
                    ep = (ep as ITopologicalOperator).Difference(ip) as IPolygon;
                }
                //ep.Generalize(distance * 0.001);
                return ep;
            }
            return null;
        }
        static List<IRing> buffer(IRing ring, double distance)
        {
            //ring.Generalize(Math.Abs(distance * 0.001));
            
            ISegmentCollection sc = ring as ISegmentCollection;
            if (sc == null || sc.SegmentCount < 3)
                return null;
            GRingNode beginNode = null;
            GRingNode lastNode = null;
            ISegment lastSeg = sc.get_Segment(sc.SegmentCount - 1);
            object miss = Type.Missing;
            for (int i = 0; i < sc.SegmentCount; i++)
            {
                ISegment curSeg = sc.get_Segment(i);
                if (!(curSeg is ILine))
                {
                    LineClass curline = new LineClass();
                    curline.FromPoint = curSeg.FromPoint;
                    curline.ToPoint = curSeg.ToPoint;
                    curSeg = curline;
                }
                double a1 = (curSeg as ILine).Angle;
                double a2 = (lastSeg as ILine).Angle;

                double da = a1 - a2;

                double angle = Math.PI - (a1 - a2);
                double d = Math.Abs(distance / Math.Sin(angle / 2));
                LineClass line = new LineClass();

                if (distance > 0)
                {
                    line.ConstructAngleBisector(curSeg.ToPoint, lastSeg.ToPoint, lastSeg.FromPoint, d, false);
                }
                else
                {
                    line.ConstructAngleBisector(lastSeg.FromPoint, lastSeg.ToPoint, curSeg.ToPoint, d, false);
                }
                GRingNode currentNode = new GRingNode();
                //System.Diagnostics.Debug.WriteLine(currentNode.Index);
                currentNode.p = (line.ToPoint as ESRI.ArcGIS.esriSystem.IClone).Clone() as IPoint;
                currentNode.pre = lastNode;
                if (lastNode != null)
                {
                    lastNode.next = currentNode;
                }
                lastNode = currentNode;
                if (beginNode == null)
                {
                    beginNode = currentNode;
                }
                lastSeg = curSeg;
            }
            beginNode.pre = lastNode;
            lastNode.next = beginNode;
            List<GRingNode> nodes = SimplifyRing(beginNode);
            //List<GRingNode> nodes = new List<GRingNode>();
            //nodes.Add(beginNode);
            List<IRing> rings = new List<IRing>();
            foreach (GRingNode node in nodes)
            {
                GRingNode currentNode = node;
                RingClass r = new RingClass();
                do
                {
                    IPoint outPoint = new PointClass();
                    double oad = 0.0;
                    double ofd = 0.0;
                    bool rightsite = false;
                    ring.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, currentNode.p, false, outPoint, ref oad, ref ofd, ref rightsite);
                    if (ofd > Math.Abs(distance) * 0.9)
                    {
                        r.AddPoint(currentNode.p, ref miss, ref miss);
                    }

                    currentNode = currentNode.next;
                }
                while (currentNode != node);
                r.Close();
                if (r.IsExterior == ring.IsExterior)
                {
                    //if(Math.Abs(r.Area) > Math.Abs(distance * 0.1))
                    rings.Add(r);
                }

            }
            return rings;
        }

        static List<GRingNode> SimplifyRing(GRingNode beginNode)
        {
            List<GRingNode> nodes = new List<GRingNode>();
            GRingNode current = beginNode.next;
            #region 自相交
            /*
             * 1.从起始点（beginNode）往后推（current）
             * 2.从（current）往前推，如果相交，在交点（intersectPoint）处添加两个点
             * 
             */
            while (current != beginNode)
            {
                GRingNode compareNode = current.pre;
                System.Diagnostics.Debug.Write("\n" + current.Index + "<->");
                while (compareNode != beginNode.pre)
                {
                    //System.Diagnostics.Debug.Write(compareNode.Index + "<->");
                    if (compareNode == current.pre)
                    {
                        //判断平行往回走的情况
                        //TODO 会不会删掉一个起始点？
                        if (compareNode.ReverseWith(current))
                        {
                            compareNode.next = current.next;
                            current.next.pre = compareNode;
                            current = compareNode;
                        }
                        compareNode = compareNode.pre;
                        continue;
                    }
                    //TODO 判断平行相交的情况
                    IPoint intersectPoint = current.IntersectEx(compareNode) as IPoint;
                    if (intersectPoint != null)
                    {
                        GRingNode n1 = new GRingNode();
                        n1.p = intersectPoint;
                        n1.next = compareNode.next;
                        n1.pre = current;
                        compareNode.next.pre = n1;

                        GRingNode n2 = new GRingNode();
                        n2.p = intersectPoint;
                        n2.pre = compareNode;
                        n2.next = current.next;
                        current.next.pre = n2;

                        current.next = n1;
                        compareNode.next = n2;
                        nodes.Add(n1);
                        current = n2;
                    }
                    compareNode = compareNode.pre;
                }
                current = current.next;
            }
            #endregion
            nodes.Add(beginNode);

            return nodes;
        }

        private static IPolygon AddPoints(IPolygon polygon, double minDistance)
        {
            IPolygon poly = (polygon as IClone).Clone() as IPolygon;
            IGeometryCollection gc = poly as IGeometryCollection;

            for (int i = 0; i < gc.GeometryCount; i++)
            {
                IRing ring = gc.get_Geometry(i) as IRing;
                IPointCollection pc = ring as IPointCollection;

                IPoint currentPoint = new PointClass();
                for (int j = 0; j < pc.PointCount; j++)
                {
                    IPoint p1 = pc.get_Point(j);
                    double x = p1.X;
                    double y = p1.Y;
                    if (j != 0)
                    {
                        int addCount = 0;

                        double distane = Math.Sqrt((x - currentPoint.X) * (x - currentPoint.X) + (y - currentPoint.Y) * (y - currentPoint.Y));
                        addCount = Convert.ToInt32(distane / minDistance);
                        if (addCount < 2)
                        {
                            addCount = 2;
                        }
                        for (int k = 1; k <= addCount; k++)
                        {
                            IPoint p = new PointClass();
                            p.X = (x * k + currentPoint.X * (addCount - k)) / addCount;
                            p.Y = (y * k + currentPoint.Y * (addCount - k)) / addCount;
                            pc.InsertPoints(j + k, 1, ref p);
                        }
                        j += addCount;

                    }
                    currentPoint.X = x;
                    currentPoint.Y = y;
                }
            }

            return poly;
        }


        internal class GRingNode
        {
            private static UInt64 NodeIndex = 0;
            internal GRingNode()
            {
                Index = NodeIndex;
                NodeIndex++;
            }
            internal UInt64 Index;
            internal IPoint p;
            internal GRingNode next;
            internal GRingNode pre;
            internal double Angle
            {
                get
                {
                    return Line.Angle;
                }
            }
            ILine line;
            internal ILine Line
            {
                get
                {
                    line = new LineClass();
                    line.FromPoint = p;
                    line.ToPoint = next.p;

                    return line;
                }
            }

            internal bool ParallelWith(GRingNode other)
            {
                double a1 = this.Angle;
                double a2 = other.Angle;
                return Math.Abs(a1 - a2) < 0.001;
            }

            internal bool ReverseWith(GRingNode other)
            { 
                double a1 = this.Angle;
                double a2 = other.Angle;
                return Math.Abs(Math.Abs(a1 - a2) - Math.PI) < 0.001;
            }
            /// <summary>
            /// 判断是否与另外一条线段相交并返回交点
            /// 重叠的话返回一条线段
            /// </summary>
            /// <param name="other">另外一条线段</param>
            /// <returns>如果相交返回交点；
            /// 如果重叠返回一条线段，线段起点离this.p较近；
            /// 如果不相交返回空</returns>
            internal IGeometry IntersectEx(GRingNode other)
            {
                try
                {
                    Vector3DClass v1 = new Vector3DClass();
                    v1.ConstructDifference(this.next.p, this.p);
                    v1.ZComponent = 0;
                    Vector3DClass v2 = new Vector3DClass();
                    v2.ConstructDifference(other.next.p, other.p);
                    v2.ZComponent = 0;
                    IVector v3 = v1.CrossProduct(v2);
                    double area = v3.Magnitude;
                    if (area < 0.01)
                    {
                        return null;
                    }

                    double dis = (Line as IProximityOperator).ReturnDistance(other.Line);
                    if (dis > 0)
                    {
                        return null;
                    }
                    PointClass point = new PointClass();

                    point.ConstructAngleIntersection(this.p, this.Angle, other.p, other.Angle);
                    if (point.IsEmpty)
                    {
                        return null;
                    }
                    return point;
                }
                catch
                {
                    return null;
                }
            }
            internal IPoint Intersect(GRingNode other)
            {
                object miss = Type.Missing;
                PolylineClass pl1 = new PolylineClass();
                pl1.AddPoint(this.p, ref miss, ref miss);
                pl1.AddPoint(this.next.p, ref miss, ref miss);
                //pl1.AddSegment(this.line as ISegment, ref miss, ref miss);
                PolylineClass pl2 = new PolylineClass();
                pl2.AddPoint(other.p, ref miss, ref miss);
                pl2.AddPoint(other.next.p, ref miss, ref miss);
                if (pl1.Disjoint(pl2))
                    return null;
                IGeometry geo = pl1.Intersect(pl2, esriGeometryDimension.esriGeometry0Dimension);
                if (geo is IPoint)
                {
                    return geo as IPoint;
                }
                else if (geo is IMultipoint)
                {
                    IPointCollection pc = geo as IPointCollection;
                    double minDistance = double.MaxValue;
                    IPoint rp = null;
                    for (int i = 0; i < pc.PointCount; i++)
                    {
                        IProximityOperator ip = pc.get_Point(i) as IProximityOperator;
                        double dist = ip.ReturnDistance(this.p);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            rp = ip as IPoint;
                        }
                    }
                    return rp;
                }
                else
                {
                    return null;
                }

            }
        }
    }
}

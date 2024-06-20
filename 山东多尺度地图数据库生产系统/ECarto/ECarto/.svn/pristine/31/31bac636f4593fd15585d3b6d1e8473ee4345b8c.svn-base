using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

using System.Diagnostics;

namespace SMGI.Plugin.GeneralEdit.CenterLineCut
{
    public class CenterLine : List<CenterLinePath>
    {
        public CenterLine(IPolygon polygon, ITin tin)
        {
            this.tin = tin;
            this.polygon = polygon;
            Other = null;
        }

        public IPolyline Line
        {
            get
            {
                PolylineClass resultPolyline = new PolylineClass();
                foreach (CenterLinePath info in this)
                {
                    resultPolyline.SpatialReference = info.Line.SpatialReference;
                    for (int i = 0; i < (info.Line as IGeometryCollection).GeometryCount; i++)
                    {
                        IGeometry geo = (info.Line as IGeometryCollection).get_Geometry(i);
                        resultPolyline.AddGeometries(1, ref geo);
                    }
                }
                return resultPolyline;
            }
        }
        public ITin Tin
        {
            get { return tin; }
        }
        public IPolygon Polygon
        {
            get { return polygon; }
        }
        public object Other;
        private IPolygon polygon;
        private ITin tin;
    }

    public class CenterLinePath
    {
        public CenterLinePath(IPolyline line, double width)
        {
            Line = line;
            Width = width;
        }
        public IPolyline Line;
        public double Width;
    }

    public class CenterLineFactory
    {
        internal class PolylineInfo
        {
            internal PolylineInfo()
            {
                Triangles = new List<ITinTriangle>();
                Edges = new List<ITinEdge>();
                StartLength = 0;
                FromGroup = -1;
                ToGroup = -2;
                Area = 0;
            }
            public ITinAdvanced2 Tin;
            public IPolyline Polyline;
            public double StartLength;
            public List<ITinTriangle> Triangles;
            public List<ITinEdge> Edges;
            public double Area;
            public int FromGroup;
            public int ToGroup;
        }

        public CenterLineFactory()
        {
            minLineLength = 80;
            minDistance = 50;
        }

        private double minLineLength;
        private double minDistance;

        public CenterLine Create2(IPolygon polygon)
        {
            /* 生成中轴线
            * 1 加密多边形
            * 2 生成三角网
            * 
            * 3.0 处理两种特殊情况（只有3条边或者4条边，1个多边形或者两个多边形）
            * 3.1 计算tag值
            * 
            * do
            * {
            * 3.2 获取骨架线
            * 3.3 剔除小毛刺
            * }
            * while(没小毛刺了)
            * 4 调整1类三角形端点选取，化简，并计算每一段的面积 
            * 5 合并交叉口
            */
            //0 计算参数
            double w = ((polygon as IArea).Area / polygon.Length) * 2.5;
            minDistance = w * 0.5;
            //去毛刺的长度
            minLineLength = w * 1.5;

            //1 加密多边形
            IPolygon poly = AddPoints2(polygon);

            //2 生成三角网
            ITin tin = GetTin(poly);
            ITinEdit tinEdit = tin as ITinEdit;

            //3.0 处理两种特殊情况
            if ((poly as ISegmentCollection).SegmentCount == 3)
            {
                return CreateWhenJustOneTriange(tin, polygon);
            }
            if ((poly as ISegmentCollection).SegmentCount == 4)
            {
                return CreateWhenJustTwoTriange(tin, polygon);
            }

            //3.1 计算tag值
            SetTriangleTag(tin as ITinAdvanced2);
            SetEdgeTag(tin as ITinAdvanced2);


            int count = 0;
            List<PolylineInfo> lines = null;
            do
            {
                //tinEdit.DeleteNodesOutsideDataArea();

                //3.2 获取骨架线
                lines = GetLines(tin as ITinAdvanced2);

                //3.3 剔除小毛刺
                count = CheckLines(tin as ITinEdit, lines);

            }
            while (count != 0);

            //4 计算每一段的面积,调整1类三角形端点选取，化简
            foreach (PolylineInfo info in lines)
            {
                info.Area = 0;
                foreach (ITinTriangle tri in info.Triangles)
                {
                    if (tri.TagValue == 3)
                    {
                        info.Area += tri.Area / 3;
                    }
                    info.Area += tri.Area;
                }

                //处理线端头延伸问题
                AjustPointOfHead(info, polygon);

                //info.Polyline.Generalize(w / 20);
                info.Polyline.SpatialReference = polygon.SpatialReference;
                /*
                if (info.Triangles[0].TagValue == 3 && info.Triangles[info.Triangles.Count - 1].TagValue == 1)
                {
                    bool s = true;
                    (info.Polyline as IConstructCurve).ConstructExtended((info.Polyline as IClone).Clone() as IPolyline , polygon, 8, ref s);
                    info.Polyline.SpatialReference = polygon.SpatialReference;
                    //info.Polyline = (polygon as ITopologicalOperator).Intersect(info.Polyline, esriGeometryDimension.esriGeometry2Dimension) as IPolyline;
                }
                 */
                info.Polyline.SnapToSpatialReference();

            }

            //5 合并交叉口
            MergePoints(lines, polygon.Envelope);

            CenterLine centerLine = new CenterLine(polygon, tin);
            foreach (PolylineInfo info in lines)
            {
                centerLine.Add(new CenterLinePath(info.Polyline, info.Area / info.Polyline.Length));
            }

            return centerLine;
        }

        public IPolyline Create(IPolygon polygon)
        {
            /* 生成中轴线
             * 1 加密多边形
             * 2 生成三角网
             * 3.1 计算tag值
             * 
             * do
             * {
             * 3.2 获取骨架线
             * 3.3 剔除小毛刺
             * }
             * while(没小毛刺了)
             * 4 合并交叉口
             */
            //0 计算参数
            double w = ((polygon as IArea).Area / polygon.Length) * 2.5;
            minDistance = w * 0.5;
            minLineLength = w * 2;

            //1 加密多边形
            IPolygon poly = AddPoints2(polygon);


            //2 生成三角网
            ITin tin = GetTin(poly);
            ITinEdit tinEdit = tin as ITinEdit;

            //3.1 计算tag值
            SetTriangleTag(tin as ITinAdvanced2);
            SetEdgeTag(tin as ITinAdvanced2);


            int count = 0;
            List<PolylineInfo> lines = null;
            do
            {
                //tinEdit.DeleteNodesOutsideDataArea();

                //3.2 获取骨架线
                lines = GetLines(tin as ITinAdvanced2);

                //3.3 剔除小毛刺
                count = CheckLines(tin as ITinEdit, lines);

            }
            while (count != 0);

            //4 化简并计算每一段的面积
            foreach (PolylineInfo info in lines)
            {
                info.Polyline.Generalize(0.1);
                info.Polyline.SpatialReference = polygon.SpatialReference;
                info.Area = 0;
                foreach (ITinTriangle tri in info.Triangles)
                {
                    info.Area += tri.Area;
                }
            }

            //5 合并交叉口
            MergePoints(lines, polygon.Envelope);

            //6 输出中轴线
            PolylineClass resultPolyline = new PolylineClass();
            foreach (PolylineInfo info in lines)
            {
                resultPolyline.SpatialReference = (tin as IGeoDataset).SpatialReference;
                for (int i = 0; i < (info.Polyline as IGeometryCollection).GeometryCount; i++)
                {
                    IGeometry geo = (info.Polyline as IGeometryCollection).get_Geometry(i);
                    resultPolyline.AddGeometries(1, ref geo);
                }
            }

            resultPolyline.SpatialReference = polygon.SpatialReference;

            return resultPolyline;
        }


        /// <summary>
        /// 只有一个三角形，返回短边中点与对应顶点的连线
        /// </summary>
        /// <param name="tin">三角网</param>
        /// <param name="polygon">原多边形</param>
        /// <returns>中轴线</returns>
        public CenterLine CreateWhenJustOneTriange(ITin tin,IPolygon polygon) {
            CenterLine l = new CenterLine(polygon, tin);
            ITinAdvanced2 t = tin as ITinAdvanced2;
            for (int i = 1; i <= t.TriangleCount; i++)
            {
                var tri = t.GetTriangle(i);
                if (tri.IsInsideDataArea) {
                    double minl = double.MaxValue;
                    ITinEdge e = null;
                    for (int j = 0; j < 3; j++)
                    {
                        var ee = tri.get_Edge(j);
                        if (ee.Length < minl) {
                            e = ee;
                            minl = ee.Length;
                        }
                    }
                    IPoint p1 = new PointClass { 
                        X = (e.FromNode.X + e.ToNode.X) * 0.5,
                        Y = (e.FromNode.Y + e.ToNode.Y) * 0.5
                    };
                    IPoint p2 = new PointClass
                    {
                        X= e.GetNextInTriangle().ToNode.X,
                        Y = e.GetNextInTriangle().ToNode.Y
                    };

                    var line = new PolylineClass();
                    line.AddPoints(1,ref p1);
                    line.AddPoints(1,ref p2);

                    //var cp = new CenterLinePath(line, (polygon as IArea).Area / line.Length);
                    
                    l.Add(new CenterLinePath(line,(polygon as IArea).Area / line.Length));
                    return l;
                }
            }
            return l;
        }

        /// <summary>
        /// 只有两个三角形，返回短边中点与对边中点连线
        /// </summary>
        /// <param name="tin">三角网</param>
        /// <param name="polygon">原多边形</param>
        /// <returns>中轴线</returns>
        public CenterLine CreateWhenJustTwoTriange(ITin tin, IPolygon polygon)
        {
            CenterLine l = new CenterLine(polygon, tin);
            ITinAdvanced2 t = tin as ITinAdvanced2;
            for (int i = 1; i <= t.EdgeCount; i++)
            {
                var edge = t.GetEdge(i);
                if (edge.IsInsideDataArea && edge.GetNeighbor().IsInsideDataArea)
                {
                    var line = new PolylineClass();

                    ITinEdge[] edges = new ITinEdge[4];
                    edges[0] = edge.GetNextInTriangle();
                    edges[1] = edge.GetPreviousInTriangle();
                    edges[2] = edge.GetNeighbor().GetNextInTriangle();
                    edges[3] = edge.GetNeighbor().GetPreviousInTriangle();

                    int minIndex = 0;
                    double minLength = double.MaxValue;

                    for (int j = 0; j < edges.Length; j++)
                    {
                        var len = edges[j].Length;
                        if (len < minLength)
                        {
                            minLength = len;
                            minIndex = j;
                        }
                    }

                    //第一点,最短边中点
                    {
                        var ee = edges[minIndex];

                        IPoint pt = new PointClass
                        {
                            X = (ee.FromNode.X + ee.ToNode.X) * 0.5,
                            Y = (ee.FromNode.Y + ee.ToNode.Y) * 0.5
                        };
                        line.AddPoints(1, ref pt);
                    }

                    minIndex += 2;
                    if (minIndex >= 4) {
                        minIndex -= 4;
                    }

                    //第二点，对边中点
                    {
                        var ee = edges[minIndex];

                        IPoint pt = new PointClass
                        {
                            X = (ee.FromNode.X + ee.ToNode.X) * 0.5,
                            Y = (ee.FromNode.Y + ee.ToNode.Y) * 0.5
                        };
                        line.AddPoints(1, ref pt);
                    }

                    l.Add(new CenterLinePath(line, (polygon as IArea).Area / line.Length));
                    return l;
                }
            }
            return l;
        }


        /// <summary>
        /// 调整端头上一类三角形的终点位置
        /// </summary>
        /// <param name="info"></param>
        private void AjustPointOfHead(PolylineInfo info,IPolygon polygon)
        {
            if (info.Triangles.Count <= 2)
                return;
            
            
            //这条线的宽度
            double w = info.Area / info.Polyline.Length;
            #region 处理两端
            for(int i = 0;i< 2;i++)
            {
                info.Triangles.Reverse();
                info.Polyline.ReverseOrientation();

                var tri = info.Triangles[0];
                if (tri.TagValue != 1)
                {
                    continue;
                }

                var len = (info.Polyline as ISegmentCollection).get_Segment(0).Length;
                var edge = GetButtomEdgeOfTriangle(tri);
                /// 说明是去毛刺形成的1类三角形,用延伸法
                
                if (edge.GetNextInTriangle().LeftTriangle.IsInsideDataArea)
                {
                    continue;
                    var p1 = (info.Polyline as IClone).Clone() as IPolyline;
                    var l = w * 4.0 ;
                    if (l < len * 1.05) {
                        l = len * 1.05;
                    }

                    bool h;
                    int p,s;
                    p1.SplitAtDistance(l , false, true, out h, out p, out s);
                    if (h)
                    {
                        (p1 as IGeometryCollection).RemoveGeometries(0, 1);
                    }

                    var polyline = new PolylineClass(); 
                    bool r = false;
                    polyline.ConstructExtended(p1, polygon, (int)esriCurveExtension.esriNoExtendAtTo, ref r);
                    if (r) {
                        polyline.SpatialReference = info.Polyline.SpatialReference;
                        info.Polyline = polyline;
                    }
                    continue;
                }

                //纯一类三角形，调整一类三角形
                var pts = CalPoints(tri);

                IPoint pt1 = new PointClass();

                var seg1ength = w;
                if(len > seg1ength){
                    seg1ength = len;
                }

                info.Polyline.QueryPoint(esriSegmentExtension.esriNoExtension, seg1ength , false, pt1);

                IPoint pt2 = new PointClass();
                info.Polyline.QueryPoint(esriSegmentExtension.esriNoExtension, seg1ength * 2, false, pt2);

                double x2 = pt2.X - pt1.X;
                double y2 = pt2.Y - pt1.Y;
                double a2 = x2 * x2 + y2 * y2;

                double minCos = double.MaxValue;
                IPoint bestPoint = null;
                foreach (var pt in pts)
                {
                    double x1 = pt.X - pt1.X;
                    double y1 = pt.Y - pt1.Y;
                    double b2 = x1 * x1 + y1 * y1;

                    double x3 = pt.X - pt2.X;
                    double y3 = pt.Y - pt2.Y;
                    double c2 = x3 * x3 + y3 * y3;

                    //余弦定律 cos(C) = (a^2 + b^2 - c^2)/(2ab)
                    double cos = (a2 + b2 - c2) / (2.0 * Math.Sqrt(a2 * b2));
                    if (cos < minCos) {
                        minCos = cos;
                        bestPoint = pt;
                    }
                }

                info.Polyline.FromPoint = bestPoint;
            } 
            #endregion

        }
        /// <summary>
        /// 获取1类三角形的底边
        /// </summary>
        /// <param name="tri">一类三角形</param>
        /// <returns>底边</returns>
        ITinEdge GetButtomEdgeOfTriangle(ITinTriangle tri)
        {

            for (int i = 0; i < 3; i++)
            {
                var e = tri.get_Edge(i);
                if (e.TagValue == 2)
                {
                    return e;
                }
            }
            return null;
        }

        /// <summary>
        /// 计算候选点
        /// </summary>
        /// <param name="tri">三角形（一类）</param>
        /// <returns>三个候选点：顶点，两条腰的中点</returns>
        IPoint[] CalPoints(ITinTriangle tri) {
            ITinEdge edge = GetButtomEdgeOfTriangle(tri);

            if (edge == null) {
                return null;
            }

            IPoint[] pts = new IPoint[3];

            var ne = edge.GetNextInTriangle();
            pts[0] = new PointClass { 
                X = ne.ToNode.X,
                Y = ne.ToNode.Y
            };

            pts[1] = new PointClass
            {
                X = (ne.FromNode.X + ne.ToNode.X) / 2.0,
                Y = (ne.FromNode.Y + ne.ToNode.Y) / 2.0
            };
            ne = edge.GetPreviousInTriangle();
            pts[2] = new PointClass
            {
                X = (ne.FromNode.X + ne.ToNode.X) / 2.0,
                Y = (ne.FromNode.Y + ne.ToNode.Y) / 2.0
            };
            return pts;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tin"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        private int CheckLines(ITinEdit tin, List<PolylineInfo> lines)
        {
            int count = 0;
            if (lines.Count == 3)
            {
                bool exception = true;
                foreach (PolylineInfo info in lines)
                {
                    if (info.Triangles[0].TagValue != 3
                        || info.Triangles[info.Triangles.Count - 1].TagValue != 1
                        || info.Polyline.Length - info.StartLength > minLineLength)
                    {
                        exception = false;
                        break;
                    }
                }
                if (exception)
                {
                    return 0;
                }
            }
            List<ITinTriangle> startTri = new List<ITinTriangle>();
            foreach (PolylineInfo info in lines)
            {
                //起点三角形不为3类、终点三角形不为1类，长度大于阈值
                if (info.Triangles[0].TagValue != 3
                    || info.Triangles[info.Triangles.Count - 1].TagValue != 1
                    || info.Polyline.Length - info.StartLength > minLineLength)
                {
                    foreach (ITinEdge edge in info.Edges)
                    {
                        tin.SetEdgeTagValue(edge.Index, 2);
                    }
                    continue;
                }


                int index0 = info.Triangles[0].Index;
                foreach (ITinTriangle triangle in info.Triangles)
                {
                    int index = triangle.Index;
                    if (index == index0)
                    {
                        startTri.Add(triangle);
                        continue;
                    }

                    tin.SetTriangleTagValue(index, 0);
                }

                tin.SetEdgeTagValue(info.Edges[0].Index, 1);
                count++;
            }

            foreach (ITinTriangle triangle in startTri)
            {
                tin.SetTriangleTagValue(triangle.Index, triangle.TagValue - 1);
            }

            return count;
        }

        /// <summary>
        /// 根据多边形生成tin，生成类别为esriTinHardClip
        /// </summary>
        /// <param name="polygon">需要生成的多边形</param>
        /// <returns>返回生成的tin</returns>
        private ITin GetTin(IPolygon polygon)
        {
            TinClass tin = new TinClass();

            EnvelopeClass tinExtent = new EnvelopeClass();
            tinExtent.SpatialReference = polygon.SpatialReference;
            tinExtent.Union(polygon.Envelope);
            tin.InitNew(tinExtent);

            tin.StartInMemoryEditing();

            object z = 0;
            tin.AddShape(polygon, esriTinSurfaceType.esriTinHardClip, 0, ref z);

            return tin as ITin;
        }

        /// <summary>
        /// 更加现有多边形生成加密多边形（现有多边形不改变）
        /// </summary>
        /// <param name="poly">需要加密的多边形</param>
        /// <returns>加密过后的多边形</returns>
        private IPolygon AddPoints(IPolygon polygon)
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

        private IPolygon AddPoints2(IPolygon polygon)
        {
            var pp = (polygon as IClone).Clone() as IPolygon;
            pp.Generalize(minDistance / 30.0);
            //(pp as IPolycurve).Densify(minDistance, 0);
            return pp;
            PolygonClass poly = new PolygonClass();
            poly.SpatialReference = polygon.SpatialReference;
            IGeometryCollection gc = polygon as IGeometryCollection;

            for (int i = 0; i < gc.GeometryCount; i++)
            {
                RingClass r = new RingClass();
                IRing ring = gc.get_Geometry(i) as IRing;
                IPointCollection pc = ring as IPointCollection;

                IPoint currentPoint = new PointClass();
                for (int j = 0; j < pc.PointCount; j++)
                {
                    IPoint p1 = pc.get_Point(j);
                    double x = p1.X;
                    double y = p1.Y;

                    if (j == 0)
                    {
                        currentPoint.X = x;
                        currentPoint.Y = y;
                        r.AddPoints(1, ref currentPoint);
                        continue;
                    }

                    double distane = Math.Sqrt((x - currentPoint.X) * (x - currentPoint.X) + (y - currentPoint.Y) * (y - currentPoint.Y));
                    /*
                    double rate = minDistance / distane;
                    if (rate >= 0.5)
                    {
                        IPoint p = new PointClass();
                        p.X = (x + currentPoint.X) / 2;
                        p.Y = (y + currentPoint.Y) / 2;
                        r.AddPoints(1, ref p);

                        p.X = x;
                        p.Y = y;
                        r.AddPoints(1, ref p);
                    }
                    else 
                    {
                        IPoint p = new PointClass();
                        p.X = (x * rate + currentPoint.X * (1 - rate));
                        p.Y = (y * rate + currentPoint.Y * (1 - rate));
                        r.AddPoints(1, ref p);

                        p.X = (x + currentPoint.X) / 2;
                        p.Y = (y + currentPoint.Y) / 2;
                        r.AddPoints(1, ref p);

                        p.X = (x * (1 - rate) + currentPoint.X * rate);
                        p.Y = (y * (1 - rate) + currentPoint.Y * rate);
                        r.AddPoints(1, ref p);

                        p.X = x;
                        p.Y = y;
                        r.AddPoints(1, ref p);
                    }
                     * */

                    int addCount = 0;
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
                        r.AddPoints(1, ref p);
                    }

                    currentPoint.X = x;
                    currentPoint.Y = y;
                }
                IGeometry g = r as IGeometry;
                poly.AddGeometries(1, ref g);
            }
            return poly;
        }


        /// <summary>
        /// 设置三角形的tag值
        /// </summary>
        /// <param name="tin"></param>
        private void SetTriangleTag(ITinAdvanced2 tin)
        {
            ITinEdit tinEdit = tin as ITinEdit;
            for (int i = 1; i <= tin.TriangleCount; i++)
            {
                ITinTriangle triangle = tin.GetTriangle(i);
                if (!triangle.IsInsideDataArea)
                {
                    tinEdit.SetTriangleTagValue(i, 0);
                    continue;
                }

                int t1, t2, t3;
                triangle.QueryAdjacentTriangleIndices(out t1, out t2, out t3);

                int count = 0;
                if (t1 != 0 && tin.GetTriangle(t1).IsInsideDataArea)
                    count++;
                if (t2 != 0 && tin.GetTriangle(t2).IsInsideDataArea)
                    count++;
                if (t3 != 0 && tin.GetTriangle(t3).IsInsideDataArea)
                    count++;

                tinEdit.SetTriangleTagValue(i, count);
            }


        }
        /// <summary>
        /// 设置边的tag值
        /// </summary>
        /// <param name="tin"></param>
        private void SetEdgeTag(ITinAdvanced2 tin)
        {
            ITinEdit tinEdit = tin as ITinEdit;
            for (int i = 1; i <= tin.EdgeCount; i++)
            {
                ITinEdge edge = tin.GetEdge(i);
                if (!edge.IsInsideDataArea)
                {
                    tinEdit.SetEdgeTagValue(i, 0);
                    continue;
                }
                int count = 0;
                if (edge.LeftTriangle.TagValue != 0)
                    count++;
                if (edge.RightTriangle.TagValue != 0)
                    count++;
                tinEdit.SetEdgeTagValue(i, count);
            }
        }

        /// <summary>
        /// 根据tin得到骨架线
        /// </summary>
        /// <param name="tin">传入的tin</param>
        /// <returns>返回骨架线集合</returns>
        private List<PolylineInfo> GetLines(ITinAdvanced2 tin)
        {
            List<PolylineInfo> lines = new List<PolylineInfo>();
            int Value3Count = 0;
            int tag1Index = 0;
            #region 如果存在任意一个3类三角形
            for (int i = 1; i <= tin.TriangleCount; i++)
            {
                ITinTriangle triangle = tin.GetTriangle(i);
                ITinEdit tinEdit = tin as ITinEdit;
                if (triangle.TagValue != 3)
                {
                    if (triangle.TagValue == 1)
                    {
                        tag1Index = triangle.Index;
                    }
                    continue;
                }
                Value3Count++;

                IPoint point = new PointClass();
                IPoint beginPoint = getCenterPoint(triangle);

                #region 分三个方向分别添加
                for (int j = 0; j < 3; j++)
                {
                    ITinEdge edge = triangle.get_Edge(j);
                    if (edge.TagValue != 2)
                    {
                        continue;
                    }

                    PolylineInfo polylineInfo = new PolylineInfo();
                    polylineInfo.Tin = tin;
                    polylineInfo.Triangles.Add(triangle);

                    PolylineClass poly = new PolylineClass();
                    poly.AddPoints(1, ref beginPoint);

                    point.X = (edge.FromNode.X + edge.ToNode.X) / 2;
                    point.Y = (edge.FromNode.Y + edge.ToNode.Y) / 2;

                    poly.AddPoints(1, ref point);

                    polylineInfo.StartLength = poly.Length;
                    ITinTriangle triangle2 = null;
                    if (edge.LeftTriangle.Index == triangle.Index)
                    {
                        triangle2 = edge.RightTriangle;
                    }
                    else
                    {
                        triangle2 = edge.LeftTriangle;
                    }

                    //针对3种情况分别添加
                    do
                    {
                        polylineInfo.Triangles.Add(triangle2);
                        polylineInfo.Edges.Add(edge);
                        IPoint nextPoint = nextTriangle(ref triangle2, ref edge);
                        if (nextPoint != null)
                        {
                            poly.AddPoints(1, ref nextPoint);
                        }

                    } while (triangle2 != null);

                    polylineInfo.Polyline = poly;
                    lines.Add(polylineInfo);
                }
                #endregion
            }
            #endregion

            #region 如果不存在3类三角形但存在1类三角形

            if (Value3Count == 0 && tag1Index != 0)
            {
                ITinTriangle triangle = tin.GetTriangle(tag1Index);
                ITinEdge edge = null;
                for (int j = 0; j < 3; j++)
                {
                    if (triangle.get_Edge(j).TagValue == 2)
                    {
                        edge = triangle.get_Edge(j);
                    }
                }
                ITinNode node = getOtherNode(triangle, edge);
                PolylineInfo polylineInfo = new PolylineInfo();
                polylineInfo.Tin = tin;
                polylineInfo.Triangles.Add(triangle);

                PolylineClass poly = new PolylineClass();

                IPoint point = new PointClass();
                node.QueryAsPoint(point);
                poly.AddPoints(1, ref point);

                point.X = (edge.FromNode.X + edge.ToNode.X) / 2;
                point.Y = (edge.FromNode.Y + edge.ToNode.Y) / 2;

                poly.AddPoints(1, ref point);

                ITinTriangle triangle2 = null;
                if (edge.LeftTriangle.Index == triangle.Index)
                {
                    triangle2 = edge.RightTriangle;
                }
                else
                {
                    triangle2 = edge.LeftTriangle;
                }

                do
                {
                    polylineInfo.Triangles.Add(triangle2);
                    polylineInfo.Edges.Add(edge);
                    point = nextTriangle(ref triangle2, ref edge);
                    poly.AddPoints(1, ref point);
                }
                while (triangle2 != null);

                polylineInfo.Polyline = poly;
                lines.Add(polylineInfo);
            }
            #endregion
            #region 如果只有2类三角形
            if (Value3Count == 0 && tag1Index == 0)
            {
                ITinTriangle triangle = null;
                for (int i = 1; i <= tin.TriangleCount; i++)
                {
                    triangle = tin.GetTriangle(i);
                    if (triangle.TagValue == 2)
                    {
                        break;
                    }
                }
                int startIndex = triangle.Index;
                ITinEdge edge = null;
                for (int j = 0; j < 3; j++)
                {
                    if (triangle.get_Edge(j).TagValue == 2)
                    {

                        edge = triangle.get_Edge(j);
                        break;
                    }
                }

                PolylineInfo polylineInfo = new PolylineInfo();
                polylineInfo.Tin = tin;

                PolylineClass poly = new PolylineClass();

                IPoint point = new PointClass();
                point.X = (edge.FromNode.X + edge.ToNode.X) / 2;
                point.Y = (edge.FromNode.Y + edge.ToNode.Y) / 2;
                poly.AddPoints(1, ref point);

                do
                {
                    polylineInfo.Triangles.Add(triangle);
                    polylineInfo.Edges.Add(edge);
                    point = nextTriangle(ref triangle, ref edge);
                    poly.AddPoints(1, ref point);
                }
                while (triangle != null && triangle.Index != startIndex && edge.TagValue == 2);

                polylineInfo.Polyline = poly;
                lines.Add(polylineInfo);

            }
            #endregion
            return lines;
        }

        /// <summary>
        /// 获取当前三角形的点，并将边和三角形都指向下一个
        /// </summary>
        /// <param name="triangle">经过的三角形，完成后被改为下一个三角形，没有下一个则为空</param>
        /// <param name="edge">进入该三角形的边，完成后被改为三角形出来的边，没有下一个则为空</param>
        /// <returns>返回下一个点</returns>
        private IPoint nextTriangle(ref ITinTriangle triangle, ref ITinEdge edge)
        {
            ITinAdvanced2 tin = triangle.TheTin as ITinAdvanced2;
            ITinEdit tinEdit = tin as ITinEdit;

            tinEdit.SetEdgeTagValue(edge.Index, -2);

            IPoint point = new PointClass();

            if (triangle.TagValue == 1)
            {
                ITinNode node = getOtherNode(triangle, edge);
                node.QueryAsPoint(point);
                triangle = null;
            }
            else if (triangle.TagValue == 3)
            {
                point = getCenterPoint(triangle);
                triangle = null;
            }
            else if (triangle.TagValue == 2)
            {
                ITinNode node = getOtherNode(triangle, edge);

                for (int k = 0; k < 3; k++)
                {
                    ITinEdge edge2 = triangle.get_Edge(k);
                    if ((edge2.FromNode.Index == node.Index || edge2.ToNode.Index == node.Index) && edge2.TagValue == 2)
                    {
                        edge = edge2;
                    }
                }
                point.X = (edge.FromNode.X + edge.ToNode.X) / 2;
                point.Y = (edge.FromNode.Y + edge.ToNode.Y) / 2;
                if (edge.LeftTriangle.Index == triangle.Index)
                {
                    triangle = edge.RightTriangle;
                }
                else
                {
                    triangle = edge.LeftTriangle;
                }
            }
            else
            {
                triangle = null;
            }
            return point;
        }

        /// <summary>
        /// 获取三角形中与某条边对着的顶点
        /// </summary>
        /// <param name="triangle">三角形</param>
        /// <param name="edge">三角形的一边</param>
        /// <returns>对着edge的那个顶点</returns>
        private ITinNode getOtherNode(ITinTriangle triangle, ITinEdge edge)
        {
            if (triangle.TheTin != edge.TheTin)
            {
                return null;
            }

            if (edge.RightTriangle.Index == triangle.Index)
            {
                return edge.GetNextInTriangle().ToNode;
            }
            else if (edge.LeftTriangle.Index == triangle.Index)
            {
                return edge.GetNeighbor().GetNextInTriangle().ToNode;
            }
            else
            {
                return null;
            }
        }

        private int setTriangle(ITinTriangle triangle, int tagValue)
        {
            if (!triangle.IsInsideDataArea || triangle.TagValue % 4 == 3)
            {
                return 0;
            }

            ITinEdit tin = triangle.TheTin as ITinEdit;

            tin.SetTriangleOutsideDataArea(triangle.Index);
            return 0;
        }

        private int getTriangleKind(ITinTriangle triangle)
        {
            if (!triangle.IsInsideDataArea)
                return 0;

            ITinAdvanced2 tin = triangle.TheTin as ITinAdvanced2;
            int t1, t2, t3;
            triangle.QueryAdjacentTriangleIndices(out t1, out t2, out t3);

            int count = 0;
            if (t1 != 0 && tin.GetTriangle(t1).IsInsideDataArea)
                count++;
            if (t2 != 0 && tin.GetTriangle(t2).IsInsideDataArea)
                count++;
            if (t3 != 0 && tin.GetTriangle(t3).IsInsideDataArea)
                count++;

            return count;
        }

        private IPoint getCenterPoint(ITinTriangle triangle)
        {
            ITinNode[] n = new ITinNode[3];
            n[0] = triangle.get_Node(0);
            n[1] = triangle.get_Node(1);
            n[2] = triangle.get_Node(2);

            IPoint pt = new PointClass { 
                X = (n[0].X + n[1].X+n[2].X)/ 3.0,
                Y = (n[0].Y + n[1].Y + n[2].Y) / 3.0
            };
            //return pt;
            int index = 0;
            double minl = double.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                double l = distance(n[(i + 1) % 3], n[(i + 2) % 3]);
                if (minl > l)
                {
                    index = i;
                    minl = l;
                }
            }
            IPoint p = new PointClass();
            p.X = (n[index].X * 2 + n[(index + 1) % 3].X + n[(index + 2) % 3].X) / 4;
            p.Y = (n[index].Y * 2 + n[(index + 1) % 3].Y + n[(index + 2) % 3].Y) / 4;
            return p;
        }
        private double distance(ITinNode n, ITinNode o)
        {
            return Math.Sqrt((n.X - o.X) * (n.X - o.X) + (n.Y - o.Y) * (n.Y - o.Y));
        }
        private double Max(double x, double y, double z)
        {
            double c = (x > y) ? x : y;
            return (c > z ? c : z);
        }
        private double Min(double x, double y, double z)
        {
            double c = (x < y) ? x : y;
            return (c < z ? c : z);
        }

        private void MergePoints(List<PolylineInfo> lines, IEnvelope env)
        {
            //1.取得落在三类三角形内的端点
            List<PointInfo> points = new List<PointInfo>();
            foreach (PolylineInfo info in lines)
            {
                if (info.Triangles[0].TagValue == 3)
                {
                    points.Add(new PointInfo(points.Count, info, true));
                }
                if (info.Triangles[info.Triangles.Count - 1].TagValue == 3)
                {
                    points.Add(new PointInfo(points.Count, info, false));
                }
            }

            // 2. 构建三角网，并记录三角网每个节点对应的端点信息
            TinClass tin = new TinClass();
            tin.InitNew(env);
            tin.StartInMemoryEditing();

            Dictionary<int, List<PointInfo>> dic = new Dictionary<int, List<PointInfo>>();
            object z = 0;
            foreach (PointInfo pi in points)
            {
                IPoint p = pi.Point;
                p.Z = 0;
                ITinNode node = new TinNodeClass();
                int maxValue = tin.NodeCount;
                tin.AddPointZ(p, pi.Index, node);
                if (node.Index > maxValue)
                {
                    List<PointInfo> pis = new List<PointInfo>();
                    dic.Add(node.Index, pis);
                }
                dic[node.Index].Add(pi);
            }

            //3.按照三角网对端点进行分组
            List<List<ITinNode>> groups = new List<List<ITinNode>>();
            for (int i = 1; i <= tin.NodeCount; i++)
            {
                ITinNode node = tin.GetNode(i);
                if (!node.IsInsideDataArea)
                {
                    continue;
                }

                PointInfo pi = points[node.TagValue];
                if (pi.Used)
                {
                    continue;
                }

                List<ITinNode> group = new List<ITinNode>();
                FindNodes(points, node, group);
                if (group.Count > 1)
                {
                    groups.Add(group);
                }
            }

            //4.把每个分组的点拉到一个点上去
            foreach (List<ITinNode> g in groups)
            {
                int groupIndex = groups.IndexOf(g);
                IPoint p = new PointClass();
                p.X = 0;
                p.Y = 0;
                foreach (ITinNode node in g)
                {
                    p.X += node.X / g.Count;
                    p.Y += node.Y / g.Count;
                }
                foreach (ITinNode node in g)
                {
                    foreach (PointInfo pi in dic[node.Index])
                    {
                        IPointCollection pc = pi.Info.Polyline as IPointCollection;
                        if (pi.IsFromPoint)
                        {
                            pc.UpdatePoint(0, p);
                            pi.Info.FromGroup = groupIndex;
                        }
                        else
                        {
                            pc.UpdatePoint(pc.PointCount - 1, p);
                            pi.Info.ToGroup = groupIndex;
                        }
                    }
                }
            }

            //5.移除两端节点都动了的线
            List<PolylineInfo> removeLines = new List<PolylineInfo>();
            foreach (PolylineInfo line in lines)
            {
                if (line.FromGroup == line.ToGroup)
                {
                    removeLines.Add(line);
                }
            }
            foreach (PolylineInfo line in removeLines)
            {
                lines.Remove(line);
            }
        }

        private void FindNodes(List<PointInfo> points, ITinNode seed, List<ITinNode> group)
        {
            PointInfo pi = points[seed.TagValue];
            if (pi.Used)
            {
                return;
            }
            group.Add(seed);
            pi.Used = true;

            ITinEdgeArray edges = seed.GetIncidentEdges();

            for (int i = 0; i < edges.Count; i++)
            {
                ITinEdge edge = edges.get_Element(i);
                if (edge.Length < minLineLength / 2)
                {
                    if (edge.ToNode.IsInsideDataArea)
                    {
                        FindNodes(points, edge.ToNode, group);
                    }
                }
            }


        }
        internal class PointInfo
        {
            public PointInfo(int index, PolylineInfo info, bool isFrom)
            {
                Index = index;
                Info = info;
                IsFromPoint = isFrom;
                Used = false;
            }
            public int Index;
            public PolylineInfo Info;
            public bool IsFromPoint;

            public bool Used;

            public IPoint Point
            {
                get { return IsFromPoint ? Info.Polyline.FromPoint : Info.Polyline.ToPoint; }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.GeneralEdit
{
    public class CenterLinePath
    {
        public IPolyline Line { get; set; }
        public double Width { get; set; }

        public CenterLinePath(IPolyline line, double width)
        {
            Line = line;
            Width = width;
        }
    }

    public class CenterLine : List<CenterLinePath>
    {
        public IPolygon Polygon { get; set; }
        public ITin Tin { get; set; }
        public object Other;

        public IPolyline Line
        {
            get
            {
                var polyline = new PolylineClass();
                foreach (CenterLinePath clp in this)
                {
                    polyline.SpatialReference = clp.Line.SpatialReference;
                    var gc = (IGeometryCollection)clp.Line;
                    for (var i = 0; i < gc.GeometryCount; i++)
                    {
                        polyline.AddGeometry(gc.Geometry[i]);
                    }
                }
                return polyline;
            }
        }

        public CenterLine(IPolygon polygon, ITin tin)
        {
            Polygon = polygon;
            Tin = tin;
            Other = null;
        }
    }

    public class PolylineInfo
    {
        public ITinAdvanced2 Tin { get; set; }
        public IPolyline Polyline { get; set; }
        public double StartLength { get; set; }
        public List<ITinTriangle> Triangles { get; set; }
        public List<ITinEdge> Edges { get; set; }
        public double Area { get; set; }
        public int FromGroup { get; set; }
        public int ToGroup { get; set; }

        public PolylineInfo()
        {
            Triangles = new List<ITinTriangle>();
            Edges = new List<ITinEdge>();
            StartLength = 0;
            FromGroup = -1;
            ToGroup = -2;
            Area = 0;
        }
    }

    public class PointInfo
    {
        public int Index { get; set; }
        public PolylineInfo Info { get; set; }
        public bool IsFromPoint { get; set; }
        public bool Used { get; set; }

        public IPoint Point
        {
            get { return IsFromPoint ? Info.Polyline.FromPoint : Info.Polyline.ToPoint; }
        }

        public PointInfo(int index, PolylineInfo info, bool isFrom)
        {
            Index = index;
            Info = info;
            IsFromPoint = isFrom;
            Used = false;
        }
    }

    public class CenterLineHelper
    {
        private double _minLength;
        private double _minDistance;
        public CenterLineHelper()
        {
            _minLength = 80;
            _minDistance = 50;
        }

        //生成中心线
        public CenterLine Create(IPolygon polygon)
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
            var width = (((IArea)polygon).Area / polygon.Length) * 2.5;
            _minDistance = width * 0.5;
            _minLength = width * 2;

            //1 加密多边形
            var newpolygon = AddPoints(polygon);

            //2 生成三角网
            var tin = CreateTin(newpolygon);

            //3.1 计算tag值
            SetTriangleTag(tin as ITinAdvanced2);
            SetEdgeTag(tin as ITinAdvanced2);

            int count;
            List<PolylineInfo> lines;
            do
            {
                //3.2 获取骨架线
                lines = GetLines(tin as ITinAdvanced2);

                //3.3 剔除小毛刺
                count = CheckLines(tin as ITinEdit, lines);
            }
            while (count != 0);

            //4 化简并计算每一段的面积
            foreach (var line in lines)
            {
                line.Polyline.Generalize(width / 20);
                line.Polyline.SpatialReference = polygon.SpatialReference;
                /*
                if (info.Triangles[0].TagValue == 3 && info.Triangles[info.Triangles.Count - 1].TagValue == 1)
                {
                    bool s = true;
                    (info.Polyline as IConstructCurve).ConstructExtended((info.Polyline as IClone).Clone() as IPolyline , polygon, 8, ref s);
                    info.Polyline.SpatialReference = polygon.SpatialReference;
                    //info.Polyline = (polygon as ITopologicalOperator).Intersect(info.Polyline, esriGeometryDimension.esriGeometry2Dimension) as IPolyline;
                }
                 */
                line.Polyline.SnapToSpatialReference();

                line.Area = 0;
                foreach (ITinTriangle tri in line.Triangles)
                {
                    if (tri.TagValue == 3)
                    {
                        line.Area += tri.Area / 3;
                    }
                    line.Area += tri.Area;
                }
            }

            //5 合并交叉口
            MergePoints(lines, polygon.Envelope);

            CenterLine centerLine = new CenterLine(polygon,tin);
            foreach (PolylineInfo info in lines)
            {
                centerLine.Add(new CenterLinePath(info.Polyline,info.Area / info.Polyline.Length));
            }

            return centerLine;
        }

        //加密多边形
        private IPolygon AddPoints(IPolygon polygon)
        {
            var newPolygon = new PolygonClass {SpatialReference = polygon.SpatialReference};
            var gc = (IGeometryCollection)polygon;

            for (var i = 0; i < gc.GeometryCount; i++)
            {
                var ring = new RingClass();
                var pc = (IPointCollection)gc.Geometry[i];

                IPoint currentPoint = new PointClass();
                for (var j = 0; j < pc.PointCount; j++)
                {
                    var x = pc.Point[j].X;
                    var y = pc.Point[j].Y;

                    if (j == 0)
                    {
                        currentPoint = new PointClass {X = x, Y = y};
                        ring.AddPoint(currentPoint);
                        continue;
                    }

                    var distane = Math.Sqrt((x - currentPoint.X) * (x - currentPoint.X) + (y - currentPoint.Y) * (y - currentPoint.Y));
                    var addCount = Convert.ToInt32(distane / _minDistance);
                    addCount = addCount < 2 ? 2 : addCount;

                    for (var k = 1; k <= addCount; k++)
                    {
                        ring.AddPoint(new PointClass { X = (x * k + currentPoint.X * (addCount - k)) / addCount, Y = (y * k + currentPoint.Y * (addCount - k)) / addCount });
                    }

                    currentPoint = new PointClass { X = x, Y = y };
                }
                newPolygon.AddGeometry(ring);
            }
            return newPolygon;
        }

        //根据多边形生成esriTinHardClip类型的Tin
        public ITin CreateTin(IPolygon polygon)
        {
            var tin = new TinClass();
            var tinExtent = new EnvelopeClass {SpatialReference = polygon.SpatialReference};
            tinExtent.Union(polygon.Envelope);
            tin.InitNew(tinExtent);
            tin.StartInMemoryEditing();

            object z = 0;
            tin.AddShape(polygon, esriTinSurfaceType.esriTinHardClip, 0, ref z);
            return tin;
        }

        //设置Tin三角形的tag值
        private void SetTriangleTag(ITinAdvanced2 tin)
        {
            var tinEdit = (ITinEdit) tin;
            for (var i = 1; i <= tin.TriangleCount; i++)
            {
                var triangle = tin.GetTriangle(i);
                if (!triangle.IsInsideDataArea)
                {
                    tinEdit.SetTriangleTagValue(i, 0);
                    continue;
                }

                int t1, t2, t3;
                triangle.QueryAdjacentTriangleIndices(out t1, out t2, out t3);

                var count = 0;
                if (t1 != 0 && tin.GetTriangle(t1).IsInsideDataArea)
                    count++;
                if (t2 != 0 && tin.GetTriangle(t2).IsInsideDataArea)
                    count++;
                if (t3 != 0 && tin.GetTriangle(t3).IsInsideDataArea)
                    count++;

                tinEdit.SetTriangleTagValue(i, count);
            }
        }

        //设置Tin边的tag值
        private void SetEdgeTag(ITinAdvanced2 tin)
        {
            var tinEdit = (ITinEdit) tin;
            for (var i = 1; i <= tin.EdgeCount; i++)
            {
                var edge = tin.GetEdge(i);
                if (!edge.IsInsideDataArea)
                {
                    tinEdit.SetEdgeTagValue(i, 0);
                    continue;
                }

                var count = 0;
                if (edge.LeftTriangle.TagValue != 0)
                    count++;
                if (edge.RightTriangle.TagValue != 0)
                    count++;
                tinEdit.SetEdgeTagValue(i, count);
            }
        }

        //根据tin获取骨架线
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
                        || info.Polyline.Length - info.StartLength > _minLength)
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
                    || info.Polyline.Length - info.StartLength > _minLength)
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
                tin.SetTriangleTagValue(triangle.Index, triangle.TagValue -1);
            }
            
            return count;
        }

        /// <summary>
        /// 生成岔口交叉线
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public IPolyline GetSplitLine(IPolygon polygon)
        {
          
            IPolyline line = null;
            object o=Type.Missing;
            IGeometryCollection bag = new GeometryBagClass() as IGeometryCollection;
              
            double w = ((polygon as IArea).Area / polygon.Length) * 2.5;
            _minDistance = w * 0.5;
            _minLength = w * 2;
            #region
            //1 加密多边形
            IPolygon poly = AddPoints(polygon);
            

            //2 生成三角网
            ITin tin1 = CreateTin(poly);
               
            ITinAdvanced2 tin = tin1 as ITinAdvanced2;
            SetTriangleTag(tin as ITinAdvanced2);
            SetEdgeTag(tin as ITinAdvanced2);
            int count = 0;
            List<PolylineInfo> lines = null;
            do
            {
                //3.2 获取骨架线
                lines = GetLines(tin as ITinAdvanced2);
                //3.3 剔除小毛刺
                count = CheckLines(tin as ITinEdit, lines);
                GC.Collect();
            }
            while (count != 0);
               
            //4.提取道路岔口分割线
            for (int i = 1; i <= tin.TriangleCount; i++)
            {
                ITinTriangle triangle = tin.GetTriangle(i);
                if (!triangle.IsInsideDataArea)
                {
                    continue;
                }

                int t1, t2, t3;
                triangle.QueryAdjacentTriangleIndices(out t1, out t2, out t3);

                count = 0;
                if (t1 != 0 && tin.GetTriangle(t1).IsInsideDataArea && tin.GetTriangle(t1).TagValue != 1)
                    count++;
                if (t2 != 0 && tin.GetTriangle(t2).IsInsideDataArea && tin.GetTriangle(t2).TagValue != 1)
                    count++;
                if (t3 != 0 && tin.GetTriangle(t3).IsInsideDataArea && tin.GetTriangle(t3).TagValue != 1)
                    count++;
                if (triangle.TagValue==3&&count==3)
                {

                    IPoint centerpoint = getCenterPoint(triangle);
                    IPoint pPoint1 =new PointClass();
                    triangle.get_Node(0).QueryAsPoint(pPoint1);
                    IPoint pPoint2 = new PointClass();
                    triangle.get_Node(1).QueryAsPoint(pPoint2);
                    IPoint pPoint3 = new PointClass();
                    triangle.get_Node(2).QueryAsPoint(pPoint3);

                    ILine pline1 = new LineClass();
                    pline1.PutCoords(pPoint1,centerpoint );
                    ILine pline2 = new LineClass();
                    pline2.PutCoords(pPoint2,centerpoint);
                    ILine pline3= new LineClass();
                    pline3.PutCoords(centerpoint, pPoint3);

                    ISegmentCollection gc = new PolylineClass();
                    gc.AddSegment(pline1 as ISegment, ref o, ref o);
                    bag.AddGeometry((IGeometry)gc, ref o, ref o);
               
                    gc = new PolylineClass();
                    gc.AddSegment(pline2 as ISegment, ref o, ref o);
                    bag.AddGeometry((IGeometry)gc, ref o, ref o);
                 
                    gc = new PolylineClass();
                    gc.AddSegment(pline3 as ISegment, ref o, ref o);
                    bag.AddGeometry((IGeometry)gc, ref o, ref o);
                    
                }

            }
            GC.Collect();
            #endregion
            if(bag.GeometryCount>0)
            {
                ITopologicalOperator op = new PolylineClass() as ITopologicalOperator;
                op.ConstructUnion(bag as IEnumGeometry);
                line = op as IPolyline;
            }
            return line;
        }
        public void ShowAllTINEdge(IMap map,IPolygon polygon)
        {


            try
            {
                double w = ((polygon as IArea).Area / polygon.Length) * 2.5;
                _minDistance = w * 0.5;
                _minLength = w * 2;

                //1 加密多边形
                IPolygon poly = AddPoints(polygon);


                //2 生成三角网
                ITin tin1 = CreateTin(poly);

                ITinAdvanced2 tin = tin1 as ITinAdvanced2;
                SetTriangleTag(tin);
                ITinEdit tinEdit = tin as ITinEdit;
                for (int i = 1; i <= tin.TriangleCount; i++)
                {
                    ITinTriangle triangle = tin.GetTriangle(i);
                    IPoint center = getCenterPoint(triangle);
                    if (!triangle.IsInsideDataArea)
                    {
                        tinEdit.SetTriangleTagValue(i, 0);
                        continue;
                    }

                    int t1, t2, t3;
                    triangle.QueryAdjacentTriangleIndices(out t1, out t2, out t3);

                    int count = 0;
                    if (t1 != 0 && tin.GetTriangle(t1).IsInsideDataArea && tin.GetTriangle(t1).TagValue != 1)
                        count++;
                    if (t2 != 0 && tin.GetTriangle(t2).IsInsideDataArea && tin.GetTriangle(t2).TagValue != 1)
                        count++;
                    if (t3 != 0 && tin.GetTriangle(t3).IsInsideDataArea && tin.GetTriangle(t3).TagValue != 1)
                        count++;
                    // if (triangle.TagValue==3&&count==3)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            ITinEdge edge = triangle.get_Edge(j);
                            if (edge.TagValue != -2)
                            {
                                ISegmentCollection gc = new PolylineClass();
                                ILine pline = new LineClass();
                                edge.QueryAsLine(pline);
                                tinEdit.SetEdgeTagValue(edge.Index, -2);
                                gc.AddSegment(pline as ISegment);
                            }
                        }
                    }
                    //tinEdit.SetTriangleTagValue(i, count);
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return gc as IPolyline;
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

            int fromNode = edge.FromNode.Index;
            int toNode = edge.ToNode.Index;

            if (fromNode == triangle.get_Node(0).Index)
            {
                if (toNode == triangle.get_Node(1).Index)
                {
                    return triangle.get_Node(2);
                }
                if (toNode == triangle.get_Node(2).Index)
                {
                    return triangle.get_Node(1);
                }
            }
            else if (fromNode == triangle.get_Node(1).Index)
            {
                if (toNode == triangle.get_Node(0).Index)
                {
                    return triangle.get_Node(2);
                }
                if (toNode == triangle.get_Node(2).Index)
                {
                    return triangle.get_Node(0);
                }
            }
            else if (fromNode == triangle.get_Node(2).Index)
            {
                if (toNode == triangle.get_Node(1).Index)
                {
                    return triangle.get_Node(0);
                }
                if (toNode == triangle.get_Node(0).Index)
                {
                    return triangle.get_Node(1);
                }
            }
            else
            {
                return null;
            }
            return null;
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
            int index = 0;
            //double minl = double.MaxValue;
            //for (int i = 0; i < 3; i++)
            //{ 
            //    double l = distance(n[(i+1)%3], n[(i+2)%3]);
            //    if (minl > l)
            //    {
            //        index = i;
            //        minl = l;
            //    }
            //}
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

        private void MergePoints(List<PolylineInfo> lines,IEnvelope env)
        {
            //1.取得落在三类三角形内的端点
            List<PointInfo> points = new List<PointInfo>();
            foreach (PolylineInfo info in lines)
            {
                if (info.Triangles[0].TagValue == 3)
                {
                    points.Add(new PointInfo(points.Count,info,true));
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

            Dictionary<int, List<PointInfo>> dic = new Dictionary<int,List<PointInfo>>();
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
                if (edge.Length < _minLength / 2)
                {
                    if (edge.ToNode.IsInsideDataArea)
                    {
                        FindNodes(points, edge.ToNode, group);
                    }
                }
            }
            

        }
    }
}

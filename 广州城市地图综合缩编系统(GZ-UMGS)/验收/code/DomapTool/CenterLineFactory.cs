using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

using System.Diagnostics;

namespace DomapTool
{
  public class CenterLine : List<CenterLinePath> {
    public CenterLine(IPolygon polygon, ITin tin) {
      this.tin = tin;
      this.polygon = polygon;
      Other = null;
    }

    public IPolyline Line {
      get {
        PolylineClass resultPolyline = new PolylineClass();
        foreach (CenterLinePath info in this) {
          resultPolyline.SpatialReference = info.Line.SpatialReference;
          for (int i = 0; i < (info.Line as IGeometryCollection).GeometryCount; i++) {
            IGeometry geo = (info.Line as IGeometryCollection).get_Geometry(i);
            resultPolyline.AddGeometries(1, ref geo);
          }
        }
        return resultPolyline;
      }
    }
    public ITin Tin {
      get { return tin; }
    }
    public IPolygon Polygon {
      get { return polygon; }
    }
    public object Other;
    private IPolygon polygon;
    private ITin tin;
  }

  public class CenterLinePath {
    public CenterLineFactory.PolylineInfo Info { get; set; }
    public CenterLinePath(IPolyline line, double width) {
      Line = line;
      Width = width;
    }
    public CenterLinePath(CenterLineFactory.PolylineInfo info) {
      Info = info;
      Line = info.Polyline;
      Width = info.Area / Line.Length;
    }
    public IPolyline Line;
    public double Width;
  }

  public class CenterLineFactory {
    public class PolylineInfo {
      internal PolylineInfo() {
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

    public CenterLineFactory() {
      minLineLength = 80;
      minDistance = 50;
    }

    private double minLineLength;
    private double minDistance;

    struct TagInfo {
      public int ringIndex;
      public int segIndex;      
    }
    public void Create3(IPolygon polygon) {
      double w = ((polygon as IArea).Area / polygon.Length) * 2.5;
      minDistance = w * 0.5;
      minLineLength = w * 2;

      
      IPolygon poly = (polygon as IClone).Clone() as IPolygon;
      poly.Densify(minDistance,0);

      TinClass tin = new TinClass();
      tin.InitNew(poly.Envelope);
      tin.StartInMemoryEditing();

      List<TagInfo> tags = new List<TagInfo>();
      
      for (int i = 0; i < (poly as IGeometryCollection).GeometryCount; i++) {
        ISegmentCollection ring = (poly as IGeometryCollection).get_Geometry(i) as ISegmentCollection;
        for (int j = 0; j < ring.SegmentCount; j++) {
          var index = new TagInfo();
          index.ringIndex = i;
          index.segIndex = j;
          IPoint p = ring.get_Segment(j).FromPoint;
          p.Z = 0;
          tin.AddPointZ(p, tags.Count);
          tags.Add(index);
        }
      }
      
      tin.SelectAll(esriTinElementType.esriTinTriangle, true);
      tin.SelectByArea(esriTinElementType.esriTinTriangle, poly, true, true, esriTinSelectionType.esriTinSelectionFlip);
      IEnumTinElement tinElements = tin.GetSelection(esriTinElementType.esriTinTriangle);
      ITinElement triangle = null;
      while ((triangle = tinElements.Next())!=null) {
        tin.SetTriangleOutsideDataArea(triangle.Index);
      }

      SetTriangleTag(tin as ITinAdvanced2);
      
    }
    

    public CenterLine Create2(IPolygon polygon) {
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
      //IPolygon poly = (polygon as IClone).Clone() as IPolygon;
      poly.Generalize(w* 0.05);
      poly.Densify(minDistance, 0);
      //2 生成三角网
      ITin tin = GetTin(poly);
      ITinEdit tinEdit = tin as ITinEdit;

      //3.1 计算tag值
      SetTriangleTag(tin as ITinAdvanced2);
      SetEdgeTag(tin as ITinAdvanced2);


      int count = 0;
      List<PolylineInfo> lines = null;
      do {
        //tinEdit.DeleteNodesOutsideDataArea();

        //3.2 获取骨架线
        lines = GetLines(tin as ITinAdvanced2);

        //3.3 剔除小毛刺
        count = CheckLines(tin as ITinEdit, lines);

      }
      while (count != 0);

      //4 化简并计算每一段的面积
      foreach (PolylineInfo info in lines) {
        info.Polyline.Generalize(w / 20);
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

        info.Area = 0;
        foreach (ITinTriangle tri in info.Triangles) {
          if (tri.TagValue == 3) {
            info.Area += tri.Area / 3;
          }
          info.Area += tri.Area;
        }
      }

      //5 合并交叉口
      MergePoints(lines, polygon.Envelope);

      CenterLine centerLine = new CenterLine(polygon, tin);
      foreach (PolylineInfo info in lines) {
        centerLine.Add(new CenterLinePath(info));
      }

      return centerLine;
    }

    public IPolyline Create(IPolygon polygon) {
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
      //IPolygon poly = (polygon as IClone).Clone() as IPolygon;

      //2 生成三角网
      ITin tin = GetTin(poly);
      ITinEdit tinEdit = tin as ITinEdit;

      //3.1 计算tag值
      SetTriangleTag(tin as ITinAdvanced2);
      SetEdgeTag(tin as ITinAdvanced2);


      int count = 0;
      List<PolylineInfo> lines = null;
      do {
        //tinEdit.DeleteNodesOutsideDataArea();

        //3.2 获取骨架线
        lines = GetLines(tin as ITinAdvanced2);

        //3.3 剔除小毛刺
        count = CheckLines(tin as ITinEdit, lines);

      }
      while (count != 0);

      //4 化简并计算每一段的面积
      foreach (PolylineInfo info in lines) {
        info.Polyline.Generalize(0.1);
        info.Polyline.SpatialReference = polygon.SpatialReference;
        info.Area = 0;
        foreach (ITinTriangle tri in info.Triangles) {
          info.Area += tri.Area;
        }
      }

      //5 合并交叉口
      MergePoints(lines, polygon.Envelope);

      //6 输出中轴线
      PolylineClass resultPolyline = new PolylineClass();
      foreach (PolylineInfo info in lines) {
        resultPolyline.SpatialReference = (tin as IGeoDataset).SpatialReference;
        for (int i = 0; i < (info.Polyline as IGeometryCollection).GeometryCount; i++) {
          IGeometry geo = (info.Polyline as IGeometryCollection).get_Geometry(i);
          resultPolyline.AddGeometries(1, ref geo);
        }
      }

      resultPolyline.SpatialReference = polygon.SpatialReference;

      return resultPolyline;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tin"></param>
    /// <param name="lines"></param>
    /// <returns></returns>
    private int CheckLines(ITinEdit tin, List<PolylineInfo> lines) {
      int count = 0;
      if (lines.Count == 3) {
        bool exception = true;
        foreach (PolylineInfo info in lines) {
          if (info.Triangles[0].TagValue != 3
              || info.Triangles[info.Triangles.Count - 1].TagValue != 1
              || info.Polyline.Length - info.StartLength > minLineLength) {
            exception = false;
            break;
          }
        }
        if (exception) {
          return 0;
        }
      }
      List<ITinTriangle> startTri = new List<ITinTriangle>();
      foreach (PolylineInfo info in lines) {
        //起点三角形不为3类、终点三角形不为1类，长度大于阈值
        if (info.Triangles[0].TagValue != 3
            || info.Triangles[info.Triangles.Count - 1].TagValue != 1
            || info.Polyline.Length - info.StartLength > minLineLength) {
          foreach (ITinEdge edge in info.Edges) {
            tin.SetEdgeTagValue(edge.Index, 2);
          }
          continue;
        }


        int index0 = info.Triangles[0].Index;
        foreach (ITinTriangle triangle in info.Triangles) {
          int index = triangle.Index;
          if (index == index0) {
            startTri.Add(triangle);
            continue;
          }

          tin.SetTriangleTagValue(index, 0);
        }

        tin.SetEdgeTagValue(info.Edges[0].Index, 1);
        count++;
      }

      foreach (ITinTriangle triangle in startTri) {
        tin.SetTriangleTagValue(triangle.Index, triangle.TagValue - 1);
      }

      return count;
    }

    /// <summary>
    /// 根据多边形生成tin，生成类别为esriTinHardClip
    /// </summary>
    /// <param name="polygon">需要生成的多边形</param>
    /// <returns>返回生成的tin</returns>
    private ITin GetTin(IPolygon polygon) {
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
    private IPolygon AddPoints(IPolygon polygon) {
      IPolygon poly = (polygon as IClone).Clone() as IPolygon;
      IGeometryCollection gc = poly as IGeometryCollection;

      for (int i = 0; i < gc.GeometryCount; i++) {
        IRing ring = gc.get_Geometry(i) as IRing;
        IPointCollection pc = ring as IPointCollection;

        IPoint currentPoint = new PointClass();
        for (int j = 0; j < pc.PointCount; j++) {
          IPoint p1 = pc.get_Point(j);
          double x = p1.X;
          double y = p1.Y;
          if (j != 0) {
            int addCount = 0;

            double distane = Math.Sqrt((x - currentPoint.X) * (x - currentPoint.X) + (y - currentPoint.Y) * (y - currentPoint.Y));
            addCount = Convert.ToInt32(distane / minDistance);
            if (addCount < 2) {
              addCount = 2;
            }
            for (int k = 1; k <= addCount; k++) {
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

    private IPolygon AddPoints2(IPolygon polygon) {
      PolygonClass poly = new PolygonClass();
      poly.SpatialReference = polygon.SpatialReference;
      IGeometryCollection gc = polygon as IGeometryCollection;

      for (int i = 0; i < gc.GeometryCount; i++) {
        RingClass r = new RingClass();
        IRing ring = gc.get_Geometry(i) as IRing;
        IPointCollection pc = ring as IPointCollection;

        IPoint currentPoint = new PointClass();
        for (int j = 0; j < pc.PointCount; j++) {
          IPoint p1 = pc.get_Point(j);
          double x = p1.X;
          double y = p1.Y;

          if (j == 0) {
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
          if (addCount < 2) {
            addCount = 2;
          }
          for (int k = 1; k <= addCount; k++) {
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
    private void SetTriangleTag(ITinAdvanced2 tin) {
      ITinEdit tinEdit = tin as ITinEdit;
      for (int i = 1; i <= tin.TriangleCount; i++) {
        ITinTriangle triangle = tin.GetTriangle(i);
        if (!triangle.IsInsideDataArea) {
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
    private void SetEdgeTag(ITinAdvanced2 tin) {
      ITinEdit tinEdit = tin as ITinEdit;
      for (int i = 1; i <= tin.EdgeCount; i++) {
        ITinEdge edge = tin.GetEdge(i);
        if (!edge.IsInsideDataArea) {
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
    private List<PolylineInfo> GetLines(ITinAdvanced2 tin) {
      List<PolylineInfo> lines = new List<PolylineInfo>();
      int Value3Count = 0;
      int tag1Index = 0;
      #region 如果存在任意一个3类三角形
      for (int i = 1; i <= tin.TriangleCount; i++) {
        ITinTriangle triangle = tin.GetTriangle(i);
        ITinEdit tinEdit = tin as ITinEdit;
        if (triangle.TagValue != 3) {
          if (triangle.TagValue == 1) {
            tag1Index = triangle.Index;
          }
          continue;
        }
        Value3Count++;

        IPoint point = new PointClass();
        IPoint beginPoint = getCenterPoint(triangle);

        #region 分三个方向分别添加
        for (int j = 0; j < 3; j++) {
          ITinEdge edge = triangle.get_Edge(j);
          if (edge.TagValue != 2) {
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
          ITinTriangle triangle2 = edge.LeftTriangle;
          //if (edge.LeftTriangle.Index == triangle.Index) {
          //  triangle2 = edge.RightTriangle;
          //}
          //else {
          //  triangle2 = edge.LeftTriangle;
          //}

          //针对3种情况分别添加
          do {
            polylineInfo.Triangles.Add(triangle2);
            polylineInfo.Edges.Add(edge);
            IPoint nextPoint = nextTriangle(ref triangle2, ref edge);
            if (nextPoint != null) {
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

      if (Value3Count == 0 && tag1Index != 0) {
        ITinTriangle triangle = tin.GetTriangle(tag1Index);
        ITinEdge edge = null;
        for (int j = 0; j < 3; j++) {
          if (triangle.get_Edge(j).TagValue == 2) {
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
        if (edge.LeftTriangle.Index == triangle.Index) {
          triangle2 = edge.RightTriangle;
        }
        else {
          triangle2 = edge.LeftTriangle;
        }

        do {
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
      if (Value3Count == 0 && tag1Index == 0) {
        ITinTriangle triangle = null;
        for (int i = 1; i <= tin.TriangleCount; i++) {
          triangle = tin.GetTriangle(i);
          if (triangle.TagValue == 2) {
            break;
          }
        }
        int startIndex = triangle.Index;
        ITinEdge edge = null;
        for (int j = 0; j < 3; j++) {
          if (triangle.get_Edge(j).TagValue == 2) {

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

        IPoint fromPoint = new PointClass();
        fromPoint.X = point.X;
        fromPoint.Y = point.Y;

        do {
          polylineInfo.Triangles.Add(triangle);
          polylineInfo.Edges.Add(edge);
          point = nextTriangle(ref triangle, ref edge);
          poly.AddPoints(1, ref point);
        }
        while (triangle != null && triangle.Index != startIndex && edge.TagValue == 2);

        poly.AddPoints(1, ref fromPoint);
        polylineInfo.Polyline = poly;
        lines.Add(polylineInfo);

      }
      #endregion
      return lines;
    }

    /// <summary>
    /// 获取当前三角形的点，并将边和三角形都指向下一个
    /// </summary>
    /// <param name="triangle">
    /// 经过的三角形，完成后被改为下一个三角形，没有下一个则为空
    /// 第一次进入为：
    /// 起始三角形的下一个三角形
    /// </param>
    /// <param name="edge">
    /// 进入该三角形的边，完成后被改为三角形出来的边，没有下一个则为空
    /// 第一次进入为：
    /// 起始三角形的边
    /// </param>
    /// <returns>返回下一个点</returns>
    private IPoint nextTriangle(ref ITinTriangle triangle, ref ITinEdge edge) {
      ITinAdvanced2 tin = triangle.TheTin as ITinAdvanced2;
      ITinEdit tinEdit = tin as ITinEdit;

      tinEdit.SetEdgeTagValue(edge.Index, -2);

      IPoint point = new PointClass();

      if (triangle.TagValue == 1) {
        ITinNode node = edge.GetNeighbor().GetNextInTriangle().ToNode;//getOtherNode(triangle, edge);
        node.QueryAsPoint(point);
        triangle = null;
      }
      else if (triangle.TagValue == 3) {
        point = getCenterPoint(triangle);
        triangle = null;
      }
      else if (triangle.TagValue == 2) {
        ITinNode node = edge.GetNeighbor().GetNextInTriangle().ToNode;//getOtherNode(triangle, edge);

        edge = edge.GetNeighbor();
        if (edge.GetNextInTriangle().TagValue == 2) {
          edge = edge.GetNextInTriangle();
        }
        else {
          edge = edge.GetPreviousInTriangle();
        }

        //for (int k = 0; k < 3; k++) {
        //  ITinEdge edge2 = triangle.get_Edge(k);
        //  if ((edge2.FromNode.Index == node.Index || edge2.ToNode.Index == node.Index) && edge2.TagValue == 2) {
        //    edge = edge2;
        //  }
        //}

        point.X = (edge.FromNode.X + edge.ToNode.X) / 2;
        point.Y = (edge.FromNode.Y + edge.ToNode.Y) / 2;
        triangle = edge.LeftTriangle;
        //if (edge.LeftTriangle.Index == triangle.Index) {
        //  triangle = edge.RightTriangle;
        //}
        //else {
        //  triangle = edge.LeftTriangle;
        //}
      }
      else {
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
    private ITinNode getOtherNode(ITinTriangle triangle, ITinEdge edge) {
      if (triangle.TheTin != edge.TheTin) {
        return null;
      }

      int fromNode = edge.FromNode.Index;
      int toNode = edge.ToNode.Index;

      if (fromNode == triangle.get_Node(0).Index) {
        if (toNode == triangle.get_Node(1).Index) {
          return triangle.get_Node(2);
        }
        if (toNode == triangle.get_Node(2).Index) {
          return triangle.get_Node(1);
        }
      }
      else if (fromNode == triangle.get_Node(1).Index) {
        if (toNode == triangle.get_Node(0).Index) {
          return triangle.get_Node(2);
        }
        if (toNode == triangle.get_Node(2).Index) {
          return triangle.get_Node(0);
        }
      }
      else if (fromNode == triangle.get_Node(2).Index) {
        if (toNode == triangle.get_Node(1).Index) {
          return triangle.get_Node(0);
        }
        if (toNode == triangle.get_Node(0).Index) {
          return triangle.get_Node(1);
        }
      }
      else {
        return null;
      }
      return null;
    }

    private int setTriangle(ITinTriangle triangle, int tagValue) {
      if (!triangle.IsInsideDataArea || triangle.TagValue % 4 == 3) {
        return 0;
      }

      ITinEdit tin = triangle.TheTin as ITinEdit;

      tin.SetTriangleOutsideDataArea(triangle.Index);
      return 0;
    }

    private int getTriangleKind(ITinTriangle triangle) {
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

    private IPoint getCenterPoint(ITinTriangle triangle) {
      ITinNode[] n = new ITinNode[3];
      n[0] = triangle.get_Node(0);
      n[1] = triangle.get_Node(1);
      n[2] = triangle.get_Node(2);
      int index = 0;
      double minl = double.MaxValue;
      for (int i = 0; i < 3; i++) {
        double l = distance(n[(i + 1) % 3], n[(i + 2) % 3]);
        if (minl > l) {
          index = i;
          minl = l;
        }
      }
      IPoint p = new PointClass();
      p.X = (n[index].X * 2 + n[(index + 1) % 3].X + n[(index + 2) % 3].X) / 4;
      p.Y = (n[index].Y * 2 + n[(index + 1) % 3].Y + n[(index + 2) % 3].Y) / 4;
      return p;
    }
    private double distance(ITinNode n, ITinNode o) {
      return Math.Sqrt((n.X - o.X) * (n.X - o.X) + (n.Y - o.Y) * (n.Y - o.Y));
    }
    private double Max(double x, double y, double z) {
      double c = (x > y) ? x : y;
      return (c > z ? c : z);
    }
    private double Min(double x, double y, double z) {
      double c = (x < y) ? x : y;
      return (c < z ? c : z);
    }

    private void MergePoints(List<PolylineInfo> lines, IEnvelope env) {
      //1.取得落在三类三角形内的端点
      List<PointInfo> points = new List<PointInfo>();
      foreach (PolylineInfo info in lines) {
        if (info.Triangles[0].TagValue == 3) {
          points.Add(new PointInfo(points.Count, info, true));
        }
        if (info.Triangles[info.Triangles.Count - 1].TagValue == 3) {
          points.Add(new PointInfo(points.Count, info, false));
        }
      }

      // 2. 构建三角网，并记录三角网每个节点对应的端点信息
      TinClass tin = new TinClass();
      tin.InitNew(env);
      tin.StartInMemoryEditing();

      Dictionary<int, List<PointInfo>> dic = new Dictionary<int, List<PointInfo>>();
      object z = 0;
      foreach (PointInfo pi in points) {
        IPoint p = pi.Point;
        p.Z = 0;
        ITinNode node = new TinNodeClass();
        int maxValue = tin.NodeCount;
        tin.AddPointZ(p, pi.Index, node);
        if (node.Index > maxValue) {
          List<PointInfo> pis = new List<PointInfo>();
          dic.Add(node.Index, pis);
        }
        dic[node.Index].Add(pi);
      }

      //3.按照三角网对端点进行分组
      List<List<ITinNode>> groups = new List<List<ITinNode>>();
      for (int i = 1; i <= tin.NodeCount; i++) {
        ITinNode node = tin.GetNode(i);
        if (!node.IsInsideDataArea) {
          continue;
        }

        PointInfo pi = points[node.TagValue];
        if (pi.Used) {
          continue;
        }

        List<ITinNode> group = new List<ITinNode>();
        FindNodes(points, node, group);
        if (group.Count > 1) {
          groups.Add(group);
        }
      }

      //4.把每个分组的点拉到一个点上去
      foreach (List<ITinNode> g in groups) {
        int groupIndex = groups.IndexOf(g);
        IPoint p = new PointClass();
        p.X = 0;
        p.Y = 0;
        foreach (ITinNode node in g) {
          p.X += node.X / g.Count;
          p.Y += node.Y / g.Count;
        }
        foreach (ITinNode node in g) {
          foreach (PointInfo pi in dic[node.Index]) {
            IPointCollection pc = pi.Info.Polyline as IPointCollection;
            if (pi.IsFromPoint) {
              pc.UpdatePoint(0, p);
              pi.Info.FromGroup = groupIndex;
            }
            else {
              pc.UpdatePoint(pc.PointCount - 1, p);
              pi.Info.ToGroup = groupIndex;
            }
          }
        }
      }

      //5.移除两端节点都动了的线
      List<PolylineInfo> removeLines = new List<PolylineInfo>();
      foreach (PolylineInfo line in lines) {
        if (line.FromGroup == line.ToGroup) {
          removeLines.Add(line);
        }
      }
      foreach (PolylineInfo line in removeLines) {
        lines.Remove(line);
      }
    }

    private void FindNodes(List<PointInfo> points, ITinNode seed, List<ITinNode> group) {
      PointInfo pi = points[seed.TagValue];
      if (pi.Used) {
        return;
      }
      group.Add(seed);
      pi.Used = true;

      ITinEdgeArray edges = seed.GetIncidentEdges();

      for (int i = 0; i < edges.Count; i++) {
        ITinEdge edge = edges.get_Element(i);
        if (edge.Length < minLineLength / 2) {
          if (edge.ToNode.IsInsideDataArea) {
            FindNodes(points, edge.ToNode, group);
          }
        }
      }


    }
    internal class PointInfo {
      public PointInfo(int index, PolylineInfo info, bool isFrom) {
        Index = index;
        Info = info;
        IsFromPoint = isFrom;
        Used = false;
      }
      public int Index;
      public PolylineInfo Info;
      public bool IsFromPoint;

      public bool Used;

      public IPoint Point {
        get { return IsFromPoint ? Info.Polyline.FromPoint : Info.Polyline.ToPoint; }
      }
    }
  }
}

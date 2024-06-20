using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using GENERALIZERLib;

namespace DomapTool {
  public class Generalizer {
    private string paraFilePath;
    private double orgScale;
    private double dstScale;
    private domapUnitsType inUnitsType;
    private IGenHydro hydro;
    private IGenPolygon genPolygon;
    private ILineSimplification lineSimplification;
    private IGeneralizerCore core = null;
    private IGenSelection genSelection;
    private SysParaTable para;
    public Generalizer() {
    }
    private const string paraContext = @"
BEGINPARA
小板房长度	1	4
小板房宽度	0	4
生成小板房的最小面积	1	4
小板房选取比例%	60	4
原始资料图比例尺	10000	4
综合目标图比例尺	50000	4
精度设置	1	4
等高线选取高程间距	20	4
等高线删除弯曲的深度	2.59765929080396	4
高程点选取密度每平方分米个数	60	4
首曲线属性码	4	4
计曲线属性码	4	4
曲线特征点提取时的弯曲深度	3.000000	1
面目标转换为点目标的最大面积	400.000000	2
线目标变点目标的最大长度值	50.000000	1
三角网加密步长	2	4
中轴线提取双线邻近的间距	10.000000	1
中轴线提取时多边形邻近间距	749.026265834325	4
中轴线网络可删除的最小弧段长度	13.5654533122313	4
多边形合并的最小间距	357.918346894589	4
种子法构面半径	10.000000	1
曲线压缩矢高	2	4
曲线光滑步长	5.000000	1
边界曲线化简弯曲深度	17.2187156212207	4
线求交后可删除的最小长度	3.000000	1
定长线长度	21.000000	1
多边形小间距探测距离	2	4
多边形瓶颈探测距离	9.25434267073345	4
曲线小弯曲探测弯曲深度	2	4
结点匹配最小间距	1.000000	1
接口咬合容差	1.000000	1
道路拓宽宽度	3.000000	1
街道拓宽宽度	3.000000	1
栅格边长	1.000000	1
平行线生成宽度	3.000000	1
悬挂点检查容差	1.000000	1
征点提取时的弯曲深度	4.000000	1
建筑物多边形化简边长阈值	1.0	4
综合准备文件存放路径	9.000000	1
ENDPARA
";
    public void InitGeneralizer(string paraPath, double orgScale, double dstScale) {
      paraFilePath = paraPath;
      this.orgScale = orgScale;
      this.dstScale = dstScale;
      inUnitsType = domapUnitsType.domapUnitsMeter;

      core = new GeneralizerCoreClass();
      core.InitSysParaTable(paraFilePath);

      core.InitGenEvironment(orgScale, dstScale, 1, inUnitsType);
      IAGeneralization aGeneralization = core as IAGeneralization;
      para = core.SystemParameter;
      hydro = aGeneralization.HydroGeneralize;
      genPolygon = aGeneralization.PolygonGeneralize;
      genSelection = aGeneralization.Selection;
      lineSimplification = aGeneralization.LineSimplification;
    }
    public void InitGeneralizer(double orgScale, double dstScale) {
      string paraPath = System.Environment.GetEnvironmentVariable("TEMP");
      System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(paraPath);
      paraPath = info.FullName;
      int mm = 1;
      //string tempPath=System.Environment.GetFolderPath(Environment.SpecialFolder.System);
      string finalFile = "tp" + mm;
      while (System.IO.File.Exists(paraPath + "\\" + finalFile + ".inf")) {
        mm++;
        finalFile = "tp" + mm;
      }
      paraPath = paraPath + "\\" + finalFile + ".inf";
      System.IO.FileStream stream = System.IO.File.Create(paraPath);
      System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);
      writer.Write(paraContext);
      writer.Flush();
      writer.Close();
      stream.Close();
      paraFilePath = paraPath;
      this.orgScale = orgScale;
      this.dstScale = dstScale;
      inUnitsType = domapUnitsType.domapUnitsMeter;

      core = new GeneralizerCoreClass();
      core.InitSysParaTable(paraFilePath);

      core.InitGenEvironment(orgScale, dstScale, 1, inUnitsType);
      IAGeneralization aGeneralization = core as IAGeneralization;
      para = core.SystemParameter;
      hydro = aGeneralization.HydroGeneralize;
      genPolygon = aGeneralization.PolygonGeneralize;
      genSelection = aGeneralization.Selection;
      lineSimplification = aGeneralization.LineSimplification;
    }

    //TODO 修复
    public IGeometry ExtractCenterLineFromPolygon(IPolygon polygon) {
      DoPolygon inDoPolygon = new DoPolygonClass();
      IGeometryCollection geometryCol = (IGeometryCollection)polygon;

      for (int nGeometryIndex = 0; nGeometryIndex < geometryCol.GeometryCount; nGeometryIndex++) {
        if (0 == nGeometryIndex) {
          DoLine doLine = new DoLineClass();
          IRing ring = (IRing)geometryCol.get_Geometry(nGeometryIndex);
          IPointCollection pointCol = (IPointCollection)ring;
          for (int nPointIndex = 0; nPointIndex < pointCol.PointCount; nPointIndex++) {
            IPoint point = (IPoint)pointCol.get_Point(nPointIndex);
            doLine.AddRealXY(point.X, point.Y);
          }
          inDoPolygon.SetExteriorLine(doLine);
        }
        else {
          DoLine doLine = new DoLineClass();
          IRing ring = (IRing)geometryCol.get_Geometry(nGeometryIndex);
          IPointCollection pointCol = (IPointCollection)ring;
          for (int nPointIndex = 0; nPointIndex < pointCol.PointCount; nPointIndex++) {
            IPoint point = (IPoint)pointCol.get_Point(nPointIndex);
            doLine.AddRealXY(point.X, point.Y);
          }
          inDoPolygon.AddInteriorLine(doLine);
        }
      }


      DoMultiLine outDoMultiline = new DoMultiLineClass();

      hydro.ExtractAxisWithinPolygonEx(inDoPolygon, out outDoMultiline);

      IGeometryCollection geoCol = new PolylineClass();

      for (int nLineIndex = 0; nLineIndex < outDoMultiline.LineCount; nLineIndex++) {
        IPointCollection pointCol = new PathClass();
        DoLine doline = new DoLineClass();
        doline = (DoLine)outDoMultiline.get_Line(nLineIndex);
        object missing = Type.Missing;
        System.Array coords = (System.Array)doline.Coords;
        for (int nPointIndex = 0; nPointIndex < doline.PointsCount; nPointIndex++) {
          IPoint point = new PointClass();
          point.PutCoords((double)coords.GetValue(0, nPointIndex), (double)coords.GetValue(1, nPointIndex));
          pointCol.AddPoint(point, ref missing, ref missing);
        }
        IPath path = (IPath)pointCol;
        geoCol.AddGeometry((IGeometry)path, ref missing, ref missing);
      }
      IPolyline outPolyline = (IPolyline)geoCol;
      IGeometry geoPolyline = outPolyline as IGeometry;
      return geoPolyline;
    }

    //TODO 修复
    public IGeometry BuildingClusterAggregation(IGeometryCollection polygonCollection, double dAggrThreshold) {
      DoMultipoly inDoMultipoly = new DoMultipolyClass();
      IDoGeometryCollection doGeometryCollection = inDoMultipoly as IDoGeometryCollection;
      for (int i = 0; i < polygonCollection.GeometryCount; i++) {
        IPolygon inPolygon = polygonCollection.get_Geometry(i) as IPolygon;
        DoPolygon inDoPolygon = ESRISimplePolygonToDoPolygon(inPolygon as IPolygon4);
        doGeometryCollection.AddGeometry(inDoPolygon as IDoGeometry);
      }

      inDoMultipoly = doGeometryCollection as DoMultipoly;

      DoMultipoly outDoMultipoly = genPolygon.BuildingClusterAggregationEx(inDoMultipoly, dAggrThreshold);

      IDoGeometryCollection outDoGeometryCol = outDoMultipoly as IDoGeometryCollection;


      if (1 == outDoGeometryCol.GeometryCount) {
        IDoGeometry doGeometry = outDoGeometryCol.get_Geometry(0);
        DoPolygon outDoPolygon = doGeometry as DoPolygon;
        IPolygon outPolygon = DoMapPolygonToESRIPolygon(outDoPolygon);
        IGeometry geometry = new PolygonClass();
        geometry = outPolygon;
        return geometry;
      }
      else {
        System.Windows.Forms.MessageBox.Show("距离太远，无法合并!");
        return null;
      }

    }


    /// <summary>
    /// 多边形聚合（保持正交状态）
    /// </summary>
    /// <param name="polygonCollection">传入的多边形集合</param>
    /// <param name="dis">多边形合并的最小间距</param>
    /// <returns>聚合后的多边形</returns>
    public IPolygon AggregationOfPolygons(IGeometryCollection polygonCollection, double dis) {
      if (polygonCollection == null || polygonCollection.GeometryCount == 0) {
        return null;
      }
      DoMultipoly inDoMultipoly = new DoMultipolyClass();
      IDoGeometryCollection doGeometryCollection = inDoMultipoly as IDoGeometryCollection;
      for (int polygonIndex = 0; polygonIndex < polygonCollection.GeometryCount; polygonIndex++) {
        IPolygon4 inPolygon = polygonCollection.get_Geometry(polygonIndex) as IPolygon4;
        IGeometryCollection gc = inPolygon.ConnectedComponentBag as IGeometryCollection;
        for (int i = 0; i < gc.GeometryCount; i++) {
          DoPolygon inDoPolygon = ESRISimplePolygonToDoPolygon(gc.get_Geometry(i) as IPolygon4);
          doGeometryCollection.AddGeometry(inDoPolygon as IDoGeometry);
        }
      }

      inDoMultipoly = doGeometryCollection as DoMultipoly;
      DoPolygon outoutDopolygon = null;

      object t = "多边形合并的最小间距";
      //double distance = dis * 1000 / orgScale;
      core.SystemParameter.SetParaValue(ref t, dis.ToString());
      genPolygon.AggregationOfOrthogonalPolygons(inDoMultipoly, out outoutDopolygon);

      if (null == outoutDopolygon) {
        return null;
      }
      else {
        IPolygon outPolygon = DoMapPolygonToESRIPolygon(outoutDopolygon);
        return outPolygon;
      }

    }

    /// <summary>
    /// 利用三角网弯曲深度对线进行化简
    /// </summary>
    /// <param name="line">需要化简的线</param>
    /// <param name="BendDepth4Removal">被删除的弯曲深度</param>
    /// <returns>化简后的线</returns>
    public IPolyline SimplifyPolylineByDT(IPolyline line, double BendDepth4Removal) {
      if (line == null || line.IsEmpty) {
        return null;
      }
      object miss = Type.Missing;
      IGeometryCollection gc = line as IGeometryCollection;
      PolylineClass rPolyline = new PolylineClass();
      rPolyline.SpatialReference = line.SpatialReference;
      for (int i = 0; i < gc.GeometryCount; i++) {
        IPath path = gc.get_Geometry(i) as IPath;
        DoLine inDoline = ESRIPathToDomapLine(path);
        object o = "等高线删除弯曲的深度";
        //generalizerCore.SystemParameter.SetParaValue(ref o, BendDepth4Removal.ToString());

        DoLine outDoline = lineSimplification.CountorSimplification1(inDoline, BendDepth4Removal, 3, 3);
        rPolyline.AddGeometry(DomapLineToESRIPath(outDoline, false), ref miss, ref miss);
      }
      rPolyline.Simplify();
      rPolyline.SimplifyNetwork();
      return rPolyline;
    }

    /// <summary>
    /// 利用三角网弯曲深度对线进行化简
    /// </summary>
    /// <param name="path">需要化简的线，如果传入的是IRing，则返回的也是IRing</param>
    /// <param name="bendDepth4Removal">被删除的弯曲深度</param>
    /// <returns>返回的线</returns>
    public IPath SimplifyPathByDT(IPath path, double bendDepth4Removal) {
      if (path == null || path.IsEmpty) {
        return null;
      }
      DoLine inDoline = ESRIPathToDomapLine(path);
      DoLine outDoline = lineSimplification.CountorSimplification1(inDoline, bendDepth4Removal, 3, 3);
      return DomapLineToESRIPath(outDoline, path is IRing);
    }

    public IPolygon SimplifyPolygonByDT(IPolygon poly, double bendDepth4Removal) {
      if (poly == null || poly.IsEmpty) {
        return null;
      }
      object miss = Type.Missing;
      IGeometryCollection gc = poly as IGeometryCollection;
      PolygonClass rPolygon = new PolygonClass();
      rPolygon.SpatialReference = poly.SpatialReference;
      for (int i = 0; i < gc.GeometryCount; i++) {
        IRing ring = gc.get_Geometry(i) as IRing;
        IRing oRing = SimplifyPathByDT(ring, bendDepth4Removal) as IRing;
        if (oRing != null)
          rPolygon.AddGeometry(oRing, ref miss, ref miss);
      }
      rPolygon.Simplify();
      rPolygon.SimplifyEx(true, true, true);
      return rPolygon;
    }
    /// <summary>
    /// 化简建筑物边界，距离为实地距离
    /// </summary>
    /// <param name="inGeometry"></param>
    /// <param name="realDistance"></param>
    /// <returns></returns>
    public IPath SimplifyBuildingBoundaryByLiu(IPath path, double realDistance) {
      if (path == null || path.IsEmpty) {
        return null;
      }
      DoLine inDoline = this.ESRIPathToDomapLine(path);

      DoLine outDoline = lineSimplification.SimplifyBuildingLine(inDoline, realDistance);
      IPath outPath = DomapLineToESRIPath(outDoline, path is IRing);
      return outPath;
    }

    /// <summary>
    /// 化简多边形边界
    /// </summary>
    /// <param name="poly">传入的多边形</param>
    /// <param name="d">建筑物多边形化简边长阈值</param>
    /// <returns>化简后的多边形</returns>
    public IPolygon SimplifyBuildingBoundary(IPolygon poly, double d) {
      if (poly == null)
        return null;
      object parName = "建筑物多边形化简边长阈值";
      //double parvalue = d * 1000 / this.orgScale;
      para.SetParaValue(ref parName, d.ToString());
      IGeometryCollection gBag = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
      ITopologicalOperator result = null;
      for (int i = 0; i < gBag.GeometryCount; i++) {
        IPolygon childPoly = gBag.get_Geometry(i) as IPolygon;
        (childPoly as ITopologicalOperator).Simplify();
        DoPolygon dopoly = ESRISimplePolygonToDoPolygon(childPoly as IPolygon4);
        DoPolygon genDoPoly = genPolygon.BuildingSimplification(dopoly);
        IPolygon genPoly = DoMapPolygonToESRIPolygon(genDoPoly);
        if (result == null) {
          result = genPoly as ITopologicalOperator;
        }
        else {
          result = result.Union(genPoly) as ITopologicalOperator;
        }
      }
      result.Simplify();
      return result as IPolygon;
    }

    //TODO 修复
    public IGeometry SimplifyPolylineByDT2(IGeometry inGeometry, double BendDepth4Removal) {
      if (inGeometry.GeometryType == esriGeometryType.esriGeometryPolyline) {
        IPolyline inPolyline = new PolylineClass();
        inPolyline = inGeometry as IPolyline;
        DoLine inDoline = ESRIPolylineToDomapLine(inPolyline);

        object o = "边界曲线化简弯曲深度";
        core.SystemParameter.SetParaValue(ref o, BendDepth4Removal.ToString());

        DoLine outDoline = lineSimplification.CurveSimplification(inDoline);

        IPolyline outPolyline = DomapLineToESRIPolyline(outDoline);
        IGeometry outGeometry = outPolyline as IGeometry;
        return outGeometry;
      }
      else {
        System.Windows.Forms.MessageBox.Show("输入数据类型不为Polyline");
        return null;
      }
    }

    //TODO 修复
    public IGeometry CutTheBranchesOfCurve(IGeometry inGeometry, double BendWidth2Removal, double lengthRatio) {
      if (inGeometry.GeometryType == esriGeometryType.esriGeometryPolyline) {
        IPolyline inPolyline = new PolylineClass();
        inPolyline = inGeometry as IPolyline;
        DoLine inDoline = ESRIPolylineToDomapLine(inPolyline);

        DoLine outDoline = lineSimplification.CutNarrowBendOfLine(inDoline, BendWidth2Removal, lengthRatio, 0.2);

        IPolyline outPolyline = DomapLineToESRIPolyline(outDoline);
        IGeometry outGeometry = outPolyline as IGeometry;
        return outGeometry;
      }
      else {
        System.Windows.Forms.MessageBox.Show("输入数据类型不为Polyline");
        return null;
      }
    }

    //TODO 修复
    private IPolyline DomapLineToESRIPolyline(DoLine domapLine) {
      object missing = Type.Missing;
      IPointCollection pointCol = new PolylineClass();
      System.Array coords = domapLine.Coords as System.Array;
      for (int pointIndex = 0; pointIndex < domapLine.PointsCount; pointIndex++) {
        IPoint point = new PointClass();
        point.PutCoords((double)coords.GetValue(0, pointIndex), (double)coords.GetValue(1, pointIndex));
        pointCol.AddPoint(point, ref missing, ref missing);
      }
      IPolyline polyline = pointCol as IPolyline;
      return polyline;
    }

    //TODO 修复
    private DoLine ESRIPolylineToDomapLine(IPolyline inPolyline) {
      DoLine outDoline = new DoLine();
      IPointCollection pointCol = inPolyline as IPointCollection;
      for (int pointIndex = 0; pointIndex < pointCol.PointCount; pointIndex++) {
        IPoint point = pointCol.get_Point(pointIndex);
        outDoline.AddRealXY(point.X, point.Y);
      }
      return outDoline;
    }
    public IBooleanArray MultiPointSelection(IMultipoint mp, double ratio) {
      IDoMultipoint dmp = ESRIMultipointToDomapMp(mp);
      if (dmp == null)
        return null;
      return genSelection.DistributionKeepingSelection(dmp as IDoGeometryCollection, ratio);
      //return genSelection.ElevationPointSelection(mp);
    }

    IDoMultipoint ESRIMultipointToDomapMp(IMultipoint mp) {
      if (mp == null || mp.IsEmpty)
        return null;
      IDoMultipoint dmp = new DoMultipointClass();
      IGeometryCollection gc = mp as IGeometryCollection;

      for (int i = 0; i < gc.GeometryCount; i++) {
        dmp.AddPoint(ESRIPointToDomapPoint(gc.get_Geometry(i) as IPoint) as doPoint);
      }
      return dmp;
    }
    IDoPoint ESRIPointToDomapPoint(IPoint p) {
      if (p == null || p.IsEmpty)
        return null;
      IDoPoint dp = new doPointClass();
      dp.RealX = p.X;
      dp.RealY = p.Y;
      return dp;
    }
    IPoint DomapPointToESRIPoint(IDoPoint dp) {
      if (dp == null)
        return null;
      IPoint p = new PointClass();
      p.X = dp.RealX;
      p.Y = dp.RealY;
      return p;
    }

    /// <summary>
    /// 把DoLine转换为IPath
    /// </summary>
    /// <param name="domapLine">传入的DoLine</param>
    /// <param name="asRing">是否返回一个环</param>
    /// <returns>返回的IPath，如果asRing为true，返回的是一个IRing，否则为IPath</returns>
    private IPath DomapLineToESRIPath(DoLine domapLine, bool asRing) {
      if (domapLine == null || domapLine.PointsCount == 0) {
        return null;
      }
      object missing = Type.Missing;
      IPointCollection pointCol = asRing ? (new RingClass() as IPointCollection) : (new PathClass() as IPointCollection);
      System.Array coords = domapLine.Coords as System.Array;
      for (int pointIndex = 0; pointIndex < domapLine.PointsCount; pointIndex++) {
        IPoint point = new PointClass();
        point.PutCoords((double)coords.GetValue(0, pointIndex), (double)coords.GetValue(1, pointIndex));
        pointCol.AddPoint(point, ref missing, ref missing);
      }
      if (asRing) {
        (pointCol as IRing).Close();
      }
      return pointCol as IPath;
    }

    private DoLine ESRIPathToDomapLine(IPath path) {
      DoLine outDoline = new DoLine();
      IPointCollection pointCol = path as IPointCollection;
      for (int pointIndex = 0; pointIndex < pointCol.PointCount; pointIndex++) {
        IPoint point = pointCol.get_Point(pointIndex);
        outDoline.AddRealXY(point.X, point.Y);
      }
      return outDoline;
    }

    /// <summary>
    ///  从domapPolygon转换为IPolygon
    /// </summary>
    /// <param name="domapPolygon">传入的多边形</param>
    /// <returns>传出的多边形</returns>
    private IPolygon DoMapPolygonToESRIPolygon(DoPolygon domapPolygon) {
      if (domapPolygon == null) {
        return null;
      }
      IGeometryCollection geoCollection = new PolygonClass();
      object missing = Type.Missing;

      DoLine domapExteriorLine = domapPolygon.ExteriorLine;

      IPointCollection pointCollection = new RingClass();

      System.Array array = domapExteriorLine.Coords as System.Array;
      for (int itemIndex = 0; itemIndex < domapExteriorLine.PointsCount; itemIndex++) {
        IPoint point = new PointClass();
        point.PutCoords((double)array.GetValue(0, itemIndex), (double)array.GetValue(1, itemIndex));
        pointCollection.AddPoint(point, ref missing, ref missing);
      }

      IRing ring = pointCollection as IRing;
      geoCollection.AddGeometry(ring as IGeometry, ref missing, ref missing);



      for (int interiorRingIndex = 0; interiorRingIndex < domapPolygon.InteriorLineCount; interiorRingIndex++) {
        IPointCollection pointCol = new RingClass();
        DoLine domapInteriorLine = domapPolygon.get_InteriorLine(interiorRingIndex);
        array = domapInteriorLine.Coords as System.Array;
        for (int interiorRingPointIndex = 0; interiorRingPointIndex < domapInteriorLine.PointsCount; interiorRingPointIndex++) {
          IPoint point = new PointClass();
          point.PutCoords((double)array.GetValue(0, interiorRingPointIndex), (double)array.GetValue(1, interiorRingPointIndex));
          pointCol.AddPoint(point, ref missing, ref missing);
        }
        ring = pointCol as IRing;
        geoCollection.AddGeometry(ring as IGeometry, ref missing, ref missing);
      }

      IPolygon outPolygon = geoCollection as IPolygon;
      outPolygon.Close();
      return outPolygon;
    }

    /// <summary>
    /// 从简单的IPolygon 转换为DoPolygon
    /// </summary>
    /// <param name="inPolygon">传入的多边形，只能拥有一个外环</param>
    /// <returns>返回的DoPolygon</returns>
    private DoPolygon ESRISimplePolygonToDoPolygon(IPolygon4 inPolygon) {
      DoPolygon outDoPolygon = new DoPolygon();
      if (1 == inPolygon.ExteriorRingCount) {
        IRing exteriorRing = new RingClass();
        IGeometryBag exteriorRingBag = inPolygon.ExteriorRingBag;
        IEnumGeometry exteriorRingsEnum = exteriorRingBag as IEnumGeometry;
        exteriorRingsEnum.Reset();
        exteriorRing = exteriorRingsEnum.Next() as IRing;

        IPointCollection pointCol = exteriorRing as IPointCollection;

        DoLine doline = new DoLine();

        IPoint point = new PointClass();
        for (int pointIndex = 0; pointIndex < pointCol.PointCount; pointIndex++) {
          point = pointCol.get_Point(pointIndex);
          doline.AddRealXY(point.X, point.Y);
        }

        point = pointCol.get_Point(pointCol.PointCount - 1);
        doline.AddRealXY(point.X, point.Y);

        outDoPolygon.SetExteriorLine(doline);


        for (int interiorRingIndex = 0; interiorRingIndex < inPolygon.get_InteriorRingCount(exteriorRing); interiorRingIndex++) {
          IRing interiorRing = new RingClass();
          inPolygon.QueryInteriorRingsEx(exteriorRing, interiorRingIndex, out interiorRing);

          pointCol = interiorRing as IPointCollection;

          for (int pointIndex = 0; pointIndex < pointCol.PointCount; pointIndex++) {
            point = pointCol.get_Point(pointIndex);
            doline.AddRealXY(point.X, point.Y);
          }

          point = pointCol.get_Point(pointCol.PointCount - 1);
          doline.AddRealXY(point.X, point.Y);

          outDoPolygon.AddInteriorLine(doline);

        }
        return outDoPolygon;

      }
      else {
        return null;
      }

    }



  }
}

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
namespace ShellTBDivided
{
    /// <summary>
    /// 图斑剖分融合
    /// </summary>
    public class TBDivideClass0
    {
        string mSourceGDB = string.Empty;
        string mFclName = string.Empty;
        string mRoadField = "RoadID";
        string mDisField = "DisFID";
        string mGBField = "DLBM";
      
        int mLimitNum = 300;
        IQueryFilter mQuery = new QueryFilterClass();
        string mSql = "((DLBM like '1001%') or (DLBM like '1002%') or (DLBM like '1004%') or(DLBM like '1003%') or (DLBM like '1006%'))";
        public TBDivideClass0(string sourceGDB, string sql, int limit, string fclName="DLTB")
        {
            mSourceGDB = sourceGDB;
            mSql=sql;
            mLimitNum= limit;
            mFclName = fclName;
        }
        public TBDivideClass0(string sourceGDB, string sql, string fclName = "DLTB")
        {
            mSourceGDB = sourceGDB;
            mSql = sql;
            mFclName = fclName;
        }
        private void AddField(IFeatureClass fCls, string fieldName)
        {
            int index = fCls.FindField(fieldName);
            if (index != -1)
            {
                return;
            }
            IFields pFields = fCls.Fields;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.AliasName_2 = fieldName;
           // pFieldEdit.Length_2 = 1;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            IClass pTable = fCls as IClass;
            pTable.AddField(pField);
            pFieldsEdit = null;
            pField = null;
        }
        /// <summary>
        /// 预处理
        /// </summary>
        public void TBDividePre(string mExePath)
        {
            try
            {
                IQueryFilter qf = new QueryFilterClass();

                qf.WhereClause = mSql;
                IWorkspaceFactory wf = new FileGDBWorkspaceFactoryClass();
                var fs = wf.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace;
                var fclTB = (fs as IFeatureWorkspace).OpenFeatureClass(mFclName);
                AddField(fclTB, mRoadField);
                AddField(fclTB, mDisField);
                int count = 0;

                #region 预处理
                int roadIndex = fclTB.FindField(mRoadField);
                int disIndex = fclTB.FindField(mDisField);
                IFeatureCursor feCusor = fclTB.Update(null, false);
                IFeature fe = null;
                while ((fe = feCusor.NextFeature()) != null)
                {
                    fe.set_Value(disIndex, fe.OID);
                    feCusor.UpdateFeature(fe);
                }
                Marshal.ReleaseComObject(feCusor);
                feCusor = fclTB.Update(qf, false);
                while ((fe = feCusor.NextFeature()) != null)
                {
                    #region
                    fe.set_Value(roadIndex, fe.OID);
                    feCusor.UpdateFeature(fe);
                    //记录当前要素ID
                    if (fe.Shape.IsEmpty)
                    {
                        continue;
                    }
                    IPolygon pg = fe.Shape as IPolygon;
                    //计算特征指标
                    IArea pgArea = pg as IArea;
                    double area = pgArea.Area;
                    double pgLenth = (pg as IPolycurve).Length;
                    double flagArea = (pgLenth * pgLenth) / area;
                    //满足狭长指标

                    //if (flagArea >= 50 && area > 100)
                    //{
                        count++;

                   // }
                    //else
                    //{
                        //continue;
                    //}
                    #endregion
                }

                Marshal.ReleaseComObject(feCusor);
                #endregion
                XDocument doc = new XDocument();
                XElement root = new XElement("Root");
                root.SetValue(count);
                doc.Add(root);
                string tempxml = mExePath + "\\autoTBPre.xml";
                if (File.Exists(tempxml))
                {
                    File.Delete(tempxml);
                }
                doc.Save(tempxml);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 执行:剖分  融合 全部处理
        /// </summary>
        public void Excute(int start,int total)
        {
            List<int> items = new List<int>();
            IWorkspaceFactory wf1 = new FileGDBWorkspaceFactoryClass();
            var fs = wf1.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace;
            var fclTB = (wf1.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace).OpenFeatureClass(mFclName);
            int index1 = fclTB.FindField(mRoadField);
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = mSql;
            IFeatureCursor feCusor = fclTB.Update(qf, false);
            IFeature fe = null;
          
            while ((fe = feCusor.NextFeature()) != null)
            {

                fe.set_Value(index1, fe.OID);
                feCusor.UpdateFeature(fe);
                #region
                //记录当前要素ID
                if (fe.Shape.IsEmpty)
                {
                    continue;
                }

                IPolygon pg = fe.Shape as IPolygon;

                //计算特征指标
                IArea pgArea = pg as IArea;
                double area = pgArea.Area;
                double pgLenth = (pg as IPolycurve).Length;
                double flagArea = (pgLenth * pgLenth) / area;
               
                //满足狭长指标

                //if (flagArea >= 50 && area > 100)
                //{
                    items.Add(fe.OID);

                //}
                //else
                //{
                //    continue;
                //}
                #endregion
                if (items.Count > mLimitNum-1)
                    break;
            }
            Marshal.ReleaseComObject(feCusor);
            CenterLineFactory cfFac = new CenterLineFactory();
            for (int i = 0; i < items.Count; i++)
            {
                int msg = start + i + 1;
              
                try
                {
                    IFeature roadFe = fclTB.GetFeature(items[i]);
                    Console.WriteLine("当前正在处理第：" + msg + "/" + total + "，进程【" + (i + 1) + "," + items.Count + "】"+",oid:"+roadFe.OID);
                    int parentID = int.Parse(roadFe.get_Value(index1).ToString());
                    IPolygon item = roadFe.ShapeCopy as IPolygon;
                    CenterLine centerLine = cfFac.Create2(item);
                    //添加道路面
                    IGeometry geometryLine = centerLine.Line as IGeometry;
                    (geometryLine as ITopologicalOperator).Simplify();
                    IFeatureEdit splitFe = roadFe as IFeatureEdit;
                    //添加分割线,并切割
                    IPolyline topLine = CreateTopologyCutLineByCenterLine(fclTB, geometryLine as IPolyline, item, items[i]);
                    (topLine as ITopologicalOperator).Simplify();
                    IPolyline spLine = (geometryLine as ITopologicalOperator).Union(topLine) as IPolyline;
                    (spLine as ITopologicalOperator).Simplify();
                    ISet splitFeatures = splitFe.Split(spLine);
                    Marshal.ReleaseComObject(item);
                    List<IFeature> leftPolygons = new List<IFeature>();
                    if (splitFeatures != null)
                    {
                        splitFeatures.Reset();
                        while (true)
                        {
                            IFeature fesub = splitFeatures.Next() as IFeature;
                            if (fesub == null)
                            {
                                break;
                            }
                            leftPolygons.Add(fesub);
                        }

                    }
                    int leftNum = 0;
                    do
                    {

                        //融合关系  merge->subfe
                        Dictionary<int, List<int>> mergeInfos = new Dictionary<int, List<int>>();
                        List<IFeature> alonePolygons = new List<IFeature>();
                        //2.融合
                        //对狭长图斑的碎片进行融合
                        foreach (var feRoad in leftPolygons)
                        {
                            #region

                            IFeature f = FindLongestToucthFeature(feRoad.ShapeCopy, fclTB, parentID);
                            if (f == null)
                            {
                                alonePolygons.Add(feRoad);
                                continue;
                            }
                            else
                            {
                                //直接融合
                                ITopologicalOperator pto = f.ShapeCopy as ITopologicalOperator;
                                IGeometry geoTb = feRoad.ShapeCopy;
                                IGeometry result = pto.Union(geoTb);
                                if (result != null && !result.IsEmpty)
                                {
                                    (result as ITopologicalOperator).Simplify();
                                    f.Shape = result as IGeometry;
                                    f.Store();
                                    Marshal.ReleaseComObject(pto);
                                    Marshal.ReleaseComObject(geoTb);
                                    Marshal.ReleaseComObject(result);
                                    feRoad.Delete();
                                }
                              
                            }
                            #endregion
                        }
                        leftNum = leftPolygons.Count;
                        leftPolygons = new List<IFeature>();
                        leftPolygons.AddRange(alonePolygons);
                        


                    }
                    while (leftPolygons.Count > 0 && leftNum > leftPolygons.Count);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("失败："+ex.Message);
                }

            }

        }


        public void Excute2(int start, int total)
        {
            List<int> items = new List<int>();
            List<int> itemsDiv = new List<int>();
            IWorkspaceFactory wf1 = new FileGDBWorkspaceFactoryClass();
            var fs = wf1.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace;
            var fclTB = (fs as IFeatureWorkspace).OpenFeatureClass(mFclName);
            int roadIndex = fclTB.FindField(mRoadField);
            int gbIndex = fclTB.FindField(mGBField);
            int disIndex = fclTB.FindField(mDisField);
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = mSql;
            IFeatureCursor feCusor = fclTB.Search(qf, false);
            IFeature fe = null;

            while ((fe = feCusor.NextFeature()) != null)
            {
                #region
                //记录当前要素ID
                if (fe.Shape.IsEmpty)
                {
                    continue;
                }
                IPolygon pg = fe.Shape as IPolygon;
                //计算特征指标
                IArea pgArea = pg as IArea;
                double area = pgArea.Area;
                double pgLenth = (pg as IPolycurve).Length;
                double flagArea = (pgLenth * pgLenth) / area;
                //满足狭长指标
                items.Add(fe.OID);
                itemsDiv.Add(fe.OID);
                if (items.Count > mLimitNum - 1)
                    break;
                //if (flagArea >= 50 && area > 200)
                //{
                //    itemsDiv.Add(fe.OID);
                //    if (items.Count > mLimitNum - 1)
                //        break;
                //}
                //else
                //{
                //    if (items.Count > mLimitNum - 1)
                //        break;
                //    continue;

                //}
                #endregion
               
            }
            Marshal.ReleaseComObject(feCusor);
           
            CenterLineFactory cfFac = new CenterLineFactory();
            for (int i = 0; i < items.Count; i++)
            {
                int msg = start + i + 1;
               
                Console.WriteLine("当前正在剖分处理第：" + msg + "/" + total + "，进程【" + (i + 1) + "," + items.Count + "】");
                try
                {
                    IFeature roadFe = fclTB.GetFeature(items[i]);
                    List<IFeature> leftPolygons = new List<IFeature>();
                    int parentID = int.Parse(roadFe.get_Value(roadIndex).ToString());
                    //记录融合ID
                    Dictionary<int, int> disOIDs = new Dictionary<int, int>();
                    if (itemsDiv.Contains(items[i]))
                    {
                        #region 需要剖分的要素
                        IPolygon item = roadFe.ShapeCopy as IPolygon;
                        CenterLine centerLine = cfFac.Create2(item);
                        //添加道路面
                        IGeometry geometryLine = centerLine.Line as IGeometry;
                        (geometryLine as ITopologicalOperator).Simplify();
                        IFeatureEdit splitFe = roadFe as IFeatureEdit;
                        //添加分割线,并切割
                        IPolyline topLine = CreateTopologyCutLineByCenterLine(fclTB, geometryLine as IPolyline, item, items[i]);
                        (topLine as ITopologicalOperator).Simplify();
                        IPolyline spLine = (geometryLine as ITopologicalOperator).Union(topLine) as IPolyline;
                        (spLine as ITopologicalOperator).Simplify();
                        ISet splitFeatures = splitFe.Split(spLine);
                        Marshal.ReleaseComObject(item);

                      
                        if (splitFeatures != null)
                        {
                            splitFeatures.Reset();
                            while (true)
                            {
                                IFeature fesub = splitFeatures.Next() as IFeature;
                                if (fesub == null)
                                {
                                    break;
                                }
                                leftPolygons.Add(fesub);
                                disOIDs[fesub.OID] = fesub.OID;
                            }

                        }
                        #endregion
                    }
                    else
                    {
                        leftPolygons.Add(roadFe);
                        disOIDs[roadFe.OID] = roadFe.OID;
                    }
                    int leftNum = 0;
                    do
                    {

                        //融合关系  merge->subfe
                        Dictionary<int, IFeature> mergeInfos = new Dictionary<int, IFeature>();
                        List<IFeature> alonePolygons = new List<IFeature>();
                        //1.查找 
                        foreach (var feRoad in leftPolygons)
                        {
                            #region

                            IFeature f = FindLongestToucthFeature(feRoad.ShapeCopy, fclTB, parentID);
                            if (f == null)
                            {
                                alonePolygons.Add(feRoad);
                                continue;
                            }
                            else
                            {
                                mergeInfos[feRoad.OID] = f;
                                 
                            }
                            #endregion
                        }
                        //2.融合
                        foreach (var kv in mergeInfos)
                        {
                            //设置融合字段
                            IFeature f = kv.Value;
                            int disOID = int.Parse(f.get_Value(disIndex).ToString());
                            IQueryFilter mQuery = new QueryFilterClass();
                            qf.WhereClause = "ObjectID = " + kv.Key;
                            IFeature feature;
                            IFeatureCursor fcursor = fclTB.Update(qf, false);
                            while ((feature = fcursor.NextFeature()) != null)
                            {
                                feature.set_Value(gbIndex, f.get_Value(gbIndex));
                                feature.set_Value(roadIndex, DBNull.Value);
                                feature.set_Value(disIndex, disOID);
                                fcursor.UpdateFeature(feature);

                            }
                            Marshal.ReleaseComObject(fcursor);
                        }
                        leftNum = leftPolygons.Count;
                        leftPolygons = new List<IFeature>();
                        leftPolygons.AddRange(alonePolygons);
                    }
                    while (leftPolygons.Count > 0 && leftNum > leftPolygons.Count);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("失败：" + ex.Message);
                }

            }

        }


      


        
       
        private IGeometry unionGeometry(IFeature big, List<int> smallfes, IFeatureClass pfclTb)
        {
            IGeometry pgeo = null;
            IGeometryCollection pgeoBags = null;
            ITopologicalOperator pto = null;
            int bigFeID = big.OID;
            try
            {
                pgeoBags = new GeometryBagClass();
                foreach (var feid in smallfes)
                {
                    pgeoBags.AddGeometry(pfclTb.GetFeature(feid).ShapeCopy);
                }
                pgeoBags.AddGeometry(big.ShapeCopy);
                IPolygon pPolygon = new PolygonClass();
                pto = pPolygon as ITopologicalOperator;
                pto.ConstructUnion(pgeoBags as IEnumGeometry);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pgeoBags);
                return pto as IGeometry;
            }
            catch (Exception ex)//可能报错Construct
            {
                Console.WriteLine("合并几何失败：" + ex.Message);
                try
                {
                    pto = big.ShapeCopy as ITopologicalOperator;
                    foreach (var feid in smallfes)
                    {
                        pto = pto.Union(pfclTb.GetFeature(feid).ShapeCopy) as ITopologicalOperator;

                    }
                    return pto as IGeometry;
                }
                catch (Exception ex1)
                {

                }
            }
            return null;
        }
        /// <summary>
        /// 查找选中共享边最长的要素：不包括自身(切割的图斑不融于自身)
        /// </summary>
        /// <param name="pg"></param>
        /// <param name="fc"></param>
        /// <returns></returns>
        private IFeature FindLongestToucthFeature(IGeometry pg, IFeatureClass fc, int feid)
        {
            try
            {
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = pg;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
               // sf.WhereClause = mRoadField + " <>" + feid + " or " + mRoadField + " is NULL";
                sf.WhereClause =mRoadField + " is NULL";//非道路图斑

                IFeatureCursor fcursor = fc.Search(sf, false);
                IFeature fe = null;
                double maxLength = 0;
                int rID = -1;
                Dictionary<int, IFeature> neighbors = new Dictionary<int, IFeature>();
                while ((fe = fcursor.NextFeature()) != null)
                {
                    neighbors.Add(fe.OID, fe);
                }
                Marshal.ReleaseComObject(fcursor);

                if (neighbors.Count == 1)
                {
                    rID = neighbors.Keys.First();
                }
                else
                {
                    foreach (var kv in neighbors)
                    {
                        double length = LengthOfNeighborPolygon(pg, kv.Value.Shape as IPolygon);
                        if (length > maxLength)
                        {
                            maxLength = length;
                            rID = kv.Key;
                        }
                    }
                }
                Marshal.ReleaseComObject(pg);
                if (rID != -1)
                {
                    return fc.GetFeature(rID);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }
        private double LengthOfNeighborPolygon(IGeometry poly1, IPolygon poly2)
        {
            ITopologicalOperator4 topoOper = poly1 as ITopologicalOperator4;
            IGeometry geo = null;
            try
            {
                geo = topoOper.Intersect(poly2, esriGeometryDimension.esriGeometry1Dimension);
            }
            catch (Exception)
            {
                try
                {
                    topoOper.Simplify();
                    (poly2 as ITopologicalOperator).Simplify();
                    geo = topoOper.Intersect(poly2, esriGeometryDimension.esriGeometry1Dimension);
                }
                catch (Exception)
                {
                    return -2;
                }
            }

            if (geo == null || geo.IsEmpty)
            {
                return -2;
            }
            double len = (geo as IPolyline).Length;
            Marshal.ReleaseComObject(geo);
            return len;

        }

        private IPolyline CreateTopologyCutLineByCenterLine(IFeatureClass fcPolygon, IPolyline centerPolyline, IPolygon polygon, int tbID)
        {

            IPolyline cutLine = new PolylineClass();
            cutLine.SpatialReference = polygon.SpatialReference;

            //找到与多边形相交的所有共享边上的节点（起始节点）
            List<IPoint> toucthPoints = FindToucthFeatureNodes(tbID, polygon, fcPolygon);

            for (int i = 0; i < toucthPoints.Count; i++)
            {
                IPoint point = toucthPoints[i];
                IPoint outPoint = new PointClass();
                double alongDistance = 0;
                double fromDistance = 0;
                bool rightSide = false;

                //获取拓扑节点(point)到中轴线的最近点(outpoint)
                centerPolyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, point, true, outPoint, ref alongDistance, ref fromDistance, ref rightSide);

                PathClass path = new PathClass();
                path.AddPoints(1, ref point);
                path.AddPoints(1, ref outPoint);

                IGeometry geo = path;
                (cutLine as IGeometryCollection).AddGeometries(1, ref geo);

                //处理图边界
                //延长线
                IPoint oPoint = new PointClass();
                oPoint.X = outPoint.X + (outPoint.X - point.X);
                oPoint.Y = outPoint.Y + (outPoint.Y - point.Y);
                IPoint otherPoint = new PointClass();
                polygon.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, oPoint, true, otherPoint, ref alongDistance, ref fromDistance, ref rightSide);

                if (otherPoint.Compare(point) != 0)
                {
                    PathClass otherPath = new PathClass();
                    otherPath.AddPoints(1, ref outPoint);
                    otherPath.AddPoints(1, ref otherPoint);
                    geo = otherPath;
                    (cutLine as IGeometryCollection).AddGeometries(1, ref geo);
                }
            }
            return cutLine;
        }
        private List<IPoint> FindToucthFeatureNodes(int feID, IGeometry polygon, IFeatureClass fc)
        {


            ISpatialFilter qf = new SpatialFilterClass();
            Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
            IFeature fe;
            qf.Geometry = polygon;
            qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            qf.WhereClause = fc.OIDFieldName + " <> " + feID;
            IFeatureCursor cursor = fc.Search(qf, false);
            List<IPolyline> listEdges = new List<IPolyline>();
            while ((fe = cursor.NextFeature()) != null)
            {
                IGeometry result = (polygon as ITopologicalOperator).Intersect(fe.Shape, esriGeometryDimension.esriGeometry1Dimension);
                if (result != null && !result.IsEmpty)
                {
                    IGeometryCollection gc = result as IGeometryCollection;
                    if (gc.GeometryCount > 1)
                    {
                        for (int i = 0; i < gc.GeometryCount; i++)
                        {
                            PolylineClass polyline = new PolylineClass();
                            polyline.AddGeometry(gc.get_Geometry(i));
                            polyline.Simplify();
                            listEdges.Add(polyline as IPolyline);
                        }
                    }
                    else
                    {
                        listEdges.Add(result as IPolyline);
                    }
                    //listEdges.Add(result as IPolyline);
                }
            }
            Marshal.ReleaseComObject(cursor);
            List<IPoint> nodeList = new List<IPoint>();
            try
            {
                //当前要素的所有边
                foreach (var edge in listEdges)
                {
                    #region
                    IGeometry geo = edge;
                    bool bEqual = false;
                    //from point
                    foreach (IPoint p in nodeList)
                    {
                        IRelationalOperator re = p as IRelationalOperator;
                        if (re.Equals((geo as IPolyline).FromPoint))
                        {
                            bEqual = true;
                            break;
                        }
                    }
                    if (!bEqual)
                        nodeList.Add((geo as IPolyline).FromPoint);
                    //to point
                    bEqual = false;
                    foreach (IPoint p in nodeList)
                    {
                        IRelationalOperator re = p as IRelationalOperator;

                        if (re.Equals((geo as IPolyline).ToPoint))
                        {
                            bEqual = true;
                            break;
                        }
                    }
                    if (!bEqual)
                        nodeList.Add((geo as IPolyline).ToPoint);

                    #endregion
                }

            }
            catch (Exception ex)
            {
                return nodeList;
            }
            return nodeList;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;
namespace SMGI.Plugin.GeneralEdit
{
    public class FeatureTopoExec : SMGICommand
    {
        public FeatureTopoExec()
        {
            m_caption = "对象间拓扑处理";
        }
        List<IFeature> selFeasPoint = new List<IFeature>();//点
        List<IFeature> selFeasPolyline = new List<IFeature>();//线
        List<IFeature> selFeasPolygon = new List<IFeature>();//面
        List<IFeature> selFeas = new List<IFeature>();
        private double tolerance = 0.01;//容差
        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    if (0 == m_Application.MapControl.Map.SelectionCount)
                        return false;


                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = mapEnumFeature.Next();

                    bool res = false;
                    int nSelLine = 0;
                    while (feature != null)
                    {
                        nSelLine++;
                        feature = mapEnumFeature.Next();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
                    if (nSelLine > 1)
                    {
                        res = true;
                    }
                    return res;
                }
                else
                    return false;
            }
        }


        public override void OnClick()
        {
            selFeasPoint.Clear();
            selFeasPolyline.Clear();
            selFeasPolygon.Clear();
            selFeas.Clear();
            #region 收集所有被选择的要素

            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();
            IFeature feature = null;
            while ((feature = mapEnumFeature.Next()) != null)
            {
                if (feature.Shape is IPoint)
                {
                    selFeasPoint.Add(feature);
                    selFeas.Add(feature);
                }
                else if (feature.Shape is IPolyline)
                {
                    selFeasPolyline.Add(feature);
                    selFeas.Add(feature);
                }
                else if (feature.Shape is IPolygon)
                {
                    selFeasPolygon.Add(feature);
                    selFeas.Add(feature);
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
            #endregion
            m_Application.EngineEditor.StartOperation();
            #region 处理点--点优先捕捉到线其次是面边线

            if (selFeasPoint.Count > 0)
            {
                foreach (var itemPt in selFeasPoint)
                {
                    ITopologicalOperator topo = itemPt.ShapeCopy as ITopologicalOperator;
                    IGeometry geometry = topo.Buffer(tolerance);
                    IRelationalOperator relationalOperatorPt = geometry as IRelationalOperator;//好接口

                    if (selFeasPolyline.Count > 0)
                    {
                        foreach (var itemPy in selFeasPolyline)
                        {
                            if (!relationalOperatorPt.Disjoint(itemPy.Shape))//相交
                            {
                                IPolyline temppy = itemPy.Shape as IPolyline;
                                IPoint pt = vertic(temppy, itemPt.Shape as IPoint);
                                itemPt.Shape = pt;
                                itemPt.Store();
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var itemPg in selFeasPolygon)
                        {
                            if (relationalOperatorPt.Disjoint(itemPg.Shape))//相交
                            {
                                ITopologicalOperator topoPg = itemPg.ShapeCopy as ITopologicalOperator;
                                IPolyline temppy = topoPg.Boundary as IPolyline;
                                IPoint pt = vertic(temppy, itemPt.Shape as IPoint);
                                itemPt.Shape = pt;
                                itemPt.Store();
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            #region 处理线--线与线处理悬挂点，线与面有两个交点则捕捉到面边线
            if (selFeasPolyline.Count > 1 || (selFeasPolyline.Count > 0 && selFeasPolygon.Count > 0))
            {
                foreach (var itemPt in selFeasPolyline)
                {
                    IPolyline polyline = itemPt.ShapeCopy as IPolyline;
                    if (selFeasPolyline.Count > 1)
                    {//悬挂点--若不及则延伸，若超过则打断并删除短的
                        ITopologicalOperator topoF = polyline.FromPoint as ITopologicalOperator;//起点
                        IGeometry geometry = topoF.Buffer(tolerance);
                        IRelationalOperator relationalOperatorPt = geometry as IRelationalOperator;//好接口

                        ITopologicalOperator topoT = polyline.ToPoint as ITopologicalOperator;//终点
                        IGeometry geometryT = topoT.Buffer(tolerance);
                        IRelationalOperator relationalOperatorPtT = geometryT as IRelationalOperator;//好接口

                        IRelationalOperator relationalOperatorPtL = polyline as IRelationalOperator;//好接口

                        foreach (var itemPy in selFeasPolyline)
                        {
                            if (itemPt.OID == itemPy.OID) { continue; }
                            if (!relationalOperatorPt.Disjoint(itemPy.Shape))//相交
                            {
                                if (!relationalOperatorPtL.Disjoint(itemPy.Shape))//相交
                                {
                                    ITopologicalOperator2 pTopo = (ITopologicalOperator2)polyline;
                                    IGeometry InterGeo = pTopo.Intersect(itemPy.Shape, esriGeometryDimension.esriGeometry0Dimension);
                                    IGeometryCollection geoCol = InterGeo as IGeometryCollection;
                                    IPoint splitPt1 = geoCol.get_Geometry(0) as IPoint;
                                    IFeatureEdit feEdit = (IFeatureEdit)itemPt;
                                    try
                                    {
                                        var feSet = feEdit.Split(splitPt1);
                                        if (feSet != null)
                                        {
                                            feSet.Reset();
                                            while (true)
                                            {
                                                IFeature fe = feSet.Next() as IFeature;
                                                if (fe == null)
                                                {
                                                    break;
                                                }

                                                if ((fe.Shape as IPolyline).Length < 1)
                                                {
                                                    fe.Delete();
                                                    break;
                                                }

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { System.Diagnostics.Trace.WriteLine(ex.Message); } break;
                                }
                                else
                                {
                                    IPolyline temppy = itemPy.Shape as IPolyline;
                                    IPoint pt = vertic(temppy, polyline.FromPoint as IPoint);
                                    IPolyline tempp = polyineex(polyline, pt, true);
                                    itemPt.Shape = tempp;
                                    itemPt.Store(); break;
                                }

                            }
                            else if (!relationalOperatorPtT.Disjoint(itemPy.Shape))//相交
                            {
                                if (!relationalOperatorPtL.Disjoint(itemPy.Shape))//相交
                                {
                                    ITopologicalOperator2 pTopo = (ITopologicalOperator2)polyline;
                                    IGeometry InterGeo = pTopo.Intersect(itemPy.Shape, esriGeometryDimension.esriGeometry0Dimension);
                                    IGeometryCollection geoCol = InterGeo as IGeometryCollection;
                                    IPoint splitPt1 = geoCol.get_Geometry(0) as IPoint;
                                    IFeatureEdit feEdit = (IFeatureEdit)itemPt;
                                    try
                                    {
                                        var feSet = feEdit.Split(splitPt1);
                                        if (feSet != null)
                                        {
                                            feSet.Reset();
                                            while (true)
                                            {
                                                IFeature fe = feSet.Next() as IFeature;
                                                if (fe == null)
                                                {
                                                    break;
                                                }

                                                if ((fe.Shape as IPolyline).Length < 1)
                                                {
                                                    fe.Delete();
                                                    break;
                                                }

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    { System.Diagnostics.Trace.WriteLine(ex.Message); } break;
                                }
                                else
                                {
                                    IPolyline temppy = itemPy.Shape as IPolyline;
                                    IPoint pt = vertic(temppy, polyline.ToPoint as IPoint);
                                    IPolyline tempp = polyineex(polyline, pt, false);
                                    itemPt.Shape = tempp;
                                    itemPt.Store();
                                    break;
                                }
                            }
                        }
                    }
                    if (selFeasPolygon.Count > 0)
                    {
                        // 线捕捉到面边线
                        foreach (var itemPg in selFeasPolygon)
                        {
                            IRelationalOperator relationalOperatorPtL = polyline as IRelationalOperator;//好接口
                            //ITopologicalOperator topoPg = itemPg.ShapeCopy as ITopologicalOperator;
                            //IPolyline tempp = topoPg.Boundary as IPolyline;
                            if (!relationalOperatorPtL.Disjoint(itemPg.Shape))//相交
                            {
                                IPolyline temppy = polygonclip(itemPg.ShapeCopy as IPolygon, polyline);
                                IPointCollection reshapePath = new PathClass();
                                reshapePath.AddPointCollection(temppy as IPointCollection);
                                try
                                {
                                    bool flag = polyline.Reshape(reshapePath as IPath);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                                (polyline as ITopologicalOperator).Simplify();
                                itemPt.Shape = polyline;
                                itemPt.Store();
                                break;
                            }
                        }
                    }
                }
            }
            #endregion
            #region 处理面--面与面-面缝隙-面相交
            //面缝隙
            if (selFeasPolygon.Count > 1)
            {//合并
                IPolygon tempunion = selFeasPolygon[0].ShapeCopy as IPolygon;
                bool flag = false;
                IPolygon temp = tempunion;
                for (int i = 1; i < selFeasPolygon.Count; i++)
                {
                    temp = union(temp, selFeasPolygon[i].ShapeCopy as IPolygon);
                }
                IPolygon4 IPolyon = temp as IPolygon4;
                IGeometryBag IGeometryBag = IPolyon.ExteriorRingBag;
                IEnumGeometry geobag = IGeometryBag as IEnumGeometry;
                geobag.Reset();
                IRing RING = null;
                IRing ring = null;
                while ((RING = geobag.Next() as IRing) != null)
                {
                    int bag = IPolyon.get_InteriorRingCount(RING);
                    if (bag > 0)
                    {
                        IPolyon.QueryInteriorRings(RING, ref ring);
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    ring.ReverseOrientation();
                    IPolygon IPolygon = ringaspolygon(ring);
                    IPolygon polygon = union(selFeasPolygon[0].ShapeCopy as IPolygon, IPolygon);
                    selFeasPolygon[0].Shape = polygon;
                    selFeasPolygon[0].Store();
                }
            }
            //面相交
            if (selFeasPolygon.Count > 1)
            {//
                for (int i = 0; i < selFeasPolygon.Count; i++)
                {
                    IFeature fea = selFeasPolygon[i];
                    IPolygon objPolygon = fea.ShapeCopy as IPolygon;
                    IRelationalOperator relationalOperatorPgon = objPolygon as IRelationalOperator;//好接口
                    for (int j = 0; j < selFeasPolygon.Count; j++)
                    {
                        IFeature tempfea = selFeasPolygon[j];
                        IPolygon tempPolygon = tempfea.ShapeCopy as IPolygon;
                        if (relationalOperatorPgon.Equals(tempPolygon)) //跳过本身
                        { continue; }
                        if (relationalOperatorPgon.Overlaps(tempPolygon)) //要素重叠
                        {
                            IPolygon temp = union(objPolygon, tempPolygon);
                            IPolygon cut = (temp as ITopologicalOperator).Difference(objPolygon) as IPolygon;
                            tempfea.Shape = cut;
                            tempfea.Store();
                        }
                    }

                }
            }
            #endregion
            m_Application.EngineEditor.StopOperation("对象间拓扑处理");
            m_Application.ActiveView.Refresh();

        }
        /// <summary>
        /// polyline上离输入点最近的点
        /// </summary>
        /// <param name="SelectedFeature"></param>
        /// <param name="boulptcoll"></param>
        /// <param name="hydlp"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public IPoint vertic(IPolyline polyline, IPoint hydlp)
        {
            IPoint boulp = new PointClass();
            double distancealong = 0;
            double distanceForm = 0;
            bool right = false;
            polyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, hydlp, false, boulp, ref distancealong, ref distanceForm, ref right);
            return boulp;
        }
        /// <summary>
        /// 线插入点
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="hydlp"></param>
        /// <returns></returns>
        public IPolyline polyineex(IPolyline polyline, IPoint hydlp, bool startPt)
        {
            IPointCollection pointCollection = polyline as IPointCollection;

            if (startPt)
            {
                IPointCollection pCollection = new PathClass();
                pCollection.AddPoint(hydlp);
                pointCollection.InsertPointCollection(0, pCollection);

            }
            else
            {
                pointCollection.AddPoint(hydlp);
            }
            ITopologicalOperator2 ptopo = polyline as ITopologicalOperator2;
            ptopo.IsKnownSimple_2 = false;
            ptopo.Simplify();
            return polyline;
        }
        /// <summary>
        /// polygon与polyline交线
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public IPolyline polygonclip(IPolygon polygon, IPolyline polyline)
        {
            IPolyline pl = new PolylineClass();
            IPointCollection pCollectionpl = pl as IPointCollection;
            IGeometry intersPoints = (polygon as ITopologicalOperator).Intersect(polyline, esriGeometryDimension.esriGeometry1Dimension);
            IGeometryCollection intersPointCol = intersPoints as IGeometryCollection;

            for (int i = 0; i < intersPointCol.GeometryCount; i++)
            {
                //  IPolyline intersPt = intersPointCol.get_Geometry(i) as IPolyline;
                IPointCollection pyCollectionpll = intersPointCol.get_Geometry(i) as IPointCollection;
                //  IPointCollection pyCollectionpl = intersPt as IPointCollection;
                pCollectionpl.AddPointCollection(pyCollectionpll);
            }
            (pl as ITopologicalOperator).Simplify();

            return pl;
        }
        public IPolygon union(IPolygon basefeature, IPolygon objectfeature)
        {
            ITopologicalOperator feaTopo = basefeature as ITopologicalOperator;
            IGeometry geo = feaTopo.Union(objectfeature);
            feaTopo.Simplify();
            return geo as IPolygon;
        }
        /// <summary>
        /// 环转面
        /// </summary>
        /// <param name="ring"></param>
        /// <returns></returns>
        public IPolygon ringaspolygon(IRing ring)
        {
            ISegmentCollection segol = ring as ISegmentCollection;
            IPolygon polygon = new PolygonClass();
            ISegmentCollection segolcl = polygon as ISegmentCollection;
            segolcl.AddSegmentCollection(segol);

            ITopologicalOperator feaTopo = polygon as ITopologicalOperator;
            feaTopo.Simplify();
            return polygon;
        }
        /// <summary>
        /// 面合并
        /// </summary>
        /// <param name="basefeature"></param>
        /// <param name="objectfeature"></param>
        public void union(IFeature basefeature, IFeature objectfeature)
        {
            IGeometry geo = objectfeature.ShapeCopy;
            ITopologicalOperator feaTopo = geo as ITopologicalOperator;
            geo = feaTopo.Union(basefeature.ShapeCopy);
            feaTopo.Simplify();
            basefeature.Shape = geo;
            objectfeature.Delete();
            basefeature.Store();
        }
    }
}

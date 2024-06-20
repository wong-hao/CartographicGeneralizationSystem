using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.GeneralEdit
{
    public class LayerFeatureTopo : SMGI.Common.SMGICommand
    {
        IFeatureWorkspace featureWorkspace;
        double tolerance;
        Dictionary<string, int> layerlevel = new Dictionary<string, int>();
        Dictionary<IFeature, int> tempfeaturepy = new Dictionary<IFeature, int>();
        Dictionary<IFeature, int> tempfeaturepn = new Dictionary<IFeature, int>();
        Dictionary<IFeature, int> tempfeature = new Dictionary<IFeature, int>();
        public LayerFeatureTopo()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        public override void OnClick()
        {

            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;
            string mdbpath = m_Application.Template.Root + @"\LayerToporule.mdb";
            string mdbname = "LayerTopoLevel";
            DataTable ruleDtCOM = ReadToDataTable(mdbpath, mdbname);//通用
            LayerFeatureTopoForm frmResult = new LayerFeatureTopoForm(ruleDtCOM, m_Application);
            frmResult.ShowDialog();
            m_Application.EngineEditor.StartOperation();
            if (frmResult.DialogResult != DialogResult.OK) { return; }
            tolerance = double.Parse(frmResult.tolerance);
            layerlevel = frmResult.layerlevel;
            using (var wo = m_Application.SetBusy())
            {
                foreach (var item in layerlevel)
                {
                    IFeatureClass FeatureClass = featureWorkspace.OpenFeatureClass(item.Key);
                    wo.SetText("正在处理【" + FeatureClass.AliasName + "】......");
                    if (FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                    {
                        pointprocess(FeatureClass, item.Value);
                    }
                    else if (FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        polylineprocess(FeatureClass, item.Value);
                    }
                    else if (FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        polygonprocess(FeatureClass, item.Value);
                    }
                }
            }
            m_Application.EngineEditor.StopOperation("图层间拓扑处理");
            m_Application.ActiveView.Refresh();
        }
        /// <summary>
        /// 点处理
        /// </summary>
        /// <param name="FeatureClass"></param>
        public void pointprocess(IFeatureClass FeatureClass, int level)
        {
            IFeatureCursor boulFCCursor = FeatureClass.Search(null, false);
            IFeature SelectedFeature = null;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            while ((SelectedFeature = boulFCCursor.NextFeature()) != null)
            {
                tempfeature.Clear();
                tempfeaturepn.Clear();
                tempfeaturepy.Clear();
                IPoint point = SelectedFeature.ShapeCopy as IPoint;
                foreach (var item in layerlevel)
                {
                    if (item.Key == FeatureClass.AliasName) { continue; }
                    IFeatureClass tempFeatureClass = featureWorkspace.OpenFeatureClass(item.Key);
                    if (tempFeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        sf.Geometry = (point as ITopologicalOperator).Buffer(tolerance);
                        IFeature maskedFeature = null;
                        IFeatureCursor maskedCursor = tempFeatureClass.Search(sf, false);
                        while ((maskedFeature = maskedCursor.NextFeature()) != null)
                        {
                            tempfeaturepy.Add(maskedFeature, item.Value);
                        }
                        Marshal.ReleaseComObject(maskedCursor);
                    }
                    else if (tempFeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        sf.Geometry = (point as ITopologicalOperator).Buffer(tolerance);
                        IFeature maskedFeature = null;
                        IFeatureCursor maskedCursor = tempFeatureClass.Search(sf, false);
                        while ((maskedFeature = maskedCursor.NextFeature()) != null)
                        {
                            tempfeaturepn.Add(maskedFeature, item.Value);
                        }
                        Marshal.ReleaseComObject(maskedCursor);
                    }
                }
                bool flag = pointpolylineprocess(SelectedFeature, level);//若点没有捕捉到线，才考虑捕捉到面边线
                if (!flag || tempfeaturepy.Count == 0) { pointpolygonprocess(SelectedFeature, level); }
            }
            Marshal.ReleaseComObject(boulFCCursor);
        }
        /// <summary>
        /// 点捕捉到线
        /// </summary>
        /// <param name="FeatureClass"></param>
        public bool pointpolylineprocess(IFeature SelectedFeature, int level)
        {
            IPoint point = SelectedFeature.ShapeCopy as IPoint;
            bool falg = false;
            if (tempfeaturepy.Count == 0) { return falg; }
            Dictionary<IFeature, int> temppy = Min(tempfeaturepy);
            foreach (var item in temppy)
            {
                if (item.Value > level) { continue; }//若点级别高于线级别则跳过
                IPolyline temppoly = item.Key.Shape as IPolyline;
                IPoint pt = vertic(temppoly, point);
                SelectedFeature.Shape = pt;
                SelectedFeature.Store();
                falg = true;
                break;
            }
            return falg;
        }
        /// <summary>
        /// 点捕捉到面边线
        /// </summary>
        /// <param name="FeatureClass"></param>
        public bool pointpolygonprocess(IFeature SelectedFeature, int level)
        {
            IPoint point = SelectedFeature.ShapeCopy as IPoint;
            bool falg = false;
            if (tempfeaturepn.Count == 0) { return falg; }
            Dictionary<IFeature, int> temppy = Min(tempfeaturepn);
            foreach (var item in temppy)
            {
                if (item.Value > level) { continue; }//若点级别高于线级别则跳过

                ITopologicalOperator topoPg = item.Key.ShapeCopy as ITopologicalOperator;
                IPolyline temppoly = topoPg.Boundary as IPolyline;
                IPoint pt = vertic(temppoly, point);
                SelectedFeature.Shape = pt;
                SelectedFeature.Store();
                falg = true;
                break;
            }
            return falg;
        }
        /// <summary>
        /// 线处理
        /// </summary>
        /// <param name="FeatureClass"></param>
        /// <param name="level"></param>
        public void polylineprocess(IFeatureClass FeatureClass, int level)
        {
            IFeatureCursor boulFCCursor = FeatureClass.Search(null, false);
            IFeature SelectedFeature = null;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            while ((SelectedFeature = boulFCCursor.NextFeature()) != null)
            {
                tempfeature.Clear();
                tempfeaturepn.Clear();
                tempfeaturepy.Clear();
                IPolyline point = SelectedFeature.ShapeCopy as IPolyline;
                foreach (var item in layerlevel)
                {

                    IFeatureClass tempFeatureClass = featureWorkspace.OpenFeatureClass(item.Key);
                    if (tempFeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        sf.Geometry = (point as ITopologicalOperator).Buffer(tolerance);
                        IFeature maskedFeature = null;
                        IFeatureCursor maskedCursor = tempFeatureClass.Search(sf, false);
                        while ((maskedFeature = maskedCursor.NextFeature()) != null)
                        {
                            if (item.Key == FeatureClass.AliasName && SelectedFeature.OID == maskedFeature.OID) { continue; }
                            tempfeaturepy.Add(maskedFeature, item.Value);
                        }
                        Marshal.ReleaseComObject(maskedCursor);
                    }
                    else if (tempFeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        sf.Geometry = (point as ITopologicalOperator).Buffer(tolerance);
                        IFeature maskedFeature = null;
                        IFeatureCursor maskedCursor = tempFeatureClass.Search(sf, false);
                        while ((maskedFeature = maskedCursor.NextFeature()) != null)
                        {
                            tempfeaturepn.Add(maskedFeature, item.Value);
                        }
                        Marshal.ReleaseComObject(maskedCursor);
                    }
                }
                bool flag = polylinepolylineprocess(SelectedFeature, level);//
                polylinepolygonprocess(SelectedFeature, level);
            }
            Marshal.ReleaseComObject(boulFCCursor);

        }
        /// <summary>
        /// 线悬挂点处理
        /// </summary>
        /// <param name="SelectedFeature"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool polylinepolylineprocess(IFeature SelectedFeature, int level)
        {
            bool falg = false;
            IPolyline polyline = SelectedFeature.ShapeCopy as IPolyline;
            if (tempfeaturepy.Count == 0) { return falg; }
            Dictionary<IFeature, int> temppy = Min(tempfeaturepy);
            ITopologicalOperator topoF = polyline.FromPoint as ITopologicalOperator;//起点
            IGeometry geometry = topoF.Buffer(tolerance);
            IRelationalOperator relationalOperatorPt = geometry as IRelationalOperator;//好接口

            ITopologicalOperator topoT = polyline.ToPoint as ITopologicalOperator;//终点
            IGeometry geometryT = topoT.Buffer(tolerance);
            IRelationalOperator relationalOperatorPtT = geometryT as IRelationalOperator;//好接口

            IRelationalOperator relationalOperatorPtL = polyline as IRelationalOperator;//好接口


            foreach (var itemPy in temppy)
            {
                if (itemPy.Value > level) { continue; }//若线级别高于线级别则跳过
                IPolyline temppoly = itemPy.Key.Shape as IPolyline;

                if (!relationalOperatorPt.Disjoint(itemPy.Key.Shape))//相交
                {
                    if (!relationalOperatorPtL.Disjoint(itemPy.Key.Shape))//相交
                    {
                        ITopologicalOperator2 pTopo = (ITopologicalOperator2)polyline;
                        IGeometry InterGeo = pTopo.Intersect(itemPy.Key.Shape, esriGeometryDimension.esriGeometry0Dimension);
                        IGeometryCollection geoCol = InterGeo as IGeometryCollection;
                        IPoint splitPt1 = geoCol.get_Geometry(0) as IPoint;
                        IFeatureEdit feEdit = (IFeatureEdit)SelectedFeature;
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
                        IPolyline temppyy = itemPy.Key.Shape as IPolyline;
                        IPoint pt = vertic(temppyy, polyline.FromPoint as IPoint);
                        IPolyline tempp = polyineex(polyline, pt, true);
                        SelectedFeature.Shape = tempp;
                        SelectedFeature.Store(); break;
                    }

                }
                else if (!relationalOperatorPtT.Disjoint(itemPy.Key.Shape))//相交
                {
                    if (!relationalOperatorPtL.Disjoint(itemPy.Key.Shape))//相交
                    {
                        ITopologicalOperator2 pTopo = (ITopologicalOperator2)polyline;
                        IGeometry InterGeo = pTopo.Intersect(itemPy.Key.Shape, esriGeometryDimension.esriGeometry0Dimension);
                        IGeometryCollection geoCol = InterGeo as IGeometryCollection;
                        IPoint splitPt1 = geoCol.get_Geometry(0) as IPoint;
                        IFeatureEdit feEdit = (IFeatureEdit)SelectedFeature;
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
                        IPolyline temppyy = itemPy.Key.Shape as IPolyline;
                        IPoint pt = vertic(temppyy, polyline.ToPoint as IPoint);
                        IPolyline tempp = polyineex(polyline, pt, false);
                        SelectedFeature.Shape = tempp;
                        SelectedFeature.Store();
                        break;
                    }
                }
                falg = true;
                break;
            }
            return falg;
        }
        /// <summary>
        /// 线捕捉到面边线
        /// </summary>
        /// <param name="SelectedFeature"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool polylinepolygonprocess(IFeature SelectedFeature, int level)
        {
            bool falg = false;
            IPolyline polyline = SelectedFeature.ShapeCopy as IPolyline;
            if (tempfeaturepn.Count == 0) { return falg; }
            Dictionary<IFeature, int> temppy = Min(tempfeaturepn);
            foreach (var itemPy in temppy)
            {
                IPolyline temppyy = polygonclip(itemPy.Key.ShapeCopy as IPolygon, polyline);
                IPointCollection reshapePath = new PathClass();
                reshapePath.AddPointCollection(temppyy as IPointCollection);
                try
                {
                    bool flag = polyline.Reshape(reshapePath as IPath);
                }
                catch (Exception)
                {
                    continue;
                }
                (polyline as ITopologicalOperator).Simplify();
                SelectedFeature.Shape = polyline;
                SelectedFeature.Store();
                break;
            }
            return falg;
        } /// <summary>

        ///面处理=面缝隙（仅处理相邻面缝隙情况）、面相交
        public void polygonprocess(IFeatureClass FeatureClass, int level)
        {
            IFeatureCursor boulFCCursor = FeatureClass.Search(null, false);
            IFeature SelectedFeature = null;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            while ((SelectedFeature = boulFCCursor.NextFeature()) != null)
            {
                tempfeature.Clear();
                tempfeaturepn.Clear();
                tempfeaturepy.Clear();
                IPolygon point = SelectedFeature.ShapeCopy as IPolygon;
                foreach (var item in layerlevel)
                {
                    IFeatureClass tempFeatureClass = featureWorkspace.OpenFeatureClass(item.Key);
                    if (tempFeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        sf.Geometry = (point as ITopologicalOperator).Buffer(tolerance);
                        IFeature maskedFeature = null;
                        IFeatureCursor maskedCursor = tempFeatureClass.Search(sf, false);
                        while ((maskedFeature = maskedCursor.NextFeature()) != null)
                        {
                            if (item.Key == FeatureClass.AliasName && SelectedFeature.OID == maskedFeature.OID) { continue; }
                            tempfeaturepn.Add(maskedFeature, item.Value);
                            tempfeaturepn.Add(SelectedFeature, level);
                            polygonpolygonprocess(SelectedFeature, level);//面缝隙
                            polygonIntersctpolygonprocess(SelectedFeature, level);//面相交
                            tempfeaturepn.Clear();
                        }
                        Marshal.ReleaseComObject(maskedCursor);
                    }
                }


            }
            Marshal.ReleaseComObject(boulFCCursor);

        }
        /// <summary>
        /// 面缝隙
        /// </summary>
        /// <param name="SelectedFeature"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool polygonpolygonprocess(IFeature SelectedFeature, int level)
        {
            bool flag = false;
            IPolygon polyline = SelectedFeature.ShapeCopy as IPolygon;
            if (tempfeaturepn.Count == 0) { return flag; }
            Dictionary<IFeature, int> temppy = Max(tempfeaturepn);
            IPolygon tempunion = temppy.First().Key.ShapeCopy as IPolygon;
            IPolygon temp = tempunion;
            foreach (var itemPy in temppy)
            {
                temp = union(temp, itemPy.Key.ShapeCopy as IPolygon);
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
                IPolygon polygon = union(temppy.First().Key.ShapeCopy as IPolygon, IPolygon);
                temppy.First().Key.Shape = polygon;
                temppy.First().Key.Store();
            }
            return flag;
        }
        /// <summary>
        /// 面相交
        /// </summary>
        /// <param name="SelectedFeature"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool polygonIntersctpolygonprocess(IFeature SelectedFeature, int level)
        {
            bool flag = false;
            IPolygon polyline = SelectedFeature.ShapeCopy as IPolygon;
            if (tempfeaturepn.Count == 0) { return flag; }
            Dictionary<IFeature, int> temppy = Max(tempfeaturepn);
            IPolygon tempunion = temppy.First().Key.ShapeCopy as IPolygon;
            foreach (var item in temppy)
            {
                IFeature fea = item.Key;
                IPolygon objPolygon = fea.ShapeCopy as IPolygon;
                IRelationalOperator relationalOperatorPgon = objPolygon as IRelationalOperator;//好接口
                foreach (var item2 in temppy)
                {
                    IFeature tempfea = item2.Key;
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
            return flag;
        }
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
        /// 升序排列
        /// </summary>
        /// <param name="countpara"></param>
        /// <returns></returns>
        public Dictionary<IFeature, int> Min(Dictionary<IFeature, int> countpara)
        {
            Dictionary<IFeature, int> countpara1 = countpara.OrderBy(o => o.Value).ToDictionary(p => p.Key, o => o.Value);
            return countpara1;
        }
        /// <summary>
        /// 降序排列
        /// </summary>
        /// <param name="countpara"></param>
        /// <returns></returns>
        public Dictionary<IFeature, int> Max(Dictionary<IFeature, int> countpara)
        {
            Dictionary<IFeature, int> countpara1 = countpara.OrderByDescending(o => o.Value).ToDictionary(p => p.Key, o => o.Value);
            return countpara1;
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
        public IPolygon union(IPolygon basefeature, IPolygon objectfeature)
        {
            ITopologicalOperator feaTopo = basefeature as ITopologicalOperator;
            IGeometry geo = feaTopo.Union(objectfeature);
            feaTopo.Simplify();
            return geo as IPolygon;
        }
        /// <summary>
        ///  从mdb提取表
        /// </summary>
        /// <param name="mdbFilePath">mdb路径</param>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        public static DataTable ReadToDataTable(string mdbFilePath, string tableName)
        {
            DataTable pDataTable = new DataTable();
            IWorkspaceFactory pWorkspaceFactory = new AccessWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(mdbFilePath, 0);
            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTTable);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            ITable pTable = null;
            while (pDataset != null)
            {
                if (pDataset.Name == tableName)
                {
                    pTable = pDataset as ITable;
                    break;
                }
                pDataset = pEnumDataset.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
            if (pTable != null)
            {
                ICursor pCursor = pTable.Search(null, false);
                IRow pRow = null;
                //添加表的字段信息
                for (int i = 0; i < pTable.Fields.FieldCount; i++)
                {
                    pDataTable.Columns.Add(pTable.Fields.Field[i].Name);
                }
                //添加数据
                while ((pRow = pCursor.NextRow()) != null)
                {
                    DataRow dr = pDataTable.NewRow();
                    for (int i = 0; i < pRow.Fields.FieldCount; i++)
                    {
                        object obValue = pRow.get_Value(i);
                        if (obValue != null && !Convert.IsDBNull(obValue))
                        {
                            dr[i] = pRow.get_Value(i);
                        }
                        else
                        {
                            dr[i] = "";
                        }
                    }
                    pDataTable.Rows.Add(dr);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
            }
            return pDataTable;
        }
    }
}


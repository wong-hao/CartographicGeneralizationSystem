using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using GENERALIZERLib;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;


namespace BuildingGen
{
    public class WaterAggreforSelect : BaseGenTool
    {
        INewPolygonFeedback fb;
        GLayerInfo info;
        Generalizer gen;
        public WaterAggreforSelect()
        {
            base.m_category = "GBuilding";
            base.m_caption = "水系合并";
            base.m_message = "对选定水系进行合并";
            base.m_toolTip = "对选定水系进行合并";
            base.m_name = "WaterUnion";
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("水系合并",(double)15)
            };
            gen = new Generalizer();
        }

        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null);
            }
        }

        public override void OnClick()
        {
            info = null;
            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers)
            {
                if (tempInfo.LayerType == GCityLayerType.水系
                    && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && tempInfo.OrgLayer != null
                    )
                {
                    info = tempInfo;
                    break;
                }
            }
            if (info == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到水系图层");
                return;
            }

            IMap map = m_application.Workspace.Map;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null)
            {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;
            if (fc.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                System.Windows.Forms.MessageBox.Show("当前编辑图层不是面状图层");
                return;
            }
            gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 10000, 50000);

        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            if (info == null)
                return;

            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb == null)
            {
                fb = new NewPolygonFeedbackClass();
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                fb.Start(p);
            }
            else
            {
                fb.AddPoint(p);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (fb != null)
            {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }

        public override void OnDblClick()
        {
            if (fb == null)
                return;

            IPolygon p = fb.Stop();
            fb = null;
            Gen(p);

            m_application.MapControl.Refresh();
        }

        private void Gen(IPolygon range)
        {
            IFeatureLayer layer = new FeatureLayerClass();
            layer.FeatureClass = (info.Layer as IFeatureLayer).FeatureClass;
            layer.Name = "tp";
            IFeatureClass fc = layer.FeatureClass;

            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureSelection fselect = layer as IFeatureSelection;

            fselect.SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);

            try
            {

                double distance = (double)m_application.GenPara["水系合并"];
                ISelectionSet set = fselect.SelectionSet;

                if (set.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("请选定要素!");
                }
                GLayerManager layerManager = m_application.Workspace.LayerManager;

                IEnumIDs ids = set.IDs;
                int id = -1;

                GeometryBagClass gb = new GeometryBagClass();
                IFeature fb = null;
                double maxArea = 0;
                List<int> toDeleteID = new List<int>();
                /*while ((id = ids.Next()) != -1)
                {
                    IFeature feature = fc.GetFeature(id);
                    IArea area = feature.ShapeCopy as IArea;
                    IGeometry shape = feature.ShapeCopy;
                    gb.AddGeometries(1, ref shape);
                    if (area.Area>maxArea)
                    {
                        toDeleteID.Add(fb.OID);
                        maxArea = area.Area;
                        fb = feature;
                    }
                    else
                    {
                        toDeleteID.Add(id);
                    }
                }*/

                bool ii = CompleteFeatureGroupHandle_NoGroup2(layer, distance);

               #region 缓冲合并
               // //IPolygon poly = gen.AggregationOfPolygons(gb, 5);
               // IPolygon poly = CompleteFeatureGroupHandle(gb, 5);

               // //IPolygon poly=gb.get_Geometry(0) as IPolygon;
               // //ITopologicalOperator to= poly as ITopologicalOperator;
               // //for(int j=1;j<gb.GeometryCount;j++)
               // //{
               // //    to=to.Union(gb.get_Geometry(j)) as ITopologicalOperator;
               // //    to.Simplify();
               // //}

               //if (fb != null && poly != null)
               //{
               //    //fb.Shape = to as IGeometry;
               //    fb.Shape = poly;
               //    fb.Store();
               //    int[] deleteID = toDeleteID.ToArray();
               //    IFeature deleteFeat = null;
               //    for (int j = 0; j < deleteID.Length; j++)
               //    {
               //        deleteFeat = fc.GetFeature(deleteID[j]);
               //        deleteFeat.Delete();

               //    }
               //}
               //else
               //{
               //    return;
               //}
               #endregion

               m_application.MapControl.Refresh();
          

            }
            catch (Exception ex)
            {

            }
        }

        private IPolygon TransformRingToPolygon(IRing ring)
        {
            try
            {
                IPolygon poly;
                ITopologicalOperator topoOper;

                IGeometryCollection geoCol = new PolygonClass();
                object missing = Type.Missing;
                geoCol.AddGeometry(ring as IGeometry, ref missing, ref missing);
                poly = geoCol as IPolygon;
                topoOper = poly as ITopologicalOperator;
                topoOper.Simplify();

                return poly;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }

        private IPolygon CompleteFeatureGroupHandle(IGeometryCollection gb,double distance)
        {
            IPolygon tpPoly=new PolygonClass();
            for (int m = 0; m < gb.GeometryCount; m++)
            {
                bool isR = false;
                tpPoly = gb.get_Geometry(m) as IPolygon;
                IProximityOperator toProxi = tpPoly as IProximityOperator;
                for (int j = 0; j < gb.GeometryCount; j++)
                {
                    if (j == m)
                    {
                        continue;
                    }
                    IPolygon tpPoly_ = gb.get_Geometry(j) as IPolygon;
                    double dis = toProxi.ReturnDistance(tpPoly_);
                    if (dis < distance)
                    {
                        isR = true;
                        break;
                    }
                }
                if (isR == false)
                {
                    System.Windows.Forms.MessageBox.Show("Terrible!距离太远");
                    return null;
                }
            }
            if (gb.GeometryCount > 1)
            {
                object missing = Type.Missing;
                IGeometryCollection geoColOri = new GeometryBagClass();
                //把原始的Geometry放到geoColOri中
                geoColOri = gb;
                //得到合并后的Geometry,放在topo变量中
                ITopologicalOperator topo = geoColOri.get_Geometry(0) as ITopologicalOperator;
                topo = topo.Buffer(distance) as ITopologicalOperator;
                topo.Simplify();
                for (int i = 1; i < geoColOri.GeometryCount; i++)
                {
                    ITopologicalOperator topoIn = geoColOri.get_Geometry(i) as ITopologicalOperator;
                    topoIn = topoIn.Buffer(distance) as ITopologicalOperator;
                    topoIn.Simplify();
                    topo = topo.Union(topoIn as IGeometry) as ITopologicalOperator;
                }
                topo = topo.Buffer(-distance) as ITopologicalOperator;
                //找出桥，并把凹凸的地方抹掉
                IGeometryCollection geoBridgeCol = new GeometryBagClass();
                ITopologicalOperator topoBridge = topo;
                for (int i = 0; i < geoColOri.GeometryCount; i++)
                {
                    topoBridge = topoBridge.Difference(geoColOri.get_Geometry(i)) as ITopologicalOperator;
                    topoBridge.Simplify();
                }
                IGeometryCollection geoColDent = topoBridge as IGeometryCollection;
                for (int i = 0; i < geoColDent.GeometryCount; i++)//17
                {
                    IGeometry geo = geoColDent.get_Geometry(i);
                    if ((geo as IArea).Area < 0 || geo.IsEmpty || geo == null)
                    {
                        continue;
                    }
                    IPolygon polygon = TransformRingToPolygon(geo as IRing);
                    if ((polygon as IGeometry).IsEmpty)
                    {
                        continue;
                    }
                    IRelationalOperator relDent = polygon as IRelationalOperator;
                    int intersectCount = 0;
                    for (int j = 0; j < geoColOri.GeometryCount; j++)
                    {
                        if (relDent.Touches(geoColOri.get_Geometry(j)))
                        {
                            intersectCount++;
                        }
                    }
                    if (intersectCount < 2)
                    {
                        topo = topo.Difference(polygon) as ITopologicalOperator;
                    }
                    else
                    {
                        geoBridgeCol.AddGeometry(polygon, ref missing, ref missing);
                    }
                }
                //把原始的geometry合并到最后生成的合并好的几何体topo中
                double areaOfOri = 0;
                for (int i = 0; i < geoColOri.GeometryCount; i++)
                {
                    areaOfOri += (geoColOri.get_Geometry(i) as IArea).Area;
                    topo = topo.Union(geoColOri.get_Geometry(i)) as ITopologicalOperator;
                    topo.Simplify();
                }
                IPolygon result = topo as IPolygon;
                return result;

            }
            else
            {
                return null;
            }

        }


        //合并最终版本
        private void Densify(List<IFeature> featureList)
        {
            IPolygon poly = new PolygonClass();
            foreach (IFeature oriFeature in featureList)
            {
                poly = oriFeature.Shape as IPolygon;
                poly.Densify(7, 0);
                oriFeature.Shape = poly;
                oriFeature.Store();
            }

        }

        public bool isOutsideTri(ITinTriangle tri)
        {
            int tag = -1;
            tag = (tri.get_Node(0)).TagValue;
            if (tag == (tri.get_Node(1)).TagValue && tag == (tri.get_Node(2)).TagValue)
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        public void addNodeOIDtoList(ITinTriangle tri, List<int> toList)
        {
            bool isExisted = false;
            for (int i = 0; i < 3; i++)
            {
                ITinNode node = tri.get_Node(i);

                foreach (int j in toList)
                {
                    if (j == node.TagValue)
                    {
                        isExisted = true;
                    }
                }
                if (!isExisted)
                {
                    toList.Add(node.TagValue);
                }
            }
        }

        private IPolygon GetPolygonFromFeatureList(List<IFeature> featureList)
        {
            IFeature[] tempArray = featureList.ToArray();
            ITopologicalOperator temp = tempArray[0].ShapeCopy as ITopologicalOperator;
            temp.Simplify();
            for (int i = 1; i < featureList.Count; i++)
            {
                ITopologicalOperator anotherTopo = tempArray[i].ShapeCopy as ITopologicalOperator;
                anotherTopo.Simplify();
                temp = temp.Union(anotherTopo as IGeometry) as ITopologicalOperator;
                temp.Simplify();
            }

            IPolygon polygon = temp.ConvexHull() as IPolygon;

            return polygon;
        }

        private bool CompleteFeatureGroupHandle_NoGroup2(IFeatureLayer featLayer,double dis)
        {
        
            Dictionary<int, int> ClusterID = new Dictionary<int, int>();
            IFeatureClass pInputFeatClass = featLayer.FeatureClass;
            IFeatureSelection featSelection = featLayer as IFeatureSelection;
            List<IFeature> featureList = new List<IFeature>();
            ISelectionSet selectionSet = featSelection.SelectionSet;
            ICursor oriCursor;
            IFeature tempFeat;
            selectionSet.Search(null, false, out oriCursor);
            IFeatureCursor cursor = oriCursor as IFeatureCursor;
            while ((tempFeat = cursor.NextFeature()) != null)
            {
                featureList.Add(tempFeat);
            }
            if (featureList.Count > 1)
            {
                object missing = Type.Missing;
                Densify(featureList);
                TinClass tin = new TinClass();
                tin.InitNew(m_application.MapControl.FullExtent);
                tin.StartInMemoryEditing();
                object z = 0;
                IGeometry poly = new PolygonClass();       
                IPointCollection geoOriPoints = new MultipointClass();

                foreach (IFeature feature in featureList)
                {
                    IPolygon4 t = feature.ShapeCopy as IPolygon4;
                    IPolygon4 final = removeInteriorRings(t);
                    //IPolygon4 final = t;
                    feature.Shape = final as IGeometry;
                    feature.Store();

                    geoOriPoints = feature.Shape as IPointCollection;

                    int tag = feature.OID;
                    for (int i = 0; i < geoOriPoints.PointCount; i++)
                    {
                        tin.AddShape(geoOriPoints.get_Point(i), esriTinSurfaceType.esriTinMassPoint, tag, ref z);
                    }
                }

                ITinValueFilter filter = new TinValueFilterClass();
                ITinFeatureSeed seed = new TinTriangleClass();
                filter.ActiveBound = esriTinBoundType.esriTinUniqueValue;
                filter.UniqueValue = 0;
                seed.UseTagValue = true;
                ITinEdit tinEdit = tin as ITinEdit;
                ITinAdvanced tinAd = tin as ITinAdvanced;

                for (int i = 1; i < tin.TriangleCount + 1; i++)
                {
                    ITinTriangle tinTriangle = tin.GetTriangle(i);
                    if (!tinTriangle.IsInsideDataArea || !isOutsideTri(tinTriangle))
                    //if (!tinTriangle.IsInsideDataArea )
                    {
                        tinEdit.SetTriangleTagValue(i, -2);
                    }
                    else
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            ITinEdge tinEdge = tinTriangle.get_Edge(k);
                            if (tinEdge.Length > dis)
                            {
                                tinEdit.SetTriangleTagValue(i, -2);
                            }

                        }
                    }
                }

                IGeometryCollection tinPolyColl = new GeometryBagClass();
                IFeature touchFeat;
                List<int> deleteOIDList = new List<int>();
                List<int> touchOIDList = new List<int>();
                try
                {
                    for (int i = 1; i < tin.TriangleCount + 1; i++)
                    {
                        if ((tin.GetTriangle(i)).TagValue == 0)
                        {

                            seed = tin.GetTriangle(i) as ITinFeatureSeed;
                            ITinPolygon tinPoly = tinAd.ExtractPolygon(seed as ITinElement, filter as ITinFilter, false);
                            IPolygon polygon = tinPoly.AsPolygon(null, false);
                            ITopologicalOperator polygonTo = polygon as ITopologicalOperator;
                            polygonTo.Simplify();
                            IEnumTinTriangle enumTri = tinPoly.AsTriangles();

                            ITinTriangle tempTriangle;
                            while ((tempTriangle = enumTri.Next()) != null)
                            {
                                addNodeOIDtoList(tempTriangle, touchOIDList);
                                tinEdit.SetTriangleTagValue(tempTriangle.Index, -1);
                            }
                            int[] array = touchOIDList.ToArray();

                            if (polygon != null && touchOIDList.Count > 0)
                            {
                                IFeature maxAreaFeat = null;
                                double maxArea = 0;

                                for (int m = 0; m < array.Length; m++)
                                {
                                    int touchID = array[m];
                                    while (ClusterID.ContainsKey(touchID))
                                    {
                                        bool a = ClusterID.TryGetValue(touchID, out touchID);
                                    }

                                    touchFeat = pInputFeatClass.GetFeature(touchID);
                                  
                                    ITopologicalOperator touchOriTopo = touchFeat.ShapeCopy as ITopologicalOperator;
                                    touchOriTopo.Simplify();
                                    polygonTo = polygonTo.Union(touchFeat.ShapeCopy) as ITopologicalOperator;
                                    polygonTo.Simplify();
                                    if ((touchFeat.Shape as IArea).Area > maxArea)
                                    {
                                        maxArea = (touchFeat.Shape as IArea).Area;
                                        maxAreaFeat = touchFeat;
                                    }

                                }

                                bool c = false;
                                for (int y = 0; y < array.Length; y++)
                                {
                                    if (array[y] != maxAreaFeat.OID)
                                    {
                                        int tpID = array[y];
                                        while (ClusterID.ContainsKey(tpID))
                                        {
                                            bool a = ClusterID.TryGetValue(tpID, out tpID);
                                        }
                                        if (tpID != maxAreaFeat.OID) //确保不会相等，导致最后存储要素为空
                                        {
                                            IFeature todele = pInputFeatClass.GetFeature(tpID);
                                            todele.Delete();

                                            if (ClusterID.ContainsKey(tpID))
                                            {
                                                c = ClusterID.Remove(tpID);
                                            }

                                            ClusterID.Add(tpID, maxAreaFeat.OID);//加入字典，重新梳理已删除的要素与新增要素之间的关系，以便后面融合操作.???待高手解决
                                        }

                                    }
                                }

                                IFeature mergeFeature = maxAreaFeat;

                                IPolygon4 finalPolygon = removeInteriorRings(polygonTo as IPolygon4);
                                //IPolygon4 finalPolygon = polygonTo as IPolygon4;
                                mergeFeature.Shape = finalPolygon as IGeometry;


                                mergeFeature.Store(); //!
    
                                featSelection.Add(mergeFeature);//新生成的要素需重新加入选择集        
              
                            }
                            touchOIDList.Clear();
                        }

                    }

                }
                catch (Exception ex)
                { }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(tin);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
            }
            featureList.Clear();
            ClusterID.Clear();
            return true;

        }

        private IPolygon4 removeInteriorRings(IPolygon4 ringsPoly)
        {
            ITopologicalOperator tt = ringsPoly as ITopologicalOperator;
            tt.Simplify();
            IGeometryBag gb = ringsPoly.ConnectedComponentBag;
            IEnumGeometry gbe = gb as IEnumGeometry;
            gbe.Reset();
            IGeometry gbeg = gbe.Next();
            double maxAr = 0;
            IGeometry maxG = null;
            while (gbeg != null)
            {
                IArea gbega = gbeg as IArea;
                if (gbega.Area > maxAr)
                {
                    maxG = gbeg;
                    maxAr = gbega.Area;
                }
                gbeg = gbe.Next();
            }
            ringsPoly = maxG as IPolygon4;
            IGeometryBag exteriorRings = ringsPoly.ExteriorRingBag;
            IEnumGeometry exteriorEnum = exteriorRings as IEnumGeometry;
            exteriorEnum.Reset();
            IRing currentExterRing = exteriorEnum.Next() as IRing;
            IRing tpInteriorRing = new RingClass();
            IPolygon4 noInterRingPolygon = new PolygonClass();
            IGeometryCollection ringsCollection = noInterRingPolygon as IGeometryCollection;
            IGeometry forAddGeometry = currentExterRing as IGeometry;
            ringsCollection.AddGeometries(1, ref forAddGeometry);
            while (currentExterRing != null)
            {
                IGeometryBag interiorRings = ringsPoly.get_InteriorRingBag(currentExterRing);
                IEnumGeometry interiorEnum = interiorRings as IEnumGeometry;
                tpInteriorRing = interiorEnum.Next() as IRing;
                while (tpInteriorRing != null)
                {
                    IArea tpInteriorRingArea = tpInteriorRing as IArea;
                    if (Math.Abs(tpInteriorRingArea.Area) > 600)
                    {
                        forAddGeometry = tpInteriorRing as IGeometry;
                        ringsCollection.AddGeometries(1, ref forAddGeometry);

                    }

                    tpInteriorRing = interiorEnum.Next() as IRing;
                }

                currentExterRing = exteriorEnum.Next() as IRing;
            }
            return noInterRingPolygon;
        }

    }

}


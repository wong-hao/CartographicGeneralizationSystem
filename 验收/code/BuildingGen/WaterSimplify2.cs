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


namespace BuildingGen {
    public class WaterSimplify2 : BaseGenTool {
        INewPolygonFeedback fb;
        GLayerInfo info;
        Generalizer gen;
        public WaterSimplify2() {
            base.m_category = "GWater";
            base.m_caption = "水系化简";
            base.m_message = "对选定水系进行化简";
            base.m_toolTip = "对选定水系进行化简";
            base.m_name = "WaterSimplify2";
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("水系化简弯曲深度",(double)22),
                new GenDefaultPara("水系最小上图面积",(double)400)
            };
            gen = new Generalizer();
        }

        public override bool Enabled {
            get {
                return (m_application.Workspace != null);
            }
        }

        public override void OnClick() {
            info = null;
            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers) {
                if (tempInfo.LayerType == GCityLayerType.水系
                    && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && tempInfo.OrgLayer != null
                    ) {
                    info = tempInfo;
                    break;
                }
            }
            if (info == null) {
                System.Windows.Forms.MessageBox.Show("没有找到水系图层");
                return;
            }

            IMap map = m_application.Workspace.Map;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null) {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;
            if (fc.ShapeType != esriGeometryType.esriGeometryPolygon) {
                System.Windows.Forms.MessageBox.Show("当前编辑图层不是面状图层");
                return;
            }
            gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 2000, 10000);
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button == 4)
                return;
            if (info == null)
                return;

            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb == null) {
                fb = new NewPolygonFeedbackClass();
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                fb.Start(p);
            }
            else {
                fb.AddPoint(p);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y) {
            if (fb != null) {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }

        public override void OnDblClick() {
            if (fb == null)
                return;

            IPolygon p = fb.Stop();
            fb = null;
            Gen2(p);

            m_application.MapControl.Refresh();
        }
        private IFeatureClass GetWaterLineClass()
        {
            IFeatureClass centralizedDitch = null;
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;
            if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
            {
                centralizedDitch = FeatWS.OpenFeatureClass("沟渠中心线");
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("没有找到水系中心线层！");
            }
            return centralizedDitch;
        }

        Dictionary<int, IFeature> id_ifeature = new Dictionary<int, IFeature>();
        private void Gen(IPolygon range) {
            IFeatureLayer layer = new FeatureLayerClass();
            layer.FeatureClass = (info.Layer as IFeatureLayer).FeatureClass;


            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureSelection fselect = layer as IFeatureSelection;

            fselect.SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
            ICursor cr;
            fselect.SelectionSet.Search(null, false, out cr);
            IFeatureCursor c = cr as IFeatureCursor;
            IFeature fe = null;
            id_ifeature.Clear();
            while ((fe = c.NextFeature()) != null)
            {
                id_ifeature.Add(fe.OID, fe);
            }

            wait=m_application.SetBusy(true);
            try {
                //gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 10000, 50000);

                double distance = (double)m_application.GenPara["水系化简弯曲深度"];
                double minArea = (double)m_application.GenPara["水系最小上图面积"];
                ISelectionSet set = (layer as IFeatureSelection).SelectionSet;

                if (set.Count == 0) {
                    System.Windows.Forms.MessageBox.Show("Please Select Feature!");
                }
                GLayerManager layerManager = m_application.Workspace.LayerManager;


                string tempName = m_application.Workspace.LayerManager.TempLayerName();
                layer.Name =tempName+"a";
                ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = m_application.Geoprosessor;
                gp.OverwriteOutput = true;
                SimplifyPolygon simplify = new SimplifyPolygon(layer, tempName, "BEND_SIMPLIFY", distance);
                simplify.minimum_area = minArea;
                //simplify.error_option = "RESOLVE_ERRORS";
                object isOK = gp.Execute(simplify, null);
                if (isOK == null)
                {
                    MessageBox.Show("化简失败！");
                }

                IFeatureClass toCls = (m_application.Workspace.Workspace as IFeatureWorkspace).OpenFeatureClass(tempName);
                CopyAttribute2(layer.FeatureClass, sf, toCls);

            }
            catch (Exception ex) {

            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cr);
            m_application.SetBusy(false);
        }
        WaitOperation wait;
        private void CopyAttribute2(IFeatureClass fromClass, IQueryFilter qf, IFeatureClass ToClass) {
            IFeatureCursor featCursor;

            IFeature toFeat = null;
            IFeatureCursor tmpCursor = ToClass.Search(null, true);
            Dictionary<int, int> addFeatures = new Dictionary<int, int>();
            IFeatureCursor insertCursor = fromClass.Insert(true);
            while ((toFeat = tmpCursor.NextFeature()) != null) {
                wait.Step(ToClass.FeatureCount(null));
                ISpatialFilter filter = new SpatialFilterClass();
                filter.Geometry = toFeat.Shape;
                filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                //filter.GeometryField = ToClass.ShapeFieldName;


                featCursor = fromClass.Search(filter, true);
                IFeature tempFeat = null;
                int maxID = -1;
                double maxArea = 0;
                while ((tempFeat = featCursor.NextFeature()) != null) {
                    if ((tempFeat.Shape as IArea).Area > maxArea) {
                        maxArea = (tempFeat.Shape as IArea).Area;
                        maxID = tempFeat.OID;
                    }
                    //tempFeat.Shape = toFeat.Shape;
                    //tempFeat.Store();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);

                if (maxID == -1) {
                    continue;
                }
                //IFeature maxFe = fromClass.GetFeature(maxID);
                IFeature maxFe = null;
                id_ifeature.TryGetValue(maxID, out maxFe);
                IFeatureBuffer fb = maxFe as IFeatureBuffer;
                fb.Shape = toFeat.ShapeCopy;
                addFeatures.Add((int)insertCursor.InsertFeature(fb), 0);
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);  

            }
            insertCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
            featCursor = fromClass.Update(qf, true);
            while ((toFeat = featCursor.NextFeature()) != null) {
                if (!addFeatures.ContainsKey(toFeat.OID))
                    featCursor.DeleteFeature();
            }
            featCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);
        }

        private void Gen2(IPolygon range)
        {
            try
            {
                IFeatureClass fc2 = GetWaterLineClass();
                IFeatureClass fc = (info.Layer as IFeatureLayer).FeatureClass;
                ISpatialFilter sf1 = new SpatialFilterClass();
                sf1.Geometry = range;
                sf1.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor fc1 = fc.Update(sf1, false);
                IFeature feat = null;
                double depth = (double)m_application.GenPara["水系化简弯曲深度"];
                double minArea = (double)m_application.GenPara["水系最小上图面积"];
                int count = fc.FeatureCount(sf1);
                wait = m_application.SetBusy(true);
                while ((feat = fc1.NextFeature()) != null)
                {
                    wait.SetText("水系面化简中...");
                    wait.Step(count);
                    if ((feat.Shape as IArea).Area < minArea)
                    {
                        feat.Delete();
                        continue;
                    }
                    IPolygon4 tpPoly = gen.SimplifyPolygonByDT(feat.Shape as IPolygon, depth) as IPolygon4;
                    IPolygon4 poly = removeInteriorRings(tpPoly, minArea);
                    (poly as ITopologicalOperator).Simplify();
                    if (poly.IsEmpty)
                    {
                        continue;
                    }
                    feat.Shape = poly;
                    feat.Store();
                }
                fc1.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fc1);
                m_application.MapControl.Refresh();

                if (fc2 != null)
                {
                    IFeatureCursor fcur2 = fc2.Update(sf1, false);
                    IFeature feat2 = null;
                    int count2 = fc2.FeatureCount(sf1);
                    while ((feat2 = fcur2.NextFeature()) != null)
                    {
                        try
                        {
                            wait.SetText("水系中心线化简中..." + feat2.OID);
                            wait.Step(count);
                            if ((feat2.Shape as IPolyline).Length < depth)
                            {
                                continue;
                            }
                            IPolyline tpline = gen.SimplifyPolylineByDT(feat2.Shape as IPolyline, depth) as IPolyline;
                            (tpline as ITopologicalOperator).Simplify();
                            if (tpline.IsEmpty)
                            {
                                continue;
                            }
                            feat2.Shape = tpline;
                            feat2.Store();
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                    fcur2.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur2);
                }

            }
            catch(Exception ex)
            {
                m_application.SetBusy(false);
            }
            m_application.SetBusy(false);
            m_application.MapControl.Refresh();
        }
        private IPolygon4 removeInteriorRings(IPolygon4 ringsPoly,double areaA)
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
                    if (Math.Abs(tpInteriorRingArea.Area) > areaA)
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

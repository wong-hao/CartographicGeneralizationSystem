using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;
namespace BuildingGen {
    public class VegetationGeneralizeEx : BaseGenTool {
        GLayerInfo VegetationLayer;
        Generalizer gen;
        public VegetationGeneralizeEx() {
            base.m_category = "GVegetation";
            base.m_caption = "植被综合";
            base.m_message = "植被综合";
            base.m_toolTip = "植被综合";
            base.m_name = "VegetationGeneralizeEx";
            gen = new Generalizer();
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("植被融合距离",(double)10),
                new GenDefaultPara("植被最小上图面积",(double)400),
                new GenDefaultPara("植被化简深度",(double)3)
            };
        }
        public override bool Enabled {
            get {
                return (m_application.Workspace != null);
            }
        }
        public override void OnClick() {
            VegetationLayer = null;
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.植被
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && info.OrgLayer != null
                    ) {
                    VegetationLayer = info;
                    break;
                }
            }
            if (VegetationLayer == null) {
                System.Windows.Forms.MessageBox.Show("没有找到植被图层");
                return;
            }

        }

        void Gen(IPolygon range) {
            IFeatureLayer layer = (VegetationLayer.Layer as IFeatureLayer);

            if (layer == null) {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;
            WaitOperation wo = m_application.SetBusy(true);

            try {
                gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 2000, 10000);
                double depth = (double)m_application.GenPara["植被化简深度"];
                //ESRI.ArcGIS.esriSystem.ITrackCancel trackCancel = new CancelTrackerClass();
                string featureClassName = m_application.Workspace.LayerManager.TempLayerName();
                IFeatureLayer fl = new FeatureLayerClass();
                fl.FeatureClass = fc;
                fl.Name = featureClassName;

                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = range;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                (fl as IFeatureSelection).SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
                AggregatePolygons ap = new AggregatePolygons();
                ap.in_features = fl;
                ap.aggregation_distance = (double)m_application.GenPara["植被融合距离"];
                ap.out_feature_class = m_application.Workspace.Workspace.PathName + "\\" + featureClassName;
                ap.minimum_area = (double)m_application.GenPara["植被最小上图面积"];
                ap.minimum_hole_size = (double)m_application.GenPara["植被最小上图面积"];

                //m_application.Geoprosessor.AddOutputsToMap = false;

                wo.SetText("正在进行综合准备。");
                //m_application.Geoprosessor.ClearMessages();
                //ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
                object result = m_application.Geoprosessor.Execute(ap, null);
                fl.FeatureClass = null;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fl);
                IFeatureClass tmpFc = (m_application.Workspace.Workspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
                ITable table = (m_application.Workspace.Workspace as IFeatureWorkspace).OpenTable(System.IO.Path.GetFileName(ap.out_table.ToString()));
                IQueryFilter qf_table = new QueryFilterClass();
                
                //SpatialFilterClass sf2 = new SpatialFilterClass();
                //sf2.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                //sf2.SpatialRelDescription = "T********";
                IFeatureCursor cursor = tmpFc.Search(null, true);
                IFeature tmpF = null;
                int fCount = tmpFc.FeatureCount(null);
                wo.Step(0);
                Dictionary<int, int> used_dic = new Dictionary<int, int>();
                IFeatureCursor insertCursor = fc.Insert(true);
                
                while ((tmpF = cursor.NextFeature()) != null) {
                    //sf2.Geometry = tmpF.Shape;
                    wo.SetText("正在进行综合操作[" + tmpF.OID + "/"+fCount+"]");
                    qf_table.WhereClause = "OUTPUT_FID = " + tmpF.OID;
                    ICursor queryProCursor = table.Search(qf_table, true);
                    IRow row = null;
                    int maxID = -1;
                    double maxArea = 0;
                    IFeature f = null;
                    while ((row = queryProCursor.NextRow()) != null) {
                        int input_id = (int)row.get_Value(row.Fields.FindField("INPUT_FID"));
                        f = fc.GetFeature(input_id);
                        IArea ar = (f.Shape as IArea);
                        if (ar == null || ar.Area < 0.1)
                            continue;
                        double a = ar.Area;
                        if (maxArea < a) {
                            maxArea = a;
                            maxID = f.OID;
                        }

                    }
                    wo.Step(fCount);

                    if (maxID == -1)
                        continue;
                    IPolygon poly = gen.SimplifyPolygonByDT(tmpF.Shape as IPolygon, depth);
                    (poly as ITopologicalOperator).Simplify();
                    if (poly.IsEmpty)
                        continue;
                    f = fc.GetFeature(maxID);
                    f.Shape = poly;
                    int oid = (int)insertCursor.InsertFeature(f as IFeatureBuffer);
                    if (!used_dic.ContainsKey(oid)) {
                        used_dic.Add(oid, f.OID);
                    }

                }
                insertCursor.Flush();
                IFeatureCursor updateFeatureCursor = layer.FeatureClass.Update(sf, true);
                IFeature deleteFeature = null;
                while ((deleteFeature = updateFeatureCursor.NextFeature()) != null) {
                    if (!used_dic.ContainsKey(deleteFeature.OID))
                        updateFeatureCursor.DeleteFeature();
                }
                updateFeatureCursor.Flush();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(updateFeatureCursor);

                //System.Runtime.InteropServices.Marshal.ReleaseComObject(iCur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(tmpFc);
            }
            catch {
            }
            m_application.SetBusy(false);
            m_application.MapControl.Refresh();

        }
        INewPolygonFeedback fb;
        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button == 4)
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
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb != null) {
                fb.MoveTo(p);
            }
        }

        public override void OnDblClick() {
            if (fb != null) {
                IPolygon poly = fb.Stop();
                fb = null;
                Gen(poly);
            }
        }
        public override void OnKeyDown(int keyCode, int Shift) {
            switch (keyCode) {
            case (int)System.Windows.Forms.Keys.Escape:
                if (fb != null) {
                    fb.Stop();
                    fb = null;
                }
                break;
            }
        }
    }
}

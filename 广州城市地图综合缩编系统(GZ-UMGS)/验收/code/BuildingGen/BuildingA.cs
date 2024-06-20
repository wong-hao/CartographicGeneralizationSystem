using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
namespace BuildingGen {
    public class BuildingA : BaseGenTool {
        INewPolygonFeedback fb;
        List<IPolygon> polygons;
        ISymbol symbol;
        GLayerInfo buildingLayer;
        IGPUtilities2 gpUtilities;
        public BuildingA() {
            base.m_category = "GBuilding";
            base.m_caption = "建筑物融合(手工)";
            base.m_message = "建筑物融合";
            base.m_toolTip = "建筑物融合";
            base.m_name = "BuildingA";
            polygons = new List<IPolygon>();
            SimpleFillSymbolClass sfs = new SimpleFillSymbolClass();
            sfs.Color = GRenderer.ColorFromRGB(0, 0, 0, true);
            SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
            sls.Color = GRenderer.ColorFromRGB(255, 20, 20, false);
            sls.Width = 2;
            sfs.Outline = sls;
            symbol = sfs;
            gpUtilities = new GPUtilitiesClass();
        }
        public override void OnCreate(object hook) {
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
        }

        public override bool Enabled {
            get {
                return m_application.Workspace != null
                    && m_application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing
                    //&& m_application.Workspace.EditLayer != null
                ;
            }
        }
        public override void OnClick() {
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.建筑物
                    //&& (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    ) {
                    buildingLayer = info;
                    break;
                }
            }

        }
        public override void OnKeyUp(int keyCode, int Shift) {
            switch (keyCode) {
            case (int)System.Windows.Forms.Keys.Space:
                GroupGP();
                break;
            }
        }

        private IFeatureClass A(IQueryFilter qf, IFeatureClass fc) {
            IFeatureLayer layer = new FeatureLayerClass();
            string name = m_application.Workspace.LayerManager.TempLayerName();
            layer.Name = name;
            layer.FeatureClass = fc;
            (layer as IFeatureSelection).SelectFeatures(qf, esriSelectionResultEnum.esriSelectionResultNew, false);
            GPExe(layer, m_application.Geoprosessor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(layer);
            return (m_application.Workspace.Workspace as IFeatureWorkspace).OpenFeatureClass(name);
            //return (IFeatureClass)m_application.Geoprosessor.Open(layer.Name);
        }

        private void GroupGP() {
            if (buildingLayer == null) {
                System.Windows.Forms.MessageBox.Show("没有建筑物图层");
                return;
            }
            IFeatureClass fc = (buildingLayer.Layer as IFeatureLayer).FeatureClass;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int i = 0;
            foreach (IPolygon item in polygons) {
                IFeatureLayer layer = new FeatureLayerClass();
                layer.Name = buildingLayer.Layer.Name + (i++);
                layer.FeatureClass = fc;
                sf.Geometry = item;
                (layer as IFeatureSelection).SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
                GPExe(layer, m_application.Geoprosessor);
            }
        }

        private object GPExe(IFeatureLayer layer, Geoprocessor processor) {
            AggregatePolygons ap = new AggregatePolygons();
            ap.in_features = layer;
            //processor.SetEnvironmentValue("Workspace", "in_memory");

            ap.out_feature_class = layer.Name;
            //ap.out_table ="c:\\temp\\" + layer.Name + "_tbl";
            ap.aggregation_distance = m_application.GenPara["建筑物融合（批量）_融合距离"];
            ap.minimum_area = m_application.GenPara["建筑物融合（批量）_最小上图面积"];
            ap.minimum_hole_size = m_application.GenPara["建筑物融合（批量）_最小保留洞面积"];
            ap.orthogonality_option = "ORTHOGONAL";

            return processor.Execute(ap, null);
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            (e.display as IDisplay).SetSymbol(symbol);
            foreach (IPolygon item in polygons) {
                (e.display as IDisplay).DrawPolygon(item);
            }
        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button != 1)
                return;
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb == null) {
                fb = new NewPolygonFeedbackClass();
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                fb.Start(p);
            } else {
                fb.AddPoint(p);
            }
        }
        public override void OnMouseMove(int Button, int Shift, int X, int Y) {
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb != null) {
                fb.MoveTo(p);
            }
        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y) {
            //base.OnMouseUp(Button, Shift, X, Y);
        }
        public override void OnDblClick() {
            if (fb == null) {
                return;
            }

            IPolygon polygon = fb.Stop();
            //polygons.Add(polygon);
            fb = null;

            if (buildingLayer == null) {
                System.Windows.Forms.MessageBox.Show("没有建筑物图层");
                return;
            }
            IFeatureClass fc = (buildingLayer.Layer as IFeatureLayer).FeatureClass;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.Geometry = polygon;
            IFeatureClass resFeature = A(sf, fc);
            IFeatureCursor fCursor = fc.Update(sf, true);
            IFeature f = null;
            m_application.EngineEditor.StartOperation();
            while (fCursor.NextFeature() != null) {
                fCursor.DeleteFeature();
            }
            fCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
            fCursor = resFeature.Search(null, true);
            while ((f = fCursor.NextFeature()) != null) {
                IFeature nFeature = fc.CreateFeature();
                nFeature.Shape = f.ShapeCopy;
                nFeature.Store();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(resFeature);
            m_application.EngineEditor.StopOperation("融合");
            m_application.MapControl.Refresh();
        }
    }
}

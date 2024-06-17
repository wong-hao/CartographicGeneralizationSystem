using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen {
    class ConflictOfBuildingAndRoad :BaseGenCommand{
        public ConflictOfBuildingAndRoad() {
            base.m_category = "GBuilding";
            base.m_caption = "冲突处理";
            base.m_message = "解决建筑物与其他要素冲突";
            base.m_toolTip = "解决建筑物与其他要素类的冲突";
            base.m_name = "ConflictOfBuildingAndRoad";
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null; ;
            }
        }
        public override void OnClick() {
            GLayerInfo buildingLayer = GetBuildingLayer(m_application.Workspace);
            GLayerInfo roadLayer = GetRoadLayer(m_application.Workspace);
            if (buildingLayer == null) {
                System.Windows.Forms.MessageBox.Show("缺少建筑物图层");
                return;
            }
            if (roadLayer == null) {
                System.Windows.Forms.MessageBox.Show("缺少道路中心线图层");
                return;
            }

            WaitOperation wo = m_application.SetBusy(true);
            IFeatureLayer bfl = buildingLayer.Layer as IFeatureLayer;
            IFeatureLayer rfl = roadLayer.Layer as IFeatureLayer;
            IFeatureCursor bfCursor = bfl.FeatureClass.Update(null, true);
            IFeatureCursor insertCursor = bfl.FeatureClass.Insert(true);
            IFeature bFeature = null;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            object miss = Type.Missing;
            int wCount = bfl.FeatureClass.FeatureCount(null);
            while ((bFeature = bfCursor.NextFeature())!=null) {
                wo.Step(wCount);
                wo.SetText("正在处理【" + bFeature.OID + "】");
                sf.Geometry = bFeature.Shape;
                //if (rfl.FeatureClass.FeatureCount(sf) == 0)
                    //continue;

                IFeatureCursor rfCursor = rfl.FeatureClass.Search(sf, true);
                IFeature rFeature = null;
                PolylineClass line = new PolylineClass();
                while ((rFeature = rfCursor.NextFeature())!= null) {
                    IGeometryCollection gc = rFeature.Shape as IGeometryCollection;
                    for (int i = 0; i < gc.GeometryCount; i++) {
                        line.AddGeometry(gc.get_Geometry(i), ref miss, ref miss);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rfCursor);
                if (line.IsEmpty)
                    continue;
                line.Simplify();
                ITopologicalOperator4 topo = bFeature.ShapeCopy as ITopologicalOperator4;
                double area = (topo as IArea).Area;
                
                IGeometryCollection gcCut = null;
                try{
                    gcCut = topo.Cut2(line);
                }
                catch
                {
                    continue;
                }
                for (int i = 0; i < gcCut.GeometryCount; i++) {
                    IArea a = gcCut.get_Geometry(i) as IArea;
                    if (a.Area * 10 > area) {
                        bFeature.Shape = a as IGeometry;
                        insertCursor.InsertFeature(bFeature as IFeatureBuffer);
                    }
                }
                bfCursor.DeleteFeature();
            }
            bfCursor.Flush();
            insertCursor.Flush();
            m_application.SetBusy(false);
            m_application.MapControl.Refresh();
        }
        private GLayerInfo GetBuildingLayer(GWorkspace workspace) {
            foreach (GLayerInfo info in workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.建筑物
                    //&& (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    ) {
                    return info;
                }
            }
            return null;
        }
        private GLayerInfo GetRoadLayer(GWorkspace workspace) {
            foreach (GLayerInfo info in workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.道路
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    ) {
                    return info;
                }
            }
            return null;
        }

    }
}

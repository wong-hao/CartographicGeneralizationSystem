using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.NetworkAnalysis;

namespace BuildingGen {
    public class RoadRankChange : BaseGenTool {
        public RoadRankChange() {
            base.m_category = "GRoad";
            base.m_caption = "手工分级";
            base.m_message = "道路手工分级";
            base.m_toolTip = "道路手工等级\n拉框选择提升道路等级；\n按住shift拉框选择降低等级。";
            base.m_name = "RoadRankChange";
            roadLayer = null;
            rankID = -1;
            fb = null;
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }
        GLayerInfo roadLayer;
        int rankID;
        public override void OnClick() {
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.道路
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    ) {
                    roadLayer = info;
                    break;
                }
            }
            if (roadLayer == null) {
                System.Windows.Forms.MessageBox.Show("没有找到道路层");
                return;
            }

            IFeatureClass fc = (roadLayer.Layer as IFeatureLayer).FeatureClass;

            rankID = fc.Fields.FindField("道路等级");
            if (rankID == -1) {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "道路等级";
                field.Type_2 = esriFieldType.esriFieldTypeInteger;
                fc.AddField(field as IField);
                rankID = fc.Fields.FindField("道路等级");
            }
        }

        INewEnvelopeFeedback fb;

        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button == 4)
                return;
            if (fb == null && roadLayer !=null) {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb = new NewEnvelopeFeedbackClass();
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                fb.Start(p);
            }
        }
        public override void OnMouseMove(int Button, int Shift, int X, int Y) {
            if (fb != null) {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y) {
            if (fb != null) {
                IEnvelope env = fb.Stop();
                fb = null;
                int s = -1;
                if (Shift == 1) {
                    s = 1;
                }
                SpatialFilterClass sf = new SpatialFilterClass();
                sf.Geometry = env;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor fCursor = (roadLayer.Layer as IFeatureLayer).FeatureClass.Update(sf, true);
                IFeature feature = null;
                while ((feature = fCursor.NextFeature()) != null) {
                    object v = feature.get_Value(rankID);
                    int rank = 4;
                    if (v != null && v != DBNull.Value) {
                        rank = Convert.ToInt32(v) ;
                    }
                    rank += s;
                    while (rank > 4) {
                        rank--;
                    }
                    while (rank < 1) {
                        rank++;
                    }
                    feature.set_Value(rankID, rank);
                    fCursor.UpdateFeature(feature);
                }
                fCursor.Flush();
                m_application.MapControl.Refresh();
            }
        }
    }
}

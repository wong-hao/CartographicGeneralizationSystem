using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
namespace BuildingGen {
    public class RoadStroke : BaseGenTool {
        GLayerInfo roadLayer;
        public RoadStroke() {
            base.m_category = "GRoad";
            base.m_caption = "手工选取";
            base.m_message = "道路手工选取";
            base.m_toolTip = "道路手工选取。\n拉框选取道路；\n按住shift拉框取消选取。";
            base.m_name = "RoadStroke";
            roadLayer = null;
            usedID = -1;
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }
        int usedID;
        public override void OnClick() {
            roadLayer = null;
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
                System.Windows.Forms.MessageBox.Show("缺少道路中心线图层！");
                return;
            }
            IFeatureClass fc = (roadLayer.Layer as IFeatureLayer).FeatureClass;
            usedID = fc.FindField("_GenUsed");
            if (usedID == -1) {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "_GenUsed";
                field.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
                fc.AddField(field as IField);
                usedID = fc.Fields.FindField("_GenUsed");
            }
            IFeatureLayerDefinition fdefinition = roadLayer.Layer as IFeatureLayerDefinition;
            fdefinition.DefinitionExpression = "_GenUsed = 1";
        }
        INewEnvelopeFeedback fb;

        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button == 4)
                return;
            if (fb == null && roadLayer != null) {
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
                int s = 1;
                if (Shift == 1) {
                    s = 0;
                }

                IFeatureLayerDefinition fdefinition = roadLayer.Layer as IFeatureLayerDefinition;
                fdefinition.DefinitionExpression = "";

                SpatialFilterClass sf = new SpatialFilterClass();
                sf.Geometry = env;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor fCursor = (roadLayer.Layer as IFeatureLayer).FeatureClass.Update(sf, true);
                IFeature feature = null;
                while ((feature = fCursor.NextFeature()) != null) {                    
                    feature.set_Value(usedID, s);
                    fCursor.UpdateFeature(feature);
                }
                fCursor.Flush();

                fdefinition.DefinitionExpression = "_GenUsed = 1";
                m_application.MapControl.Refresh();
            }
        }
    }
}

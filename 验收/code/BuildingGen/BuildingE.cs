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
using System.Collections;
using System.Windows.Forms;

namespace BuildingGen {
    public class BuildingE : BaseGenTool {
        IPolygon m_range;
        List<IPolyline> m_lines;
        public BuildingE() {
            base.m_category = "GBuilding";
            base.m_caption = "规则建筑物分组";
            base.m_message = "规则建筑物分组";
            base.m_toolTip = "对规则建筑物进行自动分组";
            base.m_name = "BuildingE";
            m_lines = new List<IPolyline>();
        }
        public override void OnCreate(object hook) {
            //base.OnCreate(hook);
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            ISimpleFillSymbol sfs = new SimpleFillSymbolClass();
            //sfs.Style = esriSimpleFillStyle.esriSFSNull;
            IDisplay display = e.display as IDisplay;
            //display. = m_application.MapControl.Extent;
            ISimpleLineSymbol sls = new SimpleLineSymbolClass();
            sls.Width = 1;
            sls.Style = esriSimpleLineStyle.esriSLSSolid;
            sfs.Outline = sls;
            if (m_range != null) {
                display.SetSymbol(sfs as ISymbol);
                display.DrawPolygon(m_range);
                
            }
            IRgbColor rgb = new RgbColorClass();
            (rgb).Red = 255;
            (rgb).Green = 0;
            (rgb).Blue = 0;
            sls.Color = rgb;
            if (m_lines.Count > 0) {
                display.SetSymbol(sls as ISymbol);
                foreach (var item in m_lines) {
                    display.DrawPolyline(item);
                }
            }
        }
        private bool isInit = false;
        public override void OnClick() {
            if (!isInit) {
                isInit = true;
                m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
            }
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }

        INewPolygonFeedback fb_polygon;
        INewLineFeedback fb_line;
        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button != 1)
                return;
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (m_range == null) {
                if (fb_polygon == null) {
                    fb_polygon = new NewPolygonFeedbackClass();
                    fb_polygon.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                    fb_polygon.Start(p);
                }
                else {
                    fb_polygon.AddPoint(p);
                }
            }
            else {
                if (fb_line == null) {
                    fb_line = new NewLineFeedbackClass();
                    fb_line.Start(p);
                }
            }
            
        }
        public override void OnMouseMove(int Button, int Shift, int X, int Y) {
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb_polygon != null) {
                fb_polygon.MoveTo(p);
            }
            if (fb_line != null) {
                fb_line.MoveTo(p);
            }

        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y) {
            if (Button != 1)
                return;
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb_line != null) {
                IPolyline line = fb_line.Stop();
                fb_line = null;
                if (line == null || line.IsEmpty) {
                    return;
                }
                m_lines.Add(line);
            }

        }
        public override void OnDblClick() {
            if (fb_polygon != null) {
                IPolygon poly = fb_polygon.Stop();
                fb_polygon = null;
                if (poly == null || poly.IsEmpty) {
                    return;
                }
                m_range = poly;
                m_application.MapControl.Refresh();
            }
        }

        private GLayerInfo GetLayer(GWorkspace workspace) {
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

    }
}

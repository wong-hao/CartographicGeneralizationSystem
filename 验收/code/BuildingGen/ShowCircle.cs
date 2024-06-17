using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
namespace BuildingGen {
    public class ShowCircle : BaseGenCommand{
        MovePolygonFeedbackClass fb;
        CircleSizeDlg csDlg;
        SimpleFillSymbolClass fillSymbol;
        public ShowCircle() {
            base.m_category = "GSystem";
            base.m_caption = "显示圆圈";
            base.m_message = "调整在鼠标周围是否显示圆圈";
            base.m_toolTip = "调整在鼠标周围是否显示圆圈\n圆圈的直径等于设定值。";
            base.m_name = "ShowCircle";
            fillSymbol = new SimpleFillSymbolClass();
            fillSymbol.Style = esriSimpleFillStyle.esriSFSNull;
            
            csDlg = new CircleSizeDlg();
            csDlg.R = 5;
            csDlg.IsShowCircle = false;
        }
        public override void OnGenCreate(GApplication app) {
            base.OnGenCreate(app);
            m_application.MapControl.OnMouseMove += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnMouseMoveEventHandler(MapControl_OnMouseMove);
            m_application.WorkspaceOpened += new EventHandler(m_application_WorkspaceOpened);
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
            m_application.MapControl.OnKeyDown += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnKeyDownEventHandler(MapControl_OnKeyDown);
        }

        void MapControl_OnKeyDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnKeyDownEvent e) {
            switch (e.keyCode) { 
            case (int)System.Windows.Forms.Keys.O:
                csDlg.IsShowCircle = !csDlg.IsShowCircle;
                Show();
                break;
            }
        }

        void m_application_WorkspaceOpened(object sender, EventArgs e) {
            if (m_application.Workspace.MapConfig["ShowCircle"] == null) {
                m_application.Workspace.MapConfig["ShowCircle"] = csDlg.IsShowCircle;
                m_application.Workspace.MapConfig["CircleR"] = csDlg.R;
            }
            else {
                csDlg.IsShowCircle= (bool)m_application.Workspace.MapConfig["ShowCircle"];
                csDlg.R = (double)m_application.Workspace.MapConfig["CircleR"];
            }
            Show();
            if (fb != null)
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
            
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            if (fb != null) {
                fb.Refresh((e.display as IDisplay).hDC);
            }
        }

        void MapControl_OnMouseMove(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseMoveEvent e) {
            if (fb != null) {
                IPoint p = new PointClass();
                p.X = e.mapX;
                p.Y = e.mapY;
                fb.MoveTo(p);
            }
                
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }
        public override void OnClick() {
            if (csDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            Show();
        }
        void Show() {
            if (fb != null) {
                fb.Stop();
                fb = null;
            }
            m_application.Workspace.MapConfig["ShowCircle"] = csDlg.IsShowCircle;
            m_application.Workspace.MapConfig["CircleR"] = csDlg.R;

            if (!csDlg.IsShowCircle)
                return;

            fb = new MovePolygonFeedbackClass();
            fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
            //fb.Symbol = fillSymbol;
            PointClass p = new PointClass();
            p.X = 0;
            p.Y = 0;
            IGeometry c = p.Buffer(csDlg.R / 2);

            fb.Start(c as IPolygon, p);
        }
    }
}

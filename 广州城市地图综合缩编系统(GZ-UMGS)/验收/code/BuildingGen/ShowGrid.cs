using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
namespace BuildingGen {
    class ShowGrid : BaseGenCommand{

        SimpleFillSymbolClass fillSymbol;
        SimpleLineSymbolClass lineSymbol;
        public ShowGrid() {
            base.m_category = "GSystem";
            base.m_caption = "显示网格";
            base.m_message = "调整在图面上是否显示网格";
            base.m_toolTip = "调整在图面上是否显示网格";
            base.m_name = "ShowGrid";
            fillSymbol = new SimpleFillSymbolClass();
            fillSymbol.Style = esriSimpleFillStyle.esriSFSNull;
            lineSymbol = new SimpleLineSymbolClass();
            gsDlg = new GridSize();
            
            gsDlg.GridX = 2000;
            gsDlg.GridY = 2000;
            gsDlg.IsShowGrid = false;
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }
        public override void OnGenCreate(GApplication app) {            
            base.OnGenCreate(app);
            app.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
            app.WorkspaceOpened += new EventHandler(app_WorkspaceOpened);
        }

        void app_WorkspaceOpened(object sender, EventArgs e) {
            if (m_application.Workspace.MapConfig["ShowGrid"] == null) {
                m_application.Workspace.MapConfig["ShowGrid"] = gsDlg.IsShowGrid;
                m_application.Workspace.MapConfig["GridSizeX"] = gsDlg.GridX;
                m_application.Workspace.MapConfig["GridSizeY"] = gsDlg.GridY;
            }
            else {
                gsDlg.IsShowGrid = (bool)m_application.Workspace.MapConfig["ShowGrid"];
                gsDlg.GridX = (double)m_application.Workspace.MapConfig["GridSizeX"];
                gsDlg.GridY = (double)m_application.Workspace.MapConfig["GridSizeY"];
            }
        }


        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            if (!Enabled)
                return;
            if (!gsDlg.IsShowGrid)
                return;

            PolylineClass line = new PolylineClass();
            
            IEnvelope fullEnv = m_application.MapControl.FullExtent;
            IEnvelope currentEnv = m_application.MapControl.Extent;
            int xf = (int)((currentEnv.XMin - fullEnv.XMin) / gsDlg.GridX) - 1;
            int yf = (int)((currentEnv.YMin - fullEnv.YMin) / gsDlg.GridY) - 1 ;
            double currentx = fullEnv.XMin + (xf * gsDlg.GridX);
            double currenty = fullEnv.YMin + (yf * gsDlg.GridY);
            IPoint p = new PointClass();
            object miss = Type.Missing;
            while (currentx < currentEnv.XMax) {
                PathClass path = new PathClass();
                p.X = currentx;
                p.Y = currentEnv.YMin;
                path.AddPoint(p, ref miss, ref miss);
                p.Y = currentEnv.YMax;
                path.AddPoint(p, ref miss, ref miss);
                currentx += gsDlg.GridX;
                line.AddGeometry(path, ref miss, ref miss);
            }
            while (currenty < currentEnv.YMax) {
                PathClass path = new PathClass();
                p.X = 0;
                p.Y = currenty;
                path.AddPoint(p, ref miss, ref miss);
                p.X = currentEnv.XMax;
                path.AddPoint(p, ref miss, ref miss);
                currenty += gsDlg.GridY;
                line.AddGeometry(path, ref miss, ref miss);
            }
            IDisplay display = e.display as IDisplay;
            display.SetSymbol(lineSymbol);
            display.DrawPolyline(line);
        }
        GridSize gsDlg;
        public override void OnClick() {
            //base.OnClick();
            
            if (gsDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                m_application.Workspace.MapConfig["ShowGrid"] = gsDlg.IsShowGrid;
                m_application.Workspace.MapConfig["GridSizeX"] = gsDlg.GridX;
                m_application.Workspace.MapConfig["GridSizeY"] = gsDlg.GridY;
                m_application.MapControl.Refresh();
            }
        }
    }
}

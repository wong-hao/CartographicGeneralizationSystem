using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
namespace BuildingGen
{
    public class GWorkspace
    {
        public IWorkspace Workspace { get; private set; }
        public GLayerManager LayerManager { get; private set; }
        public GConfig MapConfig { get; private set; }
        public GApplication Application { get; private set; }
        public IMap Map
        {
            get { return mMap; }
            internal set
            {
                mMap = value; 
                Application.MapControl.Map = mMap;
            }
        }
        private IMap mMap;
        public string Name 
        {
            get
            {
                return Workspace.PathName;
            }
        }
        public GLayerInfo EditLayer { get; set; }

        private GWorkspace(GApplication app,IWorkspace ws)
        {
            this.Application = app;
            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            gp.SetEnvironmentValue("workspace", ws.PathName);
            app.Geoprosessor = gp;
            this.Workspace = ws;
            this.mMap = Application.MapControl.Map;
            //载入配置表
            MapConfig = GConfig.Create(this); 
            //载入图层信息
            LayerManager = new GLayerManager(this);
            this.Map.Name = (ws as IDataset).Name;
            if (this.Map.MapUnits == esriUnits.esriUnknownUnits) {
                this.Map.MapUnits = esriUnits.esriMeters;
                app.MapControl.MapUnits = esriUnits.esriMeters;
            }
        }
        internal static GWorkspace Open(GApplication app, string path)
        {
            if (!IsWorkspace(path))
                return null;
            
            IWorkspace w = GApplication.GDBFactory.OpenFromFile(path, 0);

            GWorkspace ws = new GWorkspace(app,w); 
            return ws;
        }
        internal static GWorkspace CreateWorkspace(GApplication app, string path)
        {
            //1.创建mdb工作区
            string dir = System.IO.Path.GetDirectoryName(path);
            string name = System.IO.Path.GetFileName(path);
            IWorkspaceName wname = GApplication.GDBFactory.Create(dir, name, null, 0);
            IWorkspace w = (wname as IName).Open() as IWorkspace;
            //2.创建配置表
            GConfig.CreateConfigTable(w);

            GWorkspace ws = new GWorkspace(app,w);
            ws.Save();
            return ws;
        }
        public static bool IsWorkspace(string path)
        {
            if (!GApplication.GDBFactory.IsWorkspace(path))
                return false;

            IWorkspace w = GApplication.GDBFactory.OpenFromFile(path, 0);
            if (!GConfig.ExistConfigTable(w))
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(w);
                return false;
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(w);
            return true;
        }
        public void Save()
        {
            foreach (var info in LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.建筑物) {
                    CityRender.RenderLayer(info, EnvRenderMode.仅显示综合层, LayerRenderMode.背景化显示, FeatureRenderMode.正常显示);
                }
            }
            LayerManager.Save();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Diagnostics;
using System.Threading;
namespace SMGI.Common
{
    public class GWorkspace
    {
        public bool IsCollaborativing
        {
            set;
            get;
        }

        public int LocalBaseVersion
        {
            set;
            get;
        }
        /// <summary>
        /// 用于协同作业加锁机制的文件流
        /// </summary>
        public System.IO.FileStream ColllaborativeFileStream { get; set; }
        /// <summary>
        /// 锁住文件
        /// </summary>
        /// <param name="filename">被锁的文件全路径</param>
        public void LockFile(string filename)
        {
            ColllaborativeFileStream = null;
            ColllaborativeFileStream = new System.IO.FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            ColllaborativeFileStream.Lock(0, 255);
        }

        /// <summary>
        /// 文件解锁
        /// </summary>
        /// <returns>解锁是否成功</returns>
        public bool UnLockFile()
        {
            if (ColllaborativeFileStream != null)
            {
                ColllaborativeFileStream.Unlock(0, 255);
                ColllaborativeFileStream.Close();
                ColllaborativeFileStream = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断文件是否上锁
        /// </summary>
        /// <param name="filename">文件全路径</param>
        /// <returns>是否上锁</returns>
        public string IsFileLock(string filename)
        {
            try
            {
                if (ColllaborativeFileStream == null)
                {
                    ColllaborativeFileStream = new System.IO.FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                }
                var buffer = Encoding.Default.GetBytes(System.Environment.MachineName);
                ColllaborativeFileStream.Write(buffer, 0, buffer.Length);
                ColllaborativeFileStream.Flush();
                ColllaborativeFileStream.Close();
                ColllaborativeFileStream = null;

                Thread.Sleep(10000);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                if (ColllaborativeFileStream != null)
                {
                    ColllaborativeFileStream.Close();
                    ColllaborativeFileStream = null;
                }
                return ex.Message;
            }
            return string.Empty;
        }
        public List<IRepresentation> SelectedRepresentations
        {
            get { return MapManagers[CurrentLayerManagerIndex].CurrentRepresentation; }
            //set { MapManagers[CurrentLayerManagerIndex].CurrentRepresentation = value; }
        }
        public ILayer CurrentLayer
        {
            get
            {
                return this.Application.TOCSelectItem.Layer;
            }
        }
        public IRepresentationWorkspaceExtension RepersentationWorkspaceExtension
        {
            get
            {
                IWorkspaceExtensionManager wem = EsriWorkspace as IWorkspaceExtensionManager;
                UID uid = new UIDClass();
                uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
                IRepresentationWorkspaceExtension rwe = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
                return rwe;
            }
        }
        public IWorkspace EsriWorkspace { get; private set; }
        public LayerManager LayerManager
        {
            get
            {
                return MapManagers[CurrentLayerManagerIndex];
            }
        }

        [SMGIAutoSave]
        internal int CurrentLayerManagerIndex { get; set; }

        public List<LayerManager> MapManagers { get; private set; }
        List<IMapFrame> mapFrames;

        [SMGIAutoSave]
        private List<Guid> MapIDs { get; set; }

        public void FocusMapAt(int i)
        {
            if (i < 0)
            {
                i = 0;
            }
            if (i >= MapManagers.Count)
            {
                i = MapManagers.Count - 1;
            }
            CurrentLayerManagerIndex = i;

            bool active = Application.MapControl.ActiveView.IsActive();
            if (active)
            {
                Application.MapControl.ActiveView.Deactivate();
                Application.MapControl.Map = this.Map;
                Application.MapControl.ActiveView.Activate(Application.MapControl.hWnd);
                Application.MapControl.Refresh();
            }
            active = Application.PageLayoutControl.ActiveView.IsActive();
            if (active)
            {
                Application.PageLayoutControl.ActiveView.Deactivate();
                //Application.MapControl.Map = this.Map;
                Application.PageLayoutControl.ActiveView.Activate(Application.PageLayoutControl.hWnd);
                Application.PageLayoutControl.Refresh();
            }

            //Application.MapControl.Refresh();
            //Application.TOCControl.Refresh();
            //Application.LayoutState = Application.LayoutState;
        }

        public Guid AddMap()
        {
            Guid mapID = Guid.NewGuid();
            var l = new LayerManager(this, mapID);
            MapManagers.Add(l);

            SynsMap();

            return mapID;
        }

        public LayerManager GetMapByGuid(Guid guid)
        {
            foreach (var item in MapManagers)
            {
                if (item.MapID.CompareTo(guid) == 0)
                {
                    return item;
                }
            }
            return null;
        }

        public string NewMapID()
        {
            return Guid.NewGuid().ToString();
        }
        public Common.Config MapConfig { get; private set; }
        public Common.GApplication Application { get; private set; }
        public LogManager Log { get; private set; }

        public IMap Map
        {
            get { return MapManagers[CurrentLayerManagerIndex].Map; }
        }

        [SMGIAutoSave]
        public IPageLayout PageLayout { get; internal set; }

        [SMGIAutoSave]
        public IEnvelope PageLayoutExtent { get; internal set; }

        [SMGIAutoSave]
        public IEnvelope MapExtent { get; internal set; }

        [SMGIAutoSave]
        public LayoutState LastState { get; internal set; }

        [SMGIAutoSave]
        internal string LastPath { get; set; }

        public bool IsDirty
        {
            get;
            set;
        }

        public string FullName
        {
            get
            {
                return EsriWorkspace.PathName;
            }
        }
        public string Name
        {

            get
            {
                return (EsriWorkspace as IDataset).Name;
            }
        }

        public double Scale
        {
            get { return Map.ReferenceScale; }
            set
            {
                Map.ReferenceScale = value;
                //MapFrame.MapScale = value;
            }
        }

        [SMGIAutoSave]
        public IGeometry OuterBoundary { get; set; }

        AutoSaveProperty autosave;
        private GWorkspace(Common.GApplication app, IWorkspace ws, bool flag = false)
        {
            if (flag)
            {
                this.LastState = LayoutState.MapControl;
                this.Application = app;
                ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = app.GPTool;
                gp.SetEnvironmentValue("workspace", ws.PathName);
                this.EsriWorkspace = ws;
                RasterLoad();
                (PageLayout as IActiveViewEvents_Event).FocusMapChanged += new IActiveViewEvents_FocusMapChangedEventHandler(GWorkspace_FocusMapChanged);
            }
            else
            {
                this.LastState = LayoutState.MapControl;
                this.Application = app;
                ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = app.GPTool;
                gp.SetEnvironmentValue("workspace", ws.PathName);
                //app.Geoprosessor = gp;
                this.EsriWorkspace = ws;
                //this.mMap = Application.MapControl.Map;
                //载入配置表
                MapConfig = Common.Config.Create(this);

                autosave = new AutoSaveProperty(this, MapConfig);
                Load();

                //初始化日志信息
                Log = new LogManager(this);
                (PageLayout as IActiveViewEvents_Event).FocusMapChanged += new IActiveViewEvents_FocusMapChangedEventHandler(GWorkspace_FocusMapChanged);
            }
        }

        void GWorkspace_FocusMapChanged()
        {
            IMap map = (PageLayout as IActiveView).FocusMap;
            for (int i = 0; i < MapManagers.Count; i++)
            {
                if (map == MapManagers[i].Map)
                {
                    FocusMapAt(i);
                    break;
                }
            }
        }
        internal static GWorkspace InitRaster(GApplication app, IWorkspace w, string filename)
        {
            GWorkspace ws = new GWorkspace(app, w, true);

            IDataset ds = (w as IRasterWorkspace2) as IDataset;
            IRasterDataset rasterdata = (w as IRasterWorkspace2).OpenRasterDataset(filename);
            IRaster raster = rasterdata.CreateDefaultRaster();
            IRasterLayer pRasterLayer = new RasterLayerClass();
            pRasterLayer.CreateFromRaster(raster);
            ILayer layer = pRasterLayer as ILayer;
            ws.LayerManager.Map.AddLayer(layer);

            //   ws.Save();

            return ws;
        }
        internal static GWorkspace Init(GApplication app, IWorkspace w)
        {
            if (IsWorkspace(w))
            {
                return Open(app, w);
            }
            Common.Config.CreateConfigTable(w);
            GWorkspace ws = new GWorkspace(app, w);

            if (w.IsDirectory())
                SetWorkspaceIcon(app, w.PathName);

            IDataset ds = w as IDataset;
            List<IFeatureClass> fcs = QueryFeatureClass(ds);
            foreach (var fc in fcs)
            {
                if (fc.FeatureType == esriFeatureType.esriFTAnnotation)
                {
                    IFeatureLayer fl = new FDOGraphicsLayerClass();
                    fl.FeatureClass = fc;
                    fl.Name = fc.AliasName;
                    ws.LayerManager.Map.AddLayer(fl);
                }
                else
                {
                    IFeatureLayer fl = new FeatureLayerClass();
                    fl.FeatureClass = fc;
                    fl.Name = fc.AliasName;
                    ws.LayerManager.Map.AddLayer(fl);
                }
            }
            ws.Save();

            return ws;
        }
        internal static GWorkspace Open(GApplication app, IWorkspace w)
        {
            if (IsWorkspace(w))
            {
                return new GWorkspace(app, w);
            }
            else
            {
                return Init(app, w);
            }
        }

        public static List<IFeatureClass> QueryFeatureClass(IDataset ds)
        {
            List<IFeatureClass> res = new List<IFeatureClass>();
            if (ds is IFeatureClass)
            {
                res.Add(ds as IFeatureClass);
            }
            else if (ds is IWorkspace)
            {
                IEnumDataset eds = ds.Subsets;
                IDataset cds = null;
                while (eds != null && (cds = eds.Next()) != null)
                {
                    res.AddRange(QueryFeatureClass(cds));
                }
            }
            else if (ds is IFeatureDataset)
            {
                IEnumDataset eds = ds.Subsets;
                IDataset cds = null;
                while (eds != null && (cds = eds.Next()) != null)
                {
                    res.AddRange(QueryFeatureClass(cds));
                }
            }
            return res;
        }

        internal static GWorkspace Open(Common.GApplication app, string path)
        {
            if (!IsWorkspace(path))
                return null;

            IWorkspaceFactory wf = QueryFactory(path);
            IWorkspace w = wf.OpenFromFile(path, 0);

            GWorkspace ws = new GWorkspace(app, w);

            return ws;
        }
        internal static GWorkspace CreateWorkspace(Common.GApplication app, string path)
        {
            //1.创建gdb工作区
            string dir = System.IO.Path.GetDirectoryName(path);
            string name = System.IO.Path.GetFileName(path);
            IWorkspaceName wname = Common.GApplication.GDBFactory.Create(dir, name, null, 0);
            IWorkspace w = (wname as IName).Open() as IWorkspace;
            //2.创建配置表
            Common.Config.CreateConfigTable(w);

            GWorkspace ws = new GWorkspace(app, w);
            //ws.LayerManager.CreateRootLayer();
            //ws.Map = new MapClass();
            //ws.Map.Name = ws.EsriWorkspace;
            ws.Save();
            SetWorkspaceIcon(app, w.PathName);
            return ws;
        }
        public static bool IsWorkspace(IWorkspace w)
        {
            return Common.Config.ExistConfigTable(w);
        }

        private static IWorkspaceFactory QueryFactory(string path)
        {
            IWorkspaceFactory wf = null;
            if (GApplication.GDBFactory.IsWorkspace(path))
            {
                wf = GApplication.GDBFactory;
            }
            else if (GApplication.MDBFactory.IsWorkspace(path))
            {
                wf = GApplication.MDBFactory;
            }
            return wf;
        }
        public static bool IsWorkspace(string path)
        {
            IWorkspaceFactory wf = QueryFactory(path);

            if (wf == null)
                return false;

            IWorkspace w = wf.OpenFromFile(path, 0);
            if (!Common.Config.ExistConfigTable(w))
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(w);
                return false;
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(w);
            return true;
        }
        private void RasterLoad()
        {           
            bool newWorkspace = (this.PageLayout == null);

            if (this.PageLayout == null)
            {
                this.PageLayout = new PageLayoutClass();
            }

            Application.PageLayoutControl.PageLayout = this.PageLayout;

            if (MapIDs == null)
            {
                MapIDs = new List<Guid>();
            }
            if (MapIDs.Count == 0)
            {
                MapIDs.Add(Guid.Empty);
            }
            //载入图层信息
            MapManagers = new List<Common.LayerManager>();

            foreach (var name in MapIDs)
            {
                MapManagers.Add(new LayerManager(this, name));
            }
            if (newWorkspace)
                SynsMap();         
        }
        private void Load()
        {
            autosave.Load();
            bool newWorkspace = (this.PageLayout == null);

            if (this.PageLayout == null)
            {
                this.PageLayout = new PageLayoutClass();
            }

            Application.PageLayoutControl.PageLayout = this.PageLayout;

            if (MapIDs == null)
            {
                MapIDs = new List<Guid>();
            }
            if (MapIDs.Count == 0)
            {
                MapIDs.Add(Guid.Empty);
            }
            //载入图层信息
            MapManagers = new List<Common.LayerManager>();

            foreach (var name in MapIDs)
            {
                MapManagers.Add(new LayerManager(this, name));
            }

            //ClearMapFrame();
            if (newWorkspace)
                SynsMap();

            //var c = PageLayout as IGraphicsContainer;
            //foreach (var item in LayerManagers)
            //{
            //    IClone cl = c.FindFrame(item.Map) as IClone;
            //    if (cl != null)
            //    {
            //        cl.Assign(item.MapFrame as IClone);
            //    }
            //}
        }


        private void ClearMapFrame()
        {
            var c = (PageLayout as IGraphicsContainer);
            c.Reset();
            IElement e = null;
            List<IElement> mes = new List<IElement>();
            while ((e = c.Next()) != null)
            {
                if (e is IMapFrame)
                    mes.Add(e);
            }
            foreach (var item in mes)
            {
                c.DeleteElement(item);
            }
        }

        private void SynsMap()
        {
            if (Application.MapControl.ActiveView.IsActive())
                Application.MapControl.ActiveView.Deactivate();
            if (!Application.PageLayoutControl.ActiveView.IsActive())
                Application.PageLayoutControl.ActiveView.Activate(Application.PageLayoutControl.hWnd);

            IMaps maps = new SMGIMaps();
            maps.Reset();
            foreach (var item in MapManagers)
            {
                maps.Add(item.Map);
            }

            PageLayout.ReplaceMaps(maps);


            Application.PageLayoutControl.ActiveView.FocusMap = this.Map;

            Application.PageLayoutControl.ActiveView.Deactivate();
            Application.MapControl.ActiveView.Activate(Application.MapControl.hWnd);

            //var c = PageLayout as IGraphicsContainer;
            //foreach (var item in MapManagers)
            //{
            //    var frame = c.FindFrame(item.Map);
            //    if(frame !=null)
            //        (frame as IElementProperties).CustomProperty = item.MapID.ToString();
            //}
        }
        public void RasterSave()
        {
            MapIDs.Clear();
            foreach (var item in MapManagers)
            {
                MapIDs.Add(item.MapID);
                item.Save();
            }
            
        }
        public event EventHandler WorkspaceSaved;
        public void Save()
        {
            MapIDs.Clear();
            foreach (var item in MapManagers)
            {
                MapIDs.Add(item.MapID);
                item.Save();
            }

            Log.Save();

            LastPath = EsriWorkspace.PathName;
            autosave.Save();

            if (WorkspaceSaved != null)
            {
                WorkspaceSaved(this, new EventArgs());
            }
        }

        public bool Close()
        {
            return true;
        }

        static void SetWorkspaceIcon(Common.GApplication app, string dir)
        {
            string IniText = "[.ShellClassInfo]\nIconResource={0},0";
            string iniPath = dir + "\\desktop.ini";
            string icoPath = dir + "\\full.ico";
            File.Copy(GApplication.ExePath + "\\full.ico", icoPath);

            //File.SetAttributes(iniPath, File.GetAttributes(dir)|FileAttributes.Hidden );
            File.SetAttributes(icoPath, File.GetAttributes(icoPath) | FileAttributes.Hidden);

            using (var writer = System.IO.File.CreateText(iniPath))
            {
                writer.Write(string.Format(IniText, "full.ico"));
            }

            File.SetAttributes(iniPath, File.GetAttributes(iniPath) | FileAttributes.Hidden | FileAttributes.System);
            File.SetAttributes(dir, File.GetAttributes(dir) | FileAttributes.System);
        }

    }
}

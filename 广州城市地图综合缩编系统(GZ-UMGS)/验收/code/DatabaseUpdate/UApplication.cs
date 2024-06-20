#define AAA
using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;


namespace DatabaseUpdate
{
    public class UApplication
    {
        internal static UApplication Application = new UApplication();
        //internal static SdeWorkspaceFactoryClass SDEWorkspaceFactory = new SdeWorkspaceFactoryClass();
        internal static FileGDBWorkspaceFactoryClass FileGDBWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
        public UWorkspace Workspace { get; private set; }
        //在数据连接里面设置
        public IApplication EsriApplication { get; set; }
        //在SyncMapWindows里面设置
        public AxMapControl EsriMapControl { get; set; }
        public IMap MxMap
        {
            get
            {
                return (Application.EsriApplication.Document as IMxDocument).ActiveView.FocusMap;
            }
        }
        public IMap McMap
        {
            get
            {
                return EsriMapControl.Map;
            }
        }
        public bool Available { get { return EsriApplication != null && EsriMapControl != null; } }
        private UApplication()
        {
            Workspace = null;
            EsriApplication = null;
        }
        public event Action<UWorkspace> WorkspaceOpened;
        /*public void Open(IPropertySet connectPropertySet) {
          Workspace = UWorkspace.Open(this, connectPropertySet);
          if (WorkspaceOpened != null) {
            WorkspaceOpened(this.Workspace);
          }
        }*/
        public void Open(string tpWSPath,int[] scales,ULayerInfos.LayerType[] layerTypes)
        {
          Workspace = UWorkspace.Open(this, tpWSPath, scales, layerTypes);
            if (WorkspaceOpened != null)
                WorkspaceOpened(this.Workspace);
        }

        public void SyncMap()
        {
            EsriMapControl.Extent = (EsriApplication.Document as IMxDocument).ActiveView.Extent;
        }
    }
    public class UWorkspace
    {
        //private IPropertySet conn;
        //public IWorkspace SDEWorkSpace { get; private set; }
        private string wsPath;
        public IWorkspace FileWorkSpace { get; private set; }
        public UApplication Application { get; private set; }
        public ITable LayerInfoTable { get; private set; }
        private ULayerInfos mcLayers;
        private ULayerInfos mxLayers;
        //private IMap NullMap { get; set; }
        public ULayerInfos McLayers
        {
            get
            {
                return mcLayers;
            }
            set
            {
                IMap m = this.Application.EsriMapControl.Map;
                if (value != null)
                {
                    foreach (var item in value.Layers.Values)
                    {
                        m.AddLayer(item);
                    }
                }
                if (mcLayers != null)
                {
                    foreach (var item in mcLayers.Layers.Values)
                    {
                        m.DeleteLayer(item);
                    }
                }
                this.mcLayers = value;
                this.Application.SyncMap();
            }
        }
        public ULayerInfos MxLayers
        {
            get
            {
                return mxLayers;
            }
            set
            {
                IMxDocument doc = this.Application.EsriApplication.Document as IMxDocument;
                var extent = doc.ActiveView.Extent;
                IMap m = doc.ActiveView.FocusMap;
                if (value != null)
                {
                    foreach (var item in value.Layers.Values)
                    {
                        m.AddLayer(item);
                    }
                }
                if (mxLayers != null)
                {
                    foreach (var item in mxLayers.Layers.Values)
                    {
                        m.DeleteLayer(item);
                    }
                }
                this.mxLayers = value;
                doc.ActiveView.Extent = extent;
                doc.ActiveView.Refresh();
            }
        }

        public Dictionary<int, ULayerInfos> DataBaseLayerInfos { get; private set; }
        public ULayerInfos UpdateLayerInfos { get; set; }
        public int[] Scales { get; private set; }
        public ULayerInfos.LayerType[] LayerTypes { get; set; }
        private UWorkspace(UApplication app, int[] scales, ULayerInfos.LayerType[] layerTypes)
        {
            this.Application = app;
            this.Scales = scales;
            this.LayerTypes = layerTypes;
            DataBaseLayerInfos = new Dictionary<int, ULayerInfos>();
        }
        //private void Open(IPropertySet connectPropertySet) {
        //  this.conn = connectPropertySet;
        //  SDEWorkSpace = UApplication.SDEWorkspaceFactory.Open(conn, Application.EsriApplication.hWnd);

        //  LayerInfoTable = OpenLayerInfoTable(SDEWorkSpace);

        //  int[] scales = GetScales(LayerInfoTable);
        //  DataBaseLayerInfos.Clear();
        //  foreach (var item in scales) {
        //    DataBaseLayerInfos[item] = new ULayerInfos(this, item);
        //  }
        //  this.UpdateLayerInfos = new ULayerInfos(this);
        //  this.McLayers = this.DataBaseLayerInfos[2000];
        //  this.MxLayers = this.UpdateLayerInfos;
        //}
        private void Open(string tpWSPath)
        {
            this.wsPath = tpWSPath;
            FileWorkSpace = UApplication.FileGDBWorkspaceFactory.OpenFromFile(wsPath, Application.EsriApplication.hWnd);
            
            DataBaseLayerInfos.Clear();
            foreach (var item in Scales)
            {
              DataBaseLayerInfos[item] = new ULayerInfos(this, item);
            }
            this.UpdateLayerInfos = new ULayerInfos(this);
            /*foreach (var item in Enum.GetNames(typeof(ULayerInfos.LayerType)))
            {
                string name = item;
                IMap tpMxMap = this.Application.MxMap;
                ILayer tpLayer = null;
                for (int m = 0; m < tpMxMap.LayerCount; m++)
                {
                    tpLayer = tpMxMap.get_Layer(m);
                    if (tpLayer.Name == item)
                    {
                        tpMxMap.DeleteLayer(tpLayer);
                        break;
                    }
                }
            }*/
            this.Application.MxMap.ClearLayers();
            this.Application.McMap.ClearLayers();
            foreach (var item in this.DataBaseLayerInfos.Values)
            {
              this.McLayers = item;
              break;
            }
            //this.McLayers = this.DataBaseLayerInfos.Values.GetEnumerator().Current;
            this.MxLayers = this.UpdateLayerInfos;
        }

        /*ITable OpenLayerInfoTable(IWorkspace workSpace) {
          return (workSpace as IFeatureWorkspace).OpenTable("SDE.GEODATASRELATIONS");
        }
        int[] GetScales(ITable table) {
          string scaleName = "比例尺";
          int scaleId = table.FindField(scaleName);
          Dictionary<int, int> dic = new Dictionary<int, int>();
          ICursor cursor = table.Search(null, true);
          IRow row = null;
          while ((row = cursor.NextRow()) != null) {
            int scale = Convert.ToInt32(row.get_Value(scaleId));
            dic[scale] = scale;
          }
          System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
          int[] ret = new int[dic.Keys.Count];
          dic.Keys.CopyTo(ret, 0);
          return ret;
        }
        static internal UWorkspace Open(UApplication app, IPropertySet connectPropertySet) {
          UWorkspace w = new UWorkspace(app);
          w.Open(connectPropertySet);
          return w;
        }*/
        static internal UWorkspace Open(UApplication app, string tppWSPath, int[] scales, ULayerInfos.LayerType[] layerTypes)
        {
          if (scales == null)
          {
            scales = new int[] { 2000 };
          }
            UWorkspace w = new UWorkspace(app,scales,layerTypes);
            w.Open(tppWSPath);
            return w;
        }
    }
    public class ULayerInfos
    {
        public enum LayerType : int
        {         
            植被面,
            水系面,
            房屋面,
            道路面,
            道路中心线,
            POI点
        }

        public Dictionary<string, IFeatureLayer> Layers;
        public UWorkspace Workspace { get; private set; }
        public bool DataBaseLayers { get; private set; }
        public int Scale { get; set; }
        public ULayerInfos(UWorkspace workspace, int scale)
        {
            this.Layers = new Dictionary<string, IFeatureLayer>();
            this.Scale = scale;
            this.Workspace = workspace;
            this.DataBaseLayers = true;
            InitSDE();
        }

        public ULayerInfos(UWorkspace workspace)
        {
            this.Layers = new Dictionary<string, IFeatureLayer>();
            this.Workspace = workspace;
            this.Scale = 0;
            this.DataBaseLayers = false;
            IMap tpMxMap = this.Workspace.Application.MxMap;
            /*foreach (var item in Enum.GetNames(typeof(LayerType)))
            {
                string name = item;               
                ILayer tpLayer = null;
                for (int m = 0; m < tpMxMap.LayerCount; m++)
                {
                    tpLayer = tpMxMap.get_Layer(m);
                    if (tpLayer.Name == item)
                    {
                        RenderForUpdate.RenderLayer(tpLayer);
                        this.Layers[item] = tpLayer as IFeatureLayer;
                        break;
                    }
                }
            }*/
            for (int m = 0; m < tpMxMap.LayerCount; m++)
            {
                ILayer tpLayer = null;
                tpLayer = tpMxMap.get_Layer(m);
                bool isUpdateLayer = false;
                foreach (var item in Enum.GetNames(typeof(LayerType)))
                {
                    string name = item;
                    if (tpLayer.Name == name)
                    {
                        RenderForUpdate.RenderLayer(tpLayer);
                        this.Layers[item] = tpLayer as IFeatureLayer;
                        isUpdateLayer = true;
                        break;
                    }
                }
                if(!isUpdateLayer)
                    this.Layers[tpLayer.Name] = tpLayer as IFeatureLayer;
            }
        }
        private void InitSDE()
        {
            //ITable layerInfotable = this.Workspace.LayerInfoTable;
            //IQueryFilter qf = new QueryFilterClass();
            foreach (var item in Workspace.LayerTypes)
            {
                /*qf.WhereClause = string.Format("地物要素 = '{0}' and 比例尺 = {1}", item, Scale.ToString());
                ICursor cursor = layerInfotable.Search(qf, true);
                IRow row = cursor.NextRow();
                if (row == null) {
                  System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                  continue;
                }

                string name = row.get_Value(row.Fields.FindField("图层名称")).ToString();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);*/
                
                IFeatureLayer featurelayer = new FeatureLayerClass();
                //featurelayer.Name = item;
                featurelayer.Name = string.Format("{0}{1}",item,Scale.ToString());
                string name = featurelayer.Name;
                IFeatureClass fc = (Workspace.FileWorkSpace as IFeatureWorkspace).OpenFeatureClass(name);
                featurelayer.FeatureClass = fc;
                featurelayer.Name = item.ToString();
                RenderForUpdate.RenderLayer(featurelayer);
                featurelayer.Name = name;
                this.Layers[item.ToString()] = featurelayer;
            }
        }
    }
}

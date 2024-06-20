using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Display;

namespace BuildingGen {
    [Serializable]
    public enum GCityLayerType {
        建筑物,
        道路,
        水系,
        植被,
        铁路,
        高架,
        工矿,
        禁测,
        BRT交通面,
        绿化岛,
        POI
    }
    [Serializable]
    public class GLayerInfo {
        internal int index;
        internal int orgIndex;
        internal string featureClassName;
        public GCityLayerType LayerType;
        public double Scale = 10000;
        public object otherInfo;
        [NonSerialized]
        public ILayer Layer;
        [NonSerialized]
        public ILayer OrgLayer;
        public override string ToString() {
            return Layer.Name;
        }
    }

    public class GLayerManager {
        GWorkspace ws;
        public List<GLayerInfo> Layers { get; internal set; }
        public event EventHandler LayerChanged;
        internal GLayerManager(GWorkspace workspace) {
            ws = workspace;
            Layers = new List<GLayerInfo>();
            LoadLayers();
        }
        internal void LoadLayers() {
            GConfig config = ws.MapConfig;
            IMap map = config["_gmap"] as IMap;
            GLayerInfo[] layers = config["_glayers"] as GLayerInfo[];

            object editLayerIndex = config["_geditlayer"];

            if (map != null || layers != null) {
                Layers.AddRange(layers);
                foreach (GLayerInfo info in Layers) {
                    if (info.index >= 0 && info.index < map.LayerCount) {
                        info.Layer = map.get_Layer(info.index);
                        if (info.Layer is IFeatureLayer) {
                            IFeatureClass fc = (ws.Workspace as IFeatureWorkspace).OpenFeatureClass(info.featureClassName);
                            (info.Layer as IFeatureLayer).FeatureClass = fc;
                        }
                        if (editLayerIndex != null && info.index == Convert.ToInt32(editLayerIndex)) {
                            ws.EditLayer = info;
                        }
                    }
                    if (info.orgIndex >= 0 && info.orgIndex < map.LayerCount) {
                        info.OrgLayer = map.get_Layer(info.orgIndex);
                    }
                }
                if (map.MapUnits == ESRI.ArcGIS.esriSystem.esriUnits.esriUnknownUnits) {
                    map.MapUnits = ESRI.ArcGIS.esriSystem.esriUnits.esriMeters;
                }
                for (int kk = 0; kk < map.LayerCount; kk++)//BY YWH
                {
                    ILayer tpLayer = map.get_Layer(kk);
                    if (tpLayer.Name.Contains("植被面"))
                        map.MoveLayer(tpLayer, map.LayerCount - 1);
                }
                    ws.Map = map;
            }
        }
        internal void Save() {
            GConfig config = ws.MapConfig;
            SyncInfo();
            config["_gmap"] = ws.Map;
            config["_glayers"] = this.Layers.ToArray();
            if (ws.EditLayer != null) {
                config["_geditlayer"] = ws.EditLayer.index;
            }
        }
        private void SyncInfo() {
            IMap map = ws.Map;
            Dictionary<ILayer, int> dic = new Dictionary<ILayer, int>();
            for (int i = 0; i < map.LayerCount; i++) {
                dic.Add(map.get_Layer(i), i);
            }
            foreach (GLayerInfo info in Layers) {
                info.index = dic[info.Layer];
                if (info.Layer is IFeatureLayer) {
                    info.featureClassName = ((info.Layer as IFeatureLayer).FeatureClass as IDataset).Name;
                } else {
                    info.featureClassName = string.Empty;
                }
                if (info.OrgLayer == null) {
                    info.orgIndex = -1;
                } else {
                    info.orgIndex = dic[info.OrgLayer];
                }
            }
        }

        public void AddLayer(string WorkspacePath, string featureClassName, string name, GCityLayerType layertype) {
            IWorkspace ws = null;
            if (GApplication.ShpFactory.IsWorkspace(WorkspacePath)) {
                ws = GApplication.ShpFactory.OpenFromFile(WorkspacePath, 0);
            }
            if (GApplication.MDBFactory.IsWorkspace(WorkspacePath)) {
                ws = GApplication.MDBFactory.OpenFromFile(WorkspacePath, 0);
            }
            if (GApplication.GDBFactory.IsWorkspace(WorkspacePath)) {
                ws = GApplication.GDBFactory.OpenFromFile(WorkspacePath, 0);
            }
            string orgLayerName = name + "_原始层";
            GLayerInfo info = AddLayer(ws, featureClassName, orgLayerName, null, layertype);
            string genLayerName = name + "_综合层";
            GLayerInfo genInfo = AddLayer(this.ws.Workspace, orgLayerName, genLayerName, info.Layer, layertype);
            IFeatureLayer genLayer = (genInfo.Layer as IFeatureLayer);
            IFeatureClass fc = genLayer.FeatureClass;
            int usedid = fc.FindField("_GenUsed");
            if (usedid == -1) {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "_GenUsed";
                field.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
                //field.IsNullable_2 = false;
                field.DefaultValue_2 = "-1";
                fc.AddField(field);
            }

            this.ws.Save();
            if (LayerChanged != null) {
                LayerChanged(this, new EventArgs());
            }
        }
        #region 废弃
        //private void CreateGenLayer(GLayerInfo info, bool copyData) 
        //{
        //    string featureClassName = info.Layer.Name + "(综合层)";
        //    if (copyData)
        //    {
        //        AddLayer(ws.Workspace, (info.Layer as IFeatureLayer).FeatureClass.AliasName, featureClassName, info.Layer, info.LayerType);
        //    } 
        //    else 
        //    {
        //        IFeatureClass fc = CreateFeatureClassByCopyStructure((info.Layer as IFeatureLayer).FeatureClass, featureClassName);
        //        IFeatureLayer l = new FeatureLayerClass();
        //        l.FeatureClass = fc;
        //        l.Name = featureClassName;

        //        GLayerInfo layerInfo = new GLayerInfo();
        //        layerInfo.Layer = l;
        //        layerInfo.featureClassName = featureClassName;
        //        layerInfo.OrgLayer = info.Layer;
        //        ws.Map.AddLayer(l);
        //        this.Layers.Add(layerInfo);
        //    }
        //    this.ws.Save();
        //    if (LayerChanged != null) {
        //        LayerChanged(this, new EventArgs());
        //    }
        //}

        //private IFeatureClass CreateFeatureClassByCopyStructure(IFeatureClass fc, string name)
        //{
        //    IFeatureWorkspace fws = ws.Workspace as IFeatureWorkspace;
        //    IFields fields = (fc.Fields as ESRI.ArcGIS.esriSystem.IClone).Clone() as IFields;
        //    IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
        //    IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
        //    return fws.CreateFeatureClass(name, fields, ocDescription.InstanceCLSID, ocDescription.ClassExtensionCLSID, fcDescription.FeatureType, fc.ShapeFieldName, "");
        //}
        #endregion
        public void AddExistLayer(string featureClassName, ILayer orgLayer) {
            IFeatureClass fc = (ws.Workspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
            IFeatureLayer l = new FeatureLayerClass();
            l.FeatureClass = fc;
            l.Name = featureClassName;
            GLayerInfo layerInfo = new GLayerInfo();
            layerInfo.Layer = l;
            layerInfo.featureClassName = featureClassName;
            layerInfo.OrgLayer = orgLayer;
            ws.Map.AddLayer(l);
            this.Layers.Add(layerInfo);
            this.ws.Save();
            if (LayerChanged != null) {
                LayerChanged(this, new EventArgs());
            }
        }

        public bool NameExist(string name) {
            return (ws.Workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name)
                || (ws.Workspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, name);
        }

        public string TempLayerName() {
            string name = "";
            for (int i = 0; true; i++) {
                name = "tp" + i;
                if (!NameExist(name)) {
                    break;
                }
            }
            return name;
        }

        public IFeatureClass TempLayer(List<IField> fields) {
            IFeatureWorkspace fws = ws.Workspace as IFeatureWorkspace;
            string name = TempLayerName();
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFields rfields = ocDescription.RequiredFields;
            if (fields != null)
                foreach (IField f in fields) {
                    (rfields as IFieldsEdit).AddField(f);
                }
            try {
                return fws.CreateFeatureClass(name, rfields,
                    ocDescription.InstanceCLSID, ocDescription.ClassExtensionCLSID,
                    fcDescription.FeatureType, fcDescription.ShapeFieldName, "");
            } catch {
                return null;
            }
        }

        /// <summary>
        /// 向工作区中添加图层
        /// </summary>
        /// <param name="srcWorkSpace">源工作区</param>
        /// <param name="featureClassName">源图层名</param>
        /// <param name="name">加入后名称</param>
        /// <param name="orgLayer">原始图层(如果为空则为源图层，反之为综合层)</param>
        /// <param name="layertype">图层类型</param>
        private GLayerInfo AddLayer(IWorkspace srcWorkSpace, string featureClassName, string name, ILayer orgLayer, GCityLayerType layertype) {
            GLayerInfo layerInfo = new GLayerInfo();

            IFeatureLayer featureLayer = AddLayerToWorkspace(srcWorkSpace, featureClassName, name, (orgLayer == null));
            featureLayer.Visible = (orgLayer != null);
            featureLayer.Selectable = (orgLayer != null);
            layerInfo.Layer = featureLayer;
            layerInfo.OrgLayer = orgLayer;
            layerInfo.LayerType = layertype;
            if (layerInfo.Layer != null) {
                ws.Map.AddLayer(layerInfo.Layer);
            }

            Layers.Add(layerInfo);
            return layerInfo;
        }

        /// <summary>
        /// 复制当前图层到工作区中
        /// </summary>
        /// <param name="srcWorkSpace">源工作区</param>
        /// <param name="featureClassName">源图层名称</param>
        /// <param name="dstFeatureClassName">目标图层名称</param>
        /// <returns>返回一个已经加好的ILayer</returns>
        private IFeatureLayer AddLayerToWorkspace(IWorkspace srcWorkSpace, string featureClassName, string dstFeatureClassName, bool check) {
            IFeatureClass fc = ConvertFeatureClassEx(srcWorkSpace, featureClassName, ws.Workspace, dstFeatureClassName, check);
            IFeatureLayer featureLayer = new FeatureLayerClass();
            //featureLayer.Name = featureClassName;
            featureLayer.FeatureClass = fc;
            featureLayer.Name = dstFeatureClassName;

            return featureLayer;
        }


        private IFeatureClass ConvertFeatureClassEx(IWorkspace sourceWorkspace, string orgFeatureClass, IWorkspace targetWorkspace, string dstFeatureClass, bool check) {
            Geoprocessor gp = ws.Application.Geoprosessor;
            gp.SetEnvironmentValue("OutputZFlag", "disabled");
            gp.SetEnvironmentValue("OutputMFlag", "disabled");
            FeatureClassToFeatureClass F2F = new FeatureClassToFeatureClass();
            F2F.in_features = ((IFeatureWorkspace)sourceWorkspace).OpenFeatureClass(orgFeatureClass);
            F2F.out_path = targetWorkspace.PathName;
            F2F.out_name = dstFeatureClass;
            gp.OverwriteOutput = true;
            gp.Execute(F2F, null);


            IFeatureWorkspace fw = targetWorkspace as IFeatureWorkspace;
            IFeatureClass fc = fw.OpenFeatureClass(dstFeatureClass);
            if (false) {
                RepairGeometry rg = new RepairGeometry();
                //rg.delete_null = true.ToString();
                rg.in_features = fc;
                gp.Execute(rg, null);
            }
            return fc;
        }
    }
}

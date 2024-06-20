using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Xml.Serialization;
using ESRI.ArcGIS.esriSystem;
using System.IO;

namespace SMGI.Common
{
    [Serializable]
    public class LayerDictionaryItem
    {
        private string layerName = "";
        //数据库中名称
        public string LayerName
        {
            get { return layerName; }
            set { layerName = value; }
        }

        private string layerCaption = "";
        //图层名称
        public string LayerCaption
        {
            get { return layerCaption; }
            set { layerCaption = value; }
        }

        private string layerGroup = "";
        //图层分组
        public string LayerGroup
        {
            get { return layerGroup; }
            set { layerGroup = value; }
        }

        private string layerGeoType = "";
        //几何类型, 点、线、面
        public string LayerGeoType
        {
            get { return layerGeoType; }
            set { layerGeoType = value; }
        }


        private List<GBItem> gbitems = null;
        public List<GBItem> Gbitems
        {
            get
            {
                return gbitems;
            }
            set
            {
                gbitems = value;

            }
        }
        public bool GBExists(string key)
        {
            if (gbs.Count == 0)
            {
                foreach (GBItem item in gbitems)
                {
                    if (!gbs.ContainsKey(item.GBCode))
                    {
                        gbs.Add(item.GBCode, item);
                    }
                }
            }
            return gbs.ContainsKey(key);
        }

        [NonSerialized]
        private Dictionary<string, GBItem> gbs = null;
        //public Dictionary<string, GBItem> GBs
        //{
        //    get { return gbs; }
        //    set { gbs = value; }
        //}

        public void AddGB(GBItem gi)
        {
            if (!gbs.ContainsKey(gi.GBCode))
            {
                gbs.Add(gi.GBCode, gi);
                gbitems.Add(gi);
            }
            else
            {
                System.Console.WriteLine(string.Format("{0} is Exist in {1}", gi.GBCode, layerName));
            }
        }

        public LayerDictionaryItem()
        {
            gbs = new Dictionary<string, GBItem>();
            gbitems = new List<GBItem>();

        }
    }


    public struct GBItem
    {

        public string GBCode;
        public string GBName;
        public string GBGeometry;
    }

    [Serializable]
    public class LayerDictionary
    {
        private string dictName = "";
        public string DictName
        {
            get { return dictName; }
            set { dictName = value; }
        }

        private List<LayerDictionaryItem> items = null;
        public List<LayerDictionaryItem> Items
        {
            get { return items; }
            set
            {
                items = value;
            }
        }


        public LayerDictionary()
        {
            items = new List<LayerDictionaryItem>();
        }

        public void AddItem(string lyrname, string lyrgroup, string lyrCaption, string lyrgeo, GBItem gi)
        {
            LayerDictionaryItem targetItem = null;
            if ((targetItem = ItemByName(lyrname)) != null)
            {
                targetItem.AddGB(gi);
            }
            else
            {
                targetItem = new LayerDictionaryItem();
                targetItem.LayerName = lyrname; targetItem.LayerGroup = lyrgroup;
                targetItem.LayerGeoType = lyrgeo; targetItem.LayerCaption = lyrCaption;
                targetItem.AddGB(gi);

                items.Add(targetItem);
            }
        }

        public LayerDictionaryItem ItemByName(string key)
        {
            LayerDictionaryItem ldi = null;
            try
            {
                ldi = items.Find(new Predicate<LayerDictionaryItem>(x =>
                {
                    return x.LayerName == key;
                }));
            }
            catch
            {

            }
            return ldi;
        }

        public LayerDictionaryItem ItemByCaption(string key)
        {
            LayerDictionaryItem ldi = null;
            try
            {
                ldi = items.Find(new Predicate<LayerDictionaryItem>(x =>
                {
                    return x.LayerCaption == key;
                }));
            }
            catch
            {

            }
            return ldi;
        }

        /// <summary>
        /// 根据点、线、面 +GB 码 得到所在的图层
        /// </summary>
        /// <param name="gbkey">GB码 </param>
        /// <param name="geoType"> 点，线，面 </param>
        /// <returns></returns>
        public string[] getLayersForGB(string gbkey, string geoType)
        {
            List<string> layers = new List<string>();
            foreach (LayerDictionaryItem item in items)
            {
                if (item.LayerGeoType == geoType && item.GBExists(gbkey))
                {
                    layers.Add(item.LayerName);
                }
            }
            return layers.ToArray();
        }
        /// <summary>
        /// 返回图层内所有的国标码
        /// </summary>
        /// <param name="lyrNameKey"></param>
        /// <returns></returns>
        private GBItem[] getGBForLayer(string lyrNameKey)
        {

            List<GBItem> gbs = new List<GBItem>();
            LayerDictionaryItem ldi;
            try
            {
                ldi = items.Find(new Predicate<LayerDictionaryItem>(x => { return x.LayerName == lyrNameKey; }));
                gbs.AddRange(ldi.Gbitems);
            }
            catch
            {

            }
            return gbs.ToArray();
        }

        public void Save2String()
        {

        }

    }

    [Serializable]
    public class CartoMapConfig
    {
        private string cfgName = string.Empty;
        public string CfgName
        {
            get { return cfgName; }
            set { cfgName = value; }
        }

        private string cfgDescription = "";
        public string CfgDescription
        {
            get { return cfgDescription; }
            set { cfgDescription = value; }
        }
        public bool SupportMapLevel { get; set; }

        private string mapTitle1 = "Title1:WHU";
        private string mapTitle2 = "Title2:WHU";
        private string mapTitle3 = "Title3:WHU";
        private long m_scale = 50000;
        private string extentLayer = "BOUA";


        private List<CartoLayerConfig> layers;
        public List<CartoLayerConfig> Layers
        {
            get { return layers; }
            set { layers = value; }
        }
        public CartoMapConfig() { SupportMapLevel = true; }

        public GApplication App { set { app = value; } }

        [NonSerialized]
        private SMGI.Common.GApplication app;
        public CartoMapConfig(SMGI.Common.GApplication ap)
        {
            app = ap; SupportMapLevel = true;
        }

        private IMap m_map = null;
        public IMap ESRIMap { get { return m_map; } }

        private string mapstring = "";
        public string MapString
        {
            get
            {
                if (m_map == null)
                {
                    return "";
                }
                else
                {
                    return Common.GConvert.ObjectToBase64(m_map);
                }
            }
            set
            {
                m_map = Common.GConvert.Base64ToObject(value) as IMap;
            }
        }

        //初始化
        public void InitFromMap(IMap map)
        {
            m_map = map;
            InitFromLayers();
        }

        public void InitFromMxdDoc(string document)
        {
            try
            {
                IMap m = CartoMapConfig.openMapDoc(document);
                if (m != null)
                {
                    InitFromMap(m);
                }
            }
            catch
            {

            }
        }

        public void InitFromLyrFile(string fileName)
        {
            ILayerFile lf = new LayerFileClass();
            lf.Open(fileName);
            if (lf.Layer != null)
            {
                IMap m = new MapClass();
                m.AddLayer(lf.Layer);
                InitFromMap(m);
                this.SupportMapLevel = false;
            }
        }

        public static IMap openMapDoc(string mapStr)
        {
            IMapDocument mapDoc = new MapDocumentClass();
            IMap map = null;
            if (File.Exists(mapStr) &&
                mapDoc.get_IsMapDocument(mapStr) &&
                !mapDoc.get_IsReadOnly(mapStr))
            {
                try
                {
                    mapDoc.Open(mapStr);
                    map = mapDoc.get_Map(0);
                    map = (map as IClone).Clone() as IMap;
                    mapDoc.Close();
                }
                catch
                {

                }
            }
            return map;
        }

        public string GetMatchLayer(string lyrName)
        {
            string matchName = String.Empty;
            if (layers != null && layers.Count > 0)
            {
                foreach (CartoLayerConfig cl in layers)
                {
                    if (cl.LayerName == lyrName || cl.LayerCaption == lyrName)
                    {
                        matchName = cl.LayerCaption;
                        break;
                    }
                }
            }
            return matchName;
        }

        private void InitFromLayers()
        {
            try
            {
                if (m_map == null)
                {
                    return;
                }

                List<ILayer> inputLayers = getAllLayers(m_map);
                if (inputLayers != null && inputLayers.Count > 0)
                {
                    layers = new List<CartoLayerConfig>();
                    foreach (ILayer l in inputLayers)
                    {
                        CartoLayerConfig clc = new CartoLayerConfig(l);
                        LayerDictionaryItem dictItem = app.GBsAndLayerInfo.GetLayerItem(l.Name);
                        if (dictItem != null)
                        {
                            clc.LayerName = dictItem.LayerName;
                            clc.LayerCaption = dictItem.LayerCaption;
                            clc.LayerGroup = dictItem.LayerGroup;
                        }
                        else
                        {
                            clc.LayerName = l.Name;
                            clc.LayerCaption = l.Name;
                            clc.LayerGroup = l.Name;
                        }

                        if ((l is IGeoFeatureLayer) &&
                            (l as IGeoFeatureLayer).Renderer is IRepresentationRenderer)
                        {
                            clc.LayerRenderInfo.RepValues = ImportRepresentationRuleToStyle(
                                 (l as IGeoFeatureLayer).Renderer as IRepresentationRenderer,
                                 (l as IFeatureLayer).FeatureClass.ShapeType,
                                 app.StyleMgr);
                            clc.LayerRenderInfo.IsConvertRep = true;
                        }

                        layers.Add(clc);
                    }
                }
            }
            catch (Exception eex)
            {

            }
        }


        private List<ILayer> getAllLayers(IMap map)
        {
            //Ilayer
            return LoopThroughLayersOfSpecificUID(map, "{34C20002-4D3C-11D0-92D8-00805F7C28B0}");
        }

        public List<ILayer> LoopThroughLayersOfSpecificUID(ESRI.ArcGIS.Carto.IMap map, System.String layerCLSID)
        {
            List<ILayer> lyrs = new List<ILayer>();
            if (map == null || layerCLSID == null)
            {
                return lyrs;
            }
            ESRI.ArcGIS.esriSystem.IUID uid = new ESRI.ArcGIS.esriSystem.UIDClass();
            uid.Value = layerCLSID; // Example: "{E156D7E5-22AF-11D3-9F99-00C04F6BC78E}" = IGeoFeatureLayer
            try
            {
                ESRI.ArcGIS.Carto.IEnumLayer enumLayer =
                    map.get_Layers(((ESRI.ArcGIS.esriSystem.UID)(uid)), true); // Explicit Cast 
                enumLayer.Reset();
                ESRI.ArcGIS.Carto.ILayer layer = enumLayer.Next();
                while (!(layer == null))
                {
                    lyrs.Add(layer);
                    layer = enumLayer.Next();
                }
            }
            catch (System.Exception ex)
            {

            }
            return lyrs;
        }


        public void InitFromStyleGB(ILayer[] inputLayers, StyleManager styleMgr, string fieldName)
        {
            if (inputLayers == null || styleMgr == null)
            {
                return;
            }
            foreach (var lyr in inputLayers)
            {
                if (lyr != null && lyr is IFeatureLayer)
                {
                    int idx = -1;
                    if ((idx = (lyr as IFeatureLayer).FeatureClass.FindField(fieldName)) < 0)
                    {
                        //RenderFeatureLayer.SimpleRenderLayer(lyr as IFeatureLayer,
                        //RenderFeatureLayer.createDefaultSimpleSymbol((lyr as IFeatureLayer).FeatureClass.ShapeType), "无GB字段");

                    }
                    else
                    {
                        IFeatureClass fcls = (lyr as IFeatureLayer).FeatureClass;
                        IFeatureCursor fcur = fcls.Search(null, true);
                        //这里的GB码从知识库里获得
                        List<string> vals = app.GBsAndLayerInfo.GetGBsForLayer(lyr.Name);
                        List<string> valsInData = RenderFeatureLayer.CalculateUniqueValuesSMGI(fcur as ICursor, idx);

                        foreach (string v in valsInData)
                        {
                            if (!vals.Contains(v))
                            {
                                vals.Add(v);
                            }
                        }


                        System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur);

                        Dictionary<string, string> codebase =
                            new Dictionary<string, string>();
                        SymbolKnowledgeItem it;
                        foreach (string item in vals)
                        {
                            try
                            {
                                it = app.StyleKnowledgeBases.SymbolsBase.Find(new Predicate<SymbolKnowledgeItem>(x =>
                                {
                                    return x["GB"].ToString() == item;
                                }));
                            }
                            catch
                            {
                                continue;
                            }

                            if (!codebase.ContainsKey(item))
                            {
                                codebase.Add(item, it == null ? item : it["StyleID"].ToString());
                            }

                        }

                        if (vals.Count > 0)
                        {
                            RenderFeatureLayer.UniqueValueRenderLayer(lyr as IFeatureLayer, fieldName,
                                codebase, styleMgr);
                        }
                        else
                        {
                            RenderFeatureLayer.SimpleRenderLayer(lyr as IFeatureLayer,
                               RenderFeatureLayer.createErrorSimpleSymbol((lyr as IFeatureLayer).FeatureClass.ShapeType), "无GB字段");

                        }
                    }


                }
                else
                {

                }
            }
        }

        public List<string> ImportRepresentationRuleToStyle(IRepresentationRenderer pRepRenderer,
            esriGeometryType geoType, StyleManager styMgr)
        {
            List<string> listStr = new List<string>();
            try
            {
                IRepresentationClass pRepClass;
                pRepClass = pRepRenderer.RepresentationClass;
                IRepresentationRules pRepRules;
                IRepresentationRule pRepRule;
                pRepRules = pRepClass.RepresentationRules;
                IRepresentationRuleItem ruleItem;
                int pID = 0;
                string sName = "";
                pRepRules.Reset();
                pRepRules.Next(out pID, out pRepRule);
                while (pRepRule != null)
                {
                    try
                    {
                        ruleItem = new RepresentationRuleItem();
                        sName = pRepRules.get_Name(pID);
                        if (geoType == esriGeometryType.esriGeometryPoint)
                        {
                            ruleItem.GeometryType = esriGeometryType.esriGeometryPoint;
                        }
                        if (geoType == esriGeometryType.esriGeometryPolyline)
                        {
                            ruleItem.GeometryType = esriGeometryType.esriGeometryLine;
                        }
                        if (geoType == esriGeometryType.esriGeometryPolygon)
                        {
                            ruleItem.GeometryType = esriGeometryType.esriGeometryPolygon;
                        }
                        ruleItem.RepresentationRule = pRepRule;

                        styMgr.addRepRule(styMgr.DefaultStylePath, ruleItem, sName);
                        listStr.Add(sName);
                        pRepRules.Next(out pID, out pRepRule);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {

            }
            return listStr;

        }





        //匹配

        public void MatchConfig(Dictionary<string, string> lyrStrs, bool maplevel, bool lyrOrder)
        {
            //map level
            if (this.m_map == null || app == null) { return; }
            if (maplevel) { MapLevelMatch(m_map, app.Workspace.Map); }

            //layer level
            Dictionary<string, ILayer> lyrPosition = new Dictionary<string, ILayer>();
            foreach (string tl in lyrStrs.Keys)
            {
                try
                {
                    LayerInfo[] TargetLyr = app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
                    {
                        return x.Layer.Name == tl;
                    }));

                    CartoLayerConfig clc = layers.Find(new Predicate<CartoLayerConfig>(x =>
                    {
                        return x.LayerCaption == lyrStrs[tl];
                    }));

                    if (clc == null) { continue; }
                    foreach (LayerInfo l in TargetLyr)
                    {
                        l.Layer.Visible = clc.FeatLayer.Visible;
                        if (l.Layer is IFeatureLayer)
                        {
                            if (l.Layer is IGeoFeatureLayer)
                            {
                                CopyFeatureLayer(clc, l.Layer as IFeatureLayer);
                            }
                            else if (l.Layer is IFDOGraphicsLayer)
                            {

                            }
                            else
                            {

                            }
                        }
                        else if (l.Layer is IRasterLayer)
                        {

                        }
                        else { }

                        l.Layer.Name = lyrStrs[tl];
                        lyrPosition.Add(lyrStrs[tl], l.Layer);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("图层 " + tl + " 匹配失败，错误提示是：" + ex.Message);
                    continue;
                }
            }

            if (!lyrOrder) { return; }
            foreach (CartoLayerConfig lyr in this.layers)
            {
                if (lyrPosition.ContainsKey(lyr.LayerCaption))
                {
                    (app.Workspace.Map as IMapLayers).MoveLayer(lyrPosition[lyr.LayerCaption], layers.IndexOf(lyr));
                }
            }
        }

        public void AdjustMatchRelationship(Dictionary<string, string> dicts)
        {
            if (dicts == null) { return; }
            foreach (string tl in dicts.Keys)
            {
                CartoLayerConfig clc = layers.Find(new Predicate<CartoLayerConfig>(x =>
                {
                    return x.LayerCaption == dicts[tl];
                }));

                if (clc == null) { continue; }
                clc.LayerName = tl;
            }
        }

        private void MapLevelMatch(IMap src, IMap target)
        {
            target.AnnotationEngine = src.AnnotationEngine;
            (target as IMapOverposter).OverposterProperties =
                ((src as IMapOverposter).OverposterProperties as IClone).Clone() as IOverposterProperties;

        }

        public void CopyFeatureLayer(CartoLayerConfig src, IFeatureLayer tgt, string keyField = "GB")
        {
            try
            {
                if (src != null && tgt != null)
                {
                    if ((src.FeatLayer as IGeoFeatureLayer).Renderer is ISimpleRenderer ||
                        (src.FeatLayer as IGeoFeatureLayer).Renderer is IUniqueValueRenderer)
                    {
                        (tgt as IGeoFeatureLayer).Renderer = (src.FeatLayer as IGeoFeatureLayer).Renderer;
                        (tgt as IGeoFeatureLayer).DisplayAnnotation = (src.FeatLayer as IGeoFeatureLayer).DisplayAnnotation;
                        (tgt as IGeoFeatureLayer).AnnotationProperties = (src.FeatLayer as IGeoFeatureLayer).AnnotationProperties;
                        (tgt as IGeoFeatureLayer).DisplayField = (src.FeatLayer as IGeoFeatureLayer).DisplayField;
                        (tgt as ISymbolLevels).UseSymbolLevels = (src.FeatLayer as ISymbolLevels).UseSymbolLevels;

                    }
                    else if (src.LayerRenderInfo.IsConvertRep &&
                         src.LayerRenderInfo.RepValues != null &&
                       src.LayerRenderInfo.RepValues.Count > 0)
                    {
                        ReproduceRep(src, tgt, keyField);
                    }

                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("图层 " + tgt.Name + " 匹配失败，错误提示是：" + ex.Message);
            }
        }

        public void ReproduceRep(CartoLayerConfig src, IFeatureLayer flnew, string filedName)
        {
            IRepresentationClass repCls = null;
            int idx_GB = flnew.FeatureClass.FindField(filedName);
            if (idx_GB > 0)
            {
                repCls = app.Workspace.LayerManager.RetriveRepClassFromStyle(flnew.FeatureClass, src.LayerRenderInfo.RepValues);
                if (repCls != null)
                {
                    IFeatureCursor featCursor = flnew.FeatureClass.Update(null, false);
                    IFeature feat;
                    IRepresentationRules pRules = repCls.RepresentationRules;
                    int pID = 0;
                    IRepresentationRule pRule = null;

                    while ((feat = featCursor.NextFeature()) != null)
                    {
                        string name = feat.get_Value(idx_GB).ToString();
                        pRules.Reset();
                        pRules.Next(out pID, out pRule);
                        while (pRule != null)
                        {
                            string ruleName = pRules.get_Name(pID);
                            if (name.Equals(ruleName))
                            {
                                feat.set_Value(repCls.RuleIDFieldIndex, pID);
                                featCursor.UpdateFeature(feat);
                                break;
                            }
                            pRules.Next(out pID, out pRule);
                        }
                    }

                    featCursor.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);
                    IRepresentationRenderer repRenderer = new RepresentationRendererClass();
                    repRenderer.RepresentationClass = repCls;
                    (flnew as IGeoFeatureLayer).Renderer = repRenderer as IFeatureRenderer;
                }
            }
        }





        //*******************************静态函数
        public static List<CartoMapConfig> loadConfig(GApplication app)
        {
            List<CartoMapConfig> cfgList = new List<CartoMapConfig>();
            try
            {
                string configMap = app.AppConfig["地图配置.默认"].ToString();
                CartoMapConfig[] cfgs = Common.GConvert.Base64ToObject(configMap)
                      as CartoMapConfig[];
                if (cfgs != null && cfgs.Length > 0)
                {
                    cfgList.AddRange(cfgs);
                }
            }
            catch { }
            return cfgList;
        }
        public static void saveConfig(GApplication app, CartoMapConfig[] cfgs)
        {
            app.AppConfig["地图配置.默认"] = Common.GConvert.ObjectToBase64(cfgs);
        }



    }
}

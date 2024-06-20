using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
//using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Collections;
using ESRI.ArcGIS.CartographyTools;
using ESRI.ArcGIS.Maplex;
using System.Windows.Forms;
namespace SMGI.Common
{
    public class LayerManager
    {
        private static string MapConfigKey = typeof(LayerManager).FullName + ".Map.";
        private static string LayerConfigKey = typeof(LayerManager).FullName + ".Layer.";
        private string MyLayerConfigKey {
            get {
                string key = LayerConfigKey + mapID;

                return key;
            }
        }
        private string MyMapConfigKey {
            get {
                string key = MapConfigKey + mapID;

                return key;
            }
        }
        public Guid MapID {
            get {
                return mapID;
            }
        }
        GWorkspace ws;
        Guid mapID;

        
        internal IMapFrame MapFrameByMap {
            get
            {
                return (ws.PageLayout as IGraphicsContainer).FindFrame(Map) as IMapFrame;
            }
        }
        internal IMapFrame MapFrameByID {
            get { return FindMapFrameInPageLayout(mapID.ToString()); }
        }
        public IMap Map
        {
            get;
            set;
        }
        private List<IRepresentation> currentRepresentation;
        public List<IRepresentation> CurrentRepresentation
        {
            get {
                return currentRepresentation;
            }
        }

        public void UpdateCurrentRepresentation()
        {
            currentRepresentation = new List<IRepresentation>();

            var mc = new MapContextClass();
            mc.InitFromDisplay((Map as IActiveView).ScreenDisplay.DisplayTransformation);

            var lyrs = GetLayer(l =>
            {
                return l is IGeoFeatureLayer
                    && (l as IGeoFeatureLayer).Renderer is IRepresentationRenderer;
            });
            foreach (var l in lyrs)
            {
                var rr = (l as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                var rpc = rr.RepresentationClass;
                if (rpc==null)
                {
                    continue;
                }
                var ss = (l as IFeatureSelection).SelectionSet;
                IQueryFilter qf = new QueryFilterClass();
                rpc.PrepareFilter(qf);
                ICursor c;
                ss.Search(qf, false, out c);
                IFeatureCursor fcuror = c as IFeatureCursor;
                IFeature fe = null;
                while ((fe = fcuror.NextFeature()) != null)
                {
                    var r = rpc.GetRepresentation(fe, mc);
                    currentRepresentation.Add(r);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fcuror);
            }
        }
        public delegate bool LayerChecker(ILayer info);

        public IEnumerable<ILayer> GetLayer(LayerChecker checker)
        {
            IEnumLayer layers = Map.get_Layers();
            layers.Reset();
            ILayer l = null;
            while ((l = layers.Next())!=null)
            {
                if (l is IFeatureLayer && (l as IFeatureLayer).FeatureClass == null)
                {
                    continue;
                }

                if (checker(l))
                {
                    yield return l;
                }
            }
        }

        public event EventHandler LayerChanged;
        internal LayerManager(GWorkspace workspace,Guid mapID)
        {
            this.mapID = mapID;
            ws = workspace;
            LoadLayers();
            UpdateCurrentRepresentation();
            (Map as IActiveViewEvents_Event).SelectionChanged += new IActiveViewEvents_SelectionChangedEventHandler(LayerManager_SelectionChanged);
        }

        void LayerManager_SelectionChanged()
        {
            UpdateCurrentRepresentation();
        }
        internal void LoadLayers()
        {
            Common.Config config = ws.MapConfig;

            var frame = FindMapFrameInPageLayout(MapID.ToString());
            if (frame == null || frame.Map == null)
            {
                Map = new MapClass();
                //IAnnotateMap sm = new MaplexAnnotateMapClass();
                //Map.AnnotationEngine = sm;
                Map.Description = MapID.ToString();
                Map.Name = ws.Name + ((MapID == Guid.Empty) ? "【主图】" : "【附图】");
                if (frame != null)
                    frame.Map = Map;

                if (this.Map.MapUnits == esriUnits.esriUnknownUnits)
                {
                    this.Map.MapUnits = esriUnits.esriMeters;
                }
            }
            else
            {
                string mapName = ws.Name + ((MapID == Guid.Empty) ? "【主图】" : "【附图】");
                Map = frame.Map;
                if (Map.Name != mapName)
                {
                    Map.Name = mapName;
                }

            }
            
            IMap map = this.Map;
            if (map.Description != mapID.ToString())
            {
                map.Description = MapID.ToString();
            }
            LayerManager.ReConnectData(ws.LastPath, ws.EsriWorkspace, map);
            // .ReConnectData(ws.EsriWorkspace, map);

        }

        public static void ReConnectData(string lastWorkspacePath,IWorkspace ws, IMap map)
        {
            var layers = map.get_Layers();
            ILayer l = null;
            layers.Reset();
            IWorkspaceName wname = (ws as IDataset).FullName as IWorkspaceName;
            while ((l = layers.Next()) != null)
            {
                if (l is IDataLayer2)
                {
                    var dl = l as IDataLayer2;
                    if (dl.InWorkspace(ws))
                        continue;
                    IName name = dl.DataSourceName;
                    IDatasetName dname = name as IDatasetName;
                    if (lastWorkspacePath != null && dname.WorkspaceName.PathName != lastWorkspacePath)
                    {
                        continue;
                    }
                    try
                    {
                        dname.WorkspaceName = wname;
                        dl.Connect(name);                        
                    }
                    catch (Exception)
                    {

                        continue;
                    }

                }
            }
        }


        IMapFrame FindMapFrameInPageLayout(string customPropertyString)
        {
            var c = ws.PageLayout as IGraphicsContainer;
            
            IElement e = null;
            c.Reset();
            while ((e = c.Next())!=null)
            {
                if (e is IMapFrame)
                {
                    var me = e as IMapFrame;
                    if (me.Map.Description == customPropertyString)
                        return me;
                }
            }
            return null;
        }

        private ILayer GetLayerAt(IMap map, int[] indexs)
        {
            if (indexs.Length == 0)
                return null;
            if (map == null)
                return null;
            try
            {
                int idx = indexs[0];
                ILayer layer = map.get_Layer(idx);
                for (int j = 1; j < indexs.Length; j++)
                {
                    idx = indexs[j];
                    layer = (layer as ICompositeLayer).get_Layer(idx);
                }
                return layer;
            }
            catch
            {
                return null;
            }
        }
        internal void Save()
        {
            (MapFrameByMap as IElementProperties).CustomProperty = MapID.ToString();
       }


        public bool NameExist(string name)
        {
            IWorkspace2 ws2 = (ws.EsriWorkspace as IWorkspace2);
            return ws2.get_NameExists(esriDatasetType.esriDTFeatureClass, name) ||
                ws2.get_NameExists(esriDatasetType.esriDTFeatureDataset, name) ||
                ws2.get_NameExists(esriDatasetType.esriDTRasterDataset, name);
        }

        /// <summary>
        /// 将一个要素类导入到工作区中
        /// </summary>
        /// <param name="fc">要导入的要素类</param>
        /// <param name="qf">过滤条件</param>
        /// <returns>导入的要素类（名称为fc的前4个字加当前时间）</returns>
        public IFeatureClass ImportFeatureClass(IFeatureClass fc, IQueryFilter qf,ISpatialReference sp=null,bool changeName = true,bool isAnnotation=false)
        {
            IFeatureClassLoad pload = null;
            IFeatureClass featureClass = null;
            IFeatureCursor writeCursor = null;
            string name = string.Empty;
            if (changeName)
            {
                do
                {
                    name = (fc as IDataset).Name;
                    DateTime now = DateTime.Now;
                    string time = now.ToString("yyyyMMddHHmmss");
                    name += time;
                } while (NameExist(name));
            }
            else
            {
                name = (fc as IDataset).Name;
                if (NameExist(name))
                    return null;
            }
            try
            {
                if (isAnnotation)
                {
                    if (sp==null)
	                {
		                 featureClass = CreateStandardAnnotationClass(ws.EsriWorkspace as IFeatureWorkspace, null, name, (fc as IGeoDataset).SpatialReference,10000, esriUnits.esriMeters,string.Empty);
	                }
                    else
	                {
                        featureClass = CreateStandardAnnotationClass(ws.EsriWorkspace as IFeatureWorkspace, null, name, sp,10000, esriUnits.esriMeters,string.Empty);
                    }          
                }
                else
                {
                    if (sp == null)
                    {
                        featureClass = CreateFeatureClass(name, fc.Fields, (fc as IGeoDataset).SpatialReference);
                    }
                    else
                    {
                        featureClass = CreateFeatureClass(name, fc.Fields, sp);
                    }
                   
                }
                
                pload = featureClass as IFeatureClassLoad;
                pload.LoadOnlyMode = true;
                IFeatureCursor readCursor = fc.Search(qf, true);
                writeCursor = featureClass.Insert(true);
                IFeature feature = null;
                int count = 0;
                while ((feature = readCursor.NextFeature()) != null)
                {
                    count++;
                    IFeatureBuffer fb = featureClass.CreateFeatureBuffer();
                    for (int i = 0; i < fb.Fields.FieldCount; i++)
                    {
                        IField field = fb.Fields.get_Field(i);
                        if (!(field as IFieldEdit).Editable)
                        {
                            continue;
                        }
                        if (field.Type == esriFieldType.esriFieldTypeGeometry)
                        {
                            fb.Shape = feature.ShapeCopy;
                            continue;
                        }
                        fb.set_Value(i, feature.get_Value(feature.Fields.FindField(field.Name)));
                    }
                    writeCursor.InsertFeature(fb);
                }
                writeCursor.Flush();
                if (featureClass != null)
                {
                    (featureClass as IFeatureClassManage).UpdateExtent();
                }
            }
            catch
            {

            }
            finally
            {
                if (writeCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);
                }
                if (pload != null)
                {
                    pload.LoadOnlyMode = false;
                }
            }
            return featureClass;
        }

        /// <summary>
        /// 将一个要素类导入到工作区中
        /// </summary>
        /// <param name="fc">要导入的要素类</param>
        /// <param name="qf">过滤条件</param>
        /// <returns>导入的要素类（名称为fc的前4个字加当前时间）</returns>
        public IFeatureClass ImportFeatureClassWithName(IFeatureClass fc, IQueryFilter qf, string newName, ISpatialReference sp=null, bool isAnnotation = false)
        {
            IFeatureClassLoad pload = null;
            IFeatureClass featureClass = null;
            IFeatureCursor writeCursor = null;
            string name = string.Empty;
            name = newName;
            
            if (NameExist(name))
                return null;

            try
            {
                if (isAnnotation)
                {
                    if (sp == null)
                    {
                        featureClass = CreateStandardAnnotationClass(ws.EsriWorkspace as IFeatureWorkspace, null, name, (fc as IGeoDataset).SpatialReference, 10000, esriUnits.esriMeters, string.Empty);
                    }
                    else
                    {
                        featureClass = CreateStandardAnnotationClass(ws.EsriWorkspace as IFeatureWorkspace, null, name, sp, 10000, esriUnits.esriMeters, string.Empty);
                    }
                }
                else
                {
                    if (sp == null)
                    {
                        featureClass = CreateFeatureClass(name, fc.Fields, (fc as IGeoDataset).SpatialReference);
                    }
                    else
                    {
                        featureClass = CreateFeatureClass(name, fc.Fields, sp);
                    }

                }
                pload = featureClass as IFeatureClassLoad;
                pload.LoadOnlyMode = true;
                IFeatureCursor readCursor = fc.Search(qf, true);
                writeCursor = featureClass.Insert(true);
                IFeature feature = null;
                int count = 0;
                while ((feature = readCursor.NextFeature()) != null)
                {
                    count++;
                    IFeatureBuffer fb = featureClass.CreateFeatureBuffer();
                    for (int i = 0; i < fb.Fields.FieldCount; i++)
                    {
                        IField field = fb.Fields.get_Field(i);
                        if (!(field as IFieldEdit).Editable)
                        {
                            continue;
                        }
                        if (field.Type == esriFieldType.esriFieldTypeGeometry)
                        {
                            fb.Shape = feature.ShapeCopy;
                            continue;
                        }
                        fb.set_Value(i, feature.get_Value(feature.Fields.FindField(field.Name)));
                    }
                    writeCursor.InsertFeature(fb);
                }
                writeCursor.Flush();
                if (featureClass != null)
                {
                    (featureClass as IFeatureClassManage).UpdateExtent();
                }
            }
            catch
            {

            }
            finally
            {
                if (writeCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);
                }
                if (pload != null)
                {
                    pload.LoadOnlyMode = false;
                }
            }
            return featureClass;
        }
        public string CreateUniqueFeatureClassName(string seed)
        {
            string name = string.Empty;
            do
            {
                DateTime now = DateTime.Now;
                string time = now.ToString("yyyyMMddHHmmss");
                name = seed;
                name += time;
            } while (NameExist(name));

            return name;
        }

        public IFields CreateEssentialFields(
            esriGeometryType geoType,
            esriFeatureType featureType,
            ISpatialReference spatialReference)
        {
            IFields fields = null;
            try
            {
                if (spatialReference == null)
                {
                    throw new Exception("spatial reference is must have");
                }
                IGeometryDef geometryDef = new GeometryDefClass();
                IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                geometryDefEdit.GeometryType_2 = geoType;


                ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)spatialReference;
                spatialReferenceResolution.ConstructFromHorizon();
                ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)spatialReference;
                spatialReferenceTolerance.SetDefaultXYTolerance();
                geometryDefEdit.SpatialReference_2 = spatialReference;


                IField geometryField = new FieldClass();
                IFieldEdit geometryFieldEdit = (IFieldEdit)geometryField;
                geometryFieldEdit.Name_2 = "SHAPE";
                geometryFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                geometryFieldEdit.GeometryDef_2 = geometryDef;

                IFeatureClassDescription featureClassDescription = new FeatureClassDescriptionClass();
                fields = (featureClassDescription as IObjectClassDescription).RequiredFields;
                (fields as IFieldsEdit).set_Field(fields.FindFieldByAliasName(featureClassDescription.ShapeFieldName), geometryField);
            }
            catch
            {

            }
            return fields;
        }

        public IFeatureClass CreateStandardAnnotationClass(IFeatureWorkspace
    featureWorkspace, IFeatureDataset featureDataset, String className,
    ISpatialReference spatialReference, int referenceScale, esriUnits
    referenceScaleUnits, String configKeyword)
        {
            // Create an annotation class and provide it with a name.
            ILabelEngineLayerProperties labelEngineLayerProperties = new
                LabelEngineLayerPropertiesClass();
            IAnnotateLayerProperties annotateLayerProperties = (IAnnotateLayerProperties)
                labelEngineLayerProperties;
            annotateLayerProperties.Class = "Default";

            // Get the symbol from the annotation class. Make any changes to its properties
            // here.
            ITextSymbol annotationTextSymbol = labelEngineLayerProperties.Symbol;
            ISymbol annotationSymbol = (ISymbol)annotationTextSymbol;

            // Create a symbol collection and add the default symbol from the
            // annotation class to the collection. Assign the resulting symbol ID
            // to the annotation class.
            ISymbolCollection symbolCollection = new SymbolCollectionClass();
            ISymbolCollection2 symbolCollection2 = (ISymbolCollection2)symbolCollection;
            ISymbolIdentifier2 symbolIdentifier2 = null;
            symbolCollection2.AddSymbol(annotationSymbol, "Default", out
        symbolIdentifier2);
            labelEngineLayerProperties.SymbolID = symbolIdentifier2.ID;

            // Add the annotation class to a collection.
            IAnnotateLayerPropertiesCollection annotateLayerPropsCollection = new
                AnnotateLayerPropertiesCollectionClass();
            annotateLayerPropsCollection.Add(annotateLayerProperties);

            // Create a graphics layer scale object.
            IGraphicsLayerScale graphicsLayerScale = new GraphicsLayerScaleClass();
            graphicsLayerScale.ReferenceScale = referenceScale;
            graphicsLayerScale.Units = referenceScaleUnits;

            // Create the overposter properties for the standard label engine.
            IOverposterProperties overposterProperties = new BasicOverposterPropertiesClass()
                ;

            // Instantiate a class description object.
            IObjectClassDescription ocDescription = new
                AnnotationFeatureClassDescriptionClass();
            IFeatureClassDescription fcDescription = (IFeatureClassDescription)ocDescription;

            // Get the shape field from the class description's required fields.
            IFields requiredFields = ocDescription.RequiredFields;
            int shapeFieldIndex = requiredFields.FindField(fcDescription.ShapeFieldName);
            IField shapeField = requiredFields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = shapeField.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.SpatialReference_2 = spatialReference;

            // Create the annotation layer factory.
            IAnnotationLayerFactory annotationLayerFactory = new
                FDOGraphicsLayerFactoryClass();

            // Create the annotation feature class and an annotation layer for it.
            IAnnotationLayer annotationLayer = annotationLayerFactory.CreateAnnotationLayer
                (featureWorkspace, featureDataset, className, geometryDef, null,
                annotateLayerPropsCollection, graphicsLayerScale, symbolCollection, false,
                false, false, true, overposterProperties, configKeyword);

            // Get the feature class from the feature layer.
            IFeatureLayer featureLayer = (IFeatureLayer)annotationLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;

            return featureClass;
        }

        public IFeatureClass CreateFeatureClass(string name, IFields org_fields,ISpatialReference spatialReference)
        {
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription featureDescription = fcDescription as IObjectClassDescription;

            IFieldsEdit fields = featureDescription.RequiredFields as IFieldsEdit;


            for (int i = 0; i < org_fields.FieldCount; i++)
            {
                IField field = org_fields.get_Field(i);
                if (!(field as IFieldEdit).Editable)
                {
                    continue;
                }
                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    (fields as IFieldsEdit).set_Field(fields.FindFieldByAliasName((featureDescription as IFeatureClassDescription).ShapeFieldName),
                        (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField);
                    continue;
                }
                if (fields.FindField(field.Name) >= 0)
                {
                    continue;
                }
                IField field_new = (field as ESRI.ArcGIS.esriSystem.IClone).Clone() as IField;
                (fields as IFieldsEdit).AddField(field_new);
            }

            int shapeFieldIndex = fields.FindField(fcDescription.ShapeFieldName);
            IField Shapefield = fields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = Shapefield.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
             geometryDefEdit.SpatialReference_2 = spatialReference;

            IFeatureWorkspace fws = ws.EsriWorkspace as IFeatureWorkspace;

            System.String strShapeField = string.Empty;

            return fws.CreateFeatureClass(name, fields,
                  featureDescription.InstanceCLSID, featureDescription.ClassExtensionCLSID,
                  esriFeatureType.esriFTSimple,
                  (featureDescription as IFeatureClassDescription).ShapeFieldName,
                  string.Empty);
        }
        #region rendy
        //public IRepresentationClass RetriveRepClassFromStyle(IFeatureClass fcls, List<string> listValues)
        //{
        //    IRepresentationClass repCls = null;
        //    try
        //    {
        //        IRepresentationRules pRules = new RepresentationRulesClass();
        //        /* bug of esri
        //        //IDataStatistics dtStatistics = new DataStatisticsClass();
        //        //IField field = fcls.Fields.get_Field(fieldIndx);
        //        //ICursor cursor = (ICursor)fcls.Search(null, false);

        //        //dtStatistics.Field = field.Name;
        //        //dtStatistics.Cursor = cursor;
        //        //IEnumerator uniqValues = dtStatistics.UniqueValues;
        //        //string sName = "";
        //        //uniqValues.Reset();
        //         */

        //        IRepresentationRule pRule;

        //        foreach (string s in listValues)
        //        {
        //            IRepresentationRuleItem pRuleItem = this.ws.Application.StyleMgr.getRepRule(ws.Application.StyleMgr.DefaultStylePath, s);
        //            if (pRuleItem != null)
        //            {
        //                pRule = pRuleItem.RepresentationRule;
        //                int id = pRules.Add(pRule);
        //                pRules.set_Name(id, s);
        //            }
        //        }

        //        if (pRules != null)
        //        {
        //            repCls = CreateRepresentationClass(fcls, (fcls as IDataset).Name + "_Rep", pRules);
        //            //repCls = CreateVoidRepresentationClass(fcls);
        //        }
        //    }
        //    catch
        //    {

        //    }

        //    return repCls;
        //}
        #endregion
        public IRepresentationClass CreateRepresentationClass(IFeatureClass fcls,
            string repName,
            IRepresentationRules pRules)
        {
            IRepresentationClass repCls = null;
            try
            {
                IRepresentationWorkspaceExtension pRepWSExt;
                IWorkspaceExtensionManager pExtManager;
                UID pUID;
                pExtManager = (fcls as IDataset).Workspace as IWorkspaceExtensionManager;
                pUID = new UID();
                pUID.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
                pRepWSExt = pExtManager.FindExtension(pUID) as IRepresentationWorkspaceExtension;

                if (pRepWSExt.get_FeatureClassHasRepresentations(fcls))
                {
                    IEnumDatasetName enumDsName = pRepWSExt.get_FeatureClassRepresentationNames(fcls);
                    enumDsName.Reset();
                    IDatasetName dsName = null;
                    while ((dsName = enumDsName.Next()) != null)
                    {
                        if (dsName.Name == repName)
                        {
                            repName = dsName.Name.Substring(0, 4) + "Rep" + System.DateTime.Now.ToString("MMddHHmmss");
                            break;
                        }
                    }
                }
                repCls = pRepWSExt.CreateRepresentationClass(fcls, repName, "RuleID" + System.DateTime.Now.ToString("MMddHHmmss"),
                    "Override" + System.DateTime.Now.ToString("MMddHHmmss"), false, pRules, null);
            }
            catch
            {
            }
            return repCls;
        }

        public IRepresentationClass CreateVoidRepresentationClass(IFeatureClass fcls)
        {
            AddRepresentation ar = new AddRepresentation(fcls, fcls.AliasName);
            Helper.ExecuteGPTool(this.ws.Application.GPTool, ar, null);
            IRepresentationClass repCls = ar.out_features as IRepresentationClass;

            return repCls;

        }



        public IRasterDataset ImportRasterDataset(IRasterDataset raster)
        {
            string name = string.Empty;
            do
            {
                name = (raster as IDataset).Name;
                DateTime now = DateTime.Now;
                string time = now.ToString("yyyyMMddHHmmss");
                name = name.Substring(0, 4);
                name += time;
            } while (NameExist(name));
            return (raster as ISaveAs2).SaveAs(name, ws.EsriWorkspace, "GDB") as IRasterDataset;
        }
        public IRaster ImportRaster(IRaster raster)
        {
            string org_name = ((raster as IRaster2).RasterDataset as IDataset).Name;
            string name = org_name;
            //do
            //{
            //    name = org_name;
            //    DateTime now = DateTime.Now;
            //    string time = now.ToString("yyyyMMddHHmmss");
            //    name = name.Substring(0, 4);
            //    name += time;
            //} while (NameExist(name));
            return ((raster as ISaveAs2).SaveAs(name, ws.EsriWorkspace, "GDB") as IRasterDataset).CreateDefaultRaster();
        }

        
    }
}

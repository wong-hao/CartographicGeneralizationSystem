using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace DatabaseUpdate
{
    /// <summary>
    /// Summary description for FindUpdateObects.
    /// </summary>
    [Guid("3de9647f-53ab-4272-b4cd-73fba0f036e9")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DatabaseUpdate.FindUpdateObjects")]
    public sealed class FindUpdateObjects : BaseCommand
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        private IApplication m_application;
        public FindUpdateObjects()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text
            base.m_caption = "查找更新要素";  //localizable text
            base.m_message = "查找更新要素";  //localizable text 
            base.m_toolTip = "查找更新要素";  //localizable text 
            base.m_name = "DatabaseUpdate.FindUpdateObjects";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overriden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            m_application = hook as IApplication;

            //Disable if it is not ArcMap
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false;

            // TODO:  Add other initialization code
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && UApplication.Application != null && UApplication.Application.Workspace != null;
            }
        }

        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public override void OnClick()
        {
            IStatusBar sBar = UApplication.Application.EsriApplication.StatusBar;
            IStepProgressor sPro = sBar.ProgressBar;
            sPro.Position = 0;
         
            UApplication app = UApplication.Application;
            ULayerInfos mxlayers = app.Workspace.MxLayers;
            ULayerInfos mclayers = app.Workspace.McLayers;
            //List<int> conflictsFeatsID = new List<int>();
            foreach (IFeatureLayer mxlayer in mxlayers.Layers.Values)
            {
                sBar.ShowProgressBar("处理中...", 0, mclayers.Layers.Count, 1, true);

                //IFeatureClass MxClass = mxlayer.FeatureClass;
                ISelectionSet set = (mxlayer as IFeatureSelection).SelectionSet;
                GeometryBagClass gb = new GeometryBagClass();
                //IFeatureCursor fcur = MxClass.Search(null, false);
                ICursor cu = null;
                set.Search(null, false, out cu);
                IFeatureCursor fcur =cu as IFeatureCursor;
                IFeature feat = null;
                object miss = Type.Missing;
                while ((feat = fcur.NextFeature()) != null)
                {
                    gb.AddGeometry(feat.ShapeCopy, ref miss, ref miss);
                }
                IGeometry poly;
                if (!mxlayer.Name.Contains("道路中心线"))
                {
                    poly = new PolygonClass();
                }
                else
                {
                    poly = new PolylineClass();
                }
                (poly as ITopologicalOperator).ConstructUnion(gb);
                foreach (IFeatureLayer mclayer in mclayers.Layers.Values)
                {
                    sBar.StepProgressBar();
                    if (mclayer.Selectable)
                    {
                        List<int> tpList = findUpdatesWithObjects(poly, mclayer);
                    }
                }
                UApplication.Application.EsriMapControl.ActiveView.Refresh();
                sBar.HideProgressBar();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur);
            }           
        }

        #endregion

        public List<int> findUpdatesWithObjects(IGeometry poly, IFeatureLayer McLayer)
        {
            IFeatureClass McClass = McLayer.FeatureClass;

            List<int> McObjects = new List<int>();
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelRelation;
            sf.SpatialRelDescription = "T********";
            sf.Geometry = poly;

            (McLayer as IFeatureSelection).SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
            ISelectionSet set = (McLayer as IFeatureSelection).SelectionSet;
            IEnumIDs ids = set.IDs;
            ids.Reset();
            int id = -1;
            while ((id = ids.Next()) != -1)
            {
                McObjects.Add(id);
            }
            return McObjects;
        }

    }
}

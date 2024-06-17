using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;


namespace DatabaseUpdate
{
    /// <summary>
    /// Summary description for PlantMinHole.
    /// </summary>
    [Guid("820fc2f0-c347-4735-86d1-9da00c5d5799")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DatabaseUpdate.PlantMinHole")]
    public sealed class PlantMinHole : BaseTool
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
        INewPolygonFeedback fb;
        public PlantMinHole()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text 
            base.m_caption = "植被";  //localizable text 
            base.m_message = "植被去内环";  //localizable text
            base.m_toolTip = "植被去内环";  //localizable text
            base.m_name = "植被去内环";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
            try
            {
                //
                // TODO: change resource name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        IFeatureClass fc = null;
        IFeatureLayer plantLayer = null;
        #region Overriden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            m_application = hook as IApplication;

            for (int i = 0; i < (m_application.Document as IMxDocument).ActiveView.FocusMap.LayerCount; i++)
            {
                IFeatureLayer mxlayer = (m_application.Document as IMxDocument).ActiveView.FocusMap.get_Layer(i) as IFeatureLayer;
                if (mxlayer.Name.Contains("植被"))
                {
                    plantLayer = mxlayer;
                    fc = plantLayer.FeatureClass;
                    break;
                }
            }
            //Disable if it is not ArcMap
            if (hook is IMxApplication && plantLayer != null)
                base.m_enabled = true;
            else
                base.m_enabled = false;

            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            for (int i = 0; i < (m_application.Document as IMxDocument).ActiveView.FocusMap.LayerCount; i++)
            {
                IFeatureLayer mxlayer = (m_application.Document as IMxDocument).ActiveView.FocusMap.get_Layer(i) as IFeatureLayer;
                if (mxlayer.Name.Contains("植被"))
                {
                    plantLayer = mxlayer;
                    fc = plantLayer.FeatureClass;
                    break;
                }
            }
            if (plantLayer == null)
            {
                MessageBox.Show("请确保层名中含有“植被”!");
            }
        }
        public override bool Enabled
        {
            get
            {
                bool isable = false;
                for (int i = 0; i < (m_application.Document as IMxDocument).ActiveView.FocusMap.LayerCount; i++)
                {
                    IFeatureLayer mxlayer = (m_application.Document as IMxDocument).ActiveView.FocusMap.get_Layer(i) as IFeatureLayer;
                    if (mxlayer.Name.Contains("植被"))
                    {
                        isable = true;
                        break;
                    }
                }

                return isable;

            }
        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            IPoint p = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb == null)
            {
                fb = new NewPolygonFeedbackClass();
                fb.Display = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay;
                fb.Start(p);
            }
            else
            {
                fb.AddPoint(p);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            if (fb != null)
            {
                IPoint p = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add RoadStrokeTool.OnMouseUp implementation
        }

        public override void OnDblClick()
        {
            if (fb == null)
                return;
            IPolygon p = fb.Stop();
            fb = null;
            //ISpatialFilter sf = new SpatialFilterClass();
            //sf.Geometry = p;
            //sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            //(roadlayers as IFeatureSelection).SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
            Gen(p);
        }

        void Gen(IPolygon range)
        {
            ISpatialFilter qf = new SpatialFilterClass();
            qf.Geometry = range;
            qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor fCur;
            IFeature f;
            fCur = fc.Update(qf, false);
            IFeatureCursor insertCur = fc.Insert(false);
            try
            {
                double minArea = 90000;
                int count = fc.FeatureCount(qf);
                IStatusBar sBar = m_application.StatusBar;
                IStepProgressor sPro = sBar.ProgressBar;
                sPro.Position = 0;
                sBar.ShowProgressBar("处理中...", 0, count, 1, true);
                while ((f = fCur.NextFeature()) != null)
                {
                    sBar.StepProgressBar();
                    IGeometry geoCopy = f.Shape;
                    IPolygon forMinArea = geoCopy as IPolygon;
                    ITopologicalOperator oriTO = forMinArea as ITopologicalOperator;
                    oriTO.Simplify();

                    IPolygon4 result = oriTO as IPolygon4;
                    IGeometryCollection resultColl = result.ConnectedComponentBag as IGeometryCollection;
                    for (int i = 0; i < resultColl.GeometryCount; i++)
                    {
                        IPolygon tpPo = resultColl.get_Geometry(i) as IPolygon;
                        if ((tpPo as IArea).Area < minArea)
                        {
                            continue;
                        }
                        IPolygon4 iPolygon = removeInteriorRings(tpPo as IPolygon4, 200000);
                        f.Shape = iPolygon;
                        Convert.ToInt32(insertCur.InsertFeature(f as IFeatureBuffer));
                    }
                    f.Delete();
                }
                sBar.HideProgressBar();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCur);
                (m_application.Document as IMxDocument).ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

            }
            catch (Exception ex)
            {
            }
        }

        private IPolygon4 removeInteriorRings(IPolygon4 ringsPoly, double areaA)
        {
            ITopologicalOperator tt = ringsPoly as ITopologicalOperator;
            tt.Simplify();
            IGeometryBag exteriorRings = ringsPoly.ExteriorRingBag;
            IEnumGeometry exteriorEnum = exteriorRings as IEnumGeometry;
            exteriorEnum.Reset();
            IRing currentExterRing = exteriorEnum.Next() as IRing;
            IRing tpInteriorRing = new RingClass();
            IPolygon4 noInterRingPolygon = new PolygonClass();
            IGeometryCollection ringsCollection = noInterRingPolygon as IGeometryCollection;
            IGeometry forAddGeometry = currentExterRing as IGeometry;
            ringsCollection.AddGeometries(1, ref forAddGeometry);
            while (currentExterRing != null)
            {
                IGeometryBag interiorRings = ringsPoly.get_InteriorRingBag(currentExterRing);
                IEnumGeometry interiorEnum = interiorRings as IEnumGeometry;
                tpInteriorRing = interiorEnum.Next() as IRing;
                while (tpInteriorRing != null)
                {
                    IArea tpInteriorRingArea = tpInteriorRing as IArea;
                    if (Math.Abs(tpInteriorRingArea.Area) > areaA)
                    {
                        forAddGeometry = tpInteriorRing as IGeometry;
                        ringsCollection.AddGeometries(1, ref forAddGeometry);

                    }

                    tpInteriorRing = interiorEnum.Next() as IRing;
                }

                currentExterRing = exteriorEnum.Next() as IRing;
            }
            return noInterRingPolygon;
        }

        public override bool Deactivate()
        {
            return base.Deactivate();
        }
        public override void OnKeyUp(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null)
                    {
                        fb.Stop();
                        fb = null;
                    }
                    break;
            }
        }
        #endregion
    }
}

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

namespace DomapTool
{
    /// <summary>
    /// Summary description for ConflictHandleOfBuildAndRoad.
    /// </summary>
    [Guid("0fd1148b-1035-4b3c-ab9f-d2d3f751074d")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DomapTool.ConflictHandleOfBuildAndRoad")]
    public sealed class ConflictHandleOfBuildAndRoad : BaseCommand
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
        IEditor editor;
        public ConflictHandleOfBuildAndRoad()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text
            base.m_caption = "建筑物与道路的冲突处理";  //localizable text
            base.m_message = "解决建筑物与道路冲突";  //localizable text 
            base.m_toolTip = "解决建筑物与道路冲突";  //localizable text 
            base.m_name = "DomapTool.ConflictHandleOfBuildAndRoad";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

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
            editor = m_application.FindExtensionByName("ESRI Object Editor") as IEditor;
            // TODO:  Add other initialization code
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && editor.EditState == esriEditState.esriStateEditing;
            }
        }
        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public override void OnClick()
        {
            IActiveView av = (m_application.Document as IMxDocument).ActiveView;
            IMap map = av.FocusMap;
            GenerateUpdateAreaWin d = new GenerateUpdateAreaWin(map);
            d.Text = "建筑物与道路的冲突处理";
            d.label1.Text = "建筑物层：";
            d.label2.Text = "道路层：";
            d.comboBox2.Items.Clear();
            for (int j = 0; j < map.LayerCount; j++)
            {
                ILayer layer = map.get_Layer(j);
                IFeatureLayer l = layer as IFeatureLayer;
                if (l != null && l.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                {
                    GenerateUpdateAreaWin.FeatureLayerWarp fw = new GenerateUpdateAreaWin.FeatureLayerWarp(l);
                    d.comboBox2.Items.Add(fw);
                }
            }
            if (d.comboBox2.Items.Count > 0)
                d.comboBox2.SelectedIndex = 0;
            if (d.ShowDialog() == DialogResult.Cancel)
                return;
            IFeatureLayer buildLayer = d.SelectUpdateLayer;
            IFeatureLayer roadLayer = d.SelectUpdateAreaLayer;
            IFeatureClass buildC = buildLayer.FeatureClass;
            IFeatureClass roadC = roadLayer.FeatureClass;
            if (buildLayer == null ||roadLayer==null)
            {
                MessageBox.Show("请确保操作图层有效!");
                return;
            }
            if (buildC == null||roadC==null) return;

            editor.StartOperation();
            IFeatureCursor bfCursor = buildC.Update(null, true);
            IFeatureCursor insertCursor = buildC.Insert(true);
            IFeature bFeature = null;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            object miss = Type.Missing;
            IStatusBar sBar = m_application.StatusBar;
            IStepProgressor sPro = sBar.ProgressBar;
            sPro.Position = 0;
            int wCount = buildC.FeatureCount(null);
            sBar.ShowProgressBar("正在处理...", 0, wCount, 1, true);
            while ((bFeature = bfCursor.NextFeature()) != null)
            {
                sBar.StepProgressBar();
                sf.Geometry = bFeature.Shape;
                IFeatureCursor rfCursor = roadC.Search(sf, true);
                IFeature rFeature = null;
                PolylineClass line = new PolylineClass();
                while ((rFeature = rfCursor.NextFeature()) != null)
                {
                    IGeometryCollection gc = rFeature.Shape as IGeometryCollection;
                    for (int i = 0; i < gc.GeometryCount; i++)
                    {
                        line.AddGeometry(gc.get_Geometry(i), ref miss, ref miss);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rfCursor);
                if (line.IsEmpty)
                    continue;
                line.Simplify();
                ITopologicalOperator4 topo = bFeature.ShapeCopy as ITopologicalOperator4;
                double area = (topo as IArea).Area;

                IGeometryCollection gcCut = null;
                try
                {
                    gcCut = topo.Cut2(line);
                }
                catch
                {
                    continue;
                }
                for (int i = 0; i < gcCut.GeometryCount; i++)
                {
                    IArea a = gcCut.get_Geometry(i) as IArea;
                    if (a.Area * 10 > area)
                    {
                        bFeature.Shape = a as IGeometry;
                        insertCursor.InsertFeature(bFeature as IFeatureBuffer);
                    }
                }
                bfCursor.DeleteFeature();
            }
            bfCursor.Flush();
            insertCursor.Flush();
            sBar.HideProgressBar();
            editor.StopOperation("建筑物与道路冲突处理");
            (m_application.Document as IMxDocument).ActiveView.Refresh();
        }

        #endregion
    }
}

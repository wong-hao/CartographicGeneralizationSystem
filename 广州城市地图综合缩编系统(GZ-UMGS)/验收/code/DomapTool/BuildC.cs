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
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesFile;
namespace DomapTool
{
    /// <summary>
    /// Summary description for BuildC.
    /// </summary>
    [Guid("389382ef-1b8f-4170-83d1-7fdfe29741df")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DomapTool.BuildC")]
    public sealed class BuildC : BaseTool
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
        private List<IPolygon> polygons;
        private List<IPolyline> centerLines;
        private IApplication m_application;
        IFeatureClass fc = null;
        IFeatureLayer buildLayer = null;
        INewPolygonFeedback fb;
        IEditor editor;
        public BuildC()
        {
            //
            // TODO: Define values for the public properties
            //
            polygons = new List<IPolygon>();
            centerLines = new List<IPolyline>();
            base.m_category = "Domap"; //localizable text 
            base.m_caption = "缝隙等宽化";  //localizable text 
            base.m_message = "建筑物间隙";  //localizable text
            base.m_toolTip = "将建筑物间隙进行等宽化处理";  //localizable text
            base.m_name = "DomapTool.BuildC";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
            GGenPara.Para.RegistPara("街区缝隙宽度", (double)5);
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

        #region Overriden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
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
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            ac = (m_application.Document as IMxDocument).ActiveView;
            LayersPara d = new LayersPara((m_application.Document as IMxDocument).ActiveView.FocusMap);
            d.Text = "缝隙等宽化";
            d.textBox1.Text = GGenPara.Para["街区缝隙宽度"].ToString();

            if (d.ShowDialog() == DialogResult.Cancel)
                return;
            buildLayer = d.SelectLayer;
            dis = Convert.ToDouble(d.textBox1.Text);
            GGenPara.Para["街区缝隙宽度"] = dis;
            fc = buildLayer.FeatureClass;
            if (buildLayer == null)
            {
                MessageBox.Show("请确保建筑物图层不为空!");
                return;
            }
        }
        double dis;
        IActiveView ac;
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button != 1)
                return;
            IPoint p = ac.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb == null)
            {
                fb = new NewPolygonFeedbackClass();
                fb.Display =ac.ScreenDisplay;
                fb.Start(p);
            }
            else
            {
                fb.AddPoint(p);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (fb != null)
            {
                IPoint p = ac.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add BuildC.OnMouseUp implementation
        }

        public override void OnKeyDown(int keyCode, int Shift)
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

        public override void OnDblClick()
        {
            if (fb != null)
            {
                IPolygon poly = fb.Stop();
                fb = null;
                if (poly == null || poly.IsEmpty)
                {
                    return;
                }
                Gen2(poly);
                ac.Refresh();
            }
        }
        #endregion
        void Gen2(IPolygon range)
        {
            //var wo = m_application.SetBusy(true);
            polygons.Clear();
            centerLines.Clear();
            IFeatureLayer layer = buildLayer;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor fCursor = layer.FeatureClass.Search(sf, true);
            IFeature feature = null;
            IPolygon poly = new PolygonClass();

            int groupID = layer.FeatureClass.FindField("G_BuildGroup");
            ISpatialReferenceFactory srf_ = new SpatialReferenceEnvironmentClass();
            IProjectedCoordinateSystem pcs_ = srf_.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_CM_114E);
            double bufferWidth = dis;

            //wo.SetText("正在进行分析准备");
            //wo.Step(8);
            IStatusBar sBar = m_application.StatusBar;
            IStepProgressor sPro = sBar.ProgressBar;
            sPro.Position = 0;

            GeometryBagClass gb = new GeometryBagClass();
            GeometryBagClass gb2 = new GeometryBagClass();
            int wCount = layer.FeatureClass.FeatureCount(sf);
            sBar.ShowProgressBar("正在进行分析准备...", 0, wCount, 1, true);
            object miss = Type.Missing;
            while ((feature = fCursor.NextFeature()) != null)
            {
                //wo.Step(wCount);
                sBar.StepProgressBar();
                IGeometry geoCopy = feature.ShapeCopy;
                geoCopy.SpatialReference = pcs_;
                gb.AddGeometry(feature.ShapeCopy, ref miss, ref miss);
                gb2.AddGeometry((geoCopy as ITopologicalOperator).Buffer(bufferWidth * -0.05), ref miss, ref miss);
            }
            //wo.Step(8);
            (poly as ITopologicalOperator).ConstructUnion(gb);

            poly.SpatialReference = pcs_;

           // wo.Step(8);
            ITopologicalOperator buffer = (poly as ITopologicalOperator).Buffer(bufferWidth) as ITopologicalOperator;
            poly = new PolygonClass();
            poly.SpatialReference = pcs_;
            (poly as ITopologicalOperator).ConstructUnion(gb2);
            poly = buffer.Difference(poly) as IPolygon;
            double width = (poly as IArea).Area / poly.Length * 2;
            width *= 0.4;
            //width *= 0.8;

            //wo.Step(8);
            try
            {

                poly.SpatialReference = pcs_ as IProjectedCoordinateSystem;
                IGeometryCollection gc = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                poly = null;
                for (int i = 0; i < gc.GeometryCount; i++)
                {
                    IArea ar = gc.get_Geometry(i) as IArea;
                    if (ar.Area > 225)
                    {
                        if (poly == null)
                        {
                            poly = ar as IPolygon;
                        }
                        else
                        {
                            poly = (ar as ITopologicalOperator).Union(poly) as IPolygon;
                        }
                    }
                }
            }
            catch
            {
            }
            //wo.Step(8);
            //wo.SetText("正在进行分析。");

            poly.Generalize(width * 0.1);
            CenterLineFactory cf = new CenterLineFactory();
            (poly as ITopologicalOperator).Simplify();
            IGeometryCollection buffBag = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
            sBar.ShowProgressBar("正在进行分析...", 0, buffBag.GeometryCount, 1, true);
            for (int i = 0; i < buffBag.GeometryCount; i++)
            {
                sBar.StepProgressBar();
                try
                {
                    poly = buffBag.get_Geometry(i) as IPolygon;

                    CenterLine cl = cf.Create2(poly);
                    PolylineClass resultPolyline = new PolylineClass();
                    resultPolyline.SpatialReference = pcs_;
                    foreach (var info in cl)
                    {
                        if (info.Info.Triangles[info.Info.Triangles.Count - 1].TagValue != 1
                            && info.Info.Triangles[0].TagValue != 1
                            //&& info.Width > bufferWidth / 2
                            )
                            for (int j = 0; j < (info.Line as IGeometryCollection).GeometryCount; j++)
                            {
                                IGeometry geo = (info.Line as IGeometryCollection).get_Geometry(j);
                                resultPolyline.AddGeometries(1, ref geo);
                            }

                    }

                    IPolyline centerLine = resultPolyline;

                    centerLine.Generalize(width * 1.5);
                    //centerLine.Smooth(0);
                    //centerLine = cf.Create(poly);
                    poly = (centerLine as ITopologicalOperator).Buffer(bufferWidth / 2) as IPolygon;
                    (poly as ITopologicalOperator).Simplify();
                    IGeometryCollection gcc = poly as IGeometryCollection;

                    int index = -1;
                    for (int j = 0; j < gcc.GeometryCount; j++)
                    {
                        IRing r = gcc.get_Geometry(j) as IRing;
                        if (r.IsExterior)
                        {
                            index = j;
                            break;
                        }
                    }
                    gcc.RemoveGeometries(index, 1);
                    (poly as ITopologicalOperator).Simplify();

                    polygons.Add(poly);
                    centerLines.Add(centerLine);
                }
                catch
                {
                }
            }
            //wo.Step(8);
            //wo.SetText("正在提交结果");
            editor.StartOperation();
            sBar.ShowProgressBar("正在提交结果...", 0, 1, 1, true);
            sPro.Position = 1;
            ISpatialFilter sf_add = new SpatialFilterClass();
            sf_add.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor insertCursor = layer.FeatureClass.Insert(true);
            List<int> addIDs = new List<int>();
            foreach (var item in polygons)
            {
                IGeometryCollection gcc = (item as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                for (int i = 0; i < gcc.GeometryCount; i++)
                {
                    IGeometry addPoly = gcc.get_Geometry(i) as IPolygon;
                    (addPoly as ITopologicalOperator).Simplify();
                    addPoly.SpatialReference = (fc as IGeoDataset).SpatialReference;
                    sf_add.Geometry = addPoly;
                    IFeatureCursor findCursor = layer.FeatureClass.Search(sf_add, true);
                    IFeature findFeature;
                    int maxID = -1;
                    double maxArea = 0;
                    while ((findFeature = findCursor.NextFeature()) != null)
                    {
                        try
                        {
                            IArea insertPart = findFeature.Shape as IArea;
                            if (insertPart.Area > maxArea)
                            {
                                maxArea = insertPart.Area;
                                maxID = findFeature.OID;
                            }
                        }
                        catch { }
                    }
                    if (maxID == -1)
                        continue;
                    findFeature = layer.FeatureClass.GetFeature(maxID);
                    (findFeature as IFeatureBuffer).Shape = addPoly;
                    if (groupID > 0)
                        (findFeature as IFeatureBuffer).set_Value(groupID, -3);
                    addIDs.Add((int)insertCursor.InsertFeature(findFeature as IFeatureBuffer));
                }
            }
            insertCursor.Flush();
           // wo.Step(8);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
            IFeatureCursor updataCursor = layer.FeatureClass.Update(sf, true);
            IFeature updataFeature = null;
            while ((updataFeature = updataCursor.NextFeature()) != null)
            {
                if (!addIDs.Contains(updataFeature.OID))
                {
                    updataCursor.DeleteFeature();
                }
            }
            updataCursor.Flush();
            //wo.Step(8);
            sBar.HideProgressBar();
           // m_application.SetBusy(false);
            ac.Refresh();
            editor.StopOperation("缝隙等宽化");
        }
    }
    internal class TagInfo
    {
        internal int nodeIndex;
        internal int fid;
        internal int partIndex;
        internal int pointIndex;
    }
}

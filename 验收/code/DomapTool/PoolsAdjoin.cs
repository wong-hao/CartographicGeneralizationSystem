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
//using GENERALIZERLib;

namespace DomapTool
{
    /// <summary>
    /// Summary description for PoolsAdjoin.
    /// </summary>
    [Guid("87d63f96-89d2-4b98-9d52-c9c608fa554c")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DomapTool.PoolsAdjoin")]
    public sealed class PoolsAdjoin : BaseTool
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
        //Generalizer gen;
        private List<IPolygon> polygons;
        private IApplication m_application;
        INewPolygonFeedback fb;
        public PoolsAdjoin()
        {
            //
            // TODO: Define values for the public properties
            //
            polygons = new List<IPolygon>();
            //gen = new Generalizer();
            base.m_category = "湖泊毗邻"; //localizable text 
            base.m_caption = "湖泊毗邻";  //localizable text 
            base.m_message = "湖泊毗邻";  //localizable text
            base.m_toolTip = "湖泊毗邻";  //localizable text
            base.m_name = "DomapTool.PoolsAdjoin";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
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
            GGenPara.Para.RegistPara("湖泊毗邻宽度", 5);
            GGenPara.Para.RegistPara("湖泊毗邻最小面积", 100);
        }

        #region Overriden Class Methods
        IFeatureClass fc = null;
        IFeatureLayer waterLayer = null;
        IEditor editor;
        IActiveView ac;
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

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        double mindis;
        double minarea;
        string queryf;
        IField ysdmF;
        string ysdmV;
        public override void OnClick()
        {
            ac = (m_application.Document as IMxDocument).ActiveView;
            DoWithQueryFilterDlg d = new DoWithQueryFilterDlg(ac.FocusMap);
            if (d.ShowDialog() == DialogResult.Cancel)
                return;
            waterLayer = d.SelectLayer;
            mindis = d.width;
            minarea = d.minArea;
            fc = waterLayer.FeatureClass;
            if (waterLayer == null)
            {
                MessageBox.Show("请确保操作图层不为空!");
                return;
            }
            //gen.InitGeneralizer(System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\GenPara.inf", 2000, 10000);           
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && editor.EditState == esriEditState.esriStateEditing;
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
            // TODO:  Add PoolsAdjoin.OnMouseUp implementation
        }

        public override void OnDblClick()
        {
            if (fb == null)
                return;
            IPolygon p = fb.Stop();
            fb = null;
            try {
                Gen2(p);
            }
            catch  {
                
            }
            
        }

        void Gen2(IPolygon range)
        {
            //var wo = m_application.SetBusy(true);
            polygons.Clear();
            IFeatureLayer layer = waterLayer;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.WhereClause =queryf;

            IFeatureCursor fCursor = null;
            try
            {
                fCursor = layer.FeatureClass.Search(sf, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询语句不符合规定!");
            }
            IFeature feature = null;
            IPolygon poly = new PolygonClass();

            IFeatureClass fc = layer.FeatureClass;
            
            //int groupID = layer.FeatureClass.FindField("G_Group");
            //int YSDMid = layer.FeatureClass.FindField("要素代码");
            ISpatialReferenceFactory srf_ = new SpatialReferenceEnvironmentClass();
            ISpatialReference pcs_ = (fc as IGeoDataset).SpatialReference;
            if(pcs_ == null || pcs_.FactoryCode == 0)
                pcs_ = srf_.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_CM_114E);
            double bufferWidth = mindis;

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
                sBar.StepProgressBar();
                //if (Convert.ToString(feature.get_Value(YSDMid)) != "624050")
                //{
                //    continue;
                //}
                IGeometry geoCopy = feature.ShapeCopy;
                geoCopy.SpatialReference = pcs_;
                gb.AddGeometry(feature.ShapeCopy, ref miss, ref miss);
                gb2.AddGeometry((geoCopy as ITopologicalOperator).Buffer(bufferWidth * -0.05), ref miss, ref miss);
            }
            //wo.Step(8);
            (poly as ITopologicalOperator).ConstructUnion(gb);
            poly.SpatialReference = pcs_;

            //wo.Step(8);
            ITopologicalOperator buffer = (poly as ITopologicalOperator).Buffer(bufferWidth / 2) as ITopologicalOperator;
            poly = new PolygonClass();
            poly.SpatialReference = pcs_;
            (poly as ITopologicalOperator).ConstructUnion(gb2);
            poly = buffer.Difference(poly) as IPolygon;
            double width = (poly as IArea).Area / poly.Length * 2;
            width *= 0.4;

            //wo.Step(8);
            try
            {

                poly.SpatialReference = pcs_ as IProjectedCoordinateSystem;
                IGeometryCollection gc = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
            }
            catch
            {
            }

            if (poly == null)
            {
                //m_application.SetBusy(false);
                return;
            }
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
                    PolygonClass polyDeleteSmallRing = new PolygonClass();
                    for (int j = 0; j < (poly as IGeometryCollection).GeometryCount; j++)
                    {
                        IRing smallRing = (poly as IGeometryCollection).get_Geometry(j) as IRing;
                        if (Math.Abs((smallRing as IArea).Area) > minarea * 0.6)
                        {
                            polyDeleteSmallRing.AddGeometry(smallRing, ref miss, ref miss);
                        }
                    }
                    polyDeleteSmallRing.Simplify();
                    if (!polyDeleteSmallRing.IsEmpty)
                    {
                        poly = polyDeleteSmallRing;
                    }
                    CenterLine cl = cf.Create2(poly);
                    PolylineClass resultPolyline = new PolylineClass();
                    resultPolyline.SpatialReference = pcs_;
                    IPolyline simplifyLine = new PolylineClass();
                    foreach (var info in cl)
                    {
                        if (info.Info.Triangles[info.Info.Triangles.Count - 1].TagValue != 1
                            && info.Info.Triangles[0].TagValue != 1
                            )
                        {
                            //simplifyLine = gen.SimplifyPolylineByDT2(info.Line, 8) as IPolyline;
                            simplifyLine = info.Line;
                            simplifyLine.Generalize(mindis * 0.2);
                            //simplifyLine.Smooth(0.2);//
                        }
                        
                        for (int j = 0; j < (simplifyLine as IGeometryCollection).GeometryCount; j++)
                        {
                            IGeometry geo = (simplifyLine as IGeometryCollection).get_Geometry(j);
                            IPolyline newLine = new PolylineClass();
                            IGeometryCollection newLineColl = newLine as IGeometryCollection;
                            newLineColl.AddGeometries(1, ref geo);
                            resultPolyline.AddGeometries(1, ref geo);
                            //centerLines.Add(newLine);
                        }

                    }
                    IPolyline centerLine = resultPolyline;

                    poly = (centerLine as ITopologicalOperator).Buffer(0.001) as IPolygon;
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
                }
                catch (Exception ex)
                {
                }
            }
            //List<IPolygon> tooMinAreaHole = new List<IPolygon>();
            editor.StartOperation();
            sBar.ShowProgressBar("正在提交结果...", 0, 1, 1, true);
            sPro.Position = 1;
            ISpatialFilter sf_add = new SpatialFilterClass();
            sf_add.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            //sf_add.WhereClause = queryf;
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
                            if (addIDs.Contains(findFeature.OID))
                            {
                                continue;
                            }
                            IArea insertPart = findFeature.Shape as IArea;
                            if (insertPart.Area > maxArea)
                            {
                                maxArea = insertPart.Area;
                                maxID = findFeature.OID;
                            }
                        }
                        catch { }
                    }
                    if (maxID == -1 && maxArea < minarea * 0.6)
                    {
                        continue;
                    }
                    findFeature = layer.FeatureClass.GetFeature(maxID);
                    (findFeature as IFeatureBuffer).Shape = addPoly;
                    //if (groupID > 0)
                    //    (findFeature as IFeatureBuffer).set_Value(groupID, -3);
                    addIDs.Add((int)insertCursor.InsertFeature(findFeature as IFeatureBuffer));
                }
            }
            insertCursor.Flush();
            //wo.Step(8);
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
            sBar.HideProgressBar();
            ac.Refresh();
            editor.StopOperation("湖泊毗邻");
        }
        #endregion
    }
}

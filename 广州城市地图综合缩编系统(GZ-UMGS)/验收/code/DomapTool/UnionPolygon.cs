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
    /// Summary description for UnionPolygon.
    /// </summary>
    [Guid("5b678af6-5da1-42eb-bca0-ddbbeab9c643")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DomapTool.UnionPolygon")]
    public sealed class UnionPolygon : BaseTool
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
        INewPolygonFeedback fb;
        private IApplication m_application;
        public UnionPolygon()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text 
            base.m_caption = "非邻近图斑合并";  //localizable text 
            base.m_message = "非邻近图斑合并";  //localizable text
            base.m_toolTip = "非邻近图斑合并";  //localizable text
            base.m_name = "DomapTool.UnionPolygon";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
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
            GGenPara.Para.RegistPara("非邻近图斑合并距离", 5);
        }

        #region Overriden Class Methods
        IEditor editor;
        IFeatureClass fc = null;
        IFeatureLayer Layer = null;
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
        double minDis;
        public override void OnClick()
        {
            LayersPara d = new LayersPara((m_application.Document as IMxDocument).ActiveView.FocusMap);
            if (d.ShowDialog() == DialogResult.Cancel)
                return;
            Layer = d.SelectLayer;
            minDis = d.dis;
            fc = Layer.FeatureClass;
            if (Layer == null)
            {
                MessageBox.Show("请确保操作图层不为空!");
                return;
            }
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
            // TODO:  Add UnionPolygon.OnMouseUp implementation
        }
        public override void OnDblClick()
        {
            if (fb == null)
                return;
            IPolygon p = fb.Stop();
            fb = null;
            Gen(p);
        }
        private void Gen(IPolygon range)
        {
            try
            {
                //editor.StartOperation();
                ISpatialFilter qf = new SpatialFilterClass();
                qf.Geometry = range;
                qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor fCur;
                IFeature f;
                fCur = fc.Update(qf, false);

                int count = fc.FeatureCount(qf);
                IStatusBar sBar = m_application.StatusBar;
                IStepProgressor sPro = sBar.ProgressBar;
                sPro.Position = 0;
                sBar.ShowProgressBar("处理中...", 0, count, 1, true);
                GeometryBagClass gb = new GeometryBagClass();
                IFeature fb = null;
                double maxArea = 0;
                List<int> toDeleteID = new List<int>();
                int maxAreaFeatOID = -1;
                while ((f = fCur.NextFeature()) != null)
                {
                    sBar.StepProgressBar();
                    IArea area = f.ShapeCopy as IArea;
                    IGeometry shape = f.ShapeCopy;
                    gb.AddGeometries(1, ref shape);
                    if (area.Area > maxArea)
                    {
                        //if(maxArea!=0)
                        //toDeleteID.Add(fb.OID);
                        //maxArea = area.Area;
                        //fb = f;

                        maxArea = area.Area;
                        maxAreaFeatOID = f.OID;
                        toDeleteID.Add(f.OID);
                    }
                    else
                    {
                        toDeleteID.Add(f.OID);
                    }
                }
                sBar.HideProgressBar();

                toDeleteID.Remove(maxAreaFeatOID);
                fb = fc.GetFeature(maxAreaFeatOID);

                #region 缓冲合并
                IPolygon poly = CompleteFeatureGroupHandle(gb, minDis);
                editor.StartOperation();
                if (fb != null && poly != null)
                {
                    fb.Shape = poly;
                    fb.Store();
                    int[] deleteID = toDeleteID.ToArray();
                    IFeature deleteFeat = null;
                    for (int j = 0; j < deleteID.Length; j++)
                    {
                        deleteFeat = fc.GetFeature(deleteID[j]);
                        deleteFeat.Delete();
                    }
                }
                else
                {
                    editor.StopOperation("合并");
                    return;
                }
                #endregion

                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCur);
                (m_application.Document as IMxDocument).ActiveView.Refresh();
                editor.StopOperation("合并");
            }
            catch (Exception ex)
            {
                editor.StopOperation("合并");
            }
        }
        #endregion
        private IPolygon CompleteFeatureGroupHandle(IGeometryCollection gb, double distance)
        {
            IPolygon tpPoly = new PolygonClass();
            for (int m = 0; m < gb.GeometryCount; m++)
            {
                bool isR = false;
                tpPoly = gb.get_Geometry(m) as IPolygon;
                IProximityOperator toProxi = tpPoly as IProximityOperator;
                for (int j = 0; j < gb.GeometryCount; j++)
                {
                    if (j == m)
                    {
                        continue;
                    }
                    IPolygon tpPoly_ = gb.get_Geometry(j) as IPolygon;
                    double dis = toProxi.ReturnDistance(tpPoly_);
                    if (dis < distance)
                    {
                        isR = true;
                        break;
                    }
                }
                if (isR == false)
                {
                    System.Windows.Forms.MessageBox.Show("Terrible!距离太远");
                    return null;
                }
            }
            if (gb.GeometryCount > 1)
            {
                object missing = Type.Missing;
                IGeometryCollection geoColOri = new GeometryBagClass();
                //把原始的Geometry放到geoColOri中
                geoColOri = gb;
                //得到合并后的Geometry,放在topo变量中
                ITopologicalOperator topo = geoColOri.get_Geometry(0) as ITopologicalOperator;
                topo = topo.Buffer(distance) as ITopologicalOperator;
                topo.Simplify();
                for (int i = 1; i < geoColOri.GeometryCount; i++)
                {
                    ITopologicalOperator topoIn = geoColOri.get_Geometry(i) as ITopologicalOperator;
                    topoIn = topoIn.Buffer(distance) as ITopologicalOperator;
                    topoIn.Simplify();
                    topo = topo.Union(topoIn as IGeometry) as ITopologicalOperator;
                }
                topo = topo.Buffer(-distance) as ITopologicalOperator;
                //找出桥，并把凹凸的地方抹掉
                IGeometryCollection geoBridgeCol = new GeometryBagClass();
                ITopologicalOperator topoBridge = topo;
                for (int i = 0; i < geoColOri.GeometryCount; i++)
                {
                    topoBridge = topoBridge.Difference(geoColOri.get_Geometry(i)) as ITopologicalOperator;
                    topoBridge.Simplify();
                }
                IGeometryCollection geoColDent = topoBridge as IGeometryCollection;
                for (int i = 0; i < geoColDent.GeometryCount; i++)//17
                {
                    IGeometry geo = geoColDent.get_Geometry(i);
                    if ((geo as IArea).Area < 0 || geo.IsEmpty || geo == null)
                    {
                        continue;
                    }
                    IPolygon polygon = TransformRingToPolygon(geo as IRing);
                    if ((polygon as IGeometry).IsEmpty)
                    {
                        continue;
                    }
                    IRelationalOperator relDent = polygon as IRelationalOperator;
                    int intersectCount = 0;
                    for (int j = 0; j < geoColOri.GeometryCount; j++)
                    {
                        if (relDent.Touches(geoColOri.get_Geometry(j)))
                        {
                            intersectCount++;
                        }
                    }
                    if (intersectCount < 2)
                    {
                        topo = topo.Difference(polygon) as ITopologicalOperator;
                    }
                    else
                    {
                        geoBridgeCol.AddGeometry(polygon, ref missing, ref missing);
                    }
                }
                //把原始的geometry合并到最后生成的合并好的几何体topo中
                double areaOfOri = 0;
                for (int i = 0; i < geoColOri.GeometryCount; i++)
                {
                    areaOfOri += (geoColOri.get_Geometry(i) as IArea).Area;
                    topo = topo.Union(geoColOri.get_Geometry(i)) as ITopologicalOperator;
                    topo.Simplify();
                }
                IPolygon result = topo as IPolygon;
                return result;

            }
            else
            {
                return null;
            }

        }
        private IPolygon TransformRingToPolygon(IRing ring)
        {
            try
            {
                IPolygon poly;
                ITopologicalOperator topoOper;

                IGeometryCollection geoCol = new PolygonClass();
                object missing = Type.Missing;
                geoCol.AddGeometry(ring as IGeometry, ref missing, ref missing);
                poly = geoCol as IPolygon;
                topoOper = poly as ITopologicalOperator;
                topoOper.Simplify();

                return poly;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }
    }
}

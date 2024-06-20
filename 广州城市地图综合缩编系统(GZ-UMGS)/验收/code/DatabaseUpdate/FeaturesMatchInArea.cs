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
using ESRI.ArcGIS.Display;

namespace DatabaseUpdate
{
    /// <summary>
    /// Summary description for FeaturesMatchInArea.
    /// </summary>
    [Guid("d78f579c-d803-4bb1-9276-bf5dc2ba39a5")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DatabaseUpdate.FeaturesMatchInArea")]
    public sealed class FeaturesMatchInArea : BaseTool
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
        SimpleFillSymbolClass sfs;
        SimpleFillSymbolClass sfs2;
        SimpleMarkerSymbolClass sms;
        private IApplication m_application;
        public FeaturesMatchInArea()
        {
            sfs = new SimpleFillSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 255;
            rgb.Blue = 0;
            rgb.Green = 0;
            SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
            sls.Width = 2;
            sls.Color = rgb;
            sfs.Outline = sls;
            sfs.Style = esriSimpleFillStyle.esriSFSNull;

            sfs2 = new SimpleFillSymbolClass();
            RgbColorClass rgb2 = new RgbColorClass();
            rgb2.Red = 0;
            rgb2.Blue = 0;
            rgb2.Green = 0;
            SimpleLineSymbolClass sls2 = new SimpleLineSymbolClass();
            sls2.Width = 2;
            sls2.Color = rgb2;
            sfs2.Outline = sls2;
            sfs2.Style = esriSimpleFillStyle.esriSFSNull;

            sms = new SimpleMarkerSymbolClass();
            sms.Size = 9;
            sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text 
            base.m_caption = "区域更新";  //localizable text 
            base.m_message = "区域更新";  //localizable text
            base.m_toolTip = "区域更新";  //localizable text
            base.m_name = "区域更新";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
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
        /// Occurs when this tool is clicked
        /// </summary>
        bool initDraw = false;
        bool initDraw2 = false;
        IFeatureClass fc = null;
        IFeatureLayer fLayer = null;
        IActiveView ac = null;
        IActiveView ac2 = null;
        public override void OnClick()
        {
            UApplication app = UApplication.Application;
            ULayerInfos mxlayers = app.Workspace.MxLayers;
            ULayerInfos mclayers = app.Workspace.McLayers;
            foreach (IFeatureLayer mxlayer in mxlayers.Layers.Values)
            {
                if (mxlayer.Name == "房屋面" || mxlayer.Name == "水系面" || mxlayer.Name == "绿地面")
                {
                    fLayer = mxlayer;
                    fc = mxlayer.FeatureClass;
                    break;
                }
            }
            if (fLayer == null || fc == null)
            {
                System.Windows.Forms.MessageBox.Show("请加入指定图层！");
                return;
            }
            ac = (m_application.Document as IMxDocument).ActiveView;
            ac2 = UApplication.Application.EsriMapControl.ActiveView;
            if (!initDraw)
            {
                IActiveViewEvents_Event events = ac as IActiveViewEvents_Event;
                events.AfterDraw += new IActiveViewEvents_AfterDrawEventHandler(events_AfterDraw);
                initDraw = true;
            }
            if (!initDraw2)
            {
                UApplication.Application.EsriMapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
                initDraw2 = true;
            }

            ISelectionSet set = (fLayer as IFeatureSelection).SelectionSet;
            GeometryBagClass gb = new GeometryBagClass();
            ICursor cu = null;
            set.Search(null, false, out cu);
            IFeatureCursor fcur = cu as IFeatureCursor;
            IFeature feat = null;
            object miss = Type.Missing;
            while ((feat = fcur.NextFeature()) != null)
            {
                gb.AddGeometry(feat.ShapeCopy, ref miss, ref miss);
            }
            if (gb.GeometryCount == 0)
            {
                System.Windows.Forms.MessageBox.Show("请选择需要更新的要素！");
                return;
            }
            oriPolygon=new PolygonClass();
            (oriPolygon as ITopologicalOperator).ConstructUnion(gb);
            oriPolygon = (oriPolygon as ITopologicalOperator).ConvexHull() as IPolygon;
            ac.Refresh();
            ac2.Refresh();
            UApplication.Application.EsriMapControl.ActiveView.Refresh();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur);

        }

        void events_AfterDraw(ESRI.ArcGIS.Display.IDisplay dis, esriViewDrawPhase phase)
        {
            if (SelectPolygon == null && oriPolygon == null)
            {
                return;
            }
            if (SelectPolygon != null)
            {
                dis.SetSymbol(sfs);
                dis.DrawPolygon(SelectPolygon);
                dis.SetSymbol(sms);
                for (int i = 0; i < (SelectPolygon as IPointCollection).PointCount; i++)
                {
                    IPoint p = (SelectPolygon as IPointCollection).get_Point(i);
                    dis.DrawPoint(p);
                }
            }
            else
            {
                dis.SetSymbol(sfs2);
                dis.DrawPolygon(oriPolygon);
            }
        }
        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e)
        {
            if (SelectPolygon == null && oriPolygon == null)
            {
                return;
            }
            IDisplay dis = e.display as IDisplay;
            if (SelectPolygon != null)
            {
                dis.SetSymbol(sfs);
                dis.DrawPolygon(SelectPolygon);
                dis.SetSymbol(sms);
                for (int i = 0; i < (SelectPolygon as IPointCollection).PointCount; i++)
                {
                    IPoint p = (SelectPolygon as IPointCollection).get_Point(i);
                    dis.DrawPoint(p);
                }
            }
            else
            {
                dis.SetSymbol(sfs2);
                dis.DrawPolygon(oriPolygon);
            }
        }

        IPolygon oriPolygon = null;
        IPolygon SelectPolygon = null;
        IPolygonMovePointFeedback fb;
        IPoint lastPoint = null;

        public override void OnDblClick()
        {
            if (lastPoint == null)
                return;
            if (fc == null)
                return;
            if (oriPolygon == null)
                return;
            IRelationalOperator selectRe = oriPolygon as IRelationalOperator;
            if (selectRe.Contains(lastPoint))
            {
                SelectPolygon = oriPolygon;
            }
            else
            {
                SelectPolygon = null;
            }
            ac.Refresh();
            ac2.Refresh();

        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            if (SelectPolygon == null)
                return;
            IPoint p = ac.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            double mapdis = ac.ScreenDisplay.DisplayTransformation.FromPoints(3);
            if (Button == 1 && Shift == 1)
            {
                IHitTest hit = SelectPolygon as IHitTest;
                IPoint hitpoint = new PointClass();
                double distance = 0;
                int partIndex = -1;
                int segIndex = -1;
                bool r = false;
                bool isHit = hit.HitTest(p, mapdis, esriGeometryHitPartType.esriGeometryPartBoundary, hitpoint, ref distance, ref partIndex, ref segIndex, ref r);
                if (isHit)
                {
                    IRing ring = (hit as IGeometryCollection).get_Geometry(partIndex) as IRing;
                    (ring as IPointCollection).InsertPoints(segIndex + 1, 1, ref hitpoint);
                    IGeometry geo = ring;
                    (hit as IGeometryCollection).SetGeometries(1, ref geo);
                    //selectFeature.Shape = hit as IGeometry;
                    //selectFeature.Store();
                    ac.Refresh();
                    ac2.Refresh();
                }
            }//增加节点
            else if (Button == 2)
            {
                IHitTest hit = SelectPolygon as IHitTest;
                IPoint hitpoint = new PointClass();
                double distance = 0;
                int partIndex = -1;
                int segIndex = -1;
                bool r = false;
                bool isHit = hit.HitTest(p, mapdis, esriGeometryHitPartType.esriGeometryPartVertex, hitpoint, ref distance, ref partIndex, ref segIndex, ref r);
                if (isHit)
                {
                    IRing ring = (hit as IGeometryCollection).get_Geometry(partIndex) as IRing;
                    (ring as IPointCollection).RemovePoints(segIndex, 1);
                    IGeometry geo = ring;
                    (hit as IGeometryCollection).SetGeometries(1, ref geo);
                    ac.Refresh();
                    ac2.Refresh();
                }
            }//删除节点
            else if (Button == 1 && Shift == 0)
            {

                IHitTest hit = SelectPolygon as IHitTest;
                IPoint hitpoint = new PointClass();
                double distance = 0;
                int partIndex = -1;
                int segIndex = -1;
                bool r = false;
                bool isHit = hit.HitTest(p, mapdis, esriGeometryHitPartType.esriGeometryPartVertex, hitpoint, ref distance, ref partIndex, ref segIndex, ref r);
                if (isHit)
                {
                    fb = new PolygonMovePointFeedbackClass();
                    fb.Display = ac.ScreenDisplay;
                    int index = 0;
                    for (int i = 0; i < partIndex; i++)
                    {
                        IPointCollection pc = (hit as IGeometryCollection).get_Geometry(i) as IPointCollection;
                        index += pc.PointCount;
                    }
                    index += segIndex;
                    fb.Start(hit as IPolygon, index, hitpoint);
                }
            }//移动节点
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint p = ac.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            lastPoint = p;

            if (fb != null)
            {
                fb.MoveTo(p);
            }
        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            if (fb != null)
            {
                SelectPolygon = fb.Stop();
                fb = null;
                ac.Refresh();
                ac2.Refresh();
            }
        }
        public override void OnKeyDown(int keyCode, int Shift)
        {
            switch ((System.Windows.Forms.Keys)keyCode)
            {
                case System.Windows.Forms.Keys.Escape:
                case System.Windows.Forms.Keys.Space:
                    SelectPolygon = null;
                    ac.Refresh();
                    ac2.Refresh();
                    break;
            }
        }
        #endregion
    }
}

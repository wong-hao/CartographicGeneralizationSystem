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
    /// Summary description for RoadStrokeTool.
    /// </summary>
    [Guid("d75ee670-3e3c-4a07-aa48-a5efa713254a")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DatabaseUpdate.RoadStrokeTool")]
    public sealed class RoadStrokeTool : BaseTool
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
        public RoadStrokeTool()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text 
            base.m_caption = "构建Stroke";  //localizable text 
            base.m_message = "构建Stroke";  //localizable text
            base.m_toolTip = "构建Stroke";  //localizable text
            base.m_name = "DomapTool.RoadStrokeTool";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
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
        IEditor editor;
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
        int rankID;
        public override void OnClick()
        {
            DoUpdate d = new DoUpdate((m_application.Document as IMxDocument).ActiveView.FocusMap);
            d.Text = "构建Stroke";
            if (d.ShowDialog() == DialogResult.Cancel)
                return;
            roadlayers = d.SelectLayer;
            fc = roadlayers.FeatureClass;
            if (roadlayers == null || fc.ShapeType != esriGeometryType.esriGeometryPolyline)
            {
                MessageBox.Show("请确保操作图层有效!");
                return;
            }
            if (fc == null) return;
            rankID = fc.Fields.FindField("道路等级");
            if (rankID == -1)
            {
                MessageBox.Show("请添加字段“道路等级”！");
                return;
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
            // TODO:  Add RoadStrokeTool.OnMouseUp implementation
        }

        public override void OnDblClick()
        {
            if (fb == null)
                return;
            IPolygon p = fb.Stop();
            fb = null;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = p;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            (roadlayers as IFeatureSelection).SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
            FindStrokes();
        }

        public override bool Deactivate()
        {
            return base.Deactivate();
        }
        public override void OnKeyUp(int keyCode, int Shift)
        {
            //if (fc == null) return;
            //int rankID = fc.Fields.FindField("道路等级");
            //if (rankID == -1)
            //{
            //    MessageBox.Show("请添加字段“道路等级”！");
            //    return;
            //}
            //editor.StartOperation();
            ISelectionSet set = (roadlayers as IFeatureSelection).SelectionSet;
            switch (keyCode)
            {

                case (int)System.Windows.Forms.Keys.D1:
                    editor.StartOperation();
                    ICursor cur = null;
                    set.Search(null, true, out cur);
                    IFeatureCursor fCursor = cur as IFeatureCursor;
                    IFeature tpFeat = null;
                    while ((tpFeat = fCursor.NextFeature()) != null)
                    {
                        object beforeRank = tpFeat.get_Value(rankID);
                        if (beforeRank == null||beforeRank == DBNull.Value)
                        {
                            tpFeat.set_Value(rankID, 1);
                        }
                        else
                        {
                            int br = Convert.ToInt16(beforeRank);
                            if (br == 1)
                            {
                                tpFeat.set_Value(rankID, 1);
                                tpFeat.Store();
                            }
                            else
                            {
                                tpFeat.set_Value(rankID, --br);
                                tpFeat.Store();
                            }
                        }
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cur);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                    editor.StopOperation("改等级");
                    break;
                case (int)System.Windows.Forms.Keys.D2:
                    editor.StartOperation();
                    ICursor cur2 = null;
                    set.Search(null, true, out cur2);
                    IFeatureCursor fCursor2 = cur2 as IFeatureCursor;
                    IFeature tpFeat2 = null;
                    while ((tpFeat2 = fCursor2.NextFeature()) != null)
                    {
                        object beforeRank = tpFeat2.get_Value(rankID);
                        if (beforeRank == null ||beforeRank== DBNull.Value)
                        {
                            tpFeat2.set_Value(rankID, 1);
                            tpFeat2.Store();
                        }
                        else
                        {
                            int br = Convert.ToInt16(beforeRank);
                            br += 1;
                            if (br >= 4)
                            {
                                br = 4;
                            }
                            tpFeat2.set_Value(rankID, br);
                            tpFeat2.Store();
                        }
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cur2);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor2);
                    editor.StopOperation("改等级");
                    break;
            }
            //editor.StopOperation("改等级");
        }
        #endregion


        //Dictionary<int, List<IFeature>> stroke_feature_dic = new Dictionary<int, List<IFeature>>();
        Dictionary<int, int> fid_stroke_dic = new Dictionary<int, int>();
        IFeatureClass fc = null;
        IFeatureLayer roadlayers = null;
        public void FindStrokes()
        {
            fid_stroke_dic.Clear();
            ISelectionSet set = (roadlayers as IFeatureSelection).SelectionSet;
            ICursor cur=null;
            set.Search(null, true, out cur);
            IFeatureCursor fCursor = cur as IFeatureCursor;
            IFeature lineFeature = null;
            int strokeValue = 1;
            while ((lineFeature = fCursor.NextFeature()) != null)
            {
                try
                {
                    if (fid_stroke_dic.ContainsKey(lineFeature.OID))
                        continue;
                    int count = 0;
                    strokeLine info = FindStrokeLine(fc, lineFeature.OID, true);
                    while (info != null)
                    {
                        if (fid_stroke_dic.ContainsKey(info.OID))
                        {
                            break;
                        }
                        else
                        {
                            fid_stroke_dic.Add(info.OID, strokeValue);
                            set.Add(info.OID);
                        }
                        count++;
                        info = FindStrokeLine(fc, info.OID, !(info.fromPoint));
                    }
                    info = FindStrokeLine(fc, lineFeature.OID, false);
                    while (info != null)
                    {
                        if (fid_stroke_dic.ContainsKey(info.OID))
                        {
                            break;
                        }
                        else
                        {
                            fid_stroke_dic.Add(info.OID, strokeValue);
                            set.Add(info.OID);
                        }
                        count++;
                        info = FindStrokeLine(fc, info.OID, !(info.fromPoint));
                    }

                    if (!fid_stroke_dic.ContainsKey(lineFeature.OID))
                    {
                        fid_stroke_dic.Add(lineFeature.OID, strokeValue);
                        set.Add(info.OID);
                    }
                    strokeValue++;
                                       
                }
                catch
                {
                }
            }
            (m_application.Document as IMxDocument).ActiveView.Refresh();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cur);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);

        }

        internal class strokeLine
        {
            internal int OID;
            internal bool fromPoint;
            internal double length;
        }
        internal static strokeLine FindStrokeLine(IFeatureClass fc, int oid, bool fromPoint)
        {
            IFeature self = fc.GetFeature(oid);
            strokeLine result = new strokeLine();

            IPoint passPoint = null;
            IPoint beforePoint = null;
            if (fromPoint)
            {
                passPoint = (self.Shape as IPolyline).FromPoint;
                beforePoint = (self.Shape as IPointCollection).get_Point(1);
            }
            else
            {
                passPoint = (self.Shape as IPolyline).ToPoint;
                beforePoint = (self.Shape as IPointCollection).get_Point((self.Shape as IPointCollection).PointCount - 2);
            }
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = (passPoint as ITopologicalOperator).Buffer(5);
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int count = fc.FeatureCount(sf);
            if (count < 2)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(self);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(sf);
                return null;
            }
            else
            {
                IFeatureCursor fCursor = fc.Search(sf, true);
                IFeature f = null;
                double minAngle = double.MaxValue;
                result.OID = -1;
                result.fromPoint = true;
                while ((f = fCursor.NextFeature()) != null)
                {
                    if (f.OID == self.OID)
                    {
                        continue;
                    }
                    IPoint fp = (f.Shape as IPolyline).FromPoint;
                    if ((passPoint as IProximityOperator).ReturnDistance(fp) < 0.01)
                    {
                        IPoint toPoint = (f.Shape as IPointCollection).get_Point(1);
                        double a = Angle(beforePoint, passPoint, toPoint);
                        if (a < minAngle && a < Math.PI / 6)
                        {
                            result.OID = f.OID;
                            result.fromPoint = true;
                            result.length = (f.Shape as IPolyline).Length;
                        }
                    }
                    IPoint tp = (f.Shape as IPolyline).ToPoint;
                    if ((passPoint as IProximityOperator).ReturnDistance(tp) < 0.01)
                    {
                        IPoint toPoint = (f.Shape as IPointCollection).get_Point((f.Shape as IPointCollection).PointCount - 2);
                        double a = Angle(beforePoint, passPoint, toPoint);
                        if (a < minAngle && a < Math.PI / 6)
                        {
                            result.OID = f.OID;
                            result.fromPoint = false;
                            result.length = (f.Shape as IPolyline).Length;
                        }
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(self);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(sf);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);

                if (result.OID != -1)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }
        static double Angle(IPoint from, IPoint pass, IPoint to)
        {
            IConstructAngle ca = new GeometryEnvironmentClass();
            double a = ca.ConstructThreePoint(from, pass, to);
            return Math.PI - Math.Abs(a);
        }
    }
}

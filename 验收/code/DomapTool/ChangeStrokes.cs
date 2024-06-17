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
using System.Windows.Forms;
namespace DomapTool
{
    /// <summary>
    /// Summary description for ChangeStrokes.
    /// </summary>
    [Guid("12c1f474-9f6d-4fd3-81e4-957638c8ea0c")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DatabaseUpdate.ChangeStrokes")]
    public sealed class ChangeStrokes : BaseTool
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
        INewEnvelopeFeedback fb;
        public ChangeStrokes()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text 
            base.m_caption = "改变等级";  //localizable text 
            base.m_message = "改变等级";  //localizable text
            base.m_toolTip = "改变等级";  //localizable text
            base.m_name = "DomapTool.ChangeStrokes";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
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
        int rankID;
        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            DoUpdate d = new DoUpdate((m_application.Document as IMxDocument).ActiveView.FocusMap);
            d.Text = "更改等级";
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
                fb = new NewEnvelopeFeedbackClass();
                fb.Display = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay;
                fb.Start(p);
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
        IFeatureClass fc = null;
        IFeatureLayer roadlayers = null;
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            if (fb != null)
            {
                IEnvelope env = fb.Stop();
                fb = null;
                //int rankID = fc.Fields.FindField("道路等级");
                //if (rankID == -1)
                //{
                //    MessageBox.Show("请添加字段“道路等级”！");
                //}
                //IEditor editor = m_application.FindExtensionByName("ESRI Object Editor") as IEditor;
                editor.StartOperation();

                int s = -1;
                if (Shift == 1)
                {
                    s = 1;
                }
                SpatialFilterClass sf = new SpatialFilterClass();
                sf.Geometry = env;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor fCursor = fc.Update(sf, true);
                IFeature feature = null;
                while ((feature = fCursor.NextFeature()) != null)
                {
                    object v = feature.get_Value(rankID);
                    int rank = 4;
                    if (v != null && v != DBNull.Value)
                    {
                        rank = Convert.ToInt32(v);
                    }
                    rank += s;
                    while (rank > 4)
                    {
                        rank--;
                    }
                    while (rank < 1)
                    {
                        rank++;
                    }
                    feature.set_Value(rankID, rank);
                    fCursor.UpdateFeature(feature);
                }

                fCursor.Flush();
                editor.StopOperation("改等级");
            }
        }      

        public override bool Deactivate()
        {
            return base.Deactivate();
        }
        
        #endregion
    }
}

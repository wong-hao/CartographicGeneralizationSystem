using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Editor;


namespace SMGI.Plugin.GeneralEdit
{
    public class ReShapeAllTool:SMGITool
    {
        private NewLineFeedbackClass fb;
        private MovePointFeedbackClass mp_fb;
        public ReShapeAllTool()
        {
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
        }

        public override void OnClick()
        {
            fb = null;
            mp_fb = new MovePointFeedbackClass();
            var view = m_Application.ActiveView;
            mp_fb.Display = view.ScreenDisplay;
            mp_fb.Start(view.Extent.UpperLeft, view.Extent.UpperLeft);
            var sms = mp_fb.Symbol as ISimpleMarkerSymbol;
            sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
            sms.Size = 8;
            mp_fb.Symbol = sms as ISymbol;

            var editor = m_Application.EngineEditor;
            IEngineEditSketch es = editor as IEngineEditSketch;
            try
            {
                es.FinishSketchPart();
                es.FinishSketch();
            }
            catch
            { }
            
        }

        public override bool Deactivate()
        {
            if (mp_fb!=null)
            {
                mp_fb.Stop();
                mp_fb = null;
                
            }
            return base.Deactivate();
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button != 1)
                return;
            var editor = m_Application.EngineEditor;
            IEngineEditSketch es = editor as IEngineEditSketch;
            //es.EditSketchExtension = new DimensionEditExtensionClass ();
            if (fb == null)
            {
                es.GeometryType = esriGeometryType.esriGeometryPolyline;
                es.Geometry = new PolylineClass();
            }
            var pt = ToSnapedMapPoint(X, Y);
            es.AddPoint(pt, true);
            if (fb != null)
            {
                fb.Stop();
                fb = null;
            }

            fb = new NewLineFeedbackClass();
            var dis = m_Application.ActiveView.ScreenDisplay;
            fb.Display = dis;
            fb.Start(pt);
        }
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            var pt = ToSnapedMapPoint(X, Y);
            mp_fb.MoveTo(pt);
            if (fb != null)
            {
                fb.MoveTo(pt);
            }
        }
        public override void OnDblClick()
        {
            if (fb == null)
                return;

            if (fb != null)
            {
                fb.Stop();
                fb = null;
            }

            var editor = m_Application.EngineEditor;
            IEngineEditSketch es = editor as IEngineEditSketch;
            IPolyline line = es.Geometry as IPolyline;

            try
            {
                es.FinishSketch();
            }
            catch
            { }
            if (line.IsEmpty)
                return;


            

            IPath path = (line as IGeometryCollection).Geometry[0] as IPath;
            var view = m_Application.ActiveView;
            
            var ef = editor.EditSelection;
            editor.StartOperation();
            bool anyoneReshaped = false;

            IFeature f = null;       
            while ((f = ef.Next())!=null)
            {
                if (!(f.Shape is IPolycurve))
                {
                    continue;
                }
                IPolycurve curve = f.ShapeCopy as IPolycurve;
                if (curve.Reshape(path))
                {
                    anyoneReshaped = true;
                    f.Shape = curve;
                    f.Store();
                }
            }

            if (anyoneReshaped)
            {
                editor.StopOperation(this.Caption);
                view.Refresh();
            }
            else
            {
                editor.AbortOperation();
                MessageBox.Show("没有任何目标被修改！");
            }
        }
        public override void Refresh(int hDC)
        {
            if (fb != null)
                fb.Refresh(hDC);
            if (mp_fb != null)
                mp_fb.Refresh(hDC);
        }

         public override bool Enabled
         {
             get
             {
                 return m_Application != null
                     && m_Application.Workspace != null
                     && m_Application.ActiveView.FocusMap.SelectionCount >= 2 
                     && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
             }
         }
    }
}

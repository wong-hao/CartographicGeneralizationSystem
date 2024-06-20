using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace SMGI.Common
{
    public abstract class SMGITool : SMGICommand, ITool
    {
        protected Cursor m_cursor;

        protected ISnappingEnvironment m_snappingEnv;
        protected IPointSnapper m_snapper;
        protected ISnappingFeedback m_snappingFeedback;

        private bool m_needSnap;
        protected bool NeedSnap
        {
            get
            {
                return m_needSnap;
            }
            set
            {
                m_needSnap = value;
                if (!m_needSnap && m_snappingFeedback != null)
                {
                    m_snappingFeedback.Update(null, 0);
                }
            }
        }
        protected void ClearSnapperCache()
        {
            m_snapper.ClearCache();
        }

        protected SMGITool()
            : base()
        {
            m_cursor = Cursors.Arrow;
            m_needSnap = true;

            this.Clicked += new Action(SMGITool_Clicked);
        }

        void SMGITool_Clicked()
        {
            //捕捉
            IHookHelper hookHelper = new HookHelperClass();
            hookHelper.Hook = m_Application.MapControl.Object;

            IExtensionManager extensionManager = (hookHelper as IHookHelper2).ExtensionManager;
            if (extensionManager != null)
            {
                UID guid = new UIDClass();
                guid.Value = "{E07B4C52-C894-4558-B8D4-D4050018D1DA}"; //Snapping extension.
                IExtension extension = extensionManager.FindExtension(guid);
                m_snappingEnv = extension as ISnappingEnvironment;
                m_snapper = m_snappingEnv.PointSnapper;
                m_snappingFeedback = new SnappingFeedbackClass();
                m_snappingFeedback.Initialize(hookHelper.Hook, m_snappingEnv, true);
            }
        }

        public virtual int Cursor
        {
            get { return m_cursor.Handle.ToInt32(); }
        }

        public virtual bool Deactivate()
        {
            return true;
        }
        public virtual bool OnContextMenu(int x, int y)
        {
            return false;
        }

        public virtual void OnDblClick()
        {

        }
        public virtual void OnKeyDown(int keyCode, int shift)
        {

        }
        public virtual void OnKeyUp(int keyCode, int shift)
        {

        }

        public virtual void OnMouseDown(int button, int shift, int x, int y)
        {

        }
        public virtual void OnMouseMove(int button, int shift, int x, int y)
        {
        }
        public virtual void OnMouseUp(int button, int shift, int x, int y)
        {

        }
        public virtual void Refresh(int hdc)
        {

        }

        public IPoint ToSnapedMapPoint(int x, int y)
        {
            IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            if (m_needSnap)
            {
                ISnappingResult snapResult = m_snapper.Snap(currentMouseCoords);
                if (snapResult != null)
                    currentMouseCoords = snapResult.Location;
            }


            return currentMouseCoords;
        }

        bool ITool.OnContextMenu(int x, int y)
        {
            return this.OnContextMenu(x, y);

        }

        bool ITool.Deactivate()
        {
            return this.Deactivate();
        }

        void ITool.OnMouseDown(int button, int shift, int x, int y)
        {
            this.OnMouseDown(button, shift, x, y);
        }

        void ITool.OnMouseMove(int button, int shift, int x, int y)
        {
            if (m_needSnap)
            {
                //捕捉
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                ISnappingResult snapResult = m_snapper.Snap(currentMouseCoords);
                m_snappingFeedback.Update(snapResult, 0);
                if (snapResult != null)
                {
                    currentMouseCoords = snapResult.Location;
                }

                m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(currentMouseCoords, out x, out y);
            }


            this.OnMouseMove(button, shift, x, y);
        }

        void ITool.OnMouseUp(int button, int shift, int x, int y)
        {
            this.OnMouseUp(button, shift, x, y);
        }

        void ITool.Refresh(int hdc)
        {
            //捕捉
            if (m_snappingFeedback != null)
                m_snappingFeedback.Refresh(hdc);

            this.Refresh(hdc);
        }


    }
}

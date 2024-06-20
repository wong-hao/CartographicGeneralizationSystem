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
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public  class SelectorSimpleTool:SMGITool
    {
        class Win32Window : System.Windows.Forms.IWin32Window
        {
            public IntPtr Handle
            {
                get;
                set;
            }
        }
        public SelectorSimpleTool()
        {
            m_caption = "选择工具";
            currentTool = new ControlsSelectFeaturesToolClass();

            NeedSnap = false;
        }

        ControlsSelectFeaturesToolClass currentTool;
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            currentTool.OnCreate(m_Application.MapControl.Object);
        }
        Win32Window mapWin = new Win32Window();
        public override void OnClick()
        {
            currentTool.OnClick();
            mapWin.Handle =new IntPtr(m_Application.MapControl.hWnd);
        }

        public override bool Deactivate()
        {
            return currentTool.Deactivate();
        }

        public override bool OnContextMenu(int x, int y)
        {
            return currentTool.OnContextMenu(x,y);
        }

        public override void OnDblClick()
        {
            currentTool.OnDblClick();
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
            currentTool.OnKeyDown(keyCode, shift);
        }
        FrmSelectionDo frm = null;
        public override void OnKeyUp(int keyCode, int shift)
        {
            currentTool.OnKeyUp(keyCode, shift);
             //空格键进行弹窗
            if (keyCode == 32)
            {
                if (frm == null || frm.IsDisposed)
                {
                    frm = new FrmSelectionDo();
                }
                else
                {
                    frm.LoadFeatures();
                }
                frm.Show(mapWin);
                frm.TopMost = true;
                frm.Activate();
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            currentTool.OnMouseDown(button, shift, x, y);
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            currentTool.OnMouseMove(button, shift, x, y);
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            currentTool.OnMouseUp(button, shift, x, y);
            if (frm != null && !frm.IsDisposed)
            {
                    frm.LoadFeatures();
                    frm.Show();
                    frm.TopMost = true;
                    frm.Activate();
            }
               
        }

        public override void Refresh(int hdc)
        {
            currentTool.Refresh(hdc);
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }
    }
}

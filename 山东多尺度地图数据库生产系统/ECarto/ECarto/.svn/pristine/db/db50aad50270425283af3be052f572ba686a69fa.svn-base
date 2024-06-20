using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;

namespace SMGI.Plugin.EmergencyMap
{
     
    public sealed class LabelLineEllipticTool :SMGITool
    {

        public LabelLineEllipticTool()
        {
            
        }
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace!=null;
            }
        }
        #region Overridden Class Methods

      

        IActiveView act;
        string text = string.Empty;

        bool isCheck = false;
        bool isDraw = false;
        string txtType = "Õ÷‘≤";
        public override void OnClick()
        {
            isCheck = false;
            act = m_Application.ActiveView;
            FrmLabelLineSet frm = new FrmLabelLineSet(txtType);
            if (DialogResult.OK == frm.ShowDialog())
            {
                text = frm.TxtValue;
                isCheck = true;
            }
        }
        public override bool Deactivate()
        {
            LabelClass.Instance.ActiveDefaultLayer();
            isCheck = false;
            isDraw = false;
            text = string.Empty;
            return base.Deactivate();
        }
        
        public override void OnKeyUp(int keyCode, int shift)
        {
            if (keyCode == 32)
            {
                isCheck = false;
                FrmLabelLineSet frm = new FrmLabelLineSet(txtType);
                if (DialogResult.OK == frm.ShowDialog())
                {
                    text = frm.TxtValue;
                    isCheck = true;
                }
            }
        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (!isCheck)
                return;
            if (isDraw || Button != 1)
            {
                return;
            }
            IPoint pp = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            LabelClass.Instance.CreateLabelLine(text, pp);
            isDraw = true;
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (!isCheck)
                return;
            if (isDraw)
            {
                IPoint pp = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                if (Shift == 1)
                {
                    LabelClass.Instance.MoveLabelLine8Dir(pp);
                }
                else
                {
                    LabelClass.Instance.MoveLabelLine(pp);
                }
            }
        }
        public override void OnDblClick()
        {
            if (!isCheck)
                return;
            if (!isDraw)
                return;
            LabelClass.Instance.LabelLineToMap();
            isDraw = false;
            
        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add CreateLabelLineTool.OnMouseUp implementation
        }
        #endregion
    }
}

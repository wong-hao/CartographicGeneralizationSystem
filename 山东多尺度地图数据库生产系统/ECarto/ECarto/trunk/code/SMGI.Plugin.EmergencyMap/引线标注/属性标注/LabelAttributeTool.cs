using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ESRI.ArcGIS.Controls;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Xml.Linq;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Maplex;
using ESRI.ArcGIS.Display;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.EmergencyMap
{
  
    public class LabelAttributeTool : SMGI.Common.SMGITool
    {
        public LabelAttributeTool()
        {
            m_caption = "属性标注";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
                     
            }
        }
        IActiveView act;
        string text = string.Empty;

        bool isCheck = false;
        bool isDraw = false;
        public override void OnClick()
        {
            act = m_Application.ActiveView;
            //设置窗体
            var frm = new FrmLabelLyr();
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            isCheck = true;
            LabelClass.Instance.LabAttrDt = frm.targetDt;
        }
        public override bool Deactivate()
        {
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
                var frm = new FrmLabelLyr();
                if (frm.ShowDialog() != DialogResult.OK)
                {
                    LabelClass.Instance.LabAttrDt = frm.targetDt;
                    isCheck = true;
                    isDraw = false;
                }
            }
        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (!isCheck)
                return;
            if (Button == 2)//右键取消
            {
                LabelClass.Instance.LabelLineCancel();
                isDraw = false;

            }
            if (isDraw || Button != 1)
            {
                return;
            }
            
            IRubberBand pRubberBand = new RubberRectangularPolygonClass();
            var geo = pRubberBand.TrackNew(act.ScreenDisplay, null);
            if (geo == null || geo.IsEmpty)
            {
                geo = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y); ;
            }
            if (geo == null || geo.IsEmpty)
            {
                return;
            }
            IPoint pp = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            isDraw = LabelClass.Instance.CreateAttributeLable(pp, geo);
            
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
       
    }
   
        
}

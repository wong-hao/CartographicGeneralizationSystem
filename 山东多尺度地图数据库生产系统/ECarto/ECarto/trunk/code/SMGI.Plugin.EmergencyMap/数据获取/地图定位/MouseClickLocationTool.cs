using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.EmergencyMap
{
    public class MouseClickLocationTool : SMGITool
    {
        public MouseClickLocationTool()
        {
            m_caption = "单击定位";
            m_toolTip = "鼠标单击进行定位";
            m_category = "辅助";

            NeedSnap = false;
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null && m_Application.MapControl.Map.LayerCount > 0;
            }
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
        }

        public override void OnClick()
        {
            
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            IPoint centerPoint = ToSnapedMapPoint(x, y);//裁切中心点

            MouseClickLocationForm frm = new MouseClickLocationForm(m_Application);
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = new System.Drawing.Point(x, y);
            frm.centerPoint = centerPoint;
            frm.ShowDialog();
            
                //PaperSize ps = frm.ClipPaperSize;

                //IGroupElement clipELe = ClipElement.createClipGroupElement(m_Application, centerPoint, ps.PaperWidth, ps.PaperHeight, frm.RefScale);
                //ClipElement.SetClipGroupElement(m_Application, clipELe);
            
        }

        public override bool Deactivate()
        {

            return base.Deactivate();
        }
    }
}

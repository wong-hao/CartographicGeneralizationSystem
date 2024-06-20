using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
namespace SMGI.Plugin.EmergencyMap
{
    public class MapPageMoveCmd : SMGICommand
    {
        public MapPageMoveCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null &&
                    ClipElement.GetClipRangeElement(m_Application.MapControl.ActiveView.GraphicsContainer) != null;;
            }
        }

        public override void OnClick()
        {
            FrmEleMove frmmove = new FrmEleMove();
            frmmove.Show();
            frmmove.Activate();
        }
    }
}

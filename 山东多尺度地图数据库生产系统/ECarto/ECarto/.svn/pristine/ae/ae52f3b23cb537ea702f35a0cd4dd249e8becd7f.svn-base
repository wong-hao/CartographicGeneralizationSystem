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
    public class MapSizeInfoCmd : SMGICommand
    {
        public MapSizeInfoCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null &&
                  ClipElement.GetClipRangeElement(m_Application.MapControl.ActiveView.GraphicsContainer) != null; ;
            }
        }

        public override void OnClick()
        {
            FrmMapSizeInfo frm = new FrmMapSizeInfo(m_Application);
            frm.Show();
            frm.Activate();

        }
    }
    
}

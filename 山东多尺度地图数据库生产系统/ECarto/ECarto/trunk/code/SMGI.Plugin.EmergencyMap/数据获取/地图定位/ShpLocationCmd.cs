using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.EmergencyMap
{
    public class ShpLocationCmd : SMGICommand
    {
        public ShpLocationCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null && m_Application.MapControl.Map.LayerCount > 0;
            }
        }

        public override void OnClick()
        {
            ShpLocationForm frm = new ShpLocationForm(m_Application);
            frm.Show();                 //非模式窗体
        }


    }
}

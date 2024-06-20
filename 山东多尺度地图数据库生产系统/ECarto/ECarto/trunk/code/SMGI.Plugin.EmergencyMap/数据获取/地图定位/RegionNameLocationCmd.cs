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
    public class RegionNameLocationCmd:SMGICommand
    {
        public RegionNameLocationCmd()
        {
            
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace==null && m_Application.MapControl.Map.LayerCount > 0;
            }
        }
        RegionNameLocationForm locationfrm = null;
        public override void OnClick()
        {
            if (locationfrm == null || locationfrm.IsDisposed)
            {
                locationfrm = new RegionNameLocationForm(m_Application);
                //非模式对话框
                locationfrm.Show();
            }
            else
            {
                locationfrm.WindowState = FormWindowState.Normal;
                locationfrm.Activate();
            }

        }
    }
}

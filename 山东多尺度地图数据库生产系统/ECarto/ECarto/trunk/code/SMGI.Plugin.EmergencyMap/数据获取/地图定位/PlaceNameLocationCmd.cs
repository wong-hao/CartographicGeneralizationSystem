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
    public class PlaceNameLocationCmd : SMGICommand
    {
        public PlaceNameLocationCmd()
        {
            
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace==null && m_Application.MapControl.Map.LayerCount > 0;
            }
        }
        PlaceNameLocationForm namefrm = null;
        public override void OnClick()
        {
            if (namefrm == null || namefrm.IsDisposed)
            {

                namefrm = new PlaceNameLocationForm(m_Application);
                //非模式对话框
                namefrm.Show();
            }
            else
            {
                namefrm.WindowState = FormWindowState.Normal;
                namefrm.Activate();
            } 

        }
    }
}

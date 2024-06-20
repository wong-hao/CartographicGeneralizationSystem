using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 行政区划定位（行政区划列表）
    /// </summary>
    public class AdministrativeRegeionLocationCmd : SMGICommand
    {
        private AdministrativeRegeionLocationForm _locationFrm = null;

        public AdministrativeRegeionLocationCmd()
        {
            
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace==null && m_Application.MapControl.Map.LayerCount > 0;
            }
        }

        public override void OnClick()
        {
            if (_locationFrm == null || _locationFrm.IsDisposed)
            {
                _locationFrm = new AdministrativeRegeionLocationForm(m_Application);
                _locationFrm.StartPosition = FormStartPosition.CenterParent;

                //非模式对话框
                _locationFrm.Show();
            }
            else
            {
                _locationFrm.WindowState = FormWindowState.Normal;
                _locationFrm.Activate();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using SMGI.Plugin.EmergencyMap.DataSource;
namespace SMGI.Plugin.EmergencyMap
{
    public class DEMDataSetCmd:SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null;
            }
        }
        public override void OnClick()
        {
            DEMData frm = new DEMData(m_Application);
            frm.ShowDialog();
        }
    }
}

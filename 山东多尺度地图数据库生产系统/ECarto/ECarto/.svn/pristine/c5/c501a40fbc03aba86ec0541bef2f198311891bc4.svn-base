using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using SMGI.Plugin.EmergencyMap.DataSource;

namespace SMGI.Plugin.EmergencyMap
{
    //系统配置修改
    public class MapSouceDataSetCmd : SMGICommand
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
            FrmMapServerSet frm = new FrmMapServerSet();
            frm.ShowDialog();
        }
    }
}

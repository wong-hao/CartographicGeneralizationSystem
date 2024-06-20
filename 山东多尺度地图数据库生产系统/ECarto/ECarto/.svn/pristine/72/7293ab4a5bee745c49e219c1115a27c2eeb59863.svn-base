using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.BaseFunction
{
    public class ReferenceScaleSetCmd : SMGI.Common.SMGICommand
    {

        public ReferenceScaleSetCmd()
        {
            m_caption = "设置参照比例尺";
            
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            IMap map = m_Application.Workspace.Map;
            ReferenceScaleDialog dlg = new ReferenceScaleDialog();
            dlg.Scale = map.ReferenceScale;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                map.ReferenceScale = dlg.Scale;
                (map as IActiveView).Refresh();
            }
        }

    }
}

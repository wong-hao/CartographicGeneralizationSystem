using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.CartoUI;

namespace SMGI.Plugin.BaseFunction
{
    public class LabelManagerCmd : SMGI.Common.SMGICommand
    {
        public LabelManagerCmd()
        {
            m_caption = "标注管理";
            m_toolTip = "标注管理";
            m_category = "标注";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null;
            }
        }

        public override void OnClick()
        {
            ILabelManagerDialog labelmanager = new LabelManagerDialog();
            labelmanager.DoModal(m_Application.MapControl.Map, m_Application.MapControl.hWnd);  
        }
    }
}

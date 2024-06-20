using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.EmergencyMap
{
    public class SetSpatialReferenceCommand : SMGICommand
    {
        public SetSpatialReferenceCommand()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace==null;
            }
        }

        public override void OnClick()
        {
            SetSpatialReferenceForm frm = new SetSpatialReferenceForm(m_Application);
            if (DialogResult.OK == frm.ShowDialog())
            {
                ISpatialReference sr = frm.targetSpatialReference;               
                //更新裁切框

            }
        }

    }
}

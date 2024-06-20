using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.BaseFunction
{
    //数据投影
    public class GDBProjectCmd : SMGI.Common.SMGICommand
    {
        public GDBProjectCmd()
        {
            m_category = "数据投影";
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
            GDBProjectForm frm = new GDBProjectForm(m_Application);
            frm.ShowDialog();
        }

    }
}

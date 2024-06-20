using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.EmergencyMap
{
    public class FrmMapGridCmd: SMGICommand
    {
        public FrmMapGridCmd()
        {
            m_caption = "格网线生成";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null&&m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            FrmGridLine frm = new FrmGridLine();
            frm.ShowDialog();
            GC.Collect();
        }

      
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace SMGI.Plugin.BaseFunction
{
    public class DataClearCmd : SMGICommand
    {
        public DataClearCmd()
        {
            m_caption = "数据清理";
            m_toolTip = "清理数据库";
            m_category = "预处理";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing;
            }
        }
       
        public override void OnClick()
        {
            var dc = m_Application.Workspace.EsriWorkspace as IDatabaseCompact;
            if (dc != null && dc.CanCompact())
            {
                dc.Compact();
                MessageBox.Show("清理完成！", "提示", MessageBoxButtons.OK);
            }
        }
    }
}
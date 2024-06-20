 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.MapGeneralization
{
    /// <summary>
    /// 拓扑创建
    /// </summary>
    public class TopolgyCreateCmd : SMGICommand
    {
        public TopolgyCreateCmd()
        {
            m_caption = "拓扑创建";
            m_toolTip = "拓扑创建";
            m_category = "拓扑";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null&&m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing;

            }
        }
        public override void OnClick()
        {
            IMap pMap = m_Application.ActiveView as IMap;
            FrmTopolgy dlg = new FrmTopolgy(pMap);
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
           
        }
    }
}


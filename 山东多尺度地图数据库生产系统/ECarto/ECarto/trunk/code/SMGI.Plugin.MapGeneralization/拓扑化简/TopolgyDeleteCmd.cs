 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
namespace SMGI.Plugin.MapGeneralization
{
    /// <summary>
    /// 拓扑删除
    /// </summary>
    public class TopolgyDeleteCmd : SMGICommand
    {
        
        public TopolgyDeleteCmd()
        {
            m_caption = "拓扑删除";
            m_toolTip = "拓扑删除";
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
            TopologyHelper helper = new TopologyHelper(m_Application.ActiveView);
            WaitOperation wo = m_Application.SetBusy();
            try
            {

                wo.SetText("正在删除拓扑...");
                helper.TopologyDelete();
                wo.Dispose();
                TopologyApplication.Topology = null;
                MessageBox.Show("删除完成!");
                 
            }
            catch
            {
                TopologyApplication.Topology = null;
                wo.Dispose();
            }
           
        }
    }
}


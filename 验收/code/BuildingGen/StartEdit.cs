using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingGen
{
    public class StartEdit:BaseGenCommand
    {
        public StartEdit()
        {
            base.m_category = "GSystem";
            base.m_caption = "开始编辑";
            base.m_message = "开始对地图的编辑";
            base.m_toolTip = "开始对地图的编辑";
            base.m_name = "StartEdit";
            base.m_bitmap = base.m_bitmap = System.Drawing.Bitmap.FromHbitmap((IntPtr)(new ESRI.ArcGIS.Controls.ControlsEditingStartCommandClass()).Bitmap);
        }
        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null) && (m_application.EngineEditor.EditState != ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing);
            }
        }
        public override void OnClick()
        {
            m_application.StartEdit();
        }
    }
}

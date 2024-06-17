using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingGen
{
    public class SaveEdit:BaseGenCommand
    {
        public SaveEdit()
        {
            base.m_category = "GSystem";
            base.m_caption = "保存编辑";
            base.m_message = "保存对地图的编辑";
            base.m_toolTip = "保存对地图的编辑";
            base.m_name = "SaveEdit";
            base.m_bitmap = System.Drawing.Bitmap.FromHbitmap((IntPtr)(new ESRI.ArcGIS.Controls.ControlsEditingSaveCommand()).Bitmap);
        }
        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null) && (m_application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing);
            }
        }
        public override void OnClick()
        {
            m_application.SaveEdit();
        }

    }
}

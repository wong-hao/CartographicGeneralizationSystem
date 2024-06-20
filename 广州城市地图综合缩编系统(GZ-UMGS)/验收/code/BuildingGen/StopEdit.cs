using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
namespace BuildingGen
{
    public class StopEdit:BaseGenCommand
    {
        public StopEdit()
        {
            base.m_category = "GSystem";
            base.m_caption = "停止编辑";
            base.m_message = "停止对地图的编辑";
            base.m_toolTip = "停止对地图的编辑";
            base.m_name = "StopEdit";
            base.m_bitmap = System.Drawing.Bitmap.FromHbitmap((IntPtr)(new ESRI.ArcGIS.Controls.ControlsEditingStopCommand()).Bitmap);
        }
        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null) 
                    && (m_application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing);
            }
        }
        public override void OnClick()
        {
            if (m_application.EngineEditor.HasEdits())
            {
                DialogResult r = MessageBox.Show("地图数据已经被编辑，是否保存？", "提示", MessageBoxButtons.YesNoCancel);
                if (r == DialogResult.Yes)
                {
                    m_application.StopEdit(true);
                }
                else if (r == DialogResult.No)
                {
                    m_application.StopEdit(false);
                }
                else
                {
                }
            }
            else
            {
                m_application.StopEdit(true);
            }
        }

    }
}

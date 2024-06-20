using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMGI.Plugin.BaseFunction
{
    public class WorkspaceSaveCmd : SMGI.Common.SMGICommand
    {
        public WorkspaceSaveCmd()
        {
            m_caption = "保存工程";
            m_toolTip = "保存当前工程";
            m_category = "工程";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }
        public override void OnClick()
        {
            if (m_Application.Workspace == null)
            {
                System.Windows.Forms.MessageBox.Show("未打开地图工程");
                return;
            }
            if (m_Application.EngineEditor.EditState != ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateNotEditing)
            {
                var r = System.Windows.Forms.MessageBox.Show("正在编辑状态，需要关闭编辑，是否保存编辑？","正在编辑", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (r == System.Windows.Forms.DialogResult.Yes)
                {
                    m_Application.EngineEditor.StopEditing(true);
                }
                else if (r == System.Windows.Forms.DialogResult.No)
                {
                    m_Application.EngineEditor.StopEditing(false);
                }
                else
                {
                    return;
                }
            }
            using(var wo = m_Application.SetBusy())
            {
                wo.SetText("正在保存...");
                System.Threading.Thread.Sleep(500);
                m_Application.Workspace.Save();
            }
        }
    }
}

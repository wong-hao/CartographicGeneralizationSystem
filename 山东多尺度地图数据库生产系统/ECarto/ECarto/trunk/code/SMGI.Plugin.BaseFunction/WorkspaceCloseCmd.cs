using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMGI.Plugin.BaseFunction
{
    public class WorkspaceCloseCmd : SMGI.Common.SMGICommand
    {
        public WorkspaceCloseCmd()
        {
            m_caption = "关闭工程";
            m_toolTip = "关闭当前工程";
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

            if (m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
            {
                System.Windows.Forms.DialogResult r = System.Windows.Forms.MessageBox.Show("正在开启编辑，是否保存编辑？", "提示", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (r == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    m_Application.EngineEditor.StopEditing (r == System.Windows.Forms.DialogResult.Yes);
                }
            }

            System.Windows.Forms.DialogResult r1 = System.Windows.Forms.MessageBox.Show("工作区尚未保存，是否保存？", "提示", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
            if (r1 == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            else
            {
                if (r1 == System.Windows.Forms.DialogResult.Yes)
                    m_Application.Workspace.Save();
            }
            m_Application.CloseWorkspace();
        }
    }
}

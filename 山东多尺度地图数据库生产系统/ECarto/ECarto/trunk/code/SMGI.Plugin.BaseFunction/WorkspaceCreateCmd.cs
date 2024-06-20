using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SMGI.Common;
using System.Windows.Forms;

namespace SMGI.Plugin.BaseFunction
{
    public class WorkspaceCreateCmd:SMGI.Common.SMGICommand
    {
        public WorkspaceCreateCmd() {
            m_caption = "创建工程";
            m_toolTip = "创建一个空的工程";
            m_category = "工程";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null;
            }
        }
        public override void OnClick()
        {
            if (m_Application.Workspace != null)
            {
                System.Windows.Forms.MessageBox.Show("已经打开工作区，请先关闭工作区!");
                return;
            }

            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在创建工作区");
                m_Application.CreateWorkspace(dlg.FileName);
            }           
        }
    }



}

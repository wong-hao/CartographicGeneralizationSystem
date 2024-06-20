using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.BaseFunction
{
    class ProjectWorkspaceCmd:SMGI.Common.SMGICommand
    {
        public ProjectWorkspaceCmd()
        {
            m_caption = "打开工程";
            m_toolTip = "打开一个已有的工程";
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
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择工程文件夹";
            fbd.ShowNewFolderButton = false;
            
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在打开工作区");
                if (GWorkspace.IsWorkspace(fbd.SelectedPath))
                {
                    m_Application.OpenWorkspace(fbd.SelectedPath);
                }
                else { 
                    
                }
            }           
        }
    }
}

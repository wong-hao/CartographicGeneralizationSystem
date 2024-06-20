using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.BaseFunction
{
    public class WorkspaceLastOpenCmd : SMGI.Common.SMGICommand
    {
        string ConfigKey()
        {
            return this.GetType().FullName + ".WorkspacePath";
        }
        public WorkspaceLastOpenCmd()
        {
            m_caption = "最近工程";
            m_toolTip = "打开最近打开过的工程";
            m_category = "工程";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null && m_Application.AppConfig[ConfigKey()] != null;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            m_Application.WorkspaceOpened += new EventHandler(m_Application_WorkspaceOpened);
        }

        void m_Application_WorkspaceOpened(object sender, EventArgs e)
        {
            m_Application.AppConfig[ConfigKey()] = m_Application.Workspace.EsriWorkspace.PathName;
        }
        public override void OnClick()
        {
            if (m_Application.Workspace != null)
            {
                System.Windows.Forms.MessageBox.Show("已经打开工作区，请先关闭工作区!");
                return;
            }

            object path = m_Application.AppConfig[ConfigKey()];
            if(path == null)
            {
                MessageBox.Show("最近地图工程路径错误。");
                return;
            }
            try
            {
                if (path.ToString().ToLower().EndsWith(".gdb"))
                {
                    if (!GApplication.GDBFactory.IsWorkspace(path.ToString()))
                    {
                        MessageBox.Show("无效的最近工程文件路径！");
                        return;
                    }
                }
                else if (path.ToString().ToLower().EndsWith(".mdb"))
                {
                    if (!GApplication.MDBFactory.IsWorkspace(path.ToString()))
                    {
                        MessageBox.Show("无效的最近工程文件路径！");
                        return;
                    }
                }


                using (m_Application.SetBusy())
                {
                    m_Application.OpenWorkspace(path.ToString());
                    
                    //加载粘滞容差 zhx@2022.4.12
                    int tol = AppConfig.StickyMoveTolerance;
                    if (tol > 0)
                    {
                        IEngineEditProperties2 _editorProp;
                        _editorProp = GApplication.Application.EngineEditor as IEngineEditProperties2;
                        _editorProp.StickyMoveTolerance = tol;
                    }
                }
            }
            catch 
            {
                MessageBox.Show(string.Format("打开工程失败，请检查【{0}】是否为有效地图工程。",path));
            }
        }
        
    }

}

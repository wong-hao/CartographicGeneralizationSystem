using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SystemUI;

namespace BuildingGen
{
    /// <summary>
    /// Summary description for CreateNewDocument.
    /// </summary>
    internal class OpenDoc : BaseGenCommand
    {
        private IHookHelper m_hookHelper = null;

        public OpenDoc()
        {
            base.m_category = "GDocument";
            base.m_caption = "打开工作区";
            base.m_message = "打开一个新的工作区";
            base.m_toolTip = "打开一个新的工作区";
            base.m_name = "OpenDoc";
        }

        public override void OnCreate(object hook)
        {
            if (m_hookHelper == null)
                m_hookHelper = new HookHelperClass();

            m_hookHelper.Hook = hook;
        }
        public override void OnClick()
        {
            GApplication app = m_application;
            if (app.Workspace != null)
            {
                DialogResult r = MessageBox.Show("当前工作区已经打开，是否保存？","提示",MessageBoxButtons.YesNoCancel);
                if(r == DialogResult.Cancel)
                {
                    return;
                }
                else if (r == DialogResult.Yes)
                {
                    app.Workspace.Save();
                }
                else
                {
                    
                } 
            }

            OpenWSDlg2 dlg = new OpenWSDlg2();
            object path = app.AppConfig["最后工作区"];
            if (path != null && path.ToString() != string.Empty) {
                dlg.CurrentDirectory = path.ToString(); ;
            }
            if (dlg.ShowDialog() != DialogResult.OK) {
                return;
            }
            app.OpenWorkspace(dlg.LayerPath);
            app.AppConfig["最后工作区"] = dlg.CurrentDirectory;

        }
    }

    internal class SaveDoc : BaseGenCommand {

        public SaveDoc() {
            base.m_category = "GDocument";
            base.m_caption = "保存工作区";
            base.m_message = "保存当前工作区";
            base.m_toolTip = "保存当前工作区";
            base.m_name = "SaveDoc";
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }

        public override void OnClick() {
            m_application.Workspace.Save();
        }
    }

    internal class CloseDoc : BaseGenCommand {

        public CloseDoc() {
            base.m_category = "GDocument";
            base.m_caption = "关闭工作区";
            base.m_message = "关闭当前工作区";
            base.m_toolTip = "关闭当前工作区";
            base.m_name = "CloseDoc";
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }

        public override void OnClick() {
            m_application.CloseWorkspace();
        }
    }

}

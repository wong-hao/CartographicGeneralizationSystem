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
            base.m_caption = "�򿪹�����";
            base.m_message = "��һ���µĹ�����";
            base.m_toolTip = "��һ���µĹ�����";
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
                DialogResult r = MessageBox.Show("��ǰ�������Ѿ��򿪣��Ƿ񱣴棿","��ʾ",MessageBoxButtons.YesNoCancel);
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
            object path = app.AppConfig["�������"];
            if (path != null && path.ToString() != string.Empty) {
                dlg.CurrentDirectory = path.ToString(); ;
            }
            if (dlg.ShowDialog() != DialogResult.OK) {
                return;
            }
            app.OpenWorkspace(dlg.LayerPath);
            app.AppConfig["�������"] = dlg.CurrentDirectory;

        }
    }

    internal class SaveDoc : BaseGenCommand {

        public SaveDoc() {
            base.m_category = "GDocument";
            base.m_caption = "���湤����";
            base.m_message = "���浱ǰ������";
            base.m_toolTip = "���浱ǰ������";
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
            base.m_caption = "�رչ�����";
            base.m_message = "�رյ�ǰ������";
            base.m_toolTip = "�رյ�ǰ������";
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

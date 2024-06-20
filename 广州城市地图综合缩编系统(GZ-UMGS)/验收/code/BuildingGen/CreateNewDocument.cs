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
    internal class CreateNewDocument : BaseGenCommand
    {
        private IHookHelper m_hookHelper = null;

        //constructor
        public CreateNewDocument()
        {
            //update the base properties
            base.m_category = "GDocument";
            base.m_caption = "����������";
            base.m_message = "����һ���µĹ�����";
            base.m_toolTip = "����һ���µĹ�����";
            base.m_name = "CreateNewDocument";
        }

        #region Overriden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (m_hookHelper == null)
                m_hookHelper = new HookHelperClass();

            m_hookHelper.Hook = hook;
        }

        
        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
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
            app.CloseWorkspace();
            CreateWorkspaceDlg dlg = new CreateWorkspaceDlg();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            WaitOperation wo = app.SetBusy(true);
            app.CreateWorkspace(dlg.WorkspacePath);
            app.Workspace.MapConfig["OrgScale"] = dlg.OrgScale;
            app.Workspace.MapConfig["GenScale"] = dlg.GenScale;

            
            CreateLayerInfo info = dlg.Water;
            wo.SetText("���ڵ���ˮϵ");
            //if (info != null)
            //    app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            info = dlg.Water;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);
            wo.Step(13);
            wo.SetText("���ڵ���ֲ��");
            info = dlg.Plant;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);
            wo.Step(13);
            wo.SetText("���ڵ��������");
            info = dlg.Forbid;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            wo.Step(13);
            wo.SetText("���ڵ����·��");
            info = dlg.RoadA;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            wo.Step(13);
            wo.SetText("���ڵ��뽨����");
            info = dlg.Building;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            wo.Step(13);
            wo.SetText("���ڵ��빤����");
            info = dlg.Factory;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            wo.Step(13);
            wo.SetText("���ڵ���BRTͼ��");
            info = dlg.BRT;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            wo.Step(13);
            wo.SetText("���ڵ����̻���");
            info = dlg.Island;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            wo.Step(13);
            wo.SetText("���ڵ���߼���");
            info = dlg.RoadHA;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            wo.Step(13);
            wo.SetText("���ڵ����·������");
            info = dlg.RoadL;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            wo.Step(13);
            wo.SetText("���ڵ���߼�������");
            info = dlg.RoadHL;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);

            wo.Step(13);
            wo.SetText("���ڵ�����·��");
            info = dlg.Railway;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);
            
            wo.Step(13);
            wo.SetText("���ڵ���POI����");
            info = dlg.POI;
            if (info != null)
                app.Workspace.LayerManager.AddLayer(info.WorkspacePath, info.FeatureName, info.Name, info.Type);
            
            wo.Step(13);
            app.SetBusy(false);

            //app.workspace = GWorkspace.CreateWorkspace(Path.GetDirectoryName(sfd.FileName), Path.GetFileName(sfd.FileName));

        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.GeneralEdit
{
    public class StartEditingCommand : SMGI.Common.SMGICommand
    {
        public StartEditingCommand()
        {
            m_caption = "开始编辑";
            m_category = "基础编辑";
        }

        public override bool Enabled
        {
            get
            {
                return (m_Application.Workspace != null 
                    && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing);
            }
        }

        public override void OnClick()
        {
            if (m_Application.Workspace == null)
            {
                MessageBox.Show("请先加载工作区！");
                return;
            }
            if (m_Application.Workspace.LayerManager.Map.LayerCount < 1)
            {
                MessageBox.Show("当前编辑图层为空！");
                return;
            }
            if (m_Application.EngineEditor != null)
            {
                IEngineEditor tempEditor = m_Application.EngineEditor;
                try
                {
                    if (tempEditor.EditState == esriEngineEditState.esriEngineStateNotEditing)
                    {
                        tempEditor.StartEditing(m_Application.Workspace.EsriWorkspace, m_Application.Workspace.Map);
                        tempEditor.EnableUndoRedo(true);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("开启编辑出现异常：" + ex.Message);
                }
            }
        }
    }

    public class SaveEditingCommand : SMGI.Common.SMGICommand
    {
        public SaveEditingCommand()
        {
            m_caption = "保存编辑";
            m_category = "基础编辑";

        }

        public override bool Enabled
        {
            get
            {
                return (m_Application.Workspace != null
                    && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing);
            }
        }

        public override void OnClick()
        {
            if (m_Application == null)
                return;

            ITool curTool = null;
            if (m_Application.LayoutState == LayoutState.MapControl)
                curTool = m_Application.MapControl.CurrentTool;
            else
                curTool = m_Application.PageLayoutControl.CurrentTool;

            m_Application.SaveEdit();

            if (curTool != null)
            {
                if (m_Application.LayoutState == LayoutState.MapControl)
                    m_Application.MapControl.CurrentTool = curTool;
                else
                    m_Application.PageLayoutControl.CurrentTool = curTool;
            }
        }
    }
    
    public class StopEditingCommand : SMGI.Common.SMGICommand
    {
        public StopEditingCommand()
        {
            m_caption = "停止编辑";
            m_category = "基础编辑";
        }

        public override bool Enabled
        {
            get
            {
                return (m_Application.Workspace != null
                    && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing);
            }
        }

        public override void OnClick()
        {
            try
            {
                if (m_Application == null)
                    return;

                if (m_Application.EngineEditor == null)
                    return;

                if (m_Application.EngineEditor.HasEdits())
                {
                    if (MessageBox.Show("是否保存现有编辑？", "是否保存", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        m_Application.StopEdit(true);
                    }
                    else
                    {
                        m_Application.StopEdit(false);
                    }
                }
                else
                {
                    m_Application.StopEdit(false);
                }

                if (m_Application.LayoutState == LayoutState.MapControl)
                {
                    if (!(m_Application.MapControl.CurrentTool as ICommand).Enabled)
                    {
                        m_Application.MapControl.CurrentTool = null;
                    }
                }
                else
                {
                    if (!(m_Application.PageLayoutControl.CurrentTool as ICommand).Enabled)
                    {
                        m_Application.PageLayoutControl.CurrentTool = null;
                    }
                }

            }
            catch
            {
            }
        }
    }

    public class UndoCommand : SMGI.Common.SMGICommand
    {
        ICommand cmd;
        public UndoCommand()
        {
            m_caption = "撤销";
            m_category = "编辑管理";
            m_message = "撤销";           
        }
        public override int Bitmap
        {
            get
            {
                return cmd.Bitmap;
            }
        }
        public override bool Enabled
        {
            get {
                return cmd.Enabled;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            var cp = app.MainForm.CommandPool as ICommandPoolEdit;
            UID uID = new UIDClass();
            uID.Value = "{380FB31E-6C24-4F5C-B1DF-47F33586B885}";
            IArray ar = cp.AddUID(uID);
            cmd = ar.Element[0] as ICommand;
        }
        public override void OnClick()
        {
            //(m_Application.EngineEditor.EditWorkspace as IWorkspaceEdit).UndoEditOperation();
            cmd.OnClick();
        }
    }

    public class RedoCommand : SMGI.Common.SMGICommand
    {
        ICommand cmd;
        public RedoCommand()
        {
            m_caption = "重做";
            m_category = "编辑管理";
            m_message = "重做";
            
        }
        public override int Bitmap
        {
            get
            {
                return cmd.Bitmap;
            }
        }
        public override bool Enabled
        {
            get
            {
                return cmd.Enabled;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            var cp = app.MainForm.CommandPool as ICommandPoolEdit;
            UID uID = new UIDClass();
            uID.Value = "{B0675372-0271-4680-9A2C-269B3F0C01E8}";
            IArray ar = cp.AddUID(uID);
            cmd  = ar.Element[0] as ICommand;
        }
        public override void OnClick()
        {
            cmd.OnClick();
        }
    }

    public class CopyCommand : SMGI.Common.SMGICommand
    {
        ICommand cmd;
        public CopyCommand()
        {
            m_caption = "复制";
            m_category = "复制选中目标";
            m_message = "复制";

        }
        public override int Bitmap
        {
            get
            {
                return cmd.Bitmap;
            }
        }
        public override bool Enabled
        {
            get
            {
                return cmd.Enabled;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            var cp = app.MainForm.CommandPool as ICommandPoolEdit;
            UID uID = new UIDClass();
            uID.Value = "{2D7BD886-7531-4198-820D-551A5A14569E}";
            IArray ar = cp.AddUID(uID);
            cmd = ar.Element[0] as ICommand;

        }
        public override void OnClick()
        {
            cmd.OnClick();
        }
    }
    public class CutCommand : SMGI.Common.SMGICommand
    {
        ICommand cmd;
        public CutCommand()
        {
            m_caption = "剪切";
            m_category = "剪切选中目标";
            m_message = "剪切";

        }
        public override int Bitmap
        {
            get
            {
                return cmd.Bitmap;
            }
        }
        public override bool Enabled
        {
            get
            {
                return cmd.Enabled;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            var cp = app.MainForm.CommandPool as ICommandPoolEdit;
            UID uID = new UIDClass();
            uID.Value = "{C03E7512-CA4E-4197-8386-57830425D13A}";
            IArray ar = cp.AddUID(uID);
            cmd = ar.Element[0] as ICommand;

        }
        public override void OnClick()
        {
            cmd.OnClick();
        }
    }
    public class PasteCommand : SMGI.Common.SMGICommand
    {
        ICommand cmd;
        public PasteCommand()
        {
            m_caption = "粘贴";
            m_category = "粘贴剪切板中的目标";
            m_message = "粘贴";

        }
        public override int Bitmap
        {
            get
            {
                return cmd.Bitmap;
            }
        }
        public override bool Enabled
        {
            //get
            //{
            //    if (m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing)
            //        return false;
            //    bool canUndo = false;
            //    (m_Application.EngineEditor.EditWorkspace as IWorkspaceEdit).HasRedos(ref canUndo);
            //    return canUndo;
            //}
            get
            {
                return cmd.Enabled;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            var cp = app.MainForm.CommandPool as ICommandPoolEdit;
            UID uID = new UIDClass();
            uID.Value = "{99FADFFD-A788-4F1F-A1B3-04909B06735A}";
            IArray ar = cp.AddUID(uID);
            cmd = ar.Element[0] as ICommand;

        }
        public override void OnClick()
        {
            //(m_Application.EngineEditor.EditWorkspace as IWorkspaceEdit).RedoEditOperation();
            cmd.OnClick();
        }
    }

    public class DeleteCommand : SMGI.Common.SMGICommand
    {
        ICommand cmd;
        public DeleteCommand()
        {
            m_caption = "删除";
            m_category = "删除选中的目标";
            m_message = "删除选中的目标";

        }
        public override int Bitmap
        {
            get
            {
                return cmd.Bitmap;
            }
        }
        public override bool Enabled
        {
            //get
            //{
            //    if (m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing)
            //        return false;
            //    bool canUndo = false;
            //    (m_Application.EngineEditor.EditWorkspace as IWorkspaceEdit).HasRedos(ref canUndo);
            //    return canUndo;
            //}
            get
            {
                return cmd.Enabled;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            var cp = app.MainForm.CommandPool as ICommandPoolEdit;
            UID uID = new UIDClass();
            uID.Value = "{C4CB4830-8C2E-49AE-9D12-73E822BCA90E}";
            IArray ar = cp.AddUID(uID);
            cmd = ar.Element[0] as ICommand;

        }
        public override void OnClick()
        {
            //(m_Application.EngineEditor.EditWorkspace as IWorkspaceEdit).RedoEditOperation();
            cmd.OnClick();
        }
    }

    public class EnableSnappingCommand : SMGI.Common.SMGICommand
    {
        ISnappingEnvironment snapEnv;
        public EnableSnappingCommand()
        {
            m_caption = "使用捕捉";
            m_category = "捕捉";
            m_message = "启用或禁止地图的捕捉";

        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.MapControl != null;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            IHookHelper hookHelper = new HookHelperClass();
            hookHelper.Hook = app.MapControl.Object;

            IExtensionManager extensionManager = (hookHelper as IHookHelper2).ExtensionManager;
            if (extensionManager != null)
            {
                UID guid = new UIDClass();
                guid.Value = "{E07B4C52-C894-4558-B8D4-D4050018D1DA}"; //Snapping extension.
                IExtension extension = extensionManager.FindExtension(guid);
                snapEnv = extension as ISnappingEnvironment;
            }

        }
        public override void OnClick()
        {
            if (snapEnv != null)
            {
                snapEnv.Enabled = !snapEnv.Enabled;
            }

        }
    }

}

using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingGen
{
    public class EditLayer:BaseGenCommand,ESRI.ArcGIS.SystemUI.IToolControl
    {
        private System.Windows.Forms.ComboBox cb;
        EventHandler layerChanged;
        public EditLayer()
        {
            m_category = "GSystem";
            cb = new System.Windows.Forms.ComboBox();
            cb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            layerChanged = new EventHandler(LayerManager_LayerChanged);
        }

        void cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_application.Workspace != null)
            {
                m_application.Workspace.EditLayer = cb.SelectedItem as GLayerInfo;
            }
        }

        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null;
                    //&& m_application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnGenCreate(GApplication app)
        {
            this.m_application = app;
            app.WorkspaceOpened += new EventHandler(app_WorkspaceOpened);
            app.WorkspaceClosed += new EventHandler(app_WorkspaceClosed);
            cb.SelectedIndexChanged += new EventHandler(cb_SelectedIndexChanged);
            GetList();
        }

        void app_WorkspaceClosed(object sender, EventArgs e)
        {
            cb.Items.Clear();
        }

        void app_WorkspaceOpened(object sender, EventArgs e)
        {
            m_application.Workspace.LayerManager.LayerChanged += layerChanged;
            GetList();
        }

        void LayerManager_LayerChanged(object sender, EventArgs e)
        {
            GetList();
        }

        private void GetList()
        {
            cb.Items.Clear();
            if (m_application.Workspace == null)
            {
                return;
            }
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers)
            {
                cb.Items.Add(info);
            }
            
            object layer = m_application.Workspace.EditLayer;
            if (layer != null && cb.Items.Contains(layer))
            {
                cb.SelectedItem = layer;
            }
            else if (cb.Items.Count > 0)
            {
                cb.SelectedIndex = 0;
            }            
        }
        #region IToolControl 成员

        public bool OnDrop(ESRI.ArcGIS.SystemUI.esriCmdBarType barType)
        {
            return barType == ESRI.ArcGIS.SystemUI.esriCmdBarType.esriCmdBarTypeToolbar;
        }

        public void OnFocus(ESRI.ArcGIS.SystemUI.ICompletionNotify complete)
        {
            //throw new NotImplementedException();
        }

        public int hWnd
        {
            get { return cb.Handle.ToInt32(); }
        }

        #endregion
    }
}

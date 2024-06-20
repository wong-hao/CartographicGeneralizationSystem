using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;

namespace SMGI.Plugin.BaseFunction
{
    public class SnapEditorSetCmd : SMGI.Common.SMGICommand
    {
        public SnapEditorSetCmd()
        {
            m_caption = "捕捉设置";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            SnapEditorSetForm ed = new SnapEditorSetForm(m_Application);
            ed.ShowDialog(m_Application.MainForm as IWin32Window);
        }
    }
}

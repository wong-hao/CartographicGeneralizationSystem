using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;

namespace SMGI.Plugin.AnnotationEngine
{
    public class AnnoBreakFaceToNorthCommand : SMGICommand
    {
        public AnnoBreakFaceToNorthCommand()
        {
            m_caption = "字头朝上";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                m_Application.EngineEditor.StartOperation();

                AnnoBreakFaceToNorth anno2North = new AnnoBreakFaceToNorth(m_Application, "ANNOTATION_SM");
                err = anno2North.toNorth(-m_Application.ActiveView.ScreenDisplay.DisplayTransformation.Rotation, wo);

                m_Application.EngineEditor.StopOperation("河流注记朝北处理");

                m_Application.MapControl.Refresh();
            }

            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err);
            }
            
        }
    }
}

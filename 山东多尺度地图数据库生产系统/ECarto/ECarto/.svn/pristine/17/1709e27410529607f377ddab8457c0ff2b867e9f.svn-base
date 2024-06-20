using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.EmergencyMap
{
    public class AnnoHorToVerCmd : SMGICommand
    {
        public AnnoHorToVerCmd()
        {
            m_caption = "注记横竖排转换";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing && (m_Application.MapControl.Map.FeatureSelection as IEnumFeature).Next() != null;
            }
        }

        public override void OnClick()
        {
            string err = "";
            using (var wo = m_Application.SetBusy())
            {
                m_Application.EngineEditor.StartOperation();
                AnnoHorVerConvert annohorverconvert = new AnnoHorVerConvert(m_Application);
               
                    annohorverconvert.ConvertHorVer(wo);
                
                m_Application.EngineEditor.StopOperation("注记横竖排转换");

                m_Application.MapControl.Refresh();
            }

            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err);
            }
            
        }
    }
}

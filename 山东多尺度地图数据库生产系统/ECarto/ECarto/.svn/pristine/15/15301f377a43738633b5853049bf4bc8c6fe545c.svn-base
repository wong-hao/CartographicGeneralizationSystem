using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using ESRI.ArcGIS.SystemUI;

namespace SMGI.Plugin.GeneralEdit
{
    public class EditOptionCmd : SMGI.Common.SMGICommand
    {
        public EditOptionCmd()
        {
            m_caption = "±à¼­Ñ¡Ïî";
            m_category = "»ù´¡±à¼­";
        }

        public override void OnClick()
        {
            var frm = new EditOptionForm();
            frm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            frm.ShowDialog();
        }

        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null ;
            }
        }
    }
}

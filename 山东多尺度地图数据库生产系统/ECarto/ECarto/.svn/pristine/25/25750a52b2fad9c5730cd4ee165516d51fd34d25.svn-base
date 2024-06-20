using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using DevExpress.XtraBars.Docking;

namespace SMGI.Plugin.CollaborativeWork
{
    public class ShowConflictResultCommand : SMGICommand
    {

        public ShowConflictResultCommand()
        {
            
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && 
                    m_Application.Workspace != null && 
                    CollaborativeTask.Instance.DetectState == CollaborativeTask.DetectionState.DETECTED;
            }
        }

        public override void OnClick()
        {
            ConflictResultTable.Instance.upateTable();

            ConflictResultTable.Instance.show();
        }

        
    }
}

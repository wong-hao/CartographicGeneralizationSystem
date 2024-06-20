using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;

namespace SMGI.Plugin.EmergencyMap
{
    public class ShowDataSearchViewCmd : SMGICommand
    {

        public ShowDataSearchViewCmd()
        {
            
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && 
                    m_Application.Workspace != null;
            }
        }

        public override void OnClick()
        {
            DataSearchTable.Instance.Show();
        }

        
    }
}

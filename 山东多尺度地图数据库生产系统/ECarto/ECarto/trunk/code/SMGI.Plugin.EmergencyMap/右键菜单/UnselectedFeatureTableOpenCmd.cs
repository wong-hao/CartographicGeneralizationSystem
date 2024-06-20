using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;

namespace SMGI.Plugin.EmergencyMap
{
    public class UnselectedFeatureTableOpenCmd : SMGI.Common.SMGIContextMenu
    {
        private string _selStateFN = "selectstate";

        public UnselectedFeatureTableOpenCmd()
        {
            m_caption = "未选取要素";
        }
        public override bool Enabled
        {
            get
            {
                bool bFlag = false;
                if (this.CurrentContextItem is IFeatureLayer)
                {
                    IFeatureLayer l = this.CurrentContextItem as IFeatureLayer;
                    if (l.FeatureClass != null && l.FeatureClass.FindField(_selStateFN) != -1)
                    {
                        bFlag = true;
                    }

                }

                return bFlag;
            }
        }
        public override void OnClick()
        {
            ILayer layer = this.CurrentContextItem as ILayer;

            AttributeTableForm frm = new AttributeTableForm(m_Application, m_Application.MapControl.Map, layer, string.Format("{0} is not null", _selStateFN), _selStateFN);
            frm.Show(m_Application.MainForm as IWin32Window);
        }
    }
}

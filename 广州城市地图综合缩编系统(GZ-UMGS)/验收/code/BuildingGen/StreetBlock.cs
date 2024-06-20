using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingGen {
    public class StreetBlock :BaseGenCommand{
        public StreetBlock() {
            base.m_category = "GTool";
            base.m_caption = "构建街区";
            base.m_message = "根据道路及行政区构建街区";
            base.m_toolTip = "根据道路及行政区构建街区";
            base.m_name = "StreetBlock";
        }

        public override void OnClick() {
            StreetBlockConfigDlg dlg = new StreetBlockConfigDlg(m_application);
            dlg.ShowDialog();
        }
    }
}

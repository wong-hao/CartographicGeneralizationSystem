using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
namespace BuildingGen {
    public class ClearWorkspace:BaseGenCommand {
        public ClearWorkspace() {
            base.m_category = "GSystem";
            base.m_caption = "清理工作区";
            base.m_message = "清理工作区";
            base.m_toolTip = "清理工作区";
            base.m_name = "ClearWorkspace";
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }
        public override void OnClick() {
            IDataset ds = m_application.Workspace.Workspace as IDataset;
            IEnumDataset eds = ds.Subsets;
            IDataset cds = null;
            WaitOperation wo = m_application.SetBusy(true);
            while ((cds = eds.Next()) != null) {
                if (cds.Name.StartsWith("tp")) {
                    wo.SetText("正在清理[" + cds.Name + "]");
                    //System.Diagnostics.Debug.WriteLine(cds.Name);
                    if (cds.CanDelete())
                        cds.Delete();
                }
            }
            m_application.SetBusy(false);
        }
    }
}

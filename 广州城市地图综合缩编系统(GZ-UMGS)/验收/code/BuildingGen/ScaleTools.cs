using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingGen {
    internal class ScaleOrg : BaseGenCommand {

        public ScaleOrg() {
            base.m_category = "GScale";
            base.m_caption = "原始比例尺";
            base.m_message = "关闭当前工作区";
            base.m_toolTip = "关闭当前工作区";
            base.m_name = "CloseDoc";
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }

        public override void OnClick() {
            if (m_application.Workspace == null)
                return;
            object orgScale = m_application.Workspace.MapConfig["OrgScale"];
            if (orgScale == null) {
                orgScale = 2000.0;
                m_application.Workspace.MapConfig["OrgScale"] = orgScale;
            }
            if (m_application.Workspace.Map.MapUnits == ESRI.ArcGIS.esriSystem.esriUnits.esriUnknownUnits) {
                m_application.Workspace.Map.MapUnits = ESRI.ArcGIS.esriSystem.esriUnits.esriMeters;
            }
            m_application.MapControl.MapScale = Convert.ToDouble(orgScale);
            m_application.MapControl.Refresh();
        }
    }

    internal class ScaleGen : BaseGenCommand {

        public ScaleGen() {
            base.m_category = "GScale";
            base.m_caption = "综合比例尺";
            base.m_message = "关闭当前工作区";
            base.m_toolTip = "关闭当前工作区";
            base.m_name = "CloseDoc";
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }

        public override void OnClick() {
            if (m_application.Workspace == null)
                return;
            object orgScale = m_application.Workspace.MapConfig["GenScale"];
            if (orgScale == null) {
                orgScale = 10000.0;
                m_application.Workspace.MapConfig["GenScale"] = orgScale;
            }
            if (m_application.Workspace.Map.MapUnits == ESRI.ArcGIS.esriSystem.esriUnits.esriUnknownUnits) {
                m_application.Workspace.Map.MapUnits = ESRI.ArcGIS.esriSystem.esriUnits.esriMeters;
            }
            m_application.MapControl.MapScale = Convert.ToDouble(orgScale);
            m_application.MapControl.Refresh();
        }
    }

}

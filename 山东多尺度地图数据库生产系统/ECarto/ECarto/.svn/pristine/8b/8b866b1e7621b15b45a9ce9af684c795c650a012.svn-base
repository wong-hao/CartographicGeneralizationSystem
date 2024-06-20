using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;

namespace SMGI.Plugin.BaseFunction
{
    public class QuickViewCmd : SMGI.Common.SMGICommand
    {
        public static bool _isOn;
        public QuickViewCmd()
        {
            m_caption = "快速显示";
            m_toolTip = "切换快速显示";
            m_category = "环境";
            _isOn = false;
        }

        public override bool Enabled
        {
            get { return m_Application != null && m_Application.Workspace != null; }
        }

        public override void OnClick()
        {
            _isOn = !_isOn;
            var scale = 10000.0;
            if (m_Application.Workspace.Map.ReferenceScale != 0)
                scale = m_Application.Workspace.Map.ReferenceScale + 100;

            var lns = new List<string> {"TERL", "VEGL", "BOUL", "HYDL", "PIPL"};
            foreach (var ln in lns.Where(i => !string.IsNullOrEmpty(i.Trim())))
            {
                var layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == ln.ToUpper())).FirstOrDefault();
                if (layer == null) continue;
                layer.MinimumScale = _isOn ? scale : 0;
            }
            m_Application.ActiveView.Refresh();}
    }
}

 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;


namespace SMGI.Plugin.MapGeneralization
{
    public class TemporaryLayerAddCmd : SMGICommand
    {
        public TemporaryLayerAddCmd()
        {
            m_caption = "添加临时数据";
            m_toolTip = "向工作区添加临时数据";
            m_category = "数据";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null;

            }
        }
        public override void OnClick()
        {
            SelectDataDlg dlg = m_Application.SelectDataDialog;
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            ILayer[] layer = dlg.Layers;
            using (var wo = m_Application.SetBusy())
            {
                var lm = m_Application.Workspace.LayerManager;
                foreach (ILayer l in layer)
                {
                    wo.SetText(string.Format("正在导入图层【{0}】", l.Name));
                    l.Name = l.Name + "_Temp";
                    if (l is IFeatureLayer)
                    {
                        lm.Map.AddLayer(l);
                        lm.Map.MoveLayer(l, 0);
                    }
                    else if (l is IRasterLayer)
                    {
                        lm.Map.AddLayer(l);
                        lm.Map.MoveLayer(l, lm.Map.LayerCount - 1);
                    }
                    else if (l is ITopologyLayer)
                    {
                        lm.Map.AddLayer(l);
                        lm.Map.MoveLayer(l, lm.Map.LayerCount - 1);
                    }

                }
            }
        }
    }
}


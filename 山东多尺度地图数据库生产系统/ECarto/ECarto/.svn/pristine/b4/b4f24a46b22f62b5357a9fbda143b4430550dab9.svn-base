using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// Li:添加外部专题数据
    /// </summary>
    public class AddThematicDataCmd : SMGICommand
    {

        public AddThematicDataCmd()
        {
            m_caption = "添加专题数据";
            m_toolTip = "向工作区添加专题数据，并匹配符号";
        }
      public override bool Enabled
      {
          get
          {
              return m_Application != null && m_Application.Workspace != null;

          }
      }
      public override void OnClick()
      {
          AddThematicDataForm frm = new AddThematicDataForm();
          frm.StartPosition = FormStartPosition.CenterParent;
          if (frm.ShowDialog() != DialogResult.OK)
              return;

          IFeatureLayer layer = new FeatureLayerClass();
          layer.FeatureClass = frm.InFeatureClass;
          layer.Name = frm.InFeatureClass.AliasName;
          (layer as IGeoFeatureLayer).Renderer = frm.FeatureRenderer;

          m_Application.Workspace.LayerManager.Map.AddLayer(layer);
          m_Application.Workspace.LayerManager.Map.MoveLayer(layer, 0);

          m_Application.TOCControl.ActiveView.Refresh();
          m_Application.TOCControl.Update();
      }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using GENERALIZERLib;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;

namespace BuildingGen {
  class DeletePoint:BaseGenCommand {
    public DeletePoint() {
      base.m_category = "GOther";
      base.m_caption = "剔除重复点";
      base.m_message = "剔除重复点";
      base.m_toolTip = "剔除重复点";
      base.m_name = "DeletePoint";
    }

    public override bool Enabled {
      get {
        return true;
      }
    }

    public override void OnClick() {
      GLayerInfo info = null;
      foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers) {
        if (tempInfo.LayerType == GCityLayerType.POI
            && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint
            && tempInfo.OrgLayer != null
            ) {
          info = tempInfo;
          break;
        }
      }
      if (info == null) {
        System.Windows.Forms.MessageBox.Show("没有找到POI点数据，或者数据不为点状", "提示",
          System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
        return;
      }
    }
  }
}

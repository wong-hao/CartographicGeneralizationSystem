using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataManagementTools;

namespace BuildingGen {
  class RoadSplit:BaseGenCommand {
    public RoadSplit() {
      base.m_category = "GRoad";
      base.m_caption = "道路打散";
      base.m_message = "将道路在交点处打散处理";
      base.m_toolTip = "将道路在交点处打散处理";
      base.m_name = "RoadSplit";

    }
    public override bool Enabled {
      get {
        return m_application.Workspace != null;
      }
    }
    public override void OnClick() {
      GLayerInfo info = GetLayer();
      SplitLayer(info);

      m_application.MapControl.Refresh();
    }

    private void SplitLayer(GLayerInfo layer) {
      IFeatureLayer flayer = layer.Layer as IFeatureLayer;
      IFeatureClass fcOrg = flayer.FeatureClass;
      IFeatureWorkspace fworkspace = (m_application.Workspace.Workspace as IFeatureWorkspace);

      string tmpName = m_application.Workspace.LayerManager.TempLayerName();
      FeatureToLine ftl = new FeatureToLine();
      ftl.in_features = m_application.Workspace.Name + "\\" + layer.featureClassName;
      ftl.out_feature_class = m_application.Workspace.Name + "\\" + tmpName;
      ftl.cluster_tolerance = 0.01;

      object result = m_application.Geoprosessor.Execute(ftl, null);
#if DEBUG
      if (result == null) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < m_application.Geoprosessor.MessageCount; i++) {
          sb.AppendLine(m_application.Geoprosessor.GetMessage(i));
        }
        System.Windows.Forms.MessageBox.Show(sb.ToString());
        return;
      }
#endif
      //return;
      string name = (fcOrg as IDataset).Name;
      IFeatureClass fcNew = fworkspace.OpenFeatureClass(tmpName);
      flayer.FeatureClass = fcNew;
      (fcOrg as IDataset).Delete();
      (fcNew as IDataset).Rename(name);
      m_application.MapControl.Refresh();
    }

    private GLayerInfo GetLayer() {
      foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers) {
        if (info.LayerType == GCityLayerType.道路
            && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
            && info.OrgLayer != null
            ) {
          return info;
        }
      }
      return null;
    }

  }
}

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

namespace BuildingGen {
  /// <summary>
  /// 水系冲突处理
  /// 检查水系和其他要素的冲突：规则如下
  /// 绿地：水系与绿地产生重叠为冲突
  /// 当绿地不完全落在水系中时：切割绿地
  /// 当绿地完全落在水系中时：根据面积大小决定切割对象
  /// </summary>
  class HydroConflict :BaseGenCommand {
    public HydroConflict() {
      base.m_category = "GWater";
      base.m_caption = "冲突处理";
      base.m_message = "水系与其他要素冲突处理";
      base.m_toolTip = "水系与其他要素冲突处理";
      base.m_name = "HydroConflict";
    }
    public override bool Enabled {
      get {
        return m_application.Workspace != null;
      }
    }

    public override void OnClick() {
      GLayerInfo HydroLayer = GetHydroLayer(m_application.Workspace);
      if (HydroLayer == null) {
        return;
      }
      GLayerInfo VLayer = GetVegetationLayer(m_application.Workspace);
      if (VLayer == null) {
        return;
      }
      WaitOperation wo = m_application.SetBusy(true);
      double vArea = 2000;
      IFeatureLayer hflayer = HydroLayer.Layer as IFeatureLayer;
      IFeatureLayer vflayer = VLayer.Layer as IFeatureLayer;
      IFeatureClass hfc = hflayer.FeatureClass;
      IFeatureClass vfc = vflayer.FeatureClass;
      IFeatureCursor hfCursor = hfc.Search(null, true);
      ISpatialFilter sf = new SpatialFilterClass();
      sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      IFeature hfeature = null;
      int wCount = hfc.FeatureCount(null);
      while ((hfeature = hfCursor.NextFeature())!=null) {
        wo.SetText(string.Format("正在检查：{0}", hfeature.OID));
        wo.Step(wCount);
        if (vfc.FeatureCount(sf) == 0) {
          continue;
        }
        IRelationalOperator ro = hfeature.Shape as IRelationalOperator;
        sf.Geometry = hfeature.Shape;
        IFeatureCursor vfCursor = vfc.Update(sf, true);
        IFeature vfeature = null;
        while ((vfeature = vfCursor.NextFeature())!=null) {
          //水包含绿地
          if (ro.Contains(vfeature.Shape)) {
            if ((vfeature.Shape as IArea).Area > vArea) {
              continue;
            }
            vfCursor.DeleteFeature();
            continue;
          }
          else {
            IGeometry geo = (vfeature.Shape as ITopologicalOperator).Difference(hfeature.Shape);
            if ((geo as IArea).Area < vArea) {
              vfCursor.DeleteFeature();
            }
            else {
              vfeature.Shape = geo;
              vfCursor.UpdateFeature(vfeature);
            }
          }
        }
        vfCursor.Flush();
        System.Runtime.InteropServices.Marshal.ReleaseComObject(vfCursor);
      }

      wCount = vfc.FeatureCount(null);
      IFeatureCursor uvfCursor = vfc.Update(null, true);
      IFeatureCursor ivfCursor = vfc.Insert(true);
      IFeature updateFeatrue = null;
      wo.Step(0);
      while ((updateFeatrue = uvfCursor.NextFeature())!=null) {
        wo.SetText(string.Format("正在整理：{0}", updateFeatrue.OID));
        wo.Step(wCount);
        IGeometryCollection gc = (updateFeatrue.Shape as IPolygon4).ConnectedComponentBag as IGeometryCollection;
        if (gc.GeometryCount == 1)
          continue;
        for (int i = 0; i < gc.GeometryCount; i++) {
          IGeometry geo = gc.get_Geometry(i);
          updateFeatrue.Shape = geo;
          ivfCursor.InsertFeature(updateFeatrue as IFeatureBuffer);
        }
        uvfCursor.DeleteFeature();
      }
      uvfCursor.Flush();
      ivfCursor.Flush();
      System.Runtime.InteropServices.Marshal.ReleaseComObject(uvfCursor);
      System.Runtime.InteropServices.Marshal.ReleaseComObject(ivfCursor);
      m_application.SetBusy(false);
      m_application.MapControl.Refresh();
    }
    private GLayerInfo GetHydroLayer(GWorkspace workspace) {
      foreach (GLayerInfo info in workspace.LayerManager.Layers) {
        if (info.LayerType == GCityLayerType.水系
          //&& (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
            && info.OrgLayer != null
            ) {
          return info;
        }
      }
      return null;
    }
    private GLayerInfo GetVegetationLayer(GWorkspace workspace) {
      foreach (GLayerInfo info in workspace.LayerManager.Layers) {
        if (info.LayerType == GCityLayerType.植被
          //&& (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
            && info.OrgLayer != null
            ) {
          return info;
        }
      }
      return null;
    }
  }
}

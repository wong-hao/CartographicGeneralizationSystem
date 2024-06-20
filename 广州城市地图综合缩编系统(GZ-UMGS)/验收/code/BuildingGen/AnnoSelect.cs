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
  class AnnoSelect :BaseGenCommand {
    public AnnoSelect() {
      base.m_category = "GOther";
      base.m_caption = "注记选取";
      base.m_message = "注记选取";
      base.m_toolTip = "注记选取";
      base.m_name = "AnnoSelect";
      base.m_usedParas = new GenDefaultPara[] {          
        new GenDefaultPara("注记选取网格宽度",(double)15)//这个是网格大小（毫米）  
        ,new GenDefaultPara("注记选取网格高度",(double)6)//这个是网格大小 （毫米）
      };
    }
    public override bool Enabled {
      get {
        return m_application.Workspace !=null;
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
      if (info == null)
        return;
      WaitOperation wo = m_application.SetBusy(true);
      IFeatureLayer flayer = (info.Layer as IFeatureLayer);
      IFeatureClass fc = flayer.FeatureClass;
      IEnvelope env = m_application.MapControl.FullExtent;
      double xStep = (double)m_application.GenPara["注记选取网格宽度"];
      double yStep = (double)m_application.GenPara["注记选取网格高度"];
      xStep = (xStep / 1000) * (double)m_application.Workspace.MapConfig["GenScale"];
      yStep = (yStep / 1000) * (double)m_application.Workspace.MapConfig["GenScale"];
      ISpatialFilter sf = new SpatialFilterClass();
      sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      //sf.WhereClause = "code <> 1";
      int cIndex = fc.FindField("code");
      int wIndex = fc.FindField("weight");
      for (double x = env.XMin; x < env.XMax; x += xStep) {
        for (double y = env.YMin; y < env.YMax; y += yStep) {
          wo.SetText(string.Format("正在处理({0},{1})", Convert.ToInt32(x), Convert.ToInt32(y)));
          IEnvelope cEnv = new EnvelopeClass();
          cEnv.XMin = x;
          cEnv.YMin = y;
          cEnv.XMax = x + xStep;
          cEnv.YMax = y + yStep;
          sf.Geometry = cEnv;
          if (fc.FeatureCount(sf) < 2)
            continue;

          IFeatureCursor fCursor = fc.Search(sf, false);
          IFeature featrue = null;
          IFeature saveFeature = null;
          int weight = 0;
          bool hasSuperNode = false;
          while ((featrue = fCursor.NextFeature()) != null) {
            //1.是超级节点的情况
            if (Convert.ToInt32(featrue.get_Value(cIndex)) == 1) {
              if (hasSuperNode) {
                continue;
              }
              if (saveFeature != null) {
                saveFeature.Delete();
              }

              saveFeature = featrue;
              hasSuperNode = true;
              continue;
            }//if (Convert.ToInt32(fc.get_Value(cIndex)) == 1)

            //2.不是超级节点的情况
            // 2.1 出现过超级节点
            if (hasSuperNode) {
              featrue.Delete();
              continue;
            }//if (hasSuperNode)
            // 2.2 没有出现过超级节点
            int cWeight = Convert.ToInt32(featrue.get_Value(wIndex));
            if (saveFeature == null) {
              saveFeature = featrue;
              weight = cWeight;
              continue;
            }//if (saveFeature == null)
            if (cWeight > weight) {
              saveFeature.Delete();
              saveFeature = featrue;
              weight = cWeight;
            }//if
            else {
              featrue.Delete();
            }
          }//while
        }//fory
      }//forx
      m_application.SetBusy(false);
      m_application.MapControl.Refresh();
    }//onclicked
  }//class AnnoSelect
}//namespace

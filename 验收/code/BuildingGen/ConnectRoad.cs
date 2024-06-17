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
using System.Windows.Forms;
namespace BuildingGen {
  public class ConnectRoad : BaseGenCommand {
    public ConnectRoad() {
      base.m_category = "GRoad";
      base.m_caption = "同名道路连接";
      base.m_message = "同名道路连接";
      base.m_toolTip = "同名道路连接";
      base.m_name = "roadConnect";
    }
    public override bool Enabled {
      get {
        return m_application.Workspace != null;
      }
    }
    public override void OnClick() {
      if (System.Windows.Forms.MessageBox.Show("将进行合并操作，请确认", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel) {
        return;
      }
      GLayerInfo info = null;
      foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers) {
        if (tempInfo.LayerType == GCityLayerType.道路
            && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
            && tempInfo.OrgLayer != null
            ) {
          info = tempInfo;
          break;
        }
      }
      if (info == null)
        return;
      WaitOperation wo = m_application.SetBusy(true);
      IFeatureClass fc = (info.Layer as IFeatureLayer).FeatureClass;
      int fCount = fc.FeatureCount(null);

      int usedID = fc.FindField("_GenUsed");
      int nameField = fc.FindField("道路名");
      int rankID = fc.FindField("道路等级");
      int widthID = fc.FindField("宽度");
      if (usedID == -1) {
        IFieldEdit2 field = new FieldClass();
        field.Name_2 = "_GenUsed";
        field.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
        field.DefaultValue_2 = 1;
        fc.AddField(field as IField);
        usedID = fc.Fields.FindField("_GenUsed");
      }
      bool isFieldExisted_rank = true;
      bool isFieldExisted_width = true;
      if (rankID == -1) {
        isFieldExisted_rank = false;
      }
      if (widthID == -1) {
        isFieldExisted_width = false;
      }
      IFeatureCursor fCuror = fc.Search(null, false);
      IFeature feature = null;
      //IPoint tempPoi=new PointClass();
      List<IFeature> intersectFeats = new List<IFeature>();
      List<IPoint> fromToPoi = new List<IPoint>();
      Dictionary<int, bool> isExist = new Dictionary<int, bool>();
      try {
        while ((feature = fCuror.NextFeature()) != null) {
          wo.Step(fCount);
          wo.SetText("正在处理" + feature.OID.ToString());
          fromToPoi.Clear();
          IPolyline tpLine = feature.Shape as IPolyline;
          fromToPoi.Add(tpLine.FromPoint);
          fromToPoi.Add(tpLine.ToPoint);
          ISpatialFilter filter = new SpatialFilterClass();
          filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
          foreach (IPoint tempPoi in fromToPoi) {
            intersectFeats.Clear();
            filter.Geometry = tempPoi as IGeometry;
            filter.WhereClause = "_GenUsed = 1";
            IFeatureCursor featCursor = fc.Search(filter, false);
            IFeature nextFeat = null;
            isExist.Clear();
            while ((nextFeat = featCursor.NextFeature()) != null) {
              if (!isExist.ContainsKey(nextFeat.OID)) {
                IPolyline line = nextFeat.Shape as IPolyline;
                if (line.Length == 0) {
                  nextFeat.set_Value(usedID, 2);
                }
                else {
                  intersectFeats.Add(nextFeat);
                  isExist.Add(nextFeat.OID, false);
                }
              }
            }
            if (intersectFeats.Count == 2) {
              IFeature[] intersectFeatArray = intersectFeats.ToArray();
              IFeature oneFeat = intersectFeatArray[0];
              IFeature anotherFeat = intersectFeatArray[1];
              string name = "ywh";
              //foreach (IFeature tpFeat in intersectFeats)
              for (int j = 0; j < 2; j++) {
                IFeature tpFeat = intersectFeatArray[j];
                IPoint nextFromPoi = (tpFeat.Shape as IPolyline).FromPoint;
                IPoint nextToPoi = (tpFeat.Shape as IPolyline).ToPoint;
                if ((tempPoi.X != nextFromPoi.X || tempPoi.Y != nextFromPoi.Y) && (tempPoi.X != nextToPoi.X || tempPoi.Y != nextToPoi.Y)) {
                  break;
                }
                if (name == "ywh") {
                  name = Convert.ToString(tpFeat.get_Value(nameField));
                }
                else {
                  string anotherName = Convert.ToString(tpFeat.get_Value(nameField));
                  if (name != "" && name != " " && name != "无名路" && anotherName != "" && anotherName != " " && anotherName != "无名路" && name != anotherName) {
                    break;
                  }
                  ITopologicalOperator topo = oneFeat.ShapeCopy as ITopologicalOperator;
                  topo.Simplify();
                  ITopologicalOperator topoOther = anotherFeat.ShapeCopy as ITopologicalOperator;
                  topoOther.Simplify();
                  IPolyline mergeLine = topo.Union(topoOther as IGeometry) as IPolyline;

                  int rank = -3;
                  if (isFieldExisted_rank) {
                    int firstRank = Convert.ToInt16(oneFeat.get_Value(rankID));
                    int secondRank = Convert.ToInt16(anotherFeat.get_Value(rankID));
                    if (firstRank < secondRank) {
                      rank = firstRank;
                    }
                    else {
                      rank = secondRank;
                    }
                  }
                  double width = -3;
                  if (isFieldExisted_width) {
                    double firstWidth = Convert.ToDouble(oneFeat.get_Value(widthID));
                    double secondWidth = Convert.ToDouble(anotherFeat.get_Value(widthID));
                    width = firstWidth * ((oneFeat.Shape as IPolyline).Length / ((oneFeat.Shape as IPolyline).Length + (anotherFeat.Shape as IPolyline).Length))
                        + secondWidth * ((oneFeat.Shape as IPolyline).Length / ((oneFeat.Shape as IPolyline).Length + (anotherFeat.Shape as IPolyline).Length));

                  }

                  if (name != "" && name != " " && name != "无名路") {
                    if (tpFeat.OID != oneFeat.OID) {
                      oneFeat.Shape = mergeLine as IGeometry;
                      oneFeat.set_Value(rankID, rank);
                      oneFeat.set_Value(widthID, width);
                      //oneFeat.set_Value(usedID, 2);
                      oneFeat.Store();
                      anotherFeat.Delete();
                    }
                    else {
                      anotherFeat.Shape = mergeLine as IGeometry;
                      anotherFeat.set_Value(rankID, rank);
                      anotherFeat.set_Value(widthID, width);
                      //anotherFeat.set_Value(usedID, 2);
                      anotherFeat.Store();
                      oneFeat.Delete();
                    }
                  }
                  else {
                    if (tpFeat.OID != anotherFeat.OID) {
                      oneFeat.Shape = mergeLine as IGeometry;
                      oneFeat.set_Value(rankID, rank);
                      oneFeat.set_Value(widthID, width);
                      //oneFeat.set_Value(usedID, 2);
                      oneFeat.Store();
                      anotherFeat.Delete();
                    }
                    else {
                      anotherFeat.Shape = mergeLine as IGeometry;
                      anotherFeat.set_Value(rankID, rank);
                      anotherFeat.set_Value(widthID, width);
                      //anotherFeat.set_Value(usedID, 2);
                      anotherFeat.Store();
                      oneFeat.Delete();
                    }
                  }
                }
              }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);
          }

        }
      }
      catch (Exception ex) {
      }
      System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);

      wo.Step(0);
      //IFeatureLayerDefinition fdefinition = info.Layer as IFeatureLayerDefinition;
      //fdefinition.DefinitionExpression = "_GenUsed = 0";
      m_application.MapControl.Refresh();
      m_application.SetBusy(false);
    }
  }
}

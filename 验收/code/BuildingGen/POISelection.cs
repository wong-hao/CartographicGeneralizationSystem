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
  public class POISelection : BaseGenCommand {
    Generalizer gen;
    public POISelection() {
      base.m_category = "GOther";
      base.m_caption = "POI选取";
      base.m_message = "选取POI";
      base.m_toolTip = "选取POI";
      base.m_name = "POISelection";
      base.m_usedParas = new GenDefaultPara[] {          
        new GenDefaultPara("POI选取比例",(double)0.5)//这个是保留的比例            
      };
      gen = new Generalizer();
    }
    public override bool Enabled {
      get {
        return m_application.Workspace != null;
      }
    }
    class NodeWarp {
      public enum eState {
        Normal,
        Locked,
        Aways,
        Delete
      }
      public ITinNode Node { get; set; }
      public NodeWarp(ITinNode node) {
        this.Node = node;
        State = eState.Normal;
      }
      public double Area {
        get { return (Node.GetVoronoiRegion(null) as IArea).Area; }
      }
      public int Index { get { return Node.Index; } }
      public int Tag { get { return Node.TagValue; } }
      public eState State { get; set; }

    }
    bool[] MultiPointSelection(IMultipoint points, List<Int64> value, List<int> code, double rate) {
      TinClass tin = new TinClass();
      tin.InitNew(points.Envelope);
      tin.StartInMemoryEditing();
      IPointCollection pc = points as IPointCollection;
      List<int> tinNodeIndex = new List<int>();
      for (int i = 0; i < pc.PointCount; i++) {
        IPoint p = pc.get_Point(i);
        p.Z = 0;
        tin.AddPointZ(p, i);
      }
      double maxArea = double.MinValue;
      List<NodeWarp> nodes = new List<NodeWarp>();
      Dictionary<int, NodeWarp> dic = new Dictionary<int, NodeWarp>();
      for (int i = 1; i <= tin.NodeCount; i++) {
        ITinNode node = tin.GetNode(i);
        if (!node.IsInsideDataArea)
          continue;
        NodeWarp warp = new NodeWarp(node);
        warp.State = (code[warp.Tag] == 1) ? NodeWarp.eState.Aways : NodeWarp.eState.Normal;
        if (warp.Area > maxArea)
          maxArea = warp.Area;
        nodes.Add(warp);
        dic.Add(warp.Index, warp);
      }
      Comparison<NodeWarp> cmp = (n1, n2) => {
        try {
          return (n1.Area + value[n1.Tag] * maxArea) > (n2.Area + value[n2.Tag] * maxArea) ? 1 : -1;
        }
        catch {
          return 0;
        }
      };
      nodes.Sort(cmp);
      int deleteCount = 0;
      int ci = 0;
      while (deleteCount < pc.PointCount * (1 - rate)) {
        if (ci == nodes.Count) {
          int changeCount = 0;
          nodes.ForEach((nw) => {
            if (nw.State == NodeWarp.eState.Locked) {
              nw.State = NodeWarp.eState.Normal;
              changeCount++;
            }
          });
          if (changeCount == 0)
            break;
          ci = 0;
        }
        NodeWarp n = nodes[ci];
        ci++;

        if (n.State != NodeWarp.eState.Normal) {
          continue;
        }

        n.State = NodeWarp.eState.Delete;
        deleteCount++;
        ITinNodeArray array = n.Node.GetAdjacentNodes();
        for (int i = 0; i < array.Count; i++) {
          ITinNode node = array.get_Element(i);
          if (!node.IsInsideDataArea)
            continue;
          NodeWarp adNode = dic[node.Index];
          if (adNode.State == NodeWarp.eState.Normal)
            adNode.State = NodeWarp.eState.Locked;
        }
      }

      bool[] selected = new bool[pc.PointCount];
      nodes.ForEach((nw) => {
        selected[nw.Tag] = (nw.State != NodeWarp.eState.Delete);
      });
      return selected;
    }
    public override void OnClick() {
      gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 2000, 10000);
      GLayerInfo info = null;
      foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers) {
        if (tempInfo.LayerType == GCityLayerType.POI
          //&& (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
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
      if (usedID == -1) {
        IFieldEdit2 field = new FieldClass();
        field.Name_2 = "_GenUsed";
        field.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
        fc.AddField(field as IField);
        usedID = fc.Fields.FindField("_GenUsed");
      }
      IFeatureCursor fCuror = fc.Search(null, false);
      List<IFeature> features = new List<IFeature>();
      MultipointClass mp = new MultipointClass();
      object miss = Type.Missing;
      IFeature f = null;
      int vIndex = fc.FindField("Weight");
      int cIndex = fc.FindField("Code");
      List<Int64> values = new List<long>();
      List<int> codes = new List<int>();

      while ((f = fCuror.NextFeature()) != null) {
        IPoint p = null;
        if (fc.ShapeType == esriGeometryType.esriGeometryPolyline) {
          IPolyline line = f.Shape as IPolyline;
          p = new PointClass();
          line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, p);
        }
        else {
          p = f.Shape as IPoint;
        }
        mp.AddGeometry(p, ref miss, ref miss);
        values.Add(Convert.ToInt64(f.get_Value(vIndex)));
        codes.Add(Convert.ToInt32(f.get_Value(cIndex)));
        features.Add(f);
        wo.Step(fCount);
      }
      bool[] selected = this.MultiPointSelection(mp, values, codes, (double)m_application.GenPara["POI选取比例"]);
      //IBooleanArray ba = gen.MultiPointSelection(mp, (double)m_application.GenPara["POI选取比例"]);
      wo.Step(0);
      //for (int i = 0; i < ba.Count; i++) {

      //    features[i].set_Value(usedID, ba.get_Item(i) ? 1 : 0);
      //    features[i].Store();
      //    wo.Step(fCount);
      //}
      for (int i = 0; i < selected.Length; i++) {
        features[i].set_Value(usedID, selected[i] ? 1 : 0);
        features[i].Store();
        wo.Step(fCount);
      }
      IFeatureLayerDefinition fdefinition = info.Layer as IFeatureLayerDefinition;
      fdefinition.DefinitionExpression = "_GenUsed = 1";
      m_application.MapControl.Refresh();
      m_application.SetBusy(false);
    }
  }
}

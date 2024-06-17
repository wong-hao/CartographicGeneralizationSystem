using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.NetworkAnalysis;

namespace BuildingGen {
  public class RoadRank : BaseGenCommand {
    double i = 0;//i:控制分阶段显示道路选取过程
    public RoadRank() {
      base.m_category = "GRoad";
      base.m_caption = "自动分级";
      base.m_message = "道路分级";
      base.m_toolTip = "道路分等级";
      base.m_name = "RoadRank";
      m_usedParas = new GenDefaultPara[] { 
            new GenDefaultPara("一级道路宽度",20.0d),
            new GenDefaultPara("二级道路宽度",10.0d),
            new GenDefaultPara("三级道路宽度",4.0d),
            new GenDefaultPara("一级道路长度",5000.0d),
            new GenDefaultPara("二级道路长度",2000.0d),
            new GenDefaultPara("三级道路长度",1000.0d)
           };

    }
    public override bool Enabled {
      get {
        return m_application.Workspace != null
          //&& m_application.Workspace.EditLayer != null
            ;

      }
    }
    public override void OnClick() {
      GLayerInfo roadLayer = null;
      foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers) {
        if (info.LayerType == GCityLayerType.道路
            && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
            && info.OrgLayer != null
            ) {
          roadLayer = info;
          break;
        }
      }
      if (roadLayer == null) {
        System.Windows.Forms.MessageBox.Show("没有找到道路层");
        return;
      }

      IFeatureClass fc = (roadLayer.Layer as IFeatureLayer).FeatureClass;

      int widthID = fc.Fields.FindField("宽度");
      int areaID = fc.FindField("面积");
      if (widthID == -1 || areaID == -1) {
        System.Windows.Forms.MessageBox.Show("请先计算宽度");
        return;
      }
      WaitOperation wo = m_application.SetBusy(true);


      int rankID = fc.Fields.FindField("道路等级");
      if (rankID == -1) {
        IFieldEdit2 field = new FieldClass();
        field.Name_2 = "道路等级";
        field.Type_2 = esriFieldType.esriFieldTypeInteger;
        fc.AddField(field as IField);
        rankID = fc.Fields.FindField("道路等级");
      }

      int strokeID = fc.Fields.FindField("道路分组");
      if (strokeID == -1) {
        IFieldEdit2 field = new FieldClass();
        field.Name_2 = "道路分组";
        field.Type_2 = esriFieldType.esriFieldTypeInteger;
        fc.AddField(field as IField);
        strokeID = fc.Fields.FindField("道路分组");
      }

      int scount = (fc.FeatureCount(null));
      //wo.Step(scount);
      IFeatureCursor fCursor = fc.Search(null, true);
      IFeature lineFeature = null;

      Dictionary<int, int> fid_stroke_dic = new Dictionary<int, int>();
      int strokeValue = 1;
      while ((lineFeature = fCursor.NextFeature()) != null) {
        wo.SetText("正在处理：[" + lineFeature.OID.ToString() + "]");
        wo.Step(scount);
        try {
          if (fid_stroke_dic.ContainsKey(lineFeature.OID))
            continue;
          double width = (double)lineFeature.get_Value(widthID);
          double length = (lineFeature.Shape as IPolyline).Length;
          if (false && LengthEnough(m_application, width, length)) {
            lineFeature.set_Value(strokeID, -1);
            lineFeature.set_Value(rankID, GetRank(m_application, width, length));
            lineFeature.Store();
          }
          else {
            //System.Diagnostics.Debug.WriteLine(lineFeature.OID + "不够长度");

            int count = 0;
            strokeLine info = FindStrokeLine(fc, lineFeature.OID, true);
            while (info != null) {
              if (fid_stroke_dic.ContainsKey(info.OID)) {
                break;
              }
              else {
                fid_stroke_dic.Add(info.OID, strokeValue);
              }
              count++;
              info = FindStrokeLine(fc, info.OID, !(info.fromPoint));
            }
            info = FindStrokeLine(fc, lineFeature.OID, false);
            while (info != null) {
              if (fid_stroke_dic.ContainsKey(info.OID)) {
                break;
              }
              else {
                fid_stroke_dic.Add(info.OID, strokeValue);
              }
              count++;
              info = FindStrokeLine(fc, info.OID, !(info.fromPoint));
            }
            if (count == 0) {
              System.Diagnostics.Debug.WriteLine(lineFeature.OID + "找不到连接线");
              lineFeature.set_Value(strokeID, -1);
              lineFeature.set_Value(rankID, GetRank(m_application, width, length));
              lineFeature.Store();
            }
            else {
              if (!fid_stroke_dic.ContainsKey(lineFeature.OID)) {
                fid_stroke_dic.Add(lineFeature.OID, strokeValue);
              }
              strokeValue++;
            }
          }
        }
        catch {
        }

      }
      wo.SetText("正在完成计算工作");
      GeoDatabaseHelperClass gh = new GeoDatabaseHelperClass();
      scount = fid_stroke_dic.Count;
      //wo.Step(scount);
      //wo.SetMaxValue(fid_stroke_dic.Count);
      int[] ids = new int[fid_stroke_dic.Count];
      fid_stroke_dic.Keys.CopyTo(ids, 0);
      IFeatureCursor strokeCursor = gh.GetFeatures(fc, ref ids, false);
      Dictionary<int, List<IFeature>> stroke_feature_dic = new Dictionary<int, List<IFeature>>();
      while ((lineFeature = strokeCursor.NextFeature()) != null) {
        wo.SetText("正在处理例外：[" + lineFeature.OID.ToString() + "]");
        wo.Step(scount);
        int sid = fid_stroke_dic[lineFeature.OID];
        if (!stroke_feature_dic.ContainsKey(sid)) {
          stroke_feature_dic.Add(sid, new List<IFeature>());
        }
        stroke_feature_dic[sid].Add(lineFeature);
      }

      foreach (int sid in stroke_feature_dic.Keys) {
        double length = 0;
        double area = 0;
        foreach (IFeature item in stroke_feature_dic[sid]) {
          length += (item.Shape as IPolycurve).Length;
          area += (double)item.get_Value(areaID);
        }
        double stroke_width = area / length;
        int stroke_rand = GetRank(m_application, stroke_width, length);
        foreach (IFeature item in stroke_feature_dic[sid]) {
          item.set_Value(rankID, stroke_rand);
          item.set_Value(widthID, stroke_width);
          item.set_Value(strokeID, sid);
          item.Store();
        }
      }

      //fCursor.Flush();
      m_application.SetBusy(false);
      m_application.MapControl.Refresh();

    }

    internal static int GetRank(GApplication app, double width, double length) {
      try {
        if (width > (double)app.GenPara["一级道路宽度"]
            && length > (double)app.GenPara["一级道路长度"]) {
          return 1;
        }
        else if (width > (double)app.GenPara["二级道路宽度"]
            && length > (double)app.GenPara["二级道路长度"]) {
          return 2;
        }
        else if (width > (double)app.GenPara["三级道路宽度"]
            && length > (double)app.GenPara["三级道路长度"]) {
          if (width > (double)app.GenPara["一级道路宽度"])
            return 2;
          return 3;
        }
        else {
          if (width > (double)app.GenPara["二级道路宽度"] && length > (double)app.GenPara["三级道路长度"] / 2)
            return 3;
          return 4;
        }
      }
      catch {
        return 1;
      }
    }

    internal static bool LengthEnough(GApplication app, double width, double length) {
      try {
        if (width > (double)app.GenPara["一级道路宽度"]
            && length < (double)app.GenPara["一级道路长度"]) {
          return false;
        }
        else if (width > (double)app.GenPara["二级道路宽度"]
            && length < (double)app.GenPara["二级道路长度"]) {
          return false;
        }
        else if (width > (double)app.GenPara["三级道路宽度"]
            && length < (double)app.GenPara["三级道路长度"]) {
          return false;
        }
        else {
          return true;
        }
      }
      catch {
        return true;
      }
    }

    internal class strokeLine {
      internal int OID;
      internal bool fromPoint;
      internal double length;
    }
    internal static strokeLine FindStrokeLine(IFeatureClass fc, int oid, bool fromPoint) {
      IFeature self = fc.GetFeature(oid);
      strokeLine result = new strokeLine();

      IPoint passPoint = null;
      IPoint beforePoint = null;
      if (fromPoint) {
        passPoint = (self.Shape as IPolyline).FromPoint;
        beforePoint = (self.Shape as IPointCollection).get_Point(1);
      }
      else {
        passPoint = (self.Shape as IPolyline).ToPoint;
        beforePoint = (self.Shape as IPointCollection).get_Point((self.Shape as IPointCollection).PointCount - 2);
      }
      ISpatialFilter sf = new SpatialFilterClass();
      sf.Geometry = (passPoint as ITopologicalOperator).Buffer(5);
      sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      int count = fc.FeatureCount(sf);
      if (count < 2) {
        System.Runtime.InteropServices.Marshal.ReleaseComObject(self);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(sf);
        return null;
      }
      else {
        IFeatureCursor fCursor = fc.Search(sf, true);
        IFeature f = null;
        double minAngle = double.MaxValue;
        result.OID = -1;
        result.fromPoint = true;
        while ((f = fCursor.NextFeature()) != null) {
          if (f.OID == self.OID) {
            continue;
          }
          IPoint fp = (f.Shape as IPolyline).FromPoint;
          if ((passPoint as IProximityOperator).ReturnDistance(fp) < 0.01) {
            IPoint toPoint = (f.Shape as IPointCollection).get_Point(1);
            double a = Angle(beforePoint, passPoint, toPoint);
            if (a < minAngle && a < Math.PI / 6) {
              result.OID = f.OID;
              result.fromPoint = true;
              result.length = (f.Shape as IPolyline).Length;
            }
          }
          IPoint tp = (f.Shape as IPolyline).ToPoint;
          if ((passPoint as IProximityOperator).ReturnDistance(tp) < 0.01) {
            IPoint toPoint = (f.Shape as IPointCollection).get_Point((f.Shape as IPointCollection).PointCount - 2);
            double a = Angle(beforePoint, passPoint, toPoint);
            if (a < minAngle && a < Math.PI / 6) {
              result.OID = f.OID;
              result.fromPoint = false;
              result.length = (f.Shape as IPolyline).Length;
            }
          }
        }
        System.Runtime.InteropServices.Marshal.ReleaseComObject(self);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(sf);
        System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);

        if (result.OID != -1) {
          return result;
        }
        else {
          return null;
        }
      }
    }
    static double Angle(IPoint from, IPoint pass, IPoint to) {
      double a = GApplication.GeometryEnvironment.ConstructThreePoint(from, pass, to);
      return Math.PI - Math.Abs(a);
    }
  }
}

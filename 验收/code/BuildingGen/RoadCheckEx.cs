using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

using BuildingGen.Road;

namespace BuildingGen {
  class RoadCheckEx : BaseGenCommand {

    public enum RoadOperation { 
      删除与延伸,
      升级与降级
    }

    public RoadCheckEx() {
      base.m_category = "GRoad";
      base.m_caption = "悬挂线处理";
      base.m_message = "对道路中的悬挂线进行处理，默认已经做过道路打散";
      base.m_toolTip = "对道路中的悬挂线进行处理\n默认已经做过道路打散";
      base.m_name = "RoadCheckEx";
      base.m_usedParas = new GenDefaultPara[]{
        new GenDefaultPara("最小道路出头长度",(double)50)
        ,new GenDefaultPara("最大道路挂接距离",(double)10)
        ,new GenDefaultPara("悬挂线处理方式",(RoadOperation)RoadOperation.删除与延伸)
        ,new GenDefaultPara("悬挂线处理等级",(int) 5)
      };
    }
    public override bool Enabled {
      get {
        return m_application.Workspace != null;
      }
    }

    public override void OnClick() {
      GLayerInfo info = GetLayer();      
      CheckLayer(info);
      m_application.MapControl.Refresh();
    }

    /// <summary>
    /// 删除短悬挂线
    /// </summary>
    /// <param name="layer"></param>
    private void CheckLayer(GLayerInfo layer) {
      IFeatureLayer flayer = layer.Layer as IFeatureLayer;
      IFeatureClass fc = flayer.FeatureClass;
      RoadOperation roadOpt = (RoadOperation)m_application.GenPara["悬挂线处理方式"];
      int checkLever = (int)m_application.GenPara["悬挂线处理等级"];
      IQueryFilter qf = new QueryFilterClass();
      qf.WhereClause = "道路等级 < " + (checkLever + 1).ToString();

      Graph g = new Graph(fc,qf);
      List<GraphNode> hangNodes = g.GetHangNode();
#if DEBUG
      ESRI.ArcGIS.Display.IDisplay dis = m_application.MapControl.ActiveView.ScreenDisplay;
      ESRI.ArcGIS.Display.SimpleMarkerSymbolClass sms = new ESRI.ArcGIS.Display.SimpleMarkerSymbolClass();
      sms.Style = ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSCircle;
      sms.Size = 5;
      dis.StartDrawing(dis.hDC, 0);
      dis.SetSymbol(sms);
      foreach (var item in hangNodes) {
        dis.DrawPoint(item.point);
      }
      dis.FinishDrawing();
      System.Windows.Forms.MessageBox.Show("test");
      //return;
#endif
      if (roadOpt == RoadOperation.删除与延伸) {
        DeleteAndExtent(fc, g);
      }
      else {
        ChangeLever(fc, g);
      }
    }//CheckLayer

    private void ChangeLever(IFeatureClass fc, Graph g) {
      double minLength = (double)m_application.GenPara["最小道路出头长度"];
      double minDistance = (double)m_application.GenPara["最大道路挂接距离"];
      int checkLever = (int)m_application.GenPara["悬挂线处理等级"];
      int roadLeverIndex = fc.FindField("道路等级");
      ISpatialFilter sf = new SpatialFilterClass();
      List<GraphNode> hangNodes = g.GetHangNode();

      sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      foreach (var currentHangNode in hangNodes) {
        int oidvalue = currentHangNode.conectFeatures[0];
        int oid = GraphNode.GetOID(oidvalue);
        bool isFromPoint = GraphNode.GetIsFromPoint(oidvalue);

        IFeature feautre = fc.GetFeature(oid);
        IPolyline line = feautre.Shape as IPolyline;

        //如果前后都很短
        #region 悬挂线很短，直接降级
        if (true) {
          int nextValue = GraphNode.GetReversalIndex(oidvalue);
          GraphNode nextNote = g.nodes[g.linesNode[nextValue]];
          double allLength = 0;
          List<IFeature> featureToChange = new List<IFeature>();
          featureToChange.Add(feautre);
          allLength += line.Length;
          bool isLenghtEnough = (allLength < minLength);
          while (!isLenghtEnough) {
            if (nextNote.conectFeatures.Count == 2) {
              int otherValue = (nextNote.conectFeatures[0] == nextValue) ? nextNote.conectFeatures[1] : nextNote.conectFeatures[0];
              int otherFeatureID = GraphNode.GetOID(otherValue);
              IFeature otherFeature = fc.GetFeature(otherFeatureID);
              IPolyline otherLine = otherFeature.Shape as IPolyline;
              allLength += otherLine.Length;
              isLenghtEnough = (allLength < minLength);
              featureToChange.Add(otherFeature);
              nextValue = GraphNode.GetReversalIndex(otherValue);
              nextNote = g.nodes[g.linesNode[nextValue]];
            }
            else {
              break;
            }
          }
          if (!isLenghtEnough) {
            foreach (var featureItem in featureToChange) {
              featureItem.set_Value(roadLeverIndex, checkLever + 1);
              featureItem.Store();
            }
            continue;
          }
        }
        #endregion
        
        IPoint p = isFromPoint ? line.FromPoint : line.ToPoint;
        IGeometry geo = (p as ITopologicalOperator).Buffer(minDistance);
        sf.Geometry = geo;
        Graph tmpGraph = new Graph(fc, sf);
        GraphNode tmpNode = tmpGraph.nodes[tmpGraph.linesNode[oidvalue]];
        FindInfo currentFindInfo = new FindInfo(null, tmpNode, oidvalue, 0);
        SortedDictionary<FindInfo, int> findInfos = new SortedDictionary<FindInfo, int>();
        findInfos.Add(currentFindInfo, 0);
        while (true) {
          currentFindInfo = null;
          foreach (var findItem in findInfos.Keys) {
            currentFindInfo = findItem;
            break;
          }
          findInfos.Remove(currentFindInfo);

          if (currentFindInfo == null)
            break;
          if (currentFindInfo.AllLength > minDistance) {
            break;
          }
          foreach (var itemmm in currentFindInfo.CurrentNode.conectFeatures) {
            if (itemmm == currentFindInfo.FromPathId)
              continue;

            int otherValue = GraphNode.GetReversalIndex(itemmm);
            int findFID = GraphNode.GetOID(itemmm);
            IFeature findFeature = fc.GetFeature(findFID);
            IPolyline findLine = findFeature.Shape as IPolyline;
            int findLever = (int)findFeature.get_Value(roadLeverIndex);
            if (findLever <= checkLever) {
              FindInfo setNode = currentFindInfo;
              while (setNode.ParentNode !=null) {
                int setFID = GraphNode.GetOID(setNode.FromPathId);
                IFeature setFeature = fc.GetFeature(setFID);
                setFeature.set_Value(roadLeverIndex, checkLever);
                setFeature.Store();
              }
              break;
            }
            GraphNode findNode = tmpGraph.nodes[tmpGraph.linesNode[otherValue]];
            FindInfo childFindInfo = new FindInfo(currentFindInfo, findNode, otherValue, findLine.Length);
            findInfos.Add(childFindInfo, 0);
          }
        }

      }//foreach (var item in hangNodes)
    }

    internal class FindInfo:IComparable {
      public FindInfo(FindInfo parentNode,GraphNode currentNode,int fromPathId,double length) {
        ParentNode = parentNode;
        FromPathId = fromPathId;
        Length = length;
        CurrentNode = currentNode;
      }
      public FindInfo ParentNode{get;private set;}
      public GraphNode CurrentNode { get; private set; }
      public int FromPathId { get; private set; }
      public double Length { get; private set; }
      public double AllLength {
        get {
          if (ParentNode == null)
            return Length;
          else
            return ParentNode.AllLength + Length;
        }
      }

      #region IComparable 成员

      public int CompareTo(object obj) {
        return (this.AllLength > (obj as FindInfo).AllLength) ? 1 : -1;
      }

      #endregion
    }
    private void DeleteAndExtent(IFeatureClass fc,Graph g) {
      double minLength = (double)m_application.GenPara["最小道路出头长度"];
      double minDistance = (double)m_application.GenPara["最大道路挂接距离"];
      Dictionary<int, bool> deleteFeatures = new Dictionary<int, bool>();
      ISpatialFilter sf = new SpatialFilterClass();
      List<GraphNode> hangNodes = g.GetHangNode();

      sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      foreach (var item in hangNodes) {
        int oidvalue = item.conectFeatures[0];
        int oid = GraphNode.GetOID(oidvalue);
        bool isFromPoint = GraphNode.GetIsFromPoint(oidvalue);

        IFeature feautre = fc.GetFeature(oid);
        IPolyline line = feautre.Shape as IPolyline;
        //长度过短的直接删除
        if (line.Length < minDistance) {
          if (!deleteFeatures.ContainsKey(oid))
            deleteFeatures.Add(oid, isFromPoint);
          continue;
        }
        IPoint p = isFromPoint ? line.FromPoint : line.ToPoint;
        IGeometry geo = (p as ITopologicalOperator).Buffer(minDistance);
        sf.Geometry = geo;
        if (fc.FeatureCount(sf) < 2) {
          if (line.Length < minLength)
            if (!deleteFeatures.ContainsKey(oid))
              deleteFeatures.Add(oid, isFromPoint);
          continue;
        }

        bool findExtent = false;
        IPolyline nearExtentLine = null;
        esriCurveExtension flag = isFromPoint ? esriCurveExtension.esriNoExtendAtTo : esriCurveExtension.esriNoExtendAtFrom;
        IFeatureCursor fCursor = fc.Search(sf, true);
        IFeature nearFeature = null;
        while ((nearFeature = fCursor.NextFeature()) != null) {
          if (nearFeature.OID == feautre.OID)
            continue;
          PolylineClass extentLine = new PolylineClass();
          bool extensionsPerformed = false;
          extentLine.ConstructExtended(line, nearFeature.Shape as IPolyline, (int)flag, ref extensionsPerformed);
          if (extensionsPerformed && (extentLine.Length - line.Length < minDistance * 2)) {
            findExtent = true;
            if (nearExtentLine == null) {
              nearExtentLine = extentLine;
            }
            else {
              if (nearExtentLine.Length > extentLine.Length) {
                nearExtentLine = extentLine;
              }//if
            }//else
          }//if
          //之前找到了延伸线就不找最近点了,否则接着找最近点
          if (findExtent)
            continue;
          IPoint nearPoint = new PointClass();
          double distanceAlongCurve = 0;
          double distanceFromCurve = 0;
          bool bRightSide = false;
          (nearFeature.Shape as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, p, false
            , nearPoint, ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
          IPointCollection newLine = feautre.ShapeCopy as IPointCollection;

          if (isFromPoint)
            newLine.InsertPoints(0, 1, ref nearPoint);
          else
            newLine.AddPoints(1, ref nearPoint);

          if ((nearExtentLine == null) || nearExtentLine.Length > (newLine as IPolyline).Length) {
            nearExtentLine = newLine as IPolyline;
          }//if          
        }//while

        feautre.Shape = nearExtentLine;
        feautre.Store();

      }//foreach (var item in hangNodes)
      foreach (var item in deleteFeatures.Keys) {
        IFeature fe = fc.GetFeature(item);
        fe.Delete();
      }//foreach      

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

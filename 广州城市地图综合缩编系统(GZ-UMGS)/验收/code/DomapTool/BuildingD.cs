using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;

namespace DomapTool {
  /// <summary>
  /// Summary description for BuildingD.
  /// </summary>
  [Guid("ab2485c2-7868-4480-a5be-5f3d62f0c4be")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DomapTool.BuildingD")]
  public sealed class BuildingD : BaseTool {
    #region COM Registration Function(s)
    [ComRegisterFunction()]
    [ComVisible(false)]
    static void RegisterFunction(Type registerType) {
      // Required for ArcGIS Component Category Registrar support
      ArcGISCategoryRegistration(registerType);

      //
      // TODO: Add any COM registration code here
      //
    }

    [ComUnregisterFunction()]
    [ComVisible(false)]
    static void UnregisterFunction(Type registerType) {
      // Required for ArcGIS Component Category Registrar support
      ArcGISCategoryUnregistration(registerType);

      //
      // TODO: Add any COM unregistration code here
      //
    }

    #region ArcGIS Component Category Registrar generated code
    /// <summary>
    /// Required method for ArcGIS Component Category registration -
    /// Do not modify the contents of this method with the code editor.
    /// </summary>
    private static void ArcGISCategoryRegistration(Type registerType) {
      string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
      MxCommands.Register(regKey);

    }
    /// <summary>
    /// Required method for ArcGIS Component Category unregistration -
    /// Do not modify the contents of this method with the code editor.
    /// </summary>
    private static void ArcGISCategoryUnregistration(Type registerType) {
      string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
      MxCommands.Unregister(regKey);

    }

    #endregion
    #endregion

    private IApplication m_application;
    INewPolygonFeedback fb;
    int fieldID;
    int maxValue;
    IDataStatistics statistics;
    IPolygon lastPolygon;
    bool needDrawLastPolygon = false;
    public BuildingD() {
      //
      // TODO: Define values for the public properties
      //
      statistics = new DataStatisticsClass();
      base.m_category = "Domap"; //localizable text 
      base.m_caption = "自动分组";  //localizable text 
      base.m_message = "建筑物自动分组";  //localizable text
      base.m_toolTip = "绘制多边形选择建筑物区域，\n由计算机对建筑物自动分组。\n按L显示最后操作的区域。";  //localizable text
      base.m_name = "DomapTool.BuildingD";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
      try {
        //
        // TODO: change resource name if necessary
        //
        string bitmapResourceName = GetType().Name + ".bmp";
        base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
        base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
      }
      catch (Exception ex) {
        System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
      }

      GGenPara.Para.RegistPara("建筑物最小上图面积", (double)400);
      GGenPara.Para.RegistPara("建筑物合并距离", (double)5);
    }

    #region Overriden Class Methods
    IEditor editor;
    IFeatureClass fc = null;
    IFeatureLayer buildLayer = null;
    IFeatureLayer roadLayer = null;
    /// <summary>
    /// Occurs when this tool is created
    /// </summary>
    /// <param name="hook">Instance of the application</param>
    public override void OnCreate(object hook) {
      m_application = hook as IApplication;

      //Disable if it is not ArcMap
      if (hook is IMxApplication)
        base.m_enabled = true;
      else
        base.m_enabled = false;
      editor = m_application.FindExtensionByName("ESRI Object Editor") as IEditor;
      // TODO:  Add other initialization code
    }
    /// <summary>
    /// Occurs when this tool is clicked
    /// </summary>

    public override bool Enabled {
      get {
        return base.Enabled && editor.EditState == esriEditState.esriStateEditing;
      }
    }

    double area;
    double dis;
    IActiveView ac;
    bool registDraw = false;
    public override void OnClick() {
      ac = (m_application.Document as IMxDocument).ActiveView;
      //DeleteMinHole d = new DeleteMinHole((m_application.Document as IMxDocument).ActiveView.FocusMap);
      //d.Text = "自动分组";
      //d.label2 = "自动分组面积";            
      //d.label3 = "自动分组距离";
      //d.textBox1.Text = "400";
      //d.textBox2.Text = "20";
      BuildingDDialog d = new BuildingDDialog(ac.FocusMap);
      if (d.ShowDialog() == DialogResult.Cancel)
        return;
      buildLayer = d.buildingLayer;
      if (d.roadLayer.Name != "无") {
        roadLayer = d.roadLayer;
      }
      area = d.BArea;
      dis = d.BDis;
      GGenPara.SavePara();
      fc = buildLayer.FeatureClass;
      if (buildLayer == null) {
        MessageBox.Show("请确保建筑物图层不为空!");
        return;
      }
      if (!registDraw) {
        IActiveViewEvents_Event events = ac as IActiveViewEvents_Event;
        events.AfterDraw += new IActiveViewEvents_AfterDrawEventHandler(events_AfterDraw);
      }

      fieldID = CheckField(buildLayer);
      if (fieldID == -1) {
        MessageBox.Show("请先加入整型字段【G_Group】");
        return;
      }
      IQueryFilter qf = new QueryFilterClass();
      qf.WhereClause = "G_Group >0";
      statistics.Cursor = buildLayer.Search(qf, true) as ICursor;
      statistics.Field = "G_Group";
      try {
        maxValue = Convert.ToInt32(statistics.Statistics.Maximum);
      }
      catch {
        maxValue = 0;
      }
    }
    private int CheckField(IFeatureLayer blayer) {

      IFeatureClass fc = blayer.FeatureClass;
      int index = fc.FindField("G_Group");
      //if (index == -1)
      //{
      //    IFieldEdit field = new FieldClass();
      //    field.Name_2 = "G_Group";
      //    field.Type_2 = esriFieldType.esriFieldTypeInteger;
      //    field.DefaultValue_2 = 0;
      //    field.Editable_2 = true;
      //    field.IsNullable_2 = false;
      //    IFields fss = fc.Fields;
      //    IFieldsEdit fsse = fss as IFieldsEdit;
      //    fsse.AddField(field);
      //    index = fc.FindField("G_Group");
      //    ClearGroup(fc, index);
      //}
      if (index == -1)
        return -1;
      (blayer as IGeoFeatureLayer).DisplayAnnotation = true;
      ESRI.ArcGIS.Carto.IAnnotateLayerPropertiesCollection alp = (blayer as IGeoFeatureLayer).AnnotationProperties;
      alp.Clear();
      ESRI.ArcGIS.Carto.ILabelEngineLayerProperties lelp = new ESRI.ArcGIS.Carto.LabelEngineLayerPropertiesClass();
      lelp.Expression = "[G_Group]";
      lelp.BasicOverposterLayerProperties.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
      alp.Add(lelp as ESRI.ArcGIS.Carto.IAnnotateLayerProperties);
      //(info.Layer as IGeoFeatureLayer).DisplayField = "G_BuildingGroup";
      return index;
    }
    private void ClearGroup(IFeatureClass fc, int index) {
      IQueryFilter qf = new QueryFilterClass();
      qf.WhereClause = "G_Group !=-1";
      IFeatureCursor cursor = fc.Update(null, true);
      IFeature f = null;
      while ((f = cursor.NextFeature()) != null) {
        object v = f.get_Value(index);
        f.set_Value(index, 0);
        cursor.UpdateFeature(f);
      }
      cursor.Flush();
      System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
    }
    void events_AfterDraw(ESRI.ArcGIS.Display.IDisplay dis, esriViewDrawPhase phase) {
      if (needDrawLastPolygon && (lastPolygon != null)) {
        SimpleFillSymbolClass sfs = new SimpleFillSymbolClass();
        sfs.Style = esriSimpleFillStyle.esriSFSNull;
        SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
        RgbColorClass rgb = new RgbColorClass();
        rgb.Red = 255;
        rgb.Green = 0;
        rgb.Blue = 0;
        sls.Color = rgb;
        sls.Width = 2;
        sfs.Outline = sls;
        dis.SetSymbol(sfs);
        dis.DrawPolygon(lastPolygon);
      }
    }

    public override void OnMouseDown(int Button, int Shift, int X, int Y) {
      if (Button == 4)
        return;
      IPoint p = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
      if (fb == null) {
        fb = new NewPolygonFeedbackClass();
        fb.Display = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay;
        fb.Start(p);
      }
      else {
        fb.AddPoint(p);
      }
    }

    public override void OnMouseMove(int Button, int Shift, int X, int Y) {
      if (Button == 4)
        return;
      if (fb != null) {
        IPoint p = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
        fb.MoveTo(p);
      }
    }

    public override void OnMouseUp(int Button, int Shift, int X, int Y) {
      // TODO:  Add BuildingD.OnMouseUp implementation
    }

    public override void OnDblClick() {
      if (fb != null) {
        IPolygon poly = fb.Stop();
        fb = null;
        if (poly == null || poly.IsEmpty) {
          return;
        }
        lastPolygon = poly;
        Gen2(poly);
        ac.Refresh();
      }
    }

    #endregion

    private void Gen2(IPolygon range) {
      IFeatureLayer layer = buildLayer;
      ISpatialFilter sf = new SpatialFilterClass();
      sf.Geometry = range;
      sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      int allCount = layer.FeatureClass.FeatureCount(sf);

      IFeatureCursor fCursor = layer.FeatureClass.Search(sf, true);
      IFeature feature = null;

      IFeatureLayer rLayer = null;
      IFeatureCursor roadCursor = null;

      if (roadLayer != null) {
        rLayer = roadLayer;
        roadCursor = rLayer.FeatureClass.Search(sf, true);
        allCount += rLayer.FeatureClass.FeatureCount(sf);
      }

      object z = 0;
      object miss = Type.Missing;
      double paraDistance = dis;
      ISpatialReferenceFactory srf_ = new SpatialReferenceEnvironmentClass();
      IProjectedCoordinateSystem pcs_ = srf_.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_CM_114E);

      try {
        // 1建网
        IStatusBar sBar = m_application.StatusBar;
        IStepProgressor sPro = sBar.ProgressBar;
        sPro.Position = 0;
        sBar.ShowProgressBar("正在进行分析准备...", 0, allCount, 1, true);
        //aSetText("正在进行分析准备");
        TinClass tin = new TinClass();
        tin.InitNew(ac.FullExtent);
        tin.StartInMemoryEditing();
        Dictionary<int, FeatureNode> nodes = new Dictionary<int, FeatureNode>();
        List<FeatureNode> nodeList = new List<FeatureNode>();
        while ((feature = fCursor.NextFeature()) != null) {
          sBar.StepProgressBar();
          IGeometry geoCopy = feature.ShapeCopy;
          geoCopy.SpatialReference = pcs_;
          IPolygon geo = (geoCopy as ITopologicalOperator).Buffer(-0.1) as IPolygon;
          geo.Generalize(0.1);
          geo.Densify(paraDistance / 5, 0);
          FeatureNode n = new FeatureNode(feature.OID, (feature.Shape as IArea).Area);
          nodes.Add(feature.OID, n);
          nodeList.Add(n);
          for (int i = 0; i < (geo as IPointCollection).PointCount; i++) {
            IPoint p = (geo as IPointCollection).get_Point(i);
            p.Z = 0;
            tin.AddPointZ(p, feature.OID);
          }
        }
        if (roadCursor != null) {
          while ((feature = roadCursor.NextFeature()) != null) {
            sBar.StepProgressBar();
            IGeometry geoCopy = feature.ShapeCopy;
            geoCopy.SpatialReference = pcs_;
            IPolyline geo = geoCopy as IPolyline;
            geo.Densify(paraDistance / 5, 0);
            for (int i = 0; i < (geo as IPointCollection).PointCount; i++) {
              IPoint p = (geo as IPointCollection).get_Point(i);
              p.Z = 0;
              tin.AddPointZ(p, -1);
            }

          }
        }

        //2 .找到要素之间的临近关系图
        sPro.Position = 0;
        sBar.ShowProgressBar("正在分析数据...", 0, tin.TriangleCount, 1, true);
        //aSetText("正在分析数据");
        for (int i = 1; i <= tin.TriangleCount; i++) {
          sBar.StepProgressBar();
          ITinTriangle triangle = tin.GetTriangle(i);
          if (!triangle.IsInsideDataArea) {
            continue;
          }

          ITinEdge edge = triangle.get_Edge(0);
          int tag1 = edge.FromNode.TagValue;
          int tag2 = edge.ToNode.TagValue;
          int tag3 = edge.GetNextInTriangle().ToNode.TagValue;

          if (tag1 == -1 || tag2 == -1 || tag3 == -1) {
            continue;
          }//有一个顶点在路上
          if (tag1 == tag2 && tag2 == tag3) {
            continue;
          }//同一个要素上                
          else if (tag1 != tag2 && tag1 != tag3 && tag2 != tag3) {
            continue;
          }//三个不同要素上
          else {
            double wOnFeature = 0;
            double disBeFeature = 0;

            if (tag3 == tag2) {
              edge = edge.GetNextInTriangle();
            }
            else if (tag1 == tag3) {
              edge = edge.GetPreviousInTriangle();
            }
            wOnFeature = edge.Length;
            PointClass centerPoint = new PointClass();
            centerPoint.X = (edge.FromNode.X + edge.ToNode.X) / 2;
            centerPoint.Y = (edge.FromNode.Y + edge.ToNode.Y) / 2;
            IPoint toPoint = new PointClass();
            edge.GetNextInTriangle().ToNode.QueryAsPoint(toPoint);
            disBeFeature = centerPoint.ReturnDistance(toPoint);
            double ra = disBeFeature / paraDistance;
            double rate = (1 - ra);
            if (rate <= 0)
              continue;
            double v = wOnFeature * (0.3 * rate + 0.7);

            FeatureNode fNode = nodes[edge.GetNextInTriangle().ToNode.TagValue];
            FeatureNode tNode = nodes[edge.FromNode.TagValue];
            FeatureEdge feaEdge = null;
            if (fNode.Edges.ContainsKey(tNode)) {
              feaEdge = fNode.Edges[tNode];
            }
            else {
              feaEdge = new FeatureEdge(fNode, tNode);
            }
            feaEdge.Value += v;
          }
        }

        // 3 分组   
        // 3.1 排序 
        sPro.Position = 1;
        sBar.ShowProgressBar("正在自动分组...", 0, 1, 1, true);
        //aSetText("正在自动分组");
        nodeList.Sort();
        LinkedList<FeatureNode> linkNodes = new LinkedList<FeatureNode>(nodeList);

        List<FeatureNode> results = new List<FeatureNode>();
        while (linkNodes.First != null) {
          if (AreaEnough(linkNodes.First.Value.Area)) {
            results.AddRange(linkNodes);
            break;
          }
          LinkedListNode<FeatureNode> linkNode = linkNodes.First;
          FeatureNode best = null;
          double max = double.MinValue;
          double baseArea = linkNode.Value.Area;
          foreach (var item in linkNode.Value.Edges.Keys) {
            double v = linkNode.Value.Edges[item].Value * (0.2 + 0.8 * baseArea / (item.Area + baseArea));
            if (v > max) {
              best = item;
              max = v;
            }
          }
          if (best == null) {
            results.Add(linkNode.Value);
            linkNodes.Remove(linkNodes.First);
          }
          else {
            best.Merge(linkNodes.First.Value);
            linkNodes.Remove(linkNodes.First);
            linkNode = linkNodes.Find(best);
            LinkedListNode<FeatureNode> next = linkNode.Next;
            linkNodes.Remove(linkNode);
            while (next != null) {
              if (next.Value.Area > best.Area) {
                linkNodes.AddBefore(next, best);
                break;
              }
              else {
                next = next.Next;
              }
            }
            if (next == null) {
              if (linkNodes.Count > 0)
                linkNodes.AddLast(best);
              else
                results.Add(best);
            }
          }
        }


        //4 写结果
        sPro.Position = 0;
        sBar.ShowProgressBar("正在保存分组结果...", 0, results.Count, 1, true);
        //aSetText("正在保存分组结果");
        editor.StartOperation();
        foreach (var item in results) {
          sBar.StepProgressBar();
          maxValue++;
          foreach (var oid in item.OIDs) {
            //IFeature fe = layer.FeatureClass.GetFeature(oid);
            //fe.set_Value(fieldID, maxValue);
            //fe.Store();
            IRow rw = (layer.FeatureClass as ITable).GetRow(oid);
            rw.set_Value(fieldID, maxValue);
            rw.Store();
          }
        }
        editor.StopOperation("自动分组");
        sBar.HideProgressBar();
      }
      catch (Exception ex) {
        editor.StopOperation("自动分组");
      }
    }
    internal class FeatureNode : IComparable {
      internal List<int> OIDs;
      internal double Area;
      internal Dictionary<FeatureNode, FeatureEdge> Edges;
      internal FeatureNode(int oid, double area) {
        Edges = new Dictionary<FeatureNode, FeatureEdge>();
        this.OIDs = new List<int>();
        OIDs.Add(oid);
        this.Area = area;
      }

      internal void Merge(FeatureNode other) {
        OIDs.AddRange(other.OIDs);
        other.OIDs.Clear();

        Area += other.Area;
        other.Area = 0;

        foreach (var item in other.Edges.Keys) {
          if (item == this) {
            this.Edges.Remove(other);
            continue;
          }
          //double value = other.Edges[item].Value;
          if (this.Edges.ContainsKey(item)) {
            this.Edges[item].Value += other.Edges[item].Value;
            item.Edges.Remove(other);
          }
          else {
            this.Edges.Add(item, other.Edges[item]);
            item.Edges.Remove(other);
            item.Edges.Add(this, other.Edges[item]);
          }
        }
        other.Edges.Clear();

      }

      #region IComparable 成员

      public int CompareTo(object obj) {
        return this.Area < (obj as FeatureNode).Area ? -1 : 1;
      }

      #endregion

    }
    internal class FeatureEdge {
      internal FeatureNode FromNode;
      internal FeatureNode ToNode;
      internal double Value;
      internal FeatureEdge(FeatureNode from, FeatureNode to) {
        this.FromNode = from;
        FromNode.Edges.Add(to, this);
        this.ToNode = to;
        ToNode.Edges.Add(from, this);
        Value = 0;
      }
    }
    private bool AreaEnough(double areab) {
      return areab > area;
    }

    public override void OnKeyDown(int keyCode, int Shift) {
      switch (keyCode) {
        case (int)System.Windows.Forms.Keys.Escape:
          if (fb != null) {
            fb.Stop();
            fb = null;
          }
          break;
        case (int)System.Windows.Forms.Keys.L:
          needDrawLastPolygon = !needDrawLastPolygon;
          ac.Refresh();
          break;
      }
    }
  }
}

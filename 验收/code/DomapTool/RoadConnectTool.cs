using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using System.Windows.Forms;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;

namespace DomapTool {
  /// <summary>
  /// Summary description for RoadConnectTool.
  /// </summary>
  [Guid("2634920f-2454-4b93-81b8-0363d9efe2aa")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DomapTool.RoadConnectTool")]
  public sealed class RoadConnectTool : BaseTool {
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

    IFeatureLayer roadLayer;
    SimpleFillSymbolClass sfs;
    SimpleMarkerSymbolClass sms;
    private IApplication m_application;
    private IEditor editor;
    public RoadConnectTool() {
      //
      // TODO: Define values for the public properties
      //
      base.m_category = "Domap综合工具"; //localizable text 
      base.m_caption = "道路挂接检查";  //localizable text 
      base.m_message = "道路挂接检查";  //localizable text
      base.m_toolTip = "道路挂接检查";  //localizable text
      base.m_name = "DomapTool.RoadConnectTool";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
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

      roadLayer = null;
      sfs = new SimpleFillSymbolClass();
      SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
      RgbColorClass rgb = new RgbColorClass();
      rgb.Red = 0;
      rgb.Green = 0;
      rgb.Blue = 255;
      sls.Color = rgb;
      sls.Width = 2;
      sfs.Outline = sls;
      sfs.Style = esriSimpleFillStyle.esriSFSNull;

      sms = new SimpleMarkerSymbolClass();
      sms.OutlineColor = rgb;
      sms.Style = esriSimpleMarkerStyle.esriSMSCircle;
      sms.OutlineSize = 2;
      sms.Size = 15;
      sms.Outline = true;
      rgb.NullColor = true;
      sms.Color = rgb;

    }

    #region Overriden Class Methods

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


    public override bool Enabled {
      get {
        return base.Enabled && editor.EditState == esriEditState.esriStateEditing;
      }
    }
    /// <summary>
    /// Occurs when this tool is clicked
    /// </summary>
    IActiveView ac = null;
    public override void OnClick() {
        
        ac = (m_application.Document as IMxDocument).ActiveView;
        IActiveViewEvents_Event events = ac as IActiveViewEvents_Event;
        events.AfterDraw += new IActiveViewEvents_AfterDrawEventHandler(events_AfterDraw);
      if (gb != null && gb.Count > 0) {
        if (System.Windows.Forms.MessageBox.Show(
                "已经计算过连通关系，是否重新计算？", 
                "提示", 
                System.Windows.Forms.MessageBoxButtons.YesNo) 
                == System.Windows.Forms.DialogResult.No) 
        {
          (m_application.Document as IMxDocument).ActiveView.Refresh();
          return;
        }
      }
      Analysis();
      (m_application.Document as IMxDocument).ActiveView.Refresh();
    }
    public override bool Deactivate()
    {
        IActiveViewEvents_Event events = ac as IActiveViewEvents_Event;
        events.AfterDraw -= new IActiveViewEvents_AfterDrawEventHandler(events_AfterDraw);
        return true;
    }
    public override void OnMouseDown(int Button, int Shift, int X, int Y) {
      if (Button == 4)
        return;
      IActiveView activeView = (m_application.Document as IMxDocument).ActiveView;
      IPoint p = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
      double sDis = activeView.ScreenDisplay.DisplayTransformation.FromPoints(5);
      double distance = 0;
      int part = -1;
      int seg = -1;
      bool rightSide = false;
      IPoint hitPoint = new PointClass();
      for (int i = 0; i < gb.GeometryCount; i++) {
        IHitTest hit = gb.get_Geometry(i).Envelope as IHitTest;
        if (
          hit.HitTest(p, sDis, 
                      esriGeometryHitPartType.esriGeometryPartVertex
                      /* | esriGeometryHitPartType.esriGeometryPartCentroid*/, 
                      hitPoint, ref distance, ref  part, ref seg, ref rightSide)) {
          try {
            AutoProcess(i, Button == 1);
            //Next();
            (m_application.Document as IMxDocument).ActiveView.Refresh();
          }
          catch {
          }
          break;
        }
      }
    }

    public override void OnKeyUp(int keyCode, int Shift) {
      switch (keyCode) {
        case (int)System.Windows.Forms.Keys.Space:
          ProscessAll(true);
          (m_application.Document as IMxDocument).ActiveView.Refresh();
          break;
        case (int)System.Windows.Forms.Keys.Enter:
          ProscessAll(false);
          (m_application.Document as IMxDocument).ActiveView.Refresh();
          break;
        case (int)System.Windows.Forms.Keys.R:
          Analysis();
          (m_application.Document as IMxDocument).ActiveView.Refresh();
          break;
        case (int)System.Windows.Forms.Keys.N:
          Next();
          break;
      }
    }

    //public override void Refresh(int hDC) {
    //  IDisplay dis = new SimpleDisplayClass();
    //  dis.StartDrawing(hDC, 0);
    //  if (gb != null) {
    //    bool drawEnv = dis.DisplayTransformation.FromPoints(15) < 8;
    //    if (drawEnv) {
    //      dis.SetSymbol(sfs);
    //      for (int i = 0; i < gb.GeometryCount; i++) {
    //        IEnvelope env = gb.get_Geometry(i).Envelope;
    //        env.Expand(4, 4, false);
    //        dis.DrawRectangle(env);
    //      }
    //    }


    //    dis.SetSymbol(sms);
    //    for (int i = 0; i < gb.GeometryCount; i++) {
    //      dis.DrawMultipoint(gb.get_Geometry(i));
    //    }

    //  }
    //  dis.FinishDrawing();

    //}

    void events_AfterDraw(ESRI.ArcGIS.Display.IDisplay dis, esriViewDrawPhase phase)
    {
        if (gb != null)
        {
            bool drawEnv = dis.DisplayTransformation.FromPoints(15) < 8;
            if (true)
            {
                dis.SetSymbol(sfs);
                for (int i = 0; i < gb.GeometryCount; i++)
                {
                    IEnvelope env = gb.get_Geometry(i).Envelope;
                    env.Expand(4, 4, false);
                    dis.DrawRectangle(env);
                }
            }
            dis.SetSymbol(sms);
            for (int i = 0; i < gb.GeometryCount; i++)
            {
                dis.DrawMultipoint(gb.get_Geometry(i));
            }
        }
    }


    #endregion

    #region Function For Check And Connect
    void FindNode(ITinNode seed, Dictionary<int, IPoint> nodes, double distance) {
      ITinEdgeArray edges = seed.GetIncidentEdges();
      ITinEdit tin = seed.TheTin as ITinEdit;
      tin.SetNodeTagValue(seed.Index, -1);
      PointClass p = new PointClass();
      p.X = seed.X;
      p.Y = seed.Y;
      nodes.Add(seed.Index, p);
      for (int i = 0; i < edges.Count; i++) {
        ITinEdge edge = edges.get_Element(i);
        if (edge.Length < distance) {
          if (edge.ToNode.IsInsideDataArea && edge.ToNode.TagValue != -1) {
            FindNode(edge.ToNode, nodes, distance);
          }
        }
      }
    }

    TinClass tin;
    Dictionary<int, List<int>> nID_fID_dic;
    List<Dictionary<int, IPoint>> groups;
    GeometryBagClass gb;
    int currentErr;
    void Analysis() {
      IMap map = (m_application.Document as IMxDocument).ActiveView.FocusMap;
      RoadConnectConfigDlg dlg = new RoadConnectConfigDlg(map);
      
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      roadLayer = dlg.SelectLayer;
      double dis = dlg.Distance;
      //double dis = (double)m_application.GenPara["道路挂接距离"];
      //WaitOperation wo = m_application.SetBusy(true);
      //wo.SetText("正在进行分析准备……");
      tin = new TinClass();
      tin.InitNew(roadLayer.AreaOfInterest);
      tin.StartInMemoryEditing();

      int featureCount = (roadLayer).FeatureClass.FeatureCount(null);
      IFeatureCursor fCursor = (roadLayer).Search(null, true);
      IFeature feature = null;
      IPoint p = new PointClass();
      p.Z = 0;
      ITinNode node = new TinNodeClass();
      nID_fID_dic = new Dictionary<int, List<int>>();
      while ((feature = fCursor.NextFeature()) != null) {
        //wo.SetText("正在进行分析准备:" + feature.OID.ToString());
        //wo.Step(featureCount);
        IPolyline line = feature.Shape as IPolyline;
        p.X = line.FromPoint.X;
        p.Y = line.FromPoint.Y;
        tin.AddPointZ(p, 1, node);
        if (!nID_fID_dic.ContainsKey(node.Index)) {
          nID_fID_dic[node.Index] = new List<int>();
        }
        nID_fID_dic[node.Index].Add(feature.OID);
        p.X = line.ToPoint.X;
        p.Y = line.ToPoint.Y;
        tin.AddPointZ(p, 1, node);
        if (!nID_fID_dic.ContainsKey(node.Index)) {
          nID_fID_dic[node.Index] = new List<int>();
        }
        nID_fID_dic[node.Index].Add(-feature.OID);
      }
      //wo.Step(0);
      groups = new List<Dictionary<int, IPoint>>();
      for (int i = 1; i <= tin.NodeCount; i++) {
        //wo.SetText("正在分析:" + i.ToString());
        //wo.Step(tin.NodeCount);

        ITinNode n = tin.GetNode(i);
        if (n.TagValue != -1 && n.IsInsideDataArea) {
          Dictionary<int, IPoint> g = new Dictionary<int, IPoint>();
          FindNode(n, g, dis);
          if (g.Count > 1) {
            groups.Add(g);
          }
        }
      }
      gb = new GeometryBagClass();
      object miss = Type.Missing;
      //wo.Step(0);
      //wo.SetText("正在整理分析结果");

      foreach (Dictionary<int, IPoint> g in groups) {
        //wo.Step(groups.Count);

        MultipointClass mp = new MultipointClass();
        //centerPoint.X = 0;
        //centerPoint.Y = 0;
        //int count = 0;

        foreach (int nid in g.Keys) {
          IPoint pend = g[nid];
          mp.AddGeometry(pend, ref miss, ref miss);
          //centerPoint.X += pend.X;
          //centerPoint.Y += pend.Y;
          //count++;
        }
        //centerPoint.X = centerPoint.X / count;
        //centerPoint.Y = centerPoint.Y / count;
        gb.AddGeometry(mp, ref miss, ref miss);
      }
      currentErr = groups.Count;
      //m_application.SetBusy(false);
      System.Windows.Forms.MessageBox.Show("检查完成，共" + groups.Count + "个错误");
    }

    void AutoProcess(int index, bool commit) {
      if (commit) {
        Dictionary<int, IPoint> group = groups[index];
        IFeatureClass fc = ((roadLayer) as IFeatureLayer).FeatureClass;
        IGeometry geo = gb.get_Geometry(index);
        IEnvelope env = geo.Envelope;
        IPoint p = (env as IArea).Centroid;
        editor.StartOperation();
        foreach (var item in group.Keys) {
          List<int> fids = nID_fID_dic[item];
          foreach (var fid in fids) {
            IFeature feature = fc.GetFeature(Math.Abs(fid));
            IPolyline line = feature.ShapeCopy as IPolyline;
            if (fid > 0) {
              line.FromPoint = p;
            }
            else {
              line.ToPoint = p;
            }
            feature.Shape = line;
            feature.Store();
          }
        }
        editor.StopOperation("道路挂接");
      }
      groups.RemoveAt(index);
      gb.RemoveGeometries(index, 1);
    }
    void ProscessAll(bool commit) {
      //WaitOperation wo = m_application.SetBusy(true);
      try {
        int count = gb.GeometryCount;
        for (int i = gb.GeometryCount - 1; i >= 0; i--) {
          AutoProcess(i, commit);
          //wo.Step(count);
        }
      }
      catch {
      }
      //m_application.SetBusy(false);
    }
    void Next() {
      if (groups == null || groups.Count == 0)
        return;
      currentErr--;
      if (currentErr < 0) {
        currentErr = groups.Count - 1;
      }
      if (currentErr > groups.Count - 1) {
        currentErr = 0;
      }
      IEnvelope env = gb.get_Geometry(currentErr).Envelope;
      env.Expand(8, 8, false);
      (m_application.Document as IMxDocument).ActiveView.Extent = env;
      (m_application.Document as IMxDocument).ActiveView.Refresh();
    }

    #endregion
  }
}

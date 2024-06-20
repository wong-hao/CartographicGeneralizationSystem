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
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesFile;

namespace DomapTool {
  /// <summary>
  /// Summary description for BuildingB.
  /// </summary>
  [Guid("fbfecf88-566a-4c3d-9e91-0ad10ba955b7")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DomapTool.BuildingB")]
  public sealed class BuildingB : BaseTool {
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
    INewPolygonFeedback fb;
    INewLineFeedback fb_line;
    int fieldID;
    int maxValue;
    IDataStatistics statistics;
    IEditor editor;
    IFeatureClass fc = null;
    IFeatureLayer buildLayer = null;
    ESRI.ArcGIS.Geoprocessor.Geoprocessor Geoprosessor;

    private IApplication m_application;
    public BuildingB() {
      //
      // TODO: Define values for the public properties
      //
      statistics = new DataStatisticsClass();
      Geoprosessor = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
      this.Geoprosessor.AddOutputsToMap = false;
      base.m_category = "Domap"; //localizable text 
      base.m_caption = "分组";  //localizable text 
      base.m_message = "建筑物分组";  //localizable text
      base.m_toolTip = "建筑物分组";  //localizable text
      base.m_name = "DomapTool.BuildingB";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
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
    public override void OnClick() {
      ac = (m_application.Document as IMxDocument).ActiveView;
      DeleteMinHole d = new DeleteMinHole((m_application.Document as IMxDocument).ActiveView.FocusMap);
      d.Text = "分组";
      d.label2.Text = "最小洞面积";
      d.label3.Text = "融合距离";
      d.textBox1.Text = GGenPara.Para["建筑物最小上图面积"].ToString();
      d.textBox2.Text = GGenPara.Para["建筑物合并距离"].ToString();
      if (d.ShowDialog() == DialogResult.Cancel)
        return;
      buildLayer = d.SelectLayer;
      area = Convert.ToDouble(d.textBox1.Text);
      GGenPara.Para["建筑物最小上图面积"] = area;
      dis = Convert.ToDouble(d.textBox2.Text);
      GGenPara.Para["建筑物合并距离"] = dis;
      GGenPara.SavePara();
      fc = buildLayer.FeatureClass;
      if (buildLayer == null) {
        MessageBox.Show("请确保建筑物图层不为空!");
        return;
      }
      fieldID = CheckField(buildLayer);
      ac.Refresh();
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
      if (index == -1)
        return -1;
      (blayer as IGeoFeatureLayer).DisplayAnnotation = true;
      ESRI.ArcGIS.Carto.IAnnotateLayerPropertiesCollection alp = (blayer as IGeoFeatureLayer).AnnotationProperties;
      alp.Clear();
      ESRI.ArcGIS.Carto.ILabelEngineLayerProperties lelp = new ESRI.ArcGIS.Carto.LabelEngineLayerPropertiesClass();
      lelp.Expression = "[G_Group]";
      lelp.BasicOverposterLayerProperties.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
      alp.Add(lelp as ESRI.ArcGIS.Carto.IAnnotateLayerProperties);
      return index;
    }
    public override void OnMouseDown(int Button, int Shift, int X, int Y) {
      if (Button == 1) {
        IPoint p = ac.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
        if (Shift == 1 || fb_line != null) {
          if (false) {
            if (fb_line == null) {
              fb_line = new NewLineFeedbackClass();
              fb_line.Display = ac.ScreenDisplay;
              fb_line.Start(p);
            }
            else {
              editor.StartOperation();
              fb_line.AddPoint(p);
              IPolyline line = fb_line.Stop();
              fb_line = null;
              IPoint fromPoint = line.FromPoint;
              ISpatialFilter sf = new SpatialFilterClass();
              sf.Geometry = fromPoint;
              sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
              IFeatureCursor fCursor = buildLayer.FeatureClass.Update(sf, true);
              IFeature feature = fCursor.NextFeature();
              if (feature == null)
                return;
              sf.Geometry = line.ToPoint;
              IFeatureCursor cursor = buildLayer.FeatureClass.Update(sf, true);
              //IFeatureCursor cursor = buildLayer.FeatureClass.Search(sf, true);
              IFeature oFe = cursor.NextFeature();
              object value = 0;
              if (oFe != null) {
                value = oFe.get_Value(fieldID);
              }
              feature.set_Value(fieldID, value);
              //feature.Store();
              fCursor.UpdateFeature(feature);
              editor.StopOperation("分组2");
              fCursor.Flush();
              System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
              System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
              ac.Refresh();

            }
          }
        }
        else {
          if (fb == null) {
            fb = new NewPolygonFeedbackClass();
            fb.Display = ac.ScreenDisplay;
            fb.Start(p);
          }
          else {
            fb.AddPoint(p);
          }
        }
      }
      if (Button == 2) {
        try {
          if (System.Windows.Forms.MessageBox.Show("将进行合并操作，请确认", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK) {
            GroupLayer();
          }
        }
        catch (Exception ex) {
        }
      }
    }

    public override void OnMouseMove(int Button, int Shift, int X, int Y) {
      IPoint p = ac.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

      if (fb != null) {
        fb.MoveTo(p);
      }
      if (fb_line != null) {
        fb_line.MoveTo(p);
      }
    }
    private bool IsSetDone;
    public override void OnDblClick() {
      if (fb == null) {
        return;
      }

      IPolygon polygon = fb.Stop();
      fb = null;

      if (buildLayer == null) {
        System.Windows.Forms.MessageBox.Show("没有建筑物图层");
        return;
      }
      editor.StartOperation();
      ISpatialFilter filter = new SpatialFilterClass();
      filter.Geometry = polygon;
      filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
      IFeatureCursor FCursor = buildLayer.FeatureClass.Update(filter, true);
      IFeature feature = null;
      int v = -1;
      if (!IsSetDone) {
        maxValue++;
        v = maxValue;
      }
      v = v % 100000;
      while ((feature = FCursor.NextFeature()) != null) {
        feature.set_Value(fieldID, v);
        FCursor.UpdateFeature(feature);
      }
      editor.StopOperation("分组");
      FCursor.Flush();
      System.Runtime.InteropServices.Marshal.ReleaseComObject(FCursor);
      ac.Refresh();
    }

    public override void OnKeyDown(int keyCode, int Shift) {
      switch (keyCode) {
        case (int)System.Windows.Forms.Keys.Escape:
          if (fb != null) {
            fb.Stop();
            fb = null;
          }
          else if (fb_line != null) {
            fb_line.Stop();
            fb_line = null;
          }
          else if (MessageBox.Show("将清空所有组，是否继续？", "注意", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
            editor.StartOperation();
            ClearGroup(buildLayer.FeatureClass, fieldID, false);
            editor.StopOperation("清空分组");
            ac.Refresh();
          }
          break;
        case (int)System.Windows.Forms.Keys.Space:
          //GroupLayer();
          break;
        case (int)System.Windows.Forms.Keys.A:
          IsSetDone = !IsSetDone;
          break;
        case (int)System.Windows.Forms.Keys.C:
          if (MessageBox.Show("即将把所有编组号变为0，是否继续？", "注意", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
            editor.StartOperation();
            ClearGroup(buildLayer.FeatureClass, fieldID, true);
            editor.StopOperation("清空分组");
            ac.Refresh();
          }
          break;
      }
    }

    public override void OnMouseUp(int Button, int Shift, int X, int Y) {
      // TODO:  Add BuildingB.OnMouseUp implementation
    }
    #endregion
    private void ClearGroup(IFeatureClass fc, int index, bool ContainsNegative) {
      IFeatureCursor cursor = fc.Update(null, true);
      IFeature f = null;
      while ((f = cursor.NextFeature()) != null) {
        object v = f.get_Value(index);
        try {
          if (!ContainsNegative && Convert.ToInt32(v) <= 0)
            continue;
        }
        catch {
        }
        f.set_Value(index, 0);
        cursor.UpdateFeature(f);
      }
      cursor.Flush();
      System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
    }

    private void GroupLayer() {
      try {
        //string tempPath = System.Environment.GetFolderPath(Environment.SpecialFolder.System);
        string tempPath = System.Environment.GetEnvironmentVariable("TEMP");
        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(tempPath);
        tempPath = info.FullName;
        IWorkspaceFactory shpfileWSF = new ShapefileWorkspaceFactoryClass();
        IWorkspace ws = shpfileWSF.OpenFromFile(tempPath, 0);


        int wCount = 0;
        IStatusBar sBar = m_application.StatusBar;
        IStepProgressor sPro = sBar.ProgressBar;
        sPro.Position = 0;

        IFeatureClass fc_org = buildLayer.FeatureClass;
        List<GroupInfo> groups = new List<GroupInfo>();
        Dictionary<int, GroupInfo> group_dic_groupid_info = new Dictionary<int, GroupInfo>();
        Dictionary<int, GroupInfo> group_dic_tmpfid_info = new Dictionary<int, GroupInfo>();
        MultipointClass mp = new MultipointClass();
        object miss = Type.Missing;
        IQueryFilter qfAll = new QueryFilterClass();
        qfAll.WhereClause = "G_Group > 0";
        IFeatureCursor orgGroupCursor = fc_org.Search(qfAll, true);
        wCount = fc_org.FeatureCount(qfAll);
        sBar.ShowProgressBar("正在提取已经分组的建筑物...", 0, wCount, 1, true);
        IFeature feature = null;
        while ((feature = orgGroupCursor.NextFeature()) != null) {
          sBar.StepProgressBar();
          int groupid = Convert.ToInt32(feature.get_Value(fieldID));
          if (!group_dic_groupid_info.ContainsKey(groupid)) {
            group_dic_groupid_info.Add(groupid, new GroupInfo(groupid));
            group_dic_groupid_info[groupid].Center = (feature.Shape as IArea).Centroid;
            groups.Add(group_dic_groupid_info[groupid]);
            mp.AddPoint(group_dic_groupid_info[groupid].Center, ref miss, ref miss);
          }
          group_dic_groupid_info[groupid].IDS_org_tmp.Add(feature.OID, -1);
        }
        if (mp.PointCount == 0) {
          sBar.HideProgressBar();
          return;
        }
        sBar.ShowProgressBar("正在进行数据准备...", 0, wCount, 1, true);
        IPoint centerPoint = (mp.Envelope as IArea).Centroid;
        ITransform2D mp_scaled = mp.Clone() as ITransform2D;
        mp_scaled.Scale(centerPoint, 200, 200);
        IPointCollection mpc_scaled = mp_scaled as IPointCollection;
        for (int i = 0; i < mp.PointCount; i++) {
          IPoint bp = mp.get_Point(i);
          IPoint fp = mpc_scaled.get_Point(i);
          groups[i].dx = fp.X - bp.X;
          groups[i].dy = fp.Y - bp.Y;
        }
        sPro.Position = 0;
        IFeatureClass fc_tmp = CreateLayer(ws);
        foreach (GroupInfo g in groups) {
          foreach (int oid in g.IDS_org_tmp.Keys) {
            sBar.StepProgressBar();
            IFeature f = fc_org.GetFeature(oid);
            ITransform2D geo = f.ShapeCopy as ITransform2D;
            geo.Move(g.dx, g.dy);
            IFeature nf = fc_tmp.CreateFeature();
            nf.Shape = geo as IGeometry;
            nf.Store();
            g.IDS_tmp_org.Add(nf.OID, oid);
            group_dic_tmpfid_info.Add(nf.OID, g);
          }
        }
        sBar.HideProgressBar();
        sBar.ShowProgressBar("正在进行合并操作...", 0, 1, 1, true);
        sPro.Position = 1;
        AggregatePolygons ap = new AggregatePolygons();
        ap.in_features = fc_tmp;
        //ap.out_feature_class = m_application.Workspace.Workspace.PathName + "\\" + m_application.Workspace.LayerManager.TempLayerName();
        int mm = 1;
        //string tempPath=System.Environment.GetFolderPath(Environment.SpecialFolder.System);
        string finalFile = "tp" + mm;
        while (System.IO.File.Exists(tempPath + "\\" + finalFile + ".shp")) {
          mm++;
          finalFile = "tp" + mm;
        }
        ap.out_feature_class = tempPath + "\\" + finalFile;
        //ap.out_table ="c:\\temp\\" + layer.Name + "_tbl";
        ap.aggregation_distance = dis;
        ap.minimum_hole_size = area;
        ap.orthogonality_option = "ORTHOGONAL";
        Geoprosessor.AddOutputsToMap = false;
        Geoprosessor.Execute(ap, null);
        IFeatureClass fc_merge = (ws as IFeatureWorkspace).OpenFeatureClass(System.IO.Path.GetFileName(ap.out_feature_class.ToString()));
        ITable table_merge = (ws as IFeatureWorkspace).OpenTable(System.IO.Path.GetFileName(ap.out_table.ToString()));

        Dictionary<int, List<int>> merge_dic_out_in = new Dictionary<int, List<int>>();
        Dictionary<int, int> merge_dic_in_out = new Dictionary<int, int>();
        int input_index = table_merge.FindField("INPUT_FID");
        int output_index = table_merge.FindField("OUTPUT_FID");
        int rowCount = table_merge.RowCount(null);
        sBar.HideProgressBar();
        sBar.ShowProgressBar("正在整理合并结果...", 0, rowCount, 1, true);
        sPro.Position = 0;
        ICursor cursor_table = table_merge.Search(null, true);
        IRow row_table = null;
        while ((row_table = cursor_table.NextRow()) != null) {
          //wo.Step(rowCount);
          sBar.StepProgressBar();
          int input_FID = Convert.ToInt32(row_table.get_Value(input_index));
          int output_FID = Convert.ToInt32(row_table.get_Value(output_index));
          if (!merge_dic_out_in.ContainsKey(output_FID)) {
            merge_dic_out_in.Add(output_FID, new List<int>());
          }
          merge_dic_out_in[output_FID].Add(input_FID);
          if (!merge_dic_in_out.ContainsKey(input_FID)) {
            merge_dic_in_out.Add(input_FID, output_FID);
          }
          else {
          }
        }

        wCount = fc_merge.FeatureCount(null);
        sBar.HideProgressBar();
        sBar.ShowProgressBar("正在保存合并结果...", 0, wCount, 1, true);
        sPro.Position = 0;
        editor.StartOperation();
        IFeatureCursor outCursor = fc_merge.Search(null, true);
        IFeature outFeature = null;
        IGeometry mergePolygon = null;
        double miniArea = area;
        while ((outFeature = outCursor.NextFeature()) != null) {
          sBar.StepProgressBar();
          try {
            ITransform2D shape = outFeature.ShapeCopy as ITransform2D;
            int oid = outFeature.OID;
            List<int> tmp_OIDs = merge_dic_out_in[oid];
            GroupInfo gInfo = group_dic_tmpfid_info[tmp_OIDs[0]];
            int org_OID = gInfo.IDS_tmp_org[tmp_OIDs[0]];
            shape.Move(-gInfo.dx, -gInfo.dy);

            IFeature usedFeature = null;
            foreach (int tmp_OID in tmp_OIDs) {
              int orgFeatureOID = gInfo.IDS_tmp_org[tmp_OID];
              IFeature orgFeature = fc_org.GetFeature(orgFeatureOID);
              if (usedFeature == null) {
                usedFeature = orgFeature;
              }
              else if ((usedFeature.Shape as IArea).Area < (orgFeature.Shape as IArea).Area) {
                usedFeature = orgFeature;
              }
            }

            (shape as ITopologicalOperator).Simplify();
            IGeometryCollection gc = (shape as IPolygon4).ConnectedComponentBag as IGeometryCollection;
            for (int shapeid = 0; shapeid < gc.GeometryCount; shapeid++) {
              IGeometry shapeToAdd = gc.get_Geometry(shapeid);
              if (shapeToAdd == null || shapeToAdd.IsEmpty)
                continue;

              IFeature addFeature = fc_org.CreateFeature();
              addFeature.Shape = shape as IGeometry;
              for (int fieldid = 0; fieldid < addFeature.Fields.FieldCount; fieldid++) {
                IField field = addFeature.Fields.get_Field(fieldid);
                if (field.Editable && field.Type != esriFieldType.esriFieldTypeGeometry) {
                  if (fieldid == fieldID) {
                    addFeature.set_Value(fieldid, -1);
                  }
                  else {
                    addFeature.set_Value(fieldid, usedFeature.get_Value(fieldid));
                  }
                }
              }
              addFeature.Store();

            }
          }
          catch (Exception ex) {
            continue;
          }

        }
        sBar.HideProgressBar();
        wCount = fc_org.FeatureCount(qfAll);
        sBar.ShowProgressBar("正在清理临时数据...", 0, 1, 1, true);
        sPro.Position = 1;
        IFeatureCursor delectCursor = fc_org.Update(qfAll, true);
        while (delectCursor.NextFeature() != null) {
          delectCursor.DeleteFeature();
        }
        editor.StopOperation("合并");
        delectCursor.Flush();
        sBar.HideProgressBar();
      }
      catch (Exception ex) {

      }
      finally {
        editor.StopOperation("合并");
        //m_application.SetBusy(false);
      }
      ac.Refresh();
    }
    internal class GroupInfo {
      internal int GroupID;
      internal Dictionary<int, int> IDS_org_tmp;
      internal Dictionary<int, int> IDS_tmp_org;
      internal double dx;
      internal double dy;
      internal IPoint Center;
      internal GroupInfo(int groupID) {
        this.GroupID = groupID;
        IDS_org_tmp = new Dictionary<int, int>();
        IDS_tmp_org = new Dictionary<int, int>();
      }
    }


    private IFeatureClass CreateLayer(IWorkspace ss) {
      List<IField> fs = new List<IField>();
      IFieldEdit2 field = new FieldClass();
      field.Name_2 = "GroupID";
      field.Type_2 = esriFieldType.esriFieldTypeInteger;
      fs.Add(field as IField);
      field = new FieldClass();
      field.Name_2 = "OrgID";
      field.Type_2 = esriFieldType.esriFieldTypeInteger;
      fs.Add(field as IField);
      IFeatureWorkspace fws = ss as IFeatureWorkspace;
      //string name = TempLayerName();
      int mm = 1;
      string name = "tp" + mm;
      //while ((ss as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name) || (ss as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, name))
      while (System.IO.File.Exists(ss.PathName + "\\" + name + ".shp")) {
        mm++;
        name = "tp" + mm;
      }
      IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
      IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
      IFields rfields = ocDescription.RequiredFields;
      if (fs != null)
        foreach (IField f in fs) {
          (rfields as IFieldsEdit).AddField(f);
        }
      try {
        return fws.CreateFeatureClass(name, rfields,
            ocDescription.InstanceCLSID, ocDescription.ClassExtensionCLSID,
            fcDescription.FeatureType, fcDescription.ShapeFieldName, "");
      }
      catch {
        return null;
      }

    }
  }
}

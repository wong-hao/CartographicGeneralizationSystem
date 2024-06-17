using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace DatabaseUpdate
{
  /// <summary>
  /// Summary description for FindFeatureBySelectedFeature.
  /// </summary>
  [Guid("cd2eb26c-204b-4ad8-ab87-0304236bc183")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DatabaseUpdate.FindFeatureBySelectedFeature")]
  public sealed class FindFeatureBySelectedFeature : BaseCommand
  {
    #region COM Registration Function(s)
    [ComRegisterFunction()]
    [ComVisible(false)]
    static void RegisterFunction(Type registerType)
    {
      // Required for ArcGIS Component Category Registrar support
      ArcGISCategoryRegistration(registerType);

      //
      // TODO: Add any COM registration code here
      //
    }

    [ComUnregisterFunction()]
    [ComVisible(false)]
    static void UnregisterFunction(Type registerType)
    {
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
    private static void ArcGISCategoryRegistration(Type registerType)
    {
      string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
      MxCommands.Register(regKey);

    }
    /// <summary>
    /// Required method for ArcGIS Component Category unregistration -
    /// Do not modify the contents of this method with the code editor.
    /// </summary>
    private static void ArcGISCategoryUnregistration(Type registerType)
    {
      string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
      MxCommands.Unregister(regKey);

    }

    #endregion
    #endregion

    private IApplication m_application;
    public FindFeatureBySelectedFeature()
    {
      //
      // TODO: Define values for the public properties
      //
      base.m_category = "Domap"; //localizable text
      base.m_caption = "查找影响建筑物";  //localizable text
      base.m_message = "查找影响到的建筑物并选中";  //localizable text 
      base.m_toolTip = "查找本图层被选中的在小比例中影响到的建筑物并在本图层中选中";  //localizable text 
      base.m_name = "DatabaseUpdate.FindFeatureBySelectedFeature";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

      try
      {
        //
        // TODO: change bitmap name if necessary
        //
        string bitmapResourceName = GetType().Name + ".bmp";
        base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
      }
    }

    #region Overriden Class Methods

    public override bool Enabled
    {
      get
      {
        return base.Enabled && UApplication.Application != null
          && UApplication.Application.Workspace != null
          && UApplication.Application.MxMap.SelectionCount > 0;
      }
    }

    /// <summary>
    /// Occurs when this command is created
    /// </summary>
    /// <param name="hook">Instance of the application</param>
    public override void OnCreate(object hook)
    {
      if (hook == null)
        return;

      m_application = hook as IApplication;

      //Disable if it is not ArcMap
      if (hook is IMxApplication)
        base.m_enabled = true;
      else
        base.m_enabled = false;

      // TODO:  Add other initialization code
    }

    /// <summary>
    /// Occurs when this command is clicked
    /// </summary>
    public override void OnClick()
    {

      object miss = Type.Missing;   
      // TODO: Add FindFeatureBySelectedFeature.OnClick implementation
      #region 1 获取图层 mxLayer,mcLayer
      DoUpdate dlg = new DoUpdate(UApplication.Application.MxMap);
      dlg.Text = "变化图层";
      if (dlg.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      IFeatureLayer mxLayer = dlg.SelectLayer;

      string name = mxLayer.Name;
      ULayerInfos mcInfo = UApplication.Application.Workspace.McLayers;
      if (!mcInfo.Layers.ContainsKey(name))
      {
        MessageBox.Show("未找到数据库中对应图层");
        return;
      }
      IFeatureLayer mcLayer = mcInfo.Layers[name];
      if (mxLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon
        || mcLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon)
      {
        MessageBox.Show("本工具只对面图层有效");
        return;
      }
      #endregion

      #region 2 获取mx中选中的目标 geometry
      IGeometry geometry = null;
      {
        IFeatureSelection mxSelection = mxLayer as IFeatureSelection;
        if (mxSelection.SelectionSet.Count <= 0)
        {
          MessageBox.Show("没有选中目标");
          return;
        }
        GeometryBagClass bag = new GeometryBagClass();
        ITopologicalOperator tp = null;
        ISelectionSet sset = mxSelection.SelectionSet;
        IEnumIDs ids = sset.IDs;
        int id = -1;     
        while ((id = ids.Next()) != -1)
        {
          IGeometry geo = mxLayer.FeatureClass.GetFeature(id).ShapeCopy;
          bag.AddGeometry(geo, ref miss, ref miss);
          if (tp == null)
          {
            tp = mxLayer.FeatureClass.GetFeature(id).ShapeCopy as ITopologicalOperator;
          }
        }
        tp.ConstructUnion(bag);
        geometry = tp as IGeometry;
      }
      #endregion

      #region 3 查找mc中相关的目标并选中 geometry
      {
        ISpatialFilter sf = new SpatialFilterClass();
        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelRelation;
        sf.SpatialRelDescription = "T********";
        sf.Geometry = geometry as IGeometry;
        IFeatureCursor fc = mcLayer.FeatureClass.Search(sf, false);
        IFeature fe = null;
        GeometryBagClass bag = new GeometryBagClass();
        ITopologicalOperator tp = null;
        IFeatureSelection mcSelection = mcLayer as IFeatureSelection;
        mcSelection.Clear();
        while ((fe = fc.NextFeature()) != null)
        {
          IGeometry geo = fe.ShapeCopy;
          bag.AddGeometry(geo, ref miss, ref miss);
          if (tp == null)
          {
            tp = fe.ShapeCopy as ITopologicalOperator;
          }
          mcSelection.Add(fe);
        }
        if (tp == null)
        {
          MessageBox.Show("没有找到影响要素");
          return;
        }
        
        tp.ConstructUnion(bag);
        geometry = tp as IGeometry;
      }
      #endregion

      #region 4 反选mx中的相关目标
      {
        ISpatialFilter sf = new SpatialFilterClass();
        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelRelation;
        sf.SpatialRelDescription = "T********";
        sf.Geometry = geometry as IGeometry;
        IFeatureCursor fc = mxLayer.FeatureClass.Search(sf, false);
        IFeature fe = null;
        IFeatureSelection mxSelection = mxLayer as IFeatureSelection;
        mxSelection.Clear();
        while ((fe = fc.NextFeature()) != null)
        {
          ITopologicalOperator geo = fe.Shape as ITopologicalOperator;
          IArea a = geo.Intersect(geometry, esriGeometryDimension.esriGeometry2Dimension) as IArea;
          if (a == null)
            continue;
          double la = a.Area;
          double ba = (geo as IArea).Area;
          if (la * 20.0 < ba)
          {
            continue;
          }
          mxSelection.Add(fe);
        }
      }
      (UApplication.Application.McMap as IActiveView).Refresh();
      (UApplication.Application.MxMap as IActiveView).Refresh();
      #endregion
    }

    #endregion
  }
}

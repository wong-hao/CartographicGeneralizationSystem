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
      base.m_caption = "����Ӱ�콨����";  //localizable text
      base.m_message = "����Ӱ�쵽�Ľ����ﲢѡ��";  //localizable text 
      base.m_toolTip = "���ұ�ͼ�㱻ѡ�е���С������Ӱ�쵽�Ľ����ﲢ�ڱ�ͼ����ѡ��";  //localizable text 
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
      #region 1 ��ȡͼ�� mxLayer,mcLayer
      DoUpdate dlg = new DoUpdate(UApplication.Application.MxMap);
      dlg.Text = "�仯ͼ��";
      if (dlg.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      IFeatureLayer mxLayer = dlg.SelectLayer;

      string name = mxLayer.Name;
      ULayerInfos mcInfo = UApplication.Application.Workspace.McLayers;
      if (!mcInfo.Layers.ContainsKey(name))
      {
        MessageBox.Show("δ�ҵ����ݿ��ж�Ӧͼ��");
        return;
      }
      IFeatureLayer mcLayer = mcInfo.Layers[name];
      if (mxLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon
        || mcLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon)
      {
        MessageBox.Show("������ֻ����ͼ����Ч");
        return;
      }
      #endregion

      #region 2 ��ȡmx��ѡ�е�Ŀ�� geometry
      IGeometry geometry = null;
      {
        IFeatureSelection mxSelection = mxLayer as IFeatureSelection;
        if (mxSelection.SelectionSet.Count <= 0)
        {
          MessageBox.Show("û��ѡ��Ŀ��");
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

      #region 3 ����mc����ص�Ŀ�겢ѡ�� geometry
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
          MessageBox.Show("û���ҵ�Ӱ��Ҫ��");
          return;
        }
        
        tp.ConstructUnion(bag);
        geometry = tp as IGeometry;
      }
      #endregion

      #region 4 ��ѡmx�е����Ŀ��
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

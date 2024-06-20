using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace DomapTool {
  /// <summary>
  /// Summary description for LineSimplify.
  /// </summary>
  [Guid("b9b755e1-e7f5-4bd9-9e2e-4e85403d6c95")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DomapTool.LineSimplify")]
  public sealed class LineSimplify : BaseCommand {
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
    IEditor editor;
    public LineSimplify() {
      //
      // TODO: Define values for the public properties
      //
      base.m_category = "Domap"; //localizable text
      base.m_caption = "曲线化简";  //localizable text
      base.m_message = "对选中曲线进行化简";  //localizable text 
      base.m_toolTip = "对选中曲线进行化简";  //localizable text 
      base.m_name = "DomapTool.LineSimplify";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")
      GGenPara.Para.RegistPara("曲线化简容差", (double)5);
      try {
        //
        // TODO: change bitmap name if necessary
        //
        string bitmapResourceName = GetType().Name + ".bmp";
        base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
      }
      catch (Exception ex) {
        System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
      }
    }

    #region Overriden Class Methods

    /// <summary>
    /// Occurs when this command is created
    /// </summary>
    /// <param name="hook">Instance of the application</param>
    public override void OnCreate(object hook) {
      if (hook == null)
        return;

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
    /// Occurs when this command is clicked
    /// </summary>
    public override void OnClick() {
      // TODO: Add LineSimplify.OnClick implementation
      LineSimplifyDlg dlg = new LineSimplifyDlg();
      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
        return;
      }
      double paraValue = dlg.ParaValue;
      if (paraValue <= 0) {
        System.Windows.Forms.MessageBox.Show("参数错误！");
        return;
      }

      double orgScale = paraValue * 200;
      double dstScale = paraValue * 1000;

      Generalizer gen = new Generalizer();
      gen.InitGeneralizer(orgScale, dstScale);

      editor.StartOperation();
      IEnumFeature efs = editor.EditSelection;
      IFeature feature = null;
      object miss = Type.Missing;
      while ((feature = efs.Next()) != null) {
        if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline) {
          IPolyline line = gen.SimplifyPolylineByDT(feature.Shape as IPolyline, paraValue);
          (line as ITopologicalOperator).Simplify();
          feature.Shape = line;
          feature.Store();
        }
        else if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon) {
          IPolygon polygon = gen.SimplifyPolygonByDT(feature.Shape as IPolygon, paraValue);
          (polygon as ITopologicalOperator).Simplify();
          IGeometryCollection geoc = polygon as IGeometryCollection;
          PolygonClass ppp = new PolygonClass();
          for (int i = 0; i < geoc.GeometryCount; i++) {
            IRing r = geoc.get_Geometry(i) as IRing;
            double area = (r as IArea).Area;
            if (Math.Abs(area) > paraValue * paraValue / 10) {
              ppp.AddGeometry(r, ref miss, ref miss);
            }
          }
          ppp.Simplify();
          if (ppp.IsEmpty) {
            feature.Delete();
          }
          else {
            feature.Shape = ppp;
            feature.Store();
          }
        }
        else { 
        }        
      }
      editor.StopOperation("曲线化简");
      (m_application.Document as IMxDocument).ActiveView.Refresh();
    }

    public override bool Enabled {
      get {
        return base.Enabled
          && (m_application.Document as IMxDocument).ActiveView.FocusMap.SelectionCount > 0
          && editor.EditState == esriEditState.esriStateEditing
          && editor.SelectionCount > 0;
          ;
      }
    }

    #endregion
  }
}

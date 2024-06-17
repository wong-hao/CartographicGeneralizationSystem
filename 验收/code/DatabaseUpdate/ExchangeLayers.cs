using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;

namespace DatabaseUpdate {
  /// <summary>
  /// Summary description for ExchangeLayers.
  /// </summary>
  [Guid("6fd7429f-0155-43b6-8967-ddfaf1d92420")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DatabaseUpdate.ExchangeLayers")]
  public sealed class ExchangeLayers : BaseCommand {
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
    public ExchangeLayers() {
      //
      // TODO: Define values for the public properties
      //
      base.m_category = "Domap"; //localizable text
      base.m_caption = "交互图层";  //localizable text
      base.m_message = "交互数据库与更新图层";  //localizable text 
      base.m_toolTip = "交互数据库与更新图层";  //localizable text 
      base.m_name = "DatabaseUpdate.ExchangeLayers";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

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

      // TODO:  Add other initialization code
    }

    public override bool Enabled {
      get {
        return base.Enabled && UApplication.Application!= null && UApplication.Application.Workspace !=null;
      }
    }
    /// <summary>
    /// Occurs when this command is clicked
    /// </summary>
    public override void OnClick() {
      UApplication app = UApplication.Application;
      ULayerInfos mxlayers = app.Workspace.MxLayers;
      ULayerInfos mclayers = app.Workspace.McLayers;
      //app.Workspace.McLayers = null;
      app.Workspace.McLayers = mxlayers;
      app.Workspace.MxLayers = mclayers;
      foreach (var item in app.Workspace.McLayers.Layers.Values)
      {
          if (item.Name.Contains("道路中心线"))
          {
              app.McMap.MoveLayer(item, 0);
              break;
          }
      }
      foreach (var item in app.Workspace.MxLayers.Layers.Values)
      {
          if (item.Name.Contains("道路中心线"))
          {
              app.MxMap.MoveLayer(item, 0);
              break;
          }
      }

    }

    #endregion
  }
}

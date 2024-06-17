using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using System.Windows.Forms;

namespace DatabaseUpdate {
  /// <summary>
  /// Summary description for UpdataLayersProperty.
  /// </summary>
  [Guid("2c0a057c-1ca6-4207-9e8d-fa22d8424806")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DatabaseUpdate.UpdataLayersProperty")]
  public sealed class UpdataLayersProperty : BaseCommand ,IWin32Window {
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
    public UpdataLayersProperty() {
      //
      // TODO: Define values for the public properties
      //
      base.m_category = "Domap"; //localizable text
      base.m_caption = "图层属性";  //localizable text
      base.m_message = "修改更新图层属性";  //localizable text 
      base.m_toolTip = "修改更新图层属性";  //localizable text 
      base.m_name = "DatabaseUpdate.UpdataLayersProperty";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

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

    /// <summary>
    /// Occurs when this command is clicked
    /// </summary>
    public override void OnClick() {
      //LayerProperties pro = new LayerProperties(UApplication.Application);
      //pro.ShowDialog(this);
    }

    public override bool Enabled {
      get {
        return base.Enabled && UApplication.Application !=null
          &&UApplication.Application.Workspace !=null;
      }
    }
    #endregion

    #region IWin32Window 成员

    public IntPtr Handle {
      get { return new IntPtr(m_application.hWnd); }
    }

    #endregion
  }
}

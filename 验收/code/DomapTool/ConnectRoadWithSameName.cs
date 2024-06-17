using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;

namespace DomapTool {
  /// <summary>
  /// Summary description for ConnectRoadWithSameName.
  /// </summary>
  [Guid("d09fb91a-9725-4b3c-afb4-51654b056152")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DomapTool.ConnectRoadWithSameName")]
  public sealed class ConnectRoadWithSameName : BaseCommand {
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
    public ConnectRoadWithSameName() {
      //
      // TODO: Define values for the public properties
      //
      base.m_category = "Domap"; //localizable text
      base.m_caption = "同名道路连接";  //localizable text
      base.m_message = "同名道路连接";  //localizable text 
      base.m_toolTip = "连接所有同名道路（空间邻近的）";  //localizable text 
      base.m_name = "DomapTool.ConnectRoadWithSameName";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

      GGenPara.Para.RegistPara("无名路标识", "无名路; ;无路名");
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
      // TODO: Add ConnectRoadWithSameName.OnClick implementation
    }

    #endregion
  }
}

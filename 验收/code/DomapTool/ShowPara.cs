using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;

namespace DomapTool {
  /// <summary>
  /// Summary description for ShowPara.
  /// </summary>
  [Guid("760334ae-770a-4a74-b74f-e006c469aa1b")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DomapTool.ShowPara")]
  public sealed class ShowPara : BaseCommand {
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
    public ShowPara() {
      //
      // TODO: Define values for the public properties
      //
      base.m_category = "Domap"; //localizable text
      base.m_caption = "参数表";  //localizable text
      base.m_message = "显示参数表";  //localizable text 
      base.m_toolTip = "显示参数表";  //localizable text 
      base.m_name = "DomapTool.ShowPara";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

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
    GenParaDlg dlg;
    public override void OnClick() {
      // TODO: Add ShowPara.OnClick implementation
      if(dlg == null)
        dlg = new GenParaDlg();
      dlg.Show();
    }

    #endregion


  }
}

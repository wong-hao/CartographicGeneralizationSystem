using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;

namespace DatabaseUpdate
{
  /// <summary>
  /// Summary description for DataManagementToolBar.
  /// </summary>
  [Guid("d1415677-5e01-4fa0-be90-a105ebb1a865")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DatabaseUpdate.DataManagementToolBar")]
  public sealed class DataManagementToolBar : BaseToolbar
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
      MxCommandBars.Register(regKey);
    }
    /// <summary>
    /// Required method for ArcGIS Component Category unregistration -
    /// Do not modify the contents of this method with the code editor.
    /// </summary>
    private static void ArcGISCategoryUnregistration(Type registerType)
    {
      string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
      MxCommandBars.Unregister(regKey);
    }

    #endregion
    #endregion

    public DataManagementToolBar()
    {
      //
      // TODO: Define your toolbar here by adding items
      //
      //AddItem("esriArcMapUI.ZoomInTool");
      //BeginGroup(); //Separator
      //AddItem("{FBF8C3FB-0480-11D2-8D21-080009EE4E51}", 1); //undo command
      //AddItem(new Guid("FBF8C3FB-0480-11D2-8D21-080009EE4E51"), 2); //redo command
      AddItem(typeof(ConnectDatabase));
      //AddItem(typeof(UpdataLayersProperty));
      BeginGroup();
      AddItem(typeof(SyncMapCommand));
      AddItem(typeof(ExchangeLayers));
      AddItem(typeof(ScaleSelector));
      
    }

    public override string Caption
    {
      get
      {
        //TODO: Replace bar caption
        return "Domap数据管理";
      }
    }
    public override string Name
    {
      get
      {
        //TODO: Replace bar ID
        return "DataManagementToolBar";
      }
    }
  }
}
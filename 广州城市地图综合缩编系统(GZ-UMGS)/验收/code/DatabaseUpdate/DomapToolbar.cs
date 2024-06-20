using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;

namespace DatabaseUpdate {
  /// <summary>
  /// Summary description for DomapToolbar.
  /// </summary>
  [Guid("d057b16d-2395-4d59-80e9-d82a7858305b")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DatabaseUpdate.DomapToolbar")]
  public sealed class DomapToolbar : BaseToolbar {
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
      MxCommandBars.Register(regKey);
    }
    /// <summary>
    /// Required method for ArcGIS Component Category unregistration -
    /// Do not modify the contents of this method with the code editor.
    /// </summary>
    private static void ArcGISCategoryUnregistration(Type registerType) {
      string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
      MxCommandBars.Unregister(regKey);
    }

    #endregion
    #endregion

    public DomapToolbar() {
      //
      // TODO: Define your toolbar here by adding items
      //
      //AddItem("esriArcMapUI.ZoomInTool");
      //BeginGroup(); //Separator
      //AddItem("{FBF8C3FB-0480-11D2-8D21-080009EE4E51}", 1); //undo command
      //AddItem(new Guid("FBF8C3FB-0480-11D2-8D21-080009EE4E51"), 2); //redo command

      AddItem(typeof(GenerateUpdateArea));
      AddItem(typeof(FindChangeFeats));
      AddItem(typeof(FindChangeFeatsArea));
      AddItem(typeof(FindFeatureBySelectedFeature));
      AddItem(typeof(UpdateSelectedFeats));
      //AddItem(typeof(FindUpdateObjects));
      //AddItem(typeof(RoadStrokeTool));
      //AddItem(typeof(ChangeStrokes));
      BeginGroup();
      //AddItem(typeof(PlantMinHole));
    }

    public override string Caption {
      get {
        //TODO: Replace bar caption
        return "Domap数据更新";
      }
    }
    public override string Name {
      get {
        //TODO: Replace bar ID
        return "DomapToolbar";
      }
    }
    
  }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;

namespace DomapTool {
  /// <summary>
  /// Summary description for DomapToolBar.
  /// </summary>
  [Guid("bc58c852-db22-4d24-bf26-4c8fe155785b")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DomapTool.DomapToolBar")]
  public sealed class DomapToolBar : BaseToolbar {
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

    public DomapToolBar() {
      //
      // TODO: Define your toolbar here by adding items
      //
      //AddItem("esriArcMapUI.ZoomInTool");
      //BeginGroup(); //Separator
      //AddItem("{FBF8C3FB-0480-11D2-8D21-080009EE4E51}", 1); //undo command
      //AddItem(new Guid("FBF8C3FB-0480-11D2-8D21-080009EE4E51"), 2); //redo command
      AddItem(typeof(ShowPara));
      BeginGroup();
      AddItem(typeof(BuildingD));
      AddItem(typeof(BuildingB));
      AddItem(typeof(BuildingSimplify));
      AddItem(typeof(BuildC));
      AddItem(typeof(ConflictHandleOfBuildAndRoad));
      BeginGroup();
      AddItem(typeof(LineSimplify));
      AddItem(typeof(RoadConnectTool));
      AddItem(typeof(CheckOverlapForRoad));
      AddItem(typeof(RoadStrokeTool));
      AddItem(typeof(ChangeStrokes));
      BeginGroup();
      AddItem(typeof(PoolsAdjoin));
      BeginGroup();
      AddItem(typeof(PlantMinHole));
      BeginGroup();
      AddItem(typeof(POIForShortCommand));
      AddItem(typeof(Axis));
      AddItem(typeof(UnionPolygon));
      AddItem(typeof(PolygonOverlapCheck));
      AddItem(typeof(AutoSelectLines));

    }

    public override string Caption {
      get {
        //TODO: Replace bar caption
        return "Domap×ÛºÏ¹¤¾ß";
      }
    }
    public override string Name {
      get {
        //TODO: Replace bar ID
        return "DomapTool.DomapToolBar";
      }
    }
  }
}
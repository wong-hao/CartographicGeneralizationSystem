using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace DomapTool {
  /// <summary>
  /// Summary description for POIForShortCommand.
  /// </summary>
  [Guid("af3c4551-6013-4919-bf9d-3dcef8bfa51f")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DomapTool.POIForShortCommand")]
  public sealed class POIForShortCommand : BaseCommand {
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
    private IEditor editor;
    public POIForShortCommand() {
      //
      // TODO: Define values for the public properties
      //
      base.m_category = "Domap综合工具"; //localizable text
      base.m_caption = "POI简称";  //localizable text
      base.m_message = "获取POI点的简称并存储";  //localizable text 
      base.m_toolTip = "获取POI点的简称并存储";  //localizable text 
      base.m_name = "DomapTool.POIForShortCommand";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

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

    public override bool Enabled {
      get {
        return base.Enabled && editor.EditState == esriEditState.esriStateEditing;
      }
    }

    /// <summary>
    /// Occurs when this command is clicked
    /// </summary>
    public override void OnClick() {
      // TODO: Add POIForShortCommand.OnClick implementation
      IActiveView activeView = (m_application.Document as IMxDocument).ActiveView;
      POIForShortConfigDlg dlg = new POIForShortConfigDlg(activeView.FocusMap);
      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
        return;
      }

      IFeatureLayer layer = dlg.SelectLayer;
      int fullIndex = layer.FeatureClass.FindField(dlg.FullField.Name);
      int shortIndex = layer.FeatureClass.FindField(dlg.ShortField.Name);

      editor.StartOperation();
      IFeatureCursor cursor = layer.FeatureClass.Update(null, true);
      IFeature feature = null;
      while ((feature = cursor.NextFeature())!=null) {
        string name = feature.get_Value(fullIndex).ToString();
        string shortName = GetShortName(name);
        feature.set_Value(shortIndex, shortName);
        cursor.UpdateFeature(feature);
        System.Diagnostics.Debug.WriteLine("正在处理:" + feature.OID.ToString());
      }
      cursor.Flush();

      editor.StopOperation("计算简称");
    }

    #endregion
    private static string[] districtNames = new string[] {
      "越秀","天河","白云"
      ,"荔湾","海珠","黄埔"
      ,"萝岗","番禺","南沙"
      ,"花都","增城","从化"
    };
    private string GetShortName(string fullName) {
      if (fullName.StartsWith("广东省")) {
        string tmpName = fullName.Substring(3);
        if (tmpName.StartsWith("广州市")) {
          tmpName = tmpName.Substring(3);
          foreach (var item in districtNames) {
            if (tmpName.StartsWith(item))
              return tmpName;
          }
          return "市" + tmpName;
        }
        else {
          return "省" + tmpName;
        }
      }
      else if (fullName.StartsWith("广东广州")) {
        return fullName.Substring(2);
      }
      else if (fullName.StartsWith("广州市")) {
        string tmpName = fullName.Substring(3);
        foreach (var item in districtNames) {
          if (tmpName.StartsWith(item))
            return tmpName;
        }
        return "市" + tmpName;
      }
      else if (fullName.StartsWith("广州")) {
        string tmpName = fullName.Substring(2);
        foreach (var item in districtNames) {
          if (tmpName.StartsWith(item))
            return tmpName;
        }
        return fullName;
      }
      else {
        return fullName;
      }
    }
  }
}

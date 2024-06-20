using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.SystemUI;
using System.Windows.Forms;
namespace DatabaseUpdate {
  /// <summary>
  /// Summary description for ScaleSelector.
  /// </summary>
  [Guid("5ecdcc89-78c9-48ec-a137-f7ac164ded48")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DatabaseUpdate.ScaleSelector")]
  public sealed class ScaleSelector : BaseCommand,IToolControl {
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

    private ComboBox cb;
    private IApplication m_application;
    public ScaleSelector() {
      //
      // TODO: Define values for the public properties
      //
      base.m_category = "Domap"; //localizable text
      base.m_caption = "选择数据源";  //localizable text
      base.m_message = "选择数据库中不同比例尺的数据源";  //localizable text 
      base.m_toolTip = "选择数据库中不同比例尺的数据源";  //localizable text 
      base.m_name = "DatabaseUpdate.ScaleSelector";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")
      cb = new ComboBox();
      cb.DropDownStyle = ComboBoxStyle.DropDownList;
      cb.SelectedIndexChanged += new EventHandler(cb_SelectedIndexChanged);
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

    void cb_SelectedIndexChanged(object sender, EventArgs e) {
      int scale = (int)cb.SelectedItem;
      if (UApplication.Application.Workspace.McLayers.DataBaseLayers) {
        UApplication.Application.Workspace.McLayers = UApplication.Application.Workspace.DataBaseLayerInfos[scale];
      }
      else {
        UApplication.Application.Workspace.MxLayers = UApplication.Application.Workspace.DataBaseLayerInfos[scale];
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

      UApplication.Application.WorkspaceOpened += new Action<UWorkspace>(Application_WorkspaceOpened);
      
    }

    void Application_WorkspaceOpened(UWorkspace obj) {
      cb.Items.Clear();
      foreach (var item in obj.DataBaseLayerInfos.Keys) {
        cb.Items.Add(item);
      }
      if (cb.Items.Count > 0) {
        cb.SelectedIndex = 0;
      }
    }

    /// <summary>
    /// Occurs when this command is clicked
    /// </summary>
    public override void OnClick() {
      // TODO: Add ScaleSelector.OnClick implementation
    }

    public override bool Enabled {
      get {
        return base.Enabled && UApplication.Application.Workspace !=null;
      }
    }

    #endregion

    #region IToolControl 成员

    public bool OnDrop(esriCmdBarType barType) {
      return true;
      //throw new NotImplementedException();
    }

    public void OnFocus(ICompletionNotify complete) {
      //throw new NotImplementedException();
    }

    public int hWnd {
      get { return cb.Handle.ToInt32(); }
    }

    #endregion
  }
}

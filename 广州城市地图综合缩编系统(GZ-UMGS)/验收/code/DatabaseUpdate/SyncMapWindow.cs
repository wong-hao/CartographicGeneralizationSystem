using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace DatabaseUpdate {
  [Guid("5012111a-9010-4f08-bebc-1c6e5a9206e6")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("DatabaseUpdate.SyncMap")]
  public partial class SyncMapWindow : UserControl, IDockableWindowDef {
    private IApplication m_application;

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
      MxDockableWindows.Register(regKey);

    }
    /// <summary>
    /// Required method for ArcGIS Component Category unregistration -
    /// Do not modify the contents of this method with the code editor.
    /// </summary>
    private static void ArcGISCategoryUnregistration(Type registerType) {
      string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
      MxDockableWindows.Unregister(regKey);

    }

    #endregion
    #endregion

    public SyncMapWindow() {      
      InitializeComponent();
    }

    #region IDockableWindowDef Members

    string IDockableWindowDef.Caption {
      get {
        //TODO: Replace with locale-based initial title bar caption
        return "数据对比窗口";        
      }
    }

    int IDockableWindowDef.ChildHWND {
      get { return this.Handle.ToInt32(); }
    }

    string IDockableWindowDef.Name {
      get {
        //TODO: Replace with any non-localizable string
        return this.Name;
      }
    }
    Timer timer;
    void IDockableWindowDef.OnCreate(object hook) {
      timer = new Timer();
      timer.Interval = 100;
      timer.Tick += new EventHandler(timer_Tick);
      timer.Start();
      UApplication.Application.EsriMapControl = this.axMapControl;
      axMapControl.ShowScrollbars = false;
      axMapControl.AutoMouseWheel = false;
      m_application = hook as IApplication;
      IMxDocument doc = m_application.Document as IMxDocument;
      IMap map = doc.ActiveView.FocusMap;
      IActiveViewEvents_Event events = doc.ActiveView as IActiveViewEvents_Event;
      //events.AfterDraw += new IActiveViewEvents_AfterDrawEventHandler(events_AfterDraw);
    }

    IEnvelope lastEnv;
    void timer_Tick(object sender, EventArgs e) {
      IMxDocument doc = m_application.Document as IMxDocument;
      IActiveView view = doc.ActiveView;
      IEnvelope env = view.Extent;
      if (lastEnv == null) {
        lastEnv = env;
        axMapControl.Extent = env;
        return;
      }
      if (lastEnv.LowerLeft.Compare(env.LowerLeft) != 0 || lastEnv.UpperRight.Compare(env.UpperRight) != 0) {
        lastEnv = env;
        axMapControl.Extent = env; 
      }     
    }



    void events_AfterDraw(ESRI.ArcGIS.Display.IDisplay Display, esriViewDrawPhase phase) {      
        IMxDocument doc = m_application.Document as IMxDocument;
        IActiveView view = doc.ActiveView;
        IEnvelope env = view.Extent;
        axMapControl.Extent = env;
        //axMapControl.Refresh();
    }

    void IDockableWindowDef.OnDestroy() {
      timer.Stop();
      timer.Dispose();
      //TODO: Release resources and call dispose of any ActiveX control initialized
      axMapControl.Dispose();
    }

    object IDockableWindowDef.UserData {
      get { return null; }
    }

    #endregion
  }
}

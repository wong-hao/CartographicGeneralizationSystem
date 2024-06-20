using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Threading;
namespace BuildingGen {
    public class GApplication {
        public static IWorkspaceFactory GDBFactory = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();
        public static IWorkspaceFactory MDBFactory = new ESRI.ArcGIS.DataSourcesGDB.AccessWorkspaceFactoryClass();
        public static IWorkspaceFactory ShpFactory = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();
        public static IWorkspaceFactory MemoryFactory = new ESRI.ArcGIS.DataSourcesGDB.InMemoryWorkspaceFactory();
        public static ESRI.ArcGIS.Geometry.GeometryEnvironmentClass GeometryEnvironment = new ESRI.ArcGIS.Geometry.GeometryEnvironmentClass();
        public static string ApplicationName = "广州城市地图综合缩编系统(GZ-UMGS)";
        
        public GWorkspace Workspace { get; internal set; }
        public GConfig AppConfig { get; internal set; }
        public GGenPara GenPara { get; internal set; }
        public AxMapControl MapControl { get; internal set; }
        public AxTOCControl TocControl { get; internal set; }
        public GenParaDlg GenParaDlg { get; internal set; }
        public IWorkspace MemoryWorkspace { get; internal set; }
        private System.Windows.Forms.Form mainform;
        public IEngineEditor EngineEditor { get; internal set; }
        //private WaitForm waitForm;
        public ESRI.ArcGIS.Geoprocessor.Geoprocessor Geoprosessor { get; internal set; }

        public event EventHandler WorkspaceOpened;
        public event EventHandler WorkspaceClosed;

        internal GApplication(AxMapControl ctrl, AxTOCControl toc, System.Windows.Forms.Form form) {
            this.mainform = form;
            MapControl = ctrl;
            MapControl.Map.Name = "未打开";
            TocControl = toc;
            TocControl.Update();
            Workspace = null;
            EngineEditor = new EngineEditorClass();
            AppConfig = GConfig.Create(this);
            GenPara = new GGenPara();
            object para = AppConfig["_gPara"];
            if (para != null) {
                GenPara.LoadFromString(para.ToString());
            }
            GenParaDlg = new GenParaDlg(GenPara);
            //GenParaDlg.SetData(GenPara.data);
            Geoprosessor = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            //waitForm = new WaitForm();
            mainform.FormClosing += new System.Windows.Forms.FormClosingEventHandler(mainform_FormClosing);
            GTOCManager tm = new GTOCManager(this);
            GMapControlManager mm = new GMapControlManager(ctrl);
            ESRI.ArcGIS.esriSystem.IName mName = MemoryFactory.Create("", "Memorty", null, 0) as ESRI.ArcGIS.esriSystem.IName;
            MemoryWorkspace = mName.Open() as IWorkspace;
        }

        void mainform_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e) {
            AppConfig["_gPara"] = GenPara.SaveToString();

            if (Workspace != null) {
                System.Windows.Forms.DialogResult r = System.Windows.Forms.MessageBox.Show("工作区尚未保存，是否保存？", "提示", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (r == System.Windows.Forms.DialogResult.Cancel) {
                    e.Cancel = true;
                }
                else {
                    if (r == System.Windows.Forms.DialogResult.Yes)
                        Workspace.Save();
                    if (abort != null) {
                        abort(this);
                    }
                }
            }
        }

        public string ExePath {
            get {
                return System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            }
        }

        public void CreateWorkspace(string path) {
            Workspace = GWorkspace.CreateWorkspace(this, path);

            mainform.Text = ApplicationName + "-" + Workspace.Name;
            MapControl.Refresh();
            TocControl.Update();
            if (WorkspaceOpened != null) {
                WorkspaceOpened(this, new EventArgs());
            }
        }
        public void OpenWorkspace(string path) {
            SetBusy(true);
            Workspace = GWorkspace.Open(this, path);
            mainform.Text = ApplicationName + "-" + Workspace.Name;
            MapControl.Refresh();
            TocControl.Update();
            if (WorkspaceOpened != null) {
                WorkspaceOpened(this, new EventArgs());
            }
            SetBusy(false);
        }
        public void CloseWorkspace() {
          mainform.Text = ApplicationName;
            this.Workspace = null;
            this.MapControl.Map = new ESRI.ArcGIS.Carto.MapClass();
            this.MapControl.Map.Name = "未打开";
            TocControl.Update();
            MapControl.Refresh();
            if (WorkspaceClosed != null) {
                WorkspaceClosed(this, new EventArgs());
            }
        }

        public void StartEdit() {
            if (Workspace == null) {
                return;
            }
            if (EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing) {
                return;
            }
            EngineEditor.StartEditing(Workspace.Workspace, Workspace.Map);
            EngineEditor.EnableUndoRedo(true);
        }
        public void StopEdit(bool saveChange) {
            if (EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing) {
                EngineEditor.StopEditing(saveChange);
            }
        }
        public void SaveEdit() {
            if (EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing) {
                EngineEditor.StopEditing(true);
                StartEdit();
            }
        }

        System.Windows.Forms.Cursor lastCursor;
        Thread thread;
        internal event Action<GApplication> abort;
        internal event Action<string> waitText;
        //internal event Action<int> maxValue;
        internal event Action<int> step;
        public WaitOperation SetBusy(bool busy) {
            if (busy) {
                lastCursor = mainform.Cursor;
                mainform.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            }
            else {
                mainform.Cursor = (lastCursor == null) ? System.Windows.Forms.Cursors.Default : lastCursor;
            }

            if (busy) {
                ThreadStart start = () => {
                    WaitForm wait = new WaitForm(this);
                    wait.ShowDialog();
                    //wait.Dispose();
                };
                thread = new Thread(start);
                thread.Start();
                WaitOperation wo = new WaitOperation();
                wo.SetText = (v) => {
                        if (waitText != null)
                            waitText(v);
                    };
                //wo.SetMaxValue = (v) =>
                //    {
                //        if (maxValue != null)
                //            maxValue(v);
                //    };
                wo.Step = (v) => {
                    if (step != null) {
                        step(v);
                    }
                };
                return wo;
            }
            else {
                if (abort != null)
                    abort(this);
                //this.mainform.Refresh();
            }
            return null;

        }
    }

    public class WaitOperation {
        public Action<string> SetText;
        //public Action<int> SetMaxValue;
        public Action<int> Step;
    }


}

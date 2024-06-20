using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.DataSourcesFile;
using System.Windows.Forms;
using System.Text;

namespace DatabaseUpdate
{
    /// <summary>
    /// Summary description for UpdateSelectedFeats.
    /// </summary>
    [Guid("64766a80-58b9-4c71-a136-d4c911439026")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DatabaseUpdate.UpdateSelectedFeats")]
    public sealed class UpdateSelectedFeats : BaseCommand
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
            MxCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        private IApplication m_application;
        public UpdateSelectedFeats()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text
            base.m_caption = "执行更新";  //localizable text
            base.m_message = "执行更新";  //localizable text 
            base.m_toolTip = "执行更新";  //localizable text 
            base.m_name = "DatabaseUpdate.UpdateSelectedFeats";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overriden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
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

        public override bool Enabled
        {
            get
            {
                return base.Enabled && editor.EditState == esriEditState.esriStateEditing;
            }
        }
        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public DoUpdate doUpdate;
        IActiveView ac;
        private IEditor editor;
        public override void OnClick()
        {
            try
            {
                ac = (m_application.Document as IMxDocument).ActiveView;
                IMap map = ac.FocusMap;
                doUpdate = new DoUpdate(map);
                IFeatureLayer updateLayer = null;
                if (doUpdate.ShowDialog() == DialogResult.OK)
                {
                    updateLayer = doUpdate.SelectLayer;
                }
                else
                {
                    doUpdate = null;
                    return;
                }
                if (updateLayer == null)
                {
                    MessageBox.Show("请导入更新图层！");
                    return;
                }
                UApplication app = UApplication.Application;
                //ULayerInfos mxlayers = app.Workspace.MxLayers;
                ULayerInfos mclayers = app.Workspace.McLayers;
                IFeatureLayer mcLayer = null;
                foreach (IFeatureLayer tpmcLayer in mclayers.Layers.Values)
                {
                    //if (tpmcLayer.Name == updateLayer.Name)
                    if (updateLayer.Name.Contains(tpmcLayer.Name))
                    {
                        mcLayer = tpmcLayer;
                        break;
                    }
                }
                if (mcLayer == null)
                {
                    MessageBox.Show("请检查导入的图层名是否符合要求！");
                    return;
                }
                IFeatureClass mxClass = updateLayer.FeatureClass;
                IFeatureClass mcClass = mcLayer.FeatureClass;
                IDataset mxDataset = mxClass as IDataset;
                IWorkspace mxWS = mxDataset.Workspace;
                if (mxWS.Type != esriWorkspaceType.esriLocalDatabaseWorkspace)
                {
                    MessageBox.Show("请切换数据库到主窗口！");
                    return;
                }

                IFields mxFields = mxClass.Fields;
                IFields mcFields = mcClass.Fields;
                List<string> moreFieldsName = new List<string>();
                List<string> lessFieldsName = new List<string>();
                List<string> commonFieldsName = new List<string>();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < mxFields.FieldCount; i++)
                {
                    IField mxfield = mxFields.get_Field(i);
                    if (!mxfield.Editable)
                        continue;
                    int mcIdx = mcFields.FindField(mxfield.Name);
                    if (mcIdx == -1)
                    {
                        sb.AppendLine(string.Format("\t[{0}]-[无对应]：采集数据中没有找到对应字段;",mxfield.Name));
                        continue;
                    }

                    IField mcfield = mcFields.get_Field(mcIdx);
                    if (mxfield.Type != mcfield.Type)
                    {
                        sb.AppendLine(string.Format("\t[{0}]-[{1}]：字段类型不一致;", mxfield.Name,mcfield.Name));
                        continue;
                    }
                    commonFieldsName.Add(mcfield.Name);

                }
                for (int i = 0; i < mcFields.FieldCount; i++)
                {
                    IField mcfield = mcFields.get_Field(i);
                    if (!mcfield.Editable)
                        continue;
                    int mxIdx = mxFields.FindField(mcfield.Name);
                    if (mxIdx == -1)
                    {
                        sb.AppendLine(string.Format("\t[无对应]-[{0}]：数据库数据中没有找到对应字段;", mcfield.Name));
                        continue;
                    }

                }
                if (sb.Length > 0)
                {
                    string message = string.Format("数据库字段与采集数据字段不一致：\t\t\t\t\t\t\t\n{0}是否忽略，继续执行更新？", sb.ToString());
                    if (MessageBox.Show(message,"字段不一致", MessageBoxButtons.YesNo,MessageBoxIcon.Information) == DialogResult.No)
                    {
                        return;
                    }
                }
                editor.StartOperation();
                ISelectionSet mcSet = (mcLayer as IFeatureSelection).SelectionSet;
                ICursor mcCur;
                mcSet.Search(null, false, out mcCur);
                IFeatureCursor mcFeatCur = mcCur as IFeatureCursor;
                IFeature mcFeat = null;
                IFeatureCursor insertCur = mxClass.Insert(false);
                IStatusBar sBar = m_application.StatusBar;
                IStepProgressor sPro = sBar.ProgressBar;
                sPro.Position = 0;
                sBar.ShowProgressBar("更新执行中...", 0, mcSet.Count, 1, true);
                while ((mcFeat = mcFeatCur.NextFeature()) != null)
                {
                    sBar.StepProgressBar();
                    IFeatureBuffer insertBuffer = mxClass.CreateFeatureBuffer();
                    insertBuffer.Shape = mcFeat.Shape;
                    foreach (string f in commonFieldsName)
                    {
                        int indexx = mxClass.FindField(f);
                        insertBuffer.set_Value(indexx, mcFeat.get_Value(mcClass.FindField(f)));
                    }
                    insertCur.InsertFeature(insertBuffer);
                }
                sBar.HideProgressBar();
                ISelectionSet mxSet = (updateLayer as IFeatureSelection).SelectionSet;
                ICursor mxCur;
                mxSet.Search(null, false, out mxCur);
                IFeatureCursor mxFeatCur = mxCur as IFeatureCursor;
                IFeature mxFeat = null;
                sBar.ShowProgressBar("更新执行中...", 0, mxSet.Count, 1, true);
                while ((mxFeat = mxFeatCur.NextFeature()) != null)
                {
                    sBar.StepProgressBar();
                    mxFeat.Delete();
                }
                sBar.HideProgressBar();
                editor.StopOperation("数据更新");
                System.Runtime.InteropServices.Marshal.ReleaseComObject(mcCur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(mxCur);
                ac.Refresh();
                UApplication.Application.EsriMapControl.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
            }
        }


        #endregion
    }
}

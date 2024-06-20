using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using Microsoft.Win32;

namespace DatabaseUpdate
{
    /// <summary>
    /// Summary description for ConnectDatabase.
    /// </summary>
    [Guid("a1d1fbdf-6b19-4b24-9563-61feef0ec069")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DatabaseUpdate.ConnectDatabase")]
    public sealed class ConnectDatabase : BaseCommand
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
        public ConnectDatabase()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text
            base.m_caption = "连接数据库";  //localizable text
            base.m_message = "连接数据库";  //localizable text 
            base.m_toolTip = "连接数据库";  //localizable text 
            base.m_name = "DatabaseUpdate.ConnectDatabase";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

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
            UApplication.Application.EsriApplication = m_application;
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
        public override void OnClick()
        {

            /*SpatialDatabaseConnection connect = new SpatialDatabaseConnection();
            IPropertySet connectSet = new PropertySetClass();
            if (connect.ShowDialog() == DialogResult.OK)
            {
                connectSet.SetProperty("SERVER", connect.serverName);
                connectSet.SetProperty("INSTANCE", connect.serviceName);
                connectSet.SetProperty("USER", connect.user);
                connectSet.SetProperty("PASSWORD", connect.password);
                connectSet.SetProperty("AUTHENTICATION_MODE", "DBMS");
                connectSet.SetProperty("VERSION", connect.version);
            }
            else
            {
                return;
            }
          try {
            UApplication.Application.Open(connectSet);
          }
          catch {
            System.Windows.Forms.MessageBox.Show("无法连接到数据库，请检查网络或者数据库服务器。", "错误", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1);
          }*/
            
            //FolderBrowserDialog fbd = new FolderBrowserDialog();//输入数据层名称必须是房屋面等标准名称
            //fbd.SelectedPath = @"E:\广州城市地图综合\验收\演示\数据更新演示\试验数据库.gdb";
            //fbd.ShowNewFolderButton = false;
            //if (fbd.ShowDialog() == DialogResult.Cancel)
            //    return;
            //string wsPath = fbd.SelectedPath;

          ConnectLocalDataDlg dlg = new ConnectLocalDataDlg();
          if (dlg.ShowDialog() != DialogResult.OK)
            return;
          string wsPath = dlg.GDBPath;
          int[] scales = dlg.Scales;
          ULayerInfos.LayerType[] featureClassNames = dlg.FeatureClassName;

            FileGDBWorkspaceFactoryClass wsFactory = new FileGDBWorkspaceFactoryClass();
            try
            {
                if (wsFactory.IsWorkspace(wsPath))
                {
                  UApplication.Application.Open(wsPath, scales,featureClassNames);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("无法连接到数据库，请检查数据库。", "错误", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1);
            }
        }

        #endregion
    }
}

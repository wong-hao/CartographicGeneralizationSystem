using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.BaseFunction
{
    public class RasterOpenCMD : SMGI.Common.SMGICommand
    {
        public RasterOpenCMD()
        {
            m_caption = "打开栅格数据";
            m_toolTip = "打开一个已有的IMG";
            m_category = "工程";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null;
            }
        }
        string ConfigKey()
        {
            return this.GetType().FullName + ".WorkspacePath";
        }

        public override void OnClick()
        {
            if (m_Application.Workspace != null)
            {
                MessageBox.Show("已经打开工作区，请先关闭工作区!");
                return;
            }

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "栅格文件(*.IMG)|*.IMG|*.bmp|*.bmp|*.jpg|*.jpg|*.tif|*.tif";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string SelectedPath = dlg.FileName;
            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在打开工作区");      
                m_Application.AppConfig[ConfigKey()] = SelectedPath;
                string pathname = System.IO.Path.GetDirectoryName(SelectedPath);
                string filename = System.IO.Path.GetFileName(SelectedPath);
                IWorkspace ws = GApplication.RasterFactory.OpenFromFile(pathname, 0); 
               // m_Application.InitESRIRasterWorkspace(ws,filename);                

            }
        }
           
      
    }
   
}

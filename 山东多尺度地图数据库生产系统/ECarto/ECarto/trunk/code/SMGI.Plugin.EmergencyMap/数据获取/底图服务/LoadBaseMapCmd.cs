using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.GISClient;

namespace SMGI.Plugin.EmergencyMap
{
    public class LoadBaseMapCmd: SMGICommand
    {
        public LoadBaseMapCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace==null;
            }
        }

        public override void OnClick()
        {
             
            try
            {
                FrmBaseMapSet frm = new FrmBaseMapSet();
                if (frm.ShowDialog() != DialogResult.OK)
                    return;
                using (WaitOperation wo = m_Application.SetBusy())
                {
                    m_Application.MapControl.Map.ClearLayers();
                    m_Application.MapControl.Map.Name = "地图服务";
                    var info = frm.MapServiceInfo;
                    if (info.Url == "本地数据库")
                    {
                        string gdb = info.Element.Element("MapServiceUrl").Element("DataBase").Value + ".gdb";
                        LoadLocalBase(gdb, info.Name);
                    }
                    if (info.Url == "SDE数据库")
                    {
                        LoadSDE(info.Element.Element("MapServiceUrl"), info.Name);
                    }
                }
                m_Application.MapControl.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                MessageBox.Show("地图服务连接失败！");
            }
               
        }

        public void LoadLocalBase(string gdb,string mxd)
        {
            string gdbpath = m_Application.Template.Root + "\\专家库\\地图服务\\地图文档\\" + gdb;
            IWorkspaceFactory wf = new FileGDBWorkspaceFactoryClass();
            IWorkspace ws=  wf.OpenFromFile(gdbpath, 0);
            string mxdFullFileName = m_Application.Template.Root + "\\专家库\\地图服务\\地图文档\\"+mxd;
            m_Application.MapControl.LoadMxFile(mxdFullFileName);
            IEnumLayer layers = m_Application.MapControl.Map.get_Layers();
            layers.Reset();
            ILayer l = null;
            while ((l = layers.Next()) != null)
            {
                if (l is IFeatureLayer)
                {
                 
                    if((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass,l.Name))
                    {
                        var fc = (ws as IFeatureWorkspace).OpenFeatureClass(l.Name);
                        (l as IFeatureLayer).FeatureClass = fc;
                       
                    }
                }

            }

           
                           
        }
      
        public void LoadSDE(XElement ele,string mxd)
        {
            string _database = ele.Element("DataBase").Value;
            string _sdeAddress = ele.Element("IPAddress").Value;
            string _userName = ele.Element("UserName").Value;
            string _password = ele.Element("Password").Value;
            
            IWorkspace ws = m_Application.GetWorkspacWithSDEConnection(_sdeAddress, _userName, _password, _database);
            if (null == ws)
            {
                MessageBox.Show("无法访问服务器！");
                return;
            }
            string mxdFullFileName = m_Application.Template.Root + "\\专家库\\地图服务\\地图文档\\" + mxd;
            m_Application.MapControl.LoadMxFile(mxdFullFileName);
            IEnumLayer layers = m_Application.MapControl.Map.get_Layers();
            layers.Reset();
            ILayer l = null;
            while ((l = layers.Next()) != null)
            {
                if (l is IFeatureLayer)
                {

                    if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, l.Name))
                    {
                        var fc = (ws as IFeatureWorkspace).OpenFeatureClass(l.Name);
                        (l as IFeatureLayer).FeatureClass = fc;
                    }
                }

            }
        }

    }
}

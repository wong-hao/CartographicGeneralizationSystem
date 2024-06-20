using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.BaseFunction
{
    public class ExportMxdCommand : SMGI.Common.SMGICommand
    {
        public ExportMxdCommand()
        {
            m_caption = "导出Mxd";
            m_toolTip = "将当前工程导出为Mxd文档";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }
        public override void OnClick()
        {
            if (m_Application.Workspace == null)
            {
                System.Windows.Forms.MessageBox.Show("未打开地图工程");
                return;
            }
           // string folderPath = m_Application.Workspace.EsriWorkspace.PathName;
           // string[] temppath = folderPath.Split('\\');
           // string tempp = folderPath.Substring(0, folderPath.Length - temppath[temppath.Length - 1].Length);
           // string tempp = temppath[temppath.Length-1].Split('_')[0];
            IMapDocument mapDoc = new MapDocumentClass();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "地图文档(*.mxd)|*.mxd";

            string mapName = "";
            try
            {
                mapName = GetMapName();
                if (mapName == "")
                {
                    mapName = GetGdbName();
                }
                sfd.FileName = mapName;
            }
            catch
            {

            }
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                mapDoc.New(sfd.FileName);
                if (m_Application.LayoutState == LayoutState.PageLayoutControl)
                {
                    var pg = GConvert.Base64ToObject(GConvert.ObjectToBase64(m_Application.Workspace.PageLayout));
                    mapDoc.ReplaceContents(pg as IMxdContents);
                }
                else
                {
                    var mp = GConvert.Base64ToObject(GConvert.ObjectToBase64(m_Application.Workspace.Map));
                    mapDoc.ReplaceContents(mp as IMxdContents);
                }

                mapDoc.Save(true, false);
                mapDoc.Close();

                //if (m_Application.LayoutState == LayoutState.PageLayoutControl)
                //{
                //    //m_Application.PageLayoutControl.DocumentFilename = string.Empty;
                //    m_Application.PageLayoutControl.ActiveView.FocusMap = m_Application.Workspace.Map;
                //}
                //else
                //{
                //    //m_Application.MapControl.DocumentFilename = string.Empty;
                //    m_Application.MapControl.Map=m_Application.Workspace.Map;
                //}
            }
        }
        private string GetMapName()
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFDOGraphicsLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LANNO").FirstOrDefault();
            if (lyr == null)
                return "";
            IFeatureClass fclanno = (lyr as IFeatureLayer).FeatureClass;
            IQueryFilter qf = new QueryFilterClass();
            IFeature fe;
            IFeatureCursor cursor = null;
            //图名
            qf.WhereClause = "TYPE = '图名'";
            cursor = fclanno.Search(qf, false);
            fe = cursor.NextFeature();
            if (fe != null)
            {
                var namefe = fe as IAnnotationFeature2;
                var txtEle = namefe.Annotation as ITextElement;
                string mapName = txtEle.Text;
                mapName = mapName.Replace(" ", "");
                return mapName;
            }
            else
            {
                return "";
            }
        }

        private string GetGdbName()
        {
            string gdbpath = GApplication.Application.Workspace.EsriWorkspace.PathName;
            string gdbname = System.IO.Path.GetFileNameWithoutExtension(gdbpath);
            gdbname = gdbname.Replace("_Ecarto", "");
            gdbname = gdbname.Replace(" ", "");
            return gdbname;
        }
    }
}

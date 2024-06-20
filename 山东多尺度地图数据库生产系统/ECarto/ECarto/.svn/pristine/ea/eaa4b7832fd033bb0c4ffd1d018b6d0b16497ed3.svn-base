using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Framework;
using SMGI.Common;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessing;

namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class DataMapExportCmd : SMGICommand
    {
      
        public DataMapExportCmd()
        {
            m_caption = "输出PDF地图";
            m_toolTip = "输出PDF地图";
            m_category = "输出";
        }
        public override void OnClick()
        {
            FrmExportDataMap pExport = new FrmExportDataMap();
            if (pExport.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            double width = pExport.PageWidth;
            double height = pExport.PageHeight;
            //var paramContent = EnvironmentSettings.getContentElement(m_Application);
            //var pagesize = paramContent.Element("PageSize");//页面大小
            //double width = double.Parse(pagesize.Element("Width").Value);
            //double height = double.Parse(pagesize.Element("Height").Value);

            string err = "";
            using (WaitOperation wo = m_Application.SetBusy())
            {
                wo.SetText("正在输出地图...");
                MapExportHelper mh = new MapExportHelper();

                err = mh.ExportPageLayoutMap(m_Application, pExport.Resolution, pExport.m_FileName, pExport.ColorMode, true, width, height);
              
            }
            if (err != "")
            {
                MessageBox.Show(err);
            }
            else
            {
                if (MessageBox.Show(string.Format("输出完成：{0}\n是否打开该文件？", pExport.m_FileName), "完成", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(pExport.m_FileName);
                }
            }
            
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.LayoutState == LayoutState.PageLayoutControl;
            }
        }
        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            messageRaisedAction("正在输出地图...");
            
            try
            {
                string sourceFileGDB = GApplication.Application.Workspace.FullName;
                var paramContent = EnvironmentSettings.getContentElement(GApplication.Application);
                var pagesize = paramContent.Element("PageSize");//页面大小
                double width = double.Parse(pagesize.Element("Width").Value);
                double height = double.Parse(pagesize.Element("Height").Value);


                double max = Math.Max(height, width);
                double min = Math.Min(height, width);
                string fileName = args.Element("FileName").Value;
                long res = long.Parse(args.Element("Resolution").Value);

                string err = "";
                var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "ClipBoundary");
                })).FirstOrDefault() as IFeatureLayer;
                IFeature f;
                IFeatureCursor cursor = lyrs.FeatureClass.Search(new QueryFilterClass { WhereClause = "TYPE = '页面'" }, false);
                f = cursor.NextFeature();
                if (f != null)
                {
                    if (f.Shape.Envelope.Width > f.Shape.Envelope.Height)
                    {
                        width = max;
                        height = min;
                    }
                    else
                    {
                        width = min;
                        height = max;
                    }
                }
                Marshal.ReleaseComObject(cursor);
                Marshal.ReleaseComObject(f);
                //判断纸张范围
                var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper () == "LANNO");
                })).FirstOrDefault() as IFeatureLayer;
                IGeoDataset geodataset = lyr.FeatureClass as IGeoDataset;
                if (!geodataset.Extent.IsEmpty)
                {
                    double mapwidth = (geodataset.Extent.Width) / m_Application.ActiveView.FocusMap.ReferenceScale * 1000;
                    double mapheight = (geodataset.Extent.Height) / m_Application.ActiveView.FocusMap.ReferenceScale * 1000;
                    if (width < mapwidth)
                    {
                        width = mapwidth + 30;
                    }
                    if (height < mapheight)
                    {
                        height = mapheight + 20;
                    }
                }
                MapExportHelper mh = new MapExportHelper();

                err = mh.ExportPageLayoutMap(m_Application, res, fileName, esriExportColorspace.esriExportColorspaceCMYK, true, width, height);
                if (err != string.Empty)
                {
                    messageRaisedAction(string.Format("导出地图失败：{0}",err));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
            
        }
    }
    
}

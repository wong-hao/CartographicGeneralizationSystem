using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessing;
namespace SMGI.Plugin.MapGeneralization
{
    public class PolygonGeneralizeCmd : SMGI.Common.SMGICommand
    {
        public PolygonGeneralizeCmd()
        {
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
            }
        }
       
        public override void OnClick()
        {
            var layerSelector = new LayerSelectWithGeneralizeForm(m_Application);
            layerSelector.GeoTypeFilter = esriGeometryType.esriGeometryPolygon;
            if (layerSelector.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            if (layerSelector.pSelectLayer == null)
            {
                return;
            }
            IFeatureClass inputFC = (layerSelector.pSelectLayer as IFeatureLayer).FeatureClass;
            
            var gp = m_Application.GPTool;

            //打开计时器
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            using (var wo = m_Application.SetBusy())
            {
                if (layerSelector.txtBend.Text.Trim() == null || layerSelector.txtSmooth.Text.Trim() == null || layerSelector.txtScale.Text.Trim() == null) { return; }
                double bendValue = double.Parse(layerSelector.txtBend.Text.Trim());
                double smoothValue = double.Parse(layerSelector.txtSmooth.Text.Trim());
                double mapScale = double.Parse(layerSelector.txtScale.Text.Trim());

                //0.线要素类
                IWorkspace ws = (inputFC as IDataset).Workspace;
                IFeatureWorkspace feaWS = m_Application.Workspace.EsriWorkspace as IFeatureWorkspace;
                string workspacePath = ws.PathName;

                gp.OverwriteOutput = true;

                string simplifyInFeature = workspacePath + @"\" + inputFC.AliasName;
                string simplifyOutFeature = workspacePath + @"\" + inputFC.AliasName + "ToSimplify";
                try
                {
                    double bendLens = bendValue * mapScale / 1000;
                    wo.SetText("正在化简......");
                    ESRI.ArcGIS.CartographyTools.SimplifyPolygon simplifyLineTool = new ESRI.ArcGIS.CartographyTools.SimplifyPolygon(simplifyInFeature, simplifyOutFeature, "BEND_SIMPLIFY", bendLens);

                    IGeoProcessorResult geoResult = (IGeoProcessorResult)gp.Execute(simplifyLineTool, null);
                    if (geoResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                    {
                        wo.SetText("正在平滑......");
                        string smoothLineOutFeature = workspacePath + @"\" + inputFC.AliasName + "ToSimplifySmooth";
                        ESRI.ArcGIS.CartographyTools.SmoothPolygon smoothLineTool = new ESRI.ArcGIS.CartographyTools.SmoothPolygon(simplifyOutFeature, smoothLineOutFeature, "PAEK", smoothValue);
                        geoResult = (IGeoProcessorResult)gp.Execute(smoothLineTool, null);
                    }
                }
                catch (Exception ex)
                {
                    string ms = "";
                    if (gp.MessageCount > 0)
                    {
                        for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                        {
                            ms += gp.GetMessage(Count);
                        }
                    }
                    MessageBox.Show(ms);
                    throw ex;
                }
                finally
                {
                    //释放临时要素类
                    IFeatureClass simplifyFC = feaWS.OpenFeatureClass(inputFC.AliasName + "ToSimplify");
                    if (simplifyFC != null)
                    {
                        IDataset dt = simplifyFC as IDataset;
                        dt.Delete();
                    }

                    IFeatureClass simplifyPntFC = feaWS.OpenFeatureClass(inputFC.AliasName + "ToSimplify_Pnt");
                    if (simplifyPntFC != null)
                    {
                        IDataset dt = simplifyPntFC as IDataset;
                        dt.Delete();
                    }

                    Marshal.ReleaseComObject(inputFC);
                    Marshal.ReleaseComObject(simplifyFC);
                    Marshal.ReleaseComObject(simplifyPntFC);
                }
            }

            watch.Stop();
            MessageBox.Show("处理完成!\r\n" + "处理耗时：" + watch.Elapsed.Hours + ":" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds);
        }
    }
}

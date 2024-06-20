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
using SMGI.Common.Algrithm;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.CartographyTools;
namespace SMGI.Plugin.MapGeneralization
{
    //化简
    public class TopolgyEdgeSimpifyPreCmd: SMGI.Common.SMGICommand
    {
      
        List<string> _ruleNames = new List<string>();

        public TopolgyEdgeSimpifyPreCmd()
        {
            m_caption = "拓扑边化简 预处理";
            m_toolTip = "拓扑边化简 预处理";
            m_category = "拓扑";

        }
        public override bool Enabled
        {
            get
            {
                
                return TopologyApplication.Topology != null && m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
            }
        }
        double SMHeigth = 0;
        double SMWidth = 0;
        ITopology topgy = null;
        public override void OnClick()
        {
           
            var wo = m_Application.SetBusy();
            Dictionary<string, List<int>> errorsDic = new Dictionary<string, List<int>>();
            try
            {
                wo.SetText("创建分区");
                IFeatureClassContainer fcContainer = TopologyApplication.Topology as IFeatureClassContainer;
                var targetfcl = fcContainer.get_Class(0);
                string inlyrs = (targetfcl as IDataset).Workspace.PathName + "\\" + targetfcl.FeatureDataset.Name + "\\" + targetfcl.AliasName;
                var gp = new Geoprocessor() { OverwriteOutput = true };
                MakeFeatureLayer gpLayer = new MakeFeatureLayer { in_features = inlyrs, out_layer = "Top_Layer" };
                ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult geoRe = (ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult)gp.Execute(gpLayer, null);
                ESRI.ArcGIS.Geoprocessing.IGPUtilities gpUtils = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
                if (geoRe.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {

                    CreateCartographicPartitions gPpartition = new CreateCartographicPartitions();
                    gPpartition.in_features = "Top_Layer";
                    gPpartition.out_features = "Partitions";
                    gPpartition.feature_count = 500;
                    RunTool(gp, gPpartition);
                }
                wo.Dispose();
                    
                
            }
            catch(Exception  ex)
            {
                wo.Dispose();
               
            }
            MessageBox.Show("预处理完成！");
          
        }
        private void RunTool(Geoprocessor geoprocessor, IGPProcess process, ITrackCancel TC = null)
        {
            geoprocessor.OverwriteOutput = true;
            try
            {
                geoprocessor.Execute(process, null);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                string ms = "";
                if (geoprocessor.MessageCount > 0)
                {
                    for (int Count = 0; Count < geoprocessor.MessageCount; Count++)
                    {
                        ms += geoprocessor.GetMessage(Count);
                    }
                }
                MessageBox.Show(ms);
            }
        }
        private void DrawSelePolyline(IPolyline pl)
        {
            IElement element = null;
            IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            gc.Reset();
            IElement ele = null;
            while ((ele = gc.Next()) != null)
            {
                IElementProperties ep1 = ele as IElementProperties;
                if (ep1.Name == "TraceSelLine")
                    m_Application.ActiveView.GraphicsContainer.DeleteElement(ele);
            }
            ILineElement lineElement = null;
            ISimpleLineSymbol lineSymbol = null;
            IElementProperties ep = null;
            //绘制选中的线
          // if (selPolyline != null)
            {
                lineElement = new LineElementClass();
                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                lineSymbol.Color = new RgbColorClass { Red = 255 };
                lineSymbol.Width = 2;
                lineElement.Symbol = lineSymbol;
                //
                element = lineElement as IElement;
                element.Geometry = pl;
                ep = element as IElementProperties;
                ep.Name = "TraceSelLine";
                gc.AddElement(element, 0);
            }
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
        }
        public  void SetZValue(IGeometry geoSource, IGeometry pGeo)
        {
            IZAware pZAware0 = (IZAware)geoSource;
            if (pZAware0.ZAware)
            {
                IZAware pZAware = (IZAware)pGeo;
                pZAware.ZAware = true;
                IPointCollection pc = pGeo as IPointCollection;
                for (int i = 0; i < pc.PointCount; i++)
                {
                    ESRI.ArcGIS.Geometry.IPoint point = pc.get_Point(i);
                    point.Z = 0;
                }

            }
            else
            {
                IZAware pZAware = (IZAware)pGeo;
                pZAware.ZAware = false;
            }

            ////M值μ
            //if (pGeometryDef.HasM)
            //{
            //    IMAware pMAware = (IMAware)pGeo;
            //    pMAware.MAware = true;
            //}
            //else
            //{
            //    IMAware pMAware = (IMAware)pGeo;
            //    pMAware.MAware = false;

            //}
        }

       
        
    }
}

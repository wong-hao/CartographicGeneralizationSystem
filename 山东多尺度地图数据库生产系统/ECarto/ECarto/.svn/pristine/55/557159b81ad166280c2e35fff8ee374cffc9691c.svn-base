using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Collections;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Diagnostics;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.Drawing;
using ESRI.ArcGIS.Display;
using System.Windows.Forms;

namespace SMGI.Plugin.MapGeneralization
{
    public class DangleCheck : SMGITool
    {
        List<IElement> elementlist;
        IGraphicsContainer pGraphicsContainer;
        Dictionary<int, string> fileds;
        public DangleCheck()
        {
            m_caption = "显示悬挂点";
            m_toolTip = "显示悬挂点";
            m_category = "特殊";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        public override void OnClick()
        {
            elementlist = new List<IElement>();
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
            IEngineEditLayers engineEditLayer = editsketch as IEngineEditLayers;
            IFeatureLayer featureLayer = engineEditLayer.TargetLayer as IFeatureLayer;
            IFeatureClass CheckClass = featureLayer.FeatureClass;
            List<IPoint> pCheckint = new List<IPoint>();
            List<string> tempName;
            IMap Map = m_Application.MapControl.Map;
            IGraphicsLayer g1 = Map.BasicGraphicsLayer;
            pGraphicsContainer = g1 as IGraphicsContainer;
            if (CheckClass.ShapeType != esriGeometryType.esriGeometryPolyline) { MessageBox.Show("请设置当前编辑图层为线图层"); }
            Dictionary<string, string> Grid = new Dictionary<string, string>();
            Process progWindow = new Process();
            CreateAdjacencyRelation grid = new CreateAdjacencyRelation(progWindow);
            Grid = grid.CreateAdjacencyRelation1(CheckClass);

            IFeatureCursor pFeatCursor = CheckClass.Search(null, false);
            IFeature pFeature = pFeatCursor.NextFeature();

            while (pFeature != null)
            {
                progWindow.label1.Text = "正在分析要素【" + pFeature.OID.ToString() + "】......";
                

                System.Windows.Forms.Application.DoEvents();
                tempName = new List<string>();

                List<string> tempoid = new List<string>();

                try
                {
                    IPolyline CheckLine = pFeature.Shape as IPolyline;

                    if (CheckLine.Length > 0)
                    {
                        fileds = new Dictionary<int, string>();
                        double x = ((int)(CheckLine.FromPoint.X * 10)) / 10;
                        double y = ((int)(CheckLine.FromPoint.Y * 10)) / 10;
                        string curitem1 = x + "_" + y;

                        double x2 = ((int)(CheckLine.ToPoint.X * 10)) / 10;
                        double y2 = ((int)(CheckLine.ToPoint.Y * 10)) / 10;
                        string curitem2 = x2 + "_" + y2;
                        if (Grid.ContainsKey(curitem1))//起点
                        {
                            string temp = Grid[curitem1];
                            string[] temp1 = temp.Split(',');
                            if (temp1.Length == 1)
                            {
                                pCheckint.Add(CheckLine.FromPoint);
                            }
                        }
                        if (Grid.ContainsKey(curitem2))//终点
                        {
                            string temp = Grid[curitem2];
                            string[] temp1 = temp.Split(',');
                            if (temp1.Length == 1)
                            {
                                pCheckint.Add(CheckLine.ToPoint);

                            }
                        }
                        tempName.Clear();
                        pFeature = pFeatCursor.NextFeature();
                    }

                }
                catch (Exception ex)
                {
                    string err = ex.Message;
                    //errorExcelFile.RecordError("意外错误", "", "该项检查未完成");
                }
            }
            //  GC.Collect();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatCursor);
            for (int i = 0; i < pCheckint.Count; i++)
            {
                Marker(pCheckint[i]);
            }

        }
        public void Marker(IPoint point)
        {
            Color outlineRgb = Color.FromName("forestgreen");
            IElement element = null;

            SimpleMarkerSymbolClass sms = new SimpleMarkerSymbolClass();
            sms.OutlineColor = ConvertColorTolColor(outlineRgb);
            sms.Style = esriSimpleMarkerStyle.esriSMSCircle;
            sms.OutlineSize = 2;
            sms.Size = 15;
            sms.Outline = true;
            sms.Color = ConvertColorTolColor(outlineRgb);
            IMarkerSymbol symbol = (IMarkerSymbol)sms;
            IMarkerElement markerElement = new MarkerElementClass();
            element = markerElement as IElement;
            markerElement.Symbol = symbol;
            element.Geometry = point;
            // delet.Add(element);
            elementlist.Add(element);
            pGraphicsContainer.AddElement(element, 0);
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);


        } //颜色转换
        public IColor ConvertColorTolColor(Color color)
        {
            IColor pColor = new RgbColorClass();
            pColor.RGB = color.B * 65536 + color.G * 256 + color.R;
            pColor.Transparency = 10;
            return pColor;
        }
        public override bool Deactivate()
        {
            try
            {
                for (int i = 0; i < elementlist.Count; i++)
                {
                    try
                    {
                        pGraphicsContainer.DeleteElement(elementlist[i]);

                    }
                    catch (Exception)
                    {

                    }

                }
            }
            catch (Exception)
            {

            }
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            m_Application.MapControl.ActiveView.Refresh();

            return true;
        }
    }
}

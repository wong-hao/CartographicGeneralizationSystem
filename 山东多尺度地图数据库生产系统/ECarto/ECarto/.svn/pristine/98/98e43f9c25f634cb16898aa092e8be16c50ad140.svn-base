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
    public class PseudoCheck : SMGITool
    {
        Dictionary<int, string> fileds;
        List<IElement> elementlist;
        IGraphicsContainer pGraphicsContainer;
        Dictionary<string, string> Grid = new Dictionary<string, string>();

        double tolerance = 0.0000009;
        public PseudoCheck()
        {
            m_caption = "显示伪节点";
            m_toolTip = "显示伪节点";
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
            var layerSelector = new PseLayerSelectWithFiedsForm(m_Application);
            layerSelector.GeoTypeFilter = esriGeometryType.esriGeometryPolyline;
            if (layerSelector.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            elementlist = new List<IElement>();
            IMap Map = m_Application.MapControl.Map;
            IGraphicsLayer g1 = Map.BasicGraphicsLayer;
            pGraphicsContainer = g1 as IGraphicsContainer;
            IFeatureLayer inputFC = layerSelector.pSelectLayer as IFeatureLayer;
            ArrayList fieldArray = layerSelector.FieldArray;
            CheckPseudo_Supplement(inputFC.FeatureClass, fieldArray);
        }
        private void CheckPseudo_Supplement(IFeatureClass CheckClass, ArrayList FieldArray)
        {
            List<int> pCheckint = new List<int>();
            List<IPoint> pChecked = new List<IPoint>();
            List<string> tempName;

            Process progWindow = new Process();
            CreateAdjacencyRelation grid = new CreateAdjacencyRelation(progWindow);
            Grid = grid.CreateAdjacencyRelation1(CheckClass);

            IFeatureCursor pFeatCursor = CheckClass.Search(null, false);
            IFeature pFeature = pFeatCursor.NextFeature();

            while (pFeature != null)
            {
                progWindow.label1.Text = "正在处理图层【" + (CheckClass as IDataset).Name + "】中的要素" + pFeature.OID.ToString() + "......";
                System.Windows.Forms.Application.DoEvents();
                tempName = new List<string>();

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

                        if (Grid.ContainsKey(curitem1))
                        {
                            string temp = Grid[curitem1];
                            string[] temp1 = temp.Split(',');
                            if (temp1.Length == 2)
                            {
                                for (int i = 0; i < temp1.Length; i++)
                                {
                                    if (!tempName.Contains(temp1[i]))
                                    {
                                        IFeature CheckFeature = CheckClass.GetFeature(int.Parse(temp1[i]));
                                        IPolyline polyline = CheckFeature.Shape as IPolyline;
                                        string xy = polyline.FromPoint.X + "," + polyline.FromPoint.Y + "," + polyline.ToPoint.X + "," + polyline.ToPoint.Y;
                                        string filed = null;
                                        for (int j = 0; j < FieldArray.Count; j++)
                                        {
                                            int FieldIndex = CheckClass.FindField(FieldArray[j].ToString());
                                            if (FieldIndex != -1)
                                            {
                                                if (filed != null)
                                                {
                                                    filed = filed + "," + CheckFeature.get_Value(FieldIndex).ToString();
                                                }
                                                else
                                                {
                                                    filed = CheckFeature.get_Value(FieldIndex).ToString();
                                                }
                                            }
                                        }
                                        fileds.Add(tempName.Count, temp1[i] + "," + xy + "," + filed);

                                        tempName.Add(temp1[i]);

                                    }
                                }

                            }
                        }

                        if (Grid.ContainsKey(curitem2))
                        {
                            string temp = Grid[curitem2];
                            string[] temp1 = temp.Split(',');
                            if (temp1.Length == 2)
                            {
                                for (int i = 0; i < temp1.Length; i++)
                                {
                                    if (!tempName.Contains(temp1[i]))
                                    {
                                        IFeature CheckFeature = CheckClass.GetFeature(int.Parse(temp1[i]));
                                        IPolyline polyline = CheckFeature.Shape as IPolyline;
                                        string xy = polyline.FromPoint.X + "," + polyline.FromPoint.Y + "," + polyline.ToPoint.X + "," + polyline.ToPoint.Y;
                                        string filed = null;

                                        for (int j = 0; j < FieldArray.Count; j++)
                                        {
                                            int FieldIndex = CheckClass.FindField(FieldArray[j].ToString());
                                            if (FieldIndex != -1)
                                            {
                                                if (filed != null)
                                                {
                                                    filed = filed + "," + CheckFeature.get_Value(FieldIndex).ToString();
                                                }
                                                else
                                                {
                                                    filed = CheckFeature.get_Value(FieldIndex).ToString();
                                                }
                                            }
                                        }
                                        fileds.Add(tempName.Count, temp1[i] + "," + xy + "," + filed);
                                        tempName.Add(temp1[i]);
                                    }
                                }
                            }
                        }
                        ////  起点
                        for (int k = 0; k < tempName.Count; k++)
                        {
                            string[] str = fileds[k].Split(',');

                            string oid = str[0];

                            if (pFeature.OID.ToString() != oid && pCheckint.Contains(int.Parse(oid)) == false)
                            {
                                IPoint Selectfrom = new PointClass();
                                Selectfrom.X = Convert.ToDouble(str[1]);
                                Selectfrom.Y = Convert.ToDouble(str[2]);
                                IPoint Selectto = new PointClass();
                                Selectto.X = Convert.ToDouble(str[3]);
                                Selectto.Y = Convert.ToDouble(str[4]);
                                IPolyline pFeatureline = pFeature.Shape as IPolyline;
                                IGeometry pGeo1 = pFeatureline.FromPoint;
                                IProximityOperator ProxiOP = (pGeo1) as IProximityOperator;
                                if (ProxiOP.ReturnDistance(Selectfrom) < tolerance || ProxiOP.ReturnDistance(Selectto) < tolerance)
                                {
                                    bool judge = false;
                                    for (int i = 0; i < FieldArray.Count; i++)
                                    {
                                        int FieldIndex = CheckClass.FindField(FieldArray[i].ToString());
                                        if (FieldIndex != -1)
                                        {
                                            string psfeature = pFeature.get_Value(FieldIndex).ToString();
                                            string psecfeature = str[5 + i];
                                            if (psfeature != psecfeature)
                                            {
                                                judge = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (judge == false)//属性相同
                                    {
                                        pChecked.Add(CheckLine.FromPoint);
                                    }
                                }
                            }
                        }

                        for (int k = 0; k < tempName.Count; k++)
                        {
                            string[] str = fileds[k].Split(',');
                            string oid = str[0];

                            if (pFeature.OID.ToString() != oid && pCheckint.Contains(int.Parse(oid)) == false)
                            {
                                IPoint Selectfrom = new PointClass();
                                Selectfrom.X = Convert.ToDouble(str[1]);
                                Selectfrom.Y = Convert.ToDouble(str[2]);
                                IPoint Selectto = new PointClass();
                                Selectto.X = Convert.ToDouble(str[3]);
                                Selectto.Y = Convert.ToDouble(str[4]);

                                IPolyline pFeatureline = pFeature.Shape as IPolyline;
                                IGeometry pGeo1 = pFeatureline.ToPoint;
                                IProximityOperator ProxiOP = (pGeo1) as IProximityOperator;
                                if (ProxiOP.ReturnDistance(Selectfrom) < tolerance || ProxiOP.ReturnDistance(Selectto) < tolerance)
                                {
                                    bool judge = false;
                                    for (int i = 0; i < FieldArray.Count; i++)
                                    {
                                        int FieldIndex = CheckClass.FindField(FieldArray[i].ToString());
                                        if (FieldIndex != -1)
                                        {
                                            string psfeature = pFeature.get_Value(FieldIndex).ToString();
                                            string psecfeature = str[5 + i];
                                            if (psfeature != psecfeature)
                                            {
                                                judge = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (judge == false)//属性相同
                                    { pChecked.Add(CheckLine.ToPoint); }
                                }
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
            GC.Collect();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatCursor);
            for (int i = 0; i < pChecked.Count; i++)
            {
                Marker(pChecked[i]);
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

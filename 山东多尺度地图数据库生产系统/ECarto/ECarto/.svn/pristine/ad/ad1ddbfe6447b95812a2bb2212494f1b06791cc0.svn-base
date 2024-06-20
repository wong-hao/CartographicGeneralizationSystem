using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.EmergencyMap
{
    public class ExtractLandFormsCmd : SMGICommand
    {
        public ExtractLandFormsCmd()
        {
            m_caption = "提取地貌";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && 
                    m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing;;
            }
        }

        public override void OnClick()
        {
            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在提取地貌类型......");

                IRasterDataset demDataset = CommonMethods.getDEMFromWorkspace(m_Application.Workspace.EsriWorkspace);
                if (null == demDataset)
                {
                    MessageBox.Show("没有找到DEM栅格数据集！");
                    return ;
                }
                
                LandForms lf = new LandForms(m_Application);
                IFeatureLayer landFormsLayer = lf.ExtractLandForms(demDataset, wo);
                (landFormsLayer as ILayerEffects).Transparency = 20;
                if (landFormsLayer != null)
                {
                    int toIndex = m_Application.MapControl.LayerCount;
                    var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon && (l as IGeoFeatureLayer).FeatureClass.AliasName == "BOUA";
                    })).ToArray();
                    if (lyrs.Count() > 0)
                    {
                        for (int i = 0; i < m_Application.MapControl.LayerCount; ++i)
                        {
                            if (lyrs[0] == m_Application.MapControl.get_Layer(i))
                            {
                                toIndex = i + 1;
                                break;
                            }
                        }
                    }

                    m_Application.MapControl.AddLayer(landFormsLayer, toIndex);

                    string renderFieldName = "GRIDCODE";
                    int index = landFormsLayer.FeatureClass.Fields.FindField(renderFieldName);
                    if (index != -1)
                    {
                        //设置图层渲染
                        IUniqueValueRenderer render = new UniqueValueRendererClass();
                        render.FieldCount = 1;
                        render.set_Field(0, renderFieldName);

                        
                        List<IColor> clrs = new List<IColor>();
                        clrs.Add(new CmykColorClass { Cyan = 40, Magenta = 0, Yellow = 60, Black = 0, Transparency = 50 });//平原
                        clrs.Add(new CmykColorClass { Cyan = 0, Magenta = 20, Yellow = 30, Black = 0, Transparency = 50 });//丘陵
                        clrs.Add(new CmykColorClass { Cyan = 20, Magenta = 35, Yellow = 40, Black = 0, Transparency = 50 });//山地
                        clrs.Add(new CmykColorClass { Cyan = 35, Magenta = 60, Yellow = 60, Black = 0, Transparency = 50 });//高山地

                        int i = 0;
                        foreach (var kv in lf.LandFormsCode2Name)
                        {
                            ISimpleFillSymbol sym = new SimpleFillSymbolClass();
                            sym.Style = esriSimpleFillStyle.esriSFSSolid;
                            sym.Outline.Color = new CmykColorClass { NullColor = true };
                            if (i < 4)
                            {
                                sym.Color = clrs[i];
                            }
                            else
                            {
                                //生成一个随机色（一般情况）
                                int r = (int)CommonMethods.GetRandNum(0.0,255.0);
                                int g = (int)CommonMethods.GetRandNum(0.0,255.0);
                                int b = (int)CommonMethods.GetRandNum(0.0,255.0);
                                sym.Color = new RgbColorClass { Red = r, Green = g, Blue = b };
                            }

                            render.AddValue(kv.Key.ToString(), "地貌类型", sym as ISymbol);
                            render.set_Label(kv.Key.ToString(), kv.Value);

                            ++i;
                        }

                        (landFormsLayer as IGeoFeatureLayer).Renderer = render as IFeatureRenderer;

                        m_Application.TOCControl.ActiveView.Refresh();
                        m_Application.TOCControl.Update();
                    }
                    

                }
                
                m_Application.MapControl.Refresh();
                
            }


        }

        
    }
}

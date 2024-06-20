using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Controls;

using System.Linq;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.CartographyTools;
namespace SMGI.Plugin.EmergencyMap
{
    public class RepresentationSwitch : SMGICommand
    {
        public RepresentationSwitch()
        {
            m_caption = "图库切换";
            m_toolTip = "图库切换";
            m_category = "辅助";
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
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(x =>
            {
                return x is IGeoFeatureLayer;

            })).ToArray();

            Random rand=new Random();
            foreach (var l in lyrs)
            {
                if (l.Name.StartsWith("VEGA_") || l.Name.EndsWith("普色") || l.Name.EndsWith("注记"))
                    continue;
                IGeoFeatureLayer geoFlyr = l as IGeoFeatureLayer;
                if (geoFlyr.Renderer is IRepresentationRenderer)
                {
                    SimpleRenderer simpRender = new SimpleRendererClass();
                    ISymbol sym;
                    if (geoFlyr.FeatureClass.ShapeType== esriGeometryType.esriGeometryPoint)
                    {
                         sym = new SimpleMarkerSymbolClass();
                         (sym as ISimpleMarkerSymbol).Color = new RgbColorClass(){
                             Red = rand.Next(255),
                             Green = rand.Next(255),
                             Blue = rand.Next(255)
                         };
                    }
                    else if (geoFlyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                         sym = new SimpleLineSymbolClass();
                         (sym as ISimpleLineSymbol).Color = new RgbColorClass()
                         {
                             Red = rand.Next(255),
                             Green = rand.Next(255),
                             Blue = rand.Next(255)
                         };
                    }
                    else 
                    {
                         sym = new SimpleFillSymbolClass();
                         (sym as ISimpleFillSymbol).Color = new RgbColorClass()
                         {
                             Red = rand.Next(255),
                             Green = rand.Next(255),
                             Blue = rand.Next(255)
                         };
                    }
                    simpRender.Symbol = sym;
                    geoFlyr.Renderer = simpRender as IFeatureRenderer;
                }
                else
                {
                    if (geoFlyr.Renderer is ISimpleRenderer)
                    {
                        IFeatureClass fc = geoFlyr.FeatureClass;
                        IRepresentationRenderer repRenderer = new RepresentationRendererClass();
                        IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
                        bool hasRep = pRepWksExt.get_FeatureClassHasRepresentations(fc);
                        if (hasRep)
                        {
                            IEnumDatasetName enumDatasetName = pRepWksExt.get_FeatureClassRepresentationNames(fc);
                            enumDatasetName.Reset();
                            IDatasetName pDatasetName = enumDatasetName.Next();
                            if (pDatasetName!=null)
                            {
                                repRenderer.RepresentationClass = pRepWksExt.OpenRepresentationClass(pDatasetName.Name);
                            }
                        }
                        geoFlyr.Renderer = repRenderer as IFeatureRenderer;
                    }
                }
            }
            
            if (m_Application.MapControl.Map.ReferenceScale!=0)
            {
                m_Application.AppConfig["MapScale"] = m_Application.MapControl.Map.ReferenceScale;
                m_Application.MapControl.Map.ReferenceScale = 0;
                
            }
            else
            {
                int mapScale = Convert.ToInt32(m_Application.AppConfig["MapScale"]);
                m_Application.MapControl.Map.ReferenceScale = mapScale;
            }
            m_Application.MapControl.Refresh();
        }
    }
}
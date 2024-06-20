using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
namespace BuildingGen
{
    public class Render
    {
        public IFeatureLayer GetLayerByName(GWorkspace ws, string name)
        {
            IFeatureLayer targetLayer = new FeatureLayerClass();
            foreach (GLayerInfo LayerInfo in ws.LayerManager.Layers)
            {
                if (LayerInfo.Layer.Name == name)
                {
                    targetLayer = LayerInfo.Layer as IFeatureLayer;
                }
            }
            return targetLayer;
        }
        public IFeatureWorkspace CreateLayerInmemeory(IFeatureClass sourceFClass, string targetLayerName)
        {
            IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactoryClass();
            IWorkspaceName newWSName = workspaceFactory.Create("", "myWorkspace", null, 0);
            IName name = (IName)newWSName;
            IWorkspace inMemoryWS = (IWorkspace)name.Open();
            IFields sourceFields = sourceFClass.Fields;
            IFeatureClass featClass = null;
            try
            {
                featClass = ((IFeatureWorkspace)inMemoryWS).CreateFeatureClass(targetLayerName, sourceFields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            }
            catch (Exception ex)
            {
            }
            return (IFeatureWorkspace)inMemoryWS;
        }

        public void SingleStruc_UniqueValueRenderFeatureLayer(GLayerInfo layerInfo, string s_filedname)
        {
            IFeatureLayer featureLayer = layerInfo.Layer as IFeatureLayer;
            string geoRenderType = "";
            switch (featureLayer.FeatureClass.ShapeType)
            {
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                    geoRenderType = "Fill Symbols";
                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                    geoRenderType = "Line Symbols";
                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                    geoRenderType = "Marker Symbols";
                    break;
                default:
                    geoRenderType = "Fill Symbols";
                    break;
            }
            IUniqueValueRenderer unRender = new UniqueValueRendererClass();
            if (layerInfo.LayerType == GCityLayerType.建筑物)
            {
                unRender.FieldCount = 1;
                unRender.set_Field(0, "建筑结构");
            }
            ISymbol tempSymbol;
            ISymbol tempLineSymbol;
            string currentValue;
            string structCata;
            string standardValue = "";

            if (geoRenderType == "Fill Symbols")
            {
                if (layerInfo.LayerType == GCityLayerType.建筑物)
                {
                    unRender.DefaultSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                    unRender.UseDefaultSymbol = true;
                    IRandomColorRamp randomColors = new RandomColorRampClass();
                    randomColors.MaxSaturation = 28;
                    randomColors.MinSaturation = 1;
                    randomColors.MaxValue = 97;
                    randomColors.MinValue = 24;
                    randomColors.StartHue = 1;
                    randomColors.EndHue = 360;
                    randomColors.UseSeed = true;
                    randomColors.Seed = 30;
                    randomColors.Size = 1500;  //颜色数目要够
                    randomColors.Name = "colors";
                    bool pOK = true;
                    randomColors.CreateRamp(out pOK);
                    IEnumColors pEnumColors = randomColors.Colors;
                    pEnumColors.Reset();

                    string[] Structure = new string[] { "A", "B", "C", "D", "棚", "破" };
                    for (int i = 0; i < Structure.Length; i++)
                    {
                        structCata = Structure[i];
                        if (structCata == "A" || structCata == "B")
                        {
                            currentValue = structCata;

                            tempLineSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSSolid, CreateRGBColor(0, 0, 0, false) as IColor, 2);
                            tempSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                            ((ISimpleFillSymbol)tempSymbol).Outline = tempLineSymbol as ILineSymbol;
                            ISimpleFillSymbol SF = tempSymbol as ISimpleFillSymbol;
                            if ((pEnumColors.Next()) == null)
                            {
                                pEnumColors.Reset();
                            }
                            ((ISimpleFillSymbol)tempSymbol).Color = pEnumColors.Next();
                            if (structCata == "A")
                            {
                                unRender.AddValue(currentValue, "", tempSymbol);
                                standardValue = currentValue;
                            }
                            if (structCata == "B")
                            {
                                unRender.AddReferenceValue(currentValue, standardValue);
                            }
                            unRender.set_Label(currentValue, "A,B");

                        }
                        if (structCata == "C" || structCata == "D")
                        {
                            currentValue = structCata;

                            tempLineSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSDot, CreateRGBColor(0, 0, 0, false) as IColor, 2);
                            tempSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                            ((ISimpleFillSymbol)tempSymbol).Outline = tempLineSymbol as ILineSymbol;
                            if ((pEnumColors.Next()) == null)
                            {
                                pEnumColors.Reset();
                            }
                            ((ISimpleFillSymbol)tempSymbol).Color = pEnumColors.Next();
                            if (structCata == "C")
                            {
                                unRender.AddValue(currentValue, "", tempSymbol);
                                standardValue = currentValue;
                            }
                            if (structCata == "D")
                            {
                                unRender.AddReferenceValue(currentValue, standardValue);
                            }
                            unRender.set_Label(currentValue, "C,D");
                        }
                        if (structCata == "棚")
                        {
                            currentValue = structCata;

                            tempLineSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSSolid, CreateRGBColor(0, 0, 0, false) as IColor, 1);
                            tempSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                            ((ISimpleFillSymbol)tempSymbol).Outline = tempLineSymbol as ILineSymbol;
                            if ((pEnumColors.Next()) == null)
                            {
                                pEnumColors.Reset();
                            }
                            ((ISimpleFillSymbol)tempSymbol).Color = pEnumColors.Next();
                            unRender.AddValue(currentValue, "", tempSymbol);
                        }
                        if (structCata == "破")
                        {
                            currentValue = structCata;

                            tempLineSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSDot, CreateRGBColor(0, 0, 0, false) as IColor, 1);
                            tempSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                            ((ISimpleFillSymbol)tempSymbol).Outline = tempLineSymbol as ILineSymbol;
                            if ((pEnumColors.Next()) == null)
                            {
                                pEnumColors.Reset();
                            }
                            ((ISimpleFillSymbol)tempSymbol).Color = pEnumColors.Next();
                            unRender.AddValue(currentValue, "", tempSymbol);
                        }
                    }
                }

                if (featureLayer.Name == "植被面")
                {
                    tempSymbol = CreateNewDefaultFillSymbol(197, 250, 203, false);
                    unRender.DefaultSymbol = tempSymbol;
                    unRender.DefaultLabel = "植被";
                    unRender.UseDefaultSymbol = true;
                }
                if (featureLayer.Name == "交通面")
                {
                    tempSymbol = CreateNewDefaultFillSymbol(245, 202, 122, false);
                    unRender.DefaultSymbol = tempSymbol;
                    unRender.DefaultLabel = "交通面";
                    unRender.UseDefaultSymbol = true;
                }
                if (featureLayer.Name == "水系面")
                {
                    tempSymbol = CreateNewDefaultFillSymbol(151, 219, 242, false);
                    unRender.DefaultSymbol = tempSymbol;
                    unRender.DefaultLabel = "水系面";
                    unRender.UseDefaultSymbol = true;
                }
            }
            if (geoRenderType == "Line Symbols")
            {
                tempSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSSolid, CreateRGBColor(255, 170, 0, false), 3);
                unRender.DefaultSymbol = tempSymbol;
                unRender.DefaultLabel = "道路中心线";
                unRender.UseDefaultSymbol = true;
            }
            IGeoFeatureLayer GeoFeatureLayer = featureLayer as IGeoFeatureLayer;
            GeoFeatureLayer.Renderer = unRender as IFeatureRenderer;
            GeoFeatureLayer.DisplayField = s_filedname;

        }

        public void UniqueValueRenderFeatureLayer(IFeatureLayer featureLayer, string s_filedname)
        {
            string geoRenderType = "";
            switch (featureLayer.FeatureClass.ShapeType)
            {
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                    geoRenderType = "Fill Symbols";
                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                    geoRenderType = "Line Symbols";
                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                    geoRenderType = "Marker Symbols";
                    break;
                default:
                    geoRenderType = "Fill Symbols";
                    break;
            }
            IUniqueValueRenderer unRender = new UniqueValueRendererClass();

            if (featureLayer.Name == "房屋面")
            {
                unRender.FieldCount = 2;
                unRender.set_Field(0, "建筑结构");//
                unRender.set_Field(1, "G_BuildingGroup");
                unRender.FieldDelimiter = "|";
            }
            ISymbol tempSymbol;
            string currentValue;
            string standardValue = "";

            if (geoRenderType == "Fill Symbols")
            {
                if (featureLayer.Name == "房屋面")
                {
                    unRender.DefaultSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                    unRender.UseDefaultSymbol = true;
                    int GroupIndex = featureLayer.FeatureClass.FindField("G_BuildingGroup");
                    IFeatureCursor featCursor = null;
                    featCursor = featureLayer.FeatureClass.Search(null, false);  //?????
                    double maxGroup = 0;
                    double tempMaxGroup = 0;
                    string structCata;
                    IFeature nextFeat;
                    ISymbol tempLineSymbol;
                    while ((nextFeat = featCursor.NextFeature()) != null)
                    {
                        tempMaxGroup = Convert.ToDouble(nextFeat.get_Value(GroupIndex));
                        if (tempMaxGroup > maxGroup)
                        {
                            maxGroup = tempMaxGroup;
                        }
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);

                    IRandomColorRamp randomColors = new RandomColorRampClass();
                    randomColors.MaxSaturation = 28;
                    randomColors.MinSaturation = 1;
                    randomColors.MaxValue = 97;
                    randomColors.MinValue = 24;
                    randomColors.StartHue = 1;
                    randomColors.EndHue = 360;
                    randomColors.UseSeed = true;
                    randomColors.Seed = 30;
                    randomColors.Size = 1500;  //颜色数目要够
                    randomColors.Name = "colors";
                    bool pOK = true;
                    randomColors.CreateRamp(out pOK);
                    IEnumColors pEnumColors = randomColors.Colors;
                    pEnumColors.Reset();

                    for (int j = 1; j < maxGroup + 1; j++)
                    {

                        string[] Structure = new string[] { "A", "B", "C", "D", "棚", "破" };
                        for (int i = 0; i < Structure.Length; i++)
                        {
                            structCata = Structure[i];
                            if (structCata == "A" || structCata == "B")
                            {
                                currentValue = structCata + "|" + Convert.ToString(j);

                                tempLineSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSSolid, CreateRGBColor(0, 0, 0, false) as IColor, 2);
                                tempSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                                ((ISimpleFillSymbol)tempSymbol).Outline = tempLineSymbol as ILineSymbol;
                                ISimpleFillSymbol SF = tempSymbol as ISimpleFillSymbol;
                                if ((pEnumColors.Next()) == null)
                                {
                                    pEnumColors.Reset();
                                }
                                ((ISimpleFillSymbol)tempSymbol).Color = pEnumColors.Next();
                                //SF.Color =CreateRGBColor(144, 24, 24, false);
                                if (structCata == "A")
                                {
                                    unRender.AddValue(currentValue, "", tempSymbol);
                                    standardValue = currentValue;
                                }
                                if (structCata == "B")
                                {
                                    unRender.AddReferenceValue(currentValue, standardValue);
                                }
                                unRender.set_Label(currentValue, "A,B|" + Convert.ToString(j));

                            }
                            if (structCata == "C" || structCata == "D")
                            {
                                currentValue = structCata + "|" + Convert.ToString(j);

                                tempLineSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSDashDotDot, CreateRGBColor(0, 0, 0, false) as IColor, 2);
                                tempSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                                ((ISimpleFillSymbol)tempSymbol).Outline = tempLineSymbol as ILineSymbol;
                                if ((pEnumColors.Next()) == null)
                                {
                                    pEnumColors.Reset();
                                }
                                ((ISimpleFillSymbol)tempSymbol).Color = pEnumColors.Next();
                                if (structCata == "C")
                                {
                                    unRender.AddValue(currentValue, "", tempSymbol);
                                    standardValue = currentValue;
                                }
                                if (structCata == "D")
                                {
                                    unRender.AddReferenceValue(currentValue, standardValue);
                                }
                                unRender.set_Label(currentValue, "C,D|" + Convert.ToString(j));
                            }
                            if (structCata == "棚")
                            {
                                currentValue = structCata + "|" + Convert.ToString(j);

                                tempLineSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSSolid, CreateRGBColor(0, 0, 0, false) as IColor, 1);
                                tempSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                                ((ISimpleFillSymbol)tempSymbol).Outline = tempLineSymbol as ILineSymbol;
                                if ((pEnumColors.Next()) == null)
                                {
                                    pEnumColors.Reset();
                                }
                                ((ISimpleFillSymbol)tempSymbol).Color = pEnumColors.Next();
                                unRender.AddValue(currentValue, "", tempSymbol);
                            }
                            if (structCata == "破")
                            {
                                currentValue = structCata + "|" + Convert.ToString(j);

                                tempLineSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSNull, CreateRGBColor(0, 0, 0, false) as IColor, 1);
                                tempSymbol = CreateNewDefaultFillSymbol(200, 200, 100, false);
                                ((ISimpleFillSymbol)tempSymbol).Outline = tempLineSymbol as ILineSymbol;
                                if ((pEnumColors.Next()) == null)
                                {
                                    pEnumColors.Reset();
                                }
                                ((ISimpleFillSymbol)tempSymbol).Color = pEnumColors.Next();
                                unRender.AddValue(currentValue, "", tempSymbol);
                            }
                        }

                    }                 
                }
                if (featureLayer.Name == "植被面")
                {
                    tempSymbol = CreateNewDefaultFillSymbol(197, 250, 203, false);
                    unRender.DefaultSymbol = tempSymbol;
                    unRender.DefaultLabel = "植被";
                    unRender.UseDefaultSymbol = true;
                }
                if (featureLayer.Name == "交通面")
                {
                    tempSymbol = CreateNewDefaultFillSymbol(245, 202, 122, false);
                    unRender.DefaultSymbol = tempSymbol;
                    unRender.DefaultLabel = "交通面";
                    unRender.UseDefaultSymbol = true;
                }
                if (featureLayer.Name == "水系面")
                {
                    tempSymbol = CreateNewDefaultFillSymbol(151, 219, 242, false);
                    unRender.DefaultSymbol = tempSymbol;
                    unRender.DefaultLabel = "水系面";
                    unRender.UseDefaultSymbol = true;
                }
            }
            if (geoRenderType == "Line Symbols")
            {
                tempSymbol = CreateNewLineSymbol(esriSimpleLineStyle.esriSLSSolid, CreateRGBColor(255, 170, 0, false), 3);
                unRender.DefaultSymbol = tempSymbol;
                unRender.DefaultLabel = "道路中心线";
                unRender.UseDefaultSymbol = true;
            }
            IGeoFeatureLayer GeoFeatureLayer = featureLayer as IGeoFeatureLayer;
            GeoFeatureLayer.Renderer = unRender as IFeatureRenderer;
            GeoFeatureLayer.DisplayField = s_filedname;

            //ILayerEffects set_Transparency = GeoFeatureLayer as ILayerEffects; //设置层的透明度
            //set_Transparency.Transparency = 50;

        }

        //创建边框的函数
        private ISymbol CreateNewLineSymbol(esriSimpleLineStyle lineStyle, IColor lineColor, double lineWidth)
        {
            ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
            simpleLineSymbol.Style = lineStyle;
            simpleLineSymbol.Width = lineWidth;
            simpleLineSymbol.Color = lineColor;
            return simpleLineSymbol as ISymbol;
        }
        private ISymbol CreateNewDefaultFillSymbol(int R, int G, int B, bool nullColor)
        {
            ISymbol ssim = new SimpleFillSymbolClass();
            IRgbColor rgbcolor = (IRgbColor)CreateRGBColor(R, G, B, nullColor);
            ((ISimpleFillSymbol)ssim).Color = rgbcolor;
            ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
            simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            //边界
            simpleLineSymbol.Color = CreateRGBColor(100, 100, 100, false);
            ((ISimpleFillSymbol)ssim).Outline = simpleLineSymbol as ILineSymbol;
            return ssim;
        }
        public static IColor CreateRGBColor(int rr, int gg, int bb, bool nullColor)
        {
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = rr;
            rgbColor.Green = gg;
            rgbColor.Blue = bb;
            rgbColor.NullColor = nullColor;
            //rgbColor.Transparency = 0;

            return rgbColor as IColor;
        }
    }
}

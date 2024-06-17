using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
namespace BuildingGen {
    public enum EnvRenderMode {
        仅显示原图层,
        仅显示综合层,
        一起显示
    }
    public enum LayerRenderMode {
        标准化显示,
        背景化显示
    }
    [Flags]
    public enum FeatureRenderMode {
        正常显示 = 0,
        强化分组显示 = 1,
        强化建筑结构显示 = 2,
        强化道路分级显示 = 4
    }

    public static class CityRender {
        static BuildingRenderer brender = new BuildingRenderer();

        public static void RenderLayer(GLayerInfo layerInfo, EnvRenderMode envMode, LayerRenderMode layerMode, FeatureRenderMode featureMode) {
            layerInfo.Layer.Visible = true;

            bool isOrg = (layerInfo.OrgLayer == null);
            if (isOrg && envMode == EnvRenderMode.仅显示综合层) {
                layerInfo.Layer.Visible = false;
                return;
            }
            if (!isOrg && envMode == EnvRenderMode.仅显示原图层) {
                layerInfo.Layer.Visible = false;
                return;
            }

            bool useFill = true;
            bool useLine = true;
            bool isArea = (layerInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon;
            if (isArea) {
                if (envMode == EnvRenderMode.一起显示) {
                    useFill = isOrg;
                    useLine = !isOrg;
                }
            } else {
                useLine = true;
                useFill = false;
            }

            int[] fillValues;
            int[] lineValues;
            switch (layerInfo.LayerType) {
            case GCityLayerType.建筑物:
                if ((featureMode & (FeatureRenderMode.强化分组显示 | FeatureRenderMode.强化建筑结构显示))  == FeatureRenderMode.正常显示) {
                    fillValues = BuildingFillColor;
                    lineValues = BuildingLineColor;
                } else {
                    brender.Mode = featureMode;
                    (layerInfo.Layer as IGeoFeatureLayer).Renderer = brender as IFeatureRenderer;
                    return;
                }
                break;
            case GCityLayerType.道路:
                fillValues = RoadFillColor;
                lineValues = isArea ? RoadLineColor : RoadZXXLineColor;
                break;
            case GCityLayerType.水系:
                fillValues = RiverFillColor;
                lineValues = RiverLineColor;
                break;
            case GCityLayerType.绿化岛:
            case GCityLayerType.植被:
                fillValues = PlantFillColor;
                lineValues = PlantLineColor;
                break;
            case GCityLayerType.铁路:
            case GCityLayerType.高架:
            case GCityLayerType.工矿:
            case GCityLayerType.禁测:
            case GCityLayerType.BRT交通面:            
            default:
                fillValues = new int[] { 0, 0, 80 };
                lineValues = new int[] { 0, 0, 60 };
                break;
            }
            IHsvColor fillColor = null;
            IHsvColor lineColor = null;

            fillColor = HSVColor(fillValues,
                useFill ? (layerMode == LayerRenderMode.标准化显示 ? ColorMode.Normal : ColorMode.Gray) : ColorMode.UnVisable);
            lineColor = HSVColor(lineValues,
                useLine ? (layerMode == LayerRenderMode.标准化显示 ? ColorMode.Normal : ColorMode.Gray) : ColorMode.UnVisable);

            IUniqueValueRenderer render = new UniqueValueRendererClass();
            ISymbol symbol = null;
            if (isArea) {
                ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = fillColor;
                fillSymbol.Color.NullColor = !useFill;
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Color = lineColor;
                lineSymbol.Color.NullColor = !useLine;
                if (!useLine)
                    lineSymbol.Style = esriSimpleLineStyle.esriSLSNull;
                fillSymbol.Outline = lineSymbol;
                if (!useFill)
                    fillSymbol.Style = esriSimpleFillStyle.esriSFSNull;
                symbol = fillSymbol as ISymbol;
            } else {
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                
                if (!useLine)
                    lineSymbol.Style = esriSimpleLineStyle.esriSLSNull;
                if (isOrg) {
                    lineSymbol.Width = 1;
                    
                } else {
                    lineSymbol.Width = 2;
                    //lineColor.Transparency = 50;
                }
                lineSymbol.Color = lineColor;

                lineSymbol.Color.NullColor = !useLine;
                symbol = lineSymbol as ISymbol;
            }
            render.DefaultSymbol = symbol;
            render.UseDefaultSymbol = true;

            if (layerInfo.LayerType == GCityLayerType.道路 && !isOrg && !isArea && ((featureMode & FeatureRenderMode.强化道路分级显示) != 0))
            {
                render.FieldCount = 1;
                render.set_Field(0, "道路等级");
                render.set_FieldType(0, false);
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 3;
                lineSymbol.Color = HSVColor(RoadZXXLineColorRank1, layerMode == LayerRenderMode.标准化显示 ? ColorMode.Normal : ColorMode.Gray);
                render.AddValue("1", "道路等级", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 2;
                //lineSymbol.Style = esriSimpleLineStyle.esriSLSNull;
                lineSymbol.Color = HSVColor(RoadZXXLineColorRank2, layerMode == LayerRenderMode.标准化显示 ? ColorMode.Normal : ColorMode.Gray);
                render.AddValue("2", "道路等级", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 1;
                //lineSymbol.Style = esriSimpleLineStyle.esriSLSNull;
                lineSymbol.Color = HSVColor(RoadZXXLineColorRank3, layerMode == LayerRenderMode.标准化显示 ? ColorMode.Normal : ColorMode.Gray);
                render.AddValue("3", "道路等级", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 1;
                //lineSymbol.Style = esriSimpleLineStyle.esriSLSNull;
                //lineSymbol.Style = esriSimpleLineStyle.esriSLSDash;
                lineSymbol.Color = HSVColor(RoadZXXLineColorRank4, layerMode == LayerRenderMode.标准化显示 ? ColorMode.Normal : ColorMode.Gray);
                render.AddValue("4", "道路等级", lineSymbol as ISymbol);

            }

            (layerInfo.Layer as IGeoFeatureLayer).Renderer = render as IFeatureRenderer;
        }

        public static int[] RiverFillColor = new int[] { 195, 38, 95 };
        public static int[] RiverLineColor = new int[] { 227, 73, 92 };

        public static int[] BuildingFillColor = new int[] { 32, 38, 100 };
        public static int[] BuildingLineColor = new int[] { 0, 0, 0 };

        public static int[] RoadFillColor = new int[] { 60, 55, 100 };
        public static int[] RoadLineColor = new int[] { 0, 0, 80 };
        public static int[] RoadZXXLineColor = new int[] { 9, 93, 98 };
        public static int[] RoadZXXLineColorRank1 = new int[] { 9, 93, 98 };
        public static int[] RoadZXXLineColorRank2 = new int[] { 9, 93, 78 };
        //public static int[] RoadZXXLineColorRank3 = new int[] { 29, 93, 58 };
        public static int[] RoadZXXLineColorRank3 = new int[] { 9, 93, 98 };
        //public static int[] RoadZXXLineColorRank4 = new int[] { 19, 93, 38 };
        public static int[] RoadZXXLineColorRank4 = new int[] { 0, 0, 60 };
        public static int[] PlantFillColor = new int[] { 101, 25, 100 };
        public static int[] PlantLineColor = new int[] { 0, 0, 80 };

        public enum ColorMode {
            Normal,
            Gray,
            UnVisable
        }
        public static IHsvColor HSVColor(int[] color, ColorMode mode) {
            return HSVColor(color[0], color[1], color[2], mode);
        }
        public static IHsvColor HSVColor(int h, int s, int v, ColorMode mode) {
            IHsvColor hsvColor = new HsvColorClass();
            hsvColor.Hue = h;
            hsvColor.Saturation = (mode == ColorMode.Normal) ? s : s / 2;
            hsvColor.Value = (mode == ColorMode.Normal) ? v : ((v < 80) ? 80 : v);
            hsvColor.NullColor = (mode == ColorMode.UnVisable);
            return hsvColor;
        }

    }

}

using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
namespace DatabaseUpdate
{
    public static class RenderForUpdate
    {
        public static void RenderLayer(ILayer layerInfo)
        {
            layerInfo.Visible = true;
            int[] fillValues;
            int[] lineValues;
            bool isArea = (layerInfo as IFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon;
            switch (layerInfo.Name)
            {
                case "房屋面":
                    fillValues = BuildingFillColor;
                    lineValues = BuildingLineColor;
                    break;
                case "道路中心线":
                    fillValues = RoadFillColor;
                    lineValues = RoadZXXLineColor;
                    break;
                case "道路面":
                    fillValues = RoadFillColor;
                    lineValues = RoadLineColor;
                    break;
                case "水系面":
                    fillValues = RiverFillColor;
                    lineValues = RiverLineColor;
                    break;
                case "绿化岛":
                case "植被面":
                    fillValues = PlantFillColor;
                    lineValues = PlantLineColor;
                    break;
                case "铁路":
                case "高架":
                case "工矿":
                case "禁测":
                case "BRT交通面":
                default:
                    fillValues = new int[] { 0, 0, 80 };
                    lineValues = new int[] { 0, 0, 60 };
                    break;
            }
            IHsvColor fillColor = null;
            IHsvColor lineColor = null;

            fillColor = HSVColor(fillValues);
            lineColor = HSVColor(lineValues);

            IUniqueValueRenderer render = new UniqueValueRendererClass();
            ISymbol symbol = null;
            if (isArea)
            {
                ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = fillColor;
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Color = lineColor;
                fillSymbol.Outline = lineSymbol;
                symbol = fillSymbol as ISymbol;
            }
            else
            {
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 1;
                lineSymbol.Color = lineColor;
                symbol = lineSymbol as ISymbol;
            }
            render.DefaultSymbol = symbol;
            render.UseDefaultSymbol = true;

            if (layerInfo.Name == "道路中心线")
            {
                render.FieldCount = 1;
                render.set_Field(0, "道路等级");
                render.set_FieldType(0, false);
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 3;
                lineSymbol.Color = HSVColor(RoadZXXLineColorRank1);
                render.AddValue("1", "道路等级", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 2;
                lineSymbol.Color = HSVColor(RoadZXXLineColorRank2);
                render.AddValue("2", "道路等级", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 1;
                lineSymbol.Color = HSVColor(RoadZXXLineColorRank3);
                render.AddValue("3", "道路等级", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 1;
                lineSymbol.Style = esriSimpleLineStyle.esriSLSDash;
                lineSymbol.Color = HSVColor(RoadZXXLineColorRank4);
                render.AddValue("4", "道路等级", lineSymbol as ISymbol);

            }

            (layerInfo as IGeoFeatureLayer).Renderer = render as IFeatureRenderer;
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
        public static int[] RoadZXXLineColorRank3 = new int[] { 29, 93, 58 };
        public static int[] RoadZXXLineColorRank4 = new int[] { 19, 93, 38 };

        public static int[] PlantFillColor = new int[] { 101, 25, 100 };
        public static int[] PlantLineColor = new int[] { 0, 0, 80 };

        public static IHsvColor HSVColor(int[] color)
        {
            return HSVColor(color[0], color[1], color[2]);
        }
        public static IHsvColor HSVColor(int h, int s, int v)
        {
            IHsvColor hsvColor = new HsvColorClass();
            hsvColor.Hue = h;
            hsvColor.Saturation =  s ;
            hsvColor.Value =  v ;
            hsvColor.NullColor = false;
            return hsvColor;
        }

    }

}

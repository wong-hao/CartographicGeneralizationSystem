using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
namespace BuildingGen {
    internal static class GRenderer {
        public static IColor ColorFromRGB(int r, int g, int b, bool nullColor) {
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = r;
            rgb.Green = g;
            rgb.Blue = b;
            rgb.NullColor = nullColor;
            return rgb;
        }

        static RgbColorClass grayColor;
        static GRenderer() {
            grayColor = new RgbColorClass();
            grayColor.Red = 200;
            grayColor.Blue = 200;
            grayColor.Green = 200;
        }
        internal static void Render(GLayerInfo info) {
            if (info.OrgLayer != null) {
                UniqueValueRendererClass uvr = new UniqueValueRendererClass();
                uvr.set_Field(0, "_GenUsed");
                switch ((info.Layer as IFeatureLayer).FeatureClass.ShapeType) {
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                    SimpleLineSymbolClass sl = new SimpleLineSymbolClass();
                    sl.Color = grayColor;

                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                    break;
                }

            } else {
            }
        }
        internal static ISymbol GetSymbol(GCityLayerType layerType, esriGeometryType geoType, bool used) {
            RgbColorClass lineColor = new RgbColorClass();
            RgbColorClass fillColor = new RgbColorClass();

            switch (layerType) {
            case GCityLayerType.建筑物:
                lineColor.Red = 0;
                lineColor.Blue = 0;
                lineColor.Green = 0;
                lineColor.NullColor = !used;
                fillColor.Red = 0xDF;
                fillColor.Blue = 0xDF;
                fillColor.Green = 0xDF;
                break;
            case GCityLayerType.道路:
                lineColor.Red = 0;
                lineColor.Blue = 0;
                lineColor.Green = 0;
                fillColor.Red = 0xDF;
                fillColor.Blue = 0xDF;
                fillColor.Green = 0xDF;
                break;
            case GCityLayerType.水系:
                lineColor.Red = 0;
                lineColor.Blue = 0xFF;
                lineColor.Green = 0;
                fillColor.Red = 0;
                fillColor.Blue = 0x80;
                fillColor.Green = 0;
                break;
            case GCityLayerType.植被:
                break;
            default:
                break;
            }

            SimpleFillSymbolClass sf = new SimpleFillSymbolClass();

            if (geoType == esriGeometryType.esriGeometryPolyline) {
            }
            SimpleLineSymbolClass sl = new SimpleLineSymbolClass();
            return null;

        }
    }
}

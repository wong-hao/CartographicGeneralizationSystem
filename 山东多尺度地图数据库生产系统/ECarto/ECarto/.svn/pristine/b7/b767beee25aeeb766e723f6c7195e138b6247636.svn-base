using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Display;
using System.Collections;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class RenderFeatureLayer
    {

        //esri自带的单值方法，目前在10.1里面是有内存泄露的Bug
        public static List<string> CalculateUniqueValuesESRI(ICursor cur, string fieldName)
        {
            List<string> strings_ = new List<string>();
            IDataStatistics ds = new DataStatisticsClass();
            ds.Cursor = cur;
            ds.Field = fieldName;
            IEnumerator enumm = ds.UniqueValues;
            enumm.Reset();
            while (enumm.MoveNext())
            {
                strings_.Add(enumm.Current.ToString().Trim());
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ds);

            return strings_;
        }
        //自己重新实现
        public static List<string> CalculateUniqueValuesSMGI(ICursor cur, int fieldIndex)
        {
            List<string> strings_ = new List<string>();
            try
            {
                IRow r = null;
                if (cur != null && fieldIndex >= 0)
                {
                    while ((r = cur.NextRow()) != null)
                    {
                        string val = r.get_Value(fieldIndex).ToString();
                        if (!string.IsNullOrEmpty(val) && !strings_.Contains(val))
                        {
                            strings_.Add(val);
                        }
                    }
                }
            }
            catch
            {

            }
            return strings_;
        }



        /// <summary>
        /// 多值渲染
        /// </summary>
        /// <param name="featureLayer"></param>
        /// <param name="field"></param>
        /// <param name="symDict"></param>
        /// <param name="styleMgr"></param>
        /// <returns></returns>
        public static string UniqueValueRenderLayer(
            IFeatureLayer featureLayer,
            string field,
            Dictionary<string, string> symDict,
            StyleManager styleMgr)
        {
            string msg = "";
            try
            {
                if (featureLayer == null || featureLayer.FeatureClass == null || field == string.Empty || symDict == null || symDict.Count < 1)
                {
                    throw new Exception("请确保所有的输入参数均真实有效！");
                }

                IFeatureClass featCls = featureLayer.FeatureClass;
                if (featCls.FindField(field) < 0)
                {
                    throw new Exception("不存在名为: " + field + " 的字段");
                }
                IField targetField = featCls.Fields.get_Field(featCls.FindField(field));
                bool fieldisString = (targetField.Type == esriFieldType.esriFieldTypeString);
                IGeoFeatureLayer geoFeatLayer = featureLayer as IGeoFeatureLayer;
                IUniqueValueRenderer uniqueRender = new UniqueValueRendererClass();

                uniqueRender.FieldCount = 1;
                uniqueRender.set_Field(0, field);
                uniqueRender.set_FieldType(0, fieldisString);

                //uniqueRender.UseDefaultSymbol = true;

                if (symDict.Count > 0)
                {
                    string symType = SymbolClassString.SymbolClassStrByGeometryType(featCls.ShapeType);
                    ISymbol sym = null;
                    foreach (string s in symDict.Keys)
                    {

                        if ((sym = styleMgr.getSymbol(styleMgr.DefaultStylePath, symType, symDict[s])) != null)
                        {
                            uniqueRender.AddValue(s, field, sym);
                            string sb = "";
                            if ((sb = styleMgr.getLabel(s)) != "")
                            {
                                uniqueRender.set_Label(s, s + " " + sb);
                            }
                            else
                            {
                                uniqueRender.set_Label(s, s + " 非标编码");
                            }
                        }
                        else
                        {
                            sym = RenderFeatureLayer.createErrorSimpleSymbol(featCls.ShapeType);
                            if (sym != null)
                            {
                                uniqueRender.AddValue(s, field, sym);
                                string sb = "";
                                if ((sb = styleMgr.getLabel(s)) != "")
                                {
                                    uniqueRender.set_Label(s, s + " " + sb + " 符号未定义");
                                }
                                else
                                {
                                    uniqueRender.set_Label(s, s + " 非标编码");
                                }
                            }

                        }
                    }
                    geoFeatLayer.Renderer = uniqueRender as IFeatureRenderer;
                    IUID pUID = new UIDClass();
                    pUID.Value = "{683C994E-A17B-11D1-8816-080009EC732A}";
                    geoFeatLayer.RendererPropertyPageClassID = pUID as UIDClass;
                }

            }
            catch (Exception eex)
            {
                msg = eex.Message;
            }

            return msg;
        }

        public static void SimpleRenderLayer(IFeatureLayer fl, ISymbol sym, string label)
        {
            if (fl != null)
            {
                ISimpleRenderer sm = new SimpleRendererClass();
                sm.Symbol = sym;
                sm.Label = label;
                (fl as IGeoFeatureLayer).Renderer = sm as IFeatureRenderer;
            }
        }


        public static ISymbol createErrorSimpleSymbol(esriGeometryType t)
        {
            ISymbol s = null;
            switch (t)
            {
                case esriGeometryType.esriGeometryPoint:
                    s = createSimpleMarkerSymbol(
                        0,
                        createRGBColor(255, 0, 0),
                        false, createRGBColor(255, 0, 0), 0, 1, esriSimpleMarkerStyle.esriSMSDiamond, 0, 0);
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    s = createSimpleFillSymbol(createRGBColor(255, 0, 0),
                        (ILineSymbol)createSimpleLineSymbol(createRGBColor(255, 0, 0), esriSimpleLineStyle.esriSLSDot, 0.5), esriSimpleFillStyle.esriSFSNull);
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    s = createSimpleLineSymbol(createRGBColor(255, 0, 0), esriSimpleLineStyle.esriSLSDash, 0.5);
                    break;
                default:
                    s = null;
                    break;
            }
            return s;
        }

        public static ISymbol createDefaultSimpleSymbol(esriGeometryType t)
        {
            ISymbol s = null;
            switch (t)
            {
                case esriGeometryType.esriGeometryPoint:
                    s = createSimpleMarkerSymbol(
                        0,
                        createRGBColor(104, 104, 104),
                        false, createRGBColor(104, 104, 104), 0, 1, esriSimpleMarkerStyle.esriSMSSquare, 0, 0);
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    s = createSimpleFillSymbol(createRGBColor(153, 179, 179),
                        (ILineSymbol)createSimpleLineSymbol(createRGBColor(104, 104, 104), esriSimpleLineStyle.esriSLSSolid, 0.5),
                        esriSimpleFillStyle.esriSFSSolid);
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    s = createSimpleLineSymbol(createRGBColor(104, 104, 104), esriSimpleLineStyle.esriSLSSolid, 0.5);
                    break;
                default:
                    s = null;
                    break;
            }
            return s;
        }





        public static ISymbol createSimpleMarkerSymbol(
            double angle, IColor clr, bool outLine, IColor otClr,
            double OtLSize, double size, esriSimpleMarkerStyle style, double xoff,
            double yoff)
        {
            ISimpleMarkerSymbol sms = new SimpleMarkerSymbolClass();
            sms.Angle = angle;
            sms.Color = clr;
            sms.Outline = outLine;
            sms.OutlineColor = otClr;
            sms.OutlineSize = OtLSize;
            sms.Size = size;
            sms.Style = style;
            sms.XOffset = xoff;
            sms.YOffset = yoff;
            return sms as ISymbol;
        }



        public static ISymbol createSimpleLineSymbol(IColor clr, esriSimpleLineStyle style, double width)
        {
            ISimpleLineSymbol sls = new SimpleLineSymbolClass();
            sls.Color = clr;
            sls.Style = style;
            sls.Width = width;
            return sls as ISymbol;
        }

        public static ISymbol createSimpleFillSymbol(
            IColor clr, ILineSymbol ls, esriSimpleFillStyle style
            )
        {
            ISymbol sym = new SimpleFillSymbolClass();
            (sym as ISimpleFillSymbol).Color = clr;
            (sym as ISimpleFillSymbol).Outline = ls;
            (sym as ISimpleFillSymbol).Style = style;

            return sym;
        }


        public static IColor createRGBColor(int r, int g, int b, byte transparency = 255)
        {
            IColor clr = new RgbColorClass();
            (clr as IRgbColor).Red = r;
            (clr as IRgbColor).Green = g;
            (clr as IRgbColor).Blue = b;
            (clr as IRgbColor).Transparency = transparency;
            return clr;

        }

        public static IColor createCMYKColor(int c, int m, int y, int k)
        {
            IColor clr = new CmykColorClass();
            (clr as ICmykColor).Cyan = c;
            (clr as ICmykColor).Magenta = m;
            (clr as ICmykColor).Yellow = y;
            (clr as ICmykColor).Black = k;
            return clr;
        }

        public static void FastRenderLayer()
        {


        }








    }
}

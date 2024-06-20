using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Collections;
using ESRI.ArcGIS.esriSystem;
using System.Collections.Generic;
using stdole;
using System.Data;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Runtime.InteropServices;
namespace SMGI.Plugin.EmergencyMap.MapDeOut
{
    public class AnnoGeometryHelper
    {
        public static IGeometry AnnoGeometry(string txt, IPoint ShapePosition, ITextSymbol sym)
        {
            var dc = DCHelper.GetDC(IntPtr.Zero);

            var ts = sym as IQueryGeometry;

            IGeometry geo = ts.GetGeometry(dc.ToInt32(), GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation, ShapePosition);

            DCHelper.ReleaseDC(IntPtr.Zero, dc);
            return geo;
        }
        public static IGeometry AnnoGeometry(string txt,double size,string fontname="宋体")
        {
            var dc = DCHelper.GetDC(IntPtr.Zero);
            IPoint ShapePosition = new PointClass { X = 0, Y = 0 };
            var el = new TextElementClass
            {
                Text = txt,
                FontName = fontname,
                Size = size * 2.83,
                //Italic = this.Font.Italic,
                //Bold = this.Font.Bold,
                AnchorPoint = esriAnchorPointEnum.esriBottomLeftCorner,
                VerticalAlignment = esriTextVerticalAlignment.esriTVACenter,
                HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter,
                Geometry = ShapePosition,
                CharacterWidth = 0,
                WordSpacing = 0
            };
            var ts = el.Symbol as IQueryGeometry;

            IGeometry geo = ts.GetGeometry(dc.ToInt32(), GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation, ShapePosition);
            Marshal.ReleaseComObject(el);
            DCHelper.ReleaseDC(IntPtr.Zero, dc);
            return geo;
        }

        public static ICmykColor GetColorByString(string cmyk)
        {
            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 100;
            try
            {
                for (int i = 0; i <= cmyk.Length; i++)
                {
                    if (i == cmyk.Length)
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('C'))
                        {
                            CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('M'))
                        {
                            CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('Y'))
                        {
                            CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('K'))
                        {
                            CMYK_Color.Black = int.Parse(sbs.Substring(1));
                        }
                        break;
                    }
                    else
                    {
                        char C = cmyk[i];
                        if (D.Contains(C))
                        {
                            sb.Append(C);
                        }
                        else
                        {
                            string sbs = sb.ToString();
                            if (sbs.Contains('C'))
                            {
                                CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('M'))
                            {
                                CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('Y'))
                            {
                                CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('K'))
                            {
                                CMYK_Color.Black = int.Parse(sbs.Substring(1));
                            }
                            sb.Clear();
                            sb.Append(C);
                        }
                    }
                }
                return CMYK_Color;
            }
            catch
            {
                return null;
            }

        }

        public static string GetStringByColor(IColor color)
        {
            ICmykColor cmykColor = new CmykColorClass { CMYK = color.CMYK };
            string cmykString = string.Empty;
            if (cmykColor.Cyan != 0)
                cmykString += "C" + cmykColor.Cyan.ToString();
            if (cmykColor.Magenta != 0)
                cmykString += "M" + cmykColor.Magenta.ToString();
            if (cmykColor.Yellow != 0)
                cmykString += "Y" + cmykColor.Yellow.ToString();
            if (cmykColor.Black != 0)
                cmykString += "K" + cmykColor.Black.ToString();
            return cmykString;
        }
    }

    public class DCHelper
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr hdc);
    } 

}

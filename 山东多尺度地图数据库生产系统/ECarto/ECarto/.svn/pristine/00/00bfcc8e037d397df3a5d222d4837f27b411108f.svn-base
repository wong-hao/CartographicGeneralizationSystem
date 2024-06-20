using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using System.Drawing;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
namespace SMGI.Plugin.EmergencyMap
{
    //完成.net 颜色到arcgis 颜色转换
    public class ColorHelper
    {

        public void RGBToHSV(int r, int g, int b)
        {
            System.Drawing.Color rgbcolor = System.Drawing.Color.FromArgb(r, g, b);
            float h = rgbcolor.GetHue();
            float s = rgbcolor.GetSaturation();
            float v = rgbcolor.GetBrightness();
        }
        public static Color ConvertIColorToColor(IColor pRgbColor)
        {
            return ColorTranslator.FromOle(pRgbColor.RGB);
        }
        //CMYK转.net color
        public static Color ConvertICMYKColorToColor(ICmykColor pcmyk)
        {
            double c = pcmyk.Cyan;
            double m = pcmyk.Magenta;
            double y = pcmyk.Yellow;
            double k = pcmyk.Black;
            double r = 255 * (100 - c) * (100 - k) * 1e-4;
            double g = 255 * (100 - m) * (100 - k) * 1e-4;
            double b = 255 * (100 - y) * (100 - k) * 1e-4;
            IRgbColor rgb = new RgbColorClass();
            rgb.Red = (int)r;
            rgb.Green = (int)g;
            rgb.Blue = (int)b;
            IColor pcolor = (rgb as IClone).Clone() as IColor;
            return ConvertIColorToColor(pcolor);
        }

        //cmyk字符转windowcolor
        public static Color GetColorByCmykStr(string cmyk)
        {
            var icmk = GetColorByString(cmyk);
            return ConvertICMYKColorToColor(icmk);
        }
        //windowcolor转cmyk字符
        public static string GetCmykStrByColor(Color p_Color)
        {
            var icmk = ConvertColorToCMYK(p_Color);
            return "C"+ icmk.Cyan + "M" + icmk.Magenta + "Y" + icmk.Yellow + "K" + icmk.Black;
           
        }


        // cmyk字符串（形如：C100M200Y100K50）
        public static ICmykColor GetColorByString(string cmyk)
        {
            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;
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

        //.net color转为ICMYK
        public static ICmykColor ConvertColorToCMYK(Color p_Color)
        {
            IRgbColor pColor = new RgbColorClass { RGB = p_Color.B * 65536 + p_Color.G * 256 + p_Color.R };
            ICmykColor pcmyk = ConvertRGBToCMYK(pColor);
            return pcmyk;
        }
        //.net color转为IRGB
        public static IRgbColor ConvertColorToIColor(Color p_Color)
        {
            IRgbColor pColor = new RgbColorClass { RGB = p_Color.B * 65536 + p_Color.G * 256 + p_Color.R };
            return pColor;
        }
        //IRGB转ICMYK
        public static ICmykColor ConvertRGBToCMYK(IRgbColor rgb)
        {
            ICmykColor pcolor = new CmykColorClass();
            double c = (double)(255 - rgb.Red) / 255;
            double m = (double)(255 - rgb.Green) / 255;
            double y = (double)(255 - rgb.Blue) / 255;
            double k = (double)Math.Min(c, Math.Min(m, y));
            if (k == 1.0)
            {
                c = m = y = 0;
                k = 0.6;
            }
            else
            {
                c = (c - k) / (1 - k);
                m = (m - k) / (1 - k);
                y = (y - k) / (1 - k);
            }
            c *= 100;
            m *= 100;
            y *= 100;
            k *= 100;
            pcolor.Cyan = (int)c;
            pcolor.Magenta = (int)m;
            pcolor.Yellow = (int)y;
            pcolor.Black = (int)k;

            return pcolor;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);//返回设备场景句柄
        [DllImport("gdi32.dll")]
        private static extern int GetPixel(IntPtr hdc, Point p);//取指定点颜色
        /// <summary>
        /// 拾取颜色
        /// 示例：ColorHelper.PickColor(x, y, m_Application.ActiveView.ScreenDisplay.hWnd)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="hWnd">0时为全屏的设备场景</param>
        /// <returns></returns>
        public static IColor PickColor(int x, int y, int hWnd = 0)
        {
            IColor clr = null;

            IntPtr hdc = GetDC(new IntPtr(hWnd));//取到设备场景

            Point p = new Point(x, y);
            int c = GetPixel(hdc, p);//取指定点颜色
            int r = (c & 0xFF);
            int g = (c & 0xFF00) / 256;
            int b = (c & 0xFF0000) / 65536;

            clr = new RgbColorClass(){ Red = r, Green = g, Blue =b};

            return clr;
        }

    }

    //境界颜色
    public class BOUAColor
    {
        public int FID;
        public string Name;
        public string ColorPlan;//颜色方案
        public string ColorIndex;//颜色索引
        public CmykColorClass CmykColor;//颜色；
    }
}

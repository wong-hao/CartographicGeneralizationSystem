using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Common
{
    public class FastDrawer:IDisposable
    {
        //public IActiveView ActiveView { get; set; }
        public IDisplay Display { get; set; }
        public SimpleMarkerSymbolClass SMS { get; set; }
        public SimpleLineSymbolClass SLS { get; set; }
        public SimpleFillSymbolClass SFS { get; set; }
        public TextSymbolClass TS { get; set; }
        bool defaultStarted;
        public FastDrawer(IDisplay dis, bool started = false)
        {
            defaultStarted = started;
            Display = dis;
            if(!started)
                Display.StartDrawing(0, -1);
            SMS = new SimpleMarkerSymbolClass();
            SLS = new SimpleLineSymbolClass();
            SFS = new SimpleFillSymbolClass();
            TS = new TextSymbolClass();
        }

        public void Draw(IGeometry geo, ISymbol symbol)
        {
            if (geo is IPoint)
            {
                Display.SetSymbol(symbol);
                Display.DrawPoint(geo);
            }
            else if (geo is IPolyline)
            {
                Display.SetSymbol(symbol);
                Display.DrawPolyline(geo);
            }
            else if (geo is IPolygon)
            {
                Display.SetSymbol(symbol);
                Display.DrawPolygon(geo);
            }
        }

        public void Draw(IGeometry geo)
        {
            if (geo is IPoint)
            {
                Display.SetSymbol(SMS);
                Display.DrawPoint(geo);
            }
            else if (geo is IPolyline)
            {
                Display.SetSymbol(SLS);
                Display.DrawPolyline(geo);
            }
            else if (geo is IPolygon)
            {
                Display.SetSymbol(SFS);
                Display.DrawPolygon(geo);
            }
        }
        public void DrawText(IGeometry geo, string str)
        {
            Display.SetSymbol(TS);
            Display.DrawText(geo, str);
        }
        public static IColor RGBColor(int r,int g,int b)
        {
            RgbColorClass c = new RgbColorClass();
            c.Red = r;
            c.Green = g;
            c.Blue = b;
            return c;
        }

        /// <summary>
        /// HSV
        /// </summary>
        /// <param name="h">0-360</param>
        /// <param name="s">0-100</param>
        /// <param name="v">0-100</param>
        /// <returns>color</returns>
        public static IColor HSVColor(int h, int s, int v)
        {
            HsvColorClass c = new HsvColorClass();
            c.Hue = h;
            c.Saturation = s;
            c.Value = v;
            return c;
        }
        public void Dispose()
        {
            if(!defaultStarted)
            Display.FinishDrawing();
        }
    }
}

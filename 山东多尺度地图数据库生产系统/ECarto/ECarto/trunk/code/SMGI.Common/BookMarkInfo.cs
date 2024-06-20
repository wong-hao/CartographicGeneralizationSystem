using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMGI.Common
{
    [Serializable]
    public class BookMarkInfo
    {
        internal Bitmap bitmap;
        internal double xmin, xmax, ymin, ymax;
        internal DateTime time;
        internal string name;
        public BookMarkInfo(IActiveView view)
        {
            bitmap = new Bitmap(240, 160);
            time = DateTime.Now;
            name = time.ToString();
            xmin = view.Extent.XMin;
            xmax = view.Extent.XMax;
            ymin = view.Extent.YMin;
            ymax = view.Extent.YMax;
            tagRECT rect = new tagRECT();
            rect.left = 0;
            rect.right = 240;
            rect.top = 0;
            rect.bottom = 160;
            System.Drawing.Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            view.Output(g.GetHdc().ToInt32(), 0, ref rect, null, null);
            g.Dispose();
        }
        public override string ToString()
        {
            return name.ToString();
        }
        public IEnvelope Envelope
        {
            get
            {
                EnvelopeClass env = new EnvelopeClass();
                env.XMin = xmin;
                env.XMax = xmax;
                env.YMin = ymin;
                env.YMax = ymax;
                return env;
            }
        }

    }

}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
namespace BuildingGen {
    public class BookMark : BaseGenCommand {
        public BookMark() {
            base.m_category = "GSystem";
            base.m_caption = "书签管理";
            base.m_message = "管理视图书签";
            base.m_toolTip = "道路自动选取";
            base.m_name = "BookMark";
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }
        BookMarkDlg dlg;
        public override void OnClick() {
            if (dlg == null || dlg.IsDisposed) {
                dlg = new BookMarkDlg(m_application);
            }
            dlg.Show();
        }
    }
    [Serializable]
    public class BookMarkInfo {
        internal Bitmap bitmap;
        internal double xmin, xmax, ymin, ymax;
        internal DateTime time;
        public BookMarkInfo(IActiveView view) {
            bitmap = new Bitmap(240, 160);
            time = DateTime.Now;
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
        public override string ToString() {
            return time.ToString();
        }
        public IEnvelope Envelope {
            get {
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

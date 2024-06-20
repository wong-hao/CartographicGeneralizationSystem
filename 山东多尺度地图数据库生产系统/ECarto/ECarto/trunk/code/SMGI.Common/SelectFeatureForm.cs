using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using SMGI.Common;

namespace SMGI.Common
{
    public partial class SelectFeatureForm : Form
    {
        IActiveView view;
        SimpleMarkerSymbolClass sms;
        SimpleFillSymbolClass sfs;
        SimpleLineSymbolClass sls;
        Timer t;
        public SelectFeatureForm(IActiveView v)
        {
            InitializeComponent();
            view = v;
            sms = new SimpleMarkerSymbolClass();
            sfs = new SimpleFillSymbolClass();
            sls = new SimpleLineSymbolClass();

            RgbColorClass c = new RgbColorClass();
            c.Red = 0;
            c.Blue = 255;
            c.Green = 0;

            sls.Width = 2;
            sls.Color = c;

            sms.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            sls.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            sfs.ROP2 = esriRasterOpCode.esriROPNotXOrPen;

            sfs.Outline = sls;
            t = new Timer();
            t.Interval = 300;
            t.Tick += new EventHandler(t_Tick);
        }

        void t_Tick(object sender, EventArgs e)
        {

            var fw = FeaturesListBox.SelectedItem as FeatureWarp;
            var geo = fw.Feature.Shape;

            var dis = view.ScreenDisplay;

                dis.StartDrawing(dis.hDC, (short)ESRI.ArcGIS.Display.esriScreenCache.esriNoScreenCache);

                if (geo is IPoint)
                {
                    dis.SetSymbol(sms);
                    for (int i = 0; i < 6; i++)
                    {
                        dis.DrawPoint(geo);
                        System.Threading.Thread.Sleep(200);
                        dis.DrawPoint(geo);
                    }
                                        
                }
                else if (geo is IPolyline)
                {
                    dis.SetSymbol(sls);
                    dis.DrawPolyline(geo);
                    System.Threading.Thread.Sleep(200);
                    dis.DrawPolyline(geo);

                }
                else if (geo is IPolygon)
                {
                    dis.SetSymbol(sfs);

                    dis.DrawPolygon(geo);
                    System.Threading.Thread.Sleep(200);
                    dis.DrawPolygon(geo);

                }

                dis.FinishDrawing();
            t.Stop();
        }


        private void lbFeatures_SelectedIndexChanged(object sender, EventArgs e)
        {
            var fw = FeaturesListBox.SelectedItem as FeatureWarp;
            if (fw == null)
                return;
            var env = fw.Feature.Shape.Envelope;
            var pt = new PointClass();
            pt.X = (env.XMax + env.XMin) / 2;
            pt.Y = (env.YMax + env.YMin) / 2;
            
            env.Width *= 2;
            env.Height *= 2;
            env = view.Extent;
            env.CenterAt(pt);

            t.Start();
        }

        private void SelectFeatureForm_Load(object sender, EventArgs e)
        {
            IMap map = view.FocusMap;
            IEnumFeature ef = map.FeatureSelection as IEnumFeature;
            ef.Reset();
            IFeature f = null;
            List<FeatureWarp> fs = new List<FeatureWarp>();
            while ((f = ef.Next()) != null)
            {
                fs.Add(new FeatureWarp(f));
            }
            FeaturesListBox.Items.AddRange(fs.ToArray());

            if (FeaturesListBox.Items.Count > 0)
                FeaturesListBox.SelectedIndex = 0;
        }

        private void lbFeatures_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }

    public class FeatureWarp
    {
        public FeatureWarp(IFeature f)
        {
            Feature = f;
        }
        public IFeature Feature { get; set; }
        public override string ToString()
        {
            if (State == -3)
            {
                return string.Format("{0}-{1}", Feature.Class.AliasName, Feature.OID);
            }
            else
            {
                return string.Format("{0}-{1}=（{2}）", Feature.Class.AliasName, Feature.OID,State); 
            }
        }
        public int State {             
            get{
                int state = -3;
                //IField fld = Fields.FindField(cmdUpdateRecord.CollabVERSION);
                int fldIdx = Feature.Fields.FindField("SMGIVERSION");
                if (fldIdx >= 0)
                {
                    try
                    {
                        state = Int32.Parse(Feature.get_Value(fldIdx).ToSafeString());
                    }
                    catch (Exception ex)
                    { }                    
                }
                return state;
            } 
        }
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

namespace DomapTool {
    public partial class SelectForm : Form {
        public List<IFeature> Features { get; private set; }
        List<IGeometry> Geometrys;
        PolylineClass line;
        public List<bool> FeatureIsSelect { get; private set; }
        SimpleFillSymbolClass sfs = new SimpleFillSymbolClass();
        SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
        SimpleMarkerSymbolClass sms = new SimpleMarkerSymbolClass();
        int[,] position;
        public SelectForm() {
            InitializeComponent();
            Features = new List<IFeature>();
            Geometrys = new List<IGeometry>();
            FeatureIsSelect = new List<bool>();
            axMapControl1.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(axMapControl1_OnAfterDraw);
            axMapControl1.OnMouseDown += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnMouseDownEventHandler(axMapControl1_OnMouseDown);
            //axMapControl1.OnDoubleClick += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnDoubleClickEventHandler(axMapControl1_OnDoubleClick);
            axMapControl1.OnKeyDown += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnKeyDownEventHandler(axMapControl1_OnKeyDown);
            position = new int[,] {{-1,-1},{0,-1},{1,-1},
                                   {1, 0},        {1,1},
                                   {0,1},{-1,1},{-1,0}};
            this.Text = "选择要素";
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 255;
            rgb.Green = 0;
            rgb.Blue = 0;
            sls.Color = rgb;
            sls.Width = 2;
        }

        void axMapControl1_OnKeyDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnKeyDownEvent e) {
            switch (e.keyCode) {
                case (int)Keys.Enter:
                    DialogResult = DialogResult.OK;
                    break;
                case (int) Keys.Escape:
                    DialogResult = DialogResult.Cancel;
                    break;
                default:
                    break;
            }
        }

        void axMapControl1_OnDoubleClick(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnDoubleClickEvent e) {
            DialogResult = DialogResult.OK;
        }

        void axMapControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e) {
            if (e.button == 4)
                return;
            if (e.button == 1) {
                IPoint p = new PointClass();
                p.X = e.mapX; p.Y = e.mapY;
                for (int i = 0; i < Geometrys.Count; i++) {
                    IRelationalOperator rel = Geometrys[i] as IRelationalOperator;
                    if (Geometrys[i].GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        ITopologicalOperator d = Geometrys[i] as ITopologicalOperator;
                        d.Simplify();
                        rel = d.Buffer(5) as IRelationalOperator;
                    }
                    if (rel.Contains(p)) {
                        FeatureIsSelect[i] = !(FeatureIsSelect[i]);
                        break;
                    }
                }
                axMapControl1.Refresh();
            }
            else {
                for (int i = 0; i < FeatureIsSelect.Count; i++) {
                    FeatureIsSelect[i] = !FeatureIsSelect[i];
                }
                axMapControl1.Refresh();
            }
        }

        void axMapControl1_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            IDisplay dis = e.display as IDisplay;
            ISymbol s = null;
            ISymbol s2 = null;
            Action<IGeometry> act = null;
            

            for (int i = 0; i < Features.Count; i++) {
                IFeature item = Features[i];
                switch (item.Shape.GeometryType) {
                    case esriGeometryType.esriGeometryAny:
                        break;
                    case esriGeometryType.esriGeometryBag:
                        break;
                    case esriGeometryType.esriGeometryBezier3Curve:
                        break;
                    case esriGeometryType.esriGeometryCircularArc:
                        break;
                    case esriGeometryType.esriGeometryEllipticArc:
                        break;
                    case esriGeometryType.esriGeometryEnvelope:
                        break;
                    case esriGeometryType.esriGeometryLine:
                        break;
                    case esriGeometryType.esriGeometryMultiPatch:
                        break;
                    case esriGeometryType.esriGeometryMultipoint:
                        break;
                    case esriGeometryType.esriGeometryNull:
                        break;
                    case esriGeometryType.esriGeometryPath:
                        break;
                    case esriGeometryType.esriGeometryPoint:
                        s2 = sms.Clone() as ISymbol;
                        s = sms as ISymbol;
                        act = new Action<IGeometry>(dis.DrawPoint);
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        s2 = sfs.Clone() as ISymbol;
                        (s2 as ISimpleFillSymbol).Style = esriSimpleFillStyle.esriSFSNull;
                        s = sfs as ISymbol;
                        act = new Action<IGeometry>(dis.DrawPolygon);
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        s2 = sfs.Clone() as ISymbol;
                        s = sls as ISymbol;
                        act = new Action<IGeometry>(dis.DrawPolyline);
                        break;
                    case esriGeometryType.esriGeometryRay:
                        break;
                    case esriGeometryType.esriGeometryRing:
                        break;
                    case esriGeometryType.esriGeometrySphere:
                        break;
                    case esriGeometryType.esriGeometryTriangleFan:
                        break;
                    case esriGeometryType.esriGeometryTriangleStrip:
                        break;
                    case esriGeometryType.esriGeometryTriangles:
                        break;
                    default:
                        break;
                }
                dis.SetSymbol(s);
                act(item.Shape);
                if (FeatureIsSelect[i]) {

                    dis.SetSymbol(s);
                }
                else {
                    dis.SetSymbol(s2);
                }
                act(Geometrys[i]);

            }
            if (Geometrys[0].GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                dis.SetSymbol(s2);
            }
            else
            {
                dis.SetSymbol(sls);
            }
            dis.DrawPolyline(line);
        }


        private void SelectForm_Load(object sender, EventArgs e) {
            Geometrys.Clear();
            FeatureIsSelect.Clear();
            if (Features.Count == 0) {
                return;
            }
            IEnvelope env = null;
            foreach (var item in Features) {
                if (env == null) {
                    env = item.Shape.Envelope;
                }
                else {
                    env.Union(item.Shape.Envelope);
                }
            }
            line = new PolylineClass();
            for (int i = 0; i < Features.Count; i++) {
                IFeature item = Features[i];

                double dx = position[i%8, 0] * (env.Width * 2);
                double dy = position[i%8, 1] * (env.Height * 2);

                ITransform2D tras = item.ShapeCopy as ITransform2D;
                tras.Move(dx, dy);
                Geometrys.Add(tras as IGeometry);
                object miss = Type.Missing;
                PathClass path = new PathClass();
                if (Geometrys[0].GeometryType != esriGeometryType.esriGeometryPolyline)
                {
                    path.AddPoint((item.Shape.Envelope as IArea).LabelPoint, ref miss, ref miss);
                    path.AddPoint(((tras as IGeometry).Envelope as IArea).LabelPoint, ref miss, ref miss);
                }
                else
                {
                    IPoint midPoi=new PointClass();
                    (item.Shape as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, midPoi);
                    path.AddPoint(midPoi, ref miss, ref miss);
                    (tras as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, midPoi);
                    path.AddPoint(midPoi, ref miss, ref miss);
                }
                line.AddGeometry(path, ref miss, ref miss);
                FeatureIsSelect.Add(true);
            }
            env.Expand(6, 6, true);
            //env.Expand(20, 20, false);
            axMapControl1.Extent = env;
            axMapControl1.Refresh();
        }

    }
}

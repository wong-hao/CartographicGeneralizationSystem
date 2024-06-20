using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using GENERALIZERLib;

namespace BuildingGen {
    public class BuildingSelect : BaseGenTool{
        Generalizer gen;
        INewPolygonFeedback fb;
        public BuildingSelect()
        {
            base.m_category = "GBuilding";
            base.m_caption = "选取夸大";
            base.m_message = "建筑物选取夸大";
            base.m_toolTip = "建筑物选取，并对保留的建筑物进行夸大处理";
            base.m_name = "BuildingSelect";
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("建筑物选取比例",(double)0.2)
                ,new GenDefaultPara("建筑物最小上图面积",(double)100)
            };
            gen = new Generalizer();
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;

            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb == null)
            {
                fb = new NewPolygonFeedbackClass();
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                fb.Start(p);
            }
            else
            {
                fb.AddPoint(p);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (fb != null)
            {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }

        public override void OnDblClick()
        {
            /*if (System.Windows.Forms.MessageBox.Show("是否对全图进行建筑物选取夸大操作", "请确认耗时操作", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }*/
            if (fb == null)
                return;
            IPolygon p = fb.Stop();
            fb = null;
            GLayerInfo layer = GetLayer(this.m_application.Workspace);
            if (layer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到建筑物图层");
                return;
            }
            
            gen.InitGeneralizer(2000, 10000);

            WaitOperation wo = m_application.SetBusy(true);
            double minArea = (double)m_application.GenPara["建筑物最小上图面积"];
            IFeatureClass fc = (layer.Layer as IFeatureLayer).FeatureClass;
            IFeature f = null;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.Geometry = p;
            IFeatureCursor fcursor = fc.Update(sf, true);
            List<int> features = new List<int>();
            MultipointClass mp = new MultipointClass();
            object miss = Type.Missing;
            wo.SetText("正在准备数据");
            int wCount = fc.FeatureCount(sf);

            while ((f = fcursor.NextFeature()) != null)
            {
                wo.Step(wCount);
                if (f.Shape.IsEmpty)
                {
                    fcursor.DeleteFeature();
                    continue;
                }
                if ((f.Shape as IArea).Area < minArea)
                {
                    features.Add(f.OID);
                    mp.AddGeometry((f.Shape as IArea).Centroid, ref miss, ref miss);
                }
            }
            fcursor.Flush();
            wo.SetText("正在进行选取");
            IBooleanArray ba = gen.MultiPointSelection(mp, (double)m_application.GenPara["建筑物选取比例"]);

            double minWidth = Math.Sqrt(minArea / 15) * 3;
            double minHeight = Math.Sqrt(minArea / 15) * 5;
            wCount = ba.Count;
            wo.SetText("正在夸大");
            wo.Step(0);
            for (int i = 0; i < ba.Count; i++)
            {
                f = fc.GetFeature(features[i]);
                wo.Step(wCount);
                if (ba.get_Item(i) && !f.Shape.IsEmpty && (f.Shape as IArea).Area > 1)
                {
                    MBR mbr = MBR.GetMBR(f.Shape as IPolygon);
                    bool widthBigger = (mbr.env.Width > mbr.env.Height);
                    double dx = widthBigger ? (mbr.env.Width > minHeight ? 0 : (minHeight - mbr.env.Width) / 2) : (mbr.env.Width > minWidth ? 0 : (minWidth - mbr.env.Width) / 2);
                    double dy = (!widthBigger) ? (mbr.env.Height > minHeight ? 0 : (minHeight - mbr.env.Height) / 2) : (mbr.env.Height > minWidth ? 0 : (minWidth - mbr.env.Height) / 2);
                    mbr.env.Expand(dx, dy, false);
                    f.Shape = mbr.Shape;
                    f.Store();
                }
                else
                {
                    f.Delete();
                }
            }
            m_application.SetBusy(false);
            m_application.MapControl.Refresh();
        }

        /*public override void OnClick() {
            if (System.Windows.Forms.MessageBox.Show("是否对全图进行建筑物选取夸大操作", "请确认耗时操作", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) {
                return;
            }
            GLayerInfo layer = GetLayer(this.m_application.Workspace);
            if (layer == null) {
                System.Windows.Forms.MessageBox.Show("没有找到建筑物图层");
                return;
            }
            gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 2000, 10000);

            WaitOperation wo = m_application.SetBusy(true);
            double minArea = (double)m_application.GenPara["建筑物最小上图面积"];
            IFeatureClass fc = (layer.Layer as IFeatureLayer).FeatureClass;
            IFeature f = null;
            IFeatureCursor fcursor = fc.Update(null, true);
            List<int> features = new List<int>();
            MultipointClass mp = new MultipointClass();
            object miss = Type.Missing;
            wo.SetText("正在准备数据");
            int wCount = fc.FeatureCount(null);

            while ((f =  fcursor.NextFeature())!=null) {
                wo.Step(wCount);
                if (f.Shape.IsEmpty) {
                    fcursor.DeleteFeature();
                    continue;
                }
                if ((f.Shape as IArea).Area < minArea) {
                    features.Add(f.OID);
                    mp.AddGeometry((f.Shape as IArea).Centroid,ref miss,ref miss);
                }
            }
            fcursor.Flush();
            wo.SetText("正在进行选取");
            IBooleanArray ba = gen.MultiPointSelection(mp, (double)m_application.GenPara["建筑物选取比例"]);

            double minWidth = Math.Sqrt(minArea / 15) * 3;
            double minHeight = Math.Sqrt(minArea / 15) * 5;
            wCount = ba.Count;
            wo.SetText("正在夸大");
            wo.Step(0);
            for (int i = 0; i < ba.Count; i++) {
                f = fc.GetFeature(features[i]);
                wo.Step(wCount);
                if (ba.get_Item(i) && !f.Shape.IsEmpty && (f.Shape as IArea).Area > 1) {
                    MBR mbr = MBR.GetMBR(f.Shape as IPolygon);
                    bool widthBigger = (mbr.env.Width > mbr.env.Height);
                    double dx = widthBigger ? (mbr.env.Width > minHeight ? 0 : (minHeight - mbr.env.Width) / 2) : (mbr.env.Width > minWidth ? 0 : (minWidth - mbr.env.Width) / 2);
                    double dy = (!widthBigger) ? (mbr.env.Height > minHeight ? 0 : (minHeight - mbr.env.Height) / 2) : (mbr.env.Height > minWidth ? 0 : (minWidth - mbr.env.Height) / 2);
                    mbr.env.Expand(dx, dy, false);
                    f.Shape = mbr.Shape;
                    f.Store();
                }
                else {
                    f.Delete();
                }
            }
            m_application.SetBusy(false);
            
        }*/

        private GLayerInfo GetLayer(GWorkspace workspace) {
            foreach (GLayerInfo info in workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.建筑物
                    && info.OrgLayer != null
                    ) {
                    return info;
                }
            }
            return null;
        }

        internal class MBR {
            internal IPoint centerPoint;
            internal IEnvelope env;
            internal double angle;
            MBR(IEnvelope e, double a,IPoint p) {
                env = e;
                angle = a;
                centerPoint = p;
            }

            internal IPolygon Shape {
                get {
                    PolygonClass p = new PolygonClass();
                    RingClass r = new RingClass();
                    object miss = Type.Missing;
                    r.AddPoint(env.LowerLeft, ref miss, ref miss);
                    r.AddPoint(env.UpperLeft, ref miss, ref miss);                    
                    r.AddPoint(env.UpperRight, ref miss, ref miss);
                    r.AddPoint(env.LowerRight, ref miss, ref miss);
                    r.Close();
                    p.AddGeometry(r, ref miss, ref miss);
                    p.Rotate(centerPoint, angle);
                    return p;
                }
            }
            
            static internal MBR GetMBR(IPolygon poly) {
                IPolygon p = (poly as ITopologicalOperator).ConvexHull() as IPolygon;
                IPoint centerPoint = (p as IArea).Centroid;
                ISegmentCollection sc = p as ISegmentCollection;
                double minArea = double.MaxValue;
                double minAngle = 0;
                IEnvelope minEnv = null;
                for (int i = 0; i < sc.SegmentCount; i++) {
                    ILine line = sc.get_Segment(i) as ILine;
                    double a = line.Angle;
                    IPolygon rp = (p as IClone).Clone() as IPolygon;
                    (rp as ITransform2D).Rotate(centerPoint, -1 * a);
                    IEnvelope env = rp.Envelope;
                    if ((env as IArea).Area < minArea) {
                        minArea = (env as IArea).Area;
                        minEnv = env;
                        minAngle = a;
                    }
                }
                return new MBR(minEnv, minAngle, centerPoint);
            }
        }
    }
}

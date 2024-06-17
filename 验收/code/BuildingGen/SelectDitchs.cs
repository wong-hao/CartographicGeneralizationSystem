using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using GENERALIZERLib;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;


namespace BuildingGen
{
    public class SelectDitchs : BaseGenTool
    {
        INewLineFeedback fb;
        GLayerInfo info;
        public SelectDitchs()
        {
            base.m_category = "GWater";
            base.m_caption = "沟渠选取";
            base.m_message = "对沟渠进行选取";
            base.m_toolTip = "对沟渠进行选取";
            base.m_name = "DitchSelect";
        }

        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null);
            }
        }

        public override void OnClick()
        {
            info = null;
            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers)
            {
                if (tempInfo.LayerType == GCityLayerType.水系
                    && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && tempInfo.OrgLayer != null
                    )
                {
                    info = tempInfo;
                    break;
                }
            }

        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            if (info == null)
                return;

            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb == null)
            {
                fb = new NewLineFeedbackClass();
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
            if (fb == null)
                return;

            IPolyline p = fb.Stop();
            fb = null;
            Gen(p);

            m_application.MapControl.Refresh();
        }

        private double CalculateLength(IPoint oriPOI, IPolygon targetPoly)
        {
            IPolycurve targetPolycurve = targetPoly as IPolycurve;
            IPoint nearestPOI = new PointClass();
            double distanceACurve = 0;
            double distanceFCurve = 0;
            bool isRightSide = false;
            targetPolycurve.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, oriPOI, false, nearestPOI, ref distanceACurve, ref distanceFCurve, ref isRightSide);
            return distanceFCurve;
        }

        private void Gen(IPolyline line)
        {
            IFeatureClass  fc = (info.Layer as IFeatureLayer).FeatureClass;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = line;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
            sf.WhereClause = "要素代码='632250'";
            IFeatureCursor featCur = fc.Search(sf, false);
            List<ifeat_dis> IfeatDis = new List<ifeat_dis>();
            IFeature feat = null;
            while ((feat = featCur.NextFeature()) != null)
            {
                ifeat_dis tp = new ifeat_dis(feat, CalculateLength(line.FromPoint, feat.Shape as IPolygon));
                IfeatDis.Add(tp);
            }
            IfeatDis.Sort((e1, e2) => { return (e1.dis < e2.dis) ? -1 : 1; });
            bool isSet = false;
            foreach (ifeat_dis tppp in IfeatDis)
            {
                if (isSet)
                {
                    IFeature featt = tppp.feat;
                    featt.Delete();

                }
                isSet = !isSet;
            }
            m_application.MapControl.Refresh();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featCur);
        }

        internal class ifeat_dis
        {
            internal IFeature feat;
            internal double dis;
            internal ifeat_dis(IFeature tpFeat,double ptDis)
            {
                feat = tpFeat;
                dis = ptDis;
            }
        }

        public override void OnKeyDown(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null)
                    {
                        fb.Stop();
                        fb = null;
                    }
                    break;
            }
        }
     

    }

}

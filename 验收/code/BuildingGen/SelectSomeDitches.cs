using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
using System.Collections;
using System.Windows.Forms;
namespace BuildingGen
{
    public class SelectSomeDitches : BaseGenTool
    {
        INewLineFeedback fb_line;

        public SelectSomeDitches()
        {
            base.m_category = "GBuilding";
            base.m_caption = "隔条选取沟渠";
            base.m_message = "沟渠选取";
            base.m_toolTip = "沟渠选取";
            base.m_name = "DitchGroup2";
        }

        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null
                    && m_application.EngineEditor.EditState != ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing
                    //&& m_application.Workspace.EditLayer != null
                ;
            }
        }
        IFeatureClass ditchClass = null;
        public override void OnClick()
        {

            IFeatureLayer ditchLayer = new FeatureLayerClass();
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;
            if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
            {
                IFeatureClass centralizedDitch2 = FeatWS.OpenFeatureClass("沟渠中心线");
                ditchClass = centralizedDitch2;
                ditchLayer.Name = "沟渠中心线";
                ditchLayer.FeatureClass = centralizedDitch2;

                bool isExistedLayer = false;
                for (int m = 0; m < m_application.MapControl.LayerCount; m++)
                {
                    if (m_application.MapControl.get_Layer(m).Name == "沟渠中心线")
                    {
                        isExistedLayer = true;
                    }
                }
                if (!isExistedLayer)
                {
                    m_application.MapControl.AddLayer(ditchLayer, 0);
                }

                _GenUsed2 = centralizedDitch2.FindField("_GenUsed");
                if (_GenUsed2 == -1)
                {
                    IFieldEdit2 field = new FieldClass();
                    field.Name_2 = "_GenUsed";
                    field.Type_2 = esriFieldType.esriFieldTypeInteger;
                    centralizedDitch2.AddField(field as IField);
                    _GenUsed2 = centralizedDitch2.Fields.FindField("_GenUsed");
                }
            }
        }
        int _GenUsed2 = -1;

        private IFeatureClass CreateLayer()
        {
            List<IField> fs = new List<IField>();
            IFieldEdit2 field = new FieldClass();
            field.Name_2 = "GroupID";
            field.Type_2 = esriFieldType.esriFieldTypeInteger;
            fs.Add(field as IField);
            field = new FieldClass();
            field.Name_2 = "OrgID";
            field.Type_2 = esriFieldType.esriFieldTypeInteger;
            fs.Add(field as IField);
            return m_application.Workspace.LayerManager.TempLayer(fs);
        }

        IPoint oriPOI = new PointClass();
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 1)
            {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

                if (fb_line == null)
                {
                    fb_line = new NewLineFeedbackClass();
                    fb_line.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                    fb_line.Start(p);
                }
                else
                {
                    fb_line.AddPoint(p);
                    IPolyline line = fb_line.Stop();
                    Gen(line);
                    fb_line = null;
                    m_application.MapControl.Refresh();
                }

            }
            if (Button == 2)
            {
                return;
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb_line != null)
            {
                fb_line.MoveTo(p);
            }
        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            //base.OnMouseUp(Button, Shift, X, Y);
        }

        private double CalculateLength(IPoint oriPOI, IPolyline targetPoly)
        {
            IPolycurve targetPolycurve = targetPoly as IPolycurve;
            IPoint nearestPOI = new PointClass();
            double distanceACurve = 0;
            double distanceFCurve = 0;
            bool isRightSide = false;
            targetPolycurve.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, oriPOI, false, nearestPOI, ref distanceACurve, ref distanceFCurve, ref isRightSide);
            return distanceFCurve;
        }

        public override void OnDblClick()
        {
            m_application.MapControl.Refresh();
        }

        public override void OnKeyDown(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    fb_line = null;
                    break;
                case (int)System.Windows.Forms.Keys.Space:
                    break;
            }
        }


        private void Gen(IPolyline line)
        {
            IFeatureClass fc = ditchClass;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = line;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
            sf.WhereClause = "要素代码='632250'";
            IFeatureCursor featCur = fc.Search(sf, false);
            List<ifeat_dis> IfeatDis = new List<ifeat_dis>();
            IFeature feat = null;
            while ((feat = featCur.NextFeature()) != null)
            {
                ifeat_dis tp = new ifeat_dis(feat, CalculateLength(line.FromPoint, feat.Shape as IPolyline));
                IfeatDis.Add(tp);
            }
            IfeatDis.Sort((e1, e2) => { return (e1.dis < e2.dis) ? -1 : 1; });
            bool isSet = false;
            foreach (ifeat_dis tppp in IfeatDis)
            {
                if (isSet)
                {
                    IFeature featt = tppp.feat;
                    featt.set_Value(_GenUsed2, -1);
                    featt.Store();
                }
                if (!isSet)
                {
                    IFeature featt = tppp.feat;
                    featt.set_Value(_GenUsed2, 1);
                    featt.Store();
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
            internal ifeat_dis(IFeature tpFeat, double ptDis)
            {
                feat = tpFeat;
                dis = ptDis;
            }
        }




    }
}

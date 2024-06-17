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
    public class HandlerForWaterBuildingConflict : BaseGenTool
    {
        INewPolygonFeedback fb;
        GLayerInfo Waterinfo;
        GLayerInfo Buildinginfo;
        public HandlerForWaterBuildingConflict()
        {
            base.m_category = "GWater";
            base.m_caption = "水系建筑冲突处理";
            base.m_message = "水系建筑冲突处理";
            base.m_toolTip = "水系建筑冲突处理";
            base.m_name = "conflict";
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
            Waterinfo = null;
            Buildinginfo = null;
            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers)
            {
                if (tempInfo.LayerType == GCityLayerType.水系
                    && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && tempInfo.OrgLayer != null
                    )
                {
                    Waterinfo = tempInfo;
                }
                if (tempInfo.LayerType == GCityLayerType.建筑物
                   && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                   && tempInfo.OrgLayer != null
                   )
                {
                    Buildinginfo = tempInfo;
                }
            }
            if (Waterinfo== null||Buildinginfo==null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到图层");
                return;
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
            if (fb == null)
                return;

            IPolygon p = fb.Stop();
            fb = null;
            Gen2(p);

            m_application.MapControl.Refresh();
        }
        private void Gen2(IPolygon range)
        {
            try
            {
                IFeatureClass waterClass = (Waterinfo.Layer as IFeatureLayer).FeatureClass;
                IFeatureClass buildingClass = (Buildinginfo.Layer as IFeatureLayer).FeatureClass;

                ISpatialFilter sf1 = new SpatialFilterClass();
                sf1.Geometry = range;
                sf1.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor fc1 = buildingClass.Update(sf1, false);
                IFeature feat = null;
                WaitOperation w = m_application.SetBusy(true);
                int count = buildingClass.FeatureCount(sf1);
                while ((feat = fc1.NextFeature()) != null)
                {
                    w.SetText("数据处理中...."+feat.OID);
                    w.Step(count);
                    IArea buildArea = feat.Shape as IArea;
                    if (buildArea.Area > 1200)
                    {
                        continue;
                    }
                    ISpatialFilter sf2 = new SpatialFilterClass();
                    sf2.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                    sf2.Geometry = feat.Shape;
                    int waterCount = waterClass.FeatureCount(sf2);
                    if (waterCount > 0)
                    {
                        feat.Delete();
                    }
                    //IFeatureCursor fc2 = waterClass.Search(sf2, false);
                    //IFeature feat2 = null;
                    //while ((feat2 = fc2.NextFeature()) != null)
                    //{
                    //    feat.Delete();
                    //    break;
                    //}
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(fc2);
                }
                m_application.SetBusy(false);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fc1);
                m_application.MapControl.Refresh();
            }
            catch(Exception ex)
            {
                m_application.SetBusy(false);
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


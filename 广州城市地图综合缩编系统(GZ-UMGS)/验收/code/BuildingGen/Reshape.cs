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
    public class Reshape : BaseGenTool
    {
        INewLineFeedback fb;
        GLayerInfo info;
        GCityLayerType currentLayerType;
        public Reshape(GCityLayerType layerType)
        {
            base.m_category = "GBuilding";
            base.m_caption = "曲线串接";
            base.m_message = "对选定的要素进行串接";
            base.m_toolTip = "对选定的要素进行串接";
            base.m_name = "Reshape";

            currentLayerType = layerType;
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
                if (tempInfo.LayerType == currentLayerType
                    //&& (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && tempInfo.OrgLayer != null
                    )
                {
                    info = tempInfo;
                    break;
                }
            }
            if (info == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到图层");
                return;
            }

            IMap map = m_application.Workspace.Map;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null)
            {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;

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

        private void Gen(IPolyline range)
        {
            try
            {
                IFeatureLayer layer = new FeatureLayerClass();
                layer.FeatureClass = (info.Layer as IFeatureLayer).FeatureClass;
                layer.Name = "tp";
                IFeatureClass fc = layer.FeatureClass;

                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = range;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor cur = fc.Update(sf, true);
                IFeature feat = cur.NextFeature();
                IPolygon4 ringsPoly = feat.ShapeCopy as IPolygon4;

                IGeometryBag exteriorRings = ringsPoly.ExteriorRingBag;
                IEnumGeometry exteriorEnum = exteriorRings as IEnumGeometry;
                exteriorEnum.Reset();
                IRing currentExterRing = exteriorEnum.Next() as IRing;
                IRing tpInteriorRing = new RingClass();
                IPolygon4 RingPolygon = new PolygonClass();
                IGeometryCollection ringsCollection = RingPolygon as IGeometryCollection;
                IGeometry forAddGeometry = currentExterRing as IGeometry;
                ringsCollection.AddGeometries(1, ref forAddGeometry);
                while (currentExterRing != null)
                {
                    IGeometryBag interiorRings = ringsPoly.get_InteriorRingBag(currentExterRing);
                    IEnumGeometry interiorEnum = interiorRings as IEnumGeometry;
                    tpInteriorRing = interiorEnum.Next() as IRing;
                    while (tpInteriorRing != null)
                    {

                        forAddGeometry = tpInteriorRing as IGeometry;
                        ringsCollection.AddGeometries(1, ref forAddGeometry);
                        tpInteriorRing = interiorEnum.Next() as IRing;
                    }

                    currentExterRing = exteriorEnum.Next() as IRing;
                }
                IGeometryCollection c = range as IGeometryCollection;
                IPath currentPath = (c.get_Geometry(0)) as IPath;
                //foreach (IGeometry r in ringsCollection)
                //{
                for (int i = 0; i < ringsCollection.GeometryCount;i++)
                {
                    IRing ring = ringsCollection.get_Geometry(i) as IRing;
                    ring.Reshape(currentPath);
                }
                //}
                feat.Shape = RingPolygon as IGeometry;
                feat.Store();
                m_application.MapControl.Refresh();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cur);
            }
            catch (Exception ex)
            {

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
                    m_application.MapControl.Refresh();
                    break;             
            }
        }

    }

}


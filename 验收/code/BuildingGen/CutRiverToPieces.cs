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
    public class CutRiverToPieces : BaseGenTool
    {
        INewLineFeedback fb;
        GLayerInfo info;
        Generalizer gen;
        public CutRiverToPieces()
        {
            base.m_category = "GWater";
            base.m_caption = "分割河流";
            base.m_message = "分割河流";
            base.m_toolTip = "分割一条河流中的狭窄部分";
            base.m_name = "CutRiver";
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
            if (info == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到水系图层");
                return;
            }

            IMap map = m_application.Workspace.Map;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null)
            {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;
            if (fc.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                System.Windows.Forms.MessageBox.Show("当前编辑图层不是面状图层");
                return;
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
                fb = new NewLineFeedback();
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
            IFeatureClass riverClass = (info.Layer as IFeatureLayer).FeatureClass;

            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor insertCur = riverClass.Insert(true);

            IFeatureCursor fcur = null;
            IFeature feat = null;
            fcur = riverClass.Update(sf, false);
            while ((feat = fcur.NextFeature()) != null)
            {
                ITopologicalOperator4 topo = feat.ShapeCopy as ITopologicalOperator4;
                IGeometryCollection geoColl;
                try
                {
                    geoColl = topo.Cut2(range);
                }
                catch
                {
                    continue;
                }
                for (int i = 0; i < geoColl.GeometryCount; i++)
                {
                    IGeometry tp=geoColl.get_Geometry(i);
                    feat.Shape = tp;
                    insertCur.InsertFeature(feat as IFeatureBuffer);
                }
                fcur.DeleteFeature();
            }

            fcur.Flush();
            insertCur.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCur);
            m_application.MapControl.Refresh();
        }

    }

}

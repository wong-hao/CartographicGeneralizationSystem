using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;
namespace BuildingGen
{
    public class PlantMinHole : BaseGenTool
    {
        GLayerInfo VegetationLayer;
        Generalizer gen;
        public PlantMinHole()
        {
            base.m_category = "GVegetation";
            base.m_caption = "植被化简";
            base.m_message = "植被化简";
            base.m_toolTip = "植被化简";
            base.m_name = "PlantMinHole";
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
            VegetationLayer = null;
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers)
            {
                if (info.LayerType == GCityLayerType.植被
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && info.OrgLayer != null
                    )
                {
                    VegetationLayer = info;
                    break;
                }
            }
            if (VegetationLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到植被图层");
                return;
            }

        }

        void Gen(GLayerInfo orgLayerInfo, IPolygon range)
        {
            IFeatureLayer waterLayer = orgLayerInfo.Layer as IFeatureLayer;
            IFeatureClass fc = waterLayer.FeatureClass;
            ISpatialFilter qf = new SpatialFilterClass();
            qf.Geometry = range;
            qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor fCur;
            IFeature f;
            fCur = fc.Update(qf, false);
            try
            {
                double minArea = (double)m_application.GenPara["植被最小上图面积"];
                WaitOperation w = m_application.SetBusy(true);
                int count = fc.FeatureCount(qf);
                while ((f = fCur.NextFeature()) != null)
                {
                    w.SetText("处理中..." + f.OID);
                    w.Step(count);
                    IGeometry geoCopy = f.Shape;
                    IPolygon forMinArea = geoCopy as IPolygon;
                    ITopologicalOperator oriTO = forMinArea as ITopologicalOperator;
                    oriTO.Simplify();

                    IPolygon4 result = oriTO as IPolygon4;
                    IGeometryCollection resultColl = result.ConnectedComponentBag as IGeometryCollection;
                    for (int i = 0; i < resultColl.GeometryCount; i++)
                    {
                        IPolygon tpPo = resultColl.get_Geometry(i) as IPolygon;
                        if ((tpPo as IArea).Area < minArea)
                        {
                            continue;
                        }
                        f.Shape = tpPo;
                        int ID = Convert.ToInt32(insertCur.InsertFeature(f as IFeatureBuffer));
                    }
                    f.Delete();
                }
                m_application.SetBusy(false);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCur);
                m_application.MapControl.Refresh();

            }
            catch (Exception ex)
            {
                m_application.SetBusy(false);
            }
        }

        INewPolygonFeedback fb;
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
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb != null)
            {
                fb.MoveTo(p);
            }
        }

        public override void OnDblClick()
        {
            if (fb != null)
            {
                IPolygon poly = fb.Stop();
                fb = null;
                Gen(VegetationLayer, poly);
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

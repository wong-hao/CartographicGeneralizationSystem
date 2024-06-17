using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen
{
    public class BuildCut : BaseGenTool
    {
        GLayerInfo waterLayer;
        public BuildCut()
        {
            base.m_category = "GBuilding";
            base.m_caption = "切割尖角";
            base.m_message = "分离";
            base.m_toolTip = "去除建筑物尖角";
            base.m_name = "BuildCut";
            waterLayer = null;
        }
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null;
            }
        }
        public override void OnClick()
        {
            waterLayer = null;
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers)
            {
                if (info.LayerType == GCityLayerType.建筑物
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && info.OrgLayer != null
                    )
                {
                    waterLayer = info;
                    break;
                }
            }
            if (waterLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到图层");
                return;
            }
            if (System.Windows.Forms.MessageBox.Show("将进行切割操作，请确认", "提示", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            IFeatureClass waterClass = (waterLayer.Layer as IFeatureLayer).FeatureClass;
            reshapeDitch(waterLayer);

            m_application.MapControl.Refresh();
        }
        void reshapeDitch(GLayerInfo orgLayerInfo)
        {
            IFeatureLayer waterLayer = orgLayerInfo.Layer as IFeatureLayer;
            IFeatureClass fc = waterLayer.FeatureClass;
            IFeatureCursor fCur;
            IFeature f;
            fCur = fc.Update(null, false);
            ISpatialReferenceFactory srf_ = new SpatialReferenceEnvironmentClass();
            IProjectedCoordinateSystem pcs_ = srf_.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_CM_114E);
            IFeatureCursor insertCur = fc.Insert(false);
            try
            {
                WaitOperation w = m_application.SetBusy(true);
                List<IFeature> allFeats = new List<IFeature>();
                int count = fc.FeatureCount(null);
                Dictionary<int, bool> isInsertedFeat = new Dictionary<int, bool>();
                while ((f = fCur.NextFeature()) != null)
                {
                    if (isInsertedFeat.ContainsKey(f.OID))
                    {
                        continue;
                    }
                    w.SetText("切割尖角中..."+f.OID);
                    w.Step(count);
                    IGeometry geoCopy = f.ShapeCopy;
                    geoCopy.SpatialReference = pcs_;
                    IPolygon forMinArea = geoCopy as IPolygon;
                    double width = ((forMinArea as IArea).Area * 2.5) / forMinArea.Length;
                    double minArea = 200;
                    double bufferDis = 2;
                    IPolygon bufferPoly;
                    if (width / 2.5 > 1.5)
                    {
                        bufferPoly = (geoCopy as ITopologicalOperator).Buffer(-2) as IPolygon;
                        bufferPoly = (bufferPoly as ITopologicalOperator).Buffer(3) as IPolygon;
                    }
                    else
                    {
                        bufferPoly = (geoCopy as ITopologicalOperator).Buffer(-width/2.5) as IPolygon;
                        bufferPoly = (bufferPoly as ITopologicalOperator).Buffer(width/2) as IPolygon;
                    }
                    if ((bufferPoly as IArea).Area < minArea)
                    {
                        continue;
                    }
                    ITopologicalOperator fcop = f.ShapeCopy as ITopologicalOperator;
                    fcop.Simplify();
                    ITopologicalOperator bcop = bufferPoly as ITopologicalOperator;
                    bcop.Simplify();
                    bufferPoly.SpatialReference = f.Shape.SpatialReference;
                    IPolygon4 resultPoly = fcop.Difference(bufferPoly) as IPolygon4;
                    bufferPoly = fcop.Difference(resultPoly) as IPolygon;//主体

                    IGeometryCollection otherPoly = new PolygonClass();//需要剪切的部分

                    IGeometryCollection resultColl = resultPoly.ConnectedComponentBag as IGeometryCollection;//网须
                    for (int i = 0; i < resultColl.GeometryCount; i++)
                    {
                        IPolygon tpPo = resultColl.get_Geometry(i) as IPolygon;
                        IRelationalOperator tpPoRe = tpPo as IRelationalOperator;

                        int touchCount = 0;
                        IGeometryCollection mainparts = (bufferPoly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                        for (int j = 0; j < mainparts.GeometryCount; j++)
                        {
                            if (tpPoRe.Touches(mainparts.get_Geometry(j)))
                            {
                                touchCount = touchCount + 1;
                            }
                        }

                        if (touchCount < 2)
                        {
                            IGeometryCollection tpPoColl = tpPo as IGeometryCollection;
                            for (int m = 0; m < tpPoColl.GeometryCount; m++)
                            {
                                IGeometry tG = tpPoColl.get_Geometry(m);
                                //double width2=((tG as IArea).Area * 2.5) / (tG as IRing).Length;
                                //if (width2> 0.1)
                                //{
                                //    continue;
                                //}
                                otherPoly.AddGeometries(1, ref tG);
                            }
                        }
                    }

                    IPolygon result = fcop.Difference(otherPoly as IGeometry) as IPolygon;
                    f.Shape = result;
                    f.Store();
                }
                m_application.SetBusy(false);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCur);
            }
            catch (Exception ex)
            {
                m_application.SetBusy(false);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCur);
        }
    }
}
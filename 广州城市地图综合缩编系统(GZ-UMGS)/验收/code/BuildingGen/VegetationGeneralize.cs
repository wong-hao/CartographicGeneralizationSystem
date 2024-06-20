using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace BuildingGen
{
    class VegetationGeneralize : BaseGenCommand
    {
        Generalizer gen;
        public VegetationGeneralize()
        {
            base.m_category = "GVegetation";
            base.m_caption = "植被综合";
            base.m_message = "植被综合";
            base.m_toolTip = "植被综合";
            base.m_name = "VegetationGeneralize";
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("植被_融合距离",(double)3),
                new GenDefaultPara("植被_化简深度",(double)3)
            };
            gen = new Generalizer();
        }
        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null)
                    && (m_application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                    && (m_application.Workspace.EditLayer != null);
            }
        }
        public override void OnClick()
        {
            IMap map = m_application.Workspace.Map;
            GLayerInfo info = m_application.Workspace.EditLayer;
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

            m_application.EngineEditor.StartOperation();
            try
            {
                gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 10000, 50000);
                ISelectionSet set = (layer as IFeatureSelection).SelectionSet;
                IEnumIDs ids = set.IDs;
                int id = -1;
                double distance = Math.Abs((double)m_application.GenPara["植被_融合距离"]);

                //融合起来
                IPolygon union = null;
                while ((id= ids.Next())!=-1)
                {
                    IFeature feature = fc.GetFeature(id);
                    
                    if (union == null)
                    {
                        union = feature.ShapeCopy as IPolygon;
                    }
                    else
                    {
                        IPolygon poly = (feature.Shape as ITopologicalOperator).Union(union) as IPolygon;
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(union);
                        union = poly;
                    }
                    feature.Delete();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                }
                bool nonLinear = false;
                (union as ISegmentCollection).HasNonLinearSegments(ref nonLinear);
                if (nonLinear)
                {
                    union.Densify(distance, 0);
                    union.Generalize(0.01);
                }
                IPolygon outer1 = (union as ITopologicalOperator).Buffer(distance / 2) as IPolygon;
                IPolygon outer = (outer1 as ITopologicalOperator).Buffer(-distance/2) as IPolygon;
                //outer = (outer as ITopologicalOperator).Buffer(distance/2) as IPolygon;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(outer1);                

                IGeometryCollection outBag = (outer as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                for (int i = 0; i < outBag.GeometryCount; i++)
                {
                    IGeometry a1 = outBag.get_Geometry(i);

                    IFeature f = fc.CreateFeature();
                    f.Shape = outBag.get_Geometry(i);
                    f.Store();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(f); 
                }

                m_application.EngineEditor.StopOperation("植被融合");
                
            }
            catch
            {
                m_application.EngineEditor.AbortOperation();                
            }
            m_application.MapControl.Refresh();
        }
        private void Gen2(IFeature feature)
        { 
        }
    }
}

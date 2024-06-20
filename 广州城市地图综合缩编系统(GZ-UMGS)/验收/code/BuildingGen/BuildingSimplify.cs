using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace BuildingGen
{
    public class BuildingSimplify:BaseGenCommand
    {
        IPolygon result;
        Generalizer gen;
        BuildingGenCore.SimplifyBuildingGeneralizer sbg;
        public BuildingSimplify()
        {
            base.m_category = "GBuilding";
            base.m_caption = "建筑物化简";
            base.m_message = "对选定建筑物进行化简";
            base.m_toolTip = "对选定建筑物进行化简";
            base.m_name = "BuildingSimplify";
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("建筑物化简_化简容差",(double)3)
                //,new GenDefaultPara("多边形合并的最小间距",(double)3)
            };
            gen = new Generalizer();
            sbg = new BuildingGenCore.SimplifyBuildingGeneralizer();
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

                while ((id = ids.Next()) != -1)
                {
                    IFeature feature = fc.GetFeature(id);
                    Gen2(feature);
                }
                m_application.EngineEditor.StopOperation("建筑物化简");
                m_application.MapControl.Refresh();
            }
            catch
            {
                m_application.EngineEditor.AbortOperation();
            }
        }
        private void Gen(IFeature feature)
        {
            
            object miss = Type.Missing;
            IPolygon poly = feature.Shape as IPolygon;
            IGeometryCollection p = feature.ShapeCopy as IGeometryCollection;
            p.RemoveGeometries(0, p.GeometryCount);
            IGeometryCollection gc = poly as IGeometryCollection;
            for (int i = 0; i < gc.GeometryCount; i++)
            {
                IRing ring = gc.get_Geometry(i) as IRing;
                IPath path = gen.SimplifyBuildingBoundaryByLiu(ring,10);
                RingClass nring = new RingClass();
                for (int j = 0; j < (path as IPointCollection).PointCount; j++)
                {
                    IPoint point = (path as IPointCollection).get_Point(j) as IPoint;
                    nring.AddPoint(point, ref miss, ref miss);
                }
                
                p.AddGeometry(nring, ref miss, ref miss);                
            }
            (p as ITopologicalOperator).Simplify();
            feature.Shape = p as IGeometry;
            feature.Store();
        }
        private void Gen2(IFeature feature)
        {
            IPolygon poly = feature.Shape as IPolygon;
            poly = GBuilding.Simplify(poly, (double)m_application.GenPara["建筑物化简_化简容差"]);
            if (poly != null && !poly.IsEmpty)
            {
                feature.Shape = poly;
                feature.Store();
            }
        }
        
    }
}

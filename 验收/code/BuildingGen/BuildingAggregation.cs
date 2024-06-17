using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using GENERALIZERLib;

namespace BuildingGen
{
    class BuildingAggregation:BaseGenCommand
    {
        Generalizer gen;
        public BuildingAggregation()
        {
            base.m_category = "GBuilding";
            base.m_caption = "建筑物合并";
            base.m_message = "对选定建筑物进行融合";
            base.m_toolTip = "对选定建筑物进行融合";
            base.m_name = "BuildingSimplify";
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("建筑物融合_融合距离",(double)3)
                //,new GenDefaultPara("建筑物多边形化简聚类角度",(double)3)
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

            //base.OnClick();
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

                GeometryBagClass gb = new GeometryBagClass();
                IFeature fb = null;
                while ((id = ids.Next()) != -1)
                {
                    IFeature feature = fc.GetFeature(id);
                    IGeometry shape = feature.ShapeCopy;
                    gb.AddGeometries(1,ref shape);
                    if (fb == null)
                    {
                        fb = feature;
                    }
                    else
                    {
                        feature.Delete();
                    }
                }
                IPolygon poly = gen.AggregationOfPolygons(gb, (double)m_application.GenPara["建筑物融合_融合距离"]);

                if (fb != null && poly != null)
                {
                    fb.Shape = poly;
                    fb.Store();
                    m_application.EngineEditor.StopOperation("建筑物化简");
                }
                else
                {
                    m_application.EngineEditor.AbortOperation();
                }
                m_application.MapControl.Refresh();
            }
            catch
            {
                m_application.EngineEditor.AbortOperation();
            }
        }

    }
}

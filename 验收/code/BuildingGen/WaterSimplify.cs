using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace BuildingGen
{
    public class WaterSimplify:BaseGenCommand
    {
        Generalizer gen;
        public WaterSimplify()
        {
            base.m_category = "GWater";
            base.m_caption = "水系化简";
            base.m_message = "对选定水系进行化简";
            base.m_toolTip = "对选定水系进行化简";
            base.m_name = "WaterSimplify";
            //base.m_usedParas = new GenDefaultPara[] 
            //{ 
            //    new GenDefaultPara("水系化简_弯曲深度",(double)3)
            //};
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
                double depth = (double)m_application.GenPara["水系化简_弯曲深度"];
                while ((id = ids.Next()) != -1)
                {
                    IFeature feature = fc.GetFeature(id);
                    IPolygon poly = gen.SimplifyPolygonByDT(feature.Shape as IPolygon, depth);
                    feature.Shape = poly;
                    feature.Store();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                }
                m_application.EngineEditor.StopOperation("水系化简");
                m_application.MapControl.Refresh();
            }
            catch
            {
                m_application.EngineEditor.AbortOperation();
            }

        }
        private void Gen(IFeature feature)
        { 
            
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataManagementTools;

namespace BuildingGen
{
    class WaterLineSplit : BaseGenCommand
    {
        public WaterLineSplit()
        {
            base.m_category = "GWater";
            base.m_caption = "水系打散";
            base.m_message = "将水系在交点处打散处理";
            base.m_toolTip = "将水系在交点处打散处理";
            base.m_name = "WaterSplit";

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
            //GLayerInfo info = GetLayer();
            //SplitLayer(info);
            IFeatureClass lines = GetWaterLineClass();
            SplitLayer(lines);

            m_application.MapControl.Refresh();
        }

        private void SplitLayer(IFeatureClass layer)
        {
            //IFeatureLayer flayer = layer.Layer as IFeatureLayer;
            //IFeatureClass fcOrg = flayer.FeatureClass;
            IFeatureClass fcOrg = layer;
            IFeatureWorkspace fworkspace = (m_application.Workspace.Workspace as IFeatureWorkspace);

            string tmpName = m_application.Workspace.LayerManager.TempLayerName();
            FeatureToLine ftl = new FeatureToLine();
            ftl.in_features = m_application.Workspace.Name + "\\" + (layer as IDataset).Name;
            ftl.out_feature_class = m_application.Workspace.Name + "\\" + tmpName;
            ftl.cluster_tolerance = 0.01;

            object result = m_application.Geoprosessor.Execute(ftl, null);
#if DEBUG
            if (result == null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < m_application.Geoprosessor.MessageCount; i++)
                {
                    sb.AppendLine(m_application.Geoprosessor.GetMessage(i));
                }
                System.Windows.Forms.MessageBox.Show(sb.ToString());
                return;
            }
#endif
            string name = (fcOrg as IDataset).Name;
            IFeatureClass fcNew = fworkspace.OpenFeatureClass(tmpName);
            //flayer.FeatureClass = fcNew;
            (fcOrg as IDataset).Delete();
            (fcNew as IDataset).Rename(name);
            m_application.MapControl.Refresh();
        }

        private GLayerInfo GetLayer()
        {
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers)
            {
                if (info.LayerType == GCityLayerType.水系
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    )
                {
                    return info;
                }
            }
            return null;
        }
        private IFeatureClass GetWaterLineClass()
        {
            IFeatureClass centralizedDitch = null;
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;
            if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
            {
                centralizedDitch = FeatWS.OpenFeatureClass("沟渠中心线");
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("没有找到水系中心线层！");
            }
            return centralizedDitch;
        }
    }
}

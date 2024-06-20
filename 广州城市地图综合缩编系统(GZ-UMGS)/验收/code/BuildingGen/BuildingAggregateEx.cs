using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

namespace BuildingGen
{
    public class BuildingAggregateEx:BaseGenCommand
    {
        public BuildingAggregateEx()
        {
            base.m_category = "GBuilding";
            base.m_caption = "建筑物融合";
            base.m_message = "建筑物融合";
            base.m_toolTip = "建筑物融合\n合并后的属性将会是所在面积最大建筑物的属性";
            base.m_name = "BuildingAggregateEx";
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("建筑物融合_融合距离",(double)3)
            };
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
            m_application.EngineEditor.StartOperation();
            try
            {
                double distance = (double)m_application.GenPara["建筑物融合_融合距离"];
                GBuilding.Aggregate(m_application.Workspace.EditLayer.Layer as IFeatureLayer, distance, null);
                m_application.EngineEditor.StopOperation("建筑物融合");
            }
            catch
            {
                m_application.EngineEditor.AbortOperation();
            }
            m_application.MapControl.Refresh();

        }
    }
}

 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.MapGeneralization
{
    /// <summary>
    /// 拓扑初始化
    /// </summary>
    public class TopolgyIntiCmd : SMGICommand
    {
        private Dictionary<string, string> layersName = new Dictionary<string, string>();
        private Dictionary<string, IFeatureDataset> fDsDic = new Dictionary<string, IFeatureDataset>();
      
        public TopolgyIntiCmd()
        {
            m_caption = "拓扑初始化";
            m_toolTip = "拓扑初始化";
            m_category = "拓扑";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null&&m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing;

            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            return;
            var ws = app.Workspace.EsriWorkspace;
            var fds=   ws.get_Datasets(esriDatasetType.esriDTFeatureDataset);
            fds.Reset();
            IDataset pds = null;
            while((pds=fds.Next())!=null)
            {
                  IFeatureDataset pfds = pds as IFeatureDataset;
                  fDsDic[pfds.Name] = pfds;

                  IFeatureDataset featureDs = pds as IFeatureDataset;
                  ITopologyContainer2 topologyContainer = (ITopologyContainer2)featureDs;
                  string topolgyName = pds.Name + "_Topology";

                  ITopology pTopology = GetTop(topologyContainer, topolgyName);
                  TopologyApplication.TopName = topolgyName;
                  TopologyApplication.Topology = pTopology;
                  
            }
            
          
        }
        private ITopology GetTop(ITopologyContainer2 topologyContainer, string topolgyName)
        {
            ITopology top = null;
            try
            {

                top = topologyContainer.get_TopologyByName(topolgyName);
                return top;
            }
            catch (Exception ex)
            {
                return top;
            }
        }
       
        public override void OnClick()
        {
            
           
        }
    }
}


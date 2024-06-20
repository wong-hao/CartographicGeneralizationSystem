using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Xml;
using System.Xml.Linq;
using SMGI.Common.Algrithm;
namespace SMGI.Plugin.MapGeneralization
{
    //化简
    public class TopolgySimpilfyCmd : SMGI.Common.SMGICommand
    {
      
        List<string> _ruleNames = new List<string>();

        public TopolgySimpilfyCmd()
        {
            m_caption = "拓扑化简Cmd"; 
        }
        public override bool Enabled
        {
            get
            {
                
                return TopologyApplication.SelPolyline != null && m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        double SMHeigth = 0;
        double SMWidth = 0;
        ITopology topgy = null;
        public override void OnClick()
        {
            var dlg = new FrmSimplify();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SMHeigth = dlg.heigth;
                SMWidth = dlg.width;
                
            }
            var pEditor = m_Application.EngineEditor;
            try
            {
                pEditor.StartOperation();
                using (var wo = m_Application.SetBusy())
                {
                    IEnvelope penvelope;
                    var pgeo = TopologyApplication.SelPolyline;
                    IPolyline line = (pgeo as IClone).Clone() as IPolyline;
                    TopologyHelper helper = new TopologyHelper(m_Application.ActiveView);
                    var topgy = TopologyApplication.Topology;
                    ITopologyGraph graph = helper.CreateTopGraph(topgy);

                    var pl = SimplifyByDTAlgorithm.SimplifyByDT(line as IPolycurve, SMHeigth, SMWidth);

                    IPointCollection reshapePath = new PathClass();
                    reshapePath.AddPointCollection(pl as IPointCollection);


                    graph.SetEdgeGeometry(TopologyApplication.SelEdge, reshapePath as IPath);

                    graph.Post(out penvelope);
                    IPoint midpoint=new PointClass();
                    pl.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, midpoint);
                    helper.QueryTopEdge(graph, midpoint);
                   // TopologyApplication.SelPolyline = TopologyApplication.SelEdge.Geometry as IPolyline;
                   // m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                    m_Application.ActiveView.Refresh();
                    GC.Collect();
                    pEditor.StopOperation("拓扑化简");
                }
            }
            catch
            {
                pEditor.AbortOperation();
            }
        }

       
        
    }
}

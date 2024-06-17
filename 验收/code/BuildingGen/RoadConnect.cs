using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
namespace BuildingGen
{
    public class RoadConnect : BaseGenCommand
    {
        ITinLayer2 tinlayer;
        public RoadConnect()
        {
            base.m_category = "GRoad";
            base.m_caption = "道路连接";
            base.m_message = "连接道路";
            base.m_toolTip = "连接道路";
            base.m_name = "RoadConnect";
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
            IFeatureLayer layer = m_application.Workspace.EditLayer.Layer as IFeatureLayer;
            IFeatureClass fc = layer.FeatureClass;
            if (layer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
            {
                return;
            }

            m_application.EngineEditor.StartOperation();
            try
            {
                TinClass tin = new TinClass();
                tin.InitNew(m_application.MapControl.ActiveView.FullExtent);
                tin.StartInMemoryEditing();

                ISelectionSet set = (layer as IFeatureSelection).SelectionSet;
                IEnumIDs ids = set.IDs;
                int id = -1;

                IPoint p = new PointClass();
                p.Z = 0;
                ITinNode node = new TinNodeClass();
                Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
                while ((id = ids.Next()) != -1)
                {
                    IPolyline line = layer.FeatureClass.GetFeature(id).Shape as IPolyline;
                    p.X = line.FromPoint.X;
                    p.Y = line.FromPoint.Y;
                    tin.AddPointZ(p, 1, node);
                    if (!dic.ContainsKey(node.Index))
                    {
                        dic[node.Index] = new List<int>();
                    }
                    dic[node.Index].Add(id);
                    p.X = line.ToPoint.X;
                    p.Y = line.ToPoint.Y;
                    tin.AddPointZ(p, 1, node);
                    if (!dic.ContainsKey(node.Index))
                    {
                        dic[node.Index] = new List<int>();
                    }
                    dic[node.Index].Add(-id);
                }

                List<List<int>> groups = new List<List<int>>();
                for (int i = 1; i <= tin.NodeCount; i++)
                {
                    ITinNode n = tin.GetNode(i);
                    if (n.TagValue != -1 && n.IsInsideDataArea)
                    {
                        List<int> g = new List<int>();
                        FindNode(n, g, 9);
                        if (g.Count > 1)
                        {
                            groups.Add(g);
                        }
                    }
                }

                foreach (List<int> g in groups)
                {
                    IPoint centerPoint = new PointClass();
                    centerPoint.X = 0;
                    centerPoint.Y = 0;
                    int count = 0;

                    foreach (int nid in g)
                    {
                        List<int> fids = dic[nid];
                        foreach (int fid in fids)
                        {
                            IFeature feature = fc.GetFeature(Math.Abs(fid));
                            IPolyline line = feature.Shape as IPolyline;
                            IPoint pend = fid > 0 ? line.FromPoint : line.ToPoint;
                            centerPoint.X += pend.X;
                            centerPoint.Y += pend.Y;
                            count++;
                        }
                    }
                    centerPoint.X = centerPoint.X / count;
                    centerPoint.Y = centerPoint.Y / count;
                    foreach (int nid in g)
                    {
                        List<int> fids = dic[nid];
                        foreach (int fid in fids)
                        {
                            IFeature feature = fc.GetFeature(Math.Abs(fid));
                            IPolyline line = feature.ShapeCopy as IPolyline;
                            if (fid > 0) line.FromPoint = centerPoint;
                            else line.ToPoint = centerPoint;
                            feature.Shape = line;
                            feature.Store();
                        }
                    }
                }

                if (tinlayer == null)
                {
                    tinlayer = new TinLayerClass();
                    m_application.MapControl.AddLayer(tinlayer);
                }
                tinlayer.Dataset = tin;
                TinEdgeRendererClass renderer = new TinEdgeRendererClass();
                ESRI.ArcGIS.Display.SimpleLineSymbolClass sl = new ESRI.ArcGIS.Display.SimpleLineSymbolClass();
                //sl.Color = ColorFromRGB(255, 0, 0, false);
                sl.Width = 1;
                renderer.Symbol = sl;
                tinlayer.ClearRenderers();
                tinlayer.AddRenderer(renderer);
                m_application.EngineEditor.StopOperation("线挂接");
                m_application.MapControl.Refresh();

            }
            catch
            {
                m_application.EngineEditor.AbortOperation();
            }


        }

        void FindNode(ITinNode seed, List<int> nodes, double distance)
        {
            ITinEdgeArray edges = seed.GetIncidentEdges();
            ITinEdit tin = seed.TheTin as ITinEdit;
            tin.SetNodeTagValue(seed.Index, -1);
            nodes.Add(seed.Index);
            for (int i = 0; i < edges.Count; i++)
            {
                ITinEdge edge = edges.get_Element(i);
                if (edge.Length < distance)
                {
                    if (edge.ToNode.IsInsideDataArea && edge.ToNode.TagValue != -1)
                    {
                        FindNode(edge.ToNode, nodes, distance);
                    }
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class RoadEndsProcessCmd : SMGICommand
    {

        public RoadEndsProcessCmd()
        {
            m_caption = "道路端头处理";
            m_toolTip = "道路端头处理";
            m_category = "道路端头处理";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            string err = "";
             using (var wo = m_Application.SetBusy())
             {
                 err = RoadEndsProcess(wo);
             }

             if (err == "")
             {
                 m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_Application.ActiveView.Extent);
             }
             else
             {
                 MessageBox.Show(err);
             }
        }

        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            messageRaisedAction("正在处理道路端头符号......");
            string err = RoadEndsProcess();
            if (err == "")
            {
                m_Application.Workspace.Save();
            }

            return err == "";
        }
        
        public static string RoadEndsProcess(WaitOperation wo = null)
        {
            string info = "";
            try
            {
                IFeatureLayer roadLayer = null;
                Dictionary<string, string> name2SQL = new Dictionary<string, string>();
                #region 读取配置文件,获取处理对象信息
                if (wo != null)
                    wo.SetText(string.Format("正在读取配置文件信息......"));

                string fileName = GApplication.Application.Template.Root + @"\专家库\RoadsEnds.xml";
                if (!File.Exists(fileName))
                {
                    return string.Format("没有找到配置文件：{0}", fileName);
                }
                var ruleDoc = XDocument.Load(fileName);
                var contentItem = ruleDoc.Element("RoadsEnds").Element("Content");
                if (contentItem != null)
                {
                    string fcName = contentItem.Attribute("FCName").Value.ToString().Trim().ToUpper();
                    roadLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
                    })).FirstOrDefault() as IFeatureLayer;
                    if (roadLayer == null)
                    {
                        return string.Format("没有找到道路要素类：{0}", fcName);
                    }

                    var items = contentItem.Elements("Item");
                    foreach (XElement ele in items)
                    {
                        name2SQL[ele.Value] = ele.Attribute("sql").Value.ToString();
                    }
                }
                #endregion

                IGeoFeatureLayer geoLayer = roadLayer as IGeoFeatureLayer;
                IMapContext mctx = new MapContextClass();
                mctx.Init(GApplication.Application.ActiveView.FocusMap.SpatialReference, 
                    GApplication.Application.ActiveView.FocusMap.ReferenceScale, geoLayer.AreaOfInterest);
                IRepresentationRenderer repRender = geoLayer.Renderer as IRepresentationRenderer;
                IRepresentationClass rpc = repRender.RepresentationClass;

                foreach(var kv in name2SQL)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在处理{0}......", kv.Key));

                    //1.将每种类型道路构建路网
                    RoadAlgorithm.RoadGraph graph = new RoadAlgorithm.RoadGraph();

                    IQueryFilter qf = new QueryFilterClass();
                    if(roadLayer.FeatureClass.FindField("RuleID") != -1)
                    {
                        qf.WhereClause = string.Format("({0}) and RuleID <> 1", kv.Value);
                    }
                    else
                    {
                        qf.WhereClause = kv.Value ;
                    }
                    IFeatureCursor feCursor = roadLayer.Search(qf, false);
                    IFeature fe = null;
                    while((fe = feCursor.NextFeature()) != null)
                    {
                        graph.Add(fe);
                    }
                    Marshal.ReleaseComObject(feCursor);

                    //2.刷选端头路
                    var endroads = graph.GetAllEndRoads();

                    //3.端头路 制图覆盖 盖为平角
                    foreach (var edge in endroads)
                    {
                        var rep = rpc.GetRepresentation(edge.Feature, mctx);
                        OverrideGapsValueSet(rep);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);


                info = ex.Message;
            }

            return info;
        }

        public static void OverrideGapsValueSet(IRepresentation rep)
        {
            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            for (int i = 0; i < ruleOrg.LayerCount; i++)
            {
                IBasicLineSymbol lineSym = ruleOrg.get_Layer(i) as IBasicLineSymbol;
                IGeometricEffects effects = ruleOrg.get_Layer(i) as IGeometricEffects;

                IGraphicAttributes ga = lineSym.Stroke as IGraphicAttributes;
                if (ga != null)
                {
                    int id = ga.get_IDByName("Caps");
                    rep.set_Value(ga, id, esriLineCapStyle.esriLCSButt);
                }
            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
            rep.UpdateFeature();
            rep.Feature.Store();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(rep);
        }
      
    }

    public static class RoadAlgorithm
    {
        public class RoadEdge
        {
            public RoadNode From { get; set; }
            public RoadNode To { get; set; }
          

            public int FeatureOID { get; set; }
            public IFeatureClass FeatureClass { get; set; }
            public IFeature Feature { get; set; }
            public int TreeID { get; set; }
          
            public RoadEdge(IFeature fe)
            {
                FeatureOID = fe.OID;
                TreeID = -1;
                FeatureClass = fe.Class as IFeatureClass;
                Feature = fe;
                From = null;
                To = null;
            }

        }

        public class PointWarp : IComparable<PointWarp>
        {
            public IPoint Point { get; private set; }

            public PointWarp(IPoint pt)
            {
                Point = pt;
            }
            public int CompareTo(PointWarp other)
            {
                return Point.Compare(other.Point);
            }
        }

        public class RoadNode
        {
            public IPoint Point { get; private set; }

            //邻接的道路
            public List<RoadEdge> AdjEdages { get; private set; }
            public RoadNode(IPoint pt)
            {
                Point = pt;
                AdjEdages = new List<RoadEdge>();
             
                
            }

        }

        public class RoadGraph
        {
            public SortedList<PointWarp, RoadNode> Nodes { get; set; }
            public List<RoadEdge> Edges { get; set; }
            public RoadGraph()
            {
                Nodes = new SortedList<PointWarp, RoadNode>();
                Edges = new List<RoadEdge>();
            }

            public void Add(IFeature feature)
            {
                RoadEdge edge = new RoadEdge(feature);
                Edges.Add(edge);
                IPolyline line = feature.Shape as IPolyline;
                {   //起点
                    var fromPoint = new PointWarp(line.FromPoint);
                    RoadNode node;
                    if (!Nodes.ContainsKey(fromPoint))
                    {
                        node = new RoadNode(fromPoint.Point);
                        Nodes.Add(fromPoint, node);
                    }
                    else
                    {
                        node = Nodes[fromPoint];
                    }

                    node.AdjEdages.Add(edge);
                    edge.From = node;
                }
                {  //终点
                    var toPoint = new PointWarp(line.ToPoint);
                    RoadNode node;
                    if (!Nodes.ContainsKey(toPoint))
                    {
                        node = new RoadNode(toPoint.Point);
                        Nodes.Add(toPoint, node);
                    }
                    else
                    {
                        node = Nodes[toPoint];
                    }

                    node.AdjEdages.Add(edge);
                    edge.To = node;
                }
            }
            /// <summary>
            /// 端头路
            /// </summary>
            /// <returns></returns>
            public IEnumerable<RoadEdge> GetAllEndRoads()
            {
                return from x in Edges where (x.From.AdjEdages.Count == 1 || x.To.AdjEdages.Count == 1) select x;
            }
            /// <summary>
            /// 端头节点
            /// </summary>
            /// <returns></returns>
            public IEnumerable<RoadNode> GetAllEndNodes()
            {
                return from x in Nodes.Values where x.AdjEdages.Count == 1 select x;
            }
        }
    }

  
}

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

namespace SMGI.Plugin.MapGeneralization
{
    public class POIHelper
    {
        GApplication _app;
        public POIHelper(GApplication app)
        {
            _app = app;
        }

        public string GetWhereClauseFromTemplate(string layerName, string ruleName, string tpName)
        {
            string whereClause = "";
            XElement contentXEle = _app.Template.Content;
            XElement ruleTP = contentXEle.Element(tpName);
            XElement ruleMatchXEle = ruleTP.Element("RuleMatch");
            string ruleMatchFileName = _app.Template.Root + "\\" + ruleMatchXEle.Value;
            DataTable _dtLayerRule = Helper.ReadToDataTable(ruleMatchFileName, "图层对照规则");
            if (_dtLayerRule.Rows.Count > 0)
            {
                DataRow[] rows = _dtLayerRule.Select("图层='" + layerName + "' and RuleName='" + ruleName + "'");
                if (rows.Length > 0)
                {
                    whereClause = rows[0]["定义查询"].ToString();
                }
            }
            return whereClause;
        }

        public bool[] MultiPointSelection(IMultipoint points, double rate, WaitOperation wo)
        {
            if (points==null || points.IsEmpty)
            {
                return null;
            }
            TinClass tin = new TinClass();
            tin.InitNew(points.Envelope);
            tin.StartInMemoryEditing();
            IPointCollection pc = points as IPointCollection;
            List<int> tinNodeIndex = new List<int>();
            wo.SetMaxValue(tin.NodeCount);
            for (int i = 0; i < pc.PointCount; i++)
            {
                if (i % 1000 == 0)
                    wo.SetText(string.Format("正在构建三角网[{0}]", i));
                //wo.Step(pc.PointCount);

                IPoint p = pc.get_Point(i);
                p.Z = 0;
                tin.AddPointZ(p, i);
            }
            double maxArea = double.MinValue;
            List<NodeWarp> nodes = new List<NodeWarp>();
            Dictionary<int, NodeWarp> dic = new Dictionary<int, NodeWarp>();
            wo.SetMaxValue(tin.NodeCount);
            for (int i = 1; i <= tin.NodeCount; i++)
            {
                if (i % 1000 == 0)
                    wo.SetText(string.Format("正在计算节点密度[{0}]", i));
                //wo.Step(tin.NodeCount);

                ITinNode node = tin.GetNode(i);
                if (!node.IsInsideDataArea)
                    continue;
                NodeWarp warp = new NodeWarp(node);
                warp.State = NodeWarp.eState.Normal;
                if (warp.Area > maxArea)
                    maxArea = warp.Area;
                nodes.Add(warp);
                dic.Add(warp.Index, warp);
            }
            //Comparison<NodeWarp> cmp = new Comparison<NodeWarp>(NodeCMP); 
            wo.Step(0);
            wo.SetText("正在排序。。。");
            nodes.Sort(NodeCMP);
            int deleteCount = 0;
            int ci = 0;
            while (deleteCount < pc.PointCount * (1 - rate))
            {
                if (ci == nodes.Count)
                {
                    int changeCount = 0;
                    foreach (var nw in nodes)
                    {
                        if (nw.State == NodeWarp.eState.Locked)
                        {
                            nw.State = NodeWarp.eState.Normal;
                            changeCount++;
                        }
                    }
                    if (changeCount == 0)
                        break;
                    ci = 0;
                }
                NodeWarp n = nodes[ci];
                ci++;

                if (n.State != NodeWarp.eState.Normal)
                {
                    continue;
                }

                n.State = NodeWarp.eState.Delete;
                deleteCount++;
                ITinNodeArray array = n.Node.GetAdjacentNodes();
                for (int i = 0; i < array.Count; i++)
                {
                    ITinNode node = array.get_Element(i);
                    if (!node.IsInsideDataArea)
                        continue;
                    NodeWarp adNode = dic[node.Index];
                    if (adNode.State == NodeWarp.eState.Normal)
                        adNode.State = NodeWarp.eState.Locked;
                }
            }

            bool[] selected = new bool[pc.PointCount];
            foreach (var nw in nodes)
            {
                selected[nw.Tag] = (nw.State != NodeWarp.eState.Delete);
            }
            return selected;
        }

        public int NodeCMP(NodeWarp n1, NodeWarp n2)
        {
            try
            {
                return n1.Area.CompareTo(n2.Area);
            }
            catch
            {
                return 0;
            }
        }

        public void DelayTime(double second)
        {
            DateTime tempTime = DateTime.Now;
            while (tempTime.AddSeconds(second).CompareTo(DateTime.Now) > 0)
                Application.DoEvents();
        }
    }


    public class NodeWarp
    {
        public enum eState
        {
            Normal,
            Locked,
            Aways,
            Delete
        }
        public ITinNode Node { get; set; }
        public NodeWarp(ITinNode node)
        {
            this.Node = node;
            State = eState.Normal;
            Area = (Node.GetVoronoiRegion(null) as IArea).Area;
        }
        public double Area
        {
            get;
            private set;
        }
        public int Index { get { return Node.Index; } }
        public int Tag { get { return Node.TagValue; } }
        public eState State { get; set; }

    }
}

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
using System.IO;
using System.Runtime.InteropServices;
namespace SMGI.Plugin.EmergencyMap
{
    public class POIHelper
    {
        GApplication _app;
        public POIHelper(GApplication app)
        {
            _app = app;
        }

        public string GetWhereClauseFromTemplate(string layerName, string ruleName)
        {
            string whereClause = "";

            string ruleMatchFileName = EnvironmentSettings.getLayerRuleDBFileName(_app);
            DataTable _dtLayerRule = CommonMethods.ReadToDataTable(ruleMatchFileName, "图层对照规则");
            if (_dtLayerRule.Rows.Count > 0)
            {
                DataRow[] rows = _dtLayerRule.Select("图层='" + layerName + "' and RuleName='" + ruleName + "'");
                if (rows.Length > 0)
                {
                    whereClause = rows[0]["定义查询"].ToString();
                }
                else
                {
                    //图层对照规则表中不存在该规则
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
            if (wo != null)
                wo.SetMaxValue(tin.NodeCount);
            for (int i = 0; i < pc.PointCount; i++)
            {
                if (wo != null && i % 1000 == 0)
                    wo.SetText(string.Format("正在构建三角网[{0}]", i));
                IPoint p = pc.get_Point(i);
                p.Z = 0;
                tin.AddPointZ(p, i);
            }
            double maxArea = double.MinValue;
            List<NodeWarp> nodes = new List<NodeWarp>();
            Dictionary<int, NodeWarp> dic = new Dictionary<int, NodeWarp>();
            if (wo != null)
                wo.SetMaxValue(tin.NodeCount);
            for (int i = 1; i <= tin.NodeCount; i++)
            {
                if (wo != null && i % 1000 == 0)
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
            //  SortedList
            //Comparison<NodeWarp> cmp = new Comparison<NodeWarp>(NodeCMP); 
            if (wo != null)
            {
                wo.Step(0);
                wo.SetText("正在排序。。。");
            }
            
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
      
        
        /// <summary>
        /// 顾及道路关系的POI选取 加锁方式选取
        /// </summary>
        public bool[] MultiPointSelectionReLock(IMultipoint points, double rate, WaitOperation wo)
        {
            if (points == null || points.IsEmpty)
            {
                return null;
            }
            TinClass tin = new TinClass();
            tin.InitNew(points.Envelope);
            tin.StartInMemoryEditing();
            IPointCollection pc = points as IPointCollection;
            List<int> tinNodeIndex = new List<int>();
            if (wo != null)
                wo.SetMaxValue(tin.NodeCount);
            Dictionary<int, int> roadPoint = new Dictionary<int, int>();
            int level = 0;
            for (int i = 0; i < pc.PointCount; i++)
            {
                if (wo != null && i % 1000 == 0)
                    wo.SetText(string.Format("正在构建三角网[{0}]", i));
                IPoint p = pc.get_Point(i);
                p.Z = 0;
                roadPoint[i] = p.ID;
                level = level < p.ID ? p.ID : level;
                tin.AddPointZ(p, i);
            }
            double maxArea = double.MinValue;
            List<NodeWarp> nodes = new List<NodeWarp>();
            Dictionary<int, NodeWarp> dic = new Dictionary<int, NodeWarp>();
            if (wo != null)
                wo.SetMaxValue(tin.NodeCount);
            for (int i = 1; i <= tin.NodeCount; i++)
            {
                if (wo != null && i % 1000 == 0)
                    wo.SetText(string.Format("正在计算节点密度[{0}]", i));
                ITinNode node = tin.GetNode(i);
                if (!node.IsInsideDataArea)
                    continue;

                NodeWarp warp = new NodeWarp(node);
                warp.State = NodeWarp.eState.Normal;
                int id = roadPoint[node.TagValue];
                warp.PowerValue = id;//节点权重
                if (id > 0)//交叉口的POI
                {
                    warp.State = NodeWarp.eState.Aways;//交叉口的居民点 不删除
                }
                if (warp.Area > maxArea)
                    maxArea = warp.Area;
                nodes.Add(warp);
                dic.Add(warp.Index, warp);
            }
            if (wo != null)
            {
                wo.Step(0);
                wo.SetText("正在排序。。。");
            }

            nodes.Sort(NodeCMP);
            int deleteCount = 0;
            int ci = 0;
            #region
            //第一次 交叉口上的要素点 状态改为 Allways.可能出现 选取比例不足
            //while (deleteCount < pc.PointCount * (1 - rate))
            //{
            //    #region
            //    if (ci == nodes.Count)
            //    {
            //        int changeCount = 0;
            //        foreach (var nw in nodes)
            //        {
            //            if (nw.State == NodeWarp.eState.Locked)
            //            {
            //                nw.State = NodeWarp.eState.Normal;
            //                changeCount++;
            //            }
            //        }
            //        if (changeCount == 0)
            //            break;
            //        ci = 0;
            //    }
            //    NodeWarp n = nodes[ci];
            //    ci++;

            //    if (n.State != NodeWarp.eState.Normal)
            //    {
            //        continue;
            //    }

            //    n.State = NodeWarp.eState.Delete;
            //    deleteCount++;
            //    ITinNodeArray array = n.Node.GetAdjacentNodes();
            //    for (int i = 0; i < array.Count; i++)//邻接节点加锁
            //    {
            //        ITinNode node = array.get_Element(i);
            //        if (!node.IsInsideDataArea)
            //            continue;
            //        NodeWarp adNode = dic[node.Index];
            //        if (adNode.State == NodeWarp.eState.Normal)
            //            adNode.State = NodeWarp.eState.Locked;
            //    }
            //    #endregion
            //}
            //当比例不足时，处理Allways状态的节点
            #endregion
            // 交叉口上的要素点 状态改为 Allways.可能出现 选取比例不足，直到deleteCount满足要求
            for(int step=0;step<=level;step++)
            {
                ci = 0;
                foreach (var nw in nodes)
                {
                    if (nw.State == NodeWarp.eState.Aways)//将权重不够的节点，更改状态为常规
                    {
                        if (nw.PowerValue <= step)
                            nw.State = NodeWarp.eState.Normal;
                    }
                }
                while (deleteCount < pc.PointCount * (1 - rate))
                {
                    #region
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
                    #endregion
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
                if (deleteCount == pc.PointCount * (1 - rate))
                    break;
            }
            bool[] selected = new bool[pc.PointCount];
            foreach (var nw in nodes)
            {
                selected[nw.Tag] = (nw.State != NodeWarp.eState.Delete);
            }
            Marshal.ReleaseComObject(tin);
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

        /// <summary>
        /// 读取POI选取的过滤条件文件，返回各要素类中不参与POI选取的名称几何
        /// AGNP:d、的、
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> getFilterNamesOfPOISelection(string fileName)
        {
            Dictionary<string, List<string>> result = null;

            if (fileName == "" || !System.IO.File.Exists(fileName))
            {
                return result;
            }

            var file = File.Open(fileName, FileMode.Open);
            using( var stream = new StreamReader(file, Encoding.Default))
            {
                while(!stream.EndOfStream)
                {
                    string line = stream.ReadLine().ToUpper();
                    var ns = line.Split(new string[] { ":", "："}, StringSplitOptions.RemoveEmptyEntries);
                    if(ns.Count() < 2)
                        continue;

                    string val1 = ns[0];
                    string val2 = ns[1];
                    var names = val2.Split('、');

                    if (names.Count() == 0)
                        continue;

                    if (result == null)
                        result = new Dictionary<string, List<string>>();

                    if (!result.ContainsKey(val1))
                    {
                        result.Add(val1, names.ToList());
                    }
                    else
                    {
                        result[val1].AddRange(names.ToArray());
                    }
                }
            }

            return result;
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
        public int PowerValue { get; set; }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.MapGeneralization
{
    public class HydroLineSelectCmd:SMGITool
    {
        private INewEnvelopeFeedback fb;

        public HydroLineSelectCmd()
        {
        }

        internal class HydroLineInfo : IComparable<HydroLineInfo>
        {

            #region 要素信息
            /// <summary>
            /// 对应的IFeature
            /// </summary>
            internal IFeature Feature { get; set; }

            /// <summary>
            /// FromPoint是不是水源
            /// </summary>
            internal bool FromPointAtSource { get; set; }
            /// <summary>
            /// 长度
            /// </summary>
            internal double Lenght { get; set; }

            #endregion


            #region 最短路径信息
            /// <summary>
            /// 没有形成环路则有Child必然有Neighbors
            /// 但是环路中可能存在没有Child但是有Neighbors的情况
            /// </summary>
            internal int ChildCount { get; set; }

            /// <summary>
            /// 上游邻接的河流
            /// </summary>
            internal List<HydroLineInfo> Neighbors { get; set; }
            /// <summary>
            /// 上游邻接河流的交点是不是ToPoint，与FromPointAtSource意义一致
            /// </summary>
            internal List<bool> NeighborsSide { get; set; }

            /// <summary>
            /// 这条线段的河源到达跟节点的最短路径长度
            /// </summary>
            internal double DistanceFromRoot { get; set; }

            /// <summary>
            /// 最短路径上的父节点
            /// </summary>
            internal HydroLineInfo ShortestPathParent { get; set; }
            #endregion

            #region 树结构ID
            /// <summary>
            /// 最长水系树的PathID
            /// 每条河流只属于一个Path，该Path能够拥有最长的长度
            /// </summary>
            internal int PathID { get; set; }
            /// <summary>
            /// 仅算到这个河段（PathID相等），并不从河源算到河口
            /// </summary>
            internal double PathLenght
            {
                get
                {
                    var c = this;
                    double l = 0;
                    while (c != null && c.PathID == this.PathID)
                    {
                        l += c.Lenght;
                        c = c.ShortestPathParent;
                    }
                    return l;
                }
            }
            #endregion

            internal HydroLineInfo(IFeature feature, bool fromPointAtSource, HydroLineInfo shortestPathParent)
            {
                Feature = feature;
                Lenght = (feature.Shape as IPolycurve).Length;
                ShortestPathParent = shortestPathParent;
                if (shortestPathParent != null)
                    shortestPathParent.ChildCount++;
                DistanceFromRoot = double.MaxValue;
                Neighbors = new List<HydroLineInfo>();
                NeighborsSide = new List<bool>();
                ChildCount = 0;
                FromPointAtSource = fromPointAtSource;
                PathID = -1;
            }

            /// <summary>
            /// 寻找与当前线的水源邻接的线，记录在Neighbors中
            /// </summary>
            /// <param name="infos">用于判断被找到的线有没有被其他线找到过</param>
            internal void FindNeighbors(Dictionary<int, HydroLineInfo> infos)
            {
                ISpatialFilter sf = new SpatialFilter();
                IPoint queryPoint = this.FromPointAtSource
                    ? (this.Feature.Shape as IPolycurve).FromPoint
                    : (this.Feature.Shape as IPolycurve).ToPoint;

                sf.Geometry = queryPoint;//(queryPoint as ITopologicalOperator).Buffer();
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureClass fc = this.Feature.Class as IFeatureClass;
                IFeatureCursor fcursor = fc.Search(sf, false);
                IFeature ffe = null;
                while ((ffe = fcursor.NextFeature()) != null)
                {
                    if (ffe.OID == this.Feature.OID)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(ffe);
                        continue;
                    }
                    HydroLineInfo nb = null;
                    bool intersectAtToPoint = ((sf.Geometry as IRelationalOperator).Contains((ffe.Shape as IPolycurve).ToPoint));
                    if (infos.ContainsKey(ffe.OID))
                    {
                        nb = infos[ffe.OID];
                    }
                    else
                    {
                        nb = new HydroLineInfo(ffe, intersectAtToPoint, this);
                        infos.Add(ffe.OID, nb);
                    }

                    this.NeighborsSide.Add(intersectAtToPoint);
                    this.Neighbors.Add(nb);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fcursor);
            }

            public int CompareTo(HydroLineInfo other)
            {
                var r = this.DistanceFromRoot.CompareTo(other.DistanceFromRoot);
                if (r == 0)
                {
                    return this.Feature.OID.CompareTo(other.Feature.OID);
                }
                return r;
            }
        }

        internal class HydroRoot : HydroLineInfo
        {
            internal Dictionary<int, HydroLineInfo> HydroLineInfos { get; set; }
            internal Dictionary<int, List<HydroLineInfo>> PathInfos { get; set; }
            internal HydroRoot(IFeature feature, bool fromPointAtSource)
                : base(feature, fromPointAtSource, null)
            {
                HydroLineInfos = new Dictionary<int, HydroLineInfo>();
                PathInfos = new Dictionary<int, List<HydroLineInfo>>();
                DistanceFromRoot = Lenght;
                HydroLineInfos.Add(feature.OID, this);
                Build();
            }

            #region 构建最短路径树结构
            void Build()
            {
                SortedSet<HydroLineInfo> info = new SortedSet<HydroLineInfo>();
                info.Add(this);
                while (info.Count > 0)
                {
                    BuildGraph(info);
                }
            }

            void BuildGraph(SortedSet<HydroLineInfo> findInfo)
            {
                var current = findInfo.Min;
                findInfo.Remove(current);
                current.FindNeighbors(HydroLineInfos);
                for (int i = 0; i < current.Neighbors.Count; i++)
                {
                    var neighbor = current.Neighbors[i];
                    double dis = current.DistanceFromRoot + neighbor.Lenght;
                    if (dis > neighbor.DistanceFromRoot)
                    {
                        continue;
                    }

                    neighbor.DistanceFromRoot = dis;
                    if (neighbor.ShortestPathParent != null)
                        neighbor.ShortestPathParent.ChildCount--;

                    neighbor.ShortestPathParent = current;
                    neighbor.FromPointAtSource = current.NeighborsSide[i];

                    findInfo.Remove(neighbor);
                    findInfo.Add(neighbor);
                    current.ChildCount++;
                }
            }
            #endregion

            #region 计算最长路径信息
            internal List<HydroLineInfo> SetPath()
            {
                var allPath = from x in HydroLineInfos.Values
                              where x.ChildCount <= 0
                              orderby x.DistanceFromRoot descending
                              select x;

                int currentPathID = 0;

                foreach (var path in allPath)
                {
                    var currentLine = path;

                    while (currentLine != null && currentLine.PathID < 0)
                    {
                        currentLine.PathID = currentPathID;
                        if (!PathInfos.ContainsKey(currentPathID))
                        {
                            PathInfos.Add(currentPathID, new List<HydroLineInfo>());
                        }
                        PathInfos[currentPathID].Add(currentLine);
                        currentLine = currentLine.ShortestPathParent;
                    }
                    currentPathID++;
                }
                var list = from x in allPath
                           orderby x.PathLenght descending
                           select x;
                return list.ToList();
            }
            #endregion

            /// <summary>
            /// 选取河段
            /// </summary>
            /// <param name="sources">河源信息，按长度逆序排列（长的在前）</param>
            /// <param name="rate">选取比例</param>
            /// <returns>应该被删除的FID</returns>
            internal List<int> Select(List<HydroLineInfo> sources, double rate)
            {
                List<int> result = new List<int>();
                double allLength = 0;
                foreach (var path in sources)
                {
                    allLength += path.PathLenght;
                }
                double currentLength = allLength;
                for (int i = sources.Count - 1; i >= 0; i--)
                {
                    var path = sources[i];
                    currentLength -= path.PathLenght;
                    if (currentLength / allLength < rate)
                    {
                        break;
                    }

                    var c = path;
                    while (c != null && c.PathID == path.PathID)
                    {
                        result.Add(c.Feature.OID);
                        c = c.ShortestPathParent;
                    }
                }
                return result;
            }
        }

        IFeatureClass fc = null;
        IFeature rootFeature = null;
        List<int> deletedFeatures = null;
        HydroRoot rootInfo = null;

        [SMGIParameter("河流选取比例",0.1,"河流选取（保留）比例，按长度计算")]
        public double SelectRate { get; set; }

        public override bool Enabled
        {
            get
            {
                return m_Application != null
                    && m_Application.Workspace != null;
            }
        }


        public override void OnClick()
        {
            var lyr= m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "HYDL")).FirstOrDefault();
            fc = (lyr as IFeatureLayer).FeatureClass;
        }

       
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;

            IPoint pPt =ToSnapedMapPoint(x, y);
            if (fb == null && button == 1)
            {
                fb = new NewEnvelopeFeedbackClass();
                fb.Display = m_Application.ActiveView.ScreenDisplay;
                fb.Start(pPt);
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            IPoint pPt = ToSnapedMapPoint(x, y);
            if (fb != null && button == 1)
            {
                fb.MoveTo(pPt);
            }
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (fb == null)
                return;

            if (deletedFeatures != null)
            {
                IEnvelope env = fb.Stop();
                fb = null;
                var f = SelectFeature(env);
                if (f == null)
                    return;

                if (rootInfo.HydroLineInfos.ContainsKey(f.OID))
                {
                    if (shift == 1)
                    {
                        HydroLineInfo info = rootInfo.HydroLineInfos[f.OID];
                        var pid = info.PathID;
                        var plist = rootInfo.PathInfos[pid];
                        foreach (var ii in plist)
                        {
                            int idx = deletedFeatures.IndexOf(ii.Feature.OID);
                            if (idx >= 0)
                            {
                                deletedFeatures.RemoveAt(idx);
                            }
                            else
                            {
                                deletedFeatures.Add(f.OID);
                            }
                        }
                    }
                    else
                    {
                        int idx = deletedFeatures.IndexOf(f.OID);
                        if (idx >= 0)
                        {
                            deletedFeatures.RemoveAt(idx);
                        }
                        else
                        {
                            deletedFeatures.Add(f.OID);
                        }
                    }
                    m_Application.MapControl.Refresh();
                }
                else
                {
                    Clear();
                    rootFeature = f;
                    m_Application.MapControl.Refresh();
                }
            }
            else if (rootFeature == null)
            {
                IEnvelope env = fb.Stop();
                fb = null;
                rootFeature = SelectFeature(env);
                m_Application.MapControl.Refresh();
            }
            else
            {
                IRelationalOperator envRel = fb.Stop() as IRelationalOperator;
                fb = null;
                bool fromPointInEnv = envRel.Contains((rootFeature.Shape as IPolycurve).FromPoint);
                bool toPointInEnv = envRel.Contains((rootFeature.Shape as IPolycurve).ToPoint);

                if (fromPointInEnv == toPointInEnv)
                {
                    rootFeature = SelectFeature(envRel as IEnvelope);
                    m_Application.MapControl.Refresh();
                    return;
                }

                HydroRoot root = new HydroRoot(rootFeature, toPointInEnv);
                var paths = root.SetPath();
                this.LoadParameters();
                deletedFeatures = root.Select(paths, this.SelectRate);
                rootInfo = root;

                m_Application.ActiveView.Refresh();
            }
        }

        IFeature SelectFeature(IEnvelope env)
        {
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = env;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            IFeatureCursor fcursor = fc.Search(sf, false);
            var f = fcursor.NextFeature();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fcursor);
            return f;
        }

        public override void Refresh(int hdc)
        {
            if (fb != null)
                fb.Refresh(hdc);

            if (rootFeature != null)
            {
                IDisplay dis = new SimpleDisplayClass();
                dis.DisplayTransformation = m_Application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation;

                dis.StartDrawing(hdc, -1);

                SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
                RgbColorClass c = new RgbColorClass();
                c.Red = 0;
                c.Blue = 255;
                c.Green = 0;

                sls.Color = c;
                sls.Width = 2;

                dis.SetSymbol(sls as ISymbol);
                dis.DrawPolyline(rootFeature.Shape);

                SimpleMarkerSymbolClass sms = new SimpleMarkerSymbolClass();
                c.Red = 255;
                c.Blue = 0;
                c.Green = 0;

                sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
                sms.Color = c;
                dis.SetSymbol(sms);
                dis.DrawPoint((rootFeature.Shape as IPolycurve).ToPoint);

                dis.FinishDrawing();
            }

            if (fc != null && deletedFeatures != null)
            {
                var env = m_Application.MapControl.ActiveView.Extent as IRelationalOperator;

                SimpleLineSymbolClass sls = new SimpleLineSymbolClass();

                RgbColorClass c = new RgbColorClass();
                c.Red = 230;
                c.Blue = 230;
                c.Green = 230;

                sls.Color = c;
                sls.Width = 2;

                IDisplay dis = new SimpleDisplayClass();
                dis.DisplayTransformation = m_Application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation;

                dis.StartDrawing(hdc, -1);
                dis.SetSymbol(sls);

                foreach (var fid in deletedFeatures)
                {
                    IFeature f = fc.GetFeature(fid);
                    if (env.Disjoint(f.Shape))
                        continue;
                    dis.DrawPolyline(f.Shape);
                }
                dis.FinishDrawing();
            }
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            if (keyCode == (int)Keys.Escape)
            {
                Clear();
                m_Application.MapControl.Refresh();
            }

            if (keyCode == (int)Keys.Delete)
            {
                if (m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                {
                    m_Application.EngineEditor.StartOperation();
                }
                foreach (var fid in deletedFeatures)
                {
                    IFeature f = fc.GetFeature(fid);
                    f.Delete();
                }
                Clear();
                if (m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                {
                    m_Application.EngineEditor.StopOperation(this.Caption);
                }
                m_Application.MapControl.Refresh();
            }
        }

        void Clear()
        {
            this.deletedFeatures = null;
            this.rootFeature = null;
            this.rootInfo = null;
        }
    }
}

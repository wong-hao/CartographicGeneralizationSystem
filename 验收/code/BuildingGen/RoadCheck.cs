using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen
{
    public class RoadCheck : BaseGenTool
    {
        GLayerInfo roadLayer;
        SimpleFillSymbolClass sfs;
        SimpleMarkerSymbolClass sms;
        public RoadCheck()
        {
            base.m_category = "GRoad";
            base.m_caption = "连通性检查";
            base.m_message = "道路检查";
            base.m_toolTip = "检查并修正道路连通性。\n按r键重新进行检查。\n按n键跳至下一个错误。\n在错误地区单击左键自动处理错误;\n在错误地区单击右键自动忽略错误；\n按空格自动处理所有错误；\n按回车自动忽略所有错误。";
            base.m_name = "RoadCheck";
            m_usedParas = new GenDefaultPara[] 
            {
                new GenDefaultPara("道路挂接距离",(double)0.5)
            };
            roadLayer = null;
            sfs = new SimpleFillSymbolClass();
            SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 0;
            rgb.Green = 0;
            rgb.Blue = 255;
            sls.Color = rgb;
            sls.Width = 2;
            sfs.Outline = sls;
            sfs.Style = esriSimpleFillStyle.esriSFSNull;

            sms = new SimpleMarkerSymbolClass();
            sms.OutlineColor = rgb;
            sms.Style = esriSimpleMarkerStyle.esriSMSCircle;
            sms.OutlineSize = 2;
            sms.Size = 15;
            sms.Outline = true;
            rgb.NullColor = true;
            sms.Color = rgb;

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
            roadLayer = null;
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers)
            {
                if (info.LayerType == GCityLayerType.道路
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    )
                {
                    roadLayer = info;
                    break;
                }
            }
            if (roadLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到道路图层");
                return;
            }
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            if (gb != null && gb.Count > 0)
            {
                if (System.Windows.Forms.MessageBox.Show("已经计算过连通关系，是否重新计算？", "提示", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                {
                    m_application.MapControl.Refresh();
                    return;
                }
            }
            Analysis();
            m_application.MapControl.Refresh();
        }

        void FindNode(ITinNode seed, Dictionary<int, IPoint> nodes, double distance)
        {
            ITinEdgeArray edges = seed.GetIncidentEdges();
            ITinEdit tin = seed.TheTin as ITinEdit;
            tin.SetNodeTagValue(seed.Index, -1);
            PointClass p = new PointClass();
            p.X = seed.X;
            p.Y = seed.Y;
            nodes.Add(seed.Index, p);
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

        TinClass tin;
        Dictionary<int, List<int>> nID_fID_dic;
        List<Dictionary<int, IPoint>> groups;
        GeometryBagClass gb;
        int currentErr;
        void Analysis()
        {
            double dis = (double)m_application.GenPara["道路挂接距离"];
            WaitOperation wo = m_application.SetBusy(true);
            wo.SetText("正在进行分析准备……");
            tin = new TinClass();
            tin.InitNew(m_application.MapControl.ActiveView.FullExtent);
            tin.StartInMemoryEditing();

            int featureCount = (roadLayer.Layer as IFeatureLayer).FeatureClass.FeatureCount(null);
            IFeatureCursor fCursor = (roadLayer.Layer as IFeatureLayer).Search(null, true);
            IFeature feature = null;
            IPoint p = new PointClass();
            p.Z = 0;
            ITinNode node = new TinNodeClass();
            nID_fID_dic = new Dictionary<int, List<int>>();
            while ((feature = fCursor.NextFeature()) != null)
            {
                wo.SetText("正在进行分析准备:" + feature.OID.ToString());
                wo.Step(featureCount);
                IPolyline line = feature.Shape as IPolyline;
                p.X = line.FromPoint.X;
                p.Y = line.FromPoint.Y;
                tin.AddPointZ(p, 1, node);
                if (!nID_fID_dic.ContainsKey(node.Index))
                {
                    nID_fID_dic[node.Index] = new List<int>();
                }
                nID_fID_dic[node.Index].Add(feature.OID);
                p.X = line.ToPoint.X;
                p.Y = line.ToPoint.Y;
                tin.AddPointZ(p, 1, node);
                if (!nID_fID_dic.ContainsKey(node.Index))
                {
                    nID_fID_dic[node.Index] = new List<int>();
                }
                nID_fID_dic[node.Index].Add(-feature.OID);
            }
            wo.Step(0);
            groups = new List<Dictionary<int, IPoint>>();
            for (int i = 1; i <= tin.NodeCount; i++)
            {
                wo.SetText("正在分析:" + i.ToString());
                wo.Step(tin.NodeCount);

                ITinNode n = tin.GetNode(i);
                if (n.TagValue != -1 && n.IsInsideDataArea)
                {
                    Dictionary<int, IPoint> g = new Dictionary<int, IPoint>();
                    FindNode(n, g, dis);
                    if (g.Count > 1)
                    {
                        groups.Add(g);
                    }
                }
            }
            gb = new GeometryBagClass();
            object miss = Type.Missing;
            wo.Step(0);
            wo.SetText("正在整理分析结果");

            foreach (Dictionary<int, IPoint> g in groups)
            {
                wo.Step(groups.Count);

                MultipointClass mp = new MultipointClass();
                //centerPoint.X = 0;
                //centerPoint.Y = 0;
                //int count = 0;

                foreach (int nid in g.Keys)
                {
                    IPoint pend = g[nid];
                    mp.AddGeometry(pend, ref miss, ref miss);
                    //centerPoint.X += pend.X;
                    //centerPoint.Y += pend.Y;
                    //count++;
                }
                //centerPoint.X = centerPoint.X / count;
                //centerPoint.Y = centerPoint.Y / count;
                gb.AddGeometry(mp, ref miss, ref miss);
            }
            currentErr = groups.Count;
            m_application.SetBusy(false);
            System.Windows.Forms.MessageBox.Show("检查完成，共" + groups.Count + "个错误");
        }

        public override bool Deactivate()
        {
            m_application.MapControl.OnAfterDraw -= new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            return true;
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e)
        {
            IDisplay dis = e.display as IDisplay;

            if (gb != null)
            {
                bool drawEnv = dis.DisplayTransformation.FromPoints(15) < 8;
                if (drawEnv)
                {
                    dis.SetSymbol(sfs);
                    for (int i = 0; i < gb.GeometryCount; i++)
                    {
                        IEnvelope env = gb.get_Geometry(i).Envelope;
                        env.Expand(4, 4, false);
                        dis.DrawRectangle(env);
                    }
                }


                dis.SetSymbol(sms);
                for (int i = 0; i < gb.GeometryCount; i++)
                {
                    dis.DrawMultipoint(gb.get_Geometry(i));
                }

            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            double sDis = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(5);
            double distance = 0;
            int part = -1;
            int seg = -1;
            bool rightSide = false;
            IPoint hitPoint = new PointClass();
            for (int i = 0; i < gb.GeometryCount; i++)
            {
                IHitTest hit = gb.get_Geometry(i).Envelope as IHitTest;
                if (hit.HitTest(p, sDis, esriGeometryHitPartType.esriGeometryPartVertex/* | esriGeometryHitPartType.esriGeometryPartCentroid*/, hitPoint, ref distance, ref  part, ref seg, ref rightSide))
                {
                    try
                    {
                        AutoProcess(i, Button == 1);
                        //Next();
                        m_application.MapControl.Refresh();
                    }
                    catch
                    {
                    }
                    break;
                }
            }
        }


        void AutoProcess(int index, bool commit)
        {
            if (commit)
            {
                Dictionary<int, IPoint> group = groups[index];
                IFeatureClass fc = ((roadLayer.Layer) as IFeatureLayer).FeatureClass;
                IGeometry geo = gb.get_Geometry(index);
                IEnvelope env = geo.Envelope;
                IPoint p = (env as IArea).Centroid;

                foreach (var item in group.Keys)
                {
                    List<int> fids = nID_fID_dic[item];
                    foreach (var fid in fids)
                    {
                        IFeature feature = fc.GetFeature(Math.Abs(fid));
                        IPolyline line = feature.ShapeCopy as IPolyline;
                        if (fid > 0)
                        {
                            line.FromPoint = p;
                        }
                        else
                        {
                            line.ToPoint = p;
                        }
                        feature.Shape = line;
                        feature.Store();

                    }
                }
            }
            groups.RemoveAt(index);
            gb.RemoveGeometries(index, 1);
        }
        void ProscessAll(bool commit)
        {
            WaitOperation wo = m_application.SetBusy(true);
            try
            {
                int count = gb.GeometryCount;
                for (int i = gb.GeometryCount-1; i >= 0; i--)
                {
                    AutoProcess(i, commit);
                    wo.Step(count);
                }
            }
            catch
            { 
            }
            m_application.SetBusy(false);
        }
        void Next()
        {
            if (groups == null || groups.Count == 0)
                return;
            currentErr--;
            if (currentErr < 0)
            {
                currentErr = groups.Count - 1;
            }
            if (currentErr > groups.Count - 1)
            {
                currentErr = 0;
            }
            IEnvelope env = gb.get_Geometry(currentErr).Envelope;
            env.Expand(8, 8, false);
            m_application.MapControl.Extent = env;
            m_application.MapControl.Refresh();
        }

        public override void OnKeyUp(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Space:
                    ProscessAll(true);
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.Enter:
                    ProscessAll(false);
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.R:
                    Analysis();
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.N:
                    Next();
                    break;
            }
        }
    }
}

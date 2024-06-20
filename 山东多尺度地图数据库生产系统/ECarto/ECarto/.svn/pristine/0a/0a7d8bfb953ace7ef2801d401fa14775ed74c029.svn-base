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
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessing;
using System.Collections;
namespace SMGI.Plugin.MapGeneralization
{
    public class DangleProcessCmd : SMGI.Common.SMGITool
    {
        ILayer curLayer;
        SimpleFillSymbolClass sfs;
        SimpleMarkerSymbolClass sms;
        //距离阈值
        double disValue;
        //编辑器
        IEngineEditor editor;
        //算法相关
        TinClass tin;
        Dictionary<int, List<int>> nID_fID_dic;
        List<Dictionary<int, IPoint>> groups;
        GeometryBagClass gb;
        int currentErr;

        public DangleProcessCmd()
        {
            //参数初始化
            Init();

        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        /// <summary>
        /// 自动刷新函数
        /// </summary>
        /// <param name="hDC"></param>
        public override void Refresh(int hDC)
        {
            var dis = m_Application.ActiveView.ScreenDisplay;
            dis.StartDrawing(hDC, 0);
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
            dis.FinishDrawing();

            //base.Refresh(hDC);
        }

        /// <summary>
        /// 单击响应函数
        /// </summary>
        public override void OnClick()
        {
            editor = m_Application.EngineEditor;
            var layerSelector = new LayerSelectForm(m_Application);
            layerSelector.GeoTypeFilter = esriGeometryType.esriGeometryPolyline;
            if (layerSelector.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var activeView = m_Application.ActiveView;
            curLayer = null;
            IMap pMap = activeView.FocusMap;
            curLayer = layerSelector.pSelectLayer;

            if (curLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到需要检查的图层");
                return;
            }

            disValue = Convert.ToDouble(layerSelector.tbValue.Text.Trim());

            if (gb != null && gb.Count > 0)
            {
                if (System.Windows.Forms.MessageBox.Show("已经计算过连通关系，是否重新计算？", "提示", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                {
                    activeView.Refresh();
                    return;
                }
            }
            Analysis();
            activeView.Refresh();
        }


        /// <summary>
        /// 鼠标单击响应函数
        /// </summary>
        /// <param name="Button"></param>
        /// <param name="Shift"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            var activeView = m_Application.ActiveView;

            if (Button == 4)
                return;
            IPoint p = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            double sDis = activeView.ScreenDisplay.DisplayTransformation.FromPoints(5);
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
                        activeView.Refresh();
                    }
                    catch (Exception es)
                    {
                        MessageBox.Show(es.Message);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 处理键盘指令
        /// </summary>
        /// <param name="keyCode">键盘标识码</param>
        /// <param name="Shift"></param>
        public override void OnKeyUp(int keyCode, int Shift)
        {
            var activeView = m_Application.ActiveView;

            switch (keyCode)
            {
                //case (int)System.Windows.Forms.Keys.Space:
                //    ProscessAll(true);
                //    activeView.Refresh();
                //    break;
                //case (int)System.Windows.Forms.Keys.Enter:
                //    ProscessAll(false);
                //    activeView.Refresh();
                //    break;
                case (int)System.Windows.Forms.Keys.R:
                    Analysis();
                    activeView.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.N:
                    Next();
                    break;
                case (int)System.Windows.Forms.Keys.D:
                    this.AutoProcess(currentErr, true);
                    activeView.Refresh();
                    break;
            }
        }

        /// <summary>
        /// 参数初始化
        /// </summary>
        public void Init()
        {
            curLayer = null;

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

            disValue = 1.0;
        }

        /// <summary>
        /// 分析问题函数
        /// </summary>
        void Analysis()
        {
            var activeView = m_Application.ActiveView;

            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("正在进行分析准备……");
            tin = new TinClass();
            tin.InitNew(activeView.FullExtent);
            tin.StartInMemoryEditing();

            int featureCount = (curLayer as IFeatureLayer).FeatureClass.FeatureCount(null);
            IFeatureCursor fCursor = (curLayer as IFeatureLayer).Search(null, true);
            IFeature feature = null;
            IPoint p = new PointClass();
            p.Z = 0;
            ITinNode node = new TinNodeClass();
            nID_fID_dic = new Dictionary<int, List<int>>();
            while ((feature = fCursor.NextFeature()) != null)
            {
                IPolyline line = feature.Shape as IPolyline;
                if (line.Length > 0)
                {
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

            }

            groups = new List<Dictionary<int, IPoint>>();
            for (int i = 1; i <= tin.NodeCount; i++)
            {
                wo.SetText("正在分析:" + i.ToString());

                ITinNode n = tin.GetNode(i);
                if (n.TagValue != -1 && n.IsInsideDataArea)
                {
                    Dictionary<int, IPoint> g = new Dictionary<int, IPoint>();
                    FindNode(n, g, disValue);
                    if (g.Count > 1)
                    {
                        groups.Add(g);
                    }
                }
            }
            gb = new GeometryBagClass();
            object miss = Type.Missing;

            wo.SetText("正在整理分析结果");

            foreach (Dictionary<int, IPoint> g in groups)
            {

                MultipointClass mp = new MultipointClass();

                foreach (int nid in g.Keys)
                {
                    IPoint pend = g[nid];
                    mp.AddGeometry(pend, ref miss, ref miss);
                }
                gb.AddGeometry(mp, ref miss, ref miss);
            }
            currentErr = groups.Count;
            wo.Dispose();
            System.Windows.Forms.MessageBox.Show("检查完成，共" + groups.Count + "个错误");
        }


        /// <summary>
        /// 递归查找三角网指定节点周边存在的所有问题，作为一个问题集返回
        /// </summary>
        /// <param name="seed">三角网节点</param>
        /// <param name="nodes">存储问题点集</param>
        /// <param name="distance">距离阈值</param>
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

        /// <summary>
        /// 自动单步处理问题
        /// </summary>
        /// <param name="index">问题索引</param>
        /// <param name="commit">是否忽略</param>
        void AutoProcess(int index, bool commit)
        {
            if (editor.EditState == esriEngineEditState.esriEngineStateNotEditing)
            {
                MessageBox.Show("请打开启编辑");
                return;
            }

            if (commit)
            {
                editor.StartOperation();

                Dictionary<int, IPoint> group = groups[index];
                IFeatureClass fc = ((curLayer) as IFeatureLayer).FeatureClass;
                IGeometry geo = gb.get_Geometry(index);
                IEnvelope env = geo.Envelope;
                IPoint p = (env as IArea).Centroid;

                foreach (var item in group.Keys)
                {
                    List<int> fids = nID_fID_dic[item];


                    List<int> deleteFIDs = new List<int>();
                    foreach (var fid in fids)
                    {
                        IFeature feature = fc.GetFeature(Math.Abs(fid));

                        if (fid > 0 && fids.Contains(-fid))
                        {
                            deleteFIDs.Add(fid);
                        }
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

                    foreach (var fffid in deleteFIDs)
                    {
                        IFeature feature = fc.GetFeature(fffid);
                        IPolyline pl = feature.Shape as IPolyline;
                        if (pl.Length < 2 * disValue)
                        {
                            feature.Delete();
                        }
                    }
                }
                editor.StopOperation("连通性处理");
            }
            groups.RemoveAt(index);
            gb.RemoveGeometries(index, 1);
        }

        /// <summary>
        /// 处理全部问题
        /// </summary>
        /// <param name="commit">是否忽略</param>
        void ProscessAll(bool commit)
        {
            using (WaitOperation wo = GApplication.Application.SetBusy())
            {
                try
                {
                    int count = gb.GeometryCount;
                    if (count==0)
                    {
                        return;
                    }
                    if (editor.EditState == esriEngineEditState.esriEngineStateNotEditing)
                    {
                        MessageBox.Show("请打开启编辑");
                        return;
                    }
                    for (int i = gb.GeometryCount - 1; i >= 0; i--)
                    {
                        wo.SetText("还有【" + (i + 1) + "】个对象处理完成");
                        AutoProcess(i, commit);
                    }
                    MessageBox.Show("处理完成！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 定位至下一个问题
        /// </summary>
        void Next()
        {
            var activeView = m_Application.ActiveView;

            if (groups == null || groups.Count == 0)
            {
                MessageBox.Show("处理完成！");
                return;
            }
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
            activeView.Extent = env;

            selectCurFeature(currentErr);

            activeView.Refresh();
        }

        //选中当前问题对应的要素
        void selectCurFeature(int index)
        {
            //清理所选要素
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            m_Application.MapControl.Map.ClearSelection();

            //
            IFeatureClass fc = ((curLayer) as IFeatureLayer).FeatureClass;
            Dictionary<int, IPoint> group = groups[index];
            foreach (var item in group.Keys)
            {
                List<int> fids = nID_fID_dic[item];
                foreach (var fid in fids)
                {
                    IFeature feature = fc.GetFeature(Math.Abs(fid));

                    //选中当前问题对应的要素
                    m_Application.MapControl.Map.SelectFeature(curLayer, feature);
                }
            }


            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            
        }
    }
}

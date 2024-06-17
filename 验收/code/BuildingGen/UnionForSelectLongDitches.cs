using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen
{
    public class UnionForSelectLongDitches : BaseGenTool
    {
        GLayerInfo waterLayer;      
        IFeatureClass lineFC;
        public UnionForSelectLongDitches()
        {
            base.m_category = "GWater";
            base.m_caption = "连接并删除短沟渠";
            base.m_message = "选取湖泊";
            base.m_toolTip = "连接并删除短沟渠,处理沟渠重叠的情况";
            base.m_name = "ditch";
            waterLayer = null;
            
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
            waterLayer = null;
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers)
            {
                if (info.LayerType == GCityLayerType.水系
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && info.OrgLayer != null
                    )
                {
                    waterLayer = info;
                    break;
                }
            }
            if (waterLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到水系图层");
                return;
            }
            if (System.Windows.Forms.MessageBox.Show("将进行连接并删除短沟渠,处理沟渠重叠操作，请确认", "提示", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;
            if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
            {
                IFeatureClass centralizedDitch2 = FeatWS.OpenFeatureClass("沟渠中心线");
                ditchClass2 = centralizedDitch2;
                int fieldIndex = ditchClass2.FindField("_GenUsed");
                genUsedID = fieldIndex;

                connectDitchLine(centralizedDitch2);
                unionTouchedDitchLines2(centralizedDitch2);
                deleteOverlapDitches(centralizedDitch2);
            }
 
            m_application.MapControl.Refresh();
        }
        IFeatureClass ditchClass2 = null;
        int genUsedID = -1;
      
        public override bool Deactivate()
        {
            //m_application.MapControl.OnAfterDraw -= new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            return true;
        }

        public override void OnKeyUp(int keyCode, int Shift)
        {

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
        void connectDitchLine(IFeatureClass DitchLineFeats)
        {
            try
            {
                double dis = 30;//注意：小于8的原线段变为点
                WaitOperation wo = m_application.SetBusy(true);
                wo.SetText("正在进行分析准备……");
                tin = new TinClass();
                tin.InitNew(m_application.MapControl.ActiveView.FullExtent);
                tin.StartInMemoryEditing();

                IQueryFilter qf = new QueryFilterClass();
                //qf.WhereClause = "_GenUsed=-2" + " " + "OR" + " " + "_GenUsed=0";

                int featureCount = DitchLineFeats.FeatureCount(qf);
                IFeatureCursor fCursor = DitchLineFeats.Search(qf, true);
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
                    foreach (int nid in g.Keys)
                    {
                        IPoint pend = g[nid];
                        mp.AddGeometry(pend, ref miss, ref miss);
                    }
                    gb.AddGeometry(mp, ref miss, ref miss);
                }
                m_application.SetBusy(false);

                //ID_groupIDs.Clear();
                ProscessAll(true, DitchLineFeats);
                m_application.MapControl.Refresh();
            }
            catch (Exception ex)
            {
            }
        }

        //void connectDitchLine2(IFeatureClass DitchLineFeats)
        //{
        //    IFeatureClass waterClass = (waterLayer.Layer as IFeatureLayer).FeatureClass;

        //    try
        //    {
        //        double dis = 15;//注意：小于8的原线段变为点
        //        WaitOperation wo = m_application.SetBusy(true);
        //        wo.SetText("正在进行分析准备……");
        //        tin = new TinClass();
        //        tin.InitNew(m_application.MapControl.ActiveView.FullExtent);
        //        tin.StartInMemoryEditing();

        //        int featureCount = DitchLineFeats.FeatureCount(null);
        //        IFeatureCursor fCursor = DitchLineFeats.Search(null, true);
        //        IFeature feature = null;
        //        IPoint p = new PointClass();
        //        p.Z = 0;
        //        ITinNode node = new TinNodeClass();
        //        nID_fID_dic = new Dictionary<int, List<int>>();
        //        while ((feature = fCursor.NextFeature()) != null)
        //        {
        //            wo.SetText("正在进行分析准备:" + feature.OID.ToString());
        //            wo.Step(featureCount);
        //            IPolyline line = feature.Shape as IPolyline;
        //            p.X = line.FromPoint.X;
        //            p.Y = line.FromPoint.Y;
        //            tin.AddPointZ(p, 1, node);
        //            if (!nID_fID_dic.ContainsKey(node.Index))
        //            {
        //                nID_fID_dic[node.Index] = new List<int>();
        //            }
        //            nID_fID_dic[node.Index].Add(feature.OID);
        //            p.X = line.ToPoint.X;
        //            p.Y = line.ToPoint.Y;
        //            tin.AddPointZ(p, 1, node);
        //            if (!nID_fID_dic.ContainsKey(node.Index))
        //            {
        //                nID_fID_dic[node.Index] = new List<int>();
        //            }
        //            nID_fID_dic[node.Index].Add(-feature.OID);
        //        }

        //        int featureCount = waterClass.FeatureCount(null);
        //        IFeatureCursor fCursor = waterClass.Search(null, false);
        //        IFeature feature = null;
        //        object z = 0;
        //        while ((feature = fCursor.NextFeature()) != null)
        //        {
        //            w.SetText("分析准备2:" + feature.OID.ToString());
        //            w.Step(featureCount);
        //            IPolygon line = feature.ShapeCopy as IPolygon;
        //            ITopologicalOperator lineto = line as ITopologicalOperator;
        //            lineto.Simplify();
        //            line.Densify(8, 0);
        //            IPointCollection pc = line as IPointCollection;
        //            for (int i = 0; i < pc.PointCount; i++)
        //            {
        //                tin.AddPointZ(pc.get_Point(i), -feature.OID - 3, node);
        //                if (!nID_fID_dic.ContainsKey(node.Index))
        //                {
        //                    nID_fID_dic[node.Index] = new List<int>();
        //                }
        //                nID_fID_dic[node.Index].Add(-feature.OID - 3);
        //            }
        //        }

        //        wo.Step(0);
        //        groups = new List<Dictionary<int, IPoint>>();
        //        for (int i = 1; i <= tin.NodeCount; i++)
        //        {
        //            wo.SetText("正在分析:" + i.ToString());
        //            wo.Step(tin.NodeCount);

        //            ITinNode n = tin.GetNode(i);
        //            if (n.TagValue != -1 && n.IsInsideDataArea)
        //            {
        //                Dictionary<int, IPoint> g = new Dictionary<int, IPoint>();
        //                FindNode2(n, g, dis);
        //                if (g.Count > 1)
        //                {
        //                    groups.Add(g);
        //                }
        //            }
        //        }
        //        gb = new GeometryBagClass();
        //        object miss = Type.Missing;
        //        wo.Step(0);
        //        wo.SetText("正在整理分析结果");

        //        foreach (Dictionary<int, IPoint> g in groups)
        //        {
        //            wo.Step(groups.Count);
        //            MultipointClass mp = new MultipointClass();
        //            foreach (int nid in g.Keys)
        //            {
        //                IPoint pend = g[nid];
        //                mp.AddGeometry(pend, ref miss, ref miss);
        //            }
        //            gb.AddGeometry(mp, ref miss, ref miss);
        //        }
        //        m_application.SetBusy(false);

        //        ProscessAll2(true, DitchLineFeats);
        //        m_application.MapControl.Refresh();
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        void ProscessAll(bool commit, IFeatureClass DitchClass)
        {
            WaitOperation wo = m_application.SetBusy(true);
            try
            {
                int count = gb.GeometryCount;
                for (int i = gb.GeometryCount - 1; i >= 0; i--)
                {
                    AutoProcess(i, commit, DitchClass);
                    wo.Step(count);
                }
            }
            catch
            {
            }
            m_application.SetBusy(false);
        }

        //Dictionary<int, List<int>> ID_groupIDs = new Dictionary<int, List<int>>();//每个id的未处理组
        void AutoProcess(int index, bool commit, IFeatureClass DitchClass)
        {
            if (commit)
            {
                Dictionary<int, IPoint> group = groups[index];
                IFeatureClass fc = DitchClass;
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
                        line.Generalize(1);
                        if (fid > 0)
                        {
                            line.FromPoint = p;
                            //IPolyline newLine = new PolylineClass();
                            //IPoint po = line.FromPoint;
                            //(newLine as IPointCollection).AddPoints(1, ref po);
                            //(newLine as IPointCollection).AddPoints(1, ref p);
                            //ITopologicalOperator lineTo = line as ITopologicalOperator;
                            //lineTo.Simplify();
                            //(newLine as ITopologicalOperator).Simplify();
                            //line = lineTo.Union(newLine) as IPolyline;
                        }
                        else
                        {
                            line.ToPoint = p;
                            //IPolyline newLine = new PolylineClass();
                            //IPoint po = line.ToPoint;
                            //(newLine as IPointCollection).AddPoints(1, ref po);
                            //(newLine as IPointCollection).AddPoints(1, ref p);
                            //ITopologicalOperator lineTo = line as ITopologicalOperator;
                            //lineTo.Simplify();
                            //(newLine as ITopologicalOperator).Simplify();
                            //line = lineTo.Union(newLine) as IPolyline;
                        }
                        feature.Shape = line;
                        feature.Store();

                    }
                }
            }
            groups.RemoveAt(index);
            gb.RemoveGeometries(index, 1);
        }

        void unionTouchedDitchLines2(IFeatureClass ditchClass)
        {
            try
            {
                IFeatureCursor insert = ditchClass.Insert(false);
                IQueryFilter qf2 = new QueryFilterClass();
                //qf2.WhereClause = "_GenUsed=-2" + " " + "OR" + " " + "_GenUsed=0";
                IFeatureCursor fC = ditchClass.Update(qf2, false);
                IFeature ff = null;
                ISpatialFilter sff = new SpatialFilterClass();
                //sff.WhereClause = "_GenUsed=-2" + " " + "OR" + " " + "_GenUsed=0";
                sff.SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches;
                WaitOperation w = m_application.SetBusy(true);
                int c = ditchClass.FeatureCount(qf2);
                while ((ff = fC.NextFeature()) != null)
                {
                    w.SetText("正在融合沟渠线..." + ff.OID);
                    w.Step(c);
                    IPolyline pp = ff.ShapeCopy as IPolyline;
                    if (pp.Length == 0)
                    {
                        ff.Delete();
                        continue;
                    }
                    ITopologicalOperator ppt = pp as ITopologicalOperator;
                    ppt.Simplify();
                    double oriAngle = (pp.FromPoint.Y - pp.ToPoint.Y) / (pp.FromPoint.X - pp.ToPoint.X);
                    sff.Geometry = pp;
                    IFeatureCursor resultCur = ditchClass.Search(sff, false);
                    IFeature touchFeat = null;
                    //IPolyline unionedLine = new PolylineClass();
                    bool isUnioned = false;
                    while ((touchFeat = resultCur.NextFeature()) != null)
                    {
                        IPolyline tt = touchFeat.ShapeCopy as IPolyline;
                        double targetAngle = (tt.FromPoint.Y - tt.ToPoint.Y) / (tt.FromPoint.X - tt.ToPoint.X);
                        double difference = Math.Abs(oriAngle - targetAngle);
                        if (difference > 0.7)
                        {
                            continue;
                        }
                        ITopologicalOperator ttp = tt as ITopologicalOperator;
                        ttp.Simplify();
                        ppt = ppt.Union(ttp as IGeometry) as ITopologicalOperator;
                        ppt.Simplify();
                        isUnioned = true;
                        touchFeat.Delete();
                    }
                    if (isUnioned)
                    {
                        IPolyline pptl = ppt as IPolyline;
                        if (pptl.Length > 0)//150
                        {
                            IFeatureBuffer unionedBuffer = ff as IFeatureBuffer;
                            unionedBuffer.Shape = ppt as IGeometry;
                            insert.InsertFeature(unionedBuffer);
                        }
                        ff.Delete();
                    }
                    else
                    {
                        if (pp.Length < 0)//150
                        {
                            ff.Delete();
                        }
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(resultCur);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fC);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(insert);
                m_application.SetBusy(false);
            }
            catch (Exception exx)
            {
                m_application.SetBusy(false);
            }
        }

        void deleteOverlapDitches(IFeatureClass ditchClass)
        {
            try
            {
                ISpatialFilter sf = new SpatialFilterClass();
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;
                IFeatureCursor fcur = null;
                IFeature feat = null;
                fcur = ditchClass.Update(null, false);
                WaitOperation w = m_application.SetBusy(true);
                int oo = ditchClass.FeatureCount(null);
                while ((feat = fcur.NextFeature()) != null)
                {
                    w.SetText("删除重叠的沟渠...");
                    w.Step(oo);
                    sf.Geometry = feat.ShapeCopy;
                    if ((feat.Shape as IPolyline).Length == 0)
                    {
                        feat.set_Value(genUsedID,-1);
                        feat.Store();
                        feat.Delete();//
                        continue;
                    }
                    IFeatureCursor fcur2 = ditchClass.Search(sf, false);
                    IFeature feat2 = null;
                    bool isOverlap = false;
                    while ((feat2 = fcur2.NextFeature()) != null)
                    {
                        if (feat.OID == feat2.OID)
                        {
                            continue;
                        }
                        isOverlap = true;
                    }
                    if (isOverlap)
                    {
                        feat.set_Value(genUsedID, -9);
                        feat.Store();
                        feat.Delete();//
                    }
                    fcur2.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur2);
                }
                fcur.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur);

                m_application.SetBusy(false);
            }
            catch(Exception ex)
            {
                m_application.SetBusy(false);
            }
        }

        void pointsAttributeTransfer()
        {
            try
            {
                IFeatureClass fromClass = null;
                IFeatureClass toClass = null;
                IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
                IWorkspace2 WS2 = FeatWS as IWorkspace2;
                if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "镇点"))
                {
                    fromClass = FeatWS.OpenFeatureClass("镇点");
                }
                if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "镇注记"))
                {
                    toClass = FeatWS.OpenFeatureClass("镇注记");
                }
                int index = toClass.FindField("待确认");
                if (index == -1)
                {
                    IFieldEdit2 field = new FieldClass();
                    field.Name_2 = "待确认";
                    field.Type_2 = esriFieldType.esriFieldTypeInteger;
                    field.DefaultValue_2 = 0;
                    toClass.AddField(field);
                    index = toClass.FindField("待确认");
                }
                IFeatureCursor insertCur = toClass.Insert(false);
                IFeatureCursor fcur1 = null;
                fcur1 = fromClass.Search(null, false);
                IFeature feat = null;
                Dictionary<int, bool> isConvertedCopy = new Dictionary<int, bool>();
                Dictionary<int, int> isAgainFound = new Dictionary<int, int>();
                WaitOperation w = m_application.SetBusy(true);
                int c = fromClass.FeatureCount(null);
                while ((feat = fcur1.NextFeature()) != null)
                {
                    w.Step(c);
                    IPoint p = feat.ShapeCopy as IPoint;
                    IPolygon poly = (p as ITopologicalOperator).Buffer(1500) as IPolygon;
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                    sf.Geometry = poly;
                    IFeatureCursor fcur2 = null;
                    fcur2 = toClass.Search(sf, false);
                    IFeature bestFeat = null;
                    double minDis = 1000000000000;
                    IFeature feat2 = null;
                    while ((feat2 = fcur2.NextFeature()) != null)
                    {
                        if (isConvertedCopy.ContainsKey(feat2.OID))
                        {
                            continue;
                        }
                        IPoint p2 = feat2.ShapeCopy as IPoint;
                        double tpDis = (p2 as IProximityOperator).ReturnDistance(p);
                        if (tpDis < minDis)
                        {
                            minDis = tpDis;
                            bestFeat = feat2;
                        }
                    }
                    IFeatureBuffer fb = bestFeat as IFeatureBuffer;
                    if (bestFeat == null)
                    {
                        fb = toClass.CreateFeatureBuffer();

                        //continue;
                    }
                    int idIndex = Convert.ToInt16(insertCur.InsertFeature(fb));

                    IFeature toInsertFeat = toClass.GetFeature(idIndex);
                    toInsertFeat.Shape = feat.ShapeCopy;
                    toInsertFeat.Store();
                    if (bestFeat == null)
                    {
                        toInsertFeat.set_Value(index, -1);
                        toInsertFeat.Store();
                    }
                    else
                    {
                        if (isAgainFound.ContainsKey(bestFeat.OID))
                        {
                            IFeature tp = toClass.GetFeature(isAgainFound[bestFeat.OID]);
                            tp.set_Value(index, -1);
                            toInsertFeat.set_Value(index, -1);
                            toInsertFeat.Store();
                            isAgainFound.Remove(bestFeat.OID);
                        }
                        isAgainFound.Add(bestFeat.OID, idIndex);
                    }
                    isConvertedCopy.Add(idIndex, true);

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur2);
                }
                IFeatureCursor deleteCur = toClass.Search(null, false);
                IFeature deleteFeat = null;
                while ((deleteFeat = deleteCur.NextFeature()) != null)
                {
                    if (!isConvertedCopy.ContainsKey(deleteFeat.OID))
                    {
                        deleteFeat.Delete();
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteCur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur1);
                m_application.SetBusy(false);
            }
            catch (Exception ex)
            {
            }
        }

    }
}

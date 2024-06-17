using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
using System.Collections;
using System.Windows.Forms;

namespace BuildingGen {
    public class BuildingD : BaseGenTool {
        INewPolygonFeedback fb;
        GLayerInfo buildingLayer;
        GLayerInfo roadLayer;
        int fieldID;
        int maxValue;
        IDataStatistics statistics;
        IPolygon lastPolygon;
        bool needDrawLastPolygon = false;
        public BuildingD() {
            base.m_category = "GBuilding";
            base.m_caption = "自动分组";
            base.m_message = "建筑物自动分组";
            base.m_toolTip = "绘制多边形选择建筑物区域，\n由计算机对建筑物自动分组。\n按L显示最后操作的区域。";
            base.m_name = "BuildingD";
            base.m_usedParas = new GenDefaultPara[] 
            {
                new GenDefaultPara("建筑物自动分组面积",(double)400),
                new GenDefaultPara("建筑物自动分组距离",(double)20)
            };
            statistics = new DataStatisticsClass();
        }
        public override void OnKeyDown(int keyCode, int Shift) {
            switch (keyCode) {
            case (int)System.Windows.Forms.Keys.Escape:
                if (fb != null) {
                    fb.Stop();
                    fb = null;
                }
                break;
            case (int)System.Windows.Forms.Keys.L:
                needDrawLastPolygon = !needDrawLastPolygon;
                break;
            }
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }
        public override void Refresh(int hDC) {
            base.Refresh(hDC);
            if (fb != null)
                fb.Refresh(hDC);
        }
        bool registDraw = false;
        public override void OnClick() {
            if (!registDraw) {
                m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
                registDraw = true;
            }
            buildingLayer = GetBuildingLayer(m_application.Workspace);
            roadLayer = GetRoadLayer(m_application.Workspace);
            fieldID = CheckField(buildingLayer);
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "G_BuildingGroup >0";
            statistics.Cursor = (buildingLayer.Layer as IFeatureLayer).Search(qf, true) as ICursor;
            statistics.Field = "G_BuildingGroup";

            try {
                maxValue = Convert.ToInt32(statistics.Statistics.Maximum);
            }
            catch {
                maxValue = 0;
            }
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            if(needDrawLastPolygon && (lastPolygon != null))
            {
                SimpleFillSymbolClass sfs = new SimpleFillSymbolClass();
                sfs.Style = esriSimpleFillStyle.esriSFSNull;
                SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
                RgbColorClass rgb = new RgbColorClass();
                rgb.Red = 255;
                rgb.Green = 0;
                rgb.Blue = 0;
                sls.Color = rgb;
                sls.Width = 2;
                sfs.Outline = sls;
                IDisplay dis = e.display as IDisplay;
                dis.SetSymbol(sfs);
                dis.DrawPolygon(lastPolygon);
            }
        }

        private void Gen(IPolygon range) {
            IFeatureLayer layer = buildingLayer.Layer as IFeatureLayer;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int allCount = layer.FeatureClass.FeatureCount(sf);
            WaitOperation wo = null;
            if (allCount > 500) {
                wo = m_application.SetBusy(true);
            }
            IFeatureCursor fCursor = layer.FeatureClass.Search(sf, true);
            IFeature feature = null;

            object z = 0;
            object miss = Type.Missing;
            PolygonClass poly = new PolygonClass();

            while ((feature = fCursor.NextFeature()) != null) {
                IGeometryCollection gc = feature.Shape as IGeometryCollection;
                for (int i = 0; i < gc.GeometryCount; i++) {
                    poly.AddGeometry(gc.get_Geometry(i), ref miss, ref miss);
                }
            }
            poly.Simplify();

            IGeometryCollection gb = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;

            Dictionary<int, IPolygon> group_group_dic = new Dictionary<int, IPolygon>();
            for (int i = 0; i < gb.GeometryCount; i++) {
                group_group_dic.Add(i, gb.get_Geometry(i) as IPolygon);
            }
            List<GroupInfo> group_group_list = LitterGroup(group_group_dic);
            foreach (var groupInfo in group_group_list) {
                sf.Geometry = groupInfo.Shape;

                IFeatureCursor gfc = layer.FeatureClass.Search(sf, false);
                Dictionary<int, IPolygon> dic = new Dictionary<int, IPolygon>();
                Dictionary<int, IFeature> fid_feature_dic = new Dictionary<int, IFeature>();

                while ((feature = gfc.NextFeature()) != null) {
                    fid_feature_dic.Add(feature.OID, feature);

                    dic.Add(feature.OID, feature.ShapeCopy as IPolygon);
                }
                try {
                    List<GroupInfo> groupList = LitterGroup(dic);
                    foreach (var item in groupList) {
                        maxValue++;
                        foreach (var fid in item.dic.Keys) {
                            IFeature fe = fid_feature_dic[fid];
                            fe.set_Value(fieldID, maxValue);
                            fe.Store();
                        }
                    }
                }
                catch (Exception ex) {
                }
            }
            if (wo != null) {
                m_application.SetBusy(false);
            }
        }


        private void Gen2(IPolygon range) {
            IFeatureLayer layer = buildingLayer.Layer as IFeatureLayer;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int allCount = layer.FeatureClass.FeatureCount(sf);
            WaitOperation wo = null;
            if (allCount > 50) {
                wo = m_application.SetBusy(true);
            }
            Action<string> aSetText = (text) => { if (wo != null) wo.SetText(text); };
            Action<int> aStep = (s) => { if (wo != null) wo.Step(s); };
                        
            IFeatureCursor fCursor = layer.FeatureClass.Search(sf, true);
            IFeature feature = null;

            IFeatureLayer rLayer = null;
            IFeatureCursor roadCursor = null;

            if (roadLayer != null) {
                rLayer = roadLayer.Layer as IFeatureLayer;
                roadCursor = rLayer.FeatureClass.Search(sf, true);
                allCount += rLayer.FeatureClass.FeatureCount(sf);
            }            

            object z = 0;
            object miss = Type.Missing;
            double paraDistance = (double)m_application.GenPara["建筑物自动分组距离"];
            ISpatialReferenceFactory srf_ = new SpatialReferenceEnvironmentClass();
            IProjectedCoordinateSystem pcs_ = srf_.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_CM_114E);
            
            try {
                // 1建网
                aSetText("正在进行分析准备");
                TinClass tin = new TinClass();
                tin.InitNew(m_application.MapControl.FullExtent);
                tin.StartInMemoryEditing();
                Dictionary<int, FeatureNode> nodes = new Dictionary<int, FeatureNode>();
                List<FeatureNode> nodeList = new List<FeatureNode>();
                while ((feature = fCursor.NextFeature()) != null) {
                    aStep(allCount);
                    IGeometry geoCopy = feature.ShapeCopy;
                    geoCopy.SpatialReference = pcs_;
                    IPolygon geo = (geoCopy as ITopologicalOperator).Buffer(-0.1) as IPolygon;
                    geo.Generalize(0.1);
                    geo.Densify(paraDistance / 5, 0);
                    FeatureNode n = new FeatureNode(feature.OID, (feature.Shape as IArea).Area);
                    nodes.Add(feature.OID, n);
                    nodeList.Add(n);
                    for (int i = 0; i < (geo as IPointCollection).PointCount; i++) {
                        IPoint p = (geo as IPointCollection).get_Point(i);
                        p.Z = 0;
                        tin.AddPointZ(p, feature.OID);
                    }
                }
                if (roadCursor != null) {
                    while ((feature = roadCursor.NextFeature()) != null) {
                        aStep(allCount);
                        IGeometry geoCopy = feature.ShapeCopy;
                        geoCopy.SpatialReference = pcs_;
                        IPolyline geo = geoCopy as IPolyline;
                        geo.Densify(paraDistance / 5, 0);
                        //FeatureNode n = new FeatureNode(feature.OID, (feature.Shape as IArea).Area);
                        //nodes.Add(feature.OID, n);
                        //nodeList.Add(n);
                        for (int i = 0; i < (geo as IPointCollection).PointCount; i++) {
                            IPoint p = (geo as IPointCollection).get_Point(i);
                            p.Z = 0;
                            tin.AddPointZ(p, -1);
                        }

                    }
                }
                //const int kCount = 20;
                //int[] disCount = new int[kCount];

                //2 .找到要素之间的临近关系图
                aSetText("正在分析数据");
                for (int i = 1; i <= tin.TriangleCount; i++) {
                    aStep(tin.TriangleCount);
                    ITinTriangle triangle = tin.GetTriangle(i);
                    if (!triangle.IsInsideDataArea) {
                        continue;
                    }

                    ITinEdge edge = triangle.get_Edge(0);
                    int tag1 = edge.FromNode.TagValue;
                    int tag2 = edge.ToNode.TagValue;
                    int tag3 = edge.GetNextInTriangle().ToNode.TagValue;

                    if (tag1 == -1 || tag2 == -1 || tag3 == -1) {
                        continue;
                    }//有一个顶点在路上
                    if (tag1 == tag2 && tag2 == tag3) {
                        continue;
                    }//同一个要素上                
                    else if (tag1 != tag2 && tag1 != tag3 && tag2 != tag3) {
                        continue;
                    }//三个不同要素上
                    else {
                        double wOnFeature = 0;
                        double disBeFeature = 0;

                        if (tag3 == tag2) {
                            edge = edge.GetNextInTriangle();
                        }
                        else if (tag1 == tag3) {
                            edge = edge.GetPreviousInTriangle();
                        }
                        wOnFeature = edge.Length;
                        PointClass centerPoint = new PointClass();
                        centerPoint.X = (edge.FromNode.X + edge.ToNode.X) / 2;
                        centerPoint.Y = (edge.FromNode.Y + edge.ToNode.Y) / 2;
                        IPoint toPoint = new PointClass();
                        edge.GetNextInTriangle().ToNode.QueryAsPoint(toPoint);
                        disBeFeature = centerPoint.ReturnDistance(toPoint);
                        //disBeFeature = (edge.GetNextInTriangle().Length + edge.GetPreviousInTriangle().Length) / 2;
                        double ra = disBeFeature / paraDistance;
                        double rate = (1 - ra);
                        if (rate <= 0)
                            continue;
                        //disCount[(int)(ra * kCount) - 1]++;
                        double v = wOnFeature * (0.3 * rate + 0.7);

                        FeatureNode fNode = nodes[edge.GetNextInTriangle().ToNode.TagValue];
                        FeatureNode tNode = nodes[edge.FromNode.TagValue];
                        FeatureEdge feaEdge = null;
                        if (fNode.Edges.ContainsKey(tNode)) {
                            feaEdge = fNode.Edges[tNode];
                        }
                        else {
                            feaEdge = new FeatureEdge(fNode, tNode);
                        }
                        feaEdge.Value += v;
                    }
                }

                // 3 分组   
                // 3.1 排序 

                //nodeList.Sort((n1, n2) => { return n1.Area < n2.Area ? -1 : 1; });
                aSetText("正在自动分组");
                nodeList.Sort();
                LinkedList<FeatureNode> linkNodes = new LinkedList<FeatureNode>(nodeList);

                List<FeatureNode> results = new List<FeatureNode>();
                while (linkNodes.First != null) {
                    if (AreaEnough(linkNodes.First.Value.Area)) {
                        results.AddRange(linkNodes);
                        break;
                    }
                    LinkedListNode<FeatureNode> linkNode = linkNodes.First;
                    FeatureNode best = null;
                    double max = double.MinValue;
                    double baseArea = linkNode.Value.Area;
                    foreach (var item in linkNode.Value.Edges.Keys) {
                        double v = linkNode.Value.Edges[item].Value * (0.2 + 0.8 * baseArea / (item.Area + baseArea));
                        if (v > max) {
                            best = item;
                            max = v;
                        }
                    }
                    if (best == null) {
                        results.Add(linkNode.Value);
                        linkNodes.Remove(linkNodes.First);
                    }
                    else {
                        best.Merge(linkNodes.First.Value);
                        linkNodes.Remove(linkNodes.First);
                        linkNode = linkNodes.Find(best);
                        LinkedListNode<FeatureNode> next = linkNode.Next;
                        linkNodes.Remove(linkNode);
                        while (next != null) {
                            if (next.Value.Area > best.Area) {
                                linkNodes.AddBefore(next, best);
                                break;
                            }
                            else {
                                next = next.Next;
                            }
                        }
                        if (next == null) {
                            if (linkNodes.Count > 0)
                                linkNodes.AddLast(best);
                            else
                                results.Add(best);
                        }
                    }
                }


                //4 写结果
                aSetText("正在保存分组结果");
                foreach (var item in results) {
                    aStep(results.Count);
                    maxValue++;
                    foreach (var oid in item.OIDs) {
                        IFeature fe = layer.FeatureClass.GetFeature(oid);
                        fe.set_Value(fieldID, maxValue);
                        fe.Store();
                    }
                }
            }
            catch {
            }
            if (wo != null) {
                m_application.SetBusy(false);
            }
        }
        internal class FeatureNode : IComparable {
            internal List<int> OIDs;
            internal double Area;
            internal Dictionary<FeatureNode, FeatureEdge> Edges;
            internal FeatureNode(int oid, double area) {
                Edges = new Dictionary<FeatureNode, FeatureEdge>();
                this.OIDs = new List<int>();
                OIDs.Add(oid);
                this.Area = area;
            }

            internal void Merge(FeatureNode other) {
                OIDs.AddRange(other.OIDs);
                other.OIDs.Clear();

                Area += other.Area;
                other.Area = 0;



                foreach (var item in other.Edges.Keys) {
                    if (item == this) {
                        this.Edges.Remove(other);
                        continue;
                    }
                    //double value = other.Edges[item].Value;
                    if (this.Edges.ContainsKey(item)) {
                        this.Edges[item].Value += other.Edges[item].Value;
                        item.Edges.Remove(other);
                    }
                    else {
                        this.Edges.Add(item, other.Edges[item]);
                        item.Edges.Remove(other);
                        item.Edges.Add(this, other.Edges[item]);
                    }
                }
                other.Edges.Clear();

            }

            #region IComparable 成员

            public int CompareTo(object obj) {
                return this.Area < (obj as FeatureNode).Area ? -1 : 1;
            }

            #endregion

        }
        internal class FeatureEdge {
            internal FeatureNode FromNode;
            internal FeatureNode ToNode;
            internal double Value;
            internal FeatureEdge(FeatureNode from, FeatureNode to) {
                this.FromNode = from;
                FromNode.Edges.Add(to, this);
                this.ToNode = to;
                ToNode.Edges.Add(from, this);
                Value = 0;
            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button != 1)
                return;
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb == null) {
                fb = new NewPolygonFeedbackClass();
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                fb.Start(p);
            }
            else {
                fb.AddPoint(p);
            }
        }
        public override void OnMouseMove(int Button, int Shift, int X, int Y) {
            if (fb != null) {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }
        public override void OnDblClick() {
            if (fb != null) {
                IPolygon poly = fb.Stop();
                fb = null;
                if (poly == null || poly.IsEmpty) {
                    return;
                }
                lastPolygon = poly;
                Gen2(poly);
                m_application.MapControl.Refresh();
            }
        }

        private GLayerInfo GetBuildingLayer(GWorkspace workspace) {
            foreach (GLayerInfo info in workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.建筑物
                    //&& (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    ) {
                    return info;
                }
            }
            return null;
        }
        private GLayerInfo GetRoadLayer(GWorkspace workspace) {
            foreach (GLayerInfo info in workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.道路
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    ) {
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// 检查字段是否存在，不存在则添加，最后返回字段索引
        /// 字段名：G_BuildingGroup
        /// 字段类型：int
        /// 字段含义：-1对应综合后的要素，0对应未分组要素，大于0为各分组
        /// </summary>
        /// <param name="info">图层</param>
        /// <returns>字段索引</returns>
        private int CheckField(GLayerInfo info) {
            IFeatureClass fc = (info.Layer as IFeatureLayer).FeatureClass;
            int index = fc.FindField("G_BuildingGroup");
            if (index == -1) {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "G_BuildingGroup";
                field.Type_2 = esriFieldType.esriFieldTypeInteger;
                field.DefaultValue_2 = 0;
                fc.AddField(field);
                index = fc.FindField("G_BuildingGroup");
                ClearGroup(fc, index);
            }
            (info.Layer as IGeoFeatureLayer).DisplayAnnotation = false;
            ESRI.ArcGIS.Carto.IAnnotateLayerPropertiesCollection alp = (info.Layer as IGeoFeatureLayer).AnnotationProperties;
            alp.Clear();
            ESRI.ArcGIS.Carto.ILabelEngineLayerProperties lelp = new ESRI.ArcGIS.Carto.LabelEngineLayerPropertiesClass();
            lelp.Expression = "[G_BuildingGroup]";
            lelp.BasicOverposterLayerProperties.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
            alp.Add(lelp as ESRI.ArcGIS.Carto.IAnnotateLayerProperties);
            //(info.Layer as IGeoFeatureLayer).DisplayField = "G_BuildingGroup";
            return index;
        }
        private void ClearGroup(IFeatureClass fc, int index) {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "G_BuildingGroup !=-1";
            IFeatureCursor cursor = fc.Update(null, true);
            IFeature f = null;
            while ((f = cursor.NextFeature()) != null) {
                object v = f.get_Value(index);
                //try
                //{
                //    if (Convert.ToInt32(v) == -1)
                //        continue;
                //}
                //catch
                //{
                //}
                f.set_Value(index, 0);
                cursor.UpdateFeature(f);
            }
            cursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }


        private List<GroupInfo> LitterGroup(Dictionary<int, IPolygon> polygons) {
            List<GroupInfo> infos = new List<GroupInfo>();
            Dictionary<int, GroupInfo> fid_group_dic = new Dictionary<int, GroupInfo>();

            foreach (var item in polygons.Keys) {
                GroupInfo g = new GroupInfo(item, polygons[item]);
                infos.Add(g);
                fid_group_dic.Add(item, g);
            }

            if (infos.Count <= 1) {
                return infos;
            }

            TinClass tin;
            PolylineClass tinEdge;
            tin = new TinClass();
            tin.InitNew(m_application.MapControl.FullExtent);
            tin.StartInMemoryEditing();
            foreach (int item in polygons.Keys) {
                IPoint p = (polygons[item] as IArea).Centroid;
                p.Z = 0;
                tin.AddPointZ(p, item);
            }
            tinEdge = new PolylineClass();
            object miss = Type.Missing;

            List<ITinEdge> edges = new List<ITinEdge>();
            Dictionary<int, double> distanceOfPolygon = new Dictionary<int, double>();
            //List<double> distanceOfPolygon = new List<double>();
            for (int i = 1; i <= tin.EdgeCount; i++) {
                ITinEdge edge = tin.GetEdge(i);
                if (!edge.IsInsideDataArea)
                    continue;

                IPolygon p1 = polygons[edge.FromNode.TagValue];
                IPolygon p2 = polygons[edge.ToNode.TagValue];
                //edge.TagValue = edges.Count;
                double dis = (p1 as IProximityOperator).ReturnDistance(p2);
                distanceOfPolygon.Add(edge.Index, dis);
                edges.Add(edge);
            }

            edges.Sort((e1, e2) => { return (distanceOfPolygon[e1.Index] < distanceOfPolygon[e2.Index]) ? -1 : 1; });


            List<ITinEdge> other_edges = new List<ITinEdge>();

            foreach (ITinEdge item in edges) {
                //长度大于 建筑物分组距离 
                if (LenghtInLimit(distanceOfPolygon[item.Index]) != -1) {
                    //长度大于 2倍 建筑物分组距离 （放到其他类别中）
                    if (LenghtInLimit(distanceOfPolygon[item.Index]) != 0) {
                        other_edges.Add(item);
                        continue;
                    }
                    //否则终止（由于已经排序，剩下的都会大于2倍建筑物分组距离）
                    else {
                        break;
                    }
                }
                ITinNode node1 = item.FromNode;
                ITinNode node2 = item.ToNode;
                GroupInfo info1 = null;
                GroupInfo info2 = null;

                #region 找有没有已经被分组
                if (fid_group_dic.ContainsKey(node1.TagValue)) {
                    info1 = fid_group_dic[node1.TagValue];
                }
                else {
                    info1 = new GroupInfo(node1.TagValue, polygons[node1.TagValue]);
                    fid_group_dic.Add(node1.TagValue, info1);
                    infos.Add(info1);
                }
                if (fid_group_dic.ContainsKey(node2.TagValue)) {
                    info2 = fid_group_dic[node2.TagValue];
                }
                else {
                    info2 = new GroupInfo(node2.TagValue, polygons[node2.TagValue]);
                    fid_group_dic.Add(node2.TagValue, info2);
                    infos.Add(info2);
                }
                #endregion

                if (info1 == info2)
                    continue;
                if ((info1.CenterPoint as IProximityOperator).ReturnDistance(info2.CenterPoint) > (double)m_application.GenPara["建筑物自动分组距离"]) {
                    other_edges.Add(item);
                }
                if (!AreaEnough(info1.Area) && !AreaEnough(info2.Area)) {
                    info1.Merge(info2);
                    foreach (int fid in info2.dic.Keys) {
                        fid_group_dic[fid] = info1;
                    }
                    infos.Remove(info2);
                }
                if (!AreaEnough(info1.Area) || !AreaEnough(info2.Area)) {
                    other_edges.Add(item);
                }
            }

            foreach (var item in other_edges) {
                ITinNode node1 = item.FromNode;
                ITinNode node2 = item.ToNode;
                GroupInfo info1 = null;
                GroupInfo info2 = null;
                #region 找有没有已经被分组
                if (fid_group_dic.ContainsKey(node1.TagValue)) {
                    info1 = fid_group_dic[node1.TagValue];
                }
                else {
                    info1 = new GroupInfo(node1.TagValue, polygons[node1.TagValue]);
                    fid_group_dic.Add(node1.TagValue, info1);
                    infos.Add(info1);
                }
                if (fid_group_dic.ContainsKey(node2.TagValue)) {
                    info2 = fid_group_dic[node2.TagValue];
                }
                else {
                    info2 = new GroupInfo(node2.TagValue, polygons[node2.TagValue]);
                    fid_group_dic.Add(node2.TagValue, info2);
                    infos.Add(info2);
                }
                #endregion
                if (info1 == info2)
                    continue;
                if (!AreaEnough(info1.Area) || !AreaEnough(info2.Area)) {
                    info1.Merge(info2);
                    foreach (int fid in info2.dic.Keys) {
                        fid_group_dic[fid] = info1;
                    }
                    infos.Remove(info2);
                }
            }

            //List<Dictionary<int, IFeature>> result = new List<Dictionary<int, IFeature>>();

            //foreach (GroupInfo info in infos)
            //{
            //    result.Add(info.dic);
            //}

            return infos;
        }

        private bool AreaEnough(double area) {
            return area > (double)m_application.GenPara["建筑物自动分组面积"];
        }
        /// <summary>
        /// 判断距离与 建筑物自动分组距离的关系
        /// </summary>
        /// <param name="length">比较的距离</param>
        /// <returns>
        /// 长度小于 建筑物分组距离 时 返回-1
        /// 长度大于 建筑物分组距离 小于2倍 建筑物分组距离 时 返回0
        /// 长度大于 2倍 建筑物分组距离 时 返回1
        /// </returns>
        private int LenghtInLimit(double length) {
            double l = (double)m_application.GenPara["建筑物自动分组距离"];
            return (length < l) ? -1 : ((length < l * 2) ? 0 : 1);
        }

        internal class GroupInfo {
            internal Dictionary<int, IPolygon> dic;
            internal GroupInfo(int id, IPolygon frist) {
                dic = new Dictionary<int, IPolygon>();
                dic.Add(id, frist);
            }
            internal double Area {
                get {
                    double a = 0;
                    foreach (IPolygon item in dic.Values) {
                        a += (item as IArea).Area;
                    }
                    return a;
                }
            }
            internal IPoint CenterPoint {
                get {
                    IPoint result = null;
                    foreach (IPolygon item in dic.Values) {
                        IPoint c = (item as IArea).Centroid;
                        if (result == null) {
                            result = c;
                        }
                        else {
                            result.X += c.X;
                            result.Y += c.Y;
                        }
                    }
                    return result;
                }
            }
            internal void Merge(GroupInfo other) {
                foreach (var item in other.dic.Keys) {
                    dic.Add(item, other.dic[item]);
                }
            }

            internal IGeometry Shape {
                get {
                    IGeometry result = null;
                    foreach (IPolygon item in dic.Values) {
                        if (result == null) {
                            result = item;
                        }
                        else {
                            result = (item as ITopologicalOperator).Union(result);
                        }
                    }
                    return result;
                }
            }
        }
    }
}

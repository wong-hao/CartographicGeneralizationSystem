using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen
{
    class DitchesAutoSelectEx : BaseGenTool
    {
        IFeatureClass lineFC;
        INewPolygonFeedback fb;
        public DitchesAutoSelectEx()
        {
            base.m_category = "GWater";
            base.m_caption = "隔条选取沟渠";
            base.m_message = "河流";
            base.m_toolTip = "隔条选取沟渠";
            base.m_name = "CentralizeRiver";
        }
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null;
            }
        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;

            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb == null)
            {
                fb = new NewPolygonFeedbackClass();
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                fb.Start(p);
            }
            else
            {
                fb.AddPoint(p);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (fb != null)
            {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }

        public override void OnDblClick()
        {
            if (fb == null)
                return;

            IPolygon p = fb.Stop();
            fb = null;
            GLayerInfo waterLayer = null;
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
            IFeatureClass waterClass = (waterLayer.Layer as IFeatureLayer).FeatureClass;
            YSDM = waterClass.FindField("要素代码");
            mc = waterClass.FindField("名称");
            sxlx = waterClass.FindField("水系类型");
            yzhlx = waterClass.FindField("养殖类型");
            shmgc = waterClass.FindField("水面高程");
            qdgch = waterClass.FindField("起点高程");
            zhdgch = waterClass.FindField("终点高程");
            shxjb = waterClass.FindField("水系级别");
            gchh = waterClass.FindField("工程号");
            shcrq = waterClass.FindField("施测日期");
            cly = waterClass.FindField("测量员");
            jchy = waterClass.FindField("检查员");
            bzh = waterClass.FindField("备注");
            cjshj = waterClass.FindField("采集时间");
            gxshj = waterClass.FindField("更新时间");
            _GenUsed = waterClass.FindField("_GenUsed");
            Shape_Leng = waterClass.FindField("Shape_Leng");
            /*if (System.Windows.Forms.MessageBox.Show("将进行选取操作，请确认", "提示", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }*/
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;
            if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
            {
                IFeatureClass centralizedDitch2 = FeatWS.OpenFeatureClass("沟渠中心线");
                _GenUsed2 = centralizedDitch2.FindField("_GenUsed");
                autoSelectDitchLines(centralizedDitch2, p);
            }

            m_application.MapControl.Refresh();
        }

        #region 字段
        int YSDM = -1;
        int mc = -1;
        int sxlx = -1;
        int yzhlx = -1;
        int shmgc = -1;
        int qdgch = -1;
        int zhdgch = -1;
        int shxjb = -1;
        int gchh = -1;
        int shcrq = -1;
        int cly = -1;
        int jchy = -1;
        int bzh = -1;
        int cjshj = -1;
        int gxshj = -1;
        int _GenUsed = -1;
        int Shape_Leng = -1;

        int YSDM2 = -1;
        int mc2 = -1;
        int sxlx2 = -1;
        int yzhlx2 = -1;
        int shmgc2 = -1;
        int qdgch2 = -1;
        int zhdgch2 = -1;
        int shxjb2 = -1;
        int gchh2 = -1;
        int shcrq2 = -1;
        int cly2 = -1;
        int jchy2 = -1;
        int bzh2 = -1;
        int cjshj2 = -1;
        int gxshj2 = -1;
        int _GenUsed2 = -1;
        int Shape_Leng2 = -1;
        #endregion

        public bool isOutsideTri(ITinTriangle tri)
        {
            int tag = -1;
            tag = (tri.get_Node(0)).TagValue;
            if (tag == (tri.get_Node(1)).TagValue && tag == (tri.get_Node(2)).TagValue)
            {
                return false;
            }
            if (tag != (tri.get_Node(1)).TagValue && tag != (tri.get_Node(2)).TagValue && (tri.get_Node(1)).TagValue != (tri.get_Node(2)).TagValue)
            {
                return false;
            }
            return true;

        }
        public void addNodeOIDtoList(ITinTriangle tri, List<int> toList)
        {
            bool isExisted = false;
            for (int i = 0; i < 3; i++)
            {
                ITinNode node = tri.get_Node(i);

                foreach (int j in toList)
                {
                    if (j == node.TagValue)
                    {
                        isExisted = true;
                    }
                }
                if (!isExisted)
                {
                    toList.Add(node.TagValue);
                }
            }
        }
        Dictionary<int, bool> hasDeletedFeats = new Dictionary<int, bool>();
        Dictionary<int, bool> hasSavedFeats = new Dictionary<int, bool>();
        Dictionary<int, List<int>> Feat_neighberFeats = new Dictionary<int, List<int>>();
        //List<int> nextList = new List<int>();
        void select(IFeature startFeat, bool priviousFeatHasDeleted)
        {
            //List<int> nextList = new List<int>();
            if (!Feat_neighberFeats.ContainsKey(startFeat.OID) || hasDeletedFeats.ContainsKey(startFeat.OID) || hasSavedFeats.ContainsKey(startFeat.OID))
            {
                return;
            }
            List<int> nextList = Feat_neighberFeats[startFeat.OID];
            if (!priviousFeatHasDeleted)
            {
                if (!hasDeletedFeats.ContainsKey(startFeat.OID))
                {
                    hasDeletedFeats.Add(startFeat.OID, true);
                }
                startFeat.set_Value(_GenUsed2, -1);
                startFeat.Store();
            }
            else
            {
                if (!hasSavedFeats.ContainsKey(startFeat.OID))
                {
                    hasSavedFeats.Add(startFeat.OID, true);
                }
                startFeat.set_Value(_GenUsed2, 1);
                startFeat.Store();
            }

            if (!priviousFeatHasDeleted)
            {
                foreach (int nextIDs in nextList)
                {
                    if (hasDeletedFeats.ContainsKey(nextIDs))
                    {
                        continue;
                    }
                    IFeature nextFeat = ID_Feat[nextIDs];
                    //IFeature nextFeat = lineFC.GetFeature(nextIDs);
                    select(nextFeat, true);
                }
            }
            else
            {
                foreach (int nextIDs in nextList)
                {
                    if (hasSavedFeats.ContainsKey(nextIDs))
                    {
                        continue;
                    }
                    IFeature nextFeat = ID_Feat[nextIDs];
                    //IFeature nextFeat = lineFC.GetFeature(nextIDs);
                    select(nextFeat, false);
                }
            }

        }
        Dictionary<int, IFeature> ID_Feat = new Dictionary<int, IFeature>();
        TinClass tin;
        void autoSelectDitchLines(IFeatureClass DitchLineFeats, IPolygon range)
        {
            lineFC = DitchLineFeats;
            //Dictionary<int, List<IFeature>> groupID_groupMembers = new Dictionary<int, List<IFeature>>();
            ID_Feat.Clear();
            Feat_neighberFeats.Clear();
            hasSavedFeats.Clear();
            hasDeletedFeats.Clear();

            WaitOperation wo = m_application.SetBusy(true);
            wo.SetText("正在进行分析准备……");
            tin = new TinClass();
            tin.InitNew(m_application.MapControl.ActiveView.FullExtent);
            tin.StartInMemoryEditing();

            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int featureCount = DitchLineFeats.FeatureCount(null);
            IFeatureCursor fCursor = DitchLineFeats.Search(sf, false);
            IFeature feature = null;
            ITinNode node = new TinNodeClass();
            object z = 0;
            try
            {
                while ((feature = fCursor.NextFeature()) != null)
                {
                    wo.SetText("正在进行分析(1/4):" + feature.OID.ToString());
                    wo.Step(featureCount);
                    IPolyline line = feature.ShapeCopy as IPolyline;
                    if (line.Length < 1)//1w:100,2.5w:120,5w:150不需隔条需求，直接按长度380删除短的，再手工留下错误代码中的短的河流
                    {
                        feature.set_Value(_GenUsed2, -1);
                        feature.Store();
                        //feature.Delete();
                        continue;
                    }
                    ITopologicalOperator lineto = line as ITopologicalOperator;
                    lineto.Simplify();
                    line.Densify(4, 0);
                    //feature.Shape = line as IGeometry;
                    //feature.Store();
                    IPointCollection pc = line as IPointCollection;
                    for (int i = 0; i < pc.PointCount; i++)
                    {
                        tin.AddShape(pc.get_Point(i), esriTinSurfaceType.esriTinMassPoint, feature.OID, ref z);
                    }
                    ID_Feat.Add(feature.OID, feature);
                }
                ITinEdit tinEdit = tin as ITinEdit;
                ITinAdvanced tinAd = tin as ITinAdvanced;
                ITinValueFilter filter = new TinValueFilterClass();
                ITinFeatureSeed seed = new TinTriangleClass();
                filter.ActiveBound = esriTinBoundType.esriTinUniqueValue;
                //filter.UniqueValue = 0;
                seed.UseTagValue = true;
                wo.Step(0);
                for (int i = 1; i < tin.TriangleCount + 1; i++)
                {
                    wo.SetText("正在进行分析(2/4):" + i);
                    wo.Step(tin.TriangleCount);
                    ITinTriangle tinTriangle = tin.GetTriangle(i);
                    bool istoolong = false;
                    if (!tinTriangle.IsInsideDataArea || !isOutsideTri(tinTriangle))
                    {
                        tinEdit.SetTriangleTagValue(i, -2);
                        continue;
                    }
                    else
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            ITinEdge tinEdge = tinTriangle.get_Edge(k);
                            if (tinEdge.Length > 180)//88,110,180
                            {
                                istoolong = true;
                                tinEdit.SetTriangleTagValue(i, -2);
                                break;
                            }
                        }
                    }
                    int oid1 = -1;
                    int oid2 = -1;
                    if (!istoolong)
                    {
                        oid1 = (tinTriangle.get_Node(0)).TagValue;
                        if ((tinTriangle.get_Node(1)).TagValue != oid1)
                        {
                            oid2 = (tinTriangle.get_Node(1)).TagValue;
                        }
                        else
                        {
                            oid2 = (tinTriangle.get_Node(2)).TagValue;
                        }
                        int lasttagvalue = 0;
                        if (oid1 < oid2)
                        {
                            lasttagvalue = oid1 * 2 + oid2;
                        }
                        else
                        {
                            lasttagvalue = oid2 * 2 + oid1;
                        }
                        tinEdit.SetTriangleTagValue(i, lasttagvalue);
                    }
                }
                wo.Step(0);
                for (int i = 1; i < tin.TriangleCount + 1; i++)
                {
                    wo.SetText("存储分析结果(3/4)..." + i);
                    wo.Step(tin.TriangleCount);
                    int tpValue = (tin.GetTriangle(i)).TagValue;
                    if (tpValue != -2 && tpValue != 0)
                    {
                        filter.UniqueValue = (tin.GetTriangle(i)).TagValue;
                        seed = tin.GetTriangle(i) as ITinFeatureSeed;
                        ITinPolygon tinPoly = tinAd.ExtractPolygon(seed as ITinElement, filter as ITinFilter, false);
                        IPolygon polygon = tinPoly.AsPolygon(null, false);
                        IEnumTinTriangle enumTri = tinPoly.AsTriangles();
                        int triCount = 0;
                        enumTri.Reset();
                        ITinTriangle tpTri = null;
                        List<int> touchOIDList = new List<int>();
                        while ((tpTri = enumTri.Next()) != null)
                        {
                            addNodeOIDtoList(tpTri, touchOIDList);
                            tinEdit.SetTriangleTagValue(tpTri.Index, -2);
                            triCount++;
                        }
                        if (triCount < 30)
                        {
                            continue;
                        }
                        int[] touchArray = touchOIDList.ToArray();
                        if (touchArray.Length < 2)
                        {
                            continue;
                        }
                        IFeature oriFeat = ID_Feat[touchArray[0]];
                        //IFeature oriFeat = DitchLineFeats.GetFeature(touchArray[0]);
                        IPolyline oriLine = oriFeat.Shape as IPolyline;
                        IFeature nextFeat = ID_Feat[touchArray[1]];
                        //IFeature nextFeat = DitchLineFeats.GetFeature(touchArray[1]);
                        IPolyline nextLine = nextFeat.Shape as IPolyline;
                        double oriAngle = (oriLine.FromPoint.Y - oriLine.ToPoint.Y) / (oriLine.FromPoint.X - oriLine.ToPoint.X);
                        double nextAngle = (nextLine.FromPoint.Y - nextLine.ToPoint.Y) / (nextLine.FromPoint.X - nextLine.ToPoint.X);
                        double difference = Math.Abs(oriAngle - nextAngle);
                        if (difference > 0.5)
                        {
                            continue;
                        }

                        if (Feat_neighberFeats.ContainsKey(touchArray[0]))
                        {
                            List<int> neighberFeats = Feat_neighberFeats[touchArray[0]];
                            if (!neighberFeats.Contains(touchArray[1]))
                            {
                                neighberFeats.Add(touchArray[1]);
                            }
                        }
                        else
                        {
                            List<int> neighberFeats = new List<int>();
                            neighberFeats.Add(touchArray[1]);
                            Feat_neighberFeats.Add(touchArray[0], neighberFeats);
                        }

                        if (Feat_neighberFeats.ContainsKey(touchArray[1]))
                        {
                            List<int> neighberFeats = Feat_neighberFeats[touchArray[1]];
                            if (!neighberFeats.Contains(touchArray[0]))
                            {
                                neighberFeats.Add(touchArray[0]);
                            }
                        }
                        else
                        {
                            List<int> neighberFeats = new List<int>();
                            neighberFeats.Add(touchArray[0]);
                            Feat_neighberFeats.Add(touchArray[1], neighberFeats);
                        }
                    }
                }
                wo.Step(0);
                int co = DitchLineFeats.FeatureCount(null);
                //foreach (IFeature f in ID_Feat.Values)
                IFeatureCursor fcuu;
                fcuu = DitchLineFeats.Update(null, true);
                IFeature f = null;
                while ((f = fcuu.NextFeature()) != null)
                {
                    wo.SetText("整理数据(4/4)...");
                    //wo.Step(ID_Feat.Count);
                    wo.Step(co);
                    select(f, false);
                }
                m_application.SetBusy(false);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                m_application.MapControl.Refresh();
            }
            catch (Exception ex)
            {
                m_application.SetBusy(false);
            }
        }

        public override void OnKeyUp(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Space:
                    IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
                    IWorkspace2 WS2 = FeatWS as IWorkspace2;
                    if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
                    {
                        IFeatureClass centralizedDitch2 = FeatWS.OpenFeatureClass("沟渠中心线");
                        IFeatureCursor fcur = null;
                        IFeature feat = null;
                        _GenUsed2 = centralizedDitch2.FindField("_GenUsed");
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = "_GenUsed<0";
                        fcur = centralizedDitch2.Update(qf, false);
                        WaitOperation w = m_application.SetBusy(true);
                        int c = centralizedDitch2.FeatureCount(qf);
                        while ((feat = fcur.NextFeature()) != null)
                        {
                            w.SetText("删除没有选中的要素...");
                            w.Step(c);
                            feat.Delete();
                        }
                        m_application.SetBusy(false);
                    }

                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null)
                    {
                        fb.Stop();
                        fb = null;
                    }
                    break;
            }
        }
    }
}

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
namespace BuildingGen
{
    public class DitchGroup : BaseGenTool
    {
        INewPolygonFeedback fb;
        INewLineFeedback fb_line;
        GLayerInfo buildingLayer;
        int fieldID;
        int maxValue;
        IDataStatistics statistics;
        public DitchGroup()
        {
            base.m_category = "GBuilding";
            base.m_caption = "沟渠面合并选取";
            base.m_message = "沟渠合并选取";
            base.m_toolTip = "沟渠合并选取";
            base.m_name = "DitchGroup";
            //base.m_usedParas = new GenDefaultPara[]
            //{
            //    new GenDefaultPara("沟渠融合距离",(double)15)
            //};
            statistics = new DataStatisticsClass();
        }

        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null
                    && m_application.EngineEditor.EditState != ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing
                    //&& m_application.Workspace.EditLayer != null
                ;
            }
        }

        public override void OnClick()
        {
            buildingLayer = GetLayer(m_application.Workspace);
            fieldID = CheckField(buildingLayer);
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "G_BuildingGroup >0";
            statistics.Cursor = (buildingLayer.Layer as IFeatureLayer).Search(qf, true) as ICursor;
            statistics.Field = "G_BuildingGroup";
            try
            {
                maxValue = Convert.ToInt32(statistics.Statistics.Maximum);
            }
            catch
            {
                maxValue = 0;
            }
        }

        private void ClearGroup(IFeatureClass fc, int index)
        {
            IFeatureCursor cursor = fc.Update(null, true);
            IFeature f = null;
            while ((f = cursor.NextFeature()) != null)
            {
                object v = f.get_Value(index);
                try
                {
                    if (Convert.ToInt32(v) == -1)
                        continue;
                }
                catch
                {
                }
                f.set_Value(index, 0);
                cursor.UpdateFeature(f);
            }
            cursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }
        private GLayerInfo GetLayer(GWorkspace workspace)
        {
            foreach (GLayerInfo info in workspace.LayerManager.Layers)
            {
                if (info.LayerType == GCityLayerType.水系
                    //&& (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    )
                {
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
        private int CheckField(GLayerInfo info)
        {
            IFeatureClass fc = (info.Layer as IFeatureLayer).FeatureClass;
            int index = fc.FindField("G_BuildingGroup");
            if (index == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "G_BuildingGroup";
                field.Type_2 = esriFieldType.esriFieldTypeInteger;
                field.DefaultValue_2 = 0;
                fc.AddField(field);
                index = fc.FindField("G_BuildingGroup");
                ClearGroup(fc, index);
            }
            //(info.Layer as IGeoFeatureLayer).DisplayAnnotation = true;
            //ESRI.ArcGIS.Carto.IAnnotateLayerPropertiesCollection alp = (info.Layer as IGeoFeatureLayer).AnnotationProperties;
            //alp.Clear();
            //ESRI.ArcGIS.Carto.ILabelEngineLayerProperties lelp = new ESRI.ArcGIS.Carto.LabelEngineLayerPropertiesClass();
            //lelp.Expression = "[G_BuildingGroup]";
            //lelp.BasicOverposterLayerProperties.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
            //alp.Add(lelp as ESRI.ArcGIS.Carto.IAnnotateLayerProperties);
            return index;
        }

        internal class GroupInfo
        {
            internal int GroupID;
            internal Dictionary<int, int> IDS_org_tmp;
            internal Dictionary<int, int> IDS_tmp_org;
            internal double dx;
            internal double dy;
            internal IPoint Center;
            internal GroupInfo(int groupID)
            {
                this.GroupID = groupID;
                IDS_org_tmp = new Dictionary<int, int>();
                IDS_tmp_org = new Dictionary<int, int>();
            }
        }


        private IFeatureClass CreateLayer()
        {
            List<IField> fs = new List<IField>();
            IFieldEdit2 field = new FieldClass();
            field.Name_2 = "GroupID";
            field.Type_2 = esriFieldType.esriFieldTypeInteger;
            fs.Add(field as IField);
            field = new FieldClass();
            field.Name_2 = "OrgID";
            field.Type_2 = esriFieldType.esriFieldTypeInteger;
            fs.Add(field as IField);
            return m_application.Workspace.LayerManager.TempLayer(fs);
        }

        IPoint oriPOI = new PointClass();
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 1)
            {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                if (Shift == 1 || fb_line != null)
                {
                    if (fb_line == null)
                    {
                        fb_line = new NewLineFeedbackClass();
                        fb_line.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                        fb_line.Start(p);
                    }
                    else
                    {
                        fb_line.AddPoint(p);
                        IPolyline line = fb_line.Stop();
                        Gen(line);
                        fb_line = null;
                        m_application.MapControl.Refresh();
                    }
                }
                else
                {
                    if (fb == null)
                    {
                        fb = new NewPolygonFeedbackClass();
                        fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                        fb.Start(p);
                        oriPOI = p;
                    }
                    else
                    {
                        fb.AddPoint(p);
                    }
                }
            }
            if (Button == 2)
            {
                //try
                //{
                //    if (System.Windows.Forms.MessageBox.Show("将进行合并操作，请确认", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                //    {
                //        GroupLayer();
                //    }
                //}
                //catch (Exception ex)
                //{
                //}
                return;
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb != null)
            {
                fb.MoveTo(p);
            }
            if (fb_line != null)
            {
                fb_line.MoveTo(p);
            }
        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            //base.OnMouseUp(Button, Shift, X, Y);
        }

        private double CalculateLength(IPoint oriPOI, IPolygon targetPoly)
        {
            IPolycurve targetPolycurve = targetPoly as IPolycurve;
            IPoint nearestPOI = new PointClass();
            double distanceACurve = 0;
            double distanceFCurve = 0;
            bool isRightSide = false;
            targetPolycurve.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, oriPOI, false, nearestPOI, ref distanceACurve, ref distanceFCurve, ref isRightSide);
            return distanceFCurve;
        }

        public override void OnDblClick()
        {
            if (fb == null)
            {
                return;
            }

            IPolygon polygon = fb.Stop();
            fb = null;

            if (buildingLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有水系图层");
                return;
            }
            IFeatureLayer layer1 = new FeatureLayerClass();
            layer1.FeatureClass = (buildingLayer.Layer as IFeatureLayer).FeatureClass;
            layer1.Name = "tp";
            IFeatureClass fc = layer1.FeatureClass;

            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = polygon;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.WhereClause = "要素代码='632250'";
            IFeatureSelection fselect = layer1 as IFeatureSelection;

            fselect.SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
            double distance = (double)m_application.GenPara["沟渠融合距离"];
            //bool ii = CompleteFeatureGroupHandle_NoGroup2(layer1, distance);

            #region Buffer合并
            ISelectionSet set = fselect.SelectionSet;
            ICursor c;
            set.Search(null, false, out c);
            Dictionary<int, IFeature> ID_Feat = new Dictionary<int, IFeature>();
            IFeatureCursor fcur = c as IFeatureCursor;
            IFeature f = null;
            while ((f = fcur.NextFeature()) != null)
            {
                ID_Feat.Add(f.OID, f);
            }            
            GeometryBagClass gb = new GeometryBagClass();
            Dictionary<int, bool> isUnioned = new Dictionary<int, bool>();
            Dictionary<int, bool> isDeleted = new Dictionary<int, bool>();
            WaitOperation w = m_application.SetBusy(true);
            foreach (IFeature oriF in ID_Feat.Values)
            {
                w.Step(ID_Feat.Count);
                //isUnioned.Clear();
                if (isDeleted.ContainsKey(oriF.OID))
                {
                    continue;
                }
                gb.SetEmpty();
                IFeature fb2 = oriF;
                double maxArea = (oriF.Shape as IArea).Area;
                //if (!isUnioned.ContainsKey(oriF.OID))
                //{
                //    isUnioned.Add(oriF.OID, true);
                //}
                List<int> toDeleteID = new List<int>();
                IProximityOperator oriP = oriF.Shape as IProximityOperator;
                IGeometry oriG=oriF.ShapeCopy;
                gb.AddGeometries(1, ref oriG);
                foreach (IFeature anotherF in ID_Feat.Values)
                { 
                    //if (anotherF != oriF&&!isUnioned.ContainsKey(anotherF.OID))
                    if(anotherF!=oriF&&!isDeleted.ContainsKey(anotherF.OID))
                    {
                        double di = oriP.ReturnDistance(anotherF.Shape);
                        if (di < distance)
                        {
                            IGeometry anotherS = anotherF.ShapeCopy;
                            gb.AddGeometries(1, ref anotherS);
                            //isUnioned.Add(anotherF.OID, true);
                            IArea area = anotherF.ShapeCopy as IArea;
                            if (area.Area > maxArea)
                            {
                                maxArea = area.Area;
                                if (!toDeleteID.Contains(fb2.OID))
                                {
                                    toDeleteID.Add(fb2.OID);
                                }
                                if (!isDeleted.ContainsKey(fb2.OID))
                                {
                                    isDeleted.Add(fb2.OID, true);
                                }
                                fb2 = anotherF;
                            }
                            else
                            {
                                if (!toDeleteID.Contains(anotherF.OID))
                                {
                                    toDeleteID.Add(anotherF.OID);
                                }
                                if (!isDeleted.ContainsKey(anotherF.OID))
                                {
                                    isDeleted.Add(anotherF.OID, true);
                                }
                            }
                        }
                    }
                }
                if (fb2 != oriF)
                {
                    if (!toDeleteID.Contains(oriF.OID))
                    {
                        toDeleteID.Add(oriF.OID);
                    }
                    if (!isDeleted.ContainsKey(oriF.OID))
                    {
                        isDeleted.Add(oriF.OID, true);
                    }
                }
                if (gb.GeometryCount == 1)
                {
                    continue;
                }
                IPolygon poly = CompleteFeatureGroupHandle(gb, 15);
                if (fb2 != null && poly != null)
                {
                    fb2.Shape = poly;
                    fb2.Store();
                    int[] deleteID = toDeleteID.ToArray();
                    IFeature deleteFeat = null;
                    for (int j = 0; j < deleteID.Length; j++)
                    {
                        //deleteFeat = fc.GetFeature(deleteID[j]);
                        deleteFeat = ID_Feat[deleteID[j]];
                        deleteFeat.Delete();
                    }
                }
            }
            m_application.SetBusy(false);
            #endregion

            Gen2(polygon);

            m_application.MapControl.Refresh();
        }

        private IPolygon TransformRingToPolygon(IRing ring)
        {
            try
            {
                IPolygon poly;
                ITopologicalOperator topoOper;

                IGeometryCollection geoCol = new PolygonClass();
                object missing = Type.Missing;
                geoCol.AddGeometry(ring as IGeometry, ref missing, ref missing);
                poly = geoCol as IPolygon;
                topoOper = poly as ITopologicalOperator;
                topoOper.Simplify();

                return poly;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }
        private IPolygon CompleteFeatureGroupHandle(IGeometryCollection gb, double distance)
        {
            IPolygon tpPoly = new PolygonClass();
            //for (int m = 0; m < gb.GeometryCount; m++)
            //{
            //    bool isR = false;
            //    tpPoly = gb.get_Geometry(m) as IPolygon;
            //    IProximityOperator toProxi = tpPoly as IProximityOperator;
            //    for (int j = 0; j < gb.GeometryCount; j++)
            //    {
            //        if (j == m)
            //        {
            //            continue;
            //        }
            //        IPolygon tpPoly_ = gb.get_Geometry(j) as IPolygon;
            //        double dis = toProxi.ReturnDistance(tpPoly_);
            //        if (dis < distance)
            //        {
            //            isR = true;
            //            break;
            //        }
            //    }
            //    if (isR == false)
            //    {
            //        System.Windows.Forms.MessageBox.Show("Terrible!距离太远");
            //        return null;
            //    }
            //}
            if (gb.GeometryCount > 1)
            {
                object missing = Type.Missing;
                IGeometryCollection geoColOri = new GeometryBagClass();
                //把原始的Geometry放到geoColOri中
                geoColOri = gb;
                //得到合并后的Geometry,放在topo变量中
                ITopologicalOperator topo = geoColOri.get_Geometry(0) as ITopologicalOperator;
                topo = topo.Buffer(distance) as ITopologicalOperator;
                topo.Simplify();
                for (int i = 1; i < geoColOri.GeometryCount; i++)
                {
                    ITopologicalOperator topoIn = geoColOri.get_Geometry(i) as ITopologicalOperator;
                    topoIn = topoIn.Buffer(distance) as ITopologicalOperator;
                    topoIn.Simplify();
                    topo = topo.Union(topoIn as IGeometry) as ITopologicalOperator;
                }
                topo = topo.Buffer(-distance) as ITopologicalOperator;
                //找出桥，并把凹凸的地方抹掉
                IGeometryCollection geoBridgeCol = new GeometryBagClass();
                ITopologicalOperator topoBridge = topo;
                for (int i = 0; i < geoColOri.GeometryCount; i++)
                {
                    topoBridge = topoBridge.Difference(geoColOri.get_Geometry(i)) as ITopologicalOperator;
                    topoBridge.Simplify();
                }
                IGeometryCollection geoColDent = topoBridge as IGeometryCollection;
                for (int i = 0; i < geoColDent.GeometryCount; i++)//17
                {
                    IGeometry geo = geoColDent.get_Geometry(i);
                    if ((geo as IArea).Area < 0 || geo.IsEmpty || geo == null)
                    {
                        continue;
                    }
                    IPolygon polygon = TransformRingToPolygon(geo as IRing);
                    if ((polygon as IGeometry).IsEmpty)
                    {
                        continue;
                    }
                    IRelationalOperator relDent = polygon as IRelationalOperator;
                    int intersectCount = 0;
                    for (int j = 0; j < geoColOri.GeometryCount; j++)
                    {
                        if (relDent.Touches(geoColOri.get_Geometry(j)))
                        {
                            intersectCount++;
                        }
                    }
                    if (intersectCount < 2)
                    {
                        topo = topo.Difference(polygon) as ITopologicalOperator;
                    }
                    else
                    {
                        geoBridgeCol.AddGeometry(polygon, ref missing, ref missing);
                    }
                }
                //把原始的geometry合并到最后生成的合并好的几何体topo中
                double areaOfOri = 0;
                for (int i = 0; i < geoColOri.GeometryCount; i++)
                {
                    areaOfOri += (geoColOri.get_Geometry(i) as IArea).Area;
                    topo = topo.Union(geoColOri.get_Geometry(i)) as ITopologicalOperator;
                    topo.Simplify();
                }
                IPolygon result = topo as IPolygon;
                return result;

            }
            else
            {
                return null;
            }

        }

        //Dictionary<IFeature, IPolygon> unionedFeats = new Dictionary<IFeature, IPolygon>();
        public override void OnKeyDown(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    if (MessageBox.Show("将清空所有组，是否继续？", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ClearGroup((buildingLayer.Layer as IFeatureLayer).FeatureClass, fieldID);
                        m_application.MapControl.Refresh();
                    }
                    fb = null;
                    fb_line = null;
                    break;
                case (int)System.Windows.Forms.Keys.Space:
                    //GroupLayer();
                    break;
            }
        }

        private void Gen(IPolyline line)
        {
            IFeatureClass fc = (buildingLayer.Layer as IFeatureLayer).FeatureClass;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = line;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
            sf.WhereClause = "要素代码='632250'";
            IFeatureCursor featCur = fc.Search(sf, false);
            List<ifeat_dis> IfeatDis = new List<ifeat_dis>();
            IFeature feat = null;
            while ((feat = featCur.NextFeature()) != null)
            {
                ifeat_dis tp = new ifeat_dis(feat, CalculateLength(line.FromPoint, feat.Shape as IPolygon));
                //ifeat_dis tp = new ifeat_dis(feat, CalculateLength(oriPOI, feat.Shape as IPolygon));
                IfeatDis.Add(tp);
            }
            IfeatDis.Sort((e1, e2) => { return (e1.dis < e2.dis) ? -1 : 1; });
            bool isSet = false;
            foreach (ifeat_dis tppp in IfeatDis)
            {
                if (isSet)
                {
                    IFeature featt = tppp.feat;
                    featt.Delete();

                }
                isSet = !isSet;
            }
            m_application.MapControl.Refresh();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featCur);
        }

        private void Gen2(IPolygon  range)
        {
            IFeatureClass fc = (buildingLayer.Layer as IFeatureLayer).FeatureClass;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.WhereClause = "要素代码='632250'";
            IFeatureCursor featCur = fc.Search(sf, false);
            List<ifeat_dis> IfeatDis = new List<ifeat_dis>();
            IFeature feat = null;
            while ((feat = featCur.NextFeature()) != null)
            {

                ifeat_dis tp = new ifeat_dis(feat, CalculateLength(oriPOI, feat.Shape as IPolygon));
                IfeatDis.Add(tp);

            }
            IfeatDis.Sort((e1, e2) => { return (e1.dis < e2.dis) ? -1 : 1; });
            bool isSet = false;
            foreach (ifeat_dis tppp in IfeatDis)
            {
                if (isSet)
                {
                    IFeature featt = tppp.feat;
                    featt.Delete();

                }
                isSet = !isSet;
            }
            m_application.MapControl.Refresh();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featCur);
        }

        //private void gen2(IPolygon range)
        //{
        //    IFeatureLayer layer = buildingLayer.Layer as IFeatureLayer;
        //    ISpatialFilter sf = new SpatialFilterClass();
        //    sf.Geometry = range;
        //    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
        //    sf.WhereClause = "要素代码='632250'";

        //    IFeatureCursor fCursor = layer.FeatureClass.Search(sf, true);
        //    IFeature feature = null;

        //    object z = 0;
        //    object miss = Type.Missing;
        //    //double paraDistance = (double)m_application.GenPara["湖泊自动分组距离"];
        //    ISpatialReferenceFactory srf_ = new SpatialReferenceEnvironmentClass();
        //    IProjectedCoordinateSystem pcs_ = srf_.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_CM_114E);

        //    TinClass tin = new TinClass();
        //    tin.InitNew(m_application.MapControl.FullExtent);
        //    tin.StartInMemoryEditing();
        //    while ((feature = fCursor.NextFeature()) != null)
        //    {
        //        //aStep(allCount);
        //        IGeometry geoCopy = feature.ShapeCopy;
        //        geoCopy.SpatialReference = pcs_;
        //        IPolygon geo = (geoCopy as ITopologicalOperator).Buffer(-0.1) as IPolygon;
        //        geo.Generalize(0.1);
        //        //geo.Densify(paraDistance / 5, 0);

        //        for (int i = 0; i < (geo as IPointCollection).PointCount; i++)
        //        {
        //            IPoint p = (geo as IPointCollection).get_Point(i);
        //            p.Z = 0;
        //            tin.AddPointZ(p, feature.OID);
        //        }
        //    }



        //}

        internal class ifeat_dis
        {
            internal IFeature feat;
            internal double dis;
            internal ifeat_dis(IFeature tpFeat, double ptDis)
            {
                feat = tpFeat;
                dis = ptDis;
            }
        }



        private void Densify(List<IFeature> featureList)
        {
            IPolygon poly = new PolygonClass();
            foreach (IFeature oriFeature in featureList)
            {
                poly = oriFeature.Shape as IPolygon;
                poly.Densify(7, 0);
                oriFeature.Shape = poly;
                oriFeature.Store();
            }

        }

        private IPolygon4 removeInteriorRings(IPolygon4 ringsPoly)
        {
            ITopologicalOperator tt = ringsPoly as ITopologicalOperator;
            tt.Simplify();
            IGeometryBag gb = ringsPoly.ConnectedComponentBag;
            IEnumGeometry gbe = gb as IEnumGeometry;
            gbe.Reset();
            IGeometry gbeg = gbe.Next();
            double maxAr = 0;
            IGeometry maxG = null;
            while (gbeg != null)
            {
                IArea gbega = gbeg as IArea;
                if (gbega.Area > maxAr)
                {
                    maxG = gbeg;
                    maxAr = gbega.Area;
                }
                gbeg = gbe.Next();
            }
            ringsPoly = maxG as IPolygon4;
            IGeometryBag exteriorRings = ringsPoly.ExteriorRingBag;
            IEnumGeometry exteriorEnum = exteriorRings as IEnumGeometry;
            exteriorEnum.Reset();
            IRing currentExterRing = exteriorEnum.Next() as IRing;
            IRing tpInteriorRing = new RingClass();
            IPolygon4 noInterRingPolygon = new PolygonClass();
            IGeometryCollection ringsCollection = noInterRingPolygon as IGeometryCollection;
            IGeometry forAddGeometry = currentExterRing as IGeometry;
            ringsCollection.AddGeometries(1, ref forAddGeometry);
            while (currentExterRing != null)
            {
                IGeometryBag interiorRings = ringsPoly.get_InteriorRingBag(currentExterRing);
                IEnumGeometry interiorEnum = interiorRings as IEnumGeometry;
                tpInteriorRing = interiorEnum.Next() as IRing;
                while (tpInteriorRing != null)
                {
                    IArea tpInteriorRingArea = tpInteriorRing as IArea;
                    if (Math.Abs(tpInteriorRingArea.Area) > 600)
                    {
                        forAddGeometry = tpInteriorRing as IGeometry;
                        ringsCollection.AddGeometries(1, ref forAddGeometry);

                    }

                    tpInteriorRing = interiorEnum.Next() as IRing;
                }

                currentExterRing = exteriorEnum.Next() as IRing;
            }
            return noInterRingPolygon;
        }

        public bool isOutsideTri(ITinTriangle tri)
        {
            int tag = -1;
            tag = (tri.get_Node(0)).TagValue;
            if (tag == (tri.get_Node(1)).TagValue && tag == (tri.get_Node(2)).TagValue)
            {
                return false;
            }
            else
            {
                return true;
            }


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

        private bool CompleteFeatureGroupHandle_NoGroup2(IFeatureLayer featLayer, double dis)
        {
            Dictionary<int, int> ClusterID = new Dictionary<int, int>();
            IFeatureClass pInputFeatClass = featLayer.FeatureClass;
            IFeatureSelection featSelection = featLayer as IFeatureSelection;
            List<IFeature> featureList = new List<IFeature>();
            ISelectionSet selectionSet = featSelection.SelectionSet;
            ICursor oriCursor;
            IFeature tempFeat;
            selectionSet.Search(null, false, out oriCursor);
            IFeatureCursor cursor = oriCursor as IFeatureCursor;
            while ((tempFeat = cursor.NextFeature()) != null)
            {
                featureList.Add(tempFeat);
            }
            if (featureList.Count > 1)
            {
                object missing = Type.Missing;
                Densify(featureList);
                TinClass tin = new TinClass();
                tin.InitNew(m_application.MapControl.FullExtent);
                tin.StartInMemoryEditing();
                object z = 0;
                IGeometry poly = new PolygonClass();
                IPointCollection geoOriPoints = new MultipointClass();

                foreach (IFeature feature in featureList)
                {
                    IPolygon4 t = feature.ShapeCopy as IPolygon4;
                    IPolygon4 final = removeInteriorRings(t);
                    //IPolygon4 final = t;
                    feature.Shape = final as IGeometry;
                    feature.Store();

                    geoOriPoints = feature.Shape as IPointCollection;

                    int tag = feature.OID;
                    for (int i = 0; i < geoOriPoints.PointCount; i++)
                    {
                        tin.AddShape(geoOriPoints.get_Point(i), esriTinSurfaceType.esriTinMassPoint, tag, ref z);
                    }
                }

                ITinValueFilter filter = new TinValueFilterClass();
                ITinFeatureSeed seed = new TinTriangleClass();
                filter.ActiveBound = esriTinBoundType.esriTinUniqueValue;
                filter.UniqueValue = 0;
                seed.UseTagValue = true;
                ITinEdit tinEdit = tin as ITinEdit;
                ITinAdvanced tinAd = tin as ITinAdvanced;

                for (int i = 1; i < tin.TriangleCount + 1; i++)
                {
                    ITinTriangle tinTriangle = tin.GetTriangle(i);
                    if (!tinTriangle.IsInsideDataArea || !isOutsideTri(tinTriangle))
                    //if (!tinTriangle.IsInsideDataArea )
                    {
                        tinEdit.SetTriangleTagValue(i, -2);
                    }
                    else
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            ITinEdge tinEdge = tinTriangle.get_Edge(k);
                            if (tinEdge.Length > dis)
                            {
                                tinEdit.SetTriangleTagValue(i, -2);
                            }

                        }
                    }
                }

                IGeometryCollection tinPolyColl = new GeometryBagClass();
                IFeature touchFeat;
                List<int> deleteOIDList = new List<int>();
                List<int> touchOIDList = new List<int>();
                try
                {
                    for (int i = 1; i < tin.TriangleCount + 1; i++)
                    {
                        if ((tin.GetTriangle(i)).TagValue == 0)
                        {

                            seed = tin.GetTriangle(i) as ITinFeatureSeed;
                            ITinPolygon tinPoly = tinAd.ExtractPolygon(seed as ITinElement, filter as ITinFilter, false);
                            IPolygon polygon = tinPoly.AsPolygon(null, false);
                            ITopologicalOperator polygonTo = polygon as ITopologicalOperator;
                            polygonTo.Simplify();
                            IEnumTinTriangle enumTri = tinPoly.AsTriangles();

                            ITinTriangle tempTriangle;
                            while ((tempTriangle = enumTri.Next()) != null)
                            {
                                addNodeOIDtoList(tempTriangle, touchOIDList);
                                tinEdit.SetTriangleTagValue(tempTriangle.Index, -1);
                            }
                            int[] array = touchOIDList.ToArray();

                            if (polygon != null && touchOIDList.Count > 0)
                            {
                                IFeature maxAreaFeat = null;
                                double maxArea = 0;

                                for (int m = 0; m < array.Length; m++)
                                {
                                    int touchID = array[m];
                                    while (ClusterID.ContainsKey(touchID))
                                    {
                                        bool a = ClusterID.TryGetValue(touchID, out touchID);
                                    }

                                    touchFeat = pInputFeatClass.GetFeature(touchID);

                                    ITopologicalOperator touchOriTopo = touchFeat.ShapeCopy as ITopologicalOperator;
                                    touchOriTopo.Simplify();
                                    polygonTo = polygonTo.Union(touchFeat.ShapeCopy) as ITopologicalOperator;
                                    polygonTo.Simplify();
                                    if ((touchFeat.Shape as IArea).Area > maxArea)
                                    {
                                        maxArea = (touchFeat.Shape as IArea).Area;
                                        maxAreaFeat = touchFeat;
                                    }

                                }

                                bool c = false;
                                for (int y = 0; y < array.Length; y++)
                                {
                                    if (array[y] != maxAreaFeat.OID)
                                    {
                                        int tpID = array[y];
                                        while (ClusterID.ContainsKey(tpID))
                                        {
                                            bool a = ClusterID.TryGetValue(tpID, out tpID);
                                        }
                                        if (tpID != maxAreaFeat.OID) //确保不会相等，导致最后存储要素为空
                                        {
                                            IFeature todele = pInputFeatClass.GetFeature(tpID);
                                            todele.Delete();
                                            if (ClusterID.ContainsKey(tpID))
                                            {
                                                c = ClusterID.Remove(tpID);
                                            }

                                            ClusterID.Add(tpID, maxAreaFeat.OID);//加入字典，重新梳理已删除的要素与新增要素之间的关系，以便后面融合操作.???待高手解决
                                        }

                                    }
                                }

                                IFeature mergeFeature = maxAreaFeat;

                                IPolygon4 finalPolygon = removeInteriorRings(polygonTo as IPolygon4);
                                //IPolygon4 finalPolygon = polygonTo as IPolygon4;
                                mergeFeature.Shape = finalPolygon as IGeometry;
                                mergeFeature.Store(); 

                                featSelection.Add(mergeFeature);//新生成的要素需重新加入选择集        

                            }
                            touchOIDList.Clear();
                        }

                    }

                }
                catch (Exception ex)
                { }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(tin);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
            }
            featureList.Clear();
            ClusterID.Clear();
            return true;

        }
    }
}

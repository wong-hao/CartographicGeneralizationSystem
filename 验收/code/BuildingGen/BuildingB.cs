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
    public class BuildingB : BaseGenTool {
        INewPolygonFeedback fb;
        INewLineFeedback fb_line;
        GLayerInfo buildingLayer;
        int fieldID;
        int maxValue;
        IDataStatistics statistics;
        public BuildingB() {
            base.m_category = "GBuilding";
            base.m_caption = "分组";
            base.m_message = "建筑物分组";
            base.m_toolTip = "建筑物分组";
            base.m_name = "BuildingB";
            base.m_usedParas = new GenDefaultPara[]
            {
                new GenDefaultPara("建筑物融合距离",(double)5)
                ,new GenDefaultPara("建筑物最小洞面积",(double)100)
                //,new GenDefaultPara("建筑物融合_最小保留洞面积",(double)0)
            };
            statistics = new DataStatisticsClass();
            IsSetDone = false;
        }

        public override void Refresh(int hDC) {
            base.Refresh(hDC);
            if(fb != null)
                fb.Refresh(hDC);
            if (fb_line != null)
                fb_line.Refresh(hDC);
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null
                    && m_application.EngineEditor.EditState != ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing
                    //&& m_application.Workspace.EditLayer != null
                ;
            }
        }

        public override void OnClick() {
            buildingLayer = GetLayer(m_application.Workspace);
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

        private void ClearGroup(IFeatureClass fc, int index,bool ContainsNegative) {
            IFeatureCursor cursor = fc.Update(null, true);
            IFeature f = null;
            while ((f = cursor.NextFeature()) != null) {
                object v = f.get_Value(index);
                try {
                    if (!ContainsNegative && Convert.ToInt32(v) <= 0)
                        continue;
                }
                catch {
                }
                f.set_Value(index, 0);
                cursor.UpdateFeature(f);
            }
            cursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }
        private GLayerInfo GetLayer(GWorkspace workspace) {
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
                ClearGroup(fc, index,true);
            }
            (info.Layer as IGeoFeatureLayer).DisplayAnnotation = true;
            ESRI.ArcGIS.Carto.IAnnotateLayerPropertiesCollection alp = (info.Layer as IGeoFeatureLayer).AnnotationProperties;
            alp.Clear();
            ESRI.ArcGIS.Carto.ILabelEngineLayerProperties lelp = new ESRI.ArcGIS.Carto.LabelEngineLayerPropertiesClass();
            lelp.Expression = "[G_BuildingGroup]";
            lelp.BasicOverposterLayerProperties.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
            alp.Add(lelp as ESRI.ArcGIS.Carto.IAnnotateLayerProperties);
            //(info.Layer as IGeoFeatureLayer).DisplayField = "G_BuildingGroup";
            return index;
        }
        private void GroupLayer() {
            WaitOperation wo = m_application.SetBusy(true);
            try {
                int wCount = 0;
                wo.SetText("正在提取已经分组的建筑物。");
                IFeatureClass fc_org = (buildingLayer.Layer as IFeatureLayer).FeatureClass;
                //qf.WhereClause = "G_BuildingGroup IS Not NULL AND G_BuildingGroup > 0";
                List<GroupInfo> groups = new List<GroupInfo>();
                Dictionary<int, GroupInfo> group_dic_groupid_info = new Dictionary<int, GroupInfo>();
                Dictionary<int, GroupInfo> group_dic_tmpfid_info = new Dictionary<int, GroupInfo>();
                MultipointClass mp = new MultipointClass();
                object miss = Type.Missing;
                IQueryFilter qfAll = new QueryFilterClass();
                qfAll.WhereClause = "G_BuildingGroup > 0";
                IFeatureCursor orgGroupCursor = fc_org.Search(qfAll, true);
                wCount = fc_org.FeatureCount(qfAll);
                IFeature feature = null;
                while ((feature = orgGroupCursor.NextFeature()) != null) {
                    wo.Step(wCount);
                    int groupid = Convert.ToInt32(feature.get_Value(fieldID));
                    if (!group_dic_groupid_info.ContainsKey(groupid)) {
                        group_dic_groupid_info.Add(groupid, new GroupInfo(groupid));
                        group_dic_groupid_info[groupid].Center = (feature.Shape as IArea).Centroid;
                        groups.Add(group_dic_groupid_info[groupid]);
                        mp.AddPoint(group_dic_groupid_info[groupid].Center, ref miss, ref miss);
                    }
                    group_dic_groupid_info[groupid].IDS_org_tmp.Add(feature.OID, -1);
                }
                if (mp.PointCount == 0) {
                    m_application.SetBusy(false);
                    return;
                }
                //else if (mp.PointCount == 1) {
                //    m_application.SetBusy(false);
                //    return;
                //}
                wo.SetText("正在进行数据准备。");
                IPoint centerPoint = (mp.Envelope as IArea).Centroid;
                ITransform2D mp_scaled = mp.Clone() as ITransform2D;
                mp_scaled.Scale(centerPoint, 200, 200);
                IPointCollection mpc_scaled = mp_scaled as IPointCollection;
                for (int i = 0; i < mp.PointCount; i++) {
                    IPoint bp = mp.get_Point(i);
                    IPoint fp = mpc_scaled.get_Point(i);
                    groups[i].dx = fp.X - bp.X;
                    groups[i].dy = fp.Y - bp.Y;
                }
                wo.Step(0);
                IFeatureClass fc_tmp = CreateLayer();
                foreach (GroupInfo g in groups) {
                    foreach (int oid in g.IDS_org_tmp.Keys) {
                        wo.Step(wCount);
                        IFeature f = fc_org.GetFeature(oid);
                        ITransform2D geo = f.ShapeCopy as ITransform2D;
                        geo.Move(g.dx, g.dy);
                        IFeature nf = fc_tmp.CreateFeature();
                        nf.Shape = geo as IGeometry;
                        nf.Store();
                        //g.IDS_org_tmp[oid] = nf.OID;
                        g.IDS_tmp_org.Add(nf.OID, oid);
                        group_dic_tmpfid_info.Add(nf.OID, g);
                    }
                }
                wo.SetText("正在进行合并操作。");
                AggregatePolygons ap = new AggregatePolygons();
                ap.in_features = fc_tmp;
                //processor.SetEnvironmentValue("Workspace", "in_memory");

                ap.out_feature_class = m_application.Workspace.Workspace.PathName + "\\" + m_application.Workspace.LayerManager.TempLayerName();
                //ap.out_table ="c:\\temp\\" + layer.Name + "_tbl";
                ap.aggregation_distance = m_application.GenPara["建筑物融合距离"];
                //ap.minimum_area = m_application.GenPara["建筑物最小上图面积"];
                ap.minimum_hole_size = m_application.GenPara["建筑物最小洞面积"];
                ap.orthogonality_option = "ORTHOGONAL";

                m_application.Geoprosessor.Execute(ap, null);


                wo.SetText("正在整理合并结果。");
                IFeatureClass fc_merge = (m_application.Workspace.Workspace as IFeatureWorkspace).OpenFeatureClass(System.IO.Path.GetFileName(ap.out_feature_class.ToString()));
                ITable table_merge = (m_application.Workspace.Workspace as IFeatureWorkspace).OpenTable(System.IO.Path.GetFileName(ap.out_table.ToString()));

                Dictionary<int, List<int>> merge_dic_out_in = new Dictionary<int, List<int>>();
                Dictionary<int, int> merge_dic_in_out = new Dictionary<int, int>();
                int input_index = table_merge.FindField("INPUT_FID");
                int output_index = table_merge.FindField("OUTPUT_FID");
                int rowCount = table_merge.RowCount(null);
                ICursor cursor_table = table_merge.Search(null, true);
                IRow row_table = null;
                while ((row_table = cursor_table.NextRow()) != null) {
                    wo.Step(rowCount);
                    int input_FID = Convert.ToInt32(row_table.get_Value(input_index));
                    int output_FID = Convert.ToInt32(row_table.get_Value(output_index));
                    if (!merge_dic_out_in.ContainsKey(output_FID)) {
                        merge_dic_out_in.Add(output_FID, new List<int>());
                    }
                    merge_dic_out_in[output_FID].Add(input_FID);
                    if (!merge_dic_in_out.ContainsKey(input_FID)) {
                        merge_dic_in_out.Add(input_FID, output_FID);
                    }
                    else {
                    }
                }

                //int building_struct_id = fc_org.FindField("建筑结构");
                //if (building_struct_id == -1) {
                //}

                wCount = fc_merge.FeatureCount(null);
                wo.Step(0);
                wo.SetText("正在保存合并结果");
                IFeatureCursor outCursor = fc_merge.Search(null, true);
                IFeature outFeature = null;
                IGeometry mergePolygon = null;
                double miniArea = (double)m_application.GenPara["建筑物最小上图面积"];
                while ((outFeature = outCursor.NextFeature()) != null) {
                    wo.Step(wCount);
                    wo.SetText("正在保存合并结果[" + outFeature.OID + "/" + wCount + "]");
                    try {
                        ITransform2D shape = outFeature.ShapeCopy as ITransform2D;
                        int oid = outFeature.OID;
                        List<int> tmp_OIDs = merge_dic_out_in[oid];
                        GroupInfo gInfo = group_dic_tmpfid_info[tmp_OIDs[0]];
                        int org_OID = gInfo.IDS_tmp_org[tmp_OIDs[0]];
                        shape.Move(-gInfo.dx, -gInfo.dy);

                        IFeature usedFeature = null;
                        foreach (int tmp_OID in tmp_OIDs) {
                            int orgFeatureOID = gInfo.IDS_tmp_org[tmp_OID];
                            IFeature orgFeature = fc_org.GetFeature(orgFeatureOID);
                            if (usedFeature == null) {
                                usedFeature = orgFeature;
                            }
                            else if ((usedFeature.Shape as IArea).Area < (orgFeature.Shape as IArea).Area) {
                                usedFeature = orgFeature;
                            }
                        }

                        (shape as ITopologicalOperator).Simplify();
                        //if (mergePolygon == null) {
                        //    mergePolygon = (shape as IClone).Clone() as IGeometry;
                        //}
                        //else {

                        //    shape = (shape as ITopologicalOperator).Difference(mergePolygon) as ITransform2D;
                        //    mergePolygon = (mergePolygon as ITopologicalOperator).Union(shape as IGeometry);
                        //}
                        IGeometryCollection gc = (shape as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                        for (int shapeid = 0; shapeid < gc.GeometryCount; shapeid++) {
                            IGeometry shapeToAdd = gc.get_Geometry(shapeid);
                            if (shapeToAdd == null || shapeToAdd.IsEmpty)
                                continue;

                            IFeature addFeature = fc_org.CreateFeature();
                            addFeature.Shape = shape as IGeometry;
                            for (int fieldid = 0; fieldid < addFeature.Fields.FieldCount; fieldid++) {
                                IField field = addFeature.Fields.get_Field(fieldid);
                                if (field.Editable && field.Type != esriFieldType.esriFieldTypeGeometry) {
                                    if (fieldid == fieldID) {
                                        addFeature.set_Value(fieldid, -1);
                                    }
                                    else {
                                        addFeature.set_Value(fieldid, usedFeature.get_Value(fieldid));
                                    }
                                }
                            }
                            addFeature.Store();

                        }
                    }
                    catch (Exception ex) {
                        continue;
                    }

                }

                wCount = fc_org.FeatureCount(qfAll);
                wo.Step(0);
                wo.SetText("正在清理临时数据");
                IFeatureCursor delectCursor = fc_org.Update(qfAll, true);
                while (delectCursor.NextFeature() != null) {
                    wo.Step(0);
                    delectCursor.DeleteFeature();
                }
                delectCursor.Flush();
            }
            catch (Exception ex) {
            }
            finally {
                m_application.SetBusy(false);
            }
            m_application.MapControl.Refresh();
        }

        internal class GroupInfo {
            internal int GroupID;
            internal Dictionary<int, int> IDS_org_tmp;
            internal Dictionary<int, int> IDS_tmp_org;
            internal double dx;
            internal double dy;
            internal IPoint Center;
            internal GroupInfo(int groupID) {
                this.GroupID = groupID;
                IDS_org_tmp = new Dictionary<int, int>();
                IDS_tmp_org = new Dictionary<int, int>();
            }
        }


        private IFeatureClass CreateLayer() {
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
        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button == 1) {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                if (Shift == 1 || fb_line != null) {
                    if (fb_line == null) {
                        fb_line = new NewLineFeedbackClass();
                        fb_line.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                        fb_line.Start(p);
                    }
                    else {
                        fb_line.AddPoint(p);
                        IPolyline line = fb_line.Stop();
                        fb_line = null;
                        IPoint fromPoint = line.FromPoint;
                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.Geometry = fromPoint;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        IFeatureCursor fCursor = (buildingLayer.Layer as IFeatureLayer).FeatureClass.Update(sf, true);
                        IFeature feature = fCursor.NextFeature();
                        if (feature == null)
                            return;
                        sf.Geometry = line.ToPoint;
                        IFeatureCursor cursor = (buildingLayer.Layer as IFeatureLayer).FeatureClass.Update(sf, true);
                        IFeature oFe = cursor.NextFeature();
                        object value = 0;
                        if (oFe != null) {
                            value = oFe.get_Value(fieldID);
                        }
                        feature.set_Value(fieldID, value);
                        fCursor.UpdateFeature(feature);
                        fCursor.Flush();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                        m_application.MapControl.Refresh();
                    }
                }
                else {
                    if (fb == null) {
                        fb = new NewPolygonFeedbackClass();
                        fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                        fb.Start(p);
                    }
                    else {
                        fb.AddPoint(p);
                    }
                }
            }
            if (Button == 2) {
                try {
                    if (System.Windows.Forms.MessageBox.Show("将进行合并操作，请确认", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK) {
                        GroupLayer();
                    }
                }
                catch (Exception ex) {
                }
            }
        }
        public override void OnMouseMove(int Button, int Shift, int X, int Y) {
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb != null) {
                fb.MoveTo(p);
            }
            if (fb_line != null) {
                fb_line.MoveTo(p);
            }
        }
        private bool IsSetDone;
        public override void OnMouseUp(int Button, int Shift, int X, int Y) {
            //base.OnMouseUp(Button, Shift, X, Y);
        }
        public override void OnDblClick() {
            if (fb == null) {
                return;
            }

            IPolygon polygon = fb.Stop();
            fb = null;

            if (buildingLayer == null) {
                System.Windows.Forms.MessageBox.Show("没有建筑物图层");
                return;
            }
            ISpatialFilter filter = new SpatialFilterClass();
            filter.Geometry = polygon;
            filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor FCursor = (buildingLayer.Layer as IFeatureLayer).FeatureClass.Update(filter, true);
            IFeature feature = null;
            int v = -1;
            if (!IsSetDone) {
                maxValue++;
                v = maxValue;
            }
            v = v % 100000;
            while ((feature = FCursor.NextFeature()) != null) {
                feature.set_Value(fieldID, v);
                FCursor.UpdateFeature(feature);
            }
            FCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(FCursor);
            m_application.MapControl.Refresh();
        }
        public override void OnKeyDown(int keyCode, int Shift) {
            switch (keyCode) {
            case (int)System.Windows.Forms.Keys.Escape:
                if (fb != null) {
                    fb.Stop();
                    fb = null;
                }
                else if (fb_line != null) {
                    fb_line.Stop();
                    fb_line = null;
                }
                else if (MessageBox.Show("将清空所有组，是否继续？", "注意", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                    ClearGroup((buildingLayer.Layer as IFeatureLayer).FeatureClass, fieldID,false);
                    m_application.MapControl.Refresh();
                }
                break;
            case (int)System.Windows.Forms.Keys.Space:
                //GroupLayer();
                break;  
            case (int)System.Windows.Forms.Keys.A:
                IsSetDone = !IsSetDone;
                break;
            case (int)System.Windows.Forms.Keys.C:
                if (MessageBox.Show("即将把所有编组号变为0，是否继续？", "注意", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                    ClearGroup((buildingLayer.Layer as IFeatureLayer).FeatureClass, fieldID, true);
                    m_application.MapControl.Refresh();
                }
                break;
            }
        }

    }
}

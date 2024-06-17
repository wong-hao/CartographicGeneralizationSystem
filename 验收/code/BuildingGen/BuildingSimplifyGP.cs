using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
namespace BuildingGen {
    public class BuildingSimplifyGP : BaseGenTool {
        public BuildingSimplifyGP() {
            base.m_category = "GBuilding";
            base.m_caption = "化简";
            base.m_message = "对选定图层做建筑物化简";
            base.m_toolTip = "对选定图层做建筑物化简；\n按r对分组号为-1的建筑物进行化简。";
            base.m_name = "BuildingSimplifyGP";
            base.m_usedParas = new GenDefaultPara[]
            {
                new GenDefaultPara("建筑物化简容差",(double)5)
                ,new GenDefaultPara("建筑物最小洞面积",(double)100)
                //,new GenDefaultPara("",(double)0)
            };
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null
                    && m_application.EngineEditor.EditState != ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing
                    //&& m_application.Workspace.EditLayer != null
                    ;
            }
        }
        public override void OnKeyDown(int keyCode, int Shift) {
            switch (keyCode) {
            case (int)System.Windows.Forms.Keys.Escape:
                if (fb != null) {
                    fb.Stop();
                    fb = null;
                }
                break;
            case (int)System.Windows.Forms.Keys.R:
                if(System.Windows.Forms.MessageBox.Show("将要化简分组号为-1的建筑物，是否继续？","耗时操作提示", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    Gen2(null);
                break;
            }

        }
        private GLayerInfo GetLayer(GWorkspace workspace) {
            foreach (GLayerInfo info in workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.建筑物
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && info.OrgLayer != null
                    ) {
                    return info;
                }
            }
            return null;
        }
        private void Gen2(IPolygon range) {

            IMap map = m_application.Workspace.Map;
            GLayerInfo info = GetLayer(m_application.Workspace);
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null) {
                return;
            }
            IFeatureClass fc = layer.FeatureClass;

            IQueryFilter simpleQf = null;

            int groupID = fc.FindField("G_BuildingGroup");

            if (range != null) {
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = range;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                if (groupID >= 0)
                    sf.WhereClause = "G_BuildingGroup <> -3";
                simpleQf = sf;
            }
            else {
                if (groupID < 0)
                    return;
                IQueryFilter iqf = new QueryFilterClass();
                iqf.WhereClause = "G_BuildingGroup = -1";
                simpleQf = iqf;
            }

            WaitOperation wo = m_application.SetBusy(true);
            wo.SetText("正在读取化简数据");

            List<IField> fields = new List<IField>();
            IFieldEdit2 field = new FieldClass();
            field.Name_2 = "simpoid";
            field.Type_2 = esriFieldType.esriFieldTypeInteger;
            fields.Add(field as IField);
            field = new FieldClass();
            field.Name_2 = "dx";
            field.Type_2 = esriFieldType.esriFieldTypeDouble;
            fields.Add(field as IField);
            field = new FieldClass();
            field.Name_2 = "dy";
            field.Type_2 = esriFieldType.esriFieldTypeDouble;
            fields.Add(field as IField);
            IFeatureClass fc_tp = m_application.Workspace.LayerManager.TempLayer(fields);

            IFeatureCursor insert_Cursor = fc_tp.Insert(true);
            IFeatureCursor checkCursor = fc.Search(simpleQf, true);
            IFeature checkFeature = null;
            List<int> ids = new List<int>();
            Dictionary<int, IPoint> fid_dxy_dic = new Dictionary<int, IPoint>();
            object miss = Type.Missing;
            int wCount = fc.FeatureCount(simpleQf);
            MultipointClass mp = new MultipointClass();
            IQueryFilter qf = new QueryFilterClass();
            while ((checkFeature = checkCursor.NextFeature()) != null) {
                wo.Step(wCount);
                ids.Add(checkFeature.OID);
                IPoint p = (checkFeature.Shape as IArea).Centroid;
                mp.AddPoint(p, ref miss, ref miss);
            }
            wo.Step(0);
            wo.SetText("正在分析化简数据");
            IPointCollection mp_trans = mp.Clone() as IPointCollection;
            IPoint centerPoint = (mp.Envelope as IArea).Centroid;
            (mp_trans as ITransform2D).Scale(centerPoint, 200, 200);
            for (int i = 0; i < mp.PointCount; i++) {
                wo.Step(mp.PointCount);
                PointClass p = new PointClass();
                p.X = mp_trans.get_Point(i).X - mp.get_Point(i).X;
                p.Y = mp_trans.get_Point(i).Y - mp.get_Point(i).Y;
                int fid = ids[i];
                IFeature feature = fc.GetFeature(fid);
                ITransform2D shape = feature.ShapeCopy as ITransform2D;
                shape.Move(p.X, p.Y);
                IFeatureBuffer fbuffer = fc_tp.CreateFeatureBuffer();
                fbuffer.Shape = shape as IGeometry;
                fbuffer.set_Value(fbuffer.Fields.FindField("simpoid"), fid);
                fbuffer.set_Value(fbuffer.Fields.FindField("dx"), p.X);
                fbuffer.set_Value(fbuffer.Fields.FindField("dy"), p.Y);
                insert_Cursor.InsertFeature(fbuffer);
                fid_dxy_dic.Add(fid, p);
            }
            insert_Cursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insert_Cursor);

            SimplifyBuilding sb = new SimplifyBuilding();

            sb.in_features = fc_tp;

            string outName = m_application.Workspace.LayerManager.TempLayerName();

            sb.out_feature_class = m_application.Workspace.Workspace.PathName + "\\" + outName;
            //m_application.Geoprosessor.SetEnvironmentValue("workspace", m_application.Workspace.Workspace.PathName);
            sb.simplification_tolerance = m_application.GenPara["建筑物化简容差"];

            wo.SetText("正在化简建筑物");
            object result = m_application.Geoprosessor.Execute(sb, null);

            if (result == null) {
                m_application.SetBusy(false);                
                throw new Exception();
            }
            IFeatureClass fc_gen = (m_application.Workspace.Workspace as IFeatureWorkspace).OpenFeatureClass(outName);
            IFeatureCursor gen_cursor = fc_gen.Search(null, true);
            IFeature gen_feature = null;
            wCount = fc_gen.FeatureCount(null);
            wo.Step(0);
            wo.SetText("正在整理化简结果");
            //object miss = Type.Missing;
            while ((gen_feature = gen_cursor.NextFeature()) != null) {
                wo.Step(wCount);
                int org_oid = (int)gen_feature.get_Value(gen_feature.Fields.FindField("simpoid"));
                double dx = (double)gen_feature.get_Value(gen_feature.Fields.FindField("dx"));
                double dy = (double)gen_feature.get_Value(gen_feature.Fields.FindField("dy"));
                IFeature org_feature = fc.GetFeature(org_oid);
                ITransform2D shape = gen_feature.ShapeCopy as ITransform2D;
                shape.Move(-dx, -dy);
                (shape as IPolygon).Generalize((double)m_application.GenPara["建筑物化简容差"] * 0.2);
                (shape as ITopologicalOperator).Simplify();
                if ((shape as IGeometryCollection).GeometryCount > 1) {
                    IGeometryCollection gc = shape as IGeometryCollection;
                    PolygonClass newPolygon = new PolygonClass();

                    for (int i = 0; i < gc.GeometryCount; i++) {
                        IArea ring = gc.get_Geometry(i) as IArea;
                        if (Math.Abs(ring.Area) * 10 > (double)m_application.GenPara["建筑物最小上图面积"]) {
                            newPolygon.AddGeometry(ring as IGeometry, ref miss, ref miss);
                        }
                    }
                    if (!newPolygon.IsEmpty) {
                        newPolygon.Simplify();
                        shape = newPolygon;
                    }
                }
                org_feature.Shape = shape as IGeometry;
                if (groupID >= 0)
                    org_feature.set_Value(groupID, -2);
                org_feature.Store();
            }
            m_application.SetBusy(false);
            m_application.MapControl.Refresh();
        }
        private void Gen(IPolygon range) {
            IMap map = m_application.Workspace.Map;
            GLayerInfo info = GetLayer(m_application.Workspace);
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null) {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;

            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            ISpatialFilter sf2 = new SpatialFilterClass();            
            sf2.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            WaitOperation wo = m_application.SetBusy(true);

            IFeatureCursor checkCursor = fc.Search(sf, true);
            IFeature checkFeature = null;
            //List<int> 
            object miss = Type.Missing;
            wo.SetText("正在准备化简数据");
            int wCount = fc.FeatureCount(sf);
            Dictionary<int, int> mDic = new Dictionary<int, int>();
            while ((checkFeature = checkCursor.NextFeature())!=null) {
                wo.Step(wCount);
                if ((checkFeature.Shape as IGeometryCollection).GeometryCount > 1) {
                    PolygonClass iLine = new PolygonClass();
                    IRing er = ((checkFeature.Shape as IPolygon4).ExteriorRingBag as IGeometryCollection).get_Geometry(0) as IRing;
                    IGeometryCollection gcr = (checkFeature.Shape as IPolygon4).get_InteriorRingBag(er) as IGeometryCollection;
                    for (int i = 0; i < gcr.GeometryCount; i++) {
                        iLine.AddGeometry(gcr.get_Geometry(i), ref miss, ref miss);
                    }
                    iLine.Simplify();
                    sf2.Geometry = iLine;
                    IFeatureCursor iCursor = fc.Search(sf2, true);
                    IFeature iFeature = null;
                    IGeometry poly = checkFeature.ShapeCopy;
                    while ((iFeature = iCursor.NextFeature()) != null) {
                        if (iFeature.OID == checkFeature.OID) {
                            continue;
                        }
                        poly = (poly as ITopologicalOperator).Union(iFeature.Shape);
                        if (!mDic.ContainsKey(iFeature.OID))
                            mDic.Add(iFeature.OID, iFeature.OID);
                    }
                    checkFeature.Shape = poly;
                    checkFeature.Store();
                }
            }
            foreach (var item in mDic.Keys) {
                IFeature dFe = fc.GetFeature(item);
                dFe.Delete();
            }
            IFeatureLayer tmpLayer = new FeatureLayerClass();
            tmpLayer.FeatureClass = fc;

            (tmpLayer as IFeatureSelection).SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
            SimplifyBuilding sb = new SimplifyBuilding();

            sb.in_features = tmpLayer;
            //int i = 1;
            string featureName = layer.Name;
            string outName = m_application.Workspace.LayerManager.TempLayerName();
            tmpLayer.Name = outName;
            featureName = outName;
            sb.out_feature_class = m_application.Workspace.Workspace.PathName + "\\" + featureName;
            //m_application.Geoprosessor.SetEnvironmentValue("workspace", m_application.Workspace.Workspace.PathName);
            sb.simplification_tolerance = m_application.GenPara["建筑物化简容差"];
            //sb.minimum_area = m_application.GenPara["建筑物最小上图面积"];

            sb.conflict_option = "CHECK_CONFLICTS";
            try {
                //RepairGeometry rg = new RepairGeometry();
                //rg.in_features = tmpLayer;
                //object result = m_application.Geoprosessor.Execute(rg, null);
                wo.SetText("正在化简建筑物");
                object result = m_application.Geoprosessor.Execute(sb, null);

                if (result == null) {
                    throw new Exception();
                }
                IFeatureClass fc_gen = (m_application.Workspace.Workspace as IFeatureWorkspace).OpenFeatureClass(outName);
                IFeatureCursor cursor_gen = fc_gen.Search(null, true);
                IFeature feature_gen = null;
                Dictionary<int, int> addIDs = new Dictionary<int, int>();
                wo.SetText("正在整理化简结果");
                wo.Step(0);
                wCount = fc_gen.FeatureCount(null);
                while ((feature_gen = cursor_gen.NextFeature()) != null) {
                    wo.Step(wCount);
                    IFeature feature_new = fc.CreateFeature();
                    IPolygon shapecopy = feature_gen.ShapeCopy as IPolygon;
                    shapecopy.Generalize((double)sb.simplification_tolerance * 0.2);
                    feature_new.Shape = shapecopy;
                    for (int j = 0; j < fc.Fields.FieldCount; j++) {
                        IField field = fc.Fields.get_Field(j);
                        if (field.Editable && field.Type != esriFieldType.esriFieldTypeGeometry) {
                            feature_new.set_Value(j, feature_gen.get_Value(feature_gen.Fields.FindField(field.Name)));
                        }
                    }
                    feature_new.Store();
                    addIDs.Add(feature_new.OID, feature_new.OID);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor_gen);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fc_gen);
                IFeatureCursor delete_Cursor = fc.Update(sf, true);
                IFeature feature_delete = null;
                wo.SetText("正在清理");
                wCount = fc.FeatureCount(sf);
                while ((feature_delete = delete_Cursor.NextFeature()) != null) {
                    wo.Step(wCount);
                    if (addIDs.ContainsKey(feature_delete.OID)) {
                        continue;
                    }
                    delete_Cursor.DeleteFeature();
                }
                delete_Cursor.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(delete_Cursor);
                //m_application.Workspace.LayerManager.AddExistLayer(featureName, layer);
            }
            catch {
                System.Windows.Forms.MessageBox.Show("化简出错");
            }
            m_application.SetBusy(false);

            m_application.MapControl.Refresh();
        }
        public override void OnClick() {
        }
        INewPolygonFeedback fb;
        public override void Refresh(int hDC) {
            base.Refresh(hDC);
            if (fb != null)
                fb.Refresh(hDC);
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
                Gen2(poly);
            }
        }

    }
}

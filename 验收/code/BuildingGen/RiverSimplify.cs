using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen {
    public class RiverSimplify : BaseGenTool {
        List<IPolyline> lines;
        SimpleLineSymbolClass lineSymbol;
        public RiverSimplify() {
            base.m_category = "GWater";
            base.m_caption = "中心线";
            base.m_message = "对选定双线河进行化简";
            base.m_toolTip = "对选定双线河进行化简";
            base.m_name = "RiverSimplify";
            lines = new List<IPolyline>();
            lineSymbol = new SimpleLineSymbolClass();
            lineSymbol.Width = 2;
        }

        public override bool Enabled {
            get {
                return (m_application.Workspace != null);
            }
        }

        private GLayerInfo GetLayer(GWorkspace workspace) {
            foreach (GLayerInfo info in workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.水系
                    //&& (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && info.OrgLayer != null
                    ) {
                    return info;
                }
            }
            return null;
        }

        public override void OnClick() {
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
            IMap map = m_application.Workspace.Map;
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
            IFeatureLayer layer = (waterLayer.Layer as IFeatureLayer);
            if (layer == null) {
                System.Windows.Forms.MessageBox.Show("没有找到水系面");
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
        }
        public override bool Deactivate() {
            m_application.MapControl.OnAfterDraw -= new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            return base.Deactivate();
        }
        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            IDisplay dis = e.display as IDisplay;
            dis.SetSymbol(lineSymbol);
            foreach (var item in lines) {
                dis.DrawPolyline(item);
            }
        }
        List<IFeature> feats = new List<IFeature>();
        void GetCenterline(IPolygon range) {
            feats.Clear();
            GLayerInfo info = GetLayer(m_application.Workspace);
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null) {
                System.Windows.Forms.MessageBox.Show("没有找到水系面");
            }

            SpatialFilterClass sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureClass fc = layer.FeatureClass;
            IFeature feaute = null;
            IFeatureCursor cursor = fc.Search(sf, false);
            IPolygon poly = null;
            while ((feaute = cursor.NextFeature())!=null) {
                feats.Add(feaute);
                if (poly == null) {
                    poly = feaute.ShapeCopy as IPolygon;
                }
                else {
                    poly = (poly as ITopologicalOperator).Union(feaute.Shape) as IPolygon;
                }
            }
            IGeometryCollection gc = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
            BuildingGenCore.CenterLineFactory cf = new BuildingGenCore.CenterLineFactory();

            lines.Clear();
            for (int i = 0; i < gc.GeometryCount; i++) {
                IPolygon part = gc.get_Geometry(i) as IPolygon;
                try {
                    BuildingGenCore.CenterLine cl = cf.Create2(part);
                    lines.Add(cl.Line);
                }
                catch { 
                }
            }
        }


        INewPolygonFeedback fb;
        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button == 4)
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
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (fb != null)
                fb.MoveTo(p);
        }
        public override void OnDblClick() {
            if (fb != null) {
                IPolygon poly = fb.Stop();
                fb = null;
                GetCenterline(poly);
                m_application.MapControl.Refresh();
            }
        }
        public override void OnKeyDown(int keyCode, int Shift) {
            IFeatureClass centralizedDitch = null;
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;
            if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
            {
                centralizedDitch = FeatWS.OpenFeatureClass("沟渠中心线");
            }
            #region 字段
            YSDM2 = centralizedDitch.FindField("要素代码");
            mc2 = centralizedDitch.FindField("名称");
            sxlx2 = centralizedDitch.FindField("水系类型");
            yzhlx2 = centralizedDitch.FindField("养殖类型");
            shmgc2 = centralizedDitch.FindField("水面高程");
            qdgch2 = centralizedDitch.FindField("起点高程");
            zhdgch2 = centralizedDitch.FindField("终点高程");
            shxjb2 = centralizedDitch.FindField("水系级别");
            gchh2 = centralizedDitch.FindField("工程号");
            shcrq2 = centralizedDitch.FindField("施测日期");
            cly2 = centralizedDitch.FindField("测量员");
            jchy2 = centralizedDitch.FindField("检查员");
            bzh2 = centralizedDitch.FindField("备注");
            cjshj2 = centralizedDitch.FindField("采集时间");
            gxshj2 = centralizedDitch.FindField("更新时间");
            _GenUsed2 = centralizedDitch.FindField("_GenUsed");
            Shape_Leng2 = centralizedDitch.FindField("Shape_Leng");
            #endregion

            //groupIndex = centralizedDitch.FindField("G_BuildingGroup");

            int index = centralizedDitch.FindField("G_BuildingGroup");
            if (index == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "G_BuildingGroup";
                field.Type_2 = esriFieldType.esriFieldTypeInteger;
                field.DefaultValue_2 = 0;
                centralizedDitch.AddField(field);
                index = centralizedDitch.FindField("G_BuildingGroup");
                //ClearGroup(oriFeatClass, index);
            }

            switch (keyCode) { 
            case (int)System.Windows.Forms.Keys.Escape:
                if (fb != null) {
                    fb.Stop();
                    fb = null;
                    feats.Clear();
                    lines.Clear();
                }
                break;
            case (int)System.Windows.Forms.Keys.Space:

                IFeatureCursor forInsertCur = centralizedDitch.Insert(false);
                IFeature[] featss = feats.ToArray();
                IFeatureBuffer forInsertBuffer = centralizedDitch.CreateFeatureBuffer();
                ConvertAttributes_Feat(featss[0], forInsertBuffer);
                IPolyline[] line = lines.ToArray();
                forInsertBuffer.Shape = line[0];
                forInsertBuffer.set_Value(_GenUsed2, 0);
                forInsertCur.InsertFeature(forInsertBuffer);
                featss[0].Delete();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(forInsertCur);
                m_application.MapControl.Refresh();
                break;
            }
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
        void ConvertAttributes_Feat(IFeature oriFeat, IFeatureBuffer targetBuffer)
        {
            if (YSDM2 != -1)
                targetBuffer.set_Value(YSDM2, oriFeat.get_Value(YSDM));
            if (mc2 != -1)
                targetBuffer.set_Value(mc2, oriFeat.get_Value(mc));
            if (sxlx2 != -1)
                targetBuffer.set_Value(sxlx2, oriFeat.get_Value(sxlx));
            if (yzhlx2 != -1)
                targetBuffer.set_Value(yzhlx2, oriFeat.get_Value(yzhlx));
            if (shmgc2 != -1)
                targetBuffer.set_Value(shmgc2, oriFeat.get_Value(shmgc));
            if (qdgch2 != -1)
                targetBuffer.set_Value(qdgch2, oriFeat.get_Value(qdgch));
            if (zhdgch2 != -1)
                targetBuffer.set_Value(zhdgch2, oriFeat.get_Value(zhdgch));
            if (shxjb2 != -1)
                targetBuffer.set_Value(shxjb2, oriFeat.get_Value(shxjb));
            if (gchh2 != -1)
                targetBuffer.set_Value(gchh2, oriFeat.get_Value(gchh));
            if (shcrq2 != -1)
                targetBuffer.set_Value(shcrq2, oriFeat.get_Value(shcrq));
            if (cly2 != -1)
                targetBuffer.set_Value(cly2, oriFeat.get_Value(cly));
            if (jchy2 != -1)
                targetBuffer.set_Value(jchy2, oriFeat.get_Value(jchy));
            if (bzh2 != -1)
                targetBuffer.set_Value(bzh2, oriFeat.get_Value(bzh));
            if (cjshj2 != -1)
                targetBuffer.set_Value(cjshj2, oriFeat.get_Value(cjshj));
            if (gxshj2 != -1)
                targetBuffer.set_Value(gxshj2, oriFeat.get_Value(gxshj));
            if (_GenUsed2 != -1)
                targetBuffer.set_Value(_GenUsed2, oriFeat.get_Value(_GenUsed));
            if (Shape_Leng2 != -1)
                targetBuffer.set_Value(Shape_Leng2, oriFeat.get_Value(Shape_Leng));

        }
    }
}

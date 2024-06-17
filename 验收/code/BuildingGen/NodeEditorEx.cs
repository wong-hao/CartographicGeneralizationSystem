using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

namespace BuildingGen {
    class NodeEditorEx : BaseGenTool {
        GCityLayerType currentLayerType;
        SimpleFillSymbolClass sfs;
        SimpleMarkerSymbolClass sms;
        public NodeEditorEx(GCityLayerType layerType) {
            base.m_category = "GBuilding";
            base.m_caption = "编辑节点";
            base.m_message = "对选定要素进行编辑";
            base.m_toolTip = "对选定要素进行编辑。\n鼠标移至节点处，右键删除；\n按住shift键左键点击加入节点；\n空格键保存。";
            base.m_name = "nodeeditor";

            currentLayerType = layerType;
            sfs = new SimpleFillSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 255;
            rgb.Blue = 0;
            rgb.Green = 0;
            SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
            sls.Width = 2;
            sls.Color = rgb;
            sfs.Outline = sls;
            sfs.Style = esriSimpleFillStyle.esriSFSNull;
            sms = new SimpleMarkerSymbolClass();
            sms.Size = 9;
            sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
        }

        public override bool Enabled {
            get {
                return (m_application.Workspace != null);
            }
        }
        bool initDraw = false;
        GLayerInfo info;
        public override void OnClick() {
            info = null;
            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers) {
                if (tempInfo.LayerType == currentLayerType
                    && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && tempInfo.OrgLayer != null
                    ) {
                    info = tempInfo;
                    break;
                }
            }
            if (info == null) {
                System.Windows.Forms.MessageBox.Show("没有找到图层");
                return;
            }

            if (!initDraw) {
                m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
                initDraw = true;
            }
        }
        IFeature selectFeature = null;

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            if (selectFeature == null) {
                return;
            }
            IPolygon SelectPolygon = selectFeature.Shape as IPolygon;
            IDisplay dis = e.display as IDisplay;
            dis.SetSymbol(sfs);
            dis.DrawPolygon(SelectPolygon);
            dis.SetSymbol(sms);
            for (int i = 0; i < (SelectPolygon as IPointCollection).PointCount; i++) {
                IPoint p = (SelectPolygon as IPointCollection).get_Point(i);
                dis.DrawPoint(p);
            }
        }
        public override void OnDblClick() {
            if (lastPoint == null)
                return;
            if (info == null)
                return;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = lastPoint;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor fCursor = (info.Layer as IFeatureLayer).FeatureClass.Search(sf, false);
            selectFeature = fCursor.NextFeature();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
            m_application.MapControl.Refresh();
        }
        IPoint lastPoint = null;
        public override void OnMouseMove(int Button, int Shift, int X, int Y) {
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            lastPoint = p;

            if (fb != null) {
                double mapdis = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(5);
                IGeometry geo = (p as ITopologicalOperator).Buffer(mapdis);
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = geo;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor fCursor = (info.Layer as IFeatureLayer).FeatureClass.Search(sf, true);
                IFeature feature = null;
                while ((feature = fCursor.NextFeature())!=null) {
                    IHitTest hit = feature.ShapeCopy as IHitTest;
                    PointClass hitpoint = new PointClass();
                    double distance = 0;
                    int partIndex = -1;
                    int segIndex = -1;
                    bool r = false;
                    bool isHit = hit.HitTest(p, mapdis, esriGeometryHitPartType.esriGeometryPartBoundary, hitpoint, ref distance, ref partIndex, ref segIndex, ref r);
                    if (isHit) {
                        p = hitpoint;
                        break;
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                fb.MoveTo(p);
            }
        }
        IPolygonMovePointFeedback fb;
        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button == 4)
                return;
            if (selectFeature == null)
                return;
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            double mapdis = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(3);
            if (Button == 1 && Shift == 1) {
                IHitTest hit = selectFeature.ShapeCopy as IHitTest;
                IPoint hitpoint = new PointClass();
                double distance = 0;
                int partIndex = -1;
                int segIndex = -1;
                bool r = false;
                bool isHit = hit.HitTest(p, mapdis, esriGeometryHitPartType.esriGeometryPartBoundary, hitpoint, ref distance, ref partIndex, ref segIndex, ref r);
                if (isHit) {
                    IRing ring = (hit as IGeometryCollection).get_Geometry(partIndex) as IRing;
                    (ring as IPointCollection).InsertPoints(segIndex + 1, 1, ref hitpoint);
                    //(hit as IGeometryCollection).GeometriesChanged();
                    IGeometry geo = ring;
                    (hit as IGeometryCollection).SetGeometries(1, ref geo);
                    selectFeature.Shape = hit as IGeometry;
                    selectFeature.Store();
                    m_application.MapControl.Refresh();
                }
            }//增加节点
            else if (Button == 2) {
                IHitTest hit = selectFeature.ShapeCopy as IHitTest;
                IPoint hitpoint = new PointClass();
                double distance = 0;
                int partIndex = -1;
                int segIndex = -1;
                bool r = false;
                bool isHit = hit.HitTest(p, mapdis, esriGeometryHitPartType.esriGeometryPartVertex, hitpoint, ref distance, ref partIndex, ref segIndex, ref r);
                if (isHit) {
                    IRing ring = (hit as IGeometryCollection).get_Geometry(partIndex) as IRing;
                    (ring as IPointCollection).RemovePoints(segIndex, 1);
                    //(hit as IGeometryCollection).GeometriesChanged();
                    IGeometry geo = ring;
                    (hit as IGeometryCollection).SetGeometries(1, ref geo);
                    selectFeature.Shape = hit as IGeometry;
                    selectFeature.Store();
                    m_application.MapControl.Refresh();
                }
            }//删除节点
            else if (Button == 1 && Shift == 0) {

                IHitTest hit = selectFeature.ShapeCopy as IHitTest;
                IPoint hitpoint = new PointClass();
                double distance = 0;
                int partIndex = -1;
                int segIndex = -1;
                bool r = false;
                bool isHit = hit.HitTest(p, mapdis, esriGeometryHitPartType.esriGeometryPartVertex, hitpoint, ref distance, ref partIndex, ref segIndex, ref r);
                if (isHit) {
                    fb = new PolygonMovePointFeedbackClass();
                    fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                    int index = 0;
                    for (int i = 0; i < partIndex; i++) {
                        IPointCollection pc = (hit as IGeometryCollection).get_Geometry(i) as IPointCollection;
                        index += pc.PointCount;
                    }
                    index += segIndex;
                    fb.Start(hit as IPolygon, index, hitpoint);
                }
            }//移动节点
        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y) {
            if (fb != null) {
                IPolygon poly = fb.Stop();
                fb = null;
                if (poly != null) {
                    selectFeature.Shape = poly;
                    selectFeature.Store();
                    m_application.MapControl.Refresh();
                }
            }
        }
        public override void OnKeyDown(int keyCode, int Shift) {
            switch ((System.Windows.Forms.Keys)keyCode) {
            case System.Windows.Forms.Keys.Escape:
            case System.Windows.Forms.Keys.Space:
                selectFeature = null;
                m_application.MapControl.Refresh();
                break;
            }
        }
    }
}

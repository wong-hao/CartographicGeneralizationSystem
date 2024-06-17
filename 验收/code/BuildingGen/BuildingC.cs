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
    public class BuildingC : BaseGenTool {

        private List<IPolygon> polygons;
        private List<IPolyline> centerLines;
        public BuildingC() {
            base.m_category = "GBuilding";
            base.m_caption = "缝隙等宽化";
            base.m_message = "建筑物间隙";
            base.m_toolTip = "将建筑物间隙进行等宽化处理";
            base.m_name = "BuildingC";
            m_usedParas = new GenDefaultPara[] { 
                new GenDefaultPara("建筑物缝隙宽度",2.0d)
            };
            polygons = new List<IPolygon>();
            centerLines = new List<IPolyline>();
        }
        public override void OnCreate(object hook) {
            //base.OnCreate(hook);
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            ISimpleFillSymbol sfs = new SimpleFillSymbolClass();
            //sfs.Style = esriSimpleFillStyle.esriSFSNull;
            IDisplay display = e.display as IDisplay;
            //display. = m_application.MapControl.Extent;
            ISimpleLineSymbol sls = new SimpleLineSymbolClass();
            sls.Width = 1;
            sls.Style = esriSimpleLineStyle.esriSLSSolid;
            sfs.Outline = sls;
            if (polygons.Count > 0) {
                display.SetSymbol(sfs as ISymbol);
                foreach (var item in polygons) {
                    //display.DrawPolygon(item);
                }
            }
            IRgbColor rgb = new RgbColorClass();
            (rgb).Red = 255;
            (rgb).Green = 0;
            (rgb).Blue = 0;
            sls.Color = rgb;
            if (centerLines.Count > 0) {
                display.SetSymbol(sls as ISymbol);
                foreach (var item in centerLines) {
                    //display.DrawPolyline(item);
                }
            }
        }
        private bool isInit = false;
        public override void OnClick() {
            if (!isInit) {
                isInit = true;
                m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
            }
        }
        public override bool Enabled {
            get {
                return m_application.Workspace !=null;
            }
        }
        void Gen2(IPolygon range) {
            var wo = m_application.SetBusy(true);
            polygons.Clear();
            centerLines.Clear();
            GLayerInfo buildingLayer = GetLayer(m_application.Workspace);
            IFeatureLayer layer = buildingLayer.Layer as IFeatureLayer;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor fCursor = layer.FeatureClass.Search(sf, true);
            IFeature feature = null;
            IPolygon poly = new PolygonClass();

            int groupID = layer.FeatureClass.FindField("G_BuildingGroup");
            ISpatialReferenceFactory srf_ = new SpatialReferenceEnvironmentClass();
            IProjectedCoordinateSystem pcs_ = srf_.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_CM_114E);
            double bufferWidth = (double)m_application.GenPara["建筑物缝隙宽度"];

            wo.SetText("正在进行分析准备");
            wo.Step(8);
            GeometryBagClass gb = new GeometryBagClass();
            GeometryBagClass gb2 = new GeometryBagClass();
            int wCount = layer.FeatureClass.FeatureCount(sf);
            object miss = Type.Missing;
            while ((feature = fCursor.NextFeature()) != null) {
                wo.Step(wCount);
                IGeometry geoCopy = feature.ShapeCopy;
                geoCopy.SpatialReference = pcs_;
                gb.AddGeometry(feature.ShapeCopy, ref miss, ref miss);
                gb2.AddGeometry((geoCopy as ITopologicalOperator).Buffer(bufferWidth * -0.05), ref miss, ref miss);
            }
            wo.Step(8);
            (poly as ITopologicalOperator).ConstructUnion(gb);

            poly.SpatialReference = pcs_;

            wo.Step(8);
            ITopologicalOperator buffer = (poly as ITopologicalOperator).Buffer(bufferWidth) as ITopologicalOperator;
            poly = new PolygonClass();
            poly.SpatialReference = pcs_;
            (poly as ITopologicalOperator).ConstructUnion(gb2);
            poly = buffer.Difference(poly) as IPolygon;
            double width = (poly as IArea).Area / poly.Length * 2;
            width *= 0.4;
            //width *= 0.8;

            wo.Step(8);
            try {

                poly.SpatialReference = pcs_ as IProjectedCoordinateSystem;
                //poly = (poly as ITopologicalOperator).Buffer(-width) as IPolygon;
                //poly = (poly as ITopologicalOperator).Buffer(width) as IPolygon;
                IGeometryCollection gc = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                poly = null;
                for (int i = 0; i < gc.GeometryCount; i++) {
                    IArea ar = gc.get_Geometry(i) as IArea;
                    if (ar.Area > 225) {
                        if (poly == null) {
                            poly = ar as IPolygon;
                        }
                        else {
                            poly = (ar as ITopologicalOperator).Union(poly) as IPolygon;
                        }
                    }
                }
            }
            catch {
            }
            wo.Step(8);
            wo.SetText("正在进行分析。");
            poly.Generalize(width * 0.1);
            BuildingGenCore.CenterLineFactory cf = new BuildingGenCore.CenterLineFactory();
            (poly as ITopologicalOperator).Simplify();
            IGeometryCollection buffBag = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
            for (int i = 0; i < buffBag.GeometryCount; i++) {
                try {
                    poly = buffBag.get_Geometry(i) as IPolygon;

                    BuildingGenCore.CenterLine cl = cf.Create2(poly);
                    PolylineClass resultPolyline = new PolylineClass();
                    resultPolyline.SpatialReference = pcs_;
                    foreach (var info in cl) {
                        if (info.Info.Triangles[info.Info.Triangles.Count - 1].TagValue != 1
                            && info.Info.Triangles[0].TagValue != 1
                            //&& info.Width > bufferWidth / 2
                            )
                            for (int j = 0; j < (info.Line as IGeometryCollection).GeometryCount; j++) {
                                IGeometry geo = (info.Line as IGeometryCollection).get_Geometry(j);
                                resultPolyline.AddGeometries(1, ref geo);
                            }

                    }

                    IPolyline centerLine = resultPolyline;

                    centerLine.Generalize(width * 1.5);
                    //centerLine.Smooth(0);
                    //centerLine = cf.Create(poly);
                    poly = (centerLine as ITopologicalOperator).Buffer(bufferWidth / 2) as IPolygon;
                    (poly as ITopologicalOperator).Simplify();
                    IGeometryCollection gcc = poly as IGeometryCollection;

                    int index = -1;
                    for (int j = 0; j < gcc.GeometryCount; j++) {
                        IRing r = gcc.get_Geometry(j) as IRing;
                        if (r.IsExterior) {
                            index = j;
                            break;
                        }
                    }
                    gcc.RemoveGeometries(index, 1);
                    (poly as ITopologicalOperator).Simplify();

                    polygons.Add(poly);
                    centerLines.Add(centerLine);
                }
                catch {
                }
            }
            wo.Step(8);
            wo.SetText("正在提交结果");
            ISpatialFilter sf_add = new SpatialFilterClass();
            sf_add.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor insertCursor = layer.FeatureClass.Insert(true);
            List<int> addIDs = new List<int>();
            foreach (var item in polygons) {
                IGeometryCollection gcc = (item as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                for (int i = 0; i < gcc.GeometryCount; i++) {
                    IGeometry addPoly = gcc.get_Geometry(i) as IPolygon;
                    (addPoly as ITopologicalOperator).Simplify();
                    sf_add.Geometry = addPoly;
                    IFeatureCursor findCursor = layer.FeatureClass.Search(sf_add, true);
                    IFeature findFeature;
                    int maxID = -1;
                    double maxArea = 0;
                    while ((findFeature = findCursor.NextFeature()) != null) {
                        try {
                            IArea insertPart = findFeature.Shape as IArea;
                            if (insertPart.Area > maxArea) {
                                maxArea = insertPart.Area;
                                maxID = findFeature.OID;
                            }
                        }
                        catch { }
                    }
                    if (maxID == -1)
                        continue;
                    findFeature = layer.FeatureClass.GetFeature(maxID);
                    (findFeature as IFeatureBuffer).Shape = addPoly;
                    if(groupID > 0)
                        (findFeature as IFeatureBuffer).set_Value(groupID, -3);
                    addIDs.Add((int)insertCursor.InsertFeature(findFeature as IFeatureBuffer));
                }
            }
            insertCursor.Flush();
            wo.Step(8);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
            IFeatureCursor updataCursor = layer.FeatureClass.Update(sf, true);
            IFeature updataFeature = null;
            while ((updataFeature = updataCursor.NextFeature())!= null) {
                if (!addIDs.Contains(updataFeature.OID)) {
                    updataCursor.DeleteFeature();
                }                
            }
            updataCursor.Flush();
            wo.Step(8);

            m_application.SetBusy(false);
            m_application.MapControl.Refresh();
        }

        INewPolygonFeedback fb;
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
                m_application.MapControl.Refresh();
            }
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


        internal class TagInfo {
            internal int nodeIndex;
            internal int fid;
            internal int partIndex;
            internal int pointIndex;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen {
    public class BuildingCheck : BaseGenTool {
        GLayerInfo buildingLayer;
        SimpleFillSymbolClass sfs;
        SimpleMarkerSymbolClass sms;
        double minArea;

        public BuildingCheck() {
            base.m_category = "GBuilding";
            base.m_caption = "数据检查";
            base.m_message = "建筑物数据检查";
            base.m_toolTip = "检查并修正建筑物数据。";
            base.m_name = "BuildingCheck";
            minArea = 1;

            buildingLayer = null;
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

        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }
        public override void OnClick() {
            buildingLayer = null;
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.建筑物
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && info.OrgLayer != null
                    ) {
                    buildingLayer = info;
                    break;
                }
            }
            if (buildingLayer == null) {
                System.Windows.Forms.MessageBox.Show("没有找到建筑物图层。");
                return;
            }
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            if (gb != null && gb.Count > 0) {
                if (System.Windows.Forms.MessageBox.Show("已经计算过重叠关系，是否重新计算？", "提示", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) {
                    m_application.MapControl.Refresh();
                    return;
                }
            }
            Analysis();
            m_application.MapControl.Refresh();
        }
        List<Dictionary<int, IFeature>> overlapGroups;
        GeometryBagClass gb;
        GeometryBagClass gbOverlap;
        void Analysis() {
            int complexCount = 0;
            int autoCount = 0;
            int remainCount = 0;
            WaitOperation wo = m_application.SetBusy(true);
            IFeature feature = null;
            IFeatureClass fc = (buildingLayer.Layer as IFeatureLayer).FeatureClass;
            int wCount = fc.FeatureCount(null);
            IFeatureCursor fCursor = fc.Search(null, true);
            List<int> complexFeatures = new List<int>();
            List<int> emptyFeatures = new List<int>();
            wo.SetText("正在检查复杂多边形以及空多边形。");
            while ((feature = fCursor.NextFeature()) != null) {
                wo.Step(wCount);
                wo.SetText("正在检查复杂多边形[" + feature.OID + "]。");
                if (feature.Shape == null || feature.Shape.IsEmpty || (feature.Shape as IArea).Area < minArea) {
                    emptyFeatures.Add(feature.OID);
                    continue;
                }
                if (((feature.Shape as IPolygon4).ConnectedComponentBag as IGeometryCollection).GeometryCount > 1) {
                    complexFeatures.Add(feature.OID);
                }
                else {
                    if (!(feature.Shape as ITopologicalOperator).IsSimple) {
                        ITopologicalOperator sgeo = feature.ShapeCopy as ITopologicalOperator;
                        sgeo.Simplify();
                        feature.Shape = sgeo as IGeometry;
                        feature.Store();
                        complexCount++;
                    }
                }
            }
            foreach (var item in emptyFeatures) {
                complexCount++;
                feature = fc.GetFeature(item);
                feature.Delete();
            }
            foreach (var item in complexFeatures) {
                complexCount++;
                feature = fc.GetFeature(item);
                ComplexFeature(feature, feature.ShapeCopy as IPolygon4, fc);
            }
            fCursor = fc.Search(null, false);
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelEnvelopeIntersects;
            Dictionary<int, int> fid_gid_dic = new Dictionary<int, int>();
            List<Dictionary<int, IFeature>> groups = new List<Dictionary<int, IFeature>>();
            wo.Step(0);
            wo.SetText("正在进行叠置分析。");
            while ((feature = fCursor.NextFeature()) != null) {
                wo.Step(wCount);
                wo.SetText("正在进行叠置分析【" + feature.OID + "】");
                Dictionary<int, IFeature> g = null;
                int groupid = -1;
                if (fid_gid_dic.ContainsKey(feature.OID)) {
                    groupid = fid_gid_dic[feature.OID];
                    g = groups[groupid];
                }
                else {
                    g = new Dictionary<int, IFeature>();
                    g.Add(feature.OID, feature);
                    groupid = groups.Count;
                    groups.Add(g);
                    fid_gid_dic.Add(feature.OID, groupid);
                }

                sf.Geometry = feature.Shape;
                IFeatureCursor overlapCursor = fc.Search(sf, false);
                IFeature overlapFeature = null;
                while ((overlapFeature = overlapCursor.NextFeature()) != null) {
                    if (overlapFeature.OID == feature.OID)
                        continue;

                    if (fid_gid_dic.ContainsKey(overlapFeature.OID)) {
                        int find_groupID = fid_gid_dic[overlapFeature.OID];
                        if (find_groupID != groupid) {
                            IArea a = (feature.Shape as ITopologicalOperator).Intersect(overlapFeature.Shape, esriGeometryDimension.esriGeometry2Dimension) as IArea;
                            if (a != null && a.Area > minArea) {
                                Dictionary<int, IFeature> fg = groups[find_groupID];
                                foreach (var item in fg.Values) {
                                    g.Add(item.OID, item);
                                    fid_gid_dic[item.OID] = groupid;
                                }
                                fg.Clear();
                            }
                        }
                        continue;
                    }
                    IArea area = (feature.Shape as ITopologicalOperator).Intersect(overlapFeature.Shape, esriGeometryDimension.esriGeometry2Dimension) as IArea;
                    if (area != null && area.Area > minArea) {
                        g.Add(overlapFeature.OID, overlapFeature);
                        fid_gid_dic.Add(overlapFeature.OID, groupid);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(overlapCursor);
            }
            wo.SetText("正在进行自动处理");
            overlapGroups = new List<Dictionary<int, IFeature>>();
            foreach (var item in groups) {
                if (item.Count < 2) {
                    continue;
                }
                else if (item.Count == 2) {
                    autoCount++;
                    IFeature[] fe = new IFeature[2];
                    item.Values.CopyTo(fe, 0);
                    IArea a2 = fe[0].Shape as IArea;
                    IArea a3 = fe[1].Shape as IArea;

                    IArea a1 = (fe[0].Shape as ITopologicalOperator).Intersect(fe[1].Shape, esriGeometryDimension.esriGeometry2Dimension) as IArea;

                    //item.Values.CopyTo(fe, 0);
                    if (Math.Abs(a1.Area / a2.Area - 1) < 0.01 && Math.Abs(a1.Area / a3.Area - 1) < 0.01) {
                        fe[1].Delete();
                        continue;
                    }
                }
                overlapGroups.Add(item);
            }

            object miss = Type.Missing;
            gb = new GeometryBagClass();
            foreach (var item in overlapGroups) {
                remainCount++;
                IEnvelope env = null;
                foreach (var fe in item.Values) {
                    if (env == null) {
                        env = fe.Shape.Envelope;
                    }
                    else {
                        env.Union(fe.Shape.Envelope);
                    }
                }
                gb.AddGeometry(env, ref miss, ref miss);
            }
            m_application.SetBusy(false);
            string message = string.Format("共修复复杂多边形{0}个；\n自动修复重叠多边形{1}个，剩余{2}个需要手工修复。", complexCount, autoCount, remainCount);
            System.Windows.Forms.MessageBox.Show(message);
        }

        private void ComplexFeature(IFeature feature, IPolygon4 geo, IFeatureClass fc) {
            IFeatureCursor insertCursor = fc.Insert(true);
            IGeometryCollection gc = (geo.ConnectedComponentBag as IGeometryCollection);
            for (int i = 0; i < gc.GeometryCount; i++) {
                IGeometry g = gc.get_Geometry(i);
                if (g.IsEmpty || (g as IArea).Area < minArea)
                    continue;
                feature.Shape = g;
                insertCursor.InsertFeature(feature as IFeatureBuffer);
            }
            insertCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
            feature.Delete();
        }

        public override bool Deactivate() {
            m_application.MapControl.OnAfterDraw -= new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            return true;
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e) {
            IDisplay dis = e.display as IDisplay;

            if (gb != null) {
                bool drawEnv = dis.DisplayTransformation.FromPoints(15) < 8;
                if (true) {
                    dis.SetSymbol(sfs);
                    for (int i = 0; i < gb.GeometryCount; i++) {
                        IEnvelope env = gb.get_Geometry(i).Envelope;
                        //env.Expand(4, 4, false);
                        dis.DrawRectangle(env);
                    }
                }


                //dis.SetSymbol(sms);
                //for (int i = 0; i < gb.GeometryCount; i++) {
                //    dis.DrawMultipoint(gb.get_Geometry(i));
                //}

            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button == 4)
                return;
            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            for (int i = 0; i < gb.GeometryCount; i++) {
                IEnvelope env = gb.get_Geometry(i) as IEnvelope;
                if ((env as IRelationalOperator).Contains(p)) {
                    SelectForm sform = new SelectForm();
                    foreach (var item in overlapGroups[i].Values) {
                        sform.Features.Add(item);
                    }
                    if (sform.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                        List<IFeature> insertFeatures = new List<IFeature>();
                        for (int j = 0; j < sform.Features.Count; j++) {
                            if (!sform.FeatureIsSelect[j]) {
                                sform.Features[j].Delete();
                            }
                            else {
                                insertFeatures.Add(sform.Features[j]);
                            }
                        }
                        if (insertFeatures.Count >= 2) {
                            IGeometry op = null;
                            foreach (var iFe in insertFeatures) {
                                if (op == null) {
                                    op = iFe.ShapeCopy;
                                    continue;
                                }
                                else {
                                    IGeometry geo = (iFe.Shape as ITopologicalOperator).Difference(op);
                                    op = (iFe.Shape as ITopologicalOperator).Union(op);
                                    if (geo == null || geo.IsEmpty || (geo as IArea).Area < minArea) {
                                        iFe.Delete();
                                    }
                                    else {
                                        ComplexFeature(iFe, geo as IPolygon4, iFe.Table as IFeatureClass);
                                    }
                                }
                            }
                        }
                        gb.RemoveGeometries(i, 1);
                        overlapGroups.RemoveAt(i);
                        m_application.MapControl.Refresh();
                    }
                    break;
                }
            }
        }


        void AutoProcess(int index, bool commit) {

        }
        void ProscessAll(bool commit) {
            if (true) {
                try {
                    for (int i = gb.GeometryCount - 1; i >= 0; i--) {
                        List<IFeature> insertFeatures = new List<IFeature>();
                        foreach (var item in overlapGroups[i].Values) {
                            insertFeatures.Add(item);
                        }

                        if (insertFeatures.Count >= 2) {
                            IGeometry op = null;
                            foreach (var iFe in insertFeatures) {
                                if (op == null) {
                                    op = iFe.ShapeCopy;
                                    continue;
                                }
                                else {
                                    IGeometry geo = (iFe.Shape as ITopologicalOperator).Difference(op);
                                    op = (iFe.Shape as ITopologicalOperator).Union(op);
                                    if (geo == null || geo.IsEmpty || (geo as IArea).Area < minArea) {
                                        iFe.Delete();
                                    }
                                    else {
                                        ComplexFeature(iFe, geo as IPolygon4, iFe.Table as IFeatureClass);
                                    }
                                }
                            }
                        }
                    }
                    gb.RemoveGeometries(0, gb.GeometryCount);
                    overlapGroups.Clear();
                    //m_application.MapControl.Refresh();
                }
                catch {
                }
            }
        }
        int currentErr = 0;
        void Next() {
            if (overlapGroups == null || overlapGroups.Count == 0)
                return;
            currentErr--;
            if (currentErr < 0) {
                currentErr = gb.Count - 1;
            }
            if (currentErr > gb.Count - 1) {
                currentErr = 0;
            }
            IEnvelope env = gb.get_Geometry(currentErr).Envelope;
            env.Expand(8, 8, false);
            m_application.MapControl.Extent = env;
            m_application.MapControl.Refresh();
        }

        public override void OnKeyUp(int keyCode, int Shift) {
            switch (keyCode) {
            case (int)System.Windows.Forms.Keys.Space:
                ProscessAll(true);
                m_application.MapControl.Refresh();
                break;
            case (int)System.Windows.Forms.Keys.Enter:
                //ProscessAll(false);
                //m_application.MapControl.Refresh();
                break;
            case (int)System.Windows.Forms.Keys.R:
                Analysis();
                m_application.MapControl.Refresh();
                break;
            case (int)System.Windows.Forms.Keys.N:
                Next();
                break;
            }
        }
    }
}

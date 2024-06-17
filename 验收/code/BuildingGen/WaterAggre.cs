using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using GENERALIZERLib;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;


namespace BuildingGen {
    public class WaterAggre : BaseGenTool {
        INewPolygonFeedback fb;
        Dictionary<string, string> waterCatagory = new Dictionary<string, string>();
        GLayerInfo info;
        Generalizer gen;
        public WaterAggre() {
            base.m_category = "GWater";
            base.m_caption = "同名河流合并";
            base.m_message = "对选定区域河流进行合并";
            base.m_toolTip = "对选定区域河流进行合并。";
            base.m_name = "WaterAggregate";
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("邻接河流合并的特征值比",(double)40)
            };
            gen = new Generalizer();
            waterCatagory.Add("塘", "624050");
            waterCatagory.Add("河流", "614450");
            waterCatagory.Add("沟渠", "632250");
            waterCatagory.Add("湖泊", "621050");
            waterCatagory.Add("时令湖", "622050");
            waterCatagory.Add("水库", "623050");
            waterCatagory.Add("塘二", "699450");
        }

        public override bool Enabled {
            get {
                return (m_application.Workspace != null);
            }
        }

        public override void OnClick() {
            info = null;
            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers) {
                if (tempInfo.LayerType == GCityLayerType.水系
                    && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && tempInfo.OrgLayer != null
                    ) {
                    info = tempInfo;
                    break;
                }
            }
            if (info == null) {
                System.Windows.Forms.MessageBox.Show("没有找到水系图层");
                return;
            }

            IMap map = m_application.Workspace.Map;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null) {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;

            gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 2000, 10000);

        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y) {
            if (Button == 4)
                return;
            if (info == null)
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
            if (fb == null)
                return;

            IPolygon p = fb.Stop();
            fb = null;
            Gen2(p);

            m_application.MapControl.Refresh();
        }
        Dictionary<int, bool> toUpdate = new Dictionary<int, bool>();
        private void Gen2(IPolygon range)
        {
            toUpdate.Clear();
            IFeatureClass fc = (info.Layer as IFeatureLayer).FeatureClass;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor fcur = fc.Search(sf, false);

            IFeature feat = null;
            while ((feat = fcur.NextFeature()) != null)
            {
                toUpdate.Add(feat.OID, true);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur);
            double ratio = (double)m_application.GenPara["邻接河流合并的特征值比"];
            Dictionary<int, bool> hasDeleted = new Dictionary<int, bool>();
            try
            {
                int nameIndex = fc.FindField("名称");
                int catagoryIndex = fc.FindField("要素代码");
                IFeatureCursor featCursor;
                IFeature touchFeat = null;
                IPolyline touchLine = new PolylineClass();

                WaitOperation wait = m_application.SetBusy(true);
                int o = toUpdate.Count;
                foreach(int ID in toUpdate.Keys)
                {
                    wait.SetText("正在合并河流...");
                    wait.Step(o);
                    if (hasDeleted.ContainsKey(ID))
                    {
                        continue;
                    }
                    IFeature Feat = fc.GetFeature(ID);
                    string catagory = Convert.ToString(Feat.get_Value(catagoryIndex));
                    if (catagory == "699450" || catagory == "624050" || catagory == "621050" || catagory == "622050" || catagory == "623050")
                    {
                        continue;
                    }
                    bool isNotUnioned = true;
                    while (isNotUnioned)
                    {
                        double maxArea = (Feat.Shape as IArea).Area;
                        string maxAreaFeatName = Convert.ToString(Feat.get_Value(nameIndex));

                        ISpatialFilter filter = new SpatialFilterClass();
                        filter.Geometry = Feat.Shape;
                        filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        filter.GeometryField = fc.ShapeFieldName;
                        featCursor = fc.Search(filter, false);

                        ITopologicalOperator oriTopoOper = Feat.ShapeCopy as ITopologicalOperator;
                        oriTopoOper.Simplify();

                        double AreaToLength = getRiverAreaDivideLength(Feat);
                        while ((touchFeat = featCursor.NextFeature()) != null)
                        {
                            if (touchFeat.OID == Feat.OID || !toUpdate.ContainsKey(touchFeat.OID) || Convert.ToString(touchFeat.get_Value(catagoryIndex))!="614450")
                            {
                                continue;
                            }
                            ITopologicalOperator touchTopo = touchFeat.ShapeCopy as ITopologicalOperator;
                            touchTopo.Simplify();
                            double re = Math.Abs((AreaToLength - getRiverAreaDivideLength(touchFeat)) / AreaToLength);
                            //设置阈值
                            if (AreaToLength < 100)
                            {
                                double tpRaio = 0;
                                if (AreaToLength > 25 || getRiverAreaDivideLength(touchFeat) > 25)
                                {
                                    tpRaio = ratio;
                                }
                                else
                                {
                                    tpRaio = 75;
                                }

                                if (re < tpRaio / 100 && IfCanUnion(Feat, touchFeat, maxAreaFeatName, info.Layer as IFeatureLayer))
                                {
                                    oriTopoOper = oriTopoOper.Union(touchTopo as IGeometry) as ITopologicalOperator;
                                    oriTopoOper.Simplify();

                                    string tempName = Convert.ToString(touchFeat.get_Value(nameIndex));
                                    if ((touchFeat.Shape as IArea).Area > maxArea && tempName != "")
                                    {
                                        maxAreaFeatName = tempName;
                                        maxArea = (touchFeat.Shape as IArea).Area;
                                    }

                                    hasDeleted.Add(touchFeat.OID, true);
                                    touchFeat.Delete();
                                    isNotUnioned = false;
                                }
                            }
                            else
                            {
                                if (getRiverAreaDivideLength(touchFeat) > 100 && IfCanUnion(Feat, touchFeat, maxAreaFeatName, info.Layer as IFeatureLayer))
                                {
                                    oriTopoOper = oriTopoOper.Union(touchTopo as IGeometry) as ITopologicalOperator;
                                    oriTopoOper.Simplify();

                                    string tempName = Convert.ToString(touchFeat.get_Value(nameIndex));
                                    if ((touchFeat.Shape as IArea).Area > maxArea && tempName != "")
                                    {
                                        maxAreaFeatName = tempName;
                                        maxArea = (touchFeat.Shape as IArea).Area;
                                    }

                                    hasDeleted.Add(touchFeat.OID, true);
                                    touchFeat.Delete();
                                    isNotUnioned = false;
                                }
                            }

                        }
                        Feat.Shape = oriTopoOper as IGeometry;
                        Feat.set_Value(nameIndex, maxAreaFeatName);
                        Feat.Store();
                        featCursor.Flush();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);
                        isNotUnioned = isNotUnioned ? false : true;
                    }
                }
                m_application.SetBusy(false);
            }
            catch(Exception ex)
            {
                m_application.SetBusy(false);
            }

        }

        Dictionary<int, IFeature> id_ifeature = new Dictionary<int, IFeature>();

        private void Gen(IPolygon range) {
            IFeatureLayer layer = new FeatureLayerClass();
            layer.FeatureClass = (info.Layer as IFeatureLayer).FeatureClass;

            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int allCount = layer.FeatureClass.FeatureCount(sf);
            WaitOperation wo = null;
            //if (allCount > 500)
            //{
            //    wo = m_application.SetBusy(true);
            //}
            IFeatureSelection fselect = layer as IFeatureSelection;

            fselect.SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
            ICursor cr;
            fselect.SelectionSet.Search(null, false, out cr);
            IFeatureCursor c = cr as IFeatureCursor;
            IFeature fe = null;
            id_ifeature.Clear();
            while ((fe = c.NextFeature()) != null)
            {
                id_ifeature.Add(fe.OID, fe);
            }

            try {
                double ratio = (double)m_application.GenPara["邻接河流合并的特征值比"];
                ISelectionSet set = (layer as IFeatureSelection).SelectionSet;

                if (set.Count == 0) {
                    System.Windows.Forms.MessageBox.Show("请选定要素!");
                }
                GLayerManager layerManager = m_application.Workspace.LayerManager;

                Dictionary<int, bool> hasDeleted = new Dictionary<int, bool>();
                Dictionary<int, bool> hasDeleted2 = new Dictionary<int, bool>();
                hasDeleted2 = RiverAggre_Union(layer, ratio, hasDeleted);

            }
            catch (Exception ex) {

            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cr);
        }

        //合并邻接的河流
        private Dictionary<int, bool> RiverAggre_Union(IFeatureLayer layer, double rati, Dictionary<int, bool> hasD)
        {
            Dictionary<int, bool> hasDeleted = new Dictionary<int, bool>();
            try
            {
                IFeatureSelection fs = layer as IFeatureSelection;
                IFeatureClass fc = layer.FeatureClass;
                int nameIndex = fc.FindField("名称");
                int catagoryIndex = fc.FindField("要素代码");
                ISelectionSet set = (layer as IFeatureSelection).SelectionSet;

                IEnumIDs IDs = set.IDs;
                int ID = -2;
                ICursor cursor;
                IFeatureCursor featCursor;
                IFeature touchFeat = null;
                IPolyline touchLine = new PolylineClass();
                Dictionary<int, bool> IfUsedID = new Dictionary<int, bool>();

                WaitOperation wait = m_application.SetBusy(true);
                int o = set.Count;
                while ((ID = IDs.Next()) != -1)
                {
                    wait.SetText("正在合并河流...");
                    wait.Step(o);
                    if (IfUsedID.ContainsKey(ID) || hasD.ContainsKey(ID))
                    {
                        continue;
                    }
                    //IFeature Feat = fc.GetFeature(ID);
                    IFeature Feat = null;
                    id_ifeature.TryGetValue(ID, out Feat);
                    string catagory = Convert.ToString(Feat.get_Value(catagoryIndex));
                    if (catagory == "699450" || catagory == "624050" || catagory == "621050" || catagory == "622050" || catagory == "623050")
                    {
                        continue;
                    }
                    double maxArea = (Feat.Shape as IArea).Area;
                    string maxAreaFeatName = Convert.ToString(Feat.get_Value(nameIndex));

                    ISpatialFilter filter = new SpatialFilterClass();
                    filter.Geometry = Feat.Shape;
                    filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    filter.GeometryField = fc.ShapeFieldName;
                    set.Search(filter, false, out cursor);
                    featCursor = cursor as IFeatureCursor;

                    ITopologicalOperator oriTopoOper = Feat.ShapeCopy as ITopologicalOperator;
                    oriTopoOper.Simplify();

                    double AreaToLength = getRiverAreaDivideLength(Feat);
                    while ((touchFeat = featCursor.NextFeature()) != null)
                    {
                        if (touchFeat.OID == Feat.OID)
                        {
                            continue;
                        }
                        ITopologicalOperator touchTopo = touchFeat.ShapeCopy as ITopologicalOperator;
                        touchTopo.Simplify();
                        double re = Math.Abs((AreaToLength - getRiverAreaDivideLength(touchFeat)) / AreaToLength);
                        //设置阈值
                        if (AreaToLength < 100)
                        {
                            double tpRaio = 0;
                            if (AreaToLength > 25 || getRiverAreaDivideLength(touchFeat) > 25)
                            {
                                tpRaio = rati;
                            }
                            else
                            {
                                tpRaio = 75;
                            }

                            if (re < tpRaio / 100 && IfCanUnion(Feat, touchFeat, maxAreaFeatName, layer))
                            {
                                oriTopoOper = oriTopoOper.Union(touchTopo as IGeometry) as ITopologicalOperator;
                                oriTopoOper.Simplify();

                                string tempName = Convert.ToString(touchFeat.get_Value(nameIndex));
                                if ((touchFeat.Shape as IArea).Area > maxArea && tempName != "")
                                {
                                    maxAreaFeatName = tempName;
                                    maxArea = (touchFeat.Shape as IArea).Area;
                                }

                                hasDeleted.Add(touchFeat.OID, true);
                                IfUsedID.Add(touchFeat.OID, true);
                                touchFeat.Delete();
                            }
                        }
                        else
                        {
                            if (getRiverAreaDivideLength(touchFeat) > 100 && IfCanUnion(Feat, touchFeat, maxAreaFeatName, layer))
                            {
                                oriTopoOper = oriTopoOper.Union(touchTopo as IGeometry) as ITopologicalOperator;
                                oriTopoOper.Simplify();

                                string tempName = Convert.ToString(touchFeat.get_Value(nameIndex));
                                if ((touchFeat.Shape as IArea).Area > maxArea && tempName != "")
                                {
                                    maxAreaFeatName = tempName;
                                    maxArea = (touchFeat.Shape as IArea).Area;
                                }

                                hasDeleted.Add(touchFeat.OID, true);
                                IfUsedID.Add(touchFeat.OID, true);
                                touchFeat.Delete();
                            }
                        }

                    }
                    Feat.Shape = oriTopoOper as IGeometry;
                    Feat.set_Value(nameIndex, maxAreaFeatName);
                    Feat.Store();
                    fs.Add(Feat);

                    cursor.Flush();
                    featCursor.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);

                }
                m_application.SetBusy(false);
            }
            catch
            {
                m_application.SetBusy(false);
            }
            return hasDeleted;
        }

        private double getRiverAreaDivideLength(IFeature Feat) {
            IPolygon polygon = Feat.ShapeCopy as IPolygon;
            IArea areaPolygon = polygon as IArea;
            double area = areaPolygon.Area;
            double length = polygon.Length;
            double ratio = (area * 2) / length;
            return ratio;
        }

        private bool IfCanUnion(IFeature oriFeat, IFeature touchFeat, string maxAreaFeatName, IFeatureLayer layer)
        {
            IFeatureClass fc = layer.FeatureClass;
            int nameIndex = fc.FindField("名称");

            string oriName = Convert.ToString(oriFeat.get_Value(nameIndex));
            string touchName = Convert.ToString(touchFeat.get_Value(nameIndex));

            if (oriName != "" && oriName != " " && touchName != "" && touchName != " " && oriName != touchName)
            { return false; }
            if (maxAreaFeatName != "" && maxAreaFeatName != " " && touchName != "" && touchName != " "&& touchName != maxAreaFeatName)
            { return false; }
            return true;
        }

        public override void OnKeyDown(int keyCode, int Shift) {
            switch (keyCode) {
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null) {
                        fb.Stop();
                        fb = null;
                    }
                    break;
            }
        }
    }

}


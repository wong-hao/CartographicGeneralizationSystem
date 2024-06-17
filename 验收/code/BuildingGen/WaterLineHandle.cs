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
using GENERALIZERLib;

namespace BuildingGen
{
    public class WaterLineHandle : BaseGenTool
    {
        Generalizer gen;
        private List<IPolygon> polygons;
        private List<IPolyline> centerLines;
        public WaterLineHandle()
        {
            base.m_category = "GWater";
            base.m_caption = "湖泊毗邻";
            base.m_message = "湖泊毗邻";
            base.m_toolTip = "湖泊毗邻";
            base.m_name = "WaterLineHandle";
            base.m_usedParas = new GenDefaultPara[] 
            { 
                new GenDefaultPara("湖泊缝隙宽度",(double)10)

            };
            polygons = new List<IPolygon>();
            centerLines = new List<IPolyline>();
            gen = new Generalizer();
        }
        public override void OnCreate(object hook)
        {
            
            //m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e)
        {
            ISimpleFillSymbol sfs = new SimpleFillSymbolClass();
            //sfs.Style = esriSimpleFillStyle.esriSFSNull;
            IDisplay display = e.display as IDisplay;
            //display. = m_application.MapControl.Extent;
            ISimpleLineSymbol sls = new SimpleLineSymbolClass();
            sls.Width = 2;
            sls.Style = esriSimpleLineStyle.esriSLSSolid;
            sfs.Outline = sls;
            if (polygons.Count > 0)
            {
                display.SetSymbol(sfs as ISymbol);
                foreach (var item in polygons)
                {
                    //display.DrawPolygon(item);
                }
            }
            IRgbColor rgb = new RgbColorClass();
            (rgb).Red = 63;
            (rgb).Green = 100;
            (rgb).Blue = 235;
            sls.Color = rgb;
            if (centerLines.Count > 0)
            {
                display.SetSymbol(sls as ISymbol);
                foreach (var item in centerLines)
                {
                    //display.DrawPolyline(item);
                }
            }
        }
        private bool isInit = false;
        public override void OnClick()
        {
            if (!isInit)
            {
                isInit = true;
                //m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
            }
            GLayerInfo buildingLayer = GetLayer(m_application.Workspace);
            IFeatureLayer layer = buildingLayer.Layer as IFeatureLayer;
            IFeatureClass fc = layer.FeatureClass;
            //IFeatureClass waterLine = null;
            //IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            //IWorkspace2 WS2 = FeatWS as IWorkspace2;
            //if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "池塘分界线"))
            //{
            //    centerLines.Clear();
            //    waterLine = FeatWS.OpenFeatureClass("池塘分界线");
            //    //IFeatureCursor deleteCur = waterLine.Update(null, false);
            //    //IFeature deleteF = null;
            //    //while ((deleteF = deleteCur.NextFeature()) != null)
            //    //{
            //    //    centerLines.Add(deleteF.Shape as IPolyline);
            //    //}
            //    //System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteCur);

            //    int _GenUsed2 = -1;
            //    IFeatureLayer ditchLayer = new FeatureLayerClass();
            //    _GenUsed2 = waterLine.FindField("G_BuildingGroup");
            //    ditchLayer.Name = "池塘分界线";
            //    ditchLayer.FeatureClass = waterLine;
            //    ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
            //    IUniqueValueRenderer render = new UniqueValueRendererClass();
            //    lineSymbol.Width = 1;
            //    IHsvColor hsvColor4 = new HsvColorClass();
            //    hsvColor4.Hue = 206;
            //    hsvColor4.Saturation = 96;
            //    hsvColor4.Value = 99;
            //    lineSymbol.Color = hsvColor4;
            //    render.DefaultSymbol = lineSymbol as ISymbol;
            //    render.UseDefaultSymbol = true;
            //    render.FieldCount = 1;
            //    render.set_Field(0, "G_BuildingGroup");
            //    render.set_FieldType(0, false);

            //    lineSymbol = new SimpleLineSymbolClass();
            //    lineSymbol.Width = 1;
            //    IHsvColor hsvColor6 = new HsvColorClass();
            //    hsvColor6.Hue = 60;
            //    hsvColor6.Saturation = 100;
            //    hsvColor6.Value = 100;
            //    lineSymbol.Color = hsvColor6;
            //    render.AddValue("", "G_BuildingGroup", lineSymbol as ISymbol);

            //    (ditchLayer as IGeoFeatureLayer).DisplayField = "G_BuildingGroup";
            //    (ditchLayer as IGeoFeatureLayer).Renderer = render as IFeatureRenderer;
            //    for (int m = 0; m < m_application.MapControl.LayerCount; m++)
            //    {
            //        if (m_application.MapControl.get_Layer(m).Name == "池塘分界线")
            //        {
            //            m_application.MapControl.DeleteLayer(m);
            //        }
            //    }
            //    m_application.MapControl.AddLayer(ditchLayer, 0);
            //}
            m_application.MapControl.Refresh();
            gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 2000, 10000);
            
        }
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null;
            }
        }
        void Gen2(IPolygon range)
        {
            //gen.InitGeneralizer(m_application.ExePath + "\\GenPara.inf", 2000, 10000);
            var wo = m_application.SetBusy(true);
            polygons.Clear();
            GLayerInfo buildingLayer = GetLayer(m_application.Workspace);
            IFeatureLayer layer = buildingLayer.Layer as IFeatureLayer;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.WhereClause = "要素代码='624050'";
            IFeatureCursor fCursor = layer.FeatureClass.Search(sf, true);
            IFeature feature = null;
            IPolygon poly = new PolygonClass();

            IFeatureClass fc = layer.FeatureClass;
            //IFeatureClass waterLine = null;
            //IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            //IWorkspace2 WS2 = FeatWS as IWorkspace2;
            //if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "池塘分界线"))
            //{
            //    waterLine = FeatWS.OpenFeatureClass("池塘分界线");
            //    ISpatialFilter sfff = new SpatialFilterClass();
            //    sfff.Geometry = range;
            //    sfff.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            //    IFeatureCursor deleteCur = waterLine.Update(sfff, false);
            //    IFeature deleteF = null;
            //    while ((deleteF = deleteCur.NextFeature()) != null)
            //    {
            //        deleteF.Delete();
            //    }
            //    System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteCur);

            //}
            //else
            //{
            //    ESRI.ArcGIS.esriSystem.IClone clone = (fc.Fields as ESRI.ArcGIS.esriSystem.IClone).Clone();
            //    IFields sourceFileds = clone as IFields;
            //    IField shpField = sourceFileds.get_Field(sourceFileds.FindField("Shape"));
            //    IGeometryDef geometryDef = shpField.GeometryDef;
            //    IGeometryDefEdit geoDefEdit = geometryDef as IGeometryDefEdit;
            //    geoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
            //    waterLine = FeatWS.CreateFeatureClass("池塘分界线", sourceFileds, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            //}
            //IFeatureCursor forInsertCur = waterLine.Insert(false);


            int groupID = layer.FeatureClass.FindField("G_BuildingGroup");
            int YSDMid = layer.FeatureClass.FindField("要素代码");
            ISpatialReferenceFactory srf_ = new SpatialReferenceEnvironmentClass();
            IProjectedCoordinateSystem pcs_ = srf_.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_CM_114E);
            double bufferWidth = (double)m_application.GenPara["湖泊缝隙宽度"];

            wo.SetText("正在进行分析准备");
            wo.Step(8);
            GeometryBagClass gb = new GeometryBagClass();
            GeometryBagClass gb2 = new GeometryBagClass();
            int wCount = layer.FeatureClass.FeatureCount(sf);
            object miss = Type.Missing;
            while ((feature = fCursor.NextFeature()) != null)
            {
                wo.Step(wCount);
                if (Convert.ToString(feature.get_Value(YSDMid)) != "624050")
                {
                    continue;
                }
                IGeometry geoCopy = feature.ShapeCopy;
                geoCopy.SpatialReference = pcs_;
                gb.AddGeometry(feature.ShapeCopy, ref miss, ref miss);
                gb2.AddGeometry((geoCopy as ITopologicalOperator).Buffer(bufferWidth * -0.05), ref miss, ref miss);
            }
            wo.Step(8);
            (poly as ITopologicalOperator).ConstructUnion(gb);
            poly.SpatialReference = pcs_;

            wo.Step(8);
            ITopologicalOperator buffer = (poly as ITopologicalOperator).Buffer(bufferWidth/2) as ITopologicalOperator;
            poly = new PolygonClass();
            poly.SpatialReference = pcs_;
            (poly as ITopologicalOperator).ConstructUnion(gb2);
            poly = buffer.Difference(poly) as IPolygon;
            double width = (poly as IArea).Area / poly.Length * 2;
            width *= 0.4;

            wo.Step(8);
            try
            {

                poly.SpatialReference = pcs_ as IProjectedCoordinateSystem;
                IGeometryCollection gc = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                //poly = null;
                //for (int i = 0; i < gc.GeometryCount; i++)
                //{
                //    IArea ar = gc.get_Geometry(i) as IArea;
                //    if (ar.Area > 225)
                //    {
                //        if (poly == null)
                //        {
                //            poly = ar as IPolygon;
                //        }
                //        else
                //        {
                //            poly = (ar as ITopologicalOperator).Union(poly) as IPolygon;
                //        }
                //    }
                //}
            }
            catch
            {
            }

            wo.Step(0);
            if (poly == null)
            {
                m_application.SetBusy(false);
                //forInsertCur.Flush();
                return;
            }
            poly.Generalize(width * 0.1);
            BuildingGenCore.CenterLineFactory cf = new BuildingGenCore.CenterLineFactory();
            (poly as ITopologicalOperator).Simplify();
            IGeometryCollection buffBag = (poly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
            for (int i = 0; i < buffBag.GeometryCount; i++)
            {
                wo.SetText("正在进行分析。");
                wo.Step(buffBag.GeometryCount);
                try
                {
                    poly = buffBag.get_Geometry(i) as IPolygon;
                    PolygonClass polyDeleteSmallRing = new PolygonClass();
                    for (int j = 0; j < (poly as IGeometryCollection).GeometryCount; j++)
                    {
                        IRing smallRing = (poly as IGeometryCollection).get_Geometry(j) as IRing;
                        if (Math.Abs((smallRing as IArea).Area) > (double)m_application.GenPara["水系最小上图面积"] * 0.6)
                        {
                            polyDeleteSmallRing.AddGeometry(smallRing, ref miss, ref miss);
                        }
                    }
                    polyDeleteSmallRing.Simplify();
                    if (!polyDeleteSmallRing.IsEmpty)
                    {
                        poly = polyDeleteSmallRing;
                    }
                    BuildingGenCore.CenterLine cl = cf.Create2(poly);
                    PolylineClass resultPolyline = new PolylineClass();
                    resultPolyline.SpatialReference = pcs_;
                    IPolyline simplifyLine = new PolylineClass();
                    foreach (var info in cl)
                    {
                        if (info.Info.Triangles[info.Info.Triangles.Count - 1].TagValue != 1
                            && info.Info.Triangles[0].TagValue != 1
                            //&& info.Width > bufferWidth / 2
                            )
                            simplifyLine = gen.SimplifyPolylineByDT2(info.Line, 8) as IPolyline;//
                            for(int j=0;j<(simplifyLine as IGeometryCollection).GeometryCount;j++)
                            {
                                IGeometry geo = (simplifyLine as IGeometryCollection).get_Geometry(j);
                                IPolyline newLine = new PolylineClass();
                                IGeometryCollection newLineColl = newLine as IGeometryCollection;
                                newLineColl.AddGeometries(1, ref geo);
                                //IFeatureBuffer forInsertBuffer = waterLine.CreateFeatureBuffer();
                                //forInsertBuffer.Shape = newLine;
                                //forInsertCur.InsertFeature(forInsertBuffer);
                                resultPolyline.AddGeometries(1, ref geo);
                                centerLines.Add(newLine);
                            }

                    }
                    IPolyline centerLine = resultPolyline;

                    poly = (centerLine as ITopologicalOperator).Buffer(0.001) as IPolygon;
                    (poly as ITopologicalOperator).Simplify();
                    IGeometryCollection gcc = poly as IGeometryCollection;

                    int index = -1;
                    for (int j = 0; j < gcc.GeometryCount; j++)
                    {
                        IRing r = gcc.get_Geometry(j) as IRing;
                        if (r.IsExterior)
                        {
                            index = j;
                            break;
                        }
                    }
                    gcc.RemoveGeometries(index, 1);
                    
                    (poly as ITopologicalOperator).Simplify();

                    polygons.Add(poly);
                    //centerLines.Add(centerLine);
                }
                catch(Exception ex)
                {
                }
            }
            //List<IPolygon> tooMinAreaHole = new List<IPolygon>();
            wo.Step(8);
            wo.SetText("正在提交结果");
            ISpatialFilter sf_add = new SpatialFilterClass();
            sf_add.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf_add.WhereClause=  "要素代码='624050'";
            IFeatureCursor insertCursor = layer.FeatureClass.Insert(true);
            List<int> addIDs = new List<int>();
            foreach (var item in polygons)
            {
                IGeometryCollection gcc = (item as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                for (int i = 0; i < gcc.GeometryCount; i++)
                {
                    IGeometry addPoly = gcc.get_Geometry(i) as IPolygon;
                    (addPoly as ITopologicalOperator).Simplify();
                    sf_add.Geometry = addPoly;
                    IFeatureCursor findCursor = layer.FeatureClass.Search(sf_add, true);
                    IFeature findFeature;
                    int maxID = -1;
                    double maxArea = 0;
                    while ((findFeature = findCursor.NextFeature()) != null)
                    {
                        try
                        {
                            if (addIDs.Contains(findFeature.OID))
                            {
                                continue;
                            }
                            IArea insertPart = findFeature.Shape as IArea;
                            if (insertPart.Area > maxArea)
                            {
                                maxArea = insertPart.Area;
                                maxID = findFeature.OID;
                            }
                        }
                        catch { }
                    }
                    if (maxID == -1 && maxArea < (double)m_application.GenPara["水系最小上图面积"] * 0.6)
                    {
                        continue;
                    }
                    findFeature = layer.FeatureClass.GetFeature(maxID);
                    (findFeature as IFeatureBuffer).Shape = addPoly;
                    if (groupID > 0)
                        (findFeature as IFeatureBuffer).set_Value(groupID, -3);
                    addIDs.Add((int)insertCursor.InsertFeature(findFeature as IFeatureBuffer));
                }
            }
            insertCursor.Flush();
            wo.Step(8);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
            IFeatureCursor updataCursor = layer.FeatureClass.Update(sf, true);
            IFeature updataFeature = null;
            while ((updataFeature = updataCursor.NextFeature()) != null)
            {
                if (!addIDs.Contains(updataFeature.OID))
                {
                    updataCursor.DeleteFeature();
                }
            }
            updataCursor.Flush();
            wo.Step(8);

            //IFeatureLayer insertLayer = new FeatureLayerClass();
            //insertLayer.FeatureClass = waterLine;
            //insertLayer.Name = "池塘分界线";
            //bool isExistedLayer = false;
            //for (int m = 0; m < m_application.MapControl.LayerCount; m++)
            //{
            //    if (m_application.MapControl.get_Layer(m).Name == "池塘分界线")
            //    {
            //        isExistedLayer = true;
            //    }
            //}
            //if (!isExistedLayer)
            //{
            //    m_application.MapControl.AddLayer(insertLayer, 0);
            //}
            m_application.SetBusy(false);
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(forInsertCur);
            m_application.MapControl.Refresh();
        }

        public override void OnKeyUp(int keyCode, int Shift)
        {

            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.D:
                    GLayerInfo buildingLayer = GetLayer(m_application.Workspace);
                    IFeatureLayer layer = buildingLayer.Layer as IFeatureLayer;
                    IFeatureClass fc = layer.FeatureClass;
                    IFeatureClass waterLine = null;
                    IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
                    IWorkspace2 WS2 = FeatWS as IWorkspace2;
                    if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "池塘分界线"))
                    {
                        waterLine = FeatWS.OpenFeatureClass("池塘分界线");
                        IFeatureCursor deleteCur = waterLine.Update(null, false);
                        IFeature deleteF = null;
                        while ((deleteF = deleteCur.NextFeature()) != null)
                        {
                            deleteF.Delete();
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteCur);
                    }
                    centerLines.Clear();
                    m_application.MapControl.Refresh();
                    break;
            }
        }

        INewPolygonFeedback fb;
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button != 1)
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
            if (fb != null)
            {
                IPolygon poly = fb.Stop();
                fb = null;
                if (poly == null || poly.IsEmpty)
                {
                    return;
                }
                Gen2(poly);
                m_application.MapControl.Refresh();
            }
        }

        public override void OnKeyDown(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null)
                    {
                        fb.Stop();
                        fb = null;
                    }
                    break;
            }
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


        internal class TagInfo
        {
            internal int nodeIndex;
            internal int fid;
            internal int partIndex;
            internal int pointIndex;
        }
    }
}

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


namespace BuildingGen
{
    public class NodeEditor : BaseGenTool
    {//伪节点删不掉
        IGeometry currentEditGeo;
        IPointCollection tpPointcollection;
        INewLineFeedback lineFB1;
        INewLineFeedback lineFB2;
        IPolyline line1;
        IPolyline line2;
        bool isEdit=false;//to do edit，是否处于节点选中并拖移过程中
        IPoint lastPoint;
        IPoint movePoint;
        GLayerInfo info;
        public NodeEditor()
        {
            base.m_category = "GBuilding";
            base.m_caption = "编辑节点";
            base.m_message = "对选定要素进行编辑";
            base.m_toolTip = "对选定要素进行编辑。\n鼠标移至节点处，右键删除；\n按住shift键左键点击加入节点；\n空格键保存。";
            base.m_name = "nodeeditor";
        }

        GCityLayerType currentLayerType;
        public NodeEditor(GCityLayerType layerType)
        {
            base.m_category = "GBuilding";
            base.m_caption = "编辑节点";
            base.m_message = "对选定要素进行编辑";
            base.m_toolTip = "对选定要素进行编辑。\n鼠标移至节点处，右键删除；\n按住shift键左键点击加入节点；\n空格键保存。";
            base.m_name = "nodeeditor";

            currentLayerType = layerType;
        }


        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null);
            }
        }

        public override void OnClick()
        {
            info = null;
            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers)
            {
                if (tempInfo.LayerType == currentLayerType
                    && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && tempInfo.OrgLayer != null
                    )
                {
                    info = tempInfo;
                    break;
                }
            }
            if (info == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到图层");
                return;
            }

            IMap map = m_application.Workspace.Map;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null)
            {
                return;
            }
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            IFeatureClass fc = layer.FeatureClass;        

        }

        bool notToPan = false;//控制不吸附
        public override void OnKeyDown(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    shape.SetEmpty();
                    line1 = null;
                    line2 = null;
                    toRemovePoint = null;
                    isInMode = false;
                    (pointcollection as IGeometry).SetEmpty();
                    lastPoint = null;
                    toDoDelete = false;
                    deleteSeg = null;
                    if (lineFB1 != null)
                    {
                        lineFB1.Stop();
                        lineFB1 = null;
                    }
                    if (lineFB2 != null)
                    {
                        lineFB2.Stop();
                        lineFB2 = null;
                    }
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.Space:
                    if (!shape.IsEmpty && !isEdit && isInMode)//shift用来控制是否加入节点
                    {
                        editFeat.Shape = shape;
                        editFeat.Store(); 
                        shape.SetEmpty();
                        line1 = null;
                        line2 = null;
                        toRemovePoint = null;
                        isInMode = false;
                        (pointcollection as IGeometry).SetEmpty();
                        lastPoint = null;
                        toDoDelete = false;
                        m_application.MapControl.Refresh();

                    }
                    break;
                case (int)System.Windows.Forms.Keys.P:
                    if(notToPan)
                    {
                        notToPan = false;
                    }
                    else
                    {
                        notToPan = true;
                    }
                    break;
            }
        }

        IGeometry shape = new PolygonClass();
        IPoint toRemovePoint = new PointClass();
        bool isdone = true;//判断是否已经完成编辑存储
        bool toDoDelete = false;//是否是删除节点操作
        ISegment deleteSeg = null;//删除操作时Reshape用的线段
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {         
            if (Button == 4)
                return;
            if (info == null)
                return;

            if (lastPoint != null && tpPointcollection != null)
            {
                IFeatureLayer layer = info.Layer as IFeatureLayer;
                IFeatureClass fc = layer.FeatureClass;
                IPoint downPoint = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

                double distance = 0;
                int hitPartIndex = 0;
                int hitSegmentIndex = 0;
                bool isOnRightSide = false;
                bool isHit = false;
                IPoint hitPoint = new PointClass();
                IHitTest hitTest = currentEditGeo as IHitTest;
                isHit = hitTest.HitTest(downPoint, 4, esriGeometryHitPartType.esriGeometryPartVertex, hitPoint, ref distance, ref hitPartIndex, ref hitSegmentIndex, ref isOnRightSide);

                if (hitPoint != null && !hitPoint.IsEmpty)
                {
                    isEdit = true;
                }
                ISegmentCollection currentSegs = currentEditGeo as ISegmentCollection;
                ISegment tpSeg;
                IPoint refPoint1=null;
                IPoint refPoint2=null;
                for (int m = 0; m < currentSegs.SegmentCount; m++)
                {
                    if (!isEdit)
                    {
                        break;
                    }
                    if (refPoint1 != null && refPoint2 != null)
                    {
                        break;
                    }
                    tpSeg = currentSegs.get_Segment(m);
                    if (tpSeg.FromPoint.X == hitPoint.X && tpSeg.FromPoint.Y == hitPoint.Y)
                    {
                        if (refPoint1 == null)
                        {
                            refPoint1 = tpSeg.ToPoint;
                        }
                        else
                        {
                            refPoint2 = tpSeg.ToPoint;
                        }
                    }
                    if (tpSeg.ToPoint.X == hitPoint.X && tpSeg.ToPoint.Y == hitPoint.Y)
                    {
                        if (refPoint1 == null)
                        {
                            refPoint1 = tpSeg.FromPoint;
                        }
                        else
                        {
                            refPoint2 = tpSeg.FromPoint;
                        }
                    }
                }
                deleteSeg = new LineClass();
                deleteSeg.FromPoint = refPoint1;
                deleteSeg.ToPoint = refPoint2;
                if (isEdit)
                {
                    toRemovePoint = hitPoint;

                    if (Button == 2) //TO DO DELETE
                    {
                        toDoDelete = true;
                        //return;
                    }
                    if (Button == 1)
                    {
                        toDoDelete = false;
                    }

                    if (Shift != 1)
                    {
                        lineFB1 = new NewLineFeedbackClass();
                        lineFB1.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                        lineFB2 = new NewLineFeedbackClass();
                        lineFB2.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                        lineFB1.Start(refPoint1);
                        lineFB2.Start(refPoint2);
                    }
                }

                if (!shape.IsEmpty && !isEdit && isInMode && Shift != 1)//shift用来控制是否加入节点
                {
                    //editFeat.Shape = shape;
                    //if (Shift == 2)  //按住ctrl键同时点击空白处保存
                    //{
                    //    editFeat.Store();
                    //}
                    shape.SetEmpty();
                    line1 = null;
                    line2 = null;
                    toRemovePoint = null;
                    isInMode = false;
                    (pointcollection as IGeometry).SetEmpty();
                    lastPoint = null;
                    toDoDelete = false;
                    m_application.MapControl.Refresh();

                }

            }
        }
        private void Gen(IPolyline range)
        {
            try
            {
                IPolygon4 ringsPoly = currentEditGeo as IPolygon4;

                IGeometryBag exteriorRings = ringsPoly.ExteriorRingBag;
                IEnumGeometry exteriorEnum = exteriorRings as IEnumGeometry;
                exteriorEnum.Reset();
                IRing currentExterRing = exteriorEnum.Next() as IRing;
                IRing tpInteriorRing = new RingClass();
                IPolygon4 RingPolygon = new PolygonClass();
                IGeometryCollection ringsCollection = RingPolygon as IGeometryCollection;
                IGeometry forAddGeometry = currentExterRing as IGeometry;
                ringsCollection.AddGeometries(1, ref forAddGeometry);
                while (currentExterRing != null)
                {
                    IGeometryBag interiorRings = ringsPoly.get_InteriorRingBag(currentExterRing);
                    IEnumGeometry interiorEnum = interiorRings as IEnumGeometry;
                    tpInteriorRing = interiorEnum.Next() as IRing;
                    while (tpInteriorRing != null)
                    {

                        forAddGeometry = tpInteriorRing as IGeometry;
                        ringsCollection.AddGeometries(1, ref forAddGeometry);
                        tpInteriorRing = interiorEnum.Next() as IRing;
                    }

                    currentExterRing = exteriorEnum.Next() as IRing;
                }
                IGeometryCollection c = range as IGeometryCollection;
                IPath currentPath = (c.get_Geometry(0)) as IPath;
                for (int i = 0; i < ringsCollection.GeometryCount; i++)
                {
                    IRing ring = ringsCollection.get_Geometry(i) as IRing;
                    ring.Reshape(currentPath);
                }
                shape = ringsPoly as IGeometry;
                m_application.MapControl.Refresh();
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(cur);
            }
            catch (Exception ex)
            {

            }
        }

        //IPoint finalPoint = new PointClass();
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            object Missing = Type.Missing;
            IPoint upPoint = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (Shift == 1 && isInMode)//按住shift键加入节点
            {
                //IPolyline oriPolyline = currentEditGeo as IPolyline;
                IPolycurve oriPolycurve = currentEditGeo as IPolycurve;
                bool splitHappened = false;
                int newPartIndex = -1;
                int newSegmentIndex = -1;
                oriPolycurve.SplitAtPoint(upPoint, true, false, out splitHappened, out newPartIndex, out newSegmentIndex);
                shape = oriPolycurve as IGeometry;

                isEdit = false;
                m_application.MapControl.Refresh();
            }

            if (isEdit == true && toDoDelete == false)
            {
                if (isGet && !notToPan)
                {
                    lineFB1.AddPoint(getPoint);
                    lineFB2.AddPoint(getPoint);
                }
                else
                {
                    lineFB1.AddPoint(upPoint);
                    lineFB2.AddPoint(upPoint);
                }
                line1 = lineFB1.Stop();
                line2 = lineFB2.Stop();

                //finalPoint = upPoint;

                lineFB1 = null;
                lineFB2 = null;
                if (line1 == null || line1.IsEmpty||line2.IsEmpty||line2==null)
                {
                    return;
                }

                isEdit = false;
                m_application.MapControl.Refresh();
            }

            if (toDoDelete == true)
            {
                ISegmentCollection segC = new PolylineClass();
                segC.AddSegments(1, ref deleteSeg);
                Gen(segC as IPolyline);
                isEdit = false;
            }

            if (line1 != null && line2 != null)
            {            
                if (toDoDelete == false)
                {
                    ISegmentCollection pre_segC = new PolylineClass();
                    pre_segC.AddSegments(1, ref deleteSeg);
                    Gen(pre_segC as IPolyline);

                    ISegmentCollection segC1 = line1 as ISegmentCollection;
                    ISegment seg1 = segC1.get_Segment(0);
                    ISegmentCollection segC2 = line2 as ISegmentCollection;
                    ISegment seg2 = segC2.get_Segment(0);
                    ISegmentCollection segC = new PolylineClass();
                    segC.AddSegments(1, ref seg1);
                    segC.AddSegments(1, ref seg2);
                    Gen(segC as IPolyline);
                }
            }

            
        }

        IEnvelope toGetEnv = new EnvelopeClass();
        bool isGet = false;//判断是否吸附
        IPoint getPoint = new PointClass();
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
            {
                return;
            }
            movePoint = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (isEdit == true)
            {
                if (!notToPan)
                {
                    isGet = false;                  
                    toGetEnv.XMin = movePoint.X - 2;
                    toGetEnv.XMax = movePoint.X + 2;
                    toGetEnv.YMin = movePoint.Y - 2;
                    toGetEnv.YMax = movePoint.Y + 2;
                    toGetEnv.CenterAt(movePoint);
                    IFeatureClass fc = (info.Layer as IFeatureLayer).FeatureClass;
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = toGetEnv as IGeometry;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeatureCursor fCursor = fc.Search(sf, false);
                    IFeature feat = null;
                    IPolygon po = null;
                    List<IFeature> getFeats = new List<IFeature>();
                    while ((feat = fCursor.NextFeature()) != null)
                    {
                        getFeats.Add(feat);
                        if (feat.OID == editFeat.OID)
                        {
                            continue;
                        }
                        po = feat.Shape as IPolygon;
                        double distance = 0;
                        int hitPartIndex = 0;
                        int hitSegmentIndex = 0;
                        bool isOnRightSide = false;
                        IPoint hitPoint = new PointClass();
                        IHitTest hitTest = po as IHitTest;
                        isGet = hitTest.HitTest(movePoint, 2.5, esriGeometryHitPartType.esriGeometryPartVertex, getPoint, ref distance, ref hitPartIndex, ref hitSegmentIndex, ref isOnRightSide);
                        if (isGet)
                        {
                            break;
                        }
                    }
                    if (!isGet)
                    {
                        IFeature[] featsA = getFeats.ToArray();
                        IFeature feat2 = null;
                        for (int j = 0; j < featsA.Length; j++)
                        {
                            feat2 = featsA[j];
                            if (feat2.OID == editFeat.OID)
                            {
                                continue;
                            }
                            po = feat2.Shape as IPolygon;
                            double distance = 0;
                            int hitPartIndex = 0;
                            int hitSegmentIndex = 0;
                            bool isOnRightSide = false;
                            IPoint hitPoint = new PointClass();
                            IHitTest hitTest = po as IHitTest;
                            isGet = hitTest.HitTest(movePoint, 1.5, esriGeometryHitPartType.esriGeometryPartBoundary, getPoint, ref distance, ref hitPartIndex, ref hitSegmentIndex, ref isOnRightSide);
                            if (isGet)
                            {
                                break;
                            }
                        }
                    }
                    getFeats.Clear();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                }

                if (isGet && !notToPan)
                {
                    lineFB1.MoveTo(getPoint);
                    lineFB2.MoveTo(getPoint);
                }
                else
                {
                    lineFB1.MoveTo(movePoint);
                    lineFB2.MoveTo(movePoint);
                }
            }
        }

        public override bool Deactivate()
        {
            m_application.MapControl.OnAfterDraw -= new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            return true;
        }

        IFeature editFeat = null;
        bool isInMode = false;//判断是否处于选中要素状态
        IGeometryCollection pointcollection = new MultipointClass();
        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e)
        {
            ISimpleLineSymbol sls = new SimpleLineSymbolClass();
            sls.Width = 2;
            sls.Style = esriSimpleLineStyle.esriSLSSolid;

            IRgbColor rgb = new RgbColorClass();
            (rgb).Red = 255;
            (rgb).Green = 0;
            (rgb).Blue = 0;
            sls.Color = rgb;
            IScreenDisplay screenDisplay = e.display as IScreenDisplay;

            if (lastPoint != null)
            {
                IFeatureLayer layer = info.Layer as IFeatureLayer;
                IFeatureClass fc = layer.FeatureClass;

                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = lastPoint;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                    IFeatureCursor FeatUpdateCursor = fc.Search(sf, false);
                    IFeature feature_DLTB = null;

                    IPointCollection ptcollection;
                    object o_missing = Type.Missing;

                    while ((feature_DLTB = FeatUpdateCursor.NextFeature()) != null)
                    {
                        isInMode = true;
                        if (shape.IsEmpty)
                        {
                            shape = feature_DLTB.Shape as IPolygon;
                        }
                        ptcollection = shape as IPointCollection; //
                        (pointcollection as IGeometry).SetEmpty();
                        (pointcollection as IPointCollection).AddPointCollection(ptcollection);
                        tpPointcollection = ptcollection;//edit tool
                        currentEditGeo = shape;

                        editFeat = feature_DLTB;
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(FeatUpdateCursor);
                
   

                ISymbol vertexSymbol = (ISymbol)CreateCharacterMarkerSymbol(47, CreateRGBColor(10, 10, 10, false), 10);
                if (isGet && !notToPan)
                {
                    vertexSymbol = (ISymbol)CreateCharacterMarkerSymbol(47, CreateRGBColor(255, 10, 10, false), 10);
                }
                screenDisplay.SetSymbol(vertexSymbol);
                screenDisplay.DrawMultipoint(pointcollection as IGeometry);
            }


            if (isEdit==false&&line1 != null && line2 != null)
            {


                screenDisplay.SetSymbol(sls as ISymbol);

                screenDisplay.DrawPolyline(line1);
                screenDisplay.DrawPolyline(line2);
            }

            ISimpleFillSymbol sfs = new SimpleFillSymbolClass();
            sfs.Color = rgb;
            sfs.Outline = sls as ILineSymbol;
            sfs.Style = esriSimpleFillStyle.esriSFSNull;
            if (!shape.IsEmpty)
            {

                screenDisplay.SetSymbol(sfs as ISymbol);

                screenDisplay.DrawPolygon(shape);
            }

        }

        public override void OnDblClick()
        {
            if (movePoint == null)
            {
                return;
            }
            lastPoint = movePoint;
            //isInMode = true;
            m_application.MapControl.Refresh();
        }

        private void Gen(IPolygon range)
        {
            IFeatureLayer layer = info.Layer as IFeatureLayer;
            IFeatureClass fc = layer.FeatureClass;

            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureSelection fselect = layer as IFeatureSelection;

            IFeatureCursor FeatUpdateCursor = fc.Update(sf, false);
            IFeature feature_DLTB = null;

            IGeometryCollection pointcollection = new MultipointClass();
            IPointCollection ptcollection;
            object o_missing = Type.Missing;

            while ((feature_DLTB = FeatUpdateCursor.NextFeature()) != null)
            {
                ptcollection = feature_DLTB.Shape as IPointCollection;
                (pointcollection as IPointCollection).AddPointCollection(ptcollection);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(FeatUpdateCursor);

            ISymbol vertexSymbol = (ISymbol)CreateCharacterMarkerSymbol(47, CreateRGBColor(10, 10, 10, false), 10);
            IScreenDisplay screenDisplay = m_application.MapControl.ActiveView.ScreenDisplay;
            screenDisplay.StartDrawing(screenDisplay.hDC, (int)esriScreenCache.esriNoScreenCache);
            screenDisplay.SetSymbol(vertexSymbol);
            screenDisplay.DrawMultipoint(pointcollection as IGeometry);
            screenDisplay.FinishDrawing();
            (pointcollection as IGeometry).SetEmpty();


        }
        private static IMarkerSymbol CreateCharacterMarkerSymbol(int index_, IColor color_, int size_)
        {
            IMarkerSymbol markersymbol = new CharacterMarkerSymbolClass();
            IColor rgbcolor = color_;
            (markersymbol as ICharacterMarkerSymbol).CharacterIndex = index_;
            (markersymbol as ICharacterMarkerSymbol).Angle = 0;
            (markersymbol as ICharacterMarkerSymbol).Color = color_;
            (markersymbol as ICharacterMarkerSymbol).XOffset = 0;
            (markersymbol as ICharacterMarkerSymbol).YOffset = 0;
            (markersymbol as ICharacterMarkerSymbol).Size = size_;
            return markersymbol;
        }
        private static IColor CreateRGBColor(int rr, int gg, int bb, bool nullColor)
        {
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = rr;
            rgbColor.Green = gg;
            rgbColor.Blue = bb;
            rgbColor.NullColor = nullColor;

            return rgbColor as IColor;
        }
    }
}


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
    public class lineNodeEdit : BaseGenTool
    {
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
        public lineNodeEdit()
        {
            base.m_category = "GBuilding";
            base.m_caption = "编辑节点";
            base.m_message = "对选定要素进行编辑";
            base.m_toolTip = "对选定要素进行编辑。\n鼠标移至节点处，右键删除；\n按住shift键左键点击加入节点；\n空格键保存。";
            base.m_name = "linenodeeditor";
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
                if (tempInfo.LayerType == GCityLayerType.道路
                    && (tempInfo.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
                    && tempInfo.OrgLayer != null
                    )
                {
                    info = tempInfo;
                    break;
                }
            }
            if (info == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到道路图层");
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
                    lastPoint = null;
                    if (lineFB1 != null) {
                        lineFB1.Stop();
                        lineFB1 = null;
                    }
                    if (lineFB2 != null) {
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
                    if (notToPan)
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

        IGeometry shape = new PolylineClass();
        IPoint toRemovePoint = new PointClass();
        //bool isdone = true;//判断是否已经完成编辑存储
        bool toDoDelete = false;//是否是删除节点操作
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
                isHit = hitTest.HitTest(downPoint, 3, esriGeometryHitPartType.esriGeometryPartVertex, hitPoint, ref distance, ref hitPartIndex, ref hitSegmentIndex, ref isOnRightSide);           

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
                if (isEdit)
                {
                    toRemovePoint = hitPoint;

                    if (Button == 2&&currentSegs.SegmentCount>1) //TO DO DELETE
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
                        if (refPoint1 != null)//
                        {
                            lineFB1 = new NewLineFeedbackClass();
                            lineFB1.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                            lineFB1.Start(refPoint1);
                        }
                        if (refPoint2 != null)
                        {
                            lineFB2 = new NewLineFeedbackClass();
                            lineFB2.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                            lineFB2.Start(refPoint2);
                        }
                    }
                }

                if (!shape.IsEmpty && !isEdit&&isInMode&&Shift!=1)//shift键用来控制是否加入节点
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
        IPoint finalPoint = new PointClass();
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            object Missing = Type.Missing;
            IPoint upPoint = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            if (Shift == 1 && isInMode)//按住shift键加入节点
            {
                IPolycurve oriPolycurve = currentEditGeo as IPolycurve;
                bool splitHappened = false;
                int newPartIndex = -1;
                int newSegmentIndex = -1;
                oriPolycurve.SplitAtPoint(upPoint, true, false, out splitHappened, out newPartIndex, out newSegmentIndex);
                shape = oriPolycurve as IGeometry;

                isEdit = false;
                m_application.MapControl.Refresh();
            }

            if (isEdit == true)
            {
                if (isGet&&!notToPan)//
                {
                    if (lineFB1 != null)
                    {
                        lineFB1.AddPoint(getPoint);
                        line1 = lineFB1.Stop();
                    }
                    if (lineFB2 != null)
                    {
                        lineFB2.AddPoint(getPoint);
                        line2 = lineFB2.Stop();
                    }
                    finalPoint = getPoint;
                }
                else
                {
                    if (lineFB1 != null)
                    {
                        lineFB1.AddPoint(upPoint);
                        //lineFB2.AddPoint(upPoint);
                        line1 = lineFB1.Stop();
                    }
                    if (lineFB2 != null)
                    {
                        lineFB2.AddPoint(upPoint);
                        line2 = lineFB2.Stop();
                    }
                    finalPoint = upPoint;
                }
                //finalPoint = upPoint;

                //tpLine1 = line1;
                //tpLine2 = line2;
                lineFB1 = null;
                lineFB2 = null;
                //if (line1 == null || line1.IsEmpty||line2.IsEmpty||line2==null)
                if (line1 == null && line2 == null)
                {
                    return;
                }

                isEdit = false;
                m_application.MapControl.Refresh();
            }

            if (line1 != null || line2 != null)
            {            
                IPointCollection oriPoints = currentEditGeo as IPointCollection;
                IPolyline newTpPoly=new PolylineClass();
                IPointCollection shapePoints = newTpPoly as IPointCollection;
       
                for (int k = 0; k < oriPoints.PointCount; k++)
                {
                    if (toRemovePoint == null || toRemovePoint.IsEmpty)
                    {
                        break;
                    }
                    IPoint tpP = oriPoints.get_Point(k);
                    if (tpP.X != toRemovePoint.X && tpP.Y != toRemovePoint.Y)
                    {
                        shapePoints.AddPoint(tpP, ref Missing, ref Missing);
                    }
                    else
                    {
                        if (toDoDelete == false)//TO DO DELETE
                        {
                            shapePoints.AddPoint(finalPoint, ref Missing, ref Missing);
                        }
                    }
                }

                shape = newTpPoly;

                m_application.MapControl.Refresh();
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
                    IPolyline po = null;
                    List<IFeature> getFeats = new List<IFeature>();
                    while ((feat = fCursor.NextFeature()) != null)
                    {
                        getFeats.Add(feat);
                        if (feat.OID == editFeat.OID)
                        {
                            continue;
                        }
                        po = feat.Shape as IPolyline;
                        double distance = 0;
                        int hitPartIndex = 0;
                        int hitSegmentIndex = 0;
                        bool isOnRightSide = false;
                        IPoint hitPoint = new PointClass();
                        IHitTest hitTest = po as IHitTest;
                        isGet = hitTest.HitTest(movePoint, 3, esriGeometryHitPartType.esriGeometryPartVertex, getPoint, ref distance, ref hitPartIndex, ref hitSegmentIndex, ref isOnRightSide);
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
                            po = feat2.Shape as IPolyline;
                            double distance = 0;
                            int hitPartIndex = 0;
                            int hitSegmentIndex = 0;
                            bool isOnRightSide = false;
                            IPoint hitPoint = new PointClass();
                            IHitTest hitTest = po as IHitTest;
                            isGet = hitTest.HitTest(movePoint, 2, esriGeometryHitPartType.esriGeometryPartBoundary, getPoint, ref distance, ref hitPartIndex, ref hitSegmentIndex, ref isOnRightSide);
                            if (isGet)
                            {
                                break;
                            }
                        }
                    }
                    getFeats.Clear();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                }

                if (isGet&&!notToPan)//
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
            (rgb).Red = 222;
            (rgb).Green = 222;
            (rgb).Blue = 0;
            sls.Color = rgb;
            IScreenDisplay screenDisplay = e.display as IScreenDisplay;

            if (lastPoint != null)
            {
                IFeatureLayer layer = info.Layer as IFeatureLayer;
                IFeatureClass fc = layer.FeatureClass;

                ITopologicalOperator queryCirTo = lastPoint as ITopologicalOperator;
                IGeometry queryCir = queryCirTo.Buffer(2);


                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = queryCir as IGeometry;//buffer
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor FeatUpdateCursor = fc.Search(sf, false);

                IFeature feature_DLTB = null;

                IPointCollection ptcollection;
                object o_missing = Type.Missing;

                if ((feature_DLTB = FeatUpdateCursor.NextFeature()) != null)
                {
                    isInMode = true;
                    if (shape.IsEmpty)
                    {
                        shape = feature_DLTB.Shape as IPolyline;
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

            if (!shape.IsEmpty)
            {

                screenDisplay.SetSymbol(sls as ISymbol);

                screenDisplay.DrawPolyline(shape);
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

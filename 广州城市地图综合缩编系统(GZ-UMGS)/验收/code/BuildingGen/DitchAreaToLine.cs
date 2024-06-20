using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen
{
    public class DitchAreaToLine:BaseGenTool
    {
        INewPolygonFeedback fb;
        GLayerInfo waterLayer;
        SimpleFillSymbolClass sfs;
        SimpleMarkerSymbolClass sms;
        IFeatureClass lineFC;
        public DitchAreaToLine()
        {
            base.m_category = "GWater";
            base.m_caption = "沟渠面中轴化";
            base.m_message = "分离沟渠";
            base.m_toolTip = "对沟渠进行面分离，面中轴化，线分离，合并";
            base.m_name = "DitchAreaToLine";
            waterLayer = null;
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
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
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
            if (fb == null)
                return;

            IPolygon p = fb.Stop();
            fb = null;
            //Gen(p);
            /*if (System.Windows.Forms.MessageBox.Show("将进行沟渠分离、中轴化操作，请确认", "提示", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }*/
            IFeatureLayer ditchLayer = new FeatureLayerClass();
            IFeatureClass waterClass = (waterLayer.Layer as IFeatureLayer).FeatureClass;
            w = m_application.SetBusy(true);
            reshapeDitch(waterLayer,p);

            centralizeAndReshape(waterLayer,p);
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;
            if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
            {
                IFeatureClass centralizedDitch2 = FeatWS.OpenFeatureClass("沟渠中心线");
                ReshapeDitchLine2(centralizedDitch2,p);//应该只对沟渠区域做
               // connectDitchLine(centralizedDitch2);
                //unionTouchedDitchLines2(centralizedDitch2);

                _GenUsed2 = centralizedDitch2.FindField("_GenUsed");
                //autoSelectDitchLines(centralizedDitch2);

                ditchLayer.Name = "沟渠中心线";
                ditchLayer.FeatureClass = centralizedDitch2;
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                IUniqueValueRenderer render = new UniqueValueRendererClass();
                lineSymbol.Width = 2;
                IHsvColor hsvColor4 = new HsvColorClass();
                hsvColor4.Hue = 206;
                hsvColor4.Saturation = 96;
                hsvColor4.Value = 99;
                lineSymbol.Color = hsvColor4;
                render.DefaultSymbol = lineSymbol as ISymbol;
                render.UseDefaultSymbol = true;
                render.FieldCount = 1;
                render.set_Field(0, "_GenUsed");
                render.set_FieldType(0, false);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 2;
                IHsvColor hsvColor6 = new HsvColorClass();
                hsvColor6.Hue = 60;
                hsvColor6.Saturation = 100;
                hsvColor6.Value = 100;
                lineSymbol.Color = hsvColor6;
                render.AddValue("2", "_GenUsed", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 2;
                IHsvColor hsvColor = new HsvColorClass();
                hsvColor.Hue = 206;
                hsvColor.Saturation = 96;
                hsvColor.Value = 99;
                lineSymbol.Color = hsvColor;
                render.AddValue("1", "_GenUsed", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 2;
                IHsvColor hsvColor5 = new HsvColorClass();
                hsvColor5.Hue = 30;
                hsvColor5.Saturation = 40;
                hsvColor5.Value = 60;
                lineSymbol.Color = hsvColor5;
                render.AddValue("-2", "_GenUsed", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 2;
                IHsvColor hsvColor2 = new HsvColorClass();
                hsvColor2.Hue = 0;
                hsvColor2.Saturation = 0;
                hsvColor2.Value = 0;
                lineSymbol.Color = hsvColor2;
                render.AddValue("-1", "_GenUsed", lineSymbol as ISymbol);

                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Width = 2;
                IHsvColor hsvColor3 = new HsvColorClass();
                hsvColor3.Hue = 206;
                hsvColor3.Saturation = 96;
                hsvColor3.Value = 99;
                lineSymbol.Color = hsvColor3;
                render.AddValue("0", "_GenUsed", lineSymbol as ISymbol);

                (ditchLayer as IGeoFeatureLayer).DisplayField = "_GenUsed";
                (ditchLayer as IGeoFeatureLayer).Renderer = render as IFeatureRenderer;
                //(ditchLayer as IGeoFeatureLayer).DisplayField = "_GenUsed";
                bool isExistedLayer = false;
                for (int m = 0; m < m_application.MapControl.LayerCount; m++)
                {
                    if (m_application.MapControl.get_Layer(m).Name == "沟渠中心线")
                    {
                        isExistedLayer = true;
                        m_application.MapControl.DeleteLayer(m);
                    }
                }
                m_application.MapControl.AddLayer(ditchLayer, 0);
            }
            m_application.MapControl.Refresh();
        }
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null;
            }
        }
        public override void OnClick()
        {
            waterLayer = null;
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
            if (waterLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到水系图层");
                return;
            }
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
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
            m_application.MapControl.Refresh();
        }
        WaitOperation w;
        List<IPolygon> toShowPolygon = new List<IPolygon>();
        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e)
        {
            IDisplay dis = e.display as IDisplay;
            if (toShowPolygon.Count != 0)
            {
                IPolygon[] showPolygons = toShowPolygon.ToArray();
                dis.SetSymbol(sfs);
                for (int i = 0; i < showPolygons.Length; i++)
                {
                    dis.DrawPolygon(showPolygons[i]);
                }
            }

        }
        void reshapeDitch(GLayerInfo orgLayerInfo,IPolygon range)
        {
            IFeatureLayer waterLayer = orgLayerInfo.Layer as IFeatureLayer;
            IFeatureClass fc = waterLayer.FeatureClass;
            int fieldID = fc.FindField("要素代码");
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range as IGeometry;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.WhereClause = "要素代码='632250'";
            IFeatureCursor fCur;
            IFeature f;
            fCur = fc.Update(sf, false);
            ISpatialReferenceFactory srf_ = new SpatialReferenceEnvironmentClass();
            IProjectedCoordinateSystem pcs_ = srf_.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_CM_114E);
            IFeatureCursor insertCur = fc.Insert(false);
            try
            {
                List<IFeature> allFeats = new List<IFeature>();
                int count = fc.FeatureCount(sf);
                Dictionary<int, bool> isInsertedFeat = new Dictionary<int, bool>();
                while ((f = fCur.NextFeature()) != null)
                {
                    if (isInsertedFeat.ContainsKey(f.OID))
                    {
                        continue;
                    }
                    w.SetText("1/3:切割网状沟渠中...");
                    w.Step(count);
                    IGeometry geoCopy = f.ShapeCopy;
                    geoCopy.SpatialReference = pcs_;
                    IPolygon forMinArea = geoCopy as IPolygon;
                    double width = ((forMinArea as IArea).Area * 2.5) / forMinArea.Length;
                    double minArea = Math.Pow(width, 2) ;
                    IPolygon bufferPoly = (geoCopy as ITopologicalOperator).Buffer(-width/0.6) as IPolygon;
                    bufferPoly = (bufferPoly as ITopologicalOperator).Buffer(width/0.5) as IPolygon;
                    if (minArea < 88) minArea = 88;
                    /*if ((bufferPoly as IArea).Area < minArea)
                    {
                        continue;
                    }*/
                    ITopologicalOperator fcop = f.ShapeCopy as ITopologicalOperator;
                    fcop.Simplify();
                    ITopologicalOperator bcop = bufferPoly as ITopologicalOperator;
                    bcop.Simplify();
                    bufferPoly.SpatialReference = f.Shape.SpatialReference;
                    IPolygon4 resultPoly = fcop.Difference(bufferPoly) as IPolygon4;
                    /*if ((resultPoly as IArea).Area < minArea)
                    {
                        continue;
                    }*/
                    IGeometryCollection resultColl = resultPoly.ConnectedComponentBag as IGeometryCollection;
                    for (int i = 0; i < resultColl.GeometryCount; i++)
                    {
                        IPolygon tpPo = resultColl.get_Geometry(i) as IPolygon;
                        /*if ((tpPo as IArea).Area < minArea)
                        {
                            continue;
                        }*/
                        f.Shape = tpPo;
                        int ID=Convert.ToInt32(insertCur.InsertFeature(f as IFeatureBuffer));
                        isInsertedFeat.Add(ID, true);
                    }
                    bufferPoly = fcop.Difference(resultPoly) as IPolygon;
                    f.Shape = bufferPoly;
                    f.Store();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCur);
            }              
            catch (Exception ex)
            {
                m_application.SetBusy(false);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCur);
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
        void ConvertAttributes_Feat(IFeature oriFeat,IFeatureBuffer targetBuffer)
        {
            if(YSDM2!=-1)
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
            targetBuffer.set_Value(bzh2,oriFeat.get_Value(bzh));
            if (cjshj2 != -1)
            targetBuffer.set_Value(cjshj2,oriFeat.get_Value(cjshj));
            if (gxshj2 != -1)
            targetBuffer.set_Value(gxshj2,oriFeat.get_Value(gxshj));
            if (_GenUsed2 != -1)
            targetBuffer.set_Value(_GenUsed2, oriFeat.get_Value(_GenUsed));
            if (Shape_Leng2 != -1)
            targetBuffer.set_Value(Shape_Leng2, oriFeat.get_Value(Shape_Leng));
    
        }

        void centralizeAndReshape(GLayerInfo orgLayerInfo,IPolygon range)
        {
            IFeatureLayer waterLayer = orgLayerInfo.Layer as IFeatureLayer;
            IFeatureClass fc = waterLayer.FeatureClass;

            try
            {
                IFeatureClass centralizedDitch = null;
                IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
                IWorkspace2 WS2 = FeatWS as IWorkspace2;
                if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
                {
                    centralizedDitch = FeatWS.OpenFeatureClass("沟渠中心线");
                    /*IFeatureCursor deleteCur = centralizedDitch.Update(null, false);
                    IFeature deleteF = null;
                    while ((deleteF = deleteCur.NextFeature()) != null)
                    {
                        deleteF.Delete();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteCur);*/
                }
                else
                {
                    ESRI.ArcGIS.esriSystem.IClone clone = (fc.Fields as ESRI.ArcGIS.esriSystem.IClone).Clone();
                    IFields sourceFileds = clone as IFields;
                    IField shpField = sourceFileds.get_Field(sourceFileds.FindField("Shape"));
                    IGeometryDef geometryDef = shpField.GeometryDef;
                    IGeometryDefEdit geoDefEdit = geometryDef as IGeometryDefEdit;
                    geoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
                    centralizedDitch = FeatWS.CreateFeatureClass("沟渠中心线", sourceFileds, null, null, esriFeatureType.esriFTSimple, "Shape", "");
                }
                IFeatureCursor forInsertCur = centralizedDitch.Insert(false);

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

                int fieldID = fc.FindField("要素代码");
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = range as IGeometry;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                sf.WhereClause = "要素代码='632250'";
                IFeatureCursor fCur;
                fCur = fc.Update(sf, false);

                //WaitOperation w = m_application.SetBusy(true);//???????
                w.Step(0);
                int count = fc.FeatureCount(sf);
                BuildingGenCore.CenterLineFactory cf = new BuildingGenCore.CenterLineFactory();
                IFeature feat = null;
                while ((feat = fCur.NextFeature()) != null)
                {
                    w.SetText("2/3:中轴化..."+feat.OID);
                    w.Step(count);
                    try
                    {
                        IGeometry geoCopy = feat.ShapeCopy;
                        IPolygon forMinArea = geoCopy as IPolygon;
                        double width = ((forMinArea as IArea).Area * 2.5) / forMinArea.Length;
                        double tpWidth = Convert.ToDouble(m_application.GenPara["水系中轴化宽度阈值"]);
                        if (width > tpWidth)//10
                        {
                            feat.set_Value(_GenUsed, 1);
                            feat.Store();
                            continue;
                        }
                        feat.set_Value(_GenUsed, -1);
                        feat.Store();

                        BuildingGenCore.CenterLine centerLine = cf.Create2(forMinArea);

                        IPolyline resultLine = centerLine.Line;
                        if (resultLine.Length < 0.1)//
                        {
                            feat.Delete();//删除
                            continue;
                        }

                        IFeatureBuffer forInsertBuffer = centralizedDitch.CreateFeatureBuffer();//?
                        ConvertAttributes_Feat(feat, forInsertBuffer);
                        forInsertBuffer.Shape = resultLine;
                        forInsertBuffer.set_Value(_GenUsed2, 0);//?
                        forInsertCur.InsertFeature(forInsertBuffer);

                        feat.Delete();//删除
                    }
                    catch(Exception ex)
                    {
                        continue;
                    }

                }
                IFeatureLayer insertLayer = new FeatureLayerClass();
                insertLayer.FeatureClass = centralizedDitch;
                insertLayer.Name = "沟渠中心线";
                bool isExistedLayer = false;
                for (int m = 0; m < m_application.MapControl.LayerCount; m++)
                {
                    if (m_application.MapControl.get_Layer(m).Name == "沟渠中心线")
                    {
                        isExistedLayer = true;
                    }
                }
                if (!isExistedLayer)
                {
                    m_application.MapControl.AddLayer(insertLayer, 0);
                }
                m_application.MapControl.Refresh();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(forInsertCur);
                //m_application.SetBusy(false);//??????
            }
            catch (Exception ex)
            {
                m_application.SetBusy(false);
            }
        }

        class SegmentNode :IComparable{
            IPoint p;
            public SegmentNode(IPoint point)
            {
                p = (point as ESRI.ArcGIS.esriSystem.IClone).Clone() as IPoint;
                p.M = 0;
                p.Z = 0;
                p.ID = 0;

            }

            #region IComparable 成员

            public int CompareTo(object obj)
            {
                SegmentNode other = obj as SegmentNode;
                return p.Compare(other.p);
            }

            #endregion
        }
        class SegGraph
        {
            public SortedDictionary<SegmentNode, List<int>> Nodes;
            public List<ISegment> segs;
            public SegGraph()
            {
                Nodes = new SortedDictionary<SegmentNode, List<int>>();
                segs = new List<ISegment>();
            }
            public void AddSegment(ISegment seg)
            {                
                segs.Add(seg);
                int idx = segs.Count;
                SegmentNode n = new SegmentNode(seg.FromPoint);
                if (!Nodes.ContainsKey(n))
                    Nodes.Add(n, new List<int>());
                Nodes[n].Add(idx);
                n = new SegmentNode(seg.ToPoint);
                if (!Nodes.ContainsKey(n))
                    Nodes.Add(n,new List<int>());
                Nodes[n].Add(-idx);
            }
        }
        void ReshapeDitchLine2(IFeatureClass DitchLineFeats,IPolygon range)
        {
            Dictionary<int, double> index_angle = new Dictionary<int, double>();
            List<int> isgrouped = new List<int>();
            
            try
            {
                //WaitOperation w = m_application.SetBusy(true);//???????
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = range as IGeometry;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                sf.WhereClause = "要素代码='632250'";
                int cou = DitchLineFeats.FeatureCount(sf);
                IFeatureCursor fCur = DitchLineFeats.Update(sf, false);
                IFeatureCursor insertCur = DitchLineFeats.Insert(false);
                IFeature feat = null;
                Dictionary<int, bool> isInsertedFeat = new Dictionary<int, bool>();
                while ((feat = fCur.NextFeature()) != null)
                {
                    index_angle.Clear();
                    isgrouped.Clear();
                    if (isInsertedFeat.ContainsKey(feat.OID))
                    {
                        continue;
                    }
                    //System.Diagnostics.Debug.WriteLine(feat.OID);
                    w.SetText("3/3:分离沟渠网状线...");
                    w.Step(cou);
                    IPolyline line = feat.ShapeCopy as IPolyline;
                    line.Generalize(1.5);
                    IGeometryCollection pathColl = line as IGeometryCollection;
                    ISegmentCollection segColl = line as ISegmentCollection;

                    SegGraph segGraph = new SegGraph();
                    for (int m = 0; m < segColl.SegmentCount; m++)
                    {
                        segGraph.AddSegment(segColl.get_Segment(m));
                        isgrouped.Add(0);
                    }
                    //System.Collections.BitArray bitArray = new System.Collections.BitArray(segGraph.segs.Count);
                    int maxGroup=0;
                    List<ISegment> segs = segGraph.segs;
                    for (int m = 0; m < segs.Count;m++)
                    {
                        ISegment tpSeg = segs[m];
                        if (isgrouped[m] == 0)
                        {
                            isgrouped[m] = maxGroup + 1;
                            maxGroup = maxGroup + 1;
                        }
                        else
                        {
                            continue;
                        }
                        int findidx = m+1;
                        int lastIdx = -findidx;
                        IPoint pppp = tpSeg.FromPoint;
                        IPoint lastPoint = tpSeg.ToPoint;
                        do
                        {
                            lastIdx = -findidx;
                            findidx = 0;
                            
                            SegmentNode tpNode = new SegmentNode(pppp);
                            IPoint Pt1 = lastPoint;
                            List<int> connectSegs = segGraph.Nodes[tpNode];

                            foreach (int segIndex in connectSegs)
                            {
                                if (segIndex == lastIdx)
                                    continue;
                                IPoint passPt = null;
                                IPoint Pt2 = null;
                                ISegment other = segs[Math.Abs(segIndex) -1];
                                if (isgrouped[Math.Abs(segIndex) - 1] != 0)
                                    continue;
                                if (segIndex > 0)
                                {

                                    passPt = other.FromPoint;
                                    Pt2 = other.ToPoint;
                                }
                                else
                                {
                                    passPt = other.ToPoint;
                                    Pt2 = other.FromPoint;
                                }
                                double a = GApplication.GeometryEnvironment.ConstructThreePoint(Pt1, passPt, Pt2);
                                if ((Math.PI - Math.Abs(a)) < Math.PI / 5)
                                {
                                    findidx = segIndex;
                                    tpSeg = other;
                                    pppp = Pt2;
                                    lastPoint = passPt;
                                    isgrouped[Math.Abs(segIndex) - 1] = isgrouped[m];
                                    break;
                                }

                            }

                        } while (findidx != 0);

                        tpSeg = segs[m];
                        pppp = tpSeg.ToPoint;
                        lastPoint = tpSeg.FromPoint;
                        findidx = -(m + 1);
                        //lastIdx = m;
                        do
                        {
                            lastIdx = -findidx;
                            findidx = 0;
                            //IPoint pppp = (lastIdx < 0) ? tpSeg.ToPoint : tpSeg.FromPoint;
                            SegmentNode tpNode = new SegmentNode(pppp);
                            IPoint Pt1 = lastPoint;
                            List<int> connectSegs = segGraph.Nodes[tpNode];

                            foreach (int segIndex in connectSegs)
                            {
                                IPoint passPt = null;
                                IPoint Pt2 = null;
                                ISegment other = segs[Math.Abs(segIndex) - 1];
                                if (isgrouped[Math.Abs(segIndex) - 1] != 0)
                                    continue;
                                if (segIndex > 0)
                                {
                                    passPt = other.FromPoint;
                                    Pt2 = other.ToPoint;
                                }
                                else
                                {
                                    passPt = other.ToPoint;
                                    Pt2 = other.FromPoint;
                                }
                                double a = GApplication.GeometryEnvironment.ConstructThreePoint(Pt1, passPt, Pt2);
                                if ((Math.PI - Math.Abs(a)) < Math.PI / 5)
                                {
                                    findidx = segIndex;
                                    tpSeg = other;
                                    lastPoint = passPt;
                                    pppp = Pt2;
                                    isgrouped[Math.Abs(segIndex) - 1] = isgrouped[m];
                                    break;
                                }

                            }

                        } while (findidx != 0);
                    }
                    for (int m = 1; m < maxGroup + 1; m++)
                    {
                        PolylineClass newLine = new PolylineClass();
                        IGeometryCollection newColl = newLine as IGeometryCollection;

                        IPath oriPath = new PathClass();
                        for (int k = 0; k < isgrouped.Count; k++)
                        {
                            if (isgrouped[k] != m) continue;
                            ISegment ss = segs[k];
                            (oriPath as ISegmentCollection).AddSegments(1, ref ss);
                            
                            
                        }
                        IGeometry newo = oriPath as IGeometry;
                        newColl.AddGeometries(1, ref newo);
                        newLine.Simplify();
                        IFeatureBuffer tpBuffer = feat as IFeatureBuffer;
                        tpBuffer.Shape = newLine;
                        int insertID = Convert.ToInt32(insertCur.InsertFeature(tpBuffer));
                    }

                        feat.Delete();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCur);
                m_application.SetBusy(false);

            }
            catch (Exception ex)
            {
                m_application.SetBusy(false);
            }
        }

        void FindNode(ITinNode seed, Dictionary<int, IPoint> nodes, double distance)
        {
            ITinEdgeArray edges = seed.GetIncidentEdges();
            ITinEdit tin = seed.TheTin as ITinEdit;
            tin.SetNodeTagValue(seed.Index, -1);
            PointClass p = new PointClass();
            p.X = seed.X;
            p.Y = seed.Y;
            nodes.Add(seed.Index, p);
            for (int i = 0; i < edges.Count; i++)
            {
                ITinEdge edge = edges.get_Element(i);
                if (edge.Length < distance)
                {
                    if (edge.ToNode.IsInsideDataArea && edge.ToNode.TagValue != -1)
                    {
                        FindNode(edge.ToNode, nodes, distance);
                    }
                }
            }
        }
        TinClass tin;
        Dictionary<int, List<int>> nID_fID_dic;
        List<Dictionary<int, IPoint>> groups;
        GeometryBagClass gb;
        void connectDitchLine(IFeatureClass DitchLineFeats)
        {
            try
            {
                double dis = 8;//注意：小于8的原线段变为点
                //WaitOperation wo = m_application.SetBusy(true);//??????
                w.SetText("正在进行分析准备……");
                tin = new TinClass();
                tin.InitNew(m_application.MapControl.ActiveView.FullExtent);
                tin.StartInMemoryEditing();

                int featureCount = DitchLineFeats.FeatureCount(null);
                IFeatureCursor fCursor = DitchLineFeats.Search(null, true);
                IFeature feature = null;
                IPoint p = new PointClass();
                p.Z = 0;
                ITinNode node = new TinNodeClass();
                nID_fID_dic = new Dictionary<int, List<int>>();
                while ((feature = fCursor.NextFeature()) != null)
                {
                    w.SetText("4/7:正在进行分析准备:" + feature.OID.ToString());
                    w.Step(featureCount);
                    IPolyline line = feature.Shape as IPolyline;
                    p.X = line.FromPoint.X;
                    p.Y = line.FromPoint.Y;
                    tin.AddPointZ(p, 1, node);
                    if (!nID_fID_dic.ContainsKey(node.Index))
                    {
                        nID_fID_dic[node.Index] = new List<int>();
                    }
                    nID_fID_dic[node.Index].Add(feature.OID);
                    p.X = line.ToPoint.X;
                    p.Y = line.ToPoint.Y;
                    tin.AddPointZ(p, 1, node);
                    if (!nID_fID_dic.ContainsKey(node.Index))
                    {
                        nID_fID_dic[node.Index] = new List<int>();
                    }
                    nID_fID_dic[node.Index].Add(-feature.OID);
                }
                w.Step(0);
                groups = new List<Dictionary<int, IPoint>>();
                for (int i = 1; i <= tin.NodeCount; i++)
                {
                    w.SetText("5/7:正在分析:" + i.ToString());
                    w.Step(tin.NodeCount);

                    ITinNode n = tin.GetNode(i);
                    if (n.TagValue != -1 && n.IsInsideDataArea)
                    {
                        Dictionary<int, IPoint> g = new Dictionary<int, IPoint>();
                        FindNode(n, g, dis);
                        if (g.Count > 1)
                        {
                            groups.Add(g);
                        }
                    }
                }
                gb = new GeometryBagClass();
                object miss = Type.Missing;
                w.Step(0);
                w.SetText("6/7:正在整理分析结果");

                foreach (Dictionary<int, IPoint> g in groups)
                {
                    w.Step(groups.Count);
                    MultipointClass mp = new MultipointClass();
                    foreach (int nid in g.Keys)
                    {
                        IPoint pend = g[nid];
                        mp.AddGeometry(pend, ref miss, ref miss);
                    }
                    gb.AddGeometry(mp, ref miss, ref miss);
                }
                //m_application.SetBusy(false);//??????

                //ID_groupIDs.Clear();
                ProscessAll(true, DitchLineFeats);
                m_application.MapControl.Refresh();
            }
            catch (Exception ex)
            {
            }
        }

        void ProscessAll(bool commit,IFeatureClass DitchClass)
        {
            //WaitOperation wo = m_application.SetBusy(true);//???
            try
            {
                int count = gb.GeometryCount;
                for (int i = gb.GeometryCount - 1; i >= 0; i--)
                {
                    AutoProcess(i, commit,DitchClass);
                    w.Step(count);
                }
            }
            catch
            {
            }
            //m_application.SetBusy(false);
        }

        Dictionary<int, List<int>> ID_groupIDs = new Dictionary<int, List<int>>();//每个id的未处理组
        void AutoProcess(int index, bool commit,IFeatureClass DitchClass)
        {
            if (commit)
            {
                Dictionary<int, IPoint> group = groups[index];
                IFeatureClass fc = DitchClass;
                IGeometry geo = gb.get_Geometry(index);
                IEnvelope env = geo.Envelope;
                IPoint p = (env as IArea).Centroid;

                foreach (var item in group.Keys)
                {
                    List<int> fids = nID_fID_dic[item];
                    foreach (var fid in fids)
                    {
                        IFeature feature = fc.GetFeature(Math.Abs(fid));
                        IPolyline line = feature.ShapeCopy as IPolyline;
                        line.Generalize(1);
                        if (fid > 0)
                        {
                            line.FromPoint = p;
                        }
                        else
                        {
                            line.ToPoint = p;
                        }
                        feature.Shape = line;
                        feature.Store();

                    }
                }
            }
            groups.RemoveAt(index);
            gb.RemoveGeometries(index, 1);
        }

        void getGroupsForUnion(int oriID,List<int> GroupIDs)
        {
            if (GroupIDs.Contains(oriID))
            {
                return;
            }
            GroupIDs.Add(oriID);

            List<int> subGroup = ID_groupIDs[oriID];
            foreach (int subID in subGroup)
            {
                if (GroupIDs.Contains(subID))
                {
                    continue;
                }

                getGroupsForUnion(subID, GroupIDs);
            }
        }

        List<int> isHandledIDs = new List<int>();
        void unionTouchedDitchLines(IFeatureClass ditchClass)
        {
            isHandledIDs.Clear();
            IFeature f = null;
            IFeatureCursor fcur = ditchClass.Update(null, false);
            while ((f = fcur.NextFeature()) != null)
            {
                if (!isHandledIDs.Contains(f.OID))
                {
                    List<int> group = new List<int>();
                    getGroupsForUnion(f.OID, group);
                    isHandledIDs.AddRange(group);
                    if (group.Count == 1)
                    {
                        continue;
                    }
                    int[] g = group.ToArray();
                    IFeature okFeat = null;
                    GeometryBag forUnionBag = new GeometryBagClass();
                    IGeometry tpGeo=null;
                    for(int k=0;k<g.Length;k++)
                    {
                        IFeature featForDelete = null;
                        if (k == 0)
                        {
                            okFeat = ditchClass.GetFeature(g[0]);
                            tpGeo=okFeat.ShapeCopy;
                            (forUnionBag as IGeometryCollection).AddGeometries(1, ref tpGeo);
                        }
                        else
                        {
                            featForDelete = ditchClass.GetFeature(g[k]);
                            tpGeo = featForDelete.ShapeCopy;
                            (forUnionBag as IGeometryCollection).AddGeometries(1, ref tpGeo);
                        }
                        featForDelete.Delete();
                    }
                    ITopologicalOperator to = tpGeo as ITopologicalOperator;
                    to.Simplify();
                    ITopologicalOperator to2 = forUnionBag as ITopologicalOperator;
                    to2.Simplify();
                    to.ConstructUnion(forUnionBag as IEnumGeometry);
                    okFeat.Shape = to as IGeometry;
                    okFeat.Store();
                }
            }
        }

        void unionTouchedDitchLines2(IFeatureClass ditchClass)
        {
            try
            {
                IFeatureCursor insert = ditchClass.Insert(false);
                IFeatureCursor fC = ditchClass.Update(null, false);
                IFeature ff = null;
                ISpatialFilter sff = new SpatialFilterClass();
                sff.SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches;
                //WaitOperation w=m_application.SetBusy(true);//???????
                int c = ditchClass.FeatureCount(null);
                while ((ff = fC.NextFeature()) != null)
                {
                    w.SetText("7/7:正在融合沟渠线..."+ff.OID);
                    w.Step(c);
                    IPolyline pp = ff.ShapeCopy as IPolyline;
                    if (pp.Length == 0)
                    {
                        ff.Delete();
                        continue;
                    }
                    ITopologicalOperator ppt = pp as ITopologicalOperator;
                    ppt.Simplify();
                    double oriAngle = (pp.FromPoint.Y - pp.ToPoint.Y) / (pp.FromPoint.X - pp.ToPoint.X);
                    sff.Geometry = pp;
                    IFeatureCursor resultCur = ditchClass.Search(sff, false);
                    IFeature touchFeat = null;
                    bool isUnioned = false;
                    while ((touchFeat = resultCur.NextFeature()) != null)
                    {
                        IPolyline tt = touchFeat.ShapeCopy as IPolyline;
                        //double targetAngle = (tt.FromPoint.Y - tt.ToPoint.Y) / (tt.FromPoint.X - tt.ToPoint.X);

                        if (tt.FromPoint.X == pp.FromPoint.X && tt.FromPoint.Y == pp.FromPoint.Y)
                        {
                            double a = GApplication.GeometryEnvironment.ConstructThreePoint(tt.ToPoint, tt.FromPoint, pp.ToPoint);
                            if ((Math.PI - Math.Abs(a)) > 0)
                            {
                                continue;
                            }
                        }
                        else if (tt.ToPoint.X == pp.FromPoint.X && tt.ToPoint.Y == pp.FromPoint.Y)
                        {
                            double a = GApplication.GeometryEnvironment.ConstructThreePoint(tt.FromPoint, tt.ToPoint, pp.ToPoint);
                            if ((Math.PI - Math.Abs(a)) > 0)
                            {
                                continue;
                            }
                        }
                        else if (tt.ToPoint.X == pp.ToPoint.X && tt.ToPoint.Y == pp.ToPoint.Y)
                        {
                            double a = GApplication.GeometryEnvironment.ConstructThreePoint(tt.FromPoint, tt.ToPoint, pp.FromPoint);
                            if ((Math.PI - Math.Abs(a)) > 0)
                            {
                                continue;
                            }
                        }
                        else if (tt.FromPoint.X == pp.ToPoint.X && tt.FromPoint.Y == pp.ToPoint.Y)
                        {
                            double a = GApplication.GeometryEnvironment.ConstructThreePoint(tt.ToPoint, tt.FromPoint, pp.FromPoint);
                            if ((Math.PI - Math.Abs(a)) > 0)
                            {
                                continue;
                            }
                        }

                        /*double difference = Math.Abs(oriAngle - targetAngle);
                        if (difference > 0.3)
                        {
                            continue;
                        }*/
                        ITopologicalOperator ttp = tt as ITopologicalOperator;
                        ttp.Simplify();
                        ppt = ppt.Union(ttp as IGeometry) as ITopologicalOperator;
                        ppt.Simplify();
                        isUnioned = true;
                        touchFeat.Delete();
                    }
                    if (isUnioned)
                    {
                        IPolyline pptl = ppt as IPolyline;
                        if (pptl.Length > 1)
                        {
                            IFeatureBuffer unionedBuffer = ff as IFeatureBuffer;
                            unionedBuffer.Shape = ppt as IGeometry;
                            insert.InsertFeature(unionedBuffer);
                        }
                        ff.Delete();
                    }
                    else
                    {
                        if (pp.Length < 1)
                            ff.Delete();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(resultCur);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fC);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(insert);
                m_application.SetBusy(false);
            }
            catch (Exception exx)
            {
                m_application.SetBusy(false);
            }
        }

        public override bool Deactivate()
        {
            m_application.MapControl.OnAfterDraw -= new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            return true;
        }
        public bool isOutsideTri(ITinTriangle tri)
        {
            int tag = -1;
            tag = (tri.get_Node(0)).TagValue;
            if (tag == (tri.get_Node(1)).TagValue && tag == (tri.get_Node(2)).TagValue)
            {
                return false;
            }
            if (tag != (tri.get_Node(1)).TagValue && tag != (tri.get_Node(2)).TagValue && (tri.get_Node(1)).TagValue != (tri.get_Node(2)).TagValue)
            {
                return false;
            }
            return true;

        }
        public void addNodeOIDtoList(ITinTriangle tri, List<int> toList)
        {
            bool isExisted = false;
            for (int i = 0; i < 3; i++)
            {
                ITinNode node = tri.get_Node(i);

                foreach (int j in toList)
                {
                    if (j == node.TagValue)
                    {
                        isExisted = true;
                    }
                }
                if (!isExisted)
                {
                    toList.Add(node.TagValue);
                }
            }
        }
        Dictionary<int, bool> hasDeletedFeats = new Dictionary<int, bool>();
        Dictionary<int, bool> hasSavedFeats = new Dictionary<int, bool>();
        Dictionary<int, List<int>> Feat_neighberFeats = new Dictionary<int, List<int>>();
        //List<int> nextList = new List<int>();
        void select(IFeature startFeat, bool priviousFeatHasDeleted)
        {
            //List<int> nextList = new List<int>();
            if (!Feat_neighberFeats.ContainsKey(startFeat.OID)||hasDeletedFeats.ContainsKey(startFeat.OID)||hasSavedFeats.ContainsKey(startFeat.OID))
            {
                return;
            }
            List<int> nextList = Feat_neighberFeats[startFeat.OID];
            if (!priviousFeatHasDeleted)
            {
                if (!hasDeletedFeats.ContainsKey(startFeat.OID))
                {
                    hasDeletedFeats.Add(startFeat.OID,true);
                }
                startFeat.set_Value(_GenUsed2, -1);
                startFeat.Store();
            }
            else
            {
                if (!hasSavedFeats.ContainsKey(startFeat.OID))
                {
                    hasSavedFeats.Add(startFeat.OID,true);
                }
                startFeat.set_Value(_GenUsed2, 1);
                startFeat.Store();
            }

            if (!priviousFeatHasDeleted)
            {
                foreach (int nextIDs in nextList)
                {
                    if(hasDeletedFeats.ContainsKey(nextIDs))
                    {
                        continue;
                    }
                    IFeature nextFeat = ID_Feat[nextIDs];
                    //IFeature nextFeat = lineFC.GetFeature(nextIDs);
                    select(nextFeat, true);
                }
            }
            else
            {
                foreach (int nextIDs in nextList)
                {
                    if (hasSavedFeats.ContainsKey(nextIDs))
                    {
                        continue;
                    }
                    IFeature nextFeat = ID_Feat[nextIDs];
                    //IFeature nextFeat = lineFC.GetFeature(nextIDs);
                    select(nextFeat, false);
                }
            }

        }
        Dictionary<int, IFeature> ID_Feat = new Dictionary<int, IFeature>();
        void autoSelectDitchLines(IFeatureClass DitchLineFeats)
        {
            lineFC = DitchLineFeats;
            //Dictionary<int, List<IFeature>> groupID_groupMembers = new Dictionary<int, List<IFeature>>();
            ID_Feat.Clear();
            Feat_neighberFeats.Clear();
            hasSavedFeats.Clear();
            hasDeletedFeats.Clear();

            //WaitOperation wo = m_application.SetBusy(true);//??????
            w.SetText("正在进行分析准备……");
            tin = new TinClass();
            tin.InitNew(m_application.MapControl.ActiveView.FullExtent);
            tin.StartInMemoryEditing();

            int featureCount = DitchLineFeats.FeatureCount(null);
            IFeatureCursor fCursor = DitchLineFeats.Search(null, false);
            IFeature feature = null;
            ITinNode node = new TinNodeClass();
            object z = 0;
            try
            {
                while ((feature = fCursor.NextFeature()) != null)
                {
                    w.SetText("8/11:正在进行分析准备:" + feature.OID.ToString());
                    w.Step(featureCount);
                    IPolyline line = feature.ShapeCopy as IPolyline;
                    //if (line.Length < 120)//1w:100,2.5w:120?????
                    if(line.Length<0)
                    {
                        feature.set_Value(_GenUsed2, -2);
                        feature.Store();
                        feature.Delete();
                        continue;
                    }
                    ITopologicalOperator lineto = line as ITopologicalOperator;
                    lineto.Simplify();
                    line.Densify(4, 0);
                    //feature.Shape = line as IGeometry;
                    //feature.Store();
                    IPointCollection pc = line as IPointCollection;
                    for (int i = 0; i < pc.PointCount; i++)
                    {
                        tin.AddShape(pc.get_Point(i), esriTinSurfaceType.esriTinMassPoint, feature.OID, ref z);
                    }
                    ID_Feat.Add(feature.OID, feature);
                }
                ITinEdit tinEdit = tin as ITinEdit;
                ITinAdvanced tinAd = tin as ITinAdvanced;
                ITinValueFilter filter = new TinValueFilterClass();
                ITinFeatureSeed seed = new TinTriangleClass();
                filter.ActiveBound = esriTinBoundType.esriTinUniqueValue;
                //filter.UniqueValue = 0;
                seed.UseTagValue = true;
                w.Step(0);
                for (int i = 1; i < tin.TriangleCount + 1; i++)
                {
                    w.SetText("9/11:沟渠邻近分析..."+i);
                    w.Step(tin.TriangleCount);
                    ITinTriangle tinTriangle = tin.GetTriangle(i);
                    bool istoolong = false;
                    if (!tinTriangle.IsInsideDataArea || !isOutsideTri(tinTriangle))
                    {
                        tinEdit.SetTriangleTagValue(i, -2);
                        continue;
                    }
                    else
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            ITinEdge tinEdge = tinTriangle.get_Edge(k);
                            if (tinEdge.Length > 110)//88
                            {
                                istoolong = true;
                                tinEdit.SetTriangleTagValue(i, -2);
                                break;
                            }
                        }
                    }
                    int oid1 = -1;
                    int oid2 = -1;
                    if (!istoolong)
                    {
                        oid1 = (tinTriangle.get_Node(0)).TagValue;
                        if ((tinTriangle.get_Node(1)).TagValue != oid1)
                        {
                            oid2 = (tinTriangle.get_Node(1)).TagValue;
                        }
                        else
                        {
                            oid2 = (tinTriangle.get_Node(2)).TagValue;
                        }
                        int lasttagvalue=0;
                        if (oid1 < oid2)
                        {
                            lasttagvalue = oid1 * 2 + oid2;
                        }
                        else
                        {
                            lasttagvalue = oid2 * 2 + oid1;
                        }
                        tinEdit.SetTriangleTagValue(i,lasttagvalue);
                    }
                }
                w.Step(0);
                for (int i = 1; i < tin.TriangleCount + 1; i++)
                {
                    w.SetText("10/11:存储分析结果..."+i);
                    w.Step(tin.TriangleCount);
                    int tpValue=(tin.GetTriangle(i)).TagValue;
                    if (tpValue!= -2&&tpValue!=0)
                    {
                        filter.UniqueValue = (tin.GetTriangle(i)).TagValue;
                        seed = tin.GetTriangle(i) as ITinFeatureSeed;
                        ITinPolygon tinPoly = tinAd.ExtractPolygon(seed as ITinElement, filter as ITinFilter, false);
                        IPolygon polygon = tinPoly.AsPolygon(null, false);
                        IEnumTinTriangle enumTri = tinPoly.AsTriangles();
                        int triCount = 0;
                        enumTri.Reset();
                        ITinTriangle tpTri = null;
                        List<int> touchOIDList = new List<int>();
                        while ((tpTri = enumTri.Next()) != null)
                        {
                            addNodeOIDtoList(tpTri, touchOIDList);
                            tinEdit.SetTriangleTagValue(tpTri.Index, -2);
                            triCount++;
                        }
                        if (triCount < 15)
                        {
                            continue;
                        }
                        int[] touchArray = touchOIDList.ToArray();
                        if (touchArray.Length < 2)
                        {
                            continue;
                        }
                        IFeature oriFeat = ID_Feat[touchArray[0]];
                        //IFeature oriFeat = DitchLineFeats.GetFeature(touchArray[0]);
                        IPolyline oriLine = oriFeat.Shape as IPolyline;
                        IFeature nextFeat = ID_Feat[touchArray[1]];
                        //IFeature nextFeat = DitchLineFeats.GetFeature(touchArray[1]);
                        IPolyline nextLine = nextFeat.Shape as IPolyline;
                        double oriAngle = (oriLine.FromPoint.Y - oriLine.ToPoint.Y) / (oriLine.FromPoint.X - oriLine.ToPoint.X);
                        double nextAngle = (nextLine.FromPoint.Y - nextLine.ToPoint.Y) / (nextLine.FromPoint.X - nextLine.ToPoint.X);
                        double difference = Math.Abs(oriAngle - nextAngle);
                        if (difference > 0.5)
                        {
                            continue;
                        }

                        if (Feat_neighberFeats.ContainsKey(touchArray[0]))
                        {
                            List<int> neighberFeats = Feat_neighberFeats[touchArray[0]];
                            if (!neighberFeats.Contains(touchArray[1]))
                            {
                                neighberFeats.Add(touchArray[1]);
                            }
                        }
                        else
                        {
                            List<int> neighberFeats = new List<int>();
                            neighberFeats.Add(touchArray[1]);
                            Feat_neighberFeats.Add(touchArray[0], neighberFeats);
                        }

                        if (Feat_neighberFeats.ContainsKey(touchArray[1]))
                        {
                            List<int> neighberFeats = Feat_neighberFeats[touchArray[1]];
                            if (!neighberFeats.Contains(touchArray[0]))
                            {
                                neighberFeats.Add(touchArray[0]);
                            }
                        }
                        else
                        {
                            List<int> neighberFeats = new List<int>();
                            neighberFeats.Add(touchArray[0]);
                            Feat_neighberFeats.Add(touchArray[1], neighberFeats);
                        }
                    }
                }
                w.Step(0);
                int co = DitchLineFeats.FeatureCount(null);
                //foreach (IFeature f in ID_Feat.Values)
                IFeatureCursor fcuu;
                fcuu = DitchLineFeats.Update(null, true);
                IFeature f = null;
                while((f=fcuu.NextFeature())!=null)
                {
                    w.SetText("11/11:整理数据...");
                    //wo.Step(ID_Feat.Count);
                    w.Step(co);
                    select(f, false);
                }
                m_application.SetBusy(false);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                m_application.MapControl.Refresh();
            }
            catch (Exception ex)
            {
                m_application.SetBusy(false);
            }
        }
     
        public override void OnKeyUp(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Space:
                    IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
                    IWorkspace2 WS2 = FeatWS as IWorkspace2;
                    if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
                    {
                        IFeatureClass centralizedDitch2 = FeatWS.OpenFeatureClass("沟渠中心线");
                        IFeatureCursor fcur = null;
                        IFeature feat = null;
                        _GenUsed2 = centralizedDitch2.FindField("_GenUsed");
                        IQueryFilter qf=new QueryFilterClass();
                        qf.WhereClause="_GenUsed<0";
                        fcur = centralizedDitch2.Update(qf, false);
                        WaitOperation w = m_application.SetBusy(true);
                        int c = centralizedDitch2.FeatureCount(qf);
                        while ((feat = fcur.NextFeature()) != null)
                        {
                            w.SetText("删除没有选中的要素...");
                            w.Step(c);
                            feat.Delete();
                        }
                        m_application.SetBusy(false);
                    }

                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null)
                    {
                        fb.Stop();
                        fb = null;
                    }
                    break;
            }
        }
    }
}

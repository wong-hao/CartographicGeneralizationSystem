using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen
{
    public class CentralizeNarrowRiver : BaseGenTool
    {
        INewPolygonFeedback fb;
        public CentralizeNarrowRiver()
        {
            base.m_category = "GWater";
            base.m_caption = "水系中轴化";
            base.m_message = "水系中轴化";
            base.m_toolTip = "中轴化狭窄的河流、沟渠";
            base.m_name = "CentralizeNarrowRiver";
            base.m_usedParas = new GenDefaultPara[] { 
                new GenDefaultPara("水系中轴化宽度阈值",25)
            };
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
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null; ;
            }
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
            centralizeAndReshape(waterLayer, p);
            m_application.MapControl.Refresh();
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
        void centralizeAndReshape(GLayerInfo orgLayerInfo, IPolygon range)
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
                    //IFeatureCursor deleteCur = centralizedDitch.Update(null, false);
                    //IFeature deleteF = null;
                    //while ((deleteF = deleteCur.NextFeature()) != null)
                    //{
                    //    deleteF.Delete();
                    //}
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteCur);
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

                //double yWidth = (double)m_application.GenPara["水系中轴化宽度阈值"];
                int yWidth = (int)m_application.GenPara["水系中轴化宽度阈值"];
                int fieldID = fc.FindField("要素代码");
                ISpatialFilter qf = new SpatialFilterClass();
                qf.Geometry = range;
                qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                qf.WhereClause = "要素代码='614450'" + " " + "OR" + " " + "要素代码='632250'";
                IFeatureCursor fCur;
                fCur = fc.Update(qf, false);

                WaitOperation w = m_application.SetBusy(true);
                int count = fc.FeatureCount(qf);
                BuildingGenCore.CenterLineFactory cf = new BuildingGenCore.CenterLineFactory();
                IFeature feat = null;
                while ((feat = fCur.NextFeature()) != null)
                {
                    w.SetText("中轴化..." + feat.OID);
                    w.Step(count);
                    try
                    {
                        IGeometry geoCopy = feat.ShapeCopy;
                        IPolygon forMinArea = geoCopy as IPolygon;
                        double width = ((forMinArea as IArea).Area * 2.5) / forMinArea.Length;
                        if (width > yWidth)//2.5w:25,5w:40,10w:100
                        {
                            feat.set_Value(_GenUsed, 1);
                            feat.Store();
                            continue;
                        }
                        feat.set_Value(_GenUsed, -1);
                        feat.Store();

                        BuildingGenCore.CenterLine centerLine = cf.Create2(forMinArea);

                        IPolyline resultLine = centerLine.Line;
                        //if (resultLine.Length < 70)//
                        //{
                        //    feat.set_Value(_GenUsed, -2);
                        //    feat.Store();
                        //    continue;
                        //}
                        IFeatureBuffer forInsertBuffer = centralizedDitch.CreateFeatureBuffer();//?
                        ConvertAttributes_Feat(feat, forInsertBuffer);
                        //resultLine.Generalize(1);
                        forInsertBuffer.Shape = resultLine;
                        if (resultLine.Length < 120)
                        {
                            forInsertBuffer.set_Value(_GenUsed2, -8);
                            feat.Delete();
                            continue;
                        }
                        else
                        {
                            forInsertBuffer.set_Value(_GenUsed2, 0);
                        }
                        forInsertCur.InsertFeature(forInsertBuffer);

                        feat.Delete();
                    }
                    catch (Exception ex)
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
                m_application.SetBusy(false);
            }
            catch (Exception ex)
            {
                m_application.SetBusy(false);
            }
        }

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

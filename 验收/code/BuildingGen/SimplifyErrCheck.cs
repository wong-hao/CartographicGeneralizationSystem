using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;


namespace BuildingGen
{
    public class SimplifyErrCheck : BaseGenTool
    {
        INewPolygonFeedback fb;
        GLayerInfo info;
        SimpleFillSymbolClass sfs;
        SimpleFillSymbolClass sfs_;

        GCityLayerType currentLayerType;
        public SimplifyErrCheck(GCityLayerType layerType)
        {
            base.m_category = "GBuilding";
            base.m_caption = "化简错误检查";
            base.m_message = "图斑丢失检查";
            base.m_toolTip = "图斑丢失检查。";
            base.m_name = "SimErr";

            sfs = new SimpleFillSymbolClass();
            sfs_ = new SimpleFillSymbolClass();
            SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
            SimpleLineSymbolClass sls_ = new SimpleLineSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 0;
            rgb.Green = 0;
            rgb.Blue = 150;

            sls.Color = rgb;
            sls.Width = 2;
            sfs.Outline = sls;
            sfs.Style = esriSimpleFillStyle.esriSFSNull;

            RgbColorClass rgb_ = new RgbColorClass();
            rgb_.Red = 245;
            rgb_.Green = 245;
            rgb_.Blue = 122;
            sls_.Width = 1;
            sls_.Style = esriSimpleLineStyle.esriSLSNull;
            sfs_.Outline = sls_;
            sfs_.Style = esriSimpleFillStyle.esriSFSSolid;
            sfs_.Color = rgb_;

            currentLayerType = layerType;
        }

        public override bool Enabled
        {
            get
            {
                return (m_application.Workspace != null);
            }
        }

        public override bool Deactivate()
        {
            m_application.MapControl.OnAfterDraw -= new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            return true;
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
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e)
        {
            IDisplay dis = e.display as IDisplay;
            if (toShowAreaRatio == 0)
            {
                return;
            }
            if (toShowPolygon.Count != 0||oritoShowPolygon.Count!=0)
            {
                IPolygon[] showPolygons = toShowPolygon.ToArray();
                dis.SetSymbol(sfs);
                for (int i = 0; i < showPolygons.Length; i++)
                {
                    dis.DrawPolygon(showPolygons[i]);
                }   
                IPolygon[] oriShowPolygons = oritoShowPolygon.ToArray();
                dis.SetSymbol(sfs_);
                for (int i = 0; i < oriShowPolygons.Length; i++)
                {
                    dis.DrawPolygon(oriShowPolygons[i]);
                }                     
            }

        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            if (info == null)
                return;

            IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            bool isDelete = false;
            if (oritoShowPolygon.Count != 0)
            {
                IPolygon[] errArray = oritoShowPolygon.ToArray();
                for (int j = 0; j < errArray.Length; j++)
                {
                    IEnvelope env = errArray[j].Envelope;
                    if ((env as IRelationalOperator).Contains(p))
                    {
                        oritoShowPolygon.Remove(errArray[j]);
                        int removeID;
                        oriPolygon_ID.TryGetValue(errArray[j], out removeID);
                        oriFeatIDs.Remove(removeID);
                        m_application.MapControl.Refresh();
                        isDelete = true;
                        break;
                    }
                }
            }
            if (toShowPolygon.Count != 0)
            {
                IPolygon[] errArray = toShowPolygon.ToArray();
                for (int j = 0; j < errArray.Length; j++)
                {
                    IEnvelope env = errArray[j].Envelope;
                    if ((env as IRelationalOperator).Contains(p))
                    {
                        toShowPolygon.Remove(errArray[j]);
                        int removeID;
                        Polygon_ID.TryGetValue(errArray[j], out removeID);
                        featIDs.Remove(removeID);
                        m_application.MapControl.Refresh();
                        isDelete = true;
                        break;
                    }
                }
            }

            if (isDelete) return;
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

            //featIDs.Clear();
            //oriFeatIDs.Clear();

        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
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
            LeakCheck(p);

            m_application.MapControl.Refresh();
        }

        List<int> featIDs = new List<int>();
        List<int> oriFeatIDs = new List<int>();
        IFeatureClass featClass;
        IFeatureClass oriFeatClass;

        Dictionary<int, double> ID_AreaRatio = new Dictionary<int, double>();
        Dictionary<int, Dictionary<int, IFeature>> ID_Feats = new Dictionary<int, Dictionary<int, IFeature>>();
        Dictionary<int, IPolygon> oriID_IPolygon = new Dictionary<int, IPolygon>();
        double toShowAreaRatio = 0;
        private void LeakCheck(IPolygon range)
        {
            ID_AreaRatio.Clear();
            ID_Feats.Clear();
            oriID_IPolygon.Clear();

            featClass = (info.Layer as IFeatureLayer).FeatureClass;
            oriFeatClass = (info.OrgLayer as IFeatureLayer).FeatureClass;
            double minArea = (double)m_application.GenPara["水系最小上图面积"];
            
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            //IFeatureCursor featCursor = featClass.Search(sf, false);
            IFeatureCursor oriFeatCursor = oriFeatClass.Search(sf, false);

            IFeature feat = null;
            IFeatureCursor featCursor;
            WaitOperation wo = m_application.SetBusy(true);
            wo.SetText("正在检查数据...");
            while ((feat = oriFeatCursor.NextFeature()) != null)
            {
                wo.Step(400);
                double tpArea = (feat.Shape as IArea).Area;
                if (tpArea < minArea)
                {
                    continue;
                }
                double simplifiedArea = 0;

                sf.Geometry = feat.Shape;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                featCursor = featClass.Search(sf, false);
                IFeature toFeat;
                Dictionary<int,IFeature> tpID_Feats=new Dictionary<int,IFeature>();
                while ((toFeat = featCursor.NextFeature()) != null)
                {                                     
                    IArea a = (toFeat.Shape as ITopologicalOperator).Intersect(feat.Shape, esriGeometryDimension.esriGeometry2Dimension) as IArea;
                    simplifiedArea += a.Area;
                    if (a.Area > 0)
                    {
                        tpID_Feats.Add(toFeat.OID, toFeat);
                    }
                }                
                double ratio = simplifiedArea / tpArea;
                ID_AreaRatio.Add(feat.OID, ratio);
                ID_Feats.Add(feat.OID, tpID_Feats);
                oriID_IPolygon.Add(feat.OID, feat.ShapeCopy as IPolygon);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);
            }
            m_application.SetBusy(false);
            toShowAreaRatio = 0.5;
            oriFeatIDs.Clear();
            featIDs.Clear();
            toShowPolygon.Clear();
            oritoShowPolygon.Clear();
            oriPolygon_ID.Clear();
            Polygon_ID.Clear();
            foreach (int oriID in ID_AreaRatio.Keys)
            {
                double ratio = 1;
                ID_AreaRatio.TryGetValue(oriID, out ratio);
                if (ratio < toShowAreaRatio)
                {
                    oriFeatIDs.Add(oriID);
                    oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                    oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                    Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                    foreach (IFeature tpFeats in toShowFeats.Values)
                    {
                        toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                        featIDs.Add(tpFeats.OID);
                        Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(oriFeatCursor);
            m_application.MapControl.Refresh();
        }

        int genUsedIndex = -1;
        int groupIndex = -1;
        List<IPolygon> toShowPolygon = new List<IPolygon>();
        List<IPolygon> oritoShowPolygon = new List<IPolygon>();
        Dictionary<IPolygon, int> Polygon_ID = new Dictionary<IPolygon, int>();
        Dictionary<IPolygon, int> oriPolygon_ID = new Dictionary<IPolygon, int>();
        public override void OnKeyDown(int keyCode, int Shift)
        {
            genUsedIndex = featClass.FindField("_GenUsed");
            groupIndex = featClass.FindField("G_BuildingGroup");

            int index = oriFeatClass.FindField("G_BuildingGroup");
            if (index == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "G_BuildingGroup";
                field.Type_2 = esriFieldType.esriFieldTypeInteger;
                field.DefaultValue_2 = 0;
                oriFeatClass.AddField(field);
                index = oriFeatClass.FindField("G_BuildingGroup");
                ClearGroup(oriFeatClass, index);
            }
            index = oriFeatClass.FindField("_GenUsed");
            if (index == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "_GenUsed";
                field.Type_2 = esriFieldType.esriFieldTypeInteger;
                field.DefaultValue_2 = 0;
                oriFeatClass.AddField(field);
                index = oriFeatClass.FindField("_GenUsed");
                ClearGroup(oriFeatClass, index);
            }

            IGraphicsContainer pGraphicsContainer = m_application.MapControl.Map as IGraphicsContainer;
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null)
                    {
                        fb.Stop();
                        fb = null;
                    }
                    break;
                case (int)System.Windows.Forms.Keys.Space:
                    if (featIDs.Count != 0 || oriFeatIDs.Count != 0)
                    {
                        IPolygon[] oriShowPolygons = oritoShowPolygon.ToArray();
                        for (int i = 0; i < oriShowPolygons.Length; i++)
                        {
                            IMarkerElement pMarkerElement = new MarkerElementClass();
                            pMarkerElement.Symbol = CreateCharacterMarkerSymbol(47, CreateRGBColor(255, 0, 0, false), 20);
                            IPoint pMarkerPt = (oriShowPolygons[i] as IArea).LabelPoint;
                            IElement pNewElement = pMarkerElement as IElement;
                            pNewElement.Geometry = pMarkerPt as IGeometry;
                            pGraphicsContainer.AddElement(pNewElement, 0);

                        }

                        IFeatureCursor insertCur = featClass.Insert(true);

                        int[] oriFeatIDArray = oriFeatIDs.ToArray();
                        IFeature tpFeat = null;
                        IFeature insertFeat = null;
                        for (int i = 0; i < oriFeatIDArray.Length; i++)
                        {
                            tpFeat = oriFeatClass.GetFeature(oriFeatIDArray[i]);
                            int insertID = (int)(insertCur.InsertFeature(tpFeat as IFeatureBuffer));
                            insertFeat = featClass.GetFeature(insertID);
                            if (genUsedIndex != -1 && insertFeat != null)
                            {
                                insertFeat.set_Value(genUsedIndex, -1);
                                insertFeat.Store();
                            }
                            if (groupIndex != -1 && insertFeat != null)
                            {
                                insertFeat.set_Value(groupIndex, -1);
                                insertFeat.Store();
                            }
                        }

                        int[] featIDArray = featIDs.ToArray();
                        //IFeature tpFeat = null;
                        for (int i = 0; i < featIDArray.Length; i++)
                        {
                            tpFeat = featClass.GetFeature(featIDArray[i]);
                            tpFeat.Delete();
                        }

                        featIDs.Clear();
                        oriFeatIDs.Clear();
                        toShowPolygon.Clear();
                        ID_Feats.Clear();
                        ID_AreaRatio.Clear();
                        oritoShowPolygon.Clear();
                        oriID_IPolygon.Clear();
                        oriPolygon_ID.Clear();
                        Polygon_ID.Clear();
                        m_application.MapControl.Refresh();
                    }
                    break;
                case (int)System.Windows.Forms.Keys.D:

                    pGraphicsContainer.DeleteAllElements();
                    featIDs.Clear();
                    oriFeatIDs.Clear();
                    toShowPolygon.Clear();
                    ID_Feats.Clear();
                    ID_AreaRatio.Clear();
                    oritoShowPolygon.Clear();
                    oriID_IPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    m_application.MapControl.Refresh();
                    
                    break;
                case (int)System.Windows.Forms.Keys.N:
                    Next();
                    break;
                case (int)System.Windows.Forms.Keys.D1:                    
                    toShowAreaRatio = 0.1;
                    oriFeatIDs.Clear();
                    featIDs.Clear();
                    toShowPolygon.Clear();
                    oritoShowPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    foreach (int oriID in ID_AreaRatio.Keys)
                    {
                        double ratio = 1;
                        ID_AreaRatio.TryGetValue(oriID, out ratio);
                        if (ratio < toShowAreaRatio)
                        {
                            oriFeatIDs.Add(oriID);
                            oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                            oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                            Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                            foreach (IFeature tpFeats in toShowFeats.Values)
                            {
                                toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                                featIDs.Add(tpFeats.OID);
                                Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                            }
                        }
                    }
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.D2:
                    toShowAreaRatio = 0.2;
                    oriFeatIDs.Clear();
                    featIDs.Clear();
                    toShowPolygon.Clear();
                    oritoShowPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    foreach (int oriID in ID_AreaRatio.Keys)
                    {
                        double ratio = 1;
                        ID_AreaRatio.TryGetValue(oriID, out ratio);
                        if (ratio < toShowAreaRatio)
                        {
                            oriFeatIDs.Add(oriID);
                            oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                            oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                            Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                            foreach (IFeature tpFeats in toShowFeats.Values)
                            {
                                toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                                featIDs.Add(tpFeats.OID);
                                Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                            }
                        }
                    }
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.D3:
                    toShowAreaRatio = 0.3;
                    oriFeatIDs.Clear();
                    featIDs.Clear();
                    toShowPolygon.Clear();
                    oritoShowPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    foreach (int oriID in ID_AreaRatio.Keys)
                    {
                        double ratio = 1;
                        ID_AreaRatio.TryGetValue(oriID, out ratio);
                        if (ratio < toShowAreaRatio)
                        {
                            oriFeatIDs.Add(oriID);
                            oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                            oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                            Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                            foreach (IFeature tpFeats in toShowFeats.Values)
                            {
                                toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                                featIDs.Add(tpFeats.OID);
                                Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                            }
                        }
                    }
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.D4:
                    toShowAreaRatio = 0.4;
                    oriFeatIDs.Clear();
                    featIDs.Clear();
                    toShowPolygon.Clear();
                    oritoShowPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    foreach (int oriID in ID_AreaRatio.Keys)
                    {
                        double ratio = 1;
                        ID_AreaRatio.TryGetValue(oriID, out ratio);
                        if (ratio < toShowAreaRatio)
                        {
                            oriFeatIDs.Add(oriID);
                            oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                            oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                            Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                            foreach (IFeature tpFeats in toShowFeats.Values)
                            {
                                toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                                featIDs.Add(tpFeats.OID);
                                Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                            }
                        }
                    }
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.D5:
                    toShowAreaRatio = 0.5;
                    oriFeatIDs.Clear();
                    featIDs.Clear();
                    toShowPolygon.Clear();
                    oritoShowPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    foreach (int oriID in ID_AreaRatio.Keys)
                    {
                        double ratio = 1;
                        ID_AreaRatio.TryGetValue(oriID, out ratio);
                        if (ratio < toShowAreaRatio)
                        {
                            oriFeatIDs.Add(oriID);
                            oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                            oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                            Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                            foreach (IFeature tpFeats in toShowFeats.Values)
                            {
                                toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                                featIDs.Add(tpFeats.OID);
                                Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                            }
                        }
                    }
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.D6:
                    toShowAreaRatio = 0.6;
                    oriFeatIDs.Clear();
                    featIDs.Clear();
                    toShowPolygon.Clear();
                    oritoShowPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    foreach (int oriID in ID_AreaRatio.Keys)
                    {
                        double ratio = 1;
                        ID_AreaRatio.TryGetValue(oriID, out ratio);
                        if (ratio < toShowAreaRatio)
                        {
                            oriFeatIDs.Add(oriID);
                            oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                            oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                            Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                            foreach (IFeature tpFeats in toShowFeats.Values)
                            {
                                toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                                featIDs.Add(tpFeats.OID);
                                Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                            }
                        }
                    }
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.D7:
                    toShowAreaRatio = 0.7;
                    oriFeatIDs.Clear();
                    featIDs.Clear();
                    toShowPolygon.Clear();
                    oritoShowPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    foreach (int oriID in ID_AreaRatio.Keys)
                    {
                        double ratio = 1;
                        ID_AreaRatio.TryGetValue(oriID, out ratio);
                        if (ratio < toShowAreaRatio)
                        {
                            oriFeatIDs.Add(oriID);
                            oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                            oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                            Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                            foreach (IFeature tpFeats in toShowFeats.Values)
                            {
                                toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                                featIDs.Add(tpFeats.OID);
                                Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                            }
                        }
                    }
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.D8:
                    toShowAreaRatio = 0.8;
                    oriFeatIDs.Clear();
                    featIDs.Clear();
                    toShowPolygon.Clear();
                    oritoShowPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    foreach (int oriID in ID_AreaRatio.Keys)
                    {
                        double ratio = 1;
                        ID_AreaRatio.TryGetValue(oriID, out ratio);
                        if (ratio < toShowAreaRatio)
                        {
                            oriFeatIDs.Add(oriID);
                            oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                            oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                            Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                            foreach (IFeature tpFeats in toShowFeats.Values)
                            {
                                toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                                featIDs.Add(tpFeats.OID);
                                Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                            }
                        }
                    }
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.D9:
                    toShowAreaRatio = 0.9;
                    oriFeatIDs.Clear();
                    featIDs.Clear();
                    toShowPolygon.Clear();
                    oritoShowPolygon.Clear();
                    oriPolygon_ID.Clear();
                    Polygon_ID.Clear();
                    foreach (int oriID in ID_AreaRatio.Keys)
                    {
                        double ratio = 1;
                        ID_AreaRatio.TryGetValue(oriID, out ratio);
                        if (ratio < toShowAreaRatio)
                        {
                            oriFeatIDs.Add(oriID);
                            oritoShowPolygon.Add(oriID_IPolygon[oriID]);
                            oriPolygon_ID.Add(oriID_IPolygon[oriID], oriID);
                            Dictionary<int, IFeature> toShowFeats = ID_Feats[oriID];
                            foreach (IFeature tpFeats in toShowFeats.Values)
                            {
                                toShowPolygon.Add(tpFeats.ShapeCopy as IPolygon);
                                featIDs.Add(tpFeats.OID);
                                Polygon_ID.Add(tpFeats.ShapeCopy as IPolygon, tpFeats.OID);
                            }
                        }
                    }
                    m_application.MapControl.Refresh();
                    break;
            }
        }

        private void ClearGroup(IFeatureClass fc, int index)
        {
            IFeatureCursor cursor = fc.Update(null, true);
            IFeature f = null;
            while ((f = cursor.NextFeature()) != null)
            {
                object v = f.get_Value(index);
                f.set_Value(index, 0);
                cursor.UpdateFeature(f);
            }
            cursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }

        int currentErr = 0;
        void Next()
        {
            if (oritoShowPolygon.Count == 0)
                return;
            currentErr--;
            if (currentErr < 0)
            {
                currentErr = oritoShowPolygon.Count - 1;
            }
            if (currentErr >oritoShowPolygon.Count-1)
            {
                currentErr=0;
            }
            IPolygon[] errArray=oritoShowPolygon.ToArray();
            IEnvelope env = errArray[currentErr].Envelope;
            env.Expand(8, 8, false);
            m_application.MapControl.Extent = env;
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

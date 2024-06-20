using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen
{
    public class ConflictHandleOfRoadBuild : BaseGenTool
    {
        GLayerInfo waterLayer;
        SimpleFillSymbolClass sfs;
        SimpleMarkerSymbolClass sms;
        IFeatureClass lineFC;
        IGraphicsContainer pGraphicsContainer;
        public ConflictHandleOfRoadBuild()
        {
            base.m_category = "GBuilding";
            base.m_caption = "房屋道路冲突处理";
            base.m_message = "房屋道路冲突处理";
            base.m_toolTip = "房屋道路冲突处理";
            base.m_name = "BRHandle";
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
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null;
            }
        }
        IFeatureClass buildingClass;
        public override void OnClick()
        {
            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            pGraphicsContainer = m_application.MapControl.Map as IGraphicsContainer;
            IFeatureLayer buildingLayer = new FeatureLayerClass();
            IFeatureLayer roadLayer = new FeatureLayerClass();

            foreach (GLayerInfo tempInfo in m_application.Workspace.LayerManager.Layers)
            {
                if (tempInfo.LayerType == GCityLayerType.建筑物)
                {
                    buildingLayer = tempInfo.Layer as IFeatureLayer;
                }
                if (tempInfo.LayerType == GCityLayerType.道路)
                {
                    roadLayer = tempInfo.Layer as IFeatureLayer;
                }
            }
            buildingClass = buildingLayer.FeatureClass;
            IFeatureClass roadClass = roadLayer.FeatureClass;

            if (System.Windows.Forms.MessageBox.Show("将进行冲突分析操作，请确认", "提示", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            toShowPolygon.Clear();
            Build_Road.Clear();

            BuildID_Road.Clear();
            Shape_ID.Clear();

            IFeatureCursor featCur = roadClass.Search(null, false);
            IFeature feat = null;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
            WaitOperation w = m_application.SetBusy(true);
            w.Step(0);
            int c = roadClass.FeatureCount(null);
            while((feat=featCur.NextFeature())!=null)
            {
                w.SetText("正在分析..."+feat.OID);
                w.Step(c);
                sf.Geometry = feat.Shape;
                IFeatureCursor featCur2 = buildingClass.Search(sf, false);
                IFeature feat2 = null;
                while ((feat2 = featCur2.NextFeature()) != null)
                {
                    //if (!Build_Road.ContainsKey(feat2))
                    if (!BuildID_Road.ContainsKey(feat2.OID))
                    {
                        List<IFeature> roads = new List<IFeature>();
                        roads.Add(feat);
                        //Build_Road.Add(feat2, roads);
                        BuildID_Road.Add(feat2.OID, roads);
                    }
                    else
                    {
                        //List<IFeature> roads = Build_Road[feat2];
                        List<IFeature> roads = BuildID_Road[feat2.OID];
                        roads.Add(feat);
                    }
                    if (!toShowPolygon.Contains(feat2.Shape as IPolygon))
                    {
                        toShowPolygon.Add(feat2.Shape as IPolygon);
                    }

                    if (!Shape_ID.ContainsKey(feat2.Shape as IPolygon))
                    {
                        Shape_ID.Add(feat2.Shape as IPolygon, feat2.OID);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(featCur2);
            }

            IPolygon[] oriShowPolygons = toShowPolygon.ToArray();
            for (int i = 0; i < oriShowPolygons.Length; i++)
            {
                IMarkerElement pMarkerElement = new MarkerElementClass();
                pMarkerElement.Symbol = CreateCharacterMarkerSymbol(47, CreateRGBColor(255, 0, 0, false), 25);
                IPoint pMarkerPt = (oriShowPolygons[i] as IArea).LabelPoint;
                IElement pNewElement = pMarkerElement as IElement;
                pNewElement.Geometry = pMarkerPt as IGeometry;
                pGraphicsContainer.AddElement(pNewElement, 0);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featCur);
            m_application.SetBusy(false);

            m_application.MapControl.Refresh();
        }

        Dictionary<IFeature, List<IFeature>> Build_Road = new Dictionary<IFeature, List<IFeature>>();
        Dictionary<int, List<IFeature>> BuildID_Road = new Dictionary<int, List<IFeature>>();
        Dictionary<IPolygon, int> Shape_ID = new Dictionary<IPolygon, int>();

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
            {
                return;
            }
            if (Button == 1)
            {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

                if (toShowPolygon.Count != 0)
                {
                    IPolygon[] errArray = toShowPolygon.ToArray();
                    for (int j = 0; j < errArray.Length; j++)
                    {
                        IEnvelope env = errArray[j].Envelope;
                        if ((env as IRelationalOperator).Contains(p))
                        {
                            toShowPolygon.Remove(errArray[j]);
                            BuildID_Road.Remove(Shape_ID[errArray[j]]);
                            m_application.MapControl.Refresh();
                            break;
                        }
                    }
                }
                m_application.MapControl.Refresh();
            }
        }

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

        public override bool Deactivate()
        {
            m_application.MapControl.OnAfterDraw -= new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

            return true;
        }

        public override void OnKeyUp(int keyCode, int Shift)
        {

        }

        public override void OnKeyDown(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    toShowPolygon.Clear();
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.N:
                    Next();
                    break;
                case (int)System.Windows.Forms.Keys.D:
                    toShowPolygon.Clear();
                    pGraphicsContainer.DeleteAllElements();
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.Space:
                    try
                    {
                        WaitOperation ww = m_application.SetBusy(true);
                        ww.Step(0);
                        IFeatureCursor insertCur = buildingClass.Insert(false);
                        //foreach (IFeature buildFeat in Build_Road.Keys)
                        foreach (int buildFeatID in BuildID_Road.Keys)
                        {
                            ww.SetText("正在分割提交...");
                            ww.Step(BuildID_Road.Count);
                            IFeature buildFeat = buildingClass.GetFeature(buildFeatID);
                            double oriArea = (buildFeat.Shape as IArea).Area;

                            List<IPolygon> result = new List<IPolygon>();
                            List<IPolygon> isCutedBuilding = new List<IPolygon>();
                            //cutBuilding(buildFeat.ShapeCopy as IPolygon, Build_Road[buildFeat],false,ref result);
                            cutBuilding(buildFeat.ShapeCopy as IPolygon, BuildID_Road[buildFeatID], isCutedBuilding, ref result);

                            foreach (IPolygon insertPoly in result)
                            {
                                double partArea=(insertPoly as IArea).Area;
                                double ratio = partArea / oriArea;
                                if (ratio > 0.1)
                                {
                                    IFeatureBuffer insertBuffer = buildFeat as IFeatureBuffer;
                                    insertBuffer.Shape = insertPoly as IGeometry;
                                    insertCur.InsertFeature(insertBuffer);
                                }
                            }
                            buildFeat.Delete();
                        }
                        m_application.SetBusy(false);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCur);
                        m_application.MapControl.Refresh();
                    }
                    catch (Exception ex)
                    {
                    }
                    break;
            }
        }
        void cutBuilding(IPolygon building,List<IFeature> crossRoads,List<IPolygon> isCutedBuild, ref List<IPolygon> resultBuilds)
        {
            bool iscut = false;
            //IPolygon building = buildFeat.ShapeCopy;
            foreach (IFeature roadF in crossRoads)
            {
                IPolyline road = roadF.Shape as IPolyline;
                ITopologicalOperator buildTO = building as ITopologicalOperator;
                buildTO.Simplify();
                IRelationalOperator buildRE = building as IRelationalOperator;
                if (buildRE.Crosses(road as IGeometry))
                {
                    IGeometry leftGeo;
                    IGeometry rightGeo;
                    try
                    {
                        buildTO.Cut(road, out leftGeo, out rightGeo);
                        isCutedBuild.Add(building);
                        iscut = true;

                        if (resultBuilds.Contains(building))
                        {
                            resultBuilds.Remove(building);
                        }
                    }
                    catch
                    {
                        if (!resultBuilds.Contains(building)&&!isCutedBuild.Contains(building))
                        {
                            resultBuilds.Add(building);
                        }
                        continue;
                    }
                    cutBuilding(leftGeo as IPolygon, crossRoads, isCutedBuild,ref resultBuilds);
                    cutBuilding(rightGeo as IPolygon, crossRoads, isCutedBuild,ref resultBuilds);
                    if (iscut)
                    {
                        break;
                    }
                }
                else
                {
                    if (!resultBuilds.Contains(building) && !isCutedBuild.Contains(building))
                    {
                        resultBuilds.Add(building);
                    }
                }
            }

        }

        int currentErr = 0;
        void Next()
        {
            if (toShowPolygon.Count == 0)
                return;
            currentErr--;
            if (currentErr < 0)
            {
                currentErr = toShowPolygon.Count - 1;
            }
            if (currentErr > toShowPolygon.Count - 1)
            {
                currentErr = 0;
            }
            IPolygon[] errArray = toShowPolygon.ToArray();
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

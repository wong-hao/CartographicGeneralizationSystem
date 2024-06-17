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
    public class GetOriFeatsforRoad : BaseGenTool
    {
        INewPolygonFeedback fb;
        GLayerInfo info;
        SimpleLineSymbolClass sfs;
        SimpleLineSymbolClass sfs_;
        public GetOriFeatsforRoad()
        {
            base.m_category = "GBuilding";
            base.m_caption = "提取原要素";
            base.m_message = "提取选定范围内的原要素";
            base.m_toolTip = "提取选定范围内的原要素。\n拉框选取；\n按住shift点击右键或者按D键删除要素；\n空格键确认提取。";
            base.m_name = "getOriFeatsforLine";

            sfs = new SimpleLineSymbolClass();
            sfs_ = new SimpleLineSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 0;
            rgb.Green = 0;
            rgb.Blue = 150;

            sfs.Color = rgb;
            sfs.Width = 2;

            RgbColorClass rgb_ = new RgbColorClass();
            rgb_.Red = 245;
            rgb_.Green = 245;
            rgb_.Blue = 122;
            sfs_.Width = 1;
            sfs_.Color = rgb_;

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
                System.Windows.Forms.MessageBox.Show("没有找到图层");
                return;
            }

            IMap map = m_application.Workspace.Map;
            IFeatureLayer layer = (info.Layer as IFeatureLayer);
            if (layer == null)
            {
                return;
            }

            IFeatureClass fc = layer.FeatureClass;
            if (fc.ShapeType != esriGeometryType.esriGeometryPolyline)
            {
                System.Windows.Forms.MessageBox.Show("当前编辑图层不是线状图层");
                return;
            }

            m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e)
        {
            IDisplay dis = e.display as IDisplay;
            if (featIDs.Count != 0 || oriFeatIDs.Count != 0)
            {
                int[] featIDArray = featIDs.ToArray();
                IFeature tpFeat = null;
                dis.SetSymbol(sfs);
                for (int i = 0; i < featIDArray.Length; i++)
                {
                    tpFeat = featClass.GetFeature(featIDArray[i]);
                    dis.DrawPolyline(tpFeat.Shape);
                }

                int[] oriFeatIDArray = oriFeatIDs.ToArray();
                dis.SetSymbol(sfs_);
                for (int i = 0; i < oriFeatIDArray.Length; i++)
                {
                    tpFeat = oriFeatClass.GetFeature(oriFeatIDArray[i]);
                    dis.DrawPolyline(tpFeat.Shape);
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

            if (Button == 2 && Shift == 1)
            {
                ITopologicalOperator queryCirTo = p as ITopologicalOperator;
                IGeometry queryCir = queryCirTo.Buffer(2);

                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = queryCir;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor featCursor = featClass.Search(sf, false);
                IFeature toDelete = null;
                if ((toDelete = featCursor.NextFeature()) != null)
                {
                    toDelete.Delete();
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);
                m_application.MapControl.Refresh();
                return;
            }

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

            featIDs.Clear();
            oriFeatIDs.Clear();

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
            Gen(p);

            m_application.MapControl.Refresh();
        }

        List<int> featIDs = new List<int>();
        List<int> oriFeatIDs = new List<int>();
        IFeatureClass featClass;
        IFeatureClass oriFeatClass;

        private void Gen(IPolygon range)
        {
            IFeatureLayer layer = new FeatureLayerClass();
            IFeatureSelection fselect = layer as IFeatureSelection;
            layer.FeatureClass = (info.Layer as IFeatureLayer).FeatureClass;

            featClass = layer.FeatureClass;
            oriFeatClass = (info.OrgLayer as IFeatureLayer).FeatureClass;


            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            IFeatureCursor featCursor = featClass.Search(sf, false);
            IFeatureCursor oriFeatCursor = oriFeatClass.Search(sf, false);

            IFeature feat = null;
            while ((feat = featCursor.NextFeature()) != null)
            {
                featIDs.Add(feat.OID);
            }
            while ((feat = oriFeatCursor.NextFeature()) != null)
            {

                oriFeatIDs.Add(feat.OID);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oriFeatCursor);

        }

        int genUsedIndex = -1;
        int areaIndex = -1;
        int widthIndex = -1;
        int groupIndex = -1;
        int rankIndex = -1;
        int linkIndex = -1;

        public override void OnKeyDown(int keyCode, int Shift)
        {
            genUsedIndex = featClass.FindField("_GenUsed");
            areaIndex = featClass.FindField("面积");
            widthIndex = featClass.FindField("宽度");
            groupIndex = featClass.FindField("道路分组");
            rankIndex = featClass.FindField("道路等级");
            linkIndex = featClass.FindField("道路连通度");

            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null)
                    {
                        fb.Stop();
                        fb = null;
                    }
                    featIDs.Clear();
                    oriFeatIDs.Clear();
                    break;
                case (int)System.Windows.Forms.Keys.Space:
                    if (featIDs.Count != 0 || oriFeatIDs.Count != 0)
                    {
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
                            if (areaIndex != -1 && insertFeat != null)
                            {
                                insertFeat.set_Value(areaIndex, -1);
                                insertFeat.Store();
                            }
                            if (widthIndex != -1 && insertFeat != null)
                            {
                                insertFeat.set_Value(widthIndex, -1);
                                insertFeat.Store();
                            }
                            if (rankIndex != -1 && insertFeat != null)
                            {
                                insertFeat.set_Value(rankIndex, -1);
                                insertFeat.Store();
                            }
                            if (linkIndex != -1 && insertFeat != null)
                            {
                                insertFeat.set_Value(linkIndex, -1);
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
                        m_application.MapControl.Refresh();
                    }
                    break;
                case (int)System.Windows.Forms.Keys.D:
                    if (featIDs.Count != 0)
                    {
                        IFeature tpFeat = null;
                        int[] featIDArray = featIDs.ToArray();
                        //IFeature tpFeat = null;
                        for (int i = 0; i < featIDArray.Length; i++)
                        {
                            tpFeat = featClass.GetFeature(featIDArray[i]);
                            tpFeat.Delete();
                        }
                        featIDs.Clear();
                        oriFeatIDs.Clear();
                        m_application.MapControl.Refresh();
                    }
                    break;
            }
        }


    }

}

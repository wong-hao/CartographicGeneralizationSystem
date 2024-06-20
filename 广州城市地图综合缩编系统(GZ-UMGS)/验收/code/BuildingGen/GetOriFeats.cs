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
    public class GetOriFeats : BaseGenTool
    {
        INewPolygonFeedback fb;
        GLayerInfo info;
        SimpleFillSymbolClass sfs;
        SimpleFillSymbolClass sfs_;
        public GetOriFeats()
        {
            base.m_category = "GBuilding";
            base.m_caption = "提取原要素";
            base.m_message = "提取选定范围内的原要素";
            base.m_toolTip = "提取选定范围内的原要素。\n拉框选取；\n按住shift点击右键或者按D键删除要素；\n空格键确认提取。";
            base.m_name = "getOriFeats";

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

        }

        GCityLayerType currentLayerType;
        public GetOriFeats(GCityLayerType layerType)
        {
            base.m_category = "GBuilding";
            base.m_caption = "提取原要素";
            base.m_message = "提取选定范围内的原要素";
            base.m_toolTip = "提取选定范围内的原要素。\n拉框选取；\n按住shift点击右键或者按D键删除要素；\n空格键确认提取。";
            base.m_name = "getOriFeats";

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

        //public override bool Deactivate()
        //{
        //    m_application.MapControl.OnAfterDraw -= new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);

        //    return true;
        //}

        bool initDraw = false;
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

            IFeatureClass fc = layer.FeatureClass;
            if (fc.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                System.Windows.Forms.MessageBox.Show("当前编辑图层不是面状图层");
                return;
            }
            if (!initDraw)
            {
                m_application.MapControl.OnAfterDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterDrawEventHandler(MapControl_OnAfterDraw);
                initDraw = true;
            }
        }

        void MapControl_OnAfterDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterDrawEvent e)
        {
            IDisplay dis = e.display as IDisplay;
            if (feats.Count != 0 || oriFeats.Count != 0)
            {
                if (feats.Count < 3500 && oriFeats.Count < 3500)
                {
                    dis.SetSymbol(sfs);
                    foreach (IFeature f in feats)
                    {
                        dis.DrawPolygon(f.Shape);
                    }

                    dis.SetSymbol(sfs_);
                    foreach (IFeature f2 in oriFeats)
                    {
                        dis.DrawPolygon(f2.Shape);
                    }
                }
                else
                {
                    dis.SetSymbol(sfs);
                    dis.DrawPolygon(chooseArea);
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
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = p;
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
            feats.Clear();
            oriFeats.Clear();

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

        IPolygon chooseArea;
        public override void OnDblClick()
        {
            if (fb == null)
                return;

            IPolygon p = fb.Stop();
            chooseArea = p;
            fb = null;
            Gen(p);

            m_application.MapControl.Refresh();
        }

        IFeatureClass featClass;
        IFeatureClass oriFeatClass;

        List<IFeature> feats = new List<IFeature>();
        List<IFeature> oriFeats = new List<IFeature>();

        private void Gen(IPolygon range)
        {
            IFeatureLayer layer = new FeatureLayerClass();
            IFeatureSelection fselect = layer as IFeatureSelection;
            layer.FeatureClass = (info.Layer as IFeatureLayer).FeatureClass;

            featClass=layer.FeatureClass;
            oriFeatClass = (info.OrgLayer as IFeatureLayer).FeatureClass;


            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            IFeatureCursor featCursor = featClass.Search(sf, false);
            IFeatureCursor oriFeatCursor = oriFeatClass.Search(sf, false);

            int count = featClass.FeatureCount(sf);
            int count2 = oriFeatClass.FeatureCount(sf);
            WaitOperation w=null;
            if (count > 300 || count2 > 300)
            {
                w=m_application.SetBusy(true);
            }
            IFeature feat=null;
            while ((feat = featCursor.NextFeature()) != null)
            {
                if (count2 > 300 || count > 300)
                {
                    w.Step(count+count2);
                }
                feats.Add(feat);
            }
            while ((feat = oriFeatCursor.NextFeature()) != null)
            {
                if (count > 300 || count2 > 300)
                {
                    w.Step(count2+count);
                }
                oriFeats.Add(feat);
            }
            if (count > 300 || count2 > 300)
            {
                m_application.SetBusy(false);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oriFeatCursor);

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

        int genUsedIndex = -1;
        int groupIndex = -1;
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
            index = oriFeatClass.FindField("要素代码");
            if (index == -1)
            {
                IFieldEdit2 field = new FieldClass();
                field.Name_2 = "要素代码";
                field.Type_2 = esriFieldType.esriFieldTypeString;
                field.DefaultValue_2 = "0000";
                oriFeatClass.AddField(field);
                index = oriFeatClass.FindField("要素代码");
            }

            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null)
                    {
                        fb.Stop();
                        fb = null;
                    }
                    feats.Clear();
                    oriFeats.Clear();
                    m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.Space:
                    if(feats.Count!=0||oriFeats.Count!=0)
                    {
                        IFeatureCursor insertCur = featClass.Insert(true);

                        WaitOperation w = null;
                        if (feats.Count > 300 || oriFeats.Count > 300)
                        {
                            w = m_application.SetBusy(true);
                        }
                        foreach (IFeature tpFeat in oriFeats)
                        {
                            if (feats.Count > 300 || oriFeats.Count > 300)
                            {
                                w.SetText("正在存储...");
                                w.Step(feats.Count + oriFeats.Count);
                            }
                            insertCur.InsertFeature(tpFeat as IFeatureBuffer);
                        }

                        foreach (IFeature tpFeat2 in feats)
                        {
                            if (feats.Count > 300 || oriFeats.Count > 300)
                            {
                                w.SetText("正在存储...");
                                w.Step(feats.Count + oriFeats.Count);
                            }
                            tpFeat2.Delete();
                        }
                        if (feats.Count > 300 || oriFeats.Count > 300)
                        {
                            m_application.SetBusy(false);
                        }
                        oriFeats.Clear();
                        feats.Clear();

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCur);
                        m_application.MapControl.Refresh();
                    }
                    break;
                case (int)System.Windows.Forms.Keys.D:
                    if(feats.Count!=0)
                    {
                        foreach (IFeature tpFeat2 in feats)
                        {
                            tpFeat2.Delete();
                        }
                        oriFeats.Clear();
                        feats.Clear();
                        m_application.MapControl.Refresh();
                    }
                    break;
            }
        }


    }

}

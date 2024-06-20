using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.NetworkAnalysis;

namespace BuildingGen
{
    public class SelectDitchByHands : BaseGenTool
    {
        public SelectDitchByHands()
        {
            base.m_category = "GRoad";
            base.m_caption = "沟渠取舍";
            base.m_message = "手工选取沟渠";
            base.m_toolTip = "手工选取沟渠\n拉框选择；\n按住shift拉框选择删除。";
            base.m_name = "selectDitch";
            roadLayer = null;
            ditchClass = null;
            //rankID = -1;
            _GenUsed2 = -1;
            fb = null;
        }
        public override bool Enabled
        {
            get
            {
                return m_application.Workspace != null;
            }
        }
        IFeatureClass ditchClass;
        GLayerInfo roadLayer;
        int rankID;
        int _GenUsed2;
        public override void OnClick()
        {
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers)
            {
                if (info.LayerType == GCityLayerType.水系
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon
                    && info.OrgLayer != null
                    )
                {
                    roadLayer = info;
                    break;
                }
            }
            if (roadLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到层");
                return;
            }

            IFeatureClass fc = (roadLayer.Layer as IFeatureLayer).FeatureClass;

            //rankID = fc.Fields.FindField("道路等级");
            //if (rankID == -1)
            //{
            //    IFieldEdit2 field = new FieldClass();
            //    field.Name_2 = "道路等级";
            //    field.Type_2 = esriFieldType.esriFieldTypeInteger;
            //    fc.AddField(field as IField);
            //    rankID = fc.Fields.FindField("道路等级");
            //}

            //IFeatureLayer targetLayer =null;
            IFeatureLayer ditchLayer = new FeatureLayerClass();
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;
            if (WS2.get_NameExists(esriDatasetType.esriDTFeatureClass, "沟渠中心线"))
            {
                IFeatureClass centralizedDitch2 = FeatWS.OpenFeatureClass("沟渠中心线");
                ditchClass = centralizedDitch2;
                //IFeatureLayer ditchLayer = new FeatureLayerClass();
                ditchLayer.Name = "沟渠中心线";
                ditchLayer.FeatureClass = centralizedDitch2;
                //targetLayer = ditchLayer;

                //bool isExistedLayer = false;
                //for (int m = 0; m < m_application.MapControl.LayerCount; m++)
                //{
                //    if (m_application.MapControl.get_Layer(m).Name == "沟渠中心线")
                //    {
                //        isExistedLayer = true;
                //    }
                //}
                //if (!isExistedLayer)
                //{
                //    m_application.MapControl.AddLayer(ditchLayer, 0);
                //}
                ////m_application.MapControl.AddLayer(ditchLayer, 0);

                _GenUsed2 = centralizedDitch2.FindField("_GenUsed");
                if (_GenUsed2 == -1)
                {
                    IFieldEdit2 field = new FieldClass();
                    field.Name_2 = "_GenUsed";
                    field.Type_2 = esriFieldType.esriFieldTypeInteger;
                    centralizedDitch2.AddField(field as IField);
                    _GenUsed2 = centralizedDitch2.Fields.FindField("_GenUsed");
                }
            }
            ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
            IUniqueValueRenderer render = new UniqueValueRendererClass();
            lineSymbol.Width = 2;
            IHsvColor hsvColor4 = new HsvColorClass();
            hsvColor4.Hue = 9;
            hsvColor4.Saturation = 93;
            hsvColor4.Value = 98;
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
            //if (!isExistedLayer)
            //{
                //m_application.MapControl.AddLayer(ditchLayer, 0);
            //}
            m_application.MapControl.AddLayer(ditchLayer, 0);
            m_application.MapControl.Refresh(); 

        }

        INewEnvelopeFeedback fb;

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            if (fb == null && ditchClass != null)
            {
                IPoint p = m_application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb = new NewEnvelopeFeedbackClass();
                fb.Display = m_application.MapControl.ActiveView.ScreenDisplay;
                fb.Start(p);
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
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            if (fb != null)
            {
                IEnvelope env = fb.Stop();
                fb = null;
                int s = -1;
                if (Shift == 1)
                {
                    s = 1;
                }
                SpatialFilterClass sf = new SpatialFilterClass();
                sf.Geometry = env;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor fCursor = ditchClass.Update(sf, true);
                IFeature feature = null;
                while ((feature = fCursor.NextFeature()) != null)
                {
                    //object v = feature.get_Value(_GenUsed2);
                    if (s == -1)
                    {
                        feature.set_Value(_GenUsed2, 1);
                        fCursor.UpdateFeature(feature);
                    }
                    else
                    {
                        feature.set_Value(_GenUsed2, -1);
                        fCursor.UpdateFeature(feature);
                    }
                }
                fCursor.Flush();
                m_application.MapControl.Refresh();
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
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = "_GenUsed<0";
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
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace BuildingGen
{
    public class SelectPoolsByArea : BaseGenTool
    {
        GLayerInfo waterLayer;
        SimpleFillSymbolClass sfs;
        SimpleMarkerSymbolClass sms;
        IFeatureClass lineFC;
        public SelectPoolsByArea()
        {
            base.m_category = "GWater";
            base.m_caption = "选取湖泊";
            base.m_message = "选取湖泊";
            base.m_toolTip = "按照面积选取湖泊";
            base.m_name = "ditches";
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
 
            IFeatureWorkspace FeatWS = m_application.Workspace.Workspace as IFeatureWorkspace;
            IWorkspace2 WS2 = FeatWS as IWorkspace2;

            int fieldIndex = waterClass.FindField("_GenUsed");
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "要素代码='624050'" + " " + "OR" + " " + "要素代码='621050'";
            IFeatureCursor fcur;
            fcur = waterClass.Update(qf, false);
            IFeature feat;
            WaitOperation w = m_application.SetBusy(true);
            int d = waterClass.FeatureCount(null);
            while ((feat = fcur.NextFeature()) != null)
            {
                w.Step(d);
                IArea area = feat.Shape as IArea;
                if (area.Area < 400)
                {
                    feat.set_Value(fieldIndex, -5);
                    feat.Store();
                }
            }
            m_application.SetBusy(false);
            m_application.MapControl.Refresh();
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
    }
}

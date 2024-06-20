using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesGDB;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class VegaGbUpdate : SMGICommand
    {
        public VegaGbUpdate()
        {
            m_caption = "植被面GB更新";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            var frm = new VegaGbUpdateForm();
            if (frm.ShowDialog() != DialogResult.OK) return;

            //获取植被面
            var vega = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == frm.VegaName)).FirstOrDefault();
            var vegp = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == frm.VegpName)).FirstOrDefault();

            var vegaFc = ((IFeatureLayer)vega).FeatureClass;
            var vegpFc = ((IFeatureLayer)vegp).FeatureClass;
            var vegaGb = vegaFc.Fields.FindField("GB");
            var vegpGb = vegpFc.Fields.FindField("GB");
            if (vegaGb == -1 || vegpGb == -1) return;

            m_Application.EngineEditor.StartOperation();
            var vegaCu = vegaFc.Search(null, false);
            IFeature vegaFe;
            while ((vegaFe = vegaCu.NextFeature()) != null)
            {
                var filter = new SpatialFilterClass { Geometry = vegaFe.Shape, SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects };
                var vegpCu = vegpFc.Search(filter, false);
                var vegpFe = vegpCu.NextFeature();
                if (vegpFe != null)
                {
                    vegaFe.Value[vegaGb] = vegpFe.Value[vegpGb];
                    vegaFe.Store();
                }
                Marshal.ReleaseComObject(vegpCu);
            }

            m_Application.EngineEditor.StopOperation("植被面GB更新");
            m_Application.ActiveView.Refresh();
            MessageBox.Show("处理完成！");
        }
    }
}

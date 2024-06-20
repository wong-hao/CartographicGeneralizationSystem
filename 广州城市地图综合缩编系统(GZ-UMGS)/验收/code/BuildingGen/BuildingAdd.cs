using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace BuildingGen {
    public class BuildingAdd : BaseGenCommand {
        public BuildingAdd() {
            base.m_category = "GBuilding";
            base.m_caption = "补回小建筑";
            base.m_message = "补回小建筑";
            base.m_toolTip = "补充工具\n对之前删除的小面积建筑补回";
            base.m_name = "BuildingAdd";
        }
        public override bool Enabled {
            get {
                return m_application.Workspace != null;
            }
        }

        public override void OnClick() {
            if (System.Windows.Forms.MessageBox.Show("是否对全图进行小建筑提取操作", "请确认耗时操作", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) {
                return;
            }
            GLayerInfo layer = null;
            foreach (GLayerInfo info in m_application.Workspace.LayerManager.Layers) {
                if (info.LayerType == GCityLayerType.建筑物
                    && info.OrgLayer != null
                    ) {
                    layer = info;
                }
            }
            if (layer == null) {
                System.Windows.Forms.MessageBox.Show("没有找到建筑物图层。");
                return;            
            }
            WaitOperation wo = m_application.SetBusy(true);
            IFeatureClass fc = (layer.Layer as IFeatureLayer).FeatureClass;
            IFeatureClass orgFc = (layer.OrgLayer as IFeatureLayer).FeatureClass;
            IFeatureCursor fCursor = orgFc.Search(null, true);
            IFeatureCursor updateCursor = fc.Insert(true);
            IFeature orgFeature = null;
            double minArea = (double)m_application.GenPara["建筑物最小上图面积"];
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int wCount = fc.FeatureCount(null);
            while ((orgFeature =  fCursor.NextFeature())!=null) {
                wo.Step(wCount);
                if ((orgFeature.Shape as IArea).Area > minArea) {
                    continue;
                }
                sf.Geometry = orgFeature.Shape;
                if (fc.FeatureCount(sf) > 0)
                    continue;

                IFeatureBuffer fb = fc.CreateFeatureBuffer();
                for (int i = 0; i < fb.Fields.FieldCount; i++) {
                    IField field = fb.Fields.get_Field(i);
                    if (!field.Editable)
                        continue;
                    int index = orgFeature.Fields.FindField(field.Name);
                    if (index == -1)
                        continue;
                    fb.set_Value(i, orgFeature.get_Value(index));
                }
                updateCursor.InsertFeature(fb);
            }
            updateCursor.Flush();
            m_application.MapControl.Refresh();
            m_application.SetBusy(false);
        }

    }

}

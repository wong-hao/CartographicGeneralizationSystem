using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.GeneralEdit
{
    public class FeatureMergeTool : SMGICommand
    {
        public FeatureMergeTool()
        {
            m_caption = "要素合并";
        }
        private void AutoProcess()
        {
            var editor = m_Application.EngineEditor;

            using (var sff = new SelectFeatureForm(m_Application.MapControl.ActiveView))
            {
                sff.Text = "要素选择"; 
                if (sff.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                
                FeatureWarp w = sff.FeaturesListBox.SelectedItem as FeatureWarp;
                if (w == null)
                    return;

                editor.StartOperation();
                try
                {
                    List<IFeature> delFeList = new List<IFeature>();
                    for (int i = 0; i < sff.FeaturesListBox.Items.Count; i++)
                    {
                        if (i == sff.FeaturesListBox.SelectedIndex)
                        {
                            continue;
                        }
                        var f = (sff.FeaturesListBox.Items[i] as FeatureWarp).Feature;
                        if (w.Feature.Shape.GeometryType == f.Shape.GeometryType && w.Feature.Shape.GeometryType != esriGeometryType.esriGeometryPoint)
                        {
                            IGeometry geo = w.Feature.ShapeCopy;
                            
                            ITopologicalOperator feaTopo = geo as ITopologicalOperator;
                            feaTopo.Simplify();

                            //对被合并的f.ShapeCopy进行Simplify
                            IGeometry fGeo = f.ShapeCopy;
                            (fGeo as ITopologicalOperator).Simplify();

                            geo = feaTopo.Union(fGeo);

                            feaTopo.Simplify();

                            w.Feature.Shape = geo;
                            w.Feature.Store();

                            delFeList.Add(f);
                        }

                    }

                    foreach (var f in delFeList)
                    {
                        f.Delete();
                    }

                    editor.StopOperation("合并要素");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                    m_Application.EngineEditor.AbortOperation();
                }
            }
            m_Application.MapControl.ActiveView.Refresh();
        }

        public override void OnClick()
        {
            var map = m_Application.ActiveView.FocusMap;
            
            if (map.SelectionCount < 2)
            {
                MessageBox.Show("合并要素个数不能小于2，请至少选择2个以上要素");
                return;
            }
            AutoProcess();
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null
                    && m_Application.Workspace != null
                    && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
    }
}

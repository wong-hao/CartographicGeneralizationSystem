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
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class FeatureCutTool : SMGITool
    {
        public FeatureCutTool()
        {
            m_caption = "面分割";
        }

        public override void OnClick()
        {
            var map = m_Application.ActiveView.FocusMap;
            if (map.SelectionCount ==0)
            {
                MessageBox.Show("请先选择一个要素");
            }
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

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }

            var map = m_Application.ActiveView.FocusMap;
            if (map.SelectionCount == 0)
            {
                MessageBox.Show("请先选择一个要素");
            }

            var editor = m_Application.EngineEditor;


            IGeometry trackGeometry = m_Application.MapControl.TrackLine();
            if (trackGeometry.IsEmpty)
            {
                return;
            }

            ITopologicalOperator trackTopo = trackGeometry as ITopologicalOperator;
            trackTopo.Simplify();

            editor.StartOperation();
            var selectFeas = map.FeatureSelection as IEnumFeature;
            IFeature fe = null;
            while ((fe=selectFeas.Next())!=null)
            {
                IFeatureEdit feEdit = (IFeatureEdit)fe;
                try
                {
                    var feSet = feEdit.Split(trackGeometry);
                    if (feSet != null)
                    {
                        feSet.Reset();
                    }
                }
                catch (Exception ex)
                {
                    editor.AbortOperation();
                }
            }
            Marshal.ReleaseComObject(selectFeas);
            editor.StopOperation("面合并");
            m_Application.ActiveView.Refresh();
        }
    }
}

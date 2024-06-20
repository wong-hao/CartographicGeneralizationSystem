using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class SynchornPolygonCut : SMGITool
    {
        public SynchornPolygonCut()
        {
            m_caption = "面分割";
        }

        public override void OnClick()
        {
            var map = m_Application.ActiveView.FocusMap;
            if (map.SelectionCount == 0)
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
            IFeature fea = null;
            while ((fea = selectFeas.Next()) != null)
            {
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).Name == fea.Class.AliasName;

                })).ToArray();
                IGeoFeatureLayer geoFealyr =  lyrs[0] as IGeoFeatureLayer;
                IFeatureEdit feEdit = (IFeatureEdit)fea;
                try
                {
                    var feSet = feEdit.Split(trackGeometry);
                    if (feSet != null)
                    {
                        if (geoFealyr.Renderer is IRepresentationRenderer)
                        {
                            IMapContext mctx = new MapContextClass();
                            mctx.Init((geoFealyr.FeatureClass as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFealyr.AreaOfInterest);
                            var rpc = (geoFealyr.Renderer as IRepresentationRenderer).RepresentationClass;
                            feSet.Reset();
                            while (true)
                            {
                                IFeature fe = feSet.Next() as IFeature;
                                if (fe == null)
                                {
                                    break;
                                } 
                                IRepresentation p = rpc.GetRepresentation(fe, mctx);
                                bool over = p.HasShapeOverride; 
                                if (over)
                                {
                                    //p.RemoveShapeOverride();
                                    IGeometry intersPoints = (p.Shape as ITopologicalOperator).Intersect(p.Feature.Shape, esriGeometryDimension.esriGeometry2Dimension);
                                    p.Shape = intersPoints;
                                    p.UpdateFeature();
                                    p.Feature.Store();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    editor.AbortOperation();
                }
            }
            Marshal.ReleaseComObject(selectFeas);
            editor.StopOperation("面分割");
            m_Application.ActiveView.Refresh();
        }
    }
}

using System;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.GeneralEdit
{
    public class SynchornMultiToSingle : SMGICommand
    {
        public SynchornMultiToSingle()
        {
            m_caption = "同步打散要素";
            m_toolTip = "打散选中要素";
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
            var map = m_Application.Workspace.Map;
            if (map.SelectionCount == 0) return;

            var fes = (IEnumFeature)map.FeatureSelection;
            fes.Reset();
            IFeature fe = null;
            m_Application.EngineEditor.StartOperation();
            while ((fe = fes.Next()) != null)
            {
                var layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name == fe.Class.AliasName)).FirstOrDefault();
                if (layer == null || !layer.Visible || !(layer is IFeatureLayer)) continue;
                IGeoFeatureLayer geoFealyr = layer as IGeoFeatureLayer;
                var fc = ((IFeatureLayer)layer).FeatureClass;

                if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolygon) //面打散
                {
                    var po = (IPolygon4)fe.ShapeCopy;
                    var gc = (IGeometryCollection)po.ConnectedComponentBag;
                    IGeometryCollection gcr = null;
                    if (gc.GeometryCount <= 1) continue;
                    if (geoFealyr.Renderer is IRepresentationRenderer)
                    {
                        IMapContext mctx = new MapContextClass();
                        mctx.Init((geoFealyr.FeatureClass as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFealyr.AreaOfInterest);
                        var rpc = (geoFealyr.Renderer as IRepresentationRenderer).RepresentationClass;
                        IRepresentation p = rpc.GetRepresentation(fe, mctx);
                        bool over = p.HasShapeOverride;
                        if (over)
                        {
                            gcr = (p.ShapeCopy as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                        }
                        for (var i = 1; i < gc.GeometryCount; i++)
                        {
                            var fci = fc.Insert(true);
                            var fb = fc.CreateFeatureBuffer();
                            fb = (IFeatureBuffer)fe;
                            fb.Shape = gc.Geometry[i];
                            object feid = fci.InsertFeature(fb);
                            fci.Flush();
                            if (over)
                            {
                                var newfe = fc.GetFeature(int.Parse(feid.ToString()));
                                IRepresentation p1 = rpc.GetRepresentation(newfe, mctx);
                                p1.RemoveShapeOverride();
                                p1.Shape = gcr.get_Geometry(i);
                                p1.UpdateFeature();
                                p1.Feature.Store();
                            }
                        }
                        fe.Shape = gc.Geometry[0];
                        fe.Store();
                        if (over)
                        {
                            p.RemoveShapeOverride();
                            p.Shape = gcr.get_Geometry(0);
                            p.UpdateFeature();
                            p.Feature.Store();
                        }
                    }
                }
                else if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline) //线打散
                {
                    var gc = (IGeometryCollection)fe.ShapeCopy;
                    IGeometryCollection gcr=null;
                    if (gc.GeometryCount <= 1) continue;
                    if (geoFealyr.Renderer is IRepresentationRenderer)
                    {
                        IMapContext mctx = new MapContextClass();
                        mctx.Init((geoFealyr.FeatureClass as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFealyr.AreaOfInterest);
                        var rpc = (geoFealyr.Renderer as IRepresentationRenderer).RepresentationClass;
                        IRepresentation p = rpc.GetRepresentation(fe, mctx);
                        bool over = p.HasShapeOverride;
                        if (over)
                        {
                            gcr = (IGeometryCollection)p.ShapeCopy;
                        }
                        for (var i = 1; i < gc.GeometryCount; i++)
                        {
                            var fci = fc.Insert(true);
                            var fb = fc.CreateFeatureBuffer();
                            fb = (IFeatureBuffer)fe;
                            IPointCollection pc = new PolylineClass();
                            pc.AddPointCollection((IPointCollection)gc.Geometry[i]);
                            fb.Shape = pc as IPolyline;
                            object feid = fci.InsertFeature(fb);
                            fci.Flush();
                            if (over)
                            {
                                var newfe = fc.GetFeature(int.Parse(feid.ToString()));
                                IRepresentation p1 = rpc.GetRepresentation(newfe, mctx);
                                //IPointCollection pc2 = new PolylineClass();
                                //pc2.AddPointCollection((IPointCollection)gcr.Geometry[i]);
                                //p1.RemoveShapeOverride();
                                //p1.Shape = pc2 as IPolyline;
                                //p1.UpdateFeature();
                                ModifyOverride(newfe, p1);
                            }
                        }IPointCollection pl = new PolylineClass();
                        pl.AddPointCollection((IPointCollection)gc.Geometry[0]);
                        fe.Shape = pl as IPolyline;
                        fe.Store();
                        if (over)
                        {
                            //IPointCollection pc3 = new PolylineClass();
                            //pc3.AddPointCollection((IPointCollection)gcr.Geometry[0]);
                            //p.RemoveShapeOverride();
                            //p.Shape = pc3 as IPolyline;
                            //p.UpdateFeature();
                            ModifyOverride(fe, p);
                        }
                    }
                }
            }
            m_Application.EngineEditor.StopOperation("打散选中要素");
            m_Application.ActiveView.Refresh();
        }
        public void ModifyOverride(IFeature fe, IRepresentation rep)
        {
            IPolyline Overridepolyline = rep.ShapeCopy as IPolyline;
            IPolyline fepolyline = fe.ShapeCopy as IPolyline;
            IPolyline tempShape = (rep.ShapeCopy as ITopologicalOperator).Intersect(fepolyline, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            //tempShape2 = (rep.ShapeCopy as ITopologicalOperator).Difference(fepolyline) as IPolyline;
            IRelationalOperator relationalOperator = tempShape as IRelationalOperator;//好接口
            if (relationalOperator.Equals(fepolyline))
            {
                rep.RemoveShapeOverride();
                rep.Shape = tempShape;
                rep.UpdateFeature();
                rep.Feature.Store();
            }
            //else
            //{

            //}
        }
    }
}
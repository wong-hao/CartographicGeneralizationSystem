using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.MapGeneralization
{
    public class TestPlanarGraphTool : SMGITool
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null
                    && GetLayer() != null;
            }
        }

        IFeatureLayer GetLayer()
        {
            var ls = m_Application.Workspace.LayerManager.GetLayer((l) =>
            {
                return l is IFeatureLayer
                    && m_Application.Workspace.EsriWorkspace.PathName
                    == ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName
                    && (l as IFeatureLayer).FeatureClass.ShapeType ==  esriGeometryType.esriGeometryPolyline;
            });
            return ls.FirstOrDefault() as IFeatureLayer;
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;
            var layer = GetLayer();
            if (layer == null)
                return;

            var range = m_Application.MapControl.TrackPolygon();
            if (range == null)
                return;

            ISpatialFilter sf = new SpatialFilterClass();

            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            PlanarGraph pg = new PlanarGraph(new PointComparer { E = 0.01});
            var cursor = layer.Search(sf, true);
            
            IFeature fe = null;
            while ((fe = cursor.NextFeature()) != null)
            {
                pg.AddLine(fe.Shape as IPolyline,fe.OID);
            }

            var gc = m_Application.Workspace.Map as IGraphicsContainer;
            var fc = layer.FeatureClass;
            pg.CalFace();
            pg.CalConnectGraph();
            foreach (var face in pg.Faces)
            {
                var p = face.CalPolygon(oid =>
                {
                    return fc.GetFeature(oid).ShapeCopy as IPointCollection;
                });

                if (face != pg.LeftEdgeOfConnectGraph(face.ChildGraphIndex).RightFace)
                {
                    var el = new PolygonElementClass();
                    el.Geometry = p;

                    gc.AddElement(el,100);
                }
            }
            m_Application.MapControl.Refresh();
        }
    }
}

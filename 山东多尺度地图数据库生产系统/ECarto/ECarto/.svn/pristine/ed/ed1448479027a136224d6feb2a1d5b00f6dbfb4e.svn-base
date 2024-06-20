using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;

namespace SMGI.Plugin.MapGeneralization
{
    public class CanalDangleDeleteTool : SMGITool
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null
                    && GetLayer() != null && m_Application.EngineEditor.EditState== esriEngineEditState.esriEngineStateEditing;
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


            //1.建图，获得节点和边的几何
            IFeature fe = null;
            while ((fe = cursor.NextFeature()) != null)
            {
                pg.AddLine(fe.Shape as IPolyline,fe.OID);
            }
            pg.CalFace();
            pg.CalConnectGraph();
            //var gc = m_Application.Workspace.Map as IGraphicsContainer;
            //var fc = layer.FeatureClass;
            //foreach (var node in pg.LeftNodeOfConnectGraph.Values)
            //{
            //    var f = fc.GetFeature(node.Edge.CommonIndex);
            //    var el = new MarkerElementClass();
            //    el.Geometry = (f.Shape as IPolyline).FromPoint;
            //    gc.AddElement(el, 100);
            //}

            //2.构建Stroke连接，依次遍历每一个弧段，根据相邻弧段夹角划入唯一的Stroke
            //pg.ConstructStroke();
            
            //3.遍历所有NODE,找出悬挂边
            var gc = m_Application.Workspace.Map as IGraphicsContainer;
            var fc = layer.FeatureClass;
            var dangles=pg.FindDangleEdges();
            foreach (var item in dangles)
            {
                var f = fc.GetFeature(item.CommonIndex);
                var el = new LineElementClass();
                el.Geometry = f.Shape;
                gc.AddElement(el,100);
            }
            m_Application.MapControl.Refresh();
        }
    }
}

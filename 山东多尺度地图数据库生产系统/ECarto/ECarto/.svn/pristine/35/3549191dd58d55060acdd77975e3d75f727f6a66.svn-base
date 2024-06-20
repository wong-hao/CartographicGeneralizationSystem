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
    public class CanalStrokeTool : SMGITool
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


            //2.构建Stroke连接，依次遍历每一个弧段，根据相邻弧段夹角划入唯一的Stroke
            pg.ConstructStroke();
            
            //3.遍历所有沟渠，对SID赋值
            var fc = layer.FeatureClass;
            m_Application.EngineEditor.StartOperation();
            pg.WriteStroke(fc,"SID");
            m_Application.EngineEditor.StopOperation("构建Stroke");

            //var gc = m_Application.Workspace.Map as IGraphicsContainer;
            //var fc = layer.FeatureClass;
            //pg.CalFace();
            //pg.CalConnectGraph();
            //foreach (var face in pg.Faces)
            //{
            //    var p = face.CalPolygon(oid =>
            //    {
            //        return fc.GetFeature(oid).ShapeCopy as IPointCollection;
            //    });

            //    if (face != pg.LeftEdgeOfConnectGraph(face.ChildGraphIndex).RightFace)
            //    {
            //        var el = new PolygonElementClass();
            //        el.Geometry = p;

            //        gc.AddElement(el,100);
            //    }
            //}
            m_Application.MapControl.Refresh();
        }
    }
}

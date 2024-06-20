using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.MapGeneralization
{
    public class CanalSelectToolWithMinStroke : SMGITool
    {
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null
                    && GetLayer() != null
                    && m_Application.EngineEditor.EditState== esriEngineEditState.esriEngineStateEditing;
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
            
            var gc = m_Application.Workspace.Map as IGraphicsContainer;
            var fc = layer.FeatureClass;
            ISpatialFilter sf = new SpatialFilterClass();

            sf.Geometry = range;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            PlanarGraph pg = new PlanarGraph(new PointComparer { E = 0.01});
            var cursor = layer.Search(sf, true);
            
            //1.建图
            IFeature fe = null;
            while ((fe = cursor.NextFeature()) != null)
            {
                pg.AddLine(fe.Shape as IPolyline,fe.OID);
            }

            //2、初始化选取，去悬挂
            //var dangles=pg.FindDangleEdges();
            ////m_Application.EngineEditor.StartOperation();
            //foreach (var item in dangles)
            //{
            //    var f = fc.GetFeature(item.CommonIndex);
            //    if ((f.Shape as IPolyline).Length<300)
            //    {
            //        var el = new LineElementClass();
            //        el.Geometry = f.Shape;
            //        gc.AddElement(el, 100);
            //    }

            //    //f.Delete();
            //}
           // m_Application.EngineEditor.StopOperation("沟渠初始化");

            //4、结合STOKE、FACE，最终选取
            SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 255;
            rgb.Green = 0;
            rgb.Blue = 0;
            sls.Color = rgb;

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
                     //if ((p as IArea).Area < 500000)
                    {
                        //根据Edge长度，计算组成Face所有Edge的瓜分面积
                        List<Edge> faceEdges = new List<Edge>();

                        var currentEdge = face.Edge;
                        double totalLength = 0;
                        while (currentEdge.RightFace == face)
                        {
                            double length=(fc.GetFeature(currentEdge.CommonIndex).Shape as IPolyline).Length;
                            totalLength += length;
                            currentEdge.Length = length;
                            faceEdges.Add(currentEdge);
                            currentEdge = currentEdge.Next;
                            if (currentEdge==face.Edge)
                            {
                                break;
                            }
                        }

                        foreach (var edgeItem in faceEdges)
                        {
                            edgeItem.Area = (p as IArea).Area * edgeItem.Length / totalLength;
                        }
                        //var e2 = new LineElementClass();
                        //e2.Geometry = fc.GetFeature(face.Edge.CommonIndex).Shape;
                        //e2.Symbol = sls;
                        //gc.AddElement(e2, 101);

                        //var el = new PolygonElementClass();
                        //el.Geometry = p;
                        //gc.AddElement(el, 100);
                    }
                }
            }

            //2、构建Stroke
            pg.ConstructStroke();
            //计算所有Sroke的总长度、总面积
            pg.CalcStroke();
            //将stroke按总面积排序
            double totalArea = 0;
            List<Stroke> strokeList = new List<Stroke>();
            foreach (var stroke in pg.StrokeDic.Values)
            {
                if (stroke.Area==0)
                {
                    continue;
                }
                bool flag=false;
                foreach (var ee in stroke.Edges)
                {
                    if (ee.Area==0)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    continue;
                }

                totalArea += stroke.Area;
                strokeList.Add(stroke);
            }
            strokeList.Sort(StrokeCMP);

            List<Face> lockFaces = new List<Face>();
            m_Application.EngineEditor.StartOperation();
            foreach (var sk in strokeList)
            {
                bool hasLock = false;
                foreach (var ee in sk.Edges)
                {
                    if (lockFaces.Contains(ee.RightFace)||
                         ee.RightFace == pg.LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace||
                         ee.Twin.RightFace == pg.LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace)
                    {
                        hasLock = true;
                        break;
                    }
                }
                if (hasLock)
                {
                    continue;
                }

                foreach (var ee in sk.Edges)
                {

                    var nextEE = ee.Next;
                    var preTwin = ee.Twin.Prev;

                    nextEE.Prev = preTwin;
                    preTwin.Next = nextEE;

                    var preEE = ee.Prev;
                    var nextTwin = ee.Twin.Next;
   

                    preEE.Next = nextTwin;
                    nextTwin.Prev = preEE;
                    nextEE.RightFace.Edge = nextEE;
                    pg.Faces.Remove(nextTwin.RightFace);                    
                    var curTwin=nextTwin;
                    while (curTwin.RightFace != nextEE.RightFace)
                    {
                        curTwin.RightFace = nextEE.RightFace;
                        curTwin = curTwin.Next;
                    }


                    pg.Edges.Remove(ee);
                    pg.Edges.Remove(ee.Twin);

                    var p = nextEE.RightFace.CalPolygon(oid =>
                    {
                        return fc.GetFeature(oid).ShapeCopy as IPointCollection;
                    });

                    if (Math.Abs((p as IArea).Area)>750000)
                    {
                        lockFaces.Add(nextEE.RightFace);
                        //var el = new PolygonElementClass();
                        //el.Geometry = p;
                        //gc.AddElement(el, 100);
                    }

     

                    fc.GetFeature(ee.CommonIndex).Delete();
                }
            }
            m_Application.EngineEditor.StopOperation("沟渠选取");
            //根据选取比例，按面积依次删除Stroke,删除Stroke的同时，将相邻面合并，同时更新平面图
            //double ratio = 0.7;
            //double lastArea = totalArea;
            //m_Application.EngineEditor.StartOperation();
            //foreach (var sk in strokeList)
            //{
            //    lastArea -= sk.Area;
            //    if (lastArea/totalArea<ratio)
            //    {
            //        break;
            //    }
            //    else
            //    {
            //        foreach (var ee in sk.Edges)
            //        {
            //            //fc.GetFeature(ee.CommonIndex).Delete();
            //            var e2 = new LineElementClass();
            //            e2.Geometry = fc.GetFeature(ee.CommonIndex).Shape;
            //            e2.Symbol = sls;
            //            gc.AddElement(e2, 101);
            //        }
            //    }
            //}
            //m_Application.EngineEditor.StopOperation("沟渠选取");
 
            //foreach (var strokeList in pg.StrokeDic.Values)
            //{
            //    foreach (var edge in strokeList)
            //    {

            //        var rightPolygon = edge.RightFace.CalPolygon(oid =>
            //        {
            //            return fc.GetFeature(oid).ShapeCopy as IPointCollection;
            //        });

            //        if (edge.RightFace != pg.LeftEdgeOfConnectGraph(edge.RightFace.ChildGraphIndex).RightFace)
            //        {
            //            if ((rightPolygon as IArea).Area>500000)
            //            {
            //                var el = new PolygonElementClass();
            //                el.Geometry = rightPolygon;
            //                gc.AddElement(el, 100);

            //                var e2 = new LineElementClass();
            //                e2.Geometry = fc.GetFeature(edge.CommonIndex).Shape;
            //                e2.Symbol = sls;
            //                gc.AddElement(e2, 101);
            //            }

            //        }

            //        //if (edge.RightFace!=null && edge.LeftFace!=null)
            //        //{
            //        //    var rightPolygon = edge.RightFace.CalPolygon(oid =>
            //        //    {
            //        //        return fc.GetFeature(oid).ShapeCopy as IPointCollection;
            //        //    });

            //        //    var leftPolygon = edge.LeftFace.CalPolygon(oid =>
            //        //    {
            //        //        return fc.GetFeature(oid).ShapeCopy as IPointCollection;
            //        //    });

            //        //    if (edge.RightFace != pg.LeftEdgeOfConnectGraph(edge.RightFace.ChildGraphIndex).RightFace
            //        //        && edge.LeftFace != pg.LeftEdgeOfConnectGraph(edge.LeftFace.ChildGraphIndex).LeftFace)
            //        //    {
            //        //        if ((rightPolygon as IArea).Area+(rightPolygon as IArea).Area>800000)
            //        //        {
            //        //            var el = new LineElementClass();
            //        //            el.Geometry = fc.GetFeature(edge.CommonIndex).Shape;
            //        //            el.Symbol = sls;
            //        //            gc.AddElement(el, 100);
            //        //        }
            //        //    }
            //        //}
            //    }
            //}
            m_Application.MapControl.Refresh();
        }

        public int StrokeCMP(Stroke s1, Stroke s2)
        {
            try
            {
                return s1.Area.CompareTo(s2.Area);
            }
            catch
            {
                return 0;
            }
        }
    }
}

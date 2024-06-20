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
using System.Windows.Forms;
namespace SMGI.Plugin.MapGeneralization
{
    /// <summary>
    /// 沟渠选取工具：end
    /// </summary>
    public class CanalSelectTool : SMGITool
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
                     && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "HYDL"
                    && (l as IFeatureLayer).FeatureClass.ShapeType ==  esriGeometryType.esriGeometryPolyline;
            });
            return ls.FirstOrDefault() as IFeatureLayer;
        }


        IFeatureLayer hydlLayer = null;
        IFeatureClass hydlFcl = null;
        public override void OnClick()
        {
            hydlLayer = GetLayer();
            FrmCanalSet frm = new FrmCanalSet();
            frm.ShowDialog();
        }
        public override void OnKeyUp(int keyCode, int shift)
        {
           if (keyCode == (int)Keys.Space)
            {
                FrmCanalSet frm = new FrmCanalSet();
                frm.ShowDialog();
            }
        }
        Dictionary<int, int> deleteFeatures = new Dictionary<int, int>();

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            
            if (button != 1)
                return;
            deleteFeatures.Clear();

            if (hydlLayer == null)
                return;
            var layer = hydlLayer;
            var fc = layer.FeatureClass;
            var range = m_Application.MapControl.TrackPolygon();
            if (range == null)
                return;
       
           
            SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 255;
            rgb.Green = 0;
            rgb.Blue = 0;
            sls.Color = rgb;
            using (WaitOperation wo = m_Application.SetBusy())
            {


                #region 1.建平面图

                var gc = m_Application.Workspace.Map as IGraphicsContainer;

                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = range;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                PlanarGraph pg = new PlanarGraph(new PointComparer { E = 0.01 });
                var cursor = layer.Search(sf, true);

                IFeature fe = null;
                while ((fe = cursor.NextFeature()) != null)
                {
                    pg.AddLine(fe.Shape as IPolyline, fe.OID);
                }
                #endregion

                #region 2.第一次去悬挂，去小端头，主要为了避免干扰构STROKE
                var dangles = pg.FindDangleEdges();
                foreach (var edge in dangles)
                {
                    var f = fc.GetFeature(edge.CommonIndex);
                    if ((f.Shape as IPolyline).Length < PlanarGraph.DANGLELENGTH * 0.2)
                    {
                        //逆时针转一圈，只有自己
                        if (edge.CCWEdge == edge)
                        {
                            PointWarp pt = new PointWarp(edge.From.Point);
                            pg.Nodes.Remove(pt);
                            pg.Connect(edge.Twin.Prev, edge.Next);
                        }
                        else
                        {
                            PointWarp pt = new PointWarp(edge.To.Point);
                            pg.Nodes.Remove(pt);
                            pg.Connect(edge.Prev, edge.Twin.Next);
                        }

                        //维护边
                        //pg.Connect(edge.Prev,edge.Next);
                        //pg.Connect(edge.Twin.Prev, edge.Twin.Next);

                        pg.Edges.Remove(edge);
                        pg.Edges.Remove(edge.Twin);

                        deleteFeatures[f.OID] = f.OID;
                    }

                }
                #endregion

                #region 3.构面、连接子图
                pg.CalFace();
                pg.CalConnectGraph();
                #endregion

                #region 4.计算组成面的所有Edge的瓜分面积,同时为非边界的FACE赋面积
                foreach (var edge in pg.Edges)
                {
                    var face = edge.RightFace;
                    if (face != pg.LeftEdgeOfConnectGraph(edge.ChildGraphIndex).RightFace)
                    {
                        var p = face.CalPolygon(oid =>
                        {
                            return fc.GetFeature(oid).ShapeCopy as IPointCollection;
                        });

                        var currentEdge = edge;
                        List<Edge> faceEdges = new List<Edge>();
                        double totalLength = 0;
                        do
                        {
                            double length = (fc.GetFeature(currentEdge.CommonIndex).Shape as IPolyline).Length;
                            totalLength += length;
                            currentEdge.Length = length;
                            faceEdges.Add(currentEdge);
                            currentEdge = currentEdge.Next;
                        } while (currentEdge != edge);

                        var faceArea = (p as IArea).Area;
                        face.Area = Math.Abs(faceArea);

                        foreach (var edgeItem in faceEdges)
                        {
                            edgeItem.Area = faceArea * edgeItem.Length / totalLength;
                        }

                    }
                    else
                    {
                        var currentEdge = edge;
                        do
                        {
                            double length = (fc.GetFeature(currentEdge.CommonIndex).Shape as IPolyline).Length;
                            currentEdge.Length = length;
                            currentEdge = currentEdge.Next;
                        } while (currentEdge != edge);
                    }
                }
                #endregion

                #region 5.构建Stroke

                //5.1 构建Stroke,角度阈值40度，约弧度0.7，若有多条Edge满足，则角度最小优先
                pg.ConstructStroke();
                //5.2 计算所有Sroke的总长度、总面积、EdgeList、FaceList
                pg.CalcStroke();
                #endregion

                #region 6.第二次去悬挂，去更长一点的端头，为了防止选取过后出现过多的孤立线段,同时对已有的STROKE集合进行过滤:边界与悬挂的Stroke舍弃
                Dictionary<int, Stroke> skSelectDic = new Dictionary<int, Stroke>();
                List<int> dangleStrokeList = new List<int>();
                foreach (var sid in pg.StrokeDic.Keys)
                {
                    bool isSingle = true;
                    bool isBoundary = false;
                    foreach (var ee in pg.StrokeDic[sid].Edges)
                    {
                        if (ee.RightFace != ee.LeftFace)
                        {
                            isSingle = false;
                            if ((ee.RightFace == pg.LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace &&
                                ee.LeftFace != pg.LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace) ||
                                (ee.RightFace != pg.LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace &&
                                ee.LeftFace == pg.LeftEdgeOfConnectGraph(ee.ChildGraphIndex).RightFace))
                            {
                                isBoundary = true;
                                break;
                            }
                        }
                    }

                    if (isSingle && pg.StrokeDic[sid].Length < PlanarGraph.DANGLELENGTH)
                    {
                        foreach (var edge in pg.StrokeDic[sid].Edges)
                        {
                            if (!edge.IsFrist)
                            {
                                continue;
                            }

                            //逆时针转一圈，只有自己
                            if (edge.CCWEdge == edge)
                            {
                                PointWarp pt = new PointWarp(edge.From.Point);
                                pg.Nodes.Remove(pt);
                                pg.Connect(edge.Twin.Prev, edge.Next);
                            }
                            else
                            {
                                PointWarp pt = new PointWarp(edge.To.Point);
                                pg.Nodes.Remove(pt);
                                pg.Connect(edge.Prev, edge.Twin.Next);
                            }
                            //维护边

                            //pg.Connect(edge.Prev, edge.Next);
                            //pg.Connect(edge.Twin.Prev, edge.Twin.Next);

                            pg.Edges.Remove(edge);
                            pg.Edges.Remove(edge.Twin);

                            deleteFeatures[edge.CommonIndex] = edge.CommonIndex;
                        }
                        if (!dangleStrokeList.Contains(sid))
                        {
                            dangleStrokeList.Add(sid);
                        }
                    }
                    else
                    {
                        if (isBoundary)
                        {
                            skSelectDic[sid] = pg.StrokeDic[sid];
                        }
                    }
                }

                foreach (var sid in dangleStrokeList)
                {
                    pg.StrokeDic.Remove(sid);
                }

                foreach (var sid in skSelectDic.Keys)
                {
                    pg.LockStrokeDic[sid] = pg.StrokeDic[sid];
                }

                #endregion

                #region 7.依序合并小面到相邻面，且该EDGE的所属的STROKE全部舍掉，同时合并所有相关的面，合并最优指标：面积最小，长度最大

                //7.1 按面积对所有FACE从小排序
                pg.Faces.Sort(FaceCMP);


                //7.2 依序合并
                foreach (var face in pg.Faces)
                {
                    //过滤边界
                    if (face == pg.LeftEdgeOfConnectGraph(face.ChildGraphIndex).RightFace)
                        continue;


                    //面积大于0，小于一定阈值
                    if (face.Area > 0 && face.Area < PlanarGraph.SELECTAREA)
                    {
                        //最小指标，最优指标
                        double minIndicator = double.MaxValue;
                        //最小指标对应的EDGE
                        Edge minEdge = null;
                        var curEdge = face.Edge;
                        do
                        {
                            var curStroke = pg.StrokeDic[curEdge.StrokeID];

                            //如果该Stroke被锁住则跳过
                            if (pg.IsStrokeLocked(curStroke.ID))
                            {
                                curEdge = curEdge.Next;
                                continue;
                            }
                            ///                
                            if (curStroke.Length > PlanarGraph.DANGLENMAX)
                            {
                                curEdge = curEdge.Next;
                                continue;
                            }

                            //过滤删除后出现悬挂
                            if (pg.HasDangle(curStroke))
                            {
                                curEdge = curEdge.Next;
                                continue;
                            }

                            double strokeArea = curStroke.Area;
                            double edgeLength = curEdge.Length;
                            var indicator = Math.Sqrt(strokeArea) / edgeLength;

                            //比最小指标小，则更新
                            if (indicator < minIndicator)
                            {
                                minIndicator = indicator;
                                minEdge = curEdge;
                            }
                            curEdge = curEdge.Next;
                        } while (curEdge != face.Edge);

                        //若找到了则合并
                        if (minEdge != null)
                        {
                            foreach (var ee in pg.StrokeDic[minEdge.StrokeID].Edges)
                            {
                                if (ee.IsFrist)
                                {
                                    var newFace = pg.MergeFace(ee);
                                    if (newFace != pg.LeftEdgeOfConnectGraph(face.ChildGraphIndex).RightFace)
                                    {
                                        if (newFace.Area > PlanarGraph.SELECTAREA)
                                        {
                                            pg.LockFace(newFace);
                                        }
                                    }
                                    deleteFeatures[ee.CommonIndex] = ee.CommonIndex;
                                }

                            }
                            pg.StrokeDic.Remove(minEdge.StrokeID);
                        }
                    }
                }
                #endregion

                m_Application.EngineEditor.StartOperation();
                foreach (var oid in deleteFeatures.Keys)
                {
                    //  fc.GetFeature(oid).Delete();
                }
                m_Application.EngineEditor.StopOperation("沟渠选取");
            }
            m_Application.MapControl.Refresh();
        }

        
        public int FaceCMP(Face f1, Face f2)
        {
            try
            {
                return f1.Area.CompareTo(f2.Area);
            }
            catch
            {
                return 0;
            }
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

        public override void Refresh(int hdc)
        {
            
            if(deleteFeatures.Count>0)
            {
                IDisplay dis = new SimpleDisplayClass();

                dis.DisplayTransformation = m_Application.ActiveView.ScreenDisplay.DisplayTransformation;
                dis.DisplayTransformation.ReferenceScale = 0;
                dis.StartDrawing(hdc, -1);

                SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
                RgbColorClass c = new RgbColorClass();
                c.Red = 0;
                c.Blue = 255;
                c.Green = 0;

                sls.Color = c;
                sls.Width = 2;

                dis.SetSymbol(sls as ISymbol);
                foreach (var kv in deleteFeatures)
                {
                    var fe = hydlLayer.FeatureClass.GetFeature(kv.Key);
                    dis.DrawPolyline(fe.ShapeCopy);
                }
               

                dis.FinishDrawing();
            }
        }
    }
}

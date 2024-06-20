using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class SeniorReshapePolygonsTool : SMGITool
    {
        public SeniorReshapePolygonsTool()
        {
            m_caption = "狭长图斑修改";
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
            var editor = m_Application.EngineEditor;

            IActiveView view = m_Application.ActiveView;
            IMap map = view.FocusMap;
            IScreenDisplay screenDisplay = view.ScreenDisplay;

            if (map.SelectionCount != 1)
            {
                MessageBox.Show("必须（只能）选中一个要素！");
                return;
            }

        }
       public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button !=1)
            {
                return;
            }

            IActiveView view = m_Application.ActiveView;
            IMap map = view.FocusMap;
            IScreenDisplay screenDisplay = view.ScreenDisplay;

            if (map.SelectionCount!=1)
            {
                MessageBox.Show("必须（只能）选中一个要素！");
                return;
            }
            IEnumFeature enumFeature =map.FeatureSelection as IEnumFeature;
            IFeature pFeature = enumFeature.Next();
           
            //如果被选中
            if (pFeature!=null)
            {
                //画一个多边形，用于切割需要被融合的面
                var editor = m_Application.EngineEditor;
                IGeometry trackPolygon = m_Application.MapControl.TrackPolygon();
                if (trackPolygon.IsEmpty)
                {
                    return;
                }
                //确保多边形拓扑正确
                ITopologicalOperator trackTopo = trackPolygon as ITopologicalOperator;
                trackTopo.Simplify();

                //如果多边形为空则返回
                if (trackPolygon == null || trackPolygon.IsEmpty)
                {
                    return;
                }

                IRelationalOperator trackRel=trackPolygon as IRelationalOperator;
                
                //定一个空间条件过滤变量，用于空间筛选
                ISpatialFilter sf = new SpatialFilter();

                editor.StartOperation();

                if (!(pFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon))
                {
                    return;
                }

                IPolygon4 pg = pFeature.Shape as IPolygon4;
                if (pg.IsEmpty)
                {
                    return;
                }
                IFeatureClass fc = (pFeature.Class as IFeatureClass);
                IFeatureCursor insertCursor = fc.Insert(true);
                //如果该要素在多边形内，则直接将其删除，并将其图形融合进与其共享边最长的图版中
                if (trackRel.Contains(pg))
                {
                    pFeature.Delete();
                    UnionPolygonToMaxShareBoundary(pg, map, pFeature, sf);
                }
                else
                {
                    IGeometryBag tempGeoBag = pg.ConnectedComponentBag;
                    //如果该要素的几何为多部件要素，则循环遍历该多部件要素
                    if ((tempGeoBag as IGeometryCollection).GeometryCount > 1)
                    {
                        IGeometryCollection pgCol = tempGeoBag as IGeometryCollection;
                        for (int j = 0; j < pgCol.GeometryCount; j++)
                        {
                            IGeometry geoItem = pgCol.get_Geometry(j);
                            //如果该部件在多边形内 则将其与整个要素求异，将结果放回原始要素内保存，并将其融入周边与其拥有最大共享边的要素内
                            if (trackRel.Contains(geoItem))
                            {
                                pFeature.Shape = (pg as ITopologicalOperator).Difference(geoItem);
                                pFeature.Store();
                                UnionPolygonToMaxShareBoundary(geoItem, map, pFeature, sf);
                            }
                            else
                            {
                                IGeometry cutGeo = null;
                                IGeometryCollection pgCutCol = null;
                                //利用多边形的边界去切割该部件，得到被切的集合
                                try
                                {
                                    pgCutCol = (geoItem as ITopologicalOperator4).Cut2(trackTopo.Boundary as IPolyline);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                                //找到被切割集合中包含在多边形内的部分，将其保存
                                for (int i = 0; i < pgCutCol.GeometryCount; i++)
                                {
                                    if (trackRel.Contains(pgCutCol.get_Geometry(i)))
                                    {
                                        cutGeo = (pgCutCol.get_Geometry(i) as IClone).Clone() as IGeometry;
                                    }

                                }
                                //如果找到了该部分，则将其与原始图形求异并将结果保存到原始要素，且将切割的部分融入周边
                                if (cutGeo != null)
                                {
                                    pFeature.Shape = (pg as ITopologicalOperator).Difference(cutGeo);
                                    pFeature.Store();
                                    UnionPolygonToMaxShareBoundary(cutGeo, map, pFeature, sf);
                                }
                            }
                        }
                    }
                    else//如果为单部件
                    {
                        IGeometry cutGeo = null;
                        IGeometryCollection pgCutCol = null;
                        try
                        {
                            pgCutCol = (pg as ITopologicalOperator4).Cut2(trackTopo.Boundary as IPolyline);
                        }
                        catch (Exception)
                        {
                            return;
                        }
                        for (int i = 0; i < pgCutCol.GeometryCount; i++)
                        {
                            if (trackRel.Contains(pgCutCol.get_Geometry(i)))
                            {
                                cutGeo = (pgCutCol.get_Geometry(i) as IClone).Clone() as IGeometry;
                            }

                        }
                        if (cutGeo != null)
                        {
                            IGeometry geoDiff = (pg as ITopologicalOperator).Difference(cutGeo);
                            IGeometryBag diffGeoBag = (geoDiff as IPolygon4).ConnectedComponentBag;
                            //如果该要素的几何为多部件要素，则循环遍历该多部件要素
                            if ((diffGeoBag as IGeometryCollection).GeometryCount > 1)
                            {
                                IGeometryCollection pgCol = diffGeoBag as IGeometryCollection;
                                for (int j = 0; j < pgCol.GeometryCount; j++)
                                {
                                    IGeometry geoItem = pgCol.get_Geometry(j);
                                    IFeatureBuffer newFea = fc.CreateFeatureBuffer();
                                    newFea = pFeature as IFeatureBuffer;
                                    newFea.Shape = geoItem;
                                    insertCursor.InsertFeature(newFea);
                                }
                                pFeature.Delete();
                            }
                            else
                            {
                                pFeature.Shape = geoDiff;
                            }
                            pFeature.Store();
                            UnionPolygonToMaxShareBoundary(cutGeo, map, pFeature, sf);
                        }
                    }
                }
                insertCursor.Flush();
                Marshal.ReleaseComObject(insertCursor);
                view.Refresh();
                editor.StopOperation("高级修面");
            }
        }

       public void UnionPolygonToMaxShareBoundary(IGeometry pg, IMap map, IFeature pFeature, ISpatialFilter sf)
       {
           sf.Geometry = pg;
           sf.WhereClause = "";
           ITopologicalOperator diffGeoTopo = pg as ITopologicalOperator;
           IFeature maxFeature = null;
           double maxLength = 0;
           IFeatureClass geoFc = pFeature.Class as IFeatureClass;

           IFeatureCursor geoCursor = geoFc.Search(sf, false);
           IFeature geoFeature;
           while ((geoFeature = geoCursor.NextFeature()) != null)
           {
               if (geoFeature.OID == pFeature.OID)
               {
                   continue;
               }
               IPolygon geoPG = geoFeature.Shape as IPolygon;
               IGeometry intersBoudary = diffGeoTopo.Intersect(geoPG, esriGeometryDimension.esriGeometry1Dimension);
               if (intersBoudary.IsEmpty)
               {
                   continue;
               }
               if ((intersBoudary as IPolyline).Length > maxLength)
               {
                   maxLength = (intersBoudary as IPolyline).Length;
                   maxFeature = geoFeature;
               }
           }
           Marshal.ReleaseComObject(geoCursor);

           if (maxLength != 0 && maxFeature != null)
           {
               IGeometry maxShape = maxFeature.Shape;
               maxFeature.Shape = diffGeoTopo.Union(maxShape);
               maxFeature.Store();
           }
       }
    }
}

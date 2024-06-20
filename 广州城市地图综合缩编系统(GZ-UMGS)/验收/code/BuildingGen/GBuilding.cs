using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

namespace BuildingGen
{
    internal static class GBuilding
    {
        internal delegate bool CanAggregate(IFeature feature1, IFeature feature2);
        internal static void Aggregate(IFeatureLayer layer, double distance, CanAggregate canAggregate)
        {
            distance = Math.Abs(distance);
            IFeatureClass fc = layer.FeatureClass;
            //确定分组
            ISelectionSet set = (layer as IFeatureSelection).SelectionSet;
            if (set.Count <= 1)
            {
                throw new Exception("null selection");
            }
            IEnumIDs ids = set.IDs;
            int id = -1;
            List<List<int>> featureGroups = new List<List<int>>();
            if (canAggregate == null)
            {
                List<int> group = new List<int>();
                while ((id = ids.Next()) != -1)
                {
                    group.Add(id);
                }
                featureGroups.Add(group);
            }
            else
            {
                while ((id = ids.Next()) != -1)
                {
                    IFeature feature = fc.GetFeature(id);
                    bool can = false;
                    foreach (List<int> item in featureGroups)
                    {
                        IFeature groupFeature = fc.GetFeature(item[0]);
                        if (canAggregate(groupFeature, feature))
                        {
                            item.Add(id);
                            can = true;
                            break;
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(groupFeature);
                    }
                    if (!can)
                    {
                        List<int> group = new List<int>();
                        group.Add(id);
                        featureGroups.Add(group);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                }
            }
            foreach (List<int> group in featureGroups)
            {
                //融合起来
                IPolygon union = null;
                foreach (int item in group)
                {
                    IFeature feature = fc.GetFeature(item);
                    IPolygon poly = feature.ShapeCopy as IPolygon;
                    if (union == null)
                    {
                        union = poly;
                    }
                    else
                    {
                        union = (poly as ITopologicalOperator).Union(union) as IPolygon;
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                }
                IPolygon outer = Aggregate(union, distance);
                outer = (outer as ITopologicalOperator).Union(union) as IPolygon;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(union);
                IGeometryCollection outBag = (outer as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                for (int i = 0; i < outBag.GeometryCount; i++)
                {
                    IGeometry a1 = outBag.get_Geometry(i);
                    IGeometry result = a1;
                    double maxArea = -1;
                    int maxAreaID = -1;
                    foreach (int item in group)
                    {
                        IFeature feature = fc.GetFeature(item);
                        if (!(feature.Shape as IRelationalOperator).Disjoint(a1))
                        {
                            result = (result as ITopologicalOperator).Union(feature.Shape);
                            double area = (feature.Shape as IArea).Area;
                            if (maxArea < area)
                            {
                                if (maxAreaID != -1)
                                {
                                    IFeature f = fc.GetFeature(maxAreaID);
                                    f.Delete();
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                                }
                                maxAreaID = item;
                                maxArea = area;
                            }
                            else
                            {
                                feature.Delete();
                            }
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                    }
                    IFeature maxFeature = fc.GetFeature(maxAreaID);
                    maxFeature.Shape = result;
                    maxFeature.Store();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(maxFeature);
                }
            }
        }

        /// <summary>
        /// 融合一组多边形
        /// </summary>
        /// <param name="polygons">融合后的多边形</param>
        /// <returns></returns>
        private static IPolygon Aggregate(IPolygon polygons, double distance)
        {
            List<IPoint> intersectPoints = new List<IPoint>();
            IGeometryCollection gc = (polygons as IPolygon4).ConnectedComponentBag as IGeometryCollection;
            IPolygon buffer = null;
            for (int i = 0; i < gc.GeometryCount; i++)
            {
                IPolygon item = gc.get_Geometry(i) as IPolygon;
                IPolygon bufferItem = GBuffer.Buffer(item, distance);
                if (buffer == null)
                {
                    buffer = bufferItem;
                    continue;
                }
                IGeometry ips = (bufferItem as ITopologicalOperator).Intersect(buffer, esriGeometryDimension.esriGeometry0Dimension);
                if (ips is IPoint)
                {
                    intersectPoints.Add(ips as IPoint);
                }
                else if (ips is IMultipoint)
                {
                    for (int j = 0; j < (ips as IPointCollection).PointCount; j++)
                    {
                        intersectPoints.Add((ips as IPointCollection).get_Point(j));
                    }
                }
                else
                {
                }
                buffer = (bufferItem as ITopologicalOperator).Union(buffer) as IPolygon;
            }
            buffer.Generalize(distance * 0.1);
            IPoint hitPoint = new PointClass();
            double hitdistance = 0;
            int hitPartIndex = -1;
            int hitSegIndex = -1;
            bool hitRightSite = false;
            GeometryEnvironmentClass ge = new GeometryEnvironmentClass();

            //处理相交点上的尖角
            foreach (IPoint ip in intersectPoints)
            {
                if ((buffer as IHitTest).HitTest(ip, 0.1, esriGeometryHitPartType.esriGeometryPartVertex, hitPoint, ref hitdistance, ref hitPartIndex, ref hitSegIndex, ref hitRightSite))
                {
                    ISegmentCollection hitRing = (buffer as IGeometryCollection).get_Geometry(hitPartIndex) as ISegmentCollection;
                    ISegment sn1 = hitRing.get_Segment(hitSegIndex);
                    ISegment sp1 = hitRing.get_Segment((hitSegIndex == 0) ? hitRing.SegmentCount - 1 : hitSegIndex - 1);
                    double hitAngel = ge.ConstructThreePoint(sp1.FromPoint, sn1.FromPoint, sn1.ToPoint);


                    if (Math.Abs(hitAngel) < Math.PI / 6)
                    {
                        ISegment s1 = null;
                        ISegment s2 = null;
                        if (sn1.Length > sp1.Length)
                        {
                            s1 = sn1;
                            s2 = hitRing.get_Segment(((hitSegIndex - 2) >= 0) ? (hitSegIndex - 2) : (hitRing.SegmentCount + hitSegIndex - 2));
                        }
                        else
                        {
                            s1 = sp1;
                            s2 = hitRing.get_Segment(((hitSegIndex + 1) < hitRing.SegmentCount) ? (hitSegIndex + 1) : (hitSegIndex + 1 - hitRing.SegmentCount));
                        }
                        PointClass movePoint = new PointClass();
                        movePoint.ConstructAngleIntersection(s1.FromPoint, (s1 as ILine).Angle, s2.FromPoint, (s2 as ILine).Angle);
                        IPoint[] mps = new IPoint[] { movePoint };
                        ge.ReplacePoints((hitRing as IPointCollection4), hitSegIndex, 1, ref mps);
                        if (Math.Abs((hitRing as IArea).Area) < distance)
                        {
                            (buffer as IGeometryCollection).RemoveGeometries(hitPartIndex, 1);
                        }
                    }
                }
            }

            IPolygon buffer2 = GBuffer.Buffer(buffer, -distance);
            return buffer2;
        }

        internal static IPolygon Simplify(IPolygon polygon, double distance)
        {
            distance = Math.Abs(distance);
            try
            {
                IPolygon buffer = GBuffer.Buffer(polygon, distance);

                buffer = GBuffer.Buffer(buffer, -2*distance);
                buffer = GBuffer.Buffer(buffer, distance);
                return buffer;
            }
            catch
            {
                return null;
            }
        }
    }
}

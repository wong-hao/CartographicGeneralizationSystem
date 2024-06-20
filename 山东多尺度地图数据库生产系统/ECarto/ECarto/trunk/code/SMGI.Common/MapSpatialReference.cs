using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.AutoMapping.Plugin
{
    public class MapSpatialReference   //空间参考类
    {
        private IProjectedCoordinateSystem proSpatialReference = null;  //投影坐标系
        public static  int objectcount=0; 
        public  IProjectedCoordinateSystem PCS
        {        
            get
            {
                return proSpatialReference;
            }
        }

        private IGeographicCoordinateSystem geoSpatialReference = null;  //几何坐标系
        public  IGeographicCoordinateSystem GCS
        {
            get
            {
                return geoSpatialReference;
            }
        }

        private  MapSpatialReference()  
        { 
        
        }
        /// <summary>
        /// 传入投影坐标系，每次都会在内部复制一份新的投影坐标系，同时得到其对应的几何坐标系
        /// objectcount字段用于计量当前为第几次新建的对象
        /// </summary>
        /// <param name="pcs"></param>
        public MapSpatialReference(IProjectedCoordinateSystem pcs)  
        {
            proSpatialReference = (pcs as ESRI.ArcGIS.esriSystem.IClone).Clone() as IProjectedCoordinateSystem;
            (proSpatialReference as IProjectedCoordinateSystem5).FalseEasting = falseEasting;
            geoSpatialReference=proSpatialReference.GeographicCoordinateSystem;
            objectcount++;
        }

        private double centralMeridian = 0;
        private double falseEasting = 500000;

        public IProjectedCoordinateSystem SetProjectedCoordinateSystem(double central_)
        {
            centralMeridian = central_;
            ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)proSpatialReference;
            spatialReferenceResolution.ConstructFromHorizon();
            ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)proSpatialReference;
            spatialReferenceTolerance.SetDefaultXYTolerance();
            (proSpatialReference as IProjectedCoordinateSystem5).set_CentralMeridian(true, centralMeridian);
            (proSpatialReference as IProjectedCoordinateSystem5).FalseEasting = falseEasting;
            (proSpatialReference as IProjectedCoordinateSystem5).SetDomain(-50000000, 50000000, -50000000, 50000000);
            return proSpatialReference;
        }



        
    }

    public class MarginAnnotationPoint  //封装
    {
        private double longitude = 0;
        public double Longitude
        {
            get { return longitude; }
        }
        private double latitude = 0;
        public double Latitude
        {
            get { return latitude; }
        }

        private double projected_map_x = 0;
        public double Projected_map_x
        {
            get { return projected_map_x; }
            set { projected_map_x = value; }
        }
        private double projected_map_y = 0;
        public double Projected_map_y
        {
            get { return projected_map_y; }
            set { projected_map_y = value; }
        }

        public MarginAnnotationPoint(double input_x, double input_y, ISpatialReference sr_PCS, bool isLonlat)
        {
            IPoint point_ = null;
            if (isLonlat)
            {
                longitude = input_x;
                latitude = input_y;
                point_ = new PointClass();
                ISpatialReference sr = (sr_PCS as IProjectedCoordinateSystem).GeographicCoordinateSystem as ISpatialReference;
                point_.SpatialReference = sr;
                point_.PutCoords(longitude , latitude );
                point_.Project(sr_PCS);

                projected_map_x = point_.X;
                projected_map_y = point_.Y;
            }
            else
            {
                projected_map_x = input_x;
                projected_map_y = input_y;

                point_ = new PointClass();
                ISpatialReference sr = (sr_PCS as IProjectedCoordinateSystem).GeographicCoordinateSystem as ISpatialReference;
                point_.SpatialReference = sr_PCS;
                point_.PutCoords(input_x, input_y);
                point_.Project(sr);
                longitude = point_.X;
                latitude = point_.Y;
            }
        }

        public MarginAnnotationPoint()
        {
            // TODO: Complete member initialization
        }
    }

    public class CreateSubdivision2Clip_ESRI
    {
        /// <summary>
        /// 生成分幅框的线
        /// </summary>
        /// <param name="longitudes"></param>
        /// <param name="latitudes"></param>
        /// <param name="polylines"></param>
        /// <param name="sr_"></param>
        public static void CreateGridPolylineFromBLArray(double[] longitudes,
            double[] latitudes,
            out IPolyline[] polylines,ISpatialReference sr)
        {
            if (longitudes == null || latitudes == null || longitudes.Length < 1 || latitudes.Length < 1)
            {
                polylines = null;
                return;
            }
            
            List<IPolyline> polylines_list = new List<IPolyline>();

            IPoint point_temp_min = null;
            IPoint point_temp_max = null;
            IPolyline polyline_ = null;
            System.Object missing = Type.Missing;
            for (int i = 0; i < longitudes.Length; i++)
            {
                point_temp_min = new PointClass();
                point_temp_min.SpatialReference = sr;
                point_temp_max = new PointClass();
                point_temp_max.SpatialReference = sr;

                point_temp_min.PutCoords(longitudes[i], latitudes[0]);
                point_temp_max.PutCoords(longitudes[i], latitudes[latitudes.Length - 1]);
                polyline_ = new PolylineClass();
                (polyline_ as IPointCollection).AddPoint(point_temp_min, ref missing, ref missing);
                (polyline_ as IPointCollection).AddPoint(point_temp_max, ref missing, ref missing);
                polylines_list.Add(polyline_);
            }

            for (int i = 0; i < latitudes.Length; i++)
            {
                point_temp_min = new PointClass();
                point_temp_min.SpatialReference = sr;
                point_temp_max = new PointClass();
                point_temp_max.SpatialReference = sr;

                point_temp_min.PutCoords(longitudes[0], latitudes[i]);
                point_temp_max.PutCoords(longitudes[longitudes.Length - 1], latitudes[i]);
                polyline_ = new PolylineClass();
                (polyline_ as IPointCollection).AddPoint(point_temp_min, ref missing, ref missing);
                (polyline_ as IPointCollection).AddPoint(point_temp_max, ref missing, ref missing);
                polylines_list.Add(polyline_);
            }
            polylines = polylines_list.ToArray();
        }

        /// <summary>
        /// 根据输入的经纬度数组，传回标准分幅的裁切框以及图幅号
        /// </summary>
        /// <param name="longitudes"></param>
        /// <param name="latitudes"></param>
        /// <param name="scale_"></param>
        /// <param name="three_six"></param>
        /// <param name="polygons"></param>
        /// <param name="ZoneNumber"></param>
        /// <param name="sr">传入以后会复制一份，根据需要修改中央经线，每个图幅有自己的中央经线，保证正确</param>
        public static void CreateGridPolygonFromLonLatArray(double[] longitudes,
            double[] latitudes,
            double scale_,    
            out IPolygon[] polygons, out string[] ZoneNumber,ISpatialReference sr)
        {
            if (longitudes == null || latitudes == null || longitudes.Length < 1 || latitudes.Length < 1)
            {
                polygons = null;
                ZoneNumber = null;
                return;
            }

            List<IPolygon> polygons_list = new List<IPolygon>();
            List<string> zone_numbers = new List<string>();

            IPoint point_temp_min = null;
            IPoint point_temp_es = null;
            IPoint point_temp_max = null;
            IPoint point_temp_wn = null;
            IPolygon polygon_ = null;
            IRing ring_ = null;
            System.Object missing = Type.Missing;
            MapSpatialReference mp_ = new MapSpatialReference(sr as IProjectedCoordinateSystem);
            for (int i = 0; i < longitudes.Length - 1; i++)
            {
                for (int j = 0; j < latitudes.Length - 1; j++)
                {
                 
                    string zone_No = MapSubdivision.CalculateSubdivisionNumber((long)scale_,
                        (longitudes[i] + longitudes[i + 1]) / 2,
                        (latitudes[j] + latitudes[j + 1]) / 2);

                    point_temp_min = new PointClass();
                    point_temp_min.SpatialReference = mp_.GCS;
                    point_temp_es = new PointClass();
                    point_temp_es.SpatialReference = mp_.GCS;
                    point_temp_max = new PointClass();
                    point_temp_max.SpatialReference = mp_.GCS;
                    point_temp_wn = new PointClass();
                    point_temp_wn.SpatialReference = mp_.GCS;

                    point_temp_min.PutCoords(longitudes[i], latitudes[j]);
                    point_temp_es.PutCoords(longitudes[i + 1], latitudes[j]);
                    point_temp_max.PutCoords(longitudes[i + 1], latitudes[j + 1]);
                    point_temp_wn.PutCoords(longitudes[i], latitudes[j + 1]);

                    point_temp_min.Project((ISpatialReference)mp_.PCS);
                    point_temp_es.Project((ISpatialReference)mp_.PCS);
                    point_temp_max.Project((ISpatialReference)mp_.PCS);
                    point_temp_wn.Project((ISpatialReference)mp_.PCS);

                    polygon_ = new PolygonClass();
                    polygon_.SpatialReference = (ISpatialReference)mp_.PCS;
                    ring_ = new RingClass();
                    (ring_ as IPointCollection).AddPoint(point_temp_min, ref missing, ref missing);
                    (ring_ as IPointCollection).AddPoint(point_temp_es, ref missing, ref missing);
                    (ring_ as IPointCollection).AddPoint(point_temp_max, ref missing, ref missing);
                    (ring_ as IPointCollection).AddPoint(point_temp_wn, ref missing, ref missing);
                    (ring_ as IPointCollection).AddPoint(point_temp_min, ref missing, ref missing);

                    ring_.ReverseOrientation();
                    (polygon_ as IGeometryCollection).AddGeometry(ring_ as IGeometry, ref missing, ref missing);
                    polygon_.Close();


                    polygons_list.Add(polygon_);
                    zone_numbers.Add(zone_No);
                }
            }
            polygons = polygons_list.ToArray();
            ZoneNumber = zone_numbers.ToArray();

            polygon_ = null;
            ring_ = null;
            polygons_list.Clear();
            zone_numbers.Clear();
        }

        /// <summary>
        /// 两点直接
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static IPolyline NewLineby2Pts(double x1, double y1, double x2, double y2)
        {
            IPolyline polyline_ = new PolylineClass();
            IGeometryBridge2 geometryBridge = new GeometryEnvironmentClass();

            WKSPoint[] wksPoints = new WKSPoint[2];
            WKSPoint wksPoint = new WKSPoint();
            wksPoint.X = x1;
            wksPoint.Y = y1;
            wksPoints[0] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = x2;
            wksPoint.Y = y2;
            wksPoints[1] = wksPoint;

            geometryBridge.SetWKSPoints(polyline_ as IPointCollection4, ref wksPoints);

            return polyline_;

        }
        /// <summary>
        /// 返回以两点直连线为对角线的一个矩形
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static IPolyline NewRectLineby2Pts(double x1, double y1, double x2, double y2)
        {
            IPolyline polyline_ = new PolylineClass();
            IGeometryBridge2 geometryBridge = new GeometryEnvironmentClass();

            WKSPoint[] wksPoints = new WKSPoint[5];
            WKSPoint wksPoint = new WKSPoint();
            wksPoint.X = x1;
            wksPoint.Y = y1;
            wksPoints[0] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = x2;
            wksPoint.Y = y1;
            wksPoints[1] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = x2;
            wksPoint.Y = y2;
            wksPoints[2] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = x1;
            wksPoint.Y = y2;
            wksPoints[3] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = x1;
            wksPoint.Y = y1;
            wksPoints[4] = wksPoint;

            geometryBridge.SetWKSPoints(polyline_ as IPointCollection4, ref wksPoints);

            return polyline_;


        }

        /// <summary>
        /// 四个点生成 非 矩形框
        /// </summary>
        /// <param name="bl_x"></param>
        /// <param name="bl_y"></param>
        /// <param name="br_x"></param>
        /// <param name="br_y"></param>
        /// <param name="tr_x"></param>
        /// <param name="tr_y"></param>
        /// <param name="tl_x"></param>
        /// <param name="tl_y"></param>
        /// <returns></returns>
        public static IPolyline NewMargineLineBy4Pts(double bl_x, double bl_y, double br_x, double br_y,
            double tr_x, double tr_y, double tl_x, double tl_y)
        {
            IPolyline polyline_ = new PolylineClass();
            IGeometryBridge2 geometryBridge = new GeometryEnvironmentClass();

            WKSPoint[] wksPoints = new WKSPoint[5];
            WKSPoint wksPoint = new WKSPoint();
            wksPoint.X = bl_x;
            wksPoint.Y = bl_y;
            wksPoints[0] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = br_x;
            wksPoint.Y = br_y;
            wksPoints[1] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = tr_x;
            wksPoint.Y = tr_y;
            wksPoints[2] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = tl_x;
            wksPoint.Y = tl_y;
            wksPoints[3] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = bl_x;
            wksPoint.Y = bl_y;
            wksPoints[4] = wksPoint;
            geometryBridge.SetWKSPoints(polyline_ as IPointCollection4, ref wksPoints);
            return polyline_;

        }

        /// <summary>
        /// 十字丝
        /// </summary>
        /// <param name="x_"></param>
        /// <param name="y_"></param>
        /// <param name="length_"></param>
        /// <returns></returns>
        public static IPolyline CrosslineByCenterPoint(double x_, double y_, double length_)
        {
            IPolyline polyline_ = new PolylineClass();
            IPath path_vertical = new PathClass();
            IPath path_horizontal = new PathClass();
            IGeometryBridge2 geometryBridge = new GeometryEnvironmentClass();

            WKSPoint[] wksPoints = new WKSPoint[2];
            WKSPoint wksPoint = new WKSPoint();
            wksPoint.X = x_;
            wksPoint.Y = y_ - length_;
            wksPoints[0] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = x_;
            wksPoint.Y = y_ + length_;
            wksPoints[1] = wksPoint;

            geometryBridge.SetWKSPoints(path_vertical as IPointCollection4, ref wksPoints);

            wksPoint = new WKSPoint();
            wksPoint.X = x_ - length_;
            wksPoint.Y = y_;
            wksPoints[0] = wksPoint;

            wksPoint = new WKSPoint();
            wksPoint.X = x_ + length_;
            wksPoint.Y = y_;
            wksPoints[1] = wksPoint;

            geometryBridge.SetWKSPoints(path_horizontal as IPointCollection4, ref wksPoints);

            object missing = Type.Missing;
            (polyline_ as IGeometryCollection).AddGeometry(path_horizontal as IGeometry, ref missing, ref missing);
            (polyline_ as IGeometryCollection).AddGeometry(path_vertical as IGeometry, ref missing, ref missing);

            return polyline_;
        }

        /// <summary>
        /// 基于一个点，按指定的方向和长度做一段线
        /// </summary>
        /// <param name="x_"></param>
        /// <param name="y_"></param>
        /// <param name="line_direction"></param>
        /// <param name="length_"></param>
        /// <returns></returns>
        public static IPolyline ConstructLineByDirectionandLength(double x_, double y_, LineDirection line_direction, double length_)
        {
            IPolyline polyline_ = new PolylineClass();
            IGeometryBridge2 geometryBridge = new GeometryEnvironmentClass();

            WKSPoint[] wksPoints = new WKSPoint[2];
            WKSPoint wksPoint = new WKSPoint();
            wksPoint.X = x_;
            wksPoint.Y = y_;
            wksPoints[0] = wksPoint;

            if (line_direction == LineDirection.bottom)
            {
                y_ -= length_;
            }
            else if (line_direction == LineDirection.top)
            {
                y_ += length_;
            }
            else if (line_direction == LineDirection.left)
            {
                x_ -= length_;
            }
            else
            {
                x_ += length_;
            }


            wksPoint = new WKSPoint();
            wksPoint.X = x_;
            wksPoint.Y = y_;
            wksPoints[1] = wksPoint;

            geometryBridge.SetWKSPoints(polyline_ as IPointCollection4, ref wksPoints);

            return polyline_;

        }

        /// <summary>
        /// 根据线上一点X，求Y
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x_"></param>
        /// <returns></returns>
        public static double PointY_on_Line(double x1, double y1, double x2, double y2, double x_)
        {
            if (y1 == y2)
                return y1;
            else
            {
                return (y2 - y1) * (x_ - x1) / (x2 - x1) + y1;
            }
        }
        /// <summary>
        /// 根据线上一点Y求X
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="y_"></param>
        /// <returns></returns>
        public static double PointX_on_Line(double x1, double y1, double x2, double y2, double y_)
        {
            if (x1 == x2)
                return x1;
            else
            {
                return (x2 - x1) * (y_ - y1) / (y2 - y1) + x1;
            }

        }


        public enum LineDirection
        {
            top,
            bottom,
            left,
            right
        }


    }
}

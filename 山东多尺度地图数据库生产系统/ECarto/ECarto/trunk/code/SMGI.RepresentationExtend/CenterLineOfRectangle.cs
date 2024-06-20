using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("停车场")]
    [Guid("02888a5c-e339-40b9-8e29-c89c84ccb643")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.CenterLineOfRectangle")]
    public class CenterLineOfRectangle: BaseGeographicEffect, IGeometricEffect
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            BaseGeographicEffect.GeometricEffectRegistration(registerType);
        }
        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            BaseGeographicEffect.GeometricEffectUnregistration(registerType);
        }
        #endregion

        [SMGIGraphic("长线", 0, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        public bool IsLong { get; set; }

        [SMGIGraphic("短线", 1, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        public bool IsShort { get; set; }

        public CenterLineOfRectangle()
        {
            IsLong = true;
            IsShort = false;
        }

        public IGeometry NextGeometry()
        {
            if (en == null || !en.MoveNext())
                return null;

            return en.Current;
        }

        IEnumerator<IGeometry> en;
        public void Reset(IGeometry Geometry)
        {
            en = ResetGeometry(Geometry as IPolygon);            
        }

        IEnumerator<IGeometry> ResetGeometry(IPolygon pg)
        {
            ITopologicalOperator topo = pg as ITopologicalOperator;
            IPolyline pl = topo.Boundary as IPolyline;
            ISegmentCollection segCol = pl as ISegmentCollection;
            if (segCol.SegmentCount!=4)
            {
                yield break;
            }
            
            IPolyline curpl = new PolylineClass();
            (curpl as ISegmentCollection).AddSegment(segCol.get_Segment(0));
            IPoint curPoint = new PointClass();
            curpl.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, curPoint);
            
            IPolyline centerpl1 = new PolylineClass();
            IPolyline centerpl2 = new PolylineClass();
            IRelationalOperator segRel = curpl as IRelationalOperator;
            
            IPolyline anotherpl1 = new PolylineClass();
            IPolyline anotherpl2 = new PolylineClass();
            IPolyline displ = new PolylineClass();
            IPoint anotherPoint1 = new PointClass();
            IPoint anotherPoint2 = new PointClass();
            IPoint disPoint = new PointClass();
            
            if (segRel.Disjoint(segCol.get_Segment(1)))
            {
                (displ as ISegmentCollection).AddSegment(segCol.get_Segment(1));
                displ.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, disPoint);
                centerpl1.FromPoint = curPoint;
                centerpl1.ToPoint = disPoint;
                
                (anotherpl1 as ISegmentCollection).AddSegment(segCol.get_Segment(2));
                anotherpl1.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, anotherPoint1);
                (anotherpl2 as ISegmentCollection).AddSegment(segCol.get_Segment(3));
                anotherpl2.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, anotherPoint2);
                
                centerpl2.FromPoint = anotherPoint1;
                centerpl2.ToPoint = anotherPoint2;

                if (IsLong)
                {
                    if (centerpl1.Length>centerpl2.Length)
                    {
                        yield return centerpl1;
                    }
                    else
                    {
                        yield return centerpl2;
                    }
                }

                if (IsShort)
                {
                    if (centerpl1.Length < centerpl2.Length)
                    {
                        yield return centerpl1;
                    }
                    else
                    {
                        yield return centerpl2;
                    }
                }
            }
            else if (segRel.Disjoint(segCol.get_Segment(2)))
            {
                (displ as ISegmentCollection).AddSegment(segCol.get_Segment(2));
                displ.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, disPoint);
                centerpl1.FromPoint = curPoint;
                centerpl1.ToPoint = disPoint;

                (anotherpl1 as ISegmentCollection).AddSegment(segCol.get_Segment(1));               
                anotherpl1.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, anotherPoint1);
                (anotherpl2 as ISegmentCollection).AddSegment(segCol.get_Segment(3));
                anotherpl2.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, anotherPoint2);

                centerpl2.FromPoint = anotherPoint1;
                centerpl2.ToPoint = anotherPoint2;

                if (IsLong)
                {
                    if (centerpl1.Length > centerpl2.Length)
                    {
                        yield return centerpl1;
                    }
                    else
                    {
                        yield return centerpl2;
                    }
                }

                if (IsShort)
                {
                    if (centerpl1.Length < centerpl2.Length)
                    {
                        yield return centerpl1;
                    }
                    else
                    {
                        yield return centerpl2;
                    }
                }
            }
            else if (segRel.Disjoint(segCol.get_Segment(3)))
            {
                (displ as ISegmentCollection).AddSegment(segCol.get_Segment(3));
                displ.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, disPoint);
                centerpl1.FromPoint = curPoint;
                centerpl1.ToPoint = disPoint;

                (anotherpl1 as ISegmentCollection).AddSegment(segCol.get_Segment(2));
                anotherpl1.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, anotherPoint1);
                (anotherpl2 as ISegmentCollection).AddSegment(segCol.get_Segment(1));
                anotherpl2.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, anotherPoint2);

                centerpl2.FromPoint = anotherPoint1;
                centerpl2.ToPoint = anotherPoint2;

                if (IsLong)
                {
                    if (centerpl1.Length > centerpl2.Length)
                    {
                        yield return centerpl1;
                    }
                    else
                    {
                        yield return centerpl2;
                    }
                }

                if (IsShort)
                {
                    if (centerpl1.Length < centerpl2.Length)
                    {
                        yield return centerpl1;
                    }
                    else
                    {
                        yield return centerpl2;
                    }
                }
            }
            yield break;
        }


        public esriGeometryType get_OutputType(esriGeometryType inputType)
        {
            if (inputType == esriGeometryType.esriGeometryPolygon)
                return esriGeometryType.esriGeometryPolyline;
            else
                return esriGeometryType.esriGeometryNull;
        }
    }
}

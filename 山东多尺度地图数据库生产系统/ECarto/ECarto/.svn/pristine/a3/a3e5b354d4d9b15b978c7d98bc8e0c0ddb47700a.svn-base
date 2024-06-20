using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("局部显示线")]
    [Guid("297fd995-6df0-461e-97ee-d8128d6f997d")]

    public class NotDashLineEffect : BaseGeographicEffect ,IGeometricEffect
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

        public NotDashLineEffect()
        {
            InVisableID = 999;
            IsCompMode = false;
        }

        [SMGIGraphic("显示部分ID",0, GraphicAttributeTypeEnum.GraphicAttributeIntegerType)]
        public int InVisableID { get; set; }


        [SMGIGraphic("兼容模式", 1, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        public bool IsCompMode { get; set; }

        public IGeometry NextGeometry()
        {
            if (eg == null || !eg.MoveNext())
            {
                return null;
            }
            return eg.Current;
        }

        public void Reset(IGeometry Geometry)
        {
            eg = GetGeomentryEnum(Geometry);
        }
        IEnumerator<IGeometry> eg;
        private IEnumerator<IGeometry> GetGeomentryEnum(IGeometry geo)
        {
            if (!(geo is IPolycurve))
                yield return null;
            if (!(geo as IPointIDAware).PointIDAware)
            {
                if (IsCompMode)
                {
                    yield return geo;
                }
                else
                {
                    yield return null;
                }
                yield break;
            }
            PolylineClass line = new PolylineClass();
            ISegmentCollection sc = geo as ISegmentCollection;

            for (int i = 0; i < sc.SegmentCount; i++)
            {
                ISegment seg = sc.Segment[i];
                if (seg.FromPoint.ID == InVisableID )
                {
                    line.AddSegment(seg);
                }
            }

            line.Simplify();
            yield return line;
        }
        public esriGeometryType get_OutputType(esriGeometryType inputType)
        {
            if (inputType == esriGeometryType.esriGeometryPolygon
                || inputType == esriGeometryType.esriGeometryPolyline)
            {
                return esriGeometryType.esriGeometryPolyline;
            }
            return esriGeometryType.esriGeometryNull;
        }
    }
}

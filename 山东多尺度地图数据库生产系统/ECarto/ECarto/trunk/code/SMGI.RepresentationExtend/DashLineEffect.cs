using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("局部消隐线")]
    [Guid("A9D99C65-6265-4ED0-8A26-8F38A777A7BD")]
    public class DashLineEffect: BaseGeographicEffect ,IGeometricEffect
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

        public DashLineEffect()
        {
            InVisableID = -999;
        }
        [SMGIGraphic("不显示部分ID",0, GraphicAttributeTypeEnum.GraphicAttributeIntegerType)]
        public int InVisableID { get; set; }

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
                yield return geo;
                yield break;
            }
            PolylineClass line = new PolylineClass();
            ISegmentCollection sc = geo as ISegmentCollection;
            List<IPoint> pts = new List<IPoint>();

            for (int i = 0; i < sc.SegmentCount; i++)
            {
                ISegment seg = sc.Segment[i];
                if (seg.FromPoint.ID != InVisableID )
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

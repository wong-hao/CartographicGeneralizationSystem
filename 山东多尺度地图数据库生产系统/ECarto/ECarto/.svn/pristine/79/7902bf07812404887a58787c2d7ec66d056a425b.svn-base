using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("面中心线")]
    [Guid("3684E74A-5E60-4A67-A6EB-BB0D1D03211A")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.MianLine")]
    public class MianLine : BaseGeographicEffect, IGeometricEffect
    {
        #region COM Registration Function(s)
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            GeometricEffectRegistration(registerType);
        }
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            GeometricEffectUnregistration(registerType);
        }
        #endregion

        [SMGIGraphic("长线", 0, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        public bool IsLong { get; set; }
        //[SMGIGraphic("短线", 1, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        //public bool IsShort {get;set;}

        public IGeometry NextGeometry()
        {
            if (_en == null || !_en.MoveNext())
                return null;

            return _en.Current;
        }

        IEnumerator<IGeometry> _en;
        public void Reset(IGeometry geometry)
        {
            _en = ResetGeometry(geometry as IPolygon);
        }

        IEnumerator<IGeometry> ResetGeometry(IPolygon pg)
        {
            var pl = ((ITopologicalOperator) pg).Boundary as IPolyline;
            var segCol = pl as ISegmentCollection;
            var points = new List<IPoint>();points.Clear();
            for (var i = 0; i < segCol.SegmentCount; i++)
            {
                IPolyline curpl = new PolylineClass();
                (curpl as ISegmentCollection).AddSegment(segCol.Segment[i]);
                IPoint curPoint = new PointClass();
                curpl.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, curPoint);
                points.Add(curPoint);
            }
            double leng = 0;
            int fp = 0, top = 0;
            IPolyline centerpl1 = new PolylineClass();
            IPolyline jieguo = new PolylineClass();
            for (int i = 0; i < points.Count-1; i++)
            {
                centerpl1.FromPoint = points[i];
                for (int j = i+1; j < points.Count; j++)
                {
                    centerpl1.ToPoint = points[j];
                    if (!(centerpl1.Length > leng)) continue;
                    leng = centerpl1.Length;
                    fp = i;
                    top = j;
                }
            }
            jieguo.FromPoint = points[fp];
            jieguo.ToPoint = points[top];
            yield return jieguo;
        }


        public esriGeometryType get_OutputType(esriGeometryType inputType)
        {
            if (inputType == esriGeometryType.esriGeometryPolygon)
                return esriGeometryType.esriGeometryPolyline;
            return esriGeometryType.esriGeometryNull;
        }
    }
}

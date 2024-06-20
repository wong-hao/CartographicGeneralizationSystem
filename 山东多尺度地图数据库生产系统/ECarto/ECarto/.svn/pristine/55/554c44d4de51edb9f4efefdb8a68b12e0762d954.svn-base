using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("面首线")]
    [Guid("2f3c771d-ccf6-4531-b69a-a259424e23cb")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.Miansx")]
    public class Miansx : BaseGeographicEffect, IGeometricEffect
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
        [SMGIGraphic("一", 0, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        public bool Yi { get; set; }
        bool _mBDone;
        IGeometry _mPGeom;
        int _plIndex = -1;
        IPolyline[] _pls;
        public Miansx()
        {
            Yi = false;
        }
        public IGeometry NextGeometry()
        {
            if (_mBDone)
            {
                return null;
            }
            if (_mPGeom == null || _mPGeom.IsEmpty)
            {
                return null;
            }
            if (_plIndex == -1)
            {
                _pls = MBridge(_mPGeom as IPolygon);
                if (_pls == null)
                {
                    return null;
                }
                _plIndex = 0;
            }

            if (_plIndex == _pls.Length)
            {
                _mBDone = true;
                return null;
            }
            return _pls[_plIndex++];
        }

        public void Reset(IGeometry geometry)
        {
            if (geometry.IsEmpty)
            {
                return;
            }
            _mPGeom = geometry;
            _plIndex = -1;
            _pls = null;
            _mBDone = false;
        }
        public esriGeometryType get_OutputType(esriGeometryType inputType)
        {
            if (inputType == esriGeometryType.esriGeometryPolygon)
                return esriGeometryType.esriGeometryPolyline;
            return esriGeometryType.esriGeometryNull;
        }
        private IPolyline[] MBridge(IPolygon pg)
        {
            var pls = new List<IPolyline>();
            var pll = ((ITopologicalOperator)pg).Boundary as IPolyline;
            var segCol = pll as ISegmentCollection;
            if (segCol == null) return null;
            IPolyline curpl = new PolylineClass();
            (curpl as ISegmentCollection).AddSegment(segCol.Segment[0]);
            pls.Add(curpl);
            return pls.ToArray();
        }
    }
}

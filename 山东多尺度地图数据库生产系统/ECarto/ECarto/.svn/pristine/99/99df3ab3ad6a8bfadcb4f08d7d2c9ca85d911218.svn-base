using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("长短线")]
    [Guid("538C6FF4-A9BF-49A6-BEAE-D6EB6D436EF4")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.ChangDuanXian")]
    public class ChangDuanXian : BaseGeographicEffect, IGeometricEffect
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
        private bool IsLong { get; set; }
        //[SMGIGraphic("短线", 1, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        //public bool IsShort { get; set; }
        bool _mBDone;
        IGeometry _mPGeom;
        int _plIndex = -1;
        IPolyline[] _pls;
        public ChangDuanXian()
        {
            IsLong = true;
            //IsShort = false;
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
            return inputType == esriGeometryType.esriGeometryPolygon ? esriGeometryType.esriGeometryPolyline : esriGeometryType.esriGeometryNull;
        }

        private IPolyline[] MBridge(IPolygon pg)
        {
            var pls = new List<IPolyline>();
            var pll = ((ITopologicalOperator) pg).Boundary as IPolyline;
            var segCol = pll as ISegmentCollection;
            if (segCol == null) return null;
            if (segCol.SegmentCount != 4)
            {
                return null;
            }
            IPolyline curpl = new PolylineClass();
            (curpl as ISegmentCollection).AddSegment(segCol.Segment[0]);
            IPolyline curpl1 = new PolylineClass();
            (curpl1 as ISegmentCollection).AddSegment(segCol.Segment[1]);
            IPolyline curpl2 = new PolylineClass();
            (curpl2 as ISegmentCollection).AddSegment(segCol.Segment[2]);
            IPolyline curpl3 = new PolylineClass();
            (curpl3 as ISegmentCollection).AddSegment(segCol.Segment[3]);
            if (curpl.Length > curpl1.Length)
            {
                if (IsLong)
                {
                    pls.Add(curpl); pls.Add(curpl2);
                }
                else
                {
                    pls.Add(curpl1); pls.Add(curpl3);
                } 
            }
            else
            {
                if (IsLong)
                {
                    pls.Add(curpl1); pls.Add(curpl3);
                }
                else
                {
                    pls.Add(curpl); pls.Add(curpl2);
                } 
            }
            return pls.ToArray();
        }
    }
}

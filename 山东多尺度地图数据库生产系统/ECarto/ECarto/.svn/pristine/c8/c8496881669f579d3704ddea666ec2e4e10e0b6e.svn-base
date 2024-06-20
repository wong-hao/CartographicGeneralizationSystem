using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("台阶")]
    [Guid("ac2f3374-86cb-4998-b2e1-ae5f8ff43aa7")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.TaiJie")]
    public class TaiJie : BaseGeographicEffect, IGeometricEffect
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
        public TaiJie()
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
            if (Yi)
            {
                var cout = curpl.Length / 2.83 > curpl2.Length / 2.83 ? curpl2.Length / 2.83 : curpl.Length / 2.83;
                for (int i = 1; i < cout; i++)
                {
                    IPoint sPoint = new PointClass();
                    curpl.QueryPoint(esriSegmentExtension.esriNoExtension, 2.83*i, false, sPoint);
                    IPoint ePoint = new PointClass();
                    curpl2.QueryPoint(esriSegmentExtension.esriNoExtension, curpl2.Length - 2.83 * i, false, ePoint);
                    IPolyline pl = new PolylineClass { FromPoint = sPoint, ToPoint = ePoint };
                    pls.Add(pl);
                }
            }
            else
            {
                var cout = curpl1.Length / 2.83 > curpl3.Length / 2.83 ? curpl3.Length / 2.83 : curpl1.Length / 2.83;
                for (int i = 1; i < cout; i++)
                {
                    IPoint sPoint = new PointClass();
                    curpl1.QueryPoint(esriSegmentExtension.esriNoExtension, 2.83 * i, false, sPoint);
                    IPoint ePoint = new PointClass();
                    curpl3.QueryPoint(esriSegmentExtension.esriNoExtension, curpl3.Length - 2.83 * i, false, ePoint);
                    IPolyline pl = new PolylineClass { FromPoint = sPoint, ToPoint = ePoint };
                    pls.Add(pl);
                }
            }
            return pls.ToArray();
        }
    }
}

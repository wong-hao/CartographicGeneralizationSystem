using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("棚房")]
    [Guid("1B128134-712E-4946-8F55-A19957814588")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.PfPolygonEffect")]
    public class PfPolygonEffect : BaseGeographicEffect, IGeometricEffect
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
        //[SMGIGraphic("二", 1, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        //public bool ER { get; set; }
        //[SMGIGraphic("三", 2, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        //public bool SAN { get; set; }
        //[SMGIGraphic("四", 3, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        //public bool SI { get; set; }
        bool _mBDone;
        IGeometry _mPGeom;
        int _plIndex=-1;
        IPolyline[] _pls;
        public PfPolygonEffect()
        {
            Yi = true;
            //_plIndex = -1;
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

        public void Reset(IGeometry Geometry)
        {
            if (Geometry.IsEmpty)
            {
                return;
            }
            _mPGeom = Geometry;
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

        private IPolyline[] MBridge(IPolygon pl, double width = 2.83, double tickLength = 2.83, double tickAngle = 135)
        {
            //1.初始化参数
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            
            for (int i = 0; i < ((ISegmentCollection) pl).SegmentCount; i++)
            {
                var linshi = new PolylineClass();
                (linshi as IPointCollection).AddPoint(((ISegmentCollection) pl).Segment[i].FromPoint);
                (linshi as IPointCollection).AddPoint(((ISegmentCollection) pl).Segment[i].ToPoint);
                //pls.Add(linshi);
                pls.Add(ExtendTickLine1(tickLength,tickAngle, true, linshi, ge));
            }
            //2.做平行线
            return pls.ToArray();
        }
        public IPolyline ExtendTickLine(double length, double angle, bool frompt, IPolyline plnew, IGeometryBridge ge)
        {
            if (plnew.IsEmpty)
            {
                return null;
            }
            IPoint firstP = new PointClass();
            if (frompt)
            {
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, -1 * length, false, firstP);
            }
            else
            {
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, plnew.Length + length, false, firstP);
            }

            if (!firstP.IsEmpty)
            {
                var lineSeg = new LineClass();
                if (frompt)
                {
                    lineSeg.FromPoint = firstP;
                    lineSeg.ToPoint = plnew.FromPoint;
                    lineSeg.Rotate(plnew.FromPoint, angle / 180 * Math.PI);
                }
                else
                {
                    lineSeg.FromPoint = plnew.ToPoint;
                    lineSeg.ToPoint = firstP;
                    lineSeg.Rotate(plnew.ToPoint, angle / 180 * Math.PI);
                }
                IPolyline pl=new PolylineClass();
                (pl as IPointCollection).AddPoint(lineSeg.FromPoint);
                (pl as IPointCollection).AddPoint(lineSeg.ToPoint);
                return pl;
            }
            return null;
        }
        public IPolyline ExtendTickLine1(double length, double angle, bool frompt, IPolyline plnew, IGeometryBridge ge)
        {
            if (plnew.IsEmpty)
            {
                return null;
            }
            IPoint firstP = new PointClass();
            if (frompt)
            {
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, -1 * length, false, firstP);
            }
            else
            {
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, plnew.Length + length, false, firstP);
            }

            if (!firstP.IsEmpty)
            {
                var lineSeg = new PolylineClass();
                if (frompt)
                {
                    lineSeg.FromPoint = firstP;
                    lineSeg.ToPoint = plnew.FromPoint;
                    lineSeg.Rotate(plnew.FromPoint, angle / 180 * Math.PI);

                }
                else
                {
                    lineSeg.FromPoint = plnew.ToPoint;
                    lineSeg.ToPoint = firstP;
                    lineSeg.Rotate(plnew.ToPoint, angle / 180 * Math.PI);
                }
                IPolyline ee = lineSeg;
                return ee;
            }
            return null;
        }
    }
}

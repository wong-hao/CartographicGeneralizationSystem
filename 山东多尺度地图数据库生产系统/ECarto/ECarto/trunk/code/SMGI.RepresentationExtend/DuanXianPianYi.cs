using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("短线平行线")]
    [Guid("14F2EBD0-803E-4D3F-AA2E-016240BEA3BC")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.DuanXianPianYi")]
    public class DuanXianPianYi : BaseGeographicEffect, IGeometricEffect
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
                IPoint sPoint=new PointClass();
                IPoint sPoint1 = new PointClass();
                IPoint sPoint2 = new PointClass();
                IPoint sPoint3 = new PointClass();
                curpl.QueryPoint(esriSegmentExtension.esriNoExtension, 20, false, sPoint);
                curpl.QueryPoint(esriSegmentExtension.esriNoExtension, 40, false, sPoint1);
                curpl.QueryPoint(esriSegmentExtension.esriNoExtension, curpl.Length-40, false, sPoint2);
                curpl.QueryPoint(esriSegmentExtension.esriNoExtension, curpl.Length - 20, false, sPoint3);
                IPoint ePoint = new PointClass();
                IPoint ePoint1 = new PointClass();
                IPoint ePoint2 = new PointClass();
                IPoint ePoint3 = new PointClass();
                curpl2.QueryPoint(esriSegmentExtension.esriNoExtension, 20, false, ePoint);
                curpl2.QueryPoint(esriSegmentExtension.esriNoExtension, 40, false, ePoint1);
                curpl2.QueryPoint(esriSegmentExtension.esriNoExtension, curpl2.Length - 40, false, ePoint2);
                curpl2.QueryPoint(esriSegmentExtension.esriNoExtension, curpl2.Length - 20, false, ePoint3);
                IPolyline pl = new PolylineClass { FromPoint = sPoint, ToPoint = ePoint3 };
                IPolyline pl1 = new PolylineClass { FromPoint = sPoint1, ToPoint = ePoint2 };
                IPolyline pl2 = new PolylineClass { FromPoint = sPoint2, ToPoint = ePoint1 };
                IPolyline pl3 = new PolylineClass { FromPoint = sPoint3, ToPoint = ePoint };
                pls.Add(pl);
                pls.Add(pl1);
                pls.Add(pl2);
                pls.Add(pl3);
            }
            else
            {
                IPoint sPoint = new PointClass();
                IPoint sPoint1 = new PointClass();
                IPoint sPoint2 = new PointClass();
                IPoint sPoint3 = new PointClass();
                curpl1.QueryPoint(esriSegmentExtension.esriNoExtension, 20, false, sPoint);
                curpl1.QueryPoint(esriSegmentExtension.esriNoExtension, 40, false, sPoint1);
                curpl1.QueryPoint(esriSegmentExtension.esriNoExtension, curpl1.Length - 40, false, sPoint2);
                curpl1.QueryPoint(esriSegmentExtension.esriNoExtension, curpl1.Length - 20, false, sPoint3);
                IPoint ePoint = new PointClass();
                IPoint ePoint1 = new PointClass();
                IPoint ePoint2 = new PointClass();
                IPoint ePoint3 = new PointClass();
                curpl3.QueryPoint(esriSegmentExtension.esriExtendAtFrom, 20, false, ePoint);
                curpl3.QueryPoint(esriSegmentExtension.esriExtendAtFrom, 40, false, ePoint1);
                curpl3.QueryPoint(esriSegmentExtension.esriExtendAtTo, curpl3.Length - 40, false, ePoint2);
                curpl3.QueryPoint(esriSegmentExtension.esriExtendAtTo, curpl3.Length - 20, false, ePoint3);
                IPolyline pl = new PolylineClass { FromPoint = sPoint, ToPoint = ePoint3 };
                IPolyline pl1 = new PolylineClass { FromPoint = sPoint1, ToPoint = ePoint2 };
                IPolyline pl2 = new PolylineClass { FromPoint = sPoint2, ToPoint = ePoint1 };
                IPolyline pl3 = new PolylineClass { FromPoint = sPoint3, ToPoint = ePoint };
                pls.Add(pl);
                pls.Add(pl1);
                pls.Add(pl2);
                pls.Add(pl3);
            }
            return pls.ToArray();
        }
    }
}

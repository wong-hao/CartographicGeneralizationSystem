using System;
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("面交线")]
    [Guid("0A5F6474-4C8B-4C8E-B4FC-9F14D0340663")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.XPolygonEffect")]
    public class XPolygonEffect : BaseGeographicEffect, IGeometricEffect
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

        IEnumerator<IGeometry> en;
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
            return inputType == esriGeometryType.esriGeometryPolygon ? esriGeometryType.esriGeometryPolyline : esriGeometryType.esriGeometryNull;
        }

        private IPolyline[] MBridge(IPolygon pl, double width = 2.83)
        {
            //1.初始化参数
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            //计算角度顺序
            var xulie = new Dictionary<int, double>();
            for (int i = 0; i < ((ISegmentCollection) pl).SegmentCount; i++)
            {
                double value = 0;
                if (i == 0)
                {
                    value = (((ISegmentCollection) pl).get_Segment(i) as ILine).Angle -
                            (((ISegmentCollection) pl).get_Segment(((ISegmentCollection) pl).SegmentCount - 1) as ILine).Angle;
                }
                else
                {
                    value = (((ISegmentCollection) pl).get_Segment(i) as ILine).Angle -
                            (((ISegmentCollection) pl).get_Segment(i - 1) as ILine).Angle;
                }
                xulie.Add(i, Math.Abs(value));
            }
            var px = xulie.OrderByDescending(t => t.Value).ToDictionary(p => p.Key, o => o.Value);
            var hao = new List<int>(); hao.Clear();
            int jis = 0;
            foreach (var keyValuePair in px)
            {
                if (jis <= 3)
                {
                    hao.Add(keyValuePair.Key);
                    jis++;
                }
            }
            var zhao = hao.OrderBy(i => i).ToList();
            var fenbian = new List<PolylineClass>();
            for (int i = 0; i < zhao.Count; i++)
            {
                var leng = i == zhao.Count - 1 ? (pl as ISegmentCollection).SegmentCount : zhao[i + 1];
                var linshi = new PolylineClass();
                (linshi as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(zhao[i]).FromPoint);
                for (int j = zhao[i]; j < leng; j++)
                {
                    (linshi as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(j).ToPoint);
                }
                if (zhao[0] != 0 && i == zhao.Count - 1)
                {
                    for (int j = 0; j < zhao[0]; j++)
                    {
                        (linshi as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(j).ToPoint);
                    }
                }
                fenbian.Add(linshi);
            }
            var xLine1 = new PolylineClass();
            (xLine1 as IPointCollection).AddPoint(fenbian[0].FromPoint);
            (xLine1 as IPointCollection).AddPoint(fenbian[2].FromPoint);
            var xLine2 = new PolylineClass();
            (xLine2 as IPointCollection).AddPoint(fenbian[0].ToPoint);
            (xLine2 as IPointCollection).AddPoint(fenbian[2].ToPoint);
            pls.Add(xLine1);
            pls.Add(xLine2);
            return pls.ToArray();
        }
    }
}

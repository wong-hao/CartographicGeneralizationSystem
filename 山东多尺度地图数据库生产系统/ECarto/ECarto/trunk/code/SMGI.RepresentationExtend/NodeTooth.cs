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
    [SMGIClassName("规则四角点")]
    [Guid("EF8C074A-9ACD-4B9C-BFAA-FB34832AC91C")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.NodeTooth")]
    public class NodeTooth : BaseGeographicEffect, IGeometricEffect
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
        public bool IsLong {get;set;}
        [SMGIGraphic("长度", 1, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        double TickLength {get;set;}
        [SMGIGraphic("角度", 2, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        double TickAngle {get;set;}
        bool m_bDone;
        IGeometry m_pGeom;
        int plIndex = -1;
        PolylineClass[] pls;

        public NodeTooth()
        {
            IsLong = true;
            TickAngle = 0;
            TickLength = 1.415;
        }

        public IGeometry NextGeometry()
        {
            if (m_bDone)
            {
                return null;
            }
            if (m_pGeom == null || m_pGeom.IsEmpty)
            {
                return null;
            }
            if (plIndex == -1)
            {
                pls = MBridge(m_pGeom as IPolygon);
                if (pls == null)
                {
                    return null;
                }
                plIndex = 0;
            }

            if (plIndex == pls.Length)
            {
                m_bDone = true;
                return null;
            }
            return pls[plIndex++];
        }
        public void Reset(IGeometry geometry)
        {
            if (geometry.IsEmpty)
            {
                return;
            }
            m_pGeom = geometry;
            plIndex = -1;
            pls = null;
            m_bDone = false;
        }
        public esriGeometryType get_OutputType(esriGeometryType inputType)
        {
            if (inputType == esriGeometryType.esriGeometryPolygon)
                return esriGeometryType.esriGeometryPolyline;
            return esriGeometryType.esriGeometryNull;
        }

        private PolylineClass[] MBridge(IPolygon pg)
        {
            IGeometryBridge ge = new GeometryEnvironmentClass();
            var points = new List<PolylineClass>();
            var topo = pg as ITopologicalOperator;
            var pll = topo.Boundary as IPolyline;
            var segCol = pll as ISegmentCollection;
            if (segCol.SegmentCount != 4){return null;}
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
                    points.Add(ExtendTickLine(TickLength, 360-TickAngle, true, curpl, ge));
                    points.Add(ExtendTickLine(TickLength, TickAngle, false, curpl, ge));
                    points.Add(ExtendTickLine(TickLength, 360 - TickAngle, true, curpl2, ge));
                    points.Add(ExtendTickLine(TickLength, TickAngle, false, curpl2, ge));
                }
                else
                {
                    points.Add(ExtendTickLine(TickLength, 360 - TickAngle, true, curpl1, ge));
                    points.Add(ExtendTickLine(TickLength, TickAngle, false, curpl1, ge));
                    points.Add(ExtendTickLine(TickLength, 360 - TickAngle, true, curpl3, ge));
                    points.Add(ExtendTickLine(TickLength, TickAngle, false, curpl3, ge));
                }
            }
            else
            {
                if (IsLong)
                {
                    points.Add(ExtendTickLine(TickLength, 360 - TickAngle, true, curpl1, ge));
                    points.Add(ExtendTickLine(TickLength, TickAngle, false, curpl1, ge));
                    points.Add(ExtendTickLine(TickLength, 360 - TickAngle, true, curpl3, ge));
                    points.Add(ExtendTickLine(TickLength, TickAngle, false, curpl3, ge));
                }
                else
                {
                    points.Add(ExtendTickLine(TickLength, 360 - TickAngle, true, curpl, ge));
                    points.Add(ExtendTickLine(TickLength, TickAngle, false, curpl, ge));
                    points.Add(ExtendTickLine(TickLength, 360 - TickAngle, true, curpl2, ge));
                    points.Add(ExtendTickLine(TickLength, TickAngle, false, curpl2, ge));
                }
            }
            return points.ToArray();
        }
        public PolylineClass ExtendTickLine(double length, double angle, bool frompt, IPolyline plnew, IGeometryBridge ge)
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
                    lineSeg.FromPoint = firstP;
                    lineSeg.ToPoint = plnew.ToPoint;
                    lineSeg.Rotate(plnew.ToPoint, angle / 180 * Math.PI);
                }
                return lineSeg;
            }
            return null;
        }
    }
}

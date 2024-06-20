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
    [SMGIClassName("渐变局部消隐线")]
    [Guid("5A16B6EA-2629-4D8C-87E0-2E91B5E8184B")]
    public class GradualDashLineEffect : BaseGeographicEffect, IGeometricEffect
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

        GeometricEffectTaperedPolygonClass helper;
        public GradualDashLineEffect()
        {
            InVisableID = -999;
            StartWidth = 0.1;
            StopWidth = 0.1;
            helper = new ESRI.ArcGIS.Display.GeometricEffectTaperedPolygonClass();
            
        }
        [SMGIGraphic("不显示部分ID",2, GraphicAttributeTypeEnum.GraphicAttributeIntegerType)]
        public int InVisableID { get; set; }

        [SMGIGraphic("起始宽度", 0, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        public double StartWidth { get; set; }

        [SMGIGraphic("终止宽度", 1, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        public double StopWidth { get; set; }


        public esriGeometryType get_OutputType(esriGeometryType inputType)
        {
            if (inputType == esriGeometryType.esriGeometryPolyline)
                return esriGeometryType.esriGeometryPolygon;
            else
                return esriGeometryType.esriGeometryNull;
        }


        //输出渐变效果后的几何
        public IGeometry NextGeometry()
        {
            if (en == null || !en.MoveNext())
                return null;

            return en.Current;
        }

        IEnumerator<IGeometry> en;
        //输入原始几何线
        public void Reset(IGeometry Geometry)
        {
            en = ResetGeometry(Geometry as IPolyline);
        }
        //对原始几何进行几何效果处理
        IEnumerator<IGeometry> ResetGeometry(IPolyline line)
        {
            var helper = new GeometricEffectTaperedPolygonClass();

            //from
            helper.set_Value(0, StartWidth);
            //to
            helper.set_Value(1, StopWidth);
            //length 没搞懂
            helper.set_Value(2, 0);
            helper.Reset(line);
          
           
            //if (line.FromPoint.ID == InVisableID)
            //    yield return null;
             List<IPolyline> pCutpolyline = new List<IPolyline>();
           

            ISegmentCollection sc = line as ISegmentCollection;
            IPointCollection pc = line as IPointCollection;
            if ((line as IPointIDAware).PointIDAware)
            {
                PolylineClass cutline = null;
                for (int i = 0; i < pc.PointCount; i++)
                {
                    IPoint pt = pc.get_Point(i);
                    if (pt.ID == InVisableID && cutline==null)
                    {
                        cutline = new PolylineClass();
                        cutline.AddPoint(pt);
                    }
                    if (pt.ID == InVisableID && cutline != null)
                    {
                        cutline.AddPoint(pt);
                    }
                    if (pt.ID != InVisableID && cutline != null)
                    {
                        cutline.AddPoint(pt);
                        cutline.Simplify();
                        pCutpolyline.Add(cutline);
                        cutline = null;
                    }
                }
            }
         
            //for (int i = 0; i < sc.SegmentCount; i++)
            //{
            //    ISegment seg = sc.Segment[i];
            //    if (seg.FromPoint.ID == InVisableID)
            //    {
            //        PolylineClass lineCut = new PolylineClass();
            //        lineCut.AddSegment(seg);
                   
                    
            //        lineCut.Simplify();
            //        pCutpolyline.Add(lineCut);
            //    }
            //}
           
           
            
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                IGeometry tapPolygon=g;
                foreach (var cutline in pCutpolyline)
                {
                    IPolygon polygon = BufferLine(cutline);
                    ITopologicalOperator to= tapPolygon as ITopologicalOperator;
                    tapPolygon=  to.Difference(polygon);

                }
                yield return tapPolygon;
            }
            yield break;
        }
        //构造平头缓冲面
        private IPolygon BufferLine(IPolyline polyline)
        {
            IConstructCurve line1 = new PolylineClass();
            line1.ConstructOffset(polyline, StopWidth+0.1, esriConstructOffsetEnum.esriConstructOffsetMitered);
           
            IConstructCurve line2 = new PolylineClass();
            line2.ConstructOffset(polyline, -StopWidth- 0.1, esriConstructOffsetEnum.esriConstructOffsetMitered);
            (line2 as IPolyline).ReverseOrientation();

            IPointCollection pc = new PolygonClass();
            pc.AddPointCollection(line1 as IPointCollection);
            pc.AddPointCollection(line2 as IPointCollection);
            IPolygon polygon = pc as IPolygon;
            (polygon as ITopologicalOperator).Simplify();
            return polygon;

        }
    }
}

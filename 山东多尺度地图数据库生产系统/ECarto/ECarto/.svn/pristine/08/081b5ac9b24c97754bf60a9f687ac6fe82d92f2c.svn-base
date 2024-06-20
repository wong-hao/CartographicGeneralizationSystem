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
    [SMGIClassName("石质陡崖")]
    [Guid("8070cdcf-00c0-4f08-9464-39bc96561b0f")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.SlopeGeometryEffect")]
    public class SlopeGeometryEffect: BaseGeographicEffect, IGeometricEffect
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

        public SlopeGeometryEffect()
        {
            allRatio = 0.6;
            firstRatio = 0.25;
            secondRatio = 0.5;
            thirdRatio = 0.75;
        }

        [SMGIGraphic("总体比例",0, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        public double allRatio { get; set; }

        [SMGIGraphic("第一段比例", 1, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        public double firstRatio { get; set; }

        [SMGIGraphic("第二段比例", 2, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        public double secondRatio { get; set; }

        [SMGIGraphic("第三段比例", 3, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        public double thirdRatio { get; set; }

        public IGeometry NextGeometry()
        {
            if (en == null || !en.MoveNext())
                return null;

            return en.Current;
        }

        IEnumerator<IGeometry> en;
        public void Reset(IGeometry Geometry)
        {
            en = ResetGeometry(Geometry as IPolyline);            
        }

        IEnumerator<IGeometry> ResetGeometry(IPolyline line)
        {
            bool isSplit;
            int splitIndex, segIndex;
            object o = Type.Missing;
            IPolyline pl = (line as IClone).Clone() as IPolyline;
            pl.SplitAtDistance(allRatio, true, false, out isSplit, out splitIndex, out segIndex);
            ISegmentCollection plSegcCol = pl as ISegmentCollection;

            if (isSplit)
            {
                plSegcCol.RemoveSegments(segIndex, plSegcCol.SegmentCount - segIndex, true);
                plSegcCol.SegmentsChanged();

                IPolyline pl1 = new PolylineClass();
                ILine tickline1 = new LineClass();
                pl.QueryNormal(esriSegmentExtension.esriNoExtension, firstRatio, true, 1.6, tickline1);
                (pl1 as ISegmentCollection).AddSegment(tickline1 as ISegment);
                yield return pl1;

                IPolyline pl2 = new PolylineClass();
                ILine tickline2 = new LineClass();
                pl.QueryNormal(esriSegmentExtension.esriNoExtension, secondRatio, true, 2.7, tickline2);
                (pl2 as ISegmentCollection).AddSegment(tickline2 as ISegment);
                yield return pl2;

                IPolyline pl3 = new PolylineClass();
                ILine tickline3 = new LineClass();
                pl.QueryNormal(esriSegmentExtension.esriNoExtension, thirdRatio, true, 3.8, tickline3);
                (pl3 as ISegmentCollection).AddSegment(tickline3 as ISegment);
                yield return pl3;
            }
            yield break;
        }


        public esriGeometryType get_OutputType(esriGeometryType inputType)
        {
            if (inputType == esriGeometryType.esriGeometryPolyline)
                return esriGeometryType.esriGeometryPolyline;
            else
                return esriGeometryType.esriGeometryNull;
        }
    }
}

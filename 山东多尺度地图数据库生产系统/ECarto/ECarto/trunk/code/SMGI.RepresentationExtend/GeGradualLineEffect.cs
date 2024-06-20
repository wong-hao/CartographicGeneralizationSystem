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
    [SMGIClassName("两头尖符号")]
    [Guid("6A78F45C-9957-49FF-8A40-75C0F5C012E8")]
    public class GeGradualLineEffect : BaseGeographicEffect, IGeometricEffect
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

        public GeGradualLineEffect()
        {
        }

        [SMGIGraphic("最窄宽度",0, GraphicAttributeTypeEnum.GraphicAttributeSizeType)]
        public double MinWidth { get; set; }

        [SMGIGraphic("最宽宽度",1, GraphicAttributeTypeEnum.GraphicAttributeSizeType)]
        public double MaxWidth { get; set; }

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
            var helper = new GeometricEffectTaperedPolygonClass();

            //from
            helper.set_Value(0, MinWidth);
            //to
            helper.set_Value(1, MaxWidth);
            //length 没搞懂
            helper.set_Value(2, 0);

            var gc = line as IGeometryCollection;
            for (int i = 0; i < gc.GeometryCount; i++)
            {
                var newLine = new PolylineClass();
                var path = (gc.Geometry[i] as IClone).Clone() as IPath;
                newLine.AddGeometry(path);
                bool hped;
                int partid,segid;
                newLine.SplitAtDistance(0.5, true, true,out hped, out partid, out segid);
                var newLine1 = new PolylineClass();
                var newLine2 = new PolylineClass();
                newLine1.AddGeometry(newLine.get_Geometry(0));
                newLine2.AddGeometry(newLine.get_Geometry(1));
                newLine2.ReverseOrientation();
                helper.Reset(newLine1);
                while (true)
                {
                    var g = helper.NextGeometry();
                    if (g == null)
                        break;
                    yield return g;
                }

                var pt = newLine1.ToPoint;
                yield return (pt as ITopologicalOperator).Buffer(MaxWidth / 2);

                helper.Reset(newLine2);
                while(true)
                {
                    var g = helper.NextGeometry();
                    if (g == null)
                        break;
                    yield return g;
                }
            }
            
            yield break;
        }


        public esriGeometryType get_OutputType(esriGeometryType inputType)
        {
            if (inputType == esriGeometryType.esriGeometryPolyline)
                return esriGeometryType.esriGeometryPolygon;
            else
                return esriGeometryType.esriGeometryNull;
        }
    }
}

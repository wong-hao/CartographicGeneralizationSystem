using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.CATIDs;
using System.Runtime.InteropServices;
namespace SMGI.RepresentationExtend
{
    [Guid("D8080B26-1126-4FAD-BB20-6DAB81D7ECA5")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.DikeGeometryEffect")]

    public class DikeGeometryEffect : IGeometricEffect, IGraphicAttributes, IPersistVariant
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            ArcGISCategoryRegistration(registerType);
        }
        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            ArcGISCategoryUnregistration(registerType);
        }
        #endregion

        #region Component Category Registration
        static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = String.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            GeometricEffect.Register(regKey);
        }
        static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = String.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            GeometricEffect.Unregister(regKey);
        }
        #endregion
        bool m_bDone;
        IGeometry m_pGeom;
        int plIndex = -1;
        IPolyline[] pls;
        enum DikeType { 干堤依比例 }

        DikeType DType
        {
            get;
            set;
        }
        double Width
        {
            get;
            set;
        }
        double TickLength
        {
            get;
            set;
        }

        double TickAngle
        {
            get;
            set;
        }

        public DikeGeometryEffect()
        {
            Width = 1.42;        //0.5mm  1pt=0.3528mm
            TickAngle = 45;
            TickLength = 5;  //0.5mm
            plIndex = -1;
            DType = DikeType.干堤依比例;

        }

        #region IGeometricEffect Members

        IGeometry IGeometricEffect.NextGeometry()
        {
            if (m_bDone)
            {
                return null;
            }
            else
            {
                if (m_pGeom == null || m_pGeom.IsEmpty)
                {
                    return null;
                }
                if (plIndex == -1)
                {                  
                    if (DType == DikeType.干堤依比例)
                    {
                        pls = CalculateGeneralDike(m_pGeom as IPolyline, true, Width, 5);
                    }

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
                else
                {
                    return pls[plIndex++];
                }
            }
        }

        void IGeometricEffect.Reset(IGeometry Geometry)
        {
            if (Geometry.IsEmpty)
            {
                return;
            }
            m_pGeom = Geometry;
            plIndex = -1;
            pls = null;
            m_bDone = false;
        }

        esriGeometryType IGeometricEffect.get_OutputType(esriGeometryType inputType)
        {
            if (inputType == esriGeometryType.esriGeometryPolyline)
            {
                return inputType;
            }
            return esriGeometryType.esriGeometryNull;
        }
        #endregion

        #region IGraphicAttributes Members

        string IGraphicAttributes.ClassName
        {
            get { return "国三制图院 干堤（依比例）"; }
        }

        int IGraphicAttributes.GraphicAttributeCount
        {
            get { return 2; }
        }

        int IGraphicAttributes.get_ID(int attrIndex)
        {
            if (attrIndex >= 0 & attrIndex < 2)
            {
                return attrIndex;
            }
            return -1;
        }

        int IGraphicAttributes.get_IDByName(string Name)
        {
            if (Name == "堤类型")
            {
                return 0;
            }
            if (Name == "堤宽")
            {
                return 1;
            }

            return -1;
        }

        string IGraphicAttributes.get_Name(int attrId)
        {
            if (attrId == 0)
                return "堤类型";
            if (attrId == 1)
                return "堤宽";

            return "";
        }

        IGraphicAttributeType IGraphicAttributes.get_Type(int attrId)
        {
            if (attrId == 0)
            {
                GraphicAttributeEnumTypeClass graphEnum = new GraphicAttributeEnumTypeClass();
                graphEnum.AddValue(0, DikeType.干堤依比例.ToString());               

                return graphEnum;
            }
            if (attrId == 1)
            {
                GraphicAttributeSizeTypeClass gaSizeType = new GraphicAttributeSizeTypeClass();
                return gaSizeType;

            }

            return null;
        }

        object IGraphicAttributes.get_Value(int attrId)
        {
            if (attrId == 0)
            {
                return DType;
            }
            if (attrId == 1)
            {
                return Width;
            }

            return 0;
        }

        void IGraphicAttributes.set_Value(int attrId, object val)
        {
            if (attrId == 0)
            {
                DType = (DikeType)val;
            }
            if (attrId == 1)
            {
                Width = (double)val;
            }

        }
        #endregion

        #region IPersistVariant Members

        UID IPersistVariant.ID
        {
            get
            {
                UID pUID;
                pUID = new UID();
                pUID.Value = "{" + this.GetType().GUID.ToString() + "}";
                return pUID;
            }
        }

        void IPersistVariant.Load(IVariantStream Stream)
        {
            int version;
            version = (int)Stream.Read();
            DType = (DikeType)Stream.Read();
            Width = (double)Stream.Read();
        }

        void IPersistVariant.Save(IVariantStream Stream)
        {
            int version;
            version = 1;
            Stream.Write(version);
            Stream.Write(DType);
            Stream.Write(Width);
        }

        #endregion
        /// <summary>
        /// 计算
        /// </summary>
        private IPolyline[] CalculateGeneralDike(IPolyline pl, bool AsRatio, double width = 1.42, double tickLength = 5)
        {
            //1.初始化参数
            List<IPolyline> pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            double halfWidth = width / 2;

            //2.做平行线
            PolylineClass ccTop = new PolylineClass();
            ccTop.ConstructOffset(pl, -1 * halfWidth, esriConstructOffsetEnum.esriConstructOffsetSimple);
            PolylineClass ccDown = new PolylineClass();
            ccDown.ConstructOffset(pl, halfWidth, esriConstructOffsetEnum.esriConstructOffsetSimple);
            //if (AsRatio)
            //{
            //    //3.生成齿线
            //    ExtendTickLine(5, 90, true, ccDown, pls);

            //}
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }

            //3.生成齿线

            ////上半部分
            //if (bUL)
            //{
            //    ExtendTickLine(tickLength, 360 - tickAngle, true, ccTop, ge);
            //}

            //if (bUR)
            //{
            //    ExtendTickLine(tickLength, tickAngle, false, ccTop, ge);
            //}


            ////下半部分
            //if (bDR)
            //{
            //    ExtendTickLine(tickLength, tickAngle, true, ccDown, ge);
            //}

            //if (bDL)
            //{
            //    ExtendTickLine(tickLength, 360 - tickAngle, false, ccDown, ge);
            //}

            //if (bDouble)
            //{
            //    double widthExtend = 0.4 * halfWidth;
            //    PolylineClass cTop2 = new PolylineClass();
            //    cTop2.ConstructOffset(pl, -1 * (halfWidth + widthExtend), esriConstructOffsetEnum.esriConstructOffsetSimple);
            //    PolylineClass cTop2Ext = new PolylineClass();
            //    bool suc1 = false;
            //    cTop2Ext.ConstructExtended(cTop2, ccTop, (int)esriCurveExtension.esriDefaultCurveExtension, ref suc1);

            //    PolylineClass cDown2 = new PolylineClass();
            //    cDown2.ConstructOffset(pl, (halfWidth + widthExtend), esriConstructOffsetEnum.esriConstructOffsetSimple);
            //    PolylineClass cDown2Ext = new PolylineClass();
            //    bool suc2 = false;
            //    cDown2Ext.ConstructExtended(cDown2, ccDown, (int)esriCurveExtension.esriDefaultCurveExtension, ref suc2);

            //    if (suc1 && suc2)
            //    {
            //        pls.Add(cTop2Ext);
            //        pls.Add(cDown2Ext);
            //    }
            //}

            pls.Add(ccTop);
            pls.Add(ccDown);

            return pls.ToArray();
        }
        /// <summary>
        /// 计算齿线
        /// </summary>
        /// <param name="length"></param>//length=5m
        /// <param name="angle"></param>
        /// <param name="frompt"></param>
        /// <param name="plnew"></param>

        public void ExtendTickLine(double length, double angle, bool frompt, IPolyline plnew, List<IPolyline> pls)
        {
            if (plnew.IsEmpty)
            {
                return;
            }
            int ToPointNumber = (int)((plnew.Length - 1) / 15);
            List<IPoint> points = new List<IPoint>();

            IPoint firstFromP = new PointClass();
            IPoint firstToP = new PointClass();
            plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, 0.2 * length, false, firstFromP);//第一个点距离线的FormPoint1m
            plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, length, false, firstToP);//第一个点距离线的FormPoint1m

            points.Add(firstToP);
            //      plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, plnew.Length + length, false, firstP);
            LineClass lineSeg = new LineClass();
            lineSeg.FromPoint = firstFromP;
            lineSeg.ToPoint = firstToP;
            lineSeg.Rotate(firstToP, angle / 180 * Math.PI);
            pls.Add(lineSeg as IPolyline);
            for (int i = 1; i < ToPointNumber + 1; i++)
            {
                IPoint FromPoint = new PointClass();
                IPoint ToPoint = new PointClass();
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, 1 + length * 3 * i, false, FromPoint);//10m
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, 1 + length * 2 * i, false, ToPoint);//15m

                LineClass lineSeg1 = new LineClass();
                lineSeg1.FromPoint = FromPoint;
                lineSeg1.ToPoint = ToPoint;
                lineSeg1.Rotate(ToPoint, angle / 180 * Math.PI);
                pls.Add(lineSeg1 as IPolyline);
            }

            //if (!firstP.IsEmpty)
            //{
            //    LineClass lineSeg = new LineClass();
            //    if (frompt)
            //    {
            //        lineSeg.FromPoint = firstP;
            //        lineSeg.ToPoint = plnew.FromPoint;
            //        lineSeg.Rotate(plnew.FromPoint, angle / 180 * Math.PI);

            //        IPoint[] fps = new IPoint[1] { lineSeg.FromPoint };

            //    }
            //    else
            //    {
            //        lineSeg.FromPoint = plnew.ToPoint;
            //        lineSeg.ToPoint = firstP;
            //        lineSeg.Rotate(plnew.ToPoint, angle / 180 * Math.PI);

            //        (plnew as IPointCollection4).AddPoint(lineSeg.ToPoint);
            //    }
            //}
        }
    }
}

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
    [Guid("19a3f6b3-9680-48f2-a3a3-cc0243bee209")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.BridgeGeometryEffect")]
    public class BridgeGeometryEffect : IGeometricEffect, IGraphicAttributes, IPersistVariant
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
        enum BridgeType { 铁路桥, 公路桥, 双层桥, 并行桥, 级面桥, 铁索桥, 栈桥, 隧道, 明峒, 引桥}

        BridgeType BType
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

        bool IsHasUpperLeftTick
        {
            get;
            set;
        }

        bool IsHasUpperRightTick
        {
            get;
            set;
        }
        bool IsHasDownLeftTick
        {
            get;
            set;
        }

        bool IsHasDownRightTick
        {
            get;
            set;
        }
        Dictionary<BridgeType, double> TickLengthDic = new Dictionary<BridgeType, double>();
        public BridgeGeometryEffect()
        {
            Width =2.83;        //1mm  1pt=0.3528mm
            TickAngle = 45;
            TickLength = 1.415;  //0.5mm
            TickLengthDic[BridgeType.铁路桥] = 1.42;
            TickLengthDic[BridgeType.公路桥] = 1.42;
            TickLengthDic[BridgeType.双层桥] = 2.55;
            TickLengthDic[BridgeType.并行桥] = 1.42;
            TickLengthDic[BridgeType.级面桥] = 1.42;
            TickLengthDic[BridgeType.栈桥] = 1.42;
            TickLengthDic[BridgeType.引桥] = 1.42;
            TickLengthDic[BridgeType.隧道] = 1.42;
            TickLengthDic[BridgeType.明峒] = 1.42;
            TickLengthDic[BridgeType.铁索桥] = 1.42;
            
           
            plIndex = -1;
            BType = BridgeType.公路桥;
            IsHasDownRightTick = true;
            IsHasDownLeftTick = true;
            IsHasUpperLeftTick = true;
            IsHasUpperLeftTick = true;
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
                if (m_pGeom==null ||m_pGeom.IsEmpty)
                {
                    return null;
                }
                if (plIndex == -1)
                {
                    if (BType==BridgeType.铁路桥 || BType==BridgeType.公路桥)
                    {
                        pls = CalculateGeneralBridge(m_pGeom as IPolyline, Width, TickLength, 45, false, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                    }
                    else if (BType == BridgeType.双层桥)
                    {
                        pls = CalculateGeneralBridge(m_pGeom as IPolyline, Width, TickLength, 45, true, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                    }
                    else if (BType==BridgeType.并行桥)
                    {
                        pls = CalculateParallelBridge(m_pGeom as IPolyline, Width, TickLength, 45, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                    }
                    else if (BType == BridgeType.级面桥)
                    {
                        pls = CalculateGradeMBridge(m_pGeom as IPolyline, Width, TickLength, 45, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                    }
                    else if (BType == BridgeType.级面桥)
                    {
                        pls = CalculateGradeMBridge(m_pGeom as IPolyline, Width, TickLength, 45, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                    }
                    else if (BType == BridgeType.铁索桥)
                    {
                        pls = CalculateChainBridge(m_pGeom as IPolyline, Width, TickLength, 45, 2.83, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                    }
                    else if (BType == BridgeType.栈桥 || BType == BridgeType.引桥)
                    {
                        pls = CalculateTestleBridge(m_pGeom as IPolyline, Width, TickLength, 45, 4.53, 2.26, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                    }
                    else if (BType == BridgeType.隧道 || BType == BridgeType.明峒)
                    {
                        pls = CalculateTunnelBridge(m_pGeom as IPolyline, Width, TickLength, 1.42);
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
            get { return "国三制图院 桥"; }
        }

        int IGraphicAttributes.GraphicAttributeCount
        {
            get { return 7; }
        }

        int IGraphicAttributes.get_ID(int attrIndex)
        {
            if (attrIndex >= 0 & attrIndex < 7)
            {
                return attrIndex;
            }
            return -1;
        }

        int IGraphicAttributes.get_IDByName(string Name)
        {
            if (Name == "桥类型")
            {
                return 0;
            }
            if (Name == "桥宽")
            {
                return 1;
            }
            if (Name == "齿长")
            {
                return 2;
            }
            if (Name == "左上")
            {
                return 3;
            }
            if (Name == "左下")
            {
                return 4;
            }
            if (Name == "右上")
            {
                return 5;
            }
            if (Name == "右下")
            {
                return 6;
            }
            
            return -1;
        }

        string IGraphicAttributes.get_Name(int attrId)
        {
            if (attrId == 0)
                return "桥类型";
            if (attrId == 1)
                return "桥宽";
            if (attrId == 2)
                return "齿长";
            if (attrId == 3)
                return "左上";
            if (attrId == 4)
                return "左下";
            if (attrId == 5)
                return "右上";
            if (attrId == 6)
                return "右下";
            
            return "";
        }

        IGraphicAttributeType IGraphicAttributes.get_Type(int attrId)
        {
            if (attrId == 0)
            {
                GraphicAttributeEnumTypeClass graphEnum= new GraphicAttributeEnumTypeClass();
                graphEnum.AddValue(0, BridgeType.铁路桥.ToString());
                graphEnum.AddValue(1, BridgeType.公路桥.ToString());
                graphEnum.AddValue(2, BridgeType.双层桥.ToString());
                graphEnum.AddValue(3, BridgeType.并行桥.ToString());
                graphEnum.AddValue(4, BridgeType.级面桥.ToString());
                graphEnum.AddValue(5, BridgeType.铁索桥.ToString());
                graphEnum.AddValue(6, BridgeType.栈桥.ToString());
                graphEnum.AddValue(7, BridgeType.隧道.ToString());
                graphEnum.AddValue(8, BridgeType.明峒.ToString());
                graphEnum.AddValue(9, BridgeType.引桥.ToString());
                return graphEnum;
            }
            if (attrId == 1)
            {
                GraphicAttributeSizeTypeClass gaSizeType = new GraphicAttributeSizeTypeClass();
                return gaSizeType;

            }
            if (attrId == 2)
            {
                GraphicAttributeSizeTypeClass gaSizeType = new GraphicAttributeSizeTypeClass();
                return gaSizeType;
            }
            if (attrId == 3)
            {
                GraphicAttributeBooleanTypeClass gaBoolType = new GraphicAttributeBooleanTypeClass();
                return gaBoolType;

            }
            if (attrId == 4)
            {
                GraphicAttributeBooleanTypeClass gaBoolType = new GraphicAttributeBooleanTypeClass();
                return gaBoolType;

            }
            if (attrId == 5)
            {
                GraphicAttributeBooleanTypeClass gaBoolType = new GraphicAttributeBooleanTypeClass();
                return gaBoolType;

            }
            if (attrId == 6)
            {
                GraphicAttributeBooleanTypeClass gaBoolType = new GraphicAttributeBooleanTypeClass();
                return gaBoolType;
            }
            
            return null;
        }

        object IGraphicAttributes.get_Value(int attrId)
        {
            if (attrId == 0)
            {
                return BType;
            }
            if (attrId == 1)
            {
                return Width;
            }
            if (attrId == 2)
            {
                return TickLength;
            }
            if (attrId == 3)
            {
                return IsHasUpperLeftTick;
            }
            if (attrId == 4)
            {
                return IsHasDownLeftTick;
            }
            if (attrId == 5)
            {
                return IsHasUpperRightTick;
            }
            if (attrId == 6)
            {
                return IsHasDownRightTick;
            }
            
            return 0;
        }

        void IGraphicAttributes.set_Value(int attrId, object val)
        {
            if (attrId == 0)
            {
                BType = (BridgeType)val ;
                TickLength = TickLengthDic[BType];
            }
            if (attrId == 1)
            {
                Width = (double)val;
            }
            if (attrId == 2)
            {
                TickLength = (double)val;
            }
            if (attrId == 3)
            {
                IsHasUpperLeftTick = (bool)val;
            }
            if (attrId == 4)
            {
                IsHasDownLeftTick = (bool)val;
            }
            if (attrId == 5)
            {
                IsHasUpperRightTick = (bool)val;
            }
            if (attrId == 6)
            {
                IsHasDownRightTick = (bool)val;
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
                pUID.Value = "{"+this.GetType().GUID.ToString()+"}";
                return pUID;
            }
        }

        void IPersistVariant.Load(IVariantStream Stream)
        {
            int version ;
            version = (int)Stream.Read();
            BType = (BridgeType)Stream.Read();
            Width = (double)Stream.Read();
            IsHasUpperLeftTick = (bool)Stream.Read();
            IsHasDownLeftTick = (bool)Stream.Read();
            IsHasUpperRightTick = (bool)Stream.Read();
            IsHasDownRightTick = (bool)Stream.Read();
            TickLength = (double)Stream.Read();
        }

        void IPersistVariant.Save(IVariantStream Stream)
        {
            int version;
            version = 1;
            Stream.Write(version);
            Stream.Write(BType);
            Stream.Write(Width);
            Stream.Write(IsHasUpperLeftTick);
            Stream.Write(IsHasDownLeftTick);
            Stream.Write(IsHasUpperRightTick);
            Stream.Write(IsHasDownRightTick);
            Stream.Write(TickLength);
        }

        #endregion

        /// <summary>
        /// 计算普通桥(双层桥)
        /// </summary>
        private IPolyline[] CalculateGeneralBridge(IPolyline pl, double width = 2.83, double tickLength = 1.42, double tickAngle = 45, bool bDouble = false, bool bUL = true, bool bUR = true, bool bDL = true, bool bDR = true)
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
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }
            
            //3.生成齿线

            //上半部分
            if (bUL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, true, ccTop, ge);
            }

            if (bUR)
            {
                ExtendTickLine(tickLength, tickAngle, false, ccTop, ge);
            }
            

            //下半部分
            if (bDR)
            {
                ExtendTickLine(tickLength, tickAngle, true, ccDown, ge);
            }

            if (bDL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, false, ccDown, ge);
            }
            
            if (bDouble)
            {
                double widthExtend = 0.4 * halfWidth;
                PolylineClass cTop2 = new PolylineClass();
                cTop2.ConstructOffset(pl, -1 * (halfWidth + widthExtend), esriConstructOffsetEnum.esriConstructOffsetSimple);
                PolylineClass cTop2Ext = new PolylineClass();
                bool suc1 = false;
                cTop2Ext.ConstructExtended(cTop2, ccTop, (int)esriCurveExtension.esriDefaultCurveExtension, ref suc1);

                PolylineClass cDown2 = new PolylineClass();
                cDown2.ConstructOffset(pl, (halfWidth + widthExtend), esriConstructOffsetEnum.esriConstructOffsetSimple);
                PolylineClass cDown2Ext = new PolylineClass();
                bool suc2 = false;
                cDown2Ext.ConstructExtended(cDown2, ccDown, (int)esriCurveExtension.esriDefaultCurveExtension, ref suc2);

                if (suc1 && suc2)
                {
                    pls.Add(cTop2Ext);
                    pls.Add(cDown2Ext);
                }
            }

            pls.Add(ccTop);
            pls.Add(ccDown);

            return pls.ToArray();
        }


        /// <summary>
        /// 计算隧道、明峒
        /// </summary>
        private IPolyline[] CalculateTunnelBridge(IPolyline pl, double width = 2.83, double solidLenth = 4.53, double dashLenth = 2.26)
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
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }

            IPolyline upline = (ccTop as IClone).Clone() as IPolyline;
            IPolyline newUp = new PolylineClass();

            IPolyline downline = (ccDown as IClone).Clone() as IPolyline;
            IPolyline newDown = new PolylineClass();

            var helper = new GeometricEffectDashClass();
            var d = new[] { solidLenth, dashLenth };
            helper.set_Value(0, d);
            helper.set_Value(1, 1);

            helper.Reset(upline);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                IGeometry cg = (g as IClone).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);

                IGeometryCollection geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    IGeometry geo = (geoCol.get_Geometry(i) as IClone).Clone() as IGeometry ;
                    
                    (newUp as IGeometryCollection).AddGeometry(geo);
                }
                

            }

            helper.Reset(downline);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                IGeometry cg = (g as IClone).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);

                IGeometryCollection geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    IGeometry geo = (geoCol.get_Geometry(i) as IClone).Clone() as IGeometry;
                    (newDown as IGeometryCollection).AddGeometry(geo);
                }

            }

            pls.Add(newUp);
            pls.Add(newDown);
            return pls.ToArray();
        }

        /// <summary>
        /// 计算栈桥
        /// </summary>
        private IPolyline[] CalculateTestleBridge(IPolyline pl, double width = 2.83, double tickLength = 1.42, double tickAngle = 45, double solidLenth = 4.53, double dashLenth = 2.26, bool bUL = true, bool bUR = true, bool bDL = true, bool bDR = true)
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
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }

            IPolyline upline = (ccTop as IClone).Clone() as IPolyline;
            IPolyline newUp = new PolylineClass();

            
            IPolyline downline = (ccDown as IClone).Clone() as IPolyline;
            IPolyline newDown = new PolylineClass();

            var helper = new GeometricEffectDashClass();
            var d = new[] { solidLenth, dashLenth };
            helper.set_Value(0, d);
            helper.set_Value(1, 1);
            helper.Reset(upline);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                IGeometry cg = (g as IClone).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);

                IGeometryCollection geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    IGeometry geo = (geoCol.get_Geometry(i) as IClone).Clone() as IGeometry;

                    (newUp as IGeometryCollection).AddGeometry(geo);
                }


            }

            helper.Reset(downline);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                IGeometry cg = (g as IClone).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);
                IGeometryCollection geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    IGeometry geo = (geoCol.get_Geometry(i) as IClone).Clone() as IGeometry;
                    (newDown as IGeometryCollection).AddGeometry(geo);
                }

            }

            //3.生成齿线

            //上半部分
            if (bUL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, true, newUp, ge);
            }

            if (bUR)
            {
                ExtendTickLine(tickLength, tickAngle, false, newUp, ge);
            }


            //下半部分
            if (bDR)
            {
                ExtendTickLine(tickLength, tickAngle, true, newDown, ge);
            }

            if (bDL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, false, newDown, ge);
            }

            pls.Add(newUp);
            pls.Add(newDown);

            return pls.ToArray();
        }

        /// <summary>
        /// 计算铁索桥
        /// </summary>
        private IPolyline[] CalculateChainBridge(IPolyline pl, double width = 2.83, double tickLength = 1.42, double tickAngle = 45, double gaps=2.83, bool bUL = true, bool bUR = true, bool bDL = true, bool bDR = true)
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
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }

            //3.中间缆索
            PolylineClass midChainLine = new PolylineClass();
            (midChainLine as IPointCollection).AddPoint(ccTop.FromPoint);
            double topDis=0,donwDis=0;
            while (true)
            {
                if ((donwDis + gaps) > ccDown.Length && (topDis + gaps) > ccTop.Length)
                {
                    break;
                }

                if (donwDis == 0)
                {
                    IPoint pt = new PointClass();
                    donwDis += gaps / 2;
                    ccDown.QueryPoint(esriSegmentExtension.esriNoExtension, donwDis, false, pt);
                    (midChainLine as IPointCollection).AddPoint(pt);
                }
                else
                {
                    if ((topDis + gaps) <= ccTop.Length)
                    {
                        IPoint topPt = new PointClass();
                        topDis += gaps;
                        ccTop.QueryPoint(esriSegmentExtension.esriNoExtension, topDis, false, topPt);
                        (midChainLine as IPointCollection).AddPoint(topPt);
                    }

                    if ((donwDis + gaps) <= ccDown.Length)
                    {
                        IPoint downPt = new PointClass();
                        donwDis += gaps;
                        ccDown.QueryPoint(esriSegmentExtension.esriNoExtension, donwDis, false, downPt);
                        (midChainLine as IPointCollection).AddPoint(downPt);
                    }
                }

            }
            //4.生成齿线
            //上半部分
            if (bUL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, true, ccTop, ge);
            }

            if (bUR)
            {
                ExtendTickLine(tickLength, tickAngle, false, ccTop, ge);
            }


            //下半部分
            if (bDR)
            {
                ExtendTickLine(tickLength, tickAngle, true, ccDown, ge);
            }

            if (bDL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, false, ccDown, ge);
            }

            pls.Add(midChainLine);
            pls.Add(ccTop);
            pls.Add(ccDown);

            return pls.ToArray();
        }

        /// <summary>
        /// 计算级面桥
        /// </summary>
        private IPolyline[] CalculateGradeMBridge(IPolyline pl, double width = 2.83, double tickLength = 1.42, double tickAngle = 45, bool bUL = true, bool bUR = true, bool bDL = true, bool bDR = true)
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
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }


            //3.两面横梁
            PolylineClass leftLine = new PolylineClass();
            (leftLine as IPointCollection).AddPoint(ccTop.FromPoint);
            (leftLine as IPointCollection).AddPoint(ccDown.FromPoint);

            PolylineClass rightLine = new PolylineClass();
            (rightLine as IPointCollection).AddPoint(ccTop.ToPoint);
            (rightLine as IPointCollection).AddPoint(ccDown.ToPoint);
            //4.生成齿线

            //上半部分
            if (bUL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, true, ccTop, ge);
            }

            if (bUR)
            {
                ExtendTickLine(tickLength, tickAngle, false, ccTop, ge);
            }


            //下半部分
            if (bDR)
            {
                ExtendTickLine(tickLength, tickAngle, true, ccDown, ge);
            }

            if (bDL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, false, ccDown, ge);
            }


            pls.Add(leftLine);
            pls.Add(rightLine);
            pls.Add(ccTop);
            pls.Add(ccDown);

            return pls.ToArray();
        }
        /// <summary>
        /// 计算并行桥
        /// </summary>
        private IPolyline[] CalculateParallelBridge(IPolyline pl, double width = 5.66, double tickLength = 1.42, double tickAngle = 45, bool bUL = true, bool bUR = true, bool bDL = true, bool bDR = true)
        {
            //1.初始化参数
            List<IPolyline> pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            //pls.Add(pl);

            double halfWidth = width / 2;

            //2.做平行线
            PolylineClass ccTop = new PolylineClass();
            ccTop.ConstructOffset(pl, -1 * halfWidth, esriConstructOffsetEnum.esriConstructOffsetSimple);
            PolylineClass ccDown = new PolylineClass();
            ccDown.ConstructOffset(pl, halfWidth, esriConstructOffsetEnum.esriConstructOffsetSimple);
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }

            //3.生成齿线
            //上半部分
            if (bUL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, true, ccTop, ge);
            }

            if (bUR)
            {
                ExtendTickLine(tickLength, tickAngle, false, ccTop, ge);
            }


            //下半部分
            if (bDR)
            {
                ExtendTickLine(tickLength, tickAngle, true, ccDown, ge);
            }

            if (bDL)
            {
                ExtendTickLine(tickLength, 360 - tickAngle, false, ccDown, ge);
            }

            pls.Add(ccTop);
            pls.Add(ccDown);

            return pls.ToArray();
        }
        /// <summary>
        /// 计算齿线
        /// </summary>
        /// <param name="length"></param>
        /// <param name="angle"></param>
        /// <param name="frompt"></param>
        /// <param name="plnew"></param>
        /// <param name="ge"></param>
        public void ExtendTickLine(double length, double angle, bool frompt, IPolyline plnew, IGeometryBridge ge)
        {
            if (plnew.IsEmpty)
            {
                return;
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
                LineClass lineSeg = new LineClass();
                if (frompt)
                {
                    lineSeg.FromPoint = firstP;
                    lineSeg.ToPoint = plnew.FromPoint;
                    lineSeg.Rotate(plnew.FromPoint, angle / 180 * Math.PI);

                    IPoint[] fps = new IPoint[1] { lineSeg.FromPoint };

                    ge.InsertPoints(plnew as IPointCollection4, 0, ref fps);
                }
                else
                {
                    lineSeg.FromPoint = plnew.ToPoint;
                    lineSeg.ToPoint = firstP;
                    lineSeg.Rotate(plnew.ToPoint, angle / 180 * Math.PI);

                    (plnew as IPointCollection4).AddPoint(lineSeg.ToPoint);
                }
            }
        }
    }
}

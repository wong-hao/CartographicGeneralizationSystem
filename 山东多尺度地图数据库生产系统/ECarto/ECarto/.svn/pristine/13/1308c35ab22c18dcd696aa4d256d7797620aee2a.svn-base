using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [SMGIClassName("面状桥")]
    [Guid("EC57EF11-D1DA-41F1-9733-633E77F5BF73")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.BridgePolygonEffect")]
    public class BridgePolygonEffect : BaseGeographicEffect, IGeometricEffect
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

        [SMGIGraphic("长线", 1, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        public bool IsLong { get; set; }
        [SMGIGraphic("反向", 2, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        public bool IsFan { get; set; }
        bool m_bDone;
        IGeometry m_pGeom;
        int plIndex = -1;
        IPolyline[] pls;
        //面状桥汇总
        public enum BridgeType { 一般桥, 涵洞, 栈桥, 级面桥, 滚水坝, 滚水坝虚线, 拦水坝通车, 拦水坝不通车, 干堤, 隧道, 并行桥, 铁索桥, 圆弧桥, 明峒, 引桥, 拦水坝加固点 }
        [SMGIGraphic("类型", 0, typeof (BridgeType))]
        public BridgeType BType
        {
            get;
            set;
        }
        double Width
        {
            get;
            set;
        }
        [SMGIGraphic("齿长", 3, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        double TickLength
        {
            get;
            set;
        }
        [SMGIGraphic("角度", 4, GraphicAttributeTypeEnum.GraphicAttributeDoubleType)]
        double TickAngle
        {
            get;
            set;
        }
        [SMGIGraphic("左上", 5, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        bool IsHasUpperLeftTick
        {
            get;
            set;
        }
        [SMGIGraphic("右上", 6, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        bool IsHasUpperRightTick
        {
            get;
            set;
        }
        [SMGIGraphic("左下", 7, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        bool IsHasDownLeftTick
        {
            get;
            set;
        }
        [SMGIGraphic("右下", 8, GraphicAttributeTypeEnum.GraphicAttributeBooleanType)]
        bool IsHasDownRightTick
        {
            get;
            set;
        }
        public BridgePolygonEffect()
        {
            Width = 2.83;        //1mm  1pt=0.3528mm
            TickAngle = 45;
            TickLength = 2.9;  //0.5mm
            plIndex = -1;
            BType = BridgeType.一般桥;
            IsHasDownRightTick = true;
            IsHasDownLeftTick = true;
            IsHasUpperRightTick = true;
            IsHasUpperLeftTick = true;
            IsLong = true;
            IsFan = false;
        }
        //nextgeometry
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
                if (BType==BridgeType.一般桥)
                {
                    pls = MBridge(m_pGeom as IPolygon, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                }
                else if (BType == BridgeType.涵洞)
                {
                    pls = MBridge1(m_pGeom as IPolygon, 5.66, 2.83, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                }
                else if (BType == BridgeType.栈桥)
                {
                    pls = MBridge2(m_pGeom as IPolygon, 5.66, 2.83, IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                }

                else if (BType == BridgeType.级面桥)
                {
                    pls = MBridge3(m_pGeom as IPolygon);
                }
                else if (BType == BridgeType.滚水坝)
                {
                    pls = MBridge4(m_pGeom as IPolygon);
                }
                else if (BType == BridgeType.滚水坝虚线)
                {
                    pls = MBridge5(m_pGeom as IPolygon);
                }
                else if (BType == BridgeType.拦水坝通车)
                {
                    pls = MBridge6(m_pGeom as IPolygon);
                }
                else if (BType == BridgeType.拦水坝不通车)
                {
                    pls = MBridge7(m_pGeom as IPolygon);
                }
                else if (BType == BridgeType.拦水坝加固点)
                {
                    pls = MBridge66(m_pGeom as IPolygon);
                }
                else if (BType == BridgeType.干堤)
                {
                    pls = MBridge9(m_pGeom as IPolygon);
                }
                else if (BType == BridgeType.隧道)
                {
                    pls = MBridge8(m_pGeom as IPolygon);
                }
                //else if (BType == BridgeType.铁索桥)
                //{
                //    pls = CalculateChainBridge(m_pGeom as IPolyline, Width, 1.42, 45, 2.83,IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                //}
                //else if (BType == BridgeType.栈桥 || BType == BridgeType.引桥)
                //{
                //    pls = CalculateTestleBridge(m_pGeom as IPolyline, Width, 1.42, 45,4.53,2.26,IsHasUpperLeftTick, IsHasUpperRightTick, IsHasDownRightTick, IsHasDownLeftTick);
                //}
                //else if (BType == BridgeType.隧道 || BType == BridgeType.明峒)
                //{
                //    pls = CalculateTunnelBridge(m_pGeom as IPolyline, Width,2.83,1.42);
                //}
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
        //IGeometry en is't used
        IEnumerator<IGeometry> en;
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

        private IPolyline[] MBridge(IPolygon pl, bool bUl = true, bool bUr = true, bool bDl = true, bool bDr = true)
        {
            //1.初始化参数
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            //计算角度顺序
            var xulie = new Dictionary<int, double>();
            for (int i = 0; i < ((ISegmentCollection) pl).SegmentCount; i++)
            {
                double value;
                if (i==0)
                {
                    value = ((ILine) ((ISegmentCollection) pl).Segment[i]).Angle -
                            (((ISegmentCollection) pl).get_Segment(((ISegmentCollection) pl).SegmentCount - 1) as ILine).Angle;
                }
                else
                {
                    value = (((ISegmentCollection) pl).get_Segment(i) as ILine).Angle -
                            (((ISegmentCollection) pl).get_Segment(i - 1) as ILine).Angle;
                }
                xulie.Add(i,Math.Abs(value));
            }
            var px = xulie.OrderByDescending(t => t.Value).ToDictionary(p => p.Key, o => o.Value); 
            var hao=new List<int>();hao.Clear();
            int jis = 0;
            foreach (var keyValuePair in px)
            {
                if (jis<=3)
                {
                    hao.Add(keyValuePair.Key);
                    jis++;
                }
            }
            var zhao = hao.OrderBy(i => i).ToList();
            var fenbian=new List<PolylineClass>();
            for (int i = 0; i < zhao.Count; i++)
            {
                var leng = i==zhao.Count-1 ? ((ISegmentCollection) pl).SegmentCount : zhao[i + 1];
                var linshi = new PolylineClass();
                (linshi as IPointCollection).AddPoint(((ISegmentCollection) pl).Segment[zhao[i]].FromPoint);
                for (int j = zhao[i]; j < leng; j++)
                {
                    (linshi as IPointCollection).AddPoint(((ISegmentCollection) pl).Segment[j].ToPoint);
                }
                if (zhao[0]!=0&&i==zhao.Count-1)
                {
                    for (int j = 0; j < zhao[0]; j++)
                    {
                        (linshi as IPointCollection).AddPoint(((ISegmentCollection) pl).Segment[j].ToPoint);
                    }
                }
                fenbian.Add(linshi);
            }
            //2.做平行线
            var ccTop = new PolylineClass();
            double maxLength = fenbian.Max(i => i.Length);
            var maxLine = fenbian.First(i => Math.Abs(i.Length - maxLength) <= 0);
            //(ccTop as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(0).FromPoint);
            //(ccTop as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(0).ToPoint);
            var ccDown = new PolylineClass();
            var cixu= fenbian.IndexOf(maxLine);
            if (fenbian.Count != 4) return null;
            if (IsLong)
            {
                ccTop = fenbian[cixu];
                if (cixu == 0 || cixu == 1)
                {
                    ccDown = fenbian[cixu + 2];
                }
                else if (cixu == 2 || cixu == 3)
                {
                    ccDown = fenbian[cixu - 2];
                }
            }
            else
            {
                if (cixu==0)
                {
                    ccTop = fenbian[cixu + 1];
                    ccDown = fenbian[cixu + 3];
                }
                else if (cixu == 1)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                }
                else if (cixu == 2)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                }
                else if (cixu == 3)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu - 3];
                }
            }
            //(ccDown as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(2).ToPoint);
            //(ccDown as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(2).FromPoint);
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }


            //3.两面横梁
            //PolylineClass leftLine = new PolylineClass();
            //(leftLine as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(1).FromPoint);
            //(leftLine as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(1).ToPoint);

            //PolylineClass rightLine = new PolylineClass();
            //(rightLine as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(3).FromPoint);
            //(rightLine as IPointCollection).AddPoint((pl as ISegmentCollection).get_Segment(3).ToPoint);
            //4.生成齿线

            //上半部分
            if (bUl)
            {
                ExtendTickLine(TickLength, 360 - TickAngle, true, ccTop, ge);
            }
            
            if (bUr)
            {
                ExtendTickLine(TickLength, TickAngle, false, ccTop, ge);
            }


            //下半部分
            if (bDr)
            {
                ExtendTickLine(TickLength, 360-TickAngle, true, ccDown, ge);
            }

            if (bDl)
            {
                ExtendTickLine(TickLength, TickAngle, false, ccDown, ge);
            }


            //pls.Add(leftLine);
            //pls.Add(rightLine);
            pls.Add(ccTop);
            pls.Add(ccDown);
            return pls.ToArray();
        }
        private IPolyline[] MBridge1(IPolygon pl, double solidLenth = 2.83, double dashLenth = 1.42, bool bUl = true, bool bUr = true, bool bDl = true, bool bDr = true)
        {
            ////1.初始化参数
            //var pls = new List<IPolyline>();
            //IGeometryBridge ge = new GeometryEnvironmentClass();
            //var topo = pg as ITopologicalOperator;
            //var pll = topo.Boundary as IPolyline;
            //var segCol = pll as ISegmentCollection;
            //if (segCol.SegmentCount != 4)
            //{
            //    return null;
            //}
            //IPolyline curpl = new PolylineClass();
            //(curpl as ISegmentCollection).AddSegment(segCol.Segment[0]);
            //IPolyline curpl1 = new PolylineClass();
            //(curpl1 as ISegmentCollection).AddSegment(segCol.Segment[1]);
            //IPolyline curpl2 = new PolylineClass();
            //(curpl2 as ISegmentCollection).AddSegment(segCol.Segment[2]);
            //IPolyline curpl3 = new PolylineClass();
            //(curpl3 as ISegmentCollection).AddSegment(segCol.Segment[3]);
            
            ////IPolyline upline = (ccTop as IClone).Clone() as IPolyline;
            //IPolyline upline;
            //IPolyline newUp = new PolylineClass();

            ////IPolyline downline = (ccDown as IClone).Clone() as IPolyline;
            //IPolyline downline;
            //IPolyline newDown = new PolylineClass();
            //if (curpl.Length > curpl1.Length)
            //{
            //    upline = (curpl as IClone).Clone() as IPolyline;
            //    downline = (curpl2 as IClone).Clone() as IPolyline;
            //}
            //else
            //{
            //    upline = (curpl1 as IClone).Clone() as IPolyline;
            //    downline = (curpl3 as IClone).Clone() as IPolyline;
            //}
            //var helper = new GeometricEffectDashClass();
            //var d = new[] { solidLenth, dashLenth };
            //helper.set_Value(0, d);
            //helper.set_Value(1, 1);
            //helper.Reset(upline);
            //while (true)
            //{
            //    var g = helper.NextGeometry();
            //    if (g == null)
            //        break;
            //    var cg = ((IClone) g).Clone() as IGeometry;
            //    Marshal.ReleaseComObject(g);
            //    var geoCol = cg as IGeometryCollection;
            //    for (int i = 0; i < geoCol.GeometryCount; i++)
            //    {
            //        var geo = ((IClone) geoCol.Geometry[i]).Clone() as IGeometry;
            //        (newUp as IGeometryCollection).AddGeometry(geo);
            //    }
            //}
            //helper.Reset(downline);
            //while (true)
            //{
            //    var g = helper.NextGeometry();
            //    if (g == null)
            //        break;
            //    var cg = (g as IClone).Clone() as IGeometry;
            //    Marshal.ReleaseComObject(g);
            //    var geoCol = cg as IGeometryCollection;
            //    for (int i = 0; i < geoCol.GeometryCount; i++)
            //    {
            //        var geo = (geoCol.Geometry[i] as IClone).Clone() as IGeometry;
            //        (newDown as IGeometryCollection).AddGeometry(geo);
            //    }

            //}
            //pls.Add(newUp);
            //pls.Add(newDown);

            ////3.两面横梁
            //IPolyline leftLine = new PolylineClass();
            //(leftLine as IPointCollection).AddPoint(upline.ToPoint);
            //(leftLine as IPointCollection).AddPoint(downline.FromPoint);

            //IPolyline rightLine = new PolylineClass();
            //(rightLine as IPointCollection).AddPoint(upline.FromPoint);
            //(rightLine as IPointCollection).AddPoint(downline.ToPoint);
            ////4.生成齿线
            ////上半部分
            //if (bUl)
            //{
            //    ExtendTickLine(TickLength, 360 - TickAngle, true, leftLine, ge);
            //}

            //if (bUr)
            //{
            //    ExtendTickLine(TickLength, TickAngle, false, leftLine, ge);
            //}


            ////下半部分
            //if (bDr)
            //{
            //    ExtendTickLine(TickLength, TickAngle, true, rightLine, ge);
            //}

            //if (bDl)
            //{
            //    ExtendTickLine(TickLength, 360 - TickAngle, false, rightLine, ge);
            //}


            //pls.Add(leftLine);
            //pls.Add(rightLine);
            //return pls.ToArray();
            //1.初始化参数
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            //计算角度顺序
            var xulie = new Dictionary<int, double>();
            for (int i = 0; i < ((ISegmentCollection)pl).SegmentCount; i++)
            {
                double value;
                if (i == 0)
                {
                    value = ((ILine)((ISegmentCollection)pl).Segment[i]).Angle -
                            (((ISegmentCollection)pl).get_Segment(((ISegmentCollection)pl).SegmentCount - 1) as ILine).Angle;
                }
                else
                {
                    value = (((ISegmentCollection)pl).get_Segment(i) as ILine).Angle -
                            (((ISegmentCollection)pl).get_Segment(i - 1) as ILine).Angle;
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
                var leng = i == zhao.Count - 1 ? ((ISegmentCollection)pl).SegmentCount : zhao[i + 1];
                var linshi = new PolylineClass();
                (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[zhao[i]].FromPoint);
                for (int j = zhao[i]; j < leng; j++)
                {
                    (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                }
                if (zhao[0] != 0 && i == zhao.Count - 1)
                {
                    for (int j = 0; j < zhao[0]; j++)
                    {
                        (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                    }
                }
                fenbian.Add(linshi);
            }
            if (fenbian.Count != 4) return null;
            //2.做平行线
            var ccTop = new PolylineClass();
            double maxLength = fenbian.Max(i => i.Length);
            var maxLine = fenbian.First(i => Math.Abs(i.Length - maxLength) <= 0);
            var ccDown = new PolylineClass();
            var leftLine = new PolylineClass();
            var rightLine = new PolylineClass();
            IPolyline newUp=new PolylineClass();
            IPolyline newDown=new PolylineClass();
            var cixu = fenbian.IndexOf(maxLine);
            if (IsLong)
            {
                ccTop = fenbian[cixu];
                if (cixu == 0)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu + 3];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 1)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 2)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 3)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu - 3];
                }
            }
            else
            {
                if (cixu == 0)
                {
                    ccTop = fenbian[cixu + 3];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 1)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 2)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu - 2];
                }
                else if (cixu == 3)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu - 3];
                    leftLine = fenbian[cixu - 2];
                }
                rightLine = fenbian[cixu];
            }
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }
            var helper = new GeometricEffectDashClass();
            var d = new[] { solidLenth, dashLenth };
            helper.set_Value(0, d);
            helper.set_Value(1, 1);
            helper.Reset(ccTop);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                var cg = ((IClone)g).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);
                var geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    var geo = ((IClone)geoCol.Geometry[i]).Clone() as IGeometry;
                    (newUp as IGeometryCollection).AddGeometry(geo);
                }
            }
            helper.Reset(ccDown);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                var cg = (g as IClone).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);
                var geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    var geo = (geoCol.Geometry[i] as IClone).Clone() as IGeometry;
                    (newDown as IGeometryCollection).AddGeometry(geo);
                }

            }
            //4.生成齿线

            //上半部分
            if (bUl)
            {
                ExtendTickLine(TickLength, 360 - TickAngle, true, leftLine, ge);
            }

            if (bUr)
            {
                ExtendTickLine(TickLength, TickAngle, false, leftLine, ge);
            }


            //下半部分
            if (bDr)
            {
                ExtendTickLine(TickLength, 360 - TickAngle, true, rightLine, ge);
            }

            if (bDl)
            {
                ExtendTickLine(TickLength, TickAngle, false, rightLine, ge);
            }
            pls.Add(leftLine);
            pls.Add(rightLine);
            pls.Add(newUp);
            pls.Add(newDown);
            return pls.ToArray();
        }
        private IPolyline[] MBridge2(IPolygon pg, double solidLenth = 2.83, double dashLenth = 1.42, bool bUl = true, bool bUr = true, bool bDl = true, bool bDr = true)
        {
            //1.初始化参数
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            var topo = pg as ITopologicalOperator;
            var pll = topo.Boundary as IPolyline;
            var segCol = pll as ISegmentCollection;
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

            //IPolyline upline = (ccTop as IClone).Clone() as IPolyline;
            IPolyline upline;
            IPolyline newUp = new PolylineClass();

            //IPolyline downline = (ccDown as IClone).Clone() as IPolyline;
            IPolyline downline;
            IPolyline newDown = new PolylineClass();
            if (curpl.Length > curpl1.Length)
            {
                upline = (curpl as IClone).Clone() as IPolyline;
                downline = (curpl2 as IClone).Clone() as IPolyline;
            }
            else
            {
                upline = (curpl1 as IClone).Clone() as IPolyline;
                downline = (curpl3 as IClone).Clone() as IPolyline;
            }
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
                var cg = ((IClone)g).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);
                var geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    var geo = ((IClone)geoCol.Geometry[i]).Clone() as IGeometry;
                    (newUp as IGeometryCollection).AddGeometry(geo);
                }
            }
            helper.Reset(downline);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                var cg = (g as IClone).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);
                var geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    var geo = (geoCol.Geometry[i] as IClone).Clone() as IGeometry;
                    (newDown as IGeometryCollection).AddGeometry(geo);
                }

            }
            pls.Add(newUp);
            pls.Add(newDown);

            //3.两面横梁
            IPolyline leftLine = new PolylineClass();
            (leftLine as IPointCollection).AddPoint(upline.ToPoint);
            (leftLine as IPointCollection).AddPoint(downline.FromPoint);

            IPolyline rightLine = new PolylineClass();
            (rightLine as IPointCollection).AddPoint(upline.FromPoint);
            (rightLine as IPointCollection).AddPoint(downline.ToPoint);
            
            //上半部分
            if (bUl)
            {
                pls.Add(ExtendTickLine1(TickLength, 360 - TickAngle, true, leftLine, ge));
            }

            if (bUr)
            {
                pls.Add(ExtendTickLine1(TickLength, TickAngle, false, leftLine, ge));
            }


            //下半部分
            if (bDr)
            {
                pls.Add(ExtendTickLine1(TickLength, TickAngle, true, rightLine, ge));
            }

            if (bDl)
            {
                pls.Add(ExtendTickLine1(TickLength, 360 - TickAngle, false, rightLine, ge));
            }
            return pls.ToArray();
        }
        private IPolyline[] MBridge3(IPolygon pg)
        {
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
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
            if (curpl.Length > curpl1.Length)
            {
                IPoint sPoint = new PointClass();
                IPoint sPoint1 = new PointClass();
                IPoint sPoint2 = new PointClass();
                IPoint sPoint3 = new PointClass();
                curpl.QueryPoint(esriSegmentExtension.esriNoExtension, 3, false, sPoint);
                curpl.QueryPoint(esriSegmentExtension.esriNoExtension, 6, false, sPoint1);
                curpl.QueryPoint(esriSegmentExtension.esriNoExtension, curpl.Length - 6, false, sPoint2);
                curpl.QueryPoint(esriSegmentExtension.esriNoExtension, curpl.Length - 3, false, sPoint3);
                IPoint ePoint = new PointClass();
                IPoint ePoint1 = new PointClass();
                IPoint ePoint2 = new PointClass();
                IPoint ePoint3 = new PointClass();
                curpl2.QueryPoint(esriSegmentExtension.esriNoExtension, 3, false, ePoint);
                curpl2.QueryPoint(esriSegmentExtension.esriNoExtension, 6, false, ePoint1);
                curpl2.QueryPoint(esriSegmentExtension.esriNoExtension, curpl2.Length - 6, false, ePoint2);
                curpl2.QueryPoint(esriSegmentExtension.esriNoExtension, curpl2.Length - 3, false, ePoint3);
                IPolyline pl = new PolylineClass { FromPoint = sPoint, ToPoint = ePoint3 };
                IPolyline pl1 = new PolylineClass { FromPoint = sPoint1, ToPoint = ePoint2 };
                IPolyline pl2 = new PolylineClass { FromPoint = sPoint2, ToPoint = ePoint1 };
                IPolyline pl3 = new PolylineClass { FromPoint = sPoint3, ToPoint = ePoint };
                pls.Add(pl);
                pls.Add(pl1);
                pls.Add(pl2);
                pls.Add(pl3);
                pls.Add(ExtendTickLine1(TickLength, 360 - TickAngle, true, curpl, ge));
                pls.Add(ExtendTickLine1(TickLength, TickAngle, false, curpl, ge));
                pls.Add(ExtendTickLine1(TickLength, 360 - TickAngle, true, curpl2, ge));
                pls.Add(ExtendTickLine1(TickLength, TickAngle, false, curpl2, ge));

            }
            else
            {
                IPoint sPoint = new PointClass();
                IPoint sPoint1 = new PointClass();
                IPoint sPoint2 = new PointClass();
                IPoint sPoint3 = new PointClass();
                curpl1.QueryPoint(esriSegmentExtension.esriNoExtension, 3, false, sPoint);
                curpl1.QueryPoint(esriSegmentExtension.esriNoExtension, 6, false, sPoint1);
                curpl1.QueryPoint(esriSegmentExtension.esriNoExtension, curpl1.Length - 6, false, sPoint2);
                curpl1.QueryPoint(esriSegmentExtension.esriNoExtension, curpl1.Length - 3, false, sPoint3);
                IPoint ePoint = new PointClass();
                IPoint ePoint1 = new PointClass();
                IPoint ePoint2 = new PointClass();
                IPoint ePoint3 = new PointClass();
                curpl3.QueryPoint(esriSegmentExtension.esriExtendAtFrom, 3, false, ePoint);
                curpl3.QueryPoint(esriSegmentExtension.esriExtendAtFrom, 6, false, ePoint1);
                curpl3.QueryPoint(esriSegmentExtension.esriExtendAtTo, curpl3.Length - 6, false, ePoint2);
                curpl3.QueryPoint(esriSegmentExtension.esriExtendAtTo, curpl3.Length - 3, false, ePoint3);
                IPolyline pl = new PolylineClass { FromPoint = sPoint, ToPoint = ePoint3 };
                IPolyline pl1 = new PolylineClass { FromPoint = sPoint1, ToPoint = ePoint2 };
                IPolyline pl2 = new PolylineClass { FromPoint = sPoint2, ToPoint = ePoint1 };
                IPolyline pl3 = new PolylineClass { FromPoint = sPoint3, ToPoint = ePoint };
                pls.Add(pl);
                pls.Add(pl1);
                pls.Add(pl2);
                pls.Add(pl3);
                pls.Add(ExtendTickLine1(TickLength, 360 - TickAngle, true, curpl1, ge));
                pls.Add(ExtendTickLine1(TickLength, TickAngle, false, curpl1, ge));
                pls.Add(ExtendTickLine1(TickLength, 360 - TickAngle, true, curpl3, ge));
                pls.Add(ExtendTickLine1(TickLength, TickAngle, false, curpl3, ge));
            }
            return pls.ToArray();
        }
        private IPolyline[] MBridge4(IPolygon pl)
        {
            //var pls = new List<IPolyline>();
            //var pll = ((ITopologicalOperator)pg).Boundary as IPolyline;
            //var segCol = pll as ISegmentCollection;
            //if (segCol == null) return null;
            //if (segCol.SegmentCount != 4)
            //{
            //    return null;
            //}
            //IPolyline curpl = new PolylineClass();
            //(curpl as ISegmentCollection).AddSegment(segCol.Segment[0]);
            //IPolyline curpl1 = new PolylineClass();
            //(curpl1 as ISegmentCollection).AddSegment(segCol.Segment[1]);
            //IPolyline curpl2 = new PolylineClass();
            //(curpl2 as ISegmentCollection).AddSegment(segCol.Segment[2]);
            //IPolyline curpl3 = new PolylineClass();
            //(curpl3 as ISegmentCollection).AddSegment(segCol.Segment[3]);
            //if (curpl.Length > curpl1.Length)
            //{
            //    pls.Add(curpl);
            //    pls.Add(curpl2);
            //    ExtendTickLine(1.42, 270, curpl2, pls);

            //}
            //else
            //{
            //    pls.Add(curpl1);
            //    pls.Add(curpl3);
            //    ExtendTickLine(1.42, 270, curpl3, pls);
            //}
            //return pls.ToArray();
            //1.初始化参数
            var pls = new List<IPolyline>();
            //计算角度顺序
            var xulie = new Dictionary<int, double>();
            for (int i = 0; i < ((ISegmentCollection)pl).SegmentCount; i++)
            {
                double value;
                if (i == 0)
                {
                    value = ((ILine)((ISegmentCollection)pl).Segment[i]).Angle -
                            (((ISegmentCollection)pl).get_Segment(((ISegmentCollection)pl).SegmentCount - 1) as ILine).Angle;
                }
                else
                {
                    value = (((ISegmentCollection)pl).get_Segment(i) as ILine).Angle -
                            (((ISegmentCollection)pl).get_Segment(i - 1) as ILine).Angle;
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
                var leng = i == zhao.Count - 1 ? ((ISegmentCollection)pl).SegmentCount : zhao[i + 1];
                var linshi = new PolylineClass();
                (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[zhao[i]].FromPoint);
                for (int j = zhao[i]; j < leng; j++)
                {
                    (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                }
                if (zhao[0] != 0 && i == zhao.Count - 1)
                {
                    for (int j = 0; j < zhao[0]; j++)
                    {
                        (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                    }
                }
                fenbian.Add(linshi);
            }
            //2.做平行线
            var ccTop = new PolylineClass();
            double maxLength = fenbian.Max(i => i.Length);
            var maxLine = fenbian.First(i => Math.Abs(i.Length - maxLength) <= 0);
            var ccDown = new PolylineClass();
            var leftLine = new PolylineClass();
            var rightLine = new PolylineClass();
            var cixu = fenbian.IndexOf(maxLine);
            if (IsLong)
            {
                ccTop = fenbian[cixu];
                if (cixu == 0)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu + 3];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 1)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 2)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 3)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu - 3];
                }
            }
            else
            {
                if (cixu == 0)
                {
                    ccTop = fenbian[cixu + 3];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 1)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 2)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu - 2];
                }
                else if (cixu == 3)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu - 3];
                    leftLine = fenbian[cixu - 2];
                }
                rightLine = fenbian[cixu];
            }
            pls.Add(ccTop);
            pls.Add(ccDown);
            ExtendTickLine(1.42, 270, ccDown, pls);
            return pls.ToArray();
        }
        private IPolyline[] MBridge5(IPolygon pl)
        {
            //1.初始化参数
            var pls = new List<IPolyline>();
            //计算角度顺序
            var xulie = new Dictionary<int, double>();
            for (int i = 0; i < ((ISegmentCollection)pl).SegmentCount; i++)
            {
                double value;
                if (i == 0)
                {
                    value = ((ILine)((ISegmentCollection)pl).Segment[i]).Angle -
                            (((ISegmentCollection)pl).get_Segment(((ISegmentCollection)pl).SegmentCount - 1) as ILine).Angle;
                }
                else
                {
                    value = (((ISegmentCollection)pl).get_Segment(i) as ILine).Angle -
                            (((ISegmentCollection)pl).get_Segment(i - 1) as ILine).Angle;
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
                var leng = i == zhao.Count - 1 ? ((ISegmentCollection)pl).SegmentCount : zhao[i + 1];
                var linshi = new PolylineClass();
                (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[zhao[i]].FromPoint);
                for (int j = zhao[i]; j < leng; j++)
                {
                    (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                }
                if (zhao[0] != 0 && i == zhao.Count - 1)
                {
                    for (int j = 0; j < zhao[0]; j++)
                    {
                        (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                    }
                }
                fenbian.Add(linshi);
            }
            //2.做平行线
            if (fenbian.Count != 4) return null;
            var ccTop = new PolylineClass();
            double maxLength = fenbian.Max(i => i.Length);
            var maxLine = fenbian.First(i => Math.Abs(i.Length - maxLength) <= 0);
            var ccDown = new PolylineClass();
            var leftLine = new PolylineClass();
            var rightLine = new PolylineClass();
            IPolyline newUp = new PolylineClass();
            IPolyline newDown = new PolylineClass();
            var cixu = fenbian.IndexOf(maxLine);
            if (IsLong)
            {
                ccTop = fenbian[cixu];
                if (cixu == 0)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu + 3];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 1)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 2)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 3)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu - 3];
                }
            }
            else
            {
                if (cixu == 0)
                {
                    ccTop = fenbian[cixu + 3];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 1)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 2)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu - 2];
                }
                else if (cixu == 3)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu - 3];
                    leftLine = fenbian[cixu - 2];
                }
                rightLine = fenbian[cixu];
            }
            if ((ccTop as IGeometry).IsEmpty || (ccDown as IGeometry).IsEmpty) { return pls.ToArray(); }
            if (IsFan) {
                var helper = new GeometricEffectDashClass();
                var d = new[] { 5.66, 2.83 };
                helper.set_Value(0, d);
                helper.set_Value(1, 1);
                helper.Reset(ccDown);
                while (true)
                {
                    var g = helper.NextGeometry();
                    if (g == null)
                        break;
                    var cg = ((IClone)g).Clone() as IGeometry;
                    Marshal.ReleaseComObject(g);
                    var geoCol = cg as IGeometryCollection;
                    for (int i = 0; i < geoCol.GeometryCount; i++)
                    {
                        var geo = ((IClone)geoCol.Geometry[i]).Clone() as IGeometry;
                        (newDown as IGeometryCollection).AddGeometry(geo);
                    }
                }
                pls.Add(newDown);
                pls.Add(ccTop);
                ExtendTickLine(1.42, 270, ccTop, pls);
                return pls.ToArray();
            } else {
                var helper = new GeometricEffectDashClass();
                var d = new[] { 5.66, 2.83 };
                helper.set_Value(0, d);
                helper.set_Value(1, 1);
                helper.Reset(ccTop);
                while (true)
                {
                    var g = helper.NextGeometry();
                    if (g == null)
                        break;
                    var cg = ((IClone)g).Clone() as IGeometry;
                    Marshal.ReleaseComObject(g);
                    var geoCol = cg as IGeometryCollection;
                    for (int i = 0; i < geoCol.GeometryCount; i++)
                    {
                        var geo = ((IClone)geoCol.Geometry[i]).Clone() as IGeometry;
                        (newUp as IGeometryCollection).AddGeometry(geo);
                    }
                }
                pls.Add(newUp);
                pls.Add(ccDown);
                ExtendTickLine(1.42, 270, ccDown, pls);
                return pls.ToArray();
            }
            
        }
        private IPolyline[] MBridge6(IPolygon pl)
        {
            //1.初始化参数
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            //计算角度顺序
            var xulie = new Dictionary<int, double>();
            for (int i = 0; i < ((ISegmentCollection)pl).SegmentCount; i++)
            {
                double value;
                if (i == 0)
                {
                    value = ((ILine)((ISegmentCollection)pl).Segment[i]).Angle -
                            (((ISegmentCollection)pl).get_Segment(((ISegmentCollection)pl).SegmentCount - 1) as ILine).Angle;
                }
                else
                {
                    value = (((ISegmentCollection)pl).get_Segment(i) as ILine).Angle -
                            (((ISegmentCollection)pl).get_Segment(i - 1) as ILine).Angle;
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
                var leng = i == zhao.Count - 1 ? ((ISegmentCollection)pl).SegmentCount : zhao[i + 1];
                var linshi = new PolylineClass();
                (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[zhao[i]].FromPoint);
                for (int j = zhao[i]; j < leng; j++)
                {
                    (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                }
                if (zhao[0] != 0 && i == zhao.Count - 1)
                {
                    for (int j = 0; j < zhao[0]; j++)
                    {
                        (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                    }
                }
                fenbian.Add(linshi);
            }
            //2.做平行线
            if (fenbian.Count != 4) return null;
            var ccTop = new PolylineClass();
            double maxLength = fenbian.Max(i => i.Length);
            var maxLine = fenbian.First(i => Math.Abs(i.Length - maxLength) <= 0);
            var ccDown = new PolylineClass();
            var leftLine = new PolylineClass();
            var rightLine = new PolylineClass();
            var cixu = fenbian.IndexOf(maxLine);
            if (IsLong)
            {
                ccTop = fenbian[cixu];
                if (cixu == 0)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu + 3];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 1)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 2)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 3)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu - 3];
                }
            }
            else
            {
                if (cixu == 0)
                {
                    ccTop = fenbian[cixu + 3];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 1)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 2)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu - 2];
                }
                else if (cixu == 3)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu - 3];
                    leftLine = fenbian[cixu - 2];
                }
                rightLine = fenbian[cixu];
            }
            ExtendTickLine1(1.42, 270, ccTop, pls);
            ExtendTickLine2(2.83, 270, ccDown, pls);
            ExtendTickLine(TickLength, 360 - TickAngle, true, ccTop, ge);
            ExtendTickLine(TickLength, TickAngle, false, ccTop, ge);
            ExtendTickLine(TickLength, 360 - TickAngle, true, ccDown, ge);
            ExtendTickLine(TickLength, TickAngle, false, ccDown, ge);
            pls.Add(ccTop);
            pls.Add(ccDown);
            return pls.ToArray();
        }
        private IPolyline[] MBridge66(IPolygon pl)
        {
            //1.初始化参数
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            //计算角度顺序
            var xulie = new Dictionary<int, double>();
            for (int i = 0; i < ((ISegmentCollection)pl).SegmentCount; i++)
            {
                double value;
                if (i == 0)
                {
                    value = ((ILine)((ISegmentCollection)pl).Segment[i]).Angle -
                            (((ISegmentCollection)pl).get_Segment(((ISegmentCollection)pl).SegmentCount - 1) as ILine).Angle;
                }
                else
                {
                    value = (((ISegmentCollection)pl).get_Segment(i) as ILine).Angle -
                            (((ISegmentCollection)pl).get_Segment(i - 1) as ILine).Angle;
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
                var leng = i == zhao.Count - 1 ? ((ISegmentCollection)pl).SegmentCount : zhao[i + 1];
                var linshi = new PolylineClass();
                (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[zhao[i]].FromPoint);
                for (int j = zhao[i]; j < leng; j++)
                {
                    (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                }
                if (zhao[0] != 0 && i == zhao.Count - 1)
                {
                    for (int j = 0; j < zhao[0]; j++)
                    {
                        (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                    }
                }
                fenbian.Add(linshi);
            }
            if (fenbian.Count != 4) return null;
            //2.做平行线
            var ccTop = new PolylineClass();
            double maxLength = fenbian.Max(i => i.Length);
            var maxLine = fenbian.First(i => Math.Abs(i.Length - maxLength) <= 0);
            var ccDown = new PolylineClass();
            var leftLine = new PolylineClass();
            var rightLine = new PolylineClass();
            var cixu = fenbian.IndexOf(maxLine);
            if (IsLong)
            {
                ccTop = fenbian[cixu];
                if (cixu == 0)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu + 3];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 1)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 2)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 3)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu - 3];
                }
            }
            else
            {
                if (cixu == 0)
                {
                    ccTop = fenbian[cixu + 3];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 1)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 2)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu - 2];
                }
                else if (cixu == 3)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu - 3];
                    leftLine = fenbian[cixu - 2];
                }
                rightLine = fenbian[cixu];
            }
            ExtendTickLine11(1.42, 270, ccTop, pls);
            ExtendTickLine22(2.83, 270, ccDown, pls);
            return pls.ToArray();
        }
        private IPolyline[] MBridge7(IPolygon pl)
        {
            //var pls = new List<IPolyline>();
            //IGeometryBridge ge = new GeometryEnvironmentClass();
            //var pll = ((ITopologicalOperator)pg).Boundary as IPolyline;
            //var segCol = pll as ISegmentCollection;
            //if (segCol == null) return null;
            //if (segCol.SegmentCount != 4)
            //{
            //    return null;
            //}
            //IPolyline curpl = new PolylineClass();
            //(curpl as ISegmentCollection).AddSegment(segCol.Segment[0]);
            //IPolyline curpl1 = new PolylineClass();
            //(curpl1 as ISegmentCollection).AddSegment(segCol.Segment[1]);
            //IPolyline curpl2 = new PolylineClass();
            //(curpl2 as ISegmentCollection).AddSegment(segCol.Segment[2]);
            //IPolyline curpl3 = new PolylineClass();
            //(curpl3 as ISegmentCollection).AddSegment(segCol.Segment[3]);
            //if (curpl.Length > curpl1.Length)
            //{
            //    pls.Add(curpl);
            //    pls.Add(curpl2);
            //    ExtendTickLine1(1.42, 270, curpl, pls);
            //    ExtendTickLine2(2.83, 270, curpl2, pls);

            //}
            //else
            //{
            //    pls.Add(curpl1);
            //    pls.Add(curpl3);
            //    ExtendTickLine1(1.42, 270, curpl1, pls);
            //    ExtendTickLine2(2.83, 270, curpl3, pls);
            //}
            //return pls.ToArray();
            //1.初始化参数
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            //计算角度顺序
            var xulie = new Dictionary<int, double>();
            for (int i = 0; i < ((ISegmentCollection)pl).SegmentCount; i++)
            {
                double value;
                if (i == 0)
                {
                    value = ((ILine)((ISegmentCollection)pl).Segment[i]).Angle -
                            (((ISegmentCollection)pl).get_Segment(((ISegmentCollection)pl).SegmentCount - 1) as ILine).Angle;
                }
                else
                {
                    value = (((ISegmentCollection)pl).get_Segment(i) as ILine).Angle -
                            (((ISegmentCollection)pl).get_Segment(i - 1) as ILine).Angle;
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
                var leng = i == zhao.Count - 1 ? ((ISegmentCollection)pl).SegmentCount : zhao[i + 1];
                var linshi = new PolylineClass();
                (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[zhao[i]].FromPoint);
                for (int j = zhao[i]; j < leng; j++)
                {
                    (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                }
                if (zhao[0] != 0 && i == zhao.Count - 1)
                {
                    for (int j = 0; j < zhao[0]; j++)
                    {
                        (linshi as IPointCollection).AddPoint(((ISegmentCollection)pl).Segment[j].ToPoint);
                    }
                }
                fenbian.Add(linshi);
            }
            //2.做平行线
            if (fenbian.Count != 4) return null;
            var ccTop = new PolylineClass();
            double maxLength = fenbian.Max(i => i.Length);
            var maxLine = fenbian.First(i => Math.Abs(i.Length - maxLength) <= 0);
            var ccDown = new PolylineClass();
            var leftLine = new PolylineClass();
            var rightLine = new PolylineClass();
            var cixu = fenbian.IndexOf(maxLine);
            if (IsLong)
            {
                ccTop = fenbian[cixu];
                if (cixu == 0)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu + 3];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 1)
                {
                    ccDown = fenbian[cixu + 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 2)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu + 1];
                }
                else if (cixu == 3)
                {
                    ccDown = fenbian[cixu - 2];
                    leftLine = fenbian[cixu - 1];
                    rightLine = fenbian[cixu - 3];
                }
            }
            else
            {
                if (cixu == 0)
                {
                    ccTop = fenbian[cixu + 3];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 1)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu + 2];
                }
                else if (cixu == 2)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu + 1];
                    leftLine = fenbian[cixu - 2];
                }
                else if (cixu == 3)
                {
                    ccTop = fenbian[cixu - 1];
                    ccDown = fenbian[cixu - 3];
                    leftLine = fenbian[cixu - 2];
                }
                rightLine = fenbian[cixu];
            }
            pls.Add(ccTop);
            pls.Add(ccDown);
            ExtendTickLine1(1.42, 270, ccTop, pls);
            ExtendTickLine2(2.83, 270, ccDown, pls);
            return pls.ToArray();
        }
        private IPolyline[] MBridge8(IPolygon pg)
        {
            
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
            var topo = pg as ITopologicalOperator;
            var pll = topo.Boundary as IPolyline;
            var segCol = pll as ISegmentCollection;
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

            //IPolyline upline = (ccTop as IClone).Clone() as IPolyline;
            IPolyline upline;
            IPolyline newUp = new PolylineClass();

            //IPolyline downline = (ccDown as IClone).Clone() as IPolyline;
            IPolyline downline;
            IPolyline newDown = new PolylineClass();
            if (curpl.Length > curpl1.Length)
            {
                upline = (curpl as IClone).Clone() as IPolyline;
                downline = (curpl2 as IClone).Clone() as IPolyline;
            }
            else
            {
                upline = (curpl1 as IClone).Clone() as IPolyline;
                downline = (curpl3 as IClone).Clone() as IPolyline;
            }
            var helper = new GeometricEffectDashClass();
            var d = new[] { 5.66, 2.83 };
            helper.set_Value(0, d);
            helper.set_Value(1, 1);
            helper.Reset(upline);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                var cg = ((IClone)g).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);
                var geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    var geo = ((IClone)geoCol.Geometry[i]).Clone() as IGeometry;
                    (newUp as IGeometryCollection).AddGeometry(geo);
                }
            }
            helper.Reset(downline);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                var cg = (g as IClone).Clone() as IGeometry;
                Marshal.ReleaseComObject(g);
                var geoCol = cg as IGeometryCollection;
                for (int i = 0; i < geoCol.GeometryCount; i++)
                {
                    var geo = (geoCol.Geometry[i] as IClone).Clone() as IGeometry;
                    (newDown as IGeometryCollection).AddGeometry(geo);
                }

            }
            pls.Add(newUp);
            pls.Add(newDown);
            //IPoint origin = new PointClass();
            //origin.X = (upline.FromPoint.X + downline.ToPoint.X)/2;
            //origin.Y=(upline.FromPoint.Y + downline.ToPoint.Y)/2;
            //ICircularArc ee = new CircularArcClass();
            //ee.FromPoint = upline.FromPoint;
            //ee.ToPoint = downline.ToPoint;
            //var startPoint = upline.FromPoint;
            //var endPoint = downline.ToPoint;
            //ee.PutCoords(origin, startPoint, endPoint, esriArcOrientation.esriArcClockwise);
            //MessageBox.Show(ee.Length.ToString());
            //MessageBox.Show((ee as IPolyline).Length.ToString());
            //pls.Add(ee as IPolyline);
            //ICircularArc ee1 = new CircularArcClass();
            //ee1.FromPoint = upline.ToPoint;
            //ee1.ToPoint = downline.FromPoint;
            //pls.Add(ee as IPolyline);
            return pls.ToArray();
        }
        private IPolyline[] MBridge9(IPolygon pg)
        {
            var pls = new List<IPolyline>();
            IGeometryBridge ge = new GeometryEnvironmentClass();
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
            if (curpl.Length > curpl1.Length)
            {
                pls.Add(curpl);
                pls.Add(curpl2);
                ExtendTickLine3(1.42, 270, curpl, pls);
                ExtendTickLine3(1.42, 270, curpl2, pls);

            }
            else
            {
                pls.Add(curpl1);
                pls.Add(curpl3);
                ExtendTickLine3(1.42, 270, curpl1, pls);
                ExtendTickLine3(1.42, 270, curpl2, pls);
            }
            return pls.ToArray();
        }
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

            if (firstP.IsEmpty) return;
            var lineSeg = new LineClass();
            if (frompt)
            {
                lineSeg.FromPoint = firstP;
                lineSeg.ToPoint = plnew.FromPoint;
                lineSeg.Rotate(plnew.FromPoint, angle / 180 * Math.PI);

                var fps = new[] { lineSeg.FromPoint };

                ge.InsertPoints(plnew as IPointCollection4, 0, ref fps);
            }
            else
            {
                lineSeg.FromPoint = plnew.ToPoint;
                lineSeg.ToPoint = firstP;
                lineSeg.Rotate(plnew.ToPoint, angle / 180 * Math.PI);
                ((IPointCollection4) plnew).AddPoint(lineSeg.ToPoint);
            }
        }
        //只生齿线
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
        //滚水坝的垂线
        private void ExtendTickLine(double length, double angle, IPolyline plnew, List<IPolyline> pls)
        {
            if (plnew.IsEmpty)
            {
                return;
            }
            double ylen = (plnew.Length - ((int)(plnew.Length / 5.66)) * 5.66) / 2;
            var toPointNumber = (int)(plnew.Length / 5.66);//15m
            IPoint firstFromP = new PointClass();
            IPoint firstToP = new PointClass();
            plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen, false, firstFromP);//第一个起点距离线的FormPoint1m
            plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, length + ylen, false, firstToP);//第一个终点距离线的FormPoint6m
            //    points.Add(firstToP);
            //      plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, plnew.Length + length, false, firstP);
            var lineSeg = new LineClass { FromPoint = firstFromP, ToPoint = firstToP };
            lineSeg.Rotate(firstFromP, (360 - angle) / 180 * Math.PI);
            IPolyline pline = new PolylineClass();
            pline.FromPoint = lineSeg.FromPoint;
            pline.ToPoint = lineSeg.ToPoint;
            pls.Add(pline);
            for (int i = 1; i < toPointNumber+1; i++)
            {
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 5.66 * i, false, fromPoint);//10m
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 5.66 * i + length, false, toPoint);//15m
                var lineSeg1 = new LineClass { FromPoint = fromPoint, ToPoint = toPoint };
                lineSeg1.Rotate(fromPoint, (360 - angle) / 180 * Math.PI);
                IPolyline plinenew = new PolylineClass();
                plinenew.FromPoint = lineSeg1.FromPoint;
                plinenew.ToPoint = lineSeg1.ToPoint;
                pls.Add(plinenew);
            }
        }
        //拦水坝的短垂线
        private void ExtendTickLine1(double length, double angle, IPolyline plnew, List<IPolyline> pls)
        {
            if (plnew.IsEmpty)
            {
                return;
            }
            double ylen = (plnew.Length - ((int)(plnew.Length / 8.49)) * 8.49) / 2;
            var toPointNumber = (int)(plnew.Length / 8.49);//15m
            IPoint firstFromP = new PointClass();
            IPoint firstToP = new PointClass();
            plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen, false, firstFromP);//第一个起点距离线的FormPoint1m
            plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, length + ylen, false, firstToP);//第一个终点距离线的FormPoint6m
            //    points.Add(firstToP);
            //      plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, plnew.Length + length, false, firstP);
            var lineSeg = new LineClass { FromPoint = firstFromP, ToPoint = firstToP };
            lineSeg.Rotate(firstFromP, (360 - angle) / 180 * Math.PI);
            IPolyline pline = new PolylineClass();
            pline.FromPoint = lineSeg.FromPoint;
            pline.ToPoint = lineSeg.ToPoint;
            pls.Add(pline);
            for (int i = 1; i < toPointNumber + 1; i++)
            {
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 8.49 * i, false, fromPoint);//10m
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 8.49 * i + length, false, toPoint);//15m
                var lineSeg1 = new LineClass { FromPoint = fromPoint, ToPoint = toPoint };
                lineSeg1.Rotate(fromPoint, (360 - angle) / 180 * Math.PI);
                IPolyline plinenew = new PolylineClass();
                plinenew.FromPoint = lineSeg1.FromPoint;
                plinenew.ToPoint = lineSeg1.ToPoint;
                pls.Add(plinenew);
            }
        }
        //拦水坝的长垂线
        private void ExtendTickLine2(double length, double angle, IPolyline plnew, List<IPolyline> pls)
        {
            if (plnew.IsEmpty)
            {
                return;
            }
            double ylen = (plnew.Length - ((int)(plnew.Length / 8.49)) * 8.49) / 2;
            var toPointNumber = (int)(plnew.Length / 8.49);//15m
            IPoint firstFromP = new PointClass();
            IPoint firstToP = new PointClass();
            plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen, false, firstFromP);//第一个起点距离线的FormPoint1m
            plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, length + ylen, false, firstToP);//第一个终点距离线的FormPoint6m
            //    points.Add(firstToP);
            //      plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, plnew.Length + length, false, firstP);
            var lineSeg = new LineClass { FromPoint = firstFromP, ToPoint = firstToP };
            lineSeg.Rotate(firstFromP, (360 - angle) / 180 * Math.PI);
            IPolyline pline = new PolylineClass();
            pline.FromPoint = lineSeg.FromPoint;
            pline.ToPoint = lineSeg.ToPoint;
            pls.Add(pline);
            for (int i = 1; i < toPointNumber + 1; i++)
            {
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 8.49 * i, false, fromPoint);//10m
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 8.49 * i + length, false, toPoint);//15m
                var lineSeg1 = new LineClass { FromPoint = fromPoint, ToPoint = toPoint };
                lineSeg1.Rotate(fromPoint, (360 - angle) / 180 * Math.PI);
                IPolyline plinenew = new PolylineClass();
                plinenew.FromPoint = lineSeg1.FromPoint;
                plinenew.ToPoint = lineSeg1.ToPoint;
                pls.Add(plinenew);
            }
        }
        //拦水坝的加固点
        private void ExtendTickLine11(double length, double angle, IPolyline plnew, List<IPolyline> pls)
        {
            if (plnew.IsEmpty)
            {
                return;
            }
            double ylen = (plnew.Length - ((int)(plnew.Length / 8.49)) * 8.49) / 2;
            var toPointNumber = (int)(plnew.Length / 8.49);//15m
            for (int i = 1; i < toPointNumber+1; i++)
            {
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 8.49 * i - 4.245, false, fromPoint);//10m
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 8.49 * i - 4.245 + 0.7, false, toPoint);//15m
                var lineSeg1 = new LineClass { FromPoint = fromPoint, ToPoint = toPoint };
                lineSeg1.Rotate(fromPoint, (360 - angle) / 180 * Math.PI);
                IPolyline plinenew = new PolylineClass();
                plinenew.FromPoint = lineSeg1.FromPoint;
                plinenew.ToPoint = lineSeg1.ToPoint;
                pls.Add(plinenew);
            }
        }
        //拦水坝的加固点
        private void ExtendTickLine22(double length, double angle, IPolyline plnew, List<IPolyline> pls)
        {
            if (plnew.IsEmpty)
            {
                return;
            }
            double ylen = (plnew.Length - ((int)(plnew.Length / 8.49)) * 8.49) / 2;
            var toPointNumber = (int)(plnew.Length / 8.49);//15m
            for (int i = 1; i < toPointNumber+1; i++)
            {
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 8.49 * i - 4.245, false, fromPoint);//10m
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 8.49 * i - 4.245 + 0.7, false, toPoint);//15m
                var lineSeg1 = new LineClass { FromPoint = fromPoint, ToPoint = toPoint };
                lineSeg1.Rotate(fromPoint, (360 - angle) / 180 * Math.PI);
                IPolyline plinenew = new PolylineClass();
                plinenew.FromPoint = lineSeg1.FromPoint;
                plinenew.ToPoint = lineSeg1.ToPoint;
                pls.Add(plinenew);
            }
        }
        //干堤（去掉起始的齿）
        private void ExtendTickLine3(double length, double angle, IPolyline plnew, List<IPolyline> pls)
        {
            if (plnew.IsEmpty)
            {
                return;
            }
            double ylen = (plnew.Length - ((int)(plnew.Length / 5.66)) * 5.66) / 2;
            var toPointNumber = (int)(plnew.Length / 5.66);//15m
            IPoint firstFromP = new PointClass();
            IPoint firstToP = new PointClass();
            for (int i = 0; i < toPointNumber+1; i++)
            {
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 5.66 * i, false, fromPoint);//10m
                plnew.QueryPoint(esriSegmentExtension.esriExtendTangents, ylen + 5.66 * i + length, false, toPoint);//15m
                var lineSeg1 = new LineClass { FromPoint = fromPoint, ToPoint = toPoint };
                lineSeg1.Rotate(fromPoint, (360 - angle) / 180 * Math.PI);
                IPolyline plinenew = new PolylineClass();
                plinenew.FromPoint = lineSeg1.FromPoint;
                plinenew.ToPoint = lineSeg1.ToPoint;
                pls.Add(plinenew);
            }
        }
    }
}

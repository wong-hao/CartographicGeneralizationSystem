using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using stdole;
using SMGI.Common;
using SMGI.Plugin.ThematicChart.ThematicChart;
namespace SMGI.Plugin.ThematicChart
{
    //柱状图处理辅助类
    [DataContract]
    public class ColumnJson
    {
        [DataMember(Order = 0, IsRequired = true)]
        public List<ThematicColor> Colors;
        [DataMember(Order = 1)]
        public double Size;
        [DataMember(Order = 2)]
        public double YInterval;
        [DataMember(Order = 3)]
        public bool XYShow;
        [DataMember(Order = 4)]
        public bool LengendShow;
        [DataMember(Order = 5)]
        public string DataSource;
        [DataMember(Order = 6)]
        public string ThematicType;
        [DataMember(Order = 7)]
        public string LayerName;
        [DataMember(Order = 8)]
        public string Title;
        [DataMember(Order = 9)]
        public bool GeoNum;
        [DataMember(Order = 10)]
        public bool GeoRel;
        [DataMember(Order = 11)]
        public double TxtAngel;
        [DataMember(Order = 12)]
        public bool IsTransparent;
    }
   
    
    //根据属性绘制柱状图
    public class ColumnHelper
    {
        ColumnJson columnInfo = null;
        IActiveView pAc = null;
        GraphicsHelper gh = null;
        private ILayer pRepLayer = null;
        private ILayer annoly = null;
        IFeatureClass annoFcl = null;
        List<int> colorlist = new List<int>();
        List<ICmykColor> colors = new List<ICmykColor>();
        private double mapScale = 1000;
        double markerSize = 20;
        private bool geoNum = true;
        private bool xyAxis = true;
        private bool geoRel = false;
        private double angle = 0;
        private double ykedu = 0;
        private string chartTitle = "";
        IPoint basePointInit = new PointClass() { X = 0, Y = 0 };
        IPoint basePointReal = null;
        List<string> Ykedu = new List<string>();
        private List<IElement> eles = new List<IElement>();
        Dictionary<IPoint, string> lbpt = new Dictionary<IPoint, string>();//数量标注
        Dictionary<IPoint, string> lgpt = new Dictionary<IPoint, string>();//图例标注
        //绘制3D柱状图
        public List<IElement> Draw3DColumnChart(ColumnJson columnInfo,IPoint _centerpoint)
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pRepLayer = lyrs.First();

            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LANNO");
            })).ToArray();
            var annoly = lyr.First();
            annoFcl = (annoly as IFeatureLayer).FeatureClass;

            eles.Clear();
            pAc = GApplication.Application.ActiveView;
            gh = new GraphicsHelper(pAc);
            IPoint basePoint = basePointInit;
            basePointReal = _centerpoint;
            //columnInfo相关信息
            markerSize = columnInfo.Size;
            xyAxis = columnInfo.XYShow;
            geoNum = columnInfo.GeoNum;
            ykedu = columnInfo.YInterval;
            angle = columnInfo.TxtAngel;
            geoRel = columnInfo.GeoRel;
            foreach (var c in columnInfo.Colors)
            {
                var cmyk = new CmykColorClass();
                cmyk.Cyan = c.C;
                cmyk.Yellow = c.Y;
                cmyk.Magenta = c.M;
                cmyk.Black = c.K;
                colors.Add(cmyk);
            }
            colorlist.Clear();
            lbpt.Clear();
            //#region
            //菱形基本高度为 10m
            double diaHeight = 1e-2 * mapScale;
            //柱基本宽度
            double baseWidth = 4e-2 * mapScale;
            //每个柱子间距
            double columnstep = 0;
            //每个组间距
            double cp = 4e-2 * mapScale;
            //数据
            double max = 0; int columns = 0;
            Dictionary<string, Dictionary<string, double>> datas = JsonHelper.CHDataSource(columnInfo.DataSource);
            string[] types = CreateStaticDatas(datas, ref max);
            columns = datas.Count * datas.First().Value.Count;
            chartTitle = columnInfo.Title;
             //组数
            int groups = datas.Count;

             ////绘制xy轴
            double kedu = ykedu;
            max = ykedu * 10;
            if (xyAxis)
            {
                DrawXYaxi(basePoint, kedu, columns, groups);
            }
            else
            {
                DrawXYaxiNull(basePoint, kedu, columns, groups);
            }
            //每根柱子颜色
            //绘制柱子
            int step = 0;
            //柱高度 
            double reheight = 2 * baseWidth;//柱高度
            double dis = baseWidth + columnstep;
            int i = 0;
            double test = 0;
            foreach (var kv1 in datas)
            {
                int color = 0;
                var dicvals = kv1.Value;
                foreach (var kv in dicvals)
                {
                    #region
                    reheight = 9.5 * baseWidth * kv.Value / max;
                    IPoint pbasePoint = null;
                    pbasePoint = new PointClass() { X = basePoint.X + step * dis, Y = basePoint.Y };
                    IColor pcolor = colors[color];
                    int black = (pcolor as ICmykColor).Black;
                    //正面
                    IPolygon rectange = gh.CreateRectangle(pbasePoint, baseWidth, -reheight);
                    var ele = DrawColunnPolygon(rectange, pcolor);
                    eles.Add(ele);
                    IPoint leftdown = new PointClass() { X = pbasePoint.X, Y = pbasePoint.Y + reheight };
                    //顶
                    IColor pcolor1 = (pcolor as IClone).Clone() as IColor;
                    (pcolor1 as ICmykColor).Black = black + 15;
                    IPolygon diamond = gh.ConstructTopDiamond(leftdown, baseWidth, diaHeight);
                    ele = DrawColunnPolygon(diamond, pcolor1);
                    eles.Add(ele);
                    IPoint temp = new PointClass() { X = pbasePoint.X + baseWidth, Y = pbasePoint.Y };
                    //侧面
                    IColor pcolor2 = (pcolor as IClone).Clone() as IColor;
                    (pcolor2 as ICmykColor).Black = black + 25;
                    IPolygon diamond1 = gh.ConstructSideDiamond(temp, diaHeight, reheight);
                    ele = DrawColunnPolygon(diamond1, pcolor2);
                    eles.Add(ele);
                    //绘制标注
                    if (geoNum)
                    {
                        IPoint labelpoint = new PointClass();
                        double dx = diaHeight / Math.Tan(Math.PI / 4) + baseWidth;
                        labelpoint.PutCoords(leftdown.X + dx / 2, leftdown.Y + mapScale * 1.3e-2);

                        // labelpoint.PutCoords(leftdown.X + baseWidth / 2, leftdown.Y + mapScale * 1.3e-2);
                        //ele = DrawTxt(labelpoint, kv.Value.ToString(), 25);
                        //eles.Add(ele);
                        //总长度
                        double total = 4e-1 * mapScale;
                        lbpt.Add(new PointClass { X = (labelpoint.X - basePoint.X) / total, Y = (labelpoint.Y - basePoint.Y) / total }, kv.Value.ToString());

                    }
                    step++;
                    color++;
                    #endregion
                }
                step++;
                i++;
            }
            //绘制图例
            if (columnInfo.LengendShow)
            {
                DrawLengend(basePoint, types, columns, groups);
            }
            else
            {
                DrawLengendNULL(basePoint, types, columns, groups);
            }
            int obj = 0;
            //尺寸
            columnInfo.ThematicType = "3D柱状图";
            string jsonText = JsonHelper.GetJsonText(columnInfo);
            var repmarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, basePointReal, jsonText, out obj, markerSize);
            CreateAnnotion(repmarker, basePointReal, markerSize, datas, obj);
            pAc.Refresh();
            return eles;   
        }
        #region
        /// <summary>
        /// 绘制图例
        /// </summary>
        /// <param name="basepoint"></param>
        private void DrawLengend(IPoint _basepoint, string[] subtypes, double columns, double groups)
        {
            lgpt.Clear();
            IPoint basepoint = (_basepoint as IClone).Clone() as IPoint;
            //每组间距宽度
            double groupwith = 4e-2 * mapScale;
            //基本宽度
            double basewidth = 4e-2 * mapScale;
            //X轴总长度
            double xdistance = (columns + 1) * basewidth + (groups - 1) * groupwith;
            double cx1 = (3e-2 * mapScale) * Math.Tan(Math.PI / 4);
            double cy1 = (3e-2 * mapScale) * Math.Tan(Math.PI / 4);
            basepoint.X -= cx1;
            basepoint.Y -= cy1;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取图列的高度
            double heightunit = 3.97427;//1号字体1万的高度
            double fontsize = 50;
            double lgheight = heightunit * fontsize * mapScale / 10000;
            int ct = subtypes.Length;
            //全部图例+文字长度
            double length = lgheight * 2 * ct;//图例长度
            double lginterval = mapScale * 10.0e-3;//图例间间隔
            double txtinterval = mapScale * 5.0e-3;//文字图例间隔
            length += lginterval * (ct - 1);//间隔
            foreach (string str in subtypes)
            {
                length += gh.GetStrWidth(str, mapScale, fontsize) + txtinterval;// ;//加文字宽度
            }
            //X轴长度
            double Xdis = xdistance;


            double dx = Xdis / 2 - length / 2;//平移距离
            double stepX = 0;
            double total = 4e-1 * mapScale;
            for (int i = 0; i < subtypes.Length; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - 2.5e-2 * mapScale;
                upleft.PutCoords(stepX + basepoint.X + dx, y);
                //tulie
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                var ele = gh.DrawPolygon(prect, colors[i], 0);
                eles.Add(ele);

                stepX += lgheight * 2;
                //wenzi
                double strwidth = gh.GetStrWidth(subtypes[i], mapScale, fontsize);
                stepX += txtinterval;//文字间隔
                IPoint txtpoint = new PointClass() { X = txtinterval + upleft.X + strwidth / 2 + lgheight * 2, Y = upleft.Y - 0.8 * lgheight };
                double sx = (txtpoint.X - _basepoint.X) / total;
                double sy = (txtpoint.Y - _basepoint.Y) / total;
                IPoint pt = new PointClass { X = sx, Y = sy };
                lbpt.Add(pt, subtypes[i]);
                //ele = gh.DrawTxt(txtpoint, subtypes[i], fontsize);
                //eles.Add(ele);
                stepX += strwidth + lginterval;//文字距离+图例间隔

            }
        }
        private void DrawLengendNULL(IPoint _basepoint, string[] subtypes, double columns, double groups)
        {

            IPoint basepoint = (_basepoint as IClone).Clone() as IPoint;
            //每组间距宽度
            double groupwith = 4e-2 * mapScale;
            //基本宽度
            double basewidth = 4e-2 * mapScale;
            //X轴总长度
            double xdistance = (columns + 1) * basewidth + (groups - 1) * groupwith;
            double cx1 = (3e-2 * mapScale) * Math.Tan(Math.PI / 4);
            double cy1 = (3e-2 * mapScale) * Math.Tan(Math.PI / 4);
            basepoint.X -= cx1;
            basepoint.Y -= cy1;
            IActiveView pAc = GApplication.Application.ActiveView;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取图列的高度
            double heightunit = 3.97427;//1号字体1万的高度
            double fontsize = 50;
            double lgheight = heightunit * fontsize * mapScale / 10000;
            int ct = subtypes.Length;
            //全部图例+文字长度
            double length = lgheight * 2 * ct;//图例长度
            double lginterval = mapScale * 10.0e-3;//图例间间隔
            double txtinterval = mapScale * 5.0e-3;//文字图例间隔
            length += lginterval * (ct - 1);//间隔
            foreach (string str in subtypes)
            {
                length += gh.GetStrWidth(str, mapScale, fontsize) + txtinterval;// ;//加文字宽度
            }
            //X轴长度
            double Xdis = xdistance;


            double dx = Xdis / 2 - length / 2;//平移距离
            double stepX = 0;
            double total = 4e-1 * mapScale;
            for (int i = 0; i < subtypes.Length; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - 2.5e-2 * mapScale;
                upleft.PutCoords(stepX + basepoint.X + dx, y);
                //tulie
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                var ele = gh.DrawPolygonBg(prect, colors[i], 0);
                eles.Add(ele);

                stepX += lgheight * 2;
                //wenzi
                double strwidth = gh.GetStrWidth(subtypes[i], mapScale, fontsize);
                stepX += txtinterval;//文字间隔

                stepX += strwidth + lginterval;//文字距离+图例间隔

            }
        }
      
        private IElement DrawColunnPolygon(IGeometry geo, IColor pcolor)
        {
           
            IElement pEl = null;
            try
            {
                IFillShapeElement polygonElement = new PolygonElementClass();

                ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
                ISimpleLineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Color = pcolor;
                linesym.Width = 0.05;
                smsymbol.Outline = linesym;
                smsymbol.Color = pcolor;
                polygonElement.Symbol = smsymbol;
                pEl = polygonElement as IElement;
                pEl.Geometry = geo as IGeometry;
                eles.Add(pEl);
                return pEl;
            }
            catch
            {
                return pEl;
            }
        }
        //绘制xy轴
        private void DrawXYaxi(IPoint orgin, double val, double columns, double groups, bool draw = false)
        {
            Ykedu.Clear();
            //基本宽度
            double basewidth = 4e-2 * mapScale;
            //每组间距宽度
            double groupwith = 4e-2 * mapScale;
            //X轴总长度
            double xdistance = (columns + 1) * basewidth + (groups - 1) * groupwith;
            //1 横线
            double cx = orgin.X - 5e-3 * mapScale;
            double cy = orgin.Y + 10e-3 * mapScale;
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(cx, cy + (4e-2 * mapScale * i));
                IPoint pt = new PointClass();
                pt.PutCoords(cx + xdistance, cy + (4e-2 * mapScale * i));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLine(pc as IPolyline);
                eles.Add(ele);
            }
            //2斜
            double cx1 = (3e-2 * mapScale) * Math.Tan(Math.PI / 4);
            double cy1 = (3e-2 * mapScale) * Math.Tan(Math.PI / 4);
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(cx, cy + (4e-2 * mapScale * i));
                IPoint pt = new PointClass();
                pt.PutCoords(cx - cx1, cy - cy1 + (4e-2 * mapScale * i));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLine(pc as IPolyline);
                eles.Add(ele);

            }
            //Y刻度
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(cx - cx1 - (5e-3 * mapScale), cy - cy1 + (4e-2 * mapScale * i));
                IPoint pt = new PointClass();
                pt.PutCoords(cx - cx1, cy - cy1 + (4e-2 * mapScale * i));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLine(pc as IPolyline);
                eles.Add(ele);
                double txtval = (i * val);

                string txt = (txtval).ToString();
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(pf.X - txt.Length * 5 * 1e-3 * mapScale, pf.Y);
                Ykedu.Add(txt);
             
            }
            //x刻度
            for (int i = 0; i < groups; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(cx - cx1 + xdistance * (i + 1) / groups, cy - cy1);
                IPoint pt = new PointClass();
                pt.PutCoords(cx - cx1 + xdistance * (i + 1) / groups, cy - cy1 - (5e-3 * mapScale));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLine(pc as IPolyline); eles.Add(ele);
            }
            //Y
            IPointCollection pline = new PolylineClass();
            IPoint f = new PointClass();
            f.PutCoords(cx - cx1, cy - cy1);
            IPoint t = new PointClass();
            t.PutCoords(cx - cx1, cy - cy1 + 10 * basewidth);
            pline.AddPoint(f);
            pline.AddPoint(t);
            var ele1 = DrawLine(pline as IPolyline); eles.Add(ele1);
            //X
            pline = new PolylineClass();
            f = new PointClass();
            f.PutCoords(cx - cx1, cy - cy1);
            t = new PointClass();
            t.PutCoords(cx - cx1 + xdistance, cy - cy1);
            pline.AddPoint(f);
            pline.AddPoint(t);
            ele1 = DrawLine(pline as IPolyline); eles.Add(ele1);
            //x斜
            pline = new PolylineClass();
            f = new PointClass();
            f.PutCoords(cx + xdistance, cy);
            t = new PointClass();
            t.PutCoords(cx - cx1 + xdistance, cy - cy1);
            pline.AddPoint(f);
            pline.AddPoint(t);
            ele1 = DrawLine(pline as IPolyline); eles.Add(ele1);
        }

        private IElement DrawLine(IPolyline pline,double width=2)
        {
           
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = width;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = 122;
                rgb.Blue = 122;
                rgb.Green = 122;
                linesym.Color = rgb;
                polygonElement.Symbol = linesym;
                pEl = polygonElement as IElement;
                pEl.Geometry = pline as IGeometry;

                eles.Add(pEl);
               

            }
            catch
            {

            }
            return pEl;
        }
        //绘制透明xy轴
        private void DrawXYaxiNull(IPoint orgin, double val, double columns, double groups, bool draw = false)
        {
            Ykedu.Clear();
            //基本宽度
            double basewidth = 4e-2 * mapScale;
            //每组间距宽度
            double groupwith = 4e-2 * mapScale;
            //X轴总长度
            double xdistance = (columns + 1) * basewidth + (groups - 1) * groupwith;
            //1 横线
            double cx = orgin.X - 5e-3 * mapScale;
            double cy = orgin.Y + 10e-3 * mapScale;
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(cx, cy + (4e-2 * mapScale * i));
                IPoint pt = new PointClass();
                pt.PutCoords(cx + xdistance, cy + (4e-2 * mapScale * i));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLineNull(pc as IPolyline);
                eles.Add(ele);
            }
            //2斜
            double cx1 = (3e-2 * mapScale) * Math.Tan(Math.PI / 4);
            double cy1 = (3e-2 * mapScale) * Math.Tan(Math.PI / 4);
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(cx, cy + (4e-2 * mapScale * i));
                IPoint pt = new PointClass();
                pt.PutCoords(cx - cx1, cy - cy1 + (4e-2 * mapScale * i));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLineNull(pc as IPolyline);
                eles.Add(ele);

            }
            //Y刻度
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(cx - cx1 - (5e-3 * mapScale), cy - cy1 + (4e-2 * mapScale * i));
                IPoint pt = new PointClass();
                pt.PutCoords(cx - cx1, cy - cy1 + (4e-2 * mapScale * i));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLineNull(pc as IPolyline);
                eles.Add(ele);
                double txtval = (i * val);

                string txt = (txtval).ToString();
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(pf.X - txt.Length * 5 * 1e-3 * mapScale, pf.Y);
                Ykedu.Add(txt);
                //ele= gh.DrawTxt(txtpoint, txt, 55);
                // eles.Add(ele);
            }
            //x刻度
            for (int i = 0; i < groups; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(cx - cx1 + xdistance * (i + 1) / groups, cy - cy1);
                IPoint pt = new PointClass();
                pt.PutCoords(cx - cx1 + xdistance * (i + 1) / groups, cy - cy1 - (5e-3 * mapScale));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLineNull(pc as IPolyline); eles.Add(ele);
            }
            //Y
            IPointCollection pline = new PolylineClass();
            IPoint f = new PointClass();
            f.PutCoords(cx - cx1, cy - cy1);
            IPoint t = new PointClass();
            t.PutCoords(cx - cx1, cy - cy1 + 10 * basewidth);
            pline.AddPoint(f);
            pline.AddPoint(t);
            var ele1 = DrawLineNull(pline as IPolyline); eles.Add(ele1);
            //X
            pline = new PolylineClass();
            f = new PointClass();
            f.PutCoords(cx - cx1, cy - cy1);
            t = new PointClass();
            t.PutCoords(cx - cx1 + xdistance, cy - cy1);
            pline.AddPoint(f);
            pline.AddPoint(t);
            ele1 = DrawLineNull(pline as IPolyline); eles.Add(ele1);
            //x斜
            pline = new PolylineClass();
            f = new PointClass();
            f.PutCoords(cx + xdistance, cy);
            t = new PointClass();
            t.PutCoords(cx - cx1 + xdistance, cy - cy1);
            pline.AddPoint(f);
            pline.AddPoint(t);
            ele1 = DrawLineNull(pline as IPolyline); eles.Add(ele1);
        }

     
        private IElement DrawLineNull(IPolyline pline)
        {
            
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ISimpleLineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 2;
                linesym.Style = esriSimpleLineStyle.esriSLSNull;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = 122;
                rgb.Blue = 122;
                rgb.Green = 122;
                linesym.Color = rgb;
                polygonElement.Symbol = linesym;
                pEl = polygonElement as IElement;
                pEl.Geometry = pline as IGeometry;

               
               

            }
            catch
            {

            }
            return pEl;
        }
        private string[] CreateStaticDatas(Dictionary<string, Dictionary<string, double>> datas, ref double max)
        {

            List<string> types = new List<string>();
            var data = datas.First().Value;
            foreach (var k in data)
            {
                types.Add(k.Key);
            }
            foreach (var kv in datas)
            {
                var vals = kv.Value.OrderByDescending(r => r.Value);

                max = vals.First().Value > max ? vals.First().Value : max;

            }
            return types.ToArray();
        }
        private string[] CreateStaticDatasCl(Dictionary<string, Dictionary<string, double>> datas, ref double max)
        {
            List<string> types = new List<string>();
            var data = datas.First().Value;
            foreach (var k in data)
            {
                types.Add(k.Key);//类别名称
            }
            foreach (var kv in datas)
            {
                var vals = kv.Value;
                double a = 0;
                foreach (var c in vals)
                {
                    a = a + c.Value;
                }
                if (a >= max)
                {
                    max = a;
                }
            }
            return types.ToArray();
        }
        #endregion

        //绘制2D柱状图
        public List<IElement> Draw2DColumnChart(ColumnJson columnInfo, IPoint _centerpoint)
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pRepLayer = lyrs.First();

            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LANNO");
            })).ToArray();
            var annoly = lyr.First();
            annoFcl = (annoly as IFeatureLayer).FeatureClass;
            eles.Clear();
            pAc = GApplication.Application.ActiveView;
            gh = new GraphicsHelper(pAc);
            IPoint basePoint = basePointInit;
            basePointReal = _centerpoint;
            //columnInfo相关信息
            markerSize = columnInfo.Size;
            xyAxis = columnInfo.XYShow;
            geoNum = columnInfo.GeoNum;
            ykedu = columnInfo.YInterval;
            angle = columnInfo.TxtAngel;
            geoRel = columnInfo.GeoRel;
            chartTitle = columnInfo.Title;
            foreach (var c in columnInfo.Colors)
            {
                var cmyk = new CmykColorClass();
                cmyk.Cyan = c.C;
                cmyk.Yellow = c.Y;
                cmyk.Magenta = c.M;
                cmyk.Black = c.K;
                colors.Add(cmyk);
            }
            colorlist.Clear();
            lbpt.Clear();
            double max = 0;
            var datas = JsonHelper.CHDataSource(columnInfo.DataSource);
            string[] types = CreateStaticDatas(datas, ref max);
            //组数
            int groups = datas.Count;
            //y刻度
            double kedu = ykedu;
            max = ykedu * 10;
            int columns = datas.Count * datas.First().Value.Count;
            //绘制XY轴
            if (columnInfo.XYShow)
            {
                DrawXYaxis(basePoint, kedu, columns, datas.Count);
            }
            else
            {
                DrawXYaxis2DNull(basePoint, kedu, columns, datas.Count);
            }
            DrawColumns(basePoint, datas, max, colors);
            //绘制图例
            if (columnInfo.LengendShow)
            {
                DrawLengend2D(basePoint, types, columns, datas.Count);
            }
            else
            {
                DrawLengend2DNull(basePoint, types, columns, datas.Count);
            }

            int obj = 0;
            //尺寸
            columnInfo.ThematicType = "2D柱状图";
            string jsonText = JsonHelper.GetJsonText(columnInfo);
            var repmarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, basePointReal, jsonText, out obj, markerSize);
            CreateAnnotion2D(repmarker, basePointReal, markerSize, datas, obj);

            return eles;
            pAc.Refresh();
        }
        #region 绘制2D柱状图
        private void DrawXYaxis(IPoint orgin, double val, double columns, double groups)
        {
            Ykedu.Clear();
            //每一个柱子基本宽度
            double basewidth = 5e-3 * mapScale;
            double total = 6 * 1e-2 * mapScale;
            double xdistance = (columns + 1) * basewidth + (groups - 1) * basewidth;//X轴长度
            xdistance += basewidth;
            //1 横线
            double cx = orgin.X - 2e-3 * mapScale;
            double cy = orgin.Y;
            double step = 0.6 * 1e-2 * mapScale;//每个刻度间距
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass() { X = cx, Y = orgin.Y + i * step };
                IPoint pt = new PointClass() { X = cx + xdistance, Y = orgin.Y + i * step };
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLine(pc as IPolyline, 0.2);
                eles.Add(ele);
            }
            //y轴
            IPoint xpoint = new PointClass() { X = orgin.X, Y = orgin.Y };
            IPoint xpoint1 = new PointClass() { X = orgin.X, Y = orgin.Y + 10 * step };
            IGeometry pline = ContructPolyLine(xpoint1, xpoint);
            var eley = DrawLine(pline as IPolyline, 0.2);
            eles.Add(eley);
            //Y刻度
            for (int i = 0; i < 11; i++)
            {

                int txtval = (int)(i * val);
                string txt = (txtval).ToString();
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(cx - txt.Length * 1e-3 * mapScale, orgin.Y + i * step);
                Ykedu.Add(txt);

            }
            //x刻度
            double d = columns / groups * basewidth;
            for (int i = 0; i < groups; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(orgin.X + (i) * (d + basewidth) + 1.5 * basewidth + d, cy);
                IPoint pt = new PointClass();
                pt.PutCoords(orgin.X + (i) * (d + basewidth) + 1.5 * basewidth + d, cy - (2e-3 * mapScale));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLine(pc as IPolyline, 0.2);
                eles.Add(ele);
            }


        }
        private void DrawXYaxis2D(IPoint orgin, double val, double columns, double groups)
        {
            Ykedu.Clear();
           
            //每一个柱子基本宽度
            double basewidth = 5e-3 * mapScale;
            double total = 6 * 1e-2 * mapScale;
            double xdistance = (columns + 1) * basewidth + (groups - 1) * basewidth;//X轴长度
            xdistance += basewidth;
            //1 横线
            double cx = orgin.X - 2e-3 * mapScale;
            double cy = orgin.Y;
            double step = 0.6 * 1e-2 * mapScale;//每个刻度间距
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass() { X = cx, Y = orgin.Y + i * step };
                IPoint pt = new PointClass() { X = cx + xdistance, Y = orgin.Y + i * step };
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLine(pc as IPolyline,0.2);
                eles.Add(ele);
            }
            //y轴
            IPoint xpoint = new PointClass() { X = orgin.X, Y = orgin.Y };
            IPoint xpoint1 = new PointClass() { X = orgin.X, Y = orgin.Y + 10 * step };
            IGeometry pline = ContructPolyLine(xpoint1, xpoint);
            var eley = DrawLine(pline as IPolyline, 0.2);
            eles.Add(eley);
            //Y刻度
            for (int i = 0; i < 11; i++)
            {

                int txtval = (int)(i * val);
                string txt = (txtval).ToString();
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(cx - txt.Length * 1e-3 * mapScale, orgin.Y + i * step);
                
            }
            //x刻度
            double d = columns / groups * basewidth;
            for (int i = 0; i < groups; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(orgin.X + (i) * (d + basewidth) + 1.5 * basewidth + d, cy);
                IPoint pt = new PointClass();
                pt.PutCoords(orgin.X + (i) * (d + basewidth) + 1.5 * basewidth + d, cy - (2e-3 * mapScale));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLine(pc as IPolyline, 0.2);
                eles.Add(ele);
            }


        }
        private void DrawXYaxis2DNull(IPoint orgin, double val, double columns, double groups)
        {
            Ykedu.Clear();

            //每一个柱子基本宽度
            double basewidth = 5e-3 * mapScale;
            double total = 6 * 1e-2 * mapScale;
            double xdistance = (columns + 1) * basewidth + (groups - 1) * basewidth;//X轴长度
            xdistance += basewidth;
            //1 横线
            double cx = orgin.X - 2e-3 * mapScale;
            double cy = orgin.Y;
            double step = 0.6 * 1e-2 * mapScale;//每个刻度间距
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass() { X = cx, Y = orgin.Y + i * step };
                IPoint pt = new PointClass() { X = cx + xdistance, Y = orgin.Y + i * step };
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLineNull(pc as IPolyline);
                eles.Add(ele);
            }
            //y轴
            IPoint xpoint = new PointClass() { X = orgin.X, Y = orgin.Y };
            IPoint xpoint1 = new PointClass() { X = orgin.X, Y = orgin.Y + 10 * step };
            IGeometry pline = ContructPolyLine(xpoint1, xpoint);
            var eley = DrawLineNull(pline as IPolyline);
            eles.Add(eley);
            //Y刻度
            for (int i = 0; i < 11; i++)
            {

                int txtval = (int)(i * val);
                string txt = (txtval).ToString();
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(cx - txt.Length * 1e-3 * mapScale, orgin.Y + i * step);

            }
            //x刻度
            double d = columns / groups * basewidth;
            for (int i = 0; i < groups; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass();
                pf.PutCoords(orgin.X + (i) * (d + basewidth) + 1.5 * basewidth + d, cy);
                IPoint pt = new PointClass();
                pt.PutCoords(orgin.X + (i) * (d + basewidth) + 1.5 * basewidth + d, cy - (2e-3 * mapScale));
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLineNull(pc as IPolyline);
                eles.Add(ele);
            }


        }
        private void DrawColumns2D(IPoint pBasePoint, Dictionary<string, Dictionary<string, double>> datas, double max, List<ICmykColor> cmykColors)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double dis = 5 * 1e-3 * mapScale;//一个数据长度
            int groups = datas.Count;
            double total = 6 * 1e-2 * mapScale;
            int num = 0;
            double xdis = pBasePoint.X + dis;
        
            foreach (var k in datas)
            {
                var lists = k.Value;
                double step = dis;
                int j = 0;
                foreach (var kv in lists)
                {
                    double height = kv.Value / max * total * 0.95;
                    IPoint point = new PointClass() { X = xdis + num * dis, Y = pBasePoint.Y };
                    IPolygon columngeo = gh.CreateRectangle(point, dis, -height);
                    var ele = DrawPolygon(columngeo, cmykColors[j]);
                    eles.Add(ele);
                    //绘制注记
                    if (geoNum)
                    {
                        IPoint labelpoint = new PointClass();
                        labelpoint.PutCoords(point.X + dis / 2, point.Y + height + mapScale * 1e-3);

                        lbpt.Add(new PointClass { X = (labelpoint.X - pBasePoint.X) / total, Y = (labelpoint.Y - pBasePoint.Y) / total }, kv.Value.ToString());
                    }
                 
                    j++;
                    num++;
                }
                //绘制分类

                IPoint typePoint = new PointClass();
                typePoint.PutCoords(xdis + num * dis - lists.Count / 2 * dis, pBasePoint.Y - mapScale * 4e-3);
                num++;

            }
           
        }
        private void DrawColumns(IPoint pBasePoint, Dictionary<string, Dictionary<string, double>> datas, double max, List<ICmykColor> cmykColors)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double dis = 5 * 1e-3 * mapScale;//一个数据长度
            int groups = datas.Count;
            double total = 6 * 1e-2 * mapScale;
            int num = 0;
            double xdis = pBasePoint.X + dis;
            lbpt.Clear();
            foreach (var k in datas)
            {
                var lists = k.Value;
                double step = dis;
                int j = 0;
                foreach (var kv in lists)
                {
                    double height = kv.Value / max * total * 0.95;
                    IPoint point = new PointClass() { X = xdis + num * dis, Y = pBasePoint.Y };
                    IPolygon columngeo = gh.CreateRectangle(point, dis, -height);
                    var ele = DrawPolygon(columngeo, cmykColors[j]);
                    eles.Add(ele);
                    //绘制注记
                    if (geoNum)
                    {
                        IPoint labelpoint = new PointClass();
                        labelpoint.PutCoords(point.X + dis / 2, point.Y + height + mapScale * 1e-3);

                        lbpt.Add(new PointClass { X = (labelpoint.X - pBasePoint.X) / total, Y = (labelpoint.Y - pBasePoint.Y) / total }, kv.Value.ToString());
                    }
                    j++;
                    num++;
                }
                //绘制分类

                IPoint typePoint = new PointClass();
                typePoint.PutCoords(xdis + num * dis - lists.Count / 2 * dis, pBasePoint.Y - mapScale * 4e-3);

                num++;

            }
            //绘制title
            if (chartTitle != "")
            {
                IPoint titlePoint = new PointClass();
                titlePoint.PutCoords(pBasePoint.X + num * dis / 2, pBasePoint.Y + total + mapScale * 3e-3);

            }
        }

        private IPolyline ContructPolyLine(IPoint f, IPoint t)
        {
            try
            {
                IGeometryCollection pPolyline = new PolylineClass();
                IPointCollection pCl = new PathClass();
                pCl.AddPoint(f);
                pCl.AddPoint(t);
                pPolyline.AddGeometry(pCl as IGeometry);
                (pPolyline as ITopologicalOperator).Simplify();
                return pPolyline as IPolyline;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        private IElement DrawPolygon(IGeometry pgeo, IColor pcolor)
        {
           
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            smsymbol.Color = pcolor;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();
            smline.Style = esriSimpleLineStyle.esriSLSNull;
            smsymbol.Outline = smline;

            polygonElement.Symbol = smsymbol;

            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;

            return pEl;
        }
        private void DrawLengend2D(IPoint _basepoint, string[] subtypes, double columns, double groups)
        {
            lgpt.Clear();
            IPoint basepoint = (_basepoint as IClone).Clone() as IPoint;
            //每一个柱子基本宽度
            double basewidth = 5e-3 * mapScale;

            double xdistance = (columns + 1) * basewidth + (groups - 1) * basewidth;//X轴长度
            GraphicsHelper gh = new GraphicsHelper(pAc);
           
            //获取图列的高度
            double heightunit = 3.97427;//1号字体1万的高度
            double fontsize = 8;
            double lgheight = heightunit * fontsize * mapScale / 10000;
            int ct = subtypes.Length;
            //全部图例+文字长度
            double length = lgheight * 2 * ct;//图例长度
            double lginterval = mapScale * 4.0e-3;//图例间间隔
            double txtinterval = mapScale * 2.0e-3;//文字图例间隔
            length += lginterval * (ct - 1);//间隔
            foreach (string str in subtypes)
            {
                length += gh.GetStrWidth(str, mapScale, fontsize) + txtinterval;// ;//加文字宽度
            }
            //X轴长度
            double Xdis = xdistance;


            double dx = Xdis / 2 - length / 2;//平移距离
            double stepX = 0;
            for (int i = 0; i < subtypes.Length; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - 5.5e-3 * mapScale;
                upleft.PutCoords(stepX + basepoint.X + dx, y);
                //tulie
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                var ele = gh.DrawPolygon(prect, colors[i], 0);
                eles.Add(ele);
                stepX += lgheight * 2;
                //wenzi
                double strwidth = gh.GetStrWidth(subtypes[i], mapScale, fontsize);
                stepX += txtinterval;//文字间隔
                IPoint txtpoint = new PointClass() { X = txtinterval + upleft.X + strwidth / 2 + lgheight * 2, Y = upleft.Y - 0.8 * lgheight };
                ele = gh.DrawTxt(txtpoint, subtypes[i], fontsize);
                stepX += strwidth + lginterval;//文字距离+图例间隔
                double sx = (txtpoint.X - _basepoint.X) / (60.0e-3 * mapScale);
                double sy = (txtpoint.Y - _basepoint.Y) / (60.0e-3 * mapScale);
                lgpt.Add(new PointClass { X = sx, Y = sy }, subtypes[i]);

                
            }
        }
        private void DrawLengend2DNull(IPoint _basepoint, string[] subtypes, double columns, double groups)
        {
            IActiveView pAc = GApplication.Application.ActiveView;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IPoint basepoint = (_basepoint as IClone).Clone() as IPoint;
            //每一个柱子基本宽度
            double basewidth = 5e-3 * mapScale;

            double xdistance = (columns + 1) * basewidth + (groups - 1) * basewidth;//X轴长度


            //获取图列的高度
            double heightunit = 3.97427;//1号字体1万的高度
            double fontsize = 8;
            double lgheight = heightunit * fontsize * mapScale / 10000;
            int ct = subtypes.Length;
            //全部图例+文字长度
            double length = lgheight * 2 * ct;//图例长度
            double lginterval = mapScale * 4.0e-3;//图例间间隔
            double txtinterval = mapScale * 2.0e-3;//文字图例间隔
            length += lginterval * (ct - 1);//间隔
            foreach (string str in subtypes)
            {
                length += gh.GetStrWidth(str, mapScale, fontsize) + txtinterval;// ;//加文字宽度
            }
            //X轴长度
            double Xdis = xdistance;


            double dx = Xdis / 2 - length / 2;//平移距离
            double stepX = 0;
            for (int i = 0; i < subtypes.Length; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - 5.5e-3 * mapScale;
                upleft.PutCoords(stepX + basepoint.X + dx, y);
                //tulie
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                var ele = gh.DrawPolygonBg(prect, colors[i], 0);
                eles.Add(ele);
                stepX += lgheight * 2;
                //wenzi
                double strwidth = gh.GetStrWidth(subtypes[i], mapScale, fontsize);
                stepX += txtinterval;//文字间隔
                IPoint txtpoint = new PointClass() { X = txtinterval + upleft.X + strwidth / 2 + lgheight * 2, Y = upleft.Y - 0.8 * lgheight };
                ele = gh.DrawTxt(txtpoint, subtypes[i], fontsize);
                stepX += strwidth + lginterval;//文字距离+图例间隔




            }
        }
        #endregion

        //绘制分类柱状图
        public List<IElement> DrawClassifyColumns(ColumnJson columnInfo, IPoint _centerpoint)
        {

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pRepLayer = lyrs.First();

            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LANNO");
            })).ToArray();
            var annoly = lyr.First();
            annoFcl = (annoly as IFeatureLayer).FeatureClass;

            eles.Clear();
            pAc = GApplication.Application.ActiveView;
            gh = new GraphicsHelper(pAc);
            IPoint basePoint = basePointInit;
            basePointReal = _centerpoint;

            //columnInfo相关信息
            this.columnInfo = columnInfo;
            markerSize = columnInfo.Size;
            xyAxis = columnInfo.XYShow;
            geoNum = columnInfo.GeoNum;
            angle = columnInfo.TxtAngel;
            geoRel = columnInfo.GeoRel;
            chartTitle = columnInfo.Title;
            colors = new List<ICmykColor>();
            foreach (var c in columnInfo.Colors)
            {
                var cmyk = new CmykColorClass();
                cmyk.Cyan = c.C;
                cmyk.Yellow = c.Y;
                cmyk.Magenta = c.M;
                cmyk.Black = c.K;
                colors.Add(cmyk);
            }
            var datas = JsonHelper.CHDataSource(columnInfo.DataSource);
            double max = 0;
            string[] types = CreateStaticDatasCl(datas, ref max);
            int group = datas.Count;
            //绘制分类统计图
            DrawColumnCls(basePoint, datas, max);

            if (columnInfo.LengendShow)
            {
                DrawLengendCls(basePoint, types.ToArray(), group);
            }
            else
            {
                DrawLengendClsBg(basePoint, types.ToArray(), group);
            }
            int obj = 0;
            //尺寸

            string jsonText = JsonHelper.GetJsonText(columnInfo);
            ChartsToRepHelper ch = new ChartsToRepHelper();
            var repmarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, basePointReal, jsonText, out obj, markerSize);
            CreateAnnotionCl(repmarker, basePointReal, markerSize, datas, obj);

            pAc.Refresh();
            return eles;
        }
        double totalEle = 0;
        private void DrawColumnCls(IPoint pBasepoint, Dictionary<string, Dictionary<string, double>> datas, double max)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            
            //一个数据宽度

            int groups = datas.Count;
            double dis = 1333 / (2 * groups);
            dis = Math.Round(dis, 0);
            //x总长
            double xsum = groups * dis * 2;
            double total = 1000;
            int num = 0;
            double d = 1.0 / datas.Count * dis;
            double xdis = pBasepoint.X;
            lbpt.Clear();

            double step = 0;
            foreach (var k in datas)
            {
                var lists = k.Value;
                int j = 0;
                double h = 0;
                foreach (var kv in lists)
                {
                    double height = kv.Value / max * total;
                    IPoint point = new PointClass() { X = xdis + step, Y = pBasepoint.Y + h };
                    IPolygon columngeo = gh.CreateRectangle(point, dis, -height);
                    var ele = DrawPolygon(columngeo, colors[j]);
                    eles.Add(ele);
                    //存储数量注记
                    if (columnInfo.GeoNum)
                    {
                        IPoint labelpoint = new PointClass() { X = point.X + dis / 2.0, Y = point.Y + height / 2.0 };
                        IPoint pt = new PointClass() { X = (labelpoint.X - pBasepoint.X) / total, Y = (labelpoint.Y - pBasepoint.Y) / total };
                        lbpt.Add(pt, kv.Value.ToString());
                    } 
                    h += height;
                    j++;
                }
                step += 2 * dis;
                num++;
            }
        }
        private void DrawLengendCls(IPoint _basepoint, string[] subtypes, int groups)
        {
            lgpt.Clear();
            IPoint basepoint = (_basepoint as IClone).Clone() as IPoint;


            double dis = 1333 / (2 * groups);
            dis = Math.Round(dis, 0);
            totalEle = 1000 + dis + dis * 0.8;
            double xdistance = (groups * 2 - 1) * dis;//X轴长度

            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取图列的高度
            double heightunit = 3.97427;//1号字体1万的高度
            double fontsize = 8;
            double lgheight = dis * 0.8;
            int ct = subtypes.Length;
            //全部图例+文字长度
            double length = lgheight * 2 * ct;//图例长度
            double lginterval = dis;//图例间间隔
            double txtinterval = dis / 2;//文字图例间隔
            length += lginterval * (ct - 1);//间隔
            foreach (string str in subtypes)
            {
                length += gh.GetStrWidth(str, 10000, fontsize) + txtinterval;//加文字宽度
            }
            //X轴长度
            double Xdis = xdistance;


            double dx = Xdis / 2 - length / 2;//平移距离
            double stepX = 0;
            for (int i = 0; i < subtypes.Length; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - dis;
                upleft.PutCoords(stepX + basepoint.X + dx, y);
                //tulie
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                var ele = gh.DrawPolygon(prect, colors[i], 0);
                eles.Add(ele);
                stepX += lgheight * 5;
                //wenzi
                double strwidth = gh.GetStrWidth(subtypes[i], 10000, fontsize);
                stepX += txtinterval;//文字间隔
                double dy = lgheight / 2.0 - heightunit / 2.0;
                IPoint txtpoint = new PointClass() { X = txtinterval + upleft.X + strwidth * 5 + lgheight * 2, Y = upleft.Y - lgheight * 0.5 };
                ele = gh.DrawTxt(txtpoint, subtypes[i], fontsize);
                stepX += strwidth + lginterval;//文字距离+图例间隔
                double sx = (txtpoint.X - _basepoint.X) / 1000.0;
                double sy = (txtpoint.Y - _basepoint.Y) / 1000.0;

                lgpt.Add(new PointClass { X = sx, Y = sy }, subtypes[i]);
            }
        }

        private void DrawLengendClsBg(IPoint _basepoint, string[] subtypes, int groups)
        {
            lgpt.Clear();
            IActiveView pAc = GApplication.Application.ActiveView;
            IPoint basepoint = (_basepoint as IClone).Clone() as IPoint;
            double dis = 1333 / (2 * groups);
            dis = Math.Round(dis, 0);
            totalEle = 1000 + dis + dis * 0.8;
            double xdistance = (groups * 2 - 1) * dis;//X轴长度

            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取图列的高度
            double heightunit = 3.97427;//1号字体1万的高度
            double fontsize = 10;
            double lgheight = dis * 0.8;
            int ct = subtypes.Length;
            //全部图例+文字长度
            double length = lgheight * 2 * ct;//图例长度
            double lginterval = dis;//图例间间隔
            double txtinterval = dis / 2;//文字图例间隔
            length += lginterval * (ct - 1);//间隔
            foreach (string str in subtypes)
            {
                length += gh.GetStrWidth(str, 10000, fontsize) + txtinterval;// ;//加文字宽度
            }
            //X轴长度
            double Xdis = xdistance;


            double dx = Xdis / 2 - length / 2;//平移距离
            double stepX = 0;
            for (int i = 0; i < subtypes.Length; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - dis;
                upleft.PutCoords(stepX + basepoint.X + dx, y);
                //tulie
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                var ele = gh.DrawPolygonBg(prect, colors[i], 0);
                eles.Add(ele);
                stepX += lgheight * 2;
                //wenzi
                double strwidth = gh.GetStrWidth(subtypes[i], 10000, fontsize);
                stepX += txtinterval;//文字间隔
                double dy = lgheight / 2.0 - heightunit / 2.0;
                IPoint txtpoint = new PointClass() { X = txtinterval + upleft.X + strwidth / 2 + lgheight * 2, Y = upleft.Y - lgheight * 0.5 };
                ele = gh.DrawTxt(txtpoint, subtypes[i], fontsize);
                stepX += strwidth + lginterval;//文字距离+图例间隔

                double sx = (txtpoint.X - _basepoint.X) / 1000.0;
                double sy = (txtpoint.Y - _basepoint.Y) / 1000.0;

                lgpt.Add(new PointClass { X = sx, Y = sy }, subtypes[i]);
            }
        }

        #region 新增方法
        private void CreateAnnotionCl(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, Dictionary<string, Dictionary<string, double>> datas, int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double Xaxis;
            double Yaxis;
            double size = curms * 1.0e-3 * markersize;
            //1.确定柱状图长度和宽度
            if (RepMarker.Height < RepMarker.Width)
            {
                width = size;
                height = size * RepMarker.Height / RepMarker.Width;
            }
            else
            {
                height = size;
                width = size * RepMarker.Width / RepMarker.Height;
            }
            //2.确定XY轴当前比例尺实际大小
            Yaxis = 1000 / totalEle * height;
            Xaxis = width;
            //文字大小一个柱子宽度的0.8
            double fontsize = (totalEle - 1000) / 1.8 * 0.4;
            fontsize = fontsize / 1000 * Yaxis;
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);

            fontsize *= 2.83;
            //3.计算X轴文字坐标以及文字大小

            double step = 0.0;
            double dx = Xaxis / (datas.Count * 2 - 1);
            foreach (var kv in datas)
            {
                string key = kv.Key;
                var point = new PointClass();
                point.X = pAnchor.X + dx / 4.0 + step;
                point.Y = pAnchor.Y;
                InsertAnnoFeaTXT(point, key, fontsize * 0.5, id);
                step += dx * 2.0;
            }
            //4.计算图例文字坐标以及文字大小
            foreach (var kv in lgpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis;
                double y = kv.Key.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                InsertAnnoFeaCl(p, kv.Value, fontsize * 0.5, id);
            }
            //5.计算统计标注
            foreach (var kv in lbpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis;
                double y = kv.Key.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                InsertAnnoFeaCl(p, kv.Value, fontsize * 0.6, id);
            }
            //7.绘制标题
            if (chartTitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X + Xaxis / 5, Y = pAnchor.Y + 1.5 * Yaxis };
                InsertAnnoFeaCl(pt, chartTitle, fontsize * 1.0, id);
            }
        }
        private void CreateAnnotion2D(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, Dictionary<string, Dictionary<string, double>> datas, int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double Xaxis;
            double Yaxis;
            double size = curms * 1.0e-3 * markersize;
            //1.确定柱状图长度和宽度
            if (RepMarker.Height < RepMarker.Width)
            {
                width = size;
                height = size * RepMarker.Height / RepMarker.Width;
            }
            else
            {
                height = size;
                width = size * RepMarker.Width / RepMarker.Height;
            }
            //2.确定XY轴当前比例尺实际大小
            Yaxis = 60 / (68.6794) * height;
            Xaxis = width - Yaxis / 20;
            //
            double dy = Yaxis / 10;
            double fontsize = (dy / 5);
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);
            fontsize *= 2.83;
            //3.计算X轴文字坐标以及文字大小
            int j = 0;
            double dx = Xaxis / datas.Count;
            foreach (var kv in datas)
            {
                string key = kv.Key;
                var point = new PointClass();
                point.X = pAnchor.X + dx / 2 + j * dx;
                point.Y = pAnchor.Y - dy / 4 - fontsize * (curms * heightunit * 1e-4);
                InsertAnnoFeaTXT(point, key, fontsize, id);

                j++;

            }
            //4.计算y轴文字坐标以及文字大小
            for (int i = 0; i < Ykedu.Count; i++)
            {
                var point = new PointClass();
                point.X = pAnchor.X - dy / 2;
                var txtwidth = gh.GetStrWidth(Ykedu[i], curms, fontsize);
                point.X -= txtwidth / 2;
                point.Y = pAnchor.Y + i * dy;

                InsertAnnoFea(point, Ykedu[i], fontsize, id);
            }
            //5.计算图例文字坐标以及文字大小
            foreach (var kv in lgpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis;
                double y = kv.Key.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                InsertAnnoFea(p, kv.Value, fontsize * 0.8, id);
            }
            //6.计算统计标注
            foreach (var kv in lbpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis;
                double y = kv.Key.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                InsertAnnoFea(p, kv.Value, fontsize * 0.8, id);
            }
            //7.绘制标题
            if (chartTitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X + Xaxis / 2, Y = pAnchor.Y + Yaxis + Yaxis / 10 };
                InsertAnnoFea(pt, chartTitle, fontsize * 1.5, id);
            }
        }
        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor_, double markersize, Dictionary<string, Dictionary<string, double>> datas, int id)
        {
            IPoint pAnchor = new PointClass { X = pAnchor_.X, Y = pAnchor_.Y };
            double curms = pAc.FocusMap.ReferenceScale;

            double height;
            double width;
            double Xaxis;
            double Yaxis;
            double size = curms * 1.0e-3 * markersize;
            //1.确定柱状图长度和宽度
            if (RepMarker.Height < RepMarker.Width)
            {
                width = size;
                height = size * RepMarker.Height / RepMarker.Width;
            }
            else
            {
                height = size;
                width = size * RepMarker.Width / RepMarker.Height;
            }
            //2.确定XY轴当前比例尺实际大小

            Yaxis = 400.0 / (484.871) * height;
            double dy = Yaxis / 10;
            Xaxis = width - (5.22 + 30.386) / 40.4283 * dy;
            // 确定xy轴原点
            double mx = 35 / 40.4283 * dy;
            double my = 19.8967 / 40.4283 * dy;
            pAnchor.X -= mx;
            pAnchor.Y -= my;

            double fontsize = (dy / 5);
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);
            fontsize *= 2.83;//当前比例尺的字体大小

            //3.计算X轴文字坐标以及文字大小
            int j = 0;
            double dx = Xaxis / datas.Count;
            foreach (var kv in datas)
            {
                string key = kv.Key;
                var point = new PointClass();
                point.X = pAnchor.X + dx / 2 + j * dx;
                point.Y = pAnchor.Y - dy / 10 - fontsize * (curms * heightunit * 1e-4);
                InsertAnnoFeaTXT(point, key, fontsize, id);
                j++;

            }
            //4.计算y轴文字坐标以及文字大小
            for (int i = 0; i < Ykedu.Count; i++)
            {
                var point = new PointClass();
                point.X = pAnchor.X - dy / 2;
                var txtwidth = gh.GetStrWidth(Ykedu[i], curms, fontsize);
                point.X -= txtwidth / 2;
                point.Y = pAnchor.Y + i * dy;

                InsertAnnoFea(point, Ykedu[i], fontsize, id);
            }
            //5.计算图例文字坐标以及文字大小
            foreach (var kv in lgpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis;
                double y = kv.Key.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                InsertAnnoFea(p, kv.Value, fontsize * 0.8, id);
            }
            //6.计算统计标注
            double tw = 0;
            foreach (var kv in lbpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis + mx;
                double y = kv.Key.Y;
                y = y * Yaxis + my;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                //p.X -= tw;
                InsertAnnoFea(p, kv.Value, fontsize * 0.8, id);
                tw += gh.GetStrWidth(kv.Value, curms, fontsize) / 2;
            }
            ////7.绘制标题
            if (chartTitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X + Xaxis / 2, Y = pAnchor.Y + Yaxis + Yaxis / 10 };
                InsertAnnoFea(pt, chartTitle, fontsize * 1.5, id);
            }
        }
        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor_, double markersize, string columnTitle, int id, string axisName)
        {
            IPoint pAnchor = new PointClass { X = pAnchor_.X, Y = pAnchor_.Y };
            double curms = pAc.FocusMap.ReferenceScale;

            double height;
            double width;
            double Xaxis;
            double Yaxis;
            double size = curms * 1.0e-3 * markersize;
            //1.确定柱状图长度和宽度
            if (RepMarker.Height < RepMarker.Width)
            {
                width = size;
                height = size * RepMarker.Height / RepMarker.Width;
            }
            else
            {
                height = size;
                width = size * RepMarker.Width / RepMarker.Height;
            }
            //2.确定XY轴当前比例尺实际大小

            Yaxis = 400.0 / (484.871) * height;
            double dy = Yaxis / 10;
            Xaxis = width - (5.22 + 30.386) / 40.4283 * dy;
            // 确定xy轴原点
            double mx = 35 / 40.4283 * dy;
            double my = 19.8967 / 40.4283 * dy;
            pAnchor.X -= mx;
            pAnchor.Y -= my;

            double fontsize = (dy / 5);
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);
            fontsize *= 2.83;//当前比例尺的字体大小

            //3.计算X轴文字坐标以及文字大小
            //int j = 0;
            //double dx = 4*dy;
            //{
            //    string key = axisName;
            //    var point = new PointClass();
            //    point.X = pAnchor.X + dx / 2 + j * dx;
            //    point.Y = pAnchor.Y - dy / 10 - fontsize * (curms * heightunit * 1e-4);
            //    InsertAnnoFeaTXT(point, key, fontsize, id);
            //    j++;
            //}

            //4.计算y轴文字坐标以及文字大小
            for (int i = 0; i < Ykedu.Count; i++)
            {
                var point = new PointClass();
                point.X = pAnchor.X - dy / 2;
                var txtwidth = gh.GetStrWidth(Ykedu[i], curms, fontsize);
                point.X -= txtwidth / 2;
                point.Y = pAnchor.Y + i * dy;

                InsertAnnoFea(point, Ykedu[i], fontsize, id);
            }
            //5.计算图例文字坐标以及文字大小
            foreach (var kv in lgpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis;
                double y = kv.Key.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                InsertAnnoFea(p, kv.Value, fontsize * 0.8, id);
            }
            //6.计算统计标注
            double tw = 0;
            foreach (var kv in lbpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis + mx;
                double y = kv.Key.Y;
                y = y * Yaxis + my;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                //p.X -= tw;
                InsertAnnoFea(p, kv.Value, fontsize * 0.8, id);
                tw += gh.GetStrWidth(kv.Value, curms, fontsize) / 2;
            }
            ////7.绘制标题
            if (columnTitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X + Xaxis / 2, Y = pAnchor.Y + Yaxis + Yaxis / 10 };
                InsertAnnoFea(pt, columnTitle, fontsize * 1.5, id);
            }
        }
        private void InsertAnnoFeaTXT(IGeometry pGeometry, string annoName, double fontSize, int id)
        {
            IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize);
            IElement pElement = pTextElement as IElement;
            ITransform2D ptrans = pElement as ITransform2D;
            ptrans.Rotate(pGeometry as IPoint, Math.PI * angle / 180);
            IElement ele = ptrans as IElement;
            IFeature pFeature = annoFcl.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.Annotation = pElement;
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pFeature.Store();
        }
        private ITextElement CreateTextElement(IGeometry pGeoTxt, string txt, IFontDisp pFont, double size)
        {
            IRgbColor pColor = new RgbColorClass()
            {
                Red = 0,
                Blue = 0,
                Green = 0
            };

            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = pColor,
                Font = pFont,
                Size = size
            };
            ITextElement pTextElment = null;
            IElement pEle = null;
            pTextElment = new TextElementClass()
            {
                Symbol = pTextSymbol,
                ScaleText = true,
                Text = txt,
            };
            pEle = pTextElment as IElement;

            pEle.Geometry = pGeoTxt;
            return pTextElment;
        }
        private ITextElement CreateTextElementCl(IGeometry pGeoTxt, string txt, IFontDisp pFont, double size)
        {
            IRgbColor pColor = new RgbColorClass()
            {
                Red = 0,
                Blue = 0,
                Green = 0
            };

            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = pColor,
                Font = pFont,
                Size = size
            };
            ITextElement pTextElment = null;
            IElement pEle = null;
            pTextElment = new TextElementClass()
            {
                Symbol = pTextSymbol,
                ScaleText = true,
                Text = txt,
                HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter,
                VerticalAlignment = esriTextVerticalAlignment.esriTVACenter
            };
            pEle = pTextElment as IElement;

            pEle.Geometry = pGeoTxt;
            return pTextElment;
        }
        private void InsertAnnoFea(IGeometry pGeometry, string annoName, double fontSize, int id)
        {
            IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize);
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annoFcl.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.Annotation = pElement;
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pFeature.Store();
            //return true;
        }
        private void InsertAnnoFeaCl(IGeometry pGeometry, string annoName, double fontSize, int id)
        {
            IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElementCl(pGeometry, annoName, font, fontSize);
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annoFcl.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.Annotation = pElement;
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pFeature.Store();
            //return true;
        }
        private void DrawBgEle(int ct, IPoint pt, int f = 1)
        {
            //菱形基本高度为 10m
            double diaHeight = 1e-2 * mapScale;
            //柱基本宽度
            double baseWidth = 4e-2 * mapScale;
            //每个柱子间距
            double columnstep = 0;
            double dis = baseWidth + columnstep;
            double width = (dis) * ct;
            double height = 10 * baseWidth;
            height *= f;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IGeometry pgeo = gh.CreateRectangle(pt, width, -height);

            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Style = esriSimpleFillStyle.esriSFSNull;

            ISimpleLineSymbol smline = new SimpleLineSymbolClass();
            smline.Style = esriSimpleLineStyle.esriSLSNull;

            smsymbol.Outline = smline;
            polygonElement.Symbol = smsymbol;

            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
            eles.Add(pEl);
        }
        #endregion
    }
}

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Collections;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using stdole;
using System.Collections.Generic;
using SMGI.Common;
using System.Linq;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    /// <summary>
    /// 绘制3d柱状图
    /// </summary>
    public sealed class DrawColumn3D
    {
      

     
        private IActiveView pAc = null;
        private double mapScale = 1000;
        private IFeatureClass annoFcl = null;
        private ILayer pRepLayer = null;
        GraphicsHelper gh = null;
        private bool xyAxis = true;
        private double ykedu = 0;
        private double angle = 0;
        //绘制3D柱状图
        public DrawColumn3D(IActiveView pac, double ms)
        {
            pAc = pac;
           // mapScale = ms;
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

            gh = new GraphicsHelper(pAc);


        }

        private List<IElement> eles = new List<IElement>();
        IPoint basePointInit = new PointClass() { X = 0, Y = 0 };
        IPoint basePointReal = null;
        double markerSize = 20;
        bool geoNum = true;
        /// <summary>
        /// 外部公开函数：绘制3D柱状图
        /// </summary>
        public void Draw3DColumns(FrmCloumnChartsSet frm, IPoint centerpoint)
        {
            CommonMethods.ClearThematicCarto(centerpoint, (pRepLayer as IFeatureLayer).FeatureClass, annoFcl);
            eles.Clear();
            gh = new GraphicsHelper(pAc);
            basePointReal=centerpoint;
            markerSize = frm.MarkerSize;
            colors = frm.CMYKColors;
            xyAxis = frm.XYAxis;
            geoNum = frm.GeoNum;
            ykedu = frm.KeDu;
            angle = frm.TxtAngle;
            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("正在处理...");
            bool checkGeo = frm.GeoRelated;
            if (checkGeo)
            {
                Draw3DColumnGeo(frm);
            }
            else
            {
                Draw3DColumnUnGeo(frm);
            }
            wo.Dispose();
            pAc.Refresh();
            MessageBox.Show("生成完成");
        }
        #region
        private string[] CreateStaticDatas(  Dictionary<string, Dictionary<string, double>> datas, ref double max )
        {

            List<string> types = new List<string>();
            var data = datas.First().Value;
            foreach (var k in data)
            {
                types.Add(k.Key);
            }
            foreach(var kv in datas)
            {
                var vals = kv.Value.OrderByDescending(r => r.Value);

                max = vals.First().Value > max ? vals.First().Value : max;
               
            }
            return types.ToArray();
        }
        List<int> colorlist = new List<int>();
        List<ICmykColor> colors = null;
        private  string chartTitle = "";
        bool LengendDraw = true;
        //不做地理关联
        private void  Draw3DColumnUnGeo(FrmCloumnChartsSet frm)
        {
            IPoint basePoint = basePointInit;
            colorlist.Clear();
            lbpt.Clear();
            #region
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

            Dictionary<string, Dictionary<string, double>> datas = frm.ChartDatas;
            string[] types = CreateStaticDatas(datas, ref max);
            columns = datas.Count * datas.First().Value.Count;
            chartTitle = frm.ChartTitle;
            //组数
            int groups = datas.Count;
            markerSize = frm.MarkerSize;
            colors = frm.CMYKColors;
            //绘制xy轴
            double kedu = Math.Ceiling(max) / frm.s;
            kedu = ykedu;
            max = ykedu * 10;
            if (xyAxis)
            {
                DrawXYaxi(basePoint, kedu, columns, groups);
            }
            else
            {
                DrawXYaxiNull(basePoint, kedu, columns, groups);
                Ykedu.Clear();
            }
            LengendDraw = xyAxis;
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
            #endregion
            //绘制图例
          //  if (frm.GeoLengend)
            {
                DrawLengend(basePoint, types, columns, groups, frm.GeoLengend);
            }


            int obj = 0;
            var columnInfo = frm.columnInfo;
            //尺寸

            columnInfo.ThematicType = "3D柱状图";
            string jsonText = JsonHelper.GetJsonText(columnInfo);
            //ChartsToRepHelper ch = new ChartsToRepHelper();
            // var repmarker = ch.CreateFeature(pAc,eles, pRepLayer, basePointReal, out obj, jsonText, markerSize);
            //创建白色图表背景
            if (!frm.IsTransparent)
            {
                CreateWhiteBackGround(eles);
            }
            var repmarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, basePointReal, jsonText, out obj, markerSize);
            CreateAnnotion(repmarker, basePointReal, markerSize, datas, obj);
        }
        //地理关联
        private void Draw3DColumnGeo(FrmCloumnChartsSet frm)
        {
            colorlist.Clear();          
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
         
            Dictionary<string, Dictionary<string, double>> datas = frm.ChartDatas;
            string geoRelate = frm.GeoLayer;                                                    //地理空间关联
            Dictionary<string, IPoint> namesPt = ChartsDataSource.ObtainGeoRelated(geoRelate);  
            string[] types = CreateStaticDatas(datas, ref max);

            foreach (var chartsData in datas)
            {
                lbpt.Clear();
                eles.Clear();
                IPoint basePoint = (basePointInit as IClone).Clone() as IPoint;
                string name = chartsData.Key;
                var dicvals = chartsData.Value;
                if (geoRelate != "")
                {
                    if (namesPt.ContainsKey(name))
                        basePointReal = namesPt[name];
                }
                columns = dicvals.Count;
                string chartTitle = frm.ChartTitle;
                //组数
                int groups = 1;             
                //绘制xy轴
                double kedu = max / 9.5;
                kedu = ykedu;
                max = ykedu * 10;
                if (xyAxis)
                {
                    DrawXYaxi(basePoint, kedu, columns, groups);
                }
                else
                {
                    DrawXYaxiNull(basePoint, kedu, columns, groups);
                    Ykedu.Clear();
                }
                //每根柱子颜色
                //绘制柱子
                int step = 0;
                //柱高度 
                double reheight = 2 * baseWidth;//柱高度
                double dis = baseWidth + columnstep;
                       
                int color = 0;
                DrawBgEle(dicvals.Count, new PointClass() { X = basePoint.X, Y = basePoint.Y });
                //var dicvals = kv1.Value;
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
                    if (frm.GeoNum)
                    {
                        IPoint labelpoint = new PointClass();
                        double dx = diaHeight / Math.Tan(Math.PI / 4) + baseWidth;
                        labelpoint.PutCoords(leftdown.X + dx / 2, leftdown.Y + mapScale * 1.3e-2);
                        double total = 4e-1 * mapScale;
                        lbpt.Add(new PointClass { X = (labelpoint.X - basePoint.X) / total, Y = (labelpoint.Y - basePoint.Y) / total }, kv.Value.ToString());
                        //ele = DrawTxt(labelpoint, kv.Value.ToString(), 25);
                        //eles.Add(ele);

                    }
                    step++;
                    color++;
                    #endregion
                }
                    
                //绘制图例
           //     if (frm.GeoLengend)
                {
                    DrawLengend(basePoint, types, columns, groups, frm.GeoLengend);
                }
               
                int obj = 0;

                var columnInfo = frm.columnInfo;
                var ds = new Dictionary<string, Dictionary<string, double>>();
                ds[chartsData.Key] = chartsData.Value;
                string jsdata = JsonHelper.JsonChartData(ds);
                columnInfo.DataSource = jsdata;
                //尺寸
                columnInfo.ThematicType = "3D柱状图";
                string jsonText = JsonHelper.GetJsonText(columnInfo);
                //创建白色图表背景
                if (!frm.IsTransparent)
                {
                    CreateWhiteBackGround(eles);
                }
                var repmarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, basePointReal, jsonText, out obj, markerSize);

                CreateAnnotion(repmarker, basePointReal, markerSize,  obj,chartsData.Key);
  
            }
         
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

        /// <summary>
        /// 将文字注记添加到注记类
        /// </summary>
        Dictionary<IPoint, string> lbpt = new Dictionary<IPoint, string>();//数量标注
        Dictionary<IPoint, string> lgpt = new Dictionary<IPoint, string>();//图例
        List<string> Ykedu = new List<string>();//刻度
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
           
            Yaxis = 400.0/ (484.871) * height;
            double dy = Yaxis / 10;
            Xaxis = width - (5.22+30.386)/40.4283*dy;
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
            if (LengendDraw)
            {
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
            double tw=0;
            foreach (var kv in lbpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis+mx;
                double y = kv.Key.Y;
                y = y * Yaxis+my;

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
        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor_, double markersize,  int id,string axisName)
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
            if (chartTitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X + Xaxis / 2, Y = pAnchor.Y + Yaxis + Yaxis / 10 };
                InsertAnnoFea(pt, chartTitle, fontsize * 1.5, id);
            }
        }
        #region
        private List<ICmykColor> createColor(int type)
        {
            List<ICmykColor> icolors = new List<ICmykColor>();
            Dictionary<int, Color> colorsDic = new Dictionary<int, Color>();
            int ct = 1;
            foreach (var c in (typeof(Color)).GetMembers())
            {
                if (c.MemberType == System.Reflection.MemberTypes.Property)
                {
                    Color item = Color.FromName(c.Name);
                    colorsDic[ct++] = item;
                }
            }
            Random r = new Random();
            List<int> temps = new List<int>();
            for (int i = 0; i < type; i++)
            {
                int color = r.Next(1, ct - 1);
                while (temps.Contains(color))
                {
                    color = r.Next(1, ct - 1);
                }
                temps.Add(color);
                IColor pc = ConvertColorToIColor(colorsDic[color]);
                ICmykColor pcolor = ConvertRGBToCMYK(pc as IRgbColor);
                icolors.Add(pcolor);
            }
            return icolors;
        }
        private ICmykColor ConvertRGBToCMYK(IRgbColor rgb)
        {
            ICmykColor pcolor = new CmykColorClass();
            double c = (double)(255 - rgb.Red) / 255;
            double m = (double)(255 - rgb.Green) / 255;
            double y = (double)(255 - rgb.Blue) / 255;
            double k = (double)Math.Min(c, Math.Min(m, y));
            if (k == 1.0)
            {
                c = m = y = 0;
                k = 0.6;
            }
            else
            {
                c = (c - k) / (1 - k);
                m = (m - k) / (1 - k);
                y = (y - k) / (1 - k);
            }
            c *= 100;
            m *= 100;
            y *= 100;
            k *= 100;
            pcolor.Cyan = (int)c;
            pcolor.Magenta = (int)m;
            pcolor.Yellow = (int)y;
            pcolor.Black = (int)k;

            return pcolor;
        }
        //.net color转为Icolor
        private IColor ConvertColorToIColor(Color p_Color)
        {
            IColor pColor = new RgbColorClass { RGB = p_Color.B * 65536 + p_Color.G * 256 + p_Color.R };
            return pColor;
        }
        private ISimpleFillSymbol GetColumnSymbol(int type)
        {
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            IRgbColor rgb = new RgbColorClass();
            if (type == 0)
            {
                rgb.Red = 61;
                rgb.Green = 119;
                rgb.Blue = 188;
            }
            else if (type == 1)
            {
                rgb.Red = 40;
                rgb.Green = 84;
                rgb.Blue = 136;
            }
            else if (type == 2)
            {
                rgb.Red = 29;
                rgb.Green = 65;
                rgb.Blue = 111;
            }
            else if (type == 3)
            {
                rgb.Red = 193;
                rgb.Green = 63;
                rgb.Blue = 59;
            }
            else if (type == 4)
            {
                rgb.Red = 138;
                rgb.Green = 40;
                rgb.Blue = 38;
            }
            else if (type == 5)
            {
                rgb.Red = 114;
                rgb.Green = 29;
                rgb.Blue = 26;
            }
            else if (type == 6)
            {
                rgb.Red = 149;
                rgb.Green = 186;
                rgb.Blue = 75;
            }
            else if (type == 7)
            {
                rgb.Red = 107;
                rgb.Green = 133;
                rgb.Blue = 49;
            }
            else if (type == 8)
            {
                rgb.Red = 86;
                rgb.Green = 109;
                rgb.Blue = 36;
            }
            ILineSymbol linesym = new SimpleLineSymbolClass();
            linesym.Width = 0.05;
            smsymbol.Outline = linesym;
            smsymbol.Color = rgb;
            return smsymbol;
        }
        //绘制三维柱状
        private IElement DrawColunnPolygon(IGeometry geo, IColor pcolor)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
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
                
                pAc.Refresh();
                //eles.Add(pEl);
                return pEl;


            }
            catch
            {
                return pEl;
            }
        }
        #endregion
        /// <summary>
        /// 绘制图例
        /// </summary>
        /// <param name="basepoint"></param>
        private void DrawLengend(IPoint _basepoint, string[] subtypes, double columns, double groups,bool lg)
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
                var ele= gh.DrawPolygon(prect, colors[i], 0);
                if (!lg)
                {
                    var sf = new SimpleFillSymbolClass { Style = esriSimpleFillStyle.esriSFSNull };
                    ISimpleLineSymbol smline = new SimpleLineSymbolClass();
                    smline.Style = esriSimpleLineStyle.esriSLSNull;
                    sf.Outline = smline;
                    (ele as IFillShapeElement).Symbol = sf;
                }
                eles.Add(ele);
                 
                stepX += lgheight * 2;
                if (lg)
                {
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
        }
        //绘制xy轴
        private void DrawXYaxi(IPoint orgin, double val, double columns, double groups,bool draw=false)
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
                var ele= DrawLine(pc as IPolyline);
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
            var ele1=   DrawLine(pline as IPolyline); eles.Add(ele1);
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
       
        private IElement DrawLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polylineElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 2;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = 122;
                rgb.Blue = 122;
                rgb.Green = 122;
                linesym.Color = rgb;
                polylineElement.Symbol = linesym;
                pEl = polylineElement as IElement;
                pEl.Geometry = pline as IGeometry;
               
                eles.Add(pEl);
                pAc.Refresh();

            }
            catch
            {

            }
            return pEl;
        }
        private IElement DrawLineNull(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
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

                eles.Add(pEl);
                pAc.Refresh();

            }
            catch
            {

            }
            return pEl;
        }
        private IElement DrawTxt(IPoint point, string txt, double fontsize)
        {

            IFontDisp pFont = new StdFont()
            {
                Name = "黑体",
                Size = 16
            } as IFontDisp;
            ITextSymbol pTextSymbol = new TextSymbolClass()
            {

                Font = pFont,
                Size = fontsize
            };

            try
            {
                IElement pEl = null;
                ITextElement ptxt = new TextElementClass();
                ptxt.Text = txt.Trim();
                ptxt.ScaleText = true;
                ptxt.Symbol = pTextSymbol;
                pEl = ptxt as IElement;
                pEl.Geometry = point;
                IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                
                pAc.Refresh();
                return pEl;
            }
            catch
            {
                return null;
            }
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
                Text = txt
            };
            pEle = pTextElment as IElement;

            pEle.Geometry = pGeoTxt;
            return pTextElment;
        }

        private void CreateWhiteBackGround(List<IElement> eles)
        {
            IEnvelope unionEnv = new EnvelopeClass();
            foreach (IElement el in eles)
            {
                IEnvelope env= el.Geometry.Envelope;
                unionEnv.Union(env);
            }
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            RectangleElementClass polygonElement = new RectangleElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            ISimpleLineSymbol linesym = new SimpleLineSymbolClass();
            linesym.Style = esriSimpleLineStyle.esriSLSNull;
            smsymbol.Outline = linesym;
            smsymbol.Color = new RgbColorClass() { Red = 255, Green = 255, Blue = 255} as IColor;
            polygonElement.Symbol = smsymbol;
            pEl = polygonElement as IElement;
            pEl.Geometry = unionEnv;
            eles.Insert(0, pEl);
        }
        #endregion
    }
}

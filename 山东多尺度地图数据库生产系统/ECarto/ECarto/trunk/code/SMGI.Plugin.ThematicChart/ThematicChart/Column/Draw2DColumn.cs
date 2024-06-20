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
using System.Linq;
using SMGI.Common;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    /// <summary>
    /// 绘制2D柱状图
    /// </summary>
    public sealed class Draw2DColumn
    {
        private IActiveView pAc = null;
        private double mapScale = 1000;
        private List<IElement> eles = new List<IElement>();
        private ILayer pRepLayer = null;
        GraphicsHelper gh = null;
        bool geoNum = true;
        IFeatureClass annoFcl = null;
        //绘制2D柱状图
        public Draw2DColumn(IActiveView pac, double ms)
        {
            pAc = pac;
            //mapScale = ms;
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
        private double ykedu;
        public void Create2DColumns(FrmCloumnChartsSet frm, IPoint centerpoint)
        {
            CommonMethods.ClearThematicCarto(centerpoint, (pRepLayer as IFeatureLayer).FeatureClass, annoFcl);
            geoNum = frm.GeoNum;
            ykedu = frm.KeDu;
            angle = frm.TxtAngle;
            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("正在处理...");
            if (frm.GeoRelated)
            {
                Create2DColumnsGeo(centerpoint, frm);
            }
            else
            {
                Create2DColumnsUnGeo(centerpoint, frm);
            }
            wo.Dispose();
            pAc.Refresh();
            MessageBox.Show("生成完成");
        }
            
        /// <summary>
        /// 公开函数：绘制柱状图
        /// </summary>
        private void Create2DColumnsUnGeo(IPoint centerpoint,FrmCloumnChartsSet frm)
        {
            eles.Clear();
            double max = 0;

            //获取颜色

            Dictionary<string, Dictionary<string, double>> datas = frm.ChartDatas;
            string[] types = CreateStaticDatas(datas, ref max);
            chartTitle = frm.ChartTitle;
            cmykColors = frm.CMYKColors;
            double markersize=frm.MarkerSize;
            double kedu = max / 9.5;
            kedu = ykedu;
            max = kedu * 10;
            IPoint orpt = new PointClass() { X = 0, Y = 0 };
            int columns = datas.Count * datas.First().Value.Count;
          //  if (frm.XYAxis)
            {
                DrawXYaxis(orpt, kedu, columns, datas.Count, frm.XYAxis);
            }
            DrawColumns(orpt, datas, max, cmykColors);
            //if (frm.GeoLengend)
            {
                DrawLengend(orpt, types, columns, datas.Count, frm.GeoLengend);
            }
            int obj = 0;
           // var repmarker = gh.CreateFeaturesEx(eles, pRepLayer, centerpoint,out obj,markersize);
            var columnInfo = frm.columnInfo;
            //尺寸
            columnInfo.ThematicType = "2D柱状图";
            string jsonText = JsonHelper.GetJsonText(columnInfo);
            //创建白色图表背景
            if (!frm.IsTransparent)
            {
                CreateWhiteBackGround(eles);
            }
            var repmarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, centerpoint, jsonText, out obj, frm.MarkerSize);
            CreateAnnotion(repmarker, centerpoint, markersize, datas, obj); 
        }
        //地理关联
        string geoRelate = "";
        private void Create2DColumnsGeo(IPoint _centerpoint, FrmCloumnChartsSet frm)
        {
            double max = 0;

            //获取颜色
            
            Dictionary<string, Dictionary<string, double>> datas = frm.ChartDatas;
            string[] types = CreateStaticDatas(datas, ref max);
            chartTitle = frm.ChartTitle;
            cmykColors = frm.CMYKColors;
            double kedu = ykedu;
            max = kedu * 10;
            geoRelate = frm.GeoLayer;
            Dictionary<string, IPoint> namesPt = ChartsDataSource.ObtainGeoRelated(geoRelate);
            foreach (var chartsData in datas)
            {
                eles.Clear();
                IPoint centerpoint = (_centerpoint as IClone).Clone() as IPoint;
                IPoint orpt = new PointClass() { X = 0, Y = 0 };
                string name = chartsData.Key;
                if (geoRelate != "")
                {
                    if (namesPt.ContainsKey(name))
                        centerpoint = namesPt[name];
                }
                var dicval = chartsData.Value;
            
                int columns = dicval.Count;
                // if (frm.XYAxis)
                {
                    DrawXYaxis(orpt, kedu, columns, 1, frm.XYAxis);
                }
                DrawColumnsGeo(orpt, dicval, max, cmykColors);
              //  if (frm.GeoLengend)
                {
                    DrawLengend(orpt, types, columns, 1, frm.GeoLengend);
                }

                //gh.CreateFeatures(eles, pRepLayer, centerpoint, frm.MarkerSize);
                int obj = 0;
                var columnInfo = frm.columnInfo;
                Dictionary<string, Dictionary<string, double>> ds = new Dictionary<string, Dictionary<string, double>>();
                ds.Add(chartsData.Key,chartsData.Value);
                columnInfo.DataSource = JsonHelper.JsonChartData(ds);
                //尺寸
                columnInfo.ThematicType = "2D柱状图";
                string jsonText = JsonHelper.GetJsonText(columnInfo);
                //创建白色图表背景
                if (!frm.IsTransparent)
                {
                    CreateWhiteBackGround(eles);
                }
                var repmarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, centerpoint, jsonText, out obj, frm.MarkerSize);
                CreateAnnotiongeo(repmarker, centerpoint, frm.MarkerSize, datas, obj);
            }
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
        List<ICmykColor> cmykColors = null;
        string chartTitle = "";
       

        private ISimpleFillSymbol GetColorSymbol(IColor pcolor)
        {
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Color = pcolor;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();
            smline.Style = esriSimpleLineStyle.esriSLSNull;
            smline.Width =1.75;
            smline.Color = new RgbColorClass() { Red = 220, Blue = 220, Green = 220 };
            smsymbol.Outline = smline;
            return smsymbol;
        }
        private void DrawLengend(IPoint _basepoint, string[] subtypes, double columns, double groups,bool lg)
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
                var ele= gh.DrawPolygon(prect, cmykColors[i], 0);
                if (!lg)
                {
                  
                    var sf=new SimpleFillSymbolClass { Style = esriSimpleFillStyle.esriSFSNull };
                    ISimpleLineSymbol smline = new SimpleLineSymbolClass();
                    smline.Style = esriSimpleLineStyle.esriSLSNull;
                    sf.Outline = smline;
                    (ele as IFillShapeElement).Symbol = sf;
                }
                eles.Add(ele);
                stepX += lgheight * 2;
                //wenzi
                if (lg)
                {
                    double strwidth = gh.GetStrWidth(subtypes[i], mapScale, fontsize);
                    stepX += txtinterval;//文字间隔
                    IPoint txtpoint = new PointClass() { X = txtinterval + upleft.X + strwidth / 2 + lgheight * 2, Y = upleft.Y - 0.8 * lgheight };
                    ele = gh.DrawTxt(txtpoint, subtypes[i], fontsize);
                    stepX += strwidth + lginterval;//文字距离+图例间隔

                    double sx = (txtpoint.X - _basepoint.X) / (60.0e-3 * mapScale);
                    double sy = (txtpoint.Y - _basepoint.Y) / (60.0e-3 * mapScale);

                    lgpt.Add(new PointClass { X = sx, Y = sy }, subtypes[i]);
                }
                
                //eles.Add(ele);
            }
        }
        /// <summary>
        /// 绘制xy轴
        /// </summary>
        /// <param name="orgin">坐标原点</param>
        /// <param name="val">一个单位刻度值</param>
        /// <param name="columns">柱总数</param>
        /// <param name="groups">类别</param>
        Dictionary<IPoint, string> YkeduGeo = new Dictionary<IPoint, string>();
        private void DrawXYaxis(IPoint orgin, double val, double columns, double groups,bool axis)
        {
            Ykedu.Clear();
            YkeduGeo.Clear();
            //每一个柱子基本宽度
            double basewidth = 5e-3 * mapScale;
            double total = 6 * 1e-2 * mapScale;
            double xdistance = (columns + 1) * basewidth + (groups - 1) * basewidth;//X轴长度
            xdistance += basewidth;
            //1 横线
            double cx = orgin.X - 2e-3 * mapScale;
            double cy = orgin.Y ;
            double step =0.6*1e-2 * mapScale;//每个刻度间距
            for (int i = 0; i < 11; i++)
            {
                IPointCollection pc = new PolylineClass();
                IPoint pf = new PointClass() { X = cx, Y = orgin.Y + i * step };
                IPoint pt = new PointClass() { X = cx+xdistance, Y = orgin.Y + i * step };
                pc.AddPoint(pf);
                pc.AddPoint(pt);
                var ele = DrawLine(pc as IPolyline);
                if (!axis)
                {

                    (ele as ILineElement).Symbol = new SimpleLineSymbolClass { Style = esriSimpleLineStyle.esriSLSNull };
                }
                eles.Add(ele);
            }
            //y轴
            IPoint xpoint = new PointClass() { X = orgin.X, Y = orgin.Y };
            IPoint xpoint1 = new PointClass() { X = orgin.X, Y = orgin.Y+10 * step };
            IGeometry pline = ContructPolyLine(xpoint1, xpoint);
            
            var eley = DrawLine(pline as IPolyline);
            if (!axis)
            {

                (eley as ILineElement).Symbol = new SimpleLineSymbolClass { Style = esriSimpleLineStyle.esriSLSNull };
            }
            eles.Add(eley);
            
            //Y刻度
            for (int i = 0; i < 11; i++)
            {
               
                int txtval = (int)(i * val);
                string txt = (txtval).ToString();
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(cx - txt.Length  * 1e-3 * mapScale, orgin.Y + i * step);
                if (axis)
                {
                    if (geoRelate != "")
                    {
                        IPoint pt = new PointClass() { X = (txtpoint.X - orgin.X) / total, Y = (txtpoint.Y - orgin.Y) / total };
                        YkeduGeo.Add(pt, txt);
                    }
                    else
                    {
                        Ykedu.Add(txt);
                    }
                }
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
                
                var ele = DrawLine(pc as IPolyline);
                if (!axis)
                {

                    (ele as ILineElement).Symbol = new SimpleLineSymbolClass { Style = esriSimpleLineStyle.esriSLSNull };
                }
                eles.Add(ele);
                 
            }
            

        }
        ///绘制柱  
        private void DrawColumnsGeo(IPoint pBasePoint,  Dictionary<string, double> datas, double max, List<ICmykColor> cmykColors)
        {
            lbpt.Clear();
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double dis = 5 * 1e-3 * mapScale;//一个数据长度
            int groups = datas.Count;
            double total = 6 * 1e-2 * mapScale;
            int num = 0;
            double xdis = pBasePoint.X + dis;
            
            var lists = datas;
            double step = dis;
            int j = 0;
            foreach (var kv in lists)
            {
                double height = kv.Value / max * total * 0.95;
                IPoint point = new PointClass() { X = xdis + num * dis, Y = pBasePoint.Y };
                IPolygon columngeo = gh.CreateRectangle(point, dis, -height);
                var ele=  DrawPolygon(columngeo, cmykColors[j]);
                eles.Add(ele);
                //绘制注记
                if (geoNum)
                {
                    IPoint labelpoint = new PointClass();
                    labelpoint.PutCoords(point.X + dis / 2, point.Y + height + mapScale * 1e-3);
                    //ele = DrawTxt(labelpoint, kv.Value.ToString(), 4);
                    //eles.Add(ele);
                    IPoint pt = new PointClass() { X = (labelpoint.X - pBasePoint.X) / total, Y = (labelpoint.Y - pBasePoint.Y) / total };
                    lbpt.Add(pt, kv.Value.ToString());
                }
                j++;
                num++;
            }
            DrawBgEle(groups, pBasePoint);
             
            
        }
        private void DrawBgEle(int ct, IPoint pt, int f = 1)
        {
           
            double  basewidth = 5 * 1e-3 * mapScale;//一个数据长度

            double height = 6 * 1e-2 * mapScale;



            double width = (basewidth) * ct;
           
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
        double angle;
        /// <summary>
        /// 创建文字标注注记到ANNO
        /// </summary>
        Dictionary<IPoint, string> lbpt = new Dictionary<IPoint, string>();
        Dictionary<IPoint, string> lgpt = new Dictionary<IPoint, string>();
        List<string> Ykedu = new List<string>();
        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, Dictionary<string, Dictionary<string, double>> datas,int id)
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
                point.X = pAnchor.X + dx / 2+j*dx;
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
                point.Y = pAnchor.Y +i*dy;
                 
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
                x = x *Yaxis;
                double y = kv.Key.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X+x;
                p.Y = pAnchor.Y+y;

                InsertAnnoFea(p, kv.Value, fontsize*0.8, id);
            }
            //7.绘制标题
            if (chartTitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X + Xaxis / 2, Y = pAnchor.Y + Yaxis + Yaxis  / 10 };
                InsertAnnoFea(pt, chartTitle, fontsize * 1.5, id);
            }
        }
        private void CreateAnnotiongeo(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, Dictionary<string, Dictionary<string, double>> datas, int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            //double Xaxis;
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
            Yaxis = 170.0 / 194.682 * height;
            //
            double dy = Yaxis / 10;
            double fontsize = (dy / 5);
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);
            fontsize *= 2.83;
            //3.计算X轴文字坐标以及文字大小
           
            //4.计算y轴文字坐标以及文字大小
            foreach (var k in YkeduGeo  )
            {

                var point = new PointClass();
                point.X = pAnchor.X - dy / 2;
                var txtwidth = gh.GetStrWidth(k.Value, curms, fontsize);
                point.X -= txtwidth / 2;
                //point.Y = pAnchor.Y + i * dy;
                point.Y = pAnchor.Y + k.Key.Y * Yaxis;
                InsertAnnoFea(point, k.Value , fontsize, id);
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
                IPoint pt = new PointClass { X = pAnchor.X + width / 2, Y = pAnchor.Y + Yaxis + Yaxis / 10 };
                InsertAnnoFea(pt, chartTitle, fontsize * 1.5, id);
            }
        }
       
        ///绘制柱  
        private void DrawColumns(IPoint pBasePoint, Dictionary<string, Dictionary<string, double>> datas, double max, List<ICmykColor> cmykColors)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double dis = 5* 1e-3 * mapScale;//一个数据长度
            int groups = datas.Count;
            double total = 6* 1e-2 * mapScale;
            int num = 0;
            double xdis = pBasePoint.X + dis;
            lbpt.Clear();
            foreach(var k in datas)
            {
                var lists = k.Value;
                double step = dis;
                int j = 0;
                foreach (var kv in lists)
                {
                    double height = kv.Value / max * total * 0.95;
                    IPoint point = new PointClass() { X = xdis + num * dis, Y = pBasePoint.Y  };
                    IPolygon columngeo = gh.CreateRectangle(point, dis, -height);
                    var ele=  DrawPolygon(columngeo, cmykColors[j]);
                    eles.Add(ele);
                    //绘制注记
                    if (geoNum)
                    {
                        IPoint labelpoint = new PointClass();
                        labelpoint.PutCoords(point.X + dis / 2, point.Y + height + mapScale * 1e-3);
                       
                        lbpt.Add(new PointClass { X = (labelpoint.X - pBasePoint.X)/total, Y = (labelpoint.Y - pBasePoint.Y)/total }, kv.Value.ToString());
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

        private IElement DrawPolygon(IGeometry pgeo, IColor pcolor)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
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
        private IElement DrawLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 0.2;
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
        private IElement DrawTxt(IPoint point, string txt, double fontsize)
        {

            IFontDisp pFont = new StdFont()
            {
                Name = "黑体",
                Size = 2
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
            ptrans.Rotate(pGeometry as IPoint , Math.PI * angle / 180);
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
                HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter,
                VerticalAlignment=esriTextVerticalAlignment.esriTVABottom,
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
                IEnvelope env = el.Geometry.Envelope;
                unionEnv.Union(env);
            }
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            RectangleElementClass polygonElement = new RectangleElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            ISimpleLineSymbol linesym = new SimpleLineSymbolClass();
            linesym.Style = esriSimpleLineStyle.esriSLSNull;
            smsymbol.Outline = linesym;
            smsymbol.Color = new RgbColorClass() { Red = 255, Green = 255, Blue = 255 } as IColor;
            polygonElement.Symbol = smsymbol;
            pEl = polygonElement as IElement;
            pEl.Geometry = unionEnv;
            eles.Insert(0, pEl);
        }
    }
}

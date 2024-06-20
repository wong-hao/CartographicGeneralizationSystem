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
    /// 单系列条形图
    /// </summary>
    public  class DrawBarSingle
    {
        private IActiveView pAc = null;
        private double mapScale = 10000;
        private List<IElement> eles = new List<IElement>();
        private ILayer pRepLayer = null;
        private IFeatureClass annoFcl = null;
        GraphicsHelper gh = null;
        private string chartTitle = "";
        private IColor bgColor = null;
        double markerSize = 20;
        public double ColumnStep = 0.5;//条形间距
        //绘制斜轴柱状图
        public DrawBarSingle(IActiveView pac, double ms)
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
            annoFcl = (lyr.First() as IFeatureLayer ).FeatureClass;

            gh = new GraphicsHelper(pAc);
        }
        public void CreateSingleBar(FrmSingleBarChartsSet frm,IPoint centerpoint)
        {
            CommonMethods.ClearThematicCarto(centerpoint, (pRepLayer as IFeatureLayer).FeatureClass, annoFcl);
            chartTitle = frm.ChartTitle;
            bgColor = frm.CMYKColors;
            markerSize = frm.MarkerSize;
            ColumnStep = Math.Round(frm.ColumnStep * 0.01, 2);
            if (frm.GeoRelated)
            {
                CreateSingleBarGeo(centerpoint, frm);
            }
            else
            {
                CreateSingleBarUnGeo(centerpoint, frm);
            }
            pAc.Refresh();
            MessageBox.Show("生成完成");
        }
        /// <summary>
        /// 绘制条形图
        /// </summary>
        public void CreateSingleBarGeo(IPoint _centerpoint,FrmSingleBarChartsSet frm)
        {
            double max = 0;
            //获取设置
            IColor  cmykColors = frm.CMYKColors;
            var groupdatas = frm.ChartDatas;

            foreach (var kv in groupdatas)
            {
                var vals = kv.Value.OrderByDescending(r => r.Value);
                max = vals.First().Value > max ? vals.First().Value : max;
            }
            double kedu =Math.Ceiling( max / 0.95);
            int ct = 1;
            double dis = 8 * 1e-3 * mapScale;//一组分类数据高度
            double dataval = 5 * 1e-3 * mapScale;//数据间间隔
            string geoRelate = frm.GeoLayer;
            Dictionary<string, IPoint> namesPt = ChartsDataSource.ObtainGeoRelated(geoRelate);
            foreach (var kv in groupdatas)
            {

                eles.Clear();
                //IPoint centerpoint = (_centerpoint as IClone).Clone() as IPoint;
                IPoint centerpoint = new PointClass() { X = 0, Y = 0 };
                //由大到小排序
                var staticsData = kv.Value.OrderBy(tt => tt.Value).ToDictionary(k => k.Key, v => v.Value);
                DrawBgEle(centerpoint, staticsData.Count);
                string name = kv.Key;
                if (geoRelate != "")
                {
                    if (namesPt.ContainsKey(name))
                        _centerpoint = namesPt[name];
                }
                DrawColumns(centerpoint, staticsData, max, cmykColors);

                
                //IPoint shapPt = (centerpoint as IClone).Clone() as IPoint;
                int obj = 0;
                ChartsToRepHelper ch = new ChartsToRepHelper();
                var remaker = ch.CreateFeatureEX(pAc, eles, pRepLayer, _centerpoint, out obj, markerSize);
                createAnno(remaker, _centerpoint, markerSize, chartTitle, obj);
                //gh.CreateFeatures(eles, pRepLayer, shapPt, markerSize);
               
            }
        }
        public void CreateSingleBarUnGeo(IPoint _centerpoint, FrmSingleBarChartsSet frm)
        {
            double max = 0;
            //获取设置
            IColor cmykColors = frm.CMYKColors;
            var groupdatas = frm.ChartDatas;
            foreach (var kv in groupdatas)
            {
                var vals = kv.Value.OrderByDescending(r => r.Value);
                max = vals.First().Value > max ? vals.First().Value : max;
                break;
            }
            double kedu =Math.Ceiling( max / 0.95);
            
            double dis = 8 * 1e-3 * mapScale;//一组分类数据高度
            double dataval = 5 * 1e-3 * mapScale;//数据间间隔
           
            
            foreach (var kv in groupdatas)
            {

                eles.Clear();
                //IPoint centerpoint = (_centerpoint as IClone).Clone() as IPoint;
                IPoint centerpoint = new PointClass() { X = 0, Y = 0 };
                //由大到小排序
                var staticsData = kv.Value.OrderBy(tt => tt.Value).ToDictionary(k => k.Key, v => v.Value);
                DrawBgEle(centerpoint, staticsData.Count);
                 
                DrawColumns(centerpoint, staticsData, max, cmykColors);

                double step =3e-2 * mapScale;
                IPoint shapPt = (centerpoint as IClone).Clone() as IPoint;

                //gh.CreateFeatures(eles, pRepLayer, shapPt, markerSize);
                int obj = 0;
                ChartsToRepHelper ch = new ChartsToRepHelper();
                var remaker = ch.CreateFeatureEX(pAc, eles, pRepLayer, _centerpoint, out obj, markerSize);
                createAnno(remaker, _centerpoint, markerSize, chartTitle, obj);
                //_centerpoint.Y -= step;
                break;

            }
        }
        #region

        private void DrawBgEle(IPoint centerPt,int ct)
        {
            double diswith = 10 * 1e-3 * mapScale;//一组分类数据宽度
            double width =  10 * diswith * 2;
            double dis = 7 * 1e-3 * mapScale;//一组分类数据高度
            double dataval = 3 * 1e-3 * mapScale;//数据间间隔
            double height = ct * (dis + dataval);
            IPolygon columngeo = gh.CreateRectangle(centerPt, width, -height);
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
            pEl.Geometry = columngeo as IGeometry;
            eles.Add(pEl);
            
        }
        //获取数据
        private Dictionary<string, double> getStaticDatas(ref double max)
        {    
            List<Dictionary<string, double>> datas = new List<Dictionary<string, double>>();
            max = 0;
            Random random = new Random();
            
            Dictionary<string, double> dicvals = new Dictionary<string, double>();
               
            dicvals["制造业"] = random.Next(20, 150);
            dicvals["建筑业"] = random.Next(1, 110);
            dicvals["批发零售业"] = random.Next(10, 150);
            dicvals["租赁服务业"] = random.Next(30, 150);
            dicvals["房地产业"] = random.Next(20, 150);
            dicvals["采矿业"] = random.Next(1, 110);
            dicvals["住宿餐饮业"] = random.Next(10, 150);
            dicvals["交通运输业、仓储和邮政业"] = random.Next(30, 150);
            dicvals["电力、热力、水生产和供应业"] = random.Next(20, 150);
            dicvals["科学研究技术服务业"] = random.Next(1, 110);
            dicvals["文化体育业"] = random.Next(10, 150);
            dicvals["软件技术"] = random.Next(30, 150);
            dicvals["水利、环境业"] = random.Next(30, 150);
            var vals = dicvals.OrderByDescending(r => r.Value);

            max = vals.First().Value > max ? vals.First().Value : max;
               
            datas.Add(dicvals);
            return dicvals;
        }
        bool xyAxis = false;
     

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
        /// <summary>
        /// 绘制xy轴
        /// </summary>
        //x轴文字刻度
        //Dictionary<IPoint, string> xKeDu = new Dictionary<IPoint, string>();
        private void DrawXYaxis(IPoint pBasePoint,int groups,double kd)
        {

            double total = 7 * 1e-3 * mapScale;
            double dis = 10 * 1e-3 * mapScale;
            //X轴
            IPoint xpoint = new PointClass() { X = pBasePoint.X + 10 * dis * 2, Y = pBasePoint.Y };
            IPoint xpoint1 = new PointClass() { X = pBasePoint.X -dis/10*2, Y = pBasePoint.Y };
            IGeometry pline = ContructPolyLine(xpoint1, xpoint);
            DrawLine(pline as IPolyline);
            //X轴 格网+刻度
            for (int i = 1; i <= 10; i++)
            {
                IPoint point = new PointClass() { X = pBasePoint.X + i * dis*2, Y = pBasePoint.Y-dis/10*2 };
                IPoint point1 = new PointClass() { X = pBasePoint.X + i * dis * 2, Y = pBasePoint.Y + groups * dis };
                IGeometry line = ContructPolyLine(point, point1);
                DrawLine(line as IPolyline);
              
                double txtval = (i* kd/10.0);
                if (kd/10.0 >=1)
                {
                    txtval = Math.Floor(txtval);
                }
                else if (kd / 10.0 < 1 && kd / 10.0 >= 0.1)
                {
                    txtval = Math.Round(txtval, 2);

                }
                else if (kd / 10.0 < 0.1 && kd / 10.0 >= 0.01)
                {
                    txtval = Math.Round(txtval,4);
                }
                
                string txt = (txtval).ToString();
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(pBasePoint.X + i * dis*2, pBasePoint.Y - 8*dis / 10);
                double cx = (txtpoint.X - pBasePoint.X) / total;
                double cy = (txtpoint.Y - pBasePoint.Y) / total;
                IPoint pt = new PointClass() { X = cx, Y = cy };
                //xKeDu.Add(pt, txt);

                //DrawTxt(txtpoint, txt, 15);
            }
            //Y轴刻度
            for (int i = 1; i <= groups; i++)
            {
                IPoint point = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + i * dis };
                IPoint point1 = new PointClass() { X = pBasePoint.X - dis / 10 * 2, Y = pBasePoint.Y + i * dis };
                IGeometry line = ContructPolyLine(point, point1);
                 DrawLine(line as IPolyline);
            }
          
          
        }
       
        ///绘制柱  
        //注记数量和类别文字注记获取
        Dictionary<IPoint, string> annoNum = new Dictionary<IPoint, string>();
        Dictionary<IPoint, string> annoClass = new Dictionary<IPoint, string>();
        private void DrawColumns(IPoint pBasePoint, Dictionary<string, double> datas, double max, IColor cmykColors)
        {
            annoNum.Clear();
            annoClass.Clear();
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double diswith = 10 * 1e-3 * mapScale;//一组分类数据宽度
            double dis = 7* 1e-3 * mapScale;//一组分类数据高度
            double dataval = ColumnStep*dis;//数据间间隔
            int groups = datas.Count;
            int i = 0;
            double total = dis*datas.Count;
            double cx = 0;
            double cy = 0;
            foreach (var kv in datas)
            {

                IPoint point = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + 1.5e-3 * mapScale + i * (dis + dataval) };
                double width = kv.Value / max * 10 * diswith*2 * 0.95;
                if (width < 50)
                    width = 50;
                IColor pcolor = (cmykColors as IClone).Clone() as IColor;
                //正面
                double diaHeight = 15;
                IPolygon rectange = gh.CreateRectangle(point, width, -dis);
                var ele = DrawCubePolygon(rectange, pcolor);
                eles.Add(ele);
                IPoint leftdown = new PointClass() { X = point.X, Y = point.Y + dis };
                //顶
                IColor pcolor1 = (pcolor as IClone).Clone() as IColor;
                (pcolor1 as ICmykColor).Black += +15;
                IPolygon diamond = gh.ConstructTopDiamond(leftdown, width, diaHeight);
                ele = DrawCubePolygon(diamond, pcolor1);
                eles.Add(ele);
                IPoint temp = new PointClass() { X = point.X + width, Y = point.Y };
                //侧面
                IColor pcolor2 = (pcolor as IClone).Clone() as IColor;
                (pcolor2 as ICmykColor).Black += 25;
                IPolygon diamond1 = gh.ConstructSideDiamond(temp, diaHeight, dis);
                ele = DrawCubePolygon(diamond1, pcolor2);
                eles.Add(ele);
                //数量注记
                IPoint labelpoint = new PointClass();
                double strlen = gh.GetStrWidth(kv.Value.ToString(),mapScale,20);
                labelpoint.PutCoords(pBasePoint.X + width + 0.5*strlen, point.Y + dis * 0.5);
                
                cx = (labelpoint.X - pBasePoint.X) / total;
                cy = (labelpoint.Y - pBasePoint.Y) / total;
                IPoint pt = new PointClass() { X = cx, Y = cy };
                annoNum.Add(pt, kv.Value.ToString());
                //DrawTxt(labelpoint, kv.Value.ToString(), 20);
                //类别注记
                double txtunit = 3.97427;//1号1万字体和宽度
                //strlen = gh.GetStrLen(kv.Key.Trim());
                IPoint typePoint = new PointClass();
                typePoint.PutCoords(pBasePoint.X - dis * 0.5, point.Y + dis * 0.5);

                cx = (typePoint.X - pBasePoint.X) / total;
                cy = (typePoint.Y - pBasePoint.Y) / total;
                IPoint pt2 = new PointClass() { X = cx, Y = cy };
                annoClass.Add(pt2, kv.Key.ToString());
                //DrawTxt(typePoint, kv.Key.ToString(), 20);
                i++;
                  
            }
            //标题注记
            //IPoint titlePoint = new PointClass();
            //titlePoint.PutCoords(pBasePoint.X + datas.Last().Value / max * 10 * diswith  * 0.95, pBasePoint.Y + groups * diswith + diswith);
            //if (chartTitle != "")
            //{

            //    cx = (titlePoint.X - pBasePoint.X) / total;
            //    cy = (titlePoint.Y - pBasePoint.Y) / total;
            //    pt.PutCoords(cx, cy);
            //    anno.Add(pt, chartTitle);
            //    //DrawTxt(titlePoint, chartTitle, 30);
            //}
            //Y轴
            if (xyAxis)
            {
                IPoint p1 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y - dis / 10 };
                IPoint p2 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + groups * diswith };
                IGeometry line1 = ContructPolyLine(p1, p2);
                DrawLine(line1 as IPolyline);
            }
        }
        private void DrawPolygon(IGeometry pgeo,IColor pcolor)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            smsymbol.Color = pcolor;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();
            smline.Color = pcolor;
            smline.Width = 0.0025;
            smsymbol.Outline = smline;

            polygonElement.Symbol = smsymbol;
    
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
            eles.Add(pEl);
        }
        private void DrawLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 0.0025;
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
                Name = "宋体",
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
                eles.Add(pEl);
                return pEl;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        private void createAnno(IRepresentationMarker RepMarker, IPoint center, double markersize, string charttile, int id)
        {

            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
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
            double total = ((70 * annoClass.Count) / RepMarker.Height) * height;
            double df = (70 / RepMarker.Height) * height*0.5;
            double fontsize = df;
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);
            fontsize *= 2.83;

            double x = 0;
            double y = 0;
            
            ////y刻度
            //foreach (var k in xKeDu)
            //{
            //    x = k.Key.X * width + center.X;
            //    y = k.Key.Y * width + center.Y;
            //    pt.PutCoords(x, y);
            //    InsertAnnoFea(pt, k.Value, fontsize * 0.6, id);
            //}
            //数量标注
            foreach (var k in annoNum)
            {
                double strwidth = gh.GetStrWidth(k.Value, curms, fontsize * 0.5);
                x = k.Key.X * total + center.X + strwidth / 2.0;
                y = k.Key.Y * total + center.Y;
                IPoint pt = new PointClass() { X = x, Y = y };
                InsertAnnoFea(pt, k.Value, fontsize * 0.5, id);
            }
            ////图例
            //foreach (var k in annolg)
            //{
            //    x = k.Key.X * Xaxis + center.X;
            //    y = k.Key.Y * Xaxis + center.Y;
            //    pt.PutCoords(x, y);
            //    InsertAnnoFea(pt, k.Value, fontsize * 0.4, id);
            //}
            //类别
            foreach (var k in annoClass)
            {
                double strwidth = gh.GetStrWidth(k.Value, curms, fontsize * 0.8);
                System.Diagnostics.Debug.WriteLine(strwidth);
                x = k.Key.X * total + center.X;
                y = k.Key.Y * total + center.Y;
                IPoint pt = new PointClass() { X = x, Y = y };
                InsertAnnoFea(pt, k.Value, fontsize * 0.8, id);
            }
            //标题
            if (charttile != "")
            {
                x = width / 2 + center.X;
                y = height + center.Y + height / 5;
                IPoint pt = new PointClass() { X = x, Y = y };
                InsertAnnoFea(pt, charttile, fontsize * 1.3, id);
            }
        }

        private void InsertAnnoFea(IGeometry pGeometry, string annoName, double fontSize, int id)
        {
            try
            {
                IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
                ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize);
                IElement pElement = pTextElement as IElement;
                IFeature pFeature = annoFcl.CreateFeature();
                IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
                pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID;
                pAnnoFeature.LinkedFeatureID = id;
                pAnnoFeature.Annotation = pElement;
                pFeature.Store();
            }
            catch
            { }
            //return true;
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
                Size = size,
                VerticalAlignment = esriTextVerticalAlignment.esriTVACenter,
                HorizontalAlignment = esriTextHorizontalAlignment.esriTHARight
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

        private IElement DrawCubePolygon(IGeometry geo, IColor pcolor)
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
    }
}

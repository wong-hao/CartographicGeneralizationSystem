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
 
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using stdole;
using System.Collections.Generic;
using System.Linq;
using SMGI.Common;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
   
    public sealed class DrawChartsLine
    {
        
        private IActiveView pAc = null;
        private double mapScale = 1000;
        //private double mapScale2 = 0;
        private ILayer pRepLayer = null;
        private IFeatureClass annoFcl = null;
        GraphicsHelper gh = null;
        double markerSize = 20;
        string  markerType = "";
        bool markerTag = false;
        private List<IElement> eles = new List<IElement>();
        private List<ICmykColor> cmykColors = new List<ICmykColor>();
        private double Ykedu = 0;
        private double angle = 0;
        private double Min = 0;
        //饼状散图
        public DrawChartsLine(IActiveView _pAc, double _mapScale)
        {
            pAc = _pAc;
            //mapScale2 = _mapScale;
            gh = new GraphicsHelper(pAc);
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pRepLayer = lyrs.First();

            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer ) && ((l as IFeatureLayer).FeatureClass.AliasName == "LANNO");
            })).ToArray();
           annoFcl= (lyr.First() as IFeatureLayer).FeatureClass;
        }
        //公开调用函数
        public void DrawBrokenLine(IPoint _centerPoint)
        {
            IPoint centerpoint = _centerPoint;
            double max=0;
            FrmLineChartsSet frm = new FrmLineChartsSet();
            DialogResult dr= frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            eles.Clear();
            //获取数据
            var groupdatas = frm.ChartDatas;
           
            //获取颜色
            int nums = groupdatas.First().Value.Count;
            getStaticDatas(groupdatas, ref max);
            markerType = frm.GeoDataTagName;
            markerTag = frm.GeoDataTag;
            cmykColors = frm.CMYKColors;
            markerSize = frm.MarkerSize;
            Ykedu = frm.KeDu;
            angle = frm.TxtAngle;
            double sum = Ykedu * 10;
            Min = frm.min;
            IPoint orpt = new PointClass() { X = 0, Y = 0 };
            if (frm.XYAxis)
            {
                DrawXYaxis(orpt, groupdatas, nums, Ykedu, Min);
            }
            DrawPoint(orpt, groupdatas, sum, cmykColors);
            if (frm.GeoLengend)
            {
                DrawLengend(orpt, groupdatas.Keys.ToArray(), groupdatas.First().Value.Count, groupdatas.Count);

            } string title = frm.ChartTitle;
            if (title != "")
            {
                DrawTitle(orpt, title, nums);
            }
            int obj = 0;
            ChartsToRepHelper ch = new ChartsToRepHelper();
            //创建白色图表背景
            if (!frm.IsTransparent)
            {
                CreateWhiteBackGround(eles);
            }
            var remarker = ch.CreateFeatureEX(pAc, eles, pRepLayer, centerpoint, out obj, markerSize);
            //gh.CreateFeatures(eles, pRepLayer, centerpoint,markerSize);
            CreateAnnotion(remarker, centerpoint, markerSize, groupdatas, obj);
            pAc.Refresh();
            MessageBox.Show("生成完成");
        }
        #region
        Dictionary<string, IPoint> annoTitle = new Dictionary<string, IPoint>();
        private void DrawTitle(IPoint basepoint, string title, int nums)
        {
            annoTitle.Clear();
            IPoint pt = new PointClass();
            double dis = 20 * 1e-3 * mapScale;
            double total = dis * 10;
            double xdistance = (2 * nums) * dis;//X轴长度
            pt.X = basepoint.X + xdistance / 2;
            pt.Y = basepoint.Y + 7 * dis + 10e-3 * mapScale;
            IPoint p = new PointClass();
            double cx = (basepoint.X - pt.X) / total;//负
            double cy = (basepoint.Y - pt.X) / total;//负
            p.PutCoords(cx, cy);
            annoTitle.Add(title, p);
            //DrawTxt(pt, title, 40);
        }
        //获取图例标注
        Dictionary<string, IPoint> annolg = new Dictionary<string, IPoint>();
        private void DrawLengend(IPoint _basepoint, string[] subtypes, double columns, double groups)
        {
            annolg.Clear();
            double dis = 20  * 1e-3 * mapScale;//一组分类数据长度
            double total = dis * 10; ;
            IPoint basepoint = (_basepoint as IClone).Clone() as IPoint;
            basepoint.Y -= 15e-3 * mapScale;
            //每一个柱子基本宽度
            double basewidth = 5e-3 * mapScale;

            double xdistance = (2 * columns + 1) * dis;//X轴长度

            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取图列的高度
            double heightunit = 3.97427;//1号字体1万的高度
            double fontsize = 30;
            double lgheight = heightunit * fontsize * mapScale / 10000;
            int ct = subtypes.Length;
            //全部图例+文字长度
            double lgwidth = lgheight *3;
            double length = lgwidth * ct;//图例长度
            
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
                IPoint fp = new PointClass() { X = upleft.X, Y = upleft.Y - lgheight / 2 };
                IPoint tp = new PointClass() { X = upleft.X+lgwidth, Y = upleft.Y - lgheight / 2 };
                IPolyline pline = gh.ContructPolyLine(fp, tp);
                DrawLine1(pline, cmykColors[i]);
                IPoint cp = new PointClass() { X = upleft.X + lgwidth/2, Y = upleft.Y - lgheight / 2 };
                if (markerType != "" && markerTag)
                {
                    DrawMarker(cp, cmykColors[i]);
                }
                stepX += lgwidth;
               
                double strwidth = gh.GetStrWidth(subtypes[i], mapScale, fontsize);
                stepX += txtinterval;//文字间隔
                IPoint txtpoint = new PointClass() { X = txtinterval + upleft.X  + lgwidth, Y = upleft.Y - 0.8 * lgheight };
                //var ele = gh.DrawTxt(txtpoint, subtypes[i], fontsize);
                IPoint pt=new PointClass();
                double cx = (txtpoint.X - _basepoint.X) / total;
                double cy = (txtpoint.Y - _basepoint.X) / total;
                pt.PutCoords(cx,cy);
                annolg.Add(subtypes[i], pt);
                stepX += strwidth + lginterval;//文字距离+图例间隔
                //eles.Add(ele);
 
            }
        }
        private void getStaticDatas( Dictionary<string, Dictionary<string, double>> ChartDatas, ref double max)
        {    
         
            max = 0;
           
            foreach (var kv in ChartDatas)
            {
                Dictionary<string, double> dicvals = kv.Value;
                var vals = dicvals.OrderByDescending(r => r.Value);

                max = vals.First().Value > max ? vals.First().Value : max;
            }
            
        }
        private void DrawPoint(IPoint pBasePoint, Dictionary<string, Dictionary<string, double>> datas, double max, List<ICmykColor> cmykColors)
        {
            double dis = 20 * 1e-3 * mapScale;//一组分类数据长度
            int groups = datas.Count;
            double total = dis * 10;
            int i = 0;
            foreach(var linedata in datas)
            {
                var lists = linedata.Value;
                int j = 0;
                List<IPoint> points = new List<IPoint>();
                foreach (var kv in lists)
                {
                    //double width = kv.Value / max * 10 * dis * 0.95;
                    double width = kv.Value / max * total;
                    IPoint point = new PointClass() { X = pBasePoint.X + (2*j + 1) * dis, Y = pBasePoint.Y + width };
                    points.Add(point);
                    j++;
                }
                //绘制点
                j = 0;
                if (markerTag)
                {
                    foreach (var p in points)
                    {
                        DrawMarker(p, cmykColors[i]);
                    }
                }
                //绘制线
                IPolyline line = ContructPolyLine1(points);
                DrawLine1(line, cmykColors[i]);
                i++;
            }
        }
        private void DrawMarker(IGeometry pgeo,IColor pcolor)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            ISimpleMarkerSymbol sym = new SimpleMarkerSymbolClass();
            if (markerType == "正方形")
            {
                sym.Style = esriSimpleMarkerStyle.esriSMSSquare;
            }
            else if (markerType == "菱形")
            {
                sym.Style = esriSimpleMarkerStyle.esriSMSDiamond;
            }
            else if (markerType == "圆形")
            {
                sym.Style = esriSimpleMarkerStyle.esriSMSCircle;
            }
            double size = 5;
            //sym.Size = 12;
            sym.Size = size;
            sym.Color = pcolor;
            IMarkerElement pmarkerEle = new MarkerElementClass();
            pmarkerEle.Symbol = sym;
            pEl = pmarkerEle as IElement;
            pEl.Geometry = pgeo as IGeometry;
            eles.Add(pEl); 

        }
        //获取x,y轴的标注
        Dictionary<string, IPoint> annox = new Dictionary<string, IPoint>();
        Dictionary<string, IPoint> annoy = new Dictionary<string, IPoint>();
        private void DrawXYaxis(IPoint pBasePoint,Dictionary<string, Dictionary<string, double>> datas, int nums,double kd, double min)
        {
            annox.Clear();
            annoy.Clear();
            double dis = 20 * 1e-3 * mapScale;//一组分类数据长度
            double total = 20 * 1e-2 * mapScale;
            int j = 0;
            foreach(var kv in datas.First().Value)
            {
                string type = kv.Key;
                IPoint point = new PointClass() { X = pBasePoint.X + (2 * j + 1) * dis, Y = pBasePoint.Y - 10e-3 * mapScale };
                IPoint pt = new PointClass();
                double cx = (pBasePoint.X - point.X) / total;//占y轴的比例（为负）
                double cy = (pBasePoint.Y - point.Y) / total;//占y轴的比例（为正）
                pt.PutCoords(cx,cy);
                annox.Add(type, pt);
                //DrawTxt(point, type, 18);
                j++;
                IPoint f = new PointClass() { X = pBasePoint.X + (2 * j) * dis, Y = pBasePoint.Y };
                IPoint t = new PointClass() { X = pBasePoint.X + (2 * j) * dis, Y = pBasePoint.Y - 2e-3 * mapScale };
                IGeometry xpline = ContructPolyLine(f, t);
                DrawLine(xpline as IPolyline);
            }

            dis = 40 * 1e-3 * mapScale;
            //num为有多少行有效数据
            //X轴
            IPoint xpoint = new PointClass() { X = pBasePoint.X + nums * dis, Y = pBasePoint.Y };
            IPoint xpoint1 = new PointClass() { X = pBasePoint.X - dis / 10 / 2, Y = pBasePoint.Y };
            IGeometry pline = ContructPolyLine(xpoint1, xpoint);
            DrawLine(pline as IPolyline);
            //X轴及刻度值
            for (int i = 0; i <= 10; i++)
            {
                IPoint point = new PointClass() { X = pBasePoint.X + nums * dis, Y = pBasePoint.Y +i* dis / 2 };
                IPoint point1 = new PointClass() { X = pBasePoint.X - dis / 10 / 2, Y = pBasePoint.Y + i * dis / 2 };
                IGeometry line = ContructPolyLine(point, point1);
                DrawLine(line as IPolyline);
                //double txtval = kd * i + min;
                //string txt = (kd * i + min).ToString();
                string txt = (kd * i ).ToString();
                double txtwidth=  gh.GetStrWidth(txt, mapScale, 25);
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(pBasePoint.X - dis / 10 / 2 - 2*1e-3 * mapScale-txtwidth/2, pBasePoint.Y + i * dis / 2);
                IPoint pt = new PointClass();
                double cx = (pBasePoint.X - txtpoint.X) / total;//x占总的y轴的比例(比例值为负)
                double cy = (pBasePoint.Y - txtpoint.Y) / total;//y占总的y轴的比例(比例值为负)
                pt.PutCoords (cx,cy);
                annoy.Add(txt, pt);
                //DrawTxt(txtpoint, txt, 25);
            }
            //Y轴刻度
            
            IPoint p1 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + 10 * dis/2 };
            IPoint p2 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y - dis / 10 / 2 };
            IGeometry line1 = ContructPolyLine(p1, p2);
            DrawLine(line1 as IPolyline);
            

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
                IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                eles.Add(pEl); 
                return pEl;
            }
            catch
            {
                return null;
            }
        }
        private void DrawLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 1;
               
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
        private void DrawLine1(IPolyline pline,IColor pcolor)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 2;
                linesym.Color = pcolor;
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
        private IPolyline ContructPolyLine1(List<IPoint> ps)
        {
            try
            {
                IGeometryCollection pPolyline = new PolylineClass();
                IPointCollection pCl = new PathClass();
                foreach (IPoint p in ps)
                {
                    pCl.AddPoint(p);
                }
              
              
                pPolyline.AddGeometry(pCl as IGeometry);
                (pPolyline as ITopologicalOperator).Simplify();
                return pPolyline as IPolyline;
            }
            catch (Exception ex)
            {
                return null;
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
                pAnnoFeature.AnnotationClassID = pFeature.OID;
                pAnnoFeature.LinkedFeatureID = id;
                pAnnoFeature.Annotation = pElement;
                pFeature.Store();
            }
            catch
            { 
            }
            //return true;
        }
        //文字注记
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
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pAnnoFeature.Annotation = ele;
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

        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, Dictionary<string, Dictionary<string, double>> datas, int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double Xaxis;
            double Yaxis;
            double size = curms * 1.0e-3 * markersize;
            int column = datas.First().Value.Count;
            //1.确定折线图长度和宽度
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
            Yaxis = (202-2.02) / (226.461) * height;
            Xaxis = Yaxis * 0.1 * column * 2;
            //
            double dy = Yaxis / 10;
            double fontsize = (dy / 5);
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms* heightunit * 1e-4);
            fontsize *= 2.83;
            //3.计算X轴文字坐标以及文字大小
            int j = 0;
            double dx = Xaxis / column;
            foreach (var kv in annox)
            {
                string key = kv.Key;
                var point = new PointClass();
                //point.X = pAnchor.X - kv.Value.X * Yaxis;
                //point.Y = pAnchor.X - kv.Value.Y * Yaxis;
                point.X = pAnchor.X + dx / 2 + j * dx;
                point.Y = pAnchor.Y - dx / 10 - fontsize * (curms * heightunit * 1e-4);
                InsertAnnoFeaTXT(point, key, fontsize, id);
                //InsertAnnoFea(point, key, fontsize, id);
                j++;

            }
            //4.计算y轴文字坐标以及文字大小
            j = 0;
            foreach (var k in annoy)
            {
                var point = new PointClass();
                //point.Y = pAnchor.Y - k.Value.Y * Yaxis;
                var txtwidth = gh.GetStrWidth(k.Key, curms, fontsize);
                point.X = pAnchor.X - dy / 10 - txtwidth / 2;
                //point.X = pAnchor.X - dy / 2;
                //var txtwidth = gh.GetStrWidth(Ykedu[i], curms, fontsize);
                //point.X -= txtwidth / 2;
                point.Y = pAnchor.Y + j * dy;

                InsertAnnoFea(point, k.Key, fontsize, id);
                j++;
            }
            //5.计算图例文字坐标以及文字大小
            foreach (var kv in annolg)
            {
                double strwidth = gh.GetStrWidth(kv.Key, curms, fontsize * 0.8);
                string s = kv.Key;
                double x = kv.Value.X;
                x = x * Yaxis + strwidth / 2.0;
                double y = kv.Value.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                InsertAnnoFea(p, s, fontsize * 0.8, id);
            }
            //6.计算统计标注
            //foreach (var kv in lbpt)
            //{
            //    double x = kv.Key.X;
            //    x = x * Yaxis;
            //    double y = kv.Key.Y;
            //    y = y * Yaxis;

            //    var p = new PointClass();
            //    p.X = pAnchor.X + x;
            //    p.Y = pAnchor.Y + y;

            //    InsertAnnoFea(p, kv.Value, fontsize * 0.6, id);
            //}
            //7.绘制标题
            foreach (var k in annoTitle)
            {
                IPoint pt = new PointClass { X = pAnchor.X + Xaxis / 2, Y = pAnchor.Y + Yaxis + Yaxis / 10 };
                //IPoint pt = new PointClass() { X = pAnchor.X - k.Value.X * Yaxis, Y = pAnchor.Y - k.Value.Y * Yaxis };
                InsertAnnoFea(pt, k.Key, fontsize * 1.5, id);
            }
            //if (chartTitle != "")
            //{
            //    IPoint pt = new PointClass { X = pAnchor.X + Xaxis / 2, Y = pAnchor.Y + Yaxis + Yaxis / 10 };
            //    InsertAnnoFea(pt, chartTitle, fontsize * 1.5, id);
            //}
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
        #endregion
    }
}

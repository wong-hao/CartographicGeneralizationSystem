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
using SMGI.Common;
using System.Collections.Generic;
using System.Linq;

namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public sealed class Draw3DCircelPie2
    {
       private IActiveView pAc = null;
        private List<IElement> eles = new List<IElement>();
        private ILayer pRepLayer = null;
        ILayer annoly = null;
        private IFeatureClass annoFcl = null;
        public static Dictionary<IPoint, string> annoTxt = new Dictionary<IPoint, string>();//文字注记
        public static Dictionary<IPoint, string> staticTxt = new Dictionary<IPoint, string>();//统计数据注记
        public static Dictionary<int, IPoint> staticPoints = new Dictionary<int, IPoint>();//统计几何
     
        double TOTAL;//总比值
        double lgVal;//图例间隔
        GraphicsHelper gh = null;
        double mapScale = 1000;
        private double linewidth = 0.283;
    
        List<IPoint> arcPoint = new List<IPoint>();//每个弧注记中点
        List<double> centerAngle = new List<double>();//每个弧中点角度
        List<double> percentage = new List<double>();//每个弧比例
        List<ICmykColor> cmykColors = null;//每个部分的颜色
        bool enablePieVals = false;
        string noteLableType = "";
        double markerSize = 20;
        double markerSizeMax = 20;

        private string chartName = "三维圆饼图";
        private string chartType = "饼图";
        double ridia = 0;//圆半径
        IPoint centerpoint = new PointClass() { X = 0, Y = 0 };
        IPoint _centerpoint = null;
        IPolygon pEllipseClone = null;//圆形
        IGeometry pEllipseRing = null;//圆环    
        double total = 0;//总数
        double moveR = 0;//平移的距离R
        string labeType = string.Empty;
        public Draw3DCircelPie2(IActiveView pac, double ms)
        {
            pAc = pac;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pRepLayer = lyrs.First();

            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LANNO");
            })).ToArray();
            annoly = lyr.First();
            annoFcl = (annoly as IFeatureLayer).FeatureClass;

            gh = new GraphicsHelper(pAc);
        }
        //绘制3D圆图      
        public void Draw3DCirclePie(FrmPieChartsSet frm,IPoint anchorpoint, double width = 5)
        {
            CommonMethods.ClearThematicCarto(anchorpoint, (pRepLayer as IFeatureLayer).FeatureClass, annoFcl);
            IntialData();
            IPolygon pEllipse = null;
            ITopologicalOperator pTo = null;
          
            GraphicsHelper gh = new GraphicsHelper(pAc);
            _centerpoint = anchorpoint;
            //获取数据        

            //获取颜色等设置
            string labetype = frm.LableType;
            labeType = labetype;
            noteLableType = labetype;
            string chartTitle = frm.ChartTitle;
            cmykColors = frm.CMYKColors;
            enablePieVals = frm.EnablePieVals;
            markerSize = frm.MarkerSize;
            markerSizeMax = frm.MarkerSizeMax;
            string geoRelate = frm.GeoLayer;
            var multiDatas = frm.ChartDatas;
            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("正在处理...");
            Dictionary<string, IPoint> namesPt = null;
            if (geoRelate != "")
            {
                namesPt = ChartsDataSource.ObtainGeoRelated(geoRelate);
            }


            // start 计算饼图大小
            var sumDatalist = getMaxStaticData(multiDatas);
            double fxa = 0;
            if ((sumDatalist.Max() - sumDatalist.Min()) != 0)
            {
                fxa = (markerSizeMax - markerSize) / (sumDatalist.Max() - sumDatalist.Min());
            }
            double fxb = 0;
            fxb = fxa * sumDatalist.Min() + markerSize;
            int flagNum = 0;
            // end 计算饼图大小
            foreach (var kvCharts in multiDatas)
            {
                eles.Clear(); arcPoint.Clear(); centerAngle.Clear(); percentage.Clear();
                #region
                string name = kvCharts.Key;
                if (geoRelate != "")
                {
                    if (namesPt.ContainsKey(name))
                        _centerpoint = namesPt[name];
                }
                Dictionary<string, double> dicvals = kvCharts.Value;
                total=   getStaticData(dicvals);

                // 构造椭圆
                double a = 40 * mapScale / 1000;
                pEllipse = gh.ConsturctEllipse(centerpoint, a, a);
                pEllipseClone = (pEllipse as IClone).Clone() as IPolygon;
                pTo = pEllipse as ITopologicalOperator;
                pTo.Simplify();
                // 构造椭圆环
                pEllipseRing = ContrustEllipseRing(pEllipse, width);
                List<IPoint> paths = new List<IPoint>();
                List<IPoint> circelpaths = new List<IPoint>();
                List<IPoint> circelpaths1 = new List<IPoint>();
                //创建切割点
                CreateClipPoint(a, dicvals, ref paths, ref circelpaths, ref circelpaths1);
                IGeometryCollection pPolyline = new PolylineClass();
                IPointCollection pCl = new PathClass();
                IGeometry pleftGeo = null;
                IGeometry prightGeo = null;
                #region 创建扇形：绘制
                int ct = 0; double step = 0;  flagNum = 0;//最后一个环
                List<IPoint> movePoints = new List<IPoint>();
                foreach (var kv in dicvals)
                {
                    pPolyline = new PolylineClass();
                    pCl = new PathClass();
                    IPoint point1 = paths[ct];
                    IPoint point2 = paths[ct + 1];
                    pCl.AddPoint(point1);
                    pCl.AddPoint(centerpoint);
                    pCl.AddPoint(point2);
                    pPolyline.AddGeometry(pCl as IGeometry);
                    pleftGeo = null;
                    prightGeo = null;
                    pTo.Cut(pPolyline as IPolyline, out pleftGeo, out prightGeo);
                    double angle = step / total * Math.PI * 2;
                    angle += kv.Value / total * Math.PI;
                    step += kv.Value;
                    IPoint movepoint = getPoint(angle, moveR, centerpoint);
                    movePoints.Add(movepoint);
                    //绘制边缘
                    CreateFansEdge(centerpoint, circelpaths[ct], movepoint, cmykColors[ct], circelpaths1[ct]);
                    CreateFansEdge(centerpoint, circelpaths[ct + 1], movepoint, cmykColors[ct], circelpaths1[ct + 1]);
                    double sx = movepoint.X - centerpoint.X;
                    double sy = movepoint.Y - centerpoint.Y;
                   
                    //绘制扇形
                    DrawFans(pleftGeo, cmykColors[ct], sx, sy);
                    staticPoints[ct] = new PointClass { X = sx + (pleftGeo as IArea).Centroid.X, Y = sy + (pleftGeo as IArea).Centroid.Y };
                    //绘制下圆环
                    IGeometry pgeoring = CutEllipseRing(centerpoint, point2, ref pEllipseRing);
                    if (pgeoring != null)
                    {
                        ICmykColor pcmyk = (cmykColors[ct] as IClone).Clone() as ICmykColor;
                        pcmyk.Black += 15;
                        DrawFans(pgeoring, pcmyk, sx, sy);
                        flagNum = ct;
                    }
                  
                    ct++;
                }
                //绘制下圆环
                if (!pEllipseRing.IsEmpty)
                {
                    flagNum++;

                    double dx = movePoints[flagNum].X - centerpoint.X;
                    double dy = movePoints[flagNum].Y - centerpoint.Y;
                    ICmykColor pcmyk = (cmykColors[flagNum] as IClone).Clone() as ICmykColor;
                    pcmyk.Black += 15;
                   
                    DrawFans(pEllipseRing, pcmyk, dx, dy);
                }
                
                ct = 0;
                //绘制标注
                annoTxt.Clear(); staticTxt.Clear();
                double totalheight = pEllipse.Envelope.Height + width;
                double lgheight = totalheight / (dicvals.Count + (dicvals.Count - 1) * 0.8);
                TOTAL = (dicvals.Count - 1) * 1.8 * lgheight;
                lgVal = 1.8 * lgheight;

                if (labetype == "图例式标注")
                {
                    DrawPieNoteLengend(dicvals,width);
                }
                else if (labetype == "引线式标注")
                {
                    DrawPieNoteLine(dicvals);
                    
                }
                else if (labetype == "压盖式标注")
                {
                    DrawPieNoteOverlap(dicvals);
                }

                //获取统计数据
                string stics = "";
                if (enablePieVals)
                {
                    stics = DrawStaticPieVals(dicvals);
                }

                double piesize = total * fxa + fxb;
                var piedata = new Dictionary<string, Dictionary<string, double>>();
                piedata[kvCharts.Key] = kvCharts.Value;
                string jsdata = JsonHelper.JsonChartData(piedata);
                PieJson PieInfo = frm.PieInfo;
                PieInfo.DataSource = jsdata;
                //尺寸
                PieInfo.Size = piesize;
                PieInfo.ThematicType = "3D圆饼图";
                string jsonText = JsonHelper.GetJsonText(PieInfo);
                
                int obj = 0;
                //创建白色图表背景
                if (!frm.IsTransparent)
                {
                    CreateWhiteBackGround(eles);
                }
                var remarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, _centerpoint, jsonText, out obj, piesize);
                CreateAnnotion(remarker, _centerpoint, piesize, chartTitle, stics, annoTxt, labetype, obj);
                #endregion
                #endregion
            }
            pAc.Refresh();
            wo.Dispose();
            MessageBox.Show("生成完成");
        }
        private List<double> getMaxStaticData(Dictionary<string, Dictionary<string, double>> datas)
        {
            List<double> lists = new List<double>();
            foreach (var kv in datas)
            {
                double total = getStaticData(kv.Value);
                lists.Add(total);
            }
            return lists;
        }
        private double getStaticData(Dictionary<string, double> dicvals)
        {
            double total = 0;
            foreach (var kv in dicvals)
            {
                total += kv.Value;
            }
            return total;
        }
       
        private void IntialData()
        {
            //初始化
            moveR = 4 * mapScale / 1000;
            arcPoint.Clear();
            centerAngle.Clear();
            percentage.Clear();
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

        #region 绘制相关
        private string  DrawStaticPieVals(Dictionary<string, double> _dicvals)
        {
            string txt = "";
            double sum = 0;
            foreach (var kv in _dicvals)
            {
                sum += kv.Value;
            }
            txt = sum.ToString();
            return txt;
        }
        private void CreateClipPoint(double a, Dictionary<string, double> dicvals, ref List<IPoint> paths, ref List<IPoint> circelpaths, ref  List<IPoint> circelpaths1)
        {
            double step = 0;
            double r = 2*a;//半径
            ridia = a;
            IPoint orignPoint = new PointClass() { X = centerpoint.X, Y = centerpoint.Y + r };
            paths.Add(orignPoint);
            circelpaths.Add(new PointClass() { X = centerpoint.X, Y = centerpoint.Y + ridia });
            circelpaths1.Add(new PointClass() { X = centerpoint.X, Y = centerpoint.Y + ridia });
            foreach (var kv in dicvals)
            {
                //zhuji
                centerAngle.Add((step + kv.Value / 2) / total * Math.PI * 2);
                arcPoint.Add(getPoint((step + kv.Value / 2) / total * Math.PI * 2, ridia, centerpoint));
                percentage.Add(Math.Round(kv.Value / total * 100, 1));

                step += kv.Value;
                double angle = step / total * Math.PI * 2;
                IPoint leftPoint = getPoint(angle, r, centerpoint);
                paths.Add(leftPoint);
                IPoint circelpoint = getPoint(angle, ridia, centerpoint);
                //修正Y
                IPoint pcircelp = RepairEdgePoint(centerpoint, leftPoint, pEllipseRing, circelpoint);
                circelpaths1.Add(pcircelp);
                circelpaths.Add(circelpoint);
            }
        }
        //修正边Y值
        private IPoint RepairEdgePoint(IPoint center, IPoint point, IGeometry pEllipseRing, IPoint circel)
        {
             double height = 4 * mapScale / 1000;
             IPoint edgepoint = new PointClass() { X = circel.X, Y = circel.Y - height };
             try
             {
                 double r = (center as IProximityOperator).ReturnDistance(point);
                 IGeometryCollection pPolyline = new PolylineClass();
                 IPointCollection pCl = new PathClass();
                 pCl.AddPoint(center);
                 pCl.AddPoint(point);
                 pPolyline.AddGeometry(pCl as IGeometry);
                 ITopologicalOperator pTo = pPolyline as ITopologicalOperator;
                 pTo.Simplify();
                 IRelationalOperator pRe = pPolyline as IRelationalOperator;
                 if (!pRe.Disjoint(pEllipseRing))//相交
                 {
                     IPolyline pinsect = (pPolyline as ITopologicalOperator).Intersect(pEllipseRing, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                     if (pinsect.IsEmpty)
                         return edgepoint;
                     pPolyline = new PolylineClass();
                     pCl = new PathClass();
                     pCl.AddPoint(new PointClass() { X = pinsect.FromPoint.X, Y = pinsect.FromPoint.Y + r });
                     pCl.AddPoint(new PointClass() { X = pinsect.FromPoint.X, Y = pinsect.FromPoint.Y - r });
                     pPolyline.AddGeometry(pCl as IGeometry);
                     pinsect = (pPolyline as ITopologicalOperator).Intersect(pEllipseRing, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                     if (pinsect.IsEmpty)
                         return edgepoint;
                     edgepoint.Y = pinsect.ToPoint.Y;
                 }
                 return edgepoint;
             }
             catch
             {
                 return edgepoint;
             }
        }
        //绘制扇形变
        private IPolygon CreateFansEdge(IPoint center, IPoint f, IPoint movepoint, ICmykColor pcolor,IPoint f1)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            
            double height = 4 * mapScale / 1000;
            IPointCollection pPolygon = new PolygonClass();
            pPolygon.AddPoint(center);
            pPolygon.AddPoint(new PointClass() { X = center.X, Y = center.Y - height });
            pPolygon.AddPoint(f1);
            pPolygon.AddPoint(f);
            pPolygon.AddPoint(center);
            (pPolygon as ITopologicalOperator).Simplify();
          
            ITransform2D pTrans2d = pPolygon as ITransform2D;
            pTrans2d.Move(movepoint.X - center.X, movepoint.Y - center.Y);
            if (!(pPolygon as IGeometry).IsEmpty)
            {
               
                IElement pEl = null;
                IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                IFillShapeElement polygonElement = new PolygonElementClass();
                ICmykColor pcmyk = (pcolor as IClone).Clone() as ICmykColor;
                pcmyk.Black += 15;
                polygonElement.Symbol = GetColorSymbol(pcmyk);
                pEl = polygonElement as IElement;
                pEl.Geometry = pPolygon as IGeometry;
                eles.Add(pEl);
               
               
            }
            return pTrans2d as IPolygon;
        }
        private void DrawEllipseRing(double angle, IGeometry ellipsering, IPoint centerpoint)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);


            IGeometry left = (ellipsering as IClone).Clone() as IGeometry; 
            IGeometry right = (ellipsering as IClone).Clone() as IGeometry; 
            double r = (ellipsering.Envelope.Height + ellipsering.Envelope.Width);
            IGeometryCollection pPolyline = new PolylineClass();
            IPointCollection pCl = new PathClass();
            pCl.AddPoint(centerpoint);
            pCl.AddPoint(new PointClass() { X = centerpoint.X - r * Math.Tan(angle), Y = centerpoint.Y- r / Math.Tan(angle) });
            pPolyline.AddGeometry(pCl as IGeometry);
           
            IRelationalOperator pRe = pPolyline as IRelationalOperator;
            if (!pRe.Disjoint(ellipsering))
            {
                IPolyline pinsect = (pPolyline as ITopologicalOperator).Intersect(ellipsering, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                //左右两边
                IEnvelope penv = ellipsering.Envelope;
                penv.Expand(2, 2, true);
                penv.XMax = pinsect.FromPoint.X;

                (left as ITopologicalOperator).Clip(penv);
                penv = ellipsering.Envelope;
                penv.Expand(2, 2, true);
                penv.XMin = pinsect.FromPoint.X;
                (right as ITopologicalOperator).Clip(penv);
                
            }
            //绘制
            IRgbColor rgb = new RgbColorClass();
            rgb.Red = 133;
            rgb.Green = 133;
            rgb.Blue = 133;
            IRgbColor rgb1 = new RgbColorClass();
            rgb1.Red = 253;
            rgb1.Green = 253;
            rgb1.Blue = 253;

            
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            //右边
            IGradientFillSymbol fillsym = gh.CreateGradientSym(rgb1, rgb);
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            polygonElement.Symbol = fillsym;
            pEl = polygonElement as IElement;
            pEl.Geometry = left as IGeometry;

            
            //左边
            fillsym = gh.CreateGradientSym(rgb, rgb1);
          
            polygonElement = new PolygonElementClass();
            polygonElement.Symbol = fillsym;
            pEl = polygonElement as IElement;
            pEl.Geometry = right as IGeometry;

            //
            DrawEllipseRing(ellipsering);
        }
        //切割环
        private IGeometry CutEllipseRing(IPoint f, IPoint t, ref IGeometry ellipsering)
        {
            try
            {
                IGeometry left = null;
                IGeometry right = null;
                double r = ellipsering.Envelope.Height;
                IGeometryCollection pPolyline = new PolylineClass();
                IPointCollection pCl = new PathClass();
                pCl.AddPoint(f);
                pCl.AddPoint(t);
                pPolyline.AddGeometry(pCl as IGeometry);

                IRelationalOperator pRe = pPolyline as IRelationalOperator;
                if (!pRe.Disjoint(ellipsering))
                {
                     left = (ellipsering as IClone).Clone() as IGeometry;
                     right = (ellipsering as IClone).Clone() as IGeometry; 
                    IPolyline pinsect = (pPolyline as ITopologicalOperator).Intersect(ellipsering, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                    //左右两边
                    IEnvelope penv = ellipsering.Envelope;
                    penv.Expand(2, 2, true);
                    penv.XMax = pinsect.FromPoint.X;

                    (left as ITopologicalOperator).Clip(penv);
                    penv = ellipsering.Envelope;
                    penv.Expand(2, 2, true);
                    penv.XMin = pinsect.FromPoint.X;
                    (right as ITopologicalOperator).Clip(penv);
                    ellipsering = left;
                }

                return right;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //绘制环
        private void DrawEllipseRing(IGeometry pgeo)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Style = esriSimpleFillStyle.esriSFSNull;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();
            smline.Style = esriSimpleLineStyle.esriSLSSolid;
            smline.Width = 1.75;
            smline.Color = new RgbColorClass() { Red = 220, Blue = 220, Green = 220 };
            smsymbol.Outline = smline;
            polygonElement.Symbol = smsymbol;
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
          
        }
        //绘制扇形
        private void DrawFans(IGeometry pgeo, IColor pcolor,double sx,double sy)
        {
            ITransform2D ptrans2d = pgeo as ITransform2D;
            ptrans2d.Move(sx, sy);
            IElement pEl = null;
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IFillShapeElement polygonElement = new PolygonElementClass();
            polygonElement.Symbol = GetColorSymbol(pcolor);
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
          
            eles.Add(pEl);
           
        }
        private ISimpleFillSymbol GetColorSymbol(IColor pcolor)
        {
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Color = pcolor;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();

            smline.Width = linewidth;
            smline.Color = pcolor;
            smsymbol.Outline = smline;
            return smsymbol;
        }
      
        /// <summary>
        /// 构造椭圆环
        /// </summary>
        /// <param name="pEllipse">椭圆</param>
        /// <param name="width">环高度</param>
        /// <returns></returns>
        private IGeometry ContrustEllipseRing(IPolygon pEllipse, double width)
        {
            IGeometry pEllipseRing = null;
            try
            {
                GraphicsHelper gh = new GraphicsHelper(pAc);
                IEnvelope pEnvelope=pEllipse.Envelope;
                IPoint centerpoint = new PointClass() { X = (pEnvelope.XMin + pEnvelope.XMax) / 2, Y = (pEnvelope.YMin + pEnvelope.YMax) / 2 };

                //1.平移
                IPolygon cloneEllipse = (pEllipse as IClone).Clone() as IPolygon;
                ITransform2D ellipseTrans = cloneEllipse as ITransform2D;
                ellipseTrans.Move(0, -width * 1e-3 * mapScale);
                //2 构造矩形
                double recwidth = pEnvelope.Width;
                double recheight = width * 1e-3 * mapScale;
                IPoint upleft = new PointClass() { X = centerpoint.X - recwidth / 2, Y = centerpoint.Y };
                IPolygon pRect = gh.CreateRectangle(upleft, recwidth, recheight);
                //2.获取下半椭圆
                IGeometryCollection pPolyline = new PolylineClass();
                IPointCollection pCl = new PathClass();
                pCl.AddPoint(new PointClass() { X = upleft.X, Y = upleft.Y - recheight });
                pCl.AddPoint(new PointClass() { X = upleft.X + recwidth, Y = upleft.Y - recheight });
                pPolyline.AddGeometry(pCl as IGeometry);
                IGeometry pleftGeo = null;
                IGeometry prightGeo = null;
                (cloneEllipse as ITopologicalOperator).Cut(pPolyline as IPolyline, out pleftGeo, out prightGeo);
                //3.矩形，下半椭圆合并
                IGeometry pgeo1 = (prightGeo as ITopologicalOperator).Union(pRect as IGeometry);
                //4.椭圆环
                pEllipseRing = (pgeo1 as ITopologicalOperator).Difference(pEllipse as IGeometry);

                (pEllipseRing as ITopologicalOperator).Simplify();
                return pEllipseRing as IGeometry;
            }
            catch
            {
                return null;
            }
        }
        //
        private IPoint getPoint(double angle, double r, IPoint center)
        {
            double dx = 0, dy = 0;
            IPoint point = new PointClass();
            if (angle < Math.PI * 0.5)
            {
                dx = r * Math.Sin(angle);
                dy = r * Math.Cos(angle);
            }
            else if ((angle >= Math.PI * 0.5) && (angle < Math.PI))
            {
                dx = r * Math.Cos(angle - Math.PI * 0.5);
                dy = -r * Math.Sin(angle - Math.PI * 0.5);
            }
            else if ((angle >= Math.PI) && (angle < Math.PI * 1.5))
            {
                dx = -r * Math.Sin(angle - Math.PI);
                dy = -r * Math.Cos(angle - Math.PI);
            }
            else if ((angle >= Math.PI * 1.5) && (angle <= Math.PI * 2))
            {
                dx = -r * Math.Cos(angle - Math.PI * 1.5);
                dy = r * Math.Sin(angle - Math.PI * 1.5);
            }
            point.PutCoords(center.X + dx, center.Y + dy);
            return point;
        }
        #endregion
        #region 1.引线式标注
        //调用
        public void DrawPieNoteLine(Dictionary<string ,double>dicvals)
        {
            int ct = 0;
            double step = 0;
            foreach (var kv in dicvals)
            {
                double angle = step / total * Math.PI * 2;
                angle += kv.Value / total * Math.PI;
                step += kv.Value;
                IPoint movepoint = getPoint(angle, moveR, centerpoint);
                double sx = movepoint.X - centerpoint.X;
                double sy = movepoint.Y - centerpoint.Y;
                DrawPieNote(sx, sy, centerAngle[ct], arcPoint[ct], kv.Key + ":" + percentage[ct] + "%", centerpoint);
                DrawPieAnnotation(ct, kv.Value.ToString());
                ct++;

            }
        }
        private void DrawPieNote(double sx,double sy,double angle, IPoint pArc, string txt, IPoint center, double r = 8)
        {
            //1.
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IPoint p = new PointClass();
            IPoint other = new PointClass();
            double a = Math.Abs(pArc.Y - center.Y) / Math.Abs(pArc.X - center.X);
            double b = Math.Atan(1 / a);
            a = Math.Atan(a);
            angle = angle / Math.PI * 180;
            double slopeAngle = Math.PI / 6;
            double R = 30;
            //double txtheight = 1 * mapScale * 1e-3;
            if (angle <= 90)
            {

                double edgeX = center.X + (r + 40) * mapScale * 1e-3;
                double dis = edgeX - pArc.X;
                double y = dis * Math.Tan(slopeAngle);
                p.PutCoords(edgeX, pArc.Y + y);
                other.PutCoords(p.X + R * mapScale * 1e-3, p.Y);
            }
            else if (angle > 90 && angle <= 180)
            {

                double edgeX = center.X + (r + 40) * mapScale * 1e-3;
                double dis = Math.Abs(edgeX - pArc.X);
                double y = dis * Math.Tan(slopeAngle);
                p.PutCoords(edgeX, pArc.Y - y);
                other.PutCoords(p.X + R * mapScale * 1e-3, p.Y);

            }
            else if (angle > 180 && angle <= 270)
            {

                double edgeX = center.X - (r + 40) * mapScale * 1e-3;
                double dis = Math.Abs(edgeX - pArc.X);
                double y = dis * Math.Tan(slopeAngle);
                p.PutCoords(edgeX, pArc.Y - y);
                other.PutCoords(p.X - R * mapScale * 1e-3, p.Y);
                double txtlen = gh.GetStrLen(txt) / 4;
                other.X -= txtlen*15;
            }
            else if (angle > 270 && angle <= 360)
            {

                double edgeX = center.X - (r + 40) * mapScale * 1e-3;
                double dis = Math.Abs(edgeX - pArc.X);
                double y = dis * Math.Tan(slopeAngle);
                p.PutCoords(edgeX, pArc.Y + y);
                other.PutCoords(p.X - R * mapScale * 1e-3, p.Y);
                double txtlen = gh.GetStrLen(txt) / 4;
                other.X -= txtlen * 15;
            }

            IPointCollection pc = new PolylineClass();
            pc.AddPoint(pArc);
            pc.AddPoint(p);
            pc.AddPoint(other);
            ITransform2D ptrans2d = pc as ITransform2D;
            ptrans2d.Move(sx, sy);

            var ele=  DrawLine(pc as IPolyline);
            eles.Add(ele);
            //other.Y += sy;
            //other.X += sx;
            double cx = (other.X - center.X) / TOTAL;
            double cy = (other.Y - center.Y) / TOTAL;
            cx = (p.X + other.X) * 0.5;
            cy = (p.Y + other.Y) * 0.5;
            IPoint pt = new PointClass() { X = cx+sx, Y = cy+sy };
            annoTxt.Add(pt, txt);
        }
        private IElement DrawLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = linewidth;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = 10;
                rgb.Blue = 10;
                rgb.Green = 10;
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
        #endregion 
        #region 2. 压盖式标注
        //调用
        public void DrawPieNoteOverlap(Dictionary<string, double> dicvals)
        {
            int ct = 0;
            double step = 0;
            double pieMR = 4 * mapScale / 1000;
            foreach (var kv in dicvals)
            {
                double angle = step / total * Math.PI * 2;
                angle += kv.Value / total * Math.PI;
                step += kv.Value;
                IPoint movepoint = getPoint(angle, pieMR, _centerpoint);
                double sx = movepoint.X - _centerpoint.X;
                double sy = movepoint.Y - _centerpoint.Y;
                IPoint center = _centerpoint;
                IPoint edgepoint = getPoint(angle, ridia, _centerpoint);
           
                IPoint labelPoint=new PointClass() { X = (center.X + edgepoint.X) / 2, Y = (center.Y + edgepoint.Y) / 2 };
                ITransform2D ptrans2d = labelPoint as ITransform2D;
                ptrans2d.Move(sx, sy);
                string txt= kv.Key + ":" + percentage[ct] + "%";
                double cx = (labelPoint.X - center.X) / TOTAL;
                double cy = (labelPoint.Y - center.Y) / TOTAL;
                IPoint pt = new PointClass() { X = cx, Y = cy };
                pt = staticPoints[ct];
                annoTxt.Add(pt, txt);
                ct++;
            }
        }
        #endregion

        #region 3.图例式注记
        //调用
        public void DrawPieNoteLengend( Dictionary<string, double> data,double height)
        {
            IGeometry pgeo = pEllipseClone;
            height = mapScale * height * 1.0e-3;

            string[] types = new string[data.Count];
            int t = 0;
            foreach (var kv in data)
            {
                types[t++] = kv.Key;
            }
            GraphicsHelper gh = new GraphicsHelper(pAc);
            int ct = data.Count;
            double totalheight = pgeo.Envelope.Height + height;
            IPoint basepoint = new PointClass() { X = centerpoint.X + pgeo.Envelope.Width / 2 + height+moveR, Y = centerpoint.Y + pgeo.Envelope.Height / 2 };
            //获取每个图列的高度
            double lgheight = totalheight / (ct + (ct - 1) * 0.8);
            for (int i = 0; i < ct; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - i * (1.8) * lgheight;
                upleft.PutCoords(basepoint.X, y);
                IPolygon prect = gh.CreateRectangle(upleft, lgheight*4/3.0, lgheight);
               
                var ele= gh.DrawPolygon(prect, cmykColors[i], 0);
                eles.Add(ele);

                double cx = (upleft.X + lgheight * 4 / 3.0 + lgheight * 0.5 - centerpoint.X) / TOTAL;
                double cy = (upleft.Y - lgheight * 0.5 - centerpoint.Y) / TOTAL;
                cy = (prect as IArea).Centroid.Y;
                cx = (prect as IArea).Centroid.X + prect.Envelope.Width * 0.5 + prect.Envelope.Height * 0.5;
              


                IPoint pt = new PointClass() { X = cx, Y = cy };
                annoTxt.Add(pt, types[i]);
                DrawPieAnnotation(i, data[types[i]] + ":" + percentage[i] + "%");
            }

        }
        public void DrawPieAnnotation(int index, string txt)
        {
            IPoint txtpoint2 = staticPoints[index];
            staticTxt.Add(txtpoint2, txt);

        }
        //文字注记
        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, string pietitle, string staticsnum, Dictionary<IPoint, string> annoTxt, string labetype, int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double size = curms * 1.0e-3 * markersize;
            double fontsize = 0; double repTransform = 1;
            //1.确定饼图长度和宽度
            if (RepMarker.Height < RepMarker.Width)
            {
                width = size;
                height = size * RepMarker.Height / RepMarker.Width;
                fontsize = width / 50;
            }
            else
            {
                height = size;
                width = size * RepMarker.Width / RepMarker.Height;
                fontsize = height / 50;
            }
            repTransform = height / RepMarker.Height;
            //2.根据统计图实际大小确定字体的大小
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);
            fontsize *= 2.83;
            //3.绘制图例注记
            double total = (((annoTxt.Count - 1) * (lgVal)) / RepMarker.Height) * height;
            //foreach (var k in annoTxt)
            //{

            //    IPoint pt = new PointClass();
            //    double x = 0.0;
            //    if (labetype == "图例式标注")
            //    {
            //        double strwidth = gh.GetStrWidth(k.Value, curms, fontsize);
            //        x = k.Key.X * total + pAnchor.X + strwidth / 2.0;
            //    }
            //    else
            //    {
            //        x = k.Key.X * total + pAnchor.X;
            //    }
            //    double y = k.Key.Y * total + pAnchor.Y;
            //    pt.PutCoords(x, y);
            //    InsertAnnoFea(pt, k.Value, fontsize * 0.8, id);
            //}
            foreach (var k in annoTxt)
            {
                IPoint pt = new PointClass();
                double x = 0.0;

                x = k.Key.X * repTransform + pAnchor.X;

                double y = k.Key.Y * repTransform + pAnchor.Y;
                pt.PutCoords(x, y);
                esriTextVerticalAlignment ver = esriTextVerticalAlignment.esriTVACenter;
                esriTextHorizontalAlignment hor = esriTextHorizontalAlignment.esriTHACenter;
                if (labeType.Contains("引线"))
                {
                    ver = esriTextVerticalAlignment.esriTVABottom;
                }
                if (labeType.Contains("图例"))
                {
                    hor = esriTextHorizontalAlignment.esriTHALeft;
                }
                InsertAnnoFea(pt, k.Value, fontsize * 0.8, id, ver,hor);
            }
            foreach (var k in staticTxt)
            {

                IPoint pt = new PointClass();
                double x = 0.0;

                x = k.Key.X * repTransform + pAnchor.X;

                double y = k.Key.Y * repTransform + pAnchor.Y;
                pt.PutCoords(x, y);
                InsertAnnoFea(pt, k.Value, fontsize * 0.8, id);
            }
            //4.绘制统计标注
            if (staticsnum != "")
            {
                InsertAnnoFea(pAnchor, staticsnum, fontsize * 0.8, id);
            }
            //5.绘制标题
            if (pietitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X, Y = pAnchor.Y + height / 2.0 + height / 10 };
                InsertAnnoFea(pt, pietitle, fontsize * 1.5, id);
            }
        }
        private void InsertAnnoFea(IGeometry pGeometry, string annoName, double fontSize, int id, esriTextVerticalAlignment ver = esriTextVerticalAlignment.esriTVACenter  , esriTextHorizontalAlignment hor = esriTextHorizontalAlignment.esriTHACenter)
        {
            IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize,ver,hor);
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annoFcl.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pAnnoFeature.Annotation = pElement;
            pFeature.Store();
        }
        private ITextElement CreateTextElement(IGeometry pGeoTxt, string txt, IFontDisp pFont, double size, esriTextVerticalAlignment ver = esriTextVerticalAlignment.esriTVACenter,esriTextHorizontalAlignment hor = esriTextHorizontalAlignment.esriTHACenter)
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
                VerticalAlignment =ver,
                HorizontalAlignment = hor
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

        #endregion
    }
}

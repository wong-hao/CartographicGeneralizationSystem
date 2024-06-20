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
using SMGI.Common;
using System.Linq;

namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    /// <summary>
    ///  3d环绘制辅助类
    /// </summary>
   public sealed class Draw3DRingPie2
    {
       private IActiveView pAc = null;
        private double mapScale = 1000;
        private List<IElement> eles = new List<IElement>();
        private ILayer pRepLayer = null;
        private IFeatureClass annoFcl = null;
        public static Dictionary<IPoint, string> annoTxt = new Dictionary<IPoint, string>();//文字注记
        public static Dictionary<IPoint, string> staticTxt = new Dictionary<IPoint, string>();//统计数据注记
        public static Dictionary<int, IPoint> staticPoints = new Dictionary<int, IPoint>();//统计几何
        double TOTAL;//总比值
        double lgVal;//图例间隔
        GraphicsHelper gh = null;
        double linewidth = 0.25;
        public Draw3DRingPie2(IActiveView pac, double ms)
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
            annoFcl = (lyr.First() as IFeatureLayer).FeatureClass;

            gh = new GraphicsHelper(pAc);
        }
        /// <summary>
        /// 公共方法绘制环饼
        /// </summary>
        public  void DrawRingPieCharts(FrmPieChartsSet frm,IPoint anchorpoint)
        {
            CommonMethods.ClearThematicCarto(anchorpoint, (pRepLayer as IFeatureLayer).FeatureClass, annoFcl);   
            eles.Clear();
            Draw3DRingStatic(frm,anchorpoint);
           
          
        }
        #region


        double markerSize = 20;
        double markerSizeMax = 20;
        string noteLableType = "";
        IPoint _centerpoint = null;
        IPoint centerpoint = new PointClass() { X = 0, Y = 0 };
        IPolygon pEllipseClone = null;//椭圆
        IPolygon pEllipseIn = null;//内环   
        double total = 0;//总数
        List<double> centerAngle = new List<double>();//每个弧中点角度
        List<double> percentage = new List<double>();//每个弧比例
        List<ICmykColor> cmykColors = null;//每个部分的颜色
        bool enablePieVals = false;
        string labeType = string.Empty;

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
        
        //绘制3d环状图
        private void Draw3DRingStatic(FrmPieChartsSet frm,IPoint ctpoint, double width = 3.5)
        {
            
            IPolygon pEllipse = null;
            double ringrate = 0.4;
            double a = 40 * mapScale / 1000;
            double b = 20 * mapScale / 1000;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取颜色设置
            PieJson PieInfo = frm.PieInfo;
            markerSize = frm.MarkerSize;
            markerSizeMax = frm.MarkerSizeMax;
            noteLableType = frm.LableType;
            string labetype = frm.LableType;
            labeType = labetype;
            ringrate = frm.RingRate;
            //长短轴比率
            double abrate = frm.EllipseRate;
            if (abrate > 0)
            {
                b = a / abrate;
            }
            enablePieVals = frm.EnablePieVals;
            string chartTitle = frm.ChartTitle;
            cmykColors = frm.CMYKColors;
            string geoRelate = frm.GeoLayer;
            var multiDatas = frm.ChartDatas;
            //---计算饼图大小
            var sumDatalist = getMaxStaticData(multiDatas);
            double fxa = 0;
            if ((sumDatalist.Max() - sumDatalist.Min()) != 0)
            {
                fxa = (markerSizeMax - markerSize) / (sumDatalist.Max() - sumDatalist.Min());
            }
            double fxb = 0;
            fxb = fxa * sumDatalist.Min() + markerSize;
            //---计算饼图大小
            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("正在处理...");
            Dictionary<string, IPoint> namesPt = null;
            if (geoRelate != "")
            {
                namesPt = ChartsDataSource.ObtainGeoRelated(geoRelate);
            }

            

            foreach (var kvCharts in multiDatas)
            {
                eles.Clear();
                centerAngle.Clear() ;//每个弧中点角度
                percentage.Clear();//每个弧比例
                #region
                string name = kvCharts.Key;
                _centerpoint = ctpoint;
                if (geoRelate != "")
                {
                    if (namesPt.ContainsKey(name))
                        _centerpoint = namesPt[name];
                }
                Dictionary<string, double> dicvals = kvCharts.Value;
               
                total=  getStaticData(dicvals);             
              
                //构造椭圆
                pEllipse = gh.ConsturctEllipse(centerpoint, a, b);
                pEllipseClone = (pEllipse as IClone).Clone() as IPolygon;
                IGeometry pEllipseRing = null;//结果2
                double dheight = width * 1e-3 * mapScale;
                // 构造椭圆环 下
                pEllipseRing = ContrustEllipseRing(pEllipse, width);
                pEllipseIn = gh.ConsturctEllipse(centerpoint, a * ringrate, b * ringrate);
                IPolygon pEllipseInRing = null;//结果3
                //  构造椭圆环 内
                pEllipseInRing = ConstructInnerRing(centerpoint, a, b, ringrate);
                
               
                //绘制
                ITopologicalOperator pTo = pEllipse as ITopologicalOperator;
                List<IPoint> paths = new List<IPoint>();
                List<IPoint> halfPaths = new List<IPoint>();//一半点
                #region 创建切割点
                double step = 0;
                double r = a + b;//半径
                IPoint orignPoint = new PointClass() { X = centerpoint.X, Y = centerpoint.Y + r };
                paths.Add(orignPoint);
                foreach (var kv in dicvals)
                {
                    centerAngle.Add((step + kv.Value / 2) / total * Math.PI * 2);
                    percentage.Add(Math.Round(kv.Value / total * 100, 1));
                     
                    step += kv.Value;
                    double angle = step / total * Math.PI * 2;
                    IPoint leftPoint = getPoint(angle, r, centerpoint);
                    paths.Add(leftPoint);
                    angle = (step - kv.Value / 2) / total * Math.PI * 2;
                    IPoint halfPoint = getPoint(angle, r, centerpoint);
                    halfPaths.Add(halfPoint);
                }
                #endregion
                IGeometryCollection pPolyline = new PolylineClass();
                IPointCollection pCl = new PathClass();
               
                #region 创建扇形
                int ct = 0;
                IGeometry pleftgeo=null;
                IGeometry prightgeo = null;
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
                     pTo.Cut(pPolyline as IPolyline, out pleftgeo, out prightgeo);
                     IGeometry geofan= (pleftgeo as ITopologicalOperator).Difference(pEllipseIn);

                     DrawFans(geofan, cmykColors[ct]);
                     staticPoints[ct] = (geofan as IArea).LabelPoint;
                    ct++;
                }
             
                #endregion
                DrawEllipseRing(Math.PI / 6, pEllipseRing, centerpoint);
                DrawEllipseInRing(Math.PI / 6, pEllipseInRing, centerpoint);
                //绘制标注
                #region
                ct = 0;
                annoTxt.Clear();
                staticTxt.Clear();
                double totalheight = pEllipse.Envelope.Height + width;
                double lgheight = totalheight / (dicvals.Count + (dicvals.Count - 1) * 0.8);
                TOTAL = (dicvals.Count - 1) * 1.8 * lgheight;
                lgVal = 1.8 * lgheight;

                if (labetype == "图例式标注")
                {
                    DrawPieNoteLengend( width, dicvals);
                  
                }
                else if (labetype == "引线式标注")
                {
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteLine(centerAngle[ct], pEllipse, txt+":"+percentage[ct]+"%");
                        DrawPieAnnotation(ct, kv.Value.ToString());
                        ct++;
                    }
                }
                else if (labetype == "压盖式标注")
                {
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteOverlap(ct, txt+":"+percentage[ct]+"%");
                        ct++;
                    }
                }
                //获取统计数据
                string stics = "";
                if (enablePieVals)
                {
                    stics = DrawStaticPieVals(dicvals);
                }

                #endregion
                double piesize = total * fxa + fxb;
                var piedata = new Dictionary<string, Dictionary<string, double>>();
                piedata[kvCharts.Key] = kvCharts.Value;
                string jsdata = JsonHelper.JsonChartData(piedata);
                PieInfo.DataSource = jsdata;
                //尺寸
                PieInfo.Size = piesize;
                PieInfo.ThematicType = "3D环状饼图";
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
            }
            pAc.Refresh();
            wo.Dispose();
            MessageBox.Show("生成完成");
            
        }
        #endregion
        private void DrawcPieNoteVals(IPoint center, Dictionary<string, double> dicvals, string ChtName, int objid)
        {

            double a = 0.5 * markerSize * 1e-3 * mapScale;
            double b = 0.25 * markerSize * 1e-3 * mapScale;
            IPolygon pEllipse = gh.ConsturctEllipse(_centerpoint, a, b);
            double angle1 = 0;
            double total = getStaticData(dicvals);
            IEnvelope en = pEllipse.Envelope;
            double l = en.XMax - en.XMin;
            double l2 = en.YMax - en.YMin;
            double step = 0;
            foreach (var kv in dicvals)
            {
                if (kv.Value == 0)
                    continue;
                angle1 = (step + kv.Value / 2) / total * Math.PI * 2;
                step += kv.Value;
                double dx = 0;
                double dy = 0;
                double angle = angle1 / Math.PI * 180;
                #region
                if (angle <= 45)
                {
                    dy = 3.5;
                }
                else if (angle > 45 && angle <= 90)
                {
                    dx = 5.5;

                }
                else if (angle > 90 && angle <= 135)
                {
                    dy = -2.5;
                    dx = 16;

                }
                else if (angle > 135 && angle <= 180)
                {
                    dx = 1.5;
                    dy = -8;

                }
                else if (angle > 180 && angle <= 225)
                {
                    dx = -1.5;
                    dy = -8;

                }
                else if (angle > 225 && angle <= 270)
                {
                    dy = -2.5;
                    dx = -16;

                }
                else if (angle > 270 && angle <= 315)
                {
                    dx = -5.5;


                }
                else if (angle > 315 && angle <= 360)
                {
                    dy = 3.5;

                }
                #endregion

                dx = dx * 1e-3 * mapScale;
                dy = dy * 1e-3 * mapScale;
                IPoint pt = GetEllipsePoint(angle1, pEllipse);
                IFontDisp pFont = new StdFont()
                {
                    Name = "黑体",
                    Size = 2
                } as IFontDisp;
                pt.X += dx;
                pt.Y += dy;
                double fontsize = 3.2 * 2.83;
                InsertAnnoFea(pt,  kv.Key, fontsize, objid);
            }

            IPoint pt2 = GetEllipsePoint(0, pEllipse);
            double x = pt2.X;
            double y = pt2.Y + l2 / 2;
            pt2 = new PointClass() { X = x, Y = y };
            IFontDisp pFont2 = new StdFont()
            {
                Name = "黑体",
                Size = 2
            } as IFontDisp;
            double fontsize2 = 3.2 * 2.83 * 1.3;
            InsertAnnoFea(pt2,  ChtName, fontsize2, objid);
        }
       
        #region 绘制相关
        private string DrawStaticPieVals(Dictionary<string, double> _dicvals)
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
        // 构造内部椭圆以及环
        private IPolygon ConstructInnerRing(IPoint centerpoint,double a,double b, double ringrate=0.7)
        {
            IPolygon pEllipseIn = null;
            IPolygon pEllipseInRing = null;//结果3
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double recwidth = 2*a;
            double recheight = 3.5 * 1e-3 * mapScale;
            // 绘制内部椭圆 
            pEllipseIn = gh.ConsturctEllipse(centerpoint, a * ringrate, b * ringrate);
            //绘制内部椭圆 下移5个单位
            IPolygon pEllipseInIn = (pEllipseIn as IClone).Clone() as IPolygon;
            ITransform2D pTrans = pEllipseInIn as ITransform2D;
            pTrans.Move(0, -recheight);

            ITopologicalOperator pto = pEllipseIn as ITopologicalOperator;
            pEllipseInRing = pto.Difference(pEllipseInIn) as IPolygon;
            
           
           
            
            return pEllipseInRing;
        }
        private IPolygon ConstructInnerRing1(IPoint centerpoint, double a, double b, double ringrate = 0.7)
        {
            IPolygon pEllipseIn = null;
            IPolygon pEllipseInRing = null;//结果3
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double recwidth = 2 * a;
            double recheight = 5 * 1e-3 * mapScale;
            // 绘制内部椭圆以及环
            pEllipseIn = gh.ConsturctEllipse(centerpoint, a * ringrate, b * ringrate);
            //1.上半椭圆
            IGeometryCollection pPolyline = new PolylineClass();
            IPointCollection pCl = new PathClass();
            pCl.AddPoint(new PointClass() { X = centerpoint.X - recwidth, Y = centerpoint.Y });
            pCl.AddPoint(new PointClass() { X = centerpoint.X + recwidth, Y = centerpoint.Y });
            pPolyline.AddGeometry(pCl as IGeometry);
            IGeometry pupGeo = null;
            IGeometry pdownGeo = null;
            (pEllipseIn as ITopologicalOperator).Cut(pPolyline as IPolyline, out pupGeo, out pdownGeo);
            //2.上半内部椭圆

            IPoint incenter = new PointClass() { X = centerpoint.X, Y = centerpoint.Y - recheight };
            IPolygon pEllipseInIn = gh.ConsturctEllipse(incenter, a * ringrate, b * ringrate - recheight);
            IEnvelope penin = pEllipseInIn.Envelope;
            penin.Expand(2, 2, true);
            penin.YMin = centerpoint.Y - recheight;
            (pEllipseInIn as ITopologicalOperator).Clip(penin);

            //IGeometry pupGeoin = null;
            //IGeometry pdownGeoin = null;
            //(pEllipseInIn as ITopologicalOperator).Cut(pPolyline as IPolyline, out pupGeoin, out pdownGeoin);
            //3.上半椭圆内部
            pEllipseInRing = (pupGeo as ITopologicalOperator).Difference(pEllipseInIn) as IPolygon;
            return pEllipseInRing;
        }
        //绘制下圆环
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

           
            eles.Add(pEl);
            //左边
            fillsym = gh.CreateGradientSym(rgb, rgb1);
           
            polygonElement = new PolygonElementClass();
            polygonElement.Symbol = fillsym;
            pEl = polygonElement as IElement;
            pEl.Geometry = right as IGeometry;

          
            eles.Add(pEl);
            //左边
            pAc.Refresh();
            DrawEllipseRing(ellipsering);
        }
        //绘制内圆环
        private void DrawEllipseInRing(double angle, IGeometry ellipsering, IPoint centerpoint)
        {

            GraphicsHelper gh = new GraphicsHelper(pAc);


            (ellipsering as ITopologicalOperator).Simplify();
            IGeometry left = (ellipsering as IClone).Clone() as IGeometry; 
            IGeometry right = (ellipsering as IClone).Clone() as IGeometry; 
            double r = (ellipsering.Envelope.Height + ellipsering.Envelope.Width);
            IGeometryCollection pPolyline = new PolylineClass();
            IPointCollection pCl = new PathClass();
            pCl.AddPoint(centerpoint);
            pCl.AddPoint(new PointClass() { X = centerpoint.X + r * Math.Tan(angle), Y = centerpoint.Y + r / Math.Tan(angle) });
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
            rgb1.Red = 255;
            rgb1.Green = 255;
            rgb1.Blue = 255;


            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            //左边
            IGradientFillSymbol fillsym = gh.CreateGradientSym(rgb1, rgb);
         
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            polygonElement.Symbol = fillsym;
            pEl = polygonElement as IElement;
            pEl.Geometry = left as IGeometry;

          
            eles.Add(pEl);
            rgb1.Red = 240;
            rgb1.Green = 240;
            rgb1.Blue = 240;
            //右边
            fillsym = gh.CreateGradientSym(rgb, rgb1);
           
            polygonElement = new PolygonElementClass();
            polygonElement.Symbol = fillsym;
            pEl = polygonElement as IElement;
            pEl.Geometry = right as IGeometry;

           
            eles.Add(pEl);
            //左边
            pAc.Refresh();
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
                    IPolyline pinsect = (pPolyline as ITopologicalOperator).Intersect(ellipsering, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                    //切割线
                    pPolyline = new PolylineClass();
                    pCl = new PathClass();
                    pCl.AddPoint(pinsect.FromPoint);
                    pCl.AddPoint(new PointClass() { X = pinsect.FromPoint.X, Y = pinsect.FromPoint.Y - r });
                    pPolyline.AddGeometry(pCl as IGeometry);
                    (ellipsering as ITopologicalOperator).Cut(pPolyline as IPolyline, out left, out right);
                    ellipsering = right;
                }

                return left;

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
            smline.Width = linewidth/2;
            smline.Color = new RgbColorClass() { Red = 220, Blue = 220, Green = 220 };
            smsymbol.Outline = smline;
            polygonElement.Symbol = smsymbol;
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
           
            eles.Add(pEl);
            pAc.Refresh();
        }
        //绘制扇形
        private void DrawFans(IGeometry pgeo, IColor pcolor)
        {
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
            smline.Style = esriSimpleLineStyle.esriSLSSolid;
            smline.Width =linewidth;
            smline.Color = new RgbColorClass() { Red = 220, Blue = 220, Green = 220 };
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
        
        #region 标注相关
        
        #region 1.引线式标注
        //调用
        public void DrawPieNoteLine(double angle, IGeometry pgeo, string txt)
        {
            IPoint edgepoint = GetEllipsePoint(angle, pgeo);
            DrawPieNote(angle, edgepoint, txt,centerpoint);
        }
       
        private IPoint GetEllipsePoint(double ange, IGeometry pEllipse)
        {
            double r = pEllipse.Envelope.Width / 2 + pEllipse.Envelope.Height / 2;
            IPoint center = (pEllipse as IArea).Centroid;
            IPoint p = getPoint(ange, r, center);
            IPointCollection pc = new PolylineClass();
            pc.AddPoint(p);
            pc.AddPoint(center);
            ICurve pcuver = pc as ICurve;
            IPoint otherpoint = new PointClass();
            pcuver.QueryPoint(esriSegmentExtension.esriExtendAtTo, 2, true, otherpoint);
            ITransform2D ptrans = pc as ITransform2D;
            ptrans.Scale(p, 2, 2);
            IPolyline pline = (ptrans as ITopologicalOperator).Intersect(pEllipse, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            IProximityOperator pPro = p as IProximityOperator;
            double distance = pPro.ReturnDistance(pline.FromPoint);
            double distance1 = pPro.ReturnDistance(pline.ToPoint);
            if (distance < distance1)
            {
                return pline.FromPoint;
            }
            else
            {
                return pline.ToPoint;
            }
        }
        private void DrawPieNote(double angle, IPoint pArc, string txt,IPoint center, double r = 5)
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
            double txtunit = 1.0;//1号字体宽度
            double txthalflen = gh.GetStrLen(txt) / 4;
            double fontsize = 13;
            double txtwidth = txthalflen * txtunit * fontsize;
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
                other.PutCoords(p.X - R * mapScale * 1e-3 - txtwidth, p.Y);
            }
            else if (angle > 270 && angle <= 360)
            {
                double edgeX = center.X - (r + 40) * mapScale * 1e-3;
                double dis = Math.Abs(edgeX - pArc.X);
                double y = dis * Math.Tan(slopeAngle);
                p.PutCoords(edgeX, pArc.Y + y);
                other.PutCoords(p.X - R * mapScale * 1e-3-txtwidth, p.Y);
            }
            IPointCollection pc = new PolylineClass();
            pc.AddPoint(pArc);
            pc.AddPoint(p);
            pc.AddPoint(other);

            var ele= DrawLine(pc as IPolyline);
            eles.Add(ele);

           // other.Y += 5;
            double cx = (other.X - center.X) / TOTAL;
            double cy = (other.Y - center.Y) / TOTAL;
            cx = (p.X + other.X) * 0.5;
            cy = (p.Y + other.Y) * 0.5;

            IPoint pt = new PointClass() { X = cx, Y = cy };
            annoTxt.Add(pt, txt);
        }
        private IElement  DrawLine(IPolyline pline)
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
               
              
                pAc.Refresh();

            }
            catch
            {

            }
            return pEl;
        }
        
        #endregion
        #region 2. 压盖式标注
        //调用
        public void DrawPieNoteOverlap(int index, string txt)
        {
            //IPoint center = (geo as IArea).Centroid;
            //IPoint edgepoint = GetEllipsePoint(angle, geo);
            //IPoint txtpoint = new PointClass() { X = (center.X + edgepoint.X) / 2, Y = (center.Y + edgepoint.Y) / 2 };
            //double txtlen = gh.GetStrLen(txt) / 4 * 12;
            //txtpoint.X -= txtlen;
            //IPoint txtpoint2 = new PointClass() { X = (txtpoint.X - center.X) / TOTAL, Y = (txtpoint.Y - center.Y) / TOTAL };
            IPoint txtpoint2 = staticPoints[index];
            annoTxt.Add(txtpoint2,txt);
        }
        #endregion
        public void DrawPieAnnotation(int index, string txt)
        {
            IPoint txtpoint2 = staticPoints[index];
            staticTxt.Add(txtpoint2, txt);
          
        }
        #region 3.图例式注记
        //调用
        public void DrawPieNoteLengend(double height,Dictionary<string, double> data )
        {
            IGeometry pgeo = pEllipseClone;
            height = mapScale * height * 1.0e-3;
            string[] types = new string[data.Count];
            int t = 0;
            foreach (var kv in data)
            {
                types[t++] = kv.Key;
            }
            //double txtunit =1;//1号字体和宽度
            GraphicsHelper gh = new GraphicsHelper(pAc);
            int ct = data.Count;
            double totalheight = pgeo.Envelope.Height + height;

            IPoint basepoint = new PointClass() { X = centerpoint.X + pgeo.Envelope.Width / 2 + height , Y = centerpoint.Y + pgeo.Envelope.Height / 2 };
            //获取每个图列的高度
            double lgheight = totalheight / (ct + (ct - 1) * 0.8);
            for (int i = 0; i < ct; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - i * (1.8) * lgheight;
                upleft.PutCoords(basepoint.X, y);
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 4 / 3.0, lgheight);
                var ele= gh.DrawPolygon(prect, cmykColors[i], 0);
                eles.Add(ele);
                double cx=(upleft .X+lgheight * 4 / 3.0+lgheight *0.5-centerpoint.X)/TOTAL;
                double cy = (upleft.Y - lgheight * 0.5 - centerpoint.Y) / TOTAL;
                cy = (prect as IArea).Centroid.Y;
                cx = (prect as IArea).Centroid.X + prect.Envelope.Width * 0.5 + prect.Envelope.Height*0.5;
                IPoint pt = new PointClass() { X = cx, Y = cy };
                annoTxt.Add(pt, types[i]);
                DrawPieAnnotation(i, data[types[i]] + ":" + percentage[i] + "%");
               

            }
        }
       
       //文字注记
        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, string pietitle, string staticsnum, Dictionary<IPoint, string> annoTxt, string labetype, int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double size = curms * 1.0e-3 * markersize;
            double fontsize = 0;
            double repTransform = 1;
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
                InsertAnnoFea(pt, k.Value, fontsize * 0.8, id, ver, hor);
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
        private void InsertAnnoFea(IGeometry pGeometry, string annoName, double fontSize, int id, esriTextVerticalAlignment ver = esriTextVerticalAlignment.esriTVACenter, esriTextHorizontalAlignment hor = esriTextHorizontalAlignment.esriTHACenter)
        {
            IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize, ver,hor);
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annoFcl.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pAnnoFeature.Annotation = pElement;
            pFeature.Store();
        }
        private ITextElement CreateTextElement(IGeometry pGeoTxt, string txt, IFontDisp pFont, double size, esriTextVerticalAlignment ver = esriTextVerticalAlignment.esriTVACenter, esriTextHorizontalAlignment hor = esriTextHorizontalAlignment.esriTHACenter)
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
                VerticalAlignment = ver,
                HorizontalAlignment =hor
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
        #endregion
    }
}

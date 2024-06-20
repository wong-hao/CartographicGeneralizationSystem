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

    public sealed class Draw3DRingPie
    {

        private IActiveView pAc = null;
        private double mapScale = 1000;
        private List<IElement> eles = new List<IElement>();
        private ILayer pRepLayer = null;
        private IFeatureClass annoFcl = null;//注记图层
        private Dictionary<IPoint, string> annoTxt = new Dictionary<IPoint, string>();//文字注记
        private double TOTAL = 0.0;//图例总高度
        private double lgVal = 0.0;//图例总间距
        private string labetype = "";//标注类型
        GraphicsHelper gh = null;
        double linewidth = 0.25;
        public Draw3DRingPie(IActiveView pac, double ms)
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
        public void DrawRingPieCharts(IPoint anchorpoint)
        {

            eles.Clear();
            Draw3DRingStatic(anchorpoint);


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
        private void Draw3DRingStatic(IPoint ctpoint, double width = 3.5)
        {

            IPolygon pEllipse = null;
            double ringrate = 0.4;
            double a = 40 * mapScale / 1000;
            double b = 20 * mapScale / 1000;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取颜色设置
            FrmPieChartsSet frm = new FrmPieChartsSet(ctpoint,"三维环饼图");
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            markerSize = frm.MarkerSize;
            markerSizeMax = frm.MarkerSizeMax;
            noteLableType = frm.LableType;
             labetype = frm.LableType;
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
                centerAngle.Clear();//每个弧中点角度
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

                total = getStaticData(dicvals);

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
                IGeometry pleftgeo = null;
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
                    IGeometry geofan = (pleftgeo as ITopologicalOperator).Difference(pEllipseIn);

                    DrawFans(geofan, cmykColors[ct]);
                    ct++;
                }

                #endregion
                DrawEllipseRing(Math.PI / 6, pEllipseRing, centerpoint);
                DrawEllipseInRing(Math.PI / 6, pEllipseInRing, centerpoint);
                //绘制标注
                double totalheight = pEllipse.Envelope.Height + width;
                double lgheight = totalheight / (dicvals.Count + (dicvals.Count - 1) * 0.8);
                TOTAL = (dicvals.Count - 1) * 1.8 * lgheight;
                lgVal = 1.8 * lgheight;
                annoTxt.Clear();


                DrawPieLables(labetype, dicvals, width);
                //if (chartTitle != "")
                //{
                //    DrawPieTitle(Math.PI / 4, pEllipse, chartTitle);
                //}
                //获取统计数据
                string stics = "";
                if (enablePieVals)
                {
                    stics = DrawStaticPieValsTXT(dicvals, centerpoint, a);
                }
                #endregion
                double piesize = total * fxa + fxb;
                ChartsToRepHelper ch = new ChartsToRepHelper();
                int obj = 0;
                var remarker = ch.CreateFeatureEX(pAc, eles, pRepLayer, _centerpoint, out obj, piesize);
                CreateAnnotion(remarker, _centerpoint, piesize, chartTitle, stics, annoTxt, labetype, obj);

            }
            pAc.Refresh();
            wo.Dispose();
            MessageBox.Show("生成完成");

        }
        #endregion
        #region 绘制相关
        private void DrawStaticPieVals(Dictionary<string, double> _dicvals, IPoint center, double ellHeight)
        {

            double fontsize = 15;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            string txt = "";
            double sum = 0;
            foreach (var kv in _dicvals)
            {
                sum += kv.Value;
            }
            txt = sum.ToString();
            IPoint txtpoint = new PointClass() { X = center.X, Y = center.Y - ellHeight * 0.4 };

            double txtlen = gh.GetStrLen(txt) / 4;
            double dis = txtlen * fontsize * 1.0;
            txtpoint.X -= dis;
            var ele = gh.DrawTxt(txtpoint, txt, fontsize);

            eles.Add(ele);
        }
        private string  DrawStaticPieValsTXT(Dictionary<string, double> _dicvals, IPoint center, double ellHeight)
        {

            //double fontsize = 15;
            //GraphicsHelper gh = new GraphicsHelper(pAc);
            string txt = "";
            double sum = 0;
            foreach (var kv in _dicvals)
            {
                sum += kv.Value;
            }
            txt = sum.ToString();
            return txt;
            //IPoint txtpoint = new PointClass() { X = center.X, Y = center.Y - ellHeight * 0.4 };

            //double txtlen = gh.GetStrLen(txt) / 4;
            //double dis = txtlen * fontsize * 1.0;
            //txtpoint.X -= dis;
            //var ele = gh.DrawTxt(txtpoint, txt, fontsize);

            //eles.Add(ele);
        }
        // 构造内部椭圆以及环
        private IPolygon ConstructInnerRing(IPoint centerpoint, double a, double b, double ringrate = 0.7)
        {
            IPolygon pEllipseIn = null;
            IPolygon pEllipseInRing = null;//结果3
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double recwidth = 2 * a;
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
            pCl.AddPoint(new PointClass() { X = centerpoint.X - r * Math.Tan(angle), Y = centerpoint.Y - r / Math.Tan(angle) });
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
            smline.Width = linewidth / 2;
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
            smline.Width = linewidth;
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
                IEnvelope pEnvelope = pEllipse.Envelope;
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

        #region 标注相关
        private void DrawPieLables(string labelType, Dictionary<string, double> dicvals, double ringheight)
        {

            switch (labelType)
            {
                case "引线式标注":
                    DrawPieNoteLine(dicvals);
                    break;
                case "压盖式标注":
                    DrawPieNoteOverlap(dicvals);
                    break;
                case "图例式标注":
                    DrawPieNoteLengend(dicvals, ringheight);
                    break;
                default:

                    break;
            }
        }
        #region 1.引线式标注
        //调用
        public void DrawPieNoteLine(Dictionary<string, double> dicvals)
        {
            int ct = 0;
            double step = 0;
            foreach (var kv in dicvals)
            {
                double angle = step / total * Math.PI * 2;
                angle += kv.Value / total * Math.PI;
                step += kv.Value;
                IPoint edgepoint = GetEllipsePoint(centerAngle[ct], pEllipseClone);
                DrawPieNote(centerAngle[ct], edgepoint, kv.Key + ":" + percentage[ct] + "%", centerpoint);
                ct++;

            }
        }
        private void DrawPieTitle(double angle, IGeometry pEllipse, string txt, double r = 5)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double slopeAngle = angle;
            IPoint center = (pEllipse as IArea).LabelPoint;
            double a = pEllipse.Envelope.Width / 2 + r * mapScale * 1e-3;
            double b = pEllipse.Envelope.Height / 2;

            double height = Math.Tan(angle) * a;

            double fontsize = 20;
            IPoint txtpoint = new PointClass() { X = center.X, Y = center.Y + b + height };
            if (noteLableType != "引线式标注")
            {
                txtpoint.Y = b + b / 2;
                fontsize = 15;
            }
            double txtlen = (gh.GetStrLen(txt) / 2 - 1) / 2 * fontsize + fontsize / 2;
            txtpoint.X -= txtlen;

            var ele = gh.DrawTxt(txtpoint, txt, fontsize);
            eles.Add(ele);
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
        private void DrawPieNote(double angle, IPoint pArc, string txt, IPoint center, double r = 5)
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
            double txtheight = 1 * mapScale * 1e-3;
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
                other.PutCoords(p.X - R * mapScale * 1e-3 - txtwidth, p.Y);
            }

            IPointCollection pc = new PolylineClass();
            pc.AddPoint(pArc);
            pc.AddPoint(p);
            pc.AddPoint(other);
            var ele = DrawLine(pc as IPolyline);
            eles.Add(ele);

            other.Y += 5;

            double cx = (other.X - center.X) / TOTAL;
            double cy = (other.Y - center.Y) / TOTAL;
            IPoint lbpt = new PointClass() { X = cx, Y = cy };
            annoTxt.Add(lbpt, txt);
            //ele = gh.DrawTxt(other, txt, 13);
            //eles.Add(ele);
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
        public void DrawPieNoteOverlap(Dictionary<string, double> dicvals)
        {
            int ct = 0;
            double step = 0;
            foreach (var kv in dicvals)
            {
                double angle = step / total * Math.PI * 2;
                angle += kv.Value / total * Math.PI;
                step += kv.Value;

                IPoint edgepoint = GetEllipsePoint(angle, pEllipseClone);
                IPoint edgepoint1 = GetEllipsePoint(angle, pEllipseIn);
                GraphicsHelper gh = new GraphicsHelper(pAc);
                IPoint labelPoint = new PointClass() { X = (edgepoint1.X + edgepoint.X) / 2, Y = (edgepoint1.Y + edgepoint.Y) / 2 };
                string txt = kv.Key + ":" + percentage[ct] + "%";
                double txtlen = gh.GetStrLen(txt) / 4 * 12;

                double cx = (labelPoint.X - centerpoint.X) / TOTAL;
                double cy = (labelPoint.Y - centerpoint.Y) / TOTAL;
                IPoint lbpt = new PointClass() { X = cx, Y = cy };
                annoTxt.Add(lbpt, txt);
                //labelPoint.X -= txtlen;
                //var ele = gh.DrawTxt(labelPoint, txt, 12);
                //eles.Add(ele);
                ct++;
            }
        }
        #endregion

        #region 3.图例式注记
        //调用
        public void DrawPieNoteLengend(Dictionary<string, double> data, double height)
        {
            IGeometry pgeo = pEllipseClone;
            height = mapScale * height * 1.0e-3;


            string[] types = new string[data.Count];
            int t = 0;
            foreach (var kv in data)
            {
                types[t++] = kv.Key;
            }

            double txtunit = 1;//1号字体和宽度
            GraphicsHelper gh = new GraphicsHelper(pAc);
            int ct = data.Count;
            double totalheight = pgeo.Envelope.Height + height;

            IPoint basepoint = new PointClass() { X = centerpoint.X + pgeo.Envelope.Width / 2 + height, Y = centerpoint.Y + pgeo.Envelope.Height / 2 };
            //获取每个图列的高度
            double lgheight = totalheight / (ct + (ct - 1) * 0.8);
            for (int i = 0; i < ct; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - i * (1.8) * lgheight;
                upleft.PutCoords(basepoint.X, y);
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 4 / 3.0, lgheight);
                double fontsize = 8;
                fontsize = Math.Round(fontsize, 1);
                double strwidth = gh.GetStrLen(types[i]) * txtunit * (mapScale / 10000) * fontsize / 2;
                var ele = gh.DrawPolygon(prect, cmykColors[i], 0);
                eles.Add(ele);
                IPoint txtpoint = new PointClass() { X = upleft.X + lgheight * 4 / 3.0 + 2 * mapScale * 1.0e-3 + strwidth / 2, Y = upleft.Y - lgheight * 0.5 };

                IPoint pt = new PointClass() { X = (txtpoint.X - centerpoint.X) / TOTAL, Y = (txtpoint.Y - centerpoint.Y) / TOTAL };
                annoTxt.Add(pt, types[i]);
                //ele = gh.DrawTxt(txtpoint, types[i], fontsize);
                //eles.Add(ele);
            }

        }

        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, string pietitle, string staticsnum, Dictionary<IPoint, string> annoTxt, string labetype, int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double size = curms * 1.0e-3 * markersize;
            double fontsize = 0;
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
                if (labetype == "图例式标注")
                {
                    double strwidth = gh.GetStrWidth(k.Value, curms, fontsize); ;
                    x = k.Key.X * total + pAnchor.X + strwidth / 2.0;
                }
                else
                {
                    x = k.Key.X * total + pAnchor.X;
                }
                double y = k.Key.Y * total + pAnchor.Y;
                pt.PutCoords(x, y);
                InsertAnnoFea(pt, k.Value, fontsize * 0.8, id);
            }

            //4.绘制统计标注
            if (staticsnum != "")
            {
                InsertAnnoFea(pAnchor, staticsnum, fontsize * 0.5, id);
            }
            //5.绘制标题
            if (pietitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X, Y = pAnchor.Y + height / 2.0 + height / 10 };
                InsertAnnoFea(pt, pietitle, fontsize * 1.5, id);
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
                int indexId = annoFcl.FindField("ObjID");
                IAnnotationFeature pAnnoFeature = pFeature as IAnnotationFeature;
                pAnnoFeature.Annotation = pElement;
                pFeature.set_Value(indexId, id);
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
                HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter
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

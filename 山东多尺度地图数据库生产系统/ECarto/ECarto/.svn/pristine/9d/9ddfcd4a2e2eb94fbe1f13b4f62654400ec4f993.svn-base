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
    [DataContract]
    public class PieJson
    {
        [DataMember(Order = 0, IsRequired = true)]
        public List<ThematicColor> Colors;
        [DataMember(Order = 1)]
        public string Title;
        [DataMember(Order = 2)]
        public string LabelInfo;
        [DataMember(Order = 3)]
        public double Size;
        [DataMember(Order = 4)]
        public bool TotalLable;
        [DataMember(Order = 5)]
        public double RingRate;
        [DataMember(Order = 6)]
        public double EllipseRate;
        [DataMember(Order = 7)]
        public bool GeoRalated;
        [DataMember(Order = 8)]
        public string LayerName;
        [DataMember(Order = 9)]
        public string DataSource;
        [DataMember(Order = 10)]
        public string ThematicType;
    }
    [DataContract]
    public class ThematicColor
    {
        [DataMember(Order = 0, IsRequired = true)]
        public int C;
        [DataMember(Order = 1)]
        public int M;
        [DataMember(Order = 2)]
        public int Y;
        [DataMember(Order = 3)]
        public int K;
        [DataMember(Order = 4)]
        public string ColorName;
    }
    [DataContract]
    public class PieSize
    {
        [DataMember(Order = 0, IsRequired = true)]
        public double Max;
        [DataMember(Order = 1)]
        public double Min;

    }

    //根据属性绘制饼状图
    public class PieHelper
    {
        GraphicsHelper gh = null;
        private ILayer pRepLayer = null;
        private ILayer annoly = null;
        IFeatureClass annoFcl = null;

        double TOTAL;//总比值
        double lgVal;//图例间隔
        IActiveView pAc=null;
        private double linewidth = 0.75;

        private double markerSize = 20;
        double mapScale = 1000;
        List<IElement> eles = new List<IElement>();
        public List<IElement> Draw3DPieStatic(PieJson PieInfo, IPoint _centerpoint, double width = 5)
        {
            IPolygon pEllipse = null;
            ITopologicalOperator pTo = null;
            pAc = GApplication.Application.ActiveView;
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

            //PieInfo相关信息
            markerSize = PieInfo.Size;
            string labetype = PieInfo.LabelInfo;
            //noteLableType = labetype;
            bool enablePieVals = PieInfo.TotalLable;
            string chartTitle = PieInfo.Title;
            List<ICmykColor> cmykColors = new List<ICmykColor>();
            foreach (var c in PieInfo.Colors)
            {
                var cmyk = new CmykColorClass();
                cmyk.Cyan = c.C;
                cmyk.Yellow = c.Y;
                cmyk.Magenta = c.M;
                cmyk.Black = c.K;
                cmykColors.Add(cmyk);
            }
            var multiDatas = JsonHelper.CHDataSource(PieInfo.DataSource);
            WaitOperation wo = GApplication.Application.SetBusy();


            IPoint centerpoint = new PointClass() { X = 0, Y = 0 };
            foreach (var kvCharts in multiDatas)
            {
                eles.Clear();
                #region
                Dictionary<string, double> dicvals = kvCharts.Value;
                double total = 0;
                foreach (var kv in dicvals)
                {
                    total += kv.Value;
                }

                double ellipseRate = PieInfo.EllipseRate;
                // 构造椭圆
                double a = 40 * mapScale / 1000;
                double b = 40 / ellipseRate * mapScale / 1000;
                pEllipse = gh.ConsturctEllipse(centerpoint, a, b);
                pTo = pEllipse as ITopologicalOperator;
                pTo.Simplify();

                IGeometry pEllipseRing = null;
                // 构造椭圆环下 
                pEllipseRing = ContrustEllipseRing(pEllipse, width);

                DrawEllipseRing(Math.PI / 6, pEllipseRing, centerpoint);

                List<IPoint> paths = new List<IPoint>();

                #region 创建切割点
                double step = 0;
                double r = a + b;//半径
                IPoint orignPoint = new PointClass() { X = centerpoint.X, Y = centerpoint.Y + r };
                paths.Add(orignPoint);
                List<double> centerAngle = new List<double>();//每个弧中点角度
                List<double> percentage = new List<double>();//每个弧比例
                foreach (var kv in dicvals)
                {
                    centerAngle.Add((step + kv.Value / 2) / total * Math.PI * 2);
                    step += kv.Value;
                    double angle = step / total * Math.PI * 2;
                    percentage.Add(Math.Round(kv.Value / total * 100, 1));
                    IPoint leftPoint = getPoint(angle, r, centerpoint);
                    paths.Add(leftPoint);

                }
                #endregion
                IGeometryCollection pPolyline = new PolylineClass();
                IPointCollection pCl = new PathClass();
                IGeometry pleftGeo = null;
                IGeometry prightGeo = null;
                #region 创建扇形：绘制
                int ct = 0;

                foreach (var kv in dicvals)
                {
                    IPoint point1 = paths[ct];
                    IPoint point2 = paths[ct + 1];
                    #region
                    pPolyline = new PolylineClass();
                    pCl = new PathClass();
                    pCl.AddPoint(point1);
                    pCl.AddPoint(centerpoint);
                    pCl.AddPoint(point2);
                    pPolyline.AddGeometry(pCl as IGeometry);
                    pleftGeo = null;
                    prightGeo = null;

                    (pPolyline as ITopologicalOperator).Simplify();
                    pTo.Cut(pPolyline as IPolyline, out pleftGeo, out prightGeo);
                    DrawFans(pleftGeo, cmykColors[ct]);

                    #endregion
                    ct++;
                }
                #endregion
                ct = 0;
                Draw3DPie.annoTxt.Clear();//
                double totalheight = pEllipse.Envelope.Height + width;
                double lgheight = totalheight / (dicvals.Count + (dicvals.Count - 1) * 0.8);
                TOTAL = (dicvals.Count - 1) * 1.8 * lgheight;
                lgVal = 1.8 * lgheight;
                //标注相关
                if (labetype == "图例式标注")
                {
                    DrawPieNoteLengend(PieInfo, cmykColors, pEllipse, width, dicvals);
                }
                else if (labetype == "引线式标注")
                {
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteLine(PieInfo, centerAngle[ct], pEllipse, txt + ":" + percentage[ct] + "%");
                        ct++;
                    }
                }
                else if (labetype == "压盖式标注")
                {
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteOverlap(PieInfo, centerAngle[ct], pEllipse, txt + ":" + percentage[ct] + "%");
                        ct++;
                    }
                }
                //获取统计数据
                string stics = "";
                if (enablePieVals)
                {
                    stics = DrawStaticPieVals(dicvals);
                }

                //数据源
                var piedata = new Dictionary<string, Dictionary<string, double>>();
                piedata[kvCharts.Key] = kvCharts.Value;
                string jsdata = JsonHelper.JsonChartData(piedata);
                PieInfo.DataSource = jsdata;
                string jsonText = JsonHelper.GetJsonText(PieInfo);
                int obj = 0;

                var remarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, _centerpoint, jsonText, out obj, markerSize);
                CreateAnnotion(remarker, _centerpoint, markerSize, chartTitle, stics, Draw3DPie.annoTxt, labetype, obj);
                #endregion



            }
            pAc.Refresh();
            wo.Dispose();
            return eles;
        }
        #region 绘制3d饼图
        private  IGeometry ContrustEllipseRing(IPolygon pEllipse, double width)
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
        private  IPoint getPoint(double angle, double r, IPoint center)
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
            //pgeo.Project(pAc.FocusMap.SpatialReference);
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Style = esriSimpleFillStyle.esriSFSNull;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();
            smline.Style = esriSimpleLineStyle.esriSLSSolid;
            smline.Width = linewidth * 0.5;
            smline.Color = new RgbColorClass() { Red = 220, Blue = 220, Green = 220 };
            smsymbol.Outline = smline;
            polygonElement.Symbol = smsymbol;
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;

            eles.Add(pEl);

        }
        //绘制扇形
        private  void DrawFans(IGeometry pgeo, IColor pcolor)
        {
            IElement pEl = null;
            //pgeo.Project(pAc.FocusMap.SpatialReference);
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IFillShapeElement polygonElement = new PolygonElementClass();
            polygonElement.Symbol = GetColorSymbol(pcolor);
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;

            eles.Add(pEl);

        }
        private ISimpleFillSymbol GetColorSymbolPie(IColor pcolor)
        {
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Color = pcolor;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();

            smline.Width = linewidth;
            smline.Color = pcolor;
            smsymbol.Outline = smline;
            return smsymbol;
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

        #region 注记
        IPoint centerpoint = new PointClass() { X = 0, Y = 0 };
        public void DrawPieNoteLine(PieJson PieInfo, double angle, IGeometry pgeo, string txt)
        {
            IPoint edgepoint = GetEllipsePoint(angle, pgeo);
            //(edgepoint as IGeometry).Project(pAc.FocusMap.SpatialReference);
            DrawPieNote(PieInfo, angle, edgepoint, txt, centerpoint);
        }
          //根据角度获取椭圆的点
        private IPoint  GetEllipsePoint(double ange,IGeometry pEllipse)
        {
            double r = pEllipse.Envelope.Width / 2 + pEllipse.Envelope.Height / 2;
           
            IPoint center = (pEllipse as IArea).LabelPoint;
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

        private void DrawPieNote(PieJson PieInfo, double angle, IPoint pArc, string txt, IPoint center, double r = 5)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IPoint p = new PointClass();
            IPoint other = new PointClass();
            IPoint other2 = new PointClass();
            double a=Math.Abs(pArc.Y-center.Y)/Math.Abs(pArc.X-center.X);
            double b=Math.Atan(1/a);
            a=Math.Atan(a);
            angle = angle / Math.PI * 180;
            double slopeAngle = Math.PI / 6;
            double R = 30;
            
            double txtunit = 1.0;//1号字体宽度
            double txthalflen = gh.GetStrLen(txt) / 4;
            double fontsize = 13;
            double txtwidth = txthalflen * txtunit * fontsize;
            if (angle <= 90)
            {
               
                double edgeX = center.X+(r + 40) * mapScale * 1e-3; 
                double dis = edgeX - pArc.X;
                double y = dis * Math.Tan(slopeAngle);


                p.PutCoords(edgeX, pArc.Y + y);
                other.PutCoords(p.X + R * mapScale * 1e-3, p.Y);

                
               
            }
            else if (angle > 90 && angle <= 180)
            {
               
                double edgeX = center.X +(r + 40) * mapScale * 1e-3;
                double dis =Math.Abs( edgeX - pArc.X);
                double y = dis * Math.Tan(slopeAngle);


                p.PutCoords(edgeX, pArc.Y -y);
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
            else if (angle >270 && angle <= 360)
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
            IPoint pt = new PointClass() { X = cx, Y = cy };
            if (PieInfo.ThematicType == "3D饼图")
            {
                Draw3DPie.annoTxt.Add(pt, txt);
            }
            if (PieInfo.ThematicType == "3D环状饼图")
            {
                Draw3DRingPie2.annoTxt.Add(pt, txt);
            }
            if (PieInfo.ThematicType == "3D圆饼图")
            {
                Draw3DCircelPie2.annoTxt.Add(pt, txt);
            }
           
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
        #region 2.覆盖式标注
        public void DrawPieNoteOverlap(PieJson PieInfo, double angle, IGeometry geo, string txt)
        {
            IPoint center = (geo as IArea).Centroid;
            IPoint edgepoint = GetEllipsePoint(angle, geo);
            IPoint txtpoint = new PointClass() { X = (center.X + edgepoint.X) / 2, Y = (center.Y + edgepoint.Y) / 2 };
            //double txtlen = gh.GetStrLen(txt) / 4 * 12;
            //txtpoint.X -= txtlen;
            IPoint txtpoint2 = new PointClass() { X = (txtpoint.X - center.X) / TOTAL, Y = (txtpoint.Y - center.Y) / TOTAL };
            if (PieInfo.ThematicType == "3D饼图")
            {
                Draw3DPie.annoTxt.Add(txtpoint2, txt);
            }
            if (PieInfo.ThematicType == "3D环状饼图")
            {
                Draw3DRingPie2.annoTxt.Add(txtpoint2, txt);
            }
            if (PieInfo.ThematicType == "3D圆饼图")
            {
                Draw3DCircelPie2.annoTxt.Add(txtpoint2, txt);
            }
        }
        #endregion
        #region 3.图例式注记    
        private void DrawPieNoteLengend(PieJson PieInfo, List<ICmykColor> cmykColors, IGeometry pgeo, double height, Dictionary<string, double> data)
        {
            height = mapScale * height * 1.0e-3;


            string[] types = new string[data.Count];
            int t=0;
            foreach (var kv in data)
            {
                types[t++] = kv.Key;
            }
            double txtunit = 1.0;//1号字体宽度
       
            GraphicsHelper gh = new GraphicsHelper(pAc);
            int ct = data.Count;
            double totalheight = pgeo.Envelope.Height + height;

            IPoint basepoint = new PointClass() { X = centerpoint.X + pgeo.Envelope.Width / 2 + height, Y = centerpoint.Y + pgeo.Envelope.Height / 2 };
            //获取每个图列的高度
            double lgheight = totalheight / (ct + (ct - 1) * 0.8);
            for(int i=0;i<ct;i++)
            {
                IPoint upleft=new PointClass();
                double y = basepoint.Y - i * (1.8) * lgheight;
                upleft.PutCoords(basepoint.X, y);
                IPolygon prect = gh.CreateRectangle(upleft, 4.0/3 * lgheight, lgheight);
            
                double fontsize = 4.4;
                double strwidth = gh.GetStrLen(types[i]) * txtunit * (mapScale / 10000) * fontsize/2;
                var ele=  gh.DrawPolygon(prect, cmykColors[i],0);
                eles.Add(ele);
                double cx = (upleft.X + 2 * height + lgheight / 2.0 - centerpoint.X) / TOTAL;
                double cy = (upleft.Y - lgheight * 0.5 - centerpoint.Y) / TOTAL;
                IPoint pt = new PointClass() { X = cx, Y = cy };

                if (PieInfo.ThematicType == "3D饼图")
                {
                    Draw3DPie.annoTxt.Add(pt, types[i]);
                }
                if (PieInfo.ThematicType == "3D环状饼图")
                {
                    Draw3DRingPie2.annoTxt.Add(pt, types[i]);
                }
                if (PieInfo.ThematicType == "3D圆饼图")
                {
                    Draw3DCircelPie2.annoTxt.Add(pt, types[i]);
                }
            }

        }
     
       
        #endregion
        #endregion
        #endregion

        #region 绘制3d环状图
        //绘制3d环状图
        public List<IElement> Draw3DRingStatic(PieJson PieInfo, IPoint _centerpoint, double width = 3.5)
        {

            IPolygon pEllipse = null;
            ITopologicalOperator pTo = null;
            double ringrate = 0.4;
            double a = 40 * mapScale / 1000;
            double b = 20 * mapScale / 1000;
            pAc = GApplication.Application.ActiveView;
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
            //获取颜色设置

            markerSize = PieInfo.Size;
            string labetype = PieInfo.LabelInfo;
            bool enablePieVals = PieInfo.TotalLable;
            string chartTitle = PieInfo.Title;
            ringrate = PieInfo.RingRate;
            //长短轴比率
            double abrate = PieInfo.EllipseRate;
            if (abrate > 0)
            {
                b = a / abrate;
            }
            List<ICmykColor> cmykColors = new List<ICmykColor>();
            foreach (var c in PieInfo.Colors)
            {
                var cmyk = new CmykColorClass();
                cmyk.Cyan = c.C;
                cmyk.Yellow = c.Y;
                cmyk.Magenta = c.M;
                cmyk.Black = c.K;
                cmykColors.Add(cmyk);
            }
            var multiDatas = JsonHelper.CHDataSource(PieInfo.DataSource);
            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("正在处理...");
            List<double> centerAngle = new List<double>();//每个弧中点角度
            List<double> percentage = new List<double>();//每个弧比例
            foreach (var kvCharts in multiDatas)
            {
                eles.Clear();
                centerAngle.Clear();//每个弧中点角度
                percentage.Clear();//每个弧比例
                #region
                Dictionary<string, double> dicvals = kvCharts.Value;

                double total = getStaticData(dicvals);

                //构造椭圆
                pEllipse = gh.ConsturctEllipse(centerpoint, a, b);
                var pEllipseClone = (pEllipse as IClone).Clone() as IPolygon;
                IGeometry pEllipseRing = null;//结果2
                double dheight = width * 1e-3 * mapScale;
                // 构造椭圆环 下
                pEllipseRing = ContrustEllipseRing(pEllipse, width);
                var pEllipseIn = gh.ConsturctEllipse(centerpoint, a * ringrate, b * ringrate);
                IPolygon pEllipseInRing = null;//结果3
                //  构造椭圆环 内
                pEllipseInRing = ConstructInnerRing(centerpoint, a, b, ringrate);


                //绘制
                pTo = pEllipse as ITopologicalOperator;
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
                Draw3DRingPie2.annoTxt.Clear();
                double totalheight = pEllipse.Envelope.Height + width;
                double lgheight = totalheight / (dicvals.Count + (dicvals.Count - 1) * 0.8);
                TOTAL = (dicvals.Count - 1) * 1.8 * lgheight;
                lgVal = 1.8 * lgheight;

                ct = 0;
                if (labetype == "图例式标注")
                {
                    DrawPieNoteLengend(PieInfo, cmykColors, pEllipse, width, dicvals);

                    //DrawPieNoteLengend(width, dicvals);
                }
                else if (labetype == "引线式标注")
                {
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteLine(PieInfo, centerAngle[ct], pEllipse, txt + ":" + percentage[ct] + "%");
                        ct++;
                    }
                }
                else if (labetype == "压盖式标注")
                {
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteOverlap(PieInfo, centerAngle[ct], pEllipse, txt + ":" + percentage[ct] + "%");
                        ct++;
                    }
                }

                //获取统计数据
                string stics = "";
                if (enablePieVals)
                {
                    stics = DrawStaticPieVals(dicvals);
                }

                //数据源
                var piedata = new Dictionary<string, Dictionary<string, double>>();
                piedata[kvCharts.Key] = kvCharts.Value;
                string jsdata = JsonHelper.JsonChartData(piedata);
                PieInfo.DataSource = jsdata;
                string jsonText = JsonHelper.GetJsonText(PieInfo);
                int obj = 0;

                var remarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, _centerpoint, jsonText, out obj, markerSize);
                CreateAnnotion(remarker, _centerpoint, markerSize, chartTitle, stics, Draw3DRingPie2.annoTxt, labetype, obj);

                #endregion


            }
            pAc.Refresh();
            wo.Dispose();
            return eles;

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
            
            DrawEllipseRing(ellipsering);
        }
     
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
     
        private double getStaticData(Dictionary<string, double> dicvals)
        {
            double total = 0;
            foreach (var kv in dicvals)
            {
                total += kv.Value;
            }

            return total;
        }
        #endregion

        #region 绘制3d圆饼图
        List<IPoint> arcPoint = new List<IPoint>();//每个弧注记中点
        List<double> centerAngle = new List<double>();//每个弧中点角度
        List<double> percentage = new List<double>();//每个弧比例
        public List<IElement> Draw3DCirclePie(PieJson PieInfo, IPoint _centerpoint, double width = 5)
        {
            linewidth = 0.1;
            arcPoint.Clear(); centerAngle.Clear(); percentage.Clear();
            double moveR = 4 * mapScale / 1000;

            IPolygon pEllipse = null;
            ITopologicalOperator pTo = null;

            pAc = GApplication.Application.ActiveView;
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

            //获取数据
            markerSize = PieInfo.Size;
            string labetype = PieInfo.LabelInfo;
            bool enablePieVals = PieInfo.TotalLable;
            string chartTitle = PieInfo.Title;
            List<ICmykColor> cmykColors = new List<ICmykColor>();
            foreach (var c in PieInfo.Colors)
            {
                var cmyk = new CmykColorClass();
                cmyk.Cyan = c.C;
                cmyk.Yellow = c.Y;
                cmyk.Magenta = c.M;
                cmyk.Black = c.K;
                cmykColors.Add(cmyk);
            }
            var multiDatas = JsonHelper.CHDataSource(PieInfo.DataSource);

            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("正在处理...");
            foreach (var kvCharts in multiDatas)
            {

                #region
                Dictionary<string, double> dicvals = kvCharts.Value;
                double total = getStaticData(dicvals);

                // 构造椭圆
                double a = 40 * mapScale / 1000;
                pEllipse = gh.ConsturctEllipse(centerpoint, a, a);
                var pEllipseClone = (pEllipse as IClone).Clone() as IPolygon;
                pTo = pEllipse as ITopologicalOperator;
                pTo.Simplify();
                // 构造椭圆环
                var pEllipseRing = ContrustEllipseRing(pEllipse, width);
                List<IPoint> paths = new List<IPoint>();
                List<IPoint> circelpaths = new List<IPoint>();
                List<IPoint> circelpaths1 = new List<IPoint>();
                //创建切割点
                CreateClipPoint(pEllipseRing, a, dicvals, ref paths, ref circelpaths, ref circelpaths1);
                IGeometryCollection pPolyline = new PolylineClass();
                IPointCollection pCl = new PathClass();
                IGeometry pleftGeo = null;
                IGeometry prightGeo = null;
                #region 创建扇形：绘制
                int ct = 0; double step = 0; int flagNum = 0;//最后一个环
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
                #endregion
                ct = 0;
                //绘制标注
                Draw3DCircelPie2.annoTxt.Clear();
                double totalheight = pEllipse.Envelope.Height + width;
                double lgheight = totalheight / (dicvals.Count + (dicvals.Count - 1) * 0.8);
                TOTAL = (dicvals.Count - 1) * 1.8 * lgheight;
                lgVal = 1.8 * lgheight;
                if (labetype == "图例式标注")
                {
                    DrawPieNoteLengend(PieInfo, cmykColors, pEllipse, width + 5, dicvals);

                }
                else if (labetype == "引线式标注")
                {
                    linewidth = 0.75;
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteLine(PieInfo, centerAngle[ct], pEllipse, txt + ":" + percentage[ct] + "%");
                        ct++;
                    }

                }
                else if (labetype == "压盖式标注")
                {
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteOverlap(PieInfo, centerAngle[ct], pEllipse, txt + ":" + percentage[ct] + "%");
                        ct++;
                    }
                }
                //获取统计数据
                string stics = "";
                if (enablePieVals)
                {
                    stics = DrawStaticPieVals(dicvals);
                }

                //数据源
                var piedata = new Dictionary<string, Dictionary<string, double>>();
                piedata[kvCharts.Key] = kvCharts.Value;
                string jsdata = JsonHelper.JsonChartData(piedata);
                PieInfo.DataSource = jsdata;
                string jsonText = JsonHelper.GetJsonText(PieInfo);
                int obj = 0;

                var remarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, _centerpoint, jsonText, out obj, markerSize);
                CreateAnnotion(remarker, _centerpoint, markerSize, chartTitle, stics, Draw3DCircelPie2.annoTxt, labetype, obj);

                #endregion
            }
            pAc.Refresh();
            wo.Dispose();
            return eles;
        }
    

        private void DrawFans(IGeometry pgeo, IColor pcolor, double sx, double sy)
        {
            ITransform2D ptrans2d = pgeo as ITransform2D;
            ptrans2d.Move(sx, sy);
            IElement pEl = null;
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IFillShapeElement polygonElement = new PolygonElementClass();
            polygonElement.Symbol = GetColorSymbolPie(pcolor);
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;

            eles.Add(pEl);

        }
        //绘制扇形变
        private IPolygon CreateFansEdge(IPoint center, IPoint f, IPoint movepoint, ICmykColor pcolor, IPoint f1)
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
                polygonElement.Symbol = GetColorSymbolPie(pcmyk);
                pEl = polygonElement as IElement;
                pEl.Geometry = pPolygon as IGeometry;
                eles.Add(pEl);


            }
            return pTrans2d as IPolygon;
        }
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

        private void CreateClipPoint(IGeometry pEllipseRing, double a, Dictionary<string, double> dicvals, ref List<IPoint> paths, ref List<IPoint> circelpaths, ref  List<IPoint> circelpaths1)
        {
            double step = 0;
            double r = 2 * a;//半径
            double ridia = a;
            IPoint orignPoint = new PointClass() { X = centerpoint.X, Y = centerpoint.Y + r };
            paths.Add(orignPoint);
            circelpaths.Add(new PointClass() { X = centerpoint.X, Y = centerpoint.Y + ridia });
            circelpaths1.Add(new PointClass() { X = centerpoint.X, Y = centerpoint.Y + ridia });
            double total = getStaticData(dicvals);
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
        #endregion

        #region 新增方法
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

        //文字转注记
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
                    double strwidth = gh.GetStrWidth(k.Value, curms, fontsize);
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
                InsertAnnoFea(pAnchor, staticsnum, fontsize * 0.8, id);
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
            IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize);
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annoFcl.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID; ;
            pAnnoFeature.LinkedFeatureID = id;
            pAnnoFeature.Annotation = pElement;
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
    }
}

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
    /// ���Ʊ�ͼ
    /// </summary>
   
    public sealed class Draw3DPie
    {
        
      
        private IActiveView pAc = null;
        private double mapScale = 1000;
        private double linewidth = 0.283;
        private string chartName = "��ά��ͼ";
        private string chartType = "��ͼ";
        private ILayer pRepLayer = null;
        private ILayer annoly = null;
        IFeatureClass annoFcl = null;
        GraphicsHelper gh = null;
        public static Dictionary<IPoint, string> annoTxt = new Dictionary<IPoint, string>();//����ע��
        public static Dictionary<IPoint, string> staticTxt = new Dictionary<IPoint, string>();//ͳ������ע��
        public static Dictionary<int, IPoint> staticPoints = new Dictionary<int, IPoint>();//ͳ�Ƽ���
        double TOTAL;//�ܱ�ֵ
        double lgVal;//ͼ�����
        string labeType = string.Empty;
        public Draw3DPie(IActiveView pac, double ms)
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
        private List<IElement> eles = new List<IElement>();
        IPoint _centerpoint = null;
        IPoint centerpoint = new PointClass() { X = 0, Y = 0 };
        IPoint cpClone = null;
        bool enablePieVals = false;
        double markerSize = 20;
        double markerSizeMax = 20;
        string noteLableType = "";
        /// <summary>
        /// �������ú���
        /// </summary>
        public  void Draw3DPieCharts(FrmPieChartsSet frm,IPoint basepoint)
        {
            CommonMethods.ClearThematicCarto(basepoint, (pRepLayer as IFeatureLayer).FeatureClass, annoFcl);
            _centerpoint = basepoint;
            cpClone = (basepoint as IClone).Clone() as IPoint;
            eles.Clear();
            Draw3DPieStatic(frm);
        }
        #region �������
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
        double a = 40;//��Բ������
        double b = 20;//��Բ�̰���
        List<double> centerAngle = new List<double>();//ÿ�����е�Ƕ�
        List<double> percentage = new List<double>();//ÿ��������
        //����3d��ͼ      
        private void Draw3DPieStatic(FrmPieChartsSet frm,double width = 5)
        {
            centerAngle.Clear();
            percentage.Clear();
            IPolygon pEllipse = null;
            ITopologicalOperator pTo = null;
            GraphicsHelper gh = new GraphicsHelper(pAc);
        
            //��ȡ��ɫ������
            PieJson PieInfo = frm.PieInfo;
            string labetype = frm.LableType;
            noteLableType = labetype;
            labeType = labetype;
            enablePieVals=frm.EnablePieVals;
            string chartTitle = frm.ChartTitle;
            markerSize = frm.MarkerSize;
            markerSizeMax = frm.MarkerSizeMax;
            List<ICmykColor> cmykColors = frm.CMYKColors;
            string geoRelate = frm.GeoLayer;
            var multiDatas = frm.ChartDatas;
            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("���ڴ���...");
            Dictionary<string, IPoint> namesPt = null;
            if (geoRelate != "")
            {
               namesPt = ChartsDataSource.ObtainGeoRelated(geoRelate);
            }
                //�����ͼ��С
            var sumDatalist = getMaxStaticData(multiDatas);
            double fxa = 0;
            if ((sumDatalist.Max() - sumDatalist.Min())!=0)
            {
                fxa = (markerSizeMax - markerSize) / (sumDatalist.Max() - sumDatalist.Min());
            }
            double fxb = 0;
            fxb = fxa * sumDatalist.Min() + markerSize;
         
            int flagNum = 0;

            foreach (var kvCharts in multiDatas)
            {
                flagNum++;
                string msg = string.Format("���ڴ�����..{0}/{1}", flagNum, multiDatas.Count);
                wo.SetText(msg);
                eles.Clear();
              
                #region
                string name = kvCharts.Key;
                if (geoRelate != "")
                {
                    if(namesPt.ContainsKey(name))
                        _centerpoint = namesPt[name];
                }
                Dictionary<string, double> dicvals = kvCharts.Value;

                double total = getStaticData(dicvals);
              
             
                double ellipseRate = frm.EllipseRate;
                // ������Բ
                a = 40 * mapScale / 1000;
                b = 40 / ellipseRate * mapScale / 1000;
                pEllipse = gh.ConsturctEllipse(centerpoint, a, b);
                pTo = pEllipse as ITopologicalOperator;
                pTo.Simplify();
             
                IGeometry pEllipseRing = null;
                // ������Բ���� 
                pEllipseRing = ContrustEllipseRing(pEllipse, width);

                DrawEllipseRing(Math.PI / 6, pEllipseRing, centerpoint);

                List<IPoint> paths = new List<IPoint>();

                #region �����и��
                double step = 0;
                double r = a + b;//�뾶
                IPoint orignPoint = new PointClass() { X = centerpoint.X, Y = centerpoint.Y + r };
                paths.Add(orignPoint);
            
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
                #region �������Σ�����
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
                    staticPoints[ct] = (pleftGeo as IArea).LabelPoint;
                    #endregion                  
                    ct++;
                }
                #endregion
                //��ע���
                #region
                ct = 0;
                annoTxt.Clear();
                staticTxt.Clear();
                double totalheight = pEllipse.Envelope.Height + width;
                double lgheight = totalheight / (dicvals.Count + (dicvals.Count - 1) * 0.8);
                TOTAL = (dicvals.Count - 1) * 1.8 * lgheight;
                lgVal = 1.8 * lgheight;


                if (labetype == "ͼ��ʽ��ע")
                {
                    DrawPieNoteLengend(cmykColors, pEllipse, width, dicvals);
                }
                else if(labetype =="����ʽ��ע")
                {
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteLine(centerAngle[ct], pEllipse, txt+":"+percentage[ct]+"%");
                        DrawPieAnnotation(ct, kv.Value.ToString());
                        ct++;
                    }
                }
                else if (labetype=="ѹ��ʽ��ע")
                {
                    foreach (var kv in dicvals)
                    {
                        string txt = kv.Key;
                        DrawPieNoteOverlap(ct, txt+":"+percentage[ct]+"%");
                        ct++;
                    }
                }
                #endregion
                double piesize = total * fxa + fxb;

                #region
                //��ȡͳ������
                string stics = "";
                if (enablePieVals)
                {
                   stics = DrawStaticPieVals(dicvals);
                }
                //����Ҫ��
                //����Դ
                var piedata = new Dictionary<string, Dictionary<string, double>>();
                piedata[kvCharts.Key] = kvCharts.Value;
                string jsdata = JsonHelper.JsonChartData(piedata);
                PieInfo.DataSource = jsdata;
                //�ߴ�
                PieInfo.Size = piesize;
                PieInfo.ThematicType = "3D��ͼ";
                string jsonText = JsonHelper.GetJsonText(PieInfo);
                int obj = 0;
                //������ɫͼ����
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
            MessageBox.Show("�������");
        }
        public void DrawPieAnnotation(int index, string txt)
        {
            IPoint txtpoint2 = staticPoints[index];
            staticTxt.Add(txtpoint2, txt);

        }
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
                //��������
                IEnvelope penv = ellipsering.Envelope;
                penv.Expand(2, 2, true);
                penv.XMax = pinsect.FromPoint.X;

                (left as ITopologicalOperator).Clip(penv);
                penv = ellipsering.Envelope;
                penv.Expand(2, 2, true);
                penv.XMin = pinsect.FromPoint.X;
                (right as ITopologicalOperator).Clip(penv);
                
            }
            //����
            IRgbColor rgb = new RgbColorClass();
            rgb.Red = 133;
            rgb.Green = 133;
            rgb.Blue = 133;
            IRgbColor rgb1 = new RgbColorClass();
            rgb1.Red = 253;
            rgb1.Green = 253;
            rgb1.Blue = 253;

            
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            //�ұ�
            IGradientFillSymbol fillsym = gh.CreateGradientSym(rgb1, rgb);
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            polygonElement.Symbol = fillsym;
            pEl = polygonElement as IElement;
            pEl.Geometry = left as IGeometry;

        
            eles.Add(pEl);
            //���
            fillsym = gh.CreateGradientSym(rgb, rgb1);
          
            polygonElement = new PolygonElementClass();
            polygonElement.Symbol = fillsym;
            pEl = polygonElement as IElement;
            pEl.Geometry = right as IGeometry;
            eles.Add(pEl);
          
            //���
           
            DrawEllipseRing(ellipsering);
        }
        //�и
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
                    //�и���
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
        //���ƻ�
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
            smline.Width = linewidth*0.5;
            smline.Color = new RgbColorClass() { Red = 220, Blue = 220, Green = 220 };
            smsymbol.Outline = smline;
            polygonElement.Symbol = smsymbol;
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
         
            eles.Add(pEl);
           
        }
        //��������
        private void DrawFans(IGeometry pgeo, IColor pcolor)
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
        /// ������Բ��
        /// </summary>
        /// <param name="pEllipse">��Բ</param>
        /// <param name="width">���߶�</param>
        /// <returns></returns>
        private IGeometry ContrustEllipseRing(IPolygon pEllipse, double width)
        {
            IGeometry pEllipseRing = null;
            try
            {
                GraphicsHelper gh = new GraphicsHelper(pAc);
                IEnvelope pEnvelope=pEllipse.Envelope;
                IPoint centerpoint = new PointClass() { X = (pEnvelope.XMin + pEnvelope.XMax) / 2, Y = (pEnvelope.YMin + pEnvelope.YMax) / 2 };

                //1.ƽ��
                IPolygon cloneEllipse = (pEllipse as IClone).Clone() as IPolygon;
                ITransform2D ellipseTrans = cloneEllipse as ITransform2D;
                ellipseTrans.Move(0, -width * 1e-3 * mapScale);
                //2 �������
                double recwidth = pEnvelope.Width;
                double recheight = width * 1e-3 * mapScale;
                IPoint upleft = new PointClass() { X = centerpoint.X - recwidth / 2, Y = centerpoint.Y };
                IPolygon pRect = gh.CreateRectangle(upleft, recwidth, recheight);
                //2.��ȡ�°���Բ
                IGeometryCollection pPolyline = new PolylineClass();
                IPointCollection pCl = new PathClass();
                pCl.AddPoint(new PointClass() { X = upleft.X, Y = upleft.Y - recheight });
                pCl.AddPoint(new PointClass() { X = upleft.X + recwidth, Y = upleft.Y - recheight });
                pPolyline.AddGeometry(pCl as IGeometry);
                IGeometry pleftGeo = null;
                IGeometry prightGeo = null;
                (cloneEllipse as ITopologicalOperator).Cut(pPolyline as IPolyline, out pleftGeo, out prightGeo);
                //3.���Σ��°���Բ�ϲ�
                IGeometry pgeo1 = (prightGeo as ITopologicalOperator).Union(pRect as IGeometry);
                //4.��Բ��
                pEllipseRing = (pgeo1 as ITopologicalOperator).Difference(pEllipse as IGeometry);

                (pEllipseRing as ITopologicalOperator).Simplify();
                return pEllipseRing as IGeometry;
            }
            catch
            {
                return null;
            }
        }
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
        #region ��ע���
        #region 1.���߱�ע
        public void DrawPieNoteLine(double angle, IGeometry pgeo, string txt)
        {
            IPoint edgepoint = GetEllipsePoint(angle, pgeo);
            DrawPieNote(angle, edgepoint, txt, centerpoint);
        }
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
        private void DrawPieNote(double angle, IPoint pArc,string txt,IPoint center, double r = 5)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IPoint p = new PointClass();
            IPoint other = new PointClass();
            double a=Math.Abs(pArc.Y-center.Y)/Math.Abs(pArc.X-center.X);
            double b=Math.Atan(1/a);
            a=Math.Atan(a);
            angle = angle / Math.PI * 180;
            double slopeAngle = Math.PI / 6;
            double R = 30;
            //double txtheight = 1 * mapScale * 1e-3;
            double txtunit = 1.0;//1��������
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
            //other.Y += 5;
            double cx = (other.X - center.X) / TOTAL;
            double cy = (other.Y - center.Y) / TOTAL;
            cx = (p.X + other.X) * 0.5;
            cy = (p.Y + other.Y) * 0.5;
            IPoint pt = new PointClass() { X = cx, Y = cy };
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
        #region 2.����ʽ��ע
        public void DrawPieNoteOverlap(int index, string txt)
        {
            //IPoint center = (geo as IArea).Centroid;
            //IPoint edgepoint = GetEllipsePoint(angle, geo);
            //IPoint txtpoint =new PointClass() {X=(center.X+edgepoint.X)/2,Y=(center.Y+edgepoint.Y)/2 };
            //double txtlen = gh.GetStrLen(txt) / 4 * 12;
            //txtpoint.X -= txtlen;
            IPoint txtpoint2 = staticPoints[index];
          //  IPoint txtpoint2 = new PointClass() { X = (txtpoint.X - center.X) / TOTAL, Y = (txtpoint.Y - center.Y) / TOTAL };
            annoTxt.Add(txtpoint2, txt);
        }
        #endregion
        #region 3.ͼ��ʽע��    
        private void DrawPieNoteLengend(List<ICmykColor> cmykColors,IGeometry pgeo,double height,Dictionary<string,double>data)
        {
            height = mapScale * height * 1.0e-3;


            string[] types = new string[data.Count];
            int t=0;
            foreach (var kv in data)
            {
                types[t++] = kv.Key;
            }
            double txtunit = 1.0;//1��������
       
            GraphicsHelper gh = new GraphicsHelper(pAc);
            int ct = data.Count;
            double totalheight = pgeo.Envelope.Height + height;

            IPoint basepoint = new PointClass() { X = centerpoint.X + pgeo.Envelope.Width / 2 + height, Y = centerpoint.Y + pgeo.Envelope.Height / 2 };
            //��ȡÿ��ͼ�еĸ߶�
            double lgheight = totalheight / (ct + (ct - 1) * 0.8);
            for(int i=0;i<ct;i++)
            {
                IPoint upleft=new PointClass();
                double y = basepoint.Y - i * (1.8) * lgheight;
                upleft.PutCoords(basepoint.X, y);
                IPolygon prect = gh.CreateRectangle(upleft, 2*height, lgheight);
                var ele=  gh.DrawPolygon(prect, cmykColors[i],0);
                eles.Add(ele);
                double cx = (upleft.X + 2 * height + lgheight / 2.0 - centerpoint.X) / TOTAL;
                double cy = (upleft.Y - lgheight * 0.5 - centerpoint.Y) / TOTAL;
                cy = (prect as IArea).Centroid.Y;
                cx = (prect as IArea).Centroid.X + prect.Envelope.Width * 0.5 + prect.Envelope.Height * 0.5;
            
                IPoint pt = new PointClass() { X = cx, Y = cy };
                annoTxt.Add(pt, types[i]);
                DrawPieAnnotation(i, data[types[i]] + ":" + percentage[i] + "%");
            }
        }

        //����תע��
        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, string pietitle, string staticsnum, Dictionary<IPoint, string> annoTxt, string labetype, int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double size = curms * 1.0e-3 * markersize;
            double fontsize = 0; double repTransform = 1;
            //1.ȷ����ͼ���ȺͿ��
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
            //2.����ͳ��ͼʵ�ʴ�Сȷ������Ĵ�С
            double heightunit = 3.97427;//1������1��ĸ߶�
            fontsize = fontsize / (curms * heightunit * 1e-4);
            fontsize *= 2.83;
            //3.����ͼ��ע��
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
                if (labeType.Contains("����"))
                {
                    ver = esriTextVerticalAlignment.esriTVABottom;
                }
                if (labeType.Contains("ͼ��"))
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
          

            //4.����ͳ�Ʊ�ע
            if (staticsnum != "")
            {
                InsertAnnoFea(pAnchor, staticsnum, fontsize * 0.8, id);
            }
            //5.���Ʊ���
            if (pietitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X, Y = pAnchor.Y + height / 2.0 + height / 10 };
                InsertAnnoFea(pt, pietitle, fontsize * 1.5, id);
            }
        }
        private void InsertAnnoFea(IGeometry pGeometry, string annoName, double fontSize, int id, esriTextVerticalAlignment ver = esriTextVerticalAlignment.esriTVACenter, esriTextHorizontalAlignment hor = esriTextHorizontalAlignment.esriTHACenter)
        {
            IFontDisp font = new StdFont() { Name = "����", Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize,ver,hor);
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annoFcl.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID; ;
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
                VerticalAlignment =ver,
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

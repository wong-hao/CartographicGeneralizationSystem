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
    /// 多系列条形图辅助类
    /// </summary>
    public sealed class DrawBarMulit 
    {
        private IActiveView pAc = null;
        private double mapScale = 1000;
        private List<IElement> eles = new List<IElement>();
        private ILayer pRepLayer = null;
        private IFeatureClass annoFcl = null;
        GraphicsHelper gh = null;
        private string chartTitle = "";
        private IColor bgColor = null;
        string geoRelate = "";
        double markerSize = 20;
        public bool XyAxis = false;
        //绘制斜轴柱状图
        public DrawBarMulit(IActiveView pac,double ms)
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
            annoFcl = (lyr.First() as IFeatureLayer).FeatureClass;

            gh = new GraphicsHelper(pAc);
        }
        public void CreateMultiBars(IPoint centerpoint)
        {
            //获取颜色
            FrmBarChartsSet frm = new FrmBarChartsSet("簇状条形图");
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            XyAxis = frm.XYAxis;
            if (frm.GeoRelated)
            {
                CreateMultiBarsGeo(centerpoint, frm);
            }
            else
            {
                CreateMultiBarsUnGeo(centerpoint, frm);
            }
            pAc.Refresh();
            MessageBox.Show("生成完成");
        }
        public void CreateMultiBarsGeo(IPoint _centerpoint, FrmBarChartsSet frm)
        {
            //获取颜色
            Dictionary<string, Dictionary<string, double>> groupdatas = frm.ChartDatas;
            cmykColors = frm.CMYKColors;
            chartTitle = frm.ChartTitle;
            markerSize = frm.MarkerSize;
            
            double max = 0;
            string[] types = getStaticDatas(groupdatas, ref max);
            double kedu = frm.KeDu;
             geoRelate = frm.GeoLayer;
            Dictionary<string, IPoint> namesPt = ChartsDataSource.ObtainGeoRelated(geoRelate);
            foreach (var kv in groupdatas)
            {
                eles.Clear();
                IPoint centerpoint = (_centerpoint as IClone).Clone() as IPoint;
                IPoint orpt = new PointClass() { X = 0, Y = 0 };
                string name = kv.Key;
                if (geoRelate != "")
                {
                    if (namesPt.ContainsKey(name))
                        centerpoint = namesPt[name];
                }
                DrawBgEle(orpt, kv.Value.Count);
                //if (frm.XYAxis)
                {
                    DrawXYaxis(orpt, 1, kedu, types, frm.XYAxis);
                }
                DrawColumn(orpt, kv.Value, max, cmykColors);
                if (frm.GeoLengend)
                {
                    DrawLengend(orpt);
                }
                //标题
                //if (chartTitle != "")
                //{
                //    DrawBarTitle(centerpoint, chartTitle);
                //}
                //gh.CreateFeatures(eles, pRepLayer, centerpoint,frm.MarkerSize);
                int obj = 0;
                ChartsToRepHelper ch = new ChartsToRepHelper();
                var remaker = ch.CreateFeatureEX(pAc, eles, pRepLayer, centerpoint, out obj, markerSize);
               // var remaker = gh.CreateFeaturesEx(eles, pRepLayer, centerpoint, out obj, markerSize);
                createAnno(remaker, centerpoint, markerSize, chartTitle, obj);
            }
        }
        /// <summary>
        /// 绘制多系列条形图
        /// </summary>
        public void CreateMultiBarsUnGeo(IPoint centerpoint, FrmBarChartsSet frm)
        {
            eles.Clear();
            Dictionary<string, Dictionary<string, double>> groupdatas = frm.ChartDatas;
            cmykColors = frm.CMYKColors;
            chartTitle = frm.ChartTitle;
            markerSize = frm.MarkerSize;
            double max = 0;
            string[] types = getStaticDatas(groupdatas, ref max);
            double kedu = frm.KeDu;
            IPoint orpt = new PointClass() { X = 0, Y = 0 };
          //  if (frm.XYAxis)
            {
                DrawXYaxis(orpt, groupdatas.Count, kedu, types, frm.XYAxis);
            }
            DrawColumns(orpt, groupdatas, max, cmykColors);
            if (frm.GeoLengend)
            {
                DrawLengend(orpt);
            }
           
            int obj=0;
            ChartsToRepHelper ch = new ChartsToRepHelper();
            var remarker = ch.CreateFeatureEX(pAc, eles, pRepLayer, centerpoint, out obj, markerSize);
       
             createAnno(remarker, centerpoint, markerSize, chartTitle, obj);
        }
        #region

        private void DrawBgEle(IPoint point,int types)
        {
            double xdis = 40 * 1e-3 * mapScale;//一组分类数据长度
           
            double height = 4 * xdis;
          
          
            double width = 6 * xdis;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IGeometry pgeo = gh.CreateRectangle(point, width, -height);

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
        //获取系列数据
        private string[] getStaticDatas(Dictionary<string, Dictionary<string, double>> groupdatas, ref double max)
        {

            List<string> types=new List<string>();
            foreach( var kv in groupdatas)
            {
                Dictionary<string, double> dicvals = kv.Value;
                var vals = dicvals.OrderByDescending(r => r.Value);

                max = vals.First().Value > max ? vals.First().Value : max;
                types.Add(kv.Key);
               
            }
            subtypes=new List<string>();
            var data=groupdatas.First();
            foreach(var kv1 in data.Value)
            {
                subtypes.Add(kv1.Key);
            }
            return types.ToArray();
        }
        List<ICmykColor> cmykColors=null;
        List<string> subtypes=null;
       
        private void DrawBarTitle(IPoint pbasePoint,string txt)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double dis = 40 * 1e-3 * mapScale;

            IPoint txtpoint = new PointClass() { X = pbasePoint.X + 3 * dis, Y = pbasePoint.Y + 4 * dis + 1e-2 * mapScale };
            //var ele=  gh.DrawTxt(txtpoint, txt, 30);
            //eles.Add(ele);
        }
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
       ///<summary>
       ///绘制图列
       ///</summary>
       ///<param name="basepoint">坐标原点</param>
       //获取图例标注
        Dictionary<IPoint, string> annolg = new Dictionary<IPoint, string>();
        private void DrawLengend(IPoint basepoint)
        {
            annolg.Clear();
            double total = 6 * 40 * 1.0e-3 * mapScale;
            GraphicsHelper gh = new GraphicsHelper(pAc);
             //获取图列的高度
            double heightunit = 3.97427;//1号字体1万的高度
            double lgheight = heightunit * 20 * mapScale / 10000;
            int ct = subtypes.Count;
            //全部图例+文字长度
            double length = lgheight *2 * ct;//图例
            length += mapScale * 4.0e-3*(ct-1);//间隔
            foreach (string str in subtypes)
            {
                length += gh.GetStrWidth(str,mapScale,20) + mapScale * 2.0e-3 ;// ;//加文字宽度
            }
            //X轴长度
            double Xdis= 240 * 1e-3 * mapScale;
          

            double dx = Xdis / 2 - length / 2;//平移距离
            double stepX = 0;
            for (int i = 0; i < subtypes.Count; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - 1.3e-2 * mapScale;
                upleft.PutCoords(stepX+basepoint.X+dx, y);
                //tulie
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                var ele= gh.DrawPolygon(prect, cmykColors[i], 0);
                eles.Add(ele);
                stepX += lgheight * 2;
                //wenzi
              
                double fontsize = 20;
                double strwidth = gh.GetStrWidth(subtypes[i], mapScale, 20);
                stepX += mapScale * 2.0e-3;//文字间隔
                IPoint txtpoint = new PointClass() { X = mapScale * 2.0e-3 + upleft.X + strwidth / 2 + lgheight * 2, Y = upleft.Y - 0.5*lgheight };
                double cx = (txtpoint.X - basepoint.X) / total;
                double cy = (txtpoint.Y - basepoint.Y) / total;
                IPoint pt = new PointClass() { X = cx, Y = cy };
                annolg.Add(pt, subtypes[i]);
                //ele = gh.DrawTxt(txtpoint, subtypes[i], fontsize);
                //eles.Add(ele);
                stepX += strwidth + mapScale * 4.0e-3;//文字间隔;

            }
        }
        /// <summary>
        /// 绘制xy轴
        /// </summary>
        //获取x轴刻度值和实际坐标
        Dictionary<IPoint, string> Ykedu = new Dictionary<IPoint, string>();
        Dictionary<IPoint, string> annoClass = new Dictionary<IPoint, string>();
        private void DrawXYaxis(IPoint pBasePoint,int groups,double kd,string[] groupNames,bool axis=true)
        {
            Ykedu.Clear();
            annoClass.Clear();
            GraphicsHelper gh=new GraphicsHelper();
            double dis = 40 * 1e-3 * mapScale;
            //X轴
            IPoint xpoint = new PointClass() { X = pBasePoint.X + 6*dis, Y = pBasePoint.Y };
            IPoint xpoint1 = new PointClass() { X = pBasePoint.X -dis/10/2, Y = pBasePoint.Y };
            //X轴 
            double total = dis * 6;
            if (axis)
            {
                IGeometry pline = ContructPolyLine(xpoint1, xpoint);
                DrawLine(pline as IPolyline);
                for (int i = 1; i <= 6; i++)
                {
                    IPoint point = new PointClass() { X = pBasePoint.X + i * dis, Y = pBasePoint.Y - dis / 10 / 2 };
                    IPoint point1 = new PointClass() { X = pBasePoint.X + i * dis, Y = pBasePoint.Y + 4 * dis };
                    IGeometry line = ContructPolyLine(point, point1);

                    DrawLine(line as IPolyline);

                    int txtval = (int)(i * kd * 10 / 6);
                    string txt = (txtval).ToString();
                    IPoint txtpoint = new PointClass();
                    txtpoint.PutCoords(pBasePoint.X + i * dis, pBasePoint.Y - 4 * dis / 10 / 2);
                    double cx = (txtpoint.X - pBasePoint.X) / total;
                    double cy = (txtpoint.Y - pBasePoint.Y) / total;
                    IPoint pt = new PointClass() { X = cx, Y = cy };
                    Ykedu.Add(pt, txt);
                }
            }
            //Y轴刻度
            double yLength = 4 * dis;
            double yDis = yLength / groups;
            for (int i = 1; i <= groups; i++)
            {
                IPoint point = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + i * yDis };
                IPoint point1 = new PointClass() { X = pBasePoint.X - dis / 10 / 2, Y = pBasePoint.Y + i * yDis };
                IGeometry line = ContructPolyLine(point, point1);
                if (axis)
                {
                    DrawLine(line as IPolyline);
                }
                //绘制类别
                IPoint ylabelPoint = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + i * yDis - 0.5 * yDis };
                string groupname = groupNames[i - 1];

 
                double cx = (ylabelPoint.X - pBasePoint.X) / total;
                double cy = (ylabelPoint.Y - pBasePoint.Y) / total;
                IPoint pt = new PointClass() { X = cx, Y = cy };
                annoClass.Add(pt, groupname);
                
            }
             
        }

        private void DrawColumn(IPoint pBasePoint, Dictionary<string, double> datas, double max, List<ICmykColor> cmykColors)
        {
            annoNum.Clear();
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double heightunit = 3.97427;//1号字体1万的高度
            double xdis = 40 * 1e-3 * mapScale;//一组分类数据长度
            double total = xdis * 6.0;
            int groups = 1;
            double yLength = 4 * xdis;
            double ydis = yLength / groups;
            int i = 0;
           // foreach (var kvdic in datas)
            {
                var lists = datas;
                double step = ydis / (1 + lists.Count);//一条记录的高度
                double fontsize = step / heightunit / (mapScale / 10000);
                fontsize = Math.Round(fontsize, 1);
                double dy = step * 0.8;
                if (fontsize > 15)
                {
                    fontsize = 15;
                    dy = step * 0.5 + 15 * heightunit * mapScale / 10000 * 0.4;
                }
                int j = 0;
                foreach (var kv in lists)
                {

                    IPoint point = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + i * ydis + step * (j + 1) + 0.5 * step };
                    double width = kv.Value / max * 6 * xdis * 0.95;
                    IPolygon columngeo = gh.CreateRectangle(point, width, step);
                    DrawPolygon(columngeo, cmykColors[j]);
                    //数量注记
                    IPoint labelpoint = new PointClass();
                    labelpoint.PutCoords(point.X + width + step*0.5, point.Y - step*0.5);
                    double cx = (labelpoint.X - pBasePoint.X) / total;
                    double cy = (labelpoint.Y - pBasePoint.Y) / total;
                    IPoint pt = new PointClass() { X = cx, Y = cy };
                    annoNum.Add(pt, kv.Value.ToString());
                    //DrawTxt(labelpoint, kv.Value.ToString(), fontsize);
                    //gh.DrawPolygon(columngeo);
                    j++;
                }
                i++;
            }
            //Y轴
            if (XyAxis)
            {
                IPoint p1 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y - 2 * mapScale * 1.0e-3 };
                IPoint p2 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + 4 * xdis };
                IGeometry line = ContructPolyLine(p1, p2);
                DrawLine(line as IPolyline);
            }
        }
      
        ///绘制柱  
        //获取数量注记
        Dictionary<IPoint, string> annoNum = new Dictionary<IPoint, string>();
        private void DrawColumns(IPoint pBasePoint,  Dictionary<string,Dictionary<string, double>> datas, double max, List<ICmykColor> cmykColors)
        {
            annoNum.Clear();
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double heightunit = 3.97427;//1号字体1万的高度
            double xdis = 40 * 1e-3 * mapScale;//一组分类数据长度
            double total = xdis * 6.0;
            int groups = datas.Count;
            double yLength = 4 * xdis;
            double ydis = yLength / groups;
            int i = 0;
            foreach(var kvdic in datas)
            {
                var lists = kvdic.Value;
                double step = ydis / (1 + lists.Count);//一条记录的高度
                double fontsize = step / heightunit / (mapScale / 10000);
                fontsize = Math.Round(fontsize, 1);
                double dy = step * 0.8;
                if (fontsize > 15)
                {
                    fontsize = 15;
                    dy = step * 0.5 + 15 * heightunit * mapScale / 10000 * 0.4;
                }
                int j = 0;
                foreach (var kv in lists)
                {

                    IPoint point = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + i * ydis + step*(j+1)+0.5*step };
                    double width = kv.Value / max * 6 * xdis * 0.95;
                    IPolygon columngeo = gh.CreateRectangle(point, width, step);
                    DrawPolygon(columngeo, cmykColors[j]);
                    //数量注记
                    IPoint labelpoint = new PointClass();
                    labelpoint.PutCoords(point.X + width + step, point.Y - step*0.5);
                    double cx = (labelpoint.X - pBasePoint.X) / total;
                    double cy = (labelpoint.Y - pBasePoint.Y) / total;
                    IPoint pt = new PointClass() { X = cx, Y = cy };
                    annoNum.Add(pt, kv.Value.ToString());
                    //DrawTxt(labelpoint, kv.Value.ToString(), fontsize);
                    //gh.DrawPolygon(columngeo);
                     j++;
                }
                i++;
            }
              //Y轴
            if (XyAxis)
            {
                //Y轴
                IPoint p1 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y - 2 * mapScale * 1.0e-3 };
                IPoint p2 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + 4 * xdis };
                IGeometry line = ContructPolyLine(p1, p2);
                DrawLine(line as IPolyline);
            }
        }
        #endregion
        #region
        private void DrawPolygon(IGeometry pgeo,IColor pcolor)
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
                IGraphicsContainer pContainer = pAc as IGraphicsContainer;
                //eles.Add(pEl);
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
            double Xaxis;
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
            Xaxis = (242 / RepMarker.Width) * width;
            double fontsize = (Xaxis / 50);
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);
            fontsize *= 2.83;

            double x = 0;
            double y = 0;
            IPoint pt=new PointClass ();
            //y刻度
            foreach (var k in Ykedu)
            {
                x = k.Key.X * Xaxis + center.X;
                y = k.Key.Y * Xaxis + center.Y;
                pt.PutCoords(x, y);
                InsertAnnoFea(pt, k.Value, fontsize * 0.6, id);
            }
            //数量标注
            foreach (var k in annoNum)
            {
                x = k.Key.X * Xaxis + center.X;
                y = k.Key.Y * Xaxis + center.Y;
                pt.PutCoords(x, y);
                InsertAnnoFeaNums(pt, k.Value, fontsize * 0.3, id);
            }
            //图例
            foreach (var k in annolg)
            {
                x = k.Key.X * Xaxis + center.X;
                y = k.Key.Y * Xaxis + center.Y;
                pt.PutCoords(x, y);
                InsertAnnoFea(pt, k.Value, fontsize * 0.4, id);
            }
            //类别
            if (geoRelate == "")
            {
                foreach (var k in annoClass)
                {
                    double strwidth = gh.GetStrWidth(k.Value, curms, fontsize * 0.6);
                    x = k.Key.X * Xaxis + center.X - strwidth / 2.0;
                    y = k.Key.Y * Xaxis + center.Y;
                    pt.PutCoords(x, y);
                    InsertAnnoFea(pt, k.Value, fontsize * 0.6, id);
                }
            }
            //标题
            if (charttile != "")
            {
                x = width / 2 + center.X ;
                y = height + center.Y + height / 10;
                pt.PutCoords(x, y);
                InsertAnnoFea(pt, charttile, fontsize * 1.3, id);
            }
        }
        public void InsertAnnoFeaNums(IGeometry pGeometry, string annoName, double fontSize, int id)
        {
            try
            { 
                IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
                ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize);
                IElement pElement = pTextElement as IElement;
                ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElement;
                pSymbolCollEle.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
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
        public  void InsertAnnoFea(IGeometry pGeometry, string annoName, double fontSize, int id)
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
       
    }
}

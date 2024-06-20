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
    /// 绘制数量立体柱状图
    /// </summary>
 
    public sealed class DrawCubeColumn
    {
        private IActiveView pAc = null;
        private double mapScale = 10000;
        private List<IElement> eles = new List<IElement>();
        private ILayer pRepLayer = null;
        GraphicsHelper gh = null;
        private double markerSize = 20;
        public DrawCubeColumn(IActiveView pac, double ms)
        {
            pAc = pac;
            mapScale = ms;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pRepLayer = lyrs.First();
            gh = new GraphicsHelper(pAc);
        }
        public void CreateCubeColumns(IPoint _centerpoint)
        {
            FrmCloumnNumSet frm = new FrmCloumnNumSet("");
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            markerSize = frm.MarkerSize;
            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("正在处理...");
            if (frm.GeoRalated)
            {
                CreateCubeColumnsGeo(_centerpoint, frm);
            }
            else
            {
                CreateCubeColumnsUnGeo(_centerpoint, frm);
            }
            pAc.Refresh();
            wo.Dispose();
            MessageBox.Show("生成完成");
        }
        //地理关联
        public void CreateCubeColumnsGeo(IPoint _centerpoint, FrmCloumnNumSet frm)
        {
            
          
            //获取数据
            //获取颜色
            string geoRelate = frm.GeoLayer;
            Dictionary<string, IPoint> namesPt = ChartsDataSource.ObtainGeoRelated(geoRelate);

            cmykColors = frm.CMYKColors;
            Dictionary<string, double> dicIntMax = frm.dicInt;
            Dictionary<string, double> dicDecimalMax = frm.dicDecimal;
            string title = frm.ChartTitle;
            foreach (var chartsData in frm.ChartDatas)
            {
                eles.Clear();
             
                Dictionary<string, double> valsDic = chartsData.Value;
                IPoint centerpoint = (_centerpoint as IClone).Clone() as IPoint;
                string name = chartsData.Key;
                if (geoRelate != "")
                {
                    if (namesPt.ContainsKey(name))
                        centerpoint = namesPt[name];
                }
                DrawBgEle(dicIntMax.Count + dicDecimalMax.Count, centerpoint);
               
                IPoint ptShap = (centerpoint as IClone).Clone() as IPoint;
                rate = frm.RectRate;             
                indexInt = dicIntMax.Count;
                indexDic = dicDecimalMax.Count;
               
                //柱状图
                #region
                double numstep = mapScale * 1e-2;
                foreach (var kv in valsDic)
                {
                    cubedis = 10 * 1e-3 * mapScale;
                    cubstep = 0;
                    Dictionary<string, double> dicInt = new Dictionary<string, double>();
                    Dictionary<string, double> dicDecimal = new Dictionary<string, double>();
                    SplitData(kv.Value, ref dicInt, ref dicDecimal);
                    //整数部分
                    int color = indexInt - dicInt.Count;
                    DrawColumns(centerpoint, dicInt, cmykColors, color);
                  
                    //小数部分
                    DrawColumns(centerpoint, dicDecimal, cmykColors, indexInt);
                    //标注
                    IPoint lbpoint = new PointClass() { X = centerpoint.X + cubstep / 2, Y = centerpoint.Y - 1e-2 * mapScale };
                    var ele= gh.DrawTxt(lbpoint, kv.Value.ToString(), 30);
                    centerpoint.X += cubstep + numstep;
                    eles.Add(ele);
                }
                #endregion
                gh.CreateFeatures(eles, pRepLayer, ptShap, markerSize);
            }
            eles.Clear();
            IPoint pBasePoint = new PointClass() { X = _centerpoint.X, Y = _centerpoint.Y - 2e-2 * mapScale };
          
            //图例
            DrawBgEle(dicIntMax.Count + dicDecimalMax.Count, pBasePoint, -1);
            DrawLengend(title, pBasePoint, dicIntMax, dicDecimalMax);
            gh.CreateFeatures(eles, pRepLayer, pBasePoint, markerSize);
        }
        private void DrawBgEle(int ct,IPoint pt,int f=1)
        {
            double dis =  10 * 1e-3 * mapScale; ;//一个正方形长度
            double step = 10 * 1e-3 * mapScale * 0.25;//间距2.5cm

            double width = (dis + step) * ct;
            double height = 10 * dis;
            height *= f;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IGeometry pgeo= gh.CreateRectangle(pt, width, -height);

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
        /// 公共函数：绘制数量立方体
        /// </summary>
        /// <param name="centerpoint"></param>
        public void CreateCubeColumnsUnGeo(IPoint centerpoint, FrmCloumnNumSet frm)
        {
            
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double min = 0;
            cmykColors = frm.CMYKColors;
        
            rate = frm.RectRate;
            string title = frm.ChartTitle;

            Dictionary<string, double> dicIntMax = frm.dicInt;
            Dictionary<string, double> dicDecimalMax = frm.dicDecimal;
            
            indexInt = dicIntMax.Count;
            indexDic = dicDecimalMax.Count;
         
            //柱状图
            #region
            double numstep = mapScale * 2e-2;
            foreach (var kv in frm.ChartDatas)
            {
                eles.Clear();
                DrawBgEle(dicIntMax.Count + dicDecimalMax.Count, centerpoint);
                Dictionary<string, double> valsDic = kv.Value;
                double val = valsDic.First().Value;
                cubedis = 10 * 1e-3 * mapScale;
                cubstep = 0;
                Dictionary<string, double> dicInt = new Dictionary<string, double>();
                Dictionary<string, double> dicDecimal = new Dictionary<string, double>();
                SplitData(val, ref dicInt, ref dicDecimal);
                //整数部分
                int color = indexInt - dicInt.Count;
                DrawColumns(centerpoint, dicInt, cmykColors, color);

                //小数部分
                DrawColumns(centerpoint, dicDecimal, cmykColors, indexInt);
                //标注
                IPoint lbpoint = new PointClass() { X = centerpoint.X + cubstep / 2, Y = centerpoint.Y - 1e-2 * mapScale };
                var ele = gh.DrawTxt(lbpoint, val.ToString(), 15);
                eles.Add(ele);
                IPoint shapPt = (centerpoint as IClone).Clone() as IPoint;
                gh.CreateFeatures(eles, pRepLayer, shapPt, markerSize);
                centerpoint.X += cubstep;
            }
            #endregion
            //图例
            eles.Clear();
            DrawBgEle(dicIntMax.Count + dicDecimalMax.Count, centerpoint,-1);
            DrawLengend(title, centerpoint, dicIntMax, dicDecimalMax);
            gh.CreateFeatures(eles, pRepLayer, centerpoint, markerSize);
        }
        #region
        private void SplitData(double val, ref Dictionary<string, double> dicInt, ref Dictionary<string, double> dicDecimal)
        {
          
            string txt = val.ToString();
           
            string[] txts = txt.Split('.');
            string str= txts[0];
            //整数
            char[] chars= str.ToCharArray();
            int length=chars.Length;
            foreach (char c in chars)
            {
                string label="1";
                for(int i=0;i<length-1;i++)
                {
                    label+="0";
                }
                dicInt[label] = double.Parse(c.ToString());
                length--;
            }
            //小数
            if (txts.Length == 2)
            {
                chars = txts[1].ToCharArray();
                length = chars.Length;
                int j = 0;
                foreach (char c in chars)
                {
                    string label = "0.";
                    for (int i = 0; i < j; i++)
                    {
                        label += "0";
                    }
                    label += "1";
                    dicDecimal[label] = double.Parse(c.ToString());
                    j++;
                }
            }
        }
        //获取数据
        private Dictionary<string, double> getStaticDatas(ref double min)
        {     //类别
            int type = 4;
            //组数
            int groups = 4;
            List<Dictionary<string, double>> datas = new List<Dictionary<string, double>>();
            
            Random random = new Random();
           
            Dictionary<string, double> dicvals = new Dictionary<string, double>();
               
            dicvals["高速公路"] = random.Next(20, 150);
            dicvals["一级公路"] = random.Next(10, 50);
            dicvals["二级公路"] = random.Next(10, 150);
            dicvals["三级公路"] = random.Next(30, 150);
            var vals = dicvals.OrderBy(r => r.Value);

            min = vals.First().Value;



            return dicvals;
        }
        private double rate = 0.85;
        List<ICmykColor> cmykColors = null;
        int indexInt = 0, indexDic = 0;
    
        //绘制图例
        private void DrawLengend(string title, IPoint pBasePoint, Dictionary<string, double> dicInt, Dictionary<string, double> dicDecimal)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            cubedis = 10 * 1e-3 * mapScale;
            int num = dicInt.Count;
            double ystep = cubedis + 5e-3 * mapScale;
            double fontsize = 15;
            //整数部分
            int j = 0;
            IPoint titlepoint = new PointClass() { X = pBasePoint.X + 2e-2 * mapScale, Y = pBasePoint.Y-(j) * ystep };
            var elet = gh.DrawTxt(titlepoint, title, 20); eles.Add(elet);
            foreach(var kv in dicInt)
            {
                IPoint point = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y - (j + 1) * ystep };
                //正面
                IPolygon columngeo = gh.CreateRectangle(point, cubedis, cubedis);
                var ele= DrawPolygon(columngeo, cmykColors[j]);
                eles.Add(ele);
                //顶面
                IPolygon diatopgeo = gh.ConstructTopDiamond(point, cubedis, cubedis * 0.35);
                ele = DrawPolygon(diatopgeo, cmykColors[j]);
                eles.Add(ele);
                //侧面
                IPoint pointside = new PointClass() { X = pBasePoint.X + cubedis, Y = pBasePoint.Y - (j + 1) * ystep };
                IPolygon diasidegeo = gh.ConstructSideDiamond(pointside, cubedis * 0.35, -cubedis);
                ele = DrawPolygon(diasidegeo, cmykColors[j]);
                eles.Add(ele);
                //注记
                double stw = gh.GetStrWidth(kv.Key.ToString(), mapScale, fontsize);
                double dy = cubedis * 0.5;
                IPoint lbpoint = new PointClass() { X = point.X + 3e-2 * mapScale + stw/2, Y = point.Y - dy };
                ele = gh.DrawTxt(lbpoint, kv.Key.ToString(), fontsize);
                eles.Add(ele);
                cubedis *= rate;
                j++;
            }
            num = 3;
            foreach (var kv in dicDecimal)
            {
                IPoint point = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y - (j + 1) * ystep };
                //正面
                IPolygon columngeo = gh.CreateRectangle(point, cubedis, cubedis);
                var ele = DrawPolygon(columngeo, cmykColors[j]); eles.Add(ele);  
                //顶面
                IPolygon diatopgeo = gh.ConstructTopDiamond(point, cubedis, cubedis * 0.35);
                ele = DrawPolygon(diatopgeo, cmykColors[j]); eles.Add(ele);
                //侧面
                IPoint pointside = new PointClass() { X = pBasePoint.X + cubedis, Y = pBasePoint.Y - (j + 1) * ystep };
                IPolygon diasidegeo = gh.ConstructSideDiamond(pointside, cubedis * 0.35, -cubedis);
                ele = DrawPolygon(diasidegeo, cmykColors[j]); eles.Add(ele);
                //注记
                double stw = gh.GetStrWidth(kv.Key.ToString(), mapScale, fontsize);
                double dy = cubedis * 0.5;
                IPoint lbpoint = new PointClass() { X = point.X +3e-2 * mapScale + stw / 2, Y = point.Y - dy };
                ele = gh.DrawTxt(lbpoint, kv.Key.ToString(), fontsize); eles.Add(ele);

                cubedis *= rate;
                j++;
            }
            
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
        double cubstep = 0;
        double cubedis =0;//一个正方形长度;
        ///绘制柱  
        private void DrawColumns(IPoint pBasePoint, Dictionary<string, double> lists,  List<ICmykColor> cmykColors,int cindex=0)
        {
           
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double dis = cubedis;//一个正方形长度
            double step = 10 * 1e-3 * mapScale * 0.25;//间距2.5cm
            int i= 0;
          
            foreach (var kv in lists)
            {
                int num=(int)(kv.Value);//四舍五入取整,正方形个数
                if (num == 0)
                {
                    IPoint point = new PointClass() { X = pBasePoint.X + cubstep, Y = pBasePoint.Y + (0) * cubedis };
                   
                    IPolygon diatopgeo = gh.ConstructTopDiamond(point, cubedis, cubedis * 0.35);
                    var ele = DrawPolygon(diatopgeo, cmykColors[i + cindex]);
                    eles.Add(ele);
                   
                }
                for (int j = 0; j < num; j++)
                {
                    IPoint point = new PointClass() { X = pBasePoint.X + cubstep, Y = pBasePoint.Y + (j + 1) * cubedis };
                    //正面
                    IPolygon columngeo = gh.CreateRectangle(point, cubedis, cubedis);
                    var ele = DrawPolygon(columngeo, cmykColors[i + cindex]); eles.Add(ele);
                     
                    //顶面
                    IPolygon diatopgeo = gh.ConstructTopDiamond(point, cubedis, cubedis * 0.35);
                    ele = DrawPolygon(diatopgeo, cmykColors[i + cindex]); eles.Add(ele);
                    
                    //侧面
                    IPoint pointside = new PointClass() { X = pBasePoint.X + cubstep + cubedis, Y = pBasePoint.Y + (j) * cubedis };
                    IPolygon diasidegeo = gh.ConstructSideDiamond(pointside, cubedis * 0.35, cubedis);
                    ele = DrawPolygon(diasidegeo, cmykColors[i + cindex]); eles.Add(ele);
                   
                    
                }
               
                cubstep += step + cubedis;
                cubedis *= rate;
              
                
                i++;
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
            smline.Style = esriSimpleLineStyle.esriSLSSolid;          
        
            IRgbColor rgb = new RgbColorClass();
            rgb.Red = 220;
            rgb.Blue = 220;
            rgb.Green = 220;
            smline.Color = rgb;
            smline.Width = 1;

            smsymbol.Outline = smline;
            polygonElement.Symbol = smsymbol;
    
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
            
            return pEl;
        }
        private void DrawLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 1.5;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = 122;
                rgb.Blue = 122;
                rgb.Green = 122;
                linesym.Color = rgb;
                polygonElement.Symbol = linesym;
                pEl = polygonElement as IElement;
                pEl.Geometry = pline as IGeometry;
                
                pAc.Refresh();

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

        #endregion
    }
}

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
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    /// <summary>
    /// 绘制正负条形图辅助类
    /// </summary>
   
    public sealed class DrawNpBar
    {
        private IActiveView pAc = null;
        private double mapScale = 10000;
        
        public DrawNpBar(IActiveView pAc_, double ms)
        {
             pAc=pAc_;
            mapScale=ms;
        }

       
       
        /// <summary>
        /// 绘制正负条形
        /// </summary>
        /// <param name="centerpoint"></param>
        public void CreateNpBars(IPoint centerpoint)
        {
            //获取颜色
            FrmBarChartsSet frm = new FrmBarChartsSet("正负条形图");
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            Dictionary<string, Dictionary<string, double>> groupdatas = frm.ChartDatas;
            string chartTitle = frm.ChartTitle;
            xyAxis = frm.XYAxis;
            double max=0;
            string[] type = getStaticDatas(groupdatas, ref max);
           
         
            List<ICmykColor> cmykColors = frm.CMYKColors;
            if(xyAxis)
               CreateXYaxi(centerpoint, max);
            DrawColumns(centerpoint, groupdatas, max, cmykColors);
            DrawLengend(centerpoint, groupdatas.First().Value.Count, type, cmykColors);
            //标题
            if (chartTitle != "")
            {
                DrawBarTitle(centerpoint, chartTitle);
            }
        }
        #region 相关辅助函数
        //获取数据
        private string[] getStaticDatas(Dictionary<string, Dictionary<string, double>> groupdatas, ref double max)
        {

            List<string> types = new List<string>();
            foreach (var kv in groupdatas)
            {
                Dictionary<string, double> dicvals = kv.Value;
                var vals = dicvals.OrderByDescending(r => r.Value);

                max = vals.First().Value > max ? vals.First().Value : max;
                types.Add(kv.Key);

            }
            return types.ToArray();
        }
        bool xyAxis = false;
        private double dis =0;//1刻度间距
        private void DrawBarTitle(IPoint pbasePoint, string txt)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double dis = 30 * 1e-3 * mapScale;

            IPoint txtpoint = new PointClass() { X = pbasePoint.X, Y = pbasePoint.Y + dis + 2e-3 * mapScale };
            gh.DrawTxt(txtpoint, txt, 8);
        }
        /// <summary>
        /// 绘制xy轴
        /// </summary>
        private void CreateXYaxi(IPoint pBasePoint, double max)
        {
            dis =5 * 1e-3 * mapScale;
            double kd = max / 0.95;
            //Y轴
            for (int i = -5; i <= 5; i++)
            {
                IPoint point = new PointClass() { X = pBasePoint.X - i * dis, Y = pBasePoint.Y + 6 * dis };
                IPoint point1 = new PointClass() { X = pBasePoint.X - i * dis, Y = pBasePoint.Y };
                IGeometry line = ContructPolyLine(point, point1);
                if (i == 0)
                {
                    continue;
                }
                else
                {
                    DrawYLine(line as IPolyline);
                }
            }
            //右边X轴
            IPoint xpoint = new PointClass() { X = pBasePoint.X + 5 * dis, Y = pBasePoint.Y };
            IPoint xpoint1 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y };
            IGeometry pline = ContructPolyLine(xpoint1, xpoint);
            DrawLine(pline as IPolyline);
            //右边刻度
            for (int i = 0; i <= 5; i++)
            {
                IPoint point = new PointClass() { X = pBasePoint.X + i * dis, Y = pBasePoint.Y - dis / 5 };
                IPoint point1 = new PointClass() { X = pBasePoint.X + i * dis, Y = pBasePoint.Y  };
                IGeometry line = ContructPolyLine(point, point1);
                DrawLine(line as IPolyline);
                int txtval = i * (int)(kd / 6.0);
                string txt = (txtval).ToString();
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(pBasePoint.X + i * dis, pBasePoint.Y - 2*dis / 5);
                DrawTxt(txtpoint, txt, 3);
            }
            //左边
            xpoint = new PointClass() { X = pBasePoint.X - 5 * dis, Y = pBasePoint.Y };
            xpoint1 = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y };
            pline = ContructPolyLine(xpoint1, xpoint);
            DrawLine(pline as IPolyline);
            //左边刻度
            for (int i = 1; i <= 5; i++)
            {
                IPoint point = new PointClass() { X = pBasePoint.X - i * dis, Y = pBasePoint.Y - dis / 5 };
                IPoint point1 = new PointClass() { X = pBasePoint.X- i * dis, Y = pBasePoint.Y };
                IGeometry line = ContructPolyLine(point, point1);
                DrawLine(line as IPolyline);
                int txtval = i * (int)(kd / 6.0);
                string txt = (txtval).ToString();
                IPoint txtpoint = new PointClass();
                txtpoint.PutCoords(pBasePoint.X - i * dis, pBasePoint.Y - 2*dis / 5);
                DrawTxt(txtpoint, txt, 3);
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
        
       
        // 柱子高度30cm
        // x 长度30cm
        ///绘制柱  
        private void DrawColumns(IPoint pBasePoint, Dictionary<string, Dictionary<string, double>> groupdatas,  double max, List<ICmykColor> cmykColors)
        {

           
            Dictionary<string, double> left = groupdatas.First().Value;
            Dictionary<string, double> right = groupdatas.Last().Value;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double height = 30 * 1e-3 * mapScale;//总高度Y
            double width = 25 * 1e-3 * mapScale;//一侧宽度X
         
            double columnheight = 30/1.5/left.Count;
            columnheight = columnheight * 1e-3 * mapScale;//每个柱子高度
            double interval = columnheight/2;//柱子间间隔
            int i= 1;
            double heightunit = 3.97427;//1号字体1万的高度
            double fontsize = columnheight / heightunit / (mapScale / 10000);
            fontsize = Math.Round(fontsize, 1);
            foreach (var kv in right)
            {
                double temp = (kv.Value / max)*width*0.92;//四舍五入取整,正方形个数

                IPoint point = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + i * (columnheight + interval) };
                IPolygon columngeo = gh.CreateRectangle(point, temp, columnheight);
              

                DrawPolygon(columngeo, cmykColors[1]);
                //数量注记
                IPoint labelpoint = new PointClass();
                labelpoint.PutCoords(point.X + temp + mapScale * 2e-3, point.Y);
                if (fontsize >= 4)
                {
                    fontsize = 4;
                    double dy = columnheight / 2 + fontsize * heightunit * (mapScale / 10000) * 0.4;
                    labelpoint.Y -= dy;
                }
                else
                {
                    labelpoint.Y -= columnheight * 0.8;
                }
                DrawTxt(labelpoint, kv.Value.ToString(), fontsize);
              
                i++;
            }
            i = 1;
            foreach (var kv in left)
            {
                double temp = (kv.Value / max) * width * 0.92;//四舍五入取整,正方形个数

                IPoint point = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + i * (columnheight + interval) };
                IPolygon columngeo = gh.CreateRectangle(point, -temp, columnheight);
                DrawPolygon(columngeo, cmykColors[0]);
                //Y注记
                IPoint labelPoint = new PointClass() { X = pBasePoint.X - width - interval/2, Y = point .Y};
                if (fontsize >= 4)
                {
                    fontsize = 4;
                    double dy = columnheight / 2 + fontsize * heightunit * (mapScale / 10000)*0.4;
                    labelPoint.Y -= dy;
                }
                else
                {
                    labelPoint.Y -= columnheight * 0.8;
                }
                double strwidth = gh.GetStrWidth(kv.Key, mapScale, fontsize);
                labelPoint.X -= strwidth/2;
                gh.DrawTxt(labelPoint, kv.Key, fontsize);
                //数量注记
                IPoint labelpoint = new PointClass();
                labelpoint.PutCoords(point.X - temp - mapScale * 2e-3, labelPoint.Y);
                DrawTxt(labelpoint, kv.Value.ToString(), fontsize);
              
                i++;
            }
            //y轴
            if (xyAxis)
            {
                IPoint pointf = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y + 6 * dis };
                IPoint pointt = new PointClass() { X = pBasePoint.X, Y = pBasePoint.Y };
                IGeometry line = ContructPolyLine(pointf, pointt);
                DrawLine(line as IPolyline);
            }
        }
        ///<summary>
        ///绘制图列
        ///</summary>
        ///<param name="basepoint">坐标原点</param>
        private void DrawLengend(IPoint basepoint, int count, string[] data, List<ICmykColor> cmykColors)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取图列的高度
          

            double lgheight = 20;
            int ct = data.Length;
            //全部图例+文字长度
            double length = 2*lgheight * ct;//图例
            length += mapScale * 2.0e-3 * (ct - 1);//图例间隔
            double fontsize = 4;
            foreach (string str in data)
            {
                length += gh.GetStrWidth(str, mapScale, fontsize) + mapScale * 1.0e-3;// ;//加文字宽度与间隔
            }
            //X轴长度
            double Xdis = 50 * 1e-3 * mapScale;


            double dx = Xdis / 2 - length / 2;//平移距离
            double stepX = 0;
            for (int i = 0; i < data.Length; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - 5e-3 * mapScale;
                upleft.PutCoords(stepX + basepoint.X-Xdis/2 + dx, y);
                //tuli len
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                gh.DrawPolygon(prect, cmykColors[i], 0);
                stepX += lgheight * 2;
                //wenzi len
                double strwidth = gh.GetStrWidth(data[i], mapScale, fontsize);
                stepX += mapScale * 1.0e-3;//文字间隔
                IPoint txtpoint = new PointClass() { X = mapScale * 1.0e-3 + upleft.X + strwidth / 2 + lgheight * 2, Y = upleft.Y - 0.8 * lgheight };
                gh.DrawTxt(txtpoint, data[i], fontsize);
                stepX += strwidth; //文字长度;
                stepX +=  mapScale * 2.0e-3;//tuli间隔;

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
            smline.Style = esriSimpleLineStyle.esriSLSNull;          
        
            IRgbColor rgb = new RgbColorClass();
            rgb.Red = 220;
            rgb.Blue = 220;
            rgb.Green = 220;
            smline.Color = rgb;
            smline.Width = 0.5;

            smsymbol.Outline = smline;
            polygonElement.Symbol = smsymbol;
    
            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
            pContainer.AddElement(pEl, 0);
            pAc.Refresh();
        }
        private void DrawLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 0.5;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = 122;
                rgb.Blue = 122;
                rgb.Green = 122;
                linesym.Color = rgb;
                polygonElement.Symbol = linesym;
                pEl = polygonElement as IElement;
                pEl.Geometry = pline as IGeometry;
                pContainer.AddElement(pEl, 0);
                pAc.Refresh();

            }
            catch
            {

            }
        }
        private void DrawYLine(IPolyline pline)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            try
            {

                ILineElement polygonElement = new LineElementClass();
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Width = 0.5;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = 222;
                rgb.Blue = 222;
                rgb.Green = 222;
                linesym.Color = rgb;
                polygonElement.Symbol = linesym;
                pEl = polygonElement as IElement;
                pEl.Geometry = pline as IGeometry;
                pContainer.AddElement(pEl, 0);
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
                pContainer.AddElement(pEl, 0);
                pAc.Refresh();
                return pEl;
            }
            catch
            {
                return null;
            }
        }
        #endregion

    }
}

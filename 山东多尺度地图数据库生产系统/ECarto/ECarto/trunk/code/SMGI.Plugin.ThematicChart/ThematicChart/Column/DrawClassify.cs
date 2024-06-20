using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using stdole;

using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public sealed class DrawClassify
    {
        /// <summary>
        /// 绘制分类柱状图
        /// </summary>
        private IActiveView pAc = null;
        public ColumnJson columnInfo = null;
        private double mapScale = 1000;
        private double mapScale2=0 ;
        private List<IElement> eles = new List<IElement>();
        private ILayer pRepLayer = null;
        private IFeatureClass annoFcl = null;
        GraphicsHelper gh = null;
        //窗体传过来的参数
        private  string chartTitle;
        private bool geoNum = true;
        private double angle=0;
        List<ICmykColor> cmykColors = null;

       
        public  DrawClassify(IActiveView pac, double ms)
        {
            pAc = pac;
            mapScale2 = ms;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            pRepLayer = lyrs.First();

            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LANNO");
            })).ToArray();
            var annoly = lyr.First();
            annoFcl = (annoly as IFeatureLayer).FeatureClass;

            gh = new GraphicsHelper(pAc);
        }
        public void CreateClassifyColumns(FrmClassifySet frm ,IPoint centerpoint)
        {
            CommonMethods.ClearThematicCarto(centerpoint, (pRepLayer as IFeatureLayer).FeatureClass, annoFcl);
            angle = frm.TxtAngle;
            WaitOperation wo = GApplication.Application.SetBusy();
            wo.SetText("正在处理...");

            ColumnsUnGeo(centerpoint, frm);//地理不关联
            
            wo.Dispose();
            pAc.Refresh();
            MessageBox.Show("生成完成");
        }
        //地理不关联
        private void ColumnsUnGeo(IPoint center, FrmClassifySet frm)
        {
            eles.Clear();
            columnInfo = frm.ColomInfo;
            cmykColors = frm.CMYKColors;
            chartTitle = frm.ChartTitle;
            double markersize = frm.MarkerSize;
            double max = 0;
            Dictionary<string, Dictionary<string, double>> datas = frm.ChartDatas;
            string[] types = CreateStaticDatas(datas, ref max);
            int group = datas.Count;
            //绘制分类统计图
            IPoint orginpt = new PointClass();
            orginpt.PutCoords(0, 0);
            DrawColumns(orginpt, datas, max);

            if (frm.GeoLengend)
            {
                DrawLengend(orginpt, types, group);
            }
            else
            {
                DrawLengendBg(orginpt, types, group);
            }
            int obj = 0;
            //尺寸
          
            string jsonText = JsonHelper.GetJsonText(columnInfo);
            ChartsToRepHelper ch = new ChartsToRepHelper();
            //创建白色图表背景
            if (!frm.IsTransparent)
            {
                CreateWhiteBackGround(eles);
            }
            var repmarker = ChartsToRepHelper.CreateFeature(pAc, eles, pRepLayer, center, jsonText, out obj, markersize);
           // var repmarker = ch.CreateFeatureEX(pAc, eles, pRepLayer, center, out obj, markersize);
            CreateAnnotion(repmarker, center, markersize, datas, obj);
        }
        //数量标注
        Dictionary<IPoint, string> lbpt = new Dictionary<IPoint, string>();
     
        private void DrawColumns(IPoint pBasepoint, Dictionary <string ,Dictionary <string ,double >>datas,double max)
        {
            GraphicsHelper gh = new GraphicsHelper(pAc);
            //一个数据宽度
          
            int groups = datas.Count;
            double dis = 1333 / (2 * groups);
            dis = Math.Round(dis, 0);
            //x总长
            double xsum = groups * dis * 2;//1333?,没有用到!
            double total = 1000;
            int num = 0;
            double d = 1.0 / datas.Count * dis;//没有用到!
            double xdis = pBasepoint.X ;
            lbpt.Clear();
            
            double step = 0;
            foreach (var k in datas)
            {
                var lists = k.Value;
                int j = 0;
                double h = 0;
                foreach (var kv in lists)
                {
                    double height = kv.Value / max * total;
                    IPoint point = new PointClass() { X = xdis + step, Y = pBasepoint.Y + h };
                    IPolygon columngeo = gh.CreateRectangle(point, dis, -height);
                    var ele = DrawPolygon(columngeo, cmykColors[j]);
                    eles.Add(ele);
                    //存储数量注记
                    if (columnInfo.GeoNum)
                    {
                        IPoint labelpoint = new PointClass() { X = point.X + dis / 2.0, Y = point.Y+height /2.0 };
                        IPoint pt = new PointClass() { X = (labelpoint.X - pBasepoint.X) / total, Y = (labelpoint.Y - pBasepoint.Y) / total };
                        lbpt.Add(pt, kv.Value.ToString());
                    }
                    h += height;
                    j++;
                }
                step += 2*dis;
                num++;
            }
        }

       

        //获取数据类别（行首）名称及最大值
        private string[] CreateStaticDatas(Dictionary<string, Dictionary<string, double>> datas, ref double max)
        {
            List<string> types = new List<string>();
            var data = datas.First().Value;
            foreach (var k in data)
            {
                types.Add(k.Key);//类别名称
            }
            foreach (var kv in datas)
            {
                var vals = kv.Value;
                double a = 0;
                foreach (var c in vals)
                {
                    a = a + c.Value;
                }
                if (a >= max)
                {
                    max = a;
                }
            }
            return types.ToArray();
        }

      
        
        //图例坐标和标注
        Dictionary<IPoint, string> lgpt = new Dictionary<IPoint, string>();
        double totalEle = 0;
        private void DrawLengend(IPoint _basepoint, string[] subtypes, int groups)
        {
            lgpt.Clear();
            IPoint basepoint = (_basepoint as IClone).Clone() as IPoint;
           
           
            double dis = 1333 / (2 * groups);
            dis = Math.Round(dis, 0);
            totalEle = 1000 + dis + dis * 0.8;
            double xdistance = (groups * 2 - 1) * dis;//X轴长度

            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取图列的高度
            double fontsize = 8;
            double lgheight = dis * 0.8;//图例高度
            int ct = subtypes.Length;
            //全部图例+文字长度
            double length = lgheight * 2 * ct;//图例长度
            double lginterval = dis/2;//图例之间间隔
            double txtinterval = dis/2;//文字与图例间隔
            length += lginterval * (ct - 1);//图例之间间隔累加
            foreach (string str in subtypes)
            {
                length += gh.GetStrWidth(str, 10000, fontsize) + txtinterval;//计算图例行总长度
            }
            double stepX = 0;
            for (int i = 0; i < subtypes.Length; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - dis;

                upleft.PutCoords(stepX + basepoint.X, y);
                //tulie
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                var ele = gh.DrawPolygon(prect, cmykColors[i], 0);
                eles.Add(ele);
                //wenzi
                double strwidth = gh.GetStrWidth(subtypes[i], 10000, fontsize);
                IPoint txtpoint = new PointClass() { X = (txtinterval + upleft.X + strwidth * 0.5 + lgheight * 2), Y = upleft.Y - lgheight * 0.5 };
                stepX += (txtinterval + strwidth + lginterval + lgheight * 2);//文字距离+图例间隔
                double sx = (txtpoint.X - _basepoint.X) / 1000.0;
                double sy = (txtpoint.Y - _basepoint.Y) / 1000.0;
                lgpt.Add(new PointClass { X = sx, Y = sy }, subtypes[i]);
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
            smline.Style = esriSimpleLineStyle.esriSLSNull;
            smsymbol.Outline = smline;

            polygonElement.Symbol = smsymbol;

            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;

            return pEl;
        }
        private void DrawLengendBg(IPoint _basepoint, string[] subtypes, int groups)
        {
            lgpt.Clear();
            IPoint basepoint = (_basepoint as IClone).Clone() as IPoint;


            double dis = 1333 / (2 * groups);
            dis = Math.Round(dis, 0);
            totalEle = 1000 + dis + dis * 0.8;
            double xdistance = (groups * 2 - 1) * dis;//X轴长度

            GraphicsHelper gh = new GraphicsHelper(pAc);
            //获取图列的高度
            double heightunit = 3.97427;//1号字体1万的高度
            double fontsize = 10;
            double lgheight = dis * 0.8;
            int ct = subtypes.Length;
            //全部图例+文字长度
            double length = lgheight * 2 * ct;//图例长度
            double lginterval = dis;//图例间间隔
            double txtinterval = dis / 2;//文字图例间隔
            length += lginterval * (ct - 1);//间隔
            foreach (string str in subtypes)
            {
                length += gh.GetStrWidth(str, 10000, fontsize) + txtinterval;// ;//加文字宽度
            }
            //X轴长度
            double Xdis = xdistance;


            double dx = Xdis / 2 - length / 2;//平移距离
            double stepX = 0;
            for (int i = 0; i < subtypes.Length; i++)
            {
                IPoint upleft = new PointClass();
                double y = basepoint.Y - dis;
                upleft.PutCoords(stepX + basepoint.X + dx, y);
                //tulie
                IPolygon prect = gh.CreateRectangle(upleft, lgheight * 2, lgheight);
                var ele = gh.DrawPolygonBg(prect, cmykColors[i], 0);
                eles.Add(ele);
                stepX += lgheight * 2;
                //wenzi
                double strwidth = gh.GetStrWidth(subtypes[i], 10000, fontsize);
                stepX += txtinterval;//文字间隔
                double dy = lgheight / 2.0 - heightunit / 2.0;
                IPoint txtpoint = new PointClass() { X = txtinterval + upleft.X + strwidth / 2 + lgheight * 2, Y = upleft.Y - lgheight * 0.5 };
                ele = gh.DrawTxt(txtpoint, subtypes[i], fontsize);
                stepX += strwidth + lginterval;//文字距离+图例间隔

                double sx = (txtpoint.X - _basepoint.X) / 1000.0;
                double sy = (txtpoint.Y - _basepoint.Y) / 1000.0;

                lgpt.Add(new PointClass { X = sx, Y = sy }, subtypes[i]);

            }
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
        #region 将注记存储至ANNO   注记相关

        private void CreateAnnotion(IRepresentationMarker RepMarker, IPoint pAnchor, double markersize, Dictionary<string, Dictionary<string, double>> datas, int id)
        {
            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double Xaxis;
            double Yaxis;
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
            //2.确定XY轴当前比例尺实际大小
            Yaxis = 1000 / totalEle * height;
            Xaxis = width;
            //文字大小一个柱子宽度的0.8
            double fontsize = (totalEle - 1000) / 1.8*0.4;
            fontsize = fontsize / 1000 * Yaxis;
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);
            double dy = fontsize;
            fontsize *= 2.83;
            //3.计算X轴文字坐标以及文字大小
            double dis = 1333 / (2 * datas.Count);//一个柱子的制图表达宽度，pt
            double allColumnWidth = dis * (datas.Count * 2 - 1);//全部柱子的制图表达宽度(包含柱子间的间隔)，pt
            double step = 0.0;
            double dx = (allColumnWidth / RepMarker.Width) * Xaxis / (datas.Count * 2 - 1);//一个柱子的实际宽度，m
            foreach (var kv in datas)
            {
                string key = kv.Key;
                var point = new PointClass();
                point.X = pAnchor.X + dx / 2.0 + step;
                point.Y = pAnchor.Y - 0.5*dx;
                InsertAnnoFeaTXT(point, key, fontsize * 0.5, id);
                step += dx * 2.0;
            }            
            //4.计算图例文字坐标以及文字大小
            foreach (var kv in lgpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis;
                double y = kv.Key.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                InsertAnnoFea(p, kv.Value, fontsize * 0.5, id);
            }
            //5.计算统计标注
            foreach (var kv in lbpt)
            {
                double x = kv.Key.X;
                x = x * Yaxis;
                double y = kv.Key.Y;
                y = y * Yaxis;

                var p = new PointClass();
                p.X = pAnchor.X + x;
                p.Y = pAnchor.Y + y;

                InsertAnnoFea(p, kv.Value, fontsize * 0.6, id);
            }
            //7.绘制标题
            if (chartTitle != "")
            {
                IPoint pt = new PointClass { X = pAnchor.X + Xaxis / 5, Y = pAnchor.Y + 1.5 * Yaxis };
                InsertAnnoFea(pt, chartTitle, fontsize * 1.0, id);
            }
        }
       
        private void InsertAnnoFea(IGeometry pGeometry, string annoName, double fontSize, int id)
        {
            IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize);
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annoFcl.CreateFeature();
            
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.Annotation = pElement;
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            //IAnnotationFeature pAnnoFeature = pFeature as IAnnotationFeature;
            //pAnnoFeature.Annotation = pElement;
            //pAnnoFeature.AnnotationClassID = pFeature.OID;
            //pAnnoFeature.LinkedFeatureID = id;
            // pFeature.set_Value(indexId, id);
            pFeature.Store();
            
        }

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
            pAnnoFeature.Annotation = ele;
            pAnnoFeature.AnnotationClassID = (pRepLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
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
                HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter,
                VerticalAlignment = esriTextVerticalAlignment.esriTVACenter,
                Text = txt
            };
            pEle = pTextElment as IElement;

            pEle.Geometry = pGeoTxt;
            return pTextElment;
        }
        #endregion
    }
}

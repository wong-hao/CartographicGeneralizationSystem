using System;
using System.Collections.Generic;
using System.Text;
using stdole;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using SMGI.Common;
using System.Linq;
using SMGI.Plugin.ThematicChart.ThematicChart;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.ThematicChart
{
    public class ThematicBcgCmd : SMGICommand
    {
        //底质法
        public ThematicBcgCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        Dictionary<string, ICmykColor> ColorsDic = null;
        Dictionary<string, string> GradeDic = null;
        public override void OnClick()
        {
            eles.Clear();
            pAc = m_Application.ActiveView;
            mapScale = (pAc as IMap).ReferenceScale;
            FrmThematicBcg frm = new FrmThematicBcg();
            DialogResult dr=   frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            string title= frm.GeoTitle;
            ColorsDic = frm.ColorsDic;
            GradeDic = frm.gradeData;
          
            string lyrname = frm.GeoLayer;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).Name== lyrname);
            })).ToArray();
            if (lyrs.Length == 0)
            {
                MessageBox.Show("不存在图层：" + lyrname);
                return;
            }
            ILayer pRepLayer = lyrs.First();
            var extent=((pRepLayer as IFeatureLayer).FeatureClass as IGeoDataset).Extent;
            IPoint anhorpoint = extent.LowerLeft;
            anhorpoint.Y -= 10 * 1e-3 * mapScale;

            m_Application.EngineEditor.StartOperation();
            IPoint orpt = new PointClass() { X = 0, Y = 0 };
            DrawLengend(title, orpt);
            var lyrsp= GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            ILayer pPointLayer = lyrsp.First();
            GraphicsHelper gh = new GraphicsHelper(pAc);
            int obj = 0;
            var remarker= gh.CreateFeaturesBgEx(eles, pPointLayer, anhorpoint,out obj, 50);
            CreateAnno(remarker,  pPointLayer, anhorpoint,title,  50, obj);
            LayerRenderer(pRepLayer as IFeatureLayer);
            m_Application.EngineEditor.StopOperation("底质法渲染");
        }
        private void LayerRenderer(IFeatureLayer  boualyr)
        {
            try
            {

                IUniqueValueRenderer pUniqueRen = new UniqueValueRendererClass();
                pUniqueRen.FieldCount = 1;
                pUniqueRen.set_Field(0, "NAME");
                IFeature fe;
                IFeatureCursor cursor = boualyr.FeatureClass.Search(null, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    string name = fe.get_Value(boualyr.FeatureClass.FindField("NAME")).ToString();
                    name = name.Trim();
                    if (GradeDic.ContainsKey(name))
                    {
                        pUniqueRen.AddValue(name, "NAME", ObtainSymbol(name));
                    }
                }
                //pUniqueRen.DefaultSymbol = sym as ISymbol;
                //pUniqueRen.DefaultLabel = "邻区";
                //pUniqueRen.UseDefaultSymbol = true;

                (boualyr as IGeoFeatureLayer).Renderer = pUniqueRen as IFeatureRenderer;
                m_Application.ActiveView.Refresh();
            }
            catch(Exception ex)
            {
                MessageBox.Show("质底法错误："+ex.Message);
            }
        }
        public void OverrideColorValueSet(IRepresentation rep, IColor pColor)
        {

            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            IBasicFillSymbol fillSym = ruleOrg.get_Layer(0) as IBasicFillSymbol;
            IGraphicAttributes ga = fillSym.FillPattern as IGraphicAttributes;
            if (fillSym != null)
            {
                if (ga.ClassName == "SolidColorPattern")
                {
                    int id = ga.get_IDByName("Color");
                    rep.set_Value(ga, id, pColor);
                }
                if (ga.ClassName == "GradientPattern")
                {
                    int id1 = ga.get_IDByName("Color1");
                    rep.set_Value(ga, id1, pColor);
                    int id2 = ga.get_IDByName("Color2");
                    rep.set_Value(ga, id2, pColor);
                }

            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
            rep.UpdateFeature();
            rep.Feature.Store();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(rep);
        }
        private ISymbol ObtainSymbol(string name)
        {
            string grade=GradeDic[name];
            ISimpleFillSymbol sym = new SimpleFillSymbolClass();
            sym.Color = ColorsDic[grade];
            ISimpleLineSymbol sl = new SimpleLineSymbolClass();
            sl.Style = esriSimpleLineStyle.esriSLSNull;
            sym.Outline = sl;
            return sym as ISymbol;
        }
        //绘制图例相关
        List<IElement> eles = new List<IElement>();
        double mapScale = 0;
        IActiveView pAc = null;
        private void DrawBgEle(int ct, IPoint pt, int f = 1)
        {
            double dis = 10 * 1e-3 * mapScale; ;//一个正方形长度
            double step = 10 * 1e-3 * mapScale * 0.25;//间距2.5cm

            double width = (dis + step) * ct;
            double height = 10 * dis;
            height *= f;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            IGeometry pgeo = gh.CreateRectangle(pt, width, -height);

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
        Dictionary<IPoint, string> annoTxt = new Dictionary<IPoint, string>();
        private void DrawLengend(string title, IPoint pBasePoint)
        {
            annoTxt.Clear();
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            GraphicsHelper gh = new GraphicsHelper(pAc);
           
           
            double  cubedis = 10 * 1e-3 * mapScale;
            
            int num = ColorsDic.Count;
            double total = cubedis * num;
            double ystep = cubedis + 5e-3 * mapScale;
            double fontsize = 8*2.83;
            double cubstep = 0;
             
            int j = 0;
            if (title != "")
            {
                //IPoint titlepoint = new PointClass() { X = pBasePoint.X + 2e-2 * mapScale, Y = pBasePoint.Y + 2e-2 * mapScale };
                //double cx = (titlepoint.X - pBasePoint.X) / total;
                //double cy = (titlepoint.Y - pBasePoint.Y) / total;
                //IPoint pt = new PointClass() { X = cx, Y = cy };
                ////var elet = gh.DrawTxt(titlepoint, title, 20);
                ////eles.Add(elet);
                //annoTxt.Add(pt, title);
            }
            foreach (var kv in ColorsDic)
            {

                IPoint point = new PointClass() { X = pBasePoint.X + cubstep, Y = pBasePoint.Y - (j) * ystep - 5e-3 * mapScale};
                IPolygon columngeo = gh.CreateRectangle(point, 2 * cubedis, cubedis);
                DrawPolygon(columngeo, kv.Value);
                //注记
                double stw = gh.GetStrWidth(kv.Key.ToString(), mapScale, fontsize);
                //double dy = cubedis * 0.65;
                IPoint lbpoint = new PointClass() { X = point.X + 3e-2 * mapScale + stw / 2, Y = point.Y - cubedis *0.5 };
                double cx = (lbpoint.X - pBasePoint.X) / total;
                double cy = (lbpoint.Y - pBasePoint.Y) / total;
                IPoint pt = new PointClass() { X = cx, Y = cy };
                annoTxt.Add(pt, kv.Key.ToString());
                //var ele = gh.DrawTxt(lbpoint, kv.Key.ToString(), fontsize);
                //eles.Add(ele);
                j++;
            }

            

        }
        private void DrawPolygon(IGeometry pgeo, IColor pcolor)
        {
            IGraphicsContainer pContainer = pAc as IGraphicsContainer;
            IElement pEl = null;
            IFillShapeElement polygonElement = new PolygonElementClass();
            ISimpleFillSymbol smsymbol = new SimpleFillSymbolClass();
            smsymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            smsymbol.Color = pcolor;
            ISimpleLineSymbol smline = new SimpleLineSymbolClass();
            smline.Style = esriSimpleLineStyle.esriSLSSolid;



            smline.Color = pcolor;
            smline.Width = 0.5;

            smsymbol.Outline = smline;
            polygonElement.Symbol = smsymbol;

            pEl = polygonElement as IElement;
            pEl.Geometry = pgeo as IGeometry;
            eles.Add(pEl);
            
        }

        private bool InsertAnnoFea(IFeatureClass pFeatCls, IGeometry pGeometry,ILayer pPointLayer,  string annoName, double fontSize, int id)
        {
            IFontDisp pFont = new StdFont()
            {
                Name = "黑体",
                Size = 2
            } as IFontDisp;

            IFeatureClass annocls = pFeatCls;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, pFont, fontSize);
         
           
           
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annocls.CreateFeature();
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.AnnotationClassID = (pPointLayer as IFeatureLayer).FeatureClass.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pAnnoFeature.Annotation = pElement;
            pFeature.Store();


            return true;
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
                VerticalAlignment=esriTextVerticalAlignment.esriTVACenter
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
        private void CreateAnno(IRepresentationMarker RepMarker, ILayer LayerPoint,IPoint pAncher,string title,double markersize, int id)
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LANNO");
            })).ToArray();
            ILayer pAnoLayer = lyrs.First();
            IFeatureClass fclAnno = (pAnoLayer as IFeatureLayer).FeatureClass;

            double curms = pAc.FocusMap.ReferenceScale;
            double height;
            double width;
            double Size = curms * 1.0e-3 * markersize;
            double fontsize = 0;
            //1.确定图例的实际大小
            if (RepMarker.Height < RepMarker.Width)
            {
                width = Size;
                height = Size * RepMarker.Height / RepMarker.Width;
            }
            else
            {
                height = Size;
                width = Size * RepMarker.Width / RepMarker.Height;
            }
            //图例的高度
            double lgHeight = (28.3465 * ColorsDic.Count / RepMarker.Height) * height;
            fontsize = (28.3465/RepMarker .Height )*height *0.3;
            //图例的宽度
            double lgWidth = width;
            //图例间间隔
            double valLg = (3.6929 / RepMarker.Height) * height;
            //确定文字大小
            double heightunit = 3.97427;//1号字体1万的高度
            fontsize = fontsize / (curms * heightunit * 1e-4);
            fontsize *= 2.83;
            int i = 1;
            GraphicsHelper gh = new GraphicsHelper(pAc);
            double max=0;
            foreach (var k in annoTxt)
            {
                double strWidth = gh.GetStrWidth(k.Value, curms, fontsize);
                double cx = pAncher.X + k.Key.X * lgHeight;
                double cy = pAncher.Y + k.Key.Y * lgHeight;
                IPoint pt = new PointClass() { X = cx, Y = cy };
                InsertAnnoFea(fclAnno, pt, LayerPoint, k.Value, fontsize*0.7, id);
                double d=k.Key.X*lgHeight;
                if(max<d )
                {
                    max=d;
                }
                i++;
            }
            if (title != "")
            {
                double cx = pAncher.X + max / 2.0;
                double cy = pAncher.Y + (28.3465 / RepMarker.Height) * height;
                IPoint pt = new PointClass() { X = cx, Y = cy };
                InsertAnnoFea(fclAnno, pt, LayerPoint, title, fontsize * 1.3, id);
            }
        }
    }
}
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
namespace SMGI.Plugin.ThematicChart
{
    public class JsonHelper
    {
        #region 饼图
        public static PieJson GetPieInfo(string jsonTxt)
        {
            try
            {
                PieJson pieinfo = new PieJson();
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonTxt)))
                {
                    DataContractJsonSerializer ds = new DataContractJsonSerializer(pieinfo.GetType());
                    pieinfo = (PieJson)ds.ReadObject(stream);
                }
                return pieinfo;
            }
            catch
            {
                return null;
            }
        }
        public static string GetJsonText(PieJson pieInfo)
        {
            string jsonText = "";
            DataContractJsonSerializer js = new DataContractJsonSerializer(pieInfo.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                js.WriteObject(ms, pieInfo);
                jsonText = Encoding.UTF8.GetString(ms.ToArray());
            }
            return jsonText;
        }
        #endregion
        #region 柱状图
        public static ColumnJson GetColumnInfo(string jsonTxt)
        {
            try
            {
                ColumnJson columninfo = new ColumnJson();
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonTxt)))
                {
                    DataContractJsonSerializer ds = new DataContractJsonSerializer(columninfo.GetType());
                    columninfo = (ColumnJson)ds.ReadObject(stream);
                }
                return columninfo;
            }
            catch
            {
                return null;
            }
        }
        public static string GetJsonText(ColumnJson columnInfo)
        {
            string jsonText = "";
            DataContractJsonSerializer js = new DataContractJsonSerializer(columnInfo.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                js.WriteObject(ms, columnInfo);
                jsonText = Encoding.UTF8.GetString(ms.ToArray());
            }
            return jsonText;
        }
        #endregion
        //将数据源转为json
        public static string JsonChartData(Dictionary<string, Dictionary<string, double>> chartDatas)
        {
            string jsonText = "";
            DataContractJsonSerializer js = new DataContractJsonSerializer(chartDatas.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                js.WriteObject(ms, chartDatas);
                jsonText = Encoding.UTF8.GetString(ms.ToArray());
            }
            return jsonText;
        }
        //将json转为字典类型数据源
        public static Dictionary<string, Dictionary<string, double>> CHDataSource(string jsonText)
        {
            Dictionary<string, Dictionary<string, double>> chartDs = new Dictionary<string, Dictionary<string, double>>();
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonText)))
            {
                DataContractJsonSerializer ds = new DataContractJsonSerializer(chartDs.GetType());
                chartDs = (Dictionary<string, Dictionary<string, double>>)ds.ReadObject(stream);
            }
            return chartDs;
        }
        public static void UpdateFeature(IFeature fe, string jsonTxt)
        {
            PieJson PieInfo = GetPieInfo(jsonTxt);
            ColumnJson columnInfo = GetColumnInfo(jsonTxt);
            if (PieInfo == null)
                return;
            PieHelper ph = new PieHelper();
            ColumnHelper ch = new ColumnHelper();
            List<IElement> eles = null;
            switch (PieInfo.ThematicType)
            {
                case "3D饼图":
                    eles = ph.Draw3DPieStatic(PieInfo, fe.Shape as IPoint);
                    break;
                case "3D环状饼图":
                    eles = ph.Draw3DRingStatic(PieInfo, fe.Shape as IPoint);
                    break;
                case "3D圆饼图":
                    eles = ph.Draw3DCirclePie(PieInfo, fe.Shape as IPoint);
                    break;
                case "3D柱状图":
                    eles = ch.Draw3DColumnChart(columnInfo, fe.Shape as IPoint);
                    break;
                case "2D柱状图":
                    eles = ch.Draw2DColumnChart(columnInfo, fe.Shape as IPoint);
                    break;
                case "分类柱状图":
                    eles = ch.DrawClassifyColumns(columnInfo, fe.Shape as IPoint);
                    break;
            }
            ICmykColor pnullcolor = new CmykColorClass();
            pnullcolor.CMYK = 6;
            pnullcolor.Black = 6;
            pnullcolor.Cyan = 0;
            pnullcolor.Magenta = 0;
            pnullcolor.NullColor = true;
            pnullcolor.RGB = 15790320;
            pnullcolor.Transparency = 0;
            pnullcolor.UseWindowsDithering = true;
            pnullcolor.Yellow = 0;
            IColor ColorNULL = pnullcolor;
            double size = PieInfo.Size;
            size = size * 2.83;
            fe.set_Value(fe.Fields.FindField("JsonTxt"), jsonTxt);
            fe.Store();

            //添加规则
            #region
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();

            ChartsToRepHelper.AddRepGraphics(eles, pGraphics, ColorNULL);

            #endregion
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
            //自由

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "LPOINT");
            })).ToArray();
            var player = lyrs.First();
            var rp = (player as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(fe.Shape, r);
            rep.Graphics.RemoveAll();
            rep.Graphics = repGraphics;

            rep.UpdateFeature();
            rep.Feature.Store();
        }


    }
}

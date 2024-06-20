using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;

namespace SMGI.Common
{

    public class GBAndLayerInfo
    {
        private GApplication app;
        LayerDictionary lyrDict = null;
        string defaultName = "数据字典.默认";
        IWorkspaceFactory wsf = null;
        public GBAndLayerInfo(GApplication application)
        {
            wsf = new ShapefileWorkspaceFactoryClass();

            app = application;
            if (app.AppConfig[defaultName] == null)
            {
                loadDefault();
            }
#if DEBUG
            loadDefault();
#endif

            try
            {
                string lyrDictStr = app.AppConfig[defaultName].ToString();
                lyrDict = (LayerDictionary)Common.GConvert.Base64ToObject(lyrDictStr);
            }
            catch
            { }

        }

        public LayerDictionaryItem GetLayerItem(string key)
        {
            LayerDictionaryItem item = null;
            try
            {
                if (lyrDict != null)
                {
                    item = lyrDict.ItemByName(key);
                    if (item == null)
                    {
                        item = lyrDict.ItemByCaption(key);
                    }
                }

            }
            catch
            { }
            return item;
        }

        public List<string> GetGBsForLayer(string lyrKey)
        {
            List<string> result = new List<string>();
            if (lyrDict != null)
            {
                LayerDictionaryItem l = null;
                if ((l = lyrDict.ItemByName(lyrKey)) != null)
                {
                    foreach (GBItem item in l.Gbitems)
                    {
                        result.Add(item.GBCode);
                    }
                }
            }
            return result;
        }


        public string GetGeotype(string gbkey)
        {
            string result = null;
            Regex regex = new Regex("^[\u4e00-\u9fa5]+");
            if (regex.IsMatch(gbkey))
            {
                return "面";
            }
            if (gbkey == "1000000")
            {
                return "面";
            }
            if(lyrDict !=null)
            {
                foreach (LayerDictionaryItem l in lyrDict.Items)
                {
                    foreach (GBItem item in l.Gbitems)
                    {
                        if (item.GBCode == gbkey)
                        {
                            result = l.LayerGeoType;
                            return result;
                        }
                    }
                }
            }
            return result;
        }

        public string GetLayerCaption(string key)
        {
            LayerDictionaryItem item = null;
            if (lyrDict != null)
            {
                item = lyrDict.ItemByName(key);
            }
            return item == null ? string.Empty : item.LayerCaption;
        }

        public string[] GetLayerForGB(string gbkey, string geoType)
        {
            if (lyrDict != null)
            {
                return lyrDict.getLayersForGB(gbkey, geoType);
            }
            else
            {
                return new string[] { };
            }
        }

        private void loadDefault()
        {
            try
            {

                IWorkspace ws = wsf.OpenFromFile(app.ExePath, 0);
                if (ws != null)
                {
                    ITable t = (ws as IFeatureWorkspace).OpenTable("GBAndLayer.dbf");
                    if (t != null)
                    {
                        lyrDict = new LayerDictionary();

                        int idx_name = t.FindField("图层名称");
                        int idx_caption = t.FindField("数据分层");
                        int idx_group = t.FindField("要素分类");
                        int idx_geotype = t.FindField("几何类型");
                        int idx_gbcode = t.FindField("GB_要素代");
                        int idx_gbname = t.FindField("要素名称");
                        int idx_gbgeo = t.FindField("几何特征");

                        ICursor cur = t.Search(null, true);
                        IRow r = null;

                        while ((r = cur.NextRow()) != null)
                        {

                            string LayerName = r.get_Value(idx_name).ToString();
                            string LayerGroup = r.get_Value(idx_group).ToString();
                            string LayerCaption = r.get_Value(idx_caption).ToString();
                            string LayerGeoType = r.get_Value(idx_geotype).ToString();

                            GBItem gi = new GBItem();
                            gi.GBCode = r.get_Value(idx_gbcode).ToString();
                            gi.GBGeometry = r.get_Value(idx_gbname).ToString();
                            gi.GBName = r.get_Value(idx_gbgeo).ToString();

                            lyrDict.AddItem(LayerName, LayerGroup, LayerCaption, LayerGeoType, gi);
                        }

                        app.AppConfig[defaultName] = Common.GConvert.ObjectToBase64(lyrDict);
                    }
                }
                if (ws != null)
                {

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
                }
            }
            catch (Exception ee)
            {

            }
        }

    }
}

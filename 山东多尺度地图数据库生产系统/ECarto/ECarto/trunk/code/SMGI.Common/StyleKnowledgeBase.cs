using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Dynamic;
using System.IO;
using ESRI.ArcGIS.Display;
using System.Xml.Serialization;

namespace SMGI.Common
{
    [Serializable]
    public class FieldInfo
    {
        private List<fileditem> fieldLists = null;
        public List<fileditem> FieldLists
        {
            get { return fieldLists; }
            set { fieldLists = value; }
        }

        public FieldInfo()
        {
            //fieldLists = FieldInfo.InitFieldList();
        }
        public void Init() { fieldLists = FieldInfo.InitFieldList(); }

        public object GetValue(string key)
        {
            object o = null;
            foreach (fileditem item in fieldLists)
            {
                if (item.FieldName == key)
                {
                    o = item.FieldValue;
                    break;
                }
            }
            return o == null ? string.Empty : o;
        }
        public void SetValue(string key, object value)
        {
            foreach (fileditem item in fieldLists)
            {
                if (item.FieldName == key)
                {
                    item.FieldValue = value;
                }
            }
        }

        public static List<fileditem> InitFieldList()
        {
            List<fileditem> fielditems = new List<fileditem>();
            fielditems.Add(new fileditem("GB", "GB", ""));
            fielditems.Add(new fileditem("CODE", "代码", ""));
            fielditems.Add(new fileditem("GBName", "名称", ""));
            fielditems.Add(new fileditem("StyleID", "符号库id", ""));
            fielditems.Add(new fileditem("GeoType", "几何类型", ""));

            fielditems.Add(new fileditem("RenderField", "渲染字段", ""));
            fielditems.Add(new fileditem("RenderValue", "渲染值", ""));

            fielditems.Add(new fileditem("Label1", "加注字段1", ""));
            fielditems.Add(new fileditem("LabelID1", "labelid1", ""));
            fielditems.Add(new fileditem("Label2", "加注字段2", ""));
            fielditems.Add(new fileditem("LabelID2", "labelid2", ""));
            fielditems.Add(new fileditem("Label3", "加注字段3", ""));
            fielditems.Add(new fileditem("LabelID3", "labelid3", ""));

            fielditems.Add(new fileditem("Angle", "方位", ""));
            fielditems.Add(new fileditem("AngleCode", "代码索引", ""));
            fielditems.Add(new fileditem("AngleLayer", "要素图层", ""));
            fielditems.Add(new fileditem("AngleRelLayer", "关联要素图", ""));
            return fielditems;
        }
    }

    [Serializable]
    public class fileditem
    {
        //must english
        private string fieldname;
        public string FieldName { get { return fieldname; } set { fieldname = value; } }
        //can be chinese
        private string fieldintable;
        public string FieldinTable { get { return fieldintable; } set { fieldintable = value; } }
        private object fieldvalue;
        public object FieldValue { get { return fieldvalue; } set { fieldvalue = value; } }

        public fileditem(string _name, string _nameintable, object _value)
        {
            fieldname = _name; fieldintable = _nameintable; fieldvalue = _value;
        }
        public fileditem() { }
    }
    [Serializable]
    public class SymbolKnowledgeItem
    {

        FieldInfo fi = null;
        public FieldInfo Fi
        {
            get { return fi; }
            set { fi = value; }
        }


        private IStyleGalleryItem sym = null;
        [XmlIgnore]
        public IStyleGalleryItem StyleItem
        {
            get { return sym; }
            set { sym = value; }
        }


        /// <summary>
        /// <para>GB,GB</para>
        /// <para>CODE,代码</para>
        /// <para>GBName,名称</para>
        /// <para>StyleID, 符号库id</para>
        /// <para>GeoType, 几何类型</para>
        /// <para>Angle, 方位</para>
        /// <para>AngleCode, 代码索引</para>
        /// <para>AngleLayer, 要素图层</para>
        /// <para>AngleRelLayer, 关联要素图</para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object this[string id]
        {
            get
            {
                return fi.GetValue(id);
            }
            set
            {
                fi.SetValue(id, value);
            }
        }

        public SymbolKnowledgeItem(IRow r)
        {
            fi = new FieldInfo();
            fi.Init();
            FillFieldInfo(r);
        }
        public SymbolKnowledgeItem(IStyleGalleryItem item)
        {
            fi = new FieldInfo();
            fi.Init();
            FillFieldInfo(item);
            sym = item;
        }

        private void FillFieldInfo(IStyleGalleryItem it)
        {
            foreach (fileditem item in fi.FieldLists)
            {
                item.FieldValue = it.Name;
            }
            if (it.Item is ISymbol)
            {
                if (it.Item is IFillSymbol)
                {
                    this["GeoType"] = "面";
                }
                else if (it.Item is IMarkerSymbol)
                {
                    this["GeoType"] = "点";
                }
                else if (it.Item is ILineSymbol)
                {
                    this["GeoType"] = "线";
                }
                else
                {

                }

            }
        }

        private void FillFieldInfo(IRow row)
        {
            int i = -1;
            object o = null;
            foreach (fileditem item in fi.FieldLists)
            {
                i = -1;
                o = null;
                if ((i = row.Fields.FindField(item.FieldinTable)) > 0)
                {
                    if ((o = row.get_Value(i)) != null)
                    {
                        item.FieldValue = ParseObject(o);
                    }
                }
            }
        }

        private object ParseObject(object o)
        {
            string s = o.ToString();
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
            {
                return string.Empty;
            }
            else
            {
                s = s.Trim();
                //if (s.Contains(","))
                //{
                //    string[] ss = s.Split(',');
                //    if (ss.Length > 1)
                //    {
                //        return ss;
                //    }
                //    else { return s; }
                //}
                //else
                //{
                return s;
                //}
            }
        }

        public SymbolKnowledgeItem() { }
    }

    public class StyleKnowledgeConfig
    {
        public string StyleKnowledgeName { get; set; }
        public string StyleFile { get; set; }
        public string RenderField { get; set; }
        public string DBFName { get; set; }
        public List<SymbolKnowledgeItem> SymbolsBase { get; set; }


        public StyleKnowledgeConfig(GApplication application, string name, string filepath, string field, string dbf = "")
        {
            StyleKnowledgeName = name;
            StyleFile = filepath;
            RenderField = field;
            DBFName = dbf;
            SymbolsBase = new List<SymbolKnowledgeItem>();
            if (DBFName != string.Empty) { LoadFromDBF(application); }
            else { LoadFromServerStyle(application); }
        }

        private void LoadFromDBF(GApplication app)
        {
            string name = DBFName;
            IWorkspace ws = GApplication.ShpFactory.OpenFromFile(GApplication.ExePath, 0);
            ITable tb = (ws as IFeatureWorkspace).OpenTable(name);
            ICursor c = tb.Search(null, true);
            IRow row = null;
            int idx = tb.FindField(RenderField);
            if (idx < 0) { return; }
            while ((row = c.NextRow()) != null)
            {
                if (row.get_Value(idx) != null)
                {
                    SymbolKnowledgeItem ski = new SymbolKnowledgeItem(row);
                    SymbolsBase.Add(ski);
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(tb);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
        }

        private void LoadFromServerStyle(GApplication app)
        {
            SymbolsBase.Clear();

            string[] symbols = new string[3]{ 
                SymbolClassString.点符号,
                SymbolClassString.线符号,
                SymbolClassString.面符号
            };

            if (StyleManager.IsValidServerStyleFile(StyleFile))
            {
                foreach (var item in symbols)
                {
                    IStyleGalleryItem[] styleItems = app.StyleMgr.GetAllStyleItems(StyleFile, app.StyleMgr.StyleGallery, item);
                    foreach (var it in styleItems)
                    {
                        SymbolKnowledgeItem ski = new SymbolKnowledgeItem(it);
                        SymbolsBase.Add(ski);
                    }
                }
            }
        }


        public StyleKnowledgeConfig()
        {
        }
    }

    public class StyleKnowledgeBase
    {
        public static string KnowledgeBaseName = "符号知识库";
        public static string DefaultConfigName = "符号知识库.默认";

        public List<StyleKnowledgeConfig> styleConfigs;

        public string ConfigName { get; set; }

        private StyleKnowledgeConfig styleKnowledgeConfig;

        public StyleKnowledgeConfig ActiveStyleConfig { get { return styleKnowledgeConfig; } }

        public List<SymbolKnowledgeItem> SymbolsBase
        {
            get
            {
                return styleKnowledgeConfig.SymbolsBase;
            }
        }


        private SMGI.Common.GApplication app;

        /// <summary>
        /// 根据国标和几何类型得到符号库知识项,几何类型包括以下几项：
        /// <para>标注点</para>
        /// <para>点</para>
        /// <para>定位点</para>
        /// <para>范围线构面</para>
        /// <para>轮廓线构面</para>
        /// <para>面</para>
        /// <para>线</para>
        /// <para>有向点</para>
        /// <para>有向线</para>
        /// <para>中心线</para>
        /// </summary>
        ///
        /// <param name="gb">GB码</param>
        /// <param name="geo">几何类型</param>
        ///
        /// <returns></returns>
        /// 
        public List<SymbolKnowledgeItem> GetKnowledgeItem(string gb, string geo)
        {
            List<SymbolKnowledgeItem> its = new List<SymbolKnowledgeItem>();
            if (SymbolsBase != null)
            {
                foreach (SymbolKnowledgeItem item in SymbolsBase)
                {
                    if (item["GeoType"].ToString() == geo && item["GB"].ToString() == gb)
                    {
                        its.Add(item);
                    }
                }
            }
            return its;
        }

        public StyleKnowledgeBase(SMGI.Common.GApplication gapp)
        {
            ConfigName = DefaultConfigName;
            app = gapp;
            styleConfigs = new List<Common.StyleKnowledgeConfig>();
            Load();
            Activate(ConfigName);
        }

        public void AddConfig(StyleKnowledgeConfig SKC)
        {
            if (SKC != null)
            {
                if (ISConfigExist(SKC.StyleKnowledgeName))
                {

                }
                else
                {
                    styleConfigs.Add(SKC);
                }
            }
        }

        public bool ISConfigExist(string name)
        {
            return styleConfigs.Exists(new Predicate<StyleKnowledgeConfig>(x => { return x.StyleKnowledgeName == name; }));
        }

        public void Activate(string name)
        {
            styleKnowledgeConfig = null;
            foreach (var item in styleConfigs)
            {
                if (item.StyleKnowledgeName == name)
                {
                    styleKnowledgeConfig = item;
                    break;
                }
            }
            if (styleKnowledgeConfig == null && name != DefaultConfigName)
            {
                Activate(DefaultConfigName);
            }
        }

        private void Load()
        {
            try
            {
                if (app.AppConfig[KnowledgeBaseName] != null)
                {
                    Init();
                    object o = GConvert.Base64ToObject(app.AppConfig[KnowledgeBaseName].ToString());
                    if (o == null)//兼容之前的代码
                    {
                        Init();
                        o = GConvert.Base64ToObject(app.AppConfig[KnowledgeBaseName].ToString());
                    }
                    if (o != null)
                    {
                        StyleKnowledgeConfig[] cs = (StyleKnowledgeConfig[])o;
                        styleConfigs.AddRange(cs);
                    }

                }
                else
                {
                    Init();
                }
            }
            catch { }
        }

        private void Init()
        {
            StyleKnowledgeConfig SKC = new StyleKnowledgeConfig(
                       app, DefaultConfigName, app.StyleMgr.DefaultStylePath, "GB", "符号知识库.dbf");
            styleConfigs.Add(SKC);
            Save();
        }

        public void Save()
        {
            try
            {
                if (styleConfigs != null && styleConfigs.Count > 0)
                {
                    app.AppConfig[KnowledgeBaseName] = GConvert.ObjectToBase64(styleConfigs.ToArray());
                }
            }
            catch { }
        }


    }
}

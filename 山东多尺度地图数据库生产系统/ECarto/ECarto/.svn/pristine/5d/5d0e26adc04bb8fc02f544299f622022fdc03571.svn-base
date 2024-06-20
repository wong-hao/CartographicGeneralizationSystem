using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Collections;
using ESRI.ArcGIS.esriSystem;
using System.Collections.Generic;
using stdole;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Runtime.InteropServices;
using SMGI.Plugin.EmergencyMap.MapDeOut;
using System.IO;
using System.Xml;
using System.Xml.Linq;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmLgSet : Form
    {
        GApplication app;
        LengendModel lgModel = LengendModel.MapView;
        IEnvelope LgEnvelope = null;
        IEnvelope LgEnvelopeOut = null;
        public LengendDyCreateCmd cmd = null;
        Dictionary<string, List<string>> LengendFliters = new Dictionary<string, List<string>>();
        double OldLineLen = 0;
        string lgRulePath = string.Empty;
        List<string> fliterLyrs = new List<string>();//过滤图层
        public FrmLgSet(LengendModel model, IEnvelope pen, IEnvelope penout)
        {
            app = GApplication.Application;
            InitializeComponent();
            Dictionary<string, string> envString = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            lgRulePath = GApplication.Application.Template.Root + "\\底图\\" + envString["BaseMap"] + "\\" + envString["MapTemplate"] + "\\图例\\";
            cmbTemplate.Items.Clear();
            //加载图例类型
            string xmlpath = lgRulePath + "LengendTemplate.Xml";
            XDocument doc;
            doc = XDocument.Load(xmlpath);
            var items = doc.Element("Template").Elements("Lengend");
            cmbTemplate.Items.Clear();
            foreach (var item in items)
            {
                cmbTemplate.Items.Add(item.Attribute("name").Value);
            }
            if (cmbTemplate.Items.Count > 0)
                cmbTemplate.SelectedIndex = 0;
            lgRulePath += cmbTemplate.SelectedItem.ToString() + "\\";
            lgModel = model;
            LgEnvelope = pen;
            LgEnvelopeOut = penout;

            if (model == LengendModel.MapView)
            {
                sortLyrs = new List<string>() { "RESP", "AGNP", "LRDL", "JJTH", "LRRL", "HYDL", "HYDA", "TERP", "LFCP" };
                if (File.Exists(lgRulePath + "LengendFliter.xml"))
                {
                    doc = XDocument.Load(lgRulePath + "LengendFliter.xml");
                    var lengends = doc.Element("Lengend").Elements("Item");
                    foreach (var item in lengends)
                    {
                        string fclname = item.Attribute("FeatureClass").Value;
                        List<string> ids = item.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        LengendFliters[fclname] = ids;
                    }
                    if (doc.Element("Lengend").Elements("Layer") != null)
                    {
                        foreach (var lyr in doc.Element("Lengend").Elements("Layer"))
                        {
                            string fclname = lyr.Attribute("FeatureClass").Value;

                            fliterLyrs.Add(fclname);
                        }
                    }
                }
                LoadLengendItemsd();
            }
            if (model == LengendModel.Template)
            {
                string path = lgRulePath + @"LengendRule.xml";
                LoadLengendXml(path);
            }
            if (cmbMapType.Items.Count > 0)
                cmbMapType.SelectedIndex = 0;
            LoadNearestSize();
        }
        //从模板读取图例项
        private void LoadLengendXml(string path)
        {


            XDocument doc;

            {
                doc = XDocument.Load(path);
                var content = doc.Element("LengendTemplate").Element("Lengend");
                //加载树
                var lyrs = content.Elements("Layer");
                foreach (var lyr in lyrs)
                {
                    string lyrName = lyr.Attribute("LayerName").Value;
                    if (lyrName == "注记图例")
                    {
                        TreeNode annoNodelyr = new TreeNode(lyrName);
                        var drs = lyr.Elements("DataRow");
                        foreach (var dr in drs)
                        {
                            string check = dr.Attribute("display").Value;
                            string name = dr.Element("AnnoName").Value;
                            string fontname = dr.Element("AnnoFontName").Value;
                            string note = dr.Element("AnnoNote").Value;
                            double fontsize = double.Parse(dr.Element("AnnoFontSize").Value);
                            string color = dr.Element("AnnoColor").Value;
                            var cmyk = AnnoGeometryHelper.GetColorByString(color);
                            AnnoItemInfo itemAnno = new AnnoItemInfo
                            {
                                FontSize = fontsize,
                                AnnoNote = note,
                                FontName = fontname,
                                AnnoColor = cmyk,
                                AnnoName = name
                            };
                            TreeNode nodelyr = new TreeNode(name);
                            nodelyr.Tag = itemAnno;
                            nodelyr.Checked = bool.Parse(check);
                            annoNodelyr.Nodes.Add(nodelyr);
                        }
                        annoNodelyr.Checked = bool.Parse(lyr.Attribute("Checked").Value);
                        tvItems.Nodes.Add(annoNodelyr);
                    }
                    else
                    {
                        TreeNode nodelyr = new TreeNode(lyrName);
                        foreach (var item in lyr.Elements("ItemInfo"))
                        {
                            string ruleID = item.Attribute("RuleID").Value;
                            string ruleName = item.Value;
                            TreeNode ruleItem = new TreeNode(ruleName);
                            ruleItem.Tag = int.Parse(ruleID);
                            ruleItem.Checked = bool.Parse(item.Attribute("Checked").Value);
                            nodelyr.Nodes.Add(ruleItem);
                        }
                        nodelyr.Checked = bool.Parse(lyr.Attribute("Checked").Value);
                        tvItems.Nodes.Add(nodelyr);
                    }

                }
                //加载基本信息
                var lengendInfo = doc.Element("LengendTemplate").Element("LengendInfo");
                var itemInfo = lengendInfo.Element("Column");
                numColumn.Value = int.Parse(itemInfo.Value);
                //图例标题字体
                itemInfo = lengendInfo.Element("TitleFontSize");
                TitleFontSize.Text = itemInfo.Value;
                //图例文字水平间隔
                itemInfo = lengendInfo.Element("CharactersXStep");
                txtAnnoStep.Text = itemInfo.Value;
                //图例项字体
                itemInfo = lengendInfo.Element("ItemFontSize");
                ItemFontSize.Text = itemInfo.Value;
                //图例项垂直间隔
                itemInfo = lengendInfo.Element("ItemYStep");
                VerticalInterval.Text = itemInfo.Value;
                //图例项水平间隔
                itemInfo = lengendInfo.Element("ItemXStep");
                HorizontalInterval.Text = itemInfo.Value;
                //比例尺间隔
                itemInfo = lengendInfo.Element("ScaleBarStep");
                if (itemInfo != null)
                {
                    TxtscaleBar.Text = itemInfo.Value;
                }
                //图例类型
                itemInfo = lengendInfo.Element("LegendType");
                if (itemInfo != null)
                {
                    cmbTemplate.SelectedIndex = Convert.ToInt32(itemInfo.Value);
                }
                //图例项尺寸
                //线长度
                if (lengendInfo.Element("PlLegendLength") != null)
                {
                    itemInfo = lengendInfo.Element("PlLegendLength");
                    this.txtLgEleLen.Text = itemInfo.Value;

                }
                //面长度
                if (lengendInfo.Element("PoLegendLength") != null)
                {
                    itemInfo = lengendInfo.Element("PoLegendLength");
                    this.txtLgEleALen.Text = itemInfo.Value;
                }
                //面宽度
                if (lengendInfo.Element("PoLegendWidth") != null)
                {
                    itemInfo = lengendInfo.Element("PoLegendWidth");
                    this.txtLgEleAWid.Text = itemInfo.Value;
                }

            }


        }
        //从外部经验库xml加载
        private void LoadLengendOutXml(string fileName)
        {


            XDocument doc;
            tvItems.Nodes.Clear();

            {
                doc = XDocument.Load(fileName);
                var maplengend = doc.Element("Template").Element("Content").Element("MapLengend");
                var content = maplengend.Element("Lengend");
                //加载树
                var lyrs = content.Elements("Layer");
                foreach (var lyr in lyrs)
                {
                    string lyrName = lyr.Attribute("LayerName").Value;
                    if (lyrName == "注记图例")
                    {
                        TreeNode annoNodelyr = new TreeNode(lyrName);
                        var drs = lyr.Elements("DataRow");
                        foreach (var dr in drs)
                        {
                            string check = dr.Attribute("display").Value;
                            string name = dr.Element("AnnoName").Value;
                            string fontname = dr.Element("AnnoFontName").Value;
                            string note = dr.Element("AnnoNote").Value;
                            double fontsize = double.Parse(dr.Element("AnnoFontSize").Value);
                            string color = dr.Element("AnnoColor").Value;
                            var cmyk = AnnoGeometryHelper.GetColorByString(color);
                            AnnoItemInfo itemAnno = new AnnoItemInfo
                            {
                                FontSize = fontsize,
                                AnnoNote = note,
                                FontName = fontname,
                                AnnoColor = cmyk,
                                AnnoName = name
                            };
                            TreeNode nodelyr = new TreeNode(name);
                            nodelyr.Tag = itemAnno;
                            nodelyr.Checked = bool.Parse(check);
                            annoNodelyr.Nodes.Add(nodelyr);
                        }
                        annoNodelyr.Checked = bool.Parse(lyr.Attribute("Checked").Value);
                        tvItems.Nodes.Add(annoNodelyr);
                    }
                    else
                    {
                        TreeNode nodelyr = new TreeNode(lyrName);
                        foreach (var item in lyr.Elements("ItemInfo"))
                        {
                            string ruleID = item.Attribute("RuleID").Value;
                            string ruleName = item.Value;
                            TreeNode ruleItem = new TreeNode(ruleName);
                            ruleItem.Tag = int.Parse(ruleID);
                            ruleItem.Checked = bool.Parse(item.Attribute("Checked").Value);
                            nodelyr.Nodes.Add(ruleItem);
                        }
                        nodelyr.Checked = bool.Parse(lyr.Attribute("Checked").Value);
                        tvItems.Nodes.Add(nodelyr);
                    }

                }
                //加载基本信息
                var lengendInfo = maplengend.Element("LengendInfo");
                //图例列数
                var itemInfo = lengendInfo.Element("Column");
                numColumn.Value = int.Parse(itemInfo.Value);
                //图例文字水平间隔
                itemInfo = lengendInfo.Element("CharactersXStep");
                txtAnnoStep.Text = itemInfo.Value;
                //图例标题字体
                itemInfo = lengendInfo.Element("TitleFontSize");
                TitleFontSize.Text = itemInfo.Value;
                //图例项字体
                itemInfo = lengendInfo.Element("ItemFontSize");
                ItemFontSize.Text = itemInfo.Value;
                //图例项垂直间隔
                itemInfo = lengendInfo.Element("ItemYStep");
                VerticalInterval.Text = itemInfo.Value;
                //图例项水平间隔
                itemInfo = lengendInfo.Element("ItemXStep");
                HorizontalInterval.Text = itemInfo.Value;
                //比例尺间隔
                itemInfo = lengendInfo.Element("ScaleBarStep");
                TxtscaleBar.Text = itemInfo.Value;
                //图例类型
                itemInfo = lengendInfo.Element("LegendType");
                cmbTemplate.SelectedIndex = Convert.ToInt32(itemInfo.Value);
                //图例尺寸
                //线长度
                if (lengendInfo.Element("PlLegendLength") != null)
                {
                    itemInfo = lengendInfo.Element("PlLegendLength");
                    this.txtLgEleLen.Text = itemInfo.Value;

                }
                //面长度
                if (lengendInfo.Element("PoLegendLength") != null)
                {
                    itemInfo = lengendInfo.Element("PoLegendLength");
                    this.txtLgEleALen.Text = itemInfo.Value;
                }
                //面宽度
                if (lengendInfo.Element("PoLegendWidth") != null)
                {
                    itemInfo = lengendInfo.Element("PoLegendWidth");
                    this.txtLgEleAWid.Text = itemInfo.Value;
                }
                //图例类型
                if (lengendInfo.Element("LegendType") != null)
                {
                    itemInfo = lengendInfo.Element("LegendType");
                    cmbTemplate.SelectedIndex = Convert.ToInt32(itemInfo.Value);
                }
            }


        }
        //从当前地图视图读取图例项
        private void LoadLengendItemsd()
        {
            // 要素对应的制图表达
            List<string> filterStrs = new List<string>(new string[] { "桥梁隧道", "水系面边线", "BOUL", "QJA", "QJL", "ROADTEMP", "BRGA", "AANL", "VEGA", "BOUA", "BOUA6", "BOUA5", "LPOINT", "LLINE", "LPOLY", "ATTACHEDAREAMASK", "CLIPBOUNDARY", "GRID", "SDM", "PAGE", "_ATTACH" });
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => !(l is IFDOGraphicsLayer) && (l is IGeoFeatureLayer) && l.Visible).ToArray();
            var lyrdic = new Dictionary<ILayer, Dictionary<int, string>>();
            LgLyrDic = new List<string>();
            foreach (var lyr in fliterLyrs)
            {
                if (!filterStrs.Contains(lyr))
                {
                    filterStrs.Add(lyr);
                }
            }
            foreach (ILayer lyr in lyrs)
            {
                //  string name = lyr.Name.ToUpper();
                string name = (lyr as IFeatureLayer).FeatureClass.AliasName.ToUpper();

                if (!filterStrs.Contains(name))
                {

                    if (name == "JJTH")
                    {
                        name = "BOUL";
                        var lyr0 = GApplication.Application.Workspace.LayerManager.GetLayer(l => !(l is IFDOGraphicsLayer) && (!l.Name.Contains("海岸线")) && (l is IGeoFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == name).FirstOrDefault();
                        if (lyr0 == null)
                            continue;

                        var dic = UniqueValueRepRender(lyr0 as IFeatureLayer, "RuleID");
                        if (dic.Count == 0)
                            continue;
                        LgLyrDic.Add(name);
                        lyrdic[lyr0] = dic;
                    }
                    else
                    {
                        var dic = UniqueValueRepRender(lyr as IFeatureLayer, "RuleID");
                        if (dic.Count == 0)
                            continue;
                        LgLyrDic.Add(name);
                        lyrdic[lyr] = dic;
                    }
                }

            }
            //排序
            var list = lyrdic.ToList();
            LoadItemOrder();
            if (itemOrders.Count == 0)
            {
                list.Sort(CompareLyr);
                list.Reverse();
            }
            else
            {
                if (itemOrders.Count == 0)
                {
                    LoadItemOrder();
                }
                foreach (var kv in list)
                {
                    var fcName = (kv.Key as IFeatureLayer).FeatureClass.AliasName.ToUpper();

                    if (!itemOrders.Keys.Contains(fcName))
                    {
                        itemOrders.Add(fcName, new List<string>());
                    }
                }

                // list.Sort(CompareLyr);
                list.Sort(CompareLengendLyr);
                // list.Reverse();
            }
           
            tvItems.Nodes.Clear();
            foreach (var l in list)
            {
                var lyr = l.Key;
                string name = lyr.Name.ToUpper();
                string fclName = (lyr as IFeatureLayer).FeatureClass.AliasName;
                sortLyrName = fclName.ToUpper();
                List<int> lyrRuleIDs = l.Value.Keys.ToList();
                if (itemOrders.Count > 0 && itemOrders.ContainsKey(sortLyrName) && itemOrders[sortLyrName].Count >= 0)
                {
                    foreach (var item in lyrRuleIDs)
                    {
                        if (!itemOrders[sortLyrName].Contains(item.ToString()))
                        {
                            itemOrders[sortLyrName].Add(item.ToString());
                        }
                    }
                    
                    lyrRuleIDs.Sort(CompareRuleIDLyr);
                }

                TreeNode nodelyr = new TreeNode(name);
                foreach(var key in lyrRuleIDs)
               // foreach (var kv in l.Value)
                {
                    //过滤图例项目
                    if (LengendFliters.ContainsKey(fclName))
                    {
                        if (LengendFliters[fclName].Contains(key.ToString()))
                            continue;
                    }
                    TreeNode item = new TreeNode(l.Value[key]);
                    item.Tag = key;
                    if (!l.Value[key].Contains("跳绘"))
                    {
                        item.Checked = true;
                    }
                    nodelyr.Nodes.Add(item);

                }
                nodelyr.Tag = lyr;
                nodelyr.Checked = true;
                tvItems.Nodes.Add(nodelyr);
            }
             
            list.Clear();
            list = null;
            //加载说明图例
            LoadAnnoLegend();
        }
        string sortLyrName = string.Empty;
        //图层排序 fclName->ruleIDs
        Dictionary<string, List<string>> itemOrders = new Dictionary<string, List<string>>();
        /// 对图例项进行排序ItemInfo
        private void LoadItemOrder()
        {
            if (File.Exists(lgRulePath + "LengendOrder.xml"))
            {
                var doc = XDocument.Load(lgRulePath + "LengendOrder.xml");
                var lengends = doc.Element("Lengend").Elements("Item");
                foreach (var item in lengends)
                {
                    string fclname = item.Attribute("FeatureClass").Value.ToUpper();
                    List<string> ids = item.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    itemOrders[fclname] = ids;
                }

            }
        }
        //图层排序
        int CompareLengendLyr(KeyValuePair<ILayer, Dictionary<int, string>> item1, KeyValuePair<ILayer, Dictionary<int, string>> item2)
        {
            if (itemOrders.Count == 0)
                return 0;

            string fclName1 = (item1.Key as IFeatureLayer).FeatureClass.AliasName.ToUpper();
            string fclName2 = (item2.Key as IFeatureLayer).FeatureClass.AliasName.ToUpper();
            int termIndex1 = System.Array.IndexOf(itemOrders.Keys.ToArray(), fclName1);
            int termIndex2 = System.Array.IndexOf(itemOrders.Keys.ToArray(), fclName2);

            return termIndex1.CompareTo(termIndex2);
        }
        //图层内部排序
        int CompareRuleIDLyr(int item1,  int  item2)
        {
            if (itemOrders.Count == 0)
                return 0;

            int ruleID1 = itemOrders[sortLyrName].IndexOf(item1.ToString());
            int ruleID2 = itemOrders[sortLyrName].IndexOf(item2.ToString());


            return ruleID1.CompareTo(ruleID2);
        }
        //加载说明图例
        private void LoadAnnoLegend()
        {
            string path = lgRulePath + @"LengendAnnno.xml";

            XDocument doc;
            TreeNode annoNode = new TreeNode("注记图例");

            {
                doc = XDocument.Load(path);
                var content = doc.Element("DataTable");
                var lyrs = content.Elements("DataRow");
                foreach (var lyr in lyrs)
                {
                    string check = lyr.Attribute("display").Value;
                    string name = lyr.Element("AnnoName").Value;
                    string fontname = lyr.Element("AnnoFontName").Value;
                    string note = lyr.Element("AnnoNote").Value;
                    double fontsize = double.Parse(lyr.Element("AnnoFontSize").Value);
                    string color = lyr.Element("AnnoColor").Value;
                    var cmyk = AnnoGeometryHelper.GetColorByString(color);
                    AnnoItemInfo itemAnno = new AnnoItemInfo
                    {
                        FontSize = fontsize,
                        AnnoNote = note,
                        FontName = fontname,
                        AnnoColor = cmyk,
                        AnnoName = name
                    };
                    TreeNode nodelyr = new TreeNode(name);
                    nodelyr.Tag = itemAnno;
                    nodelyr.Checked = bool.Parse(check);
                    annoNode.Nodes.Add(nodelyr);
                }

            }
            tvItems.Nodes.Insert(0, annoNode);

        }
        /// <summary>
        /// 负数 表示非制图表达图层
        /// </summary>
        /// <param name="pGeoLayer"></param>
        /// <returns></returns>
        private Dictionary<int, string> LoadSimpleRendererLayer(IGeoFeatureLayer pGeoLayer)
        {
            var rulesNamesDic = new Dictionary<int, string>();
            IWorkspace ws2 = GApplication.Application.Workspace.EsriWorkspace as IWorkspace;
            IDataset ds = pGeoLayer.FeatureClass as IDataset;

            #region
            IFeatureRenderer feRenderer = pGeoLayer.Renderer;
            if (feRenderer is ISimpleRenderer)
            {
                ISymbol symbol = (feRenderer as ISimpleRenderer).Symbol;
                string lb = (feRenderer as ISimpleRenderer).Label;
                rulesNamesDic.Add(-1, lb);
            }
            if (feRenderer is IUniqueValueRenderer)
            {
                var ur = feRenderer as IUniqueValueRenderer;
                for (int j = 0; j <= ur.ValueCount - 1; j++)
                {
                    string xv;
                    xv = ur.get_Value(j);
                    ISymbol symbol = ur.get_Symbol(xv);
                    string lb = ur.get_Label(xv);
                    rulesNamesDic.Add(-(1 + j), lb);
                }

            }
            if (feRenderer is IClassBreaksRenderer)
            {
                var cb = feRenderer as IClassBreaksRenderer;
                for (int i = 0; i < cb.BreakCount; i++)
                {

                    ISymbol symbol = cb.get_Symbol(i);
                    string lb = cb.get_Label(i);
                    rulesNamesDic.Add(-(1 + i), lb);
                }

            }
            #endregion

            return rulesNamesDic;

        }

        private Dictionary<int, string> UniqueValueRepRender(IFeatureLayer pFeatureLayer, string pUniqueFieldName)
        {
            var rulesNamesDic = new Dictionary<int, string>();
            var resultDic = new Dictionary<int, string>();
            var rulesDic = new Dictionary<int, IRepresentationRule>();
            try
            {
                IGeoFeatureLayer pGeoLayer = pFeatureLayer as IGeoFeatureLayer;
                if (pGeoLayer == null) return null;
                //获取要素对应的制图表达
                IFeatureRenderer feRenderer = ((IGeoFeatureLayer)pFeatureLayer).Renderer;
                if (!(feRenderer is IRepresentationRenderer))
                {
                    return LoadSimpleRendererLayer(pGeoLayer);

                }
                IRepresentationRenderer representationRenderer = (IRepresentationRenderer)feRenderer;
                IRepresentationClass repClass = representationRenderer.RepresentationClass;
                var rules = repClass.RepresentationRules;
                int ruleid;
                IRepresentationRule rule;
                rules.Reset();
                while (true)
                {

                    rules.Next(out ruleid, out rule);
                    if (rule == null)
                        break;
                    rulesDic[ruleid] = rule;
                }


                ITable pTable = pGeoLayer.FeatureClass as ITable;
                ICursor pCusor;
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = LgEnvelope;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                sf.WhereClause = (pGeoLayer as ESRI.ArcGIS.Carto.IFeatureLayerDefinition).DefinitionExpression;
                sf.AddField(pUniqueFieldName);
                pCusor = pTable.Search(sf, true);//获取字段
                IEnumerator pEnumerator;

                //获取字段中各要素属性唯一值
                IDataStatistics pDataStatistics = new DataStatisticsClass();
                pDataStatistics.Field = pUniqueFieldName;//获取统计字段
                pDataStatistics.Cursor = pCusor;
                pEnumerator = pDataStatistics.UniqueValues;
                while (pEnumerator.MoveNext())
                {
                    string valuet = pEnumerator.Current.ToString();
                    if (valuet == null) { continue; }
                    int value = int.Parse(valuet);
                    try
                    {
                        if (rules.get_Name(value) != "不显示要素")
                        {
                            string rulename = rules.get_Name(value);
                            rulename = System.Text.RegularExpressions.Regex.Replace(rulename, @"\d", "");
                            rulename = rulename.Trim();
                            if (rulename != "")
                            {
                                rulesNamesDic[value] = rulename;
                            }
                            else
                            {
                                rulesNamesDic[value] = rules.get_Name(value);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                //调整图例的顺序
                foreach (var kv in rulesDic)
                {
                    if (rulesNamesDic.ContainsKey(kv.Key))
                    {
                        resultDic[kv.Key] = rulesNamesDic[kv.Key];
                    }

                }


                return resultDic;
            }
            catch
            {
                return rulesNamesDic;
            }
        }
        private List<string> sortLyrs = null;
        bool inti = true;

        /// <summary>
        /// 从标准模板获取当前地图纸张接近的参数
        /// </summary>
        /// <returns></returns>
        private void LoadNearestSize()
        {
            double width = 0;
            double height = 0;

            IGeometry pageGeo = null;
            try
            {
                #region 获取尺寸
                Dictionary<string, string> envString = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
                if (envString == null)
                {
                    envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
                }
                if (envString != null && envString.ContainsKey("Width") && envString.ContainsKey("Height"))
                {
                    width = double.Parse(envString["Width"]); //毫米
                    height = double.Parse(envString["Height"]);//毫米
                }
                else
                {

                    if (GApplication.Application.Workspace != null)
                    {
                        var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l => { return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToLower() == "clipboundary"); })).First();
                        if (lyr != null)
                        {
                            #region
                            IFeatureClass clipfcl = (lyr as IFeatureLayer).FeatureClass;
                            if (clipfcl.FeatureCount(null) != 0)
                            {
                                IFeature fe = null;
                                IQueryFilter qf = new QueryFilterClass();
                                qf.WhereClause = "TYPE = '页面'";
                                IFeatureCursor cursor = clipfcl.Search(qf, false);
                                fe = cursor.NextFeature();
                                pageGeo = fe.ShapeCopy as ESRI.ArcGIS.Geometry.IPolygon;
                                Marshal.ReleaseComObject(cursor);
                                Marshal.ReleaseComObject(qf);
                                Marshal.ReleaseComObject(fe);
                                double ms = GApplication.Application.ActiveView.FocusMap.ReferenceScale;
                                width = pageGeo.Envelope.Width / ms * 1000; //毫米
                                height = pageGeo.Envelope.Height / ms * 1000;//毫米
                            }
                            #endregion
                        }
                    }
                }
                if (width == 0)
                {
                    var paramContent = EnvironmentSettings.getContentElement(GApplication.Application);
                    var pagesize = paramContent.Element("PageSize");//页面大小
                    width = double.Parse(pagesize.Element("Width").Value);
                    height = double.Parse(pagesize.Element("Height").Value);
                }
                #endregion
                double min = Math.Min(width, height);
                double max = Math.Max(width, height);
                string xmlpath = GApplication.Application.Template.Root + @"\整饰\图例专家库\LengendMapSizeRule.xml";
                if (!File.Exists(xmlpath))
                    return;
                XDocument doc;
                doc = XDocument.Load(xmlpath);
                var items = doc.Element("LengendTemplate").Element("LengendInfo").Elements("Lengend");

                string nameSel = string.Empty;
                foreach (var item in items)
                {
                    string type = item.Attribute("Type").Value;
                    cmbLgSize.Items.Add(type);
                    double pagewidth = double.Parse(item.Attribute("Width").Value);
                    double pageheight = double.Parse(item.Attribute("Height").Value);
                    double pagemin = Math.Min(pagewidth, pageheight);
                    double pagemax = Math.Max(pagewidth, pageheight);
                    if (nameSel == string.Empty)
                    {
                        #region 考虑两边
                        if ((Math.Abs(pagemin - min) / min) < 0.1 && Math.Abs(pagemax - max) / max < 0.1)
                        {
                            nameSel = type;
                            continue;
                        }
                        #endregion
                    }
                    if (nameSel == string.Empty)
                    {
                        #region 考虑单边
                        if ((Math.Abs(pagemin - min) / min) < 0.1 || Math.Abs(pagemin - max) / max < 0.1)
                        {
                            nameSel = type;
                            continue;
                        }

                        #endregion
                    }


                }
                if (nameSel != string.Empty)
                {
                    cmbLgSize.SelectedIndex = cmbLgSize.Items.IndexOf(nameSel);
                }
                else
                {
                    cmbLgSize.SelectedIndex = cmbLgSize.Items.Count - 1;
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);


            }

        }

        private void LoadPlans()
        {
            if (inti)
            {
                inti = false;
                return;
            }
            fliterLyrs.Clear();
            Dictionary<string, string> envString = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            lgRulePath = GApplication.Application.Template.Root + "\\底图\\" + envString["BaseMap"] + "\\" + envString["MapTemplate"] + "\\图例\\";
            lgRulePath += cmbTemplate.SelectedItem.ToString() + "\\";
            if (lgModel == LengendModel.MapView)
            {
                sortLyrs = new List<string>() { "RESP", "AGNP", "LRDL", "JJTH", "LRRL", "HYDL", "HYDA", "TERP", "LFCP" };
                if (File.Exists(lgRulePath + "LengendFliter.xml"))
                {
                    var doc = XDocument.Load(lgRulePath + "LengendFliter.xml");
                    var lengends = doc.Element("Lengend").Elements("Item");
                    foreach (var item in lengends)
                    {
                        string fclname = item.Attribute("FeatureClass").Value;
                        List<string> ids = item.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        LengendFliters[fclname] = ids;
                    }
                    if (doc.Element("Lengend").Elements("Layer") != null)
                    {
                        foreach (var lyr in doc.Element("Lengend").Elements("Layer"))
                        {
                            string fclname = lyr.Attribute("FeatureClass").Value;

                            fliterLyrs.Add(fclname);
                        }
                    }
                }
                LoadLengendItemsd();
            }
            if (lgModel == LengendModel.Template)
            {
                string path = lgRulePath + @"LengendRule.xml";
                LoadLengendXml(path);
            }
            if (cmbMapType.Items.Count > 0)
                cmbMapType.SelectedIndex = 0;
        }

        private void FrmLgSet_Load(object sender, EventArgs e)
        {
            //地图模板 
            if (lgModel == LengendModel.Template)
            {
                return;
            }
            string planxml = "";
            if (ExpertiseParamsClass.LoadOutParams(out planxml))
            {
                if (File.Exists(planxml))
                {
                    LoadLengendOutXml(planxml);
                }
            }
            cmbMapType.SelectedIndex = 0;
            string path = lgRulePath + "LengendRule.xml";
            btImport.Visible = System.IO.File.Exists(path);

        }

        public Dictionary<ILayer, Dictionary<int, string>> LgItemsDic = null;
        public List<KeyValuePair<ILayer, Dictionary<int, string>>> LgItemsList = null;
        public List<string> LgLyrDic = null;
        public string DiaState = "OK";
        public int LengendColumn = 2;
        public double repItemXStep = 71.0 / 2.83;//图例项水平间隔
        public double repItemYStep = 10.0 / 2.83;//图例垂直项间隔
        public double TitleFont = 10;
        public double ItemFont = 2.3;
        public double repAnnoStep = 30.0 / 2.83;//图例说明间隔
        public double ScaleBarStep = 11;        //比例尺间隔
        public int LegendType = 0;              //图例类型
        private void SetParms()
        {
            LengendColumn = (int)numColumn.Value;
            double r = 0;
            double.TryParse(HorizontalInterval.Text, out r);
            if (r != 0)
            {
                repItemXStep = r;
            }
            double.TryParse(VerticalInterval.Text, out r);
            if (r != 0)
            {
                repItemYStep = r;
            }
            double.TryParse(TitleFontSize.Text, out r);
            if (r != 0)
            {
                TitleFont = r;
            }
            double.TryParse(ItemFontSize.Text, out r);
            if (r != 0)
            {
                ItemFont = r;
            }
            double.TryParse(txtAnnoStep.Text, out r);
            if (r != 0)
            {
                repAnnoStep = r;
            }
            double.TryParse(TxtscaleBar.Text, out r);
            if (r != 0)
            {
                ScaleBarStep = r;
            }
            LegendType = cmbTemplate.SelectedIndex;
        }

        public void OrginizeParms()
        {
            LgItemsDic = new Dictionary<ILayer, Dictionary<int, string>>();
            LgLyrDic = new List<string>();
            LengendColumn = (int)numColumn.Value;
            annolist = new List<AnnoItemInfo>();
            int temp = 0;
            cmd.LyrTreeNodes.Clear();
            for (int i = 0; i < tvItems.Nodes.Count; i++)
            {
                TreeNode node = tvItems.Nodes[i];
                #region
                if (node.Checked)
                {

                    if (node.Text == "注记图例")
                    {
                        annoFlag = temp;
                        foreach (TreeNode item in node.Nodes)
                        {
                            if (item.Checked)
                            {
                                AnnoItemInfo info = (AnnoItemInfo)item.Tag;
                                annolist.Add(info);
                            }
                        }
                    }
                    else
                    {
                        //  var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == node.Text)).FirstOrDefault();
                        var lyrs = node.Tag as IFeatureLayer;
                        if (lyrs == null)
                        {
                            lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == node.Text)).FirstOrDefault() as IFeatureLayer;
                            if (lyrs == null)
                                continue;
                        }
                        var list = new Dictionary<int, string>();
                        foreach (TreeNode item in node.Nodes)
                        {
                            if (item.Checked)
                            {
                                int ruleID = (int)item.Tag;
                                list[ruleID] = item.Text;

                            }
                        }
                        LgLyrDic.Add(lyrs.Name);
                        LgItemsDic.Add(lyrs, list);
                    }
                    temp++;
                }
                #endregion
                cmd.LyrTreeNodes.Add(node.Clone() as TreeNode);
            }

            SetParms();
            LgItemsList = LgItemsDic.ToList();
        }
        public void DoExcute()
        {

            LengendHelper lg = new LengendHelper(LgEnvelope);
            lg.lgSizeType = LgOutLine;
            lg.GroupLengend = cbGroupLg.Checked;
            var lgLyr = this.LgItemsList;
            lg.LgLocation = cbOutMap.Checked ? "OUT" : "IN";
            lg.LGTemplate = cmbTemplate.SelectedItem.ToString();
            lg.repItemYStep = this.repItemYStep * 2.83;
            lg.repItemXStep = this.repItemXStep * 2.83;
            lg.ItemFontSize = this.ItemFont * 2.83;
            lg.TitleFontSize = this.TitleFont * 2.83;
            lg.repAnnoStep = this.repAnnoStep * 2.83;
            lg.MapLgType = cmbMapType.SelectedItem.ToString();
            lg.lgItemLen = double.Parse(txtLgEleLen.Text) * 2.83;
            lg.lgItemLenA = double.Parse(txtLgEleALen.Text) * 2.83;
            lg.lgItemWidthA = double.Parse(txtLgEleAWid.Text) * 2.83;



            lg.ScaleBarStep = this.ScaleBarStep;
            lg.location = this.LGLocation;
            var anchorPoint = LgEnvelope.UpperLeft;
            switch (lg.location)
            {
                case "左上":
                    anchorPoint = cbOutMap.Checked ? LgEnvelopeOut.UpperLeft : LgEnvelope.UpperLeft;
                    break;
                case "右上":
                    anchorPoint = cbOutMap.Checked ? LgEnvelopeOut.UpperRight : LgEnvelope.UpperRight;
                    break;
                case "左下":
                    anchorPoint = cbOutMap.Checked ? LgEnvelopeOut.LowerLeft : LgEnvelope.LowerLeft;
                    break;
                case "右下":
                    anchorPoint = cbOutMap.Checked ? LgEnvelopeOut.LowerRight : LgEnvelope.LowerRight;
                    break;
                default:
                    break;
            }

            int annoFlag = this.annoFlag;
            var itemInfos = this.annolist;
            //预览
            WaitOperation wo = GApplication.Application.SetBusy();
            app.EngineEditor.StartOperation();
            try
            {
                wo.SetText("正在生成图例....");

                if (lgModel == LengendModel.MapView)
                {
                    lg.CreateLengendMapView(annoFlag, itemInfos, lgLyr, anchorPoint, this.LengendColumn);
                }
                if (lgModel == LengendModel.Template)
                {
                    lg.CreateLengendTemplate(annoFlag, itemInfos, lgLyr, anchorPoint, this.LengendColumn);
                }
                wo.Dispose();
                app.EngineEditor.StopOperation("生成图例");
            }
            catch
            {
                wo.Dispose();
                app.EngineEditor.AbortOperation();
            }
        }
        private void btOk_Click(object sender, EventArgs e)
        {
            #region 输入验证
            ValidateUtil valid = new ValidateUtil();
            if (!valid.TraversalTextBox(this.Controls))
            {
                return;
            }
            #endregion
            LgItemsDic = new Dictionary<ILayer, Dictionary<int, string>>();
            LgLyrDic = new List<string>();
            LengendColumn = (int)numColumn.Value;
            annolist = new List<AnnoItemInfo>();
            int temp = 0;
            cmd.LyrTreeNodes.Clear();
            for (int i = 0; i < tvItems.Nodes.Count; i++)
            {
                TreeNode node = tvItems.Nodes[i];
                #region
                if (node.Checked)
                {

                    if (node.Text == "注记图例")
                    {
                        annoFlag = temp;
                        foreach (TreeNode item in node.Nodes)
                        {
                            if (item.Checked)
                            {
                                AnnoItemInfo info = (AnnoItemInfo)item.Tag;
                                annolist.Add(info);
                            }
                        }
                    }
                    else
                    {
                        // var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && (l.Name == node.Text)).FirstOrDefault();
                        var lyrs = node.Tag as IFeatureLayer;
                        if (lyrs == null)
                        {
                            lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == node.Text)).FirstOrDefault() as IFeatureLayer;
                            if (lyrs == null)
                                continue;
                        }
                        var list = new Dictionary<int, string>();
                        foreach (TreeNode item in node.Nodes)
                        {
                            if (item.Checked)
                            {
                                int ruleID = (int)item.Tag;
                                list[ruleID] = item.Text;

                            }
                        }
                        LgLyrDic.Add(lyrs.Name);
                        LgItemsDic.Add(lyrs, list);
                    }
                    temp++;
                }
                #endregion
                cmd.LyrTreeNodes.Add(node.Clone() as TreeNode);
            }

            SetParms();
            LgItemsList = LgItemsDic.ToList();
            DoExcute();
            SaveLGPramas();
            this.Close();
            //DialogResult = DialogResult.OK;
        }

        private void showStatus(object s, System.Timers.ElapsedEventArgs e)
        {
            GApplication.Application.MainForm.ShowStatus("图例生成成功");
        }

        //注记参数
        public int annoFlag = 0;
        public List<AnnoItemInfo> annolist = null;
        public void ViewLengend(WaitOperation wo = null)
        {
            LgItemsDic = new Dictionary<ILayer, Dictionary<int, string>>();
            LgLyrDic = new List<string>();
            LengendColumn = (int)numColumn.Value;
            annolist = new List<AnnoItemInfo>();
            int temp = 0;
            cmd.LyrTreeNodes.Clear();
            for (int i = 0; i < tvItems.Nodes.Count; i++)
            {
                TreeNode node = tvItems.Nodes[i];
                #region
                if (node.Checked)
                {

                    if (node.Text == "注记图例")
                    {
                        annoFlag = temp;
                        foreach (TreeNode item in node.Nodes)
                        {
                            if (item.Checked)
                            {
                                AnnoItemInfo info = (AnnoItemInfo)item.Tag;
                                annolist.Add(info);
                            }
                        }
                    }
                    else
                    {
                        // var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == node.Text)).FirstOrDefault();
                        var lyrs = node.Tag as IFeatureLayer;
                        if (lyrs == null)
                        {
                            lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == node.Text)).FirstOrDefault() as IFeatureLayer;
                            if (lyrs == null)
                                continue;
                        }

                        var list = new Dictionary<int, string>();
                        foreach (TreeNode item in node.Nodes)
                        {
                            if (item.Checked)
                            {
                                int ruleID = (int)item.Tag;
                                list[ruleID] = item.Text;

                            }
                        }
                        LgLyrDic.Add(lyrs.Name);
                        LgItemsDic.Add(lyrs, list);
                    }
                    temp++;
                }
                #endregion
                cmd.LyrTreeNodes.Add(node.Clone() as TreeNode);
            }

            SetParms();
            LgItemsList = LgItemsDic.ToList();
            //预览

            try
            {
                if (app.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                    app.EngineEditor.StartOperation();
                if (wo != null)
                {
                    wo.SetText("正在生成图例....");
                }
                CreateOverView(annoFlag, annolist);
                if (wo != null)
                {
                    wo.Dispose();
                }
                if (app.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                    app.EngineEditor.StopOperation("生成图例");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                if (wo != null)
                {
                    MessageBox.Show(ex.ToString());
                    wo.Dispose();
                }
                if (app.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing)
                    app.EngineEditor.AbortOperation();
            }
            SaveLGPramas();
        }
        private void btView_Click(object sender, EventArgs e)
        {


            ViewLengend();

            //MessageBox.Show("预览生成！");
        }
        private void CreateOverView(int annoIndex, List<AnnoItemInfo> itemInfos)
        {
            var repLyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LLINE"))).FirstOrDefault();
            var linefcl = (repLyr as IFeatureLayer).FeatureClass;
            IQueryFilter qf = new QueryFilterClass();
            (linefcl as IFeatureClassLoad).LoadOnlyMode = false;
            qf.WhereClause = "TYPE like '%图廓%'";
            IGeometry clipGeoIn = null;
            if (linefcl.FeatureCount(qf) > 0)
            {
                IFeature fe = null;
                IFeatureCursor cursor = linefcl.Search(qf, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    string type = fe.get_Value(linefcl.FindField("TYPE")).ToString();
                    if (type == "内图廓")
                    {
                        clipGeoIn = fe.ShapeCopy.Envelope;
                    }

                }
                Marshal.ReleaseComObject(cursor);
            }
            if (clipGeoIn == null)
            {
                MessageBox.Show("不存在内图廓，请先生成");
                return;
            }



            LengendHelper lg = new LengendHelper(LgEnvelope);
            lg.lgSizeType = LgOutLine;
            lg.GroupLengend = cbGroupLg.Checked;
            var lgLyr = this.LgItemsList;
            lg.MapLgType = cmbMapType.SelectedItem.ToString();
            lg.LgLocation = cbOutMap.Checked ? "OUT" : "IN";
            lg.LGTemplate = cmbTemplate.SelectedItem.ToString();
            lg.ScaleBarStep = this.ScaleBarStep;
            lg.repItemYStep = this.repItemYStep * 2.83;
            lg.repItemXStep = this.repItemXStep * 2.83;
            lg.ItemFontSize = this.ItemFont * 2.83;
            lg.TitleFontSize = this.TitleFont * 2.83;
            lg.repAnnoStep = this.repAnnoStep * 2.83;

            lg.lgItemLen = double.Parse(txtLgEleLen.Text) * 2.83;
            lg.lgItemLenA = double.Parse(txtLgEleALen.Text) * 2.83;
            lg.lgItemWidthA = double.Parse(txtLgEleAWid.Text) * 2.83;

            lg.location = LGLocation;
            var anchorPoint = clipGeoIn.Envelope.UpperLeft;
            switch (lg.location)
            {
                case "左上":
                    anchorPoint = cbOutMap.Checked ? LgEnvelopeOut.UpperLeft : clipGeoIn.Envelope.UpperLeft;
                    break;
                case "右上":
                    anchorPoint = cbOutMap.Checked ? LgEnvelopeOut.UpperRight : clipGeoIn.Envelope.UpperRight;
                    break;
                case "左下":
                    anchorPoint = cbOutMap.Checked ? LgEnvelopeOut.LowerLeft : clipGeoIn.Envelope.LowerLeft;
                    break;
                case "右下":
                    anchorPoint = cbOutMap.Checked ? LgEnvelopeOut.LowerRight : clipGeoIn.Envelope.LowerRight;
                    break;
                default:
                    break;
            }
            if (lgModel == LengendModel.MapView)
            {
                lg.CreateLengendMapView(annoFlag, itemInfos, lgLyr, anchorPoint, this.LengendColumn);
            }
            if (lgModel == LengendModel.Template)
            {
                lg.CreateLengendTemplate(annoFlag, itemInfos, lgLyr, anchorPoint, this.LengendColumn);
            }
            GApplication.Application.ActiveView.Refresh();
            Application.DoEvents();
            this.Refresh();
        }

        private int CompareLyr(KeyValuePair<ILayer, Dictionary<int, string>> item1, KeyValuePair<ILayer, Dictionary<int, string>> item2)
        {
            string lyr1 = (item1.Key as IFeatureLayer).FeatureClass.AliasName.ToUpper();
            string lyr2 = (item2.Key as IFeatureLayer).FeatureClass.AliasName.ToUpper();
            if (sortLyrs.Contains(lyr1) && sortLyrs.Contains(lyr2))
            {
                if (sortLyrs.IndexOf(lyr1) < sortLyrs.IndexOf(lyr2))
                    return 1;
                if (sortLyrs.IndexOf(lyr1) > sortLyrs.IndexOf(lyr2))
                    return -1;
                if (sortLyrs.IndexOf(lyr1) == sortLyrs.IndexOf(lyr2))
                    return 0;
            }
            else if (!sortLyrs.Contains(lyr1) && sortLyrs.Contains(lyr2))
            {
                return -1;
            }
            else if (sortLyrs.Contains(lyr1) && !sortLyrs.Contains(lyr2))
            {
                return 1;
            }
            else if (!sortLyrs.Contains(lyr1) && !sortLyrs.Contains(lyr2))
            {

                var index1 = LgLyrDic.IndexOf(lyr1);
                var index2 = LgLyrDic.IndexOf(lyr2);
                if (index1 < index2)
                    return 1;
                if (index1 > index2)
                    return -1;
                if (index1 == index2)
                    return 0;

            }
            return 0;

        }

        private void ExportLGSet_Click(object sender, EventArgs e)
        {
            FrmXmlSave frm = new FrmXmlSave();
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            try
            {
                ExportXML(frm.XmlPath);
                MessageBox.Show("导出成功！");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void ExportXML(string fileName)
        {

            FileInfo f = new FileInfo(fileName);
            XDocument doc = new XDocument();
            doc.Declaration = new XDeclaration("1.0", "utf-8", "");
            var root = new XElement("LengendTemplate");
            doc.Add(root);
            //图例配置信息
            var lengendInfo = new XElement("LengendInfo");
            root.Add(lengendInfo);
            {
                SetParms();
                //图例列数
                var itemInfo = new XElement("Column");
                itemInfo.SetValue(LengendColumn);
                itemInfo.SetAttributeValue("Name", "图例列数");
                lengendInfo.Add(itemInfo);
                //图例文字水平间隔
                itemInfo = new XElement("CharactersXStep");
                itemInfo.SetValue(repAnnoStep);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例文字水平间隔");
                lengendInfo.Add(itemInfo);
                //图例标题字体
                itemInfo = new XElement("TitleFontSize");
                itemInfo.SetValue(TitleFont);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例标题字体");
                lengendInfo.Add(itemInfo);
                //图例项字体
                itemInfo = new XElement("ItemFontSize");
                itemInfo.SetValue(ItemFont);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例项字体");
                lengendInfo.Add(itemInfo);
                //图例项垂直间隔
                itemInfo = new XElement("ItemYStep");
                itemInfo.SetValue(repItemYStep);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例项垂直间隔");
                lengendInfo.Add(itemInfo);
                //图例项水平间隔
                itemInfo = new XElement("ItemXStep");
                itemInfo.SetValue(repItemXStep);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例项水平间隔");
                lengendInfo.Add(itemInfo);
                //比例尺间隔
                itemInfo = new XElement("ScaleBarStep");
                itemInfo.SetValue(ScaleBarStep);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "比例尺间隔");
                lengendInfo.Add(itemInfo);
                //图例类型
                itemInfo = new XElement("LegendType");
                itemInfo.SetValue(LegendType);
                itemInfo.SetAttributeValue("Name", "图例类型");
                lengendInfo.Add(itemInfo);
            }
            //图例项信息
            var content = new XElement("Lengend");
            root.Add(content);

            foreach (TreeNode node in tvItems.Nodes)
            {
                // if (node.Checked)
                {
                    if (node.Text == "注记图例")
                    {
                        var lyr = new XElement("Layer");
                        lyr.SetAttributeValue("LayerName", node.Text);
                        lyr.SetAttributeValue("Checked", node.Checked);
                        foreach (TreeNode item in node.Nodes)
                        {
                            AnnoItemInfo annoInfo = (AnnoItemInfo)item.Tag;
                            var dataRowInfo = new XElement("DataRow");
                            dataRowInfo.SetAttributeValue("unit", "毫米");
                            dataRowInfo.SetAttributeValue("display", item.Checked);
                            lyr.Add(dataRowInfo);
                            //添加属性
                            var itemInfo = new XElement("AnnoName") { Value = annoInfo.AnnoName };
                            dataRowInfo.Add(itemInfo);
                            itemInfo = new XElement("AnnoFontSize") { Value = annoInfo.FontSize.ToString() };
                            dataRowInfo.Add(itemInfo);
                            itemInfo = new XElement("AnnoFontName") { Value = annoInfo.FontName };
                            dataRowInfo.Add(itemInfo);
                            itemInfo = new XElement("AnnoNote") { Value = annoInfo.AnnoNote };
                            dataRowInfo.Add(itemInfo);
                            itemInfo = new XElement("AnnoColor") { Value = AnnoGeometryHelper.GetStringByColor(annoInfo.AnnoColor) };
                            dataRowInfo.Add(itemInfo);


                        }
                        content.Add(lyr);
                    }
                    else
                    {
                        var lyr = new XElement("Layer");
                        lyr.SetAttributeValue("LayerName", node.Text);
                        lyr.SetAttributeValue("Checked", node.Checked);
                        foreach (TreeNode item in node.Nodes)
                        {
                            // if (item.Checked)
                            {
                                int ruleID = (int)item.Tag;
                                var itemInfo = new XElement("ItemInfo");
                                itemInfo.SetAttributeValue("RuleID", ruleID);
                                itemInfo.SetAttributeValue("Checked", item.Checked);
                                itemInfo.SetValue(item.Text);
                                lyr.Add(itemInfo);
                            }
                        }
                        content.Add(lyr);
                    }

                }

            }

            doc.Save(fileName);
        }

        private void SaveLGPramas()
        {


            var root = new XElement("LengendTemplate");

            //图例配置信息
            var lengendInfo = new XElement("LengendInfo");
            root.Add(lengendInfo);
            {

                #region
                SetParms();
                //图例列数
                var itemInfo = new XElement("Column");
                itemInfo.SetValue(LengendColumn);
                itemInfo.SetAttributeValue("Name", "图例列数");
                lengendInfo.Add(itemInfo);
                //图例文字水平间隔
                itemInfo = new XElement("CharactersXStep");
                itemInfo.SetValue(repAnnoStep);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例文字水平间隔");
                lengendInfo.Add(itemInfo);
                //图例标题字体
                itemInfo = new XElement("TitleFontSize");
                itemInfo.SetValue(TitleFont);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例标题字体");
                lengendInfo.Add(itemInfo);
                //图例项字体
                itemInfo = new XElement("ItemFontSize");
                itemInfo.SetValue(ItemFont);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例项字体");
                lengendInfo.Add(itemInfo);
                //图例项垂直间隔
                itemInfo = new XElement("ItemYStep");
                itemInfo.SetValue(repItemYStep);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例项垂直间隔");
                lengendInfo.Add(itemInfo);
                //图例项水平间隔
                itemInfo = new XElement("ItemXStep");
                itemInfo.SetValue(repItemXStep);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "图例项水平间隔");
                lengendInfo.Add(itemInfo);
                //比例尺间隔
                itemInfo = new XElement("ScaleBarStep");
                itemInfo.SetValue(ScaleBarStep);
                itemInfo.SetAttributeValue("Unit", "毫米");
                itemInfo.SetAttributeValue("Name", "比例尺间隔");
                lengendInfo.Add(itemInfo);
                //图例类型
                itemInfo = new XElement("LegendType");
                itemInfo.SetValue(LegendType);
                itemInfo.SetAttributeValue("Name", "图例类型");
                lengendInfo.Add(itemInfo);
                #endregion
            }
            //图例项信息
            var content = new XElement("Lengend");
            root.Add(content);

            foreach (TreeNode node in tvItems.Nodes)
            {
                // if (node.Checked)
                {
                    if (node.Text == "注记图例")
                    {
                        #region
                        var lyr = new XElement("Layer");
                        lyr.SetAttributeValue("LayerName", node.Text);
                        lyr.SetAttributeValue("Checked", node.Checked);
                        foreach (TreeNode item in node.Nodes)
                        {
                            AnnoItemInfo annoInfo = (AnnoItemInfo)item.Tag;
                            var dataRowInfo = new XElement("DataRow");
                            dataRowInfo.SetAttributeValue("unit", "毫米");
                            dataRowInfo.SetAttributeValue("display", item.Checked);
                            lyr.Add(dataRowInfo);
                            //添加属性
                            var itemInfo = new XElement("AnnoName") { Value = annoInfo.AnnoName };
                            dataRowInfo.Add(itemInfo);
                            itemInfo = new XElement("AnnoFontSize") { Value = annoInfo.FontSize.ToString() };
                            dataRowInfo.Add(itemInfo);
                            itemInfo = new XElement("AnnoFontName") { Value = annoInfo.FontName };
                            dataRowInfo.Add(itemInfo);
                            itemInfo = new XElement("AnnoNote") { Value = annoInfo.AnnoNote };
                            dataRowInfo.Add(itemInfo);
                            itemInfo = new XElement("AnnoColor") { Value = AnnoGeometryHelper.GetStringByColor(annoInfo.AnnoColor) };
                            dataRowInfo.Add(itemInfo);


                        }
                        #endregion
                        content.Add(lyr);
                    }
                    else
                    {
                        #region
                        var lyr = new XElement("Layer");
                        lyr.SetAttributeValue("LayerName", node.Text);
                        lyr.SetAttributeValue("Checked", node.Checked);
                        foreach (TreeNode item in node.Nodes)
                        {
                            // if (item.Checked)
                            {
                                int ruleID = (int)item.Tag;
                                var itemInfo = new XElement("ItemInfo");
                                itemInfo.SetAttributeValue("RuleID", ruleID);
                                itemInfo.SetAttributeValue("Checked", item.Checked);
                                itemInfo.SetValue(item.Text);
                                lyr.Add(itemInfo);
                            }
                        }
                        #endregion
                        content.Add(lyr);
                    }

                }

            }

            ExpertiseParamsClass.UpdateMapLengend(GApplication.Application, root);

        }

        private void up_Click(object sender, EventArgs e)
        {
            TreeNode nodeSel = tvItems.SelectedNode;
            if (nodeSel == null)
            {
                MessageBox.Show("请选择需要移动的节点");
                return;
            }
            if (nodeSel.PrevNode == null)
            {
                tvItems.Focus();
                return;
            }
            int index = nodeSel.Index;
            TreeNode parent = nodeSel.Parent;
            TreeNode clone = (TreeNode)nodeSel.Clone();
            if (parent == null)
            {
                tvItems.Nodes.Insert(index - 1, clone);
            }
            else
            {
                parent.Nodes.Insert(index - 1, clone);

            }
            nodeSel.Remove();
            tvItems.SelectedNode = clone;
            tvItems.Focus();

        }

        private void down_Click(object sender, EventArgs e)
        {
            TreeNode nodeSel = tvItems.SelectedNode;
            if (nodeSel == null)
            {
                MessageBox.Show("请选择需要移动的节点");
                return;
            }
            if (nodeSel.NextNode == null)
            {
                tvItems.Focus();
                return;
            }
            int index = nodeSel.Index;
            TreeNode parent = nodeSel.Parent;
            TreeNode clone = (TreeNode)nodeSel.Clone();
            if (parent == null)
            {
                tvItems.Nodes.Insert(index + 2, clone);
            }
            else
            {
                parent.Nodes.Insert(index + 2, clone);

            }
            nodeSel.Remove();
            tvItems.SelectedNode = clone;
            tvItems.Focus();
        }

        private void top_Click(object sender, EventArgs e)
        {
            TreeNode nodeSel = tvItems.SelectedNode;
            if (nodeSel == null)
            {
                MessageBox.Show("请选择需要移动的节点");
                return;
            }
            int index = nodeSel.Index;
            TreeNode parent = nodeSel.Parent;
            TreeNode clone = (TreeNode)nodeSel.Clone();
            if (parent == null)
            {
                tvItems.Nodes.Insert(0, clone);
            }
            else
            {
                parent.Nodes.Insert(0, clone);

            }
            nodeSel.Remove();
            tvItems.SelectedNode = clone;
            tvItems.Focus();
        }

        private void bottom_Click(object sender, EventArgs e)
        {
            TreeNode nodeSel = tvItems.SelectedNode;
            if (nodeSel == null)
            {
                MessageBox.Show("请选择需要移动的节点");
                return;
            }
            int index = nodeSel.Index;
            TreeNode parent = nodeSel.Parent;
            TreeNode clone = (TreeNode)nodeSel.Clone();
            if (parent == null)
            {
                tvItems.Nodes.Insert(tvItems.Nodes.Count, clone);
            }
            else
            {
                parent.Nodes.Insert(parent.Nodes.Count, clone);

            }
            nodeSel.Remove();
            tvItems.SelectedNode = clone;
            tvItems.Focus();
        }
        public string LGLocation = "右下";
        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (sender as RadioButton);
            if (rb.Checked)
            {
                LGLocation = rb.Text;
            }
        }
        //帮助
        private void FrmLgSet_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            string path = GApplication.Application.Template.Root + @"\整饰\帮助\";
            path += "动态图例功能说明.htm";
            System.Diagnostics.Process.Start(path);
        }

        public void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btLast_Click(object sender, EventArgs e)
        {
            string planStr = @"专家库\经验方案\经验方案.xml";
            string fileName = app.Template.Root + @"\" + planStr;
            LoadLengendOutXml(fileName);
        }
        //导入配置
        private void btImport_Click(object sender, EventArgs e)
        {
            FrmLengendSet frm = new FrmLengendSet();
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;

            string path = frm.LgRuleXML;
            if (System.IO.File.Exists(path))
            {
                lgModel = LengendModel.Template;
                tvItems.Nodes.Clear();
                LoadLengendXml(path);

            }


        }

        private void txtLgEleLen_Enter(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                double.TryParse(tb.Text, out OldLineLen);

            }
        }

        private void txtLgEleLen_Leave(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                double lineLen = 0;
                double.TryParse(tb.Text, out lineLen);
                double delta = lineLen - OldLineLen;

                double annoStep = 0;
                double.TryParse(txtAnnoStep.Text, out annoStep);
                annoStep += delta;
                txtAnnoStep.Text = annoStep.ToString();

                double hInterval = 0;
                double.TryParse(HorizontalInterval.Text, out hInterval);
                hInterval += delta;
                HorizontalInterval.Text = hInterval.ToString();

            }
        }

        private void cmbTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPlans();
        }
        bool LgOutLine = true;
        private void cmbLgSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            string xmlpath = GApplication.Application.Template.Root + @"\整饰\图例专家库\LengendMapSizeRule.xml";
            if (!File.Exists(xmlpath))
                return;
            try
            {
                XDocument doc;
                doc = XDocument.Load(xmlpath);
                var items = doc.Element("LengendTemplate").Element("LengendInfo").Elements("Lengend");
                string nameSel = string.Empty;
                foreach (var item in items)
                {
                    string type = item.Attribute("Type").Value;
                    if (type == cmbLgSize.SelectedItem.ToString())
                    {
                        //加载基本信息
                        var lengendInfo = item;
                        var itemInfo = lengendInfo.Element("Column");
                        numColumn.Value = int.Parse(itemInfo.Value);
                        //图例标题字体
                        itemInfo = lengendInfo.Element("TitleFontSize");
                        TitleFontSize.Text = itemInfo.Value;
                        //文字水平间隔
                        itemInfo = lengendInfo.Element("CharactersXStep");
                        txtAnnoStep.Text = itemInfo.Value;
                        //图例项字体
                        itemInfo = lengendInfo.Element("ItemFontSize");
                        ItemFontSize.Text = itemInfo.Value;
                        //图例项垂直间隔
                        itemInfo = lengendInfo.Element("ItemYStep");
                        VerticalInterval.Text = itemInfo.Value;
                        //图例项水平间隔
                        itemInfo = lengendInfo.Element("ItemXStep");
                        HorizontalInterval.Text = itemInfo.Value;
                        //比例尺间隔
                        itemInfo = lengendInfo.Element("ScaleBarStep");
                        if (itemInfo != null)
                        {
                            TxtscaleBar.Text = itemInfo.Value;
                        }
                        //图例类型
                        itemInfo = lengendInfo.Element("LegendType");
                        if (itemInfo != null)
                        {
                            cmbTemplate.SelectedIndex = Convert.ToInt32(itemInfo.Value);
                        }
                        //图例尺寸
                        //线长度
                        if (lengendInfo.Element("PlLegendLength") != null)
                        {
                            itemInfo = lengendInfo.Element("PlLegendLength");
                            this.txtLgEleLen.Text = itemInfo.Value;

                        }
                        //面长度
                        if (lengendInfo.Element("PoLegendLength") != null)
                        {
                            itemInfo = lengendInfo.Element("PoLegendLength");
                            this.txtLgEleALen.Text = itemInfo.Value;
                        }
                        //面宽度
                        if (lengendInfo.Element("PoLegendWidth") != null)
                        {
                            itemInfo = lengendInfo.Element("PoLegendWidth");
                            this.txtLgEleAWid.Text = itemInfo.Value;
                        }
                        //图例边线
                        if (lengendInfo.Element("MapOutLine") != null)
                        {
                            itemInfo = lengendInfo.Element("MapOutLine");
                            LgOutLine = bool.Parse(itemInfo.Value);
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }

        }
        #region 节点拖动
        private void tvItems_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode tn = e.Item as TreeNode;
            if ((e.Button == MouseButtons.Left) && (tn != null))
            {
                this.tvItems.DoDragDrop(tn, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
            }
        }

        private void tvItems_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void tvItems_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            System.Drawing.Point pt = ((TreeView)sender).PointToClient(new System.Drawing.Point(e.X, e.Y));

            //目标
            TreeNode DestinationNode = ((TreeView)sender).GetNodeAt(pt);
            //原始
            TreeNode OrginNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
            if (OrginNode == null)
                return;
            if (DestinationNode == null)
            {

                return;
            }


            if (OrginNode.Nodes.Count == 0)//子节点
            {
                #region
                if (DestinationNode.Parent == null)
                {
                    if (DestinationNode.Text != OrginNode.Parent.Text)
                    {
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.Move;
                        return;
                    }
                }
                else//子节点之间
                {
                    if (DestinationNode.Parent.Text != OrginNode.Parent.Text)
                    {
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.Move;
                        return;
                    }
                }
                #endregion

            }
            else
            {
                if (DestinationNode.Parent != null)
                    return;
                e.Effect = DragDropEffects.Move;
            }

        }

        private void tvItems_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode OrginNode;
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                System.Drawing.Point pt = ((TreeView)sender).PointToClient(new System.Drawing.Point(e.X, e.Y));

                //目标
                TreeNode DestinationNode = ((TreeView)sender).GetNodeAt(pt);
                //原始数据
                OrginNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                if (DestinationNode == null || OrginNode == null)
                    return;
                if (e.Effect == DragDropEffects.None)
                    return;



                if (OrginNode.Nodes.Count == 0)//子节点的移动
                {
                    #region
                    if (DestinationNode.Parent == null)
                    {
                        //移动到第一个
                        DestinationNode.Nodes.Insert(0, (TreeNode)OrginNode.Clone());
                    }
                    else//子节点之间
                    {

                        //目标节点的下一个
                        int index = DestinationNode.Index + 1;
                        Console.WriteLine(DestinationNode + ":index:" + index);
                        DestinationNode.Parent.Nodes.Insert(index, (TreeNode)OrginNode.Clone());

                    }
                    #endregion

                }
                else //父节点的移动
                {
                    if (DestinationNode.Text == OrginNode.Text)
                        return;
                    int index = DestinationNode.Index;
                    tvItems.Nodes.Insert(index, (TreeNode)OrginNode.Clone());

                }
                //treeView1.CollapseAll();
                //删除已经移动的节点
                OrginNode.Remove();

            }


        }
        #endregion
    }
}

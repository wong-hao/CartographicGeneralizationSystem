using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.Xml.Linq;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class NormalQJForm : Form
    {
        /// <summary>
        /// 是否在主区生成常规骑界
        /// </summary>
        public bool BMainRegionQJ
        {
            get
            {
                return cbMainQJ.Checked;
            }
        }

        /// <summary>
        /// 主区骑界生成范围SQL条件
        /// </summary>
        public string MainRegionQJSql
        {
            get;
            internal set;
        }

        /// <summary>
        /// 主区境界线GB->宽度
        /// </summary>
        public Dictionary<int, double> MainRegionGB2QJWidth
        {
            get
            {
                return _mainRegionGB2QJWidth;
            }
        }
        private Dictionary<int, double> _mainRegionGB2QJWidth;

        /// <summary>
        /// 主区骑界颜色
        /// </summary>
        public ICmykColor MainRegionQJColor
        {
            set;
            get;
        }

        /// <summary>
        /// 是否在邻区生成常规骑界
        /// </summary>
        public bool BAdjRegionQJ
        {
            get
            {
                return cbAdjQJ.Checked;
            }
        }


        /// <summary>
        /// 邻区骑界生成范围SQL条件
        /// </summary>
        public string AdjRegionQJSql
        {
            get;
            internal set;
        }

        /// <summary>
        /// 邻区境界线GB->宽度
        /// </summary>
        public Dictionary<int, double> AdjRegionGB2QJWidth
        {
            get
            {
                return _adjRegionGB2QJWidth;
            }
        }
        private Dictionary<int, double> _adjRegionGB2QJWidth;

        /// <summary>
        /// 邻区骑界颜色
        /// </summary>
        public ICmykColor AdjRegionQJColor
        {
            set;
            get;
        }

        private GApplication _app;
        private System.Xml.Linq.XDocument _ruleDoc;
        public NormalQJForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
            _ruleDoc = null;
            _mainRegionGB2QJWidth = new Dictionary<int, double>();
            _adjRegionGB2QJWidth = new Dictionary<int, double>();
        }

        private void NormalQJForm_Load(object sender, EventArgs e)
        {
            try
            {
                string fileName = _app.Template.Root + @"\专家库\骑界\常规骑界.xml";
                _ruleDoc = XDocument.Load(fileName);

                var contentItem = _ruleDoc.Element("Template").Element("Content");
                if (contentItem != null)
                {
                    #region 初始化骑界生成范围相关控件的值
                    var boulItem = contentItem.Element("BOUL");
                    if (boulItem != null)
                    {

                        Dictionary<int, string> gb2Name = new Dictionary<int, string>();

                        cbMainBOULSQL.ValueMember = "Key";
                        cbMainBOULSQL.DisplayMember = "Value";

                        cbAdjBOULSQL.ValueMember = "Key";
                        cbAdjBOULSQL.DisplayMember = "Value";

                        var items = boulItem.Elements("Item");
                        foreach (XElement ele in items)
                        {
                            int gb = 0;
                            int.TryParse(ele.Attribute("gb").Value.ToString(), out gb);
                            double width = 0;
                            double.TryParse(ele.Attribute("qjWidth").Value.ToString(), out width);

                            if (!_mainRegionGB2QJWidth.ContainsKey(gb))
                                _mainRegionGB2QJWidth.Add(gb, width);

                            if (!_adjRegionGB2QJWidth.ContainsKey(gb))
                                _adjRegionGB2QJWidth.Add(gb, width);

                            if (!gb2Name.ContainsKey(gb))
                                gb2Name.Add(gb, ele.Value);
                        }

                        Dictionary<string, string> sql2Name = new Dictionary<string, string>();
                        foreach (var kv in gb2Name)
                        {
                            string sql = "";
                            foreach (var gb in gb2Name.Keys)
                            {
                                if (gb > kv.Key)
                                    continue;

                                if (sql == "")
                                {
                                    sql = string.Format("gb in ({0}", gb);
                                }
                                else
                                {
                                    sql += string.Format(",{0}", gb);
                                }
                            }
                            sql += ")";

                            cbMainBOULSQL.Items.Add(new KeyValuePair<string, string>(sql, kv.Value));
                            cbAdjBOULSQL.Items.Add(new KeyValuePair<string, string>(sql, kv.Value));
                        }
                    }
                    #endregion

                    #region 初始化各级境界骑界宽度
                    if (_mainRegionGB2QJWidth.ContainsKey(630201))
                    {
                        tbMainProvinceQJWidth.Text = _mainRegionGB2QJWidth[630201].ToString();
                    }
                    if (_mainRegionGB2QJWidth.ContainsKey(640201))
                    {
                        tbMainStateQJWidth.Text = _mainRegionGB2QJWidth[640201].ToString();
                    }
                    if (_mainRegionGB2QJWidth.ContainsKey(650201))
                    {
                        tbMainCountyQJWidth.Text = _mainRegionGB2QJWidth[650201].ToString();
                    }
                    if (_mainRegionGB2QJWidth.ContainsKey(660201))
                    {
                        tbMainTownQJWidth.Text = _mainRegionGB2QJWidth[660201].ToString();
                    }

                    if (_adjRegionGB2QJWidth.ContainsKey(630201))
                    {
                        tbAdjProvinceQJWidth.Text = _adjRegionGB2QJWidth[630201].ToString();
                    }
                    if (_adjRegionGB2QJWidth.ContainsKey(640201))
                    {
                        tbAdjStateQJWidth.Text = _adjRegionGB2QJWidth[640201].ToString();
                    }
                    if (_adjRegionGB2QJWidth.ContainsKey(650201))
                    {
                        tbAdjCountyQJWidth.Text = _adjRegionGB2QJWidth[650201].ToString();
                    }
                    if (_adjRegionGB2QJWidth.ContainsKey(660201))
                    {
                        tbAdjTownQJWidth.Text = _adjRegionGB2QJWidth[660201].ToString();
                    }
                    #endregion
                }

                #region 初始化骑界的颜色
                try
                {
                    ILayer sdmLayer = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("SDM"))).FirstOrDefault();
                    if (sdmLayer != null)
                    {
                        IFeatureClass sdmFC = (sdmLayer as IFeatureLayer).FeatureClass;
                        IRepresentationWorkspaceExtension repWsExt = _app.Workspace.RepersentationWorkspaceExtension;
                        IEnumDatasetName enumDatasetName = repWsExt.get_FeatureClassRepresentationNames(sdmFC);
                        enumDatasetName.Reset();
                        IDatasetName datasetName = enumDatasetName.Next();
                        IRepresentationClass repClass = repWsExt.OpenRepresentationClass(datasetName.Name);

                        //获取颜色,设置控件背景颜色
                        IRepresentationRules rules = repClass.RepresentationRules;
                        rules.Reset();
                        IRepresentationRule rule = null;
                        int ruleID;
                        while (true)
                        {
                            rules.Next(out ruleID, out rule);
                            if (rule == null) break;

                            string ruleName = rules.get_Name(ruleID);
                            if (ruleName  == "内层")
                            {
                                IBasicFillSymbol fillSym = rule.get_Layer(0) as IBasicFillSymbol;
                                if (fillSym != null)
                                {
                                    IFillPattern fillPattern = fillSym.FillPattern;
                                    IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                                    IColor c = fillAttrs.get_Value(0) as IColor;

                                    btMainQJColor.BackColor = ColorHelper.ConvertIColorToColor(c); ;
                                }
                            }
                            else if (ruleName == "外层")
                            {
                                IBasicFillSymbol fillSym = rule.get_Layer(0) as IBasicFillSymbol;
                                if (fillSym != null)
                                {
                                    IFillPattern fillPattern = fillSym.FillPattern;
                                    IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                                    IColor c = fillAttrs.get_Value(0) as IColor;

                                    btAdjQJColor.BackColor = ColorHelper.ConvertIColorToColor(c); ;
                                }
                            }
                        }
                    }
                    
                }
                catch
                {
                }
                #endregion

                //根据是否存在邻区，初始化复选框控件的选择状态
                Dictionary<string, string> envString = _app.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
                if (envString == null)
                {
                    envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
                }
                if (envString.ContainsKey("AttachMap"))
                {
                    bool attachMap = bool.Parse(envString["AttachMap"]);
                    cbAdjQJ.Checked = attachMap;
                    if (!attachMap)
                    {
                        cbMainQJ.Checked = true;
                        panelAdj.Visible = false;

                        this.Height = 240;
                    }
                }

                //根据方案类型，更新各控件的值
                updateByScheme(_ruleDoc);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
        }

        private void cbMainQJ_CheckedChanged(object sender, EventArgs e)
        {
            gbMain.Enabled = cbMainQJ.Checked;
        }

        private void cbAdjQJ_CheckedChanged(object sender, EventArgs e)
        {
            gbAdj.Enabled = cbAdjQJ.Checked;
        }

        private void btMainQJColor_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            IColor color = ColorHelper.ConvertColorToIColor(btMainQJColor.BackColor);

            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
                btMainQJColor.BackColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
            }
        }

        private void btAdjQJColor_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            IColor color = ColorHelper.ConvertColorToIColor(btAdjQJColor.BackColor);

            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
                btAdjQJColor.BackColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            MainRegionQJSql = "";
            if (cbMainQJ.Checked)
            {
                var obj = (KeyValuePair<string, string>)cbMainBOULSQL.SelectedItem;
                MainRegionQJSql = obj.Key;

                MainRegionQJColor = ColorHelper.ConvertColorToCMYK(btMainQJColor.BackColor);

                double proWidth = 0, stateWidth = 0, countyWidth = 0, townWidth = 0;
                double.TryParse(tbMainProvinceQJWidth.Text.ToString(), out proWidth);
                double.TryParse(tbMainStateQJWidth.Text.ToString(), out stateWidth);
                double.TryParse(tbMainCountyQJWidth.Text.ToString(), out countyWidth);
                double.TryParse(tbMainTownQJWidth.Text.ToString(), out townWidth);
                if (proWidth > 0 && _mainRegionGB2QJWidth.ContainsKey(630201))
                {
                    _mainRegionGB2QJWidth[630201] = proWidth;
                }
                if (stateWidth > 0 && _mainRegionGB2QJWidth.ContainsKey(640201))
                {
                    _mainRegionGB2QJWidth[640201] = stateWidth;
                }
                if (countyWidth > 0 && _mainRegionGB2QJWidth.ContainsKey(650201))
                {
                    _mainRegionGB2QJWidth[650201] = countyWidth;
                }
                if (townWidth > 0 && _mainRegionGB2QJWidth.ContainsKey(660201))
                {
                    _mainRegionGB2QJWidth[660201] = townWidth;
                }
            }

            AdjRegionQJSql = "";
            if (cbAdjQJ.Checked)
            {
                var obj = (KeyValuePair<string, string>)cbAdjBOULSQL.SelectedItem;
                AdjRegionQJSql = obj.Key;

                AdjRegionQJColor = ColorHelper.ConvertColorToCMYK(btAdjQJColor.BackColor);

                double proWidth = 0, stateWidth = 0, countyWidth = 0, townWidth = 0;
                double.TryParse(tbAdjProvinceQJWidth.Text.ToString(), out proWidth);
                double.TryParse(tbAdjStateQJWidth.Text.ToString(), out stateWidth);
                double.TryParse(tbAdjCountyQJWidth.Text.ToString(), out countyWidth);
                double.TryParse(tbAdjTownQJWidth.Text.ToString(), out townWidth);
                if (proWidth > 0 && _adjRegionGB2QJWidth.ContainsKey(630201))
                {
                    _adjRegionGB2QJWidth[630201] = proWidth;
                }
                if (stateWidth > 0 && _adjRegionGB2QJWidth.ContainsKey(640201))
                {
                    _adjRegionGB2QJWidth[640201] = stateWidth;
                }
                if (countyWidth > 0 && _adjRegionGB2QJWidth.ContainsKey(650201))
                {
                    _adjRegionGB2QJWidth[650201] = countyWidth;
                }
                if (townWidth > 0 && _adjRegionGB2QJWidth.ContainsKey(660201))
                {
                    _adjRegionGB2QJWidth[660201] = townWidth;
                }
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void updateByScheme(XDocument ruleDoc, string scheme = "自定义")
        {
            var schemeItem = _ruleDoc.Element("Template").Element("Scheme");
            if (schemeItem == null)
                return;

            var typeItems = schemeItem.Elements("Type");
            XElement mapTypeItem = null;
            foreach (XElement ele in typeItems)
            {
                string sn = ele.Attribute("MapLevel").Value.ToString();
                if (sn == scheme)
                {
                    mapTypeItem = ele;
                    break;
                }
            }
            if (mapTypeItem == null)
                return;


            #region 初始化骑界生成范围相关控件的值
            var boulItem = mapTypeItem.Element("BOUL");
            if (boulItem != null)
            {
                string mainQJName = boulItem.Attribute("mainRegion").Value.ToString().Trim();
                string adjQJName = boulItem.Attribute("adjRegion").Value.ToString().Trim();

                int index = cbMainBOULSQL.Items.Count > 0 ? 0 : -1;
                for (int i = 0; i < cbMainBOULSQL.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<string, string>)cbMainBOULSQL.Items[i];
                    if (kv.Value == mainQJName)
                        index = i;
                }
                cbMainBOULSQL.SelectedIndex = index;

                index = cbAdjBOULSQL.Items.Count > 0 ? 0 : -1;
                for (int i = 0; i < cbAdjBOULSQL.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<string, string>)cbAdjBOULSQL.Items[i];
                    if (kv.Value == adjQJName)
                        index = i;
                }
                cbAdjBOULSQL.SelectedIndex = index;

            }
            #endregion
        }

    }
}

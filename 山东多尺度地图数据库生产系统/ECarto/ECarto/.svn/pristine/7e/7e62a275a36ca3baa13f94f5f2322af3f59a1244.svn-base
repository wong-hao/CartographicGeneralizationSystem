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
using System.IO;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class FeatureSelectForm : Form
    {
        #region 河流选取属性
        /// <summary>
        /// 是否对水系要素进行选取
        /// </summary>
        public bool BRiverSelect
        {
            set;
            get;
        }

        /// <summary>
        /// 主区河流线保留的选取等级
        /// </summary>
        public int MainHYDLGrade
        {
            set;
            get;
        }

        /// <summary>
        /// 邻区河流线保留的选取等级
        /// </summary>
        public int AdjacentHYDLGrade
        {
            set;
            get;
        }

        /// <summary>
        /// 主区河流面保留的面积阀值
        /// </summary>
        public double MainHYDAArea
        {
            set;
            get;
        }

        /// <summary>
        /// 邻区河流面保留的面积阀值
        /// </summary>
        public double AdjacentHYDAArea
        {
            set;
            get;
        }
        #endregion

        #region 道路选取属性
        /// <summary>
        /// 是否对道路要素进行选取
        /// </summary>
        public bool BRoadSelect
        {
            set;
            get;
        }

        /// <summary>
        /// 主区道路线保留的选取等级
        /// </summary>
        public int MainLRDLGrade
        {
            set;
            get;
        }

        /// <summary>
        /// 邻区道路线保留的选取等级
        /// </summary>
        public int AdjacentLRDLGrade
        {
            set;
            get;
        }

        /// <summary>
        /// 是否保留地铁、轻轨要求
        /// </summary>
        public bool BReserveSubway
        {
            set;
            get;
        }
        #endregion

        #region 街区面选取属性
        /// <summary>
        /// 是否对街区面要素进行选取
        /// </summary>
        public bool BRESASelect
        {
            set;
            get;
        }

        /// <summary>
        /// 主区街区面保留级别（与城市道路分类码对应1/2/3/4/5）
        /// </summary>
        public int MainSelectRESALevel
        {
            set;
            get;
        }

        /// <summary>
        /// 邻区街区面保留级别（与城市道路分类码对应1/2/3/4/5）
        /// </summary>
        public int AdjacentSelectRESALevel
        {
            set;
            get;
        }

        /// <summary>
        /// 街区面保留级别->街区面保留的SQL语句
        /// </summary>
        public Dictionary<int, string> Level2SelectRESASQL
        {
            get
            {
                return _level2SelectRESASQL;
            }
        }
        private Dictionary<int, string> _level2SelectRESASQL;


        #endregion

        #region 地名点选取属性
        /// <summary>
        /// 是否对地名点要素进行选取
        /// </summary>
        public bool BAGNPSelect
        {
            set;
            get;
        }

        /// <summary>
        /// 主区是否按Priority选取，否则按Class选取
        /// </summary>
        public bool EnableMainAGNPPriority
        {
            get
            {
                return rdbMainAGNPPriority.Checked;
            }
        }

        /// <summary>
        /// 邻区是否按Priority选取，否则按Class选取
        /// </summary>
        public bool EnableAdjAGNPPriority
        {
            get
            {
                return rdbAdjAGNPPriority.Checked;
            }
        }

        /// <summary>
        /// 主区地名点保留的优先级
        /// </summary>
        public int MainAGNPPriority
        {
            set;
            get;
        }
        /// <summary>
        /// 主区需显示的地名点SQL语句与其对应的选择状态
        /// </summary>
        public Dictionary<string, bool> MainAGNPSQL2SelectState
        {
            get
            {
                return _mainAGNPSQL2SelectState;
            }
        }
        private Dictionary<string, bool> _mainAGNPSQL2SelectState;

        /// <summary>
        /// 邻区地名点保留的优先级
        /// </summary>
        public int AdjacentAGNPPriority
        {
            set;
            get;
        }
        /// <summary>
        /// 邻区需显示的地名点SQL语句与其对应的选择状态
        /// </summary>
        public Dictionary<string, bool> AdjacentAGNPSQL2SelectState
        {
            get
            {
                return _adjacentAGNPSQL2SelectState;
            }
        }
        private Dictionary<string, bool> _adjacentAGNPSQL2SelectState;
        #endregion

        #region POI选取属性
        /// <summary>
        /// 是否对POI要素进行选取
        /// </summary>
        public bool BPOISelect
        {
            set;
            get;
        }

        /// <summary>
        /// 主区POI保留的优先级
        /// </summary>
        public int MainPOIPriority
        {
            set;
            get;
        }

        /// <summary>
        /// 邻区POI保留的优先级
        /// </summary>
        public int AdjPOIPriority
        {
            set;
            get;
        }
        #endregion

        #region 植被面选取属性
        /// <summary>
        /// 是否对植被要素进行选取
        /// </summary>
        public bool BVEGASelect
        {
            set;
            get;
        }

        /// <summary>
        /// 主区植被面保留的面积阀值
        /// </summary>
        public double MainVEGAArea
        {
            set;
            get;
        }

        /// <summary>
        /// 邻区植被面保留的面积阀值
        /// </summary>
        public double AdjacentVEGAArea
        {
            set;
            get;
        }
        #endregion

        #region 境界线选取属性
        /// <summary>
        /// 是否对境界线要素进行选取
        /// </summary>
        public bool BBOULSelect
        {
            set;
            get;
        }

        /// <summary>
        /// 主区境界线保留的最小级别GB值
        /// </summary>
        public int MainBOULGB
        {
            set;
            get;
        }

        /// <summary>
        /// 邻区境界线保留的最小级别GB值
        /// </summary>
        public int AdjacentBOULGB
        {
            set;
            get;
        }
        #endregion

        #region 表面注记选取属性
        /// <summary>
        /// 是否对表面注记要素进行选取
        /// </summary>
        public bool BBOUAAnnoSelect
        {
            set;
            get;
        }

        /// <summary>
        /// 本省的PAC开头两位字符（如江苏省PAC为320000，则开头两位字符为"32"）
        /// </summary>
        public string ProvPAC
        {
            get
            {
                return _provPac;
            }
        }
        private string _provPac;

        /// <summary>
        /// 主区保留的表面注记对应的BOUA要素类名
        /// </summary>
        public string MainFCNameOfBOUAAnno
        {
            set;
            get;
        }

        /// <summary>
        /// 邻区（省内）保留的表面注记对应的BOUA要素类名
        /// </summary>
        public string InProvAdjacentFCNameOfBOUAAnno
        {
            set;
            get;
        }

        /// <summary>
        /// 邻区（省外）保留的表面注记对应的BOUA要素类名
        /// </summary>
        public string OutProvAdjacentFCNameOfBOUAAnno
        {
            set;
            get;
        }
        #endregion

        public string BaseMap
        {
            get
            {
                return _baseMap;
            }
        }
        private string _baseMap;

        private GApplication _app;
        private double _mapScale;
        private XDocument _ruleDoc; 
        private bool _bAttachMap;

        public class ScaleRange
        {
            public double MinScale
            {
                get;
                set;
            }
            public double MaxScale
            {
                get;
                set;
            }
            public ScaleRange(double minScale, double maxScale)
            {
                MinScale = minScale;
                MaxScale = maxScale;
            }
        }


        public FeatureSelectForm(GApplication app)
        {
            InitializeComponent();

            _mainAGNPSQL2SelectState = new Dictionary<string, bool>();
            _adjacentAGNPSQL2SelectState = new Dictionary<string, bool>();
            _level2SelectRESASQL = new Dictionary<int, string>();
            _provPac = "";
            _app = app;
            _mapScale = _app.ActiveView.FocusMap.ReferenceScale;
            _ruleDoc = null;
            _baseMap = "一般";
            _bAttachMap = true;
        }

        private void FeatureSelectForm_Load(object sender, EventArgs e)
        {
            try
            {
                Dictionary<string, string> envString = _app.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
                if (envString == null)
                {
                    envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
                }
                if (envString.ContainsKey("AttachMap"))
                    _bAttachMap = bool.Parse(envString["AttachMap"]);
                if (envString.ContainsKey("BaseMap"))
                    _baseMap = envString["BaseMap"];

                string fileName = string.Format("{0}\\专家库\\要素选取\\{1}\\FeatureSelectRule.xml", _app.Template.Root, _baseMap);
                if (!File.Exists(fileName))
                {
                    throw new Exception(string.Format("没有找到选取配置文件：{0}", fileName));
                }
                _ruleDoc = XDocument.Load(fileName);

                #region 获取方案类型
                List<string> schemeNameList = new List<string>();
                var schemeItem = _ruleDoc.Element("Template").Element("Scheme");
                if (schemeItem != null)
                {
                    bool riverGroupVisble = bool.Parse(schemeItem.Attribute("riverGroup").Value);
                    bool roadGroupVisble = bool.Parse(schemeItem.Attribute("roadGroup").Value);
                    bool resaGroupVisble = bool.Parse(schemeItem.Attribute("resaGroup").Value);
                    bool agnpGroupVisble = bool.Parse(schemeItem.Attribute("agnpGroup").Value);
                    bool poiGroupVisble = bool.Parse(schemeItem.Attribute("poiGroup").Value);
                    bool vegaGroupVisble = bool.Parse(schemeItem.Attribute("vegaGroup").Value);
                    bool boulGroupVisble = bool.Parse(schemeItem.Attribute("boulGroup").Value);
                    bool bouaAnnoGroupVisble = bool.Parse(schemeItem.Attribute("bouaAnnoGroup").Value);

                    panelRiverGroup.Visible = riverGroupVisble;
                    if (!riverGroupVisble)
                    {
                        this.Height -= panelRiverGroup.Height;
                    }
                    panelRoadGroup.Visible = roadGroupVisble;
                    if (!roadGroupVisble)
                    {
                        this.Height -= panelRoadGroup.Height;
                    }
                    panelResaGroup.Visible = resaGroupVisble;
                    if (!resaGroupVisble)
                    {
                        this.Height -= panelResaGroup.Height;
                    }
                    panelAGNPGroup.Visible = agnpGroupVisble;
                    if (!agnpGroupVisble)
                    {
                        this.Height -= panelAGNPGroup.Height;
                    }
                    panelPOIGroup.Visible = poiGroupVisble;
                    if (!poiGroupVisble)
                    {
                        this.Height -= panelPOIGroup.Height;
                    }
                    panelVegaGroup.Visible = vegaGroupVisble;
                    if (!vegaGroupVisble)
                    {
                        this.Height -= panelVegaGroup.Height;
                    }
                    panelBoulGroup.Visible = boulGroupVisble;
                    if (!boulGroupVisble)
                    {
                        this.Height -= panelBoulGroup.Height;
                    }
                    panelBouaAnnoGroup.Visible = bouaAnnoGroupVisble;
                    if (!bouaAnnoGroupVisble)
                    {
                        this.Height -= panelBouaAnnoGroup.Height;
                    }


                    var typeItems = schemeItem.Elements("Type");
                    foreach (XElement ele in typeItems)
                    {
                        string sn = ele.Attribute("MapLevel").Value.ToString();
                        if (!schemeNameList.Contains(sn))
                            schemeNameList.Add(sn);
                    }
                }

                cbMapType.Items.AddRange(schemeNameList.ToArray());
                #endregion

                #region 初始化各控件值
                var contentItem = _ruleDoc.Element("Template").Element("Content");
                if (contentItem != null)
                {
                    #region 初始化水系线选取相关控件的值
                    var hydlItem = contentItem.Element("HYDL");
                    if (hydlItem != null)
                    {
                        cbMainHYDLGrade.ValueMember = "Key";
                        cbMainHYDLGrade.DisplayMember = "Value";

                        cbAdjHYDLGrade.ValueMember = "Key";
                        cbAdjHYDLGrade.DisplayMember = "Value";

                        cbMainHYDLGrade.Items.Add(new KeyValuePair<ScaleRange, string>(null, "不参与"));
                        cbAdjHYDLGrade.Items.Add(new KeyValuePair<ScaleRange, string>(null, "不参与"));

                        var items = hydlItem.Elements("Item");
                        foreach (XElement ele in items)
                        {
                            double minScale, maxScale;
                            double.TryParse(ele.Attribute("minScale").Value.ToString(), out minScale);
                            double.TryParse(ele.Attribute("maxScale").Value.ToString(), out maxScale);
                            ScaleRange sr = new ScaleRange(minScale, maxScale);

                            cbMainHYDLGrade.Items.Add(new KeyValuePair<ScaleRange, string>(sr, ele.Value));
                            cbAdjHYDLGrade.Items.Add(new KeyValuePair<ScaleRange, string>(sr, ele.Value));
                        }
                    }
                    #endregion

                    #region 初始化道路线选取相关控件的值
                    var lrdlItem = contentItem.Element("LRDL");
                    if (lrdlItem != null)
                    {
                        cbMainLRDLGrade.ValueMember = "Key";
                        cbMainLRDLGrade.DisplayMember = "Value";

                        cbAdjLRDLGrade.ValueMember = "Key";
                        cbAdjLRDLGrade.DisplayMember = "Value";

                        cbMainLRDLGrade.Items.Add(new KeyValuePair<ScaleRange, string>(null, "不参与"));
                        cbAdjLRDLGrade.Items.Add(new KeyValuePair<ScaleRange, string>(null, "不参与"));

                        var items = lrdlItem.Elements("Item");
                        foreach (XElement ele in items)
                        {
                            double minScale, maxScale;
                            double.TryParse(ele.Attribute("minScale").Value.ToString(), out minScale);
                            double.TryParse(ele.Attribute("maxScale").Value.ToString(), out maxScale);
                            ScaleRange sr = new ScaleRange(minScale, maxScale);

                            cbMainLRDLGrade.Items.Add(new KeyValuePair<ScaleRange, string>(sr, ele.Value));
                            cbAdjLRDLGrade.Items.Add(new KeyValuePair<ScaleRange, string>(sr, ele.Value));
                        }
                    }
                    #endregion

                    #region 初始化街区面选取相关控件的值
                    var resaItem = contentItem.Element("RESA");
                    if (resaItem != null)
                    {
                        cbMainRESALevel.ValueMember = "Key";
                        cbMainRESALevel.DisplayMember = "Value";

                        cbAdjRESALevel.ValueMember = "Key";
                        cbAdjRESALevel.DisplayMember = "Value";

                        cbMainRESALevel.Items.Add(new KeyValuePair<int, string>(-1, "不参与"));
                        cbAdjRESALevel.Items.Add(new KeyValuePair<int, string>(-1, "不参与"));

                        var items = resaItem.Elements("Item");
                        foreach (XElement ele in items)
                        {
                            string sql = ele.Attribute("sql").Value.ToString();
                            int level = 0;
                            int.TryParse(ele.Attribute("cityroadClass").Value.ToString(), out level);

                            if (!_level2SelectRESASQL.ContainsKey(level))
                                _level2SelectRESASQL.Add(level, sql);

                            cbMainRESALevel.Items.Add(new KeyValuePair<int, string>(level, ele.Value));
                            cbAdjRESALevel.Items.Add(new KeyValuePair<int, string>(level, ele.Value));
                        }

                    }
                    #endregion

                    #region 初始化居民地点选取相关控件的值
                    var AGNPPriorityItem = contentItem.Element("AGNPPriority");
                    if (AGNPPriorityItem != null)
                    {
                        cbMainAGNPPriority.ValueMember = "Key";
                        cbMainAGNPPriority.DisplayMember = "Value";
                        cbAdjAGNPPriority.ValueMember = "Key";
                        cbAdjAGNPPriority.DisplayMember = "Value";

                        cbMainAGNPPriority.Items.Add(new KeyValuePair<ScaleRange, string>(null, "不参与"));
                        cbAdjAGNPPriority.Items.Add(new KeyValuePair<ScaleRange, string>(null, "不参与"));

                        var items = AGNPPriorityItem.Elements("Item");
                        foreach (XElement ele in items)
                        {
                            double minScale, maxScale;
                            double.TryParse(ele.Attribute("minScale").Value.ToString(), out minScale);
                            double.TryParse(ele.Attribute("maxScale").Value.ToString(), out maxScale);
                            ScaleRange sr = new ScaleRange(minScale, maxScale);

                            cbMainAGNPPriority.Items.Add(new KeyValuePair<ScaleRange, string>(sr, ele.Value));
                            cbAdjAGNPPriority.Items.Add(new KeyValuePair<ScaleRange, string>(sr, ele.Value));
                        }
                    }

                    var AGNPClassItem = contentItem.Element("AGNPClass");
                    if (AGNPClassItem != null)
                    {
                        clbMainAGNPClass.ValueMember = "Key";
                        clbMainAGNPClass.DisplayMember = "Value";
                        clbAdjAGNPClass.ValueMember = "Key";
                        clbAdjAGNPClass.DisplayMember = "Value";

                        var items = AGNPClassItem.Elements("Item");
                        foreach (XElement ele in items)
                        {
                            string sql = ele.Attribute("sql").Value.ToString();

                            clbMainAGNPClass.Items.Add(new KeyValuePair<string, string>(sql, ele.Value));
                            clbAdjAGNPClass.Items.Add(new KeyValuePair<string, string>(sql, ele.Value));
                        }
                    }
                    #endregion

                    #region 初始化POI选取相关控件的值
                    var poiItem = contentItem.Element("POI");
                    if (poiItem != null)
                    {
                        cbMainPOIPriority.ValueMember = "Key";
                        cbMainPOIPriority.DisplayMember = "Value";

                        cbAdjPOIPriority.ValueMember = "Key";
                        cbAdjPOIPriority.DisplayMember = "Value";

                        cbMainPOIPriority.Items.Add(new KeyValuePair<ScaleRange, string>(null, "不参与"));
                        cbAdjPOIPriority.Items.Add(new KeyValuePair<ScaleRange, string>(null, "不参与"));

                        var items = poiItem.Elements("Item");
                        foreach (XElement ele in items)
                        {
                            double minScale, maxScale;
                            double.TryParse(ele.Attribute("minScale").Value.ToString(), out minScale);
                            double.TryParse(ele.Attribute("maxScale").Value.ToString(), out maxScale);
                            ScaleRange sr = new ScaleRange(minScale, maxScale);

                            cbMainPOIPriority.Items.Add(new KeyValuePair<ScaleRange, string>(sr, ele.Value));
                            cbAdjPOIPriority.Items.Add(new KeyValuePair<ScaleRange, string>(sr, ele.Value));
                        }
                    }
                    #endregion

                    #region 初始化境界线选取相关控件的值
                    var boulItem = contentItem.Element("BOUL");
                    if (boulItem != null)
                    {
                        cbMainBOULLevel.ValueMember = "Key";
                        cbMainBOULLevel.DisplayMember = "Value";

                        cbAdjBOULLevel.ValueMember = "Key";
                        cbAdjBOULLevel.DisplayMember = "Value";


                        cbMainBOULLevel.Items.Add(new KeyValuePair<string, string>("", "不参与"));
                        cbAdjBOULLevel.Items.Add(new KeyValuePair<string, string>("", "不参与"));

                        var items = boulItem.Elements("Item");
                        foreach (XElement ele in items)
                        {
                            string gb = ele.Attribute("gb").Value.ToString();

                            cbMainBOULLevel.Items.Add(new KeyValuePair<string, string>(gb, ele.Value));
                            cbAdjBOULLevel.Items.Add(new KeyValuePair<string, string>(gb, ele.Value));
                        }

                    }
                    #endregion

                    #region 初始化表面注记选取相关控件的值
                    var bouaAnnoItem = contentItem.Element("BOUAAnno");
                    if (bouaAnnoItem != null)
                    {
                        cbMainBOUAAnnoLevel.ValueMember = "Key";
                        cbMainBOUAAnnoLevel.DisplayMember = "Value";

                        cbInProvBOUAAnnoLevel.ValueMember = "Key";
                        cbInProvBOUAAnnoLevel.DisplayMember = "Value";

                        cbOutProvBOUAAnnoLevel.ValueMember = "Key";
                        cbOutProvBOUAAnnoLevel.DisplayMember = "Value";

                        cbMainBOUAAnnoLevel.Items.Add(new KeyValuePair<string, string>("", "不选取"));
                        cbInProvBOUAAnnoLevel.Items.Add(new KeyValuePair<string, string>("", "不选取"));
                        cbOutProvBOUAAnnoLevel.Items.Add(new KeyValuePair<string, string>("", "不选取"));

                        string provPac = bouaAnnoItem.Attribute("ProvPac").Value.ToString().Trim();
                        if (provPac.Length > 2)
                            _provPac = provPac.Substring(0, 2);

                        var items = bouaAnnoItem.Elements("Item");
                        foreach (XElement ele in items)
                        {
                            string fcName = ele.Attribute("fcName").Value.ToString();

                            cbMainBOUAAnnoLevel.Items.Add(new KeyValuePair<string, string>(fcName, ele.Value));
                            cbInProvBOUAAnnoLevel.Items.Add(new KeyValuePair<string, string>(fcName, ele.Value));
                            cbOutProvBOUAAnnoLevel.Items.Add(new KeyValuePair<string, string>(fcName, ele.Value));
                        }

                    }
                    #endregion
                }

                if (!_bAttachMap)
                {
                    lbMainRiverGrade.Text = lbMainRiverGrade.Text.Replace("主区", "");
                    lbMainRiverArea.Text = lbMainRiverArea.Text.Replace("主区", "");
                    lbMainRoadGrade.Text = lbMainRoadGrade.Text.Replace("主区", "");
                    lbMainRESALevel.Text = lbMainRESALevel.Text.Replace("主区", "");
                    rdbMainAGNPPriority.Text = rdbMainAGNPPriority.Text.Replace("主区", "");
                    rdbMainAGNPClass.Text = rdbMainAGNPClass.Text.Replace("主区", "");
                    lbMainPOIGrade.Text = lbMainPOIGrade.Text.Replace("主区", "");
                    lbMainVEGAArea.Text = lbMainVEGAArea.Text.Replace("主区", ""); 
                    lbMainBoulLevel.Text = lbMainBoulLevel.Text.Replace("主区", "");
                    lbMainBouaAnnoLevel.Text = lbMainBouaAnnoLevel.Text.Replace("主区", "");

                    panelRiver.Visible = false;
                    panelRoad.Visible = false;
                    panelRESA.Visible = false;
                    panelAGNP.Visible = false;
                    panelPOI.Visible = false;
                    panelVEGA.Visible = false;
                    panelBOUL.Visible = false;
                    panelBOUAAnno.Visible = false;

                    gbRiver.Width = 290;
                    gbRoad.Width = 290;
                    gbRESA.Width = 290;
                    gbAGNP.Width = 290;
                    
                    gbBOUL.Width = 290;
                    gbBOUAAnno.Width = 290;

                    this.Width = 400;
                }
                #endregion

                //根据方案类型，更新各控件的值
                updateByScheme(_ruleDoc);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                this.Close();
            }
        }

        private void cbMapType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //根据方案类型，更新各控件的值
            updateByScheme(_ruleDoc, cbMapType.Text);
        }

        private void cbRiverSelect_CheckedChanged(object sender, EventArgs e)
        {
            gbRiver.Enabled = cbRiverSelect.Checked;
        }

        private void cbRoadSelect_CheckedChanged(object sender, EventArgs e)
        {
            gbRoad.Enabled = cbRoadSelect.Checked;
        }

        private void cbRESASelect_CheckedChanged(object sender, EventArgs e)
        {
            gbRESA.Enabled = cbRESASelect.Checked;
        }

        private void cbANGPSelect_CheckedChanged(object sender, EventArgs e)
        {
            gbAGNP.Enabled = cbANGPSelect.Checked;
        }

        private void cbPOISelect_CheckedChanged(object sender, EventArgs e)
        {
            gbPOI.Enabled = cbPOISelect.Checked;
        }

        private void cbVEGASelect_CheckedChanged(object sender, EventArgs e)
        {
            gbVEGA.Enabled = cbVEGASelect.Checked;
        }

        private void cbBOULSelect_CheckedChanged(object sender, EventArgs e)
        {
            gbBOUL.Enabled = cbBOULSelect.Checked;
        }

        private void cbBOUAAnnoSelect_CheckedChanged(object sender, EventArgs e)
        {
            gbBOUAAnno.Enabled = cbBOUAAnnoSelect.Checked;
        }

        private void rdbMainAGNP_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbMainAGNPPriority.Checked)
            {
                cbMainAGNPPriority.Enabled = true;
                clbMainAGNPClass.Enabled = false;
            }
            else
            {
                cbMainAGNPPriority.Enabled = false;
                clbMainAGNPClass.Enabled = true;
            }
        }

        private void rdbAdjAGNP_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbAdjAGNPPriority.Checked)
            {
                cbAdjAGNPPriority.Enabled = true;
                clbAdjAGNPClass.Enabled = false;
            }
            else
            {
                cbAdjAGNPPriority.Enabled = false;
                clbAdjAGNPClass.Enabled = true;
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (!cbRiverSelect.Checked && !cbRoadSelect.Checked && !cbRESASelect.Checked && !cbANGPSelect.Checked &&
                !cbPOISelect.Checked && !cbVEGASelect.Checked && !cbBOULSelect.Checked && !cbBOUAAnnoSelect.Checked)
            {
                MessageBox.Show("请至少选择一类要素进行选取操作！");
                return;
            }

            BRiverSelect = cbRiverSelect.Checked;
            if (BRiverSelect)
            {
                #region 水系
                var obj = (KeyValuePair<ScaleRange, string>)cbMainHYDLGrade.SelectedItem;
                if (obj.Key == null)//不参与选取
                {
                    MainHYDLGrade = -1;
                }
                else
                {
                    int grade;
                    int.TryParse(obj.Value.ToString(), out grade);

                    MainHYDLGrade = grade;
                }

                
                obj = (KeyValuePair<ScaleRange, string>)cbAdjHYDLGrade.SelectedItem;
                if (obj.Key == null)//不参与选取
                {
                    AdjacentHYDLGrade = -1;
                }
                else
                {
                    int grade;
                    int.TryParse(obj.Value.ToString(), out grade);

                    AdjacentHYDLGrade = grade;
                }


                double area;

                double.TryParse(tbMainHYDAArea.Text, out area);
                MainHYDAArea = area;
                
                double.TryParse(tbAdjHYDAArea.Text, out area);
                AdjacentHYDAArea = area;
                #endregion
            }

            BRoadSelect = cbRoadSelect.Checked;
            if (BRoadSelect)
            {
                #region 道路
                var obj = (KeyValuePair<ScaleRange, string>)cbMainLRDLGrade.SelectedItem;
                if (obj.Key == null)//不参与选取
                {
                    MainLRDLGrade = -1;
                }
                else
                {
                    int grade;
                    int.TryParse(obj.Value.ToString(), out grade);

                    MainLRDLGrade = grade;
                }

                obj = (KeyValuePair<ScaleRange, string>)cbAdjLRDLGrade.SelectedItem;
                if (obj.Key == null)//不参与选取
                {
                    AdjacentLRDLGrade = -1;
                }
                else
                {
                    int grade;
                    int.TryParse(obj.Value.ToString(), out grade);

                    AdjacentLRDLGrade = grade;
                }

                BReserveSubway = cbSubway.Checked;
                #endregion
            }

            BRESASelect = cbRESASelect.Checked;
            if (BRESASelect)
            {
                #region 居民地面
                var obj = (KeyValuePair<int, string>)cbMainRESALevel.SelectedItem;
                MainSelectRESALevel = obj.Key;

                obj = (KeyValuePair<int, string>)cbAdjRESALevel.SelectedItem;
                AdjacentSelectRESALevel = obj.Key;

                #endregion
            }

            BAGNPSelect = cbANGPSelect.Checked;
            if (BAGNPSelect)
            {
                #region 居民地点
                var obj = (KeyValuePair<ScaleRange, string>)cbMainAGNPPriority.SelectedItem;
                if (obj.Key == null)//不参与选取
                {
                    MainAGNPPriority = -1;
                }
                else
                {
                    int priority;
                    int.TryParse(obj.Value.ToString(), out priority);

                    MainAGNPPriority = priority;
                }

                obj = (KeyValuePair<ScaleRange, string>)cbAdjAGNPPriority.SelectedItem;
                if (obj.Key == null)//不参与选取
                {
                    AdjacentAGNPPriority = -1;
                }
                else
                {
                    int priority;
                    int.TryParse(obj.Value.ToString(), out priority);

                    AdjacentAGNPPriority = priority;
                }

                for (int i = 0; i < clbMainAGNPClass.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<string, string>)clbMainAGNPClass.Items[i];

                    _mainAGNPSQL2SelectState.Add(kv.Key, clbMainAGNPClass.GetItemChecked(i));
                }

                for (int i = 0; i < clbAdjAGNPClass.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<string, string>)clbAdjAGNPClass.Items[i];

                    _adjacentAGNPSQL2SelectState.Add(kv.Key, clbAdjAGNPClass.GetItemChecked(i));
                }
                #endregion
            }

            BPOISelect = cbPOISelect.Checked;
            if (BPOISelect)
            {
                #region POI
                var obj = (KeyValuePair<ScaleRange, string>)cbMainPOIPriority.SelectedItem;
                if (obj.Key == null)//不参与选取
                {
                    MainPOIPriority = -1;
                }
                else
                {
                    int priority;
                    int.TryParse(obj.Value.ToString(), out priority);

                    MainPOIPriority = priority;
                }
                
                obj = (KeyValuePair<ScaleRange, string>)cbAdjPOIPriority.SelectedItem;
                if (obj.Key == null)//不参与选取
                {
                    AdjPOIPriority = -1;
                }
                else
                {
                    int priority;
                    int.TryParse(obj.Value.ToString(), out priority);

                    AdjPOIPriority = priority;
                }

                #endregion
            }

            BVEGASelect = cbVEGASelect.Checked;
            if (BVEGASelect)
            {
                #region 植被面
                double area;

                double.TryParse(tbMainVEGAArea.Text, out area);
                MainVEGAArea = area;

                double.TryParse(tbAdjVEGAArea.Text, out area);
                AdjacentVEGAArea = area;
                #endregion
            }

            BBOULSelect = cbBOULSelect.Checked;
            if (BBOULSelect)
            {
                #region 境界线
                var obj = (KeyValuePair<string, string>)cbMainBOULLevel.SelectedItem;
                if (obj.Key == "")//不参与选取
                {
                    MainBOULGB = -1;
                }
                else
                {
                    int gb;
                    int.TryParse(obj.Key.ToString(), out gb);

                    MainBOULGB = gb;
                }

                obj = (KeyValuePair<string, string>)cbAdjBOULLevel.SelectedItem;
                if (obj.Key == "")//不参与选取
                {
                    AdjacentBOULGB = -1;
                }
                else
                {
                    int gb;
                    int.TryParse(obj.Key.ToString(), out gb);

                    AdjacentBOULGB = gb;
                }
                #endregion
            }

            BBOUAAnnoSelect = cbBOUAAnnoSelect.Checked;
            if (BBOUAAnnoSelect)
            {
                #region 表面注记
                var obj = (KeyValuePair<string, string>)cbMainBOUAAnnoLevel.SelectedItem;
                MainFCNameOfBOUAAnno = obj.Key;

                obj = (KeyValuePair<string, string>)cbInProvBOUAAnnoLevel.SelectedItem;
                InProvAdjacentFCNameOfBOUAAnno = obj.Key;

                obj = (KeyValuePair<string, string>)cbOutProvBOUAAnnoLevel.SelectedItem;
                OutProvAdjacentFCNameOfBOUAAnno = obj.Key;
                #endregion
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;

        }

        private void updateByScheme(XDocument ruleDoc, string scheme = "自定义")
        {
            //更新方案控件
            int index = cbMapType.Items.Count > 0 ? 0 : -1;
            if (cbMapType.Items.Contains(scheme))
            {
                index = cbMapType.Items.IndexOf(scheme);  
            }
            cbMapType.SelectedIndex = index;
            scheme = cbMapType.SelectedItem.ToString();

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

            #region 水系线相关控件
            var hydlItem = mapTypeItem.Element("HYDL");
            if (hydlItem != null)
            {
                string mainGrade = hydlItem.Attribute("mainGrade").Value.ToString().Trim();
                string adjGrade = hydlItem.Attribute("adjGrade").Value.ToString().Trim();

                index = cbMainHYDLGrade.Items.Count > 0 ? 0 : -1;
                for (int i = 0; i < cbMainHYDLGrade.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<ScaleRange, string>)cbMainHYDLGrade.Items[i];
                    if(kv.Value == mainGrade)
                        index = i;
                }
                cbMainHYDLGrade.SelectedIndex = index;

                index = cbAdjHYDLGrade.Items.Count > 0 ? 0 : -1;
                if (_bAttachMap)
                {
                    for (int i = 0; i < cbAdjHYDLGrade.Items.Count; ++i)
                    {
                        var kv = (KeyValuePair<ScaleRange, string>)cbAdjHYDLGrade.Items[i];
                        if (kv.Value == adjGrade)
                            index = i;
                    }
                }
                cbAdjHYDLGrade.SelectedIndex = index;
            }
            #endregion

            #region 水系面相关控件
            var hydaItem = mapTypeItem.Element("HYDA");
            if (hydaItem != null)
            {
                string mainArea = hydaItem.Attribute("mainArea").Value.ToString().Trim();
                string adjArea = hydaItem.Attribute("adjArea").Value.ToString().Trim();

                tbMainHYDAArea.Text = mainArea;
                if (_bAttachMap)
                {
                    tbAdjHYDAArea.Text = adjArea;
                }
                else
                {
                    tbAdjHYDAArea.Text = "0";
                }
            }
            #endregion

            #region 道路线相关控件
            var lrdlItem = mapTypeItem.Element("LRDL");
            if (lrdlItem != null)
            {
                string mainGrade = lrdlItem.Attribute("mainGrade").Value.ToString().Trim();
                string adjGrade = lrdlItem.Attribute("adjGrade").Value.ToString().Trim();

                index = cbMainLRDLGrade.Items.Count > 0 ? 0 : -1;
                for (int i = 0; i < cbMainLRDLGrade.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<ScaleRange, string>)cbMainLRDLGrade.Items[i];
                    if (kv.Value == mainGrade)
                        index = i;
                }
                cbMainLRDLGrade.SelectedIndex = index;

                index = cbAdjLRDLGrade.Items.Count > 0 ? 0 : -1;
                if (_bAttachMap)
                {
                    for (int i = 0; i < cbAdjLRDLGrade.Items.Count; ++i)
                    {
                        var kv = (KeyValuePair<ScaleRange, string>)cbAdjLRDLGrade.Items[i];
                        if (kv.Value == adjGrade)
                            index = i;
                    }
                }
                cbAdjLRDLGrade.SelectedIndex = index;
            }
            #endregion

            #region 街区面相关控件
            var resaItem = mapTypeItem.Element("RESA");
            if (resaItem != null)
            {
                string mainClass = resaItem.Attribute("mainRegion").Value.ToString().Trim();
                string adjClass = resaItem.Attribute("adjRegion").Value.ToString().Trim();

                index = cbMainRESALevel.Items.Count > 0 ? 0 : -1;
                for (int i = 0; i < cbMainRESALevel.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<int, string>)cbMainRESALevel.Items[i];
                    if (kv.Value == mainClass)
                        index = i;
                }
                cbMainRESALevel.SelectedIndex = index;

                index = cbAdjRESALevel.Items.Count > 0 ? 0 : -1;
                if (_bAttachMap)
                {
                    for (int i = 0; i < cbAdjRESALevel.Items.Count; ++i)
                    {
                        var kv = (KeyValuePair<int, string>)cbAdjRESALevel.Items[i];
                        if (kv.Value == adjClass)
                            index = i;
                    }
                }
                cbAdjRESALevel.SelectedIndex = index;
            }
            #endregion

            #region 居民地点相关控件
            var mainAGNPPriorityItem = mapTypeItem.Element("MainAGNPPriority");
            if (mainAGNPPriorityItem != null)
            {
                string priority = mainAGNPPriorityItem.Attribute("priority").Value.ToString().Trim();

                index = cbMainAGNPPriority.Items.Count > 0 ? 0 : -1;
                for (int i = 0; i < cbMainAGNPPriority.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<ScaleRange, string>)cbMainAGNPPriority.Items[i];
                    if (kv.Value == priority)
                        index = i;
                }
                cbMainAGNPPriority.SelectedIndex = index;
            }

            var mainAGNPClassItem = mapTypeItem.Element("MainAGNPClass");
            if (mainAGNPClassItem != null)
            {
                for (int i = 0; i < clbMainAGNPClass.Items.Count; ++i)
                {
                    clbMainAGNPClass.SetItemChecked(i, false);
                }

                var items = mainAGNPClassItem.Elements("Item");
                foreach (XElement ele in items)
                {
                    bool selState = bool.Parse(ele.Attribute("slected").Value);
                    string name = ele.Value;

                    if (selState)
                    {
                        for (int i = 0; i < clbMainAGNPClass.Items.Count; ++i)
                        {
                            var kv = (KeyValuePair<string, string>)clbMainAGNPClass.Items[i];
                            if (kv.Value == name)
                                clbMainAGNPClass.SetItemChecked(i, true);
                        }
                    }
                }
            }

            if (_bAttachMap)
            {
                var adjAGNPPriorityItem = mapTypeItem.Element("AdjAGNPPriority");
                if (adjAGNPPriorityItem != null)
                {
                    string priority = adjAGNPPriorityItem.Attribute("priority").Value.ToString().Trim();

                    index = cbAdjAGNPPriority.Items.Count > 0 ? 0 : -1;
                    for (int i = 0; i < cbAdjAGNPPriority.Items.Count; ++i)
                    {
                        var kv = (KeyValuePair<ScaleRange, string>)cbAdjAGNPPriority.Items[i];
                        if (kv.Value == priority)
                            index = i;
                    }
                    cbAdjAGNPPriority.SelectedIndex = index;
                }

                var adjAGNPClassItem = mapTypeItem.Element("AdjAGNPClass");
                if (adjAGNPClassItem != null)
                {
                    for (int i = 0; i < clbAdjAGNPClass.Items.Count; ++i)
                    {
                        clbAdjAGNPClass.SetItemChecked(i, false);
                    }

                    var items = adjAGNPClassItem.Elements("Item");
                    foreach (XElement ele in items)
                    {
                        bool selState = bool.Parse(ele.Attribute("slected").Value);
                        string name = ele.Value;

                        if (selState)
                        {
                            for (int i = 0; i < clbAdjAGNPClass.Items.Count; ++i)
                            {
                                var kv = (KeyValuePair<string, string>)clbAdjAGNPClass.Items[i];
                                if (kv.Value == name)
                                    clbAdjAGNPClass.SetItemChecked(i, true);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < clbAdjAGNPClass.Items.Count; ++i)
                {
                    clbAdjAGNPClass.SetItemChecked(i, true);
                }
            }
            #endregion

            #region POI相关控件
            var poiItem = mapTypeItem.Element("POI");
            if (poiItem != null)
            {
                string mainPriority = poiItem.Attribute("priority").Value.ToString().Trim();
                string adjPriority = poiItem.Attribute("adjPriority").Value.ToString().Trim();

                index = cbMainPOIPriority.Items.Count > 0 ? 0 : -1;
                for (int i = 0; i < cbMainPOIPriority.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<ScaleRange, string>)cbMainPOIPriority.Items[i];
                    if (kv.Value == mainPriority)
                        index = i;
                }
                cbMainPOIPriority.SelectedIndex = index;

                index = cbAdjPOIPriority.Items.Count > 0 ? 0 : -1;
                if (_bAttachMap)
                {
                    for (int i = 0; i < cbAdjPOIPriority.Items.Count; ++i)
                    {
                        var kv = (KeyValuePair<ScaleRange, string>)cbAdjPOIPriority.Items[i];
                        if (kv.Value == adjPriority)
                            index = i;
                    }
                }
                cbAdjPOIPriority.SelectedIndex = index;
            }
            #endregion

            #region 植被面相关控件
            var vegaItem = mapTypeItem.Element("VEGA");
            if (vegaItem != null)
            {
                string mainArea = hydaItem.Attribute("mainArea").Value.ToString().Trim();
                string adjArea = hydaItem.Attribute("adjArea").Value.ToString().Trim();

                tbMainVEGAArea.Text = mainArea;
                if (_bAttachMap)
                {
                    tbAdjVEGAArea.Text = adjArea;
                }
                else
                {
                    tbAdjVEGAArea.Text = "0";
                }
            }
            #endregion

            #region 境界线相关控件
            var boulItem = mapTypeItem.Element("BOUL");
            if (boulItem != null)
            {
                string mainLevel = boulItem.Attribute("mainRegion").Value.ToString().Trim();
                string adjLevel = boulItem.Attribute("adjRegion").Value.ToString().Trim();

                index = cbMainBOULLevel.Items.Count > 0 ? 0 : -1;
                for (int i = 0; i < cbMainBOULLevel.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<string, string>)cbMainBOULLevel.Items[i];
                    if (kv.Value == mainLevel)
                        index = i;
                }
                cbMainBOULLevel.SelectedIndex = index;

                index = cbAdjBOULLevel.Items.Count > 0 ? 0 : -1;
                if (_bAttachMap)
                {
                    for (int i = 0; i < cbAdjBOULLevel.Items.Count; ++i)
                    {
                        var kv = (KeyValuePair<string, string>)cbAdjBOULLevel.Items[i];
                        if (kv.Value == adjLevel)
                            index = i;
                    }
                }
                cbAdjBOULLevel.SelectedIndex = index;
            }
            #endregion

            #region 表面注记相关控件
            var bouaAnnoItem = mapTypeItem.Element("BOUAAnno");
            if (bouaAnnoItem != null)
            {
                string mainLevel = bouaAnnoItem.Attribute("mainRegion").Value.ToString().Trim();
                string inProvLevel = bouaAnnoItem.Attribute("inProvAdjRegion").Value.ToString().Trim();
                string outProvLevel = bouaAnnoItem.Attribute("outProvAdjRegion").Value.ToString().Trim();

                index = cbMainBOUAAnnoLevel.Items.Count > 0 ? 0 : -1;
                for (int i = 0; i < cbMainBOUAAnnoLevel.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<string, string>)cbMainBOUAAnnoLevel.Items[i];
                    if (kv.Value == mainLevel)
                        index = i;
                }
                cbMainBOUAAnnoLevel.SelectedIndex = index;

                index = cbInProvBOUAAnnoLevel.Items.Count > 0 ? 0 : -1;
                if (_bAttachMap)
                {
                    for (int i = 0; i < cbInProvBOUAAnnoLevel.Items.Count; ++i)
                    {
                        var kv = (KeyValuePair<string, string>)cbInProvBOUAAnnoLevel.Items[i];
                        if (kv.Value == inProvLevel)
                            index = i;
                    }
                }
                cbInProvBOUAAnnoLevel.SelectedIndex = index;
                

                index = cbOutProvBOUAAnnoLevel.Items.Count > 0 ? 0 : -1;
                if (_bAttachMap)
                {
                    for (int i = 0; i < cbOutProvBOUAAnnoLevel.Items.Count; ++i)
                    {
                        var kv = (KeyValuePair<string, string>)cbOutProvBOUAAnnoLevel.Items[i];
                        if (kv.Value == outProvLevel)
                            index = i;
                    }
                }
                cbOutProvBOUAAnnoLevel.SelectedIndex = index;
            }
            #endregion
        }

        
    

        
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using System.Xml.Linq;
using System.IO;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class POISelectionFormEx : Form
    {
        public struct RuleInfo
        {
            public string FclName;//图层名称
            public int RuleID;// 
            public string RuleName;
            public double Ratio;
            public bool Selected;
            public double BufferVal;//居民点尺寸 mm
        }
        //要素不显示的ruleID
        public Dictionary<string, int> fclDisplayDic = new Dictionary<string, int>();
        private GApplication _app;
        public Dictionary<string, List<RuleInfo>> _fc2Rules;//要素类-规则集合:主区
        public Dictionary<string, List<RuleInfo>> _fc2RulesEx;//要素类-规则集合:附区

        private List<POISelection.POISelectionInfo> _poiSelectionInfoList;//被选择抽稀的图层及规则信息
        public List<POISelection.POISelectionInfo> POISelectionInfoList
        {
            get
            {
                return _poiSelectionInfoList;
            }
        }
        public List<POISelection.POISelectionInfo> POISelectionInfoListEx;//附区
        public string FilterFileName
        {
            get
            {
                return tbFilterFileName.Text;
            }
        }
        string selectFileName;

        public int WeightScale
        {
            get
            {
                return int.Parse(numWeightScale.Value.ToString());
            }
        }

        public POISelectionFormEx(GApplication app)
        {
            InitializeComponent();
            _app = app;
            _fc2RulesEx = new Dictionary<string, List<RuleInfo>>();
            _fc2Rules = new Dictionary<string, List<RuleInfo>>();
            _poiSelectionInfoList = new List<POISelection.POISelectionInfo>();
            POISelectionInfoListEx = new List<POISelection.POISelectionInfo>();
            LoadParas();
        }

        #region 事件

        private void cbLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            //更新控件
            updateSelectTable(cbLayers.Text);
        }

        private void cbOnlySelect_CheckedChanged(object sender, EventArgs e)
        {
            //更新控件
            updateSelectTable(cbLayers.Text);
        }

        private void dgSelectRule_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            DataGridView dg = sender as DataGridView;
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewCheckBoxCell checkboxCell = dg.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
            if (checkboxCell != null)
            {
                string fcName = dg.Rows[e.RowIndex].Cells["FeatureClass"].Value.ToString();
                string ruleName = dg.Rows[e.RowIndex].Cells["RuleName"].Value.ToString();
                if (dg.Columns[e.ColumnIndex].Name == "Selected")
                {
                    if (_fc2Rules.ContainsKey(fcName))
                    {
                        List<RuleInfo> riList = _fc2Rules[fcName];
                        for (int i = 0; i < riList.Count; ++i)
                        {
                            RuleInfo ri = riList[i];

                            if (ri.RuleName == ruleName)
                            {
                                ri.Selected = Convert.ToBoolean(checkboxCell.EditedFormattedValue);
                                riList[i] = ri;
                                break;
                            }
                        }
                    }
                }
                if (dg.Columns[e.ColumnIndex].Name == "SelectedEx")
                {
                    if (_fc2RulesEx.ContainsKey(fcName))
                    {
                        List<RuleInfo> riList = _fc2RulesEx[fcName];
                        for (int i = 0; i < riList.Count; ++i)
                        {
                            RuleInfo ri = riList[i];

                            if (ri.RuleName == ruleName)
                            {
                                ri.Selected = Convert.ToBoolean(checkboxCell.EditedFormattedValue);
                                riList[i] = ri;
                                break;
                            }
                        }
                    }
                }

            }
        }

        private void dgSelectRule_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            DataGridView dg = sender as DataGridView;
            if ("Scale" == dgSelectRule.Columns[e.ColumnIndex].Name)
            {
                string fcName = dg.Rows[e.RowIndex].Cells["FeatureClass"].Value.ToString();
                string ruleName = dg.Rows[e.RowIndex].Cells["RuleName"].Value.ToString();
                if (_fc2Rules.ContainsKey(fcName))
                {
                    List<RuleInfo> riList = _fc2Rules[fcName];
                    for (int i = 0; i < riList.Count; ++i)
                    {
                        RuleInfo ri = riList[i];

                        if (ri.RuleName == ruleName)
                        {
                            double ratio;
                            double.TryParse(dgSelectRule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out ratio);
                            ri.Ratio = ratio*0.01;
                            riList[i] = ri;
                            break;
                        }
                    }
                }
            }
        }

        private void dgSelectRule_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dg = sender as DataGridView;
            if (dg.Columns[e.ColumnIndex].Name != "Selected" && dg.Columns[e.ColumnIndex].Name != "SelectedEx")
                return;
            for (int i = 0; i < dgSelectRule.Rows.Count; ++i)
            {
                string fcName = dgSelectRule.Rows[i].Cells["FeatureClass"].Value.ToString();
                string ruleName = dgSelectRule.Rows[i].Cells["RuleName"].Value.ToString();

                if (dg.Columns[e.ColumnIndex].Name == "Selected")
                {
                    List<RuleInfo> riList = _fc2Rules[fcName];
                    for (int j = 0; j < riList.Count; ++j)
                    {
                        RuleInfo ri = riList[j];
                        if (ri.RuleName == ruleName)
                        {
                            ri.Selected = !ri.Selected;
                            riList[j] = ri;

                            ((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["Selected"]).Value = ri.Selected;
                            break;
                        }
                    }
                }
                if (dg.Columns[e.ColumnIndex].Name == "SelectedEx")
                {
                    List<RuleInfo> riList = _fc2RulesEx[fcName];
                    for (int j = 0; j < riList.Count; ++j)
                    {
                        RuleInfo ri = riList[j];
                        if (ri.RuleName == ruleName)
                        {
                            ri.Selected = !ri.Selected;
                            riList[j] = ri;

                            ((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["SelectedEx"]).Value = ri.Selected;
                            break;
                        }
                    }
                }
            }

        }

        private void btnFilterFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog pDialog = new OpenFileDialog();
            pDialog.AddExtension = true;
            pDialog.DefaultExt = "txt";
            pDialog.Filter = "文本文档（*txt）|*.txt";
            pDialog.FilterIndex = 0;
            if (pDialog.ShowDialog() == DialogResult.OK)
            {
                tbFilterFileName.Text = pDialog.FileName;
            }
        }

        public void btOK_Click(object sender, EventArgs e)
        {
            //重新更新_fc2Rules;
            for (int i = 0; i < dgSelectRule.Rows.Count; ++i)
            {
                //主区
                string fcName = dgSelectRule.Rows[i].Cells["FeatureClass"].Value.ToString();
                string ruleName = dgSelectRule.Rows[i].Cells["RuleName"].Value.ToString();

                List<RuleInfo> riList = _fc2Rules[fcName];
                for (int j = 0; j < riList.Count; ++j)
                {
                    RuleInfo ri = riList[j];
                    if (ri.RuleName == ruleName)
                    {
                        
                        riList[j] = ri;
                        ri.Selected = (bool)(((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["Selected"]).Value) ;
                        ri.Ratio =0.01* double.Parse(((dgSelectRule.Rows[i].Cells["Scale"]).Value.ToString()));
                        riList[j] = ri;
                        _fc2Rules[fcName] = riList;                       
                        break;
                    }
                }
                //邻区
                riList = _fc2RulesEx[fcName];
                for (int j = 0; j < riList.Count; ++j)
                {
                    RuleInfo ri = riList[j];
                    if (ri.RuleName == ruleName)
                    {
                        riList[j] = ri;
                        ri.Selected = (bool)(((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["SelectedEx"]).Value);
                        ri.Ratio = 0.01 * double.Parse(((dgSelectRule.Rows[i].Cells["ScaleEx"]).Value.ToString()));
                        riList[j] = ri;
                        _fc2RulesEx[fcName] = riList;
                        break;
                    }
                }
            }


            foreach(var item in _fc2Rules)
            {
                string fcName = item.Key;
                List<RuleInfo> riList = item.Value;

                foreach (var ri in riList)
                {
                    if (ri.Selected)
                    {
                        string ruleName = ri.RuleName;
                        double ratio = ri.Ratio;

                        _poiSelectionInfoList.Add(new POISelection.POISelectionInfo { BufferVal=ri.BufferVal, RuleID=ri.RuleID, FCName = fcName, RuleName = ruleName, Ratio = ratio });
                    }
                }
            }

            foreach (var item in _fc2RulesEx)
            {
                string fcName = item.Key;
                List<RuleInfo> riList = item.Value;

                foreach (var ri in riList)
                {
                    if (ri.Selected)
                    {
                        string ruleName = ri.RuleName;
                        double ratio = ri.Ratio;

                        POISelectionInfoListEx.Add(new POISelection.POISelectionInfo { BufferVal = ri.BufferVal, RuleID = ri.RuleID, FCName = fcName, RuleName = ruleName, Ratio = ratio });
                    }
                }
            }
        
            DialogResult = DialogResult.OK;
        }
        #endregion 

        #region 方法
        private void LoadParas()
        {
            var lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint;
            })).ToArray();

            XElement expertiseContent = ExpertiseDatabase.getContentElement(_app);
            XElement poiSelection = expertiseContent.Element("POISelection");
            var items = poiSelection.Elements("Item");
            #region 初始化主区规则
            //初始化成员_fc2Rules
            foreach (var layer in lyrs)
            {
                IFeatureLayer featureLayer = layer as IFeatureLayer;

                if (featureLayer.FeatureClass.FindField("RuleID") == -1 || featureLayer.FeatureClass.FindField("ATTACH") == -1)
                    continue;

                string fcName = featureLayer.FeatureClass.AliasName;
                List<RuleInfo> rules = new List<RuleInfo>();
                Dictionary<int, double> symSizeDic;
                var ruleNamesDic = GetRuleNames(featureLayer.FeatureClass, out symSizeDic);
                foreach (var rn in ruleNamesDic)
                {
                    double ratio = 0.6;
                    foreach (XElement item in items)
                    {
                        if (fcName == item.Element("FeatureClassName").Value && rn.Key.Trim() == item.Element("RuleName").Value.Trim())
                        {
                            ratio = double.Parse(item.Element("Ratio").Value);
                        }
                    }

                    rules.Add(new RuleInfo { BufferVal = symSizeDic[rn.Value], RuleID = rn.Value, RuleName = rn.Key, Ratio = ratio, Selected = false });
                }

                if (rules.Count == 0)
                    continue;
                if(!_fc2Rules.ContainsKey(fcName))
                    _fc2Rules.Add(fcName, rules);
            }
            #endregion

            #region 初始化附区规则
            poiSelection = expertiseContent.Element("POISelectionEx");
            items = poiSelection.Elements("Item");
            foreach (var layer in lyrs)
            {
                IFeatureLayer featureLayer = layer as IFeatureLayer;

                if (featureLayer.FeatureClass.FindField("RuleID") == -1 || featureLayer.FeatureClass.FindField("ATTACH") == -1)
                    continue;

                string fcName = featureLayer.FeatureClass.AliasName;
                List<RuleInfo> rules = new List<RuleInfo>();

                Dictionary<int, double> symSizeDic;
                var ruleNamesDic = GetRuleNames(featureLayer.FeatureClass, out symSizeDic);
                foreach (var rn in ruleNamesDic)
                {
                    double ratio = 0.3;
                    foreach (XElement item in items)
                    {
                        if (fcName == item.Element("FeatureClassName").Value && rn.Key.Trim() == item.Element("RuleName").Value.Trim())
                        {
                            ratio = double.Parse(item.Element("Ratio").Value);
                        }
                    }

                    rules.Add(new RuleInfo { BufferVal = symSizeDic[rn.Value], RuleID = rn.Value, RuleName = rn.Key, Ratio = ratio, Selected = false });
                }

                if (rules.Count == 0)
                    continue;

                if(!_fc2RulesEx.ContainsKey(fcName))
                    _fc2RulesEx.Add(fcName, rules);
            }
            #endregion
            //初始化界面
            foreach (var fcName in _fc2Rules.Keys)
            {
                cbLayers.Items.Add(fcName);
            }
            int index = cbLayers.Items.Add("所有图层");
            cbLayers.SelectedIndex = index;
        }

        public void SetAutoParas()
        {
            foreach (var item in _fc2Rules)
            {
                string fcName = item.Key;
                List<RuleInfo> riList = item.Value;

                foreach (var ri in riList)
                {
                    if (ri.Selected)
                    {
                        string ruleName = ri.RuleName;
                        double ratio = ri.Ratio;

                        _poiSelectionInfoList.Add(new POISelection.POISelectionInfo { BufferVal = ri.BufferVal, RuleID = ri.RuleID, FCName = fcName, RuleName = ruleName, Ratio = ratio });
                    }
                }
            }
            //附区
            foreach (var item in _fc2RulesEx)
            {
                string fcName = item.Key;
                List<RuleInfo> riList = item.Value;

                foreach (var ri in riList)
                {
                    if (ri.Selected)
                    {
                        string ruleName = ri.RuleName;
                        double ratio = ri.Ratio;

                        POISelectionInfoListEx.Add(new POISelection.POISelectionInfo { BufferVal = ri.BufferVal, RuleID = ri.RuleID, FCName = fcName, RuleName = ruleName, Ratio = ratio });
                    }
                }
            }
        }

        private Dictionary<string,int> GetRuleNames(IFeatureClass fc,out Dictionary<int,double> sizeDic)
        {
            var ruleDic = new Dictionary<string, int>();
            sizeDic = new Dictionary<int, double>();
            IRepresentationClass rpc = OpenRepClass(fc);
            if (rpc == null)
            {
                ruleDic.Add("全部要素", 0);
                sizeDic[0] = 0;
                return ruleDic;
            }

            var rules = rpc.RepresentationRules;
            rules.Reset();
            int id;
            IRepresentationRule rule;
            rules.Next(out id, out rule);
            while (rule != null)
            {
                if (!rules.get_Name(id).Contains("不显示要素"))
                {
                    ruleDic.Add(rules.get_Name(id), id);
                    //获取居民点的尺寸  mm
                    IBasicSymbol pBasicSymbol = rule.Layer[0];//取一个图层
                    if (pBasicSymbol is IBasicMarkerSymbol)
                    {
                        IGraphicAttributes graphicAttributes = pBasicSymbol as IGraphicAttributes;
                        double ptBufferVal = Convert.ToDouble(graphicAttributes.Value[2]) / 2.8345;//磅转毫米(考虑到与其冲突的符号也有大小，这里直接用其直径)
                        sizeDic[id] = ptBufferVal;
                    }
                }
                else
                {
                    fclDisplayDic[fc.AliasName] = id;
                }

                rules.Next(out id, out rule);
            }
            return ruleDic;
        }

        private IRepresentationClass OpenRepClass(IFeatureClass fc)
        {
            if (fc == null)
                return null;

            try
            {
                IDataset pDs = fc as IDataset;
                IWorkspace pWorkspace = pDs.Workspace;
                IRepresentationWorkspaceExtension pRepWSExt;
                IWorkspaceExtensionManager pExtManager = pWorkspace as IWorkspaceExtensionManager;
                UID pUID = new UID();
                pUID.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
                IWorkspaceExtension pWksExt = pExtManager.FindExtension(pUID);
                pRepWSExt = pWksExt as IRepresentationWorkspaceExtension;
                string featClsName = pDs.Name;
                string strRepClassName = featClsName;

                bool bHasRepClass = pRepWSExt.get_FeatureClassHasRepresentations(fc);
                IRepresentationClass pRepClass = null;
                if (bHasRepClass)
                {
                    pRepClass = pRepWSExt.OpenRepresentationClass(strRepClassName);
                }
                return pRepClass;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null;
            }
        }

        private void updateSelectTable(string fcName)
        {
            dgSelectRule.Rows.Clear();
            
            if (fcName == "所有图层")
            {
                foreach (var item in _fc2Rules)
                {
                    var itemEx = _fc2RulesEx[item.Key];
                    addRows(item.Key, item.Value, itemEx);
                }
                return;
            }
            else if (_fc2Rules.ContainsKey(fcName))
            {
                var itemEx = _fc2RulesEx[fcName];
                addRows(fcName, _fc2Rules[fcName], itemEx);
                //其他图层:不处理
                for (int i = 0; i < _fc2Rules.Keys.ToArray().Length; i++)
                {
                    string key = _fc2Rules.Keys.ToArray()[i];
                    if (key == fcName)
                        continue;
                    List<RuleInfo> riList = _fc2Rules[key];
                    for (int j = 0; j < riList.Count; ++j)
                    {
                        RuleInfo ri = riList[j];
                        ri.Selected = false;
                        riList[j] = ri;
                    }
                    _fc2Rules[key] = riList;
                }
                //其他图层:不处理
                for (int i = 0; i < _fc2RulesEx.Keys.ToArray().Length; i++)
                {
                    string key = _fc2RulesEx.Keys.ToArray()[i];
                    if (key == fcName)
                        continue;
                    List<RuleInfo> riList = _fc2Rules[key];
                    for (int j = 0; j < riList.Count; ++j)
                    {
                        RuleInfo ri = riList[j];
                        ri.Selected = false;
                        riList[j] = ri;
                    }
                    _fc2RulesEx[key] = riList;
                }
                return;
            }

            
        }

        private void addRows(string fcName, List<RuleInfo> riList, List<RuleInfo> riListEx)
        {
            bool bOnlySelected = cbOnlySelect.Checked;
            for(int i=0;i<riList.Count;i++)
            {
                var fi = riList[i];
                var fiEx=riListEx[i];
                if (bOnlySelected && (!fi.Selected&&!fiEx.Selected))
                {
                    continue;
                }

                int rowIndex = dgSelectRule.Rows.Add();

                ((DataGridViewCheckBoxCell)dgSelectRule.Rows[rowIndex].Cells["Selected"]).Value = fi.Selected;
                dgSelectRule.Rows[rowIndex].Cells["FeatureClass"].Value = fcName;
                dgSelectRule.Rows[rowIndex].Cells["RuleName"].Value = fi.RuleName;
                dgSelectRule.Rows[rowIndex].Cells["Scale"].Value = (fi.Ratio*100).ToString();

                dgSelectRule.Rows[rowIndex].Cells["ScaleEx"].Value = (fiEx.Ratio*100).ToString();
                dgSelectRule.Rows[rowIndex].Cells["SelectedEx"].Value = fiEx.Selected;
            }
        }
    
        #endregion

        

       
        private void POISelectionFormEx_Load(object sender, EventArgs e)
        {

        }

        private void btnBrowseSelectFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "选取文件|*.xml";
            of.FileName = "POISelectRule";
            of.InitialDirectory = GApplication.Application.Template.Root + @"\专家库\POI选取";
            DialogResult dr = of.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            tbBrowseSelectFileName.Text = of.FileName;
            string XmlPath = tbBrowseSelectFileName.Text;
            LoadOutParams(XmlPath);
        }

        private void btnSaveSelectFileName_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "选取文件|*.xml";
            sf.FileName = "POISelectRule";
            sf.InitialDirectory = GApplication.Application.Template.Root + @"\专家库\POI选取";
            DialogResult dr = sf.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            tbSaveSelectFileName.Text = sf.FileName;
            string XmlPath = tbSaveSelectFileName.Text;
            bool exp = ExportXML(XmlPath);
            if (exp)
            {
                MessageBox.Show("成功导出选取文件!", "提示");
            }
            else
            {
                MessageBox.Show("导出选取文件失败!", "提示");
            }
        }

        private bool ExportXML(string fileName)
        {



            try
            {
                FileInfo f = new FileInfo(fileName);
                XDocument doc = new XDocument();
                doc.Declaration = new XDeclaration("1.0", "utf-8", "");
                var root = new XElement("POISelectRule");
                doc.Add(root);
                //图例配置信息
                var content = new XElement("POISelectInfo");
                content.SetAttributeValue("TYPE", "主区");
                root.Add(content);
                foreach (DataGridViewRow row in dgSelectRule.Rows)
                {
                    // if (node.Checked)
                    {

                        var lyr = new XElement("Layer");
                        lyr.SetAttributeValue("LayerName", row.Cells[0].Value.ToString());//要素类
                        lyr.SetAttributeValue("Rule", row.Cells[1].Value.ToString());//规则
                        lyr.SetAttributeValue("Percent",double.Parse(row.Cells[2].Value.ToString())*0.01);//选取比例
                        lyr.SetAttributeValue("Checked", row.Cells[3].Value.ToString());//选择状态
                        content.Add(lyr);
                    }

                }



           
                //图例配置信息
                var content2 = new XElement("POISelectInfoEx");
                content2.SetAttributeValue("TYPE", "附区");
                root.Add(content2);
                foreach (DataGridViewRow row in dgSelectRule.Rows)
                {
                    // if (node.Checked)
                    {

                        var lyr = new XElement("Layer");
                        lyr.SetAttributeValue("LayerName", row.Cells[0].Value.ToString());//要素类
                        lyr.SetAttributeValue("Rule", row.Cells[1].Value.ToString());//规则
                        lyr.SetAttributeValue("Percent", double.Parse(row.Cells[4].Value.ToString()) * 0.01);//选取比例
                        lyr.SetAttributeValue("Checked", row.Cells[5].Value.ToString());//选择状态
                        content2.Add(lyr);
                    }

                }


                doc.Save(fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.ToString());
                return false;
            }
            return true;
        }
        /// <summary>
        /// 加载外部参数
        /// </summary>
        /// <param name="fileName">XML文件全路径</param>
        public void LoadOutParams(string fileName)
        {
            try
            {
                try
                {
                    XDocument doc = XDocument.Load(fileName);
                  
                    {
                       

                        //主区
                        var content = doc.Element("POISelectRule").Element("POISelectInfo");
                        dgSelectRule.Rows.Clear();
                        var POIItems = content.Elements("Layer");
                        if (!cbLayers.Items.Contains("所有图层"))
                        {
                            int index = cbLayers.Items.Add("所有图层");
                            cbLayers.SelectedIndex = index;
                        }
                        dgSelectRule.Rows.Clear();
                        List<RuleInfo> rules = new List<RuleInfo>();
                        foreach (XElement ele in POIItems)
                        {
                            string LayerName = ele.Attribute("LayerName").Value;
                            string Rule = ele.Attribute("Rule").Value;
                            string Percent = ele.Attribute("Percent").Value;
                            string TempChecked = ele.Attribute("Checked").Value;
                            bool Checked;
                            if (TempChecked.Trim().ToUpper() == "TRUE")
                                Checked = true;
                            else
                                Checked = false;

                            var ruleItem = new RuleInfo();
                            ruleItem.RuleName = Rule;
                            ruleItem.FclName = LayerName;
                            ruleItem.Ratio =double.Parse(Percent);
                            ruleItem.Selected = Checked;
                            rules.Add(ruleItem);
                        }

                       


                        //附区
                        List<RuleInfo> rulesEx = new List<RuleInfo>();
                        var contentEx = doc.Element("POISelectRule").Element("POISelectInfoEx");
                        var POIItemsEx = contentEx.Elements("Layer");
                      
                        foreach (XElement ele in POIItemsEx)
                        {
                            string LayerName = ele.Attribute("LayerName").Value;
                            string Rule = ele.Attribute("Rule").Value;
                            string Percent = ele.Attribute("Percent").Value;
                            string TempChecked = ele.Attribute("Checked").Value;
                            bool Checked;
                            if (TempChecked.Trim().ToUpper() == "TRUE")
                                Checked = true;
                            else
                                Checked = false;
                            var ruleItem = new RuleInfo();
                            ruleItem.RuleName = Rule;
                            ruleItem.FclName = LayerName;
                            ruleItem.Ratio = double.Parse(Percent);
                            ruleItem.Selected = Checked;
                            rulesEx.Add(ruleItem);

                        }
                        for(int i=0;i<rules.Count;i++)
                        {
                            int rowIndex = dgSelectRule.Rows.Add();

                            ((DataGridViewCheckBoxCell)dgSelectRule.Rows[rowIndex].Cells["Selected"]).Value =rules[i].Selected;
                            dgSelectRule.Rows[rowIndex].Cells["FeatureClass"].Value = rules[i].FclName;
                            dgSelectRule.Rows[rowIndex].Cells["RuleName"].Value = rules[i].RuleName;
                            dgSelectRule.Rows[rowIndex].Cells["Scale"].Value =(rules[i].Ratio*100).ToString();

                            dgSelectRule.Rows[rowIndex].Cells["ScaleEx"].Value = (rulesEx[i].Ratio*100).ToString();
                            dgSelectRule.Rows[rowIndex].Cells["SelectedEx"].Value = rulesEx[i].Selected;
                        }
                    }
                }
                catch
                {

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.ToString());
            }
        }
        
    }
}

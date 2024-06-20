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
    public partial class POISelectionForm : Form
    {
        public struct RuleInfo
        {
            public string FclName;//图层名称
            public int RuleID;// 
            public string RuleName;
            public double Ratio;
            public bool Selected;
            public double BufferVal;
        }
        //要素不显示的ruleID
        public Dictionary<string, int> fclDisplayDic = new Dictionary<string, int>();
        private GApplication _app;
        public Dictionary<string, List<RuleInfo>> _fc2Rules;//要素类-规则集合

        private List<POISelection.POISelectionInfo> _poiSelectionInfoList;//被选择抽稀的图层及规则信息
        public List<POISelection.POISelectionInfo> POISelectionInfoList
        {
            get
            {
                return _poiSelectionInfoList;
            }
        }

        public string FilterFileName
        {
            get
            {
                return tbFilterFileName.Text;
            }
        }

        public int WeightScale
        {
            get
            {
                return int.Parse(numWeightScale.Value.ToString());
            }
        }

        public POISelectionForm(GApplication app)
        {
            InitializeComponent();
            _app = app;
            _fc2Rules = new Dictionary<string, List<RuleInfo>>();
            _poiSelectionInfoList = new List<POISelection.POISelectionInfo>();
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
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewCheckBoxCell checkboxCell = dgSelectRule.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
            if (checkboxCell != null)
            {
                string fcName = dgSelectRule.Rows[e.RowIndex].Cells["FeatureClass"].Value.ToString();
                string ruleName = dgSelectRule.Rows[e.RowIndex].Cells["RuleName"].Value.ToString();
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
        }

        private void dgSelectRule_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if ("Scale" == dgSelectRule.Columns[e.ColumnIndex].Name)
            {
                string fcName = dgSelectRule.Rows[e.RowIndex].Cells["FeatureClass"].Value.ToString();
                string ruleName = dgSelectRule.Rows[e.RowIndex].Cells["RuleName"].Value.ToString();
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
                            ri.Ratio = ratio;
                            riList[i] = ri;
                            break;
                        }
                    }
                }
            }
        }

        private void dgSelectRule_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            for (int i = 0; i < dgSelectRule.Rows.Count; ++i)
            {
                string fcName = dgSelectRule.Rows[i].Cells["FeatureClass"].Value.ToString();
                string ruleName = dgSelectRule.Rows[i].Cells["RuleName"].Value.ToString();

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
                string fcName = dgSelectRule.Rows[i].Cells["FeatureClass"].Value.ToString();
                string ruleName = dgSelectRule.Rows[i].Cells["RuleName"].Value.ToString();

                List<RuleInfo> riList = _fc2Rules[fcName];
                for (int j = 0; j < riList.Count; ++j)
                {
                    RuleInfo ri = riList[j];
                    if (ri.RuleName == ruleName)
                    {

                        riList[j] = ri;
                        ri.Selected = (bool)(((DataGridViewCheckBoxCell)dgSelectRule.Rows[i].Cells["Selected"]).Value);
                        ri.Ratio = double.Parse(((dgSelectRule.Rows[i].Cells["Scale"]).Value.ToString()));
                        riList[j] = ri;
                        _fc2Rules[fcName] = riList;
                        break;
                    }
                }
            }


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

                    rules.Add(new RuleInfo { BufferVal = symSizeDic[rn.Value], RuleID = rn.Value, RuleName = rn.Key, Ratio = ratio, Selected = true });
                }

                if (rules.Count == 0)
                    continue;

                //_fc2Rules.Add(fcName, rules);

                if (!_fc2Rules.ContainsKey(fcName))
                    _fc2Rules.Add(fcName, rules);
            }

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
        }

        private Dictionary<string, int> GetRuleNames(IFeatureClass fc, out Dictionary<int, double> sizeDic)
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
                        if (fc.AliasName.ToUpper() == "HFCP")
                            sizeDic[id] = 0.1;
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

        //private Dictionary<string,int> GetRuleNames(IFeatureClass fc)
        //{
        //    var ruleDic = new Dictionary<string, int>();

        //    IRepresentationClass rpc = OpenRepClass(fc);
        //    if (rpc == null)
        //    {
        //        ruleDic.Add("全部要素", 0);
        //        return ruleDic;
        //    }

        //    var rules = rpc.RepresentationRules;
        //    rules.Reset();
        //    int id;
        //    IRepresentationRule rule;
        //    rules.Next(out id, out rule);
        //    while (rule != null)
        //    {
        //        if (!rules.get_Name(id).Contains("不显示要素"))
        //        {
        //            ruleDic.Add(rules.get_Name(id), id);
        //        }
        //        else
        //        {
        //            fclDisplayDic[fc.AliasName] = id;
        //        }
        //        rules.Next(out id, out rule);
        //    }
        //    return ruleDic;
        //}

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
                    addRows(item.Key, item.Value);
                }

                return;
            }
            else if (_fc2Rules.ContainsKey(fcName))
            {
                addRows(fcName, _fc2Rules[fcName]);
                //其他图层:不处理
                for(int i=0;i<_fc2Rules.Keys.ToArray().Length;i++)
                {
                    string key = _fc2Rules.Keys.ToArray()[i];
                    if(key==fcName)
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
                return;
            }

            
        }

        private void addRows(string fcName, List<RuleInfo> riList)
        {
            bool bOnlySelected = cbOnlySelect.Checked;

            foreach (var fi in riList)
            {
                if (bOnlySelected && !fi.Selected)
                {
                    continue;
                }

                int rowIndex = dgSelectRule.Rows.Add();

                ((DataGridViewCheckBoxCell)dgSelectRule.Rows[rowIndex].Cells["Selected"]).Value = fi.Selected;
                dgSelectRule.Rows[rowIndex].Cells["FeatureClass"].Value = fcName;
                dgSelectRule.Rows[rowIndex].Cells["RuleName"].Value = fi.RuleName;
                dgSelectRule.Rows[rowIndex].Cells["Scale"].Value = fi.Ratio.ToString();
            }
        }
        #endregion

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
        /// <summary>
        /// 加载外部参数
        /// </summary>
        /// <param name="fileName">XML文件全路径</param>
        public void LoadOutParams(string fileName)
        {
            try
            {
                XDocument doc = XDocument.Load(fileName);
               
                {
                    

                    //只有主区，没附区
                    var content = doc.Element("POISelectRule").Element("POISelectInfo");
                    dgSelectRule.Rows.Clear();
                    var POIItems = content.Elements("Layer");
                    if (!cbLayers.Items.Contains("所有图层"))
                    {
                        int index = cbLayers.Items.Add("所有图层");
                        cbLayers.SelectedIndex = index;
                    }
                    dgSelectRule.Rows.Clear();
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
                        //初始化界面
                        int rowIndex = dgSelectRule.Rows.Add();
                        ((DataGridViewCheckBoxCell)dgSelectRule.Rows[rowIndex].Cells["Selected"]).Value = Checked;
                        dgSelectRule.Rows[rowIndex].Cells["FeatureClass"].Value = LayerName;
                        dgSelectRule.Rows[rowIndex].Cells["RuleName"].Value = Rule;
                        dgSelectRule.Rows[rowIndex].Cells["Scale"].Value = Percent;

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
         bool exp=ExportXML(XmlPath);
         if (exp)
         {
             MessageBox.Show("成功导出选取文件!","提示");
         }
         else
         {
             MessageBox.Show("导出选取文件失败!","提示");
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
                        lyr.SetAttributeValue("Percent", row.Cells[2].Value.ToString());//选取比例
                        lyr.SetAttributeValue("Checked", row.Cells[3].Value.ToString());//选择状态
                        content.Add(lyr);
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

        private void POISelectionForm_Load(object sender, EventArgs e)
        {

        }

        
         
    
        
    }
}

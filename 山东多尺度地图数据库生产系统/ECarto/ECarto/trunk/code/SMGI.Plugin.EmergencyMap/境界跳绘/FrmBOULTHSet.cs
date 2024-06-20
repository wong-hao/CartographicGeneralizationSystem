using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using System.Xml.Linq;
using SMGI.Common;
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmBOULTHSet : Form
    {
        public double tolerance;
        public Dictionary<string,string> layerList; 
        GApplication app;
        public Dictionary<string, BoulDrawInfo> BoulDrawDic = new Dictionary<string, BoulDrawInfo>();
        string type = string.Empty;
        public FrmBOULTHSet(GApplication app_,string type_="")
        {
            InitializeComponent();
            app = app_;
            type = type_;
            LoadParas();
            SetAutoParas();
        }
        /// <summary>
        /// 加载外部参数
        /// </summary>
        /// <param name="fileName">XML文件全路径</param>
        private void LoadOutParams(string fileName)
        {
            BoulDrawDic.Clear();
            XDocument doc = XDocument.Load(fileName);
          
            {
              
                var content = doc.Element("Expertise").Element("Content");
                //容差
                string tolerance = content.Element("BufferValue").Value;
                txtTolerance.Text = tolerance;
                //图层
                foreach (var ele in content.Element("ObjectLayer").Elements("Item"))
                {
                    int rowIndex = dgLyrs.Rows.Add();

                    ((DataGridViewCheckBoxCell)dgLyrs.Rows[rowIndex].Cells["Selected"]).Value = true;
                    dgLyrs.Rows[rowIndex].Cells["Lyr"].Value = ele.Value;
                   
                    if (ele.Attribute("SQL") != null)
                    {
                        dgLyrs.Rows[rowIndex].Cells["SQL"].Value = (ele.Attribute("SQL").Value);
                    }
                    else
                    {
                        dgLyrs.Rows[rowIndex].Cells["SQL"].Value ="";
                    }
                    if (ele.Attribute("Note") != null)
                    {
                        dgLyrs.Rows[rowIndex].Cells["Note"].Value = (ele.Attribute("Note").Value);
                    }
                    else
                    {
                        dgLyrs.Rows[rowIndex].Cells["Note"].Value=("全部要素...");
                    }
                   
                  
                }
                //境界
                foreach (var ele in content.Element("Boul").Elements("Item"))
                {
                    #region
                    var blankwidth = ele.Element("BlankValue").Value;
                    var linewidth = ele.Element("SolidValue").Value;
                    var pointstep = ele.Element("PointStep").Value;
                    var groups = ele.Element("SymbolGroup").Value;
                    string gbStr = ele.Element("GB").Value.ToString();
                    string[] gbStrList = gbStr.Split('、');
                    List<int> gbList = new List<int>();
                    foreach (var item in gbStrList)
                    {
                        gbList.Add(int.Parse(item.Trim()));
                    }
                    var info = new BoulDrawInfo
                    {
                        Level = ele.Element("Level").Value,
                        BlankValue = double.Parse(blankwidth),
                        SolidValue = double.Parse(linewidth),
                        PointStep = double.Parse(pointstep),
                        GBList = gbList,
                        Checked=true,
                        SymbolGroup = double.Parse(groups)
                    };
                    BoulDrawDic[info.Level] = info;
                    switch (ele.Element("Level").Value)
                    {
                        case "Province":
                            blankWidthProvince.Text = blankwidth;
                            lineWidthProvince.Text = linewidth;
                            groupsProvince.Text = groups;
                            break;
                        case "State":
                            blankWidthState.Text = blankwidth;
                            lineWidthState.Text = linewidth;
                            groupsState.Text = groups;
                            break;
                        case "County":
                            blankWidthCounty.Text = blankwidth;
                            lineWidthCounty.Text = linewidth;
                            groupsCounty.Text = groups;
                            break;
                        case "Town":
                            blankWidthTown.Text = blankwidth;
                            lineWidthTown.Text = linewidth;
                            groupsTown.Text = groups;
                            break;
                        default:
                            break;
                    }
                    #endregion
                }

            }
        }
        private bool CheckValid(BoulDrawInfo info)
        {

            if (info.BlankValue <= 0 || info.SolidValue <= 0 || info.SymbolGroup <= 0)
            {
                MessageBox.Show("输入参数不合法！请输入大于0的数字");
                return false;
            }
            return true;
        }
        private bool CheckPrams()
        {
            int rowNum = 0;
            for (int i = 0; i < dgLyrs.Rows.Count; ++i)
            {
                bool val = bool.Parse(dgLyrs.Rows[i].Cells[0].Value.ToString());
                if (val)
                    rowNum++;
            }

            if (rowNum == 0)
            {
            
                MessageBox.Show("请选择跳绘制图层！");
                return false;
            }
            //省
            var info = BoulDrawDic["Province"];
            double r = 0;
            double.TryParse(blankWidthProvince.Text, out r); info.BlankValue = r;
            double.TryParse(lineWidthProvince.Text, out r); info.SolidValue = r;
            double.TryParse(groupsProvince.Text, out r); info.SymbolGroup = r;
            info.Checked = cbProvince.Checked;
            if (!CheckValid(info)) return false;
            //市
            info = BoulDrawDic["State"];
            double.TryParse(blankWidthState.Text, out r);info.BlankValue=r;
            double.TryParse(lineWidthState.Text, out r); info.SolidValue = r;
            double.TryParse(groupsState.Text, out r); info.SymbolGroup = r;
            info.Checked = cbState.Checked;
            if (!CheckValid(info)) return false;
            //县
            info = BoulDrawDic["County"];
            info.Checked = cbCounty.Checked;
            double.TryParse(blankWidthCounty.Text, out r); info.BlankValue = r;
            double.TryParse(lineWidthCounty.Text, out r); info.SolidValue = r;
            double.TryParse(groupsCounty.Text, out r); info.SymbolGroup = r;
            if (!CheckValid(info)) return false;
            //镇
            info = BoulDrawDic["Town"];
            info.Checked = cbTown.Checked;
            double.TryParse(blankWidthTown.Text, out r); info.BlankValue = r;
            double.TryParse(lineWidthTown.Text, out r); info.SolidValue = r;
            double.TryParse(groupsTown.Text, out r); info.SymbolGroup = r;
            if (!CheckValid(info)) return false;

            return true;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            ValidateUtil valid = new ValidateUtil();
            if (!valid.TraversalTextBox(this.Controls))
            {
                return;
            }
            if (!CheckPrams())
                return;
            
            tolerance=0;
            double.TryParse( txtTolerance.Text,out tolerance);
            layerList = new Dictionary<string,string>();
           
            for (int i = 0; i < dgLyrs.Rows.Count; ++i)
            {
                bool val = bool.Parse(dgLyrs.Rows[i].Cells[0].Value.ToString());
                if (val)
                {
                    layerList.Add(dgLyrs.Rows[i].Cells[1].Value.ToString(), dgLyrs.Rows[i].Cells[2].Value.ToString());
                }
            }
            if (type == "OneKey")
            {
                DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
            
            string err = "";
            string ruleMatchFileName = EnvironmentSettings.getLayerRuleDBFileName(GApplication.Application);
            DataTable  _dtLayerRule =Helper.ReadToDataTable(ruleMatchFileName, "图层对照规则");
            using (var wo = app.SetBusy())
            {
                this.Hide();

                if(type!=string.Empty)
                {
                     var bs = new BoulSkipDrawHelper(app, BoulDrawDic, tolerance, layerList);
                     bs.ProvinceSkip = cbProvince.Checked;
                     bs.StateSkip = cbState.Checked;
                     bs.CountySkip = cbCounty.Checked;
                     bs.TownSkip = cbTown.Checked;
                     bs._dtLayerRule = _dtLayerRule;
                     err = bs.boulProduce(wo);
                }
                else 
                {
                    var bp = new BoulAutoProduceHelper(app, BoulDrawDic, tolerance, layerList.Keys.ToList());
                    bp._dtLayerRule = _dtLayerRule;
                    err = bp.boulProduce(wo);
                }
            }
             
            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err);
            }
            else
            {
                SavePramas();
                 
                MessageBox.Show("跳绘完成！");
            }

            this.Close();
        }
        

        private void btView_Click(object sender, EventArgs e)
        {
      
            string err = "";

            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err);
            }
            else
            {
                SavePramas();
                MessageBox.Show("跳绘完成！");
            }

        
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmBOULTHSet_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            string path = GApplication.Application.Template.Root + @"\专家库\境界跳绘\";
            path += "境界跳绘制帮助.htm";
            System.Diagnostics.Process.Start(path);
        }

        //保存参数到经验库xml
        private void SavePramas()
        {
            var boulDraw = new XElement("BoulDraw");
            var bufferdis = new XElement("BufferValue");
            bufferdis.Value = txtTolerance.Text;
            boulDraw.Add(bufferdis);

            var lyr = new XElement("ObjectLayer");
            for (int i = 0; i < dgLyrs.Rows.Count; i++)
            {

                string lyrName = dgLyrs.Rows[i].Cells["Lyr"].Value.ToString();
                var lyritem = new XElement("Item");
                lyritem.Value = lyrName;
                bool check = bool.Parse(dgLyrs.Rows[i].Cells["Selected"].Value.ToString());
                lyritem.SetAttributeValue("checked", check);
                string sql = dgLyrs.Rows[i].Cells["SQL"].Value.ToString();
                lyritem.SetAttributeValue("SQL", sql);
                string note = dgLyrs.Rows[i].Cells["Note"].Value.ToString();
                lyritem.SetAttributeValue("Note", note);
                lyr.Add(lyritem);
            }
            boulDraw.Add(lyr);
       
            var boul = new XElement("Boul");
            boulDraw.Add(boul);
            #region
            //省
            var  item = new XElement("Item");
            var  level = new XElement("Level", "Province");
            level.SetAttributeValue("name", "省");
            item.Add(level);
            item.Add(new XElement("PointStep", BoulDrawDic["Province"].PointStep));
            List<int> gbList = BoulDrawDic["Province"].GBList;
            string gbStr = "";
            for (int i = 0; i < gbList.Count; ++i)
            {
                if (i == gbList.Count - 1)
                {
                    gbStr += gbList[i].ToString();
                }
                else
                {
                    gbStr += gbList[i].ToString() + "、";
                }
            }
            item.Add(new XElement("GB", gbStr));
            item.Add(new XElement("BlankValue", blankWidthProvince.Text));
            item.Add(new XElement("SolidValue", lineWidthProvince.Text));
            item.Add(new XElement("SymbolGroup", groupsProvince.Text));
            boul.Add(item);
            //市
            item = new XElement("Item");
            level = new XElement("Level","State");
            level.SetAttributeValue("name", "市");
            item.Add(level);
            item.Add(new XElement("PointStep", BoulDrawDic["State"].PointStep));
            gbList = BoulDrawDic["State"].GBList;
            gbStr = "";
            for (int i = 0; i < gbList.Count; ++i)
            {
                if (i == gbList.Count - 1)
                {
                    gbStr += gbList[i].ToString();
                }
                else
                {
                    gbStr += gbList[i].ToString() + "、";
                }
            }
            item.Add(new XElement("GB", gbStr));
            item.Add(new XElement("BlankValue", blankWidthState.Text));
            item.Add(new XElement("SolidValue", lineWidthState.Text));
            item.Add(new XElement("SymbolGroup", groupsState.Text));
            boul.Add(item);
            //县
            item = new XElement("Item");
            level = new XElement("Level", "County");
            level.SetAttributeValue("name", "县");
            item.Add(level);
            item.Add(new XElement("PointStep", BoulDrawDic["County"].PointStep));
            gbList = BoulDrawDic["County"].GBList;
            gbStr = "";
            for (int i = 0; i < gbList.Count; ++i)
            {
                if (i == gbList.Count - 1)
                {
                    gbStr += gbList[i].ToString();
                }
                else
                {
                    gbStr += gbList[i].ToString() + "、";
                }
            }
            item.Add(new XElement("GB", gbStr));
            item.Add(new XElement("BlankValue", blankWidthCounty.Text));
            item.Add(new XElement("SolidValue", lineWidthCounty.Text));
            item.Add(new XElement("SymbolGroup", groupsCounty.Text));
            boul.Add(item);
            //乡镇
            item = new XElement("Item");
            level = new XElement("Level", "Town");
            level.SetAttributeValue("name", "乡镇");
            item.Add(level);
            item.Add(new XElement("PointStep", BoulDrawDic["Town"].PointStep));
            gbList = BoulDrawDic["Town"].GBList;
            gbStr = "";
            for (int i = 0; i < gbList.Count; ++i)
            {
                if (i == gbList.Count - 1)
                {
                    gbStr += gbList[i].ToString();
                }
                else
                {
                    gbStr += gbList[i].ToString() + "、";
                }
            }
            item.Add(new XElement("GB", gbStr));
            item.Add(new XElement("BlankValue", blankWidthTown.Text));
            item.Add(new XElement("SolidValue", lineWidthTown.Text));
            item.Add(new XElement("SymbolGroup", groupsTown.Text));
            boul.Add(item);
            #endregion
            ExpertiseParamsClass.UpdateBoulDraw(GApplication.Application, boulDraw);
        }
        /// <summary>
        /// 加载经验方案参数
        /// </summary>
        /// <param name="fileName">经验方案XML</param>
        private void LoadBOULTHParams(string fileName)
        {
            BoulDrawDic.Clear();
            dgLyrs.Rows.Clear();

            XDocument doc = XDocument.Load(fileName);
           
            {
               
                try
                {
                    var content = doc.Element("Template").Element("Content");
                    var boulDraw = content.Element("BoulDraw");
                    //容差
                    string tolerance = boulDraw.Element("BufferValue").Value;
                    txtTolerance.Text = tolerance;
                    //图层
                    foreach (var ele in boulDraw.Element("ObjectLayer").Elements("Item"))
                    {
                        int rowIndex = dgLyrs.Rows.Add();

                        ((DataGridViewCheckBoxCell)dgLyrs.Rows[rowIndex].Cells["Selected"]).Value =Convert.ToBoolean(ele.Attribute("checked").Value);
                        dgLyrs.Rows[rowIndex].Cells["Lyr"].Value = ele.Value;

                        if (ele.Attribute("SQL") != null)
                        {
                            dgLyrs.Rows[rowIndex].Cells["SQL"].Value = (ele.Attribute("SQL").Value);
                        }
                        else
                        {
                            dgLyrs.Rows[rowIndex].Cells["SQL"].Value = "";
                        }
                        if (ele.Attribute("Note") != null)
                        {
                            dgLyrs.Rows[rowIndex].Cells["Note"].Value = (ele.Attribute("Note").Value);
                        }
                        else
                        {
                            dgLyrs.Rows[rowIndex].Cells["Note"].Value = ("全部要素...");
                        }
                   
                    }
                    //境界
                    foreach (var ele in boulDraw.Element("Boul").Elements("Item"))
                    {
                        #region
                        var blankwidth = ele.Element("BlankValue").Value;
                        var linewidth = ele.Element("SolidValue").Value;
                        var pointstep = ele.Element("PointStep").Value;
                        var groups = ele.Element("SymbolGroup").Value;
                        string gbStr = ele.Element("GB").Value.ToString();
                        string[] gbStrList = gbStr.Split('、');
                        List<int> gbList = new List<int>();
                        foreach (var item in gbStrList)
                        {
                            gbList.Add(int.Parse(item.Trim()));
                        }
                        var info = new BoulDrawInfo
                        {
                            Level = ele.Element("Level").Value,
                            BlankValue = double.Parse(blankwidth),
                            SolidValue = double.Parse(linewidth),
                            PointStep = double.Parse(pointstep),
                            GBList = gbList,
                            SymbolGroup = double.Parse(groups)
                        };
                        BoulDrawDic[info.Level] = info;
                        switch (ele.Element("Level").Value)
                        {
                            case "Province":
                                blankWidthProvince.Text = blankwidth;
                                lineWidthProvince.Text = linewidth;
                                groupsProvince.Text = groups;
                                break;
                            case "State":
                                blankWidthState.Text = blankwidth;
                                lineWidthState.Text = linewidth;
                                groupsState.Text = groups;
                                break;
                            case "County":
                                blankWidthCounty.Text = blankwidth;
                                lineWidthCounty.Text = linewidth;
                                groupsCounty.Text = groups;
                                break;
                            case "Town":
                                blankWidthTown.Text = blankwidth;
                                lineWidthTown.Text = linewidth;
                                groupsTown.Text = groups;
                                break;
                            default:
                                break;
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(string.Format("经验方案中【境界跳绘】参数配置错误：{0}", ex.Message));
                    return;
                }

            }
        }

        private void btn_LastParas_Click(object sender, EventArgs e)
        {
            string planStr = @"专家库\经验方案\经验方案.xml";
            string fileName = app.Template.Root + @"\" + planStr;
            LoadBOULTHParams(fileName);            
        }

        private void LoadParas()
        {
            string planxml = "";
            if (ExpertiseParamsClass.LoadOutParams(out planxml))//加载外部参数
            {
                if (File.Exists(planxml))
                {
                    LoadOutParams(planxml);
                }
            }
            else//加载默认参数
            {
                string fileName = app.Template.Root + @"\专家库\境界跳绘\境界跳绘参数.xml";
                LoadOutParams(fileName);
            }
        }

        public void SetAutoParas()
        {
            ValidateUtil valid = new ValidateUtil();
            if (!valid.TraversalTextBox(this.Controls))
            {
                return;
            }
            if (!CheckPrams())
                return;
            tolerance = 0;
            double.TryParse(txtTolerance.Text, out tolerance);
            layerList = new Dictionary <string,string>();
            for (int i = 0; i < dgLyrs.Rows.Count; ++i)
            {
                bool val = bool.Parse(dgLyrs.Rows[i].Cells[0].Value.ToString());
                if (val)
                {
                    layerList.Add(dgLyrs.Rows[i].Cells[1].Value.ToString(), dgLyrs.Rows[i].Cells[2].Value.ToString());
                }
            }
            
        }

        private void FrmBOULTHSet_Load(object sender, EventArgs e)
        {
            cmbMapType.SelectedIndex = 0;
        }

        private void dgLyrs_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cbProvince_CheckedChanged(object sender, EventArgs e)
        {
            gbProvince.Enabled = cbProvince.Checked;
        }

        private void cbState_CheckedChanged(object sender, EventArgs e)
        {
            gbState.Enabled = cbState.Checked;
        }

        private void cbCounty_CheckedChanged(object sender, EventArgs e)
        {
            gbCounty.Enabled = cbCounty.Checked;
        }

        private void cbTown_CheckedChanged(object sender, EventArgs e)
        {
            gbTown.Enabled = cbTown.Checked;
        }

        private void cmbMapType_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbProvince.Checked = false;
            cbState.Checked = false;
            cbCounty.Checked = false;
            cbTown.Checked = false;
            switch (cmbMapType.SelectedItem.ToString())
            {
                case "省图":
                      cbProvince.Checked = true;
                      cbState.Checked = true;
                    break;
                case "市图":
                      cbProvince.Checked = true;
                      cbState.Checked = true;
                      cbCounty.Checked = true;
                    break;
                default://
                      cbProvince.Checked = true;
                      cbState.Checked = true;
                      cbCounty.Checked = true;
                      cbTown.Checked = true;
                    break;
            }
        }

        
        
    }
    public  class BoulDrawInfo
    {
        public string Level
        {
            get;
            set;
        }
        public bool Checked
        {
            get;
            set;
        }
        public List<int> GBList
        {
            get;
            set;
        }
        public double BlankValue
        {
            get;
            set;
        }
        public double SolidValue
        {
            get;
            set;
        }
        public double PointStep
        {
            get;
            set;
        }
        public double SymbolGroup
        {
            get;
            set;
        }
    }
}

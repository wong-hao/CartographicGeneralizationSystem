using System;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using System.Collections;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Xml.Linq;
using ESRI.ArcGIS.Framework;
using SMGI.Common;
using System.IO;
using SMGI.Plugin.EmergencyMap.LabelSym;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmAttributeTool : Form
    {
        Font txtFont = new Font("宋体", 10);
        Color textColor = Color.Black;

        Font txtFont1 = new Font("宋体", 10);
        Color textColor1 = Color.Black;
        DataRow drRule = null;
        DataTable sourceDt = null;
        public DataTable targetDt = null;
        public string ExpressStr = string.Empty;
        public string ExpressStr1 = string.Empty;
        public FrmAttributeTool()
        {
            InitializeComponent();
        }
        int panelTypeHeight = 0;
        int panelTextHeight = 0;
        int panelSepHeight = 0;
        public bool IsExpress
        {
            get
            {
                return cbVbScript.Checked;
            }
        }
        public bool IsExpressDown
        {
            get
            {
                return cbVbScriptDown.Checked;
            }
        }
        private void LoadRule(bool import=false)
        {
            try
            {

                if (import)
                {
                    int index = cmbLyr.Items.IndexOf(drRule["图层"].ToString().Trim());
                    cmbLyr.SelectedIndex = index;
                    cmbFields.SelectedIndex = cmbFields.Items.IndexOf(drRule["标注字段"].ToString().Trim());
                    if (drRule["分割线显示"].ToString() =="是")
                    {
                        cBTextDown.Checked = true;
                    }
                    cmbFields1.SelectedIndex = cmbFields1.Items.IndexOf(drRule["标注字段1"].ToString().Trim());
                    cmbLableMethod.SelectedIndex = cmbLableMethod.Items.IndexOf(drRule["标注方法"].ToString().Trim());
                    txtTypeName.Text = (drRule["分类名称"].ToString().Trim());
                    txtSql.Text = (drRule["过滤条件"].ToString().Trim());
                    //注记表达式
                    ExpressStr=drRule["注记表达式"].ToString().Trim();
                    if (drRule["表达式应用"].ToString() == "是")
                    {
                        cbVbScript.Checked = true;
                    }
                    if (drRule["表达式应用下标"].ToString() == "是")
                    {
                        cbVbScriptDown.Checked = true;
                        ExpressStr1 = drRule["注记表达式下标"].ToString().Trim();
                    }
                    
                }
                else
                {
                    int index = cmbLyr.Items.IndexOf(drRule["图层"].ToString().Trim());
                    cmbLyr.SelectedIndex = index;
                    cmbFields.SelectedIndex = cmbFields.Items.IndexOf(drRule["标注字段"].ToString().Trim());
                    if (drRule["分割线显示"].ToString() == "是")
                    {
                        cBTextDown.Checked = true;
                    }
                    cmbFields1.SelectedIndex = cmbFields1.Items.IndexOf(drRule["标注字段1"].ToString().Trim());
                    cmbLableMethod.SelectedIndex = cmbLableMethod.Items.IndexOf(drRule["标注方法"].ToString().Trim());
                    txtTypeName.Text = (drRule["分类名称"].ToString().Trim());
                    txtSql.Text = (drRule["过滤条件"].ToString().Trim());

                    //注记表达式
                    ExpressStr = drRule["注记表达式"].ToString().Trim();
                    if (drRule["表达式应用"].ToString() == "是")
                    {
                        cbVbScript.Checked = true;
                    }
                    if (drRule["表达式应用下标"].ToString() == "是")
                    {
                        cbVbScriptDown.Checked = true;
                        ExpressStr1 = drRule["注记表达式下标"].ToString().Trim();
                    }
                }
                numericUpDownTxt.Value=decimal.Parse(drRule["文本间距"].ToString());
                //文字信息
                string fontname = drRule["字体名称"].ToString();
                float fontsize = float.Parse(drRule["字体大小"].ToString());
                
                bool fontItalic = false;
                bool fontunderLine = false;
                bool fontBold = false;
                bool.TryParse(drRule["字体加粗"].ToString(), out fontBold);
                bool.TryParse(drRule["字体斜体"].ToString(), out fontItalic);
                bool.TryParse(drRule["字体下划线"].ToString(), out fontunderLine);
                System.Drawing.FontStyle fs = FontStyle.Regular;
                if(fontBold)
                {
                    fs=FontStyle.Bold;
                }
                if (fontunderLine)
                {
                    fs |= FontStyle.Underline;
                     
                }
                if (fontItalic)
                {
                    fs |= FontStyle.Italic;
                }
                txtFont = new System.Drawing.Font(fontname, fontsize,fs);

                lbFont.Text = fontname + "," + fontsize + "pt";
                textColor = ColorHelper.GetColorByCmykStr(drRule["字体颜色"].ToString());
                lbColor.ForeColor = textColor;
                lbColor.Text = textColor.Name;
                lbFont.Font = txtFont;
                string fontname1 = drRule["字体名称1"].ToString();
                float fontsize1 = float.Parse(drRule["字体大小1"].ToString());
              
                bool.TryParse(drRule["字体加粗1"].ToString(), out fontBold);
                bool.TryParse(drRule["字体斜体1"].ToString(), out fontItalic);
                bool.TryParse(drRule["字体下划线1"].ToString(), out fontunderLine);
               
                fs = FontStyle.Regular;
                if (fontBold)
                {
                    fs = FontStyle.Bold;
                }
                if (fontunderLine)
                {
                    fs |= FontStyle.Underline;

                }
                if (fontItalic)
                {
                    fs |= FontStyle.Italic;
                }

                txtFont1 = new System.Drawing.Font(fontname1, fontsize1,fs);
                lbFont1.Text = fontname1 + "," + fontsize1 + "pt";
                lbFont1.Font = txtFont1;
                textColor1 = ColorHelper.GetColorByCmykStr(drRule["字体颜色1"].ToString());
                lbColor1.ForeColor = textColor1;
                lbColor1.Text = textColor1.Name;
                //背景框
                cmbFillType.SelectedIndex = LabelClass.Instance.ExtentName.Values.ToList().IndexOf(drRule["内容框类型"].ToString());
                btColorFill.BackColor = ColorHelper.GetColorByCmykStr(drRule["内容框填充颜色"].ToString());
                cmbFillStlye.SelectedIndex = LabelClass.Instance.FillStyle.Values.ToList().IndexOf(drRule["内容框填充类型"].ToString());
                cmbFillLineStyle.SelectedIndex = LabelClass.Instance.LineStyle.Values.ToList().IndexOf(drRule["内容框边线类型"].ToString());
                btFillLineColor.BackColor = ColorHelper.GetColorByCmykStr(drRule["内容框边线颜色"].ToString());
                txtFillLineWidth.Text = drRule["内容框边线宽度"].ToString();
                //锚点
                CmbAnchor.SelectedIndex = LabelClass.Instance.AnchorName.Values.ToList().IndexOf(drRule["锚点类型"].ToString());
                txtAnchorSize.Text = drRule["锚点尺寸"].ToString();

                AnchorColor.BackColor = ColorHelper.GetColorByCmykStr(drRule["锚点填充颜色"].ToString());
                cmbAnchorLineStyle.SelectedIndex = LabelClass.Instance.LineStyle.Values.ToList().IndexOf(drRule["锚点边线类型"].ToString());
                AnchorLineColor.BackColor = ColorHelper.GetColorByCmykStr(drRule["锚点边线颜色"].ToString());
                cmbAnchorFillStyle.SelectedIndex = LabelClass.Instance.FillStyle.Values.ToList().IndexOf(drRule["锚点填充类型"].ToString());
                txtAnchorLineWIdth.Text = drRule["锚点边线宽度"].ToString();

                //连接线

                btColorLabelLine.BackColor = ColorHelper.GetColorByCmykStr(drRule["连接线颜色"].ToString());
                cmbLabelLineType.SelectedIndex = LabelClass.Instance.LineStyle.Values.ToList().IndexOf(drRule["连接线类型"].ToString());
                txtLabelLineWidth.Text = drRule["连接线宽度"].ToString();
                txtLabelLineLens.Text = drRule["连接线长度"].ToString();

                cbSep.Checked = false;
                cmbSepType.SelectedIndex = LabelClass.Instance.LineStyle.Values.ToList().IndexOf(drRule["分割线类型"].ToString());
                txtSepInterval.Text = drRule["分割线间距"].ToString();
                txtSepLineWidth.Text = drRule["分割线宽度"].ToString();
                btSepColor.BackColor = ColorHelper.GetColorByCmykStr(drRule["分割线颜色"].ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }
        private void FrmAttributeTool_Load(object sender, EventArgs e)
        {
            try
            {
                cbSep.Checked = cBTextDown.Checked;
                var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l.Visible&&(l is IFeatureLayer)&&!(l is IFDOGraphicsLayer);
                }));
                foreach(var lyr in lyrs)
                {
                    if ((lyr as IFeatureLayer).FeatureClass != null)
                        cmbLyr.Items.Add((lyr as IFeatureLayer).FeatureClass.AliasName);
                }


                panelTypeHeight = panelType.Height;
                panelTextHeight = panelTextDown.Height;
                panelSepHeight = panelSepLine.Height;
                //默认下标为空
                panelTextDown.Height = 0;
                panelSepLine.Height = 0;
                

                this.Height -=(panelTextHeight + panelSepHeight);

                string mdbPath = GApplication.Application.Template.Root + @"\专家库\标注引线\引线规则.mdb";
                sourceDt = Helper.ReadToDataTable(mdbPath, "属性标注");
                drRule = sourceDt.Rows[0];
                if (LabelClass.Instance.LabAttrDt != null)
                {
                    if (LabelClass.Instance.LabAttrDt.Rows.Count > 0)
                    {
                        drRule = LabelClass.Instance.LabAttrDt.Rows[0];
                    }
                }
               


                cmbLableMethod.Items.AddRange(new string[] { "同一种方式标注", "分类标注"});
                cmbFontLocation.Items.AddRange(new string[] { "居中", "靠左", "靠右" });
                cmbFontLocation1.Items.AddRange(new string[] { "居中", "靠左", "靠右" });
                


                cmbLableMethod.SelectedIndex = 0;
                cmbFontLocation.SelectedIndex = 0;
                cmbFontLocation1.SelectedIndex = 0;

 
               
                foreach (var kv in LabelClass.Instance.AnchorName)
                {
                    CmbAnchor.Items.Add(kv.Value);
                }
                foreach (var kv in LabelClass.Instance.ExtentName)
                {
                    cmbFillType.Items.Add(kv.Value);
                }
                foreach (var kv in LabelClass.Instance.LineStyle)
                {
                    cmbLabelLineType.Items.Add(kv.Value);
                    cmbFillLineStyle.Items.Add(kv.Value);
                    cmbAnchorLineStyle.Items.Add(kv.Value);
                    cmbSepType.Items.Add(kv.Value);
                }

                LoadRule();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }
        private List<string> GetFields(IFeatureClass fc)
        {
            List<string> ruleNames = new List<string>();
            for (int i = 0; i < fc.Fields.FieldCount; i++)
            {
                var field = fc.Fields.get_Field(i);
                if (field.Type != esriFieldType.esriFieldTypeGeometry && field.Type != esriFieldType.esriFieldTypeBlob && !field.Name.ToLower().Contains("shape") && field.Type != esriFieldType.esriFieldTypeOID)
                {
                    ruleNames.Add(field.Name.ToUpper());
                }
            }
            return ruleNames;
        }
        private void FrmLabelLine_Load(object sender, EventArgs e)
        {


          

        }
        string tableName = "属性标注";
        private void UpdateRule()
        {
            string mdbPath = GApplication.Application.Template.Root + @"\专家库\标注引线\引线规则.mdb";
            string id = drRule["ID"].ToString();
            string sql = "UPDATE " + tableName + " SET ";
            for (int i = 0; i < drRule.Table.Columns.Count; i++)
            {

                string columnName = drRule.Table.Columns[i].ColumnName;
                if (columnName.ToUpper() == "ID")
                    continue;
                string value = drRule[i].ToString();
                sql += columnName + "= '" + value + "',";
            }
            //sql +=  "图层 = '" + this.txtLyr.Text + "',";
            //sql += "标注字段 = '" + this.cmbFields.SelectedItem.ToString() + "',"; 
            sql = sql.Substring(0, sql.Length - 1);
            sql += "where ID=" + id;
            IWorkspaceFactory awf = new AccessWorkspaceFactory();
            var ws = awf.OpenFromFile(mdbPath, 0);
            ws.ExecuteSQL(sql);
            Marshal.ReleaseComObject(ws);
            Marshal.ReleaseComObject(awf);
        }
        private bool UpdateParms()
        {
            if (cmbLyr.SelectedItem == null)
            {
                MessageBox.Show("请设置图层和字段！");
                return false;
            }
            if (IsExpress && ExpressStr == string.Empty)
            {
                MessageBox.Show("请设置注记表达式！");
                return false;
            }
            if (!IsExpress && cmbFields.SelectedItem == null)
            {
                MessageBox.Show("请设置字段！");
                return false;
            }

            if (IsExpressDown && ExpressStr1 == string.Empty && cBTextDown.Checked)
            {
                MessageBox.Show("请设置下标注记表达式！");
                return false;
            }
            if (!IsExpressDown && cmbFields1.SelectedItem == null && cBTextDown.Checked)
            {
                MessageBox.Show("请设置下标字段！");
                return false;
            }

            if (CmbAnchor.SelectedItem == null)
            {
                MessageBox.Show("请先选择锚点类型");
                return false;
            }
            if (cmbLabelLineType.SelectedItem == null)
            {
                MessageBox.Show("请先选择连接线类型");
                return false;
            }
            if (cmbFillLineStyle.SelectedItem == null)
            {
                MessageBox.Show("请先选择填充边线类型");
                return false;
            }
            if (cmbAnchorLineStyle.SelectedItem == null)
            {
                MessageBox.Show("请先选择锚点边线类型");
                return false;
            }
            if (cmbLyr.SelectedItem == null)
            {
                MessageBox.Show("请先选择图层");
                return false;
            }
            if (cmbFields.SelectedItem == null)
            {
                MessageBox.Show("请先选择字段");
                return false;
            }
           
            try
            {
                double.Parse(txtAnchorSize.Text);
                double.Parse(txtLabelLineWidth.Text);
                double.Parse(txtLabelLineLens.Text);
                double.Parse(txtFillLineWidth.Text);
                double.Parse(txtAnchorLineWIdth.Text);

            }
            catch
            {
                MessageBox.Show("请输入正确数值！");
            }
            drRule["表达式应用"] = IsExpress?"是":"否";
            if (IsExpress)
            {
                drRule["注记表达式"] = ExpressStr;
            }
            drRule["表达式应用下标"] = IsExpressDown ? "是" : "否";
            if (IsExpressDown)
            {
                drRule["注记表达式下标"] = ExpressStr1;
            }
            drRule["分类名称"] = txtTypeName.Text;
            drRule["标注方法"] = cmbLableMethod.SelectedItem.ToString();
            drRule["过滤条件"] = txtSql.Text;
            if (cmbLableMethod.SelectedIndex == 0)
                drRule["过滤条件"] = "";
            drRule["图层"] = cmbLyr.SelectedItem.ToString();
            drRule["标注字段"] = this.cmbFields.SelectedItem.ToString();
            if (cmbFields1.SelectedItem != null)
                drRule["标注字段1"] = this.cmbFields1.SelectedItem.ToString();
            //文本上标
            drRule["字体名称"] = (txtFont.Name);
            drRule["字体大小"] = (txtFont.Size);
            drRule["字体颜色"] = (ColorHelper.GetCmykStrByColor(textColor));
            drRule["字体加粗"] = (txtFont.Bold.ToString());
            drRule["字体斜体"] = (txtFont.Italic.ToString());
            drRule["字体下划线"] = (txtFont.Underline.ToString());
            drRule["字体对齐方式"] = cmbFontLocation.SelectedItem.ToString();
            drRule["文本间距"]=numericUpDownTxt.Value;
            //下标
            drRule["字体名称1"] = (txtFont1.Name);
            drRule["字体大小1"] = (txtFont1.Size);
            drRule["字体颜色1"] = (ColorHelper.GetCmykStrByColor(textColor1));
            drRule["字体加粗1"] = (txtFont1.Bold.ToString());
            drRule["字体斜体1"] = (txtFont1.Italic.ToString());
            drRule["字体下划线1"] = (txtFont1.Underline.ToString());
            drRule["字体对齐方式1"] = cmbFontLocation1.SelectedItem.ToString();
            //背景框
            drRule["内容框类型"] = cmbFillType.SelectedItem.ToString();
            drRule["内容框填充颜色"] = ColorHelper.GetCmykStrByColor(btColorFill.BackColor);
            drRule["内容框填充类型"] = cmbFillStlye.SelectedItem.ToString();
            drRule["内容框边线颜色"] = ColorHelper.GetCmykStrByColor(btFillLineColor.BackColor);
            drRule["内容框边线类型"] = cmbFillLineStyle.SelectedItem.ToString();
            drRule["内容框边线宽度"] = txtFillLineWidth.Text;
            //锚点
            drRule["锚点类型"] = CmbAnchor.SelectedItem.ToString();
            drRule["锚点尺寸"] = txtAnchorSize.Text;
            drRule["锚点边线类型"] = cmbAnchorLineStyle.SelectedItem.ToString();
            drRule["锚点填充颜色"] = ColorHelper.GetCmykStrByColor(AnchorColor.BackColor);
            drRule["锚点边线颜色"] = ColorHelper.GetCmykStrByColor(AnchorLineColor.BackColor);
            drRule["锚点边线宽度"] = txtAnchorLineWIdth.Text;
            drRule["锚点填充类型"] = cmbAnchorFillStyle.SelectedItem.ToString();
            //连接线
            drRule["连接线颜色"] = ColorHelper.GetCmykStrByColor(btColorLabelLine.BackColor);
            drRule["连接线类型"] = cmbLabelLineType.SelectedItem.ToString();
            drRule["连接线宽度"] = txtLabelLineWidth.Text;
            drRule["连接线长度"] = txtLabelLineLens.Text;
            //分割线
            drRule["分割线显示"] = this.cBTextDown.Checked ? "是" : "否";
            drRule["分割线颜色"] = ColorHelper.GetCmykStrByColor(btSepColor.BackColor);
            drRule["分割线类型"] = cmbSepType.SelectedItem.ToString();
            drRule["分割线宽度"] = txtSepLineWidth.Text;
            drRule["分割线间距"] = txtSepInterval.Text;
            //批量 
            drRule["批量"] = cbBatch.Checked.ToString();
            
            return true;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            bool val= UpdateParms();
            if(!val)
                return;
            // UpdateRule();
            targetDt = (sourceDt).Clone();
            targetDt.Rows.Add(drRule.ItemArray);
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btFont_Click(object sender, EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = txtFont;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFont = pFontDialog.Font;
                lbFont.Font = txtFont;
                lbFont.Text = pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";
               
            }
        }

        private void btfontColor_Click(object sender, EventArgs e)
        {
            ColorDialog pColorDialog = new ColorDialog();
            pColorDialog.Color = textColor;
            if (pColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                lbColor.ForeColor = pColorDialog.Color;
                lbColor.Text = pColorDialog.Color.Name;
                textColor = pColorDialog.Color;
            }
        }

        private void btColor_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ColorHelper.ConvertColorToIColor(btn.BackColor);
            color.NullColor = bool.Parse(btColorFill.Tag.ToString());
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;

            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;


            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
                btn.BackColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
                btn.Tag = colorPalette.Color.NullColor;
            }

        }


        bool expand = false;
        private void btAdv_Click(object sender, EventArgs e)
        {

            if (expand)
            {
                this.Height -= (panelAnchor.Height + panelTextDown.Height + panelBackground.Height + gbSep.Height);
            }
            else
            {
                this.Height += (panelAnchor.Height + panelTextDown.Height + panelBackground.Height + gbSep.Height);
            }
            expand = !expand;
        }
     
        private void cmbLableMethod_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (cmbLableMethod.SelectedIndex==0)
            {
                if (panelType.Height == panelTypeHeight)
                {
                    panelType.Height = 0;
                    this.Height -= panelTypeHeight;
                   
                }
            }
            else
            {
                if (panelType.Height == 0)
                {
                    panelType.Height = panelTypeHeight;
                    this.Height += panelTypeHeight;
                    
                }
            }

        }

        private void cmbLyr_SelectedIndexChanged(object sender, System.EventArgs e)
        {
             IFeatureLayer feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
             ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == cmbLyr.SelectedItem.ToString().ToUpper())).FirstOrDefault() as IFeatureLayer;
            if (feLayer != null)
            {
                cmbFields.Items.Clear();
                cmbFields1.Items.Clear();
                foreach (var kv in GetFields(feLayer.FeatureClass))
                {
                    cmbFields.Items.Add(kv);
                    cmbFields1.Items.Add(kv);
                }
                cmbFields.Text = "";
                cmbFields1.Text = "";
                cmbFields.SelectedIndex = cmbFields.Items.IndexOf(drRule["标注字段"].ToString().ToUpper());
                cmbFields1.SelectedIndex = cmbFields1.Items.IndexOf(drRule["标注字段"].ToString().ToUpper());


            }
                   
        }

        private void btExport_Click(object sender, System.EventArgs e)
        {
            bool flag= UpdateParms();
            if (!flag)
                return;
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "标注文件|*.xml";
            sf.FileName = txtTypeName.Text;
            sf.InitialDirectory = GApplication.Application.Template.Root + @"\专家库\标注引线";
            DialogResult dr = sf.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            string fileName = sf.FileName;
            XDocument doc = new XDocument();
            doc.Declaration = new XDeclaration("1.0", "utf-8", "");
            var root = new XElement("LableTemplate");
            doc.Add(root);
            //配置信息
            for (int i = 0; i < drRule.Table.Columns.Count; i++)
            {
                string name = drRule.Table.Columns[i].ColumnName;
                string val = drRule[i].ToString();
                var itemInfo = new XElement(name);
                itemInfo.SetValue(val);
                root.Add(itemInfo);

            }
            doc.Save(fileName);
        
        }

        private void cBTextDown_CheckedChanged(object sender, System.EventArgs e)
        {
            cbSep.Checked = cBTextDown.Checked;
            if (cBTextDown.Checked)
            {

                panelTextDown.Height = panelTextHeight;
                panelSepLine.Height = panelSepHeight;
                this.Height += panelTextHeight + panelSepHeight;
            }
            else
            {
                panelTextDown.Height = 0;
                panelSepLine.Height = 0;
                this.Height -= panelTextHeight + panelSepHeight;
            }

        }

        private void btFont1_Click(object sender, System.EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = txtFont1;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFont1 = pFontDialog.Font;
                lbFont1.Text = pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";
                lbFont1.Font = txtFont1;
               
            }
        }

        private void btfontColor1_Click(object sender, System.EventArgs e)
        {
            ColorDialog pColorDialog = new ColorDialog();
            pColorDialog.Color = textColor1;
            if (pColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                lbColor1.ForeColor = pColorDialog.Color;
                lbColor1.Text = pColorDialog.Color.Name;
                textColor1 = pColorDialog.Color;
            }
        }

        private void btImport_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "选取文件|*.xml";
            of.FileName = "标注规则";
            of.InitialDirectory = GApplication.Application.Template.Root + @"\专家库\标注引线";
            DialogResult dr = of.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            XDocument doc = XDocument.Load(of.FileName);
            var root = doc.Element("LableTemplate");
            foreach (var ele in root.Elements())
            {
                string name = ele.Name.LocalName;
                string val = ele.Value;
                if (name != "ID")
                {
                    drRule[name] = val;
                }
            }
            LoadRule(true);
            
        }

        private void cbSep_CheckedChanged(object sender, System.EventArgs e)
        {

        }

        private void btSql_Click(object sender, System.EventArgs e)
        { 
            if( cmbLyr.SelectedItem==null)
                return; 
            IFeatureLayer feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                    ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == cmbLyr.SelectedItem.ToString().ToUpper())).FirstOrDefault() as IFeatureLayer;
            if (feLayer == null)
            {
                return;
            }
            SelectBySQLDialog dlg = new SelectBySQLDialog(GApplication.Application.ActiveView as IMap);
            dlg.OnlyOneLayer = true;
            dlg.LayerSelected = feLayer;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtSql.Text = dlg.SQLCondition;
            }
        }

        private void btExpress_Click(object sender, System.EventArgs e)
        {
            FrmLbExpress frm = new FrmLbExpress(ExpressStr);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                ExpressStr = frm.ExpressionStr;
            }
        }

        private void cbVbScript_CheckedChanged(object sender, System.EventArgs e)
        {
            this.cmbFields.Enabled = !cbVbScript.Checked;
            this.btExpress.Enabled = cbVbScript.Checked;
        }

        private void btExpressDown_Click(object sender, System.EventArgs e)
        {
            FrmLbExpress frm = new FrmLbExpress(ExpressStr1);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                ExpressStr1 = frm.ExpressionStr;
            }

        }

        private void cbVbScriptDown_CheckedChanged(object sender, System.EventArgs e)
        {
            this.cmbFields1.Enabled = !cbVbScriptDown.Checked;
            this.btExpressDown.Enabled = cbVbScriptDown.Checked;
        }

        private void cmbFontLocation1_SelectedIndexChanged(object sender, System.EventArgs e)
        {

        }

        

       

       
        
       
    }
}

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
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Xml.Linq;
using ESRI.ArcGIS.Framework;
using SMGI.Common;
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmLabelInfo: Form
    {
        
        
        Font txtFont = new Font("宋体", 10);
        Color textColor = Color.Black;
        
        DataRow drRule = null;
        public FrmLabelInfo(DataRow drRule_)
        {
            InitializeComponent();
            drRule = drRule_;
        }
        double mapscale = 1;
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
           
          
            try
            {
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

                txtLyr.Text = drRule["图层"].ToString();
                {
                    IFeatureLayer feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                   ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == drRule["图层"].ToString().ToUpper())).FirstOrDefault() as IFeatureLayer;
                    if (feLayer != null)
                    {
                        foreach (var kv in GetFields(feLayer.FeatureClass))
                        {
                            cmbFields.Items.Add(kv);
                        }
                        cmbFields.SelectedIndex = cmbFields.Items.IndexOf(drRule["标注字段"].ToString().ToUpper());
                    }
                    else
                    {
                        cmbFields.Items.Add(drRule["标注字段"].ToString());
                        cmbFields.SelectedIndex = 0;
                    }
                }
                //文字信息
                string fontname = drRule["字体名称"].ToString();
                float fontsize = float.Parse(drRule["字体大小"].ToString());
                txtFont = new System.Drawing.Font(fontname, fontsize);
                lbFont.Text = fontname + "," + fontsize + "pt";
                textColor = ColorHelper.GetColorByCmykStr(drRule["字体颜色"].ToString());
                lbColor.ForeColor = textColor;
                lbColor.Text = textColor.Name;

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
                //分割线
                if (drRule["分割线显示"].ToString() == "是")
                {
                    cbSep.Checked = true;
                }
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
            sql+= "where ID=" + id;
            IWorkspaceFactory awf = new AccessWorkspaceFactory();
            var ws = awf.OpenFromFile(mdbPath, 0);
            ws.ExecuteSQL(sql);
            Marshal.ReleaseComObject(ws);
            Marshal.ReleaseComObject(awf);
        }
        private void btOK_Click(object sender, EventArgs e)
        {
            if (CmbAnchor.SelectedItem == null)
                return;
            if (cmbLabelLineType.SelectedItem == null)
                return;
            if (cmbFillLineStyle.SelectedItem == null)
                return;
            if (cmbAnchorLineStyle.SelectedItem == null)
                return;
            if (CmbAnchor.SelectedItem == null)
                return;
            

            try
            {
                double.Parse(txtAnchorSize.Text);
                double.Parse(txtLabelLineWidth.Text );
                double.Parse(txtLabelLineLens.Text);
                double.Parse(txtFillLineWidth.Text);
                double.Parse(txtAnchorLineWIdth.Text);
                 
            }
            catch
            {
                MessageBox.Show("请输入正确数值！");
            }
            drRule["图层"] = this.txtLyr.Text;
            drRule["标注字段"] = this.cmbFields.SelectedItem.ToString();
            drRule["字体名称"] = (txtFont.Name);
            drRule["字体大小"] = (txtFont.Size);
            drRule["字体颜色"]=(ColorHelper.GetCmykStrByColor(textColor));
                  

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
            drRule["锚点填充类型"]= cmbAnchorFillStyle.SelectedItem.ToString();
            //连接线
            drRule["连接线颜色"] = ColorHelper.GetCmykStrByColor(btColorLabelLine.BackColor);
            drRule["连接线类型"] = cmbLabelLineType.SelectedItem.ToString();
            drRule["连接线宽度"] = txtLabelLineWidth.Text;
            drRule["连接线长度"] = txtLabelLineLens.Text;
            //分割线
             
            drRule["分割线显示"] = this.cbSep.Checked?"是":"否";
            drRule["分割线颜色"] = ColorHelper.GetCmykStrByColor(btSepColor.BackColor);
            drRule["分割线类型"] = cmbSepType.SelectedItem.ToString();
            drRule["分割线宽度"] = txtSepLineWidth.Text;
            drRule["分割线间距"] = txtSepInterval.Text;     
          
            UpdateRule();
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
                lbFont.Text = pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";
                txtFont = pFontDialog.Font;
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
                this.Height -= (panelLine.Height + panelBackGround.Height + panelAnthor.Height + gbSep.Height);
            }
            else
            {
                this.Height += (panelLine.Height + panelBackGround.Height + panelAnthor.Height + gbSep.Height);
            }
            expand = !expand;
        }

        private void btSepColor_Click(object sender, EventArgs e)
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
        
       
 
    }
}

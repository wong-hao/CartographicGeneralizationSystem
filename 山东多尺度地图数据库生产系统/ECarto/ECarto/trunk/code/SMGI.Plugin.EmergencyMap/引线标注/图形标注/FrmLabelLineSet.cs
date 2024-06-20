using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using SMGI.Common;
using System.IO;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmLabelLineSet: Form
    {
        /// <summary>
        /// 形状类型
        /// </summary>
        string type = string.Empty;
        public FrmLabelLineSet( string type_ = "")
        {
            InitializeComponent();
            type = type_;
        }
        private Color textColor = Color.FromName("Black");
        private System.Drawing.Font textFontName = new System.Drawing.Font("黑体", 12);
        private void FrmLabelLine_Load(object sender, EventArgs e)
        {
            if (type == "圆形")
            {
                this.txtContent.Text = "10";
            }
            this.Height -= (panelLine.Height + panelBackGround.Height + panelAnthor.Height + gbSep.Height);

            LabelClass.Instance.POIState = false;
            try
            {
                foreach(var kv in LabelClass.Instance.AnchorName)
                {
                    CmbAnchor.Items.Add(kv.Value);
                }
                foreach(var kv in LabelClass.Instance.ExtentName)
                {
                    cmbFillType.Items.Add(kv.Value);
                }
                foreach(var kv in LabelClass.Instance.LineStyle)
                {
                    cmbLabelLineType.Items.Add(kv.Value);
                    cmbFillLineStyle.Items.Add(kv.Value);
                    cmbAnchorLineStyle.Items.Add(kv.Value);
                    cmbSepType.Items.Add(kv.Value);
                }

                string fileName = GApplication.Application.Template.Root + @"\专家库\标注引线\默认配置.xml";
                if(!File.Exists(fileName))
                {
                    return;
                }
                XDocument doc = XDocument.Load(fileName);
                //只有主区，没附区
                var root = doc.Element("Template").Element("LabelText");
               
                XElement ele = root.Element("Text");
                //文字信息
                string fontname= ele.Element("Font").Attribute("FontName").Value;
                float fontsize = float.Parse(ele.Element("Font").Attribute("FontSize").Value);
                string colorName = ele.Element("Font").Attribute("FontColor").Value;
                       
                textColor =ColorHelper.GetColorByCmykStr(ele.Element("Font").Attribute("FontColor").Value);
                textFontName = new System.Drawing.Font(fontname, fontsize);

                lbFont.Text = textFontName.Name + "," + textFontName.Size + "pt";
                lbColor.ForeColor = textColor;
                lbColor.Text = textColor.Name;
                      
                //背景框
                ele = root.Element("Content");
                string txttype = ele.Element("TextType").Value;
                if (type != string.Empty)
                {
                    txttype = type;
                }
                cmbFillType.SelectedIndex = LabelClass.Instance.ExtentName.Values.ToList().IndexOf(txttype);
                btColorFill.BackColor = ColorHelper.GetColorByCmykStr(ele.Element("FillColor").Value);
                cmbFillStlye.SelectedIndex = cmbFillStlye.Items.IndexOf(ele.Element("FillStlye").Value);
                cmbFillLineStyle.SelectedIndex = LabelClass.Instance.LineStyle.Values.ToList().IndexOf(ele.Element("TextLineType").Value);
                btFillLineColor.BackColor = ColorHelper.GetColorByCmykStr(ele.Element("TextLineColor").Value);
                txtFillLineWidth.Text = ele.Element("TextLineWidth").Value;
                //锚点
                ele = root.Element("Anchor");
                CmbAnchor.SelectedIndex = LabelClass.Instance.AnchorName.Values.ToList().IndexOf(ele.Element("AnchorType").Value);
                txtAnchorSize.Text = ele.Element("AnchorSize").Value;
                AnchorColor.BackColor = ColorHelper.GetColorByCmykStr(ele.Element("AnchorFillColor").Value);
               // btColorFill.Tag = ele.Element("AnchorNUllColor").Value;
                cmbAnchorLineStyle.SelectedIndex = LabelClass.Instance.LineStyle.Values.ToList().IndexOf(ele.Element("AnchorLineStyle").Value);
                AnchorLineColor.BackColor = ColorHelper.GetColorByCmykStr(ele.Element("AnchorLineColor").Value);
                cmbAnchorFillStyle.SelectedIndex = cmbAnchorFillStyle.Items.IndexOf(ele.Element("FillStlye").Value);
                txtAnchorLineWIdth.Text = ele.Element("AnchorLineWidth").Value;
                //连接线
                ele = root.Element("ConnectLine");
                btColorLabelLine.BackColor=ColorHelper.GetColorByCmykStr( ele.Element("LineColor").Value);
                cmbLabelLineType.SelectedIndex=LabelClass.Instance.LineStyle.Values.ToList().IndexOf( ele.Element("LineType").Value);
                txtLabelLineWidth.Text = ele.Element("LineWidth").Value;
                txtLabelLineLens.Text = ele.Element("ConnectLens").Value;
                //分割线
                ele = root.Element("SepLine");
                if (ele.Attribute("check") != null)
                {
                    this.cbSep.Checked = bool.Parse(ele.Attribute("check").Value);
                }
                cmbSepType.SelectedIndex = LabelClass.Instance.LineStyle.Values.ToList().IndexOf(ele.Element("SepLineType").Value);
                txtSepInterval.Text = ele.Element("SepInterval").Value;
                txtSepLineWidth.Text = ele.Element("SepLineWidth").Value;
                btSepColor.BackColor = ColorHelper.GetColorByCmykStr(ele.Element("SepLineColor").Value);
                       
              
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }

        }
        public string TxtValue
        {
            get
            {
                string val = txtContent.Text.Trim();
                string[] strs = val.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length > 1)
                {
                    val = string.Empty;
                    for (int i = 0; i < strs.Length - 1; i++)
                    {
                        val += strs[i] + "\r\n";
                    }
                    val += strs[strs.Length - 1];
                }
                return val;
            }
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
            if (txtContent.Text.Trim() == "")
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
            string fileName = GApplication.Application.Template.Root + @"\专家库\标注引线\默认配置.xml";
            if (!File.Exists(fileName))
            {
                return;
            }
            XDocument doc = XDocument.Load(fileName);
            var root = doc.Element("Template").Element("LabelText");
         
            //文字信息
            XElement ele = root.Element("Text");
            ele.Element("Font").Attribute("FontName").SetValue(textFontName.Name);
            ele.Element("Font").Attribute("FontSize").SetValue(textFontName.Size);
            ele.Element("Font").Attribute("FontColor").SetValue(ColorHelper.GetCmykStrByColor(textColor));
            if (ele.Element("Font").Attribute("FontItalic") == null)
            {
                XAttribute newatt = new XAttribute("FontItalic", "");
                ele.Element("Font").Add(newatt);
            }
            if (ele.Element("Font").Attribute("FontBold") == null)
            {
                XAttribute newatt = new XAttribute("FontBold", "");
                ele.Element("Font").Add(newatt);
            }
            ele.Element("Font").Attribute("FontItalic").SetValue(textFontName.Italic);
            ele.Element("Font").Attribute("FontBold").SetValue(textFontName.Bold);  

            //背景框
            ele = root.Element("Content");
          
            
            ele.Element("TextType").Value = cmbFillType.SelectedItem.ToString();
            ele.Element("FillColor").Value = ColorHelper.GetCmykStrByColor(btColorFill.BackColor);
            ele.Element("FillStlye").Value = cmbFillStlye.SelectedItem.ToString();
            ele.Element("TextLineColor").Value = ColorHelper.GetCmykStrByColor(btFillLineColor.BackColor);
            ele.Element("TextLineType").Value = cmbFillLineStyle.SelectedItem.ToString();
            ele.Element("TextLineWidth").Value = txtFillLineWidth.Text;
         

            //锚点
            ele = root.Element("Anchor");
            ele.Element("AnchorType").Value = CmbAnchor.SelectedItem.ToString();
            ele.Element("AnchorSize").Value = txtAnchorSize.Text;
            ele.Element("AnchorLineStyle").Value = cmbAnchorLineStyle.SelectedItem.ToString();
            ele.Element("AnchorFillColor").Value = ColorHelper.GetCmykStrByColor(AnchorColor.BackColor);
            ele.Element("AnchorLineColor").Value = ColorHelper.GetCmykStrByColor(AnchorLineColor.BackColor);
            ele.Element("AnchorLineWidth").Value = txtAnchorLineWIdth.Text;
            ele.Element("FillStlye").Value = cmbAnchorFillStyle.SelectedItem.ToString();
            //连接线
            ele = root.Element("ConnectLine");         
            ele.Element("LineColor").Value = ColorHelper.GetCmykStrByColor(btColorLabelLine.BackColor);
            ele.Element("LineType").Value = cmbLabelLineType.SelectedItem.ToString();
            ele.Element("LineWidth").Value = txtLabelLineWidth.Text;
            ele.Element("ConnectLens").Value = txtLabelLineLens.Text;

            //分割线
            ele = root.Element("SepLine");
            if (ele.Attribute("check") != null)
            {
                ele.Attribute("check").Value = this.cbSep.Checked.ToString();
                //this.cbSep.Checked = bool.Parse(ele.Attribute("check").Value);
            }
            ele.Element("SepLineColor").Value = ColorHelper.GetCmykStrByColor(btSepColor.BackColor);
            ele.Element("SepLineType").Value = cmbSepType.SelectedItem.ToString();
            ele.Element("SepLineWidth").Value = txtSepLineWidth.Text;
            ele.Element("SepInterval").Value = txtSepInterval.Text;     
                    
          
            doc.Save(fileName);
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btFont_Click(object sender, EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = textFontName;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbFont.Text = pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";
                textFontName = pFontDialog.Font;
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

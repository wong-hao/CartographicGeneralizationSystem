using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.IO;
using System.Xml.Linq;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace SMGI.Plugin.AnnotationEngine
{
    public partial class AnnotationAttributeForm : Form
    {
        /// <summary>
        /// 输出的注记属性
        /// </summary>
        public AnnotationAttribute OutAnnoAttribute
        {
            protected set;
            get;
        }


        private int _hWnd = 0;
        public AnnotationAttributeForm(int hWnd, Dictionary<IFeature, AnnotationAttribute> selFe2Attri)
        {
            InitializeComponent();

            _hWnd = hWnd;

            initParamFromConfigFile();

            #region 初始化注记图层组件
            string fcName = "";
            foreach (var kv in selFe2Attri)
            {
                if (fcName == "")
                {
                    fcName = (kv.Key.Class as IDataset).Name;
                    continue;
                }

                if (fcName != (kv.Key.Class as IDataset).Name)
                {
                    fcName = "";
                    break;
                }
            }
            if (fcName != "")
            {
                cbAnnoFCName.Items.Add(fcName);
                cbAnnoFCName.SelectedIndex = 0;
            }
            #endregion

            #region 初始化注记文本框
            string annoText = "";
            foreach (var kv in selFe2Attri)
            {
                if (annoText == "")
                {
                    annoText = kv.Value.Text;
                    continue;
                }

                if (annoText != kv.Value.Text)
                {
                    annoText = "";
                    break;
                }
            }

            if (annoText != "")
            {
                tbContent.Text = annoText;
            }
            #endregion

            #region 初始化字体名组件
            string fontName = "";
            foreach (var kv in selFe2Attri)
            {
                if (fontName == "")
                {
                    fontName = kv.Value.FontName;
                    continue;
                }

                if (fontName != kv.Value.FontName)
                {
                    fontName = "";
                    break;
                }
            }

            if (fontName != "")
            {
                cbFontFamily.Text = fontName;
            }
            #endregion

            #region 初始化字体大小组件
            string fontSize = "";
            foreach (var kv in selFe2Attri)
            {
                if (fontSize == "")
                {
                    fontSize = kv.Value.FontSize.ToString();
                    continue;
                }

                if (fontSize != kv.Value.FontName.ToString())
                {
                    fontSize = "";
                    break;
                }
            }
            if (fontSize != "")
            {
                cbFontSize.Text = fontSize;
            }
            #endregion
            
            #region 初始化字体颜色组件
            IColor clr = null;
            foreach (var kv in selFe2Attri)
            {
                if (clr == null)
                {
                    clr = kv.Value.FontColor;
                    continue;
                }

                if (clr.RGB != kv.Value.FontColor.RGB)
                {
                    clr = null;
                    break;
                }
            }
            if (clr != null)
            {
                cbAnnoCMYKColor.Text = GetCMYKTextByColor(clr);
                btnAnnoColor.BackColor = ConvertESRIColorToColor(clr);
            }
            #endregion

            //以下组件以第一个要素的属性为准进行初始化
            var firstAttri = selFe2Attri.First().Value;
            #region 初始化字体加粗组件
            cbBold.Checked = firstAttri.EnableBold;
            #endregion

            #region 注记是否已放置
            cbStatus.Checked = (firstAttri.AnnoStatus == ESRI.ArcGIS.Carto.esriAnnotationStatus.esriAnnoStatusPlaced);
            #endregion

            #region CJK
            cbCJK.Checked = firstAttri.CJKCharactersRotation;
            #endregion

            #region 初始化字体对齐方式组件
            cb_Horizontal.ValueMember = "Key";
            cb_Horizontal.DisplayMember = "Value";
            cb_Horizontal.Items.Add(new KeyValuePair<esriTextHorizontalAlignment, string>(esriTextHorizontalAlignment.esriTHALeft, "左对齐"));
            cb_Horizontal.Items.Add(new KeyValuePair<esriTextHorizontalAlignment, string>(esriTextHorizontalAlignment.esriTHACenter, "居中对齐"));
            cb_Horizontal.Items.Add(new KeyValuePair<esriTextHorizontalAlignment, string>(esriTextHorizontalAlignment.esriTHARight, "右对齐"));
            cb_Horizontal.Items.Add(new KeyValuePair<esriTextHorizontalAlignment, string>(esriTextHorizontalAlignment.esriTHAFull, "两端对齐"));

            cb_Vertical.ValueMember = "Key";
            cb_Vertical.DisplayMember = "Value";
            cb_Vertical.Items.Add(new KeyValuePair<esriTextVerticalAlignment, string>(esriTextVerticalAlignment.esriTVATop, "顶部对齐"));
            cb_Vertical.Items.Add(new KeyValuePair<esriTextVerticalAlignment, string>(esriTextVerticalAlignment.esriTVACenter, "居中对齐"));
            cb_Vertical.Items.Add(new KeyValuePair<esriTextVerticalAlignment, string>(esriTextVerticalAlignment.esriTVABaseline, "基线对齐"));
            cb_Vertical.Items.Add(new KeyValuePair<esriTextVerticalAlignment, string>(esriTextVerticalAlignment.esriTVABottom, "底部对齐"));


            int index = -1;
            for (int i = 0; i < cb_Horizontal.Items.Count; ++i)
            {
                var kv = (KeyValuePair<esriTextHorizontalAlignment, string>)cb_Horizontal.Items[i];
                if (kv.Key == firstAttri.HorizontalAlignment)
                {
                    index = i;
                    break;
                }
            }
            cb_Horizontal.SelectedIndex = index;

            index = -1;
            for (int i = 0; i < cb_Vertical.Items.Count; ++i)
            {
                var kv = (KeyValuePair<esriTextVerticalAlignment, string>)cb_Vertical.Items[i];
                if (kv.Key == firstAttri.VerticalAlignment)
                {
                    index = i;
                    break;
                }
            }
            cb_Vertical.SelectedIndex = index;
            #endregion

            #region 初始化注记背景组件
            cb_Balloon.ValueMember = "Key";
            cb_Balloon.DisplayMember = "Value";
            cb_Balloon.Items.Add(new KeyValuePair<esriBalloonCalloutStyle, string>(esriBalloonCalloutStyle.esriBCSRectangle,"矩形"));
            cb_Balloon.Items.Add(new KeyValuePair<esriBalloonCalloutStyle, string>(esriBalloonCalloutStyle.esriBCSRoundedRectangle, "圆角矩形"));

            ckTextBackground.Checked = firstAttri.EnableTextBackground;
            this.gb_Balloon.Enabled = firstAttri.EnableTextBackground;
            if (firstAttri.EnableTextBackground)
            {
                int styleIndex = -1;
                for (int i = 0; i < cb_Balloon.Items.Count; ++i)
                {
                    var kv = (KeyValuePair<esriBalloonCalloutStyle, string>)cb_Balloon.Items[i];
                    if (kv.Key == firstAttri.BackgroundStyle)
                    {
                        styleIndex = i;
                        break;
                    }
                }
                cb_Balloon.SelectedIndex = styleIndex;

                this.btBackgroudCol.BackColor = ConvertESRIColorToColor(firstAttri.BackgroundColor);

                num_LineWidth.Value = (decimal)firstAttri.BackgroundBorderWidth;
                this.btLineColor.BackColor = ConvertESRIColorToColor(firstAttri.BackgroundBorderColor);

                this.num_up.Value = (decimal)firstAttri.BackgroundMarginUp;
                this.num_down.Value = (decimal)firstAttri.BackgroundMarginDown;
                this.num_left.Value = (decimal)firstAttri.BackgroundMarginLeft;
                this.num_right.Value = (decimal)firstAttri.BackgroundMarginRight;
                
            }
            #endregion

            #region 初始化蒙版组件
            cb_Mask.ValueMember = "Key";
            cb_Mask.DisplayMember = "Value";
            cb_Mask.Items.Add(new KeyValuePair<esriMaskStyle, string>(esriMaskStyle.esriMSNone, "无"));
            cb_Mask.Items.Add(new KeyValuePair<esriMaskStyle, string>(esriMaskStyle.esriMSHalo, "光晕"));

            int maskIndex = -1;
            for (int i = 0; i < cb_Mask.Items.Count; ++i)
            {
                var kv = (KeyValuePair<esriMaskStyle, string>)cb_Mask.Items[i];
                if (kv.Key == firstAttri.MaskStyle)
                {
                    maskIndex = i;
                    break;
                }
            }
            cb_Mask.SelectedIndex = maskIndex;

            this.num_Mask.Value = (decimal)firstAttri.MaskSize;

            if (firstAttri.MaskColor != null)
            {
                cbMaskCMYKColor.Text = GetCMYKTextByColor(firstAttri.MaskColor);
                btnMaskColor.BackColor = ConvertESRIColorToColor(firstAttri.MaskColor);
            }
            #endregion

            this.cbAnnoFCName.Enabled = false;
        }

        private void AnnotationAttributeForm_Load(object sender, EventArgs e)
        {
            
        }

        private void cbAnnoCMYKColor_TextChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            IColor color = GetColorByCMYKText(cb.Text);
            if (color == null)
                return;

            this.btnAnnoColor.BackColor = ConvertESRIColorToColor(color);
        }

        private void ckTextBackground_CheckedChanged(object sender, EventArgs e)
        {
            this.gb_Balloon.Enabled = ckTextBackground.Checked;
        }

        private void cbMaskCMYKColor_TextChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            IColor color = GetColorByCMYKText(cb.Text);
            if (color == null)
                return;

            this.btnMaskColor.BackColor = ConvertESRIColorToColor(color);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            #region 参数合法性检查
            if (ckTextBackground.Checked)
            {
                if (cb_Balloon.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("请设置背景样式");
                    return ;
                }
            }
            #endregion

            OutAnnoAttribute = new AnnotationAttribute();
            #region 属性赋值
            OutAnnoAttribute.Text = tbContent.Text;

            double fontSize = 0;
            double.TryParse(cbFontSize.Text, out fontSize);
            OutAnnoAttribute.FontSize = fontSize;

            OutAnnoAttribute.FontName = cbFontFamily.Text;

            OutAnnoAttribute.FontColor = GetColorByCMYKText(cbAnnoCMYKColor.Text);

            OutAnnoAttribute.EnableBold = cbBold.Checked;

            if (cbStatus.Checked)
            {
                OutAnnoAttribute.AnnoStatus = ESRI.ArcGIS.Carto.esriAnnotationStatus.esriAnnoStatusPlaced;
            }
            else
            {
                OutAnnoAttribute.AnnoStatus = ESRI.ArcGIS.Carto.esriAnnotationStatus.esriAnnoStatusUnplaced;
            }

            OutAnnoAttribute.CJKCharactersRotation = cbCJK.Checked;
            
            OutAnnoAttribute.HorizontalAlignment = ((KeyValuePair<esriTextHorizontalAlignment, string>)cb_Horizontal.SelectedItem).Key;
            OutAnnoAttribute.VerticalAlignment = ((KeyValuePair<esriTextVerticalAlignment, string>)cb_Vertical.SelectedItem).Key;

            //注记背景
            OutAnnoAttribute.EnableTextBackground = ckTextBackground.Checked;
            if (OutAnnoAttribute.EnableTextBackground)
            {
                //背景
                OutAnnoAttribute.BackgroundStyle = ((KeyValuePair<esriBalloonCalloutStyle, string>)cb_Balloon.SelectedItem).Key;
                OutAnnoAttribute.BackgroundColor = ConvertColorToESRIColor(this.btBackgroudCol.BackColor);

                //轮廓
                OutAnnoAttribute.BackgroundBorderWidth = (double)this.num_LineWidth.Value;
                OutAnnoAttribute.BackgroundBorderColor = ConvertColorToESRIColor(this.btLineColor.BackColor);

                //边框
                OutAnnoAttribute.BackgroundMarginUp = (double)this.num_up.Value;
                OutAnnoAttribute.BackgroundMarginDown = (double)this.num_down.Value;
                OutAnnoAttribute.BackgroundMarginLeft = (double)this.num_left.Value;
                OutAnnoAttribute.BackgroundMarginRight = (double)this.num_right.Value;
            }

            //文字蒙版
            OutAnnoAttribute.MaskStyle = ((KeyValuePair<esriMaskStyle, string>)cb_Mask.SelectedItem).Key;
            OutAnnoAttribute.MaskSize = (double)num_Mask.Value;
            OutAnnoAttribute.MaskColor = GetColorByCMYKText(cbMaskCMYKColor.Text);
            #endregion

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void initParamFromConfigFile()
        {
            #region 读取预设字体大小配置文件
            List<string> fontSizeList = new List<string>();
            string fileName = GApplication.Application.Template.Root + @"\AnnoFont.xml";
            if (File.Exists(fileName))
            {
                XDocument doc = XDocument.Load(fileName);
                var annoFontItem = doc.Element("AnnoFont");
                var fontItems = annoFontItem.Elements("Font");
                foreach (XElement ele in fontItems)
                {
                    double size = double.Parse(ele.Attribute("size").Value);
                    fontSizeList.Add(size.ToString());
                }
            }
            if (fontSizeList.Count > 0)
            {
                cbFontSize.Items.AddRange(fontSizeList.Distinct().ToArray());
            }
            #endregion

            #region 读取预设字体
            System.Drawing.Text.InstalledFontCollection installFonts = new System.Drawing.Text.InstalledFontCollection();

            List<string> fontNameList = new List<string>();
            string annoRuleFilePath = GApplication.Application.Template.Root + @"\" + GApplication.Application.Template.Content.Element("AnnoFull").Value;
            if (File.Exists(annoRuleFilePath))
            {
                DataTable fontMappingTable = CommonMethods.ReadToDataTable(annoRuleFilePath, "字体映射");
                for (int j = 0; j < fontMappingTable.Rows.Count; j++)
                {
                    DataRow row = fontMappingTable.Rows[j];
                    string fontName = row["目标字体名"].ToString();

                    bool fontExist = false;
                    foreach (System.Drawing.FontFamily ff in installFonts.Families)
                    {
                        if (ff.Name == fontName)
                        {
                            fontExist = true;
                        }
                    }
                    if (!fontExist)
                    {
                        fontName = "宋体";//默认的话给宋体
                    }

                    if (!fontNameList.Contains(fontName))
                    {
                        fontNameList.Add(fontName);
                    }

                }
            }
            else
            {
                foreach (System.Drawing.FontFamily ff in installFonts.Families)
                {
                    fontNameList.Add(ff.Name);
                }
            }

            if (fontNameList.Count > 0)
            {
                cbFontFamily.Items.AddRange(fontNameList.Distinct().ToArray());
            }
            #endregion
        }
        /// <summary>
        /// 调色板事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnColor_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ConvertColorToESRIColor(btn.BackColor);

            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;

            var clrPalette = new ColorPalette();
            if (clrPalette.TrackPopupMenu(ref tagRect, color, false, _hWnd))
            {
                btn.BackColor = ConvertESRIColorToColor(clrPalette.Color);
                if (btn.Name == "btnAnnoColor")
                {
                    this.cbAnnoCMYKColor.Text = GetCMYKTextByColor(clrPalette.Color);
                }
                else if (btn.Name == "btnMaskColor")
                {
                    this.cbMaskCMYKColor.Text = GetCMYKTextByColor(clrPalette.Color);
                }
            }
        }

        /// <summary>
        /// 根据颜色对象得到CMYK字符串
        /// </summary>
        /// <param name="color">IColor颜色值</param>
        /// <returns>CMYK字符串（形如：C100M200Y100K50）</returns>
        private string GetCMYKTextByColor(IColor color)
        {
            ICmykColor cmykColor = new CmykColorClass { CMYK = color.CMYK };
            string cmykString = string.Empty;
            if (cmykColor.Cyan != 0)
                cmykString += "C" + cmykColor.Cyan.ToString();
            if (cmykColor.Magenta != 0)
                cmykString += "M" + cmykColor.Magenta.ToString();
            if (cmykColor.Yellow != 0)
                cmykString += "Y" + cmykColor.Yellow.ToString();
            if (cmykColor.Black != 0)
                cmykString += "K" + cmykColor.Black.ToString();
            return cmykString == string.Empty ? "C0M0Y0K0" : cmykString;
        }
        /// <summary>
        /// 根据CMYK字符串得到CMYK颜色值
        /// </summary>
        /// <param name="cmyk">cmyk字符串（形如：C100M200Y100K50）</param>
        /// <returns>CMYK颜色值</returns>
        private IColor GetColorByCMYKText(string cmyk)
        {
            if (cmyk.Trim() == "")
                return null;

            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;
            try
            {
                for (int i = 0; i <= cmyk.Length; i++)
                {
                    if (i == cmyk.Length)
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('C'))
                        {
                            CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('M'))
                        {
                            CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('Y'))
                        {
                            CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('K'))
                        {
                            CMYK_Color.Black = int.Parse(sbs.Substring(1));
                        }
                        break;
                    }
                    else
                    {
                        char C = cmyk[i];
                        if (D.Contains(C))
                        {
                            sb.Append(C);
                        }
                        else
                        {
                            string sbs = sb.ToString();
                            if (sbs.Contains('C'))
                            {
                                CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('M'))
                            {
                                CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('Y'))
                            {
                                CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('K'))
                            {
                                CMYK_Color.Black = int.Parse(sbs.Substring(1));
                            }
                            sb.Clear();
                            sb.Append(C);
                        }
                    }
                }
                return CMYK_Color;
            }
            catch
            {
                return null;
            }

        }
        /// <summary>
        /// IColor转Color
        /// </summary>
        /// <param name="pRgbColor"></param>
        /// <returns></returns>
        private Color ConvertESRIColorToColor(IColor rgbColor)
        {
            return ColorTranslator.FromOle(rgbColor.RGB);
        }
        /// <summary>
        /// Color转IColor
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private IColor ConvertColorToESRIColor(Color color)
        {
            IColor clr = new RgbColorClass();
            clr.RGB = color.B * 65536 + color.G * 256 + color.R;
            return clr;
        }

    }
}

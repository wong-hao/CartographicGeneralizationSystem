using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class AnnotationClassifiedModifyForm : Form
    {
        public IFeatureLayer ObjAnnoLayer
        {
            get
            {
                if (cbAnnotationLayer.SelectedItem == null)
                    return null;

                return ((KeyValuePair<IFeatureLayer, string>)cbAnnotationLayer.SelectedItem).Key;
            }

        }

        public string AnnoSelectSQL
        {
            get
            {
                return string.Format("{0} = '{1}'", _typeFN, cbAnnotationType.Text);
            }
        }

        public AnnotationAttribute OutAnnotationAttribute
        {
            get
            {
                return _outAnnotationAttribute;
            }
        }
        private AnnotationAttribute _outAnnotationAttribute;

        public bool EnableModifyStyle
        {
            get
            {
                return cbStyle.Checked;
            }
        }

        public bool EnableModifyAlign
        {
            get
            {
                return cbAlign.Checked;
            }
        }

        public bool EnableModifyBackGround
        {
            get
            {
                return cbBackGround.Checked;
            }
        }

        public bool EnableModifyMask
        {
            get
            {
                return cbMask.Checked;
            }
        }

        private int _hWnd;
        private string _typeFN;
        public AnnotationClassifiedModifyForm(int hWnd, string typeFN = "分类")
        {
            InitializeComponent();

            _hWnd = hWnd;
            _typeFN = typeFN;
            _outAnnotationAttribute = null;
        }

        private void AnnotationClassifiedModifyForm_Load(object sender, EventArgs e)
        {
            #region 初始化注记图层
            cbAnnotationLayer.ValueMember = "Key";
            cbAnnotationLayer.DisplayMember = "Value";
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IFDOGraphicsLayer; })).ToArray();
            foreach (var lyr in lyrs)
            {
                int typeIndex = (lyr as IFeatureLayer).FeatureClass.FindField(_typeFN);
                if (typeIndex == -1)
                    continue;

                cbAnnotationLayer.Items.Add(new KeyValuePair<IFeatureLayer, string>(lyr as IFeatureLayer, lyr.Name));
            }
            #endregion


            #region 获取系统字体
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily ff in fonts.Families)
            {
                cbFontFamily.Items.Add(ff.Name);
            }
            #endregion

            #region 对齐方式
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
            #endregion


            #region 初始化注记背景组件
            cb_Balloon.ValueMember = "Key";
            cb_Balloon.DisplayMember = "Value";
            cb_Balloon.Items.Add(new KeyValuePair<esriBalloonCalloutStyle, string>(esriBalloonCalloutStyle.esriBCSRectangle, "矩形"));
            cb_Balloon.Items.Add(new KeyValuePair<esriBalloonCalloutStyle, string>(esriBalloonCalloutStyle.esriBCSRoundedRectangle, "圆角矩形"));
            #endregion

            #region 初始化蒙版组件
            cb_Mask.ValueMember = "Key";
            cb_Mask.DisplayMember = "Value";
            cb_Mask.Items.Add(new KeyValuePair<esriMaskStyle, string>(esriMaskStyle.esriMSNone, "无"));
            cb_Mask.Items.Add(new KeyValuePair<esriMaskStyle, string>(esriMaskStyle.esriMSHalo, "光晕"));
            #endregion

            if (cbAnnotationLayer.Items.Count == 0)
            {
                updateAnnoControl();
            }
            cbAnnotationLayer.SelectedIndex = cbAnnotationLayer.Items.Count - 1;
        }

        private void cbAnnotationLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbAnnotationType.Items.Clear();

            if (ObjAnnoLayer == null)
                return;

            int typeIndex = ObjAnnoLayer.FeatureClass.FindField(_typeFN);
            if (typeIndex != -1)
            {
                //分类的唯一值
                IQueryFilter qf = new QueryFilterClass();
                qf.SubFields = _typeFN;
                IFeatureCursor feCursor = ObjAnnoLayer.Search(qf, true);
                IDataStatistics dataStatistics = new DataStatisticsClass();
                dataStatistics.Field = _typeFN;//获取统计字段
                dataStatistics.Cursor = feCursor as ICursor;
                var enumerator = dataStatistics.UniqueValues;
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != DBNull.Value)
                    {
                        string valuet = enumerator.Current.ToString();
                        cbAnnotationType.Items.Add(valuet);
                    }
                   
                }
                Marshal.ReleaseComObject(feCursor);
            }
            if (cbAnnotationType.Items.Count == 0)
            {
                updateAnnoControl();

                return;
            }
            cbAnnotationType.SelectedIndex = 0;
        }

        private void cbAnnotationType_SelectedIndexChanged(object sender, EventArgs e)
        {

            //获取当前注记类型属性
            Dictionary<AnnotationAttribute, int> annoAttr2Count = new Dictionary<AnnotationAttribute, int>();

            if (ObjAnnoLayer != null && cbAnnotationType.Text != "")
            {
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = AnnoSelectSQL;
                IFeatureCursor feCursor = ObjAnnoLayer.Search(qf, true);
                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    IAnnotationFeature annoFe = fe as IAnnotationFeature;

                    AnnotationAttribute attri = GetAnnoFeatureAttribute(annoFe);

                    bool newAttri = true;
                    foreach (var kv in annoAttr2Count)
                    {
                        if (attri.AttributeEquals(kv.Key))
                        {
                            newAttri = false;
                            annoAttr2Count[kv.Key] += 1;
                            break;
                        }
                    }
                    if (newAttri)
                    {
                        annoAttr2Count.Add(attri, 1);
                    }
                }
                Marshal.ReleaseComObject(feCursor);
            }

            //更新控件
            updateAnnoControl(annoAttr2Count);
        }

        private void btnAnnoColor_Click(object sender, EventArgs e)
        {
            var location = getSubControlLocationInForm(btnAnnoColor);
            location.Offset(this.Location);//相对于_hWnd
            int frmLeftBorderWidth = (this.Width - this.ClientRectangle.Width) / 2;//窗体左侧边框宽度
            int frmTopBorderHeight = (this.Height - this.ClientRectangle.Height) - frmLeftBorderWidth;//窗体顶部边框高度
            location.X += frmLeftBorderWidth;
            location.Y += frmTopBorderHeight;

            tagRECT tagRect = new tagRECT();
            tagRect.left = location.X;
            tagRect.top = location.Y;
            tagRect.right = location.X + btnAnnoColor.Width;
            tagRect.bottom = location.Y + btnAnnoColor.Height;

            IColor color = GetEsriColorByCMYKText(lbAnnoColorCMYK.Text);
            var colorPalette = new ColorPalette();
            if (colorPalette.TrackPopupMenu(ref tagRect, color, true, _hWnd))
            {
                btnAnnoColor.BackColor = GetColorByEsriColor(colorPalette.Color);
                lbAnnoColorCMYK.Text = GetCMYKTextByEsriColor(colorPalette.Color);
            }
        }

        private void cbStyle_CheckedChanged(object sender, EventArgs e)
        {
            gbStyle.Enabled = cbStyle.Checked;
        }

        private void cbAlign_CheckedChanged(object sender, EventArgs e)
        {
            gb_TextAlign.Enabled = cbAlign.Checked;
        }

        private void cbBackGround_CheckedChanged(object sender, EventArgs e)
        {
            gb_Balloon.Enabled = cbBackGround.Checked;
        }

        private void btBackgroudCol_Click(object sender, EventArgs e)
        {
            var location = getSubControlLocationInForm(btBackgroudCol);
            location.Offset(this.Location);//相对于_hWnd
            int frmLeftBorderWidth = (this.Width - this.ClientRectangle.Width) / 2;//窗体左侧边框宽度
            int frmTopBorderHeight = (this.Height - this.ClientRectangle.Height) - frmLeftBorderWidth;//窗体顶部边框高度
            location.X += frmLeftBorderWidth;
            location.Y += frmTopBorderHeight;

            tagRECT tagRect = new tagRECT();
            tagRect.left = location.X;
            tagRect.top = location.Y;
            tagRect.right = location.X + btBackgroudCol.Width;
            tagRect.bottom = location.Y + btBackgroudCol.Height;

            IColor color = GetEsriColorByCMYKText(lbBackgroundColorCMYK.Text);
            var colorPalette = new ColorPalette();
            if (colorPalette.TrackPopupMenu(ref tagRect, color, true, _hWnd))
            {
                btBackgroudCol.BackColor = GetColorByEsriColor(colorPalette.Color);
                lbBackgroundColorCMYK.Text = GetCMYKTextByEsriColor(colorPalette.Color);
            }
        }

        private void btLineColor_Click(object sender, EventArgs e)
        {
            var location = getSubControlLocationInForm(btLineColor);
            location.Offset(this.Location);//相对于_hWnd
            int frmLeftBorderWidth = (this.Width - this.ClientRectangle.Width) / 2;//窗体左侧边框宽度
            int frmTopBorderHeight = (this.Height - this.ClientRectangle.Height) - frmLeftBorderWidth;//窗体顶部边框高度
            location.X += frmLeftBorderWidth;
            location.Y += frmTopBorderHeight;

            tagRECT tagRect = new tagRECT();
            tagRect.left = location.X;
            tagRect.top = location.Y;
            tagRect.right = location.X + btLineColor.Width;
            tagRect.bottom = location.Y + btLineColor.Height;

            IColor color = GetEsriColorByCMYKText(lbBackgroundBdColorCMYK.Text);
            var colorPalette = new ColorPalette();
            if (colorPalette.TrackPopupMenu(ref tagRect, color, true, _hWnd))
            {
                btLineColor.BackColor = GetColorByEsriColor(colorPalette.Color);
                lbBackgroundBdColorCMYK.Text = GetCMYKTextByEsriColor(colorPalette.Color);
            }
        }

        private void btnMaskColor_Click(object sender, EventArgs e)
        {
            var location = getSubControlLocationInForm(btnMaskColor);
            location.Offset(this.Location);//相对于_hWnd
            int frmLeftBorderWidth = (this.Width - this.ClientRectangle.Width) / 2;//窗体左侧边框宽度
            int frmTopBorderHeight = (this.Height - this.ClientRectangle.Height) - frmLeftBorderWidth;//窗体顶部边框高度
            location.X += frmLeftBorderWidth;
            location.Y += frmTopBorderHeight;

            tagRECT tagRect = new tagRECT();
            tagRect.left = location.X;
            tagRect.top = location.Y;
            tagRect.right = location.X + btnMaskColor.Width;
            tagRect.bottom = location.Y + btnMaskColor.Height;
            
            IColor color = GetEsriColorByCMYKText(lbMaskColorCMYK.Text);
            var colorPalette = new ColorPalette();
            if (colorPalette.TrackPopupMenu(ref tagRect, color, true, _hWnd))
            {
                btnMaskColor.BackColor = GetColorByEsriColor(colorPalette.Color);
                lbMaskColorCMYK.Text = GetCMYKTextByEsriColor(colorPalette.Color);
            }
        }

        private void cbMask_CheckedChanged(object sender, EventArgs e)
        {
            gb_Mask.Enabled = cbMask.Checked;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            #region 参数合法性检查
            if (cbAlign.Checked)
            {
                if (cb_Horizontal.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("请设置水平对齐方法！");
                    return;
                }
                if (cb_Vertical.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("请设置垂直对齐方法！");
                    return;
                }
            }

            if (cbBackGround.Checked && cbEnableBackgroud.Checked)
            {
                if (cb_Balloon.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("请设置文本背景样式！");
                    return;
                }
                if (lbBackgroundColorCMYK.Text == "" && lbBackgroundBdColorCMYK.Text == "")
                {
                    MessageBox.Show("请至少设置文本背景填充颜色或文本背景轮廓填充颜色中的一项！");
                    return;
                }
            }

            if (cbMask.Checked)
            {
                if (cbMask.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("请设置蒙版样式！");
                    return;
                }
                var maskStyle = ((KeyValuePair<esriMaskStyle, string>)cb_Mask.SelectedItem).Key;
                if (lbMaskColorCMYK.Text == "" && maskStyle != esriMaskStyle.esriMSNone)
                {
                    MessageBox.Show("请设置蒙版颜色！");
                    return;
                }
            }
            #endregion

            _outAnnotationAttribute = new AnnotationAttribute();
            #region 属性赋值
            _outAnnotationAttribute.FontName = cbFontFamily.Text;

            _outAnnotationAttribute.FontSize = (double)this.num_fontSize.Value;

            _outAnnotationAttribute.FontColor = GetEsriColorByCMYKText(lbAnnoColorCMYK.Text);

            if (cbStyle.Checked)
            {
                _outAnnotationAttribute.EnableBold = cbBold.Checked;
            }

            if (cbAlign.Checked)
            {
                _outAnnotationAttribute.HorizontalAlignment = ((KeyValuePair<esriTextHorizontalAlignment, string>)cb_Horizontal.SelectedItem).Key;
                _outAnnotationAttribute.VerticalAlignment = ((KeyValuePair<esriTextVerticalAlignment, string>)cb_Vertical.SelectedItem).Key;
            }

            //注记背景
            if (cbBackGround.Checked)
            {
                _outAnnotationAttribute.EnableTextBackground = cbEnableBackgroud.Checked;
                if (_outAnnotationAttribute.EnableTextBackground)
                {
                    //背景
                    _outAnnotationAttribute.BackgroundStyle = ((KeyValuePair<esriBalloonCalloutStyle, string>)cb_Balloon.SelectedItem).Key;
                    _outAnnotationAttribute.BackgroundColor = GetEsriColorByCMYKText(lbBackgroundColorCMYK.Text);

                    //轮廓
                    _outAnnotationAttribute.BackgroundBorderWidth = (double)this.num_LineWidth.Value;
                    _outAnnotationAttribute.BackgroundBorderColor = GetEsriColorByCMYKText(lbBackgroundBdColorCMYK.Text);

                    //边框
                    _outAnnotationAttribute.BackgroundMarginUp = (double)this.num_up.Value;
                    _outAnnotationAttribute.BackgroundMarginDown = (double)this.num_down.Value;
                    _outAnnotationAttribute.BackgroundMarginLeft = (double)this.num_left.Value;
                    _outAnnotationAttribute.BackgroundMarginRight = (double)this.num_right.Value;
                }
            }

            //文字蒙版
            if (cbMask.Checked)
            {
                _outAnnotationAttribute.MaskStyle = ((KeyValuePair<esriMaskStyle, string>)cb_Mask.SelectedItem).Key;
                _outAnnotationAttribute.MaskSize = (double)num_Mask.Value;
                _outAnnotationAttribute.MaskColor = GetEsriColorByCMYKText(lbMaskColorCMYK.Text);
            }
            #endregion

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }


        private void updateAnnoControl(Dictionary<AnnotationAttribute, int> annoAttr2Count = null)
        {
            if (annoAttr2Count == null || annoAttr2Count.Count == 0)
            {
                cbFontFamily.SelectedIndex = -1;
                num_fontSize.Value = 0;

                btnAnnoColor.BackColor = Color.Transparent;
                lbAnnoColorCMYK.Text = "";

                cbBold.Checked = false;

                cbAlign.Checked = false;
                cbBackGround.Checked = false;
                cbMask.Checked = false;
            }
            else
            {
                annoAttr2Count = annoAttr2Count.OrderByDescending(o => o.Value).ToDictionary(p => p.Key, o => o.Value);//按value降序排列
                AnnotationAttribute firstAttri = annoAttr2Count.First().Key;

                #region 基本信息
                string fontName = "";
                foreach (var kv in annoAttr2Count)
                {
                    if (fontName == "")
                    {
                        fontName = kv.Key.FontName;
                        continue;
                    }

                    if (fontName != kv.Key.FontName)
                    {
                        fontName = "";
                        break;
                    }
                }
                if (fontName == "")
                {
                    cbFontFamily.SelectedIndex = -1;
                }
                else
                {
                    cbFontFamily.Text = fontName;
                }

                double fontSize = 0;
                foreach (var kv in annoAttr2Count)
                {
                    if (fontSize == 0)
                    {
                        fontSize = kv.Key.FontSize;
                        continue;
                    }

                    if (Math.Abs(fontSize - kv.Key.FontSize) > 0.01)//小数点后两位
                    {
                        fontSize = 0;
                        break;
                    }
                }
                num_fontSize.Value = (decimal)fontSize;

                IColor fontColor = null;
                foreach (var kv in annoAttr2Count)
                {
                    if (kv.Key.FontColor == null)
                        continue;

                    if (fontColor == null)
                    {
                        fontColor = kv.Key.FontColor;
                        continue;
                    }

                    if (fontColor.CMYK != kv.Key.FontColor.CMYK)
                    {
                        fontColor = null;
                        break;
                    }
                }
                if (fontColor == null)
                {
                    btnAnnoColor.BackColor = Color.Transparent;
                    lbAnnoColorCMYK.Text = "";
                }
                else
                {
                    btnAnnoColor.BackColor = GetColorByEsriColor(fontColor);
                    lbAnnoColorCMYK.Text = GetCMYKTextByEsriColor(fontColor);
                }
                #endregion

                #region 样式
                cbBold.Checked = firstAttri.EnableBold;
                #endregion

                #region 对齐方式
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

                #region 文本背景
                cbEnableBackgroud.Checked = firstAttri.EnableTextBackground;

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

                if (firstAttri.BackgroundColor != null)
                {
                    btBackgroudCol.BackColor = GetColorByEsriColor(firstAttri.BackgroundColor);
                    lbBackgroundColorCMYK.Text = GetCMYKTextByEsriColor(firstAttri.BackgroundColor);
                }
                else
                {
                    btBackgroudCol.BackColor = Color.Transparent;
                    lbBackgroundColorCMYK.Text = "";
                }

                num_LineWidth.Value = (decimal)firstAttri.BackgroundBorderWidth;

                if (firstAttri.BackgroundBorderColor != null)
                {
                    btLineColor.BackColor = GetColorByEsriColor(firstAttri.BackgroundBorderColor);
                    lbBackgroundBdColorCMYK.Text = GetCMYKTextByEsriColor(firstAttri.BackgroundBorderColor);
                }
                else
                {
                    btLineColor.BackColor = Color.Transparent;
                    lbBackgroundBdColorCMYK.Text = "";
                }

                num_up.Value = (decimal)firstAttri.BackgroundMarginUp;
                num_down.Value = (decimal)firstAttri.BackgroundMarginDown;
                num_left.Value = (decimal)firstAttri.BackgroundMarginLeft;
                num_right.Value = (decimal)firstAttri.BackgroundMarginRight;
                #endregion

                #region 蒙板
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
                    btnMaskColor.BackColor = GetColorByEsriColor(firstAttri.MaskColor);
                    lbMaskColorCMYK.Text = GetCMYKTextByEsriColor(firstAttri.MaskColor);
                }
                else
                {
                    btnMaskColor.BackColor = Color.Transparent;
                    lbMaskColorCMYK.Text = "";
                }
                #endregion
            }

            //要素个数
            int groupCount = 0, feCount=0;
            if(annoAttr2Count != null)
            {
                groupCount = annoAttr2Count.Count;
                foreach (var kv in annoAttr2Count)
                {
                    feCount += kv.Value;
                }
            }
            lbAnnoFeatureCount.Text = string.Format("本类型注记属性有{0}组，共有注记要素{1}个", groupCount, feCount);
        }

        private Color GetColorByEsriColor(IColor color)
        {
            if (color.NullColor)
                return Color.Transparent;

            return ColorTranslator.FromOle(color.RGB);
        }
        /// <summary>
        /// 根据esri颜色对象得到CMYK字符串
        /// </summary>
        /// <param name="color">颜色对象</param>
        /// <returns>CMYK字符串（形如：C100M200Y100K50）</returns>
        private string GetCMYKTextByEsriColor(IColor color)
        {
            ICmykColor cmykColor = new CmykColorClass { CMYK = color.CMYK };
            string cmykString = string.Empty;
            if (color.NullColor)
                return cmykString;

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
        private IColor GetEsriColorByCMYKText(string cmyk)
        {
            if (cmyk.Trim() == "")
                return new CmykColorClass() { NullColor = true };

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
                return new CmykColorClass() { NullColor = true };
            }

        }

        /// <summary>
        /// 获取注记要素属性
        /// </summary>
        /// <param name="annoFe"></param>
        /// <returns></returns>
        private AnnotationAttribute GetAnnoFeatureAttribute(IAnnotationFeature annoFe)
        {
            AnnotationAttribute attri = new AnnotationAttribute();

            ITextElement textElement = annoFe.Annotation as ITextElement;
            attri.Text = textElement.Text;
            attri.FontName = textElement.Symbol.Font.Name;
            attri.FontSize = textElement.Symbol.Size / 2.8345;
            attri.FontColor = textElement.Symbol.Color;

            attri.EnableBold = (textElement as ISymbolCollectionElement).Bold;
            attri.HorizontalAlignment = (textElement.Symbol as IFormattedTextSymbol).HorizontalAlignment;
            attri.VerticalAlignment = (textElement.Symbol as IFormattedTextSymbol).VerticalAlignment;


            attri.EnableTextBackground = ((textElement.Symbol as IFormattedTextSymbol).Background != null);
            if (attri.EnableTextBackground)
            {
                IBalloonCallout balloonCallout = (textElement.Symbol as IFormattedTextSymbol).Background as IBalloonCallout;
                if (balloonCallout != null)
                {
                    attri.BackgroundStyle = balloonCallout.Style;
                    attri.BackgroundColor = balloonCallout.Symbol.Color;
                    attri.BackgroundBorderWidth = balloonCallout.Symbol.Outline.Width;
                    attri.BackgroundBorderColor = balloonCallout.Symbol.Outline.Color;
                    attri.BackgroundMarginLeft = (balloonCallout as ITextMargins).LeftMargin;
                    attri.BackgroundMarginUp = (balloonCallout as ITextMargins).TopMargin;
                    attri.BackgroundMarginRight = (balloonCallout as ITextMargins).RightMargin;
                    attri.BackgroundMarginDown = (balloonCallout as ITextMargins).BottomMargin;
                }
            }

            attri.MaskStyle = (textElement.Symbol as IMask).MaskStyle;
            attri.MaskSize = (textElement.Symbol as IMask).MaskSize;
            if ((textElement.Symbol as IMask).MaskSymbol != null)
                attri.MaskColor = (textElement.Symbol as IMask).MaskSymbol.Color;

            return attri;
        }

        /// <summary>
        /// 获取子控件在窗体中的位置（屏幕坐标）
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private Point getSubControlLocationInForm(Control c)
        {
            Point location = new Point(0, 0);
            do
            {
                location.Offset(c.Location);
                c = c.Parent;
            }
            while (c != this && c != null);//循环到当前窗体

            return location;
        }
    }
}

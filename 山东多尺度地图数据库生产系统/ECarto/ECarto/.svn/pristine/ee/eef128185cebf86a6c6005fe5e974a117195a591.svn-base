using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using System.Xml.Linq;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class AnnoAttribute : Form
    {
        int hWnd = 0;
        Dictionary<string, string> CommonFonts = new Dictionary<string, string>();
        List<string> ztdz=new List<string>();
        IColorPalette colorPalette;
        IPoint annoPoint = null;
        public DataRow AnnoRuleRow = null;//选择的注记规则
        /// <summary>
        /// 输入的样式
        /// </summary>
        public AnnoStyle InAnnoStyle=null;
        /// <summary>
        /// 输出的样式
        /// </summary>
        public AnnoStyle OutAnnoStyle;

        //文本背景颜色
        private string bgColorText;
        private string bgLineColorText;

        //对齐、气泡、样式
        Dictionary<string, esriTextHorizontalAlignment> str2Horizontal = new Dictionary<string, esriTextHorizontalAlignment>();
        Dictionary<string, esriTextVerticalAlignment> str2Vertical = new Dictionary<string, esriTextVerticalAlignment>();
        Dictionary<string, esriBalloonCalloutStyle> str2BallonCallout = new Dictionary<string, esriBalloonCalloutStyle>();
        Dictionary<string, esriMaskStyle> str2MaskStyle = new Dictionary<string, esriMaskStyle>();
        public string GetDicKeyByValue(Dictionary<string, esriTextHorizontalAlignment> dic, esriTextHorizontalAlignment _value)
        {
            var keys = dic.Where(q => q.Value == _value).Select(q => q.Key);
            return keys.First();
        }
        public string GetDicKeyByValue(Dictionary<string, esriTextVerticalAlignment> dic, esriTextVerticalAlignment _value)
        {
            var keys = dic.Where(q => q.Value == _value).Select(q => q.Key);
            return keys.First();
        }
        public string GetDicKeyByValue(Dictionary<string, esriBalloonCalloutStyle> dic, esriBalloonCalloutStyle _value)
        {
            var keys = dic.Where(q => q.Value == _value).Select(q => q.Key);
            return keys.First();
        }
        public string GetDicKeyByValue(Dictionary<string, esriMaskStyle> dic, esriMaskStyle _value)
        {
            var keys = dic.Where(q => q.Value == _value).Select(q => q.Key);
            return keys.First();
        }
        /// <summary>
        /// 修改注记属性窗
        /// </summary>
        /// <param name="_hWnd"></param>
        /// <param name="_mdbPath"></param>
        /// <param name="curlyrName"></param>
        public AnnoAttribute(int _hWnd,string[] lyrs,IPoint anchor,DataRow rowRule=null)
        {
            InitializeComponent();
            hWnd = _hWnd;
            this.comboBoxLyr.Items.AddRange(lyrs);
            this.comboBoxLyr.SelectedIndex = this.comboBoxLyr.Items.IndexOf("ANNO");
            if (this.comboBoxLyr.SelectedIndex == -1)
                this.comboBoxLyr.SelectedIndex = this.comboBoxLyr.Items.IndexOf("注记");
            annoPoint = anchor;
            #region 对齐方式
            str2Horizontal.Add("左对齐", esriTextHorizontalAlignment.esriTHALeft);
            str2Horizontal.Add("居中对齐",esriTextHorizontalAlignment.esriTHACenter);
            str2Horizontal.Add("右对齐", esriTextHorizontalAlignment.esriTHARight);
            str2Horizontal.Add("两端对齐",esriTextHorizontalAlignment.esriTHAFull);
            str2Vertical.Add("顶部对齐", esriTextVerticalAlignment.esriTVATop);
            str2Vertical.Add("居中对齐", esriTextVerticalAlignment.esriTVACenter);
            str2Vertical.Add("基线对齐", esriTextVerticalAlignment.esriTVABaseline);
            str2Vertical.Add("底部对齐", esriTextVerticalAlignment.esriTVABottom);
            #endregion
            cb_Horizontal.SelectedIndex = 1;
            cb_Vertical.SelectedIndex = 1;
            #region 背景样式
            #region 气泡样式
            str2BallonCallout.Add("矩形", esriBalloonCalloutStyle.esriBCSRectangle);
            str2BallonCallout.Add("圆角矩形", esriBalloonCalloutStyle.esriBCSRoundedRectangle);
            //str2BallonCallout.Add("椭圆", esriBalloonCalloutStyle.esriBCSOval);
            #endregion
           
            #region 文字蒙版
            str2MaskStyle.Add("无", esriMaskStyle.esriMSNone);
            str2MaskStyle.Add("光晕", esriMaskStyle.esriMSHalo);
        
            #endregion
            this.cb_Mask.SelectedIndex = 0;
            #endregion
            comboBoxFont.SelectedIndex = 0;
            InitialParams();
            AnnoRuleRow = rowRule;
            if (rowRule != null)
            {
                SetAnnoRuleParms(rowRule);
            }
        }
        /// <summary>
        /// 添加注记
        /// </summary>
        /// <param name="_hWnd"></param>
        /// <param name="inAnoStyle"></param>
        public AnnoAttribute(int _hWnd, AnnoStyle inAnoStyle)
        {
            InitializeComponent();
            hWnd = _hWnd;

            InAnnoStyle = inAnoStyle;

            this.comboBoxLyr.Items.Add(inAnoStyle.LyrName);
            this.comboBoxLyr.Text = inAnoStyle.LyrName;
            this.cbBold.Checked = inAnoStyle.FontBold;
            #region 对齐方式
            str2Horizontal.Add("左对齐", esriTextHorizontalAlignment.esriTHALeft);
            str2Horizontal.Add("居中对齐", esriTextHorizontalAlignment.esriTHACenter);
            str2Horizontal.Add("右对齐", esriTextHorizontalAlignment.esriTHARight);
            str2Horizontal.Add("两端对齐", esriTextHorizontalAlignment.esriTHAFull);
            str2Vertical.Add("顶部对齐", esriTextVerticalAlignment.esriTVATop);
            str2Vertical.Add("居中对齐", esriTextVerticalAlignment.esriTVACenter);
            str2Vertical.Add("基线对齐", esriTextVerticalAlignment.esriTVABaseline);
            str2Vertical.Add("底部对齐", esriTextVerticalAlignment.esriTVABottom);
            #endregion
            this.cb_Horizontal.Text = GetDicKeyByValue(str2Horizontal, inAnoStyle.Horizontal);
            this.cb_Vertical.Text = GetDicKeyByValue(str2Vertical, inAnoStyle.Vertical);
            #region 背景样式
            #region 气泡样式
            str2BallonCallout.Add("矩形", esriBalloonCalloutStyle.esriBCSRectangle);
            str2BallonCallout.Add("圆角矩形", esriBalloonCalloutStyle.esriBCSRoundedRectangle);
            //str2BallonCallout.Add("椭圆", esriBalloonCalloutStyle.esriBCSOval);
          
            #endregion
            this.checkBall.Checked = inAnoStyle.HasMask;
            if (!inAnoStyle.HasMask)
            {
                this.gb_Balloon.Enabled = false;
            }
            else
            {
                this.cb_Balloon.Text = GetDicKeyByValue(str2BallonCallout, inAnoStyle.BalloonCallout);
                this.outLineWidth.Value = (decimal)inAnoStyle.TextMarginsUp;
                this.num_down.Value = (decimal)inAnoStyle.TextMarginsDown;
                this.num_left.Value = (decimal)inAnoStyle.TextMarginsLeft;
                this.num_right.Value = (decimal)inAnoStyle.TextMarginsRight;
                //填充颜色
                IColor color = GetColorByString(inAnoStyle.MaskColorCMYK);
                this.btBackgroudCol.BackColor = ConvertIColorToColor(color);
                bgColorText = inAnoStyle.MaskColorCMYK;
                //轮廓属性
                num_LineWidth.Value = (decimal)inAnoStyle.LineWidth;
                color = GetColorByString(inAnoStyle.LineColorCMYK);
                this.btLineColor.BackColor = ConvertIColorToColor(color);
                bgLineColorText = inAnoStyle.LineColorCMYK;
            }
            #region 文字蒙版
            str2MaskStyle.Add("无", esriMaskStyle.esriMSNone);
            str2MaskStyle.Add("光晕", esriMaskStyle.esriMSHalo);

            #endregion
            this.cb_Mask.Text = GetDicKeyByValue(str2MaskStyle, inAnoStyle.TextMaskStyle);
            this.num_Mask.Value = (decimal)inAnoStyle.TextMaskSize;
            btn_MaskColor.BackColor = ConvertIColorToColor(GetColorByString(inAnoStyle.TextMaskColor));
            comboBoxMask.Text = inAnoStyle.TextMaskColor;
            #endregion
            this.comboBoxLyr.Enabled = false;
            InitialParams();
        }


        /// <summary>
        /// 初始化一些参数,获取制图字体
        /// </summary>
        private void InitialParams() {
            //初始化字体集
            CommonFonts = new Dictionary<string, string>();
            try
            {
                CommonFonts["黑体"] = "黑体";
                Dictionary<string, string> dic = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
                if (dic == null)
                {
                    dic = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
                }
                string baseMap = dic["BaseMap"];
                string template = dic["MapTemplate"];
                string mdbPath = GApplication.Application.Template.Root + "\\底图\\" + baseMap + "\\" + template + "\\规则对照.mdb";
                DataTable mapTable = CommonMethods.ReadToDataTable(mdbPath, "字体映射");
                for (int i = 0; i < mapTable.Rows.Count; i++)
                {
                    string fontString = mapTable.Rows[i]["国标字体"].ToString();
                    string replaceStr = mapTable.Rows[i]["替换字体"].ToString();
                    CommonFonts[fontString] = replaceStr;
                }
            }
            catch(Exception ex)
            {

                
            }
            this.cbFontFamily.Items.AddRange(CommonFonts.Keys.ToArray());
            //初始化调色板
            colorPalette = new ColorPalette();

          
            this.comboBoxFont.TextChanged += new EventHandler(comboBox_TextChanged);
            this.comboBoxMask.TextChanged += new EventHandler(comboBox_TextChanged);
           
            if (InAnnoStyle != null)
            {
                SetAnnoStyle(InAnnoStyle);
            }
            else
            {
                this.cbFontFamily.SelectedIndex = 0;
            }

        }


        /*说明：颜色下拉框和调色板按钮之间的逻辑关系为：调色板获取的颜色设置下拉框的显示文本
         * 通过显示文本值的改变事件去设置调色按钮的底色。
         */
        /// <summary>
        /// 下拉框事件,TextChanged和Leave采用这种方式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_TextChanged(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            IColor color = GetColorByString(combo.Text);
            if (color == null)
                return;
            if (combo.Name == "comboBoxFont")
            {
                this.BtnFontColor.BackColor = ConvertIColorToColor(color);
            }
            else
            {
                this.btn_MaskColor.BackColor = ConvertIColorToColor(color);
            }
        }

       
        /// <summary>
        /// 根据输入的注记样式，初始化界面值,并初始化输出的样式
        /// </summary>
        /// <param name="an"></param>
        private void SetAnnoStyle(AnnoStyle an) {
           
            //基本属性
            tbContent.Text = an.Text;
            var itemfont= CommonFonts.Where(t => t.Value == an.FontName).FirstOrDefault();
            string fontname = "";
            if (itemfont.Key != null)
            {
                fontname = itemfont.Key;
            }
            int index = cbFontFamily.Items.IndexOf(fontname);
            if (index != -1)
                cbFontFamily.SelectedIndex = index;
            nudFontSize.Value = Convert.ToDecimal(an.FontSize);
            //设置注记要素的字体颜色
            comboBoxFont.Text = an.FontColorCMYK;
           
        }
        
        private void BtnFontColor_Click(object sender, EventArgs e)
        {
            var location = getSubControlLocationInForm(BtnFontColor);
            location.Offset(this.Location);//相对于_hWnd
            int frmLeftBorderWidth = (this.Width - this.ClientRectangle.Width) / 2;//窗体左侧边框宽度
            int frmTopBorderHeight = (this.Height - this.ClientRectangle.Height) - frmLeftBorderWidth;//窗体顶部边框高度
            location.X += frmLeftBorderWidth;
            location.Y += frmTopBorderHeight;

            tagRECT tagRect = new tagRECT();
            tagRect.left = location.X;
            tagRect.top = location.Y;
            tagRect.right = location.X + BtnFontColor.Width;
            tagRect.bottom = location.Y + BtnFontColor.Height;

            IColor color = GetColorByString(comboBoxFont.Text);
            if (colorPalette.TrackPopupMenu(ref tagRect, color, true, this.hWnd))
            {
                BtnFontColor.BackColor = ConvertIColorToColor(colorPalette.Color);
                comboBoxFont.Text = GetStringByColor(colorPalette.Color);
            }
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

            IColor color = GetColorByString(bgColorText);
            if (colorPalette.TrackPopupMenu(ref tagRect, color, true, this.hWnd))
            {
                btBackgroudCol.BackColor = ConvertIColorToColor(colorPalette.Color);
                bgColorText = GetStringByColor(colorPalette.Color);
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

            IColor color = GetColorByString(bgLineColorText);
            if (colorPalette.TrackPopupMenu(ref tagRect, color, true, this.hWnd))
            {
                btLineColor.BackColor = ConvertIColorToColor(colorPalette.Color);
                bgLineColorText = GetStringByColor(colorPalette.Color);
            }
        }

        private void btn_MaskColor_Click(object sender, EventArgs e)
        {
            var location = getSubControlLocationInForm(btn_MaskColor);
            location.Offset(this.Location);//相对于_hWnd
            int frmLeftBorderWidth = (this.Width - this.ClientRectangle.Width) / 2;//窗体左侧边框宽度
            int frmTopBorderHeight = (this.Height - this.ClientRectangle.Height) - frmLeftBorderWidth;//窗体顶部边框高度
            location.X += frmLeftBorderWidth;
            location.Y += frmTopBorderHeight;

            tagRECT tagRect = new tagRECT();
            tagRect.left = location.X;
            tagRect.top = location.Y;
            tagRect.right = location.X + btn_MaskColor.Width;
            tagRect.bottom = location.Y + btn_MaskColor.Height;

            IColor color = GetColorByString(comboBoxMask.Text);
            if (colorPalette.TrackPopupMenu(ref tagRect, color, true, this.hWnd))
            {
                btn_MaskColor.BackColor = ConvertIColorToColor(colorPalette.Color);
                comboBoxMask.Text = GetStringByColor(colorPalette.Color);
            }
        }

        /// <summary>
        /// 确认提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, EventArgs e)
        {

            bool flag = ValueValidator();
            if (!flag)
                return;
            GetAnnoParmas();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
        private void btView_Click(object sender, EventArgs e)
        {
            bool flag = ValueValidator();
            if (!flag)
                return;
            GetAnnoParmas();
            if (InAnnoStyle == null)
            {

                AddAnno();
            }
            else
            {
                UpdateAnno();
            }
        }
        private void GetAnnoParmas()
        {
            OutAnnoStyle = new AnnoStyle();
            if (InAnnoStyle != null)
                OutAnnoStyle.Angle = InAnnoStyle.Angle;
            OutAnnoStyle.Text = tbContent.Text;
            OutAnnoStyle.FontSize = Convert.ToDouble(nudFontSize.Value);
            OutAnnoStyle.LyrName = comboBoxLyr.Text;
            OutAnnoStyle.FontName = cbFontFamily.Text;
            if (CommonFonts.ContainsKey(cbFontFamily.Text))
                OutAnnoStyle.FontName = CommonFonts[cbFontFamily.SelectedItem.ToString()];
            OutAnnoStyle.FontBold = cbBold.Checked;

            OutAnnoStyle.FontBold = cbBold.Checked;
            OutAnnoStyle.FontColorCMYK = comboBoxFont.Text;

            //文本对齐方式
            OutAnnoStyle.Horizontal = str2Horizontal[cb_Horizontal.Text];
            OutAnnoStyle.Vertical = str2Vertical[cb_Vertical.Text];

            //背景注记
            OutAnnoStyle.HasMask = checkBall.Checked;
            if (checkBall.Checked)
            {
                //背景
                OutAnnoStyle.MaskColorCMYK = bgColorText;
                OutAnnoStyle.BalloonCallout = str2BallonCallout[cb_Balloon.SelectedItem.ToString()];
                //轮廓
                OutAnnoStyle.LineColorCMYK = bgLineColorText;
                OutAnnoStyle.LineWidth = (double)this.num_LineWidth.Value;
                //边框
                OutAnnoStyle.TextMarginsUp = (double)this.outLineWidth.Value;
                OutAnnoStyle.TextMarginsDown = (double)this.num_down.Value;
                OutAnnoStyle.TextMarginsLeft = (double)this.num_left.Value;
                OutAnnoStyle.TextMarginsRight = (double)this.num_right.Value;
            }
            //文字蒙版
            OutAnnoStyle.TextMaskStyle = str2MaskStyle[cb_Mask.Text];
            OutAnnoStyle.TextMaskSize = (double)num_Mask.Value;
            OutAnnoStyle.TextMaskColor = comboBoxMask.Text;
          
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
        /// <summary>
        /// 值验证器
        /// </summary>
        private bool ValueValidator() {
            //if (this.tbContent.Text.Trim() == string.Empty)
            //{
            //    MessageBox.Show("注记文本不能为空！");
            //    return false;
            //}
            //if (this.cbFontFamily.Text == string.Empty)
            //{
            //    MessageBox.Show("请选择注记字体！");
            //    return false;
            //}
            //if (this.nudFontSize.Value == 0)
            //{
            //    MessageBox.Show("注记尺寸不能为0！");
            //    return false;
            //}
            if (this.comboBoxFont.Text.Trim()==string.Empty)
            {
                MessageBox.Show("请设置字体颜色");
                return false;
            }
            if (checkBall.Checked)
            {
                if(cb_Balloon.Text.Trim()==string.Empty)
                {
                    MessageBox.Show("请设置气泡样式");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// IColor转Color
        /// </summary>
        /// <param name="pRgbColor"></param>
        /// <returns></returns>
        private Color ConvertIColorToColor(IColor pRgbColor)
        {
            return ColorTranslator.FromOle(pRgbColor.RGB);
        }
        /// <summary>
        /// Color转IColor
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private IColor ConvertColorToIColor(Color color)
        {
            IColor pColor = new RgbColorClass();
            pColor.RGB = color.B * 65536 + color.G * 256 + color.R;
            return pColor;
        }

        /// <summary>
        /// 根据注记规则里的CMYK字符串得到CMYK颜色值
        /// </summary>
        /// <param name="cmyk">cmyk字符串（形如：C100M200Y100K50）</param>
        /// <returns>CMYK颜色值</returns>
        private IColor GetColorByString(string cmyk)
        {
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
        /// 根据CMYK颜色值得到CMYK字符串
        /// </summary>
        /// <param name="color">IColor颜色值</param>
        /// <returns>CMYK字符串（形如：C100M200Y100K50）</returns>
        private string GetStringByColor(IColor color)
        {
            ICmykColor cmykColor = new CmykColorClass { CMYK=color.CMYK };
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
       
        private void AnnoAttribute_Load(object sender, EventArgs e)
        {
            if (InAnnoStyle == null)
            {
                btView.Visible = false;
            }
        }

        private void checkBall_CheckedChanged(object sender, EventArgs e)
        {
            this.gb_Balloon.Enabled = checkBall.Checked;
        }

       
        private void AddAnno()
        {
            GApplication m_Application = GApplication.Application;
            m_Application.EngineEditor.EnableUndoRedo(true);
            //修改字体符号样式
            m_Application.EngineEditor.StartOperation();
            string message = string.Empty;
            AnnoStyle style = OutAnnoStyle;
            ITextElement textElement = AnnoFunc.CreateTextElement(annoPoint,
                style.Text,
                style.FontName,
                style.CharacterWidth,
                style.Itatic,
                style.FontSize,
                GetColorByString(style.FontColorCMYK),
                out message,
                style.FontBold,
                style.Horizontal,
                style.Vertical);
            if (textElement == null)
            {
                return;
            }
            IElement pElement = textElement as IElement;
            //
            IFeatureClass pFeatureClass = null;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return (l is IFDOGraphicsLayer) && (l as IFeatureLayer).FeatureClass.AliasName == style.LyrName; })).FirstOrDefault();
            if (lyrs != null)
            {
                pFeatureClass = (lyrs as IFeatureLayer).FeatureClass;
                //添加背景边框，如果有的话
                IElement newElement = pElement as IElement;
                //蒙版
                ITextSymbol textSymbol = textElement.Symbol;
                IMask textMask = textSymbol as IMask;
                textMask.MaskSize = style.TextMaskSize;
                textMask.MaskStyle = style.TextMaskStyle;
                ILineSymbol maskLineSymbol = new SimpleLineSymbol();
                maskLineSymbol.Width = 0;
                IFillSymbol maskSymbol = new SimpleFillSymbolClass();
                maskSymbol.Color = GetColorByString(style.TextMaskColor);
                maskSymbol.Outline = maskLineSymbol;
                textMask.MaskSymbol = maskSymbol;
                if (style.HasMask)//气泡背景
                {
                    //设置文本背景框样式
                    IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;
                    IBalloonCallout balloonCallout = new BalloonCalloutClass();
                    balloonCallout.Style = style.BalloonCallout;
                    //设置文本背景框颜色
                    IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                    fillSymbol.Color = GetColorByString(style.MaskColorCMYK);
                    ILineSymbol lineSymbol = new SimpleLineSymbol();
                    lineSymbol.Width = style.LineWidth;
                    lineSymbol.Color = GetColorByString(style.LineColorCMYK);
                    fillSymbol.Outline = lineSymbol;
                    balloonCallout.Symbol = fillSymbol;
                    //设置背景框边距
                    ITextMargins textMarigns = balloonCallout as ITextMargins;
                    textMarigns.PutMargins(style.TextMarginsLeft, style.TextMarginsUp, style.TextMarginsRight, style.TextMarginsDown);
                    formattedTextSymbol.Background = balloonCallout as ITextBackground;
                }
                textSymbol.Angle = style.Angle * 180 / Math.PI;//角度
                textElement.Symbol = textSymbol;

                IFeature pNewFeature = pFeatureClass.CreateFeature();
                IAnnotationFeature pNewAnnoFeature = pNewFeature as IAnnotationFeature;
                pNewAnnoFeature.Annotation = pElement;
                pNewFeature.Store();
                m_Application.EngineEditor.StopOperation("注记符号添加");
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, pNewFeature.Shape.Envelope);
            }
        }
       
        public Dictionary<string, Lyr_AnnoStyle> selectedFeaureDic = null;
        private void UpdateAnno()
        {
            GApplication m_Application = GApplication.Application;
            m_Application.EngineEditor.EnableUndoRedo(true);
            m_Application.EngineEditor.StartOperation();
            AnnoStyle commonStyle = InAnnoStyle;
            AnnoStyle updateStyle = OutAnnoStyle;
            IEnvelope refreshEnvelop = null;
            foreach (string featureIDClassName in selectedFeaureDic.Keys)
            {
                IFeatureLayer ppFeatureLayer = selectedFeaureDic[featureIDClassName].featureLayer;
                int featureID = Convert.ToInt32(featureIDClassName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                IFeature feature = ppFeatureLayer.FeatureClass.GetFeature(featureID);
                IAnnotationFeature2 annoFeature = feature as IAnnotationFeature2;
                int AnnotationClassID = annoFeature.AnnotationClassID;
                int FeatureID = annoFeature.LinkedFeatureID;
                //-----------------根据外包框和锚点以及位置点进行计算新的定位要素------
                AnnoStyle oldStyle = selectedFeaureDic[featureIDClassName].annoStyle;
                UpdateAnnoFeature(feature, oldStyle, updateStyle);
            }
            m_Application.EngineEditor.StopOperation("注记符号修改");
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, refreshEnvelop);
        }
        private void UpdateAnnoFeature(IFeature fe, AnnoStyle oldStyle, AnnoStyle newStyle)
        {
            IAnnotationFeature2 annoFeature = fe as IAnnotationFeature2;
            ITextElement textElement = annoFeature.Annotation as ITextElement;

            if (newStyle.Text.Trim() != "" && oldStyle.Text != newStyle.Text)
            {
                #region 文本
                textElement.Text = newStyle.Text;
                #endregion
            }

            if (newStyle.FontName != "" && oldStyle.FontName != newStyle.FontName)
            {
                #region 字体
                System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
                foreach (System.Drawing.FontFamily ff in fonts.Families)
                {
                    if (ff.Name == newStyle.FontName)
                    {
                        (textElement as ISymbolCollectionElement).FontName = newStyle.FontName;
                    }
                }
                #endregion
            }

            if (newStyle.FontSize > 0 && oldStyle.FontSize != newStyle.FontSize)
            {
                #region 字体大小
                (textElement as ISymbolCollectionElement).Size = newStyle.FontSize * 2.8345;
                #endregion
            }

            if (oldStyle.FontColorCMYK != newStyle.FontColorCMYK)
            {
                #region 字体颜色
                (textElement as ISymbolCollectionElement).Color = GetColorByString(newStyle.FontColorCMYK);
                #endregion
            }

            if (oldStyle.FontBold != newStyle.FontBold)
            {
                #region 加粗
                (textElement as ISymbolCollectionElement).Bold = newStyle.FontBold;
                #endregion
            }

            if (oldStyle.Horizontal != newStyle.Horizontal || oldStyle.Vertical != newStyle.Vertical)
            {
                #region 对齐方式
                (textElement as ISymbolCollectionElement).HorizontalAlignment = newStyle.Horizontal;
                (textElement as ISymbolCollectionElement).VerticalAlignment = newStyle.Vertical;
                #endregion
            }

            #region 注记背景
            if (newStyle.HasMask)
            {
                ITextSymbol textSymbol = textElement.Symbol;
                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;

                IBalloonCallout balloonCallout = new BalloonCalloutClass();
                balloonCallout.Style = newStyle.BalloonCallout;

                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = GetColorByString(newStyle.MaskColorCMYK);

                ILineSymbol lineSymbol = new SimpleLineSymbol();
                lineSymbol.Width = newStyle.LineWidth;
                lineSymbol.Color = GetColorByString(newStyle.LineColorCMYK);

                fillSymbol.Outline = lineSymbol;
                balloonCallout.Symbol = fillSymbol;


                ITextMargins textMarigns = balloonCallout as ITextMargins;
                textMarigns.PutMargins(newStyle.TextMarginsLeft, newStyle.TextMarginsUp, newStyle.TextMarginsRight, newStyle.TextMarginsDown);

                (textSymbol as IFormattedTextSymbol).Background = balloonCallout as ITextBackground;
                textElement.Symbol = textSymbol;
            }
            else
            {
                ITextSymbol textSymbol = textElement.Symbol;
                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;

                (textSymbol as IFormattedTextSymbol).Background = null;
                textElement.Symbol = textSymbol;
            }

            #endregion

            if (oldStyle.TextMaskStyle != newStyle.TextMaskStyle)
            {
                #region 蒙板类型
                ITextSymbol textSymbol = textElement.Symbol;
                (textSymbol as IMask).MaskStyle = newStyle.TextMaskStyle;

                textElement.Symbol = textSymbol;
                #endregion
            }

            if (oldStyle.TextMaskSize != newStyle.TextMaskSize)
            {
                #region 蒙板大小
                ITextSymbol textSymbol = textElement.Symbol;
                (textSymbol as IMask).MaskSize = newStyle.TextMaskSize;

                textElement.Symbol = textSymbol;
                #endregion
            }

            if (newStyle.HasMask && (oldStyle.TextMaskColor != newStyle.TextMaskColor))
            {
                #region 蒙板颜色
                ITextSymbol textSymbol = textElement.Symbol;
                if (textSymbol is IMask && (textSymbol as IMask).MaskSymbol != null)
                {
                    IFillSymbol fillSymbol = (textSymbol as IMask).MaskSymbol;
                    fillSymbol.Color = GetColorByString(newStyle.TextMaskColor);

                    (textSymbol as IMask).MaskSymbol = fillSymbol;
                    textElement.Symbol = textSymbol;
                }
                #endregion
            }

            annoFeature.Annotation = textElement as IElement;
            fe.Store();
        }
        private void btset_Click(object sender, EventArgs e)
        {
            var ruleset = new FrmAnnoRuleSet(AnnoRuleRow);
           if( ruleset.ShowDialog()==DialogResult.OK)
           {
               AnnoRuleRow = ruleset.targetAnnoRule;
               lbRule.Text = "注记规则：" + AnnoRuleRow["图层"] + ":" + AnnoRuleRow["注记说明"];
               SetAnnoRuleParms(AnnoRuleRow);
           }
        }
        private void SetAnnoRuleParms(DataRow row)
        {
            lbRule.Text = "注记规则：" + AnnoRuleRow["图层"] + ":" + AnnoRuleRow["注记说明"];
            this.cb_Mask.SelectedIndex = 0;
            cbBold.Checked = false;

            string fontsize = row["注记大小"].ToString().Trim();
            nudFontSize.Value = decimal.Parse(fontsize);
            string fontname = row["注记字体"].ToString().Trim();//国标字体
        
            cbFontFamily.SelectedIndex = (CommonFonts.Keys.ToList<string>()).IndexOf(fontname);
            string annocolor = row["注记颜色"].ToString().Trim();
            comboBoxFont.Text = annocolor;
            string annostyle = row["字体样式"].ToString().Trim();
            if (annostyle == "加粗")
            {
                cbBold.Checked = true;
            }

            string bubblebg = row["注记气泡颜色"].ToString().Trim();
            IColor color = GetColorByString(bubblebg);
            if (color != null)
            {
                btBackgroudCol.BackColor = ConvertIColorToColor(color);
                bgColorText = bubblebg;
            }
            checkBall.Checked = false;
            if (bubblebg != string.Empty)
            {
                checkBall.Checked = true;
                string bubbleshp = row["气泡形状"].ToString().Trim();
                cb_Balloon.SelectedIndex = 0;
                if (bubbleshp == "圆角")
                {
                    cb_Balloon.SelectedIndex = 1;
                }
                else if (bubblebg == "椭圆")
                {
                    cb_Balloon.SelectedIndex = 2;
                }
                string bublinecolor = row["气泡边线颜色"].ToString().Trim();
                color = GetColorByString(bublinecolor);
                if (color != null)
                {
                    btLineColor.BackColor = ConvertIColorToColor(color);
                    bgLineColorText = bublinecolor;
                }
                string bublinewidth = row["气泡边线宽度"].ToString().Trim();
                num_LineWidth.Value = decimal.Parse(bublinewidth);
              
            }
            
            string maskcolor = row["晕圈颜色"].ToString().Trim();
            if (maskcolor != string.Empty)
            {
                comboBoxMask.Text = maskcolor;
            }
            string masksize = row["晕圈大小"].ToString().Trim();
            if (masksize != string.Empty)
            {
                num_Mask.Value = decimal.Parse(masksize);
            }
        }

        private void cb_Balloon_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 获取子控件在窗体中的位置（屏幕坐标）
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private System.Drawing.Point getSubControlLocationInForm(Control c)
        {
            System.Drawing.Point location = new System.Drawing.Point(0, 0);
            do
            {
                location.Offset(c.Location);
                c = c.Parent;
            }
            while (c != this && c != null);//循环到当前窗体

            return location;
        }

    }


     
    /// <summary>
    /// 注记修改定义的部分样式属性
    /// </summary>
    public class AnnoStyle
    {
       

        private double angle = 0;
        /// <summary>
        /// 注记的旋转角度
        /// </summary>
        public double Angle
        {
            get { return angle; }
            set { angle = value; }
        }
        /// <summary>
        /// 文本水平对齐
        /// </summary>
        private esriTextHorizontalAlignment horizontal;
        public esriTextHorizontalAlignment Horizontal
        {
            get { return horizontal; }
            set { horizontal = value; }
        }

        /// <summary>
        /// 文本垂直对齐
        /// </summary>
        private esriTextVerticalAlignment vertical;
        public esriTextVerticalAlignment Vertical
        {
            get { return vertical; }
            set { vertical = value; }
        }

        /// <summary>
        /// 气泡样式
        /// </summary>
        private esriBalloonCalloutStyle balloonCallout;
        public esriBalloonCalloutStyle BalloonCallout
        {
            get { return balloonCallout; }
            set { balloonCallout = value; }
        }
        private double textMarginsUp;
        /// <summary>
        /// 上边距
        /// </summary>
        public double TextMarginsUp
        {
            get { return textMarginsUp; }
            set { textMarginsUp = value; }
        }
        private double textMarginsDown;
        /// <summary>
        /// 下边距
        /// </summary>
        public double TextMarginsDown
        {
            get { return textMarginsDown; }
            set { textMarginsDown = value; }
        }
        private double textMarginsLeft;
        /// <summary>
        /// 左边距
        /// </summary>
        public double TextMarginsLeft
        {
            get { return textMarginsLeft; }
            set { textMarginsLeft = value; }
        }
        private double textMarginsRight;
        /// <summary>
        /// 右边距
        /// </summary>
        public double TextMarginsRight
        {
            get { return textMarginsRight; }
            set { textMarginsRight = value; }
        }
      

        private bool noNormal = true;
        /// <summary>
        /// 是否为正常注记，正常注记文本格式为垂直居中，水平居中,默认为正常ture
        /// </summary>
        public bool NoNormal
        {
            get { return noNormal; }
            set{noNormal=value;}
        }


        private string lyrName = string.Empty;
        /// <summary>
        /// 图层名字
        /// </summary>
        public string LyrName
        {
            get { return lyrName; }
            set { lyrName = value; }
        }
       
        
        private string text = string.Empty;
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        private string fontName = string.Empty;
        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName
        {
            get { return fontName; }
            set { fontName = value; }
        }
        private double characterSpace = 0;
        /// <summary>
        /// 字间距
        /// </summary>
        public double CharacterSpace
        {
            get { return characterSpace; }
            set { characterSpace = value; }
        }
        private double characterWidth = 100;
        /// <summary>
        /// 字宽
        /// </summary>
        public double CharacterWidth
        {
            get { return characterWidth; }
            set { characterWidth = value; }
        }
        private bool itatic = false;
        /// <summary>
        /// 斜体
        /// </summary>
        public bool Itatic
        {
            get { return itatic; }
            set { itatic = value; }
        }
        private bool fontBold = false;

        public bool FontBold
        {
            get { return fontBold; }
            set { fontBold = value; }
        }
        private double fontSize = 10;
        /// <summary>
        /// 字大
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }
        private string fontColorCMYK = "K100";
        /// <summary>
        /// C10M20Y30K40
        /// </summary>
        public string FontColorCMYK
        {
            get { return fontColorCMYK; }
            set { fontColorCMYK = value; }
        }
        private bool hasMask = false;
        /// <summary>
        /// 是否拥气泡背景
        /// </summary>
        public bool HasMask
        {
            get { return hasMask; }
            set { hasMask = value; }
        }
        private string maskColorCMYK;//填充颜色

        /// <summary>
        /// 填充颜色
        /// </summary>
        public string MaskColorCMYK
        {
            get { return maskColorCMYK; }
            set { maskColorCMYK = value; }
        }


        private string lineColorCMYK;//边框颜色
        public string LineColorCMYK
        {
            get { return lineColorCMYK; }
            set { lineColorCMYK = value; }
        }


        private double lineWidth;//边框宽度
        public double LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }
        /// <summary>
        /// 文字蒙版样式
        /// </summary>
        private esriMaskStyle textMaskStyle;
        public esriMaskStyle TextMaskStyle
        {
            get { return textMaskStyle; }
            set { textMaskStyle = value; }
        }

        /// <summary>
        /// 文字蒙版大小
        /// </summary>
        private double textMaskSize;
        public double TextMaskSize
        {
            get { return textMaskSize; }
            set { textMaskSize = value; }
        }

        private string textMaskColor = string.Empty;
        /// <summary>
        /// 文字蒙版颜色（C10M20Y30K40）
        /// </summary>
        public string TextMaskColor
        {
            get { return textMaskColor; }
            set { textMaskColor = value; }
        }

       


        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public AnnoStyle Clone()
        {
            AnnoStyle an = new AnnoStyle();
            //利用反射进行克隆
            System.Reflection.PropertyInfo[] PropertyInfos = this.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo PropertyInfo in PropertyInfos)
            {
                object v1 = PropertyInfo.GetValue(this, null);
                if (PropertyInfo.Name == "Envelope")
                {
                    IEnvelope envelope = v1 as IEnvelope;
                    IEnvelope envelopeCopy = (envelope as IClone).Clone() as IEnvelope;
                    PropertyInfo.SetValue(an, envelopeCopy, null);
                }
                else
                {
                    PropertyInfo.SetValue(an,v1, null);
                }
            }
            return an;
        }

    }
    
}

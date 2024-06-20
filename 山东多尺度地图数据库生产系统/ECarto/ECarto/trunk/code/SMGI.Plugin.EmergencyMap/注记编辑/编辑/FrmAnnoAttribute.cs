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
using System.Runtime.InteropServices;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmAnnoAttribute : Form
    {
        int hWnd = 0;
        Dictionary<string, string> CommonFonts = new Dictionary<string, string>();
        List<string> ztdz=new List<string>();
        IColorPalette colorPalette;
        IPoint annoPoint = null;
        string TypeAnno = "分类";
        public string AnnoSelectSQL = string.Empty;
     

        public DataRow AnnoRuleRow = null;//选择的注记规则
        /// <summary>
        /// 输入的样式
        /// </summary>
        public AnnoStyle InAnnoStyle=null;
        /// <summary>
        /// 输出的样式
        /// </summary>
        public AnnoStyle OutAnnoStyle;

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
        public FrmAnnoAttribute(int _hWnd,DataRow rowRule=null)
        {
            InitializeComponent();
            hWnd = _hWnd;
            //获取当前工作空间的所有注记图层
            comboBoxLyr.ValueMember = "Key";
            comboBoxLyr.DisplayMember = "Value";

            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IFDOGraphicsLayer; })).ToArray();
            foreach (var lyr in lyrs)
            {
                if (lyr is IFeatureLayer)
                {
                    int typeIndex =(lyr as IFeatureLayer).FeatureClass.FindField(TypeAnno);
                    if(typeIndex!=-1)
                    {
                       comboBoxLyr.Items.Add(new KeyValuePair<IFeatureLayer, string>(lyr as IFeatureLayer, lyr.Name));
                    }
                }
            }
         
            
            annoPoint = null;
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
            if (comboBoxLyr.Items.Count > 0)
                comboBoxLyr.SelectedIndex = comboBoxLyr.Items.Count - 1;
        }
        /// <summary>
        /// 添加注记
        /// </summary>
        /// <param name="_hWnd"></param>
        /// <param name="inAnoStyle"></param>
        public FrmAnnoAttribute(int _hWnd, AnnoStyle inAnoStyle)
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
                //轮廓属性
                num_LineWidth.Value = (decimal)inAnoStyle.LineWidth;
                color = GetColorByString(inAnoStyle.LineColorCMYK);
                this.btLineColor.BackColor = ConvertIColorToColor(color);
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
            this.cbFontFamily.SelectedIndex = 0;
            //初始化调色板
            colorPalette = new ColorPalette();
            //绑定相关事件:颜色
            this.BtnFontColor.Click += new EventHandler(BtnColor_Click);
            this.btn_MaskColor.Click += new EventHandler(BtnColor_Click);
            this.btBackgroudCol.Click += new EventHandler(BtnColor_Click);
            this.btLineColor.Click += new EventHandler(BtnColor_Click);
          
            this.comboBoxFont.TextChanged += new EventHandler(comboBox_TextChanged);
            this.comboBoxMask.TextChanged += new EventHandler(comboBox_TextChanged);
           
            if (InAnnoStyle != null)
            {
                SetAnnoStyle(InAnnoStyle);
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
           
           
            var itemfont= CommonFonts.Where(t => t.Value == an.FontName).FirstOrDefault();
            string fontname = "";
            if (itemfont.Key == null)
            {
                fontname = "黑体";
            }
            else
            {
                fontname = itemfont.Key;
            }
            int index = cbFontFamily.Items.IndexOf(fontname);
            cbFontFamily.SelectedIndex = index;
            nudFontSize.Value = Convert.ToDecimal(an.FontSize);
            //设置注记要素的字体颜色
            comboBoxFont.Text = an.FontColorCMYK;
           
        }
        /// <summary>
        /// 调色板事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnColor_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ConvertColorToIColor(btn.BackColor);
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            //tagRect.right = (this.Left*2+this.Width)/2;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            //tagRect.top = this.Top;
            //这个颜色板以左下角坐标定位，我也是醉了
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, this.hWnd))
            {
                btn.BackColor = ConvertIColorToColor(colorPalette.Color);
                if (btn.Name == "BtnFontColor")
                {
                    this.comboBoxFont.Text = GetStringByColor(colorPalette.Color);
                }
                else if(btn.Name=="btn_MaskColor")
                {
                    this.comboBoxMask.Text =  GetStringByColor(colorPalette.Color);
                }
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
            AnnoSelectSQL = "分类='"+cmbAnnoType.SelectedItem.ToString()+"'";
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
            OutAnnoStyle.FontSize = Convert.ToDouble(nudFontSize.Value);
            OutAnnoStyle.LyrName = comboBoxLyr.Text;
            OutAnnoStyle.FontName = CommonFonts[cbFontFamily.SelectedItem.ToString()];
            OutAnnoStyle.FontBold = cbBold.Checked;

            OutAnnoStyle.FontBold = cbBold.Checked;
            OutAnnoStyle.FontColorCMYK = GetStringByColor(ConvertColorToIColor(BtnFontColor.BackColor));

            //文本对齐方式
            OutAnnoStyle.Horizontal = str2Horizontal[cb_Horizontal.Text];
            OutAnnoStyle.Vertical = str2Vertical[cb_Vertical.Text];

            //背景注记
            OutAnnoStyle.HasMask = checkBall.Checked;
            if (checkBall.Checked)
            {
                //背景
                OutAnnoStyle.MaskColorCMYK = GetStringByColor(ConvertColorToIColor(this.btBackgroudCol.BackColor));
                OutAnnoStyle.BalloonCallout = str2BallonCallout[cb_Balloon.SelectedItem.ToString()];
                //轮廓
                OutAnnoStyle.LineColorCMYK = GetStringByColor(ConvertColorToIColor(this.btLineColor.BackColor));
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
            OutAnnoStyle.TextMaskColor = GetStringByColor(ConvertColorToIColor(btn_MaskColor.BackColor));
          
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
            if (this.cmbAnnoType.SelectedItem == null || this.comboBoxLyr.SelectedItem == null)
            {
                MessageBox.Show("注记类型不能为空！");
                return false;
            }
            if (this.cbFontFamily.Text == string.Empty)
            {
                MessageBox.Show("请选择注记字体！");
                return false;
            }
            if (this.nudFontSize.Value == 0)
            {
                MessageBox.Show("注记尺寸不能为0！");
                return false;
            }
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
                IElement element = annoFeature.Annotation;
                IGeometry posGeometry = element.Geometry;
                if (!updateStyle.NoNormal && posGeometry is IPoint)
                {
                    //非正常文本格式的时候，且该注记的定位元素为点状，采用这种方式
                    IPoint nPoint = (feature.Shape as IArea).LabelPoint;
                    (posGeometry as IPoint).Y = nPoint.Y;
                }

                //创建新的textElement
                string message = string.Empty;
                ITextElement newTextElement = AnnoFunc.CreateTextElement(posGeometry,
                            oldStyle.Text,
                            updateStyle.FontName,
                            oldStyle.CharacterWidth,
                            oldStyle.Itatic,
                            updateStyle.FontSize,
                            GetColorByString(updateStyle.FontColorCMYK),
                            out message,
                            oldStyle.FontBold,
                            oldStyle.Horizontal,
                            oldStyle.Vertical
                            );
                if (selectedFeaureDic.Count == 1)
                {
                    newTextElement = AnnoFunc.CreateTextElement(posGeometry, oldStyle.Text,
                            updateStyle.FontName,
                            updateStyle.CharacterWidth,
                            updateStyle.Itatic,
                            updateStyle.FontSize,
                            GetColorByString(updateStyle.FontColorCMYK),
                            out message,
                            updateStyle.FontBold,
                            updateStyle.Horizontal,
                            updateStyle.Vertical);
                }  
                if (newTextElement == null)
                    return;
                //添加背景边框，如果有的话
                IElement newElement = newTextElement as IElement;

                //蒙版
                ITextSymbol textSymbol = newTextElement.Symbol;
                IMask textMask = textSymbol as IMask;
                textMask.MaskSize = updateStyle.TextMaskSize;
                textMask.MaskStyle = updateStyle.TextMaskStyle;
                ILineSymbol maskLineSymbol = new SimpleLineSymbol();
                maskLineSymbol.Width = 0;
                IFillSymbol maskSymbol = new SimpleFillSymbolClass();
                maskSymbol.Color = GetColorByString(updateStyle.TextMaskColor);
                maskSymbol.Outline = maskLineSymbol;
                textMask.MaskSymbol = maskSymbol;
                if (updateStyle.HasMask)//气泡背景
                {
                    //设置文本背景框样式
                    IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;
                    IBalloonCallout balloonCallout = new BalloonCalloutClass();
                    balloonCallout.Style = updateStyle.BalloonCallout;
                    balloonCallout.LeaderTolerance = 10;
                    //设置文本背景框颜色
                    IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                    fillSymbol.Color = GetColorByString(updateStyle.MaskColorCMYK);
                    ILineSymbol lineSymbol = new SimpleLineSymbol();
                    lineSymbol.Width = updateStyle.LineWidth;
                    lineSymbol.Color = GetColorByString(updateStyle.LineColorCMYK);
                    fillSymbol.Outline = lineSymbol;
                    balloonCallout.Symbol = fillSymbol;
                    //设置背景框边距
                    ITextMargins textMarigns = balloonCallout as ITextMargins;
                    textMarigns.PutMargins(updateStyle.TextMarginsLeft, updateStyle.TextMarginsUp, updateStyle.TextMarginsRight, updateStyle.TextMarginsDown);
                    formattedTextSymbol.Background = balloonCallout as ITextBackground;
                }
                textSymbol.Angle = updateStyle.Angle * 180 / Math.PI;//角度
                newTextElement.Symbol = textSymbol;
                annoFeature.Annotation = newElement;
                feature.Store();
                IEnvelope en = feature.Shape.Envelope;
                if (refreshEnvelop == null)
                    refreshEnvelop = en;
                else
                {
                    if (en.XMax > refreshEnvelop.XMax)
                        refreshEnvelop.XMax = en.XMax;
                    if (en.YMax > refreshEnvelop.YMax)
                        refreshEnvelop.YMax = en.YMax;
                    if (en.XMin < refreshEnvelop.XMin)
                        refreshEnvelop.XMin = en.XMin;
                    if (en.YMin < refreshEnvelop.YMin)
                        refreshEnvelop.YMin = en.YMin;
                }
            }
            m_Application.EngineEditor.StopOperation("注记符号修改");
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, refreshEnvelop);
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
        public IFeatureLayer ObjAnnoLayer
        {
            get
            {
                if (comboBoxLyr.SelectedItem == null)
                    return null;

                return ((KeyValuePair<IFeatureLayer, string>)comboBoxLyr.SelectedItem).Key;
            }

        }

        private void comboBoxLyr_SelectedIndexChanged(object sender, EventArgs e)
        {
            int typeIndex = ObjAnnoLayer.FeatureClass.FindField(TypeAnno);
            if (typeIndex != -1)
            {
                cmbAnnoType.Items.Clear();
                //分类的唯一值
                IQueryFilter qf = new QueryFilterClass();
                qf.SubFields = TypeAnno;
                IFeatureCursor feCursor = ObjAnnoLayer.Search(qf, true);
                IDataStatistics dataStatistics = new DataStatisticsClass();
                dataStatistics.Field = TypeAnno;//获取统计字段
                dataStatistics.Cursor = feCursor as ICursor;
                var enumerator = dataStatistics.UniqueValues;
                while (enumerator.MoveNext())
                {
                    string valuet = enumerator.Current.ToString();
                    if (valuet == null)
                    {
                        continue;
                    }

                    cmbAnnoType.Items.Add(valuet);
                    cmbAnnoType.SelectedIndex = 0;
                }
                Marshal.ReleaseComObject(feCursor);
            }
           
        }
        /// <summary>
        /// 根据文本元素获取其注记属性
        /// </summary>
        /// <param name="textElement"></param>
        /// <returns></returns>
        private AnnoStyle GetStyleByFeature(IFeature feature)
        {
            ITextElement textElement = (feature as IAnnotationFeature2).Annotation as ITextElement;
            AnnoStyle style = new AnnoStyle();
            style.LyrName = feature.Class.AliasName;
            style.Angle = textElement.Symbol.Angle * Math.PI / 180;
            style.Horizontal = textElement.Symbol.HorizontalAlignment;
            style.Vertical = textElement.Symbol.VerticalAlignment;

            IFormattedTextSymbol formattedTextSymbol = textElement.Symbol as IFormattedTextSymbol;
            if (formattedTextSymbol.VerticalAlignment != esriTextVerticalAlignment.esriTVACenter ||
                formattedTextSymbol.HorizontalAlignment != esriTextHorizontalAlignment.esriTHACenter)
            {
                style.NoNormal = false;
            }
            //背景：气泡
            if (formattedTextSymbol.Background != null)
            {
                style.HasMask = true;
                IBalloonCallout balloonCallout = formattedTextSymbol.Background as IBalloonCallout;
                if (balloonCallout == null)
                {
                    return null;
                }
                style.BalloonCallout = balloonCallout.Style;
                style.TextMarginsLeft = (balloonCallout as ITextMargins).LeftMargin;
                style.TextMarginsUp = (balloonCallout as ITextMargins).TopMargin;
                style.TextMarginsRight = (balloonCallout as ITextMargins).RightMargin;
                style.TextMarginsDown = (balloonCallout as ITextMargins).BottomMargin;
                style.MaskColorCMYK = GetStringByColor(balloonCallout.Symbol.Color);
                style.LineColorCMYK = GetStringByColor(balloonCallout.Symbol.Outline.Color);
                style.LineWidth = balloonCallout.Symbol.Outline.Width;
            }
            //蒙版
            style.TextMaskSize = (textElement.Symbol as IMask).MaskSize;
            style.TextMaskStyle = (textElement.Symbol as IMask).MaskStyle;
            if ((textElement.Symbol as IMask).MaskSymbol != null)
                style.TextMaskColor = GetStringByColor((textElement.Symbol as IMask).MaskSymbol.Color);

            style.Text = textElement.Text;
            style.FontColorCMYK = GetStringByColor(textElement.Symbol.Color);
            style.FontName = textElement.Symbol.Font.Name;
            style.FontSize = textElement.Symbol.Size / 2.8345;
            ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)textElement;
            style.FontBold = pSymbolCollEle.Bold;
            style.CharacterWidth = pSymbolCollEle.CharacterWidth;
            style.CharacterSpace = pSymbolCollEle.CharacterSpacing;
            style.Itatic = pSymbolCollEle.Italic;
            return style;
        }
        private void SetAnnoAttribute(AnnoStyle inAnoStyle)
        {
           
            InAnnoStyle = inAnoStyle;

         
            this.cbBold.Checked = inAnoStyle.FontBold;
           
            this.cb_Horizontal.Text = GetDicKeyByValue(str2Horizontal, inAnoStyle.Horizontal);
            this.cb_Vertical.Text = GetDicKeyByValue(str2Vertical, inAnoStyle.Vertical);
            #region 背景样式
           
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
                //轮廓属性
                num_LineWidth.Value = (decimal)inAnoStyle.LineWidth;
                color = GetColorByString(inAnoStyle.LineColorCMYK);
                this.btLineColor.BackColor = ConvertIColorToColor(color);
            }
            this.cb_Mask.Text = GetDicKeyByValue(str2MaskStyle, inAnoStyle.TextMaskStyle);
            this.num_Mask.Value = (decimal)inAnoStyle.TextMaskSize;
            btn_MaskColor.BackColor = ConvertIColorToColor(GetColorByString(inAnoStyle.TextMaskColor));
            comboBoxMask.Text = inAnoStyle.TextMaskColor;
            #endregion
          //  this.comboBoxLyr.Enabled = false;
            SetAnnoStyle(inAnoStyle);
        }
        private void cmbAnnoType_SelectedIndexChanged(object sender, EventArgs e)
        {
            AnnoSelectSQL = "分类='" + cmbAnnoType.SelectedItem.ToString() + "'";
            IFeatureCursor cursor= ObjAnnoLayer.FeatureClass.Search(new QueryFilterClass{WhereClause=AnnoSelectSQL},false);
            IFeature fe= cursor.NextFeature();
            Marshal.ReleaseComObject(cursor);
            if (fe != null)
            {

                AnnoStyle astyle = GetStyleByFeature(fe);
                SetAnnoAttribute(astyle);
            }
        }

    }


     
    
    
}

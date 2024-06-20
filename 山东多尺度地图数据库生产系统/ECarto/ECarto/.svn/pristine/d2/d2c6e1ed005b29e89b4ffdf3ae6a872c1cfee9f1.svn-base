using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using System.Drawing.Text;

namespace SMGI.Plugin.EmergencyMap
{

    public partial class FrmLableBall : Form
    {
        public string inputText = "";
        public ITextSymbol textSymbol;
        private bool ModifFillColor = false;
        private IActiveView View;
        public bool flag = false;
        IPoint pPoint = new PointClass();      
        IGraphicsContainer pGraphicsContainer;
      
        string[] fontsize = { "5", "5.5", "6.5", "7.5", "8", "9", "10", "10.5", "11", "12", "14", "16", "18", "20"
                            , "22", "24", "26", "28", "36", "48", "72"};
        Color Fillcolor;
        Color Linecolor;
        Color Fontcolor;
        IElement elementT;
        string style, ConterThickness, Angel,FontName,FontPosition,FontType,FontFigure;
        public FrmLableBall()
        {
            InitializeComponent();
            this.Text = "创建气泡注记";
            this.btView.Visible = false;
        }      
        public FrmLableBall(string str, ITextSymbol texSy, IActiveView view, IGraphicsContainer pGraphicsContainer1, IElement elementT1)
        {
            InitializeComponent();
            this.Text = "修改气泡注记";
            this.txtContent.Text = (elementT1 as ITextElement).Text;
            elementT = elementT1;
            string[] s = str.Split(new char[] { ',' });
            inputText=s[0];
            pPoint.X =Convert.ToDouble(s[1]);
            pPoint.Y = Convert.ToDouble(s[2]); 
            Fillcolor = Color. FromArgb(int.Parse(s[3]));
            style=s[4];
            ConterThickness = s[5];
            Linecolor = Color.FromArgb(int.Parse(s[6]));
            Angel = s[7];
            Fontcolor = Color.FromArgb(int.Parse(s[8]));
            FontName = s[9];
            FontPosition = s[10];
            FontType = s[11];
            FontFigure = s[12];     
          
            pGraphicsContainer = pGraphicsContainer1;                     
            textSymbol = texSy;
          
            View = view;
        }      

        private void PlottingToolModifyForm_Load(object sender, EventArgs e)
        {
            cmbBallStyle.Items.Add("矩形框");
            cmbBallStyle.Items.Add("圆角矩形");
            cmbBallStyle.Items.Add("无边框");
            cmbFontLocation.Items.Add("靠左");
            cmbFontLocation.Items.Add("居中");
            cmbFontLocation.Items.Add("靠右");
            cmbFontShp.Items.Add("常规");
            cmbFontShp.Items.Add("加粗");
            cmbFontShp.Items.Add("倾斜");
        
            for (int i = 0; i < fontsize.Length; i++)
            {
                cmbFontSize.Items.Add(fontsize[i]);
            }
            InstalledFontCollection MyFont = new InstalledFontCollection();
            FontFamily[] MyFontFamilies = MyFont.Families;
            int count = MyFontFamilies.Length;
            for (int i = 0; i < count; i++)
            {
                cmbFont.Items.Add(MyFontFamilies[i].Name);
            }
            cmbBallStyle.SelectedIndex = 0;
            cmbFontLocation.SelectedIndex = 0;
            cmbFontShp.SelectedIndex = 0;
            cmbFontSize.SelectedIndex = 0;
            cmbFont.SelectedIndex = cmbFont.Items.IndexOf("宋体");
            if (this.btView.Visible)
            {
                btColorFill.BackColor = Fillcolor;
                btColorOutLine.BackColor = Linecolor;
                btColorText.BackColor = Fontcolor;

                txtContent.Text = inputText;
                txtContent.Multiline = true;
                txtOutLineWidth.Text = ConterThickness;
                txtAngle.Text = Angel;

                cmbBallStyle.SelectedItem = style;


                
                cmbFontSize.SelectedItem = FontName;
                cmbFontLocation.SelectedItem = FontPosition;

              
              
                cmbFont.SelectedItem = FontType;
                cmbFontShp.SelectedItem = FontFigure;
            }
        }
        public stdole.IFontDisp GetFontDisp(string Name, string Text, FontStyle fontStyle)
        {
            string fontFamilyName = Name;
            Font font = new Font(Name, Convert.ToSingle(Text), fontStyle);

            return ESRI.ArcGIS.ADF.COMSupport.OLE.GetIFontDispFromFont(font) as stdole.IFontDisp;
        }
        public IColor ConvertColorTolColor(Color color)
        {
            IColor pColor = new RgbColorClass();
            pColor.RGB = color.B * 65536 + color.G * 256 + color.R;
            pColor.Transparency = 10;

            return pColor;
        }      

        private void button2_Click(object sender, EventArgs e)
        {
            if (txtContent.Text.Trim() == string.Empty)
                return;
            if (this.btView.Visible)
            {
                #region
                ISimpleFillSymbol pSmpleFill = new SimpleFillSymbol();
                pSmpleFill.Style = esriSimpleFillStyle.esriSFSSolid;
                if (ModifFillColor)
                {
                    pSmpleFill.Color = ConvertColorTolColor(this.btColorFill.BackColor);
                }
                else
                    pSmpleFill.Color = ConvertColorTolColor(this.btColorFill.BackColor);


                ISimpleLineSymbol lineSymbol = new SimpleLineSymbol();
                lineSymbol.Width = Convert.ToDouble(txtOutLineWidth.Text);
                lineSymbol.Color = ConvertColorTolColor(this.btColorOutLine.BackColor);
                pSmpleFill.Outline = lineSymbol;

                IBalloonCallout pBllCallout = new BalloonCalloutClass();
                switch (cmbBallStyle.Text)
                {
                    case "矩形框":
                        pBllCallout.Style = esriBalloonCalloutStyle.esriBCSRectangle;

                        break;
                    case "圆角矩形":
                        pBllCallout.Style = esriBalloonCalloutStyle.esriBCSRoundedRectangle;
                        break;
                    case "无边框":
                        pBllCallout.Style = esriBalloonCalloutStyle.esriBCSOval;
                        break;
                }

                pBllCallout.Symbol = pSmpleFill;
                pBllCallout.LeaderTolerance = 5;
                pBllCallout.AnchorPoint = (((elementT as ITextElement).Symbol as IFormattedTextSymbol).Background as IBalloonCallout).AnchorPoint;

                IFormattedTextSymbol pTextSymbol = new TextSymbolClass();
                pTextSymbol.Direction = esriTextDirection.esriTDAngle;
                pTextSymbol.Angle = Convert.ToDouble(txtAngle.Text);
                pTextSymbol.Background = pBllCallout as ITextBackground;
                pTextSymbol.Color = ConvertColorTolColor(this.btColorText.BackColor);
                pTextSymbol.Size = Convert.ToDouble(cmbFontSize.Text);
                FontStyle fontStyle = new FontStyle();
                switch (cmbFontShp.Text)
                {
                    case "常规":
                        fontStyle = FontStyle.Regular;
                        break;
                    case "加粗":
                        fontStyle = FontStyle.Bold;
                        break;
                    case "倾斜":
                        fontStyle = FontStyle.Italic;
                        break;
                }
                pTextSymbol.Font = GetFontDisp(cmbFont.Text, cmbFontSize.Text, fontStyle);
                switch (cmbFontLocation.Text)
                {
                    case "靠左":
                        pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
                        break;
                    case "居中":
                        pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                        break;
                    case "靠右":
                        pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHARight;
                        break;
                }

                textSymbol = pTextSymbol as ITextSymbol;
                inputText = txtContent.Text;

                ITextElement textElement = elementT as ITextElement;
                textElement.Text = txtContent.Text;
                textElement.Symbol = textSymbol;
                textElement.ScaleText = true;

                IElement element = (IElement)elementT;

                string elementParameter = txtContent.Text + "," + pPoint.X + "," + pPoint.Y + "," + this.btColorFill.BackColor.ToArgb()
                      + "," + cmbBallStyle.Text + "," + txtOutLineWidth.Text + "," + this.btColorOutLine.BackColor.ToArgb() + "," + txtAngle.Text
                      + "," + this.btColorText.BackColor.ToArgb() + "," + cmbFontSize.Text + "," + cmbFontLocation.Text + "," + cmbFont.Text + "," + cmbFontShp.Text;
                (element as IElementProperties3).Name = elementParameter;//原参数
                (element as IElementProperties3).Type = LabelType.BallCallout.ToString();//类型

                pGraphicsContainer.UpdateElement(elementT);
                View.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);//绘制新元素
                #endregion
            }
         //   pGraphicsContainer.DeleteElement(elementT);//删除旧元素
            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.colorDialog1.ShowDialog() == DialogResult.OK)
            {
                this.btColorFill.BackColor = this.colorDialog1.Color;
                ModifFillColor = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.colorDialog1.ShowDialog() == DialogResult.OK)
            {
                this.btColorOutLine.BackColor = this.colorDialog1.Color;
                ModifFillColor = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.colorDialog1.ShowDialog() == DialogResult.OK)
            {
                this.btColorText.BackColor = this.colorDialog1.Color;
                ModifFillColor = true;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            pGraphicsContainer.DeleteElement(elementT);//删除
            View.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            this.Close();
        }
        //预览
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (txtContent.Text.Trim() == string.Empty)
                return;
            ISimpleFillSymbol pSmpleFill = new SimpleFillSymbol();
            pSmpleFill.Style = esriSimpleFillStyle.esriSFSSolid;
            if (ModifFillColor)
            {
                pSmpleFill.Color = ConvertColorTolColor(this.btColorFill.BackColor);
            }
            else
                pSmpleFill.Color = ConvertColorTolColor(this.btColorFill.BackColor);


            ISimpleLineSymbol lineSymbol = new SimpleLineSymbol();
            lineSymbol.Width = Convert.ToDouble(txtOutLineWidth.Text);
            lineSymbol.Color = ConvertColorTolColor(this.btColorOutLine.BackColor);
            pSmpleFill.Outline = lineSymbol;

            IBalloonCallout pBllCallout = new BalloonCalloutClass();
            switch (cmbBallStyle.Text)
            {
                case "矩形框":
                    pBllCallout.Style = esriBalloonCalloutStyle.esriBCSRectangle;

                    break;
                case "圆角矩形":
                    pBllCallout.Style = esriBalloonCalloutStyle.esriBCSRoundedRectangle;
                    break;
                case "无边框":
                    pBllCallout.Style = esriBalloonCalloutStyle.esriBCSOval;
                    break;
            }

            pBllCallout.Symbol = pSmpleFill;
            pBllCallout.LeaderTolerance = 5;
            pBllCallout.AnchorPoint = (((elementT as ITextElement).Symbol as IFormattedTextSymbol).Background as IBalloonCallout).AnchorPoint;

            IFormattedTextSymbol pTextSymbol = new TextSymbolClass();
            pTextSymbol.Direction = esriTextDirection.esriTDAngle;
            pTextSymbol.Angle = Convert.ToDouble(txtAngle.Text);
            pTextSymbol.Background = pBllCallout as ITextBackground;
            pTextSymbol.Color = ConvertColorTolColor(this.btColorText.BackColor);
            pTextSymbol.Size = Convert.ToDouble(cmbFontSize.Text);
            FontStyle fontStyle = new FontStyle();
            switch (cmbFontShp.Text)
            {
                case "常规":
                    fontStyle = FontStyle.Regular;
                    break;
                case "加粗":
                    fontStyle = FontStyle.Bold;
                    break;
                case "倾斜":
                    fontStyle = FontStyle.Italic;
                    break;
            }
            pTextSymbol.Font = GetFontDisp(cmbFont.Text, cmbFontSize.Text, fontStyle);
            switch (cmbFontLocation.Text)
            {
                case "靠左":
                    pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
                    break;
                case "居中":
                    pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                    break;
                case "靠右":
                    pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHARight;
                    break;
            }

            textSymbol = pTextSymbol as ITextSymbol;
            inputText = txtContent.Text;

            ITextElement textElement = elementT as ITextElement;
            textElement.Text = txtContent.Text;
            textElement.Symbol = textSymbol;
            textElement.ScaleText = true;

            IElement element = (IElement)elementT;

            string elementParameter = txtContent.Text + "," + pPoint.X + "," + pPoint.Y + "," + this.btColorFill.BackColor.ToArgb()
                  + "," + cmbBallStyle.Text + "," + txtOutLineWidth.Text + "," + this.btColorOutLine.BackColor.ToArgb() + "," + txtAngle.Text
                  + "," + this.btColorText.BackColor.ToArgb() + "," + cmbFontSize.Text + "," + cmbFontLocation.Text + "," + cmbFont.Text + "," + cmbFontShp.Text;
            (element as IElementProperties3).Name = elementParameter;//原参数
            (element as IElementProperties3).Type = LabelType.BallCallout.ToString();//类型

            pGraphicsContainer.UpdateElement(elementT);
            View.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);//绘制新元素
        }
        public bool POI = false;
        private void btAdv_Click(object sender, EventArgs e)
        {
            FrmLabelAd frm = new FrmLabelAd();
            if (frm.ShowDialog() == DialogResult.OK)
                POI = frm.cbPOI.Checked;
        }

        private void cmbFontLocation_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}

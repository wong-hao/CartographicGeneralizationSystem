using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public partial class FrmMoveline : Form
    {
        public FrmMoveline()
        {
            InitializeComponent();
        }
        public FrmMoveline(IColor fcolor,IColor tcolor)
        {
            InitializeComponent();
            this.btColorFrom.BackColor = ConvertIColorToColor(fcolor);
            this.btColorEnd.BackColor = ConvertIColorToColor(tcolor);
        }
        int hWnd = 0;
        public double lineFromWidth = 2;
        public double lineToWidth = 6;
        public double angle = 0;
        public IColor ColorFrom;
        public IColor BorderColor;
        public IColor ColorTo;

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

        private void GetChartsData()
        {
            //lineFromWidth = Convert.ToDouble(txtFromWidth.Text.ToString());
            double.TryParse(txtFromWidth.Text.ToString(), out lineFromWidth);
            //lineToWidth = Convert.ToDouble(txtToWidth.Text.ToString());
            double.TryParse(txtToWidth.Text.ToString(), out lineToWidth);
            //ColorFrom = ConvertColorToIColor(btColorFrom.BackColor);
           // ColorTo = ConvertColorToIColor(btColorEnd.BackColor);
        }

       
        private void btColorFrom_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette = new ColorPalette();
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ConvertColorToIColor(btn.BackColor);
            if (ColorFrom != null)
                color = ColorFrom;
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, this.hWnd))
            {
                this.btColorFrom.BackColor = ConvertIColorToColor(colorPalette.Color);
                ColorFrom = new CmykColorClass { CMYK = colorPalette.Color.CMYK };
            }
        }

        private void btColorEnd_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette = new ColorPalette();
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ConvertColorToIColor(btn.BackColor);
            if (ColorTo != null)
                color = ColorTo;
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, this.hWnd))
            {
                this.btColorEnd.BackColor = ConvertIColorToColor(colorPalette.Color);
                ColorTo = new CmykColorClass { CMYK = colorPalette.Color.CMYK };
            }
        }
        
        private void btok_Click(object sender, EventArgs e)
        {
            GetChartsData();
            if (lineFromWidth == 0 || lineToWidth == 0)
            {
                MessageBox.Show("起止宽度不能为0！");
                return;
            }
            if (cbboxLineType.SelectedItem.ToString() == "无边线")
            {
                BorderColor = ObtainNULLColor();
            }
            else
            {
              //  BorderColor = ConvertColorToIColor(btLineColor.BackColor);
            }
            DialogResult = DialogResult.OK;
        }

        private void FrmMoveline_Load(object sender, EventArgs e)
        {
        }

        private void btcancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void txtFromWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            //{
            //    e.Handled = true;
            //}
            //else
            //{
            //    return;
            //}
        }

        private void txtToWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            //{
            //    e.Handled = true;
            //}
            //else
            //{
            //    return;
            //}
        }

        private void txtInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
            else
            {
                return;
            }
        }

        private void txtAngle_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
            else
            {
                return;
            }
        }

        private void txtPercentage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
            else
            {
                return;
            }
        }
        //获取空颜色测试
        private static IColor ObtainNULLColor()
        {
            ICmykColor pnullcolor = new CmykColorClass();
            pnullcolor.CMYK = 6;
            pnullcolor.Black = 6;
            pnullcolor.Cyan = 0;
            pnullcolor.Magenta = 0;
            pnullcolor.NullColor = true;
            pnullcolor.RGB = 15790320;
            pnullcolor.Transparency = 0;
            pnullcolor.UseWindowsDithering = true;
            pnullcolor.Yellow = 0;
            return pnullcolor;


        }
        private void FrmMoveline_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void txtXiaoying_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void txtAngle_TextChanged(object sender, EventArgs e)
        {

        }

        private void FrmMoveline_Load_1(object sender, EventArgs e)
        {
            cbboxLineType.SelectedIndex = 0;
            BorderColor = ConvertColorToIColor(btLineColor.BackColor);
            ColorFrom = ConvertColorToIColor(btColorFrom.BackColor);
            ColorTo = ConvertColorToIColor(btColorEnd.BackColor);
        }

        private void btLineColor_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette = new ColorPalette();
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ConvertColorToIColor(btn.BackColor);
            if (BorderColor != null)
                color = BorderColor;
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, this.hWnd))
            {
                this.btLineColor.BackColor = ConvertIColorToColor(colorPalette.Color);
                BorderColor = new CmykColorClass { CMYK = colorPalette.Color.CMYK };
            }
        }
    }
}

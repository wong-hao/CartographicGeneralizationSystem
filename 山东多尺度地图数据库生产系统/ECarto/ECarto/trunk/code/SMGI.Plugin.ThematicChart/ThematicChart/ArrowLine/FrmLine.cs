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
    public partial class FrmLine : Form
    {
        public FrmLine()
        {
            InitializeComponent();
        }
        public FrmLine(IColor col)
        {
            InitializeComponent();
            this.btColorFrom.BackColor = ConvertIColorToColor(col);
        }
        public IColor ColorFrom;
        public double lineWidth = 6;
        private int hWnd = 0;
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
            lineWidth = Convert.ToDouble(txtWidth.Text.ToString());
           // ColorFrom = ConvertColorToIColor(btColorFrom.BackColor);
        }
        private void btColorFrom_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette = new ColorPalette();
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ConvertColorToIColor(btn.BackColor);
            if(ColorFrom!=null)
                color=ColorFrom;
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, this.hWnd))
            {
                this.btColorFrom.BackColor = ConvertIColorToColor(colorPalette.Color);
                ColorFrom = new CmykColorClass { CMYK = colorPalette.Color.CMYK };
            }
        }

        private void txtToWidth_KeyPress(object sender, KeyPressEventArgs e)
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

        private void btok_Click(object sender, EventArgs e)
        {
            GetChartsData();
            if (lineWidth == 0)
                return;
            DialogResult = DialogResult.OK;
        }

        private void btcancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void FrmLine_Load(object sender, EventArgs e)
        {
            ColorFrom = ConvertColorToIColor(btColorFrom.BackColor);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using stdole;
using System.IO;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using System.Xml.Linq;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    public partial class FrmRangeLine : Form
    {
        public double LineWidth = 3;
        public double MarkerSize = 3;
        public double MarkerSizeInt = 1;
        public ICmykColor CMYKColors;
        public FrmRangeLine()
        {
            InitializeComponent();
        }

        private void btok_Click(object sender, EventArgs e)
        {
            LineWidth = double.Parse(txtWidth.Text);
            MarkerSize = double.Parse(markerSize.Text);
            MarkerSizeInt = double.Parse(markerInterval.Text);
           // CMYKColors = ColorHelper.ConvertColorToCMYK(btBgColor.BackColor);
            DialogResult = DialogResult.OK;
        }

        private void btcancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btBgColor_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color =ColorHelper.ConvertColorToIColor(btn.BackColor);
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            //tagRect.right = (this.Left*2+this.Width)/2;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            //tagRect.top = this.Top;

            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
                this.btBgColor.BackColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
                CMYKColors = new CmykColorClass { CMYK = CMYKColors.CMYK };
            }
        }

        private void FrmRangeLine_Load(object sender, EventArgs e)
        {
            CMYKColors = ColorHelper.ConvertColorToCMYK(btBgColor.BackColor);
        }
    }
}

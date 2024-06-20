using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.ThematicChart
{

    public partial class FrmEarthToolSet : Form
    {
        public FrmEarthToolSet()
        {
            InitializeComponent();

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public int RingCount;
        public double RingDis;
        public double CenterSize;
        public double RingLineWidth;
        public double Level;
        public double Depth;
        public double WaveFontSize;
        public double CenterFontSize;
        public IColor WaveAnnoColor;
        public IColor CenterAnnoColor;
        
        private void btOK_Click(object sender, EventArgs e)
        {
            RingCount = int.Parse(txtRingCount.Text);
            RingDis = double.Parse(txtRingDis.Text);
            CenterSize = double.Parse(txtCenterSize.Text);
            RingLineWidth = double.Parse(txtLineWidth.Text);
            Level = double.Parse(tb_level.Text);
            Depth = double.Parse(tb_depth.Text);
            WaveFontSize = (double)Wave_FontSize.Value;
            CenterFontSize = (double)Center_FontSize.Value;
            WaveAnnoColor = new RgbColorClass { Red = btn_waveColor.BackColor.R, Green = btn_waveColor.BackColor.G, Blue = btn_waveColor.BackColor.B};
            CenterAnnoColor = new RgbColorClass { Red = btn_centerColor.BackColor.R, Green = btn_centerColor.BackColor.G, Blue = btn_centerColor.BackColor.B };
            this.DialogResult = DialogResult.OK;
        }

        private void btn_waveColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color c = colorDialog1.Color;
                this.btn_waveColor.BackColor = c;
            }
        }

        private void btn_centerColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color c = colorDialog1.Color;
                this.btn_centerColor.BackColor = c;
            }
        }
    }
}

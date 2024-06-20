using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using SMGI.Plugin.ThematicChart;
using SMGI.Common;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.ThematicChart
{
    public partial class FrmColor : Form
    {
        public IColor SelectColor;
        public FrmColor(IColor oldcolor)
        {
            InitializeComponent();
            BtnCurColor.BackColor =ColorHelper.ConvertIColorToColor(oldcolor);
            BtnCurColor.Tag = oldcolor;
        }
        public FrmColor(Color oldcolor)
        {
            InitializeComponent();
            BtnCurColor.BackColor = oldcolor;
            BtnCurColor.Tag =  ColorHelper.ConvertColorToIColor(oldcolor);
        }
        private void FrmColor_Load(object sender, EventArgs e)
        {
            LoadColorHsv();
            DrawButton();
        }
        private List<HsvColorClass> HsvColors = new List<HsvColorClass>();
        private void LoadColorHsv()
        {
            string rulegdb = GApplication.Application.Template.Content.Element("ThematicRule").Value;
            string template = GApplication.Application.Template.Root + "\\" + rulegdb;

            IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspace ws = wsFactory.OpenFromFile(template, 0);
            ITable pTable = (ws as IFeatureWorkspace).OpenTable("ThematicColor");
            ICursor cursor = pTable.Search(null, false);
            IRow prow = null;
            int hindex = pTable.FindField("H");
            int sindex = pTable.FindField("S");
            int vindex = pTable.FindField("V");
            while ((prow = cursor.NextRow()) != null)
            {
                string h = prow.get_Value(hindex).ToString().Trim();
                string s = prow.get_Value(sindex).ToString().Trim();
                string v = prow.get_Value(vindex).ToString().Trim();
                HsvColorClass pcolor = new HsvColorClass();
                pcolor.Hue = int.Parse(h);
                pcolor.Saturation = int.Parse(s);
                pcolor.Value = int.Parse(v);
                HsvColors.Add(pcolor);
            }
            Marshal.ReleaseComObject(cursor);
        }
        

        private void DrawButton()
        {
            int i=1;
            foreach (var hsv in HsvColors)
            {
                double a = (double)i;
                double dx = i % 5;
                double dy = Math.Ceiling((a / 5));

                int x = (int)dx;
                int y = (int)dy-1;

                Color pcolor = ColorHelper.ConvertIColorToColor(hsv);
                Panel panel = new Panel();
                panel.Width = 35;
                panel.Height = 35;
                panel.Location = new Point(35 + x * 55, 15+55*y);
                panel.BackColor = pcolor;
                panel.Tag = hsv as IColor;
                ColorPanel.Controls.Add(panel);
                i++;
                panel.MouseClick+=new MouseEventHandler(panel_MouseClick);
            }
          
           
         
      
        }



        private void panel_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (Control c in ColorPanel.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.Panel))
                {
                    (c as Panel).BorderStyle = BorderStyle.None;
                }
            }
            (sender as Panel).BorderStyle = BorderStyle.Fixed3D;
            BtnCurColor.BackColor = (sender as Panel).BackColor;
            BtnCurColor.Tag = (sender as Panel).Tag as IColor;
        }

        private void btMore_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            IColor color = ConvertColorToIColor(BtnCurColor.BackColor);
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;
            //tagRect.right = (this.Left*2+this.Width)/2;
            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            //tagRect.top = this.Top;
            //这个颜色板以左下角坐标定位，我也是醉了
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
                BtnCurColor.BackColor = ConvertIColorToColor(colorPalette.Color);
                BtnCurColor.Tag = colorPalette.Color;
            }
        }
        public IColor ConvertColorToIColor(Color p_Color)
        {
            IColor pColor = new RgbColorClass { RGB = p_Color.B * 65536 + p_Color.G * 256 + p_Color.R };
            return pColor;
        }
        private Color ConvertIColorToColor(IColor pRgbColor)
        {
            return ColorTranslator.FromOle(pRgbColor.RGB);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            SelectColor = BtnCurColor.Tag as IColor; 
            DialogResult = DialogResult.OK;
        }
    }
}

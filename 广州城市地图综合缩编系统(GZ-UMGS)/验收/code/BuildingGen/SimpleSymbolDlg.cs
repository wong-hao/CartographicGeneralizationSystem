using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;

namespace BuildingGen
{
    public partial class SimpleSymbolDlg : Form
    {
        private Color lastFillColor;
        private Color lastLineColor;

        public ISymbol Symbol { get; private set; }

        private SimpleFillSymbolClass fs;
        private SimpleLineSymbolClass ls;
        private SimpleMarkerSymbolClass ms;
        public SimpleSymbolDlg(ISymbol symbol)
        {
            fs = new SimpleFillSymbolClass();
            ls = new SimpleLineSymbolClass();
            ms = new SimpleMarkerSymbolClass();
            fs.Outline = ls;

            InitializeComponent();
            LineWidth.Items.Clear();
            //LineWidth.SelectedIndex = 0;
            MarkerStyle.SelectedIndex = 0;

            //MarkerStyle.Items.Clear();
            for (int i = 0; i < 20; i++)
            {
                LineWidth.Items.Add((double)i);
            }
            LineWidth.SelectedIndex = 0;
            if (symbol is ISimpleLineSymbol)
            {
                FillGroup.Enabled = false;
                MarkerStyleGroup.Enabled = false;
                IRgbColor rgbColor = new RgbColorClass();
                rgbColor.RGB = (symbol as ILineSymbol).Color.RGB;
                LineColor.BackColor = Color.FromArgb(rgbColor.Red,rgbColor.Green,rgbColor.Blue);
                LineWidth.SelectedItem = (symbol as ILineSymbol).Width;
                useLineColor.Checked = (symbol as ILineSymbol).Color.NullColor;
                Symbol = ls;
                //(Symbol as ILineSymbol).Color = //ColorFromRGB(LineColor.BackColor.R
            }
            else if (symbol is ISimpleFillSymbol)
            {
                MarkerStyleGroup.Enabled = false;
                ILineSymbol ls_temp = (symbol as ISimpleFillSymbol).Outline;
                IRgbColor rgbColor = new RgbColorClass();
                rgbColor.RGB = (ls_temp as ILineSymbol).Color.RGB;
                //int i_color = (ls_temp as ILineSymbol).Color.RGB & 0x00ffffff;
                //LineColor.BackColor = Color.FromArgb(i_color >> 16, (i_color >> 8) & 0x00ff, i_color & 0x0000ff);
                LineColor.BackColor = Color.FromArgb(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
                LineWidth.SelectedItem = (ls_temp as ILineSymbol).Width;
                useLineColor.Checked = (ls_temp as ILineSymbol).Color.NullColor;

                rgbColor.RGB = (symbol as ISimpleFillSymbol).Color.RGB;
                FillColor.BackColor = Color.FromArgb(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
                UseFillColor.Checked = (symbol as ISimpleFillSymbol).Color.NullColor;

                Symbol = fs;
            }
            else if (symbol is ISimpleMarkerSymbol)
            {
                useLineColor.Checked = !(symbol as ISimpleMarkerSymbol).Outline;
                IRgbColor rgbColor = new RgbColorClass();
                rgbColor.RGB = (symbol as ISimpleMarkerSymbol).OutlineColor.RGB;
                LineColor.BackColor = Color.FromArgb(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
                LineWidth.SelectedItem = (symbol as ISimpleMarkerSymbol).OutlineSize;

                rgbColor.RGB = (symbol as ISimpleMarkerSymbol).Color.RGB;
                FillColor.BackColor = Color.FromArgb(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
                useLineColor.Checked = (symbol as ISimpleMarkerSymbol).Color.NullColor;
                LineWidth.SelectedIndex = (int)(symbol as ISimpleMarkerSymbol).Style;

                Symbol = ms;
            }
            else
            {

            }
        }

        private void UpdateSymbol()
        {
            if (Symbol is ISimpleLineSymbol)
            {
                (Symbol as ILineSymbol).Color = ColorFromColor(LineColor.BackColor, useLineColor.Checked);

                (Symbol as ILineSymbol).Width = (double)LineWidth.SelectedItem;
            }
            else if (Symbol is ISimpleFillSymbol)
            {
                ILineSymbol ls_temp = (Symbol as ISimpleFillSymbol).Outline;
                (ls_temp as ILineSymbol).Color = ColorFromColor(LineColor.BackColor, useLineColor.Checked);
                (ls_temp as ILineSymbol).Width = (double)LineWidth.SelectedItem;
                (Symbol as ISimpleFillSymbol).Color = ColorFromColor(FillColor.BackColor, UseFillColor.Checked);
                (Symbol as ISimpleFillSymbol).Outline = ls_temp;
            }
            else if (Symbol is ISimpleMarkerSymbol)
            {
                (Symbol as ISimpleMarkerSymbol).Outline = !useLineColor.Checked;
                (Symbol as ISimpleMarkerSymbol).OutlineColor = ColorFromColor(LineColor.BackColor, useLineColor.Checked);
                (Symbol as ISimpleMarkerSymbol).Style = (esriSimpleMarkerStyle)LineWidth.SelectedIndex;
                (Symbol as ISimpleMarkerSymbol).Color = ColorFromColor(FillColor.BackColor, UseFillColor.Checked);
                (Symbol as ISimpleMarkerSymbol).OutlineSize = (double)LineWidth.SelectedItem;
            }


        }

        private void useLineColor_CheckedChanged(object sender, EventArgs e)
        {

            UpdateSymbol();
        }
        private IColor ColorFromRGB(int r, int g, int b, bool nullColor)
        {
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = r;
            rgb.Green = g;
            rgb.Blue = b;
            rgb.NullColor = nullColor;
            return rgb;
        }
        private IColor ColorFromColor(Color c, bool nullColor)
        {
            return ColorFromRGB(c.R, c.G, c.B, nullColor);
        }
        private void MarkerStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            ms.Style = (esriSimpleMarkerStyle)MarkerStyle.SelectedIndex;
        }

        private void UseFillColor_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSymbol();
        }

        private void LineWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSymbol();
        }

        private void LineColor_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FillColor_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FillColor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                FillColor.BackColor = cd.Color;
                UpdateSymbol();
            }
        }
        private void LineColor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                LineColor.BackColor = cd.Color;
                UpdateSymbol();
            }
        }

        private void SimpleSymbolDlg_Load(object sender, EventArgs e)
        {
            LineColor.Refresh();
            FillColor.Refresh();
        }


    }
}

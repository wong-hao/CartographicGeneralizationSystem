using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.ADF.Connection.Local;
namespace NetDesign.Com
{
    public class ColorCombox : ComboBox
    {

        public ColorCombox()
        {

            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            ItemHeight = 20;
            Height = 20;
            Width = 200;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (Items.Count == 0 || e.Index == -1)
                return;
            Rectangle borderRect;
            if ((e.State & DrawItemState.Selected) != 0)
            {
                //填充区域
                borderRect = new Rectangle(3, e.Bounds.Y, e.Bounds.Width - 5, e.Bounds.Height - 2);
                //画边框
                Pen pen = new Pen(Color.FromArgb(0, 0, 0));
                e.Graphics.DrawRectangle(pen, borderRect);
            }
            else
            {
                //填充区域
                borderRect = new Rectangle(3, e.Bounds.Y, e.Bounds.Width - 5, e.Bounds.Height - 2);
                Pen pen = new Pen(Color.FromArgb(255, 255, 255));
                e.Graphics.DrawRectangle(pen, borderRect);
            }

            ColorRamp colorRamp = (ColorRamp)this.Items[e.Index];

            int n = colorRamp.ColorNum;
            ICmykColor f = colorRamp.fromColor;
            ICmykColor t = colorRamp.toColor;
            int dc = (t.Cyan - f.Cyan) / (n - 1);
            int dm = (t.Magenta - f.Magenta) / (n - 1);
            int dy = (t.Yellow - f.Yellow) / (n - 1);

            int c, m, y;
            int wi = (e.Bounds.Width - 5) / n;
            int posx = 3;
            for (int x = 0; x < n; x++)
            {

                c = f.Cyan + x * dc;
                m = f.Magenta + x * dm;
                y = f.Yellow + x * dy;
                CmykColorClass temp = new CmykColorClass();
                temp.Cyan = c;
                temp.Magenta = m;
                temp.Yellow = y;
                temp.Black = 0;
                Color co = Converter.FromRGBColor(new RgbColorClass { RGB = temp.RGB }); 
                SolidBrush solidbrush = new SolidBrush(co);
                borderRect = new Rectangle(posx, e.Bounds.Y, wi, e.Bounds.Height - 2);
                e.Graphics.FillRectangle(solidbrush, borderRect);
                posx += wi;

            }

             
        }

        

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.DropDownHeight = 50;
            this.IntegralHeight = false;
            this.ResumeLayout(false);

        }
    }
    public class ColorRamp
    {
        public ICmykColor fromColor { get; set; }
        public ICmykColor toColor { get; set; }
        public bool Reverse = false;
        public int ColorNum = 4;
        public Dictionary<int,ICmykColor> ColorItems
        {
            get
            {
                Dictionary<int, ICmykColor> colors = new Dictionary<int, ICmykColor>();
                Dictionary<int, ICmykColor> colorsRes = new Dictionary<int, ICmykColor>();
                int n = ColorNum;
                ICmykColor f = fromColor;
                ICmykColor t = toColor;
                int dc = (t.Cyan - f.Cyan) / (n - 1);
                int dm = (t.Magenta - f.Magenta) / (n - 1);
                int dy = (t.Yellow - f.Yellow) / (n - 1);
                int c, m, y;
                for (int x = 0; x < n; x++)
                {

                    c = f.Cyan + x * dc;
                    m = f.Magenta + x * dm;
                    y = f.Yellow + x * dy;
                    CmykColorClass temp = new CmykColorClass();
                    temp.Cyan = c;
                    temp.Magenta = m;
                    temp.Yellow = y;
                    temp.Black = 0;
                    colorsRes.Add(n - 1 - x, temp);
                    colors.Add(x,temp);
                }
                if (Reverse)
                    return colorsRes;
                return colors;
            }
        }

    }
}

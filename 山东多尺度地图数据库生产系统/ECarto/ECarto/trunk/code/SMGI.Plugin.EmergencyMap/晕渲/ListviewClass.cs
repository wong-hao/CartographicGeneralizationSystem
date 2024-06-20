using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace SMGI.Plugin.EmergencyMap
{
    //自定义控件list
    public class ListViewClass:ListView
    {
        /// <summary>
        /// 色带类
        /// </summary>
        public class ColorRamp
        {
            public Color fromColor { get; set; }
            public Color toColor { get; set; }
        }
        int colorIndex = 1;
        public ListViewClass()
        {
            OwnerDraw = true;
            //fillList();
            
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            // e.DrawDefault = true;
            //base.OnDrawItem(e);
            
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }
        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            if (e.Header.Index != colorIndex)
            {
                e.DrawDefault = true;
                base.OnDrawSubItem(e);
            }
            else//重绘制
            {
                //绘制颜色
                Rectangle rect = e.Bounds;
                Rectangle borderRect;

                //填充区域
                borderRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                //画边框
                Pen pen = new Pen(Color.FromArgb(255, 255, 255));
                e.Graphics.DrawRectangle(pen, borderRect);


                ColorRamp colorRamp = (ColorRamp)this.Items[e.ItemIndex].Tag;
                //渐变画刷
                LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, colorRamp.fromColor,
                                                 colorRamp.toColor, LinearGradientMode.Horizontal);
                //填充区域
                borderRect = new Rectangle(rect.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

                e.Graphics.FillRectangle(brush, borderRect);
            }
        }
        public void fillList()
        {
            int cw = this.Width;
            ColumnHeader header = new ColumnHeader();
            header.Text = "序号";
            header.Width = (int)(0.4 * cw);
            this.Columns.Add(header);
            header = new ColumnHeader();
            header.Text = "晕眩颜色";
            header.Width = cw - (int)(0.4 * cw);
            this.Columns.Add(header);

            ListViewItem item = new ListViewItem(new string[] { "g", "f" });
            ColorRamp colorRamp = new ColorRamp();
            colorRamp.fromColor = Color.FromArgb(176, 176, 176);
            colorRamp.toColor = Color.FromArgb(255, 0, 0);
            item.Tag = colorRamp;
            this.Items.Add(item);
          
           
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Common
{
    public partial class ColorRampComboBox : ComboBox
    {
        public ColorRampComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (Items.Count == 0 || e.Index == -1)
                return;

            e.DrawBackground();
            e.DrawFocusRectangle();
            try
            {
                Image image = (Image)Items[e.Index];
                System.Drawing.Rectangle rect = e.Bounds;
                e.Graphics.DrawImage(image, rect);
            }
            catch
            {
            }
            finally
            {
                base.OnDrawItem(e);
            }

            
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;

namespace SMGI.Common
{
    public partial class ColorRampSetForm : Form
    {
        public IColorRamp ColorRamp
        {
            get;
            protected set;
        }

        public bool Invert
        {
            get;
            protected set;
        }

        public ColorRampSetForm(IColorRamp clrRamp, bool invert, int x, int y)
        {
            InitializeComponent();

            ColorRamp = clrRamp;
            cbInvert.Checked = invert;

            this.Location = new Point(x, y);
        }

        private void ColorRampSetForm_Load(object sender, EventArgs e)
        {
            try
            {
                //加载系统样式
                string defaultStyle = System.IO.Path.Combine(ESRI.ArcGIS.RuntimeManager.ActiveRuntime.Path, "Styles\\ESRI.ServerStyle");
                symbologyControl.LoadStyleFile(defaultStyle);
                symbologyControl.StyleClass = ESRI.ArcGIS.Controls.esriSymbologyStyleClass.esriStyleClassColorRamps;
                ISymbologyStyleClass symStyleClass = symbologyControl.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
                #region 排除非算法色带项(IAlgorithmicColorRamp)
                int count = symStyleClass.get_ItemCount(symStyleClass.StyleCategory);
                for (int i = count-1; i >= 0; --i)
                {
                    var item = symStyleClass.GetItem(i).Item;
                    if (item is IAlgorithmicColorRamp)
                        continue;//保留

                    if (item is IMultiPartColorRamp)
                    {
                        IMultiPartColorRamp multiColorRamp = item as IMultiPartColorRamp;
                        bool bAlgorith = true;
                        for (int j = 0; j < multiColorRamp.NumberOfRamps; ++j)
                        {
                            if (!(multiColorRamp.get_Ramp(j) is IAlgorithmicColorRamp))
                            {
                                bAlgorith = false;
                                break;
                            }
                        }

                        if (bAlgorith)
                            continue;//保留
                    }

                    //移除
                    symStyleClass.RemoveItem(i);
                }
                #endregion

                //添加当前色带
                IStyleGalleryItem styleGalleryItem = new ServerStyleGalleryItemClass();
                styleGalleryItem.Item = ColorRamp;
                symStyleClass.AddItem(styleGalleryItem, 0);

                //将样式转为图片并添加到色带组合框中
                for (int i = 0; i < symStyleClass.get_ItemCount(symStyleClass.StyleCategory); ++i)
                {
                    stdole.IPictureDisp pic = symStyleClass.PreviewItem(symStyleClass.GetItem(i), cmbColorRamp.Width, cmbColorRamp.Height);
                    Image image = Image.FromHbitmap(new System.IntPtr(pic.Handle));
                    cmbColorRamp.Items.Add(image);
                }

                if (cmbColorRamp.Items.Count > 0)
                    cmbColorRamp.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            int index = cmbColorRamp.SelectedIndex;
            if (index == -1)
            {
                MessageBox.Show("请选择有效的色带！");
                return;
            }

            ISymbologyStyleClass symStyleClass = symbologyControl.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
            IStyleGalleryItem styleGalleryItem = symStyleClass.GetItem(index);

            ColorRamp = (IColorRamp)styleGalleryItem.Item;
            Invert = cbInvert.Checked;

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}

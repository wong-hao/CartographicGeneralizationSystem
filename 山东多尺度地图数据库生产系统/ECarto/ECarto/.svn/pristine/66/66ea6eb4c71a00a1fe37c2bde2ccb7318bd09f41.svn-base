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
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Framework;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Common
{
    /// <summary>
    /// 栅格图层属性设置对话框：目前仅实现了符号系统与显示模块
    /// @LZ-20211119
    /// </summary>
    public partial class RasterLayerPropertyForm : Form
    {
        private IRasterLayer _rasterLayer;
        public IRasterRenderer RasterRenderer
        {
            get;
            protected set;
        }

        private IColorRamp _curClrRamp;
        private Dictionary<IColorRamp, List<KeyValuePair<double, string>>> _clrRamp2LabelInfo;//高级标注信息

        private int _picSaleRatio;

        public RasterLayerPropertyForm(IRasterLayer rasterLayer)
        {
            InitializeComponent();

            _rasterLayer = rasterLayer;
            RasterRenderer = rasterLayer.Renderer;

            _curClrRamp = null;
            _clrRamp2LabelInfo = new Dictionary<IColorRamp, List<KeyValuePair<double, string>>>();

            _picSaleRatio = 1;
        }

        private void RasterLayerPropertyForm_Load(object sender, EventArgs e)
        {
            try
            {
                Renderertab.Region = new Region(new RectangleF(StretchPage.Left, StretchPage.Top, StretchPage.Width, StretchPage.Height));

                #region 初始化显示Tab控件
                IRasterDisplayProps rdp = _rasterLayer.Renderer as IRasterDisplayProps;
                tbContrastRatio.Text = rdp.ContrastValue.ToString();
                tbBrightness.Text = rdp.BrightnessValue.ToString();
                tbTrans.Text = rdp.TransparencyValue.ToString();
                #endregion

                #region 初始化符号系统Tab控件
                if (_rasterLayer.BandCount == 1)//初始化页面：拉伸
                {
                    RendererTypeListBox.Items.Add("拉伸");
                    RendererTypeListBox.SelectedIndex = 0;

                    Bandpanel.Visible = false;

                    if (_rasterLayer.Renderer is IRasterStretchColorRampRenderer)
                    {
                        string defaultStyle = System.IO.Path.Combine(ESRI.ArcGIS.RuntimeManager.ActiveRuntime.Path, "Styles\\ESRI.ServerStyle");
                        InitStretchPage(_rasterLayer, defaultStyle);
                    }
                }
                else//初始化页面：RGB合成、拉伸
                {
                    RendererTypeListBox.Items.Add("拉伸");
                    RendererTypeListBox.Items.Add("RGB合成");

                    Bandpanel.Visible = true;

                    string defaultStyle = System.IO.Path.Combine(ESRI.ArcGIS.RuntimeManager.ActiveRuntime.Path, "Styles\\ESRI.ServerStyle");
                    InitStretchPage(_rasterLayer, defaultStyle);

                    InitRGBPage(_rasterLayer);

                    if (_rasterLayer.Renderer is IRasterRGBRenderer)
                    {
                        RendererTypeListBox.SelectedIndex = 1;
                    }
                    else if (_rasterLayer.Renderer is IRasterStretchColorRampRenderer)
                    {
                        RendererTypeListBox.SelectedIndex = 0;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                System.Diagnostics.Trace.WriteLine(ex.Source);

                MessageBox.Show(ex.Message);
            }
        }

        private void RendererTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = RendererTypeListBox.SelectedItem;
            if (item == null)
                return;

            if (item.ToString() == "拉伸")
            {
                Renderertab.SelectedTab = StretchPage;
            }
            else if (item.ToString() == "RGB合成")
            {
                Renderertab.SelectedTab = RGBPage;
            }
        }

        private void Renderertab_SelectedIndexChanged(object sender, EventArgs e)
        {
            //暂无操作
        }

        private void cmbBand_SelectedIndexChanged(object sender, EventArgs e)
        {
            IRasterBand rasterBand = (_rasterLayer.Raster as IRasterBandCollection).get_BandByName(cmbBand.Text);
            //rasterBand.ComputeStatsAndHist();
            if (rasterBand == null || rasterBand.Statistics == null)
                return;

            tbMaxValue.Tag = rasterBand.Statistics.Maximum;
            tbMaxValue.Text = string.Format("{0:.####}", rasterBand.Statistics.Maximum);
            tbMinValue.Tag = rasterBand.Statistics.Minimum;
            tbMinValue.Text = string.Format("{0:.####}", rasterBand.Statistics.Minimum);
            if (tbMinValue.Text == "")
                tbMinValue.Text = rasterBand.Statistics.Minimum.ToString();

            tbMaxValueLabel.Text = string.Format("高:{0}", tbMaxValue.Text);
            tbMinValueLabel.Text = string.Format("高:{0}", tbMinValue.Text);
        }

        private void btnLabel_Click(object sender, EventArgs e)
        {
            List<KeyValuePair<double, string>> labelInfo = null;
            if (_clrRamp2LabelInfo.ContainsKey(_curClrRamp))
            {
                labelInfo = _clrRamp2LabelInfo[_curClrRamp];
            }

            AdvancedLabelOfStretchForm frm = new AdvancedLabelOfStretchForm(_curClrRamp, double.Parse(tbMaxValue.Tag.ToString()), double.Parse(tbMinValue.Tag.ToString()), labelInfo);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            
            //1.添加色带配置
            ISymbologyStyleClass symStyleClass = symbologyControl.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
            IStyleGalleryItem styleGalleryItem = new ServerStyleGalleryItemClass();
            styleGalleryItem.Item = frm.ColorRamp;
            symStyleClass.AddItem(styleGalleryItem, symStyleClass.ItemCount);

            //2.生成image，并添加到色带组合框中
            _picSaleRatio = frm.LabelValueList.Count < 6 ? 5 * frm.LabelValueList.Count : 25;
            var totalImage = new Bitmap(_picSaleRatio * cmbColorRamp.Width, _picSaleRatio * cmbColorRamp.Height);
            Graphics g = Graphics.FromImage(totalImage);
            g.Clear(SystemColors.AppWorkspace);

            double deltaValue = frm.LabelValueList.First() - frm.LabelValueList.Last();
            double imageTotalLen = _picSaleRatio * cmbColorRamp.Width;
            int len = 0;
            IMultiPartColorRamp multiClrRamp = frm.ColorRamp as IMultiPartColorRamp;
            for (int i = 0; i < multiClrRamp.NumberOfRamps; ++i)
            {
                int imageLen = (int)(imageTotalLen * (frm.LabelValueList[frm.LabelValueList.Count - 2 - i] - frm.LabelValueList[frm.LabelValueList.Count -1 - i]) / deltaValue);

                styleGalleryItem = new ServerStyleGalleryItemClass();
                styleGalleryItem.Item = multiClrRamp.get_Ramp(i);
                stdole.IPictureDisp pic = symStyleClass.PreviewItem(styleGalleryItem, imageLen, _picSaleRatio * cmbColorRamp.Height);
                Image image = Image.FromHbitmap(new System.IntPtr(pic.Handle));

                g.DrawImage(image, new Point(len, 0));

                len += image.Width;

                image.Dispose();
            }
            g.Dispose();
            cmbColorRamp.SelectedIndex = cmbColorRamp.Items.Add(totalImage);  
          
            //3.更新成员变量
            labelInfo = new List<KeyValuePair<double,string>>();
            for(int i =0; i < frm.LabelValueList.Count; ++i)
            {
                labelInfo.Add(new KeyValuePair<double, string>(frm.LabelValueList[i],frm.LabelTextList[i]));
            }
            _clrRamp2LabelInfo.Add(_curClrRamp, labelInfo);
        }

        private void cmbColorRamp_SelectedIndexChanged(object sender, EventArgs e)
        {
            Image image = (Image)cmbColorRamp.SelectedItem;
            if (image == null)
                return;

            if (cbStretchInvert.Checked)
            {
                Bitmap b = new Bitmap(image);
                b.RotateFlip(RotateFlipType.Rotate270FlipXY);
                ColorRampPicBox.Image = b;
                ColorRampPicBox.Refresh();
            }
            else
            {
                Bitmap b = new Bitmap(image);
                b.RotateFlip(RotateFlipType.Rotate90FlipXY);
                ColorRampPicBox.Image = b;
                ColorRampPicBox.Refresh();
            }

            //更新色带成员变量
            ISymbologyStyleClass symStyleClass = symbologyControl.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
            IStyleGalleryItem styleGalleryItem = symStyleClass.GetItem(cmbColorRamp.SelectedIndex);
            _curClrRamp = (IColorRamp)styleGalleryItem.Item;//拉伸渲染的当前色带设置
        }

        private void cbStretchBackground_CheckedChanged(object sender, EventArgs e)
        {
            tbStretchColorG.Enabled = cbStretchBackground.Checked;
        }

        private void btnBackColor_Click(object sender, EventArgs e)
        {
            var location = GetSubControlLocationInForm(sender as Control);
            location.Offset(this.Location);//相对于_hWnd
            int frmLeftBorderWidth = (this.Width - this.ClientRectangle.Width) / 2;//窗体左侧边框宽度
            int frmTopBorderHeight = (this.Height - this.ClientRectangle.Height) - frmLeftBorderWidth;//窗体顶部边框高度
            location.X += frmLeftBorderWidth;
            location.Y += frmTopBorderHeight;

            tagRECT tagRect = new tagRECT();
            tagRect.left = location.X;
            tagRect.top = location.Y;
            tagRect.right = location.X + (sender as Control).Width;
            tagRect.bottom = location.Y + (sender as Control).Height;

            IColor color = Helper.GetEsriColorByCMYKText((sender as Control).Tag.ToString());
            var colorPalette = new ColorPalette();
            if (colorPalette.TrackPopupMenu(ref tagRect, color, true, 0))
            {
                (sender as Control).BackColor = Helper.GetColorByEsriColor(colorPalette.Color);
                (sender as Control).Tag = Helper.GetCMYKTextByEsriColor(colorPalette.Color);
            }
        }

        private void cmbStretchStretchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //暂无操作
        }

        private void cbStretchInvert_CheckedChanged(object sender, EventArgs e)
        {
            Image image = (Image)cmbColorRamp.SelectedItem;
            if (cbStretchInvert.Checked)
            {
                Bitmap b = new Bitmap(image);
                b.RotateFlip(RotateFlipType.Rotate270FlipXY);
                ColorRampPicBox.Image = b;
                ColorRampPicBox.Refresh();
            }
            else
            {
                Bitmap b = new Bitmap(image);
                b.RotateFlip(RotateFlipType.Rotate90FlipXY);
                ColorRampPicBox.Image = b;
                ColorRampPicBox.Refresh();
            }
        }

        private void cbRGBBackground_CheckedChanged(object sender, EventArgs e)
        {
            tbRGBColorR.Enabled = cbRGBBackground.Checked;
            tbRGBColorG.Enabled = cbRGBBackground.Checked;
            tbRGBColorB.Enabled = cbRGBBackground.Checked;
        }

        private void btnApp_Click(object sender, EventArgs e)
        {
            //更新
            UpdateLayerRenderer();

            GApplication.Application.TOCControl.Update();
            GApplication.Application.TOCControl.Refresh();
            (GApplication.Application.ActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, (GApplication.Application.ActiveView).Extent);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            //更新
            UpdateLayerRenderer();

            GApplication.Application.TOCControl.Update();
            GApplication.Application.TOCControl.Refresh();
            (GApplication.Application.ActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, (GApplication.Application.ActiveView).Extent);

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void InitStretchPage(IRasterLayer rasterLayer, string defaultStyle)
        {
            if (rasterLayer.Renderer is IRasterStretchColorRampRenderer)
            {
                IRasterStretchColorRampRenderer stretchRenderer = rasterLayer.Renderer as IRasterStretchColorRampRenderer;
                
                for (int i = 0; i < rasterLayer.BandCount; ++i)
                {
                    cmbBand.Items.Add((rasterLayer.Raster as IRasterBandCollection).Item(i).Bandname);
                }
                cmbBand.SelectedIndex = stretchRenderer.BandIndex;

                tbMaxValue.Tag = (stretchRenderer as IRasterStretchMinMax).StretchMax;
                tbMaxValue.Text = string.Format("{0:.####}", (stretchRenderer as IRasterStretchMinMax).StretchMax);
                tbMinValue.Tag = (stretchRenderer as IRasterStretchMinMax).StretchMin;
                tbMinValue.Text = string.Format("{0:.####}", (stretchRenderer as IRasterStretchMinMax).StretchMin);
                if (tbMinValue.Text == "")
                    tbMinValue.Text = (stretchRenderer as IRasterStretchMinMax).StretchMin.ToString();

                tbMaxValueLabel.Text = (stretchRenderer as IRasterStretchColorRampRenderer).LabelHigh.ToString();
                tbMediumValueLabel.Text = (stretchRenderer as IRasterStretchColorRampRenderer).LabelMedium.ToString();
                tbMinValueLabel.Text = (stretchRenderer as IRasterStretchColorRampRenderer).LabelLow.ToString();

                InitColorRampComboBox(defaultStyle, stretchRenderer.ColorRamp);
            }
            else
            {
                for (int i = 0; i < rasterLayer.BandCount; ++i)
                {
                    cmbBand.Items.Add((rasterLayer.Raster as IRasterBandCollection).Item(i).Bandname);
                }
                cmbBand.SelectedIndex = 0;

                IRasterBand rasterBand = (rasterLayer.Raster as IRasterBandCollection).Item(0);
                if (rasterBand.Statistics != null)
                {
                    tbMaxValue.Tag = rasterBand.Statistics.Maximum;
                    tbMaxValue.Text = string.Format("{0:.####}", rasterBand.Statistics.Maximum);
                    tbMinValue.Tag = rasterBand.Statistics.Minimum;
                    tbMinValue.Text = string.Format("{0:.####}", rasterBand.Statistics.Minimum);
                    if (tbMinValue.Text == "")
                        tbMinValue.Text = rasterBand.Statistics.Minimum.ToString();

                    tbMaxValueLabel.Text = string.Format("高:{0}", tbMaxValue.Text);
                    tbMinValueLabel.Text = string.Format("高:{0}", tbMinValue.Text);
                }

                InitColorRampComboBox(defaultStyle, null);
            }

            if (rasterLayer.Renderer is IRasterStretch2)
            {
                cbStretchBackground.Checked = (rasterLayer.Renderer as IRasterStretch2).Background;
                if ((rasterLayer.Renderer as IRasterStretch2).BackgroundValue is double[] && ((rasterLayer.Renderer as IRasterStretch2).BackgroundValue as double[]).Count() > 1)
                {
                    tbStretchColorG.Text = ((rasterLayer.Renderer as IRasterStretch2).BackgroundValue as double[])[1].ToString();
                }
                else if ((rasterLayer.Renderer as IRasterStretch2).BackgroundValue is double)
                {
                    tbStretchColorG.Text = (rasterLayer.Renderer as IRasterStretch2).BackgroundValue.ToString();
                }


                btnStretchBGColor.BackColor = Helper.GetColorByEsriColor((rasterLayer.Renderer as IRasterStretch2).BackgroundColor);
                btnStretchBGColor.Tag = Helper.GetCMYKTextByEsriColor((rasterLayer.Renderer as IRasterStretch2).BackgroundColor);

                btnStretchNoDataColor.BackColor = Helper.GetColorByEsriColor((rasterLayer.Renderer as IRasterDisplayProps).NoDataColor);
                btnStretchNoDataColor.Tag = Helper.GetCMYKTextByEsriColor((rasterLayer.Renderer as IRasterDisplayProps).NoDataColor);

                InitStetchTypeComboBox(cmbStretchStretchType, (rasterLayer.Renderer as IRasterStretch2).StretchType);

                cbStretchInvert.Checked = (rasterLayer.Renderer as IRasterStretch2).Invert;
            }
        }

        private void InitRGBPage(IRasterLayer rasterLayer)
        {
            //初始化波段列表
            ColorDataGridView.Rows.Clear();
            (ColorDataGridView.Columns["BandColumn"] as DataGridViewComboBoxColumn).Items.Clear();
            for (int i = 0; i < rasterLayer.BandCount; ++i)
            {
                (ColorDataGridView.Columns["BandColumn"] as DataGridViewComboBoxColumn).Items.Add((rasterLayer.Raster as IRasterBandCollection).Item(i).Bandname);
            }

            if (rasterLayer.Renderer is IRasterRGBRenderer)
            {
                IRasterRGBRenderer2 rgbRenderer = rasterLayer.Renderer as IRasterRGBRenderer2;

                int rowIndex = ColorDataGridView.Rows.Add();
                ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = rgbRenderer.UseRedBand;
                ColorDataGridView.Rows[rowIndex].Cells["ChannelColumn"].Value ="红色";
                if (rgbRenderer.UseRedBand)
                {
                    ((DataGridViewComboBoxCell)ColorDataGridView.Rows[rowIndex].Cells["BandColumn"]).Value = (rasterLayer.Raster as IRasterBandCollection).Item(rgbRenderer.RedBandIndex).Bandname;
                }
                rowIndex = ColorDataGridView.Rows.Add();
                ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = rgbRenderer.UseGreenBand;
                ColorDataGridView.Rows[rowIndex].Cells["ChannelColumn"].Value = "绿色";
                if (rgbRenderer.UseGreenBand)
                {
                    ((DataGridViewComboBoxCell)ColorDataGridView.Rows[rowIndex].Cells["BandColumn"]).Value = (rasterLayer.Raster as IRasterBandCollection).Item(rgbRenderer.GreenBandIndex).Bandname;
                }
                rowIndex = ColorDataGridView.Rows.Add();
                ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = rgbRenderer.UseBlueBand;
                ColorDataGridView.Rows[rowIndex].Cells["ChannelColumn"].Value = "蓝色";
                if (rgbRenderer.UseBlueBand)
                {
                    ((DataGridViewComboBoxCell)ColorDataGridView.Rows[rowIndex].Cells["BandColumn"]).Value = (rasterLayer.Raster as IRasterBandCollection).Item(rgbRenderer.BlueBandIndex).Bandname;
                }
                rowIndex = ColorDataGridView.Rows.Add();
                ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = rgbRenderer.UseAlphaBand;
                ColorDataGridView.Rows[rowIndex].Cells["ChannelColumn"].Value = "Alpha";
                if (rgbRenderer.UseAlphaBand)
                {
                    ((DataGridViewComboBoxCell)ColorDataGridView.Rows[rowIndex].Cells["BandColumn"]).Value = (rasterLayer.Raster as IRasterBandCollection).Item(rgbRenderer.AlphaBandIndex).Bandname;
                }
                
            }
            else
            {
                int rowIndex = ColorDataGridView.Rows.Add();
                ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = false;
                ColorDataGridView.Rows[rowIndex].Cells["ChannelColumn"].Value = "红色";
                if (rasterLayer.BandCount > 0)
                {
                    ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = true;
                    ((DataGridViewComboBoxCell)ColorDataGridView.Rows[rowIndex].Cells["BandColumn"]).Value = (rasterLayer.Raster as IRasterBandCollection).Item(0).Bandname;
                }

                rowIndex = ColorDataGridView.Rows.Add();
                ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = false;
                ColorDataGridView.Rows[rowIndex].Cells["ChannelColumn"].Value = "绿色";
                if (rasterLayer.BandCount > 1)
                {
                    ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = true;
                    ((DataGridViewComboBoxCell)ColorDataGridView.Rows[rowIndex].Cells["BandColumn"]).Value = (rasterLayer.Raster as IRasterBandCollection).Item(1).Bandname;
                }

                rowIndex = ColorDataGridView.Rows.Add();
                ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = false;
                ColorDataGridView.Rows[rowIndex].Cells["ChannelColumn"].Value = "蓝色";
                if (rasterLayer.BandCount > 2)
                {
                    ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = true;
                    ((DataGridViewComboBoxCell)ColorDataGridView.Rows[rowIndex].Cells["BandColumn"]).Value = (rasterLayer.Raster as IRasterBandCollection).Item(2).Bandname;
                }

                rowIndex = ColorDataGridView.Rows.Add();
                ((DataGridViewCheckBoxCell)ColorDataGridView.Rows[rowIndex].Cells["DisplayStateColumn"]).Value = false;
                ColorDataGridView.Rows[rowIndex].Cells["ChannelColumn"].Value = "Alpha";
            }

            if (rasterLayer.Renderer is IRasterStretch2)
            {
                cbRGBBackground.Checked = (rasterLayer.Renderer as IRasterStretch2).Background;
                if ((rasterLayer.Renderer as IRasterStretch2).BackgroundValue is double[] && ((rasterLayer.Renderer as IRasterStretch2).BackgroundValue as double[]).Count() > 2)
                {
                    tbRGBColorR.Text = ((rasterLayer.Renderer as IRasterStretch2).BackgroundValue as double[])[0].ToString();
                    tbRGBColorG.Text = ((rasterLayer.Renderer as IRasterStretch2).BackgroundValue as double[])[1].ToString();
                    tbRGBColorB.Text = ((rasterLayer.Renderer as IRasterStretch2).BackgroundValue as double[])[2].ToString();
                }

                btnRGBBGColor.BackColor = Helper.GetColorByEsriColor((rasterLayer.Renderer as IRasterStretch2).BackgroundColor);
                btnRGBBGColor.Tag = Helper.GetCMYKTextByEsriColor((rasterLayer.Renderer as IRasterStretch2).BackgroundColor);

                btnRGBNoDataColor.BackColor = Helper.GetColorByEsriColor((rasterLayer.Renderer as IRasterDisplayProps).NoDataColor);
                btnRGBNoDataColor.Tag = Helper.GetCMYKTextByEsriColor((rasterLayer.Renderer as IRasterDisplayProps).NoDataColor);

                InitStetchTypeComboBox(cmbRGBStretchType, (rasterLayer.Renderer as IRasterStretch2).StretchType);

                cbRGBInvert.Checked = (rasterLayer.Renderer as IRasterStretch2).Invert;
            }
        }

        private void InitColorRampComboBox(string styleFile, IColorRamp colorRamp)
        {
            try
            {
                //加载样式文件
                symbologyControl.LoadStyleFile(styleFile);
                symbologyControl.StyleClass = ESRI.ArcGIS.Controls.esriSymbologyStyleClass.esriStyleClassColorRamps;
                ISymbologyStyleClass symStyleClass = symbologyControl.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
                #region 排除非算法色带项(IAlgorithmicColorRamp)
                int count = symStyleClass.get_ItemCount(symStyleClass.StyleCategory);
                for (int i = count - 1; i >= 0; --i)
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

                //将系统样式转为图片并添加到色带组合框中
                for (int i = 0; i < symStyleClass.get_ItemCount(symStyleClass.StyleCategory); ++i)
                {
                    stdole.IPictureDisp pic = symStyleClass.PreviewItem(symStyleClass.GetItem(i), _picSaleRatio * cmbColorRamp.Width, _picSaleRatio * cmbColorRamp.Height);
                    Image image = Image.FromHbitmap(new System.IntPtr(pic.Handle));
                    cmbColorRamp.Items.Add(image);
                }

                //添加当前色带
                if (colorRamp == null)
                {
                    colorRamp = new AlgorithmicColorRampClass();
                    (colorRamp as IAlgorithmicColorRamp).Size = 255;
                    (colorRamp as IAlgorithmicColorRamp).FromColor = new RgbColorClass() { Red = 0, Blue = 0, Green = 0 };
                    (colorRamp as IAlgorithmicColorRamp).ToColor = new RgbColorClass() { Red = 255, Blue = 255, Green = 255 };
                    bool bSuccess;
                    colorRamp.CreateRamp(out bSuccess);
                }
                IStyleGalleryItem styleGalleryItem = new ServerStyleGalleryItemClass();
                styleGalleryItem.Item = colorRamp;
                symStyleClass.AddItem(styleGalleryItem, symStyleClass.ItemCount);
                
                //添加当前色带图片到组合框
                if (_rasterLayer.Renderer is IRasterStretchAdvancedLabels && (_rasterLayer.Renderer as IRasterStretchAdvancedLabels).UseAdvancedLabeling && 
                    colorRamp is IMultiPartColorRamp && (colorRamp as IMultiPartColorRamp).NumberOfRamps == (_rasterLayer.Renderer as IRasterStretchAdvancedLabels).NumLabels - 1)//高级标注
                {
                    _picSaleRatio = ((colorRamp as IMultiPartColorRamp).NumberOfRamps + 1) < 6 ? 5 * ((colorRamp as IMultiPartColorRamp).NumberOfRamps + 1) : 25;

                    var totalImage = new Bitmap(_picSaleRatio * cmbColorRamp.Width, _picSaleRatio * cmbColorRamp.Height);
                    Graphics g = Graphics.FromImage(totalImage);
                    g.Clear(SystemColors.AppWorkspace);

                    double deltaValue = (_rasterLayer.Renderer as IRasterStretchAdvancedLabels).get_LabelValue(0) - (_rasterLayer.Renderer as IRasterStretchAdvancedLabels).get_LabelValue((_rasterLayer.Renderer as IRasterStretchAdvancedLabels).NumLabels - 1);
                    double imageTotalLen = _picSaleRatio * cmbColorRamp.Width;
                    int len = 0;
                    for (int i = 0; i < (colorRamp as IMultiPartColorRamp).NumberOfRamps; ++i)
                    {
                        int imageLen = (int)(imageTotalLen * ((_rasterLayer.Renderer as IRasterStretchAdvancedLabels).get_LabelValue((_rasterLayer.Renderer as IRasterStretchAdvancedLabels).NumLabels - 2 - i) - (_rasterLayer.Renderer as IRasterStretchAdvancedLabels).get_LabelValue((_rasterLayer.Renderer as IRasterStretchAdvancedLabels).NumLabels - 1 - i)) / deltaValue);

                        var styleGallerySubItem = new ServerStyleGalleryItemClass();
                        styleGallerySubItem.Item = (colorRamp as IMultiPartColorRamp).get_Ramp(i);
                        stdole.IPictureDisp pic = symStyleClass.PreviewItem(styleGallerySubItem, imageLen, 20 * cmbColorRamp.Height);
                        Image image = Image.FromHbitmap(new System.IntPtr(pic.Handle));

                        g.DrawImage(image, new Point(len, 0));

                        len += image.Width;

                        image.Dispose();
                    }
                    g.Dispose();
                    cmbColorRamp.SelectedIndex = cmbColorRamp.Items.Add(totalImage);

                    //更新成员变量
                    var labelInfo = new List<KeyValuePair<double, string>>();
                    for (int i = 0; i < (_rasterLayer.Renderer as IRasterStretchAdvancedLabels).NumLabels; ++i)
                    {
                        labelInfo.Add(new KeyValuePair<double, string>((_rasterLayer.Renderer as IRasterStretchAdvancedLabels).get_LabelValue(i), (_rasterLayer.Renderer as IRasterStretchAdvancedLabels).get_LabelText(i)));
                    }
                    _clrRamp2LabelInfo.Add(_curClrRamp, labelInfo);
                }
                else
                {
                    stdole.IPictureDisp pic = symStyleClass.PreviewItem(styleGalleryItem, _picSaleRatio * cmbColorRamp.Width, _picSaleRatio * cmbColorRamp.Height);
                    Image image = Image.FromHbitmap(new System.IntPtr(pic.Handle));
                    cmbColorRamp.SelectedIndex = cmbColorRamp.Items.Add(image);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }

        private void InitStetchTypeComboBox(ComboBox sender, esriRasterStretchTypesEnum stretchType)
        {
            sender.ValueMember = "Key";
            sender.DisplayMember = "Value";

            int selIndex = 0;
            int index = sender.Items.Add(new KeyValuePair<esriRasterStretchTypesEnum, string>(esriRasterStretchTypesEnum.esriRasterStretch_NONE, "无"));
            if (stretchType == esriRasterStretchTypesEnum.esriRasterStretch_NONE)
            {
                selIndex = index;
            }
            index = sender.Items.Add(new KeyValuePair<esriRasterStretchTypesEnum, string>(esriRasterStretchTypesEnum.esriRasterStretch_Custom, "自定义"));
            if (stretchType == esriRasterStretchTypesEnum.esriRasterStretch_Custom)
            {
                selIndex = index;
            }
            index = sender.Items.Add(new KeyValuePair<esriRasterStretchTypesEnum, string>(esriRasterStretchTypesEnum.esriRasterStretch_StandardDeviations, "标准差"));
            if (stretchType == esriRasterStretchTypesEnum.esriRasterStretch_StandardDeviations)
            {
                selIndex = index;
            }
            index = sender.Items.Add(new KeyValuePair<esriRasterStretchTypesEnum, string>(esriRasterStretchTypesEnum.esriRasterStretch_HistogramEqualize, "直方图均衡化"));
            if (stretchType == esriRasterStretchTypesEnum.esriRasterStretch_HistogramEqualize)
            {
                selIndex = index;
            }
            index = sender.Items.Add(new KeyValuePair<esriRasterStretchTypesEnum, string>(esriRasterStretchTypesEnum.esriRasterStretch_MinimumMaximum, "最值"));
            if (stretchType == esriRasterStretchTypesEnum.esriRasterStretch_MinimumMaximum)
            {
                selIndex = index;
            }
            index = sender.Items.Add(new KeyValuePair<esriRasterStretchTypesEnum, string>(esriRasterStretchTypesEnum.esriRasterStretch_HistogramSpecification, "直方图规定化"));
            if (stretchType == esriRasterStretchTypesEnum.esriRasterStretch_HistogramSpecification)
            {
                selIndex = index;
            }
            index = sender.Items.Add(new KeyValuePair<esriRasterStretchTypesEnum, string>(esriRasterStretchTypesEnum.esriRasterStretch_PercentMinimumMaximum, "百分比截断"));
            if (stretchType == esriRasterStretchTypesEnum.esriRasterStretch_PercentMinimumMaximum)
            {
                selIndex = index;
            }
            index = sender.Items.Add(new KeyValuePair<esriRasterStretchTypesEnum, string>(esriRasterStretchTypesEnum.esriRasterStretch_ESRI, "Esri"));
            if (stretchType == esriRasterStretchTypesEnum.esriRasterStretch_ESRI)
            {
                selIndex = index;
            }

            sender.SelectedIndex = selIndex;
        }


        /// <summary>
        /// 获取子控件在窗体中的位置（屏幕坐标）
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private Point GetSubControlLocationInForm(Control c)
        {
            Point location = new Point(0, 0);
            do
            {
                location.Offset(c.Location);
                c = c.Parent;
            }
            while (c != this && c != null);//循环到当前窗体

            return location;
        }

        private void UpdateLayerRenderer()
        {
            //符号系统
            if (Renderertab.SelectedTab == StretchPage)//拉伸
            {
                #region 拉伸
                if(!(RasterRenderer is IRasterStretchColorRampRenderer))//原始非拉伸
                {
                    _rasterLayer.Renderer = new RasterStretchColorRampRendererClass();
                    RasterRenderer = _rasterLayer.Renderer;
                    RasterRenderer.Raster = _rasterLayer.Raster;
                }

                //波段
                (RasterRenderer as IRasterStretchColorRampRenderer).BandIndex = (_rasterLayer.Raster as IRasterBandCollection).get_BandIndex(cmbBand.Text);

                //色带(附高级标注设置)
                (RasterRenderer as IRasterStretchColorRampRenderer).ColorRamp = _curClrRamp;//色带
                if (_clrRamp2LabelInfo.ContainsKey(_curClrRamp))//高级标注
                {
                    (RasterRenderer as IRasterStretchAdvancedLabels).UseAdvancedLabeling = true;
                    (RasterRenderer as IRasterStretchAdvancedLabels).NumLabels = _clrRamp2LabelInfo[_curClrRamp].Count;
                    for (int i = 0; i < _clrRamp2LabelInfo[_curClrRamp].Count; ++i)
                    {
                        (RasterRenderer as IRasterStretchAdvancedLabels).set_LabelValue(i, _clrRamp2LabelInfo[_curClrRamp][i].Key);//降序
                        (RasterRenderer as IRasterStretchAdvancedLabels).set_LabelText(i, _clrRamp2LabelInfo[_curClrRamp][i].Value);
                    }
                }


                //背景色、NoData颜色
                (RasterRenderer as IRasterStretch2).Background = cbStretchBackground.Checked;
                (RasterRenderer as IRasterStretch2).BackgroundColor = Helper.GetEsriColorByCMYKText(btnStretchBGColor.Tag.ToString());
                if (cbStretchBackground.Checked)
                {
                    double val;
                    double.TryParse(tbStretchColorG.Text, out val);

                    (RasterRenderer as IRasterStretch2).BackgroundValue = val;
                }
                (RasterRenderer as IRasterDisplayProps).NoDataColor = Helper.GetEsriColorByCMYKText(btnStretchNoDataColor.Tag.ToString());

                //拉伸类型
                KeyValuePair<esriRasterStretchTypesEnum, string> stretchTypeItem = (KeyValuePair<esriRasterStretchTypesEnum, string>)cmbStretchStretchType.SelectedItem;
                (RasterRenderer as IRasterStretch2).StretchType = stretchTypeItem.Key;

                (RasterRenderer as IRasterStretch).Invert = cbStretchInvert.Checked;//反向
                #endregion
            }
            else if (Renderertab.SelectedTab == RGBPage)//RGB
            {
                #region RGB
                if (!(RasterRenderer is IRasterRGBRenderer))//原始非RGB
                {
                    _rasterLayer.Renderer = new RasterRGBRendererClass();
                    RasterRenderer = _rasterLayer.Renderer;
                    RasterRenderer.Raster = _rasterLayer.Raster;
                }

                //波段
                for (int i = 0; i < ColorDataGridView.Rows.Count; ++i)
                {
                    #region 波段设置
                    DataGridViewCheckBoxCell useBandCell = ColorDataGridView.Rows[i].Cells["DisplayStateColumn"] as DataGridViewCheckBoxCell;
                    DataGridViewTextBoxCell ChannelCell = ColorDataGridView.Rows[i].Cells["ChannelColumn"] as DataGridViewTextBoxCell;
                    DataGridViewComboBoxCell bandCell = ColorDataGridView.Rows[i].Cells["BandColumn"] as DataGridViewComboBoxCell;

                    switch (ChannelCell.Value.ToString())
                    {
                        case "红色":
                            (RasterRenderer as IRasterRGBRenderer2).UseRedBand = Convert.ToBoolean(useBandCell.EditedFormattedValue);
                            if ((RasterRenderer as IRasterRGBRenderer2).UseRedBand)
                            {
                                if (bandCell.Value != null)
                                {
                                    (RasterRenderer as IRasterRGBRenderer2).RedBandIndex = (_rasterLayer.Raster as IRasterBandCollection).get_BandIndex(bandCell.Value.ToString());
                                }
                                else
                                {
                                    (RasterRenderer as IRasterRGBRenderer2).RedBandIndex = 0;
                                    bandCell.Value = (_rasterLayer.Raster as IRasterBandCollection).Item(0).Bandname;
                                }
                            }
                            break;
                        case "绿色":
                            (RasterRenderer as IRasterRGBRenderer2).UseGreenBand = Convert.ToBoolean(useBandCell.EditedFormattedValue);
                            if ((RasterRenderer as IRasterRGBRenderer2).UseGreenBand)
                            {
                                if (bandCell.Value != null)
                                {
                                    (RasterRenderer as IRasterRGBRenderer2).GreenBandIndex = (_rasterLayer.Raster as IRasterBandCollection).get_BandIndex(bandCell.Value.ToString());
                                }
                                else
                                {
                                    (RasterRenderer as IRasterRGBRenderer2).GreenBandIndex = 0;
                                    bandCell.Value = (_rasterLayer.Raster as IRasterBandCollection).Item(0).Bandname;
                                }
                            }
                            break;
                        case "蓝色":
                            (RasterRenderer as IRasterRGBRenderer2).UseBlueBand = Convert.ToBoolean(useBandCell.EditedFormattedValue);
                            if ((RasterRenderer as IRasterRGBRenderer2).UseBlueBand)
                            {
                                if (bandCell.Value != null)
                                {
                                    (RasterRenderer as IRasterRGBRenderer2).BlueBandIndex = (_rasterLayer.Raster as IRasterBandCollection).get_BandIndex(bandCell.Value.ToString());
                                }
                                else
                                {
                                    (RasterRenderer as IRasterRGBRenderer2).BlueBandIndex = 0;
                                    bandCell.Value = (_rasterLayer.Raster as IRasterBandCollection).Item(0).Bandname;
                                }
                            }
                            break;
                        case "Alpha":
                            (RasterRenderer as IRasterRGBRenderer2).UseAlphaBand = Convert.ToBoolean(useBandCell.EditedFormattedValue);
                            if ((RasterRenderer as IRasterRGBRenderer2).UseAlphaBand)
                            {
                                if (bandCell.Value != null)
                                {
                                    (RasterRenderer as IRasterRGBRenderer2).AlphaBandIndex = (_rasterLayer.Raster as IRasterBandCollection).get_BandIndex(bandCell.Value.ToString());
                                }
                                else
                                {
                                    (RasterRenderer as IRasterRGBRenderer2).AlphaBandIndex = 0;
                                    bandCell.Value = (_rasterLayer.Raster as IRasterBandCollection).Item(0).Bandname;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    #endregion
                }


                //背景色、NoData颜色
                (RasterRenderer as IRasterStretch2).Background = cbRGBBackground.Checked;
                (RasterRenderer as IRasterStretch2).BackgroundColor = Helper.GetEsriColorByCMYKText(btnRGBBGColor.Tag.ToString());
                if (cbRGBBackground.Checked)
                {
                    double r, g, b;
                    double.TryParse(tbRGBColorR.Text, out r);
                    double.TryParse(tbRGBColorG.Text, out g);
                    double.TryParse(tbRGBColorB.Text, out b);

                    (RasterRenderer as IRasterStretch2).BackgroundValue = new double[] { r, g, b }; ;
                }
                (RasterRenderer as IRasterDisplayProps).NoDataColor = Helper.GetEsriColorByCMYKText(btnRGBNoDataColor.Tag.ToString());

                //拉伸类型
                KeyValuePair<esriRasterStretchTypesEnum, string> stretchTypeItem = (KeyValuePair<esriRasterStretchTypesEnum, string>)cmbRGBStretchType.SelectedItem;
                (RasterRenderer as IRasterStretch2).StretchType = stretchTypeItem.Key;

                (RasterRenderer as IRasterStretch).Invert = cbRGBInvert.Checked;//反向
                #endregion
            }

            //显示
            double contrastValue, brightnessValue, transparencyValue;
            double.TryParse(tbContrastRatio.Text, out contrastValue);
            double.TryParse(tbBrightness.Text, out brightnessValue);
            double.TryParse(tbTrans.Text, out transparencyValue);
            if (contrastValue < 0)
                contrastValue = 0;
            else if (contrastValue > 100)
                contrastValue = 100;
            if (brightnessValue < 0)
                brightnessValue = 0;
            else if (brightnessValue > 100)
                brightnessValue = 100;
            if (transparencyValue < 0)
                transparencyValue = 0;
            else if (transparencyValue > 100)
                transparencyValue = 100;

            (RasterRenderer as IRasterDisplayProps).ContrastValue = (int)contrastValue;
            (RasterRenderer as IRasterDisplayProps).BrightnessValue = (int)brightnessValue;
            (RasterRenderer as IRasterDisplayProps).TransparencyValue = (int)transparencyValue;

            //更新
            RasterRenderer.Update();
        }

    }
}

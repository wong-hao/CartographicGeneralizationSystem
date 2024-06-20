using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace SMGI.Common
{
    public partial class AdvancedLabelOfStretchForm : Form
    {
        public IColorRamp ColorRamp
        {
            get;
            protected set;
        }

        /// <summary>
        /// 降序
        /// </summary>
        public List<double> LabelValueList
        {
            get;
            protected set;
        }

        public List<string> LabelTextList
        {
            get;
            protected set;
        }


        private double _maxVal, _minVal;
        private List<KeyValuePair<double, string>> _labelInfo;
        private int _picSaleRatio;
        public AdvancedLabelOfStretchForm(IColorRamp clrRamp, double maxVal, double minVal, List<KeyValuePair<double, string>> labelInfo = null)
        {
            InitializeComponent();

            ColorRamp = clrRamp;
            _maxVal = maxVal;
            _minVal = minVal;
            _labelInfo = labelInfo;

            _picSaleRatio = 15;
        }

        private void AdvancedLabelOfStretchForm_Load(object sender, EventArgs e)
        {
            tbInterverNum.Controls.RemoveAt(0);

            int interNum = 2;
            if (ColorRamp is IMultiPartColorRamp)
            {
                interNum = (ColorRamp as IMultiPartColorRamp).NumberOfRamps;
            }
            tbInterverNum.Value = interNum;
            double interVal = (_maxVal - _minVal) / interNum;
            tbInterverSize.Text = interVal.ToString();

            if (_labelInfo != null && _labelInfo.Count == interNum + 1 && _labelInfo.First().Key == _maxVal && _labelInfo.Last().Key == _minVal)
            {
                LabelValueList = new List<double>();
                LabelTextList = new List<string>();
                foreach(var kv in _labelInfo)
                {
                    LabelValueList.Add(kv.Key);
                    LabelTextList.Add(kv.Value);
                }
                UpdateColorsDataView(ColorRamp, LabelValueList, LabelTextList);

                symbologyControl.StyleClass = ESRI.ArcGIS.Controls.esriSymbologyStyleClass.esriStyleClassColorRamps;
                UpdateColorRampPicture();
            }
            else
            {
                LabelValueList = new List<double>();//interNum+1
                double val = _maxVal;
                for (int i = 0; i < interNum; ++i)
                {
                    LabelValueList.Add(Math.Floor(10000.0 * val + 0.5) / 10000.0);//保留4位小数点

                    val -= interVal;
                }
                LabelValueList.Add(Math.Floor(10000.0 * _minVal + 0.5) / 10000.0);//保留4位小数点
                UpdateColorsDataView(ColorRamp, LabelValueList);

                symbologyControl.StyleClass = ESRI.ArcGIS.Controls.esriSymbologyStyleClass.esriStyleClassColorRamps;
                UpdateColorRampPicture(ColorRamp);
            }
        }

        private void rbInterval_CheckedChanged(object sender, EventArgs e)
        {
            if (rbIntervalNum.Checked)
            {
                tbInterverNum.Enabled = true;
                tbInterverSize.Enabled = false;
            }
            else
            {
                tbInterverNum.Enabled = false;
                tbInterverSize.Enabled = true;
            }
        }

        private void btnGen_Click(object sender, EventArgs e)
        {
            int interNum = (int)tbInterverNum.Value;
            double interVal = 0;
            if (rbIntervalNum.Checked)
            {
                interVal = (_maxVal - _minVal) / (double)interNum;
            }
            else
            {
                bool b = double.TryParse(tbInterverSize.Text, out interVal);
                if (!b || interVal <= 0 || Math.Ceiling((_maxVal - _minVal) / interVal) > 25)
                {
                    MessageBox.Show(string.Format("间隔大小太小，不支持大于25的多个标注！"));
                    return;
                }
            }

            LabelValueList = new List<double>();
            double val = _maxVal;
            for (int i = 0; i < interNum; ++i)
            {
                LabelValueList.Add(Math.Floor(10000.0 * val + 0.5) / 10000.0);//保留4位小数点

                val -= interVal;
            }
            LabelValueList.Add(Math.Floor(10000.0 * _minVal + 0.5) / 10000.0);//保留4位小数点
            UpdateColorsDataView(ColorRamp, LabelValueList);

            UpdateColorRampPicture();
        }

        private void labelDataGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            
            DataGridViewButtonCell btnCell = labelDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewButtonCell;
            if (btnCell != null)
            {
                var location = getSubControlLocationInForm(labelDataGridView);
                location.Offset(this.Location);//相对于_hWnd
                int frmLeftBorderWidth = (this.Width - this.ClientRectangle.Width) / 2;//窗体左侧边框宽度
                int frmTopBorderHeight = (this.Height - this.ClientRectangle.Height) - frmLeftBorderWidth;//窗体顶部边框高度
                location.X += frmLeftBorderWidth;
                location.Y += frmTopBorderHeight;

                tagRECT tagRect = new tagRECT();
                tagRect.left = location.X + e.X;
                tagRect.top = location.Y + e.Y;
                tagRect.right = location.X + e.X + btnCell.Size.Width;
                tagRect.bottom = location.Y + e.Y + btnCell.Size.Height;

                IColor color = Helper.GetEsriColorByCMYKText(btnCell.Tag.ToString());
                var colorPalette = new ColorPalette();
                if (colorPalette.TrackPopupMenu(ref tagRect, color, true, 0))
                {
                    btnCell.Style.BackColor = Helper.GetColorByEsriColor(colorPalette.Color);
                    btnCell.Style.ForeColor = btnCell.Style.BackColor;
                    btnCell.Tag = Helper.GetCMYKTextByEsriColor(colorPalette.Color);

                    labelDataGridView.ClearSelection();

                    //更新色带控件
                    UpdateColorRampPicture();
                }

            }
        }

        private void labelDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if ("ValueColumn" == labelDataGridView.Columns[e.ColumnIndex].Name)
            {
                bool bValid = true;
                double val = 0;
                if (e.RowIndex == 0 || e.RowIndex == labelDataGridView.Rows.Count - 1)
                {
                    bValid = false;
                }
                else if (!double.TryParse(labelDataGridView.Rows[e.RowIndex].Cells["ValueColumn"].Value.ToString(), out val))//编辑后的新值
                {
                    bValid = false;
                }
                else
                {
                    double maxVal = double.Parse(labelDataGridView.Rows[e.RowIndex - 1].Cells["ValueColumn"].Value.ToString());
                    double minVal = double.Parse(labelDataGridView.Rows[e.RowIndex + 1].Cells["ValueColumn"].Value.ToString());
                    if (val >= maxVal || val <= minVal)
                    {
                        bValid = false;
                    }
                }

                if (!bValid)
                {
                    labelDataGridView.Rows[e.RowIndex].Cells["ValueColumn"].Value = LabelValueList[LabelValueList.Count - 1 - e.RowIndex];//还原,值与行号反向

                    return;
                }

                if (LabelValueList[e.RowIndex] == val)
                    return;

                //更新值列表与色带控件
                LabelValueList[LabelValueList.Count - 1 - e.RowIndex] = val;
                UpdateColorRampPicture();
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            LabelValueList = new List<double>();
            LabelTextList = new List<string>();
            ColorRamp = new MultiPartColorRampClass();

            List<IColor> Colors = new List<IColor>();
            for (int i = 0; i < labelDataGridView.Rows.Count; ++i)
            {
                DataGridViewButtonCell btnCell = labelDataGridView.Rows[i].Cells["ColorColumn"] as DataGridViewButtonCell;
                DataGridViewTextBoxCell valueCell = labelDataGridView.Rows[i].Cells["ValueColumn"] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell textCell = labelDataGridView.Rows[i].Cells["LabelColumn"] as DataGridViewTextBoxCell;

                Colors.Add(Helper.GetEsriColorByCMYKText(btnCell.Tag.ToString()));
                if (i == 0)
                {
                    LabelValueList.Add(_maxVal);
                }
                else if (i == labelDataGridView.Rows.Count - 1)
                {
                    LabelValueList.Add(_minVal);
                }
                else
                {
                    LabelValueList.Add(double.Parse(valueCell.Value.ToString()));
                }
                LabelTextList.Add(textCell.Value.ToString());
            }

            for (int i = 1; i < Colors.Count; ++i)
            {
                IAlgorithmicColorRamp clrRamp = new AlgorithmicColorRampClass();
                clrRamp.Size = 255;
                clrRamp.FromColor = Colors[Colors.Count - i];
                clrRamp.ToColor = Colors[Colors.Count - i - 1];
                bool bSuccess;
                clrRamp.CreateRamp(out bSuccess);

                (ColorRamp as IMultiPartColorRamp).AddRamp(clrRamp);
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void UpdateColorRampPicture(IColorRamp clrRamp)
        {
            ISymbologyStyleClass symStyleClass = symbologyControl.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
            IStyleGalleryItem styleGalleryItem = new ServerStyleGalleryItemClass();
            styleGalleryItem.Item = clrRamp;
            stdole.IPictureDisp pic = symStyleClass.PreviewItem(styleGalleryItem, _picSaleRatio * ColorRampPicBox.Height, _picSaleRatio * ColorRampPicBox.Width);
            Image image = Image.FromHbitmap(new System.IntPtr(pic.Handle));

            Bitmap b = new Bitmap(image);
            b.RotateFlip(RotateFlipType.Rotate90FlipXY);
            ColorRampPicBox.Image = b;
            ColorRampPicBox.Refresh();
        }

        private void UpdateColorRampPicture()
        {
            List<double> labelValues = new List<double>();//降序
            List<IColor> Colors = new List<IColor>();
            for (int i = 0; i < labelDataGridView.Rows.Count; ++i)
            {
                DataGridViewButtonCell btnCell = labelDataGridView.Rows[i].Cells["ColorColumn"] as DataGridViewButtonCell;
                DataGridViewTextBoxCell valueCell = labelDataGridView.Rows[i].Cells["ValueColumn"] as DataGridViewTextBoxCell;

                Colors.Add(Helper.GetEsriColorByCMYKText(btnCell.Tag.ToString()));
                labelValues.Add(double.Parse(valueCell.Value.ToString()));
            }

            _picSaleRatio = Colors.Count < 6 ? 5 * Colors.Count : 25;

            var totalImage = new Bitmap(_picSaleRatio * ColorRampPicBox.Height, _picSaleRatio * ColorRampPicBox.Width);
            Graphics g = Graphics.FromImage(totalImage);
            g.Clear(SystemColors.AppWorkspace);

            double deltaValue = labelValues.First() - labelValues.Last();
            int imageTotalLen = _picSaleRatio * ColorRampPicBox.Height;
            int len = 0;
            for (int i = Colors.Count -1; i > 0 ; --i)
            {
                IAlgorithmicColorRamp clrRamp = new AlgorithmicColorRampClass();
                clrRamp.Size = 120;
                clrRamp.FromColor = Colors[i];
                clrRamp.ToColor = Colors[i - 1];
                bool bSuccess;
                clrRamp.CreateRamp(out bSuccess);

                ISymbologyStyleClass symStyleClass = symbologyControl.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
                IStyleGalleryItem styleGalleryItem = new ServerStyleGalleryItemClass();
                styleGalleryItem.Item = clrRamp;

                int imageLen = (int)Math.Ceiling(imageTotalLen * (labelValues[i-1] - labelValues[i]) / deltaValue);


                stdole.IPictureDisp pic = symStyleClass.PreviewItem(styleGalleryItem, imageLen, _picSaleRatio * ColorRampPicBox.Width);
                Image image = Image.FromHbitmap(new System.IntPtr(pic.Handle));
                
                g.DrawImage(image, new Point(len,0));

                len += image.Width;

                image.Dispose();
            }
            g.Dispose();

            totalImage.RotateFlip(RotateFlipType.Rotate90FlipXY);
            ColorRampPicBox.Image = totalImage;
            ColorRampPicBox.Refresh();
        }


        private void UpdateColorsDataView(IColorRamp clrRamp, List<double> valueList, List<string> labelList = null)
        {
            double maxVal = valueList.First();//默认值为降序排列
            double minVal = valueList.Last();
            int num = valueList.Count;
            IColor[] colorArray = new IColor[num];//降序
            if (clrRamp is IAlgorithmicColorRamp)
            {
                #region IAlgorithmicColorRamp
                colorArray[0] = (clrRamp as IAlgorithmicColorRamp).ToColor;
                colorArray[num - 1] = (clrRamp as IAlgorithmicColorRamp).FromColor;
                if (colorArray[0] is IRgbColor)
                {
                    int deltaR = ((colorArray[0] as IRgbColor).Red - (colorArray[num - 1] as IRgbColor).Red);
                    int deltaG = ((colorArray[0] as IRgbColor).Green - (colorArray[num - 1] as IRgbColor).Green);
                    int deltaB = ((colorArray[0] as IRgbColor).Blue - (colorArray[num - 1] as IRgbColor).Blue);

                    for (int i = 1; i < num - 1; ++i)
                    {
                        colorArray[i] = new RgbColorClass()
                        {
                            Red = (colorArray[num - 1] as IRgbColor).Red + (int)(deltaR * (valueList[i] - minVal) / (maxVal - minVal)),
                            Green = (colorArray[num - 1] as IRgbColor).Green + (int)(deltaG * (valueList[i] - minVal) / (maxVal - minVal)),
                            Blue = (colorArray[num - 1] as IRgbColor).Blue + (int)(deltaB * (valueList[i] - minVal) / (maxVal - minVal))
                        };
                    }
                }
                else if (colorArray[0] is IHsvColor)
                {
                    int deltaH = ((colorArray[0] as IHsvColor).Hue - (colorArray[num - 1] as IHsvColor).Hue);
                    int deltaS = ((colorArray[0] as IHsvColor).Saturation - (colorArray[num - 1] as IHsvColor).Saturation);
                    int deltaV = ((colorArray[0] as IHsvColor).Value - (colorArray[num - 1] as IHsvColor).Value);

                    for (int i = 1; i < num - 1; ++i)
                    {
                        colorArray[i] = new HsvColorClass()
                        {
                            Hue = (colorArray[num - 1] as IHsvColor).Hue + (int)(deltaH * (valueList[i] - minVal) / (maxVal - minVal)),
                            Saturation = (colorArray[num - 1] as IHsvColor).Saturation + (int)(deltaS * (valueList[i] - minVal) / (maxVal - minVal)),
                            Value = (colorArray[num - 1] as IHsvColor).Value + (int)(deltaV * (valueList[i] - minVal) / (maxVal - minVal))
                        };
                    }
                }
                else
                {
                    if (!(colorArray[0] is ICmykColor))
                    {
                        colorArray[0] = new CmykColorClass() { CMYK = colorArray[0].CMYK };
                        colorArray[num - 1] = new CmykColorClass() { CMYK = colorArray[num - 1].CMYK };
                    }

                    int deltaC = ((colorArray[0] as ICmykColor).Cyan - (colorArray[num - 1] as ICmykColor).Cyan);
                    int deltaM = ((colorArray[0] as ICmykColor).Magenta - (colorArray[num - 1] as ICmykColor).Magenta);
                    int deltaY = ((colorArray[0] as ICmykColor).Yellow - (colorArray[num - 1] as ICmykColor).Yellow);
                    int deltaK = ((colorArray[0] as ICmykColor).Black - (colorArray[num - 1] as ICmykColor).Black);

                    for (int i = 1; i < num - 1; ++i)
                    {
                        colorArray[i] = new CmykColorClass()
                        {
                            Cyan = (colorArray[num - 1] as ICmykColor).Cyan + (int)(deltaC * (valueList[i] - minVal) / (maxVal - minVal)),
                            Magenta = (colorArray[num - 1] as ICmykColor).Magenta + (int)(deltaM * (valueList[i] - minVal) / (maxVal - minVal)),
                            Yellow = (colorArray[num - 1] as ICmykColor).Yellow + (int)(deltaY * (valueList[i] - minVal) / (maxVal - minVal)),
                            Black = (colorArray[num - 1] as ICmykColor).Black + (int)(deltaK * (valueList[i] - minVal) / (maxVal - minVal))
                        };
                    }
                }
                #endregion

            }
            else//IMultiPartColorRamp
            {
                #region IMultiPartColorRamp
                IMultiPartColorRamp multiClrRamp = clrRamp as IMultiPartColorRamp;
                if (num == multiClrRamp.NumberOfRamps + 1)
                {
                    colorArray[0] = (multiClrRamp.get_Ramp(multiClrRamp.NumberOfRamps - 1) as IAlgorithmicColorRamp).ToColor;
                    colorArray[num - 1] = (multiClrRamp.get_Ramp(0) as IAlgorithmicColorRamp).FromColor;
                    for (int i = 1; i < num - 1; ++i)
                    {
                        colorArray[i] = (multiClrRamp.get_Ramp(num - 1 - i) as IAlgorithmicColorRamp).FromColor;//colorArray降序，MultiPartColorRamp升序
                    }
                    
                }
                else
                {
                    colorArray[0] = (multiClrRamp.get_Ramp(multiClrRamp.NumberOfRamps - 1) as IAlgorithmicColorRamp).ToColor;
                    colorArray[num - 1] = (multiClrRamp.get_Ramp(0) as IAlgorithmicColorRamp).FromColor;
                    for (int i = 1; i < num - 1; ++i)
                    {
                        double pos = (multiClrRamp.NumberOfRamps + 1) * (num - 1 - i) / (double)num;
                        int minPos = (int)Math.Floor(pos);
                        int maxPos = (int)Math.Ceiling(pos);


                        if (minPos > multiClrRamp.NumberOfRamps - 1)
                        {
                            colorArray[i] = (multiClrRamp.get_Ramp(multiClrRamp.NumberOfRamps - 1) as IAlgorithmicColorRamp).ToColor;
                            continue;
                        }

                        if (minPos == maxPos)
                        {
                            colorArray[i] = (multiClrRamp.get_Ramp(minPos) as IAlgorithmicColorRamp).FromColor;
                            continue;
                        }

                        IColor minColor = (multiClrRamp.get_Ramp(minPos) as IAlgorithmicColorRamp).FromColor;
                        IColor maxColor;
                        if (maxPos > multiClrRamp.NumberOfRamps - 1)
                        {
                            maxColor = (multiClrRamp.get_Ramp(multiClrRamp.NumberOfRamps - 1) as IAlgorithmicColorRamp).ToColor;
                        }
                        else
                        {
                            maxColor = (multiClrRamp.get_Ramp(maxPos) as IAlgorithmicColorRamp).FromColor;
                        }

                        //赋值colorArray[i]
                        if (minColor is IRgbColor && maxColor is IRgbColor)
                        {
                            colorArray[i] = new RgbColorClass()
                            {
                                Red = (int)((minColor as IRgbColor).Red * (1-(pos - minPos)) + (maxColor as IRgbColor).Red * (1-(maxPos - pos))),
                                Green = (int)((minColor as IRgbColor).Green * (1 - (pos - minPos)) + (maxColor as IRgbColor).Green * (1 - (maxPos - pos))),
                                Blue = (int)((minColor as IRgbColor).Blue * (1 - (pos - minPos)) + (maxColor as IRgbColor).Blue * (1 - (maxPos - pos)))
                            };

                        }
                        else if (minColor is IHsvColor && maxColor is IHsvColor)
                        {
                            colorArray[i] = new HsvColorClass()
                            {
                                Hue = (int)((minColor as IHsvColor).Hue * (1 - (pos - minPos)) + (maxColor as IHsvColor).Hue * (1 - (maxPos - pos))),
                                Saturation = (int)((minColor as IHsvColor).Saturation * (1 - (pos - minPos)) + (maxColor as IHsvColor).Saturation * (1 - (maxPos - pos))),
                                Value = (int)((minColor as IHsvColor).Value * (1 - (pos - minPos)) + (maxColor as IHsvColor).Value * (1 - (maxPos - pos)))
                            };
                        }
                        else
                        {
                            if (!(minColor is ICmykColor) || !(maxColor is ICmykColor))
                            {
                                minColor = new CmykColorClass() { CMYK = minColor.CMYK };
                                maxColor = new CmykColorClass() { CMYK = maxColor.CMYK };
                            }

                            colorArray[i] = new CmykColorClass()
                            {
                                Cyan = (int)((minColor as ICmykColor).Cyan * (1 - (pos - minPos)) + (maxColor as ICmykColor).Cyan * (1 - (maxPos - pos))),
                                Magenta = (int)((minColor as ICmykColor).Magenta * (1 - (pos - minPos)) + (maxColor as ICmykColor).Magenta * (1 - (maxPos - pos))),
                                Yellow = (int)((minColor as ICmykColor).Yellow * (1 - (pos - minPos)) + (maxColor as ICmykColor).Yellow * (1 - (maxPos - pos))),
                                Black = (int)((minColor as ICmykColor).Black * (1 - (pos - minPos)) + (maxColor as ICmykColor).Black * (1 - (maxPos - pos)))
                            };
                        }

                    }
                    


                }

                #endregion
            }

            labelDataGridView.Rows.Clear();
            for (int i = 0; i < num; ++i)
            {
                int rowIndex = labelDataGridView.Rows.Add();
                ((DataGridViewButtonCell)labelDataGridView.Rows[rowIndex].Cells["ColorColumn"]).Style.ForeColor = Helper.GetColorByEsriColor(colorArray[i]);
                ((DataGridViewButtonCell)labelDataGridView.Rows[rowIndex].Cells["ColorColumn"]).Style.BackColor = Helper.GetColorByEsriColor(colorArray[i]);
                ((DataGridViewButtonCell)labelDataGridView.Rows[rowIndex].Cells["ColorColumn"]).Tag = Helper.GetCMYKTextByEsriColor(colorArray[i]);
                labelDataGridView.Rows[rowIndex].Cells["ValueColumn"].Value = valueList[i].ToString();
                labelDataGridView.Rows[rowIndex].Cells["LabelColumn"].Value = valueList[i].ToString();
                if(labelList != null && labelList.Count == valueList.Count)
                    labelDataGridView.Rows[rowIndex].Cells["LabelColumn"].Value = labelList[i];
            }
            labelDataGridView.ClearSelection();
        }

        /// <summary>
        /// 获取子控件在窗体中的位置（屏幕坐标）
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private Point getSubControlLocationInForm(Control c)
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

      
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 统改某一分类注记的属性,LZ@20200728
    /// </summary>
    public class AnnotationClassifiedModifyCmd : SMGICommand
    {
        public AnnotationClassifiedModifyCmd()
        {
            m_caption = "分类注记统改";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && 
                    m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            AnnotationClassifiedModifyForm frm = new AnnotationClassifiedModifyForm(m_Application.MapControl.hWnd);
            frm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    AnnotationAttribute annoAttri = frm.OutAnnotationAttribute;

                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = frm.AnnoSelectSQL;
                    IFeatureCursor feCursor = frm.ObjAnnoLayer.Search(qf, false);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        wo.SetText(string.Format("正在更新注记要素【{0}】......", fe.OID));

                        IAnnotationFeature annoFe = fe as IAnnotationFeature;
                        ITextElement textElement = annoFe.Annotation as ITextElement;

                        //统改注记属性
                        if (annoAttri.FontName != "")
                        {
                            #region 字体
                            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
                            foreach (System.Drawing.FontFamily ff in fonts.Families)
                            {
                                if (ff.Name == annoAttri.FontName)
                                {
                                    (textElement as ISymbolCollectionElement).FontName = annoAttri.FontName;
                                }
                            }
                            #endregion
                        }

                        if (annoAttri.FontSize > 0)
                        {
                            #region 字体大小
                            (textElement as ISymbolCollectionElement).Size = annoAttri.FontSize * 2.8345;
                            #endregion
                        }

                        if (annoAttri.FontColor != null)
                        {
                            #region 字体颜色
                            (textElement as ISymbolCollectionElement).Color = annoAttri.FontColor;
                            #endregion
                        }

                        if (frm.EnableModifyStyle)
                        {
                            #region 加粗
                            (textElement as ISymbolCollectionElement).Bold = annoAttri.EnableBold;
                            #endregion
                        }

                        if (frm.EnableModifyAlign)
                        {
                            #region 对齐方式
                            (textElement as ISymbolCollectionElement).HorizontalAlignment = annoAttri.HorizontalAlignment;
                            (textElement as ISymbolCollectionElement).VerticalAlignment = annoAttri.VerticalAlignment;
                            #endregion
                        }

                        if (frm.EnableModifyBackGround)
                        {
                            #region 文本背景
                            if (annoAttri.EnableTextBackground)
                            {
                                ITextSymbol textSymbol = textElement.Symbol;
                                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;

                                IBalloonCallout balloonCallout = new BalloonCalloutClass();
                                balloonCallout.Style = annoAttri.BackgroundStyle;

                                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                                if (annoAttri.BackgroundColor == null)
                                {
                                    if (fillSymbol.Color != null)
                                    {
                                        IColor clr = new CmykColorClass(){NullColor = true};
                                        fillSymbol.Color = clr;
                                    }
                                }
                                else
                                {
                                    fillSymbol.Color = annoAttri.BackgroundColor;
                                }

                                ILineSymbol lineSymbol = new SimpleLineSymbol();
                                lineSymbol.Width = annoAttri.BackgroundBorderWidth;
                                lineSymbol.Color = annoAttri.BackgroundBorderColor;

                                fillSymbol.Outline = lineSymbol;
                                balloonCallout.Symbol = fillSymbol;


                                ITextMargins textMarigns = balloonCallout as ITextMargins;
                                textMarigns.PutMargins(annoAttri.BackgroundMarginLeft, annoAttri.BackgroundMarginUp, annoAttri.BackgroundMarginRight, annoAttri.BackgroundMarginDown);

                                (textSymbol as IFormattedTextSymbol).Background = balloonCallout as ITextBackground;
                                textElement.Symbol = textSymbol;
                            }
                            else
                            {
                                ITextSymbol textSymbol = textElement.Symbol;
                                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;

                                (textSymbol as IFormattedTextSymbol).Background = null;
                                textElement.Symbol = textSymbol;
                            }

                            #endregion
                        }

                        if (frm.EnableModifyMask)
                        {
                            #region 蒙板类型
                            ITextSymbol textSymbol = textElement.Symbol;
                            (textSymbol as IMask).MaskStyle = annoAttri.MaskStyle;

                            textElement.Symbol = textSymbol;
                            #endregion

                            #region 蒙板大小
                            textSymbol = textElement.Symbol;
                            (textSymbol as IMask).MaskSize = annoAttri.MaskSize;

                            textElement.Symbol = textSymbol;
                            #endregion

                            #region 蒙板颜色
                            textSymbol = textElement.Symbol;
                            if (annoAttri.MaskColor == null)
                            {
                                (textSymbol as IMask).MaskSymbol = null;
                            }
                            else
                            {
                                IFillSymbol fillSymbol = (textSymbol as IMask).MaskSymbol;
                                if (fillSymbol == null)
                                {
                                    fillSymbol = fillSymbol = new SimpleFillSymbol();
                                }
                                fillSymbol.Color = annoAttri.MaskColor;

                                (textSymbol as IMask).MaskSymbol = fillSymbol;
                            }
                            textElement.Symbol = textSymbol;
                            #endregion
                        }

                        annoFe.Annotation = textElement as IElement;
                        fe.Store();

                        Marshal.ReleaseComObject(fe);
                    }
                    Marshal.ReleaseComObject(feCursor);
                }

                m_Application.EngineEditor.StopOperation("分类注记统改");

                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, frm.ObjAnnoLayer, m_Application.ActiveView.Extent);
            }
            catch (Exception ex)
            {
                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(ex.Message);
            }
        }
    }

    /// <summary>
    /// 注记属性
    /// </summary>
    public class AnnotationAttribute
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text
        {
            set;
            get;
        }

        #region 字体信息
        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName
        {
            get;
            set;
        }
        /// <summary>
        /// 字体大小（默认2.0，单位：毫米）
        /// </summary>
        public double FontSize
        {
            set;
            get;
        }
        /// <summary>
        /// 字体颜色
        /// </summary>
        public IColor FontColor
        {
            get;
            set;
        }
        #endregion

        #region 基本属性
        /// <summary>
        /// 是否加粗（默认为false）
        /// </summary>
        public bool EnableBold
        {
            set;
            get;
        }
        /// <summary>
        /// 文本水平对齐
        /// </summary>
        public esriTextHorizontalAlignment HorizontalAlignment
        {
            set;
            get;
        }
        /// <summary>
        /// 文本垂直对齐
        /// </summary>
        public esriTextVerticalAlignment VerticalAlignment
        {
            set;
            get;
        }
        #endregion

        #region 文本背景
        /// <summary>
        /// 是否开启文本背景（气泡，默认为false）
        /// </summary>
        public bool EnableTextBackground
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡样式
        /// </summary>
        public esriBalloonCalloutStyle BackgroundStyle
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡背景色
        /// </summary>
        public IColor BackgroundColor
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡边界宽度（单位：毫米）
        /// </summary>
        public double BackgroundBorderWidth
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡边界颜色
        /// </summary>
        public IColor BackgroundBorderColor
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡上边框距（单位：毫米）
        /// </summary>
        public double BackgroundMarginUp
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡下边框距（单位：毫米）
        /// </summary>
        public double BackgroundMarginDown
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡左边框距（单位：毫米）
        /// </summary>
        public double BackgroundMarginLeft
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡右边框距（单位：毫米）
        /// </summary>
        public double BackgroundMarginRight
        {
            set;
            get;
        }
        #endregion

        #region 蒙板
        /// <summary>
        /// 蒙板，如晕圈等
        /// </summary>
        public esriMaskStyle MaskStyle
        {
            set;
            get;
        }
        /// <summary>
        /// 蒙板大小（单位：毫米）
        /// </summary>
        public double MaskSize
        {
            set;
            get;
        }

        /// <summary>
        /// 蒙板颜色
        /// </summary>
        public IColor MaskColor
        {
            set;
            get;
        }
        #endregion


        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public AnnotationAttribute Clone()
        {
            AnnotationAttribute attri = new AnnotationAttribute();

            //利用反射进行克隆
            System.Reflection.PropertyInfo[] PropertyInfos = this.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo PropertyInfo in PropertyInfos)
            {
                object v1 = PropertyInfo.GetValue(this, null);
                PropertyInfo.SetValue(attri, v1, null);
            }

            return attri;
        }

        public bool AttributeEquals(AnnotationAttribute attri)
        {
            if (this.FontName != attri.FontName)
                return false;

            if (Math.Abs(this.FontSize - attri.FontSize) > 0.01)//小数点后两位
                return false;

            if ((this.FontColor != null && attri.FontColor != null && this.FontColor.CMYK != attri.FontColor.CMYK) ||
                (this.FontColor == null && attri.FontColor != null) || (this.FontColor != null && attri.FontColor == null))
                return false;

            if (this.EnableBold != attri.EnableBold)
                return false;

            if (this.HorizontalAlignment != attri.HorizontalAlignment)
                return false;

            if (this.VerticalAlignment != attri.VerticalAlignment)
                return false;

            if (this.BackgroundStyle != attri.BackgroundStyle)
                return false;

            if ((this.BackgroundColor != null && attri.BackgroundColor != null && this.BackgroundColor.CMYK != attri.BackgroundColor.CMYK) ||
                (this.BackgroundColor == null && attri.BackgroundColor != null) || (this.BackgroundColor != null && attri.BackgroundColor == null))
                return false;

            if (this.BackgroundBorderWidth != attri.BackgroundBorderWidth)
                return false;

            if ((this.BackgroundBorderColor != null && attri.BackgroundBorderColor != null && this.BackgroundBorderColor.CMYK != attri.BackgroundBorderColor.CMYK) ||
                (this.BackgroundBorderColor == null && attri.BackgroundBorderColor != null) || (this.BackgroundBorderColor != null && attri.BackgroundBorderColor == null))
                return false;

            if (Math.Abs(this.BackgroundMarginLeft - attri.BackgroundMarginLeft) > 0.01)//小数点后两位
                return false;

            if (Math.Abs(this.BackgroundMarginUp - attri.BackgroundMarginUp) > 0.01)//小数点后两位
                return false;

            if (Math.Abs(this.BackgroundMarginRight - attri.BackgroundMarginRight) > 0.01)//小数点后两位
                return false;

            if (Math.Abs(this.BackgroundMarginDown - attri.BackgroundMarginDown) > 0.01)//小数点后两位
                return false;

            if (this.MaskStyle != attri.MaskStyle)
                return false;

            if (Math.Abs(this.MaskSize - attri.MaskSize) > 0.01)//小数点后两位
                return false;

            if ((this.MaskColor != null && attri.MaskColor != null && this.MaskColor.CMYK != attri.MaskColor.CMYK) ||
                (this.MaskColor == null && attri.MaskColor != null) || (this.MaskColor != null && attri.MaskColor == null))
                return false;

            return true;
        }
    }
}

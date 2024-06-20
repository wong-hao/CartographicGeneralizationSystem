using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.Windows;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.EmergencyMap
{
    public class AnnotationChangeCmd: SMGI.Common.SMGICommand
    {
        public AnnotationChangeCmd()
        {
            m_caption = "注记修改";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        private IColor GetColorByString(string cmyk)
        {
            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;
            try
            {
                for (int i = 0; i <= cmyk.Length; i++)
                {
                    if (i == cmyk.Length)
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('C'))
                        {
                            CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('M'))
                        {
                            CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('Y'))
                        {
                            CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('K'))
                        {
                            CMYK_Color.Black = int.Parse(sbs.Substring(1));
                        }
                        break;
                    }
                    else
                    {
                        char C = cmyk[i];
                        if (D.Contains(C))
                        {
                            sb.Append(C);
                        }
                        else
                        {
                            string sbs = sb.ToString();
                            if (sbs.Contains('C'))
                            {
                                CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('M'))
                            {
                                CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('Y'))
                            {
                                CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('K'))
                            {
                                CMYK_Color.Black = int.Parse(sbs.Substring(1));
                            }
                            sb.Clear();
                            sb.Append(C);
                        }
                    }
                }
                return CMYK_Color;
            }
            catch
            {
                return null;
            }

        }
        
        public override void OnClick()
        {   
            //获取所以可以选择显示的要素图层
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IFDOGraphicsLayer; })).ToArray();
            Dictionary<string,IFeatureLayer> listNames = new Dictionary<string,IFeatureLayer>();
            for (int i = 0; i < lyrs.Length; i++)
            {
                listNames.Add(lyrs[i].Name,lyrs[i] as IFeatureLayer);
            }
             FrmAnnoAttribute FrmAttri = new FrmAnnoAttribute(m_Application.MapControl.hWnd);
             if (FrmAttri.ShowDialog() == System.Windows.Forms.DialogResult.OK)
             {
               
                
                 m_Application.EngineEditor.StartOperation();
                 try
                 {
                     IQueryFilter qf = new QueryFilterClass();
                     qf.WhereClause = FrmAttri.AnnoSelectSQL;
                     var fc = listNames[FrmAttri.OutAnnoStyle.LyrName].FeatureClass;
                     IFeatureCursor feCursor = fc.Search(qf, false);
                     IFeature fe = null;
                     while ((fe = feCursor.NextFeature()) != null)
                     {
                         UpdateAnnoFeature(fe, FrmAttri.InAnnoStyle, FrmAttri.OutAnnoStyle);
                     }
                     Marshal.ReleaseComObject(feCursor);

                     m_Application.EngineEditor.StopOperation("注记修改");

                     m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, listNames[FrmAttri.OutAnnoStyle.LyrName], m_Application.ActiveView.Extent);
                 }
                 catch (Exception ex)
                 {
                     m_Application.EngineEditor.AbortOperation();

                     MessageBox.Show(ex.Message);
                 }
             }

        }


        private void UpdateAnnoFeature(IFeature fe, AnnoStyle oldStyle, AnnoStyle newStyle)
        {
            IAnnotationFeature2 annoFeature = fe as IAnnotationFeature2;
            ITextElement textElement = annoFeature.Annotation as ITextElement;

            if (newStyle.Text.Trim() != "" && oldStyle.Text != newStyle.Text)
            {
                #region 文本
                textElement.Text = newStyle.Text;
                #endregion
            }

            if (newStyle.FontName != "" && oldStyle.FontName != newStyle.FontName)
            {
                #region 字体
                System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
                foreach (System.Drawing.FontFamily ff in fonts.Families)
                {
                    if (ff.Name == newStyle.FontName)
                    {
                        (textElement as ISymbolCollectionElement).FontName = newStyle.FontName;
                    }
                }
                #endregion
            }

            if (newStyle.FontSize > 0 && oldStyle.FontSize != newStyle.FontSize)
            {
                #region 字体大小
                (textElement as ISymbolCollectionElement).Size = newStyle.FontSize * 2.8345;
                #endregion
            }

            if (oldStyle.FontColorCMYK != newStyle.FontColorCMYK)
            {
                #region 字体颜色
                (textElement as ISymbolCollectionElement).Color = GetColorByString(newStyle.FontColorCMYK);
                #endregion
            }

            if (oldStyle.FontBold != newStyle.FontBold)
            {
                #region 加粗
                (textElement as ISymbolCollectionElement).Bold = newStyle.FontBold;
                #endregion
            }

            if (oldStyle.Horizontal != newStyle.Horizontal || oldStyle.Vertical != newStyle.Vertical)
            {
                #region 对齐方式
                (textElement as ISymbolCollectionElement).HorizontalAlignment = newStyle.Horizontal;
                (textElement as ISymbolCollectionElement).VerticalAlignment = newStyle.Vertical;
                #endregion
            }

            #region 注记背景
            if (newStyle.HasMask)
            {
                ITextSymbol textSymbol = textElement.Symbol;
                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;

                IBalloonCallout balloonCallout = new BalloonCalloutClass();
                balloonCallout.Style = newStyle.BalloonCallout;

                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = GetColorByString(newStyle.MaskColorCMYK);

                ILineSymbol lineSymbol = new SimpleLineSymbol();
                lineSymbol.Width = newStyle.LineWidth;
                lineSymbol.Color = GetColorByString(newStyle.LineColorCMYK);

                fillSymbol.Outline = lineSymbol;
                balloonCallout.Symbol = fillSymbol;


                ITextMargins textMarigns = balloonCallout as ITextMargins;
                textMarigns.PutMargins(newStyle.TextMarginsLeft, newStyle.TextMarginsUp, newStyle.TextMarginsRight, newStyle.TextMarginsDown);

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

            if (oldStyle.TextMaskStyle != newStyle.TextMaskStyle)
            {
                #region 蒙板类型
                ITextSymbol textSymbol = textElement.Symbol;
                (textSymbol as IMask).MaskStyle = newStyle.TextMaskStyle;

                textElement.Symbol = textSymbol;
                #endregion
            }

            if (oldStyle.TextMaskSize != newStyle.TextMaskSize)
            {
                #region 蒙板大小
                ITextSymbol textSymbol = textElement.Symbol;
                (textSymbol as IMask).MaskSize = newStyle.TextMaskSize;

                textElement.Symbol = textSymbol;
                #endregion
            }

            if (newStyle.HasMask && (oldStyle.TextMaskColor != newStyle.TextMaskColor))
            {
                #region 蒙板颜色
                ITextSymbol textSymbol = textElement.Symbol;
                IFillSymbol fillSymbol = (textSymbol as IMask).MaskSymbol;
                fillSymbol.Color = GetColorByString(newStyle.TextMaskColor);

                (textSymbol as IMask).MaskSymbol = fillSymbol;
                textElement.Symbol = textSymbol;
                #endregion
            }

            annoFeature.Annotation = textElement as IElement;
            fe.Store();
        }
    }
}

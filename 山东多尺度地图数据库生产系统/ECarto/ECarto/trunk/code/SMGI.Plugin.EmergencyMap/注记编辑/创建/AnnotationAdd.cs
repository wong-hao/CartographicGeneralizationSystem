using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using stdole;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;
using System.Data;

namespace SMGI.Plugin.EmergencyMap
{
    public class AnnotationAdd : SMGI.Common.SMGITool
    {
        public AnnotationAdd()
        {
            m_caption = "注记添加";
            m_toolTip = "在鼠标点击位置生成注记";
            m_category = "注记编辑";
           // pMapControl = m_Application.MapControl;
        }
        private DataRow AnnoRuleRow = null;//选择的注记规则
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1 && this.Enabled)
            {
                IPoint pPoint = ToSnapedMapPoint(x, y);
                //获取所以可以选择显示的要素图层
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                { return l is IFDOGraphicsLayer; })).ToArray();
                List<string> listNames = new List<string>();
                for (int i = 0; i < lyrs.Length; i++) {
                    listNames.Add(lyrs[i].Name);
                }

                AnnoAttribute FrmAttri = new AnnoAttribute(m_Application.MapControl.hWnd, listNames.ToArray(), pPoint, AnnoRuleRow);
                if (FrmAttri.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.AnnoRuleRow = FrmAttri.AnnoRuleRow;
                    m_Application.EngineEditor.EnableUndoRedo(true);
                    //修改字体符号样式
                    m_Application.EngineEditor.StartOperation();
                    string message = string.Empty;
                    AnnoStyle style = FrmAttri.OutAnnoStyle;
                    ITextElement textElement = AnnoFunc.CreateTextElement(pPoint,
                        style.Text,
                        style.FontName,
                        style.CharacterWidth,
                        style.Itatic,
                        style.FontSize,
                        GetColorByString(style.FontColorCMYK),
                        out message);
                    if (textElement == null) {
                        return;
                    }
                    ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)textElement;
                    pSymbolCollEle.Bold = style.FontBold;
                    IElement pElement = textElement as IElement;
                    //
                    IFeatureClass pFeatureClass=null;
                    for(int i=0;i<lyrs.Length;i++){
                        if (lyrs[i].Name == style.LyrName)
                        {
                            pFeatureClass=(lyrs[i] as IFeatureLayer).FeatureClass;
                            break;
                        }
                    }
                    if (pFeatureClass != null)
                    {
                       
                        //添加背景边框，如果有的话
                        IElement newElement = pElement as IElement;

                        //蒙版
                        ITextSymbol textSymbol = textElement.Symbol;
                        IMask textMask = textSymbol as IMask;
                        textMask.MaskSize = style.TextMaskSize;
                        textMask.MaskStyle = style.TextMaskStyle;
                        ILineSymbol maskLineSymbol = new SimpleLineSymbol();
                        maskLineSymbol.Width = 0;
                        IFillSymbol maskSymbol = new SimpleFillSymbolClass();
                        maskSymbol.Color = GetColorByString(style.TextMaskColor);
                        maskSymbol.Outline = maskLineSymbol;
                        textMask.MaskSymbol = maskSymbol;
                        if (style.HasMask)//气泡背景
                        {
                            //设置文本背景框样式
                            IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;
                            IBalloonCallout balloonCallout = new BalloonCalloutClass();
                            balloonCallout.Style = style.BalloonCallout;
                            //设置文本背景框颜色
                            IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                            fillSymbol.Color = GetColorByString(style.MaskColorCMYK);
                            ILineSymbol lineSymbol = new SimpleLineSymbol();
                            lineSymbol.Width = style.LineWidth;
                            lineSymbol.Color = GetColorByString(style.LineColorCMYK);
                            fillSymbol.Outline = lineSymbol;
                            balloonCallout.Symbol = fillSymbol;
                            //设置背景框边距
                            ITextMargins textMarigns = balloonCallout as ITextMargins;
                            textMarigns.PutMargins(style.TextMarginsLeft, style.TextMarginsUp, style.TextMarginsRight, style.TextMarginsDown);
                            formattedTextSymbol.Background = balloonCallout as ITextBackground;
                        }
                        textSymbol.Angle = style.Angle * 180 / Math.PI;//角度
                        textElement.Symbol = textSymbol;
                      
                        IFeature pNewFeature = pFeatureClass.CreateFeature();
                        IAnnotationFeature pNewAnnoFeature = pNewFeature as IAnnotationFeature;
                        pNewAnnoFeature.Annotation = pElement;
                        pNewFeature.Store();
                        m_Application.EngineEditor.StopOperation("注记符号添加");
                        m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, pNewFeature.Shape.Envelope);
                    }
                }
            }
        }
        /// <summary>
        /// 根据注记规则里的CMYK字符串得到CMYK颜色值
        /// </summary>
        /// <param name="cmyk">cmyk字符串（形如：C100M200Y100K50）</param>
        /// <returns>CMYK颜色值</returns>
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

        
    }
}

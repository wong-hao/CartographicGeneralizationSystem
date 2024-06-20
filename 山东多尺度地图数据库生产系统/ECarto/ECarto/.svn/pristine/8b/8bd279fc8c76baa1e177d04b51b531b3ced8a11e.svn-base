using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using stdole;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Maplex;
using System.Data;
using SMGI.Common;
using System.Collections;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 创建的时候直接按字符进行创建（尤其是对于线性要素，（等高线不能这样处理）无需经过打散操作），
    /// </summary>
    public class AnnoFunc
    {
        /// <summary>
        /// 字体映射表
        /// </summary>
        private static DataTable MapFontTable;
        /// <summary>
        /// 获取映射字体
        /// </summary>
        /// <returns></returns>
        public static void GetMapFonts(DataTable mapFontTable) {
            for (int i = 0; i < mapFontTable.Rows.Count; i++)
            {
                DataRow dr=mapFontTable.Rows[i];
                if (dr["是否斜体"].ToString() == "是")
                    dr["是否斜体"] = true;
                else
                    dr["是否斜体"] = false;
            }
            MapFontTable = mapFontTable;
        }
        
        /// <summary>
        /// 传统注记创建中用到的方式，创建字体元素  
        /// </summary>
        /// <param name="position">元素的位置,可以为Point，可以为Polyline</param>
        /// <param name="annoText">注记的文本</param>
        /// <param name="fontFamily">字体</param>
        /// <param name="fontSize">字大</param>
        /// <param name="cmykColor">颜色</param>
        /// <returns>文本元素</returns>
        public static ITextElement CreateTextElement(IGeometry position, string annoText, string fontFamily, double fontSize, IColor cmykColor, out string message)
        {
            message = string.Empty;
            double CharacterWidth = -1;
            bool Italic=false;
            //----------------------字体判断------------------------------
            #region 字体映射
            for (int i = 0; i < MapFontTable.Rows.Count; i++)
            {
                DataRow row=MapFontTable.Rows[i];
                if (row["国标字体"].ToString() == fontFamily)
                {
                    fontFamily =row["替换字体"].ToString();
                    CharacterWidth = Convert.ToInt16(row["字体宽度"]);
                    Italic = Convert.ToBoolean(row["是否斜体"]);
                }
            }
            #endregion
            bool fontExist = false;
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily ff in fonts.Families)
            {
                if (ff.Name == fontFamily)
                {
                    fontExist = true;
                }
            }
            if (fontExist)
            {
                //创建文本元素
                ITextElement pTextElment = new TextElementClass();
                pTextElment.ScaleText = true;
                #region
                //IFontDisp pFont = new StdFont() as IFontDisp;
                //pFont.Name = fontFamily;
                //if (Italic)
                //{
                //    pFont.Italic = Italic;
                //}
                //ITextSymbol pTextSymbol = new TextSymbolClass();
                //pTextSymbol.Color = cmykColor;
                //pTextSymbol.Font = pFont;
                //if (annoText.Contains("KV"))
                //{
                //    annoText = annoText.Substring(0, annoText.Length - 2);
                //}
                //pTextSymbol.Size = fontSize * 2.8345;
                //pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                //pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                //pTextElment.Symbol = pTextSymbol;
                //pTextElment.Text = annoText;
                #endregion
                ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElment;
                #region
                pSymbolCollEle.Color = cmykColor;
                pSymbolCollEle.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                pSymbolCollEle.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                pSymbolCollEle.Size = fontSize * 2.8345;
                if (annoText.Contains("KV"))
                {
                    int aIndex = annoText.IndexOf("KV");
                    aIndex += 2;
                    if(aIndex==annoText.Length)
                       annoText = annoText.Substring(0, annoText.Length - 2);
                }
                pSymbolCollEle.Text = annoText;
                pSymbolCollEle.FontName = fontFamily;
                #endregion
                pSymbolCollEle.Bold = false;
                pSymbolCollEle.Underline = false;
                //字宽设置
                if (CharacterWidth != -1)
                {
                    pSymbolCollEle.CharacterWidth = CharacterWidth;
                }
                ////斜体设置
                //if (Italic)
                //{
                //    pSymbolCollEle.Italic = Italic;
                //}
                //=======
                IElement pEle = pTextElment as IElement;
                pEle.Geometry = position;
                return pTextElment;
            }
            else
            {
               message="系统缺失[" + fontFamily + "]字体库";
               return null;
            }
        }

        /// <summary>
        /// 传统注记创建中用到的方式，创建字体元素  
        /// </summary>
        /// <param name="position">元素的位置,可以为Point，可以为Polyline</param>
        /// <param name="annoText">注记的文本</param>
        /// <param name="fontFamily">字体</param>
        /// <param name="fontSize">字大</param>
        /// <param name="cmykColor">颜色</param>
        /// <returns>文本元素</returns>
        public static ITextElement CreateTextElement(IGeometry position, string annoText, string fontFamily, double characterWidth, bool itatic,
            double fontSize, IColor cmykColor, out string message,bool fontBold = false,
            esriTextHorizontalAlignment textHorizontal = esriTextHorizontalAlignment.esriTHACenter,esriTextVerticalAlignment textVertical = esriTextVerticalAlignment.esriTVACenter)
        {
            message = string.Empty;
            //----------------------字体判断------------------------------
            bool fontExist = false;
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily ff in fonts.Families)
            {
                if (ff.Name == fontFamily)
                {
                    fontExist = true;
                }
            }
            if (!fontExist)//所选字体不存在的话，替换为宋体
            {
                fontFamily = "宋体";
                fontExist = true;
            }
            if (fontExist)
            {
                try
                {
                    //创建文本元素
                    ITextElement pTextElment = new TextElementClass();
                    pTextElment.ScaleText = true;
                    #region
                    //IFontDisp pFont = new StdFont() as IFontDisp;
                    //pFont.Name = fontFamily;
                    //if (Italic)
                    //{
                    //    pFont.Italic = Italic;
                    //}
                    //ITextSymbol pTextSymbol = new TextSymbolClass();
                    //pTextSymbol.Color = cmykColor;
                    //pTextSymbol.Font = pFont;
                    //if (annoText.Contains("KV"))
                    //{
                    //    annoText = annoText.Substring(0, annoText.Length - 2);
                    //}
                    //pTextSymbol.Size = fontSize * 2.8345;
                    //pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                    //pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                    //pTextElment.Symbol = pTextSymbol;
                    //pTextElment.Text = annoText;
                    #endregion
                    ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElment;
                    #region
                    pSymbolCollEle.Color = cmykColor;
                    pSymbolCollEle.VerticalAlignment = textVertical;
                    pSymbolCollEle.HorizontalAlignment = textHorizontal;
                    pSymbolCollEle.Size = fontSize * 2.8345;
                    if (annoText.Contains("KV"))
                    {
                        int aIndex = annoText.IndexOf("KV");
                        aIndex += 2;
                        if (aIndex == annoText.Length)
                            annoText = annoText.Substring(0, annoText.Length - 2);
                    }
                    pSymbolCollEle.Text = annoText;
                    pSymbolCollEle.FontName = fontFamily;
                    #endregion
                    pSymbolCollEle.Bold = fontBold;
                    pSymbolCollEle.Underline = false;
                    //字宽设置
                    pSymbolCollEle.CharacterWidth = characterWidth;
                    pSymbolCollEle.Italic = itatic;
                    //=======
                    IElement pEle = pTextElment as IElement;
                    pEle.Geometry = position;
                    return pTextElment;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    return null;
                }
            }
            else
            {
                message = "系统缺失[" + fontFamily + "]字体库";
                return null;
            }
        }

    }

    public class Lyr_AnnoStyle
    {
        public IFeatureLayer featureLayer;
        public AnnoStyle annoStyle;
    }
}

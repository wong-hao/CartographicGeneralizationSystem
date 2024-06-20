using System;
using System.Collections.Generic;
using System.Drawing;
using ESRI.ArcGIS.Controls;
using System.Linq;
using System.Text;
using System.Collections;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using System.Runtime.Serialization;



namespace SMGI.Common
{
    [Serializable]
    public class Decoration 
    {
        [Serializable]
        public struct PagePara
        {
           public double PageWidth;
           public double PageHeight;
        }
        public string DecrationName { get; set; }
        public PagePara Pageparas { get; set; }
        public string  FrameElementStr { get; set;}
        public string WTKElementStr { get; set; }
        public List<string> LaceElementList { get; set; }
        public List<string> CutLineElementList { get; set; }
        public List<string> DZXElementList { get; set; }
        public List<string> CMYKElementList { get; set; }
        public string LegendElementStr { get; set; }

        public PagePara GetPagePara(IPageLayoutControl pPagelayoutControl)
        {
            PagePara mPagepara = new PagePara();
            IPage pPage = pPagelayoutControl.Page;
            mPagepara.PageWidth = pPage.PrintableBounds.Width;
            mPagepara.PageHeight = pPage.PrintableBounds.Height;
            return mPagepara;
        }
        public string  GetFrameElement(GApplication pGapp, IPageLayoutControl pPagelayoutControl)
        {
            string Objstr;
            IElement pEle = null;
            IFrameElement pFrameEle = pPagelayoutControl.ActiveView.GraphicsContainer.FindFrame(pGapp.Workspace.Map);
            pEle = (IElement)pFrameEle;
            Objstr = GConvert.ObjectToBase64(pEle);
            return Objstr;
        
        }

        public string GetSpecificElement(string KeyWord, IActiveView pActiveView)
        {
            string Objstr;
            IElement pEle = null;
            IGraphicsContainer pGrContainer = pActiveView.GraphicsContainer;
            pGrContainer.Reset();
            IElement FindEle = pGrContainer.Next();
            while (FindEle != null)
            {
                IElementProperties pElePro =(IElementProperties) FindEle;
                if (pElePro.Name.ToUpper().Contains(KeyWord.ToUpper()))
                {
                    pEle = FindEle;
                    break;
                }
                FindEle = pGrContainer.Next();
            }
            if (pEle == null)
            {
                Objstr = "";
            }
            else
            {
                Objstr = GConvert.ObjectToBase64(pEle);
            }
            return Objstr;
        }
        public List<string> GetSpecificElements(string KeyWord, IActiveView pActiveView)
        {
            List<string> EleList = new List<string>();
            string Objstr;
            IGraphicsContainer pGrContainer = pActiveView.GraphicsContainer;
            pGrContainer.Reset();
            IElement FindEle = pGrContainer.Next();
            while (FindEle != null)
            {
                IElementProperties pElePro = (IElementProperties)FindEle;
                if (pElePro.Name.ToUpper().Contains(KeyWord.ToUpper()))
                {
                    Objstr = GConvert.ObjectToBase64(FindEle);
                    EleList.Add(Objstr); 
                }
                FindEle = pGrContainer.Next();
            }

            return EleList;
        }
        public void DeleteSpecificElement(string KeyWord, IActiveView pActiveView)
        {
            IArray ElementArray = new ArrayClass();
            IGraphicsContainer pGraphicsContainer = pActiveView.GraphicsContainer;
            pGraphicsContainer.Reset();
            IElement DeleteEle = pGraphicsContainer.Next();
            while (DeleteEle != null)
            {
                IElementProperties3 EleProp = (IElementProperties3)DeleteEle;
                if (EleProp.Name.ToUpper().Contains(KeyWord.ToUpper()))
                {
                    ElementArray.Add(DeleteEle);
                }
                DeleteEle = pGraphicsContainer.Next();
            }
            if (ElementArray.Count != 0)
            {
                for (int i = 0; i <= ElementArray.Count - 1; i++)
                {
                    pGraphicsContainer.DeleteElement(ElementArray.get_Element(i) as IElement);
                }
            }
        }
        public string GetFrame(GApplication pApplication)
        {
            string Objstr;
            IElement pEle = null;
            IMap pMap = pApplication.Workspace.Map;
            pEle = pApplication.PageLayoutControl.ActiveView.GraphicsContainer.FindFrame(pMap) as IElement;
            Objstr = GConvert.ObjectToBase64(pEle);
            return Objstr;
        }
        public double MillimeterToPoint(double millimeter)
        {
            double Width = (double)millimeter * 2.54;
            Width = Math.Round(Width, 1);
            return Width;
        }
        public double PointToMillimeter(double point)
        {
            double width = Math.Round( point / 2.54,1);
            return width;
        
        }
        public IColor ConvertColorToIColor(Color p_Color)
        {
            IColor pColor = new RgbColorClass { RGB = p_Color.B * 65536 + p_Color.G * 256 + p_Color.R };
            return pColor;
        }
       



    }
}

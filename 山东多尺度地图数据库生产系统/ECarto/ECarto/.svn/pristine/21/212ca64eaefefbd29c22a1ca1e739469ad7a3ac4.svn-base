using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.AnnotationEngine
{
    /// <summary>
    /// 将注记沿线散列式放置
    /// </summary>
    public class AnnotationAlongPolylineTool : SMGI.Common.SMGITool
    {
        public AnnotationAlongPolylineTool()
        {
            m_caption = "注记沿线放置";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && 
                    m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override bool Deactivate()
        {
            m_Application.MapControl.Map.ClearSelection();
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            
            return true;
        }


        public override void OnClick()
        {
            
        }


        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1) 
                return;

            IAnnotationFeature selAnnoFeature = null;
            #region 获取选中的注记要素
            IEnumFeature enumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            IFeature feature = null;
            while ((feature = enumFeature.Next()) != null)
            {
                IAnnotationFeature annoFeature = feature as IAnnotationFeature;
                if (annoFeature == null || annoFeature.Annotation == null)
                    continue;

                IElement element = annoFeature.Annotation;
                if (element is AnnotationElement)
                {
                    if (selAnnoFeature == null)
                    {
                        selAnnoFeature = annoFeature;
                    }
                    else
                    {
                        selAnnoFeature = null;
                    }
                }
            }
            Marshal.ReleaseComObject(enumFeature);
            #endregion
            if (selAnnoFeature == null)
            {
                MessageBox.Show("请选择且只能选择一个注记要素！");
                return;
            }

            IPolyline trackPolyline = m_Application.MapControl.TrackLine() as IPolyline;
            if (trackPolyline == null || trackPolyline.IsEmpty) 
                return;

            IElement annoElement = selAnnoFeature.Annotation;
            string text = (annoElement as ITextElement).Text;
            if (annoElement is IMultiPartTextElement && (annoElement as IMultiPartTextElement).IsMultipart)
            {
                //获取多部件注记的市级文本
                text = AnnotationHelper.DelSpaceAfterChinese(text);
            }
            text = text.Replace("\r\n", "");
            text = text.Replace(" ", "");
            if (text.Length == 1) 
                return;

            m_Application.EngineEditor.EnableUndoRedo(true);
            m_Application.EngineEditor.StartOperation();
            try
            {
                #region 新建element，复制属性
                var newTextElment = new TextElementClass();

                newTextElment.ScaleText = (annoElement as ITextElement).ScaleText;
                newTextElment.Text = (annoElement as ITextElement).Text;

                if ((annoElement as ISymbolCollectionElement).Background != null)
                    (newTextElment as ISymbolCollectionElement).Background = ((annoElement as ISymbolCollectionElement).Background as IClone).Clone() as ITextBackground;

                (newTextElment as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;//沿线摆放后居中对齐，便于精确定位操控
                (newTextElment as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;//沿线摆放后居中对齐，便于精确定位操控
                (newTextElment as ISymbolCollectionElement).CharacterSpacing = 0;//不要字符间距
                (newTextElment as ISymbolCollectionElement).Bold = (annoElement as ISymbolCollectionElement).Bold;
                (newTextElment as ISymbolCollectionElement).CharacterWidth = (annoElement as ISymbolCollectionElement).CharacterWidth;
                if ((annoElement as ISymbolCollectionElement).Color != null)
                    (newTextElment as ISymbolCollectionElement).Color = ((annoElement as ISymbolCollectionElement).Color as IClone).Clone() as IColor;
                (newTextElment as ISymbolCollectionElement).FontName = (annoElement as ISymbolCollectionElement).FontName;
                if ((annoElement as ISymbolCollectionElement).Geometry != null)
                    (newTextElment as ISymbolCollectionElement).Geometry = ((annoElement as ISymbolCollectionElement).Geometry as IClone).Clone() as IGeometry;
                (newTextElment as ISymbolCollectionElement).Italic = (annoElement as ISymbolCollectionElement).Italic;
                (newTextElment as ISymbolCollectionElement).Leading = (annoElement as ISymbolCollectionElement).Leading;
                (newTextElment as ISymbolCollectionElement).Size = (annoElement as ISymbolCollectionElement).Size;
                (newTextElment as ISymbolCollectionElement).TextPath = (annoElement as ISymbolCollectionElement).TextPath;
                (newTextElment as ISymbolCollectionElement).Underline = (annoElement as ISymbolCollectionElement).Underline;
                (newTextElment as ISymbolCollectionElement).WordSpacing = (annoElement as ISymbolCollectionElement).WordSpacing;
                (newTextElment as ISymbolCollectionElement).XOffset = (annoElement as ISymbolCollectionElement).XOffset;
                (newTextElment as ISymbolCollectionElement).YOffset = (annoElement as ISymbolCollectionElement).YOffset;

                //ITextSymbol newTextSymbol = ((annoElement as ITextElement).Symbol as IClone).Clone() as ITextSymbol;
                //newTextElment.Symbol = newTextSymbol;

                annoElement = newTextElment;
                #endregion
                
                double sizeLen = 0.1;
                if (GApplication.Application.ActiveView.FocusMap.ReferenceScale > 0)
                {
                    double fontSize = (annoElement as ITextElement).Symbol.Size / 2.8345;//mm
                    sizeLen = fontSize * 1e-3 * GApplication.Application.ActiveView.FocusMap.ReferenceScale;
                }

                IMultiPartTextElement mutiPartElement = annoElement as IMultiPartTextElement;
                if (mutiPartElement != null)
                {
                    IAnnotationClassExtension2 annoExtension = (selAnnoFeature as IFeature).Class.Extension as IAnnotationClassExtension2;
                    if(!mutiPartElement.IsMultipart)
                        mutiPartElement.ConvertToMultiPart(annoExtension.get_Display(annoElement));
                    while (mutiPartElement.PartCount > 0)
                    {
                        mutiPartElement.DeletePart(0);
                    }
                    (mutiPartElement as ITextElement).Text = "";
                    (selAnnoFeature as IFeature).Shape = new PolygonClass();
                }

                IPointCollection pc = trackPolyline as IPointCollection;
                if (pc.PointCount == text.Length)//节点数与文本字符数一致
                {
                    for (int i = 0; i < text.Length; ++i)
                    {
                        string t = text[i].ToString();
                        IPoint pt = pc.get_Point(i);

                        //mutiPartElement.InsertPart(mutiPartElement.PartCount, t, pt);//旋转问题

                        IPolyline pl = new PolylineClass();
                        pl.FromPoint = new PointClass() { X = pt.X - sizeLen * 0.5, Y = pt.Y };
                        pl.ToPoint = new PointClass() { X = pt.X + sizeLen * 0.5, Y = pt.Y };
                        mutiPartElement.InsertPart(mutiPartElement.PartCount, t, pl);
                        
                    }
                }
                else//第一个字符放置在第一个点，最后一个字符放置在最后一个点，其它字符均匀排列在线上
                {
                    double splitLen = trackPolyline.Length / (text.Length - 1);
                    for (int i = 0; i < text.Length; ++i)
                    {
                        string t = text[i].ToString();
                        IPoint pt = new PointClass();
                        trackPolyline.QueryPoint(esriSegmentExtension.esriNoExtension, splitLen * i, false, pt);

                        //mutiPartElement.InsertPart(mutiPartElement.PartCount, t, pt);//旋转问题

                        IPolyline pl = new PolylineClass();
                        pl.FromPoint = new PointClass() { X = pt.X - sizeLen * 0.5, Y = pt.Y };
                        pl.ToPoint = new PointClass() { X = pt.X + sizeLen * 0.5, Y = pt.Y };
                        mutiPartElement.InsertPart(mutiPartElement.PartCount, t, pl);
                    }

                }

                (annoElement as ITextElement).Text = (annoElement as ITextElement).Text.Trim();
                selAnnoFeature.Annotation = annoElement;
                (selAnnoFeature as IFeature).Store();

                m_Application.EngineEditor.StopOperation("注记沿线摆放");

                //刷新地图
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
            }
            catch (Exception ex)
            {
                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(ex.Message);
            }

        }
    }
}

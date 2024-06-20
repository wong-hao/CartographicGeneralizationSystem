using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;
using SMGI.Common;
using System.Runtime.InteropServices;
using System.Globalization;

namespace SMGI.Plugin.AnnotationEngine
{
    public class AnnotationToMultipartCommand : SMGI.Common.SMGICommand
    {
        public AnnotationToMultipartCommand()
        {
            m_caption = "注记转多部件";
        }

        public override bool Enabled
       {
           get
           {
               return m_Application != null && m_Application.Workspace != null && 
                   m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing && 
                   (m_Application.MapControl.Map.FeatureSelection as IEnumFeature).Next() != null;
           }
       }

        public override void OnClick()
        {
            List<IFeature> selFeList = new List<IFeature>();
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
                    selFeList.Add(feature);
                }
            }
            Marshal.ReleaseComObject(enumFeature);
            #endregion
            if (selFeList.Count == 0)
                return;

            m_Application.EngineEditor.EnableUndoRedo(true);
            m_Application.EngineEditor.StartOperation();
            try
            {
                foreach (var fe in selFeList)
                {
                    IAnnotationFeature annoFeature = fe as IAnnotationFeature;
                    IElement annoElement = annoFeature.Annotation;

                    if (annoElement is IGroupElement)//元素组注记
                    {
                        //不处理
                    }
                    else
                    {
                        IMultiPartTextElement mutiPartElement = annoElement as IMultiPartTextElement;
                        if (mutiPartElement == null)
                            continue;

                        if (!mutiPartElement.IsMultipart)
                        {
                            string text = (annoElement as ITextElement).Text;
                            if (true)//直接在中文字符后面加空格
                            {
                                (annoElement as ITextElement).Text = AnnotationHelper.InsertSpaceInChinese(text);

                                IAnnotationClassExtension2 annoExtension = fe.Class.Extension as IAnnotationClassExtension2;
                                mutiPartElement.ConvertToMultiPart(annoExtension.get_Display(annoFeature.Annotation));
                            }
                            else//中文字符替换为特殊英文字符+空格，而后再进行字符替换
                            {
                                char replaceChar = '$';
                                List<char> charList = new List<char>();
                                (annoElement as ITextElement).Text = AnnotationHelper.ReplaceChineseChar(text, ref charList, replaceChar);

                                IAnnotationClassExtension2 annoExtension = fe.Class.Extension as IAnnotationClassExtension2;
                                mutiPartElement.ConvertToMultiPart(annoExtension.get_Display(annoFeature.Annotation));
                                
                                int index = 0;
                                for (int i = 0; i < mutiPartElement.PartCount; ++i)
                                {
                                    ITextElement subTextEle = mutiPartElement.QueryPart(i) as ITextElement;
                                    string subText = subTextEle.Text;
                                    if (subText == replaceChar.ToString())
                                    {
                                        mutiPartElement.ReplacePart(i, charList[index++].ToString(), (subTextEle as IElement).Geometry);
                                    }
                                }
                            }

                            (annoElement as ISymbolCollectionElement).CharacterSpacing = 0;

                            annoFeature.Annotation = annoElement;
                            fe.Store();
                        }
                    }
                }

                m_Application.EngineEditor.StopOperation("注记转多部件");

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

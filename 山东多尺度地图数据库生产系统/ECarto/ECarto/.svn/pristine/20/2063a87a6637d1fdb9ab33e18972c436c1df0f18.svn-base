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
    public class AnnotationToSinglepartCommand : SMGI.Common.SMGICommand
    {
        public AnnotationToSinglepartCommand()
        {
            m_caption = "注记转单部件";
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
            bool isExitMultiAnnoFe = false;
            #region 获取选中的注记要素
            IEnumFeature enumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            IFeature feature = null;
            while ((feature = enumFeature.Next()) != null)
            {
                IAnnotationFeature annoFeature = feature as IAnnotationFeature;
                if (annoFeature == null || annoFeature.Annotation == null)
                    continue;

                IElement element = annoFeature.Annotation;
                if (element is IAnnotationElement)
                {
                    selFeList.Add(feature);

                    if (element is IMultiPartTextElement && (element as IMultiPartTextElement).IsMultipart)
                    {
                        isExitMultiAnnoFe = true;
                    }
                }
            }
            Marshal.ReleaseComObject(enumFeature);
            #endregion
            if (!isExitMultiAnnoFe)
                return;

            m_Application.EngineEditor.EnableUndoRedo(true);
            m_Application.EngineEditor.StartOperation();
            try
            {
                foreach (var fe in selFeList)
                {
                    IAnnotationFeature annoFeature = fe as IAnnotationFeature;
                    IElement annoElement = annoFeature.Annotation;

                    if (annoElement is IMultiPartTextElement && (annoElement as IMultiPartTextElement).IsMultipart)
                    {
                        IMultiPartTextElement mutiPartElement = annoElement as IMultiPartTextElement;

                        //处理中文字符后面的空格等（这里需与转多部件工具对应）
                        string text = (annoElement as ITextElement).Text;
                        (annoElement as ITextElement).Text = AnnotationHelper.DelSpaceAfterChinese(text);

                        mutiPartElement.ConvertToSinglePart();

                        annoFeature.Annotation = annoElement;
                        fe.Store();
                    }
                }

                m_Application.EngineEditor.StopOperation("注记转单部件");

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

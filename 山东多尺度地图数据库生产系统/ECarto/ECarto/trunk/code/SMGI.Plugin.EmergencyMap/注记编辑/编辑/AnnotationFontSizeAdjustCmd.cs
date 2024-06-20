using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.Windows;

namespace SMGI.Plugin.EmergencyMap
{
    public class AnnotationFontSizeAdjustCmd: SMGI.Common.SMGICommand
    {
        public AnnotationFontSizeAdjustCmd()
        {
            m_caption = "注记字大缩放调整";
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

        public override void OnClick()
        {
            AnnotationFontSizeAdjustForm frm = new AnnotationFontSizeAdjustForm();
            frm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            m_Application.EngineEditor.StartOperation();
            try
            {
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = frm.AnnoSelectSQL;
                IFeatureCursor feCursor = frm.ObjAnnoLayer.Search(qf, false);
                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    IAnnotationFeature annoFe = fe as IAnnotationFeature;
                    ITextElement textElement = annoFe.Annotation as ITextElement;

                    ISymbolCollectionElement symbolCollEle = (ISymbolCollectionElement)textElement;
                    symbolCollEle.Size *= frm.AdjustScale;

                    annoFe.Annotation = textElement as IElement;
                    fe.Store();
                }
                Marshal.ReleaseComObject(feCursor);

                m_Application.EngineEditor.StopOperation("注记缩放");

                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, frm.ObjAnnoLayer, m_Application.ActiveView.Extent);
            }
            catch (Exception ex)
            {
                m_Application.EngineEditor.AbortOperation();

                MessageBox.Show(ex.Message);
            }

        }
    }
}

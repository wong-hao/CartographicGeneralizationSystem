using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using stdole;
using System.Drawing;

namespace SMGI.Plugin.EmergencyMap
{
    public class InsertTextTool : SMGI.Common.SMGITool
    {

        public InsertTextTool()
        {
            m_caption = "插入文本";
          
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

        }  

        public override void OnClick()
        {

        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;

            IPoint pt = ToSnapedMapPoint(x, y);

            IElement element = CreateTextElement("文本", "宋体", 10.0, pt);
            if (element != null)
            {
                bool res = EditElementPropertyCmd.EditElementProperty(element, true);
                if (res)
                {
                    m_Application.MapControl.ActiveView.GraphicsContainer.AddElement(element, 0);
                    (m_Application.ActiveView).PartialRefresh(esriViewDrawPhase.esriViewGraphics, element, null);
                }
            }
        }

        private IElement CreateTextElement(string text, string fontName, double size, IPoint pt)
        {
            ITextElement textElment = null;

            try
            {
                textElment = new TextElementClass();
                textElment.Text = text;

                (textElment as ISymbolCollectionElement).FontName = fontName;
                (textElment as ISymbolCollectionElement).Size = size;

                (textElment as IElement).Geometry = pt;

                return textElment as IElement;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                return null;
            }
        }
    }
}

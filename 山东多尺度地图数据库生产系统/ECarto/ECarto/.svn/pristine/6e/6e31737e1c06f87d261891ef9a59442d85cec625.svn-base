using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.EmergencyMap
{
    public  class AnnoLeaderLineAddTool:SMGITool
    {
        public AnnoLeaderLineAddTool()
        {
            m_caption = "注记引线";
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
        }
        public override void OnClick()
        {
        }

        public override void OnDblClick()
        {
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
        }

        public override void OnKeyUp(int keyCode, int shift)
        {

        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button!=1)
                return;

            IGeometry trackGeometry = m_Application.MapControl.TrackLine();
            if (trackGeometry.IsEmpty)
                return;

            ITopologicalOperator trackTopo = trackGeometry as ITopologicalOperator;
            trackTopo.Simplify();

            IPolyline trackPolyline = trackGeometry as IPolyline;

            //获取注记图层
            List<IFeatureClass> annoFCList = new List<IFeatureClass>();
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IFDOGraphicsLayer; })).ToArray();
            foreach(var lyr in lyrs)
            {
                if (lyr is IFeatureLayer)
                {
                    IFeatureClass fc = (lyr as IFeatureLayer).FeatureClass;
                    if(fc != null)
                        annoFCList.Add(fc);
                }
            }
            if (annoFCList.Count == 0)
                return;

            //获取牵引线的端点处注记要素，并获取其颜色值
            IColor annoColor = null;

            IGeometry geo = trackPolyline.ToPoint;
            if (m_Application.MapControl.Map.ReferenceScale > 0)
            {
                geo = (geo as ITopologicalOperator).Buffer(0.0005 * m_Application.MapControl.Map.ReferenceScale);//缓冲0.5mm
            }
            ISpatialFilter sp = new SpatialFilter();
            sp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sp.Geometry = geo;
            foreach (var annoFC in annoFCList)
            {
                IFeatureCursor feCursor = annoFC.Search(sp, false);
                IFeature fe = null;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    IAnnotationFeature annoFe = fe as IAnnotationFeature;
                    if (annoFe.Annotation == null)
                        continue;

                    ITextElement textElement = null;
                    if (annoFe.Annotation is ITextElement)
                    {
                        textElement = annoFe.Annotation as ITextElement;
                    }
                    else if (annoFe.Annotation is IGroupElement)//合并注记
                    {
                        IGroupElement gpELe = annoFe.Annotation as IGroupElement;
                        for (int i = 0; i < gpELe.ElementCount; ++i)
                        {
                            textElement = gpELe.get_Element(i) as ITextElement;
                            if (textElement != null)
                                break;//跳出GroupElement
                        }
                    }

                    if (textElement != null)
                    {
                        annoColor = textElement.Symbol.Color;
                        if(annoColor != null)
                            break;//跳出feCursor
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

                if (annoColor != null)
                    break;//跳出annoFCList
            }


            if (annoColor != null)
            {
                ILineElement lineEle = new LineElementClass();
                lineEle.Symbol = new SimpleLineSymbolClass() { Color = annoColor, Width = 0.1 };
                (lineEle as IElement).Geometry = trackPolyline;
                m_Application.ActiveView.GraphicsContainer.AddElement(lineEle as IElement, 99);
                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, trackPolyline.Envelope);
            }
            
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
        }

        public override void Refresh(int hdc)
        {
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
    }
}

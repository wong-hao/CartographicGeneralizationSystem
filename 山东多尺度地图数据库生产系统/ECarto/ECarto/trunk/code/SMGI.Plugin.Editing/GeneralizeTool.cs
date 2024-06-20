using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.GeneralEdit
{
    public class GeneralizeTool : SMGI.Common.SMGITool
    {
        private INewEnvelopeFeedback m_envelopeFeedback;
        private bool m_isMouseDown = false;
        private double maxAllowableOffset;


        /// <summary>
        /// ArcGIS高级编辑工具GeneralizeTool（概化工具）
        /// </summary>
        /// 
        public GeneralizeTool()
        {
            m_caption = "概化Tool";
            m_category = "编辑工具";
            m_toolTip = "简化所选线和面要素的形状,右键设置容差";
            NeedSnap = false;                       
        }


        /// <summary>
        /// 将Geomentry转成Path
        /// </summary>
        /// <param name="geometry">要素类型</param>
        /// <returns></returns>
        private IPath ConvertGeometryToPath(IGeometry geometry)
        {
            ISegmentCollection geoSegmentCollection = geometry as ISegmentCollection;
            ISegmentCollection pathSegmentCollection = new PathClass();
            for (int i = 0; i < geoSegmentCollection.SegmentCount; i++)
            {
                pathSegmentCollection.AddSegment(geoSegmentCollection.get_Segment(i));
            }
            return pathSegmentCollection as IPath;
        }


        /// <summary>
        /// 将Path转成Geometry
        /// </summary>
        /// <param name="path">path对象</param>
        /// <param name="egt">要转成的要素类型(线和面)</param>
        /// <returns></returns>
        private IGeometry ConvertPathToGeometry(IPath path, esriGeometryType egt)
        {
            ISegmentCollection segmentCollection = path as ISegmentCollection;
            ISegmentCollection geoSegmentColletion = null;
            switch (egt)
            {
                case esriGeometryType.esriGeometryPolyline:
                    geoSegmentColletion = new PolylineClass();
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    geoSegmentColletion = new PolygonClass();
                    break;
            }
            for (int i = 0; i < segmentCollection.SegmentCount; i++)
            {
                geoSegmentColletion.AddSegment(segmentCollection.get_Segment(i));
            }
            return geoSegmentColletion as IGeometry;
        }


        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1)//左键
            {
                m_isMouseDown = true;
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                if (m_envelopeFeedback == null)
                {
                    m_envelopeFeedback = new NewEnvelopeFeedbackClass();
                    m_envelopeFeedback.Display = m_Application.ActiveView.ScreenDisplay;
                    m_envelopeFeedback.Start(currentMouseCoords);
                }
                else
                {
                    m_envelopeFeedback.Start(currentMouseCoords);
                }
            }
        }


        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (button == 1)//左键
            {
                if (m_isMouseDown && m_envelopeFeedback != null)
                {
                    IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                    m_envelopeFeedback.MoveTo(currentMouseCoords);
                }
            }    
        }


        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            if (button == 1)//左键
            {
                if (maxAllowableOffset == null || maxAllowableOffset <= 0.0)
                {
                    MessageBox.Show(string.Format("未设置偏移限差，请右键设置"));
                }
                IPoint currentMouseCoords = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                IGeometry geo = m_envelopeFeedback.Stop();
                IMap m_Map = m_Application.ActiveView as IMap;
                if (geo != null)
                {
                    m_Map.SelectByShape(geo, null, false);
                    
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = null;
                    m_Application.EngineEditor.StartOperation();
                    while ((feature = mapEnumFeature.Next()) != null)
                    {                        
                        //只处理线要素和面要素
                        if (feature.Shape is IPolyline || feature.Shape is IPolygon)
                        {
                            IPath path = ConvertGeometryToPath(feature.Shape);

                            if (m_Application.Workspace.Map.SpatialReference is IGeographicCoordinateSystem)
                            {
                                path.Generalize(maxAllowableOffset * 0.000009);
                            }
                            else
                            {
                                path.Generalize(maxAllowableOffset);
                            }

                            IGeometry geometry = ConvertPathToGeometry(path, feature.Shape.GeometryType);
                            feature.Shape = geometry;
                            feature.Store();
                        }
                        //System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                    } 
                    //m_Application.ActiveView.Refresh();
                    m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, geo.Envelope);                    
                    //m_Application.EngineEditor.StopOperation("概化Tool操作");
                    m_Application.EngineEditor.StopOperation(this.Caption);
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                }
                m_isMouseDown = false;
                m_envelopeFeedback = null;
            }
            else if (button == 2)//右键，弹出窗口
            {
                GeneralizeForm gf;
                if (maxAllowableOffset == null)
                {
                    gf = new GeneralizeForm();
                }
                else
                {
                    gf = new GeneralizeForm(maxAllowableOffset); 
                }
                gf.Text = "概化Tool";
                if (gf.ShowDialog() == DialogResult.OK)
                {
                    maxAllowableOffset = gf.maxAllowableOffset; 
                }
            }
        }


        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    return true;
                }
                else
                    return false;
            }
        }
    }
}

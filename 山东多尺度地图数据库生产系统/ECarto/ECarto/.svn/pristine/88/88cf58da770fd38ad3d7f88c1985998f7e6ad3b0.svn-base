using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.GeneralEdit
{
    public class GeneralizeCommand:SMGI.Common.SMGICommand
    {
        /// <summary>
        /// ArcGIS高级编辑工具Generalize（概化工具）
        /// </summary>
        public GeneralizeCommand()
        {
            m_caption = "概化";
            m_category = "编辑工具";
            m_toolTip = "简化所选线和面要素的形状。简化的程度取决于限于输出几何与输入几何间距离的最大允许偏移量";
        }
        public override void OnClick()
        {
            GeneralizeForm gf = new GeneralizeForm();
            gf.Text = "概化";
            if (gf.ShowDialog() == DialogResult.OK) 
            {
                m_Application.EngineEditor.StartOperation();
                double maxX = 0;
                double minX = 0;
                double maxY = 0;
                double minY = 0;
                double maxAllowableOffset = gf.maxAllowableOffset;
                IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                mapEnumFeature.Reset();
                IFeature feature = null;
                while ((feature = mapEnumFeature.Next()) != null)
                {
                    //只处理线要素和面要素
                    if (feature.Shape is IPolyline || feature.Shape is IPolygon) 
                    {
                        if (feature.Shape.Envelope.XMax > maxX)
                            maxX = feature.Shape.Envelope.XMax;
                        if (feature.Shape.Envelope.YMax > maxY)
                            maxY = feature.Shape.Envelope.YMax;
                        if (feature.Shape.Envelope.XMin < minX)
                            minX = feature.Shape.Envelope.XMin;
                        if (feature.Shape.Envelope.YMin < minY)
                            minY = feature.Shape.Envelope.YMin;
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
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
                IEnvelope newEnvelope = new EnvelopeClass();
                newEnvelope.XMax = maxX;
                newEnvelope.YMax = maxY;
                newEnvelope.XMin = minX;
                newEnvelope.YMin = minY;
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, newEnvelope);
                m_Application.EngineEditor.StopOperation("概化操作");
            }
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

        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = mapEnumFeature.Next();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
                    if (feature != null)
                    {
                        if (feature.Shape is IPolyline || feature.Shape is IPolygon)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                            return true;
                        }
                        else
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                            return false;
                        }
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }
    }
}

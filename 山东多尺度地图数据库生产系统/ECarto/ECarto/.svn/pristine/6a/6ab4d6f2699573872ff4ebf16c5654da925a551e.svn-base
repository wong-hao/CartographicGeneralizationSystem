using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.GeneralEdit
{
    public class SmoothCommand:SMGI.Common.SMGICommand
    {
        /// <summary>
        /// ArcGIS高级编辑工具Smooth（平滑工具）
        /// </summary>
        public SmoothCommand()
        {
            m_caption = "平滑";
            m_category = "编辑工具";
            m_toolTip = "将要素的直角边和拐角边处理为贝塞尔曲线";
        }
        public override void OnClick()
        {
            GeneralizeForm gf = new GeneralizeForm();
            gf.Text = "平滑";
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
                            path.Smooth(maxAllowableOffset * 0.000009);//光滑后再抽稀
                            path.Generalize(0.1 * 0.000009);
                        }
                        else
                        {
                            path.Smooth(maxAllowableOffset);//光滑后再抽稀
                            path.Generalize(0.1);
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
                m_Application.EngineEditor.StopOperation("平滑操作");
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

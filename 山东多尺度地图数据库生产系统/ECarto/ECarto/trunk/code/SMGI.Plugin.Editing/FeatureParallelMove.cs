using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.GeneralEdit
{
    public class FeatureParallelMove : SMGI.Common.SMGICommand
    {
        public FeatureParallelMove()
        {
            m_caption = "要素平行移动";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       1 == m_Application.MapControl.Map.SelectionCount &&
                       m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        public override void OnClick() 
        {
            FeatureParallelCopyForm frm = new FeatureParallelCopyForm();
            if (frm.ShowDialog() != DialogResult.OK) 
            {
                return;
            }
            
            ISelection selectfeature = m_Application.MapControl.Map.FeatureSelection;
            IEnumFeature enumFeature = (IEnumFeature)selectfeature;
            enumFeature.Reset();
            IFeature feature = null;
            m_Application.EngineEditor.StartOperation();
            while ((feature = enumFeature.Next()) != null) 
            {
                IGeometry pGeometry = feature.ShapeCopy as IGeometry;
              
                IObjectClass pObjectFeature = feature.Class;
                IFeatureClass pFeatureclass = pObjectFeature as IFeatureClass;
                IConstructCurve pOffset = null;
                if (pFeatureclass.ShapeType == esriGeometryType.esriGeometryPolygon)
                {
                    pOffset = new PolygonClass();
                }
                else if (pFeatureclass.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    pOffset = new PolylineClass();
                }
                if (pOffset == null)
                    continue;
              

                double lengthof = frm.OffsetDis;
                pOffset.ConstructOffset(pGeometry as IPolycurve, lengthof, esriConstructOffsetEnum.esriConstructOffsetSimple);
                feature.Shape = pOffset as IGeometry;
                feature.Store();
            }
            m_Application.EngineEditor.StopOperation("平行移动");
            System.Runtime.InteropServices.Marshal.ReleaseComObject(selectfeature);
            m_Application.ActiveView.Refresh();
        }
    }
}

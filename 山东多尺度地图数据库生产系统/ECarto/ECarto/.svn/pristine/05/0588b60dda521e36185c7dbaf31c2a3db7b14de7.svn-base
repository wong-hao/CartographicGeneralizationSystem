using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;

namespace SMGI.Plugin.GeneralEdit
{
    public  class ConverseLineDirection:SMGICommand
    {
        public ConverseLineDirection()
        {
            m_caption = "线反向";
            m_toolTip = "线反向工具";
            m_category = "基础编辑";
            
        }

        public override void OnClick()
        {
            IEngineEditor m_engineEditor = m_Application.EngineEditor;
            IMap pMap = m_Application.Workspace.Map;
            IEnumFeature pEnumFeature = (IEnumFeature)pMap.FeatureSelection;

            List<IFeature> LineList = new List<IFeature>();
            ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
            IFeature feature = pEnumFeature.Next();
            while (feature != null)
            {
                if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                {
                    LineList.Add(feature);
                }
                feature = pEnumFeature.Next();
            }
            if (LineList.Count ==0)
            {
                MessageBox.Show("没有选择线要素");
                return;
            }
            m_engineEditor.StartOperation();
            for (int i = 0; i < LineList.Count; i++)
            {
                IFeature LineFeature = LineList[i];
                IGeometryCollection geoCol = LineFeature.Shape as IGeometryCollection;
                for (int k = 0; k < geoCol.GeometryCount; k++)
                {
                    IPointCollection pPointColl = (IPointCollection)geoCol.get_Geometry(k);
                    IPointCollection NewPointColl = new PolylineClass();
                    for (int j = pPointColl.PointCount - 1; j >= 0; j--)
                    {
                        object obj = Type.Missing;
                        NewPointColl.AddPoint(pPointColl.get_Point(j), ref obj, ref obj);
                    }
                    LineFeature.Shape = NewPointColl as IGeometry;
                    LineFeature.Store();
                }
            }

            m_engineEditor.StopOperation("线反向");
            m_Application.ActiveView.Refresh();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumFeature);
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null
                    && m_Application.Workspace != null
                    && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing
                    && m_Application.LayoutState == Common.LayoutState.MapControl;
            }
        }
    }
}

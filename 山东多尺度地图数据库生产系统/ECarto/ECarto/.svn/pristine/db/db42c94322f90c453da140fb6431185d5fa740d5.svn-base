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
using SMGI.Common;

namespace SMGI.Plugin.GeneralEdit
{
    public class Extend:SMGITool
    {
        public Extend()
        {
            m_caption = "延伸";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));

            NeedSnap = false;
        }
        public override void OnClick()
        {
            if (m_Application.Workspace.Map.SelectionCount == 0)
            {
                MessageBox.Show("没有选中延伸参考要素");
                return;
            }
            
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (m_Application.Workspace.Map.SelectionCount == 0)
            {
                MessageBox.Show("没有选中延伸参考要素");
                return;
            }

            var editor = m_Application.EngineEditor;
            IPoint SearchPt = ToSnapedMapPoint(x, y);

            List<ILayer> PolylineLayers = GetFeatureLayers(m_Application.Workspace.Map);
            IFeature ExtendedFeature = GetExtendedFeature(PolylineLayers, SearchPt);
            if (ExtendedFeature == null)
            {
                return;
            }
            ISegmentCollection pSegColl =(ISegmentCollection) ExtendedFeature.Shape ;
            ICurve FromCurve = (ICurve)pSegColl;

            IEnumFeature pEnumFeature = (IEnumFeature)m_Application.Workspace.Map.FeatureSelection;
            ((IEnumFeatureSetup)pEnumFeature).AllFields = true;

            editor.StartOperation();
            IFeature refFeature = pEnumFeature.Next();
            while (refFeature != null)
            {
                pSegColl = (ISegmentCollection)refFeature.Shape;
                ICurve ToCurve = (ICurve)pSegColl;
                bool Isdone = false;
                IConstructCurve pConstructEx = new PolylineClass();
                pConstructEx.ConstructExtended(FromCurve, ToCurve, 0, ref Isdone);

                if (Isdone == true)
                {
                    ExtendedFeature.Shape = (IGeometry)pConstructEx;
                    ExtendedFeature.Store();
                }
                refFeature = pEnumFeature.Next();
            }
            editor.StopOperation(null);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumFeature);
            m_Application.ActiveView.Refresh();
        }
        IFeature GetExtendedFeature(List<ILayer>LayerList,IPoint SearchPoint)
        {
            IFeature pFeature = null;
            ITopologicalOperator pTop = (ITopologicalOperator)SearchPoint;
            pTop.Simplify();
            double bufValue = 0;
            if (m_Application.Workspace.Map.SpatialReference is IGeographicCoordinateSystem)
            {
                bufValue = m_Application.Workspace.Map.MapScale * 0.002*0.000009;
            }
            else
            {
                bufValue = m_Application.Workspace.Map.MapScale * 0.002;
            }

            IGeometry BufferGeo = pTop.Buffer(bufValue);
            for (int i = 0; i < LayerList.Count; i++)
            {
                ISpatialFilter pFilter = new SpatialFilterClass();
                pFilter.Geometry = BufferGeo;
                pFilter.GeometryField = ((IFeatureLayer)LayerList[i]).FeatureClass.ShapeFieldName;
                pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor pFeatCursor = ((IFeatureLayer)LayerList[i]).Search(pFilter, false);
                pFeature = pFeatCursor.NextFeature();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatCursor);
                if (pFeature != null)
                {
                    break;
                }
            }
            return pFeature;
        }
        public List<ILayer> GetFeatureLayers(IMap pMap)
        {
            List<ILayer> LayerList = new List<ILayer>();
            for (int i = 0; i < pMap.LayerCount; i++)
            {
                ILayer pLayer = pMap.get_Layer(i);
                GetLayers(pLayer, esriGeometryType.esriGeometryPolyline, LayerList);
            }
           
            return LayerList;
        }
        private void GetLayers(ILayer pLayer, esriGeometryType pGeoType, List<ILayer> LayerList)
        {
            if (pLayer is IGroupLayer)
            {
                ICompositeLayer pGroupLayer = (ICompositeLayer)pLayer;
                for (int i = 0; i < pGroupLayer.Count; i++)
                {
                    ILayer SubLayer = pGroupLayer.get_Layer(i);
                    GetLayers(SubLayer, pGeoType, LayerList);
                }
            }
            else
            {
                if (pLayer is IFeatureLayer)
                {
                    IFeatureLayer pFeatLayer = (IFeatureLayer)pLayer;
                    IFeatureClass pFeatClass = pFeatLayer.FeatureClass;
                    if (pFeatClass.ShapeType == pGeoType &&　pLayer.Visible ==true)
                    {
                        LayerList.Add(pLayer);
                    }
                }
            }
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null
                    && m_Application.Workspace != null
                    && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
    }
}

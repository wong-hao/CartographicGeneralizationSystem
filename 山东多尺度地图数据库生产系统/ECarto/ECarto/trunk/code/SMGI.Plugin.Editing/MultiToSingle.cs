using System;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.GeneralEdit
{
    public class MultiToSingle : SMGICommand
    {
        public MultiToSingle()
        {
            m_caption = "打散要素";
            m_toolTip = "打散选中要素";
            m_category = "基础编辑";
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
            var map = m_Application.Workspace.Map;
            if (map.SelectionCount == 0) return;

            var fes = (IEnumFeature)map.FeatureSelection;
            fes.Reset();
            IFeature fe = null;
            m_Application.EngineEditor.StartOperation();
            while ((fe  =  fes.Next())!=null)
            {
                var layer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == fe.Class.AliasName)).FirstOrDefault();
                if (layer == null || !layer.Visible || !(layer is IFeatureLayer)) continue;
                var fc = ((IFeatureLayer) layer).FeatureClass;

                if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolygon) //面打散
                {
                    var po = (IPolygon4) fe.ShapeCopy;
                    var gc = (IGeometryCollection) po.ConnectedComponentBag;
                    if (gc.GeometryCount <= 1) continue;
                    for (var i = 1; i < gc.GeometryCount; i++)
                    {
                        var fci = fc.Insert(true);
                        var fb = fc.CreateFeatureBuffer();
                        fb = (IFeatureBuffer)fe;
                        fb.Shape = gc.Geometry[i];
                        fci.InsertFeature(fb);
                        fci.Flush();
                    }
                    fe.Shape = gc.Geometry[0];
                    fe.Store();
                }
                else if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline) //线打散
                {
                    var gc = (IGeometryCollection)fe.ShapeCopy;
                    if (gc.GeometryCount <= 1) continue;
                    for (var i = 1; i < gc.GeometryCount; i++)
                    {
                        var fci = fc.Insert(true);
                        var fb = fc.CreateFeatureBuffer();
                        fb = (IFeatureBuffer)fe;
                        IPointCollection pc = new PolylineClass();
                        pc.AddPointCollection((IPointCollection)gc.Geometry[i]);
                        fb.Shape = pc as IPolyline;
                        fci.InsertFeature(fb);
                        fci.Flush();
                    }
                    IPointCollection pl = new PolylineClass();
                    pl.AddPointCollection((IPointCollection)gc.Geometry[0]);
                    fe.Shape = pl as IPolyline;
                    fe.Store();
                }
            }
            m_Application.EngineEditor.StopOperation("打散选中要素");
            m_Application.ActiveView.Refresh();
        }
    }
}
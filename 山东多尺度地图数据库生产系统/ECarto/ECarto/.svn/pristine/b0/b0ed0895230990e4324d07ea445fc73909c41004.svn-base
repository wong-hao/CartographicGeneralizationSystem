using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Collections;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Diagnostics;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.Drawing;
using ESRI.ArcGIS.Display;
using System.Windows.Forms;

namespace SMGI.Plugin.GeneralEdit
{
    public class AutoConnectionPolyline : SMGI.Common.SMGICommand
    {
        List<IFeature> selFeas = null;
        double condistance = 0;
        double closedistance = 0;
        IFeatureWorkspace featureWorkspace = null;
        IFeatureClass CheckClass;
        public AutoConnectionPolyline()
        {
            m_caption = "自动连接";
            m_toolTip = "自动连接";
            m_category = "特殊";
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
        {//若有选中要素则处理选中要素否则处理当前编辑图层
            AutoConnectionPolylineForm layerSelector = new AutoConnectionPolylineForm();
            //  layerSelector.StartPosition = FormStartPosition.CenterScreen;
            //  layerSelector.Show();
            if (layerSelector.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            } selFeas = new List<IFeature>();
            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
            IEngineEditLayers engineEditLayer = editsketch as IEngineEditLayers;
            IFeatureLayer featureLayer = engineEditLayer.TargetLayer as IFeatureLayer;
            CheckClass = featureLayer.FeatureClass;
            condistance = layerSelector.connectdis;
            closedistance = layerSelector.closedis;
            m_Application.EngineEditor.StartOperation();
            IEnumFeature pEnumFeature = (IEnumFeature)m_Application.MapControl.Map.FeatureSelection;
            IFeature feature = null;
            int count = 0;
            while ((feature = pEnumFeature.Next()) != null)
            {
                if (!closed(feature))
                {
                    connect(feature);
                }
                count++;
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumFeature)   ;
            if (count == 0)
            {
                IFeature temfeature = null;
                IFeatureCursor cursor = CheckClass.Search(null, false);
                while ((temfeature = cursor.NextFeature()) != null)
                {
                    if (selFeas.Contains(temfeature)) { continue; }
                    if (!closed(temfeature)) { connect(temfeature); }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                for (int i = 0; i < selFeas.Count; i++)
                {
                    selFeas[i].Delete();
                }
            }
            m_Application.EngineEditor.StopOperation("自动连接");
            m_Application.MapControl.Refresh();
        }
        /// <summary>
        /// 闭合
        /// </summary>
        /// <param name="fea"></param>
        /// <returns></returns>
        public bool closed(IFeature fea)
        {
            bool flag = false;
            IPolyline ply = fea.ShapeCopy as IPolyline;
            IPointCollection ptc = ply as IPointCollection;
            IPoint ptfrom = ply.FromPoint;
            IPoint ptto = ply.ToPoint;
            IProximityOperator ProxiOP = (ptfrom) as IProximityOperator;
            if (ProxiOP.ReturnDistance(ptto) < closedistance)
            {
                ptc.AddPoint(ptfrom);
                (ply as ITopologicalOperator).Simplify();
                fea.Shape = ply;
                fea.Store();
                flag = true;
            }
            return flag;
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="fea"></param>
        public void connect(IFeature fea)
        {
            ISpatialFilter filter = new SpatialFilterClass();
            filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IPolyline ply = fea.ShapeCopy as IPolyline;
            IPointCollection feacol = ply as IPointCollection;
            //起点
            if (point(ply.FromPoint)) { return; }
            IGeometry ptFrom = (ply.FromPoint as ITopologicalOperator).Buffer(condistance);
            filter.Geometry = ptFrom;
            IFeature maskedFeature = null;
            IFeatureCursor maskedCursor = CheckClass.Search(filter, false);

            while ((maskedFeature = maskedCursor.NextFeature()) != null)
            {
                if (selFeas.Contains(maskedFeature)) { continue; }
                if (maskedFeature.OID == fea.OID) { continue; }
                IPolyline tempply = maskedFeature.ShapeCopy as IPolyline;
                IPointCollection col = tempply as IPointCollection;

                IPoint ptfrom = tempply.FromPoint;
                IProximityOperator ProxiOP = (ptfrom) as IProximityOperator;

                IPoint ptto = tempply.ToPoint;
                IProximityOperator ProxiOT = (ptto) as IProximityOperator;
                //起点与起点
                if (ProxiOP.ReturnDistance(ply.FromPoint) < condistance)
                {
                    if (point(ptfrom)) { continue; }
                    IPolyline py = new PolylineClass();
                    IPointCollection Tcol = py as IPointCollection;

                    for (int i = col.PointCount - 1; i > -1; i--)
                    {
                        IPoint pt = col.get_Point(i);
                        Tcol.AddPoint(pt);
                    }
                    Tcol.AddPointCollection(feacol);
                    (py as ITopologicalOperator).Simplify();
                    fea.Shape = py;
                    fea.Store();

                    selFeas.Add(maskedFeature);
                    break;
                }
                //终点与起点
                else if (ProxiOT.ReturnDistance(ply.FromPoint) < condistance)
                {
                    if (point(ptto)) { continue; }
                    col.AddPointCollection(feacol);
                    (tempply as ITopologicalOperator).Simplify();
                    fea.Shape = tempply;
                    fea.Store();
                    selFeas.Add(maskedFeature);
                    break;
                }

            } System.Runtime.InteropServices.Marshal.ReleaseComObject(maskedCursor);
            //终点
            IPolyline plyy = fea.ShapeCopy as IPolyline;
            IGeometry ptTo = (plyy.ToPoint as ITopologicalOperator).Buffer(condistance);
            filter.Geometry = ptTo;
            IFeature Feature = null;
            IFeatureCursor Cursor = CheckClass.Search(filter, false);

            while ((Feature = Cursor.NextFeature()) != null)
            {
                if (selFeas.Contains(Feature)) { continue; }
                if (Feature.OID == fea.OID) { continue; }
                IPolyline tempply = Feature.ShapeCopy as IPolyline;
                IPointCollection col = tempply as IPointCollection;

                IPoint ptfrom = tempply.FromPoint;
                IProximityOperator ProxiOP = (ptfrom) as IProximityOperator;

                IPoint ptto = tempply.ToPoint;
                IProximityOperator ProxiOT = (ptto) as IProximityOperator;


                //起点与终点
                if (ProxiOP.ReturnDistance(plyy.ToPoint) < condistance)
                {
                    if (point(ptfrom)) { continue; }
                    feacol.AddPointCollection(col);
                    (plyy as ITopologicalOperator).Simplify();
                    fea.Shape = plyy;
                    fea.Store();


                    selFeas.Add(Feature);
                    break;
                }

               //终点与终点
                else if (ProxiOT.ReturnDistance(plyy.ToPoint) < condistance)
                {
                    if (point(ptto)) { continue; }
                    IPolyline py = new PolylineClass();
                    IPointCollection Tcol = py as IPointCollection;

                    for (int i = 0; i < col.PointCount; i++)
                    {
                        IPoint pt = col.get_Point(i);
                        Tcol.AddPoint(pt);
                    }
                    feacol.AddPointCollection(Tcol);
                    (plyy as ITopologicalOperator).Simplify();
                    fea.Shape = plyy;
                    fea.Store();
                    selFeas.Add(Feature);

                    break;
                }

            } System.Runtime.InteropServices.Marshal.ReleaseComObject(Cursor);
        }
        /// <summary>
        /// 判断点是否在线上
        /// </summary>
        /// <param name="fea"></param>
        /// <returns></returns>
        public bool point(IPoint fea)
        {
            bool flag = false;
            ISpatialFilter filter = new SpatialFilterClass();
            filter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            filter.Geometry = fea;

            int count = CheckClass.FeatureCount(filter);
            if (count > 1) { flag = true; }
            return flag;
        }
    }
}

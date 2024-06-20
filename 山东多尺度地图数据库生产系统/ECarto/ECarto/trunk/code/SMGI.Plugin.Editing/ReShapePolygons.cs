using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class ReShapePolygons : SMGI.Common.SMGITool
    {
        /// <summary>
        /// 编辑器
        /// </summary>
        IEngineEditor editor;
        /// <summary>
        /// 线型符号
        /// </summary>
        ISimpleLineSymbol lineSymbol;
        /// <summary>
        /// 线型反馈
        /// </summary>
        INewLineFeedback lineFeedback;
        /// <summary>
        /// 选中的feature
        /// </summary>
        IFeature feature;

        public ReShapePolygons()
        {
            m_caption = "高级修面";
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
            editor = m_Application.EngineEditor;
            //#region Create a symbol to use for feedback
            lineSymbol = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass();	 //red
            color.Red = 255;
            color.Green = 0;
            color.Blue = 0;
            lineSymbol.Color = color;
            lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            lineSymbol.Width = 1.5;
            (lineSymbol as ISymbol).ROP2 = esriRasterOpCode.esriROPNotXOrPen;//这个属性很重要
            //#endregion
            lineFeedback = null;
            //用于解决在绘制feedback过程中进行地图平移出现线条混乱的问题
            m_Application.MapControl.OnAfterScreenDraw += new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            //获取当前选择的要素
            IEnumFeature pEnumFeature = (IEnumFeature)m_Application.MapControl.Map.FeatureSelection;
            ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
            feature = pEnumFeature.Next();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumFeature);
        }

        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (lineFeedback != null)
            {
                lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
            }
        }

        public override void OnMouseDown(int Button, int Shift, int x, int y)
        {
            if (Button != 1)
                return;
            if (lineFeedback == null)
            {
                var dis = m_Application.ActiveView.ScreenDisplay;
                lineFeedback = new NewLineFeedbackClass { Display = dis, Symbol = lineSymbol as ISymbol };
                lineFeedback.Start(ToSnapedMapPoint(x, y));
            }
            else
            {
                lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (lineFeedback != null)
            {
                lineFeedback.MoveTo(ToSnapedMapPoint(x, y));
            }
        }

        public override void OnDblClick()
        {
            IPolyline polyline = lineFeedback.Stop();
            lineFeedback = null;
            if (null == polyline || polyline.IsEmpty)
                return;

            IPointCollection reshapePath = new PathClass();
            reshapePath.AddPointCollection(polyline as IPointCollection);

            ITopologicalOperator trackTopo = polyline as ITopologicalOperator;
            trackTopo.Simplify();

            ISpatialFilter sf = new SpatialFilter();
            sf.Geometry = polyline;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon;

            })).ToArray();

            IPointCollection geoReshapePath = new PathClass();
            geoReshapePath.AddPointCollection(polyline as IPointCollection);

            try
            {
                IEngineEditLayers editLayer = editor as IEngineEditLayers;

                editor.StartOperation();

                foreach (var item in lyrs)
                {
                    if (!item.Visible)
                    {
                        continue;
                    }

                    if (!editLayer.IsEditable(item as IFeatureLayer))
                    {
                        continue;
                    }

                    IGeoFeatureLayer geoFealyr = item as IGeoFeatureLayer;
                    IFeatureClass fc = geoFealyr.FeatureClass;
                    IFeatureCursor pCursor = fc.Search(sf, false);
                    IFeature pFeature;
                    while ((pFeature = pCursor.NextFeature()) != null)
                    {
                        IPolygon pg = pFeature.ShapeCopy as IPolygon;
                        IPolygon pg1 = pFeature.ShapeCopy as IPolygon;
                        IGeometryCollection pgCol = pg as IGeometryCollection;
                        for (int i = 0; i < pgCol.GeometryCount; i++)
                        {
                            IRing r = pgCol.get_Geometry(i) as IRing;
                            try
                            {
                                r.Reshape(geoReshapePath as IPath);
                            }
                            catch (Exception)
                            {

                                continue;
                            }
                        }
                        (pg as ITopologicalOperator).Simplify();

                        ITopologicalOperator bouaTopo = pg1 as ITopologicalOperator;
                        IGeometry diffGeo = bouaTopo.Difference(pg);
                        pFeature.Shape = pg;
                        pFeature.Store();

                        IFeatureClass feaFC = pFeature.Class as IFeatureClass;

                        if (feaFC.AliasName == "VEGA") { VEGAVEGAP(diffGeo); }
                    }
                    Marshal.ReleaseComObject(pCursor);
                }

                editor.StopOperation("高级修面");

                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);

                ClearSnapperCache();
            }
            catch (Exception ex)
            {
                editor.AbortOperation();
                System.Diagnostics.Trace.WriteLine(ex.Message, "Edit Geometry Failed");
            }
        }
        public void VEGAVEGAP(IGeometry diffGeo)
        {
            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
            IFeatureClass feac = featureWorkspace.OpenFeatureClass("VEGAP");
            ISpatialFilter sf = new SpatialFilter();
            sf.Geometry = diffGeo;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeature pFeature;
            IFeatureCursor feacusor = feac.Search(sf, false);

            while ((pFeature = feacusor.NextFeature()) != null)
            {
                pFeature.Delete();
            } System.Runtime.InteropServices.Marshal.ReleaseComObject(feacusor);
        }
        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);

            return base.Deactivate();
        }
    }
}

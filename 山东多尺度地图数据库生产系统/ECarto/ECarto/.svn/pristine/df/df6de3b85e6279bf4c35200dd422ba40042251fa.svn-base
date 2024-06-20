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
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Editor;


namespace SMGI.Plugin.GeneralEdit
{
    public class ReshapePolylineRepData:SMGITool
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

        public ReshapePolylineRepData()
        {
            m_caption = "修线(图库)";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
            m_toolTip = "修线工具";
            m_category = "基础编辑";
          
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

            editor.StartOperation();
            try
            {
                //修线
                IPolyline editShape = feature.Shape as IPolyline;
                editShape.Reshape(reshapePath as IPath);
                feature.Shape = editShape;
                feature.Store();

                //修改制图几何      
                var rep = GetRepByFeature(feature);                         
                var editGeo = (IPolyline)rep.ShapeCopy;
                editGeo.Reshape(reshapePath as IPath);
                rep.Shape = editGeo;
                rep.UpdateFeature();
                rep.Feature.Store();              

                ISpatialFilter sf = new SpatialFilter();
                sf.Geometry = polyline;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l.Visible && l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon;

                })).ToArray();

                IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;
                foreach (var item in lyrs)
                {
                    if (!editLayer.IsEditable(item as IFeatureLayer))
                        continue;

                    IGeoFeatureLayer geoFealyr = item as IGeoFeatureLayer;
                    IFeatureClass fc = geoFealyr.FeatureClass;
                    IFeatureCursor pCursor = fc.Search(sf, false);
                    IFeature pFeature;
                    while ((pFeature = pCursor.NextFeature()) != null)
                    {
                        IPolygon pg = pFeature.ShapeCopy as IPolygon;
                        IGeometryCollection pgCol = pg as IGeometryCollection;
                        for (int i = 0; i < pgCol.GeometryCount; i++)
                        {
                            IRing r = pgCol.get_Geometry(i) as IRing;
                            try
                            {
                                r.Reshape(reshapePath as IPath);
                            }
                            catch (Exception ex)
                            {

                                continue;
                            }
                        }
                        (pg as ITopologicalOperator).Simplify();
                        pFeature.Shape = pg;
                        pFeature.Store();
                    }
                    Marshal.ReleaseComObject(pCursor);
                }
            }
            catch (Exception ex)
            {
                editor.AbortOperation();
                System.Diagnostics.Trace.WriteLine(ex.Message, "Edit Geometry Failed");
            }

            editor.StopOperation("修线");

            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, feature.Extent);
        }
        private IRepresentation GetRepByFeature(IFeature feature)
        {

            IMap Map = m_Application.ActiveView as IMap;
            var mc = new MapContextClass();
            mc.InitFromDisplay((Map as IActiveView).ScreenDisplay.DisplayTransformation);

            string layerName = (feature.Class as IFeatureClass).AliasName;
            ILayer repLayer = m_Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).Name.ToUpper() == (layerName.ToUpper()))).FirstOrDefault();


            var rr = (repLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            var rpc = rr.RepresentationClass;
            if (rpc == null)
            {
                return null;
            }
            var r = rpc.GetRepresentation(feature, mc);
            return r;


        }
        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            return base.Deactivate();
        }
        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    if (m_Application.MapControl.Map.SelectionCount != 1)
                        return false;
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = mapEnumFeature.Next();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
                    if (feature != null)
                    {
                        if (feature.Shape is IPolyline)
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

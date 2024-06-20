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
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.GeneralEdit
{
    public class SynchronReShapePolygons : SMGI.Common.SMGITool
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

        public SynchronReShapePolygons()
        {
            m_caption = "同步高级修面";
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
                        pFeature.Shape = pg;
                        pFeature.Store();

                        //修改面的制图几何 
                        var repPG = GetRepByFeature(pFeature);
                        if (repPG.RuleID != -1)//非自由制图表达
                        {
                            IGeometry repGeo = repPG.ShapeCopy;
                            if (repGeo is IPolygon)
                            {
                                IPolygon repEditPG = repGeo as IPolygon;
                                IGeometryCollection editPGCol = repEditPG as IGeometryCollection;
                                for (int i = 0; i < editPGCol.GeometryCount; i++)
                                {
                                    IRing r = editPGCol.get_Geometry(i) as IRing;
                                    try
                                    {
                                        r.Reshape(reshapePath as IPath);
                                    }
                                    catch (Exception ex)
                                    {

                                        continue;
                                    }
                                }
                            }
                            else if (repGeo is IPolyline)
                            {
                                var repEditPL = (IPolyline)repGeo;
                                repEditPL.Reshape(reshapePath as IPath);
                            }
                            (repGeo as ITopologicalOperator).Simplify();
                            repPG.Shape = repGeo;
                            repPG.UpdateFeature();
                            repPG.Feature.Store();
                            //植被修面特殊处理。Vega与Vegap联动
                            IFeatureClass feaFC = repPG.Feature.Class as IFeatureClass;

                            if (feaFC.AliasName == "VEGA") { VEGAVEGAP(feaFC, repPG.Feature); }
                        }
                        else//自由式制图表达
                        {
                            IRepresentationGraphics rg = repPG.Graphics;
                            rg.ResetGeometry();
                            int nGeo = 0;
                            while (true)
                            {
                                int oID;
                                IGeometry oGeo;
                                rg.NextGeometry(out oID, out oGeo);
                                if (oGeo == null) break;

                                ++nGeo;

                                oGeo = (IGeometry)((IClone)oGeo).Clone();

                                if (oGeo is IPolygon)
                                {
                                    IPolygon repEditPG = oGeo as IPolygon;
                                    IGeometryCollection editPGCol = repEditPG as IGeometryCollection;
                                    for (int i = 0; i < editPGCol.GeometryCount; i++)
                                    {
                                        IRing r = editPGCol.get_Geometry(i) as IRing;
                                        try
                                        {
                                            r.Reshape(reshapePath as IPath);
                                        }
                                        catch (Exception ex)
                                        {

                                            continue;
                                        }
                                    }
                                }
                                else if (oGeo is IPolyline)
                                {
                                    var repEditPL = (IPolyline)oGeo;
                                    repEditPL.Reshape(reshapePath as IPath);
                                }
                                (oGeo as ITopologicalOperator).Simplify();
                                rg.ChangeGeometry(oID, oGeo);
                            }
                            repPG.UpdateFeature();
                            repPG.Feature.Store();

                            if (nGeo > 1)
                            {
                                MessageBox.Show(string.Format("要素类【{0}】中的要素【{1}】为自由制图表达，且被打散为多个部分，线面联动的效果可能不正确，请改用普通制图修线工具！",
                                    pFeature.Class.AliasName, pFeature.OID));
                            }
                        }

                    }
                    Marshal.ReleaseComObject(pCursor);
                }

                editor.StopOperation("同步高级修面");
            }
            catch (Exception ex)
            {
                editor.AbortOperation();
                System.Diagnostics.Trace.WriteLine(ex.Message, "Edit Geometry Failed");
            }

            m_Application.ActiveView.Refresh();
        }
        public void VEGAVEGAP(IFeatureClass feaFC, IFeature pFeatures)
        {
            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
            IFeatureClass feac = featureWorkspace.OpenFeatureClass("VEGAP");
            var repPG = GetRepByFeature(pFeatures);

            IPolygon polygon = pFeatures.ShapeCopy as IPolygon;
            IPolygon reppolygon = repPG.ShapeCopy as IPolygon;
            ISpatialFilter sf = new SpatialFilter();
            sf.GeometryField = feaFC.ShapeFieldName;
            sf.Geometry = polygon;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeature pFeature;
            IFeatureCursor feacusor = feac.Search(sf, false);
            while ((pFeature = feacusor.NextFeature()) != null)
            {
                IRelationalOperator relationalOperatorPt = pFeature.ShapeCopy as IRelationalOperator;//好接口

                if (relationalOperatorPt.Disjoint(reppolygon))//相交

                { pFeature.Delete(); }


            } System.Runtime.InteropServices.Marshal.ReleaseComObject(feacusor);
        }
        /// <summary>
        /// 获取要素的制图表达
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
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
    }
}


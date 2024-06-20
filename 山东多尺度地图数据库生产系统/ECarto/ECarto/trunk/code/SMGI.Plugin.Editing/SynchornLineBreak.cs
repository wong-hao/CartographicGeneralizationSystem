using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.GeneralEdit
{
    public class SynchornLineBreak : SMGITool
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
        public SynchornLineBreak()
        {
            m_caption = "同步线打断";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
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
        }
        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (lineFeedback != null)
            {
                lineFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
            }
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }
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
            //双击完毕进行线条的打断
            if (null == polyline || polyline.IsEmpty)
                return;
            editor.StartOperation();
            ITopologicalOperator2 pTopo = (ITopologicalOperator2)polyline;
            pTopo.IsKnownSimple_2 = false;
            pTopo.Simplify();

            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = polyline;
            pSpatialFilter.GeometryField = "SHAPE";
            pSpatialFilter.WhereClause = "";
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline;
            })).ToArray();
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
            IEngineEditLayers engineEditLayer = editsketch as IEngineEditLayers;
            IFeatureLayer featureLayer = engineEditLayer.TargetLayer as IFeatureLayer;
            //  foreach (var item in lyrs)
            {
                //   if (!item.Visible)
                //   {
                //       continue;
                //  }
                IGeoFeatureLayer geoFealyr = featureLayer as IGeoFeatureLayer;
                IFeatureClass fc = geoFealyr.FeatureClass;
                // if (fc.ShapeType != esriGeometryType.esriGeometryPolyline) { continue; }

                IFeatureCursor pCursor = fc.Search(pSpatialFilter, false);
                IFeature pFeature = pCursor.NextFeature();

                while (pFeature != null)
                {
                    IPolyline pPolyline = pFeature.Shape as IPolyline;
                    IGeometry InterGeo = pTopo.Intersect(pFeature.Shape, esriGeometryDimension.esriGeometry0Dimension);
                    if (InterGeo.IsEmpty)
                    {
                        pFeature = pCursor.NextFeature();
                        continue;
                    }
                    IGeometryCollection geoCol = InterGeo as IGeometryCollection;
                    #region
                    if (pPolyline.IsClosed) //线闭合
                    {//从第一个交点处打断
                        //if (geoCol.GeometryCount == 1)//若只有一个交点则构造另一个交点
                        //{

                        //}
                        List<int> oid = new List<int>();
                        IPoint splitPt1 = new PointClass();
                        splitPt1 = geoCol.get_Geometry(0) as IPoint;
                        IFeatureEdit pFeatureEdit1 = (IFeatureEdit)pFeature;
                        if (geoFealyr.Renderer is IRepresentationRenderer)
                        {
                            IMapContext mctx = new MapContextClass();
                            mctx.Init((fc as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFealyr.AreaOfInterest);
                            var rpc = (geoFealyr.Renderer as IRepresentationRenderer).RepresentationClass;
                            try
                            {
                                ISet pFeatureSet = pFeatureEdit1.Split(splitPt1);
                                if (pFeatureSet != null)
                                {
                                    pFeatureSet.Reset();
                                    IFeature feature = null;
                                    while ((feature = pFeatureSet.Next() as IFeature) != null) { oid.Add(feature.OID); }
                                    pFeatureSet.Reset();
                                    int count = 0;
                                    IPolyline tempShape2 = null;
                                    while (true)
                                    {
                                        IFeature fe = pFeatureSet.Next() as IFeature;
                                        if (fe == null)
                                        {
                                            break;
                                        }
                                        IRepresentation p = rpc.GetRepresentation(fe, mctx);
                                        if (p.RuleID == -1)//本身是自由制图表达，就需要将自由制图表达的要素进行打断
                                        {//p.Shape = fe.ShapeCopy;
                                            p.Shape = fe.Shape;
                                            p.UpdateFeature();
                                            fe.Store();
                                        }
                                        else
                                        {
                                            if (count != 2)
                                            {
                                                bool over = p.HasShapeOverride;
                                                if (over) { ModifyOverride(fe, p, ref tempShape2); }
                                            }
                                            else
                                            {
                                                bool over = p.HasShapeOverride; if (over)
                                                {
                                                    p.RemoveShapeOverride();
                                                    p.Shape = tempShape2;
                                                    p.UpdateFeature();
                                                    p.Feature.Store();
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("打断失败");
                                System.Diagnostics.Trace.WriteLine(ex.Message + "1-1");
                                editor.AbortOperation();
                            }
                        }
                        else
                        {
                            try
                            {

                                ISet pFeatureSet = pFeatureEdit1.Split(splitPt1);
                                if (pFeatureSet != null)
                                {
                                    pFeatureSet.Reset();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("打断失败");
                                System.Diagnostics.Trace.WriteLine(ex.Message + "1-2");
                                editor.AbortOperation();
                            }
                        }
                        //合并因线打断形成的两个要素为一个要素                        
                        IFeature fea1 = fc.GetFeature(oid[0]);
                        IFeature fea2 = fc.GetFeature(oid[1]);
                        IFeature objectfea = null;
                        try
                        {

                            if (fea1.Shape.GeometryType == fea2.Shape.GeometryType && fea2.Shape.GeometryType != esriGeometryType.esriGeometryPoint)
                            {
                                if ((fea1.Shape as IPolyline).FromPoint.Compare(splitPt1) == 0)
                                {
                                    IPolyline pl = new PolylineClass();
                                    IPointCollection ptc = (pl) as IPointCollection;
                                    Polyline pp = fea1.Shape as Polyline;
                                    IPointCollection ptcoll = (pp) as IPointCollection;
                                    Polyline pp2 = fea2.Shape as Polyline;
                                    IPointCollection ptcoll2 = (pp2) as IPointCollection;
                                    ptc.AddPointCollection(ptcoll);
                                    ptc.AddPointCollection(ptcoll2);
                                    //IGeometry geo = fea1.ShapeCopy;
                                    (pl as ITopologicalOperator).Simplify();
                                    //(fea2.ShapeCopy as ITopologicalOperator).Simplify();
                                    //ITopologicalOperator feaTopo = geo as ITopologicalOperator;

                                    //geo = feaTopo.Union(fea2.ShapeCopy);
                                    //feaTopo.Simplify();
                                    fea1.Shape = pl;
                                    fea1.Store();
                                    fea2.Delete();
                                    objectfea = fea1;
                                }
                                else
                                {
                                    IPolyline pl = new PolylineClass();
                                    IPointCollection ptc = (pl) as IPointCollection;
                                    Polyline pp = fea1.Shape as Polyline;
                                    IPointCollection ptcoll = (pp) as IPointCollection;
                                    Polyline pp2 = fea2.Shape as Polyline;
                                    IPointCollection ptcoll2 = (pp2) as IPointCollection;
                                    ptc.AddPointCollection(ptcoll2);
                                    ptc.AddPointCollection(ptcoll);
                                    //IGeometry geo = fea1.ShapeCopy;
                                    (pl as ITopologicalOperator).Simplify();
                                    //(fea2.ShapeCopy as ITopologicalOperator).Simplify();
                                    //ITopologicalOperator feaTopo = geo as ITopologicalOperator;

                                    //geo = feaTopo.Union(fea2.ShapeCopy);
                                    //feaTopo.Simplify();
                                    fea2.Shape = pl;
                                    fea2.Store();
                                    fea1.Delete();
                                    objectfea = fea2;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("打断失败");
                            System.Diagnostics.Trace.WriteLine(ex.Message + "1-3");
                            editor.AbortOperation();
                        }

                        # region//从第二个交点开始线打断
                        List<int> featurelist = new List<int>();
                        //   for (int i = 1; i < geoCol.GeometryCount; i++)
                        {
                            if (geoCol.GeometryCount < 2)
                            {
                                pFeature = pCursor.NextFeature();
                                continue;
                            }
                            IPoint splitPt = new PointClass();
                            splitPt = geoCol.get_Geometry(1) as IPoint;
                            IFeatureEdit pFeatureEdit = (IFeatureEdit)objectfea;
                            if (geoFealyr.Renderer is IRepresentationRenderer)
                            {
                                IMapContext mctx = new MapContextClass();
                                mctx.Init((fc as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFealyr.AreaOfInterest);
                                var rpc = (geoFealyr.Renderer as IRepresentationRenderer).RepresentationClass;
                                try
                                {
                                    ISet pFeatureSet = pFeatureEdit.Split(splitPt);
                                    if (pFeatureSet != null)
                                    {
                                        pFeatureSet.Reset();
                                        int count = 0;
                                        IPolyline tempShape2 = null;
                                        while (true)
                                        {
                                            IFeature fe = pFeatureSet.Next() as IFeature;
                                            if (fe == null)
                                            {
                                                break;
                                            }
                                            featurelist.Add(fe.OID);
                                            var p = rpc.GetRepresentation(fe, mctx);
                                            if (p.RuleID == -1)//本身是自由制图表达，就需要将自由制图表达的要素进行打断
                                            {//p.Shape = fe.ShapeCopy;
                                                p.Shape = fe.Shape;
                                                p.UpdateFeature();
                                                fe.Store();
                                            }
                                            else
                                            {
                                                if (count != 2)
                                                {
                                                    bool over = p.HasShapeOverride;
                                                    if (over) { ModifyOverride(fe, p, ref tempShape2); }
                                                }
                                                else
                                                {
                                                    bool over = p.HasShapeOverride; if (over)
                                                    {
                                                        p.RemoveShapeOverride();
                                                        p.Shape = tempShape2;
                                                        p.UpdateFeature();
                                                        p.Feature.Store();
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("打断失败");
                                    System.Diagnostics.Trace.WriteLine(ex.Message + "2-1");
                                    editor.AbortOperation();
                                }
                            }
                            else
                            {
                                try
                                {

                                    ISet pFeatureSet = pFeatureEdit.Split(splitPt);
                                    if (pFeatureSet != null)
                                    {
                                        pFeatureSet.Reset();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("打断失败");
                                    System.Diagnostics.Trace.WriteLine(ex.Message + "2-2");
                                    editor.AbortOperation();
                                }
                            }
                        }
                        # endregion
                        # region//从第三个交点开始线打断

                        //    List<int> splitfeaturelist = new List<int>();
                        for (int i = 2; i < geoCol.GeometryCount; i++)
                        {
                            IPoint splitPt = new PointClass();
                            splitPt = geoCol.get_Geometry(i) as IPoint;
                            IFeatureEdit pFeatureEdit = null;
                            if (featurelist.Count != 0)
                            {
                                for (int j = 0; j < featurelist.Count; j++)
                                {
                                    IFeature featemp = fc.GetFeature(featurelist[j]);
                                    //   if (splitfeaturelist.Contains(featurelist[j])) { continue; }//已经分割过一次的跳过
                                    IPolyline polylinetemp = featemp.Shape as IPolyline;
                                    IRelationalOperator relationalOperator = polylinetemp as IRelationalOperator;//好接口
                                    if (relationalOperator.Contains(splitPt))
                                    {
                                        pFeatureEdit = (IFeatureEdit)featemp;
                                        featurelist.Remove(featurelist[j]);
                                        //    splitfeaturelist.Add(featemp.OID);
                                        break;
                                    }

                                }
                            }
                            else
                            {
                                pFeatureEdit = (IFeatureEdit)pFeature; //splitfeaturelist.Add(pFeature.OID); 
                            }

                            if (geoFealyr.Renderer is IRepresentationRenderer)
                            {
                                IMapContext mctx = new MapContextClass();
                                mctx.Init((fc as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFealyr.AreaOfInterest);
                                var rpc = (geoFealyr.Renderer as IRepresentationRenderer).RepresentationClass;
                                try
                                {
                                    ISet pFeatureSet = pFeatureEdit.Split(splitPt);
                                    if (pFeatureSet != null)
                                    {
                                        pFeatureSet.Reset();
                                        int count = 0;
                                        IPolyline tempShape2 = null;
                                        while (true)
                                        {
                                            IFeature fe = pFeatureSet.Next() as IFeature;
                                            if (fe == null)
                                            {
                                                break;
                                            }
                                            featurelist.Add(fe.OID);
                                            var p = rpc.GetRepresentation(fe, mctx);
                                            if (p.RuleID == -1)//本身是自由制图表达，就需要将自由制图表达的要素进行打断
                                            {//p.Shape = fe.ShapeCopy;
                                                p.Shape = fe.Shape;
                                                p.UpdateFeature();
                                                fe.Store();
                                            }
                                            else
                                            {
                                                if (count != 2)
                                                {
                                                    bool over = p.HasShapeOverride;
                                                    if (over) { ModifyOverride(fe, p, ref tempShape2); }
                                                }
                                                else
                                                {
                                                    bool over = p.HasShapeOverride; if (over)
                                                    {
                                                        p.RemoveShapeOverride();
                                                        p.Shape = tempShape2;
                                                        p.UpdateFeature();
                                                        p.Feature.Store();
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("打断失败");
                                    System.Diagnostics.Trace.WriteLine(ex.Message + "3-1");
                                    editor.AbortOperation();
                                }
                            }
                            else
                            {
                                try
                                {

                                    ISet pFeatureSet = pFeatureEdit.Split(splitPt);
                                    if (pFeatureSet != null)
                                    {
                                        pFeatureSet.Reset();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("打断失败");
                                    System.Diagnostics.Trace.WriteLine(ex.Message + "3-2");
                                    editor.AbortOperation();
                                }
                            }
                        }
                        # endregion
                    }
                    #endregion
                    # region
                    else//线不闭合
                    {
                        List<int> featurelist = new List<int>();
                        //    List<int> splitfeaturelist = new List<int>();
                        for (int i = 0; i < geoCol.GeometryCount; i++)
                        {
                            IPoint splitPt = new PointClass();
                            splitPt = geoCol.get_Geometry(i) as IPoint;
                            IFeatureEdit pFeatureEdit = null;
                            if (featurelist.Count != 0)
                            {
                                for (int j = 0; j < featurelist.Count; j++)
                                {
                                    IFeature featemp = fc.GetFeature(featurelist[j]);
                                    //   if (splitfeaturelist.Contains(featurelist[j])) { continue; }//已经分割过一次的跳过
                                    IPolyline polylinetemp = featemp.Shape as IPolyline;
                                    IRelationalOperator relationalOperator = polylinetemp as IRelationalOperator;//好接口
                                    if (relationalOperator.Contains(splitPt))
                                    {
                                        if ((featemp.ShapeCopy as IPolycurve).Length == 0) { continue; }
                                        pFeatureEdit = (IFeatureEdit)featemp;
                                        featurelist.Remove(featurelist[j]);
                                        //    splitfeaturelist.Add(featemp.OID);
                                        break;
                                    }

                                }
                            }
                            else
                            {
                                if ((pFeature.ShapeCopy as IPolycurve).Length == 0) { continue; }
                                pFeatureEdit = (IFeatureEdit)pFeature; //splitfeaturelist.Add(pFeature.OID); 

                            }

                            if (geoFealyr.Renderer is IRepresentationRenderer)
                            {
                                IMapContext mctx = new MapContextClass();
                                mctx.Init((fc as IGeoDataset).SpatialReference, m_Application.Workspace.Map.ReferenceScale, geoFealyr.AreaOfInterest);
                                var rpc = (geoFealyr.Renderer as IRepresentationRenderer).RepresentationClass;
                                try
                                {

                                    ISet pFeatureSet = pFeatureEdit.Split(splitPt);
                                    if (pFeatureSet != null)
                                    {
                                        pFeatureSet.Reset();
                                        int count = 0;
                                        IPolyline tempShape2 = null;
                                        while (true)
                                        {
                                            count++;
                                            IFeature fe = pFeatureSet.Next() as IFeature;
                                            if (fe == null)
                                            {
                                                break;
                                            }
                                            featurelist.Add(fe.OID);
                                            IRepresentation p = rpc.GetRepresentation(fe, mctx);
                                            if (p.RuleID == -1)//本身是自由制图表达，就需要将自由制图表达的要素进行打断
                                            {//p.Shape = fe.ShapeCopy;
                                                p.Shape = fe.Shape;
                                                p.UpdateFeature();
                                                fe.Store();
                                            }
                                            else
                                            {
                                                if (count != 2)
                                                {
                                                    bool over = p.HasShapeOverride;
                                                    if (over) { ModifyOverride(fe, p, ref tempShape2); }
                                                }
                                                else
                                                {
                                                    bool over = p.HasShapeOverride; if (over)
                                                    {
                                                        p.RemoveShapeOverride();
                                                        p.Shape = tempShape2;
                                                        p.UpdateFeature();
                                                        p.Feature.Store();
                                                    }
                                                }
                                                //是常规的制图表达，就不需要单独将制图表达的要素进行打断
                                            }
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Trace.WriteLine(ex.Message + "4-1");
                                    MessageBox.Show("打断失败");
                                    editor.AbortOperation();
                                }
                            }
                            else
                            {
                                try
                                {

                                    ISet pFeatureSet = pFeatureEdit.Split(splitPt);
                                    if (pFeatureSet != null)
                                    {
                                        pFeatureSet.Reset();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Trace.WriteLine(ex.Message + "4-2");
                                    MessageBox.Show("打断失败");
                                    editor.AbortOperation();
                                }
                            }
                        }
                    }
                    # endregion
                    pFeature = pCursor.NextFeature();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
            }
            editor.StopOperation("同步线打断");
            m_Application.MapControl.Refresh();
        }
        /// <summary>
        /// 制图表达shape修复（删除override）
        /// </summary>
        /// <param name="fe"></param>
        /// <param name="rep"></param>
        public void ModifyOverride(IFeature fe, IRepresentation rep, ref IPolyline tempShape2)
        {
            IPolyline Overridepolyline = rep.ShapeCopy as IPolyline;
            IPolyline fepolyline = fe.ShapeCopy as IPolyline;
            IPolyline tempShape = (rep.ShapeCopy as ITopologicalOperator).Intersect(fepolyline, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            tempShape2 = (rep.ShapeCopy as ITopologicalOperator).Difference(fepolyline) as IPolyline;
            IRelationalOperator relationalOperator = tempShape as IRelationalOperator;//好接口
            if (relationalOperator.Equals(fepolyline))
            {
                rep.RemoveShapeOverride();
                rep.Shape = tempShape;
                rep.UpdateFeature();
                rep.Feature.Store();
            }
            //else
            //{

            //}
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
                return m_Application != null
                    && m_Application.Workspace != null
                    && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing
                    && m_Application.LayoutState == Common.LayoutState.MapControl;
            }
        }
    }
}


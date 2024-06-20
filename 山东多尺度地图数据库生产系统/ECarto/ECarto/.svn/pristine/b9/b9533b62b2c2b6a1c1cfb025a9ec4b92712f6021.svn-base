using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.GeneralEdit
{
    /// <summary>
    /// 修面，对选中的单个面要素进行塑形
    /// </summary>
    public class ReShapePolygon : SMGI.Common.SMGITool
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

        public ReShapePolygon()
        {
            m_caption = "修面";     
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
                        if (feature.Shape is IPolygon)
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

            //获取当前选择的要素
            IEnumFeature pEnumFeature = (IEnumFeature)m_Application.MapControl.Map.FeatureSelection;
            ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
            IFeature feature = pEnumFeature.Next();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumFeature);

            editor.StartOperation();
            try
            {
                IPolygon pg = feature.ShapeCopy as IPolygon;
                IGeometryCollection pgCol = pg as IGeometryCollection;
                for (int i = 0; i < pgCol.GeometryCount; i++)
                {
                    IRing r = pgCol.get_Geometry(i) as IRing;
                    try
                    {
                        r.Reshape(reshapePath as IPath);
                    }
                    catch (Exception)
                    {

                        continue;
                    }
                }
                (pg as ITopologicalOperator).Simplify();
                feature.Shape = pg;
                feature.Store();

                editor.StopOperation("修面");

                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);

                ClearSnapperCache();
            }
            catch (Exception ex)
            {
                editor.AbortOperation();
                System.Diagnostics.Trace.WriteLine(ex.Message, "Edit Geometry Failed");
            }

            
        }

        public override bool Deactivate()
        {
            //卸掉该事件
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            return base.Deactivate();
        }
    }
}

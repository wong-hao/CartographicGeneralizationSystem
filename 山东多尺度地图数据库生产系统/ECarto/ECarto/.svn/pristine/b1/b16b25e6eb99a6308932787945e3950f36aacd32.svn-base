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
    public class LineCutCopy : SMGITool
    {
        private IEngineEditor editor;
        private ISimpleLineSymbol lineSymbol;
        private INewLineFeedback lineFeedback;
        FeatureCopyTransToolForm frm;
        List<IPoint> btndown = null;
        IPoint position = null;
        IPolyline polyline = null;
        IFeatureClass feac = null;
        IFeatureWorkspace featureWorkspace;
        double offset = 0;
        int gb = 0;
        int ruleid = 0;
        public LineCutCopy()
        {
            m_caption = "拷贝复线";
        }
        public override void Refresh(int hdc)
        {
        }
        public override void OnClick()
        {

            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;
            frm = new FeatureCopyTransToolForm();
            if (frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }


            btndown = new List<IPoint>();
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
            if (button == 1 && shift == 1)//左键加shift键
            {
                position = new PointClass();
                position = ToSnapedMapPoint(x, y);
                process();
            }
            else if (button == 1)
            {
                if (lineFeedback == null)
                {
                    var dis = m_Application.ActiveView.ScreenDisplay;
                    lineFeedback = new NewLineFeedbackClass { Display = dis, Symbol = lineSymbol as ISymbol };
                    lineFeedback.Start(ToSnapedMapPoint(x, y));
                    btndown.Add(ToSnapedMapPoint(x, y));
                }
                else
                {
                    lineFeedback.AddPoint(ToSnapedMapPoint(x, y));
                }
            }
        }
        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (lineFeedback != null)
            {
                lineFeedback.MoveTo(ToSnapedMapPoint(x, y));
            }

        }
        public IPointCollection cuPoint(IPoint pt, IPolyline polyline)
        {
            IPointCollection ptcol = new PathClass();
            ICurve ICurve = getline(pt, pt, polyline);
            if (ICurve == null) { return ptcol; }
            IPointCollection pycol = ICurve as IPointCollection;
            ITopologicalOperator topo = btndown[0] as ITopologicalOperator;
            if (topo.Equals(pycol.get_Point(0)))
            {
                for (int i = 0; i < pycol.PointCount; i++)
                {
                    IPoint temp = pycol.get_Point(i);
                    ptcol.AddPoint(temp);
                }
            }
            else
            {
                for (int i = pycol.PointCount - 1; i > -1; i--)
                {
                    IPoint temp = pycol.get_Point(i);
                    ptcol.AddPoint(temp);
                }
            }

            return ptcol;
        }
        public ICurve getline(IPoint pts, IPoint pt, IPolyline polyline)
        {
            ICurve ICurve = null;
            //  if (btndown.Count == 0) { return ICurve; }
            IPoint ptstart = pts;
            IPoint boulp = new PointClass();
            double distancealongsart = 0;
            double distancealong = 0; double distanceForm = 0; bool right = false;
            polyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, pt, false, boulp, ref distancealong, ref distanceForm, ref right);

            polyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, ptstart, false, boulp, ref distancealongsart, ref distanceForm, ref right);

            if (distancealongsart > distancealong)
            { polyline.GetSubcurve(distancealong, distancealongsart, false, out  ICurve); }
            else if (distancealongsart == distancealong) { polyline.GetSubcurve(distancealongsart, distancealong + 0.1, false, out  ICurve); }
            else { polyline.GetSubcurve(distancealongsart, distancealong, false, out  ICurve); }

            return ICurve;
        }
        public void process()
        {
            if (polyline.IsEmpty)
                return;
            editor.StartOperation();

            try
            {
                IFeatureClass feaFC = featureWorkspace.OpenFeatureClass(frm.OffsetDis.ToUpper());
                if (feaFC == null) { MessageBox.Show("目标图层不存在"); return; }
                ITopologicalOperator2 pTopo = (ITopologicalOperator2)polyline;
                pTopo.IsKnownSimple_2 = false;
                pTopo.Simplify();

                IEngineEditLayers editLayer = m_Application.EngineEditor as IEngineEditLayers;

                IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                mapEnumFeature.Reset();
                IFeature pFeature;

                while ((pFeature = mapEnumFeature.Next()) != null)
                {
                    if (feaFC.ShapeType != (pFeature.Class as IFeatureClass).ShapeType) { MessageBox.Show("目标图层与拷贝要素几何类型不一致"); return; }
                    IPoint m_BasePoint = null;
                    if (pFeature.Shape is IPoint) { m_BasePoint = pFeature.ShapeCopy as IPoint; }
                    else { m_BasePoint = ((IArea)pFeature.Shape.Envelope).Centroid; }
                    ILine line = new LineClass();
                    line.PutCoords(m_BasePoint, position);
                    //  (line as ITopologicalOperator).Simplify();
                    ICurve curve = getline(polyline.FromPoint, polyline.ToPoint, pFeature.ShapeCopy as IPolyline);
                    IFeature feaNew = feaFC.CreateFeature();
                    ITransform2D pTrans2D;
                    pTrans2D = (ITransform2D)curve;
                    pTrans2D.MoveVector(line);
                    feaNew.Shape = pTrans2D as IGeometry;
                    feaNew.Store();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                editor.AbortOperation();
            } lineFeedback = null;
            editor.StopOperation("拷贝复线");
            m_Application.MapControl.Map.ClearSelection();
            m_Application.MapControl.Refresh();
        }
        public override void OnDblClick()
        {
            polyline = lineFeedback.Stop();

            //双击完毕进行线条的合并           
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Xml;
using System.Xml.Linq;
using SMGI.Common.Algrithm;
namespace SMGI.Plugin.MapGeneralization
{
    //化简
    public class TopolgyEdgeSimpifyCmd: SMGI.Common.SMGICommand
    {
      
        List<string> _ruleNames = new List<string>();

        public TopolgyEdgeSimpifyCmd()
        {
            m_caption = "拓扑边化简 cmd方式";
            m_toolTip = "拓扑边化简 cmd方式";
            m_category = "拓扑";

        }
        public override bool Enabled
        {
            get
            {
                
                return TopologyApplication.SelPolylines != null && m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        double SMHeigth = 0;
        double SMWidth = 0;
        ITopology topgy = null;
        public override void OnClick()
        {
            #region
            var dlg = new FrmSimplify(SMHeigth, SMWidth);
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            bool res = true;

            SMHeigth = dlg.heigth;
            SMWidth = dlg.width;
            var pEditor = m_Application.EngineEditor;
            var wo = m_Application.SetBusy();
            List<IGeometry> errorsDic = new List<IGeometry>();
            try
            {
                pEditor.StartOperation();
                {

                    IEnvelope penvelope = null;
                    var edges = TopologyApplication.SelEdges;
                    string msg = "正在化简...";
                    int flag = 1;
                    TopologyHelper helper = new TopologyHelper(m_Application.ActiveView);
                    var topgy = TopologyApplication.Topology;
                    ITopologyGraph graph = helper.CreateTopGraph(topgy);
                    
                    foreach (var edge in edges)
                    {
                        #region
                        try
                        {
                            msg = "正在化简..." + (flag++).ToString() + "/" + edges.Count;
                            wo.SetText(msg);
                            IPolyline line = (edge.Geometry as IClone).Clone() as IPolyline;
                            //第一根线
                            ICurve curve1 = null;
                            line.GetSubcurve(0, 0.5, true, out curve1);
                            IPointCollection path0 = new PathClass();
                            path0.AddPointCollection(curve1 as IPointCollection);
                            IGeometryCollection polyline1 = new PolylineClass();
                            polyline1.AddGeometry(path0 as IGeometry);
                            var pl1 = SimplifyByDTAlgorithm.SimplifyByDT(polyline1 as IPolycurve, SMHeigth, SMWidth);
                            // DrawSelePolyline(pl1 as IPolyline, new RgbColorClass() { Red=255,Blue=0,Green=0});

                            //第二根线
                            ICurve curve2 = null;
                            line.GetSubcurve(0.5, 1, true, out curve2);
                            IPointCollection path1 = new PathClass();
                            path1.AddPointCollection(curve2 as IPointCollection);
                            IGeometryCollection polyline2 = new PolylineClass();
                            polyline2.AddGeometry(path1 as IGeometry);
                            var pl2 = SimplifyByDTAlgorithm.SimplifyByDT(polyline2 as IPolycurve, SMHeigth, SMWidth);

                            // DrawSelePolyline(pl2 as IPolyline, new RgbColorClass() { Red = 0, Blue = 255, Green = 0 });


                            //合并两根线
                            IPointCollection pl = new PolylineClass();
                            pl.AddPointCollection(pl1 as IPointCollection);
                            pl.AddPointCollection(pl2 as IPointCollection);
                            

                            //对合并的线进行自相交检查
                            if (helper.IsSelfCross(pl as IGeometry))
                            {
                                continue;
                            }
                            //若不存在自相交，则进行拓扑简化
                            (pl as ITopologicalOperator).Simplify();

                            //ITopologicalOperator pTopologicalOperator = pl1 as ITopologicalOperator;
                            //IGeometry intersection = pTopologicalOperator.Intersect(pl2, esriGeometryDimension.esriGeometry0Dimension);
                            //if (!intersection.IsEmpty)
                            //{
                            //    IPointCollection pc = intersection as IPointCollection;
                            //    if (pc.PointCount >= 2)
                            //        continue;
                            //}

                            //若化简后直线只存在两点，则直接跳过该种情况
                            if (pl.PointCount == 2)
                            {
                                continue;
                            }
                            //draw line
                            //DrawSelePolyline(pl as IPolyline, new RgbColorClass() { Red = 0, Blue = 0, Green = 255 });
                            IPointCollection reshapePath = new PathClass();
                            reshapePath.AddPointCollection(pl as IPointCollection);
                            var path = reshapePath as IPath;
                            path.Smooth(0.1);
                            SetZValue(line, path);
                            graph.SetEdgeGeometry(edge, path);
                            //graph.ReshapeEdgeGeometry(edge, path);

                            GC.Collect();
                        }
                        catch (Exception ex)
                        {
                            var geo = edge.Geometry;
                            if (!errorsDic.Contains(geo))
                            {

                                errorsDic.Add(geo);
                            }
                           
                            pEditor.AbortOperation();
                            pEditor.StartOperation();
                            continue;
                        }

                        #endregion


                    }
                    wo.SetText("提交拓扑化简结果...");
                    graph.Post(out penvelope);
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, penvelope);

                    wo.Dispose();
                    pEditor.StopOperation("拓扑化简");

                    if (errorsDic.Count > 0)
                    {
                        var frm = new Top.FrmTopErrors(errorsDic);
                        frm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                wo.Dispose();
                pEditor.AbortOperation();
                res = false;
            }
            MessageBox.Show("拓扑化简完成！");

            //if (res)
            //{
            //    MessageBox.Show("拓扑化简完成！");
            //}
            //else
            //{
            //    MessageBox.Show("拓扑化简失败！");
            //}
            #endregion
        }
        private void DrawSelePolyline(IPolyline pl)
        {
            IElement element = null;
            IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            gc.Reset();
            IElement ele = null;
            while ((ele = gc.Next()) != null)
            {
                IElementProperties ep1 = ele as IElementProperties;
                if (ep1.Name == "TraceSelLine")
                    m_Application.ActiveView.GraphicsContainer.DeleteElement(ele);
            }
            ILineElement lineElement = null;
            ISimpleLineSymbol lineSymbol = null;
            IElementProperties ep = null;
            //绘制选中的线
          // if (selPolyline != null)
            {
                lineElement = new LineElementClass();
                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                lineSymbol.Color = new RgbColorClass { Red = 0 ,Blue=0,Green=0};
                lineSymbol.Width = 20;
                lineElement.Symbol = lineSymbol;
                //
                element = lineElement as IElement;
                element.Geometry = pl;
                ep = element as IElementProperties;
                ep.Name = "TraceSelLine";
                gc.AddElement(element, 0);
            }
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
        }

        private void DrawSelePolyline(IPolyline pl,RgbColorClass color)
        {
            IElement element = null;
            IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            gc.Reset();
            //IElement ele = null;
            //while ((ele = gc.Next()) != null)
            //{
            //    IElementProperties ep1 = ele as IElementProperties;
            //    if (ep1.Name == "TraceSelLine")
            //        m_Application.ActiveView.GraphicsContainer.DeleteElement(ele);
            //}
            ILineElement lineElement = null;
            ISimpleLineSymbol lineSymbol = null;
            IElementProperties ep = null;
            //绘制选中的线
            // if (selPolyline != null)
            {
                lineElement = new LineElementClass();
                lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                lineSymbol.Color = color;
                lineSymbol.Width = 10;
                lineElement.Symbol = lineSymbol;
                //
                element = lineElement as IElement;
                element.Geometry = pl;
                ep = element as IElementProperties;
                ep.Name = "TraceSelLine";
                gc.AddElement(element, 0);
            }
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
        }
        public  void SetZValue(IGeometry geoSource, IGeometry pGeo)
        {
            IZAware pZAware0 = (IZAware)geoSource;
            if (pZAware0.ZAware)
            {
                IZAware pZAware = (IZAware)pGeo;
                pZAware.ZAware = true;
                IPointCollection pc = pGeo as IPointCollection;
                for (int i = 0; i < pc.PointCount; i++)
                {
                     pc.get_Point(i).Z=0;
                }

            }
            else
            {
                IZAware pZAware = (IZAware)pGeo;
                pZAware.ZAware = false;
            }

            ////M值μ
            //if (pGeometryDef.HasM)
            //{
            //    IMAware pMAware = (IMAware)pGeo;
            //    pMAware.MAware = true;
            //}
            //else
            //{
            //    IMAware pMAware = (IMAware)pGeo;
            //    pMAware.MAware = false;

            //}
        }
    }
}

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
    public class TopolgyReshapeEdge: SMGI.Common.SMGITool
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


        public TopolgyReshapeEdge()
        {
            m_caption = "拓扑选中修线";
            m_toolTip = "拓扑选中修线";
            m_category = "拓扑";
          
        }
        public override void OnClick()
        {
            editor = m_Application.EngineEditor;
            //#region Create a symbol to use for feedback
            lineSymbol = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass();	 //red
            color.Red = 0;
            color.Green = 0;
            color.Blue = 255;
            lineSymbol.Color = color;
            lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            lineSymbol.Width = 1.5;
            (lineSymbol as ISymbol).ROP2 = esriRasterOpCode.esriROPNotXOrPen;//这个属性很重要
            //#endregion
            lineFeedback = null;
            //用于解决在绘制feedback过程中进行地图平移出现线条混乱的问题
            m_Application.MapControl.OnAfterScreenDraw += new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
          
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
            ITopologyEdge edge = TopologyApplication.SelEdges[0];
            //判断是否相交
            if ((polyline as IRelationalOperator).Disjoint(edge.Geometry))
                return;


            IPointCollection reshapePath = new PathClass();
            reshapePath.AddPointCollection(polyline as IPointCollection);
            var view = m_Application.ActiveView;
            var wo = m_Application.SetBusy();
            try
            {
               
                {
                    ITopology topology = TopologyApplication.Topology;
                    TopologyHelper helper = new TopologyHelper(view);
                    wo.SetText("正在创建拓扑缓存");
                    ITopologyGraph graph = helper.CreateTopGraph(topology);

                    
                    IEnvelope penvelope;
                    editor.StartOperation();
                  
                    graph.ReshapeEdgeGeometry(edge, reshapePath as IPath);
                    graph.Post(out penvelope);
                    //重新选择
                    IPoint midpoint = new PointClass();
                    polyline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, midpoint);
                    helper.QueryTopEdge(graph, midpoint);
                    editor.StopOperation("拓扑边修线");
                    view.PartialRefresh(esriViewDrawPhase.esriViewAll, null, penvelope);
                    wo.Dispose();
                   
                }
                
            }
            catch (Exception ex)
            {
                wo.Dispose();
                editor.AbortOperation();
                System.Diagnostics.Trace.WriteLine(ex.Message, "拓扑修线失败");
            }

           //editor.StopOperation("修线");

            
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
                return TopologyApplication.SelEdges!=null&& m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState== esriEngineEditState.esriEngineStateEditing;
            }
                
             
        }
        public override void Refresh(int hdc)
        {
            if (TopologyApplication.SelEdges == null)
                return;
            if (TopologyApplication.SelPolylines == null)
                return;
             var selPolylines = TopologyApplication.SelPolylines;
         
             foreach (var selPolyline in selPolylines)
             {
                 //var selPolyline = TopologyApplication.SelEdge.Geometry;
                 if (selPolyline != null)
                 {
                     RgbColorClass c = new RgbColorClass();
                     IDisplay dis = new SimpleDisplayClass();
                     dis.DisplayTransformation = m_Application.ActiveView.ScreenDisplay.DisplayTransformation;
                     dis.DisplayTransformation.ReferenceScale = 0;
                     dis.StartDrawing(hdc, -1);
                     SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
                     c.Red = 250;
                     c.Blue = 0;
                     c.Green = 0;
                     sls.Color = c;
                     sls.Width = 2;

                     dis.SetSymbol(sls as ISymbol);
                     dis.DrawPolyline(selPolyline);
                     dis.FinishDrawing();
                 }
             }

        }

       
        
    }
}

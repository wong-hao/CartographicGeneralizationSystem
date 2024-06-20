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


namespace SMGI.Plugin.MapGeneralization
{
    //拓扑边选中
    public class TopolgyEdgeSelect:SMGITool
    {
 
        public TopolgyEdgeSelect()
        {
            m_caption = "拓扑边选择";
            m_toolTip = "拓扑边选择";
            m_category = "拓扑";

        }
        public override bool Enabled
        {
            get
            {
                return TopologyApplication.Topology!=null&& m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState== esriEngineEditState.esriEngineStateEditing;
            }
        }
        double SMHeigth = 0;
        double SMWidth = 0;
        ITopology topgy = null;
        public override void OnClick()
        {
            
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }
            topgy = TopologyApplication.Topology;
            var view = m_Application.ActiveView;
            IMap map = view.FocusMap;
            var editor = m_Application.EngineEditor;

            //画范围
            IRubberBand pRubberBand = new RubberRectangularPolygonClass();
            var geo = pRubberBand.TrackNew(view.ScreenDisplay, null);

            if (geo==null||geo.IsEmpty)
            {
                return;
            }


            var wo = m_Application.SetBusy();
            try
            {


                TopologyHelper helper = new TopologyHelper(view);
                wo.SetText("正在创建拓扑缓存");
                ITopologyGraph graph = helper.CreateTopGraph(topgy);
                var geoEdge = helper.QueryTopEdges(graph, geo.Envelope);
                TopologyApplication.SelPolylines = geoEdge;
                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                wo.Dispose();
            }
            catch
            {
                wo.Dispose();
            }
            view.Refresh();
        }
        public override bool Deactivate()
        {
            //TopologyApplication.SelEdge = null;
            //TopologyApplication.SelPolyline = null;
            return base.Deactivate();
          
        }
        public override void Refresh(int hdc)
        {
            if (TopologyApplication.SelEdges == null)
                return;
            if (TopologyApplication.SelPolylines == null)
                return;
            var selPolylines = TopologyApplication.SelPolylines;
            foreach(var selPolyline in selPolylines)
            {
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

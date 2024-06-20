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
namespace SMGI.Plugin.MapGeneralization
{
    //化简
    public class TopolgySimplifyTool : SMGI.Common.SMGITool
    {
        double _ratio=0;
        ILayer _layer = null;
        List<string> _ruleNames = new List<string>();

        public TopolgySimplifyTool()
        {
            m_caption = "拓扑化简";
            m_toolTip = "拓扑化简";
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
            var dlg = new FrmSimplify();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SMHeigth = dlg.heigth;
                SMWidth = dlg.width;
                
            }
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
          
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
            

            using (var wo =m_Application.SetBusy())
            {


                TopologyHelper helper = new TopologyHelper(view);
                wo.SetText("正在创建拓扑缓存");
                ITopologyGraph graph = helper.CreateTopGraph(topgy);
                //(view as IGraphicsContainer).DeleteAllElements();
                wo.SetText("正在化简拓扑边");
                helper.QueryTopEle(graph, geo.Envelope, SMHeigth, SMWidth);
                
            }
          
            view.Refresh();
        }
       
        public override void OnKeyUp(int keyCode, int shift)
        {
            switch (keyCode)
            {
                case 32:
                        var dlg = new FrmSimplify();
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            SMHeigth = dlg.heigth;
                            SMWidth = dlg.width;
                
                        }
                    break;
                default:
                    break;
            }
        }
        public override void Refresh(int hdc)
        {
            return;
            var selPolyline = TopologyApplication.SelEdge.Geometry as IPolyline;
         
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using SMGI.Common.Algrithm;
using DatabaseUpdate;
namespace SMGI.Plugin.MapGeneralization
{
    public class LineConflictCheckCmd : SMGI.Common.SMGICommand
    {
        bool bFirstRun = false;
        List<IPolyline> errorPolylines = new List<IPolyline>();
        int currentErrIndex;
        public LineConflictCheckCmd()
        {
             
            m_caption = "水路压盖处理";
            m_category = "水路压盖处理";
            m_toolTip = "水路压盖处理";
        }
        private IWorkspace pworkspace;

        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }
        public override void OnClick()
        {
            IActiveView pActiveView = m_Application.ActiveView;
            if (errorPolylines.Count > 0)
            {
                if (MessageBox.Show("已经检查过交汇错误，是否重新检查？", "提示",
                    MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                {
                    pActiveView.Refresh();
                    return;
                }
                else
                {
                    errorPolylines.Clear();
                }
            }

            double disValue = 10;
            FrmLineConficlt frmDistance = new FrmLineConficlt();
            IFeatureLayer roadLayer = null;
            IFeatureLayer riverLayer = null;
          
            FrmLineConficlt lineConficlt = new FrmLineConficlt();
            if (lineConficlt.ShowDialog() == DialogResult.OK)
            {

                //disValue = frmDistance.m_disValue;
                disValue = lineConficlt.m_disValue;
                riverLayer = lineConficlt.hydlLayer as IFeatureLayer;
                roadLayer = lineConficlt.lrdlLayer as IFeatureLayer;
            }
            else
            {
                return;
            }
            if (roadLayer == null || riverLayer == null)
            {
                System.Windows.Forms.MessageBox.Show("没有找到需要检查的图层");
                return;
            }
            
            var activeView = pActiveView;

            using (WaitOperation wo = m_Application.SetBusy())
            {

                wo.SetText("正在进行分析准备……");

                IMap pMap = activeView as IMap;

                List<IFeatureClass> fcList = new List<IFeatureClass>();
                IFeatureClass raodFC = (roadLayer as IFeatureLayer).FeatureClass;
                IFeatureClass riverFC = (riverLayer as IFeatureLayer).FeatureClass;
                fcList.Add(raodFC);
                fcList.Add(riverFC);

                wo.SetText("正在进行探测分析……");
                List<LineMatch.LineConflictInfo> conflictInfos = new List<LineMatch.LineConflictInfo>();
                conflictInfos = LineMatch.DetectConflict(fcList, activeView.FullExtent, disValue);

                wo.SetText("正在整理分析结果……");
                foreach (var c in conflictInfos)
                {
                    PolylineClass line = new PolylineClass();
                    line.AddGeometry(c.ConflictPartA);
                    line.AddGeometry(c.ConflictPartB);
                    errorPolylines.Add(line);
                }

                int currentErrIndex = errorPolylines.Count;
                wo.Dispose();
                MessageBox.Show(string.Format("共找到{0}对冲突线段！", (errorPolylines.Count).ToString()));
                bFirstRun = true;
                DrawOnRefresh();

            }



        }
        private  void DrawOnRefresh()
        {
           


            SimpleFillSymbolClass sfs = new SimpleFillSymbolClass();
            SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 255;
            rgb.Green = 0;
            rgb.Blue = 0;
            sls.Color = rgb;
            sls.Width = 3;
            sfs.Outline = sls;
            sfs.Style = esriSimpleFillStyle.esriSFSHollow;

            if (errorPolylines.Count == 0)
                return;
            
            IGraphicsContainer gc = m_Application.ActiveView as IGraphicsContainer;
            gc.DeleteAllElements();
            for (int i = 0; i < errorPolylines.Count; i++)
            {
                IEnvelope env = errorPolylines[i].Envelope;
                env.Expand(50, 50, false);
                if (!(errorPolylines[i] as ITopologicalOperator).IsSimple)
                {
                    rgb.Red = 0; rgb.Green = 255;
                    DrawPolygon(env as IGeometry, rgb);
                }
                else
                {
                    DrawPolygon(env as IGeometry);
                }
                

            }
            
        }
        private void DrawPolygon(IGeometry pgeo, RgbColorClass pColor)
        {
            IActiveView ac = m_Application.ActiveView as IActiveView;
            IMap pmap = ac as IMap;

            IGraphicsContainer gc = pmap as IGraphicsContainer;
            IRectangleElement pRecEle = new RectangleElementClass();
            IElement pEle = pRecEle as IElement;
            pEle.Geometry = pgeo;

 

            ILineSymbol pOutline = new SimpleLineSymbolClass();
            pOutline.Width = 2;
            pOutline.Color = pColor;

            ISimpleFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pFillSymbol.Outline = pOutline;
            pFillSymbol.Style = esriSimpleFillStyle.esriSFSNull;

            IFillShapeElement pFillShapeEle = pEle as IFillShapeElement;
            pFillShapeEle.Symbol = pFillSymbol;

            gc.AddElement((IElement)pFillShapeEle, 0);



            ac.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGraphics, null, null);
        }
        private void DrawPolygon(IGeometry pgeo)
        {   
            IActiveView ac = m_Application.ActiveView as IActiveView;
            IMap pmap = ac as IMap;
        
            IGraphicsContainer gc = pmap as IGraphicsContainer;
            IRectangleElement pRecEle = new RectangleElementClass();
            IElement pEle = pRecEle as IElement;
            pEle.Geometry = pgeo;


            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;

            ILineSymbol pOutline = new SimpleLineSymbolClass();
            pOutline.Width = 2;
            pOutline.Color = pColor;

            ISimpleFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pFillSymbol.Outline = pOutline;
            pFillSymbol.Style = esriSimpleFillStyle.esriSFSNull;

            IFillShapeElement pFillShapeEle = pEle as IFillShapeElement;
            pFillShapeEle.Symbol = pFillSymbol;

            gc.AddElement((IElement)pFillShapeEle, 0);
           
        

            ac.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGraphics, null, null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System.Diagnostics;
using SMGI.Common.Algrithm;
using SMGI.Plugin.MapGeneralization.Algrithm;
namespace SMGI.Plugin.MapGeneralization
{
    public class LineSimplifyCharactersTool:SMGI.Common.SMGITool
    {
        IFeature  feLineSimplify = null;
        IPolyline lineGeoSimplify = null;
        double width = 100;
        double height = 100;
        bool smooth = false;
        double smoothdis = 0;
        public LineSimplifyCharactersTool()
        {

            m_caption = "保持地理特征线要素化简";
            m_category = "保持地理特征线要素化简";
            m_toolTip = "保持地理特征线要素化简";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
       
       
       
        #region Overridden Class Methods


        IPointCollection characterPoints = new MultipointClass();
        private IPointCollection pPc;
        private int m_Step = 0;
        public int Step
        {
            get { return m_Step; }
            set { m_Step = value; }
        }
        private INewMultiPointFeedback m_PointsFeedback = null;
        public INewMultiPointFeedback PointsFeedback
        {
            get { return m_PointsFeedback; }
            set { m_PointsFeedback = value; }
        }
        private bool m_IsUsed = false;
        public bool IsUsed
        {
            get
            {
                return m_IsUsed;
            }
            set
            {
                m_IsUsed = value;
            }
        }
        private bool SimplifyLineFlag = false;
        //双击结束
        public override void OnDblClick()
        {
            try
            {
                if (!SimplifyLineFlag)
                {
                    return;
                }
                object objMiss = Type.Missing;
                 
                if (IsUsed == false) return;
                if (PointsFeedback == null) return;
                if (this.Step > 1)
                {
                    PointsFeedback.Stop();
                    if (pPc == null)
                    {
                        this.Step = 0;
                        this.IsUsed = false;
                        this.PointsFeedback = null;
                        return;
                    }
                   
                    this.Step = 0;
                    this.IsUsed = false;
                    this.PointsFeedback = null;
                    IMultipoint pm = pPc as IMultipoint;
                    IGeometry pPolygon = pm as IGeometry;

                    if (pPolygon.GeometryType != esriGeometryType.esriGeometryMultipoint)
                    {
                        return;
                    }
                    if (characterPoints.PointCount < 1)
                    {
                        MessageBox.Show("特征点个数为0!请先采集特征点。");
                        return;
                    }
                    SimplifyByDTCharacters.CharacterPoint = characterPoints;
                    var polycurve = SimplifyByDTCharacters.SimplifyByDT(lineGeoSimplify as IPolyline, height, width);
                    if (smooth)
                    {
                        polycurve.Smooth(smoothdis);
                    }
                    m_Application.EngineEditor.StartOperation();
                    try
                    {

                        feLineSimplify.Shape = polycurve as IGeometry;
                        feLineSimplify.Store();
                        m_Application.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, null, polycurve.Envelope);

                        m_Application.EngineEditor.StopOperation("要素化简");
                    }
                    catch
                    {
                        m_Application.EngineEditor.AbortOperation();
                    }

                }
            }
            catch (Exception ex)
            {
            }




        }
        private void CheckValide()
        {
            SimplifyLineFlag = false;
            var lineFes = new List<IFeature>();
            var map = m_Application.ActiveView.FocusMap;
            var selection = map.FeatureSelection;
            if (map.SelectionCount > 0)
            {
                lineFes = new List<IFeature>();
                IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
                selectEnumFeature.Reset();
                IFeature fe = null;
                while ((fe = selectEnumFeature.Next()) != null)
                {
                    if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        lineFes.Add(fe);
                    }
                }
                if (lineFes.Count == 0)
                {
                    MessageBox.Show("请先选择线要素");
                    return;
                }
                SimplifyLineFlag = true;
            }
            else
            {
                MessageBox.Show("请先选择线要素");
                return;
            }
        }
        public override void OnClick()
        {
            SimplifyLineFlag = false;
            var  lineFes = new List<IFeature>();
            var map = m_Application.ActiveView.FocusMap;
            var selection = map.FeatureSelection;
            if (map.SelectionCount > 0)
            {
                lineFes = new List<IFeature>();
                IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
                selectEnumFeature.Reset();
                IFeature fe = null;
                while ((fe = selectEnumFeature.Next()) != null)
                {
                    if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        lineFes.Add(fe);
                    }
                }
                if (lineFes.Count == 0)
                {
                    MessageBox.Show("请先选择线要素");
                    return;
                }
                SimplifyLineFlag = true;
            }
            else
            {
                MessageBox.Show("请先选择线要素");
                return;
            }
            feLineSimplify = lineFes[0];
            lineGeoSimplify = feLineSimplify.ShapeCopy as IPolyline;
            FrmSimplifySet frm = new FrmSimplifySet();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
            {
                SimplifyLineFlag = false;
                return;
            }
            width = frm.width;
            height = frm.heigth;
            smooth = frm.Smooth;
            smoothdis = frm.SmoothDis;

            this.Step = 0;
            this.IsUsed = false;
            this.PointsFeedback = null;
          
            m_Application.MapControl.OnAfterScreenDraw += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);

         
        }
        private void MapControl_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            if (m_PointsFeedback != null)
            {
                m_PointsFeedback.Refresh(0);
            }


        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 1)
            {
                CheckValide();
            }
            if (!SimplifyLineFlag)
            {
                return;
            }
            if (Button!=1)
            {
                return;
            }

            IPoint tpPoint = null;
            if (Button == 1)    //MouseDown左键 开始画点/连续画点  
            {

                if (this.Step <= 0)
                {
                    //开始画点  
                    //清空Graphic
                    #region
                    IGraphicsContainer pGra = m_Application.ActiveView as IGraphicsContainer;
                    IActiveView pAv = pGra as IActiveView;
                    pGra.DeleteAllElements();
                    #endregion
                    
                    tpPoint =m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                    tpPoint = GetSnapPoint(tpPoint);
                    PointsFeedback = new NewMultiPointFeedback();
                    pPc = new MultipointClass();
                    //IRgbColor pColor = new RgbColorClass();
                    //pColor.Red = 223;
                    //pColor.Green = 0;
                    //pColor.Blue = 0;

                    //ISimpleMarkerSymbol pMarkerSys = new SimpleMarkerSymbolClass();
                    //pMarkerSys.Style = esriSimpleMarkerStyle.esriSMSDiamond;
                    //pMarkerSys.Size = 10;
                    //pMarkerSys.Color = pColor;
                    //PointsFeedback.Symbol = pMarkerSys as ISymbol;
                  
                    object o = Type.Missing;

                    pPc.AddPoint(tpPoint, ref o, ref o);
                    this.PointsFeedback.Start(pPc, tpPoint);
                    this.Step += 1;
                    this.PointsFeedback.Display = m_Application.ActiveView.ScreenDisplay;
                    this.IsUsed = true;

                }
                else
                {
                    //连续画点  
                    tpPoint = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                    tpPoint = GetSnapPoint(tpPoint);
                    this.Step += 1;
                    this.PointsFeedback.Display = m_Application.ActiveView.ScreenDisplay;
                    this.PointsFeedback.MoveTo(tpPoint);
                    object o = Type.Missing;
                    pPc.AddPoint(tpPoint, ref o, ref o);
                }
                m_Application.MapControl.Refresh();
               // DrawPoint(tpPoint as IGeometry);
            }  //--  

            if (Button == 4)
            {
                IPoint tpPoint1 = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                m_Application.ActiveView.ScreenDisplay.PanStart(tpPoint1);
            }
           
               
        }
        /// <summary>
        /// 绘制点符号
        /// </summary>
        /// <param name="pPolygon"></param>
        public void DrawPoint(IGeometry pPoint)
        {
            try
            {


                IGraphicsContainer pGra = m_Application.ActiveView as IGraphicsContainer;
                IActiveView pAv = pGra as IActiveView;

                // pGra.DeleteAllElements();
                IMarkerElement pMarkerEle = new MarkerElementClass();
                IElement pEle = pMarkerEle as IElement;
                pEle.Geometry = pPoint;

                IRgbColor pColor = new RgbColorClass();
                pColor.Red = 223;
                pColor.Green = 0;
                pColor.Blue = 0;

                ISimpleMarkerSymbol pMarkerSys = new SimpleMarkerSymbolClass();
                pMarkerSys.Style = esriSimpleMarkerStyle.esriSMSCircle;
                pMarkerSys.Size = 10;
                pMarkerSys.Color = pColor;

                pMarkerEle.Symbol = pMarkerSys;

                pGra.AddElement((IElement)pMarkerEle, 0);
               
                pAv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public override void Refresh(int hdc)
        {
            if (characterPoints.PointCount>0)
            {
                IDisplay dis = new SimpleDisplayClass();
                dis.DisplayTransformation = m_Application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation;

                dis.StartDrawing(hdc, -1);
                
                RgbColorClass c = new RgbColorClass();
                SimpleMarkerSymbolClass sms = new SimpleMarkerSymbolClass();
                c.Red = 255;
                c.Blue = 0;
                c.Green = 0;

                sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
                sms.Color = c;
                dis.SetSymbol(sms);
                for (int i = 0; i < characterPoints.PointCount; i++)
                {
                    IPoint pgeo = characterPoints.get_Point(i);
                    dis.DrawPoint(pgeo);
                }
                dis.FinishDrawing();
            }
 
        }
        public void reset()
        {
            ////变量重置


            this.Step = 0;
            this.IsUsed = false;
            this.PointsFeedback = null;
            characterPoints= new MultipointClass();
            //IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            //gc.Reset();
            //gc.DeleteAllElements();
            m_Application.MapControl.ActiveView.Refresh();
            m_Application.MapControl.OnAfterScreenDraw -= new IMapControlEvents2_Ax_OnAfterScreenDrawEventHandler(MapControl_OnAfterScreenDraw);
          
        }

        public override bool Deactivate()
        {
            reset();
            return base.Deactivate();
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
            {
                IPoint tpPoint1 = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                m_Application.ActiveView.ScreenDisplay.PanMoveTo(tpPoint1);
            }
            else
            {
                if (!SimplifyLineFlag)
                {
                    return;
                }
                if (this.IsUsed == true)
                {
                    IPoint mappoint = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                    PointsFeedback.MoveTo(mappoint);

                }
            }
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            if (PointsFeedback != null)
                PointsFeedback.Display = m_Application.ActiveView.ScreenDisplay;
            if (Button == 4)
            {
                IPoint tpPoint1 = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                m_Application.ActiveView.ScreenDisplay.PanStop();
            }
           
        }
        #endregion
        private IPoint GetSnapPoint(IPoint inputPt)
        {

            IProximityOperator pro = lineGeoSimplify as IProximityOperator;
            IPoint nearpoint = new PointClass();
            pro.QueryNearestPoint(inputPt, esriSegmentExtension.esriNoExtension, nearpoint);
            IPoint splitpoint = nearpoint;
            bool haps;
            int index;
            int segment;
            (lineGeoSimplify as IPolyline).SplitAtPoint(splitpoint, true, false, out haps, out index, out segment);
            (lineGeoSimplify as ITopologicalOperator).Simplify();
             characterPoints.AddPoint(nearpoint);
            return nearpoint;
        }
    }
}

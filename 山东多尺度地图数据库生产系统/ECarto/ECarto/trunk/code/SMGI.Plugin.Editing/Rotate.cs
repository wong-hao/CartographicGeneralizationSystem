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
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;

namespace SMGI.Plugin.GeneralEdit
{
    public  class Rotate:SMGITool
    {
        public Rotate()
        {
            string str = Application.StartupPath;
            m_caption = "旋转";
            m_toolTip = "要素旋转工具";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "pen.cur")); 
            m_category = "基础编辑";
            
        }
        private ESRI.ArcGIS.Display.IRotateTracker m_RotTrack;
        private IActiveView m_Activeview ;
        IEngineEditor m_engineEditor;
        IFeature m_RotateFeature;
        IMovePointFeedback m_MovePointFB;
        IPoint m_BasePoint;
        IElement m_BasePointEle;
        bool m_InMoveArea = false;
        bool m_StartMoving = false;
 	    

        public override void OnClick()
        {
            m_Activeview = m_Application.ActiveView;
            m_engineEditor = m_Application.EngineEditor;
            m_RotTrack = new ESRI.ArcGIS.Display.EngineRotateTracker();
            
            IMap pMap = m_Application.Workspace.Map;
            if (pMap.SelectionCount != 1)
            {
                MessageBox.Show("选择要素个数不为1");
                return;
            }
            IEnumFeature pEnumFeature = (IEnumFeature)pMap.FeatureSelection;
            ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
            m_RotateFeature = pEnumFeature.Next();
            m_BasePoint = ((IArea)m_RotateFeature.Shape.Envelope).Centroid;
            //AddBasePointElement(m_Activeview);
            m_Activeview.Refresh();

            m_MovePointFB = new MovePointFeedbackClass();
            m_MovePointFB.Display = m_Activeview.ScreenDisplay;
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumFeature);
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button == 1 && m_RotateFeature != null)
            {
                if (m_InMoveArea == true)
                {
                    m_MovePointFB.Refresh(0);
                    IPoint vMovePt;
                    vMovePt = ToSnapedMapPoint(x, y);
                    //m_MovePointFB.Symbol = null;
                    m_MovePointFB.Start(m_BasePoint, vMovePt);
                    m_StartMoving = true;
                }
                else
                {
                    m_InMoveArea = false;
                    if (m_RotTrack == null)
                    {
                        m_RotTrack = new ESRI.ArcGIS.Display.EngineRotateTracker();
                    }
                    m_RotTrack.Display = m_Application.ActiveView.ScreenDisplay;
                    IGeometry pGeo = m_RotateFeature.Shape;
                    m_RotTrack.AddGeometry(pGeo);
                    
                    IArea pArea = (IArea)pGeo.Envelope;
                    m_RotTrack.Origin = m_BasePoint;

                    m_RotTrack.Refresh();
                    m_RotTrack.OnMouseDown(); 
                }
                
            }
                   
        }
        public override void OnMouseMove(int button, int shift, int x, int y)
        {

            if (  m_RotateFeature != null)
            {
                IPoint vMovePt;
                vMovePt = ToSnapedMapPoint(x, y);
                double TrackDist = 0;
                if (m_Application.Workspace.Map.SpatialReference is IGeographicCoordinateSystem)
                {
                    TrackDist = m_Application.Workspace.Map.MapScale * 0.001 * 0.000009;
                }
                else
                {
                    TrackDist = m_Application.Workspace.Map.MapScale * 0.001;
                }
                if (Math.Abs(vMovePt.X - m_BasePoint.X) <= TrackDist && Math.Abs(vMovePt.Y - m_BasePoint.Y) <= TrackDist)
                {
                    m_InMoveArea = true;
                    IMapControl2 pMapcontrol = m_Application.MapControl.Object as IMapControl2;
                    Icon icon = new Icon(GetType().Assembly.GetManifestResourceStream(GetType(), "Move.ico"));
                    //pMapcontrol.MouseIcon = (stdole.IPictureDisp)ESRI.ArcGIS.ADF.COMSupport.OLE.GetIPictureDispFromIcon(icon);
                    //pMapcontrol.MousePointer = esriControlsMousePointer.esriPointerCustom;
                    //m_cursor = System.Windows.Forms.Cursors.Arrow;
                    //m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Move.cur"));
                    //m_Application.MainForm.Cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Move.cur")); 

                }
                else
                {
                    m_InMoveArea = false;
                    //m_Application.MapControl.MousePointer = esriControlsMousePointer.esriPointerSizeAll;

                }

                if (m_StartMoving == true)
                {
                    if (m_StartMoving == true)
                    {
                        m_MovePointFB.MoveTo(vMovePt);

                    }
                }
                else
                {
                    m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
                    m_RotTrack.OnMouseMove(vMovePt);
                }        
            }
        }


        public override void OnMouseUp(int button, int shift, int x, int y)
         {
             //鼠标弹起要素保持在移动的位置
             //m_RotTrack.Refresh();
             if (this.Enabled ==true && m_RotateFeature != null)
             {
                 if (m_StartMoving == true)
                 {
                     m_BasePoint = m_MovePointFB.Stop();
                     //m_BasePointEle.Geometry = m_BasePoint;
                     //m_Activeview.GraphicsContainer.UpdateElement(m_BasePointEle);
                     m_Activeview.Refresh();
                     m_StartMoving = false;
                 }
                 else
                 {
                     m_RotTrack.OnMouseUp();
                     if (!double.IsNaN(m_RotTrack.Angle))
                     {
                         m_engineEditor.StartOperation();
                         if (m_RotateFeature.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                         {
                             var ai = m_RotateFeature.Fields.FindField("ANGLE");
                             if (ai != -1 && m_RotateFeature.Value[ai] != null)
                             {
                                 var angle = Convert.ToDouble(m_RotateFeature.Value[ai]);
                                 double value = angle - m_RotTrack.Angle / Math.PI * 180;
                                 if (value > 360)
                                     value = value - 360;
                                 if(value<0)
                                     value = value + 360;
                                 m_RotateFeature.Value[ai] = value;
                                 m_RotateFeature.Store();
                             }
                         }
                         else
                         {
                             ESRI.ArcGIS.Geometry.ITransform2D pTrans2D;
                             pTrans2D = (ITransform2D)m_RotateFeature.Shape;
                             pTrans2D.Rotate(m_RotTrack.Origin, m_RotTrack.Angle);
                             m_RotateFeature.Shape = (IGeometry)pTrans2D;
                             m_RotateFeature.Store();
                         }
                         m_engineEditor.StopOperation(null);
                         m_Activeview.Refresh();
                     }
                 }
             }
          }
        ISymbol  CreatePointSymbol()
        {
            ISymbol pSymbol;
            IMarkerSymbol pMarkerSys = new SimpleMarkerSymbolClass();
            pMarkerSys.Size = 4;
            IRgbColor  pColor = new RgbColorClass();
            pColor.Red = 223;
            pColor.Green = 115;
            pColor.Blue = 255;
            pMarkerSys.Color = (IColor)pColor;
            pSymbol = (ISymbol)pMarkerSys;
            return pSymbol;
        }
        void AddBasePointElement(IActiveView pActiveview)
        {
            IMarkerElement pMarkerEle = new MarkerElementClass();
            pMarkerEle.Symbol = CreatePointSymbol() as IMarkerSymbol;
            m_BasePointEle = (IElement)pMarkerEle;
            m_BasePointEle.Geometry = m_BasePoint;
            pActiveview.GraphicsContainer.AddElement(m_BasePointEle, 0);
        }
        public override void Refresh(int hdc)
        {
            SimpleDisplayClass simDis = new SimpleDisplayClass();
            simDis.DisplayTransformation = m_Activeview.ScreenDisplay.DisplayTransformation;
            simDis.DisplayTransformation.ReferenceScale = 0;

            IDisplay pDisplay = simDis;
            pDisplay.StartDrawing(hdc, -1);
            pDisplay.SetSymbol(CreatePointSymbol());
            pDisplay.DrawPoint(m_BasePoint);
            pDisplay.FinishDrawing();
        }

         public override bool Enabled
         {
             get
             {
                 m_engineEditor = m_Application.EngineEditor;
                 if (m_Application != null && m_Application.Workspace != null)
                 {
                     if (m_engineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing)
                     {
                         return false;
                     }

                     return true;

                 }
                 else
                 {
                     return false;
                 }
             }
         }
    }
}

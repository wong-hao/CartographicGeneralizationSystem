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
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class EditSelector : SMGITool
    {
        ControlsEditingEditToolClass currentTool;

        bool bEditVertices;//是否在编辑要素

        Dictionary<int, IPoint> selectedVertexs;//被选中的节点集合
        int curVertexIndex;//当前选中的节点的索引号

        double shiftStepDistance;//要素位移的步长（通过键盘的方向键控制被选择要素的移动）

        public EditSelector()
        {
            m_caption = "编辑";
            m_toolTip = "编辑器选择工具,支持要素整体移动，鼠标移动到要素内，整体移动；支持节点编辑：双击需要编辑的要素，Ctrl+鼠标右键框选节点，进行多个移动；或者鼠标移动到节点上左键单个移动";

            currentTool = new ControlsEditingEditToolClass();

            bEditVertices = false;
            selectedVertexs = new Dictionary<int, IPoint>();
            curVertexIndex = -1;

            shiftStepDistance = 1;//米

            NeedSnap = false;
        }


        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            currentTool.OnCreate(m_Application.MapControl.Object);

        }
        public override void OnClick()
        {
            currentTool.OnClick();

            (m_Application.EngineEditor as IEngineEditEvents_Event).OnCurrentTaskChanged += new IEngineEditEvents_OnCurrentTaskChangedEventHandler(EngineEditor_OnCurrentTaskChanged);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexMoved += new IEngineEditEvents_OnVertexMovedEventHandler(EngineEditor_OnVertexMoved);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexDeleted += new IEngineEditEvents_OnVertexDeletedEventHandler(EngineEditor_OnVertexDeleted);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnChangeFeature += new IEngineEditEvents_OnChangeFeatureEventHandler(EngineEditor_OnChangeFeature);
        }

        public override bool Deactivate()
        {
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnCurrentTaskChanged -= new IEngineEditEvents_OnCurrentTaskChangedEventHandler(EngineEditor_OnCurrentTaskChanged);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexMoved -= new IEngineEditEvents_OnVertexMovedEventHandler(EngineEditor_OnVertexMoved);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexDeleted -= new IEngineEditEvents_OnVertexDeletedEventHandler(EngineEditor_OnVertexDeleted);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnChangeFeature -= new IEngineEditEvents_OnChangeFeatureEventHandler(EngineEditor_OnChangeFeature);

            return currentTool.Deactivate();
        }

        public override int Cursor
        {
            get
            {
                return currentTool.Cursor;
            }

        }

        public override bool OnContextMenu(int x, int y)
        {
            return currentTool.OnContextMenu(x, y);
        }

        public override void OnDblClick()
        {
            currentTool.OnDblClick();
            //多点联动后，编辑工具无法恢复右键菜单(2022.8.15)
            //双击、有单一要素选择，恢复为可节点编辑
            if(m_Application.MapControl.Map.SelectionCount==1) 
                bEditVertices = true;

        }

        public override void OnKeyDown(int keyCode, int shift)
        {
            currentTool.OnKeyDown(keyCode, shift);

            if (keyCode == (int)System.Windows.Forms.Keys.O)
            {
                FeatureTransStepForm setFrm = new FeatureTransStepForm(shiftStepDistance);
                setFrm.StartPosition = FormStartPosition.CenterParent;
                if (setFrm.ShowDialog() == DialogResult.OK)
                {
                    shiftStepDistance = setFrm.DisStep;
                }
                return;
            }

            if (0 == m_Application.MapControl.Map.SelectionCount)
                return;

            if (bEditVertices)
                return;

            UnitConverterClass unitConverter = new UnitConverterClass();
            double stepDis = unitConverter.ConvertUnits(shiftStepDistance, ESRI.ArcGIS.esriSystem.esriUnits.esriMeters, m_Application.MapControl.MapUnits);

            double dx = 0, dy = 0;
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.A://左(a)
                    {
                        dx -= stepDis;
                        break;
                    }
                case (int)System.Windows.Forms.Keys.W://上(w)
                    {
                        dy += stepDis;
                        break;
                    }
                case (int)System.Windows.Forms.Keys.D://右(d)
                    {
                        dx += stepDis;
                        break;
                    }
                case (int)System.Windows.Forms.Keys.S://下(s)
                    {
                        dy -= stepDis;
                        break;
                    }
            }

            if (0 == dx && 0 == dy)
                return;

            //平移
            m_Application.EngineEditor.StartOperation();

            List<IFeature> selFeas = new List<IFeature>();
            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();
            IFeature feature = mapEnumFeature.Next();
            while (feature != null)
            {
                ITransform2D trans = feature.Shape as ITransform2D;
                trans.Move(dx, dy);
                feature.Shape = trans as IGeometry;
                feature.Store();

                feature = mapEnumFeature.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);

            m_Application.EngineEditor.StopOperation("平移");


            m_Application.MapControl.ActiveView.Refresh();
        }

        //public override void OnKeyUp(int keyCode, int shift)
        //{
        //    currentTool.OnKeyUp(keyCode, shift);
        //}
        FrmSelectionDo frm = null;
        public override void OnKeyUp(int keyCode, int shift)
        {
            currentTool.OnKeyUp(keyCode, shift);
            //空格键进行弹窗
            if (keyCode == 32)
            {
                if (frm == null || frm.IsDisposed)
                {
                    frm = new FrmSelectionDo();
                }
                else
                {
                    frm.LoadFeatures();
                }
                frm.Show();
                frm.TopMost = true;
                frm.Activate();
            }
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            currentTool.OnMouseDown(button, shift, x, y);

            curVertexIndex = -1;

            if (bEditVertices)
            {
                 if (2 == button && (Control.ModifierKeys & Keys.Control) == Keys.Control)    
               // if(button==1&&shift==2)
                {
                    selectedVertexs = new Dictionary<int, IPoint>();

                    IEnvelope pEnvelope = m_Application.MapControl.TrackRectangle();
                    if (pEnvelope.IsEmpty)
                    {
                        return;
                    }

                    var trackPolyline = new PolygonClass();
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.UpperLeft);
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.UpperRight);
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.LowerRight);
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.LowerLeft);
                    (trackPolyline as IPointCollection).AddPoint(pEnvelope.UpperLeft);

                    //将选择框内的节点标记为选择状态
                    IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
                    IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
                    if (ptcoll != null)
                    {
                        for (int i = 0; i < ptcoll.PointCount; ++i)
                        {
                            IPoint pt = ptcoll.get_Point(i);
                            if (trackPolyline.Contains(pt))
                            {
                                selectedVertexs.Add(i, pt);
                            }
                        }
                    }
                   
                }
                else
                {
                    IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
                    IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
                    //curVertexIndex = editsketch.Vertex;
                    if (ptcoll != null)
                    {
                        IEngineSnapEnvironment snapEnvi = m_Application.EngineEditor as IEngineSnapEnvironment;
                        double tol = snapEnvi.SnapTolerance;
                        IPoint mousePt = ToSnapedMapPoint(x, y);
                        var polyline = new PolygonClass();
                        if (esriEngineSnapToleranceUnits.esriEngineSnapTolerancePixels == snapEnvi.SnapToleranceUnits)
                        {
                            IPoint llPoint = new PointClass(), ulPoint = new PointClass(), urPoint = new PointClass(), lrPoint = new PointClass();

                            int iTol = (int)tol;
                            llPoint = ToSnapedMapPoint(x - iTol, y + iTol);
                            urPoint = ToSnapedMapPoint(x + iTol, y - iTol);
                            ulPoint.PutCoords(llPoint.X, urPoint.Y);
                            lrPoint.PutCoords(urPoint.X, llPoint.Y);

                            (polyline as IPointCollection).AddPoint(llPoint);
                            (polyline as IPointCollection).AddPoint(ulPoint);
                            (polyline as IPointCollection).AddPoint(urPoint);
                            (polyline as IPointCollection).AddPoint(lrPoint);
                            (polyline as IPointCollection).AddPoint(llPoint);
                        }
                        else
                        {
                            IPoint llPoint = new PointClass(), ulPoint = new PointClass(), urPoint = new PointClass(), lrPoint = new PointClass();

                            llPoint.PutCoords(mousePt.X - tol, mousePt.Y - tol);
                            urPoint.PutCoords(mousePt.X + tol, mousePt.Y + tol);
                            ulPoint.PutCoords(llPoint.X, urPoint.Y);
                            lrPoint.PutCoords(urPoint.X, llPoint.Y);

                            (polyline as IPointCollection).AddPoint(llPoint);
                            (polyline as IPointCollection).AddPoint(ulPoint);
                            (polyline as IPointCollection).AddPoint(urPoint);
                            (polyline as IPointCollection).AddPoint(lrPoint);
                            (polyline as IPointCollection).AddPoint(llPoint);
                        }
                        

                        double minDis = double.MaxValue;
                        for (int i = 0; i < ptcoll.PointCount; ++i)
                        {
                            IPoint p = ptcoll.get_Point(i);
                            if (polyline.Contains(p))
                            {
                                IProximityOperator ProxiOP = (mousePt) as IProximityOperator;
                                double dis = ProxiOP.ReturnDistance(p);
                                if (dis < minDis)
                                {
                                    curVertexIndex = i;
                                    minDis = dis;
                                }
                            }
                        }
                    }

                    //是否清空框选的节点selectedVertexs
                    if (selectedVertexs.Count != 0 && !selectedVertexs.Keys.Contains(curVertexIndex))
                    {
                        selectedVertexs = new Dictionary<int, IPoint>();
                    }

                }
            }
            else
            {
                selectedVertexs = new Dictionary<int, IPoint>();
            }
            DrawVertexs();
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            currentTool.OnMouseMove(button, shift, x, y);

            if (1 == button)
            {
                if (bEditVertices)//节点编辑（移动）
                {
                    NeedSnap = true;

                    //移动所有选择的节点
                    if (selectedVertexs.Count > 0)
                    {
                        IPoint curPoint = ToSnapedMapPoint(x, y);
                        IPoint oldPoint = selectedVertexs[curVertexIndex];

                        double dx = curPoint.X - oldPoint.X;
                        double dy = curPoint.Y - oldPoint.Y;

                        IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
                        IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
                        foreach (var kv in selectedVertexs)
                        {
                            if (kv.Key != curVertexIndex)
                            {
                                IPoint newPt = new PointClass();
                                newPt.PutCoords(kv.Value.X + dx, kv.Value.Y + dy);
                                ptcoll.UpdatePoint(kv.Key, newPt);//修正坐标
                            }
                          
                        }
                        for (int i = 0; i < selectedVertexs.Keys.Count; i++)
                        {
                            int key = selectedVertexs.Keys.ToArray()[i];
                            var pt = selectedVertexs[key];

                            pt.PutCoords(pt.X + dx, pt.Y + dy);
                            selectedVertexs[key] = pt;
                        }
                        DrawVertexs();

                    }
                }
                else
                {
                    //点要素编辑（移动）
                    if (1 == m_Application.MapControl.Map.SelectionCount)
                    {
                        IEnumFeature pEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                        pEnumFeature.Reset();
                        IFeature feature = pEnumFeature.Next();
                        if (feature != null && feature.Shape is IPoint)
                        {
                            NeedSnap = true;
                        }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumFeature);
                    }
                }
            }


        }
        private void DrawVertexs()
        {
            RgbColorClass c = new RgbColorClass();
            IGraphicsContainer gc = m_Application.ActiveView.GraphicsContainer;
            gc.DeleteAllElements();
            for (int i = 0; i <selectedVertexs.Keys.Count; i++)
            {
                int key=selectedVertexs.Keys.ToArray()[i];
                var pt = selectedVertexs[key];
                SimpleMarkerSymbolClass sms = new SimpleMarkerSymbolClass();
                c.Red = 250;
                c.Blue = 0;
                c.Green = 0;

                sms.Style = esriSimpleMarkerStyle.esriSMSSquare;
                sms.Color = c;
                if (m_Application.ActiveView.FocusMap.ReferenceScale == 0)
                    sms.Size = 5;
                else
                    sms.Size = 1;
                IMarkerElement markerEle = new MarkerElementClass();
                markerEle.Symbol = sms;
                IElement element = markerEle as IElement;
                element.Geometry = pt as IGeometry;
                gc.AddElement(element, 0);
            }
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_Application.ActiveView.Extent);
        
        }
    
        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            currentTool.OnMouseUp(button, shift, x, y);

            NeedSnap = false;


            if (2 == button && bEditVertices)
            {
                IToolbarMenu toolbarMenu = new ToolbarMenu();
                //toolbarMenu.AddItem("esriControls.ControlsEditingVertexContextMenu", 0, 0, false, esriCommandStyles.esriCommandStyleTextOnly);
                toolbarMenu.AddItem(new ControlsEditingVertexInsertCommandClass());
                toolbarMenu.AddItem(new ControlsEditingVertexDeleteCommandClass());
                toolbarMenu.CommandPool = m_Application.MainForm.CommandPool;

                IEngineEditSketch engineEditSketch = (IEngineEditSketch)m_Application.EngineEditor;
                engineEditSketch.SetEditLocation(x, y);
                toolbarMenu.PopupMenu(x, y, m_Application.MapControl.hWnd);

            }
            if (frm != null && !frm.IsDisposed)
            {
                frm.LoadFeatures();
                frm.Show();
                frm.TopMost = true;
                frm.Activate();
            }
            DrawVertexs();
        }

        public override void Refresh(int hdc)
        {
            currentTool.Refresh(hdc);
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }


        void EngineEditor_OnCurrentTaskChanged()
        {
            if (m_Application.EngineEditor.CurrentTask.UniqueName == "ControlToolsEditing_ModifyFeatureTask")
            {
                bEditVertices = true;
            }
            else
            {
                bEditVertices = false;
            }

        }

        //节点移动
        void EngineEditor_OnVertexMoved<T>(T param)
        {
            IPoint p = param as IPoint;
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;

            //int index = editsketch.Vertex;//无效?
            int index = -1;
            IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
            for (int i = 0; i < ptcoll.PointCount; ++i)
            {
                if (0 == p.Compare(ptcoll.get_Point(i)))
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                int x, y;
                m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p, out x, out y);

                IPoint snapPoint = ToSnapedMapPoint(x, y);
                ptcoll.UpdatePoint(index, snapPoint);//修正坐标

                ClearSnapperCache();
            }

        }

        //节点删除
        void EngineEditor_OnVertexDeleted<T>(T param)
        {
            IPoint p = param as IPoint;
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;

            //删除所有选择的节点
            IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
            if (selectedVertexs.Count > 0)
            {
                int numDel = 0;
                foreach (var kv in selectedVertexs)
                {
                    if (kv.Key == curVertexIndex)
                    {
                        numDel++;
                        continue;
                    }

                    int newIndex = kv.Key - numDel;
                    ptcoll.RemovePoints(newIndex, 1);
                    numDel++;
                }

                selectedVertexs = new Dictionary<int, IPoint>();

                ClearSnapperCache();
            }

        }

        //点要素移动
        void EngineEditor_OnChangeFeature(IObject Object)
        {
            IFeature feature = Object as IFeature;

            if (1 == m_Application.MapControl.Map.SelectionCount && feature.Shape is IPoint)
            {
                IPoint p = feature.Shape as IPoint;

                int x, y;
                m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p, out x, out y);

                IPoint snapPoint = ToSnapedMapPoint(x, y);
                p.PutCoords(snapPoint.X, snapPoint.Y);//修正坐标

                ClearSnapperCache();
            }
        }
    }
}

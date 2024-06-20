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
    public class MultiPointMove : SMGITool
    {
        ControlsEditingEditToolClass currentTool;

        bool bEditVertices;//是否在编辑要素
        private IEngineEditSketch m_editSketch = null;
        Dictionary<int, IPoint> selectedVertexs;//被选中的节点集合
        Dictionary<int, IPoint> selectedVertexs1;//被选中的节点集合      
        IPoint p;
        IWorkspace workspace = null;
        IFeatureWorkspace featureWorkspace;
        IFeatureClass CheckClass;
        string layername = null;
        public MultiPointMove()
        {
            m_caption = "多点移动";
            m_toolTip = "编辑器选择工具";

            currentTool = new ControlsEditingEditToolClass();

            bEditVertices = false;
            selectedVertexs = new Dictionary<int, IPoint>();
            selectedVertexs1 = new Dictionary<int, IPoint>();

            NeedSnap = false;


        }


        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            currentTool.OnCreate(m_Application.MapControl.Object);

        }
        public override void OnClick()
        {
            //MultiPointMoveFormm setFrm = new MultiPointMoveFormm();
            //setFrm.StartPosition = FormStartPosition.CenterScreen;
            //setFrm.ShowDialog();
            //layername = setFrm.layer();
            currentTool.OnClick();


            (m_Application.EngineEditor as IEngineEditEvents_Event).OnCurrentTaskChanged += new IEngineEditEvents_OnCurrentTaskChangedEventHandler(EngineEditor_OnCurrentTaskChanged);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexMoved += new IEngineEditEvents_OnVertexMovedEventHandler(EngineEditor_OnVertexMoved);
            //(m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexDeleted += new IEngineEditEvents_OnVertexDeletedEventHandler(EngineEditor_OnVertexDeleted);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnChangeFeature += new IEngineEditEvents_OnChangeFeatureEventHandler(EngineEditor_OnChangeFeature);
            workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;


        }

        public override bool Deactivate()
        {
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnCurrentTaskChanged -= new IEngineEditEvents_OnCurrentTaskChangedEventHandler(EngineEditor_OnCurrentTaskChanged);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexMoved -= new IEngineEditEvents_OnVertexMovedEventHandler(EngineEditor_OnVertexMoved);
            //(m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexDeleted -= new IEngineEditEvents_OnVertexDeletedEventHandler(EngineEditor_OnVertexDeleted);
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

        }


        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            currentTool.OnMouseDown(button, shift, x, y);


            if (true && 2 == button)
            {
                NeedSnap = true;
                move(p);
                NeedSnap = false;
            }
            if (bEditVertices && 1 == button)
            {
                selectedVertexs1 = new Dictionary<int, IPoint>();
                IEngineEditSketch editsketch1 = m_Application.EngineEditor as IEngineEditSketch;
                IEngineEditor engineEditor = m_Application.EngineEditor;

                IEnumFeature enumFeature = engineEditor.EditSelection;

                IFeature SelectFeature = enumFeature.Next();
                if (SelectFeature != null)
                {
                    Polyline pp = SelectFeature.Shape as Polyline;
                    IPointCollection ptcoll2 = (pp) as IPointCollection;
                    if (ptcoll2 != null)
                    {
                        for (int i = 0; i < ptcoll2.PointCount; ++i)
                        {
                            IPoint pt = ptcoll2.get_Point(i);

                            selectedVertexs1.Add(i, pt);

                        }
                    }

                }
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            currentTool.OnMouseMove(button, shift, x, y);
            if (1 == button)
            {
                if (bEditVertices)
                {
                    NeedSnap = true;
                    p = ToSnapedMapPoint(x, y);
                }
            }
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            currentTool.OnMouseUp(button, shift, x, y);

            NeedSnap = false;


            //if (2 == button && bEditVertices)
            //{
            //    IToolbarMenu toolbarMenu = new ToolbarMenu();
            //    //toolbarMenu.AddItem("esriControls.ControlsEditingVertexContextMenu", 0, 0, false, esriCommandStyles.esriCommandStyleTextOnly);
            //    toolbarMenu.AddItem(new ControlsEditingVertexInsertCommandClass());
            //    toolbarMenu.AddItem(new ControlsEditingVertexDeleteCommandClass());
            //    toolbarMenu.CommandPool = m_Application.MainForm.CommandPool;

            //    IEngineEditSketch engineEditSketch = (IEngineEditSketch)m_Application.EngineEditor;
            //    engineEditSketch.SetEditLocation(x, y);
            //    toolbarMenu.PopupMenu(x, y, m_Application.MapControl.hWnd);

            //}

            //if (bEditVertices && 1 == button)
            //{
            //    IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;

            //    //int index = editsketch.Vertex;//无效?
            //    int index = -1;
            //    IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
            //    for (int i = 0; i < ptcoll.PointCount; ++i)
            //    {
            //        if (0 == p.Compare(ptcoll.get_Point(i)))
            //        {
            //            index = i;
            //            break;
            //        }
            //    }
            //    IPoint point = selectedVertexs1[index];
            //    //if (index != -1)
            //    //{
            //    //    int x, y;
            //    //    m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p, out x, out y);

            //    //    IPoint snapPoint = ToSnapedMapPoint(x, y);
            //    //    ptcoll.UpdatePoint(index, snapPoint);//修正坐标
            //    //}

            //    IMap Map = m_Application.MapControl.Map;
            //    //    ISelection FeatureSelection = Map.FeatureSelection;
            //    //  IFeature polylineSelection = FeatureSelection as IFeature;
            //    CheckClass = featureWorkspace.OpenFeatureClass("HYDL");
            //    ITopologicalOperator pTOPO;


            //    pTOPO = point as ITopologicalOperator;
            //    pTOPO.Simplify();
            //    IGeometry BufferGeo2 = pTOPO.Buffer(0.00000009);
            //    ISpatialFilter pFilter = new SpatialFilterClass();
            //    pFilter.Geometry = BufferGeo2;
            //    pFilter.GeometryField = CheckClass.ShapeFieldName;
            //    pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            //    IFeatureCursor pSelectCursor = CheckClass.Search(pFilter, true);
            //    IFeature SelectFeature = pSelectCursor.NextFeature();
            //    while (SelectFeature != null)
            //    {
            //        if (SelectFeature.OID == 66)
            //        {
            //            int index1 = -1;
            //            Polyline pp = SelectFeature.Shape as Polyline;
            //            IPolyline pp2 = SelectFeature.Shape as IPolyline;
            //            //Polyline pp1 = (Polyline)SelectFeature ;
            //            IPointCollection ptcoll1 = (pp) as IPointCollection;

            //            IPointCollection ptcoll13 = (pp2) as IPointCollection;
            //            for (int i = 0; i < ptcoll1.PointCount; ++i)
            //            {
            //                IPoint ppp = ptcoll1.get_Point(i);
            //                if (0 == point.Compare(ppp))
            //                {
            //                    index1 = i;
            //                    break;
            //                }
            //            }
            //            IPointCollection ptColl = (m_Application.EngineEditor as IEngineEditSketch).Geometry as IPointCollection;
            //            if (index1 != -1)
            //            {
            //                int x1, y1;
            //                m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p, out x1, out y1);

            //                IPoint snapPoint = ToSnapedMapPoint(x1, y1);
            //                ptColl.UpdatePoint(3, snapPoint);//修正坐标
            //            }
            //        }
            //        SelectFeature = pSelectCursor.NextFeature();
            //    }
            //    System.Runtime.InteropServices.Marshal.ReleaseComObject(pSelectCursor);

            //}
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
        void store(IFeature SelectFeature, IPoint point)
        {
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;
            IEngineEditLayers engineEditLayer = editsketch as IEngineEditLayers;
            IFeatureLayer featureLayer = engineEditLayer.TargetLayer as IFeatureLayer;
            m_editSketch = m_Application.EngineEditor as IEngineEditSketch;
            m_editSketch.GeometryType = esriGeometryType.esriGeometryPolyline;
            m_editSketch.Geometry = new PolylineClass();
            {
                Polyline pp = SelectFeature.ShapeCopy as Polyline;

                IPointCollection ptcoll1 = (pp) as IPointCollection;

                for (int i = 0; i < ptcoll1.PointCount; ++i)
                {
                    IPoint ppp = ptcoll1.get_Point(i);
                    if (0 == point.Compare(ppp))
                    {
                        int x1, y1;
                        m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p, out x1, out y1);

                        IPoint snapPoint = ToSnapedMapPoint(x1, y1);

                        ptcoll1.UpdatePoint(i, snapPoint);
                        IPoint ppp9 = ptcoll1.get_Point(i);
                    }

                }

                IGeometry editShape = SelectFeature.ShapeCopy;

                IPolyline polyline = editShape as IPolyline;
                IPointCollection ptcoll1tem = (polyline) as IPointCollection;
                ptcoll1tem.ReplacePointCollection(0, ptcoll1tem.PointCount, ptcoll1);


                try
                {
                    m_Application.EngineEditor.StartOperation();
                    SelectFeature.Shape = polyline;
                    SelectFeature.Store();
                    m_Application.EngineEditor.StopOperation("Reshape Feature");
                }
                catch (Exception ex)
                {
                    // m_Application.EngineEditor.AbortOperation();
                }
                IActiveView activeView = m_Application.EngineEditor.Map as IActiveView;
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, (object)featureLayer, activeView.Extent);
            }
        }
        void move(IPoint p)
        {
            if (true)
            {
                IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;

                IEngineEditLayers engineEditLayer = editsketch as IEngineEditLayers;
                IFeatureLayer featureLayer = engineEditLayer.TargetLayer as IFeatureLayer;
                IFeatureLayer IFeatureLayer = (IFeatureLayer)featureLayer;
                //  IFeatureClass checkFeatureClass = IFeatureLayer.FeatureClass;
                //   string layer = featureLayer.Name;
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
                if (index == -1) { return; }
                IPoint point = selectedVertexs1[index];

                IMap Map = m_Application.MapControl.Map;
                // CheckClass = featureWorkspace.OpenFeatureClass(layername);
                CheckClass = IFeatureLayer.FeatureClass;
                ITopologicalOperator pTOPO;


                pTOPO = point as ITopologicalOperator;
                pTOPO.Simplify();
                IGeometry BufferGeo2 = pTOPO.Buffer(0.00000009);
                ISpatialFilter pFilter = new SpatialFilterClass();
                pFilter.Geometry = BufferGeo2;
                pFilter.GeometryField = CheckClass.ShapeFieldName;
                pFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor pSelectCursor = CheckClass.Search(pFilter, false);
                IFeature SelectFeature = pSelectCursor.NextFeature();
                while (SelectFeature != null)
                {
                    store(SelectFeature, point);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SelectFeature);
                    SelectFeature = null;
                    SelectFeature = pSelectCursor.NextFeature();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pSelectCursor);
            }
        }
        //节点移动
        void EngineEditor_OnVertexMoved<T>(T param)
        {
            IPoint pt = param as IPoint;
            IEngineEditSketch editsketch = m_Application.EngineEditor as IEngineEditSketch;

            int index = -1;
            IPointCollection ptcoll = editsketch.Geometry as IPointCollection;
            for (int i = 0; i < ptcoll.PointCount; ++i)
            {
                if (0 == pt.Compare(ptcoll.get_Point(i)))
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                int x, y;
                m_Application.ActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(pt, out x, out y);

                IPoint snapPoint = ToSnapedMapPoint(x, y);
                ptcoll.UpdatePoint(index, snapPoint);//修正坐标
            }
        }
        //点要素移动
        void EngineEditor_OnChangeFeature(IObject Object)
        {
        }
    }
}
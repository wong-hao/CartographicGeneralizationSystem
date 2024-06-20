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
    public class FeatureParallelMoveTool : SMGITool
    {
        IEngineEditor editor;
        bool bEditVertices;//是否在编辑要素
        bool modifyMajar = true;
        ControlsEditingEditToolClass currentTool;
        private IEngineEditSketch m_editSketch = null;
        string MainMap = null;//主图名
        IWorkspace workspace = null;
        IFeatureWorkspace featureWorkspace;
        public FeatureParallelMoveTool()
        {

            m_caption = "构造平行线";
            currentTool = new ControlsEditingEditToolClass();
            bEditVertices = false;
            NeedSnap = true;
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        public override bool Deactivate()
        {
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnCurrentTaskChanged -= new IEngineEditEvents_OnCurrentTaskChangedEventHandler(EngineEditor_OnCurrentTaskChanged);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexMoved -= new IEngineEditEvents_OnVertexMovedEventHandler(EngineEditor_OnVertexMoved);
            //(m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexDeleted -= new IEngineEditEvents_OnVertexDeletedEventHandler(EngineEditor_OnVertexDeleted);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnChangeFeature -= new IEngineEditEvents_OnChangeFeatureEventHandler(EngineEditor_OnChangeFeature);

            return currentTool.Deactivate();
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            currentTool.OnCreate(m_Application.MapControl.Object);

        }
        public override void OnClick()
        {
            editor = m_Application.EngineEditor;
            currentTool.OnClick();
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexMoved += new IEngineEditEvents_OnVertexMovedEventHandler(EngineEditor_OnVertexMoved);
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnChangeFeature += new IEngineEditEvents_OnChangeFeatureEventHandler(EngineEditor_OnChangeFeature);
            workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;
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
        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            currentTool.OnMouseMove(button, shift, x, y);
        }
        public override void Refresh(int hdc)
        {
            currentTool.Refresh(hdc);
        }
        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            currentTool.OnMouseUp(button, shift, x, y);
            NeedSnap = false;
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            currentTool.OnMouseDown(button, shift, x, y);
            if (button == 2)//右键
            {
                FeatureParallelCopyToolForm frm = new FeatureParallelCopyToolForm();
                if (frm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                IFeatureClass feaFC = featureWorkspace.OpenFeatureClass(frm.Tolayer.ToUpper());

                if (feaFC == null) { MessageBox.Show("目标图层不存在"); return; }
                editor.StartOperation();
                try
                {
                    IPoint position = ToSnapedMapPoint(x, y);
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature pFeature;
                    while ((pFeature = mapEnumFeature.Next()) != null)
                    {
                        if (feaFC.ShapeType != (pFeature.Class as IFeatureClass).ShapeType) { MessageBox.Show("目标图层与拷贝要素几何类型不一致"); return; }
                        IPoint boulp = new PointClass();
                        IPolyline polyline = pFeature.ShapeCopy as IPolyline;
                        IFeature feaNew = feaFC.CreateFeature();
                        double distancealong = 0; double distanceForm = 0; bool right = false;
                        polyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, position, false, boulp, ref distancealong, ref distanceForm, ref right);
                        IConstructCurve pOffset = new PolylineClass();
                        if (right)
                        {
                            pOffset.ConstructOffset(polyline, frm.OffsetDis, esriConstructOffsetEnum.esriConstructOffsetSimple);
                        }
                        else
                        {
                            pOffset.ConstructOffset(polyline, -1 * frm.OffsetDis, esriConstructOffsetEnum.esriConstructOffsetSimple);
                        }
                        feaNew.Shape = pOffset as IGeometry;
                        feaNew.Store();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    editor.AbortOperation();
                }
                editor.StopOperation("构造平行线");
                m_Application.MapControl.Refresh();
            }
        }
        void EngineEditor_OnCurrentTaskChanged()
        {
        }
        void EngineEditor_OnVertexMoved<T>(T param)
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
        //点要素移动
        void EngineEditor_OnChangeFeature(IObject Object)
        {
        }
    }
}

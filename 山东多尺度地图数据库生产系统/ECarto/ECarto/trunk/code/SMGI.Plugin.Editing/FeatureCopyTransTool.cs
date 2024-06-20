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
    public class FeatureCopyTransTool : SMGITool
    {
        IEngineEditor editor;
        bool bEditVertices;//是否在编辑要素
        bool modifyMajar = true;
        ControlsEditingEditToolClass currentTool;
        private IEngineEditSketch m_editSketch = null;
        string MainMap = null;//主图名
        IWorkspace workspace = null;
        IFeatureWorkspace featureWorkspace;
        FeatureCopyTransToolForm frm;
        public FeatureCopyTransTool()
        {
            m_caption = "拷贝";
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
            (m_Application.EngineEditor as IEngineEditEvents_Event).OnVertexMoved -= new IEngineEditEvents_OnVertexMovedEventHandler(EngineEditor_OnVertexMoved);
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
            workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;

            frm = new FeatureCopyTransToolForm();
            if (frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }
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
                IFeatureClass feaFC = featureWorkspace.OpenFeatureClass(frm.OffsetDis.ToUpper());
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
                        IPoint m_BasePoint = null;
                        if (pFeature.Shape is IPoint) { m_BasePoint = pFeature.ShapeCopy as IPoint; }
                        else { m_BasePoint = ((IArea)pFeature.Shape.Envelope).Centroid; }
                        ILine line = new LineClass();
                        line.PutCoords(m_BasePoint, position);
                        //  (line as ITopologicalOperator).Simplify();

                        IFeature feaNew = feaFC.CreateFeature();
                        ITransform2D pTrans2D;
                        pTrans2D = (ITransform2D)pFeature.ShapeCopy;
                        pTrans2D.MoveVector(line);
                        feaNew.Shape = pTrans2D as IGeometry;
                        feaNew.Store();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    editor.AbortOperation();
                }
                editor.StopOperation("拷贝");
                m_Application.MapControl.Refresh();
            }
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

    }
}

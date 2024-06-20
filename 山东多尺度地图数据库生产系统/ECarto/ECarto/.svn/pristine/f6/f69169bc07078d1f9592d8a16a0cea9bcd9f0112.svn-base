using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
using ESRI.ArcGIS.Geometry;
using System.Data;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 交互式隐藏:对点状图层要素（通过列表框选择），进行拉框选择，实现对范围内的目标点要素进行隐藏（设置为未选取），且相关联的注记也进行隐藏（设置为未选取）
    /// </summary>
    public class UnselectFeatureTool : SMGI.Common.SMGITool
    {
        List<IFeatureClass> _fcList = null;
        string _selStateFN;
        public UnselectFeatureTool()
        {
            m_category = "交互式隐藏（交互）";

            NeedSnap = false;

            _selStateFN = "selectstate";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            var frm = new UnselectFeatureToolForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.Text = "要素交互式隐藏";
            if (frm.ShowDialog() != DialogResult.OK)
                return;


            _fcList = frm.FeatureClassList;
            
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;

            if (_fcList == null)
                return;

            //拉框
            IRubberBand rubberBand = new RubberRectangularPolygonClass();
            IGeometry geo = rubberBand.TrackNew(m_Application.ActiveView.ScreenDisplay, null);
            if (geo == null || geo.IsEmpty)
                return;

            
            //清理所选要素
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            m_Application.MapControl.Map.ClearSelection();

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    foreach (var fc in _fcList)
                    {
                        wo.SetText(string.Format("正在处理要素类【{0}】......", fc.AliasName));

                        int selStateIndex = fc.FindField(_selStateFN);
                        if (selStateIndex == -1)
                        {
                            throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", fc.AliasName, _selStateFN));
                        }

                        List<int> unselectOIDList = unselectFeatureByExtentGeo(fc, geo, _selStateFN);
                        if (unselectOIDList.Count > 0)
                        {
                            selectAnnoByConnFe(fc, unselectOIDList, _selStateFN);
                        }
                    }

                }

                m_Application.EngineEditor.StopOperation("要素交互式隐藏");

                //刷新
                IEnvelope env = geo.Envelope;
                env.Expand(1.5, 1.5, true);
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, env);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                m_Application.EngineEditor.AbortOperation();
            }

        }

        public List<int> unselectFeatureByExtentGeo(IFeatureClass fc, IGeometry extentGeo, string selStateFN, string unSelText = "未选取")
        {
            List<int> unselectOIDList = new List<int>();//需隐藏要素OID集合

            int selStateIndex = fc.FindField(selStateFN);

            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = extentGeo;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.WhereClause = string.Format("{0} is null ", selStateFN);
            IFeatureCursor feCursor = fc.Update(sf, true);
            IFeature fe = null;
            while ((fe = feCursor.NextFeature()) != null)
            {
                fe.set_Value(selStateIndex, unSelText);//设置为未选取状态
                feCursor.UpdateFeature(fe);

                unselectOIDList.Add(fe.OID);
            }
            Marshal.ReleaseComObject(feCursor);

            return unselectOIDList;
        }

        /// <summary>
        /// 设置要素相关联注记的选取状态
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="unselectOIDList"></param>
        /// <param name="selStateFN"></param>
        /// <param name="unSelText"></param>
        public static void selectAnnoByConnFe(IFeatureClass fc, List<int> unselectOIDList, string selStateFN, string unSelText = "未选取")
        {
            var annoLayers = GApplication.Application.Workspace.LayerManager.GetLayer(l => l is IFDOGraphicsLayer).ToArray();
            for (int i = 0; i < annoLayers.Length; i++)
            {
                IFeatureClass annoFC = (annoLayers[i] as IFeatureLayer).FeatureClass;

                int selStateIndex = annoFC.FindField(selStateFN);
                if (selStateIndex == -1)
                {
                    continue;
                }
                string annoClassIDFN = GApplication.Application.TemplateManager.getFieldAliasName("AnnotationClassID", annoFC.AliasName);
                int annoClassIDIndex = annoFC.FindField(annoClassIDFN);
                if (annoClassIDIndex == -1)
                {
                    continue;
                }

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("{0} = {1} and {2} is null", annoClassIDFN, fc.ObjectClassID, selStateFN);
                IFeatureCursor fCursor = annoFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    IAnnotationFeature2 annoFe = f as IAnnotationFeature2;
                    if (unselectOIDList.Contains(annoFe.LinkedFeatureID))
                    {
                        f.set_Value(selStateIndex, unSelText);
                        fCursor.UpdateFeature(f);
                    }
                }
                Marshal.ReleaseComObject(fCursor);

            }
        }

    }
}

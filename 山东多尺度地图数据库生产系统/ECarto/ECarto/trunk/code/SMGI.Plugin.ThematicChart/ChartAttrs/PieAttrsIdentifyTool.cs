using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using SMGI.Common;
using System.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
 
namespace SMGI.Plugin.ThematicChart.ChartAttrs
{
    //饼状图属性监听
    public class PieAttrsIdentifyTool :SMGITool
    {
        private IActiveView pAc;
        private IFeatureLayer pTableLayer=null;//监听自由表达图层
        private double mapScale;
        private Dictionary<int, string> feAttrs = new Dictionary<int, string>();
        ILayer repLayer = null;
        IFeatureClass repFcl = null;
        public PieAttrsIdentifyTool()
        {
        }

        public override bool Enabled
        {
            get
            {

                return m_Application != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }


        public override void OnClick()
        {
            pAc = m_Application.ActiveView;
            mapScale = (m_Application.ActiveView as IMap).ReferenceScale;

            repLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOINT"))).FirstOrDefault();
            repFcl = (repLayer as IFeatureLayer).FeatureClass;
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;
            if (mapScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;

            }
            if(repFcl.FindField("JsonTxt")==-1)//不存在
                return;
            //锚点
            IRubberBand rb = new RubberEnvelopeClass();
            IEnvelope pEnvelope = rb.TrackNew(pAc.ScreenDisplay, null) as IEnvelope;
            if (pEnvelope.IsEmpty)
            {
                IPoint anchorPoint = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                IGeometry pgeo= (anchorPoint as ITopologicalOperator).Buffer(m_Application.ActiveView.FocusMap.MapScale * 2.0e-3);
                pEnvelope = pgeo.Envelope;
            }
            ISpatialFilter pSpatialFilter = null;
            pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = pEnvelope;
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
         
            IFeatureCursor pcursor = repFcl.Search(pSpatialFilter, false);
            IFeature fe = pcursor.NextFeature();
            if (fe != null)
            {
                m_Application.ActiveView.FocusMap.SelectFeature(repLayer, fe);
                pAc.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, pAc.Extent);
       
                string jsontext = fe.get_Value(repFcl.FindField("JsonTxt")).ToString();
                var themap=JsonHelper.GetPieInfo(jsontext) ;
                if (themap == null)
                    return;
                DialogResult dr = DialogResult.None;
                string jsonTxt = "";
                if (themap.ThematicType.Contains("饼图"))
                {
                    FrmPieAttribute frm = new FrmPieAttribute(jsontext);
                    dr = frm.ShowDialog();
                    jsonTxt= frm.Json;
                }
                else if (themap.ThematicType.Contains("柱状图"))
                {
                    if (themap.ThematicType == "分类柱状图")
                    {
                        FrmColumnClAttribute frm = new FrmColumnClAttribute(jsontext);
                        dr = frm.ShowDialog();
                        jsonTxt = frm.Json;
                    }
                    else
                    {
                        FrmColumnAttribute frm = new FrmColumnAttribute(jsontext);
                        dr = frm.ShowDialog();
                        jsonTxt = frm.Json;
                    }
                }               
                if (dr == DialogResult.OK)
                {
                    m_Application.EngineEditor.StartOperation();
                    JsonHelper.UpdateFeature(fe, jsonTxt);
                    fe.Delete();
                    m_Application.EngineEditor.StopOperation("专题图属性编辑");
                    m_Application.ActiveView.Refresh();
                }
            }
         
        }

         
        
    }
}
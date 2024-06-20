using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Xml.Linq;
using ESRI.ArcGIS.Display;
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class BridgeSizeCmd : SMGI.Common.SMGICommand
    {
        public BridgeSizeCmd()
        {
            m_category = "桥符号处理";
            m_caption = "桥符号处理";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                      m_Application.Workspace != null &&
                      m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        private IFeatureLayer mLyrBridge = null;
        public override void OnClick()
        {
            //从配置文件获取地名图层名
            FrmBridgeSize frm = new FrmBridgeSize();
            if (DialogResult.OK != frm.ShowDialog())
                return;

            mLyrBridge = frm.LyrBridge as IFeatureLayer;
            double scale = frm.Tolerance;
            
            m_Application.EngineEditor.StartOperation();
            try
            {
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "GB = 450306 AND NAME LIKE '%桥%'";    
                IFeatureCursor featureCursor = mLyrBridge.FeatureClass.Update(qf, false);
                IMapContext mapContext = new MapContextClass();
                mapContext.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, (mLyrBridge as IGeoFeatureLayer).AreaOfInterest);
                IFeature fe;
                while ((fe = featureCursor.NextFeature()) != null)
                {
                    OverrideMarkerSizeValue(featureCursor, fe, scale, mapContext);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
                m_Application.EngineEditor.StopOperation("桥梁处理");
                m_Application.ActiveView.Refresh();
                
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
        public void OverrideMarkerSizeValue(IFeatureCursor fCursor, IFeature feature, double scale, IMapContext mapContext)
        {

            IRepresentationRenderer repRender = (mLyrBridge as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass repClass = repRender.RepresentationClass;
            IRepresentationRules repRules = repClass.RepresentationRules;
            IRepresentation repFeature = repClass.GetRepresentation(feature, mapContext);
            IRepresentationRule rule = repRules.get_Rule(repFeature.RuleID);
            IBasicMarkerSymbol basicMarkerSymbol = null;
            for (int i = 0; i < rule.LayerCount; i++)
            {
                basicMarkerSymbol = rule.get_Layer(i) as IBasicMarkerSymbol;
                if (basicMarkerSymbol != null)
                    break;
            }
            if (basicMarkerSymbol == null)
                return;
            IGraphicAttributes markerAttributes = basicMarkerSymbol as IGraphicAttributes;
            int id = markerAttributes.get_IDByName("Size");
            double markerSize = (double)markerAttributes.get_Value(id);         //磅转毫米
            markerSize = markerSize * scale;
            repFeature.set_Value(markerAttributes, 2, markerSize);//设置尺寸
            repFeature.RepresentationClass.RepresentationRules.set_Rule(repFeature.RuleID, rule);
            repFeature.UpdateFeature();
            fCursor.UpdateFeature(feature);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(repFeature);
        }
  
        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            //从配置文件获取地名图层名
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using System.Timers;
using System.Xml.Linq;

namespace SMGI.Plugin.EmergencyMap
{
    public class SymbolManualProcessCmd : SMGI.Common.SMGITool
    {
        
        private string _targetFCName;
        private string _levelFieldName;
        private IGeoFeatureLayer _geoLayer;

        public SymbolManualProcessCmd()
        {
            m_category = "符号冲突处理";
            m_caption = "符号冲突处理";

            _geoLayer = null;
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
            //从配置文件获取地名图层名
            XElement expertiseContent = ExpertiseDatabase.getContentElement(m_Application);
            XElement sp = expertiseContent.Element("SymbolProcess");
            _targetFCName = sp.Element("FCName").Value;
            _levelFieldName = sp.Element("LevelFieldName").Value;

            var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == _targetFCName.ToUpper();
            })).FirstOrDefault();

            _geoLayer = lyr as IGeoFeatureLayer;
            if (_geoLayer != null)
            {
                //将该图层设置为可选择
                _geoLayer.Selectable = true;
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }

            if (_geoLayer == null)
                return;

            IScreenDisplay screenDisplay = m_Application.ActiveView.ScreenDisplay;
            IRubberBand rubberBand = new ESRI.ArcGIS.Display.RubberRectangularPolygonClass();
            ESRI.ArcGIS.Geometry.IGeometry selectGeometry = rubberBand.TrackNew(screenDisplay, null);

            m_Application.MapControl.Map.SelectByShape(selectGeometry, null, false);
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, selectGeometry.Envelope);

            ISelection selection = m_Application.MapControl.Map.FeatureSelection;
            IEnumFeature mapEnumFeature = selection as IEnumFeature;
            IFeature feature = null;
            mapEnumFeature.Reset();
            m_Application.EngineEditor.StartOperation();
            

            try
            {
                List<IFeature> featurelist = new List<IFeature>();
              
                while ((feature = mapEnumFeature.Next()) != null)
                {
                    if (feature.Class.AliasName == _targetFCName)
                    {
                        featurelist.Add(feature);
                    }
                }

                SymbolConflictProcess p = new SymbolConflictProcess(m_Application);


                IFeatureClass fc = _geoLayer.FeatureClass;

                int selStateIndex = fc.Fields.FindField(p.SelStateFieldName);
                if (selStateIndex == -1)
                {
                    MessageBox.Show(_targetFCName + "缺少字段" + p.SelStateFieldName + "\r\n请关闭编辑添加字段", "提示", MessageBoxButtons.OK);
                    return;
                }

                p.DupSymbolProcess(_geoLayer, _levelFieldName, m_Application.MapControl.ReferenceScale, featurelist);

                m_Application.EngineEditor.StopOperation("符号冲突处理(交互)");

                //刷新
                m_Application.MapControl.Map.ClearSelection();
                m_Application.ActiveView.Refresh();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.ToString());
                m_Application.EngineEditor.AbortOperation();

            }
        
        }

        private void showStatus(object s,ElapsedEventArgs e)
        {
            m_Application.MainForm.ShowStatus("符号冲突处理(交互) 完成");
        }
       
    }
}

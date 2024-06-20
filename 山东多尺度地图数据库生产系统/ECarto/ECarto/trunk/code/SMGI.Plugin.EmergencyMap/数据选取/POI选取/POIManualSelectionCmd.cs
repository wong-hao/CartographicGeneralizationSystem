using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;

namespace SMGI.Plugin.EmergencyMap
{
    public class POIManualSelectionCmd : SMGI.Common.SMGITool
    {
        private List<POISelection.POISelectionInfo> _poiSelectionInfoList;
        private List<POISelection.POISelectionInfo> _poiSelectionInfoListEx;
        Dictionary<string, List<string>> _fcName2filterNames;
        int _weightScale;

        public POIManualSelectionCmd()
        {
            _poiSelectionInfoList = null;
            _fcName2filterNames = null;
            NeedSnap = false;
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState== esriEngineEditState.esriEngineStateEditing;
            }
        }
        private Dictionary<string, int> displayDic = null;
        bool attachMap = false;
        public override void OnClick()
        {  
            Dictionary<string, string> envString = m_Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            attachMap = false;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            if(envString.ContainsKey("AttachMap"))
                attachMap = bool.Parse(envString["AttachMap"]);
            if (attachMap)
            {
                var frm = new POISelectionFormEx(m_Application);
                if (DialogResult.OK == frm.ShowDialog())
                {
                    _poiSelectionInfoList = frm.POISelectionInfoList;
                    _poiSelectionInfoListEx = frm.POISelectionInfoListEx;
                    displayDic = frm.fclDisplayDic;
                    //读取过滤文件;
                    _fcName2filterNames = POIHelper.getFilterNamesOfPOISelection(frm.FilterFileName);
                    _weightScale = frm.WeightScale;
                }
            }
            else
            {
                POISelectionForm frm = new POISelectionForm(m_Application);
                if (DialogResult.OK == frm.ShowDialog())
                {
                    _poiSelectionInfoList = frm.POISelectionInfoList;
                    displayDic = frm.fclDisplayDic;
                    //读取过滤文件;
                    _fcName2filterNames = POIHelper.getFilterNamesOfPOISelection(frm.FilterFileName);
                    _weightScale = frm.WeightScale;
                }
            }
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }

            var view = m_Application.ActiveView;
            IMap map = view.FocusMap;
            var editor = m_Application.EngineEditor;
            POIHelper poiHelper = new POIHelper(m_Application);

            //画范围
            IRubberBand pRubberBand = new RubberPolygonClass();
         
            var geo = pRubberBand.TrackNew(view.ScreenDisplay, null);

            if (geo == null || geo.IsEmpty)
            {
                return;
            }

            if (_poiSelectionInfoList == null)
                return;

            editor.StartOperation();

            POISelection poiSel = new POISelection(m_Application, displayDic);
            string err = string.Empty;

            if (attachMap)
            {
                //err = poiSel.manualSelectEx(_poiSelectionInfoList, _poiSelectionInfoListEx, geo, _fcName2filterNames);
                err = poiSel.POISelect(_poiSelectionInfoList, _poiSelectionInfoListEx, _fcName2filterNames, _weightScale, geo);
            }
            else
            {
                //err = poiSel.manualSelect(_poiSelectionInfoList, geo, _fcName2filterNames);
                err = poiSel.POISelect(_poiSelectionInfoList, null, _fcName2filterNames, _weightScale, geo);
            }
            

            editor.StopOperation("POI选取");

            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err);
            }

            view.Refresh();
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            switch (keyCode)
            {
                case 32:
                    if (attachMap)
                    {
                        var frm = new POISelectionFormEx(m_Application);
                        if (DialogResult.OK == frm.ShowDialog())
                        {
                            _poiSelectionInfoList = frm.POISelectionInfoList;
                            _poiSelectionInfoListEx = frm.POISelectionInfoListEx;
                            displayDic = frm.fclDisplayDic;
                            //读取过滤文件;
                            _fcName2filterNames = POIHelper.getFilterNamesOfPOISelection(frm.FilterFileName);
                        }
                    }
                    else
                    {
                        POISelectionForm frm = new POISelectionForm(m_Application);
                        if (DialogResult.OK == frm.ShowDialog())
                        {
                            _poiSelectionInfoList = frm.POISelectionInfoList;
                            displayDic = frm.fclDisplayDic;
                            //读取过滤文件;
                            _fcName2filterNames = POIHelper.getFilterNamesOfPOISelection(frm.FilterFileName);
                        }
                    }
                    break;
                default:
                    break;
            }
        }


    }
}

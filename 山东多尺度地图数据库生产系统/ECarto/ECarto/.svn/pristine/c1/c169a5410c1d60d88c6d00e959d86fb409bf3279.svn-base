using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;

namespace SMGI.Plugin.GeneralEdit
{
    public class LineCutPolygon : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    if (0 == m_Application.MapControl.Map.SelectionCount)
                        return false;
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = mapEnumFeature.Next();

                    bool res = false;
                    int nSelLine = 0;
                  
                    while (feature != null)
                    {
                        nSelLine++;
                        feature = mapEnumFeature.Next();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
                    if (nSelLine > 1)
                    {
                        res = true;
                    }
                    return res;
                }
                else
                    return false;
            }
        }
        public override void OnClick()
        {
            var map = m_Application.ActiveView.FocusMap;
            var selectFeas = map.FeatureSelection as IEnumFeature;
            IFeature polyline = null;
            IFeature polygon = null;
            IFeature fe = null;
            m_Application.EngineEditor.StartOperation();
            while ((fe = selectFeas.Next()) != null)
            {
                if (fe.Shape is IPolyline)
                {
                    polyline = fe;
                }
                if (fe.Shape is IPolygon)
                {
                    polygon = fe;
                }
            }
            IFeatureEdit feEdit = (IFeatureEdit)polygon;
            try
            {
                var feSet = feEdit.Split(polyline.Shape);
                if (feSet != null)
                {
                    feSet.Reset();
                }
            }
            catch (Exception ex)
            {
                m_Application.EngineEditor.AbortOperation();
            } m_Application.EngineEditor.StopOperation("选择线裁面");
        }
    }
}

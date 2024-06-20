using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.GeneralEdit
{
    public class PolygonCutPolygonTool : SMGI.Common.SMGITool
    {
        private int mID;
        IGeometry trackGeometry;
        IFeature feature;
        IFeatureClass feaFC;
        IFeatureWorkspace feawork;
        public PolygonCutPolygonTool()
        {
            m_caption = "画面裁面";
            mID = -999;
            NeedSnap = true;
        }
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
                    feature = mapEnumFeature.Next();

                    bool res = false;
                    int nSelLine = 0;
                    while (feature != null)
                    {
                        if (feature.Shape is IPolygon)
                        {
                            nSelLine++;
                            break;

                        } feature = mapEnumFeature.Next();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
                    if (nSelLine > 0)
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
            feawork = m_Application.Workspace.EsriWorkspace as IFeatureWorkspace;
            feaFC = feawork.OpenFeatureClass(feature.Class.AliasName);
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            trackGeometry = m_Application.MapControl.TrackPolygon();
            if (null == trackGeometry || trackGeometry.IsEmpty)
                return;
            m_Application.EngineEditor.StartOperation();
            IPolygon polygon = feature.ShapeCopy as IPolygon;
            IFeature feaNew = feaFC.CreateFeature();

            feaNew.Shape = trackGeometry;
            IFields fields = feature.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                IField field = fields.get_Field(i);

                if (!(field as IFieldEdit).Editable)
                {
                    continue;
                }
                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    continue;
                }
                feaNew.set_Value(i, feature.get_Value(i));
            }
            feaNew.Store();

            IGeometry geo = (polygon as ITopologicalOperator).Difference(trackGeometry);
            feature.Shape = geo;
            feature.Store();
            m_Application.EngineEditor.StopOperation("画面裁面");
            m_Application.ActiveView.Refresh();

        }
        public override void OnMouseMove(int button, int shift, int x, int y)
        {
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
        }
    }
}

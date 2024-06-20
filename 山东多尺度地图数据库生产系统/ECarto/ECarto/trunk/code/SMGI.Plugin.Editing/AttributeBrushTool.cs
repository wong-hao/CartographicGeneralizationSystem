using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.GeneralEdit
{
    public class AttributeBrushTool:SMGI.Common.SMGITool
    {
        private FeatureStruct featureStruct;
        /// <summary>
        /// 进行属性的刷制
        /// </summary>
        public AttributeBrushTool() 
        {
            m_caption = "属性刷";
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Brush.cur"));
            m_category = "编辑工具";
            m_toolTip = "将选择的要素属性复制到另一个要素上";

            NeedSnap = false;
        }
        public override void OnClick()
        {
            featureStruct = new FeatureStruct();
            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();
            IFeature feature = mapEnumFeature.Next();
            featureStruct.FCName = feature.Class.AliasName;
            for (int i = 0; i < feature.Fields.FieldCount; i++) 
            {
                IField field=feature.Fields.get_Field(i);
                if (field .Type!= esriFieldType.esriFieldTypeOID && field.Editable && field.Name.ToLower() != "shape")
                {
                    featureStruct.FieldInfo.Add(field.Name, field.Type);
                    featureStruct.FieldValue.Add(field.Name, feature.get_Value(i));
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            IPoint point = m_Application.MapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            ITopologicalOperator topoOper = point as ITopologicalOperator;
            IGeometry geometry = null;


            if (m_Application.Workspace.Map.SpatialReference is IGeographicCoordinateSystem)
            {
                geometry = topoOper.Buffer(5 * 0.000009);
            }
            else
            {
                geometry = topoOper.Buffer(5);
            }

            
            m_Application.MapControl.Map.SelectByShape(geometry, null, true);
            m_Application.MapControl.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewAll, null, geometry.Envelope);
            IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            mapEnumFeature.Reset();
            IFeature selectFeature = mapEnumFeature.Next();
            if (selectFeature == null)
                return;
            m_Application.EngineEditor.StartOperation();
            for (int i = 0; i < selectFeature.Fields.FieldCount; i++) 
            {
                IField field = selectFeature.Fields.get_Field(i);
                if (field.Editable && field.Name.ToLower() != "shape")
                {
                    foreach (string fieldName in featureStruct.FieldInfo.Keys) 
                    {
                        if (field.Name == fieldName && field.Type == featureStruct.FieldInfo[fieldName]) 
                        {
                            selectFeature.set_Value(i, featureStruct.FieldValue[fieldName]);
                            
                        }
                    }
                }
            }
            selectFeature.Store();

            m_Application.EngineEditor.StopOperation("属性刷属性");
            System.Runtime.InteropServices.Marshal.ReleaseComObject(selectFeature);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
        }
        public override bool Enabled
        {
            get
            {
                if (m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    IEnumFeature mapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                    mapEnumFeature.Reset();
                    IFeature feature = mapEnumFeature.Next();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
                    if (feature != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }
    }
    /// <summary>
    /// 要素集的数据结构
    /// </summary>
    public class FeatureStruct
    {
        /// <summary>
        /// 要素名称
        /// </summary>
        public string FCName;
        /// <summary>
        /// 字段信息
        /// </summary>
        public Dictionary<string, esriFieldType> FieldInfo = new Dictionary<string, esriFieldType>();
        /// <summary>
        /// 字段值
        /// </summary>
        public Dictionary<string, object> FieldValue = new Dictionary<string, object>();
    }

}

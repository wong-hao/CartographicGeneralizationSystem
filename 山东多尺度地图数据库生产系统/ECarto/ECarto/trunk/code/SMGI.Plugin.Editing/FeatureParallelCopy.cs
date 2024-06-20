﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.GeneralEdit
{
    public class FeatureParallelCopy : SMGI.Common.SMGICommand
    {
        public FeatureParallelCopy()
        {
            m_caption = "要素平行复制";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       1 == m_Application.MapControl.Map.SelectionCount &&
                       m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        public override void OnClick() 
        {
            FeatureParallelCopyForm frm = new FeatureParallelCopyForm();
            if (frm.ShowDialog() != DialogResult.OK) 
            {
                return;
            }
            
            ISelection selectfeature = m_Application.MapControl.Map.FeatureSelection;
            IEnumFeature enumFeature = (IEnumFeature)selectfeature;
            enumFeature.Reset();
            IFeature feature = null;
            m_Application.EngineEditor.StartOperation();
            while ((feature = enumFeature.Next()) != null) 
            {
                IGeometry pGeometry = feature.ShapeCopy as IGeometry;
                
                IObjectClass pObjectFeature = feature.Class;
                IFeatureClass pFeatureclass = pObjectFeature as IFeatureClass;
                IConstructCurve pOffset = null;
                if (pFeatureclass.ShapeType == esriGeometryType.esriGeometryPolygon)
                {
                    pOffset = new PolygonClass();
                }
                else if (pFeatureclass.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    pOffset = new PolylineClass();
                }
                if (pOffset == null)
                    continue;
              

                double lengthof = frm.OffsetDis;
                pOffset.ConstructOffset(pGeometry as IPolycurve, lengthof, esriConstructOffsetEnum.esriConstructOffsetSimple);
             
                IFeature pFeature = pFeatureclass.CreateFeature();
                pFeature.Shape = pOffset as IGeometry;
                int num = feature.Fields.FieldCount;
                for (int i = 0; i < num; i++)
                {
                    try
                    {
                        if (feature.Fields.get_Field(i).Type == esriFieldType.esriFieldTypeOID)
                            continue;
                        if (feature.Fields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                            continue;
                        if (feature.Fields.get_Field(i).Name.ToUpper().Contains("SHAPE"))
                            continue;
                        if (feature.get_Value(i) != DBNull.Value)
                        {
                            //string sO = (feature.get_Value(i)).ToString();
                            pFeature.set_Value(i, feature.get_Value(i));
                            pFeature.Store();
                        }
                    }
                    catch (Exception e) { continue; }
                }
            }
            m_Application.EngineEditor.StopOperation("平行复制");
            System.Runtime.InteropServices.Marshal.ReleaseComObject(selectfeature);
            m_Application.ActiveView.Refresh();
        }
    }
}

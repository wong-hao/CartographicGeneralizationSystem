using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;


namespace SMGI.Plugin.GeneralEdit
{
    public class ReShapeByOther:SMGITool
    {
        public ReShapeByOther()
        {
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
        }

        public override void OnClick()
         {
             var view = m_Application.ActiveView;
             using (var sff = new SelectFeatureForm(view))
             {
                 sff.Text = "选择套合目标";
                 if (sff.ShowDialog() != DialogResult.OK)
                 {
                     return;
                 }
                 FeatureWarp w = sff.FeaturesListBox.SelectedItem as FeatureWarp;
                 if (w == null)
                     return;

                 if (!(w.Feature.Shape is IPolyline || w.Feature.Shape is IPolygon))
                 {
                     return;
                 }
                 IGeometryCollection gc = w.Feature.Shape as IGeometryCollection;

                 var editor = m_Application.EngineEditor;
                 editor.StartOperation();
                 bool anyoneReshaped = false;
                 for (int i = 0; i < sff.FeaturesListBox.Items.Count; i++)
                 {
                     if (i == sff.FeaturesListBox.SelectedIndex)
                     {
                         continue;
                     }
                     var f = (sff.FeaturesListBox.Items[i] as FeatureWarp).Feature;
                     if (f.Shape is IPolycurve)
                     {
                         IPolycurve curve = f.ShapeCopy as IPolycurve;
                         bool reshaped = false;
                         for (int j = 0; j < gc.GeometryCount; j++)
                         {
                             IPath path = gc.get_Geometry(j) as IPath;
                             reshaped |= curve.Reshape(path);
                         }
                         if (reshaped)
                         {
                             f.Shape = curve;
                             f.Store();
                             anyoneReshaped = true;
                         }
                     }

                 }

                 if (anyoneReshaped)
                 {
                     editor.StopOperation(this.Caption);
                     view.Refresh();
                 }
                 else
                 {
                     editor.AbortOperation();
                     MessageBox.Show("没有任何目标被修改！");
                 }
             }
         }

         public override bool Enabled
         {
             get
             {
                 return m_Application != null
                     && m_Application.Workspace != null
                     && m_Application.ActiveView.FocusMap.SelectionCount >= 2 
                     && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
             }
         }
    }

    static class StaticFunction
    {
        internal static bool Reshape(this IPolycurve curve, IPath path)
        {
            bool reshaped = false;
            if (curve is IPolygon)
            {
                IGeometryCollection area = curve as IGeometryCollection;
                for (int j = 0; j < area.GeometryCount; j++)
                {
                    IRing r = area.get_Geometry(j) as IRing;
                    reshaped |= r.Reshape(path);
                }
            }
            else if (curve is IPolyline)
            {
                reshaped = (curve as IPolyline).Reshape(path);
            }

            return reshaped;
        }
    }
}

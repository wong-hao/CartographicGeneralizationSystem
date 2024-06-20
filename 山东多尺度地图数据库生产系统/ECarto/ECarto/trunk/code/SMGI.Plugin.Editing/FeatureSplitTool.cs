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

namespace SMGI.Plugin.GeneralEdit
{
    public class FeatureSplitTool:SMGITool
    {
        public FeatureSplitTool()
        {
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));
        }

         public override void OnMouseDown(int button, int shift, int x, int y)
         {
             var editor = m_Application.EngineEditor;
             IMap pMap = m_Application.Workspace.Map;

             List<IFeature> FeatureList = new List<IFeature>();
             IEnumFeature pEnumFeature = (IEnumFeature)pMap.FeatureSelection;
             ((IEnumFeatureSetup)pEnumFeature).AllFields = true;
             IFeature feature = null;
             while ((feature = pEnumFeature.Next()) != null)
             {
                 if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline || feature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                 {
                     FeatureList.Add(feature);
                 }
             }

             if (FeatureList.Count == 0)
             {
                 MessageBox.Show("没有选择要素");
                 return;
             }


             IScreenDisplay screenDisplay = m_Application.ActiveView.ScreenDisplay;
             #region Create a symbol to use for feedback

             ISimpleLineSymbol sym = new SimpleLineSymbolClass();
             IRgbColor color = new RgbColorClass();	 //red
             color.Red = 255;
             color.Green = 0;
             color.Blue = 0;

             sym.Color = color;
             sym.Style = esriSimpleLineStyle.esriSLSSolid;
             sym.Width = 2;

             #endregion

             IRubberBand SplitBand = new RubberLineClass();
             IGeometry SplitGeom = SplitBand.TrackNew(screenDisplay, sym as ISymbol);

             if (SplitGeom != null)
             {
                 editor.StartOperation();
                 ITopologicalOperator2 pTopo = (ITopologicalOperator2)SplitGeom;
                 pTopo.IsKnownSimple_2 = false;
                 pTopo.Simplify();

                 for (int i = 0; i < FeatureList.Count; i++)
                 {
                     IFeature SplitedFeature = FeatureList[i];
                     if (SplitedFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                     {
                         IGeometry InterGeo = pTopo.Intersect(SplitedFeature.Shape, esriGeometryDimension.esriGeometry0Dimension);
                         if (InterGeo != null)
                         {
                             if (InterGeo.IsEmpty == false)
                             {
                                 IPointCollection pPointColl = (IPointCollection)InterGeo;
                                 SplitGeom = pPointColl.get_Point(0);
                             }
                         }
                     }

                     IFeatureEdit pFeatureEdit = (IFeatureEdit)SplitedFeature;
                     try
                     {
                         ISet pFeatureSet = pFeatureEdit.Split(SplitGeom);
                         if (pFeatureSet != null)
                         {
                             pFeatureSet.Reset();
                             m_Application.ActiveView.Refresh();
                         }
                     }
                     catch (Exception ex)
                     {
                         editor.AbortOperation();
                     }
                 }
                 editor.StopOperation(null);
             }
             System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumFeature);
         }

         public override bool Enabled
         {
             get
             {
                 return m_Application != null
                     && m_Application.Workspace != null
                     && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
             }
         }
    }
}

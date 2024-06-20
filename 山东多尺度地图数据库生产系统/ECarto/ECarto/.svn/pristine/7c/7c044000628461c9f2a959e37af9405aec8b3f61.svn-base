using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.GeneralEdit
{
    public class CustPolygonCommands : SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            var map = m_Application.ActiveView.FocusMap;
            var selection = map.FeatureSelection;
            if (map.SelectionCount == 1)
            {
                IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
                selectEnumFeature.Reset();
                IFeature fe = selectEnumFeature.Next();

                if (fe.Shape.GeometryType != esriGeometryType.esriGeometryPolygon)
                {
                    MessageBox.Show("请先选择一个面要素");
                    return;
                }
                deleteIntersectPolygon(fe);
            }        
       }

        private void deleteIntersectPolygon(IFeature polygonFeature)
        {
            

            try
            {
                
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon;
                })).ToArray();
                ISpatialFilter qf;
                IFeatureCursor cursor;
                IFeature fe;
                var shapePolygon = polygonFeature.Shape as IPolygon;
                var shapePolyline = (polygonFeature.ShapeCopy as ITopologicalOperator).Boundary as IPolyline;
                if (lyrs.Length > 0)
                {
                    m_Application.EngineEditor.StartOperation();

                    IRelationalOperator trackRel = shapePolygon as IRelationalOperator;

                    foreach (var item in lyrs)
                    {
                        qf = new SpatialFilter();
                        qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        qf.Geometry = shapePolygon as IGeometry;
                        qf.GeometryField = "SHAPE";
                        cursor = (item as IGeoFeatureLayer).FeatureClass.Search(qf, false);
                        while ((fe = cursor.NextFeature()) != null)
                        {
                            //cursor.DeleteFeature();

                            //ITopologicalOperator pTopo = fe.Shape as ITopologicalOperator;
                            //pTopo.Clip(shapePolygon.Envelope);

                            if (trackRel.Contains(fe.Shape as IGeometry))
                            {
                                fe.Delete();

                                continue;
                            }
                            IFeatureEdit feEdit = (IFeatureEdit)fe;
                            ISet pFeatureSet = feEdit.Split(shapePolyline);
                            if (pFeatureSet != null)
                            {
                                pFeatureSet.Reset();

                                while (true)
                                {
                                    IFeature newFe = pFeatureSet.Next() as IFeature;
                                    if (newFe == null)
                                    {
                                        break;
                                    }
 
                                    if (trackRel.Contains(newFe.Shape as IGeometry))
                                    {
                                        newFe.Delete();
                                    }

                                }
                            }

                            

                        }
                        Marshal.ReleaseComObject(cursor);
                        
                    }

                    m_Application.ActiveView.Refresh();

                    m_Application.EngineEditor.StopOperation("扣面");
                }

            }
            catch(Exception ex)
            {
                m_Application.EngineEditor.AbortOperation();
            }
        }
    }
}

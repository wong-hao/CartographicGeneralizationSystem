using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common.Algrithm;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.MapGeneralization
{
    public class PolygonComprehensive : SMGI.Common.SMGICommand
    {
        IFeatureWorkspace featureWorkspace;
        public PolygonComprehensive()
        { }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null 
                    && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        public override void OnClick()
        {
            //Progress progWindow = new Progress();
            //progWindow.StartPosition = FormStartPosition.CenterScreen;
            //progWindow.Show();
            FrmSimplify frm = new FrmSimplify();
            double width = 0, heigth = 0;
            if (frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            width = frm.width;
            heigth = frm.heigth;
            IWorkspace workspace = m_Application.Workspace.EsriWorkspace;
            featureWorkspace = (IFeatureWorkspace)workspace;

            List<IFeature> myFeature = new List<IFeature>();
            ISelection selectfeature1 = m_Application.MapControl.Map.FeatureSelection;
            IEnumFeature enumFeature1 = (IEnumFeature)selectfeature1;
            enumFeature1.Reset();
            IFeature feature3 = null;
            while ((feature3 = enumFeature1.Next()) != null) 
            {
                myFeature.Add(feature3);
            }
            if (myFeature.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("请选择数据！");
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
                bool pand = true;
                
                IFeature feature2 = null;
                foreach (var feature1 in myFeature)
                {
                    IGeometry pGeometry1 = feature1.ShapeCopy as IGeometry;
                    IRelationalOperator pRelOperator = pGeometry as IRelationalOperator;
                    if (pRelOperator.Touches(pGeometry1))
                    {
                        pand = false;
                        feature2 = feature1;
                    }
                }
                if (pand == true)
                {
                    var pl = SimplifyByDTAlgorithm.SimplifyByDT(feature.ShapeCopy as IPolycurve, width, heigth);
                    feature.Shape = pl as IGeometry;
                    feature.Store();
                }
                if (pand == false)
                {
                    IGeometry pGeometry2 = feature.ShapeCopy as IGeometry;
                    IPolygon g1 = feature2.ShapeCopy as IPolygon;
                    ITopologicalOperator feaTopoI = pGeometry2 as ITopologicalOperator;
                    (g1 as ITopologicalOperator).Simplify();
                    IGeometry pGeometry3 = feaTopoI.Union(g1);
                    feaTopoI.Simplify();

                    var pl = SimplifyByDTAlgorithm.SimplifyByDT(feature.ShapeCopy as IPolycurve, width, heigth);
                    feature.Shape = pl as IGeometry;
                    feature.Store();
                    IPolygon mPolygon1 = feature.ShapeCopy as IPolygon;

                    IPolygon cut = ((pGeometry3 as IPolygon) as ITopologicalOperator).Difference(mPolygon1) as IPolygon;
                    feature2.Shape = cut;
                    feature2.Store();
                }
            }
            m_Application.ActiveView.Refresh();
            m_Application.EngineEditor.StopOperation("面综合");
            System.Windows.Forms.MessageBox.Show("数据处理完成！");
        }
    }
}

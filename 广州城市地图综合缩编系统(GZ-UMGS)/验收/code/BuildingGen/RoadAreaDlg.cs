using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
namespace BuildingGen
{
    public partial class RoadAreaDlg : Form
    {
        GApplication m_app;
        public RoadAreaDlg(GApplication app)
        {
            InitializeComponent();
            m_app = app;
            GetList(LineLayer, m_app, esriGeometryType.esriGeometryPolyline);
            GetList(AreaLayer, m_app, esriGeometryType.esriGeometryPolygon);
        }
        private void GetList(ComboBox cb,GApplication app,esriGeometryType gtype)
        {
            cb.Items.Clear();
            if (app == null)
            {
                return;
            }
            if (app.Workspace == null)
            {
                return;
            }
            foreach (GLayerInfo info in app.Workspace.LayerManager.Layers)
            {
                if(info.Layer is IFeatureLayer
                    && (info.Layer as IFeatureLayer).FeatureClass.ShapeType == gtype)
                cb.Items.Add(info);
            }

            if (cb.Items.Count > 0)
            {
                cb.SelectedIndex = 0;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            GLayerInfo areaInfo = AreaLayer.SelectedItem as GLayerInfo;
            IFeatureClass areaFc = (areaInfo.Layer as IFeatureLayer).FeatureClass;
            GLayerInfo lineInfo = LineLayer.SelectedItem as GLayerInfo;
            IFeatureClass lineFc = (lineInfo.Layer as IFeatureLayer).FeatureClass;

            int fieldID = lineFc.Fields.FindField("面积");
            if (fieldID == -1)
            {
                return;
            }
            progressBar1.Step = 1;
            progressBar1.Maximum = lineFc.FeatureCount(null) - 1;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            IFeatureCursor lineCursor = lineFc.Update(null, true);
            IFeature lineFeature;
            while ((lineFeature = lineCursor.NextFeature()) != null)
            {
                lineFeature.set_Value(fieldID, 0);
                lineCursor.UpdateFeature(lineFeature);
                progressBar1.PerformStep();
                label3.Text = string.Format("正在初始化面积{0}:{1}", progressBar1.Value, progressBar1.Maximum);
            }
            lineCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(lineCursor);

            progressBar1.Value = 0;
            progressBar1.Maximum = areaFc.FeatureCount(null) - 1;
            IFeatureCursor areaCursor = areaFc.Search(null, true);
            IFeature areaFeature;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            Dictionary<IFeature, double> lineFeatures = new Dictionary<IFeature, double>();

            while ((areaFeature = areaCursor.NextFeature())!= null)
            {
                sf.Geometry = areaFeature.Shape;
                lineCursor = lineFc.Search(sf, false);

                lineFeatures.Clear();
                double length = 0;
                while ((lineFeature = lineCursor.NextFeature()) != null)
                {
                    IPolyline intersectLine = null;
                    try
                    {
                        ITopologicalOperator opa = areaFeature.ShapeCopy as ITopologicalOperator;
                        if (!opa.IsSimple)
                        {
                            opa.Simplify();
                        }
                        ITopologicalOperator opl = lineFeature.ShapeCopy as ITopologicalOperator;
                        if (!opl.IsSimple)
                        {
                            opl.Simplify();
                        }
                        intersectLine = (opa).Intersect(opl as IGeometry, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                    }
                    catch
                    { 
                    }
                    if (intersectLine != null && !intersectLine.IsEmpty)
                    {
                        lineFeatures.Add(lineFeature, intersectLine.Length);
                        length += intersectLine.Length;
                    }
                }
                double area = (areaFeature.Shape as IArea).Area;
                foreach (IFeature item in lineFeatures.Keys)
                {
                    double a = (double)item.get_Value(fieldID);
                    item.set_Value(fieldID, a + area * lineFeatures[item] / length);
                    //lineCursor.UpdateFeature(item);
                    item.Store();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(item);
                }
                lineFeatures.Clear();
                //lineCursor.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(lineCursor);
                progressBar1.PerformStep();
                label3.Text = string.Format("正在计算面积{0}:{1}", progressBar1.Value, progressBar1.Maximum);
            }

        }
    }
}

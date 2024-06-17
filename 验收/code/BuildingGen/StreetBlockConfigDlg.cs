using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;

namespace BuildingGen {
    public partial class StreetBlockConfigDlg : Form {
        GApplication m_application;
        public StreetBlockConfigDlg(GApplication app) {
            InitializeComponent();
            m_application = app;
        }

        private void btExport_Click(object sender, EventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ShapeFile(*.shp)|*.shp";
            if (sfd.ShowDialog() == DialogResult.OK) {
                tbExport.Text = sfd.FileName;
            }
        }
        AddDataDlg addlg;
        private void inport_Click(object sender, EventArgs e) {
            Button bt = sender as Button;
            if (addlg == null) {
                addlg = new AddDataDlg();
                addlg.LayerType = GCityLayerType.道路;
                addlg.LayerName = "默认名称";
            }
            if (addlg.ShowDialog() != DialogResult.OK)
                return;
            TextBox tb = null;
            if (bt.Tag.ToString() == "road") {
                tb = tbRoad;
            }
            else if (bt.Tag.ToString() == "area") {
                tb = tbArea;
            }
            else if (bt.Tag.ToString() == "building") {
                tb = tbBuilding;
            }
            else {
                return;
            }
            tb.Text = addlg.LayerPath;
            string path = "";
            string name = "";
            IWorkspaceFactory wsf = null;
            if (!tb.Text.Contains("|")) {
                path = System.IO.Path.GetDirectoryName(tb.Text);
                name = System.IO.Path.GetFileNameWithoutExtension(tb.Text);
                wsf = GApplication.ShpFactory;
            }
            else {
                string[] paths = tb.Text.Split(new char[] { '|' });
                path = paths[0];
                name = paths[paths.Length - 1];

                if (GApplication.MDBFactory.IsWorkspace(path)) {
                    wsf = GApplication.MDBFactory;
                }
                else if (GApplication.GDBFactory.IsWorkspace(path)) {
                    wsf = GApplication.GDBFactory;
                }
            }
            IWorkspace ws = wsf.OpenFromFile(path, 0);
            IFeatureClass fc = (ws as IFeatureWorkspace).OpenFeatureClass(name);
            tb.Tag = fc;
            if (bt.Tag.ToString() == "road") {
                cbWidthField.Items.Clear();
                for (int i = 0; i < fc.Fields.FieldCount; i++) {
                    IField field = fc.Fields.get_Field(i);
                    if (field.Type == esriFieldType.esriFieldTypeDouble || field.Type == esriFieldType.esriFieldTypeInteger) {
                        cbWidthField.Items.Add(field.Name);
                    }
                }
            }
        }

        private void rbWidth_CheckedChanged(object sender, EventArgs e) {
            if (rbWidth.Checked) {
                gbStatic.Enabled = false;
                gbWidth.Enabled = true;
            }
            else {
                gbStatic.Enabled = true;
                gbWidth.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e) {
            IFeatureClass fcRoad = tbRoad.Tag as IFeatureClass;
            IFeatureClass fcArea = tbArea.Tag as IFeatureClass;
            IFeatureClass fcBuilding = tbBuilding.Tag as IFeatureClass;

            double staticWidth = 0.0;
            double widthSize = 0.0;
            double bufferRange = 0.0;
            try {
                if (rbStatic.Checked) {
                    staticWidth = Convert.ToDouble(tbStaticSize.Text);
                }
                else {
                    widthSize = Convert.ToDouble(tbWidthSize.Text);
                }
                bufferRange = Convert.ToDouble(tbBufferDis.Text);
            }
            catch {
                MessageBox.Show("请填写必要参数");
                return;
            }

            string shpDir = System.IO.Path.GetDirectoryName(tbExport.Text);
            string shpName = System.IO.Path.GetFileNameWithoutExtension(tbExport.Text);
            IWorkspace ws = GApplication.ShpFactory.OpenFromFile(shpDir, 0);
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFields rfields = ocDescription.RequiredFields;
            IFeatureClass fcExport = (ws as IFeatureWorkspace).CreateFeatureClass(shpName, rfields,
                ocDescription.InstanceCLSID, ocDescription.ClassExtensionCLSID,
                fcDescription.FeatureType, fcDescription.ShapeFieldName, "");

            IFeatureCursor exportCursor = fcExport.Insert(true);


            IFeatureCursor roadCursor = fcRoad.Search(null, true);
            IFeature roadFeature = null;
            IDoubleArray widths = new DoubleArrayClass();
            IBufferConstruction bc = new BufferConstructionClass();
            GeometryBagClass gb = new GeometryBagClass();
            GeometryBagClass outgb = new GeometryBagClass();
            object miss = Type.Missing;
            //ITopologicalOperator roadPoly = null;
            while ((roadFeature = roadCursor.NextFeature()) != null) {                
                gb.AddGeometry(roadFeature.ShapeCopy, ref miss, ref miss);
                double w = staticWidth;
                if (rbStatic.Checked) {
                    widths.Add(staticWidth);
                }
                else {
                    w = (double)roadFeature.get_Value(roadFeature.Fields.FindField(cbWidthField.Text));
                    if (w <= 0)
                        w = 1;
                    widths.Add(w);
                }
                //if (roadPoly == null) {
                //    roadPoly = (roadFeature.Shape as ITopologicalOperator).Buffer(w) as ITopologicalOperator;
                //}
                //else {
                //    roadPoly = roadPoly.Union((roadFeature.Shape as ITopologicalOperator).Buffer(w)) as ITopologicalOperator;
                //}
            }
            bc.ConstructBuffersByDistances2(gb, widths, outgb);

            PolygonClass pc = new PolygonClass();
            pc.ConstructUnion(outgb);

            IFeatureBuffer fb = fcExport.CreateFeatureBuffer();
            //fb.Shape = pc;
            //exportCursor.InsertFeature(fb);
            //exportCursor.Flush();
            //return;

            IFeatureCursor areaCursor = fcArea.Search(null, true);
            IFeature areaFeature = null;
            GeometryBagClass areagb = new GeometryBagClass();
            while ((areaFeature = areaCursor.NextFeature()) != null) {
                areagb.AddGeometry((areaFeature.Shape as ITopologicalOperator).Difference(pc), ref miss, ref miss);
            }
            GeometryBagClass ligb = new GeometryBagClass();
            bc.ConstructBuffers(areagb, -(bufferRange + widthSize), ligb);
            GeometryBagClass lagb = new GeometryBagClass();
            bc.ConstructBuffers(ligb, bufferRange, lagb);

            for (int i = 0; i < lagb.GeometryCount; i++) {
                IPolygon4 poly = lagb.get_Geometry(i) as IPolygon4;
                IGeometryCollection sigc = poly.ConnectedComponentBag as IGeometryCollection;
                for (int j = 0; j < sigc.GeometryCount; j++) {
                    fb = fcExport.CreateFeatureBuffer();
                    fb.Shape = sigc.get_Geometry(j);
                    exportCursor.InsertFeature(fb);
                }
            }
            exportCursor.Flush();
        }

    }
}

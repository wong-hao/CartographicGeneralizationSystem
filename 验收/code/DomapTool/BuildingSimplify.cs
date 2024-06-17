using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesFile;


namespace DomapTool
{
    /// <summary>
    /// Summary description for BuildingSimplify.
    /// </summary>
    [Guid("f5f704ee-b870-4c1a-bebe-2462ddbb9af0")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DomapTool.BuildingSimplify")]
    public sealed class BuildingSimplify : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);

        }

        #endregion
        #endregion
        INewPolygonFeedback fb;
        ESRI.ArcGIS.Geoprocessor.Geoprocessor Geoprosessor;
        IEditor editor;
        IFeatureClass fc2 = null;
        IFeatureLayer buildLayer = null;
        private IApplication m_application;
        public BuildingSimplify()
        {
            //
            // TODO: Define values for the public properties
            //
            Geoprosessor = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            this.Geoprosessor.AddOutputsToMap = false;
            base.m_category = "Domap"; //localizable text 
            base.m_caption = "建筑物化简";  //localizable text 
            base.m_message = "建筑物化简";  //localizable text
            base.m_toolTip = "建筑物化简";  //localizable text
            base.m_name = "DomapTool.BuildingSimplify";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
            try
            {
                //
                // TODO: change resource name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
            GGenPara.Para.RegistPara("建筑物化简容差", (double)5);
            GGenPara.Para.RegistPara("建筑物化简最小内环面积", (double)100);
        }

        #region Overriden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            m_application = hook as IApplication;

            //Disable if it is not ArcMap
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false;
            editor = m_application.FindExtensionByName("ESRI Object Editor") as IEditor;
            // TODO:  Add other initialization code
        }
        public override bool Enabled
        {
            get
            {
                return base.Enabled && editor.EditState == esriEditState.esriStateEditing;
            }
        }
        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        double area;
        double dis;
        IActiveView ac;
        public override void OnClick()
        {
            ac = (m_application.Document as IMxDocument).ActiveView;
            DeleteMinHole d = new DeleteMinHole((m_application.Document as IMxDocument).ActiveView.FocusMap);
            d.Text = "建筑物化简";
            d.label2.Text = "最小内环面积";
            d.label3.Text = "化简容差";
            d.textBox1.Text = GGenPara.Para["建筑物化简最小内环面积"].ToString();
            d.textBox2.Text = GGenPara.Para["建筑物化简容差"].ToString(); 
            if (d.ShowDialog() == DialogResult.Cancel)
                return;
            buildLayer = d.SelectLayer;
            area = Convert.ToDouble(d.textBox1.Text);
            GGenPara.Para["建筑物化简最小内环面积"] = area;
            dis = Convert.ToDouble(d.textBox2.Text);
            GGenPara.Para["建筑物化简容差"] = dis;
            fc2 = buildLayer.FeatureClass;
            if (buildLayer == null)
            {
                MessageBox.Show("请确保建筑物图层不为空!");
                return;
            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button != 1)
                return;
            IPoint p = ac.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb == null)
            {
                fb = new NewPolygonFeedbackClass();
                fb.Display = ac.ScreenDisplay;
                fb.Start(p);
            }
            else
            {
                fb.AddPoint(p);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (fb != null)
            {
                IPoint p = ac.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add BuildingSimplify.OnMouseUp implementation
        }

        public override void OnDblClick()
        {
            if (fb != null)
            {
                IPolygon poly = fb.Stop();
                fb = null;
                if (poly == null || poly.IsEmpty)
                {
                    return;
                }
                Gen2(poly);
            }
        }
        public override void OnKeyDown(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Escape:
                    if (fb != null)
                    {
                        fb.Stop();
                        fb = null;
                    }
                    break;
                case (int)System.Windows.Forms.Keys.R:
                    if (System.Windows.Forms.MessageBox.Show("将要化简分组号为-1的建筑物，是否继续？", "耗时操作提示", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        Gen2(null);
                    break;
            }
        }
        #endregion
        private IFeatureClass CreateLayer(IWorkspace ss,List<IField> fs)
        {
            IFeatureWorkspace fws = ss as IFeatureWorkspace;
            int mm = 1;
            string name = "tp" + mm;           
            while (System.IO.File.Exists(ss.PathName + "\\" + name + ".shp"))
            {
                mm++;
                name = "tp" + mm;
            }
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFields rfields = ocDescription.RequiredFields;
            if (fs != null)
                foreach (IField f in fs)
                {
                    (rfields as IFieldsEdit).AddField(f);
                }
            try
            {
                return fws.CreateFeatureClass(name, rfields,
                    ocDescription.InstanceCLSID, ocDescription.ClassExtensionCLSID,
                    fcDescription.FeatureType, fcDescription.ShapeFieldName, "");
            }
            catch
            {
                return null;
            }

        }

        private void Gen2(IPolygon range)
        {
            string tempPath = System.Environment.GetEnvironmentVariable("TEMP");
            System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(tempPath);
            tempPath = info.FullName;
            IWorkspaceFactory shpfileWSF = new ShapefileWorkspaceFactoryClass();
            IWorkspace ws = shpfileWSF.OpenFromFile(tempPath, 0);

            IStatusBar sBar = m_application.StatusBar;
            IStepProgressor sPro = sBar.ProgressBar;
            sPro.Position = 0;

            IMap map = ac.FocusMap;
            IFeatureLayer layer = buildLayer;
            if (layer == null)
            {
                return;
            }
            IFeatureClass fc = layer.FeatureClass;
            IQueryFilter simpleQf = null;
            int groupID = fc.FindField("G_Group");

            if (range != null)
            {
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = range;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                if (groupID >= 0)
                    sf.WhereClause = "G_Group <> -3";
                simpleQf = sf;
            }
            else
            {
                if (groupID < 0)
                    return;
                IQueryFilter iqf = new QueryFilterClass();
                iqf.WhereClause = "G_Group = -1";
                simpleQf = iqf;
            }

            List<IField> fields = new List<IField>();
            IFieldEdit2 field = new FieldClass();
            field.Name_2 = "simpoid";
            field.Type_2 = esriFieldType.esriFieldTypeInteger;
            fields.Add(field as IField);
            field = new FieldClass();
            field.Name_2 = "dx";
            field.Type_2 = esriFieldType.esriFieldTypeDouble;
            fields.Add(field as IField);
            field = new FieldClass();
            field.Name_2 = "dy";
            field.Type_2 = esriFieldType.esriFieldTypeDouble;
            fields.Add(field as IField);
            //IFeatureClass fc_tp = m_application.Workspace.LayerManager.TempLayer(fields);
            IFeatureClass fc_tp = CreateLayer(ws, fields);

            IFeatureCursor insert_Cursor = fc_tp.Insert(true);
            IFeatureCursor checkCursor = fc.Search(simpleQf, true);
            IFeature checkFeature = null;
            List<int> ids = new List<int>();
            Dictionary<int, IPoint> fid_dxy_dic = new Dictionary<int, IPoint>();
            object miss = Type.Missing;
            int wCount = fc.FeatureCount(simpleQf);
            sBar.ShowProgressBar("正在读取化简数据...", 0, wCount, 1, true);
            MultipointClass mp = new MultipointClass();
            IQueryFilter qf = new QueryFilterClass();
            while ((checkFeature = checkCursor.NextFeature()) != null)
            {
                sBar.StepProgressBar();
                ids.Add(checkFeature.OID);
                IPoint p = (checkFeature.Shape as IArea).Centroid;
                mp.AddPoint(p, ref miss, ref miss);
            }
            sPro.Position = 0;
            sBar.ShowProgressBar("正在分析化简数据...", 0, mp.PointCount, 1, true);
            IPointCollection mp_trans = mp.Clone() as IPointCollection;
            IPoint centerPoint = (mp.Envelope as IArea).Centroid;
            (mp_trans as ITransform2D).Scale(centerPoint, 200, 200);
            for (int i = 0; i < mp.PointCount; i++)
            {
                sBar.StepProgressBar();
                PointClass p = new PointClass();
                p.X = mp_trans.get_Point(i).X - mp.get_Point(i).X;
                p.Y = mp_trans.get_Point(i).Y - mp.get_Point(i).Y;
                int fid = ids[i];
                IFeature feature = fc.GetFeature(fid);
                ITransform2D shape = feature.ShapeCopy as ITransform2D;
                shape.Move(p.X, p.Y);
                IFeatureBuffer fbuffer = fc_tp.CreateFeatureBuffer();
                fbuffer.Shape = shape as IGeometry;
                fbuffer.set_Value(fbuffer.Fields.FindField("simpoid"), fid);
                fbuffer.set_Value(fbuffer.Fields.FindField("dx"), p.X);
                fbuffer.set_Value(fbuffer.Fields.FindField("dy"), p.Y);
                insert_Cursor.InsertFeature(fbuffer);
                fid_dxy_dic.Add(fid, p);
            }
            sBar.HideProgressBar();
            insert_Cursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insert_Cursor);

            SimplifyBuilding sb = new SimplifyBuilding();

            sb.in_features = fc_tp;

            //string outName = m_application.Workspace.LayerManager.TempLayerName();
            //sb.out_feature_class = m_application.Workspace.Workspace.PathName + "\\" + outName;
            //sb.simplification_tolerance = m_application.GenPara["建筑物化简容差"];
            int mm = 1;
            string finalFile = "tp" + mm;
            while (System.IO.File.Exists(tempPath + "\\" + finalFile + ".shp"))
            {
                mm++;
                finalFile = "tp" + mm;
            }
            sb.out_feature_class = tempPath + "\\" + finalFile;
            sb.simplification_tolerance = dis;

            //wo.SetText("正在化简建筑物");
            sBar.ShowProgressBar("正在化简建筑物...", 0, 1, 1, true);
            sPro.Position = 1;
            object result = Geoprosessor.Execute(sb, null);

            if (result == null)
            {
                sBar.HideProgressBar();
                throw new Exception();
            }
            IFeatureClass fc_gen = (ws as IFeatureWorkspace).OpenFeatureClass(System.IO.Path.GetFileName(sb.out_feature_class.ToString()));
            IFeatureCursor gen_cursor = fc_gen.Search(null, true);
            IFeature gen_feature = null;
            wCount = fc_gen.FeatureCount(null);
            sPro.Position = 0;
            sBar.ShowProgressBar("正在整理化简结果...", 0, wCount, 1, true);
            editor.StartOperation();
            while ((gen_feature = gen_cursor.NextFeature()) != null)
            {
                sBar.StepProgressBar();
                int org_oid = (int)gen_feature.get_Value(gen_feature.Fields.FindField("simpoid"));
                double dx = (double)gen_feature.get_Value(gen_feature.Fields.FindField("dx"));
                double dy = (double)gen_feature.get_Value(gen_feature.Fields.FindField("dy"));
                IFeature org_feature = fc.GetFeature(org_oid);
                ITransform2D shape = gen_feature.ShapeCopy as ITransform2D;
                shape.Move(-dx, -dy);
                (shape as IPolygon).Generalize(dis * 0.2);
                (shape as ITopologicalOperator).Simplify();
                if ((shape as IGeometryCollection).GeometryCount > 1)
                {
                    IGeometryCollection gc = shape as IGeometryCollection;
                    PolygonClass newPolygon = new PolygonClass();

                    for (int i = 0; i < gc.GeometryCount; i++)
                    {
                        IArea ring = gc.get_Geometry(i) as IArea;
                        if (Math.Abs(ring.Area) * 10 > area)
                        {
                            newPolygon.AddGeometry(ring as IGeometry, ref miss, ref miss);
                        }
                    }
                    if (!newPolygon.IsEmpty)
                    {
                        newPolygon.Simplify();
                        shape = newPolygon;
                    }
                }
                org_feature.Shape = shape as IGeometry;
                if (groupID >= 0)
                    org_feature.set_Value(groupID, -2);
                org_feature.Store();
            }
            sBar.HideProgressBar();
            ac.Refresh();
            editor.StopOperation("建筑物化简");
        }
    }
}

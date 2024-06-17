using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.DataSourcesFile;
using System.Windows.Forms;

namespace DomapTool
{
    /// <summary>
    /// Summary description for Axis.
    /// </summary>
    [Guid("e0630a87-ce8f-438f-8332-4d0eef7f5a58")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DomapTool.Axis")]
    public sealed class Axis : BaseCommand
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

        private IApplication m_application;
        public Axis()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text
            base.m_caption = "中轴化";  //localizable text
            base.m_message = "中轴化";  //localizable text 
            base.m_toolTip = "中轴化";  //localizable text 
            base.m_name = "DomapTool.Axis";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overriden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            m_application = hook as IApplication;

            //Disable if it is not ArcMap
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false;

            // TODO:  Add other initialization code
        }
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
        }
        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public GenerateUpdateAreaWin2 gua;
        IActiveView ac;
        public override void OnClick()
        {
            ac = (m_application.Document as IMxDocument).ActiveView;
            IMap map = ac.FocusMap;
            gua = new GenerateUpdateAreaWin2(map);
            string saveAreaFile = "";
            if (gua.ShowDialog() == DialogResult.OK)
            {
                saveAreaFile = gua.UpdateAreaLayerFileName;
            }
            else
            {
                gua = null;
                return;
            }
            if (saveAreaFile != "")
            {
                string path = System.IO.Path.GetDirectoryName(saveAreaFile);
                string featClassName = System.IO.Path.GetFileNameWithoutExtension(saveAreaFile);
                IWorkspaceFactory wsf = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();
                if (!System.IO.Directory.Exists(path))
                {
                    System.Windows.Forms.MessageBox.Show("请指定文件夹路径！");
                    return;
                }
                IWorkspace ws = wsf.OpenFromFile(path, 0);
                if (System.IO.File.Exists(saveAreaFile))
                {
                    System.Windows.Forms.MessageBox.Show("类已存在，请重新指定名称！");
                    return;
                }
                IFeatureSelection featSelect = gua.SelectUpdateLayer as IFeatureSelection;
                ISelectionSet set = featSelect.SelectionSet;
                if (set.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("请选择需要中轴化的要素！");
                    return;
                }
                IFeatureClass updateClass = gua.SelectUpdateLayer.FeatureClass;
                ESRI.ArcGIS.esriSystem.IClone clone = (updateClass.Fields as ESRI.ArcGIS.esriSystem.IClone).Clone();
                IFields sourceFileds = clone as IFields;
                IField shpField = sourceFileds.get_Field(sourceFileds.FindField("Shape"));
                IGeometryDef geometryDef = shpField.GeometryDef;
                IGeometryDefEdit geoDefEdit = geometryDef as IGeometryDefEdit;
                geoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
                for (int i = 0; i < sourceFileds.FieldCount; i++)
                {
                    IField tpff = sourceFileds.get_Field(i);
                    if (tpff.Type == esriFieldType.esriFieldTypeGeometry || tpff.Type == esriFieldType.esriFieldTypeOID)
                    {
                        continue;
                    }
                    if (!tpff.Editable)
                    {
                        (sourceFileds as IFieldsEdit).DeleteField(tpff);
                    }
                }
                IFeatureClass fc = (ws as IFeatureWorkspace).CreateFeatureClass(featClassName, sourceFileds, null, null, esriFeatureType.esriFTSimple, "Shape", "");
                IStatusBar sBar = m_application.StatusBar;
                IStepProgressor sPro = sBar.ProgressBar;
                sPro.Position = 0;
                //sBar.ShowProgressBar("中轴化...", 0, set.Count, 1, true);
                IFeatureCursor forInsertCur = fc.Insert(false);
                CenterLineFactory cf = new CenterLineFactory();
                IFeature feat = null;
                ICursor cur;
                set.Search(null, false, out cur);
                IFeatureCursor setFeatCur = cur as IFeatureCursor;
                GeometryBagClass gb = new GeometryBagClass();
                while ((feat = setFeatCur.NextFeature()) != null)
                {
                    sBar.StepProgressBar();
                    try
                    {
                        IGeometry geoCopy = feat.ShapeCopy;
                        IPolygon forMinArea = geoCopy as IPolygon;
                        CenterLine centerLine = cf.Create2(forMinArea);
                        IPolyline resultLine = centerLine.Line;
                        IFeatureBuffer forInsertBuffer = fc.CreateFeatureBuffer();
                        forInsertBuffer.Shape = resultLine;
                        convertAttri(feat, forInsertBuffer);
                        forInsertCur.InsertFeature(forInsertBuffer);

                        //gb.AddGeometries(1, ref geoCopy);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                //IPolygon unionPoly = new PolygonClass();
                //(unionPoly as ITopologicalOperator).ConstructUnion(gb);
                //IGeometryCollection gc = (unionPoly as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                //sBar.ShowProgressBar("中轴化...", 0, gc.GeometryCount, 1, true);
                //for (int i = 0; i < gc.GeometryCount; i++)
                //{
                //    sBar.StepProgressBar();
                //    IPolygon part = gc.get_Geometry(i) as IPolygon;
                //    try
                //    {
                //        CenterLine cl = cf.Create2(part);
                //        IPolyline resultLine = cl.Line;
                //        IFeatureBuffer forInsertBuffer = fc.CreateFeatureBuffer();
                //        forInsertBuffer.Shape = resultLine;
                //        forInsertCur.InsertFeature(forInsertBuffer);
                //    }
                //    catch
                //    {
                //    }
                //}
                sBar.HideProgressBar();
                IFeatureLayer lay = new FeatureLayerClass();
                lay.FeatureClass = fc;
                lay.Name = featClassName;
                map.AddLayer(lay as ILayer);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(forInsertCur);
                ac.Refresh();
            }
        }
        void convertAttri(IFeature areaFeat, IFeatureBuffer linFeat)
        {
            IFields fields = areaFeat.Fields;
            IFields lineFields = linFeat.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                IField field = fields.get_Field(i);
                if (!field.Editable||field.Type==esriFieldType.esriFieldTypeGeometry)
                {
                    continue;
                }
                int tpf = lineFields.FindField(field.Name);
                object va = areaFeat.get_Value(i);
                //if(va!=null)
                linFeat.set_Value(tpf, va);
            }
        }

        #endregion
    }
}

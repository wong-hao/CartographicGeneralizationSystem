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

namespace DatabaseUpdate
{
    /// <summary>
    /// Summary description for GenerateUpdateArea.
    /// </summary>
    [Guid("33ed0282-b02d-4ce3-b264-7ce8e46f5910")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DatabaseUpdate.GenerateUpdateArea")]
    public sealed class GenerateUpdateArea : BaseCommand
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
        public GenerateUpdateArea()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text
            base.m_caption = "生成更新范围";  //localizable text
            base.m_message = "生成更新范围";  //localizable text 
            base.m_toolTip = "自动生成更新范围";  //localizable text 
            base.m_name = "生成更新范围";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

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

        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public GenerateUpdateAreaWin2 gua;
        IActiveView ac;
        public override void OnClick()
        {
            ac=(m_application.Document as IMxDocument).ActiveView;
            IMap map=ac.FocusMap;
            gua = new GenerateUpdateAreaWin2(map);
            string saveAreaFile="";
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
                //IWorkspaceFactory wsf = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();
                //if (!wsf.IsWorkspace(path))
                if(!System.IO.Directory.Exists(path))
                {
                    System.Windows.Forms.MessageBox.Show("请指定文件夹路径！");
                    return;
                }                
                IWorkspace ws = wsf.OpenFromFile(path, 0);
                //if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass,featClassName))
                if(System.IO.File.Exists(saveAreaFile))
                {
                    System.Windows.Forms.MessageBox.Show("类已存在，请重新指定名称！");
                    return;
                }
                IFeatureSelection featSelect = gua.SelectUpdateLayer as IFeatureSelection;
                ISelectionSet set = featSelect.SelectionSet;
                if (set.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("请选择需要更新的要素！");
                    return;
                }

                //fc = (ws as IFeatureWorkspace).OpenFeatureClass(featClassName);

                IFeatureClass updateClass = gua.SelectUpdateLayer.FeatureClass;
                ESRI.ArcGIS.esriSystem.IClone clone = (updateClass.Fields as ESRI.ArcGIS.esriSystem.IClone).Clone();
                IFields sourceFileds = clone as IFields;

                IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
                IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
                IFields rfields = ocDescription.RequiredFields;

                IFeatureClass fc = (ws as IFeatureWorkspace).CreateFeatureClass(featClassName, rfields, null, null, esriFeatureType.esriFTSimple, "Shape", "");

                //IStatusBar sBar = m_application.StatusBar;
                //IStepProgressor sPro = sBar.ProgressBar;
                //sPro.Position = 0;
                //sBar.ShowProgressBar("生成更新区域...", 0, set.Count, 1, true);

                GeometryBagClass gb = new GeometryBagClass();
                IFeature feat = null;
                ICursor cur = null;
                set.Search(null, false, out cur);
                IFeatureCursor fcur = cur as IFeatureCursor;
                while ((feat = fcur.NextFeature()) != null)
                {
                    IPolygon Poly = feat.ShapeCopy as IPolygon;
                    IGeometry polyGeo = Poly as IGeometry;
                    gb.AddGeometries(1, ref polyGeo);
                }
                ITopologicalOperator to = gb as ITopologicalOperator;
                to.Simplify();
                IPolygon unionPoly = new PolygonClass();
                ITopologicalOperator to2 = unionPoly as ITopologicalOperator;
                to2.ConstructUnion(gb);
                IPolygon convexHull = to2.ConvexHull() as IPolygon;
                IFeatureBuffer featBuff = fc.CreateFeatureBuffer();
                featBuff.Shape = convexHull;
                IFeatureCursor insertCur = fc.Insert(false);
                insertCur.InsertFeature(featBuff);
                IFeatureLayer addLayer = new FeatureLayerClass();
                addLayer.FeatureClass = fc;
                addLayer.Name = featClassName;
                ISimpleFillSymbol sfs = new SimpleFillSymbolClass();
                IRgbColor c = new RgbColorClass();
                c.Red = 200;
                c.Green = 0;
                c.Blue = 0;
                ILineSymbol linesym = new SimpleLineSymbolClass();
                linesym.Color = c as IColor;
                linesym.Width = 2;
                sfs.Outline = linesym;
                sfs.Style = esriSimpleFillStyle.esriSFSNull;
                IUniqueValueRenderer render = new UniqueValueRendererClass();
                render.DefaultSymbol = sfs as ISymbol;
                render.UseDefaultSymbol = true;
                (addLayer as IGeoFeatureLayer).Renderer = render as IFeatureRenderer;
                map.AddLayer(addLayer);
                ac.Refresh();
                insertCur.Flush();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fcur);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cur);
            }
        }

        #endregion
    }
}

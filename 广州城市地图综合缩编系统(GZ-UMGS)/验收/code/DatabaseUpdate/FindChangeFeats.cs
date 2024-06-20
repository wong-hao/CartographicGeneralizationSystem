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
    /// Summary description for FindChangeFeats.
    /// </summary>
    [Guid("4b23c499-15e3-4138-a348-289c89fda567")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DatabaseUpdate.FindChangeFeats")]
    public sealed class FindChangeFeats : BaseCommand
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
        public FindChangeFeats()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text
            base.m_caption = "查找变化对象";  //localizable text
            base.m_message = "查找变化对象";  //localizable text 
            base.m_toolTip = "查找需要更新的对象";  //localizable text 
            base.m_name = "DatabaseUpdate.FindChangeFeats";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")

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
        public GenerateUpdateAreaWin gua2;
        IActiveView ac;
        public override void OnClick()
        {
            try
            {
                ac = (m_application.Document as IMxDocument).ActiveView;
                IMap map = ac.FocusMap;
                gua2 = new GenerateUpdateAreaWin(map);
                IFeatureLayer updateLayer = null;
                IFeatureLayer updateAreaLayer = null;
                if (gua2.ShowDialog() == DialogResult.OK)
                {
                    updateLayer = gua2.SelectUpdateLayer;
                    updateAreaLayer = gua2.SelectUpdateAreaLayer;
                }
                else
                {
                    gua2 = null;
                    return;
                }
                if (updateLayer == null || updateAreaLayer == null)
                {
                    MessageBox.Show("请导入更新图层和更新范围图层！");
                    return;
                }
                UApplication app = UApplication.Application;
                //ULayerInfos mxlayers = app.Workspace.MxLayers;
                ULayerInfos mclayers = app.Workspace.McLayers;
                IFeatureLayer databaseLayer = null;
                foreach (IFeatureLayer mcLayer in mclayers.Layers.Values)
                {
                    if(mcLayer.Name.Contains(updateLayer.Name))
                    //if (mcLayer.Name == updateLayer.Name)
                    {
                        databaseLayer = mcLayer;
                        break;
                    }
                }
                if (databaseLayer == null)
                {
                    MessageBox.Show("请检查导入的图层名是否符合要求！");
                    return;
                }
                IFeatureClass areaClass = updateAreaLayer.FeatureClass;
                IFeatureCursor areaFeatcur = areaClass.Search(null, false);
                IPolygon area = (areaFeatcur.NextFeature()).Shape as IPolygon;
                area = (area as ITopologicalOperator).Buffer(0.1) as IPolygon;
                ISpatialFilter sf = new SpatialFilterClass();
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                sf.Geometry = area;
                (updateLayer as IFeatureSelection).SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
                (databaseLayer as IFeatureSelection).SelectFeatures(sf, esriSelectionResultEnum.esriSelectionResultNew, false);
                List<int> updateRemoveIDs = new List<int>();
                List<int> databaseRemoveIDs = new List<int>();
                List<MatchResult> matchResult = new List<MatchResult>();
                ISelectionSet updateSelectSet = (updateLayer as IFeatureSelection).SelectionSet;
                ISelectionSet databaseSelectSet = (databaseLayer as IFeatureSelection).SelectionSet;
                ICursor updateCur;
                updateSelectSet.Search(null, false, out updateCur);
                IFeatureCursor updateFeatCur = updateCur as IFeatureCursor;
                IFeature updateFeat = null;
                ISpatialFilter sf2 = new SpatialFilterClass();
                sf2.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IStatusBar sBar = m_application.StatusBar;
                IStepProgressor sPro = sBar.ProgressBar;
                sPro.Position = 0;
                sBar.ShowProgressBar("更新检查中...", 0, updateSelectSet.Count, 1, true);
                //ICursor updateCur2=null;
                while ((updateFeat = updateFeatCur.NextFeature()) != null)
                {
                    sBar.StepProgressBar();
                    IGeometry tpUpdateGeo = updateFeat.Shape;
                    sf2.Geometry = tpUpdateGeo;
                    ICursor updateCur2;
                    databaseSelectSet.Search(sf2, false, out updateCur2);
                    IFeatureCursor updateFeatCur2 = updateCur2 as IFeatureCursor;
                    IFeature updateFeat2 = null;
                    bool isChangedFeat = true;
                    while ((updateFeat2 = updateFeatCur2.NextFeature()) != null)
                    {
                        IGeometry toUpdateGeo2 = updateFeat2.Shape;                        
                        double areaRatio = (toUpdateGeo2 as IArea).Area / (tpUpdateGeo as IArea).Area;
                        if (areaRatio > 0.95 && areaRatio < 1.05)
                        {
                            IArea a = (toUpdateGeo2 as ITopologicalOperator).Intersect(tpUpdateGeo, esriGeometryDimension.esriGeometry2Dimension) as IArea;
                            double areaRatio1 = a.Area / (tpUpdateGeo as IArea).Area;
                            double areaRatio2 = a.Area / (toUpdateGeo2  as IArea).Area;
                            if (areaRatio1 > 0.95 && areaRatio2 > 0.95)
                            {
                                isChangedFeat = false;
                                //if (!databaseRemoveIDs.Contains(updateFeat2.OID))
                                //{
                                //    databaseRemoveIDs.Add(updateFeat2.OID);
                                //}
                                databaseRemoveIDs.Add(updateFeat2.OID);
                                matchResult.Add(new MatchResult(updateFeat, updateFeat2, Math.Min(areaRatio1, areaRatio2)));
                                break;
                            }
                        }
                    }
                    if (!isChangedFeat)
                    {
                      //if (updateRemoveIDs.Contains(updateFeat.OID))
                      //{
                      //    updateRemoveIDs.Add(updateFeat.OID);
                      //}
                      updateRemoveIDs.Add(updateFeat.OID);
                    }
                    else
                    {
                      matchResult.Add(new MatchResult(updateFeat, null, 0));
                    }
                    //updateCur2.Flush();
                    //updateFeatCur2.Flush();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(updateCur2);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(updateFeatCur2);
                }
                sBar.HideProgressBar();

                if (updateRemoveIDs.Count == 0 && databaseRemoveIDs.Count ==0)
                {
                    return;
                }
                int[] updateArray = updateRemoveIDs.ToArray();
                int[] databaseArray = databaseRemoveIDs.ToArray();
                updateSelectSet.RemoveList(updateArray.Length, ref updateArray[0]);
                databaseSelectSet.RemoveList(databaseArray.Length, ref databaseArray[0]);

                MatchResultForm dlg = new MatchResultForm();
                dlg.matchResultBindingSource.DataSource = matchResult;
                dlg.Show();
                updateCur.Flush();
                //updateCur2.Flush();
                updateFeatCur.Flush();
                ac.Refresh();
                UApplication.Application.EsriMapControl.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}

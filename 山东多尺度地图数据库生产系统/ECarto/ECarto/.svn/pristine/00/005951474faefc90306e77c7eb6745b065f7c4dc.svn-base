using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Data;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using System.IO;
using SMGI.Common;
namespace SMGI.Plugin.EmergencyMap.Side
{
    public  class SideHelper
    {
        private static SideHelper instance = null;
        public static SideHelper InStance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SideHelper();
                }
                return instance;
            }
        }
        private void DrawResult(IGeometry polygon)
        {
             PolygonElementClass ele = new ESRI.ArcGIS.Carto.PolygonElementClass();
             ele.Geometry = polygon;
            (GApplication.Application.ActiveView as IGraphicsContainer).AddElement(ele, 0);
            GApplication.Application.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGraphics, null, null);
                       
        }
        public void CreateView(string bufferType, List<IFeature> fes,double dis=2.5)
        {
            (GApplication.Application.ActiveView as IGraphicsContainer).DeleteAllElements();
            WaitOperation wo = GApplication.Application.SetBusy();
            try
            {
                dis = dis * GApplication.Application.ActiveView.FocusMap.ReferenceScale * 1e-3;
                IFeatureClass sideSDMfcl = CreateFeatureClass("SideSDM", GApplication.Application.MapControl.SpatialReference);
                IWorkspace ws = (sideSDMfcl as IDataset).Workspace;
                // ClearLayer();
                Dictionary<int, string> dic = new Dictionary<int, string>();
                dic[2] = "外层";
                dic[1] = "内层";
                //1.创建临时要素类
                //2.gp缓冲
                foreach (var fe in fes)
                {
                    
                    var feNew = sideSDMfcl.CreateFeature();
                    feNew.Shape = fe.Shape;
                    feNew.Store();
                    ESRI.ArcGIS.AnalysisTools.Buffer gpBuffer = new ESRI.ArcGIS.AnalysisTools.Buffer();
                    if (bufferType == "DOUBLE")
                    {
                        gpBuffer.line_side = "LEFT";
                        
                        
                        #region
                        //切缓冲两头部分
                        gpBuffer.buffer_distance_or_field =  dis + " Meters";
                        gpBuffer.line_end_type = "FLAT";
                        gpBuffer.in_features = ws.PathName + "\\" + sideSDMfcl.AliasName;
                        gpBuffer.out_feature_class = ws.PathName + "\\" + sideSDMfcl.AliasName + "_B";
                        SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, gpBuffer, null);
                        IFeatureClass fc = (ws as IFeatureWorkspace).OpenFeatureClass(sideSDMfcl.AliasName + "_B");
                        IFeatureCursor cursor = fc.Search(null, false);
                        IGeometry polygon = cursor.NextFeature().ShapeCopy;
                        Marshal.ReleaseComObject(cursor);
                        if (polygon == null)
                        {
                            MessageBox.Show("色带面缓冲出错！");
                            return;
                        }
                        DrawResult(polygon);
                        (fc as IDataset).Delete();
                        #endregion
                         
                        gpBuffer.line_side = "RIGHT";
                       
                        #region
                        //切缓冲两头部分
                        gpBuffer.buffer_distance_or_field =  dis + " Meters";
                        gpBuffer.line_end_type = "FLAT";
                        gpBuffer.in_features = ws.PathName + "\\" + sideSDMfcl.AliasName;
                        gpBuffer.out_feature_class = ws.PathName + "\\" + sideSDMfcl.AliasName + "_B";
                        SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, gpBuffer, null);
                        fc = (ws as IFeatureWorkspace).OpenFeatureClass(sideSDMfcl.AliasName + "_B");
                        cursor = fc.Search(null, false);
                        polygon = cursor.NextFeature().ShapeCopy;
                        Marshal.ReleaseComObject(cursor);
                        if (polygon == null)
                        {
                            MessageBox.Show("色带面缓冲出错！");
                            return;
                        }
                        DrawResult(polygon);
                        (fc as IDataset).Delete();
                        #endregion
                        

                    }
                    else
                    {
                        gpBuffer.line_side = bufferType;
                        #region
                        //切缓冲两头部分
                        gpBuffer.buffer_distance_or_field =  dis + " Meters";
                        gpBuffer.line_end_type = "FLAT";
                        gpBuffer.in_features = ws.PathName + "\\" + sideSDMfcl.AliasName;
                        gpBuffer.out_feature_class = ws.PathName + "\\" + sideSDMfcl.AliasName + "_B";
                        SMGI.Common.Helper.ExecuteGPTool(GApplication.Application.GPTool, gpBuffer, null);
                        var fc = (ws as IFeatureWorkspace).OpenFeatureClass(sideSDMfcl.AliasName + "_B");
                        var cursor = fc.Search(null, false);
                        IGeometry polygon = cursor.NextFeature().ShapeCopy;
                        Marshal.ReleaseComObject(cursor);
                        if (polygon == null)
                        {
                            MessageBox.Show("色带面缓冲出错！");
                            return;
                        }
                        DrawResult(polygon);
                        (fc as IDataset).Delete();
                        #endregion
                         
                    }

                }
              
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
               
                MessageBox.Show(ex.Message);
            }
            finally
            {
                GC.Collect();
                wo.Dispose();
            }
        }


        private IFields CreatLayerAttribute(ISpatialReference sr, esriGeometryType geometryType = esriGeometryType.esriGeometryPolyline)
        {
            //设置字段集
            IFields fields = new FieldsClass();
            var fieldsEdit = (IFieldsEdit)fields;
            IField field = new FieldClass();
            var fieldEdit = (IFieldEdit)field;

            //创建主键
            fieldEdit.Name_2 = "OBJECTID";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(field);


            //创建图形字段
            IGeometryDef geometryDef = new GeometryDefClass();
            var geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = geometryType;
            geometryDefEdit.SpatialReference_2 = sr;

            field = new FieldClass();
            fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "SHAPE";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            fieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(field);
            return fields;
        }
        private string GetAppDataPath()
        {
            if (System.Environment.OSVersion.Version.Major <= 5)
            {
                return System.IO.Path.GetFullPath(
                    System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\..");
            }

            var dp = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var di = new System.IO.DirectoryInfo(dp);
            var ds = di.GetDirectories("SMGI");
            if (ds == null || ds.Length == 0)
            {
                var sdi = di.CreateSubdirectory("SMGI");
                return sdi.FullName;
            }
            else
            {
                return ds[0].FullName;
            }
        }

        /// <summary>
        /// 创建临时工作空间
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private IWorkspace createTempWorkspace(string fullPath)
        {
            IWorkspace pWorkspace = null;
            IWorkspaceFactory2 wsFactory = new FileGDBWorkspaceFactoryClass();

            if (!Directory.Exists(fullPath))
            {

                IWorkspaceName pWorkspaceName = wsFactory.Create(System.IO.Path.GetDirectoryName(fullPath),
                    System.IO.Path.GetFileName(fullPath), null, 0);
                IName pName = (IName)pWorkspaceName;
                pWorkspace = (IWorkspace)pName.Open();
            }
            else
            {
                pWorkspace = wsFactory.OpenFromFile(fullPath, 0);
            }



            return pWorkspace;
        }
        //创建要素类
        private IFeatureClass CreateFeatureClass(string name, ISpatialReference sr, esriGeometryType geometryType = esriGeometryType.esriGeometryPolyline)
        {
            try
            {
                //IFeatureWorkspace fws =  GApplication.Application.MemoryWorkspace as IFeatureWorkspace;
                string fullPath = GetAppDataPath() + "\\MyWorkspace.gdb";
                IWorkspace ws = createTempWorkspace(fullPath);
                IFeatureWorkspace fws = createTempWorkspace(fullPath) as IFeatureWorkspace;
                if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, name))
                {
                    (fws.OpenTable(name) as IDataset).Delete();
                }
                if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name))
                {
                    //IFeatureClass fcl = fws.OpenFeatureClass(name);
                    //(fcl as ITable).DeleteSearchedRows(null);
                    //return fcl;
                    (fws.OpenFeatureClass(name) as IDataset).Delete();
                }
                IFields org_fields = CreatLayerAttribute(sr, geometryType);

                return fws.CreateFeatureClass(name, org_fields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
            }
            catch
            {
                return null;
            }
        }
    }
}

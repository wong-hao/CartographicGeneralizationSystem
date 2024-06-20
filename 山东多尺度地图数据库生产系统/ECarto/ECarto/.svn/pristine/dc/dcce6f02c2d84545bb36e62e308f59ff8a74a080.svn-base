using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Data;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Xml;
using System.Xml.Linq;
using ESRI.ArcGIS.DataSourcesGDB;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
namespace SMGI.Plugin.MapGeneralization
{
    //拓扑化简@LZ20221029
    public class TopoSimpilfyTool : SMGI.Common.SMGITool
    {
        private static IFeatureClass _tempFC = null;
        private static string _fidFN = "fe_fid";
        private static double _refScale;
        private static string _simplyAlgorithm;
        private static double _simplifyTolerance = 0;
        private static bool _enableSmooth;
        private static string _smoothAlgorithm;
        private static double _smoothTolerance;
        

        ITopology _topgy = null;

        public TopoSimpilfyTool()
        {
            m_caption = "拓扑边化简(交互)";
            m_toolTip = "拓扑边化简";

        }
        public override bool Enabled
        {
            get
            {
                if (TopologyApplication.Topology == null)
                {
                    if (_tempFC != null)
                    {
                        (_tempFC as IDataset).Delete();
                        _tempFC = null;
                    }

                    _simplifyTolerance = 0;
                }

                return TopologyApplication.Topology!=null && m_Application != null && 
                    m_Application.Workspace != null && m_Application.EngineEditor.EditState== esriEngineEditState.esriEngineStateEditing;
            }
        }
        
        public override void OnClick()
        {
            if (_simplifyTolerance == 0)
            {
                var frm = new GeneralizeForm(m_Application.MapControl.Map.ReferenceScale);
                if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    if (_simplifyTolerance == 0)
                    {
                        m_Application.MapControl.CurrentTool = null;
                    }
                    return;
                }
                _refScale = frm.RefScale;
                _simplyAlgorithm = frm.SimplifyAlgorithm;
                _simplifyTolerance = frm.SimplifyTolerance;
                _enableSmooth = frm.EnableSmooth;
                _smoothAlgorithm = frm.SmoothAlgorithm;
                _smoothTolerance = frm.SmoothTolerance;
            }

            if (_tempFC == null)
            {
                //创建临时线要素类
                try
                {
                    string fullPath = GetAppDataPath() + "\\MyWorkspace.gdb";
                    IWorkspace ws = createTempWorkspace(fullPath);
                    IFeatureWorkspace fws = ws as IFeatureWorkspace;

                    ISpatialReference sr = null;
                    var lineLayers = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer));
                    foreach (var lyr in lineLayers)
                    {
                        IFeatureLayer feLayer = lyr as IFeatureLayer;
                        if (feLayer.FeatureClass == null)
                            continue;//空图层

                        if ((feLayer.FeatureClass as IDataset).Workspace.PathName != GApplication.Application.Workspace.EsriWorkspace.PathName)
                            continue;//临时数据

                        sr = (feLayer.FeatureClass as IGeoDataset).SpatialReference;
                    }

                    _tempFC = CreateFeatureClass(fws, "temp_topoSimplify", sr, esriGeometryType.esriGeometryPolyline, _fidFN);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                    m_Application.MapControl.CurrentTool = null;
                    return;
                }
            }

            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
          
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }

            if (_simplifyTolerance == 0 || _tempFC == null)
            {
                return;
            }

            //清空原要素类
            (_tempFC as ITable).DeleteSearchedRows(null);

            _topgy = TopologyApplication.Topology;
            var view = m_Application.ActiveView;
            IMap map = view.FocusMap;
            var editor = m_Application.EngineEditor;

            //画范围
            IRubberBand pRubberBand = new RubberRectangularPolygonClass();
            var geo = pRubberBand.TrackNew(view.ScreenDisplay, null);

            if (geo==null||geo.IsEmpty)
            {
                return;
            }

            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    TopologyHelper helper = new TopologyHelper(view);

                    wo.SetText("正在创建拓扑缓存");
                    ITopologyGraph graph = helper.CreateTopGraph(_topgy);

                    wo.SetText("正在化简拓扑边");
                    graph.SelectByGeometry((int)esriTopologyElementType.esriTopologyEdge, esriTopologySelectionResultEnum.esriTopologySelectionResultNew, geo);
                    IEnumTopologyEdge enumEdges = graph.EdgeSelection;
                    if(enumEdges.Count ==0)
                    {
                        Thread.Sleep(300);
                        return;
                    }
                    enumEdges.Reset();
                    ITopologyEdge edge = null;
                    IEnvelope penvelope;

                    Dictionary<ITopologyEdge, int> edgeList = new Dictionary<ITopologyEdge, int>();
                    #region 将拓扑边几何插入到临时要素类中
                    IFeatureCursor newFeCursor = _tempFC.Insert(true);

                    int id = 0;
                    while ((edge = enumEdges.Next()) != null)
                    {
                        IPolyline line = (edge.Geometry as IClone).Clone() as IPolyline;
                        if ((line as IZAware).ZAware)
                        {
                            (line as IZAware).ZAware = false;
                        }

                        IFeatureBuffer newFeBuffer = _tempFC.CreateFeatureBuffer();
                        //几何赋值
                        newFeBuffer.Shape = line;
                        //属性赋值
                        newFeBuffer.set_Value(_tempFC.FindField(_fidFN), ++id);

                        newFeCursor.InsertFeature(newFeBuffer);

                        //
                        edgeList.Add(edge, id);
                    }
                    newFeCursor.Flush();
                    Marshal.ReleaseComObject(newFeCursor);
                    Marshal.ReleaseComObject(enumEdges);
                    #endregion

                    Dictionary<int, IGeometry> newGeoList = new Dictionary<int, IGeometry>();
                    #region 几何化简
                    newGeoList = Generalize(_tempFC, "", _simplyAlgorithm, _simplifyTolerance, _enableSmooth, _smoothAlgorithm, _smoothTolerance);
                    #endregion


                    #region 更新
                    try
                    {
                        m_Application.EngineEditor.StartOperation();

                        enumEdges = graph.EdgeSelection;
                        enumEdges.Reset();
                        while ((edge = enumEdges.Next()) != null)
                        {
                            IPointCollection reshapePath = new PathClass();
                            reshapePath.AddPointCollection(newGeoList[edgeList[edge]] as IPointCollection);

                            graph.SetEdgeGeometry(edge, reshapePath as IPath);
                            graph.Post(out penvelope);
                        }
                        Marshal.ReleaseComObject(enumEdges);

                        m_Application.EngineEditor.StopOperation("拓扑化简");
                    }
                    catch (Exception e)
                    {
                        m_Application.EngineEditor.AbortOperation();
                        throw e;
                    }
                    #endregion
                    
                    GC.Collect();


                    //刷新
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                return;
            }
         
        }


        public override bool Deactivate()
        {
            if (_tempFC != null)
            {
                (_tempFC as IDataset).Delete();
                _tempFC = null;
            }

            return base.Deactivate();
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            switch (keyCode)
            {
                case 32:
                    //弹框，设置成员参数
                    var frm = new GeneralizeForm(_refScale,_simplyAlgorithm, _simplifyTolerance, _enableSmooth, _smoothAlgorithm, _smoothTolerance);
                    if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        if (_simplifyTolerance == 0)
                        {
                            m_Application.MapControl.CurrentTool = null;
                        }
                        return;
                    }
                    _refScale = frm.RefScale;
                    _simplyAlgorithm = frm.SimplifyAlgorithm;
                    _simplifyTolerance = frm.SimplifyTolerance;
                    _enableSmooth = frm.EnableSmooth;
                    _smoothAlgorithm = frm.SmoothAlgorithm;
                    _smoothTolerance = frm.SmoothTolerance;
                    break;
                default:
                    break;
            }
        }

        public Dictionary<int, IGeometry> Generalize(IFeatureClass inputFC, string filterText, string simplyAlgorithm, double simplifyTol, bool enableSmooth, string smoothAlgorithm, double smoothTol)
        {
            Dictionary<int, IGeometry> newGeoList = new Dictionary<int, IGeometry>();

            IWorkspace ws = (inputFC as IDataset).Workspace;
            IFeatureWorkspace fws = ws as IFeatureWorkspace;

            IFeatureClass tempFC_Simplify = null;
            IFeatureClass tempFC_Simplify_Smooth = null;

            var gp = GApplication.Application.GPTool;
            gp.SetEnvironmentValue("workspace", (inputFC as IDataset).Workspace.PathName);
            gp.OverwriteOutput = true;
            try
            {
                //选择要素
                ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer pMakeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
                pMakeFeatureLayer.in_features = inputFC;
                pMakeFeatureLayer.out_layer = inputFC.AliasName + "_Layer";
                SMGI.Common.Helper.ExecuteGPTool(gp, pMakeFeatureLayer, null);
                ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute pSelectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
                pSelectLayerByAttribute.in_layer_or_view = inputFC.AliasName + "_Layer";
                pSelectLayerByAttribute.where_clause = filterText;


                SMGI.Common.Helper.ExecuteGPTool(gp, pSelectLayerByAttribute, null);

                IQueryFilter qf = new QueryFilterClass();
                if (pSelectLayerByAttribute.where_clause != null)
                    qf.WhereClause = pSelectLayerByAttribute.where_clause.ToString();

                //化简
                ESRI.ArcGIS.CartographyTools.SimplifyLine simplifyLineTool = new ESRI.ArcGIS.CartographyTools.SimplifyLine();
                simplifyLineTool.in_features = inputFC.AliasName + "_Layer";
                simplifyLineTool.out_feature_class = ws.PathName + @"\" + inputFC.AliasName + "_Simplify";
                simplifyLineTool.algorithm = simplyAlgorithm;
                simplifyLineTool.tolerance = simplifyTol;
                SMGI.Common.Helper.ExecuteGPTool(gp, simplifyLineTool, null);
                tempFC_Simplify = (ws as IFeatureWorkspace).OpenFeatureClass(inputFC.AliasName + "_Simplify");

                //光滑
                if (enableSmooth)
                {
                    ESRI.ArcGIS.CartographyTools.SmoothLine smoothLineTool = new ESRI.ArcGIS.CartographyTools.SmoothLine();
                    smoothLineTool.in_features = ws.PathName + @"\" + inputFC.AliasName + "_Simplify";
                    smoothLineTool.out_feature_class = ws.PathName + @"\" + tempFC_Simplify.AliasName + "_Smooth";
                    smoothLineTool.algorithm = smoothAlgorithm;
                    smoothLineTool.tolerance = smoothTol;
                    SMGI.Common.Helper.ExecuteGPTool(gp, smoothLineTool, null);

                    tempFC_Simplify_Smooth = (ws as IFeatureWorkspace).OpenFeatureClass(tempFC_Simplify.AliasName + "_Smooth");
                }


                IFeatureClass outFC = null;
                if (enableSmooth)
                {
                    outFC = tempFC_Simplify_Smooth;
                }
                else
                {
                    outFC = tempFC_Simplify;
                }

                //获取化简后的要素信息
                IFeatureCursor simpCursor = outFC.Search(null, true);
                IFeature simpFe = null;
                while ((simpFe = simpCursor.NextFeature()) != null)
                {
                    int id = int.Parse(simpFe.get_Value(outFC.FindField(_fidFN)).ToString());
                    newGeoList.Add(id, simpFe.ShapeCopy);
                }
                Marshal.ReleaseComObject(simpCursor);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

                if (tempFC_Simplify != null)
                {
                    (tempFC_Simplify as IDataset).Delete();
                }

                if (tempFC_Simplify_Smooth != null)
                {
                    (tempFC_Simplify_Smooth as IDataset).Delete();
                }
            }

            return newGeoList;
        }

        /// <summary>
        /// 获取应用程序默认路径
        /// </summary>
        public static string GetAppDataPath()
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
        public static IWorkspace createTempWorkspace(string fullPath)
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

        /// <summary>
        /// 创建临时要素类
        /// </summary>
        /// <param name="fws"></param>
        /// <param name="fcName"></param>
        /// <param name="sr"></param>
        /// <param name="geoType"></param>
        /// <param name="orgFCNameIndex"></param>
        /// <param name="orgfidIndx"></param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClass(IFeatureWorkspace fws, string fcName, ISpatialReference sr, esriGeometryType geoType, string fidName = "fe_fid")
        {
            if ((fws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
            {
                IFeatureClass fc = fws.OpenFeatureClass(fcName);
                (fc as IDataset).Delete();
            }

            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
            IFieldsEdit target_fields = ocDescription.RequiredFields as IFieldsEdit;

            //新增几何所属标识号
            IField newField = new FieldClass();
            IFieldEdit newFieldEdit = (IFieldEdit)newField;
            newFieldEdit.Name_2 = fidName;
            newFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            target_fields.AddField(newField);


            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)fws;
            fieldChecker.Validate(target_fields, out enumFieldError, out validatedFields);

            int shapeFieldIndex = target_fields.FindField(fcDescription.ShapeFieldName);
            IField Shapefield = target_fields.get_Field(shapeFieldIndex);
            IGeometryDef geometryDef = Shapefield.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = geoType;
            geometryDefEdit.SpatialReference_2 = sr;

            IFeatureClass newFC = fws.CreateFeatureClass(fcName, target_fields, ocDescription.InstanceCLSID,
                ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");

            return newFC;

        }
        
    }
}

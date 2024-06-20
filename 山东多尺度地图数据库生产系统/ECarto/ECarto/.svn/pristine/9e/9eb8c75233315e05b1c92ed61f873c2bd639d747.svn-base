using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.GeoAnalyst;
using System.Xml.Linq;
using ESRI.ArcGIS.SpatialAnalyst;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessing;

namespace SMGI.Plugin.EmergencyMap
{
    public class LandForms
    {
        GApplication _app;

        /// <summary>
        /// 地貌分类编码2地貌名称
        /// </summary>
        private Dictionary<int, string> code2Name;
        public Dictionary<int, string> LandFormsCode2Name
        {
            get
            {
                return code2Name;
            }
        }

        public LandForms(GApplication app)
        {
            _app = app;

            code2Name = new Dictionary<int, string>();
        }

        public IFeatureLayer ExtractLandForms(IRasterDataset demDataset, WaitOperation wo = null)
        {
            IRaster dem = demDataset.CreateDefaultRaster();

            //获取范围多边形,裁切DEM
            IPolygon extPolygon = getWorkspaceExtentPolygon(_app.Workspace.EsriWorkspace);
            IEnvelope wsExtent = extPolygon.Envelope;
            IEnvelope demExtent = (dem as IRasterProps).Extent;
            if (extPolygon != null && (wsExtent.LowerLeft.Compare(demExtent.LowerLeft) != 0 || wsExtent.UpperRight.Compare(demExtent.UpperRight) != 0))
            {
                //裁切DEM
                dem = CommonMethods.clipRaterDataset(demDataset, extPolygon as IGeometry,0);
            }

            if (wo != null)
                wo.SetText("正在提取坡度...");


            //提取坡度
            object zFactor = 1;
            IRaster slope = ExtractSlope(dem, zFactor);
            if (null == slope)
            {
                MessageBox.Show("提取坡度失败！");

                return null;
            }

            if (wo != null)
                wo.SetText("正在进行栅格处理...");

            //栅格重分类
            IRaster reclass = RasterReclass(slope);
            if (null == reclass)
            {
                MessageBox.Show("栅格重分类失败！");

                return null;
            }

            //边界理清
            IRaster boundar = BoundaryClean(reclass);
            if (null == boundar)
            {
                MessageBox.Show("边界清理失败！");

                return null;
            }

            if (wo != null)
                wo.SetText("正在栅格转面...");

            //栅格转面
            IFeatureClass fc = RasterToPolygonFeature(boundar, _app.Workspace.EsriWorkspace);
            if (null == fc)
            {
                MessageBox.Show("栅格转面失败！");

                return null;
            }
            IFeatureLayer tmpFeatureLayer = new FeatureLayerClass();
            tmpFeatureLayer.FeatureClass = fc;
            tmpFeatureLayer.Name = fc.AliasName;


            if (wo != null)
                wo.SetText("正在进行要素处理...");


            //消除
            IFeatureLayer landFormsLayer = Eliminate(tmpFeatureLayer);

            return landFormsLayer;
        }

        //获取ws的范围
        private IPolygon getWorkspaceExtentPolygon(IWorkspace ws, string fcName = "BOUA")
        {
            IPolygon result = null;

            if (!(ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
            {
                return null;
            }

            IFeatureClass fc = (ws as IFeatureWorkspace).OpenFeatureClass(fcName);
            IFeatureCursor pCursor = fc.Search(null, false);
            IFeature f;
            while ((f = pCursor.NextFeature()) != null)
            {
                if (null == result)
                {
                    result = f.ShapeCopy as IPolygon;
                    continue;
                }

                ITopologicalOperator pTopo = result as ITopologicalOperator;
                result = pTopo.Union(f.ShapeCopy) as IPolygon;
                pTopo.Simplify();
            }


            return result;
        }

        /// <summary>
        /// 提取坡度
        /// </summary>
        /// <param name="dem"></param>
        /// <param name="zFactor"></param>
        /// <returns></returns>
        private IRaster ExtractSlope(IRaster dem, object zFactor)
        {
            ISurfaceOp surfaceOP = new RasterSurfaceOpClass();
            IGeoDataset slope = surfaceOP.Slope(dem as IGeoDataset, esriGeoAnalysisSlopeEnum.esriGeoAnalysisSlopeDegrees, ref zFactor);
            return slope as IRaster;
        }

        /// <summary>
        /// 栅格重分类
        /// </summary>
        /// <param name="raster"></param>
        /// <returns></returns>
        private IRaster RasterReclass(IRaster raster)
        {
            INumberRemap numRemap = new NumberRemapClass();
            code2Name = new Dictionary<int, string>();

            //从配置文件中获取地貌分类的坡度参数
            XElement expertiseContent = ExpertiseDatabase.getContentElement(_app);
            var landForm = expertiseContent.Element("LandForm");
            var items = landForm.Elements("Item");
            foreach (XElement ele in items)
            {
                double min = double.Parse(ele.Element("MinSlope").Value);
                double max = double.Parse(ele.Element("MaxSlope").Value);
                int val = int.Parse(ele.Element("Val").Value);
                string landFormName = ele.Element("Name").Value;

                numRemap.MapRange(min, max, val);

                code2Name[val] = landFormName;
            }

            IReclassOp reclassOP = new RasterReclassOpClass();
            IGeoDataset reclass = reclassOP.ReclassByRemap(raster as IGeoDataset, numRemap as IRemap, false);

            return reclass as IRaster;
        }

        //边界清理
        private IRaster BoundaryClean(IRaster raster)
        {
            IGeneralizeOp genOp = new RasterGeneralizeOpClass();
            IGeoDataset result = genOp.BoundaryClean(raster as IGeoDataset, esriGeoAnalysisSortEnum.esriGeoAnalysisSortNone, true);

            return result as IRaster;
        }

        /// <summary>
        /// 栅格转面
        /// </summary>
        /// <param name="raster"></param>
        /// <param name="outputWorkspace"></param>
        /// <param name="outputFeatureClassName"></param>
        /// <param name="weeding"></param>
        /// <returns></returns>
        private IFeatureClass RasterToPolygonFeature(IRaster raster, IWorkspace outputWorkspace, string outputFeatureClassName = "LandForm", bool weeding = false)
        {
            if ((outputWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, outputFeatureClassName))
            {
                ((outputWorkspace as IFeatureWorkspace).OpenFeatureClass(outputFeatureClassName) as IDataset).Delete();
            }

            IConversionOp convOp = new RasterConversionOpClass();
            IGeoDataset result = convOp.RasterDataToPolygonFeatureData(raster as IGeoDataset, outputWorkspace, outputFeatureClassName, weeding);

            return result as IFeatureClass;
        }

        private IFeatureLayer Eliminate(IFeatureLayer inFeaLayer)
        {
            //图面面积小于5mm x 5mm对应的面积
            double area = 5e-3 * _app.MapControl.ReferenceScale * 5e-3 * _app.MapControl.ReferenceScale;

            //选中inFC中面积小于area的所有要素
            _app.MapControl.Map.ClearSelection();//清理所选要素

            IQueryFilter pQueryFilter = new QueryFilterClass();
            string areaFieldName = inFeaLayer.FeatureClass.AreaField.AliasName;
            pQueryFilter.WhereClause = string.Format("{0} < {1}", areaFieldName, area);
            IFeatureCursor fCursor = inFeaLayer.FeatureClass.Search(pQueryFilter, false);
            IFeature pFeature = fCursor.NextFeature();
            while (pFeature != null)
            {
                _app.MapControl.Map.SelectFeature(inFeaLayer, pFeature);

                pFeature = fCursor.NextFeature();
            }
            Marshal.ReleaseComObject(fCursor);

            //清除
            ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
            gp.OverwriteOutput = true;
            gp.AddOutputsToMap = false;
            gp.SetEnvironmentValue("workspace", _app.Workspace.EsriWorkspace.PathName);

            ESRI.ArcGIS.DataManagementTools.Eliminate eli = new ESRI.ArcGIS.DataManagementTools.Eliminate();
            eli.in_features = inFeaLayer;
            eli.out_feature_class = _app.Workspace.EsriWorkspace.PathName + "\\地貌";
            var geoRe = SMGI.Common.Helper.ExecuteGPTool(gp, eli, null);
            if (geoRe.Status != ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
            {
                return null;
            }

            //打开要素类
            IFeatureClass fc = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass("地貌");
            IFeatureLayer outFeatureLayer = new FeatureLayerClass();
            outFeatureLayer.FeatureClass = fc;
            outFeatureLayer.Name = fc.AliasName;

            return outFeatureLayer;
        }
    }
}

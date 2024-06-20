using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using System.IO;
using System.Xml.Linq;
namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 数据服务器（栅格）
    /// </summary>
    public class RasterDataServer
    {
        private GApplication _app;//应用程序
        private string _ipAddress;//服务器IP
        private string _userName;//用户名
        private string _passWord;//密码
        private string _databaseName;//数据库名
        private string _demRaster = "dem";
        private double bufferDis = 75;
        public RasterDataServer(GApplication app, string ipAddress, string userName, string passWord, string databaseName)
        {
            _app = app;

            _ipAddress = ipAddress;
            _userName = userName;
            _passWord = passWord;
            _databaseName = databaseName;
            var envFileName = @"专家库\DEM\DEMMapServer.xml";
           

          
            {
                XDocument doc = XDocument.Load(_app.Template.Root + @"\" + envFileName);
                var content = doc.Element("Template").Element("Content");
                var server = content.Element("Server");
                _ipAddress = server.Element("IPAddress").Value;
                _userName = server.Element("UserName").Value;
                _passWord = server.Element("Password").Value;
                _databaseName = server.Element("DataBase").Value;
                _demRaster = server.Element("DEMName").Value;
            }
        }
        public RasterDataServer(double buffer, GApplication app, string ipAddress, string userName, string passWord, string databaseName)
        {
            _app = app;
            bufferDis = buffer;
            _ipAddress = ipAddress;
            _userName = userName;
            _passWord = passWord;
            _databaseName = databaseName;
        }
        /// <summary>
        /// 将数据库中指定范围内的栅格数据附加到目标工作空间中
        /// </summary>
        /// <param name="ws">目标工作空间</param>
        /// <param name="outputSpatialReference">目标空间参考</param>
        /// <param name="clipGeo">范围</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public bool AttachRasterDataset2Wokespace(IWorkspace ws, ISpatialReference outputSpatialReference, IGeometry clipGeo, WaitOperation wo = null)
        {
            try
            {
               
                //获取源数据库中的所有要素类名称集合
                IWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(_ipAddress, _userName, _passWord, _databaseName);
                if (null == sdeWorkspace)
                {
                    MessageBox.Show("无法访问服务器！");
                    return false;
                }

                if (wo != null)
                    wo.SetText("正在裁切栅格数据...");

                //
                IEnumDataset enumDataset = sdeWorkspace.get_Datasets(esriDatasetType.esriDTAny);
                enumDataset.Reset();
                IDataset dataset = null;
                while ( (dataset = enumDataset.Next()) != null)
                {
                    if (dataset is IRasterDataset)
                    {
                        IRasterDataset rasterDataset = dataset as IRasterDataset;

                        if(AttachMosaicDataset2Wokespace(rasterDataset, clipGeo, ws, outputSpatialReference, "DEM"))
                            break;
                    }
                    else if (dataset is IRasterCatalog)
                    {

                    }
                    else if (dataset is IMosaicDataset)
                    {
                        
                    }
                    else
                    {

                    }
                }
                Marshal.ReleaseComObject(enumDataset);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(string.Format("裁切栅格数据失败:{0}", ex.Message));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 将栅格数据集中指定范围内的数据附加到目标工作空间
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="clipGeo"></param>
        /// <param name="ws"></param>
        /// <param name="outputSpatialReference"></param>
        /// <param name="clipDatasetName"></param>
        /// <returns></returns>
        private bool AttachMosaicDataset2Wokespace(IRasterDataset dataset, IGeometry clipGeo, IWorkspace ws, ISpatialReference outputSpatialReference,  string clipDatasetName = "DEM")
        {
            bool res = true;

            IRaster clipRaster = CommonMethods.clipRaterDataset(dataset, clipGeo,bufferDis);
            if (null == clipRaster)
                return false;

            IRasterDataset clipRasterDataset = null;
            if ((clipRaster as IGeoDataset).SpatialReference.Name != outputSpatialReference.Name)
            {
                Geoprocessor gp = new Geoprocessor();

                ProjectRaster project = new ProjectRaster();
                project.in_raster = clipRaster;
                project.out_coor_system = outputSpatialReference;
                project.out_raster = ws.PathName + "\\" + clipDatasetName;
                var geoRe = SMGI.Common.Helper.ExecuteGPTool(gp, project, null);

                res = geoRe.Status == esriJobStatus.esriJobSucceeded;
            }
            else
            {
                IRasterValue rasterValue = new RasterValueClass();
                rasterValue.Raster = clipRaster;

                //将裁切得到的栅格添加到栅格数据集
                IRasterWorkspaceEx rasterWorkspace = ws as IRasterWorkspaceEx;
                clipRasterDataset = rasterWorkspace.SaveAsRasterDataset(clipDatasetName, clipRaster, rasterValue.RasterStorageDef, "", null, null);

                //ISaveAs2 saveas = clipRaster as ISaveAs2;
                //clipRasterDataset = saveas.SaveAsRasterDataset(clipDatasetName, ws, "GDB", rasterValue.RasterStorageDef);

                if (clipRasterDataset != null)
                    clipRasterDataset.PrecalculateStats(0);
                else
                    res = false;
            }
            
            return res;
        }
    }
}

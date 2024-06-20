using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Xml.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.IO;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 数据下载
    /// 2020@LZ
    /// </summary>
    [SMGIAutomaticCommand]
    public class DataBaseDownLoadCmd : SMGICommand
    {
        public DataBaseDownLoadCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null &&
                    ClipElement.GetClipRangeElement(m_Application.MapControl.ActiveView.GraphicsContainer) != null;
            }
        }

        public override void OnClick()
        {
            var content = EnvironmentSettings.getContentElement(m_Application);
            var server = content.Element("Server");
            string ipAddress = server.Element("IPAddress").Value;
            string userName = server.Element("UserName").Value;
            string passWord = server.Element("Password").Value;
            double scale = double.Parse(content.Element("MapScale").Value);
            string databaseName="", mapTemplateName="";
            bool res = getDatabaseInfo(scale, ref databaseName, ref mapTemplateName);
            if (!res)
            {
                MessageBox.Show(string.Format("系统不支持当前比例尺【{0}】的地图制图！", scale));
                return;
            }

            
            DataDownLoadForm frm = new DataDownLoadForm(m_Application, ipAddress, userName, passWord, databaseName, scale);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            using (var wo = GApplication.Application.SetBusy())
            {
                m_Application.MapControl.CurrentTool = null;

                wo.SetText("正在获取元数据信息......");
                #region 参数信息
                //空间参考
                ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
                if (null == targetSpatialReference)
                {
                    MessageBox.Show("未设置输出数据库的空间参考信息！");
                    return;
                }
                //裁切面
                IGeometry clipGeo = ClipElement.GetClipRangeElement(m_Application.MapControl.ActiveView.GraphicsContainer).Geometry;
                if (null == clipGeo)
                {
                    MessageBox.Show("无效的裁切面！");
                    return;
                }
                //页面矩形
                IGeometry pageGeo = ClipElement.createPageRect(m_Application, targetSpatialReference, "出版范围");
                //内图廓
                IGeometry inlineGeo = ClipElement.createInlinePolyline(m_Application, targetSpatialReference, "内图廓");
                //外图廓
                IGeometry outlineGeo = ClipElement.createInlinePolyline(m_Application, targetSpatialReference, "外图廓");
                //丁字线
                List<IGeometry> tLineGeoList = null;
                if(frm.NeedCutLine)
                    tLineGeoList = ClipElement.createMapTlinePolyline(m_Application, targetSpatialReference);
                #endregion

                //数据下载
                res = DataDownLoad(frm.IPAdress, frm.DataBase, frm.UserName, frm.Password, scale, 
                    targetSpatialReference, clipGeo, pageGeo, inlineGeo, outlineGeo, tLineGeoList, 
                    frm.FeatureClassNameList, frm.SelectedThematicList, frm.NeedClipDEM, frm.NeedClipAttach, 
                    frm.OutputGDB, wo);
                
            }

            if (res)
            {
                
                //数据结构升级
                DataBaseStructUpgradeForm upgradeFrm = new DataBaseStructUpgradeForm(m_Application, frm.OutputGDB);
                upgradeFrm.StartPosition = FormStartPosition.CenterParent;
                if (upgradeFrm.ShowDialog() != DialogResult.OK)
                    return;

                using (var wo = GApplication.Application.SetBusy())
                {
                    //获取配置信息
                    string mxdFullFileName = EnvironmentSettings.getMxdFullFileName(m_Application);
                    string ruleMatchFileName = EnvironmentSettings.getLayerRuleDBFileName(m_Application);


                    res = DataBaseStructUpgradeCmd.DataBaseStructUpgrade(upgradeFrm.SourceGDBFile, upgradeFrm.OutputGDBFile, upgradeFrm.Mapscale,
                        upgradeFrm.NeedAttachMap, mxdFullFileName, ruleMatchFileName, wo);
                }
            }

            if (res)
            {
                MessageBox.Show("操作完毕！");
            }
        }
        
        /// <summary>
        /// 命令行参数调用
        /// </summary>
        /// <param name="args"></param>
        /// <param name="messageRaisedAction"></param>
        /// <returns></returns>
        protected override bool DoCommand(XElement args, Action<string> messageRaisedAction)
        {
            bool res = false;

            try
            {
                messageRaisedAction("正在解析数据下载相关参数......");
                #region 底图服务器信息
                string outputGDB = args.Element("OutPutGDB").Value.Trim();
                string ipAddress = args.Element("IP").Value.Trim();
                string userName = args.Element("UserName").Value.Trim();
                string passWord = args.Element("PassWord").Value.Trim();
                double scale = double.Parse(args.Element("Scale").Value.Trim());
                string dbName = "";
                #region
                var expertiseContent = ExpertiseDatabase.getContentElement(m_Application);
                var scaleItems = expertiseContent.Element("MapScaleRule").Elements("Item");
                foreach (XElement ele in scaleItems)
                {
                    double min = double.Parse(ele.Element("Min").Value);
                    double max = double.Parse(ele.Element("Max").Value);
                    if (scale >= min && scale <= max)
                    {
                        dbName = ele.Element("DatabaseName").Value;
                        break;
                    }
                }
                #endregion
                #endregion

                #region 空间参考
                string srName = args.Element("SpatialReference").Value.Trim();
                ISpatialReferenceFactory spatialRefFactory = new SpatialReferenceEnvironmentClass();
                ISpatialReference sr = spatialRefFactory.CreateESRISpatialReferenceFromPRJFile(srName);
                #endregion

                #region 裁切面
                string clipGeoJson = args.Element("ClipGeoJson").Value.Trim();
                IGeometry clipGeo = CommonMethods.GeometryFromJsonString(clipGeoJson, esriGeometryType.esriGeometryPolygon);
                #endregion

                #region 页面
                string pageGeoJson = args.Element("PageGeoJson").Value.Trim();
                IGeometry pageGeo = CommonMethods.GeometryFromJsonString(pageGeoJson, esriGeometryType.esriGeometryPolygon);
                #endregion

                #region 图廓线
                string inlineGeoJson = args.Element("InlineGeoJson").Value.Trim();
                IGeometry inlineGeo = CommonMethods.GeometryFromJsonString(inlineGeoJson, esriGeometryType.esriGeometryPolyline);

                string outlineGeoJson = args.Element("OutlineGeoJson").Value.Trim();
                IGeometry outlineGeo = CommonMethods.GeometryFromJsonString(outlineGeoJson, esriGeometryType.esriGeometryPolyline);
                #endregion

                #region 丁字线
                List<IGeometry> tLineGeoList = null;
                var tLineItem = args.Element("ListTlineGeoJson");
                if (tLineItem != null)
                {
                    tLineGeoList = new List<IGeometry>();
                    foreach (var ele in tLineItem.Elements("TlineGeoJson"))
                    {
                        tLineGeoList.Add(CommonMethods.GeometryFromJsonString(ele.Value, esriGeometryType.esriGeometryPolyline));
                    }
                }
                #endregion

                #region 专题数据信息
                List<ThematicDataInfo> thematicList = new List<ThematicDataInfo>();
                var eleThematic = args.Element("Thematic");
                if (eleThematic != null)
                {
                    foreach (var item in eleThematic.Elements("Item"))
                    {
                        ThematicDataInfo info = new ThematicDataInfo();
                        info.Name = item.Element("Name").Value;
                        info.IP = item.Element("IP").Value;
                        info.UserName = item.Element("UserName").Value;
                        info.Password = item.Element("Password").Value;
                        info.DataBase = item.Element("DataBase").Value;
                        //图层
                        var lyrs = item.Elements("Layers");
                        foreach (var ele in lyrs.Elements("Layer"))
                        {
                            info.Lyrs[ele.Value] = true;
                        }
                        thematicList.Add(info);
                    }
                }
                #endregion

                #region DEM
                bool needDEM = false;
                if (args.Element("DEM") != null)
                    needDEM = bool.Parse(args.Element("DEM").Value.Trim());
                #endregion

                #region 附区
                bool needAttach = false;
                if (args.Element("Attach") != null)
                    needAttach = bool.Parse(args.Element("Attach").Value.Trim());
                #endregion


                messageRaisedAction("正在进行数据下载......");
                res = DataDownLoad(ipAddress, dbName, userName, passWord, scale,
                    sr, clipGeo, pageGeo, inlineGeo, outlineGeo, tLineGeoList,
                    null, thematicList, needDEM, needAttach, outputGDB);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }

            return res;
        }


        /// <summary>
        /// 根据比例尺获取服务器数据库及模板信息
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="dbName"></param>
        /// <param name="mapTemplateName"></param>
        /// <returns></returns>
        public static bool getDatabaseInfo(double scale, ref string dbName, ref string mapTemplateName)
        {
            bool res = false;

            var expertiseContent = ExpertiseDatabase.getContentElement(GApplication.Application);
            var mapScaleRule = expertiseContent.Element("MapScaleRule");
            var scaleItems = mapScaleRule.Elements("Item");
            foreach (XElement ele in scaleItems)
            {
                double min = double.Parse(ele.Element("Min").Value);
                double max = double.Parse(ele.Element("Max").Value);
                double templateScale = double.Parse(ele.Element("Scale").Value);
                if (scale >= min && scale <= max)
                {
                    dbName = ele.Element("DatabaseName").Value;
                    mapTemplateName = ele.Element("MapTemplate").Value;

                    res = true;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// 数据下载
        /// </summary>
        /// <param name="ipAddress">服务器IP</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <param name="scale">比例尺</param>
        /// <param name="sr">空间参考</param>
        /// <param name="clipGeo">主区几何体</param>
        /// <param name="pageGeo">页面几何体</param>
        /// <param name="inlineGeo">内图廓</param>
        /// <param name="outlineGeo">外图廓</param>
        /// <param name="tLineGeoList">丁字线(为空时，则不生成丁字线)</param>
        /// <param name="fcNameList">需下载的基础数据要素类列表（为空时，则下载全部要素类）</param>
        /// <param name="thematicList">需下载的专题数据信息列表</param>
        /// <param name="needDEM">是否需要下载DEM</param>
        /// <param name="needAttach">是否需要下载邻区数据</param>
        /// <param name="outputGDB">数据数据库路径</param>
        /// <param name="wo">提示对象</param>
        /// <returns></returns>
        public static bool DataDownLoad(string ipAddress, string dbName, string userName, string passWord,
            double scale, ISpatialReference sr, IGeometry clipGeo, IGeometry pageGeo, IGeometry inlineGeo,
            IGeometry outlineGeo, List<IGeometry> tLineGeoList, List<string> fcNameList, List<ThematicDataInfo> thematicList, 
            bool needDEM, bool needAttach, string outputGDB, WaitOperation wo = null)
        {
            try
            {

                #region 1.下载 底图数据

                if (wo != null)
                    wo.SetText("正在下载基础底图数据......");
                DataServerClass ds = new DataServerClass(GApplication.Application, ipAddress, userName, passWord, dbName);
                if (needAttach)//是否需要下载邻区数据
                {
                    // 下载 主区及邻区底图数据
                    ds.DownExtensionLoadData(clipGeo, pageGeo, outputGDB, sr, fcNameList, wo);
                }
                else
                {
                    // 下载 岛状底图数据
                    ds.DownLoadData(clipGeo, outputGDB, sr, fcNameList, wo);
                }

                #region 增加要素类(裁切面、页面)
                clipGeo.Project(sr);
                IGeometry overlapGeo = ClipElement.createOvelapRect(inlineGeo, pageGeo);//压盖几何
                Dictionary<string, esriFieldType> name2Type = new Dictionary<string, esriFieldType>();
                name2Type.Add("TYPE", esriFieldType.esriFieldTypeString);
                IFeatureClass clipFC = ds.CreateFeatureClass(outputGDB, esriGeometryType.esriGeometryPolygon, name2Type, sr, "ClipBoundary");//ClipBoundary
                IFeatureClass llineFC = ds.CreateFeatureClass(outputGDB, esriGeometryType.esriGeometryPolyline, name2Type, sr, "LLINE");//LLINE
                IFeatureClass lpolyFC = ds.CreateFeatureClass(outputGDB, esriGeometryType.esriGeometryPolygon, name2Type, sr, "LPOLY");//LPOLY
                Dictionary<string, object> name2val = new Dictionary<string, object>();
                name2val.Add("TYPE", "页面");
                ds.addFeature2FC(clipFC, pageGeo, name2val);
                name2val.Clear();
                name2val.Add("TYPE", "裁切面");
                ds.addFeature2FC(clipFC, clipGeo, name2val);
                name2val.Clear();
                name2val.Add("TYPE", "遮盖");
                ds.addFeature2FC(lpolyFC, overlapGeo, name2val);
                name2val.Clear();
                name2val.Add("TYPE", "内图廓");
                ds.addFeature2FC(llineFC, inlineGeo, name2val);
                name2val.Clear();
                name2val.Add("TYPE", "外图廓");
                ds.addFeature2FC(llineFC, outlineGeo, name2val);
                name2val.Clear();
                if (tLineGeoList != null)
                {
                    name2val.Add("TYPE", "丁字线");
                    foreach (IGeometry geo in tLineGeoList)
                        ds.addFeature2FC(llineFC, geo, name2val);
                    name2val.Clear();
                }
                #endregion

                #endregion

                //打开目标数据库
                IWorkspaceFactory wsFactory = new FileGDBWorkspaceFactoryClass();
                IWorkspace outputWS = wsFactory.OpenFromFile(outputGDB, 0);

                #region 2.下载 DEM
                if (needDEM)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在裁切栅格数据..."));

                    RasterDataServer rasterDS = new RasterDataServer(GApplication.Application, ipAddress, userName, passWord, "dem");//dem栅格数据集的名称最好放置在配置文件中！！！
                    if (needAttach)
                    {
                        //如果要裁切附区，则裁切整个页面的DEM
                        rasterDS.AttachRasterDataset2Wokespace(outputWS, sr, pageGeo);
                    }
                    else
                    {
                        rasterDS.AttachRasterDataset2Wokespace(outputWS, sr, clipGeo);
                    }
                }
                #endregion

                
                #region 3.下载 专题
                CommonMethods.ThemDataBase = "";
                CommonMethods.ThemData = false;
                foreach (var themItem in thematicList)
                {
                    ds = new DataServerClass(GApplication.Application, themItem.IP, themItem.UserName, themItem.Password, themItem.DataBase);
                    bool isuccess = ds.DownLoadThematicData(clipGeo, outputWS, sr, themItem);
                    if (!isuccess)
                    {
                        throw new Exception(string.Format("下载专题数据【{0}】失败！", themItem.DataBase));
                    }
                    CommonMethods.ThemDataBase = themItem.Name;
                    CommonMethods.ThemData = true;
                }
                #endregion

                #region 4.存储配置信息
                Config.CreateConfigTable(outputWS);
                var config = Config.Open(outputWS as IFeatureWorkspace);
                EnvironmentSettings.UpdateEnvironmentToConfig(config, needAttach);
                #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                return false;
            }

            return true;
        }
    }
}

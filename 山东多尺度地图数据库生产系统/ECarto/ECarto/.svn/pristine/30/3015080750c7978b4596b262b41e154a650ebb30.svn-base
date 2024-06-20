using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geoprocessor;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Data;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Linq;
 

using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Geometry;
using System.Xml.Linq;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;
namespace SMGI.Plugin.ThematicChart
{
    public class CommonMethods
    {
        //裁切数据的空间参考
        public static string clipSpatialRefFileName = GApplication.ExePath + @"\..\Projection\China_2000GCS_Albers.prj";
        public static ISpatialReference getClipSpatialRef()
        {
            ISpatialReference clipRef = null;

            ISpatialReferenceFactory pSpatialRefFactory = new SpatialReferenceEnvironmentClass();
            clipRef = pSpatialRefFactory.CreateESRISpatialReferenceFromPRJFile(clipSpatialRefFileName);
            if (null == clipRef)
            {
                MessageBox.Show("无效的投影文件!");
                return null;
            }

            return clipRef;
        }
        //打开文件数据库
        public static void OpenGDBFile(GApplication app, string fullFileName)
        {
            if (app.Workspace != null)
            {
                app.Workspace.Close();
            }
            if (!GApplication.GDBFactory.IsWorkspace(fullFileName))
            {
                MessageBox.Show("GDB未升级！");
            }
            //Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            //IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
            //    (factoryType);
            //IWorkspace ws = workspaceFactory.OpenFromFile(fullFileName, 0);

            IWorkspace ws = GApplication.GDBFactory.OpenFromFile(fullFileName, 0);
            if (GWorkspace.IsWorkspace(ws))
            {
                app.OpenESRIWorkspace(ws);
            }
            else
            {
                app.InitESRIWorkspace(ws);
            }
        }

        //模板匹配
        public static void MatchLayer(GApplication app, ILayer layer, IGroupLayer parent, DataTable dtLayerRule)
        {
            if (parent == null)
            {
                app.Workspace.Map.AddLayer(layer);
            }
            else
            {
                (parent as IGroupLayer).Add(layer);
            }

            if (layer is IGroupLayer)
            {
                var l = (layer as ICompositeLayer);

                List<ILayer> layers = new List<ILayer>();
                for (int i = 0; i < l.Count; i++)
                {
                    layers.Add(l.get_Layer(i));
                }
                (layer as IGroupLayer).Clear();
                foreach (var item in layers)
                {
                    MatchLayer(app, item, layer as IGroupLayer, dtLayerRule);
                }
            }
            else
            {
                string name = ((layer as IDataLayer2).DataSourceName as IDatasetName).Name;
                if (layer is IFeatureLayer)
                {
                    if ((app.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name))
                    {
                        IFeatureClass fc = (app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(name);
                        (layer as IFeatureLayer).FeatureClass = fc;


                        DataRow[] drArray = dtLayerRule.Select().Where(i => i["图层"].ToString().Trim() == name).ToArray();
                        if (drArray.Length != 0)
                        {
                            for (int i = 0; i < drArray.Length; i++)
                            {
                                string ruleFieldName = drArray[i]["RuleIDFeildName"].ToString();
                                object ruleID = drArray[i]["RuleID"];
                                object whereClause = drArray[i]["定义查询"];

                                IQueryFilter qf = new QueryFilterClass();
                                qf.WhereClause = whereClause.ToString();

                                try
                                {
                                    IFeatureCursor fCursor = fc.Update(qf, true);
                                    IFeature f = null;
                                    while ((f = fCursor.NextFeature()) != null)
                                    {
                                        f.set_Value(fc.FindField(ruleFieldName), ruleID);
                                        fCursor.UpdateFeature(f);
                                    }
                                    Marshal.ReleaseComObject(fCursor);
                                }
                                catch (Exception ex)
                                {
                                    //MessageBox.Show(ex.Message);
                                }

                            }
                        }
                    }
                }
                else if (layer is IRasterLayer)
                {
                    if ((app.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTRasterDataset, name))
                    {
                        (layer as IRasterLayer).CreateFromRaster((app.Workspace.EsriWorkspace as IRasterWorkspaceEx).OpenRasterDataset(name).CreateDefaultRaster());
                    }
                }
            }
        }


        public static void updateMapScale(GApplication app, double scale)
        {
            var envFileName = app.Template.Content.Element("EnvironmentSettings").Value;
            string fileName = app.Template.Root + @"\" + envFileName;
            FileInfo f = new FileInfo(fileName);
          
            using (var fst = f.Open(FileMode.Open))
            {

                
                 XDocument   doc = XDocument.Load(fst);

                 var content = doc.Element("Template").Element("Content");
                 var pageSizeEle = content.Element("PageSize");


                XElement expertiseContent = ExpertiseDatabase.getContentElement(app);
                var mapScaleRule = expertiseContent.Element("MapScaleRule");
                var scaleItems = mapScaleRule.Elements("Item");
                foreach (XElement ele in scaleItems)
                {
                    double min = double.Parse(ele.Element("Min").Value);
                    double max = double.Parse(ele.Element("Max").Value);
                    if (scale >= min && scale <= max)
                    {
                        XMLHelper.EditPage("MapScale", scale.ToString());
                        XMLHelper.EditPage("DatabaseName", ele.Element("DatabaseName").Value);
                        XMLHelper.EditPage("MapTemplate", ele.Element("MapTemplate").Value);
                        XMLHelper.UpdatePageInfos();
                        break;
                    }
                }

            }
           

        }

        /// <summary>
        /// 清除锚点重合的专题图表
        /// </summary>
        /// <param name="centerPoint">锚点</param>
        /// <param name="lpointFC"></param>
        /// <param name="annoFC"></param>
        public static void ClearThematicCarto(IPoint centerPoint,IFeatureClass lpointFC,IFeatureClass annoFC)
        {
            ISpatialFilter sf = new SpatialFilterClass();
            sf.WhereClause = string.Format("TYPE = '专题图'");
            sf.Geometry = centerPoint;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor lpointCursor = lpointFC.Search(sf, false);
            IFeature feature = null;
            if ((feature = lpointCursor.NextFeature()) != null)
            {
                int featureId = feature.OID;
                int annotationClassID = feature.Class.ObjectClassID;
                #region 清除专题图表关联的注记
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("FeatureID = {0} AND AnnotationClassID = {1}", featureId, annotationClassID);
                IFeatureCursor annoCursor = annoFC.Search(qf, false);
                IFeature annoFeature = null;
                while ((annoFeature = annoCursor.NextFeature()) != null)
                {
                    annoFeature.Delete();
                }
                Marshal.ReleaseComObject(annoCursor);
                #endregion
                feature.Delete();
            }
            Marshal.ReleaseComObject(lpointCursor);
        }
    }
}

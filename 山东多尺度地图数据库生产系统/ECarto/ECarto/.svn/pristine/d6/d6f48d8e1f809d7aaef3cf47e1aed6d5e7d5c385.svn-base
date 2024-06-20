using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ESRI.ArcGIS.Controls;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Xml.Linq;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Maplex;
using ESRI.ArcGIS.Display;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 基于规则表，使用maplex标注引擎进行要素标注
    /// </summary>
    [SMGIAutomaticCommand]
    public class MaplexAnnotateCmd : SMGI.Common.SMGICommand
    {
        public MaplexAnnotateCmd()
        {
            m_caption = "标注要素（Maplex）";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            //获取内图廓线范围
            string mapBorderLineLayerName = "LLine";
            if (!(m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mapBorderLineLayerName))
            {
                MessageBox.Show(string.Format("当前数据库缺少图层[{0}]!", mapBorderLineLayerName), "警告", MessageBoxButtons.OK);
                return;
            }
            IFeatureClass mapBorderFeatureClass = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(mapBorderLineLayerName);
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE='内图廓'";
            int ct = mapBorderFeatureClass.FeatureCount(qf);
            if (ct == 0)
            {
                MessageBox.Show("请先生成内图廓");
                return;
            }
            IFeatureCursor pCursor = mapBorderFeatureClass.Search(qf, false);
            IFeature fLine = pCursor.NextFeature();
            IPolyline extentGeo = fLine.ShapeCopy as IPolyline;
            Marshal.ReleaseComObject(pCursor);

            //生成注记
            DialogResult dialogResult = MessageBox.Show("批量创建可能会删除所有注记，确定要全部生成注记吗？", "提示", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes)
                return;

            string wr = "";
            string ruleMatchFileName = EnvironmentSettings.getAnnoRuleDBFileName(m_Application);
            DataTable ruleTable = CommonMethods.ReadToDataTable(ruleMatchFileName, "注记规则");
            DataTable barrierRuleTable = CommonMethods.ReadToDataTable(ruleMatchFileName, "避让规则");
            DataTable fontmappingTable = CommonMethods.ReadToDataTable(ruleMatchFileName, "字体映射");
            //添加专题注记规则
            AddThematicAnnoRule(ruleTable);
            //设置窗体
            var frm = new FrmAnnoLyr(ruleTable,ruleMatchFileName);
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            ruleTable = frm.targetDt;
            using (var wo = m_Application.SetBusy())
            {
                m_Application.EngineEditor.StartOperation();
                //获取注记要素类
                Dictionary<string, IFeatureClass> annoName2FeatureClass = new Dictionary<string, IFeatureClass>();
                System.Data.DataTable dtAnnoLayers = ruleTable.AsDataView().ToTable(true, new string[] { "注记图层" });//distinct
                for (int i = 0; i < dtAnnoLayers.Rows.Count; ++i)
                {
                    //图层名
                    string annoLayerName = dtAnnoLayers.Rows[i]["注记图层"].ToString().Trim();
                    if (!(m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, annoLayerName))
                    {
                        MessageBox.Show(string.Format("当前数据库缺少注记要素[{0}]!", annoLayerName), "警告", MessageBoxButtons.OK);
                        return;
                    }

                    IFeatureClass annoFeatureClass = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(annoLayerName);

                    annoName2FeatureClass.Add(annoLayerName, annoFeatureClass);
                }

                if (annoName2FeatureClass.Count == 0)
                {
                    MessageBox.Show("规则库中没有指定注记目标图层!", "警告", MessageBoxButtons.OK);
                    return;
                }

                MaplexAnnotateCreate ac = new MaplexAnnotateCreate(m_Application);
                wr = ac.createAnnotate(ruleTable, barrierRuleTable, fontmappingTable, annoName2FeatureClass, extentGeo, wo,frm.Reserve);
                wo.SetText("创建注记分类..");
                CreateAnnoLable(ruleTable);
                m_Application.EngineEditor.StopOperation("创建注记");
            }

            if (wr != "")
            {
                MessageBox.Show(wr, "警告", MessageBoxButtons.OK);
            }
        }
        //添加专题图注记规则
        private void AddThematicAnnoRule(DataTable baseRuleTable)
        {
            Dictionary<string, string> envString = m_Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            bool themFlag = false;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            if (envString.ContainsKey("ThemExist"))
                themFlag = bool.Parse(envString["ThemExist"]);
            if (!themFlag)
                return;
            string dirName = envString["ThemDataBase"];
            string dirpath = GApplication.Application.Template.Root + "\\专题\\";
            {
                string mdbpath = dirpath + dirName + "\\规则对照.mdb";
                if (File.Exists(mdbpath))
                {
                    DataTable ruleTable = CommonMethods.ReadToDataTable(mdbpath, "注记规则");
                    //图层名
                    for (int i = 0; i < ruleTable.Rows.Count; i++)
                    {
                        string annoLayerName = ruleTable.Rows[i]["图层"].ToString().Trim();
                        var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == annoLayerName);
                        })).FirstOrDefault() as IFeatureLayer;
                        if (lyr == null)
                            continue;
                        //存在
                        if ((m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, annoLayerName))
                        {
                            baseRuleTable.Rows.Add(ruleTable.Rows[i].ItemArray);
                        }
                    }
                }
            }
        }

        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                string sourceFileGDB = GApplication.Application.Workspace.FullName;
                if (m_Application.Workspace == null)
                    return false;
                messageRaisedAction("正在注记生成...");
                Dictionary<string, bool> annoRuleItems = new Dictionary<string, bool>();
                foreach (var item in args.Elements("RuleItem"))
                {
                    string name = item.Attribute("Name").Value;
                    annoRuleItems[name] = true;
                }
                string mapBorderLineLayerName = "LLine";
                if (!(m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mapBorderLineLayerName))
                {
                    MessageBox.Show(string.Format("当前数据库缺少图层[{0}]!", mapBorderLineLayerName), "警告", MessageBoxButtons.OK);
                    return false;
                }
                IFeatureClass mapBorderFeatureClass = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(mapBorderLineLayerName);
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "TYPE='内图廓'";
                int ct = mapBorderFeatureClass.FeatureCount(qf);
                if (ct == 0)
                {
                    MessageBox.Show("请先生成内图廓");
                    return false;
                }
                IFeatureCursor pCursor = mapBorderFeatureClass.Search(qf, false);
                IFeature fLine = pCursor.NextFeature();
                IPolyline extentGeo = fLine.ShapeCopy as IPolyline;
                Marshal.ReleaseComObject(pCursor);


                string wr = "";
                double scale = (m_Application.ActiveView as IMap).ReferenceScale;
                string ruleMatchFileName = string.Empty;
                {
                    string MapTemplate = string.Empty;
                    string ruleDataBaseFileName = m_Application.Template.Content.Element("RuleDataBase").Value;
                    #region 1 获取关系图层对照表
                 
                    var expertiseContent = ExpertiseDatabase.getContentElement(m_Application);
                    var mapScaleRule = expertiseContent.Element("MapScaleRule");
                    var scaleItems = mapScaleRule.Elements("Item");
                    foreach (XElement ele in scaleItems)
                    {
                        double min = double.Parse(ele.Element("Min").Value);
                        double max = double.Parse(ele.Element("Max").Value);
                        double templateScale = double.Parse(ele.Element("Scale").Value);
                        if (scale >= min && scale <= max)
                        {
                           var DatabaseName = ele.Element("DatabaseName").Value;
                            MapTemplate = ele.Element("MapTemplate").Value;

                        }
                    }

                    ruleMatchFileName = EnvironmentSettings.getAnnoRuleDBFileName(m_Application);
                    messageRaisedAction(string.Format("注记规则表路径：{0}",ruleMatchFileName));
                  
                    #endregion
                }
                DataTable ruleTable = CommonMethods.ReadToDataTable(ruleMatchFileName, "注记规则");
                if (ruleTable == null) 
                    return false;
                DataTable barrierRuleTable = CommonMethods.ReadToDataTable(ruleMatchFileName, "避让规则");
                DataTable fontmappingTable = CommonMethods.ReadToDataTable(ruleMatchFileName, "字体映射");
                //添加专题注记规则
                AddThematicAnnoRule(ruleTable);
                //设置窗体
                DataTable ruleTableClone = ruleTable.Clone();
                for(int i=0;i<ruleTable.Rows.Count;i++)
                {
                    DataRow dr = ruleTable.Rows[i];
                    string lyr = dr["图层"].ToString();
                    string id = dr["ID"].ToString();
                    if (annoRuleItems.ContainsKey(lyr + "_" + id))
                    {
                        ruleTableClone.ImportRow(dr);
                    }
                    
                }
              
                //获取注记要素类
                Dictionary<string, IFeatureClass> annoName2FeatureClass = new Dictionary<string, IFeatureClass>();
                System.Data.DataTable dtAnnoLayers = ruleTable.AsDataView().ToTable(true, new string[] { "注记图层" });//distinct
                for (int i = 0; i < dtAnnoLayers.Rows.Count; ++i)
                {
                    //图层名
                    string annoLayerName = dtAnnoLayers.Rows[i]["注记图层"].ToString().Trim();
                    if (!(m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, annoLayerName))
                    {
                        MessageBox.Show(string.Format("当前数据库缺少注记要素[{0}]!", annoLayerName), "警告", MessageBoxButtons.OK);
                        return false;
                    }

                    IFeatureClass annoFeatureClass = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(annoLayerName);

                    annoName2FeatureClass.Add(annoLayerName, annoFeatureClass);
                }

                if (annoName2FeatureClass.Count == 0)
                {
                    MessageBox.Show("规则库中没有指定注记目标图层!", "警告", MessageBoxButtons.OK);
                    return false;
                }

                MaplexAnnotateCreate ac = new MaplexAnnotateCreate(m_Application);

                wr = ac.createAnnotate(ruleTableClone, barrierRuleTable, fontmappingTable, annoName2FeatureClass, extentGeo);
                //创建注记分类
                CreateAnnoLable(ruleTableClone);
               
                return true;

            }
            catch(Exception ex)
            {

                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
        }
        private void CreateAnnoLable(DataTable annoRuleTable)
        {
            GApplication app = GApplication.Application;
           
            IFeatureClass annofcl = (app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass("ANNO");

            List<AnnoIDInfo> idInfo = new List<AnnoIDInfo>();
            IFeatureCursor cursor = annofcl.Search(null, false);
            IFeature fe;
            while ((fe = cursor.NextFeature()) != null)
            { 
                IAnnotationFeature2 annofe = fe as IAnnotationFeature2;
                idInfo.Add(new AnnoIDInfo { AnnoID = fe.OID, ReFeID = annofe.LinkedFeatureID, ClassID = annofe.AnnotationClassID });
            }
            Marshal.ReleaseComObject(cursor);
            for (int i = 0; i < annoRuleTable.Rows.Count; i++)
            {
                try
                {
                    DataRow dr = annoRuleTable.Rows[i];
                    for (int j = 0; j < dr.Table.Columns.Count; j++)
                    {
                        object val = dr[j];
                        if (val == null || Convert.IsDBNull(val))
                            dr[j] = "";
                    }

                    string featureClassName = dr["图层"].ToString().Trim();
                    string condition = dr["查询条件"].ToString();
                    int classID = -1;
                    List<int> featureIDs = GetFeaturesInFeatureClass(featureClassName, condition, out classID);
                    if (featureIDs.Count == 0)
                        continue;
                    string type = dr["注记说明"].ToString();
                    foreach (var id in featureIDs)
                    {
                       var infos=  idInfo.Where(t => t.ClassID == classID && t.ReFeID == id);
                       if (infos.Count()>0)
                       {
                           foreach (var info in infos)
                           {
                               int annoid = info.AnnoID;
                               IFeature feUpdate = annofcl.GetFeature(annoid);
                               feUpdate.set_Value(annofcl.FindField("分类"), type);
                               feUpdate.Store();
                           }
                       }
                    }
                }
                catch
                {
                }
            }
        }
        private List<int> GetFeaturesInFeatureClass(string featureClassName, string condition,out int classID)
        {
            List<int> featureIDList = new List<int>();
            GApplication _app = GApplication.Application;
            classID = -1;
            try
            {
                IFeatureClass featureClass = null;
                if ((_app.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, featureClassName))
                {
                    featureClass = (_app.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(featureClassName);
                }
                if (featureClass == null)
                {
                    return featureIDList;
                }
                classID = featureClass.ObjectClassID;
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = condition.Replace("[", "").Replace("]", "");//替换掉中括号，兼容mdb和gdb
                if (queryFilter.WhereClause == "")
                {
                    queryFilter = null;
                }
                if (featureClass.FeatureCount(queryFilter) > 0)
                {
                    IFeatureCursor featureCursor = featureClass.Search(queryFilter, false);
                    IFeature feature = null;
                    while ((feature = featureCursor.NextFeature()) != null)
                    {
                        featureIDList.Add(feature.OID);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return featureIDList;
        }
        class AnnoIDInfo
        {
            public int AnnoID;
            public int ReFeID;
            public int ClassID;
        }
    }
   
        
}

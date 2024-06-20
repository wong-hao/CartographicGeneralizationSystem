using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Data;
using System.IO;
using ESRI.ArcGIS.Carto;
using SMGI.Common;

namespace SMGI.Plugin.AnnotationEngine
{
    /// <summary>
    /// 道路注记创建（RTEG、RN）
    /// </summary>
    public class RoadAnnotationCreateCmd : SMGI.Common.SMGICommand
    {
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null &&
                       m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public RoadAnnotationCreateCmd()
        {
            m_caption = "道路注记创建";
        }


        public override void OnClick()
        {
            string result0=CheckRoadAnnotationCreate(m_Application);
            if (!string.IsNullOrEmpty(result0))
            {
                MessageBox.Show(result0, "提示");
                return;
            }

            string result1=DoRoadAnnotationCreate(m_Application,m_caption);
            if (!string.IsNullOrEmpty(result1))
            {
                MessageBox.Show(result0, "提示");
                return;
            }
            MessageBox.Show(string.Format("【{0}】处理完成!", m_caption), "提示");

            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
        }

        #region 提取OnClick判断事务
        public static string CheckRoadAnnotationCreate(GApplication m_Application)
        {
            string result = string.Empty;
            if (m_Application.ActiveView.FocusMap.ReferenceScale == 0)
            {
                result += string.Format("数据层：参考比例尺未进行设置\r\n");
            }

            string fcName = "LRDL";
            IFeatureLayer roadLayer = m_Application.Workspace.LayerManager.GetLayer(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName == fcName
                    && ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;
            }).FirstOrDefault() as IFeatureLayer;
            if (roadLayer == null)
            {
                result += string.Format("数据层：图层{0}缺失\r\n",fcName);
            }

            return result;
        }
        #endregion

        #region 提取OnClick逻辑事务
        public static string DoRoadAnnotationCreate(GApplication m_Application, string m_caption)
        {
            string result = string.Empty;

            string fcName = "LRDL";
            IFeatureLayer roadLayer = m_Application.Workspace.LayerManager.GetLayer(l =>
            {
                return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName == fcName
                    && ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;
            }).FirstOrDefault() as IFeatureLayer;

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    Dictionary<string, List<DataRow>> sql2Rows = new Dictionary<string, List<DataRow>>();
                    wo.SetText(string.Format("【{0}】：", m_caption) + "正在读取配置规则");
                    #region 读取规则表
                    XElement contentXEle = m_Application.Template.Content;
                    XElement annoRuleEle = contentXEle.Element("AnnoFull");
                    string annoRuleFilePath = m_Application.Template.Root + "\\" + annoRuleEle.Value;
                    if (!File.Exists(annoRuleFilePath))
                    {
                        result += (string.Format("数据层：注记规则库【{0}】缺失\r\n", annoRuleFilePath));

                        return result;
                    }
                    DataTable ruleTable = CommonMethods.ReadToDataTable(annoRuleFilePath, "注记规则");
                    DataTable fontmappingTable = CommonMethods.ReadToDataTable(annoRuleFilePath, "字体映射");
                    DataTable specialCharacterTable = CommonMethods.ReadToDataTable(annoRuleFilePath, "特殊字符");

                    //获取注记要素类
                    Dictionary<string, IFeatureClass> annoName2FeatureClass = new Dictionary<string, IFeatureClass>();
                    System.Data.DataTable dtAnnoLayers = ruleTable.AsDataView().ToTable(true, new string[] { "注记要素类名" });//distinct
                    for (int i = 0; i < dtAnnoLayers.Rows.Count; ++i)
                    {
                        //图层名
                        string annoFCName = dtAnnoLayers.Rows[i]["注记要素类名"].ToString().ToUpper().Trim();
                        if (!(m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, annoFCName))
                        {
                            result += (string.Format("数据层：注记要素类【{0}】缺失\r\n", annoFCName));
                            return result;                          
                        }

                        IFeatureClass annoFeatureClass = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(annoFCName);

                        annoName2FeatureClass.Add(annoFCName, annoFeatureClass);
                    }

                    DataRow[] drs = ruleTable.Select().Where(i => i["要素类名"].ToString().ToUpper() == fcName).ToArray();
                    for (int i = 0; i < drs.Length; i++)
                    {
                        string annoclass = drs[i]["规则分类名"].ToString().Trim();
                        if (annoclass == "主干道" || annoclass == "次干道" || annoclass == "支线")
                            continue;

                        string condition = drs[i]["查询条件"].ToString().Trim();
                        if (sql2Rows.ContainsKey(condition))
                        {
                            sql2Rows[condition].Add(drs[i]);
                        }
                        else
                        {
                            List<DataRow> rows = new List<DataRow>();
                            rows.Add(drs[i]);

                            sql2Rows.Add(condition, rows);
                        }
                    }
                    #endregion

                    //遍历规则表
                    foreach (var kv in sql2Rows)
                    {
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = kv.Key;
                        List<DataRow> drArray = kv.Value;

                        double interval = 0;//重复标注距离(mm)
                        double.TryParse(drArray.First()["重复标注"].ToString().Trim().ToUpper(), out interval);
                        interval = interval * m_Application.MapControl.ReferenceScale * 1e-3;

                        List<int> oidList = new List<int>();
                        IFeatureCursor feCursor = roadLayer.Search(qf, true);
                        IFeature fe = null;
                        while ((fe = feCursor.NextFeature()) != null)
                        {
                            if (fe.Shape == null || fe.Shape.IsEmpty)
                                continue;

                            oidList.Add(fe.OID);
                        }
                        Marshal.ReleaseComObject(feCursor);

                        foreach (var oid in oidList)
                        {
                            fe = roadLayer.FeatureClass.GetFeature(oid);
                            if (fe == null)
                                continue;

                            wo.SetText(string.Format("【{1}】：正在生成道路要素【{0}】的注记......", fe.OID, m_caption));

                            IPolyline pl = fe.Shape as IPolyline;
                            if (interval == 0 || pl.Length < 2 * interval)//只一个注记
                            {
                                //在中点出进行标注
                                var centerPt = new PointClass();
                                pl.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerPt);

                                double rotateAngle = 0;
                                ILine tangentLine = new Line();
                                pl.QueryTangent(esriSegmentExtension.esriNoExtension, 0.5, true, 0.001, tangentLine);
                                rotateAngle = AnnotationHelper.RadianNormalization(tangentLine.Angle);

                                IPoint annoPoint = centerPt;

                                //创建注记
                                AnnotationManualCreateTool.CreateAnnotation(annoName2FeatureClass, roadLayer,
                                    fe, drArray, annoPoint, fontmappingTable, specialCharacterTable, rotateAngle, true, 0.8);
                            }
                            else
                            {
                                int num = (int)(pl.Length / interval);
                                double scale = (pl.Length - (num - 1) * interval) * 0.5 / pl.Length;//起始点比例
                                for (int i = 0; i < num; ++i)
                                {
                                    IPoint pt = new PointClass();
                                    pl.QueryPoint(esriSegmentExtension.esriNoExtension, scale, true, pt);

                                    double rotateAngle = 0;
                                    ILine tangentLine = new Line();
                                    pl.QueryTangent(esriSegmentExtension.esriNoExtension, scale, true, 0.001, tangentLine);
                                    rotateAngle = AnnotationHelper.RadianNormalization(tangentLine.Angle);

                                    IPoint annoPoint = pt;

                                    //创建注记
                                    AnnotationManualCreateTool.CreateAnnotation(annoName2FeatureClass, roadLayer,
                                        fe, drArray, annoPoint, fontmappingTable, specialCharacterTable, rotateAngle);

                                    //下一注记位置比例
                                    scale += interval / pl.Length;
                                }
                            }

                            Marshal.ReleaseComObject(fe);
                        }

                    }
                }

                m_Application.EngineEditor.StopOperation("道路注记创建");

                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                m_Application.EngineEditor.AbortOperation();
                return ex.Message;              
            }
            return result;
        }
        #endregion


    }
}

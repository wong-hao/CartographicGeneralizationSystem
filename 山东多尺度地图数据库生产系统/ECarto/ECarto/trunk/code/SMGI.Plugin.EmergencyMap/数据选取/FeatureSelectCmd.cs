using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
using ESRI.ArcGIS.Geometry;
using System.Data;
using System.IO;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 要素选取：主、邻区根据ATTACH字段区分
    /// 水系线（HYDL）：主区（根据选取等级字段选取）、附区（根据选取等级字段选取）
    /// 水系面（HYDA）：主区（根据面积大小选取？？？）、附区（根据面积大小选取？？？）；
    /// 道路线（LRDL）：主区（根据选取等级字段选取）、附区（根据选取等级字段选取）
    /// 铁路线（LRRL）：是否保留地铁、轻轨等要素
    /// 居民地面（RESA）：主区（根据居民地分类CLASS1字段选取）、附区（根据居民地分类CLASS1字段选取）
    /// 地名点（AGNP）：主区（根据地名点优先级PRIORITY字段选取）、附区（根据CLASS字段选取）
    /// 兴趣点（POI） ：主区（根据地名点优先级PRIORITY字段选取）、附区（根据地名点优先级PRIORITY字段选取）
    /// 植被（VEGA）：主区（面积选取）、附区（面积选取）
    /// 境界线（BOUL）：主区（根据GB字段选取）、附区（根据GB字段选取）
    /// 表面注记（ANNO）：主区（根据表面注记关联实体（BOUA2/4/5/6）的等级选取）、省内附区（根据表面注记关联实体（BOUA2/4/5/6）的等级选取）、省外附区（根据表面注记关联实体（BOUA2/4/5/6）的等级选取）
    /// </summary>
    public class FeatureSelectCmd : SMGICommand
    {
        public FeatureSelectCmd()
        {
            m_caption = "要素选取";
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
            var frm = new FeatureSelectForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            m_Application.EngineEditor.StartOperation();
            try
            {
                using (var wo = m_Application.SetBusy())
                {
                    if (frm.BRiverSelect)
                    {
                        wo.SetText("正在处理水系线...");
                        selectHYDLByGrade("HYDL", frm.MainHYDLGrade, frm.AdjacentHYDLGrade, frm.BaseMap);


                        wo.SetText("正在处理水系面...");
                        selectHYDAByArea("HYDA", frm.MainHYDAArea * 1.0e-6 * m_Application.ActiveView.FocusMap.ReferenceScale * m_Application.ActiveView.FocusMap.ReferenceScale,
                            frm.AdjacentHYDAArea * 1.0e-6 * m_Application.ActiveView.FocusMap.ReferenceScale * m_Application.ActiveView.FocusMap.ReferenceScale);
                    }

                    if (frm.BRoadSelect)
                    {
                        wo.SetText("正在处理道路...");
                        selectLRDLByGrade("LRDL", frm.MainLRDLGrade, frm.AdjacentLRDLGrade, frm.BaseMap);


                        selectLRRL("LRRL", "LFCP", frm.BReserveSubway);
                    }

                    if (frm.BRESASelect)
                    {
                        wo.SetText("正在处理居民地面...");
                        selectRESA("RESA", "LRDL", frm.MainSelectRESALevel, frm.AdjacentSelectRESALevel, frm.Level2SelectRESASQL, frm.BaseMap);
                    }

                    if (frm.BAGNPSelect)
                    {
                        wo.SetText("正在处理地名点...");
                        if (frm.EnableMainAGNPPriority)
                        {
                            if (frm.EnableAdjAGNPPriority)
                            {
                                selectAGNP("AGNP", frm.MainAGNPPriority, frm.AdjacentAGNPPriority);
                            }
                            else
                            {
                                selectAGNP("AGNP", frm.MainAGNPPriority, frm.AdjacentAGNPSQL2SelectState);
                            }
                        }
                        else
                        {
                            if (frm.EnableAdjAGNPPriority)
                            {
                                selectAGNP("AGNP", frm.MainAGNPSQL2SelectState, frm.AdjacentAGNPPriority);
                            }
                            else
                            {
                                selectAGNP("AGNP", frm.MainAGNPSQL2SelectState, frm.AdjacentAGNPSQL2SelectState);
                            }
                        }
                    }

                    if (frm.BPOISelect)
                    {
                        wo.SetText("正在处理POI...");
                        selectPOI("POI", frm.MainPOIPriority, frm.AdjPOIPriority);
                    }

                    if (frm.BVEGASelect)
                    {
                        wo.SetText("正在处理植被面...");
                        selectVEGAByArea("VEGA", frm.MainVEGAArea * 1.0e-6 * m_Application.ActiveView.FocusMap.ReferenceScale * m_Application.ActiveView.FocusMap.ReferenceScale,
                            frm.AdjacentVEGAArea * 1.0e-6 * m_Application.ActiveView.FocusMap.ReferenceScale * m_Application.ActiveView.FocusMap.ReferenceScale);
                    }

                    if (frm.BBOULSelect)
                    {
                        wo.SetText("正在处理境界线...");
                        selectBOULByGB("BOUL", frm.MainBOULGB, frm.AdjacentBOULGB);
                        selectJJTHByBOUL("JJTH", "BOUL");
                    }

                    if (frm.BBOUAAnnoSelect)
                    {
                        wo.SetText("正在处理表面注记...");
                        selectBOUAAnno("BOUA", frm.MainFCNameOfBOUAAnno, frm.InProvAdjacentFCNameOfBOUAAnno, frm.OutProvAdjacentFCNameOfBOUAAnno, frm.ProvPAC);
                    }

                }

                MessageBox.Show("已完成要素选取！");

                m_Application.EngineEditor.StopOperation("要素选取");

                m_Application.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                m_Application.EngineEditor.AbortOperation();
            }
        }

        #region  静态方法
        /// <summary>
        /// 通过选取等级筛选河流线要素
        /// </summary>
        /// <param name="mainGrade">若为负值，则主区要素不参与本次选取</param>
        /// <param name="adjacentGrade">若为负值，则邻区要素不参与本次选取</param>
        public static void selectHYDLByGrade(string fcName, int mainGrade, int adjacentGrade, string baseMap)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass hydlFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string gradeFN = GApplication.Application.TemplateManager.getFieldAliasName("grade2", hydlFC.AliasName);
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", hydlFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", hydlFC.AliasName);

            int gradeIndex = hydlFC.FindField(gradeFN);
            if (gradeIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", hydlFC.AliasName, gradeFN));
            }
            int selStateIndex = hydlFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", hydlFC.AliasName, selStateFN));
            }
            int attachIndex = hydlFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", hydlFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的水系线要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的水系线要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区水系线选取
            if (mainGrade >= 0)//mainGrade小于0，则认为不对主区水系进行选取
            {
                //1.从主区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, gradeFN, mainGrade);//主区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = hydlFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从主区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is null and {1} is null and {2} > {3} ", attachFN, selStateFN, gradeFN, mainGrade);
                fCursor = hydlFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 邻区水系线选取
            if (adjacentGrade >= 0)//adjacentGrade小于0，则认为不对邻区水系进行选取
            {
                //1.从邻区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is not null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, gradeFN, adjacentGrade);//邻区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = hydlFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从邻区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is not null and {1} is null and {2} > {3} ", attachFN, selStateFN, gradeFN, adjacentGrade);
                fCursor = hydlFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 关联注记的选取
            selectAnnoByConnFe(hydlFC, newUnSelectOIDList, newSelectOIDList);
            #endregion

            #region 相关附属设施要素选取
            selectAncillaryFacilities(hydlFC, newUnSelectOIDList, newSelectOIDList, baseMap);
            #endregion

        }

        /// <summary>
        /// 通过面积筛选河流面要素
        /// </summary>
        /// <param name="mainMinArea">若为负值，则主区要素不参与本次选取</param>
        /// <param name="adjacentMinArea">若为负值，则邻区要素不参与本次选取</param>
        public static void selectHYDAByArea(string fcName, double mainMinArea, double adjacentMinArea)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass hydaFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", hydaFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", hydaFC.AliasName);

            int selStateIndex = hydaFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", hydaFC.AliasName, selStateFN));
            }
            int attachIndex = hydaFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", hydaFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的水系面要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的水系面要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区水系面选取
            if (mainMinArea >= 0)//mainMinArea小于0，则认为不对主区水系面进行选取
            {
                //1.从主区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is null and {1} is not null and {2} >= {3} ", attachFN, selStateFN, hydaFC.AreaField.Name, mainMinArea);//主区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = hydaFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
                
                //2.从主区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is null and {1} is null and {2} < {3} ", attachFN, selStateFN, hydaFC.AreaField.Name, mainMinArea);
                fCursor = hydaFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 邻区水系面选取
            if (adjacentMinArea >= 0)//adjacentMinArea小于0，则认为不对邻区水系面进行选取
            {
                //1.从邻区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is not null and {1} is not null and {2} >= {3} ", attachFN, selStateFN, hydaFC.AreaField.Name, adjacentMinArea);//邻区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = hydaFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从邻区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is not null and {1} is null and {2} < {3} ", attachFN, selStateFN, hydaFC.AreaField.Name, adjacentMinArea);
                fCursor = hydaFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 关联注记的选取
            selectAnnoByConnFe(hydaFC, newUnSelectOIDList, newSelectOIDList);
            #endregion
        }


        /// <summary>
        /// 通过选取等级筛选道路线要素
        /// </summary>
        /// <param name="mainGrade">若为负值，则主区要素不参与本次选取</param>
        /// <param name="adjacentGrade">若为负值，则邻区要素不参与本次选取</param>
        public static void selectLRDLByGrade(string fcName, int mainGrade, int adjacentGrade, string baseMap)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass lrdlFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string gradeFN = GApplication.Application.TemplateManager.getFieldAliasName("grade2", lrdlFC.AliasName);
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", lrdlFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", lrdlFC.AliasName);

            int gradeIndex = lrdlFC.FindField(gradeFN);
            if (gradeIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lrdlFC.AliasName, gradeFN));
            }
            int selStateIndex = lrdlFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lrdlFC.AliasName, selStateFN));
            }
            int attachIndex = lrdlFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lrdlFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的道路线要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的道路线要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区道路线选取
            if (mainGrade >= 0)//mainGrade小于0，则认为不对主区道路线进行选取
            {
                //1.从主区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, gradeFN, mainGrade);//主区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = lrdlFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从主区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is null and {1} is null and {2} > {3} ", attachFN, selStateFN, gradeFN, mainGrade);
                fCursor = lrdlFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 邻区道路线选取
            if (adjacentGrade >= 0)//adjacentGrade小于0，则认为不对邻区道路线进行选取
            {
                //1.从邻区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is not null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, gradeFN, adjacentGrade);//邻区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = lrdlFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从邻区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is not null and {1} is null and {2} > {3} ", attachFN, selStateFN, gradeFN, adjacentGrade);
                fCursor = lrdlFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 关联注记的选取
            selectAnnoByConnFe(lrdlFC, newUnSelectOIDList, newSelectOIDList);
            #endregion

            #region 相关附属设施要素选取
            selectAncillaryFacilities(lrdlFC, newUnSelectOIDList, newSelectOIDList, baseMap);
            #endregion
        }

        /// <summary>
        /// 选取铁路线要素，是否保留地铁、轻轨等要素
        /// </summary>
        /// <param name="bSubway"></param>
        public static void selectLRRL(string lrrlFCName, string lfcpFCName, bool bSubway)
        {
            #region 地铁、轻轨要素选取
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == lrrlFCName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", lrrlFCName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", lrrlFCName));
                return;
            }
            IFeatureClass lrrlFC = lyr.FeatureClass;


            string gbFN = GApplication.Application.TemplateManager.getFieldAliasName("GB", lrrlFC.AliasName);
            int gbIndex = lrrlFC.FindField(gbFN);
            if (gbIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lrrlFC.AliasName, gbFN));
            }
            string filter = string.Format("{0} in (430101, 430102) ", gbFN);//筛选数据中的地铁、轻轨要素

            //选取要素
            selectFeature(lrrlFC, filter, bSubway);
            #endregion

            #region 相关附属设施要素（地铁站、轻轨站）选取
            lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == lfcpFCName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", lfcpFCName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", lfcpFCName));
                return;
            }
            IFeatureClass lfcpFC = lyr.FeatureClass;

            gbFN = GApplication.Application.TemplateManager.getFieldAliasName("GB", lfcpFC.AliasName);
            gbIndex = lfcpFC.FindField(gbFN);
            if (gbIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lfcpFC.AliasName, gbFN));
            }
            filter = string.Format("{0} in (450101, 450102) ", gbFN);//筛选数据中的地铁站、轻轨站要素

            //选取要素
            selectFeature(lfcpFC, filter, bSubway);
            #endregion
        }

        /// <summary>
        /// 通过街区显示级别选取居民地面要素
        /// </summary>
        /// <param name="mainLevel"></param>
        /// <param name="adjacentLevel"></param>
        /// <param name="level2SelectRESASQL"></param>
        public static void selectRESA(string resaFCName, string lrdlFCName, int mainLevel, int adjacentLevel, Dictionary<int, string> level2SelectRESASQL, string baseMap)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == resaFCName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", resaFCName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", resaFCName));
                return;
            }
            IFeatureClass resaFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", resaFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", resaFC.AliasName);

            int selStateIndex = resaFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", resaFC.AliasName, selStateFN));
            }
            int attachIndex = resaFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", resaFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的居民地面要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的居民地面要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区居民地面选取
            if (level2SelectRESASQL.ContainsKey(mainLevel))
            {
                List<int> mainSelectOIDList = new List<int>();//主区中满足本次条件街区面要素OID集合
                qf.WhereClause = string.Format("{0} is null", attachFN);
                if (level2SelectRESASQL[mainLevel] != "")
                    qf.WhereClause += string.Format(" and ({0}) ", level2SelectRESASQL[mainLevel]);
                 IFeatureCursor fCursor = resaFC.Search(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    mainSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //1.循环遍历主区中的所有显示的街区面，若不满足本次条件，则隐藏
                qf.WhereClause = string.Format("{0} is null and {1} is null ", attachFN, selStateFN);
                fCursor = resaFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    if (!mainSelectOIDList.Contains(f.OID))//不满足本溪条件
                    {
                        f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                        fCursor.UpdateFeature(f);

                        //收集本次未选取状态要素的OID
                        newUnSelectOIDList.Add(f.OID);
                    }
                }
                Marshal.ReleaseComObject(fCursor);

                //2.循环遍历主区中的所有不显示的街区面，若满足本次条件，则恢复显示状态
                qf.WhereClause = string.Format("{0} is null and {1} is not null ", attachFN, selStateFN);
                fCursor = resaFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    if (mainSelectOIDList.Contains(f.OID))//满足本溪条件
                    {
                        f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                        fCursor.UpdateFeature(f);

                        //收集本次恢复选取状态要素的OID
                        newSelectOIDList.Add(f.OID);
                    }
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 邻区居民地面选取
            if (level2SelectRESASQL.ContainsKey(adjacentLevel))
            {
                List<int> adjacentSelectOIDList = new List<int>();//邻区中满足本次条件街区面要素OID集合
                qf.WhereClause = string.Format("{0} is not null", attachFN);
                if (level2SelectRESASQL[adjacentLevel] != "")
                    qf.WhereClause += string.Format(" and ({0}) ", level2SelectRESASQL[adjacentLevel]);
                IFeatureCursor fCursor = resaFC.Search(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    adjacentSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //1.循环遍历邻区中的所有显示的街区面，若不满足本次条件，则隐藏
                qf.WhereClause = string.Format("{0} is not null and {1} is null ", attachFN, selStateFN);
                fCursor = resaFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    if (!adjacentSelectOIDList.Contains(f.OID))//不满足本溪条件
                    {
                        f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                        fCursor.UpdateFeature(f);

                        //收集本次未选取状态要素的OID
                        newUnSelectOIDList.Add(f.OID);
                    }
                }
                Marshal.ReleaseComObject(fCursor);

                //2.循环遍历邻区中的所有不显示的街区面，若满足本次条件，则恢复显示状态
                qf.WhereClause = string.Format("{0} is not null and {1} is not null ", attachFN, selStateFN);
                fCursor = resaFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    if (adjacentSelectOIDList.Contains(f.OID))//满足本溪条件
                    {
                        f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                        fCursor.UpdateFeature(f);

                        //收集本次恢复选取状态要素的OID
                        newSelectOIDList.Add(f.OID);
                    }
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 更新城市道路的显示符号
            updateCityRoadSymbol(lrdlFCName, mainLevel, adjacentLevel, baseMap);
            #endregion
        }

        /// <summary>
        /// 通过地名点优先级及地名点分类码筛选地名点要素
        /// </summary>
        /// <param name="mianPriority">若为负值，则主区要素不参与本次选取</param>
        /// <param name="adjacentAGNPSQL2SelectState">邻区需显示的地名点SQL语句与其对应的选择状态</param>
        public static void selectAGNP(string fcName, int mianPriority, Dictionary<string, bool> adjacentAGNPSQL2SelectState)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass agnpFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string priorityFN = GApplication.Application.TemplateManager.getFieldAliasName("priority", agnpFC.AliasName);
            string classFN = GApplication.Application.TemplateManager.getFieldAliasName("class", agnpFC.AliasName);
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", agnpFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", agnpFC.AliasName);

            int priorityIndex = agnpFC.FindField(priorityFN);
            if (priorityIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, priorityFN));
            }
            int classIndex = agnpFC.FindField(classFN);
            if (classIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, classFN));
            }
            int selStateIndex = agnpFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, selStateFN));
            }
            int attachIndex = agnpFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的水系线要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的水系线要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区的地名点进行选取:通过地名点优先级字段
            if (mianPriority >= 0)//priority小于0，则认为不对主区地名点进行选取
            {
                //1.从主区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, priorityFN, mianPriority);//主区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = agnpFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从主区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is null and {1} is null and {2} > {3} ", attachFN, selStateFN, priorityFN, mianPriority);
                fCursor = agnpFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 邻区的地名点进行选取:通过地名点分类码
            foreach (var kv in adjacentAGNPSQL2SelectState)
            {
                qf.WhereClause = string.Format("{0} is not null and ({1}) ", attachFN, kv.Key);//邻区中,满足本次选取条件
                IFeatureCursor fCursor = agnpFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    object selStateVal = f.get_Value(selStateIndex);
                    if (kv.Value)//需显示的AGNP
                    {
                        if (!Convert.IsDBNull(selStateVal))//不显示
                        {
                            f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                            fCursor.UpdateFeature(f);

                            //收集本次恢复选取状态要素的OID
                            newSelectOIDList.Add(f.OID);
                        }
                    }
                    else//不显示的AGNP
                    {
                        if (Convert.IsDBNull(selStateVal))//已显示
                        {
                            f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                            fCursor.UpdateFeature(f);

                            //收集本次未选取状态要素的OID
                            newUnSelectOIDList.Add(f.OID);
                        }
                    }
                    
                }
                Marshal.ReleaseComObject(fCursor);
                
            }
            #endregion

            #region 关联注记的选取
            selectAnnoByConnFe(agnpFC, newUnSelectOIDList, newSelectOIDList);
            #endregion
        }
        /// <summary>
        /// 通过地名点优先级筛选地名点要素
        /// </summary>
        /// <param name="mianPriority">若为负值，则主区要素不参与本次选取</param>
        /// <param name="adjacentPriority">若为负值，则邻区要素不参与本次选取</param>
        public static void selectAGNP(string fcName, int mianPriority, int adjacentPriority)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass agnpFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string priorityFN = GApplication.Application.TemplateManager.getFieldAliasName("priority", agnpFC.AliasName);
            string classFN = GApplication.Application.TemplateManager.getFieldAliasName("class", agnpFC.AliasName);
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", agnpFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", agnpFC.AliasName);

            int priorityIndex = agnpFC.FindField(priorityFN);
            if (priorityIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, priorityFN));
            }
            int classIndex = agnpFC.FindField(classFN);
            if (classIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, classFN));
            }
            int selStateIndex = agnpFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, selStateFN));
            }
            int attachIndex = agnpFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的水系线要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的水系线要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区的地名点进行选取:通过地名点优先级字段
            if (mianPriority >= 0)//priority小于0，则认为不对主区地名点进行选取
            {
                //1.从主区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, priorityFN, mianPriority);//主区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = agnpFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从主区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is null and {1} is null and {2} > {3} ", attachFN, selStateFN, priorityFN, mianPriority);
                fCursor = agnpFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 邻区的地名点进行选取:通过地名点优先级字段
            if (adjacentPriority >= 0)//priority小于0，则认为不对邻区地名点进行选取
            {
                //1.从邻区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is not null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, priorityFN, adjacentPriority);//邻区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = agnpFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从邻区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is not null and {1} is null and {2} > {3} ", attachFN, selStateFN, priorityFN, adjacentPriority);
                fCursor = agnpFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 关联注记的选取
            selectAnnoByConnFe(agnpFC, newUnSelectOIDList, newSelectOIDList);
            #endregion
        }

        /// <summary>
        /// 通过地名点分类码筛选地名点要素
        /// </summary>
        /// <param name="mainAGNPSQL2SelectState">主区需显示的地名点SQL语句与其对应的选择状态</param>
        /// <param name="adjacentAGNPSQL2SelectState">邻区需显示的地名点SQL语句与其对应的选择状态</param>
        public static void selectAGNP(string fcName, Dictionary<string, bool> mainAGNPSQL2SelectState, Dictionary<string, bool> adjacentAGNPSQL2SelectState)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass agnpFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string priorityFN = GApplication.Application.TemplateManager.getFieldAliasName("priority", agnpFC.AliasName);
            string classFN = GApplication.Application.TemplateManager.getFieldAliasName("class", agnpFC.AliasName);
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", agnpFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", agnpFC.AliasName);

            int priorityIndex = agnpFC.FindField(priorityFN);
            if (priorityIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, priorityFN));
            }
            int classIndex = agnpFC.FindField(classFN);
            if (classIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, classFN));
            }
            int selStateIndex = agnpFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, selStateFN));
            }
            int attachIndex = agnpFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的水系线要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的水系线要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区的地名点进行选取:通过地名点分类码
            foreach (var kv in mainAGNPSQL2SelectState)
            {
                qf.WhereClause = string.Format("{0} is null and ({1}) ", attachFN, kv.Key);//主区中,满足本次选取条件
                IFeatureCursor fCursor = agnpFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    object selStateVal = f.get_Value(selStateIndex);
                    if (kv.Value)//需显示的AGNP
                    {
                        if (!Convert.IsDBNull(selStateVal))//不显示
                        {
                            f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                            fCursor.UpdateFeature(f);

                            //收集本次恢复选取状态要素的OID
                            newSelectOIDList.Add(f.OID);
                        }
                    }
                    else//不显示的AGNP
                    {
                        if (Convert.IsDBNull(selStateVal))//已显示
                        {
                            f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                            fCursor.UpdateFeature(f);

                            //收集本次未选取状态要素的OID
                            newUnSelectOIDList.Add(f.OID);
                        }
                    }

                }
                Marshal.ReleaseComObject(fCursor);

            }
            
            #endregion

            #region 邻区的地名点进行选取:通过地名点分类码
            foreach (var kv in adjacentAGNPSQL2SelectState)
            {
                qf.WhereClause = string.Format("{0} is not null and ({1}) ", attachFN, kv.Key);//邻区中,满足本次选取条件
                IFeatureCursor fCursor = agnpFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    object selStateVal = f.get_Value(selStateIndex);
                    if (kv.Value)//需显示的AGNP
                    {
                        if (!Convert.IsDBNull(selStateVal))//不显示
                        {
                            f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                            fCursor.UpdateFeature(f);

                            //收集本次恢复选取状态要素的OID
                            newSelectOIDList.Add(f.OID);
                        }
                    }
                    else//不显示的AGNP
                    {
                        if (Convert.IsDBNull(selStateVal))//已显示
                        {
                            f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                            fCursor.UpdateFeature(f);

                            //收集本次未选取状态要素的OID
                            newUnSelectOIDList.Add(f.OID);
                        }
                    }

                }
                Marshal.ReleaseComObject(fCursor);

            }
            #endregion

            #region 关联注记的选取
            selectAnnoByConnFe(agnpFC, newUnSelectOIDList, newSelectOIDList);
            #endregion
        }

        /// <summary>
        /// 通过地名点优先级及地名点分类码筛选地名点要素
        /// </summary>
        /// <param name="mainAGNPSQL2SelectState">主区需显示的地名点SQL语句与其对应的选择状态</param>
        /// <param name="adjacentPriority">若为负值，则邻区要素不参与本次选取</param>
        public static void selectAGNP(string fcName, Dictionary<string, bool> mainAGNPSQL2SelectState, int adjacentPriority)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass agnpFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string priorityFN = GApplication.Application.TemplateManager.getFieldAliasName("priority", agnpFC.AliasName);
            string classFN = GApplication.Application.TemplateManager.getFieldAliasName("class", agnpFC.AliasName);
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", agnpFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", agnpFC.AliasName);

            int priorityIndex = agnpFC.FindField(priorityFN);
            if (priorityIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, priorityFN));
            }
            int classIndex = agnpFC.FindField(classFN);
            if (classIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, classFN));
            }
            int selStateIndex = agnpFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, selStateFN));
            }
            int attachIndex = agnpFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", agnpFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的水系线要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的水系线要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区的地名点进行选取:通过地名点分类码
            foreach (var kv in mainAGNPSQL2SelectState)
            {
                qf.WhereClause = string.Format("{0} is null and ({1}) ", attachFN, kv.Key);//主区中,满足本次选取条件
                IFeatureCursor fCursor = agnpFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    object selStateVal = f.get_Value(selStateIndex);
                    if (kv.Value)//需显示的AGNP
                    {
                        if (!Convert.IsDBNull(selStateVal))//不显示
                        {
                            f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                            fCursor.UpdateFeature(f);

                            //收集本次恢复选取状态要素的OID
                            newSelectOIDList.Add(f.OID);
                        }
                    }
                    else//不显示的AGNP
                    {
                        if (Convert.IsDBNull(selStateVal))//已显示
                        {
                            f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                            fCursor.UpdateFeature(f);

                            //收集本次未选取状态要素的OID
                            newUnSelectOIDList.Add(f.OID);
                        }
                    }

                }
                Marshal.ReleaseComObject(fCursor);

            }

            #endregion

            #region 邻区的地名点进行选取:通过地名点优先级字段
            if (adjacentPriority >= 0)//priority小于0，则认为不对邻区地名点进行选取
            {
                //1.从邻区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is not null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, priorityFN, adjacentPriority);//邻区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = agnpFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从邻区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is not null and {1} is null and {2} > {3} ", attachFN, selStateFN, priorityFN, adjacentPriority);
                fCursor = agnpFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 关联注记的选取
            selectAnnoByConnFe(agnpFC, newUnSelectOIDList, newSelectOIDList);
            #endregion
        }

        /// <summary>
        /// 通过POI优先级选取
        /// </summary>
        /// <param name="mainPriority">若为负值，则主区要素不参与本次选取</param>
        /// <param name="adjPriority">若为负值，则邻区要素不参与本次选取</param>
        public static void selectPOI(string fcName, int mainPriority, int adjPriority)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass poiFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string priorityFN = GApplication.Application.TemplateManager.getFieldAliasName("priority", poiFC.AliasName);
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", poiFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", poiFC.AliasName);

            int priorityIndex = poiFC.FindField(priorityFN);
            if (priorityIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", poiFC.AliasName, priorityFN));
            }
            int selStateIndex = poiFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", poiFC.AliasName, selStateFN));
            }
            int attachIndex = poiFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", poiFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的水系线要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的水系线要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区的POI进行选取:通过优先级字段
            if (mainPriority >= 0)//priority小于0，则认为不对主区POI进行选取
            {
                //1.从主区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, priorityFN, mainPriority);//主区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = poiFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从主区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is null and {1} is null and {2} > {3} ", attachFN, selStateFN, priorityFN, mainPriority);
                fCursor = poiFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 邻区的POI进行选取:通过优先级字段
            if (adjPriority >= 0)
            {
                //1.从邻区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is not null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, priorityFN, adjPriority);//邻区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = poiFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从邻区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is not null and {1} is null and {2} > {3} ", attachFN, selStateFN, priorityFN, adjPriority);
                fCursor = poiFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 关联注记的选取
            selectAnnoByConnFe(poiFC, newUnSelectOIDList, newSelectOIDList);
            #endregion
        }

        /// <summary>
        /// 通过面积筛选植被面要素
        /// </summary>
        /// <param name="mainMinArea">若为负值，则主区要素不参与本次选取</param>
        /// <param name="adjacentMinArea">若为负值，则邻区要素不参与本次选取</param>
        public static void selectVEGAByArea(string fcName, double mainMinArea, double adjacentMinArea)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass vegaFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", vegaFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", vegaFC.AliasName);

            int selStateIndex = vegaFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", vegaFC.AliasName, selStateFN));
            }
            int attachIndex = vegaFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", vegaFC.AliasName, attachFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的水系面要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的水系面要素OID集合

            IQueryFilter qf = new QueryFilterClass();

            #region 主区水系面选取
            if (mainMinArea >= 0)//mainMinArea小于0，则认为不对主区水系面进行选取
            {
                //1.从主区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is null and {1} is not null and {2} >= {3} ", attachFN, selStateFN, vegaFC.AreaField.Name, mainMinArea);//主区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = vegaFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从主区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is null and {1} is null and {2} < {3} ", attachFN, selStateFN, vegaFC.AreaField.Name, mainMinArea);
                fCursor = vegaFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 邻区水系面选取
            if (adjacentMinArea >= 0)//adjacentMinArea小于0，则认为不对邻区水系面进行选取
            {
                //1.从邻区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is not null and {1} is not null and {2} >= {3} ", attachFN, selStateFN, vegaFC.AreaField.Name, adjacentMinArea);//邻区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = vegaFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从邻区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is not null and {1} is null and {2} < {3} ", attachFN, selStateFN, vegaFC.AreaField.Name, adjacentMinArea);
                fCursor = vegaFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 关联注记的选取
            selectAnnoByConnFe(vegaFC, newUnSelectOIDList, newSelectOIDList);
            #endregion
        }

        /// <summary>
        /// 通过GB选取需显示的境界线要素
        /// </summary>
        /// <param name="mainGB">若为负值，则不参与本次选取</param>
        /// <param name="adjacentGB">若为负值，则不参与本次选取</param>
        public static void selectBOULByGB(string fcName, int mainGB, int adjacentGB)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass boulFC = lyr.FeatureClass;
            
            #region 相关字段判断与获取
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", boulFC.AliasName);
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", boulFC.AliasName);
            string gbFN = GApplication.Application.TemplateManager.getFieldAliasName("GB", boulFC.AliasName);

            int selStateIndex = boulFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", boulFC.AliasName, selStateFN));
            }
            int attachIndex = boulFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", boulFC.AliasName, attachFN));
            }
            int gbIndex = boulFC.FindField(gbFN);
            if (gbIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", boulFC.AliasName, gbFN));
            }
            #endregion

            IQueryFilter qf = new QueryFilterClass();

            #region 主区境界线选取
            if (mainGB > 0)
            {
                //1.从主区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, gbFN, mainGB);//主区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = boulFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从主区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is null and {1} is null and {2} > {3} ", attachFN, selStateFN, gbFN, mainGB);
                fCursor = boulFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 邻区境界线选取
            if (adjacentGB > 0)
            {
                //1.从邻区未选取要素中，恢复满足本次条件的未选取要素
                qf.WhereClause = string.Format("{0} is not null and {1} is not null and {2} <= {3} ", attachFN, selStateFN, gbFN, adjacentGB);//邻区中的未选取要素,且满足本次选取条件
                IFeatureCursor fCursor = boulFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);
                }
                Marshal.ReleaseComObject(fCursor);

                //2.从邻区选取状态要素中，将不满足条件的要素设置为未选取状态
                qf.WhereClause = string.Format("{0} is not null and {1} is null and {2} > {3} ", attachFN, selStateFN, gbFN, adjacentGB);
                fCursor = boulFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

        }

        /// <summary>
        /// 通过JJTH要素关联的BOUL要素的选取状态，对JJTH要素进行选取
        /// </summary>
        public static void selectJJTHByBOUL(string jjthFCName, string boulFCName)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == jjthFCName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", jjthFCName));
                return;
            }
            IFeatureClass jjthFC = lyr.FeatureClass;

            lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == boulFCName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！", boulFCName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", boulFCName));
                return;
            }
            IFeatureClass boulFC = lyr.FeatureClass;

            #region 相关字段判断与获取
            string jjthSelStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", jjthFC.AliasName);
            int jjthSelStateIndex = jjthFC.FindField(jjthSelStateFN);
            if (jjthSelStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", jjthFC.AliasName, jjthSelStateFN));
            }

            string boulSelStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", boulFC.AliasName);
            int boulSelStateIndex = boulFC.FindField(boulSelStateFN);
            if (boulSelStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", boulFC.AliasName, boulSelStateFN));
            }
            #endregion

            IFeatureCursor fCursor = jjthFC.Update(null, true);
            IFeature f = null;
            while ((f = fCursor.NextFeature()) != null)
            {
                IPolyline pl = f.Shape as IPolyline;
                if (pl == null)
                    continue;

                IRelationalOperator2 ro = pl as IRelationalOperator2;

                object selStateVal = "要素选取";

                #region 查找该JJTH要素相关联的BOUL要素,并返回选取状态(若与多个要素关联，若其中至少有一个BOUL要素为选取状态，则选取该JJTH要素，否则，反之)
                SpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = pl;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                int cout = boulFC.FeatureCount(sf);
                IFeatureCursor boulCursor = boulFC.Search(sf, true);
                IFeature boulFe = null;
                while ((boulFe = boulCursor.NextFeature()) != null)
                {
                    if (ro.Touches(boulFe.Shape) || ro.Crosses(boulFe.Shape))
                        continue;//排除相邻、或相交的情况

                    var objVal = boulFe.get_Value(boulSelStateIndex);
                    if (Convert.IsDBNull(objVal))
                    {
                        selStateVal = DBNull.Value;
                        break;
                    }

                }
                Marshal.ReleaseComObject(boulCursor);
                #endregion

                //设置该JJTH要素的选取状态
                f.set_Value(jjthSelStateIndex, selStateVal);
                fCursor.UpdateFeature(f);
            }
            Marshal.ReleaseComObject(fCursor);

        }

        /// <summary>
        /// 表面注记选取
        /// </summary>
        /// <param name="mainBOUAFCName"></param>
        /// <param name="inProvAdjacentBOUAFCName"></param>
        /// <param name="outProvAdjacentBOUAFCName"></param>
        /// <param name="provPac"></param>
        public static void selectBOUAAnno(string fcName, string mainBOUAFCName, string inProvAdjacentBOUAFCName, string outProvAdjacentBOUAFCName, string provPac)
        {
            Dictionary<string, int> fcName2ObjectClassID = new Dictionary<string, int>();
            Dictionary<int, IFeatureClass> objectClassID2FC = new Dictionary<int, IFeatureClass>();
            #region 获取BOUA2/4/5/6要素类的ID
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper().StartsWith(fcName));
            }));
            foreach (var layer in lyrs)
            {
                IFeatureClass fc = (layer as IFeatureLayer).FeatureClass;
                if (fc == null)
                    continue;

                if (!fcName2ObjectClassID.ContainsKey(fc.AliasName))
                {
                    fcName2ObjectClassID.Add(fc.AliasName, fc.ObjectClassID);
                    objectClassID2FC.Add(fc.ObjectClassID, fc);
                }
            }
            #endregion

            var layers = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l is IFDOGraphicsLayer);
            })).ToArray();
            foreach (var lyr in layers)
            {
                IFeatureClass annoFC = (lyr as IFeatureLayer).FeatureClass;
                if (annoFC == null || (annoFC as IDataset).Workspace.PathName != GApplication.Application.Workspace.EsriWorkspace.PathName)
                    continue;


                #region 相关字段判断与获取
                string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", annoFC.AliasName);
                int selStateIndex = annoFC.FindField(selStateFN);
                if (selStateIndex == -1)
                    continue;
                string annoClassIDFN = GApplication.Application.TemplateManager.getFieldAliasName("AnnotationClassID", annoFC.AliasName);
                int annoClassIDIndex = annoFC.FindField(annoClassIDFN);
                if (annoClassIDIndex == -1)
                {
                    throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", annoFC.AliasName, annoClassIDFN));
                }
                #endregion

                //表面注记选取
                if (fcName2ObjectClassID.Count > 0)
                {
                    string classIDSet = "";
                    foreach (var item in fcName2ObjectClassID.Values)
                    {
                        if (classIDSet != "")
                            classIDSet += string.Format(",{0}", item);
                        else
                            classIDSet = string.Format("{0}", item);
                    }

                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = string.Format("{0} in ({1})", annoClassIDFN, classIDSet);
                    IFeatureCursor fCursor = annoFC.Update(qf, true);
                    IFeature f = null;
                    while ((f = fCursor.NextFeature()) != null)
                    {
                        IAnnotationFeature2 annoFe = f as IAnnotationFeature2;

                        IFeatureClass bouaFC = objectClassID2FC[annoFe.AnnotationClassID];
                        IFeature bouaFe = bouaFC.GetFeature(annoFe.LinkedFeatureID);
                        if (bouaFe == null)//关联的实体要素丢失，该表面注记不选取
                        {
                            f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                            fCursor.UpdateFeature(f);
                        }
                        else
                        {
                            #region 判断该关联要素是主区/省内邻区/省外邻区，从而设置该表面注记要素的选取状态
                            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", bouaFC.AliasName);
                            string pacFN = GApplication.Application.TemplateManager.getFieldAliasName("pac", bouaFC.AliasName);
                            int attachIndex = bouaFC.FindField(attachFN);
                            if (attachIndex == -1)
                            {
                                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", bouaFC.AliasName, attachFN));
                            }
                            int pacIndex = bouaFC.FindField(pacFN);
                            if (pacIndex == -1)
                            {
                                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", bouaFC.AliasName, pacFN));
                            }

                            object attachVal = bouaFe.get_Value(attachIndex);
                            string pacVal = bouaFe.get_Value(pacIndex).ToString();

                            if (Convert.IsDBNull(attachVal))//主区
                            {
                                if (bouaFC.AliasName.ToUpper().Trim() == mainBOUAFCName.ToUpper().Trim())
                                {
                                    f.set_Value(selStateIndex, DBNull.Value);//设置为选取状态
                                    fCursor.UpdateFeature(f);
                                }
                                else
                                {
                                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                                    fCursor.UpdateFeature(f);
                                }
                            }
                            else
                            {
                                if (pacVal.StartsWith(provPac))//省内邻区
                                {
                                    if (bouaFC.AliasName.ToUpper().Trim() == inProvAdjacentBOUAFCName.ToUpper().Trim())
                                    {
                                        f.set_Value(selStateIndex, DBNull.Value);//设置为选取状态
                                        fCursor.UpdateFeature(f);
                                    }
                                    else
                                    {
                                        f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                                        fCursor.UpdateFeature(f);
                                    }
                                }
                                else//省外邻区
                                {
                                    if (bouaFC.AliasName.ToUpper().Trim() == outProvAdjacentBOUAFCName.ToUpper().Trim())
                                    {
                                        f.set_Value(selStateIndex, DBNull.Value);//设置为选取状态
                                        fCursor.UpdateFeature(f);
                                    }
                                    else
                                    {
                                        f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                                        fCursor.UpdateFeature(f);
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    Marshal.ReleaseComObject(fCursor);
                }
            }
            
        }

        /// <summary>
        /// 设置要素相关联注记的选取状态
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="newUnSelectOIDList"></param>
        /// <param name="newSelectOIDList"></param>
        public static void selectAnnoByConnFe(IFeatureClass fc, List<int> newUnSelectOIDList, List<int> newSelectOIDList)
        {
            var layers = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l is IFDOGraphicsLayer);
            })).ToArray();
            foreach (var lyr in layers)
            {
                IFeatureClass annoFC = (lyr as IFeatureLayer).FeatureClass;
                if (annoFC == null || (annoFC as IDataset).Workspace.PathName != GApplication.Application.Workspace.EsriWorkspace.PathName)
                    continue;

                string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", annoFC.AliasName);
                int selStateIndex = annoFC.FindField(selStateFN);
                if (selStateIndex == -1)
                    continue;
                string annoClassIDFN = GApplication.Application.TemplateManager.getFieldAliasName("AnnotationClassID", annoFC.AliasName);
                int annoClassIDIndex = annoFC.FindField(annoClassIDFN);
                if (annoClassIDIndex == -1)
                {
                    throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", annoFC.AliasName, annoClassIDFN));
                }

                IQueryFilter qf = new QueryFilterClass();

                #region 设置newUnSelectOIDList关联的注记为未选取状态
                qf.WhereClause = string.Format("{0} = {1} and {2} is null", annoClassIDFN, fc.ObjectClassID, selStateFN);
                IFeatureCursor fCursor = annoFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    IAnnotationFeature2 annoFe = f as IAnnotationFeature2;
                    if (newUnSelectOIDList.Contains(annoFe.LinkedFeatureID))
                    {
                        f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                        fCursor.UpdateFeature(f);
                    }
                }
                Marshal.ReleaseComObject(fCursor);
                #endregion

                #region 恢复newSelectOIDList关联的注记为选取状态
                qf.WhereClause = string.Format("{0} = {1} and {2} is not null", annoClassIDFN, fc.ObjectClassID, selStateFN);
                fCursor = annoFC.Update(qf, true);
                while ((f = fCursor.NextFeature()) != null)
                {
                    IAnnotationFeature2 annoFe = f as IAnnotationFeature2;
                    if (newSelectOIDList.Contains(annoFe.LinkedFeatureID))
                    {
                        f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                        fCursor.UpdateFeature(f);
                    }
                }
                Marshal.ReleaseComObject(fCursor);
                #endregion
            }
        }

        /// <summary>
        /// 设置要素相关联注记的选取状态
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="oid"></param>
        /// <param name="bSelect"></param>
        public static void selectAnnoByConnFe(IFeatureClass fc, int oid, bool bSelect)
        {
            var layers = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l is IFDOGraphicsLayer);
            })).ToArray();
            foreach (var lyr in layers)
            {
                IFeatureClass annoFC = (lyr as IFeatureLayer).FeatureClass;
                if (annoFC == null || (annoFC as IDataset).Workspace.PathName != GApplication.Application.Workspace.EsriWorkspace.PathName)
                    continue;

                string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", annoFC.AliasName);
                int selStateIndex = annoFC.FindField(selStateFN);
                if (selStateIndex == -1)
                    continue;
                string annoClassIDFN = GApplication.Application.TemplateManager.getFieldAliasName("AnnotationClassID", annoFC.AliasName);
                int annoClassIDIndex = annoFC.FindField(annoClassIDFN);
                if (annoClassIDIndex == -1)
                {
                    throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", annoFC.AliasName, annoClassIDFN));
                }
                string linkedFeatureIDFN = GApplication.Application.TemplateManager.getFieldAliasName("FeatureID", annoFC.AliasName);
                int linkedFeatureIDIndex = annoFC.FindField(linkedFeatureIDFN);
                if (linkedFeatureIDIndex == -1)
                {
                    throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", annoFC.AliasName, linkedFeatureIDFN));
                }

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("{0} = {1} and {2} = {3}", annoClassIDFN, fc.ObjectClassID, linkedFeatureIDFN, oid);
                IFeatureCursor fCursor = annoFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    IAnnotationFeature2 annoFe = f as IAnnotationFeature2;
                    if (bSelect)
                    {
                        //设置关联的注记为未选取状态
                        f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                        fCursor.UpdateFeature(f);
                    }
                    else
                    {
                        //设置关联的注记为未选取状态
                        f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                        fCursor.UpdateFeature(f);

                    }
                }
                Marshal.ReleaseComObject(fCursor);
            }
            
        }

        /// <summary>
        /// 设置要素相关联的附属设施要素的选取状态
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="newUnSelectOIDList"></param>
        /// <param name="newSelectOIDList"></param>
        /// <param name="tolerance"></param>
        public static void selectAncillaryFacilities(IFeatureClass fc, List<int> newUnSelectOIDList, List<int> newSelectOIDList, string baseMap, double tolerance = 0.1)
        {
            string fileName = string.Format("{0}\\专家库\\要素选取\\{1}\\AncillaryFacilitiesRule.xml", GApplication.Application.Template.Root, baseMap);
            if (!File.Exists(fileName))
            {
                //throw new Exception(string.Format("没有找到选取配置文件：{0}", fileName));
                System.Diagnostics.Trace.WriteLine(string.Format("没有找到配置文件：{0}", fileName));
                return;
            }
            XDocument doc = XDocument.Load(fileName);

            Dictionary<string, IFeatureClass> fcName2FC = new Dictionary<string, IFeatureClass>();
            var contentEle = doc.Element("Template").Element("Content");
            //辅助要素类项
            var objectFCEle = contentEle.Element(fc.AliasName.ToUpper());
            var items = objectFCEle.Elements("Item");
            foreach (XElement ele in items)
            {
                if (!fcName2FC.ContainsKey(ele.Value))
                {
                    var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ele.Value.ToUpper());
                    })).FirstOrDefault() as IFeatureLayer;

                    if (lyr == null || lyr.FeatureClass == null)
                        continue;

                    fcName2FC.Add(ele.Value, lyr.FeatureClass);
                }
            }


            IQueryFilter qf = new QueryFilterClass();
            
            #region 设置newUnSelectOIDList关联的辅助设施要素为未选取状态
            qf.WhereClause = "";
            if (newUnSelectOIDList.Count > 0)
            {
                string oidSet = "";
                foreach (var oid in newUnSelectOIDList)
                {
                    if (oidSet != "")
                        oidSet += string.Format(",{0}", oid);
                    else
                        oidSet = string.Format("{0}", oid);
                }

                qf.WhereClause = string.Format("OBJECTID in ({0})", oidSet);

                IFeatureCursor fCursor = fc.Search(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    ITopologicalOperator top = f.ShapeCopy as ITopologicalOperator;
                    IGeometry buffGeo = top.Buffer(tolerance * 1.0e-3 * GApplication.Application.ActiveView.FocusMap.ReferenceScale);

                    ISpatialFilter sf = new SpatialFilterClass();

                    //获取该要素（未选取）相关的辅助设施要素（从设置为选取状态的辅助要素中检索）
                    foreach (var kv in fcName2FC)
                    {
                        string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", kv.Value.AliasName);
                        int selStateIndex = kv.Value.FindField(selStateFN);
                        if (selStateIndex == -1)
                        {
                            continue;
                        }

                        sf.WhereClause = string.Format("{0} is null", selStateFN);//从设置为选取状态的辅助要素中检索
                        sf.Geometry = buffGeo;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        IFeatureCursor afCursor = kv.Value.Update(sf, true);
                        IFeature af = null;
                        while ((af = afCursor.NextFeature()) != null)
                        {
                            af.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                            afCursor.UpdateFeature(af);

                            //关联注记的选取
                            selectAnnoByConnFe(kv.Value, af.OID, false);
                        }
                        Marshal.ReleaseComObject(afCursor);
                    }

                }
                Marshal.ReleaseComObject(fCursor);
            }
           
            #endregion

            #region 恢复newSelectOIDList关联的辅助设施要素为选取状态
            qf.WhereClause = "";
            if (newSelectOIDList.Count > 0)
            {
                string oidSet = "";
                foreach (var oid in newSelectOIDList)
                {
                    if (oidSet != "")
                        oidSet += string.Format(",{0}", oid);
                    else
                        oidSet = string.Format("{0}", oid);
                }

                qf.WhereClause = string.Format("OBJECTID in ({0})", oidSet);

                IFeatureCursor fCursor = fc.Search(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    ITopologicalOperator top = f.ShapeCopy as ITopologicalOperator;
                    IGeometry buffGeo = top.Buffer(tolerance * 1.0e-3 * GApplication.Application.ActiveView.FocusMap.ReferenceScale);

                    ISpatialFilter sf = new SpatialFilterClass();

                    //获取该要素（重新选取）相关的辅助设施要素（从设置为未选取状态的辅助要素中检索）
                    foreach (var kv in fcName2FC)
                    {
                        string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", kv.Value.AliasName);
                        int selStateIndex = kv.Value.FindField(selStateFN);
                        if (selStateIndex == -1)
                        {
                            continue;
                        }

                        sf.WhereClause = string.Format("{0} is not null", selStateFN);//从设置为未选取状态的辅助要素中检索
                        sf.Geometry = buffGeo;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        IFeatureCursor afCursor = kv.Value.Update(sf, true);
                        IFeature af = null;
                        while ((af = afCursor.NextFeature()) != null)
                        {
                            af.set_Value(selStateIndex, DBNull.Value);//恢复为选取状态
                            afCursor.UpdateFeature(af);

                            //关联注记的选取
                            selectAnnoByConnFe(kv.Value, af.OID, true);
                        }
                        Marshal.ReleaseComObject(afCursor);
                    }

                }
                Marshal.ReleaseComObject(fCursor);
            }
            
            #endregion

        }

        /// <summary>
        /// 1.根据街区显示级别、城市道路所在居民地分类码属性值，更新城市道路(GB=430200、430501、430502、430503)的显示符号
        /// 2.若街区不选取时，若城市道路的LGB也为城市道路(GB=430200、430501、430502、430503)，则应隐藏该城市道路；
        /// 3.若街区被重新选取时，若城市道路的LGB也为城市道路(GB=430200、430501、430502、430503)，则应显示该城市道路
        /// </summary>
        /// <param name="mainLevel">若为负值，则主区要素不参与本次选取</param>
        /// <param name="adjacentLevel">若为负值，则邻区要素不参与本次选取</param>
        public static void updateCityRoadSymbol(string fcName, int mainLevel, int adjacentLevel, string baseMap)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName);
            })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null || lyr.FeatureClass == null)
            {
                //MessageBox.Show(string.Format("未找到要素类{0}！",fcName));
                System.Diagnostics.Trace.WriteLine(string.Format("未找到要素类{0}！", fcName));
                return;
            }
            IFeatureClass lrdlFC = lyr.FeatureClass;

            #region 获取图层对照规则表
            string ruleMatchFileName = EnvironmentSettings.getLayerRuleDBFileName(GApplication.Application);
            DataTable dtLayerRule = CommonMethods.ReadToDataTable(ruleMatchFileName, "图层对照规则");
            #endregion

            #region 相关字段判断与获取
            string attachFN = GApplication.Application.TemplateManager.getFieldAliasName("Attach", lrdlFC.AliasName);
            string gbFN = GApplication.Application.TemplateManager.getFieldAliasName("GB", lrdlFC.AliasName);
            string lgbFN = GApplication.Application.TemplateManager.getFieldAliasName("LGB", lrdlFC.AliasName);
            string classFN = GApplication.Application.TemplateManager.getFieldAliasName("class1", lrdlFC.AliasName);
            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", lrdlFC.AliasName);

            int attachIndex = lrdlFC.FindField(attachFN);
            if (attachIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lrdlFC.AliasName, attachFN));
            }
            int gbIndex = lrdlFC.FindField(gbFN);
            if (gbIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lrdlFC.AliasName, gbFN));
            }
            int lgbIndex = lrdlFC.FindField(lgbFN);
            if (lgbIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lrdlFC.AliasName, lgbFN));
            }
            int classIndex = lrdlFC.FindField(classFN);
            if (classIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lrdlFC.AliasName, classFN));
            }
            int selStateIndex = lrdlFC.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", lrdlFC.AliasName, selStateFN));
            }
            #endregion

            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的城市道路线要素OID集合（街区不选取，且其内城市道路的LGB为城市道路）
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的城市道路线要素OID集合（街区不选取，且其内城市道路的LGB为城市道路）

            IQueryFilter qf = new QueryFilterClass();

            #region 更新主区城市道路的显示符号
            if (mainLevel >= 0)//mainLevel为负值，则认为不对主区城市道路的符号进行更新
            {
                qf.WhereClause = string.Format("{0} is null and {1} in (430200,430501,430502,430503) ", attachFN, gbFN);//主区中的城市道路
                IFeatureCursor fCursor = lrdlFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    int resaClass = 0;//城市道路所在居民地分类码(1/2/3/4/5,非城市道路为空，转为整形后为0)
                    int.TryParse(f.get_Value(classIndex).ToString(), out resaClass);
                    if (resaClass == 0)
                        continue;//非城市道路

                    string symbolGB = "";
                    if (resaClass <= mainLevel)//所在街区面显示,则该城市道路以GB显示符号类型
                    {
                        symbolGB = f.get_Value(gbIndex).ToString();

                        #region 判断该城市道路的选取状态是否发生变化
                        int lgb = 0;
                        int.TryParse(f.get_Value(lgbIndex).ToString(), out lgb);
                        if (lgb == 430200 || lgb == 430501 || lgb == 430502 || lgb == 430503)//城市道路的LGB亦为城市道路，则该道路的选取状态可能发生变化
                        {
                            object selStateVal = f.get_Value(selStateIndex);
                            if (!Convert.IsDBNull(selStateVal) && selStateVal.ToString() == "未选取（街区道路）")//因街区面的关系而未选取的街区道路，重新选取
                            {
                                newSelectOIDList.Add(f.OID);
                            }
                        }

                        #endregion
                    }
                    else//所在街区面不显示,则该城市道路以LGB显示符号类型
                    {
                        symbolGB = f.get_Value(lgbIndex).ToString();

                        #region 判断该城市道路的选取状态是否发生变化
                        int lgb = 0;
                        int.TryParse(symbolGB, out lgb);
                        if (lgb == 430200 || lgb == 430501 || lgb == 430502 || lgb == 430503)//城市道路的LGB亦为城市道路，则该道路的选取状态可能发生变化
                        {
                            object selStateVal = f.get_Value(selStateIndex);
                            if (Convert.IsDBNull(selStateVal))//街区面未选取，则该选取状态的城市道路需隐藏
                            {
                                newUnSelectOIDList.Add(f.OID);
                            }
                        }

                        #endregion
                    }

                    DataRow[] drArray = dtLayerRule.Select().Where(i => i["映射图层"].ToString().Trim() == fcName && i["RuleName"].ToSafeString().Contains(symbolGB)).ToArray();
                    if (drArray.Length == 0)
                        continue;

                    object ruleID = drArray[0]["RuleID"];
                    string ruleIDFeildName = drArray[0]["RuleIDFeildName"].ToString().Trim();

                    #region 获取规则ID字段索引号
                    int ruleIDIndex = lrdlFC.FindField(ruleIDFeildName);
                    if (ruleIDIndex == -1 && (lyr as IGeoFeatureLayer).Renderer is IRepresentationRenderer)
                    {
                        IRepresentationRenderer reprenderer = (lyr as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                        if (reprenderer != null)
                        {
                            IRepresentationClass repClass = reprenderer.RepresentationClass;
                            if (repClass != null)
                                ruleIDIndex = repClass.RuleIDFieldIndex;
                        }
                    }
                    #endregion
                    if (ruleIDIndex == -1)
                        continue;

                    f.set_Value(ruleIDIndex, ruleID);
                    fCursor.UpdateFeature(f);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            #region 更新邻区城市道路的显示符号
            if (adjacentLevel >= 0)//adjacentLevel为负值，则认为不对邻区城市道路的符号进行更新
            {
                qf.WhereClause = string.Format("{0} is not null and {1} in (430200,430501,430502,430503) ", attachFN, gbFN);//邻区中的城市道路
                IFeatureCursor fCursor = lrdlFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    int resaClass = 0;//城市道路所在居民地分类码(1/2/3/4/5,非城市道路为空，转为整形后为0)
                    int.TryParse(f.get_Value(classIndex).ToString(), out resaClass);
                    if (resaClass == 0)
                        continue;//非城市道路

                    string symbolGB = "";
                    if (resaClass <= adjacentLevel)//所在街区面显示,则该城市道路以GB显示符号类型
                    {
                        symbolGB = f.get_Value(gbIndex).ToString();

                        #region 判断该城市道路的选取状态是否发生变化
                        int lgb = 0;
                        int.TryParse(f.get_Value(lgbIndex).ToString(), out lgb);
                        if (lgb == 430200 || lgb == 430501 || lgb == 430502 || lgb == 430503)//城市道路的LGB亦为城市道路，则该道路的选取状态可能发生变化
                        {
                            object selStateVal = f.get_Value(selStateIndex);
                            if (!Convert.IsDBNull(selStateVal) && selStateVal.ToString() == "未选取（街区道路）")//因街区面的关系而未选取的街区道路，重新选取
                            {
                                newSelectOIDList.Add(f.OID);
                            }
                        }

                        #endregion
                    }
                    else//所在街区面不显示,则该城市道路以LGB显示符号类型
                    {
                        symbolGB = f.get_Value(lgbIndex).ToString();

                        #region 判断该城市道路的选取状态是否发生变化
                        int lgb = 0;
                        int.TryParse(symbolGB, out lgb);
                        if (lgb == 430200 || lgb == 430501 || lgb == 430502 || lgb == 430503)//城市道路的LGB亦为城市道路，则该道路的选取状态可能发生变化
                        {
                            object selStateVal = f.get_Value(selStateIndex);
                            if (Convert.IsDBNull(selStateVal))//街区面未选取，则该选取状态的城市道路需隐藏
                            {
                                newUnSelectOIDList.Add(f.OID);
                            }
                        }

                        #endregion
                    }

                    DataRow[] drArray = dtLayerRule.Select().Where(i => i["映射图层"].ToString().Trim() == fcName && i["RuleName"].ToSafeString().Contains(symbolGB)).ToArray();
                    if (drArray.Length == 0)
                        continue;

                    object ruleID = drArray[0]["RuleID"];
                    string ruleIDFeildName = drArray[0]["RuleIDFeildName"].ToString().Trim();

                    #region 获取规则ID字段索引号
                    int ruleIDIndex = lrdlFC.FindField(ruleIDFeildName);
                    if (ruleIDIndex == -1 && (lyr as IGeoFeatureLayer).Renderer is IRepresentationRenderer)
                    {
                        IRepresentationRenderer reprenderer = (lyr as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                        if (reprenderer != null)
                        {
                            IRepresentationClass repClass = reprenderer.RepresentationClass;
                            if (repClass != null)
                                ruleIDIndex = repClass.RuleIDFieldIndex;
                        }
                    }
                    #endregion
                    if (ruleIDIndex == -1)
                        continue;

                    f.set_Value(ruleIDIndex, ruleID);
                    fCursor.UpdateFeature(f);
                }
                Marshal.ReleaseComObject(fCursor);
            }
            #endregion

            //设置关联城市道路的选取状态
            foreach (var oid in newUnSelectOIDList)
            {
                IFeature f = lrdlFC.GetFeature(oid);
                f.set_Value(selStateIndex, "未选取（街区道路）");//设置为未选取状态
                f.Store();
            }

            foreach (var oid in newSelectOIDList)
            {
                IFeature f = lrdlFC.GetFeature(oid);
                f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                f.Store();
            }

            #region 关联注记的选取
            selectAnnoByConnFe(lrdlFC, newUnSelectOIDList, newSelectOIDList);
            #endregion

            #region 相关附属设施要素选取
            selectAncillaryFacilities(lrdlFC, newUnSelectOIDList, newSelectOIDList, baseMap);
            #endregion
        }

        /// <summary>
        /// 设置目标图层中指定要素的选取状态
        /// </summary>
        /// <param name="fc">目标图层</param>
        /// <param name="filter">筛选条件</param>
        /// <param name="bSelect">是否选取</param>
        public static void selectFeature(IFeatureClass fc, string filter, bool bSelect)
        {
            List<int> newUnSelectOIDList = new List<int>();//本次未被选取的道路线要素OID集合
            List<int> newSelectOIDList = new List<int>();//本次被重新选取的道路线要素OID集合

            string selStateFN = GApplication.Application.TemplateManager.getFieldAliasName("selectstate", fc.AliasName);
            int selStateIndex = fc.FindField(selStateFN);
            if (selStateIndex == -1)
            {
                throw new Exception(string.Format("要素类【{0}】中找不到字段【{1}】", fc.AliasName, selStateFN));
            }

            //1.从主区未选取要素中，恢复满足本次条件的未选取要素   
            IFeatureCursor fCursor = fc.Update(new QueryFilterClass(){WhereClause = filter}, true);
            IFeature f = null;
            while ((f = fCursor.NextFeature()) != null)
            {
                if (bSelect)
                {
                    f.set_Value(selStateIndex, DBNull.Value);//恢复选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次恢复选取状态要素的OID
                    newSelectOIDList.Add(f.OID);
                }
                else
                {
                    f.set_Value(selStateIndex, "要素选取");//设置为未选取状态
                    fCursor.UpdateFeature(f);

                    //收集本次未选取状态要素的OID
                    newUnSelectOIDList.Add(f.OID);
                }
            }
            Marshal.ReleaseComObject(fCursor);

            // 2.关联注记的选取
            selectAnnoByConnFe(fc, newUnSelectOIDList, newSelectOIDList);
        }
        #endregion
    }
}

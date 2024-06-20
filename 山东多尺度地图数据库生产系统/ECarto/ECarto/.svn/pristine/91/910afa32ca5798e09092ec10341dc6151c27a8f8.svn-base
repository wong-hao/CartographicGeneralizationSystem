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
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.AnnotationEngine
{
    /// <summary>
    /// 基于Maplex自动创建注记
    /// </summary>
    public class AnnotationAutoCreateCmd : SMGI.Common.SMGICommand
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

        public AnnotationAutoCreateCmd()
        {
            m_caption = "注记自动创建";
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

            ((IEngineEditEvents_Event)m_Application.EngineEditor).OnChangeFeature += new IEngineEditEvents_OnChangeFeatureEventHandler(EngineEditor_OnChangeFeature);
            ((IEngineEditEvents_Event)m_Application.EngineEditor).OnDeleteFeature += new IEngineEditEvents_OnDeleteFeatureEventHandler(EngineEditor_OnDeleteFeature);

        }

        public override void OnClick()
        {
            //生成注记
            DialogResult dialogResult = MessageBox.Show("批量创建将清空注记，确定要全部生成注记吗？", "提示", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes)
                return;

            string result0 = CheckAnnotationAutoCreate();
            if (!string.IsNullOrEmpty(result0))
            {
                MessageBox.Show(result0, "提示");
                return;
            }
            string result1 = DoAnnotationAutoCreate();
            if (!string.IsNullOrEmpty(result1))
            {
                MessageBox.Show(result1, "提示");
                return;
            }

            MessageBox.Show(string.Format("【{0}】处理完成!", m_caption), "提示");
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, m_Application.ActiveView.Extent);

        }


        //消隐处理（注记几何发生变化时，若存在消隐处理需更新）
        private void EngineEditor_OnChangeFeature(IObject Object)
        {
            var delFe = Object as IFeature;
            if (delFe.Shape == null)
                return;
            var delAnnoFe = Object as IAnnotationFeature2;
            if (delAnnoFe == null)
                return;

            if (!(delAnnoFe as IFeatureChanges).ShapeChanged) 
                return;
            IGeometry oldGeo = (delAnnoFe as IFeatureChanges).OriginalShape;

            int classID = delAnnoFe.AnnotationClassID;
            int linkedFID = delAnnoFe.LinkedFeatureID;
            int blanktypeIndex = delFe.Fields.FindField("blankingtype");
            if (blanktypeIndex == -1)
                return;
            string type = delFe.get_Value(blanktypeIndex).ToString();
            if (type != "单要素几何覆盖" && type != "多要素几何覆盖" && type != "单要素局部消隐")
                return;

            var lyr = m_Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IGeoFeatureLayer && (l as IFeatureLayer).FeatureClass.ObjectClassID == classID; })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null)
                return;
            //获取选取要素制图表达规则
            var feRenderer = (lyr as IGeoFeatureLayer).Renderer;
            if (!(feRenderer is IRepresentationRenderer)) 
                return;
            var repClass = ((IRepresentationRenderer)feRenderer).RepresentationClass;
            IMapContext mapContext = new MapContextClass();
            mapContext.InitFromDisplay(GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation);
            if (lyr.FeatureClass == null)
                return;
            IFeatureClass fc = lyr.FeatureClass;


            var reps = new List<IRepresentation>();
            if (type == "单要素几何覆盖")
            {
                //清理原消隐效果
                IFeature linkedFe = fc.GetFeature(linkedFID);
                if (linkedFe == null)
                    return;
                if (oldGeo != null)
                {
                    var rep = repClass.GetRepresentation(linkedFe, mapContext);
                    reps.Add(rep);

                    AnnotationHelper.OverrideRecoveryByGeometry(reps, oldGeo);
                }

                //消隐处理
                AnnotationHelper.OverrideByGeometry(reps, delFe.Shape);
            }
            else if (type == "多要素几何覆盖")
            {
                //清理原消隐效果
                if (oldGeo != null)
                {
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = oldGeo;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeatureCursor objFeCursor = fc.Search(sf, false);
                    IFeature objFe = null;
                    while ((objFe = objFeCursor.NextFeature()) != null)
                    {
                        var rep = repClass.GetRepresentation(objFe, mapContext);
                        reps.Add(rep);

                        Marshal.ReleaseComObject(objFe);
                    }
                    Marshal.ReleaseComObject(objFeCursor);

                    AnnotationHelper.OverrideRecoveryByGeometry(reps, oldGeo);
                }

                //消隐处理
                reps = new List<IRepresentation>();
                ISpatialFilter sf2 = new SpatialFilterClass();
                sf2.Geometry = delFe.Shape;
                sf2.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor objFeCursor2 = fc.Search(sf2, false);
                IFeature objFe2 = null;
                while ((objFe2 = objFeCursor2.NextFeature()) != null)
                {
                    var rep = repClass.GetRepresentation(objFe2, mapContext);
                    reps.Add(rep);

                    Marshal.ReleaseComObject(objFe2);
                }
                Marshal.ReleaseComObject(objFeCursor2);

                AnnotationHelper.OverrideByGeometry(reps, delFe.Shape);
            }
            else if (type == "单要素局部消隐")
            {
                //清理原消隐效果
                IFeature linkedFe = fc.GetFeature(linkedFID);
                if (linkedFe == null)
                    return;
                if (oldGeo != null)
                {
                    var rep = repClass.GetRepresentation(linkedFe, mapContext);
                    reps.Add(rep);

                    AnnotationHelper.DashRecoveryByGeometry(reps, oldGeo);
                }

                //消隐处理
                AnnotationHelper.DashByGeometry(reps, delFe.Shape);
            }

            
        }
        //消隐处理（注记要素删除时，若存在消隐处理需清除消隐效果）
        private void EngineEditor_OnDeleteFeature(IObject Object)
        {
            var delFe = Object as IFeature;
            var delAnnoFe = Object as IAnnotationFeature2;
            if (delAnnoFe == null)
                return;
            IGeometry oldGeo = (delAnnoFe as IFeatureChanges).OriginalShape;

            int classID = delAnnoFe.AnnotationClassID;
            int linkedFID = delAnnoFe.LinkedFeatureID;
            int blanktypeIndex = delFe.Fields.FindField("blankingtype");
            if (blanktypeIndex == -1)
                return;
            string type = delFe.get_Value(blanktypeIndex).ToString();
            if (type != "单要素几何覆盖" && type != "多要素几何覆盖" && type != "单要素局部消隐")
                return;

            var lyr = m_Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IGeoFeatureLayer && (l as IFeatureLayer).FeatureClass.ObjectClassID == classID; })).FirstOrDefault() as IFeatureLayer;
            if (lyr == null)
                return;
            //获取选取要素制图表达规则
            var feRenderer = (lyr as IGeoFeatureLayer).Renderer;
            if (!(feRenderer is IRepresentationRenderer)) 
                return;
            var repClass = ((IRepresentationRenderer)feRenderer).RepresentationClass;
            IMapContext mapContext = new MapContextClass();
            mapContext.InitFromDisplay(GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation);
            if (lyr.FeatureClass == null)
                return;
            IFeatureClass fc = lyr.FeatureClass;

            //清理原消隐效果
            var reps = new List<IRepresentation>();
            if (type == "单要素几何覆盖")
            {
                IFeature linkedFe = fc.GetFeature(linkedFID);
                if (linkedFe == null)
                    return;
                if (oldGeo != null)
                {
                    var rep = repClass.GetRepresentation(linkedFe, mapContext);
                    reps.Add(rep);

                    AnnotationHelper.OverrideRecoveryByGeometry(reps, oldGeo);
                }
            }
            else if (type == "多要素几何覆盖")
            {
                if (oldGeo != null)
                {
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = oldGeo;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeatureCursor objFeCursor = fc.Search(sf, false);
                    IFeature objFe = null;
                    while ((objFe = objFeCursor.NextFeature()) != null)
                    {
                        var rep = repClass.GetRepresentation(objFe, mapContext);
                        reps.Add(rep);

                        Marshal.ReleaseComObject(objFe);
                    }
                    Marshal.ReleaseComObject(objFeCursor);

                    AnnotationHelper.OverrideRecoveryByGeometry(reps, oldGeo);
                }

            }
            else if (type == "单要素局部消隐")
            {
                IFeature linkedFe = fc.GetFeature(linkedFID);
                if (linkedFe == null)
                    return;
                if (oldGeo != null)
                {
                    var rep = repClass.GetRepresentation(linkedFe, mapContext);
                    reps.Add(rep);

                    AnnotationHelper.DashRecoveryByGeometry(reps, oldGeo);
                }
            }
        }

        #region 提取onClick的逻辑事务
        public static string DoAnnotationAutoCreate()
        {
            string result = string.Empty;
            //获取内图廓线范围???
            string mapBorderLineLayerName = "CPTL";

            IFeatureClass mapBorderFeatureClass = (GApplication.Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(mapBorderLineLayerName);
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "GB=120100";

            IPolyline extentGeo = null;
            IFeatureCursor feCursor = mapBorderFeatureClass.Search(qf, false);
            IFeature fe = null;
            while ((fe = feCursor.NextFeature()) != null)
            {
                IPolyline shape = fe.Shape as IPolyline;
                if (shape == null || !shape.IsClosed)
                    continue;//非封闭线

                if (extentGeo == null || extentGeo.Length < (fe.Shape as IPolyline).Length)
                {
                    extentGeo = fe.ShapeCopy as IPolyline;
                }
            }
            Marshal.ReleaseComObject(feCursor);

            using (var wo = GApplication.Application.SetBusy())
            {
                wo.SetText("【注记自动创建】处理中...");
                XElement contentXEle = GApplication.Application.Template.Content;
                XElement annoRuleEle = contentXEle.Element("AnnoFull");
                string annoRuleFilePath = GApplication.Application.Template.Root + "\\" + annoRuleEle.Value;
                DataTable ruleTable = CommonMethods.ReadToDataTable(annoRuleFilePath, "注记规则");
                DataTable fontmappingTable = CommonMethods.ReadToDataTable(annoRuleFilePath, "字体映射");

                //获取注记要素类
                Dictionary<string, IFeatureClass> annoName2FeatureClass = new Dictionary<string, IFeatureClass>();
                System.Data.DataTable dtAnnoLayers = ruleTable.AsDataView().ToTable(true, new string[] { "注记要素类名" });//distinct
                for (int i = 0; i < dtAnnoLayers.Rows.Count; ++i)
                {
                    //图层名
                    string annoFCName = dtAnnoLayers.Rows[i]["注记要素类名"].ToString().Trim();
                    if (!(GApplication.Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, annoFCName))
                    {
                        result += string.Format("数据层：缺少注记要素类[{0}]!\r\n", annoFCName);
                        return result;
                    }

                    IFeatureClass annoFeatureClass = (GApplication.Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(annoFCName);

                    annoName2FeatureClass.Add(annoFCName, annoFeatureClass);
                }

                if (annoName2FeatureClass.Count == 0)
                {
                    result += string.Format("数据层：规则库中没有指定注记目标图层\r\n");
                    return result;
                }

                GApplication.Application.EngineEditor.StartOperation();

                AnnotationCreator ac = new AnnotationCreator(GApplication.Application);
                string err = ac.createAnnotateAuto(ruleTable, fontmappingTable, annoName2FeatureClass, extentGeo, wo);
                if (err == "")
                {
                    GApplication.Application.EngineEditor.StopOperation("创建注记");
                }
                else
                {
                    result += err + "\r\n";
                    GApplication.Application.EngineEditor.AbortOperation();                    
                }
            }
            return result;
        }
        #endregion

        #region 提取onClick的判断前提事务
        public static string CheckAnnotationAutoCreate()
        {
            string result = string.Empty;
            string mapBorderLineLayerName = "CPTL";
            if (!(GApplication.Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mapBorderLineLayerName))
            {
                result += string.Format("数据层：缺少图层【{0}】!\r\n", mapBorderLineLayerName);
            }

            IFeatureClass mapBorderFeatureClass = (GApplication.Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(mapBorderLineLayerName);
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "GB=120100";
            int ct = mapBorderFeatureClass.FeatureCount(qf);
            if (ct == 0)
            {
                result += string.Format("数据层：:图层【{0}】中未找到内图廓!\r\n", mapBorderLineLayerName);
            }
            return result;
        }
        #endregion
    }
}

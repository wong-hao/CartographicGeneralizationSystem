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
using System.Data;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
 
namespace SMGI.Plugin.EmergencyMap
{
    //交互侧界面生成
    public class CJMColorManualCmd : SMGI.Common.SMGICommand
    {
        Dictionary<string, int> ColorRules = new Dictionary<string,int>();//色带颜色->ruleID；
        IFeatureClass CJMFeatureClass = null;
        ILayer CJMLayer = null;
        public CJMColorManualCmd()
        {
             
            m_caption = "侧界生成";
            m_category = "侧界生成";
            m_toolTip = "侧界生成";
        }

        public override bool Enabled
        {
            get
            {

                return m_Application != null && m_Application.Workspace != null && m_Application.MapControl.Map.SelectionCount>0 && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        double cjmDis = 0.2;
        Dictionary<string, ICmykColor> colors = null;//色带颜色;
        string typeName = "";
        bool bSingle = false;
        public override void OnClick()
        {
            var SDMLyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == "SDM");
            })).ToArray();
            ILayer SDMLayer = SDMLyrs.First();
            IFeatureClass SDMFC = (SDMLayer as IFeatureLayer).FeatureClass;
            int feCount = SDMFC.FeatureCount(null);
            if (feCount == 0)
            {
                MessageBox.Show("数据没有色带面！");
                return;
            }
            try
            {
                    m_Application.EngineEditor.StartOperation();
                    CJMLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("CJL"))).FirstOrDefault();
                    CJMFeatureClass = (CJMLayer as IFeatureLayer).FeatureClass;
                    FrmCJM frm = new FrmCJM("侧界面创建");
                    frm.StartPosition = FormStartPosition.CenterParent;
                    if (frm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    typeName = frm.LyrName;
                    colors = frm.CMYKColors;
                    double dis = frm.SMDWidth;
                    cjmDis = dis * m_Application.ActiveView.FocusMap.ReferenceScale * 1e-3;
                    bSingle = frm.BSingle;
                    using (var wo = m_Application.SetBusy())
                    {
                        //1.合并行政区面
                        wo.SetText("正在合并行政区面……");
                        IGeometry unionGeometry = MergePolygon();
                        //2.创建合并面的侧界
                        wo.SetText("正在创建侧界……");
                        CreateCJM(unionGeometry);
                        //3.分割侧界（按内图廓和色带面内层分割侧界）
                        wo.SetText("正在分割侧界……");
                        IGeometry innerMapborder = GetInnerMapborder();//内图廓
                        ClipCJM(innerMapborder.Envelope, innerMapborder);
                        IGeometry innerSDM = GetInnerSDM();//色带面内层
                        ClipCJM(innerSDM, (innerSDM as ITopologicalOperator).Boundary);
                        //4.删除多余侧界
                        wo.SetText("正在优化侧界……");
                        DeleteCJM(innerMapborder.Envelope, innerSDM);
                        m_Application.EngineEditor.StopOperation("侧界面创建");
                    }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                m_Application.EngineEditor.AbortOperation();
                MessageBox.Show(string.Format("侧界面创建出错,Error:{0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                m_Application.MapControl.Map.ClearSelection();
                m_Application.ActiveView.Refresh();
            }
        }

        private IGeometry MergePolygon()
        {
            IGeometry unionGeometry = null;
            try
            {
                ISelection featureSelection = m_Application.MapControl.Map.FeatureSelection;
                IEnumFeature enumFeature = featureSelection as IEnumFeature;
                enumFeature.Reset();
                IFeature selectFeature = null;
                selectFeature = enumFeature.Next();
                unionGeometry = selectFeature.ShapeCopy;
                ITopologicalOperator feaTopo = null;
                while ((selectFeature = enumFeature.Next()) != null)
                {
                    feaTopo = unionGeometry as ITopologicalOperator;
                    unionGeometry = feaTopo.Union(selectFeature.ShapeCopy);
                    feaTopo.Simplify();
                }
                return unionGeometry;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateCJM(IGeometry unionGeo)
        {
           
            try
            {
                geometry = unionGeo;
                IGeometry outgeo = CreateBufferGeo(geometry, cjmDis);
                if (outgeo == null)
                {
                    MessageBox.Show("侧界面缓冲出错！");
                    return;
                }
                IGeoFeatureLayer geoFlyr = CJMLayer as IGeoFeatureLayer;
                IMapContext mctx = new MapContextClass();
                mctx.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, geoFlyr.AreaOfInterest);
                IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
                IRepresentationClass rpc = repRender.RepresentationClass;
                IRepresentationRules rules = rpc.RepresentationRules;
                rules.Reset();
                IRepresentationRule rule = null;
                int ruleID;
                while (true)
                {
                    rules.Next(out ruleID, out rule);
                    if (rule == null) break;
                    if (rules.get_Name(ruleID) != "不显示要素")
                    {
                        //修改颜色
                        string rulename = rules.get_Name(ruleID);
                        if (!colors.ContainsKey(rulename))
                            continue;

                        ColorRules[rulename] = ruleID;

                    }
                }

                string type = "外层";
                IFeature feinner = CJMFeatureClass.CreateFeature();
                feinner.set_Value(CJMFeatureClass.FindField("RuleID"), ColorRules[type]);
                if (cjmDis > 0)
                {
                    feinner.Shape = (outgeo as ITopologicalOperator).Difference(geometry);
                }
                else
                {
                    feinner.Shape = (geometry as ITopologicalOperator).Difference(outgeo);
                }
                feinner.Store();

                var rep = rpc.GetRepresentation(feinner, mctx);
                OverrideColorValueSet(rep, colors[type]);

                type = "内层";
                if (!bSingle)
                {
                    IGeometry ingeo = CreateBufferGeo(geometry, 0.5 * cjmDis);
                    feinner = CJMFeatureClass.CreateFeature();
                    feinner.set_Value(CJMFeatureClass.FindField("RuleID"), ColorRules[type]);
                    if (cjmDis > 0)
                    {
                        feinner.Shape = (ingeo as ITopologicalOperator).Difference(geometry);
                    }
                    else
                    {
                        feinner.Shape = (geometry as ITopologicalOperator).Difference(ingeo);
                    }
                    feinner.Store();

                    rep = rpc.GetRepresentation(feinner, mctx);
                    OverrideColorValueSet(rep, colors[type]);
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ClipCJM(IGeometry clipGeoPolygone,IGeometry clipGeoPolyline)
        {
            //检索
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelOverlaps;
            sf.Geometry = clipGeoPolygone;
            IFeatureCursor feCursor = CJMFeatureClass.Search(sf, true);
            IFeature fe = null;
            List<int> interFIDList = new List<int>();
            while ((fe = feCursor.NextFeature()) != null)
            {
                interFIDList.Add(fe.OID);
            }
            Marshal.ReleaseComObject(feCursor);

            //裁切
            foreach (var fid in interFIDList)
            {
                fe = CJMFeatureClass.GetFeature(fid);

                IFeatureEdit feEdit = (IFeatureEdit)fe;
                ISet feSet = null;
                try
                {
                    feSet = feEdit.Split(clipGeoPolyline);

                    #region 打散多部件
                    if (feSet != null)
                    {
                        feSet.Reset();

                        while (true)
                        {
                            IFeature subFe = feSet.Next() as IFeature;
                            if (subFe == null)
                                break;

                            var po = (IPolygon4)subFe.Shape;
                            var gc = (IGeometryCollection)po.ConnectedComponentBag;
                            if (gc.GeometryCount > 1)
                            {
                                var fci = CJMFeatureClass.Insert(true);
                                for (var i = 1; i < gc.GeometryCount; i++)
                                {
                                    var fb = CJMFeatureClass.CreateFeatureBuffer();

                                    //几何赋值
                                    fb.Shape = gc.Geometry[i];

                                    //属性赋值
                                    for (int j = 0; j < fb.Fields.FieldCount; j++)
                                    {
                                        IField pfield = fb.Fields.get_Field(j);
                                        if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                                        {
                                            continue;
                                        }

                                        if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                                        {
                                            continue;
                                        }

                                        int index = subFe.Fields.FindField(pfield.Name);
                                        if (index != -1 && pfield.Editable)
                                        {
                                            fb.set_Value(j, subFe.get_Value(index));
                                        }

                                    }
                                    fci.InsertFeature(fb);
                                }
                                fci.Flush();

                                System.Runtime.InteropServices.Marshal.ReleaseComObject(fci);

                                //修改subFe的几何为 gc.Geometry[0]
                                subFe.Shape = gc.Geometry[0];
                                subFe.Store();
                            }

                        }

                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    continue;
                }

                
            }
        }

        private void DeleteCJM(IGeometry innerMapBorder,IGeometry innerSDM)
        {
            IFeatureCursor feCursor = CJMFeatureClass.Update(null, false);
            IFeature fe = null;
            while ((fe = feCursor.NextFeature()) != null)
            { 
                IRelationalOperator relOp = fe.ShapeCopy as IRelationalOperator;
                if ((!relOp.Within(innerMapBorder)) || relOp.Within(innerSDM))//
                {
                    fe.Delete();
                }
            }
            Marshal.ReleaseComObject(feCursor);
        }

        private IGeometry CreateBufferGeo(IGeometry polygon,double dis)
        {
            IGeometry buffer = null;
            try
            {
                 buffer = (polygon as ITopologicalOperator).Buffer(dis);
            }
             catch
            {
                buffer = CreateBufferEffect(polygon, dis);
            }
            return buffer;
        }
        //根据几何特效来创建
        private IGeometry CreateBufferEffect(IGeometry polygon,double dis)
        {
            
            var helper = new GeometricEffectBufferClass();
            IGraphicAttributes attrs = helper as IGraphicAttributes;
            for (int i = 0; i < attrs.GraphicAttributeCount; i++)
            {
                int attrid = attrs.get_ID(i);
                string name = attrs.get_Name(attrid);

            }
            helper.set_Value(0, dis);
            helper.Reset(polygon);
            while (true)
            {
                var g = helper.NextGeometry();
                if (g == null)
                    break;
                return g;
            }
            return null;
        }

        /// <summary>
        /// 获取内图廓
        /// </summary>
        /// <returns></returns>
        private IGeometry GetInnerMapborder()
        {
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == "LLINE");
            })).ToArray();
            ILayer pRepLayer = lyrs.First();//boua
            IFeatureClass fcl = (pRepLayer as IFeatureLayer).FeatureClass;

            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE ='内图廓'";
            IFeatureCursor fcursor = fcl.Search(qf, false);
            IFeature fe = fcursor.NextFeature();
            Marshal.ReleaseComObject(fcursor);
            Marshal.ReleaseComObject(qf);
            if (fe == null)
            {
                MessageBox.Show("数据没有内图廓！");
                return null;
            }
            return fe.ShapeCopy;
        }

        /// <summary>
        /// 获取色带面内层
        /// </summary>
        /// <returns></returns>
        private IGeometry GetInnerSDM()
        {
            var SDMLyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == "SDM");
            })).ToArray();
            var ClipBoundaryLyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == "ClipBoundary");
            })).ToArray();
            
            ILayer SDMLayer = SDMLyrs.First();
            IFeatureClass SDMFC = (SDMLayer as IFeatureLayer).FeatureClass;
            ILayer CBLayer = ClipBoundaryLyrs.First();
            IFeatureClass CBFC = (CBLayer as IFeatureLayer).FeatureClass;

            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE ='内层'";
            IFeatureCursor fcursor = SDMFC.Search(qf, false);
            IFeature fe = fcursor.NextFeature();
            if (fe == null)
            {
                MessageBox.Show("数据没有色带面！");
                return null;
            }
            IGeometry sdm = fe.ShapeCopy;

            qf.WhereClause = "TYPE ='裁切面'";
            fcursor = CBFC.Search(qf,false);
            fe = fcursor.NextFeature();
            if (fe == null)
            {
                MessageBox.Show("数据没有裁切面！");
                return null;
            }
            IGeometry clip = fe.ShapeCopy;

            Marshal.ReleaseComObject(fcursor);
            Marshal.ReleaseComObject(qf);
            IGeometry unionGeo = (sdm as ITopologicalOperator).Union(clip);
            (unionGeo as ITopologicalOperator).Simplify();
            return unionGeo;

        }

        private void ClearLayer()
        {
            IFeature fe;
            IFeatureCursor cursor = CJMFeatureClass.Update(null, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                cursor.DeleteFeature();
            }
            Marshal.ReleaseComObject(cursor);
        }

        private IFeature SDMColorSet(Dictionary<string, ICmykColor> colors)
        {
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == "ClipBoundary");
            })).ToArray();
            ILayer pRepLayer = lyrs.First();//boua
            IFeatureClass fcl = (pRepLayer as IFeatureLayer).FeatureClass;
         
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE ='裁切面'";
            IFeatureCursor fcursor = fcl.Search(qf, false);
            IFeature fe = fcursor.NextFeature();
            if (fe == null)
            {
                MessageBox.Show("数据没有裁切面！");
                return null;
            }
            Marshal.ReleaseComObject(fcursor);
            Marshal.ReleaseComObject(qf);
            ILayer cjmLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("CJL"))).FirstOrDefault();
            CJMFeatureClass = (cjmLayer as IFeatureLayer).FeatureClass;
            //修改cjm图层 rule的颜色

            var rp = (cjmLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IRepresentationRules rules = m_RepClass.RepresentationRules;
         
            rules.Reset();
            IRepresentationRule rule = null;
            int ruleID;
            while (true)
            {
                rules.Next(out ruleID, out rule);
                if (rule == null) break;
                if (rules.get_Name(ruleID) != "不显示要素")
                {
                    //修改颜色
                    string rulename = rules.get_Name(ruleID);
                    if (!colors.ContainsKey(rulename))
                        continue;
                    
                    IBasicFillSymbol fillSym = rule.get_Layer(0) as IBasicFillSymbol;
                    if (fillSym != null)
                    {
                        
                       IFillPattern fillPattern = fillSym.FillPattern;
                       IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                       fillAttrs.set_Value(0, colors[rulename]);
                      
                    }
                    ColorRules[rulename] = ruleID;

                }
            }
           
          //  m_RepClass.RepresentationRules = rules;
            return fe;
        }
        public void OverrideColorValueSet(IRepresentation rep, IColor pColor)
        {

            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            IBasicFillSymbol fillSym = ruleOrg.get_Layer(0) as IBasicFillSymbol;
            IGraphicAttributes ga = fillSym.FillPattern as IGraphicAttributes;
            if (fillSym != null)
            {
                int id = ga.get_IDByName("Color");
                rep.set_Value(ga, id, pColor);
            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
            rep.UpdateFeature();
            rep.Feature.Store();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(rep);
        }
        private IRepresentationWorkspaceExtension GetRepersentationWorkspace(IWorkspace workspace)
        {
            IWorkspaceExtensionManager wem = workspace as IWorkspaceExtensionManager;
            UID uid = new UIDClass();
            uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            IRepresentationWorkspaceExtension rwe = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
            return rwe;
        }


        public ESRI.ArcGIS.Geometry.IGeometry geometry { get; set; }
    }
}

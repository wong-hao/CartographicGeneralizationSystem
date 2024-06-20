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
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using System.Data;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;

namespace SMGI.Plugin.EmergencyMap
{
    //交互色带面生成
    public class SDMColorManualTool : SMGI.Common.SMGITool
    {
        string _layerName;
        Dictionary<string, ICmykColor> _sdmColors;
        double _sdmTotalWidth;
        int _sdmLayerNum;

        bool smdFlag = false;
        Dictionary<string, int> ColorRules = new Dictionary<string, int>();//色带颜色->ruleID；
        IFeatureClass SDMfcl = null;
        ILayer sdmlayer = null;
        public SDMColorManualTool()
        {

            m_caption = "色带普色";
            m_category = "色带普色";
            m_toolTip = "色带普色";
        }

        public override bool Enabled
        {
            get
            {

                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override bool Deactivate()
        {
            smdFlag = false;
            return base.Deactivate();
        }


        public override void OnClick()
        {
            if (m_Application.ActiveView.FocusMap.ReferenceScale == 0)
            {
                MessageBox.Show("请设置地图参考比例尺！");
                return;
            }

            sdmlayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("SDM"))).FirstOrDefault();
            SDMfcl = (sdmlayer as IFeatureLayer).FeatureClass;
            SDMCreateFrom frm = new SDMCreateFrom("色带面交互创建");
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
            {
                smdFlag = false;
                return;
            }
            smdFlag = true;

            _layerName = frm.LayerName;
            _sdmColors = frm.SDMColors;
            _sdmTotalWidth = frm.SDMTotalWidth * m_Application.ActiveView.FocusMap.ReferenceScale * 1e-3;
            _sdmLayerNum = frm.SDMLayerNum;
        }

        private void CreateSDM(IGeometry g, string lyrName)
        {
                WaitOperation wo = m_Application.SetBusy();
                try
                {
                    var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IFeatureLayer) && ((l as IFeatureLayer).Name == lyrName);
                    })).FirstOrDefault();
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = g;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeature f = null;
                    if ((lyr as IFeatureLayer).FeatureClass.FeatureCount(sf) > 0)
                    {
                        var cursor = (lyr as IFeatureLayer).FeatureClass.Search(sf as IQueryFilter, false);
                        f = cursor.NextFeature();
                        if (f != null)
                        {
                            m_Application.MapControl.Map.SelectFeature(lyr, f);
                        }
                        Marshal.ReleaseComObject(cursor);

                    }
                    if (f != null)
                    {
                        #region
                        var geometry = f.ShapeCopy;
                        (geometry as ITopologicalOperator).Simplify();

                        List<IPolygon> outBouaGeoList = new List<IPolygon>();
                        List<IPolygon> inBouaGeoList = new List<IPolygon>();
                        IGeometryCollection gc = geometry as IGeometryCollection;
                        if (gc.GeometryCount > 1)
                        {
                            for (int i = 0; i < gc.GeometryCount; i++)
                            {
                                if (gc.get_Geometry(i).IsEmpty)
                                    continue;

                                if ((gc.get_Geometry(i) as IRing).IsExterior)
                                {
                                    IPolygon pl = new PolygonClass();
                                    (pl as IGeometryCollection).AddGeometry(gc.get_Geometry(i));
                                    (pl as ITopologicalOperator).Simplify();

                                    outBouaGeoList.Add(pl);
                                }
                                else
                                {
                                    IPolygon pl = new PolygonClass();
                                    (pl as IGeometryCollection).AddGeometry(gc.get_Geometry(i));
                                    (pl as ITopologicalOperator).Simplify();

                                    inBouaGeoList.Add(pl);
                                }
                            }
                        }
                        else
                        {
                            outBouaGeoList.Add(geometry as IPolygon);
                        }

                        IGeoFeatureLayer geoFlyr = sdmlayer as IGeoFeatureLayer;
                        IMapContext mctx = new MapContextClass();
                        mctx.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, geoFlyr.AreaOfInterest);

                        IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
                        IRepresentationClass rpc = repRender.RepresentationClass;

                        ICmykColor outColor = _sdmColors["外层"];
                        ICmykColor inColor = _sdmColors["内层"];
                        double widthPerLayer = _sdmTotalWidth / _sdmLayerNum;
                        if (_sdmLayerNum == 2)
                        {
                            #region 常规色带面
                            foreach (var geo in outBouaGeoList)
                            {
                                IGeometry ingeo = CreateBufferGeo(geo, widthPerLayer);
                                IGeometry outgeo = CreateBufferGeo(geo, 2 * widthPerLayer);
                                if (ingeo == null || outgeo == null)
                                {
                                    MessageBox.Show("色带面缓冲出错！");
                                    return;
                                }

                                string type = "外层";
                                IFeature feinner = SDMfcl.CreateFeature();
                                feinner.set_Value(SDMfcl.FindField("TYPE"), type);
                                feinner.set_Value(SDMfcl.FindField("RuleID"), 1);
                                if (widthPerLayer > 0)
                                {
                                    feinner.Shape = (outgeo as ITopologicalOperator).Difference(geo);
                                }
                                else
                                {
                                    feinner.Shape = (geo as ITopologicalOperator).Difference(outgeo);
                                }
                                feinner.Store();
                                var rep = rpc.GetRepresentation(feinner, mctx);
                                OverrideColorValueSet(rep, _sdmColors[type]);

                                type = "内层";
                                feinner = SDMfcl.CreateFeature();
                                feinner.set_Value(SDMfcl.FindField("TYPE"), type);
                                feinner.set_Value(SDMfcl.FindField("RuleID"), 1);
                                if (widthPerLayer > 0)
                                {
                                    feinner.Shape = (ingeo as ITopologicalOperator).Difference(geo);
                                }
                                else
                                {
                                    feinner.Shape = (geo as ITopologicalOperator).Difference(ingeo);
                                }
                                feinner.Store();
                                rep = rpc.GetRepresentation(feinner, mctx);
                                OverrideColorValueSet(rep, _sdmColors[type]);

                                Marshal.ReleaseComObject(feinner);
                            }

                            foreach (var geo in inBouaGeoList)
                            {
                                IGeometry ingeo = CreateBufferGeo(geo, -widthPerLayer);
                                IGeometry outgeo = CreateBufferGeo(geo, -2 * widthPerLayer);
                                if (ingeo == null || outgeo == null)
                                {
                                    continue;
                                }

                                string type = "外层";
                                IFeature feinner = SDMfcl.CreateFeature();
                                feinner.set_Value(SDMfcl.FindField("TYPE"), type);
                                feinner.set_Value(SDMfcl.FindField("RuleID"), 1);
                                if (widthPerLayer > 0)
                                {
                                    feinner.Shape = (geo as ITopologicalOperator).Difference(outgeo);
                                }
                                else
                                {
                                    feinner.Shape = (outgeo as ITopologicalOperator).Difference(geo);
                                }
                                feinner.Store();
                                var rep = rpc.GetRepresentation(feinner, mctx);
                                OverrideColorValueSet(rep, _sdmColors[type]);

                                type = "内层";
                                feinner = SDMfcl.CreateFeature();
                                feinner.set_Value(SDMfcl.FindField("TYPE"), type);
                                feinner.set_Value(SDMfcl.FindField("RuleID"), 1);
                                if (widthPerLayer > 0)
                                {
                                    feinner.Shape = (geo as ITopologicalOperator).Difference(ingeo);
                                }
                                else
                                {
                                    feinner.Shape = (ingeo as ITopologicalOperator).Difference(geo);
                                }
                                feinner.Store();
                                rep = rpc.GetRepresentation(feinner, mctx);
                                OverrideColorValueSet(rep, _sdmColors[type]);

                                Marshal.ReleaseComObject(feinner);
                            }
                            #endregion
                        }
                        else//色带面层数大于2
                        {
                            #region 渐变色带面
                            foreach (var geo in outBouaGeoList)
                            {
                                Dictionary<int, IGeometry> bufferGeoList = new Dictionary<int, IGeometry>();
                                bufferGeoList[-1] = geo;
                                for (int i = 0; i < _sdmLayerNum; i++)
                                {

                                    IGeometry bufferGeo = CreateBufferGeo(geo, (i + 1) * widthPerLayer);
                                    if (bufferGeo == null)
                                    {
                                        MessageBox.Show("色带面缓冲出错！");
                                        return;
                                    }
                                    bufferGeoList[i] = bufferGeo;
                                    GC.Collect();
                                }

                                for (int i = 0; i < _sdmLayerNum; i++)
                                {
                                    string type = "内层";
                                    if (_sdmLayerNum - 1 == i)
                                    {
                                        type = "外层";
                                    }

                                    IFeature newFe = SDMfcl.CreateFeature();
                                    newFe.set_Value(SDMfcl.FindField("TYPE"), type);
                                    newFe.set_Value(SDMfcl.FindField("RuleID"), 1);
                                    if (widthPerLayer > 0)
                                    {
                                        newFe.Shape = (bufferGeoList[i] as ITopologicalOperator).Difference(bufferGeoList[i - 1]);
                                    }
                                    else
                                    {
                                        newFe.Shape = (bufferGeoList[i - 1] as ITopologicalOperator).Difference(bufferGeoList[i]);
                                    }
                                    newFe.Store();


                                    CmykColorClass color = new CmykColorClass();
                                    color.Cyan = (int)(inColor.Cyan - i * (inColor.Cyan - outColor.Cyan) * 1.0 / _sdmLayerNum);
                                    color.Magenta = (int)(inColor.Magenta - i * (inColor.Magenta - outColor.Magenta) * 1.0 / _sdmLayerNum);
                                    color.Yellow = (int)(inColor.Yellow - i * (inColor.Yellow - outColor.Yellow) * 1.0 / _sdmLayerNum);
                                    color.Black = (int)(inColor.Black - i * (inColor.Black - outColor.Black) * 1.0 / _sdmLayerNum);

                                    var rep = rpc.GetRepresentation(newFe, mctx);
                                    OverrideColorValueSet(rep, color);

                                    GC.Collect();
                                }
                            }

                            foreach (var geo in inBouaGeoList)
                            {
                                Dictionary<int, IGeometry> bufferGeoList = new Dictionary<int, IGeometry>();
                                bufferGeoList[-1] = geo;
                                for (int i = 0; i < _sdmLayerNum; i++)
                                {

                                    IGeometry bufferGeo = CreateBufferGeo(geo, -(i + 1) * widthPerLayer);
                                    if (bufferGeo == null)
                                    {
                                        MessageBox.Show("色带面缓冲出错！");
                                        return;
                                    }
                                    bufferGeoList[i] = bufferGeo;
                                    GC.Collect();
                                }

                                for (int i = 0; i < _sdmLayerNum; i++)
                                {
                                    string type = "内层";
                                    if (_sdmLayerNum - 1 == i)
                                    {
                                        type = "外层";
                                    }
                                    IFeature newFe = SDMfcl.CreateFeature();
                                    newFe.set_Value(SDMfcl.FindField("TYPE"), type);
                                    newFe.set_Value(SDMfcl.FindField("RuleID"), 1);
                                    if (widthPerLayer > 0)
                                    {
                                        newFe.Shape = (bufferGeoList[i - 1] as ITopologicalOperator).Difference(bufferGeoList[i]);
                                    }
                                    else
                                    {
                                        newFe.Shape = (bufferGeoList[i] as ITopologicalOperator).Difference(bufferGeoList[i - 1]);
                                    }
                                    newFe.Store();


                                    CmykColorClass color = new CmykColorClass();
                                    color.Cyan = (int)(inColor.Cyan - i * (inColor.Cyan - outColor.Cyan) * 1.0 / _sdmLayerNum);
                                    color.Magenta = (int)(inColor.Magenta - i * (inColor.Magenta - outColor.Magenta) * 1.0 / _sdmLayerNum);
                                    color.Yellow = (int)(inColor.Yellow - i * (inColor.Yellow - outColor.Yellow) * 1.0 / _sdmLayerNum);
                                    color.Black = (int)(inColor.Black - i * (inColor.Black - outColor.Black) * 1.0 / _sdmLayerNum);

                                    var rep = rpc.GetRepresentation(newFe, mctx);
                                    OverrideColorValueSet(rep, color);

                                    GC.Collect();
                                }
                            }

                            #endregion
                        }
                        #endregion

                        var ext = geometry.Envelope;
                        ext.Expand(_sdmTotalWidth, _sdmTotalWidth, false);
                        m_Application.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeography, lyr, ext);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    GC.Collect();
                    wo.Dispose();
                }
            
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1 || !smdFlag)
            {
                return;
            }

            var view = m_Application.ActiveView;
            IMap map = view.FocusMap;
            var editor = m_Application.EngineEditor;

            //拉框范围
            IRubberBand pRubberBand = new RubberRectangularPolygonClass();
            var geo = pRubberBand.TrackNew(view.ScreenDisplay, null);
            if (geo == null || geo.IsEmpty)
            {
                geo = this.ToSnapedMapPoint(x, y);
            }
            if (geo == null || geo.IsEmpty)
            {
                return;
            }


            editor.StartOperation();

            CreateSDM(geo, _layerName);
            editor.StopOperation("交互创建色带面");
            view.Refresh();
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            switch (keyCode)
            {
                case 32:
                    var frm = new SDMCreateFrom();
                    frm.StartPosition = FormStartPosition.CenterParent;
                    if (frm.ShowDialog() != DialogResult.OK)
                        return;

                    _layerName = frm.LayerName;
                    _sdmColors = frm.SDMColors;
                    _sdmTotalWidth = frm.SDMTotalWidth * m_Application.ActiveView.FocusMap.ReferenceScale * 1e-3;
                    _sdmLayerNum = frm.SDMLayerNum;

                    break;
                default:
                    break;
            }
        }


        private IGeometry CreateBufferGeo(IGeometry polygon, double dis)
        {
            IGeometry buffer = null;
            try
            {
                buffer = (polygon as ITopologicalOperator).Buffer(dis);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
            return buffer;
        }
        //根据几何特效来创建
        private IGeometry CreateBufferEffect(IGeometry polygon, double dis)
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
        private void ClearLayer()
        {
            IFeature fe;
            IFeatureCursor cursor = SDMfcl.Update(null, false);
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
            ILayer sdmlayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("SDM"))).FirstOrDefault();
            SDMfcl = (sdmlayer as IFeatureLayer).FeatureClass;
            //修改sdm图层 rule的颜色

            var rp = (sdmlayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
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
        private void OverrideColorValueSet(IRepresentation rep, IColor color)
        {
            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);

            for (int k = 0; k < ruleOrg.LayerCount; k++)
            {
                IBasicFillSymbol fillSym = ruleOrg.get_Layer(k) as IBasicFillSymbol;
                if (fillSym != null)
                {
                    IFillPattern fillPattern = fillSym.FillPattern;
                    IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                    rep.set_Value(fillAttrs, 0, color);

                }
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
    }
}
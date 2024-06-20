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
using System.Data;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
 
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class SDMColorCmd : SMGI.Common.SMGICommand
    {
        Dictionary<string, int> ColorRules = new Dictionary<string,int>();//色带颜色->ruleID；
        IFeatureClass SDMfcl = null;
        public SDMColorCmd()
        {
             
            m_caption = "色带普色";
            m_category = "色带普色";
            m_toolTip = "色带普色";
        }

        public override bool Enabled
        {
            get
            {

                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        public override void OnClick()
        {
            if (m_Application.ActiveView.FocusMap.ReferenceScale == 0)
            {
                MessageBox.Show("请设置地图参考比例尺！");
                return;
            }

            SDMCreateFrom frm = new SDMCreateFrom();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            Dictionary<string, ICmykColor> colors = frm.SDMColors;
            double totalWidth = frm.SDMTotalWidth * m_Application.ActiveView.FocusMap.ReferenceScale * 1e-3;
            int numSDMLayer = frm.SDMLayerNum;

            IFeature fe = SDMColorSet(colors);
            if (fe == null)
                return;

            WaitOperation wo = m_Application.SetBusy();
            try
            {
                wo.SetText("正在获取裁切面....");
                CreateSDMColors(numSDMLayer, totalWidth, colors);
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


        private void CreateSDMColors(int numSDMLayer, double totalWidth, Dictionary<string, ICmykColor> colors)
        {
            double widthPerLayer = totalWidth / numSDMLayer;
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "ClipBoundary");
            })).FirstOrDefault() as IFeatureLayer;
            IFeature f;
            IFeatureCursor cursor = lyr.FeatureClass.Search(new QueryFilterClass { WhereClause = "TYPE = '裁切面'" }, false);
            f = cursor.NextFeature();
            Marshal.ReleaseComObject(cursor);
            if (f == null)
            {
                MessageBox.Show("不存在裁切面要素！");
                return;
            }

            List<IPolygon> outBouaGeoList = new List<IPolygon>();
            List<IPolygon> inBouaGeoList = new List<IPolygon>();

            var geometry = f.ShapeCopy;
            (geometry as ITopologicalOperator).Simplify();
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

            ClearLayer();

            if (numSDMLayer == 2)
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
                    feinner.set_Value(SDMfcl.FindField("RuleID"), ColorRules[type]);
                    if (widthPerLayer > 0)
                    {
                        feinner.Shape = (outgeo as ITopologicalOperator).Difference(geo);
                    }
                    else
                    {
                        feinner.Shape = (geo as ITopologicalOperator).Difference(outgeo);
                    }
                    feinner.Store();

                    type = "内层";
                    feinner = SDMfcl.CreateFeature();
                    feinner.set_Value(SDMfcl.FindField("TYPE"), type);
                    feinner.set_Value(SDMfcl.FindField("RuleID"), ColorRules[type]);
                    if (widthPerLayer > 0)
                    {
                        feinner.Shape = (ingeo as ITopologicalOperator).Difference(geo);
                    }
                    else
                    {
                        feinner.Shape = (geo as ITopologicalOperator).Difference(ingeo);
                    }
                    feinner.Store();


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
                    feinner.set_Value(SDMfcl.FindField("RuleID"), ColorRules[type]);
                    if (widthPerLayer > 0)
                    {
                        feinner.Shape = (geo as ITopologicalOperator).Difference(outgeo);
                    }
                    else
                    {
                        feinner.Shape = (outgeo as ITopologicalOperator).Difference(geo);
                    }
                    feinner.Store();

                    type = "内层";
                    feinner = SDMfcl.CreateFeature();
                    feinner.set_Value(SDMfcl.FindField("TYPE"), type);
                    feinner.set_Value(SDMfcl.FindField("RuleID"), ColorRules[type]);
                    if (widthPerLayer > 0)
                    {
                        feinner.Shape = (geo as ITopologicalOperator).Difference(ingeo);
                    }
                    else
                    {
                        feinner.Shape = (ingeo as ITopologicalOperator).Difference(geo);
                    }
                    feinner.Store();

                    Marshal.ReleaseComObject(feinner);
                }
                #endregion
            }
            else//色带面层数大于2
            {
                IMapContext mctx = new MapContextClass();
                mctx.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);

                IRepresentationRenderer repRender = (lyr as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                IRepresentationClass rpc = repRender.RepresentationClass;

                #region 渐变色带面
                ICmykColor outColor = colors["外层"];
                ICmykColor inColor = colors["内层"];
                foreach (var geo in outBouaGeoList)
                {
                    Dictionary<int, IGeometry> bufferGeoList = new Dictionary<int, IGeometry>();
                    bufferGeoList[-1] = geo;
                    for (int i = 0; i < numSDMLayer; i++)
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

                    for (int i = 0; i < numSDMLayer; i++)
                    {
                        string type = "内层";
                        if (numSDMLayer - 1 == i)
                        {
                            type = "外层";
                        }

                        IFeature newFe = SDMfcl.CreateFeature();
                        newFe.set_Value(SDMfcl.FindField("TYPE"), type);
                        newFe.set_Value(SDMfcl.FindField("RuleID"), ColorRules[type]);
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
                        color.Cyan = (int)(inColor.Cyan - i * (inColor.Cyan - outColor.Cyan) * 1.0 / numSDMLayer);
                        color.Magenta = (int)(inColor.Magenta - i * (inColor.Magenta - outColor.Magenta) * 1.0 / numSDMLayer);
                        color.Yellow = (int)(inColor.Yellow - i * (inColor.Yellow - outColor.Yellow) * 1.0 / numSDMLayer);
                        color.Black = (int)(inColor.Black - i * (inColor.Black - outColor.Black) * 1.0 / numSDMLayer);

                        var rep = rpc.GetRepresentation(newFe, mctx);
                        OverrideColorValueSet(rep, color);

                        GC.Collect();
                    }
                }

                foreach (var geo in inBouaGeoList)
                {
                    Dictionary<int, IGeometry> bufferGeoList = new Dictionary<int, IGeometry>();
                    bufferGeoList[-1] = geo;
                    for (int i = 0; i < numSDMLayer; i++)
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

                    for (int i = 0; i < numSDMLayer; i++)
                    {
                        string type = "内层";
                        if (numSDMLayer - 1 == i)
                        {
                            type = "外层";
                        }
                        IFeature newFe = SDMfcl.CreateFeature();
                        newFe.set_Value(SDMfcl.FindField("TYPE"), type);
                        newFe.set_Value(SDMfcl.FindField("RuleID"), ColorRules[type]);
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
                        color.Cyan = (int)(inColor.Cyan - i * (inColor.Cyan - outColor.Cyan) * 1.0 / numSDMLayer);
                        color.Magenta = (int)(inColor.Magenta - i * (inColor.Magenta - outColor.Magenta) * 1.0 / numSDMLayer);
                        color.Yellow = (int)(inColor.Yellow - i * (inColor.Yellow - outColor.Yellow) * 1.0 / numSDMLayer);
                        color.Black = (int)(inColor.Black - i * (inColor.Black - outColor.Black) * 1.0 / numSDMLayer);

                        var rep = rpc.GetRepresentation(newFe, mctx);
                        OverrideColorValueSet(rep, color);

                        GC.Collect();
                    }
                }

                #endregion
            }


            var ext = geometry.Envelope;
            ext.Expand(totalWidth, totalWidth, false);
            m_Application.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeography, lyr, ext);
        }

        //暂不支持渐变色带面(老版)
        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                double totoal = double.Parse(args.Element("SdmSum").Value) * m_Application.ActiveView.FocusMap.ReferenceScale * 1e-3;
                int num = int.Parse(args.Element("SmdNum").Value);
                Dictionary<string, ICmykColor> colors = new Dictionary<string, ICmykColor>();
                colors["外层"] = new CmykColorClass { CMYK = int.Parse(args.Element("ColorOut").Value) } as ICmykColor;
                colors["内层"] = new CmykColorClass { CMYK = int.Parse(args.Element("ColorIn").Value) } as ICmykColor;
                IFeature fe = SDMColorSet(colors);
                if (fe == null)
                    return false;

                CreateSDMColors(num, totoal, colors);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
            finally
            {
                GC.Collect(); 
            }

            return true;
        }
        /// <summary>
        /// CMYK字符串得到CMYK颜色值
        /// </summary>
        /// <param name="cmyk">cmyk字符串（形如：C100M200Y100K50）</param>
        /// <returns>CMYK颜色值</returns>
        private IColor GetColorByString(string cmyk)
        {
            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;
            try
            {
                for (int i = 0; i <= cmyk.Length; i++)
                {
                    if (i == cmyk.Length)
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('C'))
                        {
                            CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('M'))
                        {
                            CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('Y'))
                        {
                            CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('K'))
                        {
                            CMYK_Color.Black = int.Parse(sbs.Substring(1));
                        }
                        break;
                    }
                    else
                    {
                        char C = cmyk[i];
                        if (D.Contains(C))
                        {
                            sb.Append(C);
                        }
                        else
                        {
                            string sbs = sb.ToString();
                            if (sbs.Contains('C'))
                            {
                                CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('M'))
                            {
                                CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('Y'))
                            {
                                CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('K'))
                            {
                                CMYK_Color.Black = int.Parse(sbs.Substring(1));
                            }
                            sb.Clear();
                            sb.Append(C);
                        }
                    }
                }
                return CMYK_Color;
            }
            catch
            {
                return null;
            }

        }
        
        
        private IGeometry CreateBufferGeo(IGeometry polygon,double dis)
        {
            IGeometry buffer = null;
            try
            {
                //IPolyline boundary = (polygon as ITopologicalOperator).Boundary as IPolyline;
                buffer = (polygon as ITopologicalOperator).Buffer(dis);
            }
            catch(Exception ex)
            {
                buffer = CreateBufferEffect(polygon, dis);
            }
            return buffer;
        }
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
        //生成内存图层
        public IFeatureClass CreatePolygonMemoryLayer(ISpatialReference sp)
        {
            //设置字段集
            IFields fields = new FieldsClass();
            var fieldsEdit = (IFieldsEdit)fields;
            IField field = new FieldClass();
            var fieldEdit = (IFieldEdit)field;

            //创建主键
            fieldEdit.Name_2 = "FID";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(field);

            //创建图形字段
            IGeometryDef geometryDef = new GeometryDefClass();
            var geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            geometryDefEdit.SpatialReference_2 = sp;

            field = new FieldClass();
            fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "Shape";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            fieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(field);

            IWorkspaceFactory wf = new InMemoryWorkspaceFactoryClass();
            var wn = wf.Create("", "MemoryWorkspace", null, 0);
            var na = (IName)wn;
            var fw = (IFeatureWorkspace)(na.Open()); //打开内存空间
            var featureClass = fw.CreateFeatureClass("MW_LineConsPolygon", fields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            return featureClass;
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
            IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
            IEnumDatasetName enumDatasetName = pRepWksExt.get_FeatureClassRepresentationNames(SDMfcl);
            enumDatasetName.Reset();
            IDatasetName pDatasetName = enumDatasetName.Next();
            IRepresentationClass g_RepClass = pRepWksExt.OpenRepresentationClass(pDatasetName.Name);
            //修改sdm图层 rule的颜色
            IRepresentationRules rules = g_RepClass.RepresentationRules;
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

                    for (int k = 0; k < rule.LayerCount; k++)
                    {
                        IBasicFillSymbol fillSym = rule.get_Layer(k) as IBasicFillSymbol;
                        if (fillSym != null)
                        {
                            IFillPattern fillPattern = fillSym.FillPattern;
                            IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                            fillAttrs.set_Value(0, colors[rulename]);
                            fillSym.FillPattern = fillPattern;
                        }
                    }

                    ColorRules[rulename] = ruleID;

                }
            }
            g_RepClass.RepresentationRules = rules;
            return fe;
        }

        private ESRI.ArcGIS.Geodatabase.IRepresentationWorkspaceExtension RepresentationWorkspaceExtensionClass()
        {
            throw new NotImplementedException();
        }
        private IRepresentationWorkspaceExtension GetRepersentationWorkspace(IWorkspace workspace)
        {
            IWorkspaceExtensionManager wem = workspace as IWorkspaceExtensionManager;
            UID uid = new UIDClass();
            uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            IRepresentationWorkspaceExtension rwe = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
            return rwe;
        }

        //覆盖颜色属性制图表达
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

       }
}

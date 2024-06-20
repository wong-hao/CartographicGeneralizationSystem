using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class JustifyCulvertCmd : SMGI.Common.SMGICommand
    {
        
        public string relatedLayerName;
        public string justifyFeatureName;
        public string justifyLayerName;
        public double tolerance;
        ILayer layerJustify;
        ILayer layerRelated;
        Dictionary<string, int> Name2Size = new Dictionary<string, int>();      //不同类型要素的符号几何图形中空白的尺寸
        public JustifyCulvertCmd()
        {
            m_caption = "涵洞符号调整";
            m_category = "涵洞符号调整";
            m_toolTip = "涵洞符号调整";

            Name2Size.Add("涵洞", 3);
            Name2Size.Add("输水渡槽", 2);
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        
        public override void OnClick()
        {
            //涵洞
           
            var lyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "HFCP"))).FirstOrDefault();
            if (lyr == null)
            {
                MessageBox.Show("不存在涵洞要素！");
                return;
            }


            JustifyCulvertForm frm = new JustifyCulvertForm();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                relatedLayerName = frm.relatedLayerName;
                justifyFeatureName = frm.justifyFeatureName;
                justifyLayerName = frm.justifyLayerName;
                tolerance = frm.tolerance;
                using (var wo = m_Application.SetBusy())
                {
                    wo.SetText(string.Format("调整{0}符号大小...", justifyFeatureName));
                    m_Application.EngineEditor.StartOperation();
                    Process(relatedLayerName, justifyLayerName, justifyFeatureName);
                    m_Application.EngineEditor.StopOperation("涵洞尺寸调整！");
                }
                MessageBox.Show("符号大小调整完成!", "符号调整", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                GC.Collect();
            }
        }

        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                messageRaisedAction("涵洞尺寸开始调整....");
                JustifyCulvertForm frm = new JustifyCulvertForm();
                {
                    relatedLayerName = frm.relatedLayerName;
                    justifyFeatureName = frm.justifyFeatureName;
                    justifyLayerName = frm.justifyLayerName;
                    tolerance = frm.tolerance;
                    Process(relatedLayerName, justifyLayerName, justifyFeatureName);
                    GC.Collect();
                }
                m_Application.Workspace.Save();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
        }
       
        /// <summary>
        /// 调整符号大小
        /// </summary>
        /// <param name="relatedLyrName">关联图层名</param>
        /// <param name="justifyLyrName">调整图层名</param>
        /// <param name="justifyFeatureName">调整要素名</param>
        public void Process(string relatedLyrName, string justifyLyrName, string justifyFeatureName)
        {
            layerJustify = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == justifyLyrName))).FirstOrDefault();
            layerRelated = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == relatedLyrName.Substring(0, 4)))).FirstOrDefault();
            IFeatureClass fclJustify = (layerJustify as IFeatureLayer).FeatureClass;
            IFeatureClass fclRelated = (layerRelated as IFeatureLayer).FeatureClass;
            //获取调整要素的全部Rule
            IRepresentationRenderer repRender = (layerJustify as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass repClass = repRender.RepresentationClass;
            IRepresentationRules repRules = repClass.RepresentationRules;
            repRules.Reset();
            int ruleID = -1, objRuleID = -1;
            IRepresentationRule repRule = null;
            repRules.Next(out ruleID, out repRule);
            while (repRule != null)
            {
                string ruleName = repRules.get_Name(ruleID);
                if (ruleName.Contains(justifyFeatureName))
                {
                    objRuleID = ruleID;
                    break;
                }
                repRules.Next(out ruleID, out repRule);
            }
            if (objRuleID == -1)
                return;

            IMapContext mapContext_Related = new MapContextClass();
            mapContext_Related.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, (layerRelated as IGeoFeatureLayer).AreaOfInterest);
            IMapContext mapContext = new MapContextClass();
            mapContext.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, (layerJustify as IGeoFeatureLayer).AreaOfInterest);
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = string.Format("RuleID ={0}", objRuleID);            //筛选条件
            IFeatureCursor featureCursor = fclJustify.Update(qf, false);
            IFeature fe;
            double dis = tolerance * m_Application.ActiveView.FocusMap.ReferenceScale * 0.001;
            while ((fe = featureCursor.NextFeature()) != null)
            {
                
                IPoint Geo = fe.ShapeCopy as IPoint;//0.1容差
                IEnvelope envelope = new EnvelopeClass();
                envelope.PutCoords(Geo.X - dis, Geo.Y - dis, Geo.X + dis, Geo.Y + dis);
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = envelope;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                sf.WhereClause = "RuleID <> 1";//排除不显示要素
                IFeatureCursor relataedFeCursor = fclRelated.Search(sf as IQueryFilter, true);
                IFeature relatedFe = relataedFeCursor.NextFeature();
                if (relatedFe == null)
                {
                    continue;                           //没有相交的要素不用调整
                }
               
                //获取关联图层符号化后的最大宽度
                double relatedWidth = GetWidth(relatedFe, mapContext_Related);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(relataedFeCursor);
                if (relatedWidth == 0)
                {
                    continue;
                }
                //制图表达覆盖
                OverrideMarkerSizeValue0(featureCursor, fe, relatedWidth, mapContext);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fe);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(Geo);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(envelope);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(sf);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(relatedFe);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapContext_Related);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapContext);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
            m_Application.ActiveView.Refresh();
        }

        /// <summary>
        /// 获取宽度
        /// </summary>
        /// <param name="relatedFeature">关联要素</param>
        /// <param name="mapContext"></param>
        /// <returns></returns>
        public double GetWidth(IFeature relatedFeature, IMapContext mapContext)
        {
            int ruleID = int.Parse(relatedFeature.get_Value(relatedFeature.Fields.FindField("ruleID")).ToString());
            IRepresentationRenderer repRender_Related = (layerRelated as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass repClass_Related = repRender_Related.RepresentationClass;
            IRepresentationRules repRules_Related = repClass_Related.RepresentationRules;
           //  IRepresentation repFeature_Related = repClass_Related.GetRepresentation(relatedFeature, mapContext);
            IRepresentationRule rule_Related = repRules_Related.get_Rule(ruleID);
            double Width = 0;
            if (rule_Related != null)
            {
                for (int i = 0; i < rule_Related.LayerCount; i++)
                {
                    if (rule_Related.get_Layer(i) is IBasicLineSymbol)
                    {
                        IBasicLineSymbol basicLineSymbol = rule_Related.get_Layer(i) as IBasicLineSymbol;
                        if (basicLineSymbol == null)
                        {
                            continue;
                        }
                        IGraphicAttributes LineAttributes = basicLineSymbol.Stroke as IGraphicAttributes;
                        int id = LineAttributes.get_IDByName("Width");
                        double lineWidth = (double)LineAttributes.get_Value(id) / 2.8345;            //磅转毫米
                        if (lineWidth > Width)
                        {
                            Width = lineWidth;
                        }
                    }
                    else if (rule_Related.get_Layer(i) is IBasicFillSymbol)
                    {
                        IBasicFillSymbol basicFillSymbol = rule_Related.get_Layer(i) as IBasicFillSymbol;
                        if (basicFillSymbol == null)
                        {
                            continue;
                        }
                        IGraphicAttributes FillAttributes = basicFillSymbol.FillPattern as IGraphicAttributes;
                        int id_From = FillAttributes.get_IDByName("From width");
                        int id_To = FillAttributes.get_IDByName("To width");
                        double fromWidth = (double)FillAttributes.get_Value(id_From) / 2.8345;
                        double toWidth = (double)FillAttributes.get_Value(id_To) / 2.8345;
                        double averageWidth = (fromWidth + toWidth) / 2;
                        if (averageWidth > Width)
                        {
                            Width = averageWidth;
                        }
                    }
                }
            }
            return Width;
        }

        /// <summary>
        /// 覆盖MarkerSize值:整体缩放
        /// </summary>
        /// <param name="Cursor">调整要素游标</param>
        /// <param name="feature">调整要素</param>
        /// <param name="Width">关联线型宽度</param>
        /// <param name="mapContext"></param>
        public void OverrideMarkerSizeValue(IFeatureCursor Cursor, IFeature feature, double Width, IMapContext mapContext)
        {
            IRepresentationRenderer repRender_Justify = (layerJustify as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass repClass_Justify = repRender_Justify.RepresentationClass;
            IRepresentationRules repRules_Justify = repClass_Justify.RepresentationRules;
         
            IRepresentation repFeature_Justify = repClass_Justify.GetRepresentation(feature, mapContext);
            IRepresentationRule rule_Justify = repRules_Justify.get_Rule(repFeature_Justify.RuleID);
            IBasicMarkerSymbol basicMarkerSymbol = rule_Justify.get_Layer(0) as IBasicMarkerSymbol;
            IGraphicAttributes markerAttributes = basicMarkerSymbol as IGraphicAttributes;
            int id = markerAttributes.get_IDByName("Size");
            double markerSize = (double)markerAttributes.get_Value(id) / 2.8345;         //磅转毫米
            if (Math.Round(Math.Abs(Width - markerSize / Name2Size[justifyFeatureName]), 2) > tolerance)
            {
                markerSize = Width * Name2Size[justifyFeatureName];
                repFeature_Justify.set_Value(markerAttributes, id, markerSize * 2.8345);
                repFeature_Justify.RepresentationClass.RepresentationRules.set_Rule(repFeature_Justify.RuleID, rule_Justify);
                repFeature_Justify.UpdateFeature();
                Cursor.UpdateFeature(feature);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(repFeature_Justify);
        }
        /// <summary>
        /// 覆盖MarkerSize值:打散几何覆盖方式
        /// </summary>
        /// <param name="Cursor"></param>
        /// <param name="feature"></param>
        /// <param name="Width"></param>
        /// <param name="mapContext"></param>
        public void OverrideMarkerSizeValue0(IFeatureCursor Cursor, IFeature feature, double Width, IMapContext mapContext)
        {
             
            IRepresentationRenderer repRender_Justify = (layerJustify as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass repClass_Justify = repRender_Justify.RepresentationClass;
            IRepresentationRules repRules_Justify = repClass_Justify.RepresentationRules;
            IRepresentation repFeature_Justify = repClass_Justify.GetRepresentation(feature, mapContext);
            IRepresentationRule rule = repRules_Justify.get_Rule(repFeature_Justify.RuleID);

            IBasicMarkerSymbol basicMarkerSymbol = rule.get_Layer(0) as IBasicMarkerSymbol;
            IGraphicAttributes markerAttributes = basicMarkerSymbol as IGraphicAttributes;
            int id = markerAttributes.get_IDByName("Size");
           
            double markerSize = (double)markerAttributes.get_Value(id) / 2.8345;         //磅转毫米
            if (Math.Round(Math.Abs(Width - markerSize / Name2Size[justifyFeatureName]), 2) > tolerance)
            {
                double adjwidth = Width-0.5;//道路 缝隙宽度 

                IRepresentationGraphics justGraphics = new RepresentationMarkerClass();
                IGraphicAttributes attrs = rule.get_Layer(0) as IGraphicAttributes;
                IRepresentationGraphics graphics = attrs.get_Value(1) as IRepresentationGraphics;
                graphics.Reset();
                try
                {
                    #region
                    while (true)
                    {
                        IRepresentationRule r;
                        IGeometry geo;
                        graphics.Next(out geo, out r);
                        if (geo == null || r == null)
                            break;
                        if (geo.GeometryType == esriGeometryType.esriGeometryPolygon)
                        {
                            IRepresentationRule rclone = (r as IClone).Clone() as IRepresentationRule;

                            IGeometry geoClone = (geo as IClone).Clone() as IGeometry;
                            IPoint orgin = new PointClass() { X = 0, Y = 0 };
                            IGeometryCollection gc = geoClone as IGeometryCollection;
                            for (int i = 0; i < gc.GeometryCount; i++)
                            {
                                IGeometry pgeo = gc.get_Geometry(i);
                                PolygonClass polygon = new PolygonClass();
                                IEnvelope env = pgeo.Envelope;
                                int re = 1;
                                if (env.YMin < 0)
                                {
                                    re = -1;
                                }
                                polygon.AddGeometry(pgeo);
                                IPoint center = polygon.Centroid;
                     
                                ITransform2D trans = polygon as ITransform2D;
                                trans.Scale(center, 0.6/env.Width, 0.52/env.Height);//缩放到原来尺度
                               
                                if (re == 1)//箭头平移到原点
                                {
                                    trans.Move(0 - center.X, 0 - (center.Y - polygon.Envelope.Height / 2));
                                }
                                else
                                {
                                    trans.Move(0 - center.X, 0 - (center.Y + polygon.Envelope.Height / 2));
                                }
                                //平移到合适位置
                                trans.Move(0, re * Width/2);
                                justGraphics.Add(polygon, rclone);
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(polygon);
                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(rclone);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(geoClone);


                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show("尺寸修改错误：" + ex.Message);
                }
                markerSize = Width * Name2Size[justifyFeatureName];
                repFeature_Justify.set_Value(markerAttributes, 1, justGraphics);//设置图元
                repFeature_Justify.set_Value(markerAttributes, 2, (1+Width) * 2.8345);//设置尺寸
                repFeature_Justify.RepresentationClass.RepresentationRules.set_Rule(repFeature_Justify.RuleID, rule);
                repFeature_Justify.UpdateFeature();
                Cursor.UpdateFeature(feature);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(justGraphics);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(repFeature_Justify);
        }
    }
}

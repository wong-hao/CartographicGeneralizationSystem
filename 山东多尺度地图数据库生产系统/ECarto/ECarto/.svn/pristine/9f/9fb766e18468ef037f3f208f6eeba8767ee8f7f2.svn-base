using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using System.Xml.Linq;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;
using SMGI.Common;

namespace SMGI.Plugin.AnnotationEngine
{
    public class AnnotationManualCreateTool : SMGI.Common.SMGITool
    {
        #region 成员
        /// <summary>
        /// 注记规则表
        /// </summary>
        private DataTable _ruleTable;

        /// <summary>
        /// 字体映射表
        /// </summary>
        private DataTable _fontmappingTable;

        /// <summary>
        /// 特殊字符表
        /// </summary>
        private static DataTable _specialCharacterTable;

        /// <summary>
        /// 目标注记要素类
        /// </summary>
        private Dictionary<string, IFeatureClass> _annoName2FeatureClass;

        #endregion

        public AnnotationManualCreateTool()
        {
            m_caption = "创建注记(交互)";

            _ruleTable = null;
            _fontmappingTable = null;
            _specialCharacterTable = null;
            _annoName2FeatureClass = null;
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
            if (m_Application.ActiveView.FocusMap.ReferenceScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;
            }

            if (_annoName2FeatureClass == null)
            {
                XElement contentXEle = m_Application.Template.Content;
                XElement annoRuleEle = contentXEle.Element("AnnoFull");
                string annoRuleFilePath = m_Application.Template.Root + "\\" + annoRuleEle.Value;
                if (!File.Exists(annoRuleFilePath))
                {
                    MessageBox.Show(string.Format("未找到注记规则库【{0}】!", annoRuleFilePath), "警告", MessageBoxButtons.OK);
                    return;
                }

                //读取规则表
                _ruleTable = CommonMethods.ReadToDataTable(annoRuleFilePath, "注记规则");
                _fontmappingTable = CommonMethods.ReadToDataTable(annoRuleFilePath, "字体映射");
                _specialCharacterTable = CommonMethods.ReadToDataTable(annoRuleFilePath, "特殊字符");

                //获取注记要素类
                Dictionary<string, IFeatureClass> annoName2FeatureClass = new Dictionary<string, IFeatureClass>();
                System.Data.DataTable dtAnnoLayers = _ruleTable.AsDataView().ToTable(true, new string[] { "注记要素类名" });//distinct
                for (int i = 0; i < dtAnnoLayers.Rows.Count; ++i)
                {
                    //图层名
                    string annoFCName = dtAnnoLayers.Rows[i]["注记要素类名"].ToString().ToUpper().Trim();
                    if (!(m_Application.Workspace.EsriWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, annoFCName))
                    {
                        MessageBox.Show(string.Format("当前数据库缺少注记要素类【{0}】!", annoFCName), "警告", MessageBoxButtons.OK);
                        return;
                    }

                    IFeatureClass annoFeatureClass = (m_Application.Workspace.EsriWorkspace as IFeatureWorkspace).OpenFeatureClass(annoFCName);

                    annoName2FeatureClass.Add(annoFCName, annoFeatureClass);
                }
                _annoName2FeatureClass = annoName2FeatureClass;


                //将环境选择元素置为红色
                ISelectionEnvironment selEnvironment = new SelectionEnvironmentClass();
                IRgbColor selColor = new RgbColor();
                selColor.Red = 255;
                selEnvironment.DefaultColor = selColor;
            }
        }


        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;

            if (_annoName2FeatureClass == null || _annoName2FeatureClass.Count == 0)
                return;

            List<string> msg = new List<string>();

            //选择
            IScreenDisplay screenDisplay = m_Application.ActiveView.ScreenDisplay;
            IRubberBand rubberBand = new ESRI.ArcGIS.Display.RubberRectangularPolygonClass();
            ESRI.ArcGIS.Geometry.IGeometry selectGeometry = rubberBand.TrackNew(screenDisplay, null);
            bool justOne = false;
            if (selectGeometry.IsEmpty)
            { 
                //点查询
                IPoint pPoint = ToSnapedMapPoint(x, y);
                selectGeometry = (pPoint as ITopologicalOperator).Buffer(2);
                justOne = true;
            }

            //获取别选择的要素
            m_Application.MapControl.Map.ClearSelection();
            m_Application.MapControl.Map.SelectByShape(selectGeometry, null, justOne);
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, m_Application.ActiveView.Extent);

            List<IFeature> selFeList = new List<IFeature>();

            ISelection selection = m_Application.MapControl.Map.FeatureSelection;
            IEnumFeature mapEnumFeature = selection as IEnumFeature;
            mapEnumFeature.Reset();
            IFeature fe = null;
            while ((fe = mapEnumFeature.Next()) != null)
            {
                selFeList.Add(fe);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mapEnumFeature);
            if (selFeList.Count == 0)
                return;


            m_Application.EngineEditor.StartOperation();

            try
            {
                List<IFeature> newAnnoFeList = new List<IFeature>();//新创建的注记要素
                using (var wo = m_Application.SetBusy())
                {
                    foreach (var feature in selFeList)
                    {
                        IFeatureLayer lyr = m_Application.Workspace.LayerManager.GetLayer(l =>
                        {
                            return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).Visible && (l as IFeatureLayer).Selectable
                                && (l as IGeoFeatureLayer).FeatureClass.AliasName == feature.Class.AliasName
                                && ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;
                        }).FirstOrDefault() as IFeatureLayer;
                        if (lyr == null)
                            continue;

                        IFeatureClass fc = lyr.FeatureClass;
                        if (fc == null)
                            continue;

                        string fcName = feature.Class.AliasName.ToUpper();
                        if (_annoName2FeatureClass.ContainsKey(fcName))
                            continue;//跳过注记要素

                        #region 生成注记
                        wo.SetText(string.Format("正在生成要素类【{0}】中要素【{1}】的注记......", feature.Class.AliasName, feature.OID));

                        //要素是否存在相应的注记规则
                        List<DataRow> drArray = new List<DataRow>();
                        DataRow[] drs = _ruleTable.Select().Where(i => i["要素类名"].ToString().ToUpper() == fcName).ToArray();
                        for (int i = 0; i < drs.Length; i++)
                        {
                            string condition = drs[i]["查询条件"].ToString();
                            if (FeatureNeedCreateAnnotiaon(lyr, feature, condition))
                            {
                                drArray.Add(drs[i]);
                            }
                        }
                        if (drArray.Count == 0)
                            continue;

                        //设置注记定位点
                        IPoint annoPositionPoint = new PointClass();
                        double rotateAngle = 0;
                        if (justOne)
                        {
                            //点击选择
                            annoPositionPoint = new PointClass()
                            {
                                X = (selectGeometry.Envelope.XMax + selectGeometry.Envelope.XMin) / 2,
                                Y = (selectGeometry.Envelope.YMax + selectGeometry.Envelope.YMin) / 2
                            };

                            //如果是线要素，则获取创建注记的旋转角度
                            if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
                            {
                                IPolyline polyline = feature.Shape as IPolyline;

                                //通过QueryPointAndDistance方法获取离elementPoint最近的pPolyline上的点
                                double distanceAlongCurve = 0;//
                                double distanceFromCurve = 0;
                                bool bRightSide = false;
                                IPoint sPoint = new PointClass();
                                polyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, annoPositionPoint, false, sPoint,
                                    ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);

                                ILine tangentLine = new Line();
                                polyline.QueryTangent(esriSegmentExtension.esriNoExtension, distanceAlongCurve, false, 0.001, tangentLine);
                                rotateAngle = AnnotationHelper.RadianNormalization(tangentLine.Angle);
                                
                                annoPositionPoint = sPoint;
                            }
                        }
                        else
                        {
                            //拉框选择
                            switch (fc.ShapeType)
                            {
                                case esriGeometryType.esriGeometryPoint:
                                    annoPositionPoint = feature.ShapeCopy as IPoint;
                                    break;
                                case esriGeometryType.esriGeometryPolyline:
                                    //获取外包框的中心点
                                    IPoint tempPoint = new PointClass();
                                    tempPoint.X = (selectGeometry.Envelope.XMax + selectGeometry.Envelope.XMin) / 2;
                                    tempPoint.Y = (selectGeometry.Envelope.YMax + selectGeometry.Envelope.YMin) / 2;

                                    //获取离中心点最近的Polyline上点作为注记定位点
                                    double distanceAlongCurve = 0;
                                    double distanceFromCurve = 0;
                                    bool bRightSide = false;
                                    IPoint sPoint = new PointClass();
                                    (feature.Shape as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, tempPoint, false, sPoint,
                                        ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);

                                    ILine tangentLine = new Line();
                                    (feature.Shape as IPolyline).QueryTangent(esriSegmentExtension.esriNoExtension, distanceAlongCurve, false, 0.001, tangentLine);
                                    rotateAngle = AnnotationHelper.RadianNormalization(tangentLine.Angle);

                                    annoPositionPoint = sPoint;
                                    break;
                                case esriGeometryType.esriGeometryPolygon:
                                    annoPositionPoint = (feature.Shape as IArea).LabelPoint;
                                    break;
                            }
                        }
                        
                        //生成注记
                        var feList = CreateAnnotation(_annoName2FeatureClass, lyr, feature, drArray, annoPositionPoint, _fontmappingTable, _specialCharacterTable, rotateAngle);
                        if (feList.Count > 0)
                            newAnnoFeList.AddRange(feList.ToArray());

                        #endregion
                    }

                }

                m_Application.EngineEditor.StopOperation("注记生成(交互)");


                //刷新
                m_Application.MapControl.Map.ClearSelection();
                if (newAnnoFeList.Count > 0)
                {
                    foreach (var annoFe in newAnnoFeList)
                    {
                        ILayer annoLayer = m_Application.Workspace.LayerManager.GetLayer(l =>
                            {
                                return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == annoFe.Class.AliasName
                                    && ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == m_Application.Workspace.EsriWorkspace.PathName;
                            }).FirstOrDefault();

                        m_Application.MapControl.Map.SelectFeature(annoLayer, annoFe);
                    }
                }
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, m_Application.ActiveView.Extent);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.ToString());

                m_Application.EngineEditor.AbortOperation();
            }
        }

        /// <summary>
        /// 要素是否需要生成注记
        /// </summary>
        /// <param name="lyr"></param>
        /// <param name="fe"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        private bool FeatureNeedCreateAnnotiaon(IFeatureLayer lyr, IFeature fe, string condition)
        {
            IFeatureClass fc = (lyr as IFeatureLayer).FeatureClass;

            condition = condition.Replace("[", "").Replace("]", "");//替换掉中括号，兼容mdb和gdb
            IQueryFilter qf = new QueryFilterClass();
            if (condition == "")
                qf.WhereClause = string.Format("OBJECTID = {0}", fe.OID);
            else
                qf.WhereClause = string.Format("(OBJECTID = {0}) and ({1})", fe.OID, condition);

            int feCount = fc.FeatureCount(qf);

            return feCount > 0;
        }


        /// <summary>
        /// 创建注记
        /// </summary>
        /// <param annoName2FeatureClass="lyr">注记图层</param>
        /// <param name="lyr">图层</param>
        /// <param name="feature">要素</param>
        /// <param name="drArray">满足条件的规则行</param>
        /// <param name="annoCenterPoint">要素注记中心位置点</param>
        /// <param name="fontMappingTable">字体映射规则表</param>
        /// <param name="specialCharacterTable">特殊字符规则表</param>
        /// <param name="rotateAngle">注记旋转角度（弧度）:[0, 2*PI]</param> 
        /// <param name="bOverrun">是否处理超限注记</param>
        /// <param name="overrunScale">超限比例</param>
        /// <returns>注记要素</returns>
        public static List<IFeature> CreateAnnotation(Dictionary<string, IFeatureClass> annoName2FeatureClass, IFeatureLayer lyr, IFeature feature, List<DataRow> drArray, IPoint annoCenterPoint, DataTable fontMappingTable, DataTable specialCharacterTable, double rotateAngle = 0, bool bOverrun = false, double overrunScale = 1.0)
        {
            List<IFeature> newFeList = new List<IFeature>();

            var annoEngine = GApplication.Application.MapControl.Map.AnnotationEngine;

            try
            {
                if (annoEngine.Name.ToUpper().Contains("MAPLEX"))
                {
                    IAnnotateMap am = new AnnotateMap();
                    GApplication.Application.MapControl.Map.AnnotationEngine = am;
                }

                Dictionary<AnnotationRule, KeyValuePair<IFeature, double>> rule2EleAndLen = new Dictionary<AnnotationRule, KeyValuePair<IFeature, double>>();
                double totalLen = 0;
                double AnnoInterval = 1.0;//注记之间的间隔(字符个数)
                #region 获取该要素各注记的长度
                for (int i = 0; i < drArray.Count; i++)
                {
                    DataRow dr = drArray[i];

                    //根据规则创建一个标注类
                    AnnotationRule annoRule = new AnnotationRule(dr["要素类名"].ToString(), dr["查询条件"].ToString(), dr["规则分类名"].ToString(),
                                dr["标注字段"].ToString(), dr["注记表达式"].ToString(), dr["注记要素类名"].ToString(), dr["注记字体"].ToString(), dr["注记大小"].ToString(),
                                dr["注记颜色"].ToString(), dr["注记样式"].ToString(), dr["晕圈大小"].ToString(), dr["晕圈颜色"].ToString(), dr["气泡形状"].ToString(),
                                dr["气泡颜色"].ToString(), dr["气泡边线颜色"].ToString(), dr["气泡边线宽度"].ToString(), dr["气泡边框距"].ToString(), dr["字符宽度"].ToString(),
                                dr["CJK字符方向"].ToString(), dr["放置类型"].ToString(), dr["注记位置"].ToString(), dr["注记偏移"].ToString(), dr["最大文字间距"].ToString(),
                                dr["最大字符间距"].ToString(), dr["移除同名标注"].ToString(), dr["重复标注"].ToString(), dr["标注缓冲大小"].ToString(), dr["标注最小要素大小"].ToString(),
                                dr["连接要素"].ToString(), dr["标注要素最大部分"].ToString(), dr["要素权重"].ToString(), dr["从不移除"].ToString(), dr["消隐类型"].ToString());


                    #region 字体映射
                    for (int j = 0; j < fontMappingTable.Rows.Count; j++)
                    {
                        DataRow row = fontMappingTable.Rows[j];
                        if (row["字体名"].ToString() == annoRule.FontName)
                        {
                            annoRule.FontName = row["目标字体名"].ToString();
                            break;
                        }
                    }
                    #endregion

                    #region 获取注记文本
                    string annoText = "";
                    if (annoRule.Expression.ToUpper().Contains("FUNCTION"))
                    {
                        #region 没走通
                        //IAnnotationExpressionEngine annoEngine = new AnnotationVBScriptEngineClass();
                        //IAnnotationExpressionParser annoExpParser = annoEngine.SetCode(annoRule.Expression, "");
                        //annoText = annoExpParser.FindLabel(feature);
                        #endregion

                        List<int> oidList = new List<int>();
                        oidList.Add(feature.OID);
                        Dictionary<int, string> oid2Text = AnnotationHelper.getFeatureLabelText(GApplication.Application.MapControl.Map, lyr, oidList, annoRule.AnnoFieldName, annoRule.Expression);
                        if (oid2Text.ContainsKey(feature.OID))
                        {
                            annoText = oid2Text[feature.OID];
                        }
                    }
                    else
                    {
                        object val = feature.get_Value(feature.Fields.FindField(annoRule.AnnoFieldName));
                        if (val == null || Convert.IsDBNull(val))
                            annoText = string.Empty;
                        else
                            annoText = val.ToString();
                    }
                    if (annoText == "")
                        continue;
                    #endregion

                    ITextElement newTextElement = null;
                    if ((feature.Class as IFeatureClass).AliasName.ToUpper() == "LRDL" && annoRule.AnnoFieldName == "RTEG")//道路等级等注记特殊处理
                    {
                        newTextElement = AnnotationHelper.CreateTextElement(annoCenterPoint, annoText, annoRule, specialCharacterTable);
                    }
                    else//常规处理
                    {
                        newTextElement = AnnotationHelper.CreateTextElement(annoCenterPoint, annoText, annoRule);
                    }

                    //注记长度
                    IFeature newFe = annoName2FeatureClass[annoRule.AnnoFCName].CreateFeature();
                    (newFe as IAnnotationFeature2).Annotation = newTextElement as IElement;
                    double annoLen = CommonMethods.GetAnnoFeatureLen(newFe, GApplication.Application.ActiveView.FocusMap.ReferenceScale);

                    rule2EleAndLen.Add(annoRule, new KeyValuePair<IFeature, double>(newFe, annoLen));
                    if (totalLen == 0)
                    {
                        totalLen = annoLen;
                    }
                    else
                    {
                        totalLen = totalLen + annoLen + AnnoInterval * annoRule.FontSize * 1e-3 * GApplication.Application.ActiveView.FocusMap.ReferenceScale;
                    }
                }
                #endregion

                if (bOverrun && feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                {
                    //注记长度大于要素长度指定比例时，不生成注记
                    double shapeLen = (feature.Shape as IPolyline).Length;
                    if (totalLen > shapeLen * overrunScale)
                    {
                        foreach (var kv in rule2EleAndLen)
                        {
                            IFeature newFe = kv.Value.Key;
                            newFe.Delete();
                        }
                        GApplication.Application.MapControl.Map.AnnotationEngine = annoEngine;
                        return newFeList;
                    }

                }

                //生成注记
                IPoint annoPoint = null;//当前注记位置点
                bool isSameDirection = true;//注记方向与线的走向是否一致
                double deltaLen = 0;//基于上一条注记位置点的偏移长度
                foreach (var kv in rule2EleAndLen)
                {
                    AnnotationRule annoRule = kv.Key;
                    IFeature newFe = kv.Value.Key;
                    ITextElement newTextElement = (newFe as IAnnotationFeature).Annotation as ITextElement;
                    double annoLen = kv.Value.Value;
                    string annoText = newTextElement.Text;

                    
                    #region 更新当前注记位置
                    if (annoPoint == null)//第一条注记：位置赋值
                    {
                        #region 第一条注记位置
                        if (rule2EleAndLen.Count == 1)//仅一个注记
                        {
                            annoPoint = annoCenterPoint;
                        }
                        else//多个注记
                        {
                            double delta = totalLen * 0.5 - annoLen * 0.5;
                            if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)//往前偏移
                            {
                                double distanceAlongCurve = 0;
                                double distanceFromCurve = 0;
                                bool bRightSide = false;
                                IPoint outPoint = new PointClass();
                                (feature.Shape as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, annoCenterPoint, false, outPoint,
                                    ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);//获取前一注记点在线上的位置，及长度

                                //中心点在线上点的切向方向
                                ILine tangentLine = new Line();
                                (feature.Shape as IPolyline).QueryTangent(esriSegmentExtension.esriNoExtension, distanceAlongCurve, false, 0.001, tangentLine);
                                double centerAngle = AnnotationHelper.RadianNormalization(tangentLine.Angle);//[0, 2*PI]

                                //沿线偏移delta
                                IPoint deltaPt = new PointClass();
                                double len = distanceAlongCurve - delta;//先默认注记方向与线方向一致
                                (feature.Shape as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, len, false, deltaPt);
                                if ((centerAngle > Math.PI * 0.25 && centerAngle < Math.PI * 0.75) || (centerAngle > Math.PI * 1.25 && centerAngle < Math.PI * 1.75))
                                //if ((centerAngle > Math.PI * 0.5 && centerAngle < Math.PI * 0.75) || (centerAngle > Math.PI * 1.5 && centerAngle < Math.PI * 1.75))
                                {
                                    //注记方向从上往下，注记间的摆放顺序则为从下到上
                                    if (deltaPt.Y < outPoint.Y)
                                        isSameDirection = false;
                                }
                                else
                                {
                                    //注记方向从左到右，注记间的摆放顺序也是从左到右
                                    if (deltaPt.X > outPoint.X)
                                        isSameDirection = false;
                                }

                                if (!isSameDirection)
                                {
                                    len = distanceAlongCurve + delta;
                                    (feature.Shape as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, len, false, deltaPt);
                                }
                                annoPoint = deltaPt;

                                //获取当前点的切线方向
                                tangentLine = new Line();
                                (feature.Shape as IPolyline).QueryTangent(esriSegmentExtension.esriNoExtension, len, false, 0.001, tangentLine);
                                rotateAngle = AnnotationHelper.RadianNormalization(tangentLine.Angle);
                            }
                            else
                            {
                                annoPoint.X -= delta;
                            }
                        }
                        #endregion

                    }
                    else//非第一条注记:位置更新
                    {
                        deltaLen += annoLen * 0.5;//本条注记相对于上一条注记位置的偏移量更新
                        if (feature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                        {
                            double distanceAlongCurve = 0;
                            double distanceFromCurve = 0;
                            bool bRightSide = false;
                            IPoint outPoint = new PointClass();
                            (feature.Shape as IPolyline).QueryPointAndDistance(esriSegmentExtension.esriNoExtension, annoPoint, false, outPoint,
                                ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);//获取前一注记点在线上的位置，及长度
                            //沿线偏移deltaLen
                            IPoint deltaPt = new PointClass();
                            double len = 0;
                            if (isSameDirection)
                            {
                                len = distanceAlongCurve + deltaLen;
                            }
                            else
                            {
                                len = distanceAlongCurve - deltaLen;
                            }
                            (feature.Shape as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, len, false, deltaPt);
                            annoPoint = deltaPt;

                            //获取当前点的切线方向
                            ILine tangentLine = new Line();
                            (feature.Shape as IPolyline).QueryTangent(esriSegmentExtension.esriNoExtension, len, false, 0.001, tangentLine);
                            rotateAngle = AnnotationHelper.RadianNormalization(tangentLine.Angle);
                        }
                        else
                        {
                            annoPoint.X += deltaLen;
                        }
                    }
                    #endregion 

                    #region 添加注记
                    //更新注记位置
                    (newTextElement as IElement).Geometry = annoPoint;

                    //对线状要素的注记进行旋转处理
                    if ((feature.Class as IFeatureClass).ShapeType == esriGeometryType.esriGeometryPolyline)
                    {
                        if (annoRule.EnableCJKCharactersRotation && (rotateAngle > Math.PI * 0.25 && rotateAngle <= Math.PI * 0.5))
                        {
                            rotateAngle += Math.PI;
                        }

                        ITransform2D pTransform2D = newTextElement as ITransform2D;
                        pTransform2D.Rotate(annoPoint, rotateAngle);
                    }

                    //更新要素
                    IAnnotationFeature2 newAnnoFe = newFe as IAnnotationFeature2;
                    newAnnoFe.Annotation = newTextElement as IElement;
                    newAnnoFe.AnnotationClassID = feature.Class.ObjectClassID;//这里用该属性记录注记所对应的实体要素类
                    newAnnoFe.LinkedFeatureID = feature.OID;
                    newAnnoFe.Status = esriAnnotationStatus.esriAnnoStatusPlaced;

                    int blanktypeIndex = newFe.Fields.FindField("blankingtype");
                    if (blanktypeIndex != -1 && annoRule.BlankingType == "单要素局部消隐")
                    {
                        newFe.set_Value(blanktypeIndex, annoRule.BlankingType);
                    }

                    newFe.Store();

                    newFeList.Add(newFe);
                    #endregion

                    #region 注记消隐处理
                    if (annoRule.BlankingType == "单要素局部消隐")//如道路的RN注记
                    {
                        //注记消隐处理
                        AnnotationHelper.AnnotationBlanking(feature, newFe.Shape as IPolygon);
                        
                        #region 江苏1万地形特殊处理
                        if (GApplication.Application.Template.Root.Contains("江苏"))
                        {
                            if (feature.Class.AliasName.ToUpper() == "LRDL" && (annoRule.AnnoClass != "主干道" && annoRule.AnnoClass != "次干道" && annoRule.AnnoClass != "支线"))
                            {
                                string fcName = "HFCL";
                                IFeatureLayer hfclLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l =>
                                {
                                    return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName == fcName
                                        && ((l as IFeatureLayer).FeatureClass as IDataset).Workspace.PathName == GApplication.Application.Workspace.EsriWorkspace.PathName;
                                }).FirstOrDefault() as IFeatureLayer;
                                if (hfclLayer != null)
                                {
                                    var filter = new QueryFilterClass { WhereClause = "GB=270101 or GB=270102" };
                                    IFeatureCursor feCursor = hfclLayer.FeatureClass.Search(filter, false);
                                    IFeature f = null;
                                    while ((f = feCursor.NextFeature()) != null)
                                    {
                                        //注记消隐处理
                                        AnnotationHelper.AnnotationBlanking(f, newFe.Shape as IPolygon);

                                        Marshal.ReleaseComObject(f);
                                    }
                                    Marshal.ReleaseComObject(feCursor);
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion

                    //更新下一条注记相对与本条注记的偏移量
                    deltaLen = annoLen * 0.5 + AnnoInterval * annoRule.FontSize * 1e-3 * GApplication.Application.ActiveView.FocusMap.ReferenceScale;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GApplication.Application.MapControl.Map.AnnotationEngine = annoEngine;
            }

            return newFeList;
        }
    }
}

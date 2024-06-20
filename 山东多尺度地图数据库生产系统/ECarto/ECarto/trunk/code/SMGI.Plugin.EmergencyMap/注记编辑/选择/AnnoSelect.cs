using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Xml.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.EmergencyMap
{
    public class AnnoSelect:SMGI.Common.SMGITool
    {
        #region Li
        private IFeature _selMultipartAnnoFe;
        private int _selPartGeoIndex;
        private int _selPartTextIndex;
        private IElement _selectedElement;
        private IMovePolygonFeedback _textFeedback;
        #endregion

        public AnnoSelect() {
            m_caption = "注记选择";
            m_toolTip = "注记选择，用于平移、删除和注记修改;注记修改，请选择完毕后右击进行属性修改";
            m_category = "注记编辑";
            currentTool = new ControlsEditingEditToolClass();
            NeedSnap = false;
            //EnableSnap(false);

            #region Li
            _selMultipartAnnoFe = null;
            _selPartGeoIndex = -1;
            _selPartTextIndex = -1;
            _selectedElement = null;
            _textFeedback = null;
            #endregion
        }
        /// <summary>
        /// 编辑工具
        /// </summary>
        ControlsEditingEditToolClass currentTool;
        /// <summary>
        /// 选中的要素字典
        /// </summary>
        Dictionary<string, Lyr_AnnoStyle> selectedFeaureDic = new Dictionary<string, Lyr_AnnoStyle>();
        /// <summary>
        /// 是否按住了Ctrl键
        /// </summary>
        bool isPressCtrl = false;
        /// <summary>
        /// 是否点击清除选择集的操作
        /// </summary>
        bool isClickUnSelect = true;

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
           
            currentTool.OnCreate(m_Application.MapControl.Object);
            //绑定要素与注记关联的删除事件，实现删除要素，相关联的注记也会跟着删除
            IEngineEditEvents_Event engineEdit_Events = m_Application.EngineEditor as IEngineEditEvents_Event;
            engineEdit_Events.OnDeleteFeature += (objectFeature) =>
            {
                IFeature deleteFeature = objectFeature as IFeature;
                //判断是否非注记要素，注记要素包含FeatureID字段,非注记要素删除的时候要进行其关联注记的删除
                if (deleteFeature.Fields.FindField("FeatureID") == -1)
                {
                    //获取其featureClassID和featureID，用于在注记FeatureClass中进行关联查找
                    int featureClassID = deleteFeature.Class.ObjectClassID;
                    int feaureID = deleteFeature.OID;
                    //获取当前工程中的注记FeatureClass
                    //获取所以可以选择显示的要素图层
                    var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IFDOGraphicsLayer;
                    })).ToArray();
                    IQueryFilter queryFilter = new QueryFilterClass();
                    IEnvelope newEnvelope = deleteFeature.Extent;
                    //mdb数据库中的语法格式，若为gdb，应采用""
                    queryFilter.WhereClause = "AnnotationClassID=" + featureClassID + " and FeatureID=" + feaureID;
                    for (int i = 0; i < lyrs.Length; i++)
                    {
                        IFeatureClass featureClass = (lyrs[i] as IFeatureLayer).FeatureClass;
                        IFeatureCursor featureCursor = featureClass.Search(queryFilter, false);
                        IFeature feature = null;
                        while ((feature = featureCursor.NextFeature()) != null)
                        {
                            if (newEnvelope.XMax < feature.Extent.XMax)
                                newEnvelope.XMax = feature.Extent.XMax;
                            if (newEnvelope.XMin > feature.Extent.XMin)
                                newEnvelope.XMin = feature.Extent.XMin;
                            if (newEnvelope.YMax < feature.Extent.YMax)
                                newEnvelope.YMax = feature.Extent.YMax;
                            if (newEnvelope.YMin > feature.Extent.YMin)
                                newEnvelope.YMin = feature.Extent.YMin;
                            feature.Delete();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(featureClass);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(queryFilter);
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, newEnvelope);
                    GC.Collect();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteFeature);
            };
            //实现选中注记，相关要素高亮显示
            m_Application.MapControl.OnSelectionChanged += new EventHandler(MapControl_OnSelectionChanged);
        }
       
        /// <summary>
        /// 用于选中注记时，关联要素高亮显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapControl_OnSelectionChanged(object sender, EventArgs e)
        {
            //获取选择集
            m_Application.ActiveView.GraphicsContainer.Reset();
            IElement deleteElement = null;
            while ((deleteElement = m_Application.ActiveView.GraphicsContainer.Next()) != null)
            {
                IElementProperties deleteElementPro = deleteElement as IElementProperties;
                if (deleteElementPro != null && deleteElementPro.Name == "注记选择")
                {
                    m_Application.ActiveView.GraphicsContainer.DeleteElement(deleteElement);
                }
            }
            //当点击清除选择集的时候执行
            if (isClickUnSelect && m_Application.MapControl.Map.SelectionCount == 0)
            {
                selectedFeaureDic.Clear();
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                return;
            }
            if (m_Application.MapControl.CurrentTool is AnnoSelect)
            {
                //m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                IEnumFeature pMapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                pMapEnumFeature.Reset();
                IFeature pFeature = null;
                while ((pFeature = pMapEnumFeature.Next()) != null)
                {
                    //非注记要素直接跳过
                    if (pFeature.Fields.FindField("FeatureID") == -1)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeature);
                        continue;
                    }
                
                    //注记要素
                    object featureClassIDob = pFeature.Value[pFeature.Fields.FindField("AnnotationClassID")];
                    object featureIDob = pFeature.Value[pFeature.Fields.FindField("FeatureID")];
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeature);
                    if (featureClassIDob == null || Convert.IsDBNull(featureClassIDob))
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeature);
                        continue;
                    }
                    int featureClassID = Convert.ToInt32(featureClassIDob);
                    int featureID = Convert.ToInt32(featureIDob);
                    if (featureClassID <= 0 || featureID <= 0)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeature);
                        continue;
                    }
                    //遍历当前图层，查找feaureClassID的要素集
                    var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IGeoFeatureLayer;
                    })).ToArray();
                    for (int i = 0; i < lyrs.Length; i++)
                    {
                        if ((lyrs[i] as IFeatureLayer).FeatureClass.ObjectClassID == featureClassID)
                        {
                            IFeatureClass featureClass = (lyrs[i] as IFeatureLayer).FeatureClass;
                            IQueryFilter queryFilter = new QueryFilterClass();
                            queryFilter.WhereClause = "OBJECTID=" + featureID;
                            IFeatureCursor featureCursor = featureClass.Search(queryFilter, false);
                            IFeature feature = featureCursor.NextFeature();
                            if (feature != null)
                            {
                                

                                IColor color = new RgbColorClass { Red = 0, Green = 255, Blue = 255 } as IColor;
                                IElement element = null;
                                IGeometry geometry = feature.Shape;
                                //将图元进行着色
                                if (geometry.GeometryType == esriGeometryType.esriGeometryPoint)
                                {
                                    IMarkerElement markerElement = new MarkerElementClass();
                                    ISimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbolClass();
                                    markerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
                                    markerSymbol.Size = 2;
                                    markerSymbol.Color = color;
                                    markerElement.Symbol = markerSymbol;
                                    element = markerElement as IElement;
                                }
                                else if (geometry.GeometryType == esriGeometryType.esriGeometryPolyline)
                                {
                                    ILineElement lineElement = new LineElementClass();
                                    ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                                    lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                                    lineSymbol.Color = color;
                                    lineSymbol.Width = 1;
                                    lineElement.Symbol = lineSymbol;
                                    element = lineElement as IElement;
                                }
                                else if (geometry.GeometryType == esriGeometryType.esriGeometryPolygon)
                                {
                                    ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                                    lineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                                    lineSymbol.Color = color;
                                    lineSymbol.Width = 1;
                                    //
                                    ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass();
                                    fillSymbol.Color = color;
                                    fillSymbol.Style = esriSimpleFillStyle.esriSFSNull;
                                    fillSymbol.Outline = lineSymbol;
                                    //
                                    IFillShapeElement fillElement = new PolygonElementClass();
                                    fillElement.Symbol = fillSymbol;
                                    element = fillElement as IElement;
                                }
                                if (element != null)
                                {
                                    element.Geometry = geometry;
                                    IElementProperties elePro = element as IElementProperties;
                                    elePro.Name = "注记选择";
                                    m_Application.ActiveView.GraphicsContainer.AddElement(element, 0);
                                    //此处不刷新，在OnMouseDown下面进行刷新操作
                                }
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(queryFilter);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureClass);
                            GC.Collect();
                            break;
                        }
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pMapEnumFeature);
            }
        } 

        public override bool Enabled{
            get{
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing;
            }
        }
        public override void OnClick(){
            currentTool.OnClick();
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, m_Application.ActiveView.Extent);
            m_Application.MapControl.Map.ClearSelection();
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, m_Application.ActiveView.Extent);
        }

        public override bool Deactivate()
        {
            ClearPartElement();

            selectedFeaureDic.Clear();
            return currentTool.Deactivate();
        }

        public override bool OnContextMenu(int x, int y)
        {
            return currentTool.OnContextMenu(x, y);
        }

        public override void OnDblClick()
        {
            currentTool.OnDblClick();
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
            currentTool.OnKeyDown(keyCode, shift);
            if (keyCode == 17)//Ctrl键
                isPressCtrl = true;
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            currentTool.OnKeyUp(keyCode, shift);
            if (keyCode == 17)
                isPressCtrl = false;
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            #region Li 左键单击的位置是已选择的唯一注记要素，且该注记要素为多部件注记，则进行处理
            if (button == 1)
            {
                ClearPartElement();

                IFeature objFe = null;
                IMultiPartTextElement multiPartTextElement = null;
                if (selectedFeaureDic.Count == 1)
                {
                    var kv = selectedFeaureDic.First();
                    var fc = kv.Value.featureLayer.FeatureClass;
                    int featureID = Convert.ToInt32(kv.Key.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    try
                    {
                        objFe = fc.GetFeature(featureID);
                        if (objFe != null)
                        {
                            var objAnnoFe = objFe as IAnnotationFeature2;
                            if (objAnnoFe != null && objAnnoFe.Annotation is IMultiPartTextElement)
                            {
                                multiPartTextElement = objAnnoFe.Annotation as IMultiPartTextElement;
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                if (multiPartTextElement != null && multiPartTextElement.PartCount > 1)
                {
                    //判断当前点击位置是否位于某个部件几何内
                    IPoint pt = ToSnapedMapPoint(x, y);
                    IRelationalOperator ro = pt as IRelationalOperator;
                    //var gc = (objFe.ShapeCopy as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                    var gc = objFe.ShapeCopy as IGeometryCollection;
                    for (int i = 0; i < gc.GeometryCount; ++i)
                    {
                        //var partGeo = gc.get_Geometry(i);
                        IPolygon partGeo = new PolygonClass();
                        (partGeo as IPointCollection).AddPointCollection(gc.get_Geometry(i) as IPointCollection);
                        (partGeo as ITopologicalOperator).Simplify();

                        if (ro.Within(partGeo))
                        {
                            double xOffset = (multiPartTextElement as ISymbolCollectionElement).XOffset;
                            double yOffset = (multiPartTextElement as ISymbolCollectionElement).YOffset;

                            _selPartTextIndex = -1;//text部件索引
                            #region 如何有效的根据要素部件几何确定text部件索引
                            for (int j = 0; j < multiPartTextElement.PartCount; ++j)
                            {
                                var partEle = multiPartTextElement.QueryPart(j) as IElement;
                                IRelationalOperator textRO = null;
                                if (partEle.Geometry is IPoint)
                                {
                                    textRO = partEle.Geometry as IRelationalOperator;
                                }
                                else if (partEle.Geometry is IPolyline)
                                {
                                    var centerPT = new PointClass();
                                    (partEle.Geometry as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerPT);

                                    textRO = centerPT as IRelationalOperator;
                                }
                                else
                                {
                                    textRO = partEle.Geometry as IRelationalOperator;
                                }
                                if (textRO.Within(partGeo))
                                {
                                    _selPartTextIndex = j;
                                    break;
                                }
                            }

                            if (_selPartTextIndex == -1)
                            {

                                System.Diagnostics.Trace.WriteLine(string.Format("【{0}】捕捉异常:鼠标位置：x={1},y ={2}", i, pt.X, pt.Y));
                                for (int j = 0; j < multiPartTextElement.PartCount; ++j)
                                {
                                    var partEle = multiPartTextElement.QueryPart(j) as IElement;
                                    if (partEle.Geometry is IPoint)
                                    {
                                        System.Diagnostics.Trace.WriteLine(string.Format("注记部件【{0}】基线锚点：x={1}，y={2}", (partEle as ITextElement).Text, (partEle.Geometry as IPoint).X, (partEle.Geometry as IPoint).Y));
                                    }
                                    else if (partEle.Geometry is IPolyline && (partEle.Geometry as IPointCollection).PointCount == 2)
                                    {
                                        IPolyline pl = partEle.Geometry as IPolyline;
                                        System.Diagnostics.Trace.WriteLine(string.Format("注记部件【{0}】基线坐标：x1={1}，y1={2},x2={3},y2={4}", (partEle as ITextElement).Text, pl.FromPoint.X, pl.FromPoint.Y, pl.ToPoint.X, pl.ToPoint.Y));
                                    }
                                    else
                                    {
                                        System.Diagnostics.Trace.WriteLine(string.Format("注记部件【{0}】基线范围：xmin={1}，xmax={2},ymin={3},ymax={4}", (partEle as ITextElement).Text, partEle.Geometry.Envelope.XMin, partEle.Geometry.Envelope.XMax, partEle.Geometry.Envelope.YMin, partEle.Geometry.Envelope.YMax));
                                    }

                                }

                                continue;
                            }
                            #endregion

                            _selMultipartAnnoFe = objFe;
                            _selPartGeoIndex = i;//要素几何部件索引

                            _selectedElement = new PolygonElement();
                            _selectedElement.Geometry = partGeo;//几何数据
                            ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass();
                            fillSymbol.Color = new RgbColorClass { Red = 200, Blue = 200 };//填充颜色
                            fillSymbol.Style = esriSimpleFillStyle.esriSFSBackwardDiagonal;
                            fillSymbol.Outline = null;
                            (_selectedElement as IFillShapeElement).Symbol = fillSymbol; //符号
                            m_Application.MapControl.ActiveView.GraphicsContainer.AddElement(_selectedElement, 0);

                            //更新视图
                            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, _selectedElement, null);
                            break;
                        }
                    }
                }
            }
            #endregion

            currentTool.OnMouseDown(button, shift, x, y);
        }

        /// <summary>
        /// 更新注记的样式
        /// </summary>
        private void UpdateAnno(AnnoStyle commonStyle,AnnoStyle updateStyle) {
            //根据新旧属性，创建更新注记样式。没有变动的属性仍然采用原有的自己的属性
            
            //批量创建。。。
            m_Application.EngineEditor.EnableUndoRedo(true);
            m_Application.EngineEditor.StartOperation();
            IEnvelope refreshEnvelop = null;
            foreach (string featureIDClassName in selectedFeaureDic.Keys)
            {
                IFeatureLayer ppFeatureLayer = selectedFeaureDic[featureIDClassName].featureLayer;
                int featureID = Convert.ToInt32(featureIDClassName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                IFeature feature = ppFeatureLayer.FeatureClass.GetFeature(featureID);
                IAnnotationFeature2 annoFeature = feature as IAnnotationFeature2;
                int AnnotationClassID = annoFeature.AnnotationClassID;
                int FeatureID = annoFeature.LinkedFeatureID;
                //-----------------根据外包框和锚点以及位置点进行计算新的定位要素------
                
                AnnoStyle oldStyle = selectedFeaureDic[featureIDClassName].annoStyle;//
                //if (selectedFeaureDic.Count == 1)
                //    oldStyle = updateStyle;
                //IElement element = annoFeature.Annotation;
                //IGeometry posGeometry = element.Geometry;
                //if (!updateStyle.NoNormal && posGeometry is IPoint)
                //{
                //    //非正常文本格式的时候，且该注记的定位元素为点状，采用这种方式
                //    IPoint nPoint = (feature.Shape as IArea).LabelPoint;
                //    (posGeometry as IPoint).Y = nPoint.Y;
                //}

                ////创建新的textElement
                //string message = string.Empty;
                //ITextElement newTextElement = AnnoFunc.CreateTextElement(posGeometry,
                //            oldStyle.Text,
                //            updateStyle.FontName,
                //            oldStyle.CharacterWidth,
                //            oldStyle.Itatic,
                //            updateStyle.FontSize,
                //            GetColorByString(updateStyle.FontColorCMYK),
                //            out message,
                //            oldStyle.FontBold,
                //            oldStyle.Horizontal,
                //            oldStyle.Vertical
                //            );
                //if (selectedFeaureDic.Count == 1)
                //{
                //    newTextElement = AnnoFunc.CreateTextElement(posGeometry, oldStyle.Text,
                //            updateStyle.FontName,
                //            updateStyle.CharacterWidth,
                //            updateStyle.Itatic,
                //            updateStyle.FontSize,
                //            GetColorByString(updateStyle.FontColorCMYK),
                //            out message,
                //            updateStyle.FontBold,
                //            updateStyle.Horizontal,
                //            updateStyle.Vertical);
                //}
                //if (newTextElement == null)
                //    return;
                //ISymbolCollectionElement pSymbolCollEle = newTextElement as ISymbolCollectionElement;
                //pSymbolCollEle.CharacterSpacing = commonStyle.CharacterSpace;
                ////添加背景边框，如果有的话
                //IElement newElement = newTextElement as IElement;

                ////蒙版
                //ITextSymbol textSymbol = newTextElement.Symbol;
                //IMask textMask = textSymbol as IMask;
                //textMask.MaskSize = updateStyle.TextMaskSize;
                //textMask.MaskStyle = updateStyle.TextMaskStyle;
                //ILineSymbol maskLineSymbol = new SimpleLineSymbol();
                //maskLineSymbol.Width = 0;
                //IFillSymbol maskSymbol = new SimpleFillSymbolClass();
                //maskSymbol.Color = GetColorByString(updateStyle.TextMaskColor);
                //maskSymbol.Outline = maskLineSymbol;
                //textMask.MaskSymbol = maskSymbol;
                //if (updateStyle.HasMask)//气泡背景
                //{
                //    //设置文本背景框样式
                //    IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;
                //    IBalloonCallout balloonCallout = new BalloonCalloutClass();
                //    balloonCallout.Style = updateStyle.BalloonCallout;

                //    //设置文本背景框颜色
                //    IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                //    fillSymbol.Color = GetColorByString(updateStyle.MaskColorCMYK);
                //    ILineSymbol lineSymbol = new SimpleLineSymbol();
                //    lineSymbol.Width = updateStyle.LineWidth;
                //    lineSymbol.Color = GetColorByString(updateStyle.LineColorCMYK);
                //    fillSymbol.Outline = lineSymbol;
                //    balloonCallout.Symbol = fillSymbol;
                //    //设置背景框边距
                //    ITextMargins textMarigns = balloonCallout as ITextMargins;
                //    textMarigns.PutMargins(updateStyle.TextMarginsLeft, updateStyle.TextMarginsUp, updateStyle.TextMarginsRight, updateStyle.TextMarginsDown);
                //    formattedTextSymbol.Background = balloonCallout as ITextBackground;
                //}
                //textSymbol.Angle = updateStyle.Angle * 180 / Math.PI;//角度
                //newTextElement.Symbol = textSymbol;
                //annoFeature.Annotation = newElement;
                //feature.Store();
                //IEnvelope en = feature.Shape.Envelope;
                //if (refreshEnvelop == null)
                //    refreshEnvelop = en;
                //else
                //{
                //    if (en.XMax > refreshEnvelop.XMax)
                //        refreshEnvelop.XMax = en.XMax;
                //    if (en.YMax > refreshEnvelop.YMax)
                //        refreshEnvelop.YMax = en.YMax;
                //    if (en.XMin < refreshEnvelop.XMin)
                //        refreshEnvelop.XMin = en.XMin;
                //    if (en.YMin < refreshEnvelop.YMin)
                //        refreshEnvelop.YMin = en.YMin;
                //}

                UpdateAnnoFeature(feature, oldStyle, updateStyle);
            }
            m_Application.EngineEditor.StopOperation("注记符号修改");
            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, refreshEnvelop);
        }

        /// <summary>
        /// 根据文本元素获取其注记属性
        /// </summary>
        /// <param name="textElement"></param>
        /// <returns></returns>
        private AnnoStyle GetStyleByFeature(IFeature feature)
        {
            ITextElement textElement = (feature as IAnnotationFeature2).Annotation as ITextElement;
            AnnoStyle style = new AnnoStyle();
            style.LyrName = feature.Class.AliasName;
            style.Angle = textElement.Symbol.Angle * Math.PI / 180;
            style.Horizontal = textElement.Symbol.HorizontalAlignment;
            style.Vertical = textElement.Symbol.VerticalAlignment;
            
            IFormattedTextSymbol formattedTextSymbol = textElement.Symbol as IFormattedTextSymbol;
            if (formattedTextSymbol.VerticalAlignment!= esriTextVerticalAlignment.esriTVACenter||
                formattedTextSymbol.HorizontalAlignment!= esriTextHorizontalAlignment.esriTHACenter)
            {
                style.NoNormal = false;
            }
            //背景：气泡
            if (formattedTextSymbol.Background != null)
            {
                style.HasMask = true;
                IBalloonCallout balloonCallout = formattedTextSymbol.Background as IBalloonCallout;
                if (balloonCallout == null)
                {
                    return null;
                }
                style.BalloonCallout = balloonCallout.Style;
                style.TextMarginsLeft = (balloonCallout as ITextMargins).LeftMargin;
                style.TextMarginsUp = (balloonCallout as ITextMargins).TopMargin;
                style.TextMarginsRight = (balloonCallout as ITextMargins).RightMargin;
                style.TextMarginsDown = (balloonCallout as ITextMargins).BottomMargin;
                style.MaskColorCMYK = GetStringByColor(balloonCallout.Symbol.Color);
                style.LineColorCMYK = GetStringByColor(balloonCallout.Symbol.Outline.Color);
                style.LineWidth = balloonCallout.Symbol.Outline.Width;
            }
            //蒙版
            style.TextMaskSize = (textElement.Symbol as IMask).MaskSize;
            style.TextMaskStyle = (textElement.Symbol as IMask).MaskStyle;
            if((textElement.Symbol as IMask).MaskSymbol!=null)
               style.TextMaskColor = GetStringByColor((textElement.Symbol as IMask).MaskSymbol.Color);
 
            style.Text = textElement.Text;
            style.FontColorCMYK = GetStringByColor(textElement.Symbol.Color);
            style.FontName = textElement.Symbol.Font.Name;
            style.FontSize = textElement.Symbol.Size / 2.8345;
            ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)textElement;
            style.FontBold = pSymbolCollEle.Bold;
            style.CharacterWidth = pSymbolCollEle.CharacterWidth;
            style.CharacterSpace = pSymbolCollEle.CharacterSpacing;
            style.Itatic = pSymbolCollEle.Italic;
            return style;
        }

        /// <summary>
        /// 根据注记规则里的CMYK字符串得到CMYK颜色值
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

        /// <summary>
        /// 根据CMYK颜色值得到CMYK字符串
        /// </summary>
        /// <param name="color">IColor颜色值</param>
        /// <returns>CMYK字符串（形如：C100M200Y100K50）</returns>
        private string GetStringByColor(IColor color)
        {
            ICmykColor cmykColor = new CmykColorClass { CMYK = color.CMYK };
            string cmykString = string.Empty;
            if (cmykColor.Cyan != 0)
                cmykString += "C" + cmykColor.Cyan.ToString();
            if (cmykColor.Magenta != 0)
                cmykString += "M" + cmykColor.Magenta.ToString();
            if (cmykColor.Yellow != 0)
                cmykString += "Y" + cmykColor.Yellow.ToString();
            if (cmykColor.Black != 0)
                cmykString += "K" + cmykColor.Black.ToString();
            return cmykString == string.Empty ? "C0M0Y0K0" : cmykString;
        }
        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            #region Li 拖动选择的多部件
            if (button == 1 && _selMultipartAnnoFe != null && _selPartGeoIndex > -1)
            {
                IPoint pt = ToSnapedMapPoint(x, y);
                if (_textFeedback == null)
                {
                    //var gc = (_selMultipartAnnoFe.ShapeCopy as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                    //IPolygon selPolygon = gc.get_Geometry(_selPartIndex) as IPolygon;
                    var gc = _selMultipartAnnoFe.ShapeCopy as IGeometryCollection;
                    IPolygon selPolygon = new PolygonClass();
                    (selPolygon as IPointCollection).AddPointCollection(gc.get_Geometry(_selPartGeoIndex) as IPointCollection);
                    (selPolygon as ITopologicalOperator).Simplify();

                    IFillSymbol symbol = new SimpleFillSymbolClass();
                    symbol.Color = new RgbColorClass { NullColor = true };//填充颜色

                    _textFeedback = new MovePolygonFeedbackClass() { Display = m_Application.ActiveView.ScreenDisplay };
                    _textFeedback.Symbol = symbol as ISymbol;

                    _textFeedback.Start(selPolygon, pt);
                }
                else
                {
                    _textFeedback.MoveTo(pt);
                }

                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, _textFeedback, null);

                return;
            }
            #endregion

            currentTool.OnMouseMove(button, shift, x, y);
        }
        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            currentTool.OnMouseUp(button, shift, x, y);


            #region Li 更新注记位置
            if (button == 1 && _textFeedback != null)
            {
                var newGeo = _textFeedback.Stop();
                _textFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
                _textFeedback = null;

                //var gc = (_selMultipartAnnoFe.ShapeCopy as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                var gc = _selMultipartAnnoFe.ShapeCopy as IGeometryCollection;
                var partGeoPC = gc.get_Geometry(_selPartGeoIndex) as IPointCollection;

                //更新文本几何(该过程仅考虑进行了平移)
                double dx = newGeo.FromPoint.X - partGeoPC.get_Point(0).X;
                double dy = newGeo.FromPoint.Y - partGeoPC.get_Point(0).Y;
                var objAnnoFe = _selMultipartAnnoFe as IAnnotationFeature2;
                var multiPartTextElement = objAnnoFe.Annotation as IMultiPartTextElement;
                IElement partEle = multiPartTextElement.QueryPart(_selPartTextIndex) as IElement;
                ITransform2D trans = partEle as ITransform2D;
                trans.Move(dx, dy);
                multiPartTextElement.ReplacePart(_selPartTextIndex, (partEle as ITextElement).Text, partEle.Geometry);
                objAnnoFe.Annotation = multiPartTextElement as IElement;

                //更新要素几何
                IEnvelope env = _selMultipartAnnoFe.Shape.Envelope;
                IGeometry newShape = new PolygonClass();
                for (int i = 0; i < gc.GeometryCount; ++i)
                {
                    if (i != _selPartGeoIndex)
                    {
                        var geopc = gc.get_Geometry(i) as IPointCollection;
                        IPolygon geo = new PolygonClass();
                        (geo as IPointCollection).AddPointCollection(geopc);
                        (geo as ITopologicalOperator).Simplify();

                        (newShape as IGeometryCollection).AddGeometryCollection(geo as IGeometryCollection);
                    }
                    else
                    {
                        (newShape as IGeometryCollection).AddGeometryCollection(newGeo as IGeometryCollection);
                    }
                }
                _selMultipartAnnoFe.Shape = newShape;
                _selMultipartAnnoFe.Store();

                //更新临时元素
                _selectedElement.Geometry = newGeo;
                m_Application.MapControl.ActiveView.GraphicsContainer.UpdateElement(_selectedElement);
                env.Union(_selMultipartAnnoFe.Shape.Envelope);
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, _selectedElement, env);
            }
            #endregion

            /*-----------------------选中注记---------------------------------------------------*/
            if (button == 1)
            {
                ISelectionEnvironment pSelectionEnvironment = new SelectionEnvironmentClass();
                //defaultSelectEnviColor = pSelectionEnvironment.DefaultColor as IRgbColor;//保存系统默认的颜色
                //将环境选择元素置为红色
                IRgbColor pColor = new RgbColor();
                pColor.Red = 255;
                pSelectionEnvironment.DefaultColor = pColor;
                if (!isPressCtrl)//按住Ctrl键，进行点击多选，否则，进行一次性选择
                {
                    selectedFeaureDic.Clear();
                }
                //按住Ctrl键，Map.FeatureSelection不会接受Ctrl事件
                IEnumFeature pMapEnumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                pMapEnumFeature.Reset();
                IFeature pFeature = null;
                var lyrs = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IFDOGraphicsLayer;
                })).ToArray();
                while ((pFeature = pMapEnumFeature.Next()) != null)
                {
                    IFeatureClass pFeatureClass = pFeature.Class as IFeatureClass;
                    for (int i = 0; i < lyrs.Length; i++)
                    {
                        IFeatureLayer _FeatureLayer = lyrs[i] as IFeatureLayer;
                        if (pFeatureClass == _FeatureLayer.FeatureClass)
                        {
                            string featureIDClassName = pFeature.OID.ToString() + "_" + _FeatureLayer.FeatureClass.AliasName;
                            if (selectedFeaureDic.ContainsKey(featureIDClassName))
                                selectedFeaureDic.Remove(featureIDClassName);
                            else
                                selectedFeaureDic.Add(featureIDClassName, new Lyr_AnnoStyle { featureLayer = _FeatureLayer });
                            break;
                        }
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pMapEnumFeature);
                isClickUnSelect = false;
                m_Application.MapControl.Map.ClearSelection();
                //获取所选要素集的外包框
                IEnvelope refreshEnvelop = null;
                for (int i = 0; i < selectedFeaureDic.Keys.Count; i++)
                {
                    string featureIDClassName = selectedFeaureDic.Keys.ToArray()[i];
                    IFeatureLayer ppFeatureLayer = selectedFeaureDic[featureIDClassName].featureLayer;
                    int featureID =Convert.ToInt32(featureIDClassName.Split(new char[]{'_'},StringSplitOptions.RemoveEmptyEntries)[0]);
                    IFeature ppFeature = ppFeatureLayer.FeatureClass.GetFeature(featureID);
                    IEnvelope en = ppFeature.Shape.Envelope;
                    m_Application.MapControl.Map.SelectFeature(ppFeatureLayer, ppFeature);
                    if (refreshEnvelop == null)
                        refreshEnvelop = en;
                    else
                    {
                        if (en.XMax > refreshEnvelop.XMax)
                            refreshEnvelop.XMax = en.XMax;
                        if (en.YMax > refreshEnvelop.YMax)
                            refreshEnvelop.YMax = en.YMax;
                        if (en.XMin < refreshEnvelop.XMin)
                            refreshEnvelop.XMin = en.XMin;
                        if (en.YMin < refreshEnvelop.YMin)
                            refreshEnvelop.YMin = en.YMin;
                    }
                }
                if (refreshEnvelop != null)
                {
                    refreshEnvelop.Expand(1.2, 1.2, true);
                    m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, refreshEnvelop);
                }
                isClickUnSelect = true;
            }
            /*-----------------------右键 属性框弹出---------------------------------------------------*/
            if (button == 2 && selectedFeaureDic.Count > 0)
            {
                //查找共有属性
                AnnoStyle commonStyle = null;
                foreach (string featureIDClassName in selectedFeaureDic.Keys)
                {
                    #region
                    IFeatureLayer ppFeatureLayer = selectedFeaureDic[featureIDClassName].featureLayer;
                    int featureID = Convert.ToInt32(featureIDClassName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    IFeature feature = ppFeatureLayer.FeatureClass.GetFeature(featureID);
                    IAnnotationFeature2 annotationFeature = feature as IAnnotationFeature2;
                    IElement element = annotationFeature.Annotation;
                    if (element is IGroupElement)
                    {
                        MessageBox.Show("所选择注记为注记组，请先执行打散操作！");
                        return;
                    }
                    else if (element is IFillShapeElement)
                    {
                        MessageBox.Show("请选择文本注记！");
                        return;
                    }
                    //剩下的为文本注记，问题出在引用传递。。。请改为值传递
                    
                    //初始化一个
                    commonStyle = GetStyleByFeature(feature);
                    if (selectedFeaureDic.Count > 1)
                    {
                        //多个注记时，以下选项默认为空
                        commonStyle.Text = "";
                        commonStyle.FontName = "";
                        commonStyle.FontSize = 0;
                    }
                    if (commonStyle == null)
                        return;
                    selectedFeaureDic[featureIDClassName].annoStyle = commonStyle.Clone();
                    
                    #endregion
                }
              
                AnnoAttribute AnnoAttri = new AnnoAttribute(m_Application.MapControl.hWnd,  commonStyle);
                AnnoAttri.selectedFeaureDic = selectedFeaureDic;
                //弹窗显示
                if (AnnoAttri.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    UpdateAnno(commonStyle, AnnoAttri.OutAnnoStyle as AnnoStyle);
                }
            }
        }
        public override void Refresh(int hdc)
        {
            currentTool.Refresh(hdc);

            #region Li
            if (_textFeedback != null)
            {
                _textFeedback.Refresh(hdc);
            }
            #endregion
        }

        #region Li
        private void ClearPartElement()
        {
            //清除多部件选择
            if (_selMultipartAnnoFe != null)
            {
                if (_selectedElement != null)
                {
                    //清理临时图形元素
                    m_Application.MapControl.ActiveView.GraphicsContainer.DeleteElement(_selectedElement as IElement);
                    _selectedElement = null;
                }

                if (_textFeedback != null)
                {
                    _textFeedback.Stop();
                    _textFeedback.Refresh(m_Application.ActiveView.ScreenDisplay.hDC);
                    _textFeedback = null;
                }

                _selMultipartAnnoFe = null;
                _selPartGeoIndex = -1;
                _selPartTextIndex = -1;
            }
        }

        /// <summary>
        /// 更新注记属性
        /// </summary>
        /// <param name="fe"></param>
        /// <param name="oldStyle"></param>
        /// <param name="newStyle"></param>
        private void UpdateAnnoFeature(IFeature fe, AnnoStyle oldStyle, AnnoStyle newStyle)
        {
            IAnnotationFeature2 annoFeature = fe as IAnnotationFeature2;
            ITextElement textElement = annoFeature.Annotation as ITextElement;

            if (newStyle.Text.Trim() != "" && oldStyle.Text != newStyle.Text)
            {
                #region 文本
                textElement.Text = newStyle.Text;
                #endregion
            }

            if (newStyle.FontName != "" && oldStyle.FontName != newStyle.FontName)
            {
                #region 字体
                System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
                foreach (System.Drawing.FontFamily ff in fonts.Families)
                {
                    if (ff.Name == newStyle.FontName)
                    {
                        (textElement as ISymbolCollectionElement).FontName = newStyle.FontName;
                    }
                }
                #endregion
            }

            if (newStyle.FontSize > 0 && oldStyle.FontSize != newStyle.FontSize)
            {
                #region 字体大小
                (textElement as ISymbolCollectionElement).Size = newStyle.FontSize * 2.8345;
                #endregion
            }

            if (oldStyle.FontColorCMYK != newStyle.FontColorCMYK)
            {
                #region 字体颜色
                (textElement as ISymbolCollectionElement).Color = GetColorByString(newStyle.FontColorCMYK);
                #endregion
            }

            if (oldStyle.FontBold != newStyle.FontBold)
            {
                #region 加粗
                (textElement as ISymbolCollectionElement).Bold = newStyle.FontBold;
                #endregion
            }

            if (oldStyle.Horizontal != newStyle.Horizontal || oldStyle.Vertical != newStyle.Vertical)
            {
                #region 对齐方式
                (textElement as ISymbolCollectionElement).HorizontalAlignment = newStyle.Horizontal;
                (textElement as ISymbolCollectionElement).VerticalAlignment = newStyle.Vertical;
                #endregion
            }

            #region 注记背景
            if (newStyle.HasMask)
            {
                ITextSymbol textSymbol = textElement.Symbol;
                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;

                IBalloonCallout balloonCallout = new BalloonCalloutClass();
                balloonCallout.Style = newStyle.BalloonCallout;

                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = GetColorByString(newStyle.MaskColorCMYK);

                ILineSymbol lineSymbol = new SimpleLineSymbol();
                lineSymbol.Width = newStyle.LineWidth;
                lineSymbol.Color = GetColorByString(newStyle.LineColorCMYK);

                fillSymbol.Outline = lineSymbol;
                balloonCallout.Symbol = fillSymbol;


                ITextMargins textMarigns = balloonCallout as ITextMargins;
                textMarigns.PutMargins(newStyle.TextMarginsLeft, newStyle.TextMarginsUp, newStyle.TextMarginsRight, newStyle.TextMarginsDown);

                (textSymbol as IFormattedTextSymbol).Background = balloonCallout as ITextBackground;
                textElement.Symbol = textSymbol;
            }
            else
            {
                ITextSymbol textSymbol = textElement.Symbol;
                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;

                (textSymbol as IFormattedTextSymbol).Background = null;
                textElement.Symbol = textSymbol;
            }

            #endregion

            if (oldStyle.TextMaskStyle != newStyle.TextMaskStyle)
            {
                #region 蒙板类型
                ITextSymbol textSymbol = textElement.Symbol;
                (textSymbol as IMask).MaskStyle = newStyle.TextMaskStyle;

                textElement.Symbol = textSymbol;
                #endregion
            }

            if (oldStyle.TextMaskSize != newStyle.TextMaskSize)
            {
                #region 蒙板大小
                ITextSymbol textSymbol = textElement.Symbol;
                (textSymbol as IMask).MaskSize = newStyle.TextMaskSize;

                textElement.Symbol = textSymbol;
                #endregion
            }

            if (newStyle.HasMask&&(oldStyle.TextMaskColor != newStyle.TextMaskColor))
            {
                #region 蒙板颜色
                ITextSymbol textSymbol = textElement.Symbol;
                if (textSymbol is IMask && (textSymbol as IMask).MaskSymbol != null)
                {
                    IFillSymbol fillSymbol = (textSymbol as IMask).MaskSymbol;
                    fillSymbol.Color = GetColorByString(newStyle.TextMaskColor);

                    (textSymbol as IMask).MaskSymbol = fillSymbol;
                    textElement.Symbol = textSymbol;
                }
                #endregion
            }

            annoFeature.Annotation = textElement as IElement;
            fe.Store();
        }
        #endregion
    }
}

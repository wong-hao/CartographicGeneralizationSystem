using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.AnnotationEngine
{
    /// <summary>
    /// 注记编辑（LZ）
    /// </summary>
    public class AnnotationEditTool : SMGI.Common.SMGITool
    {
        private ControlsEditingEditToolClass _editTool;
        private Dictionary<IFeature, AnnotationAttribute> _selFe2Attri;
        private bool _isPressCtrl;
        private bool _isClickUnSelect;

        #region 多部件选择相关
        private IFeature _selMultipartAnnoFe;
        private int _selPartGeoIndex;
        private int _selPartTextIndex;
        private IElement _selectedElement;
        private IMovePolygonFeedback _textFeedback;
        #endregion

        public AnnotationEditTool()
        {
            m_caption = "注记编辑";

            _editTool = new ControlsEditingEditToolClass();
            _selFe2Attri = new Dictionary<IFeature, AnnotationAttribute>();
            _isPressCtrl = false;
            _isClickUnSelect = true;

            #region Li
            _selMultipartAnnoFe = null;
            _selPartGeoIndex = -1;
            _selPartTextIndex = -1;
            _selectedElement = null;
            _textFeedback = null;
            #endregion

            NeedSnap = false;
        }

        public override bool Enabled
        {
            get 
            { 
                return m_Application != null && m_Application.Workspace != null && 
                    m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateEditing; 
            }
        }

        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            _editTool.OnCreate(m_Application.MapControl.Object);

            ((IEngineEditEvents_Event)m_Application.EngineEditor).OnDeleteFeature += new IEngineEditEvents_OnDeleteFeatureEventHandler(EngineEditor_OnDeleteFeature);
            
        }

        public override bool Deactivate()
        {
            m_Application.MapControl.OnSelectionChanged -= new EventHandler(MapControl_OnSelectionChanged);

            ClearPartElement();

            _selFe2Attri.Clear();
            return _editTool.Deactivate();
        }

        public override void OnClick()
        {
            _editTool.OnClick();
            _selFe2Attri.Clear();

            m_Application.MapControl.OnSelectionChanged += new EventHandler(MapControl_OnSelectionChanged);

        }

        public override bool OnContextMenu(int x, int y)
        {
            return _editTool.OnContextMenu(x, y);
        }

        public override void OnDblClick()
        {
            _editTool.OnDblClick();
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
            _editTool.OnKeyDown(keyCode, shift);
            if (keyCode == (int)Keys.ControlKey) _isPressCtrl = true;
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            _editTool.OnKeyUp(keyCode, shift);
            if (keyCode == (int)Keys.ControlKey) _isPressCtrl = false;
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            #region 多部件注记:左键单击的位置是已选择的唯一注记要素，且该注记要素为多部件注记，则进行处理
            if (button == 1)
            {
                ClearPartElement();

                IFeature objFe = null;
                IMultiPartTextElement multiPartTextElement = null;
                if (_selFe2Attri.Count == 1)
                {
                    objFe = _selFe2Attri.First().Key;
                    var objAnnoFe = objFe as IAnnotationFeature2;
                    if (objAnnoFe != null && objAnnoFe.Annotation is IMultiPartTextElement)
                    {
                        multiPartTextElement = objAnnoFe.Annotation as IMultiPartTextElement;
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
                            #region 如何有效的根据要素部件几何确定text部件索引？？？
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
                                if (textRO.Within(partGeo))//有缺陷
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
                            _selPartGeoIndex = i;//要素几何部件索引（索引根据角点坐标，先y降序再x降序？？）

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

            _editTool.OnMouseDown(button, shift, x, y);
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            #region 拖动选择的多部件
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

            _editTool.OnMouseMove(button, shift, x, y);
        }

        public override void OnMouseUp(int button, int shift, int x, int y)
        {
            _editTool.OnMouseUp(button, shift, x, y);

            
            if (button == 1 && _textFeedback != null)
            {
                #region 多部件注记：更新注记位置
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

                _selMultipartAnnoFe.Store();

                //更新临时元素
                _selectedElement.Geometry = newGeo;
                m_Application.MapControl.ActiveView.GraphicsContainer.UpdateElement(_selectedElement);
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, _selectedElement, null);
                #endregion
            }
            

            if (button == 1)//左键
            {
                #region 要素选择
                //将环境选择元素置为红色
                ISelectionEnvironment selEnv = new SelectionEnvironmentClass(){DefaultColor = new RgbColorClass { Red = 255 }};

                //设置选择
                if (!_isPressCtrl)
                    _selFe2Attri.Clear();

                var enumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
                enumFeature.Reset();
                IFeature fe;
                while ((fe = enumFeature.Next()) != null)
                {
                    if (fe.FeatureType != esriFeatureType.esriFTAnnotation) 
                        continue;

                    if (_selFe2Attri.ContainsKey(fe))
                    {
                        _selFe2Attri.Remove(fe);
                    }
                    else
                    {
                        _selFe2Attri.Add(fe, null);
                    }
                }
                Marshal.ReleaseComObject(enumFeature);

                _isClickUnSelect = false;
                m_Application.MapControl.Map.ClearSelection();

                //获取所选要素集的外包框
                IEnvelope env = new EnvelopeClass();
                foreach (var kv in _selFe2Attri)
                {
                    var layer = m_Application.Workspace.LayerManager.GetLayer(l => l is IFeatureLayer && ((IFeatureLayer)l).FeatureClass.ObjectClassID == kv.Key.Class.ObjectClassID).FirstOrDefault() as IFeatureLayer;
                    if (layer == null) 
                        continue;

                    m_Application.MapControl.Map.SelectFeature(layer, kv.Key);
                    env.Union(kv.Key.Shape.Envelope);
                }
                if (!env.IsEmpty)
                {
                    env.Expand(1.2, 1.2, true);
                    m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, env);
                }

                _isClickUnSelect = true;
                #endregion
            }

            if (button == 2 && _selFe2Attri.Count > 0)//右键
            {
                #region 属性框
                for (int i = 0; i < _selFe2Attri.Count; ++i)
                {
                    var kv = _selFe2Attri.ElementAt(i);
                    AnnotationAttribute annoAttri = GetAnnoFeatureAttribute(kv.Key as IAnnotationFeature);
                    _selFe2Attri[kv.Key] = annoAttri;
                }

                var frm = new AnnotationAttributeForm(m_Application.MapControl.hWnd, _selFe2Attri);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    m_Application.EngineEditor.StartOperation();

                    try
                    {
                        foreach (var kv in _selFe2Attri)
                        {
                            UpdateAnnoFeature(kv.Key, kv.Value, frm.OutAnnoAttribute);

                            m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, kv.Key, null);
                        }

                        m_Application.EngineEditor.StopOperation("注记编辑");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        m_Application.EngineEditor.AbortOperation();
                    }

                }
                #endregion
            }
        }

        public override void Refresh(int hdc)
        {
            _editTool.Refresh(hdc);

            if (_textFeedback != null)
            {
                _textFeedback.Refresh(hdc);
            }
        }

        //要素删除时删除对应注记
        private void EngineEditor_OnDeleteFeature(IObject Object)
        {
            var delFe = Object as IFeature;
            if (delFe == null || delFe.FeatureType == esriFeatureType.esriFTAnnotation) 
                return;

            var lys = m_Application.Workspace.LayerManager.GetLayer(l => l is IFDOGraphicsLayer).ToList();
            IQueryFilter qf = new QueryFilterClass { WhereClause = "AnnotationClassID = " + delFe.Class.ObjectClassID + " and FeatureID = " + delFe.OID };
            var env = delFe.Extent;
            foreach (var item in lys)
            {
                var feCursor = (item as IFeatureLayer).FeatureClass.Search(qf, false);
                IFeature fe;
                while ((fe = feCursor.NextFeature()) != null)
                {
                    env.Union(fe.Extent);
                    fe.Delete();
                }
                Marshal.ReleaseComObject(feCursor);
            }
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, env);

            GC.Collect();
        }

        //选择要素改变事件
        private void MapControl_OnSelectionChanged(object sender, EventArgs e)
        {
            //获取选择集
            m_Application.ActiveView.GraphicsContainer.Reset();
            IElement delEle;
            while ((delEle = m_Application.ActiveView.GraphicsContainer.Next()) != null)
            {
                var delElePro = delEle as IElementProperties;
                if (delElePro.Name == "注记关联要素") 
                    m_Application.ActiveView.GraphicsContainer.DeleteElement(delEle);
            }

            //当点击清除选择集的时候执行
            if (_isClickUnSelect && m_Application.MapControl.Map.SelectionCount == 0)
            {
                _selFe2Attri.Clear();
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                return;
            }
            if (!(m_Application.MapControl.CurrentTool is AnnotationEditTool)) 
                return;

            var enumFeature = m_Application.MapControl.Map.FeatureSelection as IEnumFeature;
            enumFeature.Reset();
            IFeature fe;
            while ((fe = enumFeature.Next()) != null)
            {
                if (fe.FeatureType != esriFeatureType.esriFTAnnotation) 
                    continue;
                IAnnotationFeature2 annoFe = fe as IAnnotationFeature2;
                int cid = annoFe.AnnotationClassID;
                int fid = annoFe.LinkedFeatureID;

                var layer = m_Application.Workspace.LayerManager.GetLayer(l => l is IGeoFeatureLayer && ((IFeatureLayer)l).FeatureClass.ObjectClassID == Convert.ToInt32(cid)).FirstOrDefault() as IFeatureLayer;
                if (layer == null) 
                    continue;
                var fc = layer.FeatureClass;
                var linkedFe = fc.GetFeature(fid);
                if (linkedFe == null)
                    continue;

                //
                IColor color = new RgbColorClass { Red = 0, Green = 255, Blue = 255 };
                IElement element = null;
                if (linkedFe.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                {
                    ISimpleMarkerSymbol sms = new SimpleMarkerSymbolClass { Style = esriSimpleMarkerStyle.esriSMSCircle, Size = 2, Color = color };
                    IMarkerElement me = new MarkerElementClass { Symbol = sms };
                    element = me as IElement;
                }
                else if (linkedFe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                {
                    ISimpleLineSymbol sls = new SimpleLineSymbolClass { Style = esriSimpleLineStyle.esriSLSSolid, Color = color, Width = 1 };
                    ILineElement le = new LineElementClass { Symbol = sls };
                    element = le as IElement;
                }
                else if (linkedFe.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    ISimpleLineSymbol sls = new SimpleLineSymbolClass { Style = esriSimpleLineStyle.esriSLSSolid, Color = color, Width = 1 };
                    ISimpleFillSymbol sfs = new SimpleFillSymbolClass { Style = esriSimpleFillStyle.esriSFSNull, Color = color, Outline = sls };
                    IFillShapeElement fse = new PolygonElementClass { Symbol = sfs };
                    element = fse as IElement;
                }

                if (element != null)
                {
                    element.Geometry = linkedFe.Shape;
                    var elePro = (IElementProperties)element;
                    elePro.Name = "注记关联要素";
                    m_Application.ActiveView.GraphicsContainer.AddElement(element, 0);
                }
            }
            Marshal.ReleaseComObject(enumFeature);
        }

        /// <summary>
        /// 获取注记要素属性
        /// </summary>
        /// <param name="annoFe"></param>
        /// <returns></returns>
        private AnnotationAttribute GetAnnoFeatureAttribute(IAnnotationFeature annoFe)
        {
            AnnotationAttribute attri = new AnnotationAttribute();

            ITextElement textElement = annoFe.Annotation as ITextElement;
            attri.Text = textElement.Text;
            attri.FontName = textElement.Symbol.Font.Name;
            attri.FontSize = textElement.Symbol.Size / 2.8345;
            attri.FontColor = textElement.Symbol.Color;

            attri.EnableBold = (textElement as ISymbolCollectionElement).Bold;
            attri.HorizontalAlignment = (textElement.Symbol as IFormattedTextSymbol).HorizontalAlignment;
            attri.VerticalAlignment = (textElement.Symbol as IFormattedTextSymbol).VerticalAlignment;
            attri.AnnoStatus = (annoFe as IAnnotationFeature2).Status;
            attri.CJKCharactersRotation = (textElement.Symbol as ICharacterOrientation).CJKCharactersRotation;

            attri.EnableTextBackground = ((textElement.Symbol as IFormattedTextSymbol).Background != null);
            if (attri.EnableTextBackground)
            {
                IBalloonCallout balloonCallout = (textElement.Symbol as IFormattedTextSymbol).Background as IBalloonCallout;
                if (balloonCallout != null)
                {
                    attri.BackgroundStyle = balloonCallout.Style;
                    attri.BackgroundColor = balloonCallout.Symbol.Color;
                    attri.BackgroundBorderWidth = balloonCallout.Symbol.Outline.Width;
                    attri.BackgroundBorderColor = balloonCallout.Symbol.Outline.Color;
                    attri.BackgroundMarginLeft = (balloonCallout as ITextMargins).LeftMargin;
                    attri.BackgroundMarginUp = (balloonCallout as ITextMargins).TopMargin;
                    attri.BackgroundMarginRight = (balloonCallout as ITextMargins).RightMargin;
                    attri.BackgroundMarginDown = (balloonCallout as ITextMargins).BottomMargin;
                }
            }

            attri.MaskStyle = (textElement.Symbol as IMask).MaskStyle;
            attri.MaskSize = (textElement.Symbol as IMask).MaskSize;
            if ((textElement.Symbol as IMask).MaskSymbol != null)
                attri.MaskColor = (textElement.Symbol as IMask).MaskSymbol.Color;

            return attri;
        }

        /// <summary>
        /// 更新注记属性
        /// </summary>
        /// <param name="fe"></param>
        /// <param name="oldStyle"></param>
        /// <param name="newStyle"></param>
        private void UpdateAnnoFeature(IFeature fe, AnnotationAttribute oldAttri, AnnotationAttribute newAttri)
        {
            IAnnotationFeature2 annoFeature = fe as IAnnotationFeature2;
            ITextElement textElement = annoFeature.Annotation as ITextElement;

            //可以不修改
            if (newAttri.Text.Trim() != "" && oldAttri.Text != newAttri.Text)
            {
                #region 文本
                textElement.Text = newAttri.Text;
                #endregion
            }

            if (newAttri.FontName != "" && oldAttri.FontName != newAttri.FontName)
            {
                #region 字体
                System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
                foreach (System.Drawing.FontFamily ff in fonts.Families)
                {
                    if (ff.Name == newAttri.FontName)
                    {
                        (textElement as ISymbolCollectionElement).FontName = newAttri.FontName;
                    }
                }
                #endregion
            }

            if (newAttri.FontSize > 0 && oldAttri.FontSize != newAttri.FontSize)
            {
                #region 字体大小
                (textElement as ISymbolCollectionElement).Size = newAttri.FontSize * 2.8345;
                #endregion
            }

            if (newAttri.FontColor != null && oldAttri.FontColor != newAttri.FontColor)
            {
                #region 字体颜色
                (textElement as ISymbolCollectionElement).Color = newAttri.FontColor;
                #endregion
            }



            //直接修改
            if (oldAttri.HorizontalAlignment != newAttri.HorizontalAlignment || oldAttri.VerticalAlignment != newAttri.VerticalAlignment)
            {
                #region 对齐方式
                (textElement as ISymbolCollectionElement).HorizontalAlignment = newAttri.HorizontalAlignment;
                (textElement as ISymbolCollectionElement).VerticalAlignment = newAttri.VerticalAlignment;
                #endregion
            }

            if (oldAttri.EnableBold != newAttri.EnableBold)
            {
                #region 加粗
                (textElement as ISymbolCollectionElement).Bold = newAttri.EnableBold;
                #endregion
            }

            if (oldAttri.CJKCharactersRotation != newAttri.CJKCharactersRotation)
            {
                #region CJK
                ITextSymbol textSymbol = textElement.Symbol;
                (textSymbol as ICharacterOrientation).CJKCharactersRotation = newAttri.CJKCharactersRotation;
                textElement.Symbol = textSymbol;
                #endregion
            }

            #region 文本背景
            if (newAttri.EnableTextBackground)
            {
                ITextSymbol textSymbol = textElement.Symbol;
                IFormattedTextSymbol formattedTextSymbol = textSymbol as IFormattedTextSymbol;

                IBalloonCallout balloonCallout = new BalloonCalloutClass();
                balloonCallout.Style = newAttri.BackgroundStyle;

                IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = newAttri.BackgroundColor;

                ILineSymbol lineSymbol = new SimpleLineSymbol();
                lineSymbol.Width = newAttri.BackgroundBorderWidth;
                lineSymbol.Color = newAttri.BackgroundBorderColor;

                fillSymbol.Outline = lineSymbol;
                balloonCallout.Symbol = fillSymbol;


                ITextMargins textMarigns = balloonCallout as ITextMargins;
                textMarigns.PutMargins(newAttri.BackgroundMarginLeft, newAttri.BackgroundMarginUp, newAttri.BackgroundMarginRight, newAttri.BackgroundMarginDown);

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

            if (oldAttri.MaskStyle != newAttri.MaskStyle)
            {
                #region 蒙板类型
                ITextSymbol textSymbol = textElement.Symbol;
                (textSymbol as IMask).MaskStyle = newAttri.MaskStyle;

                textElement.Symbol = textSymbol;
                #endregion
            }

            if (oldAttri.MaskSize != newAttri.MaskSize)
            {
                #region 蒙板大小
                ITextSymbol textSymbol = textElement.Symbol;
                (textSymbol as IMask).MaskSize = newAttri.MaskSize;

                textElement.Symbol = textSymbol;
                #endregion
            }

            if (oldAttri.MaskColor != newAttri.MaskColor)
            {
                #region 蒙板颜色
                ITextSymbol textSymbol = textElement.Symbol;
                if (newAttri.MaskColor == null)
                {
                    (textSymbol as IMask).MaskSymbol = null;
                }
                else
                {
                    IFillSymbol fillSymbol = (textSymbol as IMask).MaskSymbol;
                    if (fillSymbol == null)
                    {
                        fillSymbol = new SimpleFillSymbol() { Outline = new SimpleLineSymbol() { Width = 0 } };
                    }
                    fillSymbol.Color = newAttri.MaskColor;

                    (textSymbol as IMask).MaskSymbol = fillSymbol;
                }
                textElement.Symbol = textSymbol;
                #endregion
            }

            annoFeature.Annotation = textElement as IElement;
            if (oldAttri.AnnoStatus != newAttri.AnnoStatus)
            {
                annoFeature.Status = newAttri.AnnoStatus;
            }
            fe.Store();
        }

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

    }

    //右键属性菜单可修改的注记属性信息
    public class AnnotationAttribute
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text
        {
            set;
            get;
        }

        #region 字体信息
        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName
        {
            get;
            set;
        }
        /// <summary>
        /// 字体大小（默认2.0，单位：毫米）
        /// </summary>
        public double FontSize
        {
            set;
            get;
        }
        /// <summary>
        /// 字体颜色
        /// </summary>
        public IColor FontColor
        {
            get;
            set;
        }
        #endregion

        #region 基本属性
        /// <summary>
        /// 是否加粗（默认为false）
        /// </summary>
        public bool EnableBold
        {
            set;
            get;
        }
        /// <summary>
        /// 文本水平对齐
        /// </summary>
        public esriTextHorizontalAlignment HorizontalAlignment
        {
            set;
            get;
        }
        /// <summary>
        /// 文本垂直对齐
        /// </summary>
        public esriTextVerticalAlignment VerticalAlignment
        {
            set;
            get;
        }
        /// <summary>
        /// 注记放置状态
        /// </summary>
        public esriAnnotationStatus AnnoStatus
        {
            set;
            get;
        }
        /// <summary>
        /// CJK字符方向
        /// </summary>
        public bool CJKCharactersRotation
        {
            set;
            get;
        }
        #endregion

        #region 文本背景
        /// <summary>
        /// 是否开启文本背景（气泡，默认为false）
        /// </summary>
        public bool EnableTextBackground
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡样式
        /// </summary>
        public esriBalloonCalloutStyle BackgroundStyle
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡背景色
        /// </summary>
        public IColor BackgroundColor
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡边界宽度（单位：毫米）
        /// </summary>
        public double BackgroundBorderWidth
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡边界颜色
        /// </summary>
        public IColor BackgroundBorderColor
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡上边框距（单位：毫米）
        /// </summary>
        public double BackgroundMarginUp
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡下边框距（单位：毫米）
        /// </summary>
        public double BackgroundMarginDown
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡左边框距（单位：毫米）
        /// </summary>
        public double BackgroundMarginLeft
        {
            set;
            get;
        }
        /// <summary>
        /// 气泡右边框距（单位：毫米）
        /// </summary>
        public double BackgroundMarginRight
        {
            set;
            get;
        }
        #endregion

        #region 蒙板
        /// <summary>
        /// 蒙板，如晕圈等
        /// </summary>
        public esriMaskStyle MaskStyle
        {
            set;
            get;
        }
        /// <summary>
        /// 蒙板大小（单位：毫米）
        /// </summary>
        public double MaskSize
        {
            set;
            get;
        }

        /// <summary>
        /// 蒙板颜色
        /// </summary>
        public IColor MaskColor
        {
            set;
            get;
        }
        #endregion


        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public AnnotationAttribute Clone()
        {
            AnnotationAttribute attri = new AnnotationAttribute();

            //利用反射进行克隆
            System.Reflection.PropertyInfo[] PropertyInfos = this.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo PropertyInfo in PropertyInfos)
            {
                object v1 = PropertyInfo.GetValue(this, null);
                PropertyInfo.SetValue(attri, v1, null);
            }

            return attri;
        }
    }
}

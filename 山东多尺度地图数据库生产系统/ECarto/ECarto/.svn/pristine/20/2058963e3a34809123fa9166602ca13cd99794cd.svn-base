using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.EmergencyMap
{
    public class ClipElement
    {
        public static void SetClipGroupElement(GApplication app, IGroupElement pGroupElement)
        {

            IGroupElement ge = getClipGroupElement(app.MapControl.ActiveView.GraphicsContainer);
            if (ge != null)
            {
                //清理裁切图形元素
                app.MapControl.ActiveView.GraphicsContainer.DeleteElement(ge as IElement);
            }

            if (pGroupElement == null)
                return;

            app.MapControl.ActiveView.GraphicsContainer.AddElement(pGroupElement as IElement, 0);

            //更新视图
            IElement clipRange = GetClipRangeElement(app.MapControl.ActiveView.GraphicsContainer);
            if (clipRange != null)
            {
                IEnvelope env = clipRange.Geometry.Envelope;
                env.Expand(1.6, 1.6, true);
                app.MapControl.Extent = env;
            }
            app.MapControl.ActiveView.Refresh();

        }

        public static IGroupElement getClipGroupElement(IGraphicsContainer pGraphicsContainer)
        {
            if (pGraphicsContainer == null)
                return null;

            pGraphicsContainer.Reset();
            IElement pElement = pGraphicsContainer.Next();
            while (pElement != null)
            {
                if (pElement is IGroupElement)
                {
                    IGroupElement ge = pElement as IGroupElement;

                    for (int i = 0; i < ge.ElementCount; i++)
                    {
                        IElement el = ge.get_Element(i);
                        IElementProperties3 eleProp = el as IElementProperties3;
                        if ("裁切范围" == eleProp.Name)
                        {
                            return ge;
                        }
                    }
                }

                pElement = pGraphicsContainer.Next();
            }

            return null;

        }

        public static IElement GetClipRangeElement(IGraphicsContainer pGraphicsContainer)
        {
            IGroupElement ge = getClipGroupElement(pGraphicsContainer);

            if (ge == null)
                return null;

            for (int i = 0; i < ge.ElementCount; i++)
            {
                IElement el = ge.get_Element(i);
                IElementProperties3 eleProp = el as IElementProperties3;
                if ("裁切范围" == eleProp.Name)
                {
                    return el;
                }
            }

            return null;
        }

        /// <summary>
        /// 创建裁切元素wjz修改（在目标空间参考中创建裁切框，再投影至map中显示）
        /// </summary>
        /// <param name="centerPoint">中心点</param>
        /// <param name="paperWidth">纸张宽度（mm）</param>
        /// <param name="paperHeight">纸张高度（mm）</param>
        /// <param name="refScale">比例尺</param>
        /// <returns></returns>
        public static IGroupElement createClipGroupElement(GApplication app, IPoint centerPoint, double paperWidth, double paperHeight, double refScale)
        {
            //目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = app.MapControl.Map.SpatialReference;

            //在目标空间参考系中生成裁切元素
            IMap pTempMap = new MapClass();
            pTempMap.SpatialReference = targetSpatialReference;
            UnitConverterClass unitConverter = new UnitConverterClass();
            double width = unitConverter.ConvertUnits((paperWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//宽度
            double height = unitConverter.ConvertUnits((paperHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//高度

            if (centerPoint.SpatialReference.Name != targetSpatialReference.Name)
            {
                (centerPoint as IGeometry).Project(targetSpatialReference);//对裁切中心点进行投影变换
            }
            //生成边框多边形
            double xMax = centerPoint.X + width / 2;
            double yMax = centerPoint.Y + height / 2;
            double xMin = centerPoint.X - width / 2;
            double yMin = centerPoint.Y - height / 2;
            IPolygon ply = CreatePolygon(xMin, xMax, yMin, yMax);
            ply.SpatialReference = targetSpatialReference;





            //添加图形元素
            IGroupElement pGroupElement = new GroupElementClass();
        
            if (null == targetSpatialReference)
                targetSpatialReference = app.MapControl.Map.SpatialReference;



            double Inlinewidth = unitConverter.ConvertUnits((CommonMethods.InlineWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓宽度
            double Inlineheight = unitConverter.ConvertUnits((CommonMethods.InlineHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓高度
            double MapSizeWidth = unitConverter.ConvertUnits((CommonMethods.MapSizeWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//成图尺寸宽度（实地——
            double MapSizeHeight = unitConverter.ConvertUnits((CommonMethods.MapSizeHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//成图尺寸高度（实地——
            double PageWidth = unitConverter.ConvertUnits((CommonMethods.PaperWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//页面尺寸宽度（实地——
            double PageHeight = unitConverter.ConvertUnits((CommonMethods.PaperHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//页面尺寸高度（实地——
            double InOutLineWidth = unitConverter.ConvertUnits((CommonMethods.InOutLineWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内外图廓间距（实地——
       
         
            
            //生成内图廓边框多边形
            double InlinexMax = centerPoint.X + Inlinewidth / 2;
            double InlineyMax = centerPoint.Y + Inlineheight / 2;
            double InlinexMin = centerPoint.X - Inlinewidth / 2;
            double InlineyMin = centerPoint.Y - Inlineheight / 2;
            IPolygon Inlineply = CreatePolygon(InlinexMin, InlinexMax, InlineyMin, InlineyMax);
            Inlineply.SpatialReference = targetSpatialReference;
            //生成外图廓边框多边形
            double OutlinexMax = InlinexMax + InOutLineWidth;
            double OutlineyMax = InlineyMax + InOutLineWidth;
            double OutlinexMin = InlinexMin - InOutLineWidth;
            double OutlineyMin = InlineyMin - InOutLineWidth;
            IPolygon Outlineply = CreatePolygon(OutlinexMin, OutlinexMax, OutlineyMin, OutlineyMax);
            Outlineply.SpatialReference = targetSpatialReference;

            //生成成图尺寸多边形,默认下、左、右留白一样
            double HSpace = (MapSizeWidth - Inlinewidth) / 2;
            double VSpaceUP = (MapSizeHeight - Inlineheight) - (MapSizeWidth - Inlinewidth) / 2;
            double VSpaceDown = HSpace;
            if (VSpaceUP < VSpaceDown)
            {
                VSpaceUP = (MapSizeHeight - Inlineheight) * 0.67;
                VSpaceDown = (MapSizeHeight - Inlineheight) * 0.33;
            }
            double MapsizexMax = InlinexMax + HSpace;
            double MapsizeyMax = InlineyMax + VSpaceUP;
            double MapsizexMin = InlinexMin - HSpace;
            double MapsizeyMin = InlineyMin - VSpaceDown;
            IPolygon Mapsizeply = CreatePolygon(MapsizexMin, MapsizexMax, MapsizeyMin, MapsizeyMax);
            Mapsizeply.SpatialReference = targetSpatialReference;
            //生成页面尺寸
            double PagesizexMax = MapsizexMax + (PageWidth - MapSizeWidth) / 2;
            double PagesizeyMax = MapsizeyMax + (PageHeight - MapSizeHeight) / 2;
            double PagesizexMin = MapsizexMin - (PageWidth - MapSizeWidth) / 2;
            double PagesizeyMin = MapsizeyMin - (PageHeight - MapSizeHeight) / 2;
            IPolygon Pageply = CreatePolygon(PagesizexMin, PagesizexMax, PagesizeyMin, PagesizeyMax);
            Pageply.SpatialReference = targetSpatialReference;

            IGeometry cLipShp = (centerPoint as ITopologicalOperator).Buffer(1);
            IGeometry cLipShp2=Pageply;


            #region 裁切框中心点
            //中心点符号
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            pSimpleMarkerSymbol.Color = new RgbColorClass { Red = 255 };//颜色
            pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSSquare;//符号类型
            pSimpleMarkerSymbol.Size = 5;//大小
            pSimpleMarkerSymbol.Outline = true;//显示外框线
            pSimpleMarkerSymbol.OutlineColor = new RgbColorClass { Red = 255 };//外框线颜色
            pSimpleMarkerSymbol.OutlineSize = 3;//外框线的宽度

            IElement pMarkerElment = new MarkerElementClass();
            (centerPoint as IGeometry).Project(app.MapControl.Map.SpatialReference);//对裁切中心点进行投影变换
            pMarkerElment.Geometry = centerPoint as IGeometry;//几何数据
            (pMarkerElment as IElementProperties3).Name = "裁切中心点";//名称
            (pMarkerElment as IMarkerElement).Symbol = pSimpleMarkerSymbol;//符号
            #endregion
            #region 裁切范围         
            ISimpleLineSymbol pclipLineSymbol = new SimpleLineSymbolClass();
            pclipLineSymbol.Width = 1; //边框线宽
            pclipLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pclipFillSymbol2 = new SimpleFillSymbolClass();
            pclipFillSymbol2.Color = new RgbColorClass { NullColor = true };//填充颜色
            pclipFillSymbol2.Outline = pclipLineSymbol;//边框线

            IElement pClipElement2 = new PolygonElement();
            cLipShp2.Project(app.MapControl.Map.SpatialReference);//投影变换
            pClipElement2.Geometry = Pageply;//几何数据
            (pClipElement2 as IElementProperties3).Name = "裁切范围";//名称    
            (pClipElement2 as IFillShapeElement).Symbol = pclipFillSymbol2; //符号



            #endregion

            #region 中心点扩展
            //符号
            ISimpleLineSymbol pClipLineSymbol = new SimpleLineSymbolClass();
            pClipLineSymbol.Width = 2; //边框线宽
            pClipLineSymbol.Color = new RgbColorClass { Red = 255, Green = 61, Blue = 249 };
            IFillSymbol pClipFillSymbol = new SimpleFillSymbolClass();
            pClipFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pClipFillSymbol.Outline = pClipLineSymbol;//边框线

            IElement pClipElement = new PolygonElement();
            cLipShp.Project(app.MapControl.Map.SpatialReference);//投影变换
            pClipElement.Geometry = cLipShp;//几何数据
            //(pClipElement as IElementProperties3).Name = "裁切范围";//名称
            (pClipElement as IFillShapeElement).Symbol = pClipFillSymbol; //符号
            #endregion


            #region 内图廓范围
            //符号
            ISimpleLineSymbol pInlineLineSymbol = new SimpleLineSymbolClass();
            pInlineLineSymbol.Width = 1; //边框线宽
            pInlineLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pInlineFillSymbol = new SimpleFillSymbolClass();
            pInlineFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pInlineFillSymbol.Outline = pInlineLineSymbol;//边框线

            IElement pInlineElement = new PolygonElement();
            Inlineply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pInlineElement.Geometry = Inlineply;//几何数据
            (pInlineElement as IElementProperties3).Name = "内图廓";//名称
            (pInlineElement as IFillShapeElement).Symbol = pInlineFillSymbol; //符号
            #endregion
            #region 外图廓范围
            //符号
            ISimpleLineSymbol pOutlineLineSymbol = new SimpleLineSymbolClass();
            pOutlineLineSymbol.Width = 1; //边框线宽
            pOutlineLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pOutlineFillSymbol = new SimpleFillSymbolClass();
            pOutlineFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pOutlineFillSymbol.Outline = pOutlineLineSymbol;//边框线

            IElement pOutlineElement = new PolygonElement();
            Outlineply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pOutlineElement.Geometry = Outlineply;//几何数据
            (pOutlineElement as IElementProperties3).Name = "外图廓";//名称
            (pOutlineElement as IFillShapeElement).Symbol = pOutlineFillSymbol; //符号
            #endregion
            #region 成图范围
            //符号
            ISimpleLineSymbol pMapsizeLineSymbol = new SimpleLineSymbolClass();
            pMapsizeLineSymbol.Width = 1; //边框线宽
            pMapsizeLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pMapsizeFillSymbol = new SimpleFillSymbolClass();
            pMapsizeFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pMapsizeFillSymbol.Outline = pMapsizeLineSymbol;//边框线

            IElement pMapsizeElement = new PolygonElement();
            Mapsizeply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pMapsizeElement.Geometry = Mapsizeply;//几何数据
            (pMapsizeElement as IElementProperties3).Name = "成图范围";//名称
            (pMapsizeElement as IFillShapeElement).Symbol = pMapsizeFillSymbol; //符号
            #endregion
            #region 出版范围
            //符号
            ISimpleLineSymbol pPageLineSymbol = new SimpleLineSymbolClass();
            pPageLineSymbol.Width = 1; //边框线宽
            pPageLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pPageFillSymbol = new SimpleFillSymbolClass();
            pPageFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pPageFillSymbol.Outline = pPageLineSymbol;//边框线

            IElement pPageElement = new PolygonElement();
            Pageply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pPageElement.Geometry = Pageply;//几何数据
            (pPageElement as IElementProperties3).Name = "出版范围";//名称
            (pPageElement as IFillShapeElement).Symbol = pPageFillSymbol; //符号
            #endregion

            try
            {
                pGroupElement.AddElement(pClipElement);
                pGroupElement.AddElement(pClipElement2);
                pGroupElement.AddElement(pInlineElement);
                pGroupElement.AddElement(pMapsizeElement);
                pGroupElement.AddElement(pPageElement);
                pGroupElement.AddElement(pOutlineElement);
                pGroupElement.AddElement(pMarkerElment);
                #region 计算横向和纵向角度
                //纵向角度
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                fromPoint.X = InlinexMin; fromPoint.Y = InlineyMin;
                fromPoint.SpatialReference = targetSpatialReference;
                fromPoint.Project(app.MapControl.Map.SpatialReference);
                toPoint.X = InlinexMin; toPoint.Y = InlineyMax;
                toPoint.SpatialReference = targetSpatialReference;
                toPoint.Project(app.MapControl.Map.SpatialReference);
                double Yangle = GetAngle(fromPoint, toPoint);
                //计算横向角度
                fromPoint.X = InlinexMin; fromPoint.Y = InlineyMin;
                fromPoint.SpatialReference = targetSpatialReference;
                fromPoint.Project(app.MapControl.Map.SpatialReference);
                toPoint.X = InlinexMax; toPoint.Y = InlineyMin;
                toPoint.SpatialReference = targetSpatialReference;
                toPoint.Project(app.MapControl.Map.SpatialReference);
                double Xangle = GetAngle(fromPoint, toPoint);
                Xangle = 0;
                #endregion


                #region 边缘距离
                //上边缘距离
                IPolygon proPolygon = (cLipShp as IClone).Clone() as IPolygon;
                proPolygon.Project(targetSpatialReference);

                #region 裁切面与内图廓上边缘距离
                PolylineClass lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X, Y = proPolygon.Envelope.UpperLeft.Y - 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X, Y = proPolygon.Envelope.UpperRight.Y - 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                IPolyline line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                IPoint center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                PolylineClass lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                PolylineClass linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                var geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = center.X, Y = InlineyMax });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                IRgbColor prgb = new RgbColorClass();
                prgb.Red = 220;
                IArrowMarkerSymbol arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                double updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                var txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.3, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                TextElementClass txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                MarkerElementClass markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.FromPoint as IGeometry;
                //线
                var frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.FromPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                LineElementClass lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion
                #region 下边缘距离
                //下边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X, Y = proPolygon.Envelope.LowerLeft.Y + 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X, Y = proPolygon.Envelope.LowerRight.Y + 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = center.X, Y = InlineyMin });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.7, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.ToPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.ToPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion
                #region 左边缘距离
                //左边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X + 1, Y = proPolygon.Envelope.UpperLeft.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X + 1, Y = proPolygon.Envelope.LowerLeft.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = InlinexMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = InlinexMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = InlinexMin, Y = center.Y });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.FromPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.FromPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion
                #region 右边缘距离
                //右边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X - 1, Y = proPolygon.Envelope.UpperRight.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X - 1, Y = proPolygon.Envelope.LowerRight.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = InlinexMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = InlinexMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = InlinexMax, Y = center.Y });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;


                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.ToPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.ToPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion

                #region 内外图廓距离
                //上边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = InlinexMin + (InlinexMax - InlinexMin) / 3, Y = InlineyMax });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = InlinexMin + (InlinexMax - InlinexMin) / 3, Y = InlineyMax + InOutLineWidth });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;

                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.3, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "内外图廓间距" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;

                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                #endregion



                #region 外图廓距离裁切线间距
                //上边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = OutlineyMax });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = MapsizeyMax });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                CommonMethods.MapSizeTopInterval = updis;
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);





                //右边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = OutlinexMax, Y = (OutlineyMin + OutlineyMax) / 2 });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = MapsizexMax, Y = (OutlineyMin + OutlineyMax) / 2 });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);



                //左边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = OutlinexMin, Y = (OutlineyMin + OutlineyMax) / 2 });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = MapsizexMin, Y = (OutlineyMin + OutlineyMax) / 2 });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
              
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);



                //下边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = OutlineyMin });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = MapsizeyMin });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                CommonMethods.MapSizeDownInterval = updis;
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);

                #endregion

                #region 框线文字
                txtPoint = new PointClass();
                txtPoint.X = InlinexMax; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "内\n图\n廓";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtPoint.X = OutlinexMin; txtPoint.Y = OutlineyMin + (OutlineyMax - OutlineyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外\n图\n廓";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtPoint.X = MapsizexMin; txtPoint.Y = MapsizeyMin + (MapsizeyMax - MapsizeyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "成\n图\n尺\n寸";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtPoint.X = PagesizexMax; txtPoint.Y = PagesizeyMin + (PagesizeyMax - PagesizeyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "纸\n张";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                #endregion

                IEnvelope env = cLipShp.Envelope;
                env.Expand(1.6, 1.6, true);
                app.MapControl.Extent = env;
                if (app.MapControl.ActiveView.FocusMap.MapScale < 100)
                {

                    env = Pageply.Envelope;
                    env.Expand(1.6, 1.6, true);
                    app.MapControl.Extent = env;
                }
               
                #region 各框线尺寸文字
                txtPoint = new PointClass();      
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "内图廓尺寸,宽:" + CommonMethods.InlineWidth + "，高:" + CommonMethods.InlineHeight + "（mm）";
                double textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 4;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();               
              
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓尺寸,宽:" + (CommonMethods.InlineWidth + CommonMethods.InOutLineWidth * 2) + "，高:" + (CommonMethods.InlineHeight + CommonMethods.InOutLineWidth * 2) + "（mm）";
                textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 3;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();       
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "成图尺寸,宽:" + CommonMethods.MapSizeWidth + "，高:" + CommonMethods.MapSizeHeight + "（mm）";
                textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 2;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();    
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "纸张尺寸,宽:" + CommonMethods.PaperWidth + "，高:" + CommonMethods.PaperHeight + "（mm）";
                textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 1;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                #endregion

                #endregion
            }
            catch
            {
            }



            return pGroupElement;
        }
       

       /// <summary>
       /// 内外图廓尺寸变换调整
       /// </summary>
       /// <param name="centerPoint"></param>
       /// <param name="refScale"></param>
        public static void UpdateClipGroupElement(IPoint centerPoint, double refScale)
        {
            GApplication app = GApplication.Application;
            //目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = app.MapControl.Map.SpatialReference;

            //在目标空间参考系中生成裁切元素
            IMap pTempMap = new MapClass();
            pTempMap.SpatialReference = targetSpatialReference;
            UnitConverterClass unitConverter = new UnitConverterClass();
          
            if (centerPoint.SpatialReference.Name != targetSpatialReference.Name)
            {
                (centerPoint as IGeometry).Project(targetSpatialReference);//对裁切中心点进行投影变换
            }
         

            //添加图形元素
            IGroupElement pGroupElement = new GroupElementClass();

            if (null == targetSpatialReference)
                targetSpatialReference = app.MapControl.Map.SpatialReference;



            double Inlinewidth = unitConverter.ConvertUnits((MapSizeInfoClass.InlineWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓宽度
            double Inlineheight = unitConverter.ConvertUnits((MapSizeInfoClass.InlineHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓高度
            double MapSizeWidth = unitConverter.ConvertUnits((MapSizeInfoClass.MapSizeWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//成图尺寸宽度（实地——
            double MapSizeHeight = unitConverter.ConvertUnits((MapSizeInfoClass.MapSizeHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//成图尺寸高度（实地——
            double PageWidth = unitConverter.ConvertUnits((MapSizeInfoClass.PaperWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//页面尺寸宽度（实地——
            double PageHeight = unitConverter.ConvertUnits((MapSizeInfoClass.PaperHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//页面尺寸高度（实地——
            double InOutLineWidth = unitConverter.ConvertUnits((MapSizeInfoClass.InOutLineWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内外图廓间距（实地——



            //生成内图廓边框多边形
            double InlinexMax = centerPoint.X + Inlinewidth / 2;
            double InlineyMax = centerPoint.Y + Inlineheight / 2;
            double InlinexMin = centerPoint.X - Inlinewidth / 2;
            double InlineyMin = centerPoint.Y - Inlineheight / 2;
            IPolygon Inlineply = CreatePolygon(InlinexMin, InlinexMax, InlineyMin, InlineyMax);
            Inlineply.SpatialReference = targetSpatialReference;
            //生成外图廓边框多边形
            double OutlinexMax = InlinexMax + InOutLineWidth;
            double OutlineyMax = InlineyMax + InOutLineWidth;
            double OutlinexMin = InlinexMin - InOutLineWidth;
            double OutlineyMin = InlineyMin - InOutLineWidth;
            IPolygon Outlineply = CreatePolygon(OutlinexMin, OutlinexMax, OutlineyMin, OutlineyMax);
            Outlineply.SpatialReference = targetSpatialReference;

            //生成成图尺寸多边形,默认下、左、右留白一样
            double HSpace = (MapSizeWidth - (OutlinexMax - OutlinexMin)) / 2;
            double VSpaceUP = unitConverter.ConvertUnits((MapSizeInfoClass.MapSizeTopInterval) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;
            double VSpaceDown = unitConverter.ConvertUnits((MapSizeInfoClass.MapSizeDownInterval) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;

            double MapsizexMax = OutlinexMax + HSpace;
            double MapsizeyMax = OutlineyMax + VSpaceUP;
            double MapsizexMin = OutlinexMin - HSpace;
            double MapsizeyMin = OutlineyMin - VSpaceDown;
            IPolygon Mapsizeply = CreatePolygon(MapsizexMin, MapsizexMax, MapsizeyMin, MapsizeyMax);
            Mapsizeply.SpatialReference = targetSpatialReference;
            //生成页面尺寸
            double PagesizexMax = MapsizexMax + (PageWidth - MapSizeWidth) / 2;
            double PagesizeyMax = MapsizeyMax + (PageHeight - MapSizeHeight) / 2;
            double PagesizexMin = MapsizexMin - (PageWidth - MapSizeWidth) / 2;
            double PagesizeyMin = MapsizeyMin - (PageHeight - MapSizeHeight) / 2;
            IPolygon Pageply = CreatePolygon(PagesizexMin, PagesizexMax, PagesizeyMin, PagesizeyMax);
            Pageply.SpatialReference = targetSpatialReference;

            IGeometry cLipShp = (centerPoint as ITopologicalOperator).Buffer(1);
            IGeometry cLipShp2 = Pageply;


            #region 裁切框中心点
            //中心点符号
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            pSimpleMarkerSymbol.Color = new RgbColorClass { Red = 255 };//颜色
            pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSSquare;//符号类型
            pSimpleMarkerSymbol.Size = 5;//大小
            pSimpleMarkerSymbol.Outline = true;//显示外框线
            pSimpleMarkerSymbol.OutlineColor = new RgbColorClass { Red = 255 };//外框线颜色
            pSimpleMarkerSymbol.OutlineSize = 3;//外框线的宽度

            IElement pMarkerElment = new MarkerElementClass();
            (centerPoint as IGeometry).Project(app.MapControl.Map.SpatialReference);//对裁切中心点进行投影变换
            pMarkerElment.Geometry = centerPoint as IGeometry;//几何数据
            (pMarkerElment as IElementProperties3).Name = "裁切中心点";//名称
            (pMarkerElment as IMarkerElement).Symbol = pSimpleMarkerSymbol;//符号
            #endregion
            #region 裁切范围
            IGroupElement ge = getClipGroupElement(app.MapControl.ActiveView.GraphicsContainer);
            if (ge == null)
                return;
            IPolygon clipShp = null;
            ISimpleLineSymbol pclipLineSymbol = new SimpleLineSymbolClass();
            pclipLineSymbol.Width = 1; //边框线宽
            pclipLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            for (int i = 0; i < ge.ElementCount; i++)
            {
                IElement el = ge.get_Element(i);
                IElementProperties3 eleProp = el as IElementProperties3;

                if ("裁切范围" == eleProp.Name)
                {
                    clipShp = (el.Geometry as IClone).Clone() as IPolygon;
                    pclipLineSymbol.Width = (el as IFillShapeElement).Symbol.Outline.Width;
                    pclipLineSymbol.Color = (el as IFillShapeElement).Symbol.Outline.Color;
                    break;
                }
            }
           
            IFillSymbol pclipFillSymbol2 = new SimpleFillSymbolClass();
            pclipFillSymbol2.Color = new RgbColorClass { NullColor = true };//填充颜色
            pclipFillSymbol2.Outline = pclipLineSymbol;//边框线

            IElement pClipElement2 = new PolygonElement();
            cLipShp2.Project(app.MapControl.Map.SpatialReference);//投影变换
            pClipElement2.Geometry = clipShp;//几何数据
            (pClipElement2 as IElementProperties3).Name = "裁切范围";//名称    
            (pClipElement2 as IFillShapeElement).Symbol = pclipFillSymbol2; //符号



            #endregion

            #region 中心点扩展
            //符号
            ISimpleLineSymbol pClipLineSymbol = new SimpleLineSymbolClass();
            pClipLineSymbol.Width = 2; //边框线宽
            pClipLineSymbol.Color = new RgbColorClass { Red = 255, Green = 61, Blue = 249 };
            IFillSymbol pClipFillSymbol = new SimpleFillSymbolClass();
            pClipFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pClipFillSymbol.Outline = pClipLineSymbol;//边框线

            IElement pClipElement = new PolygonElement();
            cLipShp.Project(app.MapControl.Map.SpatialReference);//投影变换
            pClipElement.Geometry = cLipShp;//几何数据
            //(pClipElement as IElementProperties3).Name = "裁切范围";//名称
            (pClipElement as IFillShapeElement).Symbol = pClipFillSymbol; //符号
            #endregion


            #region 内图廓范围
            //符号
            ISimpleLineSymbol pInlineLineSymbol = new SimpleLineSymbolClass();
            pInlineLineSymbol.Width = 1; //边框线宽
            pInlineLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pInlineFillSymbol = new SimpleFillSymbolClass();
            pInlineFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pInlineFillSymbol.Outline = pInlineLineSymbol;//边框线

            IElement pInlineElement = new PolygonElement();
            Inlineply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pInlineElement.Geometry = Inlineply;//几何数据
            (pInlineElement as IElementProperties3).Name = "内图廓";//名称
            (pInlineElement as IFillShapeElement).Symbol = pInlineFillSymbol; //符号
            #endregion
            #region 外图廓范围
            //符号
            ISimpleLineSymbol pOutlineLineSymbol = new SimpleLineSymbolClass();
            pOutlineLineSymbol.Width = 1; //边框线宽
            pOutlineLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pOutlineFillSymbol = new SimpleFillSymbolClass();
            pOutlineFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pOutlineFillSymbol.Outline = pOutlineLineSymbol;//边框线

            IElement pOutlineElement = new PolygonElement();
            Outlineply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pOutlineElement.Geometry = Outlineply;//几何数据
            (pOutlineElement as IElementProperties3).Name = "外图廓";//名称
            (pOutlineElement as IFillShapeElement).Symbol = pOutlineFillSymbol; //符号
            #endregion
            #region 成图范围
            //符号
            ISimpleLineSymbol pMapsizeLineSymbol = new SimpleLineSymbolClass();
            pMapsizeLineSymbol.Width = 1; //边框线宽
            pMapsizeLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pMapsizeFillSymbol = new SimpleFillSymbolClass();
            pMapsizeFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pMapsizeFillSymbol.Outline = pMapsizeLineSymbol;//边框线

            IElement pMapsizeElement = new PolygonElement();
            Mapsizeply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pMapsizeElement.Geometry = Mapsizeply;//几何数据
            (pMapsizeElement as IElementProperties3).Name = "成图范围";//名称
            (pMapsizeElement as IFillShapeElement).Symbol = pMapsizeFillSymbol; //符号
            #endregion
            #region 出版范围
            //符号
            ISimpleLineSymbol pPageLineSymbol = new SimpleLineSymbolClass();
            pPageLineSymbol.Width = 1; //边框线宽
            pPageLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pPageFillSymbol = new SimpleFillSymbolClass();
            pPageFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pPageFillSymbol.Outline = pPageLineSymbol;//边框线

            IElement pPageElement = new PolygonElement();
            Pageply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pPageElement.Geometry = Pageply;//几何数据
            (pPageElement as IElementProperties3).Name = "出版范围";//名称
            (pPageElement as IFillShapeElement).Symbol = pPageFillSymbol; //符号
            #endregion

            try
            {
                pGroupElement.AddElement(pClipElement);
                pGroupElement.AddElement(pClipElement2);
                pGroupElement.AddElement(pInlineElement);
                pGroupElement.AddElement(pMapsizeElement);
                pGroupElement.AddElement(pPageElement);
                pGroupElement.AddElement(pOutlineElement);
                pGroupElement.AddElement(pMarkerElment);
                #region 计算横向和纵向角度
                //纵向角度
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                fromPoint.X = InlinexMin; fromPoint.Y = InlineyMin;
                fromPoint.SpatialReference = targetSpatialReference;
                fromPoint.Project(app.MapControl.Map.SpatialReference);
                toPoint.X = InlinexMin; toPoint.Y = InlineyMax;
                toPoint.SpatialReference = targetSpatialReference;
                toPoint.Project(app.MapControl.Map.SpatialReference);
                double Yangle = GetAngle(fromPoint, toPoint);
                //计算横向角度
                fromPoint.X = InlinexMin; fromPoint.Y = InlineyMin;
                fromPoint.SpatialReference = targetSpatialReference;
                fromPoint.Project(app.MapControl.Map.SpatialReference);
                toPoint.X = InlinexMax; toPoint.Y = InlineyMin;
                toPoint.SpatialReference = targetSpatialReference;
                toPoint.Project(app.MapControl.Map.SpatialReference);
                double Xangle = GetAngle(fromPoint, toPoint);
                Xangle = 0;
                #endregion


                #region 边缘距离
                //上边缘距离
                IPolygon proPolygon = (cLipShp as IClone).Clone() as IPolygon;
                proPolygon.Project(targetSpatialReference);

                #region 裁切面与内图廓上边缘距离
                PolylineClass lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X, Y = proPolygon.Envelope.UpperLeft.Y - 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X, Y = proPolygon.Envelope.UpperRight.Y - 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                IPolyline line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                IPoint center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                PolylineClass lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                PolylineClass linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                var geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = center.X, Y = InlineyMax });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                IRgbColor prgb = new RgbColorClass();
                prgb.Red = 220;
                IArrowMarkerSymbol arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                double updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                var txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.3, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                TextElementClass txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                MarkerElementClass markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.FromPoint as IGeometry;
                //线
                var frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.FromPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                LineElementClass lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion
                #region 下边缘距离
                //下边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X, Y = proPolygon.Envelope.LowerLeft.Y + 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X, Y = proPolygon.Envelope.LowerRight.Y + 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = center.X, Y = InlineyMin });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.7, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.ToPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.ToPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion
                #region 左边缘距离
                //左边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X + 1, Y = proPolygon.Envelope.UpperLeft.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X + 1, Y = proPolygon.Envelope.LowerLeft.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = InlinexMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = InlinexMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = InlinexMin, Y = center.Y });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.FromPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.FromPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion
                #region 右边缘距离
                //右边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X - 1, Y = proPolygon.Envelope.UpperRight.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X - 1, Y = proPolygon.Envelope.LowerRight.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = InlinexMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = InlinexMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = InlinexMax, Y = center.Y });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;


                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.ToPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.ToPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion

                #region 内外图廓距离
                //上边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = InlinexMin + (InlinexMax - InlinexMin) / 3, Y = InlineyMax });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = InlinexMin + (InlinexMax - InlinexMin) / 3, Y = InlineyMax + InOutLineWidth });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;

                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.3, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "内外图廓间距" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;

                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                #endregion



                #region 外图廓距离裁切线间距
                //上边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = OutlineyMax });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = MapsizeyMax });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                MapSizeInfoClass.MapSizeTopInterval = updis;
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);





                //右边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = OutlinexMax, Y = (OutlineyMin + OutlineyMax) / 2 });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = MapsizexMax, Y = (OutlineyMin + OutlineyMax) / 2 });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);



                //左边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = OutlinexMin, Y = (OutlineyMin + OutlineyMax) / 2 });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = MapsizexMin, Y = (OutlineyMin + OutlineyMax) / 2 });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);



                //下边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = OutlineyMin });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = MapsizeyMin });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                MapSizeInfoClass.MapSizeDownInterval = updis;
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);

                #endregion

                #region 框线文字
                txtPoint = new PointClass();
                txtPoint.X = InlinexMax; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "内\n图\n廓";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtPoint.X = OutlinexMin; txtPoint.Y = OutlineyMin + (OutlineyMax - OutlineyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外\n图\n廓";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtPoint.X = MapsizexMin; txtPoint.Y = MapsizeyMin + (MapsizeyMax - MapsizeyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "成\n图\n尺\n寸";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtPoint.X = PagesizexMax; txtPoint.Y = PagesizeyMin + (PagesizeyMax - PagesizeyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "纸\n张";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                #endregion

                IEnvelope env = cLipShp.Envelope;
                env.Expand(1.6, 1.6, true);
                app.MapControl.Extent = env;
                if (app.MapControl.ActiveView.FocusMap.MapScale < 100)
                {

                    env = Pageply.Envelope;
                    env.Expand(1.6, 1.6, true);
                    app.MapControl.Extent = env;
                }

                #region 各框线尺寸文字
                txtPoint = new PointClass();
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "内图廓尺寸,宽:" + MapSizeInfoClass.InlineWidth + "，高:" + MapSizeInfoClass.InlineHeight + "（mm）";
                double textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 4;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();

                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓尺寸,宽:" + (MapSizeInfoClass.InlineWidth + MapSizeInfoClass.InOutLineWidth * 2) + "，高:" + (MapSizeInfoClass.InlineHeight + MapSizeInfoClass.InOutLineWidth * 2) + "（mm）";
                textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 3;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "成图尺寸,宽:" + MapSizeInfoClass.MapSizeWidth + "，高:" + MapSizeInfoClass.MapSizeHeight + "（mm）";
                textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 2;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "纸张尺寸,宽:" + MapSizeInfoClass.PaperWidth + "，高:" + MapSizeInfoClass.PaperHeight + "（mm）";
                textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 1;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                #endregion

                #endregion
            }
            catch
            {
            }


            GApplication.Application.MapControl.ActiveView.GraphicsContainer.DeleteAllElements();
            GApplication.Application.MapControl.ActiveView.GraphicsContainer.AddElement(pGroupElement as IElement, 0);
            GApplication.Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics,null,null);
        }


        /// <summary>
        /// 创建裁切元素
        /// </summary>
        /// <param name="app"></param>
        /// <param name="cLipShp"></param>
        /// <param name="paperWidth"></param>
        /// <param name="paperHeight"></param>
        /// <param name="refScale"></param>
        /// <returns></returns>
        public static IGroupElement createClipGroupElement(GApplication app, IPolygon cLipShp, double paperWidth, double paperHeight, double refScale)
        {
            IGroupElement pGroupElement = new GroupElementClass();
            //目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = app.MapControl.Map.SpatialReference;

            //在目标空间参考系中生成裁切元素
            IMap pTempMap = new MapClass();
            pTempMap.SpatialReference = targetSpatialReference;
            UnitConverterClass unitConverter = new UnitConverterClass();
            double width = unitConverter.ConvertUnits((paperWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//宽度
            double height = unitConverter.ConvertUnits((paperHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//高度

            IPoint centerPoint = (cLipShp.Envelope as IArea).Centroid;
            if (centerPoint.SpatialReference.Name != targetSpatialReference.Name)
            {
                (centerPoint as IGeometry).Project(targetSpatialReference);//对裁切中心点进行投影变换
            }
            //生成边框多边形
            double xMax = centerPoint.X + width / 2;
            double yMax = centerPoint.Y + height / 2;
            double xMin = centerPoint.X - width / 2;
            double yMin = centerPoint.Y - height / 2;
            IPolygon ply = CreatePolygon(xMin, xMax, yMin, yMax);
            ply.SpatialReference = targetSpatialReference;

            #region 裁切框中心点
            //中心点符号
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            pSimpleMarkerSymbol.Color = new RgbColorClass { Red = 255 };//颜色
            pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSSquare;//符号类型
            pSimpleMarkerSymbol.Size = 5;//大小
            pSimpleMarkerSymbol.Outline = true;//显示外框线
            pSimpleMarkerSymbol.OutlineColor = new RgbColorClass { Red = 255 };//外框线颜色
            pSimpleMarkerSymbol.OutlineSize = 3;//外框线的宽度

            IElement pMarkerElment = new MarkerElementClass();
            (centerPoint as IGeometry).Project(app.MapControl.Map.SpatialReference);//对裁切中心点进行投影变换
            pMarkerElment.Geometry = centerPoint as IGeometry;//几何数据
            (pMarkerElment as IElementProperties3).Name = "裁切中心点";//名称
            (pMarkerElment as IMarkerElement).Symbol = pSimpleMarkerSymbol;//符号
            #endregion

            #region 裁切范围
            //符号
            ISimpleLineSymbol pClipLineSymbol = new SimpleLineSymbolClass();
            pClipLineSymbol.Width = 2; //边框线宽
            pClipLineSymbol.Color = new RgbColorClass { Red = 255, Green = 61, Blue = 249 };
            IFillSymbol pClipFillSymbol = new SimpleFillSymbolClass();
            pClipFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pClipFillSymbol.Outline = pClipLineSymbol;//边框线

            IElement pClipElement = new PolygonElement();
            cLipShp.Project(app.MapControl.Map.SpatialReference);//投影变换
            pClipElement.Geometry = cLipShp;//几何数据
            (pClipElement as IElementProperties3).Name = "裁切范围";//名称
            (pClipElement as IFillShapeElement).Symbol = pClipFillSymbol; //符号
            #endregion

            #region 出版范围
            //符号
            ISimpleLineSymbol pPageLineSymbol = new SimpleLineSymbolClass();
            pPageLineSymbol.Width = 1; //边框线宽
            pPageLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pPageFillSymbol = new SimpleFillSymbolClass();
            pPageFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pPageFillSymbol.Outline = pPageLineSymbol;//边框线

            IElement pPageElement = new PolygonElement();
            ply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pPageElement.Geometry = ply;//几何数据
            (pPageElement as IElementProperties3).Name = "出版范围";//名称
            (pPageElement as IFillShapeElement).Symbol = pPageFillSymbol; //符号
            #endregion

            #region 边缘距离
            IPolygon proPolygon = (cLipShp as IClone).Clone() as IPolygon;
            proPolygon.Project(targetSpatialReference);

            #region 上边缘距离
            PolylineClass lineUp = new PolylineClass();
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X, Y = proPolygon.Envelope.UpperLeft.Y - 1 });
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X, Y = proPolygon.Envelope.UpperRight.Y - 1 });
            (lineUp as ITopologicalOperator).Simplify();
            lineUp.Project(targetSpatialReference);
            IPolyline line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            IPoint center = new PointClass();
            line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

            PolylineClass lineTop = new PolylineClass();
            lineTop.AddPoint(center);
            //第二个点
            PolylineClass linemid = new PolylineClass();
            linemid.AddPoint(new PointClass { X = center.X, Y = yMax + 10 });
            linemid.AddPoint(new PointClass { X = center.X, Y = yMin - 10 });
            linemid.SpatialReference = targetSpatialReference;
            linemid.Project(app.MapControl.Map.SpatialReference);
            linemid.Simplify();
            var geometry = (linemid as ITopologicalOperator).Intersect(ply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

            lineTop.AddPoint(new PointClass { X = center.X, Y = yMax });
            lineTop.Project(targetSpatialReference);
            (lineTop).Simplify();

            IRgbColor prgb = new RgbColorClass();
            prgb.Red = 220;
            IArrowMarkerSymbol arrowmarker = new ArrowMarkerSymbolClass();
            arrowmarker.Color = prgb;



            //文字
            double updis = lineTop.Length / refScale * 1000;
            updis = Math.Round(updis, 2);
            var txtPoint = new PointClass();
            lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
            txtPoint.Project(app.MapControl.Map.SpatialReference);
            TextElementClass txtEle = new TextElementClass();
            txtEle.Name = "文字间距注释";
            txtEle.Text = updis + " mm";
            txtEle.Geometry = txtPoint;


            //箭头
            lineTop.Project(app.MapControl.Map.SpatialReference);
            MarkerElementClass markerelement = new MarkerElementClass();
            markerelement.Name = "箭头间距注释";
            markerelement.Symbol = arrowmarker;
            markerelement.Geometry = geometry.FromPoint as IGeometry;
            //线
            var frompt = lineTop.FromPoint;
            lineTop = new PolylineClass();
            lineTop.AddPoint(frompt);
            lineTop.AddPoint(geometry.FromPoint);
            lineTop.Simplify();
            lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
            LineElementClass lineEl = new LineElementClass();
            lineEl.Name = "线间距注释";
            lineEl.Geometry = lineTop as IGeometry;
            //箭头角度
            arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
            pGroupElement.AddElement(txtEle);
            pGroupElement.AddElement(markerelement);
            pGroupElement.AddElement(lineEl);
            #endregion
            #region 下边缘距离
            //下边缘距离

            lineUp = new PolylineClass();
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X, Y = proPolygon.Envelope.LowerLeft.Y + 1 });
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X, Y = proPolygon.Envelope.LowerRight.Y + 1 });
            (lineUp as ITopologicalOperator).Simplify();
            lineUp.Project(targetSpatialReference);
            line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            center = new PointClass();
            line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

            lineTop = new PolylineClass();
            lineTop.AddPoint(center);
            //第二个点
            linemid = new PolylineClass();
            linemid.AddPoint(new PointClass { X = center.X, Y = yMax + 10 });
            linemid.AddPoint(new PointClass { X = center.X, Y = yMin - 10 });
            linemid.SpatialReference = targetSpatialReference;
            linemid.Project(app.MapControl.Map.SpatialReference);
            linemid.Simplify();
            geometry = (linemid as ITopologicalOperator).Intersect(ply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

            lineTop.AddPoint(new PointClass { X = center.X, Y = yMin });
            lineTop.Project(targetSpatialReference);
            (lineTop).Simplify();

            arrowmarker = new ArrowMarkerSymbolClass();
            arrowmarker.Color = prgb;



            //文字
            updis = lineTop.Length / refScale * 1000;
            updis = Math.Round(updis, 2);
            txtPoint = new PointClass();
            lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
            txtPoint.Project(app.MapControl.Map.SpatialReference);
            txtEle = new TextElementClass();
            txtEle.Name = "文字间距注释";
            txtEle.Text = updis + " mm";
            txtEle.Geometry = txtPoint;


            //箭头
            lineTop.Project(app.MapControl.Map.SpatialReference);
            markerelement = new MarkerElementClass();
            markerelement.Name = "箭头间距注释";
            markerelement.Symbol = arrowmarker;
            markerelement.Geometry = geometry.ToPoint as IGeometry;
            //线
            frompt = lineTop.FromPoint;
            lineTop = new PolylineClass();
            lineTop.AddPoint(frompt);
            lineTop.AddPoint(geometry.ToPoint);
            lineTop.Simplify();
            lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
            lineEl = new LineElementClass();
            lineEl.Name = "线间距注释";
            lineEl.Geometry = lineTop as IGeometry;
            //箭头角度
            arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
            pGroupElement.AddElement(txtEle);
            pGroupElement.AddElement(markerelement);
            pGroupElement.AddElement(lineEl);
            #endregion

            #region 左边缘距离
            //左边缘距离

            lineUp = new PolylineClass();
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X + 1, Y = proPolygon.Envelope.UpperLeft.Y });
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X + 1, Y = proPolygon.Envelope.LowerLeft.Y });
            (lineUp as ITopologicalOperator).Simplify();
            lineUp.Project(targetSpatialReference);
            line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            center = new PointClass();
            line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

            lineTop = new PolylineClass();
            lineTop.AddPoint(center);
            //第二个点
            linemid = new PolylineClass();
            linemid.AddPoint(new PointClass { X = xMin - 10, Y = center.Y });
            linemid.AddPoint(new PointClass { X = xMax + 10, Y = center.Y });
            linemid.SpatialReference = targetSpatialReference;
            linemid.Project(app.MapControl.Map.SpatialReference);
            linemid.Simplify();
            geometry = (linemid as ITopologicalOperator).Intersect(ply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

            lineTop.AddPoint(new PointClass { X = xMin, Y = center.Y });
            lineTop.Project(targetSpatialReference);
            (lineTop).Simplify();

            arrowmarker = new ArrowMarkerSymbolClass();
            arrowmarker.Color = prgb;



            //文字
            updis = lineTop.Length / refScale * 1000;
            updis = Math.Round(updis, 2);
            txtPoint = new PointClass();
            lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
            txtPoint.Project(app.MapControl.Map.SpatialReference);
            txtEle = new TextElementClass();
            txtEle.Name = "文字间距注释";
            txtEle.Text = updis + " mm";
            txtEle.Geometry = txtPoint;


            //箭头
            lineTop.Project(app.MapControl.Map.SpatialReference);
            markerelement = new MarkerElementClass();
            markerelement.Name = "箭头间距注释";
            markerelement.Symbol = arrowmarker;
            markerelement.Geometry = geometry.FromPoint as IGeometry;
            //线
            frompt = lineTop.FromPoint;
            lineTop = new PolylineClass();
            lineTop.AddPoint(frompt);
            lineTop.AddPoint(geometry.FromPoint);
            lineTop.Simplify();
            lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
            lineEl = new LineElementClass();
            lineEl.Name = "线间距注释";
            lineEl.Geometry = lineTop as IGeometry;
            //箭头角度
            arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
            pGroupElement.AddElement(txtEle);
            pGroupElement.AddElement(markerelement);
            pGroupElement.AddElement(lineEl);
            #endregion
            #region 右边缘距离
            //右边缘距离

            lineUp = new PolylineClass();
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X - 1, Y = proPolygon.Envelope.UpperRight.Y });
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X - 1, Y = proPolygon.Envelope.LowerRight.Y });
            (lineUp as ITopologicalOperator).Simplify();
            lineUp.Project(targetSpatialReference);
            line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            center = new PointClass();
            line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

            lineTop = new PolylineClass();
            lineTop.AddPoint(center);
            //第二个点
            linemid = new PolylineClass();
            linemid.AddPoint(new PointClass { X = xMin - 10, Y = center.Y });
            linemid.AddPoint(new PointClass { X = xMax + 10, Y = center.Y });
            linemid.SpatialReference = targetSpatialReference;
            linemid.Project(app.MapControl.Map.SpatialReference);
            linemid.Simplify();
            geometry = (linemid as ITopologicalOperator).Intersect(ply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

            lineTop.AddPoint(new PointClass { X = xMax, Y = center.Y });
            lineTop.Project(targetSpatialReference);
            (lineTop).Simplify();

            arrowmarker = new ArrowMarkerSymbolClass();
            arrowmarker.Color = prgb;



            //文字
            updis = lineTop.Length / refScale * 1000;
            updis = Math.Round(updis, 2);
            txtPoint = new PointClass();
            lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
            txtPoint.Project(app.MapControl.Map.SpatialReference);
            txtEle = new TextElementClass();
            txtEle.Name = "文字间距注释";
            txtEle.Text = updis + " mm";
            txtEle.Geometry = txtPoint;


            //箭头
            lineTop.Project(app.MapControl.Map.SpatialReference);
            markerelement = new MarkerElementClass();
            markerelement.Name = "箭头间距注释";
            markerelement.Symbol = arrowmarker;
            markerelement.Geometry = geometry.ToPoint as IGeometry;
            //线
            frompt = lineTop.FromPoint;
            lineTop = new PolylineClass();
            lineTop.AddPoint(frompt);
            lineTop.AddPoint(geometry.ToPoint);
            lineTop.Simplify();
            lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
            lineEl = new LineElementClass();
            lineEl.Name = "线间距注释";
            lineEl.Geometry = lineTop as IGeometry;
            //箭头角度
            arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
            pGroupElement.AddElement(txtEle);
            pGroupElement.AddElement(markerelement);
            pGroupElement.AddElement(lineEl);
            #endregion
            #endregion
            //添加图形元素

            pGroupElement.AddElement(pMarkerElment);
            pGroupElement.AddElement(pClipElement);
            pGroupElement.AddElement(pPageElement);

            return pGroupElement;
        }

        public static void MoveClipGroupElement(double dy, double dx, GApplication app)
        {
            IGroupElement pGroupElement = new GroupElementClass();
            //目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = app.MapControl.Map.SpatialReference;
            IGroupElement ge = getClipGroupElement(app.MapControl.ActiveView.GraphicsContainer);
            if (ge == null)
                return;

            IPoint centerPoint = null;
            IPolygon clipShp = null;
            bool bHasShpFile = false;
            IElement pPageElement = null;
            IElement pMarkerElment = null;
            IElement pClipElement = null;
            List<IElement> txtEles = new List<IElement>();
            for (int i = 0; i < ge.ElementCount; i++)
            {
                IElement el = ge.get_Element(i);
                IElementProperties3 eleProp = el as IElementProperties3;
                if ("裁切中心点" == eleProp.Name)
                {
                    centerPoint = el.Geometry as IPoint;
                    pMarkerElment = el;
                }
                else if ("裁切范围" == eleProp.Name)
                {
                    clipShp = el.Geometry as IPolygon;
                    pClipElement = el;
                }
                else if ("出版范围" == eleProp.Name)
                {
                    bHasShpFile = true;
                    pPageElement = el;
                }
                else
                {
                    txtEles.Add(el);
                }
            }
            if (!bHasShpFile)
                return;

            IGeometry PageGeometry = pPageElement.Geometry;

            var content = EnvironmentSettings.getContentElement(app);

            var mapScale = content.Element("MapScale");

            double refScale = Convert.ToDouble(mapScale.Value);
            PageGeometry.Project(targetSpatialReference);
            (PageGeometry as ITransform2D).Move(dx * 1.0e-3 * refScale, dy * 1.0e-3 * refScale);
            //边框多边形
            double xMax = PageGeometry.Envelope.XMax;
            double yMax = PageGeometry.Envelope.YMax;
            double xMin = PageGeometry.Envelope.XMin;
            double yMin = PageGeometry.Envelope.YMin;

            IPolygon plyPage = pPageElement.Geometry as IPolygon;

            #region 边缘距离
            //上边缘距离
            IPolygon cLipShp = pClipElement.Geometry as IPolygon;
            IPolygon proPolygon = (cLipShp as IClone).Clone() as IPolygon;
            proPolygon.Project(targetSpatialReference);
            #region 上边缘距离
            PolylineClass lineUp = new PolylineClass();
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X, Y = proPolygon.Envelope.UpperLeft.Y - 1 });
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X, Y = proPolygon.Envelope.UpperRight.Y - 1 });
            (lineUp as ITopologicalOperator).Simplify();
            lineUp.Project(targetSpatialReference);
            IPolyline line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            IPoint center = new PointClass();
            line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

            PolylineClass lineTop = new PolylineClass();
            lineTop.AddPoint(center);
            //第二个点
            PolylineClass linemid = new PolylineClass();
            linemid.AddPoint(new PointClass { X = center.X, Y = yMax + 10 });
            linemid.AddPoint(new PointClass { X = center.X, Y = yMin - 10 });
            linemid.SpatialReference = targetSpatialReference;
            linemid.Project(app.MapControl.Map.SpatialReference);
            linemid.Simplify();
            var geometry = (linemid as ITopologicalOperator).Intersect(plyPage, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

            lineTop.AddPoint(new PointClass { X = center.X, Y = yMax });
            lineTop.Project(targetSpatialReference);
            (lineTop).Simplify();

            IRgbColor prgb = new RgbColorClass();
            prgb.Red = 220;
            IArrowMarkerSymbol arrowmarker = new ArrowMarkerSymbolClass();
            arrowmarker.Color = prgb;



            //文字
            double updis = lineTop.Length / refScale * 1000;
            updis = Math.Round(updis, 2);
            var txtPoint = new PointClass();
            lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
            txtPoint.Project(app.MapControl.Map.SpatialReference);
            TextElementClass txtEle = new TextElementClass();
            txtEle.Name = "文字间距注释";
            txtEle.Text = updis + " mm";
            txtEle.Geometry = txtPoint;


            //箭头
            lineTop.Project(app.MapControl.Map.SpatialReference);
            MarkerElementClass markerelement = new MarkerElementClass();
            markerelement.Name = "箭头间距注释";
            markerelement.Symbol = arrowmarker;
            markerelement.Geometry = geometry.FromPoint as IGeometry;
            //线
            var frompt = lineTop.FromPoint;
            lineTop = new PolylineClass();
            lineTop.AddPoint(frompt);
            lineTop.AddPoint(geometry.FromPoint);
            lineTop.Simplify();
            lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
            LineElementClass lineEl = new LineElementClass();
            lineEl.Name = "线间距注释";
            lineEl.Geometry = lineTop as IGeometry;
            //箭头角度
            arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
            pGroupElement.AddElement(txtEle);
            pGroupElement.AddElement(markerelement);
            pGroupElement.AddElement(lineEl);
            #endregion
            #region 下边缘距离
            //下边缘距离

            lineUp = new PolylineClass();
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X, Y = proPolygon.Envelope.LowerLeft.Y + 1 });
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X, Y = proPolygon.Envelope.LowerRight.Y + 1 });
            (lineUp as ITopologicalOperator).Simplify();
            lineUp.Project(targetSpatialReference);
            line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            center = new PointClass();
            line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

            lineTop = new PolylineClass();
            lineTop.AddPoint(center);
            //第二个点
            linemid = new PolylineClass();
            linemid.AddPoint(new PointClass { X = center.X, Y = yMax + 10 });
            linemid.AddPoint(new PointClass { X = center.X, Y = yMin - 10 });
            linemid.SpatialReference = targetSpatialReference;
            linemid.Project(app.MapControl.Map.SpatialReference);
            linemid.Simplify();
            geometry = (linemid as ITopologicalOperator).Intersect(plyPage, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

            lineTop.AddPoint(new PointClass { X = center.X, Y = yMin });
            lineTop.Project(targetSpatialReference);
            (lineTop).Simplify();

            arrowmarker = new ArrowMarkerSymbolClass();
            arrowmarker.Color = prgb;



            //文字
            updis = lineTop.Length / refScale * 1000;
            updis = Math.Round(updis, 2);
            txtPoint = new PointClass();
            lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
            txtPoint.Project(app.MapControl.Map.SpatialReference);
            txtEle = new TextElementClass();
            txtEle.Name = "文字间距注释";
            txtEle.Text = updis + " mm";
            txtEle.Geometry = txtPoint;


            //箭头
            lineTop.Project(app.MapControl.Map.SpatialReference);
            markerelement = new MarkerElementClass();
            markerelement.Name = "箭头间距注释";
            markerelement.Symbol = arrowmarker;
            markerelement.Geometry = geometry.ToPoint as IGeometry;
            //线
            frompt = lineTop.FromPoint;
            lineTop = new PolylineClass();
            lineTop.AddPoint(frompt);
            lineTop.AddPoint(geometry.ToPoint);
            lineTop.Simplify();
            lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
            lineEl = new LineElementClass();
            lineEl.Name = "线间距注释";
            lineEl.Geometry = lineTop as IGeometry;
            //箭头角度
            arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
            pGroupElement.AddElement(txtEle);
            pGroupElement.AddElement(markerelement);
            pGroupElement.AddElement(lineEl);
            #endregion

            #region 左边缘距离
            //左边缘距离

            lineUp = new PolylineClass();
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X + 1, Y = proPolygon.Envelope.UpperLeft.Y });
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X + 1, Y = proPolygon.Envelope.LowerLeft.Y });
            (lineUp as ITopologicalOperator).Simplify();
            lineUp.Project(targetSpatialReference);
            line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            center = new PointClass();
            line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

            lineTop = new PolylineClass();
            lineTop.AddPoint(center);
            //第二个点
            linemid = new PolylineClass();
            linemid.AddPoint(new PointClass { X = xMin - 10, Y = center.Y });
            linemid.AddPoint(new PointClass { X = xMax + 10, Y = center.Y });
            linemid.SpatialReference = targetSpatialReference;
            linemid.Project(app.MapControl.Map.SpatialReference);
            linemid.Simplify();
            geometry = (linemid as ITopologicalOperator).Intersect(plyPage, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

            lineTop.AddPoint(new PointClass { X = xMin, Y = center.Y });
            lineTop.Project(targetSpatialReference);
            (lineTop).Simplify();

            arrowmarker = new ArrowMarkerSymbolClass();
            arrowmarker.Color = prgb;



            //文字
            updis = lineTop.Length / refScale * 1000;
            updis = Math.Round(updis, 2);
            txtPoint = new PointClass();
            lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
            txtPoint.Project(app.MapControl.Map.SpatialReference);
            txtEle = new TextElementClass();
            txtEle.Name = "文字间距注释";
            txtEle.Text = updis + " mm";
            txtEle.Geometry = txtPoint;


            //箭头
            lineTop.Project(app.MapControl.Map.SpatialReference);
            markerelement = new MarkerElementClass();
            markerelement.Name = "箭头间距注释";
            markerelement.Symbol = arrowmarker;
            markerelement.Geometry = geometry.FromPoint as IGeometry;
            //线
            frompt = lineTop.FromPoint;
            lineTop = new PolylineClass();
            lineTop.AddPoint(frompt);
            lineTop.AddPoint(geometry.FromPoint);
            lineTop.Simplify();
            lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
            lineEl = new LineElementClass();
            lineEl.Name = "线间距注释";
            lineEl.Geometry = lineTop as IGeometry;
            //箭头角度
            arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
            pGroupElement.AddElement(txtEle);
            pGroupElement.AddElement(markerelement);
            pGroupElement.AddElement(lineEl);
            #endregion
            #region 右边缘距离
            //右边缘距离
            lineUp = new PolylineClass();
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X - 1, Y = proPolygon.Envelope.UpperRight.Y });
            lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X - 1, Y = proPolygon.Envelope.LowerRight.Y });
            (lineUp as ITopologicalOperator).Simplify();
            lineUp.Project(targetSpatialReference);
            line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
            center = new PointClass();
            line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

            lineTop = new PolylineClass();
            lineTop.AddPoint(center);
            //第二个点
            linemid = new PolylineClass();
            linemid.AddPoint(new PointClass { X = xMin - 10, Y = center.Y });
            linemid.AddPoint(new PointClass { X = xMax + 10, Y = center.Y });
            linemid.SpatialReference = targetSpatialReference;
            linemid.Project(app.MapControl.Map.SpatialReference);
            linemid.Simplify();
            geometry = (linemid as ITopologicalOperator).Intersect(plyPage, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

            lineTop.AddPoint(new PointClass { X = xMax, Y = center.Y });
            lineTop.Project(targetSpatialReference);
            (lineTop).Simplify();

            arrowmarker = new ArrowMarkerSymbolClass();
            arrowmarker.Color = prgb;



            //文字
            updis = lineTop.Length / refScale * 1000;
            updis = Math.Round(updis, 2);
            txtPoint = new PointClass();
            lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
            txtPoint.Project(app.MapControl.Map.SpatialReference);
            txtEle = new TextElementClass();
            txtEle.Name = "文字间距注释";
            txtEle.Text = updis + " mm";
            txtEle.Geometry = txtPoint;


            //箭头
            lineTop.Project(app.MapControl.Map.SpatialReference);
            markerelement = new MarkerElementClass();
            markerelement.Name = "箭头间距注释";
            markerelement.Symbol = arrowmarker;
            markerelement.Geometry = geometry.ToPoint as IGeometry;
            //线
            frompt = lineTop.FromPoint;
            lineTop = new PolylineClass();
            lineTop.AddPoint(frompt);
            lineTop.AddPoint(geometry.ToPoint);
            lineTop.Simplify();
            lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
            lineEl = new LineElementClass();
            lineEl.Name = "线间距注释";
            lineEl.Geometry = lineTop as IGeometry;
            //箭头角度
            arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
            pGroupElement.AddElement(txtEle);
            pGroupElement.AddElement(markerelement);
            pGroupElement.AddElement(lineEl);
            #endregion
            #endregion

          

            app.MapControl.ActiveView.GraphicsContainer.DeleteElement(ge as IElement);
            PageGeometry.Project(app.MapControl.SpatialReference);
            pPageElement.Geometry = PageGeometry;
            pGroupElement.AddElement(pMarkerElment);
            pGroupElement.AddElement(pClipElement);
            pGroupElement.AddElement(pPageElement);
            app.MapControl.ActiveView.GraphicsContainer.AddElement(pGroupElement as IElement, 0);
            app.MapControl.ActiveView.Refresh();
        }
        
        public static void MoveClipGroupElement1(double dy, double dx, GApplication app)
        {
            IGroupElement pGroupElement = new GroupElementClass();
            //目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = app.MapControl.Map.SpatialReference;
            IGroupElement ge = getClipGroupElement(app.MapControl.ActiveView.GraphicsContainer);
            if (ge == null)
                return;
            var content = EnvironmentSettings.getContentElement(app);
            var mapScale = content.Element("MapScale");
            double refScale = Convert.ToDouble(mapScale.Value);
            //double x = dx * 1.0e-3 * refScale;
            //double y = dy * 1.0e-3 * refScale;
            //UnitConverterClass unitConverter = new UnitConverterClass();
            //double width = unitConverter.ConvertUnits(x, esriUnits.esriMeters, app.MapControl.MapUnits) ;//宽度
            //double height = unitConverter.ConvertUnits(y, esriUnits.esriMeters, app.MapControl.MapUnits);//高度

            //(ge as ITransform2D).Move(width, height);
            //app.MapControl.ActiveView.GraphicsContainer.UpdateElement(ge as IElement);
            //app.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            //return;
         
            IPolygon clipShp = null;


            IElement pClipElement = null;
            IElement innerMapGeometry = null;
            IPolygon plyPage = null;
            List<IElement> txtEles = new List<IElement>();
            for (int i = 0; i < ge.ElementCount; i++)
            {
                IElement el = ge.get_Element(i);
                IElementProperties3 eleProp = el as IElementProperties3;

                if ("裁切范围" == eleProp.Name)
                {
                    clipShp = el.Geometry as IPolygon;
                    IFillShapeElement polyEle = el as IFillShapeElement;
                    ILineSymbol symbol = polyEle.Symbol.Outline;
                    if (symbol.Width == 2)
                    {
                        pClipElement = el;
                    }
                    else
                    {
                       
                        txtEles.Add(el);
                    }
                }
                else if (eleProp.Name=="文字间距"||eleProp.Name=="线间距")
                {
                    //单独处理
                }
                else
                {
                    if (eleProp.Name == "内图廓")
                    {
                        innerMapGeometry = el;
                        plyPage = el.Geometry as IPolygon;
                    }
                    txtEles.Add(el);
                }
            }
         

            app.MapControl.ActiveView.GraphicsContainer.DeleteAllElements();
           
            for(int i=0;i<txtEles.Count;i++)
            {
                var el = txtEles[i];
                IGeometry geo = el.Geometry;
                geo.Project(targetSpatialReference);
                (geo as ITransform2D).Move(dx * 1.0e-3 * refScale, dy * 1.0e-3 * refScale);
                geo.Project(app.MapControl.Map.SpatialReference);
              //  app.MapControl.ActiveView.GraphicsContainer.DeleteElement(el as IElement);
                var ele = (el as IClone).Clone() as IElement;
                ele.Geometry = geo;
                pGroupElement.AddElement(ele);
            }
         
           
            if (pClipElement != null)
            {

                
                //上边缘距离
                IPolygon proPolygon = (pClipElement.Geometry as IClone).Clone() as IPolygon;
                proPolygon.Project(targetSpatialReference);
              
              
              
                var InMapply = (innerMapGeometry.Geometry as IClone).Clone() as IPolygon;
                InMapply.Project(targetSpatialReference);
                (InMapply as ITransform2D).Move(dx * 1.0e-3 * refScale, dy * 1.0e-3 * refScale);
                double InlineyMax = InMapply.Envelope.YMax;
                double InlineyMin = InMapply.Envelope.YMin;
                double InlinexMin = InMapply.Envelope.XMin;
                double InlinexMax = InMapply.Envelope.XMax;

                var Inlineply = (InMapply as IClone).Clone() as IPolygon;
                Inlineply.Project(app.MapControl.SpatialReference);
                #region 计算横向和纵向角度
                //纵向角度
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                fromPoint.X = InlinexMin; fromPoint.Y = InlineyMin;
                fromPoint.SpatialReference = targetSpatialReference;
                fromPoint.Project(app.MapControl.Map.SpatialReference);
                toPoint.X = InlinexMin; toPoint.Y = InlineyMax;
                toPoint.SpatialReference = targetSpatialReference;
                toPoint.Project(app.MapControl.Map.SpatialReference);
                double Yangle = GetAngle(fromPoint, toPoint);
                //计算横向角度
                fromPoint.X = InlinexMin; fromPoint.Y = InlineyMin;
                fromPoint.SpatialReference = targetSpatialReference;
                fromPoint.Project(app.MapControl.Map.SpatialReference);
                toPoint.X = InlinexMax; toPoint.Y = InlineyMin;
                toPoint.SpatialReference = targetSpatialReference;
                toPoint.Project(app.MapControl.Map.SpatialReference);
                double Xangle = GetAngle(fromPoint, toPoint);
                Xangle = 0;
                #endregion
                #region 裁切面与内图廓上边缘距离
                PolylineClass lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X, Y = proPolygon.Envelope.UpperLeft.Y - 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X, Y = proPolygon.Envelope.UpperRight.Y - 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                IPolyline line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                IPoint center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                PolylineClass lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                PolylineClass linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                var geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = center.X, Y = InlineyMax });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                IRgbColor prgb = new RgbColorClass();
                prgb.Red = 220;
                IArrowMarkerSymbol arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;

                if (!geometry.IsEmpty)
                {

                    //文字
                    double updis = lineTop.Length / refScale * 1000;
                    updis = Math.Round(updis, 2);
                    var txtPoint = new PointClass();
                    lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.3, true, txtPoint);
                    txtPoint.Project(app.MapControl.Map.SpatialReference);
                    TextElementClass txtEle = new TextElementClass();
                    txtEle.Name = "文字间距"; txtEle.FontName = "黑体";
                    txtEle.FontName = "黑体";
                    txtEle.Text = updis + " mm";
                    txtEle.Geometry = txtPoint;
                    txtEle.Rotate(txtPoint, -Xangle);
                    //箭头
                    lineTop.Project(app.MapControl.Map.SpatialReference);
                    MarkerElementClass markerelement = new MarkerElementClass();
                    markerelement.Name = "箭头间距";
                    markerelement.Symbol = arrowmarker;
                    markerelement.Geometry = geometry.FromPoint as IGeometry;
                    //线
                    var frompt = lineTop.FromPoint;
                    lineTop = new PolylineClass();
                    lineTop.AddPoint(frompt);
                    lineTop.AddPoint(geometry.FromPoint);
                    lineTop.Simplify();
                    lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                    LineElementClass lineEl = new LineElementClass();
                    lineEl.Name = "线间距";
                    lineEl.Geometry = lineTop as IGeometry;
                    //箭头角度
                    arrowmarker.Angle = GetRiverAngle(lineTop);
                    //markerelement.Symbol = arrowmarker;

                    // pGroupElement.AddElement(markerelement);
                    pGroupElement.AddElement(lineEl);
                    pGroupElement.AddElement(txtEle);
                }
                #endregion
                #region 下边缘距离
                //下边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X, Y = proPolygon.Envelope.LowerLeft.Y + 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X, Y = proPolygon.Envelope.LowerRight.Y + 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                if (!geometry.IsEmpty)
                {
                    lineTop.AddPoint(new PointClass { X = center.X, Y = InlineyMin });
                    lineTop.Project(targetSpatialReference);
                    (lineTop).Simplify();

                    arrowmarker = new ArrowMarkerSymbolClass();
                    arrowmarker.Color = prgb;



                    //文字
                   double updis = lineTop.Length / refScale * 1000;
                    updis = Math.Round(updis, 2);
                   IPoint  txtPoint = new PointClass();
                    lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.7, true, txtPoint);
                    txtPoint.Project(app.MapControl.Map.SpatialReference);
                    TextElementClass txtEle = new TextElementClass();
                    txtEle.Name = "文字间距"; txtEle.FontName = "黑体";
                    txtEle.Text = updis + " mm";
                    txtEle.Geometry = txtPoint;
                    txtEle.Rotate(txtPoint, -Xangle);

                    //箭头
                    lineTop.Project(app.MapControl.Map.SpatialReference);
                    MarkerElementClass markerelement = new MarkerElementClass();
                    markerelement.Name = "箭头间距";
                    markerelement.Symbol = arrowmarker;
                    markerelement.Geometry = geometry.ToPoint as IGeometry;
                    //线
                    IPoint frompt = lineTop.FromPoint;
                    lineTop = new PolylineClass();
                    lineTop.AddPoint(frompt);
                    lineTop.AddPoint(geometry.ToPoint);
                    lineTop.Simplify();
                    lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                    LineElementClass lineEl = new LineElementClass();
                    lineEl.Name = "线间距";
                    lineEl.Geometry = lineTop as IGeometry;
                    //箭头角度
                    arrowmarker.Angle = GetRiverAngle(lineTop);
                    //markerelement.Symbol = arrowmarker;

                    // pGroupElement.AddElement(markerelement);
                    pGroupElement.AddElement(lineEl);
                    pGroupElement.AddElement(txtEle);
                }
                #endregion
                #region 左边缘距离
                //左边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X + 1, Y = proPolygon.Envelope.UpperLeft.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X + 1, Y = proPolygon.Envelope.LowerLeft.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = InlinexMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = InlinexMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                if (!geometry.IsEmpty)
                {
                    lineTop.AddPoint(new PointClass { X = InlinexMin, Y = center.Y });
                    lineTop.Project(targetSpatialReference);
                    (lineTop).Simplify();

                    arrowmarker = new ArrowMarkerSymbolClass();
                    arrowmarker.Color = prgb;



                    //文字
                    double updis = lineTop.Length / refScale * 1000;
                    updis = Math.Round(updis, 2);
                    PointClass txtPoint = new PointClass();
                    lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                    txtPoint.Project(app.MapControl.Map.SpatialReference);
                    TextElementClass txtEle = new TextElementClass();
                    txtEle.Name = "文字间距"; txtEle.FontName = "黑体";
                    txtEle.Text = updis + " mm";
                    txtEle.Geometry = txtPoint;
                    txtEle.Rotate(txtPoint, -Xangle);

                    //箭头
                    lineTop.Project(app.MapControl.Map.SpatialReference);
                    MarkerElementClass markerelement = new MarkerElementClass();
                    markerelement.Name = "箭头间距";
                    markerelement.Symbol = arrowmarker;
                    markerelement.Geometry = geometry.FromPoint as IGeometry;
                    //线
                    IPoint frompt = lineTop.FromPoint;
                    lineTop = new PolylineClass();
                    lineTop.AddPoint(frompt);
                    lineTop.AddPoint(geometry.FromPoint);
                    lineTop.Simplify();
                    lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                    LineElementClass lineEl = new LineElementClass();
                    lineEl.Name = "线间距";
                    lineEl.Geometry = lineTop as IGeometry;
                    //箭头角度
                    arrowmarker.Angle = GetRiverAngle(lineTop);
                    //markerelement.Symbol = arrowmarker;

                    //pGroupElement.AddElement(markerelement);
                    pGroupElement.AddElement(lineEl);
                    pGroupElement.AddElement(txtEle);
                }
                #endregion
                #region 右边缘距离
                //右边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X - 1, Y = proPolygon.Envelope.UpperRight.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X - 1, Y = proPolygon.Envelope.LowerRight.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = InlinexMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = InlinexMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                if (!geometry.IsEmpty)
                {
                    lineTop.AddPoint(new PointClass { X = InlinexMax, Y = center.Y });
                    lineTop.Project(targetSpatialReference);
                    (lineTop).Simplify();

                    arrowmarker = new ArrowMarkerSymbolClass();
                    arrowmarker.Color = prgb;


                    //文字
                    double  updis = lineTop.Length / refScale * 1000;
                    updis = Math.Round(updis, 2);
                    PointClass txtPoint = new PointClass();
                    lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                    txtPoint.Project(app.MapControl.Map.SpatialReference);
                    TextElementClass txtEle = new TextElementClass();
                    txtEle.Name = "文字间距"; txtEle.FontName = "黑体";
                    txtEle.Text = updis + " mm";
                    txtEle.Geometry = txtPoint;
                    txtEle.Rotate(txtPoint, -Xangle);
                    //箭头
                    lineTop.Project(app.MapControl.Map.SpatialReference);
                    MarkerElementClass markerelement = new MarkerElementClass();
                    markerelement.Name = "箭头间距";
                    markerelement.Symbol = arrowmarker;
                    markerelement.Geometry = geometry.ToPoint as IGeometry;
                    //线
                    IPoint frompt = lineTop.FromPoint;
                    lineTop = new PolylineClass();
                    lineTop.AddPoint(frompt);
                    lineTop.AddPoint(geometry.ToPoint);
                    lineTop.Simplify();
                    lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                    LineElementClass lineEl = new LineElementClass();
                    lineEl.Name = "线间距";
                    lineEl.Geometry = lineTop as IGeometry;
                    //箭头角度
                    arrowmarker.Angle = GetRiverAngle(lineTop);
                    //markerelement.Symbol = arrowmarker;

                    // pGroupElement.AddElement(markerelement);
                    pGroupElement.AddElement(lineEl);
                    pGroupElement.AddElement(txtEle);
                }
                #endregion

              


 
 
                pGroupElement.AddElement(pClipElement);
            }
            app.MapControl.ActiveView.GraphicsContainer.AddElement(pGroupElement as IElement, 0);
            app.MapControl.ActiveView.Refresh();
        }

        public static IGroupElement createClipGroupElementEx(GApplication app, IPolygon cLipShp, double paperWidth, double paperHeight, double refScale, double dx, double dy)
        {

            //添加图形元素
            IGroupElement pGroupElement = new GroupElementClass();
            //目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = app.MapControl.Map.SpatialReference;

            //在目标空间参考系中生成裁切元素
            IMap pTempMap = new MapClass();
            pTempMap.SpatialReference = targetSpatialReference;
            UnitConverterClass unitConverter = new UnitConverterClass();
            double width = unitConverter.ConvertUnits((paperWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//宽度
            double height = unitConverter.ConvertUnits((paperHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//高度
            dx = unitConverter.ConvertUnits((dx) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//高度
            dy = unitConverter.ConvertUnits((dy) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//高度
            IPoint centerPoint = (cLipShp.Envelope as IArea).Centroid;
            if (centerPoint.SpatialReference.Name != targetSpatialReference.Name)
            {
                (centerPoint as IGeometry).Project(targetSpatialReference);//对裁切中心点进行投影变换
                ITransform2D ptrans2d = centerPoint as ITransform2D;
                ptrans2d.Move(dx, dy);
            }
            //生成边框多边形
            double xMax = centerPoint.X + width / 2;
            double yMax = centerPoint.Y + height / 2;
            double xMin = centerPoint.X - width / 2;
            double yMin = centerPoint.Y - height / 2;
            IPolygon ply = CreatePolygon(xMin, xMax, yMin, yMax);
            ply.SpatialReference = targetSpatialReference;

            #region 裁切框中心点
            //中心点符号
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            pSimpleMarkerSymbol.Color = new RgbColorClass { Red = 255 };//颜色
            pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSSquare;//符号类型
            pSimpleMarkerSymbol.Size = 5;//大小
            pSimpleMarkerSymbol.Outline = true;//显示外框线
            pSimpleMarkerSymbol.OutlineColor = new RgbColorClass { Red = 255 };//外框线颜色
            pSimpleMarkerSymbol.OutlineSize = 3;//外框线的宽度

            IElement pMarkerElment = new MarkerElementClass();
            (centerPoint as IGeometry).Project(app.MapControl.Map.SpatialReference);//对裁切中心点进行投影变换
            pMarkerElment.Geometry = centerPoint as IGeometry;//几何数据
            (pMarkerElment as IElementProperties3).Name = "裁切中心点";//名称
            (pMarkerElment as IMarkerElement).Symbol = pSimpleMarkerSymbol;//符号
            #endregion

            #region 裁切范围
            //符号
            ISimpleLineSymbol pClipLineSymbol = new SimpleLineSymbolClass();
            pClipLineSymbol.Width = 2; //边框线宽
            pClipLineSymbol.Color = new RgbColorClass { Red = 255, Green = 61, Blue = 249 };
            IFillSymbol pClipFillSymbol = new SimpleFillSymbolClass();
            pClipFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pClipFillSymbol.Outline = pClipLineSymbol;//边框线

            IElement pClipElement = new PolygonElement();
            cLipShp.Project(app.MapControl.Map.SpatialReference);//投影变换
            pClipElement.Geometry = cLipShp;//几何数据
            (pClipElement as IElementProperties3).Name = "裁切范围";//名称
            (pClipElement as IFillShapeElement).Symbol = pClipFillSymbol; //符号
            #endregion

            #region 出版范围
            //符号
            ISimpleLineSymbol pPageLineSymbol = new SimpleLineSymbolClass();
            pPageLineSymbol.Width = 1; //边框线宽
            pPageLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pPageFillSymbol = new SimpleFillSymbolClass();
            pPageFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pPageFillSymbol.Outline = pPageLineSymbol;//边框线

            IElement pPageElement = new PolygonElement();
            ply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pPageElement.Geometry = ply;//几何数据
            (pPageElement as IElementProperties3).Name = "出版范围";//名称
            (pPageElement as IFillShapeElement).Symbol = pPageFillSymbol; //符号
            #endregion
            try
            {
                #region 边缘距离
                //上边缘距离
                IPolygon proPolygon = (cLipShp as IClone).Clone() as IPolygon;
                proPolygon.Project(targetSpatialReference);
                #region 上边缘距离
                PolylineClass lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X, Y = proPolygon.Envelope.UpperLeft.Y - 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X, Y = proPolygon.Envelope.UpperRight.Y - 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                IPolyline line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                IPoint center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                PolylineClass lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                PolylineClass linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = yMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = yMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                var geometry = (linemid as ITopologicalOperator).Intersect(ply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = center.X, Y = yMax });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                IRgbColor prgb = new RgbColorClass();
                prgb.Red = 220;
                IArrowMarkerSymbol arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                double updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                var txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                TextElementClass txtEle = new TextElementClass();
              // txtEle.Name = "文字间距注释点";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;


                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                MarkerElementClass markerelement = new MarkerElementClass();
              //  markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.FromPoint as IGeometry;
                //线
                var frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.FromPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                LineElementClass lineEl = new LineElementClass();
              //  lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                #endregion
                #region 下边缘距离
                //下边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X, Y = proPolygon.Envelope.LowerLeft.Y + 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X, Y = proPolygon.Envelope.LowerRight.Y + 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = yMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = yMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(ply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = center.X, Y = yMin });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
               // txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;


                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
              //  markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.ToPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.ToPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
              //  lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                #endregion

                #region 左边缘距离
                //左边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X + 1, Y = proPolygon.Envelope.UpperLeft.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X + 1, Y = proPolygon.Envelope.LowerLeft.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = xMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = xMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(ply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = xMin, Y = center.Y });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
              //  txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;


                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
              //  markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.FromPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.FromPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
               // lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                #endregion
                #region 右边缘距离
                //右边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X - 1, Y = proPolygon.Envelope.UpperRight.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X - 1, Y = proPolygon.Envelope.LowerRight.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = xMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = xMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(ply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = xMax, Y = center.Y });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
              //  txtEle.Name = "文字间距注释";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;


                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
              //  markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.ToPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.ToPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
              //  lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop); markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                #endregion
                #endregion
            }
            catch
            {
            }

            pGroupElement.AddElement(pMarkerElment);
            pGroupElement.AddElement(pClipElement);
            pGroupElement.AddElement(pPageElement);

            return pGroupElement;
        }

        public static IGroupElement createClipGroupElementEx2(GApplication app, IPolygon cLipShp, double InlineWidth, double InlineHeight, double refScale, double dx, double dy)
        {

            //添加图形元素
            IGroupElement pGroupElement = new GroupElementClass();
            //目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = app.MapControl.Map.SpatialReference;
            //在目标空间参考系中生成裁切元素
            IMap pTempMap = new MapClass();
            pTempMap.SpatialReference = targetSpatialReference;
            UnitConverterClass unitConverter = new UnitConverterClass();
            double Inlinewidth = unitConverter.ConvertUnits((InlineWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓宽度
            double Inlineheight = unitConverter.ConvertUnits((InlineHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内图廓高度
            double MapSizeWidth = unitConverter.ConvertUnits((CommonMethods.MapSizeWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//成图尺寸宽度（实地——
            double MapSizeHeight = unitConverter.ConvertUnits((CommonMethods.MapSizeHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//成图尺寸高度（实地——
            double PageWidth = unitConverter.ConvertUnits((CommonMethods.PaperWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//页面尺寸宽度（实地——
            double PageHeight = unitConverter.ConvertUnits((CommonMethods.PaperHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//页面尺寸高度（实地——
            double InOutLineWidth = unitConverter.ConvertUnits((CommonMethods.InOutLineWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//内外图廓间距（实地——
            dx = unitConverter.ConvertUnits((dx) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//偏移实地
            dy = unitConverter.ConvertUnits((dy) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//偏移实地     
            var cloneClipShp = ((cLipShp as IClone).Clone() as IGeometry);
            cloneClipShp.Project(targetSpatialReference);
            IGeometry geo = cloneClipShp.Envelope;
            IPoint centerPoint = (geo as IArea).Centroid;
            if (centerPoint.SpatialReference.Name != targetSpatialReference.Name)
            {
                (centerPoint as IGeometry).Project(targetSpatialReference);//对裁切中心点进行投影变换
                ITransform2D ptrans2d = centerPoint as ITransform2D;
                ptrans2d.Move(dx, dy);
            }
            //生成内图廓边框多边形
            double InlinexMax = centerPoint.X + Inlinewidth / 2;
            double InlineyMax = centerPoint.Y + Inlineheight / 2;
            double InlinexMin = centerPoint.X - Inlinewidth / 2;
            double InlineyMin = centerPoint.Y - Inlineheight / 2;
            IPolygon Inlineply = CreatePolygon(InlinexMin, InlinexMax, InlineyMin, InlineyMax);
            Inlineply.SpatialReference = targetSpatialReference;
            //生成外图廓边框多边形
            double OutlinexMax = InlinexMax + InOutLineWidth;
            double OutlineyMax = InlineyMax + InOutLineWidth;
            double OutlinexMin = InlinexMin - InOutLineWidth;
            double OutlineyMin = InlineyMin - InOutLineWidth;
            IPolygon Outlineply = CreatePolygon(OutlinexMin, OutlinexMax, OutlineyMin, OutlineyMax);
            Outlineply.SpatialReference = targetSpatialReference;

            //生成成图尺寸多边形,默认下、左、右留白一样
            double HSpace = (MapSizeWidth - Inlinewidth) / 2;
            double VSpaceUP = (MapSizeHeight - Inlineheight) - (MapSizeWidth - Inlinewidth) / 2;
            double VSpaceDown = HSpace;
            if (VSpaceUP < VSpaceDown)
            {
                VSpaceUP = (MapSizeHeight - Inlineheight) * 0.67;
                VSpaceDown = (MapSizeHeight - Inlineheight) * 0.33;
            }
            double MapsizexMax = InlinexMax + HSpace;
            double MapsizeyMax = InlineyMax + VSpaceUP;
            double MapsizexMin = InlinexMin - HSpace;
            double MapsizeyMin = InlineyMin - VSpaceDown;
            IPolygon Mapsizeply = CreatePolygon(MapsizexMin, MapsizexMax, MapsizeyMin, MapsizeyMax);
            Mapsizeply.SpatialReference = targetSpatialReference;
            //生成页面尺寸
            double PagesizexMax = MapsizexMax + (PageWidth - MapSizeWidth) / 2;
            double PagesizeyMax = MapsizeyMax + (PageHeight - MapSizeHeight) / 2;
            double PagesizexMin = MapsizexMin - (PageWidth - MapSizeWidth) / 2;
            double PagesizeyMin = MapsizeyMin - (PageHeight - MapSizeHeight) / 2;
            IPolygon Pageply = CreatePolygon(PagesizexMin, PagesizexMax, PagesizeyMin, PagesizeyMax);
            Pageply.SpatialReference = targetSpatialReference;

            #region 裁切框中心点
            //中心点符号
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            pSimpleMarkerSymbol.Color = new RgbColorClass { Red = 255 };//颜色
            pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSSquare;//符号类型
            pSimpleMarkerSymbol.Size = 5;//大小
            pSimpleMarkerSymbol.Outline = true;//显示外框线
            pSimpleMarkerSymbol.OutlineColor = new RgbColorClass { Red = 255 };//外框线颜色
            pSimpleMarkerSymbol.OutlineSize = 3;//外框线的宽度

            IElement pMarkerElment = new MarkerElementClass();
            (centerPoint as IGeometry).Project(app.MapControl.Map.SpatialReference);//对裁切中心点进行投影变换
            pMarkerElment.Geometry = centerPoint as IGeometry;//几何数据
            (pMarkerElment as IElementProperties3).Name = "裁切中心点";//名称
            (pMarkerElment as IMarkerElement).Symbol = pSimpleMarkerSymbol;//符号
            #endregion
            #region 裁切范围
            //符号
            ISimpleLineSymbol pClipLineSymbol = new SimpleLineSymbolClass();
            pClipLineSymbol.Width = 2; //边框线宽
            pClipLineSymbol.Color = new RgbColorClass { Red = 255, Green = 61, Blue = 249 };
            IFillSymbol pClipFillSymbol = new SimpleFillSymbolClass();
            pClipFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pClipFillSymbol.Outline = pClipLineSymbol;//边框线

            IElement pClipElement = new PolygonElement();
            cLipShp.Project(app.MapControl.Map.SpatialReference);//投影变换
            pClipElement.Geometry = cLipShp;//几何数据
            (pClipElement as IElementProperties3).Name = "裁切范围";//名称
            (pClipElement as IFillShapeElement).Symbol = pClipFillSymbol; //符号
            #endregion
            #region 内图廓范围
            //符号
            ISimpleLineSymbol pInlineLineSymbol = new SimpleLineSymbolClass();
            pInlineLineSymbol.Width = 1; //边框线宽
            pInlineLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pInlineFillSymbol = new SimpleFillSymbolClass();
            pInlineFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pInlineFillSymbol.Outline = pInlineLineSymbol;//边框线

            IElement pInlineElement = new PolygonElement();
            Inlineply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pInlineElement.Geometry = Inlineply;//几何数据
            (pInlineElement as IElementProperties3).Name = "内图廓";//名称
            (pInlineElement as IFillShapeElement).Symbol = pInlineFillSymbol; //符号
            #endregion
            #region 外图廓范围
            //符号
            ISimpleLineSymbol pOutlineLineSymbol = new SimpleLineSymbolClass();
            pOutlineLineSymbol.Width = 1; //边框线宽
            pOutlineLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pOutlineFillSymbol = new SimpleFillSymbolClass();
            pOutlineFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pOutlineFillSymbol.Outline = pOutlineLineSymbol;//边框线

            IElement pOutlineElement = new PolygonElement();
            Outlineply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pOutlineElement.Geometry = Outlineply;//几何数据
            (pOutlineElement as IElementProperties3).Name = "外图廓";//名称
            (pOutlineElement as IFillShapeElement).Symbol = pOutlineFillSymbol; //符号
            #endregion
            #region 成图范围
            //符号
            ISimpleLineSymbol pMapsizeLineSymbol = new SimpleLineSymbolClass();
            pMapsizeLineSymbol.Width = 1; //边框线宽
            pMapsizeLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pMapsizeFillSymbol = new SimpleFillSymbolClass();
            pMapsizeFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pMapsizeFillSymbol.Outline = pMapsizeLineSymbol;//边框线

            IElement pMapsizeElement = new PolygonElement();
            Mapsizeply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pMapsizeElement.Geometry = Mapsizeply;//几何数据
            (pMapsizeElement as IElementProperties3).Name = "成图范围";//名称
            (pMapsizeElement as IFillShapeElement).Symbol = pMapsizeFillSymbol; //符号
            #endregion
            #region 出版范围
            //符号
            ISimpleLineSymbol pPageLineSymbol = new SimpleLineSymbolClass();
            pPageLineSymbol.Width = 1; //边框线宽
            pPageLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pPageFillSymbol = new SimpleFillSymbolClass();
            pPageFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pPageFillSymbol.Outline = pPageLineSymbol;//边框线

            IElement pPageElement = new PolygonElement();
            Pageply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pPageElement.Geometry = Pageply;//几何数据
            (pPageElement as IElementProperties3).Name = "出版范围";//名称
            (pPageElement as IFillShapeElement).Symbol = pPageFillSymbol; //符号
            #endregion

            try
            {
                pGroupElement.AddElement(pClipElement);
                pGroupElement.AddElement(pInlineElement);
                pGroupElement.AddElement(pMapsizeElement);
                pGroupElement.AddElement(pPageElement);
                pGroupElement.AddElement(pOutlineElement);
                pGroupElement.AddElement(pMarkerElment);
                #region 计算横向和纵向角度
                //纵向角度
                IPoint fromPoint = new PointClass();
                IPoint toPoint = new PointClass();
                fromPoint.X = InlinexMin; fromPoint.Y = InlineyMin;
                fromPoint.SpatialReference = targetSpatialReference;
                fromPoint.Project(app.MapControl.Map.SpatialReference);
                toPoint.X = InlinexMin; toPoint.Y = InlineyMax;
                toPoint.SpatialReference = targetSpatialReference;
                toPoint.Project(app.MapControl.Map.SpatialReference);
                double Yangle = GetAngle(fromPoint, toPoint);
                //计算横向角度
                fromPoint.X = InlinexMin; fromPoint.Y = InlineyMin;
                fromPoint.SpatialReference = targetSpatialReference;
                fromPoint.Project(app.MapControl.Map.SpatialReference);
                toPoint.X = InlinexMax; toPoint.Y = InlineyMin;
                toPoint.SpatialReference = targetSpatialReference;
                toPoint.Project(app.MapControl.Map.SpatialReference);
                double Xangle = GetAngle(fromPoint, toPoint);
                Xangle = 0;
                #endregion


                #region 边缘距离
                //上边缘距离
                IPolygon proPolygon = (cLipShp as IClone).Clone() as IPolygon;
                proPolygon.Project(targetSpatialReference);

                #region 裁切面与内图廓上边缘距离
                PolylineClass lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X, Y = proPolygon.Envelope.UpperLeft.Y - 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X, Y = proPolygon.Envelope.UpperRight.Y - 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                IPolyline line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                IPoint center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                PolylineClass lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                PolylineClass linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                var geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = center.X, Y = InlineyMax });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                IRgbColor prgb = new RgbColorClass();
                prgb.Red = 220;
                IArrowMarkerSymbol arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                double updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                var txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.3, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                TextElementClass txtEle = new TextElementClass();
                txtEle.Name = "文字间距"; txtEle.FontName = "黑体";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                MarkerElementClass markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.FromPoint as IGeometry;
                //线
                var frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.FromPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                LineElementClass lineEl = new LineElementClass();
                lineEl.Name = "线间距";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion
                #region 下边缘距离
                //下边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X, Y = proPolygon.Envelope.LowerLeft.Y + 1 });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X, Y = proPolygon.Envelope.LowerRight.Y + 1 });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMax + 10 });
                linemid.AddPoint(new PointClass { X = center.X, Y = InlineyMin - 10 });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = center.X, Y = InlineyMin });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.7, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距"; txtEle.FontName = "黑体";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.ToPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.ToPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
                lineEl.Name = "线间距";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion
                #region 左边缘距离
                //左边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperLeft.X + 1, Y = proPolygon.Envelope.UpperLeft.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerLeft.X + 1, Y = proPolygon.Envelope.LowerLeft.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = InlinexMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = InlinexMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = InlinexMin, Y = center.Y });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;



                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距"; txtEle.FontName = "黑体";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.FromPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.FromPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
                lineEl.Name = "线间距";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion
                #region 右边缘距离
                //右边缘距离

                lineUp = new PolylineClass();
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.UpperRight.X - 1, Y = proPolygon.Envelope.UpperRight.Y });
                lineUp.AddPoint(new PointClass { X = proPolygon.Envelope.LowerRight.X - 1, Y = proPolygon.Envelope.LowerRight.Y });
                (lineUp as ITopologicalOperator).Simplify();
                lineUp.Project(targetSpatialReference);
                line = (lineUp as ITopologicalOperator).Intersect(proPolygon, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                center = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);

                lineTop = new PolylineClass();
                lineTop.AddPoint(center);
                //第二个点
                linemid = new PolylineClass();
                linemid.AddPoint(new PointClass { X = InlinexMin - 10, Y = center.Y });
                linemid.AddPoint(new PointClass { X = InlinexMax + 10, Y = center.Y });
                linemid.SpatialReference = targetSpatialReference;
                linemid.Project(app.MapControl.Map.SpatialReference);
                linemid.Simplify();
                geometry = (linemid as ITopologicalOperator).Intersect(Inlineply, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                lineTop.AddPoint(new PointClass { X = InlinexMax, Y = center.Y });
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;


                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距"; txtEle.FontName = "黑体";
                txtEle.Text = updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                //箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = geometry.ToPoint as IGeometry;
                //线
                frompt = lineTop.FromPoint;
                lineTop = new PolylineClass();
                lineTop.AddPoint(frompt);
                lineTop.AddPoint(geometry.ToPoint);
                lineTop.Simplify();
                lineTop.SpatialReference = app.MapControl.Map.SpatialReference;
                lineEl = new LineElementClass();
                lineEl.Name = "线间距";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;

                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                pGroupElement.AddElement(txtEle);
                #endregion

                #region 内外图廓距离
                //上边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = InlinexMin + (InlinexMax - InlinexMin) / 3, Y = InlineyMax });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = InlinexMin + (InlinexMax - InlinexMin) / 3, Y = InlineyMax + InOutLineWidth });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();

                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;

                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.3, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "内外图廓间距" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);

                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;

                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);
                #endregion



                #region 外图廓距离裁切线间距
                //上边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = OutlineyMax });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = MapsizeyMax });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                CommonMethods.MapSizeTopInterval = updis;
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);





                //右边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = OutlinexMax, Y = (OutlineyMin + OutlineyMax) / 2 });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = MapsizexMax, Y = (OutlineyMin + OutlineyMax) / 2 });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);



                //左边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = OutlinexMin, Y = (OutlineyMin + OutlineyMax) / 2 });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = MapsizexMin, Y = (OutlineyMin + OutlineyMax) / 2 });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                //pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);



                //下边缘距离               
                lineTop = new PolylineClass();
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = OutlineyMin });//第一个点
                //第二个点  
                lineTop.AddPoint(new PointClass { X = (OutlinexMin + OutlinexMax) / 2, Y = MapsizeyMin });//第二个点
                lineTop.Project(targetSpatialReference);
                (lineTop).Simplify();
                arrowmarker = new ArrowMarkerSymbolClass();
                arrowmarker.Color = prgb;
                //文字
                updis = lineTop.Length / refScale * 1000;
                updis = Math.Round(updis, 2);
                txtPoint = new PointClass();
                lineTop.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtPoint);
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓距裁切线" + updis + " mm";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                CommonMethods.MapSizeDownInterval = updis;
                ////箭头
                lineTop.Project(app.MapControl.Map.SpatialReference);
                markerelement = new MarkerElementClass();
                markerelement.Name = "箭头间距注释";
                markerelement.Symbol = arrowmarker;
                markerelement.Geometry = lineTop.ToPoint as IGeometry;
                //线
                lineEl = new LineElementClass();
                lineEl.Name = "线间距注释";
                lineEl.Geometry = lineTop as IGeometry;
                //箭头角度
                arrowmarker.Angle = GetRiverAngle(lineTop);
                //markerelement.Symbol = arrowmarker;
                pGroupElement.AddElement(txtEle);
                // pGroupElement.AddElement(markerelement);
                pGroupElement.AddElement(lineEl);

                #endregion
                //更新视图


                IEnvelope env = cLipShp.Envelope;
                    env.Expand(1.6, 1.6, true);
                    app.MapControl.Extent = env;
                app.MapControl.ActiveView.Refresh();

                if (app.MapControl.ActiveView.FocusMap.MapScale < 100)
                {

                    env = Pageply.Envelope;
                    env.Expand(1.6, 1.6, true);
                    app.MapControl.Extent = env;
                    app.MapControl.ActiveView.Refresh();
                }

                #region 框线文字
                txtPoint = new PointClass();
                txtPoint.X = InlinexMax; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "内\n图\n廓";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtPoint.X = OutlinexMin; txtPoint.Y = OutlineyMin + (OutlineyMax - OutlineyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外\n图\n廓";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtPoint.X = MapsizexMax; txtPoint.Y = MapsizeyMin + (MapsizeyMax - MapsizeyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "成\n图\n尺\n寸";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();
                txtPoint.X = PagesizexMin; txtPoint.Y = PagesizeyMin + (PagesizeyMax - PagesizeyMin) / 5;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "纸\n张";
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, Yangle - Math.PI / 2);
                pGroupElement.AddElement(txtEle);

                #endregion
                #region 各框线尺寸文字
                txtPoint = new PointClass();               
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";              
                txtEle.Text = "内图廓尺寸,宽:" + CommonMethods.InlineWidth + "，高:" + CommonMethods.InlineHeight + "（mm）";
                double textsize = txtEle.Text.Length * (txtEle.Size/2.8345) *(app.MapControl.ActiveView.FocusMap.MapScale /1000);
                txtPoint.X = InlinexMin + textsize/2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 4;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                  
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);
                txtPoint = new PointClass();   
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "外图廓尺寸,宽:" + (CommonMethods.InlineWidth + CommonMethods.InOutLineWidth * 2) + "，高:" + (CommonMethods.InlineHeight + CommonMethods.InOutLineWidth * 2) + "（mm）";
                textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 3;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();      
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "成图尺寸,宽:" + CommonMethods.MapSizeWidth + "，高:" + CommonMethods.MapSizeHeight + "（mm）";
                textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 2;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                txtPoint = new PointClass();     
                txtEle = new TextElementClass();
                txtEle.Name = "文字间距注释";
                txtEle.Text = "纸张尺寸,宽:" + CommonMethods.PaperWidth + "，高:" + CommonMethods.PaperHeight + "（mm）";
                textsize = txtEle.Text.Length * (txtEle.Size / 2.8345) * (app.MapControl.ActiveView.FocusMap.MapScale / 1000);
                txtPoint.X = InlinexMin + textsize / 2; txtPoint.Y = InlineyMin + (InlineyMax - InlineyMin) / 15 * 1;
                txtPoint.SpatialReference = targetSpatialReference;
                txtPoint.Project(app.MapControl.Map.SpatialReference);
                txtEle.Geometry = txtPoint;
                txtEle.Rotate(txtPoint, -Xangle);
                pGroupElement.AddElement(txtEle);

                #endregion

                #endregion
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }

            return pGroupElement;
        }
        private static double GetRiverAngle(IPolyline pLine)
        {

            double dx = pLine.ToPoint.X - pLine.FromPoint.X;
            double dy = pLine.ToPoint.Y - pLine.FromPoint.Y;
            double angle = Math.Atan(Math.Abs(pLine.ToPoint.Y - pLine.FromPoint.Y) / Math.Abs(pLine.ToPoint.X - pLine.FromPoint.X));
            angle = angle / Math.PI * 180;
            //顺时针旋转
            if (dx > 0 && dy > 0)//第一象限
            {
                // angle = 360 - angle;

            }
            else if (dx <= 0 && dy >= 0)//第二象限
            {
                angle = 180 - angle;
            }
            else if ((dx < 0 && dy < 0))//第三象限
            {
                angle = 180 + angle;
            }
            else if (dx >= 0 && dy <= 0)//第四象限
            {
                angle = 360 - angle;
                // angle = 360 - angle;
            }
            return angle;
        }
        /// <summary>
        /// 计算两点连线的角度
        /// </summary>
        /// <param name="FromPoint">第一个点</param>
        /// <param name="ToPoint">第二个点</param>
        /// <returns>角度范围[-1/2Pi,-1/2Pi]</returns>
        private static double GetAngle(IPoint FromPoint, IPoint ToPoint)
        {

            double dx = ToPoint.X - FromPoint.X;
            double dy = ToPoint.Y - FromPoint.Y;
            if (ToPoint.Y - FromPoint.Y == 0)
            {
                return Math.PI / 2;
            }
            double angle = Math.Atan(Math.Abs(ToPoint.Y - FromPoint.Y) / Math.Abs(ToPoint.X - FromPoint.X));
            return angle;
        }


        public static IGroupElement setClipGroupElement(ISpatialReference targetSpatialReference, GApplication app, IPolygon cLipShp, double paperWidth, double paperHeight, double refScale, double dx, double dy)
        {
            //目标空间坐标系

            //在目标空间参考系中生成裁切元素
            IMap pTempMap = new MapClass();
            pTempMap.SpatialReference = targetSpatialReference;
            UnitConverterClass unitConverter = new UnitConverterClass();
            double width = unitConverter.ConvertUnits((paperWidth) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//宽度
            double height = unitConverter.ConvertUnits((paperHeight) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//高度
            dx = unitConverter.ConvertUnits((dx) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//高度
            dy = unitConverter.ConvertUnits((dy) / 1000, esriUnits.esriMeters, pTempMap.MapUnits) * refScale;//高度
            IPoint centerPoint = (cLipShp.Envelope as IArea).Centroid;
            if (centerPoint.SpatialReference.Name != targetSpatialReference.Name)
            {
                (centerPoint as IGeometry).Project(targetSpatialReference);//对裁切中心点进行投影变换 
            }
            ITransform2D ptrans2d = centerPoint as ITransform2D;
            ptrans2d.Move(dx, dy);
            //生成边框多边形
            double xMax = centerPoint.X + width / 2;
            double yMax = centerPoint.Y + height / 2;
            double xMin = centerPoint.X - width / 2;
            double yMin = centerPoint.Y - height / 2;
            IPolygon ply = CreatePolygon(xMin, xMax, yMin, yMax);
            ply.SpatialReference = targetSpatialReference;

            #region 裁切框中心点
            //中心点符号
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            pSimpleMarkerSymbol.Color = new RgbColorClass { Red = 255 };//颜色
            pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSSquare;//符号类型
            pSimpleMarkerSymbol.Size = 5;//大小
            pSimpleMarkerSymbol.Outline = true;//显示外框线
            pSimpleMarkerSymbol.OutlineColor = new RgbColorClass { Red = 255 };//外框线颜色
            pSimpleMarkerSymbol.OutlineSize = 3;//外框线的宽度

            IElement pMarkerElment = new MarkerElementClass();
            (centerPoint as IGeometry).Project(app.MapControl.Map.SpatialReference);//对裁切中心点进行投影变换
            pMarkerElment.Geometry = centerPoint as IGeometry;//几何数据
            (pMarkerElment as IElementProperties3).Name = "裁切中心点";//名称
            (pMarkerElment as IMarkerElement).Symbol = pSimpleMarkerSymbol;//符号
            #endregion

            #region 裁切范围
            //符号
            ISimpleLineSymbol pClipLineSymbol = new SimpleLineSymbolClass();
            pClipLineSymbol.Width = 2; //边框线宽
            pClipLineSymbol.Color = new RgbColorClass { Red = 255, Green = 61, Blue = 249 };
            IFillSymbol pClipFillSymbol = new SimpleFillSymbolClass();
            pClipFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pClipFillSymbol.Outline = pClipLineSymbol;//边框线

            IElement pClipElement = new PolygonElement();
            cLipShp.Project(app.MapControl.Map.SpatialReference);//投影变换
            pClipElement.Geometry = cLipShp;//几何数据
            (pClipElement as IElementProperties3).Name = "裁切范围";//名称
            (pClipElement as IFillShapeElement).Symbol = pClipFillSymbol; //符号
            #endregion

            #region 出版范围
            //符号
            ISimpleLineSymbol pPageLineSymbol = new SimpleLineSymbolClass();
            pPageLineSymbol.Width = 1; //边框线宽
            pPageLineSymbol.Color = new RgbColorClass { Red = 128, Green = 31, Blue = 125 };
            IFillSymbol pPageFillSymbol = new SimpleFillSymbolClass();
            pPageFillSymbol.Color = new RgbColorClass { NullColor = true };//填充颜色
            pPageFillSymbol.Outline = pPageLineSymbol;//边框线

            IElement pPageElement = new PolygonElement();
            ply.Project(app.MapControl.Map.SpatialReference);//投影变换
            pPageElement.Geometry = ply;//几何数据
            (pPageElement as IElementProperties3).Name = "出版范围";//名称
            (pPageElement as IFillShapeElement).Symbol = pPageFillSymbol; //符号
            #endregion


            //添加图形元素
            IGroupElement pGroupElement = new GroupElementClass();
            pGroupElement.AddElement(pMarkerElment);
            pGroupElement.AddElement(pClipElement);
            pGroupElement.AddElement(pPageElement);

            return pGroupElement;
        }

        /// <summary>
        /// 由于空间参考的变化，更新裁切元素
        /// </summary>
        /// <param name="app"></param>
        /// <param name="outputRef"></param>
        public static void updateClipGroupElement(GApplication app, ISpatialReference outputRef)
        {

            IGroupElement ge = getClipGroupElement(app.MapControl.ActiveView.GraphicsContainer);

            if (ge == null)
                return;

            IPoint centerPoint = null;
            IPolygon clipShp = null;
            bool bHasShpFile = false;
            for (int i = 0; i < ge.ElementCount; i++)
            {
                IElement el = ge.get_Element(i);
                IElementProperties3 eleProp = el as IElementProperties3;
                if ("裁切中心点" == eleProp.Name)
                {
                    centerPoint = el.Geometry as IPoint;
                }
                else if ("裁切范围" == eleProp.Name)
                {
                    clipShp = el.Geometry as IPolygon;
                }
                //else if ("出版范围" == eleProp.Name)
                //{
                //    bHasShpFile = true;
                //}









                if ("裁切范围" == eleProp.Name)
                {
                    clipShp = el.Geometry as IPolygon;
                    IFillShapeElement polyEle = el as IFillShapeElement;
                    ILineSymbol symbol = polyEle.Symbol.Outline;
                    if (symbol.Width == 2)
                    {
                        bHasShpFile = true;
                    }
                    else
                    {

                        bHasShpFile = false;
                    }
                }

            }






            //更新裁切元素
            double paperWidth = 0, paperHeight = 0, refScale = 1;
            var content = EnvironmentSettings.getContentElement(app);
            var pagesize = content.Element("PageSize");
            var mapScale = content.Element("MapScale");

            refScale = Convert.ToDouble(mapScale.Value);
            paperWidth = Convert.ToDouble(pagesize.Element("Width").Value);
            paperHeight = Convert.ToDouble(pagesize.Element("Height").Value);

            IGroupElement clipELe = null;
            if (bHasShpFile)
            {
                //clipELe = ClipElement.createClipGroupElement(app, clipShp, paperWidth, paperHeight, refScale);
                clipELe = ClipElement.createClipGroupElementEx2(app, clipShp, CommonMethods.InlineWidth, CommonMethods.InlineHeight, refScale, 0, 0);
            }
            else
            {
                clipELe = ClipElement.createClipGroupElement(app, centerPoint, paperWidth, paperHeight, refScale);
                
               
            }
            ClipElement.SetClipGroupElement(app, clipELe);
        }


        /// <summary>
        /// 生成页面矩形(根据裁切几何体)
        /// </summary>
        /// <param name="app"></param>
        /// <param name="outputRef">输出的投影坐标系</param>
        /// <returns></returns>
        public static IGeometry createPageRect(GApplication app, ISpatialReference outputRef, string PageType)
        {
            IGeometry geo = null;

            //获取裁切范围,出版范围

            IPolygon Ply = null;
            IGroupElement ge = getClipGroupElement(app.MapControl.ActiveView.GraphicsContainer);
            if (ge == null)
                return geo;
            for (int i = 0; i < ge.ElementCount; i++)
            {
                IElement el = ge.get_Element(i);
                IElementProperties3 eleProp = el as IElementProperties3;

                if (PageType == eleProp.Name)
                {
                    Ply = el.Geometry as IPolygon;
                }
            }

            if (Ply.SpatialReference.Name != outputRef.Name)
            {
                Ply.Project(outputRef);//对出版范围进行投影变换
            }
            //生成矩形
            IEnvelope pExtent = Ply.Envelope;
            geo = CreatePolygon(pExtent.XMin, pExtent.XMax, pExtent.YMin, pExtent.YMax);
            geo.Project(outputRef);
            return geo;
        }

        /// <summary>
        /// 根据内图廓与纸张生成压盖面
        /// </summary>
        /// <param name="Inlinegeo">内图廓</param>
        /// <param name="Pagegeo">纸张</param>
        /// <returns>亚盖面</returns>
        public static IGeometry createOvelapRect(IGeometry Inlinegeo, IGeometry Pagegeo)
        {


            //计算内图廓矩形框四个节点的坐标
            double xminIn = Inlinegeo.Envelope.XMin;
            double xmaxIn = Inlinegeo.Envelope.XMax;
            double ymaxIn = Inlinegeo.Envelope.YMax;
            double yminIn = Inlinegeo.Envelope.YMin;
            //
            double xminlap = Pagegeo.Envelope.XMin - 100;
            double xmaxlap = Pagegeo.Envelope.XMax + 100;
            double yminlap = Pagegeo.Envelope.YMin - 100;
            double ymaxlap = Pagegeo.Envelope.YMax + 100;
            IGeometryCollection geoCol = new PolygonClass();//生成遮盖面
            #region
            IPointCollection ringout = new RingClass();
            ringout.AddPoint(new PointClass { X = xminlap, Y = ymaxlap });
            ringout.AddPoint(new PointClass { X = xmaxlap, Y = ymaxlap });
            ringout.AddPoint(new PointClass { X = xmaxlap, Y = yminlap });
            ringout.AddPoint(new PointClass { X = xminlap, Y = yminlap });
            ringout.AddPoint(new PointClass { X = xminlap, Y = ymaxlap });
            geoCol.AddGeometry(ringout as IGeometry);

            IPointCollection ringin = new RingClass();
            ringin.AddPoint(new PointClass { X = xminIn, Y = ymaxIn });
            ringin.AddPoint(new PointClass { X = xmaxIn, Y = ymaxIn });
            ringin.AddPoint(new PointClass { X = xmaxIn, Y = yminIn });
            ringin.AddPoint(new PointClass { X = xminIn, Y = yminIn });
            ringin.AddPoint(new PointClass { X = xminIn, Y = ymaxIn });
            geoCol.AddGeometry(ringin as IGeometry);
            (geoCol as ITopologicalOperator).Simplify();
            #endregion

            return geoCol as IGeometry;



        }

        /// <summary>
        /// 生成图廓面
        /// </summary>
        /// <param name="app"></param>
        /// <param name="outputRef">输出的投影坐标系</param>
        /// <returns></returns>
        public static IGeometry createInlinePolyline(GApplication app, ISpatialReference outputRef, string PageType)
        {
            IGeometry geo = null;

            //获取裁切范围,出版范围

            IPolygon Ply = null;
            IGroupElement ge = getClipGroupElement(app.MapControl.ActiveView.GraphicsContainer);
            if (ge == null)
                return geo;
            for (int i = 0; i < ge.ElementCount; i++)
            {
                IElement el = ge.get_Element(i);
                IElementProperties3 eleProp = el as IElementProperties3;

                if (PageType == eleProp.Name)
                {
                    Ply = el.Geometry as IPolygon;
                }
            }

            if (Ply.SpatialReference.Name != outputRef.Name)
            {
                Ply.Project(outputRef);//对出版范围进行投影变换
            }
            //生成矩形
            IEnvelope pExtent = Ply.Envelope;

            geo = CreatePolylineBy4Points(pExtent.XMin, pExtent.XMax, pExtent.YMin, pExtent.YMax);
            geo.Project(outputRef);
            return geo;
        }


        /// <summary>
        /// 根据成图尺寸生成丁字线
        /// </summary>
        /// <param name="app"></param>
        /// <param name="outputRef">输出的投影坐标系</param>
        /// <returns></returns>
        public static List<IGeometry> createMapTlinePolyline(GApplication app, ISpatialReference outputRef)
        {
            double refscale = CommonMethods.MapScale;
            List<IGeometry> Tlinegeo = new List<IGeometry>();
            IPolygon MapPly = null;//成图范围
            IGroupElement ge = getClipGroupElement(app.MapControl.ActiveView.GraphicsContainer);
            if (ge == null)
                return Tlinegeo;
            for (int i = 0; i < ge.ElementCount; i++)
            {
                IElement el = ge.get_Element(i);
                IElementProperties3 eleProp = el as IElementProperties3;

                if ("成图范围" == eleProp.Name)
                {
                    MapPly = el.Geometry as IPolygon;
                }
            }
            if (MapPly != null)
            {
                if (MapPly.SpatialReference.Name != outputRef.Name)
                {
                    MapPly.Project(outputRef);//进行投影变换
                }
            }
            //生成矩形
            IEnvelope pExtent = MapPly.Envelope;
            IPointCollection pcline = new PolylineClass();
            IPoint p1 = new PointClass();
            IPoint p2 = new PointClass();
            //下
            p1.PutCoords((pExtent.XMin + pExtent.XMax) / 2 - 5 * 1.0e-3 * refscale, pExtent.YMin);
            p2.PutCoords((pExtent.XMin + pExtent.XMax) / 2 + 5 * 1.0e-3 * refscale, pExtent.YMin);
            pcline.AddPoint(p1);
            pcline.AddPoint(p2);
            IGeometry geo = pcline as IGeometry;
            geo.Project(outputRef);
            Tlinegeo.Add(geo);
            pcline = new PolylineClass();

            //下
            p1.PutCoords((pExtent.XMin + pExtent.XMax) / 2, pExtent.YMin);
            p2.PutCoords((pExtent.XMin + pExtent.XMax) / 2, pExtent.YMin - 5 * 1.0e-3 * refscale);
            pcline.AddPoint(p1);
            pcline.AddPoint(p2);
            geo = pcline as IGeometry;
            geo.Project(outputRef);
            Tlinegeo.Add(geo);
            pcline = new PolylineClass();

            //左
            p1.PutCoords(pExtent.XMin, (pExtent.YMin + pExtent.YMax) / 2 - 5 * 1.0e-3 * refscale);
            p2.PutCoords(pExtent.XMin, (pExtent.YMin + pExtent.YMax) / 2 + 5 * 1.0e-3 * refscale);
            pcline.AddPoint(p1);
            pcline.AddPoint(p2);
            geo = pcline as IGeometry;
            geo.Project(outputRef);
            Tlinegeo.Add(geo);
            pcline = new PolylineClass();
            //左
            p1.PutCoords(pExtent.XMin - 5 * 1.0e-3 * refscale, (pExtent.YMin + pExtent.YMax) / 2);
            p2.PutCoords(pExtent.XMin, (pExtent.YMin + pExtent.YMax) / 2);
            pcline.AddPoint(p1);
            pcline.AddPoint(p2);
            geo = pcline as IGeometry;
            geo.Project(outputRef);
            Tlinegeo.Add(geo);
            pcline = new PolylineClass();
            //上
            p1.PutCoords((pExtent.XMin + pExtent.XMax) / 2 - 5 * 1.0e-3 * refscale, pExtent.YMax);
            p2.PutCoords((pExtent.XMin + pExtent.XMax) / 2 + 5 * 1.0e-3 * refscale, pExtent.YMax);
            pcline.AddPoint(p1);
            pcline.AddPoint(p2);
            geo = pcline as IGeometry;
            geo.Project(outputRef);
            Tlinegeo.Add(geo);
            pcline = new PolylineClass();
            //上
            p1.PutCoords((pExtent.XMin + pExtent.XMax) / 2, pExtent.YMax + 5 * 1.0e-3 * refscale);
            p2.PutCoords((pExtent.XMin + pExtent.XMax) / 2, pExtent.YMax);
            pcline.AddPoint(p1);
            pcline.AddPoint(p2);
            geo = pcline as IGeometry;
            geo.Project(outputRef);
            Tlinegeo.Add(geo);
            pcline = new PolylineClass();
            //右
            p1.PutCoords(pExtent.XMax, (pExtent.YMin + pExtent.YMax) / 2 - 5 * 1.0e-3 * refscale);
            p2.PutCoords(pExtent.XMax, (pExtent.YMin + pExtent.YMax) / 2 + 5 * 1.0e-3 * refscale);
            pcline.AddPoint(p1);
            pcline.AddPoint(p2);
            geo = pcline as IGeometry;
            geo.Project(outputRef);
            Tlinegeo.Add(geo);
            pcline = new PolylineClass();

            //右
            p1.PutCoords(pExtent.XMax + 5 * 1.0e-3 * refscale, (pExtent.YMin + pExtent.YMax) / 2);
            p2.PutCoords(pExtent.XMax, (pExtent.YMin + pExtent.YMax) / 2);
            pcline.AddPoint(p1);
            pcline.AddPoint(p2);
            geo = pcline as IGeometry;
            geo.Project(outputRef);
            Tlinegeo.Add(geo);
            pcline = new PolylineClass();



            return Tlinegeo;
        }



        /// <summary>
        /// 利用四个角点创建闭合多变形
        /// </summary>
        /// <param name="x_min"></param>
        /// <param name="x_max"></param>
        /// <param name="y_min"></param>
        /// <param name="y_max"></param>
        /// <returns></returns>
        private static IPolygon CreatePolygon(double x_min, double x_max, double y_min, double y_max)
        {
            IPoint pt1 = new PointClass { X = x_min, Y = y_min };
            IPoint pt2 = new PointClass { X = x_min, Y = y_max };
            IPoint pt3 = new PointClass { X = x_max, Y = y_max };
            IPoint pt4 = new PointClass { X = x_max, Y = y_min };

            IPointCollection pPtsCol = new PolygonClass();
            pPtsCol.AddPoint(pt1);
            pPtsCol.AddPoint(pt2);
            pPtsCol.AddPoint(pt3);
            pPtsCol.AddPoint(pt4);

            IPolygon ply = pPtsCol as IPolygon;
            ply.Close();

            return ply;
        }
        public static IPolyline CreatePolylineBy4Points(double x_min, double x_max, double y_min, double y_max)
        {
            IPoint Pt1 = new PointClass();
            Pt1.PutCoords(x_min, y_min);
            IPoint Pt2 = new PointClass();
            Pt2.PutCoords(x_min, y_max);
            IPoint Pt3 = new PointClass();
            Pt3.PutCoords(x_max, y_max);
            IPoint Pt4 = new PointClass();
            Pt4.PutCoords(x_max, y_min);

            IPointCollection pPtsCol = new PolylineClass();
            object missing = Type.Missing;

            pPtsCol.AddPoint(Pt1, ref missing, ref missing);
            pPtsCol.AddPoint(Pt2, ref missing, ref missing);
            pPtsCol.AddPoint(Pt3, ref missing, ref missing);
            pPtsCol.AddPoint(Pt4, ref missing, ref missing);
            pPtsCol.AddPoint(Pt1, ref missing, ref missing); //// 为保持首尾相联，故将第一个点再添加一次

            return (pPtsCol as IPolyline);
        }

    }
}

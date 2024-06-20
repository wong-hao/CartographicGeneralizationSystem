using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;

namespace SMGI.Plugin.AnnotationEngine
{
    /// <summary>
    /// 注记打散,字向朝北,只处理河流等不规则注记
    /// </summary>
    public class AnnoBreakFaceToNorth
    {
        private GApplication _app;//应用程序
        private string AnnoLyrName = "";
        public AnnoBreakFaceToNorth(GApplication app,string annoName)
        {
            _app = app;
            AnnoLyrName = annoName;
        }

        public string toNorth(double northAngle = 0, WaitOperation wo = null)
        {
            string msg = "";

            try
            {
                var lyrs = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                { return (l.Name == AnnoLyrName )&&(l is IFDOGraphicsLayer); })).ToArray();

                if (lyrs.Length > 0)
                {
                    for (int i = 0; i < lyrs.Length; i++)
                    {
                        IFeatureLayer annoFeatureLayer = lyrs[i] as IFeatureLayer;

                        if (wo != null)
                            wo.SetText("正在处理" + annoFeatureLayer.Name + "……");

                        IFeatureClass annoFeatureClass = annoFeatureLayer.FeatureClass;
                        IFeatureCursor annoCursor = annoFeatureClass.Search(null, false);
                        IFeature annoFeature = null;
                        while ((annoFeature = annoCursor.NextFeature()) != null)
                        {
                            //判断该注记要素是否是水系
                            IAnnotationFeature2 annoFeature2 = annoFeature as IAnnotationFeature2;
                            var plyrs = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                            { return l is IGeoFeatureLayer&&(l as IFeatureLayer).FeatureClass.ObjectClassID==annoFeature2.AnnotationClassID; })).ToArray();
                            if (plyrs.Length == 0)
                                continue;
                            IFeatureClass fc = (plyrs[0] as IFeatureLayer).FeatureClass;
                            if (fc.AliasName.ToUpper() != "HYDL")
                                continue;

                            ITextElement te = annoFeature2.Annotation as ITextElement;
                            if (te != null && te.Text.Contains("("))//带括号的注记
                            {
                                try
                                {
                                    int gbIndex = fc.FindField("GB");
                                    IFeature linkFe = fc.GetFeature(annoFeature2.LinkedFeatureID);

                                    if (linkFe.get_Value(gbIndex).ToString() == "210200")//时令月份等带括号的注记不做字头朝北处理，如“(6-9)”
                                        continue;

                                }
                                catch
                                {
                                }
                                
                            }

                            SplitAnnoFeature(annoFeature, northAngle);
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(annoCursor);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                msg = ex.Message;
            }

            return msg;
        }

        /// <summary>
        /// 打散注记符号要素
        /// </summary>
        /// <param name="annoFeature">待打散的Feature的注记要素</param>
        /// <returns>返回打散要素的一个要素</returns>
        /// <param name="northAngle">旋转角度</param>
        private void SplitAnnoFeature(IFeature annoFeature, double northAngle = 0)
        {
            //获取注记要素Feature的文本符号元素TextElement
            IAnnotationFeature pAnnoFeature = annoFeature as IAnnotationFeature;
            if (pAnnoFeature == null) return;
            ITextElement pTextElement = pAnnoFeature.Annotation as ITextElement;
            if (pTextElement == null) return;
            IGeometry textElementGeometry = (pTextElement as IElement).Geometry;
            if (textElementGeometry is IPolyline)
            {
                //折线型注记,折线型注记也分两种（直线型和折线型）
                IPointCollection pointCollection = textElementGeometry as IPointCollection;
                if (pointCollection.PointCount == 2)
                {
                    //直线型注记，同属简单的矩形框注记，采用常规的方法进行打断
                    RegularAnnoSplit(annoFeature, pAnnoFeature as IAnnotationFeature2, pTextElement, northAngle);
                }
                if (pointCollection.PointCount > 2)
                {
                    //折线型注记，此时一般PointCount为注记文字的个数的2倍，2个点表示一个注记位置
                    if (pointCollection.PointCount != pTextElement.Text.Length * 2)
                    {
                        ////这种注记为部分折线和部分直线组成（如果存在的话)
                        NonRegularAnnoSplit2(annoFeature, northAngle);
                    }
                    else
                    {
                        NonRegularAnnoSplit(annoFeature, northAngle);
                    }
                }
            }
        }

        /// <summary>
        /// 非规则性注记打断的方式
        /// </summary>
        /// <param name="annoFeature"></param>
        /// <param name="northAngle"></param>
        private void NonRegularAnnoSplit(IFeature annoFeature, double northAngle = 0) 
        {
            //获取annoFeature的要素图层
            IFeatureLayer pAnnoFeatureLayer = null;
            var lyrs = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IFDOGraphicsLayer; })).ToArray();
            for (int i = 0; i < lyrs.Length; i++)
            {
                IFeatureLayer pFeatureLayer = lyrs[i] as IFeatureLayer;
                if (pFeatureLayer != null && pFeatureLayer.FeatureClass.AliasName == annoFeature.Class.AliasName)
                {
                    pAnnoFeatureLayer = pFeatureLayer;
                    break;
                }
            }
            if (pAnnoFeatureLayer == null) return;
            //打散注记
            IFeatureClass annoFeatureClass = pAnnoFeatureLayer.FeatureClass;
            //开始打算和创建
            IElement pElement = (annoFeature as IAnnotationFeature).Annotation;
            int FeatureID = (annoFeature as IAnnotationFeature2).LinkedFeatureID;
            int AnnotationClassID = (annoFeature as IAnnotationFeature2).AnnotationClassID;
            ITextElement pTextElement = pElement as ITextElement;
            IPolyline pl = pElement.Geometry as IPolyline;

            #region 调整河流注记方向
            AdjustRiverAnnoDirection(pl);
            #endregion

            IMultiPartTextElement mutiPartElement = (annoFeature as IAnnotationFeature2).Annotation as IMultiPartTextElement;
            if (mutiPartElement != null)
            {
                IAnnotationClassExtension2 annoExtension = annoFeature.Class.Extension as IAnnotationClassExtension2;
                if (!mutiPartElement.IsMultipart)
                    mutiPartElement.ConvertToMultiPart(annoExtension.get_Display((annoFeature as IAnnotationFeature2).Annotation));
                while (mutiPartElement.PartCount > 0)
                {
                    mutiPartElement.DeletePart(0);
                }
                (mutiPartElement as ITextElement).Text = "";
                annoFeature.Shape = new PolygonClass();
            }

            IPointCollection pPointCollection = pl as IPointCollection;
            string pAnnoText = pTextElement.Text;
            for (int i = 0; i < pAnnoText.Length; i++)
            {
                string ss = pAnnoText[i].ToString();
                IPoint p1 = pPointCollection.Point[i * 2];
                IPoint p2 = pPointCollection.Point[i * 2 + 1];
                IPoint positionPoint = new PointClass
                {
                    X = (p1.X + p2.X) / 2,
                    Y = (p1.Y + p2.Y) / 2
                };

                if (mutiPartElement != null)
                {
                    mutiPartElement.InsertPart(mutiPartElement.PartCount, ss, positionPoint);
                }
                else
                {
                    IPointCollection pNewPointCollection = new PolylineClass();
                    pNewPointCollection.AddPoint(positionPoint);
                    pNewPointCollection.AddPoint(p2);
                    IPolyline pNewPolyline = pNewPointCollection as IPolyline;
                    //double rotateAngle = GetAngle(pNewPolyline);
                    ITextSymbol pNewTextSymbol = (pTextElement.Symbol as IClone).Clone() as ITextSymbol;
                    pNewTextSymbol.Angle = northAngle;
                    ITextElement pNewTextElement = new TextElementClass();
                    pNewTextElement.ScaleText = pTextElement.ScaleText;
                    pNewTextElement.Symbol = pNewTextSymbol;
                    (pNewTextElement as IElement).Geometry = positionPoint;
                    ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pNewTextElement;
                    pSymbolCollEle.FlipAngle = 0;
                    pSymbolCollEle.Text = ss;
                    //朝北的话，不进行注记的旋转
                    //IElement pNewElement = pNewTextElement as IElement;
                    //ITransform2D pTransform2D = pNewElement as ITransform2D;
                    //pTransform2D.Rotate(positionPoint, rotateAngle);
                    //
                    IFeature feature = annoFeatureClass.CreateFeature();

                    #region 属性赋值
                    for (int k = 0; k < feature.Fields.FieldCount; k++)
                    {
                        IField pfield = feature.Fields.get_Field(k);
                        if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                        {
                            continue;
                        }

                        if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                        {
                            continue;
                        }

                        int index = annoFeature.Fields.FindField(pfield.Name);
                        if (index != -1 && pfield.Editable)
                        {
                            feature.set_Value(k, annoFeature.get_Value(index));
                        }

                    }
                    #endregion

                    IAnnotationFeature2 annoFeature2 = feature as IAnnotationFeature2;
                    annoFeature2.Annotation = pNewTextElement as IElement;
                    annoFeature2.AnnotationClassID = AnnotationClassID;
                    annoFeature2.LinkedFeatureID = FeatureID;
                    annoFeature2.Status = (annoFeature as IAnnotationFeature2).Status;
                    feature.Store();
                    //
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pNewTextSymbol);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pNewTextElement);
                }
            }
            //
            if (mutiPartElement != null)
            {
                (mutiPartElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                (mutiPartElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                (mutiPartElement as ISymbolCollectionElement).CharacterSpacing = 0;

                (mutiPartElement as ITextElement).Text = (mutiPartElement as ITextElement).Text.Trim();
                (annoFeature as IAnnotationFeature2).Annotation = mutiPartElement as IElement;
                annoFeature.Store();
            }
            else
            {
                annoFeature.Delete();
            }
            
        }

        /// <summary>
        /// 非规则性注记打断的方式2，针对点个数大于注记文本长度的两倍
        /// </summary>
        /// <param name="annoFeature"></param>
        /// <param name="northAngle"></param>
        private void NonRegularAnnoSplit2(IFeature annoFeature, double northAngle = 0)
        {
            //获取annoFeature的要素图层
            IFeatureLayer pAnnoFeatureLayer = null;
            var lyrs = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IFDOGraphicsLayer; })).ToArray();
            for (int i = 0; i < lyrs.Length; i++)
            {
                IFeatureLayer pFeatureLayer = lyrs[i] as IFeatureLayer;
                if (pFeatureLayer != null && pFeatureLayer.FeatureClass.AliasName == annoFeature.Class.AliasName)
                {
                    pAnnoFeatureLayer = pFeatureLayer;
                    break;
                }
            }
            if (pAnnoFeatureLayer == null) return;
            IFeatureClass annoFeatureClass = pAnnoFeatureLayer.FeatureClass;
            //开始打算和创建
            IElement pElement = (annoFeature as IAnnotationFeature).Annotation;
            int FeatureID = (annoFeature as IAnnotationFeature2).LinkedFeatureID;
            int AnnotationClassID = (annoFeature as IAnnotationFeature2).AnnotationClassID;
            ITextElement pTextElement = pElement as ITextElement;
            //根据注记的外边框进行打散处理，找到直角点
            #region
            IPolygon boderPolygon = annoFeature.Shape as IPolygon;
            IPointCollection borderPointCollection = boderPolygon as IPointCollection;
            List<int> VIndexL = new List<int>();
            for (int i = 0; i < borderPointCollection.PointCount - 1; i++)
            {
                IPoint p1, p2, p3;
                int pIndex = 0;
                if (i == borderPointCollection.PointCount - 2)
                {
                    //ps 多边形一共有5个点，第一个点和最后一个点为同一个点
                    p1 = borderPointCollection.get_Point(borderPointCollection.PointCount - 2);//倒数第二个点
                    p2 = borderPointCollection.get_Point(0);//倒数第一个点
                    p3 = borderPointCollection.get_Point(1);//第一个点
                    pIndex = 0;
                }
                else
                {
                    p1 = borderPointCollection.get_Point(i);
                    p2 = borderPointCollection.get_Point(i + 1);
                    p3 = borderPointCollection.get_Point(i + 2);
                    pIndex = i + 1;
                }
                //构建两个向量
                IVector3D vector_P1P2 = new Vector3DClass();
                vector_P1P2.XComponent = p2.X - p1.X;
                vector_P1P2.YComponent = p2.Y - p1.Y;
                IVector3D vector_P2P3 = new Vector3DClass();
                vector_P2P3.XComponent = p3.X - p2.X;
                vector_P2P3.YComponent = p3.Y - p2.Y;
                //向量积
                double vector_product = vector_P1P2.XComponent * vector_P2P3.XComponent + vector_P1P2.YComponent * vector_P2P3.YComponent;
                //在-1~1之间均判断为垂直关系
                if (vector_product < 1 && vector_product > -1)
                {
                    VIndexL.Add(pIndex);
                }
            }
            //注记中短边必定为相邻垂点，打散的中轴线采取外面的最长边
            //分别获取两条长边的polyline
            VIndexL.Sort();
            int pIndex1 = VIndexL[0];
            int pIndex2 = VIndexL[1];
            int pIndex3 = VIndexL[2];
            int pIndex4 = VIndexL[3];
            //第一条线 pIndex2~pIndex3;
            IPointCollection pc1 = new PolylineClass();
            for (int i = pIndex2; i <= pIndex3; i++)
            {
                IPoint p0 = borderPointCollection.get_Point(i);
                IPoint p1 = new PointClass { X = p0.X, Y = p0.Y };
                pc1.AddPoint(p1);
            }
            IPolyline pl1 = pc1 as IPolyline;
            //第二条线 pIndex1~pIndex4;
            IPointCollection pc2 = new PolylineClass();
            for (int i = pIndex4; i < borderPointCollection.PointCount - 2; i++)
            {
                IPoint p0 = borderPointCollection.get_Point(i);
                IPoint p1 = new PointClass { X = p0.X, Y = p0.Y };
                pc2.AddPoint(p1);
            }
            for (int i = 0; i <= pIndex1; i++)
            {
                IPoint p0 = borderPointCollection.get_Point(i);
                IPoint p1 = new PointClass { X = p0.X, Y = p0.Y };
                pc2.AddPoint(p1);
            }
            IPolyline pl2 = pc2 as IPolyline;
            IPolyline maxLengthPl = pl1.Length > pl2.Length ? pl1 : pl2;
            //计算中轴线点
            IPoint centerPoint = new PointClass
            {
                X = (borderPointCollection.get_Point(pIndex1).X + borderPointCollection.get_Point(pIndex2).X) / 2,
                Y = (borderPointCollection.get_Point(pIndex1).Y + borderPointCollection.get_Point(pIndex2).Y) / 2
            };
            IPoint PointOnline = new PointClass();
            double distanceAlongCurve = 0;
            double distanceFormCurve = 0;
            bool bRightSide = false;
            maxLengthPl.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, centerPoint, false, PointOnline, ref distanceAlongCurve, ref distanceFormCurve, ref bRightSide);
            IConstructCurve constructCurve = new PolylineClass();
            if (!bRightSide)
                distanceFormCurve = -distanceFormCurve;
            constructCurve.ConstructOffset(maxLengthPl, distanceFormCurve);
            #endregion
            IPolyline polyline = constructCurve as IPolyline;

            #region 调整河流注记方向
            AdjustRiverAnnoDirection(polyline);
            #endregion

            IMultiPartTextElement mutiPartElement = (annoFeature as IAnnotationFeature2).Annotation as IMultiPartTextElement;
            if (mutiPartElement != null)
            {
                IAnnotationClassExtension2 annoExtension = annoFeature.Class.Extension as IAnnotationClassExtension2;
                if (!mutiPartElement.IsMultipart)
                    mutiPartElement.ConvertToMultiPart(annoExtension.get_Display((annoFeature as IAnnotationFeature2).Annotation));
                while (mutiPartElement.PartCount > 0)
                {
                    mutiPartElement.DeletePart(0);
                }
                (mutiPartElement as ITextElement).Text = "";
                annoFeature.Shape = new PolygonClass();
            }

            //这种折线有自相交的问题，暂不考虑
            string text = pTextElement.Text;
            double textLength = pTextElement.Text.Length;
            double charArcLength = polyline.Length / textLength;
            double Splitlength = charArcLength;
            double Splitdistance = charArcLength / 2 + 0.0001;
            string s;//单个字符
            double startDistance = (polyline.Length - (charArcLength * textLength)) / 2;
            for (int i = 0; i < textLength; i++)
            {
                s = text[i].ToString();
                if (0 < Splitdistance && Splitdistance < polyline.Length)
                {
                    IPoint s_point = new ESRI.ArcGIS.Geometry.Point();
                    polyline.QueryPoint(esriSegmentExtension.esriNoExtension, startDistance + Splitdistance, false, s_point);
                    if (!s_point.IsEmpty)
                    {
                        if (mutiPartElement != null)//多部件
                        {
                            mutiPartElement.InsertPart(mutiPartElement.PartCount, s, s_point);
                        }
                        else
                        {
                            ITextSymbol pNewTextSymbol = (pTextElement.Symbol as IClone).Clone() as ITextSymbol;
                            pNewTextSymbol.Angle = northAngle;
                            ITextElement pNewTextElement = new TextElementClass();
                            pNewTextElement.ScaleText = pTextElement.ScaleText;
                            pNewTextElement.Symbol = pNewTextSymbol;
                            (pNewTextElement as IElement).Geometry = s_point;
                            //设置注记文本的内容
                            ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pNewTextElement;
                            pSymbolCollEle.Text = s;
                            IFeature feature = annoFeatureClass.CreateFeature();

                            #region 属性赋值
                            for (int k = 0; k < feature.Fields.FieldCount; k++)
                            {
                                IField pfield = feature.Fields.get_Field(k);
                                if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                                {
                                    continue;
                                }

                                if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                                {
                                    continue;
                                }

                                int index = annoFeature.Fields.FindField(pfield.Name);
                                if (index != -1 && pfield.Editable)
                                {
                                    feature.set_Value(k, annoFeature.get_Value(index));
                                }

                            }
                            #endregion

                            IAnnotationFeature2 annoFeature2 = feature as IAnnotationFeature2;
                            annoFeature2.Annotation = pNewTextElement as IElement;
                            annoFeature2.AnnotationClassID = AnnotationClassID;
                            annoFeature2.LinkedFeatureID = FeatureID;
                            annoFeature2.Status = (annoFeature as IAnnotationFeature2).Status;
                            feature.Store();
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pNewTextSymbol);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pNewTextElement);
                            //pElementCollection.Add(pNewTextElement as IElement);
                            Splitdistance += 0.001;
                        }
                        
                    }
                    Splitdistance = Splitdistance + Splitlength;
                }
            }
            if (mutiPartElement != null)
            {
                (mutiPartElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                (mutiPartElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                (mutiPartElement as ISymbolCollectionElement).CharacterSpacing = 0;

                (mutiPartElement as ITextElement).Text = (mutiPartElement as ITextElement).Text.Trim();
                (annoFeature as IAnnotationFeature2).Annotation = mutiPartElement as IElement;
                annoFeature.Store();
            }
            else
            {
                annoFeature.Delete();
            }
        }

        /// <summary>
        /// 规则矩形的注记要素打散
        /// </summary>
        /// <param name="_Feature">要素</param>
        /// <param name="pAnnoFeature">注记要素</param>
        /// <param name="pTextElement">文本元素</param>
        /// <param name="northAngle"></param>
        /// <returns>打散后的第一个要素</returns>
        private void RegularAnnoSplit(IFeature _Feature, IAnnotationFeature2 pAnnoFeature, ITextElement pTextElement, double northAngle = 0)
        {
            string[] lineTexts = pTextElement.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            double CharcterHeight = 0;
            double CharcterWidth = 0;
            //计算该要素pFeature多行注记的中心线，用于计算打散的分割
            IPolyline[] pLineGeometries = GetMiddleLineFromAnnoFea(pAnnoFeature, out CharcterWidth, out CharcterHeight);
            //获取annoFeature的要素图层
            IFeatureLayer pAnnoFeatureLayer = null;
            var lyrs = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IFDOGraphicsLayer; })).ToArray();
            for (int i = 0; i < lyrs.Length; i++)
            {
                IFeatureLayer pFeatureLayer = lyrs[i] as IFeatureLayer;
                if (pFeatureLayer != null && pFeatureLayer.FeatureClass.AliasName == _Feature.Class.AliasName)
                {
                    pAnnoFeatureLayer = pFeatureLayer;
                    break;
                }
            }
            if (pAnnoFeatureLayer == null) return;
            //创建注记要素集的图层
            IFeatureClass annoFeatureClass = pAnnoFeatureLayer.FeatureClass;
            int FeatureID = pAnnoFeature.LinkedFeatureID;
            int AnnotationClassID = pAnnoFeature.AnnotationClassID;

            IMultiPartTextElement mutiPartElement = pAnnoFeature.Annotation as IMultiPartTextElement;
            if (mutiPartElement != null)
            {
                IAnnotationClassExtension2 annoExtension = _Feature.Class.Extension as IAnnotationClassExtension2;
                if (!mutiPartElement.IsMultipart)
                    mutiPartElement.ConvertToMultiPart(annoExtension.get_Display(pAnnoFeature.Annotation));
                while(mutiPartElement.PartCount >0)
                {
                    mutiPartElement.DeletePart(0);
                }
                (mutiPartElement as ITextElement).Text = "";
                _Feature.Shape = new PolygonClass();
            }

            for (int i = 0; i < pLineGeometries.Length; i++)
            {
                IPolyline pl = pLineGeometries[i] as IPolyline;//注记的宽度

                #region 调整河流注记方向
                AdjustRiverAnnoDirection(pl);
                #endregion

                string lineText = lineTexts[i];
                //根据字符个数计算每个字符应分割的polyline长度
                double Splitlength = CharcterWidth;
                //计算分割后单个字符中心点间的长度
                double Splitdistance = Splitlength / 2 + 0.0001;
                string s;//单个字符
                if (lineText.Length == 1) continue;//单个字符不再做打散处理
                double startDistance = (pl.Length - (CharcterWidth * lineText.Length)) / 2;//居中显示时，距离边框的起始距离
                for (int j = 0; j < lineText.Length; j++)
                {
                    s = lineText[j].ToString();
                    if (0 < Splitdistance && Splitdistance < pl.Length)
                    {
                        IPoint s_Positon = new ESRI.ArcGIS.Geometry.Point();
                        pl.QueryPoint(esriSegmentExtension.esriNoExtension, startDistance + Splitdistance, false, s_Positon);
                        if (!s_Positon.IsEmpty)//【核心：创建打散要素】
                        {
                            if (mutiPartElement != null)//多部件
                            {
                                mutiPartElement.InsertPart(mutiPartElement.PartCount, s, s_Positon);
                            }
                            else//独立打散
                            {
                                ITextSymbol pNewTextSymbol = (pTextElement.Symbol as IClone).Clone() as ITextSymbol;
                                pNewTextSymbol.Angle = northAngle;
                                ITextElement pNewTextElement = new TextElementClass();
                                pNewTextElement.ScaleText = pTextElement.ScaleText;
                                pNewTextElement.Symbol = pNewTextSymbol;
                                (pNewTextElement as IElement).Geometry = s_Positon;
                                //设置注记文本的内容
                                ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pNewTextElement;
                                pSymbolCollEle.FlipAngle = 0;//不进行旋转
                                pSymbolCollEle.Text = s;
                                IFeature feature = annoFeatureClass.CreateFeature();

                                #region 属性赋值
                                for (int k = 0; k < feature.Fields.FieldCount; k++)
                                {
                                    IField pfield = feature.Fields.get_Field(k);
                                    if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                                    {
                                        continue;
                                    }

                                    if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                                    {
                                        continue;
                                    }

                                    int index = _Feature.Fields.FindField(pfield.Name);
                                    if (index != -1 && pfield.Editable)
                                    {
                                        feature.set_Value(k, _Feature.get_Value(index));
                                    }

                                }
                                #endregion

                                IAnnotationFeature2 annoFeature = feature as IAnnotationFeature2;
                                annoFeature.Annotation = pNewTextElement as IElement;
                                annoFeature.AnnotationClassID = AnnotationClassID;
                                annoFeature.LinkedFeatureID = FeatureID;
                                annoFeature.Status = pAnnoFeature.Status;
                                feature.Store();
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(feature);
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(pNewTextSymbol);
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(pNewTextElement);
                                //pElementCollection.Add(pNewTextElement as IElement);
                                Splitdistance += 0.001;
                            }
                        }
                        Splitdistance = Splitdistance + Splitlength;
                    }
                }
            }
            if (mutiPartElement != null)
            {
                (mutiPartElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                (mutiPartElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                (mutiPartElement as ISymbolCollectionElement).CharacterSpacing = 0;

                (mutiPartElement as ITextElement).Text = (mutiPartElement as ITextElement).Text.Trim();
                pAnnoFeature.Annotation = mutiPartElement as IElement;
                _Feature.Store();
            }
            else
            {
                _Feature.Delete();
            }
            pAnnoFeatureLayer = null;
            System.Runtime.InteropServices.Marshal.ReleaseComObject(_Feature);
        }

        /// <summary>
        /// 用于注记文本的打散,如果有换行文本，则获取每行文本的中心线，依次从上到下排列；
        /// 对于带气泡注记的打散 仅能保证打散，位置定位有问题，带气泡注记的点数比较多
        /// </summary>
        /// <param name="annoFeature">注记要素</param>
        /// <returns>中长线</returns>
        private IPolyline[] GetMiddleLineFromAnnoFea(IAnnotationFeature2 annoFeature, out double CharacterWidth, out double CharacterHeight)
        {
            IPointCollection pPointCollection = (annoFeature as IFeature).Shape as IPointCollection;
            IElement pTextElement = annoFeature.Annotation;
            string[] lineTexts = (pTextElement as ITextElement).Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            IPolyline[] linePolylines = new IPolyline[lineTexts.Length];//需要返回的每行文本的中心线
            IEnvelope rectEnvelop = (annoFeature as IFeature).Shape.Envelope;
            //注记中心点,用于确定多行中心线的位置，上偏还是下偏
            IPoint centerPoint = new PointClass() { X = (rectEnvelop.XMax + rectEnvelop.XMin) / 2, Y = (rectEnvelop.YMax + rectEnvelop.YMin) / 2 };
            int maxLineTextLength = 0;//最长字符数的长度
            for (int i = 0; i < lineTexts.Length; i++)
            {
                if (lineTexts[i].Length > maxLineTextLength)
                    maxLineTextLength = lineTexts[i].Length;
            }
            //判断矩形的最长边
            IPoint p1 = pPointCollection.get_Point(0);
            IPoint p2 = pPointCollection.get_Point(1);
            IPoint p3 = pPointCollection.get_Point(2);
            double aa = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            double bb = Math.Sqrt(Math.Pow(p2.X - p3.X, 2) + Math.Pow(p2.Y - p3.Y, 2));
            if (bb > aa)
            {
                p1 = p2; p2 = p3;
                CharacterHeight = aa / lineTexts.Length;//最短边除以文本的行数得到单个字符的高度
                CharacterWidth = bb / maxLineTextLength;//最长边除以最长文本的字符长度，得到单个字符的宽度
            }
            else
            {
                CharacterHeight = bb / lineTexts.Length;//最短边除以文本的行数得到单个字符的高度
                CharacterWidth = aa / maxLineTextLength;//最长边除以最长文本的字符长度，得到单个字符的宽度 
            }
            //根据矩形的最长边创建一个Polyline
            IPointCollection pPointColl = new PolylineClass();
            object missing = Type.Missing;

            if (p1.X > p2.X)
            {
                pPointColl.AddPoint(p2, ref  missing, ref missing);
                pPointColl.AddPoint(p1, ref  missing, ref missing);
            }
            else
            {
                pPointColl.AddPoint(p1, ref  missing, ref missing);
                pPointColl.AddPoint(p2, ref  missing, ref missing);
            }
            IPolyline pPolyline = pPointColl as IPolyline;
            //通过QueryPointAndDistance方法获取离elementPoint最近的pPolyline上的点
            IPoint outPoint = new ESRI.ArcGIS.Geometry.Point();
            double distanceAlongCurve = 0;
            double distanceFromCurve = 0;
            bool bRightSide = false;
            pPolyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, centerPoint, false, outPoint,
                ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
            double lineOffsetUnit = CharacterHeight / 2;//平移的单位长度
            for (int i = 0; i < lineTexts.Length; i++)
            {
                double lineOffset = i * CharacterHeight + lineOffsetUnit;
                if (!bRightSide)//偏移方向的判断，左侧还是右侧
                    lineOffset = -lineOffset;
                IConstructCurve pConstructCurve = new PolylineClass();
                pConstructCurve.ConstructOffset(pPolyline as IPolycurve, lineOffset);
                linePolylines[i] = pConstructCurve as IPolyline;
            }
            return linePolylines;
        }

        /// <summary>
        /// 调整注记排列方向
        /// </summary>
        /// <param name="pl"></param>
        private void AdjustRiverAnnoDirection(IPolyline pl)
        {
            double dx = pl.ToPoint.X - pl.FromPoint.X;
            double dy = pl.ToPoint.Y - pl.FromPoint.Y;
            //第一种情况：第一象限、第三象限
            if ((dx > 0 && dy > 0) || (dx < 0 && dy < 0))
            {
                #region
                double angle = Math.Atan(Math.Abs(dy) / Math.Abs(dx));
                if (angle < Math.PI / 4)//[从下往上]
                {
                    if (dy < 0)//从上往下流
                    {
                        pl.ReverseOrientation();
                    }
                }
                else//从上往下
                {
                    if (dy > 0)//从下往上流
                    {
                        pl.ReverseOrientation();
                    }
                }
                #endregion
            }
            //第二种情况：第二象限、第四象限
            if ((dx > 0 && dy < 0) || (dx < 0 && dy > 0))
            {
                //[从上往下]
                if (dy > 0)
                {
                    pl.ReverseOrientation();
                }
            }
            if (dx == 0)
            {
                //[从上往下]
                if (dy > 0)
                {
                    pl.ReverseOrientation();
                }
            }
            if (dy == 0)
            {
                //[从左往右]
                if (dx < 0)
                {
                    pl.ReverseOrientation();
                }
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Xml.Linq;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using System.Windows.Forms;

namespace SMGI.Plugin.EmergencyMap
{
    public class AddFigureMap
    {
        public enum FigureMapLocation
        {
            LeftUp,
            RightUp,
            LeftDown,
            RightDown
        }

        private GApplication _app;



        public AddFigureMap(GApplication app)
        {
            _app = app;
        }

        public string addFigureMap()
        {
            string msg = "";

            try
            {
                #region 读取附图
                //读取附图模板数据
                string mxdFullFileName = EnvironmentSettings.getLocationMxdFullFileName(_app);
                IMapDocument pMapDoc = new MapDocumentClass();

                pMapDoc.Open(mxdFullFileName, "");
                if (pMapDoc.MapCount == 0)//如果地图模板为空      
                {
                    msg = "位置模板为空,添加附图失败！";
                    return msg;
                }
                IMap templateMap = pMapDoc.get_Map(0);
                IMapFrame tempMapFrame = (pMapDoc.PageLayout as IGraphicsContainer).FindFrame(templateMap) as IMapFrame;
                IElement tempMapElement = (tempMapFrame as IFrameElement) as IElement;
                IEnvelope templateFullExtent = tempMapElement.Geometry.Envelope;

                //新增地图(附图)
                Guid mapID = _app.Workspace.AddMap();
                var lyrMgr = _app.Workspace.GetMapByGuid(mapID);
                IMap figureMap = lyrMgr.Map;
                _app.PageLayoutControl.ActiveView.Deactivate();
                (figureMap as IActiveView).Activate(_app.PageLayoutControl.hWnd);//激活附图

                //根据地图模板设置附图参数
                figureMap.SpatialReference = templateMap.SpatialReference;
                figureMap.ReferenceScale = templateMap.ReferenceScale;
                figureMap.UseSymbolLevels = templateMap.UseSymbolLevels;
                figureMap.MapUnits = templateMap.MapUnits;
                figureMap.MapScale = templateMap.MapScale;
                figureMap.AnnotationEngine = templateMap.AnnotationEngine;

                //导入元素
                var tempGC = (templateMap as IActiveView).GraphicsContainer;
                tempGC.Reset();
                IElement tempElev;
                while ((tempElev = tempGC.Next()) != null)
                {
                    if (tempElev is IMapFrame)
                    {
                        continue;
                    }
                    (figureMap as IActiveView).GraphicsContainer.AddElement(tempElev, 1);
                }
                Marshal.ReleaseComObject(tempGC);

                figureMap.ClearLayers();
                List<ILayer> layers = new List<ILayer>();
                for (int i = templateMap.LayerCount - 1; i >= 0; i--)
                {
                    var l = templateMap.get_Layer(i);
                    layers.Add(l);
                }
                templateMap.ClearLayers();

                IWorkspace ws = _app.Workspace.EsriWorkspace;
                foreach (var item in layers)
                {
                    MatchLayer(ws, figureMap, item, null);
                }
                #endregion

                #region 设置附图放置位置

                msg = setFigureMapLocate();//设置附图放置位置

                #endregion

                #region 更新视图
                IGraphicsContainerSelect gcs = _app.PageLayoutControl.PageLayout as IGraphicsContainerSelect;
                IMapFrame mapFrame = _app.PageLayoutControl.GraphicsContainer.FindFrame(figureMap) as IMapFrame;
                IElement mapElement = (mapFrame as IFrameElement) as IElement;
                if (mapFrame.Background != null)//背景设置
                {
                    (mapFrame.Background as IFrameDecoration).Color = new ESRI.ArcGIS.Display.CmykColorClass { Cyan = 0, Magenta = 0, Yellow = 0, Black = 12 };
                }
                else
                {
                    mapFrame.Background = new SymbolBackgroundClass { Color = new ESRI.ArcGIS.Display.CmykColorClass { Cyan = 0, Magenta = 0, Yellow = 0, Black = 12 } };
                }

                if (templateFullExtent != null)
                {
                    double mapXMin = mapElement.Geometry.Envelope.XMin;
                    double mapYMax = mapElement.Geometry.Envelope.YMax;
                    IPoint pt = new PointClass() as IPoint;
                    pt.PutCoords(mapXMin, mapYMax);
                    ITransform2D pTransform2D = mapElement as ITransform2D;
                    pTransform2D.Scale(pt, templateFullExtent.Width / mapElement.Geometry.Envelope.Width, templateFullExtent.Height / mapElement.Geometry.Envelope.Height);
                }
                (figureMap as IActiveView).FullExtent.CenterAt(new PointClass { X = templateFullExtent.XMin + templateFullExtent.Width, Y = templateFullExtent.YMin + templateFullExtent.Height });

                _app.TOCControl.Refresh();
                _app.PageLayoutControl.ActiveView.Activate(_app.PageLayoutControl.hWnd);
                _app.PageLayoutControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                //激活主图
                //for (int i = 0; i < _app.Workspace.MapManagers.Count; ++i )
                //{
                //    if (_app.Workspace.MapManagers[i].Map.Name.Contains("主图"))
                //    {
                //        _app.PageLayoutControl.ActiveView.FocusMap = _app.Workspace.MapManagers[i].Map;
                //        _app.PageLayoutControl.Refresh();
                //        _app.TOCControl.Refresh();

                //        break;
                //    }

                //}

                #endregion

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

        //设置附图放置位置
        private string setFigureMapLocate(FigureMapLocation location = FigureMapLocation.LeftUp)
        {
            string msg = "";

            IMapFrame mapFrame = _app.PageLayoutControl.GraphicsContainer.FindFrame(_app.Workspace.Map) as IMapFrame;
            IElement ele = (mapFrame as IFrameElement) as IElement;//附图的框架元素
            if (!(ele is IMapFrame) || _app.Workspace.Map.Name.Contains("主图") || _app.Workspace.Map == _app.Workspace.MapManagers[0])
            {
                msg = "请选择一个附图！";
                return msg;
            }
            ITransform2D transform2D = ele as ITransform2D;

            IMapFrame mainMapFrame = _app.PageLayoutControl.GraphicsContainer.FindFrame(_app.Workspace.MapManagers[0].Map) as IMapFrame;
            IElement mainMapElement = (mainMapFrame as IFrameElement) as IElement;//主图的框架元素
            switch (location)
            {
                case FigureMapLocation.LeftUp:
                    {
                        transform2D.Move(mainMapElement.Geometry.Envelope.XMin - ele.Geometry.Envelope.XMin, mainMapElement.Geometry.Envelope.YMax - ele.Geometry.Envelope.YMax);

                        break;
                    }
                case FigureMapLocation.RightUp:
                    {
                        transform2D.Move(mainMapElement.Geometry.Envelope.XMax - ele.Geometry.Envelope.XMax, mainMapElement.Geometry.Envelope.YMax - ele.Geometry.Envelope.YMax);

                        break;
                    }
                case FigureMapLocation.LeftDown:
                    {
                        transform2D.Move(mainMapElement.Geometry.Envelope.XMin - ele.Geometry.Envelope.XMin, mainMapElement.Geometry.Envelope.YMin - ele.Geometry.Envelope.YMin);

                        break;
                    }
                case FigureMapLocation.RightDown:
                    {
                        transform2D.Move(mainMapElement.Geometry.Envelope.XMax - ele.Geometry.Envelope.XMax, mainMapElement.Geometry.Envelope.YMin - ele.Geometry.Envelope.YMin);

                        break;
                    }

            }

            _app.PageLayoutControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

            return msg;
        }

        //模板匹配
        private void MatchLayer(IWorkspace ws, IMap map, ILayer layer, IGroupLayer parent)
        {
            if (parent == null)
            {
                map.AddLayer(layer);
            }
            else
            {
                (parent as IGroupLayer).Add(layer);
            }

            if (layer is IGroupLayer)
            {
                var l = (layer as ICompositeLayer);

                List<ILayer> layers = new List<ILayer>();
                for (int i = 0; i < l.Count; i++)
                {
                    layers.Add(l.get_Layer(i));
                }
                (layer as IGroupLayer).Clear();
                foreach (var item in layers)
                {
                    MatchLayer(ws, map, item, layer as IGroupLayer);
                }
            }
            else
            {
                string name = ((layer as IDataLayer2).DataSourceName as IDatasetName).Name;
                if (layer is IFeatureLayer)
                {
                    //然后匹配
                    if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name))
                    {
                        IFeatureClass fc = (ws as IFeatureWorkspace).OpenFeatureClass(name);
                        (layer as IFeatureLayer).FeatureClass = fc;
                        IFeatureClassManage fcMgr = fc as IFeatureClassManage;
                        fcMgr.UpdateExtent();
                        if (fc.AliasName.StartsWith("BOUA"))
                        {
                            (map as IActiveView).Extent = (fc as IGeoDataset).Extent;
                        }
                    }
                }
                else if (layer is IRasterLayer)
                {
                    if ((ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTRasterDataset, name))
                    {
                        (layer as IRasterLayer).CreateFromRaster((ws as IRasterWorkspaceEx).OpenRasterDataset(name).CreateDefaultRaster());
                    }
                }
            }
        }


        //返回附图位置点
        //whRat 附图宽/高比
        private IPoint GetFigureMapPoint(IEnvelope pEnvelope, string pos, double size, double whRat)
        {
            IPoint AnchorPoint = new PointClass();

            size = size / 2.0;
            double xmin = pEnvelope.XMin;
            double xmax = pEnvelope.XMax;
            double ymin = pEnvelope.YMin;
            double ymax = pEnvelope.YMax;
            double x = 0, y = 0;
            double dx = 0, dy = 0;
            if (whRat > 1)//调整y
            {
                dy = size - size / whRat;
            }
            else if (whRat < 1)//调整x
            {
                dx = size - size * whRat;
            }
            switch (pos)
            {
                case "TopLeft":
                    x = xmin + size - dx;
                    y = ymax - size + dy;
                    AnchorPoint.PutCoords(x, y);
                    break;
                case "DownLeft":
                    x = xmin + size - dx;
                    y = ymin + size - dy;
                    AnchorPoint.PutCoords(x, y);
                    break;
                case "TopRight":
                    x = xmax - size + dx;
                    y = ymax - size + dy;
                    AnchorPoint.PutCoords(x, y);
                    break;
                case "DownRight":
                    x = xmax - size + dx;
                    y = ymin + size - dy;
                    AnchorPoint.PutCoords(x, y);
                    break;
            }

            return AnchorPoint;
        }
        public string addFigureMap2(double makersize, IEnvelope feEnv, string pos)//, IPoint anchorPoint
        {
            string msg = "";

            try
            {

                string fclname = "BOUA_Bg";
                IFeatureClass fc = null;
                IWorkspace ws = null;
                InitRepPramas(out fc, out ws, fclname);
                if (fc == null)
                {
                    msg = "BOUA_Bg图层不存在";
                    return msg;
                }
                //求中心点，缩放系数
                IEnvelope penv = (fc as IGeoDataset).Extent; 
                IPoint anchorPoint = GetFigureMapPoint(feEnv, pos, makersize * 1e-3 * _app.ActiveView.FocusMap.ReferenceScale, penv.Width / penv.Height);
                
                //附图点自由图层
                ILayer repLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOINT"))).FirstOrDefault();
                IFeatureClass pointfcl = (repLayer as IFeatureLayer).FeatureClass;
                var rp = (repLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                IRepresentationClass m_RepClass = rp.RepresentationClass;

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "TYPE like '%附图%'";
                ClearFeatures(pointfcl, qf);
                IFeature fenew = pointfcl.CreateFeature();
                fenew.Shape = anchorPoint;
                fenew.set_Value(pointfcl.FindField("TYPE"), "附图");
                fenew.Store();
                //自由
                IMapContext mc = new MapContextClass();
                mc.InitFromDisplay(_app.ActiveView.ScreenDisplay.DisplayTransformation);
                IRepresentation rep = m_RepClass.GetRepresentation(fenew, mc);
                IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
                //
                //创建rule 
                IRepresentationGraphics mapGraphic = new RepresentationMarkerClass();
                IRepresentationRule figureMapRule = new RepresentationRuleClass();
                var bs = new BasicMarkerSymbolClass();
                //依次添加模板每个图层的。附图要素规则
                #region 添加BOUA

             
                double scale = 36.6; //scale=(附图原图单位长度fUint/当前单位长度mUint)/(附图原图参考比例尺frScale/当前参考比例尺mrScale)/(附图原图宽高最大值fmaxLen/附图现在宽高最大值mmaxLen)
                //scale = 500.0 / Math.Max(penv.Height, penv.Width);//缩小
                //double frScale=16000000;
                //double mrScale=GApplication.Application.ActiveView.FocusMap.ReferenceScale;
              
                //double fmaxLen = Math.Max(penv.Height, penv.Width);
                //double mmaxLen = makersize * 1e-3 * _app.ActiveView.FocusMap.ReferenceScale;
                //scale = 100000 / (16000000 / GApplication.Application.ActiveView.FocusMap.ReferenceScale);
                //scale = (frScale * fmaxLen) / (mrScale * mmaxLen);
                //scale = 100000 / scale;
                //            fUint/mUint
                //        ---------------------
                //scale =  frScale     fmaxLen
                //        --------- * ---------
                //         mrScale     mmaxLen



                IPoint ctPoint = new PointClass() { X = (penv.XMax + penv.XMin) / 2, Y = (penv.YMax + penv.YMin) / 2 };
                var ruleDic = ObtainFigureMapRepRule(fc, ws);
                IFeature fe;
                IFeatureCursor cursor = fc.Search(null, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    int ruleid = int.Parse(fe.get_Value(fc.FindField("ruleID")).ToString());
                    var rule = ruleDic[ruleid];
                    AddFeatureRule(mapGraphic, fe.Shape, rule, ctPoint, scale);
                }
                Marshal.ReleaseComObject(cursor);
                #endregion
                #region
                string mxd = GApplication.Application.Template.Root + "\\位置图\\位置图.mxd";
                IMapDocument pMapDoc = new MapDocumentClass();
                pMapDoc.Open(mxd, "");
                var tempMap = pMapDoc.get_Map(0);
                if (pMapDoc.MapCount == 0)//如果地图模板为空
                {
                    MessageBox.Show("位置地图模板不能为空！");
                    return "";
                }
                for (int i = tempMap.LayerCount - 1; i >= 0; i--)
                {
                    var lyr = tempMap.get_Layer(i);
                    fc = (lyr as IFeatureLayer).FeatureClass;
                    if (fc == null)
                        continue;
                    fclname = fc.AliasName;
                    if (lyr is IFDOGraphicsLayer)//注记
                    {
                        #region
                        ruleDic = ObtainFigureMapRepRule(fc, ws);
                        cursor = fc.Search(null, false);
                        while ((fe = cursor.NextFeature()) != null)
                        {
                            IAnnotationFeature pAnnoFeature = fe as IAnnotationFeature;
                            IElement el = pAnnoFeature.Annotation as IElement;
                            var te = el as ITextElement;
                            var text = te.Text;
                            var symbol = te.Symbol;
                            var xx = symbol as ISimpleTextSymbol;
                            IGeometry geoClone = (el.Geometry as IClone).Clone() as IGeometry;
                            IPoint orgin = new PointClass() { X = 0, Y = 0 };

                            double dx = -ctPoint.X;
                            double dy = -ctPoint.Y;
                            ITransform2D trans = geoClone as ITransform2D;
                            trans.Move(dx, dy);
                            trans.Scale(orgin, scale, scale);
                            IPoint txtpoint = null;
                            if (geoClone.GeometryType == esriGeometryType.esriGeometryPolyline)
                            {
                                IPolyline pline = geoClone as IPolyline;
                                txtpoint = new PointClass();
                                pline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtpoint);
                            }
                            else
                            {
                                txtpoint = geoClone as IPoint;
                            }
                            AddTxtElement(txtpoint, text, te.Symbol, mapGraphic);

                        }
                        Marshal.ReleaseComObject(cursor);
                        #endregion
                    }
                    else //常规要素类
                    {
                        #region
                        ruleDic = ObtainFigureMapRepRule(fc, ws);
                        cursor = fc.Search(null, false);
                        while ((fe = cursor.NextFeature()) != null)
                        {
                            int ruleid = int.Parse(fe.get_Value(fc.FindField("ruleID")).ToString());
                            var rule = ruleDic[ruleid];
                            AddFeatureRule(mapGraphic, fe.Shape, rule, ctPoint, scale);
                        }
                        Marshal.ReleaseComObject(cursor);
                        #endregion
                    }
                }

                #endregion


                #region 添加其他
                //string[] names = new string[] { "BOUA_AAA", "BOUL_AAA", "LRDL_H", "LRDL_G", "LRRL_AAA", "AGNP_AAA", "LENG_AAA" };
                //foreach (var name in names)
                //{
                //    fclname = name;
                //    fc = null;
                //    ws = null;
                //    InitRepPramas(out fc, out ws, fclname);
                //    if (fc == null)
                //        continue;
                //    //求中心点，缩放系数
                //    ruleDic = ObtainFigureMapRepRule(fc, ws);
                //    cursor = fc.Search(null, false);
                //    while ((fe = cursor.NextFeature()) != null)
                //    {
                //        int ruleid = int.Parse(fe.get_Value(fc.FindField("ruleID")).ToString());
                //        var rule = ruleDic[ruleid];
                //        AddFeatureRule(mapGraphic, fe.Shape, rule, ctPoint, scale);
                //    }
                //    Marshal.ReleaseComObject(cursor);
                //}
                #endregion

                #region 添加注记
                //fclname = "AGNP_AAA注记";
                //fc = null;
                //ws = null;
                //InitRepPramas(out fc, out ws, fclname);
                ////求中心点，缩放系数
                //ruleDic = ObtainFigureMapRepRule(fc, ws);
                //cursor = fc.Search(null, false);
                //while ((fe = cursor.NextFeature()) != null)
                //{
                //    IAnnotationFeature pAnnoFeature = fe as IAnnotationFeature;
                //    IElement el = pAnnoFeature.Annotation as IElement;
                //    var te = el as ITextElement;
                //    var text = te.Text;
                //    var symbol = te.Symbol;
                //    var xx = symbol as ISimpleTextSymbol;
                //    IGeometry geoClone = (el.Geometry as IClone).Clone() as IGeometry;
                //    IPoint orgin = new PointClass() { X = 0, Y = 0 };

                //    double dx = -ctPoint.X;
                //    double dy = -ctPoint.Y;
                //    ITransform2D trans = geoClone as ITransform2D;
                //    trans.Move(dx, dy);
                //    trans.Scale(orgin, scale, scale);
                //    IPoint txtpoint = null;
                //    if (geoClone.GeometryType == esriGeometryType.esriGeometryPolyline)
                //    {
                //        IPolyline pline = geoClone as IPolyline;
                //        txtpoint = new PointClass();
                //        pline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtpoint);
                //    }
                //    else
                //    {
                //        txtpoint = geoClone as IPoint;
                //    }
                //    AddTxtElement(txtpoint, text, te.Symbol, mapGraphic);

                //}
                //Marshal.ReleaseComObject(cursor);
                #endregion
                //添加当前的附图几何
                AddCurrendExtent(mapGraphic, penv.SpatialReference, ctPoint, scale);
                //end

                bs.set_Value(1, mapGraphic); //marker
                //对尺寸进行修正
                bs.set_Value(2, makersize * 2.83465); //size
                bs.set_Value(3, 0);//angle
                bs.set_Value(4, false);
                figureMapRule.InsertLayer(0, bs);
                //
                repGraphics.Add(anchorPoint, figureMapRule);
                rep.Graphics = repGraphics;
                rep.UpdateFeature();
                rep.Feature.Store();
                _app.ActiveView.Refresh();
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

        IPoint orginPoint = null;
        public string addFigureMapXJ(double makersize, IPoint anchorPoint, string pos, IEnvelope clipEnv)
        {
            string msg = "";

            try
            {
                //附图点自由图层
                ILayer repLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOINT"))).FirstOrDefault();
                IFeatureClass pointfcl = (repLayer as IFeatureLayer).FeatureClass;
                var rp = (repLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                IRepresentationClass m_RepClass = rp.RepresentationClass;

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "TYPE like '%附图%'";
                ClearFeatures(pointfcl, qf);
                IFeature fenew = pointfcl.CreateFeature();
                fenew.Shape = anchorPoint;
                fenew.set_Value(pointfcl.FindField("TYPE"), "附图");
                fenew.Store();
                Marshal.ReleaseComObject(qf);
                //自由
                IMapContext mc = new MapContextClass();
                mc.InitFromDisplay(_app.ActiveView.ScreenDisplay.DisplayTransformation);
                IRepresentation rep = m_RepClass.GetRepresentation(fenew, mc);
                IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
                //
                //创建rule 
                IRepresentationGraphics mapGraphic = new RepresentationMarkerClass();
                IRepresentationRule figureMapRule = new RepresentationRuleClass();
                var bs = new BasicMarkerSymbolClass();
                //依次添加模板每个图层的。附图要素规则
                #region 添加BOUA
                string fclname = "BOUA_Bg";
                IFeatureClass fc = null;
                IWorkspace ws = null;
                InitRepPramas(out fc, out ws, fclname);
                if (fc == null)
                {
                    msg = "BOUA_Bg图层不存在";
                    return msg;
                }
                IFeatureCursor fcursor = fc.Search(null, false);
                IFeature felline = fcursor.NextFeature();
                IEnvelope penv = felline.ShapeCopy.Envelope;
                Marshal.ReleaseComObject(fcursor);
                //求中心点，缩放系数

                // scale = Math.Max(penv.Height, penv.Width) / (makersize*1e-3*mapScale);
                double scale = 500.0 / Math.Max(penv.Height, penv.Width);//缩小
                IPoint ctPoint = new PointClass() { X = (penv.XMax + penv.XMin) / 2, Y = (penv.YMax + penv.YMin) / 2 };
                var ruleDic = ObtainFigureMapRepRule(fc, ws);
                IFeature fe;
                IFeatureCursor cursor = fc.Search(null, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    int ruleid = int.Parse(fe.get_Value(fc.FindField("ruleID")).ToString());
                    var rule = ruleDic[ruleid];
                    AddFeatureRule(mapGraphic, fe.Shape, rule, ctPoint, scale);
                }
                Marshal.ReleaseComObject(cursor);
                #endregion


                string mxd = GApplication.Application.Template.Root + "\\位置图\\位置图.mxd";
                IMapDocument pMapDoc = new MapDocumentClass();
                pMapDoc.Open(mxd, "");
                var tempMap = pMapDoc.get_Map(0);
                if (pMapDoc.MapCount == 0)//如果地图模板为空
                {
                    MessageBox.Show("位置地图模板不能为空！");
                    return "";
                }
                for (int i = tempMap.LayerCount-1; i >=0; i--)
                {
                    var lyr=tempMap.get_Layer(i);
                    fc = (lyr as IFeatureLayer).FeatureClass;
                    if (fc == null)
                        continue;
                    fclname = fc.AliasName;
                    if (lyr is IFDOGraphicsLayer)//注记
                    {
                        #region
                        ruleDic = ObtainFigureMapRepRule(fc, ws);
                        cursor = fc.Search(null, false);
                        while ((fe = cursor.NextFeature()) != null)
                        {
                            IAnnotationFeature pAnnoFeature = fe as IAnnotationFeature;
                            IElement el =(pAnnoFeature.Annotation as IClone).Clone() as IElement;
                          
                            var te = el as ITextElement;
                            var text = te.Text;
                            var symbol = te.Symbol;
                           
                            IGeometry geoTextEn =fe.ShapeCopy as IGeometry;
                            IGeometry geoClone = el.Geometry as IGeometry;
                            IPoint orgin = new PointClass() { X = 0, Y = 0 };
                             
                            double dx = -ctPoint.X;
                            double dy = -ctPoint.Y;
                            ITransform2D trans = geoTextEn as ITransform2D;
                            trans.Move(dx, dy);
                            trans.Scale(orgin, scale, scale);
                            double height = geoTextEn.Envelope.Height;
                            IPoint txtpoint = null;
                            if (geoClone.GeometryType == esriGeometryType.esriGeometryPolyline)
                            {
                                IPolyline pline = geoClone as IPolyline;
                                txtpoint = new PointClass();
                                pline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtpoint);
                            }
                            else
                            {
                                txtpoint = geoClone as IPoint;
                            }
                            (txtpoint as ITransform2D).Move(dx, dy);
                            (txtpoint as ITransform2D).Scale(orgin, scale, scale);
                            AddTxtElement1(height,txtpoint, text, te.Symbol, mapGraphic);

                        }
                        Marshal.ReleaseComObject(cursor);
                        #endregion
                    }
                    else //常规要素类
                    {
                        #region
                        ruleDic = ObtainFigureMapRepRule(fc, ws);
                        cursor = fc.Search(null, false);
                        while ((fe = cursor.NextFeature()) != null)
                        {
                            int ruleid = int.Parse(fe.get_Value(fc.FindField("ruleID")).ToString());
                            var rule = ruleDic[ruleid];
                            AddFeatureRule(mapGraphic, fe.Shape, rule, ctPoint, scale);
                        }
                        Marshal.ReleaseComObject(cursor);
                        #endregion
                    }
                }
               
                //添加当前的附图几何
                AddCurrendExtent(mapGraphic, penv.SpatialReference, ctPoint, scale);
                //end

                bs.set_Value(1, mapGraphic); //marker
                //对尺寸进行修正
                bs.set_Value(2, makersize * 2.83465); //size
                bs.set_Value(3, 0);//angle
                bs.set_Value(4, false);
                figureMapRule.InsertLayer(0, bs);
                //
                double _size = Math.Max((mapGraphic as IRepresentationMarker).Width, (mapGraphic as IRepresentationMarker).Height);
                double dx_ = (mapGraphic as IRepresentationMarker).Width / _size * makersize;
                double dy_ = (mapGraphic as IRepresentationMarker).Height / _size * makersize;

                IPoint newpoint = CommonMethods.GetFigureMapPointURe(clipEnv, pos, dx_, dy_);

                repGraphics.Add(newpoint, figureMapRule);
                rep.Graphics = repGraphics;
                rep.UpdateFeature();
                rep.Feature.Store();
                fenew.Shape = newpoint;
                fenew.Store();
                _app.ActiveView.Refresh();
                GC.Collect();
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

        public string addFigureMapXJAuto(double makersize, IPoint anchorPoint, string pos,IEnvelope clipEnv , IGeometry clipGeo,IEnvelope LengendEn)
        {
            string msg = "";
           
            try
            {
                //附图点自由图层
                ILayer repLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOINT"))).FirstOrDefault();
                IFeatureClass pointfcl = (repLayer as IFeatureLayer).FeatureClass;
                var rp = (repLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                IRepresentationClass m_RepClass = rp.RepresentationClass;

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "TYPE like '%附图%'";
                ClearFeatures(pointfcl, qf);
                IFeature fenew = pointfcl.CreateFeature();
                fenew.Shape = anchorPoint;
                fenew.set_Value(pointfcl.FindField("TYPE"), "附图");
                fenew.Store();
                Marshal.ReleaseComObject(qf);
                //自由
                IMapContext mc = new MapContextClass();
                mc.InitFromDisplay(_app.ActiveView.ScreenDisplay.DisplayTransformation);
                IRepresentation rep = m_RepClass.GetRepresentation(fenew, mc);
                IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
                //
                //创建rule 
                IRepresentationGraphics mapGraphic = new RepresentationMarkerClass();
                IRepresentationRule figureMapRule = new RepresentationRuleClass();
                var bs = new BasicMarkerSymbolClass();
                //依次添加模板每个图层的。附图要素规则
                #region 添加BOUA
                string fclname = "BOUA_Bg";
                IFeatureClass fc = null;
                IWorkspace ws = null;
                InitRepPramas(out fc, out ws, fclname);
                if (fc == null)
                {
                    msg = "BOUA_Bg图层不存在";
                    return msg;
                }
                IFeatureCursor fcursor = fc.Search(null, false);
                IFeature felline = fcursor.NextFeature();
                IEnvelope penv = felline.ShapeCopy.Envelope;
                Marshal.ReleaseComObject(fcursor);
                //求中心点，缩放系数

                // scale = Math.Max(penv.Height, penv.Width) / (makersize*1e-3*mapScale);
                double scale = 500.0 / Math.Max(penv.Height, penv.Width);//缩小
                IPoint ctPoint = new PointClass() { X = (penv.XMax + penv.XMin) / 2, Y = (penv.YMax + penv.YMin) / 2 };
                var ruleDic = ObtainFigureMapRepRule(fc, ws);
                IFeature fe;
                IFeatureCursor cursor = fc.Search(null, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    int ruleid = int.Parse(fe.get_Value(fc.FindField("ruleID")).ToString());
                    var rule = ruleDic[ruleid];
                    AddFeatureRule(mapGraphic, fe.Shape, rule, ctPoint, scale);
                }
                Marshal.ReleaseComObject(cursor);
                #endregion


                string mxd = GApplication.Application.Template.Root + "\\位置图\\位置图.mxd";
                IMapDocument pMapDoc = new MapDocumentClass();
                pMapDoc.Open(mxd, "");
                var tempMap = pMapDoc.get_Map(0);
                if (pMapDoc.MapCount == 0)//如果地图模板为空
                {
                    MessageBox.Show("位置地图模板不能为空！");
                    return "";
                }
                for (int i = tempMap.LayerCount - 1; i >= 0; i--)
                {
                    var lyr = tempMap.get_Layer(i);
                    fc = (lyr as IFeatureLayer).FeatureClass;
                    if (fc == null)
                        continue;
                    fclname = fc.AliasName;
                    if (lyr is IFDOGraphicsLayer)//注记
                    {
                        #region
                        ruleDic = ObtainFigureMapRepRule(fc, ws);
                        cursor = fc.Search(null, false);
                        while ((fe = cursor.NextFeature()) != null)
                        {
                            IAnnotationFeature pAnnoFeature = fe as IAnnotationFeature;
                            IElement el = pAnnoFeature.Annotation as IElement;
                            var te = el as ITextElement;
                            var text = te.Text;
                            var symbol = te.Symbol;
                            var xx = symbol as ISimpleTextSymbol;
                            IGeometry geoClone = (el.Geometry as IClone).Clone() as IGeometry;
                            IPoint orgin = new PointClass() { X = 0, Y = 0 };

                            double dx = -ctPoint.X;
                            double dy = -ctPoint.Y;
                            ITransform2D trans = geoClone as ITransform2D;
                            trans.Move(dx, dy);
                            trans.Scale(orgin, scale, scale);
                            IPoint txtpoint = null;
                            if (geoClone.GeometryType == esriGeometryType.esriGeometryPolyline)
                            {
                                IPolyline pline = geoClone as IPolyline;
                                txtpoint = new PointClass();
                                pline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, txtpoint);
                            }
                            else
                            {
                                txtpoint = geoClone as IPoint;
                            }
                            AddTxtElement1(fe.ShapeCopy.Envelope.Height,txtpoint, text, te.Symbol, mapGraphic);

                        }
                        Marshal.ReleaseComObject(cursor);
                        #endregion
                    }
                    else //常规要素类
                    {
                        #region
                        ruleDic = ObtainFigureMapRepRule(fc, ws);
                        cursor = fc.Search(null, false);
                        while ((fe = cursor.NextFeature()) != null)
                        {
                            int ruleid = int.Parse(fe.get_Value(fc.FindField("ruleID")).ToString());
                            var rule = ruleDic[ruleid];
                            AddFeatureRule(mapGraphic, fe.Shape, rule, ctPoint, scale);
                        }
                        Marshal.ReleaseComObject(cursor);
                        #endregion
                    }
                }
                #region 添加其他
               
                #endregion

                #region 添加注记
               
                #endregion
                //添加当前的附图几何
                AddCurrendExtent(mapGraphic, penv.SpatialReference, ctPoint, scale);
                //end

                bs.set_Value(1, mapGraphic); //marker
                //对尺寸进行修正
                bs.set_Value(2, makersize * 2.83465); //size
                bs.set_Value(3, 0);//angle
                bs.set_Value(4, false);
                figureMapRule.InsertLayer(0, bs);
                //
                double _size = Math.Max((mapGraphic as IRepresentationMarker).Width, (mapGraphic as IRepresentationMarker).Height);
                double dx_ = (mapGraphic as IRepresentationMarker).Width / _size * makersize;
                double dy_ = (mapGraphic as IRepresentationMarker).Height / _size * makersize;

                IPoint newpoint = CommonMethods.GetfigureMapPointAuto(clipEnv,LengendEn,clipGeo, dx_, dy_);

                repGraphics.Add(newpoint, figureMapRule);
                rep.Graphics = repGraphics;
                rep.UpdateFeature();
                rep.Feature.Store();
                fenew.Shape = newpoint;
                fenew.Store();
                _app.ActiveView.Refresh();
                GC.Collect();
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

        //改进版本：将文字转为几何存放到注记的位置
        private static void AddTxtElement1(double textHeight, IPoint txtpoint, string txt, ITextSymbol symbol, IRepresentationGraphics g_)
        {
            symbol.Size = 500;
            //获取文字高度
            IGeometry geoMapText = SMGI.Plugin.EmergencyMap.MapDeOut.AnnoGeometryHelper.AnnoGeometry(txt,txtpoint,symbol);
            double sx = textHeight /geoMapText.Envelope.Height;

            (geoMapText as ITransform2D).Scale(txtpoint, sx, sx);
           
            RepresentationRuleClass rule = new RepresentationRuleClass();
            IBasicFillSymbol flyr = new BasicFillSymbolClass();
            IFillPattern pFillPattern = new SolidColorPattern();
            var fillAttrs = pFillPattern as IGraphicAttributes;
            fillAttrs.set_Value(0, symbol.Color); //Define color.           
            flyr.FillPattern = pFillPattern;
            rule.InsertLayer(0, flyr as IBasicSymbol);
            g_.Add(geoMapText, rule);
        }
        private static void AddTxtElement(IPoint txtpoint, string txt, ITextSymbol symbol, IRepresentationGraphics g_)
        {
            double fsize = (symbol as ISimpleTextSymbol).Size;
            double dx = 0;

            var splits = txt.Split('\n', '\r');
            List<string> lists = new List<string>();
            foreach (string str in splits)
            {
                if (str.Trim() != "")
                {
                    lists.Add(str);
                }
            }
            double mapScale = GApplication.Application.MapControl.ReferenceScale;
            double txtunit = 1.0;//1号字体宽度

            double heightunit = 13.0 / 15.0;//1号字体的高度

            string[] texts = lists.ToArray();
            int row = texts.Length - 1;//行
            foreach (var t in texts)
            {
                dx = txtpoint.X;
                int col = 0;
                double dis = 0;
                double tedis = 0;
                string tt = (string)t;
                double ttnum = GetStrLen(tt);
                dis = ttnum / 2 * txtunit * fsize;//当前文字一半长度
                //dx = dx - dis;
                for (int i = 0; i < t.Length; i++, col++)
                {
                    string temp = t[i].ToString();
                    double tempnum = GetStrLen(temp);
                    //当前的位置
                    //double x = txtpoint.X;
                    //double y = txtpoint.Y;
                    double x = dx + tedis + tempnum * txtunit * fsize / 2 / 2;
                    double y = txtpoint.Y + row * heightunit * fsize + 0.65 * 0.5 * heightunit * fsize;
                    ESRI.ArcGIS.Geometry.IPoint p = new ESRI.ArcGIS.Geometry.PointClass() { X = x, Y = y };
                    //将当前点添加到制图表达中
                    TransMapPoint(t[i], p, symbol, g_);
                    tedis += tempnum * txtunit * fsize / 2;
                }
                row--;
            }
        }
        private static void TransMapPoint(char c, IPoint txtpoint, ITextSymbol symbol, IRepresentationGraphics g)
        {
            IMapContext mc = new MapContextClass();


            var xx = symbol as ISimpleTextSymbol;
            var cms = new CharacterMarkerSymbolClass();

            cms.CharacterIndex = (int)c;
            cms.Color = xx.Color;
            cms.Size = xx.Size * 1.2;
            cms.Font = xx.Font;
            cms.Angle = xx.Angle;


            var rule = new RepresentationRuleClass();
            rule.InitWithSymbol(cms as ISymbol);
            var pt = txtpoint as ESRI.ArcGIS.Geometry.IPoint;
            //获取每个文字的坐标
            g.Add(pt, rule);

        }
        //获取文字总长度
        private static double GetStrLen(string str)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if ((int)str[i] > 127)
                    count++;
            }
            return str.Length - count + count * 2;
        }
        //添加当前范围到 
        private void AddCurrendExtent(IRepresentationGraphics g, ISpatialReference sr, IPoint ctPoint, double scale)
        {
            var layer = _app.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToLower() == "clipboundary")).FirstOrDefault();
            if (layer == null)
            {
                MessageBox.Show("找不到PAGE图层", "提示");
                return;
            }
            IFeatureClass fc = (layer as IFeatureLayer).FeatureClass;
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE = '裁切面'";
            IFeatureCursor pCursor = fc.Search(qf, true);
            IFeature f = pCursor.NextFeature();
            if (null == f)
            {
                MessageBox.Show("无法找到裁切面要素!", "提示");
                return;
            }
            Marshal.ReleaseComObject(pCursor);

            IGeometry geo = f.ShapeCopy;
            (geo as IPolycurve).Generalize(3.5e-3 * GApplication.Application.ActiveView.FocusMap.ReferenceScale);
            
            IGeometry geoClone = (geo as IClone).Clone() as IGeometry;
            geoClone.Project(sr);
            IPoint orgin = new PointClass() { X = 0, Y = 0 };

            double dx = -ctPoint.X;
            double dy = -ctPoint.Y;
            ITransform2D trans = geoClone as ITransform2D;
            trans.Move(dx, dy);
            trans.Scale(orgin, scale, scale);
            //定义rule
            IRepresentationRule pRule = new RepresentationRule();
            #region
            //定义线
            IColor plineColor = new RgbColorClass() { Red = 255, Blue = 0, Green = 0 };
            IBasicLineSymbol pBasicLine = new BasicLineSymbolClass();
            ILineStroke pLineStroke = new LineStrokeClass();
            IGraphicAttributes lineAttrs = pLineStroke as IGraphicAttributes;
            lineAttrs.set_Value(0, 1); //Define width.    
            lineAttrs.set_Value(3, plineColor); //Define color.           
            pBasicLine.Stroke = pLineStroke;
            pRule.InsertLayer(0, pBasicLine as IBasicSymbol);
            //定义面
            IBasicFillSymbol pBasicFill = new BasicFillSymbolClass();
            IFillPattern pFillPattern = null;
            IGraphicAttributes fillAttrs = null;
            //单色模式:设为空色
            IColor pcolor = new RgbColorClass() { Red = 255, Blue = 0, Green = 0 };
            pcolor = ObtainNULLColor();
            pFillPattern = new SolidColorPattern();
            fillAttrs = pFillPattern as IGraphicAttributes;
            ISimpleFillSymbol psfs = new SimpleFillSymbolClass();
            psfs.Color = pcolor;
            fillAttrs.set_Value(0, pcolor); //Define color.           
            pBasicFill.FillPattern = pFillPattern;
            pRule.InsertLayer(1, pBasicFill as IBasicSymbol);
            #endregion
            g.Add(geoClone, pRule);
        }
        private static IColor ObtainNULLColor()
        {
            ICmykColor pnullcolor = new CmykColorClass();
            pnullcolor.CMYK = 6;
            pnullcolor.Black = 6;
            pnullcolor.Cyan = 0;
            pnullcolor.Magenta = 0;
            pnullcolor.NullColor = true;
            pnullcolor.RGB = 15790320;
            pnullcolor.Transparency = 0;
            pnullcolor.UseWindowsDithering = true;
            pnullcolor.Yellow = 0;
            return pnullcolor;


        }
        //添加背景要素
        private void AddFeatureBgRule(IRepresentationGraphics g, IGeometry geo, IRepresentationRule rule, IPoint ctPoint, double scale)
        {
            //获取图元中心点
            IGeometry geoClone = (geo as IClone).Clone() as IGeometry;
            IPoint orgin = new PointClass() { X = 0, Y = 0 };

            double dx = -ctPoint.X;
            double dy = -ctPoint.Y;
            ITransform2D trans = geoClone as ITransform2D;
            trans.Move(dx, dy);
            trans.Scale(orgin, scale, scale);
            IRectangleElement pReEle=new RectangleElementClass();
            IElement ele=pReEle as IElement;
            ele.Geometry = geoClone.Envelope;
            
            //根据Rule获取几何
            g.Add(ele.Geometry, rule);


        }
        private void AddFeatureRule(IRepresentationGraphics g, IGeometry geo, IRepresentationRule rule, IPoint ctPoint, double scale)
        {
            //获取图元中心点
            IGeometry geoClone = (geo as IClone).Clone() as IGeometry;
            IPoint orgin = new PointClass() { X = 0, Y = 0 };

            double dx = -ctPoint.X;
            double dy = -ctPoint.Y;
            ITransform2D trans = geoClone as ITransform2D;
            trans.Move(dx, dy);
            trans.Scale(orgin, scale, scale);
            //根据Rule获取几何
            g.Add(geoClone, rule);


        }
        private void ClearFeatures(IFeatureClass fc, IQueryFilter qf_)
        {
            IFeature fe;
            IFeatureCursor cursor = fc.Update(qf_, false);
            while ((fe = cursor.NextFeature()) != null)
            {
                cursor.DeleteFeature();
            }
            Marshal.ReleaseComObject(cursor);
        }
        //获取附图模板规则
        private Dictionary<int, IRepresentationRule> ObtainFigureMapRepRule(IFeatureClass fc, IWorkspace ws)
        {

            Dictionary<int, IRepresentationRule> ruleDics = new Dictionary<int, IRepresentationRule>();

            //根据数据库获取制图表达要素类
            #region
            IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(ws);
            IRepresentationRule rule = null;
            if (repWS.get_FeatureClassHasRepresentations(fc))
            {
                IEnumDatasetName enumDatasetName = repWS.get_FeatureClassRepresentationNames(fc);
                enumDatasetName.Reset();
                IDatasetName pDatasetName = enumDatasetName.Next();
                IRepresentationClass m_RepClass = repWS.OpenRepresentationClass(pDatasetName.Name);
                IRepresentationRules rules = m_RepClass.RepresentationRules;
                rules.Reset();

                int ruleID;
                while (true)
                {
                    rules.Next(out ruleID, out rule);
                    if (rule == null) break;
                    ruleDics.Add(ruleID, rule);
                }

            }
            #endregion
            return ruleDics;

        }
        private void InitRepPramas(out IFeatureClass fc, out IWorkspace ws, string fclName)
        {
            IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
            string path = GApplication.Application.Template.Root + "\\位置图\\位置图.gdb";

            ws = factory.OpenFromFile(path, 0);
            bool exist = (ws as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, fclName);
            if (exist)
            {
                fc = (ws as IFeatureWorkspace).OpenFeatureClass(fclName);
            }
            else
            {
                fc = null;
            }
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

using System;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using System.Collections;
using ESRI.ArcGIS.esriSystem;
using System.Collections.Generic;
using stdole;
using System.Data;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using System.Xml.Linq;
namespace SMGI.Plugin.EmergencyMap.MapDeOut
{
    public enum LengendModel
    {
        MapView = 0,
        Template = 1
    }
    
     //图例根据当前图层动态生成
    public class LengendHelper
    {
        IFeatureClass fclPoint = null;//放置图例图层
        ILayer repLyr = null;
        DataTable compositeDataTable = null;//组合图例项:图名(FclName)，RuleID,组合ID(GroupRuleID),位置(Location)
        Dictionary<int, IRepresentationRule> groupRulesDic = new Dictionary<int, IRepresentationRule>();//id->reprule
        DataTable multiDataTable = null;//共享图例项：
        Dictionary<int, IRepresentationRule> multiRulesDic = new Dictionary<int, IRepresentationRule>();//id->reprule
        GApplication m_Application = null;
        IFeatureClass annoFcl = null;
        IFeatureClass fclPolygon = null;
        IPoint orginPoint = null;

        public string LGTemplate = "常规图例";//图例模板类型：影像，常规图例，水利等等
        public string LgLocation = "IN";
        public string MapLgType = "通用";//省图，市图,县图
        double mapScale = 0;
        int lengendColumn = 1;//图例的列数
        IEnvelope LgEnvelopeIn = null;
        string lgRulePath = string.Empty;
        public bool lgSizeType =true;
        public bool GroupLengend = true;
        public LengendHelper(IEnvelope LgEnvelopeIn_=null)
        {
           
           
           
            LgEnvelopeIn = LgEnvelopeIn_;
            repLyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOINT"))).FirstOrDefault();
            if (repLyr != null)
            {
                fclPoint = (repLyr as IFeatureLayer).FeatureClass;
            }
            var lyr= GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LANNO"))).FirstOrDefault();
            if (lyr != null)
            {
                 annoFcl = (lyr as IFeatureLayer).FeatureClass;
            }
            lyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOLY"))).FirstOrDefault();
            if (lyr != null)
            {
                fclPolygon = (lyr as IFeatureLayer).FeatureClass;
            }
            m_Application = GApplication.Application;
        }


        Dictionary<string, List<string>> itemOrders = new Dictionary<string, List<string>>();
        /// 对图例项进行排序ItemInfo
        private void LoadItemOrder()
        {
            if (File.Exists(lgRulePath + "LengendOrder.xml"))
            {
                var doc = XDocument.Load(lgRulePath + "LengendOrder.xml");
                var lengends = doc.Element("Lengend").Elements("Item");
                foreach (var item in lengends)
                {
                    string fclname = item.Attribute("FeatureClass").Value;
                    List<string> ids = item.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    itemOrders[fclname] = ids;
                }
                
            }
        }
        int CompareLengendItem(ItemInfo item1, ItemInfo item2)
        {
            if(itemOrders.Count==0)
            {
                LoadItemOrder();
            }
            if (itemOrders.Count == 0)
                return 0;
            int termIndex1 = System.Array.IndexOf(itemOrders.Keys.ToArray(), item1.LyrName);
            int termIndex2 = System.Array.IndexOf(itemOrders.Keys.ToArray(), item2.LyrName);
            if (termIndex1 == -1 && termIndex2!=-1)
            {
                return 1;
            }
            if (termIndex1 != -1 && termIndex2 == -1)
            {
                return -1;
            }
            if (termIndex1 == -1 && termIndex2 == -1)
            {
                return 0;
            }
            if (termIndex1 == termIndex2)
            {
                int ruleID1 =itemOrders[item1.LyrName].IndexOf(item1.RuleID.ToString());
                int ruleID2 = itemOrders[item2.LyrName].IndexOf(item2.RuleID.ToString());
                if (ruleID1 == -1 && ruleID2 != -1)
                {
                    return 1;
                }
                if (ruleID1 != -1 && ruleID2 == -1)
                {
                    return -1;
                }
                if (ruleID1 == -1 && ruleID2 == -1)
                {
                    return 0;
                }
                return ruleID1.CompareTo(ruleID2);
            }
            else
            {
                return termIndex1.CompareTo(termIndex2);
            }
            
        }
        //公开调用方法
        //int step = 0;
        /// <summary>
        /// 从地图视图创造图例
        /// </summary>
        /// <param name="annoIndex"></param>
        /// <param name="itemInfos"></param>
        /// <param name="flyrs"></param>
        /// <param name="pBasePoint"></param>
        /// <param name="column"></param>
        public void CreateLengendMapView(int annoIndex, List<AnnoItemInfo> itemInfos, List<KeyValuePair<ILayer, Dictionary<int, string>>> flyrs, IPoint pBasePoint_, int column = 1)
        {
            Dictionary<string, string> envString = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
                lgRulePath = GApplication.Application.Template.Root + "\\底图\\" + envString["BaseMap"] + "\\" + envString["MapTemplate"] + "\\图例\\";
                lgRulePath +=LGTemplate+ "\\"; 
                mapScale = m_Application.ActiveView.FocusMap.ReferenceScale;
                for (int c = 0; c < column; c++)
                {
                     dyDics[c] = 0;
                    itemsInfo.Add(new lengendItemInfo { Column = c, Y = 0 });

                }
                //step = 0;
                lengendColumn = column;
                lengendWidth = lengendColumn * repItemXStep / 2.83;
                double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize / 2.83 + 5) / 1000;
                var pBasePoint = (pBasePoint_ as IClone).Clone() as IPoint;
                switch (location)
                {
                    case "左上":
                        if (LgLocation == "IN")
                        {
                            pBasePoint.X += (3) * mapScale / 1000;
                        }
                        if (LgLocation == "OUT")
                        {
                            pBasePoint.X -= (2 + lengendWidth) * mapScale / 1000;
                        }
                        pBasePoint.Y -= dy;
                        break;
                    case "右上":
                        if (LgLocation == "IN")
                        {
                            pBasePoint.X -= (lengendWidth + 3) * mapScale / 1000;
                        }
                        if (LgLocation == "OUT")
                        {
                            pBasePoint.X += 2 * mapScale / 1000;
                        }
                        pBasePoint.Y -= dy;
                        break;
                    case "左下":
                        if (LgLocation == "IN")
                        {
                            pBasePoint.X += (3) * mapScale / 1000;
                        }
                        if (LgLocation == "OUT")
                        {
                            pBasePoint.X -= (lengendWidth + 2) * mapScale / 1000;
                        }
                        break;
                    case "右下":
                        if (LgLocation == "IN")
                        {
                            pBasePoint.X -= (lengendWidth + 3) * mapScale / 1000;
                        }
                        if (LgLocation == "OUT")
                        {
                            pBasePoint.X += (2) * mapScale / 1000;
                        }
                        break;
                    default:
                        break;
                }
                //清空
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "TYPE like '%图例%'";
                ClearFeatures(fclPoint, qf);

                ClearFeatures(annoFcl, qf);
                ClearFeatures(fclPolygon, qf);
                orginPoint = pBasePoint;

                //初始化
                LoadCompositeItems();
                LoadShareItems();
                IPoint feOrgin = (pBasePoint as IClone).Clone() as IPoint;

                IFeature fe = fclPoint.CreateFeature();
                fe.Shape = feOrgin;
                fe.set_Value(fclPoint.FindField("TYPE"), "图例");
                fe.set_Value(fclPoint.FindField("RuleID"), 1);
                fe.Store();
                IRepresentationGraphics pGraphics = new RepresentationMarkerClass();
                for (int i = 0; i < flyrs.Count; i++)
                {
                    if (i == annoIndex)
                    {
                        //处理注记图例
                        InsertLengendAnnoItems(pGraphics, itemInfos, fe.OID);
                    }
                    var kv = flyrs[i];
                    //1.遍历图层，获取要素对应的制图表达
                    var lyr = kv.Key;
                    IFeatureLayer flyr = lyr as IFeatureLayer;
                    var rulesDic = UniqueValueRepRender(flyr, kv.Value.Keys.ToList());
                    switch (flyr.FeatureClass.ShapeType)
                    {
                        case esriGeometryType.esriGeometryPoint:
                            InsertRuleLendgendPoint(flyr, pGraphics, rulesDic, fe.OID, kv);
                            break;
                        case esriGeometryType.esriGeometryPolyline:
                            InsertRuleLendgendPolyLine(flyr, pGraphics, rulesDic, fe.OID, kv);
                            break;
                        case esriGeometryType.esriGeometryPolygon:
                            InsertRuleLendgendPolygon(flyr, pGraphics, rulesDic, fe.OID, kv);
                            break;
                        default:
                            break;
                    }
                }
                //重新布局图例项
                #region
                pGraphics.RemoveAll();
                int ct = lengendItems.Count;
                if (File.Exists(lgRulePath + "LengendOrder.xml"))//按照规则排序
                {
                   // lengendItems.Sort(CompareLengendItem);
                }
                int val = ct / column;//每列的数量
                int mod = ct % column;
                int index = 0;
                for (int c = 0; c < column; c++)
                {

                    double dx = c * repItemXStep;
                    double ystep = 0;
                    for (int row = 0; row < val; row++)
                    {
                        ystep = repItemYStep * row;
                        var item = lengendItems[index];
                        foreach (var georule in item.GeoRules)
                        {
                            IGeometry geometry = georule.Geometry;
                            if (item.lengendType == lgType.AnnoLg)
                            {
                                (geometry as ITransform2D).Move(lgItemLen / 2 + dx, -ystep);
                            }
                            else
                            {
                                (geometry as ITransform2D).Move(dx, -ystep);
                            }
                            IRepresentationRule rule = georule.Rule;
                            pGraphics.Add(geometry, rule);

                        }
                        AddLengendItemText(item, dx, ystep, fe.OID);
                        index++;
                    }
                    if (c < mod)//追加
                    {
                        ystep = repItemYStep * val;
                        var item = lengendItems[index];

                        foreach (var georule in item.GeoRules)
                        {
                            IGeometry geometry = georule.Geometry;
                            if (item.lengendType == lgType.AnnoLg)
                            {
                                (geometry as ITransform2D).Move(lgItemLen / 2 + dx, -ystep);
                            }
                            else
                            {
                                (geometry as ITransform2D).Move(dx, -ystep);
                            }

                            IRepresentationRule rule = georule.Rule;
                            pGraphics.Add(geometry, rule);

                        }

                        AddLengendItemText(item, dx, ystep, fe.OID);
                        index++;
                    }
                }

                #endregion
                IRepresentationRule r = new RepresentationRuleClass();
                var bs = new BasicMarkerSymbolClass();
                bs.set_Value(1, pGraphics); //marker
                //
                var repMaker = pGraphics as IRepresentationMarker;
                double size = Math.Max(repMaker.Width, repMaker.Height);
                bs.set_Value(2, size); //size
                bs.set_Value(3, 0);//angle
                bs.set_Value(4, false);
                r.InsertLayer(0, bs);

                var rp = (repLyr as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                IRepresentationClass m_RepClass = rp.RepresentationClass;
                IMapContext mc = new MapContextClass();
                mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
                IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
                IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
                repGraphics.Add(feOrgin, r);
                rep.Graphics = repGraphics;
                rep.UpdateFeature();
                rep.Feature.Store();
                lengendHeight = repMaker.Height / 2.83;
                CreateLengendPolygon(TitleFontSize, fe.OID);
                qf.WhereClause = "TYPE = '图例外边线'";
                IFeatureCursor cursor = fclPolygon.Search(qf, false);
                IFeature feEnv = cursor.NextFeature();
                if (feEnv == null)
                {
                    qf.WhereClause = "TYPE = '图例内边线'";                   ;
                    cursor = fclPolygon.Search(qf, false);
                    feEnv = cursor.NextFeature();
                }
                Marshal.ReleaseComObject(mc);
                Marshal.ReleaseComObject(cursor);
                if (feEnv != null)
                {
                    m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, repLyr, feEnv.Extent);
               
                Marshal.ReleaseComObject(feEnv);
                }
                GC.Collect();
            
         
        }

        //自动批量命令【自动判断位置图例】
        public void CreateLengendMapViewAuto(IEnvelope enout, IEnvelope en, int annoIndex, List<AnnoItemInfo> itemInfos, List<KeyValuePair<ILayer, Dictionary<int, string>>> flyrs, IPoint pBasePoint_, int column = 1)
        {
            Dictionary<string, string> envString = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            lgRulePath = GApplication.Application.Template.Root + "\\底图\\" + envString["BaseMap"] + "\\" + envString["MapTemplate"] + "\\图例\\";
            lgRulePath += LGTemplate + "\\"; 
            mapScale = m_Application.ActiveView.FocusMap.ReferenceScale;
            for (int c = 0; c < column; c++)
            {
                dyDics[c] = 0;
                itemsInfo.Add(new lengendItemInfo { Column = c, Y = 0 });

            }
            //step = 0;
            lengendColumn = column;
            lengendWidth = lengendColumn * repItemXStep / 2.83;
            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize / 2.83 + 5) / 1000;
            var pBasePoint = (pBasePoint_ as IClone).Clone() as IPoint;
            switch (location)
            {
                case "左上":
                    if (LgLocation == "IN")
                    {
                        pBasePoint.X += (3) * mapScale / 1000;
                    }
                    if (LgLocation == "OUT")
                    {
                        pBasePoint.X -= (2 + lengendWidth) * mapScale / 1000;
                    }
                    pBasePoint.Y -= dy;
                    break;
                case "右上":
                    if (LgLocation == "IN")
                    {
                        pBasePoint.X -= (lengendWidth + 3) * mapScale / 1000;
                    }
                    if (LgLocation == "OUT")
                    {
                        pBasePoint.X += 2 * mapScale / 1000;
                    }
                    pBasePoint.Y -= dy;
                    break;
                case "左下":
                    if (LgLocation == "IN")
                    {
                        pBasePoint.X += (3) * mapScale / 1000;
                    }
                    if (LgLocation == "OUT")
                    {
                        pBasePoint.X -= (lengendWidth + 2) * mapScale / 1000;
                    }
                    break;
                case "右下":
                    if (LgLocation == "IN")
                    {
                        pBasePoint.X -= (lengendWidth + 3) * mapScale / 1000;
                    }
                    if (LgLocation == "OUT")
                    {
                        pBasePoint.X += (2) * mapScale / 1000;
                    }
                    break;
                default:
                    break;
            }
            //清空
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE like '%图例%'";
            ClearFeatures(fclPoint, qf);

            ClearFeatures(annoFcl, qf);
            ClearFeatures(fclPolygon, qf);
            orginPoint = pBasePoint;

            //初始化
            LoadCompositeItems();
            LoadShareItems();
            IPoint feOrgin = (pBasePoint as IClone).Clone() as IPoint;
            IFeature fe = fclPoint.CreateFeature();
            fe.Shape = feOrgin;
            fe.set_Value(fclPoint.FindField("TYPE"), "图例");
            fe.set_Value(fclPoint.FindField("RuleID"), 1);
            fe.Store();
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();
            for (int i = 0; i < flyrs.Count; i++)
            {
                if (i == annoIndex)
                {
                    //处理注记图例
                    InsertLengendAnnoItems(pGraphics, itemInfos, fe.OID);
                }
                var kv = flyrs[i];
                //1.遍历图层，获取要素对应的制图表达
                var lyr = kv.Key;
                IFeatureLayer flyr = lyr as IFeatureLayer;
                var rulesDic = UniqueValueRepRender(flyr, kv.Value.Keys.ToList());
                switch (flyr.FeatureClass.ShapeType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        InsertRuleLendgendPoint(flyr, pGraphics, rulesDic, fe.OID);
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        InsertRuleLendgendPolyLine(flyr, pGraphics, rulesDic, fe.OID);
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        InsertRuleLendgendPolygon(flyr, pGraphics, rulesDic, fe.OID);
                        break;
                    default:
                        break;
                }
            }
            //重新布局图例项
            #region
            pGraphics.RemoveAll();
            int ct = lengendItems.Count;

            int val = ct / column;//每列的数量
            int mod = ct % column;
            int index = 0;
            for (int c = 0; c < column; c++)
            {

                double dx = c * repItemXStep;
                double ystep = 0;
                for (int row = 0; row < val; row++)
                {
                    ystep = repItemYStep * row;
                    var item = lengendItems[index];
                    foreach (var georule in item.GeoRules)
                    {
                        IGeometry geometry = georule.Geometry;
                        if (item.lengendType == lgType.AnnoLg)
                        {
                            (geometry as ITransform2D).Move(lgItemLen / 2 + dx, -ystep);
                        }
                        else
                        {
                            (geometry as ITransform2D).Move(dx, -ystep);
                        }
                        IRepresentationRule rule = georule.Rule;
                        pGraphics.Add(geometry, rule);

                    }
                    AddLengendItemText(item, dx, ystep, fe.OID);
                    index++;
                }
                if (c < mod)//追加
                {
                    ystep = repItemYStep * val;
                    var item = lengendItems[index];

                    foreach (var georule in item.GeoRules)
                    {
                        IGeometry geometry = georule.Geometry;
                        if (item.lengendType == lgType.AnnoLg)
                        {
                            (geometry as ITransform2D).Move(lgItemLen / 2 + dx, -ystep);
                        }
                        else
                        {
                            (geometry as ITransform2D).Move(dx, -ystep);
                        }

                        IRepresentationRule rule = georule.Rule;
                        pGraphics.Add(geometry, rule);

                    }

                    AddLengendItemText(item, dx, ystep, fe.OID);
                    index++;
                }
            }

            #endregion
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            //
            var repMaker = pGraphics as IRepresentationMarker;
            double size = Math.Max(repMaker.Width, repMaker.Height);
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);
          
            var rp = (repLyr as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(feOrgin, r);
            rep.Graphics = repGraphics;
            rep.UpdateFeature();
            rep.Feature.Store();
            lengendHeight = repMaker.Height / 2.83;
            
            CreateLengendPolygon(TitleFontSize, fe.OID);
          
            qf.WhereClause = "TYPE = '图例外边线'";
            IFeatureCursor fcursor = fclPolygon.Search(qf, false);
            IFeature feEnv = fcursor.NextFeature();
            
            #region 确定图例位置
           // return;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "ClipBoundary");
            })).FirstOrDefault() as IFeatureLayer;
            IFeature f;
            IFeatureCursor cursor = lyrs.FeatureClass.Search(new QueryFilterClass { WhereClause = "TYPE = '裁切面'" }, false);
            f = cursor.NextFeature();
            Marshal.ReleaseComObject(cursor);
            if (f != null)
            {
                IRelationalOperator ro = f.ShapeCopy as IRelationalOperator;
                IGeometry lengendLine = feEnv.ShapeCopy;
                bool flag = false;
                if (ro.Disjoint(lengendLine))//左上不相交
                {
                    flag = true;
                }
                if (!flag)
                {
                    double dxlg = en.Width - lengendLine.Envelope.Width;
                    double dylg = 0;
                    //平移 左上->右上
                    (lengendLine as ITransform2D).Move(dxlg, dylg);
                    if (ro.Disjoint(lengendLine))//右上不相交
                    {
                        MoveLengend(rep, dxlg, dylg,2,0);
                        flag = true;
                    }  
                }
               if (!flag)
                {
                    double dxlg = en.Width - lengendLine.Envelope.Width;
                    double dylg = -en.Height + lengendLine.Envelope.Height;
                    lengendLine = feEnv.ShapeCopy;
                    (lengendLine as ITransform2D).Move(dxlg, dylg);
                    if (ro.Disjoint(lengendLine))//右下不相交
                    {
                        //平移 左上->右下
                        MoveLengend(rep, dxlg, dylg, 2, -2);
                        flag = true;
                    }
                }
               if (!flag)
               {
                   double dxlg = 0;
                   double dylg = -en.Height + lengendLine.Envelope.Height;
                   lengendLine = feEnv.ShapeCopy;
                   (lengendLine as ITransform2D).Move(dxlg, dylg);
                   if (ro.Disjoint(lengendLine))
                   {
                       //平移 左上->左下
                       MoveLengend(rep, dxlg, dylg, 0, -2);
                   }
               }
               
            }
            #endregion 
            Marshal.ReleaseComObject(mc);
            Marshal.ReleaseComObject(fcursor);
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, repLyr, feEnv.Extent);
            Marshal.ReleaseComObject(feEnv);
            GC.Collect();
            return;
         
        }
        private void MoveLengend( IRepresentation rep ,double dx,double dy,double dxEx,double dyEx)
        {
            IFeature fe = rep.Feature;
            IGeometry geoOld = fe.ShapeCopy;
                 
            //更改库 几何
            ITransform2D ptrans = geoOld as ITransform2D;
            ptrans.Move(dx + dxEx * mapScale * 1.0e-3, dy + dyEx * mapScale * 1.0e-3);
            fe.Shape = geoOld;
            fe.Store();
            //更改制图图元:自由式
            if (rep.RuleID == -1)
            {

                var g = rep.Graphics;
                g.ResetGeometry();
                while (true)
                {
                    IGeometry geo;
                    int id;
                    g.NextGeometry(out id, out geo);
                    if (geo == null)
                        break;

                    var trans = (geo as IClone).Clone() as ITransform2D;
                    trans.Move(dx + dxEx * mapScale * 1.0e-3, dy + dyEx * mapScale * 1.0e-3);
                    g.ChangeGeometry(id, trans as IGeometry);
                }
                rep.Graphics = g;
                rep.UpdateFeature();
                rep.Feature.Store();
                //关联注记平移
                RelatedAnnoFeatuesMove(fe, dx + dxEx * mapScale * 1.0e-3, dy + dyEx * mapScale * 1.0e-3);
                //关联图例边
             
                {
                    ILayer polygonLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOLY"))).FirstOrDefault();
                    IFeatureClass polygonFcl = (polygonLayer as IFeatureLayer).FeatureClass;
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = "TYPE like '图例%'";
                    IFeatureCursor  cursor = polygonFcl.Update(qf, false);
                    while ((fe = cursor.NextFeature()) != null)
                    {
                         
                        string type = fe.get_Value(polygonFcl.FindField("TYPE")).ToString();
                        if (type == "图例内边线")
                        {
                            ITransform2D ptransPoly = fe.ShapeCopy as ITransform2D;
                            ptransPoly.Move(dx + dxEx * mapScale * 1.0e-3, dy + dyEx * mapScale * 1.0e-3);
                            fe.Shape = ptransPoly as IGeometry;
                            //fe.Store();
                            cursor.UpdateFeature(fe);
                        }
                        if (type == "图例外边线")
                        {
                            ITransform2D ptransPoly = fe.ShapeCopy as ITransform2D;
                            ptransPoly.Move(dx, dy);
                            fe.Shape = ptransPoly as IGeometry;
                            // fe.Store();
                            cursor.UpdateFeature(fe);
                        }
                         
                    }
                    Marshal.ReleaseComObject(cursor);
                }
               
            }
        }
        
      
       /// <summary>
       /// 从模板创造图例
       /// </summary>
       /// <param name="annoIndex"></param>
       /// <param name="itemInfos"></param>
       /// <param name="flyrs"></param>
       /// <param name="pBasePoint"></param>
       /// <param name="column"></param>
        public void CreateLengendTemplate(int annoIndex, List<AnnoItemInfo> itemInfos, List<KeyValuePair<ILayer, Dictionary<int, string>>> flyrs, IPoint pBasePoint_, int column = 1)
        {
            Dictionary<string, string> envString = GApplication.Application.Workspace.MapConfig["EMEnvironment"] as Dictionary<string, string>;
            if (envString == null)
            {
                envString = EnvironmentSettings.GetConfigVal("EMEnvironmentXML");
            }
            lgRulePath = GApplication.Application.Template.Root + "\\底图\\" + envString["BaseMap"] + "\\" + envString["MapTemplate"] + "\\图例\\";
            lgRulePath += LGTemplate + "\\"; 
            mapScale = m_Application.ActiveView.FocusMap.ReferenceScale;
            for (int c = 0; c < column; c++)
            {
                dyDics[c] = 0;
                itemsInfo.Add(new lengendItemInfo { Column = c, Y = 0 });

            }
            //step = 0;
            lengendColumn = column;
            lengendWidth = lengendColumn * repItemXStep / 2.83;
            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize / 2.83 + 5) / 1000;
            var pBasePoint = (pBasePoint_ as IClone).Clone() as IPoint;
            switch (location)
            {
                case "左上":
                    if (LgLocation == "IN")
                    {
                        pBasePoint.X += (3) * mapScale / 1000;
                    }
                    if (LgLocation == "OUT")
                    {
                        pBasePoint.X -= (2 + lengendWidth) * mapScale / 1000;
                    }
                    pBasePoint.Y -= dy;
                    break;
                case "右上":
                    if (LgLocation == "IN")
                    {
                        pBasePoint.X -= (lengendWidth + 3) * mapScale / 1000;
                    }
                    if (LgLocation == "OUT")
                    {
                        pBasePoint.X += 2 * mapScale / 1000;
                    }
                    pBasePoint.Y -= dy;
                    break;
                case "左下":
                    if (LgLocation == "IN")
                    {
                        pBasePoint.X += (3) * mapScale / 1000;
                    }
                    if (LgLocation == "OUT")
                    {
                        pBasePoint.X -= (lengendWidth + 2) * mapScale / 1000;
                    }
                    break;
                case "右下":
                    if (LgLocation == "IN")
                    {
                        pBasePoint.X -= (lengendWidth + 3) * mapScale / 1000;
                    }
                    if (LgLocation == "OUT")
                    {
                        pBasePoint.X += (2) * mapScale / 1000;
                    }
                    break;
                default:
                    break;
            }


            //清空
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "TYPE like '%图例%'";
            ClearFeatures(fclPoint, qf);

            ClearFeatures(annoFcl, qf);
            ClearFeatures(fclPolygon, qf);
            orginPoint = pBasePoint;

            //初始化
            LoadCompositeItems();
            LoadShareItems();
            IPoint feOrgin = (pBasePoint as IClone).Clone() as IPoint;
            IFeature fe = fclPoint.CreateFeature();
            fe.Shape = feOrgin;
            fe.set_Value(fclPoint.FindField("TYPE"), "图例");
            fe.set_Value(fclPoint.FindField("RuleID"), 1);
            fe.Store();
            IRepresentationGraphics pGraphics = new RepresentationMarkerClass();
            for (int i = 0; i < flyrs.Count; i++)
            {
                if (i == annoIndex)
                {
                    //处理注记图例
                    InsertLengendAnnoItems(pGraphics, itemInfos, fe.OID);
                }
                var kv = flyrs[i];
                //1.遍历图层，获取要素对应的制图表达
                var lyr = kv.Key;
                IFeatureLayer flyr = lyr as IFeatureLayer;
                var rulesDic = UniqueValueRepRender(flyr, kv.Value.Keys.ToList());
                switch (flyr.FeatureClass.ShapeType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        InsertRuleLendgendPoint(flyr, pGraphics, rulesDic, fe.OID, kv.Value);
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        InsertRuleLendgendPolyLine(flyr, pGraphics, rulesDic, fe.OID, kv.Value);
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        InsertRuleLendgendPolygon(flyr, pGraphics, rulesDic, fe.OID, kv.Value);
                        break;
                    default:
                        break;
                }

            }
            //重新布局图例项
            #region
            pGraphics.RemoveAll();
            int ct = lengendItems.Count;

            int val = ct / column;//每列的数量
            int mod = ct % column;
            int index = 0;
            for (int c = 0; c < column; c++)
            {

                double dx = c * repItemXStep;
                double ystep = 0;
                for (int row = 0; row < val; row++)
                {
                    ystep = repItemYStep * row;
                    var item = lengendItems[index];
                    foreach (var georule in item.GeoRules)
                    {
                        IGeometry geometry = georule.Geometry;
                        if (item.lengendType == lgType.AnnoLg)
                        {
                            (geometry as ITransform2D).Move(lgItemLen / 2 + dx, -ystep);
                        }
                        else
                        {
                            (geometry as ITransform2D).Move(dx, -ystep);
                        }
                        IRepresentationRule rule = georule.Rule;
                        pGraphics.Add(geometry, rule);

                    }
                  
                    AddLengendItemText(item, dx, ystep, fe.OID);
                    index++;
                }
                if (c < mod)//追加
                {
                    ystep = repItemYStep * val;
                    var item = lengendItems[index];

                    foreach (var georule in item.GeoRules)
                    {
                        IGeometry geometry = georule.Geometry;
                        if (item.lengendType == lgType.AnnoLg)
                        {
                            (geometry as ITransform2D).Move(lgItemLen / 2 + dx, -ystep);
                        }
                        else
                        {
                            (geometry as ITransform2D).Move(dx, -ystep);
                        }
                      
                        IRepresentationRule rule = georule.Rule;
                        pGraphics.Add(geometry, rule);
                            
                    }
                    
                    AddLengendItemText(item, dx, ystep, fe.OID);
                    index++;
                }
            }

            #endregion
           
            IRepresentationRule r = new RepresentationRuleClass();
            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, pGraphics); //marker
            //
            var repMaker = pGraphics as IRepresentationMarker;
            double size = Math.Max(repMaker.Width, repMaker.Height);
            bs.set_Value(2, size); //size
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);
            r.InsertLayer(0, bs);

            var rp = (repLyr as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IMapContext mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(feOrgin, r);
            rep.Graphics = repGraphics;
            rep.UpdateFeature();
            rep.Feature.Store();
            lengendHeight = repMaker.Height / 2.83;
            CreateLengendPolygon(TitleFontSize, fe.OID);
            qf.WhereClause = "TYPE = '图例外边线'";
            IFeatureCursor cursor = fclPolygon.Search(qf, false);
            IFeature feEnv = cursor.NextFeature();
            Marshal.ReleaseComObject(mc);
            Marshal.ReleaseComObject(cursor);
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, repLyr, feEnv.Extent);
            Marshal.ReleaseComObject(feEnv);
            GC.Collect();
        }

        private void AddLengendItemText(ItemInfo item,double dx,double dy,int feID)
        {
            if (item.multTxts.Count >0)
            {
                var mtItemTxt = item.multTxts;
                int flag = 0;
                double txtdy = (mtItemTxt.Count - 1) * 3;
                foreach (var txt in mtItemTxt)
                {
                    var txtPoint = (orginPoint as IClone).Clone() as IPoint;
                    ITransform2D pTransTxt = txtPoint as ITransform2D;
                    pTransTxt.Move((repAnnoStep + dx) * 1.0e-3 * mapScale / 2.83, -(dy + (flag++) * 8 - txtdy) * 1.0e-3 * mapScale / 2.83);
                    AddLengendTxt(txtPoint, txt, feID);
                }
            }
            else
            {
                var txtPoint = (orginPoint as IClone).Clone() as IPoint;
                ITransform2D pTransTxt = txtPoint as ITransform2D;
                pTransTxt.Move((repAnnoStep + dx) * 1.0e-3 * mapScale / 2.83, -dy * 1.0e-3 * mapScale / 2.83);
                AddLengendTxt(txtPoint, item.ItemText, feID);
            }
        }
         
     
        
        double lengendWidth;
        double lengendHeight;
        public string location = "左上";
        //图例边框
        private void CreateLengendPolygon(double fontsize,int feID)
        {
            var point = new PointClass() { X = orginPoint.X + ((lengendWidth) / 2) * mapScale / 1000, Y = orginPoint.Y + (repItemYStep/2.83/2+fontsize/2.83/2+2.5) * mapScale / 1000 };

             InsertAnnoFea(point, "图  例", fontsize, feID, esriTextHorizontalAlignment.esriTHACenter);
            lengendWidth += 5;
            lengendHeight += ScaleBarStep;//图例下边缘留给比例尺间隔
            fontsize = fontsize / 2.83;
            TitleFontSize = fontsize;
            switch (location)
            {
                case "左上":
                    CreateLengendLineLeftUp();
                    break;
                case "右上":
                    CreateLengendLineRightUp();
                    break;
                case "左下":
                    CreateLengendLineLeftDown(feID);
                    break;
                case "右下":
                    CreateLengendLineRightDown(feID);
                    break;
                default:
                    break;
            }
          
        }
        private void CreateLengendLineLeftDown(int feID)
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LPOLY")).FirstOrDefault();

            string mdbpath = m_Application.Template.Root + @"\整饰\整饰规则库.mdb";

            var ruleDtCOM = Helper.ReadToDataTable(mdbpath, "COMMON");//通用RuleID
            DataRow[] drs1 = ruleDtCOM.Select();
            var ruleIDPolygon = new Dictionary<string, int>();
            foreach (DataRow dr in drs1)//COMPASS
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDPolygon[keyname] = val;//!!
            }
            IFeatureClass polygonfcl = (lyrs as IFeatureLayer).FeatureClass;
            int ruleID = ruleIDPolygon["图廓"];
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize +3.5) / 1000;
            orginPoint.Y += dy;
            lengendHeight += repItemYStep / 2.83 / 2 + TitleFontSize +3.5;

            orginPoint.X -= mapScale * (3) / 1000;
            //外环
            IPointCollection pRing = new RingClass();
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + 2 * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y + 2 * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y - (lengendHeight) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - (lengendHeight) * mapScale / 1000 });

            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            IFeature fe = null;
            if (LgLocation == "IN" && lgSizeType)
             {
                  fe = polygonfcl.CreateFeature();
                  fe.Shape = geoCol as IGeometry;
                  fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
                  fe.set_Value(polygonfcl.FindField("TYPE"), "图例外边线");
                  fe.Store();
             }
            //内边线
            geoCol = new PolygonClass();
            _missing = Type.Missing;
         

            pRing = new RingClass();

            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y - lengendHeight * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - lengendHeight * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            fe = polygonfcl.CreateFeature();
            //(geoCol as ITransform2D).Move(lengendWidth * mapScale / 1000, 0);
            fe.Shape = geoCol as IGeometry;
            fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            fe.set_Value(polygonfcl.FindField("TYPE"), "图例内边线");
            fe.Store();
            LgMove(fclPoint.GetFeature(feID), lengendHeight * mapScale / 1000 - dy);
        }
        private void LgMove(IFeature currentFe, double dy)
        {
            //平移
            var rep = ObtainRep(currentFe);
            double dx = 0;

            if (rep.Graphics == null)
            {
                IGeometry geo = rep.ShapeCopy;
                (geo as ITransform2D).Move(dx, dy);
                rep.Shape = geo;
            }
            else
            {
                var g = rep.Graphics;
                g.ResetGeometry();
                while (true)
                {
                    IGeometry geo;
                    int id;
                    g.NextGeometry(out id, out geo);
                    if (geo == null)
                        break;

                    var trans = (geo as IClone).Clone() as ITransform2D;
                    trans.Move(dx, dy);
                    g.ChangeGeometry(id, trans as IGeometry);
                }
                rep.Graphics = g;
            }
            rep.UpdateFeature();
            rep.Feature.Store();
            //关联要素平移

            RelatedFeatuesMove("图例", dx, dy);
            RelatedAnnoFeatuesMove(currentFe, dx, dy);
            //当前点平移
            var fgeo = currentFe.ShapeCopy;
            (fgeo as ITransform2D).Move(dx, dy);
            currentFe.Shape = fgeo;
            currentFe.Store();

        }
        #region
        private IRepresentation ObtainRep(IFeature fe)
        {
            var rp = (repLyr as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            var mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            IRepresentation rep = m_RepClass.GetRepresentation(fe, mc);
            return rep;
        }
         //关联移动
         private void RelatedAnnoFeatuesMove(IFeature sourcefe, double dx, double dy)
         {
             IQueryFilter qf = new QueryFilterClass();
             qf.WhereClause = "AnnotationClassID = " + sourcefe.Class.ObjectClassID + "  and FeatureID =" + sourcefe.OID;
             IFeature fe;

             ILayer annoLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LANNO"))).FirstOrDefault();
             IFeatureClass annoFcl = (annoLayer as IFeatureLayer).FeatureClass;
             if (annoFcl.FeatureCount(qf) == 0)
                 return;
             IFeatureCursor cursor = annoFcl.Search(qf, false);
             while ((fe = cursor.NextFeature()) != null)
             {
                 IAnnotationFeature pAnnoFeature = fe as IAnnotationFeature;

                 ITextElement pTextElement = new TextElementClass();
                 ITextSymbol pTextSymbol = ((pAnnoFeature.Annotation as ITextElement).Symbol as IClone).Clone() as ITextSymbol;
                 pTextElement.Symbol = pTextSymbol;
                 pTextElement.Text = (pAnnoFeature.Annotation as ITextElement).Text;
                 pTextElement.ScaleText = (pAnnoFeature.Annotation as ITextElement).ScaleText;

                 ITransform2D ptrans = pAnnoFeature.Annotation.Geometry as ITransform2D;
                 ptrans.Move(dx, dy);
                 IElement pElement = pTextElement as IElement;
                 pElement.Geometry = ptrans as IGeometry;
                 pAnnoFeature.Annotation = pElement;
                 fe.Store();
             }

             Marshal.ReleaseComObject(cursor);
         }
         //关联要素平移
         private void RelatedFeatuesMove(string filter, double dx, double dy)
         {
             IQueryFilter qf = new QueryFilterClass();
             qf.WhereClause = "TYPE = '" + filter + "'";
             IFeature fe;

             ILayer annoLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LANNO"))).FirstOrDefault();
             IFeatureClass annoFcl = (annoLayer as IFeatureLayer).FeatureClass;
             IFeatureCursor cursor = annoFcl.Search(qf, false);
             while ((fe = cursor.NextFeature()) != null)
             {
                 IAnnotationFeature pAnnoFeature = fe as IAnnotationFeature;

                 ITextElement pTextElement = new TextElementClass();
                 ITextSymbol pTextSymbol = ((pAnnoFeature.Annotation as ITextElement).Symbol as IClone).Clone() as ITextSymbol;
                 pTextElement.Symbol = pTextSymbol;
                 pTextElement.Text = (pAnnoFeature.Annotation as ITextElement).Text;
                 pTextElement.ScaleText = (pAnnoFeature.Annotation as ITextElement).ScaleText;

                 ITransform2D ptrans = pAnnoFeature.Annotation.Geometry as ITransform2D;
                 ptrans.Move(dx, dy);
                 IElement pElement = pTextElement as IElement;
                 pElement.Geometry = ptrans as IGeometry;
                 pAnnoFeature.Annotation = pElement;
                 fe.Store();
             }


             ILayer polygonLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOLY"))).FirstOrDefault();
             IFeatureClass polygonFcl = (polygonLayer as IFeatureLayer).FeatureClass;
             qf.WhereClause = "TYPE like '" + filter + "%'";
             cursor = polygonFcl.Search(qf, false);
             while ((fe = cursor.NextFeature()) != null)
             {
                 ITransform2D ptrans = fe.ShapeCopy as ITransform2D;
                 ptrans.Move(dx, dy);
                 fe.Shape = ptrans as IGeometry;
                 fe.Store();
             }
             Marshal.ReleaseComObject(cursor);
         }
         #endregion
        private IPolygon LengendLineGeoLeftUp()
        {
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize +5) / 1000;
            orginPoint.Y += dy;
           
            lengendHeight += repItemYStep / 2.83 / 2 + TitleFontSize +5;
            orginPoint.X -= mapScale * (3) / 1000;
            IPointCollection pRing = new RingClass();
            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y - (lengendHeight + 2) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - (lengendHeight + 2) * mapScale / 1000 });
     
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
             PolygonElementClass ele=new PolygonElementClass();
            ele.Geometry=geoCol as IPolygon;
            (GApplication.Application.ActiveView as IGraphicsContainer).AddElement(ele,0);
            return geoCol as IPolygon;
        }
        private IPolygon LengendLineGeoLeftDown()
        {
           
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize +3.5) / 1000;
            orginPoint.Y += dy;
            lengendHeight += repItemYStep / 2.83 / 2 + TitleFontSize +3.5;

            orginPoint.X -= mapScale * (3) / 1000;
            //外环
            IPointCollection pRing = new RingClass();
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + 2 * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y + 2 * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y - (lengendHeight) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - (lengendHeight) * mapScale / 1000 });

            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            double movey = lengendHeight * mapScale / 1000 - dy;
            (geoCol as ITransform2D).Move(0, movey);
            PolygonElementClass ele=new PolygonElementClass();
            ele.Geometry=geoCol as IPolygon;
            (GApplication.Application.ActiveView as IGraphicsContainer).AddElement(ele,0);
            return geoCol as IPolygon;
        }
        private IPolygon LengendLineGeometryRightUp()
        {
           IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize +5) / 1000;
            orginPoint.Y += dy;
            lengendHeight += repItemYStep / 2.83 / 2 + TitleFontSize +5;
            orginPoint.X -= mapScale * (4) / 1000;
            //
            IPointCollection pRing = new RingClass();
            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y - (lengendHeight + 2) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - (lengendHeight + 2) * mapScale / 1000 });

            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
             PolygonElementClass ele=new PolygonElementClass();
            ele.Geometry=geoCol as IPolygon;
            (GApplication.Application.ActiveView as IGraphicsContainer).AddElement(ele,0);
            return geoCol as IPolygon;
        }
        private IPolygon LengendLineGeoRightDown()
        {
            
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize +5) / 1000;
            orginPoint.Y += dy;
            lengendHeight += repItemYStep / 2.83 / 2 + TitleFontSize +5;
            orginPoint.X -= mapScale * (4) / 1000;
            //
            IPointCollection pRing = new RingClass();
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + 2 * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y + 2 * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y - (lengendHeight) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - (lengendHeight) * mapScale / 1000 });

            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            double movey = lengendHeight * mapScale / 1000 - dy;
            (geoCol as ITransform2D).Move(0, movey);
            PolygonElementClass ele=new PolygonElementClass();
            ele.Geometry=geoCol as IPolygon;
            (GApplication.Application.ActiveView as IGraphicsContainer).AddElement(ele,0);
         

            return geoCol as IPolygon;
        }


        private void CreateLengendLineLeftUp()
        {
             
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LPOLY")).FirstOrDefault();

            string mdbpath = m_Application.Template.Root + @"\整饰\整饰规则库.mdb";
         
            var ruleDtCOM = Helper.ReadToDataTable(mdbpath, "COMMON");//通用RuleID
            DataRow[] drs1 = ruleDtCOM.Select();
            var ruleIDPolygon = new Dictionary<string, int>();
            foreach (DataRow dr in drs1)//COMPASS
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDPolygon[keyname] = val;//!!
            }
            IFeatureClass polygonfcl = (lyrs as IFeatureLayer).FeatureClass;
            int ruleID = ruleIDPolygon["图廓"];
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize +5) / 1000;
            orginPoint.Y += dy;
           
            lengendHeight += repItemYStep / 2.83 / 2 + TitleFontSize +5;
            orginPoint.X -= mapScale * (3) / 1000;
            IPointCollection pRing = new RingClass();
            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y - (lengendHeight + 2) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - (lengendHeight + 2) * mapScale / 1000 });
     
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            
            IFeatureCursor editcursor = polygonfcl.Insert(true);
            IFeatureBuffer featurebuffer = null;
            if (LgLocation == "IN" && lgSizeType)
            {
                featurebuffer = polygonfcl.CreateFeatureBuffer();
                featurebuffer.Shape = geoCol as IGeometry;
                featurebuffer.set_Value(polygonfcl.FindField("RuleID"), ruleID);
                featurebuffer.set_Value(polygonfcl.FindField("TYPE"), "图例外边线");
                editcursor.InsertFeature(featurebuffer);
             }
            //内边线
            geoCol = new PolygonClass();
            _missing = Type.Missing;
            //orginPoint.X -= mapScale * (2.0) / 1000;
            
            pRing = new RingClass();

            pRing.AddPoint(orginPoint);
    
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y - lengendHeight * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - lengendHeight * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
           
            
            featurebuffer = polygonfcl.CreateFeatureBuffer();
            featurebuffer.Shape = geoCol as IGeometry;
            featurebuffer.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            featurebuffer.set_Value(polygonfcl.FindField("TYPE"), "图例内边线");
            editcursor.InsertFeature(featurebuffer);
            editcursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(editcursor);
        
        }
        private void CreateLengendLineRightUp()
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LPOLY")).FirstOrDefault();

            string mdbpath = m_Application.Template.Root + @"\整饰\整饰规则库.mdb";

            var ruleDtCOM = Helper.ReadToDataTable(mdbpath, "COMMON");//通用RuleID
            DataRow[] drs1 = ruleDtCOM.Select();
            var ruleIDPolygon = new Dictionary<string, int>();
            foreach (DataRow dr in drs1)//COMPASS
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDPolygon[keyname] = val;//!!
            }
            IFeatureClass polygonfcl = (lyrs as IFeatureLayer).FeatureClass;
            int ruleID = ruleIDPolygon["图廓"];
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize +5) / 1000;
            orginPoint.Y += dy;
            lengendHeight += repItemYStep / 2.83 / 2 + TitleFontSize +5;
            orginPoint.X -= mapScale * (4) / 1000;
            //
            IPointCollection pRing = new RingClass();
            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y - (lengendHeight + 2) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - (lengendHeight + 2) * mapScale / 1000 });

            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            IFeature fe = null;
            if (LgLocation == "IN" && lgSizeType)
            {
                fe = polygonfcl.CreateFeature();

                fe.Shape = geoCol as IGeometry;

                fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
                fe.set_Value(polygonfcl.FindField("TYPE"), "图例外边线");
                fe.Store();
            }
            //内边线
            geoCol = new PolygonClass();
            _missing = Type.Missing;
            orginPoint.X += mapScale * (2.0) / 1000;

            pRing = new RingClass();

            pRing.AddPoint(orginPoint);

            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y - lengendHeight * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - lengendHeight * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            fe = polygonfcl.CreateFeature();
            //(geoCol as ITransform2D).Move(lengendWidth * mapScale / 1000, 0);
            fe.Shape = geoCol as IGeometry;
            fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            fe.set_Value(polygonfcl.FindField("TYPE"), "图例内边线");
            fe.Store();
        }
        private void CreateLengendLineRightDown(int feID)
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LPOLY")).FirstOrDefault();

            string mdbpath = m_Application.Template.Root + @"\整饰\整饰规则库.mdb";

            var ruleDtCOM = Helper.ReadToDataTable(mdbpath, "COMMON");//通用RuleID
            DataRow[] drs1 = ruleDtCOM.Select();
            var ruleIDPolygon = new Dictionary<string, int>();
            foreach (DataRow dr in drs1)//COMPASS
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDPolygon[keyname] = val;//!!
            }
            IFeatureClass polygonfcl = (lyrs as IFeatureLayer).FeatureClass;
            int ruleID = ruleIDPolygon["图廓"];
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            double dy = mapScale * (repItemYStep / 2.83 / 2 + TitleFontSize +5) / 1000;
            orginPoint.Y += dy;
            lengendHeight += repItemYStep / 2.83 / 2 + TitleFontSize +5;
            orginPoint.X -= mapScale * (4) / 1000;
            //
            IPointCollection pRing = new RingClass();
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + 2 * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y + 2 * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth + 2) * mapScale / 1000, Y = orginPoint.Y - (lengendHeight) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - (lengendHeight) * mapScale / 1000 });

            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            IFeature fe = null;
            if (LgLocation == "IN" && lgSizeType)
            {
                fe = polygonfcl.CreateFeature();
                fe.Shape = geoCol as IGeometry;
                fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
                fe.set_Value(polygonfcl.FindField("TYPE"), "图例外边线");
                fe.Store();
            }
            //内边线
            geoCol = new PolygonClass();
            _missing = Type.Missing;
            orginPoint.X += mapScale * (2.0) / 1000;

            pRing = new RingClass();

            pRing.AddPoint(orginPoint);

            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y - lengendHeight * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y - lengendHeight * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            fe = polygonfcl.CreateFeature();
            //(geoCol as ITransform2D).Move(lengendWidth * mapScale / 1000, 0);
            fe.Shape = geoCol as IGeometry;
            fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            fe.set_Value(polygonfcl.FindField("TYPE"), "图例内边线");
            fe.Store();
            LgMove(fclPoint.GetFeature(feID), lengendHeight * mapScale / 1000 - dy);
        }
        //将临时图层符号转换为IRule
        private Dictionary<int, IRepresentationRule> LoadTempLayer(IFeatureLayer pGeoLayer)
        {
            var rulesDic = new Dictionary<int, IRepresentationRule>();
            {
                #region
                IFeatureRenderer feRenderer = (pGeoLayer as IGeoFeatureLayer).Renderer;
                if (feRenderer is ISimpleRenderer)
                {
                    ISymbol symbol = (feRenderer as ISimpleRenderer).Symbol;
                    string lb = (feRenderer as ISimpleRenderer).Label;
                    RepresentationRuleClass ruleClass = new RepresentationRuleClass();
                    ruleClass.InitWithSymbol(symbol);
                    rulesDic.Add(-1, ruleClass);
                }
                if (feRenderer is IUniqueValueRenderer)
                {
                    var ur = feRenderer as IUniqueValueRenderer;
                    for (int j = 0; j <= ur.ValueCount - 1; j++)
                    {
                        string xv;
                        xv = ur.get_Value(j);
                        ISymbol symbol = ur.get_Symbol(xv);
                        string lb = ur.get_Label(xv);
                        RepresentationRuleClass ruleClass = new RepresentationRuleClass();
                        ruleClass.InitWithSymbol(symbol);
                        rulesDic.Add(-(1 + j), ruleClass);
                         
                    }

                }
                if (feRenderer is IClassBreaksRenderer)
                {
                    var cb = feRenderer as IClassBreaksRenderer;
                    for (int i = 0; i < cb.BreakCount; i++)
                    {

                        ISymbol symbol = cb.get_Symbol(i);
                        string lb = cb.get_Label(i);
                        RepresentationRuleClass ruleClass = new RepresentationRuleClass();
                        ruleClass.InitWithSymbol(symbol);
                        rulesDic.Add(-(1 + i), ruleClass);
                    }

                }
                #endregion
            }
            return rulesDic;

        }
        // 图层要素对应的制图表达
        private Dictionary<int, IRepresentationRule> UniqueValueRepRender(IFeatureLayer pFeatureLayer, List<int> list)
        {
            var rulesDic=new Dictionary<int,IRepresentationRule>();
            try
            {
                IGeoFeatureLayer pGeoLayer = pFeatureLayer as IGeoFeatureLayer;
               

              
                //获取要素对应的制图表达
                IFeatureRenderer feRenderer = ((IGeoFeatureLayer)pFeatureLayer).Renderer;
                if (!(feRenderer is IRepresentationRenderer))
                {
                   
                    if (list[0] < 0)//临时图层
                    {
                        rulesDic = LoadTempLayer(pFeatureLayer);
                    }
                    return rulesDic;
                   
                }
                IRepresentationRenderer representationRenderer = (IRepresentationRenderer)feRenderer;
                IRepresentationClass repClass = representationRenderer.RepresentationClass;

                var rules = repClass.RepresentationRules;
                //唯一值
                foreach (var id in list)
                {
                    rulesDic[id] = rules.get_Rule(id);
                    if (pFeatureLayer.Name.ToUpper() == "LRDL")//道路图层，圆头变平头
                    {
                        var ruleOrg = (rules.get_Rule(id) as IClone).Clone() as IRepresentationRule;
                        for (int i = 0; i < ruleOrg.LayerCount; i++)
                        {
                            IBasicLineSymbol lineSym = ruleOrg.get_Layer(i) as IBasicLineSymbol;
                            if (lineSym == null)
                                continue;

                            //IGeometricEffects effects = ruleOrg.get_Layer(i) as IGeometricEffects;
                            IGraphicAttributes ga = lineSym.Stroke as IGraphicAttributes;
                            if (ga != null)
                            {
                                int gapsid = ga.get_IDByName("Caps");
                                ga.set_Value(gapsid, esriLineCapStyle.esriLCSButt);
                                //rep.set_Value(ga, id, esriLineCapStyle.esriLCSButt);
                            }
                        }
                        rulesDic[id] = ruleOrg;
                    }
                }
                return rulesDic;
                //支持可以排序
                //var dic = rulesDic.OrderBy(t => t.Key).ToDictionary(p => p.Key, o => o.Value);
                //return dic;
            }
            catch
            {
                return rulesDic;
            }
        }

        Dictionary<int, double> dyDics = new Dictionary<int, double>();//每列的平移量
      
        List<lengendItemInfo> itemsInfo = new List<lengendItemInfo>();//图例每列的信息
        Dictionary<int, bool> multiItemDic = new Dictionary<int, bool>();//处理过的共享图例项
        Dictionary<string, List<int>> groupItemDic = new Dictionary<string, List<int>>();//处理过的组合图例

      
        public   double repItemXStep = 71;//图例项间隔
        public   double repItemYStep = 10;//图例项间隔
        public   double repAnnoStep = 30;//图例说明间隔
        public   double ScaleBarStep = 11;//比例尺与图例间隔
        public double TitleFontSize = 10;
        public double ItemFontSize = 2.3;

        private List<ItemInfo> lengendItems = new List<ItemInfo>();//所有的图例项目信息：重新排列

        //将图例项插入【线要素】地图视图
        private void InsertRuleLendgendPolyLine(IFeatureLayer pFeatureLayer, IRepresentationGraphics pGraphics, Dictionary<int, IRepresentationRule> rulesDic, int feID, KeyValuePair<ILayer, Dictionary<int, string>> kv_ = new KeyValuePair<ILayer ,Dictionary<int,string>>())
        {
            try
            {
                string fclName = pFeatureLayer.FeatureClass.AliasName;
                var rp = (pFeatureLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
                IRepresentationClass m_RepClass = null;
                IRepresentationRules rules = null;
                if (rp != null)
                {
                    m_RepClass = rp.RepresentationClass;
                    rules = m_RepClass.RepresentationRules;
                }
                //IRepresentationClass m_RepClass = rp.RepresentationClass;
                //var rules = m_RepClass.RepresentationRules;

                IPointCollection polyline = new PolylineClass();
                polyline.AddPoint(new PointClass { X = 0, Y = 0 });
                polyline.AddPoint(new PointClass { X = lgItemLen, Y = 0 });
                //水系为共享图例
                #region

                var rowsmt = multiDataTable.Select("FclName='" + fclName.ToUpper() + "'");
                //同一个图层多个组合

                Dictionary<string, string> mtIDs = new Dictionary<string, string>();//ids-type
                Dictionary<string, List<string>> mtItemTxtDic = new Dictionary<string, List<string>>();
                Dictionary<string, int> shareIDs = new Dictionary<string, int>();//ids-type
                Dictionary<int, bool> drawIDs = new Dictionary<int, bool>();//shareID->draw 
              
                if (rowsmt.Length > 0)
                {
                    foreach (var dr in rowsmt)
                    {   
                        bool DrawLg = false;
                        DrawLg = bool.Parse(dr["Draw"].ToString());
                        string str = dr["RuleID"].ToString();
                       
                        string type = dr["Type"].ToString();
                        mtIDs[str] = type;
                        shareIDs[str] = int.Parse(dr["ShareRuleID"].ToString());
                        drawIDs[int.Parse(dr["ShareRuleID"].ToString())] = DrawLg;
                        string[] strs = str.Split(new char[] { ',' });
                        str = dr["ShareItemName"].ToString();
                        strs = str.Split(new char[] { ';' });
                        List<string> list = new List<string>();
                        foreach (var s in strs)
                        {
                            list.Add(s);
                        }
                        mtItemTxtDic[dr["RuleID"].ToString()] = list;
                    }
                }
                #endregion
                // 
                foreach (var kv in rulesDic)
                {
                    var iteminfo = new ItemInfo();

                    iteminfo.RuleID = kv.Key;
                    iteminfo.LyrName = fclName;
                    var lgMt = mtIDs.Where(t => (t.Key.Split(new char[] { ',' })).Contains(kv.Key.ToString())).FirstOrDefault();
                    if (lgMt.Key != null && (MapLgType == lgMt.Value||lgMt.Value=="通用"))//共享图例处理
                    {  
                        #region
                        var mtItemTxt = new List<string>();
                        mtItemTxt = mtItemTxtDic[lgMt.Key];
                        int repID = shareIDs[lgMt.Key];
                        if (!multiItemDic[repID])
                        {

                            IPoint anchor = new PointClass { X =  lgItemLen/2, Y = 0 };
                            if (drawIDs.ContainsKey(repID) && drawIDs[repID])//重新绘制
                            {
                                var shareRule = (multiRulesDic[repID] as IClone).Clone() as IRepresentationRule;
                                var marker = (shareRule.get_Layer(0) as IGraphicAttributes).get_Value(1) as IRepresentationMarker;
                                if (marker != null)
                                {
                                    IRepresentationGraphics repGraphics = marker as IRepresentationGraphics;
                                    repGraphics.Reset();
                                    IGeometry geo0;
                                    IRepresentationRule rule0;
                                    while (true)
                                    {
                                       
                                        repGraphics.Next(out geo0, out rule0);
                                        if (rule0 == null || geo0 == null) break;
                                        var geoinfo = new GeoRuleInfo();
                                        ITransform2D geoTrans=geo0 as ITransform2D;
                                        geoTrans.Move(25.5,0);
                                        geoinfo.Geometry = geo0;
                                        geoinfo.Rule = rule0;
                                        iteminfo.GeoRules.Add(geoinfo);
                                    }
                                    iteminfo.multTxts = mtItemTxt;
                                }
                            }
                            else
                            {
                                var geoinfo = new GeoRuleInfo();
                                geoinfo.Geometry = anchor;
                                geoinfo.Rule = multiRulesDic[repID];
                                iteminfo.GeoRules.Add(geoinfo);
                                iteminfo.multTxts = mtItemTxt;
                            }
                            multiItemDic[repID] = true;


                        }
                    #endregion
                    }
                    else //其他图例
                    {
                        var drs = compositeDataTable.Select("FclName='" + fclName.ToUpper() + "' and RuleID='" + kv.Key + "'");

                        //长lgItemLenpt，间隔5pt
                        IPolyline repGeo = (polyline as IClone).Clone() as IPolyline;
                        if (drs.Length != 0)//添加组合图元：是否有位置偏移
                        {
                            double offset = double.Parse(drs[0]["OffsetY"].ToString());
                            offset = 0.01 * offset * repItemYStep;
                            if (offset != 0)
                            {
                                (repGeo as ITransform2D).Move(0, offset);
                            }
                        }
                        //add 3
                        var geoinfo = new GeoRuleInfo();
                        geoinfo.Geometry = repGeo as IPolyline;
                        geoinfo.Rule = kv.Value;
                        #region 图例道路两端平头处理
                        if (fclName == "LRDL")
                        {
                            IRepresentationRule repRule = (kv.Value as IClone).Clone() as IRepresentationRule;
                            for (int i = 0; i < repRule.LayerCount; i++)
                            {
                                if (repRule.get_Layer(i) is IBasicLineSymbol)
                                {
                                    IBasicLineSymbol basicLineSymbol = repRule.get_Layer(i) as IBasicLineSymbol;
                                    if (basicLineSymbol == null)
                                    {
                                        continue;
                                    }
                                    IGraphicAttributes LineAttributes = basicLineSymbol.Stroke as IGraphicAttributes;
                                    int id = LineAttributes.get_IDByName("Caps");
                                    LineAttributes.set_Value(id, esriLineCapStyle.esriLCSButt);
                                }
                            }
                            geoinfo.Rule = repRule;
                        }
                        #endregion
                        iteminfo.GeoRules.Add(geoinfo);
                        string groupItemText = "";
                        if (drs.Length != 0)//添加组合图元  
                        {

                            groupItemText = InsertGroupItem(drs, 0, 0, pGraphics, iteminfo);

                        }
                        var grouplist = new List<int>();//排除已添加的组合图例
                        if (groupItemDic.ContainsKey(fclName.ToUpper()))
                            grouplist = groupItemDic[fclName.ToUpper()];
                        if (grouplist.Contains(kv.Key))
                            continue;
                        //add
                        if (rules != null)
                        {
                            string rulename = rules.get_Name(kv.Key);
                            rulename = System.Text.RegularExpressions.Regex.Replace(rulename, @"\d", "");
                            rulename = rulename.Trim();
                            if (rulename != "")
                            {
                                iteminfo.ItemText = rulename + groupItemText;
                                if(groupItemText!="")
                                    iteminfo.ItemText =groupItemText;
                            }
                            else
                            {
                                iteminfo.ItemText = rules.get_Name(kv.Key) + groupItemText;
                                if (groupItemText != "")
                                    iteminfo.ItemText = groupItemText;
                            }
                        }
                        else
                        {
                            iteminfo.ItemText = kv_.Value[kv.Key];
                        }
                        //添加图例文字:间隔33

                    }
                    //add 5
                    if (iteminfo.GeoRules.Count > 0)
                    {
                        lengendItems.Add(iteminfo);
                    }
                }
                Marshal.ReleaseComObject(polyline);
                GC.Collect();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }
        //将图例项插入【点要素】地图视图
        private void InsertRuleLendgendPoint(IFeatureLayer pFeatureLayer, IRepresentationGraphics pGraphics, Dictionary<int, IRepresentationRule> rulesDic, int feID,KeyValuePair <ILayer ,Dictionary<int,string>> kv_=new KeyValuePair<ILayer ,Dictionary<int,string>>())
        {

            string fclName = pFeatureLayer.FeatureClass.AliasName;
            var rp = (pFeatureLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = null;
            IRepresentationRules rules = null;
            if (rp != null)
            {
                m_RepClass = rp.RepresentationClass;
                rules = m_RepClass.RepresentationRules;
            }
            //IRepresentationClass m_RepClass = rp.RepresentationClass;
            //var rules = m_RepClass.RepresentationRules;
            //水系为共享图例
            #region

            var rowsmt = multiDataTable.Select("FclName='" + fclName.ToUpper() + "'");
            Dictionary<int, string> mtIDs = new Dictionary<int, string>();//id-type
            List<string> mtItemTxt = new List<string>();
            if (rowsmt.Length > 0)
            {
                string str = rowsmt[0]["RuleID"].ToString();
                string type = rowsmt[0]["Type"].ToString();
                string[] strs = str.Split(new char[] { ',' });
                foreach (var s in strs)
                {
                    mtIDs.Add(int.Parse(s),type);
                }
                str = rowsmt[0]["ShareItemName"].ToString();
                strs = str.Split(new char[] { ';' });
                foreach (var s in strs)
                {
                    mtItemTxt.Add(s);
                }

            }
            #endregion
            // 
            foreach (var kv in rulesDic)
            {
                //add
                var iteminfo = new ItemInfo();
                iteminfo.LyrName = fclName;
                iteminfo.RuleID = kv.Key;
                if (mtIDs.ContainsKey(kv.Key))//共享图例处理
                {
                    if (mtIDs[kv.Key] != MapLgType && mtIDs[kv.Key]!="通用")
                    {
                        continue;
                    }
                    int repID = int.Parse(rowsmt[0]["ShareRuleID"].ToString());
                    if (!multiItemDic[repID])
                    {
                        //add 2
                        var geoinfo = new GeoRuleInfo();
                        geoinfo.Geometry = new PointClass { X = lgItemLen / 2, Y = 0 };
                        geoinfo.Rule = multiRulesDic[repID];
                        iteminfo.GeoRules.Add(geoinfo);
                        iteminfo.multTxts = mtItemTxt;
                        multiItemDic[repID] = true;
                       
                    }
                }
                else //其他图例
                {
                    var drs = compositeDataTable.Select("FclName='" + fclName.ToUpper() + "' and RuleID='" + kv.Key+"'");
                    var repGeo = new PointClass { X = lgItemLen*0.5, Y = 0 };
                    var marker = (kv.Value.get_Layer(0) as IGraphicAttributes).get_Value(1) as IRepresentationMarker;
                    if (marker.Height == 0 || marker.Width == 0)
                        continue;
                    var geooutline = marker.get_Outline(esriOutlineType.esriOutlineBox);
                    repGeo.Y -= (geooutline.Envelope.YMax + geooutline.Envelope.YMin) / 2;
                    string groupItemText = "";
                    if (drs.Length != 0)//添加组合图元
                    {
                        double groupItem = lgItemLen / (drs.Length + 1);
                        int groupFlag = 1;
                        foreach (var dr in drs)
                        {
                            #region
                            groupItemText=dr["LgNote"].ToString();
                            double locationX=double.Parse(dr["Location"].ToString());
                            double locationY = double.Parse(dr["LocationY"].ToString());
                            IPoint anchor = new PointClass { X = locationX * 0.01 * lgItemLen, Y = locationY *0.01*repItemYStep};
                            int repID = int.Parse(dr["GroupRuleID"].ToString());
                            string groupLyr = dr["GroupLayer"].ToString().Trim();
                            if (groupLyr == "")
                            {
                                // add5
                                var geoinfo = new GeoRuleInfo();
                                geoinfo.Geometry = anchor;
                                geoinfo.Rule = groupRulesDic[repID];
                                iteminfo.GeoRules.Add(geoinfo);
                            }
                            else//已有的
                            {
                                var list = new List<int>();
                                if (groupItemDic.ContainsKey(groupLyr))
                                {
                                    list = groupItemDic[groupLyr];
                                }
                                if (!list.Contains(repID))
                                {
                                    var rule = GetGroupLgRule(dr["GroupLayer"].ToString(), repID);
                                    // add4
                                    var geoinfo = new GeoRuleInfo();
                                   // geoinfo.Geometry = new PointClass { X = groupItem * groupFlag, Y = 0 };
                                    geoinfo.Geometry = new PointClass { X = locationX * 0.01 * lgItemLen, Y = locationY * 0.01 * repItemYStep };
                                    geoinfo.Rule = rule;
                                    iteminfo.GeoRules.Add(geoinfo);
                                    list.Add(repID);
                                    groupItemDic[groupLyr] = list;
                                }
                            }
                            groupFlag++;
                            #endregion
                        }
                    }
                    var grouplist = new List<int>();//排除已添加的组合图例
                    if (groupItemDic.ContainsKey(fclName.ToUpper()))
                        grouplist = groupItemDic[fclName.ToUpper()];
                    if (grouplist.Contains(kv.Key))
                        continue;
                     
                    //add 3
                    {
                        var geoinfo = new GeoRuleInfo();
                        geoinfo.Geometry = repGeo;
                        geoinfo.Rule = kv.Value;
                        iteminfo.GeoRules.Add(geoinfo);
                    }
                    if (rules != null)
                    {
                        string rulename = rules.get_Name(kv.Key);
                        rulename = System.Text.RegularExpressions.Regex.Replace(rulename, @"\d", "");
                        rulename = rulename.Trim();
                        if (rulename != "")
                        {
                            iteminfo.ItemText = rulename;
                        }
                        else
                        {
                            iteminfo.ItemText = rules.get_Name(kv.Key);
                        }
                    }
                    else
                    {
                        iteminfo.ItemText = kv_.Value[kv.Key];
                    }
                    //组合处理
                    if(groupItemText!="")
                        iteminfo.ItemText = groupItemText;
                    //iteminfo.ItemText = rules.get_Name(kv.Key);

                } 
                //add 5
                if (iteminfo.GeoRules.Count > 0)
                {
                    lengendItems.Add(iteminfo);
                }


            }
        }
        //将图例项插入【面要素】地图视图
        private void InsertRuleLendgendPolygon(IFeatureLayer pFeatureLayer, IRepresentationGraphics pGraphics, Dictionary<int, IRepresentationRule> rulesDic, int feID,KeyValuePair <ILayer ,Dictionary<int,string>> kv_=new KeyValuePair<ILayer ,Dictionary<int,string>>())
        {

            string fclName = pFeatureLayer.FeatureClass.AliasName;
            var rp = (pFeatureLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = null;
            IRepresentationRules rules = null;
            if (rp != null)
            {
                m_RepClass = rp.RepresentationClass;
                rules = m_RepClass.RepresentationRules;
            }
           
            //水系为共享图例
            #region

            var rowsmt = multiDataTable.Select("FclName='" + fclName.ToUpper() + "'");
            Dictionary<int, int> shareIDDic = new Dictionary<int, int>();//ruleID->shareID
            Dictionary<int, string> mtIDs = new Dictionary<int, string>();//ruleID-type
            Dictionary<int, List<string>> mtItemTxt = new Dictionary<int, List<string>>();//shareID->txt
          
            for(int i=0;i<rowsmt.Length;i++)
            {
                string str = rowsmt[i]["RuleID"].ToString();
                string type = rowsmt[i]["Type"].ToString();
                string[] strs = str.Split(new char[] { ',' });
                foreach (var s in strs)
                {
                    mtIDs.Add(int.Parse(s),type);
                    shareIDDic[int.Parse(s)] = int.Parse(rowsmt[i]["ShareRuleID"].ToString());
                }
                str = rowsmt[i]["ShareItemName"].ToString();
                strs = str.Split(new char[] { ';' });
                var list = new List<string>();
                foreach (var s in strs)
                {
                    list.Add(s);
                   
                }
                mtItemTxt[int.Parse(rowsmt[i]["ShareRuleID"].ToString())] = list;

            }
            #endregion
            // 
            foreach (var kv in rulesDic)
            {
                var iteminfo = new ItemInfo();
                iteminfo.RuleID = kv.Key;
                iteminfo.LyrName = fclName;
                if (mtIDs.ContainsKey(kv.Key))//共享图例处理
                {
                    if (mtIDs[kv.Key] != MapLgType && mtIDs[kv.Key] != "通用")
                    {
                        continue;
                    }
                  //  int repID = int.Parse(rowsmt[0]["ShareRuleID"].ToString());
                    int repID = shareIDDic[kv.Key];
                    if (!multiItemDic[repID])
                    {
                        IPoint anchor = new PointClass { X = lgItemLen/2, Y = 0 };
                        //add 2
                        var geoinfo = new GeoRuleInfo();
                        geoinfo.Geometry = anchor;
                        geoinfo.Rule = multiRulesDic[repID];
                        iteminfo.GeoRules.Add(geoinfo);
                        iteminfo.multTxts = mtItemTxt[repID];
                        //添加文字
                        multiItemDic[repID] = true;
                       
                    }
                }
                else //其他图例
                {
                    var drs = compositeDataTable.Select("FclName='" + fclName.ToUpper() + "' and RuleID='" + kv.Key + "'");


                    IPointCollection repGeo = new PolygonClass();
                    double stepx = (lgItemLen - lgItemLenA) / 2;
                    double widthy = lgItemWidthA / 2;
                    repGeo.AddPoint(new PointClass { X = stepx, Y = widthy });
                    repGeo.AddPoint(new PointClass { X = lgItemLenA + stepx, Y = widthy });
                    repGeo.AddPoint(new PointClass { X = lgItemLenA + stepx, Y = -widthy });
                    repGeo.AddPoint(new PointClass { X = stepx, Y = -widthy });
                    repGeo.AddPoint(new PointClass { X = stepx, Y = widthy });
                   
                    //add 3
                    var geoinfo = new GeoRuleInfo();
                    geoinfo.Geometry = repGeo as IPolygon;
                    geoinfo.Rule = kv.Value;
                    iteminfo.GeoRules.Add(geoinfo);

                    if (drs.Length != 0)//添加组合图元
                    {
                        double groupItem = lgItemLen / (drs.Length + 1);
                        int groupFlag = 1;
                        foreach (var dr in drs)
                        {
                            #region

                            IPoint anchor = new PointClass { X = groupItem * groupFlag, Y = 0 };
                           
                            int repID = int.Parse(dr["GroupRuleID"].ToString());
                            string groupLyr = dr["GroupLayer"].ToString().Trim();
                            if (groupLyr == "")
                            {
                                // add 5
                                var geoinfo1 = new GeoRuleInfo();
                                geoinfo1.Geometry = anchor;
                                geoinfo1.Rule = groupRulesDic[repID];
                                iteminfo.GeoRules.Add(geoinfo1);
                            }
                            else//已有的
                            {
                                var list = new List<int>();
                                if (groupItemDic.ContainsKey(groupLyr))
                                {
                                    list = groupItemDic[groupLyr];
                                }
                                if (!list.Contains(repID))
                                {
                                    var rule = GetGroupLgRule(dr["GroupLayer"].ToString(), repID);
                                    // add4
                                    var geoinfo1 = new GeoRuleInfo();
                                    geoinfo1.Geometry = anchor;
                                    geoinfo1.Rule = rule;
                                    iteminfo.GeoRules.Add(geoinfo1);
                                    list.Add(repID);
                                    groupItemDic[groupLyr] = list;
                                }
                            }
                            groupFlag++;
                            #endregion
                        }
                    }
                    // add 4
                   // iteminfo.ItemText = rules.get_Name(kv.Key);
                    if (rules != null)
                    {
                        string rulename = rules.get_Name(kv.Key);
                        rulename = System.Text.RegularExpressions.Regex.Replace(rulename, @"\d", "");
                        rulename = rulename.Trim();
                        if (rulename != "")
                        {
                            iteminfo.ItemText = rulename;
                        }
                        else
                        {
                            iteminfo.ItemText = rules.get_Name(kv.Key);
                        }
                    }
                    else
                    {
                        iteminfo.ItemText = kv_.Value[kv.Key];
                    }
                }
                //add 5
                if (iteminfo.GeoRules.Count > 0)
                {
                    lengendItems.Add(iteminfo);
                }

            }
        }

       public  double lgItemLen = 51;//线图例的长度
       public double lgItemLenA = 34;//面图例的长度
       public double lgItemWidthA = 14;//面图例的宽度
        //将图例项插入【线要素】模板模式
        private void InsertRuleLendgendPolyLine(IFeatureLayer pFeatureLayer, IRepresentationGraphics pGraphics, Dictionary<int, IRepresentationRule> rulesDic, int feID, Dictionary<int, string> noteDic)
        {  
          
            string fclName = pFeatureLayer.FeatureClass.AliasName;
            var rp = (pFeatureLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            var rules = m_RepClass.RepresentationRules;
            IPointCollection polyline = new PolylineClass();
            polyline.AddPoint(new PointClass { X = 0, Y = 0 });
            polyline.AddPoint(new PointClass { X = lgItemLen, Y = 0 });
            //水系为共享图例
            #region

            var rowsmt = multiDataTable.Select("FclName='" + fclName.ToUpper() + "'");
            //同一个图层多个组合

            Dictionary<string, string> mtIDs = new Dictionary<string, string>();//ids-type
            Dictionary<string, List<string>> mtItemTxtDic = new Dictionary<string, List<string>>();
            Dictionary<string, int> shareIDs = new Dictionary<string, int>();//ids-type
            if (rowsmt.Length > 0)
            {
                foreach (var dr in rowsmt)
                {
                    string str = dr["RuleID"].ToString();
                    string type = dr["Type"].ToString();
                    mtIDs[str] = type;
                    shareIDs[str] = int.Parse(dr["ShareRuleID"].ToString());
                    string[] strs = str.Split(new char[] { ',' });
                    str = dr["ShareItemName"].ToString();
                    strs = str.Split(new char[] { ';' });
                    List<string> list = new List<string>();
                    foreach (var s in strs)
                    {
                        list.Add(s);
                    }
                    mtItemTxtDic[dr["RuleID"].ToString()] = list;
                }
            }
            #endregion
            // 
            foreach (var kv in rulesDic)
            {
                //add
                var iteminfo = new ItemInfo();
                iteminfo.RuleID = kv.Key;
                iteminfo.LyrName = fclName;
                var lgMt = mtIDs.Where(t => (t.Key.Split(new char[] { ',' })).Contains(kv.Key.ToString())).FirstOrDefault();
                if (lgMt.Key != null && (MapLgType == lgMt.Value || lgMt.Value == "通用"))//共享图例处理
                {
                   #region
                    var mtItemTxt = new List<string>();
                    mtItemTxt = mtItemTxtDic[lgMt.Key];
                    //if (lgMt.Value != MapLgType)
                    //{
                    //    continue;
                    //}
                    int repID = shareIDs[lgMt.Key];
                    if (!multiItemDic[repID])
                    {
                        IPoint anchor = new PointClass { X = lgItemLen/2, Y = 0 };
                        //add
                        var geoinfo = new GeoRuleInfo();
                        geoinfo.Geometry = anchor;
                        geoinfo.Rule = multiRulesDic[repID];
                        iteminfo.GeoRules.Add(geoinfo);
                        iteminfo.multTxts = mtItemTxt;
                        multiItemDic[repID] = true;
                      
                    }
                    #endregion
                }
                else //其他图例
                {
                    #region
                    var drs = compositeDataTable.Select("FclName='" + fclName.ToUpper() + "' and RuleID='" + kv.Key + "'");

                    //长lgItemLenpt，间隔5pt
                    IPolyline repGeo = (polyline as IClone).Clone() as IPolyline;
                    if (drs.Length != 0)//添加组合图元：是否有位置偏移
                    {
                        double offset =double.Parse(drs[0]["OffsetY"].ToString());
                        offset = 0.01 * offset * repItemYStep;
                        if (offset != 0)
                        {
                            (repGeo as ITransform2D).Move(0, offset);
                        }
                    }
                    //add 3
                    var geoinfo = new GeoRuleInfo();
                    geoinfo.Geometry = repGeo as IPolyline;
                    geoinfo.Rule = kv.Value;
                    #region 图例道路两端平头处理
                    if (fclName == "LRDL")
                    {
                        IRepresentationRule repRule = (kv.Value as IClone).Clone() as IRepresentationRule;
                        for (int i = 0; i < repRule.LayerCount; i++)
                        {
                            if (repRule.get_Layer(i) is IBasicLineSymbol)
                            {
                                IBasicLineSymbol basicLineSymbol = repRule.get_Layer(i) as IBasicLineSymbol;
                                if (basicLineSymbol == null)
                                {
                                    continue;
                                }
                                IGraphicAttributes LineAttributes = basicLineSymbol.Stroke as IGraphicAttributes;
                                int id = LineAttributes.get_IDByName("Caps");
                                LineAttributes.set_Value(id, esriLineCapStyle.esriLCSButt);
                            }
                        }
                        geoinfo.Rule = repRule;
                    }
                    #endregion
                    iteminfo.GeoRules.Add(geoinfo);
                    string groupItemText = "";
                    if (drs.Length != 0)//添加组合图元
                    {
                        //add
                        groupItemText = InsertGroupItem(drs, 0, 0, pGraphics, iteminfo);
                       
                    }
                   
                    var grouplist = new List<int>();//排除已添加的组合图例
                    if (groupItemDic.ContainsKey(fclName.ToUpper()))
                        grouplist = groupItemDic[fclName.ToUpper()];
                    if (grouplist.Contains(kv.Key))
                        continue;
                    iteminfo.ItemText = noteDic[kv.Key];
                    #endregion
                }
                //add 5
                if (iteminfo.GeoRules.Count > 0)
                {
                    lengendItems.Add(iteminfo);
                }
            }
            Marshal.ReleaseComObject(polyline);
            GC.Collect();
        }
        //将图例项插入【点要素】模板模式
        private void InsertRuleLendgendPoint(IFeatureLayer pFeatureLayer, IRepresentationGraphics pGraphics, Dictionary<int, IRepresentationRule> rulesDic, int feID, Dictionary<int, string> noteDic)
        {  
           
            string fclName = pFeatureLayer.FeatureClass.AliasName;
            var rp = (pFeatureLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            var rules = m_RepClass.RepresentationRules;
            //水系为共享图例
            #region

            var rowsmt = multiDataTable.Select("FclName='" + fclName.ToUpper() + "'");
            List<int> mtIDs = new List<int>();
            List<string> mtItemTxt = new List<string>();
            if (rowsmt.Length > 0)
            {
                string str = rowsmt[0]["RuleID"].ToString();
                string[] strs = str.Split(new char[] { ',' });
                foreach (var s in strs)
                {
                    mtIDs.Add(int.Parse(s));
                }
                str = rowsmt[0]["ShareItemName"].ToString();
                strs = str.Split(new char[] { ';' });
                foreach (var s in strs)
                {
                    mtItemTxt.Add(s);
                }

            }
            #endregion
            // 
            foreach (var kv in rulesDic)
            {
                //add
                var iteminfo = new ItemInfo();
                iteminfo.RuleID = kv.Key;
                iteminfo.LyrName = fclName;
                if (mtIDs.Contains(kv.Key))//共享图例处理
                {
                    #region
                    int repID = int.Parse(rowsmt[0]["ShareRuleID"].ToString());
                    if (!multiItemDic[repID])
                    {
                        
                        IPoint anchor = new PointClass { X = lgItemLen/2, Y = 0 };
                      
                        //add 2
                        var geoinfo = new GeoRuleInfo();
                        geoinfo.Geometry = new PointClass { X = lgItemLen / 2, Y = 0 };
                        geoinfo.Rule = multiRulesDic[repID];
                        iteminfo.GeoRules.Add(geoinfo);
                        iteminfo.multTxts = mtItemTxt;
                        multiItemDic[repID] = true;
                       
                       
                    }
                    #endregion
                }
                else //其他图例
                {
                    #region
                    var drs = compositeDataTable.Select("FclName='" + fclName.ToUpper() + "' and RuleID='" + kv.Key + "'");


                    var repGeo = new PointClass { X = lgItemLen / 2, Y = 0 };
                    var marker = (kv.Value.get_Layer(0) as IGraphicAttributes).get_Value(1) as IRepresentationMarker;
                    if (marker.Height == 0 || marker.Width == 0)
                        continue;
                    var geooutline = marker.get_Outline(esriOutlineType.esriOutlineBox);
                    repGeo.Y -= (geooutline.Envelope.YMax + geooutline.Envelope.YMin) / 2;
                    if (drs.Length != 0)//添加组合图元
                    {
                        double groupItem = lgItemLen / (drs.Length + 1);
                        int groupFlag = 1;
                        foreach (var dr in drs)
                        {
                            #region

                            IPoint anchor = new PointClass { X = groupItem * groupFlag, Y = 0 };
                            int repID = int.Parse(dr["GroupRuleID"].ToString());
                            string groupLyr = dr["GroupLayer"].ToString().Trim();
                            if (groupLyr == "")
                            {
                                // add5
                                var geoinfo = new GeoRuleInfo();
                                geoinfo.Geometry = anchor;
                                geoinfo.Rule = groupRulesDic[repID];
                                iteminfo.GeoRules.Add(geoinfo);
                            }
                            else//已有的
                            {
                                var list = new List<int>();
                                if (groupItemDic.ContainsKey(groupLyr))
                                {
                                    list = groupItemDic[groupLyr];
                                }
                                if (!list.Contains(repID))
                                {
                                    var rule = GetGroupLgRule(dr["GroupLayer"].ToString(), repID);
                                    list.Add(repID);
                                    groupItemDic[groupLyr] = list;
                                    // add4
                                    var geoinfo = new GeoRuleInfo();
                                    geoinfo.Geometry = new PointClass { X = groupItem * groupFlag, Y = 0 };
                                    geoinfo.Rule = rule;
                                    iteminfo.GeoRules.Add(geoinfo);
                                }
                            }
                            groupFlag++;
                            #endregion
                        }
                    }
                    var grouplist = new List<int>();//排除已添加的组合图例
                    if (groupItemDic.ContainsKey(fclName.ToUpper()))
                        grouplist = groupItemDic[fclName.ToUpper()];
                    if (grouplist.Contains(kv.Key))
                        continue;
                    //add 3
                    {
                        var geoinfo = new GeoRuleInfo();
                        geoinfo.Geometry = repGeo;
                        geoinfo.Rule = kv.Value;
                        iteminfo.GeoRules.Add(geoinfo);
                    }
                    //添加图例文字:间隔33
                    // add 4
                    iteminfo.ItemText = noteDic[kv.Key];
                    #endregion
                }
                //add 5
                if (iteminfo.GeoRules.Count > 0)
                {
                    lengendItems.Add(iteminfo);
                }

            }
           
        }
        //将图例项插入【面要素】模板模式
        private void InsertRuleLendgendPolygon(IFeatureLayer pFeatureLayer, IRepresentationGraphics pGraphics, Dictionary<int, IRepresentationRule> rulesDic, int feID, Dictionary<int, string> noteDic)
        {

            string fclName = pFeatureLayer.FeatureClass.AliasName;
            var rp = (pFeatureLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            var rules = m_RepClass.RepresentationRules;
            //共享图例
            #region
            var rowsmt = multiDataTable.Select("FclName='" + fclName.ToUpper() + "'");
            Dictionary<int, int> shareIDDic = new Dictionary<int, int>();//ruleID->shareID
            Dictionary<int, string> mtIDs = new Dictionary<int, string>();//ruleID-type
            Dictionary<int, List<string>> mtItemTxt = new Dictionary<int, List<string>>();//shareID->txt

            for (int i = 0; i < rowsmt.Length; i++)
            {
                string str = rowsmt[i]["RuleID"].ToString();
                string type = rowsmt[i]["Type"].ToString();
                string[] strs = str.Split(new char[] { ',' });
                foreach (var s in strs)
                {
                    mtIDs.Add(int.Parse(s), type);
                    shareIDDic[int.Parse(s)] = int.Parse(rowsmt[i]["ShareRuleID"].ToString());
                }
                str = rowsmt[i]["ShareItemName"].ToString();
                strs = str.Split(new char[] { ';' });
                var list = new List<string>();
                foreach (var s in strs)
                {
                    list.Add(s);

                }
                mtItemTxt[int.Parse(rowsmt[i]["ShareRuleID"].ToString())] = list;

            }
           
            #endregion
            // 
            foreach (var kv in rulesDic)
            {
                //add 1
                var iteminfo = new ItemInfo();
                iteminfo.RuleID = kv.Key;
                iteminfo.LyrName = fclName;
                if (mtIDs.ContainsKey(kv.Key))//共享图例处理
                {
                     if (mtIDs[kv.Key] != MapLgType && mtIDs[kv.Key] != "通用")
                    {
                        continue;
                    }
                    #region
                    int repID = shareIDDic[kv.Key];
                    if (!multiItemDic[repID])
                    {
                       
                        IPoint anchor = new PointClass { X = lgItemLen/2, Y = 0 };
                        //add 2
                        var geoinfo = new GeoRuleInfo();
                        geoinfo.Geometry = anchor;
                        geoinfo.Rule = multiRulesDic[repID];
                        iteminfo.GeoRules.Add(geoinfo);
                        iteminfo.multTxts = mtItemTxt[repID];
                        //添加文字
                        multiItemDic[repID] = true;
                       
                       
                    }
                    #endregion
                }
                else //其他图例
                {
                    #region
                    var drs = compositeDataTable.Select("FclName='" + fclName.ToUpper() + "' and RuleID='" + kv.Key + "'");
                    IPointCollection repGeo = new PolygonClass();
                    double stepx = (lgItemLen - lgItemLenA) / 2;
                    double widthy = lgItemWidthA / 2;
                    repGeo.AddPoint(new PointClass { X = stepx, Y = widthy });
                    repGeo.AddPoint(new PointClass { X = lgItemLenA + stepx, Y = widthy });
                    repGeo.AddPoint(new PointClass { X = lgItemLenA + stepx, Y = -widthy });
                    repGeo.AddPoint(new PointClass { X = stepx, Y = -widthy });
                    repGeo.AddPoint(new PointClass { X = stepx, Y = widthy });
                    //add 3
                    var geoinfo = new GeoRuleInfo();
                    geoinfo.Geometry =repGeo as IPolygon ;
                    geoinfo.Rule = kv.Value;
                    iteminfo.GeoRules.Add(geoinfo);
                    if (drs.Length != 0)//添加组合图元
                    {
                        double groupItem = lgItemLen / (drs.Length + 1);
                        int groupFlag = 1;
                        foreach (var dr in drs)
                        {
                            #region

                            IPoint anchor = new PointClass { X = groupItem * groupFlag, Y = 0 };
                          
                            int repID = int.Parse(dr["GroupRuleID"].ToString());
                            string groupLyr = dr["GroupLayer"].ToString().Trim();
                            if (groupLyr == "")
                            {
                                // add 5
                                var geoinfo1 = new GeoRuleInfo();
                                geoinfo1.Geometry = anchor;
                                geoinfo1.Rule = groupRulesDic[repID];
                                iteminfo.GeoRules.Add(geoinfo1);
                            }
                            else//已有的
                            {
                                var list = new List<int>();
                                if (groupItemDic.ContainsKey(groupLyr))
                                {
                                    list = groupItemDic[groupLyr];
                                }
                                if (!list.Contains(repID))
                                {
                                    var rule = GetGroupLgRule(dr["GroupLayer"].ToString(), repID);
                                    list.Add(repID);
                                    groupItemDic[groupLyr] = list;
                                    // add4
                                    var geoinfo1 = new GeoRuleInfo();
                                    geoinfo1.Geometry = anchor;
                                    geoinfo1.Rule = rule;
                                    iteminfo.GeoRules.Add(geoinfo1);
                                }
                            }
                            groupFlag++;
                            #endregion
                        }
                    }
                    // add 4
                    iteminfo.ItemText = noteDic[kv.Key];
                    #endregion
                }
                //add 5
                if (iteminfo.GeoRules.Count > 0)
                {
                    lengendItems.Add(iteminfo);
                }
            }
        }
       
        //图例项插入【处理注记符号】
        private void InsertLengendAnnoItems(IRepresentationGraphics pGraphics, List<AnnoItemInfo> itemInfos, int feID)
        {
            var temp = ItemFontSize;
         
            foreach (var kv in itemInfos)
            {

                var iteminfo = new ItemInfo();
                iteminfo.LyrName = "ANNO";
                
                //start....处理

                string name = kv.AnnoName;
                double fontsize = kv.FontSize;

 
                //规则
                {
                    GApplication.Application.ActiveView.FocusMap.ReferenceScale = 100000;
                    var repGeo=  AnnoGeometryHelper.AnnoGeometry(name, 2.83 * fontsize,kv.FontName);
                    (repGeo as ITransform2D).Scale(new PointClass { X = 0, Y = 0 }, 0.01, 0.01);
                    //(repGeo as ITransform2D).Move((lgItemLen/2 + dx), -dy);
                    GApplication.Application.ActiveView.FocusMap.ReferenceScale = mapScale;
                    
                    #region
                    
                    //定义rule
                    IRepresentationRule pRule = new RepresentationRule();
                    //定义面
                    IBasicFillSymbol pBasicFill = new BasicFillSymbolClass();
                    IFillPattern pFillPattern = null;
                     IGraphicAttributes fillAttrs = null;                      
                    //单色模式
                    pFillPattern = new SolidColorPattern();
                    fillAttrs = pFillPattern as IGraphicAttributes;
                    fillAttrs.set_Value(0, kv.AnnoColor); //Define color. 
                        
                    pBasicFill.FillPattern = pFillPattern;
                    pRule.InsertLayer(0, pBasicFill as IBasicSymbol);
                    var geoinfo = new GeoRuleInfo();
                    geoinfo.Geometry = repGeo;
                    geoinfo.Rule = pRule;
                    iteminfo.GeoRules.Add(geoinfo);
                    //iteminfo.multTxts = mtItemTxt;
                    //pGraphics.Add(repGeo, pRule);
                    #endregion
                    Marshal.ReleaseComObject(pFillPattern);
                    Marshal.ReleaseComObject(fillAttrs);
                    Marshal.ReleaseComObject(pBasicFill);
                    //Marshal.ReleaseComObject(pRule);
                    GC.Collect();
                }
               

                string annonote = kv.AnnoNote;
                //var txtPoint = (orginPoint as IClone).Clone() as IPoint;
                iteminfo.ItemText = annonote;
                iteminfo.lengendType = lgType.AnnoLg;
                //(txtPoint as ITransform2D).Move((repAnnoStep + dx) * 1.0e-3 * mapScale / 2.83, -dy * 1.0e-3 * mapScale / 2.83);
                //AddLengendTxt(txtPoint, annonote, feID);
                //end....
               // orderlist.Y += repItemYStep;
                if (iteminfo.GeoRules.Count > 0)
                {
                    lengendItems.Add(iteminfo);
                }
            }
        }

        private  IColor ObtainNULLColor()
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
        private IRepresentationRule CreateRoadNumRules(DataRow dr,IRepresentationRule rule)
        {
            ///G2122沪蓉
            IRepresentationRule rulenew = (rule as IClone).Clone() as IRepresentationRule;
            if (!dr.Table.Columns.Contains("RoadNum"))
                return rulenew;
            if (dr["RoadNum"].ToString()==string.Empty)
                return rulenew;
            if (!dr.Table.Columns.Contains("WhereClause"))
                return rulenew;
            string lyr = dr["FclName"].ToString();
            string id = dr["RuleID"].ToString();
            //国、省、县
            string field = dr["RoadNum"].ToString();
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == lyr.ToUpper())).FirstOrDefault();
            string sql = dr["WhereClause"].ToString() + " and " + field + " is not NULL";
            if (dr["WhereClause"].ToString() == "")
            {
                sql =  field + " is not NULL";
            }
            bool GS = false;
            if (dr["LgNote"].ToString().Contains("高速") && field=="RN")
            {
                GS = true;
            }

            if ((lyrs as IFeatureLayer).FeatureClass.FindField(field)==-1)
                return rulenew;
            IFeature fe;
            ISpatialFilter sfFilter=new SpatialFilterClass();
            sfFilter.Geometry=LgEnvelopeIn;
            sfFilter.SpatialRel= esriSpatialRelEnum.esriSpatialRelIntersects;
            sfFilter.WhereClause=sql;
            IFeatureCursor cursor = (lyrs as IFeatureLayer).Search(sfFilter, false);
            string roadNum = string.Empty;
            
            while ((fe = cursor.NextFeature()) != null)
            {
                roadNum = fe.get_Value(fe.Fields.FindField(field)).ToString();
                if (roadNum.StartsWith("G") && GS)
                {
                    break;
                }
                if (roadNum != string.Empty && !GS)
                    break;
            }
            Marshal.ReleaseComObject(cursor);
            if (roadNum == string.Empty)
                return rulenew;
            for (int i = 0; i < rulenew.LayerCount; i++)
            {
                IBasicMarkerSymbol ms = rulenew.Layer[i] as IBasicMarkerSymbol;
                if (ms == null)
                    continue;
                IRepresentationMarker marker = (ms as IGraphicAttributes).get_Value(1) as IRepresentationMarker;
                IRepresentationGraphics graphics = marker as IRepresentationGraphics;
                IRepresentationGraphics gNew = (marker as IClone).Clone() as IRepresentationGraphics;
                gNew.RemoveAll();
                graphics.Reset();
                IGeometry geo;
                var geooutline = marker.get_Outline(esriOutlineType.esriOutlineBox);
                IRepresentationRule r = null;
                IRepresentationRule txtRule = null;
                double txtHeight = 1;
                IGeometry bgGeo = null;
                IRepresentationRule gbRule = null;
                try
                {

                    while (true)
                    {
                        graphics.Next(out geo, out r);
                        if (geo == null || r == null)
                            break;
                        if (r.LayerCount == 1)//说明是道路编码
                        {
                            txtRule = r;
                            txtHeight = geo.Envelope.Height;
                        }
                        else
                        {
                            bgGeo = geo;
                            gbRule = r;

                        }

                    }
                }
                catch
                {
                }
                if (txtRule.get_Layer(0) is IBasicMarkerSymbol)//说明编号是点符号
                {
                    #region
                    var rule0 = (txtRule as IClone).Clone() as IRepresentationRule;
                    var ms0 = rule0.get_Layer(0) as IBasicMarkerSymbol;
                    var marker0 = (ms0 as IGraphicAttributes).get_Value(1) as IRepresentationMarker;
                    var graphics0 = marker0 as IRepresentationGraphics;
                    graphics0.Reset();
                    IGeometry geo0;
                    IRepresentationRule r0 = null;
                    try
                    {

                        while (true)
                        {
                            graphics0.Next(out geo0, out r0);
                            if (geo0 == null || r0 == null)
                                break;
                            txtRule = r0;
                            txtHeight = marker0.Size *marker0.Height/ Math.Max(marker0.Height, marker0.Width);
                        }
                    }
                    catch
                    {
                    }
                    #endregion
                }
                //添加新的编码
                IGeometry geoRoadNum = AnnoGeometryHelper.AnnoGeometry(roadNum, 500, "黑体");
                IPoint ct = new PointClass { X = (geooutline.Envelope.XMin + geooutline.Envelope.XMax) * 0.5, Y = (geooutline.Envelope.YMin + geooutline.Envelope.YMax) * 0.5 };
                IPoint center = new PointClass { X = (geoRoadNum.Envelope.XMin + geoRoadNum.Envelope.XMax) * 0.5, Y = (geoRoadNum.Envelope.YMin + geoRoadNum.Envelope.YMax) * 0.5 };
                (geoRoadNum as ITransform2D).Move(ct.X - center.X, ct.Y - center.Y);
                double sx = txtHeight / geoRoadNum.Envelope.Height;
                (geoRoadNum as ITransform2D).Scale(ct, sx, sx);
                //添加背景
                if (geoRoadNum.Envelope.Width > bgGeo.Envelope.Width)//新的文字过长
                {
                    double sy = 0.95 * bgGeo.Envelope.Width / geoRoadNum.Envelope.Width;//压缩文字长度;
                    (geoRoadNum as ITransform2D).Scale(ct, sy, sy);
                    gNew.Add(bgGeo, gbRule);
                }
                else
                {
                    gNew.Add(bgGeo, gbRule);
                }
                //添加道路数字
                gNew.Add(geoRoadNum, txtRule);
                (ms as IGraphicAttributes).set_Value(1, gNew);
            }

            return rulenew;

        }
        //处理组合图例
        private string InsertGroupItem(DataRow[] drs,double dx,double dy,IRepresentationGraphics pGraphics,ItemInfo itemInfo=null)
        {
            string itemtxt = "";
            foreach (DataRow dr in drs)
            {
                try
                {
                    itemtxt = dr["LgNote"].ToString();
                    //itemtxt.Split(new char[]{'\r','\n'});
                    //判断组合图例是否在图层中存在
                    if (dr.Table.Columns.Contains("WhereClause"))
                    {
                        string lyr = dr["FclName"].ToString();
                        string sql = dr["WhereClause"].ToString();
                        var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == lyr.ToUpper())).FirstOrDefault();
                        ISpatialFilter sf = new SpatialFilterClass();
                        sf.Geometry = LgEnvelopeIn;
                        sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        sf.WhereClause = sql;
                        int ct = (lyrs as IFeatureLayer).FeatureClass.FeatureCount(sf);
                        if (ct == 0)
                        {
                            continue;
                        }
                    }
                    #region
                    double groupItemLocation = double.Parse(dr["Location"].ToString());//百分比0~100
                    groupItemLocation = groupItemLocation * 0.01 * lgItemLen;
                    double locationY = double.Parse(dr["LocationY"].ToString()) * 0.01 * repItemYStep;
                    IPoint anchor = new PointClass { X = groupItemLocation, Y = locationY };

                    int repID = int.Parse(dr["GroupRuleID"].ToString());
                    string groupLyr = dr["GroupLayer"].ToString().Trim();
                    if (groupLyr == "")//如果为空，表示从(图例专家库\Lengend.mdb)中读取组合图例项
                    {
                        var bs = groupRulesDic[repID].get_Layer(0);
                        if (bs is IBasicMarkerSymbol)
                        {
                            var marker = (groupRulesDic[repID].get_Layer(0) as IGraphicAttributes).get_Value(1) as IRepresentationMarker;
                            if (marker.Height == 0 || marker.Width == 0)
                                continue;
                            var geooutline = marker.get_Outline(esriOutlineType.esriOutlineBox);
                            anchor.Y -= (geooutline.Envelope.YMax + geooutline.Envelope.YMin) / 2;
                            var geoinfo = new GeoRuleInfo();
                            geoinfo.Geometry = anchor;
                            geoinfo.Rule = groupRulesDic[repID];
                            //道路编号
                            if (dr["RoadNum"].ToString() != "")
                            {
                                geoinfo.Rule = CreateRoadNumRules(dr, groupRulesDic[repID]);
                            }
                            itemInfo.GeoRules.Add(geoinfo);
                        }
                    }
                    else//已有的图层中，读取组合图例项
                    {
                        var list = new List<int>();
                        if (groupItemDic.ContainsKey(groupLyr))
                        {
                            list = groupItemDic[groupLyr];
                        }
                        //  if (!list.Contains(repID)):去掉过滤，一个图元可以跟多个要素组合：比如：火车站与高速铁路，普通铁路
                        {


                            try
                            {
                                #region
                                #region
                                var rule = GetGroupLgRule(dr["GroupLayer"].ToString(), repID);
                                #region 图例道路两端平头处理
                                IRepresentationRule repRule = null;
                                if (dr["GroupLayer"].ToString() == "LRDL")
                                {
                                    repRule = (rule as IClone).Clone() as IRepresentationRule;
                                    for (int i = 0; i < repRule.LayerCount; i++)
                                    {
                                        if (repRule.get_Layer(i) is IBasicLineSymbol)
                                        {
                                            IBasicLineSymbol basicLineSymbol = repRule.get_Layer(i) as IBasicLineSymbol;
                                            if (basicLineSymbol == null)
                                            {
                                                continue;
                                            }
                                            IGraphicAttributes LineAttributes = basicLineSymbol.Stroke as IGraphicAttributes;
                                            int id = LineAttributes.get_IDByName("Caps");
                                            LineAttributes.set_Value(id, esriLineCapStyle.esriLCSButt);
                                        }
                                    }

                                }
                                #endregion
                                var grouplyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == dr["GroupLayer"].ToString())).FirstOrDefault();
                                if (grouplyr == null)
                                    return itemtxt;

                                #endregion
                                switch ((grouplyr as IFeatureLayer).FeatureClass.ShapeType)
                                {
                                    case esriGeometryType.esriGeometryPoint:
                                        var bs = rule.get_Layer(0);
                                        if (bs is IBasicMarkerSymbol)
                                        {
                                            var marker = (rule.get_Layer(0) as IGraphicAttributes).get_Value(1) as IRepresentationMarker;
                                            if (marker.Height == 0 || marker.Width == 0)
                                                continue;

                                            var geooutline = marker.get_Outline(esriOutlineType.esriOutlineBox);
                                            anchor.Y -= (geooutline.Envelope.YMax + geooutline.Envelope.YMin) / 2;

                                            list.Add(repID);
                                            groupItemDic[groupLyr] = list;

                                            var geoinfo = new GeoRuleInfo();
                                            geoinfo.Geometry = anchor;
                                            geoinfo.Rule = rule;
                                            itemInfo.GeoRules.Add(geoinfo);
                                        }
                                        break;
                                    case esriGeometryType.esriGeometryPolyline:
                                        IPointCollection polyline = new PolylineClass();
                                        polyline.AddPoint(anchor);
                                        double len = double.Parse(dr["RuleLength"].ToString());
                                        if (len == 0)//默认线为整体长度
                                            len = lgItemLen;
                                        else//0~100百分比来做
                                        {
                                            len = len * 0.01 * lgItemLen;
                                        }
                                        IPoint topoint = new PointClass { X = anchor.X + len, Y = anchor.Y };
                                        polyline.AddPoint(topoint);
                                        if (dr["GroupLayer"].ToString() == "LRDL")
                                        {
                                            pGraphics.Add(polyline as IGeometry, repRule);
                                        }
                                        else
                                        {
                                            pGraphics.Add(polyline as IGeometry, rule);
                                        }

                                        list.Add(repID);
                                        groupItemDic[groupLyr] = list;
                                        {
                                            var geoinfo = new GeoRuleInfo();
                                            var trans = (polyline as IClone).Clone() as ITransform2D;
                                            trans.Move(-dx, dy);
                                            geoinfo.Geometry = trans as IGeometry;
                                            geoinfo.Rule = rule;
                                            if (dr["GroupLayer"].ToString() == "LRDL")
                                            {
                                                geoinfo.Rule = repRule;
                                            }
                                            else
                                            {
                                                geoinfo.Rule = rule;
                                            }
                                            itemInfo.GeoRules.Add(geoinfo);
                                        }
                                        break;
                                    case esriGeometryType.esriGeometryPolygon:
                                        break;
                                    default:
                                        break;

                                }
                                #endregion
                            }
                            catch
                            {
                            }


                        }
                    }

                    #endregion
                }
                catch
                {
                }
            }
            //重新调整位置:确定锚点
            EnvelopeClass en = new EnvelopeClass();
            foreach (var ruleInfo in itemInfo.GeoRules)
            {
                if(ruleInfo.Geometry.GeometryType == esriGeometryType.esriGeometryPolyline)
                {
                    en.Union(ruleInfo.Geometry.Envelope);
                }
            }
            //只是针对线要素
            if (Math.Abs(en.YMax + en.YMin)*0.5 >0.5)//说有偏移
            {
                List<GeoRuleInfo> groupGeo = new List<GeoRuleInfo>();
                double ctY = (en.YMax + en.YMin) * 0.5;
                foreach (var ruleInfo in itemInfo.GeoRules)
                {
                    ITransform2D geoTran = ruleInfo.Geometry as ITransform2D;
                    geoTran.Move(0, 0 - ctY);
                  ///  groupGeo.Add(new GeoRuleInfo { Geometry = geoTran as IGeometry, Rule = ruleInfo.Rule });
                }
              //  itemInfo.GeoRules = groupGeo;
            }
            return itemtxt;
        }
        
        //添加图例文字
        private void AddLengendTxt(IPoint pgeo, string name, int feid, esriTextHorizontalAlignment al = esriTextHorizontalAlignment.esriTHALeft)
        {
            //间隔：lgItemLen,8,
            //文字大小：4.0mm
            double fontsize = ItemFontSize;
            double mapscale = m_Application.ActiveView.FocusMap.ReferenceScale;
            //fontsize *= 2.83;
            InsertAnnoFea(pgeo, name, fontsize, feid, al);
        }

        //初始化组合符号:模板符号组合：高速，国道，省道+编号：
        //水平距离，垂直距离（百分比表示（1~100））
        private void LoadCompositeItems()
        {

            try
            {
                string rulepath =lgRulePath+ "Lengend.mdb";
                string path = lgRulePath+ @"LengendGroup.xml";
                #region
                compositeDataTable = new DataTable();
                compositeDataTable.Columns.Add("Location");
                compositeDataTable.Columns.Add("GroupRuleID");
                compositeDataTable.Columns.Add("RuleID");
                compositeDataTable.Columns.Add("FclName");
                compositeDataTable.Columns.Add("LgNote");
                compositeDataTable.Columns.Add("GroupLayer");
                compositeDataTable.Columns.Add("RuleLength");
                compositeDataTable.Columns.Add("RoadNum");
                compositeDataTable.Columns.Add("WhereClause");//过滤条件
                compositeDataTable.Columns.Add("LocationY");
                compositeDataTable.Columns.Add("OffsetY");//偏移值
                if (!GroupLengend)
                    return;
                XDocument doc;
                   
                {
                    doc = XDocument.Load(path);
                    var content = doc.Element("DataTable");
                    //加载树
                    var dgs = content.Elements("DataGroup");
                    foreach (var dg in dgs)
                    {

                        var drs = dg.Elements("DataRow");
                        double offsety = 0;
                        string roadNum = string.Empty;
                        if (dg.Attribute("offsetY") != null)
                        {
                            offsety = double.Parse(dg.Attribute("offsetY").Value);
                        }
                           
                        foreach (var dr in drs)
                        {

                            var newdr = compositeDataTable.NewRow();
                            double location = 0;
                            double.TryParse(dr.Element("Location").Value, out location);
                            newdr["Location"] = location;
                            newdr["GroupRuleID"] = dr.Element("GroupRuleID").Value;
                            newdr["RuleID"] = dr.Element("RuleID").Value;
                            newdr["FclName"] = dr.Element("FclName").Value;  
                            newdr["LgNote"] = dr.Element("LgNote").Value;                             
                            newdr["GroupLayer"] = dr.Element("GroupLayer").Value;
                            double val = 0;
                            double.TryParse(dr.Element("RuleLength").Value, out val);
                            newdr["RuleLength"] = val;
                            newdr["LocationY"] = 0;
                            if (dr.Element("RoadNum") != null)
                            {
                                roadNum = dr.Element("RoadNum").Value;
                                newdr["RoadNum"] = roadNum;
                            }
                            if (dr.Element("WhereClause") != null)
                            {
                                newdr["WhereClause"] = dr.Element("WhereClause").Value;
                            }
                            if (dr.Element("LocationY") != null)
                            {
                                val = 0;
                                double.TryParse(dr.Element("LocationY").Value, out val);
                                newdr["LocationY"] = val;
                            }
                            newdr["OffsetY"] = offsety; 
                            compositeDataTable.Rows.Add(newdr);
                        }
                    }



                }
                #endregion
               
               
                string templatecp = rulepath;

                string lg = "LengendGroup";

                IWorkspaceFactory wsFactorycp = new AccessWorkspaceFactoryClass();

                IWorkspace repWscp = wsFactorycp.OpenFromFile(templatecp, 0);

                IFeatureClass fc = (repWscp as IFeatureWorkspace).OpenFeatureClass(lg);
                IWorkspace ws = repWscp;
                //根据数据库获取制图表达要素类
                IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(ws);
                if (repWS.get_FeatureClassHasRepresentations(fc))
                {
                    IEnumDatasetName enumDatasetName = repWS.get_FeatureClassRepresentationNames(fc);
                    enumDatasetName.Reset();
                    IDatasetName pDatasetName = enumDatasetName.Next();
                    var m_RepClass = repWS.OpenRepresentationClass(pDatasetName.Name);
                    IRepresentationRules rules = m_RepClass.RepresentationRules;
                    rules.Reset();
                    IRepresentationRule rule = null;
                    int ruleID;
                    while (true)
                    {
                        rules.Next(out ruleID, out rule);
                        if (rule == null) break;
                        groupRulesDic[ruleID] = (rule as IClone).Clone() as IRepresentationRule;
                    }

                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        //组合符号:当前视图符号组合：铁路+出口
        private IRepresentationRule GetGroupLgRule(string fcl, int ruleID)
        {
            try
            {
                var repLyr = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == fcl.ToUpper())).FirstOrDefault();

                IFeatureRenderer feRenderer = ((IGeoFeatureLayer)repLyr).Renderer;
                if (!(feRenderer is IRepresentationRenderer)) return null;
                IRepresentationRenderer representationRenderer = (IRepresentationRenderer)feRenderer;
                IRepresentationClass repClass = representationRenderer.RepresentationClass;
                
                var rules = repClass.RepresentationRules;
                return (rules.get_Rule(ruleID) as IClone).Clone() as IRepresentationRule;
            }
            catch
            {
                return null;
            }
        }
        //初始化加载共享符号：河流，时令河等
        //共享符号。类别：通用（默认）,省图,市图,县图
        private void LoadShareItems()
        {
            try
            {
                string rulepath = lgRulePath + "Lengend.mdb";
                
                string path = lgRulePath + @"LengendShare.xml";
                #region 兼容模式
                //var docment = XDocument.Load(lgRulePath + "LengendTemplate.Xml");
                //var items = docment.Element("Template").Elements("Lengend");
                //foreach (var item in items)
                //  {
                //  if ((item.Attribute("name").Value) == LGTemplate)
                //    {
                //      if (item.Element("Lengend") != null)
                //      {
                //          rulepath = lgRulePath + item.Element("Lengend").Value;
                //      }
                //      if (item.Element("LengendShare") != null)
                //      {
                //          path = lgRulePath + item.Element("LengendShare").Value; ;
                //      }
                //    break;
                //    }
                //  }
                #endregion
              
                {
                    #region
                    multiDataTable = new DataTable();
                    multiDataTable.Columns.Add("ShareItemName");
                    multiDataTable.Columns.Add("ShareRuleID");
                    multiDataTable.Columns.Add("RuleID");
                    multiDataTable.Columns.Add("FclName");
                    multiDataTable.Columns.Add("LGNote");
                    multiDataTable.Columns.Add("Draw");
                    multiDataTable.Columns.Add("Type");
                   
                  
                    XDocument doc;
                   
                    {
                        doc = XDocument.Load(path);
                        var content = doc.Element("DataTable");
                        //加载树
                        var dgs = content.Elements("DataGroup");
                        foreach(var dg in dgs)
                        {
                            string shareName = dg.Attribute("ShareItemName").Value;
                            string shareRuleID = dg.Attribute("ShareRuleID").Value;
                            string type = "通用";
                            if (dg.Attribute("Type") != null)
                            {
                                type = dg.Attribute("Type").Value;
                            }
                            string draw = "false";
                            if (dg.Attribute("Draw") != null)
                            {
                                draw = dg.Attribute("Draw").Value;
                            }
                            var drs = dg.Elements("DataRow");
                            foreach (var dr in drs)
                            {
                                var newdr = multiDataTable.NewRow();
                                //
                                newdr["ShareItemName"] = shareName;
                                newdr["ShareRuleID"] = shareRuleID;

                                string ruleID = dr.Element("RuleID").Value;
                                newdr["RuleID"] = ruleID;
                                string fclName = dr.Element("FclName").Value;
                                newdr["FclName"] = fclName;
                                string lgNote = dr.Element("LGNote").Value;
                                newdr["LGNote"] = lgNote;
                                newdr["Type"] = type;
                                newdr["Draw"] = draw;
                                multiDataTable.Rows.Add(newdr);
                            }
                        }



                    }
                    #endregion
                }
             
                string templatecp = rulepath;

                string lg = "LengendShare";

                IWorkspaceFactory wsFactorycp = new AccessWorkspaceFactoryClass();

                IWorkspace repWscp = wsFactorycp.OpenFromFile(templatecp, 0);

                IFeatureClass fc = (repWscp as IFeatureWorkspace).OpenFeatureClass(lg);
                IWorkspace ws = repWscp;
                //根据数据库获取制图表达要素类
                IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(ws);
                if (repWS.get_FeatureClassHasRepresentations(fc))
                {
                    IEnumDatasetName enumDatasetName = repWS.get_FeatureClassRepresentationNames(fc);
                    enumDatasetName.Reset();
                    IDatasetName pDatasetName = enumDatasetName.Next();
                    var m_RepClass = repWS.OpenRepresentationClass(pDatasetName.Name);
                    IRepresentationRules rules = m_RepClass.RepresentationRules;
                    rules.Reset();
                    IRepresentationRule rule = null;
                    int ruleID;
                    while (true)
                    {
                        rules.Next(out ruleID, out rule);
                        if (rule == null) break;
                        Console.WriteLine(rules.get_Name(ruleID));
                        IRepresentationRule rulenew = (rule as IClone).Clone() as IRepresentationRule;

                        for (int i = 0; i < rulenew.LayerCount; i++)
                        {
                            IBasicMarkerSymbol ms = rulenew.Layer[i] as IBasicMarkerSymbol;
                            if (ms == null)
                                continue;
                            double size = double.Parse((ms as IGraphicAttributes).get_Value(2).ToString());
                            //添加：共享默认都是面图例项
                            if (size >lgItemLenA)
                            {
                                (ms as IGraphicAttributes).set_Value(2, lgItemLenA*0.8);
                            }
                            //if (size > Math.Max(lgItemLen, lgItemLenA))
                            //{
                            //    (ms as IGraphicAttributes).set_Value(2, Math.Max(lgItemLen, lgItemLenA));
                            //}
                            multiItemDic[ruleID] = false;
                        }
                        multiRulesDic[ruleID] = rulenew;
                    }
                    
                }
                Marshal.ReleaseComObject(fc);
                Marshal.ReleaseComObject(repWS);
                Marshal.ReleaseComObject(ws);
                Marshal.ReleaseComObject(repWscp);
                Marshal.ReleaseComObject(wsFactorycp);
                GC.Collect();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string LoadLengendPath()
        {
            string xmlpath = lgRulePath + "LengendTemplate.Xml";
            
            XDocument doc;
            string planxml = "";
          
            {
                doc = XDocument.Load(xmlpath);
                var items = doc.Element("Template").Elements("Lengend");
                
                foreach (var item in items)
                {
                    string name= item.Attribute("name").Value;
                    if (name == LGTemplate)
                    {
                        planxml = lgRulePath + item.Value;
                        break;
                    }
                }
                
                 
            }
            return planxml;
        }
         

        private IRepresentationWorkspaceExtension GetRepersentationWorkspace(IWorkspace workspace)
        {
            IWorkspaceExtensionManager wem = workspace as IWorkspaceExtensionManager;
            UID uid = new UIDClass();
            uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            IRepresentationWorkspaceExtension rwe = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
            return rwe;
        }
        private void InsertAnnoFea(IGeometry pGeometry, string annoName, double fontSize, int id, esriTextHorizontalAlignment al = esriTextHorizontalAlignment.esriTHALeft)
        {
            IFontDisp font = new StdFont() { Name = "黑体", Size = 2 } as IFontDisp;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, font, fontSize, al);
            IElement pElement = pTextElement as IElement;
            IFeature pFeature = annoFcl.CreateFeature();
            pFeature.set_Value(annoFcl.FindField("TYPE"), "图例项说明");
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.AnnotationClassID = fclPoint.FeatureClassID;
            pAnnoFeature.LinkedFeatureID = id;
            pAnnoFeature.Annotation = pElement;
            pFeature.Store();
        }
        private ITextElement CreateTextElement(IGeometry pGeoTxt, string txt, IFontDisp pFont, double size, esriTextHorizontalAlignment al = esriTextHorizontalAlignment.esriTHALeft)
        {
            IRgbColor pColor = new RgbColorClass()
            {
                Red = 0,
                Blue = 0,
                Green = 0
            };

            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = pColor,
                Font = pFont,
                Size = size,
                VerticalAlignment = esriTextVerticalAlignment.esriTVACenter,
                HorizontalAlignment = al
            };
            if (txt == "图例")
            {
                pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            }
            ITextElement pTextElment = null;
            IElement pEle = null;
            pTextElment = new TextElementClass()
            {
                Symbol = pTextSymbol,
                ScaleText = true,
                Text = txt
            };
            pEle = pTextElment as IElement;

            pEle.Geometry = pGeoTxt;
            return pTextElment;
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
        class lengendItemInfo
        {
            public double X;//锚点X距离
            public double Y;//锚点Y距离
            public int Column;//所在列
            public int Row;//所在行
        }
        public enum lgType
        {
            AnnoLg=0,
            ShareLg=1,
            GroupLg=2,
            NormalLg=3
        }
        class ItemInfo//图例项信息
        {
            public ItemInfo()
            {
                GeoRules = new List<GeoRuleInfo>();
                multTxts = new List<string>();
                lengendType = lgType.NormalLg;
            }
            public string LyrName;//图层名称
            public lgType lengendType;
            public List<string> multTxts;//共享图例
            public string ItemText;
            public int RuleID;//对应的ruleID
            public List<GeoRuleInfo> GeoRules;
        }
        class GeoRuleInfo//图例的几何和对应的制图表达规则
        {
           
            public IGeometry Geometry;
            public IRepresentationRule Rule;
        }
    }

    //注记图例
    public class AnnoItemInfo
    {
        public double FontSize;
        public string FontName;
        public string AnnoNote;
        public string AnnoName;
        public ICmykColor AnnoColor;
    }
     
}
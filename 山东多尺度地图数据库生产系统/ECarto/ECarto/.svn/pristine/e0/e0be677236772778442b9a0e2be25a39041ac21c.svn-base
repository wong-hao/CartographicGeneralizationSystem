using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.EmergencyMap
{
    public class POISelection
    {
        /// <summary>
        /// POI选取信息结构体
        /// </summary>
        public struct POISelectionInfo
        {
            public string FCName;
            public string RuleName;
            public double Ratio;
            public int  RuleID;
            public double BufferVal;//居民点尺寸
        }

        private GApplication _app;//应用程序
        private Dictionary<string, int> fcDisplayDic = new Dictionary<string,int>();
        public POISelection(GApplication app, Dictionary<string, int> fcDisplayDic_=null)
        {
            _app = app;
            if (fcDisplayDic_ != null)
            {
                fcDisplayDic = fcDisplayDic_;
            }
            InitParams();
        }
        private List<string> fclList = new List<string>();
        private void InitParams()
        {
            fclList.Add("AGNP");
            fclList.Add("HYDP");
            fclList.Add("HFCP");
        }
        public string autoSelect(List<POISelectionInfo> poiSelectionInfoList, Dictionary<string, List<string>> fcName2filterNames = null)
        {
            string msg = "";

            try
            {
                POIHelper poiHelper = new POIHelper(_app);

                IQueryFilter qf = null;

                IFeatureClass fc = null;
                IFeature f = null;
                IFeatureCursor fCuror = null;
                IFeatureClass pAnno = null;
                //ILayer annolayer = _app.Workspace.LayerManager.GetLayer(l => (l.Name.ToUpper() == ("ANNO"))).FirstOrDefault();
                ILayer annolayer=  _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == "ANNO";
                })).ToArray().FirstOrDefault();
                pAnno = (annolayer as IFeatureLayer).FeatureClass;
                IQueryFilter annoqf = new QueryFilterClass();
                //遍历要素类别
                for (int i = 0; i < poiSelectionInfoList.Count; i++)
                {
                    var lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == poiSelectionInfoList[i].FCName;
                    })).ToArray();
                    if (0 == lyrs.Count())
                    {
                        continue;
                    }

                    fc = (lyrs[0] as IFeatureLayer).FeatureClass;

                    qf = new QueryFilterClass();
                    if(poiSelectionInfoList[i].RuleName == "全部要素")
                    {
                        qf.WhereClause = "";//该图层的所有要素
                    }
                    else
                    {
                        string whereClause = "RuleID =" + poiSelectionInfoList[i].RuleID;
                      
                       // string whereClause = poiHelper.GetWhereClauseFromTemplate(poiSelectionInfoList[i].FCName, poiSelectionInfoList[i].RuleName);
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            continue;//图层对照规则表中不存在该规则
                        }
                        qf.WhereClause = whereClause;
                    }

                    MultipointClass mp = new MultipointClass();
                    List<int> features = new List<int>();

                    //该要素类中不参与选取的名称几何
                    List<string> filterNames = null;
                    int nameIndex = fc.Fields.FindField("NAME");
                    string fcName = fc.AliasName.ToUpper();
                    if (fcName2filterNames != null && fcName2filterNames.ContainsKey(fcName) && nameIndex != -1)
                    {
                        filterNames = fcName2filterNames[fcName];
                    }

                    //查询
                    fCuror = fc.Search(qf, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (filterNames != null)
                        {
                            string name = f.get_Value(nameIndex).ToString().ToUpper();
                            if (name != null && filterNames.Contains(name))
                            {
                                continue;//不参与选取
                            }
                        }

                        if (f.Shape.IsEmpty)
                        {
                            continue;//不参与选取
                        }

                        IPoint p = null;
                        p = f.ShapeCopy as IPoint;
                        IPointIDAware pntIDA = p as IPointIDAware;
                        pntIDA.PointIDAware = true;
                        p.ID = 0;
                        //判断点是否在道路线上！
                        p.ID = (CheckRoadHYDLOn(p, poiSelectionInfoList[i].BufferVal * 0.5, fcName));
                          //  p.ID = 1;
                        mp.AddGeometry(p);
                        features.Add(f.OID);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);

                    if (features.Count < 3)
                        continue;//该类注记点个数少于3         
                    bool[] selected = poiHelper.MultiPointSelectionReLock(mp, poiSelectionInfoList[i].Ratio, null);
                    if (selected == null)
                    {
                        continue;
                    }
                    Dictionary<int, bool> feaSelected = new Dictionary<int, bool>();
                    for (int j = 0; j < features.Count; j++)
                    {
                        feaSelected.Add(features[j], selected[j]);
                    }

                    //处理
                    fCuror = fc.Update(qf, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (feaSelected.ContainsKey(f.OID) && !feaSelected[f.OID])
                        {
                            if (fcDisplayDic.ContainsKey(fc.AliasName))
                            {
                                f.set_Value(fc.FindField("ruleID"), fcDisplayDic[fc.AliasName]);
                                fCuror.UpdateFeature(f);
                            }
                            else
                            {
                                fCuror.DeleteFeature();
                            }
                        }
                            
                    }
                    //处理关联的注记
                    annoqf.WhereClause = "AnnotationClassID =" + fc.FeatureClassID.ToString();
                    IFeatureCursor annofcursor = pAnno.Update(annoqf,true);
                    while ((f = annofcursor.NextFeature()) != null)
                    {
                        int feid =int.Parse(f.get_Value(pAnno.FindField("FeatureID")).ToString());
                        if (feaSelected.ContainsKey(feid) && !feaSelected[feid])
                            annofcursor.DeleteFeature();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(annofcursor);
                }
              
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
        //包含附区自动
        public string autoSelectEx(List<POISelectionInfo> poiSelectionInfoList, List<POISelectionInfo> poiSelectionInfoListEx, Dictionary<string, List<string>> fcName2filterNames = null)
        {
            string msg = "";

            try
            {
                POIHelper poiHelper = new POIHelper(_app);

                IQueryFilter qf = null;

                IFeatureClass fc = null;
                IFeature f = null;
                IFeatureCursor fCuror = null;
                IFeatureClass pAnno = null;
               
                ILayer annolayer = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == "ANNO";
                })).ToArray().FirstOrDefault();
                pAnno = (annolayer as IFeatureLayer).FeatureClass;
                IQueryFilter annoqf = new QueryFilterClass();
                //遍历要素类别 主区
                for (int i = 0; i < poiSelectionInfoList.Count; i++)
                {
                    #region
                    var lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == poiSelectionInfoList[i].FCName;
                    })).ToArray();
                    if (0 == lyrs.Count())
                    {
                        continue;
                    }

                    fc = (lyrs[0] as IFeatureLayer).FeatureClass;

                    qf = new QueryFilterClass();
                    string sqlgis = "ATTACH  is NULL";
                    if (poiSelectionInfoList[i].RuleName == "全部要素")
                    {
                        qf.WhereClause = "" + sqlgis;//该图层的所有要素
                    }
                    else
                    {
                        string whereClause = "RuleID =" + poiSelectionInfoList[i].RuleID+" and "+sqlgis;
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            continue;//图层对照规则表中不存在该规则
                        }
                        qf.WhereClause = whereClause;
                    }

                    MultipointClass mp = new MultipointClass();
                    List<int> features = new List<int>();

                    //该要素类中不参与选取的名称几何
                    List<string> filterNames = null;
                    int nameIndex = fc.Fields.FindField("NAME");
                    string fcName = fc.AliasName.ToUpper();
                    if (fcName2filterNames != null && fcName2filterNames.ContainsKey(fcName) && nameIndex != -1)
                    {
                        filterNames = fcName2filterNames[fcName];
                    }

                    //查询
                    fCuror = fc.Search(qf, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (filterNames != null)
                        {
                            string name = f.get_Value(nameIndex).ToString().ToUpper();
                            if (name != null && filterNames.Contains(name))
                            {
                                continue;//不参与选取
                            }

                        }

                        if (f.Shape.IsEmpty)
                        {
                            continue;//不参与选取
                        }

                        IPoint p = null;
                        p = f.ShapeCopy as IPoint;
                        IPointIDAware pntIDA = p as IPointIDAware;
                        pntIDA.PointIDAware = true;
                        p.ID = 0;
                        //判断点是否在水系道路线上！
                        p.ID = (CheckRoadHYDLOn(p, poiSelectionInfoList[i].BufferVal * 0.5, fcName));
                            //p.ID = 1;
                        mp.AddGeometry(p);
                        features.Add(f.OID);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(p);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);


                    if (poiSelectionInfoList[i].Ratio == 0)
                    {
                        fCuror = fc.Update(qf, true);


                    }

                    if (features.Count < 3 && poiSelectionInfoList[i].Ratio > 0)
                        continue;//该类注记点个数少于3
                    bool[] selected=null;
                    if (features.Count >= 3)
                    {
                        //筛选
                        selected = poiHelper.MultiPointSelectionReLock(mp, poiSelectionInfoList[i].Ratio, null);
                    }
                    if (poiSelectionInfoList[i].Ratio == 0)
                    {                       
                        selected = new bool[features.Count];
                        for (int j = 0; j< features.Count; j++)
                        {
                            selected[j] = false;
                        }
                    }
                    
                    if (selected == null)
                    {
                        continue;
                    }
                    Dictionary<int, bool> feaSelected = new Dictionary<int, bool>();
                    for (int j = 0; j < features.Count; j++)
                    {
                        feaSelected.Add(features[j], selected[j]);
                    }

                    //处理
                    fCuror = fc.Update(qf, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (feaSelected.ContainsKey(f.OID) && !feaSelected[f.OID])//feaSelected为true时保留要素,false时不选取
                        {
                            if (fcDisplayDic.ContainsKey(fc.AliasName))
                            {
                                f.set_Value(fc.FindField("ruleID"), fcDisplayDic[fc.AliasName]);
                                var ss = fc.FindField("ruleID");
                                var sf = fcDisplayDic[fc.AliasName];
                                fCuror.UpdateFeature(f);
                            }
                            else
                            {
                                fCuror.DeleteFeature();
                            }
                        }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                    }
                    //处理关联的注记
                    annoqf.WhereClause = "AnnotationClassID =" + fc.FeatureClassID.ToString();
                    IFeatureCursor annofcursor = pAnno.Update(annoqf, true);
                    while ((f = annofcursor.NextFeature()) != null)
                    {
                        int feid = int.Parse(f.get_Value(pAnno.FindField("FeatureID")).ToString());
                        if (feaSelected.ContainsKey(feid) && !feaSelected[feid])
                            annofcursor.DeleteFeature();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(annofcursor);
                    #endregion
                }
                //遍历要素类别 附区
                for (int i = 0; i < poiSelectionInfoListEx.Count; i++)
                {
                    #region
                    var lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == poiSelectionInfoListEx[i].FCName;
                    })).ToArray();
                    if (0 == lyrs.Count())
                    {
                        continue;
                    }

                    fc = (lyrs[0] as IFeatureLayer).FeatureClass;

                    qf = new QueryFilterClass();
                    string sqlgis = "ATTACH  = '1'";
                    if (poiSelectionInfoListEx[i].RuleName == "全部要素")
                    {
                        qf.WhereClause = sqlgis;//该图层的所有要素
                    }
                    else
                    {
                        string whereClause = "RuleID =" + poiSelectionInfoListEx[i].RuleID + " and " + sqlgis;

                        
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            continue;//图层对照规则表中不存在该规则
                        }
                        qf.WhereClause = whereClause;
                    }

                    MultipointClass mp = new MultipointClass();
                    List<int> features = new List<int>();

                    //该要素类中不参与选取的名称几何
                    List<string> filterNames = null;
                    int nameIndex = fc.Fields.FindField("NAME");
                    string fcName = fc.AliasName.ToUpper();
                    if (fcName2filterNames != null && fcName2filterNames.ContainsKey(fcName) && nameIndex != -1)
                    {
                        filterNames = fcName2filterNames[fcName];
                    }

                    //查询
                    fCuror = fc.Search(qf, true);
                   
                        while ((f = fCuror.NextFeature()) != null)
                        {
                           
                            if (filterNames != null)
                            {
                                 if(nameIndex>=0)
                                {
                                string name = f.get_Value(nameIndex).ToString().ToUpper();
                                if (name != null && filterNames.Contains(name))
                                {
                                    continue;//不参与选取
                                }
                               }
                            }

                            if (f.Shape.IsEmpty)
                            {
                                continue;//不参与选取
                            }

                            IPoint p = null;
                            p = f.ShapeCopy as IPoint;
                            IPointIDAware pntIDA = p as IPointIDAware;
                            pntIDA.PointIDAware = true;
                            p.ID = 0;
                            //判断点是否在道路线上！
                            p.ID = (CheckRoadHYDLOn(p, poiSelectionInfoListEx[i].BufferVal * 0.5, fcName));
                            //  p.ID = 1;
                            mp.AddGeometry(p);
                            features.Add(f.OID);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(p);
                        }
                    


                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);

                   if (features.Count < 3 && poiSelectionInfoListEx[i].Ratio > 0)
                        continue;//该类注记点个数少于3
                    bool[] selected = null;
                    if (features.Count >= 3)
                    {
                        //筛选
                        selected = poiHelper.MultiPointSelectionReLock(mp, poiSelectionInfoListEx[i].Ratio, null);
                    }   

                    if (poiSelectionInfoListEx[i].Ratio == 0)
                    {
                        selected = new bool[features.Count];
                        for (int j = 0; j < features.Count; j++)
                        {
                            selected[j] = false;
                        }
                    }
                    
                    if (selected == null)
                    {
                        continue;
                    }
                    Dictionary<int, bool> feaSelected = new Dictionary<int, bool>();
                    for (int j = 0; j < features.Count; j++)
                    {
                        feaSelected.Add(features[j], selected[j]);
                    }

                    //处理
                    fCuror = fc.Update(qf, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (feaSelected.ContainsKey(f.OID) && !feaSelected[f.OID])
                        {
                            if (fcDisplayDic.ContainsKey(fc.AliasName))
                            {
                                f.set_Value(fc.FindField("ruleID"), fcDisplayDic[fc.AliasName]);
                                fCuror.UpdateFeature(f);
                            }
                            else
                            {
                                fCuror.DeleteFeature();
                            }
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(f);

                    }
                    //处理关联的注记
                    annoqf.WhereClause = "AnnotationClassID =" + fc.FeatureClassID.ToString();
                    IFeatureCursor annofcursor = pAnno.Update(annoqf, true);
                    while ((f = annofcursor.NextFeature()) != null)
                    {
                        int feid = int.Parse(f.get_Value(pAnno.FindField("FeatureID")).ToString());
                        if (feaSelected.ContainsKey(feid) && !feaSelected[feid])
                            annofcursor.DeleteFeature();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(annofcursor);
                    #endregion
                }
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
        /// <summary>
        /// 是否在相应要素上：道路，水系
        /// 2-水路交叉口
        /// 1-水交叉或者路交叉口
        /// 0-独立居民地
        /// </summary>
        private int CheckRoadHYDLOn(IPoint geometry,double dis,string name)
        {
            if (dis == 0) //全部要素:
                return 0;
            if (fclList.Contains(name.ToUpper())) // AGNP 与道路关联
            {
                int val = 0;
                double ms = _app.ActiveView.FocusMap.ReferenceScale;
                ITopologicalOperator topo = geometry as ITopologicalOperator;
                double endis = dis * ms * 1e-3;
                IEnvelope geo = new EnvelopeClass();
                geo.PutCoords(geometry.X - endis, geometry.Y - endis, geometry.X + endis, geometry.Y + endis);
                ISpatialFilter inSpatialFilter = new SpatialFilterClass();
                inSpatialFilter.Geometry = geo;
                inSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                inSpatialFilter.WhereClause = "RuleID <> 1";//排除不显示要素！
                //路
                var lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LRDL";
                })).ToArray();
                if (lyrs.Length != 0)
                {
                    var fc = (lyrs[0] as IFeatureLayer).FeatureClass;
                  
                    int ncount = fc.FeatureCount(inSpatialFilter);
                    if (ncount > 0)
                        val++;
                }
                //水
                lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IGeoFeatureLayer && (l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "HYDL";
                })).ToArray();
                if (lyrs.Length != 0)
                {
                    var fc = (lyrs[0] as IFeatureLayer).FeatureClass;

                    int ncount = fc.FeatureCount(inSpatialFilter);
                    if (ncount > 0)
                        val++;
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(geo);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(inSpatialFilter);
                return val;
            }       
            return 0;
        }
        //附区交互
        public string manualSelectEx(List<POISelectionInfo> poiSelectionInfoList, List<POISelectionInfo> poiSelectionInfoListEx, IGeometry geo, Dictionary<string, List<string>> fcName2filterNames = null)
        {
            string msg = "";

            try
            {
                POIHelper poiHelper = new POIHelper(_app);

                ISpatialFilter sp = new SpatialFilterClass();
                sp.Geometry = geo;
                sp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureClass fc = null;
                IFeature f = null;
                IFeatureCursor fCuror = null;
                ILayer annolayer = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == "ANNO";
                })).ToArray().FirstOrDefault();
                IFeatureClass pAnno = (annolayer as IFeatureLayer).FeatureClass;
                IQueryFilter annoqf = new QueryFilterClass();
                 
                #region  主区
                string sqlgis = "ATTACH  is NULL";
                for (int i = 0; i < poiSelectionInfoList.Count; i++)
                {
                    var lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == poiSelectionInfoList[i].FCName;
                    })).ToArray();
                    if (0 == lyrs.Count())
                    {
                        continue;
                    }

                    fc = (lyrs[0] as IFeatureLayer).FeatureClass;

                    if (poiSelectionInfoList[i].RuleName == "全部要素")
                    {
                        sp.WhereClause = sqlgis;//该图层的所有要素
                    }
                    else
                    {
                        string whereClause = "RuleID =" + poiSelectionInfoList[i].RuleID+" and "+sqlgis;

                        // string whereClause = poiHelper.GetWhereClauseFromTemplate(poiSelectionInfoList[i].FCName, poiSelectionInfoList[i].RuleName);
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            continue;// 
                        }
                        sp.WhereClause = whereClause;
                    }

                    MultipointClass mp = new MultipointClass();
                    List<int> features = new List<int>();

                    //该要素类中不参与选取的名称几何
                    List<string> filterNames = null;
                    int nameIndex = fc.Fields.FindField("NAME");
                    string fcName = fc.AliasName.ToUpper();
                    if (fcName2filterNames != null && fcName2filterNames.ContainsKey(fcName) && nameIndex != -1)
                    {
                        filterNames = fcName2filterNames[fcName];
                    }

                    //查询
                    fCuror = fc.Search(sp, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (filterNames != null)
                        {
                            string name = f.get_Value(nameIndex).ToString().ToUpper();
                            if (name != null && filterNames.Contains(name))
                            {
                                continue;//不参与选取
                            }

                        }

                        if (f.Shape.IsEmpty)
                        {
                            continue;//不参与选取
                        }

                        IPoint p = null;
                        p = f.ShapeCopy as IPoint;
                        IPointIDAware pntIDA = p as IPointIDAware;
                        pntIDA.PointIDAware = true;
                        p.ID = 0;
                        //判断点是否在道路线上！
                        p.ID = (CheckRoadHYDLOn(p, poiSelectionInfoList[i].BufferVal * 0.5, fcName));
                          //  p.ID = 1;
                        mp.AddGeometry(p);
                        features.Add(f.OID);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);

                    if (features.Count < 3)
                        continue;//该类注记点个数少于3

                    //筛选
                    //bool[] selected = poiHelper.MultiPointSelection(mp, poiSelectionInfoList[i].Ratio, null);
                    bool[] selected = poiHelper.MultiPointSelectionReLock(mp, poiSelectionInfoList[i].Ratio, null);
                    if (selected == null)
                    {
                        continue;
                    }
                    Dictionary<int, bool> feaSelected = new Dictionary<int, bool>();
                    for (int j = 0; j < features.Count; j++)
                    {
                        feaSelected.Add(features[j], selected[j]);
                    }

                    //处理
                    fCuror = fc.Update(sp, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (feaSelected.ContainsKey(f.OID) && !feaSelected[f.OID])
                        {
                            if (fcDisplayDic.ContainsKey(fc.AliasName))
                            {
                                f.set_Value(fc.FindField("ruleID"), fcDisplayDic[fc.AliasName]);
                                fCuror.UpdateFeature(f);
                            }
                            else
                            {
                                fCuror.DeleteFeature();
                            }
                        }
                    }
                    //处理关联的注记
                    annoqf.WhereClause = "AnnotationClassID =" + fc.FeatureClassID.ToString();
                    IFeatureCursor annofcursor = pAnno.Update(annoqf, true);
                    while ((f = annofcursor.NextFeature()) != null)
                    {
                        int feid = int.Parse(f.get_Value(pAnno.FindField("FeatureID")).ToString());
                        if (feaSelected.ContainsKey(feid) && !feaSelected[feid])
                            annofcursor.DeleteFeature();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(annofcursor);

                }
                #endregion
                #region 附区
                sqlgis = "ATTACH = '1'";
                sp.WhereClause = sqlgis;
                for (int i = 0; i < poiSelectionInfoListEx.Count; i++)
                {
                    var lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == poiSelectionInfoListEx[i].FCName;
                    })).ToArray();
                    if (0 == lyrs.Count())
                    {
                        continue;
                    }

                    fc = (lyrs[0] as IFeatureLayer).FeatureClass;

                    if (poiSelectionInfoListEx[i].RuleName == "全部要素")
                    {
                        sp.WhereClause = sqlgis;//该图层的所有要素
                    }
                    else
                    {
                        string whereClause = "RuleID =" + poiSelectionInfoListEx[i].RuleID +" and "+ sqlgis;

                        if (string.IsNullOrEmpty(whereClause))
                        {
                            continue;// 
                        }
                        sp.WhereClause = whereClause;
                    }

                    MultipointClass mp = new MultipointClass();
                    List<int> features = new List<int>();

                    //该要素类中不参与选取的名称几何
                    List<string> filterNames = null;
                    int nameIndex = fc.Fields.FindField("NAME");
                    string fcName = fc.AliasName.ToUpper();
                    if (fcName2filterNames != null && fcName2filterNames.ContainsKey(fcName) && nameIndex != -1)
                    {
                        filterNames = fcName2filterNames[fcName];
                    }

                    //查询
                    fCuror = fc.Search(sp, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (filterNames != null)
                        {
                            string name = f.get_Value(nameIndex).ToString().ToUpper();
                            if (name != null && filterNames.Contains(name))
                            {
                                continue;//不参与选取
                            }

                        }

                        if (f.Shape.IsEmpty)
                        {
                            continue;//不参与选取
                        }

                        IPoint p = null;
                        p = f.ShapeCopy as IPoint;
                        IPointIDAware pntIDA = p as IPointIDAware;
                        pntIDA.PointIDAware = true;
                        p.ID = 0;
                        //判断点是否在道路线上！
                        p.ID = (CheckRoadHYDLOn(p, poiSelectionInfoListEx[i].BufferVal * 0.5, fcName));
                          //  p.ID = 1;
                        mp.AddGeometry(p);
                        features.Add(f.OID);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);

                    if (features.Count < 3)
                        continue;//该类注记点个数少于3

                    //筛选
                  //  bool[] selected = poiHelper.MultiPointSelection(mp, poiSelectionInfoListEx[i].Ratio, null);
                    bool[] selected = poiHelper.MultiPointSelectionReLock(mp, poiSelectionInfoListEx[i].Ratio, null);
                 
                    if (selected == null)
                    {
                        continue;
                    }
                    Dictionary<int, bool> feaSelected = new Dictionary<int, bool>();
                    for (int j = 0; j < features.Count; j++)
                    {
                        feaSelected.Add(features[j], selected[j]);
                    }

                    //处理
                    fCuror = fc.Update(sp, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (feaSelected.ContainsKey(f.OID) && !feaSelected[f.OID])
                        {
                            if (fcDisplayDic.ContainsKey(fc.AliasName))
                            {
                                f.set_Value(fc.FindField("ruleID"), fcDisplayDic[fc.AliasName]);
                                fCuror.UpdateFeature(f);
                            }
                            else
                            {
                                fCuror.DeleteFeature();
                            }
                        }
                    }
                    //处理关联的注记
                    annoqf.WhereClause = "AnnotationClassID =" + fc.FeatureClassID.ToString();
                    IFeatureCursor annofcursor = pAnno.Update(annoqf, true);
                    while ((f = annofcursor.NextFeature()) != null)
                    {
                        int feid = int.Parse(f.get_Value(pAnno.FindField("FeatureID")).ToString());
                        if (feaSelected.ContainsKey(feid) && !feaSelected[feid])
                            annofcursor.DeleteFeature();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(annofcursor);

                }
               
                #endregion
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

        public string manualSelect(List<POISelectionInfo> poiSelectionInfoList, IGeometry geo, Dictionary<string, List<string>> fcName2filterNames = null)
        {
            string msg = "";

            try
            {
                POIHelper poiHelper = new POIHelper(_app);

                ISpatialFilter sp = new SpatialFilterClass();
                sp.Geometry = geo;
                sp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureClass fc = null;
                IFeature f = null;
                IFeatureCursor fCuror = null;
                ILayer annolayer = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == "ANNO";
                })).ToArray().FirstOrDefault();
                IFeatureClass pAnno = (annolayer as IFeatureLayer).FeatureClass;
                IQueryFilter annoqf = new QueryFilterClass();
                //遍历要素类别
                for (int i = 0; i < poiSelectionInfoList.Count; i++)
                {
                    var lyrs = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == poiSelectionInfoList[i].FCName;
                    })).ToArray();
                    if (0 == lyrs.Count())
                    {
                        continue;
                    }

                    fc = (lyrs[0] as IFeatureLayer).FeatureClass;

                    if (poiSelectionInfoList[i].RuleName == "全部要素")
                    {
                        sp.WhereClause = "";//该图层的所有要素
                    }
                    else
                    {
                        string whereClause = "RuleID =" + poiSelectionInfoList[i].RuleID;
                      
                       // string whereClause = poiHelper.GetWhereClauseFromTemplate(poiSelectionInfoList[i].FCName, poiSelectionInfoList[i].RuleName);
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            continue;// 
                        }
                        sp.WhereClause = whereClause;
                    }

                    MultipointClass mp = new MultipointClass();
                    List<int> features = new List<int>();

                    //该要素类中不参与选取的名称几何
                    List<string> filterNames = null;
                    int nameIndex = fc.Fields.FindField("NAME");
                    string fcName = fc.AliasName.ToUpper();
                    if (fcName2filterNames != null && fcName2filterNames.ContainsKey(fcName) && nameIndex != -1)
                    {
                        filterNames = fcName2filterNames[fcName];
                    }

                    //查询
                    fCuror = fc.Search(sp, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (filterNames != null)
                        {
                            string name = f.get_Value(nameIndex).ToString().ToUpper();
                            if (name != null && filterNames.Contains(name))
                            {
                                continue;//不参与选取
                            }

                        }

                        if (f.Shape.IsEmpty)
                        {
                            continue;//不参与选取
                        }

                        IPoint p = null;
                        p = f.ShapeCopy as IPoint;
                        IPointIDAware pntIDA = p as IPointIDAware;
                        pntIDA.PointIDAware = true;
                        p.ID = 0;
                        //判断点是否在道路线上！
                        p.ID = (CheckRoadHYDLOn(p, poiSelectionInfoList[i].BufferVal * 0.5, fcName));
                           // p.ID = 1;
                        mp.AddGeometry(p);
                        features.Add(f.OID);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);

                    if (features.Count < 3)
                        continue;//该类注记点个数少于3

                    //筛选
                   // bool[] selected = poiHelper.MultiPointSelection(mp, poiSelectionInfoList[i].Ratio, null);
                    bool[] selected = poiHelper.MultiPointSelectionReLock(mp, poiSelectionInfoList[i].Ratio, null);
                    if (selected == null)
                    {
                        continue;
                    }
                    Dictionary<int, bool> feaSelected = new Dictionary<int, bool>();
                    for (int j = 0; j < features.Count; j++)
                    {
                        feaSelected.Add(features[j], selected[j]);
                    }

                    //处理
                    fCuror = fc.Update(sp, true);
                    while ((f = fCuror.NextFeature()) != null)
                    {
                        if (feaSelected.ContainsKey(f.OID) && !feaSelected[f.OID])
                        {
                            if (fcDisplayDic.ContainsKey(fc.AliasName))
                            {
                                f.set_Value(fc.FindField("ruleID"), fcDisplayDic[fc.AliasName]);
                                fCuror.UpdateFeature(f);
                            }
                            else
                            {
                                fCuror.DeleteFeature();
                            }
                        }
                    }
                    //处理关联的注记
                    annoqf.WhereClause = "AnnotationClassID =" + fc.FeatureClassID.ToString();
                    IFeatureCursor annofcursor = pAnno.Update(annoqf, true);
                    while ((f = annofcursor.NextFeature()) != null)
                    {
                        int feid = int.Parse(f.get_Value(pAnno.FindField("FeatureID")).ToString());
                        if (feaSelected.ContainsKey(feid) && !feaSelected[feid])
                            annofcursor.DeleteFeature();
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCuror);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(annofcursor);
                  
                }

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

        //by lz
        public string POISelect(List<POISelectionInfo> mainInfoList, List<POISelectionInfo> adjInfoList, Dictionary<string, List<string>> fcName2filterNames, int weightScale, IGeometry extentGeo = null, WaitOperation wo = null)
        {
            string err = "";
            try
            {
                if (mainInfoList != null && mainInfoList.Count > 0)
                {
                    #region 主区POI选取
                    for (int i = 0; i < mainInfoList.Count; i++)
                    {
                        var lyr = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == mainInfoList[i].FCName;
                        })).FirstOrDefault() as IFeatureLayer;
                        if (lyr == null || lyr.FeatureClass == null)
                            continue;
                        IFeatureClass fc = lyr.FeatureClass;
                        string fcName = fc.AliasName.ToUpper();

                        List<string> filterNames = null;//不参与选取的要素名称
                        int nameIndex = fc.Fields.FindField("name");
                        if (nameIndex != -1 && fcName2filterNames != null && fcName2filterNames.ContainsKey(fcName))
                        {
                            filterNames = fcName2filterNames[fcName];
                        }

                        //初始化filter
                        IQueryFilter filter = null;
                        if (extentGeo != null)
                        {
                            filter = new SpatialFilterClass();
                            (filter as ISpatialFilter).Geometry = extentGeo;
                            (filter as ISpatialFilter).SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                            if (mainInfoList[i].RuleName == "全部要素")
                            {
                                filter.WhereClause = "ATTACH  is NULL";
                            }
                            else
                            {
                                filter.WhereClause = string.Format("RuleID = {0} and ATTACH  is NULL", mainInfoList[i].RuleID);
                            }
                        }
                        else
                        {
                            filter = new QueryFilterClass();
                            if (mainInfoList[i].RuleName == "全部要素")
                            {
                                filter.WhereClause = "ATTACH  is NULL";
                            }
                            else
                            {
                                filter.WhereClause = string.Format("RuleID = {0} and ATTACH  is NULL", mainInfoList[i].RuleID);
                            }
                        }

                        
                        //处理
                        ProcessPOI(fc, filter, mainInfoList[i], filterNames, weightScale);
                    }
                    #endregion
                }

                if (adjInfoList != null && adjInfoList.Count > 0)
                {
                    #region 邻区POI选取
                    for (int i = 0; i < adjInfoList.Count; i++)
                    {
                        var lyr = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                        {
                            return l is IFeatureLayer && (l as IFeatureLayer).FeatureClass.AliasName == adjInfoList[i].FCName;
                        })).FirstOrDefault() as IFeatureLayer;
                        if (lyr == null || lyr.FeatureClass == null)
                            continue;
                        IFeatureClass fc = lyr.FeatureClass;
                        string fcName = fc.AliasName.ToUpper();

                        List<string> filterNames = null;//不参与选取的要素名称
                        int nameIndex = fc.Fields.FindField("name");
                        if (nameIndex != -1 && fcName2filterNames != null && fcName2filterNames.ContainsKey(fcName))
                        {
                            filterNames = fcName2filterNames[fcName];
                        }

                        //初始化filter
                        IQueryFilter filter = null;
                        if (extentGeo != null)
                        {
                            filter = new SpatialFilterClass();
                            (filter as ISpatialFilter).Geometry = extentGeo;
                            (filter as ISpatialFilter).SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                            if (adjInfoList[i].RuleName == "全部要素")
                            {
                                filter.WhereClause = "ATTACH = '1'";
                            }
                            else
                            {
                                filter.WhereClause = string.Format("RuleID = {0} and ATTACH = '1'", adjInfoList[i].RuleID);
                            }
                        }
                        else
                        {
                            filter = new QueryFilterClass();
                            if (adjInfoList[i].RuleName == "全部要素")
                            {
                                filter.WhereClause = "ATTACH = '1'";
                            }
                            else
                            {
                                filter.WhereClause = string.Format("RuleID = {0} and ATTACH = '1'", adjInfoList[i].RuleID);
                            }
                        }

                        //处理
                        ProcessPOI(fc, filter, adjInfoList[i], filterNames, weightScale);
                    }
                    #endregion
                }

                _app.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, _app.ActiveView.Extent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                err = ex.Message;
            }

            return err;
        }

        private void ProcessPOI(IFeatureClass fc, IQueryFilter filter, POISelectionInfo selInfo, List<string> filterNames, int weightScale)
        {
            Dictionary<int, bool> fid2Sel = new Dictionary<int, bool>();//要素ID->保留状态,true为保留,false为舍去

            IFeatureCursor feCursor = null;
            IFeature fe = null;

            POIHelper poiHelper = new POIHelper(_app);
            string fcName = fc.AliasName.ToUpper();
            int nameIndex = fc.Fields.FindField("name");
            int selStateIndex = fc.Fields.FindField("selectstate");
            if (selStateIndex != -1)
            {
                if(filter.WhereClause != "")
                    filter.WhereClause = string.Format("({0}) and selectstate is null", filter.WhereClause);
                else
                    filter.WhereClause = string.Format("selectstate is null");
            }
            

            #region 获取要素的选取标记
            Dictionary<int, IPoint> FID2Point = new Dictionary<int, IPoint>();//要素ID->几何
            
            feCursor = fc.Search(filter, true);
            while ((fe = feCursor.NextFeature()) != null)
            {
                if (filterNames != null)
                {
                    string name = fe.get_Value(nameIndex).ToString().ToUpper();
                    if (name != "" && filterNames.Contains(name))
                        continue;//不参与选取
                }

                if (fe.Shape.IsEmpty)
                    continue;//不参与选取


                IPoint p = fe.ShapeCopy as IPoint;
                IPointIDAware ptIDA = p as IPointIDAware;
                ptIDA.PointIDAware = true;
                p.ID = (CheckRoadHYDLOn(p, selInfo.BufferVal * 0.5, fcName));//判断点是否在水系道路线上:水路交叉口上的点(2)、水系或者道路上的点(1)、独立点（0）
                FID2Point.Add(fe.OID, p);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);

            if (FID2Point.Count > 2)
            {
                //构三角网筛选,确认哪些要素需要保留
                fid2Sel = SelPOIByTin(FID2Point, selInfo.Ratio, weightScale, null);
            }
            else //fid2Sel.Count <= 2
            {
                if (selInfo.Ratio == 0)//全部去除
                {
                    foreach(var kv in FID2Point)
                    {
                        fid2Sel.Add(kv.Key, false);
                    }
                }
                else if (selInfo.Ratio >= 1)//全部保留
                {
                    foreach (var kv in FID2Point)
                    {
                        fid2Sel.Add(kv.Key, true);
                    }
                }
                else//仅保留一个
                {
                    int maxFID = -1;
                    int maxLevel = -1;
                    foreach (var kv in FID2Point)
                    {
                        int level = kv.Value.ID;
                        if (level > maxLevel)
                        {
                            maxLevel = level;
                            maxFID = kv.Key;
                        }
                    }

                    foreach (var kv in FID2Point)
                    {
                        if (maxFID == kv.Key)
                        {
                            fid2Sel.Add(kv.Key, true);
                        }
                        else
                        {
                            fid2Sel.Add(kv.Key, false);
                        }
                    }
                }
            }
            #endregion

            #region 要素处理
            feCursor = fc.Update(filter, true);
            while ((fe = feCursor.NextFeature()) != null)
            {
                if (fid2Sel.ContainsKey(fe.OID) && !fid2Sel[fe.OID])//feaSelected为true时保留要素,false时不选取
                {
                    if (selStateIndex != -1)
                    {
                        fe.set_Value(selStateIndex, "POI选取");//设置为未选取状态
                        feCursor.UpdateFeature(fe);
                    }
                    else
                    {
                        feCursor.DeleteFeature();
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fe);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(feCursor);
            #endregion


            #region 关联注记处理
            var layers = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && (l is IFDOGraphicsLayer);
            })).ToArray();
            foreach (var lyr in layers)
            {
                IFeatureClass annoFC = (lyr as IFeatureLayer).FeatureClass;
                if (annoFC == null || (annoFC as IDataset).Workspace.PathName != _app.Workspace.EsriWorkspace.PathName)
                    continue;

                selStateIndex = annoFC.Fields.FindField("selectstate");

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "AnnotationClassID =" + fc.FeatureClassID.ToString();
                IFeatureCursor annoCursor = annoFC.Update(qf, true);
                IFeature f = null;
                while ((f = annoCursor.NextFeature()) != null)
                {
                    IAnnotationFeature2 annoFe = f as IAnnotationFeature2;
                    if (!fid2Sel.ContainsKey(annoFe.LinkedFeatureID) || fid2Sel[annoFe.LinkedFeatureID])
                        continue;

                    if (selStateIndex != -1)
                    {
                        f.set_Value(selStateIndex, "POI选取");//设置为未选取状态
                        annoCursor.UpdateFeature(f);
                    }
                    else
                    {
                        annoCursor.DeleteFeature();
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(annoCursor);
            }

            
            #endregion
        }

        /// <summary>
        /// 顾及道路关系的POI选取
        /// </summary>
        /// <param name="fid2Point"></param>
        /// <param name="rate"></param>
        /// <param name="weightScale">水路交叉口处POI的权重比例</param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public Dictionary<int, bool> SelPOIByTin(Dictionary<int, IPoint> fid2Point, double rate, int weightScale, WaitOperation wo)
        {
            Dictionary<int, bool> result = new Dictionary<int, bool>();

            MultipointClass mp = new MultipointClass();
            foreach (var kv in fid2Point)
            {
                mp.AddGeometry(kv.Value);

                result.Add(kv.Key, true);//初始化时，全部保留
            }
            int totalUnCount = (int)(fid2Point.Count * (1 - rate));
            int unSelCount = 0;

            //一次构tin最多去除的POI点数
            int maxSubCount = (int)(fid2Point.Count * 0.1);
            while (maxSubCount < 5 && maxSubCount < totalUnCount)
            {
                if (maxSubCount < 1)
                    maxSubCount = 1;

                maxSubCount = maxSubCount * 2;
            }
            int step = 1;
            while (unSelCount < totalUnCount)
            {
                int stepUnCount = maxSubCount * step;//本次Tin选取的最大点数

                //初始化tin
                TinClass tin = new TinClass();
                tin.InitNew(mp.Envelope);
                tin.StartInMemoryEditing();

                Dictionary<int, int> fid2Level = new Dictionary<int, int>();
                foreach (var kv in fid2Point)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在构建三角网......"));

                    if (!result[kv.Key])
                        continue;//已舍去，不参与本次构tin

                    IPoint p = kv.Value;
                    p.Z = 0;

                    fid2Level.Add(kv.Key, p.ID);

                    tin.AddPointZ(p, kv.Key);//如果三角网中该点几何已经存在（数据中存在重叠点），则该点不会添加到三角网中
                }

                //计算每个节点影响面积
                Dictionary<int, double> fid2Area = new Dictionary<int, double>();
                foreach (var kv in fid2Level)
                {
                    fid2Area.Add(kv.Key, 0);//初始化时，面积为0
                }
                IPolygon tinDataArea = tin.GetDataArea();//TIN的数据范围
                IRelationalOperator ro = tinDataArea as IRelationalOperator;
                for (int j = 1; j <= tin.NodeCount; j++)
                {
                    if (wo != null)
                        wo.SetText(string.Format("正在计算节点密度......"));

                    ITinNode node = tin.GetNode(j);
                    if (!node.IsInsideDataArea)
                        continue;

                    IPolygon voronoiPolygon = node.GetVoronoiRegion(null);
                    double nodeArea = 0;//添加权重后的节点影响面积
                    if (ro != null && !ro.Contains(voronoiPolygon))//节点影响范围部分超出了tin的数据范围，取两者相交的公共部分的面积
                    {
                        IPolygon interGeo = (tinDataArea as ITopologicalOperator).Intersect(voronoiPolygon as IGeometry, esriGeometryDimension.esriGeometry2Dimension) as IPolygon;
                        if (interGeo != null)
                        {
                            nodeArea = (interGeo as IArea).Area;
                            if (fid2Level[node.TagValue] > 0)
                            {
                                nodeArea = nodeArea * (fid2Level[node.TagValue] * weightScale);
                            }
                        }
                    }
                    else
                    {
                        nodeArea = (voronoiPolygon as IArea).Area;
                        if (fid2Level[node.TagValue] > 0)
                        {
                            nodeArea = nodeArea * (fid2Level[node.TagValue] * weightScale);
                        }
                    }
                    fid2Area[node.TagValue] = nodeArea;
                }

                //按面积排序(升序)
                fid2Area = fid2Area.OrderBy(o => o.Value).ToDictionary(p => p.Key, o => o.Value);

                //设置较小影响面积的POI为false
                foreach (var kv in fid2Area)
                {
                    if (unSelCount >= totalUnCount)
                        break;//已选取完毕

                    if (unSelCount >= stepUnCount && (result.Count - stepUnCount) > 3)
                        break;//本次tin内选取完毕

                    result[kv.Key] = false;
                    unSelCount++;
                }

                ++step;//循环次数
            }

            return result;

        }
    }
}

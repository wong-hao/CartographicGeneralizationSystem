using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.DataSourcesGDB;

namespace ShellTBDivided
{
     
    /// <summary>
    /// 小图斑融合要素类
    /// </summary>
    public class TBMergeClass
    {
        public TBMergeClass(string ruleFile,string gdbPath,string fclName)
        {
            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            mWorkSpace = workspaceFactory.OpenFromFile(gdbPath, 0);
            mGDBPath = gdbPath;
            mFclName = fclName;
            mGenRules = new GenRulesClass(ruleFile);
        }
        private GenRulesClass mGenRules = null;

        IWorkspace mWorkSpace = null;
        /// <summary>
        /// 地图比例尺
        /// </summary>
        public double MapScale = 70000;
        /// <summary>
        /// 不同类型的对应的面积指标
        /// </summary>
        public Dictionary<string, double> AreaParmsDic = new Dictionary<string, double>();

        string mGDBPath = "";
        string mDisField = "DisFID";//融合字段
        string mCCField = "DLBM";
        string mFclName = "DLTB";
        string mFclTop = "DLTBTop";
        Dictionary<int, List<int>> mTopDic = new Dictionary<int, List<int>>();
        ITable tableTop = null;
        IFeatureClass mFclTB = null;
        Action<string> mMsgAction = null;
        /// <summary>
        /// 图斑融合
        /// </summary>
        public void TBMergeExcute(double mapscale, string areaParms, Action<string> msgAction = null)
        {
            try
            {
                string[] areas = areaParms.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in areas)
                {
                    string[] kv = str.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    AreaParmsDic[kv[0]] = double.Parse(kv[1]);
                }               
                mMsgAction = msgAction;
              
                CreateTBTop();
              
                MapScale = mapscale;
                SmallPracellProcess();
               
                //删除top
                IFeatureClass fclTopLine = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclTop);//邻接关系线
                if (fclTopLine != null)
                    (fclTopLine as IDataset).Delete();
            }
            catch(Exception ex)
            {
                MessageBox.Show("图斑融合错误："+ex.Message);
            }

        }
        private void ElimateDltb(double area = 10)
        {
            if (mMsgAction != null)
            {
                mMsgAction("正在消除小图斑，面积指标：" +area.ToString());
            }
            IFeatureClass dltbFcls = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTB");
            IQueryFilter filter=new QueryFilterClass();
            filter.WhereClause= '"' + "Shape_Area" + '"' + "<=" + area;
            if (dltbFcls.FeatureCount(filter) < 10000)//数量少于10000则不进行消除
            {
                return;
            }            
           
            //创建Layer
            Geoprocessor gp = GApplication.Application.GPTool;
            gp.OverwriteOutput = true;
            gp.SetEnvironmentValue("workspace", mGDBPath);
            ESRI.ArcGIS.DataManagementTools.Eliminate pElim = new ESRI.ArcGIS.DataManagementTools.Eliminate();
            ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer pMakeFeatureLayer = new ESRI.ArcGIS.DataManagementTools.MakeFeatureLayer();
            pMakeFeatureLayer.in_features = "DLTB";
            pMakeFeatureLayer.out_layer = "DLTBElimiateLayer";
            //pMakeFeatureLayer.workspace = mFolderPath;
            gp.Execute(pMakeFeatureLayer, null);
            //通过Layer选择
            ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute pSelectLayerByAttribute = new ESRI.ArcGIS.DataManagementTools.SelectLayerByAttribute();
            pSelectLayerByAttribute.in_layer_or_view = "DLTBElimiateLayer";
            pSelectLayerByAttribute.where_clause = '"' + "Shape_Area" + '"' + "<=" + area;
            gp.Execute(pSelectLayerByAttribute, null);
            pElim.in_features = "DLTBElimiateLayer";
            
            pElim.out_feature_class = "DLTBElimate";
            gp.Execute(pElim, null);

            IDataset dltbds = dltbFcls as IDataset;
            if (dltbds.CanDelete())
            {
                dltbds.Delete();
            }
            IDataset ds = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass("DLTBElimate") as IDataset;
            if (ds != null && ds.CanRename())
                ds.Rename("DLTB");

        }
        /// <summary>
        /// 图斑拓扑
        /// </summary>
        private void CreateTBTop()
        {
            if (!(mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclTop))
            {
                MessageBox.Show("图斑左右关系不存在！");
                return;
            }

            IFeatureClass fclTopLine = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclTop);//邻接关系线

            IFeatureCursor fCursor = fclTopLine.Search(null, false);
            //IFeatureCursor fCursor = mFclTB.Search(null, false);
            IFeature feature = null;


            int indexLeftID = fclTopLine.FindField("LEFT_FID");
            int indexRightID = fclTopLine.FindField("RIGHT_FID");
            int numK = 0;
            while ((feature = fCursor.NextFeature()) != null)
            {
                  numK++;
                  if (numK % 2000 == 0)
                  {
                      if (mMsgAction != null)
                      {
                          mMsgAction("构建图斑邻接关系："+numK);
                      }
                  }
                int rid = Convert.ToInt32(feature.get_Value(indexRightID).ToString());
                int lid = Convert.ToInt32(feature.get_Value(indexLeftID).ToString());
                double len = (feature.Shape as IPolyline).Length;
                mAdjEdges.Add(feature.OID, new AdjEdgeInfo { OID = feature.OID, LeftID = lid, RightID = rid, Length = len });
                #region
                
                int LeftOID = lid;
                int RightOID = rid;

                #endregion
                //多边形的所有边
                #region
                if (LeftOID != -1 && RightOID != -1)
                {
                    #region 相邻图斑关系
                    //处理左边
                    {
                        List<int> ids = new List<int>();
                        if (mTopDic.ContainsKey(LeftOID))
                        {
                            ids = mTopDic[LeftOID];
                        }
                        if (!ids.Contains(RightOID))
                            ids.Add(RightOID);
                        mTopDic[LeftOID] = ids;
                        
                    }
                    //处理右边
                    {
                        List<int> ids = new List<int>();
                        if (mTopDic.ContainsKey(RightOID))
                        {
                            ids = mTopDic[RightOID];
                        }
                        if (!ids.Contains(LeftOID))
                            ids.Add(LeftOID);
                        mTopDic[RightOID] = ids;
                    }
                    #endregion
                    #region 图斑的关联边
                    //处理左边
                    {
                        List<int> ids = new List<int>();
                        if (mTBEges.ContainsKey(LeftOID))
                        {
                            ids = mTBEges[LeftOID];
                        }
                        if (!ids.Contains(feature.OID))
                            ids.Add(feature.OID);
                        mTBEges[LeftOID] = ids;

                    }
                    //处理右边
                    {
                        List<int> ids = new List<int>();
                        if (mTBEges.ContainsKey(RightOID))
                        {
                            ids = mTBEges[RightOID];
                        }
                        if (!ids.Contains(feature.OID))
                            ids.Add(feature.OID);
                        mTBEges[RightOID] = ids;
                    }
                    #endregion
                }
                #endregion
            }
            Marshal.ReleaseComObject(fCursor);
        }
        private Dictionary<int, string> mFeaturesTotalGB = new Dictionary<int, string>();//0101四位GB
        #region 动态边维护
        private Dictionary<int, AdjEdgeInfo> mAdjEdges = new Dictionary<int, AdjEdgeInfo>();
        private Dictionary<int,List<int>>mTBEges=new Dictionary<int,List<int>>();
        private void CreateAdjTBTop0()
        {
            if (!(mWorkSpace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, mFclTop))
            {
                MessageBox.Show("图斑左右关系不存在！");
                return;
            }

            IFeatureClass fclTopLine = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclTop);//邻接关系线

            IFeatureCursor fCursor = fclTopLine.Search(null, false);
            
            IFeature feature = null;


            int indexLeftID = fclTopLine.FindField("LEFT_FID");
            int indexRightID = fclTopLine.FindField("RIGHT_FID");
            int numK = 0;
            while ((feature = fCursor.NextFeature()) != null)
            {
                numK++;
                if (numK % 2000 == 0)
                {
                    if (mMsgAction != null)
                    {
                        mMsgAction("构建图斑邻接关系：" + numK);
                    }
                }
                double len=(feature.Shape as IPolyline).Length;
                int rid = Convert.ToInt32(feature.get_Value(indexRightID).ToString());
                int lid = Convert.ToInt32(feature.get_Value(indexLeftID).ToString());
                mAdjEdges.Add(feature.OID, new AdjEdgeInfo { OID = feature.OID, LeftID = lid, RightID = rid, Length = len });
                #region

                int LeftOID = lid;
                int RightOID = rid;

                #endregion
                //多边形的所有边
                #region
                if (LeftOID != -1 && RightOID != -1)
                {
                    //处理左边
                    {
                        List<int> ids = new List<int>();
                        if (mTBEges.ContainsKey(LeftOID))
                        {
                            ids = mTBEges[LeftOID];
                        }
                        if (!ids.Contains(feature.OID))
                            ids.Add(feature.OID);
                        mTBEges[LeftOID] = ids;

                    }
                    //处理右边
                    {
                        List<int> ids = new List<int>();
                        if (mTBEges.ContainsKey(RightOID))
                        {
                            ids = mTBEges[RightOID];
                        }
                        if (!ids.Contains(feature.OID))
                            ids.Add(feature.OID);
                        mTBEges[RightOID] = ids;
                    }
                }
                #endregion
            }
            Marshal.ReleaseComObject(fCursor);
        }
        private void ChangeTBTop(int id,int mergeFID)
        {
            int mergeID = mDisFidDic[mergeFID];
            List<int> bigEdges = mTBEges[mergeID];
            List<int> smallEdges = mTBEges[id];
            //修改边信息
            foreach (var edgeID in mTBEges[id])
            {
                AdjEdgeInfo info = mAdjEdges[edgeID];
                info.LeftID = info.LeftID == id ? mergeID : info.LeftID;
                info.RightID = info.RightID == id ? mergeID : info.RightID;
                if (info.LeftID == info.RightID)
                {
                    bigEdges.Remove(edgeID);                  
                }
                else
                {
                    bigEdges.Add(edgeID);
                }
            }
           
            mTBEges.Remove(id);
            mTBEges[mergeID] = bigEdges;
            
        }

        private double AdjLength(int id, int mergeFID, List<int> adjEdges)
        {
            double len = 0;
            int mergeID = mDisFidDic[mergeFID];
            foreach (int edgeID in adjEdges)
            {
                AdjEdgeInfo info = mAdjEdges[edgeID];
                if (info.LeftID == id && info.RightID == mergeID)
                    len += info.Length;
                if (info.RightID == id && info.LeftID == mergeID)
                    len += info.Length;
            }
            return len;
        }
        private int SearchTopFeature(int oid, List<int> smallFeatures,bool inSmall=true)
        {
            int indexMax = 0;
            if (!mTopDic.ContainsKey(oid))//1孤立图斑
                return indexMax;
            if (!mTBEges.ContainsKey(oid))//2.已处理被删除的不足
                return indexMax;
            IFeature smallFeature = mFclTB.GetFeature(oid);
            int disIDSelf = mDisFidDic[oid];
            //查找最长边     
            #region
            string ccSelf = mFeaturesGB[oid];
            var dic = mGenRules.TBGenRules;
            TBGenRule tbRule = dic[ccSelf];
            Dictionary<int, TBRule> dicrule = tbRule.rules;
            Dictionary<int, TBRule> sortDic = dicrule.OrderBy(t => t.Key).ToDictionary(p => p.Key, p => p.Value); //level->系列规则
            Dictionary<int, int> dicOID = new Dictionary<int, int>();//level->OID:每个等级 对应ID
            Dictionary<int, double> dicArea = new Dictionary<int, double>();//level->Length 每个等级 对应长度
            foreach (int edgeID in mTBEges[oid])
            {
                AdjEdgeInfo info=mAdjEdges[edgeID];
                int feid = info.LeftID == oid ? info.RightID : info.LeftID;
                if (!mDisFidDic.ContainsKey(feid))
                {
                    Console.WriteLine("修改拓扑时候，导致没有边：" + feid);
                    continue;
                }
                int disID = mDisFidDic[feid];
                if (disIDSelf == disID)
                    continue;
                string cc = mFeaturesGB[feid];
                if (cc == "") continue;//2.空CC码，此处不予考虑
                //3.是否在小图斑内搜索..
                if (inSmall)
                {
                    if (!smallFeatures.Contains(feid))
                        continue;
                }
                else
                {
                    if (smallFeatures.Contains(feid))
                        continue;
                }
                double geometryLength = AdjLength(oid,feid,mTBEges[oid]);
                #region 最大边长的要素索引
                bool flag = false;
                foreach (var levelkv in sortDic)
                {
                    var mergeDic = levelkv.Value.MergeDic;
                    if (!dicOID.ContainsKey(levelkv.Key))
                        dicOID[levelkv.Key] = 0;
                    if (!dicArea.ContainsKey(levelkv.Key))
                        dicArea[levelkv.Key] = 0;
                    foreach (var megerFe in mergeDic)
                    {
                        if (cc == megerFe.Key.ToString())
                        {
                            if (dicArea[levelkv.Key] < geometryLength)
                            {
                                dicArea[levelkv.Key] = geometryLength;
                                dicOID[levelkv.Key] = feid;
                            }
                            flag = true;
                            break;

                        }

                    }
                    if (flag)
                        break;
                }
                #endregion

            }
            foreach (var mergeIDKv in dicOID)
            {
                if (mergeIDKv.Value != 0)
                {
                    indexMax = mergeIDKv.Value;
                    break;
                }
            }

            #endregion
            return indexMax;
        }
        #endregion

        ///ID-> GB 
        Dictionary<int, string> mFeaturesGB = new Dictionary<int, string>();//01两位GB
        
        ///ID->Area
        Dictionary<int, double> mFeaturesArea = new Dictionary<int, double>();
        ///DisID->Area
        Dictionary<int, double> mDisArea = new Dictionary<int, double>();

        ///ID->DisID
        Dictionary<int, int> mDisFidDic = new Dictionary<int, int>();//小图斑要素的oid→合并到的对象图斑oid

        

        private void SmallPracellProcess()
        {
            //刷选面积不足的图斑
            try
            {
                Dictionary<int, double> smallFeAreas = new Dictionary<int, double>();//不够指标的要素
                Dictionary<int, List<int>> fesMergInfos = new Dictionary<int, List<int>>();//统一融合的要素
                #region
                mFclTB = (mWorkSpace as IFeatureWorkspace).OpenFeatureClass(mFclName);
                AddField(mFclTB, "DisFID");
                double areaMinUnit = 0.000001 * MapScale * MapScale;
                List<int> smallFeatures = new List<int>();//不够指标的要素
                int ccIndex = mFclTB.FindField(mCCField);//地类编码

                int num = 0;

                #region 筛选出满足条件（不够指标）的要素
                IFeatureCursor fCursor = mFclTB.Update(null, false);
                //IFeatureCursor fCursor = mFclTB.Search(null, false);
                IFeature pFeature = null;
                string areaField = mFclTB.AreaField.Name;
                int areaIndex = mFclTB.FindField(areaField);
                int disIndex = mFclTB.FindField(mDisField);
                while ((pFeature = fCursor.NextFeature()) != null)
                {
                    num++;
                    string ccCode = pFeature.get_Value(ccIndex).ToString().Trim();
                    mFeaturesTotalGB.Add(pFeature.OID, ccCode);
                    try
                    {
                        ccCode = ccCode.Substring(0, 2);
                    }
                    catch
                    {
                        continue;
                    }
                    pFeature.set_Value(disIndex, pFeature.OID);
                    fCursor.UpdateFeature(pFeature);

                    if (num % 2000 == 0)
                    {
                        if (mMsgAction != null)
                        {
                            mMsgAction("正在刷选不足指标图斑:" + num);
                        }
                    }
                    if (ccCode == string.Empty)
                    {
                        mFeaturesGB.Add(pFeature.OID, "");
                        continue;
                    }
                    mFeaturesGB.Add(pFeature.OID, ccCode);

                    double area = Convert.ToDouble(pFeature.get_Value(areaIndex).ToString().Trim());
                    mFeaturesArea.Add(pFeature.OID, area);
                    int disFID = pFeature.OID;
                    mDisArea[disFID] = area;
                    mDisFidDic[pFeature.OID] = disFID;
                    //综合条件判断
                    if (AreaParmsDic.ContainsKey(ccCode))
                    {
                        double val = AreaParmsDic[ccCode];
                        if (val > 0)
                        {
                            if (area <= val * areaMinUnit)
                            {
                                smallFeAreas[pFeature.OID] = area;

                            }

                        }
                    }



                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                #endregion
                GC.Collect();
                #endregion
                smallFeatures.AddRange(smallFeAreas.Keys.ToArray());
                if (mMsgAction != null)
                {
                    mMsgAction("小图斑自身先进行处理");
                }



                //小图斑自身先进行处理
                Dictionary<int, double> sortAreas = smallFeAreas.OrderBy(t => t.Value).ToDictionary(p => p.Key, p => p.Value);
                var geometryIDList = new List<int>();
                geometryIDList.AddRange(sortAreas.Keys.ToArray());


                //小图斑自身先进行处理前，周围如果有大图斑且属于同一类则先合并wjz
                geometryIDList = MergeSameTypeLand(geometryIDList);


                //geometryIDList = MergeSmallLand(geometryIDList);//小图斑自身先进行处理
                List<int> restFeaureID = new List<int>();
                bool flagAlone = false;//是否退出循环
                do
                {
                    flagAlone = false;
                    int numK = 0;
                    foreach (int key in geometryIDList)
                    {
                        numK++;
                        if (numK % 50 == 0)
                        {
                            if (mMsgAction != null)
                            {
                                mMsgAction("小图斑进行处理，当前:" + numK + "," + geometryIDList.Count);
                            }
                        }
                        //找到相邻面积最大ID
                        int indexMax = 0;
                        int disIDSelf = mDisFidDic[key];

                        #region 判断是否已经足够指标
                        {
                            double area = mDisArea[disIDSelf];
                            string ccCode = mFeaturesGB[key];
                            if (AreaParmsDic.ContainsKey(ccCode))
                            {
                                double val = AreaParmsDic[ccCode];
                                if (val > 0)
                                {
                                    if (area >= val * areaMinUnit)//进行了融合后面积依然不够指标
                                        continue;
                                }
                            }
                        }
                        #endregion
                        //indexMax = SearchMergeFeature1(key, geometryIDList);
                        indexMax = SearchTopFeature(key, geometryIDList, false);
                        if (indexMax == 0)
                        {
                            restFeaureID.Add(key);//剩余的小图斑纹
                            continue;
                        }
                        try
                        {
                            //....
                            ChangeTBTop(key, indexMax);
                            int disID = mDisFidDic[indexMax];
                            mDisArea[disID] += mDisArea[disIDSelf];
                            //string cc = mFeaturesGB[indexMax];
                            //cc = mFclTB.GetFeature(indexMax).get_Value(ccIndex).ToString();//true  GB 
                            string cc = mFeaturesTotalGB[indexMax];
                            var disIDs = mDisFidDic.Where(t => t.Value == disIDSelf).ToDictionary(p => p.Key, p => p.Value);
                            foreach (var kv in disIDs)
                            {
                                if (kv.Key != indexMax)
                                {
                                    IQueryFilter qf = new QueryFilterClass();
                                    qf.WhereClause = "ObjectID = " + kv.Key;
                                    IFeature feature;
                                    IFeatureCursor fcursor = mFclTB.Update(qf, false);
                                    while ((feature = fcursor.NextFeature()) != null)
                                    {
                                        feature.set_Value(disIndex, disID);
                                        feature.set_Value(ccIndex, cc);
                                        fcursor.UpdateFeature(feature);
                                        mDisFidDic[kv.Key] = disID;
                                    }
                                    Marshal.ReleaseComObject(fcursor);

                                }

                            }

                        }
                        catch
                        {
                        }

                    }
                    int fesNums = geometryIDList.Count;//当前处理个数
                    geometryIDList = restFeaureID;//下一步需要处理
                    #region//是否全部是孤立图斑
                    //是否退出循环
                    //判断GeometryIDList是否全部是孤岛
                    if (fesNums == restFeaureID.Count)
                    {
                        flagAlone = true;
                    }
                    if (!flagAlone)
                        restFeaureID = new List<int>();
                    #endregion
                }
                while (!flagAlone);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }



        }
        //小图斑自身融合
        private List<int> MergeSmallLand(List<int> smallLandIDs)
        {
            double areaMinUnit = 0.000001 * MapScale * MapScale;
            List<int> smallLands = new List<int>();
            //融合查找处理
            int numK = 0;
            int disIndex = mFclTB.FindField(mDisField);
            int ccIndex = mFclTB.FindField(mCCField);
            foreach (int key in smallLandIDs)
            {
              
                numK++;
                if (numK % 100 == 0)
                {
                    mMsgAction("小图斑自身先进行处理:当前" + numK + "/" + smallLandIDs.Count);
                    
                }
                IFeature smallFeature = mFclTB.GetFeature(key);
                int disIDSelf = mDisFidDic[key];
                
                #region 判断是否已经足够指标
                {
                    double area = mDisArea[disIDSelf];
                    string ccCode = mFeaturesGB[key];
                    if (AreaParmsDic.ContainsKey(ccCode))
                    {
                        double val = AreaParmsDic[ccCode];
                        if (val > 0)
                        {
                            if (area >= val * areaMinUnit)//进行了融合后面积依然不够指标
                                continue;
                        }
                    }
                }
                #endregion
               // int indexMax = SearchSmallMergeFeature1(key, smallLandIDs);//根据规则TBGeneralizeRule.xml进行融合
                int indexMax = SearchTopFeature(key, smallLandIDs);
                if (indexMax == 0) //周围没有合适的临近图斑 
                {
                    continue;
                }
                //要素融合
                #region
                try
                {

                    ChangeTBTop(key, indexMax);
                    int disID = mDisFidDic[indexMax];
                    //update area
                    mDisArea[disID] += mDisArea[disIDSelf];
                    //更新所有 disIDSelf
                    string cc = mFeaturesGB[indexMax];
                    cc = mFclTB.GetFeature(indexMax).get_Value(ccIndex).ToString();//true  GB 
                    var disIDs = mDisFidDic.Where(t => t.Value == disIDSelf).ToDictionary(p => p.Key, p => p.Value);
                    foreach (var kv in disIDs)
                    {
                        if (kv.Key != indexMax)
                        {
                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = "ObjectID = " + kv.Key;
                            IFeature feature;
                            IFeatureCursor fcursor = mFclTB.Update(qf, false);
                            while ((feature = fcursor.NextFeature()) != null)
                            {
                                feature.set_Value(disIndex, disID);
                                feature.set_Value(ccIndex, cc);
                                fcursor.UpdateFeature(feature);
                                mDisFidDic[kv.Key] = disID;
                            }
                            Marshal.ReleaseComObject(fcursor);
                           
                           

                        }

                    }


                   
                   
                  
                }
                catch
                {
                }
                #endregion
            }
           

            foreach (var value in smallLandIDs)
            {   
                int disID = mDisFidDic[value];
                double area = mDisArea[disID];
                string ccCode = mFeaturesGB[value];
                if (AreaParmsDic.ContainsKey(ccCode))
                {
                    double val = AreaParmsDic[ccCode];
                    if (val > 0)
                    {
                        if (area <= val * areaMinUnit)//进行了融合后面积依然不够指标
                            smallLands.Add(value);
                    }
                }

               
            }
            return smallLands;

        }


        //同类型图斑合并，同类型小图斑和大图斑或者小图斑之间都可以合并
        private List<int> MergeSameTypeLand(List<int> smallLandIDs)
        {
            double areaMinUnit = 0.000001 * MapScale * MapScale;
            List<int> smallLands = new List<int>();
            //融合查找处理
            int numK =0;
            int disIndex = mFclTB.FindField(mDisField);
            int ccIndex = mFclTB.FindField(mCCField);
            foreach (int key in smallLandIDs)
            {

                numK++;
                if (numK % 100 == 0)
                {
                    mMsgAction("小图斑同类先进行处理:当前" + numK + "/" + smallLandIDs.Count);

                }
                IFeature smallFeature = mFclTB.GetFeature(key);
                int disIDSelf = mDisFidDic[key];
                
                #region 判断是否已经足够指标
                {
                    double area = mDisArea[disIDSelf];
                    string ccCode = mFeaturesGB[key];
                    if (AreaParmsDic.ContainsKey(ccCode))
                    {
                        double val = AreaParmsDic[ccCode];
                        if (val > 0)
                        {
                            if (area >= val * areaMinUnit)//进行了融合后面积依然不够指标
                                continue;
                        }
                    }
                }
                #endregion
               // int indexSame = SearchSameTypeFeature(key);//返回图斑周围与其相邻的同类型图斑的oid
                int indexSame = SearchSameTypeTopFeature(key);  
                if (indexSame == -1) //周围没有合适的临近图斑 
                {
                    continue;
                }
                //要素融合
                #region
                try
                {
                    ChangeTBTop(key, indexSame);
                    int disID = mDisFidDic[indexSame];
                    //update area
                    mDisArea[disID] += mDisArea[disIDSelf];
                    //更新所有 disIDSelf
                    //string cc = mFeaturesGB[indexSame];
                    //cc = mFclTB.GetFeature(indexSame).get_Value(ccIndex).ToString();//true  GB 
                    string cc = mFeaturesTotalGB[indexSame];
                    var disIDs = mDisFidDic.Where(t => t.Value == disIDSelf).ToDictionary(p => p.Key, p => p.Value);
                    foreach (var kv in disIDs)
                    {
                        if (kv.Key != indexSame)
                        {
                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = "ObjectID = " + kv.Key;
                            IFeature feature;
                            IFeatureCursor fcursor = mFclTB.Update(qf, false);
                            while ((feature = fcursor.NextFeature()) != null)
                            {
                                feature.set_Value(disIndex, disID);
                                feature.set_Value(ccIndex, cc);
                                fcursor.UpdateFeature(feature);
                                mDisFidDic[kv.Key] = disID;
                            }
                            Marshal.ReleaseComObject(fcursor);
                        }
                    }





                }
                catch
                {
                }
                #endregion
            }


            foreach (var value in smallLandIDs)
            {
                int disID = mDisFidDic[value];
                double area = mDisArea[disID];
                string ccCode = mFeaturesGB[value];
                if (AreaParmsDic.ContainsKey(ccCode))
                {
                    double val = AreaParmsDic[ccCode];
                    if (val > 0)
                    {
                        if (area <= val * areaMinUnit)//进行了融合后面积依然不够指标
                            smallLands.Add(value);
                    }
                }


            }
            return smallLands;

        }

        private void AddField(IFeatureClass fCls, string fieldName)
        {
            int index = fCls.FindField(fieldName);
            if (index != -1)
            {
                return;
            }
            IFields pFields = fCls.Fields;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.AliasName_2 = fieldName;
            pFieldEdit.Length_2 = 1;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            IClass pTable = fCls as IClass;
            pTable.AddField(pField);
            pFieldsEdit = null;
            pField = null;
        }
        private int SearchSmallMergeFeature(int oid, List<int> smallFeatures)
        {
           
            IFeature smallFeature = mFclTB.GetFeature(oid);
            int disIDSelf = mDisFidDic[oid];
            IGeometry geoCopy = smallFeature.ShapeCopy;
            #region
            IFeature fe;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.WhereClause = mDisField + " <> " + disIDSelf;
            sf.Geometry = geoCopy;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor cursor = mFclTB.Search(sf, true);
            //查找相邻的最大面积要素索引
            string ccSelf = mFeaturesGB[oid];
            var dic = mGenRules.TBGenRules;
            TBGenRule tbRule = dic[ccSelf];
            Dictionary<int, TBRule> dicrule = tbRule.rules;
            Dictionary<int, TBRule> sortDic = dicrule.OrderBy(t => t.Key).ToDictionary(p => p.Key, p => p.Value); //level->系列规则
            Dictionary<int, int> dicOID = new Dictionary<int, int>();//level->OID
            Dictionary<int, double> dicArea = new Dictionary<int, double>();//level->Area
            int indexMax = 0;
            while ((fe = cursor.NextFeature()) != null)
            {
                int feid = fe.OID;
                int disID = mDisFidDic[feid];
                if (disIDSelf == disID)
                    continue;
                string cc = mFeaturesGB[feid];
                if (cc == "") continue;//2.空CC码，此处不予考虑
                if (!smallFeatures.Contains(feid))
                    continue;
                double geometryArea = mDisArea[disID];
                #region 最大面积的要素索引
                bool flag = false;
                foreach (var levelkv in sortDic)
                {
                    var mergeDic = levelkv.Value.MergeDic;
                    if (!dicOID.ContainsKey(levelkv.Key))
                        dicOID[levelkv.Key] = 0;
                    if (!dicArea.ContainsKey(levelkv.Key))
                        dicArea[levelkv.Key] = 0;
                    foreach (var megerFe in mergeDic)
                    {
                        if (cc == megerFe.Key.ToString())
                        {
                            if (dicArea[levelkv.Key] < geometryArea)
                            {
                                dicArea[levelkv.Key] = geometryArea;
                                dicOID[levelkv.Key] = feid;
                            }
                            flag = true;
                            break;

                        }

                    }
                    if (flag)
                        break;
                }
                #endregion

            }
            foreach (var mergeIDKv in dicOID)
            {
                if (mergeIDKv.Value != 0)
                {
                    indexMax = mergeIDKv.Value;
                    break;
                }
            }
            Marshal.ReleaseComObject(cursor);
            Marshal.ReleaseComObject(geoCopy);
            #endregion
            return indexMax;
        }

        private int SearchMergeFeature(int oid, List<int> smallFeatures)
        {
            IFeature smallFeature = mFclTB.GetFeature(oid);
            int disIDSelf = mDisFidDic[oid];
            IGeometry geoCopy = smallFeature.ShapeCopy;
            #region
            IFeature fe;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.WhereClause = mDisField + " <> " + disIDSelf; ;
            sf.Geometry = geoCopy;
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor cursor = mFclTB.Search(sf, true);
            //查找相邻的最大面积要素索引
            string ccSelf = mFeaturesGB[oid];
            var dic = mGenRules.TBGenRules;
            TBGenRule tbRule = dic[ccSelf];
            Dictionary<int, TBRule> dicrule = tbRule.rules;
            Dictionary<int, TBRule> sortDic = dicrule.OrderBy(t => t.Key).ToDictionary(p => p.Key, p => p.Value); //level->系列规则
            Dictionary<int, int> dicOID = new Dictionary<int, int>();//level->OID
            Dictionary<int, double> dicArea = new Dictionary<int, double>();//level->Area
            int indexMax = 0;
            while ((fe = cursor.NextFeature()) != null)
            {
                int feid = fe.OID;
                int disID = mDisFidDic[feid];
                if (disIDSelf == disID)
                    continue;
                string cc = mFeaturesGB[feid];
                if (cc == "") continue;//2.空CC码，此处不予考虑
                if (smallFeatures.Contains(feid))
                    continue;
                double geometryArea = mDisArea[disID];
                #region 最大面积的要素索引
                bool flag = false;
                foreach (var levelkv in sortDic)
                {
                    var mergeDic = levelkv.Value.MergeDic;
                    if (!dicOID.ContainsKey(levelkv.Key))
                        dicOID[levelkv.Key] = 0;
                    if (!dicArea.ContainsKey(levelkv.Key))
                        dicArea[levelkv.Key] = 0;
                    foreach (var megerFe in mergeDic)
                    {
                        if (cc == megerFe.Key.ToString())
                        {
                            if (dicArea[levelkv.Key] < geometryArea)
                            {
                                dicArea[levelkv.Key] = geometryArea;
                                dicOID[levelkv.Key] = feid;
                            }
                            flag = true;
                            break;

                        }

                    }
                    if (flag)
                        break;
                }
                #endregion

            }
            foreach (var mergeIDKv in dicOID)
            {
                if (mergeIDKv.Value != 0)
                {
                    indexMax = mergeIDKv.Value;
                    break;
                }
            }
            Marshal.ReleaseComObject(cursor);
            Marshal.ReleaseComObject(geoCopy);
            #endregion
            return indexMax;
        }

        /// <summary>
        /// 拓扑方式
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="smallFeatures"></param>
        /// <returns></returns>
        private int SearchSmallMergeFeature1(int oid, List<int> smallFeatures)
        {
            IFeature smallFeature = mFclTB.GetFeature(oid);
            int disIDSelf = mDisFidDic[oid];

            #region
            IFeature fe;

            //查找相邻的最大面积要素索引
            string ccSelf = mFeaturesGB[oid];
            var dic = mGenRules.TBGenRules;
            TBGenRule tbRule = dic[ccSelf];
            Dictionary<int, TBRule> dicrule = tbRule.rules;
            Dictionary<int, TBRule> sortDic = dicrule.OrderBy(t => t.Key).ToDictionary(p => p.Key, p => p.Value); //level->系列规则
            Dictionary<int, int> dicOID = new Dictionary<int, int>();//level->OID
            Dictionary<int, double> dicArea = new Dictionary<int, double>();//level->Area
            int indexMax = 0;
            if (!mTopDic.ContainsKey(oid))//孤立图斑
                return indexMax;
            foreach (int feid in mTopDic[oid])
            {
                int disID = mDisFidDic[feid];
                if (disIDSelf == disID)
                    continue;
                string cc = mFeaturesGB[feid];
                if (cc == "") continue;//2.空CC码，此处不予考虑
                if (!smallFeatures.Contains(feid))
                    continue;
                double geometryArea = mDisArea[disID];
                #region 最大面积的要素索引
                bool flag = false;
                foreach (var levelkv in sortDic)
                {
                    var mergeDic = levelkv.Value.MergeDic;
                    if (!dicOID.ContainsKey(levelkv.Key))
                        dicOID[levelkv.Key] = 0;
                    if (!dicArea.ContainsKey(levelkv.Key))
                        dicArea[levelkv.Key] = 0;
                    foreach (var megerFe in mergeDic)
                    {
                        if (cc == megerFe.Key.ToString())
                        {
                            if (dicArea[levelkv.Key] < geometryArea)
                            {
                                dicArea[levelkv.Key] = geometryArea;
                                dicOID[levelkv.Key] = feid;
                            }
                            flag = true;
                            break;

                        }

                    }
                    if (flag)
                        break;
                }
                #endregion

            }
            foreach (var mergeIDKv in dicOID)
            {
                if (mergeIDKv.Value != 0)
                {
                    indexMax = mergeIDKv.Value;
                    break;
                }
            }

            #endregion
            return indexMax;
        }
        /// <summary>
        /// 拓扑方式
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="smallFeatures"></param>
        /// <returns></returns>
        private int SearchSameTypeFeature(int oid)
        {
            IFeature smallFeature = mFclTB.GetFeature(oid);
            int disIDSelf = mDisFidDic[oid];

            #region
  
            //查找相邻的相同类型的图斑
            string ccSelf = mFeaturesGB[oid];
           string totalccSelf = mFeaturesTotalGB[oid];
            var dic = mGenRules.TBGenRules;
            TBGenRule tbRule = dic[ccSelf];
            Dictionary<int, TBRule> dicrule = tbRule.rules;
            Dictionary<int, TBRule> sortDic = dicrule.OrderBy(t => t.Key).ToDictionary(p => p.Key, p => p.Value); //level->系列规则
           
            Dictionary<int, double> dicArea = new Dictionary<int, double>();//oid->Area
            double maxArea=0;
            int indexMax = 0;
            if (!mTopDic.ContainsKey(oid))//孤立图斑
                return indexMax;
            foreach (int feid in mTopDic[oid])
            {
                int disID = mDisFidDic[feid];
                if (disIDSelf == disID)
                    continue;
                string cc = mFeaturesGB[feid];
                string totalcc = mFeaturesTotalGB[feid];

                if (cc == "" || totalcc == "") continue;//2.空CC码，此处不予考虑
                if (totalcc == totalccSelf)//如果DLBM完全一致，则返回该oid
                {
                    return feid;
                }
                if (cc == ccSelf)//如果一级类相同，记录下面积，返回最大面积的oid返回
                {
                    dicArea.Add(feid, mDisArea[feid]);
                }
            }
            foreach (var dicAreaItem in dicArea)
            {
                if (dicAreaItem.Value > maxArea)
                {
                    indexMax = dicAreaItem.Key;
                    maxArea = dicAreaItem.Value;
                }
            }
            #endregion
            return indexMax;
        }
        private int SearchSameTypeTopFeature(int oid)
        {
            int indexMax = -1;
            try
            {
                IFeature smallFeature = mFclTB.GetFeature(oid);
                int disIDSelf = mDisFidDic[oid];

                #region

                //查找相邻的相同类型的图斑
                string ccSelf = mFeaturesGB[oid];
                string totalccSelf = mFeaturesTotalGB[oid];
                var dic = mGenRules.TBGenRules;
                TBGenRule tbRule = dic[ccSelf];
                Dictionary<int, TBRule> dicrule = tbRule.rules;
                Dictionary<int, TBRule> sortDic = dicrule.OrderBy(t => t.Key).ToDictionary(p => p.Key, p => p.Value); //level->系列规则

                Dictionary<int, double> dicLength = new Dictionary<int, double>();//oid->Area
                double maxArea = 0;
               
                if (!mTopDic.ContainsKey(oid))//孤立图斑
                    return indexMax;
                foreach (int feid in mTopDic[oid])
                {
                    if (!mDisFidDic.ContainsKey(feid))//在修改拓扑时候，导致没有
                    {
                        Console.WriteLine("修改拓扑时候，导致没有边："+feid);
                        continue;
                    }
                    int disID = mDisFidDic[feid];
                    if (disIDSelf == disID)
                        continue;
                    string cc = mFeaturesGB[feid];
                    string totalcc = mFeaturesTotalGB[feid];

                    if (cc == "" || totalcc == "") 
                        continue;//2.空CC码，此处不予考虑
                    if (totalcc == totalccSelf)//如果DLBM完全一致，则返回该oid
                    {
                        return feid;
                    }
                    if (cc == ccSelf)//如果一级类相同，记录下面积，返回最大面积的oid返回
                    {
                        //找到公共边
                        double len = AdjLength(oid, feid, mTBEges[oid]);
                        dicLength.Add(feid, len);
                    }
                }
                foreach (var dicAreaItem in dicLength)
                {
                    if (dicAreaItem.Value > maxArea)
                    {
                        indexMax = dicAreaItem.Key;
                        maxArea = dicAreaItem.Value;
                    }
                }
                #endregion
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return indexMax;
        }
        /// <summary>
        /// 拓扑方式
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="smallFeatures"></param>
        /// <returns></returns>
        private int SearchMergeFeature1(int oid, List<int> smallFeatures)
        {
            IFeature smallFeature = mFclTB.GetFeature(oid);
            int disIDSelf = mDisFidDic[oid];

            #region

            //查找相邻的最大面积要素索引
            string ccSelf = mFeaturesGB[oid];
            var dic = mGenRules.TBGenRules;
            TBGenRule tbRule = dic[ccSelf];
            Dictionary<int, TBRule> dicrule = tbRule.rules;
            Dictionary<int, TBRule> sortDic = dicrule.OrderBy(t => t.Key).ToDictionary(p => p.Key, p => p.Value); //level->系列规则
            Dictionary<int, int> dicOID = new Dictionary<int, int>();//level->OID
            Dictionary<int, double> dicArea = new Dictionary<int, double>();//level->Area
            int indexMax = 0;
            if (!mTopDic.ContainsKey(oid))//孤立图斑
                return indexMax;
            foreach (int feid in mTopDic[oid])
            {

                int disID = mDisFidDic[feid];
                if (disIDSelf == disID)
                    continue;
                string cc = mFeaturesGB[feid];
                if (cc == "") continue;//2.空CC码，此处不予考虑
                if (smallFeatures.Contains(feid))
                    continue;
                double geometryArea = mDisArea[disID];
                #region 最大面积的要素索引
                bool flag = false;
                foreach (var levelkv in sortDic)
                {
                    var mergeDic = levelkv.Value.MergeDic;
                    if (!dicOID.ContainsKey(levelkv.Key))
                        dicOID[levelkv.Key] = 0;
                    if (!dicArea.ContainsKey(levelkv.Key))
                        dicArea[levelkv.Key] = 0;
                    foreach (var megerFe in mergeDic)
                    {
                        if (cc == megerFe.Key.ToString())
                        {
                            if (dicArea[levelkv.Key] < geometryArea)
                            {
                                dicArea[levelkv.Key] = geometryArea;
                                dicOID[levelkv.Key] = feid;
                            }
                            flag = true;
                            break;

                        }

                    }
                    if (flag)
                        break;
                }
                #endregion

            }
            foreach (var mergeIDKv in dicOID)
            {
                if (mergeIDKv.Value != 0)
                {
                    indexMax = mergeIDKv.Value;
                    break;
                }
            }

            #endregion
            return indexMax;
        }
      

       

    }

    sealed class AdjEdgeInfo
    {
        public int OID;
        public int LeftID;
        public int RightID;
        public double Length;
    }
   
}

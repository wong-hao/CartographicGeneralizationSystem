using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
namespace ShellTBDivided
{
    /// <summary>
    /// 图斑剖分融合:多进程剖分处理
    /// </summary>
    public class TBDivideClass
    {
        string mSourceGDB = string.Empty;
        string mFclName = string.Empty;
        string mRoadField = "RoadID";
        string mDisField = "DisFID";
        string mGBField = "DLBM";
        string mFclTop = "DLTBTop";
        IQueryFilter mQuery = new QueryFilterClass();
        string mDefaultGB = "";
        string mSql = "((DLBM like '1001%') or (DLBM like '1002%') or (DLBM like '1004%') or(DLBM like '1003%') or (DLBM like '1006%'))";
        public TBDivideClass(string sourceGDB, string sql, string fclName = "DLTB")
        {
            mSourceGDB = sourceGDB;
            mSql = sql;
            mFclName = fclName;
            if (mSql.Contains("1001"))
            {
                mDefaultGB = "1001";
            }
            else
            {
                mDefaultGB = "1101";
            }
        }
        public TBDivideClass(string sourceGDB, string fclName = "DLTB")
        {
            mSourceGDB = sourceGDB;
            mFclName = fclName;
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
            // pFieldEdit.Length_2 = 1;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            IClass pTable = fCls as IClass;
            pTable.AddField(pField);
            pFieldsEdit = null;
            pField = null;
        }
        /// <summary>
        /// step 1
        /// </summary>
        /// <param name="tempGDBPath"></param>
        /// <param name="fcSplit"></param>
        public void TBCenterLinePre(string tempGDBPath, string fcSplit)
        {
            try
            {
                Console.WriteLine("正在预处理....");

                IQueryFilter qf = new QueryFilterClass();
                string msg = string.Empty;
                qf.WhereClause = mSql;
                IWorkspaceFactory wf = new FileGDBWorkspaceFactoryClass();
                var fs = wf.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace;
                var fclTB = (fs as IFeatureWorkspace).OpenFeatureClass(mFclName);
                ISpatialReference sfTarget = (fclTB as IGeoDataset).SpatialReference;
                string nameFcl = "LRDLCENTERLINE";
                if ((fs as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "LRDLCENTERLINE"))
                {
                    var ds = (fs as IFeatureWorkspace).OpenFeatureClass(nameFcl) as IDataset;
                    ds.Delete();
                }
                {
                    #region
                    IField oField = new FieldClass();
                    IFields oFields = new FieldsClass();
                    IFieldsEdit oFieldsEdit = null;
                    IFieldEdit oFieldEdit = null;
                    IFeatureClass oFeatureClass = null;

                    try
                    {
                        oFieldsEdit = oFields as IFieldsEdit;
                        oFieldEdit = oField as IFieldEdit;

                        IGeometryDef geometryDef = new GeometryDefClass();
                        IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                        geometryDefEdit.AvgNumPoints_2 = 5;
                        geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
                        geometryDefEdit.GridCount_2 = 1;
                        geometryDefEdit.HasM_2 = false;
                        geometryDefEdit.HasZ_2 = false;
                        geometryDefEdit.SpatialReference_2 = sfTarget;
                        oFieldEdit.Name_2 = "SHAPE";
                        oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                        oFieldEdit.GeometryDef_2 = geometryDef;
                        oFieldEdit.IsNullable_2 = true;
                        oFieldEdit.Required_2 = true;
                        oFieldsEdit.AddField(oField);
                        oFeatureClass = (fs as IFeatureWorkspace).CreateFeatureClass(nameFcl, oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
                    }
                    catch
                    {
                    }
                    #endregion
                }

                Console.WriteLine("正在预处理【初始化提取中心线临时图层】....");
                string divFileName = Application.StartupPath + "//DivItems.txt";
                if (System.IO.File.Exists(divFileName))
                {
                    System.IO.File.Delete(divFileName);
                }
                var feCusor = fclTB.Search(qf, false);
                Console.WriteLine("正在预处理【刷选狭长图斑】....");
                int count = 1;
                IFeature fe = null;
                while ((fe = feCusor.NextFeature()) != null)
                {
                    count++;
                    if (count % 1000 == 0)
                        Console.WriteLine("正在预处理【刷选道路图斑】" + count + "....");
                    msg += fe.OID + ",";

                }
                Marshal.ReleaseComObject(feCusor);
                using (System.IO.FileStream fileStream = new System.IO.FileStream(divFileName, System.IO.FileMode.OpenOrCreate))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fileStream))
                    {
                        sw.WriteLine(msg);
                    }
                }



                var featureWs = wf.OpenFromFile(Application.StartupPath + "\\Temp.gdb", 0) as IFeatureWorkspace;
                for (int i = 1; i <= 9; i++)
                {
                    string name = "SplitLine" + i;
                    if (!(featureWs as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, name))
                    {
                        #region
                        IField oField = new FieldClass();
                        IFields oFields = new FieldsClass();
                        IFieldsEdit oFieldsEdit = null;
                        IFieldEdit oFieldEdit = null;
                        IFeatureClass oFeatureClass = null;

                        try
                        {
                            oFieldsEdit = oFields as IFieldsEdit;
                            oFieldEdit = oField as IFieldEdit;

                            IGeometryDef geometryDef = new GeometryDefClass();
                            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                            geometryDefEdit.AvgNumPoints_2 = 5;
                            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
                            geometryDefEdit.GridCount_2 = 1;
                            geometryDefEdit.HasM_2 = false;
                            geometryDefEdit.HasZ_2 = false;
                            geometryDefEdit.SpatialReference_2 = sfTarget;
                            oFieldEdit.Name_2 = "SHAPE";
                            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                            oFieldEdit.GeometryDef_2 = geometryDef;
                            oFieldEdit.IsNullable_2 = true;
                            oFieldEdit.Required_2 = true;
                            oFieldsEdit.AddField(oField);
                            oFeatureClass = (featureWs as IFeatureWorkspace).CreateFeatureClass(name, oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
                        }
                        catch
                        {
                        }
                        #endregion
                    }
                    IFeatureClass fc = featureWs.OpenFeatureClass(name);
                    (fc as ITable).DeleteSearchedRows(null);
                    IGeoDataset geoDataset = fc as IGeoDataset;
                    if (geoDataset.SpatialReference.Name != sfTarget.Name)
                    {
                        IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = geoDataset as IGeoDatasetSchemaEdit;
                        if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                        {
                            pGeoDatasetSchemaEdit.AlterSpatialReference(sfTarget);
                        }
                    }
                    Marshal.ReleaseComObject(fc);
                }

                Console.WriteLine("预处理完成！....");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        ///step 2
        public void ExcuteCenterLine(int start, int limit, string oids, int poi, string tempGDB)
        {
            IWorkspaceFactory wf = new FileGDBWorkspaceFactoryClass();

            var fstemp = wf.OpenFromFile(tempGDB, 0) as IFeatureWorkspace;
            var fclLine = (fstemp as IFeatureWorkspace).OpenFeatureClass("SplitLine" + poi);
            var fs = wf.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace;
            var fclTB = (fs as IFeatureWorkspace).OpenFeatureClass(mFclName);

            string[] fids = oids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            CenterLineFactory cfFac = new CenterLineFactory();
            foreach (var strID in fids)
            {
                Console.WriteLine("正在处理第【" + start + "-" + (start + limit) + "】,OID:" + strID);
                try
                {
                    int fid = int.Parse(strID);


                    IFeature roadFe = fclTB.GetFeature(fid);
                    IPolygon item = roadFe.Shape as IPolygon;
                    CenterLine centerLine = cfFac.Create2(item);
                    //添加道路面
                    IGeometry geometryLine = centerLine.Line as IGeometry;
                    (geometryLine as ITopologicalOperator).Simplify();

                    try
                    {


                        IFeature tbFeature = fclLine.CreateFeature();
                        tbFeature.Shape = geometryLine;
                        tbFeature.Store();


                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("保存要素错误:" + ex.Message);
                    }
                    finally
                    {

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            //insertCursor.Flush();
            Marshal.ReleaseComObject(fclLine);
            Marshal.ReleaseComObject(fstemp);
            //Marshal.ReleaseComObject(insertCursor);
            //Marshal.ReleaseComObject(fb);
        }
        
        /// <summary>
        /// 预处理  step1
        /// </summary>
        public void TBDividePre(string tempGDBPath, string fcSplit)
        {
            try
            {
                Console.WriteLine("正在预处理....");

                IQueryFilter qf = new QueryFilterClass();
                string msg = string.Empty;
                qf.WhereClause = mSql;
                IWorkspaceFactory wf = new FileGDBWorkspaceFactoryClass();
                var fs = wf.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace;
                var fclTB = (fs as IFeatureWorkspace).OpenFeatureClass(mFclName);
                AddField(fclTB, mRoadField);
                AddField(fclTB, mDisField);
                int count = 1;
                #region 预处理
                Console.WriteLine("正在预处理【初始化融合字段】....");
                int roadIndex = fclTB.FindField(mRoadField);
                int disIndex = fclTB.FindField(mDisField);
                IFeatureCursor feCusor = fclTB.Update(null, false);
                IFeature fe = null;
                while ((fe = feCusor.NextFeature()) != null)
                {
                    count++;
                    fe.set_Value(disIndex, fe.OID);
                    feCusor.UpdateFeature(fe);
                    if (count % 10000 == 0)
                        Console.WriteLine("正在预处理【初始化融合字段】" + count + "....");
                }
                Marshal.ReleaseComObject(feCusor);
                feCusor = fclTB.Update(qf, false);
                Console.WriteLine("正在预处理【刷选狭长图斑】....");
                count = 1;
                while ((fe = feCusor.NextFeature()) != null)
                {
                    count++;
                    if (count % 10000 == 0)
                        Console.WriteLine("正在预处理【刷选狭长图斑】" + count + "....");
                    #region
                    fe.set_Value(roadIndex, fe.OID);
                    feCusor.UpdateFeature(fe);
                    #endregion
                    IPolygon pg = fe.Shape as IPolygon;

                    //计算特征指标
                    IArea pgArea = pg as IArea;
                    double area = pgArea.Area;
                    double pgLenth = (pg as IPolycurve).Length;
                    double flagArea = (pgLenth * pgLenth) / area;
                    //满足狭长指标
                    if (flagArea >= 50 && area > 200)
                    {
                        msg += fe.OID + ",";
                    }


                }
                Marshal.ReleaseComObject(feCusor);
                #endregion
                Console.WriteLine("正在预处理【初始化剖分临时图层】....");
                string divFileName = Application.StartupPath + "//DivItems.txt";
                if (System.IO.File.Exists(divFileName))
                {
                    System.IO.File.Delete(divFileName);
                }

                using (System.IO.FileStream fileStream = new System.IO.FileStream(divFileName, System.IO.FileMode.OpenOrCreate))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fileStream))
                    {
                        sw.WriteLine(msg);
                    }
                }
                #region
                ISpatialReference sfTarget = (fclTB as IGeoDataset).SpatialReference;
                #region
                var featureWs = wf.OpenFromFile(Application.StartupPath + "\\Temp.gdb", 0) as IFeatureWorkspace;
                for (int i = 1; i <= 9; i++)
                {
                    string name = "SplitPoly" + i;
                    IFeatureClass fc = featureWs.OpenFeatureClass(name);
                    (fc as ITable).DeleteSearchedRows(null);
                    IGeoDataset geoDataset = fc as IGeoDataset;
                    if (geoDataset.SpatialReference.Name != sfTarget.Name)
                    {
                        IGeoDatasetSchemaEdit pGeoDatasetSchemaEdit = geoDataset as IGeoDatasetSchemaEdit;
                        if (pGeoDatasetSchemaEdit.CanAlterSpatialReference == true)
                        {
                            pGeoDatasetSchemaEdit.AlterSpatialReference(sfTarget);
                        }
                    }
                    Marshal.ReleaseComObject(fc);
                }
                #endregion
                Console.WriteLine("预处理完成！....");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 分割 2
        /// </summary>
        public void ExcuteSplit(int start, int limit, string oids, int poi, string tempGDB)
        {
            IWorkspaceFactory wf = new FileGDBWorkspaceFactoryClass();

            var fstemp = wf.OpenFromFile(tempGDB, 0) as IFeatureWorkspace;
            var fclPoly = (fstemp as IFeatureWorkspace).OpenFeatureClass("SplitPoly" + poi);


            var fs = wf.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace;
            var fclTB = (fs as IFeatureWorkspace).OpenFeatureClass(mFclName);

            string[] fids = oids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            CenterLineFactory cfFac = new CenterLineFactory();
            foreach (var strID in fids)
            {
                Console.WriteLine("正在处理第【" + start + "-" + (start + limit) + "】,OID:" + strID);
                try
                {
                    int fid = int.Parse(strID);
                    IFeature roadFe = fclTB.GetFeature(fid);
                    IPolygon item = roadFe.Shape as IPolygon;
                    CenterLine centerLine = cfFac.Create2(item);
                    //添加道路面
                    IGeometry geometryLine = centerLine.Line as IGeometry;
                    (geometryLine as ITopologicalOperator).Simplify();
                    IFeatureEdit splitFe = roadFe as IFeatureEdit;
                    //添加分割线,并切割
                    IPolyline topLine = CreateTopologyCutLineByCenterLine(fclTB, geometryLine as IPolyline, item, fid);
                    (topLine as ITopologicalOperator).Simplify();
                    IPolyline spLine = (geometryLine as ITopologicalOperator).Union(topLine) as IPolyline;
                    (spLine as ITopologicalOperator).Simplify();
                    try
                    {


                        IFeature tbFeature = fclPoly.CreateFeature();
                        tbFeature.Shape = item;
                        tbFeature.set_Value(fclPoly.FindField(mRoadField), fid);
                        tbFeature.Store();
                        (tbFeature as IFeatureEdit).Split(spLine);

                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("保存要素错误:" + ex.Message);
                    }
                    finally
                    {

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());                
                }
            }
            //insertCursor.Flush();
            Marshal.ReleaseComObject(fclPoly);
            Marshal.ReleaseComObject(fstemp);
            //Marshal.ReleaseComObject(insertCursor);
            //Marshal.ReleaseComObject(fb);
        }
        #region 融合相关函数
        //id->edge
        private Dictionary<int, AdjEdgeInfo> mAdjEdges = new Dictionary<int, AdjEdgeInfo>();
        //要素边关系
        private Dictionary<int, List<int>> mTBEges = new Dictionary<int, List<int>>();
        //空间关系
        private Dictionary<int, List<int>> mTopDic = new Dictionary<int, List<int>>();
        //是否为道路
        private Dictionary<int, bool> mRoadFlagDic = new Dictionary<int, bool>();
        private double AdjLength(int id, int mergeID, List<int> adjEdges)
        {
            double len = 0;
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
        private int FindLongestToucthFeatureTop(int feid)
        {
            try
            {

                double maxLength = 0;
                int rID = -1;
                List<int> neighbors = mTopDic[feid];
                {
                    foreach (var mergeID in neighbors)
                    {
                        if (mRoadFlagDic.ContainsKey(mergeID))
                        {
                            if (mRoadFlagDic[mergeID])
                                continue;
                        }
                        double length = AdjLength(feid, mergeID, mTBEges[feid]);
                        if (length > maxLength)
                        {
                            maxLength = length;
                            rID = mergeID;
                        }
                    }
                }
                return rID;
            }
            catch
            {
                return -1;
            }

        }
        private void CreateTBTop(IWorkspace mWorkSpace)
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
                    Console.WriteLine("构建图斑邻接关系：" + numK);
                }
                int rid = Convert.ToInt32(feature.get_Value(indexRightID).ToString());
                int lid = Convert.ToInt32(feature.get_Value(indexLeftID).ToString());
                double len = (feature.Shape as IPolyline).Length;
                mAdjEdges.Add(feature.OID, new AdjEdgeInfo { OID = feature.OID, LeftID = lid, RightID = rid, Length = len });


                int LeftOID = lid;
                int RightOID = rid;


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
        #endregion
        public void TBDivideDel(string divFiles)
        {
            Console.WriteLine("正在合并预处理....");

            IQueryFilter qf = new QueryFilterClass();
            string msg = string.Empty;
            qf.WhereClause = mSql;
            IWorkspaceFactory wf = new FileGDBWorkspaceFactoryClass();
            var fs = wf.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace;
            var fclTB = (fs as IFeatureWorkspace).OpenFeatureClass(mFclName);
            string divItems = string.Empty;
            using (System.IO.FileStream fileStream = new System.IO.FileStream(divFiles, System.IO.FileMode.OpenOrCreate))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fileStream))
                {
                    string line;

                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        divItems += line;
                    }
                }
            }
            string[] divFeatures = divItems.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> items = new List<int>();
            IQueryFilter mQuery = new QueryFilterClass();
            foreach (var divItem in divFeatures)
            {

                qf.WhereClause = "ObjectID = " + divItem;
                IFeatureCursor feCusor = fclTB.Update(qf, false);
                IFeature fe = null;
                while ((fe = feCusor.NextFeature()) != null)
                {
                    feCusor.DeleteFeature();
                }
                Marshal.ReleaseComObject(feCusor);

            }

            Console.WriteLine("预处理完成！....");
        }
        /// <summary>
        /// 分割 4
        /// </summary>
        public void ExcuteDivideMerge()
        {
            
            //MessageBox.Show("sdf");
            //return;
            
                IWorkspaceFactory wf = new FileGDBWorkspaceFactoryClass();
                var fs = wf.OpenFromFile(mSourceGDB, 0) as IFeatureWorkspace;
                IQueryFilter qf = new QueryFilterClass();
                IFeatureCursor feCusor = null;
                IFeature fea = null;
                CreateTBTop(fs as IWorkspace);
                qf.WhereClause = mRoadField + " is not null";
                var fclTB = (fs as IFeatureWorkspace).OpenFeatureClass(mFclName);
                int roadIndex = fclTB.FindField(mRoadField);
                int disIndex = fclTB.FindField(mDisField);
                int gbIndex = fclTB.FindField(mGBField);
                // fclTB.GetFeature(11321);
                feCusor = fclTB.Search(qf, false);
                int start = 1;
                Dictionary<int, List<int>> divideFea = new Dictionary<int, List<int>>();
                while ((fea = feCusor.NextFeature()) != null)
                {
                    int roadID = int.Parse(fea.get_Value(roadIndex).ToString());
                    mRoadFlagDic[fea.OID] = true;
                    #region
                    List<int> list = new List<int>();
                    if (divideFea.ContainsKey(roadID))
                    {
                        list = divideFea[roadID];
                    }
                    list.Add(fea.OID);
                    divideFea[roadID] = list;
                    #endregion
                    start++;
                    if (start % 2000 == 0)
                        Console.WriteLine("正在构建拓扑关系：" + start);
                }
                //构建邻接关系
                start = 1;
                //周围都是水面的图斑：最后处理
                Dictionary<int, List<int>> riverRoundPolygons = new Dictionary<int, List<int>>();
                bool isLand = false;
                while (divideFea.Count > 0 && !isLand)
                {
                    isLand = false;
                    riverRoundPolygons.Clear();
                    //1:46秒
                    foreach (var div in divideFea)
                    {
                        // if (div.Value.Count > 1)
                        {
                            #region
                            int msg = start++;
                            int parentID = div.Key;
                            Console.WriteLine("当前正在处理第：" + msg + ",count:" + div.Value.Count);
                            try
                            {
                                List<int> leftPolygons = new List<int>();
                                //记录融合ID
                                Dictionary<int, int> disOIDs = new Dictionary<int, int>();
                                if (divideFea.ContainsKey(parentID))
                                {

                                    foreach (var id in divideFea[parentID])
                                    {
                                        //IFeature fesub = fclTB.GetFeature(id) as IFeature;
                                        leftPolygons.Add(id);
                                        disOIDs[id] = id;
                                    }

                                }
                                int leftNum = 0;
                                do
                                {

                                    //融合关系  merge->subfe
                                    Dictionary<int, int> mergeInfos = new Dictionary<int, int>();
                                    List<int> alonePolygons = new List<int>();
                                    int nn = 0;
                                    //1.查找 
                                    foreach (var feRoad in leftPolygons)
                                    {
                                        nn++;
                                        #region
                                        // Console.WriteLine("当前正在处理第：" + msg + "，剖分融合：" + div);
                                        int longID = FindLongestToucthFeatureTop(feRoad);
                                        if (longID == -1)
                                        {
                                            alonePolygons.Add(feRoad);
                                            continue;
                                        }
                                        else
                                        {
                                            //mergeInfos[feRoad] = fclTB.GetFeature(longID);
                                            mergeInfos[feRoad] = longID;
                                        }
                                        #endregion
                                    }
                                    //2.融合
                                    foreach (var key in mergeInfos.Keys)
                                    {
                                        //设置融合字段
                                        IFeature f = fclTB.GetFeature(mergeInfos[key]);
                                        int disOID = int.Parse(f.get_Value(disIndex).ToString());
                                        IQueryFilter mQuery = new QueryFilterClass();
                                        qf.WhereClause = "ObjectID = " + key;
                                        IFeature feature;
                                        IFeatureCursor fcursor = fclTB.Update(qf, true);
                                        while ((feature = fcursor.NextFeature()) != null)
                                        {
                                            feature.set_Value(gbIndex, f.get_Value(gbIndex));
                                            feature.set_Value(roadIndex, DBNull.Value);
                                            feature.set_Value(disIndex, disOID);
                                            fcursor.UpdateFeature(feature);
                                            mRoadFlagDic[feature.OID] = false;
                                        }
                                        Marshal.ReleaseComObject(f);
                                        Marshal.ReleaseComObject(fcursor);
                                    }
                                    leftNum = leftPolygons.Count;
                                    leftPolygons = new List<int>();
                                    leftPolygons.AddRange(alonePolygons);

                                    if (leftNum == leftPolygons.Count)//处理没有变化的
                                    {
                                        riverRoundPolygons.Add(parentID,alonePolygons);
                                    }

                                }
                                while (leftPolygons.Count > 0 && leftNum > leftPolygons.Count);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("失败：" + ex.Message);
                            }
                            #endregion
                        }
                    }
                    int countUp = divideFea.Count;
                    divideFea.Clear();
                    foreach(var kv in riverRoundPolygons)
                    {
                        divideFea.Add(kv.Key, kv.Value);
                    }
                    if (countUp == divideFea.Count)
                    {
                        isLand = true;
                    }
                }
                foreach (var kv in divideFea)
                {
                    int parentID = kv.Key;
                    foreach (var l in kv.Value)
                    {
                        //设置融合字段
                        
                        int disOID = parentID;
                        IQueryFilter mQuery = new QueryFilterClass();
                        qf.WhereClause = "ObjectID = " + l;
                        IFeature feature;
                        IFeatureCursor fcursor = fclTB.Update(qf, true);
                        while ((feature = fcursor.NextFeature()) != null)
                        {
                            feature.set_Value(gbIndex, mDefaultGB);
                            feature.set_Value(roadIndex, DBNull.Value);
                            feature.set_Value(disIndex, disOID);
                            fcursor.UpdateFeature(feature);
                            mRoadFlagDic[feature.OID] = false;
                        }
                       
                        Marshal.ReleaseComObject(fcursor);
                    }
                }

               
          
        }

        /// <summary>
        /// 查找选中共享边最长的要素：不包括自身(切割的图斑不融于自身)
        /// </summary>
        /// <param name="pg"></param>
        /// <param name="fc"></param>
        /// <returns></returns>
        private IFeature FindLongestToucthFeature(IGeometry pg, IFeatureClass fc, int feid)
        {
            try
            {
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = pg;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                // sf.WhereClause = mRoadField + " <>" + feid + " or " + mRoadField + " is NULL";
                sf.WhereClause = mRoadField + " is NULL";//非道路图斑

                IFeatureCursor fcursor = fc.Search(sf, false);
                IFeature fe = null;
                double maxLength = 0;
                int rID = -1;
                Dictionary<int, IFeature> neighbors = new Dictionary<int, IFeature>();
                while ((fe = fcursor.NextFeature()) != null)
                {
                    neighbors.Add(fe.OID, fe);
                }
                Marshal.ReleaseComObject(fcursor);

                if (neighbors.Count == 1)
                {
                    rID = neighbors.Keys.First();
                }
                else
                {
                    foreach (var kv in neighbors)
                    {
                        double length = LengthOfNeighborPolygon(pg, kv.Value.Shape as IPolygon);
                        if (length > maxLength)
                        {
                            maxLength = length;
                            rID = kv.Key;
                        }
                    }
                }
                Marshal.ReleaseComObject(pg);
                if (rID != -1)
                {
                    return fc.GetFeature(rID);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }
        private double LengthOfNeighborPolygon(IGeometry poly1, IPolygon poly2)
        {
            ITopologicalOperator4 topoOper = poly1 as ITopologicalOperator4;
            IGeometry geo = null;
            try
            {
                geo = topoOper.Intersect(poly2, esriGeometryDimension.esriGeometry1Dimension);
            }
            catch (Exception)
            {
                try
                {
                    topoOper.Simplify();
                    (poly2 as ITopologicalOperator).Simplify();
                    geo = topoOper.Intersect(poly2, esriGeometryDimension.esriGeometry1Dimension);
                }
                catch (Exception)
                {
                    return -2;
                }
            }

            if (geo == null || geo.IsEmpty)
            {
                return -2;
            }
            double len = (geo as IPolyline).Length;
            Marshal.ReleaseComObject(geo);
            return len;

        }

        private IPolyline CreateTopologyCutLineByCenterLine(IFeatureClass fcPolygon, IPolyline centerPolyline, IPolygon polygon, int tbID)
        {

            IPolyline cutLine = new PolylineClass();
            cutLine.SpatialReference = polygon.SpatialReference;

            //找到与多边形相交的所有共享边上的节点（起始节点）
            List<IPoint> toucthPoints = FindToucthFeatureNodes(tbID, polygon, fcPolygon);

            for (int i = 0; i < toucthPoints.Count; i++)
            {
                IPoint point = toucthPoints[i];
                IPoint outPoint = new PointClass();
                double alongDistance = 0;
                double fromDistance = 0;
                bool rightSide = false;

                //获取拓扑节点(point)到中轴线的最近点(outpoint)
                centerPolyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, point, true, outPoint, ref alongDistance, ref fromDistance, ref rightSide);

                PathClass path = new PathClass();
                path.AddPoints(1, ref point);
                path.AddPoints(1, ref outPoint);

                IGeometry geo = path;
                (cutLine as IGeometryCollection).AddGeometries(1, ref geo);

                //处理图边界
                //延长线
                IPoint oPoint = new PointClass();
                oPoint.X = outPoint.X + (outPoint.X - point.X);
                oPoint.Y = outPoint.Y + (outPoint.Y - point.Y);
                IPoint otherPoint = new PointClass();
                polygon.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, oPoint, true, otherPoint, ref alongDistance, ref fromDistance, ref rightSide);

                if (otherPoint.Compare(point) != 0)
                {
                    PathClass otherPath = new PathClass();
                    otherPath.AddPoints(1, ref outPoint);
                    otherPath.AddPoints(1, ref otherPoint);
                    geo = otherPath;
                    (cutLine as IGeometryCollection).AddGeometries(1, ref geo);
                }
            }
            return cutLine;
        }
        private List<IPoint> FindToucthFeatureNodes(int feID, IGeometry polygon, IFeatureClass fc)
        {


            ISpatialFilter qf = new SpatialFilterClass();
            Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
            IFeature fe;
            qf.Geometry = polygon;
            qf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            qf.WhereClause = fc.OIDFieldName + " <> " + feID;
            IFeatureCursor cursor = fc.Search(qf, false);
            List<IPolyline> listEdges = new List<IPolyline>();
            while ((fe = cursor.NextFeature()) != null)
            {
                IGeometry result = (polygon as ITopologicalOperator).Intersect(fe.Shape, esriGeometryDimension.esriGeometry1Dimension);
                if (result != null && !result.IsEmpty)
                {
                    IGeometryCollection gc = result as IGeometryCollection;
                    if (gc.GeometryCount > 1)
                    {
                        for (int i = 0; i < gc.GeometryCount; i++)
                        {
                            PolylineClass polyline = new PolylineClass();
                            polyline.AddGeometry(gc.get_Geometry(i));
                            polyline.Simplify();
                            listEdges.Add(polyline as IPolyline);
                        }
                    }
                    else
                    {
                        listEdges.Add(result as IPolyline);
                    }
                    //listEdges.Add(result as IPolyline);
                }
            }
            Marshal.ReleaseComObject(cursor);
            List<IPoint> nodeList = new List<IPoint>();
            try
            {
                //当前要素的所有边
                foreach (var edge in listEdges)
                {
                    #region
                    IGeometry geo = edge;
                    bool bEqual = false;
                    //from point
                    foreach (IPoint p in nodeList)
                    {
                        IRelationalOperator re = p as IRelationalOperator;
                        if (re.Equals((geo as IPolyline).FromPoint))
                        {
                            bEqual = true;
                            break;
                        }
                    }
                    if (!bEqual)
                        nodeList.Add((geo as IPolyline).FromPoint);
                    //to point
                    bEqual = false;
                    foreach (IPoint p in nodeList)
                    {
                        IRelationalOperator re = p as IRelationalOperator;

                        if (re.Equals((geo as IPolyline).ToPoint))
                        {
                            bEqual = true;
                            break;
                        }
                    }
                    if (!bEqual)
                        nodeList.Add((geo as IPolyline).ToPoint);

                    #endregion
                }

            }
            catch (Exception ex)
            {
                return nodeList;
            }
            return nodeList;
        }
            #endregion
    }
}

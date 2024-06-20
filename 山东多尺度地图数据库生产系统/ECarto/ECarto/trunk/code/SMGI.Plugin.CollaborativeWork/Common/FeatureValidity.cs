using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using System.Diagnostics;

namespace SMGI.Plugin.CollaborativeWork
{
    /// <summary>
    /// 判断图层中要素是否有效
    /// </summary>
    public class FeatureValidity
    {
        /// <summary>
        /// 判断数据库中的的本地被编辑要素是否合法
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="localBaseVersion"></param>
        /// <param name="checkedFCName"></param>
        /// <returns></returns>
        public static bool EditFeatureIsValid(IWorkspace ws, int localBaseVersion, List<string> checkedFCName)
        {
            if (null == ws)
                return false;

            IEnumDataset pEnumDataset = ws.get_Datasets(esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            while (pDataset != null)
            {
                if (pDataset is IFeatureDataset)//要素数据集
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)ws;
                    IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                    IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF = pEnumDatasetF.Next();
                    while (pDatasetF != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = pDatasetF as IFeatureClass;
                            if (fc != null && checkedFCName.Contains(fc.AliasName))
                            {
                                if (!EditFeatureIsValid(fc, localBaseVersion))
                                    return false;
                            }
                        }

                        pDatasetF = pEnumDatasetF.Next();
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pDataset is IFeatureClass)//要素类
                {
                    IFeatureClass fc = pDataset as IFeatureClass;
                    if (fc != null && checkedFCName.Contains(fc.AliasName))
                    {
                        if (!EditFeatureIsValid(fc, localBaseVersion))
                            return false;
                    }
                }
                else
                {

                }

                pDataset = pEnumDataset.Next();

            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);

            return true;
        }

        /// <summary>
        /// 判断要素类中的的本地被编辑要素是否合法
        /// 1.空几何、长度或面积为0的几何
        /// 2.GUID未赋值
        /// 3.GUID不唯一
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="localBaseVersion"></param>
        /// <returns></returns>
        public static bool EditFeatureIsValid(IFeatureClass fc, int localBaseVersion)
        {
            int verIndex = fc.FindField(ServerDataInitializeCommand.CollabVERSION);
            if (verIndex == -1)
                return true;//不判断

            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = string.Format("{0} < 0", ServerDataInitializeCommand.CollabVERSION);
            if (fc.FeatureCount(qf) == 0)
                return true;//不存在编辑要素，不判断

            int guidIndex = fc.FindField(ServerDataInitializeCommand.CollabGUID);
            if (guidIndex == -1)
            {
                MessageBox.Show(string.Format("图层【{0}】没有找到字段【{1}】！'", fc.AliasName, ServerDataInitializeCommand.CollabGUID));
                return false;
            }

            bool res = true;
            try
            {

                #region 几何有效性判断
                if (fc.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)//编辑要素中的非删除要素存在长度为0
                {
                    qf.WhereClause = string.Format("{0} < 0 and ({1} is null or {1} <> '{2}') and shape_Length = 0", ServerDataInitializeCommand.CollabVERSION, ServerDataInitializeCommand.CollabDELSTATE, ServerDataInitializeCommand.DelStateText);
                    if (fc.FeatureCount(qf) > 0)
                    {
                        MessageBox.Show(string.Format("图层【{0}】存在长度为0的非删除状态的编辑要素,不允许这类要素存在，请先处理！'", fc.AliasName));
                        return false;
                    }
                }
                else if (fc.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)//编辑要素中的非删除要素存在面积为0
                {
                    qf.WhereClause = string.Format("{0} < 0 and ({1} is null or {1} <> '{2}') and shape_Area = 0", ServerDataInitializeCommand.CollabVERSION, ServerDataInitializeCommand.CollabDELSTATE, ServerDataInitializeCommand.DelStateText);
                    if (fc.FeatureCount(qf) > 0)
                    {
                        MessageBox.Show(string.Format("图层【{0}】存在面积为0的非删除状态的编辑要素,不允许这类要素存在，请先处理！'", fc.AliasName));
                        return false;
                    }
                }
                #endregion

                #region guid是否初始化判断
                qf.WhereClause = string.Format("{0} < 0 and ({1} is NULL or {2} = '' or {3} = 'NULL')",
                    ServerDataInitializeCommand.CollabVERSION, ServerDataInitializeCommand.CollabGUID,
                    ServerDataInitializeCommand.CollabGUID, ServerDataInitializeCommand.CollabGUID);//被编辑要素中guid未赋值
                if (fc.FeatureCount(qf) > 0)
                {
                    MessageBox.Show(string.Format("图层【{0}】被编辑过的要素中存在【{1}】未赋值的要素！", fc.AliasName, ServerDataInitializeCommand.CollabGUID));
                    return false;
                }
                #endregion

                #region guid唯一性(不考虑未赋值的情况)
                Dictionary<string, int> editGUIDList = new Dictionary<string, int>();

                IFeatureCursor fCursor = null;
                qf.WhereClause = string.Format("{0} < 0", ServerDataInitializeCommand.CollabVERSION);
                fCursor = fc.Search(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    string guid = f.get_Value(guidIndex).ToString();

                    if (editGUIDList.Keys.Contains(guid))//编辑要素中存在相同的guid
                    {
                        MessageBox.Show(string.Format("图层【{0}】的编辑要素【{1}】的【{2}】值不唯一，存在另一本地要素与其拥有相同值！",
                            fc.AliasName, f.OID, ServerDataInitializeCommand.CollabGUID));
                        res = false;

                        break;
                    }

                    editGUIDList.Add(guid, f.OID);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);

                if (res && editGUIDList.Count > 0)
                {
                    qf.WhereClause = string.Format("{0} >= 0 and {0} <= {1}",
                        ServerDataInitializeCommand.CollabVERSION, localBaseVersion);//本地要素中非编辑要素中查找是否存在于编辑要素中相同的guid
                    fCursor = fc.Search(qf, true);
                    while ((f = fCursor.NextFeature()) != null)
                    {
                        string guid = f.get_Value(guidIndex).ToString();

                        if (editGUIDList.Keys.Contains(guid))//存在与编辑要素相同的guid
                        {
                            MessageBox.Show(string.Format("图层【{0}】的编辑要素【{1}】的【{2}】值不唯一，存在另一本地要素与其拥有相同值！",
                            fc.AliasName, editGUIDList[guid], ServerDataInitializeCommand.CollabGUID));
                            res = false;

                            break;
                        }
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                }
                #endregion

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                return false;
            }

            return res;
        }
    }
}

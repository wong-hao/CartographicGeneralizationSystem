using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.CollaborativeWork
{
    public static class WorkspaceExtensions
    {
        /// <summary>
        /// 判断工作空间是否被锁住
        /// </summary>
        /// <returns></returns>
        public static bool IsLock(this IWorkspace workspace)
        {
            IEnumDataset pEnumDataset = workspace.get_Datasets(esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            while (pDataset != null)
            {
                if (pDataset is IFeatureDataset)//要素数据集
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)workspace;
                    IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                    IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                    pEnumDatasetF.Reset();
                    IDataset pDatasetF = pEnumDatasetF.Next();
                    while (pDatasetF != null)
                    {
                        if (pDatasetF is IFeatureClass)//要素类
                        {
                            IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);
                            if (fc != null)
                            {
                                Dictionary<string, esriSchemaLock> locks = GApplication.Application.GetSchemaLocksForObjectClass(fc as ISchemaLock);
                                foreach (KeyValuePair<string, esriSchemaLock> kv in locks)
                                {
                                    if (esriSchemaLock.esriExclusiveSchemaLock == kv.Value)
                                    {
                                        MessageBox.Show(string.Format("要素类'{0}'被用户'{1}'锁定，锁定数据库失败！", fc.AliasName, kv.Key));

                                        return true;
                                    }
                                }
                            }
                        }

                        pDatasetF = pEnumDatasetF.Next();
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                }
                else if (pDataset is IFeatureClass)//要素类
                {
                    IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)workspace;

                    IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDataset.Name);
                    if (fc != null)
                    {
                        Dictionary<string, esriSchemaLock> locks = GApplication.Application.GetSchemaLocksForObjectClass(fc as ISchemaLock);
                        foreach (KeyValuePair<string, esriSchemaLock> kv in locks)
                        {
                            if (esriSchemaLock.esriExclusiveSchemaLock == kv.Value)
                            {
                                MessageBox.Show(string.Format("要素类'{0}'被用户'{1}'锁定，锁定数据库失败！", fc.AliasName, kv.Key));

                                return true;
                            }
                        }
                    }
                }
                else
                {

                }

                pDataset = pEnumDataset.Next();
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);

            return false;
        }

        /// <summary>
        /// 锁定服务器端工作空间(所有要素类)
        /// </summary>
        /// <returns></returns>
        public static bool Lock(this IWorkspace workspace)
        {
            if (null == workspace)
                return false;

            List<ISchemaLock> lockObjects = new List<ISchemaLock>();

            try
            {
                IEnumDataset pEnumDataset = workspace.get_Datasets(esriDatasetType.esriDTAny);
                pEnumDataset.Reset();
                IDataset pDataset = pEnumDataset.Next();
                while (pDataset != null)
                {
                    if (pDataset is IFeatureDataset)//要素数据集
                    {
                        IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)workspace;
                        IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                        IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                        pEnumDatasetF.Reset();
                        IDataset pDatasetF = pEnumDatasetF.Next();
                        while (pDatasetF != null)
                        {
                            if (pDatasetF is IFeatureClass)//要素类
                            {
                                IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);
                                if (fc != null)
                                {
                                    Dictionary<string, esriSchemaLock> locks = GApplication.Application.GetSchemaLocksForObjectClass(fc as ISchemaLock);
                                    foreach (KeyValuePair<string, esriSchemaLock> kv in locks)
                                    {
                                        if (esriSchemaLock.esriExclusiveSchemaLock == kv.Value)
                                        {
                                            throw new Exception(string.Format("要素类'{0}'被用户'{1}'锁定，锁定数据库失败！", fc.AliasName, kv.Key));
                                        }
                                    }

                                    GApplication.Application.LockObject(fc as ISchemaLock);
                                    lockObjects.Add(fc as ISchemaLock);
                                }
                            }

                            pDatasetF = pEnumDatasetF.Next();
                        }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                    }
                    else if (pDataset is IFeatureClass)//要素类
                    {
                        IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)workspace;

                        IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDataset.Name);
                        if (fc != null)
                        {
                            Dictionary<string, esriSchemaLock> locks = GApplication.Application.GetSchemaLocksForObjectClass(fc as ISchemaLock);
                            foreach (KeyValuePair<string, esriSchemaLock> kv in locks)
                            {
                                if (esriSchemaLock.esriExclusiveSchemaLock == kv.Value)
                                {
                                    throw new Exception(string.Format("要素类'{0}'被用户'{1}'锁定，锁定数据库失败！", fc.AliasName, kv.Key));
                                }
                            }

                            GApplication.Application.LockObject(fc as ISchemaLock);
                            lockObjects.Add(fc as ISchemaLock);
                        }
                    }
                    else
                    {

                    }

                    pDataset = pEnumDataset.Next();
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
            }
            catch (Exception ex)
            {
                foreach (ISchemaLock obj in lockObjects)
                {
                    GApplication.Application.UnLockObject(obj);
                }

                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 解锁服务器端工作空间
        /// </summary>
        /// <returns></returns>
        public static bool UnLock(this IWorkspace workspace)
        {
            if (null == workspace)
                return false;

            try
            {
                IEnumDataset pEnumDataset = workspace.get_Datasets(esriDatasetType.esriDTAny);
                pEnumDataset.Reset();
                IDataset pDataset = pEnumDataset.Next();
                while (pDataset != null)
                {
                    if (pDataset is IFeatureDataset)//要素数据集
                    {
                        IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)workspace;
                        IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(pDataset.Name);
                        IEnumDataset pEnumDatasetF = pFeatureDataset.Subsets;
                        pEnumDatasetF.Reset();
                        IDataset pDatasetF = pEnumDatasetF.Next();
                        while (pDatasetF != null)
                        {
                            if (pDatasetF is IFeatureClass)//要素类
                            {
                                IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDatasetF.Name);
                                if (fc != null)
                                    GApplication.Application.UnLockObject(fc as ISchemaLock);
                            }

                            pDatasetF = pEnumDatasetF.Next();
                        }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDatasetF);
                    }
                    else if (pDataset is IFeatureClass)//要素类
                    {
                        IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)workspace;

                        IFeatureClass fc = pFeatureWorkspace.OpenFeatureClass(pDataset.Name);
                        if (fc != null)
                            GApplication.Application.UnLockObject(fc as ISchemaLock);
                    }
                    else
                    {

                    }

                    pDataset = pEnumDataset.Next();
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                return false;
            }

            return true;
        }
    }
}

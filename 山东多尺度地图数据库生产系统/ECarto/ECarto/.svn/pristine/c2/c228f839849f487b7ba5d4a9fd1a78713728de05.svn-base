using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.CollaborativeWork
{
    class RegistryHelper
    {
        /// <summary>
        /// 读取指定名称的注册表的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetRegistryData(RegistryKey root, string subkey, string name)
        {
            string registData = "";
            RegistryKey myKey = root.OpenSubKey(subkey, true);
            if (myKey != null)
            {
                var val = myKey.GetValue(name);
                if (val != null && !Convert.IsDBNull(val))
                    registData = val.ToString();
            }

            return registData;
        }

        /// <summary>
        /// 向注册表中写数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tovalue"></param> 
        public static void SetRegistryData(RegistryKey root, string subkey, string name, string value)
        {
            RegistryKey aimdir = root.CreateSubKey(subkey);
            if (aimdir != null)
                aimdir.SetValue(name, value);
        }

        /// <summary>
        /// 删除注册表中指定的注册表项
        /// </summary>
        /// <param name="name"></param>
        public static void DeleteRegistData(RegistryKey root, string subkey, string name)
        {
            RegistryKey myKey = root.OpenSubKey(subkey, true);
            if (myKey != null && myKey.GetValue(name) != null)
            {
                myKey.DeleteValue(name);
            }
        }

        /// <summary>
        /// 判断指定注册表项是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsRegistryExist(RegistryKey root, string subkey, string name)
        {
            bool bExit = false;

            RegistryKey myKey = root.OpenSubKey(subkey, true);
            if (myKey == null)
            {
                return bExit;
            }

            string[] subkeyNames = myKey.GetSubKeyNames();
            foreach (string keyName in subkeyNames)
            {
                if (keyName == name)
                {
                    bExit = true;
                    break;
                }
            }

            return bExit;
        }


        /// <summary>
        /// 注册版本
        /// </summary>
        /// <param name="ws"></param>
        public static void RegisterVersion(IWorkspace ws)
        {
            IEnumDataset enumDataset = ws.get_Datasets(esriDatasetType.esriDTAny);
            enumDataset.Reset();
            IDataset dataset = enumDataset.Next();
            while (dataset != null)
            {
                if (dataset is IFeatureDataset)
                {
                    if (!(dataset as IVersionedObject3).IsRegisteredAsVersioned)
                    {
                        try
                        {
                            (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                        }
                        catch (Exception ex)
                        {
                            if ((ex is COMException) && (ex as COMException).ErrorCode == -2147220989)
                            {
                                //针对该种情况进行特殊处理：原要素集在注册版本时没有选择moveEditsToBase项，
                                //而后在编辑过程中新增或替换了数据集中某一要素类，这样将会导致整个数据集未完全注册,
                                //若此时注册版本，并选择moveEditsToBase将会导致com组件错误【该实现不支持此操作。 [此对象的状态或所包含的对象不允许将编辑内容移动到基表中。]】
                                //这里尝试采取的策略是：先注册版本（不选择moveEditsToBase），然后再取消注册（选择moveEditsToBase，以保存先前的编辑成果到基表），最后再注册版本（选择moveEditsToBase）

                                (dataset as IVersionedObject).RegisterAsVersioned(true);//注册版本

                                (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);//反注册版本

                                (dataset as IVersionedObject3).RegisterAsVersioned3(true);//注册版本

                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);

                        (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                    }
                }

                if (dataset is IFeatureClass)
                {
                    if (!(dataset as IVersionedObject3).IsRegisteredAsVersioned)
                    {
                        try
                        {
                            (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                        }
                        catch (Exception ex)
                        {
                            if ((ex is COMException) && (ex as COMException).ErrorCode == -2147220989)
                            {
                                (dataset as IVersionedObject).RegisterAsVersioned(true);//注册版本

                                (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);//反注册版本

                                (dataset as IVersionedObject3).RegisterAsVersioned3(true);//注册版本
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);

                        (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                    }
                }

                if (dataset is ITable)
                {
                    if (!(dataset as IVersionedObject3).IsRegisteredAsVersioned)
                    {
                        try
                        {
                            (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                        }
                        catch (Exception ex)
                        {
                            if ((ex is COMException) && (ex as COMException).ErrorCode == -2147220989)
                            {
                                (dataset as IVersionedObject).RegisterAsVersioned(true);//注册版本

                                (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);//反注册版本

                                (dataset as IVersionedObject3).RegisterAsVersioned3(true);//注册版本
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            (dataset as IVersionedObject3).UnRegisterAsVersioned3(true);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("Table is not multiversion, but must be for this operation"))//表未进行多版本化，但此操作要求必须采用多版本
                            {
                                (dataset as IVersionedObject).RegisterAsVersioned(false);
                            }
                            else
                            {
                                throw ex;
                            }
                            
                        }

                        (dataset as IVersionedObject3).RegisterAsVersioned3(true);
                    }
                }
                dataset = enumDataset.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(enumDataset);
        }

    }
}

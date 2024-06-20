using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataManagementTools;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.CollaborativeWork
{
    /// <summary>
    /// 下载协同数据
    /// </summary>
    public class DownLoadDataCmd : SMGICommand
    {
        public DownLoadDataCmd()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null;
            }
        }


       
        public override void OnClick()
        {
            DownLoadDataForm frm = new DownLoadDataForm(m_Application);
            frm.StartPosition = FormStartPosition.CenterParent;
            if (DialogResult.OK == frm.ShowDialog())
            {
                bool res = false;
                using (var wo = m_Application.SetBusy())
                {
                    SDEDataServer ds = new SDEDataServer(m_Application, frm.IPAdress, frm.UserName, frm.Password, frm.DataBase);

                    //下载数据（矢量）
                    res = ds.DownLoadCollaborativeData(frm.OutputGDB, frm.RangeGeometry, frm.FieldNameUpper, frm.FeatureClassNameList, -1, wo);//现势数据

                    if (res)
                    {
                        //复制本地模板的表格
                        IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
                        IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(frm.OutputGDB, 0);
                        res = commonMethod.CopyLocalTemplateCollaTable(pWorkspace);

                        //更新输出数据库本地状态表
                        int serverVersion = ds.getServerMaxVersion();
                        if (serverVersion == -1)
                        {
                            MessageBox.Show("更新输出数据库本地状态表失败！");
                            return;

                        }
                        string ipAddress = frm.IPAdress;
                        string userName = frm.UserName;
                        string passWord = frm.Password;
                        string databaseName = frm.DataBase;
                        string extentName = frm.RangeFileName;
                        bool RembemberPassword = frm.RemberPassword;
                        commonMethod.UpdateLocalStateTable(pWorkspace, serverVersion, ipAddress, userName, passWord, databaseName,
                            extentName, frm.RangeGeometry);

                        //写注册表，存储下载数据的相关参数信息
                        if (!RegistryHelper.IsRegistryExist(Registry.LocalMachine, "SOFTWARE", "SMGI"))
                        {
                            //创建
                            Registry.LocalMachine.CreateSubKey("SOFTWARE\\SMGI");
                        }
                        string download = string.Format("{0},{1},{2},{3},{4},{5}", ipAddress, databaseName, userName, extentName, passWord, RembemberPassword.ToString()); ;
                        RegistryHelper.SetRegistryData(Registry.LocalMachine, "SOFTWARE\\SMGI", "DownLoad", download);

                    }

                }

                if (res)
                {
                    commonMethod.OpenGDBFile(m_Application, frm.OutputGDB);
                }
            }
        }

    }
}

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
    /// 删除服务器数据库中某一特定版本号的所有要素
    /// 注意：需要管理员权限
    /// </summary>
    public class ServerDBDeleteOneVersionCmd : SMGICommand
    {
        public ServerDBDeleteOneVersionCmd()
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
            ServerDBRestoreForm frm = new ServerDBRestoreForm();
            frm.Text = "服务器数据库版本删除";
            frm.StartPosition = FormStartPosition.CenterParent;
            if (DialogResult.OK != frm.ShowDialog())
                return;

            bool res = true;
            int delCount = 0;
            using (var wo = m_Application.SetBusy())
            {
                SDEDataServer ds = new SDEDataServer(m_Application, frm.IPAdress, frm.UserName, frm.Password, frm.DataBase);

                //删除服务器要素
                IQueryFilter fcQF = new QueryFilterClass() { WhereClause = string.Format("{0} = {1}", ServerDataInitializeCommand.CollabVERSION, frm.DBVersion) };
                IQueryFilter tableQF = new QueryFilterClass() { WhereClause = string.Format("versid = {0}", frm.DBVersion) };
                delCount = ds.DeleteFeatureInDatabase(fcQF, tableQF, System.Environment.MachineName, string.Format("数据库版本删除:【删除版本号为{0}的要素】", frm.DBVersion));
                if (delCount < 0)
                {
                    res = false;
                }
            }

            if (res)
            {
                //写注册表，存储下载数据的相关参数信息
                if (!RegistryHelper.IsRegistryExist(Registry.LocalMachine, "SOFTWARE", "SMGI"))
                {
                    //创建
                    Registry.LocalMachine.CreateSubKey("SOFTWARE\\SMGI");
                }
                string download = string.Format("{0},{1},{2},{3},{4},{5}", frm.IPAdress, frm.DataBase, frm.UserName, "", frm.Password, frm.RemberPassword.ToString()); ;
                RegistryHelper.SetRegistryData(Registry.LocalMachine, "SOFTWARE\\SMGI", "DownLoad", download);

                MessageBox.Show(string.Format("操作完成,本次操作共删除要素【{0}】个。", delCount));
            }
        }

    }
}

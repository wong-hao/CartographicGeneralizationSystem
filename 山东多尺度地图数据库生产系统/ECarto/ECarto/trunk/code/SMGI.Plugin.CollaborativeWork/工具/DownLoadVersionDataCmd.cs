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
    /// 下载指定版本的协同数据(不保留协同状态表)
    /// </summary>
    public class DownLoadVersionDataCmd : SMGICommand
    {
        public DownLoadVersionDataCmd()
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
            DownLoadVersionDataForm frm = new DownLoadVersionDataForm();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (DialogResult.OK != frm.ShowDialog())
                return;

            bool res = false;
            using (var wo = m_Application.SetBusy())
            {

                SDEDataServer ds = new SDEDataServer(m_Application, frm.IPAdress, frm.UserName, frm.Password, frm.DataBase);

                //下载数据（矢量）
                res = ds.DownLoadCollaborativeData(frm.OutputGDB, frm.RangeGeometry, frm.FieldNameUpper, null, frm.DBVersion, wo);//版本数据
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

                MessageBox.Show("数据下载完成！");

            }
        }

    }
}

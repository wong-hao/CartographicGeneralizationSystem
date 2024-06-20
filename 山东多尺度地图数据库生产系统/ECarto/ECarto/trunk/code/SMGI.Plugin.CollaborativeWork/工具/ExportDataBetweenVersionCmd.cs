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
    /// 从服务数据库中导出指定版本间的增量数据(不保留协同状态表)
    /// </summary>
    public class ExportDataBetweenVersionCmd : SMGICommand
    {
        public ExportDataBetweenVersionCmd()
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
            ExportDataBetweenVersionForm frm = new ExportDataBetweenVersionForm();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (DialogResult.OK != frm.ShowDialog())
                return;

            bool res = false;
            using (var wo = m_Application.SetBusy())
            {

                SDEDataServer ds = new SDEDataServer(m_Application, frm.IPAdress, frm.UserName, frm.Password, frm.DataBase);

                //导出版本间增量数据
                res = ds.ExportDataBetweenVersion(frm.OutputGDB, frm.RangeGeometry, frm.FieldNameUpper, frm.StartDBVersion, frm.EndDBVersion, wo);
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

                MessageBox.Show("增量数据导出完成！");

            }
        }

    }
}

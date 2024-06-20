using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using System.Data;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Plugin.CollaborativeWork
{
    public class DataDetectionCommand : SMGICommand
    {
        FrmDataDetection _dataDetectionFrm = null;

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.ActiveView.FocusMap.SelectionCount > 0 &&
                    m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateEditing;
            }
        }

        public override void OnClick()
        {
            int localBaseVersion = 0;
            string serverIPAddress, sdeUsername, sdePassword, sdeDataBaseName;
            using (var wo = GApplication.Application.SetBusy())
            {
                wo.SetText("正在进行初始化工作...");
                DataTable localDT = commonMethod.ReadToDataTable(m_Application.Workspace.EsriWorkspace, "SMGILocalState");
                if (null == localDT || 0 == localDT.Rows.Count)
                {
                    MessageBox.Show("本地数据库状态表为空,无法协调数据！");
                    return ;
                }

                try
                {
                    localBaseVersion = Convert.ToInt32(localDT.AsEnumerable().Select(t => t.Field<string>("BASEVERSION")).FirstOrDefault());//本地数据库的基版本
                }
                catch
                {
                    localBaseVersion = 0;
                }
                serverIPAddress = localDT.AsEnumerable().Select(t => t.Field<string>("IPADDRESS")).FirstOrDefault();
                sdeUsername = localDT.AsEnumerable().Select(t => t.Field<string>("USERNAME")).FirstOrDefault();
                sdePassword = localDT.AsEnumerable().Select(t => t.Field<string>("PASSWORD")).FirstOrDefault();
                sdeDataBaseName = localDT.AsEnumerable().Select(t => t.Field<string>("DATABASE")).FirstOrDefault();

                if (_dataDetectionFrm != null && !_dataDetectionFrm.IsDisposed)
                {
                    _dataDetectionFrm.Close();
                }

                wo.SetText("正在检测...");
                _dataDetectionFrm = new FrmDataDetection(m_Application, serverIPAddress, sdeUsername, sdePassword, sdeDataBaseName, localBaseVersion);
                _dataDetectionFrm.StartPosition = FormStartPosition.CenterScreen;
                _dataDetectionFrm.Show();

            }
        }


        
    }
}

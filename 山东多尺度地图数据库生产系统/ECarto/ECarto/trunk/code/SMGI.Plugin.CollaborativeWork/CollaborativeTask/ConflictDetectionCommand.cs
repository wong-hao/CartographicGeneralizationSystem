using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.CollaborativeWork
{
    public class ConflictDetectionCommand : SMGICommand
    {
        public ConflictDetectionCommand()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null &&
                    m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            using (var wo = GApplication.Application.SetBusy())
            {
                wo.SetText("正在进行初始化工作...");

                if (CollaborativeTask.DetectionState.DETECTING == CollaborativeTask.Instance.DetectState)
                {
                    MessageBox.Show("冲突检测已经开始，请稍等！");

                    return;
                }

                if (CollaborativeTask.DetectionState.DETECTED == CollaborativeTask.Instance.DetectState)
                {
                    MessageBox.Show("请先处理完上一次的协调任务！");

                    return;
                }

                //准备协调
                if (!CollaborativeTask.Instance.prepareCollaborate(m_Application))
                {
                    return;
                }

                wo.SetText("正在锁定数据库...");
                #region 通过咨询锁锁定sde数据库
                SDEDataServer ds = new SDEDataServer(m_Application, CollaborativeTask.Instance.ServerIPAddress, CollaborativeTask.Instance.SDEUsername, CollaborativeTask.Instance.SDEPassword, CollaborativeTask.Instance.SDEDataBaseName);
                if (!ds.AdvisoryLock())
                {
                    MessageBox.Show("数据库已被锁住,请等待解锁!");
                    return;
                }
                #endregion

                //开始协调
                string err = CollaborativeTask.Instance.startCollaborate(wo);
                if (err != "")
                {
                    MessageBox.Show(err);
                    return;
                }

                m_Application.Workspace.IsCollaborativing = true;

            }

            //显示冲突结果表
            ConflictResultTable.Instance.upateTable();
            ConflictResultTable.Instance.show();

            MessageBox.Show("冲突检测完成");
        }

       

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using System.Data;
using System.Runtime.InteropServices;
using System.IO;


namespace SMGI.Plugin.CollaborativeWork
{
    /// <summary>
    /// 提交本地编辑内容，并删除被提交内容对象的所有历史版本（湖南定制化要求）
    /// </summary>
    public class SubmitAndDelHistoryVersionCommand : SMGICommand
    {
        public SubmitAndDelHistoryVersionCommand()
        {
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && CollaborativeTask.Instance.IsCompleted() &&
                    m_Application.EngineEditor.EditState == ESRI.ArcGIS.Controls.esriEngineEditState.esriEngineStateNotEditing;
            }
        }


        public override void OnClick()
        {
            //本地数据库中被编辑的要素集合
            Dictionary<string, Dictionary<string, FeatureInfo>> editedFeatures = new Dictionary<string, Dictionary<string, FeatureInfo>>();
            using (var wo = GApplication.Application.SetBusy())
            {
                wo.SetText("正在获取本地数据库的更新要素...");
                editedFeatures = CollaborativeTask.Instance.getEditedFeatureList();
            }

            FrmSubmit submitDlg = new FrmSubmit(editedFeatures);
            if (DialogResult.OK == submitDlg.ShowDialog())
            {
                System.Diagnostics.Trace.WriteLine(string.Format("/******开始提交数据库【{0}】******/", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")));
                System.Diagnostics.Trace.WriteLine(string.Format("本地数据库路径：【{0}】", CollaborativeTask.Instance.LocalWorkspace.PathName));
                System.Diagnostics.Trace.WriteLine(string.Format("服务器信息：IP【{0}】、数据库名【{1}】", CollaborativeTask.Instance.ServerIPAddress, CollaborativeTask.Instance.SDEDataBaseName));
                System.Diagnostics.Trace.WriteLine(string.Format("提交选项信息：备注【{0}】；备份数据库选项：【{1}】", submitDlg.SubmitDesc, submitDlg.NeedBackUpDB.ToString()));

                using (var wo = GApplication.Application.SetBusy())
                {
                    DateTime start = DateTime.Now;
                    TimeSpan timeSpan;

                    if (submitDlg.NeedBackUpDB)
                    {
                        wo.SetText("正在备份本地数据库...");
                        #region 备份提交前的数据库
                        string backUpFilePath = System.IO.Path.GetDirectoryName(m_Application.Workspace.EsriWorkspace.PathName) + "\\BackUp";
                        if (!Directory.Exists(backUpFilePath))
                            Directory.CreateDirectory(backUpFilePath);
                        DirectoryInfo di = new DirectoryInfo(backUpFilePath);
                        if ((di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)//隐藏文件夹
                        {
                            File.SetAttributes(backUpFilePath, di.Attributes | FileAttributes.Hidden & ~FileAttributes.ReadOnly);
                        }
                        string backUpFileName = System.IO.Path.GetFileNameWithoutExtension(m_Application.Workspace.EsriWorkspace.PathName) + "_backup_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                        string backUpGDBPath = backUpFilePath + "\\" + backUpFileName + ".gdb";
                        IWorkspace buWS = DataBaseCommonMethod.CopyWorkSpace(CollaborativeTask.Instance.LocalWorkspace, backUpGDBPath);
                        Marshal.ReleaseComObject(buWS);
                        #endregion

                        timeSpan = DateTime.Now - start;
                        System.Diagnostics.Trace.WriteLine(string.Format("备份本地数据库用时：{0}（秒）", timeSpan.TotalMinutes));
                        start = DateTime.Now;
                    }

                    wo.SetText("正在整理本地数据库的更新要素...");
                    //本地记录表
                    DataTable localDT = commonMethod.ReadToDataTable(CollaborativeTask.Instance.LocalWorkspace, "RecordTable");
    
                    Dictionary<string, List<int>> fcName2editedFeatures = new Dictionary<string, List<int>>();//本地被编辑过的要素集合
                    foreach (var kv in editedFeatures)
                    {
                        if (!submitDlg.NeedSubmitFC.Contains(kv.Key))
                            continue;//没有勾选

                        List<int> editedFeatureOIDList = new List<int>();
                        foreach (var item in kv.Value)
                        {
                            editedFeatureOIDList.Add(item.Value.oid);
                        }

                        fcName2editedFeatures.Add(kv.Key, editedFeatureOIDList);
                    }

                    timeSpan = DateTime.Now - start;
                    System.Diagnostics.Trace.WriteLine(string.Format("整理本地编辑要素用时：{0}（秒）", timeSpan.TotalMinutes));
                    start = DateTime.Now;

                    //更新服务器
                    wo.SetText("正在更新服务器数据库...");
                    SDEDataServer ds = new SDEDataServer(m_Application, CollaborativeTask.Instance.ServerIPAddress, CollaborativeTask.Instance.SDEUsername, CollaborativeTask.Instance.SDEPassword, CollaborativeTask.Instance.SDEDataBaseName);
                    int serverLatestVersion = ds.UpdateDatabaseAndDelHistoryVersion(fcName2editedFeatures, localDT, submitDlg.SubmitDesc, System.Environment.MachineName, wo);
                    if (serverLatestVersion < 0)
                    {
                        MessageBox.Show("更新服务器数据库失败！");

                        return;
                    }
                    //解锁服务器数据库
                    ds.AdvisoryUnlock();

                    timeSpan = DateTime.Now - start;
                    System.Diagnostics.Trace.WriteLine(string.Format("更新服务器数据库用时：{0}（秒）", timeSpan.TotalMinutes));
                    start = DateTime.Now;

                    //更新本地数据库
                    wo.SetText("正在更新本地数据库...");
                    List<string> serverConflictFeatures = new List<string>();
                    foreach(var item in CollaborativeTask.Instance.ServerConflictFeatures)
                    {
                        serverConflictFeatures.AddRange(item.Value.Keys);
                    }
                    List<string> localConflictFeatures = new List<string>();
                    localConflictFeatures.AddRange(CollaborativeTask.Instance.LocalConflictFeaturesState.Keys);

                    LocalDataBase db = new LocalDataBase(m_Application, CollaborativeTask.Instance.LocalWorkspace);
                    if (!db.updateDataBase(serverLatestVersion, CollaborativeTask.Instance.LocalBaseVersion, serverConflictFeatures, localConflictFeatures))
                    {
                        System.Diagnostics.Trace.WriteLine("更新本地数据库时出现异常！");
                    }
                    else
                    {
                        timeSpan = DateTime.Now - start;
                        System.Diagnostics.Trace.WriteLine(string.Format("更新本地数据库用时：{0}（秒）", timeSpan.TotalMinutes));
                        start = DateTime.Now;
                    }

                    CollaborativeTask.Instance.completeCollaborate();

                    m_Application.Workspace.IsCollaborativing = false;

                    GApplication.Application.MapControl.ActiveView.Refresh(); 

                }

                int count = 0;
                foreach (var kv in editedFeatures)
                {
                    if (!submitDlg.NeedSubmitFC.Contains(kv.Key))
                        continue;

                    count += kv.Value.Count;
                }
                if (count > 0)
                {
                    MessageBox.Show(string.Format("提交成功,本次提交内容涉及{0}个要素类，共{1}条要素！", submitDlg.NeedSubmitFC.Count, count));
                }
                else
                {
                    MessageBox.Show("没有被编辑过的要素！");
                }


                System.Diagnostics.Trace.WriteLine(string.Format("/******数据库提交结束【{0}】******/", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"))); 
            }

            
        }


        
    }

}

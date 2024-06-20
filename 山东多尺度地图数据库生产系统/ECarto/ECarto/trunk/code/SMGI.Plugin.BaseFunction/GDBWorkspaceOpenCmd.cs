using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.BaseFunction
{
    [SMGIAutomaticCommand]
    public class GDBWorkspaceOpenCmd : SMGI.Common.SMGICommand
    {
        public GDBWorkspaceOpenCmd()
        {
            m_caption = "打开GDB";
            m_toolTip = "打开一个已有的GDB工程";
            m_category = "工程";
        }
        string ConfigKey()
        {
            return this.GetType().FullName + ".WorkspacePath";
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
            if (m_Application.Workspace != null)
            {
                MessageBox.Show("已经打开工作区，请先关闭工作区!");
                return;
            }
           
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择工程文件夹";
            fbd.ShowNewFolderButton = false;
            object path = m_Application.AppConfig[ConfigKey()];
            if (path != null)
            {
                if(Directory.Exists((string)path))
                {
                    fbd.SelectedPath = (string) path;

                }
            }

            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            
            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在打开工作区");
                if (!GApplication.GDBFactory.IsWorkspace(fbd.SelectedPath))
                {
                    MessageBox.Show("不是有效地GDB文件");
                    return;
                }
                m_Application.AppConfig[ConfigKey()] = fbd.SelectedPath;
                IWorkspace ws = GApplication.GDBFactory.OpenFromFile(fbd.SelectedPath, 0);
                if (GWorkspace.IsWorkspace(ws))
                {
                    m_Application.OpenESRIWorkspace(ws);
                }
                else
                {
                    m_Application.InitESRIWorkspace(ws);
                    var infos = m_Application.Workspace.LayerManager.GetLayer(IsBOUA);
                    foreach (var l in infos)
                    {
                        m_Application.MapControl.Extent = l.AreaOfInterest;
                        m_Application.Workspace.Save();
                        break;
                    }
                }
                
                //加载粘滞容差 zhx@2022.4.12
                int tol = AppConfig.StickyMoveTolerance;
                if (tol > 0)
                {
                    IEngineEditProperties2 _editorProp;
                    _editorProp = GApplication.Application.EngineEditor as IEngineEditProperties2;
                    _editorProp.StickyMoveTolerance = tol;
                }
            }
        }

        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                if (m_Application.Workspace != null)
                {
                    messageRaisedAction("已经打开工作区，请先关闭工作区!");
                    return false;
                }

                string fileName = args.Value.Trim();


                messageRaisedAction("正在打开工作区");
                if (!GApplication.GDBFactory.IsWorkspace(fileName))
                {
                    messageRaisedAction("不是有效地GDB文件");
                    return false;
                }
                IWorkspace ws = GApplication.GDBFactory.OpenFromFile(fileName, 0);
                if (GWorkspace.IsWorkspace(ws))
                {
                    m_Application.OpenESRIWorkspace(ws);
                }
                else
                {
                    m_Application.InitESRIWorkspace(ws);
                    var layers = m_Application.Workspace.LayerManager.GetLayer(IsBOUA);
                    foreach (var l in layers)
                    {
                        m_Application.MapControl.Extent = l.AreaOfInterest;
                        m_Application.Workspace.Save();
                        break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
        }

        bool IsBOUA(ILayer info)
        {
            return (info is IFeatureLayer)
                && ((info as IFeatureLayer).FeatureClass as IDataset).Name.ToUpper() == "BOUA";
        }
    }

    //用于读取EnvironmentConfig.xml的配置信息 zhx@2022.4.12
    public static class AppConfig
    {   //粘滞容差
        public static int StickyMoveTolerance
        {
            get
            {
                string cfgFileName = GApplication.Application.Template.Root + "\\EnvironmentConfig.xml";
                if (System.IO.File.Exists(cfgFileName))
                {
                    try
                    {
                        XDocument xmlDoc = XDocument.Load(cfgFileName);
                        var EditOptionItem = xmlDoc.Element("Option").Element("EditOption");
                        int val = int.Parse(EditOptionItem.Element("StickeyMoveTolerance").Value);
                        if (val > 0)
                        {
                            return val;
                        }

                    }
                    catch (Exception ex)
                    {
                    }
                }
                return 0;
            }
        }
 
    }
}

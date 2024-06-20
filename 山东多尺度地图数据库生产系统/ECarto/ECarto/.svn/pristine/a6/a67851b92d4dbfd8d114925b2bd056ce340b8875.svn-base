using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.BaseFunction
{
    [SMGIAutomaticCommand]
    public class OpenMDBWorkspace : SMGI.Common.SMGICommand
    {
        public OpenMDBWorkspace()
        {
            m_caption = "打开MDB";
            m_toolTip = "打开一个已有的MDB工程";
            m_category = "工程";
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
            OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();           
            dlg.Multiselect = false;
            dlg.Filter = "文件地理数据库(*.mdb)|*.mdb";
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            using (var wo = m_Application.SetBusy())
            {
                wo.SetText("正在打开工作区");
                if (!GApplication.MDBFactory.IsWorkspace(dlg.FileName))
                {
                    MessageBox.Show("不是有效地MDB文件");
                }
                IWorkspace ws = GApplication.MDBFactory.OpenFromFile(dlg.FileName,0);
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
                        m_Application.MapControl.Extent =l.AreaOfInterest;
                        m_Application.Workspace.Save();
                        break;
                    }
                }
            }
        }

        bool IsBOUA(ILayer info)
        {
            return (info is IFeatureLayer)
                && ((info as IFeatureLayer).FeatureClass as IDataset).Name.ToUpper() == "BOUA";
        }

        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            if (m_Application.Workspace != null)
            {
                messageRaisedAction("已经打开工作区，请先关闭工作区!");
                return false;
            }

            string fileName = args.Value.Trim();
            

            messageRaisedAction("正在打开工作区");
            if (!GApplication.MDBFactory.IsWorkspace(fileName))
            {
                messageRaisedAction("不是有效地MDB文件");
                return false;
            }
            IWorkspace ws = GApplication.MDBFactory.OpenFromFile(fileName, 0);
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
    }
}

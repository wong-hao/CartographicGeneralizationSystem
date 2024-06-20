using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using DevExpress.XtraBars.Docking;

namespace SMGI.Plugin.CollaborativeWork
{
    public class ConflictResultTable
    {
        private ConflictResultTable()
        {
            DevExpress.XtraBars.Docking.DockManager dockManger = (GApplication.Application.MainForm as ISMGIMainForm).DockManager as DevExpress.XtraBars.Docking.DockManager;

            _resutPanel = new DevExpress.XtraBars.Docking.DockPanel();

            _resutPanel = dockManger.AddPanel(DockingStyle.Bottom);
            _resutPanel.Text = "冲突检测结果表";


            _frmConflictResult = new FrmConflictResult(GApplication.Application);
            _frmConflictResult.Show(_resutPanel);
            _frmConflictResult.FormBorderStyle = FormBorderStyle.None;
            _frmConflictResult.TopLevel = false;
            _frmConflictResult.Dock = DockStyle.Fill;

            _resutPanel.Controls.Add(_frmConflictResult);
            _resutPanel.Options.AllowDockBottom = false;
            _resutPanel.Options.AllowDockFill = false;
            _resutPanel.Options.AllowDockLeft = false;
            _resutPanel.Options.AllowDockTop = false;
            _resutPanel.Options.AllowFloating = false;
        }

        #region 字段、属性
        private static ConflictResultTable m_instance = null;
        public static ConflictResultTable Instance
        {
            get
            {
                if (null == ConflictResultTable.m_instance)
                {
                    ConflictResultTable.m_instance = new ConflictResultTable();
                }

                return ConflictResultTable.m_instance;
            }
        }

        private DockPanel _resutPanel;

        private FrmConflictResult _frmConflictResult;
        #endregion

        #region 方法
        public void upateTable()
        {
            _frmConflictResult.upateTable();
        }

        public void show()
        {
            _resutPanel.Show();
        }
        #endregion
    }
}

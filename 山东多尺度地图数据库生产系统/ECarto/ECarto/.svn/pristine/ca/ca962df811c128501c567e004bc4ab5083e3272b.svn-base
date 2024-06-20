using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
namespace SMGI.Plugin.BaseFunction
{
    public class BookmarkCmd : SMGI.Common.SMGICommand
    {
        private BookMarkDialog bmdlg=null;
        public BookmarkCmd()
        {
            m_caption = "书签";
            m_toolTip = "弹出书签对话框";
            m_category = "环境";
            
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null;
            }
        }        

        public override void OnClick()
        {
            if (null == bmdlg || bmdlg.IsDisposed)
            {
                bmdlg = new BookMarkDialog(m_Application);
                //BookMarkDialog bmdlg = new BookMarkDialog(m_Application);            
                bmdlg.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                bmdlg.TopMost = true;
                //bmdlg.ShowDialog();
                bmdlg.Show();
            }
            else
            {
                bmdlg.Activate();
            }
        }
       
    }
}

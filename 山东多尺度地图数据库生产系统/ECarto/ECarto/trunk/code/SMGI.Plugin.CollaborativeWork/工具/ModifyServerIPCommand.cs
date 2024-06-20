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
    /// <summary>
    /// 修改本地数据库服务器ip地址
    /// </summary>
    public class ModifyServerIPCommand : SMGICommand
    {

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace == null;
            }
        }

        public override void OnClick()
        {
            ModifyServerIPForm frm = new ModifyServerIPForm();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (DialogResult.OK == frm.ShowDialog())
            {
                MessageBox.Show("修改成功！");
            }
        }


        
    }
}

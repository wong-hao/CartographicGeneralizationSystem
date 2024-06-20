using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;

namespace SMGI.Plugin.BaseFunction
{
    public class ParameterSetCmd : SMGI.Common.SMGICommand
    {
        public ParameterSetCmd()
        {
            m_caption = "参数表";
            m_toolTip = "弹出参数表对话框";
            m_category = "环境";
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null; ;
            }
        }
        public override void OnClick()
        {

            m_Application.MainForm.ShowChild(m_Application.GenParaDlg.Handle);

        }

    }
}

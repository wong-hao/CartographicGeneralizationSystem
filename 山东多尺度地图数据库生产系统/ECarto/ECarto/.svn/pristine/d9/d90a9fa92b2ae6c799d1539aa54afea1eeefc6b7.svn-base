using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace SMGI.Plugin.BaseFunction
{
    public class CommandManagerCmd : SMGI.Common.SMGICommand
    {
        public CommandManagerCmd()
        {
            m_caption = "命令管理";
            m_toolTip = "命令管理对话框";
            m_category = "环境";
        }

        public override bool Enabled
        {
            get
            {
                return true;
            }
        }
        public override void OnClick()
        {
            string rootPath = GApplication.Application.Template.Root;
            string xmlPath = String.Join("\\", rootPath, "Commands.xml");
            string exe = String.Join("\\", rootPath, "..", "..", "bin", "ECartoCommandManager.exe");
            string exePath = Path.GetFullPath(exe);

            Process pro = new Process();
            pro.StartInfo.FileName = exePath;
            pro.StartInfo.Arguments = xmlPath;
            pro.Start();
        }
    }
}

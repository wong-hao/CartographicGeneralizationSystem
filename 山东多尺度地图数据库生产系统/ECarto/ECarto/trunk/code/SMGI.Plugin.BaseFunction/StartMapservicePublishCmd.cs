using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Diagnostics;

namespace SMGI.Plugin.BaseFunction
{
    public class StartMapservicePublishCmd : SMGICommand
    {
        public StartMapservicePublishCmd()
        {
            m_caption = "启动地图服务发布系统";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null;
            }
        }

        public override void OnClick()
        {
            string className = "地图服务发布";

            var caption = m_Application.Template.Content.Element(className).Value;

            string exeFileName = GApplication.ExePath + "\\" + "SMGI.DxForm.exe";
            string arg = string.Format(@"{0} {1}", className, caption);

            Process p = new Process();
            ProcessStartInfo si = new ProcessStartInfo(exeFileName, arg);
            si.UseShellExecute = false;
            si.CreateNoWindow = false;

            p.StartInfo = si;
            p.Start();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Diagnostics;

namespace SMGI.Plugin.BaseFunction
{
    public class StartEmergencyMapCmd : SMGICommand
    {
        public StartEmergencyMapCmd()
        {
            m_caption = "启动快速制图系统";
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
            string className = "快速地图制图";

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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.BaseFunction
{
    public class TrueCMYKColor : SMGICommand
    {
        ICommand cmd;
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
            var cp = app.MainForm.CommandPool as ICommandPoolEdit;
            cmd = new TrueCMYKLib.CMYKSettingCommand() as ICommand;

            cp.AddCommand(cmd, null);

            cmd.OnClick();

        }

        public override bool Checked
        {
            get
            {
                return cmd != null && cmd.Checked;
            }
        }
        public override bool Enabled
        {
            get
            {
                return cmd != null && cmd.Enabled;
            }
        }
        public override void OnClick()
        {
            if (cmd != null)
            {
                cmd.OnClick();
                if (m_Application.ActiveView != null)
                {
                    m_Application.ActiveView.Refresh();
                }
            }
        }
    }
}

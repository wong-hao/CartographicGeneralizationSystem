using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using System.Runtime.InteropServices;

namespace SMGI.Plugin.CollaborativeWork
{
    public  class TestCommand:SMGICommand
    {
        public TestCommand()
        {
        }

        public override void OnClick()
        {
            MessageBox.Show("测试！");
        }

        public override bool Enabled
        {
            get
            {
                return true;
            }
        }
    }
}

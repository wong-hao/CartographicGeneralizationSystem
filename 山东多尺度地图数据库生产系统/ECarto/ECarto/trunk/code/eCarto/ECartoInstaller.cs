using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;


namespace eCarto
{
    [RunInstaller(true)]
    public partial class ECartoInstaller : System.Configuration.Install.Installer
    {
        public ECartoInstaller()
        {
            InitializeComponent();
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            Assembly asm = Assembly.GetExecutingAssembly();
            string exePath = System.IO.Path.GetDirectoryName(asm.Location);
            {

                string cartoPath = exePath + @"\SMGI.RepresentationExtend.dll";
                string commanPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                if (Environment.Is64BitOperatingSystem)
                {
                    commanPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                }
                commanPath += @"\Common Files\ArcGIS\bin\ESRIRegAsm.exe";
                ProcessStartInfo info = new ProcessStartInfo(commanPath);
                info.Arguments = string.Format("/s /p:desktop \"{0}\"", cartoPath);
                //info.Arguments = string.Format("\"{0}\"", cartoPath);
                //info.UseShellExecute = true;
                info.Verb = "runas";

                try
                {
                    System.Diagnostics.Process.Start(info);
                }
                catch
                {
                    return;
                }
            }
            //{
                //string cartoPath = exePath + @"\..\plugins\Generalizer.dll";

                //ProcessStartInfo info = new ProcessStartInfo("regsvr32");
                //info.Arguments = string.Format("\"{0}\" /s ", cartoPath);

                ////info.UseShellExecute = true;
                //info.Verb = "runas";

                //try
                //{
                //    System.Diagnostics.Process.Start(info);
                //}
                //catch
                //{
                //    return;
                //}
            //}

        }

    }
}

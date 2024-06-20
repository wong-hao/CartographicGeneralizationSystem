using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace eCarto
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string path = Application.StartupPath;

            path += @"\SMGI.DxForm.exe";
            ProcessStartInfo info = new ProcessStartInfo(path, "基础地理信息数据库更新");
            info.UseShellExecute = true;
            info.Verb = "runas";
            try
            {
                System.Diagnostics.Process.Start(info);
            }
            catch
            {
                return;
            }

            Application.Exit();
        }
    }
}

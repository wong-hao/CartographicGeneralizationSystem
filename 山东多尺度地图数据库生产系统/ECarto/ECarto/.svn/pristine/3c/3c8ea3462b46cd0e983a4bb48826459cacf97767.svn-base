using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ECartoCommandManager
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>     
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if(args.Length==0)
                Application.Run(new CommandManager()); 
            else
                Application.Run(new CommandManager(args[0])); 
        }
    }
}

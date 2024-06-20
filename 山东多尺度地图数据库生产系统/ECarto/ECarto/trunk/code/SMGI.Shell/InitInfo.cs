using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMGI.Shell
{
    class InitInfo:SMGI.Common.IInitInfo
    {
        public void Hide()
        {
            
        }

        public void Info(string info)
        {
            System.Console.WriteLine(info);
        }
    }
}

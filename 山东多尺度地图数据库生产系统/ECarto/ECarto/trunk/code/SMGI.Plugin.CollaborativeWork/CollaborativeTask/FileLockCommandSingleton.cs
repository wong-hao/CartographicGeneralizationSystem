using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Net;
using System.Diagnostics;
using System.Web.UI;

namespace SMGI.Plugin.CollaborativeWork
{
    class FileLockCommandSingleton
    {
        public static FileLockCommandSingleton mySingleton = null;

        private static readonly object locker = new object();


        private FileLockCommandSingleton()
        {

        }

        public FileStream fileStream { get; set; }

        public Process proc { get; set; }

        public static FileLockCommandSingleton GetInstance()
        {
            if (mySingleton == null)
            {
                lock (locker)
                {
                    if (mySingleton == null)
                    {
                   
                        mySingleton = new FileLockCommandSingleton();
                    }
                }
            }
            return mySingleton;
        }

        public  bool islockFile(string filepath)
        {
            return false;
        }

        public bool connectState(string path, string username, string password)
        {
            bool Flag = false;
            proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();

                string dosLine = @"net use " + path + " /User:" + username + " "+password+ " /PERSISTENT:YES";
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                string errormsg = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if(string.IsNullOrEmpty(errormsg)){
                    Flag = true;
                }else{
                    throw new Exception(errormsg);
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }finally{
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }

        public void lockFile(string filepath){

            bool status = false;
            status = connectState(@"\\172.16.1.50\smgi", "zch", "123456");
            if (status)
            {
                
                fileStream = new FileStream(@"\\172.16.1.50\smgi\zch.txt", FileMode.Open, FileAccess.Write, FileShare.Write);
                using (StreamWriter sr = new StreamWriter(fileStream))
                {
                    sr.Write("12211212,1112121111");
                    sr.Flush();
                }
            }
            else
            {

            }

        }

        public  void unlockFile(string filepath)
        {

        }
    }
}

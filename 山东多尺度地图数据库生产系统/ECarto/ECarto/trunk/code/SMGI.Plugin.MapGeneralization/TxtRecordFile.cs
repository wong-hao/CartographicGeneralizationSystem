﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace SMGI.Plugin.MapGeneralization
{
    class TxtRecordFile
    {
        string excelName = null;
        public TxtRecordFile(string dataName, string folderPath)
        {
            CreateErroTxtRecordFile(dataName, folderPath);
            this.excelName = folderPath + dataName + "-检查结果.txt";
        }

        public void CreateErroTxtRecordFile(string dataName, string folderPath)
        {
            //关闭所有TXT文件进程
            if (File.Exists(folderPath + dataName + "-检查结果.txt"))
            {
                try
                {
                    foreach (System.Diagnostics.Process thisproc in System.Diagnostics.Process.GetProcessesByName("TXT"))
                    {
                        if (!thisproc.CloseMainWindow())
                        {
                            thisproc.Kill();
                        }
                    }

                }
                catch (Exception)
                {

                }
            }

            if (System.IO.File.Exists(folderPath + dataName + "-检查结果.txt"))
            {

                System.IO.File.Delete(folderPath + dataName + "-检查结果.txt");
            }
            //创建检查结果的txt文件
            if (!System.IO.File.Exists(folderPath + dataName + "-检查结果.txt"))
            {

                System.IO.FileStream fsnew = System.IO.File.Create(folderPath + dataName + "-检查结果.txt");
                fsnew.Close();
            }

        }
        public void RecordError(string str)
        {
            using (System.IO.FileStream fs = System.IO.File.Open(excelName, System.IO.FileMode.Append, System.IO.FileAccess.Write))
            {

                using (System.IO.StreamWriter w = new System.IO.StreamWriter(fs))
                {

                    w.WriteLine(str);
                    w.Flush();
                    w.Close();
                }
            }
        }

        public void CreateHeader(string str)//"悬挂点检查结果", "国标", "名字","OID","版本","guid","用户",  "错误描述"
        {
            using (System.IO.FileStream fs = System.IO.File.Open(excelName, System.IO.FileMode.Append, System.IO.FileAccess.Write))
            {

                using (System.IO.StreamWriter w = new System.IO.StreamWriter(fs))
                {

                    w.WriteLine(str);
                    w.Flush();
                    w.Close();

                }
            }
        }
    }
}

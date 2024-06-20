using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using Microsoft.Win32;
using System.Text.RegularExpressions;
namespace SMGI.Common
{
    class SoftRegister
    {
        public static string CalculateSeialNum(string origNum)
        {
            char[] origChar = origNum.ToArray();
            int[] intNumber = new int[origChar.Length];
            int[] intCode = new int[origChar.Length];
            for (int i = 1; i < intCode.Length; i++)
            {
                intCode[i] = i % 9;
            }

            var orderChar = origChar.OrderBy(i => i);
            var orderString = orderChar.Aggregate("", (x, y) => x + y);
            orderString = orderString.Replace("000", "YB2");
            orderString = orderString.Replace("FFF", "RD4");
            orderString = orderString.Replace("AAA", "ZQ6");
            orderString = orderString.Replace("BBB", "TW8");
            orderString = orderString.Replace("CCC", "LL0");
            orderString = orderString.Replace("DDD", "LP3");
            orderString = orderString.Replace("ZZZ", "DJ7");
            orderString = orderString.Replace("XXX", "CL1");
            orderString = orderString.Replace("YYY", "LW5");
            orderString = orderString.Replace("EEE", "TT9");

            char[] charcode = orderString.ToArray();
            for (int j = 1; j < intNumber.Length; j++) //改变ASCII码值
            {
                intNumber[j] = Convert.ToInt32(charcode[j]) + intCode[j];
            }
            string serialNum = "";
            for (int k = 1; k < intNumber.Length; k++)
            {
                if ((intNumber[k] >= 48 && intNumber[k] <= 57) || (intNumber[k] >= 65 && intNumber[k] <= 90) || (intNumber[k] >= 97 && intNumber[k] <= 122)) //判断如果在0-9、A-Z、a-z之间
                {
                    serialNum += Convert.ToChar(intNumber[k]).ToString();
                }
                else if (intNumber[k] > 122) //判断如果大于z
                {
                    serialNum += Convert.ToChar(intNumber[k] - 10).ToString();
                }
                else
                {
                    serialNum += Convert.ToChar(intNumber[k] - 9).ToString();
                }
            }
            return serialNum;
        }
        ///<summary>
        /// 获取硬盘卷标号
        ///</summary>
        ///<returns></returns>
        public static string GetDiskVolumeSerialNumber()
        {
            ManagementClass mc = new ManagementClass("win32_NetworkAdapterConfiguration");
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }
        /// <summary>
        /// 取CPU编号
        /// </summary>
        /// <returns></returns>
        static string GetCpuID()
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();

                String strCpuID = null;
                foreach (ManagementObject mo in moc)
                {
                    strCpuID = mo.Properties["ProcessorId"].Value.ToString();
                    break;
                }
                return strCpuID;
            }
            catch
            {
                return "";
            }

        }//end method
        /// <summary>
        /// 取第一块硬盘编号，因为移动硬盘的复杂性，所以不建议加密时同时对CPUID和HDID操作，如果用户把移动硬盘设为启动盘，就会造成注册码和机器码经过运算后不符合的错误。
        /// </summary>
        /// <returns></returns>
        internal string GetHardDiskID()
        {
            string HDid = "";
            try
            {

                ManagementClass cimobject1 = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc1 = cimobject1.GetInstances();
                foreach (ManagementObject mo in moc1)
                {
                    HDid = (string)mo.Properties["Model"].Value;


                }
                return HDid;
            }
            catch
            {
                return "";
            }
        }

        internal string getMd5(string md5)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] value, hash;
            value = System.Text.Encoding.UTF8.GetBytes(md5);
            hash = md.ComputeHash(value);
            md.Clear();
            string temp = "";
            for (int i = 0, len = hash.Length; i < len; i++)
            {
                temp += hash[i].ToString("x").PadLeft(2, '0');
            }
            return temp;
        }

        /// <summary>
        /// 判断字符串中是否包含中文
        /// </summary>
        /// <param name="str">需要判断的字符串</param>
        /// <returns>判断结果</returns>
        public static bool HasChineseString(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        public static string GetCpu()
        {
            string strCpu = null;
            ManagementClass myCpu = new ManagementClass("win32_Processor");
            ManagementObjectCollection myCpuCollection = myCpu.GetInstances();
            foreach (ManagementObject myObject in myCpuCollection)
            {
                strCpu = myObject.Properties["Processorid"].Value.ToString();
            }
            return strCpu;
        }

        internal string GetMNum()
        {
            string strNum = GetCpu() + GetDiskVolumeSerialNumber();
            string strMNum = strNum.Substring(0, 24); //截取前24位作为机器码
            return strMNum;
        }
        public int[] intCode = new int[127]; //存储密钥
        public char[] charCode = new char[25]; //存储ASCII码
        public int[] intNumber = new int[25]; //存储ASCII码值
        public void SetIntCode()
        {
            for (int i = 1; i < intCode.Length; i++)
            {
                intCode[i] = i % 9;
            }
        }
        /// <summary>
        /// 生成注册码
        /// </summary>
        /// <returns></returns>
        internal string GetRNum()
        {
            RegistryKey retkey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true).CreateSubKey("wxf").CreateSubKey("wxf.INI");
            SetIntCode();
            string strMNum = GetMNum();
            for (int i = 1; i < charCode.Length; i++) //存储机器码
            {
                charCode[i] = Convert.ToChar(strMNum.Substring(i - 1, 1));
            }
            for (int j = 1; j < intNumber.Length; j++) //改变ASCII码值
            {
                intNumber[j] = Convert.ToInt32(charCode[j]) + intCode[Convert.ToInt32(charCode[j])];
            }
            string strAsciiName = ""; //注册码
            for (int k = 1; k < intNumber.Length; k++) //生成注册码
            {
                if ((intNumber[k] >= 48 && intNumber[k] <= 57) || (intNumber[k] >= 65 && intNumber[k] <= 90) || (intNumber[k] >= 97 && intNumber[k] <= 122)) //判断如果在0-9、A-Z、a-z之间
                {
                    strAsciiName += Convert.ToChar(intNumber[k]).ToString();
                }
                else if (intNumber[k] > 122) //判断如果大于z
                {
                    strAsciiName += Convert.ToChar(intNumber[k] - 10).ToString();
                }
                else
                {
                    strAsciiName += Convert.ToChar(intNumber[k] - 9).ToString();
                }
            }
            return strAsciiName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

namespace SMGI.Common
{
    internal partial class RegFrom : Form
    {
        public RegFrom()
        {
            InitializeComponent();

            string sysMachineName;

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, System.Environment.MachineName);
                sysMachineName = Convert.ToBase64String(ms.ToArray());
            }
            //if (SoftRegister.HasChineseString(System.Environment.MachineName))
            //{
            //    sysMachineName = "SMGIMachineName";
            //}
            //else
            //{
            //    sysMachineName = System.Environment.MachineName;
            //}

            //string origNum = sysMachineName;
            tbMac.Text = sysMachineName;
            //tbMac.Text = SoftRegister.CalculateSeialNum(origNum);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (RegistryHelper.IsRegistryExist(Registry.LocalMachine, "SOFTWARE", "SMGI"))
            {
                //创建
                Registry.LocalMachine.CreateSubKey("SOFTWARE\\SMGI");
            }
            RegistryHelper.SetRegistryData(Registry.LocalMachine, "SOFTWARE\\SMGI", "SMGI", tbReg.Text.Trim());
        }

        private void RegFrom_Load(object sender, EventArgs e)
        {

        }
    }
}

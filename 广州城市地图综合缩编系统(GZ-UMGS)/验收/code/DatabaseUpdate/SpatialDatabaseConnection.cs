using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using Microsoft.Win32;

namespace DatabaseUpdate
{
    public partial class SpatialDatabaseConnection : Form
    {
        public SpatialDatabaseConnection()
        {
            InitializeComponent();
            ReadRegistry();
        }

        private RegistryKey OpenReg()
        {
            RegistryKey r = Registry.LocalMachine;
            r = open(r, "SOFTWARE");
            r = open(r, "Domap");
            r = open(r, "GZ-UMGS");
            r = open(r, "DataBaseUpdate");
            r = open(r, "ConnectDatabase");
            return r;
        }
        private void ReadRegistry()
        {
            RegistryKey r = OpenReg();

            this.serverName = r.GetValue("ServerName", this.serverName).ToString();
            this.serviceName = r.GetValue("ServiceName", this.serviceName).ToString();
            this.user = r.GetValue("User", this.user).ToString();
            this.password = r.GetValue("Password", this.password).ToString();
            this.version = r.GetValue("Version", this.version).ToString();
            
        }
        RegistryKey open(RegistryKey p, string n)
        {
            RegistryKey r = p.OpenSubKey(n, true);
            if (r == null)
            {
                p.CreateSubKey(n);
                r = p.OpenSubKey(n, true);
            }
            return r;
        }

        public string serverName
        {
            get
            {
                return textBox1.Text;
            }
            private set
            {
                textBox1.Text = value;
            }
        }
        public string serviceName
        {
            get
            {
                return textBox2.Text;
            }
            private set
            {
                textBox2.Text = value;
            }
        }
        public string user
        {
            get
            {
                return textBox3.Text;
            }
            private set
            {
                textBox3.Text = value;
            }
        }
        public string password
        {
            get
            {
                return textBox4.Text;
            }
            private set
            {
                textBox4.Text = value;
            }
        }
        public string version
        {
            get
            {
                return textBox5.Text;
            }
            private set
            {
                textBox5.Text = value;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            RegistryKey r = OpenReg();

            r.SetValue("ServerName", this.serverName);
            r.SetValue("ServiceName", this.serviceName);
            r.SetValue("User", this.user);
            r.SetValue("Password", this.password);
            r.SetValue("Version", this.version);
            this.DialogResult = DialogResult.OK;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPropertySet connectSet = new PropertySetClass();
            connectSet.SetProperty("SERVER", textBox1.Text);
            connectSet.SetProperty("INSTANCE", textBox2.Text);
            connectSet.SetProperty("USER", textBox3.Text);
            connectSet.SetProperty("PASSWORD", textBox4.Text);
            connectSet.SetProperty("AUTHENTICATION_MODE", "DBMS");
            connectSet.SetProperty("VERSION", "sde.DEFAULT");
            Versions vs = new Versions(connectSet);
            if (vs.versionN == null)
                return;
            if (vs.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text = vs.versionN;
            }
        }


    }
}

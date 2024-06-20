using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;

namespace DatabaseUpdate
{
    public partial class Versions : Form
    {
        public Versions(IPropertySet set)
        {
            InitializeComponent();
            try
            {
                IWorkspaceFactory wsf = new SdeWorkspaceFactoryClass();
                IWorkspace ws = wsf.Open(set, 0);
                IVersionedWorkspace vspace = (IVersionedWorkspace)ws;
                IEnumVersionInfo enumV = vspace.Versions;
                enumV.Reset();
                IVersionInfo verIn = null;
                while ((verIn = enumV.Next()) != null)
                {
                    string verName = verIn.VersionName;
                    comboBox1.Items.Add(verName);
                }
                if (comboBox1.Items.Count > 0)
                    comboBox1.SelectedIndex = 0;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("无法连接到数据库，请检查网络或者数据库服务器。", "错误", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1);
                return;
            }
        }

        public string versionN
        {
            get
            {
                return (string)comboBox1.SelectedItem;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
